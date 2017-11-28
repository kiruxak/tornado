using System;
using System.Net;
using System.Text;

namespace PoeParser.PoeTradeIntegration {
    public static class WebHelper {
        public static void DoWebRequest(string url, string data, Action<WebResponse> action) {
            ASCIIEncoding encoding = new ASCIIEncoding();

            var dataBytes = encoding.GetBytes(data);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Timeout = 4000;
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";
            request.ContentLength = dataBytes.Length;

            using (var stream = request.GetRequestStream()) { stream.Write(dataBytes, 0, data.Length); }

            using (WebResponse response = (HttpWebResponse)request.GetResponse()) {
                action?.Invoke(response);
            }
        }
    }
}