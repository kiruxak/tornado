using System;
using System.Linq;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Spreadsheet;
using NCalc;
using Tornado.Common.Extensions;
using Tornado.Parser.Common.Extensions;
using Tornado.Parser.Entities.Affixes;
using Tornado.Parser.Parser;
using Item = Tornado.Parser.Entities.Item;

namespace Tornado.Parser.Data {
    public class TotalAffixRecord : IRecord {
        public string Name { get; set; }
        public string Template { get; set; }
        public string RegexPattern { get; set; }
        public string MathExpression { get; set; }
        public AffixType Type => AffixType.Total;

        public void Parse(string line) {
            string[] values = line.Split('\t');
            Name = values[0];
            Template = values[1];
            RegexPattern = values[2];
            MathExpression = values[3];
        }

        public TotalAffixRecord() {
        }

        public TotalAffixRecord(Row row, SharedStringTable sh) {
            var cells = row.Elements<Cell>().ToList();
            Name = cells[0].GetValue(sh);
            Template = cells[1].GetValue(sh);
            RegexPattern = cells[2].GetValue(sh);
            MathExpression = cells[3].GetValue(sh);
        }

        private static readonly Func<IAffixValue, string> SetAffixMinValue = (a) => a.MinValue.ToString();
        private static readonly Func<IAffixValue, string> SetAffixMaxValue = (a) => a.MaxValue.ToString();
        private static readonly Func<IAffixValue, string> SetAffixValue = (a) => a.Value.ToString();

        public TotalAffixValue GetValue(Item item) {
            var craft = item.Affixes.Values.OfType<AffixValue>().FirstOrDefault(a => a.IsCraft);

            double value = getValue(item, SetAffixValue);
            double maxValue = getValue(item, SetAffixMaxValue);
            double minValue = getValue(item, SetAffixMinValue);
            if (item.CraftAddedByFilter && craft != null && MathExpression.ContainsPattern(craft.Name)) {
                return new TotalAffixValue(this, value, minValue, maxValue, craft);
            }
            return new TotalAffixValue(this, value, minValue, maxValue, null);
        }

        private double getValue(Item item, Func<IAffixValue, string> setAffixValue) {
            string value = item.Affixes.Where(a => a.Value is AffixValue).Aggregate(MathExpression, (current, a) => current.Replace(a.Key, setAffixValue(a.Value).ToString()));
            value = value.Replace("Quality", item.Quality <= 20 ? "20" : item.Quality.ToString());
            value = Regex.Replace(value, "([a-zA-z]+)", "0");

            object val = new Expression(value).Evaluate();
            return val as double? ?? (double)(int)val;
        }
    }
}