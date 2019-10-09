using PoeParser.Common;
using PoeParser.Filter.Tooltip;
using Tornado.Common.Utility;
using Tornado.Parser.Entities.Affixes;
using Tornado.Parser.Filter;

namespace Tornado.Parser.Entities {
    public class WeaponItem : Item {
        public WeaponItem(string source, ItemRarity rarity, Core core, Tornado.Data.Data.BaseItem baseItem, string name, bool noCraft) : base(source, rarity, core, baseItem, name, noCraft) {
            var weaponAffixes = core.GetAffixes(this);
            Color = ToolTipColor.GeneralGroup;

            foreach (IAffixValue affix in weaponAffixes) {
                Affixes.Add(affix.Name, affix);
            }
        }

        public override NiceDictionary<string, string> GetPoeTradeAffixes(FilterResult filter) {
            var generalParams = Core.GetPoeTradeAffixes(filter);
            generalParams.Add("rarity", "rare");
            if (WhiteSockets > 0) { generalParams.Add("sockets_w", WhiteSockets.ToString()); }
            return generalParams;
        }

        public override string ToString() {
            return $"{EnumInfo<ItemType>.Current[Type].DisplayName}: {Name} - {Price}";
        }
    }
}