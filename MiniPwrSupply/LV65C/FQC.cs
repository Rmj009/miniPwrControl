using ATS_Template;
using System;
using System.Diagnostics;
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
using static StatusUI2.StatusUI;
using AtsHelper.CommandConsole;

namespace MiniPwrSupply.LV65C
{

    public class MyBlocks //20240313 Jason add for FQC Badblock check
    {
        public int startAddr { get; set; }
        public int endAddr { get; set; }
        public string partitionName { get; set; }
        public int maxBlocks { get; set; }
        public int BlockCount { get; set; }
    }

    public class Generator_BlockSpecType
    {
        public enum BlockSpecType
        {
            Default
        }
        public Dictionary<int, string> GeneratorBlockSpecType(int count)
        {
            var blockSpecTypes = new Dictionary<int, string>();
            for (int i = 0; i < count; i++)
            {
                blockSpecTypes.Add(i, "Type" + i);
            }
            return blockSpecTypes;
        }
    }
    public partial class FQC
    {
        YNConfirm okfrm = new YNConfirm(YNConfirm.Type.OK);
        YNConfirm ynfrm = new YNConfirm(YNConfirm.Type.YESNO);
        MyBlocks[] blockSpec = new MyBlocks[34]; // __JASON
        private void FQC_Function()
        {
            try
            {
                if (!CheckGoNoGo())
                    return;
                CheckAndEnableTools(Func.ReadINI("Setting", "Tftp", "Path", "C://TFTP//tftpd64.exe"));
                if (!RamBOOT())
                    return;

                Function();

                #region 15.2	CRSP Firmware Upgrade and CRSP FW switch (Optional)
                //string name = Func.ReadINI("Setting", "SFCS", "LV65C_81_SW_VER", "@LV65C_81_SW_VER_18");
                //string customerFW = "(*&^%";
                //string res = "";
                //GetFromSfcs(name, out customerFW);
                //customerFW = customerFW.Substring(0, customerFW.Length - 11);

                //SendAndChk(PortType.TELNET, "cat /proc/boot_info/rootfs/primaryboot", "#", out res, 0, 30000);
                //if (res.Contains("0"))
                //{
                //    DisplayMsg(LogType.Log, "============== Current Partition: 0 ==============");
                //    SendAndChk(PortType.TELNET, $"crspenv set FW_VER1={customerFW}", "#", out res, 0, 30000);
                //    SendAndChk(PortType.TELNET, "crspenv commit", "#", out res, 0, 30000);
                //    if (!SendAndChk(PortType.TELNET, "crspenv show", $"FW_VER1={customerFW}", out res, 0, 30000))
                //    {
                //        DisplayMsg(LogType.Log, "Write FW_VER1 fail");
                //        AddData("FW_VER1", 1);
                //        return;
                //    }
                //    DisplayMsg(LogType.Log, "Write FW_VER1 ok");
                //    //goto Current_Partition_0;

                //}
                //else if (res.Contains("1"))
                //{
                //    DisplayMsg(LogType.Log, "============== Current Partition: 1 ==============");
                //    SendAndChk(PortType.TELNET, $"crspenv set FW_VER0={customerFW}", "#", out res, 0, 30000);
                //    SendAndChk(PortType.TELNET, "crspenv commit", "#", out res, 0, 30000);
                //    if (!SendAndChk(PortType.TELNET, $"crspenv show", $"FW_VER0={customerFW}", out res, 0, 30000))
                //    {
                //        DisplayMsg(LogType.Log, "Write FW_VER0 fail");
                //        AddData("FW_VER1", 1);
                //        return;
                //    }
                //    DisplayMsg(LogType.Log, "Write FW_VER0 ok");
                //    //goto Current_Partition_1;
                //}
                //else
                //{
                //    warning = "Check Current Partition Fail";
                //    return;
                //}

                #endregion

                #region 15.6	 Firmware Upgrade and Switch
                //if (!CheckGoNoGo())
                //    return;
                //if (!SendAndChk(PortType.TELNET, "", "tmp#", 0, 2000))
                //    SendAndChk(PortType.TELNET, "cd /tmp/", "tmp#", 0, 3000);

                //SendAndChk(PortType.TELNET, $"tftp -gr hdr_removed_titan3_fw_{customerFW}.bin 192.168.1.100", "tmp#", 1000, 80000);
                //SendAndChk(PortType.TELNET, $"md5sum hdr_removed_titan3_fw_{customerFW}.bin", "tmp#", out res, 1000, 10000);
                //string fwlocalMD5 = "(&^";
                //CalculateMD5(fwPath, ref fwlocalMD5);
                //string fwlocalMD5_Setting = Func.ReadINI("Setting", "LV65C", "FW_MD5", "(*&^");
                //DisplayMsg(LogType.Log, $"{fwPath} MD5 Setting:{fwlocalMD5_Setting}");
                //DisplayMsg(LogType.Log, $"Local file {fwPath} MD5:{fwlocalMD5}");
                //if (fwlocalMD5.Length != 32)
                //{
                //    warning = "FW MD5 length must equal 32 characters";
                //    return;
                //}
                //if (!res.Contains(fwlocalMD5) || !res.Contains(fwlocalMD5_Setting))
                //{
                //    warning = "Compare FW MD5 fail";
                //    return;
                //}
                //DisplayMsg(LogType.Log, "Compare FW MD5 ok");
                //AddData("Chk_FW_MD5", 0);

                //if (!SendAndChk(PortType.TELNET, $"sysupgrade -i hdr_removed_titan3_fw_{customerFW}.bin", "Y/n", 100, 3000))
                //{
                //    warning = "Check keyword 'Y/n' fail";
                //    return;
                //}
                //if (!SendAndChk(PortType.TELNET, "n", "#", 100, 3000))
                //{
                //    warning = "Check keyword 'Y/n' fail";
                //    return;
                //}

                //SendCommand(PortType.TELNET, "reboot -f", 1000);

                //DisplayMsg(LogType.Log, "Delay 400s");
                //Thread.Sleep(400 * 1000);

                #endregion
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Log, ex.ToString());
                warning = "Exception";
            }
            finally
            {

                UartDispose(atCmdUart);
                /*
                if (Func.ReadINI("Setting", "IO_Board_Control", "IO_Control_1", "0") == "1")
                {
                    string txPin = Func.ReadINI("Setting", "IO_Board_Control", "Pin0", "0"); // dut
                    string rev_message = "";
                    status_ATS.AddLog("IO_Board_Y" + txPin + " Off...");
                    IO_Board_Control1.ConTrolIOPort_write(Int32.Parse(txPin), "2", ref rev_message);
                    DisplayMsg(LogType.Log, rev_message);
                }
                */
                SwitchRelay(CTRL.ON);

            }
        }

        private bool CheckADBDevice(int timeout = 50)
        {
            string devPortDesc = Func.ReadINI("Setting", "Setting", "PortDesc", "WNC M18Q2");
            // EzTimer timer = new EzTimer();
            //timer.Restart();
            DateTime DT = DateTime.Now;
        Loop:
            if (IsDeviceReady() == true)
            {
                DisplayMsg(LogType.Log, "Device port exist");
                // AddData("Reboot", 0);
                return true;
            }
            else
            {
                DisplayMsg(LogType.Log, "Device port doesn't exist");
                if ((DateTime.Now - DT).TotalSeconds > timeout)
                {
                    AddData("Reboot", 1);
                    return false;
                }
                Thread.Sleep(5000);
                goto Loop;
            }
        }
        private bool FindDeviceWithComportsFQC(string DeviceName, string ComportNameLoader, string ComportNameDiag, ref string res, int DeviceTime)
        {
            CommandConsole myccc = new CommandConsole();
            myccc.Start();
            string lastBuffer = "";
            for (int i = 1; i < DeviceTime; i++)
            {
                res = "";
                status_ATS.AddLog("Finding Interface " + DeviceName);

                Process myProcess = new Process();
                myProcess.StartInfo.FileName = "devcon.exe";
                myProcess.StartInfo.Arguments = "find " + DeviceName;
                myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                myProcess.StartInfo.UseShellExecute = false;
                myProcess.StartInfo.RedirectStandardInput = false;
                myProcess.StartInfo.RedirectStandardOutput = true;
                myProcess.StartInfo.CreateNoWindow = true;
                myProcess.StartInfo.Verb = "runas"; // Jason add run as admin
                myProcess.Start();
                myProcess.WaitForExit();
                Thread.Sleep(2000);

                res = myProcess.StandardOutput.ReadToEnd();
                if (res != lastBuffer)
                {
                    status_ATS.AddLog(res);
                    lastBuffer = res;
                }

                if (!res.Contains("No matching devices found"))
                {
                    bool foundLoader = res.Contains(ComportNameLoader);
                    bool foundDiag = res.Contains(ComportNameDiag);

                    if (foundLoader || foundDiag)
                    {
                        if (foundLoader)
                        {
                            status_ATS.AddLog("Find " + ComportNameLoader + " Pass");
                        }
                        else if (foundDiag)
                        {
                            status_ATS.AddLog("Found out" + ComportNameDiag + " -> Go To set command SW port to " + ComportNameLoader);
                            DisplayMsg(LogType.Log, "Diagnostics port found. This is reflow test!!");
                            DisplayMsg(LogType.Log, "Switch to QDLoader state ...");
                            DisplayMsg(LogType.Log, "adb root");
                            Send_Res_CC(myccc, "adb root && echo", "ECHO", out res);
                            CheckADBDevice();
                            DisplayMsg(LogType.Log, "adb reboot edl");
                            Send_Res_CC(myccc, "adb reboot edl && echo", "ECHO", out res, 2000);
                            DisplayMsg(LogType.Log, "Sleep 10 seconds to be ready");
                            Thread.Sleep(10 * 1000);
                        }
                        return true;
                    }
                }
            }

            status_ATS.AddLog("Find " + ComportNameLoader + " & " + ComportNameDiag + " NG");
            return false;
        }
        private void FQC_check_bad_block()
        {
            if (!CheckGoNoGo())
                return;
            string res = "";
            // CommandConsole mycc = new CommandConsole();
            try
            {
                string SN = status_ATS.txtPSN.Text;
                DisplayMsg(LogType.Log, $"SN Input: '{SN}'");
                SetTextBox(status_ATS.txtPSN, SN);
                status_ATS.SFCS_Data.PSN = SN;
                status_ATS.SFCS_Data.First_Line = SN;
                if (!ChkLinux(PortType.TELNET, 120000))
                {
                    AddData("Login", 1);
                    return;
                }
                string deviceName = string.Empty;
                bool chkDevice = false;
                for (int i = 0; i < 30; i++)
                {
                    SendAndChk(PortType.TELNET, "adb devices", "#", out res, 300, 10000);
                    Match m = Regex.Match(res, @"\n([^\s]+)\s*device");
                    if (m.Success)
                    {
                        DisplayMsg(LogType.Log, "Captured device:" + m.Groups[1].Value.ToString());
                        deviceName = m.Groups[1].Value.ToString();
                        chkDevice = true;
                        break;

                    }
                    DisplayMsg(LogType.Log, $"Watting check adb device...");
                    Thread.Sleep(1000);
                }

                if (!chkDevice)
                {
                    AddData("DUT_USB", 1);
                    return;
                }
                DisplayMsg(LogType.Log, $"Check USB device ok found device is '{deviceName}'");

                //Send_Res_CC(mycc, "killall -9 modem-monitor", "", out res);
                SendAndChk(PortType.TELNET, "echo 56 > /sys/class/gpio/export", "", out res, 0, 10000);
                SendAndChk(PortType.TELNET, "echo out > /sys/class/gpio/gpio56/direction", "", out res, 0, 10000);
                SendAndChk(PortType.TELNET, "echo 1 > /sys/class/gpio/gpio56/value", "", out res, 0, 10000);

                #region USB download Test
                #region Find download device
                //*QLoader port時 device不會出現adb device
                string sDownload_DeviceName = Func.ReadINI(Application.StartupPath, "Setting", "TestConfig", "Download_DeviceName", @"USB\VID_05C6*");
                string sQCom_DeviceNameLoader = Func.ReadINI(Application.StartupPath, "Setting", "TestConfig", "sQCom_DeviceNameLoader", "Qualcomm HS-USB QDLoader 9008");
                string sQCom_DeviceNameDiag = Func.ReadINI(Application.StartupPath, "Setting", "TestConfig", "sQCom_DeviceNameDiag", "Qualcomm HS-USB Diagnostics 90DB");
                int FindDeviceTimeout = Convert.ToInt32(Func.ReadINI(Application.StartupPath, "Setting", "TestConfig", "FindDeviceTimeout", "30"));
                int usbretrycount = 0;
            retryusbfind:
                if (FindDeviceWithComportsFQC(sDownload_DeviceName, sQCom_DeviceNameLoader, sQCom_DeviceNameDiag, ref res, FindDeviceTimeout))
                {
                    status_ATS.AddLog("Find 90DB & 9008 Pass");
                }
                else
                {
                    status_ATS.AddLog("Failed to find 90DB & 9008");
                    if (usbretrycount < 3)
                    {
                        usbretrycount++;
                        status_ATS.AddLog($"Re-try Find Device time {usbretrycount}...");

                        //modify by jennis 20231026 #27916------->
                        if (useFixture)
                        {
                            fixture.ControlIO(Fixture.FixtureIO.IO_2, CTRL.OFF);
                            Thread.Sleep(2000);
                            fixture.ControlIO(Fixture.FixtureIO.IO_2, CTRL.ON);
                            Thread.Sleep(2000);
                        }
                        else
                        {
                            MessageBox.Show("Find USB Device fail/ reboot DUT to do retry process ..."); // Jason add
                        }
                        goto retryusbfind;
                    }
                    AddData("FindUSBDevice", 1);
                    return;
                }

                if (CheckGoNoGo())
                {
                    AddData("USB", 0);
                }
                else
                {
                    AddData("USB", 1);
                    return;
                }
                #endregion'Find download device
                #endregion USB download Test
                string sFwFolder = WNC.API.Func.ReadINI("Setting", "FirehoseFW", "FwFolder", "E:\\LV65C\\Bad block\\check_bad_block");
                string MD5Result = string.Empty;

                #region Firehose download FW
                int iPromptNum = Convert.ToInt32(WNC.API.Func.ReadINI("Setting", "FirehoseFW", "PromptNum", "24"));
                string sBatName2 = WNC.API.Func.ReadINI("Setting", "FirehoseFW", "BatName", "check_bad_block-all_IMQE.bat");
                string sLogName = WNC.API.Func.ReadINI("Setting", "FirehoseFW", "BatLogName", "port_trace.txt");
                string resultlog = WNC.API.Func.ReadINI("Setting", "FirehoseFW", "resultlog", "result.txt");
                string sQCom_ComportQLoader = WNC.API.Func.ReadINI("Setting", "FirehoseFW", "QLoaderCOM", "COM4");
                int iDelayFWDownload = Convert.ToInt16(WNC.API.Func.ReadINI("Setting", "FirehoseFW", "FWDownloadDelayTime", "20"));
                status_ATS.AddLog("Kill QSaharaServer");
                KillTaskProcess("QSaharaServer");
                Thread.Sleep(1000);
                Directory.SetCurrentDirectory(sFwFolder);
                #endregion Firehose download FW

                #region Delete Old Log
                if (File.Exists(sLogName))
                {
                    status_ATS.AddLog($"Delete old sBatLog '{sLogName}'");
                    File.Delete(sLogName);
                    Thread.Sleep(100);
                }

                if (File.Exists(resultlog))
                {
                    status_ATS.AddLog($"Delete old resultlog '{resultlog}'");
                    File.Delete(resultlog);
                    Thread.Sleep(100);
                }
                #endregion Delete Old Log

                #region Check bad block
                status_ATS.AddLog(".......Start Check Bad Block.......");
                ProcessStartInfo ps = new ProcessStartInfo(sBatName2, sQCom_ComportQLoader);
                Process.Start(ps);
                string sReciveBat = "";
                status_ATS.AddLog("Delay " + iDelayFWDownload + "s...");
                Thread.Sleep(iDelayFWDownload * 1000);

                //先等 fh_loader.exe 結束再parse port_trace.txt
                DateTime dt = DateTime.Now;
                while (dt.AddSeconds(150) > DateTime.Now) //download約136秒
                {
                    if (Process.GetProcessesByName("fh_loader").Length > 0)
                        continue;

                    sReciveBat = "";
                    try
                    {
                        if (File.Exists(sLogName))
                        {
                            status_ATS.AddLog($"Check Badblock process is done found '{sLogName}'");
                            break;
                        }
                        else
                            Thread.Sleep(1000);
                    }
                    catch (Exception ex)
                    {
                        status_ATS.AddLog("read fw log Error:" + ex.Message);
                        AddData("BadBlock", 1);
                        return;
                    }
                }

                KillTaskProcess("cmd");
                Thread.Sleep(1000);
                sReciveBat = string.Empty;
                if (File.Exists(sLogName))
                {
                    status_ATS.AddLog("*********trace Log*********");
                    sReciveBat = File.ReadAllText(sLogName);
                    status_ATS.AddLog(sReciveBat);
                    status_ATS.AddLog("*********trace Log*********");
                    status_ATS.AddLog("Start Analyze FWLog");
                    if (sReciveBat.Contains("All Finished Successfully"))
                    {
                        // AddData("FWDownload", 0);
                        status_ATS.AddLog("Check 'All Finished Successfully' ok! ");
                    }
                    else
                    {
                        AddData("BadBlock", 1);
                        return;
                    }
                }
                else
                {
                    status_ATS.AddLog("Can not Find " + sLogName);
                    AddData("BadBlock", 1);
                    return;
                }

                sReciveBat = string.Empty;
                if (File.Exists(resultlog))
                {
                    #region Handle Value
                    string value = string.Empty;
                    List<string> BadblockList = new List<string>();
                    BadblockList.Clear();
                    //load_BadBlock_Spec_Array(); //Load spec for badblock
                    this.LoadBlockSpecArray(); //Load spec for badblock

                    sReciveBat = File.ReadAllText(resultlog);
                    status_ATS.AddLog("*********Result Log*********");
                    status_ATS.AddLog(sReciveBat);
                    status_ATS.AddLog("*********Result Log*********");
                    if (sReciveBat.Contains("Bad block number = 0"))
                    {
                        status_ATS.AddLog($"→ Total Bad block quantity = '0'");
                        for (int h = 0; h < blockSpec.Length; h++) //Show Pass List
                        {
                            DisplayMsg(LogType.Log, $"[Path Name] '[{blockSpec[h].partitionName}]' ['{blockSpec[h].startAddr}'~'{blockSpec[h].endAddr}']: 0 < '{blockSpec[h].maxBlocks.ToString()}' PASS");
                        }
                        status_ATS.AddLog("Check Bad Block PASS");
                        AddData("CheckBad_Block", 0);
                    }
                    else
                    {
                        Match Totalcount = Regex.Match(sReciveBat, @"Bad block number = (\d+)");
                        if (Totalcount.Success)
                        {
                            DisplayMsg(LogType.Log, $"→ Total Bad block quantity = '{Totalcount.Groups[1].Value.ToString()}'");
                        }
                        else
                        {
                            warning = "Capture Badblock leng FAIL!!!"; status_ATS.AddLog($"Capture Badblock leng FAIL!!!"); return;
                        }

                        MatchCollection Badblockct = Regex.Matches(sReciveBat, @"Bad block : (\w+)");
                        foreach (Match BadblValue in Badblockct)
                        {
                            value = BadblValue.Groups[1].Value;
                            status_ATS.AddLog($"Add [BadBlock]: '{value}'");
                            BadblockList.Add(value);
                        }

                        if (Handle_Badblock_Value(BadblockList.ToArray()))
                        {
                            status_ATS.AddLog("Check Bad Block PASS");
                            AddData("CheckBad_Block", 0);
                        }
                        else
                        {
                            status_ATS.AddLog("Check Bad Block Fail");
                            AddData("CheckBad_Block", 1);
                            return;
                        }
                    }
                    #endregion Handle Value
                }
                else
                {
                    status_ATS.AddLog("Can not Find " + resultlog);
                    AddData("FWDownload", 1);
                    return;
                }
                #endregion Check bad block

                if (CheckGoNoGo() == true)
                {
                    AddData("BadBlock", 0);
                }
                else
                {
                    AddData("BadBlock", 1);
                }

            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Log, ex.ToString());
                warning = "Exception";
                return;
            }
            finally
            {
                //if (!mycc.IsProcessExit())
                // mycc.Close();
                status_ATS.AddLog("Kill QSaharaServer");
                KillTaskProcess("QSaharaServer");
                status_ATS.AddLog("Kill fh_loader");
                KillTaskProcess("fh_loader");
                Directory.SetCurrentDirectory(Application.StartupPath);
            }
        }

        private bool Handle_Badblock_Value(string[] valueinput)
        {
            try
            {
                int[] blockCount = new int[blockSpec.Length];
                for (int j = 0; j < blockSpec.Length; j++) //Reset Count
                {
                    blockCount[j] = 0;
                }

                foreach (string value in valueinput)
                {
                    int BlockValue = Convert.ToInt32(value, 16); // Convert each string from Hexadecimal to Decimal
                    for (int i = 0; i < blockSpec.Length; i++)
                    {
                        if (BlockValue >= blockSpec[i].startAddr && BlockValue <= blockSpec[i].endAddr)
                        {
                            DisplayMsg(LogType.Log, $"[Value] '{BlockValue.ToString()}': in block '{blockSpec[i].partitionName}'");
                            DisplayMsg(LogType.Log, $"[Bad block Range]: from '{blockSpec[i].startAddr.ToString()}' to '{blockSpec[i].endAddr.ToString()}'");
                            blockCount[i]++;
                        }

                        if (blockCount[i] > blockSpec[i].maxBlocks)
                        {
                            DisplayMsg(LogType.Log, $"[Max Bad block of path '{blockSpec[i].partitionName}']: {blockCount[i]} > '{blockSpec[i].maxBlocks.ToString()}' NG");
                            AddData("Bad_Block_" + i.ToString(), 1);
                        }
                    }
                }

                for (int h = 0; h < blockSpec.Length; h++) //Show Pass List
                {
                    if (blockCount[h] < blockSpec[h].maxBlocks || blockCount[h] == blockSpec[h].maxBlocks)
                    {
                        DisplayMsg(LogType.Log, $"[Path Name] '[{blockSpec[h].partitionName}]' ['{blockSpec[h].startAddr}'~'{blockSpec[h].endAddr}']: {blockCount[h]} < '{blockSpec[h].maxBlocks.ToString()}' PASS");
                    }
                }

                if (!CheckGoNoGo())
                {
                    DisplayMsg(LogType.Log, "Check Bad Block Fail");
                    AddData("Bad_Blockck", 1);
                    return false;
                }
                else
                {
                    DisplayMsg(LogType.Log, "Check Bad Block Pass");
                    return true;
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                return false;
            }
        }

        public void VN_load_BadBlock_Spec_Array() // __JASON
        {
            #region Load Block
            status_ATS.AddLog($"Block Spec Leng: {blockSpec.Length.ToString()}");
            for (int i = 0; i < blockSpec.Length; i++) //20240313 Jason add for FQC Badblock check
            {
                blockSpec[i] = new MyBlocks();
                if (i == 0)
                {
                    blockSpec[i].startAddr = 0;
                    blockSpec[i].endAddr = 9;
                    blockSpec[i].partitionName = "mtd0";
                    blockSpec[i].maxBlocks = 3;
                }
                else if (i == 1)
                {
                    blockSpec[i].startAddr = 10;
                    blockSpec[i].endAddr = 19;
                    blockSpec[i].partitionName = "mtd1";
                    blockSpec[i].maxBlocks = 7;
                }
                else if (i == 2)
                {
                    blockSpec[i].startAddr = 20;
                    blockSpec[i].endAddr = 63;
                    blockSpec[i].partitionName = "mtd2";
                    blockSpec[i].maxBlocks = 22;
                }
                else if (i == 3)
                {
                    blockSpec[i].startAddr = 64;
                    blockSpec[i].endAddr = 69;
                    blockSpec[i].partitionName = "mtd3";
                    blockSpec[i].maxBlocks = 0;
                }
                else if (i == 4)
                {
                    blockSpec[i].startAddr = 70;
                    blockSpec[i].endAddr = 72;
                    blockSpec[i].partitionName = "mtd4";
                    blockSpec[i].maxBlocks = 0;
                }
                else if (i == 5)
                {
                    blockSpec[i].startAddr = 73;
                    blockSpec[i].endAddr = 77;
                    blockSpec[i].partitionName = "mtd5";
                    blockSpec[i].maxBlocks = 5;
                }
                else if (i == 6)
                {
                    blockSpec[i].startAddr = 78;
                    blockSpec[i].endAddr = 80;
                    blockSpec[i].partitionName = "mtd6";
                    blockSpec[i].maxBlocks = 0;
                }
                else if (i == 7)
                {
                    blockSpec[i].startAddr = 81;
                    blockSpec[i].endAddr = 83;
                    blockSpec[i].partitionName = "mtd7";
                    blockSpec[i].maxBlocks = 0;
                }
                else if (i == 8)
                {
                    blockSpec[i].startAddr = 84;
                    blockSpec[i].endAddr = 86;
                    blockSpec[i].partitionName = "mtd8";
                    blockSpec[i].maxBlocks = 0;
                }
                else if (i == 9)
                {
                    blockSpec[i].startAddr = 87;
                    blockSpec[i].endAddr = 89;
                    blockSpec[i].partitionName = "mtd9";
                    blockSpec[i].maxBlocks = 0;
                }
                else if (i == 10)
                {
                    blockSpec[i].startAddr = 90;
                    blockSpec[i].endAddr = 92;
                    blockSpec[i].partitionName = "mtd10";
                    blockSpec[i].maxBlocks = 0;
                }
                else if (i == 11)
                {
                    blockSpec[i].startAddr = 93;
                    blockSpec[i].endAddr = 95;
                    blockSpec[i].partitionName = "mtd11";
                    blockSpec[i].maxBlocks = 0;
                }
                else if (i == 12)
                {
                    blockSpec[i].startAddr = 96;
                    blockSpec[i].endAddr = 98;
                    blockSpec[i].partitionName = "mtd12";
                    blockSpec[i].maxBlocks = 0;
                }
                else if (i == 13)
                {
                    blockSpec[i].startAddr = 99;
                    blockSpec[i].endAddr = 101;
                    blockSpec[i].partitionName = "mtd13";
                    blockSpec[i].maxBlocks = 0;
                }
                else if (i == 14)
                {
                    blockSpec[i].startAddr = 102;
                    blockSpec[i].endAddr = 110;
                    blockSpec[i].partitionName = "mtd14";
                    blockSpec[i].maxBlocks = 0;
                }
                else if (i == 15)
                {
                    blockSpec[i].startAddr = 111;
                    blockSpec[i].endAddr = 115;
                    blockSpec[i].partitionName = "mtd15";
                    blockSpec[i].maxBlocks = 5;
                }
                else if (i == 16)
                {
                    blockSpec[i].startAddr = 116;
                    blockSpec[i].endAddr = 121;
                    blockSpec[i].partitionName = "mtd16";
                    blockSpec[i].maxBlocks = 6;
                }
                else if (i == 17)
                {
                    blockSpec[i].startAddr = 122;
                    blockSpec[i].endAddr = 192;
                    blockSpec[i].partitionName = "mtd17";
                    blockSpec[i].maxBlocks = 6;
                }
                else if (i == 18)
                {
                    blockSpec[i].startAddr = 193;
                    blockSpec[i].endAddr = 263;
                    blockSpec[i].partitionName = "mtd18";
                    blockSpec[i].maxBlocks = 6;
                }
                else if (i == 19)
                {
                    blockSpec[i].startAddr = 264;
                    blockSpec[i].endAddr = 266;
                    blockSpec[i].partitionName = "mtd19";
                    blockSpec[i].maxBlocks = 3;
                }
                else if (i == 20)
                {
                    blockSpec[i].startAddr = 267;
                    blockSpec[i].endAddr = 269;
                    blockSpec[i].partitionName = "mtd20";
                    blockSpec[i].maxBlocks = 0;
                }
                else if (i == 21)
                {
                    blockSpec[i].startAddr = 270;
                    blockSpec[i].endAddr = 642;
                    blockSpec[i].partitionName = "mtd21";
                    blockSpec[i].maxBlocks = 48;
                }
                else if (i == 22)
                {
                    blockSpec[i].startAddr = 643;
                    blockSpec[i].endAddr = 1015;
                    blockSpec[i].partitionName = "mtd22";
                    blockSpec[i].maxBlocks = 48;
                }
                else if (i == 23)
                {
                    blockSpec[i].startAddr = 1016;
                    blockSpec[i].endAddr = 1021;
                    blockSpec[i].partitionName = "mtd23";
                    blockSpec[i].maxBlocks = 6;
                }
                else if (i == 24)
                {
                    blockSpec[i].startAddr = 1022;
                    blockSpec[i].endAddr = 1026;
                    blockSpec[i].partitionName = "mtd24";
                    blockSpec[i].maxBlocks = 5;
                }
                else if (i == 25)
                {
                    blockSpec[i].startAddr = 1027;
                    blockSpec[i].endAddr = 1029;
                    blockSpec[i].partitionName = "mtd25";
                    blockSpec[i].maxBlocks = 0;
                }
                else if (i == 26)
                {
                    blockSpec[i].startAddr = 1030;
                    blockSpec[i].endAddr = 1032;
                    blockSpec[i].partitionName = "mtd26";
                    blockSpec[i].maxBlocks = 0;
                }
                else if (i == 27)
                {
                    blockSpec[i].startAddr = 1033;
                    blockSpec[i].endAddr = 1037;
                    blockSpec[i].partitionName = "mtd27";
                    blockSpec[i].maxBlocks = 5;
                }
                else if (i == 28)
                {
                    blockSpec[i].startAddr = 1038;
                    blockSpec[i].endAddr = 1058;
                    blockSpec[i].partitionName = "mtd28";
                    blockSpec[i].maxBlocks = 3;
                }
                else if (i == 29)
                {
                    blockSpec[i].startAddr = 1059;
                    blockSpec[i].endAddr = 1063;
                    blockSpec[i].partitionName = "mtd29";
                    blockSpec[i].maxBlocks = 5;
                }
                else if (i == 30)
                {
                    blockSpec[i].startAddr = 1064;
                    blockSpec[i].endAddr = 1066;
                    blockSpec[i].partitionName = "mtd30";
                    blockSpec[i].maxBlocks = 3;
                }
                else if (i == 31)
                {
                    blockSpec[i].startAddr = 1067;
                    blockSpec[i].endAddr = 1359;
                    blockSpec[i].partitionName = "mtd31";
                    blockSpec[i].maxBlocks = 34;
                }
                else if (i == 32)
                {
                    blockSpec[i].startAddr = 1360;
                    blockSpec[i].endAddr = 1652;
                    blockSpec[i].partitionName = "mtd32";
                    blockSpec[i].maxBlocks = 34;
                }
                else if (i == 33)
                {
                    blockSpec[i].startAddr = 1653;
                    blockSpec[i].endAddr = 2047;
                    blockSpec[i].partitionName = "mtd33";
                    blockSpec[i].maxBlocks = 249;
                }
                else
                {
                    blockSpec[i].startAddr = 0;
                    blockSpec[i].endAddr = 0;
                    blockSpec[i].partitionName = "default";
                    blockSpec[i].maxBlocks = 0;
                }
            }
            #endregion Load Block
        }

        public void LoadBlockSpecArray()
        {
            int[][] blockSpecValues = new int[][]
            {
                new int[] { 0, 10, 20, 64, 70, 73, 78, 81, 84, 87, 90, 93, 96, 99, 102, 111, 116, 122, 193, 264, 267, 270, 643, 1016, 1022, 1027, 1030, 1033, 1038, 1059, 1064, 1067, 1360, 1653, 0},
                new int[] { 9, 19, 63, 69, 72, 77, 80, 83, 86, 89, 92, 95, 98, 101, 110, 115, 121, 192, 263, 266, 269, 642, 1015, 1021, 1026, 1029, 1032, 1037, 1058, 1063, 1066, 1359, 1652, 2047, 0},
                new int[] { 3, 7,  22, 0, 0, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 6, 6, 6, 3, 0, 48, 48, 6, 5, 0, 0, 5, 3, 5, 3, 34, 34, 249, 0 },
            };
            string[] partitionName = new string[35];
            partitionName[35] = "default";
            for (int i = 0; i <= 34; i++)
            {
                partitionName[i] = "mtd" + i;
            }

            for (int i = 0; i < blockSpec.Length; i++)
            {
                blockSpec[i] = new MyBlocks();
                blockSpec[i].startAddr = blockSpecValues[0][i];
                blockSpec[i].endAddr = blockSpecValues[1][i];
                blockSpec[i].partitionName = partitionName[2][i].ToString();
                blockSpec[i].maxBlocks = blockSpecValues[3][i];
                //blockSpec[i].BlockCount = blockSpecValues[4][i];
            }
        }


        private void FQC_Function_ShortingEthSpeed()
        {
            try
            {
                if (!CheckGoNoGo())
                    return;
                //CheckAndEnableTools(Func.ReadINI("Setting", "Tftp", "Path", "C://TFTP//tftpd64.exe"));
                if (!RamBOOT())
                    return;

                EthernetTest(PortType.TELNET);

                if (!CheckGoNoGo())
                    return;

            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Log, ex.ToString());
                warning = "Exception";
            }
            finally
            {
                UartDispose(atCmdUart);
            }
        }
    }
}

