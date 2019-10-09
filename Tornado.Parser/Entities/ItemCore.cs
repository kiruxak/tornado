using System.Collections.Generic;
using System.Linq;
using PoeParser.Common;
using Tornado.Common.Utility;
using Tornado.Parser.Data;
using Tornado.Parser.Entities.Affixes;
using Tornado.Parser.Filter;

namespace Tornado.Parser.Entities {
    public class Core {
        public Core(ItemType type, Mark mark, Base @base) {
            Type     = type;
            Mark     = mark;
            Base     = @base;
            BaseName = @base.Name;
        }

        public Core(ItemType type, Mark mark, string @base) {
            Type     = type;
            Mark     = mark;
            BaseName = @base;
        }

        public int Links { get; set; }
        public int ItemLevel { get; set; }
        public ItemType Type { get; set; }
        public Mark Mark { get; set; }
        public Base Base { get; set; }
        public string BaseName { get; set; }

        public bool IsGoodBase => (ItemTypes.RareBases.Contains(BaseName) || Links == 6 || ItemLevel >= 83 || Mark == Mark.Elder || Mark == Mark.Shaper);

        public NiceDictionary<string, string> GetPoeTradeAffixes(FilterResult filter) {
            NiceDictionary<string, string> affixes = Base == null ? new NiceDictionary<string, string>() : Base.GetPoeTradeAffixes(filter);

            affixes.Add("league", Config.LeagueName);
            affixes.Add("type", EnumInfo<ItemType>.Current[filter.Item.Type].DisplayName);
            if (filter.Item.Links > 0) {
                affixes.Add("link_min", filter.Item.Links.ToString());
            }

            return affixes;
        }

        public List<IAffixValue> GetAffixes(Item item) { return Base?.GetAffixes(item) ?? new List<IAffixValue>(); }

        public NiceDictionary<string, TotalAffixRecord> GetTotalAffixes(Item item) {
            return Base?.GetTotalAffixes(item) ?? new NiceDictionary<string, TotalAffixRecord>();
        }
    }
}