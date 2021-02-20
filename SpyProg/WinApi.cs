using System;
using System.Text;
using System.Runtime.InteropServices;

namespace SpyProg {
    static class WinApi {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        public static string GetActiveWindowName() {
            IntPtr handle = GetForegroundWindow();

            const int count = 512;
            var currentWindow = new StringBuilder(count);
            GetWindowText(handle, currentWindow, count);

            return currentWindow.ToString().ToLower();
        }
    }
}
