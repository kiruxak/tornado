﻿using System.Collections.Generic;
using System.Linq;
using Tornado.Parser.PoeTrade.Extensions;

namespace Tornado.Parser.PoeTrade.Data {
    public class PoeItemData {
        public static List<PoeItemData> Parse(string source) {
            List<PoeItemData> result = new List<PoeItemData>();

            List<string> sellingLong = new List<string>(source.GetAllMatches("<span class=\"found-time-ago\">([\\w.\\s]*)</span>").Select(s => {
                if (s.IndexOf("found-time-ago") > 0) {
                    return "∞";
                }
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
                result.Add(new PoeItemData("No data"));
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

        public PoeItemData(Currency currency, string sellingTime) {
            Currency = currency;
            SellingTime = AdjustSellingTime(sellingTime);
        }

        private string AdjustSellingTime(string sellingTime) {
            return sellingTime.Replace(" weeks", "w").Replace(" week", "w")
                              .Replace(" days", "d").Replace(" day", "d").Replace("yesterday", "1d")
                              .Replace(" hours", "h").Replace(" hour", "h")
                              .Replace(" minutes", "m").Replace(" minute", "m");
        }

        public PoeItemData(string message) {
            Message = message;
            Currency = new Currency("0 chaos");
        }

        public override string ToString() {
            if (string.IsNullOrEmpty(Message)) {
                return $"{Currency}";
            }
            return Currency == null ? "∞" : Message;
        }
    }
}