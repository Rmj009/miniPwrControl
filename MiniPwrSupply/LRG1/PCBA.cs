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
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using WNC.API;
using NationalInstruments.VisaNS;
using System.Net.Security;
using System.Runtime.CompilerServices;

namespace ATS
{
    public partial class frmMain
    {
        DeviceInfor infor = new DeviceInfor();
        SFCS_Query _Sfcs_Query = new SFCS_Query();
        public class DeviceInfor
        {
            public string SerialNumber = "";
            public string HWver = "";
            public string DECT_rfpi = "";
            public string DECT_cal_rxtun = "";
            public string License_key = "";
            public string BaseMAC = "";
            public string WanMAC = "";
            public string BleMAC = "";
            public string WiFi_SSID = "";
            public string WiFi_PWD = "";
            public string Admin_PWD = "";
            public string FWver = "";
            public string HWID = "";
            public string NFC_UID = "";
            public string DECTver = "";
            public string FWver_Cust = "";
            public string HWver_Cust = "";
            public string WiFiMAC_2G = "";
            public string WiFiMAC_5G = "";
            public string WiFiMAC_6G = "";

            //Rena_20230803, add ble_ver and se_ver for BLE test
            public string BLEver = "";
            public string SEver = "";

            public void ResetParam()
            {
                SerialNumber = "";
                HWver = "";
                DECT_rfpi = "";
                DECT_cal_rxtun = "";
                License_key = "";
                BaseMAC = "";
                WanMAC = "";
                BleMAC = "";
                WiFi_SSID = "";
                WiFi_PWD = "";
                Admin_PWD = "";
                FWver = "";
                HWID = "";
                NFC_UID = "";
                DECTver = "";
                FWver_Cust = "";
                HWver_Cust = "";
                WiFiMAC_2G = "";
                WiFiMAC_5G = "";
                WiFiMAC_6G = "";
                //Rena_20230803, add ble_ver and se_ver for BLE test
                BLEver = "";
                SEver = "";
            }
        }
        private void PCBA()
        {
            if (useShield)
            {
                fixture.ControlIO(Fixture.FixtureIO.IO_6, CTRL.ON);
                fixture.ControlIO(Fixture.FixtureIO.IO_7, CTRL.ON);
                fixture.ControlIO(Fixture.FixtureIO.IO_8, CTRL.ON);
            }

            try
            {
                infor.ResetParam();
                Net.NewNetPort newNetPort = new Net.NewNetPort();
                #region create SMT file

                if (status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode)
                {
                    //SE_TODO: get infor from SFCS
                    SentPsnForGetMAC(status_ATS.txtPSN.Text.Trim());
                    for (int i = 0; i < 3; i++)
                    {
                        DisplayMsg(LogType.Log, "Delay 1s...");
                        Thread.Sleep(1000);

                        infor.SerialNumber = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@LRG1_SN");
                        infor.BaseMAC = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@MAC");
                        infor.FWver = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@MFG_FW_17");

                        infor.HWID = Func.ReadINI("Setting", "PCBA", "HWID", "1001");
                        infor.HWver = Func.ReadINI("Setting", "PCBA", "HWver", "EVT2");
                        //Rena_20230803, add ble_ver and se_ver for BLE test
                        infor.BLEver = Func.ReadINI("Setting", "PCBA", "BLEver", "v5.0.0-b108");
                        infor.SEver = Func.ReadINI("Setting", "PCBA", "SEver", "0001020E");
                        infor.BaseMAC = MACConvert(infor.BaseMAC);
                        infor.WanMAC = MACConvert(infor.BaseMAC, 1);
                        if (infor.SerialNumber != "")
                            break;
                    }

                    DisplayMsg(LogType.Log, $"Get SN From SFCS is: {infor.SerialNumber}");
                    DisplayMsg(LogType.Log, $"Get Base MAC From SFCS is: {infor.BaseMAC}");
                    DisplayMsg(LogType.Log, $"Get FWver From SFCS is: {infor.FWver}");
                    if (string.IsNullOrEmpty(infor.SerialNumber) || string.IsNullOrEmpty(infor.BaseMAC) || string.IsNullOrEmpty(infor.FWver)
                       || infor.SerialNumber.Contains("Dut not have") || infor.BaseMAC.Contains("Dut not have") || infor.FWver.Contains("Dut not have"))
                    {
                        warning = "Get from SFCS FAIL";
                        return;
                    }

                    string result = string.Empty;
                    result = infor.FWver.Substring(0, infor.FWver.Length - 9);

                    DisplayMsg(LogType.Log, $"Get FWver trim is: LRG1_{result}");
                    infor.FWver = "LRG1_" + result.ToLower();
                    infor.DECTver = Func.ReadINI("Setting", "PCBA", "DECTver", "Version 04.13 - Build 19");
                    DisplayMsg(LogType.Log, $"Get HWID From setting is: {infor.HWID}");
                    DisplayMsg(LogType.Log, $"Get HWver From setting is: {infor.HWver}");
                    DisplayMsg(LogType.Log, $"Get BaseMAC From SFCS is: {infor.BaseMAC}");
                    DisplayMsg(LogType.Log, $"Get WanMAC From SFCS is: {infor.WanMAC}");

                    DisplayMsg(LogType.Log, $"Get BLEver From setting is: {infor.BLEver}");
                    DisplayMsg(LogType.Log, $"Get SEver From setting is: {infor.SEver}");
                    if (infor.SerialNumber.Length == 18)
                    {
                        SetTextBox(status_ATS.txtPSN, infor.SerialNumber);
                        //SetTextBox(status_ATS.txtSP, infor.BaseMAC);
                        status_ATS.SFCS_Data.PSN = infor.SerialNumber;
                        status_ATS.SFCS_Data.First_Line = infor.SerialNumber + "," + status_ATS.txtSP.Text;
                    }
                    else
                    {
                        warning = "Get SN from SFCS fail";
                        return;
                    }


                    GetRFPIFromExcel(infor.BaseMAC);
                }
                else
                {
                    //DisplayMsg(LogType.Log, $"Test In engineer mode");
                    //infor.SerialNumber = string.Empty;
                    //infor.SerialNumber = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@LRG1_SN");
                    //DisplayMsg(LogType.Log, $"Get SN From SFCS is: {infor.SerialNumber}");
                    //if (infor.SerialNumber.Length == 18)
                    //{
                    //    /*SetTextBox(status_ATS.txtPSN, infor.SerialNumber);
                    //    //SetTextBox(status_ATS.txtSP, infor.BaseMAC);
                    //    status_ATS.SFCS_Data.PSN = infor.SerialNumber;
                    //    status_ATS.SFCS_Data.First_Line = infor.SerialNumber + "," + status_ATS.txtSP.Text;*/
                    //}
                    //else // if cannot get from sfcs will get from setting/ jason add 2023/09/27
                    //{
                    //    infor.SerialNumber = Func.ReadINI("Setting", "PCBA", "LRG1_SN_Sample", "");
                    //    DisplayMsg(LogType.Log, $"Get SN 'LRG1_SN_Sample=' From Setting  is: {infor.SerialNumber}");
                    //    if (string.IsNullOrEmpty(infor.SerialNumber) || infor.SerialNumber.Length != 18)
                    //    {
                    //        warning = "Get SN sample from setting fail";
                    //        return;
                    //    }

                    //    /* SetTextBox(status_ATS.txtPSN, infor.SerialNumber);
                    //     //SetTextBox(status_ATS.txtSP, infor.BaseMAC);
                    //     status_ATS.SFCS_Data.PSN = infor.SerialNumber;
                    //     status_ATS.SFCS_Data.First_Line = infor.SerialNumber + "," + status_ATS.txtSP.Text;*/

                    //}

                    //infor.BaseMAC = string.Empty;
                    //if (forHQtest)
                    //{
                    //    infor.BaseMAC = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@MAC");
                    //    DisplayMsg(LogType.Log, $"Get MAC From SFCS is: {infor.BaseMAC}");
                    //}
                    //if (infor.BaseMAC.Length == 12)
                    //{
                    //    DisplayMsg(LogType.Log, $"Get MAC From SFCS OK");
                    //}
                    //else // if cannot get from sfcs will get from setting/ jason add 2023/09/27
                    //{
                    //    infor.BaseMAC = Func.ReadINI("Setting", "PCBA", "MAC_Sample", "");
                    //    DisplayMsg(LogType.Log, $"Get SN 'MAC_Sample=' From Setting  is: {infor.BaseMAC}");
                    //    if (string.IsNullOrEmpty(infor.BaseMAC) || infor.BaseMAC.Length != 12)
                    //    {
                    //        warning = "Get MAC sample from setting fail";
                    //        return;
                    //    }
                    //}

                    //infor.BaseMAC = MACConvert(infor.BaseMAC);

                    //SetTextBox(status_ATS.txtPSN, infor.SerialNumber);
                    ////SetTextBox(status_ATS.txtSP, infor.BaseMAC);
                    //status_ATS.SFCS_Data.PSN = infor.SerialNumber;
                    //status_ATS.SFCS_Data.First_Line = infor.SerialNumber + "," + status_ATS.txtSP.Text;

                    //Rena_20230407 add for HQ test
                    GetBoardDataFromExcel(status_ATS.txtPSN.Text);
                    //Rena_20230803, add ble_ver and se_ver for BLE test
                    infor.BLEver = Func.ReadINI("Setting", "PCBA", "BLEver", "v5.0.0-b108");
                    infor.SEver = Func.ReadINI("Setting", "PCBA", "SEver", "0001020E");

                    infor.FWver = Func.ReadINI("Setting", "PCBA", "FWver", "v0.0.4.1");
                    infor.HWID = Func.ReadINI("Setting", "PCBA", "HWID", "1001");
                    infor.HWver = Func.ReadINI("Setting", "PCBA", "HWver", "EVT2");
                    infor.DECT_cal_rxtun = Func.ReadINI("Setting", "PCBA", "DECT_RXTUN", "");
                    infor.DECTver = Func.ReadINI("Setting", "PCBA", "DECTver", "Version 04.13 - Build 19");
                    infor.BaseMAC = MACConvert(infor.BaseMAC);
                    infor.WanMAC = MACConvert(infor.BaseMAC, 1);
                }

                if (status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode)
                {
                    if (!ChkStation(status_ATS.txtPSN.Text)) { return; }
                }

                #endregion

                if (Func.ReadINI("Setting", "IO_Board_Control", "IO_Control_1", "0") == "1")
                {
                    string txPin = Func.ReadINI("Setting", "IO_Board_Control", "Pin0", "0");
                    string rev_message = "";
                    status_ATS.AddLog("IO_Board_Y" + txPin + " On...");
                    IO_Board_Control1.ConTrolIOPort_write(Int32.Parse(txPin), "1", ref rev_message);
                    DisplayMsg(LogType.Log, rev_message);
                }
                else if (Func.ReadINI("Setting", "Port", "RelayBoard", "Disable").ToUpper() == "ENABLE")
                {
                    SwitchRelay(CTRL.ON);
                    Thread.Sleep(3000);
                    SwitchRelay(CTRL.OFF);
                }
                else
                {
                    frmOK.Label = "Xác nhận đã kết nối 'USB3.0' và 'điện thoại SLIC', 'dây mạng' đã kết nối vào cổng LAN màu vàng port1,\r\nVui lòng bật nguồn và nhấn nút nguồn để khởi động";
                    frmOK.ShowDialog();
                }
                DisplayMsg(LogType.Log, "Power on!!!");

                ChkBootUp(PortType.SSH);

                //if (status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode && !isGolden)
                {
                    CheckFWVerAndHWID();
                }


                if (Func.ReadINI("Setting", "PCBA", "SkipNFC", "0") == "0")
                {
                    NFCTag();
                }

                if (Func.ReadINI("Setting", "PCBA", "SkipDECT", "0") == "0")
                {
                    string rxtun = "";
                    Set_DECT_Full_Power();
                    this.DECTCal(ref rxtun);
                    infor.DECT_cal_rxtun = rxtun;
                    DisplayMsg(LogType.Log, "DECT_cal_rxtun: " + infor.DECT_cal_rxtun);
                }
                SLICTest_ByUsbModem();

                //if (forHQtest || (status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode && !isGolden))
                {
                    SetDUTInfo();
                    CheckDUTInfo();
                }

                if (isLoop == 0)
                    CheckLED();

                CheckPCIe();

                if (isLoop == 0)
                    WPSButton();

                if (isLoop == 0)
                    ResetButton();

                USBTest();
                //=================================
                this.USB30(); // testplan 5.3.9
                //=================================
                if (isLoop == 0)
                {
                    this.EthernetTest(1);
                    this.ChkMacAddr();
                }

                CurrentSensor();

                BleTest();

                if (forHQtest)
                {
                    WriteBoardDataToExcel(status_ATS.txtPSN.Text);
                }

            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                warning = "Exception";
            }
            finally
            {
                if (status_ATS._testMode == StatusUI2.StatusUI.TestMode.EngMode && isLoop == 0)
                {
                    if (!CheckGoNoGo())
                    {
                        frmOK.Label = "Test FAIL/ Gọi PE Kiểm Tra!!!!!";
                        frmOK.ShowDialog();
                    }
                    else
                    {
                        frmOK.Label = "Test PASS/ Lấy sản Phẩm ra";
                        frmOK.ShowDialog();
                    }
                }

                if (Func.ReadINI("Setting", "IO_Board_Control", "IO_Control_1", "0") == "1")
                {
                    string txPin = Func.ReadINI("Setting", "IO_Board_Control", "Pin0", "0");
                    string rev_message = "";
                    status_ATS.AddLog("IO_Board_Y" + txPin + " Off...");
                    IO_Board_Control1.ConTrolIOPort_write(Int32.Parse(txPin), "2", ref rev_message);
                    DisplayMsg(LogType.Log, rev_message);
                }
                else SwitchRelay(CTRL.ON);
            }
        }
        //Rena_20230803, EEProm Param Set RFPI
        private void Set_DECT_RFPI()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            DisplayMsg(LogType.Log, "=============== Set DECT RFPI ===============");
            string item = "SetDECTRFPI";
            string res = string.Empty;
            string RFPI_val = infor.DECT_rfpi.Replace(".", "").ToUpper(); //寫入格式為 0303B009B0

            try
            {
                bool result = false;
                int delayMs = 0;
                int timeOutMs = 10 * 1000;

                //check RFPI value
                if (RFPI_val.Length != 10)
                {
                    DisplayMsg(LogType.Log, $"RFPI '{RFPI_val}' format is wrong");
                    AddData(item, 1);
                    return;
                }

                //for (int i = 0; i < 3; i++)
                //{
                //    SendCommand(PortType.SSH, "cmbs_tcx -comname ttyMSM2 -baud 460800", delayMs);
                //    if (result = ChkResponse(PortType.SSH, ITEM.NONE, "Choose", out res, 3000))
                //        break;
                //    DisplayMsg(LogType.Log, "Delay 2s...");
                //    Thread.Sleep(2000);
                //}

                //if (!result)
                //{
                //    DisplayMsg(LogType.Log, "Enter DECT MENU fail");
                //    AddData(item, 1);
                //    return;
                //}

                //s -> 2 -> 1 -> {RFPI value (5bytes)}
                SendWithoutEnterAndChk(PortType.SSH, "s", "q => Return to Interface Menu", delayMs, timeOutMs);
                SendWithoutEnterAndChk(PortType.SSH, "2", "q => Return", delayMs, timeOutMs);
                if (!SendWithoutEnterAndChk(PortType.SSH, "1", "Current RFPI:", delayMs, timeOutMs))
                {
                    DisplayMsg(LogType.Log, "Set DECT RFPI fail");
                    AddData(item, 1);
                    return;
                }

                DisplayMsg(LogType.Log, $"Write '{RFPI_val}' to ssh");
                SSH_stream.WriteLine(RFPI_val);
                if (!ChkResponse(PortType.SSH, ITEM.NONE, "Response: OK", out res, timeOutMs))
                {
                    DisplayMsg(LogType.Log, "Set DECT RFPI fail");
                    AddData(item, 1);
                    return;
                }

                //RFPI    :     03     03     B0     09     B2
                result = false;
                string[] lines = res.Split('\n');
                foreach (string line in lines)
                {
                    if (line.Contains("RFPI") && line.Contains(":") && line.Contains(RFPI_val.Substring(0, 2)))
                    {
                        //DisplayMsg(LogType.Log, $"Found RFPI value '{line}'");
                        if (line.Replace(" ", "").Contains($"RFPI:{RFPI_val}"))
                        {
                            DisplayMsg(LogType.Log, $"Check 'RFPI:{RFPI_val}' pass");
                            result = true;
                            AddData(item, 0);
                            return;
                        }
                    }
                }
                if (!result)
                {
                    DisplayMsg(LogType.Log, $"Check 'RFPI:{RFPI_val}' fail");
                    AddData(item, 1);
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData(item, 1);
            }
        }

        private bool IsTftpd32Running()
        {
            Process[] processes = Process.GetProcessesByName("tftpd32");
            return processes.Length > 0;
        }
        private bool OpenTftpd32New(string path, int timeoutMilliseconds)
        {
            try
            {
                KillTaskProcess("tftpd32");
                Thread.Sleep(1000);

                if (Directory.Exists(path))
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = Path.Combine(path, "tftpd32.exe"),
                        WorkingDirectory = path
                    };

                    Process.Start(startInfo);

                    DisplayMsg(LogType.Log, $"Start {Path.Combine(path, "tftpd32.exe")}");

                    DateTime startTime = DateTime.Now;

                    while (!IsTftpd32Running())
                    {
                        Thread.Sleep(1000);

                        if ((DateTime.Now - startTime).TotalMilliseconds > timeoutMilliseconds)
                        {
                            DisplayMsg(LogType.Exception, "tftpd32 failed to start within the timeout period.");
                            return false;
                        }
                    }

                    DisplayMsg(LogType.Log, "tftpd32 is running.");
                    return true;
                }
                else
                {
                    DisplayMsg(LogType.Exception, $"Directory does not exist pls check TFTP tool setting: {path}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                return false;
            }
        }
        private void OpenTftpd32(string path)
        {
            try
            {
                KillTaskProcess("tftpd32");
                Thread.Sleep(1000);
                Directory.SetCurrentDirectory(path);
                Process.Start(Path.Combine(path, "tftpd32.exe"));
                DisplayMsg(LogType.Log, $"Start {path}\\tftpd32.exe");
                Directory.SetCurrentDirectory(Application.StartupPath);
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
            }
        }
        private bool BootLoader(SerialPort port, string keyword1 = "stop autoboot", string keyword2 = "IPQ9574#", int timeOutMs = 100000)
        {
            DateTime dt;
            TimeSpan ts;

            string res = string.Empty;
            string log = string.Empty;

            dt = DateTime.Now;
            log = string.Empty;
            try
            {
                bool first = true;

                while (true)
                {
                    if (!port.IsOpen)
                    {
                        DisplayMsg(LogType.Log, "Open port:" + port.PortName);
                        port.Open();
                        Thread.Sleep(100);
                    }

                    ts = new TimeSpan(DateTime.Now.Ticks - dt.Ticks);

                    if (ts.TotalMilliseconds > timeOutMs)
                    {
                        DisplayMsg(LogType.Error, "Check timeout");
                        return false;
                    }

                    res = port.ReadExisting();

                    if (res.Length != 0 && res != "\r\n")
                    {
                        DisplayMsg(LogType.Log, res);
                        log += res;
                    }

                    if (log.Contains(keyword1) && first)
                    {
                        Thread.Sleep(200);
                        DisplayMsg(LogType.Log, "Sent 'Enter'");
                        port.Write("\r\n");
                        first = false;
                    }
                    if (log.Contains(keyword2))
                    {
                        DisplayMsg(LogType.Log, "Check '" + keyword2 + "' ok");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                warning = "Exception";
                return false;
            }
        }
        private void ChkBootUp(PortType portType)
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            DisplayMsg(LogType.Log, "=============== Check BootUp ===============");
            string keyword = @"root@";
            string item = "BootUp";
            try
            {
                if (!ChkInitial(portType, keyword, 200000))
                {
                    //check again
                    if (SendAndChk(portType, "", keyword, 0, 3000))
                    {
                        AddData(item, 0);
                        return;
                    }

                    AddData(item, 1);
                    return;
                }

                AddData(item, 0);
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData(item, 1);
            }
        }
        private void Set_DECT_Full_Power()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            DisplayMsg(LogType.Log, "=============== Set DECT Full Power ===============");
            // s -> 2 -> j  \n
            // FE
            string offset_val = Func.ReadINI("Setting", "PCBA", "DECT_Default_RXTUN", "70");   //RxTun
            string keyword = "root@OpenWrt:~# \r\n";
            string item = "SetDECTFullPower";
            string Full_power_value = "FE";
            string DECTver = string.Empty;
            string res = string.Empty;
            int timeOutMs = 10 * 1000;
            int delayMs = 0;
            bool result = false;
            try
            {
                for (int i = 0; i < 3; i++)
                {
                    SendCommand(PortType.SSH, "cmbs_tcx -comname ttyMSM2 -baud 460800", delayMs);
                    if (result = ChkResponse(PortType.SSH, ITEM.NONE, "Choose", out res, 3000))
                        break;
                    DisplayMsg(LogType.Log, "Delay 2s...");
                    Thread.Sleep(2000);
                }

                if (!result)
                {
                    DisplayMsg(LogType.Log, "Enter DECT MENU fail");
                    AddData(item, 1);
                    return;
                }
                //check DECT version
                #region check_DECT_version
                Match m = Regex.Match(res, @"Target\s+: (?<DECT_ver>.+)");
                if (m.Success)
                {
                    DECTver = m.Groups["DECT_ver"].Value.Trim();
                }
                DisplayMsg(LogType.Log, "Current DECT_version:" + DECTver);
                DisplayMsg(LogType.Log, "SFCS DECT_version:" + infor.DECTver);
                if (DECTver != "" && string.Compare(DECTver, infor.DECTver, true) == 0)
                {
                    status_ATS.AddDataRaw("LRG1_DECT_Ver", infor.DECTver, infor.DECTver, "000000");
                    AddData("DECT_Ver", 0);
                }
                else
                {
                    AddData("DECT_Ver", 1);
                    DisplayMsg(LogType.Log, "Check DECT version fail");
                    return;
                }
                #endregion
                //========================================================================================
                this.Set_DECT_ID_AND_DECT_RFPI();
                //========================================================================================
                SendWithoutEnterAndChk(item, PortType.SSH, "s", "q => Return to Interface Menu", delayMs, timeOutMs);
                result = SendWithoutEnterAndChk(PortType.SSH, "2", "q => Return", out res, delayMs, timeOutMs);
                if (!result || !res.Contains("j => Full Power"))
                {
                    DisplayMsg(LogType.Log, "Enter 'EEProm Param Set' fail");
                    AddData(item, 1);
                    return;
                }

                DisplayMsg(LogType.Log, "Write j to ssh");
                SSH_stream.Write("j");
                Thread.Sleep(200);
                result = SendAndChk(PortType.SSH, $"{Full_power_value}", "q => Return to Interface Menu", out res, delayMs, timeOutMs);
                if (!result || !Regex.IsMatch(res, $"RF FULL POWER\\s+:\\s+{Full_power_value}"))
                {
                    DisplayMsg(LogType.Log, "Set FULL POWER fail");
                    AddData(item, 1);
                    return;
                }
                else
                {
                    DisplayMsg(LogType.Log, "Set FULL POWER pass");
                    AddData(item, 0);
                }

                // testplan revised 
                // ===============================
                // into EEProm Param Set
                result = SendWithoutEnterAndChk(PortType.SSH, "2", "q => Return", out res, delayMs, timeOutMs);
                SSH_stream.Write("r");
                // sent A0 之後跳出去
                //SSH_stream.WriteLine("A0");
                SendWithoutEnterAndChk(PortType.SSH, "A0\n", "q => Return", out res, delayMs, timeOutMs);
                if (!res.Contains("A0"))
                {
                    DisplayMsg(LogType.Log, "Set full power fail");
                    AddData(item, 1);
                }
                // q
                //exit calibration mode
                this.exitMode();
                #region set_RXTUN_default
                //x -> x -> 26 -> 1 -> 70
                //q (回到主選單)
                //繼續開始calibration流程
                if (true)
                {
                    DisplayMsg(LogType.Log, "Write x to ssh");
                    SSH_stream.Write("x\r");
                    ChkResponse(PortType.SSH, ITEM.NONE, "q) Quit", out res, timeOutMs);

                    if (!SendWithoutEnterAndChk(PortType.SSH, "x", "Enter Location (dec):", delayMs, timeOutMs))
                    {
                        DisplayMsg(LogType.Log, "Modify RXTUN fail");
                        AddData(item, 1);
                        return;
                    }

                    DisplayMsg(LogType.Log, $"Write '26' to ssh");
                    SSH_stream.WriteLine("26");
                    if (!ChkResponse(PortType.SSH, ITEM.NONE, "Enter Length (dec. max 512):", out res, timeOutMs))
                    {
                        DisplayMsg(LogType.Log, "Modify RXTUN fail");
                        AddData(item, 1);
                        return;
                    }

                    DisplayMsg(LogType.Log, $"Write '1' to ssh");
                    SSH_stream.WriteLine("1");
                    if (!ChkResponse(PortType.SSH, ITEM.NONE, "Enter New Data (hexadecimal):", out res, timeOutMs))
                    {
                        DisplayMsg(LogType.Log, "Modify RXTUN fail");
                        AddData(item, 1);
                        return;
                    }

                    DisplayMsg(LogType.Log, $"Write '{offset_val}' to ssh");
                    SSH_stream.WriteLine(offset_val);
                    if (!ChkResponse(PortType.SSH, ITEM.NONE, "CURRENT VALUE:", out res, timeOutMs))
                    {
                        DisplayMsg(LogType.Log, "Modify RXTUN fail");
                        AddData(item, 1);
                        return;
                    }
                    if (!SendWithoutEnterAndChk(PortType.SSH, "q", "q) Quit", delayMs, timeOutMs))
                    {
                        DisplayMsg(LogType.Log, "Modify RXTUN fail");
                        AddData(item, 1);
                        return;
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData(item, 1);
            }
            finally
            {
                //// q to layer 1 for Reboot DECT
                this.ResetDect();
            }
        }
        private void DECTCal(ref string RXTUNE)
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            DisplayMsg(LogType.Log, "=============== DECT Calibration ===============");
            string item = "DECTCal";
            string res = string.Empty;
            string keyword = "root@OpenWrt:~#";
            string offset_val = Func.ReadINI("Setting", "PCBA", "DECT_Default_RXTUN", "70");   //RxTun
            try
            {
                byte[] data = new byte[] { };
                string cmd = string.Empty;
                //string version = Func.ReadINI("Setting", "Parameter", "DECT_Version", "ERROR");
                //double freqHz = 13824000;  //Freq. = 13.824 MHz
                double freqHz = 1888356500; //1888.3565 MHz
                double pwrThreshold = Convert.ToInt32(Func.ReadINI("Setting", "DECT_SignalAnalyzer", "PwrThreshold", "0"));
                double outFreqHz = 0;
                double tolerance = 1500;
                double freqDeltaHz = 0;
                double pwr = -999;
                bool result = false;
                bool isRxtunOK = false;
                int delayMs = 0;
                int timeOutMs = 10 * 1000;
                int retryTimes = 1;
                string RxRes = string.Empty;
                DateTime dt;
                TimeSpan ts;

                if (!DECTSignalAnalyzerPresetting(1888704000))
                {
                    warning = "Initial Spectrum fail";
                    return;
                }
            RetryPwr:
                for (int i = 0; i < 3; i++)
                {
                    SendCommand(PortType.SSH, "cmbs_tcx -comname ttyMSM2 -baud 460800", delayMs);
                    if (result = ChkResponse(PortType.SSH, ITEM.NONE, "Choose", out res, 3000))
                        break;
                    DisplayMsg(LogType.Log, "Delay 2s...");
                    Thread.Sleep(2000);
                }

                if (!result)
                {
                    DisplayMsg(LogType.Log, "Enter DECT MENU fail");
                    AddData(item, 1);
                    return;
                }
                //Rena_20230524, change RXTUN default value as 70
                DisplayMsg(LogType.Log, "Write x to ssh");
                SSH_stream.Write("x\r");
                ChkResponse(PortType.SSH, ITEM.NONE, "q) Quit", out RxRes, timeOutMs);
                //Rena_20230524, check RXTUN default value 
                #region check_RXTUN_default
                if (retryTimes < 2)
                {
                    //check RXTUN default value
                    RXTUNE = ParseRXTUNE(RxRes).Trim();
                    if (RXTUNE != offset_val)
                    {
                        DisplayMsg(LogType.Log, $"Check RXTUN={offset_val} fail");
                        AddData(item, 1);
                        return;
                    }
                    else
                    {
                        DisplayMsg(LogType.Log, $"Check RXTUN={offset_val} pass");
                    }
                }
                #endregion
                // jump into CALIBRATION MENU
                //s -> ff -> 0 -> 3(continuous TX) -> 0 -> 05(CH5) -> 0 (ANT0) -> q
                SendWithoutEnterAndChk(item, PortType.SSH, "s", "FF for default", delayMs, timeOutMs);
                SendWithoutEnterAndChk(item, PortType.SSH, "ff", "2 - long slot", delayMs, timeOutMs);
                SendWithoutEnterAndChk(item, PortType.SSH, "0", "3 - continuous TX", delayMs, timeOutMs);
                SendWithoutEnterAndChk(item, PortType.SSH, "3", "Enter Instance (0..9):", delayMs, timeOutMs);
                SendWithoutEnterAndChk(item, PortType.SSH, "0", "DECT (decimal):", delayMs, timeOutMs);
                SendWithoutEnterAndChk(item, PortType.SSH, "05", "Enter Ant (0,1):", delayMs, timeOutMs);
                SendWithoutEnterAndChk(item, PortType.SSH, "0", "Press any key !", delayMs, timeOutMs);
                SendWithoutEnterAndChk(item, PortType.SSH, "q", "q) Quit", delayMs, timeOutMs);
                if (!CheckGoNoGo())
                {
                    DisplayMsg(LogType.Log, "continuous TX fail");
                    AddData(item, 1);
                    return;
                }

                if (!SendWithoutEnterAndChk(PortType.SSH, "c", "(0xFF for None):", delayMs, timeOutMs))
                {
                    DisplayMsg(LogType.Log, "Enter RXTUN fail");
                    AddData(item, 1);
                    return;
                }

                DisplayMsg(LogType.Log, "Write 07 to ssh");
                SSH_stream.Write("07\r");

                result = ChkResponse(PortType.SSH, ITEM.NONE, "RXTUN:", out res, timeOutMs);
                //RXTUNE = ParseRXTUNE(res).Trim();
                if (ParseRXTUNE(res).Trim().Length == 0)
                {
                    warning = "RXTUN is empty";
                    return;
                }

                dt = DateTime.Now;
                while (true)
                {
                    ts = new TimeSpan(DateTime.Now.Ticks - dt.Ticks);
                    if (ts.TotalMilliseconds > 240 * 1000)
                    {
                        DisplayMsg(LogType.Error, "DECT Calibration timeout");
                        AddData(item, 1);
                        return;
                    }

                    DisplayMsg(LogType.Log, "Delay 500 (ms)..");
                    System.Threading.Thread.Sleep(500);

                    if (!FetchFrequency(out outFreqHz))
                    {
                        AddData(item, 1);
                        return;
                    }
                    freqDeltaHz = outFreqHz - freqHz;
                    DisplayMsg(LogType.Log, "freqDeltaHz: " + freqDeltaHz.ToString());
                    DisplayMsg(LogType.Log, "freqHz: " + outFreqHz.ToString());
                    DisplayMsg(LogType.Log, "Tolerence: " + tolerance.ToString());

                    //Rena_20230414, disable for LRG1 HQ sample build
                    int delay = Convert.ToInt32(Func.ReadINI("Setting", "DECT_SignalAnalyzer", "DelayPower", "50"));
                    FetchPower(out pwr, delay);
                    DisplayMsg(LogType.Log, "Dect_TxPower: " + pwr.ToString());
                    if (pwr < pwrThreshold) // need to check env in V2 to set threshold
                    {
                        retryTimes++;
                        DisplayMsg(LogType.Warning, "Under power threshold : " + pwr.ToString());
                        if (retryTimes > 3)
                        {
                            warning = $"Power under threahold failed retry 3 time";
                            isRxtunOK = false;
                            return;
                        }
                        else
                        {
                            DisplayMsg(LogType.Log, $"TxPower {pwr.ToString()} Too low, try again");
                            this.ResetDect();
                            goto RetryPwr;
                        }
                        //continue;
                    }
                    //==============================================================================================
                    if (freqDeltaHz > tolerance)
                    {
                        cmd = ">";
                        data = new byte[] { 0x3e };
                    }
                    else if (freqDeltaHz < -tolerance)
                    {
                        cmd = "<";
                        data = new byte[] { 0x3c };
                    }
                    else
                    {
                        DisplayMsg(LogType.Log, " ========= Get last rxtun ========");
                        cmd = "q";
                        DisplayMsg(LogType.Log, $"Write '{cmd}' to ssh");
                        SSH_stream.Write(cmd);
                        ChkResponse(PortType.SSH, ITEM.NONE, "RXTUN: ", out res, timeOutMs);

                        RXTUNE = ParseRXTUNE(res, "RXTUN:").Trim();
                        if (RXTUNE.Length == 0)
                        {
                            warning = "RXTUN is empty";
                            return;
                        }

                        DisplayMsg(LogType.Log, "---DECT RXTUN-------------------> " + RXTUNE);
                        status_ATS.AddDataRaw("LGR1_RXTURN", RXTUNE, RXTUNE, "000000");

                        status_ATS.AddData("CrystalFrequencyHz", "Hz", outFreqHz);
                        AddData(item, 0);

                        //Rena_20230714 modify
                        //status_ATS.AddDataRaw("LRG1_DECT_RXTUN", RXTUNE, RXTUNE, "000000");
                        int decRxtune = int.Parse(RXTUNE, System.Globalization.NumberStyles.HexNumber);
                        status_ATS.AddData("DECT_RXTUNE", "", decRxtune);
                        break;
                    }
                    //Rena_20230707,LRG1 DECT會先寫預設值, 所以調整flow
                    //double delta = freqDeltaHz / 6;
                    //delta = Math.Round(delta, 0);
                    //DisplayMsg(LogType.Log, "Shift " + delta.ToString() + " times..");
                    //delta = Math.Abs(delta);
                    DisplayMsg(LogType.Log, $"Write '{cmd}' to ssh");
                    SSH_stream.Write(cmd);
                    Thread.Sleep(100);
                    if (!ChkResponse(PortType.SSH, ITEM.NONE, "RXTUN: ", out res, timeOutMs))
                    {
                        DisplayMsg(LogType.Log, $"cannot_acquire_Rxtun {res}");
                        AddData(item, 1);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData(item, 1);
            }
            finally
            {
                //exit calibration mode
                //exitMode(keyword);
                SendAndChk(PortType.SSH, "qqqqq\r\n", keyword, out res, 0, 1200);
                //SendAndChk(PortType.SSH, "cd ~", keyword, out res, 0, 3000);
            }
        }
        public void exitMode()
        {
            string res = string.Empty;
            bool result = false;
            string keyWord = "root@OpenWrt";
            do
            {
                if (SendAndChk(PortType.SSH, "qqqqq\n", keyWord, out res, 0, 1000))
                {
                    { break; }
                }
            } while (!SendAndChk(PortType.SSH, "\r\n", keyWord, out res, 0, 100));
            //for (int i = 0; i < 5; i++)
            //{
            //if (SendAndChk(PortType.SSH, "q\r\n", keyWord, out res, 0, 12000))
            //{ break; }
            //SendAndChk(PortType.SSH, "qq\r\n", keyWord, out res, 0, 10000);
            //if (SendAndChk(PortType.SSH, "\r\n", keyWord, out res, 0, 3000))
            //{ break; }
            //}
            try
            {
                for (int i = 0; i < 3; i++)
                {
                    SendCommand(PortType.SSH, "cmbs_tcx -comname ttyMSM2 -baud 460800", 0);
                    if (result = ChkResponse(PortType.SSH, ITEM.NONE, "Choose", out res, 3000))
                        break;
                    DisplayMsg(LogType.Log, "Delay 2s...");
                    Thread.Sleep(2000);
                }

                if (!result)
                {
                    DisplayMsg(LogType.Log, "Enter DECT MENU fail");
                    AddData("exitMode", 1);
                    return;
                }
            }
            catch (Exception ex)
            {

                DisplayMsg(LogType.Exception, ex.Message);
                return;
            }
        }
        private void Set_DECT_ID_AND_DECT_RFPI()
        {
            if (!CheckGoNoGo())
            {
                return;
            }
            DisplayMsg(LogType.Log, "=============== Set DECT ID ===============");
            string item = "SetDECTID";
            string res = string.Empty;
            try
            {
                //bool result = false;
                int delayMs = 0;
                int timeOutMs = 10 * 1000;
                DisplayMsg(LogType.Log, "Write x to ssh");
                SSH_stream.Write("x\r");
                ChkResponse(PortType.SSH, ITEM.NONE, "q) Quit", out res, timeOutMs);

                if (!SendWithoutEnterAndChk(PortType.SSH, "x", "Enter Location (dec):", delayMs, timeOutMs))
                {
                    DisplayMsg(LogType.Log, "Modify DECT EEPROM fail");
                    AddData(item, 1);
                    return;
                }

                DisplayMsg(LogType.Log, $"Write '21' to ssh");
                SSH_stream.WriteLine("21");
                if (!ChkResponse(PortType.SSH, ITEM.NONE, "Enter Length (dec. max 512):", out res, timeOutMs))
                {
                    DisplayMsg(LogType.Log, "Modify DECT EEPROM fail");
                    AddData(item, 1);
                    return;
                }

                DisplayMsg(LogType.Log, $"Write '3' to ssh");
                SSH_stream.WriteLine("3");
                if (!ChkResponse(PortType.SSH, ITEM.NONE, "Enter New Data (hexadecimal):", out res, timeOutMs))
                {
                    DisplayMsg(LogType.Log, "Modify DECT EEPROM fail");
                    AddData(item, 1);
                    return;
                }

                DisplayMsg(LogType.Log, "Write 0feb09 to ssh");
                SSH_stream.WriteLine("0feb09");
                if (!ChkResponse(PortType.SSH, ITEM.NONE, "CURRENT VALUE:", out res, timeOutMs))
                {
                    DisplayMsg(LogType.Log, "Modify DECT EEPROM fail");
                    AddData(item, 1);
                    return;
                }
                if (!res.Contains("0f eb 09"))
                {
                    DisplayMsg(LogType.Log, "Check 'CURRENT VALUE: 0f eb 09' fail");
                    AddData(item, 1);
                }
                else
                {
                    DisplayMsg(LogType.Log, "Check 'CURRENT VALUE: 0f eb 09' pass");
                    AddData(item, 0);
                }
                // ===============================================================
                // ==================== BACK MAIN MENU ===========================
                // ===============================================================
                this.exitMode();
                //=================================================================
                this.Set_DECT_RFPI(); //Rena_20230803, EEProm Param Set RFPI
                //=================================================================
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData(item, 1);
            }
            finally
            {
                //exit calibration mode
                this.exitMode();
            }
        }
        private bool UpgradeDECTFW()
        {
            if (!CheckGoNoGo() || isGolden)
            {
                return false;
            }

            string FW_image = Func.ReadINI("Setting", "PCBA", "DECTFWimage", ""); //DCX81_MOD_UART.bin
            string PC_IP = Func.ReadINI("Setting", "PCBA", "PC_IP", "192.168.1.2");
            string item = "UpgradeDECTFW";
            string keyword = @"root@OpenWrt";
            string res = "";
            string DECTver = "";
            bool result = false;

            try
            {
                DisplayMsg(LogType.Log, "=============== Upgrade DECT FW ===============");

                OpenTftpd32(Application.StartupPath);

                DisplayMsg(LogType.Log, "Check FW image: " + Path.Combine(Application.StartupPath, FW_image));
                if (!File.Exists(Path.Combine(Application.StartupPath, FW_image)))
                {
                    DisplayMsg(LogType.Log, "FW image doesn't exist");
                    AddData(item, 1);
                    return false;
                }

                //exit calibration mode
                for (int i = 0; i < 5; i++)
                {
                    if (SendAndChk(PortType.SSH, "q", keyword, out res, 0, 2000))
                        break;
                }

                //Upload image to DUT
                SendAndChk(PortType.SSH, "cd /tmp", "root@OpenWrt:/tmp#", out res, 0, 3000);
                SendAndChk(PortType.SSH, $"tftp -gr {FW_image} {PC_IP}", "root@OpenWrt:/tmp# \r\n", out res, 0, 5000);
                SendAndChk(PortType.SSH, "ls", "root@OpenWrt:/tmp#", out res, 0, 3000);
                if (!res.Contains(FW_image))
                {
                    DisplayMsg(LogType.Log, "Upload image to DUT fail");
                    AddData(item, 1);
                    return false;
                }

                //Enter DECT MENU
                for (int i = 0; i < 3; i++)
                {
                    SendCommand(PortType.SSH, "cmbs_tcx -comname ttyMSM2 -baud 460800", 0);
                    if (result = ChkResponse(PortType.SSH, ITEM.NONE, "Choose", out res, 3000))
                        break;
                    DisplayMsg(LogType.Log, "Delay 2s...");
                    Thread.Sleep(2000);
                }

                if (!result)
                {
                    DisplayMsg(LogType.Log, "Enter DECT MENU fail");
                    AddData(item, 1);
                    return false;
                }

                SendWithoutEnterAndChk(PortType.SSH, "f", "q => Return to Interface Menu", 0, 5000);
                SendWithoutEnterAndChk(PortType.SSH, "1", "Enter firmware binary file name:", 0, 5000);
                DisplayMsg(LogType.Log, $"Write '/tmp/{FW_image}' to ssh");
                SSH_stream.WriteLine(FW_image);
                ChkResponse(PortType.SSH, ITEM.NONE, "press 6 for - 2048", out res, 5000);
                DisplayMsg(LogType.Log, "Write '5' to ssh");
                SSH_stream.Write("5");
                Thread.Sleep(200);
                DisplayMsg(LogType.Log, "Write ' ' to ssh");
                SSH_stream.WriteLine(" ");
                ChkResponse(PortType.SSH, ITEM.NONE, "q => Return to Interface Menu", out res, 120 * 1000);

                DisplayMsg(LogType.Log, "Write 'q' to ssh");
                SSH_stream.Write("q");
                ChkResponse(PortType.SSH, ITEM.NONE, "q => Quit", out res, 5000);

                Match m = Regex.Match(res, @"Target\s+: (?<DECT_ver>.+)");
                if (m.Success)
                {
                    DECTver = m.Groups["DECT_ver"].Value.Trim();
                }
                DisplayMsg(LogType.Log, "Current DECT_version:" + DECTver);
                DisplayMsg(LogType.Log, "SFCS DECT_version:" + infor.DECTver);
                if (DECTver != "" && string.Compare(DECTver, infor.DECTver, true) == 0)
                {
                    status_ATS.AddDataRaw("LRG1_DECT_Ver", infor.DECTver, infor.DECTver, "000000");
                    DisplayMsg(LogType.Log, "Check DECT version pass");
                    AddData(item, 0);
                    return true;
                }
                else
                {
                    DisplayMsg(LogType.Log, "Check DECT version fail");
                    AddData(item, 1);
                    return false;
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                AddData(item, 1);
                return false;
            }
        }
        private string ParseRXTUNE(string content, string keyword = "RXTUN: ")
        {
            try
            {
                string item = string.Empty;
                string res = string.Empty;
                string[] msg;

                res = content.Replace("\n", "$");
                msg = res.Split('$');
                List<String> result = new List<string>();
                for (int i = 0; i < msg.Length; i++)
                {
                    if (msg[i].Contains(keyword))
                    {
                        DisplayMsg(LogType.Log, msg[i]);
                        result.Add(msg[i].Split(':')[1].Trim());
                        //return result[1];
                    }
                }
                return result[result.Count - 1];
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData("CrystalFrequencyHz", 1);
            }
            return string.Empty;
        }
        private bool DECTSignalAnalyzerPresetting(double freqMhz)
        {
            if (!CheckGoNoGo())
            {
                return false;
            }

            try
            {
                double spanHz = Convert.ToDouble(Func.ReadINI("Setting", "DECT_SignalAnalyzer", "SpanHz", "1000000"));
                double rbwKHz = Convert.ToDouble(Func.ReadINI("Setting", "DECT_SignalAnalyzer", "RbwHz", "1000"));
                double vbwKHz = Convert.ToDouble(Func.ReadINI("Setting", "DECT_SignalAnalyzer", "VbwHz", "1000"));
                double rlevDbm = Convert.ToDouble(Func.ReadINI("Setting", "DECT_SignalAnalyzer", "RefLevelDb", "0"));
                double SweepTimeMs = Convert.ToDouble(Func.ReadINI("Setting", "DECT_SignalAnalyzer", "SweepTimeMs", "0"));
                double att = Convert.ToDouble(Func.ReadINI("Setting", "DECT_SignalAnalyzer", "Attenuation", "0"));
                if (Convert.ToInt16(Func.ReadINI("Setting", "MS2830A", "Gpib", "-1")) != -1)
                {
                    #region MS2830A
                    ms2830a.SA.Preset();
                    ms2830a.SA.SetSweepTime(SweepTimeMs);
                    ms2830a.SA.SetSpan(spanHz);
                    ms2830a.SA.SetRbw(rbwKHz);
                    ms2830a.SA.SetVbw(vbwKHz);
                    ms2830a.SA.SetRefLevel(rlevDbm);
                    ms2830a.SA.SetCenterFreq(freqMhz);
                    ms2830a.SA.SetAttenuation(att);
                    return true;
                    #endregion
                }
                else
                {
                    #region N9000A
                    string Address = Func.ReadINI("Setting", "N9000A", "Address", "GPIB0::1::INSTR");
                    double CableOffset = Convert.ToDouble(Func.ReadINI("Setting", "N9000A", "CableOffset", "0"));
                    double Att = Convert.ToDouble(Func.ReadINI("Setting", "N9000A", "Att", "0"));
                    double pwr = -999;
                    double freq = -999;

                    NIVisa nivisa = new NIVisa();
                    MessageBasedSession inst9000A = nivisa.Open_Session(Address);
                    if (inst9000A != null)
                    {
                        nivisa.AGT_N9000A_Set(inst9000A, freqMhz, spanHz, rbwKHz, vbwKHz, rlevDbm, Att, false);
                        return true;
                    }
                    else
                    {
                        DisplayMsg(LogType.Log, "N9000A Open_Session NG");
                        MessageBox.Show("N9000A Spectrum無法控制, 請確認後再繼續測試");
                        return false;
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                return false;
            }
        }
        private bool FetchFrequency(out double freqHz)
        {
            freqHz = 0;

            try
            {
                if (Convert.ToInt16(Func.ReadINI("Setting", "MS2830A", "Gpib", "-1")) != -1)
                {
                    #region MS2830A
                    string section = string.Empty;
                    int delayMs = Convert.ToInt32(Func.ReadINI("Setting", "DECT_SignalAnalyzer", "FetchFrequencyDelayMs", "0"));
                    ms2830a.SA.FetchFrequency(delayMs, ref freqHz);
                    DisplayMsg(LogType.Log, "Frquency (Hz) : " + freqHz.ToString());

                    //freqMhz = freqMhz * 1000000;
                    //freq = freq - freqMhz;
                    //ppm = (freq / freqMhz) * 1000000;
                    #endregion
                }
                else
                {
                    #region N9000A
                    string Address = Func.ReadINI("Setting", "N9000A", "Address", "GPIB0::1::INSTR");
                    double CableOffset = Convert.ToDouble(Func.ReadINI("Setting", "N9000A", "CableOffset", "0"));
                    //double FreqMHz = Convert.ToDouble(Func.ReadINI("Setting", "N9000A", "FreqMHz", "0"));
                    double SpanHz = Convert.ToDouble(Func.ReadINI("Setting", "N9000A", "SpanHz", "0"));
                    double RBW = Convert.ToDouble(Func.ReadINI("Setting", "N9000A", "RBW", "0"));
                    double VBW = Convert.ToDouble(Func.ReadINI("Setting", "N9000A", "VBW", "0"));
                    double RLEVdBm = Convert.ToDouble(Func.ReadINI("Setting", "N9000A", "RLEVdBm", "0"));
                    double Att = Convert.ToDouble(Func.ReadINI("Setting", "N9000A", "Att", "0"));
                    double pwr = -999;

                    NIVisa nivisa = new NIVisa();
                    MessageBasedSession inst9000A = nivisa.Open_Session(Address);
                    if (inst9000A != null)
                    {
                        nivisa.AGT_N9000A_Get_Marker(inst9000A, ref freqHz, ref pwr);
                        DisplayMsg(LogType.Log, "Frquency (Hz) : " + freqHz.ToString());
                        return true;
                    }
                    else
                    {
                        DisplayMsg(LogType.Log, "N9000A Open_Session NG");
                        MessageBox.Show("N9000A Spectrum無法控制, 請確認後再繼續測試");
                        return false;
                    }
                    #endregion
                }
                return true;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                return false;
            }
        }
        private bool FetchPower(out double pwr, int delay = 0)
        {
            pwr = 0;
            double freqHz = 0;

            try
            {
                if (Convert.ToInt16(Func.ReadINI("Setting", "MS2830A", "Gpib", "-1")) != -1)
                {
                    #region MS2830A
                    string section = string.Empty;

                    //ms2830a.SA.FetchPowerMaxHold(0, ref pwr);
                    ms2830a.SA.FetchPower(delay, ref pwr);
                    DisplayMsg(LogType.Log, "Power (dBm) : " + pwr.ToString());

                    //freqMhz = freqMhz * 1000000;
                    //freq = freq - freqMhz;
                    //ppm = (freq / freqMhz) * 1000000;
                    #endregion
                }
                else
                {
                    #region N9000A
                    string Address = Func.ReadINI("Setting", "N9000A", "Address", "GPIB0::1::INSTR");
                    double CableOffset = Convert.ToDouble(Func.ReadINI("Setting", "N9000A", "CableOffset", "0"));
                    //double FreqMHz = Convert.ToDouble(Func.ReadINI("Setting", "N9000A", "FreqMHz", "0"));
                    double SpanHz = Convert.ToDouble(Func.ReadINI("Setting", "N9000A", "SpanHz", "0"));
                    double RBW = Convert.ToDouble(Func.ReadINI("Setting", "N9000A", "RBW", "0"));
                    double VBW = Convert.ToDouble(Func.ReadINI("Setting", "N9000A", "VBW", "0"));
                    double RLEVdBm = Convert.ToDouble(Func.ReadINI("Setting", "N9000A", "RLEVdBm", "0"));
                    double Att = Convert.ToDouble(Func.ReadINI("Setting", "N9000A", "Att", "0"));

                    NIVisa nivisa = new NIVisa();
                    MessageBasedSession inst9000A = nivisa.Open_Session(Address);
                    if (inst9000A != null)
                    {
                        nivisa.AGT_N9000A_Get_Marker(inst9000A, ref freqHz, ref pwr);
                        DisplayMsg(LogType.Log, "Frquency (Hz) : " + freqHz.ToString());
                        return true;
                    }
                    else
                    {
                        DisplayMsg(LogType.Log, "N9000A Open_Session NG");
                        MessageBox.Show("N9000A Spectrum無法控制, 請確認後再繼續測試");
                        return false;
                    }
                    #endregion
                }
                return true;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                return false;
            }
        }

        private void SetDUTInfo()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            DisplayMsg(LogType.Log, "=============== Write Board data and D2 License Key ===============");

            string keyword = "root@OpenWrt:~# \r\n"; //避免誤判到指令第一行的"root@OpenWrt:~#"
            string defaults_img_path = Path.Combine(Application.StartupPath, "default_image_backup");
            string PC_IP = Func.ReadINI("Setting", "PCBA", "PC_IP", "192.168.1.2");
            string item = "SetDUTInfo";
            string res = "";
            string cmd = "";
            //string py_md5sum = "fdb19e9b2f26d02f8eb9c5f547e91420";

            try
            {
                OpenTftpd32(defaults_img_path);

                string SerialNumber = infor.SerialNumber.Substring(infor.SerialNumber.LastIndexOf('+') + 1);
                if (SerialNumber == "" || infor.HWver == "" || infor.BaseMAC == "" || infor.DECT_rfpi == "" || infor.DECT_cal_rxtun == "" || infor.License_key == "")
                {
                    DisplayMsg(LogType.Log, $"Data SerialNumber  :{SerialNumber}");
                    DisplayMsg(LogType.Log, $"Data infor.HWver  :{infor.HWver}");
                    DisplayMsg(LogType.Log, $"Data infor.BaseMAC  :{infor.BaseMAC}");
                    DisplayMsg(LogType.Log, $"Data infor.DECT_rfpi  :{infor.DECT_rfpi}");
                    DisplayMsg(LogType.Log, $"Data infor.DECT_cal_rxtun  :{infor.DECT_cal_rxtun}");
                    DisplayMsg(LogType.Log, $"Data infor.License_key  :{infor.License_key}");

                    AddData(item, 1);
                    return;
                }

                //Partition data formatting Ext4 Partition
                DisplayMsg(LogType.Cmd, $"Write 'mkfs.ext4 /dev/mmcblk0p32' to ssh");
                SSH_stream.WriteLine("mkfs.ext4 /dev/mmcblk0p32");
                Thread.Sleep(10 * 1000); //TODO: 第一次做&重複做的flow不同,待優化
                ChkResponse(PortType.SSH, ITEM.NONE, "Writing superblocks and filesystem accounting information", "/dev/mmcblk0p32 contains a ext4 file system", out res, 5000);
                if (res.Contains("/dev/mmcblk0p32 contains a ext4 file system")) //處理已經做過的case
                {
                    //Proceed anyway?
                    Thread.Sleep(1000);
                    SendAndChk(PortType.SSH, "y", keyword, out res, 0, 10000);
                }
                //Proceed anyway? (y,N) y
                /// dev / mmcblk0p32 is mounted; will not make a filesystem here! 代表可能沒斷電 又跑一次
                //if(!res.Contains("mmcblk0p32 is mounted; will not make a filesystem here!"))
                if (!res.Contains("Writing superblocks and filesystem accounting information:"))
                {
                    DisplayMsg(LogType.Log, "mkfs.ext4 /dev/mmcblk0p32 fail");
                    AddData(item, 1);
                    return;
                }
                SendAndChk(PortType.SSH, "mkdir /mnt/test", keyword, out res, 0, 3000);
                SendAndChk(PortType.SSH, "mount -t ext4 /dev/mmcblk0p32 /mnt/test", keyword, out res, 0, 3000);
                SendAndChk(PortType.SSH, "echo test123 > /mnt/test/file;sync", keyword, out res, 0, 3000);

                // =================================
                // [5.3.4] Remove gen_board_data.py
                // =================================
                /* #region replace_py
                 int index = 0;
                 bool MD5_check = false;
                 //Rena_20230714, replace gen_board_data.py(暫時做法,之後MFG FW進版後就不需要了)
                 //put gen_board_data.py in default_image_backup folder
                 if (!File.Exists(Path.Combine(defaults_img_path, "gen_board_data.py")))
                 {
                     DisplayMsg(LogType.Log, $"File '{Path.Combine(defaults_img_path, "gen_board_data.py")}' doesn't exist");
                     AddData(item, 1);
                     return;
                 }
                 SendAndChk(PortType.SSH, $"tftp -gr gen_board_data.py {PC_IP}", keyword, out res, 0, 5000);
                 SendAndChk(PortType.SSH, "mv gen_board_data.py /lib/gen_board_data.py", keyword, out res, 0, 5000);
                 while (index++ < 5)
                 {
                     SendAndChk(PortType.SSH, "md5sum /lib/gen_board_data.py", keyword, out res, 0, 5000);
                     if (res.Contains(py_md5sum))
                     {
                         MD5_check = true;
                         DisplayMsg(LogType.Log, "gen_board_data.py MD5 check pass");
                         break;
                     }
                     else
                     {
                         DisplayMsg(LogType.Log, "gen_board_data.pyMD5 check fail");
                         Thread.Sleep(1000);
                     }
                 }
                 if (!MD5_check)
                 {
                     AddData(item, 1);
                     return;
                 }
                 #endregion*/

                //SE_TODO: get wifi_password, admin_password, wlan_ssid from SFCS
                //如果已經寫過SFCS有紀錄,就讀出SFCS的值後帶入,如果沒寫過就用renew
                //HQ sample build都直接用renew,生產時請SE修改
                SFCS_Query _sfcsQuery = new SFCS_Query();
                ATS_Template.SFCS_ATS_2_0.ATS ss = new ATS_Template.SFCS_ATS_2_0.ATS();
                string WiFi_SSID_ToWrite = string.Empty;
                string WiFi_PWD_ToWrite = string.Empty;
                string Admin_PWD_ToWrite = string.Empty;

                if (!_sfcsQuery.Get15Data(status_ATS.txtPSN.Text, "LRG1_LABEL_NETWORK", ref WiFi_SSID_ToWrite)) { WiFi_SSID_ToWrite = "renew"; }
                if (!_sfcsQuery.Get15Data(status_ATS.txtPSN.Text, "LRG1_LABEL_PW", ref WiFi_PWD_ToWrite)) { WiFi_PWD_ToWrite = "renew"; }
                if (!_sfcsQuery.Get15Data(status_ATS.txtPSN.Text, "LRG1_LABEL_ADMIN_PW", ref Admin_PWD_ToWrite)) { Admin_PWD_ToWrite = "renew"; }

                //假設wlan_ssid=BT-JHCFPT,gen_board_data.sh時只需帶JHCFPT
                //依客戶要求,EVT3-2 wlan_ssid改為EE-XXXXXX
                WiFi_SSID_ToWrite = WiFi_SSID_ToWrite.Replace("BT-", "").Replace("EE-", "");

                //Generate Board data
                //BaseMAC & DECT_rfpi are capital letters
                SendAndChk(PortType.SSH, $"gen_board_data.sh {SerialNumber} {infor.HWver} {infor.BaseMAC.ToUpper()} {infor.DECT_rfpi.ToUpper()} {infor.DECT_cal_rxtun} {WiFi_PWD_ToWrite} {Admin_PWD_ToWrite} {WiFi_SSID_ToWrite}", keyword, out res, 0, 10000);
                if (!res.Contains(keyword) || res.IndexOf("Error", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    DisplayMsg(LogType.Log, "Generate board data fail");
                    AddData(item, 1);
                    return;
                }
                //Generate D2 License Key
                cmd = $"echo \"{infor.License_key}\" > /tmp/defaults/D2License.key";
                SendAndChk(PortType.SSH, cmd, keyword, 0, 5000);
                if (!SendAndChk(PortType.SSH, "cat /tmp/defaults/D2License.key", infor.License_key, out res, 0, 5000))
                {
                    DisplayMsg(LogType.Log, "Generate D2 License Key fail");
                    AddData(item, 1);
                    return;
                }

                //Write data into DUT
                SendAndChk(PortType.SSH, "gen_squashfs.sh", "No such file or directory", "4096 bytes (4.0KB) copied", out res, 0, 40000);
                if (!res.Contains("100.00%") || res.Contains("No such file or directory"))
                {
                    DisplayMsg(LogType.Log, "Write Board data and D2 License Key fail");
                    AddData(item, 1);
                    return;
                }

                if (!SendAndChk(PortType.SSH, "ls /tmp", "defaults.img", out res, 0, 5000))
                {
                    DisplayMsg(LogType.Log, "Can't find /tmp/defaults.img");
                    AddData(item, 1);
                    return;
                }

                //backup defaults.img
                if (Directory.Exists(defaults_img_path))
                {
                    File.Delete(Path.Combine(defaults_img_path, "defaults.img"));
                    File.Delete(Path.Combine(defaults_img_path, $"defaults_{infor.SerialNumber}.img"));
                    SendAndChk(PortType.SSH, "cd /tmp", "root@OpenWrt:/tmp#", out res, 0, 3000);
                    SendAndChk(PortType.SSH, $"tftp -p -l defaults.img {PC_IP}", "root@OpenWrt:/tmp# \r\n", out res, 0, 5000);
                    if (File.Exists(Path.Combine(defaults_img_path, "defaults.img")))
                    {
                        File.Move(Path.Combine(defaults_img_path, "defaults.img"), Path.Combine(defaults_img_path, $"defaults_{infor.SerialNumber}.img"));
                        DisplayMsg(LogType.Log, $"Backup defaults.img in {Path.Combine(defaults_img_path, $"defaults_{infor.SerialNumber}.img")}");
                    }
                }

                AddData(item, 0);
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                AddData(item, 1);
            }
            finally
            {
                SendAndChk(PortType.SSH, "cd ~", keyword, out res, 0, 3000);
            }
        }

        private void CheckDUTInfo()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            DisplayMsg(LogType.Log, "=============== Verify Board data and D2 License Key ===============");

            //string keyword = @"root@OpenWrt";
            string keyword = "root@OpenWrt:~# \r\n";
            string item = "ChkDUTInfo";
            string res = "";
            string wifi_password = "";
            string admin_password = "";
            string wlan_ssid = "";
            Match m;

            try
            {
                SendAndChk(PortType.SSH, "verify_boarddata.sh", keyword, out res, 0, 5000);

                //serial_number=+119746+2333000129
                if (!res.Contains($"serial_number={infor.SerialNumber}"))
                {
                    DisplayMsg(LogType.Log, "Check serial_number fail");
                    AddData(item, 1);
                }
                //hw_ver=EVT1
                if (!res.Contains($"hardware_version={infor.HWver}"))
                {
                    DisplayMsg(LogType.Log, "Check hw_ver fail");
                    AddData(item, 1);
                }
                //mac_base=E8:C7:CF:AF:46:28
                if (!res.Contains($"mac_base={infor.BaseMAC.ToUpper()}"))
                {
                    DisplayMsg(LogType.Log, "Check mac_base fail");
                    AddData(item, 1);
                }
                //dect_identity_rfpi=03.6C.D3.A9.38
                if (!res.Contains($"dect_identity_rfpi={infor.DECT_rfpi.ToUpper()}"))
                {
                    DisplayMsg(LogType.Log, "Check dect_identity_rfpi fail");
                    AddData(item, 1);
                }
                //dect_rf_calibration_rxtun=77
                if (!res.Contains($"dect_rf_calibration_rxtun={infor.DECT_cal_rxtun}"))
                {
                    DisplayMsg(LogType.Log, "Check dect_rf_calibration_rxtun fail");
                    AddData(item, 1);
                }

                //SE_TODO: 如果已經寫過SFCS有紀錄,就讀出SFCS的值後檢查,如果沒寫過就讀出後上拋SFCS
                if (infor.WiFi_PWD == "")
                {
                    //wifi_password=DPCwqeJzEanKkuMU
                    m = Regex.Match(res, "wifi_password=(?<wifi_password>.+)");
                    if (m.Success)
                    {
                        wifi_password = m.Groups["wifi_password"].Value.Trim();
                        infor.WiFi_PWD = wifi_password;
                    }
                    else
                    {
                        DisplayMsg(LogType.Log, "Check wifi_password fail");
                        AddData(item, 1);
                    }
                }
                else
                {
                    if (!res.Contains($"wifi_password={infor.WiFi_PWD}"))
                    {
                        DisplayMsg(LogType.Log, "Check wifi_password fail");
                        AddData(item, 1);
                    }
                }

                //SE_TODO: 如果已經寫過SFCS有紀錄,就讀出SFCS的值後檢查,如果沒寫過就讀出後上拋SFCS
                if (infor.Admin_PWD == "")
                {
                    //admin_password=citerxfg
                    m = Regex.Match(res, "admin_password=(?<admin_password>.+)");
                    if (m.Success)
                    {
                        admin_password = m.Groups["admin_password"].Value.Trim();
                        infor.Admin_PWD = admin_password;
                    }
                    else
                    {
                        DisplayMsg(LogType.Log, "Check admin_password fail");
                        AddData(item, 1);
                    }
                }
                else
                {
                    if (!res.Contains($"admin_password={infor.Admin_PWD}"))
                    {
                        DisplayMsg(LogType.Log, "Check admin_password fail");
                        AddData(item, 1);
                    }
                }

                //SE_TODO: 如果已經寫過SFCS有紀錄,就讀出SFCS的值後檢查,如果沒寫過就讀出後上拋SFCS
                if (infor.WiFi_SSID == "")
                {
                    //wlan_ssid=BT-F5C26X
                    m = Regex.Match(res, "wlan_ssid=(?<wlan_ssid>.+)");
                    if (m.Success)
                    {
                        wlan_ssid = m.Groups["wlan_ssid"].Value.Trim();
                        infor.WiFi_SSID = wlan_ssid;
                    }
                    else
                    {
                        DisplayMsg(LogType.Log, "Check wlan_ssid fail");
                        AddData(item, 1);
                    }
                }
                else
                {
                    if (!res.Contains($"wlan_ssid={infor.WiFi_SSID}"))
                    {
                        DisplayMsg(LogType.Log, "Check wlan_ssid fail");
                        AddData(item, 1);
                    }
                }
                //Rena_20230717,依客戶要求EVT3-2 wlan_ssid改為EE-XXXXXX
                if (!infor.WiFi_SSID.StartsWith("EE-"))
                {
                    DisplayMsg(LogType.Log, "Check WiFi_SSID prefix 'EE-' fail");
                    AddData(item, 1);
                }

                //以下為固定值確認
                if (!res.Contains("check=0") || !res.Contains("device_category=COM_IGD") || !res.Contains("manufacturer=BT") || !res.Contains("wifi_country_revision=0") ||
                    !res.Contains("manufacturer_oui=0000DB") || !res.Contains("model_name=Smart Hub SH40J") || !res.Contains("model_number=SH40J") ||
                    !res.Contains("description=Smart Hub SH40J") || !res.Contains("product_class=SH4-1") || !res.Contains("mac_count=8") ||
                    !res.Contains("item_code=119746") || !res.Contains("brand_variant=Consumer") || !res.Contains("wifi_country_code=GB"))
                {
                    DisplayMsg(LogType.Log, "Check board data fail");
                    AddData(item, 1);
                }

                //Verify D2 License 
                //if (!SendAndChk(PortType.SSH, "cat /tmp/defaults/D2License.key", infor.License_key, out res, 0, 5000))
                SendAndChk(PortType.SSH, "cat /tmp/defaults/D2License.key", keyword, out res, 0, 5000);
                if (!res.Contains(infor.License_key))
                {
                    DisplayMsg(LogType.Log, "Check D2 License Key fail");
                    AddData(item, 1);
                }

                //Rena_20230717 add
                /*SendAndChk(PortType.SSH, "/etc/init.d/vtspd start", keyword, out res, 0, 5000);
                SendAndChk(PortType.SSH, "ps | grep ve_vtsp_main", keyword, out res, 0, 5000);
                if (!res.Contains("/usr/bin/ve_vtsp_main"))
                {
                    DisplayMsg(LogType.Log, "Check ve_vtsp_main fail, D2 License is incorrect");
                    AddData(item, 1);
                }
                SendAndChk(PortType.SSH, "/etc/init.d/vtspd stop", keyword, out res, 0, 5000);*/

                if (CheckGoNoGo())
                {
                    AddData(item, 0);
                    //upload data to SFCS
                    status_ATS.AddDataRaw("LRG1_SN", infor.SerialNumber, infor.SerialNumber, "000000");
                    status_ATS.AddDataRaw("LRG1_LABEL_MAC", infor.BaseMAC.Trim().Replace(":", ""), infor.BaseMAC.Trim().Replace(":", ""), "000000");
                    status_ATS.AddDataRaw("LRG1_HWver", infor.HWver, infor.HWver, "000000");
                    status_ATS.AddDataRaw("LRG1_DECT_rfpi", infor.DECT_rfpi, infor.DECT_rfpi, "000000");
                    status_ATS.AddDataRaw("LRG1_License_key", infor.License_key, infor.License_key, "000000");
                    status_ATS.AddDataRaw("LRG1_LABEL_NETWORK", infor.WiFi_SSID, infor.WiFi_SSID, "000000");
                    status_ATS.AddDataRaw("LRG1_LABEL_PW", infor.WiFi_PWD, infor.WiFi_PWD, "000000");
                    status_ATS.AddDataRaw("LRG1_LABEL_ADMIN_PW", infor.Admin_PWD, infor.Admin_PWD, "000000");

                    CheckRuleLRG1(infor.BaseMAC.Trim().Replace(":", ""), infor.SerialNumber, infor.WiFi_PWD, infor.Admin_PWD, infor.WiFi_SSID); //Jason add 全参数比对/ Check Thông số đầy đủ
                    //TODO:上拋其他data,如wifi相關data
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                AddData(item, 1);
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
                DisplayMsg(LogType.Exception, ex.ToString());
            }
            return "error";
        }
        private string CalculateMD5ofFile(string filename)
        {
            try
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(filename))
                    {
                        var hash = md5.ComputeHash(stream);
                        return BitConverter.ToString(hash).Replace("-", "").ToUpper();
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
            }
            return "error";
        }
        private bool WriteOrCheckDeviceInfor(PortType portType, string item, string cmd, string keyword)
        {
            string res = "";
            try
            {
                if (!CheckGoNoGo())
                    return false;
                DisplayMsg(LogType.Log, $"=============== {item} ===============");

                if (!SendAndChk(portType, cmd, keyword, out res, 0, 3000))
                {
                    AddData(item, 1);
                    return false;
                }
                AddData(item, 0);
                return true;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Log, ex.ToString());
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

                if (!SendAndChk(portType, "echo 0 > /sys/class/pwm/pwmchip0/export", keyword, out res, 200, 3000))
                    return false;

                if (!SendAndChk(portType, "echo 1 > /sys/class/pwm/pwmchip0/export", keyword, out res, 200, 3000))
                    return false;

                if (!SendAndChk(portType, "echo 2 > /sys/class/pwm/pwmchip0/export", keyword, out res, 200, 3000))
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
                DisplayMsg(LogType.Exception, ex.Message);
                warning = "Exception";
                return false;
            }
        }

        private string MACConvert(string mac, int param = 0)
        {
            try
            {
                //DisplayMsg(LogType.Log, "MAC input:" + mac);
                string ethmac = mac.Replace(":", "");
                ethmac = Convert.ToString(Convert.ToInt64(ethmac, 16) + param, 16).PadLeft(12, '0');
                var regex = "(.{2})(.{2})(.{2})(.{2})(.{2})(.{2})";
                var replace = "$1:$2:$3:$4:$5:$6";
                ethmac = Regex.Replace(ethmac, regex, replace).ToUpper();
                //DisplayMsg(LogType.Log, "MAC output:" + ethmac);
                return ethmac;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Log, ex.ToString());
                warning = "MAC Convert error";
                return "error";
            }
        }

        private void EthernetTest(int port_num)
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            int retry_cnt;
            string item = "EthernetTest";
            //string keyword = @"root@OpenWrt";
            //string res = "";
            // ================================================
            //PCBA
            //RF
            //OTA
            //FINAL
            // ================================================
            DisplayMsg(LogType.Log, "=============== Ethernet Test ===============");
            retry_cnt = 0;
            try
            {
                frmOK.Label = $"Sau khi kết nối dây mạng vào cổng LAN số {port_num}, vui lòng nhấn\"Xác nhận\"";
                frmOK.ShowDialog();

            LAN_Port_Test:
                if (SendAndChk(PortType.SSH, "mt eth linkrate", $"port {port_num}: 2500M FD", 0, 3000))
                {
                    DisplayMsg(LogType.Log, $"Check LAN Port{port_num} pass");
                    if (port_num == 5)
                    {
                        DisplayMsg(LogType.Log, "Check WAN Port pass");
                    }
                }
                else
                {
                    DisplayMsg(LogType.Log, $"Check LAN Port{port_num} fail");
                    if (retry_cnt++ < 3)
                    {
                        frmOK.Label = $"Vui lòng kiểm tra dây mạng đã được kết nối đúng vào cổng LAN số {port_num} chưa";
                        frmOK.ShowDialog();
                        DisplayMsg(LogType.Log, "Delay 1000ms, retry...");
                        Thread.Sleep(1000);
                        goto LAN_Port_Test;
                    }
                    else
                    {
                        AddData($"Eth_LAN_Port{port_num}", 1);
                        return;
                    }
                }
                //WAN Port
                //if (!ToTest_WanPort) // test bind to WAN only
                //{
                //    return;
                //}
                //retry_cnt = 0;
                //frmOK.Label = "Hãy kết nối dây mạng vào cổng WAN, sau đó nhấn\"Xác nhận\"";
                //frmOK.ShowDialog();
                //WAN_Port_Test:
                //if (SendAndChk(PortType.SSH, "mt eth linkrate", "port 5: 2500M FD", 0, 3000))
                //{
                //    DisplayMsg(LogType.Log, "Check WAN Port pass");
                //}
                //else
                //{
                //    DisplayMsg(LogType.Log, "Check WAN Port fail");
                //    if (retry_cnt++ < 3)
                //    {
                //        frmOK.Label = "Vui lòng kiểm tra dây mạng đã được kết nối đúng vào cổng WAN chưa";
                //        frmOK.ShowDialog();
                //        DisplayMsg(LogType.Log, "Delay 1000ms, retry...");
                //        Thread.Sleep(1000);
                //        goto WAN_Port_Test;
                //    }
                //    else
                //    {
                //        AddData("Eth_WAN_Port", 1);
                //        return;
                //    }
                //}

            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData(item, 1);
            }
        }

        private void ChkMacAddr()
        {
            if (!CheckGoNoGo())
            {
                return;
            }
            string keyword = @"root@OpenWrt";
            string item = "ChkMacAddr";
            string res = "";
            try
            {
                //Write MAC Address 寫完後要重開機才會生效,所以PCBA站寫入後在final站檢查
                //Eth0 ~ Eth3 = LAN = BaseMAC
                //Eth4 = WAN = BaseMAC + 1
                //echo -n -e '\xE8\xC7\xCF\xAF\x46\x38' > /tmp/mac
                DisplayMsg(LogType.Log, "=============== chk eth0-eth4 MacAddr ===============");
                string write_lan_mac = MACConvert(infor.BaseMAC).Replace(":", "\\x");
                string write_wan_mac = MACConvert(infor.WanMAC).Replace(":", "\\x");
                DisplayMsg(LogType.Log, $"BaseMAC: {infor.BaseMAC}, WanMAC: {infor.WanMAC}");
                DisplayMsg(LogType.Log, $"Write LAN MAC: {write_lan_mac}");
                DisplayMsg(LogType.Log, $"Write WAN MAC: {write_wan_mac}");

                SendAndChk(PortType.SSH, $"echo -n -e '\\x{write_lan_mac}' > /tmp/mac", keyword, out res, 0, 3000); //eth0
                SendAndChk(PortType.SSH, $"echo -n -e '\\x{write_lan_mac}' >> /tmp/mac", keyword, out res, 0, 3000); //eth1
                SendAndChk(PortType.SSH, $"echo -n -e '\\x{write_lan_mac}' >> /tmp/mac", keyword, out res, 0, 3000); //eth2
                SendAndChk(PortType.SSH, $"echo -n -e '\\x{write_lan_mac}' >> /tmp/mac", keyword, out res, 0, 3000); //eth3
                SendAndChk(PortType.SSH, $"echo -n -e '\\x{write_wan_mac}' >> /tmp/mac", keyword, out res, 0, 3000); //eth4
                SendAndChk(PortType.SSH, "dd if=/tmp/mac of=/dev/mmcblk0p21 bs=1 count=30", keyword, out res, 0, 3000);
                if (res.Contains("30 bytes (30B) copied") && !res.Contains("No such file or directory"))
                {
                    DisplayMsg(LogType.Log, "Write eth0~eth4 MAC Address pass");
                    AddData(item, 0);
                    status_ATS.AddDataRaw("LRG1_BASE_MAC", infor.BaseMAC.Trim().Replace(":", ""), infor.BaseMAC.Trim().Replace(":", ""), "000000");
                    status_ATS.AddDataRaw("LRG1_WAN_MAC", infor.WanMAC.Trim().Replace(":", ""), infor.WanMAC.Trim().Replace(":", ""), "000000");
                }
                else
                {
                    DisplayMsg(LogType.Log, "Write eth0~eth4 MAC Address fail");
                    AddData(item, 1);
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData(item, 1);
            }
        }
        private string InsertColon(string macAddress)
        {
            macAddress = macAddress.Replace(":", "");
            if (macAddress.Length != 12)
            {
                return macAddress;
            }
            string formattedMAC = string.Join(":", Enumerable.Range(0, 6).Select(i => macAddress.Substring(i * 2, 2)));

            return formattedMAC;
        }
        private void CheckEthernetMAC()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            string item = "CheckEthernetMAC";
            string keyword = "root@OpenWrt:~# \r\n";
            string res = "";
            string formattedBaseMAC = string.Empty;

            try
            {
                DisplayMsg(LogType.Log, "=============== Check Ethernet MAC ===============");
                DisplayMsg(LogType.Log, $"Base MAC Get from SFCS: {infor.BaseMAC}");
                formattedBaseMAC = InsertColon(infor.BaseMAC);
                DisplayMsg(LogType.Log, $"Base MAC Convert: {formattedBaseMAC}");
                if (formattedBaseMAC.Length != 17 || formattedBaseMAC.Length != infor.WanMAC.Length) { DisplayMsg(LogType.Log, "Leng mac fail"); return; }
                SendAndChk(PortType.SSH, "ifconfig | grep eth", keyword, out res, 0, 5000);
                if (Regex.IsMatch(res, $"eth0.+HWaddr {formattedBaseMAC}") && Regex.IsMatch(res, $"eth1.+HWaddr {formattedBaseMAC}") &&
                    Regex.IsMatch(res, $"eth2.+HWaddr {formattedBaseMAC}") && Regex.IsMatch(res, $"eth3.+HWaddr {formattedBaseMAC}") &&
                    Regex.IsMatch(res, $"eth4.+HWaddr {infor.WanMAC}"))
                {
                    DisplayMsg(LogType.Log, "Check eth0~eth4 MAC Address pass");
                    AddData(item, 0);
                }
                else
                {
                    DisplayMsg(LogType.Log, "Check eth0~eth4 MAC Address fail");
                    AddData(item, 1);
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData(item, 1);
            }
        }
        private void CurrentSensor()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            string item = "CurrentSensor";
            string keyword = @"root@OpenWrt";
            string res = "";
            string Power = "";
            string Current = "";

            try
            {
                DisplayMsg(LogType.Log, "=============== Check Current Sensor ===============");

                //初期先收集數據,不判斷pass/fail
                SendAndChk(PortType.SSH, "mt power", keyword, out res, 0, 3000);
                Match m = Regex.Match(res, @"Power\(W\): (?<power>.+)\r\nCurrent\(A\): (?<current>.+)");
                if (m.Success)
                {
                    Power = m.Groups["power"].Value.Trim();
                    Current = m.Groups["current"].Value.Trim();
                }

                DisplayMsg(LogType.Log, "Power: " + Power);
                DisplayMsg(LogType.Log, "Current: " + Current);
                if (Power != "" && Current != "")
                {
                    status_ATS.AddData("Power", "W", Convert.ToDouble(Power));
                    status_ATS.AddData("Current", "A", Convert.ToDouble(Current));
                    //status_ATS.AddDataRaw("LRG1_Power", Power, Power, "000000");
                    //status_ATS.AddDataRaw("LRG1_Current", Current, Current, "000000");
                    AddData(item, 0);
                }
                else
                {
                    DisplayMsg(LogType.Log, "Check current sensor fail");
                    AddData(item, 1);
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData(item, 1);
            }
        }
        private void BleTest()
        {
            /*if (!CheckGoNoGo())
            {
                return;
            }

            string item = "BleTest";
            string keyword = @"root@OpenWrt";
            string res = "";
            string ble_addr = "";
            string ble_ver = "";
            string se_ver = "";


            try
            {
                DisplayMsg(LogType.Log, "=============== BLE Test ===============");
                //Rena_20230803, add ble_ver and se_ver for BLE test
                //Check BLE version
                //[I] Bluetooth stack booted: v5.0.0-b108
                SendAndChk(PortType.SSH, "echo 1 > /sys/class/gpio/ble_fw_upgrade/value", keyword, out res, 0, 5000);
                SendAndChk(PortType.SSH, "echo 0 > /sys/class/gpio/ble_rst/value", keyword, out res, 0, 5000);
                Thread.Sleep(300);
                SendAndChk(PortType.SSH, "echo 1 > /sys/class/gpio/ble_rst/value;sync;sync", keyword, out res, 0, 5000);
                SendAndChk(PortType.SSH, "bt_host_empty -u /dev/ttyMSM1 -v", keyword, out res, 0, 5000);
                Match m = Regex.Match(res, "Bluetooth stack booted: (?<BLE_ver>.+)");
                if (m.Success)
                {
                    ble_ver = m.Groups["BLE_ver"].Value.Trim();
                }
                DisplayMsg(LogType.Log, $"SFCS BLEver: {infor.BLEver}");
                DisplayMsg(LogType.Log, "BLE version: " + ble_ver);
                if (ble_ver == "" || string.Compare(infor.BLEver, ble_ver) != 0)
                {
                    DisplayMsg(LogType.Log, "Check BLE version fail");
                    AddData(item, 1);
                }
                status_ATS.AddDataRaw("LRG1_BLE_Version", ble_ver, ble_ver, "000000");

                //check security element version
                //[I] SE FW version: 0001020E
                m = Regex.Match(res, "SE FW version: (?<SE_ver>.+)");
                if (m.Success)
                {
                    se_ver = m.Groups["SE_ver"].Value.Trim();
                }
                DisplayMsg(LogType.Log, $"SFCS SEver: {infor.SEver}");
                DisplayMsg(LogType.Log, "SE version: " + se_ver);
                if (se_ver == "" || string.Compare(infor.SEver, se_ver) != 0)
                {
                    DisplayMsg(LogType.Log, "Check security element version fail");
                    AddData(item, 1);
                }

                status_ATS.AddDataRaw("LRG1_SE_Version", se_ver, se_ver, "000000");

                //BLE mac是來料就設定好了的,不需寫入,只要讀取後上拋SFCS
                SendAndChk(PortType.SSH, "echo 1 > /sys/class/gpio/ble_fw_upgrade/value;echo 0 > /sys/class/gpio/ble_rst/value;usleep 300000;echo 1 > /sys/class/gpio/ble_rst/value", "#", out res, 0, 10000);


                SendAndChk(PortType.SSH, "bt_host_empty -u /dev/ttyMSM1", "Started advertising", out res, 0, 10000);
                Match m = Regex.Match(res, @"Bluetooth public device address: (?<ble_addr>[\:\w]{17})");
                if (m.Success)
                {
                    ble_addr = m.Groups["ble_addr"].Value;
                    DisplayMsg(LogType.Log, "BLE address: " + ble_addr);
                    infor.BleMAC = ble_addr; //Rena_20230522, for HQ sample test flow
                    AddData(item, 0);
                    status_ATS.AddDataRaw("LRG1_BLE_MAC", ble_addr.Trim().Replace(":", ""), ble_addr.Trim().Replace(":", ""), "000000");
                }
                else
                {
                    DisplayMsg(LogType.Log, "Check BLE address fail");
                    AddData(item, 1);
                }

                //send ctrl+c
                SendCommand(PortType.SSH, sCtrlC, 500);
                ChkResponse(PortType.SSH, ITEM.NONE, keyword, out res, 3000);
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData(item, 1);
            }*/

            if (!CheckGoNoGo())
            {
                return;
            }

            string item = "BleTest";
            string keyword = @"root@OpenWrt";
            string res = "";
            string ble_addr = "";
            string ble_ver = "";
            string se_ver = "";

            try
            {
                DisplayMsg(LogType.Log, "=============== BLE Test ===============");

                //Rena_20230803, add ble_ver and se_ver for BLE test
                //Check BLE version
                //[I] Bluetooth stack booted: v5.0.0-b108
                SendAndChk(PortType.SSH, "echo 1 > /sys/class/gpio/ble_fw_upgrade/value", keyword, out res, 0, 5000);
                SendAndChk(PortType.SSH, "echo 0 > /sys/class/gpio/ble_rst/value", keyword, out res, 0, 5000);
                DisplayMsg(LogType.Log, @"Delay 3s UP for BLE reset time");
                Thread.Sleep(3 * 1000);
                SendAndChk(PortType.SSH, "echo 1 > /sys/class/gpio/ble_rst/value;sync;sync", keyword, out res, 0, 5000);
                Thread.Sleep(3 * 1000);
                SendAndChk(PortType.SSH, "bt_host_empty -u /dev/ttyMSM1 -v", keyword, out res, 0, 5000);
                Thread.Sleep(2 * 1000);
                Match m = Regex.Match(res, "Bluetooth stack booted: (?<BLE_ver>.+)");
                if (m.Success)
                {
                    ble_ver = m.Groups["BLE_ver"].Value.Trim();
                }
                DisplayMsg(LogType.Log, $"SFCS BLEver: {infor.BLEver}");
                DisplayMsg(LogType.Log, "BLE version: " + ble_ver);
                if (ble_ver == "" || string.Compare(infor.BLEver, ble_ver) != 0)
                {
                    DisplayMsg(LogType.Log, "Check BLE version fail");
                    AddData(item, 1);
                }

                //check security element version
                //[I] SE FW version: 0001020E
                m = Regex.Match(res, "SE FW version: (?<SE_ver>.+)");
                if (m.Success)
                {
                    se_ver = m.Groups["SE_ver"].Value.Trim();
                }
                DisplayMsg(LogType.Log, $"SFCS SEver: {infor.SEver}");
                DisplayMsg(LogType.Log, "SE version: " + se_ver);
                if (se_ver == "" || string.Compare(infor.SEver, se_ver) != 0)
                {
                    DisplayMsg(LogType.Log, "Check security element version fail");
                    AddData(item, 1);
                }

                //check mac
                //BLE mac是來料就設定好了的,不需寫入,只要讀取後上拋SFCS
                //[I] Bluetooth public device address: E8:E0:7E:E4:DE:B7
                m = Regex.Match(res, @"Bluetooth public device address: (?<ble_addr>[\:\w]{17})");
                if (m.Success)
                {
                    ble_addr = m.Groups["ble_addr"].Value;
                    DisplayMsg(LogType.Log, "BLE address: " + ble_addr);
                }
                else
                {
                    DisplayMsg(LogType.Log, "Check BLE MAC fail");
                    AddData(item, 1);
                }

                if (CheckGoNoGo())
                {
                    AddData(item, 0);
                    status_ATS.AddDataRaw("LRG1_BLE_MAC", ble_addr.Trim().Replace(":", ""), ble_addr.Trim().Replace(":", ""), "000000");
                    status_ATS.AddDataRaw("LRG1_BLE_Ver", ble_ver, ble_ver, "000000");
                    status_ATS.AddDataRaw("LRG1_SE_Ver", se_ver, se_ver, "000000");
                    infor.BleMAC = ble_addr; //Rena_20230522, for HQ sample test flow
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData(item, 1);
            }
        }
        private void NFCTag()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            string item = "NFCTag";
            string keyword = "root@OpenWrt:~# \r\n";
            string res = "";
            string NFC_UID = "";

            try
            {
                DisplayMsg(LogType.Log, "=============== Read NFC Tag UID ===============");

                //0x04 0x33 0x88 0xe2 0xed 0x10 0x90 0x00 0x44 0x00 0x00 0x00 0x00 0x00 0x00 0x00
                SendAndChk(PortType.SSH, "i2ctransfer -y 0 w1@0x55 0x0 r16", keyword, out res, 0, 3000);
                string[] lines = res.Split('\n');
                foreach (string line in lines)
                {
                    if (line.StartsWith("0x"))
                    {
                        string[] vals = line.Split(' ');
                        NFC_UID = string.Join(" ", vals, 0, 7);
                        DisplayMsg(LogType.Log, "NFC_UID: " + NFC_UID);
                        break;
                    }
                }
                //PCBA站使用NFC Tag陪測golden,不需上拋UID
                if (NFC_UID == "")
                {
                    DisplayMsg(LogType.Log, "Read NFC Tag UID fail");
                    AddData(item, 1);
                    return;
                }
                //Check NFC field detection pin
                SendAndChk(PortType.SSH, "mt gpio dump nfc", keyword, out res, 0, 3000);
                if (res.Contains("NFC: low"))
                {
                    DisplayMsg(LogType.Log, "Check NFC field detection pin - low pass");
                    AddData(item, 0);
                }
                else
                {
                    DisplayMsg(LogType.Log, "Check NFC field detection pin - low fail");
                    AddData(item, 1);
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData(item, 1);
            }
        }
        private bool PingDUT(string item, PortType portType, string ip, string keyword, out string res, int timeOutMs = 5 * 1000)
        {
            bool result = false;
            res = "";

            try
            {
                SendCommand(portType, "ping " + ip + " -c 3", 500);
                ChkResponse(portType, ITEM.NONE, keyword, out res, timeOutMs);

                if (res.Contains(ip) && res.Contains(keyword))
                {
                    AddData(item, 0);
                    result = true;
                }
                else
                {
                    AddData(item, 1);
                    result = false;
                }
                SendCommand(portType, sCtrlC, 500);
                ChkResponse(portType, ITEM.NONE, "root@OpenWrt", out res, timeOutMs);
                return result;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
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
                DisplayMsg(LogType.Exception, ex.ToString());
                AddData("SFP_TP_Test", 1);
            }
        }
        private void LED_Control(string item_name, string cmd, CTRL ctrl)
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            string keyword = @"root@OpenWrt";
            string test_item = $"{item_name}_{ctrl.ToString()}";

            try
            {
                if (ctrl == CTRL.ON)
                {
                    SendAndChk(PortType.SSH, $"mt led set {cmd} 255", keyword, 0, 3000);

                    frmYN.Label = $"Vui lòng kiểm tra {item_name} có đang sáng không?";
                    frmYN.ShowDialog();
                    if (frmYN.no)
                    {
                        AddData(test_item, 1);
                    }
                    else
                    {
                        AddData(test_item, 0);
                    }
                }
                else if (ctrl == CTRL.OFF)
                {
                    SendAndChk(PortType.SSH, $"mt led set {cmd} 0", keyword, 0, 3000);

                    frmYN.Label = $"Vui lòng kiểm tra {item_name} có đang tắt không?";
                    frmYN.ShowDialog();
                    if (frmYN.no)
                    {
                        AddData(test_item, 1);
                    }
                    else
                    {
                        AddData(test_item, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                AddData(test_item, 1);
            }
        }
        private void CheckLED()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            try
            {
                DisplayMsg(LogType.Log, "=============== LED Test ===============");

                //LED_White_On
                LED_Control("LED_White", "w", CTRL.ON);
                LED_Control("LED_White", "w", CTRL.OFF);

                //LED1_Green
                LED_Control("LED_Green", "g", CTRL.ON);
                LED_Control("LED_Green", "g", CTRL.OFF);

                //LED1_Red
                LED_Control("LED_Red", "r", CTRL.ON);
                LED_Control("LED_Red", "r", CTRL.OFF);

                //LED1_Blue
                LED_Control("LED_Blue", "b", CTRL.ON);
                LED_Control("LED_Blue", "b", CTRL.OFF);
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                AddData("LED", 1);
            }
        }

        private void CheckPCIe()
        {
            if (!CheckGoNoGo())
            {
                return;
            }
            string res = string.Empty;
            string item = "CheckPCIe";
            string keyword = "ok";
            try
            {
                DisplayMsg(LogType.Log, "=============== Check WiFi 2.4G PCIe Interface ===============");
                int Counttimer = 0;
            retryPCIE:
                //========================================================================================
                //result = SendAndChk(item, PortType.SSH, "lspci -s 0002:00:00.0 -vv | grep Speed", "LnkSta:\tSpeed 8GT/s (ok), Width x1 (ok)", 2000, 3000);
                //result &= SendAndChk(item, PortType.SSH, "lspci -s 0002:01:00.0 -vv | grep Speed", "LnkSta:\tSpeed 8GT/s (ok), Width x1 (downgraded)", 2000, 3000);
                //DisplayMsg(LogType.Log, "Check WiFi 6G PCIe Interface");
                //result &= SendAndChk(item, PortType.SSH, "lspci -s 0003:00:00.0 -vv | grep Speed", "LnkSta:\tSpeed 8GT/s (ok), Width x2 (ok)", 2000, 3000);
                //result &= SendAndChk(item, PortType.SSH, "lspci -s 0003:01:00.0 -vv | grep Speed", "LnkSta:\tSpeed 8GT/s (ok), Width x2 (ok)", 2000, 3000);
                //DisplayMsg(LogType.Log, "Check WiFi 5G PCIe Interface");
                //result &= SendAndChk(item, PortType.SSH, "lspci -s 0004:00:00.0 -vv | grep Speed", "LnkSta:\tSpeed 8GT/s (ok), Width x2 (ok)", 2000, 3000);
                //result &= SendAndChk(item, PortType.SSH, "lspci -s 0004:01:00.0 -vv | grep Speed", "LnkSta:\tSpeed 8GT/s (ok), Width x2 (ok)", 2000, 3000);
                //=========================================================================================
                //===== Audrey consoldiate cmd based on testplan
                SendAndChk(PortType.SSH, "lspci -vv | grep \"LnkSta:\"", "#", out res, 2000, 80000);
                if (res.Contains(keyword))
                {
                    DisplayMsg(LogType.Log, "Check PCIE Interface Pass");
                    AddData(item, 0);
                }
                else
                {
                    if (Counttimer < 3)
                    {
                        Counttimer++;
                        DisplayMsg(LogType.Log, "Check PCIE Interface fail go to retry");
                        goto retryPCIE;
                    }
                    else
                    {
                        DisplayMsg(LogType.Log, "Check PCIE Interface fail");
                        AddData(item, 1);
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData(item, 1);
            }
        }
        private void WPSButton()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            string res = "";
            string item = "WPSButton";
            string keyword = @"root@OpenWrt";

            try
            {
                #region WPS
                DisplayMsg(LogType.Log, "=============== WPS Button ===============");

                bool pressed = false;
                bool released = false;
                for (int i = 0; i < 3; i++)
                {
                    frmOK.Label = "Nhấn và giữ nút WPS, sau đó nhấn\"Xác nhận\"";
                    frmOK.ShowDialog();

                    SendAndChk(PortType.SSH, "mt gpio dump all", keyword, out res, 0, 3000);
                    if (res.Contains("WPS: low"))
                    {
                        pressed = true;
                        DisplayMsg(LogType.Log, "Check WPS Button pressed ok");
                    }

                    frmOK.Label = "Nhả nút WPS, sau đó nhấn\"Xác nhận\"";
                    frmOK.ShowDialog();

                    SendAndChk(PortType.SSH, "mt gpio dump all", keyword, out res, 0, 3000);
                    if (res.Contains("WPS: high"))
                    {
                        released = true;
                        DisplayMsg(LogType.Log, "Check WPS Button released ok");
                    }
                    if (pressed && released)
                    {
                        AddData(item, 0);
                        DisplayMsg(LogType.Log, "Check WPSButton Pass");
                        break;
                    }
                }
                if (!pressed || !released)
                {
                    DisplayMsg(LogType.Log, "Check WPS button fail");
                    AddData(item, 1);
                }
                #endregion
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                AddData(item, 1);
            }
        }
        private void ResetButton()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            string res = "";
            string item = "ResetButton";
            string keyword = @"root@OpenWrt";

            try
            {

                #region RESET
                DisplayMsg(LogType.Log, "=============== Reset Button ===============");

                bool pressed = false;
                bool released = false;
                for (int i = 0; i < 3; i++)
                {
                    frmOK.Label = "Nhấn và giữ nút Reset, sau đó nhấn \"Xác nhận\"";
                    frmOK.ShowDialog();

                    SendAndChk(PortType.SSH, "mt gpio dump all", keyword, out res, 0, 3000);
                    if (res.Contains("RESET: low"))
                    {
                        pressed = true;
                        DisplayMsg(LogType.Log, "Check Reset Button pressed ok");
                    }

                    frmOK.Label = "Nhả nút Reset, sau đó nhấn\"Xác nhận\"";
                    frmOK.ShowDialog();

                    SendAndChk(PortType.SSH, "mt gpio dump all", keyword, out res, 0, 3000);
                    if (res.Contains("RESET: high"))
                    {
                        released = true;
                        DisplayMsg(LogType.Log, "Check Reset Button released ok");
                    }
                    if (pressed && released)
                    {
                        AddData(item, 0);
                        DisplayMsg(LogType.Log, "Check ResetButton Pass");
                        break;
                    }
                }
                if (!pressed || !released)
                {
                    DisplayMsg(LogType.Log, "Check Reset button fail");
                    AddData(item, 1);
                }
                #endregion

                //When using standard adaptor, the AC_ALARM is low.
                //When using customized adaptor or power supply, the AC_ALARM is high.
                DisplayMsg(LogType.Log, "=============== Battery Detection ===============");
                item = "BatteryDetection";
                if (res.Contains("AC_ALARM: low"))
                {
                    AddData(item, 0);
                    DisplayMsg(LogType.Log, "got AC_ALARM: low => Battery detection Pass");
                }
                else
                {
                    AddData(item, 1);
                    DisplayMsg(LogType.Log, "Battery detection fail");
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                AddData(item, 1);
            }
        }
        private void USBTest()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            string res = "";
            string item = "USB30Test";
            string keyword = @"root@OpenWrt";

            try
            {
                DisplayMsg(LogType.Log, "=============== USB3.0 Test ===============");

                if (isLoop == 0)
                {
                    frmOK.Label = "Vui lòng xác nhận đã cắm USB 3.0 vào";
                    frmOK.ShowDialog();
                }

                SendAndChk(PortType.SSH, "mount -t vfat /dev/sda1 /mnt/", keyword, out res, 1000, 5000);
                if (res.Contains("No such file or directory"))
                {
                    DisplayMsg(LogType.Log, "mount usb fail");
                    AddData(item, 1);
                    return;
                }
                for (int i = 0; i < 3; i++)
                {
                    SendAndChk(PortType.SSH, "mount | grep \"/mnt\"", keyword, out res, 0, 10000);

                    if (res.Contains("/dev/sda1"))
                    {
                        DisplayMsg(LogType.Log, $"mount usb ok! \r\n {res}");
                        break;
                    }
                }
                if (!res.Contains("/dev/sda1"))
                {
                    DisplayMsg(LogType.Log, "mount usb fail");
                    AddData(item, 1);
                    return;
                }

                SendAndChk(PortType.SSH, "echo \"usb test\" > /mnt/test", keyword, out res, 0, 3000);
                if (!SendAndChk(PortType.SSH, "cat /mnt/test", "usb test", out res, 0, 3000))
                {
                    DisplayMsg(LogType.Log, "usb write/read fail");
                    AddData(item, 1);
                    return;
                }
                AddData(item, 0);
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                AddData(item, 1);
            }
            finally
            {
                SendAndChk(PortType.SSH, "rm -f /mnt/test;sync", keyword, out res, 0, 3000);
                SendAndChk(PortType.SSH, "umount /mnt/", keyword, out res, 0, 3000);
            }
        }
        private void SLICTest()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            string res = "";
            string item = "SLICTest";
            string keyword = @"root@OpenWrt";

            try
            {
                DisplayMsg(LogType.Log, "=============== SLIC Test ===============");

                if (!SendAndChk(PortType.SSH, "proslic_api_demo", "Demo: Linux SPI Dev Connected", out res, 0, 5000))
                {
                    AddData(item, 1);
                    DisplayMsg(LogType.Log, "proslic_api_demo fail");
                    return;
                }

                if (!SendAndChk(PortType.SSH, "", "05) Ringing Menu", out res, 0, 10000))
                {
                    AddData(item, 1);
                    DisplayMsg(LogType.Log, "Check ProSLIC Menu fail");
                    return;
                }

                SendAndChk(PortType.SSH, "5", "Stop Ringing", out res, 0, 3000);

                //Start Ringing
                SendAndChk(PortType.SSH, "0", "Stop Ringing", out res, 0, 3000);
                frmYN.Label = "不需要接起電話,只要確認電話是否有響鈴?";
                frmYN.ShowDialog();
                if (frmYN.no)
                {
                    AddData(item, 1);
                    DisplayMsg(LogType.Log, "Start ringing fail");
                    return;
                }
                else
                {
                    DisplayMsg(LogType.Log, "Start ringing pass");
                }

                //Stop Ringing
                SendAndChk(PortType.SSH, "1", "Stop Ringing", out res, 0, 3000);
                frmYN.Label = "確認電話鈴聲已停止?";
                frmYN.ShowDialog();
                if (frmYN.no)
                {
                    AddData(item, 1);
                    DisplayMsg(LogType.Log, "Stop ringing fail");
                    return;
                }
                else
                {
                    DisplayMsg(LogType.Log, "Stop ringing pass");
                }

                //exit Ringing Menu
                SendAndChk(PortType.SSH, "q", "09) Interrupt Menu", out res, 0, 3000);
                SendAndChk(PortType.SSH, "9", "09) Pulse digit decode/hook flash demo", out res, 0, 3000);

                DisplayMsg(LogType.Cmd, $"Write '9' to ssh");
                SSH_stream.WriteLine("9");
                ChkResponse(PortType.SSH, ITEM.NONE, "detected:", out res, 3000);

                frmOK.Label = "請舉起話機後按\"確定\"";
                frmOK.ShowDialog();
                if (!ChkResponse(PortType.SSH, ITEM.NONE, "OFFHOOK", out res, 10000))
                {
                    AddData(item, 1);
                    DisplayMsg(LogType.Log, "Check OFFHOOK fail");
                    return;
                }
                else
                {
                    DisplayMsg(LogType.Log, "Check OFFHOOK pass");
                }

                frmOK.Label = "請掛掉話機後按\"確定\"";
                frmOK.ShowDialog();
                if (!ChkResponse(PortType.SSH, ITEM.NONE, "ONHOOK", out res, 10000))
                {
                    AddData(item, 1);
                    DisplayMsg(LogType.Log, "Check ONHOOK fail");
                    return;
                }
                else
                {
                    DisplayMsg(LogType.Log, "Check ONHOOK pass");
                }
                AddData(item, 0);
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                AddData(item, 1);
            }
            finally
            {
                SendCommand(PortType.SSH, sCtrlC, 0);
                ChkResponse(PortType.SSH, ITEM.NONE, keyword, out res, 3000);
            }
        }


        bool CheckNvram(string data, string keyword)
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
            return true;
        }
        private void SLICTest_ByUsbModem()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            int retry_cnt = 3;
            string res = "";
            string item = "SLICTest";
            string keyword = @"root@OpenWrt";
            var comInfo1 = WNC.API.Func.ReadINI("Setting", "Port", "Modem_1_COM", "COM8");
            EzComport com1 = null;

            try
            {
                DisplayMsg(LogType.Log, "=============== SLIC Test ===============");

                if (!SendAndChk(PortType.SSH, "proslic_api_demo", "Demo: Linux SPI Dev Connected", out res, 0, 5000))
                {
                    AddData(item, 1);
                    DisplayMsg(LogType.Log, "proslic_api_demo fail");
                    return;
                }

                if (!SendAndChk(PortType.SSH, "", "05) Ringing Menu", out res, 0, 10000))
                {
                    AddData(item, 1);
                    DisplayMsg(LogType.Log, "Check ProSLIC Menu fail");
                    return;
                }

                SendAndChk(PortType.SSH, "5", "Stop Ringing", out res, 0, 3000);
                //預設響鈴方式為一聲響到底,UsbModem判斷會有問題,改成斷斷續續的響法後方便判斷
                if (!SendAndChk(PortType.SSH, "3", "Ring Cadence Timers (enabled)", out res, 0, 3000)) //更改響鈴方式
                {
                    DisplayMsg(LogType.Log, "Enable Ring Cadence fail");
                    AddData(item, 1);
                    return;
                }

                //UsbModem initial 
                com1 = new EzComport();
                com1.ComportMessageDumped += MessageOut;
            openmodem:
                if (!com1.OpenComport(comInfo1, 115200))
                {
                    DisplayMsg(LogType.Log, $"Initial UsbModem {comInfo1} fail");
                    if (retry_cnt > 0)
                    {
                        retry_cnt--;
                        goto openmodem;
                    }
                    AddData(item, 1);
                    return;
                }

                //Start Ringing
                SendAndChk(PortType.SSH, "0", "Stop Ringing", out res, 0, 3000);
                //check ring via UsbModem
                retry_cnt = 3;
            retry:

                //com1.LogFile = "myModem.txt";
                //com1.AutoLog = true;

                if (com1.WaitFor("RING", 40))
                {
                    DisplayMsg(LogType.Log, "Check 'RING' pass");
                }
                else
                {
                    DisplayMsg(LogType.Log, "Wait 'RING' fail");
                    if (retry_cnt > 0)
                    {
                        retry_cnt--;
                        goto retry;
                    }
                    AddData(item, 1);
                    return;
                }

                //Stop Ringing
                SendAndChk(PortType.SSH, "1", "Stop Ringing", out res, 0, 3000);

                //exit Ringing Menu
                SendAndChk(PortType.SSH, "q", "09) Interrupt Menu", out res, 0, 3000);
                SendAndChk(PortType.SSH, "9", "09) Pulse digit decode/hook flash demo", out res, 0, 3000);

                DisplayMsg(LogType.Cmd, $"Write '9' to ssh");
                SSH_stream.WriteLine("9");
                ChkResponse(PortType.SSH, ITEM.NONE, "detected:", out res, 3000);

                //舉起話機
                DisplayMsg(LogType.Cmd, $"Write 'ath1' to UsbModem");
                // [3.2] Adjust the SLIC test sequence, ringingèoff hookèoh hook by Stanley
                // [3.5] Adjust the SLIC test sequence, ringingèoff hookèoh hook by Stanley
                com1.WriteLine("ath1", 1000);

                if (!ChkResponse(PortType.SSH, ITEM.NONE, "OFFHOOK", out res, 10000))
                {
                    AddData(item, 1);
                    DisplayMsg(LogType.Log, "Check OFFHOOK fail");
                    return;
                }
                else
                {
                    DisplayMsg(LogType.Log, "Check OFFHOOK pass");
                }

                //掛掉話機
                DisplayMsg(LogType.Cmd, $"Write 'ath0' to UsbModem");
                com1.WriteLine("ath0", 1000);

                if (!ChkResponse(PortType.SSH, ITEM.NONE, "ONHOOK", out res, 10000))
                {
                    AddData(item, 1);
                    DisplayMsg(LogType.Log, "Check ONHOOK fail");
                    return;
                }
                else
                {
                    DisplayMsg(LogType.Log, "Check ONHOOK pass");
                }

                AddData(item, 0);
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                AddData(item, 1);
            }
            finally
            {
                if (com1 != null)
                    com1.Close();
                SendCommand(PortType.SSH, sCtrlC, 0);
                ChkResponse(PortType.SSH, ITEM.NONE, keyword, out res, 3000);
            }
        }
        private void MessageOut(object sender, EzComportMessageDumpedEventArgs e)
        {
            DisplayMsg(LogType.UsbModem, e.Message);
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
        private void CheckFWVerAndHWID()
        {
            if (!CheckGoNoGo() || isGolden)
            {
                return;
            }

            string item = "ChkFWVer";
            string keyword = @"root@OpenWrt";
            string res = "";
            string FWversion = "";
            string HWID = "";

            try
            {
                DisplayMsg(LogType.Log, "=============== Check FW version & HW ID ===============");

                SendAndChk(PortType.SSH, "mt info", keyword, out res, 0, 3000);
                Match m = Regex.Match(res, @"FW Version: (?<FWver>.+)");
                if (m.Success)
                {
                    FWversion = m.Groups["FWver"].Value.Trim();
                    string a = FWversion.Split('v')[1];
                }

                DisplayMsg(LogType.Log, "DUT FWversion: " + FWversion);

                if (Convert.ToInt32(Func.ReadINI("Setting", "CheckInfo", "FWvercheckbysetting", "0")) == 1)
                {

                    DisplayMsg(LogType.Log, "Current Setting Check enable [CheckInfo] FWvercheckbysetting=1 ");
                    string settingfwver = string.Empty;
                    settingfwver = Func.ReadINI("Setting", "CheckInfo", "FWver", "XXXX");
                    DisplayMsg(LogType.Log, "Setting FW version:" + settingfwver);
                    if (string.Compare(FWversion, settingfwver, true) == 0)
                    {
                        AddData(item, 0);
                        DisplayMsg(LogType.Log, "Check FW Version with setting PASS");
                        status_ATS.AddDataRaw("LRG1_MFG_FW_VER", FWversion, FWversion, "000000");
                    }
                    else
                    {
                        AddData(item, 1);
                        DisplayMsg(LogType.Log, "Check FW Version with setting fail");
                    }
                }
                else
                {
                    DisplayMsg(LogType.Log, "Current Check with SFCS");
                    DisplayMsg(LogType.Log, "SFCS_FWversion:" + infor.FWver);
                    if (string.Compare(FWversion, infor.FWver, true) == 0)
                    {
                        AddData(item, 0);
                        DisplayMsg(LogType.Log, "Check FW Version With SFCS PASS");
                        status_ATS.AddDataRaw("LRG1_MFG_FW_VER", FWversion, FWversion, "000000");
                    }
                    else
                    {
                        AddData(item, 1);
                        DisplayMsg(LogType.Log, "Check FW Version with SFCS fail");
                    }
                }

                //check HW ID
                item = "ChkHWID";
                //HW Version (GPIO): 1001
                m = Regex.Match(res, @"HW Version \(GPIO\): (?<HWID>.+)");
                if (m.Success)
                {
                    HWID = m.Groups["HWID"].Value.Trim();
                }

                DisplayMsg(LogType.Log, "HWID: " + HWID);
                DisplayMsg(LogType.Log, "SFCS_HWID:" + infor.HWID);


                if (string.Compare(HWID, infor.HWID, true) == 0)
                {
                    AddData(item, 0);
                    DisplayMsg(LogType.Log, "Check HW ID PASS");
                    status_ATS.AddDataRaw("LRG1_HW_ID", HWID, HWID, "000000");
                }
                else
                {
                    AddData(item, 1);
                    DisplayMsg(LogType.Log, "Check HW ID fail");
                }

            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                AddData(item, 1);
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
                DisplayMsg(LogType.Log, "Delay 2s..");
                Thread.Sleep(2000);
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
            catch (Exception camera)
            {
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
        private void USB30()
        {
            if (!CheckGoNoGo())
            {
                return;
            }
            string item = "USB3p0";
            string res = string.Empty;
            try
            {
                if (!SendAndChk(PortType.SSH, "cat /sys/bus/usb/devices/2-1/speed", "5000", out res, 0, 3000))
                {
                    DisplayMsg(LogType.Log, "check usb speed fail");
                    AddData(item, 1);
                    return;
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                warning = "Exception USB30 speed failed!!!";
            }
        }
        private bool ResetDect()
        {
            bool IsResetOK = false;
            string res = string.Empty;
            string item = "Reset__Dect";
            string keyword = "root@OpenWrt";
            try
            {
                SendAndChk(PortType.SSH, "qqqqq\r\n", keyword, out res, 0, 2500);
                //do
                //{
                //    //SendCommand(PortType.SSH, sCtrlZ, 2000); //SendKeys.SendWait("{^Z}");
                //    if (SendAndChk(PortType.SSH, "qqqqq\r\n", keyword, out res, 0, 2000))
                //    { break; }
                //} while (ChkResponse(PortType.SSH, ITEM.NONE, keyword, out res, 3000));
                //Reboot DECT
                DisplayMsg(LogType.Log, "Reboot DECT");
                SendAndChk(PortType.SSH, "echo 0 > /sys/class/gpio/dect_rst/value", keyword, out res, 0, 3000);
                Thread.Sleep(1800);
                SendAndChk(PortType.SSH, "echo 1 > /sys/class/gpio/dect_rst/value", keyword, out res, 0, 3000);
                DisplayMsg(LogType.Log, "Delay 3s...");
                Thread.Sleep(2800);
                IsResetOK = true;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData(item, 1);
            }
            return IsResetOK;
        }

    }
}
