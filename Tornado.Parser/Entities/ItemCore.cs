using System.Collections.Generic;
using PoeParser.Common;
using Tornado.Common.Utility;
using Tornado.Parser.Data;
using Tornado.Parser.Entities.Affixes;
using Tornado.Parser.Filter;

namespace Tornado.Parser.Entities {
    public class Core {
        public ItemType Type;
        public Base Base;

        public Core(ItemType type, Base @base = null) {
            Type = type;
            Base = @base;
        }

        public NiceDictionary<string, string> GetPoeTradeAffixes(FilterResult filter) {
            NiceDictionary<string, string> affixes = Base == null ? new NiceDictionary<string, string>() : Base.GetPoeTradeAffixes(filter);

            affixes.Add("league", Config.LeagueName);
            affixes.Add("type", EnumInfo<ItemType>.Current[filter.Item.Type].DisplayName);
            if (filter.Item.Links > 0) {
                affixes.Add("link_min", filter.Item.Links.ToString());
            }

            return affixes;
        }

        public List<IAffixValue> GetAffixes(Item item) {
            return Base?.GetAffixes(item) ?? new List<IAffixValue>();
        }

        public NiceDictionary<string, TotalAffixRecord> GetTotalAffixes(Item item) {
            return Base?.GetTotalAffixes(item) ?? new NiceDictionary<string, TotalAffixRecord>();
        }
    }
}