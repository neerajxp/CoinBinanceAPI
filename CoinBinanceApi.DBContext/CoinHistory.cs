using System;
using System.Collections.Generic;
using System.Text;

namespace CoinBinanceApi.DBContext
{
    public class CoinHistory
    {
        public string CoinName;
        public string Price;
        public string Currency;
        public string LowestAsk;
        public string HighestBid;
        public string PercentChange;
        public string BaseVolume;
        public string QuoteVolume;
        public string High24Hr;
        public string Low24Hr;
        public string CreatedDate;
    }
}
