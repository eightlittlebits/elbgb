﻿using System;
using System.Windows.Forms;

using static elbgb_ui.NativeMethods;

namespace elbgb_ui
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // set timer resolution to 1ms so the sleep gets the required accurcacy in the wait loop
            WinMM.TimeBeginPeriod(1);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
