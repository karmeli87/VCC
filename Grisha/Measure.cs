using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace VCC
{
    class Measure
    {
        #region DLL Import
        [DllImport("MUXdll.dll")]
        private static extern int Reset_mux(int port);
        [DllImport("MUXdll.dll")]
        private static extern int Open_port_group_fordll(int port, int group);
        [DllImport("User32.dll")]
        private static extern int SetForegroundWindow(IntPtr point);
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
        [DllImport("user32.dll")]
        static extern bool BringWindowToTop(IntPtr hWnd);
        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 SWP_SHOWWINDOW = 0x0040;
        #endregion

        private volatile bool run=false;
        private Form1 that;
        private Thread t;
        public Measure(Form1 form)
        {
            this.that = form;
        }
        public void stop()
        {
            if (!(t.ThreadState == System.Threading.ThreadState.Stopped)) {
                Logger.add("ACQ", "Abort been requested");
                run = false;
                t.Interrupt();
                t.Join();
            }
        }
        public void start()
        {
            t = new Thread(mes);
            run = true;
            t.Start();
        }
        public bool getStatus() { 
            return run&&!(t.ThreadState == System.Threading.ThreadState.Stopped); 
        }

        private string getProjectName(){
            return new DirectoryInfo(that.txtProjectPath.Text).Name;
        }

        private IntPtr lastFocused;
        private volatile Process process = null;

        private static void mkdir(string path)
        {
            if (!Directory.Exists(path))
            {
                Logger.add("INFO", "creating folder: " + path);
                Directory.CreateDirectory(path);
            }
        }
        private static void createPathDirs(string rootFolder, string burstRelativPath, string contRelativPath)
        {
            mkdir(rootFolder);
            mkdir(rootFolder + "\\" + burstRelativPath);
            mkdir(rootFolder + "\\" + contRelativPath);
        }
        [STAThread]
        private void mes()
        {
            // check for internet connection
            if (!Form1.CheckInternetConnection())
            {
                DialogResult res = MessageBox.Show("No internet connection, continue?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Hand);
                if (res == DialogResult.No)
                {
                    return;
                }
            }

                // sleep configuration
                int baseTime = Convert.ToInt32(Properties.Settings.Default.baseSleepTime);
                double factor = Convert.ToDouble(Properties.Settings.Default.factorSleep);
                int sleepTime = Convert.ToInt32(baseTime * factor);
                        
                        //Logger.add("INFO", "Sleep time: " + sleepTime);

               //MUX params
                string MUXdllPath = Path.GetDirectoryName(that.txtMUXdll.Text);
                Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + ";" + MUXdllPath);
                int COMport = Convert.ToInt32(that.MUXPort.Value);

               //project folders
                string month = DateTime.Today.Month.ToString("d2");
                string rootFolder = that.txtProjectPath.Text + "\\" + DateTime.Today.Year + "\\" + month;
                string burstRelativPath = "Burst";
                string contRelativPath = "100";
                string[] layouts = Directory.GetFiles(that.txtLayoutPath.Text, "*.lay");
                string projectName = getProjectName();

               // shutdown
                bool shutdownActive = false;
                int shutdown = 1024; //in kb
                if (!that.shutdownSwitch.Checked)
                {
                    shutdown = 0;
                }
                //FTP
                int ftpMaxSize = 1024; // in kb
                try
                {
                    ftpMaxSize *= Convert.ToInt32(that.FTPmax.Text);
                }
                catch
                {
                    ftpMaxSize *= 60; //60 MB as default
                }

               //process
                IntPtr progHdl = IntPtr.Zero;
                string procInfo="";          

            // init services
            MailService.config("robot.auto.error.reporter@gmail.com", "snake357");
            MailService.Address = that.txtEmail.Text;
            Logger.init(that.txtProjectPath.Text, that.loggerFileName);
            FTPTransfer.Config(that.ftpUser.Text, that.ftpPassword.Text,
                                that.ftpIP.Text, Convert.ToInt32(that.ftpPort.Value),that.uploadsControl);

            createPathDirs(rootFolder, burstRelativPath, contRelativPath);

            try
            {
               /* 
                System.Timers.Timer aTimer = new System.Timers.Timer(2000);
                aTimer.Elapsed += timeOut;
                aTimer.Enabled = true;
               */
                while (run)
                {
                    resetMux(COMport);
                    if (month != DateTime.Today.Month.ToString("d2"))
                    {
                        month = DateTime.Today.Month.ToString("d2");
                        rootFolder = that.txtProjectPath.Text + "\\" + DateTime.Today.Year + "\\" + month;
                        createPathDirs(rootFolder, burstRelativPath, contRelativPath);
                    }
                    foreach (ChannelCtrl ctrl in that.channelCollection)
                    {
                        // set mode
                        // true -> Cont.
                        // false -> Burst
                        bool mesMode = (ctrl.getMode() == 1);
                        // path to save the measure
                        string workingFolder = rootFolder + "\\" + ((mesMode && !shutdownActive) ? contRelativPath : burstRelativPath);
                        // get the group 
                        int groupNum = ctrl.getGroup();
                        string groupName = that.groupNameCollection[groupNum];
                        string group = (groupName != "") ? groupName : "G" + (groupNum + 1);
                        //measure file
                        string outputFile = DateTime.Now.ToString("yyyy-MM-dd HH-mm-sss-");
                        outputFile += ((group.Length > 2) ? "" : projectName) + group + ((mesMode && shutdownActive) ? "S" : "") + ".DTA";
                        //time to measure
                        double timer = Convert.ToDouble(mesMode && !shutdownActive
                                     ? that.txtCont.Text : that.txtBurst.Text) * 1000; //to ms

                        ctrl.start();
                        process = new Process();
                        Logger.add("INFO", "Opening program");
                        progHdl = getProgramHandle(process, that.txtProgram.Text);
                        if (progHdl == IntPtr.Zero)
                            throw new Exception("Program pointer is null or the program already running!");
                        procInfo = process.ProcessName;
                        Logger.add("INFO", "Process name " + procInfo);
                        Thread.Sleep(10 * sleepTime);

                        progHdl = Process.GetProcessesByName(procInfo)[0].MainWindowHandle;
                        Logger.add("INFO", "Program main handle " + progHdl);

                        //SetWindowPos(progHdl, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
                        SetForegroundWindow(progHdl);
                        Thread.Sleep(2 * sleepTime);

                        // setup the layout
                        Logger.add("INFO", "Loading layout");
                       
                        string layout = getLayoutPath(ctrl, layouts, that.groupNameCollection, shutdownActive);
                        setLayouts(procInfo, layout, sleepTime);
                        
                        // switch group
                        Logger.add("INFO", "Switching..");
                        switchGroup(ctrl.getGroup() + 1, COMport);

                        // setup the saving path
                        setSavingPath(procInfo, workingFolder, sleepTime);

                        // acq.                  
                        acq(procInfo, workingFolder + "\\" + outputFile, sleepTime);

                        // set time to sleep and start acq.
                        Logger.add("INFO", "Sleepy for " + timer);
                        Logger.add("ACQ", "Acquiring data...");

                        // main sleep
                        Thread.Sleep(Convert.ToInt32(timer));
                     
                        // stop , start the transfer and exit
                        Logger.add("ACQ", "Acquiring done");
                        exitProcess(procInfo, sleepTime);

                        long fileSizeKB = new FileInfo(workingFolder + "\\" + outputFile).Length / 1024;
                        if (!shutdownActive && that.shutdownSwitch.Checked && mesMode == true)
                        {
                            if (fileSizeKB < (shutdown * that.shutdownFilesize[groupNum]))
                            {
                                string[] str = outputFile.Split('.');
                                string newOutputFile = str[0] + "S.DTA";
                                File.Move(workingFolder + "\\"+outputFile, workingFolder + "\\"+newOutputFile);
                                outputFile = newOutputFile;
                                shutdownActive = true;
                                MailService.send("Shutdown " + projectName, 
                                    "at file " + workingFolder + "\\" + newOutputFile);
                                Logger.add("INFO", "Shutdown is now active");
                            }
                        }
                        if (fileSizeKB > ftpMaxSize) { 
                            MailService.send("Not uploaded to FTP " + projectName,
                                workingFolder + "\\" + outputFile + "is too big and was not uploaded");
                            Logger.add("WRN", "File exceed max size");
                        }
                        else
                        {
                            sendToFTP(workingFolder + "\\" + outputFile);
                        }

                       // Thread.Sleep(5 * sleepTime);
                        ctrl.stop();
                    }
                    Logger.add("DONE", "Cycle completed");
                }
            }
            catch (ThreadInterruptedException e)
            {
                Logger.add("ABORT", "Closing program " + procInfo);
                exitProcess(procInfo,sleepTime);
            }
            catch (Exception e)
            {
                try 
                {
                    run = false;
                    MailService.send("Error at project " + projectName, e.Message);
                    Logger.add("ERROR", e.Message);
                    msgboxError(e.Message);
                }
                catch (Exception ex)
                {
                    msgboxError("Measure fatal error!! " + ex.Message);                
                }
            }
            run = false;
        }

        /*
        private void setLayouts2(string procInfo, string layout, int sleepTime)
        {
            IntPtr mHwnd = Win32.getMainHandle(procInfo);
            IntPtr sysHwnd = Win32.getChild(mHwnd, 4);
            Win32.ALT(sysHwnd, (IntPtr)'f');
            Win32.ENTER(sysHwnd);
            //Win32
            sendKeys(procInfo, "%n", sleepTime);
            sendKeys(procInfo, layout, sleepTime, true); // set the path
            sendKeys(procInfo, "{ENTER}", sleepTime);
            sendKeys(procInfo, "{RIGHT}{ENTER}", sleepTime, false, false);
            Thread.Sleep(5 * sleepTime);
            //check layout
            StringBuilder sb = new StringBuilder(100);
            Win32.GetWindowText(Process.GetProcessesByName(procInfo)[0].MainWindowHandle, sb, sb.Capacity);
            if (!sb.ToString().Contains(Path.GetFileNameWithoutExtension(layout)))
            {
                Logger.add("LAY", "Failed to load, trying again");
                throw new Exception("layout not loaded");
                // failed to load ,so try to load again
                sendKeys(procInfo, "{ESC}", sleepTime);
                sendKeys(procInfo, "{ESC}", sleepTime);
                sendKeys(procInfo, "{ESC}", sleepTime);
                setLayouts(procInfo, layout, sleepTime);
            }
        }*/

        private void setLayouts(string procInfo, string layout, int sleepTime,int tryNum = 0)
        {
            sendErrMail(tryNum,"Can't load layout");
            IntPtr ptr = Process.GetProcessesByName(procInfo)[0].MainWindowHandle;
            sendKeys(procInfo, "^o", sleepTime);
            sendKeys(procInfo, "%n", sleepTime);
            sendKeys(procInfo, layout, sleepTime, true); // set the path
            sendKeys(procInfo, "{ENTER}", sleepTime);
            sendKeys(procInfo, "{RIGHT}{ENTER}", sleepTime, false, false);
            Thread.Sleep(5 * sleepTime);
            //check layout
            StringBuilder sb = new StringBuilder(100);
            
          /*  int err = Win32.GetWindowText(ptr, sb, sb.Capacity);
            if(err == 0)
                Win32.GetWindowText(Win32.GetWindow(ptr, Win32.GetWindow_Cmd.GW_HWNDNEXT), sb, sb.Capacity);
            Logger.add("LAY", String.Format("Process ({0}) name: {1}",ptr,sb.ToString()));
            if (!sb.ToString().Contains(Path.GetFileNameWithoutExtension(layout)))*/
            if(Win32.FindWindowsWithText(Path.GetFileNameWithoutExtension(layout))==0)
            {
                Logger.add("LAY", "Failed to load, trying again");
                // failed to load ,so try to load again
                sendKeys(procInfo, "{ESC}", sleepTime);
                sendKeys(procInfo, "{ESC}", sleepTime);
                sendKeys(procInfo, "{ESC}", sleepTime);
                setLayouts(procInfo, layout, sleepTime,tryNum+1);
            }              
        }
        private void setSavingPath(string procInfo, string workingFolder, int sleepTime)
        {
            sendKeys(procInfo, "%f{DOWN}{DOWN}{DOWN}{DOWN}{ENTER}", sleepTime);
            sendKeys(procInfo, "{TAB}{ENTER}", sleepTime, false, false);
            sendKeys(procInfo, workingFolder, sleepTime, true);
            sendKeys(procInfo, "{ENTER}", sleepTime, false, false);
            sendKeys(procInfo, "{TAB}{ENTER}", sleepTime, false, false);
        }
        private void acq(string procInfo, string filePath, int sleepTime,int tryNum = 0)
        {
            sendErrMail(tryNum, "Acq failed");
            sendKeys(procInfo, "{F9}", sleepTime);
            sendKeys(procInfo, "%n", sleepTime, false, false);
            sendKeys(procInfo, filePath, sleepTime, true);
            //sendKeys(procInfo, Path.GetFileName(filePath), sleepTime, true);
            sendKeys(procInfo, "{ENTER}", sleepTime, false, false);
            //check for some data as indicate that the acq. really started
            Thread.Sleep(sleepTime);
            if (File.Exists(filePath))
            {
                FileInfo fi = new FileInfo(filePath);
                if (fi.Length < 1)
                    throw new Exception("*** Acq. Failed! ***");
            }
            else
            {
                Logger.add("ACQ", "Failed to load, trying again");
                // failed to load ,so try to load again
                sendKeys(procInfo, "{F10}", sleepTime);
                sendKeys(procInfo, "{ENTER}", sleepTime);
                sendKeys(procInfo, "{ESC}", sleepTime);
                sendKeys(procInfo, "{ESC}", sleepTime);
                sendKeys(procInfo, "{ESC}", sleepTime);
                acq(procInfo, filePath, sleepTime,tryNum + 1);
            }

        }
        
        private void exitProcess(string procInfo,int sleepTime)
        {
            Process[] pro = Process.GetProcessesByName(procInfo);
            foreach (Process proc in pro)
            {
                try
                {
                    sendKeys(procInfo, "{ESC}", sleepTime);
                    sendKeys(procInfo, "{ESC}", sleepTime);
                    sendKeys(procInfo, "{ESC}", sleepTime);
                    sendKeys(proc.ProcessName, "{F10}", sleepTime);
                    sendKeys(proc.ProcessName, "{ENTER}", sleepTime);
                    sendKeys(proc.ProcessName, "%{F4}", sleepTime);
                    proc.WaitForExit(sleepTime * 5);
                }
                catch
                {
                    Logger.add("INFO", "Process was brutally killed!");
                    proc.Kill();
                }
            }
        }
        public void sendToFTP(string filepath)
        {
            try { 
                FTPTransfer.add(filepath);
                string txtfile = createTxtfile(that.txtProjectPath.Text, filepath);
                FTPTransfer.add(txtfile);
            }
            catch(Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        private static string createTxtfile(string projectpath, string dataFile)
        {
            string fname = Path.GetFileNameWithoutExtension(dataFile) + ".txt";
            string txtfile = projectpath + "\\textfiles\\" + fname;
            mkdir(projectpath + "\\textfiles");
            File.AppendAllText(txtfile, "\\" + Functions.getRelativepath(dataFile, 5)+"\\"+Path.GetFileName(dataFile));
            return txtfile;
        }

        private void sendKeys(string procName, string keysStroke, int sleepTime, bool cb = false,bool front = true)
        {
            uint pid;

            IntPtr mHandle = Process.GetProcessesByName(procName)[0].MainWindowHandle;
          //  Logger.add("PTR", "program pointer " + mHandle);
            GetWindowThreadProcessId(GetForegroundWindow(), out pid);
            IntPtr handle = Process.GetProcessById((int)pid).MainWindowHandle;

            int i = 0;
            while (handle != mHandle && i < 10 && front)
            {

                // SetWindowPos(mHandle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
                if (that.InvokeRequired)
                {
                    that.BeginInvoke((Action)(() =>
                    {
                        that.Focus();
                    }));
                }
                else
                {
                    that.Focus();
                }
                BringWindowToTop(mHandle);
                SetForegroundWindow(mHandle);
                Thread.Sleep(sleepTime * (i+1));
                
                mHandle = Process.GetProcessesByName(procName)[0].MainWindowHandle;
                GetWindowThreadProcessId(GetForegroundWindow(), out pid);
                handle = Process.GetProcessById((int)pid).MainWindowHandle;
                i++;
                Logger.add("KEYS", "trying to bring to front, try " + i);
            }
            if (i == 10) throw new Exception("Can't bring the program to front");

            if (cb)
            {   // use clipboard to avoid focus lost during keystroke
               /*
                IAsyncResult  IAres = that.BeginInvoke((Action)(() =>
                {
                    //Clipboard.Clear();
                    Clipboard.SetDataObject(keysStroke,false,5,500);
                    SendKeys.SendWait("^v");
                }));
                IAres.AsyncWaitHandle.WaitOne();
                that.EndInvoke(IAres);
                IAres.AsyncWaitHandle.Close();
                */
                SendKeys.SendWait(keysStroke);
            }
            else
                SendKeys.SendWait(keysStroke);

            Thread.Sleep(sleepTime);
            //lastFocused = GetForegroundWindow();
        }
        private void switchGroup(int group, int port)
        {
            int trys = 6;
            for (int i = 1; i < trys; i++)
            {
                int res = Open_port_group_fordll(port, group);
                if (res != group)
                {
                    Logger.add("FAIL", "Switch group change failure at try =" + i + ",switch did not returned the correct group; result = " + res);
                    Thread.Sleep(1000);
                    resetMux(port);
                    Thread.Sleep(1000);
                    resetMux(port);
                    Logger.add("INFO", "Reset switch two times. try number " + i);
                    continue;
                }
                Logger.add("INFO", "Switched to group number " + res);
                return;
            }
            Logger.add("FAIL", "Switch failed after " + trys);
            throw new Exception("Cannot switch exiting");
        }
        private int resetMux(int port)
        {
            int res = Reset_mux(port);
            if (res != 1) throw new Exception("Switch did not return reset indication; result =" + res);
            Logger.add("INFO", "Mux was resetted with the result = " + res);
            return res;
        }
        internal static string getLayoutPath(ChannelCtrl ctrl, string[] layouts, string[] names, bool shutdown)
        {
            string mode = (ctrl.getMode() == 1) && !shutdown ? "Dur" : "Loc";
            string group = "";
            group = "Group\\s?" + (ctrl.getGroup() + 1);
            string name = names[ctrl.getGroup()];
            if (!name.Equals(""))
                group = "(" + group + "|" + name + ")";

            string pattern = String.Format(".*{0}.*{1}.*\\.lay", group, mode);
            // Console.WriteLine(pattern);
            Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            foreach (string layout in layouts)
            {
                if (rgx.IsMatch(layout))
                    return layout;
            }
            throw new Exception("No layout was found for group" + (ctrl.getGroup()+1));
        }
        public bool IsProcessOpen(string name)
        {
            if (Process.GetProcessesByName(name).Length != 0)
                return true;
            return false;
        }
        private IntPtr getProgramHandle(Process proc, string progPath)
        {
            int trys = 5;
            while (trys > 0)
            {
                if (!IsProcessOpen(Path.GetFileNameWithoutExtension(progPath)))
                {
                    proc.StartInfo.FileName = progPath;
                    proc.Start();

                    // Need to wait to start
                    if (!proc.WaitForInputIdle(30000)) throw new Exception("Timeout event occured: Can't open the program");
                    return proc.MainWindowHandle;
                }
                trys--;
                Thread.Sleep(1000);
            }
            return IntPtr.Zero;
        }
        public void msgboxError(string msg)
        {
            MessageBox.Show(msg, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        private void sendErrMail(int threshold, string msg)
        {
            if (threshold == 10)
            {
                MailService.send("Error on project " + getProjectName(), msg);
            }
        }
    }
}
