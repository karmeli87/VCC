using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
//using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Windows.Forms;

namespace VCC
{
    public partial class ChannelCtrl : UserControl
    {
        public ChannelCtrl(int x,int y, int num)
        {
            InitializeComponent();
            if (num == 1)
                this.chDelete.Visible = false;
            this.chProgress.Visible = false;
            this.chNum.Text = "#"+num;
            this.chMode.SelectedIndex = 0;
            this.chGroup.SelectedIndex = 0;
            this.Location = new System.Drawing.Point(x,y);
        }

        public override string ToString()
        {
            string output = "[input" + this.chNum.Text + "]" + Environment.NewLine;
            output += "mode=" + this.chMode.SelectedIndex + Environment.NewLine;
            output += "group=" + this.chGroup.SelectedIndex + Environment.NewLine;

            return output;
        }

        public void set(int mode,int group)
        {
            this.chMode.SelectedIndex = mode;
            this.chGroup.SelectedIndex = group;
        }

        public void start()
        {
            this.chProgress.Visible = true;
        }
        public void stop()
        {
            this.chProgress.Visible = false;
        }
        public string getNum() { 
            return this.chNum.Text; 
        }
        public void setNum(int num)
        {
            this.chNum.Text = "#" + num;
        }
        public int getMode()
        {
            return this.chMode.SelectedIndex;
        }
        public int getGroup()
        {
            return this.chGroup.SelectedIndex;
        }
    }
}
