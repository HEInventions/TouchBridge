using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TouchBridge
{
    public class Program
    {
        /// <summary>
        /// Application entrypoint.
        /// </summary>
        /// <param name="args">Command line args, e.g.: ws://0.0.0.0:8282</param>
        private static void Main(string[] args)
        {
            // Preamble.
            Console.Error.WriteLine("TouchBridge started at " + DateTime.Now.ToString("ddMMyyyy-HHmm"));
            Console.Error.WriteLine("Hardy & Ellis Inventions LTD 2014");

            // Start the server.
            var pRepeater = new Repeater();
            if (args.Length > 0)
                pRepeater.Start(args[0]);
            else
                pRepeater.Start(Properties.Settings.Default.DefaultLocation);

            // Wait until exit is recieved.
            while (Console.ReadKey(true).KeyChar != 'Q')
            {
            }

            // Shut the server down cleanly.
            pRepeater.Stop();
            Console.Error.WriteLine("TouchBridge stopped by user at " + DateTime.Now.ToString("ddMMyyyy-HHmm"));
        }
    }
}
