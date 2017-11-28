using System;

namespace Tornado.Overlay.Hud.Interfaces {
    public interface IPlugin : IDisposable {
        void Render();
    }
}