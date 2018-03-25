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

        public static FilterResult Parse(string source) {
            try {
                var lines = Regex.Split(source, "\r\n");
                if (lines.Length == 1) {
                    return null;
                }

                string name = lines[1].Replace("<<set:MS>><<set:M>><<set:S>>", "");
                ItemRarity rarity = source.ParseTo(ItemRarity.Common, "Rarity: ([\\w ]*)\r\n");

                BaseItem item = null;

                if (rarity == ItemRarity.Currency) {
                    if (name.Contains("Essence")) {
                        item = new NormalItem(source, name);
                    }
                    if (rarity == ItemRarity.Common)
                        return null;
                }

                int mapTier = source.ParseTo(0, "Map Tier: (\\d+)");
                bool isCard = source.ContainsPattern("Stack Size:");
                if (mapTier > 0 && rarity != ItemRarity.Unique && !isCard) {
                    item = new MapItem(source, rarity, lines, mapTier);
                }

                if (rarity == ItemRarity.Gem) {
                    item = new GemItem(source, name);
                }

                if (cache.Contains(name))
                    return (FilterResult)cache[name];
                if (item != null && cache.Contains(item.CacheName))
                    return (FilterResult)cache[item.CacheName];

                Core core = ItemTypes.GetItemBase(lines);

                if (item == null) {
                    if (rarity == ItemRarity.Normal && core.Type == ItemType.Unknown)
                        item = new NormalItem(source, name);
                    if (rarity == ItemRarity.DivinationCard)
                        item = new DivinationCard(source, name);

                    if (rarity == ItemRarity.Unique) {
                        item = new UniqueItem(source, core, name, lines[2]);
                        if (item.Unindentified)
                            item = null;
                    }
                    if (core.Type == ItemType.Jewel && rarity != ItemRarity.Unique)
                        item = new JewelItem(source, rarity, name); //todo jewels atleast track life or es
                    if (rarity == ItemRarity.Rare) {
                        if (core.Type.IsWeapon())
                            item = new WeaponItem(source, rarity, core, name);
                        if (core.Type.IsJewelery() || core.Type == ItemType.Quiver)
                            item = new JewelryItem(source, rarity, core, name);
                        if (core.Type.IsArmor())
                            item = new ArmourItem(source, rarity, core, name);
                    }
                }

                if (item == null) {
                    return null;
                }

                var f = PoeData.Manager.Filter(item);
                if (rarity == ItemRarity.Rare) {
                    item.Color = f.Color;
                }
                Task.Run(() => { f.GetPoePrice(); });

                cache.Add(item.CacheName, f, DateTimeOffset.Now.AddMinutes(10));
                itemNames.Add(item.CacheName);
                return f;
            } catch (Exception e) {
                File.AppendAllText("Error.log", $"{DateTime.Now.ToString("g")}\r\n Source: {source} \r\nError: {e.Message + e.StackTrace}\r\n{new string('-', 30)}\r\n");
                return null;
            }
        }
    }
}