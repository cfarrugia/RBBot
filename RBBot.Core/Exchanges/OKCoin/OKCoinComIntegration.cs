using RBBot.Core.Models;
using RBBot.Core.Engine.Trading;

namespace RBBot.Core.Exchanges.OKCoin
{
    public class OKCoinComIntegration : OKCoinIntegration
    {
        public OKCoinComIntegration(IMarketPriceProcessor[] priceObservers, Exchange[] exchange) : base(priceObservers, exchange)
        {
        }

        public override string Name { get { return "OKCoin.com"; } }


        public override string wssUri { get { return "wss://real.okcoin.com:10440/websocket/okcoinapi"; } }
    }
}
