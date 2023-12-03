using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using WNC.API;
namespace MiniPwrSupply.LMG1
{
    public partial class FWswitch
    {
        //FW switch from WNC to Indigo via ethernet
        private void FWSwitch()
        {
            try
            {
                infor.ResetParam();
                #region create SMT file
                if (status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode)
                {

                    SFCS_Query _sfcsQuery = new SFCS_Query();
                    ATS_Template.SFCS_ATS_2_0.ATS ss = new ATS_Template.SFCS_ATS_2_0.ATS();
                    bool combine = false;
                    int snLength = Convert.ToInt32(Func.ReadINI("Setting", "Match", "SN_Length", "11"));
                    string snStartwith = Func.ReadINI("Setting", "Match", "SN_Start", "T");
                    GetFromSfcs("@LRG1_SN", out infor.SerialNumber);
                    GetFromSfcs("@MAC", out infor.BaseMAC);
                    //SE_TODO: get infor from SFCS
                    //GetFromSfcs("@LRG1_SN", out infor.SerialNumber); // Jason add get from SFCS
                    DisplayMsg(LogType.Log, "Get SN From SFCS is:" + infor.SerialNumber);
                    #region Check SN base on format
                    if (infor.SerialNumber.Length == 18)
                    {
                        bool SN_check = CheckSN(infor.SerialNumber);
                        if (SN_check)
                        {
                            SetTextBox(status_ATS.txtPSN, infor.SerialNumber);
                            status_ATS.SFCS_Data.PSN = infor.SerialNumber;
                            status_ATS.SFCS_Data.First_Line = infor.SerialNumber;

                            int count = 0;
                        Recombine:
                            CreatePsnFile();
                            if (!ChkCombine())
                            {
                                if (count < 3)
                                {
                                    count++;
                                    goto Recombine;
                                }
                                warning = "Combine fail";
                                return;
                            }
                        }
                        else { warning = "SN Check Not Meet target"; return; }

                    }
                    else { warning = "Get SN from SFCS fail"; return; }

                    #endregion Check SN base on format

                    #region Get Data from SFCS & Comapre with setting

                    /*                    string CustFW_FW_boardVer = Func.ReadINI("Setting", "FWSwitch", "CustFW_FW_boardVer", "");
                                        CustFW_FW_boardVer=char.ToUpper(CustFW_FW_boardVer[0]) + CustFW_FW_boardVer.Substring(1);
                                        string SFCS_CustFW = "";*/

                    string HW_VERSION_FOR_BOARD_CPN = Func.ReadINI("Setting", "FirehoseFW", "HWver_for_Board", "");

                    string[] Inform_infor = new string[] { "HWver_for_Board" };

                    //MP
                    //string[] CPN = new string[] { "@HW_VERSION_BOARD_12" };
                    //EPR
                    string partNumber = string.Empty;
                    partNumber = GetPartNumber(status_ATS.SFCS_Data.PSN);
                    DisplayMsg(LogType.Log, "partNumber is:" + partNumber);
                    string[] CPN = new string[] { "@HW_VERSION_BOARD_12" };


                    if (partNumber == "57.LMG11.003")
                    {
                        CPN = new string[] { "@HW_VERSION_BOARD_12" };
                    }
                    if (partNumber == "57.LMG11.002")
                    {
                        CPN = new string[] { "@HW_VERSION_FOR_BOARD" };
                    }

                    Compare_SFCS_Setting(Inform_infor, "FWSwitch", CPN);


                    GetFromSfcs("@INDIGO_SW_VER", out infor.FWver_Cust);
                    DisplayMsg(LogType.Log, "Get SFCS_CustFW From SFCS is:" + infor.FWver_Cust);

                    string CustFW_FW_boardVer = Func.ReadINI("Setting", "FWSwitch", "CustFW_FW_boardVer", "");
                    CustFW_FW_boardVer = CustFW_FW_boardVer.ToUpper();
                    if (CustFW_FW_boardVer.Contains(infor.FWver_Cust))
                    {
                        DisplayMsg(LogType.Log, "Get SFCS_CustFW between setting and SFCS PASS");
                        DisplayMsg(LogType.Log, "SFCS_CustFW From SFCS is:" + infor.FWver_Cust);
                        DisplayMsg(LogType.Log, "SFCS_CustFW From setting is:" + CustFW_FW_boardVer);
                    }
                    else
                    {
                        warning = $"Get SFCS_CustFW between setting and SFCS Fail!";
                        DisplayMsg(LogType.Log, "SFCS_CustFW From SFCS is:" + infor.FWver_Cust);
                        DisplayMsg(LogType.Log, "SFCS_CustFW From setting is:" + CustFW_FW_boardVer);

                        return;
                    }

                    /*                    string pattern = @"^.{6}"; // Lấy 6 ký tự đầu tiên từ chuỗi
                                        Match match = Regex.Match(infor.FWver_Cust, pattern);
                                        if (match.Success)
                                        {
                                            infor.FWver_Cust = match.Value;
                                            if (CustFW_FW_boardVer.Contains(match.Value))
                                            {
                                                DisplayMsg(LogType.Log, $"Setting CustFW processed is: {CustFW_FW_boardVer}");
                                                DisplayMsg(LogType.Log, $"SFCS CustFW is: {infor.FWver_Cust}");
                                                DisplayMsg(LogType.Log, $"Compare CustFW between SFCS & Setting file Pass");
                                            }
                                            else
                                            {
                                                DisplayMsg(LogType.Log, $"Setting CustFW processed is: {CustFW_FW_boardVer}");
                                                DisplayMsg(LogType.Log, $"SFCS CustFW is: {infor.FWver_Cust}");
                                                DisplayMsg(LogType.Log, $"Compare CustFW between SFCS & Setting file fail");
                                            }
                                        }
                    */
                    DisplayMsg(LogType.Log, "Get Base MAC From SFCS is:" + infor.BaseMAC);
                    DisplayMsg(LogType.Log, "Get HWver From SFCS is:" + infor.HWver_for_Board);
                    infor.BaseMAC = MACConvert(infor.BaseMAC);
                    DisplayMsg(LogType.Log, "Base MAC Convert" + infor.BaseMAC);
                    DisplayMsg(LogType.Log, "Get SFCS_CustFW From SFCS is:" + infor.FWver_Cust);
                    #endregion Get Data from SFCS & Comapre with setting

                }
                else
                {
                    //Rena_20230407 add for HQ test
                    GetBoardDataFromExcel1();
                    infor.HWver_for_Board = Func.ReadINI("Setting", "FWSwitch", "HWver_for_Board", "EVT1");
                    infor.FWver_Cust = Func.ReadINI("Setting", "FWSwitch", "FWver_Cust", "XXXXXXXX");
                    infor.HWver_Cust = Func.ReadINI("Setting", "FWSwitch", "HWver_Cust", "FUT");
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
                    //frmOK.Label = "確認'網路線'接到黃色lan port,並用網路線對接DUT與陪測物\r\n請將DUT與陪測物上電並按下power button開機";
                    frmOK.Label = "Xác nhận rằng dây mạng đã được kết nối vào cổng LAN màu vàng.\r\nVui lòng bật nguồn lên và nhấn nút nguồn để khởi động";
                    frmOK.ShowDialog();
                }
                DisplayMsg(LogType.Log, "Power on!!!");

                //考慮到重測流程,如果已更新到Customer FW就直接執行VerifyBoardData
                //ping MFG FW(192.168.1.1) fail -> ping Customer FW(192.168.1.254) pass -> skip upgrade, do VerifyBoardData
                //string CustFW_IP = Func.ReadINI("Setting", "IP", "CustFW", "192.168.1.254");
                //if (!telnet.Ping(sshInfo.ip, 180 * 1000))
                //{
                //    DisplayMsg(LogType.Log, $"Ping {sshInfo.ip} fail, start ping {CustFW_IP}...");
                //    if (telnet.Ping(CustFW_IP, 10 * 1000))
                //    {
                //        DisplayMsg(LogType.Log, "Already is Indigo FW, skip FW upgrade process");
                //        goto VerifyBoardData;
                //    }
                //    else
                //    {
                //        DisplayMsg(LogType.Log, $"Ping {sshInfo.ip} and {CustFW_IP} failed");
                //        AddData("BootUp", 1);
                //        return;
                //    }
                //}
                ChkBootUp(PortType.SSH);
                this.UpgradeCustFW_EPR2_3(); // build>=EPR2-3
                //this.UpgradeCustFW_new(); //(build<=EPR2-2)

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
        private void UpgradeCustFW()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            int index = 0;
            string item = "UpgradeCustFW";
            string keyword = "root@OpenWrt:~# \r\n";
            string CustFW_keyword = "root@iopsys";
            string res = "";
            string hw_ver = "";
            string CustFW_Ver = "";
            string PC_IP = Func.ReadINI("Setting", "IP", "PC", "192.168.1.2");
            string CustFW_IP = Func.ReadINI("Setting", "IP", "CustFW", "192.168.1.254");
            int PingTimeoutSec = Convert.ToInt32(Func.ReadINI("Setting", "FWSwitch", "PingTimeoutSec", "300"));
            string CustFW_FW_FileName = Func.ReadINI("Setting", "FWSwitch", "CustFW_FW_FileName", "certutil -hashfile indigo-sw4a-r2.15.3-R-924755_standard_loader_update.itb");
            DisplayMsg(LogType.Log, "=============== Upgrade to Customer FW ===============");

            try
            {
                OpenFTPdmin();

                if (!CheckGoNoGo())
                {
                    return;
                }

                //Check hardware version
                //MFG FW v0.1.2.6 only supports version 0/1/2
                SendAndChk(PortType.SSH, "cat /tmp/sysinfo/hardware_version", keyword, out res, 0, 5000);
                Match m = Regex.Match(res, @"hardware_version[\r\n]+(?<HWver>.+)");
                if (m.Success)
                {
                    hw_ver = m.Groups["HWver"].Value.Trim();
                }
                DisplayMsg(LogType.Log, $"hw_ver : {hw_ver}");
                if (hw_ver == "0" || hw_ver == "1" || hw_ver == "2")
                {
                    DisplayMsg(LogType.Log, "Check hardware version pass");
                }
                else
                {
                    DisplayMsg(LogType.Log, "Check hardware version fail");
                    AddData(item, 1);
                    return;
                }

                //download Customer FW
                if (!File.Exists(Path.Combine(Application.StartupPath, CustFW_FW_FileName)))
                {
                    DisplayMsg(LogType.Log, Path.Combine(Application.StartupPath, CustFW_FW_FileName) + " doesn't exist!!");
                    AddData(item, 1);
                    return;
                }
                SendAndChk(PortType.SSH, $"ftpget {PC_IP} {CustFW_FW_FileName}", keyword, out res, 0, 10 * 1000);
                if (res.Contains("No such file or directory") || res.Contains("Host is unreachable"))
                {
                    DisplayMsg(LogType.Log, "Download customer FW fail");
                    AddData(item, 1);
                    return;
                }

                //check md5sum
                var pattern = @"(?<md5sum>[a-zA-Z0-9]{32})\s+" + CustFW_FW_FileName;
                bool MD5_result = false;
                string MD5_inDUT = "";
                string MD5_inPC = CalculateMD5ofFile(Path.Combine(Application.StartupPath, CustFW_FW_FileName));
                DisplayMsg(LogType.Log, $"MD5_inPC: {MD5_inPC}");
                while (index++ < 5)
                {
                    SendAndChk(PortType.SSH, $"md5sum {CustFW_FW_FileName}", keyword, out res, 0, 5000);
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

                ////Rena_20230718, machid不對會造成sysupgrade fail
                //SendAndChk(PortType.SSH, "fw_setenv machid 8051404", keyword, out res, 0, 3000);
                //SendAndChk(PortType.SSH, "fw_printenv", keyword, out res, 0, 3000);
                //if (!res.Contains("machid=8051404"))
                //{
                //    DisplayMsg(LogType.Log, "Check machid=8051404 fail");
                //    AddData(item, 1);
                //    return;
                //}

                //Sysupgrade to indigo
                //開始更新後SSH就會斷線,更新到Customer FW後改ping 192.168.1.254
                SendAndChk(PortType.SSH, $"sysupgrade --target=iopsys -n -p -v {CustFW_FW_FileName}", "Commencing upgrade. Closing all shell sessions", keyword, out res, 0, 10 * 1000);
                if (res.Contains("Image check failed"))
                {
                    DisplayMsg(LogType.Log, "Image check failed, sysupgrade to indigo FW failed");
                    AddData(item, 1);
                    return;
                }

                //Rena_20230808, sysupgrade不會馬上斷ping,所以先delay避免誤判
                DisplayMsg(LogType.Log, "Delay 70s...");
                Thread.Sleep(70 * 1000);

                //ping CustFW_IP確認是否已更新到Customer FW
                if (telnet.Ping(CustFW_IP, PingTimeoutSec * 1000))
                {
                    DisplayMsg(LogType.Log, "sysupgrade to Customer FW successfully");
                    //AddData(item, 0);
                }
                else
                {
                    DisplayMsg(LogType.Log, "sysupgrade to Customer FW failed");
                    AddData(item, 1);
                    return;
                }

                #region check Customer FW Version
                GoldenSshParameter();

                for (int i = 0; i < 3; i++)
                {
                    if (golden_SSH_client != null && golden_SSH_client.IsConnected)
                        golden_SSH_client.Disconnect();

                    golden_SSH_client.Connect();
                    if (golden_SSH_client.IsConnected)
                    {
                        golden_SSH_stream = golden_SSH_client.CreateShellStream("test", 0, 0, 0, 0, 1000000);
                        if (SendAndChk(PortType.GOLDEN_SSH, "", CustFW_keyword, out res, 0, 10000))
                        {
                            break;
                        }
                    }
                    else
                    {
                        DisplayMsg(LogType.Log, "Customer FW SSH connect fail, retry...");
                        Thread.Sleep(1000);
                    }
                }

                //IOWRT Version: 7.3.0alpha1-18-g6c06c46281
                m = Regex.Match(res, "IOWRT Version:(?<CustFW_Ver>.+)");
                if (m.Success)
                {
                    CustFW_Ver = m.Groups["CustFW_Ver"].Value.Trim();
                }

                DisplayMsg(LogType.Log, $"SFCS CustFW_Ver: {infor.FWver_Cust}");
                DisplayMsg(LogType.Log, $"CustFW_Ver: {CustFW_Ver}");

                if (string.Compare(CustFW_Ver, infor.FWver_Cust) != 0)
                {
                    DisplayMsg(LogType.Log, "Check Customer FW Version fail");
                    AddData(item, 1);
                }
                else
                {
                    DisplayMsg(LogType.Log, "Check Customer FW Version pass");
                    AddData(item, 0);
                    status_ATS.AddDataRaw("LMG1_FWver_Cust", infor.FWver_Cust, infor.FWver_Cust, "000000");
                }
                #endregion
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData(item, 1);
            }
        }
        private void UpgradeCustFW_new()    //12.3.3  Switch FW to INDIGO from WNC MFG by UART (build<=EPR2-2)
        {
            if (!CheckGoNoGo())
            {
                return;
            }
            int retry_cnt = 3;
            int index = 0;
            string item = "UpgradeCustFW";
            string item1 = "Verify_Board_Data";
            string keyword = "root@OpenWrt:~# \r\n";
            string keyword2 = "IPQ5332#";
            string CustFW_keyword = "root@iopsys";
            string res = "";
            string hw_ver = "";
            string CustFW_Ver = "";
            string PC_IP = Func.ReadINI("Setting", "IP", "PC", "192.168.1.2");
            string CustFW_IP = Func.ReadINI("Setting", "IP", "CustFW", "192.168.1.254");
            string CustFW_FW_FileName = Func.ReadINI("Setting", "FWSwitch", "CustFW_FW_FileName", "");
            string CustFW_FW_boardVer = Func.ReadINI("Setting", "FWSwitch", "CustFW_FW_boardVer", "");
            CustFW_FW_boardVer = CustFW_FW_boardVer.ToLower();
            int PingTimeoutSec = Convert.ToInt32(Func.ReadINI("Setting", "FWSwitch", "PingTimeoutSec", "300"));

            DisplayMsg(LogType.Log, "=============== Upgrade to Customer FW ===============");
            try
            {
                OpenTftpd32(Application.StartupPath);
                if (!CheckGoNoGo())
                {
                    return;
                }
                //Clear overlay and then reboot
                SendAndChk(PortType.UART, "dd if=/dev/zero of=/dev/mmcblk0p26", keyword, out res, 0, 10000);
                SendAndChk(PortType.UART, "dd if=/dev/zero of=/dev/mmcblk0p27", keyword, out res, 0, 10000);
                SendAndChk(PortType.UART, "reboot", "", out res, 0, 10000);
            Enter_bootloader:
                if (!BootLoader(uart))
                {
                    DisplayMsg(LogType.Log, "Enter bootloader fail");
                    if (retry_cnt-- > 0)
                    {
                        DisplayMsg(LogType.Log, "Reboot DUT and retry...");
                        frmOK.Label = "Vui lòng trước tiên nhấn nút \"OK\", sau đó khởi động lại thiết bị (DUT)";
                        frmOK.ShowDialog();
                        goto Enter_bootloader;
                    }
                    else
                    {
                        AddData(item, 1);
                        return;
                    }
                }
                //download Customer FW
                if (!File.Exists(Path.Combine(Application.StartupPath, CustFW_FW_FileName)))
                {
                    DisplayMsg(LogType.Log, Path.Combine(Application.StartupPath, CustFW_FW_FileName) + " doesn't exist!!");
                    AddData(item, 1);
                    return;
                }
                SendAndChk(PortType.UART, $"tftpboot 0x44000000 {CustFW_FW_FileName} && setenv machid 5020102 && imgaddr=0x44000000 && source $imgaddr:script", keyword2, out res, 0, 50 * 1000);
                if (res.Contains("No such file or directory") || res.Contains("Host is unreachable"))
                {
                    DisplayMsg(LogType.Log, "Download customer FW fail");
                    AddData(item, 1);
                    return;
                }
                //check [ done ]
                int done_cnt = Regex.Matches(res, @"\[ done \]").Count;
                DisplayMsg(LogType.Log, $"[ done ] count: {done_cnt}");
                if (done_cnt < 14)
                {
                    DisplayMsg(LogType.Log, "Check [ done ] count fail");
                    AddData(item, 1);
                    return;
                }
                //reboot device
                if (!SendAndChk(PortType.UART, "reset", "resetting", 0, 10000))
                {
                    DisplayMsg(LogType.Log, "Reboot device fail");
                    AddData(item, 1);
                    return;
                }
                retry_cnt = 3;
            Enter_bootloader2:
                if (!BootLoader(uart))
                {
                    DisplayMsg(LogType.Log, "Enter bootloader fail");
                    if (retry_cnt-- > 0)
                    {
                        DisplayMsg(LogType.Log, "Reboot DUT and retry...");
                        frmOK.Label = "Vui lòng trước tiên nhấn nút \"OK\", sau đó khởi động lại thiết bị (DUT)";
                        frmOK.ShowDialog();
                        goto Enter_bootloader2;
                    }
                    else
                    {
                        AddData(item, 1);
                        return;
                    }
                }
                SendCommand(PortType.UART, "env default -a;setenv board_variant bt,sw40j-p-l1;env save;reset", 0);
                if (ChkResponse(PortType.UART, ITEM.NONE, "Please press Enter to activate this console", out res, 200000))
                {
                    SendAndChk(PortType.UART, "\r\n", "login:", out res, 0, 10 * 5000);
                    SendAndChk(PortType.UART, "root", "Password:", out res, 0, 10 * 5000);
                    SendAndChk(PortType.UART, "5hut/ra1n.dr0prun@h0m3", "~#", out res, 0, 10 * 5000);
                }
                else
                {
                    AddData(item, 1);
                    return;
                }
                if (!CheckGoNoGo())
                {
                    AddData(item, 1);
                    return;
                }

                SendAndChk(PortType.UART, $"dmesg -n1", "~#", out res, 0, 10 * 1000);
                SendAndChk(PortType.UART, $"cat /etc/board-db/version/iop_version", "~#", out res, 0, 10 * 1000);
                if (!res.Contains(infor.FWver_Cust))
                {
                    DisplayMsg(LogType.Log, "Check board-db version from DUT with setting fail");
                    DisplayMsg(LogType.Log, $"Board-db version from SFCS is: '{infor.FWver_Cust}");
                    AddData(item, 1);
                }
                else
                {
                    DisplayMsg(LogType.Log, $"Check board-db version '{infor.FWver_Cust}' from DUT with SFCS pass");
                    AddData(item, 0);
                }
                SendAndChk(PortType.UART, $"cat /defaults/board_data", "~#", out res, 0, 10 * 1000);
                if (!res.Contains($"serial_number={infor.SerialNumber}"))
                {
                    DisplayMsg(LogType.Log, "Check serial_number fail");
                    AddData(item1, 1);
                }
                else
                {
                    DisplayMsg(LogType.Log, $"Check serial_number '{infor.SerialNumber}' pass");
                }
                //hw_ver=EVT1
                if (!res.Contains($"hardware_version={infor.HWver_for_Board}"))
                //if (!res.Contains($"hw_ver={infor.HWver_for_Board}"))
                {
                    DisplayMsg(LogType.Log, "Check hw_ver fail");
                    DisplayMsg(LogType.Log, $"Hardware version from setting is: '{infor.HWver_for_Board}");
                    AddData(item1, 1);
                }
                else
                {
                    DisplayMsg(LogType.Log, $"Check hardware_version '{infor.HWver_for_Board}' with setting pass");
                }
                //mac_base=E8:C7:CF:AF:46:28
                if (!res.Contains($"mac_base={infor.BaseMAC}"))
                {
                    DisplayMsg(LogType.Log, "Check mac_base fail");
                    AddData(item1, 1);
                }
                else
                {
                    DisplayMsg(LogType.Log, $"Check mac_base '{infor.BaseMAC}' pass");
                }
                //以下為固定值確認
                if (!res.Contains("device_category=COM") || !res.Contains("manufacturer=BT") || !res.Contains("wifi_country_revision=0") ||
                    !res.Contains("manufacturer_oui=0000DB") || !res.Contains("model_name=\"Smart WiFi SW40J\"") || !res.Contains("model_number=SW40J") ||
                    !res.Contains("description=\"Smart WiFi SW40J\"") || !res.Contains("product_class=SW4-1") || !res.Contains("mac_count=7") ||
                    !res.Contains("item_code=119747") || !res.Contains("brand_variant=Consumer") || !res.Contains("wifi_country_code=GB"))
                {
                    DisplayMsg(LogType.Log, "Check board data fail");
                    AddData(item1, 1);
                }
                status_ATS.AddDataRaw("LMG1_Label_INDIGO_SW_VER", infor.FWver_Cust, infor.FWver_Cust, "000000");
                if (!CheckGoNoGo())
                {
                    AddData(item1, 1);
                    return;
                }
                //Cho thong tin SFCS PASS thi bo Skip
                //status_ATS.AddDataRaw("LMG1_Label_INDIGO_SW_VERSION", infor.FWver_Cust, infor.FWver_Cust, "000000");
                AddData(item1, 0);
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
                DisplayMsg(LogType.Log, @"constrain LMG1 IP address");
                SendAndChk(PortType.GOLDEN_SSH, "uci set dhcp.lan.limit='1'", keyword, out res, 0, 3000);
                SendAndChk(PortType.GOLDEN_SSH, "uci set dhcp.lan.start='200'", keyword, out res, 0, 3000);
                SendAndChk(PortType.GOLDEN_SSH, "/etc/init.d/dnsmasq restart", keyword, out res, 0, 3000);
                SendAndChk(PortType.GOLDEN_SSH, "cd /wnc/build/usp", "root@OpenWrt:/wnc/build/usp#", out res, 0, 3000);

                //一開始都會fail,所以增加retry
                //check board data
                for (int i = 0; i < 20; i++)
                {
                    SendAndChk(PortType.GOLDEN_SSH, $"python3 ./get.py -m manufacturer -a ../certs/mqtt.indigo.cert.pem -k ../certs/manufacturer-wnc.key.pem -f ../certs/manufacturer-wnc.cert.pem -s +119747+{infor.SerialNumber} basic.txt --ipaddr 192.168.1.200", "root@OpenWrt:/wnc/build/usp# \r\n", out res, 0, 20 * 1000);
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
                }
                m = Regex.Match(res, "Device.WiFi.SSID.1.SSID => (?<wifi_ssid>.+)");
                if (m.Success)
                {
                    wifi_ssid = m.Groups["wifi_ssid"].Value.Trim();
                }

                //SendAndChk(PortType.GOLDEN_SSH, $"python3 ./get.py -m ws -u admin -P {infor.Admin_PWD} -s {infor.SerialNumber} admin.txt", "root@OpenWrt:/wnc/build/usp# \r\n", out res, 0, 20 * 1000);
                m = Regex.Match(res, "Device.WiFi.AccessPoint.1.Security.X_BT-COM_KeyPassphrase => (?<wifi_pwd>.+)");
                if (m.Success)
                {
                    wifi_pwd = m.Groups["wifi_pwd"].Value.Trim();
                }

                DisplayMsg(LogType.Log, $"Spec fw_ver: {infor.FWver_Cust}");
                DisplayMsg(LogType.Log, $"Spec hw_ver: {infor.HWver_Cust}");
                DisplayMsg(LogType.Log, $"Spec base_mac: {infor.BaseMAC}");
                DisplayMsg(LogType.Log, $"fw_ver: {fw_ver}");
                DisplayMsg(LogType.Log, $"hw_ver: {hw_ver}");
                DisplayMsg(LogType.Log, $"base_mac: {base_mac}");

                if (string.Compare(fw_ver, infor.FWver_Cust) != 0 || string.Compare(hw_ver, infor.HWver_Cust) != 0 || string.Compare(base_mac, infor.BaseMAC) != 0)
                {
                    DisplayMsg(LogType.Log, "Check board data fail");
                    AddData(item, 1);
                }
                else
                {
                    DisplayMsg(LogType.Log, "Check board data pass");
                    DisplayMsg(LogType.Log, @"Disconnect LRG1 and LMG1, and in LRG1 SSH console, remove DHCP lease");
                    SendAndChk(PortType.GOLDEN_SSH, "rm /tmp/dhcp.leases", "root@OpenWrt:/wnc/build/usp#", out res, 0, 3000);
                    if (SendAndChk(PortType.GOLDEN_SSH, "/etc/init.d/dnsmasq restart", "root@OpenWrt:/wnc/build/usp#", out res, 0, 3000))
                    {
                        AddData(item, 0);
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
                //upload data to SFCS
                status_ATS.AddDataRaw("LRG1_FWver_Cust", infor.FWver_Cust, infor.FWver_Cust, "000000");
                status_ATS.AddDataRaw("LRG1_HWver_Cust", infor.HWver_Cust, infor.HWver_Cust, "000000");
                status_ATS.AddDataRaw("LRG1_BASE_MAC", infor.BaseMAC, infor.BaseMAC, "000000");
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
        private void SwitchFW_INDIGO(string FW_FileName) //build>=EPR2-3
        {
            if (!CheckGoNoGo())
            {
                return;
            }
            //certutil -hashfile 
            string md5sum = Func.ReadINI("Setting", "FWSwitch", "EPR23_md5sum", "6cb2a3a0f4a60af39855387dc2e238c1");

            string PC_IP = WNC.API.Func.ReadINI("Setting", "IP", "PC", "192.168.1.2");
            string res = string.Empty;
            string item = "SwitchFW_INDIGO";
            string keyword = "";
            int index = 0;
            bool isMD5sum_OK = false;
            try
            {
                DisplayMsg(LogType.Log, $"=================== {item} ===================");
                if (!File.Exists(Path.Combine(Application.StartupPath, FW_FileName))) // md5
                {
                    DisplayMsg(LogType.Log, Path.Combine(Application.StartupPath, FW_FileName) + " doesn't exist!!");
                    AddData(item, 1);
                    return;
                }
                SendAndChk(PortType.SSH, $"certutil -hashfile {FW_FileName} md5", "", out res, 0, 5000);
                if (!res.Contains(md5sum)) //debug
                {
                    DisplayMsg(LogType.Log, $"check {md5sum} NG");
                    AddData(item, 1);
                    return;
                }
                SendAndChk(PortType.SSH, $"ftpget {PC_IP} {FW_FileName}", keyword, out res, 0, 10 * 1000);
                if (res.Contains("No such file or directory") || res.Contains("Host is unreachable"))
                {
                    DisplayMsg(LogType.Log, "Download customer FW fail");
                    AddData(item, 1);
                    return;
                }
                SendAndChk(PortType.SSH, "cd /tmp", "root@OpenWrt:/tmp#", out res, 0, 3000);
                while (index++ < 5)
                {
                    SendAndChk(PortType.SSH, $"md5sum {FW_FileName}", keyword, out res, 0, 5000);
                    if (res.Contains(md5sum)) // >>> need check it out 
                    {
                        isMD5sum_OK = true;
                        DisplayMsg(LogType.Log, "MD5 check pass");
                        break;
                    }
                    else
                    {
                        DisplayMsg(LogType.Log, "MD5 check fail");
                        Thread.Sleep(500);
                    }
                }
                if (!isMD5sum_OK)
                {
                    DisplayMsg(LogType.Log, " MD5 check fail");
                    AddData(item, 1);
                    return;
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, item + "___" + ex.Message);
                AddData(item, 1);
            }
        }
        private void UpgradeCustFW_EPR2_3()
        {
            if (!CheckGoNoGo())
            {
                return;
            }
            int retry_cnt = 3;
            int index = 0;
            string item = "UpgradeCustFW";
            string item1 = "Verify_Board_Data";
            string keyword = "root@OpenWrt:~# \r\n";
            string keyword2 = "IPQ5332#";
            string res = "";
            string PC_IP = Func.ReadINI("Setting", "IP", "PC", "192.168.1.2");
            string CustFW_IP = Func.ReadINI("Setting", "IP", "CustFW", "192.168.1.254");
            string CustFW_FW_FileName = Func.ReadINI("Setting", "FWSwitch", "CustFW_FW_FileName", "");
            string CustFW_FW_boardVer = Func.ReadINI("Setting", "FWSwitch", "CustFW_FW_boardVer", "");
            string FW_FileName = Func.ReadINI("Setting", "FWSwitch", "CustFW_FW_FileName", "indigo-sw4a-r2.15.3-R-924755_standard_loader_update.itb");
            CustFW_FW_boardVer = CustFW_FW_boardVer.ToLower();
            int PingTimeoutSec = Convert.ToInt32(Func.ReadINI("Setting", "FWSwitch", "PingTimeoutSec", "300"));
            DisplayMsg(LogType.Log, "=============== Upgrade to Customer FW ===============");
            try
            {
                OpenTftpd32(Application.StartupPath);

                if (!CheckGoNoGo())
                {
                    return;
                }
                //download Customer FW
                // ================== 12.3. Switch FW to INDIGO from WNC MFG (build>=EPR2-3) ==============
                this.SwitchFW_INDIGO(FW_FileName);
                // ============================================================================================
                retry_cnt = 3;
                SendAndChk(PortType.SSH, $"sysupgrade --target=iopsys -n -p -v {FW_FileName}", "success", keyword, out res, 0, 10 * 1000);
                DisplayMsg(LogType.Log, @"Delay 4mins for FW upgrade & reboot");
                Thread.Sleep(4 * 60 * 1000);
                if (ChkResponse(PortType.UART, ITEM.NONE, "Please press Enter to activate this console", out res, 200000))
                {
                    SendAndChk(PortType.UART, "\r\n", "login:", out res, 0, 10 * 5000);
                    SendAndChk(PortType.UART, "root", "Password:", out res, 0, 10 * 5000);
                    SendAndChk(PortType.UART, "5hut/ra1n.dr0prun@h0m3", "~#", out res, 0, 10 * 5000);
                }
                else
                {
                    AddData(item, 1);
                    return;
                }
                if (!CheckGoNoGo())
                {
                    AddData(item, 1);
                    return;
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData(item, 1);
            }
        }   //12.3.1  Switch FW to INDIGO from WNC MFG (build>=EPR2-3)
    }
}

