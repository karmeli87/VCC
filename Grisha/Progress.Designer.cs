namespace VCC
{
    partial class Progress
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
            this.fName = new MetroFramework.Controls.MetroLabel();
            this.progressSpinner = new MetroFramework.Controls.MetroProgressSpinner();
            this.progressbar = new System.Windows.Forms.ProgressBar();
            this.uploadSpeed = new MetroFramework.Controls.MetroLabel();
            this.percent = new MetroFramework.Controls.MetroLabel();
            this.SuspendLayout();
            // 
            // fName
            // 
            this.fName.AutoSize = true;
            this.fName.Location = new System.Drawing.Point(-3, 4);
            this.fName.Name = "fName";
            this.fName.Size = new System.Drawing.Size(237, 20);
            this.fName.TabIndex = 0;
            this.fName.Text = "2015-04-23 15-49-09-KalmanG1.DAT";
            // 
            // progressSpinner
            // 
            this.progressSpinner.Location = new System.Drawing.Point(549, 4);
            this.progressSpinner.Maximum = 100;
            this.progressSpinner.Name = "progressSpinner";
            this.progressSpinner.Size = new System.Drawing.Size(20, 20);
            this.progressSpinner.TabIndex = 2;
            this.progressSpinner.UseSelectable = true;
            this.progressSpinner.Value = 50;
            this.progressSpinner.Visible = false;
            // 
            // progressbar
            // 
            this.progressbar.Location = new System.Drawing.Point(238, 4);
            this.progressbar.Name = "progressbar";
            this.progressbar.Size = new System.Drawing.Size(187, 20);
            this.progressbar.Step = 1;
            this.progressbar.TabIndex = 4;
            // 
            // uploadSpeed
            // 
            this.uploadSpeed.AutoSize = true;
            this.uploadSpeed.Location = new System.Drawing.Point(476, 4);
            this.uploadSpeed.Name = "uploadSpeed";
            this.uploadSpeed.Size = new System.Drawing.Size(65, 20);
            this.uploadSpeed.TabIndex = 5;
            this.uploadSpeed.Text = "Pending..";
            // 
            // percent
            // 
            this.percent.AutoSize = true;
            this.percent.Location = new System.Drawing.Point(432, 4);
            this.percent.Name = "percent";
            this.percent.Size = new System.Drawing.Size(42, 20);
            this.percent.TabIndex = 6;
            this.percent.Text = "100%";
            // 
            // Progress
            // 
           // this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.percent);
            this.Controls.Add(this.progressbar);
            this.Controls.Add(this.uploadSpeed);
            this.Controls.Add(this.progressSpinner);
            this.Controls.Add(this.fName);
            this.Name = "Progress";
            this.Size = new System.Drawing.Size(572, 31);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MetroFramework.Controls.MetroLabel fName;
        private MetroFramework.Controls.MetroProgressSpinner progressSpinner;
        private System.Windows.Forms.ProgressBar progressbar;
        private MetroFramework.Controls.MetroLabel uploadSpeed;
        private MetroFramework.Controls.MetroLabel percent;
    }
}
