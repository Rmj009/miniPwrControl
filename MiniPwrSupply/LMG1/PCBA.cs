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
using System.Globalization;
using NationalInstruments.Analysis.Conversion;
using System.Windows.Media;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Web.Services.Description;
namespace MiniPwrSupply.LMG1
{
    public partial class PCBA_Station
    {
        DeviceInfor infor = new DeviceInfor();
        SFCS_Query _Sfcs_Query = new SFCS_Query();
        public class DeviceInfor
        {
            public string SerialNumber = "";
            public string HWver = "";
            public string BaseMAC = "";
            public string BleMAC = "";
            public string FWver = "";
            public string HWID = "";
            public string FWver_Cust = "";
            public string HWver_Cust = "";
            public string WiFiMAC_2G = "";
            public string WiFiMAC_5G = "";
            public string WiFiMAC_6G = "";
            public string BLEver = "";
            public string SEver = "";
            public string Chipver = "";
            public string CalData_MD5 = "";
            public string HWver_for_Board = "";


            public void ResetParam()
            {
                SerialNumber = "";
                HWver = "";
                BaseMAC = "";
                BleMAC = "";
                FWver = "";
                HWID = "";
                FWver_Cust = "";
                HWver_Cust = "";
                WiFiMAC_2G = "";
                WiFiMAC_5G = "";
                WiFiMAC_6G = "";
                BLEver = "";
                SEver = "";
                Chipver = "";
                CalData_MD5 = "";
                HWver_for_Board = "";
            }
        }
        private void PCBA()
        {
            /*if (useShield)
            {
                fixture.ControlIO(Fixture.FixtureIO.IO_6, CTRL.ON);
                fixture.ControlIO(Fixture.FixtureIO.IO_7, CTRL.ON);
                fixture.ControlIO(Fixture.FixtureIO.IO_8, CTRL.ON);
            }*/

            try
            {
                infor.ResetParam();
                if (status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode)
                {
                    #region Combine SN
                    string SN_name = Func.ReadINI("Setting", "FirehoseFW", "SN", "@LRG1_SN");
                    string MAC_name = Func.ReadINI("Setting", "FirehoseFW", "BaseMAC", "@MAC");

                    SFCS_Query _sfcsQuery = new SFCS_Query();
                    ATS_Template.SFCS_ATS_2_0.ATS ss = new ATS_Template.SFCS_ATS_2_0.ATS();
                    bool combine = false;
                    int snLength = Convert.ToInt32(Func.ReadINI("Setting", "Match", "SN_Length", "11"));
                    string snStartwith = Func.ReadINI("Setting", "Match", "SN_Start", "T");
                    GetFromSfcs("@LRG1_SN", out infor.SerialNumber);
                    if (infor.SerialNumber.Length != snLength)
                    {
                        CreatePsnFile();
                        if (!ChkCombine())
                        {
                            warning = "Combine fail";
                            return;
                        }
                    }
                    GetFromSfcs("@LRG1_SN", out infor.SerialNumber);
                    GetFromSfcs("@MAC", out infor.BaseMAC);
                    if (infor.SerialNumber.Length != snLength && infor.SerialNumber.StartsWith(snStartwith) || string.IsNullOrEmpty(infor.BaseMAC))
                    {
                        DisplayMsg(LogType.Log, $"SN length:{snLength}");
                        DisplayMsg(LogType.Log, $"SN start with:{snStartwith}");
                        warning = "SN format fail";
                        return;
                    }
                    #endregion Combine SN

                    #region Get SFCS information
                    //SE_TODO: get infor from SFCS
                    //SentPsnForGetMAC(status_ATS.txtPSN.Text.Trim());
                    #region Check SN base on format
                    if (infor.SerialNumber.Length == 18)
                    {
                        bool SN_check = CheckSN(infor.SerialNumber);
                        if (SN_check)
                        {
                            SetTextBox(status_ATS.txtPSN, infor.SerialNumber);
                            //SetTextBox(status_ATS.txtSP, infor.BaseMAC);
                            status_ATS.SFCS_Data.PSN = infor.SerialNumber;
                            status_ATS.SFCS_Data.First_Line = infor.SerialNumber;
                        }
                        else
                        {
                            warning = "Get SN from SFCS fail";
                        }

                    }
                    else
                    {
                        warning = "Get SN from SFCS fail";
                        return;
                    }
                    #endregion Check SN base on format
                    string HW_VERSION_FOR_BOARD_CPN = Func.ReadINI("Setting", "FirehoseFW", "HWver_for_Board", "");
                    string MFG_FW_CPN = Func.ReadINI("Setting", "FirehoseFW", "FWver", "");
                    string[] Inform_infor = new string[] { "HWID", "FWver", "BLEver", "HWver_for_Board" };
                    //Run for MP
                    //string[] CPN = new string[] { "@HW_ID_10", "@MFG_FW_17", "@BLE_FW_VERSION1", "@HW_VERSION", "@HW_VERSION_BOARD_12" };
                    //Run for EPR
                    string partNumber = string.Empty;
                    partNumber = GetPartNumber(status_ATS.SFCS_Data.PSN);
                    //DisplayMsg(LogType.Log, "partNumber is:" + partNumber);
                    string[] CPN = new string[] { "@HW_ID_10", "@MFG_FW", "@BLE_FW_VERSION1", "@HW_VERSION_BOARD_12" };
                    if (partNumber == "57.LMG11.003")
                    {
                        CPN = new string[] { "@HW_ID_10", "@MFG_FW_17", "@BLE_FW_VERSION1", "@HW_VERSION_BOARD_12" };
                    }
                    if (partNumber == "57.LMG11.002")
                    {
                        CPN = new string[] { "@HW_ID_10", "@MFG_FW", "@BLE_FW_VERSION1", "@HW_VERSION", "@HW_VERSION_FOR_BOARD" };
                    }
                    Compare_SFCS_Setting(Inform_infor, "PCBA", CPN);
                    infor.Chipver = Func.ReadINI("Setting", "PCBA", "Chipver", "0x4000023D");
                    infor.SEver = Func.ReadINI("Setting", "PCBA", "SEver", "");
                    DisplayMsg(LogType.Log, "Get SN From SFCS is:" + infor.SerialNumber);
                    DisplayMsg(LogType.Log, "Get Base MAC From SFCS is:" + infor.BaseMAC);
                    DisplayMsg(LogType.Log, "Get chipver From SFCS is:" + infor.Chipver);
                    DisplayMsg(LogType.Log, "Get HWID From SFCS is:" + infor.HWID);
                    DisplayMsg(LogType.Log, "Get FWVER From SFCS is:" + infor.FWver);
                    DisplayMsg(LogType.Log, "Get BLEver From SFCS is:" + infor.BLEver);
                    DisplayMsg(LogType.Log, "Get SEver From SFCS is:" + infor.SEver);
                    DisplayMsg(LogType.Log, "Get HWver_for_Board From SFCS is:" + infor.HWver_for_Board);
                    infor.BaseMAC = MACConvert(infor.BaseMAC);
                    DisplayMsg(LogType.Log, "Base MAC Convert" + infor.BaseMAC);

                }
                else
                {
                    //Rena_20230407 add for HQ test
                    //GetBoardDataFromExcel(status_ATS.txtPSN.Text, true);
                    /*                    string STT = "4";
                                        GetBoardDataFromExcel(STT);*/
                    GetBoardDataFromExcel1();
                    infor.FWver = Func.ReadINI(Application.StartupPath, "Setting", "PCBA", "FWver", "LMG1_v0.0.0.1");
                    infor.HWver = Func.ReadINI(Application.StartupPath, "Setting", "PCBA", "HWver", "EVT1");
                    infor.HWID = Func.ReadINI(Application.StartupPath, "Setting", "PCBA", "HWID", "0000");
                    infor.BLEver = Func.ReadINI(Application.StartupPath, "Setting", "PCBA", "BLEver", "v5.0.0-b108");
                    infor.SEver = Func.ReadINI(Application.StartupPath, "Setting", "PCBA", "SEver", "00010206");
                    infor.Chipver = Func.ReadINI(Application.StartupPath, "Setting", "PCBA", "Chipver", "0x4000023D");
                    infor.HWver_for_Board = Func.ReadINI(Application.StartupPath, "Setting", "PCBA", "HWver_for_Board", "XXXX");
                    GetFromSfcs("@MAC", out infor.BaseMAC);
                    //mac_base -> LAN1
                    //mac_base + 1->LAN2
                    //mac_base + 2-> 6GHz Wi-Fi
                    //mac_base + 3-> 5GHz Wi-Fi
                    //mac_base + 4-> 2.4GHz Wi-Fi
                    //mac_base + 5->SPARE
                    //mac_base + 6->SPARE
                    infor.BaseMAC = MACConvert(infor.BaseMAC);
                    DisplayMsg(LogType.Log, "Get SN From setting is:" + infor.SerialNumber);
                    DisplayMsg(LogType.Log, "Get Base MAC From setting is:" + infor.BaseMAC);
                    DisplayMsg(LogType.Log, "Get chipver From setting is:" + infor.Chipver);
                    DisplayMsg(LogType.Log, "Get HWID From setting is:" + infor.HWID);
                    DisplayMsg(LogType.Log, "Get FWVER From setting is:" + infor.FWver);
                    DisplayMsg(LogType.Log, "Get BLEver From setting is:" + infor.BLEver);
                    DisplayMsg(LogType.Log, "Get SEver From setting is:" + infor.SEver);
                    DisplayMsg(LogType.Log, "Get HWver_for_Board From setting is:" + infor.HWver_for_Board);
                }
                if (!ChkStation(status_ATS.txtPSN.Text))
                    return;

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
                    Thread.Sleep(5000);
                    SwitchRelay(CTRL.OFF);
                }
                else
                {
                    if (Func.ReadINI("Setting", "PCBA", "DoFWUpgrade", "0") == "1")
                    {
                        if (isLoop == 0)
                        {
                            //frmOK.Label = "確認'Console線'已接上,'網路線'已接到紅色Reset Button旁的lan port,\r\n請先按下OK button,然後將DUT開機";
                            frmOK.Label = "Xác nhận rằng dây Console và 2 dây mạng đã được kết nối\r\nVui lòng nhấn nút OK trước, sau đó bật nguồn cho DUT";
                            frmOK.ShowDialog();

                        }

                    }
                    else
                    {
                        //frmOK.Label = "確認'網路線'已接到紅色Reset Button旁的lan port,\r\n請上電並按下power button開機";
                        if (isLoop == 0)
                        {
                            frmOK.Label = "Xác nhận rằng đã kết nối đúng 2 dây mạng\r\nVui lòng bật nguồn và nhấn nút nguồn để khởi động";
                            frmOK.ShowDialog();
                        }

                    }
                }
                DisplayMsg(LogType.Log, "Power on!!!");
                #region on power button

                DisplayMsg(LogType.Log, $"Delay {Convert.ToInt32(WNC.API.Func.ReadINI(Application.StartupPath, "Setting", "TimeOut", "DelayPower", "1000"))}ms");
                Thread.Sleep(Convert.ToInt32(WNC.API.Func.ReadINI(Application.StartupPath, "Setting", "TimeOut", "DelayPower", "1000")));
                string cameraResult = "";
                int n = 0;
            retryBTN:
                if (Camera())
                {
                    if (CheckCameraResult($"item_1", $"black", out cameraResult))
                    {
                        if (fixture.useFixture)
                        {
                            fixture.ControlIO(Fixture.FixtureIO.IO_5, CTRL.ON);
                            fixture.ControlIO(Fixture.FixtureIO.IO_5, CTRL.OFF);
                        }
                        else
                        {
                            frmOK.Label = "DUT chưa lên nguồn, hãy bật nguồn sau đó nhấn [OK]";
                            frmOK.ShowDialog();
                        }
                    }
                }
                if (Camera())
                {
                    if (CheckCameraResult($"item_1", $"black", out cameraResult))
                    {
                        if (fixture.useFixture)
                        {
                            n++;
                            if (n < 3)
                            {
                                DisplayMsg(LogType.Log, "Retry -->");
                                goto retryBTN;
                            }
                        }
                        else
                        {
                            frmOK.Label = "DUT chưa lên nguồn, hãy bật nguồn sau đó nhấn [OK]";
                            frmOK.ShowDialog();
                        }
                    }
                }
                #endregion
                //FW upgrade(Optional)
                if (Func.ReadINI("Setting", "PCBA", "DoFWUpgrade", "0") == "1")
                {
                    UartDispose(uart);
                    if (!CheckGoNoGo()) { return; }
                    FWUpgrade_Bootloader();
                }
                this.ChkEMMC();
                ChkBootUp(PortType.SSH);
                if (!CheckGoNoGo()) { return; }
                ClearENV();
                if (!CheckGoNoGo()) { return; }
                CheckFWVerAndHWID();
                if (forHQtest || (status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode && !isGolden))
                {
                    if (!CheckGoNoGo()) { return; }
                    SetDUTInfo();
                    // ====== Burn fuse and enable QSApp ======
                    this.SecureBootTransition();
                    DisplayMsg(LogType.Log, @"Delays 5s after SecureBootTransition");
                    Thread.Sleep(5000);
                    // ========================================
                    if (!CheckGoNoGo()) { return; }
                    CheckDUTInfo();
                }
                if (!CheckGoNoGo()) { return; }
                if (isLoop == 0)
                {
                    CheckLEDAuto();
                    // CheckLED();
                }
                if (!CheckGoNoGo()) { return; }
                CheckPCIe();
                if (isLoop == 0)
                    WPSButton();
                if (isLoop == 0)
                    ResetButton();
                if (Func.ReadINI("Setting", "Setting", "SkipEthernet", "0") == "0")
                {
                    if (!CheckGoNoGo()) { return; }
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

        //Check SN
        #region Check SN base on Inform format
        private bool CheckSN(string s1)
        {
            //Check first 8 characters is +119747+
            string pattern = @"^.{8}";
            Match match = Regex.Match(s1, pattern);
            if (match.Success)
            {
                if (match.Value == "+119747+")
                {

                }
                else
                {
                    warning = $"Incorrect SN format, Right format: +119747+, Real Format: {match.Value}";
                    //                   DisplayMsg(LogType.Log, $"Incorrect SN format, Right format: +119747+, RealFormat: {match.Value}");
                    return false;
                }
            }
            else
            {
                warning = $"Incorrect SN format, Right format: +119747+, Real Format: {match.Value}";
                return false;
            }

            //Check next 2 characters is YY
            pattern = @"^.{8}(.{2})";
            match = Regex.Match(s1, pattern);
            DateTime currentDate = DateTime.Now;
            string currentYear = currentDate.Year.ToString();
            string lastTwoCharacters = currentYear.Substring(currentYear.Length - 2);
            if (match.Success)
            {
                if (match.Groups[1].Value == lastTwoCharacters)
                {

                }
                else
                {
                    warning = $"Digits 9,10 incorrect, Right format:{lastTwoCharacters}, Real format: {match.Groups[1].Value}";
                    //                    DisplayMsg(LogType.Log, $"Digits 9,10 incorrect, Right format:{lastTwoCharacters}, Real format: {match.Groups[1].Value}");
                    return false;
                }

            }
            else
            {
                warning = $"Digits 9,10 incorrect, Right format (Day) : {lastTwoCharacters}, Real format: {match.Groups[1].Value}";
                return false;
            }


            //Check next 2 characters is Week
            int currentWeek = GetISOWeekNumber(currentDate);
            string currentWeek_str = currentWeek.ToString();

            pattern = @"^.{10}(.{2})";
            match = Regex.Match(s1, pattern);
            if (match.Success)
            {
                if (int.Parse(match.Groups[1].Value) <= currentWeek)
                {
                }
                else
                {
                    warning = $"Digits 11,12 incorrect, Right format (Week): {currentWeek_str}, Real format: {match.Groups[1].Value}";
                    //                   DisplayMsg(LogType.Log, $"Digits 11,12 incorrect, Right format (Week): {currentWeek_str}, Real format: {match.Groups[1].Value}");
                    return false;
                }

            }
            else
            {
                warning = $"Digits 11,12 incorrect, Right format: {currentWeek_str}, Real format: {match.Groups[1].Value}";
                return false;
            }

            //Check last 6 characters
            pattern = @"^\d+$";
            string s = s1.Substring(12, 6);
            match = Regex.Match(s, pattern);
            if (match.Success)
            {
                DisplayMsg(LogType.Log, $"Check SN in Eform rule PASS, SN is: {s1}");
            }
            else
            {
                warning = $"Digits from 13-18 incorrect, Right format only consit of digit (1234567890), Real format: {match.Value}";
                //               DisplayMsg(LogType.Log, $"Digits from 13-18 incorrect, Right format only consit of digit (1234567890), Real format: {match.Value}");

                return false;
            }

            return true;
        }

        static int GetISOWeekNumber(DateTime date)
        {
            CultureInfo culture = CultureInfo.CurrentCulture;
            Calendar calendar = culture.Calendar;
            CalendarWeekRule weekRule = CalendarWeekRule.FirstFourDayWeek;
            DayOfWeek firstDayOfWeek = DayOfWeek.Monday; // Thay đổi nếu ngày đầu tuần khác

            int weekNumber = calendar.GetWeekOfYear(date, weekRule, firstDayOfWeek);
            return weekNumber;
        }

        #endregion Compare data from SFCS and setting

        #region Compare data from SFCS and setting
        private void Compare_SFCS_Setting(string[] label, string cfg_station, string[] CPN)
        {
            int count = 0;
            foreach (string s in label)
            {
                //string Key_name = Func.ReadINI("Setting", "FirehoseFW", s , "");
                string ST_name = Func.ReadINI("Setting", cfg_station, s, "");
                //DisplayMsg(LogType.Log, $"Get {s} From setting is:" + ST_name);
                string SFCS_Data = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, CPN[count]);
                DisplayMsg(LogType.Log, $"SFCS Data {CPN}: {SFCS_Data}");
                if (!SFCS_Data.Contains("Dut not have"))
                {
                    SFCS_Data = SFCS_Data.Trim();
                    SFCS_Data = SFCS_Data.Substring(0, SFCS_Data.Length - 9);

                    switch (s)
                    {
                        case "chipver":
                            infor.Chipver = SFCS_Data;
                            break;
                        case "HWID":
                            infor.HWID = SFCS_Data;
                            break;
                        case "FWver":
                            infor.FWver = SFCS_Data;
                            break;
                        case "BLEver":
                            infor.BLEver = SFCS_Data;
                            break;
                        case "SEver":
                            infor.SEver = SFCS_Data;
                            break;
                        case "HWver":
                            infor.HWver = SFCS_Data;
                            break;
                        case "HWver_for_Board":
                            infor.HWver_for_Board = SFCS_Data;
                            break;

                        case "FWver_Cust":
                            infor.FWver_Cust = SFCS_Data;
                            break;
                        case "HWver_Cust":
                            infor.HWver_Cust = SFCS_Data;
                            break;
                        default:
                            warning = $"Vui long them truong du lieu {s} vao FirehoseFW trong setting";
                            return;
                    }

                    if (SFCS_Data == ST_name)
                    {
                        DisplayMsg(LogType.Log, $"SFCS is: {SFCS_Data}, setting is: {ST_name}");
                        DisplayMsg(LogType.Log, $"Compare {s} between SFCS & Setting file Pass");
                    }
                    else
                    {
                        DisplayMsg(LogType.Log, $"SFCS is: {SFCS_Data}, setting is: {ST_name}");
                        warning = $"Compare {s} between SFCS & Setting file Fail";
                        return;
                    }
                }
                else
                {
                    warning = $"Get {s} from SFCS Fail!";
                    return;
                }
                count++;
            }
        }

        #endregion
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


        private void QUTSStatusApp(string path)
        {
            try
            {
                KillTaskProcess("QUTSStatusApp");
                Thread.Sleep(1000);
                Directory.SetCurrentDirectory(path);
                Process.Start(Path.Combine(path, "QUTSStatusApp.exe"));
                DisplayMsg(LogType.Log, $"Start {path}\\QUTSStatusApp.exe");
                Directory.SetCurrentDirectory(Application.StartupPath);
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
            }
        }


        private bool BootLoader(SerialPort port, string keyword1 = "stop autoboot", string keyword2 = "IPQ5332#", int timeOutMs = 100000)
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
        private void ChkBootUp(PortType portType, string keyword = @"root@OpenWrt")
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            DisplayMsg(LogType.Log, "=============== Check BootUp ===============");
            //string keyword = @"root@OpenWrt";
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

        private void SetDUTInfo()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            DisplayMsg(LogType.Log, "=============== Write Board data ===============");

            string keyword = "root@OpenWrt:~# \r\n"; //避免誤判到指令第一行的"root@OpenWrt:~#"
            string defaults_img_path = Path.Combine(Application.StartupPath, "default_image_backup");
            string PC_IP = Func.ReadINI("Setting", "PCBA", "PC_IP", "192.168.1.2");
            string item = "SetDUTInfo";
            string res = "";

            try
            {
                OpenTftpd32(defaults_img_path);
                // -------------------------- for stress test ----------------------
                //infor.HWver_for_Board = "EVT2-3";
                // -----------------------------------------------------------------
                string SerialNumber = infor.SerialNumber.Substring(infor.SerialNumber.LastIndexOf('+') + 1);
                if (SerialNumber == "" || infor.BaseMAC == "" || infor.HWver_for_Board == "") //temporilary markup for stress test
                {
                    DisplayMsg(LogType.Log, "Board data can't be null");
                    AddData(item, 1);
                    return;
                }
                //=============================================================================================
                this.DownloadFilesRequired(false); // require verify by VN
                this.DownloadAllConfigs();
                // ------------------------------
                //this.DownloadDisgue(false);
                //this.DownloadDisgue2();
                // ----------------------------
                //5.3.4 Partition data formatting Ext4 Partition
                DisplayMsg(LogType.Cmd, $"Write 'mkfs.ext4 /dev/mmcblk0p28' to ssh");
                SSH_stream.WriteLine("mkfs.ext4 /dev/mmcblk0p28");
                Thread.Sleep(10 * 1000); //TODO: 第一次做&重複做的flow不同,待優化
                ChkResponse(PortType.SSH, ITEM.NONE, "Writing superblocks and filesystem accounting information", "/dev/mmcblk0p28 contains a ext4 file system", out res, 5000);
                if (res.Contains("/dev/mmcblk0p28 contains a ext4 file system")) //處理已經做過的case
                {
                    //Proceed anyway?
                    Thread.Sleep(500);
                    SendAndChk(PortType.SSH, "y", keyword, out res, 0, 10000);
                }
                SendAndChk(PortType.SSH, "mkdir -p /mnt/data", keyword, out res, 0, 3000);
                SendAndChk(PortType.SSH, "mkdir -p /mnt/defaults", keyword, out res, 0, 3000);
                SendAndChk(PortType.SSH, "mount -t ext4 /dev/mmcblk0p28 /mnt/data", keyword, out res, 0, 3000);
                SendAndChk(PortType.SSH, "mount | grep /dev/mmcblk0p28", keyword, out res, 0, 3000);
                this.FilesystemEncryption(true);
                if (!CheckGoNoGo()) return;
                DisplayMsg(LogType.Log, @"Delays 5s after FilesystemEncryption");
                Thread.Sleep(5000);
                DisplayMsg(LogType.Log, $"=============== Execute gen_board ===============");
                //Generate Board data
                //BaseMAC is capital letters
                SendAndChk(PortType.SSH, $"gen_board_data.sh {SerialNumber} {infor.HWver_for_Board} {infor.BaseMAC.ToUpper()}", keyword, out res, 0, 10000);
                if (!res.Contains(keyword) || res.IndexOf("Error", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    DisplayMsg(LogType.Log, "Generate board data fail");
                    AddData(item, 1);
                    return;
                }

                //Write data into DUT
                SendAndChk(PortType.SSH, "gen_squashfs.sh", "No such file or directory", "4096 bytes (4.0KB) copied", out res, 0, 40000);
                if (!res.Contains("100.00%") || res.Contains("No such file or directory"))
                {
                    DisplayMsg(LogType.Log, "Write Board data fail");
                    AddData(item, 1);

                    return;
                }

                if (!SendAndChk(PortType.SSH, "ls /tmp", "defaults.img", out res, 0, 5000))
                {
                    DisplayMsg(LogType.Log, "Can't find /tmp/defaults.img");
                    AddData(item, 1);
                    return;
                }
                if (!SendAndChk(PortType.SSH, "dd if=/tmp/defaults.img of=/dev/mapper/defaults", keyword, out res, 0, 5000))
                {
                    DisplayMsg(LogType.Log, "Can't find /tmp/defaults.img");
                    AddData(item, 1);
                    return;
                }
                SendAndChk(PortType.SSH, "sync", keyword, 0, 5000);


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

            DisplayMsg(LogType.Log, "=============== Verify Board data ===============");

            string keyword = "root@OpenWrt:~# \r\n";
            string item = "ChkDUTInfo";
            string res = "";

            try
            {
                SendAndChk(PortType.SSH, "verify_boarddata.sh", keyword, out res, 0, 5000);
                //serial_number=+119746+2333000129
                if (!res.Contains($"serial_number={infor.SerialNumber}"))
                {
                    DisplayMsg(LogType.Log, $"SFCS SN is:{infor.SerialNumber}");
                    DisplayMsg(LogType.Log, "Check serial_number fail");
                    AddData(item, 1);
                }
                else
                {
                    DisplayMsg(LogType.Log, $"Check serial_number '{infor.SerialNumber}' pass");
                }

                //hw_ver=EVT1

                if (!res.Contains($"hardware_version={infor.HWver_for_Board}"))
                //if (!res.Contains($"hw_ver={infor.HWver_for_Board}"))
                {
                    DisplayMsg(LogType.Log, $"SFCS HW version for board is:{infor.HWver_for_Board}");
                    DisplayMsg(LogType.Log, "Check hw_ver fail");
                    AddData(item, 1);
                }
                else
                {
                    DisplayMsg(LogType.Log, $"Check hw_ver '{infor.HWver_for_Board}' pass");
                }

                //mac_base=E8:C7:CF:AF:46:28
                if (!res.Contains($"mac_base={infor.BaseMAC}"))
                {
                    DisplayMsg(LogType.Log, $"SFCS MAC is:{infor.BaseMAC}");
                    DisplayMsg(LogType.Log, "Check mac_base fail");
                    AddData(item, 1);
                }
                else
                {

                    DisplayMsg(LogType.Log, $"Check mac_base '{infor.BaseMAC}' pass");
                }

                //以下為固定值確認
                if (!res.Contains("check=0") || !res.Contains("device_category=COM") || !res.Contains("manufacturer=BT") || !res.Contains("wifi_country_revision=0") ||
                    !res.Contains("manufacturer_oui=0000DB") || !res.Contains("model_name=Smart WiFi SW40J") || !res.Contains("model_number=SW40J") ||
                    !res.Contains("description=Smart WiFi SW40J") || !res.Contains("product_class=SW4-1") || !res.Contains("mac_count=7") ||
                    !res.Contains("item_code=119747") || !res.Contains("brand_variant=Consumer") || !res.Contains("wifi_country_code=GB"))
                {
                    DisplayMsg(LogType.Log, "Check board data fail");
                    AddData(item, 1);
                }

                if (CheckGoNoGo())
                {
                    AddData(item, 0);
                    //upload data to SFCS
                    status_ATS.AddDataRaw("LMG1_Label_SN", infor.SerialNumber, infor.SerialNumber, "000000");
                    status_ATS.AddDataRaw("LMG1_Label_MAC", infor.BaseMAC.Replace(":", ""), infor.BaseMAC.Replace(":", ""), "000000");
                    status_ATS.AddDataRaw("LMG1_Label_HWVER", infor.HWver_for_Board, infor.HWver_for_Board, "000000");
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
        private string MACConvert_second(string mac, int param = 0)
        {
            try
            {
                //DisplayMsg(LogType.Log, "MAC input:" + mac);
                string ethmac = mac.Replace(":", "");
                ethmac = Convert.ToString(Convert.ToInt64(ethmac, 16) + param, 16).PadLeft(12, '0');
                var regex = "(.{2})(.{2})(.{2})(.{2})(.{2})(.{2})";
                var replace = "$1-$2-$3-$4-$5-$6";
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
            string item = "EthernetTest";
            string keyword = "root@OpenWrt:~# \r\n";
            string res = "";
            try
            {
                DisplayMsg(LogType.Log, "=============== Ethernet Test ===============");

                //LAN Port1~2
                //LMG1直接接兩條ethernet,所以可同時測LAN port1 & port2

                if (write_mac)
                {
                    //Write MAC Address 寫完後要重開機才會生效,所以PCBA站寫入後在final站檢查
                    //mac_base -> LAN1
                    //mac_base + 1->LAN2

                    string write_mac_eth0 = MACConvert(infor.BaseMAC).Replace(":", "\\x");
                    string write_mac_eth1 = MACConvert(infor.BaseMAC, 1).Replace(":", "\\x");
                    DisplayMsg(LogType.Log, $"Write MAC eth0: {write_mac_eth0}");
                    DisplayMsg(LogType.Log, $"Write MAC eth1: {write_mac_eth1}");

                    SendAndChk(PortType.SSH, $"echo -n -e '\\x{write_mac_eth0}' > /tmp/mac", keyword, out res, 0, 3000); //eth0
                    SendAndChk(PortType.SSH, $"echo -n -e '\\x{write_mac_eth1}' >> /tmp/mac", keyword, out res, 0, 3000); //eth1
                    SendAndChk(PortType.SSH, "dd if=/tmp/mac of=/dev/mmcblk0p18 bs=1 count=12", keyword, out res, 0, 3000);
                    if (!res.Contains("12 bytes (12B) copied") || res.Contains("No such file or directory"))
                    {
                        DisplayMsg(LogType.Log, "Write eth0~eth1 MAC Address fail");
                        AddData(item, 1);
                        return;
                    }

                    //Verify eth MAC
                    var regex = "(.{2}):(.{2}):(.{2}):(.{2}):(.{2}):(.{2})";
                    string eth0_mac = Regex.Replace(MACConvert(infor.BaseMAC), regex, "$2$1 $4$3 $6$5").ToLower();
                    string eth1_mac = Regex.Replace(MACConvert(infor.BaseMAC, 1), regex, "$2$1 $4$3 $6$5").ToLower();
                    DisplayMsg(LogType.Log, $"eth0_mac: {eth0_mac}");
                    DisplayMsg(LogType.Log, $"eth1_mac: {eth1_mac}");
                    SendAndChk(PortType.SSH, "hexdump -n 12 /dev/mmcblk0p18", keyword, out res, 0, 5000);
                    if (!res.Contains(eth0_mac + " " + eth1_mac))
                    {
                        DisplayMsg(LogType.Log, "Check eth0~eth1 MAC fail");
                        AddData(item, 1);
                    }
                    else
                    {
                        DisplayMsg(LogType.Log, "Check eth0~eth1 MAC pass");
                        AddData(item, 0);
                        status_ATS.AddDataRaw("LMG1_ETH0_MAC", infor.BaseMAC.Replace(":", ""), infor.BaseMAC.Replace(":", ""), "000000");
                        status_ATS.AddDataRaw("LMG1_ETH1_MAC", MACConvert(infor.BaseMAC, 1).Replace(":", ""), MACConvert(infor.BaseMAC, 1).Replace(":", ""), "000000");
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData(item, 1);
            }
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

            try
            {
                DisplayMsg(LogType.Log, "=============== Check Ethernet MAC ===============");
                //mac_base -> LAN1(eth0)
                //mac_base + 1->LAN2(eth1)
                SendAndChk(PortType.SSH, "ifconfig | grep eth", keyword, out res, 0, 5000);
                DisplayMsg(LogType.Log, $"Base MAC is: {infor.BaseMAC}");

                if (Regex.IsMatch(res, $"eth0.+HWaddr {infor.BaseMAC}") && Regex.IsMatch(res, $"eth1.+HWaddr {MACConvert(infor.BaseMAC, 1)}"))
                {
                    DisplayMsg(LogType.Log, "Check eth0~eth1 MAC Address pass");
                    AddData(item, 0);
                }
                else
                {
                    DisplayMsg(LogType.Log, "Check eth0~eth1 MAC Address fail");
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
            string keyword = "root@OpenWrt:~# \r\n";
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

                /*                AddData("PowerSensor", Convert.ToDouble(Power));
                                AddData("CurrentSensor", Convert.ToDouble(Current));*/

                if (Power != "" && Current != "")
                {
                    //TODO: LMG1 spec待確認
                    status_ATS.AddData("Power", "W", Convert.ToDouble(Power));
                    status_ATS.AddData("Current", "A", Convert.ToDouble(Current));
                    //status_ATS.AddDataRaw("Power", Power, Power, "000000");
                    //status_ATS.AddDataRaw("Current", Current, Current, "000000");
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
            if (!CheckGoNoGo())
            {
                return;
            }

            string item = "BleTest";
            string keyword = "root@OpenWrt:~# \r\n";
            string res = "";
            string ble_addr = "";
            string ble_ver = "";
            string se_ver = "";

            try
            {
                DisplayMsg(LogType.Log, "=============== BLE Test ===============");

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
                infor.BLEver = infor.BLEver.Replace("BT", "v");
                DisplayMsg(LogType.Log, "BLE version convert: " + infor.BLEver);
                if (ble_ver.Contains(infor.BLEver) || !string.IsNullOrEmpty(ble_ver))
                {
                    DisplayMsg(LogType.Log, "Compare BLE version with SFCS PASS");
                }
                else
                {
                    DisplayMsg(LogType.Log, "Compare BLE version with SFCS fail");
                    AddData(item, 1);
                }
                /*if (ble_ver == "" || string.Compare(infor.BLEver, ble_ver) != 0)
                {
                                    DisplayMsg(LogType.Log, "Check BLE version fail");
                                    AddData(item, 1);
                 }*/

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
                    status_ATS.AddDataRaw("LMG1_Label_BLEVER", infor.BLEver.Replace("v", "BT"), infor.BLEver.Replace("v", "BT"), "000000");
                    status_ATS.AddDataRaw("LMG1_SE_Ver", se_ver, se_ver, "000000");
                    status_ATS.AddDataRaw("LMG1_BLE_MAC", ble_addr.Replace(":", ""), ble_addr.Replace(":", ""), "000000");
                    infor.BleMAC = ble_addr; //Rena_20230522, for HQ sample test flow,為了回填到excel
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData(item, 1);
            }
        }
        private void SetBleCTUNE()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            string item = "SetBleCTUNE";
            string res = "";
            string Commander_Tool = Application.StartupPath + "\\commander_lrg1_v1";
            CommandConsole myCmd = null;

            try
            {
                DisplayMsg(LogType.Log, "=============== Set BLE CTUNE ===============");

                myCmd = new CommandConsole();
                myCmd.Start();
                myCmd.WriteLine(Path.GetPathRoot(Commander_Tool).TrimEnd('\\'));
                myCmd.WriteLine("cd " + Commander_Tool);

                //set CTUNE as 0x66
                SendCmdAndGetResp(myCmd, "commander.exe ctune set --value 0x66 -d efr32", "DONE", out res, 10000);
                if (!res.Contains("Setting CTUNE token to 102"))
                {
                    DisplayMsg(LogType.Log, "Set BLE CTUNE fail");
                    AddData(item, 1);
                }

                //Check CTUNE
                SendCmdAndGetResp(myCmd, "commander.exe ctune get -d efr32", "DONE", out res, 10000);
                if (!res.Contains("Token: 102"))
                {
                    DisplayMsg(LogType.Log, "Check BLE CTUNE fail");
                    AddData(item, 1);
                }
                else
                {
                    DisplayMsg(LogType.Log, "Check BLE CTUNE pass");
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
                if (myCmd != null)
                    myCmd.Close();
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
                DisplayMsg(LogType.Exception, ex.ToString());
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
        private void cameraCheckLED(string item, COLOR color, STAGE stage, string cameraItem)
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            string res = string.Empty;
            string cmd = string.Empty;
            bool rs = false;
            switch (color)
            {
                case COLOR.GREEN:
                    //SendAndChk(PortType.SSH, $"mt led set {cmd} 255", keyword, 0, 3000);
                    cmd += "g";
                    break;
                case COLOR.RED:
                    cmd += "r";
                    break;
                case COLOR.BLUE:
                    cmd += "b";
                    break;
                case COLOR.WHITE:
                    cmd += "w";
                    break;
                default:
                    break;
            }
            COLOR newcolor = color;
            switch (stage)
            {
                case STAGE.ON:
                    cmd = $"mt led set {cmd} 255";
                    break;
                case STAGE.OFF:
                    cmd = $"mt led set {cmd} 0";
                    newcolor = COLOR.BLACK;
                    break;
                default:
                    break;
            }
            string keyword = cmd;
            DisplayMsg(LogType.Log, $"============ {item}_{color}_{stage} ============");
            rs = SendAndChk(PortType.SSH, cmd, keyword, 1000, 15000); //Jason change 20231031
            if (!rs)
            {
                AddData($"{item}_{color}_{stage}", 1);
                return;
            }
            if (usecamera)
            {
                string cameraResult = "";
                if (Camera())
                {

                    if (CheckCameraResult($"{cameraItem}", $"{newcolor.ToString().ToLower()}", out cameraResult))
                    {
                        AddData($"{item}_{color}_{stage}", 0);
                    }
                    else
                    {
                        AddData($"{item}_{color}_{stage}", 1);
                        return;
                    }

                }
                else
                {
                    warning = "Using camera fail";
                    return;
                }
            }
            else
            {
                if (DialogResult.No == MessageBox.Show($"Check if {item} {color} is {stage}?", "Led Test", MessageBoxButtons.YesNo))
                {
                    DisplayMsg(LogType.Log, $"Selected No");
                    AddData($"{item}_{color}_{stage}", 1);
                    return;
                }
                DisplayMsg(LogType.Log, $"Selected Yes");
                AddData($"{item}_{color}_{stage}", 0);
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

                    frmYN.Label = $"Vui lòng kiểm tra xem {item_name} có sáng hay không?";
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

                    frmYN.Label = $"Vui lòng kiểm tra xem {item_name} đã tắt hay chưa?";
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

                //開機後綠燈恆亮,先關閉綠燈
                //LED_Control("LED_Green", "g", CTRL.OFF);

                //LED1_Green
                LED_Control("LED_Green", "g", CTRL.ON);
                LED_Control("LED_Green", "g", CTRL.OFF);

                //LED1_Red
                LED_Control("LED_Red", "r", CTRL.ON);
                LED_Control("LED_Red", "r", CTRL.OFF);

                //LED1_Blue
                LED_Control("LED_Blue", "b", CTRL.ON);
                LED_Control("LED_Blue", "b", CTRL.OFF);

                //LED_White_On
                LED_Control("LED_White", "w", CTRL.ON);
                LED_Control("LED_White", "w", CTRL.OFF);
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                AddData("LED", 1);
            }
        }
        private void CheckLEDAuto()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            try
            {
                DisplayMsg(LogType.Log, "=============== LED Test ===============");

                //開機後綠燈恆亮,先關閉綠燈
                //LED_Control("LED_Green", "g", CTRL.OFF);

                //Turn green LED on
                cameraCheckLED("LEDGreen", COLOR.GREEN, STAGE.ON, "item_1");
                //Turn green LED off
                cameraCheckLED("LEDGreen", COLOR.GREEN, STAGE.OFF, "item_1");

                //Turn red LED on
                cameraCheckLED("LEDRED", COLOR.RED, STAGE.ON, "item_1");
                //Turn red LED off
                cameraCheckLED("LEDRED", COLOR.RED, STAGE.OFF, "item_1");

                //Turn blue LED on
                cameraCheckLED("LEDBlue", COLOR.BLUE, STAGE.ON, "item_1");
                //Turn blue LED off
                cameraCheckLED("LEDBlue", COLOR.BLUE, STAGE.OFF, "item_1");

                //Turn WhiteLED on
                cameraCheckLED("LED_White", COLOR.WHITE, STAGE.ON, "item_1");
                //Turn White LED off
                cameraCheckLED("LED_White", COLOR.WHITE, STAGE.OFF, "item_1");


            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                AddData("LED", 1);
            }
        }
        private void cameraCheckLEDNow(string item, COLOR color, STAGE stage, string cameraItem)
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            string res = string.Empty;
            bool rs = false;

            COLOR newcolor = color;
            switch (stage)
            {
                case STAGE.ON:
                    break;
                case STAGE.OFF:
                    newcolor = COLOR.BLACK;
                    break;
                default:
                    break;
            }
            //DisplayMsg(LogType.Log, $"RED or BLUE LED off");

            if (usecamera)
            {
                string cameraResult = "";
                if (Camera())
                {

                    if (CheckCameraResult($"{cameraItem}", $"{newcolor.ToString().ToLower()}", out cameraResult))
                    {
                        AddData($"RED or BLUE LED off", 0);
                    }
                    else
                    {
                        AddData($"{item}_{color}_{stage}", 1);
                        return;
                    }

                }
                else
                {
                    warning = "Using camera fail";
                    return;
                }
            }
            else
            {
                if (DialogResult.No == MessageBox.Show($"Check if {item} {color} is {stage}?", "Led Test", MessageBoxButtons.YesNo))
                {
                    DisplayMsg(LogType.Log, $"Selected No");
                    AddData($"{item}_{color}_{stage}", 1);
                    return;
                }
                DisplayMsg(LogType.Log, $"Selected Yes");
                AddData($"{item}_{color}_{stage}", 0);
            }
        }
        private void CheckPCIe()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            bool result = false;
            string item = "CheckPCIe";

            try
            {
                //Check 5G Up
                DisplayMsg(LogType.Log, "=============== Check PCIE Interface ==============="); // Jason modified Add retry 2023/10/07 to solve check pcie failure
                DisplayMsg(LogType.Log, "Check WiFi 5G PCIe Interface");
                int counttime = 5;
            recheck5GITFup:
                if (!SendAndChk(item, PortType.SSH, "lspci -s 0000:00:00.0 -vv | grep Speed", "LnkSta:\tSpeed 8GT/s (ok), Width x1 (ok)", 100, 3000))
                {
                    if (counttime > 0)
                    {
                        counttime--;
                        goto recheck5GITFup;
                    }
                    DisplayMsg(LogType.Log, "Check 5G PCIE Interface  UP fail");
                    AddData(item, 1);
                    return;
                }

                //Check 5G Down
                counttime = 5;
            recheck5GITFDown:
                if (!SendAndChk(item, PortType.SSH, "lspci -s 0000:01:00.0 -vv | grep Speed", "LnkSta:\tSpeed 8GT/s (ok), Width x1 (downgraded)", 0, 3000))
                {
                    if (counttime > 0)
                    {
                        counttime--;
                        goto recheck5GITFDown;
                    }
                    DisplayMsg(LogType.Log, "Check 5G PCIE Interface  Down fail");
                    AddData(item, 1);
                    return;
                }

                DisplayMsg(LogType.Log, "Check WiFi 6G PCIe Interface");
                //Check 6G UP
                counttime = 5;
            recheck6GITFup:
                if (!SendAndChk(item, PortType.SSH, "lspci -s 0001:00:00.0 -vv | grep Speed", "LnkSta:\tSpeed 8GT/s (ok), Width x2 (ok)", 0, 3000))
                {
                    if (counttime > 0)
                    {
                        counttime--;
                        goto recheck6GITFup;
                    }
                    DisplayMsg(LogType.Log, "Check 6G PCIE Interface  UP fail");
                    AddData(item, 1);
                    return;
                }

                //Check 6G Down
                counttime = 5;
            recheck6GITFDown:
                if (!SendAndChk(item, PortType.SSH, "lspci -s 0001:01:00.0 -vv | grep Speed", "LnkSta:\tSpeed 8GT/s (ok), Width x2 (ok)", 0, 3000))
                {
                    if (counttime > 0)
                    {
                        counttime--;
                        goto recheck6GITFDown;
                    }
                    DisplayMsg(LogType.Log, "Check 6G PCIE Interface Down fail");
                    AddData(item, 1);
                    return;
                }

                if (CheckGoNoGo())
                {
                    AddData(item, 0);
                    DisplayMsg(LogType.Log, "Check PCIe Interface Pass");
                }
                else
                { AddData(item, 1); DisplayMsg(LogType.Log, "Check PCIe Interface fail"); return; }


                /*DisplayMsg(LogType.Log, "Check WiFi 5G PCIe Interface");
                result = SendAndChk(item, PortType.SSH, "lspci -s 0000:00:00.0 -vv | grep Speed", "LnkSta:\tSpeed 8GT/s (ok), Width x1 (ok)", 0, 3000);
                result &= SendAndChk(item, PortType.SSH, "lspci -s 0000:01:00.0 -vv | grep Speed", "LnkSta:\tSpeed 8GT/s (ok), Width x1 (downgraded)", 0, 3000);
                DisplayMsg(LogType.Log, "Check WiFi 6G PCIe Interface");
                result &= SendAndChk(item, PortType.SSH, "lspci -s 0001:00:00.0 -vv | grep Speed", "LnkSta:\tSpeed 8GT/s (ok), Width x2 (ok)", 0, 3000);
                result &= SendAndChk(item, PortType.SSH, "lspci -s 0001:01:00.0 -vv | grep Speed", "LnkSta:\tSpeed 8GT/s (ok), Width x2 (ok)", 0, 3000);

                if (result)
                {
                    DisplayMsg(LogType.Log, "Check PCIe Interface Pass");
                    AddData(item, 0);
                }
                else
                {
                    DisplayMsg(LogType.Log, "Check PCIe Interface fail");
                    AddData(item, 1);
                }*/
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
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


                    if (fixture.useFixture)
                    {
                        fixture.ControlIO(Fixture.FixtureIO.IO_6, CTRL.ON);


                    }
                    else
                    {
                        frmOK.Label = "Nhấn và giữ nút 'WPS màu nâu', sau đó nhấn \"Xác nhận\"";
                        frmOK.ShowDialog();
                    }
                    SendAndChk(PortType.SSH, "mt gpio dump all", keyword, out res, 0, 3000);
                    if (res.Contains("WPS: low"))
                    {
                        pressed = true;
                        DisplayMsg(LogType.Log, "Check WPS Button pressed ok");
                    }
                    if (fixture.useFixture)
                    {
                        fixture.ControlIO(Fixture.FixtureIO.IO_6, CTRL.OFF);


                    }
                    else
                    {
                        frmOK.Label = "Nhả nút 'WPS màu nâu', sau đó nhấn \"Xác nhận\"";
                        frmOK.ShowDialog();
                    }
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
                    if (fixture.useFixture)
                    {
                        fixture.ControlIO(Fixture.FixtureIO.IO_7, CTRL.ON);

                    }
                    else
                    {
                        frmOK.Label = "Nhấn và giữ nút 'Reset màu đỏ', sau đó nhấn \"Xác nhận\"";
                        frmOK.ShowDialog();
                    }
                    SendAndChk(PortType.SSH, "mt gpio dump all", keyword, out res, 0, 3000);
                    if (res.Contains("RESET: low"))
                    {
                        pressed = true;
                        DisplayMsg(LogType.Log, "Check Reset Button pressed ok");
                    }
                    if (fixture.useFixture)
                    {
                        fixture.ControlIO(Fixture.FixtureIO.IO_7, CTRL.OFF);

                    }
                    else
                    {
                        frmOK.Label = "Nhả nút 'Reset màu đỏ', sau đó nhấn \"Xác nhận\"";
                        frmOK.ShowDialog();
                    }
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
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                AddData(item, 1);
            }
        }
        private void CheckFWVerAndHWID()
        {
            if (!CheckGoNoGo() || isGolden)
            {
                return;
            }

            string item = "ChkFWVer";
            string keyword = "root@OpenWrt:~# \r\n";
            string res = "";
            string FWversion = "";
            string Chipversion = "";
            string HWID = "";

            try
            {
                DisplayMsg(LogType.Log, "=============== Check FW/Chip version & HW ID ===============");

                #region Chip version
                //Frank update test plan get chipversion and update to SFCS
                //Check chip version
                //Check chip version between SFCS and DUT
                int retrydevmen = 0;
            retrydevmen:
                SendAndChk(PortType.SSH, "devmem 0xA0000", keyword, out res, 0, 3000);
                Match m = Regex.Match(res, @"0xA0000[\r\n]+(?<Chipver>.+)");
                if (m.Success)
                {
                    Chipversion = m.Groups["Chipver"].Value.Trim();
                }
                DisplayMsg(LogType.Log, "Chipversion: " + Chipversion);
                DisplayMsg(LogType.Log, "Setting_Chipversion: " + infor.Chipver);

                if (string.Compare(Chipversion, infor.Chipver, true) == 0)
                {
                    DisplayMsg(LogType.Log, "Check Chip Version Between Setting and DUT PASS");
                    status_ATS.AddDataRaw("LMG1_Chipversion", Chipversion, Chipversion, "000000");
                }
                else
                {
                    retrydevmen++;
                    DisplayMsg(LogType.Log, "Check Chip Version Between Setting and DUT fail");
                    DisplayMsg(LogType.Log, $"Retry send 'devmem 0xA0000' command {retrydevmen} time");
                    if (retrydevmen < 3) goto retrydevmen;
                    AddData(item, 1);
                    return;
                }

                /*                //Check chip version between setting and DUT
                                DisplayMsg(LogType.Log, "Chipversion: " + Chipversion);
                                DisplayMsg(LogType.Log, "Setting Chipversion: " + infor.St_Chipver);

                                if (string.Compare(Chipversion, infor.St_Chipver, true) == 0)
                                {
                                    DisplayMsg(LogType.Log, "Check Chip Version Between Setting and DUT PASS");
                                    status_ATS.AddDataRaw("LMG1_Chipversion", Chipversion, Chipversion, "000000");
                                }
                                else
                                {
                                    AddData(item, 1);
                                    DisplayMsg(LogType.Log, "Check Chip Version Between Setting and DUT fail");
                                    return;
                                }*/
                #endregion Chip version


                #region FW version
                int retry = 3;
            mtinfo_retry:
                //Check FW version between SFCS and DUT
                SendAndChk(PortType.SSH, "mt info", keyword, out res, 0, 3000);
                //Rena_20230803, FW v0.1.2.5 timing issue
                if (res.Contains("can't open '/tmp/hwid'") && retry-- > 0)
                {
                    DisplayMsg(LogType.Log, "mt info retry...");
                    Thread.Sleep(200);
                    goto mtinfo_retry;
                }

                m = Regex.Match(res, @"FW Version: (?<FWver>.+)");
                if (m.Success)
                {
                    FWversion = m.Groups["FWver"].Value.Trim();
                }

                DisplayMsg(LogType.Log, "FWversion: " + FWversion);
                DisplayMsg(LogType.Log, "SFCS_FWversion:" + infor.FWver);

                string FWversion1 = FWversion.Substring(6);

                //Mai check
                if (string.Compare(FWversion1, infor.FWver, true) == 0)
                {
                    AddData(item, 0);
                    DisplayMsg(LogType.Log, "Check FW Version between SFCS and DUT PASS");
                    status_ATS.AddDataRaw("LMG1_Label_MFGVER", infor.FWver, infor.FWver, "000000");
                }
                else
                {
                    AddData(item, 1);
                    DisplayMsg(LogType.Log, "FWversion processed is: " + FWversion1);
                    DisplayMsg(LogType.Log, "SFCS_FWversion:" + infor.FWver);
                    DisplayMsg(LogType.Log, "Check FW Version between SFCS and DUT fail");
                    return;
                }

                #endregion FW version


                #region HWID
                //check HW ID SFCS and DUT
                item = "ChkHWID";
                //HW Version (GPIO): 1001
                m = Regex.Match(res, @"HW Version \(GPIO\): (?<HWID>.+)");
                if (m.Success)
                {
                    HWID = m.Groups["HWID"].Value.Trim();
                }

                DisplayMsg(LogType.Log, "HWID: " + HWID);
                DisplayMsg(LogType.Log, "SFCS_SFCS_HWID:" + infor.HWID);
                string SFCS_HWID = infor.HWID;
                infor.HWID = infor.HWID.Substring(infor.HWID.Length - 1);
                //DisplayMsg(LogType.Log, $"HWID: {infor.HWID}");
                if (string.Compare(HWID, infor.HWID, true) == 0)
                {
                    AddData(item, 0);
                    DisplayMsg(LogType.Log, "Check HW ID Between SFCS and DUT PASS");
                    status_ATS.AddDataRaw("LMG1_Label_HWID", SFCS_HWID, SFCS_HWID, "000000");
                }
                else
                {
                    AddData(item, 1);
                    DisplayMsg(LogType.Log, "Check HW ID Between SFCS and DUT fail");
                }


                /*                //Check HWID between Setting and DUT
                                DisplayMsg(LogType.Log, "HWID: " + HWID);
                                DisplayMsg(LogType.Log, "Setting HWID:" + infor.St_HWID);

                                if (string.Compare(HWID, infor.St_HWID, true) == 0)
                                {
                                    AddData(item, 0);
                                    DisplayMsg(LogType.Log, "Check HW ID Between Setting and DUT PASS");
                                    status_ATS.AddDataRaw("LMG1_Label_HWID", HWID, HWID, "000000");
                                }
                                else
                                {
                                    AddData(item, 1);
                                    DisplayMsg(LogType.Log, "Check HW ID Between Setting and DUT fail");
                                }*/
                #endregion HWID
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                AddData(item, 1);
            }
        }
        private void ClearENV()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            string item = "ClearENV";
            string keyword = "root@OpenWrt:~# \r\n";
            string res = "";

            try
            {
                DisplayMsg(LogType.Log, "=============== Clear ENV setting ===============");

                //Clear block 13
                SendAndChk(PortType.SSH, "dd if=/dev/zero of=/dev/mmcblk0p13", keyword, out res, 0, 3000);
                if (!res.Contains("262144 bytes (256.0KB) copied"))
                {
                    AddData(item, 1);
                    DisplayMsg(LogType.Log, "Clear block 13 fail");
                    return;
                }
                SendAndChk(PortType.SSH, "hexdump /dev/mmcblk0p13", keyword, out res, 0, 3000);
                res = res.Replace("\r\n", " ");
                if (!res.Contains("0000000 0000 0000 0000 0000 0000 0000 0000 0000 * 0040000"))
                {
                    AddData(item, 1);
                    DisplayMsg(LogType.Log, "Clear block 13 fail");
                    return;
                }
                else
                {
                    DisplayMsg(LogType.Log, "Clear block 13 pass");
                }

                //Clear block 14
                SendAndChk(PortType.SSH, "dd if=/dev/zero of=/dev/mmcblk0p14", keyword, out res, 0, 3000);
                if (!res.Contains("262144 bytes (256.0KB) copied"))
                {
                    AddData(item, 1);
                    DisplayMsg(LogType.Log, "Clear block 14 fail");
                    return;
                }
                SendAndChk(PortType.SSH, "hexdump /dev/mmcblk0p14", keyword, out res, 0, 3000);
                res = res.Replace("\r\n", " ");
                if (!res.Contains("0000000 0000 0000 0000 0000 0000 0000 0000 0000 * 0040000"))
                {
                    AddData(item, 1);
                    DisplayMsg(LogType.Log, "Clear block 14 fail");
                    return;
                }
                else
                {
                    DisplayMsg(LogType.Log, "Clear block 14 pass");
                }

                if (CheckGoNoGo())
                {
                    AddData(item, 0);
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
        private bool CheckCameraResult(string item, string keyword, out string temp1)
        {
            bool rs = false;
            Directory.SetCurrentDirectory(sExeDirectory);
            temp1 = WNC.API.Func.ReadINI("cam_result", "result", item, "");
            DisplayMsg(LogType.Log, $"Camera Result:{temp1}, SPEC:{keyword}");
            try
            {
                if (temp1.ToUpper().Contains(keyword.ToUpper()))
                {
                    rs = true;
                }
                else
                {
                    //if (captureCamera)
                    //{
                    //    try
                    //    {
                    //        string image = Path.Combine(sExeDirectory, "Rect_Pic_11.png");
                    //        string file = @"D:\ImageFail\" + status_ATS.txtPSN.Text + "_" + DateTime.Now.ToString("hhmmssfff") + ".png";
                    //        DisplayMsg(LogType.Log, image);
                    //        if (File.Exists(image))
                    //        {
                    //            if (!Directory.Exists(@"D:\ImageFail"))
                    //                Directory.CreateDirectory(@"D:\ImageFail");

                    //            File.Copy(image, file, true);
                    //            DisplayMsg(LogType.Log, $"Copy {image} to {file}");
                    //        }
                    //    }
                    //    catch (Exception ex)
                    //    {

                    //        DisplayMsg(LogType.Log, "Backup image exception:" + ex.ToString());
                    //    }

                    //}

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
        private void DownloadFilesRequired(bool IsFinalstation)
        {
            if (!CheckGoNoGo())
            {
                return;
            }
            string item = "Download Files Required";
            string keyword = "root@OpenWrt:~# \r\n";
            string sIP = WNC.API.Func.ReadINI("Setting", "PCBA", "IP", "10.166.251.128");
            string PCBA_IP = WNC.API.Func.ReadINI("Setting", "PCBA", "PCBA_IP", "192.168.1.2");
            string etherMac = WNC.API.Func.ReadINI("Setting", "PCBA", "ethermac", "C0:18:50:F9:AA:12");
            string serverIP = WNC.API.Func.ReadINI("Setting", "PCBA", "SeverIP", "10.169.100.108");
            string linkrate1 = Func.ReadINI("Setting", "LinkRate", "linkratesever", "1000");
            string port1 = Func.ReadINI("Setting", "LinkRate", "portsever", "0");
            string res = string.Empty;

            DisplayMsg(LogType.Log, $"=============== {item} ===============");
            try
            {
                //Config MAC,IP for DUT
                SendAndChk(PortType.SSH, "brctl delif br-lan eth0", keyword, out res, 0, 3000);
                SendAndChk(PortType.SSH, $"ifconfig eth0 {sIP}", keyword, out res, 0, 3000);
                SendAndChk(PortType.SSH, "ifconfig eth0 down", keyword, out res, 0, 3000);
                SendAndChk(PortType.SSH, $"ifconfig eth0 hw ether {etherMac}", keyword, out res, 0, 3000);
                SendAndChk(PortType.SSH, "ifconfig eth0 up", keyword, out res, 0, 3000);
                //==================== follow test plan testing sequences =========================
                this.Chkboarddata();
                //==================== follow test plan testing sequences =========================
                if (Func.ReadINI("Setting", "IO_Board_Control2", "IO_Control_2", "0") == "1")
                {
                    for (int i = 0; i < 8; i++)
                    {
                        string rev_message = "";
                        status_ATS.AddLog("IO_Board_Y" + i + " On...");
                        IO_Board_Control2.ConTrolIOPort_write(i, "1", ref rev_message);
                        DisplayMsg(LogType.Log, rev_message);
                    }

                }
                DisplayMsg(LogType.Log, $"Delay 10 (s) ...");
                Thread.Sleep(10000);


                SendAndChk(PortType.SSH, "mt eth linkrate", keyword, out res, 0, 3000);
                if (port1 == "1")
                {
                    if (res.Contains($"port 1: {linkrate1}M FD"))
                    {
                        DisplayMsg(LogType.Log, "Check LAN Port1 pass");
                    }
                    else
                    {
                        DisplayMsg(LogType.Log, "Check LAN Port1 fail");
                        AddData("Eth_LAN_Port1", 1);
                    }
                }
                // ----------------- open Path TFTP  ----------------------
                SendAndChk(PortType.SSH, "tftp -gr ssh.bin 192.168.1.2", keyword, out res, 0, 3000);
                SendAndChk(PortType.SSH, "tftp -gr scp.bin 192.168.1.2", keyword, out res, 0, 3000);
                SendAndChk(PortType.SSH, "tftp -gr moduli 192.168.1.2", keyword, out res, 0, 3000);
                SendAndChk(PortType.SSH, "tftp -gr ssh_config 192.168.1.2", keyword, out res, 0, 3000);

                //remove old folder and copy...
                SendAndChk(PortType.SSH, "rm /usr/bin/ssh", keyword, out res, 0, 3000);
                SendAndChk(PortType.SSH, "rm /usr/bin/scp", keyword, out res, 0, 3000);
                SendAndChk(PortType.SSH, "cp ssh.bin /usr/bin/ssh", keyword, out res, 0, 3000);
                SendAndChk(PortType.SSH, "cp scp.bin /usr/bin/scp", keyword, out res, 0, 3000);
                SendAndChk(PortType.SSH, "chmod +x /usr/bin/ssh", keyword, out res, 0, 3000);
                SendAndChk(PortType.SSH, "chmod +x /usr/bin/scp", keyword, out res, 0, 3000);
                SendAndChk(PortType.SSH, "mkdir /etc/ssh", keyword, out res, 0, 3000);
                SendAndChk(PortType.SSH, "cp ssh_config /etc/ssh/", keyword, out res, 0, 3000);
                SendAndChk(PortType.SSH, "cp moduli /etc/ssh", keyword, out res, 0, 3000);

                // ----------------- Plug in the Ethernet cable  ----------------------
                if (!SendAndChk(PortType.SSH, $"scp lxg1@{serverIP}:/home/lxg1/BT_LMG1/*  /tmp/", "", out res, 3000, 15000))
                {
                    warning = $"Send 'scp lxg1@{serverIP}:/home/lxg1/BT_LMG1/*  /tmp/' fail";
                    return;
                }
                SendAndChk(PortType.SSH, "yes", $"", out res, 3000, 5000);
                if (!SendAndChk(PortType.SSH, "wnc000000", "", out res, 0, 3000))
                {
                    warning = $"Send Password is'wnc000000' fail";
                    return;
                }
                SendAndChk(PortType.SSH, "\r\n", keyword, out res, 0, 3000);
                SendAndChk(PortType.SSH, "ls /tmp", keyword, out res, 0, 3000);
                // ------------------------------------------------------------------------
                if (IsFinalstation)
                {
                    this.chkMD5sum(keyword);
                }
                // ------------------------------------
                SendAndChk(PortType.SSH, "chmod 777 /tmp/filesystem_encryption.sh", "", out res, 0, 3000);
                SendAndChk(PortType.SSH, "chmod 777 /tmp/secure_boot_transition.sh", "", out res, 0, 3000);
                SendAndChk(PortType.SSH, "chmod 777 /tmp/qualcomm.sh", "", out res, 0, 3000);
                // ------------------------------------
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, item + "____" + ex.Message);
                AddData(item, 1);
                return;
            }
        }
        private void DownloadAllConfigs()
        {
            if (!CheckGoNoGo())
            {
                return;
            }
            string item = "Download All Configs";
            string keyword = "root@OpenWrt:~# \r\n";
            string serverIP = WNC.API.Func.ReadINI("Setting", "PCBA", "SeverIP", "");
            string res = string.Empty;
            DisplayMsg(LogType.Log, $"=============== {item} ===============");
            try
            {
                SendAndChk(PortType.SSH, "mkdir /tmp/config", "", out res, 500, 3000);
                SendAndChk(PortType.SSH, $"scp lxg1@{serverIP}:/home/lxg1/BT_LMG1/config/*  /tmp/config/", "", out res, 500, 3000);
                SendAndChk(PortType.SSH, "wnc000000", "", out res, 3000, 5000);
                SendAndChk(PortType.SSH, "\r\n", "", out res, 1000, 3000);
                SendAndChk(PortType.SSH, "ls /tmp/config", keyword, out res, 0, 3000);
                if (!res.Contains("config.schema") || !res.Contains("sw40j.json"))
                {
                    DisplayMsg(LogType.Log, $"Can't catch 'config.schema' or 'sw40j.json' keyword");
                    AddData("Dowload_cfg_Fail", 1);
                    return;
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, item + "____" + ex.Message);
                AddData(item, 1);
                return;
            }
        }
        private void FilesystemEncryption(bool NeeedErase)
        {
            string item = "Filesystem Encryption";
            string keyword = "root@OpenWrt:~#";
            string serverIP = WNC.API.Func.ReadINI("Setting", "PCBA", "SeverIP", "");
            string res = string.Empty;
            int retrycount = 0;
            DisplayMsg(LogType.Log, $"=============== {item} ===============");
            try
            {
                if (NeeedErase)
                {
                    SendAndChk(PortType.SSH, "dd if=/dev/zero of=/dev/mmcblk0p29", keyword, out res, 0, 3000);
                }
                SendAndChk(PortType.SSH, $"umount /dev/mmcblk0p28", keyword, out res, 500, 3000);
                SendAndChk(PortType.SSH, $"cd /tmp", "root@OpenWrt:/tmp", out res, 0, 5000);
                SendAndChk(PortType.SSH, $"chmod u+x *.sh", "root@OpenWrt:/tmp", out res, 0, 5000);
            ChkRetryCount:
                if (SendAndChk(PortType.SSH, "./filesystem_encryption.sh -c config/sw40j.json", "SUCCESS", out res, 12000, 6000))
                {
                    DisplayMsg(LogType.Log, @"Check SUCCESS OK");
                    return;
                }
                else
                {
                    if (retrycount++ < 3)
                    {
                        DisplayMsg(LogType.Log, @"Check 'SUCCESS' Fail");
                        DisplayMsg(LogType.Log, $"Retry send command: {retrycount}");
                        goto ChkRetryCount;
                    }
                    else
                    {
                        DisplayMsg(LogType.Error, @"FilesystemEncryption NG");
                        AddData(item, 1);
                        return;
                    }

                }
                if (!CheckGoNoGo())
                {
                    DisplayMsg(LogType.Error, @"FilesystemEncryption NG");
                    AddData(item, 1);
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, item + "____" + ex.Message);
                AddData(item, 1);
            }
            finally
            {
                Thread.Sleep(2000);
                SendAndChk(PortType.SSH, $"cd ~", keyword, out res, 100, 3000);
                Thread.Sleep(1000);
            }
        }
        private void SecureBootTransition()
        {
            string item = "Secure Boot Transition";
            string keyword = "root@OpenWrt:/tmp#";
            string serverIP = WNC.API.Func.ReadINI("Setting", "PCBA", "SeverIP", "");
            string res = string.Empty;
            DisplayMsg(LogType.Log, $"=============== {item} ===============");
            try
            {
                SendAndChk(PortType.SSH, "cat /sys/devices/system/qfprom/qfprom0/authenticate", keyword, out res, 0, 3000);
                if (!res.Contains("1"))
                {
                    DisplayMsg(LogType.Log, @"Secure boot not enable");
                    SendAndChk(PortType.SSH, "cd /tmp", keyword, out res, 0, 3000);
                    SendAndChk(PortType.SSH, "./secure_boot_transition.sh -c config/sw40j.json", keyword, out res, 0, 5000);
                    //AddData(item, 1);
                    return;
                }
                DisplayMsg(LogType.Log, @"Secure boot enable ok! Skip_secure_boot_transition_sh");

                // ===================== SSH wont show, only show on Successfully in Uart ==========================
                //if (!res.Contains("Successfully loaded app and services"))
                //{
                //    DisplayMsg(LogType.Log, @"---- loaded invaild -----");
                //    AddData(item, 1);
                //    return;
                //}
                DisplayMsg(LogType.Log, @"Enable fuse and QSApp binaries");
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, item + "____" + ex.Message);
                AddData(item, 1);
            }
            finally
            {
                for (int i = 0; i < 5; i++)
                {
                    SendCommand(PortType.SSH, "cd ~", 5000);
                    if (ChkResponse(PortType.SSH, ITEM.NONE, "root@OpenWrt:~#", out res, 5000))
                    {
                        break;
                    }
                }

            }
        }
        public void DownloadDisgue2()
        {
            string item = "Download All Configs";
            string keyword = "root@OpenWrt:~# \r\n";
            string res = string.Empty;
            DisplayMsg(LogType.Log, $"=============== {item} ===============");
            try
            {
                SendAndChk(PortType.SSH, "mkdir /tmp/config", "", out res, 0, 3000);
                SendAndChk(PortType.SSH, "cp /overlay1/BT_LMG1_config/* /tmp/config/", "", out res, 0, 3000);
                SendAndChk(PortType.SSH, "ls /tmp/config", "", out res, 0, 3000);
                //SendAndChk(PortType.SSH, "chmod 777 secure_boot_transition.sh", "", out res, 0, 3000);
                SendAndChk(PortType.SSH, "mkdir -p /mnt/data", keyword, out res, 0, 3000);
                SendAndChk(PortType.SSH, "mkdir -p /mnt/defaults", keyword, out res, 0, 3000);
                SendAndChk(PortType.SSH, "mount -t ext4 /dev/mmcblk0p28 /mnt/data", keyword, out res, 0, 3000);
                SendAndChk(PortType.SSH, "mount | grep /dev/mmcblk0p28", keyword, out res, 2000, 3000);
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, item + "____" + ex.Message);
                AddData(item, 1);
            }

        }
        public void DownloadDisgue(bool isFinalStation)
        {
            string item = "Download Files Required";
            string keyword = "root@OpenWrt:~# \r\n";
            string res = string.Empty;
            string md5sum_secDat = WNC.API.Func.ReadINI("Setting", "PCBA", "secDat", "b114d555be9344cdc5d6eb93c467cc1e");
            string fscrypt_context = WNC.API.Func.ReadINI("Setting", "PCBA", "fscrypt_context", "8e3c69e485f2fe8b2df7be686aa63568");
            string cmnlib64_mdt = WNC.API.Func.ReadINI("Setting", "PCBA", "cmnlib64_mdt", "a54b4327f31dbece855325947115f523");
            string cmnlib64_b06 = WNC.API.Func.ReadINI("Setting", "PCBA", "cmnlib64_b06", "7bcd9170096db6346d73fe9f48c2d381");
            string fuseprov_b08 = WNC.API.Func.ReadINI("Setting", "PCBA", "fuseprov_b08", "4e3fcc6ce8a27c9474a3bd22eba76a46");
            string fuseprov_mdt = WNC.API.Func.ReadINI("Setting", "PCBA", "fuseprov_mdt", "d88e89e3228d7a883fa27c94012a03e8");
            DisplayMsg(LogType.Log, $"=============== {item} ===============");
            try
            {
                SendAndChk(PortType.SSH, "cp /overlay1/BT_LMG1/* /tmp/", "", out res, 0, 3000);
                if (isFinalStation)
                {
                    return;
                }
                SendAndChk(PortType.SSH, "md5sum /tmp/sec.dat", keyword, out res, 0, 3000);
                if (!res.Contains(md5sum_secDat))
                {
                    DisplayMsg(LogType.Log, @"md5sum >>> NG");
                    AddData(item, 1);
                }

                SendAndChk(PortType.SSH, "md5sum /tmp/fscrypt_context", keyword, out res, 0, 3000);
                if (!res.Contains(fscrypt_context))
                {
                    DisplayMsg(LogType.Log, @"md5sum >>> NG");
                    AddData(item, 1);
                }

                SendAndChk(PortType.SSH, "md5sum /tmp/cmnlib64.mdt", keyword, out res, 0, 3000);
                if (!res.Contains(cmnlib64_mdt))
                {
                    DisplayMsg(LogType.Log, @"md5sum >>> NG");
                    AddData(item, 1);
                }
                SendAndChk(PortType.SSH, "md5sum /tmp/cmnlib64.b06", keyword, out res, 0, 3000);
                if (!res.Contains(cmnlib64_b06))
                {
                    DisplayMsg(LogType.Log, @"md5sum >>> NG");
                    AddData(item, 1);
                }
                SendAndChk(PortType.SSH, "md5sum /tmp/fuseprov.b08", keyword, out res, 0, 3000);
                if (!res.Contains(fuseprov_b08))
                {
                    DisplayMsg(LogType.Log, @"md5sum >>> NG");
                    AddData(item, 1);
                }
                SendAndChk(PortType.SSH, "md5sum /tmp/fuseprov.mdt", keyword, out res, 0, 3000);
                if (!res.Contains(fuseprov_mdt))
                {
                    DisplayMsg(LogType.Log, @"md5sum >>> NG");
                    AddData(item, 1);
                }
                // ------------------------------------
                SendAndChk(PortType.SSH, "chmod 777 /tmp/filesystem_encryption.sh", "", out res, 0, 3000);
                SendAndChk(PortType.SSH, "chmod 777 /tmp/secure_boot_transition.sh", "", out res, 0, 3000);
                SendAndChk(PortType.SSH, "chmod 777 /tmp/qualcomm.sh", "", out res, 0, 3000);
                // ------------------------------------

            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, item + "____" + ex.Message);
                AddData(item, 1);
            }

        }
        public void ChkEMMC()
        {
            string item = "ChkEMMC";
            string keyword = "IPQ5332#";
            DisplayMsg(LogType.Log, $"=============== {item} ===============");
            int retry_cnt = 3;
            string res = "";

            try
            {
            //stop device in bootloader
            Enter_bootloader:
                if (!BootLoader(uart))
                {
                    DisplayMsg(LogType.Log, "Enter bootloader fail");
                    if (retry_cnt-- > 0)
                    {
                        DisplayMsg(LogType.Log, "Reboot DUT and retry...");
                        frmOK.Label = "Vui lòng nhấn nút OK trước, sau đó khởi động lại thiết bị (DUT)";
                        frmOK.ShowDialog();
                        goto Enter_bootloader;
                    }
                    else
                    {
                        AddData(item, 1);
                        return;
                    }
                }
                SendAndChk(PortType.SSH, "mmc info", keyword, out res, 0, 5000);
                if (!res.Contains("Capacity"))
                {
                    AddData(item, 1);
                    return;
                }
                SendAndChk(PortType.SSH, "reset", keyword, out res, 0, 5000);

            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, item + "____" + ex.Message);
                AddData(item, 1);
            }
        }
        public void chkMD5sum(string keyword)
        {
            string md5sum_secDat = WNC.API.Func.ReadINI("Setting", "PCBA", "secDat", "30af4b549a3233ccaf7c6e6d9342447f");
            string fscrypt_context = WNC.API.Func.ReadINI("Setting", "PCBA", "fscrypt_context", "003e19d329ac299d425c20a653facc76");
            string cmnlib64_mdt = WNC.API.Func.ReadINI("Setting", "PCBA", "cmnlib64_mdt", "7f6b21b3386d1dbcc85975195a3a9f1d");
            string cmnlib64_b06 = WNC.API.Func.ReadINI("Setting", "PCBA", "cmnlib64_b06", "d878f75c3b9bf86753598fa7efe309ce");
            string fuseprov_b08 = WNC.API.Func.ReadINI("Setting", "PCBA", "fuseprov_b08", "a5bf2b24af9c4ef18739627fb91bd978");
            string fuseprov_mdt = WNC.API.Func.ReadINI("Setting", "PCBA", "fuseprov_mdt", "6e5ab46437f6cc19ff7853304bd918fb");
            string item = "chkMD5sum";
            string res = "";

            try
            {
                SendAndChk(PortType.SSH, "md5sum /tmp/sec.dat", keyword, out res, 0, 3000);
                if (!res.Contains(md5sum_secDat))
                {
                    DisplayMsg(LogType.Log, @"md5sum >>> NG");
                    AddData(item, 1);
                    return;
                }
                else
                {
                    DisplayMsg(LogType.Log, $"md5sum 'sec.dat' is {md5sum_secDat} Pass");
                }

                SendAndChk(PortType.SSH, "md5sum /tmp/fscrypt_context", keyword, out res, 0, 3000);
                if (!res.Contains(fscrypt_context))
                {
                    DisplayMsg(LogType.Log, @"md5sum >>> NG");
                    AddData(item, 1);
                    return;
                }
                else
                {
                    DisplayMsg(LogType.Log, $"md5sum 'fscrypt_context' is {fscrypt_context} Pass");
                }

                SendAndChk(PortType.SSH, "md5sum /tmp/cmnlib64.mdt", keyword, out res, 0, 3000);
                if (!res.Contains(cmnlib64_mdt))
                {
                    DisplayMsg(LogType.Log, @"md5sum >>> NG");
                    AddData(item, 1);
                    return;
                }
                else
                {
                    DisplayMsg(LogType.Log, $"md5sum 'cmnlib64.mdt' is {cmnlib64_mdt} Pass");
                }
                SendAndChk(PortType.SSH, "md5sum /tmp/cmnlib64.b06", keyword, out res, 0, 3000);
                if (!res.Contains(cmnlib64_b06))
                {
                    DisplayMsg(LogType.Log, @"md5sum >>> NG");
                    AddData(item, 1);
                    return;
                }
                else
                {
                    DisplayMsg(LogType.Log, $"md5sum 'cmnlib64.b06' is {cmnlib64_b06} Pass");
                }
                SendAndChk(PortType.SSH, "md5sum /tmp/fuseprov.b08", keyword, out res, 0, 3000);
                if (!res.Contains(fuseprov_b08))
                {
                    DisplayMsg(LogType.Log, @"md5sum >>> NG");
                    AddData(item, 1);
                    return;
                }
                else
                {
                    DisplayMsg(LogType.Log, $"md5sum 'fuseprov.b08' is {fuseprov_b08} Pass");
                }
                SendAndChk(PortType.SSH, "md5sum /tmp/fuseprov.mdt", keyword, out res, 0, 3000);
                if (!res.Contains(fuseprov_mdt))
                {
                    DisplayMsg(LogType.Log, @"md5sum >>> NG");
                    AddData(item, 1);
                    return;
                }
                else
                {
                    DisplayMsg(LogType.Log, $"md5sum 'fuseprov.mdt' is {fuseprov_mdt} Pass");
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, item + "____" + ex.Message);
                AddData(item, 1);
            }
        }
    }
}

