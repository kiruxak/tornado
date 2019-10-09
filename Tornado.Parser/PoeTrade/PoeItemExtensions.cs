using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using PoeParser.Common;
using Tornado.Common.Utility;
using Tornado.Parser.Common.Extensions;
using Tornado.Parser.Entities;
using Tornado.Parser.Entities.Affixes;
using Tornado.Parser.Filter;
using Tornado.Parser.Filter.Nodes;
using Tornado.Parser.PoeTrade.Data;
using Tornado.Parser.PoeTrade.Response;

namespace Tornado.Parser.PoeTrade {
    public static class PoeItemExtensions {
        public static void OpenPoeTrade(this FilterResult result) {
            var client = new PoeTradeClient();
            var affixes = result.FilteredAffixes.Values.SelectMany(f => f).Cast<ITradeAffix>().ToList();
            affixes.AddRange(result.Item.UniqueAffixes);
            PopulateWithUniqueAffixes(result, affixes);
            client.OpenPoeTrade(affixes, result.GetPoeTradeGeneralAffixes());
        }

        public static void OpenBasePoeTrade(this FilterResult result) {
            var client = new PoeTradeClient();
            if (result.Item is Item baseItem) {
                client.OpenPoeTrade(new List<ITradeAffix>(), GetBaseQueryAffixes(result, baseItem));
            }
        }

        public static void OpenSyncPoeTrade(this FilterResult result) {
            var client = new PoeTradeClient();
            if (result.Item is Item baseItem) {
                NiceDictionary<string, string> baseAffixes = GetBaseQueryAffixes(result, baseItem);

                if (baseItem.Core?.Mark == Mark.Synthesised) {
                    var affixes = new List<ITradeAffix>();
                    var implicits = baseItem.ImplicitSource.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string @implicit in implicits) {
                        var poeTradeName = "(implicit) " + @implicit;
                        poeTradeName = Regex.Replace(poeTradeName, @"([\d.]+)", "#");
                        var affix = new TradeAffix() { IsTotal = false, Value = baseItem.ImplicitSource.ParseTo(0, @"(\d+)"), PoeTradeName = poeTradeName };
                        affixes.Add(affix);

                    }
                    client.OpenPoeTrade(affixes, baseAffixes);
                }
                if (baseItem.Core?.Mark == Mark.Fractured) {
                    List<AffixValue> affixes = baseItem.Affixes.Values.OfType<AffixValue>().Where(x => x.IsFractured).ToList();
                    List<ITradeAffix> tradeAffixes = new List<ITradeAffix>();
                    foreach (var affix in affixes) {
                        TradeAffix tradeAffix = new TradeAffix() {
                            PoeTradeName = "(fractured) " + affix.PoeTradeName,
                            Value        = affix.AffixTier.MinValue,
                            IsTotal      = false
                        };
                        tradeAffixes.Add(tradeAffix);
                    }
                    client.OpenPoeTrade(tradeAffixes, baseAffixes);
                }
            }
        }

        public static IReadOnlyCollection<PoeItemData> GetPoePrice(this FilterResult result) {
            List<PoeItemData> itemData = new List<PoeItemData>();

            if (result.Item is CurrencyItem) {
                itemData = GetPrice(result, new List<ITradeAffix>(), result.GetPoeTradeGeneralAffixes());
                result.Item.Prices.Add(BaseItem.PriceName, itemData);

                //itemData.Add(new PoeItemData(""));
                //result.Item.Prices.Add(BaseItem.PriceName, itemData);
                return itemData;
            }

            if (result.Item.Type == ItemType.Helmet && result.Item is Item helmet) {
                if (!helmet.Enchant.Contains("\r\n")) {  
                    var helmetAffix = new TradeAffix() {
                        IsTotal      = false,
                        PoeTradeName = "(enchant) " + Regex.Replace(helmet.Enchant, @"([\d.]+)", "#"),
                        Value        = helmet.Enchant.ParseSum("([\\d.]+)", 0)
                    };

                    var baseAffixes = result.GetPoeTradeGeneralAffixes();
                    if (baseAffixes.ContainsKey("armour_min")) { baseAffixes.Remove("armour_min"); }
                    if (baseAffixes.ContainsKey("evasion_min")) { baseAffixes.Remove("evasion_min"); }
                    if (baseAffixes.ContainsKey("shield_min")) { baseAffixes.Remove("shield_min"); }

                    itemData = GetPrice(result, new List<ITradeAffix>() { helmetAffix }, new NiceDictionary<string, string>() { { "league", baseAffixes["league"] } });
                    result.Item.Prices.Add($"Price [{Regex.Replace(helmet.Enchant, @"([\d.]+)", "#")}]", itemData);

                    itemData = GetPrice(result, new List<ITradeAffix>() { helmetAffix }, baseAffixes);
                    result.Item.Prices.Add($"Price [Base & {Regex.Replace(helmet.Enchant, @"([\d.]+)", "#")}]", itemData);
                }
            }

            if (result.Item.Type == ItemType.Jewel && result.Item.Rarity != ItemRarity.Unique) {
                if (result.Item is JewelItem jewelItem && (jewelItem.JewelAffixes.Count > 0)) {
                    //go to check price
                } else {
                    itemData.Add(new PoeItemData("Jewel"));
                    result.Item.Prices.Add(BaseItem.PriceName, itemData);
                    return itemData;
                }
            }

            if (result.Value <= 0.65 && result.Value != 0 && result.Item.Links < 5 && result.Item.Rarity != ItemRarity.Unique && result.Item.UniqueAffixes.Count == 0) {
                itemData.Add(new PoeItemData("Trash"));
                result.Item.Prices.Add(BaseItem.PriceName, itemData);
                return itemData;
            }

            if (result.Item.Prices.ContainsKey(BaseItem.PriceName) && result.Item.Prices[BaseItem.PriceName].Any(p => p.Message == "Items not found" || p.Currency.Value > 0)) {
                return result.Item.Prices[BaseItem.PriceName];
            }

            var affixes = result.FilteredAffixes.Values.SelectMany(f => f).Cast<ITradeAffix>().ToList();
            
            affixes.AddRange(result.Item.UniqueAffixes);
            PopulateWithUniqueAffixes(result, affixes, true);
            itemData = GetPrice(result, affixes, result.GetPoeTradeGeneralAffixes());

            result.Item.Prices.Add(BaseItem.PriceName, itemData);

            if (!(result.Item is UniqueItem)) {
                var baseAffixes = result.GetPoeTradeGeneralAffixes();
                var valueNodes = result.Filter.GetAffixNodes();

                var required = valueNodes.Where(a => !a.DisplayOnly).Select(s => s.Name).Distinct().ToList();
                var optional = valueNodes.Where(a => a.DisplayOnly && a.Value >= 1).Select(s => s.Name).Distinct().ToList().Except(required).ToList();
                var group = valueNodes.Where(a => a.DisplayOnly && a.Value >= 1).Select(s => new {s.Name, s.Group}).ToList();
                var utility = group.Where(x => x.Group == "U").Select(x => x.Name).Distinct().Except(required).ToList();
                var survive = group.Where(x => x.Group == "S").Select(x => x.Name).Distinct().Except(required).ToList();
                var damage = group.Where(x => x.Group == "D").Select(x => x.Name).Distinct().Except(required).ToList();

                if (optional.Contains("AR")) { baseAffixes.Remove("armour_min"); }
                if (optional.Contains("EV")) { baseAffixes.Remove("evasion_min"); }
                if (optional.Contains("ES")) { baseAffixes.Remove("shield_min"); }
                if (optional.Contains("flatES")) optional.Remove("flatES"); 
                if (optional.Contains("life")) optional.Remove("life");

                if (optional.Count != 0) {
                    if (utility.Count > 0 && (survive.Count > 0 || damage.Count > 0)) {
                        var noUtility = affixes.OfType<TotalAffixValue>().Where(x => !utility.Contains(x.Name)).Cast<ITradeAffix>().ToList();
                        noUtility.AddRange(result.Item.UniqueAffixes);
                        itemData = GetPrice(result, noUtility, baseAffixes);
                        result.Item.Prices.Add($"Price Ignore[{string.Join(", ", utility)}]", itemData);
                    }
                    if (survive.Count > 0 && damage.Count > 0) {
                        var noDamage = affixes.OfType<TotalAffixValue>().Where(x => !damage.Contains(x.Name)).Cast<ITradeAffix>().ToList();
                        var noSurvive = affixes.OfType<TotalAffixValue>().Where(x => !survive.Contains(x.Name)).Cast<ITradeAffix>().ToList();
                        noDamage.AddRange(result.Item.UniqueAffixes);
                        noSurvive.AddRange(result.Item.UniqueAffixes);
                        
                        itemData = GetPrice(result, noDamage, baseAffixes);
                        result.Item.Prices.Add($"Price Ignore[{string.Join(", ", damage)}]", itemData);

                        itemData = GetPrice(result, noSurvive, baseAffixes);
                        result.Item.Prices.Add($"Price Ignore[{string.Join(", ", survive)}]", itemData);
                    }
                    if (utility.Count > 0 && survive.Count > 0 && damage.Count > 0) {
                        var noUtilityAndSurvive = affixes.OfType<TotalAffixValue>().Where(x => !utility.Contains(x.Name) && !survive.Contains(x.Name)).Cast<ITradeAffix>().ToList();
                        var noUtilityAndDamage = affixes.OfType<TotalAffixValue>().Where(x => !utility.Contains(x.Name) && !damage.Contains(x.Name)).Cast<ITradeAffix>().ToList();

                        itemData = GetPrice(result, noUtilityAndSurvive, baseAffixes);
                        result.Item.Prices.Add($"Price Ignore[Unique,{string.Join(", ", utility)},{string.Join(", ", survive)}]", itemData);

                        itemData = GetPrice(result, noUtilityAndDamage, baseAffixes);
                        result.Item.Prices.Add($"Price Ignore[Unique,{string.Join(", ", utility)},{string.Join(", ", damage)}]", itemData);
                    }

                    var noOptionalAffixes = affixes.OfType<TotalAffixValue>().Where(x => !optional.Contains(x.Name)).Cast<ITradeAffix>().ToList();
                    noOptionalAffixes.AddRange(result.Item.UniqueAffixes);
                    itemData = GetPrice(result, noOptionalAffixes, baseAffixes);
                    result.Item.Prices.Add($"Price Ignore[{string.Join(", ", optional)}]", itemData);
                }
                if (result.Item.UniqueAffixes.Count > 0) {
                    itemData = GetPrice(result, affixes.OfType<TotalAffixValue>().Cast<ITradeAffix>().ToList(), baseAffixes);
                    result.Item.Prices.Add($"Price Ignore[Unique]", itemData);
                }
            }


            return itemData;
        }

        private static List<PoeItemData> GetPrice(FilterResult result, List<ITradeAffix> affixes, NiceDictionary<string, string> generalParams) {
            List<PoeItemData> itemData = new List<PoeItemData>();
            try {
                var client = new PoeTradeClient();
                itemData = client.GetPrice(affixes, generalParams).Result;
            }
            catch (WebException webException) {
                result.Item.Prices.Add(BaseItem.PriceName, new List<PoeItemData>() {
                    new PoeItemData(webException.Message)
                });

                itemData = new List<PoeItemData>();
                itemData.AddRange(GetPoePrice(result));
            }
            catch (Exception e) {
                itemData.Add(new PoeItemData(e.Message));
            }

            return itemData;
        }

        public static IReadOnlyCollection<PoeItemData> GetPoeWatcherPrice(this FilterResult result, string name, List<string> eyeAffixes) {
            List<PoeItemData> itemData = new List<PoeItemData>();
            var generalParams = result.GetPoeTradeGeneralAffixes();

            if (result.Item is UniqueItem uItem) {
                List<ITradeAffix> affixes = result.FilteredAffixes.Values.SelectMany(f => f).Cast<ITradeAffix>().ToList();
                if (uItem.IsWatcherEye) {
                    foreach (string eyeAffix in eyeAffixes) {
                        affixes.Add(new TradeAffix { PoeTradeName = eyeAffix, Value = 0, IsTotal = false });
                    }
                }
                itemData = GetPrice(result, affixes, generalParams);
                result.Item.Prices.Add(name, itemData);
            }

            return itemData;
        }

        public static IReadOnlyCollection<PoeItemData> GetPoeUniquePrice(this FilterResult result, string name, List<TradeAffix> affixes) {
            List<ITradeAffix> itemAffixes = result.FilteredAffixes.Values.SelectMany(f => f).Cast<ITradeAffix>().ToList();
            itemAffixes.AddRange(affixes);
            var generalParams = result.GetPoeTradeGeneralAffixes();
            if (generalParams.ContainsKey("pdps_min")) generalParams.Remove("pdps_min");
            if (generalParams.ContainsKey("dps_min")) generalParams.Remove("dps_min");
            if (generalParams.ContainsKey("edps_min")) generalParams.Remove("edps_min");
            if (generalParams.ContainsKey("crit_min")) generalParams.Remove("crit_min");
            if (generalParams.ContainsKey("armour_min")) generalParams.Remove("armour_min");
            if (generalParams.ContainsKey("evasion_min")) generalParams.Remove("evasion_min");
            if (generalParams.ContainsKey("shield_min")) generalParams.Remove("shield_min");


            List<PoeItemData> itemData = GetPrice(result, itemAffixes, generalParams);
            result.Item.Prices.Add(name, itemData);

            return itemData;
        }

        private static void PopulateWithUniqueAffixes(FilterResult result, List<ITradeAffix> affixes, bool loadUniquePrice = false) {
            if (result.Item is JewelItem jItem) {
                affixes.AddRange(jItem.JewelAffixes);
            }
            if (result.Item is UniqueItem uItem) {
                if (uItem.UniquePriceSets.ContainsKey(BaseItem.PriceName)) {
                    affixes.AddRange(uItem.UniquePriceSets[BaseItem.PriceName]);
                }
                foreach (var set in uItem.UniquePriceSets) {
                    if (set.Key == BaseItem.PriceName) {
                        continue;
                    }

                    if (loadUniquePrice) {
                        result.GetPoeUniquePrice(set.Key, set.Value);
                    }
                }

                if (uItem.IsWatcherEye) {
                    foreach (string eyeAffix in uItem.WatcherEyeAffixes) {
                        if (loadUniquePrice) {
                            result.GetPoeWatcherPrice(eyeAffix, new List<string>() { eyeAffix });
                        }
                        affixes.Add(new TradeAffix { PoeTradeName = eyeAffix, Value = 0, IsTotal = false });
                    }
                    if (uItem.WatcherEyeAffixes.Count == 3 && loadUniquePrice) {
                        result.GetPoeWatcherPrice("1,2 affix", uItem.WatcherEyeAffixes.Skip(1).ToList());
                        result.GetPoeWatcherPrice("2,3 affix", uItem.WatcherEyeAffixes.Take(2).ToList());
                        result.GetPoeWatcherPrice("1,3 affix", new List<string>(){ uItem.WatcherEyeAffixes[0], uItem.WatcherEyeAffixes[2]});
                    }
                }

            }
        }

        public static IReadOnlyCollection<PoeItemData> GetPoeBasePrice(this FilterResult result) {
            if (result.Item is Item baseItem) {
                var priceName = $"Base Price[{baseItem.ItemLevel},{baseItem.Core.Mark.ToString()}]";
                if ((baseItem.ItemLevel >= 70 && (baseItem.Core.Mark == Mark.Elder || baseItem.Core.Mark == Mark.Shaper || ItemTypes.RareBases.Contains(baseItem.Core.BaseName))) || baseItem.ItemLevel >= 80) {
                    NiceDictionary<string, string> affixes = GetBaseQueryAffixes(result, baseItem);
                    List<PoeItemData> itemData = GetPrice(result, new List<ITradeAffix>(), affixes);
                    result.Item.Prices.Add(priceName, FilterPrices(itemData));
                    return itemData;
                }
            }

            return null;
        }

        public static IReadOnlyCollection<PoeItemData> GetPoeSyncBasePrice(this FilterResult result) {
            if (result.Item is Item baseItem) {
                NiceDictionary<string, string> baseAffixes = GetBaseQueryAffixes(result, baseItem);
                List<PoeItemData> itemData = new List<PoeItemData>();

                if (baseItem.Core?.Mark == Mark.Synthesised) {
                    var affixes = new List<ITradeAffix>();

                    var implicits = baseItem.ImplicitSource.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string @implicit in implicits) {
                        var poeTradeName = "(implicit) " + @implicit;
                        poeTradeName = Regex.Replace(poeTradeName, @"([\d.]+)", "#");
                        var affix = new TradeAffix() { IsTotal = false, Value = baseItem.ImplicitSource.ParseTo(0, @"(\d+)"), PoeTradeName = poeTradeName };
                        affixes.Add(affix);

                        itemData = GetPrice(result, new List<ITradeAffix>(){affix}, baseAffixes);
                        result.Item.Prices.Add($"Sync Price[{baseItem.ItemLevel}, {affix.PoeTradeName}]", FilterPrices(itemData));
                    }
                    itemData = GetPrice(result, affixes, baseAffixes);
                    result.Item.Prices.Add($"Sync Price[{baseItem.ItemLevel}]", FilterPrices(itemData));

                    baseAffixes.Remove("ilvl_min");
                    itemData = GetPrice(result, affixes, baseAffixes);
                    result.Item.Prices.Add($"Sync Price[{baseItem.ItemLevel}, no ilvl]", FilterPrices(itemData));

                    if (baseAffixes.ContainsKey("base")) {
                        baseAffixes.Add("ilvl_min", result.Item.ItemLevel.ToString());
                        baseAffixes.Remove("base");
                        itemData = GetPrice(result, affixes, baseAffixes);
                        result.Item.Prices.Add($"Sync Price[{baseItem.ItemLevel}, no base]", FilterPrices(itemData));
                    }
                }
                if (baseItem.Core?.Mark == Mark.Fractured) {
                    List<AffixValue> affixes = baseItem.Affixes.Values.OfType<AffixValue>().Where(x => x.IsFractured).ToList();
                    baseAffixes.Remove("ilvl_min");
                    if (baseAffixes.ContainsKey("base")) { baseAffixes.Remove("base"); }
                    List<ITradeAffix> tradeAffixes = new List<ITradeAffix>();
                    foreach (var affix in affixes) {
                        TradeAffix tradeAffix = new TradeAffix() {
                            PoeTradeName = "(fractured) " + affix.PoeTradeName,
                            Value = affix.AffixTier.MinValue,
                            IsTotal = false
                        };
                        tradeAffixes.Add(tradeAffix);

                        if (affix.CalculatedTier <= 4) {
                            itemData = GetPrice(result, new List<ITradeAffix>() { tradeAffix }, baseAffixes);
                            if (itemData.Take(10).Average(x => x.Currency.ValueInChaos) >= 4) {
                                result.Item.Prices.Add($"Frac Price[{baseItem.ItemLevel}, {affix.Name}]", FilterPrices(itemData));
                            }
                        }
                    }
                    if (affixes.Any(x => x.CalculatedTier <= 4) || baseItem.Core?.Base?.BaseTier <= 2) {
                        baseAffixes = GetBaseQueryAffixes(result, baseItem);
                        if (new[] { ItemType.Amulet, ItemType.Ring, ItemType.AbyssalJewel, ItemType.Belt, ItemType.Jewel, ItemType.Quiver }.Contains(result.Item.Type) && baseAffixes.ContainsKey("base")) {
                            baseAffixes.Remove("base");
                        }

                        itemData    = GetPrice(result, tradeAffixes, baseAffixes);
                        if (itemData.Take(10).Average(x => x.Currency.ValueInChaos) >= 4) {
                            result.Item.Prices.Add($"Frac Price[{baseItem.ItemLevel}]", FilterPrices(itemData));
                        }

                        baseAffixes.Remove("ilvl_min");
                        itemData = GetPrice(result, tradeAffixes, baseAffixes);
                        if (itemData.Take(10).Average(x => x.Currency.ValueInChaos) >= 4) {
                            result.Item.Prices.Add($"Frac Price[{baseItem.ItemLevel}, no ilvl]", FilterPrices(itemData));
                        }

                        baseAffixes.Add("ilvl_min", result.Item.ItemLevel.ToString());

                        if (baseAffixes.ContainsKey("base")) {
                            baseAffixes.Remove("base");
                            itemData = GetPrice(result, tradeAffixes, baseAffixes);
                            if (itemData.Take(10).Average(x => x.Currency.ValueInChaos) >= 4) {
                                result.Item.Prices.Add($"Frac Price[{baseItem.ItemLevel}, no base]", FilterPrices(itemData));
                            }
                        }
                    }
                }

                return itemData;
            }

            return null;
        }

        private static List<PoeItemData> FilterPrices(List<PoeItemData> itemData) {
            return itemData.Count == 1 && itemData.Average(x => x.Currency.ValueInChaos) == 0 ? itemData : itemData.Take(10).Average(x => x.Currency.ValueInChaos) > 0.8 ? itemData : new List<PoeItemData>() { new PoeItemData("Trash") };
        }

        private static NiceDictionary<string, string> GetBaseQueryAffixes(FilterResult result, Item baseItem) {
            NiceDictionary<string, string> affixes = new NiceDictionary<string, string>();
            affixes.Add("league", Config.LeagueName);
            affixes.Add("type", EnumInfo<ItemType>.Current[result.Item.Type].DisplayName);
            affixes.Add("ilvl_min", result.Item.ItemLevel.ToString());
            affixes.Add("rarity", "non_unique");
            affixes.Add("corrupted", baseItem.Corrupted ? "1" : "0");
            affixes.Add("mirrored", baseItem.Mirrored ? "1" : "0");
            if (baseItem.Links == 6) {
                affixes.Add("link_min", "6");
            }

            if (baseItem.Core.Mark == Mark.Elder) { affixes.Add("elder", "1"); }
            if (baseItem.Core.Mark == Mark.Shaper) { affixes.Add("shaper", "1"); }
            if (baseItem.Core.Mark == Mark.Fractured) { affixes.Add("fractured", "1"); }
            if (baseItem.Core.Mark == Mark.Synthesised) { affixes.Add("synthesised", "1"); }

            var baseName = baseItem.Core?.BaseName;
            if (ItemTypes.RareBases.Contains(baseName) || baseItem.Type == ItemType.AbyssalJewel || result.Item is ArmourItem || result.Item is WeaponItem ||
                result.Item is JewelryItem) {
                affixes.Add("base", HttpUtility.UrlEncode(baseName));
            }
            return affixes;
        }
    }
}