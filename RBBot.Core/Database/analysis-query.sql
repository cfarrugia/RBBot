use rbbot

set transaction isolation level read uncommitted
select sum(approxusdbalance) as approximateValuationInUSD from [dbo].[TradeAccountUSDView]


select * from dbo.TradeAccountUSDView
order by LastUpdate desc



declare @starttime datetime = '06-nov-2017 23:00'--  '05-nov-2017 13:00'

--select * from dbo.TradeAccountUSDView 
--where iscrypto = 1 
--order by approxusdbalance 


;with tot as
(
select tot1.*, topp.Description as OppIdentifier, topp.StartTime as OppStartTime, topp.EndTime AS OppEndTime, ex.Name as Exchange, cFrom.Code As FromCurrency, cTo.Code AS ToCurrency, (cFrom.Code + ' -> ' + cTo.Code) AS CurrencyFlow,  
cFrom.ApproximateUSDValue * tot1.FromAmount AS FromAmountUSD, cTo.ApproximateUSDValue * tot1.ToAmount AS ToAmountUSD,
CAST(tot1.FromAccountBalanceBeforeTx AS VARCHAR(255)) + ' ' +  cFrom.Code as PairFromAmountBefore,
CAST(tot1.FromAccountBalanceBeforeTx AS VARCHAR(255)) + ' -> ' + CAST(tot1.EstimatedFromAccountBalanceAfterTx AS VARCHAR(255)) + ' ' +  cFrom.Code + ' (' + CAST((cFrom.ApproximateUSDValue * tot1.FromAmount) AS VARCHAR(255)) + ' USD)' as PairFromTransfer,
CAST(tot1.ToAccountBalanceBeforeTx AS VARCHAR(255)) + ' -> ' + CAST(tot1.EstimatedToAccountBalanceAfterTx AS VARCHAR(255)) + ' ' +  cTo.Code + ' (' + CAST((cTo.ApproximateUSDValue * tot1.ToAmount) AS VARCHAR(255)) + ' USD)' as PairToTransfer
from dbo.TradeOpportunityTransaction tot1
inner join dbo.TradeAccount taFrom
on taFrom.Id = tot1.FromAccountId
inner join dbo.Currency cFrom
on cFrom.Id = taFrom.CurrencyId
inner join dbo.TradeAccount taTo
on taTo.Id = tot1.ToAccountId
inner join dbo.Currency cTo
on cTo.Id = taTo.CurrencyId
inner join dbo.exchange ex
on ex.Id = tot1.ExecutedOnExchangeId
inner join dbo.TradeOpportunity topp
on topp.Id = tot1.TradeOpportunityId
where CreationDate >= @starttime
)
select totLow.OppIdentifier, 
	(totHigh.FromAmountUSD - totLow.FromAmountUSD) + (totHigh.ToAmountUSD - totLow.ToAmountUSD) ArbInUSD, 
	(totLow.Exchange + ' -> ' + totHigh.Exchange) ExchangeFlow, 
	totLow.CurrencyFlow, totLow.OppStartTime, totLow.OppEndTime,  totLow.CreationDate,
	totLow.PairFromTransfer lowPairFromTransfer, totLow.PairToTransfer lowPairToTransfer,
	totHigh.PairFromTransfer highPairFromTransfer, totHigh.PairToTransfer highPairToTransfer
	 from tot totLow
inner join tot totHigh
on totLow.TradeOpportunityId = totHigh.TradeOpportunityId and totLow.FromAmount < 0 and totHigh.FromAmount > 0
--where 
	-- Low From Amount = -1 * High From Amount always!
order by totLow.CreationDate
desc