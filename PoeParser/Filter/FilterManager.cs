using System.Collections.Generic;
using System.Linq;
using PoeParser.Data;
using PoeParser.Entities;
using PoeParser.Entities.Affixes;
using PoeParser.Filter.Nodes;
using PoeParser.Filter.Tooltip;
using PoeParser.Parser;

namespace PoeParser.Filter {
    public class FilterManager {
        public static FilterManager GetManager() {
            List<string> strings = FileReader.ReadToList(Config.PickitFileUrl);

            List<FilterGroup> groups = new List<FilterGroup>();
            List<FilterTree> filters = new List<FilterTree>();
            foreach (string line in strings) {
                if (string.IsNullOrEmpty(line) || line.StartsWith(";"))
                    continue;
                if (line.StartsWith("group")) {
                    groups.Add(new FilterGroup(line));
                } else {
                    filters.Add(FilterTreeFactory.BuildTree(line));
                }
            }

            return new FilterManager(groups, filters);
        }

        public Dictionary<ItemType, List<FilterTree>> Filters;
        public static Dictionary<string, FilterGroup> Groups = new Dictionary<string, FilterGroup>();

        protected FilterManager(List<FilterGroup> groups, List<FilterTree> filters) {
            Filters = filters.GroupBy(f => f.Type).ToDictionary(f => f.Key, f => f.ToList());
            Groups = groups.ToDictionary(g => g.Name, g => g);
        }

        public FilterResult Filter(BaseItem baseItem) {
            Item item = baseItem as Item;
            FilterResult filter = GetFilter(item);

            if (filter == null) {
                return new FilterResult(new Dictionary<string, List<string>>(), new ResultNode() { Value = 0 }, baseItem);
            }

            if (item != null && item.CanCraft) {
                Dictionary<string, double> craftedValues = new Dictionary<string, double>();

                foreach (Affix a in filter.PossiblyToCraft) {
                    var tier = a.Tiers.Where(t => t.Craft && t.ForTypes.Contains(filter.Item.Type)).OrderByDescending(t => t.MinValue).FirstOrDefault();
                    if (tier != null) {
                        IAffixValue affixValue = new AffixValue(tier, a, tier.MaxValue, true);

                        item.Affixes.Add(affixValue.Name, affixValue);
                        var cFilter = GetFilter(item);
                        if (cFilter != null) {
                            craftedValues.Add(affixValue.Name, cFilter.Value);
                        }
                        item.Affixes.Remove(affixValue.Name);
                    }
                }

                if (craftedValues.Count > 0) {
                    KeyValuePair<string, double> craftedValue = craftedValues.OrderByDescending(t => t.Value).FirstOrDefault();

                    var affixWinner = filter.PossiblyToCraft.First(a => a.Name == craftedValue.Key);
                    var tierWinner = affixWinner.Tiers.Where(t => t.Craft && t.ForTypes.Contains(filter.Item.Type)).OrderByDescending(t => t.MinValue).FirstOrDefault();

                    var winner = new AffixValue(tierWinner, affixWinner, tierWinner.MaxValue, true);
                    item.Affixes.Add(winner.Name, winner);
                    item.CraftAddedByFilter = true;
                    item.UpdateTotalAffixes();

                    var filterWinner = GetFilter(item);
                    if (filterWinner == null) {
                        return new FilterResult(new Dictionary<string, List<string>>(), new ResultNode() {Value = 0}, baseItem);
                    }
                    return filterWinner;
                }
            }

            if (filter.Item.Rarity == ItemRarity.Unique) {
                if (filter.Value < 1 && !filter.Item.Type.IsWeapon()) {
                    filter.FilteredAffixes.Clear();
                }
            }
            
            return filter;
        }

        private FilterResult GetFilter(Item item) {
            List<FilterResult> filters = new List<FilterResult>();
            if (item != null && (item.Rarity == ItemRarity.Rare || item.Rarity == ItemRarity.Unique)) {
                if (Filters.ContainsKey(item.Type)) filters.AddRange(Filters[item.Type].Select(f => f.Filter(item)));
                if (item.Type.Is1HWeapon() && Filters.ContainsKey(ItemType.Weapon1H)) filters.AddRange(Filters[ItemType.Weapon1H].Select(f => f.Filter(item)));
                if (item.Type.Is2HWeapon() && Filters.ContainsKey(ItemType.Weapon2H)) filters.AddRange(Filters[ItemType.Weapon2H].Select(f => f.Filter(item)));
            }
            return filters.OrderByDescending(f => f.Value).FirstOrDefault();
        }
    }
}