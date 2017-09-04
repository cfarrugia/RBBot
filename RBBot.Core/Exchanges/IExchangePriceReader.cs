using RBBot.Core.Engine.Trading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RBBot.Core.Exchanges
{

    public interface IExchangePriceReader : IExchange
    {
        
        /// <summary>
        /// List of observers
        /// </summary>
        IEnumerable<IMarketPriceObserver> PriceObservers { get; set;  }

        /// <summary>
        /// Initialization method for the integration.
        /// </summary>
        Task InitializeExchangePriceProcessingAsync();

        /// <summary>
        /// Called to shutdown the exchange integration
        /// </summary>
        Task ShutdownExchangePriceProcessingDownAsync();

    }
}
