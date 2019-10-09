using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Tornado.Overlay.Framework;

namespace Tornado.Overlay.Hud.Tooltip {
    public static class ClipboardService {
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern byte VkKeyScan(Char ch);

        private const uint KEYEVENTF_KEYUP = 0x0002;

        private static readonly TimeResolver<string> clipboardGetter = new TimeResolver<string>(new TimeSpan(0, 0, 0, 0, 100), GetText, TimeActionType.Periodical);
        public static string ClipBoardText => GetText();

        private static void SimulateCtrlV() {
            keybd_event(0xA2, 0x1D, 0, 0);
            keybd_event(VkKeyScan('C'), 0x1D, 0, 0);
            keybd_event(VkKeyScan('C'), 0x1D, KEYEVENTF_KEYUP, 0);
            keybd_event(0xA2, 0x1D, KEYEVENTF_KEYUP, 0);
        }

        public static string GetText() {
            string clipBoardText = "";
            SimulateCtrlV();

            Thread staThread = new Thread(delegate() {
                                              try {
                                                  clipBoardText = Clipboard.GetText();
                                              } catch (Exception) {
                                                  clipBoardText = "";
                                              }
                                          });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();

            return clipBoardText;
        }
    }
}