using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using WNC.API;
using System.Text.RegularExpressions;
using System.Diagnostics;
using EasyLibrary;

namespace MiniPwrSupply.LCS5
{
    public partial class frmConnectInterfaceMain
    {
        public enum PortType
        {
            DUT_UART,
            DIAG,
            FIXTURE,
            UART,
            TELNET,
            TELNET2,
            GOLDEN_UART_6G_BT,
            GOLDEN_UART_2G_5G,
            GOLDEN_TELNET_6G_BT,
            GOLDEN_TELNET_2G_5G,
            JS7K_GOLDEN,
            SSH,
        }

        private enum ITEM
        {
            SET_BOOTCMD,
            CHK_SERIALNUMBER,

            BT_RSSI,
            WiFi_2G_RSSI,
            WiFi_5G_RSSI,
            WiFi_6G_RSSI,
            NONE,

            LED,
        }


        private enum COMMAND
        {
            NONE,
        }

        private enum Mode
        {
            OffLine = 0x00,
            OnLine = 0x01,
            Ftm = 0x05,
            LowPower = 0x07,
            Ask = 0x0A,
            Unknow = 0x0B,
        }

        private System.IO.Ports.SerialPort uart;
        private System.IO.Ports.SerialPort serialPort;
        private System.IO.Ports.SerialPort js7k;
        private System.IO.Ports.SerialPort golden6G_BT;
        private System.IO.Ports.SerialPort golden2G_5G;
        private System.IO.Ports.SerialPort atCmdUart;
        private System.IO.Ports.SerialPort shieldUart;
        private Telnet telnet;
        private Telnet telnet2;
        private Telnet golden2G5GTelnet;
        private Telnet golden6GTelnet;

        private string diagPort = string.Empty;
        private bool portExist = false;

        private bool useShield = false;

        //Rena_20221006 disable Shielding®É·|exception,©Ò¥H­n¥ýnew
        //private Fixture fixture;
        private Fixture fixture = new Fixture();

        private void TelnetParameter()
        {
            DisplayMsg(LogType.Log, "====== Telnet ======");

            try
            {
                telnet = new Telnet("LCS5_Telnet");
                telnet2 = new Telnet("LCS5_Telnet");
                DisplayMsg(LogType.Log, "Device IP : " + telnet.IP);
                DisplayMsg(LogType.Log, "Device Port : " + telnet.Port.ToString());

                golden2G5GTelnet = new Telnet("Golden_2G5G_Telnet");

                DisplayMsg(LogType.Log, "Golden 2G/5G Device IP : " + golden2G5GTelnet.IP);
                DisplayMsg(LogType.Log, "Device Port : " + golden2G5GTelnet.Port.ToString());

                golden6GTelnet = new Telnet("Golden_6G_Telnet");

                DisplayMsg(LogType.Log, "Golden 6G Device IP : " + golden6GTelnet.IP);
                DisplayMsg(LogType.Log, "Device Port : " + golden6GTelnet.Port.ToString());
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, "Init error: " + ex.ToString());
            }
        }
        private void UartTestParameter()
        {
            DisplayMsg(LogType.Log, "====== Uart ======");

            try
            {
                #region uart
                if (Func.ReadINI("Setting", "Port", "UART", "Disable") == "Enable")
                {
                    uart = new System.IO.Ports.SerialPort();
                    uart.PortName = Func.ReadINI("Setting", "Port", "UartPort", "COM0");
                    uart.BaudRate = 115200;
                    uart.StopBits = StopBits.One;
                    uart.Parity = Parity.None;
                    uart.DataBits = 8;
                    uart.RtsEnable = false;
                    DisplayMsg(LogType.Log, "Uart Port : " + uart.PortName);
                }
                #endregion

                #region bt golden_1
                golden6G_BT = new System.IO.Ports.SerialPort();
                golden6G_BT.PortName = Func.ReadINI("Setting", "Port", "WiFi_6G_BT_Golden", "COM0");
                golden6G_BT.BaudRate = 115200;
                golden6G_BT.StopBits = StopBits.One;
                golden6G_BT.Parity = Parity.None;
                golden6G_BT.DataBits = 8;
                golden6G_BT.RtsEnable = false;
                DisplayMsg(LogType.Log, "Golden 1 Port : " + golden6G_BT.PortName);
                #endregion

                #region bt golden_2
                golden2G_5G = new System.IO.Ports.SerialPort();
                golden2G_5G.PortName = Func.ReadINI("Setting", "Port", "WiFi_2G_5G_Golden", "COM0");
                golden2G_5G.BaudRate = 115200;
                golden2G_5G.StopBits = StopBits.One;
                golden2G_5G.Parity = Parity.None;
                golden2G_5G.DataBits = 8;
                golden2G_5G.RtsEnable = false;
                DisplayMsg(LogType.Log, "Golden 2 Port : " + golden2G_5G.PortName);
                #endregion

                #region shielding
                if (Func.ReadINI("Setting", "Port", "Shielding", "Disable") == "Enable")
                {
                    string openCmd = Func.ReadINI("Setting", "Port", "ShieldingOpen", "open");
                    string closeCmd = Func.ReadINI("Setting", "Port", "ShieldingClose", "close");

                    useShield = true;
                    shieldUart = new System.IO.Ports.SerialPort();
                    shieldUart.PortName = Func.ReadINI("Setting", "Port", "ShieldingPort", "COM0");
                    shieldUart.BaudRate = 9600;
                    shieldUart.StopBits = StopBits.One;
                    shieldUart.Parity = Parity.None;
                    shieldUart.DataBits = 8;
                    shieldUart.RtsEnable = false;
                    DisplayMsg(LogType.Log, "Shielding Port : " + shieldUart.PortName);
                    //fixture = new Fixture(shieldUart, 5000);

                    fixture = new Fixture(shieldUart, 8000, openCmd, closeCmd, "ok", "ready");
                }
                else
                {
                    useShield = false;
                    DisplayMsg(LogType.Log, "Not auto control shielding");
                }
                #endregion

                if (WNC.API.Func.ReadINI("Setting", "CheckWeight", "CheckWeight", "0") == "1")
                {
                    serialPort = new SerialPort();
                    serialPort.DataReceived += new SerialDataReceivedEventHandler(this.serialPort_DataReceived);
                    if (serialPort.IsOpen)
                        serialPort.Close();
                    string setting_PortName = Func.ReadINI("Setting", "CheckWeight", "PortName", "COM1");
                    string setting_Baudrate = Func.ReadINI("Setting", "CheckWeight", "Baudrate", "9600");

                    switch (Func.ReadINI("Setting", "CheckWeight", "Parity", "NONE").Trim().ToUpper())
                    {
                        case "ODD":
                            serialPort.Parity = Parity.Even;
                            break;
                        case "EVEN":
                            serialPort.Parity = Parity.Odd;
                            break;
                        case "MARK":
                            serialPort.Parity = Parity.Mark;
                            break;
                        case "SPACE":
                            serialPort.Parity = Parity.Space;
                            break;
                        default:
                            serialPort.Parity = Parity.None;
                            break;
                    }
                    switch (Func.ReadINI("Setting", "CheckWeight", "StopBits", "ONE").Trim().ToUpper())
                    {
                        case "TWO":
                            serialPort.StopBits = StopBits.Two;
                            break;
                        case "ONEPOINTFIVE":
                            serialPort.StopBits = StopBits.OnePointFive;
                            break;
                        default:
                            serialPort.StopBits = StopBits.One;
                            break;
                    }
                    //serialPort.Parity
                    serialPort.BaudRate = Convert.ToInt32(setting_Baudrate);
                    serialPort.PortName = setting_PortName;
                    serialPort.Open();
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, "Init error: " + ex.ToString());
                MessageBox.Show($"Init {serialPort.PortName} fail");
                Application.Exit();
            }
        }

        private void UartInitial()
        {
            if (useShield)
            {
                fixture = new Fixture(shieldUart, 5000);
            }

        }

        private bool UartInitial(SerialPort port)
        {
            try
            {
                if (!ChkPort(port.PortName))
                {
                    DisplayMsg(LogType.Error, "Not find port : " + port.PortName);
                    MessageBox.Show("Not find port : " + port.PortName);
                    return false;
                }

                if (port == null)
                {
                    DisplayMsg(LogType.Log, "Initial uart fail");
                    return false;
                }

                if (port.IsOpen)
                {
                    DisplayMsg(LogType.Log, "Close uart port");
                    port.DiscardInBuffer();
                    port.DiscardOutBuffer();
                    port.Dispose();
                    port.Close();
                }

                Thread.Sleep(100);

                if (!port.IsOpen)
                {
                    DisplayMsg(LogType.Log, "Open uart port");
                    port.Open();
                    port.DiscardInBuffer();
                    port.DiscardOutBuffer();
                }

                Thread.Sleep(100);

                return true;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, "Initial Uart fail : " + ex.Message);
                return false;
            }
        }

        private void UartDispose(SerialPort port)
        {
            try
            {
                if (port != null)
                {
                    if (!ChkPort(port.PortName))
                    {
                        DisplayMsg(LogType.Error, "Not find port : " + port.PortName);
                        //MessageBox.Show("Not find port : " + port.PortName);
                        return;
                    }

                    if (port.IsOpen)
                    {
                        DisplayMsg(LogType.Log, "Close uart port");
                        port.DiscardInBuffer();
                        port.DiscardOutBuffer();
                        port.Dispose();
                        port.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
            }
        }

        private void DisposeIface()
        {
            DisplayMsg(LogType.Log, "Dispose telnet");
            telnet.Dispose();
            telnet2.Dispose();
            UartDispose(uart);
            UartDispose(golden6G_BT);
            UartDispose(golden2G_5G);
        }

        private bool ChkBootLoader(PortType port, string keyword, int timeOutMs)
        {
            DateTime dt;
            TimeSpan ts;
            int count = 0;
            string res = string.Empty;
            string log = string.Empty;
            bool result = false;

            dt = DateTime.Now;
        Re:

            res = string.Empty;
            log = string.Empty;
            try
            {
                while (true)
                {
                    ts = new TimeSpan(DateTime.Now.Ticks - dt.Ticks);

                    if (ts.TotalMilliseconds > timeOutMs)
                    {
                        DisplayMsg(LogType.Error, "timeOut");
                        return false;
                    }

                    System.Threading.Thread.Sleep(50);

                    switch (port)
                    {
                        case PortType.UART:
                            #region PortType.UART
                            if (!ChkPort(uart.PortName))
                            {
                                DisplayMsg(LogType.Error, "Not find port : " + uart.PortName);
                                MessageBox.Show("Not find port : " + uart.PortName);
                                return false;
                            }

                            if (!uart.IsOpen)
                            {
                                DisplayMsg(LogType.Log, "Open uart port");
                                uart.Open();
                            }


                            res = uart.ReadExisting();

                            if (res.Length != 0 && res != "\r\n")
                            {
                                DisplayMsg(LogType.Uart, res);
                                log += res;
                            }

                            break;
                        #endregion
                        default:
                            break;
                    }

                    if (log.Contains("Hit any key to stop autoboot") && !result)
                    {
                        uart.Write("\n");

                        System.Threading.Thread.Sleep(100);
                    }

                    if (log.Contains(keyword))
                    {
                        result = true;
                        return true;
                    }
                }

            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                if (count++ < 3)
                {
                    DisplayMsg(LogType.Retry, "Retry count : " + count.ToString());
                    GcErase(port);
                    goto Re;
                }
                return false;
            }
        }

        private bool ChkInitial(PortType portType, string keyword, int timeOutMs)
        {
            DateTime dt;
            TimeSpan ts;
            int count = 0;
            string res = string.Empty;
            string log = string.Empty;

            dt = DateTime.Now;
        Re:

            res = string.Empty;
            log = string.Empty;
            try
            {
                while (true)
                {
                    ts = new TimeSpan(DateTime.Now.Ticks - dt.Ticks);

                    if (ts.TotalMilliseconds > timeOutMs)
                    {
                        DisplayMsg(LogType.Error, "timeOut");
                        return false;
                    }

                    switch (portType)
                    {
                        case PortType.UART:
                            #region PortType.UART
                            if (!ChkPort(uart.PortName))
                            {
                                DisplayMsg(LogType.Error, "Not find port : " + uart.PortName);
                                MessageBox.Show("Not find port : " + uart.PortName);
                                return false;
                            }

                            if (!uart.IsOpen)
                            {
                                DisplayMsg(LogType.Log, "Open uart port");
                                uart.Open();
                            }

                            //uart.Write("\r\n");

                            //System.Threading.Thread.Sleep(200);

                            res = uart.ReadExisting();

                            if (res.Length != 0 && res != "\r\n")
                            {
                                DisplayMsg(LogType.Uart, res);
                                log += res;
                            }
                            break;
                        #endregion
                        case PortType.TELNET:
                            #region PortType.TELNET
                            telnet.Dispose();
                            if (!telnet.Ping(telnet.IP, timeOutMs))
                            {
                                DisplayMsg(LogType.Log, $"Ping {telnet.IP} fail..");
                                return false;
                            }
                            //Thread.Sleep(2000);
                            //DisplayMsg(LogType.Log, "Delay 2000ms...");

                            int retry = 3;
                            while (retry-- > 0)
                            {
                                if (telnet.LoginTelnet(keyword, 10 * 1000))
                                {
                                    return true;
                                }
                                else
                                {
                                    DisplayMsg(LogType.Log, "telnet login fail, retry...");
                                    telnet.Dispose();
                                    System.Threading.Thread.Sleep(1000);
                                    DisplayMsg(LogType.Error, @"SHOW ERR for Shelly");
                                }
                            }
                            MessageBox.Show("NG need PE check");
                            return false;
                        #endregion
                        case PortType.TELNET2:
                            #region PortType.TELNET2
                            telnet2.Dispose();

                            if (!telnet2.Ping(telnet2.IP, timeOutMs))
                            {
                                //MessageBox.Show("Ping fail....G?I PE ?? XAC NH?N!");
                                DisplayMsg(LogType.Log, $"Ping {telnet2.IP} fail..");
                                return false;
                            }

                            //Rena_20221013 add telnet retry
                            int retry2 = 3;
                            while (retry2-- > 0)
                            {
                                if (telnet2.LoginTelnet(keyword, 10 * 1000))
                                {
                                    return true;
                                }
                                else
                                {
                                    DisplayMsg(LogType.Log, "telnet2 login fail, retry...");
                                    telnet2.Dispose();
                                    System.Threading.Thread.Sleep(1000);
                                }
                            }
                            return false;
                        //return telnet.LoginTelnet(keyword, 10 * 1000);
                        #endregion
                        default:
                            break;
                    }

                    if (log.Contains("system ready!"))  //"wnc_bt_init: starting daemon"
                    {
                        SendCommand(portType, "\r\n", 100);
                    }

                    if (log.Contains(keyword))
                    {
                        return true;
                    }

                    System.Threading.Thread.Sleep(50);
                }

            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                if (count++ < 3)
                {
                    DisplayMsg(LogType.Retry, "Retry count : " + count.ToString());
                    GcErase(portType);
                    goto Re;
                }
                return false;
            }
        }

        private string GetCommand(PortType portType, ITEM item, string parameter)
        {
            switch (portType)
            {
                case PortType.TELNET:
                case PortType.UART:
                    switch (item)
                    {
                        case ITEM.SET_BOOTCMD:
                            return "fw_setenv bootcmd bootipq";
                        case ITEM.CHK_SERIALNUMBER:
                            return "fw_printenv serialnumber";
                        default:
                            break;
                    }
                    break;
                case PortType.DUT_UART:
                    #region PortType.DUT_UART
                    switch (item)
                    {
                        default:
                            break;
                    }
                    break;
                #endregion
                default:
                    break;
            }

            return string.Empty;
        }

        private bool SendCommand(PortType portType, string cmd, int delayMs)
        {
            int count = 0;
        //DisplayMsg(LogType.Log, uart.PortName);
        Re:
            try
            {
                switch (portType)
                {
                    case PortType.UART:
                        #region PortType.UART
                        if (!ChkPort(uart.PortName))
                        {
                            DisplayMsg(LogType.Error, "Not find port : " + uart.PortName);
                            //MessageBox.Show("Not find port : " + uart.PortName);
                            return false;
                        }

                        if (!uart.IsOpen)
                        {
                            DisplayMsg(LogType.Log, "Open uart port");
                            uart.Open();
                        }

                        DisplayMsg(LogType.Log, "Transmitted '" + cmd + "' to device");

                        uart.Write(cmd + "\r");

                        break;
                    #endregion
                    case PortType.JS7K_GOLDEN:
                        #region PortType.JS7K_GOLDEN
                        if (!ChkPort(js7k.PortName))
                        {
                            DisplayMsg(LogType.Error, "Not find golden port : " + js7k.PortName);
                            MessageBox.Show("Not find golden port : " + js7k.PortName);
                            return false;
                        }

                        if (!js7k.IsOpen)
                        {
                            DisplayMsg(LogType.JS7K, "Open golden port");
                            js7k.Open();
                        }

                        DisplayMsg(LogType.JS7K, "Transmitted '" + cmd + "' to device");

                        js7k.Write(cmd + "\r");

                        break;
                    #endregion
                    case PortType.GOLDEN_UART_6G_BT:
                        #region PortType.GOLDEN_UART_6G_BT
                        if (!ChkPort(golden6G_BT.PortName))
                        {
                            DisplayMsg(LogType.Error, "Not find golden port : " + golden6G_BT.PortName);
                            MessageBox.Show("Not find golden port : " + golden6G_BT.PortName);
                            return false;
                        }

                        if (!golden6G_BT.IsOpen)
                        {
                            DisplayMsg(LogType.GOLDEN_UART_6G_BT, "Open golden port");
                            golden6G_BT.Open();
                        }

                        DisplayMsg(LogType.GOLDEN_UART_6G_BT, "Transmitted '" + cmd + "' to device");

                        golden6G_BT.Write(cmd + "\r");

                        break;
                    #endregion
                    case PortType.GOLDEN_UART_2G_5G:
                        #region PortType.GOLDEN_UART_2G_5G
                        if (!ChkPort(golden2G_5G.PortName))
                        {
                            DisplayMsg(LogType.Error, "Not find golden port : " + golden2G_5G.PortName);
                            MessageBox.Show("Not find golden port : " + golden2G_5G.PortName);
                            return false;
                        }

                        if (!golden2G_5G.IsOpen)
                        {
                            DisplayMsg(LogType.GOLDEN_UART_2G_5G, "Open golden port");
                            golden2G_5G.Open();
                        }

                        DisplayMsg(LogType.GOLDEN_UART_2G_5G, "Transmitted '" + cmd + "' to device");

                        golden2G_5G.Write(cmd + "\r");

                        break;
                    #endregion
                    case PortType.TELNET:
                        #region PortType.TELNET
                        //DisplayMsg(LogType.Log, "====Debuger===");
                        if (!telnet.IsConnect)
                        {
                            //DisplayMsg(LogType.Log, "====Debuger3===");
                            DisplayMsg(LogType.Error, "Telnet diconnect : " + telnet.IP);
                            //DisplayMsg(LogType.Log, "====Debuger4===");
                            DisplayMsg(LogType.Error, "Login telnet again : " + telnet.IP);
                            //DisplayMsg(LogType.Log, "====Debuger5===");

                            Thread.Sleep(3000);
                            if (!telnet.LoginTelnet("root", 20000))
                            {
                                return false;
                            }
                        }

                        DisplayMsg(LogType.Log, "Transmitted '" + cmd + "' to device");

                        telnet.Transmit(cmd);
                        break;
                    #endregion
                    case PortType.TELNET2:
                        #region PortType.TELNET2
                        if (!telnet2.IsConnect)
                        {
                            DisplayMsg(LogType.Error, "Telnet 2 diconnect : " + telnet2.IP);
                            DisplayMsg(LogType.Error, "Login telnet 2 again : " + telnet2.IP);
                            Thread.Sleep(3000);
                            if (!telnet2.LoginTelnet("root", 20000))
                                return false;
                        }

                        DisplayMsg(LogType.Log, "Transmitted telnet 2 '" + cmd + "' to device");

                        if (!telnet2.Transmit(cmd))
                        {
                            if (count++ < 3)
                            {
                                DisplayMsg(LogType.Retry, "Retry count : " + count.ToString());
                                GcErase(portType);
                                goto Re;
                            }
                        }
                        break;
                    #endregion
                    case PortType.GOLDEN_TELNET_2G_5G:
                        #region PortType.GOLDEN_TELNET_2G_5G
                        if (!golden2G5GTelnet.IsConnect)
                        {
                            DisplayMsg(LogType.Error, "Telnet diconnect : " + golden2G5GTelnet.IP);
                            DisplayMsg(LogType.Error, "Login telnet again : " + golden2G5GTelnet.IP);
                            Thread.Sleep(3000);
                            if (!golden2G5GTelnet.LoginTelnet("root", 20000))
                                return false;
                        }

                        DisplayMsg(LogType.GOLDEN_TELNET_2G_5G, "Transmitted '" + cmd + "' to device");

                        golden2G5GTelnet.Transmit(cmd);
                        break;
                    #endregion
                    case PortType.GOLDEN_TELNET_6G_BT:
                        #region PortType.GOLDEN_TELNET_6G_BT
                        if (!golden6GTelnet.IsConnect)
                        {
                            DisplayMsg(LogType.Error, "Telnet diconnect : " + golden6GTelnet.IP);
                            DisplayMsg(LogType.Error, "Login telnet again : " + golden6GTelnet.IP);
                            Thread.Sleep(3000);
                            if (!golden6GTelnet.LoginTelnet("root", 20000))
                                return false;
                        }

                        DisplayMsg(LogType.GOLDEN_TELNET_6G_BT, "Transmitted '" + cmd + "' to device");

                        golden6GTelnet.Transmit(cmd);
                        break;
                    #endregion
                    case PortType.DUT_UART:
                        #region PortType.DUT_UART
                        if (!ChkPort(atCmdUart.PortName))
                        {
                            status_ATS.AddDataLog("PORT", NG);
                            DisplayMsg(LogType.Error, "Not find port : " + atCmdUart.PortName);
                            //MessageBox.Show("Not find port : " + atCmdUart.PortName);
                            return false;
                        }
                        else
                        {
                            status_ATS.AddDataLog("PORT", PASS);
                        }

                        if (!atCmdUart.IsOpen)
                        {
                            DisplayMsg(LogType.Log, "Open modem port");
                            atCmdUart.Open();
                        }

                        DisplayMsg(LogType.Log, "Transmitted '" + cmd + "' to device");
                        atCmdUart.Write(cmd + "\r\n");
                        break;
                    #endregion
                    default:
                        break;
                }
                delayMs = 0; // RD required 
                if (delayMs > 0)
                {
                    DisplayMsg(LogType.Log, "Delay " + delayMs.ToString() + " (ms)..");
                    System.Threading.Thread.Sleep(delayMs);
                }

                return true;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                if (count++ < 3)
                {
                    DisplayMsg(LogType.Retry, "Retry count : " + count.ToString());
                    GcErase(portType);
                    goto Re;
                }
                return false;
            }
        }
        private bool SendAndChk(string errorItem, PortType portType, string cmd, string keyword, int delayMs, int timeOutMs)
        {
            if (!CheckGoNoGo())
            {
                return false;
            }

            try
            {
                string res = string.Empty;
                bool result = false;

                SendCommand(portType, cmd, delayMs);
                result = ChkResponse(portType, ITEM.NONE, keyword, out res, timeOutMs);

                if (result)
                {
                    return true;
                }

                //status_ATS.AddDataLog(errorItem, NG);
                AddData(errorItem, 1);
                return false;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                //status_ATS.AddDataLog(errorItem, NG);
                AddData(errorItem, 1);
                return false;
            }
        }

        private bool SendAndChk(PortType portType, string cmd, string keyword, out string res, int delayMs, int timeOutMs, int retry = 0)
        {
            res = string.Empty;
            int c = 0;
            try
            {
            retry:
                bool result = false;

                SendCommand(portType, cmd, delayMs);
                result = ChkResponse(portType, ITEM.NONE, keyword, out res, timeOutMs);

                if (result)
                {
                    return true;
                }
                if (c < retry)
                {
                    c++;
                    goto retry;
                }

                return false;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                warning = "Exception";
                return false;
            }
        }
        private bool SendAndChk(PortType portType, string cmd, string keyword, int delayMs, int timeOutMs)
        {
            try
            {
                string res = string.Empty;
                bool result = false;

                SendCommand(portType, cmd, delayMs);

                //DisplayMsg(LogType.Log, "====Debuger2===");

                result = ChkResponse(portType, ITEM.NONE, keyword, out res, timeOutMs);

                if (result)
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                return false;
            }
        }

        private bool ChkResponse(PortType portType, ITEM item, string keyword, out string getMsg, int timeOutMs)
        {
            DateTime dt;
            TimeSpan ts;
            int count = 0;
            int delayMs = 50;
            string res = string.Empty;
            string log = string.Empty;

            getMsg = string.Empty;
            dt = DateTime.Now;
        Re:

            res = string.Empty;
            log = string.Empty;

            try
            {
                while (true)
                {
                    ts = new TimeSpan(DateTime.Now.Ticks - dt.Ticks);
                    if (ts.TotalMilliseconds > timeOutMs)
                    {
                        DisplayMsg(LogType.Error, "Check timeout");
                        return false;
                    }

                    System.Threading.Thread.Sleep(delayMs);

                    switch (portType)
                    {
                        case PortType.UART:
                            #region PortType.UART
                            if (!ChkPort(uart.PortName))
                            {
                                DisplayMsg(LogType.Error, "Not find port : " + uart.PortName);
                                MessageBox.Show("Not find port : " + uart.PortName);
                                return false;
                            }

                            if (!uart.IsOpen)
                            {
                                DisplayMsg(LogType.Log, "Open uart port");
                                uart.Open();
                            }

                            res = uart.ReadExisting();

                            if (res.Length != 0 && res != "\r\n")
                            {
                                DisplayMsg(LogType.Uart, res);
                                log += res;
                            }
                            break;
                        #endregion
                        case PortType.JS7K_GOLDEN:
                            #region PortType.JS7K_GOLDEN
                            if (!ChkPort(js7k.PortName))
                            {
                                DisplayMsg(LogType.Error, "Not find golden port : " + js7k.PortName);
                                MessageBox.Show("Not find golden port : " + js7k.PortName);
                                return false;
                            }

                            if (!js7k.IsOpen)
                            {
                                DisplayMsg(LogType.Log, "Open golden port");
                                js7k.Open();
                            }

                            res = js7k.ReadExisting();

                            if (res.Length != 0)
                            {
                                log += res;
                            }

                            if (res.Trim().Length != 0)
                            {
                                DisplayMsg(LogType.JS7K, res);
                            }
                            break;
                        #endregion
                        case PortType.GOLDEN_UART_6G_BT:
                            #region PortType.GOLDEN_UART_6G_BT
                            if (!ChkPort(golden6G_BT.PortName))
                            {
                                DisplayMsg(LogType.Error, "Not find golden port : " + golden6G_BT.PortName);
                                MessageBox.Show("Not find golden port : " + golden6G_BT.PortName);
                                return false;
                            }

                            if (!golden6G_BT.IsOpen)
                            {
                                DisplayMsg(LogType.Log, "Open golden port");
                                golden6G_BT.Open();
                            }

                            res = golden6G_BT.ReadExisting();

                            if (res.Length != 0)
                            {
                                log += res;
                            }

                            if (res.Trim().Length != 0)
                            {
                                DisplayMsg(LogType.GOLDEN_UART_6G_BT, res);
                            }
                            break;
                        #endregion
                        case PortType.GOLDEN_UART_2G_5G:
                            #region PortType.GOLDEN_UART_2G_5G
                            if (!ChkPort(golden2G_5G.PortName))
                            {
                                DisplayMsg(LogType.Error, "Not find golden port : " + golden2G_5G.PortName);
                                MessageBox.Show("Not find golden port : " + golden2G_5G.PortName);
                                return false;
                            }

                            if (!golden2G_5G.IsOpen)
                            {
                                DisplayMsg(LogType.Log, "Open golden port");
                                golden2G_5G.Open();
                            }

                            res = golden2G_5G.ReadExisting();

                            if (res.Length != 0)
                            {
                                log += res;
                            }

                            if (res.Trim().Length != 0)
                            {
                                DisplayMsg(LogType.GOLDEN_UART_2G_5G, res);
                            }
                            break;
                        #endregion
                        case PortType.TELNET:
                            #region PortType.TELNET
                            if (!telnet.IsConnect)
                            {
                                DisplayMsg(LogType.Error, "Telnet diconnect : " + telnet.IP);
                                //MessageBox.Show("Telnet diconnect : " + telnet.IP);
                                return false;
                            }
                            telnet.ChkResponse(keyword, ref log, timeOutMs);
                            break;
                        #endregion
                        case PortType.TELNET2:
                            #region PortType.TELNET2
                            if (!telnet2.IsConnect)
                            {
                                DisplayMsg(LogType.Error, "Telnet 2 diconnect : " + telnet2.IP);
                                //MessageBox.Show("Telnet diconnect : " + telnet.IP);
                                return false;
                            }
                            telnet2.ChkResponse(keyword, ref log, timeOutMs);
                            break;
                        #endregion
                        case PortType.GOLDEN_TELNET_2G_5G:
                            #region PortType.GOLDEN_TELNET_2G_5G
                            if (!golden2G5GTelnet.IsConnect)
                            {
                                DisplayMsg(LogType.Error, "Telnet diconnect : " + golden2G5GTelnet.IP);
                                //MessageBox.Show("Telnet diconnect : " + telnet.IP);
                                return false;
                            }
                            golden2G5GTelnet.ChkResponse(keyword, ref log, timeOutMs);
                            break;
                        #endregion
                        case PortType.GOLDEN_TELNET_6G_BT:
                            #region PortType.GOLDEN_TELNET_6G_BT
                            if (!golden6GTelnet.IsConnect)
                            {
                                DisplayMsg(LogType.Error, "Telnet diconnect : " + golden6GTelnet.IP);
                                //MessageBox.Show("Telnet diconnect : " + telnet.IP);
                                return false;
                            }
                            golden6GTelnet.ChkResponse(keyword, ref log, timeOutMs);
                            break;
                        #endregion
                        case PortType.DUT_UART:
                            #region PortType.DUT_UART
                            if (!ChkPort(atCmdUart.PortName))
                            {
                                DisplayMsg(LogType.Error, "Not find port : " + atCmdUart.PortName);
                                MessageBox.Show("Not find port : " + atCmdUart.PortName);
                                return false;
                            }

                            if (!atCmdUart.IsOpen)
                            {
                                DisplayMsg(LogType.Log, "Open modem port");
                                atCmdUart.Open();
                            }

                            res = atCmdUart.ReadExisting();

                            DisplayMsg(LogType.Log, res);

                            if (res.Length != 0)
                            {
                                log += res;
                            }
                            break;
                        #endregion

                        default:
                            break;
                    }

                    if (keyword.Length == 0)
                    {
                        return true;
                    }

                    if (log.Contains(keyword))
                    {
                        break;
                    }
                }

                switch (portType)
                {
                    case PortType.UART:
                        #region PortType.UART
                        break;
                    #endregion
                    case PortType.DUT_UART:
                        #region PortType.DUT_UART
                        if (atCmdUart.IsOpen)
                        {
                            DisplayMsg(LogType.Log, "Close modem port");
                            atCmdUart.Close();
                        }
                        break;
                    #endregion
                    default:
                        break;
                }

                if (log.Length != 0)
                {
                    getMsg = SortMsg(portType, item, log);
                }

                return true;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                if (count++ < 3)
                {
                    DisplayMsg(LogType.Retry, "Retry count : " + count.ToString());
                    GcErase(portType);
                    goto Re;
                }
                return false;
            }
        }

        private string SortMsg(PortType portType, ITEM item, string log)
        {
            try
            {
                string[] msg = null;
                string[] str = null;
                string cmd = string.Empty;
                string res = string.Empty;
                string antName = string.Empty;
                string bt_rssi = string.Empty;
                string data = string.Empty;
                int index = 0;

                cmd = GetCommand(portType, item, string.Empty);

                switch (portType)
                {
                    case PortType.UART:
                    case PortType.TELNET:
                        #region DUT
                        switch (item)
                        {
                            case ITEM.CHK_SERIALNUMBER:
                                #region ITEM.NORMAL
                                res = log.Replace("\r\n", "@");
                                msg = res.Split('@');
                                for (int i = 0; i < msg.Length; i++)
                                {
                                    if (msg[i].Contains(cmd))
                                    {
                                        if (msg[i + 1].Contains("["))
                                        {
                                            str = msg[i + 2].Split('=');
                                        }
                                        else
                                        {
                                            str = msg[i + 1].Split('=');
                                        }

                                        log = str[1];
                                        break;
                                    }
                                }
                                break;
                            #endregion
                            case ITEM.BT_RSSI:
                                #region ITEM.BT_RSSI
                                res = log.Replace("\r\n", "@");
                                str = res.Split('@');

                                for (int i = 0; i < str.Length; i++)
                                {
                                    if (str[i].Contains("Name (complete):"))
                                    {
                                        antName = str[i].Replace("Name (complete):", string.Empty).Replace("[0m", string.Empty).Replace(" ", string.Empty.Trim());
                                        index = str[i - 1].LastIndexOf("dBm");
                                        bt_rssi = str[i - 1].Substring(0, index).Replace("[0m", string.Empty).Replace(" ", string.Empty).Replace("RSSI:", string.Empty);
                                        //return antName + "," + bt_rssi;
                                        data = data + antName + "," + bt_rssi + ",";
                                    }
                                }
                                log = data;
                                break;
                            #endregion
                            case ITEM.WiFi_2G_RSSI:
                                #region ITEM.WiFi_RSSI
                                res = log.Replace(" ", string.Empty).Replace("\t", string.Empty).Trim();
                                str = res.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                                for (int i = 0; i < str.Length; i++)
                                {
                                    if (str[i].Contains("rssi_chain[0]"))
                                    {
                                        wifiData.AddData(0, str[i].Split(',')[0].Split('=')[1].Trim().Split(':')[1].Trim());
                                    }
                                    else if (str[i].Contains("rssi_chain[1]"))
                                    {
                                        wifiData.AddData(1, str[i].Split(',')[0].Split('=')[1].Trim().Split(':')[1].Trim());
                                    }
                                    else if (str[i].Contains("rssi_chain[2]"))
                                    {
                                        wifiData.AddData(2, str[i].Split(',')[0].Split('=')[1].Trim().Split(':')[1].Trim());
                                    }
                                    else if (str[i].Contains("rssi_chain[3]"))
                                    {
                                        wifiData.AddData(3, str[i].Split(',')[0].Split('=')[1].Trim().Split(':')[1].Trim());
                                    }
                                    else if (str[i].Contains("rssi_chain[4]"))
                                    {
                                        wifiData.AddData(3, str[i].Split(',')[0].Split('=')[1].Trim().Split(':')[1].Trim());
                                    }
                                    else if (str[i].Contains("rssi_chain[5]"))
                                    {
                                        wifiData.AddData(3, str[i].Split(',')[0].Split('=')[1].Trim().Split(':')[1].Trim());
                                    }
                                    else if (str[i].Contains("rssi_chain[6]"))
                                    {
                                        wifiData.AddData(3, str[i].Split(',')[0].Split('=')[1].Trim().Split(':')[1].Trim());
                                    }
                                    else if (str[i].Contains("rssi_chain[7]"))
                                    {
                                        wifiData.AddData(3, str[i].Split(',')[0].Split('=')[1].Trim().Split(':')[1].Trim());
                                    }
                                }
                                break;
                            #endregion
                            case ITEM.WiFi_5G_RSSI:
                                #region ITEM.WiFi_RSSI
                                res = log.Replace(" ", string.Empty).Replace("\t", string.Empty).Trim();
                                str = res.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                                for (int i = 0; i < str.Length; i++)
                                {
                                    if (str[i].Contains("rssi_chain[0]"))
                                    {
                                        wifiData.AddData(0, str[i].Split(',')[2].Split(':')[1].Trim());
                                    }
                                    else if (str[i].Contains("rssi_chain[1]"))
                                    {
                                        wifiData.AddData(1, str[i].Split(',')[2].Split(':')[1].Trim());
                                    }
                                    else if (str[i].Contains("rssi_chain[2]"))
                                    {
                                        wifiData.AddData(2, str[i].Split(',')[2].Split(':')[1].Trim());
                                    }
                                    else if (str[i].Contains("rssi_chain[3]"))
                                    {
                                        wifiData.AddData(3, str[i].Split(',')[2].Split(':')[1].Trim());
                                    }
                                    else if (str[i].Contains("rssi_chain[4]"))
                                    {
                                        wifiData.AddData(3, str[i].Split(',')[2].Split(':')[1].Trim());
                                    }
                                    else if (str[i].Contains("rssi_chain[5]"))
                                    {
                                        wifiData.AddData(3, str[i].Split(',')[2].Split(':')[1].Trim());
                                    }
                                    else if (str[i].Contains("rssi_chain[6]"))
                                    {
                                        wifiData.AddData(3, str[i].Split(',')[2].Split(':')[1].Trim());
                                    }
                                    else if (str[i].Contains("rssi_chain[7]"))
                                    {
                                        wifiData.AddData(3, str[i].Split(',')[2].Split(':')[1].Trim());
                                    }
                                }
                                break;
                            #endregion                          
                            case ITEM.NONE:
                                break;
                            default:
                                break;
                        }
                        break;
                    #endregion
                    case PortType.DUT_UART:
                        #region PortType.DUT_UART
                        switch (item)
                        {
                            default:
                                break;
                        }
                        break;
                    #endregion
                    case PortType.SSH:
                        #region DUT
                        switch (item)
                        {
                            case ITEM.CHK_SERIALNUMBER:
                                #region ITEM.NORMAL
                                res = log.Replace("\r\n", "@");
                                msg = res.Split('@');
                                for (int i = 0; i < msg.Length; i++)
                                {
                                    if (msg[i].Contains(cmd))
                                    {
                                        if (msg[i + 1].Contains("["))
                                        {
                                            str = msg[i + 2].Split('=');
                                        }
                                        else
                                        {
                                            str = msg[i + 1].Split('=');
                                        }

                                        log = str[1];
                                        break;
                                    }
                                }
                                break;
                            #endregion
                            case ITEM.BT_RSSI:
                                #region ITEM.BT_RSSI
                                res = log.Replace("\r\n", "@");
                                str = res.Split('@');

                                for (int i = 0; i < str.Length; i++)
                                {
                                    if (str[i].Contains("Name (complete):"))
                                    {
                                        antName = str[i].Replace("Name (complete):", string.Empty).Replace("[0m", string.Empty).Replace(" ", string.Empty.Trim());
                                        index = str[i - 1].LastIndexOf("dBm");
                                        bt_rssi = str[i - 1].Substring(0, index).Replace("[0m", string.Empty).Replace(" ", string.Empty).Replace("RSSI:", string.Empty);
                                        //return antName + "," + bt_rssi;
                                        data = data + antName + "," + bt_rssi + ",";
                                    }
                                }
                                log = data;
                                break;
                            #endregion
                            case ITEM.WiFi_2G_RSSI:
                                #region ITEM.WiFi_RSSI
                                res = log.Replace(" ", string.Empty).Replace("\t", string.Empty).Trim();
                                str = res.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                                for (int i = 0; i < str.Length; i++)
                                {
                                    if (str[i].Contains("rssi_chain[0]"))
                                    {
                                        wifiData.AddData(0, str[i].Split(',')[0].Split('=')[1].Trim().Split(':')[1].Trim());
                                    }
                                    else if (str[i].Contains("rssi_chain[1]"))
                                    {
                                        wifiData.AddData(1, str[i].Split(',')[0].Split('=')[1].Trim().Split(':')[1].Trim());
                                    }
                                    else if (str[i].Contains("rssi_chain[2]"))
                                    {
                                        wifiData.AddData(2, str[i].Split(',')[0].Split('=')[1].Trim().Split(':')[1].Trim());
                                    }
                                    else if (str[i].Contains("rssi_chain[3]"))
                                    {
                                        wifiData.AddData(3, str[i].Split(',')[0].Split('=')[1].Trim().Split(':')[1].Trim());
                                    }
                                }
                                break;
                            #endregion
                            case ITEM.WiFi_5G_RSSI:
                                #region ITEM.WiFi_RSSI
                                res = log.Replace(" ", string.Empty).Replace("\t", string.Empty).Trim();
                                str = res.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                                for (int i = 0; i < str.Length; i++)
                                {
                                    if (str[i].Contains("rssi_chain[0]"))
                                    {
                                        wifiData.AddData(0, str[i].Split(',')[2].Split(':')[1].Trim());
                                    }
                                    else if (str[i].Contains("rssi_chain[1]"))
                                    {
                                        wifiData.AddData(1, str[i].Split(',')[2].Split(':')[1].Trim());
                                    }
                                    else if (str[i].Contains("rssi_chain[2]"))
                                    {
                                        wifiData.AddData(2, str[i].Split(',')[2].Split(':')[1].Trim());
                                    }
                                    else if (str[i].Contains("rssi_chain[3]"))
                                    {
                                        wifiData.AddData(3, str[i].Split(',')[2].Split(':')[1].Trim());
                                    }
                                }
                                break;
                            #endregion
                            case ITEM.WiFi_6G_RSSI:
                                #region ITEM.WiFi_RSSI
                                res = log.Replace(" ", string.Empty).Replace("\t", string.Empty).Trim();
                                str = res.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                                for (int i = 0; i < str.Length; i++)
                                {
                                    if (str[i].Contains("rssi_chain[0]"))
                                    {
                                        wifiData.AddData(0, str[i].Split(',')[3].Split(':')[1].Trim());
                                    }
                                    else if (str[i].Contains("rssi_chain[1]"))
                                    {
                                        wifiData.AddData(1, str[i].Split(',')[3].Split(':')[1].Trim());
                                    }
                                    else if (str[i].Contains("rssi_chain[2]"))
                                    {
                                        wifiData.AddData(2, str[i].Split(',')[3].Split(':')[1].Trim());
                                    }
                                    else if (str[i].Contains("rssi_chain[3]"))
                                    {
                                        wifiData.AddData(3, str[i].Split(',')[3].Split(':')[1].Trim());
                                    }
                                }
                                break;
                            #endregion
                            case ITEM.NONE:
                                break;
                            default:
                                break;
                        }
                        break;
                    #endregion
                    default:
                        break;
                }


                return log;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                return log;
            }
        }

        private void GcErase(PortType portType)
        {
            int count = 0;

        Re:
            try
            {
                DisplayMsg(LogType.Log, "Erase " + portType.ToString() + " Port..");
                switch (portType)
                {
                    case PortType.DUT_UART:
                        #region PortType.DUT_UART
                        try
                        {
                            atCmdUart.Dispose();
                            atCmdUart = null;
                        }
                        catch (Exception e)
                        {
                            DisplayMsg(LogType.Exception, e.Message);
                        }
                        break;
                    #endregion
                    case PortType.UART:
                        #region PortType.UART
                        try
                        {
                            uart.Dispose();
                            uart = null;
                        }
                        catch (Exception e)
                        {
                            DisplayMsg(LogType.Exception, e.Message);
                        }
                        break;
                    #endregion
                    case PortType.GOLDEN_UART_6G_BT:
                        #region PortType.GOLDEN_UART_1
                        try
                        {
                            golden6G_BT.Dispose();
                            golden6G_BT = null;
                        }
                        catch (Exception e)
                        {
                            DisplayMsg(LogType.Exception, e.Message);
                        }
                        break;
                    #endregion
                    case PortType.GOLDEN_UART_2G_5G:
                        #region PortType.GOLDEN_UART_2
                        try
                        {
                            golden2G_5G.Dispose();
                            golden2G_5G = null;
                        }
                        catch (Exception e)
                        {
                            DisplayMsg(LogType.Exception, e.Message);
                        }
                        break;
                    #endregion       
                    case PortType.TELNET:
                        #region PortType.TELNET
                        telnet.Dispose();
                        if (ChkLinux(portType, 60 * 1000))
                        {
                            return;
                        }
                        else
                        {
                            warning = "TELNET";
                            return;
                        }
                    #endregion
                    case PortType.TELNET2:
                        #region PortType.TELNET2
                        telnet2.Dispose();
                        if (ChkLinux(portType, 60 * 1000))
                        {
                            return;
                        }
                        else
                        {
                            warning = "TELNET";
                            return;
                        }
                    #endregion
                    default:
                        break;
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
                DisplayMsg(LogType.Log, "Wait 2000 (ms)..");
                Thread.Sleep(2000);

                DisplayMsg(LogType.Log, "Create " + portType.ToString() + " Port..");

                switch (portType)
                {
                    case PortType.DUT_UART:
                        #region PortType.DUT_UART
                        if (atCmdUart == null)
                        {
                            atCmdUart = new SerialPort();
                            atCmdUart.PortName = Func.ReadINI("Setting", "Port", "ATCommandPort", "COM0");
                            atCmdUart.BaudRate = 115200;
                            atCmdUart.StopBits = StopBits.One;
                            atCmdUart.Parity = Parity.None;
                            atCmdUart.DataBits = 8;
                            atCmdUart.RtsEnable = false;
                        }
                        break;
                    #endregion
                    case PortType.UART:
                        #region PortType.UART
                        if (uart == null)
                        {
                            uart = new System.IO.Ports.SerialPort();
                            uart.PortName = Func.ReadINI("Setting", "Port", "UartPort", "COM0");
                            uart.BaudRate = 115200;
                            uart.StopBits = StopBits.One;
                            uart.Parity = Parity.None;
                            uart.DataBits = 8;
                            uart.RtsEnable = false;
                        }
                        break;
                    #endregion
                    case PortType.GOLDEN_UART_6G_BT:
                        #region PortType.GOLDEN_UART_1
                        if (golden6G_BT == null)
                        {
                            golden6G_BT = new System.IO.Ports.SerialPort();
                            golden6G_BT.PortName = Func.ReadINI("Setting", "Port", "WiFi_6G_BT_Golden", "COM0");
                            golden6G_BT.BaudRate = 115200;
                            golden6G_BT.StopBits = StopBits.One;
                            golden6G_BT.Parity = Parity.None;
                            golden6G_BT.DataBits = 8;
                            golden6G_BT.RtsEnable = false;
                        }
                        break;
                    #endregion
                    case PortType.GOLDEN_UART_2G_5G:
                        #region PortType.GOLDEN_UART_2
                        if (golden2G_5G == null)
                        {
                            golden2G_5G = new System.IO.Ports.SerialPort();
                            golden2G_5G.PortName = Func.ReadINI("Setting", "Port", "WiFi_2G_5G_Golden", "COM0");
                            golden2G_5G.BaudRate = 115200;
                            golden2G_5G.StopBits = StopBits.One;
                            golden2G_5G.Parity = Parity.None;
                            golden2G_5G.DataBits = 8;
                            golden2G_5G.RtsEnable = false;
                        }
                        break;
                    #endregion
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                if (count++ < 3)
                {
                    DisplayMsg(LogType.Retry, "Retry count : " + count.ToString());
                    goto Re;
                }
            }
        }

        private bool ChkPort(PortType portType, int timeOutMs = 3000)
        {
            switch (portType)
            {
                case PortType.DUT_UART:
                    if (!ChkPort(portType, timeOutMs))
                    {
                        portExist = false;
                        status_ATS.AddDataLog("PORT", NG);
                    }
                    else
                    {
                        portExist = true;
                        status_ATS.AddDataLog("PORT", PASS);
                        return true;
                    }
                    break;
                case PortType.UART:
                    if (!ChkPort(portType, timeOutMs))
                    {
                        status_ATS.AddDataLog("PORT", NG);
                    }
                    else
                    {
                        status_ATS.AddDataLog("PORT", PASS);
                        return true;
                    }
                    break;
                case PortType.DIAG:
                    if (!ChkPort(portType, timeOutMs))
                    {
                        portExist = false;
                        status_ATS.AddDataLog("PORT", NG);
                    }
                    else
                    {
                        portExist = true;
                        status_ATS.AddDataLog("PORT", PASS);
                        return true;
                    }
                    break;
                default:
                    break;
            }
            return false;
        }

        private bool ChkPort(string portName)
        {
            string[] portList = System.IO.Ports.SerialPort.GetPortNames();
            foreach (string port in portList)
            {
                if (port.ToUpper() == portName.ToUpper())
                {
                    return true;
                }
            }
            return false;
        }

        private bool SwitchMode(PortType portType, Mode mode)
        {
            if (!portExist)
            {
                DisplayMsg(LogType.Warning, "Cant find Port : " + diagPort);
                return false;
            }

            Mode getMode = Mode.Unknow;

            switch (portType)
            {
                case PortType.DUT_UART:
                    AtSwitchMode(mode, ref getMode);
                    break;
                case PortType.DIAG:
                    break;
                case PortType.UART:
                    break;
                default:
                    break;
            }

            if ((int)getMode != (int)mode)
            {
                DisplayMsg(LogType.Log, "Switch " + mode.ToString() + " mode fail");
                //status_ATS.AddDataLog("MODE", mode.ToString().ToUpper() + "_" + NG);
                return false;
            }
            return true;
        }

        private bool AtSwitchMode(Mode mode, ref Mode getMode)
        {
            int count = 0;
            int timeOutMs = 5 * 1000;
            string cmd = "at+cfun";
            string res = string.Empty;
            string msg = string.Empty;

            DateTime dt;
            TimeSpan ts;

        Re:
            try
            {
                getMode = Mode.Unknow;
                res = string.Empty;

                if (!ChkPort(PortType.DUT_UART, 20 * 1000))
                {
                    return false;
                }

                if (!atCmdUart.IsOpen)
                {
                    DisplayMsg(LogType.Log, "Open modem port");
                    atCmdUart.Open();
                }

                if (String.Compare(mode.ToString(), Mode.Ask.ToString(), true) == 0)
                {
                    DisplayMsg(LogType.Log, "Transmit ' " + cmd + "?" + " '");
                    atCmdUart.WriteLine(cmd + "?" + "\r\n");

                    dt = DateTime.Now;
                    while (true)
                    {
                        System.Threading.Thread.Sleep(100);

                        ts = new TimeSpan(DateTime.Now.Ticks - dt.Ticks);
                        if (ts.TotalMilliseconds >= timeOutMs)
                        {
                            DisplayMsg(LogType.Log, res);
                            DisplayMsg(LogType.Error, "Check timeout");
                            return false;
                        }

                        res = res + atCmdUart.ReadExisting();

                        if (res.Contains("+CFUN: "))
                        {
                            DisplayMsg(LogType.Log, res);

                            if (res.Contains(((int)Mode.OnLine).ToString()))
                            {
                                getMode = Mode.OnLine;
                            }
                            else if (res.Contains(((int)Mode.OffLine).ToString()))
                            {
                                getMode = Mode.OffLine;
                            }
                            else if (res.Contains(((int)Mode.Ftm).ToString()))
                            {
                                getMode = Mode.Ftm;
                            }
                            else if (res.Contains(((int)Mode.LowPower).ToString()))
                            {
                                getMode = Mode.LowPower;
                            }
                            else
                            {
                                getMode = Mode.Unknow;
                            }
                            break;
                        }
                    }
                }
                else
                {
                    DisplayMsg(LogType.Log, "Transmit ' " + cmd + "=" + ((int)mode).ToString() + " '");
                    atCmdUart.WriteLine(cmd + "=" + ((int)mode).ToString() + "\r\n");

                    dt = DateTime.Now;
                    while (true)
                    {
                        System.Threading.Thread.Sleep(100);

                        ts = new TimeSpan(DateTime.Now.Ticks - dt.Ticks);
                        if (ts.TotalMilliseconds >= timeOutMs)
                        {
                            DisplayMsg(LogType.Log, res);
                            DisplayMsg(LogType.Error, "Check timeout");
                            return false;
                        }

                        res = res + atCmdUart.ReadExisting();

                        if (res.Contains("OK"))
                        {
                            DisplayMsg(LogType.Log, res);
                            break;
                        }

                    }

                    dt = DateTime.Now;
                    while (true)
                    {
                        ts = new TimeSpan(DateTime.Now.Ticks - dt.Ticks);
                        if (ts.TotalMilliseconds >= timeOutMs)
                        {
                            DisplayMsg(LogType.Error, "Mode : " + getMode.ToString());
                            DisplayMsg(LogType.Error, "Check mode TimeOut");
                            return false;
                        }

                        AtSwitchMode(Mode.Ask, ref getMode);

                        if ((int)getMode == (int)mode)
                        {
                            DisplayMsg(LogType.Log, "Switch " + getMode.ToString() + " mode success");
                            return true;
                        }

                        System.Threading.Thread.Sleep(100);
                    }
                }

                if (atCmdUart.IsOpen)
                {
                    DisplayMsg(LogType.Log, "Close modem port");
                    atCmdUart.Close();
                }

                return true;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                getMode = Mode.Unknow;
                if (count++ < 3)
                {
                    GcErase(PortType.DUT_UART);
                    DisplayMsg(LogType.Retry, "Retry count : " + count.ToString());
                    goto Re;
                }
                return false;
            }
        }
        private bool ChkLinux(PortType port, string keyword1, string keyword2, int timeOutMs)//, ref bool detectUBoot)
        {
            DateTime dt;
            TimeSpan ts;
            int count = 0;
            string res = string.Empty;
            string log = string.Empty;
            //bool result = false;

            dt = DateTime.Now;
        Re:

            res = string.Empty;
            log = string.Empty;
            try
            {
                while (true)
                {
                    ts = new TimeSpan(DateTime.Now.Ticks - dt.Ticks);

                    if (ts.TotalMilliseconds > timeOutMs)
                    {
                        DisplayMsg(LogType.Error, "timeOut");
                        return false;
                    }

                    System.Threading.Thread.Sleep(50);

                    switch (port)
                    {
                        case PortType.UART:
                            #region PortType.UART
                            if (!ChkPort(uart.PortName))
                            {
                                DisplayMsg(LogType.Error, "Not find port : " + uart.PortName);
                                MessageBox.Show("Not find port : " + uart.PortName);
                                return false;
                            }

                            if (!uart.IsOpen)
                            {
                                DisplayMsg(LogType.Log, "Open uart port");
                                uart.Open();
                            }

                            res = uart.ReadExisting();

                            if (res.Length != 0 && res != "\r\n")
                            {
                                DisplayMsg(LogType.Uart, res);
                                log += res;
                                SendCommand(PortType.UART, "\r\n", 200);
                                //detectUBoot = true;
                                // Greg Detect U-Boot Version
                                //if (res.Contains("U-Boot 2016.01"))
                                //{
                                //    detectUBoot = true;
                                //}
                            }
                            break;
                        #endregion
                        default:
                            break;
                    }

                    if (log.Contains(keyword1))// && !result)
                    {
                        SendAndChk(PortType.UART, "\r\n", keyword2, out res, 0, 3000);
                        System.Threading.Thread.Sleep(100);
                        if (res.Contains(keyword2))
                        {
                            //result = true;
                            return true;
                        }
                    }
                    SendAndChk(PortType.UART, "\r\n", keyword2, out res, 0, 3000);
                    if (res.Contains(keyword2)) //openwrt@
                    {
                        return true;
                    }
                }

            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                if (count++ < 3)
                {
                    DisplayMsg(LogType.Retry, "Retry count : " + count.ToString());
                    GcErase(port);
                    goto Re;
                }
                return false;
            }
        }
        private bool ChkLinux(bool retry, string keyword1 = "system ready", string keyword2 = "root@OpenWrt")
        {
            if (!CheckGoNoGo())
            {
                return false;
            }

            try
            {

                int timeOutMs = Int32.Parse(Func.ReadINI("Setting", "PCBA", "MoCaTimeOut", "120")) * 1000;
                if (retry)
                {
                    timeOutMs = 20000;
                }
                bool result = false;
                bool detectUBoot = false;

                result = ChkLinux(PortType.UART, keyword1, keyword2, timeOutMs, ref detectUBoot);

                if (result)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                //status_ATS.AddDataLog("ChkUBoot", NG);
                AddData("Linux", 1);
                return false;
            }
        }
        private bool ChkLinux(PortType port, int timeOutMs)
        {
            if (!CheckGoNoGo())
            {
                return false;
            }

            try
            {
                DisplayMsg(LogType.Log, "================= Linux Initial =================");

                bool result = false;
                string keyword = "root@OpenWrt:";

                result = ChkInitial(port, keyword, timeOutMs);

                if (!result)
                {
                    AddData("Linux", 1);
                }
                //else
                //{
                //    AddData("Linux", 0);
                //}
                return result;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData("Linux", 1);
                return false;
            }
        }
        private bool ChkLinux(PortType port, string keyword1, string keyword2, int timeOutMs, ref bool detectUBoot)
        {
            DateTime dt;
            TimeSpan ts;
            int count = 0;
            string res = string.Empty;
            string log = string.Empty;
            bool result = false;

            dt = DateTime.Now;
        Re:

            res = string.Empty;
            log = string.Empty;
            try
            {
                while (true)
                {
                    ts = new TimeSpan(DateTime.Now.Ticks - dt.Ticks);

                    if (ts.TotalMilliseconds > timeOutMs)
                    {
                        DisplayMsg(LogType.Error, "timeOut");
                        return false;
                    }

                    System.Threading.Thread.Sleep(50);

                    switch (port)
                    {
                        case PortType.UART:
                            #region PortType.UART
                            if (!ChkPort(uart.PortName))
                            {
                                DisplayMsg(LogType.Error, "Not find port : " + uart.PortName);
                                MessageBox.Show("Not find port : " + uart.PortName);
                                return false;
                            }

                            if (!uart.IsOpen)
                            {
                                DisplayMsg(LogType.Log, "Open uart port");
                                uart.Open();
                            }

                            res = uart.ReadExisting();

                            if (res.Length != 0 && res != "\r\n")
                            {
                                DisplayMsg(LogType.Uart, res);
                                log += res;
                                //detectUBoot = true;
                                // Greg Detect U-Boot Version
                                if (res.Contains("U-Boot 2016.01"))
                                {
                                    detectUBoot = true;
                                }
                            }
                            break;
                        #endregion
                        default:
                            break;
                    }

                    if (log.Contains(keyword1) && !result)
                    {
                        uart.Write("\r\n");

                        System.Threading.Thread.Sleep(100);
                        if (log.Contains(keyword2))
                        {
                            result = true;
                            return true;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                if (count++ < 3)
                {
                    DisplayMsg(LogType.Retry, "Retry count : " + count.ToString());
                    GcErase(port);
                    goto Re;
                }
                return false;
            }
        }
        private bool ScanBarCode()
        {
            if (String.Compare(Func.ReadINI("Setting", "BarCodeScanner", "BarCodeScanner", "Disable").ToUpper(), CHK.Enable.ToString().ToUpper(), true) != 0)
            {
                DisplayMsg(LogType.Log, "Scan barcode manually.");
                return true;
            }

            try
            {
                byte[] initCmd = new byte[] { 0x16, 0x4D, 0x0D, 0x30, 0x34, 0x30, 0x31, 0x44, 0x30, 0x35, 0x2E };
                byte[] data = new byte[3];

                string res = string.Empty;
                string scanCmd = "16540d";
                string closeCmd = "16550d";
                bool result = false;

                string psn_keyword = Func.ReadINI("Setting", "PSN", "Keyword", "");
                int psn_length = Convert.ToInt32(Func.ReadINI("Setting", "PSN", "Length", "8"));
                int timeOutMs = 10 * 1000;
                int count = 0;

                DateTime dt;
                TimeSpan ts;

            Re:
                if (!barcodeUart.IsOpen)
                {
                    DisplayMsg(LogType.Log, "Open barcode uart");
                    barcodeUart.Open();
                }

                barcodeUart.Write(initCmd, 0, initCmd.Length);

                System.Threading.Thread.Sleep(200);

                res = barcodeUart.ReadExisting();

                DisplayMsg(LogType.Scanner, "Initial : " + res);

                #region Scan
                for (int i = 0; i < 6; i = i + 2)
                {
                    data[i / 2] = Convert.ToByte(scanCmd.Substring(i, 2), 16);
                }

                DisplayMsg(LogType.Scanner, "cmd : " + scanCmd);

                barcodeUart.Write(data, 0, 3);

                dt = DateTime.Now;
                res = string.Empty;

                while (true)
                {
                    ts = new TimeSpan(DateTime.Now.Ticks - dt.Ticks);
                    if (ts.TotalMilliseconds > timeOutMs)
                    {
                        DisplayMsg(LogType.Error, "Check timeout");
                        break;
                    }

                    System.Threading.Thread.Sleep(200);
                    res += barcodeUart.ReadExisting();
                    DisplayMsg(LogType.Scanner, "Receive : " + res);
                    res = res.Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace(" ", "").Trim();

                    if (res.Contains(psn_keyword) &&
                        res.Length == psn_length)
                    {
                        SetTextBox(status_ATS.txtPSN, res);
                        status_ATS.SFCS_Data.PSN = res;
                        status_ATS.SFCS_Data.First_Line = res;
                        result = true;
                        break;
                    }
                }
                #endregion

                #region Close
                data = new byte[3];

                for (int i = 0; i < 6; i = i + 2)
                {
                    data[i / 2] = Convert.ToByte(closeCmd.Substring(i, 2), 16);
                }

                DisplayMsg(LogType.Scanner, "cmd : " + closeCmd);

                barcodeUart.Write(data, 0, 3);
                barcodeUart.ReadExisting();

                if (barcodeUart.IsOpen)
                {
                    DisplayMsg(LogType.Log, "Close barcode uart");
                    barcodeUart.DiscardInBuffer();
                    barcodeUart.DiscardOutBuffer();
                    barcodeUart.Dispose();
                    barcodeUart.Close();
                }

                #endregion

                if (result)
                {
                    return true;
                }

                if (count++ < 3)
                {
                    DisplayMsg(LogType.Retry, "Retry count : " + count.ToString());
                    goto Re;
                }
                return false;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                return false;
            }
        }
        private bool ScanBarCode2()
        {
            if (String.Compare(Func.ReadINI("Setting", "BarCodeScanner2", "BarCodeScanner2", "Disable").ToUpper(), CHK.Enable.ToString().ToUpper(), true) != 0)
            {
                DisplayMsg(LogType.Log, "Scan barcode manually.");
                return true;
            }

            try
            {
                byte[] initCmd = new byte[] { 0x16, 0x4D, 0x0D, 0x30, 0x34, 0x30, 0x31, 0x44, 0x30, 0x35, 0x2E };
                byte[] data = new byte[3];

                string res = string.Empty;
                string scanCmd = "16540d";
                string closeCmd = "16550d";
                bool result = false;
                int timeOutMs = 10 * 1000;
                int count = 0;

                DateTime dt;
                TimeSpan ts;

            Re:
                if (!barcodeUart2.IsOpen)
                {
                    DisplayMsg(LogType.Log, "Open barcode uart");
                    barcodeUart2.Open();
                }

                barcodeUart2.Write(initCmd, 0, initCmd.Length);

                System.Threading.Thread.Sleep(200);

                res = barcodeUart2.ReadExisting();

                DisplayMsg(LogType.Scanner, "Initial : " + res);

                #region Scan
                for (int i = 0; i < 6; i = i + 2)
                {
                    data[i / 2] = Convert.ToByte(scanCmd.Substring(i, 2), 16);
                }

                DisplayMsg(LogType.Scanner, "cmd : " + scanCmd);

                barcodeUart2.Write(data, 0, 3);

                dt = DateTime.Now;
                res = string.Empty;

                while (true)
                {
                    ts = new TimeSpan(DateTime.Now.Ticks - dt.Ticks);
                    if (ts.TotalMilliseconds > timeOutMs)
                    {
                        DisplayMsg(LogType.Error, "Check timeout");
                        break;
                    }

                    System.Threading.Thread.Sleep(200);
                    res += barcodeUart2.ReadExisting();
                    DisplayMsg(LogType.Scanner, "Receive : " + res);
                    res = res.Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace(" ", "").Trim();
                    if (res.Length > 0)
                    {
                        SetTextBox(status_ATS.txtSP, res);
                        result = true;
                        break;
                    }
                }
                #endregion

                #region Close
                data = new byte[3];

                for (int i = 0; i < 6; i = i + 2)
                {
                    data[i / 2] = Convert.ToByte(closeCmd.Substring(i, 2), 16);
                }

                DisplayMsg(LogType.Scanner, "cmd : " + closeCmd);

                barcodeUart2.Write(data, 0, 3);
                barcodeUart2.ReadExisting();

                if (barcodeUart2.IsOpen)
                {
                    DisplayMsg(LogType.Log, "Close barcode uart");
                    barcodeUart2.DiscardInBuffer();
                    barcodeUart2.DiscardOutBuffer();
                    barcodeUart2.Dispose();
                    barcodeUart2.Close();
                }

                #endregion

                if (result)
                {
                    return true;
                }

                if (count++ < 3)
                {
                    DisplayMsg(LogType.Retry, "Retry count : " + count.ToString());
                    goto Re;
                }
                return false;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                return false;
            }
        }
        private bool ExcuteCurlCommand(string cmd, int delayMs, out string getMsg)
        {
            getMsg = string.Empty;

            try
            {
                Process process = new Process();
                process.StartInfo.FileName = "curl";
                process.StartInfo.Arguments = cmd;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;

                DisplayMsg(LogType.Log, "Transmitted 'curl " + cmd + "' to device");

                process.Start();

                if (delayMs > 0)
                {
                    DisplayMsg(LogType.Log, "Delay " + delayMs.ToString() + " (ms)..");
                    System.Threading.Thread.Sleep(delayMs);
                }

                getMsg = process.StandardOutput.ReadToEnd();

                DisplayMsg(LogType.Curl, getMsg);

                process.Close();
                process.Dispose();

                return true;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                return false;
            }
        }
        private bool ExecuteCurlCommand2(string cmd, int delayMs, out string getMsg)
        {
            getMsg = string.Empty;

            try
            {
                diagPort = "";
                Process process = new Process();
                process.StartInfo.FileName = "curl";
                process.StartInfo.Arguments = cmd;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                //* Set your output and error (asynchronous) handlers
                process.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
                process.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);
                //* Start process and handlers
                DisplayMsg(LogType.Log, "Transmitted 'curl " + cmd + "' to device");
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                process.Close();
                process.Dispose();

                return true;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                return false;
            }
        }

        private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            diagPort += outLine.Data;
            DisplayMsg(LogType.Curl, outLine.Data);
        }
        private bool SendCmdAndGetResp(CommandConsole myCC, string cmd, string keyword1, out string log, int timeoutInMS = 15000, int delay = 500, bool debug = false)
        {
            string res = string.Empty;
            log = string.Empty;

            try
            {
                myCC.ClearBuffer();
                status_ATS.AddLog("[Send command to dos]" + cmd);
                myCC.WriteLine(cmd);
                status_ATS.AddLog("[Delay]" + delay + "ms");
                Thread.Sleep(delay);
                //EasyLibrary.ezTimer timer = new EasyLibrary.ezTimer();
                //timer.Restart();
                DateTime dt = DateTime.Now;
                while (true)
                {
                    res = myCC.GetBufferAndClear();
                    if (res != string.Empty)
                    {
                        if (debug)
                            status_ATS.AddLog("Debug:" + res);
                        else
                            status_ATS.AddLog(res);
                        log += res;
                    }

                    if (string.IsNullOrEmpty(keyword1) || log.Contains(keyword1))
                    //||(!string.IsNullOrEmpty(keyword2) && log.Contains(keyword2) && log.LastIndexOf(keyword2) > 0))
                    {
                        DisplayMsg(LogType.Log, $"Check keyword '{keyword1}' ok");
                        return true;
                    }

                    if (dt.AddMilliseconds(timeoutInMS) < DateTime.Now)
                    {
                        DisplayMsg(LogType.Log, $"Wait keyword '{keyword1}' timeout");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                return false;
            }
        }

        private bool SendCmd(CommandConsole myCC, string cmd, int delay = 0)
        {
            try
            {
                myCC.ClearBuffer();
                status_ATS.AddLog("[Send command to dos]" + cmd);
                myCC.WriteLine(cmd);
                status_ATS.AddLog("[Delay]" + delay + "ms");
                Thread.Sleep(delay);
                return true;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                return false;
            }
        }
        private bool GetResp(CommandConsole myCC, PortType portType, ITEM item, string keyword1, out string getMsg, int timeoutInMS = 5000)
        {
            string res = string.Empty;
            string log = string.Empty;
            getMsg = string.Empty;

            try
            {

                DateTime dt = DateTime.Now;
                while (true)
                {
                    res = myCC.GetBufferAndClear();
                    if (res != string.Empty)
                    {
                        status_ATS.AddLog("GetResp Debug:" + res);
                        log += res;
                    }

                    if (string.IsNullOrEmpty(keyword1) || log.Contains(keyword1))
                    //||(!string.IsNullOrEmpty(keyword2) && log.Contains(keyword2) && log.LastIndexOf(keyword2) > 0))
                    {
                        DisplayMsg(LogType.Log, $"Check keyword '{keyword1}' ok");
                        break;
                    }

                    if (dt.AddMilliseconds(timeoutInMS) < DateTime.Now)
                    {
                        DisplayMsg(LogType.Log, $"Wait keyword '{keyword1}' timeout");
                        return false;
                    }
                }

                getMsg = SortMsg(portType, item, log);
                return true;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                return false;
            }
        }
    }
}