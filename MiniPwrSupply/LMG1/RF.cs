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
using WNC.API;

namespace MiniPwrSupply.LMG1
{
    public partial class frmMain
    {
        private string _RFTool = "";
        private string _RFLog = "";
        private string _RFTestPlan = "";
        private int _RFTimeOutSec = 0;
        private string Bridge_hostnam = "Bridge_golden";


        public enum RFTestItem
        {
            WiFi,
            BLE,
            Thread
        }

        private void RF_Test()
        {
            //CreateIQC();
            try
            {
                infor.ResetParam();
                //SE_TODO: get infor from SFCS
                if (status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode)
                {
                    //SentPsnForGetMAC(status_ATS.txtPSN.Text.Trim());
                    DisplayMsg(LogType.Log, "Delay 1s...");
                    Thread.Sleep(1000);
                    string SN_name = Func.ReadINI("Setting", "FirehoseFW", "SN", "@LRG1_SN");
                    string MAC_name = Func.ReadINI("Setting", "FirehoseFW", "BaseMAC", "@MAC");
                    #region Check SN base on format
                    SFCS_Query _sfcsQuery = new SFCS_Query();
                    ATS_Template.SFCS_ATS_2_0.ATS ss = new ATS_Template.SFCS_ATS_2_0.ATS();
                    int snLength = Convert.ToInt32(Func.ReadINI("Setting", "Match", "SN_Length", "11"));
                    string snStartwith = Func.ReadINI("Setting", "Match", "SN_Start", "T");
                    GetFromSfcs("@LRG1_SN", out infor.SerialNumber);
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
                    infor.BaseMAC = MACConvert(infor.BaseMAC);
                    DisplayMsg(LogType.Log, "Base MAC Convert" + infor.BaseMAC);
                    //WiFi 2.4G MAC = BaseMAC+4
                    infor.WiFiMAC_2G = MACConvert(infor.BaseMAC, 4);
                    //WiFi 5G MAC = BaseMAC+3
                    infor.WiFiMAC_5G = MACConvert(infor.BaseMAC, 3);
                    //WiFi 6G MAC = BaseMAC+2
                    infor.WiFiMAC_6G = MACConvert(infor.BaseMAC, 2);
                    DisplayMsg(LogType.Log, $"WiFiMAC_2G: {infor.WiFiMAC_2G}");
                    DisplayMsg(LogType.Log, $"WiFiMAC_5G: {infor.WiFiMAC_5G}");
                    DisplayMsg(LogType.Log, $"WiFiMAC_6G: {infor.WiFiMAC_6G}");
                }
                else
                {
                    //Rena_20230627, for HQ RF stress test
                    //get SN and BaseMAC
                    //GetBoardDataFromExcel(status_ATS.txtPSN.Text, true);
                    //GetFromSfcs("@LRG1_SN", out infor.SerialNumber);
                    //infor.BaseMAC = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@MAC");
                    //if (string.IsNullOrEmpty(infor.BaseMAC)) GetBoardDataFromExcel1();
                    //SetTextBox(status_ATS.txtPSN, infor.SerialNumber);
                    //if (isLoop == 0)
                    //    infor.BaseMAC = MACConvert(infor.BaseMAC);
                    //else
                    //    infor.BaseMAC = MACConvert("E8:C7:CF:AF:4D:D0", Loopcnt * 8); //for HQ stress test only
                    GetBoardDataFromExcel1();
                    //WiFi 2.4G MAC = BaseMAC+4
                    infor.WiFiMAC_2G = MACConvert(infor.BaseMAC, 4);
                    //WiFi 5G MAC = BaseMAC+3
                    infor.WiFiMAC_5G = MACConvert(infor.BaseMAC, 3);
                    //WiFi 6G MAC = BaseMAC+2
                    infor.WiFiMAC_6G = MACConvert(infor.BaseMAC, 2);
                    DisplayMsg(LogType.Log, $"BaseMAC: {infor.BaseMAC}");
                    DisplayMsg(LogType.Log, $"WiFiMAC_2G: {infor.WiFiMAC_2G}");
                    DisplayMsg(LogType.Log, $"WiFiMAC_5G: {infor.WiFiMAC_5G}");
                    DisplayMsg(LogType.Log, $"WiFiMAC_6G: {infor.WiFiMAC_6G}");
                }
                //if (!ChkStation(status_ATS.txtPSN.Text))
                //    return;
                //if (Func.ReadINI("Setting", "IO_Board_Control", "IO_Control_1", "0") == "1")
                //{
                //    string txPin = Func.ReadINI("Setting", "IO_Board_Control", "Pin0", "0");
                //    string rev_message = "";
                //    status_ATS.AddLog("IO_Board_Y" + txPin + " On...");
                //    IO_Board_Control1.ConTrolIOPort_write(Int32.Parse(txPin), "1", ref rev_message);
                //    DisplayMsg(LogType.Log, rev_message);
                //}
                //else if (Func.ReadINI("Setting", "Port", "RelayBoard", "Disable").ToUpper() == "ENABLE")
                {
                    SwitchRelay(CTRL.ON);
                    Thread.Sleep(5 * 1000);
                    SwitchRelay(CTRL.OFF);
                }
                //else
                //{
                //    if (isLoop == 0)
                //    {
                //        frmOK.Label = "Vui lòng cắm điện và nhấn nút nguồn để bật máy";
                //        frmOK.ShowDialog();
                //    }
                //}
                DisplayMsg(LogType.Log, "Power on!!!");
                //#region on power button
                //DisplayMsg(LogType.Log, $"Delay {Convert.ToInt32(WNC.API.Func.ReadINI(Application.StartupPath, "Setting", "TimeOut", "DelayPower", "1000"))}ms");
                //Thread.Sleep(Convert.ToInt32(WNC.API.Func.ReadINI(Application.StartupPath, "Setting", "TimeOut", "DelayPower", "1000")));
                //string cameraResult = "";
                //int n = 0;
                //retryBTN:
                //if (Camera())
                //{
                //    if (CheckCameraResult($"item_1", $"black", out cameraResult))
                //    {
                //        if (fixture.useFixture)
                //        {
                //            fixture.ControlIO(Fixture.FixtureIO.IO_5, CTRL.ON);
                //            fixture.ControlIO(Fixture.FixtureIO.IO_5, CTRL.OFF);
                //        }
                //        else
                //        {
                //            frmOK.Label = "DUT chưa lên nguồn, hãy bật nguồn sau đó nhấn [OK]";
                //            frmOK.ShowDialog();
                //        }
                //    }
                //}
                //if (Camera())
                //{
                //    if (CheckCameraResult($"item_1", $"black", out cameraResult))
                //    {
                //        if (fixture.useFixture)
                //        {
                //            n++;
                //            if (n < 3)
                //            {
                //                DisplayMsg(LogType.Log, "Retry -->");
                //                goto retryBTN;
                //            }
                //        }
                //        else
                //        {
                //            frmOK.Label = "DUT chưa lên nguồn, hãy bật nguồn sau đó nhấn [OK]";
                //            frmOK.ShowDialog();
                //        }
                //    }
                //}
                //#endregion

                ChkBootUp(PortType.SSH);
                // =============== remove bcoz testPlan testflow =====================
                //if (isLoop == 0)  //bcoz PCBA did not write MAC
                //    CheckEthernetMAC();
                // ==================================================================
                //BLE & Thread透過uart
                //這裡的uart是接到BLE bridge,不是接DUT
                if (Func.ReadINI("Setting", "RF", "SkipBLE", "0") == "0" || Func.ReadINI("Setting", "RF", "SkipThread", "0") == "0")
                {
                    UartDispose(uart);
                    //Frank modified 8/24 because of "Wrt:/#" + Bridge_hostnam command fail
                    ChkBootUp(PortType.UART, @"root@" + Bridge_hostnam);
                    //ChkBootUp(PortType.UART, "Wrt:/#");
                    CheckBridgeIP();
                }
                if (Func.ReadINI("Setting", "RF", "SkipBLE", "0") == "0")
                {
                    RF_BLE();
                }

                if (Func.ReadINI("Setting", "RF", "SkipThread", "0") == "0")
                {
                    RF_Thread();
                }

                if (Func.ReadINI("Setting", "RF", "SkipWiFi", "0") == "0")
                {
                    RF_WiFi();
                    EthernetTest(true);
                    if (!CheckGoNoGo()) { return; }
                    //this.ArtChksum();
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


        private void CheckBridgeIP()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            //string keyword = "root@Bridge_golden:/# \r\n";
            string keyword = "root@Bridge_golden";
            string item = "CheckBridgeIP";
            string res = "";
            string Bridge_IP = Func.ReadINI("Setting", "IP", "BLE_Bridge_IP", "192.168.1.5");

            try
            {
                DisplayMsg(LogType.Log, "Check BLE bridge IP");

                //避免uart接錯,所以要檢查BLE bridge IP,BLE bridge IP不應為192.168.1.1
                for (int i = 0; i < 3; i++)
                {
                    SendAndChk(PortType.UART, "ifconfig br-lan | grep \"inet addr\"", keyword, out res, 0, 3000);
                    if (res.Contains("inet addr"))
                    {
                        break;
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }

                if (res.Contains(Bridge_IP) && !res.Contains("192.168.1.1 "))
                {
                    DisplayMsg(LogType.Log, "Check BLE bridge IP pass");
                    AddData(item, 0);
                }
                else
                {
                    DisplayMsg(LogType.Log, "Check BLE bridge IP fail");
                    AddData(item, 1);
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
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
                //LMG1重開機機時會去抓先前產生的Cal data, 導致後續重K的值不會寫入, 所以必須先清除Cal data
                //if (!isGolden && status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode)
                {   // Must removeCalData() after ran EthernetTest() while running stress test
                    RemoveCalData();
                    RebootDUT();

                }

                EnterWiFiTestMode();

                //開始test前要關閉uart
                UartDispose(uart);

                ModifySerialMAC();
                if (!CheckGoNoGo()) { return; }
                RunIQFact(RFTestItem.WiFi);

                //if (!isGolden && status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode)
                //{
                //    CheckWiFiCalData();
                //}
                //else
                //{
                //    DisplayMsg(LogType.Log, "Golden DUT or Engineer mode skip WiFiCalData");
                //}


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
        }
        private void RF_BLE()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            DisplayMsg(LogType.Log, "=============== BLE Test ===============");

            string item = $"RF_{RFTestItem.BLE.ToString()}";
            string cmd = string.Empty;
            try
            {
                //切換BLE FW
                if (this.SwitchDmpMode("0.2.0.4")) // judge FW greater or smaller
                {
                    cmd = "bt_upgrade_utility -f /lib/firmware/efr32/bt_ncp_afh_se_coex_4_3_0_noTxlimit.gbl -p /dev/ttyMSM1";
                }
                else
                {
                    cmd = "bt_upgrade_utility -p /dev/ttyMSM1 -f /lib/firmware/efr32/bt_ncp_afh_se.gbl";
                }
                if (!SendAndChk(PortType.SSH, cmd, "Transfer completed successfully", 0, 60 * 1000))
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

                BLE_FTM_via_Bridge(RFTestItem.BLE);

                //Bring up BLE
                //SendCommand(PortType.UART, "microcom /dev/ttyMSM1 -s 115200 -X", 2000);

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
                Close_SSH_in_Bridge();
            }
        }
        private void RF_Thread()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            DisplayMsg(LogType.Log, "=============== Thread Test ===============");

            string keyword = @"root@OpenWrt";
            string item = $"RF_{RFTestItem.Thread.ToString()}";
            string res = "";

            try
            {
                SendAndChk(PortType.SSH, "killall microcom", keyword, out res, 3000, 5000);
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

                BLE_FTM_via_Bridge(RFTestItem.Thread);

                res = "";
                int retry = 0;
            Entercommand:
                if (SendAndChk(PortType.UART, "\r\n", ">", 2000, 3000))
                {
                    //Thread.Sleep(3000);
                    // SendCommand(PortType.UART, "\r\n","no command", 2000);
                    DisplayMsg(LogType.Log, "Get Ctune");
                    string Ctune = Func.ReadINI("Setting", "Setting", "Ctune", "xxxx");
                    // bool chkCtune = SendAndChk(PortType.UART, "getCtune", Ctune,out res, 2000, 3000);
                    bool chkCtune = SendAndChk(PortType.UART, "getCtune", Ctune, out res, 2000, 3000);
                    if (chkCtune)
                    {
                        DisplayMsg(LogType.Log, $"Get Ctune PASS Ctune is:{Ctune}");
                        status_ATS.AddDataRaw("Ctune", Ctune, Ctune, "000000");
                        AddData("Ctune", 0);
                    }
                    else
                    {
                        DisplayMsg(LogType.Log, $"Get Ctune Fail,DUT Ctune is:{res},Setting Ctune is: {Ctune}");
                        AddData("Ctune", 1);
                    }
                }
                else
                {
                    retry++;
                    if (retry < 3) goto Entercommand;
                    else
                    {
                        DisplayMsg(LogType.Log, $"Send Enter command fail ");
                        AddData("Enter Command", 1);
                    }
                }

                // ======================================  removal from test plan    ============================================
                //Bring up Thread
                //SendAndChk(PortType.UART, "bt_host_empty -u /dev/ttyMSM1", "Press Crtl+C to quit", out res, 0, 3000);
                //SendAndChk(PortType.UART, sCtrlC, keyword, out res, 500, 3000);
                // ===============================================================================================================
                SendCommand(PortType.UART, "microcom /dev/ttyMSM1 -s 115200 -X", 2000);

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
                Close_SSH_in_Bridge();
            }
        }
        private void BLE_FTM_via_Bridge(RFTestItem testItem)
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            string item = "Enable_FTM";
            //string keyword = "root@OpenWrt:~# \r\n";
            //string keyword_bridge = "root@Bridge_golden:/# \r\n";
            string keyword = "root@OpenWrt";
            string keyword_bridge = "root@Bridge_golden";
            string res = "";
            int retrytime = 0;
            try
            {
                DisplayMsg(LogType.Log, "Enable BLE FTM via bridge");
            pingRetry:
                DisplayMsg(LogType.Log, "------ Ping DUT via Golen ------");
                SendAndChk(PortType.UART, "ping 192.168.1.1 -c 1", keyword_bridge, out res, 0, 10 * 1000);
                //SendCommand(PortType.UART, sCtrlC, 500);
                if (res.Contains("1 packets received"))
                {
                    AddData("Golden_PING_DUT_PASS", 0);
                    DisplayMsg(LogType.Log, "Golden_PING_DUT_PASS");
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
                if (!SendAndChk(PortType.UART, "ssh 192.168.1.1 -y -y", keyword, out res, 0, 20 * 1000))
                {
                    DisplayMsg(LogType.Log, @"SSH into DUT NG");
                    AddData(item, 1);
                    return;
                }
                for (int i = 0; i < 5; i++)
                {
                    SendAndChk(PortType.SSH, "\n", "", out res, 0, 1000);
                    //DisplayMsg(LogType.Log, res);
                    if (res.Contains("root@Bridge_golden"))
                    {
                        break;
                    }
                }
                //In bridge debug console, SSH into DUT
                // ======================================================================================================
                //SendAndChk(PortType.UART, "ssh 192.168.1.1 -y -y", keyword, "Do you want to continue connecting?", out res, 0, 10 * 1000);
                //if (res.Contains("Do you want to continue connecting?"))
                //{
                //    SendAndChk(PortType.UART, "y", keyword, keyword_bridge, out res, 0, 5000);
                //}

                //if (!res.Contains("BusyBox") || !res.Contains(keyword))
                //{
                //    DisplayMsg(LogType.Log, "SSH into DUT fail");
                //    AddData(item, 1);
                //    return;
                //}
                // ======================================================================================================
                // ======================================== test plan remove this part =======================================
                //if (testItem == RFTestItem.Thread)
                //{
                //SendAndChk(PortType.UART, "bt_host_empty -u /dev/ttyMSM1", "Press Crtl+C to quit", out res, 0, 3000);
                //SendAndChk(PortType.UART, sCtrlC, keyword, out res, 500, 3000);
                //"test plan remove this part");
                //}
                // ======================================== test plan remove this part =======================================
                if (testItem == RFTestItem.BLE)
                {
                    SendAndChk(PortType.SSH, "bt_host_empty -u /dev/ttyMSM1 -v", keyword, out res, 0, 5000);
                }

                SendCommand(PortType.UART, "microcom /dev/ttyMSM1 -s 115200 -X", 2000);

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

            string DUT_IP = Func.ReadINI("Setting", "IP", "DUT", "192.168.1.1");
            string IQXel_IP = Func.ReadINI("Setting", "IP", "IQXel", "192.168.100.254");
            string item = $"RF_{testItem.ToString()}";

            DisplayMsg(LogType.Log, $"=============== Run IQfact ({testItem.ToString()}) ===============");

            try
            {
                _RFTool = Func.ReadINI("Setting", "RF", $"ToolPath_{testItem.ToString()}", string.Empty);
                _RFLog = Path.GetDirectoryName(_RFTool) + "\\Log\\logOutput.txt";

                if (isGolden)
                {
                    _RFTestPlan = Func.ReadINI("Setting", "RF", $"Golden_TestPlan_{testItem.ToString()}", string.Empty);
                }
                else
                {
                    _RFTestPlan = Func.ReadINI("Setting", "RF", $"TestPlan_{testItem.ToString()}", string.Empty);
                }

                //_RFTimeOutSec = Convert.ToInt32(Func.ReadINI("Setting", "RF", $"TimeOutSec_{testItem.ToString()}", "180"));
                // =================== debug CheckWiFiLog =================================
                _RFTimeOutSec = Convert.ToInt32(320);
                // =================== debug CheckWiFiLog =================================
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

                // ping DUT
                if (!telnet.Ping(DUT_IP, 10 * 1000))
                {
                    DisplayMsg(LogType.Error, $"Ping DUT({DUT_IP}) failed!!");
                    AddData("PingDUT", 1);
                    return false;
                }

                // ping IQXel
                if (!telnet.Ping(IQXel_IP, 10 * 1000))
                {
                    DisplayMsg(LogType.Error, $"Ping IQXel({IQXel_IP}) failed!!");
                    AddData("PingIQXel", 1);
                    return false;
                }

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
            //string path = @"C:\LitePoint\IQfact_plus\IQfact+_QCA_9224_5.0.0.13_Lock\bin\";
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

                //// chk IQFACT run or not
                //if (!CheckToolExist(Path.GetDirectoryName(path) + "IQfactRun_Console.exe"))
                //{
                //    if (!OpenTestTool(Path.GetDirectoryName(path), "IQfactRun_Console.exe", "", 3000))
                //    {
                //        status_ATS.Write_Warning("Open IQfactRun_Console.exe error", StatusUI2.StatusUI.StatusProc.Warning);
                //        DisplayMsg(LogType.Error, @"IQfactRun_Console did not open!!!!!!");
                //    }
                //    else
                //    {
                //        DisplayMsg(LogType.Log, @"IQfactRun_Console.exe check OK");
                //    }
                //}
                if (ChkResult(testItem))
                {
                    Analyze_RFLog(testItem);
                }

                //process.WaitForExit(timeOutMs);
                DisplayMsg(LogType.Log, "Complete");
                process.Dispose();
                process.Close();
                process = null;
                if (!OpenTestTool(Path.GetDirectoryName(@"C:\LitePoint\IQfact_plus\IQfact+_QCA_9224_5.0.0.13_Lock\bin\"), "IQfactRun_Console.exe", "", 3000))
                {
                    status_ATS.Write_Warning("Open IQfactRun_Console.exe error", StatusUI2.StatusUI.StatusProc.Warning);
                    DisplayMsg(LogType.Error, @"IQfactRun_Console check twice_____NG!!!!!!");
                }
                else
                {
                    DisplayMsg(LogType.Log, @"IQfactRun_Console.exe check twice_____OK");
                }
                DisplayMsg(LogType.Log, @"Delay after Dispose");
                Thread.Sleep(10 * 1000);
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
            string path = @"C:\LitePoint\IQfact_plus\IQfact+_QCA_9224_5.0.0.13_Lock\bin\";
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

                // chk IQFACT run or not
                if (!CheckToolExist(Path.GetDirectoryName(path) + "IQfactRun_Console.exe"))
                {
                    if (!OpenTestTool(Path.GetDirectoryName(path), "IQfactRun_Console.exe", "", 3000))
                    {
                        status_ATS.Write_Warning("Open IQfactRun_Console.exe error", StatusUI2.StatusUI.StatusProc.Warning);
                        DisplayMsg(LogType.Error, @"IQfactRun_Console did not open!!!!!!");
                    }
                    else
                    {
                        DisplayMsg(LogType.Log, @"IQfactRun_Console.exe check OK");
                    }
                }
                while (true)
                {
                    ts = new TimeSpan(DateTime.Now.Ticks - dt.Ticks);
                    if (ts.TotalMilliseconds > timeOutMs)
                    {
                        DisplayMsg(LogType.Error, "Check log timeout");
                        MessageBox.Show("debug for CheckWiFiLog"); //lucky request 12/21
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
                                /*                                if (LSL.Contains("-999") || USL.Contains("999"))
                                                                {
                                                                    AddData(item + "_FREQ_ERROR_AVG", Convert.ToDouble(value));
                                                                }
                                                                else AddData(item + "_FREQ_ERROR_AVG", "", LSL, USL, value, GetErrorCode(item + "_FREQ_ERROR_AVG"));*/
                                AddData(item + "_FREQ_ERROR_AVG", "", LSL, USL, value, GetErrorCode(item + "_FREQ_ERROR_AVG"));
                            }
                            else if (line.Contains("VIOLATION_PERCENT"))
                            {
                                // VIOLATION_PERCENT_VSA_1-> Tx Mask 
                                string LSL = GetLSL(line);
                                string USL = GetUSL(line);
                                string value = GetMiddleString(line, ":", "%").Trim();

                                //status_ATS.AddData(item + "_VIOLATION_PERCENT", "", LSL, USL, value, GetErrorCode(item + "_VIOLATION_PERCENT"));
                                /*                                if (LSL.Contains("-999") || USL.Contains("999"))
                                                                {
                                                                    AddData(item + "_VIOLATION_PERCENT", Convert.ToDouble(value));
                                                                }
                                                                else AddData(item + "_VIOLATION_PERCENT", "", LSL, USL, value, GetErrorCode(item + "_VIOLATION_PERCENT"));*/
                                AddData(item + "_VIOLATION_PERCENT", "", LSL, USL, value, GetErrorCode(item + "_VIOLATION_PERCENT"));
                            }
                            else if (line.Contains("EVM_DB_AVG_S1"))
                            {
                                // EVM_DB_AVG_S1	-> TX EVM
                                string LSL = GetLSL(line);
                                string USL = GetUSL(line);
                                string value = GetMiddleString(line, ":", "dB").Trim();

                                //status_ATS.AddData(item + "_EVM_DB_AVG_S1", "", LSL, USL, value, GetErrorCode(item + "_EVM_DB_AVG_S1"));
                                /*                                if (LSL.Contains("-999") || USL.Contains("999"))
                                                                {
                                                                    AddData(item + "_EVM_DB_AVG_S1", Convert.ToDouble(value));
                                                                }
                                                                else AddData(item + "_EVM_DB_AVG_S1", "", LSL, USL, value, GetErrorCode(item + "_EVM_DB_AVG_S1"));*/
                                AddData(item + "_EVM_DB_AVG_S1", "", LSL, USL, value, GetErrorCode(item + "_EVM_DB_AVG_S1"));

                            }
                            /*else if (line.Contains("POWER_DBM_RMS_AVG_S1"))
                            {
                                // POWER_DBM_RMS_AVG_S1 -> TX Power
                                string LSL = GetLSL(line);
                                string USL = GetUSL(line);
                                string value = GetMiddleString(line, ":", "dBm").Trim();

                                //status_ATS.AddData(item + "_POWER_DBM_RMS_AVG_S1", "", LSL, USL, value, GetErrorCode(item + "_POWER_DBM_RMS_AVG_S1"));
                                if (LSL.Contains("-999") || USL.Contains("999"))
                                {
                                    AddData(item + "_POWER_DBM_RMS_AVG_S1", Convert.ToDouble(value));
                                }
                                else AddData(item + "_POWER_DBM_RMS_AVG_S1", "", LSL, USL, value, GetErrorCode(item + "_POWER_DBM_RMS_AVG_S1"));

                            }*/

                            else if (line.Contains("POWER_RMS_AVG_VSA1"))
                            {
                                // POWER_DBM_RMS_AVG_S1 -> TX Power
                                string LSL = GetLSL(line);
                                string USL = GetUSL(line);
                                string value = GetMiddleString(line, ":", "dBm").Trim();

                                //status_ATS.AddData(item + "_POWER_DBM_RMS_AVG_S1", "", LSL, USL, value, GetErrorCode(item + "_POWER_DBM_RMS_AVG_S1"));
                                /*                                if (LSL.Contains("-999") || USL.Contains("999"))
                                                                {
                                                                    AddData(item + "_POWER_DBM_RMS_AVG_S1", Convert.ToDouble(value));
                                                                }
                                                                else AddData(item + "_POWER_DBM_RMS_AVG_S1", "", LSL, USL, value, GetErrorCode(item + "_POWER_DBM_RMS_AVG_S1"));*/
                                AddData(item + "_POWER_DBM_RMS_AVG_S1", "", LSL, USL, value, GetErrorCode(item + "_POWER_DBM_RMS_AVG_S1"));

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
                                /*                                if (LSL.Contains("-999") || USL.Contains("999"))
                                                                {
                                                                    AddData(item + "_PER", Convert.ToDouble(value));
                                                                }
                                                                else AddData(item + "_PER", "", LSL, USL, value, GetErrorCode(item + "_PER"));*/
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

                                /*                                if (LSL.Contains("-999") || USL.Contains("999"))
                                                                {
                                                                    AddData(item + "_DELTA_F0_Fn_MAX", Convert.ToDouble(value));
                                                                }
                                                                else AddData(item + "_DELTA_F0_Fn_MAX", "", LSL, USL, value, GetErrorCode(item + "_DELTA_F0_Fn_MAX"));*/
                                AddData(item + "_DELTA_F0_Fn_MAX", "", LSL, USL, value, GetErrorCode(item + "_DELTA_F0_Fn_MAX"));
                            }
                            else if (line.Contains("DELTA_F1_AVERAGE"))
                            {
                                string LSL = GetLSL(line);
                                string USL = GetUSL(line);
                                string value = GetMiddleString(line, ":", "kHz").Trim();

                                //status_ATS.AddData(item + "_DELTA_F1_AVERAGE", "", LSL, USL, value, GetErrorCode(item + "_DELTA_F1_AVERAGE"));
                                /*                                if (LSL.Contains("-999") || USL.Contains("999"))
                                                                {
                                                                    AddData(item + "_DELTA_F1_AVERAGE", Convert.ToDouble(value));
                                                                }
                                                                else AddData(item + "_DELTA_F1_AVERAGE", "", LSL, USL, value, GetErrorCode(item + "_DELTA_F1_AVERAGE"));*/
                                AddData(item + "_DELTA_F1_AVERAGE", "", LSL, USL, value, GetErrorCode(item + "_DELTA_F1_AVERAGE"));
                            }
                            else if (line.Contains("DELTA_F1_F0"))
                            {
                                string LSL = GetLSL(line);
                                string USL = GetUSL(line);
                                string value = GetMiddleString(line, ":", "kHz").Trim();

                                //status_ATS.AddData(item + "_DELTA_F1_F0", "", LSL, USL, value, GetErrorCode(item + "_DELTA_F1_F0"));
                                /*                                if (LSL.Contains("-999") || USL.Contains("999"))
                                                                {
                                                                    AddData(item + "_DELTA_F1_F0", Convert.ToDouble(value));
                                                                }
                                                                else AddData(item + "_DELTA_F1_F0", "", LSL, USL, value, GetErrorCode(item + "_DELTA_F1_F0"));*/
                                AddData(item + "_DELTA_F1_F0", "", LSL, USL, value, GetErrorCode(item + "_DELTA_F1_F0"));
                            }
                            else if (line.Contains("DELTA_F2_F1_AV_RATIO"))
                            {
                                string LSL = GetLSL(line);
                                string USL = GetUSL(line);
                                string value = GetMiddleString(line, ":", "(").Trim();

                                //status_ATS.AddData(item + "_DELTA_F2_F1_AV_RATIO", "", LSL, USL, value, GetErrorCode(item + "_DELTA_F2_F1_AV_RATIO"));
                                /*                                if (LSL.Contains("-999") || USL.Contains("999"))
                                                                {
                                                                    AddData(item + "_DELTA_F2_F1_AV_RATIO", Convert.ToDouble(value));
                                                                }
                                                                else AddData(item + "_DELTA_F2_F1_AV_RATIO", "", LSL, USL, value, GetErrorCode(item + "_DELTA_F2_F1_AV_RATIO"));*/
                                AddData(item + "_DELTA_F2_F1_AV_RATIO", "", LSL, USL, value, GetErrorCode(item + "_DELTA_F2_F1_AV_RATIO"));
                            }
                            else if (line.Contains("DELTA_Fn_Fn5_MAX"))
                            {
                                string LSL = GetLSL(line);
                                string USL = GetUSL(line);
                                string value = GetMiddleString(line, ":", "kHz").Trim();

                                //status_ATS.AddData(item + "_DELTA_Fn_Fn5_MAX", "", LSL, USL, value, GetErrorCode(item + "_DELTA_Fn_Fn5_MAX"));
                                /*                                if (LSL.Contains("-999") || USL.Contains("999"))
                                                                {
                                                                    AddData(item + "_DELTA_Fn_Fn5_MAX", Convert.ToDouble(value));
                                                                }
                                                                else AddData(item + "_DELTA_Fn_Fn5_MAX", "", LSL, USL, value, GetErrorCode(item + "_DELTA_Fn_Fn5_MAX"));*/
                                AddData(item + "_DELTA_Fn_Fn5_MAX", "", LSL, USL, value, GetErrorCode(item + "_DELTA_Fn_Fn5_MAX"));
                            }
                            else if (line.Contains("INITIAL_FREQ_OFFSET"))
                            {
                                string LSL = GetLSL(line);
                                string USL = GetUSL(line);
                                string value = GetMiddleString(line, ":", "kHz").Trim();

                                //status_ATS.AddData(item + "_INITIAL_FREQ_OFFSET", "", LSL, USL, value, GetErrorCode(item + "_INITIAL_FREQ_OFFSET"));
                                /*                                if (LSL.Contains("-999") || USL.Contains("999"))
                                                                {
                                                                    AddData(item + "_INITIAL_FREQ_OFFSET", Convert.ToDouble(value));
                                                                }
                                                                else AddData(item + "_INITIAL_FREQ_OFFSET", "", LSL, USL, value, GetErrorCode(item + "_INITIAL_FREQ_OFFSET"));*/
                                AddData(item + "_INITIAL_FREQ_OFFSET", "", LSL, USL, value, GetErrorCode(item + "_INITIAL_FREQ_OFFSET"));
                            }
                            else if (line.Contains("POWER_AVERAGE_DBM"))
                            {
                                string LSL = GetLSL(line);
                                string USL = GetUSL(line);
                                string value = GetMiddleString(line, ":", "dBm").Trim();

                                //status_ATS.AddData(item + "_POWER_AVERAGE_DBM", "", LSL, USL, value, GetErrorCode(item + "_POWER_AVERAGE_DBM"));
                                /*                                if (LSL.Contains("-999") || USL.Contains("999"))
                                                                {
                                                                    AddData(item + "_POWER_AVERAGE_DBM", Convert.ToDouble(value));
                                                                }
                                                                else AddData(item + "_POWER_AVERAGE_DBM", "", LSL, USL, value, GetErrorCode(item + "_POWER_AVERAGE_DBM"));*/
                                AddData(item + "_POWER_AVERAGE_DBM", "", LSL, USL, value, GetErrorCode(item + "_POWER_AVERAGE_DBM"));
                            }
                            //Rena_20230711, add ACP_MAX_POWER_DBM_OFFSET item
                            else if (line.Contains("ACP_MAX_POWER_DBM_OFFSET"))
                            {
                                string LSL = GetLSL(line);
                                string USL = GetUSL(line);
                                string value = GetMiddleString(line, ":", "dBm").Trim();
                                string item_name = line.Split(':')[0].Trim();
                                /*DisplayMsg(LogType.Log,"Item name is:" + item_name);*/


                                //上下限都沒設定時就忽略
                                if (LSL != "-999" || USL != "999")
                                {
                                    item_name = item_name.Replace("-", "_negative_");
                                    /*DisplayMsg(LogType.Log, "Item name log is:" + item_name);*/
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
                                /*                                if (LSL.Contains("-999") || USL.Contains("999"))
                                                                {
                                                                    AddData(item + "_PER", Convert.ToDouble(value));
                                                                }
                                                                else AddData(item + "_PER", "", LSL, USL, value, GetErrorCode(item + "_PER"));*/
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

                                /*                                if (LSL.Contains("-999") || USL.Contains("999"))
                                                                {
                                                                    AddData(item + "_POWER_AVERAGE", Convert.ToDouble(value));
                                                                }
                                                                else AddData(item + "_POWER_AVERAGE", "", LSL, USL, value, GetErrorCode(item + "_POWER_AVERAGE"));*/
                                AddData(item + "_POWER_AVERAGE", "", LSL, USL, value, GetErrorCode(item + "_POWER_AVERAGE"));
                            }
                            else if (line.Contains("EVM_ALL_PERCENT"))
                            {
                                string LSL = GetLSL(line);
                                string USL = GetUSL(line);
                                string value = GetMiddleString(line, ":", "%").Trim();

                                /*                                if (LSL.Contains("-999") || USL.Contains("999"))
                                                                {
                                                                    AddData(item + "_EVM_ALL_PERCENT", Convert.ToDouble(value));
                                                                }
                                                                else AddData(item + "_EVM_ALL_PERCENT", "", LSL, USL, value, GetErrorCode(item + "_EVM_ALL_PERCENT"));*/
                                AddData(item + "_EVM_ALL_PERCENT", "", LSL, USL, value, GetErrorCode(item + "_EVM_ALL_PERCENT"));
                            }
                            else if (line.Contains("FREQ_ERROR") && !line.Contains("FREQ_ERROR_AVG_PPM"))
                            {
                                string LSL = GetLSL(line);
                                string USL = GetUSL(line);
                                string value = GetMiddleString(line, ":", "KHz").Trim();

                                /*                                if (LSL.Contains("-999") || USL.Contains("999"))
                                                                {
                                                                    AddData(item + "_FREQ_ERROR", Convert.ToDouble(value));
                                                                }
                                                                else AddData(item + "_FREQ_ERROR", "", LSL, USL, value, GetErrorCode(item + "_FREQ_ERROR"));*/
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
                                /*                                if (LSL.Contains("-999") || USL.Contains("999"))
                                                                {
                                                                    AddData(item + "_PER", Convert.ToDouble(value));
                                                                }
                                                                else AddData(item + "_PER", "", LSL, USL, value, GetErrorCode(item + "_PER"));*/
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
        private void EnterWiFiTestMode()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            string res = "";
            string keyword = "root@OpenWrt:~# \r\n"; //避免誤判到指令第一行的"root@OpenWrt:~#"
            string item = "EnterWiFiTestMode";
            string PC_IP = Func.ReadINI("Setting", "IP", "PC", "192.168.1.66");
            bool IsCmdOK = false;
            try
            {
                DisplayMsg(LogType.Log, "Enter WiFi test mode");
                DisplayMsg(LogType.Log, $"PC_IP : {PC_IP}");
                for (int i = 0; i < 10; i++)
                {
                    SendAndChk(PortType.SSH, "rmmod ath12k", keyword, out res, 0, 30 * 1000);
                    if (res.Contains("module is not loaded"))
                    {
                        IsCmdOK = true;
                        break;
                    }
                    SendCommand(PortType.SSH, sCtrlC, 500);
                    ChkResponse(PortType.SSH, ITEM.NONE, keyword, out res, 5000);
                    Thread.Sleep(1000);
                }
                SendAndChk(PortType.SSH, "insmod ath12k.ko dyndbg=+p ftm_mode=1", keyword, 0, 30 * 1000);
                //diag_socket_app connect前要先打開QUTS並設定好
                SendAndChk(PortType.SSH, $"diag_socket_app -a {PC_IP} &", "logging", out res, 3000, 30 * 1000);
                if (!res.Contains("Successful connect to address:"))
                {
                    DisplayMsg(LogType.Log, $"Connect to {PC_IP} fail");
                    MessageBox.Show("DIAG_SOCKET >>>> NG");
                    AddData(item, 1);
                    return;
                }
                SendAndChk(PortType.SSH, "cp /etc/wifi/ftm.conf /tmp/ftm.conf", keyword, 0, 3000);
                SendAndChk(PortType.SSH, "/usr/sbin/ftm -n -c /tmp/ftm.conf &", keyword, 1000, 3000);
                //============================================================================================
                //SendAndChk(PortType.SSH, "wifi down", keyword, 0, 30 * 1000);
                //SendAndChk(PortType.SSH, "rmmod ecm_wifi_plugin", keyword, 0, 3000);
                //SendAndChk(PortType.SSH, "rmmod monitor", keyword, 0, 10 * 1000);
                //SendAndChk(PortType.SSH, "rmmod wifi_3_0", keyword, 0, 10 * 1000);
                //SendAndChk(PortType.SSH, "rmmod qca_ol", keyword, 0, 3000);
                //SendAndChk(PortType.SSH, "insmod qca_ol testmode=1", keyword, 0, 5000);
                //SendAndChk(PortType.SSH, "insmod wifi_3_0", keyword, 0, 20 * 1000);

                ////diag_socket_app connect前要先打開QUTS並設定好
                //SendAndChk(PortType.SSH, $"diag_socket_app -a {PC_IP} &", "logging switched", out res, 0, 30 * 1000);
                //if (!res.Contains("Successful connect to address:"))
                //{
                //    DisplayMsg(LogType.Log, $"Connect to {PC_IP} fail");
                //    AddData(item, 1);
                //    return;
                //}

                //SendAndChk(PortType.SSH, "/etc/init.d/ftm start", keyword, 0, 3000);
                //SendAndChk(PortType.SSH, "/usr/sbin/ftm -n -c /tmp/ftm.conf &", keyword, 0, 3000);
                //============================================================================================

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
        private void RebootDUT()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            string item = "RebootDUT";

            try
            {
                DisplayMsg(LogType.Log, "Reboot DUT...");

                if (Func.ReadINI("Setting", "IO_Board_Control", "IO_Control_1", "0") == "1")
                {
                    string txPin = Func.ReadINI("Setting", "IO_Board_Control", "Pin0", "0");
                    string rev_message = "";
                    status_ATS.AddLog("IO_Board_Y" + txPin + " Off...");
                    IO_Board_Control1.ConTrolIOPort_write(Int32.Parse(txPin), "2", ref rev_message);
                    DisplayMsg(LogType.Log, rev_message);

                    Thread.Sleep(3000);

                    status_ATS.AddLog("IO_Board_Y" + txPin + " On...");
                    IO_Board_Control1.ConTrolIOPort_write(Int32.Parse(txPin), "1", ref rev_message);
                    DisplayMsg(LogType.Log, rev_message);
                }
                else if (Func.ReadINI("Setting", "Port", "RelayBoard", "Disable").ToUpper() == "ENABLE")
                {
                    SwitchRelay(CTRL.ON);
                    Thread.Sleep(6000);
                    SwitchRelay(CTRL.OFF);
                }
                else
                {
                    frmOK.Label = "Vui lòng khởi động lại thiết bị (DUT)";
                    frmOK.ShowDialog();
                }

                ChkBootUp(PortType.SSH);
                DisplayMsg(LogType.Log, @"Delay after PING_OK");
                Thread.Sleep(30 * 1000);
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

                /// ========================== for stress test ====================================
                //infor.WiFiMAC_2G = "AC:91:9B:57:2A:10";
                //infor.WiFiMAC_5G = "AC:91:9B:57:2A:0F";
                //infor.WiFiMAC_6G = "AC:91:9B:57:2A:0E";
                //---------------------------------------------
                //infor.WiFiMAC_2G = "E8.C7.CF.AF.42.40";
                //infor.WiFiMAC_5G = "E8.C7.CF.AF.42.41";
                //infor.WiFiMAC_6G = "E8.C7.CF.AF.42.42";
                // ===============================================================================
                // check if MAC exist
                if (string.IsNullOrEmpty(infor.WiFiMAC_2G) || string.IsNullOrEmpty(infor.WiFiMAC_2G) || string.IsNullOrEmpty(infor.WiFiMAC_2G))
                {
                    DisplayMsg(LogType.Error, "infor.WiFiMAC doesn't exist");
                    AddData(item, 1);
                    return;
                }
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
