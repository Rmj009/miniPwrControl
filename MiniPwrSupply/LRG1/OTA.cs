﻿using System;
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
using System.Windows.Documents;
using static WNC.UI.FrmRetry;
using System.Text;
using System.Windows.Forms;
using WNC.API;
using System.IO;
using System.Threading;
using NationalInstruments.VisaNS;
using System.IO.Ports;
using System.Text.RegularExpressions;
using EasyLibrary;
using System.Windows.Documents;
using static WNC.UI.FrmRetry;
using System.Net.Security;
using System.Runtime.CompilerServices;

namespace MiniPwrSupply.LRG1
{
    public partial class frmMain
    {
        public class DeviceInfor
        {
            public string SerialNumber = "";
            public string FWver = "";

            public void ResetParam()
            {
                SerialNumber = "";
                FWver = "";
            }
        }
        private enum Antenna
        {
            Antenna_0 = 0x00,
            Antenna_1 = 0x01,
        }

        private void OTA()
        {
            try
            {
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
                    frmOK.Label = "Xác nhận đã kết nối cáp 'mạng',\r\nVui lòng bật nguồn cho DUT và Golden";
                    frmOK.ShowDialog();
                }
                DisplayMsg(LogType.Log, "Power on!!!");
                DisplayMsg(LogType.Log, $"Current PSN Input: {status_ATS.txtPSN.Text}");
                string SN = string.Empty;
                SN = status_ATS.txtPSN.Text;
                if (SN.Length == 18)
                {
                    SetTextBox(status_ATS.txtPSN, SN);
                    //SetTextBox(status_ATS.txtSP, infor.BaseMAC);
                    status_ATS.SFCS_Data.PSN = SN;
                    status_ATS.SFCS_Data.First_Line = SN;
                }
                else
                {
                    warning = "Get SN format fail";
                    return;
                }

                if (status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode)
                {
                    DisplayMsg(LogType.Log, $"Current PSN Input: {SN}");
                    if (ChkStation(SN)) { DisplayMsg(LogType.Log, "Check Station FAIL!"); return; }
                    DisplayMsg(LogType.Log, "Check Station OK!");
                }

                ChkBootUp(PortType.SSH);
                if (!CheckGoNoGo()) { return; }

                #region Ethernet test
                if (isLoop == 0)
                {
                    this.EthernetTest(1);
                    this.EthernetTest(3);
                }
                #endregion Ethernet test

                /*#region Check dect data

                string res = string.Empty;
                if (!SendAndChk(PortType.SSH, "verify_boarddata.sh", "root@OpenWrt:~# \r\n", out res, 0, 5000))
                {
                    return;
                }
                string pattern = @"dect_rf_calibration_rxtun=(\d+)";
                Match match = Regex.Match(res, pattern);
                if (match.Success)
                {
                    string dect_rf_calibration_rxtun = match.Groups[1].Value;

                    DisplayMsg(LogType.Log, "Find out RXturn -> Up data to 15 row. For 5pcs only");
                    DisplayMsg(LogType.Log, "dect_rf_calibration_rxtun: " + dect_rf_calibration_rxtun);
                    status_ATS.AddDataRaw("LGR1_RXTURN", dect_rf_calibration_rxtun, dect_rf_calibration_rxtun, "000000");
                    AddData("rxtun", 0);
                }
                else
                {
                    DisplayMsg(LogType.Log, "Fin dect_rf_calibration_rxtun fail");
                    AddData("rxtun", 1);
                    return;
                }
                #endregion check dect data*/

                if (Func.ReadINI("Setting", "OTA", "SkipNFC", "0") == "0")
                {
                    if (!CheckGoNoGo()) { return; }
                    OTA_NFC();
                }

                if (Func.ReadINI("Setting", "OTA", "SkipThread", "0") == "0")
                {
                    if (!CheckGoNoGo()) { return; }
                    OTA_Thread();
                }

                if (Func.ReadINI("Setting", "OTA", "SkipWiFi", "0") == "0")
                {
                    if (!CheckGoNoGo()) { return; }
                    OTA_WiFi();
                }

                if (Func.ReadINI("Setting", "OTA", "SkipDECT", "0") == "0")
                {
                    if (!CheckGoNoGo()) { return; }
                    OTA_DECT();
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
        private void OTA_DECT()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            string item = "DECT";

            try
            {
                DECT_PowerSetting();

                DECT_PowerTest(Antenna.Antenna_0, 1888704000);   // 1928448000 
                //DECT_PowerSetting();        // enter menu again
                DECT_PowerTest(Antenna.Antenna_1, 1888704000);   // 1928448000

                CheckEEPromValue();
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                AddData(item, 1);
            }

        }
        private void CheckEEPromValue()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            DisplayMsg(LogType.Log, "=============== Check EEProm Value ===============");

            string keyword = @"root@OpenWrt";
            string res = string.Empty;
            string item = "CheckEEPromValue";
            int delayMs = 0;
            int timeOutMs = 10 * 1000;

            try
            {
                //s->1->6(enter)->21(enter)->3(enter)
                SendWithoutEnterAndChk(item, PortType.SSH, "q", "q => Quit", delayMs, timeOutMs);
                SendWithoutEnterAndChk(item, PortType.SSH, "s", "q => Return to Interface Menu", delayMs, timeOutMs);
                SendWithoutEnterAndChk(item, PortType.SSH, "1", "q => Return", delayMs, timeOutMs);

                DisplayMsg(LogType.Log, $"Write '6' to ssh");
                SSH_stream.WriteLine("6");
                Thread.Sleep(200);
                DisplayMsg(LogType.Log, $"Write '21' to ssh");
                SSH_stream.WriteLine("21");
                Thread.Sleep(200);
                SendAndChk(PortType.SSH, "3", "Press Any Key", out res, delayMs, timeOutMs);
                if (res.Contains("Data:     0F     EB     09"))
                {
                    DisplayMsg(LogType.Log, "Check EEProm Value '0F EB 09' pass");
                    AddData(item, 0);
                }
                else
                {
                    DisplayMsg(LogType.Log, "Check EEProm Value '0F EB 09' fail");
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
                SendAndChk(PortType.SSH, "qqqqq", keyword, out res, 0, 3600);
                SendCommand(PortType.SSH, "\r\n", 2000);

                //exit calibration mode
                //for (int i = 0; i < 5; i++)
                //{
                //if (SendAndChk(PortType.SSH, "qqqq", keyword, out res, 0, 2000))
                //    break;
                //}
            }
        }
        private void DECT_PowerSetting()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            DisplayMsg(LogType.Log, "=============== DECT Power Setting ===============");

            string item = "DECT";

            try
            {
                string res = string.Empty;
                //bool result = false;
                this.enterMode();
                //for (int i = 0; i < 3; i++)
                //{
                //    SendCommand(PortType.SSH, "cmbs_tcx -comname ttyMSM2 -baud 460800", 0);
                //    if (result = ChkResponse(PortType.SSH, ITEM.NONE, "Choose", out res, 3000))
                //        break;
                //    DisplayMsg(LogType.Log, "Delay 2s...");
                //    Thread.Sleep(2000);
                //}

                //if (!result)
                //{
                //    DisplayMsg(LogType.Log, "Enter DECT MENU fail");
                //    AddData(item, 1);
                //    return;
                //}

                DisplayMsg(LogType.Log, "Write x to ssh");
                SSH_stream.Write("x\r");
                ChkResponse(PortType.SSH, ITEM.NONE, "q) Quit", out res, 10 * 1000);
                if (!res.Contains("Start ATE tests"))
                {
                    DisplayMsg(LogType.Log, "Start ATE tests fail");
                    AddData(item, 1);
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData(item, 1);
            }
        }
        private void DECT_PowerTest(Antenna antenna, double freqMhz)
        {
            if (!CheckGoNoGo())
            {
                return;
            }
            string item = "DECT Power Test";

            DisplayMsg(LogType.Log, $"=============== {item} " + antenna.ToString().Replace("_", " ") + " ===============");
            string res = string.Empty;
            string errorItem = "DECT_" + antenna.ToString();
            //int delayMs = 0;
            DateTime dt;
            TimeSpan ts;
            bool isRxtunOK = false;
            //double pwr = -999;
            int retryTimes = 1;
            int delayMs = 200; //Rena_20230713 debug
            int timeOutMs = 10 * 1000;
            //double pwrThreshold = Convert.ToInt32(Func.ReadINI("Setting", "DECT_SignalAnalyzer", "PwrThreshold", "-35"));

            try
            {
            RetryPwr:
                if (retryTimes > 2)
                {
                    this.DECT_PowerSetting();
                }
                SendWithoutEnterAndChk(errorItem, PortType.SSH, "s", "FF for default", delayMs, timeOutMs);
                SendWithoutEnterAndChk(errorItem, PortType.SSH, "ff", "2 - long slot", delayMs, timeOutMs);
                SendWithoutEnterAndChk(errorItem, PortType.SSH, "0", "3 - continuous TX", delayMs, timeOutMs);
                SendWithoutEnterAndChk(errorItem, PortType.SSH, "2", ":", delayMs, timeOutMs);
                SendWithoutEnterAndChk(errorItem, PortType.SSH, "0", ":", delayMs, timeOutMs);
                SendWithoutEnterAndChk(errorItem, PortType.SSH, "05", ":", delayMs, timeOutMs);
                SendWithoutEnterAndChk(errorItem, PortType.SSH, ((int)antenna).ToString(), "Enter Slot (two digits, 0..11):", delayMs, timeOutMs);
                SendWithoutEnterAndChk(errorItem, PortType.SSH, "02", ":", delayMs, timeOutMs);
                SendWithoutEnterAndChk(errorItem, PortType.SSH, "4", "Enter Power Level (0,1 or 2):", delayMs, timeOutMs);
                SendWithoutEnterAndChk(errorItem, PortType.SSH, "0", "Enter Normal Preamble(y/n):", delayMs, timeOutMs);
                SendWithoutEnterAndChk(errorItem, PortType.SSH, "y", "Press any key !", delayMs, timeOutMs);
                if (!CheckGoNoGo())
                { return; }
                //int c = 1;
                //retry:
                dt = DateTime.Now;
                while (true)
                {
                    ts = new TimeSpan(DateTime.Now.Ticks - dt.Ticks);
                    if (ts.TotalMilliseconds > 180 * 1000)
                    {
                        DisplayMsg(LogType.Error, "DECT_pwrTest TIMEOUT");
                        AddData(item, 1);
                        return;
                    }

                    DisplayMsg(LogType.Log, "Delay 300 (ms)..");
                    System.Threading.Thread.Sleep(300);
                    if (!FetchPwr(errorItem, freqMhz, antenna))
                    {
                        retryTimes++;
                        DisplayMsg(LogType.Warning, "Under power threshold ");
                        if (retryTimes > 3)
                        {
                            warning = $"Power under threahold failed retry 3 time";
                            RemoveFailedItem();
                            AddData(item, 1);
                            isRxtunOK = false;
                            return;
                        }
                        else
                        {
                            DisplayMsg(LogType.Log, $"TxPower Too low, try {retryTimes} time");
                            this.ResetDect();
                            goto RetryPwr;
                        }
                        //continue;
                        //if (c < 3)
                        //{
                        //    warning = "";
                        //    c++;
                        //    goto retry;
                        //}
                    }
                    else
                    {
                        break;
                    }


                }
                SendWithoutEnterAndChk(errorItem, PortType.SSH, "\r", "q) Quit", delayMs, timeOutMs);
                SendWithoutEnterAndChk(errorItem, PortType.SSH, "u", "Press any key !", delayMs, timeOutMs);
                SendWithoutEnterAndChk(errorItem, PortType.SSH, "\r", "q) Quit", delayMs, timeOutMs);
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData(errorItem, 1);
            }
            //finally
            //{
            //    SendAndChk(PortType.SSH, "qqqqq\r\n", "root@", out res, 0, 3000);
            //    SendAndChk(PortType.SSH, "\n", "root@", out res, 0, 600);
            //    DisplayMsg(LogType.Log, "exit menu");
            //}
        }

        private bool FetchPwr(string errorItem, double freqHz, Antenna antenna, bool avg = false)
        {
            double pwr = -999;
            double threshold = Convert.ToDouble(Func.ReadINI("Setting", "DECT_SignalAnalyzer", "Pwr_Threshold", "-999"));
            try
            {
                string useSpectrumSv = Func.ReadINI("Setting", "SpectrumServer", "Use", "0");
                string serverIP = Func.ReadINI("Setting", "SpectrumServer", "ServerIP", "0");
                string serverPort = Func.ReadINI("Setting", "SpectrumServer", "ServerPort", "0");

                int delayMs = Convert.ToInt32(Func.ReadINI("Setting", "DECT_SignalAnalyzer", "FetchPwrDelayMs", "0"));
                double spanHz = Convert.ToDouble(Func.ReadINI("Setting", "DECT_SignalAnalyzer", "Pwr_Span_Hz", "0"));
                double rbwHz = Convert.ToDouble(Func.ReadINI("Setting", "DECT_SignalAnalyzer", "Pwr_RBW_Hz", "0"));
                double vbwHz = Convert.ToDouble(Func.ReadINI("Setting", "DECT_SignalAnalyzer", "Pwr_VBW_Hz", "0"));
                double rlevDbm = Convert.ToDouble(Func.ReadINI("Setting", "DECT_SignalAnalyzer", "Pwr_RLEV_dBm", "0"));
                double loss = Convert.ToDouble(Func.ReadINI("Setting", "DECT_SignalAnalyzer", "Loss_" + antenna, "0"));
                double att = Convert.ToDouble(Func.ReadINI("Setting", "DECT_SignalAnalyzer", "Freq_Att", "0"));
                double sweeptime = Convert.ToDouble(Func.ReadINI("Setting", "DECT_SignalAnalyzer", "SweepTimeMs", "0"));
                double trigerlevel = Convert.ToDouble(Func.ReadINI("Setting", "DECT_SignalAnalyzer", "TrigerLevelDbm", "-10"));

                DateTime dt;
                TimeSpan ts;
                int timeOutMs = 10 * 1000;
                int avgCount = 5;
                double value = 0;

                switch (analyzer)
                {
                    case Analyzer.MS2830A:
                        #region Analyzer.MS2830A
                        #region use spectrum server
                        if (useSpectrumSv == "1")
                        {
                            DisplayMsg(LogType.Log, "============ Start connect to spectrum server ===========");
                            SpectrumClient spClient = new SpectrumClient(serverIP, Convert.ToInt32(serverPort));
                            try
                            {
                                bool setMaxHold = true;
                                bool setLinear = false;
                                if (!spClient.ConnectToServer())
                                {
                                    DisplayMsg(LogType.Log, "Kiểm tra kết nối đến Spectrum server");
                                    MessageBox.Show("Kiểm tra kết nối đến Spectrum server");
                                    warning = "Connect to spectrum fail";
                                    return false;
                                }

                                if (!spClient.SendToServer($"{status_ATS.txtPSN.Text}_start"))
                                {
                                    warning = "SendToServer fail";
                                    return false;
                                }
                                DisplayMsg(LogType.Log, "Waiting spectrum server return 'ok' in 50s");
                                if (!spClient.WaitingOk(50))
                                {
                                    warning = "Waiting Spectrum reponse fail";
                                    return false;
                                }
                                if (!spClient.SendToServer($"{status_ATS.txtPSN.Text}_span:{spanHz}_rbw:{rbwHz}_vbw:{vbwHz}_" +
                                    $"rlev:{rlevDbm}_freq:{freqHz}_att:{att}_swee:{sweeptime}_trig:{trigerlevel}_max:{setMaxHold}_linear:{setLinear}_set"))
                                {
                                    warning = "SendToServer fail";
                                    return false;
                                }
                                if (!spClient.WaitingOk(3))
                                {
                                    warning = "Waiting Spectrum reponse fail";
                                    return false;
                                }
                                avgCount = 0;
                                value = 0;
                                for (int i = 0; i < 3; i++)
                                {
                                    DisplayMsg(LogType.Log, "Delay " + delayMs + "ms...");
                                    Thread.Sleep(delayMs);
                                    if (!spClient.SendToServer($"{status_ATS.txtPSN.Text}_pwr"))
                                    {
                                        warning = "SendToServer fail";
                                        return false;
                                    }
                                    string rs = "";
                                    if (!spClient.WaitingOk(5))
                                    {
                                        warning = "Waiting Spectrum reponse fail";
                                        return false;
                                    }
                                    rs = spClient.RECEIVED;
                                    if (rs.ToLower().Contains("result"))
                                    {
                                        pwr = Double.Parse(rs.Split('_')[2]);
                                        DisplayMsg(LogType.Log, "Power:" + pwr);
                                    }
                                    Thread.Sleep(100);

                                    if (!avg && pwr != -999 && (pwr + loss) > threshold)
                                    {
                                        pwr = pwr + loss;
                                        DisplayMsg(LogType.Log, "Power + Cableloss (dB) : " + pwr.ToString());
                                        break;
                                    }

                                    if (avg && pwr != -999 && (pwr + loss) > threshold)
                                    {
                                        pwr = pwr + loss;
                                        DisplayMsg(LogType.Log, "Power + Cableloss (dB) : " + pwr.ToString());
                                        value += pwr;
                                        avgCount++;
                                    }

                                    if (pwr <= threshold) // PE require retry because sometime cannot switch IO
                                    {
                                        DisplayMsg(LogType.Log, $"Power + Cableloss (dB) '{pwr}' is less than threshold '{threshold}', retry....");
                                        continue;
                                    }
                                }

                                if (avg)
                                {
                                    pwr = value / avgCount;
                                    DisplayMsg(LogType.Log, "Total: " + value.ToString());
                                    DisplayMsg(LogType.Log, "Sample count: " + avgCount.ToString());
                                    DisplayMsg(LogType.Log, "Avg Power (dB) : " + pwr.ToString());
                                }
                                status_ATS.AddData(errorItem + "_Pwr", "dB", pwr);
                            }
                            catch (Exception ex)
                            {
                                DisplayMsg(LogType.Log, ex.ToString());
                                warning = "exception";
                            }
                            finally
                            {
                                spClient.SendToServer($"{status_ATS.txtPSN.Text}_over");
                                spClient.CloseConnection();
                                DisplayMsg(LogType.Log, "Close socket");
                            }
                        }
                        #endregion
                        else
                        {
                            #region ms2830a
                            ms2830a.SA.Preset();
                            ms2830a.SA.SetSpan(spanHz);
                            ms2830a.SA.SetRbw(rbwHz);
                            ms2830a.SA.SetVbw(vbwHz);
                            ms2830a.SA.SetRefLevel(rlevDbm);
                            ms2830a.SA.SetCenterFreq(freqHz);
                            ms2830a.SA.SetAttenuation(att);
                            ms2830a.SA.SetSweepTime(sweeptime);
                            //ms2830a.SA.SetTrigger(CTRL.ON);

                            ms2830a.SA.SetMaxHold();
                            Thread.Sleep(5000);//wait max hold

                            dt = DateTime.Now;
                            while (true)
                            {
                                ts = new TimeSpan(DateTime.Now.Ticks - dt.Ticks);

                                if (ts.TotalMilliseconds > timeOutMs ||
                                    status_ATS.CheckListData().Count != 0 ||
                                    warning != string.Empty)
                                {
                                    DisplayMsg(LogType.Error, "Check timeout");
                                    //status_ATS.AddData(errorItem + "_Pwr", "dB", -9999);
                                    status_ATS.AddData(errorItem + "_Pwr", "dB", pwr); //Jason modified

                                    if (String.Compare(Func.ReadINI("Setting", "Setting", "MessageBoxShow", "Disable"), CHK.Enable.ToString(), true) == 0)
                                    {
                                        frmOK.Label = "NG need PE confirm ” (do not power off) ,\r\nGọi PE tới confirm, không được tắt nguồn";
                                        frmOK.ShowDialog();
                                    }

                                }

                                ms2830a.SA.FetchPower(delayMs, ref pwr);

                                pwr = pwr + loss;
                                DisplayMsg(LogType.Log, "Power + Cableloss (dB) : " + pwr.ToString());

                                if (pwr > threshold)
                                {
                                    status_ATS.AddData(errorItem + "_Pwr", "dB", pwr);
                                    return true;
                                }
                                Thread.Sleep(200);
                            }

                            #endregion
                        }
                        break;
                    #endregion
                    case Analyzer.N9000A:
                        #region Analyzer.N9000A
                        string Address = Func.ReadINI("Setting", "N9000A", "Address", "GPIB0::1::INSTR");
                        double CableOffset = Convert.ToDouble(Func.ReadINI("Setting", "N9000A", "CableOffset", "0"));
                        double Att = Convert.ToDouble(Func.ReadINI("Setting", "N9000A", "Att", "0"));
                        double freq = -999;

                        NIVisa nivisa = new NIVisa();
                        MessageBasedSession inst9000A = nivisa.Open_Session(Address);

                        //if (inst9000A == null)
                        //{
                        //    DisplayMsg(LogType.Error, "N9000A Open_Session NG");
                        //    status_ATS.AddData(errorItem + "_Pwr", "dB", -9999);
                        //    nivisa.AGT_N9000A_Get_Marker(inst9000A, ref freqHz, ref pwr);
                        //    MessageBox.Show("N9000A Spectrum無法控制, 請確認後再繼續測試");
                        //    return false;
                        //}
                        if (inst9000A != null)
                        {
                            nivisa.AGT_N9000A_Get_Marker(inst9000A, ref freqHz, ref pwr);
                            DisplayMsg(LogType.Log, "Frquency (Hz) : " + freqHz.ToString());
                            return true;
                        }
                        else
                        {
                            DisplayMsg(LogType.Log, "N9000A Open_Session NG");
                            MessageBox.Show("N9000A Spectrum無法控制, 請確認後再繼續測試");
                            return false;
                        }
                        DisplayMsg(LogType.Log, nivisa.Inst_Get_Info(inst9000A));

                        Thread.Sleep(delayMs);

                        nivisa.AGT_N9000A_Set(inst9000A, freqHz, spanHz, rbwHz, vbwHz, rlevDbm, Att, true);

                        dt = DateTime.Now;

                        while (true)
                        {
                            ts = new TimeSpan(DateTime.Now.Ticks - dt.Ticks);

                            if (ts.TotalMilliseconds > timeOutMs ||
                                status_ATS.CheckListData().Count != 0 ||
                                warning != string.Empty)
                            {
                                DisplayMsg(LogType.Error, "Check timeout");
                                status_ATS.AddData(errorItem + "_Pwr", "dB", -9999);
                                nivisa.Close_Session(inst9000A);
                                return false;
                            }


                            value = 0;
                            for (int i = 0; i < avgCount; i++)
                            {
                                Thread.Sleep(delayMs);

                                nivisa.AGT_N9000A_Get_Marker(inst9000A, ref freq, ref pwr);

                                DisplayMsg(LogType.Log, "Power (dB) : " + pwr.ToString());
                                pwr = pwr + loss;
                                DisplayMsg(LogType.Log, "Power + Cableloss (dB) : " + pwr.ToString());

                                value += pwr;
                            }


                            pwr = value / avgCount;

                            //if (MessageBox.Show("Re-fetching power !!", "Retry", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            //{
                            //    continue;
                            //}

                            if (pwr > threshold)
                            {
                                status_ATS.AddData(errorItem + "_Pwr", "dB", pwr);
                                nivisa.Close_Session(inst9000A);
                                return true;
                            }

                            System.Threading.Thread.Sleep(200);
                        }
                        break;
                    #endregion
                    case Analyzer.NONE:
                        break;
                    default:
                        break;
                }

                if (!CheckGoNoGo())
                    return false;
                else return true;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                status_ATS.AddData(errorItem + "_Pwr", "dB", -9999);
                return false;
            }
            finally
            {
                //ms2830a.SA.SetTrigger(CTRL.OFF);
                DisplayMsg(LogType.Log, "Dect_TxPower: " + pwr.ToString());
                DisplayMsg(LogType.Log, "Dect_TxPower Threshold: " + threshold.ToString());
            }
        }

        private string CheckFwVer()
        {
            if (!CheckGoNoGo())
            {
                return null;
            }

            string item = "ChkFWVer";
            string keyword = "root@OpenWrt:~# \r\n";
            string res = "";
            string FWversion = "";

            try
            {
                DisplayMsg(LogType.Log, "=============== Check FW version ===============");
                //DeviceInfor infor = new DeviceInfor();
                SFCS_Query _Sfcs_Query = new SFCS_Query();

                string SFCSFWver = string.Empty;
                if (status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode)
                {
                    SFCSFWver = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@MFG_FW_17");
                    DisplayMsg(LogType.Log, $"Get FWver From SFCS is: {SFCSFWver}");

                    if (string.IsNullOrEmpty(SFCSFWver) || SFCSFWver.Contains("Dut not have"))
                    {
                        warning = "Get from SFCS FAIL";
                        return null;
                    }

                    string result = string.Empty;
                    result = SFCSFWver.Substring(0, SFCSFWver.Length - 9);
                    DisplayMsg(LogType.Log, $"Get FWver trim is: LRG1_{result}");
                    SFCSFWver = "LRG1_" + result.ToLower();
                }
                else
                {
                    SFCSFWver = Func.ReadINI("Setting", "OTA", "FWVER", "LRG1_v1.0.2.0");
                }


                #region FW version
                int retry = 3;
            mtinfo_retry:
                //Check FW version between SFCS and DUT
                SendAndChk(PortType.SSH, "mt info", keyword, out res, 0, 3000);
                //Rena_20230803, FW v0.1.2.5 timing issue
                if (res.Contains("can't open '/tmp/hwid'") && retry-- > 0)
                {
                    DisplayMsg(LogType.Log, "mt info retry...");
                    Thread.Sleep(200);
                    goto mtinfo_retry;
                }

                Match m = Regex.Match(res, @"FW Version: (?<FWver>.+)");
                if (m.Success)
                {
                    FWversion = m.Groups["FWver"].Value.Trim();
                }

                DisplayMsg(LogType.Log, "FWversion: " + FWversion); // LMG1_v0.1.2.7

                DisplayMsg(LogType.Log, "SFCS_FWversion:" + SFCSFWver);

                //sau nay sua Eform can bo di sfcs CAN SUA THANH: LMG1_v0.1.2.7
                string FWversion1 = FWversion.Substring(FWversion.Length - 7);

                if (string.Compare(FWversion1, SFCSFWver, true) == 0)
                {
                    AddData(item, 0);
                    DisplayMsg(LogType.Log, "Check FW Version between SFCS and DUT PASS");
                    status_ATS.AddDataRaw("LMG1_Label_MFGVER", SFCSFWver, SFCSFWver, "000000");
                }
                else
                {
                    AddData(item, 1);
                    DisplayMsg(LogType.Log, "Check FW Version between SFCS and DUT fail");
                    return string.Empty;
                }

                /*                // Check FW Version betwwen Setting and DUT
                                DisplayMsg(LogType.Log, "FWversion: " + FWversion);
                                DisplayMsg(LogType.Log, "Setting FWversion:" + infor.St_FWver);

                                if (string.Compare(FWversion, infor.St_FWver, true) == 0)
                                {
                                    AddData(item, 0);
                                    DisplayMsg(LogType.Log, "Check FW Version between Setting and DUT PASS");
                                    status_ATS.AddDataRaw("LMG1_Label_MFGVER", FWversion, FWversion, "000000");
                                }
                                else
                                {
                                    AddData(item, 1);
                                    DisplayMsg(LogType.Log, "Check FW Version between Setting and DUT fail");
                                    return;
                                }*/
                #endregion FW version
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData(item, 1);
            }
            return FWversion;
        }


        private string CheckFwVer_JasonModfify()
        {
            if (!CheckGoNoGo())
            {
                return null;
            }

            string item = "ChkFWVer";
            string keyword = "root@OpenWrt";
            string res = "";

            try
            {
                DisplayMsg(LogType.Log, "=============== Check FW version ===============");
                SFCS_Query _Sfcs_Query = new SFCS_Query();

                string SFCSFWver = string.Empty;
                /*if (status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode)
                {
                    SFCSFWver = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@MFG_FW_17");
                    DisplayMsg(LogType.Log, $"Get FWver From SFCS is: {SFCSFWver}");
                    if (string.IsNullOrEmpty(SFCSFWver) || SFCSFWver.Contains("Dut not have"))
                    {
                        warning = "Get @MFG_FW_17 from SFCS FAIL";
                        DisplayMsg(LogType.Log, $"Get @MFG_FW_17 from SFCS FAIL Pls check with DMIS");
                        return null;
                    }
                    string result = string.Empty;
                    result = SFCSFWver.Substring(0, SFCSFWver.Length - 9);
                    DisplayMsg(LogType.Log, $"Get FWver trim is: LRG1_{result}");
                    SFCSFWver = "LRG1_" + result.ToLower();
                }
                else
                {
                    DisplayMsg(LogType.Log, $"Test Offline get by setting ->  [OTA] FWVER=?");
                    SFCSFWver = Func.ReadINI("Setting", "OTA", "FWVER", "LRG1_v1.0.2.0");
                }*/
                SFCSFWver = Func.ReadINI("Setting", "OTA", "FWVER", "LRG1_v1.0.2.0");
                #region FW version
                int retry = 3;
            mtinfo_retry:
                //Check FW version between SFCS and DUT
                res = string.Empty;
                SendAndChk(PortType.SSH, "mt info", keyword, out res, 0, 3000);
                if (res.Contains("can't open '/tmp/hwid'") && retry-- > 0)
                {
                    DisplayMsg(LogType.Log, "mt info retry...");
                    Thread.Sleep(200);
                    goto mtinfo_retry;
                }

                if (res.Contains(SFCSFWver))
                {
                    DisplayMsg(LogType.Log, "Check DUT FW with Setting PASS");
                    DisplayMsg(LogType.Log, "DUT FW version: " + SFCSFWver);
                    DisplayMsg(LogType.Log, "Setting FW version:" + SFCSFWver);
                    status_ATS.AddDataRaw("LRG1_OTA_MFGVER", SFCSFWver, SFCSFWver, "000000");
                    AddData(item, 0);
                    return SFCSFWver;
                }
                else
                {
                    DisplayMsg(LogType.Log, "Check DUT FW with SFCS FAIL");
                    DisplayMsg(LogType.Log, "SFCS FW version:" + SFCSFWver);
                    AddData(item, 1);
                    return null;
                }
                #endregion FW version
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData(item, 1);
                return null;
            }
        }


        private void JudgeFW_Jason_Modfied()
        {
            if (!CheckGoNoGo())
            {
                DisplayMsg(LogType.Log, "=============== Fail GonoGo ===============");
                return;
            }
            string item = "SwitchDMPFW";
            DisplayMsg(LogType.Log, "=============== Switch to DMP ===============");
            try
            {
                SFCS_Query _Sfcs_Query = new SFCS_Query();
                ATS_Template.SFCS_ATS_2_0.ATS ss = new ATS_Template.SFCS_ATS_2_0.ATS();
                string res = string.Empty;
                string CheckFWversion = string.Empty;
                CheckFWversion = CheckFwVer_JasonModfify();
                if (!string.IsNullOrEmpty(CheckFWversion)) // Check FW with SFCS PASS
                {
                    DisplayMsg(LogType.Log, $"FW is Meet with Setting...... ");
                }
                else
                {
                    DisplayMsg(LogType.Log, $"FW Not Meet with Setting...... ");
                    warning = "Check FW with Setting fail";
                    return;
                }

                // FW < V1.0.0 will use -> bt_upgrade_utility -f /lib/firmware/efr32/rcp_no_encrypt_afh.gbl -p /dev/ttyMSM1
                // FW >= V1.0.0 will use -> bt_upgrade_utility -f /lib/firmware/efr32/rcp4.3.0_no_encrypt_afh.gbl -p /dev/ttyMSM1
                if (CheckFWversion.Contains("v0.1."))
                {
                    DisplayMsg(LogType.Log, $"Input version < v1.0.0");
                    if (!SendAndChk(PortType.SSH, "bt_upgrade_utility -f /lib/firmware/efr32/rcp_no_encrypt_afh.gbl -p /dev/ttyMSM1", "Transfer completed successfully", out res, 0, 60 * 1000))
                    {
                        DisplayMsg(LogType.Log, "Switch DMP FW fail");
                        AddData("SwitchDMPFW", 1);
                        return;
                    }
                    else
                    {
                        DisplayMsg(LogType.Log, "Switch DMP FW pass");
                        AddData("SwitchDMPFW", 0);
                    }
                }
                else
                {
                    DisplayMsg(LogType.Log, $"Input version >= v1.0.0");
                    if (!SendAndChk(PortType.SSH, "bt_upgrade_utility -f /lib/firmware/efr32/rcp4.3.0_no_encrypt_afh.gbl -p /dev/ttyMSM1", "Transfer completed successfully", out res, 0, 60 * 1000))
                    {
                        DisplayMsg(LogType.Log, "Switch DMP FW fail");
                        AddData("SwitchDMPFW", 1);
                        return;
                    }
                    else
                    {
                        DisplayMsg(LogType.Log, "Switch DMP FW pass");
                        AddData("SwitchDMPFW", 0);
                    }
                }

            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, $"SwitchDMPFW_Exception {ex.Message}");
                AddData(item, 1);
                return;
            }

        }
        private void JudgeFW()
        {
            if (CheckGoNoGo()) { return; }
            string item = "SwitchDMPFW";
            DisplayMsg(LogType.Log, "=============== Switch to DMP ===============");
            try
            {
                SFCS_Query _Sfcs_Query = new SFCS_Query();
                ATS_Template.SFCS_ATS_2_0.ATS ss = new ATS_Template.SFCS_ATS_2_0.ATS();


                string res = string.Empty;
                string fwVer = this.CheckFwVer().Split('v')[1]; // LRG1_v0.1.2.8

                DisplayMsg(LogType.Log, @"fw__ver_" + fwVer);
                //string SemaphoreVer = WNC.API.Func.ReadINI("Setting", "OTA", "Version", "1.0.0.0");
                string SemaphoreVer = string.Empty;
                if (status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode)
                {
                    SemaphoreVer = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@MFG_FW_17");
                    DisplayMsg(LogType.Log, $"Get FWver From SFCS is: {SemaphoreVer}");

                    if (string.IsNullOrEmpty(SemaphoreVer) || SemaphoreVer.Contains("Dut not have"))
                    {
                        warning = "Get from SFCS FAIL";
                        return;
                    }

                    string result = string.Empty;
                    result = SemaphoreVer.Substring(0, SemaphoreVer.Length - 9);
                    DisplayMsg(LogType.Log, $"Get FWver trim is: LRG1_{result}");
                    SemaphoreVer = "LRG1_" + result.ToLower();
                }
                else
                {
                    SemaphoreVer = Func.ReadINI("Setting", "OTA", "FWVER", "LRG1_v1.0.2.0");
                }

                Version targetVer = new Version(SemaphoreVer);
                Version outputVer = null;
                //這裡只有DUT要切換成DMP FW,陪測Golden需要手動切換一次就好
                //Switch to DMP(dynamic multi-protocol) FW
                if (Version.TryParse(fwVer, out outputVer))
                {
                    int comparisonResult = outputVer.CompareTo(targetVer);

                    if (comparisonResult >= 0)
                    {
                        DisplayMsg(LogType.Log, $"Input version is greater than 1.0.0.0 __comparsion__{comparisonResult}");
                        if (!SendAndChk(PortType.SSH, "bt_upgrade_utility -f /lib/firmware/efr32/rcp4.3.0_no_encrypt_afh.gbl -p /dev/ttyMSM1", "Transfer completed successfully", out res, 0, 60 * 1000))
                        {
                            DisplayMsg(LogType.Log, "Switch DMP FW fail");
                            AddData("SwitchDMPFW", 1);
                            return;
                        }
                        else
                        {
                            DisplayMsg(LogType.Log, "Switch DMP FW pass");
                            AddData("SwitchDMPFW", 0);
                        }
                    }
                    else if (comparisonResult < 0)
                    {
                        DisplayMsg(LogType.Log, $"Input version is less than 1.0.0.0 __comparsion__{comparisonResult}");
                        if (!SendAndChk(PortType.SSH, "bt_upgrade_utility -f /lib/firmware/efr32/rcp_no_encrypt_afh.gbl -p /dev/ttyMSM1", "Transfer completed successfully", out res, 0, 60 * 1000))
                        {
                            DisplayMsg(LogType.Log, "Switch DMP FW fail");
                            AddData("SwitchDMPFW", 1);
                            return;
                        }
                        else
                        {
                            DisplayMsg(LogType.Log, "Switch DMP FW pass");
                            AddData("SwitchDMPFW", 0);
                        }
                    }
                    //else
                    //{
                    //    DisplayMsg(LogType.Log, @"Input version is equal to 1.0.0.");
                    //    if (!SendAndChk(PortType.SSH, "bt_upgrade_utility -f /lib/firmware/efr32/rcp4.3.0_no_encrypt_afh.gbl -p /dev/ttyMSM1", "Transfer completed successfully", out res, 0, 60 * 1000))
                    //    {
                    //        DisplayMsg(LogType.Log, "Switch DMP FW fail");
                    //        AddData("SwitchDMPFW", 1);
                    //        return;
                    //    }
                    //}
                }
                else
                {
                    DisplayMsg(LogType.Log, "Invalid version string format");
                    return;
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, $"SwitchDMPFW_Exception {ex.Message}");
                AddData(item, 1);
            }


        }
        private void OTA_Thread()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            string item = "Thread";
            string res = "";
            string broadcast_pan_id = "";
            string broadcast_channel = "";

            try
            {
                DisplayMsg(LogType.Log, "=============== 7.3.1 Thread ===============");
                // ==============================================================
                //this.JudgeFW();
                // ==============================================================

                //這裡只有DUT要切換成DMP FW,陪測Golden需要手動切換一次就好
                //Switch to DMP(dynamic multi-protocol) FW
                /* if (!SendAndChk(PortType.SSH, "bt_upgrade_utility -f /lib/firmware/efr32/rcp_no_encrypt_afh.gbl -p /dev/ttyMSM1", "Transfer completed successfully", out res, 0, 60 * 1000))
                 {
                     DisplayMsg(LogType.Log, "Switch DMP FW fail");
                     AddData("SwitchDMPFW", 1);
                     return;
                 }
                 else
                 {
                     DisplayMsg(LogType.Log, "Switch DMP FW pass");
                     AddData("SwitchDMPFW", 0);
                 }*/

                //DUT: broadcast
                JudgeFW_Jason_Modfied();

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
                return;
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

            string respes = "Secondary CPC v4.3.0";  //will output v4.2.0 if FW < v1.0.0.0
            string flag = WNC.API.Func.ReadINI("Setting", "OTA", "CPCDVer", respes);

            try
            {
                DisplayMsg(LogType.Log, "=============== Start Thread Broadcast ===============");

                //Enable CPCD
                SendAndChk(PortType.SSH, "service cpcd start", keyword, out res, 0, 5000);
                DisplayMsg(LogType.Log, "Delay 5s...");
                Thread.Sleep(5000); //一定要delay,不然下一步會error
                SendAndChk(PortType.SSH, "logread -e cpcd", keyword, out res, 0, 5000);
                if (!res.Contains("Daemon startup was successful") || !res.Contains(flag))
                {
                    DisplayMsg(LogType.Log, $"Enable CPCD check: '{flag}' and 'Daemon startup was successful' FAIL");
                    DisplayMsg(LogType.Log, @"Secondary CPC = v4.2.0 if FW<0.1.2.8");
                    DisplayMsg(LogType.Log, @"Secondary CPC = v4.3.0 if FW>=0.1.2.8");
                    AddData(item, 1);
                    return;
                }
                DisplayMsg(LogType.Log, $"Enable CPCD check: '{flag}' and 'Daemon startup was successful' PASS");


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

            DisplayMsg(LogType.Log, $"broadcast_channel: '{broadcast_channel}'");
            DisplayMsg(LogType.Log, $"broadcast_pan_id: '{broadcast_pan_id}'");

            string item = "ThreadScan";
            string res = "";
            string keyword = "root@OpenWrt:~# \r\n";
            string pan_id = "";
            string mac_addr = "";
            string channel = "";
            double rssi = 0;
            double rssi_sum = 0;
            int rssiCount = 3; //要抓幾次rssi
            List<double> rssi_val = new List<double>();

            int CountRetrygetRSSI = 3;

            try
            {
                DisplayMsg(LogType.Log, "=============== Start Thread Scan ===============");

                if (!ChkInitial(PortType.GOLDEN_SSH, keyword, 120 * 1000))
                {
                    DisplayMsg(LogType.Log, "Golden SSH fail");
                    AddData(item, 1);
                    return;
                }
            RetrygetRSSI: //Jason add RETRY to solve RSSI Check NG 2023/10/15
                //start Thread scan
                for (int i = 1; i <= rssiCount; i++)
                {
                    pan_id = "";
                    mac_addr = "";
                    channel = "";
                    rssi = 0;

                    SendAndChk(PortType.GOLDEN_SSH, $"ot-ctl scan {broadcast_channel}", keyword, out res, 0, 10 * 1000);
                    if (res.Contains("connect session failed: No such file or directory"))
                    {
                        //Enable CPCD
                        SendAndChk(PortType.GOLDEN_SSH, "service cpcd start", keyword, out res, 0, 5000);
                        DisplayMsg(LogType.Log, "[Golden] Delay 5s...");
                        Thread.Sleep(5000); //一定要delay,不然下一步會error
                        SendAndChk(PortType.GOLDEN_SSH, "logread -e cpcd", keyword, out res, 0, 5000);
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

                        //SendAndChk(PortType.GOLDEN_SSH, "ot-ctl scan", "Done", out res, 0, 10 * 1000);
                        SendAndChk(PortType.GOLDEN_SSH, $"ot-ctl scan {broadcast_channel}", "Done", out res, 0, 10 * 1000);
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

                                /*if (string.IsNullOrEmpty(pan_id)|| string.IsNullOrEmpty(mac_addr)||string.IsNullOrEmpty(channel))
                                {
                                    if (CountRetrygetRSSI > 0)
                                    {
                                        DisplayMsg(LogType.Log, $"Cannot get RSSI Go to Re-try Get RSSI time: {CountRetrygetRSSI.ToString()}");  //Jason add RETRY to solve RSSI Check NG 2023/10/15
                                        CountRetrygetRSSI--;
                                        rssi_val.Clear();
                                        goto RetrygetRSSI;
                                    }

                                    DisplayMsg(LogType.Log, $"Get RSSI Fail");
                                    status_ATS.AddData("Thread_RSSI", "dBm", "-9999");
                                }*/

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
                    DisplayMsg(LogType.Log, $"Rssi count {rssi_val.Count} fail");
                    if (CountRetrygetRSSI > 0)
                    {
                        DisplayMsg(LogType.Log, $"Re-try To Get RSSI time {CountRetrygetRSSI.ToString()}");  //Jason add RETRY to solve RSSI Check NG 2023/10/15
                        CountRetrygetRSSI--;
                        rssi_val.Clear();
                        goto RetrygetRSSI;
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

        private void EthernetTest(int port_num)
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            int retry_cnt;
            string item = "EthernetTest";
            //string keyword = @"root@OpenWrt";
            //string res = "";
            // ================================================
            //PCBA
            //RF
            //OTA
            //FINAL
            // ================================================
            DisplayMsg(LogType.Log, "=============== Ethernet Test ===============");
            retry_cnt = 0;
            try
            {
                frmOK.Label = $"Sau khi kết nối dây mạng vào cổng LAN số {port_num}, vui lòng nhấn\"Xác nhận\"";
                frmOK.ShowDialog();

            LAN_Port_Test:
                if (SendAndChk(PortType.SSH, "mt eth linkrate", $"port {port_num}: 2500M FD", 0, 3000))
                {
                    DisplayMsg(LogType.Log, $"Check LAN Port{port_num} pass");
                    if (port_num == 5)
                    {
                        DisplayMsg(LogType.Log, "Check WAN Port pass");
                    }
                }
                else
                {
                    DisplayMsg(LogType.Log, $"Check LAN Port{port_num} fail");
                    if (retry_cnt++ < 3)
                    {
                        frmOK.Label = $"Vui lòng kiểm tra dây mạng đã được kết nối đúng vào cổng LAN số {port_num} chưa";
                        frmOK.ShowDialog();
                        DisplayMsg(LogType.Log, "Delay 1000ms, retry...");
                        Thread.Sleep(1000);
                        goto LAN_Port_Test;
                    }
                    else
                    {
                        AddData($"Eth_LAN_Port{port_num}", 1);
                        return;
                    }
                }

            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData(item, 1);
                return;
            }
        }
        private void OTA_NFC()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            string item = "NFC";
            string res = "";

            try
            {
                DisplayMsg(LogType.Log, "=============== NFC ===============");

                //Check NFC field detection pin
                SendAndChk(PortType.SSH, "mt gpio dump nfc", "root@OpenWrt:~# \r\n", out res, 0, 3000);
                if (res.Contains("NFC: high"))
                {
                    DisplayMsg(LogType.Log, "Check NFC field detection pin - high pass");
                    AddData(item, 0);
                }
                else
                {
                    DisplayMsg(LogType.Log, "Check NFC field detection pin - high fail");
                    AddData(item, 1);
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                AddData(item, 1);
            }
        }
        private bool ResetDect()
        {
            bool IsResetOK = false;
            string res = string.Empty;
            string item = "Reset__Dect";
            string keyword = "root@OpenWrt";
            try
            {
                SendAndChk(PortType.SSH, "qqqqq\r\n", keyword, out res, 0, 2500);
                //do
                //{
                //    //SendCommand(PortType.SSH, sCtrlZ, 2000); //SendKeys.SendWait("{^Z}");
                //    if (SendAndChk(PortType.SSH, "qqqqq\r\n", keyword, out res, 0, 2000))
                //    { break; }
                //} while (ChkResponse(PortType.SSH, ITEM.NONE, keyword, out res, 3000));
                //Reboot DECT
                DisplayMsg(LogType.Log, "Reboot DECT");
                SendAndChk(PortType.SSH, "echo 0 > /sys/class/gpio/dect_rst/value", keyword, out res, 0, 3000);
                Thread.Sleep(1800);
                SendAndChk(PortType.SSH, "echo 1 > /sys/class/gpio/dect_rst/value", keyword, out res, 0, 3000);
                DisplayMsg(LogType.Log, "Delay 3s...");
                Thread.Sleep(2800);
                IsResetOK = true;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData(item, 1);
            }
            return IsResetOK;
        }
        private void enterMode()
        {
            string item = "enterMode";
            string res = string.Empty;
            bool result = false;
            int delayMs = 3000;
            try
            {
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
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                warning = ">>>>> cmbs_tcx -comname ttyMSM2 -baud 460800 INVAILD";
                return;
            }
        }

    }
}
