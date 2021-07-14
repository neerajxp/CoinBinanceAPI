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
    public class CoinGeckoRatesService : BackgroundService
    {
        private readonly ILogger<GatesRatesService> _logger;
        private readonly ILogger<SharedModule> _loggerShared;
        private readonly IConfiguration _config;
        private string conStr;
        public CoinGeckoRatesService(ILogger<GatesRatesService> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CoinGecko Rates Retrieving start : " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:MM"));
            while (!stoppingToken.IsCancellationRequested)
            {
                int g=0;
                try
                {
                    
                    Global.constrGate = _config["ConnectionStringGate"];
                    Global.constrLocal = _config["ConnectionStringLocal"];
                    conStr = Global.constrGate;
                    Global.isLocalMode = 0;
                    if (_config["isLocalMode"] == "1" || System.Net.Dns.GetHostName()=="Cygnet")
                    {
                        conStr = Global.constrLocal;
                        Global.isLocalMode = 1;
                    }
                    SharedModule sm = new SharedModule(_loggerShared, _config);
                    SqlConnection makeSQLConn = new SqlConnection(conStr);
                    SqlDataAdapter da = new SqlDataAdapter("select * from  CoinGeckoRates where 1=2", conStr);
                    DataSet dsRates = new DataSet();
                    da.Fill(dsRates, "CoinGeckoRates");
                    dsRates.Tables[0].Clear();

                    DataTable dtMaster = sm.ExecuteQuery("select * from BatchMaster", conStr);
                    DataRow[] drowMaster = dtMaster.Select("ParameterName='CoinGeckoRates'");
                    int currBatch = Convert.ToInt32(drowMaster[0]["ParameterValue"]);
                    drowMaster = dtMaster.Select("ParameterName='CoinGeckoRateIntervalSeconds'");
                    int Interval = Convert.ToInt32(drowMaster[0]["ParameterValue"]);
                    //CoinGeckoPerPage
                    drowMaster = dtMaster.Select("ParameterName='CoinGeckoPerPage'");
                    int CoinGeckoPerPage = Convert.ToInt32(drowMaster[0]["ParameterValue"]);
                    //CoinGeckoNoOfPages
                    drowMaster = dtMaster.Select("ParameterName='CoinGeckoNoOfPages'");
                    int CoinGeckoNoOfPages = Convert.ToInt32(drowMaster[0]["ParameterValue"]);


                    for (int i = 1; i <= CoinGeckoNoOfPages; i++)
                    {
                        string url = @"https://api.coingecko.com/api/v3/coins/markets?vs_currency=usd&order=market_cap_desc&per_page=" + CoinGeckoPerPage + "&page=" + i + "&sparkline=false";
                        
                        var jsonResponse = sm.GetAPICall(url, 0, "");

                        var Object = JArray.Parse(jsonResponse); // parse as array  
                        foreach (JObject item in Object)
                        {
                            DataRow drow = dsRates.Tables[0].NewRow();
                            drow["Batch"] = currBatch;
                            drow["CoinId"] = Convert.ToString(item["id"]);
                            drow["CoinSymbol"] = Convert.ToString(item["symbol"]);
                            drow["CoinName"] = Convert.ToString(item["name"]);
                            drow["Current_Price"] = Convert.ToString(item["current_price"]);
                            drow["Market_Cap"] = Convert.ToString(item["market_cap"]);
                            drow["Market_Cap_Rank"] = Convert.ToString(item["market_cap_rank"]);
                            drow["Fully_Diluted_Valuation"] = Convert.ToString(item["fully_diluted_valuation"]);
                            drow["Total_Volume"] = Convert.ToString(item["total_volume"]);
                            drow["High_24h"] = Convert.ToString(item["high_24h"]);
                            drow["Low_24h"] = Convert.ToString(item["low_24h"]);
                            drow["Price_Change_24h"] = Convert.ToString(item["price_change_24h"]);
                            drow["Price_Change_Percentage_24h"] = Convert.ToString(item["price_change_percentage_24h"]);
                            drow["Market_Cap_Change_24h"] = Convert.ToString(item["market_cap_change_24h"]);
                            drow["Market_Cap_Change_Percentage_24h"] = Convert.ToString(item["market_cap_change_percentage_24h"]);
                            drow["Circulating_Supply"] = Convert.ToString(item["circulating_supply"]);
                            drow["Total_Supply"] = Convert.ToString(item["total_supply"]);
                            drow["Max_Supply"] = Convert.ToString(item["max_supply"]);
                            drow["Ath"] = Convert.ToString(item["ath"]);
                            drow["Ath_change_percentage"] = Convert.ToString(item["ath_change_percentage"]);
                            drow["Ath_Date"] = Convert.ToString(item["ath_date"]);
                            drow["Atl"] = Convert.ToString(item["atl"]);
                            drow["Atl_Change_Percentage"] = Convert.ToString(item["atl_change_percentage"]);
                            drow["Atl_Date"] = Convert.ToString(item["atl_date"]);
                            drow["Roi"] = Convert.ToString(item["roi"]);
                            drow["Last_Updated"] = Convert.ToString(item["last_updated"]);
                            drow["Image_Url"] = Convert.ToString(item["image"]);
                            drow["CreatedDate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:MM");
                            dsRates.Tables[0].Rows.Add(drow);
                        }

                        g = i;
                        sm.BulkCopy(dsRates, conStr);
                        dsRates.Tables[0].Clear();
                        dsRates.AcceptChanges();
                    }

                   // sm.BulkCopy(dsRates, conStr);

                    int newBatch = Convert.ToInt32(currBatch) + 1;
                    sm.UpdateBatch(newBatch, "CoinGeckoRates", conStr);

                    _logger.LogInformation("CoinGecko Rates Retrieved at : " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:MM"));
                    TimeSpan ts = new TimeSpan(0, 0, Interval);
                    await Task.Delay(ts, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(g.ToString());
                    _logger.LogError("CoinGecko Connetion Exception: " + ex.ToString());
                }
            }

        }   
    }
}
