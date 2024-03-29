﻿using System.Collections.Generic;
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
using WNC.API;
using Renci.SshNet;
using MiniPwrSupply.Instrument;

namespace MiniPwrSupply.LRG1
{
    public partial class LRG1_RF
    {
        private string _RFTool = "";
        private string _RFLog = "";
        private string _RFTestPlan = "";
        private int _RFTimeOutSec = 0;
        private string _LitePointFolder = string.Empty;
        private string _LitePointTool = string.Empty;
        private string _LitePointSummary = string.Empty;
        private string _LitePointLog_all = string.Empty;
        private string _LitePointLog_dutSetup_6G = string.Empty;

        public enum RFTestItem
        {
            WiFi,
            DECT,
            BLE,
            Thread
        }

        private void RF_Test()
        {
            try
            {
                infor.ResetParam();
                isGolden = false;
                OpenTftpd32(Application.StartupPath);
                //SE_TODO: get SN and BaseMAC from SFCS
                if (forHQtest)
                {
                    //Rena_20230627, for HQ RF stress test
                    //get SN and BaseMAC
                    GetBoardDataFromExcel(status_ATS.txtPSN.Text, true);

                    if (isLoop == 0)
                        infor.BaseMAC = MACConvert(infor.BaseMAC);
                    else
                        infor.BaseMAC = MACConvert("E8:C7:CF:AF:4D:D0", Loopcnt * 8); //for HQ stress test only

                    infor.WanMAC = MACConvert(infor.BaseMAC, 1);

                    //WiFi 2.4G MAC = BaseMAC+4
                    infor.WiFiMAC_2G = MACConvert(infor.BaseMAC, 4);
                    //WiFi 5G MAC = BaseMAC+3
                    infor.WiFiMAC_5G = MACConvert(infor.BaseMAC, 3);
                    //WiFi 6G MAC = BaseMAC+2
                    infor.WiFiMAC_6G = MACConvert(infor.BaseMAC, 2);

                    DisplayMsg(LogType.Log, $"BaseMAC: {infor.BaseMAC}");
                    DisplayMsg(LogType.Log, $"WanMAC: {infor.WanMAC}");
                    DisplayMsg(LogType.Log, $"WiFiMAC_2G: {infor.WiFiMAC_2G}");
                    DisplayMsg(LogType.Log, $"WiFiMAC_5G: {infor.WiFiMAC_5G}");
                    DisplayMsg(LogType.Log, $"WiFiMAC_6G: {infor.WiFiMAC_6G}");
                }

                if (status_ATS._testMode == StatusUI2.StatusUI.TestMode.EngMode)
                {
                    string BaseMACEngmode = Func.ReadINI("Setting", "MAC_Engmode", "Golden_MAC", "E8:C7:CF:AF:50:E8");
                    infor.BaseMAC = MACConvert(BaseMACEngmode);
                    //WiFi 2.4G MAC = BaseMAC+4
                    infor.WiFiMAC_2G = MACConvert(infor.BaseMAC, 4);
                    //WiFi 5G MAC = BaseMAC+3
                    infor.WiFiMAC_5G = MACConvert(infor.BaseMAC, 3);
                    //WiFi 6G MAC = BaseMAC+2
                    infor.WiFiMAC_6G = MACConvert(infor.BaseMAC, 2);

                    DisplayMsg(LogType.Log, $"BaseMAC: {infor.BaseMAC}");
                    DisplayMsg(LogType.Log, $"WanMAC: {infor.WanMAC}");
                    DisplayMsg(LogType.Log, $"WiFiMAC_2G: {infor.WiFiMAC_2G}");
                    DisplayMsg(LogType.Log, $"WiFiMAC_5G: {infor.WiFiMAC_5G}");
                    DisplayMsg(LogType.Log, $"WiFiMAC_6G: {infor.WiFiMAC_6G}");
                }
                else
                {
                    DisplayMsg(LogType.Log, $"SMT SN Input: {status_ATS.txtPSN.Text}");
                    infor.BaseMAC = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@MAC");
                    infor.SerialNumber = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@LRG1_SN");
                    if (string.IsNullOrEmpty(infor.SerialNumber) || string.IsNullOrEmpty(infor.BaseMAC)
                       || infor.SerialNumber.Contains("Dut not have") || infor.BaseMAC.Contains("Dut not have"))
                    {
                        warning = "Get from SFCS FAIL";
                        return;
                    }
                    infor.WanMAC = MACConvert(infor.BaseMAC, 1);
                    //WiFi 2.4G MAC = BaseMAC+4
                    infor.WiFiMAC_2G = MACConvert(infor.BaseMAC, 4);
                    //WiFi 5G MAC = BaseMAC+3
                    infor.WiFiMAC_5G = MACConvert(infor.BaseMAC, 3);
                    //WiFi 6G MAC = BaseMAC+2
                    infor.WiFiMAC_6G = MACConvert(infor.BaseMAC, 2);
                    DisplayMsg(LogType.Log, $"LRG1_SN: {infor.SerialNumber}");
                    DisplayMsg(LogType.Log, $"BaseMAC: {infor.BaseMAC}");
                    DisplayMsg(LogType.Log, $"WanMAC: {infor.WanMAC}");
                    DisplayMsg(LogType.Log, $"WiFiMAC_2G: {infor.WiFiMAC_2G}");
                    DisplayMsg(LogType.Log, $"WiFiMAC_5G: {infor.WiFiMAC_5G}");
                    DisplayMsg(LogType.Log, $"WiFiMAC_6G: {infor.WiFiMAC_6G}");

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
                }

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
                    //SwitchRelay(CTRL.ON);
                    //Thread.Sleep(loopDelay);
                    SwitchRelay(CTRL.OFF);
                }
                else
                {
                    frmOK.Label = "Vui lòng bật nguồn và nhấn nút nguồn để khởi động";
                    frmOK.ShowDialog();
                }
                DisplayMsg(LogType.Log, "Power on!!!");

                if (Func.ReadINI("Setting", "Golden", "GoldenSN", "(*&^%$").Contains(status_ATS.txtPSN.Text))
                {
                    isGolden = true;
                    DisplayMsg(LogType.Log, "Golden testing..." + status_ATS.txtPSN.Text);
                }
                else
                    isGolden = false;


                if (status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode)
                {
                    if (!ChkStation(status_ATS.txtPSN.Text)) { return; }
                }

                ChkBootUp(PortType.SSH);
                #region Ethenet Speed check
                if (!CheckGoNoGo()) { return; }
                if (Func.ReadINI("Setting", "RF", "SkipLANSPEED1", "0") == "0") { this.EthernetTest(1); }
                if (Func.ReadINI("Setting", "RF", "SkipLANSPEED2", "0") == "0") { this.EthernetTest(2); }
                if (Func.ReadINI("Setting", "RF", "SkipLANSPEED3", "0") == "0") { this.EthernetTest(3); }
                if (Func.ReadINI("Setting", "RF", "SkipLANSPEED4", "0") == "0") { this.EthernetTest(4); }
                if (Func.ReadINI("Setting", "RF", "SkipLANSPEED5", "0") == "0") { this.EthernetTest(5); }
                #endregion Ethenet Speed check
                if (!CheckGoNoGo()) { return; }
                this.BatteryDetection("AC_ALARM: low");
                if (isLoop == 0)
                {
                    if (status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode)
                    {
                        CheckEthernetMAC();
                    }
                }
                // ---------------------------------------
                //this.ChkD2License();//Jason add more follow NPI Thiem Liu 20231125
                if (!CheckGoNoGo()) { return; }
                CheckEthernetMAC();
                // test plan extra added
                // ----------------------------------------
                if (Func.ReadINI("Setting", "RF", "SkipDECT", "0") == "0")
                {
                    if (!CheckGoNoGo()) { return; }
                    RF_DECT();
                }
                //BLE & Thread via uart
                //   ============================= removal Not in test plan  ============================= 
                //if (Func.ReadINI("Setting", "RF", "SkipBLE", "0") == "0" || Func.ReadINI("Setting", "RF", "SkipThread", "0") == "0")
                //{
                //    //UartDispose(uart);
                //    ChkBootUp(PortType.SSH);
                //}
                //   ============================= removal Not in test plan  ============================= 
                if (Func.ReadINI("Setting", "RF", "SkipBLE", "0") == "0")
                {
                    if (!CheckGoNoGo()) { return; }
                    RF_BLE();
                }

                if (Func.ReadINI("Setting", "RF", "SkipThread", "0") == "0")
                {
                    if (!CheckGoNoGo()) { return; }
                    RF_Thread();
                }

                if (Func.ReadINI("Setting", "RF", "SkipWiFi", "0") == "0")
                {
                    if (!CheckGoNoGo()) { return; }
                    RF_WiFi();
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
        private void CheckLEDBootup(string item, COLOR color, STAGE stage, string cameraItem) // Jason Add 2023/10/15
        {
            if (!CheckGoNoGo()) { return; }

            if (useCamera)
            {
                string cameraResult = "";
                if (CameraLEDCheck())
                {
                    /*

                    List<int> Colorlist = ColorlistString.Split(',').Select(int.Parse).ToList();
                    for (int i = 0; i < Colorlist.Count; i++)
                    {
                        DisplayMsg(LogType.Log, $" Check LED Item {item} /Color {Colorlist[i]} /Stage {stage} /In camera Item {cameraItem}");
                        if (CheckCameraResult($"{cameraItem}", $"{Colorlist[i].ToString().ToLower()}", out cameraResult)) // Check If have color will return because DUT already have power
                        {
                            DisplayMsg(LogType.Log, $"Check LED bootup is ok But DUT Cannot bootup");
                            AddData($"CkLedBootup", 1);
                            Colorlist.Clear();
                            return;
                        }
                    }
                    */

                    DisplayMsg(LogType.Log, $" Check LED Item {item} /Color {color} /Stage {stage} /In camera Item {cameraItem}");
                    if (CheckCameraResult($"{cameraItem}", $"{color.ToString().ToLower()}", out cameraResult)) // Check If have color will return because DUT already have power
                    {
                        DisplayMsg(LogType.Log, $"Check LED bootup is ok But DUT Cannot bootup");
                        AddData("CkLedBootup", 1);
                        return;
                    }
                    else
                    {
                        DisplayMsg(LogType.Log, $"Check LED Fail OP forgot to press the Power button");
                        //Colorlist.Clear();
                        if (!fixture.ChkState(true))
                        {
                            DisplayMsg(LogType.Log, $"Fixture Auto Press Power button....");
                            fixture.ControlIO(Fixture.FixtureIO.IO_5, CTRL.ON); // Press button power then re-try again
                            Thread.Sleep(1000);
                            fixture.ControlIO(Fixture.FixtureIO.IO_5, CTRL.OFF);
                            Thread.Sleep(200);
                        }
                        else { MessageBox.Show("Open Fixture then Press power button"); }
                    }
                }
                else
                {
                    if (Process.GetProcessesByName("camera").Length > 0)
                    {
                        warning = "Camera is running but use camera fail!!";
                        return;
                    }
                    else
                    {
                        warning = "Using camera fail because Camera is not running!!";
                        return;
                    }
                }
            }
            else
            {
                if (DialogResult.No == MessageBox.Show($"Open Fixture then Press power button", "Led Test", MessageBoxButtons.YesNo))
                {
                    DisplayMsg(LogType.Log, $"OP choose No");
                    AddData($"CkLedBootup", 1);
                    return;
                }
                DisplayMsg(LogType.Log, $"OP choose Yes");
            }
        }
        private void TestBridge(PortType portType)
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            DisplayMsg(LogType.Log, "=============== Check TestBridge ===============");
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
        private void ReadSN(PortType portType)
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            DisplayMsg(LogType.Log, "=============== Read Serial Number ===============");

            int retry = 0;
            string keyword = @"root@OpenWrt";
            string item = "ReadSN";
            string res = "";
            string DUT_SN = "";

            try
            {
            Read_SN:
                SendAndChk(portType, "mt boarddata", keyword, out res, 0, 5000);
                //serial_number=+119746+2333000129
                Match m = Regex.Match(res, "serial_number=(?<sn>.+)");
                if (m.Success)
                {
                    DUT_SN = m.Groups["sn"].Value.Trim();
                }

                DisplayMsg(LogType.Log, $"DUT_SN: {DUT_SN}");
                if (DUT_SN == "")
                {
                    if (retry++ < 3)
                    {
                        DisplayMsg(LogType.Log, "Read serial_number fail, retry...");
                        Thread.Sleep(1000);
                        goto Read_SN;
                    }

                    DisplayMsg(LogType.Log, "Read serial_number fail");
                    AddData(item, 1);
                }
                else
                {
                    AddData(item, 0);
                    SetTextBox(status_ATS.txtPSN, DUT_SN);
                    status_ATS.SFCS_Data.First_Line = DUT_SN;
                    status_ATS.SFCS_Data.PSN = DUT_SN;
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                AddData(item, 1);
            }
        }
        private void RF_WiFi()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            DisplayMsg(LogType.Log, "=============== WiFi Test ===============");

            string item = $"RF_{RFTestItem.WiFi.ToString()}";

            try
            {
                //TODO: check QUTS status?
                // =================================
                // LRG1 DONT NEED removeCalData by SamSung
                // =================================
                if (!isGolden)
                {
                    this.RemoveCalData(PortType.SSH); //THIEM in accordance with breakdown
                }
                EnterWiFiTestMode(PortType.SSH);
                //開始test前要關閉uart
                //UartDispose(uart); //shall be removed , no Uart now

                ModifySerialMAC();

                KillTaskProcess("tftpd32");
                Thread.Sleep(1000);

                RunIQFact(RFTestItem.WiFi);

                this.CheckSecureBootEnable();

                if (!isGolden)
                {
                    //if (Func.ReadINI("Setting", "RF", "CheckCalData", "0") == "1")
                    // {
                    CheckWiFiCalDataRFStation(PortType.SSH);
                    // }
                }


                if (CheckGoNoGo())
                {
                    AddData(item, 0);
                }
                else
                {
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
                //this.UartDispose(uart);
            }
        }
        private void BLE_FTM_via_Bridge(RFTestItem testItem)
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            string item = $"Enable_{testItem.ToString()}_FTM_via_bridge";
            //string keyword = "root@OpenWrt:~# \r\n";
            //string keyword_bridge = "root@Bridge_golden:/# \r\n";
            string keyword = "root@OpenWrt";
            string keyword_bridge = "root@Bridge_golden";
            string res = "";
            string cmd = "";
            int retrytime = 0;
            try
            {
                DisplayMsg(LogType.Log, $"=============== {item} ===============");
            //In bridge debug console, SSH into DUT
            pingRetry:
                SendAndChk(PortType.UART, "ping 192.168.1.1", keyword_bridge, out res, 0, 15 * 1000);
                SendCommand(PortType.UART, sCtrlC, 500);
                if (res.Contains("ms"))
                {
                    AddData("PING_DUT_PASS", 0);
                    DisplayMsg(LogType.Log, "PING_DUT_PASS");
                }
                else
                {
                    retrytime++;
                    if (retrytime < 3)
                    {
                        goto pingRetry;
                    }
                    else
                    {
                        AddData("PING_DUT_PASS", 1);
                        if (!CheckGoNoGo())
                        {
                            DisplayMsg(LogType.Error, @"PING_DUT_NG");
                            return;
                        }
                    }
                }
                SendAndChk(PortType.UART, "ssh 192.168.1.1 -y -y", keyword_bridge, "", out res, 0, 20 * 1000);
                for (int i = 0; i < 3; i++)
                {
                    SendAndChk(PortType.SSH, "\n", "", out res, 0, 1000);
                    //DisplayMsg(LogType.Log, res);
                    if (res.Contains("root@OpenWrt:~#"))
                    {
                        break;
                    }
                }
                //if (res.Contains("Do you want to continue connecting?"))
                //{
                //    SendAndChk(PortType.UART, "y", keyword, keyword_bridge, out res, 0, 5000);
                //}
                //Thread.Sleep(10 * 1000);
                //MessageBox.Show("wait for busy box");
                //if (!res.Contains("BusyBox") || !res.Contains(keyword_bridge))
                //{
                //    DisplayMsg(LogType.Log, "SSH into DUT fail");
                //    AddData(item, 1);
                //    return;
                //}

                if (testItem == RFTestItem.BLE)
                {
                    // ====================================================================
                    if (this.SwitchDmpMode("1.0.0.0")) // judge FW greater or smaller
                    {
                        cmd = "bt_upgrade_utility -f /lib/firmware/efr32/bt_ncp_afh_se_coex_4_3_0_noTxlimit.gbl -p /dev/ttyMSM1";
                    }
                    else
                    {
                        cmd = "bt_upgrade_utility -p /dev/ttyMSM1 -f /lib/firmware/efr32/bt_ncp_afh_se.gbl";
                    }
                    // ====================================================================
                    if (!SendAndChk(PortType.SSH, cmd, "Transfer completed successfully", out res, 0, 60 * 1000))
                    {
                        DisplayMsg(LogType.Log, "Switch BLE FW fail");
                        AddData("SwitchBLEFW", 1);
                        return;
                    }
                    else
                    {
                        DisplayMsg(LogType.Log, "Switch BLE FW pass");
                        AddData("SwitchBLEFW", 0);
                    }
                }

                if (testItem == RFTestItem.Thread)
                {
                    //SendAndChk(PortType.UART, "bt_host_empty -u /dev/ttyMSM1", "Press Crtl+C to quit", out res, 0, 3000);
                    // secureboot testplan
                    SendAndChk(PortType.UART, "bt_upgrade_utility -p /dev/ttyMSM1 -f /lib/firmware/efr32/rail_test_gecko3.2.3_noFlowCtl.gbl", "Press Crtl+C to quit", out res, 0, 3000);
                    SendAndChk(PortType.UART, sCtrlC, keyword, out res, 500, 3000);
                }

                SendCommand(PortType.UART, "microcom /dev/ttyMSM1 -s 115200 -X", 5000);

                if (CheckGoNoGo())
                {
                    AddData(item, 0);
                }
                else
                {
                    AddData(item, 1);
                    return;
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                AddData(item, 1);
            }
        }
        private void RF_BLE()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            DisplayMsg(LogType.Log, "=============== BLE Test ===============");

            string keyword = @"root@";
            string item = $"RF_{RFTestItem.BLE.ToString()}";
            string res = "";

            try
            {
                //切換BLE FW
                /*if (!SendAndChk(PortType.SSH, "bt_upgrade_utility -p /dev/ttyMSM1 -f /lib/firmware/efr32/bt_ncp_afh_se.gbl", "Transfer completed successfully", out res, 0, 60 * 1000))
                {
                    DisplayMsg(LogType.Log, "Switch BLE FW fail");
                    AddData("SwitchBLEFW", 1);
                    return;
                }
                else
                {
                    DisplayMsg(LogType.Log, "Switch BLE FW pass");
                    AddData("SwitchBLEFW", 0);
                }*/

                //Bring up BLE
                //SendAndChk(PortType.UART, "echo 470 > /sys/class/gpio/export", keyword, out res, 0, 3000);
                //SendAndChk(PortType.UART, "echo out > /sys/class/gpio/gpio470/direction", keyword, out res, 0, 3000);
                //SendAndChk(PortType.UART, "echo 1 > /sys/class/gpio/gpio470/value", keyword, out res, 0, 3000);
                //SendAndChk(PortType.UART, "echo 466 > /sys/class/gpio/export", keyword, out res, 0, 3000);
                //SendAndChk(PortType.UART, "echo out > /sys/class/gpio/gpio466/direction", keyword, out res, 0, 3000);
                //SendAndChk(PortType.UART, "echo 1 > /sys/class/gpio/gpio466/value", keyword, out res, 0, 3000);

                BLE_FTM_via_Bridge(RFTestItem.BLE);//Jason add follow PE method 2023/09/29

                // SendCommand(PortType.UART, "microcom /dev/ttyMSM1 -s 115200 -X", 2000); //Jason remove follow PE method 2023/09/29

                //開始BLE test前要關閉uart
                UartDispose(uart);

                //start BLE test
                RunIQFact(RFTestItem.BLE);

                if (CheckGoNoGo())
                {
                    AddData(item, 0);
                }
                else
                {
                    AddData(item, 1);
                    return;
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                AddData(item, 1);
            }
            finally
            {
                Kill_microcom();
                // Close_SSH_in_Bridge();
            }
        }
        private void RF_Thread()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            DisplayMsg(LogType.Log, "=============== Thread Test ===============");

            string keyword = @"root@";
            string item = $"RF_{RFTestItem.Thread.ToString()}";
            string res = "";
            //string respones = "{{(getCtune)}{CTUNEXIANA:0x06a}{CTUNEXOANA:0x06a}}";   //will output 0x06a if PN = 53.LRG16.F0
            //string respes = "{{(getCtune)}{CTUNEXIANA:0x06a}{CTUNEXOANA:0x06a}}";  //will output 0x66 if PN = 53.LRG16.F01
            string respes = Func.ReadINI("Setting", "RFTest", "CTUNEThread", "{{(getCtune)}{CTUNEXIANA:0x06a}{CTUNEXOANA:0x06a}}");

            try
            {
                //切換Thread FW
                if (!SendAndChk(PortType.SSH, "bt_upgrade_utility -p /dev/ttyMSM1 -f /lib/firmware/efr32/rail_test_gecko3.2.3_noFlowCtl.gbl", "Transfer completed successfully", out res, 0, 60 * 1000))
                {
                    DisplayMsg(LogType.Log, "Switch Thread FW fail");
                    AddData("SwitchThreadFW", 1);
                    return;
                }
                else
                {
                    DisplayMsg(LogType.Log, "Switch Thread FW pass");
                    AddData("SwitchThreadFW", 0);
                }

                //Bring up Thread

                //BLE_FTM_via_Bridge(RFTestItem.Thread); //Jason Change follow PE & Ryan 2023/09/29
                SendAndChk(PortType.UART, "bt_host_empty -u /dev/ttyMSM1", "Press Crtl+C to quit", out res, 0, 3000);
                SendAndChk(PortType.UART, sCtrlC, keyword, out res, 500, 3000);
                SendCommand(PortType.UART, "microcom /dev/ttyMSM1 -s 115200 -X", 2000);
                Thread.Sleep(500);
                if (!SendAndChk(PortType.UART, "\r\n", ">", out res, 0, 5000))
                {
                    if (!SendAndChk(PortType.UART, "\r\n", ">", out res, 0, 5000)) //Jason Change follow PE & Ryan 2023/09/29
                    {
                        if (!SendAndChk(PortType.UART, "\r\n", ">", out res, 0, 5000)) { AddData("LoginMode", 1); return; }
                    }
                }

                if (SendAndChk(PortType.UART, "getCtune", respes, out res, 0, 5000))
                {
                    DisplayMsg(LogType.Log, @"get Ctune OK: " + res);
                }
                else
                {
                    if (SendAndChk(PortType.UART, "getCtune", respes, out res, 0, 5000))
                    {
                        DisplayMsg(LogType.Log, @"get Ctune OK: " + res);
                    }
                    else
                    {
                        DisplayMsg(LogType.Log, @"get Ctune NG: " + res);
                        AddData("GetCtune", 1);
                        return;
                    }
                }
                // Check CTune value based on different partNumber., 
                // For instances, 53.LRG16.F02 => 0x06a; 53.LRG16.F01 => 0x066
                //開始BLE test前要關閉uart
                UartDispose(uart);

                //start BLE test
                RunIQFact(RFTestItem.Thread);

                if (CheckGoNoGo())
                {
                    AddData(item, 0);
                }
                else
                {
                    AddData(item, 1);
                    return;
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                AddData(item, 1);
                return;
            }
            finally
            {
                Kill_microcom();
                Close_SSH_in_Bridge();
            }
        }

        private void Close_SSH_in_Bridge()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            string item = "CloseBridge";
            string keyword = @"root@Bridge_golden";
            int count = 0;
            bool result = false;

            try
            {
                DisplayMsg(LogType.Log, "Close SSH connection in Bridge");

                while (count++ < 3)
                {
                    result = SendAndChk(PortType.UART, "exit", keyword, 0, 3000);
                    if (result)
                    {
                        break;
                    }
                }

                if (result)
                {
                    AddData(item, 0);
                }
                else
                {
                    AddData(item, 1);

                    DisplayMsg(LogType.Log, "Please reboot Bridge golden!!");
                    frmOK.Label = "Please reboot Bridge golden!!";
                    frmOK.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                AddData(item, 1);
            }
        }
        private void RF_DECT()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            string item = $"RF_{RFTestItem.DECT.ToString()}";
            DisplayMsg(LogType.Log, $"=============== {item} ===============");
            try
            {
                int delay = Convert.ToInt32(Func.ReadINI("Setting", "RF", "DelaySec_DECT", "0"));
                DisplayMsg(LogType.Log, $"Sleep {delay}s");
                Thread.Sleep(delay * 1000);

                string IQXelMW_IP = Func.ReadINI("Setting", "IP", "IQxelMW", "192.168.100.253");
                string toolPath = Func.ReadINI("Setting", "RF", "ToolPath_DECT", @"D:\ATSuite_V11_0_2_DSPG_Hub_Pro_FP_V_1.2.0_WNC_LS04\Release");
                string logPath = Path.Combine(toolPath, @"Log\Log_Current.txt");
                int TimeOutSec = Convert.ToInt32(Func.ReadINI("Setting", "RF", "TimeOutSec_DECT", "240"));

                DisplayMsg(LogType.Log, "DECT tool Path:" + toolPath);
                DisplayMsg(LogType.Log, "DECT log Path:" + logPath);
                this.CheckDECTMode("None");
                LitePoint litepoint = new LitePoint(toolPath + "\\ATSuite.exe", logPath);
                if (litepoint.Start())
                {
                    litepoint.WaitResult(TimeOutSec * 1000);
                }
                else
                {
                    warning = "Cannot start litepoint";
                    return;
                }
                litepoint.CloseTool();
                if (File.Exists(logPath))
                {
                    DECTAnalysisLog(logPath);
                }
                else
                {
                    warning = "File " + logPath + " not exist";
                    return;
                }
                if (CheckGoNoGo())
                {
                    AddData(item, 0);
                }
                else
                {
                    AddData(item, 1);
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, @"RF_DECT___" + ex.Message);
                AddData(item, 1);
            }
            finally
            {
                if (SendAndChk(PortType.SSH, "\r\n", "#", 0, 2000))
                {
                    DisplayMsg(LogType.Log, "back to root@OpenWrt");
                }
                //Thread.Sleep(50);
            }
        }
        private void Kill_microcom()
        {
            string keyword = @"root@OpenWrt";
            string res = "";
            int count = 0;
            bool result = false;

            try
            {
                DisplayMsg(LogType.Log, "Kill microcom");

                while (count++ < 3)
                {
                    result = SendAndChk(PortType.SSH, "ps | grep microcom", keyword, out res, 0, 3000);
                    if (!result)
                    {
                        Thread.Sleep(500);
                    }
                    else if (res.Contains("microcom /dev/ttyMSM1"))
                    {
                        //console下完microcom後就會卡住,只能透過SSH kill microcom
                        SendAndChk(PortType.SSH, "killall microcom", keyword, out res, 0, 3000);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
            }
        }
        private bool RunIQFact(RFTestItem testItem)
        {
            if (!CheckGoNoGo())
            {
                return false;
            }
            //string DUT_IP = Func.ReadINI("Setting", "IP", "DUT", "192.168.1.1");
            //string IQXel_IP = Func.ReadINI("Setting", "IP", "IQXel", "192.168.100.254");
            // EPR-1-2 breakdown cancel the ping
            string item = $"RF_{testItem.ToString()}";

            DisplayMsg(LogType.Log, $"=============== Run IQfact ({testItem.ToString()}) ===============");

            try
            {
                _RFTool = Func.ReadINI("Setting", "RF", $"ToolPath_{testItem.ToString()}", string.Empty);
                _RFLog = Path.GetDirectoryName(_RFTool) + "\\Log\\logOutput.txt";

                if (isGolden) //Jason add 2023/09/27 if golden will use golden test plan
                {
                    _RFTestPlan = Func.ReadINI("Setting", "RF", $"Golden_TestPlan_{testItem.ToString()}", string.Empty);
                    DisplayMsg(LogType.Log, $"=============== Run IQfact In golden mode use golden test plan ===============");
                }
                else
                {
                    _RFTestPlan = Func.ReadINI("Setting", "RF", $"TestPlan_{testItem.ToString()}", string.Empty);
                    DisplayMsg(LogType.Log, $"=============== Run IQfact In Normal Mode ===============");
                }

                _RFTimeOutSec = Convert.ToInt32(Func.ReadINI("Setting", "RF", $"TimeOutSec_{testItem.ToString()}", "180"));
                DisplayMsg(LogType.Log, $"Tool: {_RFTool}");
                DisplayMsg(LogType.Log, $"Log: {_RFLog}");
                DisplayMsg(LogType.Log, $"TestPlan: {_RFTestPlan}");
                DisplayMsg(LogType.Log, $"TimeOutSec: {_RFTimeOutSec}");

                if (string.IsNullOrEmpty(_RFTool) || string.IsNullOrEmpty(_RFLog) || string.IsNullOrEmpty(_RFTestPlan))
                {
                    DisplayMsg(LogType.Log, "Tool/Log/TestPlan can't be null, please check the setting in Setting.ini");
                    AddData(item, 1);
                    return false;
                }

                if (File.Exists(_RFLog))
                {
                    DisplayMsg(LogType.Log, "Delete " + _RFLog);
                    File.Delete(_RFLog);
                }
                //===================================================
                // EPR-1-2 breakdown cancel the ping
                //===================================================
                // ping DUT
                //if (!telnet.Ping(DUT_IP, 10 * 1000))
                //{
                //    DisplayMsg(LogType.Error, $"Ping DUT({DUT_IP}) failed!!");
                //    AddData("PingDUT", 1);
                //    return false;
                //}
                // ping IQXel
                //if (!telnet.Ping(IQXel_IP, 10 * 1000))
                //{
                //    DisplayMsg(LogType.Error, $"Ping IQXel({IQXel_IP}) failed!!");
                //    AddData("PingIQXel", 1);
                //    return false;
                //}
                //===================================================
                CallIQFactProcess(testItem);

                return true;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                AddData(item, 1);
                return false;
            }
        }

        private void CallIQFactProcess(RFTestItem testItem)
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            string item = $"RF_{testItem.ToString()}";

            try
            {
                Process process;
                process = new Process();
                process.StartInfo.WorkingDirectory = Path.GetDirectoryName(_RFTool);
                process.StartInfo.FileName = _RFTool;
                process.StartInfo.Arguments = "-RUN \"" + _RFTestPlan + "\" -EXIT";
                process.StartInfo.UseShellExecute = false;
                DisplayMsg(LogType.Log, "Execute IQfactRun_Console tool");
                DisplayMsg(LogType.Log, $"Cmd: {_RFTool} -RUN \"{_RFTestPlan}\" -EXIT");
                process.Start();
                DisplayMsg(LogType.Log, "Wait for exit..");

                if (ChkResult(testItem))
                {
                    Analyze_RFLog(testItem);
                }

                //process.WaitForExit(timeOutMs);
                DisplayMsg(LogType.Log, "Complete");
                process.Dispose();
                process.Close();
                process = null;
                KillTaskProcess("IQfactRun_Console");
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, "CallIQFactProcess error, ret: " + ex.Message);
                AddData(item, 1);
            }
        }
        private bool ChkResult(RFTestItem testItem)
        {
            if (!CheckGoNoGo())
            {
                return false;
            }

            string item = $"Check{testItem.ToString()}Log";

            try
            {
                DisplayMsg(LogType.Log, $"Check {testItem.ToString()} log...");
                DisplayMsg(LogType.Log, $"Log Path: {_RFLog}");

                //int timeOutMs = 150 * 1000;
                int timeOutMs = _RFTimeOutSec * 1000;
                DateTime dt;
                TimeSpan ts;
                dt = DateTime.Now;

                while (true)
                {
                    ts = new TimeSpan(DateTime.Now.Ticks - dt.Ticks);
                    if (ts.TotalMilliseconds > timeOutMs)
                    {
                        DisplayMsg(LogType.Error, "Check log timeout");
                        AddData(item, 1);
                        return false;
                    }

                    if (File.Exists(_RFLog))
                    {
                        FileInfo fileInfo = new FileInfo(_RFLog);
                        DisplayMsg(LogType.Log, "File size: " + fileInfo.Length);
                        if (fileInfo.Length > 0)
                        {
                            DisplayMsg(LogType.Log, "Check log success.");
                            Thread.Sleep(2000);
                            return true;
                        }
                    }
                    Thread.Sleep(200);
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, "ChkResult error, ret: " + ex.Message);
                AddData(item, 1);
                return false;
            }
        }
        private string GetMiddleString(string source, string key1, string key2)
        {
            //從source字串中擷取key1--key2之間的字串
            int start_index = source.IndexOf(key1);
            int end_index = source.IndexOf(key2);

            if (start_index != -1 && end_index != -1)
                return source.Substring(start_index + 1, end_index - start_index - 1);
            else
                return "";
        }
        private string GetLSL(string source)
        {
            string res = GetMiddleString(source, "(", ",").Trim();
            if (string.IsNullOrEmpty(res))
                return "-999";
            else
                return res;
        }
        private string GetUSL(string source)
        {
            string res = GetMiddleString(source, ",", ")").Trim();
            if (string.IsNullOrEmpty(res))
                return "999";
            else
                return res;
        }
        private string GetErrorCode(string sTestItem)
        {
            string sErrorCode = Func.ReadINI("SPEC", "Error_Code", sTestItem, "000000");
            return sErrorCode;
        }
        private void Analyze_RFLog(RFTestItem testItem)
        {
            string test_item = $"RF_{testItem.ToString()}";

            try
            {
                string path = Path.GetDirectoryName(_RFLog);
                string backup_fd = path + "\\BackupLogs";
                string temp_log = path + "\\temp.txt";
                string _SN = status_ATS.txtPSN.Text;
                //string _MAC = status_ATS.txtSP.Text;

                DisplayMsg(LogType.Log, $"Analyze {testItem.ToString()} Log");

                if (File.Exists(temp_log))
                {
                    DisplayMsg(LogType.Log, "Delete " + temp_log);
                    File.Delete(temp_log);
                }
                Thread.Sleep(1000);

                if (File.Exists(_RFLog))
                {
                    DisplayMsg(LogType.Log, "Copy " + _RFLog + " to " + temp_log);
                    File.Copy(_RFLog, temp_log);
                    Thread.Sleep(1000);

                    string sTime = DateTime.Now.ToString("yyyyMMdd-HHmmss", System.Globalization.CultureInfo.InvariantCulture);
                    string backup_file = backup_fd + "\\logOutput_" + _SN + "_" + sTime + ".txt";
                    DisplayMsg(LogType.Log, "Backup log in " + backup_file);
                    Directory.CreateDirectory(backup_fd);
                    File.Copy(_RFLog, backup_file, true);
                }

                DisplayMsg(LogType.Log, "Read data from " + temp_log);
                DisplayMsg(LogType.Log, File.ReadAllText(temp_log));

                FileStream cser = new FileStream(temp_log, FileMode.Open, FileAccess.Read);
                StreamReader csrall = new StreamReader(cser, Encoding.Default);

                string line = "";
                while (!csrall.EndOfStream)
                {
                    line = csrall.ReadLine();

                    #region 1.GLOBAL_SETTINGS
                    if (line.Contains(".GLOBAL_SETTINGS"))
                    {
                        int result = 0;
                        do
                        {
                            line = csrall.ReadLine().Trim().Replace("\t", "");
                            if (line.Contains("ERROR_MESSAGE") && !line.Contains("Function completed."))
                            {
                                result = 1;
                                break;
                            }
                        }
                        while (!line.Contains("ERROR_MESSAGE"));
                        status_ATS.AddData("GLOBAL_SETTINGS", "", 0, 0, result, GetErrorCode("GLOBAL_SETTINGS"));
                    }
                    #endregion

                    #region 2.INSERT_DUT
                    if (line.Contains(".INSERT_DUT"))
                    {
                        int result = 0;
                        do
                        {
                            line = csrall.ReadLine().Trim().Replace("\t", "");
                            if (line.Contains("ERROR_MESSAGE") && !line.Contains("Function completed."))
                            {
                                result = 1;
                                break;
                            }
                        }
                        while (!line.Contains("ERROR_MESSAGE"));
                        status_ATS.AddData("INSERT_DUT", "", 0, 0, result, GetErrorCode("INSERT_DUT"));
                    }
                    #endregion

                    #region 3.INITIALIZE_DUT
                    if (line.Contains(".INITIALIZE_DUT"))
                    {
                        int result = 0;
                        do
                        {
                            line = csrall.ReadLine().Trim().Replace("\t", "");
                            if (line.Contains("ERROR_MESSAGE") && !line.Contains("Function completed."))
                            {
                                result = 1;
                                break;
                            }
                        }
                        while (!line.Contains("ERROR_MESSAGE"));
                        status_ATS.AddData("INITIALIZE_DUT", "", 0, 0, result, GetErrorCode("INITIALIZE_DUT"));
                    }
                    #endregion

                    #region 4.CONNECT_IQ_TESTER
                    if (line.Contains(".CONNECT_IQ_TESTER"))
                    {
                        int result = 0;
                        do
                        {
                            line = csrall.ReadLine().Trim().Replace("\t", "");
                            if (line.Contains("ERROR_MESSAGE") && !line.Contains("Function completed."))
                            {
                                result = 1;
                                break;
                            }
                        }
                        while (!line.Contains("ERROR_MESSAGE"));
                        status_ATS.AddData("CONNECT_IQ_TESTER", "", 0, 0, result, GetErrorCode("CONNECT_IQ_TESTER"));
                    }
                    #endregion

                    #region 5.LOAD_PATH_LOSS_TABLE
                    if (line.Contains(".LOAD_PATH_LOSS_TABLE"))
                    {
                        int result = 0;
                        do
                        {
                            line = csrall.ReadLine().Trim().Replace("\t", "");
                            if (line.Contains("ERROR_MESSAGE") && !line.Contains("Function completed."))
                            {
                                result = 1;
                                break;
                            }
                        }
                        while (!line.Contains("ERROR_MESSAGE"));
                        status_ATS.AddData("LOAD_PATH_LOSS_TABLE", "", 0, 0, result, GetErrorCode("LOAD_PATH_LOSS_TABLE"));
                    }
                    #endregion

                    #region 8.INPUT_MAC_ADDRESS
                    if (line.Contains(".INPUT_MAC_ADDRESS"))
                    {
                        int result = 0;
                        do
                        {
                            line = csrall.ReadLine().Trim().Replace("\t", "");
                            if (line.Contains("ERROR_MESSAGE") && !line.Contains("Function completed."))
                            {
                                result = 1;
                                break;
                            }
                        }
                        while (!line.Contains("ERROR_MESSAGE"));
                        status_ATS.AddData("INPUT_MAC_ADDRESS", "", 0, 0, result, GetErrorCode("INPUT_MAC_ADDRESS"));
                    }
                    #endregion

                    //#region .XTAL_CALIBRATION .TX_SWEEP_VERIFY .RX_SWEEP_VERIFY
                    //if (line.Contains(".XTAL_CALIBRATION"))
                    //{
                    //    int result = 0;
                    //    string item = line.Trim().Split('.')[1];
                    //    string[] list = item.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    //    item = list[0] + "_" + list[1];
                    //    do
                    //    {
                    //        line = csrall.ReadLine().Trim().Replace("\t", "");
                    //        if (line.Contains("ERROR_MESSAGE") && !line.Contains("Function completed."))
                    //        {
                    //            result = 1;
                    //            break;
                    //        }
                    //    }
                    //    while (!line.Contains("ERROR_MESSAGE"));
                    //    status_ATS.AddData(item, "", 0, 0, result, GetErrorCode(item));
                    //}
                    //#endregion

                    #region .TX_VERIFY
                    if (line.Contains(".TX_VERIFY EVM"))
                    {
                        string item = line.Trim().Split('.')[1];
                        string[] list = item.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        item = testItem.ToString() + "_" + list[0] + "_" + list[5] + "_" + list[6] + "_" + list[8] + "_" + list[9];
                        do
                        {
                            line = csrall.ReadLine().Trim().Replace("\t", "");

                            if (line.Contains("FREQ_ERROR_AVG"))
                            {
                                // FREQ_ERROR_AVG_ALL -> Frequncy Offset
                                string LSL = GetLSL(line);
                                string USL = GetUSL(line);
                                string value = GetMiddleString(line, ":", "ppm").Trim();

                                //status_ATS.AddData(item + "_FREQ_ERROR_AVG", "", LSL, USL, value, GetErrorCode(item + "_FREQ_ERROR_AVG"));
                                AddData(item + "_FREQ_ERROR_AVG", "", LSL, USL, value, GetErrorCode(item + "_FREQ_ERROR_AVG"));
                            }
                            else if (line.Contains("VIOLATION_PERCENT"))
                            {
                                // VIOLATION_PERCENT_VSA_1-> Tx Mask 
                                string LSL = GetLSL(line);
                                string USL = GetUSL(line);
                                string value = GetMiddleString(line, ":", "%").Trim();

                                //status_ATS.AddData(item + "_VIOLATION_PERCENT", "", LSL, USL, value, GetErrorCode(item + "_VIOLATION_PERCENT"));
                                AddData(item + "_VIOLATION_PERCENT", "", LSL, USL, value, GetErrorCode(item + "_VIOLATION_PERCENT"));
                            }
                            else if (line.Contains("EVM_DB_AVG_S1"))
                            {
                                // EVM_DB_AVG_S1	-> TX EVM
                                string LSL = GetLSL(line);
                                string USL = GetUSL(line);
                                string value = GetMiddleString(line, ":", "dB").Trim();

                                //status_ATS.AddData(item + "_EVM_DB_AVG_S1", "", LSL, USL, value, GetErrorCode(item + "_EVM_DB_AVG_S1"));
                                AddData(item + "_EVM_DB_AVG_S1", "", LSL, USL, value, GetErrorCode(item + "_EVM_DB_AVG_S1"));
                            }
                            else if (line.Contains("POWER_RMS_AVG_VSA1"))
                            {
                                // POWER_DBM_RMS_AVG_S1 -> TX Power
                                string LSL = GetLSL(line);
                                string USL = GetUSL(line);
                                string value = GetMiddleString(line, ":", "dBm").Trim();

                                //status_ATS.AddData(item + "_POWER_DBM_RMS_AVG_S1", "", LSL, USL, value, GetErrorCode(item + "_POWER_DBM_RMS_AVG_S1"));
                                AddData(item + "POWER_RMS_AVG_VSA1", "", LSL, USL, value, GetErrorCode(item + "POWER_RMS_AVG_VSA1"));
                            }
                        }
                        while (!line.Contains("ERROR_MESSAGE"));
                    }
                    #endregion

                    #region .RX_VERIFY
                    if (line.Contains(".RX_VERIFY") && !line.Contains(".RX_VERIFY_PER"))
                    {
                        string item = line.Trim().Split('.')[1];
                        string[] list = item.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        item = testItem.ToString() + "_" + list[0] + "_" + list[2] + "_" + list[3] + "_" + list[5] + "_" + list[6];
                        do
                        {
                            line = csrall.ReadLine().Trim().Replace("\t", "");

                            if (line.Contains("PER") && !line.Contains("MEASUREMENTS"))
                            {
                                string LSL = GetLSL(line);
                                string USL = GetUSL(line);
                                string value = GetMiddleString(line, ":", "%").Trim();

                                //status_ATS.AddData(item + "_PER", "", LSL, USL, value, GetErrorCode(item + "_PER"));
                                AddData(item + "_PER", "", LSL, USL, value, GetErrorCode(item + "_PER"));
                            }
                        }
                        while (!line.Contains("ERROR_MESSAGE"));
                    }
                    #endregion

                    //Rena_20230424, add for BLE TX
                    #region .TX_LE
                    if (line.Contains(".TX_LE"))
                    {
                        string item = line.Trim().Split('.')[1];
                        string[] list = item.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        item = testItem.ToString() + "_" + list[0] + "_" + list[1] + "_" + list[2];
                        do
                        {
                            line = csrall.ReadLine().Trim().Replace("\t", "");

                            if (line.Contains("DELTA_F0_Fn_MAX"))
                            {
                                string LSL = GetLSL(line);
                                string USL = GetUSL(line);
                                string value = GetMiddleString(line, ":", "kHz").Trim();

                                //status_ATS.AddData(item + "_DELTA_F0_Fn_MAX", "", LSL, USL, value, GetErrorCode(item + "_DELTA_F0_Fn_MAX"));
                                AddData(item + "_DELTA_F0_Fn_MAX", "", LSL, USL, value, GetErrorCode(item + "_DELTA_F0_Fn_MAX"));
                            }
                            else if (line.Contains("DELTA_F1_AVERAGE"))
                            {
                                string LSL = GetLSL(line);
                                string USL = GetUSL(line);
                                string value = GetMiddleString(line, ":", "kHz").Trim();

                                //status_ATS.AddData(item + "_DELTA_F1_AVERAGE", "", LSL, USL, value, GetErrorCode(item + "_DELTA_F1_AVERAGE"));
                                AddData(item + "_DELTA_F1_AVERAGE", "", LSL, USL, value, GetErrorCode(item + "_DELTA_F1_AVERAGE"));
                            }
                            else if (line.Contains("DELTA_F1_F0"))
                            {
                                string LSL = GetLSL(line);
                                string USL = GetUSL(line);
                                string value = GetMiddleString(line, ":", "kHz").Trim();

                                //status_ATS.AddData(item + "_DELTA_F1_F0", "", LSL, USL, value, GetErrorCode(item + "_DELTA_F1_F0"));
                                AddData(item + "_DELTA_F1_F0", "", LSL, USL, value, GetErrorCode(item + "_DELTA_F1_F0"));
                            }
                            else if (line.Contains("DELTA_F2_F1_AV_RATIO"))
                            {
                                string LSL = GetLSL(line);
                                string USL = GetUSL(line);
                                string value = GetMiddleString(line, ":", "(").Trim();

                                //status_ATS.AddData(item + "_DELTA_F2_F1_AV_RATIO", "", LSL, USL, value, GetErrorCode(item + "_DELTA_F2_F1_AV_RATIO"));
                                AddData(item + "_DELTA_F2_F1_AV_RATIO", "", LSL, USL, value, GetErrorCode(item + "_DELTA_F2_F1_AV_RATIO"));
                            }
                            else if (line.Contains("DELTA_Fn_Fn5_MAX"))
                            {
                                string LSL = GetLSL(line);
                                string USL = GetUSL(line);
                                string value = GetMiddleString(line, ":", "kHz").Trim();

                                //status_ATS.AddData(item + "_DELTA_Fn_Fn5_MAX", "", LSL, USL, value, GetErrorCode(item + "_DELTA_Fn_Fn5_MAX"));
                                AddData(item + "_DELTA_Fn_Fn5_MAX", "", LSL, USL, value, GetErrorCode(item + "_DELTA_Fn_Fn5_MAX"));
                            }
                            else if (line.Contains("INITIAL_FREQ_OFFSET"))
                            {
                                string LSL = GetLSL(line);
                                string USL = GetUSL(line);
                                string value = GetMiddleString(line, ":", "kHz").Trim();

                                //status_ATS.AddData(item + "_INITIAL_FREQ_OFFSET", "", LSL, USL, value, GetErrorCode(item + "_INITIAL_FREQ_OFFSET"));
                                AddData(item + "_INITIAL_FREQ_OFFSET", "", LSL, USL, value, GetErrorCode(item + "_INITIAL_FREQ_OFFSET"));
                            }
                            else if (line.Contains("POWER_AVERAGE_DBM"))
                            {
                                string LSL = GetLSL(line);
                                string USL = GetUSL(line);
                                string value = GetMiddleString(line, ":", "dBm").Trim();

                                //status_ATS.AddData(item + "_POWER_AVERAGE_DBM", "", LSL, USL, value, GetErrorCode(item + "_POWER_AVERAGE_DBM"));
                                AddData(item + "_POWER_AVERAGE_DBM", "", LSL, USL, value, GetErrorCode(item + "_POWER_AVERAGE_DBM"));
                            }
                            //Rena_20230711, add ACP_MAX_POWER_DBM_OFFSET item
                            else if (line.Contains("ACP_MAX_POWER_DBM_OFFSET"))
                            {
                                string LSL = GetLSL(line);
                                string USL = GetUSL(line);
                                string value = GetMiddleString(line, ":", "dBm").Trim();
                                string item_name = line.Split(':')[0].Trim();

                                //上下限都沒設定時就忽略
                                if (LSL != "-999" || USL != "999")
                                {
                                    AddData(item + "_" + item_name, "", LSL, USL, value, GetErrorCode(item + "_" + item_name));
                                }
                            }
                        }
                        while (!line.Contains("ERROR_MESSAGE"));
                    }
                    #endregion

                    //Rena_20230424, add for BLE RX
                    #region .RX_LE
                    if (line.Contains(".RX_LE"))
                    {
                        string item = line.Trim().Split('.')[1];
                        string[] list = item.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        item = testItem.ToString() + "_" + list[0] + "_" + list[1] + "_" + list[2];
                        do
                        {
                            line = csrall.ReadLine().Trim().Replace("\t", "");

                            if (line.Contains("RX_POWER_LEVEL"))
                            {
                                string value = GetMiddleString(line, ":", "dBm").Trim();
                                item = item + "_POWER_" + value;
                            }
                            else if (line.Contains("PER"))
                            {
                                string LSL = GetLSL(line);
                                string USL = GetUSL(line);
                                string value = GetMiddleString(line, ":", "%").Trim();

                                //status_ATS.AddData(item + "_PER", "", LSL, USL, value, GetErrorCode(item + "_PER"));
                                AddData(item + "_PER", "", LSL, USL, value, GetErrorCode(item + "_PER"));
                            }
                        }
                        while (!line.Contains("ERROR_MESSAGE"));
                    }
                    #endregion

                    //Rena_20230530, add for Thread TX
                    #region .TX_MULTI_VERIFICATION
                    if (line.Contains(".TX_MULTI_VERIFICATION"))
                    {
                        string item = line.Trim().Split('.')[1];
                        string[] list = item.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        item = testItem.ToString() + "_" + list[0] + "_" + list[1];
                        do
                        {
                            line = csrall.ReadLine().Trim().Replace("\t", "");

                            //Rena_20230710 modify test item by RD request
                            //if (line.Contains("TX_POWER_DBM") && line.Contains("("))
                            //{
                            //    string LSL = GetLSL(line);
                            //    string USL = GetUSL(line);
                            //    string value = GetMiddleString(line, ":", "dBm").Trim();

                            //    AddData(item + "_TX_POWER_DBM", "", LSL, USL, value, GetErrorCode(item + "_TX_POWER_DBM"));
                            //}
                            if (line.Contains("POWER_AVERAGE"))
                            {
                                string LSL = GetLSL(line);
                                string USL = GetUSL(line);
                                string value = GetMiddleString(line, ":", "dBm").Trim();

                                AddData(item + "_POWER_AVERAGE", "", LSL, USL, value, GetErrorCode(item + "_POWER_AVERAGE"));
                            }
                            else if (line.Contains("EVM_ALL_PERCENT"))
                            {
                                string LSL = GetLSL(line);
                                string USL = GetUSL(line);
                                string value = GetMiddleString(line, ":", "%").Trim();

                                AddData(item + "_EVM_ALL_PERCENT", "", LSL, USL, value, GetErrorCode(item + "_EVM_ALL_PERCENT"));
                            }
                            else if (line.Contains("FREQ_ERROR") && !line.Contains("FREQ_ERROR_AVG_PPM"))
                            {
                                string LSL = GetLSL(line);
                                string USL = GetUSL(line);
                                string value = GetMiddleString(line, ":", "KHz").Trim();

                                AddData(item + "_FREQ_ERROR", "", LSL, USL, value, GetErrorCode(item + "_FREQ_ERROR"));
                            }
                        }
                        while (!line.Contains("ERROR_MESSAGE"));
                    }
                    #endregion

                    //Rena_20230530, add for Thread RX
                    #region .RX_VERIFY_PER
                    if (line.Contains(".RX_VERIFY_PER"))
                    {
                        string item = line.Trim().Split('.')[1];
                        string[] list = item.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        item = testItem.ToString() + "_" + list[0] + "_" + list[1];
                        do
                        {
                            line = csrall.ReadLine().Trim().Replace("\t", "");

                            if (line.Contains("PER"))
                            {
                                string LSL = GetLSL(line);
                                string USL = GetUSL(line);
                                string value = GetMiddleString(line, ":", "%").Trim();

                                //status_ATS.AddData(item + "_PER", "", LSL, USL, value, GetErrorCode(item + "_PER"));
                                AddData(item + "_PER", "", LSL, USL, value, GetErrorCode(item + "_PER"));
                            }
                        }
                        while (!line.Contains("ERROR_MESSAGE"));
                    }
                    #endregion

                    #region .READ_MAC_ADDRESS
                    if (line.Contains(".READ_MAC_ADDRESS"))
                    {
                        int result = 0;
                        do
                        {
                            line = csrall.ReadLine().Trim().Replace("\t", "");
                            if (line.Contains("ERROR_MESSAGE") && !line.Contains("Function completed."))
                            {
                                result = 1;
                                break;
                            }
                        }
                        while (!line.Contains("ERROR_MESSAGE"));
                        status_ATS.AddData("READ_MAC_ADDRESS", "", 0, 0, result, GetErrorCode(".READ_MAC_ADDRESS"));
                    }
                    #endregion

                    #region .DISCONNECT_IQ_TESTER
                    if (line.Contains(".DISCONNECT_IQ_TESTER"))
                    {
                        int result = 0;
                        do
                        {
                            line = csrall.ReadLine().Trim().Replace("\t", "");
                            if (line.Contains("ERROR_MESSAGE") && !line.Contains("Function completed."))
                            {
                                result = 1;
                                break;
                            }
                        }
                        while (!line.Contains("ERROR_MESSAGE"));
                        status_ATS.AddData("DISCONNECT_IQ_TESTER", "", 0, 0, result, GetErrorCode("DISCONNECT_IQ_TESTER"));
                    }
                    #endregion

                    #region .REMOVE_DUT
                    if (line.Contains(".REMOVE_DUT"))
                    {
                        int result = 0;
                        do
                        {
                            line = csrall.ReadLine().Trim().Replace("\t", "");
                            if (line.Contains("ERROR_MESSAGE") && !line.Contains("Function completed."))
                            {
                                result = 1;
                                break;
                            }
                        }
                        while (!line.Contains("ERROR_MESSAGE"));
                        status_ATS.AddData("REMOVE_DUT", "", 0, 0, result, GetErrorCode("REMOVE_DUT"));
                    }
                    #endregion

                    if (line.Contains("*  P A S S  *"))
                    {
                        AddData(test_item, 0);
                        //status_ATS.AddDataLog("Calibration_" + testItem, "Pass", "Pass", "000000");
                        break;
                    }
                    if (line.Contains("*  F A I L  *"))
                    {
                        AddData(test_item, 1);
                        //status_ATS.AddDataLog("Calibration_" + testItem, "Pass", "NG", "000000");
                        break;
                    }
                }
                cser.Close();
                csrall.Close();
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, "Analyze_RFLog error, ret: " + ex.Message);
                //status_ATS.AddDataLog("Parsing", NG);
                AddData(test_item, 1);
            }
        }

        private void EnterWiFiTestMode(PortType portType)
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            string res = "";
            string keyword = "";
            string item = $"EnterWiFiTestMode";
            string PC_IP = Func.ReadINI("Setting", "IP", "PC", "192.168.1.66");

            try
            {
                if (portType == PortType.UART)
                    keyword = "root@OpenWrt:/# \r\n"; //避免誤判到指令第一行的"root@OpenWrt:/#"
                else
                    keyword = "root@OpenWrt:~# \r\n"; //避免誤判到指令第一行的"root@OpenWrt:~#"

                DisplayMsg(LogType.Log, "Enter WiFi test mode");
                DisplayMsg(LogType.Log, $"PC_IP : {PC_IP}");
                // =========================================================================================
                //SendAndChk(portType, "wifi down", keyword, 0, 30 * 1000);
                //SendAndChk(portType, "rmmod ecm_wifi_plugin", keyword, 0, 3000);
                //SendAndChk(portType, "rmmod monitor", keyword, 0, 10 * 1000);
                //SendAndChk(portType, "rmmod wifi_3_0", keyword, 0, 10 * 1000);
                //SendAndChk(portType, "rmmod qca_ol", keyword, 0, 3000);
                //SendAndChk(portType, "insmod qca_ol testmode=1", keyword, 0, 5000);
                //SendAndChk(portType, "insmod wifi_3_0", keyword, 0, 20 * 1000);

                ////diag_socket_app connect前要先打開QUTS並設定好
                //SendAndChk(portType, $"diag_socket_app -a {PC_IP} &", "logging switched", out res, 0, 30 * 1000);
                //if (!res.Contains("Successful connect to address:"))
                //{
                //    DisplayMsg(LogType.Log, $"Connect to {PC_IP} fail");
                //    AddData(item, 1);
                //    return;
                //}
                //SendAndChk(portType, "/etc/init.d/ftm start", keyword, 0, 3000);
                //SendAndChk(portType, "/usr/sbin/ftm -n -c /tmp/ftm.conf &", keyword, 0, 3000);

                //DisplayMsg(LogType.Log, "Delay 3s...");
                //System.Threading.Thread.Sleep(3000);
                // ==============================================================
                //   ==================  SAM involved ATH =====================
                // ==============================================================
                SendAndChk(portType, "rmmod ath11k_pci", keyword, 0, 30 * 1000);
                SendAndChk(portType, "rmmod ath11k_ahb", keyword, 0, 3000);
                SendAndChk(portType, "rmmod ath11k", keyword, 0, 10 * 1000);
                SendAndChk(portType, "insmod ath11k ftm_mode=1 debug_mask=0x440", keyword, 0, 10 * 1000);
                SendAndChk(portType, "insmod ath11k_pci", keyword, 0, 3000);
                SendAndChk(portType, "insmod ath11k_ahb", keyword, 0, 5000);
                SendAndChk(portType, "rmmod ath12k", keyword, 0, 10 * 1000);
                SendAndChk(portType, "insmod ath12k.ko dyndbg=+p ftm_mode=1", keyword, 0, 20 * 1000);
                SendAndChk(portType, $"diag_socket_app -a {PC_IP} &", keyword, out res, 3000, 30 * 1000);
                if (!res.Contains("logging switched"))
                {
                    //SendAndChk(portType, "\r\n", keyword, 0, 1000);
                    DisplayMsg(LogType.Log, $"logging switched not exist failed");
                    AddData(item, 1);
                    return;
                }
                //------------------------------------------------
                SendCommand(portType, "\r\n", 1000);
                //------------------------------------------------
                //if (!res.Contains("Successful connect to address:"))
                //{
                //    DisplayMsg(LogType.Log, $"Connect to {PC_IP} fail");
                //    AddData(item, 1);
                //    return;
                //}
                SendAndChk(portType, "cp /etc/wifi/ftm.conf /tmp/", keyword, out res, 0, 3000);
                SendAndChk(portType, "/usr/sbin/ftm -n -c /tmp/ftm.conf &", keyword, out res, 1000, 3000);
                // ==============================================================
                //   ==================  SAM involved ATH =====================
                // ==============================================================
                DisplayMsg(LogType.Log, "Delay 3s...");
                System.Threading.Thread.Sleep(3000);
                AddData(item, 0);
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData(item, 1);
            }
        }
        private void ModifySerialMAC()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            string item = "ModifySerialMAC";
            string text = "";
            string MacFile_2G = Func.ReadINI("Setting", "RF", "MacFile_2G", "");
            string MacFile_5G = Func.ReadINI("Setting", "RF", "MacFile_5G", "");
            string MacFile_6G = Func.ReadINI("Setting", "RF", "MacFile_6G", "");

            try
            {
                DisplayMsg(LogType.Log, "Modify MAC Address in Serial_MAC.txt");

                DisplayMsg(LogType.Log, $"MacFile_2G: {MacFile_2G}");
                DisplayMsg(LogType.Log, $"MacFile_5G: {MacFile_5G}");
                DisplayMsg(LogType.Log, $"MacFile_6G: {MacFile_6G}");

                DisplayMsg(LogType.Log, $"Mac_2G: {infor.WiFiMAC_2G}");
                DisplayMsg(LogType.Log, $"Mac_5G: {infor.WiFiMAC_5G}");
                DisplayMsg(LogType.Log, $"Mac_6G: {infor.WiFiMAC_6G}");

                // check if file exist
                if (!File.Exists(MacFile_2G) || !File.Exists(MacFile_5G) || !File.Exists(MacFile_6G))
                {
                    DisplayMsg(LogType.Error, "MacFile doesn't exist");
                    AddData(item, 1);
                    return;
                }

                //modify WiFi 2G MAC
                string pattern = "STATIC_MAC_ADDRESS	=	\\w+";
                string replacement = "STATIC_MAC_ADDRESS	=	" + infor.WiFiMAC_2G.Replace(":", "");
                text = File.ReadAllText(MacFile_2G);
                Match match = Regex.Match(text, pattern);
                if (match.Success)
                {
                    text = Regex.Replace(text, pattern, replacement, RegexOptions.Multiline);
                }
                else
                {
                    DisplayMsg(LogType.Error, $"Can't find 'STATIC_MAC_ADDRESS' in {MacFile_2G}");
                    AddData(item, 1);
                    return;
                }
                File.WriteAllText(MacFile_2G, text);
                DisplayMsg(LogType.Log, "Modify WiFi 2G MAC successfully");

                //modify WiFi 5G MAC
                replacement = "STATIC_MAC_ADDRESS	=	" + infor.WiFiMAC_5G.Replace(":", "");
                text = "";
                text = File.ReadAllText(MacFile_5G);
                match = Regex.Match(text, pattern);
                if (match.Success)
                {
                    text = Regex.Replace(text, pattern, replacement, RegexOptions.Multiline);
                }
                else
                {
                    DisplayMsg(LogType.Error, $"Can't find 'STATIC_MAC_ADDRESS' in {MacFile_5G}");
                    AddData(item, 1);
                    return;
                }
                File.WriteAllText(MacFile_5G, text);
                DisplayMsg(LogType.Log, "Modify WiFi 5G MAC successfully");

                //modify WiFi 6G MAC
                replacement = "STATIC_MAC_ADDRESS	=	" + infor.WiFiMAC_6G.Replace(":", "");
                text = "";
                text = File.ReadAllText(MacFile_6G);
                match = Regex.Match(text, pattern);
                if (match.Success)
                {
                    text = Regex.Replace(text, pattern, replacement, RegexOptions.Multiline);
                }
                else
                {
                    DisplayMsg(LogType.Error, $"Can't find 'STATIC_MAC_ADDRESS' in {MacFile_6G}");
                    AddData(item, 1);
                    return;
                }
                File.WriteAllText(MacFile_6G, text);
                DisplayMsg(LogType.Log, "Modify WiFi 6G MAC successfully");

                AddData(item, 0);
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData(item, 1);
            }
        }
        private void CheckDECTMode(string mode)
        {
            if (!CheckGoNoGo())
            {
                return;
            }
            bool result = false;
            string res = "";
            string keyword = "menu";
            string test_mode = "";
            string item = $"CheckDECTMode";
            int delayMs = 3000;
            int retry = 0;
        retryCMB:
            try
            {
                DisplayMsg(LogType.Log, " ============== Check DECT mode ==============");
                for (int i = 0; i < 3; i++)
                {
                    SendAndChk(PortType.SSH, "cmbs_tcx -comname ttyMSM2 -baud 460800", keyword, out res, delayMs, 4000);
                    if (res.Contains("q => Quit"))
                    {
                        result = true;
                        break;
                    }
                    DisplayMsg(LogType.Log, "Retry cmbs Delay 3s...");
                    Thread.Sleep(3000);
                }
                if (!result)
                {
                    if (!this.killCMBS(PortType.GOLDEN_SSH))
                    {
                        AddData(item, 1);
                        return;
                    }
                    if (retry++ > 3)
                    {
                        return;
                    }
                    goto retryCMB;
                    //Thread thread = new Thread(() => {
                    //});
                    //thread.Start();
                }
                //--------------------------------------------------------------------------------------------
                DisplayMsg(LogType.Log, "Transmitted 'x' to device");
                SSH_stream.Write("x\r");
                ChkResponse(PortType.SSH, ITEM.NONE, "q) Quit", out res, 5000);
                //check TestMode
                Match m = Regex.Match(res, @"TestMode:\s+(?<dect_test_mode>.+)");
                if (m.Success)
                {
                    test_mode = m.Groups["dect_test_mode"].Value.Trim();
                }
                DisplayMsg(LogType.Log, $"DECT test mode: {test_mode}");

                if (string.Compare(test_mode, mode, true) == 0)
                {
                    DisplayMsg(LogType.Log, $"Check TestMode:{test_mode} pass");
                    AddData(item, 0);
                }
                else
                {
                    DisplayMsg(LogType.Log, $"Check TestMode:{test_mode} fail");
                    MessageBox.Show($"{test_mode} testMode fault=> testMode shall None");
                    AddData(item, 1);
                    return;
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData(item, 1);
            }
            finally
            {
                if (this.exitCalMenu("q => Quit"))
                {
                    DisplayMsg(LogType.Log, "exit calibration menu OK");
                }
                else
                {
                    warning = "exit_calibration_menu_NG";
                    DisplayMsg(LogType.Log, "exit calibration menu >>>> NG");
                }
            }
        }
        private bool DECTAnalysisLog(string logpath)
        {
            try
            {
                Net.NetPort netport = new Net.NetPort();
                string itemPrefix = "";
                string item = "";
                string unit = "";
                string value = "";
                string lsl = "";
                string usl = "";
                double doubleLSL = -999;
                double doubleUSL = 999;
                string[] lines = File.ReadAllLines(logpath);

                double doubleValue = -9999;
                foreach (var line in lines)
                {
                    value = "";
                    unit = "";
                    lsl = "";
                    usl = "";
                    doubleLSL = -999;
                    doubleUSL = 999;
                    doubleValue = -9999;

                    if (line.Contains("DECT_TX"))
                        itemPrefix = "DECT_TX";
                    else if (line.Contains("DECT_RX_BER"))
                        itemPrefix = "DECT_RX_BER";


                    if (line.Contains("Frequency:"))
                    {
                        string frequency = line.Split(',')[0].Replace(" ", "_");
                        string car = line.Split(',')[1];
                        string power = line.Split(',')[2].Replace("-", "_");
                        string ant = line.Split(',')[3];

                        itemPrefix += "_Freq_" + frequency.Split(':')[1].Trim()
                            + "_" + ant.Split(':')[1].Trim()
                            + "_" + power.Split(':')[1].Trim();
                    }
                    else if (line.Contains("\tPower ") && !line.Contains("vs Time ")) // Power
                    {
                        unit = Regex.Split(line, @"\s+")[3].Trim(); // unit
                        item = itemPrefix + "_Power_" + unit;
                        value = Regex.Split(line, @"\s+")[2].Trim();
                        usl = netport.getMiddleString(line, "(", "~").Trim();
                        lsl = netport.getMiddleString(line, "~", ")").Trim();
                    }
                    else if (line.Contains("\tFreq Error")) //Freq Error
                    {
                        unit = Regex.Split(line, @"\s+")[4].Trim(); // unit
                        item = itemPrefix + "_Freq_Error_" + unit;
                        value = Regex.Split(line, @"\s+")[3].Trim();
                        usl = netport.getMiddleString(line, "(", "~").Trim();
                        lsl = netport.getMiddleString(line, "~", ")").Trim();
                    }
                    else if (line.Contains("\tPER ")) // PER
                    {
                        unit = Regex.Split(line, @"\s+")[3].Trim(); // unit
                        item = itemPrefix + "_PER_" + unit;
                        value = Regex.Split(line, @"\s+")[2].Trim();
                        usl = netport.getMiddleString(line, "(", "~").Trim();
                        lsl = netport.getMiddleString(line, "~", ")").Trim();
                    }
                    else if (line.Contains("\tBER ")) // BER
                    {
                        unit = Regex.Split(line, @"\s+")[3].Trim(); // unit
                        item = itemPrefix + "_BER_" + unit;
                        value = Regex.Split(line, @"\s+")[2].Trim();
                        usl = netport.getMiddleString(line, "(", "~").Trim();
                        lsl = netport.getMiddleString(line, "~", ")").Trim();
                    }
                    ////=======================================================
                    ////=======================================================
                    //else if (line.Contains("\tTiming Accuracy ")) // Timing Accuracy
                    //{
                    //    unit = Regex.Split(line, @"\s+")[3].Trim(); // unit
                    //    item = itemPrefix + "_Freq_Offset_" + unit;
                    //    value = Regex.Split(line, @"\s+")[2].Trim();
                    //    usl = netport.getMiddleString(line, "(", "~").Trim();
                    //    lsl = netport.getMiddleString(line, "~", ")").Trim();
                    //}
                    ////=======================================================
                    ////=======================================================
                    else if (line.Contains("\tFreq Offset ")) // Freq Offset
                    {
                        unit = Regex.Split(line, @"\s+")[3].Trim(); // unit
                        item = itemPrefix + "_Freq_Offset_" + unit;
                        value = Regex.Split(line, @"\s+")[2].Trim();
                        usl = netport.getMiddleString(line, "(", "~").Trim();
                        lsl = netport.getMiddleString(line, "~", ")").Trim();
                    }
                    else if (line.Contains("\tFreq Drift ")) // Freq Drift
                    {
                        unit = Regex.Split(line, @"\s+")[4].Trim(); // unit
                        item = itemPrefix + "_Freq_Drift_" + unit;
                        value = Regex.Split(line, @"\s+")[3].Trim();
                        usl = netport.getMiddleString(line, "(", "~").Trim();
                        lsl = netport.getMiddleString(line, "~", ")").Trim();
                    }
                    else if (line.Contains("\tB Field Dev Neg ")) // B Field Dev Neg 
                    {
                        unit = Regex.Split(line, @"\s+")[6].Trim(); // unit
                        item = itemPrefix + "_B_Field_Dev_Neg_" + unit;
                        value = Regex.Split(line, @"\s+")[5].Trim();
                        usl = netport.getMiddleString(line, "(", "~").Trim();
                        lsl = netport.getMiddleString(line, "~", ")").Trim();
                    }
                    else if (line.Contains("\tB Field Dev Pos ")) // B Field Dev Pos 
                    {
                        unit = Regex.Split(line, @"\s+")[6].Trim(); // unit
                        item = itemPrefix + "_B_Field_Dev_Pos_" + unit;
                        value = Regex.Split(line, @"\s+")[5].Trim();
                        usl = netport.getMiddleString(line, "(", "~").Trim();
                        lsl = netport.getMiddleString(line, "~", ")").Trim();
                    }
                    if (Double.TryParse(value, out doubleValue) && Double.TryParse(usl, out doubleUSL) && Double.TryParse(lsl, out doubleLSL)) // value
                    {
                        //if (Func.ReadINI("SPEC", "SPEC", item + "_USL", "999.0") == "999.0")
                        //    Func.WriteINI("SPEC", "SPEC", item + "_USL", "999.0");
                        //if (Func.ReadINI("SPEC", "SPEC", item + "_LSL", "-999.0") == "-999.0")
                        //    Func.WriteINI("SPEC", "SPEC", item + "_LSL", "-999.0");
                        string errorCode = Func.ReadINI("SPEC", "Error_Code", item, "ABC00012");
                        if (errorCode == "ABC00012")
                            Func.WriteINI("SPEC", "Error_Code", item, "ABC00012");
                        //status_ATS.AddData(item, unit, doubleLSL, doubleUSL, doubleValue, errorCode);
                        AddData(item, unit, doubleLSL, doubleUSL, doubleValue, errorCode);
                    }
                }
                if (!File.ReadAllText(logpath).Contains("* P A S S *"))
                {
                    DisplayMsg(LogType.Log, "Check * P A S S * fail");
                    AddData("DECTLog", 1);
                    //status_ATS.AddDataLog("DECTLog", NG);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                warning = "DECTAnalysisLog Exception";
                return false;
            }
        }
        private void RemoveCalData(PortType portType)
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            string keyword = "root@OpenWrt:~# \r\n"; //避免誤判到指令第一行的"root@OpenWrt:~#"
            string item = "RemoveCalData";

            try
            {
                DisplayMsg(LogType.Log, "Remove Calibration Data");
                SendAndChk(portType, "rm /lib/firmware/qcn9224/caldata_*.bin", keyword, 0, 3600);
                //SendAndChk(PortType.SSH, "rm /lib/firmware/IPQ5332/caldata.bin", keyword, 0, 3600);
                //SendAndChk(PortType.SSH, "rm /lib/firmware/qcn9224/caldata_1.bin", keyword, 0, 3600);
                //SendAndChk(PortType.SSH, "rm /lib/firmware/qcn9224/caldata_2.bin", keyword, 0, 3600);

                SendAndChk(portType, "dd of=/tmp/mac if=/dev/mmcblk0p21 bs=1 count=30", keyword, 0, 10 * 1000);
                SendAndChk(portType, "dd if=/dev/zero of=/dev/mmcblk0p21", keyword, 0, 10 * 1000);
                SendAndChk(portType, "dd if=/tmp/mac of=/dev/mmcblk0p21 bs=1 count=30", keyword, 0, 10 * 1000);
                ////------------------------------------------------
                //SendAndChk(portType, "\r\n", keyword, 0, 1000);
                ////------------------------------------------------

                AddData(item, 0);
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData(item, 1);
                return;
            }
        }
        private void ChkD2License()
        {
            if (!CheckGoNoGo()) { return; }
            string item = "ChkD2License";
            try
            {
                DisplayMsg(LogType.Log, $"================ {item} ================");
                string keyword = "root@OpenWrt:~# \r\n";
                string res = string.Empty;
                int retryC = 0;
            retryFlag:
                SendAndChk(PortType.SSH, "mt boarddata", keyword, out res, 0, 5000);
                Match m = Regex.Match(res, @"D2License\s*=\s*(?<D2License>.+)");
                if (m.Success)
                {
                    string D2License = m.Groups["D2License"].Value.Trim();
                    //DisplayMsg(LogType.Log, "--------- Check" + D2License+ " with SFCS ---------");
                }
                SendAndChk(PortType.SSH, "/etc/init.d/vtspd start", keyword, out res, 0, 5000);
                SendAndChk(PortType.SSH, "ps | grep ve_vtsp_main", keyword, out res, 0, 5000);
                if (status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode)
                {
                    //if (!res.Contains("/usr/bin/ve_vtsp_main"))

                    if (!res.Contains("ve_vtsp_main"))
                    {
                        while (retryC++ < 2)
                        {
                            DisplayMsg(LogType.Log, "Check ve_vtsp_main fail, D2 License is incorrect");
                            //SendAndChk(PortType.SSH, "/etc/init.d/vtspd stop", keyword, out res, 0, 5000);  //RD-Smith suggest remove this part
                            goto retryFlag;
                        }
                        AddData("D2_License", 1);
                        return;
                    }
                    else
                    {
                        DisplayMsg(LogType.Log, "Check ve_vtsp_main PASS, D2 License is correct");
                        AddData("D2_License", 0);
                    }
                }
                //SendAndChk(PortType.SSH, "/etc/init.d/vtspd stop", keyword, out res, 0, 8000);

            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, item + "___" + ex.Message);
                AddData(item, 1);
                return;
            }
            finally
            {
                //Dispose(golden_SSH_client);
            }
        }
        private bool killCMBS(PortType portType)
        {
            string item = "killCMBS";
            string keyword = "root@OpenWrt:~# \r\n";
            string res = string.Empty;
            bool isKill = false;
            DisplayMsg(LogType.Log, "Enter DECT MENU fail, purge & retry");
            DisplayMsg(LogType.Log, $"================ {item} ================");
            try
            {
                for (int i = 0; i < 3; i++)
                {
                    if (SendAndChk(portType, "killall -9 cmbs_tcx", keyword, out res, 0, 8000))
                    {
                        isKill = true;
                        DisplayMsg(LogType.Log, $"{item} succeed");
                        break;
                    }
                }
                DisplayMsg(LogType.Log, $"---------------- {item} NG ----------------");

            }
            catch (Exception ex)
            {
                isKill = false;
                DisplayMsg(LogType.Exception, item + "___" + ex.Message);
                AddData(item, 1);
            }
            return isKill;
        }
        private bool SwitchDmpMode(string _version)
        {
            if (!CheckGoNoGo())
            {
                return false;
            }
        retry:
            bool IsFwGreater = false;
            int retryMtInFo = 0;
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
                }
                Version FwVer = Version.Parse(FWversion);
                if (FwVer.CompareTo(targetVerison) > 0)
                {
                    IsFwGreater = true;
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, $"{item}=>" + ex.Message);
                retryMtInFo++;
                if (retryMtInFo > 2)
                {
                    AddData(item, 1);
                    throw;
                }
                goto retry;
            }
            return IsFwGreater;
        }
        private void CheckBatteryDetection()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            string res = "";
            string item = "BatteryDetection";
            string keyword = @"root@OpenWrt";

            try
            {
                //When using standard adaptor, the AC_ALARM is low.
                //When using customized adaptor or power supply, the AC_ALARM is high.
                //Battery detection
                SendAndChk(PortType.SSH, "mt gpio dump all", "AC_ALARM", out res, 0, 5000);

                DisplayMsg(LogType.Log, "=============== Battery Detection Check ===============");
                item = "BatteryDetection";
                if (res.Contains("AC_ALARM: low"))
                {
                    AddData(item, 0);
                    DisplayMsg(LogType.Log, "Found out 'AC_ALARM: low' => Battery detection Pass");
                }
                else
                {
                    AddData(item, 1);
                    DisplayMsg(LogType.Log, "Not Found 'AC_ALARM: low' => Battery detection Fail");
                    return;
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                AddData(item, 1);
                return;
            }
        }
        private void CheckSecureBootEnable()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            string res = "";
            string item = "Check Secure Boot Enable";
            string keyword = @"root@OpenWrt";
            try
            {
                SendAndChk(PortType.SSH, "cat /sys/devices/system/qfprom/qfprom0/authenticate", keyword, out res, 0, 3000);
                if (!res.Contains("1"))
                {
                    DisplayMsg(LogType.Log, @"Secure boot not enable");
                    AddData(item, 1);
                    return;
                }
                DisplayMsg(LogType.Log, @"Secure boot enable ok!");
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                AddData(item, 1);
                return;
            }
        }

    }
}
