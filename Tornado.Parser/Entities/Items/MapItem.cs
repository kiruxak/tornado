using System.Collections.Generic;
using System.Linq;
using System.Web;
using PoeParser.Filter.Tooltip;
using Tornado.Common.Utility;
using Tornado.Parser.Common.Extensions;
using Tornado.Parser.Filter;
using Tornado.Parser.Filter.Tooltip;

namespace Tornado.Parser.Entities {
    public class MapItem : BaseItem {
        public int MapTier { get; set; }
        public override string CacheName => $"{Name}{(Rarity == ItemRarity.Rare && Unindentified ? "Unidentified Rare" : "")}";

        public MapItem(string source, ItemRarity rarity, string[] lines, int mapTier) : base(source, rarity, ItemType.Map, GetMapName(lines, rarity, source.ContainsPattern("Unidentified"))) {
            MapTier = mapTier;
            Color = ToolTipColor.GeneralGroup;
        }

        protected static string GetMapName(string[] source, ItemRarity rarity, bool unindentified) {
            if (rarity == ItemRarity.Normal)
                return source[1];
            if (rarity == ItemRarity.Magic)
                return ItemTypes.Maps.First(m => source[1].Contains(m + " ")) + " Map";
            if (unindentified) {
                return ItemTypes.Maps.First(m => source[1].Contains(m + " ")) + " Map";
            }
            return source[2];
        }

        public override NiceDictionary<string, string> GetPoeTradeAffixes(FilterResult filter) {
            NiceDictionary<string, string> generalParams = new NiceDictionary<string, string>();
            generalParams.Add("league", Config.LeagueName);
            generalParams.Add("name", HttpUtility.UrlEncode(Name));
            if (Rarity == ItemRarity.Rare && Unindentified) {
                generalParams.Add("identified", "0");
                generalParams.Add("rarity", "rare");
            }

            return generalParams;
        }

        public override List<ColoredLine> GetTooltip(FilterResult filter) {
            var lines = new List<ColoredLine>()
                    .Add($"Map: {Name} T{MapTier}", ToolTipColor.GeneralGroup, 18)
                    .Add($"Tier: {MapTier} {MapTier + 68}iLvl", ToolTipColor.GeneralGroup)
                    .If(Unindentified, l => { l.Add($"Unindentified", ToolTipColor.Corrupted); })
                    .If(Corrupted, l => { l.Add($"Corrupted", ToolTipColor.Corrupted); });
            return lines;
        }

        public override string ToString() {
            return $"Map: {Name}{(Rarity == ItemRarity.Rare && Unindentified ? "(Unindentified Rare)" : "")} - T{MapTier} - {Price}";
        }
    }
}