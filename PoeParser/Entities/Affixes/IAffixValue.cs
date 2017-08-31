using PoeTrade.Response;

namespace PoeParser.Entities.Affixes {
    public interface IAffixValue: ITradeAffix {
        string Name { get; }
        string RegexPattern { get; }
        double MaxValue { get; }
        double MinValue { get; }
        AffixType Type { get; }
    }
}