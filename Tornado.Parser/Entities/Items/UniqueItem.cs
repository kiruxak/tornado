using System.Web;
using PoeParser.Common;
using PoeParser.Filter.Tooltip;
using Tornado.Common.Utility;
using Tornado.Parser.Entities.Affixes;
using Tornado.Parser.Filter;

namespace Tornado.Parser.Entities {
    public class UniqueItem : Item {
        public string UniqueType { get; set; }

        public UniqueItem(string source, Core core, string name, string type) : base(source, ItemRarity.Unique, core, name) {
            UniqueType = type;
            Color = ToolTipColor.UniqueGroup;

            var affixes = core.GetAffixes(this);
            foreach (IAffixValue affix in affixes) {
                Affixes.Add(affix.Name, affix);
            }
        }

        public override NiceDictionary<string, string> GetPoeTradeAffixes(FilterResult filter) {
            NiceDictionary<string, string> generalParams = Core.GetPoeTradeAffixes(filter);
            generalParams.Add("name", HttpUtility.UrlEncode(Name) + " " + UniqueType);
            generalParams.Add("rarity", "unique");
            generalParams.Add("corrupted", Corrupted ? "1" : "0");
            if (Type == ItemType.Flask) {
                generalParams.Add("q_min", Quality.ToString());
            }

            return generalParams;
        }

        public override string ToString() {
            return $"{EnumInfo<ItemType>.Current[Type].DisplayName}: {Name} - {Price}";
        }
    }
}