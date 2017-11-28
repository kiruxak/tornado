using Tornado.Overlay.Hud.Interfaces;
using Tornado.Overlay.Hud.UI;

namespace Tornado.Overlay.Hud {
    public abstract class Plugin : IPlugin {
        protected readonly Graphics Graphics;

        protected Plugin(Graphics graphics) {
            Graphics = graphics;
        }

        public virtual void Dispose() {
        }

        public abstract void Render();
    }
}