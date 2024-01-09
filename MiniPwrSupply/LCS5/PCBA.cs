//using ATS_Template.CSVs;
using EasyLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Services.Description;
using System.Windows.Forms;
//using WNC.API;
//using static WNC.UI.FrmRetry;

namespace MiniPwrSupply.LCS5
{
    public partial class frmMain
    {
        private Action<string, UInt32> mLogCallback = null;
        private Action<string, bool> mRunResultCallback = null;
        private Action<string> mUICallback = null;

        //public bool mSendCmdErrorMustResendDTM = false;
        private LCS5CsvRFTestItem csvItem = null;
        private string mComport = "";
        private int iRetryTime = 10;
        public int iItemCount = 0;
        public string code = "";
        public static frmMain SELF = null;
        // ===========================================
        // ============== VN manuf ===================
        SFCS_Query _Sfcs_Query = new SFCS_Query();
        // ===========================================


        private Action<Action, string> CalcExecutionSpendTime = (doneCallback, title) =>
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Reset();
            sw.Start();
            try
            {
                doneCallback();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                sw.Stop();
                string timeSpend = sw.Elapsed.TotalMilliseconds.ToString();
                SELF.DisplayMsg(LogType.Log, "----- Spend : " + Convert.ToString((Convert.ToDouble(timeSpend) / 1000)) + @" Sec. -----" + Environment.NewLine);
            }
        };
        public void SetLogCallback(Action<string, uint> logAction)
        {
            this.mLogCallback = logAction;
        }


        public class DeviceInfor
        {
            public string SerialNumber = "";
            public string BaseMAC = "";
            public string Eth0MAC = "";
            public string Eth1MAC = "";
            public string Eth2GMAC = "";
            public string Eth5GMAC = "";
            public string FSAN = "";
            public string PartNumber = "";        // NO partNumber in testplan
            public string NoLevel300 = string.Empty;
            public string PartNumber_100 = "";
            public string GPON = "";
            public string FWver = "";
            public string HWver = "";               //TestPlan0814 added
            public string ModuleId = "";          // NO partNumber in testplan
            public string CalixFWver = "";
            public string IPADDR = "192.168.1.1";
            public string ServerIP = "192.168.1.100";
            public string MFGDate = DateTime.Now.ToString("MM/dd/yyyy");
            public string Key = "";
            public string Md5sum = string.Empty;

            public void ResetParam()
            {
                SerialNumber = "";
                BaseMAC = "";
                Eth1MAC = "";
                Eth2GMAC = "";
                Eth5GMAC = "";
                FSAN = "";
                PartNumber = "";
                PartNumber_100 = "";
                GPON = "";
            }
        }
        private void PCBA()
        {
            if (!CheckGoNoGo())
            {
                return;
            }
            string title = "======= PCBA =======";
            string res = string.Empty;
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Reset();
            sw.Start();
            //csvItem.Date_Time = DateTime.UtcNow.AddHours(8).ToString(@"yyyy/MM/dd HH:mm:ss");
            //SELF.CalcExecutionSpendTime(() => { }, title);
            sw.Stop();
            //csvItem.END_Time = DateTime.UtcNow.AddHours(8).ToString(@"yyyy/MM/dd HH:mm:ss");
            try
            {
                DeviceInfor infor = new DeviceInfor();

                int SN_Length = Convert.ToInt32(Func.ReadINI("Setting", "PSN", "Length", "-1"));

                #region create SMT file

                if (Func.ReadINI("Setting", "Golden", "GoldenSN", "").Contains(status_ATS.txtPSN.Text))
                {
                    isGolden = true;
                    DisplayMsg(LogType.Log, "Golden testing");
                }
                else
                {
                    isGolden = false;
                }

                if (status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode)
                {
                    SentPsnForGetMAC(status_ATS.txtPSN.Text.Trim());

                    for (int i = 0; i < 3; i++)
                    {
                        DisplayMsg(LogType.Log, "Delay 1000ms...");
                        Thread.Sleep(1000);

                        infor.SerialNumber = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@LCS5_SN");

                        infor.BaseMAC = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@MAC");

                        infor.FSAN = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@LCS5_SSID_SN_FSAN");

                        string name = Func.ReadINI("Setting", "SFCS", "Calix_Name", "@LCS5_CLX_FW_VER_21");
                        infor.CalixFWver = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, name);

                        infor.PartNumber = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@LCS5_300_PN").Substring(0, 10);

                        infor.PartNumber_100 = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@LCS5_100_PN").Substring(0, 10);

                        infor.MFGDate = DateTime.Now.ToString("MM/dd/yyyy");

                        infor.GPON = "0";

                        infor.ModuleId = "0";

                        infor.FWver = Func.ReadINI("Setting", "Final", "FWver", "!@#$%");
                        string FWname = Func.ReadINI("Setting", "SFCS", "MFGFW_Name", "@LCS5_MFG_FW_VER_18");
                        GetFromSfcs(FWname, out infor.FWver);
                        infor.FWver = infor.FWver.Substring(0, infor.FWver.Length - 10);

                        if (infor.SerialNumber != "")
                            break;
                    }

                    DisplayMsg(LogType.Log, "Get SN From SFCS is:" + infor.SerialNumber);
                    DisplayMsg(LogType.Log, "Get Base MAC From SFCS is:" + infor.BaseMAC);
                    DisplayMsg(LogType.Log, "Get FSAN From SFCS is:" + infor.FSAN);
                    DisplayMsg(LogType.Log, "Get Calix FW From SFCS is:" + infor.CalixFWver);
                    DisplayMsg(LogType.Log, "Get PartNumber From SFCS is:" + infor.PartNumber);
                    DisplayMsg(LogType.Log, "Get PartNumber_100 From SFCS is:" + infor.PartNumber_100);
                    DisplayMsg(LogType.Log, "MFG date is:" + infor.MFGDate);
                    DisplayMsg(LogType.Log, "GPON pw is:" + infor.GPON);
                    DisplayMsg(LogType.Log, "FWver is:" + infor.FWver);

                    if (infor.SerialNumber.Length == 12)
                    {
                        SetTextBox(status_ATS.txtPSN, infor.SerialNumber);
                        SetTextBox(status_ATS.txtSP, infor.BaseMAC);
                        status_ATS.SFCS_Data.PSN = infor.SerialNumber;
                        status_ATS.SFCS_Data.First_Line = infor.SerialNumber;
                    }
                    else
                    {
                        warning = "Get SN from SFCS fail";
                        return;
                    }
                }
                else  // THIEM added for NPI test
                {
                    infor.SerialNumber = "630301000027";
                    infor.PartNumber = "3000301302";
                    infor.PartNumber_100 = "1000590701";
                    infor.BaseMAC = "001122334400";
                    infor.Eth1MAC = "00:11:22:33:44:00";
                    infor.Eth2GMAC = "00:11:22:33:44:02";
                    infor.Eth5GMAC = "00:11:22:33:44:03";
                    infor.FSAN = "CXNK00DBB4C2";
                    infor.MFGDate = "08/09/2023";
                    infor.CalixFWver = "23.4.905.29";
                    infor.ModuleId = "0";
                    infor.HWver = "02";
                    infor.GPON = "0";
                }

                if (!ChkStation(status_ATS.txtPSN.Text))
                {
                    return;
                }

                #endregion

                if (Func.ReadINI("Setting", "IO_Board_Control", "IO_Control_1", "0") == "1")
                {
                    string txPin = Func.ReadINI("Setting", "IO_Board_Control", "Pin0", "7");
                    string rev_message = "";
                    status_ATS.AddLog("IO_Board_Y" + txPin + " On...");
                    IO_Board_Control1.ConTrolIOPort_write(Int32.Parse(txPin), "1", ref rev_message);
                    DisplayMsg(LogType.Log, rev_message);
                }
                else
                {
                    SwitchRelay(CTRL.OFF);
                    Thread.Sleep(5000);
                    SwitchRelay(CTRL.ON);
                }

                DisplayMsg(LogType.Log, "Power on!!!");

                #region  Write DUT Infor
                //if (status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode && !isGolden)//disable for verify test plan
                {
                    #region boot loader
                    if (!BootLoader(uart))
                    {
                        //status_ATS.AddDataLog("BootLoader", NG);
                        AddData("BootLoader", 1);
                        return;
                    }
                    AddData("BootLoader", 0);
                    #endregion

                    SetDUTInfo(infor, PortType.UART);
                }
                #endregion


                this.ChkBootUp(infor);
                //===================================
                Thread.Sleep(1000);
                this.WriteHWver(infor);           // add
                //===================================
                //if (status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode)//disable for verify test plan
                {
                    CheckDUTInfo(infor, PortType.UART);
                }

                CheckLED(PortType.UART);

                ResetButton(PortType.UART);

                WPSButton(PortType.UART);

                EthernetTest(PortType.UART);

                //if (status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode)//disable for verify test plan
                {
                    NvramTest(infor, PortType.UART);
                }
                //if (status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode)//disable for verify test plan
                {
                    GenPWD(infor);
                }

                //if (status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode)//disable for verify test plan
                {
                    GetCalixPw(PortType.UART, infor);
                }


            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, @"PCBA " + ex.Message);
                warning = "Exception";
            }
            finally
            {
                if (Func.ReadINI("Setting", "IO_Board_Control", "IO_Control_1", "0") == "1")
                {
                    string txPin = Func.ReadINI("Setting", "IO_Board_Control", "Pin0", "7");
                    string rev_message = "";
                    status_ATS.AddLog("IO_Board_Y" + txPin + " Off...");
                    IO_Board_Control1.ConTrolIOPort_write(Int32.Parse(txPin), "2", ref rev_message);
                    DisplayMsg(LogType.Log, rev_message);
                }
                else
                {
                    SwitchRelay(CTRL.ON);
                }
            }

        }
        private bool ChkBootUp(DeviceInfor infor)
        {
            if (!CheckGoNoGo())
            {
                return false;
            }
            string res = string.Empty;
            bool IsBootOK = true;
            //bool alreadyRetry = false;
            string keyword = "root@OpenWrt";
            //string keyword2 = "IPQ5018#";
            DisplayMsg(LogType.Log, "=============== Check BootUp ===============");
            try
            {
                DisplayMsg(LogType.Log, @"entering kernel await delay 88s");
                Thread.Sleep(88 * 1000);
            //if (!ChkLinux(alreadyRetry, "qca-wifi loaded"))
            //{
            //    DisplayMsg(LogType.Log, "SYSTEM_Fail");
            //    if (!alreadyRetry)
            //    {
            //        SendCommand(PortType.UART, "\r\n", 200);
            //        ChkResponse(PortType.UART, ITEM.NONE, @"root@OpenWrt", out getMsg, 2000);

            //        alreadyRetry = true;
            //        goto retry;
            //    }
            //    AddData("BootUp", 1);
            //    return false;
            //}
            retry:
                if (!ChkLinux(PortType.UART, "qca-wifi loaded", keyword, 70000))
                {
                    DisplayMsg(LogType.Error, @"cannot enter kernel, dmesg and retry");
                    if (!this.HandlingTelnetPingErr())
                    {
                        DisplayMsg(LogType.Log, @"catch keyword qca-wifi, enter kernel again");
                        IsBootOK = false;
                        goto retry;
                    }
                    //if (SendAndChk("IsBootUP", PortType.UART, "dmesg | grep qca-wifi loaded", "qca-wifi loaded", 5000, 10 * 1000))
                    //{
                    //    DisplayMsg(LogType.Log, @"catch keyword qca-wifi loaded, Bootup OK");
                    //    IsBootOK = true;
                    //}
                    //if (!alreadyRetry)
                    //{
                    //    SendCommand(PortType.UART, "\r\n", 200);
                    //    alreadyRetry = true;
                    //    goto retry;
                    //}

                    AddData("BootUp", 1);
                    return false;
                }
                else
                {
                    ChkResponse(PortType.UART, ITEM.NONE, keyword, out res, 2000);
                    SendAndChk(PortType.UART, "\r\n", "#", out res, 1000, 2000);
                    if (res.Contains(keyword))
                    {
                        DisplayMsg(LogType.Log, @"--------- entering kernel OK ---------");
                        AddData("BootUp", 0);
                        IsBootOK = true;
                    }
                    else
                    {
                        DisplayMsg(LogType.Log, @"--------- entering kernel NG; retry again---------");
                        goto retry;
                    }
                    //if (SendAndChk("IsBootUP", PortType.UART, "dmesg | grep qca-wifi loaded", "qca-wifi loaded", 5000, 10 * 1000))
                    //{
                    //    DisplayMsg(LogType.Log, @"catch keyword qca-wifi loaded, Bootup OK");
                    //}
                    //else
                    //{
                    //    DisplayMsg(LogType.Log, @"ChkBootUp FAIL");
                    //    return false;
                    //}
                }
                return IsBootOK;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, @"ChkBootUp " + ex.Message);
                AddData("BootUp", 1);
                return false;
            }
        }
        private string CalculateMD5ofString(string str)
        {
            try
            {
                using (var md5 = MD5.Create())
                {
                    var bytes = Encoding.UTF8.GetBytes(str);
                    var hash = md5.ComputeHash(bytes);
                    return BitConverter.ToString(hash).Replace("-", "").ToUpper();
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, @"CalculateMD5ofString__ " + ex.Message);
            }
            return "error";
        }
        /// <summary>
        /// ////////
        /// </summary>
        /// <param name="infor"></param>
        private void GenPWD(DeviceInfor infor)
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            try
            {
                string md5sum = "";
                string WPA_KEY = "";
                string Admin_PWD = "";
                string Support_PWD = "";

                DisplayMsg(LogType.Log, $"========================== Generate WPA Key/Admin Password/Support Password ==========================");

                md5sum = CalculateMD5ofString($"8d2k{infor.FSAN}").ToLower();
                DisplayMsg(LogType.Log, $"md5sum of 8d2k{infor.FSAN} : {md5sum}");
                if (md5sum.Length != 32)
                {
                    DisplayMsg(LogType.Log, "Get md5sum fail");
                    AddData("GenPWD", 1);
                }

                WPA_KEY = md5sum.Substring(5, 16);
                Admin_PWD = md5sum.Substring(23, 8);
                Support_PWD = Admin_PWD + "!5upporT";
                DisplayMsg(LogType.Log, $"WPA_KEY : {WPA_KEY}");
                DisplayMsg(LogType.Log, $"Admin_PWD : {Admin_PWD}");
                DisplayMsg(LogType.Log, $"Support_PWD : {Support_PWD}");

                if (WPA_KEY.Length == 16 && Admin_PWD.Length == 8 && Support_PWD.Length == 16)
                {
                    AddData("GenPWD", 0);
                    status_ATS.AddDataRaw("LCS5_WPA", WPA_KEY, WPA_KEY, "000000");
                    status_ATS.AddDataRaw("LCS5_ADMIN_PWD", Admin_PWD, Admin_PWD, "000000");
                    status_ATS.AddDataRaw("LCS5_SUPPORT_PWD", Support_PWD, Support_PWD, "000000");
                }
                else
                {
                    AddData("GenPWD", 1);
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, @"GenPWD " + ex.Message);
                AddData("GenPWD", 1);
            }
        }
        private void GetCalixPw(PortType port, DeviceInfor info)
        {
            string res = "";
            if (status_ATS._testMode == StatusUI2.StatusUI.TestMode.EngMode || !CheckGoNoGo())
                return;
            try
            {
                DisplayMsg(LogType.Log, "======== Load Calix =========");
                SendAndChk(port, "tftp -g -r calix.sh 192.168.1.10", "#", out res, 0, 30000);
                SendAndChk(port, "chmod +x calix.sh", "#", out res, 0, 30000);
                SendAndChk(port, $"./calix.sh {info.FSAN}", "#", out res, 0, 3000);
                string auth_code = "";
                foreach (var item in res.Split('\n'))
                {
                    if (item.Contains("Username=support"))
                    {
                        auth_code = item.Substring(0, 93).Trim();
                        DisplayMsg(LogType.Log, "Authentication code:" + auth_code);
                        break;
                    }
                }
                if (auth_code.Length == 0)
                {
                    warning = "Get Authentication code fail";
                    return;
                }
                status_ATS.AddDataRaw("LCS5_ATH_CODE", auth_code, auth_code, "000000");
            }
            catch (Exception ex)
            {
                warning = "Exception";
                DisplayMsg(LogType.Exception, @"GenPWD " + ex.Message);
            }
        }
        private bool WriteOrCheckDeviceInfor(PortType portType, string item, string cmd, string keyword)
        {
            string res = "";
            try
            {
                if (!CheckGoNoGo())
                {
                    return false;
                }
                DisplayMsg(LogType.Log, $"=============== {item} ===============");

                if (!SendAndChk(portType, cmd, keyword, out res, 0, 5000))
                {
                    //this.HandlingTelnetPingErr();
                    AddData(item, 1);
                    return false;
                }
                AddData(item, 0);
                return true;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, @"WriteOrCheckDeviceInfor " + ex.Message);
                AddData(item, 1);
                return false;
            }
        }
        private bool InitPWM(PortType portType)
        {
            string res = "";
            try
            {
                if (!CheckGoNoGo())
                    return false;
                DisplayMsg(LogType.Log, $"========================== InitPWM ==========================");
                string keyword = "root@OpenWrt:/#";

                if (!SendAndChk(portType, "echo 10 > /sys/class/gpio/export", keyword, out res, 200, 3000))
                    return false;

                if (!SendAndChk(portType, "echo out > /sys/class/gpio/gpio10/direction", keyword, out res, 200, 3000))
                    return false;

                if (!SendAndChk(portType, "echo 12 > /sys/class/gpio/export", keyword, out res, 200, 3000))
                    return false;

                if (!SendAndChk(portType, "echo out > /sys/class/gpio/gpio12/direction", keyword, out res, 200, 3000))
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Log, ex.ToString());
                warning = "Exception";
                return false;
            }
        }
        private bool BootUp(SerialPort port, string keyword1 = "root@OpenWrt:/#", int timeOutMs = 200000)
        {
            DisplayMsg(LogType.Log, "=============== Check BootUp ===============");
            DateTime dt;
            TimeSpan ts;

            string res = string.Empty;
            string log = string.Empty;

            dt = DateTime.Now;
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

                    if (!port.IsOpen)
                    {
                        DisplayMsg(LogType.Log, "Open port:" + port.PortName);
                        port.Open();
                    }
                    res = port.ReadExisting();

                    if (res.Length != 0 && res != "\r\n")
                    {
                        DisplayMsg(LogType.Log, res);
                        log += res;
                    }
                    if (log.Contains(keyword1))
                    {
                        DisplayMsg(LogType.Log, "Check '" + keyword1 + "' ok");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, @"BootUp " + ex.Message);
                warning = "Exception";
                return false;
            }
        }
        private bool BootLoader(SerialPort port, int timeOutMs = 10000)
        {
            DisplayMsg(LogType.Log, "=============== Check BootLoader ===============");
            DateTime dt = DateTime.Now;
            TimeSpan ts;
            string res = string.Empty;
            string log = string.Empty;
            string keyword1 = "Hit any key to stop autoboot";
            string keyword2 = "IPQ5018#";
            try
            {
                while (true)
                {
                    if (!port.IsOpen)
                    {
                        DisplayMsg(LogType.Log, "Open port:" + port.PortName);
                        port.Open();
                        Thread.Sleep(500);
                    }
                    ts = new TimeSpan(DateTime.Now.Ticks - dt.Ticks);
                    res = port.ReadExisting();

                    if (res.Length != 0 && res != "\r\n")
                    {
                        DisplayMsg(LogType.Log, res);
                        log += res;
                    }

                    if (log.Contains(keyword1))
                    {
                        //Thread.Sleep(200);
                        port.Write("\n"); //Test Plan(0808) amended to add by THIEM
                        //SendAndChk(PortType.UART, "\r\n", keyword2, out res, 0, 3000);
                        SendAndChk(PortType.UART, "\n", "#", out res, 1000, 3000);
                        if (!res.Contains(keyword2))
                        {
                            return false;
                        }
                        DisplayMsg(LogType.Log, @"--------- entering bootloader ---------");
                        return true;
                    }
                    if (ts.TotalMilliseconds > timeOutMs)
                    {
                        DisplayMsg(LogType.Error, $"Check {keyword1} timeout");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, @"BootLoader " + ex.Message);
                warning = "Exception";
                return false;
            }
        }
        private string MACConvert(string mac, int param = 0)
        {
            try
            {
                DisplayMsg(LogType.Log, "MAC input:" + mac);
                string ethmac = mac.Replace(":", "");
                ethmac = Convert.ToString(Convert.ToInt64(ethmac, 16) + param, 16).PadLeft(12, '0');
                var regex = "(.{2})(.{2})(.{2})(.{2})(.{2})(.{2})";
                var replace = "$1:$2:$3:$4:$5:$6";
                ethmac = Regex.Replace(ethmac, regex, replace).ToUpper();
                DisplayMsg(LogType.Log, "MAC output:" + ethmac);
                return ethmac;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Log, ex.ToString());
                warning = "MAC Convert error";
                return "error";
            }
        }
        private void SetDUTInfo(DeviceInfor infor, PortType portType)
        {
            if (!CheckGoNoGo())
            {
                return;
            }
            string keyword = "#";
            string res = string.Empty;

            try
            {
                #region SerialNumber
                if (WriteOrCheckDeviceInfor(portType, "Write_SN", $"setenv serialno {infor.SerialNumber}", keyword))
                    status_ATS.AddDataRaw("LCS5_LABEL_SN", infor.SerialNumber, infor.SerialNumber, "000000");
                #endregion

                #region base mac
                if (WriteOrCheckDeviceInfor(portType, "Write_BaseMAC", $"setenv baseMAC {infor.BaseMAC = MACConvert(infor.BaseMAC)}", keyword))
                {
                    status_ATS.AddDataRaw("LCS5_BASE_MAC", infor.BaseMAC, infor.BaseMAC, "000000");

                    //status_ATS.AddDataRaw("LCS5_LABEL_ONU_MAC", infor.BaseMAC.Replace(":", ""), infor.BaseMAC.Replace(":", ""), "000000");
                    //status_ATS.AddDataRaw("LCS5_LABEL_MTA_MAC", MACConvert(infor.BaseMAC, 1).Replace(":", ""), MACConvert(infor.BaseMAC, 1).Replace(":", ""), "000000");
                }
                #endregion

                #region ETH0 mac
                if (WriteOrCheckDeviceInfor(portType, "Write_ETH0_MAC", $"setenv eth0addr {infor.Eth1MAC = MACConvert(infor.BaseMAC, 0)}", keyword))
                    status_ATS.AddDataRaw("LCS5_ETH1_MAC", infor.Eth0MAC, infor.Eth0MAC, "000000");
                #endregion


                #region ATH0 wifi2G
                if (WriteOrCheckDeviceInfor(portType, "Write_WiFi2G_MAC", $"setenv wifi0addr {infor.Eth2GMAC = MACConvert(infor.BaseMAC, 2)}", keyword))
                    status_ATS.AddDataRaw("LCS5_ATH0_2G_MAC", infor.Eth2GMAC, infor.Eth2GMAC, "000000");
                #endregion

                #region ATH0 wifi5G
                if (WriteOrCheckDeviceInfor(portType, "Write_WiFi5G_MAC", $"setenv wifi1addr {infor.Eth5GMAC = MACConvert(infor.BaseMAC, 3)}", keyword))
                    status_ATS.AddDataRaw("LCS5_ATH1_5G_MAC", infor.Eth5GMAC, infor.Eth5GMAC, "000000");
                #endregion

                #region FSAN
                if (WriteOrCheckDeviceInfor(portType, "Write_FSAN", $"setenv FSAN {infor.FSAN}", keyword))
                    status_ATS.AddDataRaw("LCS5_SSID_SN_FSAN", infor.FSAN, infor.FSAN, "000000");
                #endregion

                SendAndChk(portType, $"saveenv", keyword, out res, 0, 3000);
                SendAndChk(portType, $"saveenv", keyword, out res, 0, 3000);
                SendAndChk(portType, $"reset", "", out res, 0, 3000);
                Thread.Sleep(1000);

                AddData("SetDUTInfo", 0);
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, @"SetDUTInfo " + ex.Message);
                AddData("SetDUTInfo", 1);
            }
        }
        private void CheckDUTInfo(DeviceInfor infor, PortType portType)
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            try
            {
                //if (status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode && !isGolden)
                {

                    #region fw                    
                    WriteOrCheckDeviceInfor(portType, "Check_FW", $"cat etc/wnc_ver", infor.FWver);
                    #endregion
                    #region sn
                    #region sn
                    WriteOrCheckDeviceInfor(portType, "Check_SN", $"fw_printenv serialno", infor.SerialNumber);
                    #region base mac
                    WriteOrCheckDeviceInfor(portType, "Check_BaseMAC", $"fw_printenv baseMAC", infor.BaseMAC);
                    #endregion

                    #region Eth1 MAC
                    WriteOrCheckDeviceInfor(portType, "Check_Eth1MAC", $"fw_printenv eth1addr", infor.Eth1MAC);
                    #endregion
                    #region Ath0 MAC 2G
                    WriteOrCheckDeviceInfor(portType, "Check_Eth1MAC", $"fw_printenv wifi0addr", infor.Eth2GMAC);
                    #endregion
                    #region Ath1 MAC 5G
                    WriteOrCheckDeviceInfor(portType, "Check_Eth1MAC", $"fw_printenv wifi1addr", infor.Eth5GMAC);
                    #endregion
                    #region FSAN
                    WriteOrCheckDeviceInfor(portType, "Check_FSAN", $"fw_printenv FSAN", infor.FSAN);
                    #endregion
                    #endregion Check Hw version
                    WriteOrCheckDeviceInfor(portType, "Check_HWver", $"fw_printenv hwver", "hwver=" + infor.HWver);
                    #endregion
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, @"CheckDUTInfo " + ex.Message);
                AddData("CheckDUTInfo", 1);
            }
        }
        private void EthernetTest(PortType portType)
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            DisplayMsg(LogType.Log, $"========================== Ethernet Test ==========================");

            try
            {
                int timeOutMs = 10 * 1000;
                string keyword = "root@OpenWrt";
                string res = string.Empty;
                bool result = false;

                string PC_IP = Func.ReadINI("Setting", "PCBA", "Eth0_PC_IP", "192.168.1.100");

                if (SendAndChk("EthernetTest", portType, $"ping {PC_IP}", "ttl", 0, 5000))
                {
                    //SendAndChk("terminate Ping", portType, $"\0x3", null, 0, 5000);
                    SendCommand(portType, sCtrlC, 500);
                }
                else
                {
                    DisplayMsg(LogType.Log, "Ping PC Fail");
                    AddData("EthernetTest", 1);
                    return;
                }

                if (!SendAndChk("EthernetTest", portType, "ethtool eth1 | grep Speed", "2500Mb/s", 0, 5000)) //2500Mb coz laptop
                {
                    DisplayMsg(LogType.Log, "Check eth1 speed fail");
                    AddData("EthernetTest", 1);
                    return;
                }
                if (!SendAndChk("EthernetTest", portType, "ethtool eth1 | grep \"Link detected\"", "yes", 0, 5000))
                {
                    DisplayMsg(LogType.Log, "Check eth1 link detected fail");
                    AddData("EthernetTest", 1);
                    return;
                }
                AddData("EthernetTest", 0);
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, @"EthernetTest " + ex.Message);
                AddData("EthernetTest", 1);
            }
        }
        private bool PingDUT(string item, PortType portType, string ip, string keyword, out string res, int timeOutMs = 15 * 1000, int retry = 0)
        {
            bool result = false;
            res = "";
            int c = 0;
            try
            {

            retry:
                SendCommand(portType, "ping " + ip + " -c 50", 500);
                ChkResponse(portType, ITEM.NONE, keyword, out res, timeOutMs);

                if (res.Contains(ip) && res.Contains(keyword))
                {
                    AddData(item, 0);
                    result = true;
                }
                else
                {
                    if (c < retry)
                    {
                        c++;
                        SendCommand(portType, sCtrlC, 500);
                        ChkResponse(portType, ITEM.NONE, "root@OpenWrt", out res, timeOutMs);
                        Thread.Sleep(1000);
                        goto retry;
                    }
                    AddData(item, 1);
                    result = false;
                }
                SendCommand(portType, sCtrlC, 500);
                ChkResponse(portType, ITEM.NONE, "root@OpenWrt", out res, timeOutMs);
                return result;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, @"PingDUT " + ex.Message);
                AddData(item, 1);
                return false;
            }
        }
        private void SFP_TP_Test()
        {
            try
            {
                string res = "";
                double TP_value = -999;
                string TP_unit = "";
                string Iperf_CMD_TX = WNC.API.Func.ReadINI("Setting", "Iperf", "SFP_TX_CMD", "iperf3 -c 192.168.1.15 -i 1 -w 2m -t 10 -p 11111");
                int timeOutMs = Convert.ToInt32(Func.ReadINI("Setting", "Iperf", "SFPTxTimeOutMs", "20000"));

                DisplayMsg(LogType.Log, "=== SFP TP Test ===");

                SendCommand(PortType.UART, Iperf_CMD_TX, 500);
                if (!ChkResponse(PortType.UART, ITEM.NONE, "root@OpenWrt", out res, timeOutMs))
                {
                    SendCommand(PortType.UART, sCtrlC, 500);
                }

                //check iperf result
                if (!res.Contains("Gbits/sec") && !res.Contains("Mbits/sec"))
                {
                    DisplayMsg(LogType.Log, "SFP iperf fail");
                    AddData("SFP_TP_Test", 1);
                    return;
                }

                string[] msg = res.Split('\n');
                for (int i = 0; i < msg.Length; i++)
                {
                    //[  4]   0.00-10.00  sec  3.80 GBytes  3.26 Gbits/sec                  receiver
                    if (msg[i].Contains("receiver"))
                    {
                        Match m = Regex.Match(msg[i], @"([\.\d]+) (\w+)/sec");
                        if (m.Success)
                        {
                            TP_value = Convert.ToDouble(m.Groups[1].Value);
                            TP_unit = m.Groups[2].Value.Trim();
                            DisplayMsg(LogType.Log, $"SFP TP : {TP_value} {TP_unit}");
                        }
                        break;
                    }
                }
                status_ATS.AddData("SFP_TP", TP_unit, TP_value);
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, @"SFP_TP_Test " + ex.Message);
                AddData("SFP_TP_Test", 1);
            }
        }
        private void CheckLED(PortType portType)
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            try
            {

                InitPWM(portType);

                #region RED
                DisplayMsg(LogType.Log, $"========================== RED LED ON ==========================");
                SendAndChk(portType, "echo 1 > /sys/class/gpio/gpio10/value", "#", 200, 3000);
                if (!usecamera)
                {
                    DisplayMsg(LogType.Log, "Is the red led on?");
                    frmYN.Label = "Is the red led on?";
                    //frmYN.ShowDialog();
                    //markup for stress Test
                    if (frmYN.no)
                    {
                        DisplayMsg(LogType.Log, "Selected no");
                        AddData("RED_LED_ON", 1);
                        return;
                    }
                    DisplayMsg(LogType.Log, "Selected yes");
                    AddData("RED_LED_ON", 0);
                }
                else
                {
                    if (!Camera())
                    {
                        warning = "Camera error";
                        return;
                    }
                    if (!CheckCameraResult("Item_1", "red"))
                    {
                        AddData("RED_LED_ON", 1);
                        return;
                    }
                    else
                    {
                        AddData("RED_LED_ON", 0);
                    }
                }

                DisplayMsg(LogType.Log, $"========================== RED LED OFF ==========================");
                SendAndChk(portType, "echo 0 > /sys/class/gpio/gpio10/value", "#", 200, 3000);
                if (!usecamera)
                {
                    DisplayMsg(LogType.Log, "Is the red led off?");
                    frmYN.Label = "Is the red led off?";
                    //frmYN.ShowDialog();
                    //markup for stress Test

                    if (frmYN.no)
                    {
                        DisplayMsg(LogType.Log, "Selected no");
                        AddData("RED_LED_OFF", 1);
                        return;
                    }
                    DisplayMsg(LogType.Log, "Selected yes");
                    AddData("RED_LED_OFF", 0);
                }
                else
                {
                    if (!Camera())
                    {
                        warning = "Camera error";
                        return;
                    }
                    if (!CheckCameraResult("Item_1", "black"))
                    {
                        AddData("RED_LED_OFF", 1);
                        return;
                    }
                    else
                        AddData("RED_LED_OFF", 0);
                }
                #endregion

                #region GREEN
                DisplayMsg(LogType.Log, $"========================== CHECK GREEN LED ON ==========================");
                SendAndChk(portType, "echo 1 > /sys/class/gpio/gpio12/value", "#", 200, 3000);
                if (!usecamera)
                {
                    DisplayMsg(LogType.Log, "Is the green led on?");
                    frmYN.Label = "Is the green led on?";
                    //frmYN.ShowDialog();
                    //markup for stress Test

                    if (frmYN.no)
                    {
                        DisplayMsg(LogType.Log, "Selected no");
                        AddData("GREEN_LED_ON", 1);
                        return;
                    }
                    DisplayMsg(LogType.Log, "Selected yes");
                    AddData("GREEN_LED_ON", 0);
                }
                else
                {
                    if (!Camera())
                    {
                        warning = "Camera error";
                        return;
                    }
                    if (!CheckCameraResult("Item_1", "green"))
                    {
                        AddData("GREEN_LED_ON", 1);
                        return;
                    }
                    else
                    {
                        AddData("GREEN_LED_ON", 0);
                    }
                }


                DisplayMsg(LogType.Log, $"========================== CHECK GREEN LED OFF ==========================");
                SendAndChk(portType, "echo 0 > /sys/class/gpio/gpio12/value", "#", 200, 3000);
                if (!usecamera)
                {
                    DisplayMsg(LogType.Log, "Is the green led off?");
                    frmYN.Label = "Is the green led off?";
                    //frmYN.ShowDialog();
                    //markup for stress Test

                    if (frmYN.no)
                    {
                        DisplayMsg(LogType.Log, "Selected no");
                        AddData("GREEN_LED_OFF", 1);
                        return;
                    }
                    DisplayMsg(LogType.Log, "Selected yes");
                    AddData("GREEN_LED_OFF", 0);
                }
                else
                {
                    if (!Camera())
                    {
                        warning = "Camera error";
                        return;
                    }
                    if (!CheckCameraResult("Item_1", "black"))
                    {
                        AddData("GREEN_LED_OFF", 1);
                        return;
                    }
                    else
                    {
                        AddData("GREEN_LED_OFF", 0);
                    }
                }
                #endregion              

            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, @"CheckLED " + ex.Message);
                AddData("LED", 1);
            }
        }
        private void WPSButton(PortType portType)
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            try
            {
                string res = "";

                #region WPS
                DisplayMsg(LogType.Log, $"========================== WPS Button ==========================");

                SendAndChk("WPSButton", portType, "echo 28 > /sys/class/gpio/export", "#", 0, 3000);
                SendAndChk("WPSButton", portType, "echo in > /sys/class/gpio/gpio28/direction", "#", 0, 3000);

                bool pressed = false;
                bool released = false;
                for (int i = 0; i < 3; i++)
                {
                    if (!useShield)
                    {
                        frmOK.Label = "Press WPS button then click OK";
                        pressed = true;
                        released = true;
                        //frmOK.ShowDialog();
                        //markup for stress Test

                    }
                    else
                    {
                        //fixture.ControlIO(Fixture.FixtureIO.IO_10, CTRL.ON);
                    }

                    SendCommand(portType, "cat /sys/class/gpio/gpio28/value", 0);
                    ChkResponse(portType, ITEM.NONE, "#", out res, 2000);
                    if (res.Contains("0"))
                    {
                        pressed = true;
                        DisplayMsg(LogType.Log, "Check WPS Button pressed ok");
                    }
                    if (!useShield)
                    {
                        frmOK.Label = "Release WPS button then click OK";
                        //frmOK.ShowDialog();
                        pressed = true;
                        released = true;
                        //markup for stress Test

                    }
                    else
                    {
                        //fixture.ControlIO(Fixture.FixtureIO.IO_10, CTRL.OFF);
                    }
                    SendCommand(portType, "cat /sys/class/gpio/gpio28/value", 0);
                    ChkResponse(portType, ITEM.NONE, "#", out res, 2000);
                    if (res.Contains("1"))
                    {
                        released = true;
                        DisplayMsg(LogType.Log, "Check WPS Button released ok");
                    }
                    if (pressed && released)
                    {
                        AddData("WPSButton", 0);
                        DisplayMsg(LogType.Log, "Check WPSButton Pass");
                        break;
                    }
                }
                if (!pressed || !released)
                {
                    DisplayMsg(LogType.Log, "Check WPS button fail");
                    AddData("WPSButton", 1);
                    return;
                }
                #endregion
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, @"WPSButton " + ex.Message);
                AddData("WPSButton", 1);
            }
        }
        private void ResetButton(PortType portType)
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            try
            {
                string res = "";

                #region RESET
                DisplayMsg(LogType.Log, $"========================== Reset Button ==========================");

                SendAndChk("ResetButton", portType, "rmmod gpio_button_hotplug", "#", 0, 3000);
                SendAndChk("ResetButton", portType, "echo 38 > /sys/class/gpio/export", "#", 0, 3000);
                SendAndChk("ResetButton", portType, "echo in > /sys/class/gpio/gpio38/direction", "#", 0, 3000);

                bool pressed = false;
                bool released = false;
                for (int i = 0; i < 5; i++)
                {
                    if (!useShield)
                    {
                        frmOK.Label = "Press and hold reset button then click OK";
                        //frmOK.ShowDialog();
                        pressed = true;
                        released = true;

                        //markup for stress Test
                    }
                    else
                    {
                        //fixture.ControlIO(Fixture.FixtureIO.IO_9, CTRL.ON);
                    }
                    SendCommand(portType, "cat /sys/class/gpio/gpio38/value", 0);
                    ChkResponse(portType, ITEM.NONE, "#", out res, 2000);
                    if (res.Contains("0"))
                    {
                        pressed = true;
                        DisplayMsg(LogType.Log, "Check Reset Button pressed ok");
                    }
                    if (!useShield)
                    {
                        frmOK.Label = "Release reset button then click OK";
                        //frmOK.ShowDialog();
                        pressed = true;
                        released = true;
                        //markup for stress Test

                    }
                    else
                    {
                        //fixture.ControlIO(Fixture.FixtureIO.IO_9, CTRL.OFF);
                    }
                    SendCommand(portType, "cat /sys/class/gpio/gpio38/value", 0);
                    ChkResponse(portType, ITEM.NONE, "#", out res, 2000);
                    if (res.Contains("1"))
                    {
                        released = true;
                        DisplayMsg(LogType.Log, "Check Reset Button released ok");
                    }
                    if (pressed && released)
                    {
                        AddData("ResetButton", 0);
                        DisplayMsg(LogType.Log, "Check WPSButton Pass");
                        break;
                    }
                }
                if (!pressed || !released)
                {
                    DisplayMsg(LogType.Log, "Check Reset button fail");
                    AddData("ResetButton", 1);
                    return;
                }
                #endregion
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, @"ResetButton " + ex.Message);
                AddData("ResetButton", 1);
            }
        }
        private void USB30Test(PortType portType)
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            string rec = "";
            string errorItem = "USB30Test";

            try
            {
                DisplayMsg(LogType.Log, $"========================== USB3.0 Test ==========================");

                SendCommand(portType, "cat /sys/bus/usb/devices/2-1/speed", 0);
                ChkResponse(portType, ITEM.NONE, "root@OpenWrt", out rec, 3000);

                Match m = Regex.Match(rec, @"\n(?<usbspeed>\d+)");
                if (m.Success)
                {
                    double usbSpeed = Convert.ToInt64(m.Groups["usbspeed"].Value);
                    DisplayMsg(LogType.Log, $"USB3.0 Speed : {usbSpeed}");
                    AddData(errorItem, 0);
                    DisplayMsg(LogType.Log, "USB 3.0 test Cycle PASS");
                }
                else
                {
                    DisplayMsg(LogType.Log, "Cannot get usb speed value");
                    AddData(errorItem, 1);
                    return;
                }
                //TuanLV22001080 remove; follow by Jed reduce cycletime (date: 20230428) 保留USB SPEED, Final read/write測試
                if (station == "Final")
                {
                    //Rena_20221017 modify loop times as 1
                    SendCommand(portType, "./FlashTestLCS1V31.sh 1 1", 200);
                    ChkResponse(portType, ITEM.NONE, "root@OpenWrt", out rec, 100 * 1000);
                    if (rec.Contains("1 Cycle Test"))
                    {
                        AddData(errorItem, 0);
                        DisplayMsg(LogType.Log, "USB 3.0 test Cycle PASS");
                    }
                    else
                    {
                        AddData(errorItem, 1);
                        DisplayMsg(LogType.Log, "USB 3.0 test Cycle NG");
                        return;
                    }
                }

            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                AddData(errorItem, 1);
            }
        }

        private void NvramTest(DeviceInfor infor, PortType portType)
        {
            if (!CheckGoNoGo())
            {
                return;
            }
            try
            {
                string res = "";
                DisplayMsg(LogType.Log, $"========================== Nvram Test ==========================");

                SendAndChk("Nvram", portType, "wnc_nvram -h", "root@OpenWrt", 0, 3000);
                SendAndChk("Nvram", portType, "wnc_nvram -i a.bin", "root@OpenWrt", 0, 3000);
                SendAndChk("Nvram", portType, "wnc_nvram -f a.bin -V 2", "Version: 2", 0, 3000);
                SendAndChk("Nvram", portType, "wnc_nvram -f a.bin -I 0x5C", "ID:92", 0, 3000);
                SendAndChk("Nvram", portType, "wnc_nvram -f a.bin -n GPR1027E", "This name is", 0, 3000);
                SendAndChk("Nvram", portType, "wnc_nvram -f a.bin -C 'BVMNC00ARA'", "\nBVMNC00ARA", 0, 3000);
                // pend to determinate the criteria keywords
                //SendAndChk("Nvram", portType, "dd if=/dev/zero of=/dev/mmcblk0p27", "dd", 0, 3000);
                //SendAndChk("Nvram", portType, "dd if=/a.bin of=/dev/mmcblk0p27", "2+0", 0, 3000);
                //SendAndChk("Nvram", portType, "hexdump -C /dev/mmcblk0p27", "000000", 0, 3000);



                //MFG Serial number
                if (string.IsNullOrEmpty(infor.SerialNumber))
                {
                    DisplayMsg(LogType.Log, "SerialNumber is empty, write nvram fail");
                    AddData("Nvram", 1);
                    return;
                }
                else
                {
                    SendAndChk("Nvram", portType, "wnc_nvram -f a.bin -S 630301000027", "is 630301000027", 0, 3000);
                }

                //HW Part Number
                if (string.IsNullOrEmpty(infor.PartNumber))
                {
                    DisplayMsg(LogType.Log, "PartNumber is empty, write nvram fail");
                    AddData("Nvram", 1);
                    return;
                }
                else
                {
                    SendAndChk("Nvram", portType, "wnc_nvram -f a.bin -H 3000301302", "is 3000301302", 0, 3000);
                }

                if (status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode)
                {
                    status_ATS.AddDataRaw("LCS5_300_PN", infor.PartNumber, infor.PartNumber, "000000");
                    var regex = "(.{3})(.{5})(.{2})";
                    var replace = "$1-$2 $3";
                    string newString = Regex.Replace(infor.PartNumber, regex, replace);
                    status_ATS.AddDataRaw("LCS5_LABEL_300_PN", newString, newString, "000000");
                }

                //Model Number(100 Part number)
                if (string.IsNullOrEmpty(infor.PartNumber_100))
                {
                    DisplayMsg(LogType.Log, "PartNumber_100 is empty, write nvram fail");
                    AddData("Nvram", 1);
                    return;
                }
                else
                {
                    SendAndChk("Nvram", portType, "wnc_nvram -f a.bin -M " + infor.PartNumber_100, "is " + infor.PartNumber_100, 0, 3000);
                }

                if (status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode)
                {
                    status_ATS.AddDataRaw("LCS5_100_PN", infor.PartNumber_100, infor.PartNumber_100, "000000");
                    status_ATS.AddDataRaw("LCS5_LABEL_PN_REV", infor.PartNumber_100.Substring(infor.PartNumber_100.Length - 2, 2), infor.PartNumber_100.Substring(infor.PartNumber_100.Length - 2, 2), "000000");
                    var regex = "(.{3})(.{5})(.{2})";
                    var replace = "$1-$2 $3";
                    string newString = Regex.Replace(infor.PartNumber_100, regex, replace);
                    status_ATS.AddDataRaw("LCS5_LABEL_100_PN", newString, newString, "000000");
                }

                //Manufacture Information
                if (string.IsNullOrEmpty(infor.MFGDate))
                {
                    DisplayMsg(LogType.Log, "MFGDate is empty, write nvram fail");
                    AddData("Nvram", 1);
                    return;
                }
                else
                {
                    SendAndChk("Nvram", portType, "wnc_nvram -f a.bin -1 " + infor.MFGDate, "is " + infor.MFGDate, 0, 3000);
                }

                //mac num
                SendAndChk("Nvram", portType, "wnc_nvram -f a.bin -N 5", "\n5", 0, 3000);

                //Base MAC address
                if (string.IsNullOrEmpty(infor.BaseMAC))
                {
                    DisplayMsg(LogType.Log, "BaseMAC is empty, write nvram fail");
                    AddData("Nvram", 1);
                    return;
                }
                else
                {
                    SendAndChk("Nvram", portType, "wnc_nvram -f a.bin -a " + infor.BaseMAC.Replace(":", ""), "is " + ConvertStringToMACFormat(infor.BaseMAC).Replace(":", "-").ToLower(), 0, 3000);
                }

                //FSAN
                if (string.IsNullOrEmpty(infor.FSAN))
                {
                    DisplayMsg(LogType.Log, "FSAN is empty, write nvram fail");
                    AddData("Nvram", 1);
                    return;
                }
                else
                {
                    SendAndChk("Nvram", portType, "wnc_nvram -f a.bin -F " + infor.FSAN, "is " + infor.FSAN, 0, 3000);
                }

                //gpon_pw
                SendAndChk("Nvram", portType, $"wnc_nvram -f a.bin -P '0'", "is 0", 0, 3000);

                //country_code
                SendAndChk("Nvram", portType, "wnc_nvram -f a.bin -c US", "is US", 0, 3000);

                //Calix MFG version
                SendAndChk("Nvram", portType, $"wnc_nvram -f a.bin -f '{infor.CalixFWver}'", $"{infor.CalixFWver}", 0, 3000);
                status_ATS.AddDataRaw("Calix FW version", infor.CalixFWver, infor.CalixFWver, "000000");

                //module id
                SendAndChk("Nvram", portType, $"wnc_nvram -f a.bin -D '{infor.ModuleId}'", $"is:{infor.ModuleId}", 0, 3000);

                //Burnin nvram value to nvram partition
                SendAndChk(portType, "dd if=/dev/zero of=/dev/mmcblk0p27", "root@OpenWrt", out res, 0, 3000);
                SendAndChk(portType, "dd if=/a.bin of=/dev/mmcblk0p27", "root@OpenWrt", out res, 0, 3000);
                SendAndChk(portType, "hexdump -C /dev/mmcblk0p27", "root@OpenWrt", out res, 0, 3000);
                SendAndChk(portType, "dd if=/dev/mmcblk0p27 of=/a.bin", "root@OpenWrt", out res, 0, 3000);
                SendAndChk(portType, "wnc_nvram -r a.bin -A", "root@OpenWrt", out res, 0, 3000);
                bool rs = false;
                rs = CheckNvram(res, "Version: 2");
                rs = rs && CheckNvram(res, "ID: 92");
                rs = rs && CheckNvram(res, "This name is GPR1027E");
                rs = rs && CheckNvram(res, "CLEI code is BVMNC00ARA");
                rs = rs && CheckNvram(res, infor.SerialNumber);
                rs = rs && CheckNvram(res, infor.PartNumber);
                rs = rs && CheckNvram(res, infor.PartNumber_100);
                rs = rs && CheckNvram(res, infor.MFGDate);
                rs = rs && CheckNvram(res, "MAC quantity is 5");
                rs = rs && CheckNvram(res, ConvertStringToMACFormat(infor.BaseMAC).Replace(":", "-").ToLower());
                rs = rs && CheckNvram(res, infor.FSAN);
                rs = rs && CheckNvram(res, "GPON password is " + infor.GPON);
                rs = rs && CheckNvram(res, "Country code is US");
                rs = rs && CheckNvram(res, "Calix FW version is " + infor.CalixFWver);
                rs = rs && CheckNvram(res, "DTM Model ID is:" + infor.ModuleId);

                SendAndChk(portType, "rm -f a.bin", "root@OpenWrt", out res, 0, 3000);

                if (rs)
                    AddData("Nvram", 0);
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, @"NvramTest " + ex.Message);
                AddData("Nvram", 1);
            }
        }
        bool CheckNvram(string data, string keyword)
        {
            try
            {
                if (!CheckGoNoGo())
                    return false;
                if (!data.Contains(keyword))
                {
                    DisplayMsg(LogType.Log, $"Check '{keyword}' fail");
                    AddData("Nvram", 1);
                    return false;
                }
                DisplayMsg(LogType.Log, $"Check '{keyword}' ok");
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, @"CheckNvram " + ex.Message);
            }
            return true;
        }
        private void SFP_TEST()
        {
            try
            {
                if (!CheckGoNoGo())
                    return;
                DisplayMsg(LogType.Log, $"========================== SFP_TEST ==========================");
                SendAndChk("SFP", PortType.UART, "echo 467 > sys/class/gpio/export", "root@OpenWrt", 0, 3000);
                SendAndChk("SFP", PortType.UART, "echo 468 > sys/class/gpio/export", "root@OpenWrt", 0, 3000);
                SendAndChk("SFP", PortType.UART, "echo 473 > sys/class/gpio/export", "root@OpenWrt", 0, 3000);
                SendAndChk("SFP", PortType.UART, "echo 502 > sys/class/gpio/export", "root@OpenWrt", 0, 3000);

                SendAndChk("SFP", PortType.UART, "cat sys/class/gpio/gpio467/value", "\n0", 0, 3000);
                SendAndChk("SFP", PortType.UART, "cat sys/class/gpio/gpio468/value", "\n0", 0, 3000);
                SendAndChk("SFP", PortType.UART, "cat sys/class/gpio/gpio473/value", "\n0", 0, 3000);
                SendAndChk("USFPPS", PortType.UART, "cat sys/class/gpio/gpio502/value", "\n0", 0, 3000);

                if (useShield)
                    //fixture.ControlIO(Fixture.FixtureIO.IO_7, CTRL.OFF);
                    MessageBox.Show("Hay Rut Cong SFP/ Pls Unplug SFP port");
                for (int i = 0; i < 5; i++)
                {
                    string rev_message = "";
                    status_ATS.AddLog("IO_Board_Y" + i + " On...");
                    IO_Board_Control2.ConTrolIOPort_write(i, "1", ref rev_message);
                }

                SendAndChk("SFP", PortType.UART, "echo 1 > /sys/class/gpio/gpio473/value", "root@OpenWrt", 0, 3000);

                SendAndChk("SFP", PortType.UART, "cat sys/class/gpio/gpio467/value", "\n1", 0, 3000);
                SendAndChk("SFP", PortType.UART, "cat sys/class/gpio/gpio468/value", "\n1", 0, 3000);
                SendAndChk("SFP", PortType.UART, "cat sys/class/gpio/gpio473/value", "\n1", 0, 3000);
                SendAndChk("SFP", PortType.UART, "cat sys/class/gpio/gpio502/value", "\n1", 0, 3000);
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Log, ex.ToString());
                warning = "Exception";

            }
            finally
            {
                for (int i = 0; i < 5; i++)
                {
                    string rev_message = "";
                    status_ATS.AddLog("IO_Board_Y" + i + " Off...");
                    IO_Board_Control2.ConTrolIOPort_write(i, "2", ref rev_message);
                }
            }
        }
        private void MessageOut(object sender, EzComportMessageDumpedEventArgs e)
        {
            DisplayMsg(LogType.Log, e.Message);
        }
        private void VoiceTest_ByUsbModem(PortType port)
        {
            var comInfo1 = WNC.API.Func.ReadINI("Setting", "Port", "Modem_1_COM", "COM3");
            var comInfo2 = WNC.API.Func.ReadINI("Setting", "Port", "Modem_2_COM", "COM3");
            if (CheckGoNoGo() == false) return;
            string res = "";
            if (!SendAndChk("VoiceTest", port, "slic_le9632 -a 0 -r 25", "Calibration completed on both lines", 0, 5000))
            {
                AddData("SlicTest", 1);
                DisplayMsg(LogType.Log, "Initialize SLIC fail");
                return;
            }
            DisplayMsg(LogType.Log, "Initialize SLIC ok");
            AddData("SlicTest", 0);
            EzComport com1 = null;
            EzComport com2 = null;
            try
            {
                com1 = new EzComport();
                com2 = new EzComport();
                com1.ComportMessageDumped += MessageOut;
                com2.ComportMessageDumped += MessageOut;

                #region com1 send

                if (!com1.OpenComport(comInfo1, 115200))
                {
                    DisplayMsg(LogType.Log, $"Initial {comInfo1} fail");
                    AddData("VoiceTest", 1);
                    return;
                }
                if (!com2.OpenComport(comInfo2, 115200))
                {
                    DisplayMsg(LogType.Log, $"Initial {comInfo2} fail");
                    AddData("VoiceTest", 1);
                    return;
                }
                int c = 3;
            retry1:
                DisplayMsg(LogType.Log, comInfo1 + " send:");
                com1.WriteLineAndWait("atx", "OK", 5);
                DisplayMsg(LogType.Log, $"Delay 3s");
                Thread.Sleep(3000);
                com1.WriteLine("atd2", 1000);
                if (com2.WaitFor("RING", 40))
                {
                    bool check = false;
                    for (int i = 0; i < 3; i++)
                    {
                        Thread.Sleep(1000);
                        DisplayMsg(LogType.Log, comInfo1 + " send:");
                        if (com1.WriteLineAndWait("\n", "NO CARRIER", 5))
                        { check = true; break; }
                    }
                    if (!check)
                    {
                        DisplayMsg(LogType.Log, $"Wait 'NO CARRIER' fail");
                        AddData("VoiceTest", 1);
                        return;
                    }
                }
                else
                {
                    if (c > 0)
                    {
                        c--;
                        goto retry1;
                    }
                    DisplayMsg(LogType.Log, $"Wait 'RING' fail");
                    AddData("VoiceTest", 1);
                    return;
                }
                #endregion

                #region com2 send
                c = 3;
            retry2:
                DisplayMsg(LogType.Log, comInfo2 + " send:");
                com2.WriteLineAndWait("atx", "OK", 5);
                DisplayMsg(LogType.Log, $"Delay 3s");
                Thread.Sleep(3000);
                com2.WriteLine("atd1", 1000);

                if (com1.WaitFor("RING", 40))
                {

                    bool check = false;
                    for (int i = 0; i < 3; i++)
                    {
                        Thread.Sleep(1000);
                        DisplayMsg(LogType.Log, comInfo2 + " send:");
                        if (com2.WriteLineAndWait("\n", "OK", 5))
                        { check = true; break; }
                    }
                    if (!check)
                    {
                        DisplayMsg(LogType.Log, $"Wait 'NO CARRIER' fail");
                        AddData("VoiceTest", 1);
                        return;
                    }
                }
                else
                {
                    DisplayMsg(LogType.Log, $"Wait 'RING' fail");
                    if (c > 0)
                    {
                        c--;
                        goto retry2;
                    }
                    AddData("VoiceTest", 1);
                    return;
                }
                #endregion
                AddData("VoiceTest", 0);
            }
            catch (Exception e)
            {
                status_ATS.AddLog("Exception:" + e.Message);
                AddData("VoiceTest", 1);
            }
            finally
            {
                com1.Close();
                com2.Close();
                SendCommand(port, sCtrlC, 2000);
                ChkResponse(port, ITEM.NONE, "@", out res, 3000);
            }
        }

        bool SendCMD(SerialPort port, string cmd, string keyword, int delay, int timeout)
        {
            try
            {
                if (cmd.Length != 0)
                {
                    port.WriteLine(cmd);
                    DisplayMsg(LogType.Log, $"{port.PortName} send:{cmd}");
                    DisplayMsg(LogType.Log, $"Delay {delay}ms");
                    Thread.Sleep(delay);
                }

                DateTime dt = DateTime.Now;
                while (true)
                {
                    if (dt.AddMilliseconds(timeout) < DateTime.Now)
                    {
                        DisplayMsg(LogType.Log, "Timeout");
                        warning = "Timeout";
                        return false;
                    }
                    string data = port.ReadExisting();
                    if (data.Length != 0)
                        DisplayMsg(LogType.Log, $"{port.PortName} received:{data}");
                    if (data.Contains(keyword))
                    {
                        DisplayMsg(LogType.Log, $"{port.PortName} check '{keyword}' ok");
                        return true;
                    }
                    if (keyword.Length == 0)
                        return true;
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Log, ex.ToString());
                warning = "Exception";
                return false;
            }
        }
        private void CheckFWVersion()
        {
            if (!CheckGoNoGo() || isGolden)
            {
                return;
            }
            string FWversion = "(*&^%";// Func.ReadINI("Setting", "PCBA", "FWver", "BSP P1.3.2.4-3 QSDK SPF-12.0CS");
            string name = Func.ReadINI("Setting", "SFCS", "MFGFW_Name", "@LCS5_MFG_FW_VER_18");
            string errorItem = "ChkFWVer";
            string rec = "";
            try
            {
                DisplayMsg(LogType.Log, "========================== Check FW version ==========================");
                GetFromSfcs(name, out FWversion);
                FWversion = FWversion.Substring(0, FWversion.Length - 10); //P3.0.2.5.0
                DisplayMsg(LogType.Log, "FW version in sfcs:" + FWversion);
                SendCommand(PortType.UART, "cat etc/wnc_ver", 200);
                ChkResponse(PortType.UART, ITEM.NONE, "root@OpenWrt", out rec, 2000);
                //if (FWversion.Contains(","))
                //{
                //    bool chkfw = false;
                //    foreach (var item in FWversion.Split(','))
                //    {
                //        if (rec.Contains(item) && !String.IsNullOrEmpty(item))
                //        {
                //            AddData(errorItem, 0);
                //            DisplayMsg(LogType.Log, "Check FW Version PASS");
                //            chkfw = true;
                //            break;
                //        }
                //    }
                //    if (!chkfw)
                //    {
                //        AddData(errorItem, 1);
                //        DisplayMsg(LogType.Log, "Check FW Version NG");
                //        return;
                //    }
                //}
                //else
                //{
                Net.NetPort netport = new Net.NetPort();
                string a = netport.getMiddleString(rec, "BSP ", "-").Trim();
                if (rec.Contains(FWversion))
                {
                    AddData(errorItem, 0);
                    DisplayMsg(LogType.Log, "Check FW Version PASS");
                    if (status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode)
                    {
                        status_ATS.AddDataRaw("LCS5_MFG_FW_VER", FWversion, FWversion, "000000");
                    }
                }
                else
                {
                    AddData(errorItem, 1);
                    DisplayMsg(LogType.Log, "Check FW Version NG");
                    return;
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData("ChkFWVer", 1);
            }
        }
        private void CheckHWVersion()
        {
            if (!CheckGoNoGo() || isGolden)
            {
                return;
            }

            string keyword = "root@OpenWrt";
            string getMsg = "";
            string errorItem = "ChkHWVer";
            int timeOutMs = 3000;
            string binaryValue = "";

            try
            {
                DisplayMsg(LogType.Log, "========================== Check HW Version ==========================");
                //Rena_20221013 add 
                //SE_TODO: modify HW version for production
                string HWversion = Func.ReadINI("Setting", "PCBA", "HWver", "0001");

                DisplayMsg(LogType.Log, "HW version in setting:" + HWversion);
                SendCommand(PortType.UART, "echo 449 > /sys/class/gpio/export", 200);
                ChkResponse(PortType.UART, ITEM.NONE, keyword, out getMsg, timeOutMs);
                SendCommand(PortType.UART, "echo 457 > /sys/class/gpio/export", 200);
                ChkResponse(PortType.UART, ITEM.NONE, keyword, out getMsg, timeOutMs);
                SendCommand(PortType.UART, "echo 456 > /sys/class/gpio/export", 200);
                ChkResponse(PortType.UART, ITEM.NONE, keyword, out getMsg, timeOutMs);
                SendCommand(PortType.UART, "echo 450 > /sys/class/gpio/export", 200);
                ChkResponse(PortType.UART, ITEM.NONE, keyword, out getMsg, timeOutMs);

                try
                {
                    getMsg = "";
                    SendCommand(PortType.UART, "cat /sys/class/gpio/gpio449/value", 200);
                    ChkResponse(PortType.UART, ITEM.NONE, keyword, out getMsg, timeOutMs);
                    if (getMsg.Contains("gpio449"))
                    {
                        binaryValue += getMsg.Split('\n')[1].Trim();
                    }
                    getMsg = "";
                    SendCommand(PortType.UART, "cat /sys/class/gpio/gpio457/value", 200);
                    ChkResponse(PortType.UART, ITEM.NONE, keyword, out getMsg, timeOutMs);
                    if (getMsg.Contains("gpio457"))
                    {
                        binaryValue += getMsg.Split('\n')[1].Trim();
                    }
                    getMsg = "";
                    SendCommand(PortType.UART, "cat /sys/class/gpio/gpio456/value", 200);
                    ChkResponse(PortType.UART, ITEM.NONE, keyword, out getMsg, timeOutMs);
                    if (getMsg.Contains("gpio456"))
                    {
                        binaryValue += getMsg.Split('\n')[1].Trim();
                    }
                    getMsg = "";
                    SendCommand(PortType.UART, "cat /sys/class/gpio/gpio450/value", 200);
                    ChkResponse(PortType.UART, ITEM.NONE, keyword, out getMsg, timeOutMs);
                    if (getMsg.Contains("gpio450"))
                    {
                        binaryValue += getMsg.Split('\n')[1].Trim();
                    }
                }
                catch (Exception ex)
                {
                    DisplayMsg(LogType.Exception, "Get HW version exception ==> " + ex.Message);
                    AddData(errorItem, 1);
                    return;
                }

                DisplayMsg(LogType.Log, "Binary HW Version:" + binaryValue);
                if (binaryValue.Length == 4 && string.Compare(binaryValue, HWversion) == 0)
                {
                    DisplayMsg(LogType.Log, "check hwver Pass");
                    AddData(errorItem, 0);
                    status_ATS.AddDataRaw("LCS5_HW_VER", binaryValue, binaryValue, "000000");
                }
                else
                {
                    DisplayMsg(LogType.Log, "check hwver fail");
                    AddData(errorItem, 1);
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                AddData(errorItem, 1);
            }
        }
        private bool Camera()
        {
            try
            {
                if (File.Exists("d:/getSMT"))
                    File.Delete("d:/getSMT");
                if (File.Exists("d:/OK"))
                    File.Delete("d:/OK");
                if (File.Exists(sExeDirectory + "\\cam_result.ini"))
                    File.Delete(sExeDirectory + "\\cam_result.ini");

                //TuanLV22001080 remove; follow by Jed reduce cycletime (date: 20230428) 2s->1s
                DisplayMsg(LogType.Log, "Delay 1s..");
                Thread.Sleep(1000);

                File.Create("d:/getSMT").Close();
                DateTime dt = DateTime.Now;
                while (!File.Exists("d:/OK"))
                {
                    if (dt.AddMinutes(1) < DateTime.Now)
                    {
                        return false;
                    }
                    Thread.Sleep(500);
                }
                status_ATS.AddLog("camera path：" + sExeDirectory);
                return true;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, @"CameraExcpetion " + ex.Message);
                warning = "Exception";
                return false;
            }
        }
        private bool CheckCameraResult(string item, string keyword)
        {
            bool rs = false;
            Directory.SetCurrentDirectory(sExeDirectory);
            string temp1 = WNC.API.Func.ReadINI("cam_result", "result", item, "");
            try
            {
                if (temp1.ToUpper().Contains(keyword.ToUpper()))
                {
                    rs = true;
                }
            }
            catch (Exception ex)
            {
                status_ATS.AddLog(ex.ToString());
                Directory.SetCurrentDirectory(Application.StartupPath);
                return false;
            }
            status_ATS.AddLog(String.Format("Camera item {0}, result:{1}, keyword:{2}", item, temp1, keyword));
            Directory.SetCurrentDirectory(Application.StartupPath);
            return rs;
        }
        private void StartupCamera()
        {
            string sReceive = string.Empty;
            Process[] ps;
            Net.NewNetPort newNetPort = new Net.NewNetPort();
            Directory.SetCurrentDirectory(sExeDirectory);
            if (!CheckTaskProcess("camera"))
            {
                try
                {
                    newNetPort.ExecuteDOSCommand(sExeDirectory + @".\camera.exe", "", false, ref sReceive, 10, true);
                    status_ATS.AddLog("Open camera path：" + sExeDirectory);
                }
                catch (Exception ex)
                {
                    Directory.SetCurrentDirectory(Application.StartupPath);
                    status_ATS.AddLog("Camera Open Exception:" + ex.Message);
                }
            }
            else
            {
                Thread.Sleep(500);
                status_ATS.AddLog("Camera is closing...");
                KillTaskProcess("camera");
                Thread.Sleep(1000);
                newNetPort.ExecuteDOSCommand(sExeDirectory + @".\camera.exe", "", false, ref sReceive, 10, true);
                status_ATS.AddLog("Camera is opening...");
            }

            ps = Process.GetProcesses();
            string sWindowName = "NONE";
            foreach (Process p in ps)
            {
                string s = p.ProcessName;
                if (s == "camera")
                {
                    sWindowName = p.MainWindowTitle;
                    //Thread.Sleep(3000);
                    if (!File.Exists("c:\\show"))
                    {
                        File.Create("c:\\show");
                    }
                    break;
                }
            }
            if ("NONE".Equals(sWindowName))
            {
                status_ATS.AddLog("Camera Open Failed!!!");
            }
            Directory.SetCurrentDirectory(Application.StartupPath);
        }
        private bool CheckTaskProcess(string sFileName)
        {
            bool bExist = false;
            try
            {
                Process[] localAll = Process.GetProcesses();
                foreach (Process i in localAll)
                {
                    if (sFileName.Equals(i.ProcessName))
                    {
                        bExist = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("KillTaskProcess Exception: " + ex.Message);
            }
            return bExist;
        }
        private void WriteHWver(DeviceInfor infor)
        {
            if (!CheckGoNoGo())
            {
                return;
            }
            string getMsg = string.Empty;
            string keyword = "root@OpenWrt";
            try
            {
                // After booting complete check keyword upon testplan 0828
                if (WriteOrCheckDeviceInfor(PortType.UART, "Write_HW_version", $"fw_setenv hwver {infor.HWver}", keyword))
                {
                    status_ATS.AddDataRaw("LCS5_HW_ver", infor.HWver, infor.HWver, "000000");
                    SendCommand(PortType.UART, "\r\n", 200);
                    ChkResponse(PortType.UART, ITEM.NONE, @"root@OpenWrt", out getMsg, 2000);
                    AddData("WriteHWver", 0);
                    return;
                }
                DisplayMsg(LogType.Error, @"Write Hw version NG FAILED");
                AddData("WriteHWver", 1);
                return;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, @"Write Hw version NG =>" + ex.Message);
                AddData("WriteHWver", 1);
                return;
            }
        }
        private bool HandlingTelnetPingErr()
        {
            DisplayMsg(LogType.Error, @" ---- handling ping via telnet err ----");
            bool IsPingOK = false;
            try
            {
                IsPingOK = SendAndChk("IsBootUP", PortType.UART, "dmesg | grep qca-wifi loaded", "qca-wifi loaded", 5000, 10 * 1000);
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, @"HandlingTelnetPingErr Exception=>" + ex.Message);
                AddData("HandlingTelnetPingErr", 1);
                return false;
            }
            return IsPingOK;
        }
    }
}
