using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;
using PoeParser.Common;
using PoeParser.Filter.Tooltip;
using Tornado.Common.Utility;
using Tornado.Parser.Common.Extensions;
using Tornado.Parser.Filter;
using Tornado.Parser.Filter.Tooltip;

namespace Tornado.Parser.Entities {
    public class JewelItem : Item {
        public double Life { get; set; }
        public double AttackDamage { get; set; }
        public double SpellDamage { get; set; }

        public JewelItem(string source, ItemRarity rarity, string name) : base(source, rarity, new Core(ItemType.Jewel), name) {
            Color = ToolTipColor.Gem;

            if (source.Contains("Eye")) 
            {
                Life = source.ParseTo(0, "\\+(\\d+) to maximum Life");
                if (source.Contains("Hypnotic Eye Jewel"))                 {
                    Regex regex = new Regex("Adds (\\d+) to (\\d+) (\\w+) Damage to Spells");
                    Match match = regex.Match(source);

                    while (match.Success)                     {
                        SpellDamage += (double.Parse(match.Groups[1].Value) + double.Parse(match.Groups[2].Value)) / 2;
                        match = match.NextMatch();
                    }
                }
                else
                {
                    Regex regex = new Regex("Adds (\\d+) to (\\d+) (\\w+) Damage to (\\w+ )?Attacks");
                    Match match = regex.Match(source);

                    while (match.Success)
                    {
                        AttackDamage += (int.Parse(match.Groups[1].Value) + int.Parse(match.Groups[2].Value)) / 2;
                        match = match.NextMatch();
                    }
                }
            }

        }

        public override NiceDictionary<string, string> GetPoeTradeAffixes(FilterResult filter) {
            NiceDictionary<string, string> generalParams = new NiceDictionary<string, string>();
            generalParams.Add("league", Config.LeagueName);
            generalParams.Add("type", "Jewel");
            if (Rarity == ItemRarity.Unique) generalParams.Add("name", HttpUtility.UrlEncode(Name));

            return generalParams;
        }

        public override List<ColoredLine> GetTooltip(FilterResult filter) {
            var lines = new List<ColoredLine>();
            lines.Add(new ColoredLine($"Jewel: {Name}", ToolTipColor.GeneralGroup, 18));
            lines.AddEmptyLine();
            if (Life > 0) { lines.Add($"+{Life} life", "FFF2E920", 16); }
            if (SpellDamage > 0) { lines.Add($"+{SpellDamage} spell", "FFF25820", 16); }
            if (AttackDamage > 0) { lines.Add($"+{AttackDamage} attack", "FFF25820", 16); }
            return lines;
        }

        public override string ToString() {
            return $"{EnumInfo<ItemType>.Current[Type].DisplayName}: {Name} - {Price}";
        }
    }
}