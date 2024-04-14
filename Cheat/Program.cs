using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cheat
{
    static class Program
    {

        static Mutex mutex = new Mutex(true, "Cheat_Program");
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());

                // release mutex after the form is closed.
                mutex.ReleaseMutex();
                mutex.Dispose();

            }

        }
    }
}
