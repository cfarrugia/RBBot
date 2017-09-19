using RBBot.Core.Engine.Trading;
using RBBot.Core.Engine.Trading.Actions;
using RBBot.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBBot.Core.Exchanges
{
    public interface IExchangeTrader : IExchange
    {
        Task<ExchangeBalance[]> GetBalancesAsync();

        Exchange Exchange { get; }

        //Task<ExchangeBalance> GetBalanceAsync(Currency currency);

        Task<string> GetDepositAddressAsync(Currency currency);

        Task WithdrawAsync(Currency currency, decimal amount, string fromAccountAddress, string toAccountAddress);

        Task<ExchangeOrderResponse> PlaceOrder(ExchangeOrderType orderType, decimal orderAmount, ExchangeTradePair tradePair);

        TransactionFee EstimateTransactionFee(ExchangeOrderType orderType, decimal orderAmount, ExchangeTradePair tradePair);

    }
}
