using RBBot.Core.Database;
using RBBot.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBBot.Core.Helpers
{
    public static class SystemSetting
    {
        /// <summary>
        /// This is the preferred crypto-currency, the one that we try to have more and the one we feel less exposed to.
        /// </summary>
        public static Currency PreferredCyptoCurrency { get; private set; }

        public static decimal MinimumTradeOpportunityPercent { get; private set; }
        public static int OpportunityTimeoutInSeconds { get; private set; }

        public static int ExchangeConnectionTimeoutInSeconds { get; private set; }

        public static void LoadSystemSettings(RBBotContext dbContext)
        {
            //
            string preferredCode = SettingHelper.GetSystemSetting("PreferedCryptoCurrency");
            PreferredCyptoCurrency = dbContext.Currencies.Where(x => x.Code == preferredCode).Single();
            MinimumTradeOpportunityPercent = Convert.ToDecimal(SettingHelper.GetSystemSetting("MinimumTradeOpportunityPercent"));
            OpportunityTimeoutInSeconds = Convert.ToInt32(SettingHelper.GetSystemSetting("OpportunityTimeoutInSeconds"));
            ExchangeConnectionTimeoutInSeconds = Convert.ToInt32(SettingHelper.GetSystemSetting("ExchangeConnectionTimeoutInSeconds"));
        }

    }
}
