using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
   
using System.Text.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Globalization;

namespace CoinBinanceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoinListController : ControllerBase
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly ILogger<CoinListController> _logger;
        public static ArrayList coins;
      
        public CoinListController(ILogger<CoinListController> logger)
        {
            _logger = logger;
        }

        public static async Task<List<Repository>> ProcessRepositories()
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

            var streamTask = client.GetStreamAsync("https://api.github.com/orgs/dotnet/repos");
            var repositories = await System.Text.Json.JsonSerializer.DeserializeAsync<List<Repository>>(await streamTask);
            return repositories;
        }

        [HttpGet("{id}")]
        public  string  GetSample()
        {    
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://jsonplaceholder.typicode.com/todos/1");
            request.Proxy= new WebProxy("http://winproxyus1.server.lan:3128", true);
            
            request.Method = "GET";
            String test = String.Empty;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                test = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
            }
            return  test;
        }

        [HttpGet]
        public string GetCoinList()
        {
            CreateList();
            int rounding = 2;
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://pro-api.coinmarketcap.com/v1/cryptocurrency/listings/latest");
            request.Proxy = new WebProxy("http://winproxyus1.server.lan:3128", true);
            var API_KEY = "2563a5a6-1274-4943-b0b4-da724381039c";
            request.Headers.Add("X-CMC_PRO_API_KEY", API_KEY);
            request.Headers.Add("Accepts", "application/json");
            request.Method = "GET";
            String jsonToReturn = String.Empty;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                jsonToReturn = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
            }

            string jsonFormatted = JValue.Parse(jsonToReturn).ToString(Formatting.Indented);
            Root obj = Newtonsoft.Json.JsonConvert.DeserializeObject<Root>(jsonFormatted);

            List<CoinList> ls = new List<CoinList>();

            foreach (var s in coins)
            {
                var val = obj.data.Find(x => x.symbol == s.ToString());

                if (val != null)
                {
                    ls.Add(new CoinList()
                    {
                        source = "cmc",
                        id = val.id,
                        name = val.name,
                        symbol = val.symbol,
                        price = Math.Round(val.quote.USD.price, rounding),
                        last_updated = val.last_updated,
                        rank = val.cmc_rank,
                        market_cap = Math.Round(val.quote.USD.market_cap, rounding),
                        volume_24h = Math.Round(val.quote.USD.volume_24h, rounding),
                        percent_change_1h = Math.Round(val.quote.USD.percent_change_1h, rounding),
                        percent_change_24h = Math.Round(val.quote.USD.percent_change_24h, rounding),
                        percent_change_30d = Math.Round(val.quote.USD.percent_change_30d, rounding),
                        percent_change_7d = Math.Round(val.quote.USD.percent_change_7d, rounding)
                    });
                }
            }

            var sz = JsonConvert.SerializeObject(ls);
            //Log.LogInformation("Coin query processed successfully.");
            //return new OkObjectResult(sz);

            return sz;

            /*
            CreateList();
            int rounding = 2;
            try
            {              
                //log.LogInformation("Coin query started.");
                var API_KEY = "2563a5a6-1274-4943-b0b4-da724381039c";
                // var URL = new UriBuilder("https://pro-api.coinmarketcap.com/v1/cryptocurrency/listings/latest");
                var URL = new UriBuilder("https://jsonplaceholder.typicode.com/todos/1");
                var client = new WebClient();
                client.Proxy = new WebProxy("http://winproxyus1.server.lan:3128", true);
                
                client.Headers.Add("X-CMC_PRO_API_KEY", API_KEY);
                client.Headers.Add("Accepts", "application/json");
                string jsonToReturn = client.DownloadString(URL.ToString());
 
                string jsonFormatted = JValue.Parse(jsonToReturn).ToString(Formatting.Indented);
                Root obj = Newtonsoft.Json.JsonConvert.DeserializeObject<Root>(jsonFormatted);
                 
                List<CoinList> ls = new List<CoinList>();
                
                
                foreach (var s in coins)
                {
                    var val = obj.data.Find(x => x.symbol == s.ToString());
                    
                    if (val != null)
                    {
                        ls.Add(new CoinList()
                        {
                            source = "cmc",
                            id = val.id,
                            name = val.name,
                            symbol = val.symbol,
                            price = Math.Round(val.quote.USD.price, rounding),
                            last_updated = val.last_updated,
                            rank = val.cmc_rank,
                            market_cap = Math.Round(val.quote.USD.market_cap, rounding),
                            volume_24h = Math.Round(val.quote.USD.volume_24h, rounding),
                            percent_change_1h = Math.Round(val.quote.USD.percent_change_1h, rounding),
                            percent_change_24h = Math.Round(val.quote.USD.percent_change_24h, rounding),
                            percent_change_30d = Math.Round(val.quote.USD.percent_change_30d, rounding),
                            percent_change_7d = Math.Round(val.quote.USD.percent_change_7d, rounding)
                        });
                    }
                }
                
                var sz = JsonConvert.SerializeObject(ls);
                //Log.LogInformation("Coin query processed successfully.");
                return new OkObjectResult(sz);
                
            }
            catch (Exception ex)
           {
                Console.WriteLine(ex.ToString());
                return null;
            }
            */
        }
        public static void CreateList()
        {
            coins = new ArrayList();
            coins.Add("BTC");
            coins.Add("ETH");
            coins.Add("LTC");
            coins.Add("LINK");
            coins.Add("XLM");
            coins.Add("USDC");
            coins.Add("BCH");
            coins.Add("UNI");
            coins.Add("WBTC");
            coins.Add("AAVE");
            coins.Add("ATOM");
            coins.Add("EOS");
            coins.Add("XTZ");
            coins.Add("DAI");
            coins.Add("SNX");
            coins.Add("ALGO");
            coins.Add("MKT");
            coins.Add("DASH");
            coins.Add("GRT");
            coins.Add("COMP");
            coins.Add("ZEC");
            coins.Add("ETC");
            coins.Add("YFI");
            coins.Add("UMA");
            coins.Add("REN");
            coins.Add("ZRX");
            coins.Add("BAT");
            coins.Add("CGLD");
            coins.Add("BNT");
            coins.Add("LRC");
            coins.Add("OMG");
            coins.Add("MANA");
            coins.Add("KNC");
            coins.Add("NU");
            coins.Add("REP");
            coins.Add("BAND");
            coins.Add("BAL");
            coins.Add("CVC");
            coins.Add("NMR");
            coins.Add("OXT");
            coins.Add("DNT");
        }
    }

    public class Repository
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("html_url")]
        public Uri GitHubHomeUrl { get; set; }

        [JsonPropertyName("homepage")]
        public Uri Homepage { get; set; }

        [JsonPropertyName("watchers")]
        public int Watchers { get; set; }

        [JsonPropertyName("pushed_at")]
        public string JsonDate { get; set; }

        public DateTime LastPush =>
            DateTime.ParseExact(JsonDate, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
    }
    public class CoinList
    {
        public string source { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public string symbol { get; set; }
        public double price { get; set; }
        public DateTime last_updated { get; set; }
        public double volume_24h { get; set; }
        public double percent_change_1h { get; set; }
        public double percent_change_24h { get; set; }
        public double percent_change_7d { get; set; }
        public double percent_change_30d { get; set; }
        public double market_cap { get; set; }
        public int rank { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Status
    {
        public DateTime timestamp { get; set; }
        public int error_code { get; set; }
        public object error_message { get; set; }
        public int elapsed { get; set; }
        public int credit_count { get; set; }
        public object notice { get; set; }
        public int total_count { get; set; }
    }

    public class Platform
    {
        public int id { get; set; }
        public string name { get; set; }
        public string symbol { get; set; }
        public string slug { get; set; }
        public string token_address { get; set; }
    }

    public class USD
    {
        public double price { get; set; }
        public double volume_24h { get; set; }
        public double percent_change_1h { get; set; }
        public double percent_change_24h { get; set; }
        public double percent_change_7d { get; set; }
        public double percent_change_30d { get; set; }
        public double market_cap { get; set; }
        public DateTime last_updated { get; set; }
    }

    public class Quote
    {
        public USD USD { get; set; }
    }

    public class Datum
    {
        public int id { get; set; }
        public string name { get; set; }
        public string symbol { get; set; }
        public string slug { get; set; }
        public int num_market_pairs { get; set; }
        public DateTime date_added { get; set; }
        public List<string> tags { get; set; }
        public long? max_supply { get; set; }
        public double circulating_supply { get; set; }
        public double total_supply { get; set; }
        public Platform platform { get; set; }
        public int cmc_rank { get; set; }
        public DateTime last_updated { get; set; }
        public Quote quote { get; set; }
    }

    public class Root
    {
        public Status status { get; set; }
        public List<Datum> data { get; set; }
    }
}
