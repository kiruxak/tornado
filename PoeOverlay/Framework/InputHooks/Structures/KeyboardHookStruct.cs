﻿using System.Runtime.InteropServices;

namespace PoeOverlay.Framework.InputHooks.Structures {
    [StructLayout(LayoutKind.Sequential)]
    internal struct KeyboardHookStruct {
        public int VirtualKeyCode;

        public int ScanCode;

        public int Flags;

        public int Time;

        public int ExtraInfo;
    }
}