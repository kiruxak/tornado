using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using PoeParser;
using SharpDX.Windows;
using Tornado.Overlay.Framework;
using Tornado.Overlay.Hud.Interfaces;
using Tornado.Overlay.Hud.Notification;
using Tornado.Overlay.Hud.Tooltip;
using Tornado.Parser;
using Graphics2D = Tornado.Overlay.Hud.UI.Graphics;

namespace Tornado.Overlay.Hud {
    class ExternalOverlay : RenderForm {
        private readonly Func<bool> gameEnded;
        private readonly IntPtr gameHandle;
        private readonly List<IPlugin> plugins = new List<IPlugin>();
        private Graphics2D graphics;

        public ExternalOverlay(IntPtr gameHandle, Func<bool> gameEnded) {
            this.gameEnded = gameEnded;
            this.gameHandle = gameHandle;

            SuspendLayout();
            TransparencyKey = Color.Transparent;
            BackColor = Color.Black;
            FormBorderStyle = FormBorderStyle.None;
            ShowIcon = false;
            TopMost = true;
            ResumeLayout(false);
            Load += OnLoad;
        }

        public sealed override Color BackColor {
            get { return base.BackColor; }
            set { base.BackColor = value; }
        }

        private async void CheckGameState() {
            while (!gameEnded()) {
                await Task.Delay(500);
            }
            graphics.Dispose();
            Close();
        }

        private async void CheckGameWindow() {
            while (!gameEnded()) {
                await Task.Delay(1000);
                Rectangle gameSize = WinApi.GetClientRectangle(gameHandle);
                Bounds = gameSize;
            }
        }

        private void OnClosing(object sender, FormClosingEventArgs e) {
            plugins.ForEach(plugin => plugin.Dispose());
            graphics.Dispose();
        }

        private void OnDeactivate(object sender, EventArgs e) {
            BringToFront();
        }

        private async void OnLoad(object sender, EventArgs e) {
            Bounds = WinApi.GetClientRectangle(gameHandle);
            WinApi.EnableTransparent(Handle, Bounds);
            graphics = new Graphics2D(this, Bounds.Width, Bounds.Height);

            plugins.Add(new ItemTooltipPlugin(graphics, Bounds));
            if (Config.ShowNotification)
                plugins.Add(new ItemNotification(graphics, Bounds));

            Deactivate += OnDeactivate;
            FormClosing += OnClosing;

            CheckGameWindow();
            CheckGameState();
            graphics.Render += OnRender;
            await Task.Run(() => graphics.RenderLoop());
        }

        private void OnRender() {
            if (WinApi.IsForegroundWindow(gameHandle))
                plugins.ForEach(x => x.Render());
        }
    }
}