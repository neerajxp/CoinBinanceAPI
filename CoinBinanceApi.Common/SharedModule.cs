using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Text;
 

namespace CoinBinanceApi.Common
{
    public class SharedModule
    { 
        private readonly ILogger<SharedModule> _logger;
        private readonly IConfiguration _config;
        public SharedModule(ILogger<SharedModule> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public void ExecuteNonQuery(string SQLQeury, string constr)
        {
            try
            {
                SqlConnection SQLConn = new SqlConnection(constr);
                SqlCommand cmd = new SqlCommand(SQLQeury, SQLConn);
                SQLConn.Open();
                int result = cmd.ExecuteNonQuery();
                SQLConn.Close();
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception: ExecuteNonQuery() Method " + ex.ToString());
            }
        }

        public DataTable ExecuteQuery(string SQLQeury, string constr)
        {
            try
            {
                SqlConnection makeSQLConn = new SqlConnection(constr);
                SqlDataAdapter da = new SqlDataAdapter(SQLQeury, constr);
                DataSet dsresult = new DataSet();
                da.Fill(dsresult, "result");

                return dsresult.Tables[0];
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception: ExecuteQuery() Method " + ex.ToString());
                return null;
            }
        }
        public void UpdateBatch(int BatchId, string Parameter, string constr)
        {
            try
            {
                SqlConnection SQLConn = new SqlConnection(constr);
                SqlCommand cmd = new SqlCommand("UPDATE BatchMaster set ParameterValue = " + BatchId + " where ParameterName='" + Parameter + "'", SQLConn);
                SQLConn.Open();               
                int result = cmd.ExecuteNonQuery();

                SQLConn.Close();
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception: UpdateBatch() Method " + ex.ToString());
            }
        }

        public void BulkCopy(DataSet ds, string constr)
        {

            try
            {
                using (SqlConnection connection = new SqlConnection(constr))
                {
                    connection.Open();
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                    {

                        foreach (DataColumn c in ds.Tables[0].Columns)
                            bulkCopy.ColumnMappings.Add(c.ColumnName, c.ColumnName);

                        bulkCopy.DestinationTableName = ds.Tables[0].TableName;
                        try
                        {
                            bulkCopy.WriteToServer(ds.Tables[0]);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Exception: BulkCopy() WriteToServer() Method " + ex.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception: BulkCopy() Method " + ex.ToString());
            }

        }


        public int GetBatch(string Parameter, string constr)
        {
            try
            {
                SqlConnection SQLConn = new SqlConnection(constr);
                SqlCommand cmd = new SqlCommand("Select ParameterValue from BatchMaster where ParameterName='" + Parameter + "'", SQLConn);
                SQLConn.Open();
                //System.NullReferenceException occurs when their is no data/result
                string getValue = cmd.ExecuteScalar().ToString();
                if (getValue != null)
                {
                    string result = getValue.ToString();
                }
                SQLConn.Close();

                return Convert.ToInt32(getValue);
            }
            catch(Exception ex)
            {
                _logger.LogError("Exception: GetBatch() Method " + ex.ToString());
                return 0;
            }
  
        }

        public string GetAPICall(string url, int isKeyRequired, string key)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                
                if (Global.isLocalMode == 0)
                {
                    request.Proxy = new WebProxy("http://winproxyus1.server.lan:3128", true);
                }                               

                if (isKeyRequired == 1)
                {
                    var API_KEY = "2563a5a6-1274-4943-b0b4-da724381039c";
                    request.Headers.Add("X-CMC_PRO_API_KEY", API_KEY);
                }

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

                //string jsonFormatted = JValue.Parse(jsonToReturn).ToString(Formatting.Indented);
                
                //var sz = JsonConvert.SerializeObject(ls);           
                return jsonToReturn;
            }
            catch(Exception ex)
            {
                _logger.LogError("Exception: GetAPICall() Method " + ex.ToString());
                return "false";
            }
        }
    }
}

