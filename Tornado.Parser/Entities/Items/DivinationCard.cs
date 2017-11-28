using System.Collections.Generic;
using System.Web;
using PoeParser.Common;
using PoeParser.Filter.Tooltip;
using Tornado.Common.Utility;
using Tornado.Parser.Filter;
using Tornado.Parser.Filter.Tooltip;

namespace Tornado.Parser.Entities {
    public class DivinationCard : BaseItem {
        public DivinationCard(string source, string name) : base(source, ItemRarity.Normal, ItemType.DivinationCard, name) {
            Color = ToolTipColor.NormalItem;
        }

        public override NiceDictionary<string, string> GetPoeTradeAffixes(FilterResult filter) {
            NiceDictionary<string, string> generalParams = new NiceDictionary<string, string>();
            generalParams.Add("league", Config.LeagueName);
            generalParams.Add("name", HttpUtility.UrlEncode(Name));
            return generalParams;
        }

        public override List<ColoredLine> GetTooltip(FilterResult filter) {
            return new List<ColoredLine>().Add($"{EnumInfo<ItemType>.Current[Type].DisplayName}: {Name}", ToolTipColor.GeneralGroup, 18);
        }

        public override string ToString() {
            return $"{EnumInfo<ItemType>.Current[Type].DisplayName}: {Name} - {Price}";
        }
    }
}