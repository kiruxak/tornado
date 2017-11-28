using System.Collections.Generic;
using System.Linq;
using PoeParser.Filter.Tooltip;
using Tornado.Common.Utility;
using Tornado.Parser.Common.Extensions;
using Tornado.Parser.Filter;
using Tornado.Parser.Filter.Tooltip;
using Tornado.Parser.PoeTrade.Data;

namespace Tornado.Parser.Entities {
    public abstract class BaseItem : IItem {
        public string Source { get; set; }
        public string Color { get; set; } = ToolTipColor.GeneralGroup;

        public ItemRarity Rarity { get; set; }
        public ItemType Type { get; }
        public string Name { get; set; }
        public bool Corrupted { get; set; }
        public bool Unindentified { get; set; }
        public int Links { get; set; }
        public int ItemLevel { get; set; }
        public int Quality { get; set; }

        public virtual string CacheName => Name;

        public PoeItemData Price => Prices?.FirstOrDefault();
        public IReadOnlyCollection<PoeItemData> Prices { get; set; } = null;

        protected BaseItem(string source, ItemRarity rarity, ItemType type, string name) {
            Rarity = rarity;
            Type = type;
            Name = name;
            Source = source;
            Unindentified = source.ContainsPattern("Unidentified");
            Corrupted = source.ContainsPattern("Corrupted");
            ItemLevel = source.ParseTo(0, "Item Level: (\\d+)");
            Quality = source.ParseTo(0, "Quality: \\+(\\d+)%");

            Links = GetLinks();
        }

        private int GetLinks() {
            bool hasSockets = Source.ContainsPattern("(\\w-\\w-\\w-\\w-\\w-\\w)");
            if (hasSockets)
                return 6;
            hasSockets = Source.ContainsPattern("(\\w-\\w-\\w-\\w-\\w)");
            return hasSockets ? 5 : 0;
        }

        public abstract NiceDictionary<string, string> GetPoeTradeAffixes(FilterResult filter);
        public abstract List<ColoredLine> GetTooltip(FilterResult filter);
    }
}