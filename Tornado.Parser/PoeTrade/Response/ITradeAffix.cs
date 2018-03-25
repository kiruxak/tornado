namespace Tornado.Parser.PoeTrade.Response {
    public interface ITradeAffix {
        bool IsTotal { get; }
        string PoeTradeName { get; }
        double Value { get; }
    }

    public class TradeAffix : ITradeAffix {
        public bool IsTotal { get; set; }
        public string PoeTradeName { get; set; }
        public double Value { get; set; }
    }
}