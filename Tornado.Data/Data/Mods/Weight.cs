using Newtonsoft.Json;

namespace Tornado.Data.Data {
    public class Weight {
        [JsonProperty("key")]
        public Tags Key { get; set; }

        [JsonProperty("value")]
        public long Value { get; set; }

        public override string ToString() {
            return $"{Key} - {Value}";
        }
    }
}