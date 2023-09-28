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
using WNC.API;
using EasyLibrary;
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
                    infor.HWID = Func.ReadINI("Setting", "Final", "HWID", "1001");
                    infor.HWver = Func.ReadINI("Setting", "Final", "HWver", "EVT2");
                    infor.FWver = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@LRG1_MFG_FW");
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

                    if (string.IsNullOrEmpty(infor.WiFi_PWD) || string.IsNullOrEmpty(infor.Admin_PWD) || string.IsNullOrEmpty(infor.WiFi_SSID) || string.IsNullOrEmpty(infor.License_key)
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
                    DisplayMsg(LogType.Log, $"Get FWver trim is: LRG1_v{result}");
                    infor.FWver = "LRG1_v" + result;
                    DisplayMsg(LogType.Log, $"Get HWID From setting is: {infor.HWID}");
                    DisplayMsg(LogType.Log, $"Get HWver From setting is: {infor.HWver}");
                    DisplayMsg(LogType.Log, $"Get BaseMAC From SFCS is: {infor.BaseMAC}");
                    DisplayMsg(LogType.Log, $"Get WanMAC From SFCS is: {infor.WanMAC}");

                    DisplayMsg(LogType.Log, $"Get DECT_cal_rxtun From SFCS is: {infor.DECT_cal_rxtun}");
                    DisplayMsg(LogType.Log, $"Get BLEver From SFCS is: {infor.BLEver}");
                    DisplayMsg(LogType.Log, $"Get SEver From SFCS is: {infor.SEver}");
                    DisplayMsg(LogType.Log, $"Get DECT_rfpi From SFCS is: {infor.DECT_rfpi}");

                }
                else
                {
                    //Rena_20230407 add for HQ test
                    GetBoardDataFromExcel(status_ATS.txtPSN.Text, true);
                    infor.FWver = Func.ReadINI("Setting", "Final", "FWver", "v0.0.4.1");
                    infor.HWID = Func.ReadINI("Setting", "Final", "HWID", "1001");
                    infor.HWver = Func.ReadINI("Setting", "Final", "HWver", "EVT2");
                    infor.BaseMAC = MACConvert(infor.BaseMAC);
                    infor.WanMAC = MACConvert(infor.BaseMAC, 1);
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
                    SwitchRelay(CTRL.OFF);
                }
                else
                {
                    frmOK.Label = "Đảm bảo đã kết nối 'USB 3.0 flash drive và điện thoại SLIC', và 'dây mạng' đã được cắm vào cổng LAN màu vàng số 1, xin vui lòng bật nguồn và nhấn nút nguồn để khởi động";
                    frmOK.ShowDialog();
                }

                DisplayMsg(LogType.Log, "Power on!!!");

                ChkBootUp(PortType.SSH);

                CheckFWVerAndHWID();


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
                // ============== 8.3.2 ================
                CheckDECTValue(); //Rena_20230804, Verify DECT RXTUN and RFPI
                //this.VerifyPA2_Comp();    // testplan0925 remove
                // =====================================
                CheckBoardData();

                NFCTag_withReaderTool();

                if (isLoop == 0)
                    EthernetTest(false);

                CheckEthernetMAC();

                if (isLoop == 0)
                    CheckLED();
                CheckWiFiCalData(PortType.SSH);

                CheckPCIe();

                if (isLoop == 0)
                    WPSButton();

                if (isLoop == 0)
                    ResetButton();

                //SLICTest();
                SLICTest_ByUsbModem();

                USB30Test();

                CurrentSensor();

                //Rena_20230720, remove /test/file
                if (CheckGoNoGo())
                {
                    SendAndChk(PortType.SSH, "rm /test/file;sync", "root@OpenWrt:~# \r\n", 0, 5000);
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
            string keyword = "root@OpenWrt:~# \r\n";
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
                    if (SendAndChk(PortType.SSH, "q", keyword, out res, 0, 2000))
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
                SendAndChk(PortType.SSH, "mkdir /test", keyword, out res, 0, 5000);
                SendAndChk(PortType.SSH, "mount -t ext4 /dev/mmcblk0p32 /test", keyword, out res, 0, 5000);
                SendAndChk(PortType.SSH, "cat /test/file", keyword, out res, 0, 5000);
                if (!res.Contains("test123"))
                {
                    DisplayMsg(LogType.Log, "Verify partition data fail");
                    AddData(item, 1);
                }

                //Rena_20230717, remove /test/file
                //SendAndChk(PortType.SSH, "rm /test/file;sync", keyword, out res, 0, 5000);
                //SendAndChk(PortType.SSH, "umount /test", keyword, out res, 0, 5000);

                //check Board data
                SendAndChk(PortType.SSH, "mt boarddata", keyword, out res, 0, 5000);
                //serial_number=+119746+2333000129
                if (!res.Contains($"serial_number={infor.SerialNumber}"))
                {
                    DisplayMsg(LogType.Log, "Check serial_number fail");
                    AddData(item, 1);
                }
                //hw_ver=EVT1
                if (!res.Contains($"hw_ver={infor.HWver}"))
                {
                    DisplayMsg(LogType.Log, "Check hw_ver fail");
                    AddData(item, 1);
                }
                //mac_base=E8:C7:CF:AF:46:28
                DisplayMsg(LogType.Log, $"mac_base get SFCS: {infor.BaseMAC}");
                string formattedBaseMAC = string.Empty;
                formattedBaseMAC = InsertColon(infor.BaseMAC);
                DisplayMsg(LogType.Log, $"mac_base Convert: {formattedBaseMAC}");
                if (formattedBaseMAC.Length != 17 || formattedBaseMAC.Length != infor.WanMAC.Length) { DisplayMsg(LogType.Log, "Leng mac fail"); return; }
                if (!res.Contains($"mac_base={formattedBaseMAC.ToUpper()}"))
                {
                    DisplayMsg(LogType.Log, "Check mac_base fail");
                    AddData(item, 1);
                }
                else
                {
                    DisplayMsg(LogType.Log, $"DUT mac_base: {formattedBaseMAC}");
                    DisplayMsg(LogType.Log, "Check mac_base PASS");
                }

                DisplayMsg(LogType.Log, $"SN get SFCS: {infor.SerialNumber}");
                if (!res.Contains($"serial_number={infor.SerialNumber}"))
                {
                    DisplayMsg(LogType.Log, "Check SN fail");
                    AddData(item, 1);
                }
                else
                {
                    DisplayMsg(LogType.Log, $"DUT SN: {infor.SerialNumber}");
                    DisplayMsg(LogType.Log, "Check SN PASS");
                }

                //dect_identity_rfpi=03.6C.D3.A9.38
                DisplayMsg(LogType.Log, $"DECT RFPI get SFCS: {infor.DECT_rfpi}");
                if (!res.Contains($"dect_identity_rfpi={infor.DECT_rfpi.ToUpper()}"))
                {
                    DisplayMsg(LogType.Log, "Check dect_identity_rfpi fail");
                    AddData(item, 1);
                }
                else
                {
                    DisplayMsg(LogType.Log, $"DUT DECT RFPI: {infor.DECT_rfpi}");
                    DisplayMsg(LogType.Log, "Check DECT RFPI PASS");
                }

                //dect_rf_calibration_rxtun=77
                DisplayMsg(LogType.Log, $"DECT RXturn get SFCS: {infor.DECT_cal_rxtun}");
                if (!res.Contains($"dect_rf_calibration_rxtun={infor.DECT_cal_rxtun}"))
                {
                    DisplayMsg(LogType.Log, "Check dect_rf_calibration_rxtun fail");
                    AddData(item, 1);
                }
                else
                {
                    DisplayMsg(LogType.Log, $"DUT DECT RXturn: {infor.DECT_cal_rxtun}");
                    DisplayMsg(LogType.Log, "Check DECT RXturn PASS");
                }

                //wifi_password=hgzEYyxeu7UFTdfr
                DisplayMsg(LogType.Log, $"Wifi_password get SFCS: {infor.WiFi_PWD}");
                if (!res.Contains($"wifi_password={infor.WiFi_PWD}"))
                {
                    DisplayMsg(LogType.Log, "Check wifi_password fail");
                    AddData(item, 1);
                }
                else
                {
                    DisplayMsg(LogType.Log, $"DUT wifi_password: {infor.WiFi_PWD}");
                    DisplayMsg(LogType.Log, "Check wifi_password PASS");
                }

                //admin_password=citerxfg
                DisplayMsg(LogType.Log, $"admin_password get SFCS: {infor.Admin_PWD}");
                if (!res.Contains($"admin_password={infor.Admin_PWD}"))
                {
                    DisplayMsg(LogType.Log, "Check admin_password fail");
                    AddData(item, 1);
                }
                else
                {
                    DisplayMsg(LogType.Log, $"DUT admin_password: {infor.Admin_PWD}");
                    DisplayMsg(LogType.Log, "Check admin_password PASS");
                }

                //wlan_ssid=BT-F5C26X
                DisplayMsg(LogType.Log, $"wlan_ssid get SFCS: {infor.WiFi_SSID}");
                if (!res.Contains($"wlan_ssid={infor.WiFi_SSID}"))
                {
                    DisplayMsg(LogType.Log, "Check wlan_ssid fail");
                    AddData(item, 1);
                }
                else
                {
                    DisplayMsg(LogType.Log, $"DUT wlan_ssid: {infor.WiFi_SSID}");
                    DisplayMsg(LogType.Log, "Check wlan_ssid PASS");
                }

                DisplayMsg(LogType.Log, $"D2License get SFCS: {infor.License_key}");
                if (!SendAndChk(PortType.SSH, "cat /defaults/D2License.key", infor.License_key, out res, 0, 5000))
                {
                    DisplayMsg(LogType.Log, "Check D2 License Key fail");
                    AddData(item, 1);
                }
                else
                {
                    DisplayMsg(LogType.Log, $"DUT D2License: {infor.License_key}");
                    DisplayMsg(LogType.Log, "Check D2License PASS");
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
        private bool RF_enable_all_wifi()
        {
            if (!CheckGoNoGo())
            {
                return false;
            }
        //bool retry = false;
        retry:
            try
            {
                DisplayMsg(LogType.Log, $"========= WiFi PreSetting  =========");

                int delayMs = 5;
                int timeOutMs = 30 * 1000;
                WiFiInformation collection = RfType(WiFiType.WiFi);
                string keyword = "root@OpenWrt:/#";

                DisplayMsg(LogType.Log, @"----------------------------------------------");
                //SendAndChk(collection.Name, PortType.SSH, "uci set wireless.wifi0.disabled='0'", keyword, delayMs, timeOutMs);
                SendCommand(PortType.SSH, "uci set wireless.wifi0.disabled='0'", delayMs);
                Thread.Sleep(5000);
                //MessageBox.Show(@"stop");
                //SendAndChk(collection.Name, PortType.SSH, "uci set wireless.wifi1.disabled='0'", keyword, delayMs, timeOutMs);
                SendCommand(PortType.SSH, "uci set wireless.wifi1.disabled='0'", delayMs);
                Thread.Sleep(5000);
                //SendAndChk(collection.Name, PortType.SSH, "uci set wireless.wifi2.disabled='0'", keyword, delayMs, timeOutMs);
                SendCommand(PortType.SSH, "uci set wireless.wifi2.disabled='0'", delayMs);
                Thread.Sleep(5000);
                SendAndChk(collection.Name, PortType.SSH, "uci commit", keyword, delayMs, timeOutMs);
                SendAndChk(collection.Name, PortType.SSH, "wifi", keyword, delayMs, timeOutMs);
                SendAndChk(collection.Name, PortType.SSH, "sleep 10", keyword, delayMs, timeOutMs); //TODO:?
                Thread.Sleep(2000);
                DisplayMsg(LogType.Log, @"----------------------------------------------");
                return true;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                //status_ATS.AddDataLog(type.ToString(), NG);
                #region Retry
                //if (!CheckGoNoGo() && !retry)
                {
                    //retry = true;
                    DisplayMsg(LogType.Log, "Retry Wifi preseting...");
                    RemoveFailedItem();
                    warning = string.Empty;
                    goto retry;
                }
                #endregion
                //AddData("PreSetting_ RF_enable_all_wifi", 1);
                return false;
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

                OpenTftpd32New(wifi_data_backup_path, 15 * 1000);
                //Backup wifi data
                SendAndChk(portType, "cat /dev/mmcblk0p21 > /tmp/backupwifi", keyword, out res, 0, 5000);
                if (res.Contains("No such file or directory"))
                {
                    DisplayMsg(LogType.Log, "Backup wifi data fail");
                    AddData(item, 1);
                    return;
                }
                //Thread.Sleep(5 * 1000);
                // ================================
                //this.WiFiClear();
                //this.RF_enable_all_wifi();
                // ================================
                //check WiFi 2.4G MAC = BaseMAC+4
                string wifi_2g_mac = MACConvert(infor.BaseMAC, 4);
                DisplayMsg(LogType.Log, "WiFi_2G_MAC: " + wifi_2g_mac);
                //this.WiFiPreSetting(WiFiType.WiFi_2G);
                //SendAndChk(PortType.UART, "ifconfig ath0", keyword, out res, 0, 5000);
                //if (!res.Contains(wifi_2g_mac))
                //{
                //    DisplayMsg(LogType.Log, "Check WiFi 2.4G MAC fail");
                //    AddData(item, 1);
                //    return;
                //}
                wifi_2g_mac = Regex.Replace(wifi_2g_mac, regex, "$2$1 $4$3 $6$5").ToLower();
                SendAndChk(portType, "hexdump -s 0x58810 -n 6 /dev/mmcblk0p21", keyword, out res, 0, 5000);
                if (!res.Contains(wifi_2g_mac))
                {
                    DisplayMsg(LogType.Log, "Check WiFi 2.4G MAC fail");
                    AddData(item, 1);
                    return;
                }

                //check WiFi 5G MAC = BaseMAC+3
                string wifi_5g_mac = MACConvert(infor.BaseMAC, 3);
                DisplayMsg(LogType.Log, "WiFi_5G_MAC: " + wifi_5g_mac);
                //this.WiFiPreSetting(WiFiType.WiFi_5G);
                //SendAndChk(portType, "ifconfig ath2", "ath2", out res, 0, 5000);
                //DisplayMsg(LogType.Log, $"res_______{res}");
                //if (!res.Contains(wifi_5g_mac))
                //{
                //    DisplayMsg(LogType.Log, "Check WiFi 5G MAC fail");
                //    AddData(item, 1);
                //    return;
                //}
                wifi_5g_mac = Regex.Replace(wifi_5g_mac, regex, "$2$1 $4$3 $6$5").ToLower();
                SendAndChk(portType, "hexdump -s 0xbc810 -n 6 /dev/mmcblk0p21", keyword, out res, 0, 5000);
                if (!res.Contains(wifi_5g_mac))
                {
                    DisplayMsg(LogType.Log, "Check WiFi 5G MAC fail");
                    AddData(item, 1);
                    return;
                }

                //check WiFi 6G MAC = BaseMAC+2
                string wifi_6g_mac = MACConvert(infor.BaseMAC, 2);
                DisplayMsg(LogType.Log, "WiFi_6G_MAC: " + wifi_6g_mac);
                //this.WiFiPreSetting(WiFiType.WiFi_6G);
                //SendAndChk(portType, "ifconfig ath1 | grep ath1", "ath1", out res, 0, 5000);
                //if (!res.Contains(wifi_6g_mac))
                //{
                //    DisplayMsg(LogType.Log, "Check WiFi 6G MAC fail");
                //    AddData(item, 1);
                //    return;
                //}
                wifi_6g_mac = Regex.Replace(wifi_6g_mac, regex, "$2$1 $4$3 $6$5").ToLower();
                SendAndChk(portType, "hexdump -s 0x8a810 -n 6 /dev/mmcblk0p21", keyword, out res, 0, 5000);
                if (!res.Contains(wifi_6g_mac))
                {
                    DisplayMsg(LogType.Log, "Check WiFi 6G MAC fail");
                    AddData(item, 1);
                    return;
                }


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
                        DisplayMsg(LogType.Log, $"Backup wifi data in {Path.Combine(wifi_data_backup_path, $"backupwifi_{infor.SerialNumber}")}");
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



    }
}
