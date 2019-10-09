using System.Linq;
using Newtonsoft.Json;

namespace Tornado.Data.Data {
    public class BaseItem
    {
        [JsonProperty("$index")]
        public long Index { get; set; }

        [JsonProperty("Id")]
        public string Id { get; set; }

        [JsonProperty("ItemClass")]
        public long ItemClass { get; set; }

        private ItemClass _class = null;

        public ItemClass Class => _class ?? (_class = Root.Data.ItemClasses.First(s => (int)s.Index == ItemClass));

        [JsonProperty("width")]
        public long Width { get; set; }

        [JsonProperty("height")]
        public long Height { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("DropLevel")]
        public long DropLevel { get; set; }

        [JsonProperty("implicits")]
        public long[] Implicits { get; set; }

        private Mod[] _implicits = null;

        public Mod[] ModImplicits => _implicits ?? (_implicits = Implicits.Select(s => Root.Data.Mods[s.ToString()]).ToArray());

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("w")]
        public long W { get; set; }

        [JsonProperty("h")]
        public long H { get; set; }

        [JsonProperty("tags")]
        public Tags[] Tags { get; set; }

        [JsonProperty("stats")]
        public Stats Stats { get; set; }

        public override string ToString()
        {
            return $"{Index} - {Name}";
        }
    }
}