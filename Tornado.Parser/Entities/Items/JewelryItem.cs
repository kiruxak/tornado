using PoeParser.Common;
using PoeParser.Filter.Tooltip;
using Tornado.Common.Utility;
using Tornado.Parser.Filter;

namespace Tornado.Parser.Entities {
    public class JewelryItem : Item {
        public JewelryItem(string source, ItemRarity rarity, Core core, string name) : base(source, rarity, core, name) {
            Color = ToolTipColor.GeneralGroup;
        }

        public override NiceDictionary<string, string> GetPoeTradeAffixes(FilterResult filter) {
            return Core.GetPoeTradeAffixes(filter);
        }

        public override string ToString() {
            return $"{EnumInfo<ItemType>.Current[Type].DisplayName}: {Name} - {Price}";
        }
    }
}