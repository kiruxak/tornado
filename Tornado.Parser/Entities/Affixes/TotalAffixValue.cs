using System;
using System.Collections.Generic;
using PoeParser.Filter.Tooltip;
using Tornado.Parser.Data;
using Tornado.Parser.Filter.Tooltip;

namespace Tornado.Parser.Entities.Affixes {
    public class TotalAffixValue : IAffixValue {
        public string Name => TotalAffix.Name;
        public string RegexPattern => TotalAffix.RegexPattern;
        public bool IsTotal => true;
        public string PoeTradeName => RegexPattern;
        public double Value { get; }
        public double MaxValue { get; }
        public double MinValue { get; }
        public AffixType Type => AffixType.Total;
        public TotalAffixRecord TotalAffix { get; }
        private AffixValue craft { get; }

        public TotalAffixValue(TotalAffixRecord a, double value, double minValue, double maxValue, AffixValue craft) {
            TotalAffix = a;
            Value = value;
            MaxValue = maxValue;
            MinValue = minValue;
            this.craft = craft;
        }

        public override string ToString() {
            var x = $"{(MaxValue == Value ? "" : $" [{MaxValue.ToString("0.#")}]")}";
            return $"{string.Format(TotalAffix.Template, Value.ToString("0.#"))}{x}{(craft == null ? "" : $" (+{craft.Value.ToString("0.#")})")}";
        }

        public ColoredLine GetTooltipLine(string color, int size) {
            var s = TotalAffix.Template.Split(new[] {
                "{0}"
            }, StringSplitOptions.None);
            var prefix = "";

            if (s[1][0] != ' ') {
                prefix += s[1][0];
                s[1] = s[1].Substring(1, s[1].Length - 1);
            }

            return new ColoredLine(new List<ColoredText>()
                                           .Add(s[0] + Value.ToString("0.#") + prefix, ToolTipColor.GeneralGroup, size + 1)
                                           .Add(s[1], color, size)
                                           .Add(Value >= MaxValue ? "" : $" [{MinValue}-{MaxValue}]", color, size - 4, heightOffset: 2)
                                           .Add($"{(craft == null ? "" : $" (+{craft})")}", ToolTipColor.Gem, size - 4, heightOffset: 2));
        }
    }
}