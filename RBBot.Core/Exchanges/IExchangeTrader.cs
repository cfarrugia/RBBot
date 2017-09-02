using RBBot.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBBot.Core.Exchanges
{
    public interface IExchangeTrader
    {
        Task<ExchangeBalance[]> GetBalancesAsync();

        Task<ExchangeBalance> GetBalanceAsync(Currency currency);

        Task DepositAsync(Currency currency, decimal amount, string fromAccountAddress, string toAccountAddress);

        Task WithdrawAsync(Currency currency, decimal amount, string fromAccountAddress, string toAccountAddress);

        Task PlaceOrder();
    }
}
