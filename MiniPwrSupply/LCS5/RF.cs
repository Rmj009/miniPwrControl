using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
//using WNC.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiniPwrSupply.LCS5
{
    public partial class frmMain_RF
    {
        private string _LitePointFolder = string.Empty;
        private string _LitePointTool = string.Empty;
        private string _LitePointSummary = string.Empty;
        private string _LitePointLog_all = string.Empty;
        private string _LitePointLog_dutSetup_6G = string.Empty;
        private string MAC6G = "";

        private void RF_Test()
        {
            if (!CheckGoNoGo())
                return;
            string keyword = "root@OpenWrt:/#";
            string res = "";
            string _WNCSN = "";
            string _MAC = "";
            string sfcssn = "";
            string md5sumPrior = string.Empty;
            string md5sumAfter = string.Empty;

            MAC6G = "";

            try
            {
                DisplayMsg(LogType.Log, "================= RF TEST ===============");

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
                    Thread.Sleep(3000);
                    SwitchRelay(CTRL.ON);
                }
                else
                    MessageBox.Show("Power on");

                DisplayMsg(LogType.Log, "Delay 80s for bootup...");//need to be optimized
                System.Threading.Thread.Sleep(80 * 1000);
                if (!ChkInitial(PortType.TELNET, keyword, 200000))
                {
                    AddData("BootUp", 1);
                    return;
                }
                AddData("BootUp", 0);
                if (status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode)
                {
                    _WNCSN = status_ATS.txtPSN.Text;
                    GetFromSfcs("@LCS5_SN", out sfcssn);
                    GetFromSfcs("@MAC", out _MAC);
                    if (_WNCSN != sfcssn)
                    {
                        warning = "Check sn with sfcs fail";
                        return;
                    }
                    _WNCSN = sfcssn;
                }
                else
                {
                    _WNCSN = status_ATS.txtPSN.Text;
                    _MAC = status_ATS.txtSP.Text;
                }
                SetTextBox(status_ATS.txtPSN, _WNCSN);
                SetTextBox(status_ATS.txtSP, _MAC);
                status_ATS.SFCS_Data.First_Line = _WNCSN;
                status_ATS.SFCS_Data.PSN = _WNCSN;

                if (Func.ReadINI("Setting", "Golden", "GoldenSN", "").Contains(_WNCSN))
                {
                    isGolden = true;
                    DisplayMsg(LogType.Log, "Golden testing");
                }
                else isGolden = false;

                EnterTestMode();
                // TESTPLAN 0817 add 
                md5sumPrior = this.CheckCalibrationData();

                RFTest_WiFi();
                // TESTPLAN 0817 add 
                md5sumAfter = this.CheckCalibrationData();
                if (md5sumPrior == md5sumAfter)
                {
                    DisplayMsg(LogType.Error, "before" + md5sumPrior + " md5sum different => " + md5sumAfter);
                    return;
                }
                else if (md5sumPrior == null || md5sumAfter == null)
                {
                    DisplayMsg(LogType.Error, "before" + md5sumPrior + " md5sum different => " + md5sumAfter);
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
                    string txPin = Func.ReadINI("Setting", "IO_Board_Control", "Pin0", "0");
                    string rev_message = "";
                    status_ATS.AddLog("IO_Board_Y" + txPin + " Off...");
                    IO_Board_Control1.ConTrolIOPort_write(Int32.Parse(txPin), "2", ref rev_message);
                    DisplayMsg(LogType.Log, rev_message);
                }
                else SwitchRelay(CTRL.ON);
            }
        }
        private void EnterTestMode()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            try
            {
                DisplayMsg(LogType.Log, "========= Enter test mode =========");

                string res = "";
                string keyword = "root@OpenWrt:/#";
                string pc_IP = Func.ReadINI("Setting", "PC_IP", "IP", "192.168.1.77");
                DisplayMsg(LogType.Log, $"PC_IP : {pc_IP}");
                //DisplayMsg(LogType.Log, "====Debuger0===");

                SendAndChk(PortType.TELNET, "rmmod monitor", keyword, 0, 3000);

                SendAndChk(PortType.TELNET, "wifi down", keyword, 0, 30 * 1000);

                SendAndChk(PortType.TELNET, "rmmod wifi_3_0", keyword, 0, 3000);

                SendAndChk(PortType.TELNET, "rmmod wifi_2_0", keyword, 0, 3000);

                SendAndChk(PortType.TELNET, "rmmod qca_ol", keyword, 0, 3000);

                SendAndChk(PortType.TELNET, "insmod qca_ol hw_mode_id=1 testmode=1 cfg80211_config=1", keyword, 0, 3000);

                SendAndChk(PortType.TELNET, "insmod wifi_3_0", keyword, 0, 10 * 1000);

                SendAndChk(PortType.TELNET, "insmod wifi_2_0", keyword, 0, 10 * 1000);

                SendAndChk(PortType.TELNET, $"diag_socket_app -a {pc_IP} &", "logging switched", out res, 0, 30 * 1000);
                if (!res.Contains("Successful connect to address:"))
                {
                    DisplayMsg(LogType.Log, $"Connect to {pc_IP} fail");
                    AddData("EnterTestMode", 1);
                    return;
                }

                SendAndChk(PortType.TELNET, "/etc/init.d/ftm start", keyword, 0, 3000);
                SendAndChk(PortType.TELNET, "ftm -n -dd &", "Diag_LSM_Init succesful", 0, 3000);

                AddData("EnterTestMode", 0);
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData("EnterTestMode", 1);
            }
        }
        private void CheckCPUTemperature(int index)
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            try
            {
                string res = "";
                string keyword = "root@OpenWrt:/#";

                DisplayMsg(LogType.Log, "========= Check CPU zone0 temperature =========");
                SendAndChk(PortType.TELNET, "cat /sys/devices/virtual/thermal/thermal_zone0/temp", keyword, out res, 0, 3000);
                string temp = Regex.Match(res, @"/temp\r\n([-\d]+)").Groups[1].Value.Trim();
                DisplayMsg(LogType.Log, $"CPU temp : " + temp);
                status_ATS.AddDataLog("CPUTemp_" + index, temp, temp, "00000");
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
            }
        }
        private void RFTest_WiFi()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            DateTime dt;
            TimeSpan ts;
            LitePoint litePoint = null;
            string logPath = string.Empty;
            string _MAC = status_ATS.txtSP.Text;
            string keyword = "root@OpenWrt:/#";
            int timeOutMs = Convert.ToInt32(Func.ReadINI("Setting", "LitePoint", "TimeOutMs", "2000000"));
            int itemCount = Convert.ToInt32(Func.ReadINI("Setting", "Setting", "WiFi_Count", "-1"));

            #region litepoint parameter
            _LitePointFolder = Func.ReadINI("Setting", "LitePoint", "Path_WiFi", string.Empty);
            _LitePointTool = _LitePointFolder + @"\ATSuite.exe";
            _LitePointSummary = _LitePointFolder + @"\Log\log_summary.txt";
            _LitePointLog_all = _LitePointFolder + @"\Log\Log_All.csv";
            #endregion

            try
            {
                DisplayMsg(LogType.Log, "========= WiFi TX/RX Test =========");

                telnet.Dispose();
                DeleteLog_all();

                if (isGolden)
                {
                    _LitePointFolder = Func.ReadINI("Setting", "LitePoint", "Path_WiFi_Golden", string.Empty);
                    _LitePointTool = _LitePointFolder + @"\ATSuite.exe";
                    _LitePointSummary = _LitePointFolder + @"\Log\log_summary.txt";
                    _LitePointLog_all = _LitePointFolder + @"\Log\Log_All.csv";
                }
                try
                {
                    DisplayMsg(LogType.Log, "Delete: " + _LitePointFolder + "\\Log");
                    Directory.Delete(_LitePointFolder + "\\Log", true);
                }
                catch { }


                DisplayMsg(LogType.Log, "LitePoint Folder : " + _LitePointFolder);


                DisplayMsg(LogType.Log, "ATSuite path:" + _LitePointTool);
                litePoint = new LitePoint(_LitePointTool, _LitePointSummary);

                dt = DateTime.Now;

                if (litePoint.Start())
                {
                    if (litePoint.WaitResult(timeOutMs))
                    {
                        logPath = _LitePointLog_all;
                        try
                        {
                            ParsingLog_all_WiFi(logPath, itemCount);
                            ParseIqSummaryLog(_LitePointSummary);
                        }
                        catch (Exception e)
                        {
                            DisplayMsg(LogType.Exception, e.Message);
                            AddData("ParsingLog", 1);
                        }
                    }
                    else //沒有summary log的情況
                    {
                        ParseIqSummaryLog(_LitePointSummary);
                        AddData("ParsingLog", 1);
                    }
                }
                else
                {
                    DisplayMsg(LogType.Log, "Failed to start " + _LitePointTool);
                    AddData("Cal_Verify", 1);
                }

                ts = new TimeSpan(DateTime.Now.Ticks - dt.Ticks);
                DisplayMsg(LogType.Log, "WiFi test time : " + ts.TotalMilliseconds.ToString() + " (ms)");

                if (status_ATS.CheckListData().Count != 0)
                {
                    AddData("Cal_Verify", 1);
                }
                else
                {
                    AddData("Cal_Verify", 0);
                }

            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData("Cal_Verify", 1);
            }
            finally
            {
                if (litePoint != null)
                    litePoint.CloseTool();
                if (Directory.Exists(_LitePointFolder + @"\Log\FAIL\") && File.Exists(Directory.GetFiles(_LitePointFolder + @"\Log\FAIL\", "*.txt")[0]))
                    MoveRenameLog_all(Directory.GetFiles(_LitePointFolder + @"\Log\FAIL\", "*.txt")[0], "WiFi");
                else if (Directory.Exists(_LitePointFolder + @"\Log\PASS\") && File.Exists(Directory.GetFiles(_LitePointFolder + @"\Log\PASS\", "*.txt")[0]))
                    MoveRenameLog_all(Directory.GetFiles(_LitePointFolder + @"\Log\PASS\", "*.txt")[0], "WiFi");

                MoveRenameLog_all(_LitePointLog_all, "WiFi");
                MoveRenameLog_all(_LitePointSummary, "WiFi");
                DeleteLog_all();
            }
        }
        private bool ParseIqSummaryLog(string logPath)
        {
            try
            {
                DisplayMsg(LogType.Log, "ParseIqSummaryLog : \r\n" + logPath);

                if (!File.Exists(logPath))
                {
                    DisplayMsg(LogType.Error, "Not exists log file !!");
                    AddData("ParsingLog", 1);
                    return false;
                }

                string res = string.Empty;
                res = File.ReadAllText(logPath);
                DisplayMsg(LogType.Log, "Content:\r\n" + res);

                if (!res.Contains("PASS"))
                {
                    AddData("ATSuiteSummaryLog", 1);
                }
                else
                {
                    AddData("ATSuiteSummaryLog", 0);
                }
                return true;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData("ParsingLog", 1);
                return false;

            }
        }
        private bool ParsingLog_all_WiFi(string logPath, int itemCount)
        {
            try
            {
                DisplayMsg(LogType.Log, "Wifi Log path:" + logPath);

                if (!File.Exists(logPath))
                {
                    DisplayMsg(LogType.Error, "Not exists log file !!");
                    //status_ATS.AddDataLog("ParsingLog", NG);
                    AddData("ParsingLog", 1);
                    return false;
                }
                try
                {
                    if (File.Exists(logPath.Replace("csv", "txt")))
                    {
                        DisplayMsg(LogType.Log, logPath.Replace("csv", "txt"));
                        DisplayMsg(LogType.Log, File.ReadAllText(logPath.Replace("csv", "txt")));
                    }
                    else if (Directory.Exists(_LitePointFolder + @"\Log\PASS\"))
                    {
                        string file = Directory.GetFiles(_LitePointFolder + @"\Log\PASS\", "*.txt")[0];
                        DisplayMsg(LogType.Log, file);
                        DisplayMsg(LogType.Log, File.ReadAllText(file));
                    }
                    else if (Directory.Exists(_LitePointFolder + @"\Log\FAIL\"))
                    {
                        string file = Directory.GetFiles(_LitePointFolder + @"\Log\FAIL\", "*.txt")[0];
                        DisplayMsg(LogType.Log, file);
                        DisplayMsg(LogType.Log, File.ReadAllText(file));
                    }
                }
                catch
                { }

                string title = string.Empty;
                string temp = string.Empty;
                string res = string.Empty;
                string[] msg;
                string[] log;
                int item = 0;

                StreamReader Str = new StreamReader(logPath);
                res = Str.ReadToEnd();
                Str.Close();
                Str.Dispose();

                res = res.Trim().Replace("\r\n", "$");
                log = res.Split('$');

                string[] Title = res.Split('$');
                string[] Condition = res.Split('$');
                string[] Name = res.Split('$');
                string[] Value = res.Split('$');
                string[] Upper_Limit = res.Split('$');
                string[] Lower_Limit = res.Split('$');

                foreach (string s in log)
                {
                    if (s.StartsWith("Title"))
                    {
                        Title = null;
                        Title = s.Split(',');
                    }
                    else if (s.StartsWith("Condition"))
                    {
                        Condition = null;
                        Condition = s.Split(',');
                    }
                    else if (s.StartsWith("Name"))
                    {
                        Name = null;
                        Name = s.Split(',');
                    }
                    else if (s.StartsWith("Value"))
                    {
                        Value = null;
                        Value = s.Split(',');
                    }
                    else if (s.StartsWith("Upper_Limit"))
                    {
                        Upper_Limit = null;
                        Upper_Limit = s.Split(',');
                    }
                    else if (s.StartsWith("Lower_Limit"))
                    {
                        Lower_Limit = null;
                        Lower_Limit = s.Split(',');
                    }
                }

                for (int i = 0; i < Title.Length; i++)
                {
                    ///懷疑是Name split時沒有變成2個元素，所以index out of bound
                    string[] tokens = Name[i].Split('(', ')');

                    //string testitem = Title[i] + "_" + Condition[i].Replace(' ', '_') + tokens[0].Trim().Replace(' ', '_');
                    string testitem = Title[i].Replace("_VERIFY_ALL", "").Replace("_VERIFY_PER", "") + "_" + Condition[i].Replace(' ', '_') + tokens[0].Trim().Replace(' ', '_');
                    //if (testitem.StartsWith("WIFI_TX_VERIFY") || testitem.StartsWith("WIFI_RX_VERIFY"))
                    if (testitem.StartsWith("WIFI_"))
                    {
                        string unit = tokens.Length > 1 ? tokens[1] : "NA";
                        string errorCode = WNC.API.Func.ReadINI("SPEC", "Error_Code", testitem, "ABC00000");//getSPEC_INI("Error_Code", sfcsItem, "A00000");
                        if (true)
                        {
                            if (errorCode == "ABC00000")
                            {
                                string tmpUsl = string.Format("{0}_USL", testitem.Trim());
                                string tmplsl = string.Format("{0}_LSL", testitem.Trim());
                                WNC.API.Func.WriteINI("SPEC_PEadd", "SPEC", tmpUsl, Upper_Limit[i]);
                                WNC.API.Func.WriteINI("SPEC_PEadd", "SPEC", tmplsl, Lower_Limit[i]);
                                WNC.API.Func.WriteINI("SPEC_PEadd", "Error_Code", testitem, "XYZ00000");
                            }
                        }

                        //status_ATS.AddData(testitem.Trim(), unit, Lower_Limit[i], Upper_Limit[i], Value[i], errorCode);
                        status_ATS.AddData(testitem.Trim(), unit, Convert.ToDouble(Lower_Limit[i]), Convert.ToDouble(Upper_Limit[i]), Convert.ToDouble(Value[i]), errorCode);
                        item++;
                    }
                }

                DisplayMsg(LogType.Log, "item : " + item.ToString());
                return true;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                //status_ATS.AddDataLog("ParsingLog", NG);
                AddData("ParsingLog", 1);
                return false;

            }
        }
        private void MoveRenameLog_all(string logPath, string testType)
        {
            if (!File.Exists(logPath))
            {
                DisplayMsg(LogType.Error, "Not exists log file : " + logPath);
                return;
            }

            string NowYMD = DateTime.Now.ToString("yyyyMMdd");
            string NowYMDHMS = DateTime.Now.ToString("yyyyMMddHHmmss");
            if (!Directory.Exists(@"C:\BakupLitePointLog\" + NowYMD))
            {
                Directory.CreateDirectory(@"C:\BakupLitePointLog\" + NowYMD);
                System.Threading.Thread.Sleep(500);
            }

            if (Directory.Exists(@"C:\BakupLitePointLog\" + NowYMD))
            {
                if (status_ATS.CheckListData().Count != 0)
                {
                    File.Move(logPath, @"C:\BakupLitePointLog\" + NowYMD + @"\" + status_ATS.txtPSN.Text.Trim() + "_" + NowYMDHMS + "_" + testType + "_NG_" + Path.GetFileName(logPath));// + "_NG_Log_all.csv");
                }
                else
                {
                    File.Move(logPath, @"C:\BakupLitePointLog\" + NowYMD + @"\" + status_ATS.txtPSN.Text.Trim() + "_" + NowYMDHMS + "_" + testType + "_PASS_" + Path.GetFileName(logPath));
                }
            }
        }
        private void DeleteLog_all()
        {
            if (File.Exists(_LitePointSummary))
            {
                File.Delete(_LitePointSummary);
                DisplayMsg(LogType.Log, "Delete : " + _LitePointSummary);
            }
            if (File.Exists(_LitePointLog_all))
            {
                File.Delete(_LitePointLog_all);
                DisplayMsg(LogType.Log, "Delete : " + _LitePointLog_all);
            }
            if (Directory.Exists(_LitePointFolder + "\\Log\\PASS"))
            {
                Directory.Delete(_LitePointFolder + "\\Log\\PASS", true);
                DisplayMsg(LogType.Log, "Delete : " + _LitePointFolder + "\\Log\\PASS");
            }
            if (Directory.Exists(_LitePointFolder + "\\Log\\FAIL"))
            {
                Directory.Delete(_LitePointFolder + "\\Log\\FAIL", true);
                DisplayMsg(LogType.Log, "Delete : " + _LitePointFolder + "\\Log\\FAIL");
            }
        }
        private bool WriteMACtoDutSetup6G(string _MAC)//Write 6G MAC data
        {
            if (!CheckGoNoGo())
            {
                return false;
            }
            try
            {
                DisplayMsg(LogType.Log, "Base MAC :" + _MAC);

                if (!File.Exists(_LitePointLog_dutSetup_6G))
                {
                    DisplayMsg(LogType.Error, "Not exists 6G setup file !! " + _LitePointLog_dutSetup_6G);
                    AddData("Write6GMAC", 1);
                    return false;
                }

                //WiFi 6G MAC = Base MAC + 10
                long intValue = long.Parse(_MAC.Replace(":", ""), System.Globalization.NumberStyles.HexNumber);
                string wifi6GMac = (intValue + 10).ToString("X").PadLeft(12, '0');
                wifi6GMac = ConvertStringToMACFormat(wifi6GMac).Replace(":", "");
                DisplayMsg(LogType.Log, "Wifi6G MAC from DUT:" + wifi6GMac);
                if (status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode)
                {
                    SFCS_Query sfcs = new SFCS_Query();
                    sfcs.Get15Data(_MAC, "LCS5_ATH2_6G_MAC", ref MAC6G);
                    MAC6G = MAC6G.Replace(":", "").ToUpper();
                    DisplayMsg(LogType.Log, "Wifi6G MAC from SFCS:" + MAC6G);
                    if (MAC6G.Length != 12 || wifi6GMac != MAC6G)
                    {
                        DisplayMsg(LogType.Log, "Wifi6G MAC from SFCS different DUT Wifi 6G MAC");
                        AddData("Write6GMAC", 1);
                        return false;
                    }
                }
                else
                {
                    MAC6G = wifi6GMac.Replace(":", "");
                }

                DisplayMsg(LogType.Log, "Wifi6G MAC write to DUT:" + MAC6G);
                string res = string.Empty;
                //string testfile = _LitePointLog_dutSetup_6G +"_test";
                string content;
                string newContent = string.Empty;
                StreamReader strR = new StreamReader(_LitePointLog_dutSetup_6G);

                while ((content = strR.ReadLine()) != null)
                {
                    if (content.Contains("LocalMac1"))
                    {
                        newContent = newContent + "LocalMac1                        = " + MAC6G + "\r\n";
                        DisplayMsg(LogType.Log, content);
                    }
                    else if (content.Contains("SetLocalMac"))
                    {
                        newContent = newContent + "SetLocalMac                        = 1" + "\r\n";
                        DisplayMsg(LogType.Log, content);
                    }
                    else
                    {
                        newContent = newContent + content + "\r\n";
                    }
                }
                strR.Close();
                StreamWriter strW = new StreamWriter(_LitePointLog_dutSetup_6G);
                strW.Write(newContent);
                strW.Flush();
                strW.Close();
                AddData("Write6GMAC", 0);
                return true;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData("Write6GMAC", 1);
                return false;
            }
        }
        private string ConvertStringToMACFormat(string myMAC)
        {
            string mac = "";
            if (myMAC.Length == 12)
            {
                mac += myMAC.Substring(0, 2) + ":";
                mac += myMAC.Substring(2, 2) + ":";
                mac += myMAC.Substring(4, 2) + ":";
                mac += myMAC.Substring(6, 2) + ":";
                mac += myMAC.Substring(8, 2) + ":";
                mac += myMAC.Substring(10, 2);
            }
            return mac;
        }
        private string CheckCalibrationData()
        {
            string comparMd5sum = string.Empty;
            if (!CheckGoNoGo())
            {
                return "";
            }

            try
            {
                string res = "";
                string keyword = "root@OpenWrt:/#";

                DisplayMsg(LogType.Log, "========= Dump 2G/5G calibration data =========");
                SendAndChk(PortType.TELNET, "hexdump -s 0x1000 -n 16 /dev/mmcblk0p13", keyword, out res, 0, 5000);
                DisplayMsg(LogType.Log, "SHOWres ===> " + res);
                //if (res.Contains("0000 0000 0000 0000") || res.Contains("ffff ffff ffff ffff"))  //0001010
                //{
                //    DisplayMsg(LogType.Log, "Check WiFi Calibration Data fail");
                //    AddData("CheckCalibrationData", 1);
                //    return;
                //}
                if (res.Contains("0000 0000 0000 0000"))  //0001010
                {
                    DisplayMsg(LogType.Log, "Check WiFi Calibration Data fail");
                    AddData("CheckCalibrationData", 1);
                    return "";
                }
                if (res.Contains("ffff ffff ffff ffff"))  //0001010
                {
                    DisplayMsg(LogType.Log, "Check WiFi Calibration Data fail");
                    AddData("CheckCalibrationData", 1);
                    return "";
                }
                AddData("CheckCalibrationData", 0);
                SendAndChk(PortType.TELNET, "cat /dev/mmcblk0p13 | md5sum", keyword, out res, 0, 3000);
                if (res != null)
                {
                    comparMd5sum = res;
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData("CheckCalibrationData", 1);
            }
            return comparMd5sum;
        }
        private void Check6GMAC()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            try
            {
                string res = "";
                string keyword = "root@OpenWrt:/#";

                DisplayMsg(LogType.Log, "========= Check 6G MAC address =========");
                //SendAndChk(PortType.TELNET, "hexdump -v -s 0x26800 -n 100 /dev/mmcblk0p17", keyword, out res, 0, 5000);
                SendAndChk(PortType.TELNET, "hexdump -v -s 0x26800 -n 20 /dev/mmcblk0p17", keyword, out res, 0, 5000); //只顯示20 bytes

                var regex = "(.{2})(.{2})(.{2})(.{2})(.{2})(.{2})";
                string _6gmacPath1 = Regex.Replace(MAC6G.Replace(":", ""), regex, "$2$1").ToLower();
                string _6gmacPath2 = Regex.Replace(MAC6G.Replace(":", ""), regex, "$4$3").ToLower();
                string _6gmacPath3 = Regex.Replace(MAC6G.Replace(":", ""), regex, "$6$5").ToLower();

                if (!res.Contains(_6gmacPath1) || !res.Contains(_6gmacPath2) || !res.Contains(_6gmacPath3))
                {
                    DisplayMsg(LogType.Log, $"Check '{_6gmacPath1}' or '{_6gmacPath2}' or '{_6gmacPath3}' fail");
                    AddData("Check6GMAC", 1);
                }
                else
                {
                    DisplayMsg(LogType.Log, $"Check '{_6gmacPath1}' & '{_6gmacPath2}' & '{_6gmacPath3}' pass");
                    AddData("Check6GMAC", 0);
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData("Check6GMAC", 1);
            }
        }
    }
}
