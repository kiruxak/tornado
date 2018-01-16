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
            var properties =  Core.GetPoeTradeAffixes(filter);

            if (ImplicitSource.Contains("Has 1 Abyssal Socket")) {
                properties.Add("base", "Stygian Vise");
            }
            return properties;
        }

        public override string ToString() {
            return $"{EnumInfo<ItemType>.Current[Type].DisplayName}: {Name} - {Price}";
        }
    }
}