using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using PoeParser.Filter.Tooltip;
using SharpDX;
using SharpDX.Direct3D9;
using Tornado.Overlay.Framework;
using Tornado.Overlay.Framework.Helpers;
using Tornado.Overlay.Framework.InputHooks;
using Tornado.Overlay.Hud.UI;
using Tornado.Parser;
using Tornado.Parser.Common.Extensions;
using Tornado.Parser.Filter;
using Tornado.Parser.Filter.Tooltip;
using Tornado.Parser.Parser;
using Tornado.Parser.PoeTrade;
using Rectangle = System.Drawing.Rectangle;

namespace Tornado.Overlay.Hud.Tooltip {
    public class ItemTooltipPlugin : Plugin {
        private static readonly object Locker = new object();
        private const string PRICE_RESPONSE_TEXT = "Price: waiting";

        private List<Tornado.Parser.Filter.Tooltip.ColoredLine> lines;
        private string clipboard = "";
        private bool holdKey;
        private FilterResult filter;
        private DateTime? tooltipHideDt = DateTime.Now;
        private DateTime _lastUpdate = DateTime.Now;
        private DateTime _lastPoeOpenClick = DateTime.Now;
        private bool isPriceLoaded = false;
        private Vector2 checkPosition = new Vector2(0, 0);
        private readonly int checkRadius;

        public FilterResult Filter {
            get {
                lock(Locker) {
                    var position = new Vector2(Cursor.Position.X - checkPosition.X, Cursor.Position.Y - checkPosition.Y).Length();

                    if ((DateTime.Now - _lastUpdate).Milliseconds < 100 && position < checkRadius) {
                        var x = ClipboardService.ClipBoardText;
                    }

                    if (position > checkRadius / 2) {
                        checkPosition = new Vector2(Cursor.Position.X, Cursor.Position.Y);
                        var bufferClipBoard = ClipboardService.ClipBoardText;
                        if (!string.Equals(clipboard, bufferClipBoard)) {
                            if (string.Equals(clipboard, bufferClipBoard))
                                return filter;
                            clipboard = bufferClipBoard;
                            filter = GetFilter();
                        }
                        _lastUpdate = DateTime.Now;
                    }
                }
                return filter;
            }
        }

        private FilterResult GetFilter() {
            filter = ItemParser.Parse(clipboard);
            if (filter == null) {
                return null;
            }

            tooltipHideDt = null;
            isPriceLoaded = false;
            lines = new ToolTipGenerator(filter).GetTooltip();
            lines.Insert(0, new ColoredLine(PRICE_RESPONSE_TEXT, "FFCDCDCD", 18));

            return filter;
        }

        public ItemTooltipPlugin(Graphics graphics, Rectangle bounds) : base(graphics) {
            checkRadius = bounds.Width / 120;
            lines = new List<ColoredLine>();
            KeyboardHook.KeyDown += onKeyDown;
            KeyboardHook.KeyUp += onKeyUp;
        }

        private void onKeyUp(KeyInfo obj) {
            if (obj.Keys == Keys.LShiftKey && !obj.Shift) {
                holdKey = false;
                if (tooltipHideDt == null)
                    tooltipHideDt = DateTime.Now;
            }
            if (WinApi.IsKeyDownAsync(Keys.F6)) {
                FilterResult curFilter = Filter;
                if (curFilter == null) {
                    return;
                }

                if (Monitor.TryEnter(this) && (DateTime.Now - _lastPoeOpenClick).TotalMilliseconds > 500) {
                    try {
                        curFilter.OpenPoeTrade();
                        _lastPoeOpenClick = DateTime.Now;
                    } finally {
                        Monitor.Exit(this);
                    }
                }
            }
            if (WinApi.IsKeyDownAsync(Keys.F7)) {
                Config.Debug = !Config.Debug;
            }
        }

        private void onKeyDown(KeyInfo obj) {
            if (obj.Shift) {
                holdKey = true;
            }
        }

        public override void Render() {
            if (holdKey) {
                FilterResult curFilter = Filter;
                if (curFilter == null) {
                    return;
                }

                if (tooltipHideDt.HasValue && (DateTime.Now - tooltipHideDt.Value).TotalMilliseconds > 5000) {
                    return;
                }
                DrawTooltip(curFilter);
            }
        }

        private void DrawTooltip(FilterResult curFilter) {
            if (curFilter.Item.Prices != null && curFilter.Item.Prices.Count > 0 && !isPriceLoaded) {
                isPriceLoaded = true;

                var price = curFilter.Item.Price;

                ColoredLine line = lines.ElementAt(0);
                line.TextCollection.Clear();
                line.TextCollection.Add(new ColoredText($"Price: ", "FFCDCDCD", 16));
                line.TextCollection.AddRange(price.GetPriceColoredLines(price.IsExpensive() ? "FF00DE21" : "FFCDCDCD", 16));
                line.TextCollection.AddRange(curFilter.Item.Prices.Skip(1).Take(4).SelectMany(p => {
                    var linex = p.GetPriceColoredLines("FFCDCDCD", 14, false);

                    foreach (var l in linex) {
                        l.HeightOffset = 1;
                    }
                    linex.Insert(0, new ColoredText(", ", "FFCDCDCD", 14, heightOffset: 1));
                    return linex;
                }));
            }

            var width = 0;
            var height = 0;
            var x = Cursor.Position.X - 6;
            var y = Cursor.Position.Y - 6;

            Dictionary<ColoredLine, int> blocks = new Dictionary<ColoredLine, int>();

            foreach (ColoredLine line in lines.ToList()) {
                var xHeght = 0;
                var xWidth = 0;
                foreach (ColoredText text in line.TextCollection) {
                    var size = Graphics.MeasureText(text.Line, text.FontSize);
                    xHeght = xHeght > size.Height ? xHeght : size.Height;
                    xWidth += text.Width > 0 ? text.Width : size.Width;
                }
                width = width > xWidth ? width : xWidth;
                height += xHeght;
                blocks.Add(line, xHeght);
            }

            if (x - width < 0) {
                x += (width - x + curFilter.BorderWidth);
            }
            if (y - height < 0) {
                y += (height - y + curFilter.BorderWidth);
            }

            var blockW = 0;
            foreach (KeyValuePair<ColoredLine, int> pair in blocks) {
                var xWidth = 0;
                foreach (ColoredText text in pair.Key.TextCollection) {
                    var size = Graphics.MeasureText(text.Line, text.FontSize);
                    var vector = new Vector2(x - width + 3 + xWidth + (text.Align == Align.right ? (text.Width - size.Width) : 0), y - blockW - text.HeightOffset + 3);
                    Graphics.DrawText(text.Line, text.FontSize, vector, text.Color.ToBGRAColor(), FontDrawFlags.Bottom);
                    xWidth += text.Width > 0 ? text.Width : size.Width;
                }
                blockW += pair.Value;
            }

            Graphics.DrawBox(new RectangleF(x - width - curFilter.BorderWidth, y - height - curFilter.BorderWidth, width + 6 + (2 * curFilter.BorderWidth), height + 6 + (2 * curFilter.BorderWidth)), curFilter.Color.ToBGRAColor());
            Graphics.DrawBox(new RectangleF(x - width, y - height, width + 6, height + 6), new Color(0.1f, 0.1f, 0.1f, 1.0f));
        }
    }
}