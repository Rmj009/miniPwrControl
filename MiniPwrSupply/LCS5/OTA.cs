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

namespace MiniPwrSupply.LCS5
{
    public partial class frmMain
    {
        private void OTA()
        {
            string keyword = "root@OpenWrt:/#";
            string res = "";
            string _WNCSN = "";
            string _MAC = "";
            string sfcsSN = "";
            string sfcsMAC = "";
            try
            {
                DisplayMsg(LogType.Log, "================= OTA TEST ===============");

                if (Func.ReadINI("Setting", "IO_Board_Control", "IO_Control_1", "0") == "1")
                {
                    string rev_message = "";
                    string txPin = Func.ReadINI("Setting", "IO_Board_Control", "Pin0", "0");
                    status_ATS.AddLog("IO_Board_Y" + txPin + " On...");
                    IO_Board_Control1.ConTrolIOPort_write(Int32.Parse(txPin), "1", ref rev_message);
                    DisplayMsg(LogType.Log, rev_message);

                    string txPin1 = Func.ReadINI("Setting", "IO_Board_Control", "Pin1", "1");
                    status_ATS.AddLog("IO_Board_Y" + txPin1 + " On...");
                    IO_Board_Control1.ConTrolIOPort_write(Int32.Parse(txPin1), "1", ref rev_message);
                    DisplayMsg(LogType.Log, rev_message);
                }
                else if (Func.ReadINI("Setting", "Port", "RelayBoard", "Disable").ToUpper() == "ENABLE")
                {
                    SwitchRelay(CTRL.OFF);
                    Thread.Sleep(5000); //electric container
                    SwitchRelay(CTRL.ON);
                }
                else
                {
                    MessageBox.Show("Power on");
                }

                DisplayMsg(LogType.Log, $"delay {delayMFGFW}s before bootup");
                Thread.Sleep(delayMFGFW * 1000);

                #region BootUp
                if (!ChkInitial(PortType.TELNET, keyword, 100 * 1000))
                {
                    AddData("BootUp", 1);
                    return;
                }
                AddData("BootUp", 0);
                #endregion


                //#region Get SN from DUT
                //if (true)
                //{
                //    SendAndChk(PortType.TELNET, "fw_printenv serialno", "#", out res, 0, 1000);
                //    Match m = Regex.Match(res, @"serialno=(?<sn>\d+)");
                //    if (m.Success)
                //    {
                //        _WNCSN = m.Groups["sn"].Value.ToString();
                //        DisplayMsg(LogType.Log, "SN from DUT: " + _WNCSN);
                //        if (_WNCSN.Length != 12)
                //        {
                //            warning = "Get SN fail";
                //            DisplayMsg(LogType.Log, "Get SN from DUT fail");
                //            return;
                //        }
                //        SetTextBox(status_ATS.txtPSN, _WNCSN);
                //        status_ATS.SFCS_Data.First_Line = _WNCSN;
                //        status_ATS.SFCS_Data.PSN = _WNCSN;
                //    }
                //    else
                //    {
                //        warning = "Get SN fail";
                //        DisplayMsg(LogType.Log, "Get SN from DUT fail");
                //        return;
                //    }
                //}
                //#endregion


                //if (status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode)
                //{
                //    _WNCSN = status_ATS.txtPSN.Text;
                //    GetFromSfcs("@CHJGP1_SN", out sfcsSN);
                //    DisplayMsg(LogType.Log, "SN from SFCS is " + sfcsSN);

                //    GetFromSfcs("@MAC", out sfcsMAC);
                //    DisplayMsg(LogType.Log, "MAC from SFCS is " + sfcsMAC);

                //    if (sfcsSN != _WNCSN)
                //    {
                //        warning = "Check SN with sfcs fail";
                //        return;
                //    }
                //}
                //else
                //{
                //    _WNCSN = status_ATS.txtPSN.Text;
                //    //_MAC = status_ATS.txtSP.Text;
                //}
                if (Func.ReadINI(Application.StartupPath, "Setting", "Skip_Item", "Wifi2G", "0") == "0")
                {
                    WiFi(WiFiType.WiFi_2G);
                }
                if (Func.ReadINI(Application.StartupPath, "Setting", "Skip_Item", "Wifi5G", "0") == "0")
                {
                    WiFi(WiFiType.WiFi_5G);
                }
                this.WiFiDown();

                //CheckCalibrationData();
                // testPlan0814 OTA testItem to RF testItems
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, @"OTA: " + ex.Message);
                warning = "Exception";
            }
            finally
            {
                if (Func.ReadINI("Setting", "IO_Board_Control", "IO_Control_1", "0") == "1")
                {
                    string rev_message = "";
                    string txPin = Func.ReadINI("Setting", "IO_Board_Control", "Pin0", "0");
                    status_ATS.AddLog("IO_Board_Y" + txPin + " Off...");
                    IO_Board_Control1.ConTrolIOPort_write(Int32.Parse(txPin), "2", ref rev_message);
                    DisplayMsg(LogType.Log, rev_message);

                    string txPin1 = Func.ReadINI("Setting", "IO_Board_Control", "Pin1", "1");
                    status_ATS.AddLog("IO_Board_Y" + txPin1 + " Off...");
                    IO_Board_Control1.ConTrolIOPort_write(Int32.Parse(txPin1), "2", ref rev_message);
                    DisplayMsg(LogType.Log, rev_message);
                }
                else SwitchRelay(CTRL.ON);
            }
        }

    }
}
