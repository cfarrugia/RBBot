using RBBot.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBBot.Core.Engine.Trading
{
    interface ITradeAction
    {
        decimal Cost { get; }

        Currency BaseCurrency { get; }

        void ExecuteAction(bool simulate);
    }
}
