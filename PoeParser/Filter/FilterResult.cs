using System.Collections.Generic;
using System.Linq;
using PoeParser.Common.Extensions;
using PoeParser.Data;
using PoeParser.Entities;
using PoeParser.Entities.Affixes;
using PoeParser.Filter.Nodes;
using PoeParser.Filter.Tooltip;

namespace PoeParser.Filter {
    public class FilterResult {
        public Dictionary<string, List<string>> Groups { get; }
        public BaseItem Item { get; }
        public double Value { get; }
        public ILogicResultNode Filter { get; }
        public Dictionary<FilterGroup, List<IAffixValue>> FilteredAffixes { get; }
        public string Color { get; set; }
        public int BorderWidth { get; } = 2;
        public List<Affix> PossiblyToCraft { get; set; } = new List<Affix>();

        public FilterResult(Dictionary<string, List<string>> groups,ILogicResultNode node, BaseItem item) {
            Groups = groups;
            Item = item;
            Filter = node;

            var filteredAffixes = Filter.GetAffixNodes();
            var valueAffixes = filteredAffixes.Where(a => !a.DisplayOnly).ToList();

            Value = valueAffixes.Sum(v => v.Value)/ valueAffixes.Count;

            FilteredAffixes = GetGroups(filteredAffixes);

            if (item is Item) {
                if (Value <= 0.7) { Color = "FFC22626"; }
                if (Value > 0.7) { Color = "FFDE9800"; }
                if (Value > 1) { Color = "FF00DE21"; }
                if (item.Rarity == ItemRarity.Unique) Color = item.Color;
            } else {
                Color = item.Color;
            }
        }

        private Dictionary<FilterGroup, List<IAffixValue>> GetGroups(List<IAffixResultNode> filteredAffixes) {
            Dictionary<FilterGroup, List<IAffixValue>> map = new Dictionary<FilterGroup, List<IAffixValue>>();

            Item item = Item as Item;
            if (item == null) { return map; }

            var g = filteredAffixes.Where(a => !a.DisplayOnly || a.Value >= 1).Select(s => s.Name).Distinct().ToList();

            Groups.Each(pair => {
                pair.Value.Each(affix => {
                    if (item.Affixes.ContainsKey(affix)) {
                        if (item.CanCraft) {
                            var tAffix = item.Affixes[affix] as TotalAffixValue;
                            if (tAffix != null) { AddPossibleCraft(item, tAffix.TotalAffix); }
                        }

                        if (g.Contains(affix)) {
                            if (map.ContainsKey(FilterManager.Groups[pair.Key])) {
                                map[FilterManager.Groups[pair.Key]].Add(item.Affixes[affix]);
                            } else {
                                map.Add(FilterManager.Groups[pair.Key], new List<IAffixValue>() {item.Affixes[affix]});
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
                });
            });

            return map;
        }

        private void AddPossibleCraft(Item item, TotalAffixRecord affix) {
            var matches = affix.MathExpression.GetAllMatches("([a-zA-Z_]+)");

            string[] baseMods = new[] {"mAR", "mAres", "mArev", "mEV", "mEves", "mES"};
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

        public Dictionary<string, string> GetPoeTradeGeneralAffixes() { return Item.GetPoeTradeAffixes(this); }
        public List<ColoredLine> GetTooltip() { return Item.GetTooltip(this); } 
    }
}