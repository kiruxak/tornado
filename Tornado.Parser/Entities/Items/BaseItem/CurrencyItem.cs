using System.Collections.Generic;
using System.Web;
using PoeParser.Common;
using PoeParser.Filter.Tooltip;
using Tornado.Common.Utility;
using Tornado.Parser.Filter;
using Tornado.Parser.Filter.Tooltip;
using Tornado.Parser.PoeTrade.Data;

namespace Tornado.Parser.Entities {
    public class CurrencyItem : BaseItem {
        public CurrencyInfo Info { get; set; }
        public CurrencyItem(string source, string name, CurrencyInfo info) : base(source, ItemRarity.Normal, ItemTypes.VaalFrarments.Contains(name) ? ItemType.VaalFragment : ItemType.Normal, name) {
            Color = ToolTipColor.NormalItem;
            Info = info;
        }

        public override NiceDictionary<string, string> GetPoeTradeAffixes(FilterResult filter) {
            NiceDictionary<string, string> generalParams = new NiceDictionary<string, string>();
            generalParams.Add("league", Config.LeagueName);
            generalParams.Add("name", HttpUtility.UrlEncode(Name));

            return generalParams;
        }

        public override List<ColoredLine> GetTooltip(FilterResult filter) {
            if (Info == null) {
                return new List<ColoredLine>().Add($"{Name}", ToolTipColor.GeneralGroup, 18);
            }
            return new List<ColoredLine>().Add($"{Name}: {Info.chaosEquivalent}c [1c = {1/Info.chaosEquivalent:F2}]", ToolTipColor.GeneralGroup, 18);
        }

        public override string ToString() {
            return $"{EnumInfo<ItemType>.Current[Type].DisplayName}: {Name} - {Price}";
        }
    }
}