namespace MiniPwrSupply
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
            this.grpBox_Configuration = new System.Windows.Forms.GroupBox();
            this.btn_connection = new System.Windows.Forms.Button();
            this.label_DataReceived = new System.Windows.Forms.Label();
            this.label_ByteReceived = new System.Windows.Forms.Label();
            this.cmbx_baudrate = new System.Windows.Forms.ComboBox();
            this.txtbx_addr = new System.Windows.Forms.TextBox();
            this.label_Addr = new System.Windows.Forms.Label();
            this.btn_refresh = new System.Windows.Forms.Button();
            this.label_Baudrate = new System.Windows.Forms.Label();
            this.btn_Power = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btn_sendcmd = new System.Windows.Forms.Button();
            this.cmbx_com = new System.Windows.Forms.ComboBox();
            this.label_com = new System.Windows.Forms.Label();
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
            this.label_WuzhiCmd = new System.Windows.Forms.Label();
            this.txtbx_WuzhiCmd = new System.Windows.Forms.TextBox();
            this.radiobtn_hexadded = new System.Windows.Forms.RadioButton();
            this.tabCtrl_Dashboard = new System.Windows.Forms.TabControl();
            this.tabCtrl1 = new System.Windows.Forms.TabPage();
            this.tabCtrl2 = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btn_TryCmd = new System.Windows.Forms.Button();
            this.txtbx_TryCmd = new System.Windows.Forms.TextBox();
            this.label_serial1DataReceived = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btn_hexaddition = new System.Windows.Forms.Button();
            this.grpBox_Configuration.SuspendLayout();
            this.grpBox_dashboard.SuspendLayout();
            this.tabCtrl_Dashboard.SuspendLayout();
            this.tabCtrl1.SuspendLayout();
            this.tabCtrl2.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpBox_Configuration
            // 
            this.grpBox_Configuration.AutoSize = true;
            this.grpBox_Configuration.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.grpBox_Configuration.Controls.Add(this.btn_connection);
            this.grpBox_Configuration.Controls.Add(this.label_DataReceived);
            this.grpBox_Configuration.Controls.Add(this.label_ByteReceived);
            this.grpBox_Configuration.Controls.Add(this.cmbx_baudrate);
            this.grpBox_Configuration.Controls.Add(this.txtbx_addr);
            this.grpBox_Configuration.Controls.Add(this.label_Addr);
            this.grpBox_Configuration.Controls.Add(this.btn_refresh);
            this.grpBox_Configuration.Controls.Add(this.label_Baudrate);
            this.grpBox_Configuration.Controls.Add(this.btn_Power);
            this.grpBox_Configuration.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpBox_Configuration.Location = new System.Drawing.Point(3, 3);
            this.grpBox_Configuration.Margin = new System.Windows.Forms.Padding(2);
            this.grpBox_Configuration.Name = "grpBox_Configuration";
            this.grpBox_Configuration.Padding = new System.Windows.Forms.Padding(2);
            this.grpBox_Configuration.Size = new System.Drawing.Size(160, 628);
            this.grpBox_Configuration.TabIndex = 0;
            this.grpBox_Configuration.TabStop = false;
            this.grpBox_Configuration.Text = "Configuration";
            // 
            // btn_connection
            // 
            this.btn_connection.Location = new System.Drawing.Point(5, 351);
            this.btn_connection.Name = "btn_connection";
            this.btn_connection.Size = new System.Drawing.Size(133, 99);
            this.btn_connection.TabIndex = 8;
            this.btn_connection.Text = "Connection";
            this.btn_connection.UseVisualStyleBackColor = true;
            this.btn_connection.Click += new System.EventHandler(this.btn_connection_Click);
            // 
            // label_DataReceived
            // 
            this.label_DataReceived.AutoSize = true;
            this.label_DataReceived.Location = new System.Drawing.Point(76, 216);
            this.label_DataReceived.Name = "label_DataReceived";
            this.label_DataReceived.Size = new System.Drawing.Size(33, 12);
            this.label_DataReceived.TabIndex = 4;
            this.label_DataReceived.Text = "label1";
            // 
            // label_ByteReceived
            // 
            this.label_ByteReceived.AutoSize = true;
            this.label_ByteReceived.Location = new System.Drawing.Point(8, 182);
            this.label_ByteReceived.Name = "label_ByteReceived";
            this.label_ByteReceived.Size = new System.Drawing.Size(73, 12);
            this.label_ByteReceived.TabIndex = 4;
            this.label_ByteReceived.Text = "Byte Received";
            // 
            // cmbx_baudrate
            // 
            this.cmbx_baudrate.FormattingEnabled = true;
            this.cmbx_baudrate.Items.AddRange(new object[] {
            "9600",
            "115200"});
            this.cmbx_baudrate.Location = new System.Drawing.Point(79, 24);
            this.cmbx_baudrate.Name = "cmbx_baudrate";
            this.cmbx_baudrate.Size = new System.Drawing.Size(76, 20);
            this.cmbx_baudrate.TabIndex = 7;
            this.cmbx_baudrate.Text = "9600";
            // 
            // txtbx_addr
            // 
            this.txtbx_addr.Location = new System.Drawing.Point(102, 90);
            this.txtbx_addr.Margin = new System.Windows.Forms.Padding(2);
            this.txtbx_addr.Name = "txtbx_addr";
            this.txtbx_addr.Size = new System.Drawing.Size(50, 22);
            this.txtbx_addr.TabIndex = 0;
            this.txtbx_addr.Text = "1";
            // 
            // label_Addr
            // 
            this.label_Addr.AutoSize = true;
            this.label_Addr.Location = new System.Drawing.Point(10, 92);
            this.label_Addr.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_Addr.Name = "label_Addr";
            this.label_Addr.Size = new System.Drawing.Size(70, 12);
            this.label_Addr.TabIndex = 4;
            this.label_Addr.Text = "Addr (0~255)";
            // 
            // btn_refresh
            // 
            this.btn_refresh.Location = new System.Drawing.Point(13, 576);
            this.btn_refresh.Margin = new System.Windows.Forms.Padding(2);
            this.btn_refresh.Name = "btn_refresh";
            this.btn_refresh.Size = new System.Drawing.Size(125, 37);
            this.btn_refresh.TabIndex = 2;
            this.btn_refresh.Text = "Reconnect Comport";
            this.btn_refresh.UseVisualStyleBackColor = true;
            this.btn_refresh.Click += new System.EventHandler(this.btn_refresh_Click);
            // 
            // label_Baudrate
            // 
            this.label_Baudrate.AutoSize = true;
            this.label_Baudrate.Location = new System.Drawing.Point(11, 24);
            this.label_Baudrate.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_Baudrate.Name = "label_Baudrate";
            this.label_Baudrate.Size = new System.Drawing.Size(47, 12);
            this.label_Baudrate.TabIndex = 4;
            this.label_Baudrate.Text = "Baudrate";
            // 
            // btn_Power
            // 
            this.btn_Power.Font = new System.Drawing.Font("Times New Roman", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_Power.Location = new System.Drawing.Point(9, 473);
            this.btn_Power.Name = "btn_Power";
            this.btn_Power.Size = new System.Drawing.Size(129, 88);
            this.btn_Power.TabIndex = 2;
            this.btn_Power.Text = "PowerON";
            this.btn_Power.UseVisualStyleBackColor = true;
            this.btn_Power.Click += new System.EventHandler(this.btn_Power_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(18, 417);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(504, 22);
            this.textBox1.TabIndex = 2;
            // 
            // btn_sendcmd
            // 
            this.btn_sendcmd.Enabled = false;
            this.btn_sendcmd.Location = new System.Drawing.Point(360, 72);
            this.btn_sendcmd.Margin = new System.Windows.Forms.Padding(2);
            this.btn_sendcmd.Name = "btn_sendcmd";
            this.btn_sendcmd.Size = new System.Drawing.Size(130, 50);
            this.btn_sendcmd.TabIndex = 2;
            this.btn_sendcmd.Text = "設定電壓電流";
            this.btn_sendcmd.UseVisualStyleBackColor = true;
            this.btn_sendcmd.Click += new System.EventHandler(this.btn_sendcmd_Click);
            // 
            // cmbx_com
            // 
            this.cmbx_com.FormattingEnabled = true;
            this.cmbx_com.Items.AddRange(new object[] {
            "COM1"});
            this.cmbx_com.Location = new System.Drawing.Point(92, 80);
            this.cmbx_com.Name = "cmbx_com";
            this.cmbx_com.Size = new System.Drawing.Size(82, 20);
            this.cmbx_com.TabIndex = 6;
            this.cmbx_com.SelectedIndexChanged += new System.EventHandler(this.cmbx_com_SelectedIndexChanged);
            // 
            // label_com
            // 
            this.label_com.AutoSize = true;
            this.label_com.Location = new System.Drawing.Point(56, 83);
            this.label_com.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_com.Name = "label_com";
            this.label_com.Size = new System.Drawing.Size(31, 12);
            this.label_com.TabIndex = 4;
            this.label_com.Text = "COM";
            // 
            // label_OVP
            // 
            this.label_OVP.AutoSize = true;
            this.label_OVP.Location = new System.Drawing.Point(42, 28);
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
            this.grpBox_dashboard.Controls.Add(this.textBox1);
            this.grpBox_dashboard.Controls.Add(this.txtbx_Vset);
            this.grpBox_dashboard.Controls.Add(this.richTextBox1);
            this.grpBox_dashboard.Controls.Add(this.txtbx_Vin);
            this.grpBox_dashboard.Controls.Add(this.label_Vin);
            this.grpBox_dashboard.Controls.Add(this.txtbx_ocp);
            this.grpBox_dashboard.Controls.Add(this.label_OCP);
            this.grpBox_dashboard.Controls.Add(this.btn_sendcmd);
            this.grpBox_dashboard.Controls.Add(this.txtbx_ovp);
            this.grpBox_dashboard.Controls.Add(this.label_Iset);
            this.grpBox_dashboard.Controls.Add(this.label_OVP);
            this.grpBox_dashboard.Controls.Add(this.label_Vset);
            this.grpBox_dashboard.Location = new System.Drawing.Point(188, 14);
            this.grpBox_dashboard.Margin = new System.Windows.Forms.Padding(2);
            this.grpBox_dashboard.Name = "grpBox_dashboard";
            this.grpBox_dashboard.Padding = new System.Windows.Forms.Padding(2);
            this.grpBox_dashboard.Size = new System.Drawing.Size(546, 490);
            this.grpBox_dashboard.TabIndex = 1;
            this.grpBox_dashboard.TabStop = false;
            this.grpBox_dashboard.Text = "Dashboard";
            // 
            // txtbx_Iset
            // 
            this.txtbx_Iset.Enabled = false;
            this.txtbx_Iset.Location = new System.Drawing.Point(258, 84);
            this.txtbx_Iset.Margin = new System.Windows.Forms.Padding(2);
            this.txtbx_Iset.Name = "txtbx_Iset";
            this.txtbx_Iset.Size = new System.Drawing.Size(72, 22);
            this.txtbx_Iset.TabIndex = 0;
            this.txtbx_Iset.Text = "1";
            // 
            // txtbx_Vset
            // 
            this.txtbx_Vset.Enabled = false;
            this.txtbx_Vset.Location = new System.Drawing.Point(98, 81);
            this.txtbx_Vset.Margin = new System.Windows.Forms.Padding(2);
            this.txtbx_Vset.Name = "txtbx_Vset";
            this.txtbx_Vset.Size = new System.Drawing.Size(72, 22);
            this.txtbx_Vset.TabIndex = 0;
            this.txtbx_Vset.Text = "1.38";
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(18, 168);
            this.richTextBox1.Margin = new System.Windows.Forms.Padding(2);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(472, 218);
            this.richTextBox1.TabIndex = 2;
            this.richTextBox1.Text = "";
            // 
            // txtbx_Vin
            // 
            this.txtbx_Vin.Location = new System.Drawing.Point(418, 25);
            this.txtbx_Vin.Margin = new System.Windows.Forms.Padding(2);
            this.txtbx_Vin.Name = "txtbx_Vin";
            this.txtbx_Vin.Size = new System.Drawing.Size(72, 22);
            this.txtbx_Vin.TabIndex = 0;
            // 
            // label_Vin
            // 
            this.label_Vin.AutoSize = true;
            this.label_Vin.Location = new System.Drawing.Point(358, 28);
            this.label_Vin.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_Vin.Name = "label_Vin";
            this.label_Vin.Size = new System.Drawing.Size(22, 12);
            this.label_Vin.TabIndex = 4;
            this.label_Vin.Text = "Vin";
            // 
            // txtbx_ocp
            // 
            this.txtbx_ocp.Location = new System.Drawing.Point(258, 25);
            this.txtbx_ocp.Margin = new System.Windows.Forms.Padding(2);
            this.txtbx_ocp.Name = "txtbx_ocp";
            this.txtbx_ocp.Size = new System.Drawing.Size(72, 22);
            this.txtbx_ocp.TabIndex = 0;
            // 
            // label_OCP
            // 
            this.label_OCP.AutoSize = true;
            this.label_OCP.Location = new System.Drawing.Point(200, 28);
            this.label_OCP.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_OCP.Name = "label_OCP";
            this.label_OCP.Size = new System.Drawing.Size(27, 12);
            this.label_OCP.TabIndex = 4;
            this.label_OCP.Text = "OCP";
            // 
            // txtbx_ovp
            // 
            this.txtbx_ovp.Location = new System.Drawing.Point(98, 25);
            this.txtbx_ovp.Margin = new System.Windows.Forms.Padding(2);
            this.txtbx_ovp.Name = "txtbx_ovp";
            this.txtbx_ovp.Size = new System.Drawing.Size(72, 22);
            this.txtbx_ovp.TabIndex = 0;
            // 
            // label_Iset
            // 
            this.label_Iset.AutoSize = true;
            this.label_Iset.Location = new System.Drawing.Point(188, 91);
            this.label_Iset.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_Iset.Name = "label_Iset";
            this.label_Iset.Size = new System.Drawing.Size(52, 12);
            this.label_Iset.TabIndex = 4;
            this.label_Iset.Text = "Iset (~5A)";
            // 
            // label_Vset
            // 
            this.label_Vset.AutoSize = true;
            this.label_Vset.Location = new System.Drawing.Point(16, 89);
            this.label_Vset.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_Vset.Name = "label_Vset";
            this.label_Vset.Size = new System.Drawing.Size(66, 12);
            this.label_Vset.TabIndex = 4;
            this.label_Vset.Text = "Vset (6~55v)";
            // 
            // label_WuzhiCmd
            // 
            this.label_WuzhiCmd.AutoSize = true;
            this.label_WuzhiCmd.Location = new System.Drawing.Point(56, 21);
            this.label_WuzhiCmd.Name = "label_WuzhiCmd";
            this.label_WuzhiCmd.Size = new System.Drawing.Size(59, 12);
            this.label_WuzhiCmd.TabIndex = 6;
            this.label_WuzhiCmd.Text = "WuzhiCmd";
            // 
            // txtbx_WuzhiCmd
            // 
            this.txtbx_WuzhiCmd.Location = new System.Drawing.Point(58, 49);
            this.txtbx_WuzhiCmd.Name = "txtbx_WuzhiCmd";
            this.txtbx_WuzhiCmd.Size = new System.Drawing.Size(388, 22);
            this.txtbx_WuzhiCmd.TabIndex = 5;
            this.txtbx_WuzhiCmd.Text = "aa 01 22 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ce";
            // 
            // radiobtn_hexadded
            // 
            this.radiobtn_hexadded.AutoSize = true;
            this.radiobtn_hexadded.Location = new System.Drawing.Point(462, 50);
            this.radiobtn_hexadded.Name = "radiobtn_hexadded";
            this.radiobtn_hexadded.Size = new System.Drawing.Size(83, 16);
            this.radiobtn_hexadded.TabIndex = 7;
            this.radiobtn_hexadded.TabStop = true;
            this.radiobtn_hexadded.Text = "HexAddition";
            this.radiobtn_hexadded.UseVisualStyleBackColor = true;
            this.radiobtn_hexadded.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
            // 
            // tabCtrl_Dashboard
            // 
            this.tabCtrl_Dashboard.Controls.Add(this.tabCtrl1);
            this.tabCtrl_Dashboard.Controls.Add(this.tabCtrl2);
            this.tabCtrl_Dashboard.Dock = System.Windows.Forms.DockStyle.Left;
            this.tabCtrl_Dashboard.Location = new System.Drawing.Point(0, 0);
            this.tabCtrl_Dashboard.Name = "tabCtrl_Dashboard";
            this.tabCtrl_Dashboard.SelectedIndex = 0;
            this.tabCtrl_Dashboard.Size = new System.Drawing.Size(759, 660);
            this.tabCtrl_Dashboard.TabIndex = 2;
            // 
            // tabCtrl1
            // 
            this.tabCtrl1.Controls.Add(this.grpBox_Configuration);
            this.tabCtrl1.Controls.Add(this.grpBox_dashboard);
            this.tabCtrl1.Location = new System.Drawing.Point(4, 22);
            this.tabCtrl1.Name = "tabCtrl1";
            this.tabCtrl1.Padding = new System.Windows.Forms.Padding(3);
            this.tabCtrl1.Size = new System.Drawing.Size(751, 634);
            this.tabCtrl1.TabIndex = 0;
            this.tabCtrl1.Text = "Dashboard";
            this.tabCtrl1.UseVisualStyleBackColor = true;
            // 
            // tabCtrl2
            // 
            this.tabCtrl2.Controls.Add(this.groupBox2);
            this.tabCtrl2.Controls.Add(this.groupBox1);
            this.tabCtrl2.Location = new System.Drawing.Point(4, 22);
            this.tabCtrl2.Name = "tabCtrl2";
            this.tabCtrl2.Padding = new System.Windows.Forms.Padding(3);
            this.tabCtrl2.Size = new System.Drawing.Size(751, 634);
            this.tabCtrl2.TabIndex = 1;
            this.tabCtrl2.Text = "DebugPage";
            this.tabCtrl2.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox2.Controls.Add(this.btn_TryCmd);
            this.groupBox2.Controls.Add(this.txtbx_TryCmd);
            this.groupBox2.Controls.Add(this.label_serial1DataReceived);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox2.Location = new System.Drawing.Point(3, 119);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(745, 221);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "groupBox2";
            // 
            // btn_TryCmd
            // 
            this.btn_TryCmd.Location = new System.Drawing.Point(569, 38);
            this.btn_TryCmd.Name = "btn_TryCmd";
            this.btn_TryCmd.Size = new System.Drawing.Size(128, 35);
            this.btn_TryCmd.TabIndex = 5;
            this.btn_TryCmd.Text = "TryCmd";
            this.btn_TryCmd.UseVisualStyleBackColor = true;
            this.btn_TryCmd.Click += new System.EventHandler(this.btn_TryCmd_Click);
            // 
            // txtbx_TryCmd
            // 
            this.txtbx_TryCmd.Location = new System.Drawing.Point(58, 51);
            this.txtbx_TryCmd.Name = "txtbx_TryCmd";
            this.txtbx_TryCmd.Size = new System.Drawing.Size(451, 22);
            this.txtbx_TryCmd.TabIndex = 8;
            // 
            // label_serial1DataReceived
            // 
            this.label_serial1DataReceived.AutoSize = true;
            this.label_serial1DataReceived.Location = new System.Drawing.Point(56, 18);
            this.label_serial1DataReceived.Name = "label_serial1DataReceived";
            this.label_serial1DataReceived.Size = new System.Drawing.Size(178, 12);
            this.label_serial1DataReceived.TabIndex = 6;
            this.label_serial1DataReceived.Text = "serial1DataReceived or Hex Addition";
            // 
            // groupBox1
            // 
            this.groupBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox1.BackColor = System.Drawing.Color.Silver;
            this.groupBox1.Controls.Add(this.radiobtn_hexadded);
            this.groupBox1.Controls.Add(this.cmbx_com);
            this.groupBox1.Controls.Add(this.label_WuzhiCmd);
            this.groupBox1.Controls.Add(this.btn_hexaddition);
            this.groupBox1.Controls.Add(this.label_com);
            this.groupBox1.Controls.Add(this.txtbx_WuzhiCmd);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(745, 116);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "groupBox1";
            // 
            // btn_hexaddition
            // 
            this.btn_hexaddition.Location = new System.Drawing.Point(579, 21);
            this.btn_hexaddition.Name = "btn_hexaddition";
            this.btn_hexaddition.Size = new System.Drawing.Size(118, 74);
            this.btn_hexaddition.TabIndex = 2;
            this.btn_hexaddition.Text = "_hexaddition";
            this.btn_hexaddition.UseVisualStyleBackColor = true;
            this.btn_hexaddition.Click += new System.EventHandler(this.btn_hexaddition_Click);
            // 
            // Form1
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(755, 660);
            this.Controls.Add(this.tabCtrl_Dashboard);
            this.IsMdiContainer = true;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.grpBox_Configuration.ResumeLayout(false);
            this.grpBox_Configuration.PerformLayout();
            this.grpBox_dashboard.ResumeLayout(false);
            this.grpBox_dashboard.PerformLayout();
            this.tabCtrl_Dashboard.ResumeLayout(false);
            this.tabCtrl1.ResumeLayout(false);
            this.tabCtrl1.PerformLayout();
            this.tabCtrl2.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.IO.Ports.SerialPort serialPort1;
        private System.Windows.Forms.GroupBox grpBox_Configuration;
        private System.Windows.Forms.GroupBox grpBox_dashboard;
        private System.Windows.Forms.Button btn_refresh;
        private System.Windows.Forms.Label label_OVP;
        private System.Windows.Forms.Label label_com;
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
        private System.Windows.Forms.Label label_WuzhiCmd;
        private System.Windows.Forms.TextBox txtbx_WuzhiCmd;
        private System.Windows.Forms.RadioButton radiobtn_hexadded;
        private System.Windows.Forms.ComboBox cmbx_com;
        private System.Windows.Forms.ComboBox cmbx_baudrate;
        private System.Windows.Forms.Button btn_Power;
        private System.Windows.Forms.TabControl tabCtrl_Dashboard;
        private System.Windows.Forms.TabPage tabCtrl1;
        private System.Windows.Forms.TabPage tabCtrl2;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btn_hexaddition;
        private System.Windows.Forms.Label label_DataReceived;
        private System.Windows.Forms.Label label_ByteReceived;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox txtbx_TryCmd;
        private System.Windows.Forms.Label label_serial1DataReceived;
        private System.Windows.Forms.Button btn_TryCmd;
        private System.Windows.Forms.Button btn_connection;
    }
}

