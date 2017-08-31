using System.Collections.Generic;
using System.Linq;
using PoeParser.Common.Extensions;
using PoeParser.Filter;
using PoeParser.Filter.Tooltip;

namespace PoeParser.Entities {
    public class MapItem: BaseItem {
        public int MapTier { get; set; }
        public override string CacheName => $"{Name}{(Rarity == ItemRarity.Rare&&Unindentified ? "Unidentified Rare":"")}";

        public MapItem(string source, ItemRarity rarity, string[] lines, int mapTier) : base(source, rarity, ItemType.Map, GetMapName(lines, rarity, source.ContainsPattern("Unidentified"))) {
            MapTier = mapTier;
            Color = ToolTipColor.GeneralGroup;
        }

        protected static string GetMapName(string[] source, ItemRarity rarity, bool unindentified) {
            if (rarity == ItemRarity.Normal) return source[1];
            if (rarity == ItemRarity.Magic) return ItemTypes.Maps.First(m => source[1].Contains(m + " ")) + " Map";
            if (unindentified) {
                return ItemTypes.Maps.First(m => source[1].Contains(m + " ")) + " Map";
            } 
            return source[2];
        }

        public override Dictionary<string, string> GetPoeTradeAffixes(FilterResult filter) {
            Dictionary<string,string> generalParams = new Dictionary<string, string>();
            generalParams.Add("league", Config.LeagueName);
            generalParams.Add("name", System.Web.HttpUtility.UrlEncode(Name));
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
                    .If(Unindentified, l => {
                        l.Add($"Unindentified", ToolTipColor.Corrupted);
                    })
                    .If(Corrupted, l => {
                        l.Add($"Corrupted", ToolTipColor.Corrupted);
                    });
            return lines;
        }

        public override string ToString() {
            return $"Map: {Name}{(Rarity==ItemRarity.Rare && Unindentified? "(Unindentified Rare)":"")} - T{MapTier} - {Price}";
        }
    }
}