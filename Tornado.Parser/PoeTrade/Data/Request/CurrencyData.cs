using System.Collections.Generic;

namespace Tornado.Parser.PoeTrade.Data {
    public class CurrencyData {
        public List<CurrencyInfo> lines { get; set; }
        public List<CurrencyDetail> currencyDetails { get; set; }
    }
}