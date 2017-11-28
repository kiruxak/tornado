using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Tornado.Parser.Entities;
using Tornado.Parser.Filter;
using Tornado.Parser.PoeTrade.Data;
using Tornado.Parser.PoeTrade.Response;

namespace Tornado.Parser.PoeTrade {
    public static class PoeItemExtensions {
        public static void OpenPoeTrade(this FilterResult result) {
            var client = new PoeTradeClient();
            var affixes = result.FilteredAffixes.Values.SelectMany(f => f).Cast<ITradeAffix>().ToList();
            client.OpenPoeTrade(affixes, result.GetPoeTradeGeneralAffixes());
        }

        public static IReadOnlyCollection<PoeItemData> GetPoePrice(this FilterResult result) {
            List<PoeItemData> itemData = new List<PoeItemData>();

            if (result.Item.Type == ItemType.Jewel && result.Item.Rarity != ItemRarity.Unique) {
                itemData.Add(new PoeItemData("Jewel"));
                result.Item.Prices = itemData;
                return itemData;
            }

            if (result.Value <= 0.8 && result.Value != 0 && result.Item.Links < 5 && result.Item.Rarity != ItemRarity.Unique) {
                itemData.Add(new PoeItemData("Trash"));
                result.Item.Prices = itemData;
                return itemData;
            }

            if (result.Item.Prices != null && result.Item.Prices.Any(p => p.Message == "Items not found" || p.Currency.Value > 0)) {
                return result.Item.Prices;
            }

            try {
                var client = new PoeTradeClient();
                var affixes = result.FilteredAffixes.Values.SelectMany(f => f).Cast<ITradeAffix>().ToList();
                itemData = client.GetPrice(affixes, result.GetPoeTradeGeneralAffixes()).Result;
            } catch (WebException webException) {
                result.Item.Prices = new List<PoeItemData>() {
                    new PoeItemData(webException.Message)
                };
                itemData = new List<PoeItemData>();
                itemData.AddRange(GetPoePrice(result));
            } catch (Exception e) {
                itemData.Add(new PoeItemData(e.Message));
            }

            result.Item.Prices = itemData;
            return itemData;
        }
    }
}