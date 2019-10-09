using Newtonsoft.Json;

namespace Tornado.Data.Data {
    public class Stats
    {
        [JsonProperty("req")]
        public Req Req { get; set; }

        [JsonProperty("flask", NullValueHandling = NullValueHandling.Ignore)]
        public Flask Flask { get; set; }

        [JsonProperty("weapon", NullValueHandling = NullValueHandling.Ignore)]
        public Weapon Weapon { get; set; }

        [JsonProperty("defense", NullValueHandling = NullValueHandling.Ignore)]
        public Defense Defense { get; set; }
    }
}