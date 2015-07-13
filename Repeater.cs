using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using Fleck;

using TouchBridge.Forwarders.TUIO;
using TouchBridge.Forwarders.Console;
using TouchBridge.Forwarders.Win7;
using TouchBridge.Forwarders.Win8;
using TouchBridge.Forwarders.Mouse;


namespace TouchBridge
{
    /// <summary>
    /// WebSocket forwarder. Accepts incomming connections and forwards the event the appropriate generator.
    /// </summary>
    public class Repeater
    {
        /// <summary>Threasafe access to the sockets list.</summary>
        //private static Mutex mSocketLock = new Mutex();

        /// <summary>Table of active sockets to forwarder commands.</summary>
        private static ConcurrentDictionary<IWebSocketConnection, IForwarder> dSockets = new ConcurrentDictionary<IWebSocketConnection, IForwarder>();

        /// <summary>Web socket server reference.</summary>
        private WebSocketServer Server;

        /// <summary>
        /// Is the server running.
        /// </summary>
        public bool Running { get; private set; }

        /// <summary>
        /// The base URI that this repeater listens for connections on.
        /// If the server is not started, this returns null.
        /// </summary>
        public String Host { get { return (Server == null) ? null : Server.Location + ":" + Server.Port; } }

        /// <summary>
        /// Start the server listening.
        /// </summary>
        /// <param name="sHostURI"></param>
        public void Start(String sHostURI)
        {
            // Stop it.
            Stop();

            // Start the server listening on the port specified by args.
            Server = new WebSocketServer(sHostURI);
            Server.Start(OnClient);
            Running = true;
        }

        /// <summary>
        /// Stop the server listening.
        /// </summary>
        public void Stop()
        {
            // Kill the server.
            if (Server != null)
                Server.Dispose();
            Server = null; 

            // Update flags.
            Running = false;
        }

        /// <summary>
        /// When a client is connected, bind all relevant events.
        /// </summary>
        /// <param name="Socket"></param>
        private void OnClient(IWebSocketConnection Socket)
        {
            // Handle new socket opens.
            Socket.OnOpen = () =>
            {
                Console.Error.WriteLine("[II] Repeater Open:   " + Socket.ConnectionInfo.ClientIpAddress + Socket.ConnectionInfo.Path);

                try
                {
                    // Extract repeater arguments from URL.
                    var sPath = Socket.ConnectionInfo.Path.ToLowerInvariant();
                    var dArgs = Regex.Matches(sPath, "([^?=&]+)(=([^&]*))?").Cast<Match>().ToDictionary(x => x.Groups[1].Value, x => x.Groups[3].Value);

                    // Handle based on the available types.
                    if (sPath.StartsWith("/tuio"))
                    {
                        dSockets[Socket] = new TUIOForwarder(dArgs);
                    }
                    else if (sPath.StartsWith("/console"))
                    {
                        dSockets[Socket] = new ConsoleForwarder();
                    }
                    else if (sPath.StartsWith("/win7"))
                    {
                        dSockets[Socket] = new Win7Forwarder(dArgs);
                    }
                    else if (sPath.StartsWith("/win8"))
                    {
                        dSockets[Socket] = new Win8Forwarder(dArgs);
                    }
                    else if (sPath.StartsWith("/mouse"))
                    {
                        dSockets[Socket] = new MouseForwarder(dArgs);
                    }
                    else
                    {
                        throw new ArgumentException("Unsupported Repeater");
                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.Message);
                    Socket.Close();
                }
            };

            // Handle socket closures.
            Socket.OnClose = () =>
            {
                Console.Error.WriteLine("[II] Repeater Closed: " + Socket.ConnectionInfo.ClientIpAddress + Socket.ConnectionInfo.Path);

                // Clean up.
                IForwarder pOut;
                if (dSockets.TryRemove(Socket, out pOut))
                {
                    if (pOut != null)
                        pOut.Dispose();
                }
            };

            // Handle socket errors.
            Socket.OnError = (Exception e) =>
            {
                Console.WriteLine("[WW] Repeater Socket Error: " + e.Message);
                Socket.Close();
            };

            // Listen for socket commands.
            Socket.OnMessage = (string data) =>
            {
                // Parse the message out.
                var pMessage = TouchMessage.FromString(data);
                if (pMessage.Valid == false)
                {
                    //Console.Error.WriteLine("[WW] Touch Invalid: " + data);
                    return;
                }

                // Find the forwarder.
                IForwarder pFwd;
                if (dSockets.TryGetValue(Socket, out pFwd))
                {
                    if (pFwd != null)
                        pFwd.Forward(pMessage);
                }
            };
        }
    }
}
