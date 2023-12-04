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
using System.Security.Cryptography;

namespace MiniPwrSupply.LMG1
{
    public partial class Final_station
    {
        private void Final()
        {
            try
            {
                infor.ResetParam();

                #region create SMT file
                SFCS_Query _sfcsQuery = new SFCS_Query();
                if (status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode)
                {
                    //SE_TODO: get infor from SFCS
                    //SentPsnForGetMAC(status_ATS.txtPSN.Text.Trim());

                    DisplayMsg(LogType.Log, "Delay 1s...");
                    Thread.Sleep(1000);
                    string SN_name = Func.ReadINI("Setting", "FirehoseFW", "SN", "@LRG1_SN");
                    string MAC_name = Func.ReadINI("Setting", "FirehoseFW", "BaseMAC", "@MAC");

                    ATS_Template.SFCS_ATS_2_0.ATS ss = new ATS_Template.SFCS_ATS_2_0.ATS();
                    bool combine = false;
                    int snLength = Convert.ToInt32(Func.ReadINI("Setting", "Match", "SN_Length", "11"));
                    string snStartwith = Func.ReadINI("Setting", "Match", "SN_Start", "T");
                    GetFromSfcs("@LRG1_SN", out infor.SerialNumber);
                    //                       GetFromSfcs(MAC_name, out infor.BaseMAC);
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
                    infor.BaseMAC = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@MAC");

                    //Compare SFCS and setting information
                    //thieu , "SEver", chipver, fwforboard
                    string HW_VERSION_FOR_BOARD_CPN = Func.ReadINI("Setting", "FirehoseFW", "HWver_for_Board", "");
                    string MFG_FW_CPN = Func.ReadINI("Setting", "FirehoseFW", "FWver", "");

                    string partNumber = string.Empty;
                    partNumber = GetPartNumber(status_ATS.SFCS_Data.PSN);
                    DisplayMsg(LogType.Log, "partNumber is:" + partNumber);

                    string[] Inform_infor = new string[] { "HWID", "FWver", "BLEver", "HWver_for_Board" };

                    string[] CPN = new string[] { "@HW_ID_10", "@MFG_FW", "@BLE_FW_VERSION1", "@HW_VERSION_BOARD_12" };
                    if (partNumber == "57.LMG11.003")
                    {
                        CPN = new string[] { "@HW_ID_10", "@MFG_FW_17", "@BLE_FW_VERSION1", "@HW_VERSION_BOARD_12" };
                    }
                    if (partNumber == "57.LMG11.002")
                    {
                        CPN = new string[] { "@HW_ID_10", "@MFG_FW", "@BLE_FW_VERSION1", "@HW_VERSION", "@HW_VERSION_FOR_BOARD" };
                    }
                    Compare_SFCS_Setting(Inform_infor, "Final", CPN);

                    infor.Chipver = Func.ReadINI("Setting", "Final", "Chipver", "0x4000023D");
                    infor.SEver = Func.ReadINI("Setting", "Final", "SEver", "");
                    string sfcMD5 = string.Empty;
                    _sfcsQuery.Get15Data(status_ATS.txtPSN.Text, "LMG1_CalDataMD5", ref sfcMD5);
                    infor.CalData_MD5 = sfcMD5.ToUpper();

                    DisplayMsg(LogType.Log, "Get SN From SFCS is:" + infor.SerialNumber);
                    DisplayMsg(LogType.Log, "Get Base MAC From SFCS is:" + infor.BaseMAC);
                    DisplayMsg(LogType.Log, "Get chipver From SFCS is:" + infor.Chipver);
                    DisplayMsg(LogType.Log, "Get HWID From SFCS is:" + infor.HWID);
                    DisplayMsg(LogType.Log, "Get FWVER From SFCS is:" + infor.FWver);
                    DisplayMsg(LogType.Log, "Get BLEver From SFCS is:" + infor.BLEver);
                    DisplayMsg(LogType.Log, "Get SEver From SFCS is:" + infor.SEver);
                    DisplayMsg(LogType.Log, "Get HWver_for_Board From SFCS is:" + infor.HWver_for_Board);
                    DisplayMsg(LogType.Log, "CalData_MD5 is:" + infor.CalData_MD5);
                    infor.BaseMAC = MACConvert(infor.BaseMAC);
                    DisplayMsg(LogType.Log, "Base MAC Convert" + infor.BaseMAC);

                }
                else
                {
                    //bool get_excel_data=true;    
                    //if (!get_excel_data)
                    //{
                    //#region Check SN base on format
                    //if (infor.SerialNumber.Length == 18)
                    //{
                    //    bool SN_check = CheckSN(infor.SerialNumber);
                    //    if (SN_check)
                    //    {
                    //        SetTextBox(status_ATS.txtPSN, infor.SerialNumber);
                    //        //SetTextBox(status_ATS.txtSP, infor.BaseMAC);
                    //        status_ATS.SFCS_Data.PSN = infor.SerialNumber;
                    //        status_ATS.SFCS_Data.First_Line = infor.SerialNumber;
                    //    }
                    //    else
                    //    {
                    //        DisplayMsg(LogType.Log, $"Get SN from SFCS fail: {infor.SerialNumber}");
                    //        get_excel_data = true;
                    //    }

                    //}
                    //else
                    //{
                    //    DisplayMsg(LogType.Log, $"Get SN from SFCS fail: {infor.SerialNumber}");
                    //    get_excel_data = true;

                    //}
                    //#endregion Check SN base on format
                    //infor.BaseMAC = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@MAC");
                    //if (string.IsNullOrEmpty(infor.BaseMAC))
                    //{
                    //    DisplayMsg(LogType.Log, $"Get BaseMAC from SFCS fail: {infor.BaseMAC}");
                    //    get_excel_data = true;
                    //}

                    //if (get_excel_data)
                    {
                        GetBoardDataFromExcel1();
                    }
                    infor.FWver = Func.ReadINI("Setting", "Final", "FWver", "0.2.0.5");
                    infor.HWver = Func.ReadINI("Setting", "Final", "HWver", "EVT2-3");
                    infor.HWID = Func.ReadINI("Setting", "Final", "HWID", "0002");
                    infor.BLEver = Func.ReadINI("Setting", "Final", "BLEver", "v5.0.0-b108");
                    infor.SEver = Func.ReadINI("Setting", "Final", "SEver", "00010206");
                    infor.Chipver = Func.ReadINI("Setting", "Final", "Chipver", "0x0001023D");
                    infor.CalData_MD5 = Func.ReadINI("Setting", "Final", "CalData_MD5", "");
                    _sfcsQuery.Get15Data(status_ATS.txtPSN.Text, "LMG1_CalDataMD5", ref infor.CalData_MD5);
                    infor.CalData_MD5 = infor.CalData_MD5.ToUpper();
                    DisplayMsg(LogType.Log, "CalData_MD5 is:" + infor.CalData_MD5);

                    infor.HWver_for_Board = Func.ReadINI("Setting", "Final", "HWver_for_Board", "EVT2-3");

                    infor.BaseMAC = MACConvert(infor.BaseMAC);

                    //    }
                }
                //if (!ChkStation(status_ATS.txtPSN.Text))
                //    return;

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
                    if (isLoop == 0)
                    {
                        frmOK.Label = "Xác nhận rằng đã kết nối hai dây mạng\r\nVui lòng bật nguồn và nhấn nút nguồn để khởi động";
                        frmOK.ShowDialog();
                    }

                }

                DisplayMsg(LogType.Log, "Power on!!!");

                ChkBootUp(PortType.SSH);
                if (!CheckGoNoGo()) { return; }
                CheckFWVerAndHWID();
                if (!CheckGoNoGo()) { return; }
                // ---------------------------------
                //this.DownloadFilesRequired(); // require verify by VN
                //this.DownloadAllConfigs();
                this.DownloadDisgue();
                this.DownloadDisgue2();
                this.FilesystemEncryption(false);
                // ---------------------------------
                CheckBoardData();
                if (!CheckGoNoGo()) { return; }
                //EthernetTest(false);
                if (!CheckGoNoGo()) { return; }
                CheckEthernetMAC();

                if (isLoop == 0)
                    CheckLED();

                if (!CheckGoNoGo()) { return; }
                //CheckWiFiCalData();
                CheckWiFiCalData(IsFinal_staion: true);
                if (!CheckGoNoGo()) { return; }
                CheckPCIe();
                if (!CheckGoNoGo()) { return; }
                if (isLoop == 0)
                    WPSButton();
                if (!CheckGoNoGo()) { return; }
                if (isLoop == 0)
                    ResetButton();
                if (!CheckGoNoGo()) { return; }
                CurrentSensor();

                //Rena_20230808, Do FinalCheck
                if (!CheckGoNoGo()) { return; }  //Jason add check gonogo to skipped next test item if got failure 2023/10/07
                DisplayMsg(LogType.Log, "=============== FinalCheck ===============");// old code if check gonogo ok will test check cal -> is bug. already fixed 2023/10/07

                string res = "";
                var fResult = CH_RD.Check.FinalCheck(out res, new string[] { project, infor.BaseMAC });
                DisplayMsg(LogType.Log, res);
                if (!fResult)
                {
                    AddData("FinalCheck", 1);
                }
                else
                {
                    AddData("FinalCheck", 0);
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


        private void CheckBoardData()
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
                //check Board data
                SendAndChk(PortType.SSH, "mt boarddata", keyword, out res, 0, 5000);
                //serial_number=+119746+2333000129
                if (!res.Contains($"serial_number={infor.SerialNumber}"))
                {
                    DisplayMsg(LogType.Log, $"Check serial_number: '{infor.SerialNumber}' fail");
                    AddData(item, 1);
                }
                else
                {
                    DisplayMsg(LogType.Log, $"Check serial_number '{infor.SerialNumber}' pass");
                }

                //hw_ver=EVT1
                //if (!res.Contains($"hw_ver={infor.HWver_for_Board}"))
                if (!res.Contains($"hardware_version={infor.HWver_for_Board}"))
                {
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
                    DisplayMsg(LogType.Log, "Check mac_base fail");
                    AddData(item, 1);
                }
                else
                {
                    DisplayMsg(LogType.Log, $"Check mac_base '{infor.BaseMAC}' pass");
                }

                //以下為固定值確認
                if (!res.Contains("device_category=COM") || !res.Contains("manufacturer=BT") || !res.Contains("wifi_country_revision=0") ||
                    !res.Contains("manufacturer_oui=0000DB") /*|| !res.Contains("model_name=Smart WiFi SW40J")*/ || !res.Contains("model_number=SW40J") ||
                    /*!res.Contains("description=Smart WiFi SW40J") ||*/ !res.Contains("product_class=SW4-1") || !res.Contains("mac_count=7") ||
                    !res.Contains("item_code=119747") || !res.Contains("brand_variant=Consumer") || !res.Contains("wifi_country_code=GB") ||
                    !res.Contains("model_name=\"Smart WiFi SW40J\"") || !res.Contains("description=\"Smart WiFi SW40J\""))
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
        private void CheckWiFiCalData()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            int delayMs = 0;
            int timeOutMs = 30 * 1000;
            string item = "CheckWiFiCalData";
            string keyword = "root@OpenWrt:~# \r\n"; //避免誤判到指令第一行的"root@OpenWrt:~#"
            string res = "";
            string wifi_data_backup_path = Path.Combine(Application.StartupPath, "WiFiCalData_backup");
            string PC_IP = Func.ReadINI("Setting", "IP", "PC", "192.168.1.2");
            var regex = "(.{2}):(.{2}):(.{2}):(.{2}):(.{2}):(.{2})";

            try
            {
                DisplayMsg(LogType.Log, "=============== Check WiFi Cal Data ===============");
                DisplayMsg(LogType.Log, "BaseMAC: " + infor.BaseMAC);

                OpenTftpd32(wifi_data_backup_path);

                //Backup wifi data
                //SendAndChk(PortType.SSH, "cat /dev/mmcblk0p18 > /tmp/backupwifi", keyword, out res, 0, 5000);
                SendAndChk(PortType.SSH, "cat /dev/mmcblk0p18 > /tmp/backupwifi", keyword, out res, 0, 5000);
                if (res.Contains("No such file or directory"))
                {
                    DisplayMsg(LogType.Log, "Backup wifi data fail");
                    AddData(item, 1);
                    return;
                }

                /*                SendAndChk(PortType.SSH, "uci set wireless.radio0_band0.disabled='0'", keyword, delayMs, timeOutMs);
                                SendAndChk(PortType.SSH, "uci set wireless.radio0_band1.disabled='0'", keyword, delayMs, timeOutMs);
                                SendAndChk(PortType.SSH, "uci set wireless.radio0_band2.disabled='0'", keyword, delayMs, timeOutMs);
                                SendAndChk(PortType.SSH, "uci commit", keyword, delayMs, timeOutMs);
                                SendAndChk(PortType.SSH, "wifi", keyword, delayMs, timeOutMs);*/

                //check WiFi 2.4G MAC = BaseMAC+4
                string wifi_2g_mac = MACConvert(infor.BaseMAC, 4);
                DisplayMsg(LogType.Log, "WiFi_2G_MAC: " + wifi_2g_mac);
                wifi_2g_mac = Regex.Replace(wifi_2g_mac, regex, "$2$1 $4$3 $6$5").ToLower();

                /*                SendAndChk(PortType.SSH, "ifconfig wlan0", keyword, out res, 0, 5000);
                                if (!res.Contains("wifi0"))
                                {
                                    DisplayMsg(LogType.Log, "Check WiFi 2.4G MAC fail with ifconfig wifi0");
                                    AddData(item, 1);
                                    return;
                                }*/

                SendAndChk(PortType.SSH, "hexdump -s 0x001016 -n 6 /dev/mmcblk0p18", keyword, out res, 0, 5000);
                if (!res.Contains(wifi_2g_mac))
                {
                    DisplayMsg(LogType.Log, "Check WiFi 2.4G MAC fail");
                    AddData(item, 1);
                    return;
                }

                //check WiFi 5G MAC = BaseMAC+3
                string wifi_5g_mac = MACConvert(infor.BaseMAC, 3);
                DisplayMsg(LogType.Log, "WiFi_5G_MAC: " + wifi_5g_mac);
                wifi_5g_mac = Regex.Replace(wifi_5g_mac, regex, "$2$1 $4$3 $6$5").ToLower();

                /*                SendAndChk(PortType.SSH, "ifconfig wlan1", keyword, out res, 0, 5000);
                                if (!res.Contains("wifi1"))
                                {
                                    DisplayMsg(LogType.Log, "Check WiFi 5G MAC fail with ifconfig wifi1");
                                    AddData(item, 1);
                                    return;
                                }*/
                SendAndChk(PortType.SSH, "hexdump -s 0x026810 -n 6 /dev/mmcblk0p18", keyword, out res, 0, 5000);
                if (!res.Contains(wifi_5g_mac))
                {
                    DisplayMsg(LogType.Log, "Check WiFi 5G MAC fail");
                    AddData(item, 1);
                    return;
                }

                //check WiFi 6G MAC = BaseMAC+2
                string wifi_6g_mac = MACConvert(infor.BaseMAC, 2);
                DisplayMsg(LogType.Log, "WiFi_6G_MAC: " + wifi_6g_mac);
                wifi_6g_mac = Regex.Replace(wifi_6g_mac, regex, "$2$1 $4$3 $6$5").ToLower();

                /*                SendAndChk(PortType.SSH, "ifconfig wlan2", keyword, out res, 0, 5000);
                                if (!res.Contains("wifi2"))
                                {
                                    DisplayMsg(LogType.Log, "Check WiFi 6G MAC fail ifconfig wifi2");
                                    AddData(item, 1);
                                    return;
                                }*/
                SendAndChk(PortType.SSH, "hexdump -s 0x058810 -n 6 /dev/mmcblk0p18", keyword, out res, 0, 5000);
                if (!res.Contains(wifi_6g_mac))
                {
                    DisplayMsg(LogType.Log, "Check WiFi 6G MAC fail");
                    AddData(item, 1);
                    return;
                }

                //Rena_20230809, check某個區間的cal data
                SendAndChk(PortType.SSH, "hexdump -s 0x1000 -n 16 /dev/mmcblk0p18", keyword, out res, 0, 5000);
                if (res.Contains("0001000 0000 0000 0000 0000 0000 0000 0000 0000"))
                {
                    DisplayMsg(LogType.Log, "Check cal data fail, hexdump 0x1000 is empty");
                    AddData(item, 1);
                    return;
                }

                //Rena_20230809, check cal data md5sum
                string CalData_MD5 = "";
                SendAndChk(PortType.SSH, "md5sum /dev/mmcblk0p18", keyword, out res, 0, 5000);
                Match m = Regex.Match(res, @"(?<md5sum>[a-zA-Z0-9]{32})\s+/dev/mmcblk0p18");
                if (m.Success)
                {
                    CalData_MD5 = m.Groups["md5sum"].Value.Trim().ToUpper();
                }
                DisplayMsg(LogType.Log, $"CalData_MD5: {CalData_MD5}");
                if (CalData_MD5 == "")
                {
                    DisplayMsg(LogType.Log, "Get cal data md5sum fail");
                    AddData(item, 1);
                    return;
                }

                if (station == "RF")
                {
                    //upload cal data md5sum to SFCS
                    status_ATS.AddDataRaw("LMG1_CalDataMD5", CalData_MD5, CalData_MD5, "000000");
                }
                else if (station == "Final")
                {
                    //check cal data md5sum with SFCS
                    DisplayMsg(LogType.Log, $"SFCS CalData_MD5: {infor.CalData_MD5.ToUpper()}");
                    if (string.Compare(CalData_MD5, infor.CalData_MD5.ToUpper()) == 0)
                    {
                        DisplayMsg(LogType.Log, "Check cal data md5sum pass");
                    }
                    else
                    {
                        DisplayMsg(LogType.Log, "Check cal data md5sum fail");
                        AddData(item, 1);
                        return;
                    }
                }

                //Backup wifi data
                if (Directory.Exists(wifi_data_backup_path))
                {
                    File.Delete(Path.Combine(wifi_data_backup_path, "backupwifi"));
                    File.Delete(Path.Combine(wifi_data_backup_path, $"backupwifi_{infor.SerialNumber}"));
                    SendAndChk(PortType.SSH, "cd /tmp", "root@OpenWrt:/tmp#", out res, 0, 3000);
                    SendAndChk(PortType.SSH, $"tftp -p -l backupwifi {PC_IP}", "root@OpenWrt:/tmp# \r\n", out res, 0, 5000);
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
                SendAndChk(PortType.SSH, "cd ~", keyword, out res, 0, 3000);
            }
        }
        private void CheckWiFiCalData(bool IsFinal_staion)
        {
            if (!CheckGoNoGo())
            {
                return;
            }
            int delayMs = 0;
            int timeOutMs = 30 * 1000;
            string item = "CheckWiFiCalData";
            string keyword = "root@OpenWrt:~# \r\n"; //避免誤判到指令第一行的"root@OpenWrt:~#"
            string res = "";
            string wifi_data_backup_path = Path.Combine(Application.StartupPath, "WiFiCalData_backup");
            string PC_IP = Func.ReadINI("Setting", "IP", "PC", "192.168.1.2");
            var regex = "(.{2}):(.{2}):(.{2}):(.{2}):(.{2}):(.{2})";

            try
            {
                DisplayMsg(LogType.Log, "=============== Check WiFi Cal Data ===============");
                DisplayMsg(LogType.Log, "BaseMAC: " + infor.BaseMAC);

                OpenTftpd32(wifi_data_backup_path);

                //Backup wifi data
                SendAndChk(PortType.SSH, "cat /dev/mmcblk0p18 > /tmp/backupwifi", keyword, out res, 0, 5000);
                if (res.Contains("No such file or directory"))
                {
                    DisplayMsg(LogType.Log, "Backup wifi data fail");
                    AddData(item, 1);
                    return;
                }
                /*                SendAndChk(PortType.SSH, "uci set wireless.radio0_band0.disabled='0'", keyword, delayMs, timeOutMs);
                                SendAndChk(PortType.SSH, "uci set wireless.radio0_band1.disabled='0'", keyword, delayMs, timeOutMs);
                                SendAndChk(PortType.SSH, "uci set wireless.radio0_band2.disabled='0'", keyword, delayMs, timeOutMs);
                                SendAndChk(PortType.SSH, "uci commit", keyword, delayMs, timeOutMs);
                                SendAndChk(PortType.SSH, "wifi", keyword, delayMs, timeOutMs);*/
                //check WiFi 2.4G MAC = BaseMAC+4
                string wifi_2g_mac = MACConvert(infor.BaseMAC, 4);
                DisplayMsg(LogType.Log, "WiFi_2G_MAC: " + wifi_2g_mac);
                //SendAndChk(PortType.SSH, "ifconfig wlan0", wifi_2g_mac, out res, 0, 5000);
                wifi_2g_mac = Regex.Replace(wifi_2g_mac, regex, "$2$1 $4$3 $6$5").ToLower();

                SendAndChk(PortType.SSH, "hexdump -s 0x001016 -n 6 /dev/mmcblk0p18", keyword, out res, 0, 5000);
                if (!res.Contains(wifi_2g_mac))
                {
                    DisplayMsg(LogType.Log, "Check WiFi 2.4G MAC fail");
                    AddData(item, 1);
                    return;
                }

                //check WiFi 5G MAC = BaseMAC+3
                string wifi_5g_mac = MACConvert(infor.BaseMAC, 3);
                DisplayMsg(LogType.Log, "WiFi_5G_MAC: " + wifi_5g_mac);
                //SendAndChk(PortType.SSH, "ifconfig wlan1", wifi_5g_mac, out res, 0, 5000);
                wifi_5g_mac = Regex.Replace(wifi_5g_mac, regex, "$2$1 $4$3 $6$5").ToLower();

                SendAndChk(PortType.SSH, "hexdump -s 0x026810 -n 6 /dev/mmcblk0p18", keyword, out res, 0, 5000);
                if (!res.Contains(wifi_5g_mac))
                {
                    DisplayMsg(LogType.Log, "Check WiFi 5G MAC fail");
                    AddData(item, 1);
                    return;
                }

                //check WiFi 6G MAC = BaseMAC+2
                string wifi_6g_mac = MACConvert(infor.BaseMAC, 2);
                DisplayMsg(LogType.Log, "WiFi_6G_MAC: " + wifi_6g_mac);
                //SendAndChk(PortType.SSH, "ifconfig wlan2", wifi_6g_mac, out res, 0, 5000);
                wifi_6g_mac = Regex.Replace(wifi_6g_mac, regex, "$2$1 $4$3 $6$5").ToLower();

                SendAndChk(PortType.SSH, "hexdump -s 0x058810 -n 6 /dev/mmcblk0p18", keyword, out res, 0, 5000);
                if (!res.Contains(wifi_6g_mac))
                {
                    DisplayMsg(LogType.Log, "Check WiFi 6G MAC fail");
                    AddData(item, 1);
                    return;
                }

                //Rena_20230809, check某個區間的cal data
                SendAndChk(PortType.SSH, "hexdump -s 0x1000 -n 16 /dev/mmcblk0p18", keyword, out res, 0, 5000);
                if (res.Contains("0001000 0000 0000 0000 0000 0000 0000 0000 0000"))
                {
                    DisplayMsg(LogType.Log, "Check cal data fail, hexdump 0x1000 is empty");
                    AddData(item, 1);
                    return;
                }

                // ============================================================================
                if (IsFinal_staion)
                {
                    //int IfconfigOK = this.ifconfigVerifiedMac(PortType.SSH, IsFinal_staion) == true ? 0 : 1;
                    //AddData(item, IfconfigOK);
                }

                if (!CheckGoNoGo())
                {
                    return;
                }

                // ============================================================================
                //Rena_20230809, check cal data md5sum
                string CalData_MD5 = "";
                SendAndChk(PortType.SSH, "md5sum /dev/mmcblk0p18", keyword, out res, 0, 5000);
                Match m = Regex.Match(res, @"(?<md5sum>[a-zA-Z0-9]{32})\s+/dev/mmcblk0p18");
                if (m.Success)
                {
                    CalData_MD5 = m.Groups["md5sum"].Value.Trim().ToUpper();
                }
                DisplayMsg(LogType.Log, $"CalData_MD5: {CalData_MD5}");
                if (CalData_MD5 == "")
                {
                    DisplayMsg(LogType.Log, "Get cal data md5sum fail");
                    AddData(item, 1);
                    return;
                }

                if (station == "RF")
                {
                    //upload cal data md5sum to SFCS
                    status_ATS.AddDataRaw("LMG1_CalDataMD5", CalData_MD5, CalData_MD5, "000000");
                }
                else if (station == "Final")
                {
                    //check cal data md5sum with SFCS
                    DisplayMsg(LogType.Log, $"SFCS CalData_MD5: {infor.CalData_MD5.ToUpper()}");
                    if (string.Compare(CalData_MD5, infor.CalData_MD5.ToUpper()) == 0)
                    {
                        DisplayMsg(LogType.Log, "Check cal data md5sum pass");
                    }
                    else
                    {
                        DisplayMsg(LogType.Log, "Check cal data md5sum fail");
                        AddData(item, 1);
                        return;
                    }
                }

                //Backup wifi data
                if (Directory.Exists(wifi_data_backup_path))
                {
                    File.Delete(Path.Combine(wifi_data_backup_path, "backupwifi"));
                    File.Delete(Path.Combine(wifi_data_backup_path, $"backupwifi_{infor.SerialNumber}"));
                    SendAndChk(PortType.SSH, "cd /tmp", "root@OpenWrt:/tmp#", out res, 0, 3000);
                    SendAndChk(PortType.SSH, $"tftp -p -l backupwifi {PC_IP}", "root@OpenWrt:/tmp# \r\n", out res, 0, 5000);
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
                SendAndChk(PortType.SSH, "cd ~", keyword, out res, 0, 3000);
            }
        }

        private bool ifconfigVerifiedMac(PortType portType, bool IsFinal_station)
        {
            string res = string.Empty;
            bool IsPass = false;
            try
            {
                // ============================ ifconfig ath0, ath1, ath2 ===============================
                //this.RF_enable_all_wifi(PortType.SSH);
                // ================================
                Thread.Sleep(10 * 1000);
                SendAndChk(portType, "\r\n", "#", out res, 100, 5000);
                SendAndChk(portType, "ifconfig", "#", out res, 100, 5000);
                SendAndChk(portType, "ifconfig wlan0 | grep HWaddr", "wlan0", out res, 100, 5000);
                DisplayMsg(LogType.Log, "WiFi_2G_MAC: " + MACConvert_second(infor.BaseMAC, 4));
                if (!res.Contains(MACConvert_second(infor.BaseMAC, 4)))
                {
                    DisplayMsg(LogType.Log, "Check WiFi 2.4G MAC fail");
                    return IsPass;
                }
                SendAndChk(portType, "ifconfig wlan1 | grep HWaddr", "wlan1", out res, 100, 5000);
                DisplayMsg(LogType.Log, "WiFi_5G_MAC: " + MACConvert_second(infor.BaseMAC, 3));
                if (!res.Contains(MACConvert_second(infor.BaseMAC, 3)))
                {
                    DisplayMsg(LogType.Log, "Check WiFi 5G MAC fail");
                    return IsPass;
                }
                SendAndChk(portType, "ifconfig wlan2 | grep HWaddr", "wlan2", out res, 100, 5000);
                DisplayMsg(LogType.Log, "WiFi_6G_MAC: " + MACConvert_second(infor.BaseMAC, 2));
                if (!res.Contains(MACConvert_second(infor.BaseMAC, 2)))
                {
                    DisplayMsg(LogType.Log, "Check WiFi 6G MAC fail");
                    return IsPass;
                }
                IsPass = true;
                //=====================================================================================
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, @"ifconfig NG" + ex.Message);
            }
            return IsPass;
        }

        private bool RF_enable_all_wifi(PortType portype)
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

                int delayMs = 0;
                int timeOutMs = 30 * 1000;
                string keyword = "root@OpenWrt";

                DisplayMsg(LogType.Log, @"----------------------------------------------");
                SendCommand(portype, "uci set wireless.wifi0.disabled='0'", delayMs);
                SendCommand(portype, "uci set wireless.wifi1.disabled='0'", delayMs);
                SendCommand(portype, "uci set wireless.wifi2.disabled='0'", delayMs);
                SendAndChk(portype, "uci commit", keyword, delayMs, timeOutMs);
                SendAndChk(portype, "wifi", keyword, delayMs, timeOutMs);
                //Thread.Sleep(10 * 1000);
                //SendAndChk(portype, "sleep 10", keyword, delayMs, timeOutMs); //TODO:?
                //Thread.Sleep(10 * 1000);
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