using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RBBot.Core.Helpers
{
    /// <summary>
    /// This is a generic websocket manager that will be used for exchanges allowing for web-socket integrations.
    /// </summary>
    public static class WebSocketManager
    {
        private static object consoleLock = new object();
        private const int sendChunkSize = 1024;
        private const int receiveChunkSize = 1024;
        private const int connectionTimeOutMillisecond = 5000;
        private const bool verbose = true;
        private static UTF8Encoding encoder = new UTF8Encoding();


        /// <summary>
        /// Gets a connected websocket with retry mechanism
        /// </summary>
        /// <param name="server"></param>
        /// <param name="timeOutMilliseconds"></param>
        /// <param name="keepAliveInterval"></param>
        /// <returns></returns>
        private static async Task<ClientWebSocket> GetConnectedWebSocket(
            Uri server,
            int timeOutMilliseconds,
            TimeSpan keepAliveInterval = default(TimeSpan))
        {
            const int MaxTries = 5;
            int betweenTryDelayMilliseconds = 1000;

            // 
            for (int i = 1; ; i++)
            {
                try
                {
                    var cws = new ClientWebSocket();
                    if (keepAliveInterval.TotalSeconds > 0)
                    {
                        cws.Options.KeepAliveInterval = keepAliveInterval;
                    }

                    using (var cts = new CancellationTokenSource(timeOutMilliseconds))
                    {
                        await cws.ConnectAsync(server, cts.Token);
                    }
                    return cws;
                }
                catch (WebSocketException exc)
                {
                    if (i == MaxTries)
                    {
                        throw exc;
                    }

                    await Task.Delay(betweenTryDelayMilliseconds);
                    betweenTryDelayMilliseconds *= 2;
                }
            }
        }


        /// <summary>
        /// Build a new websocket, connects and send the subscribtion message.
        /// </summary>
        /// <param name="address">Address to connect to</param>
        /// <param name="subscribeMessage">The initial subscription message to send</param>
        /// <param name="receiver">The method called whenever a message is received</param>
        /// <returns></returns>
        public static async Task<ClientWebSocket> Initialize(string address, string[] subsrcibeMessage, Func<string, Task> receiver)
        {
            ClientWebSocket websocket = null;
            try
            {

                websocket = await GetConnectedWebSocket(new Uri(address), connectionTimeOutMillisecond);

                // Wait until both sends and receives terminate
                await Task.WhenAll(Receive(websocket, receiver), Send(websocket, subsrcibeMessage));

                //
                return websocket;
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }


        private static async Task Send(ClientWebSocket webSocket, string[] messages)
        {
            foreach (var message in messages)
            {
                // encode and send message;
                byte[] buffer = encoder.GetBytes(message);
                await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            
        }

        private static async Task Receive(ClientWebSocket webSocket, Func<string, Task> processor)
        {
            byte[] buffer = new byte[receiveChunkSize];
            while (webSocket.State == WebSocketState.Open)
            {
                StringBuilder completeMessage = new StringBuilder();
                bool isEndOfMessage = false;

                do
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    isEndOfMessage = result.EndOfMessage;
                    completeMessage.Append(encoder.GetString(buffer, 0, result.Count));

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                        isEndOfMessage = true;
                    }

                } while (isEndOfMessage == false);
                
                // If a message was written, then process it.
                if (completeMessage.Length > 0)
                    await processor(completeMessage.ToString());
            }
        }
        
    }



}
