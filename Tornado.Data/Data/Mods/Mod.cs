using System.Linq;
using Newtonsoft.Json;

namespace Tornado.Data.Data {
    public class Mod {
        [JsonProperty("$index")]
        public long Index { get; set; }

        [JsonProperty("Id")]
        public string Id { get; set; }

        [JsonProperty("ModTypeKey")]
        public long ModTypeKey { get; set; }

        [JsonProperty("Level")]
        public long Level { get; set; }

        [JsonProperty("CorrectGroup")]
        public string CorrectGroup { get; set; }

        [JsonProperty("Domain")]
        public Domain Domain { get; set; }

        [JsonProperty("GenerationType")]
        public GenType GenerationType { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("stats")]
        public ModStat[] Stats { get; set; }

        [JsonProperty("weight", NullValueHandling = NullValueHandling.Ignore)]
        public Weight[] Weight { get; set; }

        [JsonProperty("TagKeys", NullValueHandling = NullValueHandling.Ignore)]
        public long[] TagKeys { get; set; }

        [JsonProperty("genWeight", NullValueHandling = NullValueHandling.Ignore)]
        public Weight[] GenWeight { get; set; }

        [JsonProperty("grantedEffectId", NullValueHandling = NullValueHandling.Ignore)]
        public string GrantedEffectId { get; set; }

        [JsonProperty("grantedEffectLevel", NullValueHandling = NullValueHandling.Ignore)]
        public long? GrantedEffectLevel { get; set; }

        [JsonProperty("grantedBuffId", NullValueHandling = NullValueHandling.Ignore)]
        public string GrantedBuffId { get; set; }

        public override string ToString() {
            return $"{Index} - {CorrectGroup} [{Stats.FirstOrDefault()?.Min}-{Stats.FirstOrDefault()?.Max}]";
        }
    }
}