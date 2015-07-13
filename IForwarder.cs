using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TouchBridge
{
    /// <summary>
    /// Defines a class able to forward a touch event to another format or service.
    /// </summary>
    public interface IForwarder : IDisposable
    {
        /// <summary>
        /// Forward the given touch message.
        /// </summary>
        /// <param name="?"></param>
        void Forward(TouchMessage Message);
    }
}
