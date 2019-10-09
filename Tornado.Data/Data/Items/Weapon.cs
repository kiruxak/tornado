using Newtonsoft.Json;

namespace Tornado.Data.Data {
    public class Weapon
    {
        [JsonProperty("crit")]
        public long Crit { get; set; }

        [JsonProperty("attackSpeed")]
        public long AttackSpeed { get; set; }

        [JsonProperty("dMin")]
        public long DMin { get; set; }

        [JsonProperty("dMax")]
        public long DMax { get; set; }

        [JsonProperty("range")]
        public long Range { get; set; }
    }
}