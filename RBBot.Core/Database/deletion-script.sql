use RBBot

delete from [dbo].[TradeOpportunityValue]
delete from [dbo].[TradeOpportunityTransaction]
delete from [dbo].[TradeOpportunityRequirement]
delete from [dbo].[TradeOpportunity]

exec [dbo].[AssignMoneyToAllAccounts] 1000