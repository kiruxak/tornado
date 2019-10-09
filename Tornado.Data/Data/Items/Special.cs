using Newtonsoft.Json;

namespace Tornado.Data.Data {
    public class Special
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("mod")]
        public long Mod { get; set; }
    }
}