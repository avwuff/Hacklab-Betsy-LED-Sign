﻿namespace Betsy1
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
            this.components = new System.ComponentModel.Container();
            this.button2 = new System.Windows.Forms.Button();
            this.trkIntensity = new System.Windows.Forms.TrackBar();
            this.button4 = new System.Windows.Forms.Button();
            this.bindingSource1 = new System.Windows.Forms.BindingSource(this.components);
            this.videoTrack = new System.Windows.Forms.TrackBar();
            this.chkSplitScan = new System.Windows.Forms.CheckBox();
            this.chkIntelli = new System.Windows.Forms.CheckBox();
            this.trackLines = new System.Windows.Forms.TrackBar();
            this.button5 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.trkIntensity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.videoTrack)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackLines)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(38, 36);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(207, 50);
            this.button2.TabIndex = 2;
            this.button2.Text = "Reset Sign";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // trkIntensity
            // 
            this.trkIntensity.LargeChange = 10;
            this.trkIntensity.Location = new System.Drawing.Point(275, 62);
            this.trkIntensity.Maximum = 100;
            this.trkIntensity.Name = "trkIntensity";
            this.trkIntensity.Size = new System.Drawing.Size(299, 69);
            this.trkIntensity.TabIndex = 4;
            this.trkIntensity.TickFrequency = 5;
            this.trkIntensity.Value = 20;
            this.trkIntensity.Scroll += new System.EventHandler(this.trkIntensity_Scroll);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(38, 108);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(207, 46);
            this.button4.TabIndex = 6;
            this.button4.Text = "Video Test";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // videoTrack
            // 
            this.videoTrack.LargeChange = 500;
            this.videoTrack.Location = new System.Drawing.Point(49, 406);
            this.videoTrack.Maximum = 10000;
            this.videoTrack.Name = "videoTrack";
            this.videoTrack.Size = new System.Drawing.Size(707, 69);
            this.videoTrack.SmallChange = 100;
            this.videoTrack.TabIndex = 7;
            this.videoTrack.TickFrequency = 100;
            this.videoTrack.Visible = false;
            this.videoTrack.MouseUp += new System.Windows.Forms.MouseEventHandler(this.videoTrack_MouseUp);
            // 
            // chkSplitScan
            // 
            this.chkSplitScan.AutoSize = true;
            this.chkSplitScan.Location = new System.Drawing.Point(40, 288);
            this.chkSplitScan.Name = "chkSplitScan";
            this.chkSplitScan.Size = new System.Drawing.Size(107, 24);
            this.chkSplitScan.TabIndex = 8;
            this.chkSplitScan.Text = "Split Scan";
            this.chkSplitScan.UseVisualStyleBackColor = true;
            // 
            // chkIntelli
            // 
            this.chkIntelli.AutoSize = true;
            this.chkIntelli.Location = new System.Drawing.Point(40, 318);
            this.chkIntelli.Name = "chkIntelli";
            this.chkIntelli.Size = new System.Drawing.Size(120, 24);
            this.chkIntelli.TabIndex = 9;
            this.chkIntelli.Text = "IntelliDetect";
            this.chkIntelli.UseVisualStyleBackColor = true;
            // 
            // trackLines
            // 
            this.trackLines.Location = new System.Drawing.Point(204, 288);
            this.trackLines.Maximum = 5;
            this.trackLines.Minimum = 1;
            this.trackLines.Name = "trackLines";
            this.trackLines.Size = new System.Drawing.Size(139, 69);
            this.trackLines.TabIndex = 10;
            this.trackLines.Value = 1;
            this.trackLines.Scroll += new System.EventHandler(this.trackLines_Scroll);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(38, 168);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(207, 40);
            this.button5.TabIndex = 11;
            this.button5.Text = "Camera";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(36, 250);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(173, 20);
            this.label1.TabIndex = 12;
            this.label1.Text = "Split Scan Crazy Mode:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(294, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(156, 20);
            this.label2.TabIndex = 13;
            this.label2.Text = "Maximum Brightness";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(275, 93);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(49, 38);
            this.pictureBox1.TabIndex = 14;
            this.pictureBox1.TabStop = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(683, 19);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(51, 20);
            this.label3.TabIndex = 15;
            this.label3.Text = "label3";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(862, 15);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(207, 47);
            this.button1.TabIndex = 16;
            this.button1.Text = "check online";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(1145, 19);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(232, 43);
            this.button3.TabIndex = 17;
            this.button3.Text = "gain 100";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click_1);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(38, 215);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(51, 20);
            this.label4.TabIndex = 18;
            this.label4.Text = "label4";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(2230, 371);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.trackLines);
            this.Controls.Add(this.chkIntelli);
            this.Controls.Add(this.chkSplitScan);
            this.Controls.Add(this.videoTrack);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.trkIntensity);
            this.Controls.Add(this.button2);
            this.Name = "Form1";
            this.Text = "Av\'s LED Sign Controller";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.trkIntensity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.videoTrack)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackLines)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TrackBar trkIntensity;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.BindingSource bindingSource1;
        private System.Windows.Forms.TrackBar videoTrack;
        private System.Windows.Forms.CheckBox chkSplitScan;
        private System.Windows.Forms.CheckBox chkIntelli;
        private System.Windows.Forms.TrackBar trackLines;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Label label4;
    }
}

