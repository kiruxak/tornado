using Tornado.Common.Utility;
using Tornado.Parser.Filter;

namespace Tornado.Parser.Entities {
    public interface IItem {
        ItemRarity Rarity { get; set; }
        ItemType Type { get; }

        string Name { get; set; }
        string Source { get; set; }

        int ItemLevel { get; set; }
        bool Corrupted { get; set; }
        bool Unindentified { get; set; }
        int Quality { get; set; }

        NiceDictionary<string, string> GetPoeTradeAffixes(FilterResult filter);
    }
}