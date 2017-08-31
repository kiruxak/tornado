using PoeTrade;

namespace PoeParser.Entities.Affixes {
    public class AffixValue : IAffixValue {
        public AffixTier AffixTier { get; }
        public Affix Affix { get; }
        public bool IsTotal => false;
        public string PoeTradeName => RegexPattern;
        public double Value { get; }
        public double MaxValue => AffixTier.MaxValue;
        public double MinValue => AffixTier.MinValue;
        public string RegexPattern => Affix.RegexPattern;
        public AffixType Type => Affix.Type;
        public string Name => Affix.Name;
        public bool IsCraft { get; }

        public AffixValue(AffixTier affixTier, Affix affix, double value, bool craft) {
            AffixTier = affixTier;
            Affix = affix;
            Value = value;
            IsCraft = craft;
        }

        public override string ToString() {
            return IsCraft 
                ? $"{Value.ToString("0.#")}{Name}"
                : $"[{Value.ToString("0.#")}{(MaxValue == Value ? "" : $"/{MaxValue.ToString("0.#")}")}]{Name}";
        }
    }
}