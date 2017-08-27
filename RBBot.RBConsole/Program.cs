using RBBot.Core.Database;
using RBBot.Core.Engine;
using RBBot.Core.Exchanges.CryptoCompare;
using RBBot.Core.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBBot.RBConsole
{

    public class Program
    {



        

        

        public static void Main(string[] args)
        {
            RBBot.Core.Database.RBBotContext.ConnectionString = ConfigurationManager.ConnectionStrings["RBBot"].ConnectionString;

            try
            { 
                RBBot.Core.Engine.DataProcessingEngine.InitializeEngine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.ReadLine();

        }
    }

}

