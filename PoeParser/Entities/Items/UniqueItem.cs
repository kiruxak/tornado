
using System.Collections.Generic;
using System.Linq;
using PoeParser.Common;
using PoeParser.Entities.Affixes;
using PoeParser.Filter;
using PoeParser.Filter.Tooltip;

namespace PoeParser.Entities {
    public class UniqueItem: Item {
        public string UniqueType { get; set; }
        public UniqueItem(string source, Core core, string name, string type) : base(source, ItemRarity.Unique, core, name) {
            UniqueType = type;
            Color = ToolTipColor.UniqueGroup;

            var affixes = core.GetAffixes(this);
            foreach (IAffixValue affix in affixes) {
                Affixes.Add(affix.Name, affix);
            }
        }

        public override Dictionary<string, string> GetPoeTradeAffixes(FilterResult filter) {
            Dictionary<string, string> generalParams = Core.GetPoeTradeAffixes(filter);
            generalParams.Add("name", System.Web.HttpUtility.UrlEncode(Name) + " " + UniqueType);
            generalParams.Add("rarity","unique");
            generalParams.Add("corrupted", Corrupted ? "1" : "0");
            if (Type == ItemType.Flask) { generalParams.Add("q_min", Quality.ToString()); }

            return generalParams;
        }

        public override string ToString() {
            return $"{EnumInfo<ItemType>.Current[Type].DisplayName}: {Name} - {Price}";
        }
    }
}