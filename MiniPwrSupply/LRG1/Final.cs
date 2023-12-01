using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
//using WNC.API;
//using EasyLibrary;
using System.Net.Security;
using System.Runtime.CompilerServices;

namespace MiniPwrSupply.LRG1
{
    public partial class frmMain
    {
        private void Final()
        {
            try
            {
                infor.ResetParam();
                OpenTftpd32(Application.StartupPath);
                SFCS_Query _sfcsQuery = new SFCS_Query();
                ATS_Template.SFCS_ATS_2_0.ATS ss = new ATS_Template.SFCS_ATS_2_0.ATS();
                string res = string.Empty;

                #region Check label
                string label_NETWORKNAME = string.Empty;
                string label_IMEI = string.Empty;
                string label_ICCID = string.Empty;
                string label_QR = string.Empty;
                if (status_ATS.txtSP.Text.Contains(";"))
                {
                    label_QR = status_ATS.txtSP.Text;
                    DisplayMsg(LogType.Log, "LABLE QR: " + label_QR);
                    // label_QR = "WIFI:S:EE-WPTZ5S;T:WPA;P:FYC3ei6kpbWmnP9v;;4iyfedpg;";
                    string ssidPattern = @"S:(.*?);";
                    string wifiPasswordPattern = @"P:(.*?);";
                    string adminPasswordPattern = @";;(.*?);";
                    string ssid = GetMatchedValue(label_QR, ssidPattern);
                    string wifiPassword = GetMatchedValue(label_QR, wifiPasswordPattern);
                    string adminPassword = GetMatchedValue(label_QR, adminPasswordPattern);
                    /*ssid = ssid.ToUpper();
                    wifiPassword = wifiPassword.ToUpper();
                    adminPassword = adminPassword.ToUpper();*/
                    DisplayMsg(LogType.Log, "LRG1_LABEL_NETWORK Label: " + ssid);
                    DisplayMsg(LogType.Log, "LRG1_LABEL_PW Label: " + wifiPassword);
                    DisplayMsg(LogType.Log, "LRG1_LABEL_ADMIN_PW From Label: " + adminPassword);

                }
                else
                {
                    warning = "Get Label data fail";
                    return;
                }

                if (status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode)//Jason add
                {
                    string sfcsSSID = string.Empty;
                    string sfcPW = string.Empty;
                    string sfcadminpw = string.Empty;
                    _sfcsQuery.Get15Data(status_ATS.txtPSN.Text, "LRG1_LABEL_NETWORK", ref sfcsSSID);
                    _sfcsQuery.Get15Data(status_ATS.txtPSN.Text, "LRG1_LABEL_PW", ref sfcPW);
                    _sfcsQuery.Get15Data(status_ATS.txtPSN.Text, "LRG1_LABEL_ADMIN_PW", ref sfcadminpw);

                    if (string.IsNullOrEmpty(sfcsSSID) || string.IsNullOrEmpty(sfcPW) || string.IsNullOrEmpty(sfcadminpw)
                   || sfcsSSID.Contains("Dut not have") || sfcPW.Contains("Dut not have") || sfcadminpw.Contains("Dut not have"))
                    {
                        warning = "Get from SFCS FAIL";
                        return;
                    }

                    /*sfcsSSID = sfcsSSID.ToUpper();
                    sfcPW = sfcPW.ToUpper();
                    sfcadminpw = sfcadminpw.ToUpper();*/
                    infor.SerialNumber = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@LRG1_SN");
                    DisplayMsg(LogType.Log, "LRG1_LABEL_NETWORK from SFCS:" + sfcsSSID);
                    DisplayMsg(LogType.Log, "LRG1_LABEL_PW from SFCS:" + sfcPW);
                    DisplayMsg(LogType.Log, "LRG1_LABEL_ADMIN_PW from SFCS:" + sfcadminpw);
                    if (infor.SerialNumber.Length == 18 && infor.SerialNumber == status_ATS.txtPSN.Text)
                    {
                        SetTextBox(status_ATS.txtPSN, infor.SerialNumber);
                        //SetTextBox(status_ATS.txtSP, infor.BaseMAC);
                        status_ATS.SFCS_Data.PSN = infor.SerialNumber;
                        status_ATS.SFCS_Data.First_Line = infor.SerialNumber;
                    }
                    else
                    {
                        warning = "Get SN from SFCS fail";
                        return;
                    }

                    if (status_ATS.txtPSN.Text != infor.SerialNumber)
                    {
                        warning = "Check label SN fail";
                        return;
                    }


                    if (!label_QR.Contains(sfcsSSID))
                    {
                        warning = "Check Network Name SSID with SFCS fail";
                        return;
                    }

                    if (!label_QR.Contains(sfcPW))
                    {
                        warning = "Check label PW with SFCS fail";
                        return;
                    }

                    if (!label_QR.Contains(sfcadminpw))
                    {
                        warning = "Check label ADMIN PW with SFCS fail";
                        return;
                    }
                }
                AddData("CheckLabel", 0);
                #endregion Check label
                #region create SMT file
                if (status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode)
                {
                    infor.BaseMAC = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@MAC");
                    infor.HWID = Func.ReadINI("Setting", "Final", "HWID", "xxxx");
                    infor.HWver = Func.ReadINI("Setting", "Final", "HWver", "xxxx");
                    infor.FWver = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@MFG_FW_17");
                    infor.WanMAC = MACConvert(infor.BaseMAC, 1);

                    _sfcsQuery.Get15Data(status_ATS.txtPSN.Text, "LGR1_RXTURN", ref infor.DECT_cal_rxtun);
                    _sfcsQuery.Get15Data(status_ATS.txtPSN.Text, "LRG1_BLE_Ver", ref infor.BLEver);
                    _sfcsQuery.Get15Data(status_ATS.txtPSN.Text, "LRG1_SE_Ver", ref infor.SEver);
                    _sfcsQuery.Get15Data(status_ATS.txtPSN.Text, "LRG1_DECT_rfpi", ref infor.DECT_rfpi);

                    _sfcsQuery.Get15Data(status_ATS.txtPSN.Text, "LRG1_LABEL_PW", ref infor.WiFi_PWD);
                    _sfcsQuery.Get15Data(status_ATS.txtPSN.Text, "LRG1_LABEL_ADMIN_PW", ref infor.Admin_PWD);
                    _sfcsQuery.Get15Data(status_ATS.txtPSN.Text, "LRG1_LABEL_NETWORK", ref infor.WiFi_SSID);
                    _sfcsQuery.Get15Data(status_ATS.txtPSN.Text, "LRG1_LICENSE_KEY", ref infor.License_key);
                    infor.SerialNumber = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@LRG1_SN");

                    DisplayMsg(LogType.Log, $"Get WiFi_PWD From SFCS is: {infor.WiFi_PWD}");
                    DisplayMsg(LogType.Log, $"Get Admin_PWD From SFCS is: {infor.Admin_PWD}");
                    DisplayMsg(LogType.Log, $"Get WiFi_SSID From SFCS is: {infor.WiFi_SSID}");
                    DisplayMsg(LogType.Log, $"Get License_key From SFCS is: {infor.License_key}");
                    DisplayMsg(LogType.Log, $"Get SerialNumber From SFCS is: {infor.SerialNumber}");

                    if (string.IsNullOrEmpty(infor.BaseMAC) || string.IsNullOrEmpty(infor.FWver) || string.IsNullOrEmpty(infor.WiFi_PWD) || string.IsNullOrEmpty(infor.Admin_PWD) || string.IsNullOrEmpty(infor.WiFi_SSID) || string.IsNullOrEmpty(infor.License_key)
                        || string.IsNullOrEmpty(infor.SerialNumber) || string.IsNullOrEmpty(infor.DECT_rfpi) || string.IsNullOrEmpty(infor.DECT_cal_rxtun)
                        || string.IsNullOrEmpty(infor.BLEver) || string.IsNullOrEmpty(infor.SEver) || string.IsNullOrEmpty(infor.BaseMAC))
                    {
                        warning = "Get from SFCS data fail";
                        return;
                    }


                    DisplayMsg(LogType.Log, $"Get Base MAC From SFCS is: {infor.BaseMAC}");
                    DisplayMsg(LogType.Log, $"Get FWver From SFCS is: {infor.FWver}");
                    string result = string.Empty;
                    result = infor.FWver.Substring(0, infor.FWver.Length - 9);
                    DisplayMsg(LogType.Log, $"Get FWver trim is: LRG1_ATH_{result}");
                    infor.FWver = "LRG1_ATH_" + result.ToLower();
                    DisplayMsg(LogType.Log, $"Get HWID From setting is: {infor.HWID}");
                    DisplayMsg(LogType.Log, $"Get HWver From setting is: {infor.HWver}");
                    DisplayMsg(LogType.Log, $"Get BaseMAC From SFCS is: {infor.BaseMAC}");
                    DisplayMsg(LogType.Log, $"Get WanMAC From SFCS is: {infor.WanMAC}");

                    DisplayMsg(LogType.Log, $"Get DECT_cal_rxtun From SFCS is: {infor.DECT_cal_rxtun}");
                    DisplayMsg(LogType.Log, $"Get BLEver From SFCS is: {infor.BLEver}");
                    DisplayMsg(LogType.Log, $"Get SEver From SFCS is: {infor.SEver}");
                    DisplayMsg(LogType.Log, $"Get DECT_rfpi From SFCS is: {infor.DECT_rfpi}");
                    if (!ChkStation(status_ATS.txtPSN.Text))
                    {
                        warning = "Check Station Fail";
                        return;
                    }
                    else
                    {
                        //Rena_20230407 add for HQ test
                        GetBoardDataFromExcel(status_ATS.txtPSN.Text, true);
                        infor.FWver = Func.ReadINI("Setting", "Final", "FWver", "xxxxxx");
                        infor.HWID = Func.ReadINI("Setting", "Final", "HWID", "xxxx");
                        infor.HWver = Func.ReadINI("Setting", "Final", "HWver", "xxxxx");
                        infor.BaseMAC = MACConvert(infor.BaseMAC);
                        infor.WanMAC = MACConvert(infor.BaseMAC, 1);
                        //infor.WiFi_PWD = Func.ReadINI("Setting", "Final", "WiFi_PWD", "xxxxxx");
                        //infor.Admin_PWD = Func.ReadINI("Setting", "Final", "Admin_PWD", "xxxxxx");
                        //infor.WiFi_SSID = Func.ReadINI("Setting", "Final", "WiFi_SSID", "xxxxxx");
                        //infor.BleMAC = Func.ReadINI("Setting", "Final", "BleMAC", "xxxxxx"); ;
                        //infor.DECT_cal_rxtun = Func.ReadINI("Setting", "Final", "DECT_cal_rxtun", "xxxxxx");

                        //infor.BLEver = Func.ReadINI("Setting", "PCBA", "BLEver", "v5.0.0-b108");
                        //infor.SEver = Func.ReadINI("Setting", "PCBA", "SEver", "0001020E");
                        //infor.DECT_rfpi = Func.ReadINI("Setting", "Final", "DECT_rfpi", "xxxxxx");
                        //infor.License_key = Func.ReadINI("Setting", "Final", "License_key", "xxxxxx");
                    }
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
                    SwitchRelay(CTRL.OFF);
                }
                else
                {
                    frmOK.Label = "Đảm bảo đã kết nối 'USB 3.0 flash drive và điện thoại SLIC', và 'dây mạng' đã được cắm vào cổng LAN màu vàng số 1, xin vui lòng bật nguồn và nhấn nút nguồn để khởi động";
                    frmOK.ShowDialog();
                }

                DisplayMsg(LogType.Log, "Power on!!!");

                if (!CheckGoNoGo()) { return; }
                ChkBootUp(PortType.SSH);

                // For EngMode
                // ===========
                // remove mt boarddata and do the cmd in CheckBoardData() function
                // ===========

                //-------------------------------------------------------------------------------------
                if (infor.SerialNumber.Length == 18)
                {
                    SetTextBox(status_ATS.txtPSN, infor.SerialNumber);
                    //SetTextBox(status_ATS.txtSP, infor.BaseMAC);
                    status_ATS.SFCS_Data.PSN = infor.SerialNumber;
                    status_ATS.SFCS_Data.First_Line = infor.SerialNumber;
                    status_ATS.SFCS_Data.PSN = infor.SerialNumber;
                }
                else
                {
                    warning = "Get SN from SFCS fail";
                    return;
                }

                if (Func.ReadINI("Setting", "Golden", "GoldenSN", "(*&^%$").Contains(status_ATS.txtPSN.Text))
                {
                    isGolden = true;
                    DisplayMsg(LogType.Log, "Golden testing..." + status_ATS.txtPSN.Text);
                }
                else { isGolden = false; }

                if (isLoop == 0)
                {
                    #region Ethenet Speed check
                    if (!CheckGoNoGo()) { return; }
                    if (Func.ReadINI("Setting", "Final", "SkipLANSPEED1", "0") == "0") { this.EthernetTest(1); }
                    if (Func.ReadINI("Setting", "Final", "SkipLANSPEED2", "0") == "0") { this.EthernetTest(2); }
                    if (Func.ReadINI("Setting", "Final", "SkipLANSPEED3", "0") == "0") { this.EthernetTest(3); }
                    if (Func.ReadINI("Setting", "Final", "SkipLANSPEED4", "0") == "0") { this.EthernetTest(4); }
                    if (Func.ReadINI("Setting", "Final", "SkipLANSPEED5", "0") == "0") { this.EthernetTest(5); }
                    #endregion Ethenet Speed check
                    //this.ChkMacAddr();
                }
                if (status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode && !isGolden)
                {
                    if (!CheckGoNoGo()) { return; }
                    CheckFWVerAndHWID();
                }
                //==========================================================================================================
                //==========================================================================================================
                //==========================================================================================================

                /*#region Check dect RX turn data
                string res = string.Empty;
                string DectRXturnSFCS = string.Empty;
                string dect_rf_calibration_rxtunDUT = string.Empty;

                if (!_sfcsQuery.Get15Data(status_ATS.txtPSN.Text, "LGR1_RXTURN", ref DectRXturnSFCS)) { DisplayMsg(LogType.Log, "Failed to get LGR1_RXTURN get from SFCS"); AddData("rxtun", 1); return; }

                DisplayMsg(LogType.Log, "SFCS LGR1_RXTURN: " + DectRXturnSFCS);

                if (DectRXturnSFCS.Length <= 0) { DisplayMsg(LogType.Log, "Failed to check leng LGR1_RXTURN get from SFCS"); AddData("rxtun", 1); return; }

                if (!SendAndChk(PortType.SSH, "verify_boarddata.sh", "root@OpenWrt:~#", out res, 0, 5000)) { AddData("rxtun", 1); return; }

                Match match = Regex.Match(res, @"dect_rf_calibration_rxtun=(\d+)");
                if (match.Success)
                {
                    dect_rf_calibration_rxtunDUT = match.Groups[1].Value;
                    DisplayMsg(LogType.Log, "dect_rf_calibration_rxtun: " + dect_rf_calibration_rxtunDUT);
                }
                else
                {
                    DisplayMsg(LogType.Log, "Failed to find dect_rf_calibration_rxtun");
                    AddData("rxtun", 1);
                    return;
                }

                if (dect_rf_calibration_rxtunDUT != DectRXturnSFCS)
                {
                    DisplayMsg(LogType.Log, $"Compare Dect RXturn in DUT: {dect_rf_calibration_rxtunDUT} with RXturn in SFCS: {DectRXturnSFCS} Fail ");
                    AddData("rxtun", 1);
                    return;
                }
                else
                {
                    DisplayMsg(LogType.Log, $"Compare Dect RXturn in DUT: {dect_rf_calibration_rxtunDUT} with RXturn in SFCS: {DectRXturnSFCS} Pass ");
                    AddData("rxtun", 0);
                }

                #endregion Check dect RX turn data*/

                CheckDECTValue(); //Rena_20230804, Check DECT RXTUN and RFPI
                CheckBoardData();

                if (isLoop == 0)
                {
                    NFCTag_withReaderTool();
                }

                if (isLoop == 0) //this.VerifyPA2_Comp();
                {
                    EthernetTest(3);
                }
                // --------------------------
                this.BatteryDetection();
                // --------------------------

                //CheckEthernetMAC();

                if (isLoop == 0)
                {
                    CheckLED();
                }

                if (!isGolden)
                {
                    CheckWiFiCalData(PortType.SSH);
                }

                CheckPCIe();

                if (isLoop == 0)
                    WPSButton();

                if (isLoop == 0)
                    ResetButton();

                //SLICTest_ByUsbModem();  //SLICTest();

                USBTest();
                //=================================
                //this.USB20(); // testplan 8.3.9
                //=================================

                CurrentSensor();

                //use tool to check calibration data
                if (!CheckGoNoGo())
                {
                    return;
                }
                //infor.BaseMAC = string.Empty;
                infor.BaseMAC = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@MAC");
                if (string.IsNullOrEmpty(infor.BaseMAC) || infor.BaseMAC.Contains("Dut not have")) { warning = "Get SFCS MAC Fail"; return; }
                //infor.BaseMAC = MACConvert(infor.BaseMAC);
                DisplayMsg(LogType.Log, $"Get Base MAC From SFCS is: {infor.BaseMAC}");
                MessageBox.Show("JASON need tweak the corresponding BaseMAC");
                var fResult = CH_RD.Check.FinalCheck(out res, new string[] { project, infor.BaseMAC });//JASON need chk this
                if (!fResult)
                {
                    DisplayMsg(LogType.Log, res);
                    AddData("CheckCalDataCH", 1);
                }
                else
                {
                    DisplayMsg(LogType.Log, res);
                    AddData("CheckCalDataCH", 0);
                }

                //Rena_20230720, remove /test/file
                if (CheckGoNoGo())
                {
                    // ============================================= ATH fw no test file 
                    //SendAndChk(PortType.SSH, "rm /test/file;sync", "root@OpenWrt:~# \r\n", 0, 5000);
                    // =============================================
                    SendAndChk(PortType.SSH, "umount /test", "root@OpenWrt:~# \r\n", 0, 5000);
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
        private void VerifyPA2_Comp()
        {
            SFCS_Query _sfcsQuery = new SFCS_Query();
            if (!CheckGoNoGo())
            {
                return;
            }

            DisplayMsg(LogType.Log, "=============== Verify PA2_COMP ===============");
            string item = "Verify_PA2_COMP";
            string res = string.Empty;
            string keyword = "root@OpenWrt:~# \r\n";
            string data_ = string.Empty;
            /*string DectRXturnSFCS = string.Empty;
            if (!_sfcsQuery.Get15Data(status_ATS.txtPSN.Text, "LGR1_RXTURN", ref DectRXturnSFCS)) { DisplayMsg(LogType.Log, "Failed to get LGR1_RXTURN get from SFCS"); AddData("rxtun", 1); return; }
            DisplayMsg(LogType.Log, "SFCS LGR1_RXTURN: " + DectRXturnSFCS);*/

            try
            {
                bool result = false;
                int delayMs = 0;
                int timeOutMs = 10 * 1000;

                for (int i = 0; i < 3; i++)
                {
                    SendAndChk(PortType.SSH, "cmbs_tcx -comname ttyMSM2 -baud 460800", "q => Quit", out res, delayMs, 5000);
                    if (res.Contains("q => Quit"))
                    {
                        break;
                    }
                    DisplayMsg(LogType.Log, "Delay 2s...");
                    Thread.Sleep(2000);
                }

                if (!result)
                {
                    DisplayMsg(LogType.Log, "Enter DECT MENU fail");
                    AddData(item, 1);
                    return;
                }
                // Service, system (s) →EEProm  Param Get (1) → Flex EEprom get (6)
                // s-> 1 -> 6
                SendWithoutEnterAndChk(PortType.SSH, "s", "q => Return to Interface Menu", out res, delayMs, timeOutMs);
                SendWithoutEnterAndChk(PortType.SSH, "1", "q => Return", out res, delayMs, timeOutMs);
                DisplayMsg(LogType.Log, "Write 6 to ssh");
                SSH_stream.Write("6\r");
                Thread.Sleep(500);
                DisplayMsg(LogType.Log, @"Will not show anything after 6\r");
                SSH_stream.WriteLine("178");
                SendWithoutEnterAndChk(PortType.SSH, "1", "Press Any Key!", out res, delayMs, timeOutMs);
                //=====================================================
                //               Judge Data whether = A0
                DisplayMsg(LogType.Log, @"Judge Data whether = A0");
                //=====================================================
                string[] lines = res.Split('\n');
                foreach (string line in lines)
                {
                    if (line.Contains("Data:"))
                    {
                        data_ = line.Split(':')[1].Trim();
                        DisplayMsg(LogType.Log, $"Get Data: {data_}");
                    }
                    AddData(item, data_ == "A0" ? 0 : 1);
                }
                //=====================================================
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
                    if (SendAndChk(PortType.SSH, "qqqqq", keyword, out res, 0, 2000))
                        break;
                }
            }
        }
        //Rena_20230804, Check DECT RXTUN and RFPI
        private void CheckDECTValue()
        {
            SFCS_Query _sfcsQuery = new SFCS_Query();
            if (!CheckGoNoGo())
            {
                return;
            }
            DisplayMsg(LogType.Log, "=============== Check DECT RXTUN and RFPI ===============");
            string item = "CheckDECTValue";
            string res = string.Empty;
            string RFPI_val = infor.DECT_rfpi.Replace(".", "").ToUpper(); //格式為 0303B009B0
            bool check_RXTUN = false;
            bool check_RFPI = false;

            /*string DectRXturnSFCS = string.Empty;
            if (!_sfcsQuery.Get15Data(status_ATS.txtPSN.Text, "LGR1_RXTURN", ref DectRXturnSFCS)) { DisplayMsg(LogType.Log, "Failed to get LGR1_RXTURN get from SFCS"); AddData("rxtun", 1); return; }
            DisplayMsg(LogType.Log, "SFCS LGR1_RXTURN: " + DectRXturnSFCS);*/

            try
            {
                bool result = false;
                int delayMs = 0;
                int timeOutMs = 10 * 1000;

                for (int i = 0; i < 3; i++)
                {
                    SendAndChk(PortType.SSH, "cmbs_tcx -comname ttyMSM2 -baud 460800", "menu", out res, delayMs, 5000);
                    if (res.Contains("q => Quit"))
                    {
                        result = true;
                        break;
                    }
                    DisplayMsg(LogType.Log, "Delay 3s...");
                    Thread.Sleep(3000);
                }

                if (!result)
                {
                    DisplayMsg(LogType.Log, "Enter DECT MENU fail");
                    AddData(item, 1);
                    return;
                }
                #region Verify DECT RXTUN and RFPI
                SendWithoutEnterAndChk(PortType.SSH, "x", "q) Quit", out res, delayMs, timeOutMs);

                //c) RXTUN:       70
                //d) RFPI:        0303B009C0
                string[] lines = res.Split('\n');
                foreach (string line in lines)
                {
                    if (line.Contains("RXTUN:") && line.Contains(infor.DECT_cal_rxtun))
                    {
                        check_RXTUN = true;
                        DisplayMsg(LogType.Log, $"Check 'RXTUN:{infor.DECT_cal_rxtun}'SFCS pass");
                    }
                    if (line.Contains("RFPI:") && line.Contains(RFPI_val))
                    {
                        check_RFPI = true;
                        DisplayMsg(LogType.Log, $"Check 'RFPI:{RFPI_val}'SFCS pass");
                    }
                }

                if (check_RXTUN && check_RFPI)
                {
                    AddData(item, 0);
                }
                else
                {
                    if (!check_RXTUN)
                        DisplayMsg(LogType.Log, $"Check 'RXTUN:{infor.DECT_cal_rxtun}' fail");
                    if (!check_RXTUN)
                        DisplayMsg(LogType.Log, $"Check 'RFPI:{RFPI_val}' fail");
                    AddData(item, 1);
                }
                #endregion
                for (int i = 0; i < 5; i++)
                {//back to main menu
                    if (SendAndChk(PortType.SSH, "q", "Choose Option", out res, 100, 3500))
                    {
                        break;
                    }
                }
                #region Verify PA2_COMP
                DisplayMsg(LogType.Log, @"====== Verify PA2_COMP =======");
                SendWithoutEnterAndChk(item, PortType.SSH, "s", "q => Return to Interface Menu", delayMs, timeOutMs);
                result = SendWithoutEnterAndChk(PortType.SSH, "1", "q => Return", out res, delayMs, timeOutMs);
                do
                {
                    DisplayMsg(LogType.Log, "Write 6 to ssh");
                    SendWithoutEnterAndChk(PortType.SSH, "6\r\n", "Press", out res, delayMs, 3000);
                    DisplayMsg(LogType.Log, @"Will not show anything after 6\r");
                    if (res.Contains("Press"))
                    {
                        break;
                    }
                } while (!res.Contains("Press Any Key"));
                SSH_stream.Write("r");
                SSH_stream.WriteLine("178");
                SendWithoutEnterAndChk(PortType.SSH, "1", "Press Any Key!", out res, delayMs, timeOutMs);
                //=====================================================
                //               Judge Data whether = A0
                DisplayMsg(LogType.Log, @"------ Judge Data whether = A0 -------");
                if (!res.Contains("A0"))
                {
                    DisplayMsg(LogType.Log, "Verify PA2_COMP Fail");
                    AddData(item, 1);
                }
                if (!res.Contains("Enter Location 178"))
                {
                    DisplayMsg(LogType.Log, "Verify PA2_COMP Fail");
                    AddData(item, 1);
                }
                if (!res.Contains("Enter Length 1"))
                {
                    DisplayMsg(LogType.Log, "Verify PA2_COMP Fail");
                    AddData(item, 1);
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
                //exit calibration mode
                SendCommand(PortType.SSH, "\r\n", 1000);
                //SendWithoutEnterAndChk(item, PortType.SSH, "\n", "Press", delayMs, 1000);
                //SendAndChk(PortType.SSH, "qqqqq", keyword, out res, 0, 5000);
                for (int i = 0; i < 5; i++)
                {
                    if (SendAndChk(PortType.SSH, "qqqq", "root@OpenWrt:~#", out res, 0, 3600))
                        break;
                }
            }
        }
        public static string GetMatchedValue(string input, string pattern)
        {
            Match match = Regex.Match(input, pattern);
            if (match.Success && match.Groups.Count > 1)
            {
                return match.Groups[1].Value;
            }
            return string.Empty;
        }
        private void CheckBoardData()
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

            try
            {
                //verify partition data
                SendAndChk(PortType.SSH, "mkdir /mnt/test", keyword, out res, 0, 5000);
                SendAndChk(PortType.SSH, "mount -t ext4 /dev/mmcblk0p32 /mnt/test", keyword, out res, 0, 5000);
                SendAndChk(PortType.SSH, "mount | grep /dev/mmcblk0p32", keyword, out res, 0, 5000);
                if (!res.Contains($"rw,relatime"))
                {
                    DisplayMsg(LogType.Log, "Check hw_ver fail");
                    AddData(item, 1);
                }
                SendAndChk(PortType.SSH, "mt info", keyword, out res, 0, 5000);
                // =============================================================================
                SendAndChk(PortType.SSH, "cat /sys/devices/system/qfprom/qfprom0/authenticate", keyword, out res, 0, 3000);
                if (!res.Contains("1"))
                {
                    DisplayMsg(LogType.Log, @"Secure boot not enable");
                    AddData(item, 1);
                    return;
                }
                // =============================================================================
                // /dev/mmcblk0p32 on /mnt/test type ext4 (rw,relatime)
                this.secureBootDownloadfilesRequired();
                this.DownloadFilesRequired();
                this.FilesystemEncryption(false);
                // ========================================================================

                //check Board data
                SendAndChk(PortType.SSH, "mt boarddata", keyword, out res, 0, 5000);
                //serial_number = +119746 + 2333000129
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
                DisplayMsg(LogType.Log, $"mac_base get SFCS: {infor.BaseMAC}");
                string formattedBaseMAC = string.Empty;
                // =============================== 8.3.9 D2 License ==================================
                SendAndChk(PortType.SSH, "/etc/init.d/vtspd start", keyword, out res, 0, 5000);
                SendAndChk(PortType.SSH, "ps | grep ve_vtsp_main", keyword, out res, 0, 5000);
                // ===================================================================================
                //formattedBaseMAC = InsertColon(infor.BaseMAC);
                //DisplayMsg(LogType.Log, $"mac_base Convert: {formattedBaseMAC}");
                //if (formattedBaseMAC.Length != 17 || formattedBaseMAC.Length != infor.WanMAC.Length) { DisplayMsg(LogType.Log, "Leng mac fail"); return; }
                //if (!res.Contains($"mac_base={formattedBaseMAC.ToUpper()}"))
                //{
                //    DisplayMsg(LogType.Log, "Check mac_base fail");
                //    AddData(item, 1);
                //}
                //else
                //{
                //    DisplayMsg(LogType.Log, $"DUT mac_base: {formattedBaseMAC}");
                //    DisplayMsg(LogType.Log, "Check mac_base PASS");
                //}

                //DisplayMsg(LogType.Log, $"SN get SFCS: {infor.SerialNumber}");
                //if (!res.Contains($"serial_number={infor.SerialNumber}"))
                //{
                //    DisplayMsg(LogType.Log, "Check SN fail");
                //    AddData(item, 1);
                //}
                //else
                //{
                //    DisplayMsg(LogType.Log, $"DUT SN: {infor.SerialNumber}");
                //    DisplayMsg(LogType.Log, "Check SN PASS");
                //}

                ////dect_identity_rfpi=03.6C.D3.A9.38
                //DisplayMsg(LogType.Log, $"DECT RFPI get SFCS: {infor.DECT_rfpi}");
                //if (!res.Contains($"dect_identity_rfpi={infor.DECT_rfpi.ToUpper()}"))
                //{
                //    DisplayMsg(LogType.Log, "Check dect_identity_rfpi fail");
                //    AddData(item, 1);
                //}
                //else
                //{
                //    DisplayMsg(LogType.Log, $"DUT DECT RFPI: {infor.DECT_rfpi}");
                //    DisplayMsg(LogType.Log, "Check DECT RFPI PASS");
                //}

                ////dect_rf_calibration_rxtun=77
                //DisplayMsg(LogType.Log, $"DECT RXturn get SFCS: {infor.DECT_cal_rxtun}");
                //if (!res.Contains($"dect_rf_calibration_rxtun={infor.DECT_cal_rxtun}"))
                //{
                //    DisplayMsg(LogType.Log, "Check dect_rf_calibration_rxtun fail");
                //    AddData(item, 1);
                //}
                //else
                //{
                //    DisplayMsg(LogType.Log, $"DUT DECT RXturn: {infor.DECT_cal_rxtun}");
                //    DisplayMsg(LogType.Log, "Check DECT RXturn PASS");
                //}

                ////wifi_password=hgzEYyxeu7UFTdfr
                //DisplayMsg(LogType.Log, $"Wifi_password get SFCS: {infor.WiFi_PWD}");
                //if (!res.Contains($"wifi_password={infor.WiFi_PWD}"))
                //{
                //    DisplayMsg(LogType.Log, "Check wifi_password fail");
                //    AddData(item, 1);
                //}
                //else
                //{
                //    DisplayMsg(LogType.Log, $"DUT wifi_password: {infor.WiFi_PWD}");
                //    DisplayMsg(LogType.Log, "Check wifi_password PASS");
                //}

                ////admin_password=citerxfg
                //DisplayMsg(LogType.Log, $"admin_password get SFCS: {infor.Admin_PWD}");
                //if (!res.Contains($"admin_password={infor.Admin_PWD}"))
                //{
                //    DisplayMsg(LogType.Log, "Check admin_password fail");
                //    AddData(item, 1);
                //}
                //else
                //{
                //    DisplayMsg(LogType.Log, $"DUT admin_password: {infor.Admin_PWD}");
                //    DisplayMsg(LogType.Log, "Check admin_password PASS");
                //}

                ////wlan_ssid=BT-F5C26X
                //DisplayMsg(LogType.Log, $"wlan_ssid get SFCS: {infor.WiFi_SSID}");
                //if (!res.Contains($"wlan_ssid={infor.WiFi_SSID}"))
                //{
                //    DisplayMsg(LogType.Log, "Check wlan_ssid fail");
                //    AddData(item, 1);
                //}
                //else
                //{
                //    DisplayMsg(LogType.Log, $"DUT wlan_ssid: {infor.WiFi_SSID}");
                //    DisplayMsg(LogType.Log, "Check wlan_ssid PASS");
                //}

                //DisplayMsg(LogType.Log, $"D2License get SFCS: {infor.License_key}");
                //if (!SendAndChk(PortType.SSH, "cat /defaults/D2License.key", infor.License_key, out res, 0, 5000))
                //{
                //    DisplayMsg(LogType.Log, "Check D2 License Key fail");
                //    AddData(item, 1);
                //}
                //else
                //{
                //    DisplayMsg(LogType.Log, $"DUT D2License: {infor.License_key}");
                //    DisplayMsg(LogType.Log, "Check D2License PASS");
                //}

                //if (CheckGoNoGo())
                //{
                //    AddData(item, 0);
                //}
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                AddData(item, 1);
            }
        }

        private bool RF_enable_all_wifi(PortType portType)
        {
            if (!CheckGoNoGo())
            {
                return false;
            }
            //bool retry = false;
            //retry:
            try
            {
                DisplayMsg(LogType.Log, $"========= WiFi PreSetting  =========");

                int delayMs = 0;
                int timeOutMs = 30 * 1000;
                string keyword = "root@OpenWrt";
                string res = string.Empty;

                DisplayMsg(LogType.Log, @"----------------------------------------------");
                SendAndChk(portType, "uci set wireless.wifi0.disabled='0'", keyword, out res, 0, 10 * 1000);
                SendAndChk(portType, "uci set wireless.wifi1.disabled='0'", keyword, out res, 0, 10 * 1000);
                SendAndChk(portType, "uci set wireless.wifi2.disabled='0'", keyword, out res, 0, 10 * 1000);
                SendAndChk(portType, "uci commit", keyword, out res, 0, 10 * 1000);
                SendAndChk(portType, "wifi", "keyword", out res, 0, 40 * 1000);
                SendAndChk(portType, "sleep 10", keyword, delayMs, timeOutMs); //TODO:?
                Thread.Sleep(10 * 1000);
                DisplayMsg(LogType.Log, @"-------------    Enable WiFi Done   -----------");
                return true;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                warning = "Enable WIFI Fail";
                return false;
            }
        }


        private void CheckWiFiCalDataRFStation(PortType portType)
        {
            if (!CheckGoNoGo())
            {
                return;
            }
            string item = "CheckRF_WiFiCalData";
            string keyword = "";
            string res = "";
            string wifi_data_backup_path = Path.Combine(Application.StartupPath, "WiFiCalData_backup");
            string PC_IP = Func.ReadINI("Setting", "IP", "PC", "192.168.1.2");
            var regex = "(.{2}):(.{2}):(.{2}):(.{2}):(.{2}):(.{2})";

            try
            {
                if (portType == PortType.UART)
                    keyword = "root@OpenWrt:/# \r\n"; //避免誤判到指令第一行的"root@OpenWrt:/#"
                else
                    keyword = "root@OpenWrt:~# \r\n"; //避免誤判到指令第一行的"root@OpenWrt:~#"
                DisplayMsg(LogType.Log, "=============== Check WiFi Cal Data ===============");
                DisplayMsg(LogType.Log, "BaseMAC: " + infor.BaseMAC);
                //==============================================================
                //================ Audrey remove by testplan 6.4 ===============
                //OpenTftpd32New(wifi_data_backup_path, 15 * 1000);
                ////Backup wifi data
                //SendAndChk(portType, "cat /dev/mmcblk0p21 > /tmp/backupwifi", keyword, out res, 0, 5000);
                //if (res.Contains("No such file or directory"))
                //{
                //    DisplayMsg(LogType.Log, "Backup wifi data fail");
                //    AddData(item, 1);
                //    return;
                //}
                //==============================================================

                //check WiFi 2.4G MAC = BaseMAC+4
                string wifi_2g_mac = MACConvert(infor.BaseMAC, 4);
                DisplayMsg(LogType.Log, "WiFi_2G_MAC: " + wifi_2g_mac);
                string wifi_2g_mac_ = Regex.Replace(wifi_2g_mac, regex, "$2$1 $4$3 $6$5").ToLower();
                SendAndChk(portType, "hexdump -s 0x58810 -n 6 /dev/mmcblk0p21", keyword, out res, 0, 5000);
                if (!res.Contains(wifi_2g_mac_))
                {
                    DisplayMsg(LogType.Log, "Check WiFi 2.4G MAC fail");
                    AddData(item, 1);
                    return;
                }

                //check WiFi 5G MAC = BaseMAC+3
                string wifi_5g_mac = MACConvert(infor.BaseMAC, 3);
                DisplayMsg(LogType.Log, "WiFi_5G_MAC: " + wifi_5g_mac);
                //this.WiFiPreSetting(WiFiType.WiFi_5G);
                string wifi_5g_mac_ = Regex.Replace(wifi_5g_mac, regex, "$2$1 $4$3 $6$5").ToLower();
                SendAndChk(portType, "hexdump -s 0xbc810 -n 6 /dev/mmcblk0p21", keyword, out res, 0, 5000);
                if (!res.Contains(wifi_5g_mac_))
                {
                    DisplayMsg(LogType.Log, "Check WiFi 5G MAC fail");
                    AddData(item, 1);
                    return;
                }

                //check WiFi 6G MAC = BaseMAC+2
                string wifi_6g_mac = MACConvert(infor.BaseMAC, 2);
                DisplayMsg(LogType.Log, "WiFi_6G_MAC: " + wifi_6g_mac);
                //this.WiFiPreSetting(WiFiType.WiFi_6G);
                string wifi_6g_mac_ = Regex.Replace(wifi_6g_mac, regex, "$2$1 $4$3 $6$5").ToLower();
                SendAndChk(portType, "hexdump -s 0x8a810 -n 6 /dev/mmcblk0p21", keyword, out res, 0, 5000);
                if (!res.Contains(wifi_6g_mac_))
                {
                    DisplayMsg(LogType.Log, "Check WiFi 6G MAC fail");
                    AddData(item, 1);
                    return;
                }
                //==============================================================
                //================ Audrey remove by testplan 6.4 ===============
                //Backup wifi data
                //if (Directory.Exists(wifi_data_backup_path))
                //{
                //    File.Delete(Path.Combine(wifi_data_backup_path, "backupwifi"));
                //    File.Delete(Path.Combine(wifi_data_backup_path, $"backupwifi_{infor.SerialNumber}"));
                //    SendAndChk(portType, "cd /tmp", "root@OpenWrt:/tmp#", out res, 0, 3000);
                //    SendAndChk(portType, $"tftp -p -l backupwifi {PC_IP}", "root@OpenWrt:/tmp# \r\n", out res, 0, 5000);
                //    if (File.Exists(Path.Combine(wifi_data_backup_path, "backupwifi")))
                //    {
                //        File.Move(Path.Combine(wifi_data_backup_path, "backupwifi"), Path.Combine(wifi_data_backup_path, $"backupwifi_{infor.SerialNumber}"));
                //        DisplayMsg(LogType.Log, $"Backup wifi data in {Path.Combine(wifi_data_backup_path, $"backupwifi_{infor.SerialNumber}")}");
                //    }
                //}
                //==============================================================


                AddData(item, 0);
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                AddData(item, 1);
            }
            finally
            {
                DisplayMsg(LogType.Log, @"Trying to move working directory to root@OpenWrt:~#");
                SendAndChk(portType, "cd ~", keyword, out res, 0, 3000);
            }
        }

        private void CheckWiFiCalData(PortType portType)
        {
            if (!CheckGoNoGo())
            {
                return;
            }
            string item = "CheckWiFiCalData";
            string keyword = "";
            string res = "";
            string wifiMAC = string.Empty;
            string wifi_data_backup_path = Path.Combine(Application.StartupPath, "WiFiCalData_backup");
            string PC_IP = Func.ReadINI("Setting", "IP", "PC", "192.168.1.2");
            //var regex = "(.{2}):(.{2}):(.{2}):(.{2}):(.{2}):(.{2})";
            string wifi_2g_mac = string.Empty;
            string wifi_5g_mac = string.Empty;
            string wifi_6g_mac = string.Empty;
            string wifi_2g_md5 = string.Empty;
            string wifi_5g_md5 = string.Empty;
            string wifi_6g_md5 = string.Empty;

            try
            {
                if (portType == PortType.UART)
                    keyword = "root@OpenWrt:/# \r\n"; //避免誤判到指令第一行的"root@OpenWrt:/#"
                else
                    keyword = "root@OpenWrt:~# \r\n"; //避免誤判到指令第一行的"root@OpenWrt:~#"
                //MessageBox.Show(keyword);
                DisplayMsg(LogType.Log, "=============== Check WiFi Cal Data ===============");
                DisplayMsg(LogType.Log, "BaseMAC: " + infor.BaseMAC);

                OpenTftpd32New(wifi_data_backup_path, 15 * 1000);
                //Backup wifi data
                SendAndChk(portType, "cat /dev/mmcblk0p21 > /tmp/backupwifi", keyword, out res, 0, 5000);
                if (res.Contains("No such file or directory"))
                {
                    DisplayMsg(LogType.Log, "Backup wifi data fail");
                    AddData(item, 1);
                    return;
                }
                //==================== 8.3.5.	Check Wifi Cal Data ==============================
                Thread.Sleep(5 * 1000);
                DisplayMsg(LogType.Log, "Delay 5s"); //at least 5s
                SendAndChk(PortType.SSH, "checkWiFiCal.sh", keyword, out res, 1000, 1000);
                string[] lines = res.Split('\n');
                // Record 3 sets of MD5 to SFCS and check the MAC address
                foreach (string line in lines)
                {
                    if (line.StartsWith("WiFi_2G_MAC"))
                    {
                        wifi_2g_mac = line.Split(' ')[1].Trim();
                    }
                    if (line.StartsWith("WiFi_5G_MAC"))
                    {
                        wifi_5g_mac = line.Split(' ')[1].Trim();
                    }
                    if (line.StartsWith("WiFi_6G_MAC"))
                    {
                        wifi_6g_mac = line.Split(' ')[1].Trim();
                    }
                    if (line.StartsWith("WiFi_2G_MD5"))
                    {
                        wifi_2g_md5 = line.Split(' ')[1].Trim();
                    }
                    if (line.StartsWith("WiFi_5G_MD5"))
                    {
                        wifi_5g_md5 = line.Split(' ')[1].Trim();
                    }
                    if (line.StartsWith("WiFi_6G_MD5"))
                    {
                        wifi_6g_md5 = line.Split(' ')[1].Trim();
                    }
                }

                DisplayMsg(LogType.Log, $"WiFi 2G MAC: '{wifi_2g_mac}'");
                DisplayMsg(LogType.Log, $"WiFi 5G MAC: '{wifi_5g_mac}'");
                DisplayMsg(LogType.Log, $"WiFi 6G MAC: '{wifi_6g_mac}'");

                DisplayMsg(LogType.Log, $"WiFi 2G MD5: '{wifi_2g_md5}'");
                DisplayMsg(LogType.Log, $"WiFi 5G MD5: '{wifi_5g_md5}'");
                DisplayMsg(LogType.Log, $"WiFi 6G MD5: '{wifi_6g_md5}'");

                if (string.IsNullOrEmpty(wifi_2g_mac) || string.IsNullOrEmpty(wifi_5g_mac) || string.IsNullOrEmpty(wifi_6g_mac) || string.IsNullOrEmpty(wifi_2g_md5)
                    || string.IsNullOrEmpty(wifi_5g_md5) || string.IsNullOrEmpty(wifi_6g_md5))
                {
                    AddData("GetDUTdataFail", 1);
                    return;
                }

                DisplayMsg(LogType.Log, $"WiFi 2G DUT MAC: '{wifi_2g_mac.ToUpper()}'");
                DisplayMsg(LogType.Log, $"SFCS 2G DUT MAC: '{MACConvert(infor.BaseMAC, 4)}'");
                if (wifi_2g_mac.ToUpper() != MACConvert(infor.BaseMAC, 4))
                {
                    AddData("WiFi_2G_MAC", 1);
                    return;
                }
                else { DisplayMsg(LogType.Log, $"Check WiFi 2G MAC DUT With SFCS  PASS"); }
                DisplayMsg(LogType.Log, $"WiFi 5G DUT MAC: '{wifi_5g_mac.ToUpper()}'");
                DisplayMsg(LogType.Log, $"SFCS 5G DUT MAC: '{MACConvert(infor.BaseMAC, 3)}'");
                if (wifi_5g_mac.ToUpper() != MACConvert(infor.BaseMAC, 3))
                {
                    AddData("WiFi_5G_MAC", 1);
                    return;
                }
                else { DisplayMsg(LogType.Log, $"Check WiFi 5G MAC DUT With SFCS PASS"); }
                DisplayMsg(LogType.Log, $"WiFi 6G DUT MAC: '{wifi_6g_mac.ToUpper()}'");
                DisplayMsg(LogType.Log, $"SFCS 6G DUT MAC: '{MACConvert(infor.BaseMAC, 2)}'");
                if (wifi_6g_mac.ToUpper() != MACConvert(infor.BaseMAC, 2))
                {
                    AddData("WiFi_6G_MAC", 1);
                    return;
                }
                else { DisplayMsg(LogType.Log, $"Check WiFi 6G MAC DUT With SFCS PASS"); }

                status_ATS.AddDataRaw("LRG1_Fl_WF2G_M", wifi_2g_mac.Replace(":", "").ToUpper(), wifi_2g_mac.Replace(":", "").ToUpper(), "000000");
                status_ATS.AddDataRaw("LRG1_Fl_WF5G_M", wifi_5g_mac.Replace(":", "").ToUpper(), wifi_5g_mac.Replace(":", "").ToUpper(), "000000");
                status_ATS.AddDataRaw("LRG1_Fl_WF6G_M", wifi_6g_mac.Replace(":", "").ToUpper(), wifi_6g_mac.Replace(":", "").ToUpper(), "000000");

                status_ATS.AddDataRaw("LRG1_WIFI2G_MD5", wifi_2g_md5.ToUpper(), wifi_2g_md5.ToUpper(), "000000");
                status_ATS.AddDataRaw("LRG1_WIFI5G_MD5", wifi_5g_md5.ToUpper(), wifi_5g_md5.ToUpper(), "000000");
                status_ATS.AddDataRaw("LRG1_WIFI6G_MD5", wifi_6g_md5.ToUpper(), wifi_6g_md5.ToUpper(), "000000");
                //=========================================================================
                //=================== ATH FW test plan remove =============================
                //=========================================================================
                //check WiFi 2.4G MAC = BaseMAC + 4
                //DisplayMsg(LogType.Log, "WiFi_2G_MAC: " + wifi_2g_mac);
                //string wifi_2g_mac_ = Regex.Replace(wifi_2g_mac, regex, "$2$1 $4$3 $6$5").ToLower();
                //SendAndChk(portType, "hexdump -s 0x58810 -n 6 /dev/mmcblk0p21", keyword, out res, 0, 5000);
                //if (!res.Contains(wifi_2g_mac_))
                //{
                //    DisplayMsg(LogType.Log, $"Check WiFi 2.4G MAC '{wifi_2g_mac_}' fail");
                //    AddData(item, 1);
                //    return;
                //}
                //DisplayMsg(LogType.Log, $"Check WiFi 2.4G MAC '{wifi_2g_mac_}' PASS");
                ////check WiFi 5G MAC = BaseMAC+3
                //DisplayMsg(LogType.Log, "WiFi_5G_MAC: " + wifi_5g_mac);
                //string wifi_5g_mac_ = Regex.Replace(wifi_5g_mac, regex, "$2$1 $4$3 $6$5").ToLower();
                //SendAndChk(portType, "hexdump -s 0xbc810 -n 6 /dev/mmcblk0p21", keyword, out res, 0, 5000);
                //if (!res.Contains(wifi_5g_mac_))
                //{
                //    DisplayMsg(LogType.Log, $"Check WiFi 5G MAC '{wifi_5g_mac_}' fail");
                //    AddData(item, 1);
                //    return;
                //}
                //DisplayMsg(LogType.Log, $"Check WiFi 5G MAC '{wifi_5g_mac_}' PASS");
                ////check WiFi 6G MAC = BaseMAC+2
                //DisplayMsg(LogType.Log, "WiFi_6G_MAC: " + wifi_6g_mac);
                //string wifi_6g_mac_ = Regex.Replace(wifi_6g_mac, regex, "$2$1 $4$3 $6$5").ToLower();
                //SendAndChk(portType, "hexdump -s 0x8a810 -n 6 /dev/mmcblk0p21", keyword, out res, 0, 5000);
                //if (!res.Contains(wifi_6g_mac_))
                //{
                //    DisplayMsg(LogType.Log, $"Check WiFi 6G MAC '{wifi_6g_mac_}' fail");
                //    AddData(item, 1);
                //    return;
                //}
                //DisplayMsg(LogType.Log, $"Check WiFi 6G MAC '{wifi_6g_mac_}' PASS");
                //=========================================================================
                //=========================================================================

                // ============================ ifconfig ath0, ath1, ath2 =============================== 
                // Jason remove follow SW RD Reeves Hsieh(謝金樺) <Reeves.Hsieh@wnc.com.tw> because SW issue cannot show corret mac 2023/10/05
                // Audrey revised testplan
                //Thread.Sleep(3 * 1000);
                //SendAndChk(portType, "\r\n", "#", out res, 0, 5000);
                //SendAndChk(portType, "iw dev | grep addr", "addr", out res, 0, 15 * 1000);
                //string[] athlines = res.Split('\n');
                //foreach (string line in athlines)
                //{
                //    if (line.StartsWith("\t\taddr"))
                //    {
                //        wifiMAC += line.TrimStart('\t').TrimStart('\t');
                //        wifiMAC += "\n";
                //    }
                //}
                //DisplayMsg(LogType.Log, "All wifiMAC \r\n " + wifiMAC);
                //if (!wifiMAC.Contains(wifi_2g_mac))
                //{
                //    DisplayMsg(LogType.Log, $"Check WiFi 2.4G MAC '{wifi_2g_mac}' fail");
                //    AddData(item, 1);
                //    return;
                //}
                //DisplayMsg(LogType.Log, $"Check WiFi 2.4G MAC '{wifi_2g_mac}' PASS");
                //if (!wifiMAC.Contains(wifi_5g_mac))
                //{
                //    DisplayMsg(LogType.Log, $"Check WiFi 5G MAC '{wifi_5g_mac}' fail");
                //    AddData(item, 1);
                //    return;
                //}
                //DisplayMsg(LogType.Log, $"Check WiFi 5G MAC '{wifi_5g_mac}' PASS");
                //if (!wifiMAC.Contains(wifi_6g_mac))
                //{
                //    DisplayMsg(LogType.Log, $"Check WiFi 6G MAC '{wifi_6g_mac}' fail");
                //    AddData(item, 1);
                //    return;
                //}
                //DisplayMsg(LogType.Log, $"Check WiFi 6G MAC '{wifi_6g_mac}' PASS");
                // =====================================================================================



                //Backup wifi data
                if (Directory.Exists(wifi_data_backup_path))
                {
                    File.Delete(Path.Combine(wifi_data_backup_path, "backupwifi"));
                    File.Delete(Path.Combine(wifi_data_backup_path, $"backupwifi_{infor.SerialNumber}"));
                    SendAndChk(portType, "cd /tmp", "root@OpenWrt:/tmp#", out res, 0, 3000);
                    SendAndChk(portType, $"tftp -p -l backupwifi {PC_IP}", "root@OpenWrt:/tmp# \r\n", out res, 0, 5000);
                    if (File.Exists(Path.Combine(wifi_data_backup_path, "backupwifi")))
                    {
                        File.Move(Path.Combine(wifi_data_backup_path, "backupwifi"), Path.Combine(wifi_data_backup_path, $"backupwifi_{infor.SerialNumber}"));
                        DisplayMsg(LogType.Log, $"Backup wifi data in {Path.Combine(wifi_data_backup_path, $"backupwifi_{infor.SerialNumber}")} PASS");
                    }
                    else
                    {
                        DisplayMsg(LogType.Log, $"Backup wifi data in {Path.Combine(wifi_data_backup_path, $"backupwifi_{infor.SerialNumber}")} FAIL");
                        AddData(item, 1); return;
                    }
                }
                else { DisplayMsg(LogType.Log, $"Backup wifi data path '{wifi_data_backup_path}' not exits"); warning = "Backup Dir not exits"; return; }

                AddData(item, 0);
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                AddData(item, 1);
            }
            finally
            {
                DisplayMsg(LogType.Log, @"Trying to move working directory to root@OpenWrt:~#");
                SendAndChk(portType, "cd ~", keyword, out res, 0, 3000);
            }
        }
        private void CheckBleMac()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            string item = "BleTest";
            string keyword = @"root@OpenWrt";
            string res = "";

            try
            {
                DisplayMsg(LogType.Log, "=============== BLE Test ===============");
                DisplayMsg(LogType.Log, $"SFCS_Ble_MAC: {infor.BleMAC}");
                if (string.IsNullOrEmpty(infor.BleMAC))
                {
                    DisplayMsg(LogType.Log, "SFCS BLE MAC is null");
                    AddData(item, 1);
                    return;
                }

                SendAndChk(PortType.SSH, "bt_host_empty -u /dev/ttyMSM1", "Started advertising", out res, 0, 10000);
                if (res.Contains($"Bluetooth public device address: {infor.BleMAC}"))
                {
                    DisplayMsg(LogType.Log, "Check BLE MAC pass");
                    AddData(item, 0);
                }
                else
                {
                    DisplayMsg(LogType.Log, "Check BLE MAC fail");
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
                //send ctrl+c
                SendCommand(PortType.SSH, sCtrlC, 500);
                ChkResponse(PortType.SSH, ITEM.NONE, keyword, out res, 3000);
            }
        }
        private void NFCTag_withReaderTool()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            int retry = 3;
            string item = "NFC";
            string res = "";
            string NFC_UID_Reader = "";
            string NFC_UID_DUT = "";
            string NFC_Tool = Func.ReadINI("Setting", "Final", "NFC_Tool", "nfc_read_tool_v0_2");
            string NFC_Tool_Path = Path.Combine(Application.StartupPath, NFC_Tool);
            string keyword = "root@OpenWrt:~# \r\n";
            CommandConsole myCmd = null;

            try
            {
                DisplayMsg(LogType.Log, "=============== NFC ===============");

                frmOK.Label = "Đặt NFC vào sản phầm rồi nhấn ok để test";
                frmOK.ShowDialog();

                //read UID with ACR122U NFC reader
                myCmd = new CommandConsole();
                myCmd.Start();
                myCmd.WriteLine(Path.GetPathRoot(NFC_Tool_Path).TrimEnd('\\'));
                myCmd.WriteLine("cd " + NFC_Tool_Path);

            check_UID:
                SendCmdAndGetResp(myCmd, "nfc_read.exe ff byte 0 1", "Error", "Result:", out res, 5000, 0);
                if (res.Contains("Result:"))
                {
                    //04-78-1B-5A-88-12-90-00-44-00-00-00-00-00-00-00 前面7個byte是UID
                    Match m = Regex.Match(res, @"(?<uid>\w{2}-\w{2}-\w{2}-\w{2}-\w{2}-\w{2}-\w{2})[\w\-]+");
                    if (m.Success)
                    {
                        NFC_UID_Reader = m.Groups["uid"].Value.Trim();
                    }
                }

                DisplayMsg(LogType.Log, "NFC_UID_Reader: " + NFC_UID_Reader);

                if (NFC_UID_Reader == "")
                {
                    DisplayMsg(LogType.Log, "Check NFC UID fail");
                    //如果沒讀到UID, nfc_read.exe不會跳出,要先送Ctrl+C或Enter
                    myCmd.WriteLine("");
                    myCmd.WriteLine("");
                    if (retry-- > 0)
                    {
                        DisplayMsg(LogType.Log, "NFC UID retry...");
                        goto check_UID;
                    }
                    AddData(item, 1);
                    return;
                }

                //0x04 0x33 0x88 0xe2 0xed 0x10 0x90 0x00 0x44 0x00 0x00 0x00 0x00 0x00 0x00 0x00
                SendAndChk(PortType.SSH, "i2ctransfer -y 0 w1@0x55 0x0 r16", keyword, out res, 0, 3000);

                string[] lines = res.Split('\n');
                foreach (string line in lines)
                {
                    if (line.StartsWith("0x"))
                    {
                        string[] vals = line.Split(' ');
                        NFC_UID_DUT = string.Join("-", vals, 0, 7).Replace("0x", "").ToUpper();
                        DisplayMsg(LogType.Log, "NFC_UID_DUT: " + NFC_UID_DUT);
                        break;
                    }
                }

                if (string.Compare(NFC_UID_Reader, NFC_UID_DUT) != 0)
                {
                    DisplayMsg(LogType.Log, "Check UID from Reader & DUT fail");
                    AddData(item, 1);
                    return;
                }

                //Check NFC field detection pin
                SendAndChk(PortType.SSH, "mt gpio dump nfc", "root@OpenWrt:~# \r\n", out res, 0, 3000);
                if (res.Contains("NFC: low"))
                {
                    DisplayMsg(LogType.Log, "Check NFC field detection pin - low pass");
                    AddData(item, 0);
                    status_ATS.AddDataRaw("LRG1_NFC_UID", NFC_UID_Reader, NFC_UID_Reader, "000000");
                }
                else
                {
                    DisplayMsg(LogType.Log, "Check NFC field detection pin - low fail");
                    AddData(item, 1);
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                AddData(item, 1);
            }
            finally
            {
                if (myCmd != null)
                    myCmd.Close();
            }
        }

        private void ScanByCamera(ref List<string> result)
        {
            if (Func.ReadINI("Setting", "Camera", "Barcode", "0") == "0")
                return;
            int codeNumber = Convert.ToInt32(Func.ReadINI("Setting", "Camera", "BarcodeNumber", "5"));
            DisplayMsg(LogType.Log, "Setting code number:" + codeNumber);
            status_ATS.AddLog("Start to scan barcode...");
            bool getmac = false;
            string NewCameraPath = Func.ReadINI("Setting", "Camera", "BarcodePath", "D:/Barcode");
            for (int i = 0; i < 60; i++)
            {
                result.Clear();
                if (File.Exists(NewCameraPath + "/cam_result.ini"))
                    File.Delete(NewCameraPath + "/cam_result.ini");

                if (File.Exists("c:/ok")) File.Delete("c:/ok");
                File.Create("c:\\getSMT").Close();
                while (!File.Exists("c:\\OK")) Thread.Sleep(10);
                Thread.Sleep(800);
                //temp[0] = Func.ReadINI(NewCameraPath, "cam_result", "result", "item_1", "").Trim();
                string[] file = File.ReadAllLines(Path.Combine(NewCameraPath, "cam_result.ini"));
                for (int j = 0; j < file.Length; j++)
                {
                    DisplayMsg(LogType.Log, "Scan:" + file[j]);
                    if (file[j].Contains("item_"))
                    {
                        Match m = Regex.Match(file[j], @"item_\d+=(?<value>.*)");
                        result.Add(m.Groups["value"].Value);
                        DisplayMsg(LogType.Log, "Input:" + m.Value);
                    }
                }

                if (result.Count == codeNumber)
                {
                    getmac = true;
                    break;
                }
                Thread.Sleep(1000);
                DisplayMsg(LogType.Log, "Retry Scan.....");
            }
            if (!getmac)
            {
                warning = "Scan barcode by camera fail";
            }
        }

        private void USB20()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            string item = "USB2p0";
            string res = string.Empty;
            try
            {
                if (!SendAndChk(PortType.SSH, "cat /sys/bus/usb/devices/1-1/speed", "480", out res, 0, 3000))
                {
                    DisplayMsg(LogType.Log, "check usb speed fail");
                    AddData(item, 1);
                    return;
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                warning = "Exception USB20 speed failed!!!";
            }
        }
        public void secureBootDownloadfilesRequired()
        {
            if (!CheckGoNoGo())
            {
                return;
            }
            string item = "secure Boot Download files Required in Final station";
            //string keyword = "root@OpenWrt:~# \r\n";
            string res = string.Empty;
            DisplayMsg(LogType.Log, $"=============== {item} ===============");
            try
            {
                DisplayMsg(LogType.Log, @" --- Download all the config  --- ");
                SendAndChk(PortType.SSH, "mkdir /tmp/config", "", out res, 0, 3000);
                SendAndChk(PortType.SSH, "cp /overlay1/config/* /tmp/config", "", out res, 0, 3000);
                SendAndChk(PortType.SSH, "cp /overlay1/* /tmp/", "", out res, 0, 3000);
                SendAndChk(PortType.SSH, "ls /tmp/", "", out res, 0, 3000);
                SendAndChk(PortType.SSH, "ls /tmp/config", "", out res, 0, 3000);

                SendAndChk(PortType.SSH, "md5sum /tmp/fscrypt_context", "", out res, 0, 3000);
                SendAndChk(PortType.SSH, "chmod 777 /tmp/filesystem_encryption.sh", "", out res, 0, 3000);
                SendAndChk(PortType.SSH, "chmod 777 /tmp/qualcomm.sh", "", out res, 0, 3000);
                // --------------------------------------------------------------

            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Log, ex.Message + "__>>>check usb speed fail");
                AddData(item, 1);
            }
        }

    }
}

