using System;

namespace PoeOverlay.Framework.InputHooks {
    public delegate int HookProc(int nCode, int wParam, IntPtr lParam);
}