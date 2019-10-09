using System.Linq;
using DocumentFormat.OpenXml.Spreadsheet;
using PoeParser.Common;
using PoeParser.Filter.Tooltip;
using Tornado.Common.Utility;
using Tornado.Parser.Entities.Affixes;
using Tornado.Parser.Filter;

namespace Tornado.Parser.Entities {
    public class ArmourItem : Item {
        public int Armour { get; set; }
        public int Evasion { get; set; }
        public int Energy { get; set; }

        public ArmourItem(string source, ItemRarity rarity, Core core, Tornado.Data.Data.BaseItem baseItem, string name, bool noCraft) : base(source, rarity, core, baseItem, name, noCraft) {
            var armourAffixes = core.GetAffixes(this);
            Color = ToolTipColor.GeneralGroup;

            foreach (IAffixValue affix in armourAffixes) {
                Affixes.Add(affix.Name, affix);
            }
        }

        public override NiceDictionary<string, string> GetPoeTradeAffixes(FilterResult filter) {
            var generalParams = Core.GetPoeTradeAffixes(filter);
            generalParams.Add("rarity", "rare");

            if (ItemTypes.RareBases.Contains(Core.BaseName)) {
                generalParams.Add("base", Core.BaseName);
            }
            if (WhiteSockets > 0) { generalParams.Add("sockets_w", WhiteSockets.ToString()); }

            return generalParams;
        }

        public override string ToString() {
            return $"{EnumInfo<ItemType>.Current[Type].DisplayName}: {Name} - {Price}";
        }
    }
}