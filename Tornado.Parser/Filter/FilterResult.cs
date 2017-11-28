using System.Collections.Generic;
using System.Linq;
using Tornado.Common.Utility;
using Tornado.Parser.Common.Extensions;
using Tornado.Parser.Data;
using Tornado.Parser.Entities;
using Tornado.Parser.Entities.Affixes;
using Tornado.Parser.Filter.Nodes;
using Tornado.Parser.Filter.Tooltip;

namespace Tornado.Parser.Filter {
    public class FilterResult {
        public NiceDictionary<string, List<string>> Groups { get; }
        public BaseItem Item { get; }
        public double Value { get; }
        public ILogicResultNode Filter { get; }
        public NiceDictionary<FilterGroup, List<IAffixValue>> FilteredAffixes { get; }
        public string Color { get; set; }
        public int BorderWidth { get; } = 2;
        public List<Affix> PossiblyToCraft { get; set; } = new List<Affix>();

        public FilterResult(NiceDictionary<string, List<string>> groups, ILogicResultNode node, BaseItem item) {
            Groups = groups;
            Item = item;
            Filter = node;

            var filteredAffixes = Filter.GetAffixNodes();
            var valueAffixes = filteredAffixes.Where(a => !a.DisplayOnly).ToList();

            Value = valueAffixes.Sum(v => v.Value) / valueAffixes.Count;

            FilteredAffixes = GetGroups(filteredAffixes);

            if (item is Item) {
                if (Value <= 0.7) {
                    Color = "FFC22626";
                }
                if (Value > 0.7) {
                    Color = "FFDE9800";
                }
                if (Value > 1) {
                    Color = "FF00DE21";
                }
                if (item.Rarity == ItemRarity.Unique)
                    Color = item.Color;
            } else {
                Color = item.Color;
            }
        }

        private NiceDictionary<FilterGroup, List<IAffixValue>> GetGroups(List<IAffixResultNode> filteredAffixes) {
            NiceDictionary<FilterGroup, List<IAffixValue>> map = new NiceDictionary<FilterGroup, List<IAffixValue>>();

            Item item = Item as Item;
            if (item == null) {
                return map;
            }

            var g = filteredAffixes.Where(a => !a.DisplayOnly || a.Value >= 1).Select(s => s.Name).Distinct().ToList();

            foreach (var pair in Groups) {
                foreach (string affix in pair.Value) {
                    if (item.Affixes.ContainsKey(affix)) {
                        if (item.CanCraft) {
                            var tAffix = item.Affixes[affix] as TotalAffixValue;
                            if (tAffix != null) {
                                AddPossibleCraft(item, tAffix.TotalAffix);
                            }
                        }

                        if (g.Contains(affix)) {
                            if (map.ContainsKey(FilterManager.Groups[pair.Key])) {
                                map[FilterManager.Groups[pair.Key]].Add(item.Affixes[affix]);
                            } else {
                                map.Add(FilterManager.Groups[pair.Key], new List<IAffixValue>() {
                                    item.Affixes[affix]
                                });
                            }
                        }
                    } else {
                        if (item.CanCraft) {
                            if (PoeData.Affixes.ContainsKey(affix) && PossiblyToCraft.All(a => a.Name != affix)) {
                                PossiblyToCraft.Add(PoeData.Affixes[affix]);
                            } else {
                                AddPossibleCraft(item, PoeData.TotalAffixes.ContainsKey(affix) ? PoeData.TotalAffixes[affix] : item.Core.GetTotalAffixes(item)[affix]);
                            }
                        }
                    }
                }
            }

            return map;
        }

        private void AddPossibleCraft(Item item, TotalAffixRecord affix) {
            var matches = affix.MathExpression.GetAllMatches("([a-zA-Z_]+)");

            string[] baseMods = new[] {
                "mAR",
                "mAres",
                "mArev",
                "mEV",
                "mEves",
                "mES"
            };
            bool containsBaseMod = baseMods.Any(b => item.Affixes.ContainsKey(b));

            foreach (string match in matches) {
                if (!item.Affixes.ContainsKey(match)) {
                    if (baseMods.Contains(match) && containsBaseMod)
                        continue;

                    var aff = PoeData.Affixes[match];
                    if ((aff.Type == AffixType.Prefix && item.CanCraftPrefix) || (aff.Type == AffixType.Suffix && item.CanCraftSuffix))
                        if (PossiblyToCraft.All(a => a.Name != aff.Name))
                            PossiblyToCraft.Add(aff);
                }
            }
        }

        public NiceDictionary<string, string> GetPoeTradeGeneralAffixes() {
            return Item.GetPoeTradeAffixes(this);
        }

        public List<ColoredLine> GetTooltip() {
            return Item.GetTooltip(this);
        }
    }
}