using Newtonsoft.Json;

namespace Tornado.Data.Data {
    public class Defense
    {
        [JsonProperty("ar")]
        public long Ar { get; set; }

        [JsonProperty("ev")]
        public long Ev { get; set; }

        [JsonProperty("es")]
        public long Es { get; set; }

        [JsonProperty("block", NullValueHandling = NullValueHandling.Ignore)]
        public long? Block { get; set; }
    }
}