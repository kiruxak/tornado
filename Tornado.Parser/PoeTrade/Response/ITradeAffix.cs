using System.Collections.Generic;
using Tornado.Common.Utility;
using Tornado.Parser.Entities;

namespace Tornado.Parser.PoeTrade.Response {
    public interface ITradeAffix {
        bool IsTotal { get; }
        string PoeTradeName { get; }
        string Tooltip { get; }
        double Value { get; set; }
    }

    public class TradeAffix : ITradeAffix {
        private readonly UniqueItem m_uniqueItem;
        public TradeAffix() {}

        public TradeAffix(UniqueItem uniqueItem) { m_uniqueItem = uniqueItem; }
        public bool IsTotal { get; set; }
        public string PoeTradeName { get; set; }
        public string Tooltip { get; set; }
        public double Value { get; set; }

        public TradeAffix AddToPriceSet(string price) {
            if (m_uniqueItem != null && Value > 0) {
                if (m_uniqueItem.UniquePriceSets.ContainsKey(price)) {
                    m_uniqueItem.UniquePriceSets[price].Add(this);
                } else {
                    m_uniqueItem.UniquePriceSets.Add(price, new List<TradeAffix>() { this });
                }
            }

            return this;
        }
    }
}