using System;
using System.Collections.Generic;

namespace PoeParser.Filter.Tooltip {
    public class ColoredLine {
        public List<ColoredText> TextCollection { get; }

        public ColoredLine(List<ColoredText> textCollection = null) {
            TextCollection = textCollection ?? new List<ColoredText>();
        }

        public ColoredLine(string line, string color, int fontSize = 15) {
            TextCollection = new List<ColoredText> { new ColoredText(line, color, fontSize) };
        }
    }

    public static class ColoredLineExtensions {
        public static ColoredLine Add(this ColoredLine source, string line, string color, int fontSize = 15, int width = 0, Align align = Align.left, int heightOffset = 0) {
            source.TextCollection.Add(new ColoredText(line, color, fontSize, width, align, heightOffset));
            return source;
        }

        public static List<ColoredLine> Add(this List<ColoredLine> sourceList, string line, string color, int fontSize = 15) {
            sourceList.Add(new ColoredLine(line, color, fontSize));
            return sourceList;
        }

        public static List<ColoredLine> If(this List<ColoredLine> sourceList, bool condition, Action<List<ColoredLine>> action) {
            if (condition) action(sourceList);
            return sourceList;
        }

        public static List<ColoredLine> AddEmptyLine(this List<ColoredLine> sourceList) {
            sourceList.Add(new ColoredLine("\r\n", ToolTipColor.GeneralGroup));
            return sourceList;
        }
    }
}