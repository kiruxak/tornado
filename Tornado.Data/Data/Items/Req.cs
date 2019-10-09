using Newtonsoft.Json;

namespace Tornado.Data.Data {
    public class Req
    {
        [JsonProperty("str", NullValueHandling = NullValueHandling.Ignore)]
        public long? Str { get; set; }

        [JsonProperty("dex", NullValueHandling = NullValueHandling.Ignore)]
        public long? Dex { get; set; }

        [JsonProperty("int", NullValueHandling = NullValueHandling.Ignore)]
        public long? Int { get; set; }
    }
}