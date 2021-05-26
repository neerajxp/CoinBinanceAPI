using System;
using System.Data.SqlClient;

namespace CoinBinanceApi.Common
{
    public static class Global
    {
        public static string constrGate { get; set; }
        public static string constrLocal { get; set; }
        public static int isLocalMode { get; set; }

    }
}
