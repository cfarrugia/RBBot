using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RBBot.Core.Exchanges.GDAX
{
    public class GDAXTradeMatchJson
    {
        public string type;
        public int trade_id;
        public string maker_order_id;
        public string taker_order_id;
        public string side;
        public decimal size;
        public decimal price;
        public long sequence;
        public string time;
        public string product_id;

        
    }
}
