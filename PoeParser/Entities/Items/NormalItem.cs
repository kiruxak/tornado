﻿using System.Collections.Generic;
using PoeParser.Common;
using PoeParser.Filter;
using PoeParser.Filter.Tooltip;

namespace PoeParser.Entities {
    public class NormalItem : BaseItem {
        public NormalItem(string source, string name) : base(source, ItemRarity.Normal, ItemTypes.VaalFrarments.Contains(name) ? ItemType.VaalFragment : ItemType.Normal, name) {
            Color = Type == ItemType.VaalFragment ? ToolTipColor.Vaal : ToolTipColor.NormalItem;
        }

        public override Dictionary<string, string> GetPoeTradeAffixes(FilterResult filter) {
            Dictionary<string, string> generalParams = new Dictionary<string, string>();
            generalParams.Add("league", Config.LeagueName);
            generalParams.Add("name", System.Web.HttpUtility.UrlEncode(Name));
            return generalParams;
        }

        public override List<ColoredLine> GetTooltip(FilterResult filter) {
            return new List<ColoredLine>().Add($"{EnumInfo<ItemType>.Current[Type].DisplayName}: {Name}", Type == ItemType.VaalFragment ? ToolTipColor.Vaal : ToolTipColor.GeneralGroup, 18);
        }

        public override string ToString() {
            return $"{EnumInfo<ItemType>.Current[Type].DisplayName}: {Name} - {Price}";
        }
    }
}