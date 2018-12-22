namespace KMLConverter
{
    partial class Form1
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.btnopenfile = new System.Windows.Forms.Button();
            this.textFolderName = new System.Windows.Forms.TextBox();
            this.grpBoxOutputName = new System.Windows.Forms.GroupBox();
            this.grpBoxActivityLog = new System.Windows.Forms.GroupBox();
            this.txtActivityLog = new System.Windows.Forms.TextBox();
            this.chkWriteKmlCaption = new System.Windows.Forms.CheckBox();
            this.lblFileDest = new System.Windows.Forms.Label();
            this.btnSetSavLoc = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.button2 = new System.Windows.Forms.Button();
            this.grpBoxOutputName.SuspendLayout();
            this.grpBoxActivityLog.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(209, 14);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(157, 28);
            this.button1.TabIndex = 0;
            this.button1.Text = "Create KML";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // btnopenfile
            // 
            this.btnopenfile.Location = new System.Drawing.Point(12, 12);
            this.btnopenfile.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnopenfile.Name = "btnopenfile";
            this.btnopenfile.Size = new System.Drawing.Size(157, 28);
            this.btnopenfile.TabIndex = 1;
            this.btnopenfile.Text = "&Open File";
            this.btnopenfile.UseVisualStyleBackColor = true;
            this.btnopenfile.Click += new System.EventHandler(this.btnopenfile_Click);
            // 
            // textFolderName
            // 
            this.textFolderName.Location = new System.Drawing.Point(7, 27);
            this.textFolderName.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textFolderName.Name = "textFolderName";
            this.textFolderName.Size = new System.Drawing.Size(335, 22);
            this.textFolderName.TabIndex = 2;
            this.textFolderName.TextChanged += new System.EventHandler(this.textFolderName_TextChanged);
            // 
            // grpBoxOutputName
            // 
            this.grpBoxOutputName.Controls.Add(this.textFolderName);
            this.grpBoxOutputName.Location = new System.Drawing.Point(12, 87);
            this.grpBoxOutputName.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.grpBoxOutputName.Name = "grpBoxOutputName";
            this.grpBoxOutputName.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.grpBoxOutputName.Size = new System.Drawing.Size(355, 65);
            this.grpBoxOutputName.TabIndex = 3;
            this.grpBoxOutputName.TabStop = false;
            this.grpBoxOutputName.Text = "groupBox1";
            // 
            // grpBoxActivityLog
            // 
            this.grpBoxActivityLog.Controls.Add(this.txtActivityLog);
            this.grpBoxActivityLog.Location = new System.Drawing.Point(12, 158);
            this.grpBoxActivityLog.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.grpBoxActivityLog.Name = "grpBoxActivityLog";
            this.grpBoxActivityLog.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.grpBoxActivityLog.Size = new System.Drawing.Size(347, 329);
            this.grpBoxActivityLog.TabIndex = 4;
            this.grpBoxActivityLog.TabStop = false;
            this.grpBoxActivityLog.Text = "groupBox1";
            // 
            // txtActivityLog
            // 
            this.txtActivityLog.Location = new System.Drawing.Point(5, 25);
            this.txtActivityLog.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtActivityLog.Multiline = true;
            this.txtActivityLog.Name = "txtActivityLog";
            this.txtActivityLog.Size = new System.Drawing.Size(335, 290);
            this.txtActivityLog.TabIndex = 0;
            // 
            // chkWriteKmlCaption
            // 
            this.chkWriteKmlCaption.AutoSize = true;
            this.chkWriteKmlCaption.Location = new System.Drawing.Point(209, 60);
            this.chkWriteKmlCaption.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.chkWriteKmlCaption.Name = "chkWriteKmlCaption";
            this.chkWriteKmlCaption.Size = new System.Drawing.Size(147, 21);
            this.chkWriteKmlCaption.TabIndex = 5;
            this.chkWriteKmlCaption.Text = "Write KML Caption";
            this.chkWriteKmlCaption.UseVisualStyleBackColor = true;
            this.chkWriteKmlCaption.CheckedChanged += new System.EventHandler(this.chkWriteKmlCaption_CheckedChanged);
            // 
            // lblFileDest
            // 
            this.lblFileDest.AutoSize = true;
            this.lblFileDest.Location = new System.Drawing.Point(19, 494);
            this.lblFileDest.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFileDest.Name = "lblFileDest";
            this.lblFileDest.Size = new System.Drawing.Size(119, 17);
            this.lblFileDest.TabIndex = 7;
            this.lblFileDest.Text = "KML Destination :";
            this.lblFileDest.MouseHover += new System.EventHandler(this.lblFileDest_MouseHover);
            // 
            // btnSetSavLoc
            // 
            this.btnSetSavLoc.Location = new System.Drawing.Point(12, 53);
            this.btnSetSavLoc.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnSetSavLoc.Name = "btnSetSavLoc";
            this.btnSetSavLoc.Size = new System.Drawing.Size(157, 28);
            this.btnSetSavLoc.TabIndex = 8;
            this.btnSetSavLoc.Text = "&Set Save Location";
            this.btnSetSavLoc.UseVisualStyleBackColor = true;
            this.btnSetSavLoc.Click += new System.EventHandler(this.btnSetSavLoc_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(417, 12);
            this.button2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(100, 28);
            this.button2.TabIndex = 9;
            this.button2.Text = "button2";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Visible = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(559, 517);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.btnSetSavLoc);
            this.Controls.Add(this.lblFileDest);
            this.Controls.Add(this.chkWriteKmlCaption);
            this.Controls.Add(this.grpBoxActivityLog);
            this.Controls.Add(this.grpBoxOutputName);
            this.Controls.Add(this.btnopenfile);
            this.Controls.Add(this.button1);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.grpBoxOutputName.ResumeLayout(false);
            this.grpBoxOutputName.PerformLayout();
            this.grpBoxActivityLog.ResumeLayout(false);
            this.grpBoxActivityLog.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button btnopenfile;
        private System.Windows.Forms.TextBox textFolderName;
        private System.Windows.Forms.GroupBox grpBoxOutputName;
        private System.Windows.Forms.GroupBox grpBoxActivityLog;
        private System.Windows.Forms.TextBox txtActivityLog;
        private System.Windows.Forms.CheckBox chkWriteKmlCaption;
        private System.Windows.Forms.Label lblFileDest;
        private System.Windows.Forms.Button btnSetSavLoc;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Button button2;
    }
}

