using System.Collections.Generic;
using System.Web;
using PoeParser.Common;
using PoeParser.Filter.Tooltip;
using Tornado.Common.Utility;
using Tornado.Parser.Filter;
using Tornado.Parser.Filter.Tooltip;

namespace Tornado.Parser.Entities {
    public class JewelItem : BaseItem {
        public JewelItem(string source, ItemRarity rarity, string name) : base(source, rarity, ItemType.Jewel, name) {
            Color = ToolTipColor.GeneralGroup;
        }

        public override NiceDictionary<string, string> GetPoeTradeAffixes(FilterResult filter) {
            NiceDictionary<string, string> generalParams = new NiceDictionary<string, string>();
            generalParams.Add("league", Config.LeagueName);
            generalParams.Add("name", HttpUtility.UrlEncode(Name));

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