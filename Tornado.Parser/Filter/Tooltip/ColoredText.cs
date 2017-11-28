using System.Collections.Generic;

namespace Tornado.Parser.Filter.Tooltip {
    public enum Align {
        left,
        right
    }

    public class ColoredText {
        public string Line { get; set; }
        public string Color { get; set; }
        public int FontSize { get; set; }
        public int Width { get; set; }
        public Align Align { get; set; }
        public int HeightOffset { get; set; }

        public ColoredText(string line, string color, int fontSize = 15, int width = 0, Align align = Align.left, int heightOffset = 0) {
            Line = line;
            Color = color;
            FontSize = fontSize;
            Width = width;
            Align = align;
            HeightOffset = heightOffset;
        }
    }

    public static class ColoredTextExtensions {
        public static List<ColoredText> Add(this List<ColoredText> sourceList, string line, string color, int fontSize = 15, int width = 0, Align align = Align.left, int heightOffset = 0) {
            sourceList.Add(new ColoredText(line, color, fontSize, width, align, heightOffset));
            return sourceList;
        }
    }
}