using System;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.Spreadsheet;
using PoeParser.Common;
using Tornado.Common.Extensions;
using Tornado.Parser.Common.Extensions;
using Tornado.Parser.Entities;
using ItemType = Tornado.Data.Data.ItemType;

namespace Tornado.Parser.Data.Bases {
    public class SynthesisImplicit {
        public static SynthesisImplicit Current = null;

        public SynthesisImplicit(Row row, SharedStringTable sh) {
            List<Cell> cells = row.Elements<Cell>().ToList();
            Text = cells[0].GetValue(sh);
            Text = string.IsNullOrEmpty(Text) ? Current.Text : Text;
            ReqStat = cells[1].GetValue(sh).Replace("+([\\d.]+)", "\\+([\\d.]+)");
            ReqStat = string.IsNullOrEmpty(ReqStat) ? Current.ReqStat : ReqStat;
            ReqValue = cells[3].GetValue(sh).ParseTo(0);
            ReqValue = ReqValue == 0 ? Current.ReqValue : ReqValue;

            string[] types = cells[2].GetValue(sh).Split(new string[]{ ", "}, StringSplitOptions.RemoveEmptyEntries);

            Types = types.Select(x => {
                if (ItemType.TryParse(x.Replace(" ", "").Trim(), out ItemType type)) return type;
                throw new ArgumentOutOfRangeException();
            }).ToList();
            Types = Types.Count == 0 ? Current.Types : Types;

            if (Text.Contains(") to (")) {
                var value = Text.ParseDual("([\\d.]+) to ([\\d.]+)\\) to");
                var value2 = Text.ParseDual("to \\(([\\d.]+) to ([\\d.]+)");
                TextValue = $"({value[0]} to {value[1]}) to ({value2[0]} to {value2[1]})";
            } else if (Text.Contains(" to ")) {
                var value = Text.ParseDual("([\\d.]+) to ([\\d.]+)");
                TextValue = $"{value[0]} to {value[1]}";
            } else {
                TextValue = Text.ParseTo(0.0, "([\\d.]+)").ToString();
            }

            if (TextValue == "0") {
                TextValue = "";
            }

            Current = this;
        }

        //public string ModId { get; }
        public string Text { get; }
        public string TextValue { get; set; }
        public string ReqStat { get; }
        public int ReqValue { get; }
        public List<ItemType> Types { get; }
    }

    public class SynthesisGroup {
        public IReadOnlyCollection<ItemType> Types { get; }
        public IReadOnlyCollection<string> Text { get; }
        public IReadOnlyCollection<string> ReqValues { get; set; }
        public IReadOnlyCollection<string> ReqStats { get; set; }

        public List<SynthesisImplicit> Mods { get; set; }

        public SynthesisGroup(List<SynthesisImplicit> mods) {
            Mods = mods;
            Types = mods.First().Types;
            ReqStats = mods.Select(x => x.ReqStat).Distinct().ToList();
            Text = mods.Select(x => x.Text).Distinct().ToList();
            ReqValues = mods.Select(x => $"{x.ReqStat} - {x.ReqValue}").Distinct().ToList();
        }
    }
}