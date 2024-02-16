using System;

namespace BlazorWinFormsApp
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            blazorWebView1 = new PeakSWC.RemoteBlazorWebView.WindowsForms.BlazorWebView();
            SuspendLayout();
            // 
            // blazorWebView1
            // 
            blazorWebView1.Dock = System.Windows.Forms.DockStyle.Fill;
            blazorWebView1.EnableMirrors = true;
            blazorWebView1.Location = new System.Drawing.Point(0, 0);
            blazorWebView1.Margin = new System.Windows.Forms.Padding(0);
            blazorWebView1.Name = "blazorWebView1";
            blazorWebView1.Size = new System.Drawing.Size(1440, 1215);
            blazorWebView1.StartPath = "/";
            blazorWebView1.TabIndex = 20;
            // 
            // Form1
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1440, 1215);
            Controls.Add(blazorWebView1);
            KeyPreview = true;
            Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            Name = "Form1";
            Text = "Blazor in Windows Forms";
            Load += Form1_Load;
            ResumeLayout(false);
        }


        #endregion
        private PeakSWC.RemoteBlazorWebView.WindowsForms.BlazorWebView blazorWebView1;
    }
}
