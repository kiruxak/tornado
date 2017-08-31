using System.Collections.Generic;
using System.Linq;

namespace PoeParser.Entities.Affixes {
    public class Affix {
        public string Name { get; set; }
        public string RegexPattern { get; set; }

        public AffixType Type { get; set; }
        public List<AffixTier> Tiers { get; set; } = new List<AffixTier>();

        public AffixTier GetTier(double val, ItemType type) {
            var tier = Tiers.OrderBy(t => t.MinValue).LastOrDefault(t => val >= t.MinValue && t.ForTypes.Contains(type) && !t.Craft);
            if (tier != null) tier.Hybrid = tier.MaxValue < val;

            return tier;
        }

        public AffixTier GetCraftTier(double val, ItemType type) {
            if (Type == AffixType.Prefix) {
                return Tiers.Where(t => t.Craft && t.ForTypes.Contains(type)).OrderByDescending(t => t.MinValue).LastOrDefault(t => val >= t.MinValue && val <= t.MaxValue);
            }
            return Tiers.Where(t => t.Craft && t.ForTypes.Contains(type)).OrderByDescending(t => t.MinValue).Take(1).LastOrDefault(t => val >= t.MinValue && val <= t.MaxValue);
        }
    }

    public class AffixTier {
        public int Tier { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public int LevelRequired { get; set; }
        public bool Hybrid { get; set; } = false;
        public bool Craft { get; set; }
        public List<ItemType> ForTypes { get; set; } = new List<ItemType>();
    }
}