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
            this.blazorWebView1 = new PeakSWC.RemoteBlazorWebView.WindowsForms.BlazorWebView();
            this.SuspendLayout();
            // blazorWebView1
            // 
            this.blazorWebView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.blazorWebView1.Id = Guid.Empty;
            this.blazorWebView1.Location = new System.Drawing.Point(0, 0);
            this.blazorWebView1.Margin = new System.Windows.Forms.Padding(0);
            this.blazorWebView1.Name = "blazorWebView1";
            this.blazorWebView1.Size = new System.Drawing.Size(1872, 1555);
            this.blazorWebView1.TabIndex = 20;
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1872, 1555);
          
            this.Controls.Add(this.blazorWebView1);
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.Name = "Form1";
            this.Text = "Blazor in Windows Forms";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

       
        #endregion
        private PeakSWC.RemoteBlazorWebView.WindowsForms.BlazorWebView blazorWebView1;
    }
}
