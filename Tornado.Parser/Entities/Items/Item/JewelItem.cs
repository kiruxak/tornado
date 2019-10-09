using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;
using PoeParser.Common;
using PoeParser.Filter.Tooltip;
using Tornado.Common.Utility;
using Tornado.Parser.Common.Extensions;
using Tornado.Parser.Filter;
using Tornado.Parser.Filter.Tooltip;
using Tornado.Parser.PoeTrade.Response;

namespace Tornado.Parser.Entities {
    public class JewelItem : Item {
        public List<TradeAffix> JewelAffixes = new List<TradeAffix>();

        public JewelItem(string source, ItemRarity rarity, Core core) : base(source, rarity, core, null, core.BaseName, false) {
            Color = ToolTipColor.Gem;
            CheckIfAbyssJewel(source);
        }

        private void CheckIfAbyssJewel(string source) {
            var parts = Regex.Split(source, "--------");
            var correlationIndex = 2;
            if (source.Contains("Note:")) {
                correlationIndex += 1;
            }
            var affixes = Regex.Split(AffixSource, "\r\n");
            foreach (string s in affixes) {
                if (s.Contains("Spell") || s.Contains("Attack") || s.Contains("Critical") || s.Contains("Cast") || s.Contains("Multiplier") || s.Contains("Damage")) {
                    if (s.Contains("Dodge") || s.Contains("Block") || s.Contains("Move") || s.Contains("Blind") || s.Contains("Reduction") || s.Contains("Bleed") || s.Contains("gained") || s.Contains("Taunt")) { continue; }
                    var replaced = Regex.Replace(s, @" (\d+) ", " # ").Replace(" (fractured)", "");
                    if (replaced.Contains("#")) {
                        JewelAffixes.Add(new TradeAffix {
                            Tooltip = replaced, PoeTradeName = replaced, Value = s.ParseSumDouble(new Regex("(\\d+) to (\\d+)"), 0), IsTotal = false
                        });
                    } else {
                        replaced = Regex.Replace(replaced, @"([\d.]+)", "#");
                        JewelAffixes.Add(new TradeAffix {
                            Tooltip = replaced, PoeTradeName = replaced, Value = s.ParseSum("([\\d.]+)", 0), IsTotal = false
                        });
                    }
                }
                if (s.Contains("maximum Energy Shield") || s.Contains("maximum Life")) {
                    var replaced = Regex.Replace(s, @"(\d+)", "#");
                    JewelAffixes.Add(new TradeAffix { Tooltip = "!"+replaced, PoeTradeName = replaced, Value = s.ParseSum("(\\d+)", 0), IsTotal = false });
                }
            }
        }

        public override NiceDictionary<string, string> GetPoeTradeAffixes(FilterResult filter) {
            NiceDictionary<string, string> generalParams = new NiceDictionary<string, string>();
            generalParams.Add("league", Config.LeagueName);
            generalParams.Add("type", "Jewel");
            if (Rarity == ItemRarity.Unique) generalParams.Add("name", HttpUtility.UrlEncode(Name));

            return generalParams;
        }

        public override List<ColoredLine> GetTooltip(FilterResult filter) {
            var lines = new List<ColoredLine>();
            var size = 16;
            var survivalColor = "FFF2E920";
            var damageColor = "FFF25820";
            lines.Add(new ColoredLine($"Jewel: {Name}", ToolTipColor.GeneralGroup, 18));
            lines.AddEmptyLine();
            foreach (TradeAffix affix in JewelAffixes) {
                var color = affix.Tooltip.StartsWith("!") ? survivalColor : damageColor;
                var tooltip = affix.Tooltip.Replace("!", "");
                var prefix = tooltip.Contains("%") ? "%" : "";
                var starts = tooltip.StartsWith("+") ? "+" : "";
                tooltip = tooltip.Replace("%", "").Replace("+", "");
                lines.Add(new ColoredLine(new List<ColoredText>().Add(starts + affix.Value.ToString("0.#") + prefix, ToolTipColor.GeneralGroup, size + 1)
                                                                 .Add(' ' + tooltip, color, size)));
            }
            DrawSynthesisAffixes(lines);
            if (Type == ItemType.AbyssalJewel && ItemLevel >= 80) {
                lines.Add($"Item Level: {ItemLevel}", "FFF25820", 16);
            }
            return lines;
        }

        public override string ToString() {
            return $"{EnumInfo<ItemType>.Current[Type].DisplayName}: {Name} - {Price}";
        }
    }
}