using RBBot.Core.Engine.Trading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RBBot.Core.Engine.Trading
{
    public interface IMarketPriceObserver
    {
        Task OnMarketPriceChangeAsync(PriceChangeEvent change);

    }
}
