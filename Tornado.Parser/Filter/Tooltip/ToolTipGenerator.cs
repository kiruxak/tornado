using System.Collections.Generic;
using Tornado.Parser.Filter;
using Tornado.Parser.Filter.Tooltip;

namespace PoeParser.Filter.Tooltip {
    public class ToolTipGenerator {
        public FilterResult FilterResult;

        public ToolTipGenerator(FilterResult result) {
            FilterResult = result;
        }

        public List<ColoredLine> GetTooltip() {
            List<ColoredLine> lines = FilterResult.GetTooltip();
            lines.AddEmptyLine();

            lines.Reverse();
            return lines;
        }
    }
}