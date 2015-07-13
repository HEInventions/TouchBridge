using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TouchBridge.Forwarders.Win8
{
    /// <summary>
    /// Forward Windows8 touches.
    /// </summary>
    public class Win8Forwarder : IForwarder
    {
        /// <summary>
        /// The max number of touches at once.
        /// </summary>
        private const int MAX_TOUCHES = 10;

        /// <summary>
        /// The list of screens active when this display was started.
        /// </summary>
        private List<MonitorDetection.DisplayInfo> lScreens = MonitorDetection.QueryDisplays();

        /// <summary>
        /// The index of the screen we are using.
        /// </summary>
        private int iScreenIndex = 0;

        /// <summary>
        /// Construct a new Win8Forwarder for touch events from args.
        /// </summary>
        /// <param name="dArgs"></param>
        public Win8Forwarder(Dictionary<String, String> dArgs)
        {
            // Check Windows 8 Support.
            var Windows8 = new Version(6, 2, 9200, 0);
            if (!(Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version >= Windows8))
            {
                // Not windows 8.
                throw new Exception("Win8 TouchBridge requires Windows 8 or higher.");
            }

            // Start it up.
            TouchInjector.InitializeTouchInjection(MAX_TOUCHES, TouchFeedback.DEFAULT);

            // Pick a screen to inject onto.
            String sScreenIndex = null;
            if (!dArgs.TryGetValue("screen", out sScreenIndex))
                sScreenIndex = "0";

            if (!int.TryParse(sScreenIndex, out iScreenIndex))
                throw new ArgumentException("Cannot accept non-numeric screen index.");

            if (iScreenIndex < 0 || iScreenIndex >= lScreens.Count)
                throw new ArgumentException("Screen index out of range.");
        }

        /// <summary>
        /// Forward the given touch message.
        /// </summary>
        /// <param name="?"></param>
        public void Forward(TouchMessage Message)
        {
            const uint PRESSURE = 32000;
            const uint ORIENTATION = 0;
            const int RADIUS = 1;

            // Convert coords into appt. pixel space.
            int x = (int)(lScreens[iScreenIndex].WorkArea.Left + (Message.X * lScreens[iScreenIndex].WorkArea.Width));
            int y = (int)(lScreens[iScreenIndex].WorkArea.Top + (Message.Y * lScreens[iScreenIndex].WorkArea.Height));

            TouchInjector.InjectTouchInput(1, new PointerTouchInfo[] { TouchInjector.MakePointerTouchInfo(x, y, RADIUS, (uint)Message.ID, ORIENTATION, PRESSURE) });
        }

        /// <summary>
        /// Teardown
        /// </summary>
        public void Dispose()
        {
        }
    }
}
