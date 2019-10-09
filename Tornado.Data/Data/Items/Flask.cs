using Newtonsoft.Json;

namespace Tornado.Data.Data {
    public class Flask
    {
        [JsonProperty("life")]
        public long Life { get; set; }

        [JsonProperty("mana")]
        public long Mana { get; set; }

        [JsonProperty("duration")]
        public long Duration { get; set; }

        [JsonProperty("maxCharges")]
        public long MaxCharges { get; set; }

        [JsonProperty("chargesPerUse")]
        public long ChargesPerUse { get; set; }
    }
}