using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TouchBridge.Forwarders.Console
{
    /// <summary>
    /// Write touch messages out to the console.
    /// </summary>
    public class ConsoleForwarder : IForwarder
    {
        /// <summary>
        /// Forward the given touch message.
        /// </summary>
        /// <param name="?"></param>
        public void Forward(TouchMessage Message)
        {
            if (Message != null)
                System.Console.WriteLine(Message.ToString());
        }

        /// <summary>
        /// Delete this console forwarder.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
