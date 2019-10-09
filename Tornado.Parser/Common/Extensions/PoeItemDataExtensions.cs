using System.Collections.Generic;
using PoeParser.Filter.Tooltip;
using Tornado.Parser.Filter.Tooltip;
using Tornado.Parser.PoeTrade.Data;

namespace Tornado.Parser.Common.Extensions {
    public static class PoeItemDataExtensions {
        public static List<ColoredText> GetPriceColoredLines(this PoeItemData data, string color, int size, bool showTime = true) {
            if (string.IsNullOrEmpty(data.Message)) {
                var chaosValue = data.Currency.Type != CurrencyType.chaos ? $",~{data.Currency.ValueInChaos}c" : "";
                
                var info = showTime ?
                        string.IsNullOrEmpty(data.SellingTime) && string.IsNullOrEmpty(chaosValue) ? "" : $"({data.SellingTime}{chaosValue})" :
                        $"({data.SellingTime})";

                var recently = info.Contains("m") || (info.Contains("h") && info.ParseTo(0, "(\\d+)h") < 10);
                size = recently ? size : size - 1;

                return new List<ColoredText>()
                        .Add($" {data.Currency}", color, size)
                        .Add($"{(info)}", recently ? ToolTipColor.Offering : ToolTipColor.GeneralGroup, size);
            }

            return new List<ColoredText>().Add(data.Message, color, size);
        }

        public static bool IsExpensive(this PoeItemData data) {
            return data.Currency?.ValueInChaos > Config.MinPrice;
        }
    }
}