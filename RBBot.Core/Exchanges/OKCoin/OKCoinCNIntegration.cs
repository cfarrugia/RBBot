using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RBBot.Core.Engine;
using RBBot.Core.Models;
using RBBot.Core.Engine.MarketObservers;

namespace RBBot.Core.Exchanges.OKCoin
{
    public class OKCoinCNIntegration : OKCoinIntegration
    {
        public OKCoinCNIntegration(IMarketPriceObserver[] priceObservers, Exchange[] exchange) : base(priceObservers, exchange)
        {
        }
        public override string Name { get { return "OKCoin.cn"; } }


        public override string wssUri { get { return "wss://real.okcoin.cn:10440/websocket/okcoinapi"; } }
    }
}
