using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiniPwrSupply.IMQX
{
    internal class PCBA
    {
        public PCBA() { }
        private void PCBA_()
        {
            try
            {
                KillTaskProcess("QPST");
                if (!CheckGoNoGo()) return;
                string res = "";
                bool rs = false;
                string HWID = Func.ReadINI("Setting", "IMQX_Infor", "HWID", "0 0 0 1");
                string Ext_Ver = Func.ReadINI("Setting", "IMQX_Infor", "Ext_Ver", "101.091001027070.001");
                string Int_Ver = Func.ReadINI("Setting", "IMQX_Infor", "Int_Ver", "101.091001027070.001");
                string SN = Func.ReadINI("Setting", "IMQX_Infor", "SN", "None");
                string Model = Func.ReadINI("Setting", "IMQX_Infor", "Model", "IMQX");
                string Description = Func.ReadINI("Setting", "IMQX_Infor", "Model_Description", "IMQX_DESC");
                string ModelClass = Func.ReadINI("Setting", "IMQX_Infor", "Model_Class", "IMQX_CLASS");


                #region may can remove
                string smt = status_ATS.txtPSN.Text;
                string labelSn = status_ATS.txtPSN.Text;
                string sn = labelSn;
                string IMEI = labelSn;


                string LinuxVer = Func.ReadINI("Setting", "IMQX_Infor", "LinuxVer", "None");
                string MFG_FW_Ver = Func.ReadINI("Setting", "IMQX_Infor", "MFG_FW_Ver", "None");
                string MFG_QCOM_Ver = Func.ReadINI("Setting", "IMQX_Infor", "MFG_QCOM_Ver", "None");
                string BT_FW_Ver = Func.ReadINI("Setting", "IMQX_Infor", "BT_FW_Ver", "None");
                string QCN_Ver = Func.ReadINI("Setting", "IMQX_Infor", "QCN_Ver", "None");
                NewNetPort _net = new NewNetPort();
                #endregion may can remove
                manualTest = Func.ReadINI("Setting", "Setting", "manualTest", "Enable") == "Enable";

                //string KeyPath_backup = Func.ReadINI("Setting", "AF62", "KeyPath_backup", "\\");
                //DirectoryInfo KeyPath = new DirectoryInfo(Func.ReadINI("Setting", "AF62", "KeyPath", "\\"));
                //object[] DirInfo_Key_folder = KeyPath.GetFileSystemInfos().OrderByDescending(f => f.LastWriteTime).ToArray();
                myCC.Start();

                try
                {
                    #region Get Information form SFCS
                    if (status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode)
                    {
                        SFCS_Query _sfcsQuery = new SFCS_Query();

                        //GetFromSfcs("@AF62_SN", out sn);
                        //GetFromSfcs("N78W", out SN_N78W);
                        //int snLength = Convert.ToInt32(Func.ReadINI("Setting", "Match", "SN_Length", "11"));
                        //string snStartwith = Func.ReadINI("Setting", "Match", "SN_Start", "T");
                        //if (SN_N78W.Length != snLength)
                        //{
                        //    CreatePsnFile();
                        //    if (!ChkCombine())
                        //    {
                        //        warning = "Combine fail";
                        //        return;
                        //    }
                        //}
                        ////GetFromSfcs("@AF62_SN", out sn);
                        //GetFromSfcs("N78W", out SN_N78W);

                        //GetFromSfcs("@MAC", out MAC);
                        //GetFromSfcs("@IMEI", out IMEI);
                        //if (SN_N78W.Length != snLength && !SN_N78W.StartsWith(snStartwith))
                        //{
                        //    DisplayMsg(LogType.Log, $"SN length:{snLength}");
                        //    DisplayMsg(LogType.Log, $"SN start with:{snStartwith}");
                        //    warning = "SN format fail";
                        //    return;
                        //}
                        //DisplayMsg(LogType.Log, $"SMT: {smt}");

                        //result = false;
                        //result = IMEI.Length == 15 ? true : false;
                        //string Imei_USL = Func.ReadINI("Setting", "IMEI_RULE", "IMEI_USL", "35690885156947");
                        //string Imei_LSL = Func.ReadINI("Setting", "IMEI_RULE", "IMEI_LSL", "35690885156748");
                        //result = result & (Convert.ToInt64(Imei_LSL.Substring(0, 14)) <= Convert.ToInt64(IMEI.Substring(0, 14))) && (Convert.ToInt64(Imei_USL.Substring(0, 14)) >= Convert.ToInt64(IMEI.Substring(0, 14))) ? true : false;
                        //if (!result)
                        //{
                        //    warning = "IMEI_Rule";
                        //    DisplayMsg(LogType.Log, $"IMEI must in range from {Imei_LSL} to {Imei_USL} ");
                        //    return;
                        //}
                        ////SetTextBox(status_ATS.txtPSN, sn);
                        ////SetTextBox(status_ATS.txtSP, MAC);

                        //status_ATS.SFCS_Data.PSN = labelSn;

                        //DisplayMsg(LogType.Log, $"SN N78W from SFCS: {SN_N78W}");
                        //DisplayMsg(LogType.Log, $"MAC from SFCS: {MAC}");
                        //DisplayMsg(LogType.Log, $"IMEI from SFCS: {IMEI}");
                        //DisplayMsg(LogType.Log, $"SMT: {smt}");
                        //DisplayMsg(LogType.Log, $"Label sn: {sn}");

                        //DisplayMsg(LogType.Log, "Set first line: " + smt + "," + sn);
                        //status_ATS.SFCS_Data.First_Line = smt + "," + sn;
                    }
                    else
                    {
                        Random rand = new Random();
                        int t = rand.Next();
                        HWID = Func.ReadINI("Setting", "IMQX_Infor", "HWID", "0 0 0 1");
                        Ext_Ver = Func.ReadINI("Setting", "IMQX_Infor", "Ext_Ver", "101.091001027070.001");
                        Int_Ver = Func.ReadINI("Setting", "IMQX_Infor", "Int_Ver", "101.091001027070.001");
                        IMEI = Func.ReadINI("Setting", "IMQX_Infor", "IMEI", "355806710000036");
                        Model = Func.ReadINI("Setting", "IMQX_Infor", "Model", "IMQX") + t;
                        Description = Func.ReadINI("Setting", "IMQX_Infor", "Model_Description", "IMQX_DESC") + t;
                        ModelClass = Func.ReadINI("Setting", "IMQX_Infor", "Model_Class", "IMQX_CLASS") + t;
                        QCN_Ver = Func.ReadINI("Setting", "IMQX_Infor", "QCN_Ver", "None");
                        long tmpIMEI = 0;
                        if (long.TryParse(IMEI, out tmpIMEI))
                        {
                            IMEI = "" + (tmpIMEI + t);
                        }

                        DisplayMsg(LogType.Log, "HWID : " + HWID);
                        DisplayMsg(LogType.Log, "Ext_Ver : " + Ext_Ver);
                        DisplayMsg(LogType.Log, "Int_Ver : " + Int_Ver);
                        DisplayMsg(LogType.Log, "SN : " + sn);
                        DisplayMsg(LogType.Log, "IMEI : " + IMEI);
                        DisplayMsg(LogType.Log, "Model : " + Model);
                        DisplayMsg(LogType.Log, "Description : " + Description);
                        DisplayMsg(LogType.Log, "ModelClass : " + ModelClass);
                        DisplayMsg(LogType.Log, "QCN_Ver : " + QCN_Ver);
                    }
                    #endregion
                    if (Func.ReadINI("Setting", "Golden", "GoldenSN", "NONE").Contains(status_ATS.txtPSN.Text))
                    {
                        DisplayMsg(LogType.Log, "Golden is testing");
                        isGolden = true;
                    }
                    else isGolden = false;
                    // no power supply
                    SetPowerSupplyOnOff(Powersupply3323.PSOnOff.ON);

                    if (!ChkStation(status_ATS.txtPSN.Text))
                    {
                        warning = "Wrong Station";
                        return;
                    }

                    //MessageBox.Show("Please press button SW29");
                    if (Func.ReadINI("Setting", "IO_Board_Control", "IO_Control", "0") == "1")
                    {
                        int pin = Convert.ToInt32(Func.ReadINI("Setting", "IO_Board_Control", "Pin", "1"));
                        IO_Board_Control.ConTrolIOPort_write(pin, "1", ref rev_message);
                        DisplayMsg(LogType.Log, rev_message);
                    }
                    if (use_usb_relay_1 == 1)
                    {
                        USBRelay1OFF();
                    }

                    Thread.Sleep(5000);
                    //======================== BYPASS ========================
                    //======================== BYPASS ========================
                    //TODO:Check Current
                    //GetCurr("POWER_ON", ps_3615_chan_1);
                    //Thread.Sleep(2000);
                    //MessageBox.Show("Please release button SW29");
                    Thread.Sleep(3000);
                    //Test_DC();
                    #region FW download                

                    string FWPath = Func.ReadINI("Setting", "IMQX", "FWPath", "\\");

                    if (!UpgradeFW(FWPath))
                    {
                        DisplayMsg(LogType.Log, "Upgrade FW fail");
                        AddData("FWUpgrade", 1);
                        return;
                    }
                    AddData("FWUpgrade", 0);
                    #endregion

                    #region reboot device

                    if (!CheckGoNoGo())
                    {
                        return;
                    }

                    DisplayMsg(LogType.Log, "reboot device");

                    if (Func.ReadINI("Setting", "IO_Board_Control", "IO_Control", "0") == "1")
                    {
                        int pin = Convert.ToInt32(Func.ReadINI("Setting", "IO_Board_Control", "Pin", "1"));
                        IO_Board_Control.ConTrolIOPort_write(pin, "2", ref rev_message);
                        DisplayMsg(LogType.Log, rev_message);
                    }
                    if (use_usb_relay_1 == 1)
                    {
                        USBRelay1ON();
                    }
                    if (Func.ReadINI("Setting", "PowerSupply", "PS3323", "0") == "1")
                    {
                        SetPowerSupplyOnOff(Powersupply3323.PSOnOff.OFF);
                    }
                    SwitchRelay(CTRL.ON);
                    Thread.Sleep(5000);
                    SwitchRelay(CTRL.OFF);

                    KillTaskProcess("QPST");

                    DisplayMsg(LogType.Log, "Wait 5 seconds before boot up");
                    Thread.Sleep(5000);
                    //======================== BYPASS ========================
                    //======================== BYPASS ========================

                    CallQpst();

                    if (Func.ReadINI("Setting", "IO_Board_Control", "IO_Control", "0") == "1")
                    {
                        int pin = Convert.ToInt32(Func.ReadINI("Setting", "IO_Board_Control", "Pin", "1"));
                        IO_Board_Control.ConTrolIOPort_write(pin, "1", ref rev_message);
                        DisplayMsg(LogType.Log, rev_message);
                    }
                    else if (use_usb_relay_1 == 1)
                    {
                        USBRelay1OFF();
                    }
                    else if (Func.ReadINI("Setting", "PowerSupply", "PS3323", "0") == "1")
                    {
                        SetPowerSupplyOnOff(Powersupply3323.PSOnOff.ON);
                    }
                    else
                    {
                        //MessageBox.Show("Manual reboot");
                        SwitchRelay(CTRL.ON);
                        Thread.Sleep(5000);
                        SwitchRelay(CTRL.OFF);
                    }

                    #endregion

                    if (!CheckDeviceIsReady(100))
                    {
                        AddData("ChkDevice", 1);
                        return;
                    }
                    //======================== BYPASS ========================
                    DownloadQCN();
                    //======================== BYPASS ========================
                    #region reboot device bootupIMQX()
                    if (!CheckGoNoGo())
                    {
                        return;
                    }
                    DisplayMsg(LogType.Log, "reboot device");

                    if (Func.ReadINI("Setting", "IO_Board_Control", "IO_Control", "0") == "1")
                    {
                        int pin = Convert.ToInt32(Func.ReadINI("Setting", "IO_Board_Control", "Pin", "1"));
                        IO_Board_Control.ConTrolIOPort_write(pin, "2", ref rev_message);
                        DisplayMsg(LogType.Log, rev_message);
                    }
                    if (use_usb_relay_1 == 1)
                    {
                        USBRelay1ON();
                    }
                    if (Func.ReadINI("Setting", "PowerSupply", "PS3323", "0") == "1")
                    {
                        SetPowerSupplyOnOff(Powersupply3323.PSOnOff.OFF);
                    }

                    DisplayMsg(LogType.Log, "Wait 5 seconds before boot up");
                    Thread.Sleep(5000);

                    if (Func.ReadINI("Setting", "IO_Board_Control", "IO_Control", "0") == "1")
                    {
                        int pin = Convert.ToInt32(Func.ReadINI("Setting", "IO_Board_Control", "Pin", "1"));
                        IO_Board_Control.ConTrolIOPort_write(pin, "1", ref rev_message);
                        DisplayMsg(LogType.Log, rev_message);
                    }
                    else if (use_usb_relay_1 == 1)
                    {
                        USBRelay1OFF();
                    }
                    else if (Func.ReadINI("Setting", "PowerSupply", "PS3323", "0") == "1")
                    {
                        SetPowerSupplyOnOff(Powersupply3323.PSOnOff.ON);
                    }
                    else
                    {
                        //MessageBox.Show("Manual reboot");
                        SwitchRelay(CTRL.ON);
                        Thread.Sleep(5000);
                        SwitchRelay(CTRL.OFF);
                    }
                    #endregion

                    if (!CheckDeviceIsReady(100))
                    {
                        AddData("ChkDevice", 1);
                        return;
                    }

                    Send_Res_CC(myCC, "adb root", "", out res, 200);
                    DisplayMsg(LogType.Log, "Delay 5s...");
                    Thread.Sleep(5 * 1000);

                    if (!ChkADBDevice("adb shell\r\n", "#", 200 * 1000))
                    {
                        AddData("ChkDevice", 1);
                        return;
                    }
                    AddData("ChkDevice", 0);

                    #region Read Device identification
                    if (!Send_Res_CC(myCC, "devmem 0x221C8744 32", "0x001B40E1", out res))
                    {
                        AddData("ReadDeviceIden", 1);
                        return;
                    }
                    AddData("ReadDeviceIden", 0);

                    #endregion


                    #region check HW version
                    /*
                     adb root
                    adb shell
                    echo 427 > /sys/class/gpio/export
                    echo 428 > /sys/class/gpio/export
                    echo 429 > /sys/class/gpio/export
                    echo 430 > /sys/class/gpio/export
                    BIT4=`cat /sys/class/gpio/gpio427/value`
                    BIT3=`cat /sys/class/gpio/gpio428/value`
                    BIT2=`cat /sys/class/gpio/gpio429/value`
                    BIT1=`cat /sys/class/gpio/gpio430/value`
                    echo "$BIT4 $BIT3 $BIT2 $BIT1"
                    exit
                     */
                    Send_Res_CC(myCC, "echo 427 > /sys/class/gpio/export", "", out res);
                    Send_Res_CC(myCC, "echo 428 > /sys/class/gpio/export", "", out res);
                    Send_Res_CC(myCC, "echo 429 > /sys/class/gpio/export", "", out res);
                    Send_Res_CC(myCC, "echo 430 > /sys/class/gpio/export", "", out res);
                    Send_Res_CC(myCC, "BIT4=`cat /sys/class/gpio/gpio427/value`", "", out res);
                    Send_Res_CC(myCC, "BIT3=`cat /sys/class/gpio/gpio428/value`", "", out res);
                    Send_Res_CC(myCC, "BIT2=`cat /sys/class/gpio/gpio429/value`", "", out res);
                    Send_Res_CC(myCC, "BIT1=`cat /sys/class/gpio/gpio430/value`", "", out res);
                    rs = Send_Res_CC(myCC, "echo \"$BIT4 $BIT3 $BIT2 $BIT1\"", HWID, out res);

                    string hwid = HWID.Replace("/ #", "").Replace("echo \"$BIT4 $BIT3 $BIT2 $BIT1\"", "").Replace("\r\n", "").Replace(" ", "");
                    DisplayMsg(LogType.Log, "HWID SPEC:" + HWID.Replace(" ", "") + " Value:" + hwid);

                    Send_Res_CC(myCC, "exit", "", out res);

                    if (!rs)
                    {
                        AddData("ChkHWID", 1);
                        return;
                    }
                    AddData("ChkHWID", 0);
                    #endregion

                    #region check FW version
                    rs = Send_Res_CC(myCC, "adb shell cat /etc/wnc_version", Int_Ver, out res, 300);
                    Ext_Ver = Func.ReadINI("Setting", "IMQX_Infor", "Ext_Ver", "101.091001027070.001");
                    Int_Ver = Func.ReadINI("Setting", "IMQX_Infor", "Int_Ver", "101.091001027070.001");
                    if (rs && res.Contains(Ext_Ver) && res.Contains(Int_Ver))
                    {
                        AddData("ChkFWVer", 0);
                    }
                    else
                    {
                        AddData("ChkFWVer", 1);
                        return;
                    }
                    #endregion

                    #region write data
                    for (int i = 0; i < 3; i++)
                    {
                        Send_Res_CC(myCC, "adb shell JsonClientFTM /tmp/cgi-2-sys set_user_data_info '{ \\\"serial_number\\\":\\\"" + sn + "\\\" }'", "", out res, 300);
                        Send_Res_CC(myCC, "adb shell JsonClientFTM /tmp/cgi-2-sys set_user_data_info '{ \\\"model_name\\\":\\\"" + Model + "\\\" }'", "", out res, 300);
                        Send_Res_CC(myCC, "adb shell JsonClientFTM /tmp/cgi-2-sys set_user_data_info '{ \\\"model_desc\\\":\\\"" + Description + "\\\" }'", "", out res, 300);
                        Send_Res_CC(myCC, "adb shell JsonClientFTM /tmp/cgi-2-sys set_user_data_info '{ \\\"model_class\\\":\\\"" + ModelClass + "\\\" }'", "", out res, 300);
                        Send_Res_CC(myCC, "adb shell JsonClientFTM /tmp/cgi-2-sys get_user_data_info", "", out res, 1000);
                        if (!res.Contains(sn) && !res.Contains(Model) && !res.Contains(Description) && !res.Contains(ModelClass))
                        {
                            AddData("WriteData", 1);
                            return;
                        }
                    }
                    AddData("WriteData", 0);
                    #endregion
                    #region USB function
                    rs = Send_Res_CC(myCC, "adb shell cat /sys/devices/platform/soc/a600000.ssusb/a600000.dwc3/udc/a600000.dwc3/current_speed", "high-speed", out res);
                    string speed = res.Split(new string[] { "current_speed" }, StringSplitOptions.RemoveEmptyEntries)[1].Replace("\r\n", "").Replace(" ", "");
                    DisplayMsg(LogType.Log, "USBSpeed SPEC:high-speed Value:" + speed);
                    if (!rs)
                    {
                        AddData("CheckUSB", 1);
                        return;
                    }
                    AddData("CheckUSB", 0);
                    #endregion
                    //======================== BYPASS ========================
                    //#region ADC function
                    //rs = Send_Res_CC(myCC, "adb shell cat /sys/class/thermal/thermal_zone29/temp", "", out res, 500);
                    //rs &= CheckADC("PMX_AMUX1", res);
                    //rs &= Send_Res_CC(myCC, "adb shell cat /sys/class/thermal/thermal_zone30/temp", "", out res, 500);
                    //rs &= CheckADC("PMX_AMUX2", res);
                    //rs &= Send_Res_CC(myCC, "adb shell cat /sys/class/thermal/thermal_zone31/temp", "", out res, 500);
                    //rs &= CheckADC("PMX_AMUX3", res);
                    //rs &= Send_Res_CC(myCC, "adb shell cat /sys/class/thermal/thermal_zone32/temp", "", out res, 500);
                    //rs &= CheckADC("PMX_AMUX4", res);
                    //rs &= Send_Res_CC(myCC, "adb shell cat /sys/class/thermal/thermal_zone33/temp", "", out res, 500);
                    //rs &= CheckADC("PMX_AMUX5", res);
                    //rs &= Send_Res_CC(myCC, "adb shell cat /sys/class/thermal/thermal_zone34/temp", "", out res, 500);
                    //rs &= CheckADC("PMX_AMUX6", res);
                    //if (!rs)
                    //{
                    //    AddData("CheckADC", 1);
                    //    return;
                    //}
                    //AddData("CheckADC", 0);
                    //#endregion
                    //======================== BYPASS ========================

                    #region get modem comport
                    #region switch USB compositions
                    //Send_Res_CC(myCC, "adb shell /usr/bin/mfg set usbfunc 3", "", out res, 200);
                    //DisplayMsg(LogType.Log, "wait 2 seconds...");
                    //Thread.Sleep(2000);
                    string modemPort = "";
                    for (int i = 1; i <= 5; i++)
                    {
                        modemPort = getComPort("Modem 90DB");
                        if (modemPort == "")
                        {
                            if (i == 5)
                            {
                                DisplayMsg(LogType.Log, "Get modem port failed, get from INI");
                                modemPort = Func.ReadINI("Setting", "DUT", "ModemPort", "0");
                            }
                            DisplayMsg(LogType.Log, "Get modem port failed, retry");
                            Thread.Sleep(2000);
                            continue;
                        }
                        DisplayMsg(LogType.Log, "Find modem port " + modemPort);
                        break;
                    }
                    #endregion

                    #region check by comport(AT command)
                    SerialPort port = new SerialPort(modemPort, 115200);
                    port.DtrEnable = true;
                    port.RtsEnable = true;
                    port.Open();

                    #region IMEI
                    rs = SerialSendRes(port, "at$wimei=" + IMEI, "OK", out res);
                    rs &= SerialSendRes(port, "at+cgsn", IMEI, out res);

                    if (!rs)
                    {
                        AddData("CheckIMEI", 1);
                        port.Dispose();
                        return;
                    }
                    AddData("CheckIMEI", 0);
                    #endregion

                    //#region qcn
                    // pass before SW fix it
                    rs = SerialSendRes(port, "at$qcn?", QCN_Ver, out res);

                    if (!rs)
                    {
                        AddData("CheckQCN", 1);
                        port.Dispose();
                        return;
                    }
                    AddData("CheckQCN", 0);
                    #endregion

                    //======================== BYPASS ========================
                    //#region SIM
                    //rs = SerialSendRes(port, "at+iccid", "OK", out res);
                    //string str = res.Replace("OK", "").Replace("\r\n", "").Replace("at+iccid", "").Replace("ICCID:", "").Trim();
                    //DisplayMsg(LogType.Log, "ICCID : " + str);
                    //rs &= (str != "") && !res.Contains("ERROR");

                    //rs &= SerialSendRes(port, "at+cimi", "OK", out res);
                    //str = res.Replace("OK", "").Replace("at+cimi", "").Replace("\r\n", "").Trim();
                    //DisplayMsg(LogType.Log, "CIMI : " + str);
                    //rs &= (str != "") && !res.Contains("ERROR");

                    //if (!rs)
                    //{
                    //    AddData("CheckSIM", 1);
                    //    port.Dispose();
                    //    return;
                    //}
                    //AddData("CheckSIM", 0);
                    //port.Dispose();
                    //#endregion
                    //======================== BYPASS ========================

                    #endregion check by comport


                    DisplayMsg(LogType.Log, "End");
                    // ======================== BYPASS ========================
                }
                catch (Exception ex)
                {
                    DisplayMsg(LogType.Exception, ex.ToString());
                    warning = "Exception";
                }
                finally
                {
                    //if (Func.ReadINI("Setting", "IO_Board_Control", "IO_Control_1", "0") == "1")
                    //{
                    //    string txPin = Func.ReadINI("Setting", "IO_Board_Control", "Pin0", "0");
                    //    string rev_message = "";
                    //    status_ATS.AddLog("IO_Board_Y" + txPin + " Off...");
                    //    IO_Board_Control1.ConTrolIOPort_write(Int32.Parse(txPin), "2", ref rev_message);
                    //    DisplayMsg(LogType.Log, rev_message);
                    //}
                    //status_ATS.SFCS_Data.First_Line = sn + "," + status_ATS.txtPSN.Text + "," + BTMac + "," + passWord;
                    //status_ATS.SFCS_Data.First_Line = smt + "," + sn;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        private void bootupIMQX()
        {
            try
            {
                #region reboot device
                if (!CheckGoNoGo())
                {
                    return;
                }
                DisplayMsg(LogType.Log, "reboot device");

                if (Func.ReadINI("Setting", "IO_Board_Control", "IO_Control", "0") == "1")
                {
                    int pin = Convert.ToInt32(Func.ReadINI("Setting", "IO_Board_Control", "Pin", "1"));
                    IO_Board_Control.ConTrolIOPort_write(pin, "2", ref rev_message);
                    DisplayMsg(LogType.Log, rev_message);
                }
                if (use_usb_relay_1 == 1)
                {
                    USBRelay1ON();
                }
                if (Func.ReadINI("Setting", "PowerSupply", "PS3323", "0") == "1")
                {
                    SetPowerSupplyOnOff(Powersupply3323.PSOnOff.OFF);
                }

                DisplayMsg(LogType.Log, "Wait 5 seconds before boot up");
                Thread.Sleep(5000);
                //======================== BYPASS ========================
                if (Func.ReadINI("Setting", "IO_Board_Control", "IO_Control", "0") == "1")
                {
                    int pin = Convert.ToInt32(Func.ReadINI("Setting", "IO_Board_Control", "Pin", "1"));
                    IO_Board_Control.ConTrolIOPort_write(pin, "1", ref rev_message);
                    DisplayMsg(LogType.Log, rev_message);
                }
                else if (use_usb_relay_1 == 1)
                {
                    USBRelay1OFF();
                }
                else if (Func.ReadINI("Setting", "PowerSupply", "PS3323", "0") == "1")
                {
                    SetPowerSupplyOnOff(Powersupply3323.PSOnOff.ON);
                }
                else
                {
                    MessageBox.Show("Manual reboot");
                }
                #endregion
            }
            catch (Exception e)
            {
                DisplayMsg(LogType.Error, e.Message);
                //return false;
            }
        }
        private void DownloadQCN()
        {
            CommandConsole cc = new CommandConsole();
            cc.Start();
            //string perlFile = "swdl_nvrestore.pl";
            string perlFile = "swdl_nvrestore_autostart.pl";
            string qcnPath = Func.ReadINI("Setting", "RestoreQCN", "QCNPath", ".\\");
            string qcnName = Func.ReadINI("Setting", "RestoreQCN", "QCNName", "IMQX_hwid1000_v4_noRFCALNV_PC_defineInLGA_FBRxChar_SRS_r91_v10.01_v2.xqcn");
            //IMQX_hwid1000_v4_noRFCALNV_PC_defineInLGA_FBRxChar_SRS_r91_v10.01_v2.xqcn
            qcnName = "IMQX_hwid1000_v4_noRFCALNV_PC_defineInLGA_FBRxChar_SRS_r91_v10.01_v2.xqcn";
            int timeout = Convert.ToInt32(Func.ReadINI("Setting", "RestoreQCN", "TimeOut", "60"));
            for (int i = 1; i <= 10; i++)
            {
                if (Send_Res_CC_Stop_When_keywords(cc, $"perl {perlFile} " + Path.Combine(qcnPath, qcnName), "Restore completed successfully", new string[] { "phoneStatusNone", "Port not available" }, out res, 100, timeout * 1000))
                {
                    break;
                }
                else
                {
                    if (i == 10)
                    {
                        AddData("QCNCheck", 1);
                        return;
                    }
                    DisplayMsg(LogType.Log, "DUT not ready, wait 10 seconds");
                    Thread.Sleep(5000);
                }
            }
            AddData("QCNCheck", 0);
        }
        private bool SerialSendRes(SerialPort port, string cmd, string keyword, out string res, int delayMs = 200, int timeOutMs = 2000)
        {
            bool rs = false;
            res = "";
            string tmp = "";

            try
            {
                DisplayMsg(LogType.Log, "Send " + cmd);
                port.Write(cmd + (Char)(13));
                Thread.Sleep(delayMs);
                DateTime dt = DateTime.Now;
                while ((DateTime.Now - dt).TotalMilliseconds < timeOutMs)
                {
                    tmp = port.ReadExisting();
                    if (tmp != "")
                    {
                        res += tmp;
                        status_ATS.AddLog(tmp);
                        if (res.Contains(keyword))
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                DisplayMsg(LogType.Error, e.Message);
                return false;
            }

            return rs;
        }
        private bool CheckADC(string sItem, string res)
        {
            string s_Value = res.Split(new string[] { "/temp" }, StringSplitOptions.RemoveEmptyEntries)[1].Replace("\r\n", "").Trim();
            int value = -999;
            if (Int32.TryParse(s_Value, out value))
            {
                DisplayMsg(LogType.Log, "value = " + value);
                status_ATS.AddData(sItem, "", 10000, 60000, value, Err(sItem));
                if (value > 10000 && value < 60000)
                {
                    return true;
                }
            }
            return false;
        }
        private void CombineStation()
        {
            if (!CheckGoNoGo())
                return;

            string smt = status_ATS.txtPSN.Text;
            string sn = status_ATS.txtSP.Text;

            string res = "";
            bool rs = false;

            try
            {
                myCC.Start();

                if (!CheckDeviceIsReady(200))
                {
                    AddData("ChkDevice", 1);
                    return;
                }
                Send_Res_CC(myCC, "adb root", "", out res, 200);
                DisplayMsg(LogType.Log, "Delay 5s...");
                Thread.Sleep(5 * 1000);

                if (!ChkADBDevice("adb shell\r\n", "#", 200 * 1000))
                {
                    AddData("ChkDevice", 1);
                    return;
                }

                #region Write SN
                if (!CheckGoNoGo())
                {
                    return;
                }
                DisplayMsg(LogType.Log, "=================== Write SN ===================");
                if (!Send_Res_CC(myCC, $"wncfota_test --setSerialNo {sn}", "finish update bootinfo", out res, 200, 3000))
                {
                    DisplayMsg(LogType.Log, "Write MAC fail");
                    AddData("WriteSN", 1);
                    return;
                }
                AddData("WriteSN", 0);
                #endregion

                #region Check SN
                if (!CheckGoNoGo())
                {
                    return;
                }
                DisplayMsg(LogType.Log, "=================== Check SN ===================");
                Send_Res_CC(myCC, "wncfota_test -g | grep serialNo", "#", out res, 200, 3000);
                Net.NewNetPort _net = new Net.NewNetPort();
                string DUT_SN = _net.getMiddleString(res, ":[", "]");
                if (DUT_SN != sn)
                {
                    DisplayMsg(LogType.Log, "Check SN fail");
                    AddData("ChkSN", 1);
                    return;
                }
                AddData("ChkSN", 0);
                status_ATS.AddDataRaw("AF62_LABEL_SN", sn, sn, "00000000");
                #endregion
            }
            catch (Exception ex)
            {
                warning = "Exception";
                DisplayMsg(LogType.Exception, ex.ToString());
            }
            finally
            {
                if (!myCC.IsProcessExit())
                {
                    myCC.Close();
                }
            }
        }
        private bool CheckUartPort(int port, int timeoutMs)
        {
            DisplayMsg(LogType.Log, "CheckUartPort");
            DisplayMsg(LogType.Log, "Serial Port: COM" + port.ToString());
            DisplayMsg(LogType.Log, "Timeout: " + timeoutMs.ToString());

            string portName = "COM" + port.ToString();
            DateTime dt = DateTime.Now;
            while (DateTime.Now.Subtract(dt) < System.TimeSpan.FromMilliseconds(timeoutMs))
            {
                Thread.Sleep(500);

                List<string> portList = new List<string>(System.IO.Ports.SerialPort.GetPortNames());

                if (portList.Contains(portName))
                {
                    DisplayMsg(LogType.Log, "Port is Ready.");
                    DisplayMsg(LogType.Log, "Get Port " + port.ToString() + " : " + ((TimeSpan)DateTime.Now.Subtract(dt)).TotalMilliseconds.ToString() + " MS");
                    return true;
                }
            }

            DisplayMsg(LogType.Error, "Can't find Port.");
            return false;
        }
        private bool RunCMDbyProcess(string TestItem, string sCMD, string sPrompt, int iTimeOut, string dirFw)
        {
            if (!CheckGoNoGo())
                return false;
            try
            {
                Directory.SetCurrentDirectory(dirFw);
                string _sReceive = "";
                status_ATS.AddLog("Send to Cmd: " + sCMD);
                iTimeOut = iTimeOut * 5;

                Process P = new Process();
                P.StartInfo.FileName = "cmd";
                P.StartInfo.Arguments = sCMD;
                P.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                P.StartInfo.UseShellExecute = false;
                P.StartInfo.RedirectStandardOutput = true;
                P.StartInfo.RedirectStandardInput = true;
                P.StartInfo.CreateNoWindow = true;
                P.Start();
                P.WaitForExit();

                for (int j = 0; j < iTimeOut; j++)
                {
                    Thread.Sleep(200);
                    _sReceive += P.StandardOutput.ReadToEnd();
                    if (_sReceive.Contains(sPrompt))
                    {
                        DisplayMsg(LogType.Log, "Get From CMD:" + _sReceive);
                        //AddData(TestItem, 0);
                        Directory.SetCurrentDirectory(Application.StartupPath);
                        return true;
                    }
                }
                DisplayMsg(LogType.Log, "Get From CMD:" + _sReceive);
                //AddData(TestItem, 1);
                Directory.SetCurrentDirectory(Application.StartupPath);
                return false;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Log, "Error msg:" + ex.ToString());
                return false;
            }
        }
        private void RunCMDbyProcess1(string TestItem, string sCMD, string sPrompt, int iTimeOut)
        {
            if (!CheckGoNoGo())
                return;
            try
            {
                string path = Func.ReadINI("Setting", "BT", "Path", @"C:\ti\uniflash_8.3.0");
                status_ATS.AddLog("Path: " + path);

                Directory.SetCurrentDirectory(path);
                string _sReceive = "";
                //status_ATS.AddLog("Send to Cmd: " + sCMD);


                Process P = new Process();
                P.StartInfo.WorkingDirectory = path;
                //P.StartInfo.FileName = "dslite.bat";
                P.StartInfo.FileName = "cmd";
                //P.StartInfo.Arguments = sCMD;
                //P.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                P.StartInfo.UseShellExecute = false;
                P.StartInfo.RedirectStandardOutput = true;
                P.StartInfo.RedirectStandardInput = true;
                P.StartInfo.RedirectStandardError = true;
                P.StartInfo.CreateNoWindow = true;
                P.Start();

                status_ATS.AddLog("Send to Cmd: " + sCMD);
                P.StandardInput.WriteLine(sCMD);

                P.StandardInput.Flush();
                P.StandardInput.Close();

                P.WaitForExit();

                status_ATS.AddLog("==");

                _sReceive = "output: " + P.StandardOutput.ReadToEnd();
                status_ATS.AddLog("Get From CMD:" + _sReceive);

                if (_sReceive.Contains(sPrompt))
                {
                    AddData(TestItem, 0);
                    Directory.SetCurrentDirectory(Application.StartupPath);
                    return;
                }

                //for (int j = 0; j < iTimeOut; j++)
                //{
                //    Thread.Sleep(1000);
                //    status_ATS.AddLog("Get From CMD:" + P.StandardOutput.ReadToEnd());
                //    _sReceive += P.StandardOutput.ReadToEnd();
                //    if (_sReceive.Contains(sPrompt))
                //    {                        
                //        AddData(TestItem, 0);
                //        Directory.SetCurrentDirectory(Application.StartupPath);
                //        return;
                //    }
                //}
                //status_ATS.AddLog("Get From CMD:" + _sReceive);

                AddData(TestItem, 1);
                Directory.SetCurrentDirectory(Application.StartupPath);

            }
            catch (Exception ex)
            {
                status_ATS.AddLog("Error msg:" + ex.ToString());
                return;
            }
        }
        private void SendCmd(string Path, string cmd, ref string sReceive)
        {
            if (!CheckGoNoGo())
                return;

            Directory.SetCurrentDirectory(Path);
            string aaa;
            Process myProcess = new Process();
            status_ATS.AddLog("Send to CMD: " + cmd);
            myProcess.StartInfo.FileName = "cmd.exe";

            myProcess.StartInfo.WorkingDirectory = Path;
            myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            myProcess.StartInfo.UseShellExecute = false;
            myProcess.StartInfo.RedirectStandardInput = true;
            myProcess.StartInfo.RedirectStandardOutput = true;
            myProcess.StartInfo.RedirectStandardError = true;
            myProcess.StartInfo.CreateNoWindow = true;
            myProcess.Start();

            myProcess.StandardInput.WriteLine(cmd + "&exit");
            //myProcess.StandardInput.WriteLine(cmd);

            //Thread.Sleep(500);
            myProcess.StandardInput.AutoFlush = true;
            aaa = myProcess.StandardOutput.ReadToEnd();

            myProcess.WaitForExit();
            sReceive = aaa;
        }
        private string getComPort(string keyword)
        {
            DisplayMsg(LogType.Log, $"Try to get comport of \"{keyword}\"");
            ManagementObjectSearcher searcher1 = new ManagementObjectSearcher("root\\cimv2", "SELECT * FROM Win32_PnPEntity");
            string port = "";

            try
            {
                foreach (ManagementObject queryObj1 in searcher1.Get())
                {
                    string caption = Convert.ToString(queryObj1["Caption"]);

                    if (!caption.Contains(keyword))
                        continue;

                    string deviceID = Convert.ToString(queryObj1["PnpDeviceID"]);
                    string regPath = "HKEY_LOCAL_MACHINE\\System\\CurrentControlSet\\Enum\\" + deviceID + "\\Device Parameters";
                    port = Registry.GetValue(regPath, "PortName", "").ToString();

                    DisplayMsg(LogType.Log, "caption" + caption);
                    DisplayMsg(LogType.Log, "Com port: " + port);
                }
                // MessageBox.Show("PAUSE");
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Log, ex.ToString());
            }

            if (port == "")
            {
                DisplayMsg(LogType.Log, "Comport not found!");
            }
            else
            {
                DisplayMsg(LogType.Log, "Fount comport " + port);
            }

            return port;
        }
        private bool UpgradeFW(string directory)
        {
            if (!CheckGoNoGo())
            {
                return false;
            }
            DisplayMsg(LogType.Log, "=================== UpgradeFW ===================");
            try
            {
                string str = string.Empty;
                string logName = "port_trace.txt";
                string batName = "download-all.bat";

                if (File.Exists(directory + "\\" + logName))
                {
                    DisplayMsg(LogType.Log, "Delete " + logName);
                    File.Delete(directory + "\\" + logName);
                }

                Process process = new Process();
                process.StartInfo.WorkingDirectory = directory;
                process.StartInfo.FileName = batName;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                process.Start();
                process.WaitForExit(240 * 1000);
                process.Dispose();
                process.Close();

                if (!File.Exists(directory + "\\" + logName))
                {
                    DisplayMsg(LogType.Log, $"Check {logName} file fail");
                    return false;
                }
                StreamReader Str = new StreamReader(directory + "\\" + logName);
                str = Str.ReadToEnd();
                Str.Close();
                Str.Dispose();

                DisplayMsg(LogType.Log, str);

                if (!str.Contains("All Finished Successfully"))
                {
                    AddData("FWUpgrade", 1);
                    return false;
                }
                AddData("FWUpgrade", 0);
                return true;

                //bool rs = false;
                //string port = getComPort("QDLoader 9008");
                //if (port == "")
                //{
                //    DisplayMsg(LogType.Log, "Find device failed, get from INI");
                //    port = Func.ReadINI("Setting", "DUT", "COM9008", "0");
                //}

                //port = port.ToUpper().Replace("COM", "");

                ////DisplayMsg(LogType.Log, "run proc to upgrade FW:" + Path.Combine(directory, batName));
                //#region Sahara
                //process = new Process();
                //process.StartInfo.WorkingDirectory = directory;
                //process.StartInfo.FileName = "cmd.exe";
                //process.StartInfo.Arguments = "QSaharaServer.exe -p \\\\.\\COM" + port + " -s 13:xbl_s_devprg.melf";
                //process.StartInfo.CreateNoWindow = true;
                //process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                //process.Start();
                //Thread.Sleep(3000);
                //process.WaitForExit(30 * 1000);
                //process.Dispose();
                //process.Close();

                //Thread.Sleep(30000);

                //if (!File.Exists(directory + "\\" + logName))
                //{
                //    DisplayMsg(LogType.Log, $"Check {logName} file fail");
                //    return false;
                //}
                //Str = new StreamReader(directory + "\\" + logName);
                //str = Str.ReadToEnd();
                //Str.Close();
                //Str.Dispose();

                //rs = str.Contains("Sahara protocol completed");
                //#endregion

                //#region firehose
                //if (File.Exists(directory + "\\" + logName))
                //{
                //    DisplayMsg(LogType.Log, "Delete " + logName);
                //    File.Delete(directory + "\\" + logName);
                //}

                //process.StartInfo.WorkingDirectory = directory;
                //process.StartInfo.FileName = "fh_loader.exe";
                //process.StartInfo.Arguments = " --port=\\\\.\\COM" + port + " --sendxml=rawprogram_unsparse0.xml --memoryname=emmc";
                //process.StartInfo.CreateNoWindow = true;
                //process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                //process.Start();
                //process.WaitForExit(1000);
                //process.Dispose();
                //process.Close();

                //Thread.Sleep(30000);

                //if (!File.Exists(directory + "\\" + logName))
                //{
                //    DisplayMsg(LogType.Log, $"Check {logName} file fail");
                //    return false;
                //}
                //Str = new StreamReader(directory + "\\" + logName);
                //str = Str.ReadToEnd();
                //Str.Close();
                //Str.Dispose();

                //DisplayMsg(LogType.Log, str);
                //rs &= str.Contains("All Finished Successfully");
                //#endregion

                //if (rs)
                //{
                //    return true;
                //}
                //else
                //    return false;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                return false;
            }
        }
        private bool EnableSecureBoot()
        {
            if (status_ATS.CheckListData().Count != 0 || warning != string.Empty)
            {
                return false;
            }
            DisplayMsg(LogType.Log, "=================== EnableSecureBoot ===================");
            try
            {
                string str = string.Empty;
                string directory = Func.ReadINI("Setting", "AF62", "EnableSecureBootPath", "ERROR");
                string logName = "port_trace.txt";
                string batName = "download_sec_v2.bat";

                if (File.Exists(directory + "\\" + logName))
                {
                    DisplayMsg(LogType.Log, "Delete " + "\\" + logName);
                    File.Delete(directory + "\\" + logName);
                }

                DisplayMsg(LogType.Log, "run proc to enable secure boot:" + Path.Combine(directory, batName));

                Process process = new Process();
                process.StartInfo.WorkingDirectory = directory;
                process.StartInfo.FileName = batName;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.Start();
                process.WaitForExit(1000);
                process.Dispose();
                process.Close();

                System.Threading.Thread.Sleep(120 * 1000);

                if (!File.Exists(directory + "\\" + logName))
                {
                    DisplayMsg(LogType.Log, $"Check {logName} file fail");
                    return false;
                }

                StreamReader Str = new StreamReader(directory + "\\" + logName);
                str = Str.ReadToEnd();
                Str.Close();
                Str.Dispose();

                DisplayMsg(LogType.Log, str);

                if (str.Contains("All Finished Successfully"))
                {
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                return false;
            }
        }
        private bool CheckLed()
        {
            if (!CheckGoNoGo())
            {
                return false;
            }
            bool rs = false;
            string res = string.Empty;
            try
            {
                #region green led
                DisplayMsg(LogType.Log, "============ GREEN LED CHECK ==================");
                rs = Send_Res_CC(myCC, "echo 1008 > /sys/class/gpio/export;", "#", out res, 200, 3000);
                rs = rs && Send_Res_CC(myCC, "echo out >/sys/class/gpio/gpio1008/direction;", "#", out res, 200, 3000);
                rs = rs && Send_Res_CC(myCC, "echo 1 >/sys/class/gpio/gpio1008/value;", "#", out res, 200, 3000);
                rs = rs && Send_Res_CC(myCC, "echo 1 > /sys/class/leds/GPIO_80_LED3_G/brightness;", "#", out res, 200, 3000);
                rs = rs && Send_Res_CC(myCC, "echo 1 > /sys/class/leds/GPIO_82_LED2_G/brightness;", "#", out res, 200, 3000);
                if (!rs)
                {
                    DisplayMsg(LogType.Log, "Check Green Led On fail");
                    AddData("GreenOn", 1);
                    return false;
                }
                DisplayMsg(LogType.Log, "Is the green led on?");
                if (false && DialogResult.No == MessageBox.Show("Is the green led on?", "Led Check", MessageBoxButtons.YesNo))
                {
                    DisplayMsg(LogType.Log, "Selected No");
                    AddData("GreenOn", 1);
                    return false;
                }
                DisplayMsg(LogType.Log, "Selected Yes");
                AddData("GreenOn", 0);

                rs = rs && Send_Res_CC(myCC, "echo 0 >/sys/class/gpio/gpio1008/value;", "#", out res, 200, 3000);
                rs = rs && Send_Res_CC(myCC, "echo 0 > /sys/class/leds/GPIO_80_LED3_G/brightness;", "#", out res, 200, 3000);
                rs = rs && Send_Res_CC(myCC, "echo 0 > /sys/class/leds/GPIO_82_LED2_G/brightness;", "#", out res, 200, 3000);
                if (!rs)
                {
                    DisplayMsg(LogType.Log, "Check Green Led Off fail");
                    AddData("GreenOff", 1);
                    return false;
                }
                DisplayMsg(LogType.Log, "Is the green led off?");
                if (false && DialogResult.No == MessageBox.Show("Is the green led off?", "Led Check", MessageBoxButtons.YesNo))
                {
                    DisplayMsg(LogType.Log, "Selected No");
                    AddData("GreenOff", 1);
                    return false;
                }
                DisplayMsg(LogType.Log, "Selected Yes");
                AddData("GreenOff", 0);
                #endregion

                #region Red led
                DisplayMsg(LogType.Log, "============ RED LED CHECK ==================");
                rs = Send_Res_CC(myCC, "echo 1012 > /sys/class/gpio/export;", "#", out res, 200, 3000);
                rs = rs && Send_Res_CC(myCC, "echo out >/sys/class/gpio/gpio1012/direction;", "#", out res, 200, 3000);
                rs = rs && Send_Res_CC(myCC, "echo 1 >/sys/class/gpio/gpio1012/value;", "#", out res, 200, 3000);
                rs = rs && Send_Res_CC(myCC, "echo 1 > /sys/class/leds/GPIO_81_LED3_R/brightness;", "#", out res, 200, 3000);
                rs = rs && Send_Res_CC(myCC, "echo 1 > /sys/class/leds/GPIO_83_LED2_R/brightness;", "#", out res, 200, 3000);
                if (!rs)
                {
                    DisplayMsg(LogType.Log, "Check Red Led On fail");
                    AddData("RedOn", 1);
                    return false;
                }
                DisplayMsg(LogType.Log, "Is the red led on?");
                if (false && DialogResult.No == MessageBox.Show("Is the red led on?", "Led Check", MessageBoxButtons.YesNo))
                {
                    DisplayMsg(LogType.Log, "Selected No");
                    AddData("RedOn", 1);
                    return false;
                }
                DisplayMsg(LogType.Log, "Selected Yes");
                AddData("RedOn", 0);

                rs = rs && Send_Res_CC(myCC, "echo 0 >/sys/class/gpio/gpio1012/value;", "#", out res, 200, 3000);
                rs = rs && Send_Res_CC(myCC, "echo 0 > /sys/class/leds/GPIO_81_LED3_R/brightness;", "#", out res, 200, 3000);
                rs = rs && Send_Res_CC(myCC, "echo 0 > /sys/class/leds/GPIO_83_LED2_R/brightness;", "#", out res, 200, 3000);
                if (!rs)
                {
                    DisplayMsg(LogType.Log, "Check Red Led Off fail");
                    AddData("RedOff", 1);
                    return false;
                }
                DisplayMsg(LogType.Log, "Is the red led off?");
                if (false && DialogResult.No == MessageBox.Show("Is the red led off?", "Led Check", MessageBoxButtons.YesNo))
                {
                    DisplayMsg(LogType.Log, "Selected No");
                    AddData("RedOff", 1);
                    return false;
                }
                DisplayMsg(LogType.Log, "Selected Yes");
                AddData("RedOff", 0);
                return true;
                #endregion
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                warning = "exception";
                return false;
            }
        }
        private bool SwitchRelay(CTRL ctrl)
        {
            try
            {
                string status = string.Empty;
                string toolPath = Application.StartupPath;  // + @"\UsbRelay";
                string port = Func.ReadINI("Setting", "Port", "RelayBoardPort", "").Replace("COM", string.Empty);
                string useRelay = Func.ReadINI("Setting", "Port", "RelayBoard", "Disable");
                string cmd = string.Empty;
                string str = string.Empty;
                string keyword = string.Empty;
                int timeOutMs = 5 * 1000;

                if (String.Compare(useRelay, CTRL.Enable.ToString(), true) != 0)
                {
                    return false;
                }

                DisplayMsg(LogType.Log, "Switch Relay " + ctrl.ToString());

                switch (ctrl)
                {
                    case CTRL.ON:
                        //status = "0";
                        //keyword = "Switch OFF";
                        status = "1";
                        keyword = "Switch ON";
                        break;
                    case CTRL.OFF:
                        //status = "1";
                        //keyword = "Switch ON";
                        status = "0";
                        keyword = "Switch OFF";
                        break;
                    default:
                        break;
                }

                KillTaskProcess("UsbRelay");

                Process process = new Process();
                process.StartInfo.FileName = "cmd";
                process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WorkingDirectory = toolPath;
                process.Start();


                cmd = "UsbRelay.exe " + port + " " + status;
                DisplayMsg(LogType.Log, cmd);
                process.StandardInput.WriteLine(cmd + "\r\n");

                DateTime dt;
                TimeSpan ts;
                dt = DateTime.Now;
                while (true)
                {
                    ts = new TimeSpan(DateTime.Now.Ticks - dt.Ticks);
                    if (ts.TotalMilliseconds > timeOutMs)
                    {
                        DisplayMsg(LogType.Error, "timeOutMs");
                        KillTaskProcess("UsbRelay");
                        return false;
                    }

                    str += process.StandardOutput.ReadLine();

                    if (str.Contains(keyword))
                    {
                        DisplayMsg(LogType.Log, str);
                        break;
                    }

                    if (str.Contains("Fail"))
                    {
                        DisplayMsg(LogType.Error, str);
                        break;
                    }

                    System.Threading.Thread.Sleep(100);
                }


                KillTaskProcess("UsbRelay");

                return true;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                KillTaskProcess("UsbRelay");
                return false;
            }
        }
    }
}
