using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Text;
using System.Collections.ObjectModel;

using HidLibrary;


namespace TouchBridge.Forwarders.Win7
{
    /// <summary>
    /// TouchInjection code for Windows7 based on the HID spec.
    /// http://msdn.microsoft.com/en-us/library/windows/hardware/ff553721
    /// </summary>
    /// <remarks>This requires HIDLibrary version 2.1.0.0.  The more up-to-date versions don't work due to some changes in the construtor.</remarks>
    public class Win7Forwarder : IForwarder
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
        /// HID Device Driver.
        /// </summary>
        private static HidDevice pDevice = null;

        /// <summary>
        /// The number of clients transmitting data to this device.
        /// </summary>
        private static int iDeviceReferences = 0;

        /// <summary>
        /// Create a new Win7 forwaderer.
        /// </summary>
        /// <param name="dArgs"></param>
        public Win7Forwarder(Dictionary<String, String> dArgs)
        {
            // If the HID device is not loaded.
            if (pDevice == null)
            {
                // Access the HID device driver.
                pDevice = HidDevices.Enumerate(0xdddd, 0x0001).FirstOrDefault();
                if (pDevice == null)
                    throw new InvalidOperationException("Touchscreen HID driver was not found. Personal/non-commercial projects can use the compatible UniSoftHID driver.");

                // Open the HID device.
                try
                {
                    pDevice.OpenDevice(DeviceMode.Overlapped, DeviceMode.NonOverlapped, ShareMode.ShareRead | ShareMode.ShareWrite);
                }
                catch (Exception e)
                {
                    pDevice = null;
                    throw e;
                }
            }

            // Increment device count.
            iDeviceReferences++;

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
        /// <param name="?">The touch message to forward it with.</param>
        public void Forward(TouchMessage Message)
        {
            const int HID_CONTACT_INFO_SIZE = 14;   // Number of bytes per contact report element.
            const int MaxSize = 32767;              // Highest 16 bit signed int --- used to compute touch point coordinates as a 0-1 mutiplier.
            const ushort PRESSURE = 1;              // Finger pressure.
            const ushort WIDTH = 1;                 // Finger width and height.
            const ushort HEIGHT = 1;

            // Create a report with two spaces + one byte for end char.
            var pReport = new HidReport(2 * HID_CONTACT_INFO_SIZE + 1);
            pReport.ReportId = 1;   // Multi-touch report id

            // Write out the report data.
            using (BinaryWriter pWriter = new BinaryWriter(new MemoryStream(pReport.Data)))
            {
                // Write the contact data in the correct format for the contact.
                if (Message.Type == TouchMessage.Nature.Down)
                {
                    // First contact.
                    pWriter.Write((ushort)2);                       // 
                    pWriter.Write((ushort)(Message.X * MaxSize));   // X
                    pWriter.Write((ushort)(Message.Y * MaxSize));   // Y
                    pWriter.Write(PRESSURE);                        // Pressure
                    pWriter.Write(WIDTH);                           // Width
                    pWriter.Write(HEIGHT);                          // Height
                    pWriter.Write((ushort)Message.ID);              // ID

                    // Write the rest full of 0s.
                    pWriter.Write(new byte[HID_CONTACT_INFO_SIZE]);

                    // If it is our first report then write the byte which contains the number in the report sequence.
                    pWriter.Write((byte)1);
                }

                else if (Message.Type == TouchMessage.Nature.Up)
                {
                    // First contact.
                    pWriter.Write((ushort)2);
                    pWriter.Write((ushort)(Message.X * MaxSize));   // X
                    pWriter.Write((ushort)(Message.Y * MaxSize));   // Y
                    pWriter.Write(PRESSURE);                        // Pressure
                    pWriter.Write(WIDTH);                           // Width
                    pWriter.Write(HEIGHT);                          // Height
                    pWriter.Write((ushort)Message.ID);              // ID

                    // Secnd contact.
                    pWriter.Write((ushort)0);
                    pWriter.Write((ushort)(Message.X * MaxSize));   // X
                    pWriter.Write((ushort)(Message.Y * MaxSize));   // Y
                    pWriter.Write(PRESSURE);                        // Pressure
                    pWriter.Write(WIDTH);                           // Width
                    pWriter.Write(HEIGHT);                          // Height
                    pWriter.Write((ushort)Message.ID);              // ID

                    // If it is our first report then write the byte which contains the number in the report sequence.
                    pWriter.Write((byte)2);
                }

                else if (Message.Type == TouchMessage.Nature.Move)
                {
                    // First contact.
                    pWriter.Write((ushort)3);
                    pWriter.Write((ushort)(Message.X * MaxSize));   // X
                    pWriter.Write((ushort)(Message.Y * MaxSize));   // Y
                    pWriter.Write(PRESSURE);                        // Pressure
                    pWriter.Write(WIDTH);                           // Width
                    pWriter.Write(HEIGHT);                          // Height
                    pWriter.Write((ushort)Message.ID);              // ID

                    // Write the rest full of 0s.
                    pWriter.Write(new byte[HID_CONTACT_INFO_SIZE]);

                    // If it is our first report then write the byte which contains the number in the report sequence.
                    pWriter.Write((byte)1);
                }
            }

            // Push it to the driver.
            pDevice.WriteReport(pReport);
        }
        
        /// <summary>
        /// Teardown.
        /// </summary>
        public void Dispose()
        {
            // Subtract references.
            iDeviceReferences--;

            // Close if 0;
            if (iDeviceReferences == 0 && pDevice != null)
            {
                pDevice.CloseDevice();
                pDevice.Dispose();
            }
            pDevice = null;
        }
    }
}
