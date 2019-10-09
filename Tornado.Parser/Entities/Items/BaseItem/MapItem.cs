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
        public int MapTier { get; }
        public override string CacheName => $"{Name}|{(Unindentified ? "Unidentified" : "")}|{Quality}|{Rarity}";

        public MapItem(string source, ItemRarity rarity, string[] lines, int mapTier) : base(source, rarity, ItemType.Map, GetMapName(lines, rarity, source.ContainsPattern("Unidentified"))) {
            MapTier = mapTier;
            Color = Rarity == ItemRarity.Unique ? ToolTipColor.UniqueGroup : ToolTipColor.GeneralGroup;
        }

        protected static string GetMapName(string[] source, ItemRarity rarity, bool unindentified) {
            if (rarity == ItemRarity.Unique) {
                if (unindentified) {
                    var mapName = source[1].Replace("Superior ", "");
                    return ItemTypes.UniqueMaps[mapName];
                }
                return source[1];
            }

            if (rarity == ItemRarity.Normal)
                return source[1];
            if (rarity == ItemRarity.Magic || unindentified)
                return ItemTypes.Maps.First(m => source[1].Contains(m + " ")) + " Map";
            
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
                    .Add($"Map: {Name} T{MapTier}", Rarity == ItemRarity.Unique ? ToolTipColor.UniqueGroup : ToolTipColor.GeneralGroup, 18)
                    .If(Rarity != ItemRarity.Unique, l => l.Add($"Tier: {MapTier} {MapTier + 68}iLvl", ToolTipColor.GeneralGroup))
                    .If(Unindentified, l => { l.Add($"Unindentified", ToolTipColor.Corrupted); })
                    .If(Corrupted, l => { l.Add($"Corrupted", ToolTipColor.Corrupted); });
            return lines;
        }

        public override string ToString() {
            return $"Map: {Name}|{(Unindentified ? "(Unindentified)" : "")}|{Quality} - T{MapTier} - {Price}";
        }
    }
}