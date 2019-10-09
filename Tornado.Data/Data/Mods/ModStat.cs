using Newtonsoft.Json;

namespace Tornado.Data.Data {
    public class ModStat {
        [JsonProperty("key")]
        public long Key { get; set; }

        [JsonProperty("min")]
        public long Min { get; set; }

        [JsonProperty("max")]
        public long Max { get; set; }

        private StatValue _modStat = null;

        public StatValue StatValue => _modStat ?? (_modStat = Root.Data.Stats[Key.ToString()]);
    }
}