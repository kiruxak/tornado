using System.Collections.Generic;
using PoeParser.Filter;

namespace PoeParser.Entities {
    public interface IItem {
        ItemRarity Rarity { get; set; }
        ItemType Type { get; }

        string Name { get; set; }
        string Source { get; set; }

        int ItemLevel { get; set; }
        bool Corrupted { get; set; }
        bool Unindentified { get; set; }
        int Quality { get; set; }

        Dictionary<string, string> GetPoeTradeAffixes(FilterResult filter);
    }
}