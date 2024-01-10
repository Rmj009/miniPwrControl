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
using CH_RD;
namespace MiniPwrSupply.LCS5
{
    public partial class frmMain
    {
        private void Final()
        {
            string keyword = "root@OpenWrt:/#";
            string res = "";
            string calixver = "0";
            List<string> listLabel = new List<string>();

            //http://cas.calix.com/?sn=632311000196&pn4=3000301302&pn1=1000590702&mac=14210357605A&fs=CXNK016EFDE9&ec=478469
            string labelContentAll = "";
            string labelSn = string.Empty;
            string labelPartNumber = string.Empty;
            string labelPartNumber100 = string.Empty;
            string labelMac = string.Empty;
            string labelFsan = string.Empty;

            try
            {
                #region Get SFCS data
                DeviceInfor infor = new DeviceInfor();
                SFCS_Query _Sfcs_Query = new SFCS_Query();

                if (Func.ReadINI("Setting", "Golden", "GoldenSN", "").Contains(status_ATS.txtPSN.Text))
                {
                    isGolden = true;
                    DisplayMsg(LogType.Log, "Golden testing");
                }
                else isGolden = false;

                //if (status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode)
                {
                    labelContentAll = status_ATS.txtPSN.Text;

                    labelSn = labelContentAll.Split('=')[1].Substring(0, 12);
                    DisplayMsg(LogType.Log, $"SN from label: {labelSn}");

                    infor.SerialNumber = _Sfcs_Query.GetFromSfcs(labelSn, "@CHJGP1_SN");
                    DisplayMsg(LogType.Log, "Get SN From SFCS is:" + infor.SerialNumber);
                    if (labelSn != infor.SerialNumber)
                    {
                        DisplayMsg(LogType.Log, "SN from label with SN from SFCS not match");
                        warning = "SN Label";
                        return;
                    }
                    else
                    {
                        SetTextBox(status_ATS.txtPSN, infor.SerialNumber);
                        //SetTextBox(status_ATS.txtSP, infor.BaseMAC);
                        status_ATS.SFCS_Data.PSN = infor.SerialNumber;
                        status_ATS.SFCS_Data.First_Line = infor.SerialNumber;
                    }
                    DisplayMsg(LogType.Log, "Get SN From SFCS is:" + infor.SerialNumber);

                    infor.BaseMAC = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@MAC");
                    DisplayMsg(LogType.Log, "Get Base MAC From SFCS is:" + infor.BaseMAC);

                    infor.Eth1MAC = MACConvert(infor.BaseMAC, 0);
                    DisplayMsg(LogType.Log, "Eth1MAC convert from BaseMac: " + infor.Eth1MAC);

                    infor.Eth2GMAC = MACConvert(infor.BaseMAC, 2);
                    DisplayMsg(LogType.Log, "Eth2GMAC convert from BaseMac: " + infor.Eth2GMAC);

                    infor.Eth5GMAC = MACConvert(infor.BaseMAC, 3);
                    DisplayMsg(LogType.Log, "Eth5GMAC convert from BaseMac: " + infor.Eth5GMAC);

                    infor.FSAN = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@LCS5_SSID_SN_FSAN");
                    DisplayMsg(LogType.Log, "Get FSAN From SFCS is:" + infor.FSAN);

                    //string name = Func.ReadINI("Setting", "SFCS", "Calix_Name", "@LCS5_CLX_FW_VER_21");
                    infor.CalixFWver = Func.ReadINI(Application.StartupPath, "Setting", "LCS5_Infor", "CalixFWver", "23.4.905.34"); // _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, name);
                    DisplayMsg(LogType.Log, "Get Calix FW From Setting is:" + infor.CalixFWver);

                    string CalixFWver_sfcs = string.Empty;
                    _Sfcs_Query.Get15Data(infor.SerialNumber, "LCS5_CLX_FW_VER", ref CalixFWver_sfcs);
                    DisplayMsg(LogType.Log, $"LCS5_CLX_FW_VER from 15 line data: {CalixFWver_sfcs}");
                    if (infor.CalixFWver != CalixFWver_sfcs)
                    {
                        warning = "LCS5_CLX_FW_VER fail";
                        DisplayMsg(LogType.Log, "CLX_FW_VER from 15 line data and CLX_FW_VER from setting are differnce");
                        return;
                    }

                    infor.PartNumber = Func.ReadINI(Application.StartupPath, "Setting", "LCS5_Infor", "PartNumber", "3000301302"); //_Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@LCS5_300_PN").Substring(0, 10);
                    DisplayMsg(LogType.Log, "Get PartNumber From Setting is:" + infor.PartNumber);

                    string PartNumber_300_sfcs = string.Empty;
                    _Sfcs_Query.Get15Data(infor.SerialNumber, "LCS5_300_PN", ref PartNumber_300_sfcs);
                    DisplayMsg(LogType.Log, $"LCS5_300_PN from 15 line data: {PartNumber_300_sfcs}");
                    if (infor.PartNumber != PartNumber_300_sfcs)
                    {
                        warning = "LCS5_300_PN fail";
                        DisplayMsg(LogType.Log, "PartNumber_300 from 15 line data and PartNumber_300 from setting are differnce");
                        return;
                    }

                    infor.PartNumber_100 = Func.ReadINI(Application.StartupPath, "Setting", "LCS5_Infor", "PartNumber_100", "1000590702"); //_Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@LCS5_100_PN").Substring(0, 10);
                    DisplayMsg(LogType.Log, "Get PartNumber_100 From Setting is:" + infor.PartNumber_100);

                    string PartNumber_100_sfcs = string.Empty;
                    _Sfcs_Query.Get15Data(infor.SerialNumber, "LCS5_100_PN", ref PartNumber_100_sfcs);
                    DisplayMsg(LogType.Log, $"LCS5_100_PN from 15 line data: {PartNumber_100_sfcs}");
                    if (infor.PartNumber_100 != PartNumber_100_sfcs)
                    {
                        warning = "PartNumber_100 fail";
                        DisplayMsg(LogType.Log, "PartNumber_100 from 15 line data and PartNumber_100 from setting are differnce");
                        return;
                    }

                    infor.MFGDate = DateTime.Now.ToString("MM/dd/yyyy");
                    infor.MFGDate = Func.ReadINI(Application.StartupPath, "Setting", "LCS5_Infor", "MFGDate", "08/09/2023");
                    DisplayMsg(LogType.Log, "MFG date From Setting is:" + infor.MFGDate);

                    infor.GPON = Func.ReadINI(Application.StartupPath, "Setting", "LCS5_Infor", "GPON", "0");
                    DisplayMsg(LogType.Log, "GPON is:" + infor.GPON);

                    infor.ModuleId = Func.ReadINI(Application.StartupPath, "Setting", "LCS5_Infor", "ModuleId", "0");
                    DisplayMsg(LogType.Log, "ModuleId is:" + infor.ModuleId);

                    infor.FWver = Func.ReadINI("Setting", "LCS5_Infor", "MFG_FWver", "v1.1.1");
                    DisplayMsg(LogType.Log, "FWver From Setting is:" + infor.FWver);

                    string FWver_sfcs = string.Empty;
                    _Sfcs_Query.Get15Data(infor.SerialNumber, "LCS5_MFG_FW_VER", ref FWver_sfcs);
                    DisplayMsg(LogType.Log, $"LCS5_MFG_FW_VER from 15 line data: {FWver_sfcs}");
                    if (infor.FWver != FWver_sfcs)
                    {
                        warning = "FWver fail";
                        DisplayMsg(LogType.Log, "FW_VER from 15 line data and FW_VER from setting are differnce");
                        return;
                    }
                    //string FWname = Func.ReadINI("Setting", "SFCS", "MFGFW_Name", "@LCS5_MFG_FW_VER_18");
                    //GetFromSfcs(FWname, out infor.FWver);
                    //infor.FWver = infor.FWver.Substring(0, infor.FWver.Length - 10);                   

                    infor.HWver = Func.ReadINI(Application.StartupPath, "Setting", "LCS5_Infor", "HWver", "02");
                    DisplayMsg(LogType.Log, "HWver From Setting is:" + infor.HWver);

                    string HWver_sfcs = string.Empty;
                    _Sfcs_Query.Get15Data(infor.SerialNumber, "LCS5_HW_VER", ref HWver_sfcs);
                    DisplayMsg(LogType.Log, $"LCS5_HW_VER from 15 line data: {HWver_sfcs}");
                    if (infor.HWver != HWver_sfcs)
                    {
                        warning = "HWver fail";
                        DisplayMsg(LogType.Log, "HW_VER from 15 line data and HW_VER from setting are differnce");
                        return;
                    }

                    #region Check label             
                    labelSn = labelContentAll.Split('=')[1].Substring(0, 12);
                    DisplayMsg(LogType.Log, $"SN from label: {labelSn}");
                    if (labelSn != infor.SerialNumber)
                    {
                        DisplayMsg(LogType.Log, "SN from label with SN from SFCS not match");
                        warning = "SN Label";
                        return;
                    }
                    else DisplayMsg(LogType.Log, "SN from label and SN from SFCS are the same");

                    labelPartNumber = labelContentAll.Split('=')[2].Substring(0, 10);
                    DisplayMsg(LogType.Log, $"PartNumber from label: {labelPartNumber}");
                    if (labelPartNumber != infor.PartNumber)
                    {
                        DisplayMsg(LogType.Log, "PartNumber from label with PartNumber from SFCS not match");
                        warning = "PartNumber Label";
                        return;
                    }
                    else DisplayMsg(LogType.Log, "PartNumber from label and PartNumber from SFCS are the same");

                    labelPartNumber100 = labelContentAll.Split('=')[3].Substring(0, 10);
                    DisplayMsg(LogType.Log, $"PartNumber_100 from label: {labelPartNumber100}");
                    if (labelPartNumber100 != infor.PartNumber_100)
                    {
                        DisplayMsg(LogType.Log, "PartNumber_100 from label with PartNumber_100 from SFCS not match");
                        warning = "PartNumber_100 Label";
                        return;
                    }
                    else DisplayMsg(LogType.Log, "PartNumber_100 from label and PartNumber_100 from SFCS are the same");

                    labelMac = labelContentAll.Split('=')[4].Substring(0, 12);
                    DisplayMsg(LogType.Log, $"BaseMAC from label: {labelMac}");
                    if (labelMac != infor.BaseMAC)
                    {
                        DisplayMsg(LogType.Log, "BaseMAC from label with BaseMAC from SFCS not match");
                        warning = "BaseMAC Label";
                        return;
                    }
                    else DisplayMsg(LogType.Log, "BaseMAC from label and BaseMAC from SFCS are the same");

                    labelFsan = labelContentAll.Split('=')[5].Substring(0, 12);
                    DisplayMsg(LogType.Log, $"FSAN from label: {labelFsan}");
                    if (labelFsan != infor.FSAN)
                    {
                        DisplayMsg(LogType.Log, "FSAN from label with FSAN from SFCS not match");
                        warning = "FSAN Label";
                        return;
                    }
                    else DisplayMsg(LogType.Log, "FSAN from label and FSAN from SFCS are the same");
                    #endregion                            
                }
                #endregion



                if (Func.ReadINI("Setting", "IO_Board_Control", "IO_Control_1", "0") == "1")
                {
                    string txPin = Func.ReadINI("Setting", "IO_Board_Control", "Pin0", "0"); // dut
                    string rev_message = "";
                    status_ATS.AddLog("IO_Board_Y" + txPin + " On...");
                    IO_Board_Control1.ConTrolIOPort_write(Int32.Parse(txPin), "1", ref rev_message);
                    DisplayMsg(LogType.Log, rev_message);
                }
                else if (Func.ReadINI("Setting", "Port", "RelayBoard", "Disable").ToUpper() == "ENABLE")
                {
                    SwitchRelay(CTRL.OFF);
                    Thread.Sleep(3000);
                    SwitchRelay(CTRL.ON);
                }
                DisplayMsg(LogType.Log, "Power on!!!");

                DisplayMsg(LogType.Log, $"delay {delayMFGFW}s before bootup");
                Thread.Sleep(delayMFGFW * 1000);

                DisplayMsg(LogType.Log, "========= BootUp DUT & Initial Telnet =========");
                if (!ChkInitial(PortType.TELNET, keyword, 250 * 1000))
                {
                    AddData("BootUp", 1);
                    return;
                }
                AddData("BootUp", 0);

                int bootupdelay = Convert.ToInt32(Func.ReadINI("Setting", "LCS5", "BootupDelay", "20000"));
                DisplayMsg(LogType.Log, $"Delay {bootupdelay}ms");
                Thread.Sleep(bootupdelay);

                if (Func.ReadINI("Setting", "Setting", "IsDebug", "0") == "1" && status_ATS._testMode == StatusUI2.StatusUI.TestMode.EngMode)
                {
                    infor.SerialNumber = Func.ReadINI(Application.StartupPath, "Setting", "LCS5_Infor", "SerialNumber", "630301000027");
                    infor.NoLevel300 = Func.ReadINI(Application.StartupPath, "Setting", "LCS5_Infor", "NoLevel300", "3000301302");
                    infor.PartNumber = Func.ReadINI(Application.StartupPath, "Setting", "LCS5_Infor", "PartNumber", "1000590701");
                    infor.PartNumber_100 = Func.ReadINI(Application.StartupPath, "Setting", "LCS5_Infor", "PartNumber_100", "1000590701");
                    infor.BaseMAC = Func.ReadINI(Application.StartupPath, "Setting", "LCS5_Infor", "BaseMAC", "001122334400");
                    infor.Eth1MAC = Func.ReadINI(Application.StartupPath, "Setting", "LCS5_Infor", "Eth1MAC", "00:11:22:33:44:00");
                    infor.Eth2GMAC = Func.ReadINI(Application.StartupPath, "Setting", "LCS5_Infor", "Eth2GMAC", "00:11:22:33:44:02");
                    infor.Eth5GMAC = Func.ReadINI(Application.StartupPath, "Setting", "LCS5_Infor", "Eth5GMAC", "00:11:22:33:44:03");
                    infor.FSAN = Func.ReadINI(Application.StartupPath, "Setting", "LCS5_Infor", "FSAN", "CXNK00DBB4C2");
                    infor.MFGDate = Func.ReadINI(Application.StartupPath, "Setting", "LCS5_Infor", "MFGDate", "08/09/2023");
                    infor.CalixFWver = Func.ReadINI(Application.StartupPath, "Setting", "LCS5_Infor", "CalixFWver", "23.4.905.29");
                    infor.ModuleId = "0";
                    infor.HWver = "02";
                    infor.GPON = "0";
                }

                infor.BaseMAC = MACConvert(infor.BaseMAC);
                DisplayMsg(LogType.Log, "Base MAC convert: " + infor.BaseMAC);
                CheckDUTInfo(infor, PortType.TELNET);

                if (!CheckGoNoGo())
                {
                    return;
                }

                string md5_cal_data = "";
                _Sfcs_Query.Get15Data(infor.SerialNumber, "LCS5_CAL_DATA_MD5", ref md5_cal_data);
                DisplayMsg(LogType.Log, "LCS5_CAL_DATA_MD5 from SFCS: " + md5_cal_data);
                if (md5_cal_data == "")
                {
                    warning = "Get md5_cal_data fail";
                    DisplayMsg(LogType.Log, "Get md5_cal_data from SFCS fail");
                    return;
                }

                //Check Calibration Data md5sum              
                if (!SendAndChk(PortType.TELNET, "cat /dev/mmcblk0p13 | md5sum", keyword, out res, 0, 3000))
                {
                    AddData("ChkCaliDataMD5Sum", 1);
                    return;
                }
                if (res.Contains(md5_cal_data))
                {
                    AddData("ChkCaliDataMD5Sum", 0);
                }
                else
                {
                    AddData("ChkCaliDataMD5Sum", 1);
                    return;
                }

                CheckLED(PortType.TELNET);

                ResetButton(PortType.TELNET);

                WPSButton(PortType.TELNET);

                NvramTest_Final(infor, calixver);

                CheckAdaptorDetection(PortType.TELNET);

                CheckPSEDetection(PortType.TELNET);

                //use tool to check calibration data
                bool fResult = CH_RD.Check.FinalCheck(out res, new string[] { "LCS5", md5_cal_data });//e2fcacc27a6123666a12f140d758dec9
                                                                                                      //37c217b470067661acb7ef366cb7cfa3
                DisplayMsg(LogType.Log, res);

                if (!fResult)
                {
                    warning = "CAL_DATA fail";
                    DisplayMsg(LogType.Log, "use RD tool to check calibration data fail");
                    return;
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Log, ex.ToString());
                warning = "Exception";
            }
            finally
            {
                if (Func.ReadINI("Setting", "IO_Board_Control", "IO_Control_1", "0") == "1")
                {
                    string txPin = Func.ReadINI("Setting", "IO_Board_Control", "Pin0", "0"); // dut
                    string rev_message = "";
                    status_ATS.AddLog("IO_Board_Y" + txPin + " Off...");
                    IO_Board_Control1.ConTrolIOPort_write(Int32.Parse(txPin), "2", ref rev_message);
                    DisplayMsg(LogType.Log, rev_message);
                }

                SwitchRelay(CTRL.ON);
            }
        }
        private void NvramTest_Final(DeviceInfor infor, string calix)
        {
            string calixver = calix;
            if (!CheckGoNoGo())
            {
                return;
            }
            try
            {
                string res = "";

                DisplayMsg(LogType.Log, $"========================== Nvram Test ==========================");

                SendAndChk(PortType.TELNET, "wnc_nvram -h", "root@OpenWrt", out res, 0, 3000);
                SendAndChk(PortType.TELNET, "dd if=/dev/mmcblk0p27 of=/a.bin", "root@OpenWrt", out res, 0, 3000);
                SendAndChk(PortType.TELNET, "wnc_nvram -r a.bin -A", "root@OpenWrt", out res, 0, 3000);

                bool rs = false;
                rs = CheckNvram(res, "Version: 2");
                rs = rs && CheckNvram(res, "ID: 92");
                rs = rs && CheckNvram(res, "This name is GPR1027E");
                rs = rs && CheckNvram(res, "CLEI code is BVMNC00ARA");
                rs = rs && CheckNvram(res, infor.SerialNumber);
                rs = rs && CheckNvram(res, infor.PartNumber);
                rs = rs && CheckNvram(res, infor.PartNumber_100);
                rs = rs && CheckNvram(res, infor.MFGDate.Substring(infor.MFGDate.Length - 4, 4));
                rs = rs && CheckNvram(res, "MAC quantity is 5");
                rs = rs && CheckNvram(res, ConvertStringToMACFormat(infor.BaseMAC.Replace(":", "").Replace("-", "")).Replace(":", "-").ToLower());
                rs = rs && CheckNvram(res, "FSAN is " + infor.FSAN);
                rs = rs && CheckNvram(res, "GPON password is " + infor.GPON);
                rs = rs && CheckNvram(res, "Country code is US");
                //DisplayMsg(LogType.Log, "Get calixVer from SFCS: " + calixver);
                //rs = rs && CheckNvram(res, calixver);
                rs = rs && CheckNvram(res, infor.CalixFWver);
                rs = rs && CheckNvram(res, "DTM Model ID is:" + infor.ModuleId);

                SendAndChk(PortType.TELNET, "rm -f a.bin", "root@OpenWrt", out res, 0, 3000);

                if (rs)
                {
                    AddData("Nvram", 0);
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                AddData("Nvram", 1);
            }
        }
        private void CheckDUTInfo_Final(DeviceInfor infor)
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            try
            {
                #region  Check DUT Infor
                if ((status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode && !isGolden)) // LCS5 ignore forHQtest ||
                {
                    #region fw                    
                    WriteOrCheckDeviceInfor(PortType.TELNET, "Check_FW", $"cat /etc/wnc_ver", infor.FWver);
                    #endregion

                    #region sn
                    WriteOrCheckDeviceInfor(PortType.TELNET, "Check_SN", $"fw_printenv serialno", infor.SerialNumber);
                    #endregion

                    #region base mac
                    infor.BaseMAC = MACConvert(infor.BaseMAC);
                    WriteOrCheckDeviceInfor(PortType.TELNET, "Check_BaseMAC", $"fw_printenv baseMAC", infor.BaseMAC);
                    #endregion

                    #region ipaddr
                    WriteOrCheckDeviceInfor(PortType.TELNET, "Check_IPADDR", $"fw_printenv eth0addr", infor.IPADDR);
                    #endregion
                    #region ipaddr
                    WriteOrCheckDeviceInfor(PortType.TELNET, "Check_IPADDR", $"fw_printenv wifi0addr", infor.IPADDR);
                    #endregion
                    #region ipaddr
                    WriteOrCheckDeviceInfor(PortType.TELNET, "Check_IPADDR", $"fw_printenv wifi1addr", infor.IPADDR);
                    #endregion

                    #region FSAN
                    WriteOrCheckDeviceInfor(PortType.TELNET, "Check_FSAN", $"fw_printenv FSAN", infor.FSAN);
                    #endregion

                    #region CheckHwVer
                    WriteOrCheckDeviceInfor(PortType.TELNET, "Check_HwVer", $"fw_printenv hwver", infor.HWver);
                    #endregion

                    #region serverip
                    //WriteOrCheckDeviceInfor(PortType.TELNET, "Check_ServerIP", $"fw_printenv serverip", infor.ServerIP);
                    #endregion

                    #region serverip
                    //WriteOrCheckDeviceInfor(PortType.TELNET, "Check_BootDelay", $"fw_printenv bootdelay", "=2");
                    #endregion
                }
                #endregion
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData("CheckDUTInfo", 1);
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
        private bool CheckLabel(List<string> label, DeviceInfor infor)
        {
            try
            {
                SFCS_Query _Sfcs_Query = new SFCS_Query();
                string onuMAC = "";
                string mtaMAC = "";
                string pn100 = "";
                string pn300 = "";
                _Sfcs_Query.Get15Data(infor.SerialNumber, "LCS3_LABEL_ONU_MAC", ref onuMAC);
                _Sfcs_Query.Get15Data(infor.SerialNumber, "LCS3_LABEL_MTA_MAC", ref mtaMAC);
                _Sfcs_Query.Get15Data(infor.SerialNumber, "LCS3_300_PN", ref pn300);
                _Sfcs_Query.Get15Data(infor.SerialNumber, "LCS3_100_PN", ref pn100);
                int count = 0;
                foreach (var item in label)
                {
                    if (!item.Contains("http") && item.Contains(infor.SerialNumber))
                    {
                        if (item.Trim() != infor.SerialNumber)
                        {
                            DisplayMsg(LogType.Log, $"Compare {item.Trim()} with {infor.SerialNumber} fail");
                            return false;
                        }
                        DisplayMsg(LogType.Log, $"Compare {item.Trim()} with {infor.SerialNumber} ok");
                        count++;
                    }
                    if (!item.Contains("http") && item.Contains(onuMAC))
                    {
                        if (item.Trim() != onuMAC)
                        {
                            DisplayMsg(LogType.Log, $"Compare {item.Trim()} with {onuMAC} fail");
                            return false;
                        }
                        DisplayMsg(LogType.Log, $"Compare {item.Trim()} with {onuMAC} ok");
                        count++;
                    }
                    if (!item.Contains("http") && item.Contains(mtaMAC))
                    {
                        if (item.Trim() != mtaMAC)
                        {
                            DisplayMsg(LogType.Log, $"Compare {item.Trim()} with {mtaMAC} fail");
                            return false;
                        }
                        DisplayMsg(LogType.Log, $"Compare {item.Trim()} with {mtaMAC} ok");
                        count++;

                    }
                    if (!item.Contains("http") && item.Contains(infor.FSAN))
                    {
                        if (item.Trim() != infor.FSAN)
                        {
                            DisplayMsg(LogType.Log, $"Compare {item.Trim()} with {infor.FSAN} fail");
                            return false;
                        }
                        DisplayMsg(LogType.Log, $"Compare {item.Trim()} with {infor.FSAN} ok");
                        count++;
                    }
                    if (item.Contains(pn100))
                    {
                        Net.NetPort netport = new Net.NetPort();
                        string data = netport.getMiddleString(item, "pn1=", "&");
                        if (data != pn100)
                        {
                            DisplayMsg(LogType.Log, $"Compare {data.Trim()} with {pn100} fail");
                            return false;
                        }
                        DisplayMsg(LogType.Log, $"Compare {data.Trim()} with {pn100} ok");
                        count++;
                    }
                    if (item.Contains(pn300))
                    {
                        Net.NetPort netport = new Net.NetPort();
                        string data = netport.getMiddleString(item, "pn4=", "&");
                        if (data != pn300)
                        {
                            DisplayMsg(LogType.Log, $"Compare {data.Trim()} with {pn300} fail");
                            return false;
                        }
                        DisplayMsg(LogType.Log, $"Compare {data.Trim()} with {pn300} ok");
                        count++;
                    }
                }
                if (count != 6)
                {
                    DisplayMsg(LogType.Log, $"Check number label fail:{count}");
                    return false;
                }
                DisplayMsg(LogType.Log, "Check label Ok");
                AddData("ChkLabel", 0);
                return true;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Log, ex.ToString());
                warning = "Exception";
                return false;
            }
        }
        private void SetEthFullDuplex(string eth_index)
        {
            string res = "";
            string item = $"Eth{eth_index}_FullDuplex";

            try
            {
                if (!CheckGoNoGo())
                    return;

                DisplayMsg(LogType.Log, $"=== {item} ===");

                SendAndChk(PortType.TELNET, $"ssdk_sh debug phy set {eth_index} 0x0 0x8000", "root@OpenWrt", out res, 0, 3000);
                if (!res.Contains("SSDK Init OK"))
                {
                    DisplayMsg(LogType.Log, $"{item} fail");
                    AddData(item, 1);
                    return;
                }

                SendAndChk(PortType.TELNET, $"ssdk_sh debug phy set {eth_index} 0x0 0x3100", "root@OpenWrt", out res, 0, 3000);
                if (!res.Contains("SSDK Init OK"))
                {
                    DisplayMsg(LogType.Log, $"{item} fail");
                    AddData(item, 1);
                    return;
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Log, ex.ToString());
                AddData(item, 1);
            }
        }
        private void SetEthHalfDuplex(string eth_index)
        {
            string res = "";
            string item = $"Eth{eth_index}_HalfDuplex";

            try
            {
                if (!CheckGoNoGo())
                    return;

                DisplayMsg(LogType.Log, $"=== {item} ===");

                SendAndChk(PortType.TELNET, $"ssdk_sh debug phy set {eth_index} 0x0 0x8000", "root@OpenWrt", out res, 0, 3000);
                if (!res.Contains("SSDK Init OK"))
                {
                    DisplayMsg(LogType.Log, $"{item} fail");
                    AddData(item, 1);
                    return;
                }

                SendAndChk(PortType.TELNET, $"ssdk_sh debug phy set {eth_index} 0x0 0x3000", "root@OpenWrt", out res, 0, 3000);
                if (!res.Contains("SSDK Init OK"))
                {
                    DisplayMsg(LogType.Log, $"{item} fail");
                    AddData(item, 1);
                    return;
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Log, ex.ToString());
                AddData(item, 1);
            }
        }
        private void EthDuplexTest(string item, string ip, int eth, bool full_duplex = true)
        {
            string res = "";

            if (!CheckGoNoGo())
                return;

            try
            {
                DisplayMsg(LogType.Log, $"================ {item} ================");

                if (!SendAndChk(PortType.TELNET, $"ssdk_sh debug phy set {eth} 0x0 0x8000", "root@OpenWrt", out res, 1000, 3000, 5))// pe Jin required retry
                {
                    DisplayMsg(LogType.Log, "Gọi PE/TE kiểm tra trạng thái card mạng");
                    MessageBox.Show("Gọi PE/TE kiểm tra trạng thái card mạng");
                }
                DisplayMsg(LogType.Log, "Delay 1s...");
                Thread.Sleep(1000);
                if (!res.Contains("SSDK Init OK"))
                {
                    DisplayMsg(LogType.Log, $"{item} fail");
                    AddData(item, 1);
                    return;
                }

                if (full_duplex)
                {
                    if (!SendAndChk(PortType.TELNET, $"ssdk_sh debug phy set {eth} 0x0 0x3100", "root@OpenWrt", out res, 1000, 3000, 5))
                    {
                        DisplayMsg(LogType.Log, "Gọi PE/TE kiểm tra trạng thái card mạng");
                        MessageBox.Show("Gọi PE/TE kiểm tra trạng thái card mạng");
                    }
                }
                else
                {
                    if (!SendAndChk(PortType.TELNET, $"ssdk_sh debug phy set {eth} 0x0 0x3000", "root@OpenWrt", out res, 1000, 3000, 5))
                    {
                        DisplayMsg(LogType.Log, "Gọi PE/TE kiểm tra trạng thái card mạng");
                        MessageBox.Show("Gọi PE/TE kiểm tra trạng thái card mạng");
                    }
                }


                if (!res.Contains("SSDK Init OK"))
                {
                    DisplayMsg(LogType.Log, $"{item} fail");
                    AddData(item, 1);
                    return;
                }

                //Rena_20221014 可能會讀到Unknown所以增加retry
                int retry = 0;
            eth_speed_retry:
                SendAndChk(PortType.TELNET, $"ethtool eth{eth} | grep Speed", "root@OpenWrt", out res, 0, 5000);
                if (res.Contains("Unknown"))
                {
                    if (retry++ < 5)
                    {
                        DisplayMsg(LogType.Log, "Delay 1s...");
                        Thread.Sleep(1000);
                        DisplayMsg(LogType.Log, $"eth{eth} speed is unknown, retry...");
                        goto eth_speed_retry;
                    }
                }
                if (!res.Contains("1000Mb"))
                {
                    DisplayMsg(LogType.Log, $"Check eth{eth} speed fail");
                    if (retry++ < 5)
                    {
                        DisplayMsg(LogType.Log, $"eth{eth} speed is not contain 1000Mb, retry...");
                        Thread.Sleep(1000);
                        goto eth_speed_retry;
                    }
                    AddData(item, 1);
                    return;
                }

                if (!SendAndChk(item, PortType.TELNET, $"ethtool eth{eth} | grep \"Link detected\"", "yes", 0, 5000))
                {
                    DisplayMsg(LogType.Log, $"Check eth{eth} link detected fail");
                    return;
                }
                if (PingDUT(item, PortType.TELNET, ip, "ttl=", out res, 15000, 3))
                {
                    DisplayMsg(LogType.Log, $"Ping eth{eth} PC Pass");
                }
                else
                {
                    DisplayMsg(LogType.Log, $"Ping eth{eth} PC Fail");
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Log, ex.ToString());
                AddData(item, 1);
            }
        }
        private void EthernetTest_Final()
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

                //SPF12.0版FW預設會做這些所以可下可不下
                //network restart後console會吐一堆log,可能影響到後面的指令
                //SFP settings
                #region SFP_setting
                //DisplayMsg(LogType.Log, $"=== Config SFP ===");
                //SendAndChk(PortType.TELNET, "uci set network.lan.ifname='eth0 eth1 eth2 eth3 eth4 eth5'", keyword, 0, 5000);
                //SendAndChk(PortType.TELNET, "uci delete network.wan.ifname='eth5'", keyword, 0, 5000);
                //SendAndChk(PortType.TELNET, "uci commit network", keyword, 0, 5000);
                //SendCommand(PortType.TELNET, "/etc/init.d/network restart", 0);
                //DisplayMsg(LogType.Log, "Delay 10 (s)..");
                //System.Threading.Thread.Sleep(10 * 1000);
                //ChkResponse(PortType.TELNET, ITEM.NONE, keyword, out res, 5000);

                ////network restart後telnet會斷線需要重連
                //if (!ChkInitial(PortType.TELNET, keyword, 20000))
                //{
                //    AddData("Telnet", 1);
                //    return;
                //}
                #endregion

                //從WPS button方向開始
                //WPS Button|eth0|eth1|eth2|eth3|eth4(10G)|.....|eth5(SFP 10G)
                #region Check link-rate & ping
                string Eth0_IP = Func.ReadINI("Setting", "PCBA", "Eth0_PC_IP", "192.168.1.10");
                string Eth1_IP = Func.ReadINI("Setting", "PCBA", "Eth1_IP", "192.168.1.11");
                string Eth2_IP = Func.ReadINI("Setting", "PCBA", "Eth2_IP", "192.168.1.12");
                string Eth3_IP = Func.ReadINI("Setting", "PCBA", "Eth3_IP", "192.168.1.13");
                string Eth4_IP = Func.ReadINI("Setting", "PCBA", "Eth4_IP", "192.168.1.14"); //10G port
                string SFP_IP = Func.ReadINI("Setting", "PCBA", "SFP_IP", "192.168.1.15"); //10G SFP port
                string rec = "";

                EthDuplexTest("Eth3_Full_Duplex", Eth3_IP, 3);
                EthDuplexTest("Eth3_Half_Duplex", Eth3_IP, 3, false);

                EthDuplexTest("Eth2_Full_Duplex", Eth2_IP, 2);
                EthDuplexTest("Eth2_Half_Duplex", Eth2_IP, 2, false);

                EthDuplexTest("Eth1_Full_Duplex", Eth1_IP, 1);
                EthDuplexTest("Eth1_Half_Duplex", Eth1_IP, 1, false);

                EthDuplexTest("Eth0_Full_Duplex", Eth0_IP, 0);
                EthDuplexTest("Eth0_Half_Duplex", Eth0_IP, 0, false);

                if (!CheckGoNoGo())
                {
                    return;
                }

                #region eth4
                // ping eth4
                DisplayMsg(LogType.Log, $"====== Ping eth4 PC {Eth4_IP} ======");

                if (PingDUT("Ping_Eth4", PortType.TELNET, Eth4_IP, "ttl=", out rec, 15000, 3))
                {
                    DisplayMsg(LogType.Log, "Ping eth4 PC Pass");
                }
                else
                {
                    DisplayMsg(LogType.Log, "Ping eth4 PC Fail");
                    return;
                }
                #endregion

                if (!CheckGoNoGo())
                {
                    return;
                }

                // ping SFP
                DisplayMsg(LogType.Log, $"====== Ping SFP PC {SFP_IP} ======");
                int c = 0;
            retry:
                if (PingDUT("Ping_SFP", PortType.TELNET, SFP_IP, "ttl=", out rec, 15000, 3))
                {
                    DisplayMsg(LogType.Log, "Ping SFP PC Pass");
                }
                else
                {
                    DisplayMsg(LogType.Log, "Ping SFP PC Fail");
                    if (c < 3)
                    {
                        Thread.Sleep(2000);
                        c++;
                        goto retry;
                    }
                    return;
                }
                #endregion
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData("EthernetTest", 1);
            }
        }
    }
}
