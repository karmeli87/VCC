using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
//using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
//using System.Threading.Tasks;
using System.Windows.Forms;
using MetroFramework.Forms;
using System.IO.Ports;
using System.Management;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Net;
using System.Collections;

namespace VCC
{
    public partial class Form1 : MetroFramework.Forms.MetroForm
    {
        private int xAnchor = 35;
        internal string[] groupNameCollection = new string[4];
        internal int[] shutdownFilesize = new int[4];

        internal List<ChannelCtrl> channelCollection = new List<ChannelCtrl>();
        private List<Control> setupCollection = new List<Control>();
        private List<Control> transferCollection = new List<Control>();
        private List<Control> globalParamsCollection = new List<Control>();
        internal string loggerFileName = "log.txt";
        public Form1()
        {
            InitializeComponent();
            PopulateForm();
            ConfigForm();
            this.mainTabs.SelectedIndex = 0;
            this.FocusMe();
        }

        public static Progress createProgress(string fName, int count)
        {
            Progress p = new Progress(fName);
            p.Location = new System.Drawing.Point(0, count * 31);
            return p;
         
        }
        private void addChannel_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            ChannelCtrl origin = (ChannelCtrl)btn.Parent;
            int index = Convert.ToInt32(origin.getNum().Substring(1));
            ChannelCtrl ctrl = new ChannelCtrl(0,0,index+1);
            addCtrToPanel(ctrl);
            channelCollection.Insert(index, ctrl);
            redraw(index);
        }
        private void deleteChannel_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            ChannelCtrl origin = (ChannelCtrl)btn.Parent;
            Console.WriteLine(origin.getNum());
            origin.Dispose();
            channelCollection.Remove(origin);
            Console.WriteLine(channelCollection.Count);
            redraw();
        }
        private void redraw(int start=0)
        {
            int i=1;
            foreach (ChannelCtrl ctrl in channelCollection)
            {
                if (start < i) {
                    ctrl.Location = new Point(xAnchor, 41 * (i - 1));
                    ctrl.setNum(i);
                }
                i++;
            }
        }
        private void PopulateForm()
        {
            #region Setup Collection
            setupCollection.Add(this.txtLayoutPath);
            setupCollection.Add(this.txtMUXdll);
            setupCollection.Add(this.txtProgram);
            setupCollection.Add(this.txtProjectPath);
            setupCollection.Add(this.groupName1);
            setupCollection.Add(this.groupName2);
            setupCollection.Add(this.groupName3);
            setupCollection.Add(this.groupName4);
            setupCollection.Add(this.txtShutdown1);
            setupCollection.Add(this.txtShutdown2);
            setupCollection.Add(this.txtShutdown3);
            setupCollection.Add(this.txtShutdown4);
            #endregion

            #region Global params Collection
            globalParamsCollection.Add(this.txtBurst);
            globalParamsCollection.Add(this.txtCont);
            #endregion
            
            // Load last known config file
            if (Properties.Settings.Default.confFile != "" && File.Exists(Properties.Settings.Default.confFile))
                    LoadFile(Properties.Settings.Default.confFile);
            else { 
                #region Create default inputs
                for (int i = 0; i < 8; i++)
                {
                    ChannelCtrl ctrl = new ChannelCtrl(0, 0, i + 1);
                    channelCollection.Add(ctrl);
                    addCtrToPanel(ctrl);
                }
                #endregion
                redraw();
            }  
        }
        private void ConfigForm()
        {
            Logger.setLogger(this.logger);
            Logger.init(this.txtProjectPath.Text, loggerFileName);

            configFTP();
            #region show pending files
            int j = 0;
            foreach (string file in Functions.destringifyQueue(Properties.Settings.Default.FTPQueue))
            {
                Progress p = new Progress(Path.GetFileName(file));
                p.Location = new Point(0, 31 * j);
                this.uploadsControl.Controls.Add(p);
                j++;
            }
            #endregion

            findCom();
        }
        private void configFTP(bool offline = false)
        {
                FTPTransfer.Config(Properties.Settings.Default.ftpUser,
            Properties.Settings.Default.ftpPassword, Properties.Settings.Default.ftpIP,
            Properties.Settings.Default.ftpPort, this.uploadsControl,offline);
        }
        private void findCom()
        {
            if (this.findMUXport.Checked)
            {
                // Get a list of serial port names. 
                ManagementObjectCollection collection;
                using (var searcher = new ManagementObjectSearcher
                    (@"Select * From Win32_PnPEntity WHERE Caption like '%USB Serial Port%'"))
                    collection = searcher.Get();

                Match match;
                foreach (var device in collection)
                {
                    string output = (string)device.GetPropertyValue("Name");
                    match = Regex.Match(output, @".*\(COM(\d+)\).*", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        // Finally, we get the port value.
                        string key = match.Groups[1].Value;
                        Logger.add("COM", "Port was found at COM" + key);
                        this.MUXPort.Value = Convert.ToInt32(key);
                        return;
                    }
                }
                MetroFramework.MetroMessageBox.Show(this,"Could not found COM port", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private void evtFolderBrowse(object sender, EventArgs e)
        {
            if (sender.Equals(btnProjectPath))
            {
                folderBrowserDialog1.SelectedPath = getInitPath(this.txtProjectPath.Text);
            }
            if (sender.Equals(btnLayoutPath))
            {
                folderBrowserDialog1.SelectedPath = getInitPath(this.txtLayoutPath.Text);
            }

            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
               String selected = folderBrowserDialog1.SelectedPath;

                
               if (sender.Equals(btnProjectPath))
                   txtProjectPath.Text = selected;

               else if (sender.Equals(btnLayoutPath))
                   txtLayoutPath.Text = selected;

            }                
        }
        private void evtFileBrowse(object sender, EventArgs e)
        {
            if (sender.Equals(btnMUXsettings))
            {
                openFileDialog1.InitialDirectory = getInitPath(this.txtMUXdll.Text);
                openFileDialog1.Filter = "dll files (*.dll)|*.dll|All files (*.*)|*.*";
                openFileDialog1.FilterIndex = 1;
                openFileDialog1.Multiselect = false;
            }
            if (sender.Equals(btnProgram))
            {
                
                openFileDialog1.InitialDirectory = getInitPath(this.txtProgram.Text);
                openFileDialog1.Filter = "exe files (*.exe)|*.exe|All files (*.*)|*.*";
                openFileDialog1.FilterIndex = 1;
                openFileDialog1.Multiselect = false;
            }
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                String selected = openFileDialog1.FileName;

                if (sender.Equals(btnMUXsettings))
                    txtMUXdll.Text = selected;

                else if (sender.Equals(btnProgram))
                    txtProgram.Text = selected;
            }
        }

        private string getInitPath(string path)
        {
            string str = "";
            try{
            if(Directory.Exists(Path.GetDirectoryName(path)))
                str = Path.GetDirectoryName(path);
            }
            catch{
                str = "";
            }
            return str;
        }
        private void save_Click(object sender, EventArgs e)
        {
            Console.WriteLine(sender);
            // TODO: valitade inputs
     
            //  SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "ini files (*.ini)|*.ini|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.FileName = "";
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.InitialDirectory = this.txtProjectPath.Text;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                    // Code to write the stream goes here.
                     
                    using (StreamWriter sw = new StreamWriter(saveFileDialog1.FileName)) {
                        foreach (ChannelCtrl ctr in channelCollection)
                            sw.WriteLine(ctr.ToString());

                        string setupStr = "[SetupVars]" + Environment.NewLine;
                        foreach (Control ctr in setupCollection)
                            setupStr += ctr.Name + "=" + ctr.Text + Environment.NewLine;
                        setupStr += "shutdownSwitch=" + this.shutdownSwitch.Checked;
                        sw.WriteLine(setupStr);

                        string globalParamStr = "[GlobalParams]" + Environment.NewLine;
                        foreach (Control ctr in globalParamsCollection)
                            globalParamStr += ctr.Name + "=" + ctr.Text + Environment.NewLine;
                        sw.WriteLine(globalParamStr);
                        
                    }

                    //Properties.Settings.Default.confFile = this.txtProjectPath.Text +"\\"+ Path.GetFileName(saveFileDialog1.FileName);
                    Properties.Settings.Default.confFile = saveFileDialog1.FileName;    
                    Properties.Settings.Default.Save();
            }

        }
        private void open_Click(object sender, EventArgs e)
        { 
            openFileDialog1.Filter = "ini files (*.ini)|*.ini|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.Multiselect = true;
            openFileDialog1.InitialDirectory = this.txtProjectPath.Text;
            openFileDialog1.FileName = "";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(openFileDialog1.FileName)) { 
                    LoadFile(openFileDialog1.FileName);
                    Properties.Settings.Default.confFile = openFileDialog1.FileName;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private void addCtrToPanel(ChannelCtrl ctrl)
        {
            ctrl.chAdd.Click += new System.EventHandler(this.addChannel_Click);
            ctrl.chDelete.Click += new System.EventHandler(this.deleteChannel_Click);
            this.Inputs.Controls.Add(ctrl);
        }

        private void LoadFile(string path)
        {
            IniFile file = new IniFile(path);
            foreach (ChannelCtrl ctrl in channelCollection)
                ctrl.Dispose();
            channelCollection.Clear();

            int i = 1;
            string section = "input#" + i;
            while(file.sectionExist(section))
            {
                    int group = file.GetInteger("input#" + i, "group");
                    int mode = file.GetInteger("input#" + i, "mode");
                    ChannelCtrl ctr = new ChannelCtrl(0, 0, i);
                    ctr.set(mode, group);
                    channelCollection.Add(ctr);
                    addCtrToPanel(ctr);
                    i++;
                    section = "input#" + i;
            }
            redraw();

            foreach (Control ctr in setupCollection)
                this.Controls.Find(ctr.Name, true)[0].Text = file.GetValue("SetupVars", ctr.Name);
            foreach (Control ctr in globalParamsCollection)
                this.Controls.Find(ctr.Name, true)[0].Text = file.GetValue("GlobalParams", ctr.Name);

            this.shutdownSwitch.Checked = file.GetBoolean("SetupVars", "shutdownSwitch");

        }
        private void exit_Clickmenu(object sender, EventArgs e)
        {
            this.Close();
        }
        private void exit_Click(object sender, CancelEventArgs e)
        {
            if (workingMes != null)
            {
                if(workingMes.getStatus())
                {
                    DialogResult res = MetroFramework.MetroMessageBox.Show(this,"The cycle is still running." + Environment.NewLine
                        + "Closing this progam will break the cycle and close the measurmant program" + Environment.NewLine
                        + "Are you sure?","Exit",MessageBoxButtons.YesNo,MessageBoxIcon.Question);
                   if (res != DialogResult.Yes)
                   {
                       e.Cancel = true;
                   }
                }
            }
            if(!e.Cancel)
                prepareClose();
        }

        private void prepareClose()
        {
            //TODO: check if the ftp still transferring...
            Logger.add("INFO", "Prepare for closing...");
            if (workingMes != null) {
                Logger.add("INFO", "closing measure..");
                workingMes.stop();
            }
            Logger.add("INFO", "closing FTP..");
            FTPTransfer.close();
            Logger.add("INFO", "closing Logger..");
            Logger.join();
        }

        

        private void evtFTP(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();
            configFTP();
        }
        private void transfer(object sender, EventArgs evt)
        {
            evtFTP(sender, evt);
        }
        private void clearLog_Click(object sender, EventArgs e)
        {
            this.logger.Text = "";
        }

        private void btnLoadlog_Click(object sender, EventArgs e)
        {
            // load log file into the logger tab
            if (File.Exists(this.txtProjectPath.Text + "\\" + loggerFileName))
                this.logger.Text = File.ReadAllText(this.txtProjectPath.Text + "\\" + loggerFileName);
        }

        private Measure workingMes = null;


        private void run(object sender, EventArgs evt)
        {
            if (this.Run.Enabled == false)
            {
                Logger.add("INFO", "tried to run when still running..");
                return;
            } 
            groupNameCollection[0] = this.groupName1.Text;
            groupNameCollection[1] = this.groupName2.Text;
            groupNameCollection[2] = this.groupName3.Text;
            groupNameCollection[3] = this.groupName4.Text;
            try { 
                shutdownFilesize[0] = Convert.ToInt32(this.txtShutdown1.Text);
                shutdownFilesize[1] = Convert.ToInt32(this.txtShutdown2.Text);
                shutdownFilesize[2] = Convert.ToInt32(this.txtShutdown3.Text);
                shutdownFilesize[3] = Convert.ToInt32(this.txtShutdown4.Text);
            }
            catch
            {
                msgboxError("Invalid shutdown number!");
                return;
            }

            #region Paths validation

            if (!Directory.Exists(this.txtProjectPath.Text))
            {
                msgboxError("Fatal error: Please select a project path");
                return;
            }

            if (!Directory.Exists(this.txtLayoutPath.Text))
            {
                msgboxError("Fatal error: Please select a layout path");
                return;
            }

            if (!File.Exists(this.txtProgram.Text) || Path.GetExtension(this.txtProgram.Text) != ".exe")
            {
                msgboxError("Fatal error: Please select a program exe file");
                return;
            }

            if (!File.Exists(this.txtMUXdll.Text) || Path.GetExtension(this.txtMUXdll.Text) != ".dll")
            {
                msgboxError("Fatal error: MUXdll.dll not found!");
                return;
            }

            #endregion
            #region Form integrity validation

            if (this.txtBurst.Text == "")
            {
                msgboxError("Enter burst value");
                return;
            }
            if (this.txtCont.Text == "")
            {
                msgboxError("Enter cont. value");
                return;
            }
            #endregion
            #region check if nothing is missing
            //check if all layouts are exist
            string[] layouts = Directory.GetFiles(this.txtLayoutPath.Text, "*.lay");
            foreach (ChannelCtrl ctr in this.channelCollection)
            {
                try
                {
                    Measure.getLayoutPath(ctr, layouts, this.groupNameCollection, false);
                }
                catch (Exception e)
                {
                    msgboxError(e.Message);
                    return;
                }
            }

            #endregion

            this.Inputs.Enabled = false;
           // this.clearFTPQueue.Enabled = false;
            this.Run.Enabled = false;

            if (workingMes == null)
            {
                workingMes = new Measure(this);
                workingMes.start();
            }
            else if (!workingMes.getStatus()) 
                workingMes.start();
        }
        private void btnStop_Click(object sender, EventArgs e)
        {
            if (workingMes != null) { 
                workingMes.stop();
                workingMes = null;
                foreach (ChannelCtrl ctr in channelCollection)
                {
                    ctr.stop();
                }
            }
            foreach (ChannelCtrl ctr in channelCollection)
            {
                ctr.stop();
            }
            this.clearFTPQueue.Enabled = true;
            this.Inputs.Enabled = true;
            this.Run.Enabled = true;
        }

        private void ftpCheck_Click(object sender, EventArgs e)
        {            
            new Thread(() => FTPTransfer.check(this.ftpUser.Text, this.ftpPassword.Text,
                                this.ftpIP.Text, Convert.ToInt32(this.ftpPort.Value),this.ftpCheckSpinner)).Start(); 
        }
        public static bool CheckInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead("http://www.google.com"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private void clearFTPQueue_Click(object sender, EventArgs e)
        {
            FTPTransfer.clearQueue();
            this.uploadsControl.Controls.Clear();

        }

        private void addFTPQueue_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = getInitPath(this.txtMUXdll.Text);
            openFileDialog1.Multiselect = true;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                String[] selected = openFileDialog1.FileNames;
                foreach (string fn in selected)
                {
                    FTPTransfer.add(fn);
                }
            }
        }

        private void msgboxError(string message)
        {
            MetroFramework.MetroMessageBox.Show(this, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void txtProjectPath_TextChanged(object sender, EventArgs e)
        {
            Logger.init(this.txtProjectPath.Text, loggerFileName);
        }
    }
}
