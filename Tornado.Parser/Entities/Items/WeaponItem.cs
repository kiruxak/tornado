using PoeParser.Common;
using PoeParser.Filter.Tooltip;
using Tornado.Common.Utility;
using Tornado.Parser.Entities.Affixes;
using Tornado.Parser.Filter;

namespace Tornado.Parser.Entities {
    public class WeaponItem : Item {
        public WeaponItem(string source, ItemRarity rarity, Core core, string name) : base(source, rarity, core, name) {
            var weaponAffixes = core.GetAffixes(this);
            Color = ToolTipColor.GeneralGroup;

            foreach (IAffixValue affix in weaponAffixes) {
                Affixes.Add(affix.Name, affix);
            }
        }

        public override NiceDictionary<string, string> GetPoeTradeAffixes(FilterResult filter) {
            var generalParams = Core.GetPoeTradeAffixes(filter);
            generalParams.Add("rarity", "rare");
            return generalParams;
        }

        public override string ToString() {
            return $"{EnumInfo<ItemType>.Current[Type].DisplayName}: {Name} - {Price}";
        }
    }
}