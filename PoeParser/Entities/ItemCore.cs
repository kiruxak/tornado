using System.Collections.Generic;
using PoeParser.Common;
using PoeParser.Data;
using PoeParser.Entities.Affixes;
using PoeParser.Filter;

namespace PoeParser.Entities {
    public class Core {
        public ItemType Type;
        public Base Base;

        public Core(ItemType type, Base @base = null) {
            Type = type;
            Base = @base;
        }

        public Dictionary<string, string> GetPoeTradeAffixes(FilterResult filter) {
            Dictionary<string, string> affixes = Base == null ? new Dictionary<string, string>() : Base.GetPoeTradeAffixes(filter);

            affixes.Add("league", Config.LeagueName);
            affixes.Add("type", EnumInfo<ItemType>.Current[filter.Item.Type].DisplayName);
            if (filter.Item.Links > 0) { affixes.Add("link_min", filter.Item.Links.ToString()); }

            return affixes;
        }

        public List<IAffixValue> GetAffixes(Item item) {
            return Base?.GetAffixes(item) ?? new List<IAffixValue>();
        }

        public Dictionary<string, TotalAffixRecord> GetTotalAffixes(Item item) {
            return Base?.GetTotalAffixes(item) ?? new Dictionary<string, TotalAffixRecord>();
        }
    }
}