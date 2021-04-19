using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CoinBinanceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsRSSController : ControllerBase
    {
        public static int rand = 50;
        public static int count = 2;
        [HttpGet]
        public string GetNewsRSS()
        {
            Random random = new Random();
            List<NewsModel> newsList = new List<NewsModel>();
            CoinTelegraph(random, newsList);
            BitcoinNews(random, newsList);
            NewsBTC(random, newsList);

            newsList.Sort(delegate (NewsModel news1, NewsModel news2)
            {
                return news1.id.CompareTo(news2.id);
            });
            var sz = Newtonsoft.Json.JsonConvert.SerializeObject(newsList);

            return sz;
        }

        private static void CoinTelegraph(Random random,  List<NewsModel> newsList)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://cointelegraph.com/rss");
                request.Proxy = new WebProxy("http://winproxyus1.server.lan:3128", true);
                request.Headers.Add("Accepts", "application/json");
                request.Method = "GET";
                String apiResponse = String.Empty;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Stream dataStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(dataStream);
                    apiResponse = reader.ReadToEnd();
                    reader.Close();
                    dataStream.Close();
                }
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(new System.IO.StringReader(apiResponse));
                XmlNodeList item = xmlDoc.GetElementsByTagName("item");
                count = item.Count < count ? item.Count : count;
                for (int i = 0; i < count; i++)
                {
                    string title = item[i].ChildNodes[0].InnerText;
                    string url = item[i].ChildNodes[1].InnerText;
                    string author = item[i].ChildNodes[5].InnerText.Replace("Cointelegraph By ", "");
                    //string icon = item[i].ChildNodes[10].InnerText;
                    string published = item[i].ChildNodes[4].InnerText;

                    int st = item[i].InnerText.IndexOf("<img src=") + 10;
                    int en = item[i].InnerText.IndexOf(".jpg") + 4;
                    string thumbnail = item[i].InnerText.Substring(st, en - st);
                    newsList.Add(new NewsModel()
                    {
                        id = random.Next(rand),
                        newssource = "Coin Telegraph",
                        source = "cointelegraph",
                        thumbnail = thumbnail,
                        headline = title,
                        newsurl = url,
                        published = published,
                        author = author
                    });
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }            
        }

        private static void NewsBTC(Random random, List<NewsModel> newsList)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("http://www.newsbtc.com/feed");
                request.Proxy = new WebProxy("http://winproxyus1.server.lan:3128", true);
                request.Headers.Add("Accepts", "application/json");
                request.Method = "GET";
                String apiResponse = String.Empty;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Stream dataStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(dataStream);
                    apiResponse = reader.ReadToEnd();
                    reader.Close();
                    dataStream.Close();
                }

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(new System.IO.StringReader(apiResponse));
                XmlNodeList item = xmlDoc.GetElementsByTagName("item");
                count = item.Count < count ? item.Count : count;
                for (int i = 0; i < count; i++)
                {
                    string title = item[i].ChildNodes[0].InnerText;
                    string url = item[i].ChildNodes[1].InnerText;
                    string author = "";
                    string published = item[i].ChildNodes[2].InnerText;
                    int st = item[i].InnerText.IndexOf("https://www.newsbtc.com/wp-content/");
                    int en = item[i].InnerText.IndexOf(".png") + 4;
                    string thumbnail = item[i].InnerText.Substring(st, en - st);
                    newsList.Add(new NewsModel()
                    {
                        id = random.Next(rand),
                        newssource = "NewsBTC",
                        source = "newsbtc",
                        thumbnail = thumbnail,
                        headline = title,
                        newsurl = url
                    ,
                        published = published,
                        author = author
                    });
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
           
        }
        private static void BitcoinNews(Random random, List<NewsModel> newsList)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://news.bitcoin.com/feed");
                request.Proxy = new WebProxy("http://winproxyus1.server.lan:3128", true);
                request.Headers.Add("Accepts", "application/json");
                request.Method = "GET";
                String apiResponse = String.Empty;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Stream dataStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(dataStream);
                    apiResponse = reader.ReadToEnd();
                    reader.Close();
                    dataStream.Close();
                }

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(new System.IO.StringReader(apiResponse));
                XmlNodeList item = xmlDoc.GetElementsByTagName("item");
                count = item.Count < count ? item.Count : count;
                for (int i = 0; i < count; i++)
                {
                    string title = item[i].ChildNodes[0].InnerText;
                    string url = item[i].ChildNodes[1].InnerText;
                    string author = item[i].ChildNodes[2].InnerText;
                    string published = item[i].ChildNodes[3].InnerText;
                    int st = item[i].InnerText.IndexOf("https://news.bitcoin.com/wp-content/");
                    int en = item[i].InnerText.IndexOf(".jpg");
                    if(en ==-1)
                    {
                        en = item[i].InnerText.IndexOf(".jpeg") + 5;
                    }
                    else
                    {
                        en = en + 4;
                    }
                    string thumbnail = item[i].InnerText.Substring(st, en - st);
                    newsList.Add(new NewsModel()
                    {
                        id = random.Next(rand),
                        newssource = "Bitcoin News",
                        source = "bitcoinnews",
                        thumbnail = thumbnail,
                        headline = title,
                        newsurl = url
                    ,
                        published = published,
                        author = author
                    });
                }

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
                  
        }

       
    }
    public class NewsModel
    {
        public string source { get; set; }
        public int id { get; set; }
        public string newssource { get; set; }
        public string headline { get; set; }
        public string content { get; set; }
        public string newsurl { get; set; }
        public string thumbnail { get; set; }
        public string published { get; set; }
        public int hrsago { get; set; }
        public string keywords { get; set; }
        public string author { get; set; }
    }
}
