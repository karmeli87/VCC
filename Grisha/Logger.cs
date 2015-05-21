using MetroFramework.Controls;
using System;
using System.Collections.Generic;
using System.IO;
//using System.Linq;
using System.Text;
using System.Threading;
//using System.Threading.Tasks;
using System.Windows.Forms;

namespace VCC
{
    class Logger
    {
        private static volatile bool isInit = false;
        private static string logFilePath;
        private static Queue<string> lines= new Queue<string>();
        private static MetroTextBox loggerElement;
        private static volatile bool isWriting = false;

        static readonly object _locker = new object();
        private static Thread t;

        public static void setLogger(MetroTextBox element){
            loggerElement = element;
        }

        public static void init(string path,string file)
        {
            if (Directory.Exists(path)) { 
                logFilePath = path + "\\" + file;
                isInit = true;
            }
        }
        public static void join()
        {
            if(t != null && !(t.ThreadState == ThreadState.Stopped))
                t.Join();
        }
        public static void add(string lable, string msg)
        {
            msg = "<" + lable + "> " + msg;
            string outStr = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff ") + msg + Environment.NewLine;
            lines.Enqueue(outStr);
            lock (_locker) { 
                if (!isWriting)
                {
                    isWriting = true;
                    t = new Thread(write);
                    t.Start();
                }
            }
        }

        private static void write()
        {
            while (lines.Count > 0) { 
                try { 
                    string msg = lines.Dequeue();
                    if(isInit)
                        File.AppendAllText(logFilePath, msg);
                    if (loggerElement.InvokeRequired) { 
                        loggerElement.BeginInvoke((MethodInvoker)delegate
                        {
                            loggerElement.AppendText(msg);
                        });
                    }
                    else
                    {
                        loggerElement.AppendText(msg);
                    }
                }
                catch(Exception e)
                {
                    MessageBox.Show(e.Message,"Logger::write");
                }
            }
            isWriting = false;
        } 
    }
}
