﻿using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.Spreadsheet;
using Tornado.Common.Extensions;
using Tornado.Common.Utility;
using Tornado.Parser.Common.Extensions;
using Tornado.Parser.Entities.Affixes;
using Tornado.Parser.Parser;
using Item = Tornado.Parser.Entities.Item;

namespace Tornado.Parser.Data {
    public class ArmourBase : Base, IRecord {
        private const string AR_EXPRESSION = "({0}+fAR)*(100+Quality+mAR+mAres+mArev)/100";
        private const string EV_EXPRESSION = "({0}+fEV)*(100+Quality+mEV+mEves+mArev)/100";
        private const string ES_EXPRESSION = "({0}+fES)*(100+Quality+mES+mAres+mEves)/100";

        public int BaseArmor { get; set; }
        public int BaseEvasion { get; set; }
        public int BaseShield { get; set; }

        public void Parse(string line) {
            string[] values = line.Split('\t');
            Name = values[0];
            BaseArmor = values[1].ParseTo(0);
            BaseEvasion = values[2].ParseTo(0);
            BaseShield = values[3].ParseTo(0);
            BaseTier = values[4].ParseTo(0);
        }

        public ArmourBase() {}

        public ArmourBase(Row row, SharedStringTable sh) {
            var cells = row.Elements<Cell>().ToList();
            Name = cells[0].GetValue(sh);
            BaseArmor = cells[1].GetValue(sh).ParseTo(0);
            BaseEvasion = cells[2].GetValue(sh).ParseTo(0);
            BaseShield = cells[3].GetValue(sh).ParseTo(0);
            BaseTier = cells[4].GetValue(sh).ParseTo(0);
        }

        public override NiceDictionary<string, TotalAffixRecord> GetTotalAffixes(Item item) {
            List<TotalAffixRecord> a = new List<TotalAffixRecord>();

            TotalAffixRecord armour = new TotalAffixRecord() {
                MathExpression = string.Format(AR_EXPRESSION, BaseArmor),
                Name = BuildInAffixNames.Armour,
                RegexPattern = "",
                Template = "{0} armour"
            };
            TotalAffixRecord evasion = new TotalAffixRecord() {
                MathExpression = string.Format(EV_EXPRESSION, BaseEvasion),
                Name = BuildInAffixNames.Evasion,
                RegexPattern = "",
                Template = "{0} evasion"
            };
            TotalAffixRecord energy = new TotalAffixRecord() {
                MathExpression = string.Format(ES_EXPRESSION, BaseShield),
                Name = BuildInAffixNames.EnergyShield,
                RegexPattern = "",
                Template = "{0} energy shield"
            };

            a.Add(armour);
            a.Add(evasion);
            a.Add(energy);

            return a.ToNiceDictionary(ta => ta.Name, ta => ta);
        }
    }
}