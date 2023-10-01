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
//using WNC.API;
//using EasyLibrary;
//using NationalInstruments.VisaNS;
using System.Net.Security;
using System.Runtime.CompilerServices;

namespace MiniPwrSupply.LRG1
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
                //fixture.ControlIO(Fixture.FixtureIO.IO_6, CTRL.ON);
                //fixture.ControlIO(Fixture.FixtureIO.IO_7, CTRL.ON);
                //fixture.ControlIO(Fixture.FixtureIO.IO_8, CTRL.ON);
            }

            try
            {
                infor.ResetParam();
                Net.NewNetPort newNetPort = new Net.NewNetPort();
                #region create SMT file

                //if (status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode)
                {
                    //SE_TODO: get infor from SFCS
                    SentPsnForGetMAC(status_ATS.txtPSN.Text.Trim());
                    for (int i = 0; i < 3; i++)
                    {
                        DisplayMsg(LogType.Log, "Delay 1s...");
                        Thread.Sleep(1000);

                        infor.SerialNumber = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@LRG1_SN");
                        infor.BaseMAC = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@MAC");
                        infor.FWver = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@LRG1_MFG_FW");

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
                    string result = string.Empty;
                    result = infor.FWver.Substring(0, infor.FWver.Length - 9);

                    DisplayMsg(LogType.Log, $"Get FWver trim is: LRG1_v{result}");
                    infor.FWver = "LRG1_v" + result;
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

                CheckFWVerAndHWID();

                if (Func.ReadINI("Setting", "PCBA", "SkipNFC", "0") == "0")
                {
                    NFCTag();
                }

                if (Func.ReadINI("Setting", "PCBA", "SkipDECT", "0") == "0")
                {
                    Set_DECT_Full_Power();

                    string rxtun = "";
                    DECTCal(ref rxtun);
                    infor.DECT_cal_rxtun = rxtun;
                    DisplayMsg(LogType.Log, "DECT_cal_rxtun: " + infor.DECT_cal_rxtun);

                    Set_DECT_ID();
                    Set_DECT_RFPI(); //Rena_20230803, EEProm Param Set RFPI
                    // ===================================================================
                    this.EEProm_Set(); //testplan0922,    v. EEPROM set (PA2_COMP) => remove by v037 testplan0925
                    // ===================================================================
                }

                //SLICTest();
                SLICTest_ByUsbModem();

                if (forHQtest || (status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode && !isGolden))
                {
                    SetDUTInfo();
                    CheckDUTInfo();
                }

                if (isLoop == 0)
                {
                    CheckLED();
                }

                CheckPCIe();

                if (isLoop == 0)
                {
                    WPSButton();
                    ResetButton();
                }


                USB30Test();

                if (isLoop == 0)
                {
                    EthernetTest(true);
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
                if (status_ATS._testMode == StatusUI2.StatusUI.TestMode.EngMode)
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
            string keyword = "root@OpenWrt:~# \r\n";
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
            finally
            {
                //exit calibration mode
                for (int i = 0; i < 5; i++)
                {
                    if (SendAndChk(PortType.SSH, "q", keyword, out res, 0, 2000))
                        break;
                }
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
            string keyword = @"root@OpenWrt";
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

            string item = "SetDECTFullPower";
            string res = string.Empty;
            string keyword = "root@OpenWrt:~# \r\n";

            try
            {
                bool result = false;
                int delayMs = 0;
                int timeOutMs = 10 * 1000;
                string Full_power_value = "FE";

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

                //exit calibration mode
                for (int i = 0; i < 5; i++)
                {
                    /*if (SendAndChk(PortType.SSH, "q\r\n", keyword, out res, 0, 12000))
                        break;*/
                    SendAndChk(PortType.SSH, "q\r\n", "", out res, 0, 12000);
                    if (SendAndChk(PortType.SSH, "\r\n", keyword, out res, 0, 12000))
                    { break; }
                }

                //Reboot DECT
                DisplayMsg(LogType.Log, "Reboot DECT");
                SendAndChk(PortType.SSH, "echo 0 > /sys/class/gpio/dect_rst/value", keyword, out res, 0, 3000);
                Thread.Sleep(2000);
                SendAndChk(PortType.SSH, "echo 1 > /sys/class/gpio/dect_rst/value", keyword, out res, 0, 3000);
                DisplayMsg(LogType.Log, "Delay 3s...");
                Thread.Sleep(3000);
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData(item, 1);
            }
            finally
            {
                //exit calibration mode
                for (int i = 0; i < 5; i++)
                {
                    if (SendAndChk(PortType.SSH, "q", keyword, out res, 0, 2000))
                        break;
                }
            }
        }
        private void EEProm_Set()     // (PA2_COMP)
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            DisplayMsg(LogType.Log, "=============== Set EEProm_Param ===============");

            string item = "PA2_COMP";
            string res = string.Empty;
            string keyword = "root@OpenWrt:~# \r\n";

            try
            {
                bool result = false;
                int delayMs = 0;
                int timeOutMs = 10 * 1000;

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

                DisplayMsg(LogType.Log, "Write x to ssh");
                SSH_stream.Write("x\r");
                ChkResponse(PortType.SSH, ITEM.NONE, "q) Quit", out res, timeOutMs);

                if (!SendWithoutEnterAndChk(PortType.SSH, "x", "Enter Location (dec):", delayMs, timeOutMs))
                {
                    DisplayMsg(LogType.Log, "Modify DECT EEPROM fail");
                    AddData(item, 1);
                    return;
                }
                DisplayMsg(LogType.Log, $"Write '178' to ssh");
                SSH_stream.WriteLine("178");
                if (!ChkResponse(PortType.SSH, ITEM.NONE, "Enter Length (dec. max 512):", out res, timeOutMs))
                {
                    DisplayMsg(LogType.Log, "Modify DECT EEPROM fail");
                    AddData(item, 1);
                    return;
                }
                DisplayMsg(LogType.Log, $"Write '1' to ssh");
                SSH_stream.WriteLine("1");
                if (!ChkResponse(PortType.SSH, ITEM.NONE, "Enter New Data (hexadecimal):", out res, timeOutMs))
                {
                    DisplayMsg(LogType.Log, "Modify DECT EEPROM fail");
                    AddData(item, 1);
                    return;
                }
                DisplayMsg(LogType.Log, "Write a0 to ssh");
                SSH_stream.WriteLine("a0");
                if (!ChkResponse(PortType.SSH, ITEM.NONE, "CURRENT VALUE:", out res, timeOutMs))
                {
                    DisplayMsg(LogType.Log, "Modify DECT EEPROM fail");
                    AddData(item, 1);
                    return;
                }
                if (!res.Contains("A0"))
                {
                    DisplayMsg(LogType.Log, "Check 'CURRENT VALUE: a0' fail");
                    AddData(item, 1);
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, @"EEProm_Set=>" + ex.Message);
                AddData(item, 1);
            }
            finally
            {
                //exit calibration mode
                for (int i = 0; i < 5; i++)
                {
                    if (SendAndChk(PortType.SSH, "q", keyword, out res, 0, 2000))
                        break;
                }
            }
        }
        private void Set_DECT_ID()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            DisplayMsg(LogType.Log, "=============== Set DECT ID ===============");

            string item = "SetDECTID";
            string res = string.Empty;
            string keyword = "root@OpenWrt:~# \r\n";

            try
            {
                bool result = false;
                int delayMs = 0;
                int timeOutMs = 10 * 1000;

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
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData(item, 1);
            }
            finally
            {
                //exit calibration mode
                for (int i = 0; i < 5; i++)
                {
                    if (SendAndChk(PortType.SSH, "q", keyword, out res, 0, 2000))
                        break;
                }
            }
        }
        private void DECTCal(ref string RXTUNE)
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            DisplayMsg(LogType.Log, "=============== 5.3.3 DECT Calibration ===============");

            string item = "DECTCal";
            string res = string.Empty;
            string keyword = "root@OpenWrt:~# \r\n";

            try
            {
                string DECTver = "";
                string cmd = string.Empty;
                //string offset_val = "60";
                string offset_val = Func.ReadINI("Setting", "PCBA", "DECT_Default_RXTUN", "70");
                //string version = Func.ReadINI("Setting", "Parameter", "DECT_Version", "ERROR");
                //double freqHz = 13824000;  //Freq. = 13.824 MHz
                double freqHz = 1888356500; //1888.3565 MHz
                double pwr = -999;
                double pwrThreshold = Convert.ToInt32(Func.ReadINI("Setting", "DECT_SignalAnalyzer", "PwrThreshold", "0"));
                double outFreqHz = 0;
                //double tolerance = 5;
                double tolerance = 1500;
                double freqDeltaHz = 0;
                double delta = 0;

                bool result = false;
                int delayMs = 0;
                int timeOutMs = 10 * 1000;
                RXTUNE = "";

                byte[] data = new byte[] { };

                DateTime dt;
                TimeSpan ts;

                if (!DECTSignalAnalyzerPresetting(1888704000))
                {
                    warning = "Initial Spectrum fail";
                    return;
                }

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
                }
                else
                {
                    //更新DECT FW
                    //if (Func.ReadINI("Setting", "PCBA", "DoDECTFWUpgrade", "0") == "1")
                    //{
                    //    if (!UpgradeDECTFW())
                    //    {
                    //        return;
                    //    }
                    //}
                    //else
                    {
                        AddData("DECT_Ver", 1);
                        DisplayMsg(LogType.Log, "Check DECT version fail");
                        return;
                    }
                }
                #endregion

                //Rena_20230524, change RXTUN default value as 70
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

                    //exit calibration mode
                    for (int i = 0; i < 5; i++)
                    {
                        if (SendAndChk(PortType.SSH, "q", keyword, out res, 0, 2000))
                            break;
                    }

                    //Reboot DECT
                    DisplayMsg(LogType.Log, "Reboot DECT");
                    SendAndChk(PortType.SSH, "echo 0 > /sys/class/gpio/dect_rst/value", keyword, out res, 0, 3000);
                    Thread.Sleep(2000);
                    SendAndChk(PortType.SSH, "echo 1 > /sys/class/gpio/dect_rst/value", keyword, out res, 0, 3000);
                    DisplayMsg(LogType.Log, "Delay 3s...");
                    Thread.Sleep(3000);
                }
                #endregion

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

                DisplayMsg(LogType.Log, "Write x to ssh");
                SSH_stream.Write("x\r");
                ChkResponse(PortType.SSH, ITEM.NONE, "q) Quit", out res, timeOutMs);

                //Rena_20230524, check RXTUN default value
                #region check_RXTUN_default
                if (true)
                {
                    //check RXTUN default value
                    RXTUNE = ParseRXTUNE(res).Trim();
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

                //continuous TX
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
                int c = 0;
                while (true)
                {
                    ts = new TimeSpan(DateTime.Now.Ticks - dt.Ticks);
                    if (ts.TotalMilliseconds > 180 * 1000)
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

                    //Rena_20230414, disable for LRG1 HQ sample build
                    if (false)// need to check env in V2 to set threshold
                    {
                        int delay = Convert.ToInt32(Func.ReadINI("Setting", "DECT_SignalAnalyzer", "DelayPower", "0"));
                        FetchPower(out pwr, delay);
                        DisplayMsg(LogType.Log, "Power: " + pwr.ToString());

                        if (pwr < pwrThreshold)
                        {
                            DisplayMsg(LogType.Warning, "Under power threshold : " + pwr.ToString());
                            continue;
                        }
                    }

                    freqDeltaHz = outFreqHz - freqHz;
                    DisplayMsg(LogType.Log, "freqDeltaHz: " + freqDeltaHz.ToString());
                    DisplayMsg(LogType.Log, "freqHz: " + outFreqHz.ToString());
                    DisplayMsg(LogType.Log, "Tolerence: " + tolerance.ToString());

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
                    /*Rena_20230707,LRG1 DECT會先寫預設值,所以調整flow
                    delta = freqDeltaHz / 6;
                    delta = Math.Round(delta, 0);
                    DisplayMsg(LogType.Log, "Shift " + delta.ToString() + " times..");
                    delta = Math.Abs(delta);

                    #region Jed modify flow 2023/02/08
                    if (c <= 2)
                    {
                        c++;
                        string rxtune = RXTUNE.Replace("0x", "");
                        int decRxtune = int.Parse(rxtune, System.Globalization.NumberStyles.HexNumber);
                        int step = (int)delta + decRxtune;
                        DisplayMsg(LogType.Log, "Rxtune in decimal:" + decRxtune);
                        DisplayMsg(LogType.Log, "Rxtune + step in decimal:" + step);
                        //if (decRxtune < 112 || decRxtune > 144) //LS04
                        if (decRxtune < 96 || decRxtune > 128) //LRG1 EVT 暫定值
                        {
                            DisplayMsg(LogType.Log, $"{c}==========> Measure freqency again.");
                            DisplayMsg(LogType.Log, $"Delay 3s...");
                            Thread.Sleep(3000);
                            continue;
                        }
                    }
                    #endregion

                    for (int i = 0; i < delta; i++)*/
                    {
                        DisplayMsg(LogType.Log, $"Write '{cmd}' to ssh");
                        SSH_stream.Write(cmd);
                        if (!ChkResponse(PortType.SSH, ITEM.NONE, "RXTUN: ", out res, timeOutMs))
                        {
                            AddData(item, 1);
                            return;
                        }
                        //Thread.Sleep(1000);
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
                for (int i = 0; i < 5; i++)
                {
                    if (SendAndChk(PortType.SSH, "q", keyword, out res, 0, 2000))
                        break;
                }
                SendAndChk(PortType.SSH, "cd ~", keyword, out res, 0, 3000);
            }
        }
        //Rena_20230705,先保留原做法
        private void DECTCal_org(ref string RXTUNE)
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            DisplayMsg(LogType.Log, "=============== DECT Calibration ===============");

            string item = "DECTCal";
            string res = string.Empty;
            string keyword = @"root@OpenWrt";

            try
            {
                string DECTver = "";
                string cmd = string.Empty;
                string version = Func.ReadINI("Setting", "Parameter", "DECT_Version", "ERROR");
                double freqHz = 13824000;
                double pwr = -999;
                double pwrThreshold = Convert.ToInt32(Func.ReadINI("Setting", "DECT_SignalAnalyzer", "PwrThreshold", "0"));
                double outFreqHz = 0;
                double tolerance = 5;
                double freqDeltaHz = 0;
                double delta = 0;

                bool result = false;
                int delayMs = 0;
                int timeOutMs = 10 * 1000;
                RXTUNE = "";

                byte[] data = new byte[] { };

                DateTime dt;
                TimeSpan ts;

                if (!DECTSignalAnalyzerPresetting(freqHz))
                {
                    warning = "Initial Spectrum fail";
                    return;
                }

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
                }
                else
                {
                    if (Func.ReadINI("Setting", "PCBA", "DoDECTFWUpgrade", "0") == "1")
                    {
                        if (!UpgradeDECTFW())
                        {
                            return;
                        }
                    }
                    else
                    {
                        AddData("DECT_Ver", 1);
                        DisplayMsg(LogType.Log, "Check DECT version fail");
                        return;
                    }
                }

                //Rena_20230524, change RXTUN default value as 70 for HQ sample build(TODO)
                #region set_RXTUN_default
                //s -> 2 -> 7 -> 26 -> 1 -> 70 -> enter
                //q (回到主選單)
                //繼續開始calibration流程
                if (Func.ReadINI("Setting", "PCBA", "DECTDefaultChange", "0") == "1")
                {
                    SendWithoutEnterAndChk(PortType.SSH, "s", "q => Return to Interface Menu", delayMs, timeOutMs);
                    SendWithoutEnterAndChk(PortType.SSH, "2", "q => Return", delayMs, timeOutMs);
                    if (!SendWithoutEnterAndChk(PortType.SSH, "7", "Enter Location (dec):", delayMs, timeOutMs))
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

                    DisplayMsg(LogType.Log, $"Write '70' to ssh");
                    SSH_stream.WriteLine("70");
                    if (!ChkResponse(PortType.SSH, ITEM.NONE, "CURRENT VALUE:", out res, timeOutMs))
                    {
                        DisplayMsg(LogType.Log, "Modify RXTUN fail");
                        AddData(item, 1);
                        return;
                    }

                    if (!SendWithoutEnterAndChk(PortType.SSH, "q", "q => Return to Interface Menu", delayMs, timeOutMs))
                    {
                        DisplayMsg(LogType.Log, "Modify RXTUN fail");
                        AddData(item, 1);
                        return;
                    }
                    if (!SendWithoutEnterAndChk(PortType.SSH, "q", "q => Quit", delayMs, timeOutMs))
                    {
                        DisplayMsg(LogType.Log, "Modify RXTUN fail");
                        AddData(item, 1);
                        return;
                    }
                }
                #endregion

                //SendWithoutEnterAndChk(PortType.SSH, "x", "q) Quit", delayMs, timeOutMs);
                DisplayMsg(LogType.Log, "Write x to ssh");
                SSH_stream.Write("x\r");
                ChkResponse(PortType.SSH, ITEM.NONE, "q) Quit", out res, timeOutMs);

                //Rena_20230524, check RXTUN default value for HQ sample build(TODO)
                #region check_RXTUN_default
                if (Func.ReadINI("Setting", "PCBA", "DECTDefaultChange", "0") == "1")
                {
                    //check RXTUN default value
                    RXTUNE = ParseRXTUNE(res).Trim();
                    if (RXTUNE != "70")
                    {
                        DisplayMsg(LogType.Log, "Check RXTUN=70 fail");
                        AddData(item, 1);
                        return;
                    }
                    else
                    {
                        DisplayMsg(LogType.Log, "Check RXTUN=70 pass");
                    }
                }
                #endregion

                SendWithoutEnterAndChk(PortType.SSH, "c", "(0xFF for None):", delayMs, timeOutMs);

                DisplayMsg(LogType.Log, "Write 07 to ssh");
                SSH_stream.Write("07\r");
                result = ChkResponse(PortType.SSH, ITEM.NONE, "RXTUN:", out res, timeOutMs);
                RXTUNE = ParseRXTUNE(res).Trim();
                if (RXTUNE.Length == 0)
                {
                    warning = "RXTUN is empty";
                    return;
                }

                //Rena_20230414, for HQ sample build
                if (forHQtest)
                {
                    frmOK.Label = "請將探針接觸DECT Crystal後按OK";
                    frmOK.ShowDialog();
                    Thread.Sleep(5000);
                }

                dt = DateTime.Now;
                int c = 0;
                while (true)
                {
                    ts = new TimeSpan(DateTime.Now.Ticks - dt.Ticks);
                    if (ts.TotalMilliseconds > 180 * 1000)
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

                    //Rena_20230414, disable for LRG1 HQ sample build
                    if (false)// need to check env in V2 to set threshold
                    {
                        int delay = Convert.ToInt32(Func.ReadINI("Setting", "DECT_SignalAnalyzer", "DelayPower", "0"));
                        FetchPower(out pwr, delay);
                        DisplayMsg(LogType.Log, "Power: " + pwr.ToString());

                        if (pwr < pwrThreshold)
                        {
                            DisplayMsg(LogType.Warning, "Under power threshold : " + pwr.ToString());
                            continue;
                        }
                    }

                    freqDeltaHz = outFreqHz - freqHz;
                    DisplayMsg(LogType.Log, "freqDeltaHz: " + freqDeltaHz.ToString());
                    DisplayMsg(LogType.Log, "freqHz: " + outFreqHz.ToString());
                    DisplayMsg(LogType.Log, "Tolerence: " + tolerance.ToString());

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
                        status_ATS.AddData("CrystalFrequencyHz", "Hz", outFreqHz);
                        AddData(item, 0);
                        status_ATS.AddDataRaw("LRG1_DECT_RXTUN", RXTUNE, RXTUNE, "000000");
                        break;
                    }

                    delta = freqDeltaHz / 6;
                    delta = Math.Round(delta, 0);
                    DisplayMsg(LogType.Log, "Shift " + delta.ToString() + " times..");
                    delta = Math.Abs(delta);

                    #region Jed modify flow 2023/02/08
                    if (c <= 2)
                    {
                        c++;
                        string rxtune = RXTUNE.Replace("0x", "");
                        int decRxtune = int.Parse(rxtune, System.Globalization.NumberStyles.HexNumber);
                        int step = (int)delta + decRxtune;
                        DisplayMsg(LogType.Log, "Rxtune in decimal:" + decRxtune);
                        DisplayMsg(LogType.Log, "Rxtune + step in decimal:" + step);
                        //if (decRxtune < 112 || decRxtune > 144) //LS04
                        if (decRxtune < 96 || decRxtune > 128) //LRG1 EVT 暫定值
                        {
                            DisplayMsg(LogType.Log, $"{c}==========> Measure freqency again.");
                            DisplayMsg(LogType.Log, $"Delay 3s...");
                            Thread.Sleep(3000);
                            continue;
                        }
                    }
                    #endregion

                    for (int i = 0; i < delta; i++)
                    {
                        DisplayMsg(LogType.Log, $"Write '{cmd}' to ssh");
                        SSH_stream.Write(cmd);
                        if (!ChkResponse(PortType.SSH, ITEM.NONE, "RXTUN: ", out res, timeOutMs))
                        {
                            AddData(item, 1);
                            return;
                        }
                        Thread.Sleep(1000);
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
                //Rena_20230414, for HQ sample build
                if (forHQtest)
                {
                    frmOK.Label = "請移除探針";
                    frmOK.ShowDialog();
                }
                //exit calibration mode
                for (int i = 0; i < 5; i++)
                {
                    if (SendAndChk(PortType.SSH, "q", keyword, out res, 0, 2000))
                        break;
                }
                SendAndChk(PortType.SSH, "cd ~", keyword, out res, 0, 3000);
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
                double spanHz = Convert.ToDouble(Func.ReadINI("Setting", "DECT_SignalAnalyzer", "SpanHz", "0"));
                double rbwKHz = Convert.ToDouble(Func.ReadINI("Setting", "DECT_SignalAnalyzer", "RbwHz", "0"));
                double vbwKHz = Convert.ToDouble(Func.ReadINI("Setting", "DECT_SignalAnalyzer", "VbwHz", "0"));
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
                //#region replace_py
                //int index = 0;
                //bool MD5_check = false;
                ////Rena_20230714, replace gen_board_data.py(暫時做法,之後MFG FW進版後就不需要了)
                ////put gen_board_data.py in default_image_backup folder
                //if (!File.Exists(Path.Combine(defaults_img_path, "gen_board_data.py")))
                //{
                //    DisplayMsg(LogType.Log, $"File '{Path.Combine(defaults_img_path, "gen_board_data.py")}' doesn't exist");
                //    AddData(item, 1);
                //    return;
                //}
                //SendAndChk(PortType.SSH, $"tftp -gr gen_board_data.py {PC_IP}", keyword, out res, 0, 5000);
                //SendAndChk(PortType.SSH, "mv gen_board_data.py /lib/gen_board_data.py", keyword, out res, 0, 5000);
                //while (index++ < 5)
                //{
                //    SendAndChk(PortType.SSH, "md5sum /lib/gen_board_data.py", keyword, out res, 0, 5000);
                //    if (res.Contains(py_md5sum))
                //    {
                //        MD5_check = true;
                //        DisplayMsg(LogType.Log, "gen_board_data.py MD5 check pass");
                //        break;
                //    }
                //    else
                //    {
                //        DisplayMsg(LogType.Log, "gen_board_data.pyMD5 check fail");
                //        Thread.Sleep(1000);
                //    }
                //}
                //if (!MD5_check)
                //{
                //    AddData(item, 1);
                //    return;
                //}
                //#endregion
                //SE_TODO: get wifi_password, admin_password, wlan_ssid from SFCS
                //如果已經寫過SFCS有紀錄,就讀出SFCS的值後帶入,如果沒寫過就用renew
                //HQ sample build都直接用renew,生產時請SE修改
                string WiFi_SSID_ToWrite = "renew";
                string WiFi_PWD_ToWrite = "renew";
                string Admin_PWD_ToWrite = "renew";

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
                //SE_TODO: get wifi_password, admin_password, wlan_ssid from SFCS
                //如果已經寫過SFCS有紀錄,就讀出SFCS的值後帶入,如果沒寫過就用renew
                //HQ sample build都直接用renew,生產時請SE修改
                // ===================================
                // Markup for PCBA stress test
                // ===================================
                //SFCS_Query _sfcsQuery = new SFCS_Query();
                //ATS_Template.SFCS_ATS_2_0.ATS ss = new ATS_Template.SFCS_ATS_2_0.ATS();
                //string WiFi_SSID_ToWrite = string.Empty;
                //string WiFi_PWD_ToWrite = string.Empty;
                //string Admin_PWD_ToWrite = string.Empty;

                //if (!_sfcsQuery.Get15Data(status_ATS.txtPSN.Text, "LRG1_LABEL_NETWORK", ref WiFi_SSID_ToWrite)) { WiFi_SSID_ToWrite = "renew"; }
                //if (!_sfcsQuery.Get15Data(status_ATS.txtPSN.Text, "LRG1_LABEL_PW", ref WiFi_PWD_ToWrite)) { WiFi_PWD_ToWrite = "renew"; }
                //if (!_sfcsQuery.Get15Data(status_ATS.txtPSN.Text, "LRG1_LABEL_ADMIN_PW", ref Admin_PWD_ToWrite)) { Admin_PWD_ToWrite = "renew"; }

                ////假設wlan_ssid=BT-JHCFPT,gen_board_data.sh時只需帶JHCFPT
                ////依客戶要求,EVT3-2 wlan_ssid改為EE-XXXXXX
                //WiFi_SSID_ToWrite = WiFi_SSID_ToWrite.Replace("BT-", "").Replace("EE-", "");

                //Generate Board data
                //BaseMAC & DECT_rfpi are capital letters
                //SendAndChk(PortType.SSH, $"gen_board_data.sh {SerialNumber} {infor.HWver} {infor.BaseMAC.ToUpper()} {infor.DECT_rfpi.ToUpper()} {infor.DECT_cal_rxtun} {WiFi_PWD_ToWrite} {Admin_PWD_ToWrite} {WiFi_SSID_ToWrite}", keyword, out res, 0, 10000);
                //if (!res.Contains(keyword) || res.IndexOf("Error", StringComparison.OrdinalIgnoreCase) >= 0)
                //{
                //    DisplayMsg(LogType.Log, "Generate board data fail");
                //    AddData(item, 1);
                //    return;
                //}
                // ===================================
                // ===================================
                // ===================================
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

        private void EthernetTest(bool write_mac)
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            int retry_cnt;
            string item = "EthernetTest";
            string keyword = @"root@OpenWrt";
            string res = "";

            try
            {
                DisplayMsg(LogType.Log, "=============== Ethernet Test ===============");

                //LAN Port1~4
                for (int port_num = 1; port_num <= 4; port_num++)
                {
                    retry_cnt = 0;
                    frmOK.Label = $"Sau khi kết nối dây mạng vào cổng LAN số {port_num}, vui lòng nhấn\"Xác nhận\"";
                    frmOK.ShowDialog();

                LAN_Port_Test:
                    if (SendAndChk(PortType.SSH, "mt eth linkrate", $"port {port_num}: 2500M FD", 0, 3000))
                    {
                        DisplayMsg(LogType.Log, $"Check LAN Port{port_num} pass");
                    }
                    else
                    {
                        DisplayMsg(LogType.Log, $"Check LAN Port{port