using System.Collections.Generic;
using PoeParser.Filter.Tooltip;
using Tornado.Parser.Filter.Tooltip;
using Tornado.Parser.PoeTrade.Data;

namespace Tornado.Parser.Common.Extensions {
    public static class PoeItemDataExtensions {
        public static List<ColoredText> GetPriceColoredLines(this PoeItemData data, string color, int size, bool showTime = true) {
            if (string.IsNullOrEmpty(data.Message)) {
                return new List<ColoredText>()
                        .Add($"{data.Currency}", color, size)
                        .Add($"{(showTime ? "(" + data.SellingTime + ")" : "")}", ToolTipColor.GeneralGroup, size - 2, heightOffset: 1);
            }

            return new List<ColoredText>().Add( /*Expensive ? "∞" : Message*/ data.Message, color, size);
        }

        public static bool IsExpensive(this PoeItemData data) {
            return data.Currency?.ValueInChaos > Config.MinPrice;
        }
    }
}