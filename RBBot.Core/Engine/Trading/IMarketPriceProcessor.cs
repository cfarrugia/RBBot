using RBBot.Core.Engine.Trading;
using RBBot.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RBBot.Core.Engine.Trading
{
    public interface IMarketPriceProcessor
    {
        Task<IEnumerable<Opportunity>> OnMarketPriceChangeAsync(ExchangeTradePair changePair);

    }
}
