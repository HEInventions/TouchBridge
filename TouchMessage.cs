using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TouchBridge
{
    /// <summary>
    /// A TouchMessage is constructed from a string that is forwarded on out somewhere.
    /// </summary>
    public class TouchMessage
    {
        /// <summary>
        /// The kind of touch that this reprents. u = Up, d = Down, m = Move
        /// </summary>
        public enum Nature { Up, Down, Move, Unknown };

        /// <summary>
        /// The type of the touch.
        /// </summary>
        public Nature Type { get; internal set; }
        /// <summary>X position of the touch.</summary>
        public double X { get; internal set; }
        /// <summary>Y position of the touch.</summary>
        public double Y { get; internal set; }
        /// <summary>Z position of the touch.</summary>
        public double Z { get; internal set; }
        /// <summary>The ID of the touch (used for connecting ups, downs, and ends)</summary>
        public int ID { get; internal set; }
        /// <summary>Any user data that came with it</summary>
        public String UserData { get; internal set; }
        /// <summary>Source data</summary>
        public String Source { get; internal set; }
        /// <summary>Does this touch have all the necessary valid data.</summary>
        public bool Valid { get; internal set; }
        

        /// <summary>
        /// Internal constructor.
        /// </summary>
        internal TouchMessage()
        {
        }

        /// <summary>
        /// Return the source string used to create this touch message.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Source;
        }

        /// <summary>
        /// Parse a string in the format "#type|id|x|y|z" OR "#type|id|x|y|z|userdata". id is an integer, type is one of 'u', 'd', or 'm' (up, down, move).
        /// </summary>
        /// <param name="Data"></param>
        public static TouchMessage FromString(String Data)
        {
            // New packet.
            var pMessage = new TouchMessage() { Valid = false };

            // Source data.
            pMessage.Source = Data;

            try
            {
                // Constants.
                const char SPLIT = '|';     // delimiter
                const string MAGIC = "#";   // magic start
                const int VARS_NO_USER = 5; // number of data items excluding user data
                const int VARS_W_USER = VARS_NO_USER + 1;  // number of data items including user data

                // Verify magic.
                if (Data == null || !Data.StartsWith(MAGIC))
                    return pMessage;

                // Consume magic start.
                Data = Data.Substring(1);

                // Parse information out.
                var tParts = Data.Split(SPLIT);
                if (tParts.Length == VARS_NO_USER || tParts.Length == VARS_W_USER)
                {
                    var t = tParts[0].ToString();                       // type
                    pMessage.Type = (t == "u") ? Nature.Up : (t == "d") ? Nature.Down : (t == "m") ? Nature.Move : Nature.Unknown;

                    pMessage.ID = int.Parse(tParts[1].ToString());      // id
                    pMessage.X = double.Parse(tParts[2].ToString());    // x
                    pMessage.Y = double.Parse(tParts[3].ToString());    // y
                    pMessage.Z = double.Parse(tParts[4].ToString());    // z

                    if (tParts.Length == VARS_W_USER)                   // userdata
                        pMessage.UserData = tParts[5].ToString();
                }
                else
                {
                    return pMessage;
                }

                // If the type is unknown.
                if (pMessage.Type == Nature.Unknown)
                    return pMessage;

                // Say we are valid.
                pMessage.Valid = true;
            }
            finally
            {
                // Log malformed message string.
            }

            // Return the message.
            return pMessage;
        }
    }
}
