using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MetroFramework;
using System.Collections;
using System.Runtime.InteropServices;

namespace VCC
{
    public partial class Progress : UserControl
    {
        private string name = "";
        private int progress = 0;
        private bool running = false;

        public Progress(string name)
        {
            InitializeComponent();
            this.fName.Text = name;
            this.name = name;
            this.progressbar.Value = 0;
            this.percent.Text = "0%";
        }

        public int getProgress() {
            return progress;
        }
        public string getName()
        {
            return name;
        }
        public void setProgress(int num)
        {
            progress = num;
            running = (num < 100);
            this.progressSpinner.Visible = running;
            this.progressbar.Value = progress;
            this.percent.Text = num + "%";
        }

        public void setSpeed(string speed)
        {
            this.uploadSpeed.Text = speed;
        }
        public void cancel()
        {
            this.uploadSpeed.Text = "Cancelled";
            this.running = false;
        }
        public bool getStatus()
        {
            return running;
        }
    }
}
