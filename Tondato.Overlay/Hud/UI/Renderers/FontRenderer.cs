﻿using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct3D9;
using Tornado.Overlay.Framework.Helpers;

namespace Tornado.Overlay.Hud.UI.Renderers {
    public sealed class FontRenderer : IDisposable {
        private readonly Device device;
        private readonly Sprite sprite;
        private readonly Dictionary<Tuple<string, int>, Font> fonts;

        public FontRenderer(Device device) {
            this.device = device;
            sprite = new Sprite(device);
            fonts = new Dictionary<Tuple<string, int>, Font>();
        }

        public void Begin() {
            sprite.Begin(SpriteFlags.AlphaBlend | SpriteFlags.SortTexture);
        }

        public Size2 DrawText(string text, string fontName, int height, Vector2 position, Color color, FontDrawFlags align) {
            try {
                var font = GetFont(fontName, height);
                var rectangle = new Rectangle((int)position.X, (int)position.Y, 0, 0);
                Rectangle fontDimension = font.MeasureText(null, text, rectangle, align);
                if (!sprite.IsDisposed)
                    font.DrawText(sprite, text, fontDimension, align, color);
                return new Size2(fontDimension.Width, fontDimension.Height);
            } catch (Exception) {
                //
            }
            return new Size2();
        }

        public void End() {
            sprite.End();
        }

        public void Flush() {
            lock(fonts) {
                fonts.ForEach((key, font) => font.Dispose());
            }
            lock(fonts) {
                fonts.Clear();
            }
        }

        public Size2 MeasureText(string text, string fontName, int height, FontDrawFlags align) {
            Font font = GetFont(fontName, height);
            Rectangle fontDimension = font.MeasureText(null, text, align);
            return new Size2(fontDimension.Width, fontDimension.Height);
        }

        public void Dispose() {
            sprite.Dispose();
            Flush();
        }

        private Font GetFont(string name, int height) {
            lock(fonts) {
                Font font;
                Tuple<string, int> key = Tuple.Create(name, height);
                if (fonts.TryGetValue(key, out font))
                    return font;
                font = new Font(device, new FontDescription {
                    MipLevels = 1,
                    Height = height,
                    FaceName = name,
                    OutputPrecision = FontPrecision.Default,
                    Quality = FontQuality.ClearType
                });
                font.PreloadText(name);
                fonts.Add(key, font);
                return font;
            }
        }
    }
}