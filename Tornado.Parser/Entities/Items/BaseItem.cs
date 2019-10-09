using System.Collections.Generic;
using System.Linq;
using PoeParser.Filter.Tooltip;
using Tornado.Common.Utility;
using Tornado.Parser.Common.Extensions;
using Tornado.Parser.Filter;
using Tornado.Parser.Filter.Tooltip;
using Tornado.Parser.PoeTrade.Data;
using Tornado.Parser.PoeTrade.Response;

namespace Tornado.Parser.Entities {
    public abstract class BaseItem : IItem {
        public static string PriceName = "Price";

        public string Source { get; set; }
        public string Color { get; set; } = ToolTipColor.GeneralGroup;

        public ItemRarity Rarity { get; set; }
        public ItemType Type { get; set; }
        public string Name { get; set; }
        public bool Corrupted { get; set; }
        public bool Mirrored { get; set; }
        public bool Unindentified { get; set; }
        public int Links { get; set; }
        public int ItemLevel { get; set; }
        public int Quality { get; set; }
        public List<TradeAffix> UniqueAffixes = new List<TradeAffix>();

        public virtual string CacheName => Name;

        public PoeItemData Price => Prices.ContainsKey(PriceName) ? Prices[PriceName]?.FirstOrDefault() : null;
        public NiceDictionary<string, IReadOnlyCollection<PoeItemData>> Prices { get; set; } = new NiceDictionary<string, IReadOnlyCollection<PoeItemData>>();

        protected BaseItem(string source, ItemRarity rarity, ItemType type, string name) {
            Rarity = rarity;
            Type = type;
            Name = name;
            Source = source;
            Unindentified = source.ContainsPattern("Unidentified");
            Corrupted = source.ContainsPattern("Corrupted");
            Mirrored = source.ContainsPattern("Mirrored");
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