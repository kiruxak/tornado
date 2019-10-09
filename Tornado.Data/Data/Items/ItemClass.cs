using Newtonsoft.Json;

namespace Tornado.Data.Data {
    public class ItemClass
    {
        
        [JsonProperty("index")]
        public ItemType Index { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("Id")]
        public string Id { get; set; }

        [JsonProperty("baseItems")]
        public BaseItem[] BaseItems { get; set; }

        [JsonProperty("special")]
        public Special[] Special { get; set; }

        [JsonProperty("elderTag")]
        public Tags? ElderTag { get; set; }

        [JsonProperty("shaperTag")]
        public Tags? ShaperTag { get; set; }

        public override string ToString() {
            return $"{Index} - {Name}";
        }

        private bool? _isArmor;
        private bool? _is1HWeapon;
        private bool? _is2HWeapon;
        private bool? _isJewelery;

        public bool IsArmor => _isArmor ?? (_isArmor = (Index == ItemType.BodyArmours || 
                                                        Index == ItemType.Boots ||
                                                        Index == ItemType.Gloves ||
                                                        Index == ItemType.Helmets ||
                                                        Index == ItemType.Shields)).Value;

        public bool Is1HWeapon => _is1HWeapon ?? (_is1HWeapon = (Index == ItemType.Claws ||
                                                                 Index == ItemType.Daggers ||
                                                                 Index == ItemType.Wands ||
                                                                 Index == ItemType.OneHandSwords ||
                                                                 Index == ItemType.ThrustingOneHandSwords ||
                                                                 Index == ItemType.OneHandAxes ||
                                                                 Index == ItemType.OneHandMaces ||
                                                                 Index == ItemType.Sceptres)).Value;

        public bool Is2HWeapon => _is2HWeapon ?? (_is2HWeapon = (Index == ItemType.Bows ||
                                                                 Index == ItemType.Staves ||
                                                                 Index == ItemType.TwoHandSwords ||
                                                                 Index == ItemType.TwoHandAxes ||
                                                                 Index == ItemType.TwoHandMaces)).Value;

        public bool IsWeapon => Is1HWeapon || Is2HWeapon;

        public bool IsJewelery => _isJewelery ?? (_isJewelery = (Index == ItemType.Belts ||
                                                                 Index == ItemType.Amulets ||
                                                                 Index == ItemType.Rings)).Value;
    }
}