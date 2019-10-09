using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tornado.Parser.Common.Extensions;
using Tornado.Parser.Entities;
using Tornado.Parser.Filter;
using Tornado.Parser.PoeTrade;
using Tornado.Parser.PoeTrade.Data;
using BaseItem = Tornado.Parser.Entities.BaseItem;
using ItemType = Tornado.Parser.Entities.ItemType;

namespace Tornado.Parser.Parser {
    public class ItemParser {
        private static readonly MemoryCache cache = MemoryCache.Default;
        private static readonly List<string> itemNames = new List<string>();

        public static List<FilterResult> GetCachedItems() {
            List<FilterResult> items = new List<FilterResult>();
            foreach (string itemName in itemNames.ToList()) {
                if (cache.Contains(itemName)) {
                    items.Add((FilterResult)cache[itemName]);
                } else {
                    itemNames.Remove(itemName);
                }
            }
            return items;
        }

        public static void ClearCachedItems() {
            foreach (string itemName in itemNames) {
                cache.Remove(itemName);
            }
        }

        public static FilterResult Parse(string source, bool noCraft) {
            try {
                var lines = Regex.Split(source, "\r\n");
                if (lines.Length == 1) {
                    return null;
                }

                string name = lines[1].Replace("<<set:MS>><<set:M>><<set:S>>", "");
                ItemRarity rarity = source.ParseTo(ItemRarity.Common, "Rarity: ([\\w ]*)\r\n");

                BaseItem item = null;

                if (rarity == ItemRarity.Currency) {
                    if (name.Contains("Essence") || name.Contains("Remnant of Corruption")) {
                        item = new NormalItem(source, name, ItemType.Essence); //Essence
                    }
                    if (name.Contains(" Fossil")) {
                        item = new NormalItem(source, name, ItemType.Fossil); //Fossil
                    }
                    if (source.Contains("Sockets: D ")) {
                        item = new NormalItem(source, name, ItemType.Resonator); //Resonator
                    }
                    // if (name.Contains("Timeless")) {
                    //     item = new NormalItem(source, name, ItemType.VaalFragment); //Timeless Fragment
                    // }
                    if (ItemTypes.IncursionVials.Contains(name)) {
                        item = new NormalItem(source, name, ItemType.Vial);
                    }

                    if (item == null && Currency.Data != null) {
                        var currency = Currency.Data.lines.FirstOrDefault(x => x.currencyTypeName == name);
                        item = new CurrencyItem(source, name, currency);
                    }

                    if (rarity == ItemRarity.Common) return null;
                }

                int mapTier = source.ParseTo(0, "Map Tier: (\\d+)");
                if (mapTier > 0 && rarity != ItemRarity.DivinationCard) {
                    item = new MapItem(source, rarity, lines, mapTier);
                }

                if (rarity == ItemRarity.Gem) {
                    var vaalName = lines.FirstOrDefault(x => x.StartsWith("Vaal "));
                    item = new GemItem(source, vaalName ?? name);
                }

                if (cache.Contains(name))
                    return (FilterResult)cache[name];
                if (item != null && cache.Contains(item.CacheName))
                    return (FilterResult)cache[item.CacheName];

                Core core = ItemTypes.GetItemBase(lines);
                core.Links = GetLinks(source);
                core.ItemLevel = source.ParseTo(0, "Item Level: (\\d+)");

                if (item == null) {
                    if ((rarity == ItemRarity.Normal || rarity == ItemRarity.Magic) && (core.IsGoodBase || core.Links >= 5)) {
                        item = new NormalItem(source, core.BaseName ?? name, ItemType.RareBase, core);
                    }
                    //if (rarity == ItemRarity.Rare || source.Contains("Family:")) {
                    //    item = new NormalItem(source, lines[2], ItemType.Unknown);
                    //}
                    if (source.Contains("Family:") && source.Contains("Genus:")) {
                        item = new NormalItem(source, name, ItemType.Beast);
                    }
                    if (rarity == ItemRarity.DivinationCard) {
                        item = new DivinationCard(source, name);
                    }
                    if (rarity == ItemRarity.Normal && item == null) {
                        item = new NormalItem(source, name, ItemType.Unknown);
                    }
                    if (rarity == ItemRarity.Unique && item == null) {
                        item = new UniqueItem(source, core, name, lines[2]);
                        if (item.Unindentified) {
                            item = null;
                        }
                    }
                    if (rarity != ItemRarity.Unique && (core.Type == ItemType.Jewel || core.Type == ItemType.AbyssalJewel)) {
                        item = new JewelItem(source, rarity, core);
                    }

                    if (rarity == ItemRarity.Rare || core.Mark == Mark.Fractured) {
                        if (core.Type.IsWeapon())
                            item = new WeaponItem(source, rarity, core, null, name, noCraft);
                        if (core.Type.IsJewelery() || core.Type == ItemType.Quiver)
                            item = new JewelryItem(source, rarity, core, null, name, noCraft);
                        if (core.Type.IsArmor())
                            item = new ArmourItem(source, rarity, core, null, name, noCraft);
                    }
                }

                if (item == null) {
                    return null;
                }

                if (cache.Contains(item.CacheName))
                    return (FilterResult)cache[item.CacheName];

                var f = PoeData.Manager.Filter(item, noCraft);
                if (rarity == ItemRarity.Rare) {
                    item.Color = f.Color;
                }
                Task.Run(() => {
                    f.GetPoePrice();
                    if (!(item is UniqueItem)) {
                        f.GetPoeBasePrice();
                    }
                    if (f.Item is Item i && (i.Core?.Mark == Mark.Synthesised || i.Core?.Mark == Mark.Fractured)) {
                        f.GetPoeSyncBasePrice();
                    }
                });

                if (name != "Watcher's Eye" && !(item is JewelItem)) {
                    cache.Add(item.CacheName, f, DateTimeOffset.Now.AddMinutes(10));
                    itemNames.Add(item.CacheName);
                }
                return f;
            } catch (Exception e) {
                File.AppendAllText("Error.log", $"{DateTime.Now.ToString("g")}\r\n Source: {source} \r\nError: {e.Message + e.StackTrace}\r\n{new string('-', 30)}\r\n");
                return null;
            }
        }

        public static int GetLinks(string source) {
            bool hasSockets = source.ContainsPattern("(\\w-\\w-\\w-\\w-\\w-\\w)");
            if (hasSockets)
                return 6;
            hasSockets = source.ContainsPattern("(\\w-\\w-\\w-\\w-\\w)");
            return hasSockets ? 5 : 0;
        }
    }
}