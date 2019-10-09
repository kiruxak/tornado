using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PoeParser.Common;
using PoeParser.Filter.Tooltip;
using Tornado.Common.Utility;
using Tornado.Parser.Filter;
using Tornado.Parser.Filter.Tooltip;
using Tornado.Parser.PoeTrade.Data;

namespace Tornado.Parser.Entities {
    public class NormalItem : BaseItem {
        public Core Core { get; }

        public NormalItem(string source, string name, ItemType type, Core core = null) : base(source, ItemRarity.Normal, type, name) {
            Core = core;
            GetType(source, name);
            GetTooltipColor(Type);
        }

        private void GetType(string source, string name) {
            if (source.Contains("Right-click to add this prophecy to your character.")) {
                Type = ItemType.Prophecy;
            }
            if (name == "Offering to the Goddess") {
                Type = ItemType.Offering;
            }
            if (name == "Divine Vessel") {
                Type = ItemType.DivineVessel;
            }
            if (source.Contains(" Scarab")) {
                Type = ItemType.Scarab;
            }
            if (source.Contains(" Breachstone")) {
                Type = ItemType.Breachstone;
            }
            if (ItemTypes.VaalFrarments.Contains(name)) {
                Type = ItemType.VaalFragment;
            }
            if (ItemTypes.GuardFrarments.Contains(name)) {
                Type = ItemType.GuardFragment;
            }
            if (ItemTypes.ProphecyKeys.Contains(name)) {
                Type = ItemType.VaalFragment;
            }
            if (Type == ItemType.Fossil && Currency.FossilData != null) {
                var currency = Currency.FossilData.lines.FirstOrDefault(x => Name.Contains(x.name));
                if (currency != null) {
                    Prices.Add("Ninja", new List<PoeItemData>(){new PoeItemData(new Currency($"{currency.chaosValue} chaos"), "")});
                }
            }
        }

        private void GetTooltipColor(ItemType type) {
            switch(type) {
                case ItemType.VaalFragment :
                case ItemType.GuardFragment :
                    Color = ToolTipColor.Vaal;
                    break;
                case ItemType.Flask :
                    throw new ArgumentNullException(nameof(type));
                case ItemType.Normal :
                    Color = ToolTipColor.NormalItem;
                    break;
                case ItemType.Offering :
                case ItemType.DivineVessel :
                    Color = ToolTipColor.Offering;
                    break;
                case ItemType.Scarab :
                    Color = ToolTipColor.Betrayal;
                    break;
                case ItemType.Fossil :
                case ItemType.Resonator :
                    Color = ToolTipColor.Delve;
                    break;
                case ItemType.Essence :
                    Color = ToolTipColor.Essence;
                    break;
                case ItemType.RareBase :
                    Color = ToolTipColor.NormalItem;
                    break;
                case ItemType.Vial :
                    Color = ToolTipColor.Incursion;
                    break;
                case ItemType.Prophecy :
                case ItemType.Breachstone :
                    Color = ToolTipColor.Prophecy;
                    break;
                default :
                    Color = ToolTipColor.NormalItem;
                    break;
            }
        }

        public override NiceDictionary<string, string> GetPoeTradeAffixes(FilterResult filter) {
            NiceDictionary<string, string> generalParams = new NiceDictionary<string, string>();
            generalParams.Add("league", Config.LeagueName);

            if ((Type == ItemType.RareBase || Links >= 5) && ItemLevel > 0) {
                if (filter.Item.Links >= 5) {
                    generalParams.Add("link_min", filter.Item.Links.ToString());
                }
                generalParams.Add("ilvl_min", ItemLevel.ToString());
                generalParams.Add("base", HttpUtility.UrlEncode(Name));
                generalParams.Add("rarity", "non_unique");
                generalParams.Add("corrupted", Corrupted ? "1" : "0");
                generalParams.Add("mirrored", Mirrored ? "1" : "0");

                generalParams.Add("elder", Core?.Mark == Mark.Elder ? "1" : "0");
                generalParams.Add("shaper", Core?.Mark == Mark.Shaper ? "1" : "0");
            } else {
                generalParams.Add("name", HttpUtility.UrlEncode(Name));
            }
            return generalParams;
        }

        public override List<ColoredLine> GetTooltip(FilterResult filter) {
            if (Type == ItemType.RareBase || Links >= 5) {
                return new List<ColoredLine>().Add($"Rare Base: {Name}", ToolTipColor.GeneralGroup, 18)
                                              .Add($"Item Level: {ItemLevel}", ToolTipColor.GeneralGroup, 16)
                                              .If(Links >= 5, l => { //if 5 or 6 links
                                                  l.Add($"Links: {Links}", ToolTipColor.NormalItem, 18);
                                              });
            }
            if (Type == ItemType.Prophecy || Type == ItemType.Essence) {
                return new List<ColoredLine>().Add($"{EnumInfo<ItemType>.Current[Type].DisplayName}: {Name}", Color, 18);
            }
            return new List<ColoredLine>().Add($"{Name}", Color, 18);
        }

        public override string ToString() { return $"{EnumInfo<ItemType>.Current[Type].DisplayName}: {Name}, - {Price}"; }
    }
}