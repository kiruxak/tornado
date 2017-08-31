using System.Collections.Generic;
using PoeParser.Common.Extensions;
using PoeParser.Filter;
using PoeParser.Filter.Tooltip;

namespace PoeParser.Entities {
    public class GemItem : BaseItem {
        public int GemLevel { get; set; }
        public override string CacheName => $"{Name}|{GemLevel}|{Quality}";

        public GemItem(string source, string name) : base(source, ItemRarity.Gem, ItemType.Gem, name) {
            GemLevel = source.ParseTo(1, "Level: (\\d*)");
            Color = ToolTipColor.Gem;
        }

        public override Dictionary<string, string> GetPoeTradeAffixes(FilterResult filter) {
            Dictionary<string,string> generalParams = new Dictionary<string, string>();
            generalParams.Add("league", Config.LeagueName);
            generalParams.Add("name", System.Web.HttpUtility.UrlEncode(Name));
            generalParams.Add("level_min", GemLevel.ToString());
            generalParams.Add("q_min", Quality.ToString());
            generalParams.Add("corrupted", Corrupted ? "1" : "0");

            return generalParams;
        }

        public override List<ColoredLine> GetTooltip(FilterResult filter) {
            var lines = new List<ColoredLine>()
                .Add($"Gem: {Name}", ToolTipColor.Gem, 18)
                .AddEmptyLine()
                .Add($"Level: {GemLevel}", ToolTipColor.GeneralGroup, 18)
                .Add($"Quality: {Quality}", ToolTipColor.GeneralGroup, 18)
                .If(Corrupted, (list => list.Add("Corrupted", ToolTipColor.Corrupted, 18)));

            return lines;
        }

        public override string ToString() {
            return $"Gem: {Name} - L{GemLevel} Q{Quality} - {Price}";
        }
    }
}