using RBBot.Core.Models;
using RBBot.Core.Engine.Trading;

namespace RBBot.Core.Exchanges.OKCoin
{
    public class OKCoinCNIntegration : OKCoinIntegration
    {
        public OKCoinCNIntegration(IMarketPriceProcessor[] priceObservers, Exchange[] exchange) : base(priceObservers, exchange)
        {
        }
        public override string Name { get { return "OKCoin.cn"; } }


        public override string wssUri { get { return "wss://real.okcoin.cn:10440/websocket/okcoinapi"; } }
    }
}
