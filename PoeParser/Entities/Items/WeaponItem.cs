using System.Collections.Generic;
using PoeParser.Common;
using PoeParser.Entities.Affixes;
using PoeParser.Filter;
using PoeParser.Filter.Tooltip;

namespace PoeParser.Entities {
    public class WeaponItem: Item {
        public WeaponItem(string source, ItemRarity rarity, Core core, string name) : base(source, rarity, core, name) {
            var weaponAffixes = core.GetAffixes(this);
            Color = ToolTipColor.GeneralGroup;

            foreach (IAffixValue affix in weaponAffixes) {
                Affixes.Add(affix.Name, affix);
            }
        }

        public override Dictionary<string, string> GetPoeTradeAffixes(FilterResult filter) {
            var generalParams = Core.GetPoeTradeAffixes(filter);
            generalParams.Add("rarity", "rare");
            return generalParams;
        }

        public override string ToString() {
            return $"{EnumInfo<ItemType>.Current[Type].DisplayName}: {Name} - {Price}";
        }
    }
}