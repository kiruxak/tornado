using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PoeParser.Entities;
using PoeParser.Entities.Affixes;
using PoeParser.Filter;

namespace PoeParser.Data {
    public abstract class Base {
        public string Name { get; set; }

        public static Dictionary<string, string> PoeGeneralMap = new Dictionary<string, string>() {
            { BuildInAffixNames.PDps, "pdps_min" },
            { BuildInAffixNames.EDps, "edps_min" },
            { BuildInAffixNames.Dps, "dps_min" },
            { BuildInAffixNames.WepCrit, "crit_min" },
            { BuildInAffixNames.Armour, "armour_min" },
            { BuildInAffixNames.Evasion, "evasion_min" },
            { BuildInAffixNames.EnergyShield, "shield_min" }
        };

        public Dictionary<string, string> GetPoeTradeAffixes(FilterResult filter) {
            Dictionary<string, string> general = new Dictionary<string, string>();

            Item item = filter.Item as Item;
            if (item == null) { return general; }

            foreach (var pair in GetTotalAffixes(item)) {
                AddPoeTradeAffix(general, item, filter, pair.Key);
            }

            return general;
        }

        private void AddPoeTradeAffix(Dictionary<string, string> general, Item item, FilterResult filter, string name) {
            IAffixValue affixValue = item.Affixes.ContainsKey(name) ? item.Affixes[name] : null;

            if (affixValue?.Value > 0 && filter.FilteredAffixes.Values.Any(a => a.Any(af => af.Name == name)))
                general.Add(PoeGeneralMap[name], (affixValue.Value * Config.PoeTradeMod).ToString("0.##", new CultureInfo("en-US")));
        }

        public List<IAffixValue> GetAffixes(Item item) {
            List<IAffixValue> affixes = new List<IAffixValue>();

            foreach (var pair in GetTotalAffixes(item)) {
                var totalAffixValue = pair.Value.GetValue(item);
                if (totalAffixValue.Value > 0) {
                    affixes.Add(totalAffixValue);
                }
            }

            return affixes;
        }

        public abstract Dictionary<string, TotalAffixRecord> GetTotalAffixes(Item item);
    }
}