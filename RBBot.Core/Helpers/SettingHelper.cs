using RBBot.Core.Database;
using RBBot.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;

namespace RBBot.Core.Helpers
{
    public static class SettingHelper
    {
        private static Dictionary<Exchange, Dictionary<string, string>> exchangeSettings;
        private static Dictionary<string, string> systemSettings;

        public static void InitializeSettings(Setting[] settings)
        {
            exchangeSettings = settings.Where(x => x.Exchange != null).GroupBy(x => x.Exchange).ToDictionary(x => x.Key, y => y.ToDictionary(z => z.Name, w => GetValue(w)));
            systemSettings = settings.Where(x => x.Exchange == null).ToDictionary(x => x.Name, y => GetValue(y));
        }
        /// <summary>
        /// Encrypts setting, but doesn't persist.
        /// </summary>
        /// <param name="setting"></param>
        public static void EncryptSetting(this Setting setting)
        {
            // Ignore alredy encrypted setting
            if (setting.IsEncrypted == true) return;

            //ctx.Settings.Attach(setting);
            setting.IsEncrypted = true;
            setting.Value = Encrypt(setting.Value);
        }

        public static string GetSetting(this Exchange exchange, string name)
        {
            return GetExchangeSetting(exchange, name);
        }

        public static string GetExchangeSetting(Exchange exchange, string name)
        {
            return exchangeSettings[exchange][name];
        }

        public static string GetSystemSetting(string name)
        {
            return systemSettings[name];
        }

        /// <summary>
        /// This will get the unencrpyted value of the setting object.
        /// </summary>
        public static string GetValue(this Setting setting)
        {
            if (!setting.IsEncrypted) return setting.Value;

            return (Decrypt(setting.Value));
        }




        private static string Encrypt(string plainText)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            byte[] encryptedBytes = MachineKey.Protect(plainTextBytes);

            string encryptedText = Convert.ToBase64String(encryptedBytes);

            return encryptedText;
        }

        private static string Decrypt(string encryptedText)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);

            byte[] plainTextBytes = MachineKey.Unprotect(encryptedBytes);

            string plainText = Encoding.UTF8.GetString(plainTextBytes);

            return plainText;
        }
    }
}
