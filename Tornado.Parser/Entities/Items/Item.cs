using System;
using System.Collections.Generic;
using System.Linq;
using PoeParser.Filter.Tooltip;
using Tornado.Common.Utility;
using Tornado.Parser.Common.Extensions;
using Tornado.Parser.Data;
using Tornado.Parser.Entities.Affixes;
using Tornado.Parser.Filter;
using Tornado.Parser.Filter.Tooltip;

namespace Tornado.Parser.Entities {
    public abstract class Item : BaseItem {
        public Core Core { get; set; }
        public NiceDictionary<string, IAffixValue> Affixes { get; set; } = new NiceDictionary<string, IAffixValue>();

        private string AffixSource { get; }
        private string ImplicitSource { get; }

        //todo fix this
        public bool CanCraft => Config.ShowCraft && (PrefixesCount + SuffixesCount < 6) && (PrefixesCount < 3 || SuffixesCount < 3) && !Corrupted && !HasCraft && Rarity != ItemRarity.Unique;
        public bool CanCraftSuffix => SuffixesCount < 3 && CanCraft;
        public bool CanCraftPrefix => PrefixesCount < 3 && CanCraft;
        public bool HasCraft { get; private set; }
        public bool CraftAddedByFilter { get; set; } = false;

        public int PrefixesCount => Affixes.Values.Count(a => a.Type == AffixType.Prefix) + Affixes.Values.OfType<AffixValue>().Count(a => a.Type == AffixType.Prefix && a.AffixTier.Hybrid);
        public int SuffixesCount => Affixes.Values.Count(a => a.Type == AffixType.Suffix) + Affixes.Values.OfType<AffixValue>().Count(a => a.Type == AffixType.Suffix && a.AffixTier.Hybrid);

        protected Item(string source, ItemRarity rarity, Core core, string name) : base(source, rarity, core.Type, name) {
            Core = core;

            var parts = source.Split(new[] { "--------" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (parts.Last().ContainsPattern("\r\nMirrored")) 
                parts.RemoveAt(parts.Count - 1); // Mirrored
            if (parts.Last().ContainsPattern("Note:"))
                parts.RemoveAt(parts.Count - 1); // Note with b/o
            if (parts.Last().ContainsPattern("\r\nCorrupted"))
                parts.RemoveAt(parts.Count - 1); // Corrupted
            if (parts.Last().ContainsPattern("\r\nHas "))
                parts.RemoveAt(parts.Count - 1); // Effect

            if (rarity == ItemRarity.Unique) {
                AffixSource = parts[parts.Count - 2];
                ImplicitSource = parts[parts.Count - 3];
            } else {
                AffixSource = parts.Last();
                ImplicitSource = parts[parts.Count - 2];
            }

            GetAffixes(AffixSource, PoeData.Affixes.Values.Where(x => x.Type == AffixType.Suffix || x.Type == AffixType.Prefix));
            GetAffixes(ImplicitSource, PoeData.Affixes.Values.Where(x => x.Type == AffixType.Implicit));
            FixRarity();

            GetTotalAffixes();
        }

        public void UpdateTotalAffixes() {
            foreach (TotalAffixValue affix in Affixes.Values.OfType<TotalAffixValue>().ToList()) {
                TotalAffixValue affi = affix.TotalAffix.GetValue(this);

                if (Affixes.ContainsKey(affix.Name)) {
                    Affixes.Remove(affix.Name);
                }
                if (affi.Value > 0) {
                    Affixes.Add(affi.Name, affi);
                }
            }
            foreach (TotalAffixRecord affix in PoeData.TotalAffixes.Values.ToList()) {
                TotalAffixValue affi = affix.GetValue(this);

                if (Affixes.ContainsKey(affix.Name)) {
                    Affixes.Remove(affix.Name);
                }
                if (affi.Value > 0) {
                    Affixes.Add(affi.Name, affi);
                }
            }
        }

        private void FixRarity() {
            if (Affixes.ContainsKey("pRarity")) {
                if (PrefixesCount > 3 && SuffixesCount <= 3) {
                    Affixes.Remove("pRarity");
                } else {
                    Affixes.Remove("sRarity");
                }
            }
        }

        private void GetTotalAffixes() {
            foreach (TotalAffixRecord affix in PoeData.TotalAffixes.Values) {
                TotalAffixValue affi = affix.GetValue(this);
                if (affi.Value > 0) {
                    Affixes.Add(affi.Name, affi);
                }
            }
        }

        private void GetAffixes(string val, IEnumerable<Affix> affixes) {
            var lastAffix = val.Split(new[] {
                "\r\n"
            }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            foreach (Affix affix in affixes) {
                double affixValue = affix.RegexPattern.Contains("(\\d+) to (\\d+)") ? val.ParseDualAverage(affix.RegexPattern + "\r\n") : val.ParseSum(affix.RegexPattern + "\r\n");
                if (affixValue <= 0)
                    continue;

                AffixTier t = affix.GetTier(affixValue, Type);
                if (t != null) {
                    bool canBeCraft = lastAffix.ContainsPattern(affix.RegexPattern) && affix.Type != AffixType.Implicit;
                    if (canBeCraft) {
                        var craftTier = affix.GetCraftTier(affixValue, Type);
                        if (craftTier != null) {
                            HasCraft = true;
                            Affixes.Add(affix.Name, new AffixValue(craftTier, affix, affixValue, true));
                        } else {
                            Affixes.Add(affix.Name, new AffixValue(t, affix, affixValue, false));
                        }
                    } else {
                        Affixes.Add(affix.Name, new AffixValue(t, affix, affixValue, false));
                    }
                }
            }
        }

        public override List<ColoredLine> GetTooltip(FilterResult filter) {
            var lines = new List<ColoredLine>();
            var line = new ColoredLine();

            var uniqueType = $"{(Core.Type == ItemType.Map ? "Map:" : Core.Type == ItemType.Jewel ? "Jewel:" : Core.Type == ItemType.Flask ? "Flask" : "Unique:")} {Name}";

            line.Add(filter.Item.Rarity == ItemRarity.Unique ? uniqueType : $"{Type} P{Affixes.Count(a => a.Value.Type == AffixType.Prefix)} S{Affixes.Count(a => a.Value.Type == AffixType.Suffix)} -", Color, 16);

            if (filter.Item.Rarity != ItemRarity.Unique) {
                line.Add($" {(filter.Value * 100).ToString("F0")}%", filter.Color, 16);
                line.Add($" {(HasCraft ? "HasCraft" : "")}", ToolTipColor.Gem, 12, heightOffset: 2);
            }
            lines.Add(line);
            lines.AddEmptyLine()
                 .If(Core.Type == ItemType.Flask && filter.Item.Rarity == ItemRarity.Unique, l => { //if unique flask
                     l.Add($"Quality: {Quality}", ToolTipColor.GeneralGroup, 16);
                 })
                 .If(Links > 0, l => { //if 5 or 6 links
                     l.Add($"Links: {Links}", ToolTipColor.Corrupted, 16);
                 });

            var groupMaps = filter.FilteredAffixes;
            foreach (KeyValuePair<FilterGroup, List<IAffixValue>> pair in groupMaps.OrderBy(g => g.Key.Order)) {
                string color = FilterManager.Groups[pair.Key.Name].Color;

                foreach (IAffixValue a in pair.Value.OrderBy(a => a.RegexPattern)) {
                    var total = a as TotalAffixValue;
                    if (total != null) {
                        lines.Add(total.GetTooltipLine(color, 16));
                    } else {
                        lines.Add(a.ToString(), color, 16);
                    }
                }
            }

            if (Config.Debug) {
                lines.Add($"DEBUG", "FFFF454E", 18);

                foreach (var a in Affixes.Values.OfType<AffixValue>().OrderBy(a => a.Type)) {
                    string type = a.Type == AffixType.Implicit ? "I" : a.Type == AffixType.Prefix ? "P" : "S";
                    string color = a.Type == AffixType.Implicit ? "FFFFB735" : a.Type == AffixType.Prefix ? "FF458FFF" : "FF45FF5C";
                    lines.Add($"{(type)}{a.AffixTier.Tier}  {a.Name}{(a.IsCraft ? $"[C][{type}]" : "")}: {a.Value}/{a.MaxValue}  {a.AffixTier.MinValue}/{a.AffixTier.MaxValue}", a.IsCraft ? "FFA223EF" : color);
                }

                lines.AddEmptyLine()
                     .Add("Posible to craft", "FFFF454E", 18);

                foreach (var a in filter.PossiblyToCraft.OrderBy(a => a.Type)) {
                    string color = a.Type == AffixType.Implicit ? "FFFFB735" : a.Type == AffixType.Prefix ? "FF458FFF" : "FF45FF5C";
                    var tier = a.Tiers.Where(t => t.Craft && t.ForTypes.Contains(filter.Item.Type)).OrderByDescending(t => t.MinValue).FirstOrDefault();

                    if (tier != null)
                        lines.Add($"{a.Name}[{(a.Type == AffixType.Prefix ? "P" : "S")}]: {tier.MinValue}/{tier.MaxValue}", color);
                }
            }
            return lines;
        }
    }
}