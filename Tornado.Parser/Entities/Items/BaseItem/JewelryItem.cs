using System.Linq;
using DocumentFormat.OpenXml.Spreadsheet;
using PoeParser.Common;
using PoeParser.Filter.Tooltip;
using Tornado.Common.Utility;
using Tornado.Parser.Filter;

namespace Tornado.Parser.Entities {
    public class JewelryItem : Item {
        public JewelryItem(string source, ItemRarity rarity, Core core, Tornado.Data.Data.BaseItem baseItem, string name, bool noCraft) : base(source, rarity, core, baseItem, name, noCraft) {
            Color = ToolTipColor.GeneralGroup;
        }

        public override NiceDictionary<string, string> GetPoeTradeAffixes(FilterResult filter) {
            var properties =  Core.GetPoeTradeAffixes(filter);

            if (ItemTypes.RareBases.Contains(Core.BaseName) || Core.BaseName == "Rustic Sash") {
                properties.Add("base", Core.BaseName);
            }

            return properties;
        }

        public override string ToString() {
            return $"{EnumInfo<ItemType>.Current[Type].DisplayName}: {Name} - {Price}";
        }
    }
}