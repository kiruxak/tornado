using System.Collections.Generic;
using PoeParser.Common;
using PoeParser.Filter;
using PoeParser.Filter.Tooltip;

namespace PoeParser.Entities {
    public class JewelItem : BaseItem {
        public JewelItem(string source, ItemRarity rarity, string name) : base(source, rarity, ItemType.Jewel, name) {
            Color = ToolTipColor.GeneralGroup;
        }

        public override Dictionary<string, string> GetPoeTradeAffixes(FilterResult filter) {
            Dictionary<string, string> generalParams = new Dictionary<string, string>();
            generalParams.Add("league", Config.LeagueName);
            generalParams.Add("name", System.Web.HttpUtility.UrlEncode(Name));

            return generalParams;
        }

        public override List<ColoredLine> GetTooltip(FilterResult filter) {
            var lines = new List<ColoredLine>();
            lines.Add(new ColoredLine($"Jewel: {Name}", ToolTipColor.GeneralGroup, 18));
            return lines;
        }

        public override string ToString() {
            return $"{EnumInfo<ItemType>.Current[Type].DisplayName}: {Name} - {Price}";
        }
    }
}