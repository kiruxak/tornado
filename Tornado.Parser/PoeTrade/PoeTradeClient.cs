using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Tornado.Common.Utility;
using Tornado.Parser.PoeTrade.Data;
using Tornado.Parser.PoeTrade.Response;

namespace Tornado.Parser.PoeTrade {
    public class PoeTradeClient {
        public const string PoeTradeUrl = "http://poe.trade/search";
        public const string PoeTradeSortQuery = "sort=price_in_chaos&bare=true";

        private HttpClient CreateHttpClient() {
            var client = new HttpClient {
                BaseAddress = new Uri(PoeTradeUrl)
            };

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));

            return client;
        }

        public async Task<string> GetData(List<ITradeAffix> affixes, NiceDictionary<string, string> gerenalParams) {
            var client = CreateHttpClient();
            var x = new QueryGenerator().GenerateQuery(affixes, gerenalParams);
            var content = new StringContent(x, Encoding.UTF8, "application/json");

            try {
                var apiResponse = await client.PostAsync(PoeTradeUrl, content);
                var responseData = await apiResponse.Content.ReadAsStringAsync();
                return responseData;
            } finally {
                client?.Dispose();
            }
        }

        public async Task<List<PoeItemData>> GetPrice(List<ITradeAffix> affixes, NiceDictionary<string, string> gerenalParams) {
            var client = new PoeTradeClient();
            var data = await client.GetData(affixes, gerenalParams);
            return PoeItemData.Parse(data);
        }

        public async void OpenPoeTrade(List<ITradeAffix> affixes, NiceDictionary<string, string> gerenalParams) {
            var client = CreateHttpClient();
            var x = new QueryGenerator().GenerateQuery(affixes, gerenalParams);
            var content = new StringContent(x, Encoding.UTF8, "application/json");

            try {
                var apiResponse = await client.PostAsync(PoeTradeUrl, content);
                Process.Start(apiResponse.RequestMessage.RequestUri.AbsoluteUri);
            } finally {
                client?.Dispose();
            }
        }
    }
}