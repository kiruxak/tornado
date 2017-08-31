using System;

namespace PoeOverlay.Hud.Interfaces {
    public interface IPlugin : IDisposable {
        void Render();
    }
}