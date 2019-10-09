namespace Tornado.Parser.Entities.Affixes {
    public class AffixValue : IAffixValue {
        public AffixTier AffixTier { get; }
        public Affix Affix { get; }
        public bool IsTotal => false;
        public string PoeTradeName => RegexPattern;
        public string Tooltip { get; }
        public double Value { get; set; }
        public double MaxValue => AffixTier.MaxValue;
        public double MinValue => AffixTier.MinValue;
        public string RegexPattern => Affix.RegexPattern;
        public AffixType Type => Affix.Type;
        public string Name => Affix.Name;
        public bool IsCraft { get; }
        public bool IsFractured { get; }
        public int CalculatedTier { get; }

        public AffixValue(AffixTier affixTier, Affix affix, double value, bool craft, bool isFractured, int tier = 10) {
            AffixTier = affixTier;
            Affix = affix;
            Value = value;
            IsCraft = craft;
            IsFractured = isFractured;
            CalculatedTier = tier;
        }

        public override string ToString() {
            return IsCraft
                ? $"{Value.ToString("0.#")} {Name}"
                : $"[{Value.ToString("0.#")}{(MaxValue == Value ? "" : $"/{MaxValue.ToString("0.#")}")}] {Name}";
        }
    }
}