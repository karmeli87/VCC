using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
using System.Windows.Forms;

namespace VCC
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        //[System.Runtime.InteropServices.DllImport("user32.dll")]
      //  private static extern bool SetProcessDPIAware();

        [STAThread]
        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException +=
              new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Application.ThreadException +=
              new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            Application.EnableVisualStyles();
          //  SetProcessDPIAware();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        static void CurrentDomain_UnhandledException
       (object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Exception ex = (Exception)e.ExceptionObject;

                MessageBox.Show("Whoops! Please contact the developers with "
                   + "the following information:\n\n" + ex.Message + ex.StackTrace,
                   "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            finally
            {
                Application.Exit();
            }
        }

        public static void Application_ThreadException
          (object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            DialogResult result = DialogResult.Abort;
            try
            {
                result = MessageBox.Show("Whoops! Please contact the developers "
                  + "with the following information:\n\n" + e.Exception.Message
                  + e.Exception.StackTrace, "Application Error",
                  MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Stop);
            }
            finally
            {
                if (result == DialogResult.Abort)
                {
                    Application.Exit();
                }
            }
        }
    }
}
