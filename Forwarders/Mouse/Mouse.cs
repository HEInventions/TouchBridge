using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace TouchBridge.Forwarders.Mouse
{
    /// <summary>
    /// Singleton mouse controller.
    /// </summary>
    public class Mouse
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        /// <summary>
        /// Simluate a left mouse down in the current position.
        /// </summary>
        public static void LeftDown(int x, int y)
        {
            mouse_event(0x02, x, y, 0, 0);
        }

        /// <summary>
        /// Simluate a left mouse up in the current position.
        /// </summary>
        public static void LeftUp(int x, int y)
        {
            mouse_event(0x04, x, y, 0, 0);
        }
        /// <summary>
        /// Simulate a mouse move.
        /// </summary>
        /// <param name="x">The x-position in pixels.</param>
        /// <param name="y">The y-position in pixels.</param>
        public static void Move(int x, int y)
        {
            SetCursorPos(x, y);
        }
    }
}
