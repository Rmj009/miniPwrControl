using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using WNC.API;
using System.IO;
using System.Threading;
using NationalInstruments.VisaNS;
using System.IO.Ports;
using System.Text.RegularExpressions;
using EasyLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Management;

namespace MiniPwrSupply.LMG1
{
    public partial class LMG1_OTA
    {
        private enum Antenna
        {
            Antenna_0 = 0x00,
            Antenna_1 = 0x01,
        }

        private void OTA()
        {
            try
            {
                if (!ChkStation(status_ATS.txtPSN.Text))
                    return;

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
                    frmOK.Label = "Vui lòng kiểm tra xem 'dây mạng' đã được kết nối,\nHãy bật nguồn cho DUT & Golden";
                    frmOK.ShowDialog();
                }
                DisplayMsg(LogType.Log, "Power on!!!");

                ChkBootUp(PortType.SSH);

                if (Func.ReadINI("Setting", "OTA", "SkipThread", "0") == "0")
                {
                    OTA_Thread();
                }

                if (Func.ReadINI("Setting", "OTA", "SkipWiFi", "0") == "0")
                {
                    OTA_WiFi();
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
        private void OTA_Thread()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            int retry = 1;
            string item = "Thread";
            string cmd = "";
            string res = "";
            string broadcast_pan_id = "";
            string broadcast_channel = "";

            try
            {
                DisplayMsg(LogType.Log, "=============== Thread ===============");
                if (this.SwitchDmpMode("0.1.2.7")) // judge FW greater or smaller
                {
                    if (this.SwitchDmpMode("0.2.0.4"))
                    {
                        cmd = "bt_upgrade_utility -f /lib/firmware/efr32/rcp4.3.0_no_encrypt_afh_coex_noTxLimit.gbl -p /dev/ttyMSM1";
                    }
                    cmd = "bt_upgrade_utility -f /lib/firmware/efr32/rcp4.3.0_no_encrypt_afh.gbl -p /dev/ttyMSM1";
                }
                else
                {
                    cmd = "bt_upgrade_utility -f /lib/firmware/efr32/rcp_no_encrypt_afh.gbl -p /dev/ttyMSM1";
                }
            //這裡只有DUT要切換成DMP FW,陪測Golden需要手動切換一次就好
            //Switch to DMP(dynamic multi-protocol) FW
            //9/21/23 Frank update test plan change command to bt_upgrade_utility -f /lib/firmware/efr32/rcp_no_encrypt_afh.gbl -p /dev/ttyMSM1
            Switch_DMP_FW:
                //if (!SendAndChk(PortType.SSH, "bt_upgrade_utility -f /lib/firmware/efr32/rcp_no_encrypt_afh.gbl -p /dev/ttyMSM1", "Transfer completed successfully", out res, 0, 60 * 1000))
                if (!SendAndChk(PortType.SSH, cmd, "Transfer completed successfully", out res, 0, 60 * 1000))
                {
                    DisplayMsg(LogType.Log, "Switch DMP FW fail");
                    if (retry < 5)
                    {
                        DisplayMsg(LogType.Log, $"Switch DMP FW retry...#{retry}");
                        retry++;
                        DisplayMsg(LogType.Log, "Delay 1s");
                        Thread.Sleep(1000);
                        goto Switch_DMP_FW;
                    }
                    AddData("SwitchDMPFW", 1);
                    return;
                }
                else
                {
                    DisplayMsg(LogType.Log, "Switch DMP FW pass");
                    AddData("SwitchDMPFW", 0);
                }

                //DUT: broadcast
                Start_Thread_Broadcast(out broadcast_channel, out broadcast_pan_id);

                if (!CheckGoNoGo())
                {
                    return;
                }

                //陪測Golden: Scan and check RSSI
                Start_Thread_Scan(broadcast_channel, broadcast_pan_id);

                //Stop beacon from DUT
                SendAndChk(item, PortType.SSH, "ot-ctl thread stop", "Done", 0, 5000);

                if (CheckGoNoGo())
                    AddData(item, 0);
                else
                    AddData(item, 1);
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                AddData(item, 1);
            }

        }
        private void Start_Thread_Broadcast(out string channel, out string pan_id)
        {
            channel = "";
            pan_id = "";

            if (!CheckGoNoGo())
            {
                return;
            }

            string item = "ThreadBroadcast";
            string res = "";
            string keyword = "root@OpenWrt:~# \r\n";

            try
            {
                DisplayMsg(LogType.Log, "=============== Start Thread Broadcast ===============");

                //Enable CPCD
                SendAndChk(PortType.SSH, "service cpcd start", keyword, out res, 0, 5000);
                DisplayMsg(LogType.Log, "Delay 5s...");
                Thread.Sleep(5000); //一定要delay,不然下一步會error
                SendAndChk(PortType.SSH, "logread -e cpcd", keyword, out res, 0, 5000);
                if (!res.Contains("Daemon startup was successful"))
                {
                    DisplayMsg(LogType.Log, "Enable CPCD fail");
                    AddData(item, 1);
                    return;
                }

                //Enable OTBR agent
                SendAndChk(PortType.SSH, "service wnc_otbr-agent start", keyword, out res, 0, 5000);
                DisplayMsg(LogType.Log, "Delay 5s...");
                Thread.Sleep(5000); //一定要delay,不然下一步會error
                SendAndChk(PortType.SSH, "logread -e otbr", keyword, out res, 0, 5000);
                if (!res.Contains("Start Thread Border Agent: OK"))
                {
                    DisplayMsg(LogType.Log, "Enable OTBR agent fail");
                    AddData(item, 1);
                    return;
                }

                //start thread broadcast
                SendAndChk(item, PortType.SSH, "ot-ctl dataset init new", "Done", 0, 5000);
                SendAndChk(item, PortType.SSH, "ot-ctl dataset commit active", "Done", 0, 5000);
                SendAndChk(item, PortType.SSH, "ot-ctl ifconfig up", "Done", 0, 5000);
                SendAndChk(item, PortType.SSH, "ot-ctl thread start", "Done", 0, 5000);
                SendAndChk(item, PortType.SSH, "ot-ctl state", "Done", 0, 5000);
                Thread.Sleep(5000);
                SendAndChk(PortType.SSH, "ot-ctl dataset", "Done", out res, 0, 10 * 1000);
                if (!res.Contains("Done"))
                {
                    DisplayMsg(LogType.Log, "Start thread broadcast fail");
                    AddData(item, 1);
                    return;
                }
                //Channel: 14
                //PAN ID: 0x6686
                Match m = Regex.Match(res, "Channel: (?<channel>.+)");
                if (m.Success)
                {
                    channel = m.Groups["channel"].Value.Trim();
                }
                m = Regex.Match(res, "^PAN ID: (?<pan_id>.+)", RegexOptions.Multiline);
                if (m.Success)
                {
                    pan_id = m.Groups["pan_id"].Value.Trim().Replace("0x", "");
                }
                DisplayMsg(LogType.Log, $"Broadcast channel: {channel}");
                DisplayMsg(LogType.Log, $"Broadcast pan_id: {pan_id}");

                //Rena_20230711, add txpower
                string TXPower = Func.ReadINI("Setting", "OTA", "Thread_TXPower", "0").Trim();
                SendAndChk(item, PortType.SSH, $"ot-ctl txpower {TXPower}", "Done", 0, 5000);

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
        private void Start_Thread_Scan(string broadcast_channel, string broadcast_pan_id)
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            string item = "ThreadScan";
            string res = "";
            string keyword = "root@Bridge_golden:~# \r\n";
            string pan_id = "";
            string mac_addr = "";
            string channel = "";
            double rssi = 0;
            double rssi_sum = 0;
            int rssiCount = 3; //要抓幾次rssi
            List<double> rssi_val = new List<double>();

            try
            {
                DisplayMsg(LogType.Log, "=============== Start Thread Scan ===============");

                if (!ChkInitial(PortType.GOLDEN_SSH, keyword, 120 * 1000))
                {
                    DisplayMsg(LogType.Log, "Golden SSH fail");
                    AddData(item, 1);
                    return;
                }
                int Rssi_retrycount = 0;
            Rssi_retry:
                //start Thread scan
                for (int i = 1; i <= rssiCount; i++)
                {
                    pan_id = "";
                    mac_addr = "";
                    channel = "";
                    rssi = 0;

                    SendAndChk(PortType.GOLDEN_SSH, $"ot-ctl scan {broadcast_channel}", keyword, out res, 0, 10 * 1000);
                    //SendAndChk(PortType.GOLDEN_SSH, $"ot-ctl scan", keyword, out res, 0, 10 * 1000);
                    if (res.Contains("connect session failed: No such file or directory"))
                    {
                        //Enable CPCD
                        SendAndChk(PortType.GOLDEN_SSH, "service cpcd start", keyword, out res, 0, 5000);
                        DisplayMsg(LogType.Log, "[Golden] Delay 5s...");
                        Thread.Sleep(5000); //一定要delay,不然下一步會error
                        SendAndChk(PortType.GOLDEN_SSH, "logread -e cpcd", keyword, out res, 0, 5000);

                        string CPCD = Func.ReadINI("Setting", "Setting", "CPCD", "");

                        if (!res.Contains(CPCD))
                        {
                            DisplayMsg(LogType.Log, "Check CPCD version fail fail");
                            AddData(item, 1);
                            return;
                        }


                        if (!res.Contains("Daemon startup was successful"))
                        {
                            DisplayMsg(LogType.Log, "[Golden] Enable CPCD fail");
                            AddData(item, 1);
                            return;
                        }

                        //Enable OTBR agent
                        SendAndChk(PortType.GOLDEN_SSH, "service wnc_otbr-agent start", keyword, out res, 0, 5000);
                        DisplayMsg(LogType.Log, "[Golden] Delay 5s...");
                        Thread.Sleep(5000); //一定要delay,不然下一步會error
                        SendAndChk(PortType.GOLDEN_SSH, "logread -e otbr", keyword, out res, 0, 5000);
                        if (!res.Contains("Start Thread Border Agent: OK"))
                        {
                            DisplayMsg(LogType.Log, "[Golden] Enable OTBR agent fail");
                            AddData(item, 1);
                            return;
                        }

                        SendAndChk(PortType.GOLDEN_SSH, $"ot-ctl scan {broadcast_channel}", "Done", out res, 0, 10 * 1000);
                        //SendAndChk(PortType.GOLDEN_SSH, $"ot-ctl scan", keyword, out res, 0, 10 * 1000);
                    }

                    /*
                    | PAN  | MAC Address      | Ch | dBm | LQI |
                    +------+------------------+----+-----+-----+
                    | c544 | 6eea7a5c8520e331 | 17 | -48 | 255 |
                    */
                    string[] lines = res.Split('\n');
                    foreach (string line in lines)
                    {
                        if (line.StartsWith("|") && !line.Contains("MAC Address"))
                        {
                            string[] data = line.Split('|');
                            if (data.Length >= 5)
                            {
                                pan_id = data[1].Trim();
                                mac_addr = data[2].Trim();
                                channel = data[3].Trim();
                                rssi = Convert.ToDouble(data[4].Trim());
                                DisplayMsg(LogType.Log, $"pan_id: {pan_id}");
                                DisplayMsg(LogType.Log, $"mac_addr: {mac_addr}");
                                DisplayMsg(LogType.Log, $"channel: {channel}");
                                DisplayMsg(LogType.Log, $"rssi: {rssi}");

                                if (channel == broadcast_channel && pan_id == broadcast_pan_id)
                                {
                                    rssi_val.Add(rssi);
                                }
                                else
                                {
                                    DisplayMsg(LogType.Log, $"Can't find 'channel: {broadcast_channel}' and 'pan_id: {broadcast_pan_id}'");
                                }
                            }
                        }
                    }
                }

                if (rssi_val.Count != rssiCount)
                {
                    if (Rssi_retrycount < 3)
                    {
                        DisplayMsg(LogType.Log, $"Retry RSSI time: {Rssi_retrycount}");
                        Rssi_retrycount++;
                        rssi_val.Clear();
                        goto Rssi_retry;
                    }
                    DisplayMsg(LogType.Log, $"Rssi count {rssi_val.Count} fail");
                    status_ATS.AddData("Thread_RSSI", "dBm", "-9999");
                    return;
                }

                for (int i = 0; i < rssi_val.Count; i++)
                {
                    rssi_sum += rssi_val[i];
                }
                status_ATS.AddData("Thread_RSSI", "dBm", rssi_sum / rssi_val.Count);
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                AddData(item, 1);
            }
        }
        private bool SwitchDmpMode(string _version)
        {
            if (!CheckGoNoGo())
            {
                return false;
            }
            bool IsFwGreater = false;
            string res = string.Empty;
            string FWversion = string.Empty;
            string item = $"SwitchDmpMode";
            string keyword = "root@OpenWrt:~# \r\n";
            DisplayMsg(LogType.Log, $"=============== {item} ===============");
            Version targetVerison = new Version(_version);
            //Version targetVerison = Version.Parse("1.0.0.0");
            try
            {
                SendAndChk(PortType.SSH, "mt info", keyword, out res, 0, 3000);
                Match m = Regex.Match(res, @"FW Version: (?<FWver>.+)");
                if (m.Success)
                {
                    FWversion = m.Groups["FWver"].Value.Trim().Split('v')[1];
                    DisplayMsg(LogType.Log, "DUT FWversion: " + FWversion);
                    //string a = FWversion.Split('v')[1];
                }
                Version FwVer = Version.Parse(FWversion);
                if (FwVer.CompareTo(targetVerison) > 0)
                {
                    IsFwGreater = true;
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, $"{item}" + ex.Message);
                AddData(item, 1);
            }
            return IsFwGreater;
        }

    }
}