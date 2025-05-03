using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsServer
{
    static class Program
    {
        [DllImport("user32.dll")]
        private static extern Boolean ShowWindow(IntPtr hWnd, Int32 nCmdShow);
        public const int SW_HIDE = 0;
        public const int SW_SHOWNORMAL = 1;
        public const int SW_SHOWMINIMIZED = 2;
        public const int SW_SHOWMAXIMIZED = 3;
        public const int SW_MAXIMIZE = 3;
        public const int SW_SHOWNOACTIVATE = 4;
        public const int SW_SHOW = 5;
        public const int SW_MINIMIZE = 6;
        public const int SW_SHOWMINNOACTIVE = 7;
        public const int SW_SHOWNA = 8;
        public const int SW_RESTORE = 9;
        public const int SW_SHOWDEFAULT = 10;
        public const int SW_FORCEMINIMIZE = 11;
        public const int SW_MAX = 11;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var currentProcess = System.Diagnostics.Process.GetCurrentProcess();

            // search for another process with the same name  
            var anotherProcess = System.Diagnostics.Process.GetProcesses().FirstOrDefault(p => p.ProcessName == currentProcess.ProcessName && p.Id != currentProcess.Id);
            anotherProcess?.WaitForInputIdle();
            if (anotherProcess != null)
            {
                anotherProcess.Kill();
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //FrmServer form = new FrmServer();
            //form.Load += (s, e) => form.Hide();

            // Có thể dùng ApplicationContext để quản lý app
            //Application.Run(new ApplicationContext());
            Application.Run(new FrmServer());
        }
    }
}
