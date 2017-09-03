﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RBBot.Core.Exchanges.Poloniex.Common
{
    public static class Helper
    {
        internal const string ApiUrlHttpsRelativePublic = "public?command=";
        internal const string ApiUrlHttpsRelativeTrading = "tradingApi";

        private const int DoubleRoundingPrecisionDigits = 8;

        internal static readonly string AssemblyVersionString = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);

        internal static readonly DateTime DateTimeUnixEpochStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        internal static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

        private static BigInteger CurrentHttpPostNonce { get; set; }

        internal static string GetResponseString(this HttpWebRequest request)
        {
            using (var response = request.GetResponse())
            {
                using (var stream = response.GetResponseStream())
                {
                    if (stream == null) throw new NullReferenceException("The HttpWebRequest's response stream cannot be empty.");

                    using (var reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        internal static T DeserializeObject<T>(this JsonSerializer serializer, string value)
        {
            using (var stringReader = new StringReader(value))
            {
                using (var jsonTextReader = new JsonTextReader(stringReader))
                {
                    return (T)serializer.Deserialize(jsonTextReader, typeof(T));
                }
            }
        }

        internal static string ToHttpPostString(this Dictionary<string, object> dictionary)
        {
            var output = string.Empty;
            foreach (var entry in dictionary)
            {
                var valueString = entry.Value as string;
                if (valueString == null)
                {
                    output += "&" + entry.Key + "=" + entry.Value;
                }
                else
                {
                    output += "&" + entry.Key + "=" + valueString.Replace(' ', '+');
                }
            }

            return output.Substring(1);
        }

        internal static string ToStringNormalized(this OrderType value)
        {
            switch (value)
            {
                case OrderType.Buy:
                    return "buy";

                case OrderType.Sell:
                    return "sell";
            }

            throw new ArgumentOutOfRangeException("value");
        }

        internal static OrderType ToOrderType(this string value)
        {
            switch (value)
            {
                case "buy":
                    return OrderType.Buy;

                case "sell":
                    return OrderType.Sell;
            }

            throw new ArgumentOutOfRangeException("value");
        }

        public static double Normalize(this double value)
        {
            return Math.Round(value, DoubleRoundingPrecisionDigits, MidpointRounding.AwayFromZero);
        }

        public static string ToStringNormalized(this double value)
        {
            return value.ToString("0." + new string('#', DoubleRoundingPrecisionDigits), InvariantCulture);
        }

        public static string ToStringHex(this byte[] value)
        {
            var output = string.Empty;
            for (var i = 0; i < value.Length; i++)
            {
                output += value[i].ToString("x2", InvariantCulture);
            }

            return (output);
        }

        //public static double[] GetBollingerBandsWithSimpleMovingAverage(this IList<IMarketChartData> value, int index, int period = 20)
        //{
        //    var closes = new List<double>(period);
        //    for (var i = index; i > Math.Max(index - period, -1); i--)
        //    {
        //        closes.Add(value[i].Close);
        //    }

        //    var simpleMovingAverage = closes.Average();
        //    var stDevMultiplied = Math.Sqrt(closes.Average(x => Math.Pow(x - simpleMovingAverage, 2))) * 2;

        //    return new[] {
        //        simpleMovingAverage,
        //        simpleMovingAverage + stDevMultiplied,
        //        simpleMovingAverage - stDevMultiplied
        //    };
        //}

        internal static DateTime UnixTimeStampToDateTime(ulong unixTimeStamp)
        {
            return DateTimeUnixEpochStart.AddSeconds(unixTimeStamp);
        }

        internal static ulong DateTimeToUnixTimeStamp(DateTime dateTime)
        {
            return (ulong)Math.Floor(dateTime.Subtract(DateTimeUnixEpochStart).TotalSeconds);
        }

        internal static DateTime ParseDateTime(string dateTime)
        {
            return DateTime.SpecifyKind(DateTime.ParseExact(dateTime, "yyyy-MM-dd HH:mm:ss", InvariantCulture), DateTimeKind.Utc).ToLocalTime();
        }

        internal static string GetCurrentHttpPostNonce()
        {
            var newHttpPostNonce = new BigInteger(Math.Round(DateTime.UtcNow.Subtract(DateTimeUnixEpochStart).TotalMilliseconds * 1000, MidpointRounding.AwayFromZero));
            if (newHttpPostNonce > CurrentHttpPostNonce)
            {
                CurrentHttpPostNonce = newHttpPostNonce;
            }
            else
            {
                CurrentHttpPostNonce += 1;
            }

            return CurrentHttpPostNonce.ToString(InvariantCulture);
        }
    }
}
