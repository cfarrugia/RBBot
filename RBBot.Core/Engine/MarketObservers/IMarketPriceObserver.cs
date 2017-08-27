using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RBBot.Core.Engine.MarketObservers
{
    public interface IMarketPriceObserver
    {
        Task OnMarketPriceChangeAsync(PriceChangeEvent change);

    }
}
