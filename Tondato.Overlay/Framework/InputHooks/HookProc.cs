using System;

namespace Tornado.Overlay.Framework.InputHooks {
    public delegate int HookProc(int nCode, int wParam, IntPtr lParam);
}