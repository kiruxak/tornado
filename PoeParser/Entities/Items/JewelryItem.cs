using System.Collections.Generic;
using PoeParser.Common;
using PoeParser.Filter;
using PoeParser.Filter.Tooltip;

namespace PoeParser.Entities {
    public class JewelryItem : Item {
        public JewelryItem(string source, ItemRarity rarity, Core core, string name) : base(source, rarity, core, name) {
            Color = ToolTipColor.GeneralGroup;
        }

        public override Dictionary<string, string> GetPoeTradeAffixes(FilterResult filter) {
            return Core.GetPoeTradeAffixes(filter);
        }

        public override string ToString() {
            return $"{EnumInfo<ItemType>.Current[Type].DisplayName}: {Name} - {Price}";
        }
    }
}