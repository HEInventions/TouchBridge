using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace TouchBridge
{
    /// <summary>
    /// Find out about which monitors are made available by the system.
    /// </summary>
    public abstract class MonitorDetection
    {
        #region Multiple Monitor Detection Code
        private delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData);

        [DllImport("user32.dll")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData);

        [StructLayout(LayoutKind.Sequential)]
        private struct RectNative
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MonitorInfo
        {
            public uint Size;
            public RectNative Monitor;
            public RectNative WorkArea;
            public uint Flags;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetMonitorInfo(IntPtr hmon, ref MonitorInfo mi);
        #endregion

        /// <summary>
        /// The struct that contains the display information
        /// </summary>
        public class DisplayInfo
        {
            public string Availability { get; set; }
            public int ScreenHeight { get; set; }
            public int ScreenWidth { get; set; }
            public Rect MonitorArea { get; set; }
            public Rect WorkArea { get; set; }
            public String DeviceName { get; set; }
        }

        /// <summary>
        /// Returns the number of Displays using the Win32 functions.
        /// </summary>
        /// <returns>A collection of DisplayInfo with information about each monitor.</returns>
        public static List<DisplayInfo> QueryDisplays()
        {
            // Output monitors.
            var lMonitors = new List<DisplayInfo>();

            // Get the monitors, for each one, add it to the list.
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
                delegate(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData)
                {
                    // Create a struct to contain the info and query it.
                    MonitorInfo mi = new MonitorInfo();
                    mi.Size = (uint)Marshal.SizeOf(mi);
                    mi.DeviceName = string.Empty;
                    bool bSuccess = GetMonitorInfo(hMonitor, ref mi);

                    // If it didn't fail, write the data.
                    if (bSuccess)
                    {
                        DisplayInfo di = new DisplayInfo();
                        di.ScreenWidth = mi.Monitor.Right - mi.Monitor.Left;
                        di.ScreenHeight = mi.Monitor.Bottom - mi.Monitor.Top;
                        di.MonitorArea = new Rect(mi.Monitor.Left, mi.Monitor.Top, di.ScreenWidth, di.ScreenHeight);
                        di.WorkArea = new Rect(mi.WorkArea.Left, mi.WorkArea.Top, (mi.WorkArea.Right - mi.WorkArea.Left), (mi.WorkArea.Bottom - mi.WorkArea.Top));
                        di.Availability = mi.Flags.ToString();
                        di.DeviceName = mi.DeviceName;
                        lMonitors.Add(di);
                    }
                    return true;
                }, IntPtr.Zero);
            return lMonitors;
        }
    }
}
