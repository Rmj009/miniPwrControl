using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiniPwrSupply.LRG1
{
    public partial class frmMain
    {
        //FW switch from WNC to Indigo via ethernet
        private void FWSwitch()
        {
            try
            {
                infor.ResetParam();
                SFCS_Query _sfcsQuery = new SFCS_Query();
                ATS_Template.SFCS_ATS_2_0.ATS ss = new ATS_Template.SFCS_ATS_2_0.ATS();
                string sfcsSSID = string.Empty;
                string sfcPW = string.Empty;
                string sfcadminpw = string.Empty;

                #region create SMT file

                if (status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode)
                {
                    infor.SerialNumber = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@LRG1_SN");
                    DisplayMsg(LogType.Log, $"Get SN From SFCS is: {infor.SerialNumber}");
                    if (infor.SerialNumber.Length == 18)
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
                    SetTextBox(status_ATS.txtPSN, infor.SerialNumber);
                    status_ATS.SFCS_Data.PSN = infor.SerialNumber;
                    status_ATS.SFCS_Data.First_Line = infor.SerialNumber;
                    infor.FWver_Cust = WNC.API.Func.ReadINI("Setting", "FWSwitch", "FWver_Cust", "r0.72.0-N20230424-748456");
                    infor.HWver_Cust = WNC.API.Func.ReadINI("Setting", "FWSwitch", "HWver_Cust", "R00");
                    DisplayMsg(LogType.Log, $"Get FWver_Cust From setting is: {infor.FWver_Cust}");
                    DisplayMsg(LogType.Log, $"Get HWver_Cust From setting is: {infor.HWver_Cust}");



                    for (int i = 0; i < 3; i++)
                    {
                        DisplayMsg(LogType.Log, "Delay 1s...");
                        Thread.Sleep(1000);

                        infor.BaseMAC = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@MAC");
                        infor.BaseMAC = MACConvert(infor.BaseMAC);
                        infor.WanMAC = MACConvert(infor.BaseMAC, 1);
                        if (infor.SerialNumber != "")
                            break;
                    }


                    DisplayMsg(LogType.Log, $"Get BaseMAC From SFCS is: {infor.BaseMAC}");
                    DisplayMsg(LogType.Log, $"Get WanMAC From SFCS is: {infor.WanMAC}");
                    if (infor.SerialNumber.Length == 18)
                    {
                        DisplayMsg(LogType.Log, "Get  Data in SFCs OK...");

                    }
                    else
                    {
                        warning = "Get SN from SFCS fail";
                        return;
                    }

                    _sfcsQuery.Get15Data(status_ATS.txtPSN.Text, "LRG1_LABEL_NETWORK", ref infor.WiFi_SSID);
                    _sfcsQuery.Get15Data(status_ATS.txtPSN.Text, "LRG1_LABEL_PW", ref infor.WiFi_PWD);
                    _sfcsQuery.Get15Data(status_ATS.txtPSN.Text, "LRG1_LABEL_ADMIN_PW", ref infor.Admin_PWD);
                    DisplayMsg(LogType.Log, "LRG1_LABEL_NETWORK from SFCS:" + infor.WiFi_SSID);
                    DisplayMsg(LogType.Log, "LRG1_LABEL_PW from SFCS:" + infor.WiFi_PWD);
                    DisplayMsg(LogType.Log, "LRG1_LABEL_ADMIN_PW from SFCS:" + infor.Admin_PWD);

                }
                else
                {
                    //Rena_20230407 add for HQ test
                    // GetBoardDataFromExcel(status_ATS.txtPSN.Text, true);

                    infor.FWver_Cust = WNC.API.Func.ReadINI("Setting", "FWSwitch", "FWver_Cust", "r0.72.0-N20230424-748456");
                    infor.HWver_Cust = WNC.API.Func.ReadINI("Setting", "FWSwitch", "HWver_Cust", "R00");
                }
                if (!ChkStation(status_ATS.txtPSN.Text))
                    return;

                #endregion

                if (WNC.API.Func.ReadINI("Setting", "IO_Board_Control", "IO_Control_1", "0") == "1")
                {
                    string txPin = WNC.API.Func.ReadINI("Setting", "IO_Board_Control", "Pin0", "0");
                    string rev_message = "";
                    status_ATS.AddLog("IO_Board_Y" + txPin + " On...");
                    IO_Board_Control1.ConTrolIOPort_write(Int32.Parse(txPin), "1", ref rev_message);
                    DisplayMsg(LogType.Log, rev_message);
                }
                else if (WNC.API.Func.ReadINI("Setting", "Port", "RelayBoard", "Disable").ToUpper() == "ENABLE")
                {
                    SwitchRelay(CTRL.ON);
                    Thread.Sleep(5000);
                    SwitchRelay(CTRL.OFF);
                }
                else
                {
                    frmOK.Label = "Vui lòng kiểm tra 'dây mạng' đã được cắm vào cổng lan màu vàng, sau đó kết nối DUT và thiết bị kiểm tra với nhau bằng dây mạng\r\nHãy cấp nguồn cho DUT và thiết bị kiểm tra và nhấn nút power để bật máy";
                    frmOK.ShowDialog();
                }
                DisplayMsg(LogType.Log, "Power on!!!");

                //考慮到重測流程,如果已更新到indigo FW就直接執行VerifyBoardData
                //ping MFG FW(192.168.1.1) fail -> ping indigo FW(192.168.1.254) pass -> skip upgrade, do VerifyBoardData
                FwSwichCheck = false;
                int Duttimeoutping = Convert.ToInt32(WNC.API.Func.ReadINI("Setting", "FWSwitch", "DUTTimeoutPing", "120"));
                int CusSwPingTime = Convert.ToInt32(WNC.API.Func.ReadINI("Setting", "FWSwitch", "CusSwPingTime", "70"));
                string Indigo_IP = WNC.API.Func.ReadINI("Setting", "IP", "Indigo", "192.168.1.254");
                if (!telnet.Ping(sshInfo.ip, Duttimeoutping * 1000))
                {
                    /*DisplayMsg(LogType.Log, $"Ping {sshInfo.ip} fail, start ping {Indigo_IP}...");
                    if (telnet.Ping(Indigo_IP, CusSwPingTime * 1000))
                    {
                        DisplayMsg(LogType.Log, "Already is Indigo FW, skip FW upgrade process");
                        FwSwichCheck = true;
                        goto VerifyBoardData;
                    }
                    else
                    {
                        DisplayMsg(LogType.Log, $"Ping {sshInfo.ip} and {Indigo_IP} failed");
                        AddData("BootUp", 1);
                        return;
                    }*/

                    DisplayMsg(LogType.Log, $"Ping {sshInfo.ip} failed");
                    AddData("BootUp", 1);
                    return;
                }

                ChkBootUp(PortType.SSH);
                /* #region get SN
                 string res = "";
                 string DUTSN = string.Empty;
                 string DUTNETWORKSSID=string.Empty;
                 string DUTPW=string.Empty;
                 string DUTadminpw=string.Empty;
                 string DUTbaseMAC=string.Empty;
                 if (!SendAndChk(PortType.SSH, "verify_boarddata.sh", "root@OpenWrt:~#", out res, 0, 5000)) { AddData("CheckSN", 1); return; }
                 Match match = Regex.Match(res, @"serial_number=(\+\d+\+\d+)");
                 if (match.Success)
                 {
                     DUTSN = match.Groups[1].Value;
                     DisplayMsg(LogType.Log, "DUTSN: " + DUTSN);
                 }
                 else
                 {
                     warning = "Failed to find DUTSN";
                     AddData("CheckSN", 1);
                     return;
                 }

                 if(DUTSN.Length!=18)
                 {
                     warning = "DUT SN leng is not 18...";
                     AddData("CheckSN", 1);
                     return;
                 }
                 if (!DUTSN.Contains(infor.SerialNumber))
                 {
                     warning = "DUT SN is not meet SFCS...";
                     AddData("CheckSN", 1);
                     return;
                 }
                #endregion get SN*/

                UpgradeIndigoFW();

                //VerifyBoardData:
                VerifyBoardData();
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                warning = "Exception";
            }
            finally
            {
                if (WNC.API.Func.ReadINI("Setting", "IO_Board_Control", "IO_Control_1", "0") == "1")
                {
                    string txPin = WNC.API.Func.ReadINI("Setting", "IO_Board_Control", "Pin0", "0");
                    string rev_message = "";
                    status_ATS.AddLog("IO_Board_Y" + txPin + " Off...");
                    IO_Board_Control1.ConTrolIOPort_write(Int32.Parse(txPin), "2", ref rev_message);
                    DisplayMsg(LogType.Log, rev_message);
                }
                else SwitchRelay(CTRL.ON);
            }
        }

        static string CalculateFileMD5Hash(string filePath, string filename)
        {
            string fullPath = Path.Combine(filePath, filename);

            using (MD5 md5 = MD5.Create())
            {
                using (FileStream stream = File.OpenRead(fullPath))
                {
                    byte[] hashBytes = md5.ComputeHash(stream);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToUpper();
                }
            }
        }

        private void FWBackToMFG()
        {
            try
            {
                infor.ResetParam();
                SFCS_Query _sfcsQuery = new SFCS_Query();
                ATS_Template.SFCS_ATS_2_0.ATS ss = new ATS_Template.SFCS_ATS_2_0.ATS();
                string sfcsSSID = string.Empty;
                string sfcPW = string.Empty;
                string sfcadminpw = string.Empty;

                #region create SMT file

                if (status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode)
                {
                    infor.SerialNumber = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@LRG1_SN");
                    DisplayMsg(LogType.Log, $"Get SN From SFCS is: {infor.SerialNumber}");
                    if (infor.SerialNumber.Length == 18)
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
                    SetTextBox(status_ATS.txtPSN, infor.SerialNumber);
                    status_ATS.SFCS_Data.PSN = infor.SerialNumber;
                    status_ATS.SFCS_Data.First_Line = infor.SerialNumber;
                    for (int i = 0; i < 3; i++)
                    {
                        DisplayMsg(LogType.Log, "Delay 1s...");
                        Thread.Sleep(1000);

                        infor.BaseMAC = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@MAC");
                        infor.BaseMAC = MACConvert(infor.BaseMAC);
                        infor.WanMAC = MACConvert(infor.BaseMAC, 1);
                        if (infor.SerialNumber != "")
                            break;
                    }


                    DisplayMsg(LogType.Log, $"Get BaseMAC From SFCS is: {infor.BaseMAC}");
                    DisplayMsg(LogType.Log, $"Get WanMAC From SFCS is: {infor.WanMAC}");
                    if (infor.SerialNumber.Length == 18)
                    {
                        DisplayMsg(LogType.Log, "Get  Data in SFCs OK...");

                    }
                    else
                    {
                        warning = "Get SN from SFCS fail";
                        return;
                    }

                    _sfcsQuery.Get15Data(status_ATS.txtPSN.Text, "LRG1_LABEL_NETWORK", ref infor.WiFi_SSID);
                    _sfcsQuery.Get15Data(status_ATS.txtPSN.Text, "LRG1_LABEL_PW", ref infor.WiFi_PWD);
                    _sfcsQuery.Get15Data(status_ATS.txtPSN.Text, "LRG1_LABEL_ADMIN_PW", ref infor.Admin_PWD);
                    DisplayMsg(LogType.Log, "LRG1_LABEL_NETWORK from SFCS:" + infor.WiFi_SSID);
                    DisplayMsg(LogType.Log, "LRG1_LABEL_PW from SFCS:" + infor.WiFi_PWD);
                    DisplayMsg(LogType.Log, "LRG1_LABEL_ADMIN_PW from SFCS:" + infor.Admin_PWD);

                }
                else
                {
                    //Rena_20230407 add for HQ test
                    // GetBoardDataFromExcel(status_ATS.txtPSN.Text, true);

                    /*infor.FWver_Cust = WNC.API.Func.ReadINI("Setting", "FWSwitch", "FWver_Cust", "r0.72.0-N20230424-748456");
                    infor.HWver_Cust = WNC.API.Func.ReadINI("Setting", "FWSwitch", "HWver_Cust", "R00");*/
                }

                string Imagefile = WNC.API.Func.ReadINI("Setting", "FWBackToMFG", "Imagefile", "r0.72.0-N20230424-748456");
                string ImagefileMD5 = WNC.API.Func.ReadINI("Setting", "FWBackToMFG", "ImagefileMD5", "@@");

                string serverip = WNC.API.Func.ReadINI("Setting", "FWBackToMFG", "serverip", "192.168.1.2");
                string ipaddr = WNC.API.Func.ReadINI("Setting", "FWBackToMFG", "ipaddr", "192.168.1.1");
                string macid = WNC.API.Func.ReadINI("Setting", "FWBackToMFG", "macid", "8050301");

                string Imagefilepath = WNC.API.Func.ReadINI("Setting", "FWBackToMFG", "Imagefilepath", "@@");

                DisplayMsg(LogType.Log, $"Get Imagefilepath From setting is: {Imagefilepath}");
                DisplayMsg(LogType.Log, $"Get Imagefile From setting is: {Imagefile}");
                DisplayMsg(LogType.Log, $"Get ImagefileMD5 From setting is: {ImagefileMD5}");
                DisplayMsg(LogType.Log, $"Get serverip From setting is: {serverip}");
                DisplayMsg(LogType.Log, $"Get ipaddr From setting is: {ipaddr}");
                DisplayMsg(LogType.Log, $"Get macid From setting is: {macid}");

                if (status_ATS.txtPSN.Text.Length == 18)
                {
                    SetTextBox(status_ATS.txtPSN, status_ATS.txtPSN.Text);
                    status_ATS.SFCS_Data.PSN = status_ATS.txtPSN.Text;
                    status_ATS.SFCS_Data.First_Line = status_ATS.txtPSN.Text;
                }
                else
                {
                    warning = "SN Input type wrong";
                    return;
                }

                if (status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode)
                {
                    if (!ChkStation(status_ATS.txtPSN.Text))
                        return;
                }
                #endregion

                string md5Hash = CalculateFileMD5Hash(Imagefilepath, Imagefile);
                DisplayMsg(LogType.Log, $"Get ImagefileMD5 From setting is: {ImagefileMD5}");
                DisplayMsg(LogType.Log, $"MD5 Of current Imagefile: {md5Hash}");
                if (md5Hash != ImagefileMD5 || !md5Hash.Contains(ImagefileMD5) || md5Hash.Length != ImagefileMD5.Length)
                {
                    warning = "Check md5 Image file Fail";
                    return;
                }
                else
                {
                    DisplayMsg(LogType.Log, $"Check current MD5 Image with setting PASS");
                }

                if (WNC.API.Func.ReadINI("Setting", "IO_Board_Control", "IO_Control_1", "0") == "1")
                {
                    string txPin = WNC.API.Func.ReadINI("Setting", "IO_Board_Control", "Pin0", "0");
                    string rev_message = "";
                    status_ATS.AddLog("IO_Board_Y" + txPin + " On...");
                    IO_Board_Control1.ConTrolIOPort_write(Int32.Parse(txPin), "1", ref rev_message);
                    DisplayMsg(LogType.Log, rev_message);
                }
                else if (WNC.API.Func.ReadINI("Setting", "Port", "RelayBoard", "Disable").ToUpper() == "ENABLE")
                {
                    SwitchRelay(CTRL.OFF);
                }
                else
                {
                    frmOK.Label = "Vui lòng kiểm tra 'dây mạng' đã được cắm vào cổng lan màu vàng, sau đó kết nối DUT và thiết bị kiểm tra với nhau bằng dây mạng";
                    frmOK.ShowDialog();

                    frmOK.Label = "Bật Nguồn";
                    frmOK.ShowDialog();
                }
                DisplayMsg(LogType.Log, "Power on!!!");


                string res = string.Empty;

                #region Enter uboot set command time 1
                if (!EnterUboot(PortType.DUT_UART, "IPQ9574#", 20 * 1000))
                {
                    if (WNC.API.Func.ReadINI("Setting", "IO_Board_Control", "IO_Control_1", "0") == "1")
                    {
                        string txPin = WNC.API.Func.ReadINI("Setting", "IO_Board_Control", "Pin0", "0");
                        string rev_message = "";
                        status_ATS.AddLog("IO_Board_Y" + txPin + " Off...");
                        IO_Board_Control1.ConTrolIOPort_write(Int32.Parse(txPin), "2", ref rev_message);
                        DisplayMsg(LogType.Log, rev_message);

                        // on power
                        rev_message = "";
                        status_ATS.AddLog("IO_Board_Y" + txPin + " On...");
                        IO_Board_Control1.ConTrolIOPort_write(Int32.Parse(txPin), "1", ref rev_message);
                        DisplayMsg(LogType.Log, rev_message);
                    }
                    else if (WNC.API.Func.ReadINI("Setting", "Port", "RelayBoard", "Disable").ToUpper() == "ENABLE")
                    {
                        SwitchRelay(CTRL.OFF);
                    }
                    else
                    {
                        frmOK.Label = "Tắt sau đó bật nút khuồn lại";
                        frmOK.ShowDialog();
                    }


                    if (!EnterUboot(PortType.DUT_UART, "IPQ9574#", 20 * 1000))
                    {
                        AddData("Enteruboot", 1);
                        return;
                    }
                }
                // AddData("Enteruboot", 0);

                if (!SendAndChk(PortType.DUT_UART, "mmc erase 0x120622 0x10000", "blocks erased: OK", out res, 0, 3000)) { return; }
                if (!SendAndChk(PortType.DUT_UART, "mmc erase 0x130622 0x10000", "blocks erased: OK", out res, 0, 3000)) { return; }

                SendAndChk(PortType.DUT_UART, $"setenv serverip {serverip}", "IPQ9574#", out res, 0, 3000);
                SendAndChk(PortType.DUT_UART, $"setenv ipaddr {ipaddr}", "IPQ9574#", out res, 0, 3000);
                //SendAndChk(PortType.DUT_UART, $"tftpboot {Imagefile}", "IPQ9574#", out res, 0, 30000);
                //SendAndChk(PortType.DUT_UART, $"md5sum {Imagefile}", ImagefileMD5, out res, 0, 10000);
                if (!SendAndChk(PortType.DUT_UART, $"tftpboot {Imagefile} && setenv machid 8050301 && imgaddr=$fileaddr && source $imgaddr:script && saveenv", "Writing to MMC(0)... done", out res, 0, 30000))
                {
                    return;
                }

                if (!SendAndChk(PortType.DUT_UART, $"printenv machid", macid, out res, 0, 10000)) { return; }
                SendAndChk(PortType.DUT_UART, $"reset", "", out res, 0, 10000);
                #endregion Enter uboot set command time 1

                res = string.Empty;
                #region Enter uboot set command time 2
                if (!EnterUboot(PortType.DUT_UART, "IPQ9574#", 20 * 1000))
                {
                    if (WNC.API.Func.ReadINI("Setting", "IO_Board_Control", "IO_Control_1", "0") == "1")
                    {
                        string txPin = WNC.API.Func.ReadINI("Setting", "IO_Board_Control", "Pin0", "0");
                        string rev_message = "";
                        status_ATS.AddLog("IO_Board_Y" + txPin + " Off...");
                        IO_Board_Control1.ConTrolIOPort_write(Int32.Parse(txPin), "2", ref rev_message);
                        DisplayMsg(LogType.Log, rev_message);

                        // on power
                        rev_message = "";
                        status_ATS.AddLog("IO_Board_Y" + txPin + " On...");
                        IO_Board_Control1.ConTrolIOPort_write(Int32.Parse(txPin), "1", ref rev_message);
                        DisplayMsg(LogType.Log, rev_message);
                    }
                    else if (WNC.API.Func.ReadINI("Setting", "Port", "RelayBoard", "Disable").ToUpper() == "ENABLE")
                    {
                        SwitchRelay(CTRL.OFF);
                    }
                    else
                    {
                        frmOK.Label = "Tắt sau đó bật nút khuồn lại";
                        frmOK.ShowDialog();
                    }

                    if (!EnterUboot(PortType.DUT_UART, "IPQ9574#", 20 * 1000))
                    {
                        AddData("Enteruboot", 1);
                        return;
                    }
                }
                //AddData("Enteruboot", 0);

                if (!SendAndChk(PortType.DUT_UART, "env default -a", "Resetting to default environment", out res, 0, 3000)) { return; }
                SendAndChk(PortType.DUT_UART, "setenv board_variant bt,sh40j-p-l2", "IPQ9574#", out res, 0, 3000);
                if (!SendAndChk(PortType.DUT_UART, $"saveenv", "Writing to MMC(0)... done", out res, 0, 5000)) { return; }
                if (!SendAndChk(PortType.DUT_UART, $"printenv machid", macid, out res, 0, 3000)) { return; }
                SendAndChk(PortType.DUT_UART, $"printenv board_variant", "IPQ9574#", out res, 0, 3000);
                SendAndChk(PortType.DUT_UART, $"reset", "", out res, 0, 10000);
                #endregion Enter uboot set command time 2

                FwSwichCheck = false;
                int Duttimeoutping = Convert.ToInt32(WNC.API.Func.ReadINI("Setting", "FWBackToMFG", "DUTTimeoutPing", "120"));
                if (!telnet.Ping(sshInfo.ip, Duttimeoutping * 1000))
                {

                    DisplayMsg(LogType.Log, $"Ping {sshInfo.ip} failed");
                    AddData("BootUp", 1);
                    return;
                }
                DisplayMsg(LogType.Log, $"Back FW to MFG FW PASS");
                ChkBootUp(PortType.SSH);

                #region Check Info
                infor.BaseMAC = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@MAC");
                infor.FWver = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@LRG1_MFG_FW");
                DisplayMsg(LogType.Log, $"Get FWver From SFCS is: {infor.FWver}");
                string result = string.Empty;
                result = infor.FWver.Substring(0, infor.FWver.Length - 9);
                DisplayMsg(LogType.Log, $"Get FWver trim is: LRG1_v{result}");
                infor.FWver = "LRG1_v" + result;
                _sfcsQuery.Get15Data(status_ATS.txtPSN.Text, "LGR1_RXTURN", ref infor.DECT_cal_rxtun);
                _sfcsQuery.Get15Data(status_ATS.txtPSN.Text, "LRG1_BLE_Ver", ref infor.BLEver);
                _sfcsQuery.Get15Data(status_ATS.txtPSN.Text, "LRG1_SE_Ver", ref infor.SEver);
                _sfcsQuery.Get15Data(status_ATS.txtPSN.Text, "LRG1_DECT_rfpi", ref infor.DECT_rfpi);
                _sfcsQuery.Get15Data(status_ATS.txtPSN.Text, "LRG1_LABEL_PW", ref infor.WiFi_PWD);
                _sfcsQuery.Get15Data(status_ATS.txtPSN.Text, "LRG1_LABEL_ADMIN_PW", ref infor.Admin_PWD);
                _sfcsQuery.Get15Data(status_ATS.txtPSN.Text, "LRG1_LABEL_NETWORK", ref infor.WiFi_SSID);
                _sfcsQuery.Get15Data(status_ATS.txtPSN.Text, "LRG1_LICENSE_KEY", ref infor.License_key);
                infor.SerialNumber = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@LRG1_SN");

                DisplayMsg(LogType.Log, $"Get Base MAC From SFCS is: {infor.BaseMAC}");
                DisplayMsg(LogType.Log, $"Get LGR1_RXTURN From SFCS is: {infor.DECT_cal_rxtun}");
                DisplayMsg(LogType.Log, $"Get LRG1_BLE_Ver From SFCS is: {infor.BLEver}");
                DisplayMsg(LogType.Log, $"Get LRG1_SE_Ver From SFCS is: {infor.SEver}");
                DisplayMsg(LogType.Log, $"Get LRG1_LABEL_PW From SFCS is: {infor.WiFi_PWD}");
                DisplayMsg(LogType.Log, $"Get LRG1_LABEL_ADMIN_PW From SFCS is: {infor.Admin_PWD}");
                DisplayMsg(LogType.Log, $"Get LRG1_LABEL_NETWORK From SFCS is: {infor.WiFi_SSID}");
                DisplayMsg(LogType.Log, $"Get LRG1_LICENSE_KEY From SFCS is: {infor.License_key}");
                DisplayMsg(LogType.Log, $"Get @LRG1_SN Serial Number From SFCS is: {infor.SerialNumber}");

                if (string.IsNullOrEmpty(infor.FWver) || string.IsNullOrEmpty(infor.DECT_cal_rxtun) || string.IsNullOrEmpty(infor.BLEver) || string.IsNullOrEmpty(infor.SEver)
                    || string.IsNullOrEmpty(infor.DECT_rfpi) || string.IsNullOrEmpty(infor.WiFi_PWD) || string.IsNullOrEmpty(infor.Admin_PWD)
                    || string.IsNullOrEmpty(infor.WiFi_SSID) || string.IsNullOrEmpty(infor.License_key) || string.IsNullOrEmpty(infor.SerialNumber) || string.IsNullOrEmpty(infor.BaseMAC))
                {
                    warning = "Get from SFCS data fail";
                    return;
                }
                CheckBoardDataFW();

                #endregion Check Info
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                warning = "Exception";
                return;
            }
            finally
            {
                if (WNC.API.Func.ReadINI("Setting", "IO_Board_Control", "IO_Control_1", "0") == "1")
                {
                    string txPin = WNC.API.Func.ReadINI("Setting", "IO_Board_Control", "Pin0", "0");
                    string rev_message = "";
                    status_ATS.AddLog("IO_Board_Y" + txPin + " Off...");
                    IO_Board_Control1.ConTrolIOPort_write(Int32.Parse(txPin), "2", ref rev_message);
                    DisplayMsg(LogType.Log, rev_message);
                }
                else SwitchRelay(CTRL.ON);
            }
        }
        private void FWMFGToMFG()
        {
            try
            {
                infor.ResetParam();
                SFCS_Query _sfcsQuery = new SFCS_Query();
                ATS_Template.SFCS_ATS_2_0.ATS ss = new ATS_Template.SFCS_ATS_2_0.ATS();
                #region create SMT file

                string Imagefile = WNC.API.Func.ReadINI("Setting", "FWMFGToMFG", "Imagefile", "emmc-ipq9574-single.img");
                string ImagefileMD5 = WNC.API.Func.ReadINI("Setting", "FWMFGToMFG", "ImagefileMD5", "@@");
                string Imagefilepath = WNC.API.Func.ReadINI("Setting", "FWMFGToMFG", "Imagefilepath", "@@");
                infor.FWver = WNC.API.Func.ReadINI("Setting", "FWMFGToMFG", "FWversion", "LRG1_v0.2.7.1");
                string Bytrangfersetting = WNC.API.Func.ReadINI("Setting", "FWMFGToMFG", "Bytrangfersetting", "Bytes transferred = 64133424 (3d29930 hex)");

                DisplayMsg(LogType.Log, $"Get Imagefilepath From setting is: {Imagefilepath}");
                DisplayMsg(LogType.Log, $"Get Imagefile From setting is: {Imagefile}");
                DisplayMsg(LogType.Log, $"Get ImagefileMD5 From setting is: {ImagefileMD5}");
                DisplayMsg(LogType.Log, $"Get FW Version From setting is: {infor.FWver}");
                #endregion

                string md5Hash = CalculateFileMD5Hash(Imagefilepath, Imagefile);
                DisplayMsg(LogType.Log, $"Get ImagefileMD5 From setting is: {ImagefileMD5}");
                DisplayMsg(LogType.Log, $"MD5 Of current Imagefile: {md5Hash}");
                if (md5Hash != ImagefileMD5 || !md5Hash.Contains(ImagefileMD5) || md5Hash.Length != ImagefileMD5.Length)
                {
                    warning = "Check md5 Image file Fail";
                    return;
                }
                else
                {
                    DisplayMsg(LogType.Log, $"Check current MD5 of current Image with setting PASS");
                }

                if (WNC.API.Func.ReadINI("Setting", "IO_Board_Control", "IO_Control_1", "0") == "1")
                {
                    string txPin = WNC.API.Func.ReadINI("Setting", "IO_Board_Control", "Pin0", "0");
                    string rev_message = "";
                    status_ATS.AddLog("IO_Board_Y" + txPin + " On...");
                    IO_Board_Control1.ConTrolIOPort_write(Int32.Parse(txPin), "1", ref rev_message);
                    DisplayMsg(LogType.Log, rev_message);
                }
                else if (WNC.API.Func.ReadINI("Setting", "Port", "RelayBoard", "Disable").ToUpper() == "ENABLE")
                {
                    SwitchRelay(CTRL.OFF);
                }
                else
                {
                    frmOK.Label = "Vui lòng kiểm tra 'dây mạng' đã được cắm vào cổng lan màu vàng, sau đó kết nối DUT và thiết bị kiểm tra với nhau bằng dây mạng";
                    frmOK.ShowDialog();

                    frmOK.Label = "Bật Nguồn";
                    frmOK.ShowDialog();
                }
                DisplayMsg(LogType.Log, "Power on!!!");

                string res = string.Empty;
                #region Enter uboot set command time 1
                if (!EnterUboot(PortType.DUT_UART, "IPQ9574#", 20 * 1000))
                {
                    if (WNC.API.Func.ReadINI("Setting", "IO_Board_Control", "IO_Control_1", "0") == "1")
                    {
                        string txPin = WNC.API.Func.ReadINI("Setting", "IO_Board_Control", "Pin0", "0");
                        string rev_message = "";
                        status_ATS.AddLog("IO_Board_Y" + txPin + " Off...");
                        IO_Board_Control1.ConTrolIOPort_write(Int32.Parse(txPin), "2", ref rev_message);
                        DisplayMsg(LogType.Log, rev_message);

                        // on power
                        rev_message = "";
                        status_ATS.AddLog("IO_Board_Y" + txPin + " On...");
                        IO_Board_Control1.ConTrolIOPort_write(Int32.Parse(txPin), "1", ref rev_message);
                        DisplayMsg(LogType.Log, rev_message);
                    }
                    else if (WNC.API.Func.ReadINI("Setting", "Port", "RelayBoard", "Disable").ToUpper() == "ENABLE")
                    {
                        SwitchRelay(CTRL.OFF);
                    }
                    else
                    {
                        frmOK.Label = "Tắt sau đó bật nút khuồn lại";
                        frmOK.ShowDialog();
                    }


                    if (!EnterUboot(PortType.DUT_UART, "IPQ9574#", 20 * 1000))
                    {
                        AddData("Enteruboot", 1);
                        return;
                    }
                }

                if (!SendAndChk(PortType.DUT_UART, $"tftpboot {Imagefile} ", Bytrangfersetting, out res, 0, 40000))
                {
                    if (!SendAndChk(PortType.DUT_UART, $"\r\n", Bytrangfersetting, out res, 0, 40000))
                    {
                        AddData("UpgradeFW", 1);
                        return;
                    }
                }

                if (!SendAndChk(PortType.DUT_UART, "imgaddr=$fileaddr && source $imgaddr:script", "IPQ9574#", out res, 0, 10000)) { return; }

                // if (!SendAndChk(PortType.DUT_UART, "setenv machid 8050301 && imgaddr=$fileaddr && source $imgaddr:script && saveenv", "IPQ9574#", out res, 0, 10000)) { return; }

                SendAndChk(PortType.DUT_UART, $"reset", "", out res, 0, 10000);
                #endregion Enter uboot set command time 1

                res = string.Empty;
                FwSwichCheck = false;
                int Duttimeoutping = Convert.ToInt32(WNC.API.Func.ReadINI("Setting", "FWBackToMFG", "DUTTimeoutPing", "120"));
                if (!telnet.Ping(sshInfo.ip, Duttimeoutping * 1000))
                {

                    DisplayMsg(LogType.Log, $"Ping {sshInfo.ip} failed");
                    AddData("BootUp", 1);
                    return;
                }
                DisplayMsg(LogType.Log, $"Back MFG to MFG FW PASS");
                ChkBootUp(PortType.SSH);

                #region Check Info
                SendAndChk(PortType.SSH, "mt info", "#", out res, 0, 5000);

                DisplayMsg(LogType.Log, $"FWver get Setting: {infor.FWver}");
                if (!res.Contains($"{infor.FWver}"))
                {
                    DisplayMsg(LogType.Log, "Check FWver Fail");
                    AddData("CheckFWVer", 1);
                }
                else
                {
                    DisplayMsg(LogType.Log, $"DUT FWver: {infor.FWver}");
                    DisplayMsg(LogType.Log, "Check FWver PASS");
                    AddData("CheckFWVer", 0);
                }

                #endregion Check Info
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                warning = "Exception";
                return;
            }
            finally
            {
                if (!CheckGoNoGo())
                {
                    frmOK.Label = "Upgrade FAIL/ Nâng Cấp lỗi, Gọi PE Kiểm Tra!!!!!";
                    frmOK.ShowDialog();
                }
                else
                {
                    frmOK.Label = "Upgrade PASS/Nâng Cấp Thành công, Lấy sản Phẩm ra";
                    frmOK.ShowDialog();
                }

                if (WNC.API.Func.ReadINI("Setting", "IO_Board_Control", "IO_Control_1", "0") == "1")
                {
                    string txPin = WNC.API.Func.ReadINI("Setting", "IO_Board_Control", "Pin0", "0");
                    string rev_message = "";
                    status_ATS.AddLog("IO_Board_Y" + txPin + " Off...");
                    IO_Board_Control1.ConTrolIOPort_write(Int32.Parse(txPin), "2", ref rev_message);
                    DisplayMsg(LogType.Log, rev_message);
                }
                else SwitchRelay(CTRL.ON);
            }
        }
        private void CheckBoardDataFW()
        {
            if (!CheckGoNoGo())
            {
                DisplayMsg(LogType.Log, "Check Gonogo Fail");
                return;
            }

            DisplayMsg(LogType.Log, "=============== Verify Board data and D2 License Key After downgraded to MFG FW ===============");

            //string keyword = @"root@OpenWrt";
            string keyword = "root@OpenWrt:~# \r\n";
            string item = "ChkDUTInfo";
            string res = "";

            try
            {
                SendAndChk(PortType.SSH, "mt info", keyword, out res, 0, 5000);

                DisplayMsg(LogType.Log, $"FWver get SFCS: {infor.FWver}");
                if (!res.Contains($"{infor.FWver}"))
                {
                    DisplayMsg(LogType.Log, "Check FWver Fail");
                    AddData(item, 1);
                }
                else
                {
                    DisplayMsg(LogType.Log, $"DUT FWver: {infor.FWver}");
                    DisplayMsg(LogType.Log, "Check FWver PASS");
                }

                DisplayMsg(LogType.Log, $"HWver get SFCS: {infor.HWver}");
                if (!res.Contains($"{infor.HWver}"))
                {
                    DisplayMsg(LogType.Log, "Check hw_ver Fail");
                    AddData(item, 1);
                }
                else
                {
                    DisplayMsg(LogType.Log, $"DUT HWver: {infor.HWver}");
                    DisplayMsg(LogType.Log, "Check HWver PASS");
                }
                //check Board data
                SendAndChk(PortType.SSH, "mt boarddata", keyword, out res, 0, 5000);
                //serial_number=+119746+2333000129
                if (!res.Contains($"serial_number={infor.SerialNumber}"))
                {
                    DisplayMsg(LogType.Log, "Check serial_number fail");
                    AddData(item, 1);
                }
                //hw_ver=EVT1

                //mac_base=E8:C7:CF:AF:46:28
                DisplayMsg(LogType.Log, $"mac_base get SFCS: {infor.BaseMAC}");
                string formattedBaseMAC = string.Empty;
                formattedBaseMAC = InsertColon(infor.BaseMAC);
                DisplayMsg(LogType.Log, $"mac_base Convert: {formattedBaseMAC}");
                if (formattedBaseMAC.Length != 17) { DisplayMsg(LogType.Log, "Leng mac fail"); return; }
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


                var regex = "(.{2}):(.{2}):(.{2}):(.{2}):(.{2}):(.{2})";
                //check WiFi 2.4G MAC = BaseMAC+4
                string wifi_2g_mac = MACConvert(formattedBaseMAC, 4);
                DisplayMsg(LogType.Log, $"WiFi_2G_MAC convert from '{formattedBaseMAC}': " + wifi_2g_mac);
                string wifi_2g_machex = Regex.Replace(wifi_2g_mac, regex, "$2$1 $4$3 $6$5").ToLower();
                DisplayMsg(LogType.Log, $"WiFi_2G_MAC convert to hex: " + wifi_2g_machex);
                SendAndChk(PortType.SSH, "hexdump -s 0x58810 -n 6 /dev/mmcblk0p21", keyword, out res, 0, 5000);
                if (!res.Contains(wifi_2g_machex))
                {
                    DisplayMsg(LogType.Log, "Check WiFi 2.4G MAC fail");
                    AddData(item, 1);
                    return;
                }
                else
                {
                    DisplayMsg(LogType.Log, $"DUT WiFi_2G_MAC: {wifi_2g_mac}");
                    DisplayMsg(LogType.Log, $"DUT WiFi_2G_MAC hex value: {wifi_2g_machex}");
                    DisplayMsg(LogType.Log, $"Check WiFi 2.4G MAC PASS");
                }

                //check WiFi 5G MAC = BaseMAC+3
                string wifi_5g_mac = MACConvert(formattedBaseMAC, 3);
                DisplayMsg(LogType.Log, $"WiFi_5G_MAC convert from '{formattedBaseMAC}':  " + wifi_5g_mac);
                string wifi_5g_machex = Regex.Replace(wifi_5g_mac, regex, "$2$1 $4$3 $6$5").ToLower();
                DisplayMsg(LogType.Log, $"WiFi_5G_MAC convert to hex: " + wifi_5g_machex);
                SendAndChk(PortType.SSH, "hexdump -s 0xbc810 -n 6 /dev/mmcblk0p21", keyword, out res, 0, 5000);
                if (!res.Contains(wifi_5g_machex))
                {
                    DisplayMsg(LogType.Log, "Check WiFi 5G MAC fail");
                    AddData(item, 1);
                    return;
                }
                else
                {
                    DisplayMsg(LogType.Log, $"DUT WiFi_5G_MAC: {wifi_5g_mac}");
                    DisplayMsg(LogType.Log, $"DUT WiFi_5G_MAC hex value: {wifi_5g_machex}");
                    DisplayMsg(LogType.Log, $"Check WiFi 5G MAC PASS");
                }

                //check WiFi 6G MAC = BaseMAC+2
                string wifi_6g_mac = MACConvert(formattedBaseMAC, 2);
                DisplayMsg(LogType.Log, $"WiFi_6G_MAC convert from '{formattedBaseMAC}': " + wifi_6g_mac);
                string wifi_6g_machex = Regex.Replace(wifi_6g_mac, regex, "$2$1 $4$3 $6$5").ToLower();
                DisplayMsg(LogType.Log, $"WiFi_6G_MAC convert to hex: " + wifi_6g_machex);
                SendAndChk(PortType.SSH, "hexdump -s 0x8a810 -n 6 /dev/mmcblk0p21", keyword, out res, 0, 5000);
                if (!res.Contains(wifi_6g_machex))
                {
                    DisplayMsg(LogType.Log, "Check WiFi 6G MAC fail");
                    AddData(item, 1);
                    return;
                }
                else
                {
                    DisplayMsg(LogType.Log, $"DUT WiFi_6G_MAC: {wifi_6g_mac}");
                    DisplayMsg(LogType.Log, $"DUT WiFi_6G_MAC hex value: {wifi_6g_machex}");
                    DisplayMsg(LogType.Log, $"Check WiFi 6G MAC PASS");
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
        private bool EnterUboot(PortType port, string keyword, int timeOutMs)
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
                        DisplayMsg(LogType.Error, $"Check '{keyword}' timeOut");
                        return false;
                    }

                    switch (port)
                    {
                        case PortType.DUT_UART:
                            #region PortType.DUT_UART
                            if (atCmdUart == null)
                            {
                                atCmdUart = new SerialPort();
                                atCmdUart.PortName = WNC.API.Func.ReadINI("Setting", "Port", "ATCommandPort", "COM0");
                                atCmdUart.BaudRate = 115200;
                                atCmdUart.StopBits = StopBits.One;
                                atCmdUart.Parity = Parity.None;
                                atCmdUart.DataBits = 8;
                                atCmdUart.RtsEnable = false;
                            }
                            if (!ChkPort(atCmdUart.PortName))
                            {
                                DisplayMsg(LogType.Error, "Not find port : " + atCmdUart.PortName);
                                return false;
                            }

                            if (!atCmdUart.IsOpen)
                            {
                                DisplayMsg(LogType.Log, "Open uart port");
                                atCmdUart.Open();
                            }

                            res = atCmdUart.ReadExisting();
                            if (res.Length != 0)
                            {
                                DisplayMsg(LogType.Uart, res);
                                log += res;
                            }
                            break;
                        #endregion
                        default:
                            break;
                    }
                    if (log.Contains("autoboot"))
                    {
                        DisplayMsg(LogType.Log, "Transmitted '\r\n' to device");
                        atCmdUart.Write("\r\n");
                    }

                    if (log.Contains(keyword))
                    {
                        return true;
                    }
                    else
                    {
                        Thread.Sleep(200);
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
        private void UpgradeIndigoFW()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            int index = 0;
            string item = "UpgradeIndigoFW";
            string keyword = "root@OpenWrt:~# \r\n";
            string res = "";
            string PC_IP = WNC.API.Func.ReadINI("Setting", "IP", "PC", "192.168.1.2");
            string Indigo_IP = WNC.API.Func.ReadINI("Setting", "IP", "Indigo", "192.168.1.254");
            string Indigo_FW_FileName = WNC.API.Func.ReadINI("Setting", "FWSwitch", "Indigo_FW_FileName", "indigo-sh4a-r2.15.3-R-924755-PROD-1_standard_loader_update.itb");
            int PingTimeoutSec = Convert.ToInt32(WNC.API.Func.ReadINI("Setting", "FWSwitch", "PingTimeoutSec", "300"));

            DisplayMsg(LogType.Log, "=============== Upgrade to Indigo FW ===============");

            try
            {
                OpenFTPdmin();

                if (!CheckGoNoGo())
                {
                    return;
                }

                //Clear the data stored in overlay1 and overlay2
                SendAndChk(PortType.SSH, "dd if=/dev/zero of=/dev/mmcblk0p30", keyword, out res, 0, 5000);
                SendAndChk(PortType.SSH, "dd if=/dev/zero of=/dev/mmcblk0p31", keyword, out res, 0, 5000);

                //MFG FW v0.1.6.0更新到Indigo FW時,因為沒有寫u-boot env,更新完後的開機時間需要12mins
                //此為暫時解法,MFG FW進版後就不需要做了
                #region Setup default_env
                if (true)
                {
                    bool MD5_result_env = false;
                    SendAndChk(PortType.SSH, $"ftpget {PC_IP} indigo-sh4a-r2.15.1-R-918522-DEMO-1_standard_loader_update.itb", keyword, out res, 0, 5000);
                    while (index++ < 5)
                    {
                        SendAndChk(PortType.SSH, "md5sum indigo-sh4a-r2.15.1-R-918522-DEMO-1_standard_loader_update.itb", keyword, out res, 0, 5000);
                        if (res.Contains("1364aaf6bfad8a19902f79c357b39d4e")) // >>> need check it out
                        {
                            MD5_result_env = true;
                            DisplayMsg(LogType.Log, "MD5 check pass");
                            break;
                        }
                        else
                        {
                            DisplayMsg(LogType.Log, "MD5 check fail");
                            Thread.Sleep(500);
                        }
                    }
                    if (!MD5_result_env)
                    {
                        DisplayMsg(LogType.Log, " MD5 check fail");
                        AddData(item, 1);
                        return;
                    }
                    //SendAndChk(PortType.SSH, "dd if=default_env of=/dev/mmcblk0p17", keyword, out res, 0, 5000);
                    //SendAndChk(PortType.SSH, "dd if=default_env of=/dev/mmcblk0p18", keyword, out res, 0, 5000);
                }
                #endregion

                //download indigo FW
                if (!File.Exists(Path.Combine(Application.StartupPath, Indigo_FW_FileName)))
                {
                    DisplayMsg(LogType.Log, Path.Combine(Application.StartupPath, Indigo_FW_FileName) + " doesn't exist!!");
                    AddData(item, 1);
                    return;
                }
                SendAndChk(PortType.SSH, $"ftpget {PC_IP} {Indigo_FW_FileName}", keyword, out res, 0, 10 * 1000);
                if (res.Contains("No such file or directory") || res.Contains("Host is unreachable"))
                {
                    DisplayMsg(LogType.Log, "ftpget fail");
                    AddData(item, 1);
                    return;
                }

                //=======================================================================

                //  ===md5sum indigo-sh4a-r2.15.3-R-924755-PROD-1_standard_loader_update.itb 
                //=======================================================================

                //check md5sum
                Match m;
                var pattern = @"(?<md5sum>[a-zA-Z0-9]{32})\s+" + Indigo_FW_FileName;
                bool MD5_result = false;
                string MD5_inDUT = "";
                string MD5_inPC = CalculateMD5ofFile(Path.Combine(Application.StartupPath, Indigo_FW_FileName));
                DisplayMsg(LogType.Log, $"MD5_inPC: {MD5_inPC}");
                while (index++ < 5)
                {
                    SendAndChk(PortType.SSH, $"md5sum {Indigo_FW_FileName}", keyword, out res, 0, 5000);
                    m = Regex.Match(res, pattern);
                    if (m.Success)
                    {
                        MD5_inDUT = m.Groups["md5sum"].Value.Trim().ToUpper();
                        DisplayMsg(LogType.Log, $"MD5_inDUT: {MD5_inDUT}");
                    }

                    if (string.Compare(MD5_inPC, MD5_inDUT) == 0)
                    {
                        MD5_result = true;
                        DisplayMsg(LogType.Log, "MD5 check pass");
                        break;
                    }
                    else
                    {
                        DisplayMsg(LogType.Log, "MD5 check fail");
                        Thread.Sleep(1000);
                    }
                }
                if (!MD5_result)
                {
                    DisplayMsg(LogType.Log, "MD5 check fail");
                    AddData(item, 1);
                    return;
                }

                //Rena_20230718, machid不對會造成sysupgrade fail
                SendAndChk(PortType.SSH, "fw_setenv machid 8051404", keyword, out res, 0, 3000);
                SendAndChk(PortType.SSH, "fw_printenv", keyword, out res, 0, 3000);
                if (!res.Contains("machid=8051404"))
                {
                    DisplayMsg(LogType.Log, "Check machid=8051404 fail");
                    AddData(item, 1);
                    return;
                }

                //Sysupgrade to indigo
                //開始更新後SSH就會斷線,更新到indigo FW後改ping 192.168.1.254
                //SendCommand(PortType.SSH, $"sysupgrade -v {Indigo_FW_FileName}", 0);
                // =============================================================================
                SendAndChk(PortType.SSH, $"sysupgrade -T {Indigo_FW_FileName}", "success", keyword, out res, 0, 10 * 1000);
                // =============================================================================

                SendAndChk(PortType.SSH, $"sysupgrade -v {Indigo_FW_FileName}", "", keyword, out res, 0, 240 * 1000);
                // -----------------------------------------------------------------
                // sysupgrade will take almost 4 minute
                // -----------------------------------------------------------------
                //Rena_20230718 add
                if (res.Contains("sh40j-119746-2328000065"))
                {
                    DisplayMsg(LogType.Log, "Image check failed, sysupgrade to indigo FW failed");
                    AddData(item, 1);
                    return;
                }

                //ping 192.168.1.254確認是否已更新到indigo FW
                if (telnet.Ping(Indigo_IP, PingTimeoutSec * 1000))
                {
                    DisplayMsg(LogType.Log, "sysupgrade to indigo FW successfully");
                    AddData(item, 0);
                }
                else
                {
                    DisplayMsg(LogType.Log, "sysupgrade to indigo FW failed");
                    AddData(item, 1);
                }

            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData(item, 1);
            }
        }
        private void VerifyBoardData()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            bool result = false;
            string item = "VerifyBoardData";
            string keyword = "root@OpenWrt";
            string res = "";
            string fw_ver = "";
            string hw_ver = "";
            string base_mac = "";
            string wifi_ssid = "";
            string wifi_pwd = "";

            DisplayMsg(LogType.Log, "=============== Verify MFG Board data ===============");

            try
            {
                //connect to golden ssh
                GoldenSshParameter();

                if (!ChkInitial(PortType.GOLDEN_SSH, keyword, 120 * 1000))
                {
                    DisplayMsg(LogType.Log, "Golden SSH fail");
                    AddData("GoldenSSH", 1);
                    return;
                }

                //check if golden mode
                SendAndChk(PortType.GOLDEN_SSH, "fw_printenv", keyword, out res, 0, 3000);
                if (!res.Contains("checkcust=1"))
                {
                    SendAndChk(PortType.GOLDEN_SSH, "fw_setenv checkcust 1", keyword, out res, 0, 3000);
                    SendAndChk(PortType.GOLDEN_SSH, "reboot", keyword, out res, 0, 3000);
                    DisplayMsg(LogType.Log, "Delay 10s");
                    Thread.Sleep(10 * 1000);
                    if (!ChkInitial(PortType.GOLDEN_SSH, keyword, 120 * 1000))
                    {
                        DisplayMsg(LogType.Log, "Golden SSH fail");
                        AddData("GoldenSSH", 1);
                        return;
                    }
                }

                SendAndChk(PortType.GOLDEN_SSH, "cd /wnc/build/usp", "root@OpenWrt:/wnc/build/usp#", out res, 0, 3000);

                MessageBox.Show("golden need to setup, plz refer to 14.3.2 Verify MFG Board Data");
                // python3 ./operate.py -m ws -u admin -P { admin_password } -s {serial_number} factory.tx
                //一開始都會fail,所以增加retry
                //check board data
                for (int i = 0; i < 20; i++)
                {
                    SendAndChk(PortType.GOLDEN_SSH, $"python3 ./get.py -m manufacturer -a ../certs/mqtt.indigo.cert.pem -k ../certs/manufacturer-wnc.key.pem -f ../certs/manufacturer-wnc.cert.pem -s {infor.SerialNumber} basic.txt", "root@OpenWrt:/wnc/build/usp# \r\n", out res, 0, 20 * 1000);
                    if (res.Contains("Device.DeviceInfo.SoftwareVersion"))
                    {
                        result = true;
                        break;
                    }
                    else
                    {
                        DisplayMsg(LogType.Log, $"Get board data fail, delay 2s and then retry...");
                        Thread.Sleep(2000);
                    }
                }

                if (!result)
                {
                    AddData(item, 1);
                    return;
                }

                Match m = Regex.Match(res, "Device.DeviceInfo.SoftwareVersion => (?<fw_ver>.+)");
                if (m.Success)
                {
                    fw_ver = m.Groups["fw_ver"].Value.Trim();
                }
                m = Regex.Match(res, "Device.DeviceInfo.HardwareVersion => (?<hw_ver>.+)");
                if (m.Success)
                {
                    hw_ver = m.Groups["hw_ver"].Value.Trim();
                }
                m = Regex.Match(res, "Device.DeviceInfo.X_BT-COM_BaseMACAddress => (?<base_mac>.+)");
                if (m.Success)
                {
                    base_mac = m.Groups["base_mac"].Value.Trim();
                    // base_mac = base_mac.Trim().Replace(":", "");
                }
                m = Regex.Match(res, "Device.WiFi.SSID.1.SSID => (?<wifi_ssid>.+)");
                if (m.Success)
                {
                    wifi_ssid = m.Groups["wifi_ssid"].Value.Trim();
                }

                SendAndChk(PortType.GOLDEN_SSH, $"python3 ./get.py -m ws -u admin -P {infor.Admin_PWD} -s {infor.SerialNumber} admin.txt", "root@OpenWrt:/wnc/build/usp# \r\n", out res, 0, 20 * 1000);
                m = Regex.Match(res, "Device.WiFi.AccessPoint.1.Security.X_BT-COM_KeyPassphrase => (?<wifi_pwd>.+)");
                if (m.Success)
                {
                    wifi_pwd = m.Groups["wifi_pwd"].Value.Trim();
                }

                DisplayMsg(LogType.Log, $"Spec fw_ver: {infor.FWver_Cust}");
                DisplayMsg(LogType.Log, $"Spec hw_ver: {infor.HWver_Cust}");
                DisplayMsg(LogType.Log, $"Spec base_mac: {infor.BaseMAC}");
                DisplayMsg(LogType.Log, $"Spec wifi_ssid: {infor.WiFi_SSID}");
                DisplayMsg(LogType.Log, $"Spec wifi_pwd: {infor.WiFi_PWD}");
                DisplayMsg(LogType.Log, $"fw_ver: {fw_ver}");
                DisplayMsg(LogType.Log, $"hw_ver: {hw_ver}");
                DisplayMsg(LogType.Log, $"base_mac: {base_mac}");
                DisplayMsg(LogType.Log, $"wifi_ssid: {wifi_ssid}");
                DisplayMsg(LogType.Log, $"wifi_pwd: {wifi_pwd}");
                if (string.Compare(fw_ver, infor.FWver_Cust) != 0 || string.Compare(hw_ver, infor.HWver_Cust) != 0 || string.Compare(base_mac, infor.BaseMAC) != 0 ||
                    string.Compare(wifi_ssid, infor.WiFi_SSID) != 0 || string.Compare(wifi_pwd, infor.WiFi_PWD) != 0)
                {
                    DisplayMsg(LogType.Log, "Check board data fail");
                    AddData(item, 1);
                }
                else
                {
                    DisplayMsg(LogType.Log, "Check board data pass");
                    AddData(item, 0);

                    //upload data to SFCS
                    status_ATS.AddDataRaw("LRG1_FWver_Cust", infor.FWver_Cust, infor.FWver_Cust, "000000");
                    status_ATS.AddDataRaw("LRG1_HWver_Cust", infor.HWver_Cust, infor.HWver_Cust, "000000");
                    // ===================== test plan 14.3.2 =================================
                    if (!SendAndChk(PortType.GOLDEN_SSH, $"python3 ./operate.py -m ws -u admin -P {infor.Admin_PWD} -s {infor.SerialNumber} factory.txt", "root@OpenWrt:/wnc/build/usp#", out res, 0, 20 * 1000))
                    {
                        DisplayMsg(LogType.Log, "factory_Reset_NG");
                        AddData(item, 1);
                        return;
                    }
                    // ===================== test plan 14.3.2 =================================
                    /*status_ATS.AddDataRaw("LRG1_BASE_MAC", infor.BaseMAC, infor.BaseMAC, "000000");
                    status_ATS.AddDataRaw("LRG1_WiFi_SSID", infor.WiFi_SSID, infor.WiFi_SSID, "000000");
                    status_ATS.AddDataRaw("LRG1_WiFi_PWD", infor.WiFi_PWD, infor.WiFi_PWD, "000000");*/
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData(item, 1);
            }
        }
        private void OpenFTPdmin()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            try
            {
                string filepath = Path.Combine(Application.StartupPath, "ftpdmin.exe");
                if (!File.Exists(filepath))
                {
                    status_ATS.AddLog(filepath + " doesn't exist!!");
                    AddData("OpenFTPdmin", 1);
                    return;
                }

                KillTaskProcess("ftpdmin");
                Thread.Sleep(1000);
                Process process;
                process = new Process();
                process.StartInfo.WorkingDirectory = Application.StartupPath;
                process.StartInfo.FileName = "ftpdmin.exe";
                process.StartInfo.Arguments = "."; //Using Application.StartupPath as root directory
                process.StartInfo.CreateNoWindow = false;
                status_ATS.AddLog("Start ftpdmin.exe...");
                process.Start();
                Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData("OpenFTPdmin", 1);
            }
        }
    }
}
