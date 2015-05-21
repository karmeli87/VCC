namespace VCC
{
    partial class ChannelCtrl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.chGroup = new MetroFramework.Controls.MetroComboBox();
            this.chGroupLable = new MetroFramework.Controls.MetroLabel();
            this.chModeLable = new MetroFramework.Controls.MetroLabel();
            this.chNum = new MetroFramework.Controls.MetroLabel();
            this.chMode = new MetroFramework.Controls.MetroComboBox();
            this.chProgress = new MetroFramework.Controls.MetroProgressSpinner();
            this.chAdd = new MetroFramework.Controls.MetroButton();
            this.chDelete = new MetroFramework.Controls.MetroButton();
            this.SuspendLayout();
            // 
            // chGroup
            // 
            this.chGroup.AllowDrop = true;
            this.chGroup.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chGroup.FontSize = MetroFramework.MetroComboBoxSize.Small;
            this.chGroup.FormattingEnabled = true;
            this.chGroup.IntegralHeight = false;
            this.chGroup.ItemHeight = 21;
            this.chGroup.Items.AddRange(new object[] {
            "      1",
            "      2",
            "      3",
            "      4"});
            this.chGroup.Location = new System.Drawing.Point(130, 9);
            this.chGroup.Name = "chGroup";
            this.chGroup.Size = new System.Drawing.Size(75, 27);
            this.chGroup.TabIndex = 20;
            this.chGroup.UseSelectable = true;
            // 
            // chGroupLable
            // 
            this.chGroupLable.AutoSize = true;
            this.chGroupLable.Location = new System.Drawing.Point(66, 9);
            this.chGroupLable.Name = "chGroupLable";
            this.chGroupLable.Size = new System.Drawing.Size(48, 20);
            this.chGroupLable.TabIndex = 19;
            this.chGroupLable.Text = "Group";
            // 
            // chModeLable
            // 
            this.chModeLable.AutoSize = true;
            this.chModeLable.Location = new System.Drawing.Point(236, 9);
            this.chModeLable.Name = "chModeLable";
            this.chModeLable.Size = new System.Drawing.Size(45, 20);
            this.chModeLable.TabIndex = 1;
            this.chModeLable.Text = "Mode";
            // 
            // chNum
            // 
            this.chNum.Location = new System.Drawing.Point(3, 9);
            this.chNum.Name = "chNum";
            this.chNum.Size = new System.Drawing.Size(40, 22);
            this.chNum.TabIndex = 14;
            this.chNum.Text = "#";
            // 
            // chMode
            // 
            this.chMode.AllowDrop = true;
            this.chMode.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chMode.FontSize = MetroFramework.MetroComboBoxSize.Small;
            this.chMode.FormattingEnabled = true;
            this.chMode.IntegralHeight = false;
            this.chMode.ItemHeight = 21;
            this.chMode.Items.AddRange(new object[] {
            "Burst",
            "Cont."});
            this.chMode.Location = new System.Drawing.Point(299, 9);
            this.chMode.Name = "chMode";
            this.chMode.Size = new System.Drawing.Size(75, 27);
            this.chMode.TabIndex = 15;
            this.chMode.UseSelectable = true;
            // 
            // chProgress
            // 
            this.chProgress.Location = new System.Drawing.Point(384, 9);
            this.chProgress.Maximum = 100;
            this.chProgress.Name = "chProgress";
            this.chProgress.Size = new System.Drawing.Size(29, 31);
            this.chProgress.TabIndex = 22;
            this.chProgress.UseSelectable = true;
            this.chProgress.Value = -1;
            // 
            // chAdd
            // 
            this.chAdd.FontSize = MetroFramework.MetroButtonSize.Medium;
            this.chAdd.Location = new System.Drawing.Point(419, 12);
            this.chAdd.Name = "chAdd";
            this.chAdd.Size = new System.Drawing.Size(30, 23);
            this.chAdd.TabIndex = 23;
            this.chAdd.Text = "+";
            this.chAdd.UseSelectable = true;
            // 
            // chDelete
            // 
            this.chDelete.FontSize = MetroFramework.MetroButtonSize.Medium;
            this.chDelete.Location = new System.Drawing.Point(455, 12);
            this.chDelete.Name = "chDelete";
            this.chDelete.Size = new System.Drawing.Size(30, 23);
            this.chDelete.TabIndex = 24;
            this.chDelete.Text = "-";
            this.chDelete.UseSelectable = true;
            // 
            // ChannelCtrl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.chDelete);
            this.Controls.Add(this.chAdd);
            this.Controls.Add(this.chProgress);
            this.Controls.Add(this.chGroup);
            this.Controls.Add(this.chGroupLable);
            this.Controls.Add(this.chModeLable);
            this.Controls.Add(this.chNum);
            this.Controls.Add(this.chMode);
            this.Name = "ChannelCtrl";
            this.Size = new System.Drawing.Size(496, 45);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MetroFramework.Controls.MetroComboBox chGroup;
        private MetroFramework.Controls.MetroLabel chGroupLable;
        private MetroFramework.Controls.MetroLabel chModeLable;
        private MetroFramework.Controls.MetroLabel chNum;
        private MetroFramework.Controls.MetroComboBox chMode;
        private MetroFramework.Controls.MetroProgressSpinner chProgress;
        public MetroFramework.Controls.MetroButton chAdd;
        public MetroFramework.Controls.MetroButton chDelete;

    }
}
