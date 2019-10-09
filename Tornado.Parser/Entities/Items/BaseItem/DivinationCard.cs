using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PoeParser.Common;
using PoeParser.Filter.Tooltip;
using Tornado.Common.Utility;
using Tornado.Parser.Filter;
using Tornado.Parser.Filter.Tooltip;
using Tornado.Parser.PoeTrade.Data;

namespace Tornado.Parser.Entities {
    public class DivinationCard : BaseItem {
        public DivinationCard(string source, string name) : base(source, ItemRarity.Normal, ItemType.DivinationCard, name) {
            Color = ToolTipColor.NormalItem;

            if (Currency.DivCardData != null) {
                var currency = Currency.DivCardData.lines.FirstOrDefault(x => Name == x.name);
                if (currency != null) {
                    Prices.Add("Ninja", new List<PoeItemData>() { new PoeItemData(new Currency($"{currency.chaosValue} chaos"), "") });
                }
            }
        }

        public override NiceDictionary<string, string> GetPoeTradeAffixes(FilterResult filter) {
            NiceDictionary<string, string> generalParams = new NiceDictionary<string, string>();
            generalParams.Add("league", Config.LeagueName);
            generalParams.Add("name", HttpUtility.UrlEncode(Name));
            return generalParams;
        }

        static IEnumerable<string> Split(string str, int chunkSize) {
            return Enumerable.Range(0, str.Length / chunkSize).Select(i => str.Substring(i * chunkSize, chunkSize));
        }

        static IEnumerable<string> ChunksUpto(string str, int maxChunkSize) {
            for (int i = 0; i < str.Length; i += maxChunkSize) yield return str.Substring(i, Math.Min(maxChunkSize, str.Length - i));
        }

        public override List<ColoredLine> GetTooltip(FilterResult filter) {
            var tooltip = new List<ColoredLine>().Add($"{EnumInfo<ItemType>.Current[Type].DisplayName}: {Name}", ToolTipColor.GeneralGroup, 18);
            if (PoeData.DivCards.ContainsKey(Name)) {
                var card = PoeData.DivCards[Name];

                tooltip.AddEmptyLine();
                if (card.Locations != "N/A") {
                    if (card.Locations.Length < 140) {
                        tooltip.Add($"Locations: {card.Locations}", ToolTipColor.GeneralGroup, 16);
                    } else {
                        var lines = ChunksUpto(card.Locations, 140);
                        tooltip.Add($"Locations:", ToolTipColor.GeneralGroup, 16);

                        foreach (var l in lines) {
                            tooltip.Add(l, ToolTipColor.GeneralGroup, 16);
                        }
                    }
                }
                if (card.Additional != "N/A") {
                    tooltip.Add($"Info: {card.Additional}", ToolTipColor.GeneralGroup, 16);
                }
            }

            return tooltip;
        }

        public override string ToString() {
            return $"{EnumInfo<ItemType>.Current[Type].DisplayName}: {Name} - {Price}";
        }
    }
}