namespace ExchangeRate
{
    using System;
    using System.Configuration;
    using System.Data.SqlClient;
    using HtmlAgilityPack;
    using ScrapySharp.Network;
    using Newtonsoft.Json;

    class Program
    {
        static ScrapingBrowser _browser = new ScrapingBrowser();

        static void Main(string[] args)
        {
            string jsonstring = ExtractBanreservasExchangeRate();
            StoreExchangeRate(jsonstring);
        }

        static HtmlNode GetHtml(string url)
        {
            WebPage webpage = _browser.NavigateToPage(new Uri(url));
            return webpage.Html;
        }

        static string ExtractBanreservasExchangeRate()
        {
            string jsonstring = string.Empty;
            try
            {                
                BankRates bankrates = new BankRates("Banreservas");

                HtmlNode webpage = GetHtml("https://www.banreservas.com/#");
                HtmlNode tablenode = webpage.OwnerDocument.DocumentNode.SelectSingleNode(".//table[@class='currency-box-table']");
                HtmlNodeCollection nodes = tablenode.SelectNodes(".//tr");
                foreach (HtmlNode node in nodes)
                {
                    if (!string.IsNullOrEmpty(node.SelectSingleNode("td").InnerHtml))
                    {
                        HtmlNodeCollection exchangeRateNodes = node.SelectNodes("td");
                        string rate = exchangeRateNodes[0].InnerHtml;
                        string buy = exchangeRateNodes[1].InnerHtml;
                        string sell = exchangeRateNodes[2].InnerHtml;

                        bankrates.Rates.Add(new ExchangeRate() { Rate = rate, Buy = buy, Sell = sell });
                    }
                }
                if (bankrates.Rates.Count > 0)
                {
                    bankrates.Rates.Add(new ExchangeRate() { Rate = "DOP", Buy = "1.00", Sell = "1.00" });
                    jsonstring = JsonConvert.SerializeObject(bankrates);                    
                }                
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Error {0}", ex.Message));
            }
            return jsonstring;
        }

        static void StoreExchangeRate(string jsonrate)
        {
            string sqlData = "[acc].[proc_SaveExchangeRate]";
            SqlCommand command = new SqlCommand(sqlData);

            command.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString);
            command.Connection.Open();
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@CompanyGroupID", "2");
            command.Parameters.AddWithValue("@Data", jsonrate);
            command.Parameters.AddWithValue("@UserID", "1");
            command.ExecuteNonQuery();
            command.Connection.Close();
        }
    }
}
