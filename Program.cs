using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Timers;
using System.Management;
using System.IO;

namespace DisableAlerts
{
    class Program
    {
        public delegate bool EnumDelegate(IntPtr hWnd, int lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumDelegate lpEnumCallbackFunction, IntPtr lParam);

        // Activate an application window.
        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("User32.Dll", EntryPoint = "PostMessageA")]
        static extern bool PostMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern int SendMessage(
                int hWnd, // handle to destination window 
                uint Msg, // message 
                int wParam, // first message parameter 
                int lParam // second message parameter 
        ); 

        static void Main(string[] args)
        {
            Timer checkWindows = new System.Timers.Timer(2000);
            checkWindows.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            checkWindows.Enabled = true;
            Console.ReadLine();
            checkWindows.Stop();
        }

        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            var collection = new List<string>();
            EnumDelegate filter = delegate(IntPtr hWnd, int lParam)
            {
                StringBuilder buffer = new StringBuilder(256);
                if (GetWindowText(hWnd, buffer, buffer.Capacity) > 0)
                {
                    string title = buffer.ToString();
                    Console.WriteLine(title);
                    if (title.Contains("says:") || title.Equals("Message from webpage"))
                    {
                        bool ok = SetForegroundWindow(hWnd) && PostMessage(hWnd, 0x100, 0x0D, 1);
                        Console.WriteLine("Close dialog");
                    }
                }

                return true;
            };

            EnumDesktopWindows(IntPtr.Zero, filter, IntPtr.Zero);
        }
    }
}

