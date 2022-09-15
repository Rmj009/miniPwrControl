﻿namespace MiniPwrSupply
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
            this.serialPort1 = new System.IO.Ports.SerialPort(this.components);
            this.grpBox_port_setting = new System.Windows.Forms.GroupBox();
            this.txtbx_baudrate = new System.Windows.Forms.TextBox();
            this.txtbx_com = new System.Windows.Forms.TextBox();
            this.btn_sendcmd = new System.Windows.Forms.Button();
            this.txtbx_addr = new System.Windows.Forms.TextBox();
            this.label_com = new System.Windows.Forms.Label();
            this.btn_open = new System.Windows.Forms.Button();
            this.btn_refresh = new System.Windows.Forms.Button();
            this.label_Addr = new System.Windows.Forms.Label();
            this.label_Baudrate = new System.Windows.Forms.Label();
            this.label_OVP = new System.Windows.Forms.Label();
            this.grpBox_dashboard = new System.Windows.Forms.GroupBox();
            this.txtbx_Iset = new System.Windows.Forms.TextBox();
            this.txtbx_Vset = new System.Windows.Forms.TextBox();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.txtbx_Vin = new System.Windows.Forms.TextBox();
            this.label_Vin = new System.Windows.Forms.Label();
            this.txtbx_ocp = new System.Windows.Forms.TextBox();
            this.label_OCP = new System.Windows.Forms.Label();
            this.txtbx_ovp = new System.Windows.Forms.TextBox();
            this.label_Iset = new System.Windows.Forms.Label();
            this.label_Vset = new System.Windows.Forms.Label();
            this.grpBox_port_setting.SuspendLayout();
            this.grpBox_dashboard.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpBox_port_setting
            // 
            this.grpBox_port_setting.AutoSize = true;
            this.grpBox_port_setting.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.grpBox_port_setting.Controls.Add(this.txtbx_baudrate);
            this.grpBox_port_setting.Controls.Add(this.txtbx_com);
            this.grpBox_port_setting.Controls.Add(this.btn_sendcmd);
            this.grpBox_port_setting.Controls.Add(this.txtbx_addr);
            this.grpBox_port_setting.Controls.Add(this.label_com);
            this.grpBox_port_setting.Controls.Add(this.btn_open);
            this.grpBox_port_setting.Controls.Add(this.btn_refresh);
            this.grpBox_port_setting.Controls.Add(this.label_Addr);
            this.grpBox_port_setting.Controls.Add(this.label_Baudrate);
            this.grpBox_port_setting.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpBox_port_setting.Location = new System.Drawing.Point(0, 0);
            this.grpBox_port_setting.Margin = new System.Windows.Forms.Padding(2);
            this.grpBox_port_setting.Name = "grpBox_port_setting";
            this.grpBox_port_setting.Padding = new System.Windows.Forms.Padding(2);
            this.grpBox_port_setting.Size = new System.Drawing.Size(168, 408);
            this.grpBox_port_setting.TabIndex = 0;
            this.grpBox_port_setting.TabStop = false;
            this.grpBox_port_setting.Text = "PortSetting";
            // 
            // txtbx_baudrate
            // 
            this.txtbx_baudrate.Location = new System.Drawing.Point(97, 88);
            this.txtbx_baudrate.Name = "txtbx_baudrate";
            this.txtbx_baudrate.Size = new System.Drawing.Size(66, 22);
            this.txtbx_baudrate.TabIndex = 5;
            this.txtbx_baudrate.Text = "9600";
            // 
            // txtbx_com
            // 
            this.txtbx_com.Location = new System.Drawing.Point(97, 38);
            this.txtbx_com.Name = "txtbx_com";
            this.txtbx_com.Size = new System.Drawing.Size(66, 22);
            this.txtbx_com.TabIndex = 5;
            this.txtbx_com.Text = "11";
            // 
            // btn_sendcmd
            // 
            this.btn_sendcmd.Location = new System.Drawing.Point(25, 308);
            this.btn_sendcmd.Margin = new System.Windows.Forms.Padding(2);
            this.btn_sendcmd.Name = "btn_sendcmd";
            this.btn_sendcmd.Size = new System.Drawing.Size(106, 83);
            this.btn_sendcmd.TabIndex = 2;
            this.btn_sendcmd.Text = "_sendCmd";
            this.btn_sendcmd.UseVisualStyleBackColor = true;
            this.btn_sendcmd.Click += new System.EventHandler(this.btn_sendcmd_Click);
            // 
            // txtbx_addr
            // 
            this.txtbx_addr.Location = new System.Drawing.Point(90, 244);
            this.txtbx_addr.Margin = new System.Windows.Forms.Padding(2);
            this.txtbx_addr.Name = "txtbx_addr";
            this.txtbx_addr.Size = new System.Drawing.Size(72, 22);
            this.txtbx_addr.TabIndex = 0;
            // 
            // label_com
            // 
            this.label_com.AutoSize = true;
            this.label_com.Location = new System.Drawing.Point(11, 38);
            this.label_com.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_com.Name = "label_com";
            this.label_com.Size = new System.Drawing.Size(31, 12);
            this.label_com.TabIndex = 4;
            this.label_com.Text = "COM";
            // 
            // btn_open
            // 
            this.btn_open.Location = new System.Drawing.Point(97, 150);
            this.btn_open.Margin = new System.Windows.Forms.Padding(2);
            this.btn_open.Name = "btn_open";
            this.btn_open.Size = new System.Drawing.Size(65, 42);
            this.btn_open.TabIndex = 2;
            this.btn_open.Text = "Connect Serialport";
            this.btn_open.UseVisualStyleBackColor = true;
            this.btn_open.Click += new System.EventHandler(this.btn_open_Click);
            // 
            // btn_refresh
            // 
            this.btn_refresh.Location = new System.Drawing.Point(4, 150);
            this.btn_refresh.Margin = new System.Windows.Forms.Padding(2);
            this.btn_refresh.Name = "btn_refresh";
            this.btn_refresh.Size = new System.Drawing.Size(65, 42);
            this.btn_refresh.TabIndex = 2;
            this.btn_refresh.Text = "refresh";
            this.btn_refresh.UseVisualStyleBackColor = true;
            this.btn_refresh.Click += new System.EventHandler(this.btn_refresh_Click);
            // 
            // label_Addr
            // 
            this.label_Addr.AutoSize = true;
            this.label_Addr.Location = new System.Drawing.Point(11, 254);
            this.label_Addr.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_Addr.Name = "label_Addr";
            this.label_Addr.Size = new System.Drawing.Size(29, 12);
            this.label_Addr.TabIndex = 4;
            this.label_Addr.Text = "Addr";
            // 
            // label_Baudrate
            // 
            this.label_Baudrate.AutoSize = true;
            this.label_Baudrate.Location = new System.Drawing.Point(11, 98);
            this.label_Baudrate.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_Baudrate.Name = "label_Baudrate";
            this.label_Baudrate.Size = new System.Drawing.Size(47, 12);
            this.label_Baudrate.TabIndex = 4;
            this.label_Baudrate.Text = "Baudrate";
            // 
            // label_OVP
            // 
            this.label_OVP.AutoSize = true;
            this.label_OVP.Location = new System.Drawing.Point(28, 46);
            this.label_OVP.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_OVP.Name = "label_OVP";
            this.label_OVP.Size = new System.Drawing.Size(27, 12);
            this.label_OVP.TabIndex = 4;
            this.label_OVP.Text = "OVP";
            // 
            // grpBox_dashboard
            // 
            this.grpBox_dashboard.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.grpBox_dashboard.Controls.Add(this.txtbx_Iset);
            this.grpBox_dashboard.Controls.Add(this.txtbx_Vset);
            this.grpBox_dashboard.Controls.Add(this.richTextBox1);
            this.grpBox_dashboard.Controls.Add(this.txtbx_Vin);
            this.grpBox_dashboard.Controls.Add(this.label_Vin);
            this.grpBox_dashboard.Controls.Add(this.txtbx_ocp);
            this.grpBox_dashboard.Controls.Add(this.label_OCP);
            this.grpBox_dashboard.Controls.Add(this.txtbx_ovp);
            this.grpBox_dashboard.Controls.Add(this.label_Iset);
            this.grpBox_dashboard.Controls.Add(this.label_OVP);
            this.grpBox_dashboard.Controls.Add(this.label_Vset);
            this.grpBox_dashboard.Location = new System.Drawing.Point(181, 10);
            this.grpBox_dashboard.Margin = new System.Windows.Forms.Padding(2);
            this.grpBox_dashboard.Name = "grpBox_dashboard";
            this.grpBox_dashboard.Padding = new System.Windows.Forms.Padding(2);
            this.grpBox_dashboard.Size = new System.Drawing.Size(514, 382);
            this.grpBox_dashboard.TabIndex = 1;
            this.grpBox_dashboard.TabStop = false;
            this.grpBox_dashboard.Text = "Dashboard";
            // 
            // txtbx_Iset
            // 
            this.txtbx_Iset.Enabled = false;
            this.txtbx_Iset.Location = new System.Drawing.Point(258, 81);
            this.txtbx_Iset.Margin = new System.Windows.Forms.Padding(2);
            this.txtbx_Iset.Name = "txtbx_Iset";
            this.txtbx_Iset.Size = new System.Drawing.Size(72, 22);
            this.txtbx_Iset.TabIndex = 0;
            // 
            // txtbx_Vset
            // 
            this.txtbx_Vset.Enabled = false;
            this.txtbx_Vset.Location = new System.Drawing.Point(101, 81);
            this.txtbx_Vset.Margin = new System.Windows.Forms.Padding(2);
            this.txtbx_Vset.Name = "txtbx_Vset";
            this.txtbx_Vset.Size = new System.Drawing.Size(72, 22);
            this.txtbx_Vset.TabIndex = 0;
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(31, 140);
            this.richTextBox1.Margin = new System.Windows.Forms.Padding(2);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(459, 218);
            this.richTextBox1.TabIndex = 2;
            this.richTextBox1.Text = "";
            // 
            // txtbx_Vin
            // 
            this.txtbx_Vin.Location = new System.Drawing.Point(431, 44);
            this.txtbx_Vin.Margin = new System.Windows.Forms.Padding(2);
            this.txtbx_Vin.Name = "txtbx_Vin";
            this.txtbx_Vin.Size = new System.Drawing.Size(72, 22);
            this.txtbx_Vin.TabIndex = 0;
            // 
            // label_Vin
            // 
            this.label_Vin.AutoSize = true;
            this.label_Vin.Location = new System.Drawing.Point(358, 46);
            this.label_Vin.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_Vin.Name = "label_Vin";
            this.label_Vin.Size = new System.Drawing.Size(22, 12);
            this.label_Vin.TabIndex = 4;
            this.label_Vin.Text = "Vin";
            // 
            // txtbx_ocp
            // 
            this.txtbx_ocp.Location = new System.Drawing.Point(258, 44);
            this.txtbx_ocp.Margin = new System.Windows.Forms.Padding(2);
            this.txtbx_ocp.Name = "txtbx_ocp";
            this.txtbx_ocp.Size = new System.Drawing.Size(72, 22);
            this.txtbx_ocp.TabIndex = 0;
            // 
            // label_OCP
            // 
            this.label_OCP.AutoSize = true;
            this.label_OCP.Location = new System.Drawing.Point(185, 46);
            this.label_OCP.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_OCP.Name = "label_OCP";
            this.label_OCP.Size = new System.Drawing.Size(27, 12);
            this.label_OCP.TabIndex = 4;
            this.label_OCP.Text = "OCP";
            // 
            // txtbx_ovp
            // 
            this.txtbx_ovp.Location = new System.Drawing.Point(101, 44);
            this.txtbx_ovp.Margin = new System.Windows.Forms.Padding(2);
            this.txtbx_ovp.Name = "txtbx_ovp";
            this.txtbx_ovp.Size = new System.Drawing.Size(72, 22);
            this.txtbx_ovp.TabIndex = 0;
            // 
            // label_Iset
            // 
            this.label_Iset.AutoSize = true;
            this.label_Iset.Location = new System.Drawing.Point(185, 89);
            this.label_Iset.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_Iset.Name = "label_Iset";
            this.label_Iset.Size = new System.Drawing.Size(21, 12);
            this.label_Iset.TabIndex = 4;
            this.label_Iset.Text = "Iset";
            // 
            // label_Vset
            // 
            this.label_Vset.AutoSize = true;
            this.label_Vset.Location = new System.Drawing.Point(28, 89);
            this.label_Vset.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_Vset.Name = "label_Vset";
            this.label_Vset.Size = new System.Drawing.Size(25, 12);
            this.label_Vset.TabIndex = 4;
            this.label_Vset.Text = "Vset";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(740, 408);
            this.Controls.Add(this.grpBox_dashboard);
            this.Controls.Add(this.grpBox_port_setting);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Form1";
            this.Text = "Form1";
            this.grpBox_port_setting.ResumeLayout(false);
            this.grpBox_port_setting.PerformLayout();
            this.grpBox_dashboard.ResumeLayout(false);
            this.grpBox_dashboard.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.IO.Ports.SerialPort serialPort1;
        private System.Windows.Forms.GroupBox grpBox_port_setting;
        private System.Windows.Forms.GroupBox grpBox_dashboard;
        private System.Windows.Forms.Button btn_refresh;
        private System.Windows.Forms.Label label_OVP;
        private System.Windows.Forms.Label label_com;
        private System.Windows.Forms.Button btn_open;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button btn_sendcmd;
        private System.Windows.Forms.TextBox txtbx_Vset;
        private System.Windows.Forms.TextBox txtbx_ovp;
        private System.Windows.Forms.Label label_Vset;
        private System.Windows.Forms.TextBox txtbx_Iset;
        private System.Windows.Forms.TextBox txtbx_Vin;
        private System.Windows.Forms.Label label_Vin;
        private System.Windows.Forms.TextBox txtbx_ocp;
        private System.Windows.Forms.Label label_OCP;
        private System.Windows.Forms.Label label_Iset;
        private System.Windows.Forms.TextBox txtbx_addr;
        private System.Windows.Forms.Label label_Addr;
        private System.Windows.Forms.Label label_Baudrate;
        private System.Windows.Forms.TextBox txtbx_com;
        private System.Windows.Forms.TextBox txtbx_baudrate;
    }
}

