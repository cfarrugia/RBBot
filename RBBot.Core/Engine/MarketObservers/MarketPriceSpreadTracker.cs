using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBBot.Core.Engine.MarketObservers
{
    /// <summary>
    /// This price tracker maintains
    /// </summary>
    public class MarketPriceSpreadTracker : IMarketPriceObserver
    {
        public Task OnMarketPriceChangeAsync(PriceChangeEvent change)
        {
            throw new NotImplementedException();
        }
    }
}
