using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TouchBridge.Forwarders.Mouse
{
    /// <summary>
    /// Write touch messages out to the OS with mouse events.
    /// </summary>
    public class MouseForwarder : IForwarder
    {
        /// <summary>
        /// The list of screens active when this display was started.
        /// </summary>
        private List<MonitorDetection.DisplayInfo> lScreens = MonitorDetection.QueryDisplays();

        /// <summary>
        /// The index of the screen we are using.
        /// </summary>
        private int iScreenIndex = 0;

        /// <summary>
        /// Construct a new mouse forwarder.
        /// </summary>
        /// <param name="dArgs"></param>
        public MouseForwarder(Dictionary<String, String> dArgs)
        {
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
            if (Message != null)
                System.Console.WriteLine(Message.ToString());

            // Convert coords into appt. pixel space.
            int x = (int)( lScreens[iScreenIndex].WorkArea.Left + (Message.X * lScreens[iScreenIndex].WorkArea.Width)   );
            int y = (int)( lScreens[iScreenIndex].WorkArea.Top  + (Message.Y * lScreens[iScreenIndex].WorkArea.Height)  );

            // Handle move.
            if (Message.Type == TouchMessage.Nature.Move)
            {
                Mouse.Move(x, y);
            }

            // Handle down.
            else if (Message.Type == TouchMessage.Nature.Down)
            {
                Mouse.Move(x, y);
                Mouse.LeftDown(x, y);
            }

            // Handle up.
            else if (Message.Type == TouchMessage.Nature.Up)
            {
                Mouse.Move(x, y);
                Mouse.LeftUp(x,y);
            }
        }

        /// <summary>
        /// Delete this console forwarder.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
