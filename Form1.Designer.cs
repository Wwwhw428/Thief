using System.Drawing;
using System.Windows.Forms;
using System;

namespace Thief
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(340, 315);
            this.Name = "Form1";
            this.ResumeLayout(false);

            this.button1 = new Button();

            this.button1.Text = "Toggle";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new EventHandler(this.button1_Click);
            this.button1.KeyUp += new KeyEventHandler(this.button1_KeyUp);

            this.Controls.Add(this.button1);

            this.trayMenu = new ContextMenuStrip();
            this.trayMenu.Items.Add("Exit", null, Exit_Click);

            //this.trayIcon = new NotifyIcon();
            //this.trayIcon.Icon = new Icon("AppIcon.ico"); // 使用适当的图标文件
            //this.trayIcon.ContextMenuStrip = this.trayMenu;
            //this.trayIcon.Visible = true;
        }

        #endregion

        private System.Windows.Forms.Button btnClickThis;
        private System.Windows.Forms.Label lbTest;
        private Button button1;
        private ContextMenuStrip trayMenu;
        private NotifyIcon trayIcon;
    }
}

