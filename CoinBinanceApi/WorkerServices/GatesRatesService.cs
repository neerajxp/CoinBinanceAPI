using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks; 
using System.Threading; 
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Configuration;
using CoinBinanceApi.Common;
using CoinBinanceApi.DBContext;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Com.Gate.Rest.Stock;

namespace CoinBinanceApi.WorkerServices
{
    public class GatesRatesService : BackgroundService
    {
        private readonly ILogger<GatesRatesService> _logger;
        private readonly ILogger<SharedModule> _loggerShared;
        private readonly IConfiguration _config;
        private string conStr;
        //private static IStockRestApi stockGet = new IStockRestApi("https://data.gateapi.io");
        private static StockRestApi stockGet2 = new StockRestApi("https://data.gateapi.io");
         
        public GatesRatesService(ILogger<GatesRatesService> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Rates Retrieving start : " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:MM"));
            while (!stoppingToken.IsCancellationRequested)
            {                
                try
                {                   
                    Global.constrGate = _config["ConnectionStringGate"];
                    Global.constrLocal = _config["ConnectionStringLocal"];
                    conStr = Global.constrGate;
                    Global.isLocalMode = 0;
                    if (_config["isLocalMode"] == "1")
                    {
                        conStr = Global.constrLocal;
                        Global.isLocalMode = 1;
                    }
                    SharedModule sm = new SharedModule(_loggerShared, _config);
                    SqlConnection makeSQLConn = new SqlConnection(conStr);
                    SqlDataAdapter da = new SqlDataAdapter("select * from  GateRates where 1=2", conStr);
                    DataSet dsRates = new DataSet();
                    da.Fill(dsRates, "GateRates");
                    dsRates.Tables[0].Clear();

                    DataTable dtMaster = sm.ExecuteQuery("select * from BatchMaster", conStr);
                    DataRow[] drowMaster = dtMaster.Select("ParameterName='GateBatchRates'");
                    int currBatch = Convert.ToInt32(drowMaster[0]["ParameterValue"]);
                    drowMaster = dtMaster.Select("ParameterName='GateRateIntervalSeconds'");
                    int Interval = Convert.ToInt32(drowMaster[0]["ParameterValue"]);
                    
                    //Call Gate API
                    var ticker = stockGet2.tickers();

                    //List<CoinHistory> ch = new List<CoinHistory>();
                    dynamic dynJson =  JsonConvert.DeserializeObject(ticker);
                    JObject Object = (JObject)dynJson;
                    foreach (var item in dynJson)
                    {
                        DataRow drow = dsRates.Tables[0].NewRow();

                        string CoinName = item.Name;
                        if (CoinName.Contains("_usdt"))
                        {
                            string Price = Object[item.Name]["last"];
                            string Currency = "USDT";
                            string LowestAsk = Object[item.Name]["lowestAsk"];
                            string HighestBid = Object[item.Name]["highestBid"];
                            string PercentChange = Object[item.Name]["percentChange"];
                            string BaseVolume = Object[item.Name]["baseVolume"];
                            string QuoteVolume = Object[item.Name]["quoteVolume"];
                            string High24Hr = Object[item.Name]["high24hr"];
                            string Low24Hr = Object[item.Name]["low24hr"];
                            //ch.Add(new CoinHistory()
                            //{
                            //    CoinName = CoinName,
                            //    Price = Price,
                            //    Currency = Currency,
                            //    LowestAsk = LowestAsk,
                            //    HighestBid = HighestBid,
                            //    PercentChange = PercentChange,
                            //    BaseVolume = BaseVolume,
                            //    QuoteVolume = QuoteVolume,
                            //    High24Hr = High24Hr,
                            //    Low24Hr = Low24Hr,
                            //    CreatedDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:MM")
                            //});

                            drow["Batch"] = currBatch;
                            drow["Price"] = Price;
                            drow["CoinName"] = CoinName.ToString().Replace("_usdt", "").ToUpper();
                            drow["CoinPair"] = CoinName.ToUpper();
                            drow["Currency"] = Currency;
                            drow["LowestAsk"] = LowestAsk;
                            drow["HighestBid"] = HighestBid;
                            drow["PercentChange"] = PercentChange;
                            drow["BaseVolume"] = BaseVolume;
                            drow["QuoteVolume"] = QuoteVolume;
                            drow["High24Hr"] = High24Hr;
                            drow["Low24Hr"] = Low24Hr;
                            drow["CreatedDate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:MM");
                            dsRates.Tables[0].Rows.Add(drow);
                        }
                    }
                     
                    sm.BulkCopy(dsRates, conStr);

                    int newBatch = Convert.ToInt32(currBatch) + 1;
                    sm.UpdateBatch(newBatch, "GateBatchRates", conStr);

                    _logger.LogInformation("Rates Retrieved at : " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:MM"));
                    TimeSpan ts = new TimeSpan(0, 0, Interval);                                     
                    await Task.Delay(ts, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Connetion Exception: " + ex.ToString());
                }               
            }
        }
    }
}
