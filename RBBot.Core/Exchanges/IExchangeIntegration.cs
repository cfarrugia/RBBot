using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RBBot.Core.Exchanges
{

    public interface IExchangeIntegration
    {
        /// <summary>
        /// Exchange Name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// List of observers
        /// </summary>
        IEnumerable<Engine.IPriceObserver> PriceObservers { get; set;  }

        /// <summary>
        /// Initialization method for the integration.
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Called to shutdown the exchange integration
        /// </summary>
        Task ShutDownAsync();

    }
}
