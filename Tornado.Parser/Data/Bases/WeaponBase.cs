using System;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.Spreadsheet;
using Tornado.Common.Extensions;
using Tornado.Common.Utility;
using Tornado.Parser.Common.Extensions;
using Tornado.Parser.Entities;
using Tornado.Parser.Entities.Affixes;
using Tornado.Parser.Parser;
using Item = Tornado.Parser.Entities.Item;

namespace Tornado.Parser.Data {
    public class WeaponBase : Base, IRecord {
        private const string PDPS_EXPRESSION = "({0}+fPhysOH+fPhysTH)*(120+mPhys)/100*{1}*((100+wepAttSpeed)/100)";
        private const string EDPS_EXPRESSION = "(fColdOH+fFireOH+fLightOH+fColdTH+fFireTH+fLightTH+fColdAtt+fFireAtt)*{0}*((100+wepAttSpeed)/100)";
        private const string DPS_EXPRESSION = "Edps+Pdps";
        private const string WEPCRIT_EXPRESSION = "{0}*(100+wepCritChance)/100";

        public ItemType Type { get; set; }
        public double AttackSpeed { get; set; }
        public double CritChance { get; set; }
        public int MinDmg { get; set; }
        public int MaxDmg { get; set; }

        public void Parse(string line) {
            string[] values = line.Split('\t');
            Type = values[0].ParseTo(ItemType.Unknown);
            Name = values[1];
            MinDmg = values[3].ParseTo(0, "(\\d+) to");
            MaxDmg = values[3].ParseTo(0, "to (\\d+)");
            AttackSpeed = values[4].ParseTo(0.0);
            CritChance = values[5].ParseTo(0.0, "([\\d.]+)");
        }

        public WeaponBase() {
        }

        public WeaponBase(Row row, SharedStringTable sh) {
            var cells = row.Elements<Cell>().ToList();
            Name = cells[0].GetValue(sh);
            Type = cells[0].GetValue(sh).ParseTo(ItemType.Unknown);
            Name = cells[1].GetValue(sh);
            MinDmg = cells[3].GetValue(sh).ParseTo(0, "(\\d+) to");
            MaxDmg = cells[3].GetValue(sh).ParseTo(0, "to (\\d+)");
            AttackSpeed = cells[4].GetValue(sh).ParseTo(0.0);
            CritChance = cells[5].GetValue(sh).ParseTo(0.0, "([\\d.]+)");
        }

        public override NiceDictionary<string, TotalAffixRecord> GetTotalAffixes(Item item) {
            List<TotalAffixRecord> a = new List<TotalAffixRecord>();

            TotalAffixRecord pDps = new TotalAffixRecord() {
                MathExpression = string.Format(PDPS_EXPRESSION, Math.Round((double)(MaxDmg + MinDmg) / 2, 0), AttackSpeed),
                Name = BuildInAffixNames.PDps,
                RegexPattern = "",
                Template = "{0} phys dps"
            };
            TotalAffixRecord eDps = new TotalAffixRecord() {
                MathExpression = string.Format(EDPS_EXPRESSION, AttackSpeed),
                Name = BuildInAffixNames.EDps,
                RegexPattern = "",
                Template = "{0} ele dps"
            };
            TotalAffixRecord dps = new TotalAffixRecord() {
                MathExpression = string.Format(DPS_EXPRESSION),
                Name = BuildInAffixNames.Dps,
                RegexPattern = "",
                Template = "{0} total dps"
            };
            TotalAffixRecord wepCrit = new TotalAffixRecord() {
                MathExpression = string.Format(WEPCRIT_EXPRESSION, CritChance),
                Name = BuildInAffixNames.WepCrit,
                RegexPattern = "",
                Template = "{0}% weapon crit"
            };

            a.Add(dps);
            a.Add(eDps);
            a.Add(pDps);
            a.Add(wepCrit);

            return a.ToNiceDictionary(ta => ta.Name, ta => ta);
        }
    }
}