﻿using System.Collections.Generic;
using System.Linq;
using PoeParser.Common.Extensions;
using PoeParser.Filter.Tooltip;
using PoeParser.Properties;

namespace PoeParser.PoeTradeIntegration {
    public class PoeItemData {
        public static List<PoeItemData> Parse(string source) {
            List<PoeItemData> result = new List<PoeItemData>();

            List<string> sellingLong = new List<string>(source.GetAllMatches("<span class=\"found-time-ago\">([\\w.\\s]*)</span>").Select(s => {
                if (s.IndexOf("found-time-ago") > 0) { return "∞"; }
                var i = s.IndexOf("ago");
                var strings = (i > 0 ? s.Substring(0, i - 1) : s).Split(' ');

                if (strings.Length == 1) {
                    return $"{strings[0].ToLowerInvariant()}";
                }
                return $"{(strings[0] == "an" ? "1" : strings[0])} {strings[1]}";

            }));
            List<string> prices = source.GetAllMatches("data-buyout=\"([\\w.\\s]+)\"");

            List<Currency> currencies = new List<Currency>();
            foreach (var price in prices) {
                currencies.Add(new Currency(price));
            }

            if (prices.Count == 0) {
                result.Add(new PoeItemData(Resources.ItemsNotFound));
            } else {
                for (int i = 0; i < prices.Count; i++) {
                    result.Add(new PoeItemData(currencies[i], sellingLong[i]));
                }
            }

            return result.OrderBy(d => d.Currency.ValueInChaos).ToList();
        }

        public Currency Currency { get; set; }
        public string Message { get; set; } = "";
        public string SellingTime { get; set; } = "";
        public bool Expensive { get; }

        public PoeItemData(Currency currency, string sellingTime) {
            Currency = currency;
            SellingTime = sellingTime;
            Expensive = Currency?.ValueInChaos > Config.MinPrice;
        }

        public PoeItemData(string message) {
            Message = message;
            Currency = new Currency("0 chaos");
            Expensive = message.Equals(Resources.ItemsNotFound);
        }

        public override string ToString() {
            if (string.IsNullOrEmpty(Message)) {
                return $"{Currency}";
            }
            return Expensive ? "∞" : Message;
        }

        public List<ColoredText> GetPriceColoredLines(string color, int size, bool showTime = true) {
            if (string.IsNullOrEmpty(Message)) {
                return new List<ColoredText>()
                        .Add($"{Currency}", color, size)
                        .Add($"{(showTime ? "(" + SellingTime + ")" : "")}", ToolTipColor.GeneralGroup, size - 2, heightOffset: 1);
            }

            return new List<ColoredText>().Add(Expensive ? "∞" : Message, color, size);
        }
    }
}