namespace PoeTrade.Response {
    public interface ITradeAffix {
        bool IsTotal { get; }
        string PoeTradeName { get; }
        double Value { get; }
    }
}