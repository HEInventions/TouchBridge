using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSC.NET;

namespace TouchBridge.Forwarders.TUIO
{
    /// <summary>
    /// Create a TUIO server that forwards events on to a specific URL etc.
    /// </summary>
    /// <remarks>
    /// This is able to offset and scale the coordinates from 0-1 space to those of your choosing.
    /// The default port is 5000.
    /// </remarks>
    public class TUIOForwarder : IForwarder
    {
        /// <summary>
        /// A reference to an OSC data transmitter.
        /// </summary>
        private OSCTransmitter pUDPWriter = null;

        /// <summary>
        /// The number of data frames transmitted.
        /// </summary>
        private static int iFrame = 0;

        /// <summary>
        /// Construct a new TUIO forwarder class.
        /// </summary>
        /// <param name="dArgs">Table of optional arguments.</param>
        /// <remarks>
        /// Optional args are:
        ///    port -- The port to transmit on
        ///    host -- The host to transmit from
        ///    
        /// </remarks>
        public TUIOForwarder(Dictionary<String, String> dArgs)
        {
            // Extract path arguments.
            String port, host;
            dArgs.TryGetValue("port", out port);
            dArgs.TryGetValue("host", out host);

            // Start the UDP server.
            connectTransmitter(host == null ? "localhost" : host, port == null ? "3333" : port);
        }

        #region UDP TUIO
        /// <summary>
        /// Connect the UDP transmitter using the port and IP specified above.
        /// </summary>
        /// <returns></returns>
        private bool connectTransmitter(String Host, String Port)
        {
            try
            {
                // Close any open connections.
                disconnectTransmitter();

                // Reconnect with the new API.
                pUDPWriter = new OSCTransmitter(Host, Int32.Parse(Port));
                pUDPWriter.Connect();
                return true;
            }
            catch (Exception pError)
            {
                // Tear down.
                try
                {
                    this.disconnectTransmitter();
                }
                catch { }

                // Report the error.
                System.Console.Error.WriteLine("TUIO Connection Error: " + pError.Message);
                return false;
            }
        }

        /// <summary>
        /// Disconnect the UDP Transmitter.
        /// </summary>
        /// <returns></returns>
        private void disconnectTransmitter()
        {
            // Close any open connections.
            if (pUDPWriter != null)
                pUDPWriter.Close();
            pUDPWriter = null;
        }
        #endregion

        /// <summary>
        /// Forward the given touch message.
        /// </summary>
        /// <param name="?"></param>
        public void Forward(TouchMessage Message)
        {
            // Skip bad messages
            if (Message == null)
                return;

            // If we have a bad startup condition.
            if (pUDPWriter == null)
                return;

            // Create an new TUIO Bundle
            OSCBundle pBundle = new OSCBundle();

            // Create a fseq message and save it.  This is to associate a unique frame id with a bundle of SET and ALIVE.
            OSCMessage pMessageFseq = new OSCMessage("/tuio/2Dcur");
            pMessageFseq.Append("fseq");
            pMessageFseq.Append(++iFrame);
            pBundle.Append(pMessageFseq);

            // Create a alive message.
            OSCMessage pMessageAlive = new OSCMessage("/tuio/2Dcur");
            pMessageAlive.Append("alive");

            // Compile the set message.
            OSCMessage pMessage = new OSCMessage("/tuio/2Dcur");
            pMessage.Append("set");                             // set
            pMessage.Append((int)Message.ID);                   // session
            pMessage.Append((float)Message.X);                  // x
            pMessage.Append((float)Message.Y);                  // y
            pMessage.Append(0f);                                // dx
            pMessage.Append(0f);                                // dy
            pMessage.Append(0f);                                // motion
            pMessage.Append(1f);                                // height
            pMessage.Append(1f);                                // width

            // Append it to the bundle.
            pBundle.Append(pMessage);

            // Append the alive message for this contact to tbe bundle.
            pMessageAlive.Append((int)Message.ID);

            // Save the alive message.
            pBundle.Append(pMessageAlive);

            // Send the message off.
            this.pUDPWriter.Send(pBundle);
        }

        /// <summary>
        /// Close the TUIO server.
        /// </summary>
        public void Dispose()
        {
            disconnectTransmitter();
        }
    }
}
