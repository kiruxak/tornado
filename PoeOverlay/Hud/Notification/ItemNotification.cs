using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using PoeOverlay.Framework;
using PoeOverlay.Framework.Helpers;
using PoeOverlay.Hud.UI;
using PoeParser;
using PoeParser.Common.Extensions;
using PoeParser.Filter;
using PoeParser.Parser;
using SharpDX;
using Rectangle = System.Drawing.Rectangle;

namespace PoeOverlay.Hud.Notification {
    public class ItemNotification : Plugin {
        public List<FilterResult> Items => itemResolver.Value;

        private Rectangle bounds;
        private readonly TimeResolver<List<FilterResult>> itemResolver = new TimeResolver<List<FilterResult>>(new TimeSpan(0, 0, 0, 0, 100), getItems, TimeActionType.Periodical);

        public ItemNotification(Graphics graphics, Rectangle bounds) : base(graphics) {
            this.bounds = bounds;
        }

        private static List<FilterResult> getItems() {
            return ItemParser.GetCachedItems()
                    .Where(f => f.Item?.Price != null &&
                    ((f.Item.Price?.Currency.Value == 0 && f.Item.Price.IsExpensive()) || (f.Item.Prices.Select(p => p.Currency.ValueInChaos).Take(5).Average() > Config.MinPrice)))
                    .ToList();
        }

        public override void Render() {
            if (WinApi.IsKeyDownAsync(Keys.F3)) { ItemParser.ClearCachedItems(); }

            int x = bounds.Width / 2 - 100;
            int y = 0;

            var dotbounds = new RectangleF(0, 0, 10, 10);
            Graphics.DrawImage("dot_green.png", dotbounds, "FF0EF029".ToBGRAColor());

            var items = Items;
            if (items == null || items.Count == 0) { return; }

            var lines = items.Select(s => new LineBlock(s.Item + (s.Value > 0 ? ", " + (s.Value*100).ToString("0.#") + "%" : ""),color:s.Color)).OfType<IDrawBlock>().ToList();
            lines.Reverse();
            var lineSize = Graphics.MeasureText(((LineBlock)lines.Last()).Line, 15);

            Size2 size = DrawBlock.Draw(x, y, Graphics, lineSize.IsMouseHover(x, y) ? lines : new List<IDrawBlock> { lines.First() });

            var color = new Color(0.1f, 0.1f, 0.1f, 1.0f);
            Graphics.DrawBox(new RectangleF(x , y , size.Width, size.Height), color);
        }
    }

    public class LineBlock : IDrawBlock {
        public string Line { get; set; }
        public int FontSize { get; set; }
        public string Color { get; set; }

        public LineBlock(string line, int fontSize = 15, string color = "#FFFFFFFF") {
            FontSize = fontSize;
            Color = color;
            Line = line;
        }

        public Size2 Draw(int x, int y, Graphics g) {
            return g.DrawText(Line, 15, new Vector2(x, y), Color.ToBGRAColor());
        }
    }

    public interface IDrawBlock {
        Size2 Draw(int x, int y, Graphics g);
    }

    public static class DrawBlock {
        public static Size2 Draw(int x, int y, Graphics g, List<IDrawBlock> blocks) {
            var height = 0;
            var width = 0;

            foreach (IDrawBlock block in blocks) {
                var size = block.Draw(x, y + height, g);
                height += size.Height;
                width = width > size.Width ? width : size.Width;
            }

            return new Size2(width, height);
        }
    }

    public static class Size2Extensions {
        public static RectangleF GetRectangle(this Size2 size, int x, int y) {
            return new RectangleF(x,y,size.Width, size.Height);
        }

        public static bool IsMouseHover(this Size2 size, int x, int y) {
            return size.GetRectangle(x,y).Contains(Cursor.Position.X, Cursor.Position.Y);
        }
    }
}
