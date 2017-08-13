using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RBBot.Core.Engine
{
    public interface IPriceObserver
    {
        Task OnMarketPriceChangeAsync(PriceChangeEvent change);
    }
}
