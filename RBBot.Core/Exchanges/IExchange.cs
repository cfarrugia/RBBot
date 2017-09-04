using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBBot.Core.Exchanges
{
    public interface IExchange
    {
        /// <summary>
        /// Exchange Name
        /// </summary>
        string Name { get; }
    }
}
