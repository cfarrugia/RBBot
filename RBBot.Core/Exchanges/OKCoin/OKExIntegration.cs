using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RBBot.Core.Engine;
using RBBot.Core.Models;
using RBBot.Core.Engine.MarketObservers;

namespace RBBot.Core.Exchanges.OKCoin
{
    public class OKExIntegration : OKCoinIntegration
    {
        public OKExIntegration(IMarketPriceObserver[] priceObservers, Exchange[] exchange) : base(priceObservers, exchange)
        {
        }

        public override string Name { get { return "OKEx"; } }


        public override string wssUri { get { return "wss://real.okex.com:10440/websocket/okcoinapi"; } }
    }
}
