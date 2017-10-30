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

        Task<ExchangeOrderResponse> PlaceOrder(ExchangeOrderType orderType, decimal orderAmount, ExchangeTradePair tradePair);

        TransactionFee EstimateTransactionFee(ExchangeOrderType orderType, decimal orderAmount, ExchangeTradePair tradePair);

    }
}
