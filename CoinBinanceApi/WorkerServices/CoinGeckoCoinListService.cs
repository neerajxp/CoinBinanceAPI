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
    public class CoinGeckoCoinListService : BackgroundService
    {
        private readonly ILogger<CoinGeckoCoinListService> _logger;
        private readonly ILogger<SharedModule> _loggerShared;
        private readonly IConfiguration _config;
        private string conStr;

        public CoinGeckoCoinListService(ILogger<CoinGeckoCoinListService> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CoinGecko CoinList Retrieving start : " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:MM"));
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    Global.constrGate = _config["ConnectionStringGate"];
                    Global.constrLocal = _config["ConnectionStringLocal"];
                    conStr = Global.constrGate;
                    Global.isLocalMode = 0;
                    if (_config["isLocalMode"] == "1" || System.Net.Dns.GetHostName() == "Cygnet")
                    {
                        conStr = Global.constrLocal;
                        Global.isLocalMode = 1;
                    }
                    SharedModule sm = new SharedModule(_loggerShared, _config);
                    SqlConnection makeSQLConn = new SqlConnection(conStr);
                    SqlDataAdapter da = new SqlDataAdapter("select * from  CoinGeckoCoinList where 1=2", conStr);
                    DataSet dsRates = new DataSet();
                    da.Fill(dsRates, "CoinGeckoCoinList");
                    dsRates.Tables[0].Clear();

                    DataTable dtMaster = sm.ExecuteQuery("select * from BatchMaster", conStr);
                    DataRow[] drowMaster = dtMaster.Select("ParameterName='CoinGeckoBatchCoinList'");
                    int currBatch = Convert.ToInt32(drowMaster[0]["ParameterValue"]);
                    drowMaster = dtMaster.Select("ParameterName='CoinGeckoRateIntervalSeconds'");
                    int Interval = Convert.ToInt32(drowMaster[0]["ParameterValue"]);
                    
                    string url = @"https://api.coingecko.com/api/v3/coins/list?include_platform=true";

                    var jsonResponse = sm.GetAPICall(url, 0, "");

                    var Object = JArray.Parse(jsonResponse); // parse as array  
                    foreach (JObject item in Object)
                    {                        
                        var pltfrm = item["platforms"].Children().ToList();
                        if(pltfrm.Count>0)
                        {
                            foreach (var p in pltfrm)
                            {
                                DataRow drow = dsRates.Tables[0].NewRow();
                                drow["Batch"] = currBatch;
                                drow["CoinId"] = Convert.ToString(item["id"]);
                                drow["CoinSymbol"] = Convert.ToString(item["symbol"]);
                                drow["CoinName"] = Convert.ToString(item["name"]);
                                drow["PlatformName"] = Convert.ToString(((JProperty)p).Name);
                                drow["TokenAddress"] = Convert.ToString(((JProperty)p).Value);
                                drow["CreatedDate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:MM");
                                dsRates.Tables[0].Rows.Add(drow);
                            }
                        }
                        else
                        {
                            DataRow drow = dsRates.Tables[0].NewRow();
                            drow["Batch"] = currBatch;
                            drow["CoinId"] = Convert.ToString(item["id"]);
                            drow["CoinSymbol"] = Convert.ToString(item["symbol"]);
                            drow["CoinName"] = Convert.ToString(item["name"]); 
                            drow["CreatedDate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:MM");
                            dsRates.Tables[0].Rows.Add(drow);
                        }                       
                    }                  

                    sm.BulkCopy(dsRates, conStr);

                    int newBatch = Convert.ToInt32(currBatch) + 1;
                    sm.UpdateBatch(newBatch, "CoinGeckoBatchCoinList", conStr);

                    _logger.LogInformation("CoinGeckoCoinList Rates Retrieved at : " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:MM"));
                    TimeSpan ts = new TimeSpan(0, 0, Interval);
                    await Task.Delay(ts, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError("CoinGecko Connetion Exception: " + ex.ToString());
                }
            }
        }
    }
}
