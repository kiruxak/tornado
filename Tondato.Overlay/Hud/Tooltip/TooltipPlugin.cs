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
using Tornado.Parser.Entities;
using Tornado.Parser.Filter;
using Tornado.Parser.Filter.Tooltip;
using Tornado.Parser.Parser;
using Tornado.Parser.PoeTrade;
using Tornado.Parser.PoeTrade.Data;
using Rectangle = System.Drawing.Rectangle;

namespace Tornado.Overlay.Hud.Tooltip {
    public class ItemTooltipPlugin : Plugin {
        private static readonly object Locker = new object();
        private const string PRICE_RESPONSE_TEXT = "Price: waiting";

        private List<ColoredLine> lines;
        private string clipboard = "";
        private bool holdKey;
        private bool holdAlt;
        private bool holdZ;
        private FilterResult filter;
        private DateTime? tooltipHideDt = DateTime.Now;
        private DateTime _lastUpdate = DateTime.Now;
        private DateTime _lastPoeOpenClick = DateTime.Now;
        private List<string> priceLoaded = new List<string>();
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
            filter = ItemParser.Parse(clipboard, holdAlt);
            if (filter == null) {
                return null;
            }

            tooltipHideDt = null;
            priceLoaded = new List<string>();
            lines = new ToolTipGenerator(filter).GetTooltip();
            lines.Insert(0, new ColoredLine(PRICE_RESPONSE_TEXT, "FFCDCDCD", 18));

            return filter;
        }

        public ItemTooltipPlugin(Graphics graphics, Rectangle bounds) : base(graphics) {
            checkRadius = bounds.Width / 120;
            lines = new List<ColoredLine>();
            priceLoaded = new List<string>();
            KeyboardHook.KeyDown += onKeyDown;
            KeyboardHook.KeyUp += onKeyUp;
        }

        private void onKeyUp(KeyInfo obj) {
            if (obj.Keys == Keys.LShiftKey && !obj.Shift) {
                holdKey = false;
                if (tooltipHideDt == null)
                    tooltipHideDt = DateTime.Now;
            }
            if (!obj.Alt) {
                holdAlt = false;
            }
            if (obj.Keys == Keys.Z)
            {
                holdZ = false;
            }
            if (WinApi.IsKeyDownAsync(Keys.F6)) {
                FilterResult curFilter = Filter;
                if (curFilter == null) { return; }

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
                FilterResult curFilter = Filter;
                if (curFilter == null) { return; }

                if (Monitor.TryEnter(this) && (DateTime.Now - _lastPoeOpenClick).TotalMilliseconds > 500) {
                    try {
                        curFilter.OpenBasePoeTrade();
                        _lastPoeOpenClick = DateTime.Now;
                    } finally {
                        Monitor.Exit(this);
                    }
                }
            }
            if (WinApi.IsKeyDownAsync(Keys.F8)) {
                FilterResult curFilter = Filter;
                if (curFilter == null) { return; }

                if (Monitor.TryEnter(this) && (DateTime.Now - _lastPoeOpenClick).TotalMilliseconds > 500) {
                    try {
                        curFilter.OpenSyncPoeTrade();
                        _lastPoeOpenClick = DateTime.Now;
                    }
                    finally {
                        Monitor.Exit(this);
                    }
                }
            }

            if (WinApi.IsKeyDownAsync(Keys.F12)) {
                Config.Debug = !Config.Debug;
            }
        }

        private void onKeyDown(KeyInfo obj) {
            if (obj.Shift) {
                holdKey = true;
            }
            if (obj.Alt) {
                holdAlt = true;
            }
            if (obj.Keys == Keys.Z) {
                holdZ = true;
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
            foreach (var price in curFilter.Item.Prices.ToList()) {
                if (priceLoaded.Contains(price.Key)) {
                    continue;
                }

                var curPrice = price.Value.FirstOrDefault();

                ColoredLine line = priceLoaded.Count == 0 ? lines.ElementAt(0) : new ColoredLine();
                line.TextCollection.Clear();
                line.TextCollection.Add(new ColoredText($"{price.Key}:", "FFCDCDCD", 16));
                line.TextCollection.AddRange(curPrice.GetPriceColoredLines(curPrice.IsExpensive() ? "FF00DE21" : "FFCDCDCD", 16));
                line.TextCollection.AddRange(price.Value.Skip(1)
                                                  .Take(4)
                                                  .SelectMany(p => {
                                                      var linex = p.GetPriceColoredLines("FFCDCDCD", 14, false);
                                                      foreach (var l in linex) { l.HeightOffset = 1; }
                                                      linex.Insert(0, new ColoredText(",", "FFCDCDCD", 14, heightOffset: 1));
                                                      return linex;
                                                  }));
                if (filter.Item.Rarity != ItemRarity.Rare || filter.Item.Type == ItemType.Jewel || filter.Item.Type == ItemType.AbyssalJewel || filter.Item.Type == ItemType.Map || price.Key.StartsWith("Base Price")) {
                    var pricesCount = price.Value.Count;
                    var avgPrice = pricesCount > 20
                            ? price.Value.OrderBy(c => c.Currency.ValueInChaos)
                                   .Skip((int)(pricesCount * 0.2))
                                   .Take((int)(pricesCount * 0.6))
                                   .Average(c => c.Currency.ValueInChaos)
                            : price.Value.OrderBy(c => c.Currency.ValueInChaos).Average(c => c.Currency.ValueInChaos);
                    var exalt = Currency.Data.lines?.FirstOrDefault(c => c.currencyTypeName == Currency.CurrencyName[CurrencyType.exalted]);
                    var prefix = "c";
                    if (avgPrice > exalt?.chaosEquivalent * 3) {
                        avgPrice = avgPrice / exalt.chaosEquivalent;
                        prefix = "exa";
                    }
                    if (avgPrice > 0 && (prefix == "exa" || pricesCount > 1)) { 
                        line.TextCollection.Add(new ColoredText($"   Avg[{price.Value.Count}]:~{avgPrice:F2}{prefix}", avgPrice > 1.9 ? "FF00DE21" : "FFCDCDCD", 16));
                    }
                }
                if (priceLoaded.Count > 0) {
                    lines.Insert(0, line);
                }
                priceLoaded.Add(price.Key);
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