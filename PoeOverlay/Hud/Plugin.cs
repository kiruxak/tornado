using PoeOverlay.Hud.Interfaces;
using PoeOverlay.Hud.UI;

namespace PoeOverlay.Hud {
    public abstract class Plugin : IPlugin {
        protected readonly Graphics Graphics;
        protected Plugin(Graphics graphics) {
            Graphics = graphics;
        }

        public virtual void Dispose() {}
        public abstract void Render();
    }
}