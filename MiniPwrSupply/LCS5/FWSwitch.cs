namespace MiniPwrSupply.LCS5
{
    public partial class frmMain
    {
        public static frmMain FWswitch_SELF = null;
        private Action<string, UInt32> mLogCallback = null;
        private Action<Action, string> CalcTimeConsumption = (doneCallback, title) =>
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Reset();
            sw.Start();
            try
            {
                doneCallback();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{title} >>> " + ex.Message);
                throw ex;
            }
            finally
            {
                sw.Stop();
                string timeSpend = sw.Elapsed.TotalMilliseconds.ToString();
                FWswitch_SELF.Save_LOG_data(LogType.Log, $"-----{title} Spend : " + Convert.ToString((Convert.ToDouble(timeSpend) / 1000)) + @" Sec. -----" + Environment.NewLine);
            }
        };
        public void Save_LOG_data(LogType type, string message)
        {
            try
            {
                if (String.Compare(type.ToString(), LogType.Empty.ToString(), true) == 0)
                {
                    status_ATS.AddLog(message);
                }
                else
                {
                    status_ATS.AddLog("[ " + type.ToString() + " ]  " + message);
                }
                //uint msgType = (uint)type;
                //this.mLogCallback(message, msgType);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{message}__" + ex.Message);
                throw ex;
            }
        }
        public void FWSwitch() //MFGToCustomer
        {
            DeviceInfor infor = new DeviceInfor();
            bool rs = false;
            string keyword = "root@OpenWrt:/#";
            string res = "";
            int retryTime = 0;
            string auth_code = "";
            int rebootPingTime = Convert.ToInt32(Func.ReadINI("Setting", "DelayTime", "RebootPingTime", "30"));
            try
            {
                SFCS_Query _Sfcs_Query = new SFCS_Query();
                DisplayMsg(LogType.Log, "=========== FW Switch ===========");

                //if (status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode)
                //{

                //infor.SerialNumber = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@CHJGP1_SN");
                //DisplayMsg(LogType.Log, "Get SN From SFCS is:" + infor.SerialNumber);

                //infor.BaseMAC = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@MAC");
                //DisplayMsg(LogType.Log, "Get Base MAC From SFCS is:" + infor.BaseMAC);

                //infor.Eth1MAC = MACConvert(infor.BaseMAC, 0);
                //DisplayMsg(LogType.Log, "Eth1MAC convert from BaseMac: " + infor.Eth1MAC);

                //infor.Eth2GMAC = MACConvert(infor.BaseMAC, 2);
                //DisplayMsg(LogType.Log, "Eth2GMAC convert from BaseMac: " + infor.Eth2GMAC);

                //infor.Eth5GMAC = MACConvert(infor.BaseMAC, 3);
                //DisplayMsg(LogType.Log, "Eth5GMAC convert from BaseMac: " + infor.Eth5GMAC);

                //infor.FSAN = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@LCS5_SSID_SN_FSAN");
                //DisplayMsg(LogType.Log, "Get FSAN From SFCS is:" + infor.FSAN);

                //_Sfcs_Query.Get15Data(status_ATS.txtPSN.Text, "LCS5_300_PN", ref infor.PartNumber);                   
                //DisplayMsg(LogType.Log, $"LCS5_300_PN from 15 line data: {infor.PartNumber}");

                //_Sfcs_Query.Get15Data(infor.SerialNumber, "LCS5_100_PN", ref infor.PartNumber_100);
                //DisplayMsg(LogType.Log, $"LCS5_100_PN from 15 line data: {infor.PartNumber_100}");                 

                //infor.GPON = Func.ReadINI(Application.StartupPath, "Setting", "LCS5_Infor", "GPON", "0");
                //DisplayMsg(LogType.Log, "GPON is:" + infor.GPON);

                //infor.FWver = ")(*&^";                                   
                //_Sfcs_Query.Get15Data(status_ATS.txtPSN.Text, "LCS5_MFG_FW_VER", ref infor.FWver);
                //DisplayMsg(LogType.Log, $"LCS5_MFG_FW_VER from 15 line data: {infor.FWver}");


                //_Sfcs_Query.Get15Data(status_ATS.txtPSN.Text, "LCS5_HW_VER", ref infor.HWver);
                //DisplayMsg(LogType.Log, $"LCS5_HW_VER from 15 line data: {infor.HWver}");

                //_Sfcs_Query.Get15Data(infor.SerialNumber, "LCS5_ATH_CODE", ref auth_code);
                //DisplayMsg(LogType.Log, $"LCS5_ATH_CODE from 15 line data: {auth_code}");
                //}

                //if (Func.ReadINI("Setting", "Setting", "IsDebug", "0") == "1" && status_ATS._testMode == StatusUI2.StatusUI.TestMode.EngMode)
                {
                    //for verify
                    string readINIpath = Path.Combine(Application.StartupPath, "");
                    infor.SerialNumber = Func.ReadINI(Application.StartupPath, "Setting", "LCS5_Infor", "SerialNumber", "572203000001");
                    infor.NoLevel300 = Func.ReadINI(Application.StartupPath, "Setting", "LCS5_Infor", "NoLevel300", "3000301302");
                    infor.PartNumber = Func.ReadINI(Application.StartupPath, "Setting", "LCS5_Infor", "PartNumber", "3000301310");
                    infor.PartNumber_100 = Func.ReadINI(Application.StartupPath, "Setting", "LCS5_Infor", "PartNumber_100", "1000590710");
                    // ------
                    infor.BaseMAC = "1C:8B:76:5B:FF:00";
                    infor.Eth1MAC = MACConvert(infor.BaseMAC, 0);
                    DisplayMsg(LogType.Log, "Eth1MAC convert from BaseMac: " + infor.Eth1MAC);
                    infor.Eth2GMAC = MACConvert(infor.BaseMAC, 2);
                    DisplayMsg(LogType.Log, "Eth2GMAC convert from BaseMac: " + infor.Eth2GMAC);
                    infor.Eth5GMAC = MACConvert(infor.BaseMAC, 3);
                    DisplayMsg(LogType.Log, "Eth5GMAC convert from BaseMac: " + infor.Eth5GMAC);
                    // ------
                    infor.FSAN = Func.ReadINI(Application.StartupPath, "Setting", "LCS5_Infor", "FSAN", "CXNK00DBB4C2");
                    infor.MFGDate = Func.ReadINI(Application.StartupPath, "Setting", "LCS5_Infor", "MFGDate", "08/09/2023");
                    infor.CalixFWver = Func.ReadINI(Application.StartupPath, "Setting", "LCS5_Infor", "CalixFWver", "23.4.905.34");
                    infor.ModuleId = "0";
                    infor.HWver = "02";
                    infor.FWver = "v1.1.4";
                    infor.GPON = "0";
                    auth_code = Func.ReadINI("Setting", "LCS5_Infor", "Auth_Code", "!@#$%^");
                    DisplayMsg(LogType.Log, "Authentication code from setting:" + auth_code);
                }

                //if (infor.SerialNumber.Length == 12)
                //{
                //    SetTextBox(status_ATS.txtPSN, infor.SerialNumber);
                //    status_ATS.SFCS_Data.PSN = infor.SerialNumber;
                //    status_ATS.SFCS_Data.First_Line = infor.SerialNumber;
                //}
                //else
                //{
                //    warning = "Get SN from SFCS fail";
                //    return;
                //}

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
                if (Func.ReadINI("Setting", "FW", "Upgrade", "1") == "1")
                {
                    this.switchMFGFW();
                    Thread.Sleep(220 * 1000);
                }
                SwitchRelay(CTRL.OFF);
                Thread.Sleep(5000); //electric container
                SwitchRelay(CTRL.ON);
                DisplayMsg(LogType.Log, "Power on");
                if (Func.ReadINI("Setting", "FW_Switch", "InCustomerFW", "0") == "1")
                {
                    telnet.Dispose();
                    if (!telnet.Ping(telnet.IP, 250 * 1000))
                    {
                        DisplayMsg(LogType.Log, $"Ping {telnet.IP} fail..");
                        return;
                    }
                    Thread.Sleep(15000);
                    DisplayMsg(LogType.Log, "Delay 15000s");
                    goto Login;
                }

                DisplayMsg(LogType.Log, $"delay {delayMFGFW}s before bootup");
                Thread.Sleep(delayMFGFW * 1000);



                //===================================================================================
                if (!ChkInitial(PortType.TELNET, keyword, 250 * 1000))
                {
                    AddData("BootUp", 1);
                    return;
                }
                AddData("BootUp", 0);

                #region Upgrade
                if (Func.ReadINI("Setting", "FW", "Upgrade", "1") == "1")
                {
                    if (false)
                    {
                        #region Appendix 7.1.1 NO Check List files 
                        string tftppath = Path.Combine(Application.StartupPath, "CustomerFW");

                        string squashfsImage = Func.ReadINI("Setting", "FW", "squashfsImage_Path", "c:\\TFTP\\calix-ponsfp_squashfs.img");
                        string squashfsImage_MD5 = Func.ReadINI("Setting", "FW", "squashfsImage_MD5", "*&^%");

                        string csssImage = Func.ReadINI("Setting", "FW", "csssImage_Path", "csss-firmware.img");
                        string csssImage_MD5 = Func.ReadINI("Setting", "FW", "csssImage_MD5", "*&^%");

                        string uImage_calix = Func.ReadINI("Setting", "FW", "uImage_calix_Path", "c:\\TFTP\\openwrt-ipq807x-ipq807x_32-ipq807x-full-fit-uImage_calix.itb");
                        string uImage_calix_MD5 = Func.ReadINI("Setting", "FW", "uImage_calix_MD5", "*&^%");

                        string root_calixImage = Func.ReadINI("Setting", "FW", "root_calixImage_Path", "c:\\TFTP\\openwrt-ipq807x-ipq807x_32-squashfs-root_calix.img");
                        string root_calixImage_MD5 = Func.ReadINI("Setting", "FW", "root_calixImage_MD5", "*&^%");

                        string v2Image = Func.ReadINI("Setting", "FW", "v2Image_Path", "c:\\TFTP\\wifi_fw_squashfs_v2.img");
                        string v2Image_MD5 = Func.ReadINI("Setting", "FW", "v2Image_MD5", "*&^%");
                        //string tftppath = Func.ReadINI("Setting", "TFTP", "Path", "c:\\TFTP\\tftpd32.exe");

                        //string squashfsImage = Func.ReadINI("Setting", "FW", "squashfsImage_Path", "c:\\TFTP\\calix-ponsfp_squashfs.img");
                        //string squashfsImage_MD5 = Func.ReadINI("Setting", "FW", "squashfsImage_MD5", "*&^%");

                        //string csssImage = Func.ReadINI("Setting", "FW", "csssImage_Path", "csss-firmware.img");
                        //string csssImage_MD5 = Func.ReadINI("Setting", "FW", "csssImage_MD5", "*&^%");

                        //string uImage_calix = Func.ReadINI("Setting", "FW", "uImage_calix_Path", "c:\\TFTP\\openwrt-ipq807x-ipq807x_32-ipq807x-full-fit-uImage_calix.itb");
                        //string uImage_calix_MD5 = Func.ReadINI("Setting", "FW", "uImage_calix_MD5", "*&^%");

                        //string root_calixImage = Func.ReadINI("Setting", "FW", "root_calixImage_Path", "c:\\TFTP\\openwrt-ipq807x-ipq807x_32-squashfs-root_calix.img");
                        //string root_calixImage_MD5 = Func.ReadINI("Setting", "FW", "root_calixImage_MD5", "*&^%");

                        //string v2Image = Func.ReadINI("Setting", "FW", "v2Image_Path", "c:\\TFTP\\wifi_fw_squashfs_v2.img");
                        //string v2Image_MD5 = Func.ReadINI("Setting", "FW", "v2Image_MD5", "*&^%");

                        //string[] path = { squashfsImage, csssImage, uImage_calix, root_calixImage, v2Image };

                        //foreach (var item in path)
                        //{
                        //    if (!File.Exists(item))
                        //    {
                        //        DisplayMsg(LogType.Log, $"File {item} not exist");
                        //        warning = "File not exist";
                        //        return;
                        //    }
                        //}

                        //KillTaskProcess(Path.GetFileNameWithoutExtension(tftppath));
                        //DisplayMsg(LogType.Log, "TFTP path:" + tftppath);
                        //if (!CheckToolExist(tftppath))
                        //    if (!OpenTestTool(Path.GetDirectoryName(tftppath), Path.GetFileName(tftppath), "", 3000))
                        //    {
                        //        warning = "Open tftp fail";
                        //        return;
                        //    }
                        this.OpenTftpd32(Path.Combine(Application.StartupPath, "CustomerFW"));

                        #endregion


                        #region Appendix 7.1.2 NO Load calix sub-image >>> Download image in tmp folder and check md5sum value              
                        rs = SendAndChk(PortType.UART, "cd tmp/", "tmp#", out res, 0, 3000);
                        rs = rs && SendAndChk(PortType.UART, $"tftp -g -r {Path.GetFileName(squashfsImage)} 192.168.1.100", "tmp#", out res, 10000, 50000);
                        rs = rs && SendAndChk(PortType.UART, $"md5sum {Path.GetFileName(squashfsImage)}", squashfsImage_MD5, out res, 0, 3000);
                        if (!rs)
                        {
                            AddData("FWSwitch", 1);
                            return;
                        }

                        rs = rs && SendAndChk(PortType.UART, $"tftp -g -r {Path.GetFileName(csssImage)} 192.168.1.100", "tmp#", out res, 10000, 50000);
                        rs = rs && SendAndChk(PortType.UART, $"md5sum {Path.GetFileName(csssImage)}", csssImage_MD5, out res, 0, 3000);
                        if (!rs)
                        {
                            AddData("FWSwitch", 1);
                            return;
                        }

                        rs = rs && SendAndChk(PortType.UART, $"tftp -g -r {Path.GetFileName(uImage_calix)} 192.168.1.100", "tmp#", out res, 10000, 50000);
                        rs = rs && SendAndChk(PortType.UART, $"md5sum {Path.GetFileName(uImage_calix)}", uImage_calix_MD5, out res, 0, 3000);
                        if (!rs)
                        {
                            AddData("FWSwitch", 1);
                            return;
                        }

                        rs = rs && SendAndChk(PortType.UART, $"tftp -g -r {Path.GetFileName(root_calixImage)} 192.168.1.100", "tmp#", out res, 10000, 50000);
                        rs = rs && SendAndChk(PortType.UART, $"md5sum {Path.GetFileName(root_calixImage)}", root_calixImage_MD5, out res, 0, 3000);
                        if (!rs)
                        {
                            AddData("FWSwitch", 1);
                            return;
                        }

                        rs = rs && SendAndChk(PortType.UART, $"tftp -g -r {Path.GetFileName(v2Image)} 192.168.1.100", "tmp#", out res, 10000, 50000);
                        rs = rs && SendAndChk(PortType.UART, $"md5sum {Path.GetFileName(v2Image)}", v2Image_MD5, out res, 0, 3000);
                        if (!rs)
                        {
                            AddData("FWSwitch", 1);
                            return;
                        }

                        if (!SendAndChk(PortType.TELNET, "cd ..", keyword, out res, 0, 3000))
                        {
                            DisplayMsg(LogType.Log, $"Check {keyword} fail");
                            AddData("FWSwitch", 1);
                            return;
                        }
                        #endregion
                    }

                    #region 8.4.1 Check boot up partition
                    if (!SendAndChk(PortType.UART, $"cat /proc/boot_info/bootconfig0/0:HLOS/primaryboot", "0", out res, 0, 3000))
                    {
                        DisplayMsg(LogType.Log, "Check '0' fail");
                        AddData("FWSwitch", 1);
                        return;
                    }
                    if (!SendAndChk(PortType.UART, $"cat /proc/boot_info/bootconfig0/rootfs/primaryboot", "80000000", out res, 3000, 6000))
                    {
                        DisplayMsg(LogType.Log, "Check '80000000' fail");
                        AddData("FWSwitch", 1);
                        return;
                    }
                    if (!SendAndChk(PortType.UART, $"cat /proc/boot_info/bootconfig0/0:WIFIFW/primaryboot", "0", out res, 0, 3000))
                    {
                        DisplayMsg(LogType.Log, "Check '0' fail");
                        AddData("FWSwitch", 1);
                        return;
                    }
                    #endregion

                    if (false)
                    {
                        #region NO Upgrade calix sub-image
                        if (!SendAndChk(PortType.UART, "dd if=/dev/zero of=/dev/mmcblk0p23", "200.0MB", out res, 0, 50000))
                        {
                            DisplayMsg(LogType.Log, "Check '200.0MB' fail");
                            AddData("FWSwitch", 1);
                            return;
                        }
                        if (!SendAndChk(PortType.UART, "dd if=/tmp/calix-ponsfp_squashfs.img of=/dev/mmcblk0p23", "19.3MB", out res, 0, 50000))
                        {
                            DisplayMsg(LogType.Log, "Check '19.3MB' fail");
                            AddData("FWSwitch", 1);
                            return;
                        }
                        if (!SendAndChk(PortType.UART, "dd if=/dev/zero of=/dev/mmcblk0p26", "200.0MB", out res, 0, 50000))
                        {
                            DisplayMsg(LogType.Log, "Check '200.0MB' fail");
                            AddData("FWSwitch", 1);
                            return;
                        }
                        if (!SendAndChk(PortType.UART, "dd if=/tmp/calix-ponsfp_squashfs.img of=/dev/mmcblk0p26", "19.3MB", out res, 0, 50000))
                        {
                            DisplayMsg(LogType.Log, "Check '19.3MB' fail");
                            AddData("FWSwitch", 1);
                            return;
                        }

                        if (!SendAndChk(PortType.UART, "dd if=/dev/zero of=/dev/mmcblk0p18", "4.0MB", out res, 0, 50000))
                        {
                            DisplayMsg(LogType.Log, "Check '4.0MB' fail");
                            AddData("FWSwitch", 1);
                            return;
                        }
                        if (!SendAndChk(PortType.UART, "dd if=/tmp/csss-firmware.img of=/dev/mmcblk0p18", "4.0KB", out res, 0, 50000))
                        {
                            DisplayMsg(LogType.Log, "Check '4.0KB' fail");
                            AddData("FWSwitch", 1);
                            return;
                        }

                        if (!SendAndChk(PortType.UART, "dd if=/dev/zero of=/dev/mmcblk0p20", "12.0MB", out res, 0, 50000))
                        {
                            DisplayMsg(LogType.Log, "Check '12.0MB' fail");
                            AddData("FWSwitch", 1);
                            return;
                        }
                        if (!SendAndChk(PortType.UART, "dd if=/tmp/openwrt-ipq807x-ipq807x_32-ipq807x-full-fit-uImage_calix.itb of=/dev/mmcblk0p20", "4.3MB", out res, 0, 50000))
                        {
                            DisplayMsg(LogType.Log, "Check '4.3MB' fail");
                            AddData("FWSwitch", 1);
                            return;
                        }

                        if (!SendAndChk(PortType.UART, "dd if=/dev/zero of=/dev/mmcblk0p24", "100.0MB", out res, 0, 50000))
                        {
                            DisplayMsg(LogType.Log, "Check '100.0MB' fail");
                            AddData("FWSwitch", 1);
                            return;
                        }
                        if (!SendAndChk(PortType.UART, "dd if=/tmp/openwrt-ipq807x-ipq807x_32-squashfs-root_calix.img of=/dev/mmcblk0p24", "44.5MB", out res, 0, 50000))
                        {
                            DisplayMsg(LogType.Log, "Check '44.5MB' fail");
                            AddData("FWSwitch", 1);
                            return;
                        }

                        if (!SendAndChk(PortType.UART, "dd if=/dev/zero of=/dev/mmcblk0p25", "4.0MB", out res, 0, 50000))
                        {
                            DisplayMsg(LogType.Log, "Check '4.0MB' fail");
                            AddData("FWSwitch", 1);
                            return;
                        }
                        if (!SendAndChk(PortType.UART, "dd if=/tmp/wifi_fw_squashfs_v2.img of=/dev/mmcblk0p25", "3.9MB", out res, 0, 50000))
                        {
                            DisplayMsg(LogType.Log, "Check '3.9MB' fail");
                            AddData("FWSwitch", 1);
                            return;
                        }
                        #endregion
                    }
                }
                #endregion
                #region 8.4.1 Switch Firmware to Calix Firmware
                SendAndChk(PortType.TELNET, "echo 1 > /proc/boot_info/bootconfig0/0:HLOS/primaryboot", keyword, out res, 0, 3000);
                SendAndChk(PortType.TELNET, "echo 0xf8000000 > /proc/boot_info/bootconfig0/rootfs/primaryboot", keyword, out res, 0, 3000);
                SendAndChk(PortType.TELNET, "echo 1 > /proc/boot_info/bootconfig0/0:WIFIFW/primaryboot", keyword, out res, 0, 3000);
                SendAndChk("FWSwitch", PortType.TELNET, "cat /proc/boot_info/bootconfig0/getbinary_bootconfig > /tmp/boot1.bin", keyword, 0, 3000);
                SendAndChk(PortType.TELNET, "dd if=/dev/zero of=/dev/mmcblk0p2 bs=336 count=1", keyword, out res, 0, 3000);
                if (!res.Contains("336 bytes"))
                {
                    DisplayMsg(LogType.Log, "Check '336 bytes' fail");
                    AddData("FWSwitch", 1);
                    return;
                }

                SendAndChk(PortType.TELNET, "dd if=/tmp/boot1.bin of=/dev/mmcblk0p2 bs=336 count=1", keyword, out res, 0, 3000);
                if (!res.Contains("336 bytes"))
                {
                    DisplayMsg(LogType.Log, "Check '336 bytes' fail");
                    AddData("FWSwitch", 1);
                    return;
                }

                SendAndChk(PortType.TELNET, "dd if=/dev/zero of=/dev/mmcblk0p3 bs=336 count=1", keyword, out res, 0, 3000);
                if (!res.Contains("336 bytes"))
                {
                    DisplayMsg(LogType.Log, "Check '336 bytes' fail");
                    AddData("FWSwitch", 1);
                    return;
                }

                SendAndChk(PortType.TELNET, "dd if=/tmp/boot1.bin of=/dev/mmcblk0p3 bs=336 count=1", keyword, out res, 0, 3000);
                if (!res.Contains("336 bytes"))
                {
                    DisplayMsg(LogType.Log, "Check '336 bytes' fail");
                    AddData("FWSwitch", 1);
                    return;
                }
                SendAndChk("FWSwitch", PortType.TELNET, "sync", keyword, 1000, 3000);
                SendCommand(PortType.TELNET, "reboot", 500);
                //SendCommand(PortType.TELNET, "reboot", 0);
                //---------------------------------------------------------------------------
                // =========================================================================
                this.chkRebootStatus(rebootPingTime);
                // =========================================================================
                //---------------------------------------------------------------------------
                //DateTime dt = DateTime.Now;
                //while (true)
                //{
                //    if (dt.AddSeconds(rebootPingTime) < DateTime.Now)
                //    {
                //        warning = "Ping 192.168.1.1 ok in 30s, Reboot fail";
                //        return;
                //    }
                //    if (PromptPing("192.168.1.1", 10000))
                //        continue;
                //    else
                //    {
                //        DisplayMsg(LogType.Log, "Ping 192.168.1.1 fail, reboot ok.");
                //        break;
                //    }
                //}
                //---------------------------------------------------------------------------
                #endregion

                DisplayMsg(LogType.Log, $"Delay {delayCalixFW}s");
                Thread.Sleep(delayCalixFW * 1000);
                //---------------------------------------------------------------------------
                #region Calix FW system
                this.pingDutCustFW(infor, 200 * 1000);
            //if (!PromptPing("192.168.1.1", 250 * 1000))
            //{
            //    warning = "Ping 192.168.1.1 fail";
            //    return;
            //}
            //DisplayMsg(LogType.Log, "Delay 15s..");
            //Thread.Sleep(15000);
            //---------------------------------------------------------------------------

            //#region Check board Infor
            //infor.FSAN = "CXNK015F0D9D"; //naked
            //for (int i = 0; i < 3; i++)
            //{
            //    ExcuteCurlCommand("http://192.168.1.1/board_info.cmd", 3000, out res);
            //    if (res.Contains(infor.FSAN))
            //    {
            //        DisplayMsg(LogType.Log, $"Check '{infor.FSAN}' ok");
            //        break;
            //    }
            //}
            //if (!res.Contains(infor.FSAN))
            //{
            //    DisplayMsg(LogType.Log, $"Check '{infor.FSAN}' fail");
            //    AddData("ChkInfor", 1);
            //    return;
            //}
            //#endregion

            Login:
                string cmd = $"-v -b .\\cookies.txt -c .\\cookies.txt -d \"{auth_code}\" http://192.168.1.1/login.cgi";
                ExecuteCurlCommand2(cmd, 0, out res);
                Thread.Sleep(2000);

                //if (!diagPort.Contains(auth_code))
                //{
                //    DisplayMsg(LogType.Log, $"Check '{auth_code}' fail");
                //    AddData("ChkInfor", 1);
                //    return;
                //}

                cmd = "-v -b .\\cookies.txt -c .\\cookies.txt http://192.168.1.1/en_ssh.cmd";
                ExecuteCurlCommand2(cmd, 0, out res);
                Thread.Sleep(1000);
                if (!diagPort.Contains("Success"))
                {
                    DisplayMsg(LogType.Log, $"Check 'Success' fail");
                    AddData("ChkInfor", 1);
                    return;
                }
                if (!diagPort.Contains("Closing connection 0"))
                {
                    DisplayMsg(LogType.Log, $"'Closing connection 0' fail");
                    AddData("ChkInfor", 1);
                    return;
                }
                AddData("ChkInfor", 0);

                #region SSH access calix
                //string calixPw = "25114c89!5upporT";
                string calixPw = "";

                #region Use plink
                myCC.Start();

                string sshIp = Func.ReadINI("Setting", "SSH", "IP", "192.168.1.1");
                int sshPort = Convert.ToInt16(Func.ReadINI("Setting", "SSH", "Port", "30007"));
                string sshId = Func.ReadINI("Setting", "SSH", "Login_ID", "support");
                if (status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode)
                {
                    _Sfcs_Query.Get15Data(infor.SerialNumber, "LCS5_SUPPORT_PWD", ref calixPw);
                    DisplayMsg(LogType.Log, "Calix pw from sfcs:" + calixPw);
                }
                else
                {
                    calixPw = Func.ReadINI("Setting", "SSH", "Login_PW", "support");
                    DisplayMsg(LogType.Log, "Calix pw from setting:" + calixPw);
                }
                DisplayMsg(LogType.Log, $"Open SSH with {sshId}@{sshIp}");
                DisplayMsg(LogType.Log, "Delay 15s");
                Thread.Sleep(15 * 1000);
                rs = SendCmdAndGetResp(myCC, $"plink.exe -ssh {sshId}@{sshIp} -pw {calixPw} -P {sshPort}", "#", out res, 20000);
                if (res.Contains("(y/n)") || res.Contains("\"y\""))
                {
                    if (SendCmdAndGetResp(myCC, "y", "#", out res, 10000, 1000))
                    {
                        rs = true;//By Syn
                    }
                    //SendCmdAndGetResp(myCC, "y", "#", out res, 10000, 1000);
                    //if (!SendCmdAndGetResp(myCC, "y", "#", out res, 10000, 1000))
                    //{
                    //    myCC.Close();
                    //    Thread.Sleep(1500);
                    //    myCC.Start();
                    //    Thread.Sleep(1500);
                    //    DisplayMsg(LogType.Log, $"Open SSH again with {sshId}@{sshIp}");
                    //    SendCmdAndGetResp(myCC, $"plink.exe -ssh {sshId}@{sshIp} -pw {calixPw} -P {sshPort}", "#", out res, 20000);
                    //    if (res.Contains("(y/n)") || res.Contains("\"y\""))
                    //    {
                    //        SendCmdAndGetResp(myCC, "y", "#", out res, 10000, 1000);
                    //    }
                    //}

                }
                if (!rs)
                {
                    if (retryTime++ < 2)
                    {   // ------------------------------------------------------------------
                        //this.pingDutCustFW(250 * 1000);
                        if (!PromptPing("192.168.1.1 ", 200 * 1000))
                        {
                            warning = "Ping 192.168.1.1 fail";
                            return;
                        }
                        DisplayMsg(LogType.Log, "Delay 15s..");
                        Thread.Sleep(15000);
                        goto Login;
                    }
                    AddData("Login", 1);
                    return;
                }
                #endregion

                #region Remove MFG firmware
                if (!SendCmdAndGetResp(myCC, "cat /proc/boot_info/bootconfig0/0:HLOS/primaryboot", "\n1", out res))
                {
                    DisplayMsg(LogType.Log, "Check '1' fail");
                    AddData("RemoveMFGFW", 1);
                    return;
                }
                if (!SendCmdAndGetResp(myCC, "cat /proc/boot_info/bootconfig0/rootfs/primaryboot", "\nf8000000", out res))
                {
                    DisplayMsg(LogType.Log, "Check 'f8000000' fail");
                    AddData("RemoveMFGFW", 1);
                    return;
                }
                if (!SendCmdAndGetResp(myCC, "cat /proc/boot_info/bootconfig0/0:WIFIFW/primaryboot", "\n1", out res))
                {
                    DisplayMsg(LogType.Log, "Check '1' fail");
                    AddData("RemoveMFGFW", 1);
                    return;
                }

                if (!SendCmdAndGetResp(myCC, "dd if=/dev/zero of=/dev/mmcblk0p19", "#", out res, 50 * 1000))
                {
                    DisplayMsg(LogType.Log, "Check '#' fail");
                    AddData("RemoveMFGFW", 1);
                    return;
                }
                if (!SendCmdAndGetResp(myCC, "dd if=/dev/mmcblk0p20 of=/dev/mmcblk0p19", "#", out res, 50 * 1000))
                {
                    DisplayMsg(LogType.Log, "Check '#' fail");
                    AddData("RemoveMFGFW", 1);
                    return;
                }
                Thread.Sleep(200);

                if (!SendCmdAndGetResp(myCC, "dd if=/dev/zero of=/dev/mmcblk0p21", "#", out res, 50 * 1000))
                {
                    DisplayMsg(LogType.Log, "Check '#' fail");
                    AddData("RemoveMFGFW", 1);
                    return;
                }
                Thread.Sleep(200);
                if (!SendCmdAndGetResp(myCC, "dd if=/dev/mmcblk0p22 of=/dev/mmcblk0p21", "#", out res, 50 * 1000))
                {
                    DisplayMsg(LogType.Log, "Check '#' fail");
                    AddData("RemoveMFGFW", 1);
                    return;
                }
                Thread.Sleep(200);

                if (!SendCmdAndGetResp(myCC, "dd if=/dev/zero of=/dev/mmcblk0p14", "#", out res, 50 * 1000))
                {
                    DisplayMsg(LogType.Log, "Check '#' fail");
                    AddData("RemoveMFGFW", 1);
                    return;
                }
                Thread.Sleep(200);
                if (!SendCmdAndGetResp(myCC, "dd if=/dev/mmcblk0p15 of=/dev/mmcblk0p14", "#", out res, 50 * 1000))
                {
                    DisplayMsg(LogType.Log, "Check '#' fail");
                    AddData("RemoveMFGFW", 1);
                    return;
                }
                AddData("RemoveMFGFW", 0);
                #endregion

                #region Factory reset, power on and check ping               
                Thread.Sleep(200);
                if (!SendCmdAndGetResp(myCC, $"p2-factory-reset", "only erasing files", out res, 30 * 1000))
                    AddData("FactoryReset", 1);

                SendCmdAndGetResp(myCC, "reboot", "", out res, 8000);

                SendCmdAndGetResp(myCC, "reboot", "", out res, 8000);
                // =========================================================================
                this.chkRebootStatus(rebootPingTime);
                // =========================================================================
                //dt = DateTime.Now;
                //while (true)
                //{
                //    if (dt.AddSeconds(rebootPingTime) < DateTime.Now)
                //    {
                //        warning = "Ping 192.168.1.1 ok in 30s, Reboot fail";
                //        return;
                //    }
                //    if (PromptPing("192.168.1.1", 2000))
                //        continue;
                //    else
                //    {
                //        DisplayMsg(LogType.Log, "Ping 192.168.1.1 fail, reboot ok.");
                //        break;
                //    }
                //}

                //if (Func.ReadINI("Setting", "IO_Board_Control", "IO_Control_1", "0") == "1")
                //{
                //    string txPin = Func.ReadINI("Setting", "IO_Board_Control", "Pin0", "0");
                //    string rev_message = "";
                //    status_ATS.AddLog("IO_Board_Y" + txPin + " Off...");
                //    IO_Board_Control1.ConTrolIOPort_write(Int32.Parse(txPin), "2", ref rev_message);
                //    DisplayMsg(LogType.Log, rev_message);
                //    Thread.Sleep(2000);
                //    status_ATS.AddLog("IO_Board_Y" + txPin + " On...");
                //    IO_Board_Control1.ConTrolIOPort_write(Int32.Parse(txPin), "1", ref rev_message);
                //    DisplayMsg(LogType.Log, rev_message);
                //}
                DisplayMsg(LogType.Log, $"Delay {delayCalixFW}s after reboot");
                Thread.Sleep(delayCalixFW * 1000);
                int chkResult = this.chkRebootStatus(100 * 1000) == true ? 0 : 1;
                if (chkResult == 0)
                {
                    DisplayMsg(LogType.Log, $"ping NG More Delay 60s after reboot and try again");
                    Thread.Sleep(60 * 1000);
                    if (!PromptPing("192.168.1.1", 20 * 1000))
                    {
                        chkResult = 1;
                    }
                }
                AddData("FactoryReset", chkResult);
                AddData("FWSwitch", chkResult);
                //if (!PromptPing("192.168.1.1", 250 * 1000))
                //    AddData("FactoryReset", 1);
                //else
                //    AddData("FactoryReset", 0);
                #endregion
                #endregion
                //if (CheckGoNoGo())
                //    AddData("FWSwitch", 0);
                #endregion

            }
            catch (Exception ex)
            {
                warning = "exception";
                DisplayMsg(LogType.Log, ex.ToString());
            }
            finally
            {
                if (!myCC.IsProcessExit())
                    myCC.Close();
                if (Func.ReadINI("Setting", "IO_Board_Control", "IO_Control_1", "0") == "1")
                {
                    string txPin = Func.ReadINI("Setting", "IO_Board_Control", "Pin0", "0");
                    string rev_message = "";
                    status_ATS.AddLog("IO_Board_Y" + txPin + " Off...");
                    IO_Board_Control1.ConTrolIOPort_write(Int32.Parse(txPin), "2", ref rev_message);
                    DisplayMsg(LogType.Log, rev_message);
                }

                //======================stress test =======================================
                //SwitchRelay(CTRL.OFF);
                //Thread.Sleep(5000); //electric container
                //SwitchRelay(CTRL.ON);
                //======================stress test =======================================
            }
        }

        public void CustomerToMFG()
        {
            string res = "";
            DeviceInfor infor = new DeviceInfor();
            try
            {
                myCC.Start();

                if (Func.ReadINI("Setting", "IO_Board_Control", "IO_Control_1", "0") == "1")
                {
                    string txPin = Func.ReadINI("Setting", "IO_Board_Control", "Pin0", "0");
                    string rev_message = "";
                    status_ATS.AddLog("IO_Board_Y" + txPin + " On...");
                    IO_Board_Control1.ConTrolIOPort_write(Int32.Parse(txPin), "1", ref rev_message);
                    DisplayMsg(LogType.Log, rev_message);
                }

                infor.SerialNumber = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@CHJGP1_SN");
                infor.FSAN = _Sfcs_Query.GetFromSfcs(status_ATS.txtPSN.Text, "@LCS3_SSID_SN_FSAN");

                #region Calix FW system
                if (!PingHost("192.168.1.1 ", 150000))
                {
                    warning = "Ping 192.168.1.1 fail";
                    return;
                }
                DisplayMsg(LogType.Log, "Delay 15s..");
                Thread.Sleep(15000);

                #region Check board Infor

                for (int i = 0; i < 10; i++)
                {
                    ExcuteCurlCommand("http://192.168.1.1/board_info.cmd", 3000, out res);
                    if (res.Contains(infor.FSAN))
                    {
                        DisplayMsg(LogType.Log, $"Check '{infor.FSAN}' ok");
                        break;
                    }
                }
                if (!res.Contains(infor.FSAN))
                {
                    DisplayMsg(LogType.Log, $"Check '{infor.FSAN}' fail");
                    AddData("ChkInfor", 1);
                    return;
                }

                //string auth_code = "Username=support&auth=175fc919d9df1a294fb848c568adfa67&nonce=e2c60ab57fc77021d594e9635f9e46c9";
                string auth_code = "";
                _Sfcs_Query.Get15Data(infor.SerialNumber, "LCS3_ATH_CODE", ref auth_code);

                DisplayMsg(LogType.Log, "Authentication code from sfcs:" + auth_code);
                string cmd = $"-v -b .\\cookies.txt -c .\\cookies.txt -d \"{auth_code}\" http://192.168.1.1/login.cgi";
                ExecuteCurlCommand2(cmd, 0, out res);
                Thread.Sleep(2000);
                //if (!diagPort.Contains(auth_code))
                //{
                //    DisplayMsg(LogType.Log, $"Check '{auth_code}' fail");
                //    AddData("ChkInfor", 1);
                //    return;
                //}

                cmd = "-v -b .\\cookies.txt -c .\\cookies.txt http://192.168.1.1/en_ssh.cmd";
                ExecuteCurlCommand2(cmd, 0, out res);
                Thread.Sleep(1000);
                if (!diagPort.Contains("Success") && !diagPort.Contains("Closing connection 0"))
                {
                    DisplayMsg(LogType.Log, $"Check 'Success' and 'Closing connection 0' fail");
                    AddData("ChkInfor", 1);
                    return;
                }

                AddData("ChkInfor", 0);
                #endregion

                #region SSH access calix
                //string calixPw = "25114c89!5upporT";
                string calixPw = "";
                _Sfcs_Query.Get15Data(infor.SerialNumber, "LCS3_SUPPORT_PWD", ref calixPw);
                DisplayMsg(LogType.Log, "Calix pw from sfcs:" + calixPw);

                #region Use plink
                myCC.Start();

                string sshIp = Func.ReadINI("Setting", "SSH", "IP", "192.168.1.1");
                int sshPort = Convert.ToInt16(Func.ReadINI("Setting", "SSH", "Port", "30007"));
                string sshId = Func.ReadINI("Setting", "SSH", "Login_ID", "support");

                DisplayMsg(LogType.Log, $"Open SSH with {sshId}@{sshIp}");
                SendCmdAndGetResp(myCC, $"plink.exe -ssh {sshId}@{sshIp} -pw {calixPw} -P {sshPort}", "#", out res, 8000);
                if (res.Contains("(y/n)") || res.Contains("\"y\""))
                {
                    SendCmdAndGetResp(myCC, "y", "#", out res);
                }

                SendCmdAndGetResp(myCC, "echo 0 > /proc/boot_info/0:APPSBL/primaryboot", "#", out res);
                SendCmdAndGetResp(myCC, "echo 0 > /proc/boot_info/0:HLOS/primaryboot", "#", out res);
                SendCmdAndGetResp(myCC, "echo 0 > /proc/boot_info/rootfs/primaryboot", "#", out res);
                SendCmdAndGetResp(myCC, "echo 0 > /proc/boot_info/0:WIFIFW/primaryboot", "#", out res);
                SendCmdAndGetResp(myCC, "cat /proc/boot_info/getbinary_bootconfig > /tmp/boot0.bin", "#", out res);
                SendCmdAndGetResp(myCC, "dd if=/dev/zero of=/dev/mmcblk0p2 bs=336 count=1", "#", out res);
                SendCmdAndGetResp(myCC, "dd if=/tmp/boot0.bin of=/dev/mmcblk0p2 bs=336 count=1", "#", out res);
                SendCmdAndGetResp(myCC, "dd if=/dev/zero of=/dev/mmcblk0p3 bs=336 count=1", "#", out res);
                SendCmdAndGetResp(myCC, "dd if=/tmp/boot0.bin of=/dev/mmcblk0p3 bs=336 count=1", "#", out res);
                SendCmdAndGetResp(myCC, "sync", "#", out res);
                SendCmdAndGetResp(myCC, "reboot", "#", out res);
                DisplayMsg(LogType.Log, "Delay 15s");
                Thread.Sleep(15000);
                if (!PingHost("192.168.1.1 ", 150000))
                {
                    warning = "Ping 192.168.1.1 fail";
                    return;
                }
                AddData("SwitchToMFG", 0);
                #endregion


                #endregion
            }
            catch (Exception ex)
            {
                warning = "Exception";
                DisplayMsg(LogType.Log, ex.ToString());
            }
            finally
            {
                if (!myCC.IsProcessExit())
                {
                    myCC.Close();
                }
                if (Func.ReadINI("Setting", "IO_Board_Control", "IO_Control_1", "0") == "1")
                {
                    string txPin = Func.ReadINI("Setting", "IO_Board_Control", "Pin0", "0");
                    string rev_message = "";
                    status_ATS.AddLog("IO_Board_Y" + txPin + " Off...");
                    IO_Board_Control1.ConTrolIOPort_write(Int32.Parse(txPin), "2", ref rev_message);
                    DisplayMsg(LogType.Log, rev_message);
                }
            }
        }

        public void UpdateMFGFW() //Upgrade WNC Firmware from Calix Firmware
        {
            string res = "";
            try
            {
                Net.NetPort netport = new Net.NetPort();
                SFCS_Query _Sfcs_Query = new SFCS_Query();

                string tftppath = Func.ReadINI("Setting", "TFTP", "Path", "c:\\TFTP\\tftpd32.exe");

                string uImage = Func.ReadINI("Setting", "FW", "UImage_Path", "c:\\TFTP\\openwrt-ipq807x-ipq807x_32-ipq8074-hkxx-fit-uImage.itb");
                string uImage_MD5 = Func.ReadINI("Setting", "FW", "UImage_MD5", "*&^%");

                string rootImage = Func.ReadINI("Setting", "FW", "RootImage_Path", "c:\\TFTP\\openwrt-ipq807x-ipq807x_32-squashfs-root.img");
                string rootImage_MD5 = Func.ReadINI("Setting", "FW", "RootImage_MD5", "*&^%");

                string v2Image = Func.ReadINI("Setting", "FW", "V2Image_Path", "c:\\TFTP\\wifi_fw_ipq8074_qcn9000_squashfs_v2.img");
                string v2Image_MD5 = Func.ReadINI("Setting", "FW", "V2Image_MD5", "*&^%");

                string uImageCalix = Func.ReadINI("Setting", "FW", "UImageCalix_Path", "c:\\TFTP\\openwrt-ipq807x-ipq807x_32-ipq807x-full-fit-uImage_calix.itb");
                string uImageCalix_MD5 = Func.ReadINI("Setting", "FW", "UImageCalix_MD5", "*&^%");

                string rootImageCalix = Func.ReadINI("Setting", "FW", "RootImageCalix_Path", "c:\\TFTP\\openwrt-ipq807x-ipq807x_32-squashfs-root_calix.img");
                string rootImageCalix_MD5 = Func.ReadINI("Setting", "FW", "RootImageCalix_MD5", "*&^%");

                string v2ImageCalix = Func.ReadINI("Setting", "FW", "V2ImageCalix_Path", "c:\\TFTP\\wifi_fw_squashfs_v2.img");
                string v2ImageCalix_MD5 = Func.ReadINI("Setting", "FW", "V2ImageCalix_MD5", "*&^%");

                string[] path = { uImage, rootImage, v2Image, uImageCalix, rootImageCalix, v2ImageCalix };
                foreach (var item in path)
                {
                    if (!File.Exists(item))
                    {
                        warning = "File not exits";
                        DisplayMsg(LogType.Log, $"File '{item}' not exist");
                        return;
                    }
                }
                KillTaskProcess(Path.GetFileNameWithoutExtension(tftppath));
                DisplayMsg(LogType.Log, "TFTP path:" + tftppath);
                if (!CheckToolExist(tftppath))
                {
                    if (!OpenTestTool(Path.GetDirectoryName(tftppath), Path.GetFileName(tftppath), "", 3000))
                    {
                        warning = "Open tftp fail";
                        return;
                    }
                }

                myCC.Start();

                if (Func.ReadINI("Setting", "IO_Board_Control", "IO_Control_1", "0") == "1")
                {
                    string txPin = Func.ReadINI("Setting", "IO_Board_Control", "Pin0", "0");
                    string rev_message = "";
                    status_ATS.AddLog("IO_Board_Y" + txPin + " On...");
                    IO_Board_Control1.ConTrolIOPort_write(Int32.Parse(txPin), "1", ref rev_message);
                    DisplayMsg(LogType.Log, rev_message);
                }

                if (!PingHost("192.168.1.1 ", 250 * 1000))
                {
                    warning = "Ping 192.168.1.1 fail";
                    return;
                }
                //DisplayMsg(LogType.Log, "Delay 15s..");
                //Thread.Sleep(15000);

                DisplayMsg(LogType.Log, $"delay {delayCalixFW}s before bootup");
                Thread.Sleep(delayCalixFW * 1000);

                #region Check board Infor               
                string auth_code = string.Empty;
                _Sfcs_Query.Get15Data(status_ATS.txtPSN.Text, "LCS5_ATH_CODE", ref auth_code);
                DisplayMsg(LogType.Log, $"LCS5_ATH_CODE from 15 line data: {auth_code}");
                if (auth_code == "")
                {
                    warning = "Get auth_code from sfcs fail";
                    DisplayMsg(LogType.Log, "Get auth_code from sfcs fail");
                    return;
                }

                string cmd = $"-v -b .\\cookies.txt -c .\\cookies.txt -d \"{auth_code}\" http://192.168.1.1/login.cgi";
                ExecuteCurlCommand2(cmd, 0, out res);
                Thread.Sleep(2000);

                cmd = "-v -b .\\cookies.txt -c .\\cookies.txt http://192.168.1.1/en_ssh.cmd";
                ExecuteCurlCommand2(cmd, 0, out res);
                Thread.Sleep(1000);
                if (!diagPort.Contains("Success") && !diagPort.Contains("Closing connection 0"))
                {
                    DisplayMsg(LogType.Log, $"Check 'Success' and 'Closing connection 0' fail");
                    AddData("ChkInfor", 1);
                    return;
                }
                AddData("ChkInfor", 0);
                #endregion

                string sshIp = Func.ReadINI("Setting", "SSH", "IP", "192.168.1.1");
                int sshPort = Convert.ToInt16(Func.ReadINI("Setting", "SSH", "Port", "30007"));
                string sshId = Func.ReadINI("Setting", "SSH", "Login_ID", "support");

                string calixPw = "";
                _Sfcs_Query.Get15Data(status_ATS.txtPSN.Text, "LCS5_SUPPORT_PWD", ref calixPw);
                DisplayMsg(LogType.Log, "Calix pw from sfcs:" + calixPw);

                DisplayMsg(LogType.Log, $"Open SSH with {sshId}@{sshIp}");
                bool rs = SendCmdAndGetResp(myCC, $"plink.exe -ssh {sshId}@{sshIp} -pw {calixPw} -P {sshPort}", "#", out res, 8000);
                if (res.Contains("(y/n)") || res.Contains("\"y\""))
                {
                    rs = SendCmdAndGetResp(myCC, "y", "#", out res);
                }

                if (!rs)
                {
                    DisplayMsg(LogType.Log, "Login fail");
                    AddData("Login", 1);
                    return;
                }

                rs = SendCmdAndGetResp(myCC, "cd tmp/", "tmp #", out res);
                rs &= SendCmdAndGetResp(myCC, $"tftp -g -r {Path.GetFileName(uImage)} 192.168.1.100", "tmp #", out res, 10000, 50 * 1000);
                Thread.Sleep(1000);
                rs &= SendCmdAndGetResp(myCC, $"md5sum {Path.GetFileName(uImage)}", uImage_MD5, out res);
                if (!rs) { AddData("FWUpgrade", 1); return; }

                rs &= SendCmdAndGetResp(myCC, $"tftp -g -r {Path.GetFileName(rootImage)} 192.168.1.100", "tmp #", out res, 10000, 50 * 1000);
                rs &= SendCmdAndGetResp(myCC, $"md5sum {Path.GetFileName(rootImage)}", rootImage_MD5, out res);
                if (!rs) { AddData("FWUpgrade", 1); return; }

                rs &= SendCmdAndGetResp(myCC, $"tftp -g -r {Path.GetFileName(v2Image)} 192.168.1.100", "tmp #", out res, 10000, 50 * 1000);
                Thread.Sleep(1000);
                rs &= SendCmdAndGetResp(myCC, $"md5sum {Path.GetFileName(v2Image)}", v2Image_MD5, out res);
                if (!rs) { AddData("FWUpgrade", 1); return; }

                rs &= SendCmdAndGetResp(myCC, $"tftp -g -r {Path.GetFileName(uImageCalix)} 192.168.1.100", "tmp #", out res, 10000, 50 * 1000);
                Thread.Sleep(1000);
                rs &= SendCmdAndGetResp(myCC, $"md5sum {Path.GetFileName(uImageCalix)}", uImageCalix_MD5, out res);
                if (!rs) { AddData("FWUpgrade", 1); return; }

                rs &= SendCmdAndGetResp(myCC, $"tftp -g -r {Path.GetFileName(rootImageCalix)} 192.168.1.100", "tmp #", out res, 10000, 50 * 1000);
                Thread.Sleep(1000);
                rs &= SendCmdAndGetResp(myCC, $"md5sum {Path.GetFileName(rootImageCalix)}", rootImageCalix_MD5, out res);
                if (!rs) { AddData("FWUpgrade", 1); return; }

                rs &= SendCmdAndGetResp(myCC, $"tftp -g -r {Path.GetFileName(v2ImageCalix)} 192.168.1.100", "tmp #", out res, 10000, 50 * 1000);
                Thread.Sleep(1000);
                rs &= SendCmdAndGetResp(myCC, $"md5sum {Path.GetFileName(v2ImageCalix)}", v2ImageCalix_MD5, out res);
                if (!rs) { AddData("FWUpgrade", 1); return; }

                #region check partion
                if (!SendCmdAndGetResp(myCC, $"cat /proc/boot_info/bootconfig0/0:HLOS/primaryboot", "1", out res))
                {
                    DisplayMsg(LogType.Log, "Check '1' fail");
                    AddData("FWUpgrade", 1);
                    return;
                }
                if (!SendCmdAndGetResp(myCC, $"cat /proc/boot_info/bootconfig0/rootfs/primaryboot", "f8000000", out res))
                {
                    DisplayMsg(LogType.Log, "Check 'f8000000' fail");
                    AddData("FWUpgrade", 1);
                    return;
                }
                if (!SendCmdAndGetResp(myCC, $"cat /proc/boot_info/bootconfig0/0:WIFIFW/primaryboot", "1", out res))
                {
                    DisplayMsg(LogType.Log, "Check '1' fail");
                    AddData("FWUpgrade", 1);
                    return;
                }
                #endregion

                #region eraser and write 
                if (!SendCmdAndGetResp(myCC, $"dd if=/dev/zero of=/dev/mmcblk0p19", "12.0MB", out res, 60000))
                {
                    DisplayMsg(LogType.Log, "Check '12.0MB' fail");
                    AddData("FWUpgrade", 1);
                    return;
                }
                if (!SendCmdAndGetResp(myCC, $"dd if=/tmp/openwrt-ipq50xx-ipq50xx_32-ipq5018-mpxx-fit-uImage.itb of=/dev/mmcblk0p19", "3.8MB", out res, 60000))
                {
                    DisplayMsg(LogType.Log, "Check '3.8MB' fail");
                    AddData("FWUpgrade", 1);
                    return;
                }
                if (!SendCmdAndGetResp(myCC, $"dd if=/dev/zero of=/dev/mmcblk0p21", "124.0MB", out res, 60000))
                {
                    DisplayMsg(LogType.Log, "Check '124.0MB' fail");
                    AddData("FWUpgrade", 1);
                    return;
                }
                if (!SendCmdAndGetResp(myCC, $"dd if=/tmp/openwrt-ipq50xx-ipq50xx_32-squashfs-root.img of=/dev/mmcblk0p21", "19.5MB", out res, 60000))
                {
                    DisplayMsg(LogType.Log, "Check '19.5MB' fail");
                    AddData("FWUpgrade", 1);
                    return;
                }
                if (!SendCmdAndGetResp(myCC, $"dd if=/dev/zero of=/dev/mmcblk0p14", "12.0MB", out res, 60000))
                {
                    DisplayMsg(LogType.Log, "Check '12.0MB' fail");
                    AddData("FWUpgrade", 1);
                    return;
                }
                if (!SendCmdAndGetResp(myCC, $"dd if=/tmp/wifi_fw_ipq5018_qcn9000_qcn6122_squashfs.img of=/dev/mmcblk0p14", "6.0MB", out res, 60000))
                {
                    DisplayMsg(LogType.Log, "Check '6.0MB' fail");
                    AddData("FWUpgrade", 1);
                    return;
                }
                if (!SendCmdAndGetResp(myCC, $"sync", "#", out res, 60000))
                {
                    DisplayMsg(LogType.Log, "Check '#' fail");
                    AddData("FWUpgrade", 1);
                    return;
                }
                #endregion

                #region Switch to partition 1: Calix Firmware
                SendCmdAndGetResp(myCC, $"echo 0 > /proc/boot_info/bootconfig0/0:HLOS/primaryboot", "#", out res);
                SendCmdAndGetResp(myCC, $"echo 0x80000000 > /proc/boot_info/bootconfig0/rootfs/primaryboot", "#", out res);
                SendCmdAndGetResp(myCC, $"echo 0 > /proc/boot_info/bootconfig0/0:WIFIFW/primaryboot", "#", out res);
                SendCmdAndGetResp(myCC, $"cat /proc/boot_info/bootconfig0/getbinary_bootconfig > /tmp/boot1.bin", "#", out res);

                if (!SendCmdAndGetResp(myCC, $"dd if=/dev/zero of=/dev/mmcblk0p2 bs=336 count=1", "336B", out res, 60000))
                {
                    DisplayMsg(LogType.Log, "Check '336B' fail");
                    AddData("FWUpgrade", 1);
                    return;
                }
                if (!SendCmdAndGetResp(myCC, $"dd if=/tmp/boot1.bin of=/dev/mmcblk0p2 bs=336 count=1", "336B", out res, 60000))
                {
                    DisplayMsg(LogType.Log, "Check '336B' fail");
                    AddData("FWUpgrade", 1);
                    return;
                }
                if (!SendCmdAndGetResp(myCC, $"dd if=/dev/zero of=/dev/mmcblk0p3 bs=336 count=1", "336B", out res, 60000))
                {
                    DisplayMsg(LogType.Log, "Check '336B' fail");
                    AddData("FWUpgrade", 1);
                    return;
                }
                if (!SendCmdAndGetResp(myCC, $"dd if=/tmp/boot1.bin of=/dev/mmcblk0p3 bs=336 count=1", "336B", out res, 60000))
                {
                    DisplayMsg(LogType.Log, "Check '336B' fail");
                    AddData("FWUpgrade", 1);
                    return;
                }
                SendCmdAndGetResp(myCC, $"sync", "#", out res, 60000);
                SendCmdAndGetResp(myCC, $"reboot", "", out res, 60000);
                SendCmdAndGetResp(myCC, $"reboot", "", out res, 60000); //reboot twice ???
                #endregion

                DateTime dt = DateTime.Now;
                while (true)
                {
                    if (dt.AddSeconds(60) < DateTime.Now)
                    {
                        warning = "Ping 192.168.1.1 ok in 60s, Reboot fail";
                        return;
                    }
                    if (PingHost("192.168.1.1", 2000))
                        continue;
                    else
                    {
                        DisplayMsg(LogType.Log, "Ping 192.168.1.1 fail, reboot ok.");
                        break;
                    }
                }

                if (!PingHost("192.168.1.1 ", 250 * 1000))
                {
                    warning = "Ping 192.168.1.1 fail";
                    return;
                }
                //DisplayMsg(LogType.Log, "Delay 15s..");
                //Thread.Sleep(15000);

                if (CheckGoNoGo())
                {
                    AddData("FWUpgrade", 0);
                }


                goto End;

                #region Check board Infor
                //string auth_code = "Username=support&auth=175fc919d9df1a294fb848c568adfa67&nonce=e2c60ab57fc77021d594e9635f9e46c9";
                //auth_code = Func.ReadINI("Setting", "SSH", "AuthCode", "_)(*&^%"); ;

                DisplayMsg(LogType.Log, "Authentication code from sfcs:" + auth_code);
                cmd = $"-v -b .\\cookies.txt -c .\\cookies.txt -d \"{auth_code}\" http://192.168.1.1/login.cgi";
                ExecuteCurlCommand2(cmd, 0, out res);
                Thread.Sleep(2000);

                cmd = "-v -b .\\cookies.txt -c .\\cookies.txt http://192.168.1.1/en_ssh.cmd";
                ExecuteCurlCommand2(cmd, 0, out res);
                Thread.Sleep(1000);
                if (!diagPort.Contains("Success") && !diagPort.Contains("Closing connection 0"))
                {
                    DisplayMsg(LogType.Log, $"Check 'Success' and 'Closing connection 0' fail");
                    AddData("ChkInfor", 1);
                    return;
                }

                AddData("ChkInfor", 0);
                #endregion

                DisplayMsg(LogType.Log, $"Open SSH with {sshId}@{sshIp}");
                SendCmdAndGetResp(myCC, $"plink.exe -ssh {sshId}@{sshIp} -pw {calixPw} -P {sshPort}", "#", out res, 8000);
                if (res.Contains("(y/n)") || res.Contains("\"y\""))
                {
                    SendCmdAndGetResp(myCC, "y", "#", out res);
                }

                #region check partion
                if (!SendCmdAndGetResp(myCC, $"cat /proc/boot_info/0:APPSBL/primaryboot", "1", out res))
                {
                    DisplayMsg(LogType.Log, "Check '1' fail");
                    AddData("FWUpgrade", 1);
                    return;
                }
                if (!SendCmdAndGetResp(myCC, $"cat /proc/boot_info/0:HLOS/primaryboot", "1", out res))
                {
                    DisplayMsg(LogType.Log, "Check '1' fail");
                    AddData("FWUpgrade", 1);
                    return;
                }
                SendCmdAndGetResp(myCC, $"cat /proc/boot_info/rootfs/primaryboot", "1", out res);
                if (!res.Contains("f8000000"))
                {
                    DisplayMsg(LogType.Log, "Check '80000000' fail");
                    AddData("FWUpgrade", 1);
                    return;
                }
                SendCmdAndGetResp(myCC, $"cat /proc/boot_info/0:WIFIFW/primaryboot", "#", out res);
                if (!res.Contains("1"))
                {
                    DisplayMsg(LogType.Log, "Check '1' fail");
                    AddData("FWUpgrade", 1);
                    return;
                }
                #endregion

                #region eraser and write 
                if (!SendCmdAndGetResp(myCC, $"cd tmp/", "tmp", out res, 60000))
                {
                    DisplayMsg(LogType.Log, "Check 'tmp' fail");
                    AddData("FWUpgrade", 1);
                    return;
                }

                if (!SendCmdAndGetResp(myCC, $"dd if=/dev/zero of=/dev/mmcblk0p19", "12.0MB", out res, 60000))
                {
                    DisplayMsg(LogType.Log, "Check '12.0MB' fail");
                    AddData("FWUpgrade", 1);
                    return;
                }
                rs &= SendCmdAndGetResp(myCC, $"tftp -g -r {Path.GetFileName(uImage)} 192.168.1.100", "tmp #", out res, 10000, 20000);
                if (!SendCmdAndGetResp(myCC, $"dd if=/tmp/{Path.GetFileName(uImage)} of=/dev/mmcblk0p19", "4.7MB", out res, 60000))
                {
                    DisplayMsg(LogType.Log, "Check '4.7MB' fail");
                    AddData("FWUpgrade", 1);
                    return;
                }
                if (!SendCmdAndGetResp(myCC, $"dd if=/dev/zero of=/dev/mmcblk0p21", "100.0MB", out res, 60000))
                {
                    DisplayMsg(LogType.Log, "Check '100.0MB' fail");
                    AddData("FWUpgrade", 1);
                    return;
                }
                rs &= SendCmdAndGetResp(myCC, $"tftp -g -r {Path.GetFileName(rootImage)} 192.168.1.100", "tmp #", out res, 10000, 40000);
                if (!SendCmdAndGetResp(myCC, $"dd if=/tmp/{Path.GetFileName(rootImage)} of=/dev/mmcblk0p21", "21.0MB", out res, 60000))
                {
                    DisplayMsg(LogType.Log, "Check '21.0MB' fail");
                    AddData("FWUpgrade", 1);
                    return;
                }
                if (!SendCmdAndGetResp(myCC, $"dd if=/dev/zero of=/dev/mmcblk0p22", "4.0MB", out res, 60000))
                {
                    DisplayMsg(LogType.Log, "Check '4.0MB' fail");
                    AddData("FWUpgrade", 1);
                    return;
                }
                rs &= SendCmdAndGetResp(myCC, $"tftp -g -r {Path.GetFileName(v2Image)} 192.168.1.100", "tmp #", out res, 10000, 20000);
                if (!SendCmdAndGetResp(myCC, $"dd if=/tmp/{Path.GetFileName(v2Image)} of=/dev/mmcblk0p22", "3.9MB", out res, 60000))
                {
                    DisplayMsg(LogType.Log, "Check '3.9MB' fail");
                    AddData("FWUpgrade", 1);
                    return;
                }

                if (!SendCmdAndGetResp(myCC, $"sync", "#", out res, 60000))
                {
                    DisplayMsg(LogType.Log, "Check '#' fail");
                    AddData("FWUpgrade", 1);
                    return;
                }
                #endregion

                #region switch fw

                SendCmdAndGetResp(myCC, "echo 0 > /proc/boot_info/0:APPSBL/primaryboot", "#", out res);
                SendCmdAndGetResp(myCC, "echo 0 > /proc/boot_info/0:HLOS/primaryboot", "#", out res);
                SendCmdAndGetResp(myCC, "echo 0 > /proc/boot_info/rootfs/primaryboot", "#", out res);
                SendCmdAndGetResp(myCC, "echo 0 > /proc/boot_info/0:WIFIFW/primaryboot", "#", out res);
                SendCmdAndGetResp(myCC, "cat /proc/boot_info/getbinary_bootconfig > /tmp/boot0.bin", "#", out res);
                SendCmdAndGetResp(myCC, "dd if=/dev/zero of=/dev/mmcblk0p2 bs=336 count=1", "#", out res);
                SendCmdAndGetResp(myCC, "dd if=/tmp/boot0.bin of=/dev/mmcblk0p2 bs=336 count=1", "#", out res);
                SendCmdAndGetResp(myCC, "dd if=/dev/zero of=/dev/mmcblk0p3 bs=336 count=1", "#", out res);
                SendCmdAndGetResp(myCC, "dd if=/tmp/boot0.bin of=/dev/mmcblk0p3 bs=336 count=1", "#", out res);
                SendCmdAndGetResp(myCC, "sync", "#", out res);
                SendCmdAndGetResp(myCC, "reboot", "#", out res);
                dt = DateTime.Now;
                while (dt.AddSeconds(60) > DateTime.Now)
                {
                    if (!PingHost("192.168.1.1 ", 3000))
                    {
                        break;
                    }
                    Thread.Sleep(3000);
                }

                if (!PingHost("192.168.1.1 ", 150000))
                {
                    warning = "Ping 192.168.1.1 fail";
                    return;
                }

                if (!ChkInitial(PortType.TELNET, "#", 200000))
                {
                    AddData("BootUp", 1);
                    return;
                }
                string fw = Func.ReadINI("Setting", "FW", "MFGFW_Ver", "*&^%$");
                if (!SendAndChk("FWSwitch", PortType.TELNET, "cat etc/wnc_ver", fw, 0, 3000))
                { AddData("SwitchToMFG", 1); return; }
                AddData("SwitchToMFG", 0);
            #endregion

            End:
                DisplayMsg(LogType.Log, "End");
            }
            catch (Exception w)
            {
                warning = "Exception";
                DisplayMsg(LogType.Log, w.ToString());
            }
            finally
            {
                if (!myCC.IsProcessExit())
                    myCC.Close();
            }
        }
        public bool PromptPing(string nameOrAddress, int TimeOut)
        {
            DateTime dt = DateTime.Now;
            TimeSpan ts;
            //DisplayMsg(LogType.Log, String.Format("Ping to {0}, time out is {1}s", nameOrAddress, TimeOut / 1000));
            try
            {
                while (true)
                {
                    ts = new TimeSpan(DateTime.Now.Ticks - dt.Ticks);
                    if (ts.TotalMilliseconds > TimeOut)
                    {
                        DisplayMsg(LogType.Error, "timeOut");
                        break;
                    }
                    if (this.PingDUT(nameOrAddress, TimeOut))
                    {
                        return true;
                    }
                }
            }
            catch (System.Net.NetworkInformation.PingException)
            {
                DisplayMsg(LogType.Warning, "Ping Host Error");
                return false;
            }
            return false;
        }
        private void calixFwBackToWNC()
        {
            #region boot loader
            if (!BootLoader(uart))
            {
                AddData("BootLoader", 1);
                return;
            }
            AddData("BootLoader", 0);
            #endregion
            string keyword = "IPQ5018#";
            string res = string.Empty;
            int timeOut = 20 * 1000;
            int delayMs = 15 * 1000;
            DeviceInfor infor = new DeviceInfor();
            try
            {
                this.OpenTftpd32(Path.Combine(Application.StartupPath, "CustomerFW"));
                SendAndChk(PortType.UART, "setenv ipaddr 192.168.1.1", keyword, out res, delayMs, timeOut);
                SendAndChk(PortType.UART, "setenv serverip 192.168.1.100", keyword, out res, delayMs, timeOut);
                // ------------- ping 192.168.1.100 alive
                SendAndChk(PortType.UART, "tftpboot 0x44000000 sbl1_emmc.mbn", "Bytes transferred", out res, delayMs, timeOut);
                SendAndChk(PortType.UART, "flash 0:SBL1", keyword, out res, 0, timeOut);
                SendAndChk(PortType.UART, "tftpboot 0x44000000 bootconfig.bin", "Bytes transferred", out res, delayMs, timeOut);
                SendAndChk(PortType.UART, "flash 0:BOOTCONFIG", keyword, out res, 0, timeOut);
                SendAndChk(PortType.UART, "flash 0:BOOTCONFIG1", keyword, out res, 0, timeOut);
                SendAndChk(PortType.UART, "tftpboot 0x44000000 tz.mbn", "Bytes transferred", out res, delayMs, timeOut);
                SendAndChk(PortType.UART, "flash 0:QSEE", keyword, out res, 0, timeOut);
                SendAndChk(PortType.UART, "flash 0:QSEE_1", keyword, out res, 0, timeOut);
                SendAndChk(PortType.UART, "tftpboot 0x44000000 devcfg.mbn", "Bytes transferred", out res, delayMs, timeOut);
                SendAndChk(PortType.UART, "flash 0:DEVCFG", keyword, out res, 0, timeOut);
                SendAndChk(PortType.UART, "flash 0:DEVCFG_1", keyword, out res, 0, timeOut);
                SendAndChk(PortType.UART, "tftpboot 0x44000000 cdt-AP-MP03.3-C2_2M512M8_DDR3.bin", "Bytes transferred", out res, delayMs, timeOut);
                SendAndChk(PortType.UART, "flash 0:CDT", keyword, out res, 0, timeOut);
                SendAndChk(PortType.UART, "flash 0:CDT_1", keyword, out res, 0, timeOut);
                SendAndChk(PortType.UART, "tftpboot 0x44000000 openwrt-ipq5018-u-boot.mbn", "Bytes transferred", out res, delayMs, timeOut);
                SendAndChk(PortType.UART, "flash 0:APPSBL", keyword, out res, 0, timeOut);
                SendAndChk(PortType.UART, "flash 0:APPSBL_1", keyword, out res, 0, timeOut);
                SendAndChk(PortType.UART, "tftpboot 0x44000000 wifi_fw_ipq5018_qcn9000_qcn6122_squashfs.img", "Bytes transferred", out res, delayMs, timeOut);
                SendAndChk(PortType.UART, "flash 0:WIFIFW", keyword, out res, 0, timeOut);
                SendAndChk(PortType.UART, "tftpboot 0x44000000 wifi_fw_ipq5018_qcn9000_qcn6122_squashfs_calix.img", "Bytes transferred", out res, delayMs, timeOut);
                SendAndChk(PortType.UART, "flash 0:WIFIFW_1", keyword, out res, 0, timeOut);
                SendAndChk(PortType.UART, "tftpboot 0x44000000 openwrt-ipq50xx-ipq50xx_32-ipq5018-mpxx-fit-uImage.itb", "Bytes transferred", out res, delayMs, timeOut);
                SendAndChk(PortType.UART, "flash 0:HLOS", keyword, out res, 0, timeOut);
                SendAndChk(PortType.UART, "tftpboot 0x44000000 openwrt-ipq50xx-ipq50xx_32-ipq5018-full-fit-uImage_calix.itb", "Bytes transferred", out res, delayMs, timeOut);
                SendAndChk(PortType.UART, "flash 0:HLOS_1", keyword, out res, 0, timeOut);
                SendAndChk(PortType.UART, "tftpboot 0x44000000 openwrt-ipq50xx-ipq50xx_32-squashfs-root.img", "Bytes transferred", out res, 30000, timeOut);
                SendAndChk(PortType.UART, "flash rootfs", keyword, out res, 0, timeOut);
                SendAndChk(PortType.UART, "tftpboot 0x44000000 openwrt-ipq50xx-ipq50xx_32-squashfs-root_calix.img", "Bytes transferred", out res, 30000, timeOut);
                SendAndChk(PortType.UART, "flash rootfs_1", keyword, out res, 0, timeOut);
                SendAndChk(PortType.UART, "tftpboot 0x44000000 nvram.bin", "Bytes transferred", out res, delayMs, timeOut);
                SendAndChk(PortType.UART, "flash nvram", keyword, out res, 0, timeOut);
                SendAndChk(PortType.UART, "saveenv", keyword, out res, 0, timeOut);
                SendAndChk(PortType.UART, "reset", "#", out res, 0, timeOut);
                //--------------------------------------------------------------------------------------
                //DisplayMsg(LogType.Log, @"delay after reset");
                //Thread.Sleep(200 * 1000);
                //int chkResult = this.chkRebootStatus(90 * 1000) == true ? 0 : 1;
                //AddData("calixFwBackToWNC", chkResult);
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, "Err___" + ex.Message);
            }
            finally
            {
                KillTaskProcess("tftpd32");
                UartDispose(uart);
            }
        }
        private void OpenTftpd32(string path)
        {
            try
            {
                KillTaskProcess("tftpd32");
                Thread.Sleep(1000);
                Directory.SetCurrentDirectory(path);
                System.Diagnostics.Process.Start(Path.Combine(path, "tftpd32.exe"));
                DisplayMsg(LogType.Log, $"Start {path}\\tftpd32.exe");
                Directory.SetCurrentDirectory(Application.StartupPath);
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
            }
        }
        public bool chkRebootStatus(int rebootPingTime)
        {
            DateTime dt = DateTime.Now;
            bool IsRebootOK = false;
            try
            {
                while (true)
                {
                    if (dt.AddSeconds(rebootPingTime) < DateTime.Now)
                    {
                        warning = "Ping 192.168.1.1 ok in 30s, Reboot fail";
                        return IsRebootOK;
                    }
                    // --------- function cannot work ----------
                    if (!PingHost("192.168.1.1 ", 10000))
                    {
                        DisplayMsg(LogType.Log, "Ping 192.168.1.1 fail, reboot ok.");
                        IsRebootOK = true;
                        break;
                    }
                    // --------- function cannot work ----------
                    if (PromptPing("192.168.1.1", 20 * 1000))
                    {
                        continue;
                    }
                    else
                    {
                        DisplayMsg(LogType.Log, "Ping 192.168.1.1 fail, reboot ok.");
                        IsRebootOK = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, "chkRebootStatus >>> " + ex.Message);
            }
            return IsRebootOK;
        }
        public bool pingDutCustFW(DeviceInfor infor, int timeout)
        {
            bool IsPingOK = false;
            string res = "";
            DateTime dt = DateTime.Now;
            TimeSpan ts;
            try
            {
                do
                {
                    ts = new TimeSpan(DateTime.Now.Ticks - dt.Ticks);
                    if (ts.TotalMilliseconds > timeout)
                    {
                        DisplayMsg(LogType.Error, "timeOut");
                        break;
                    }
                    ExcuteCurlCommand("http://192.168.1.1/board_info.cmd", 3000, out res);
                    if (res.Contains(infor.FSAN))
                    {
                        DisplayMsg(LogType.Log, $"Check '{infor.FSAN}' ok");
                        IsPingOK = true;
                        break;
                    }
                    Thread.Sleep(3000);
                } while (!res.Contains(infor.FSAN));

                //if (!PromptPing("192.168.1.1", timeout))
                //{
                //    warning = "Ping 192.168.1.1 fail";
                //    return false;
                //}
                //IsPingOK = true;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, "pingDutCustFW >>> " + ex.Message);
                return false;
            }
            return IsPingOK;
        }
        public void switchMFGFW() //Upgrade WNC Firmware from Calix Firmware
        {
            string res = "";
            string keyword = "tmp #";
            int delays = 30 * 1000;
            try
            {
                this.OpenTftpd32(Path.Combine(Application.StartupPath, "CustomerFW"));

                DisplayMsg(LogType.Log, "Delay 120s after switchCustomerFW");
                Thread.Sleep(120 * 1000);
                // =========================================================
                this.downloadImageInTmp();
                // =========================================================

                SendAndChk(PortType.UART, "cat /proc/boot_info/bootconfig0/0:HLOS/primaryboot", keyword, out res, delays, delays);
                SendAndChk(PortType.UART, "cat /proc/boot_info/bootconfig0/rootfs/primaryboot", keyword, out res, delays, delays);
                SendAndChk(PortType.UART, "cat /proc/boot_info/bootconfig0/0:WIFIFW/primaryboot", keyword, out res, delays, delays);

                SendAndChk(PortType.UART, "dd if=/dev/zero of=/dev/mmcblk0p19", keyword, out res, delays, delays);
                Thread.Sleep(5000);
                SendAndChk(PortType.UART, "dd if=/tmp/openwrt-ipq50xx-ipq50xx_32-ipq5018-mpxx-fit-uImage.itb of=/dev/mmcblk0p19", keyword, out res, delays, delays);
                Thread.Sleep(5000);
                SendAndChk(PortType.UART, "dd if=/dev/zero of=/dev/mmcblk0p21", keyword, out res, delays, delays);
                Thread.Sleep(5000);
                SendAndChk(PortType.UART, "dd if=/tmp/openwrt-ipq50xx-ipq50xx_32-squashfs-root.img of=/dev/mmcblk0p21", keyword, out res, delays, delays);
                Thread.Sleep(5000);
                SendAndChk(PortType.UART, "dd if=/dev/zero of=/dev/mmcblk0p14", keyword, out res, delays, delays);
                Thread.Sleep(5000);
                SendAndChk(PortType.UART, "dd if=/tmp/wifi_fw_ipq5018_qcn9000_qcn6122_squashfs.img of=/dev/mmcblk0p14", keyword, out res, delays, delays);
                Thread.Sleep(5000);
                SendAndChk(PortType.UART, "sync", keyword, out res, delays, delays);

                SendAndChk(PortType.UART, "echo 0 > /proc/boot_info/bootconfig0/0:HLOS/primaryboot", keyword, out res, delays, delays);
                SendAndChk(PortType.UART, "echo 0x80000000 > /proc/boot_info/bootconfig0/rootfs/primaryboot", keyword, out res, delays, delays);
                SendAndChk(PortType.UART, "echo 0 > /proc/boot_info/bootconfig0/0:WIFIFW/primaryboot", keyword, out res, delays, delays);
                SendAndChk(PortType.UART, "cat /proc/boot_info/bootconfig0/getbinary_bootconfig > /tmp/boot1.bin", keyword, out res, delays, delays);

                SendAndChk(PortType.UART, "dd if=/dev/zero of=/dev/mmcblk0p2 bs=336 count=1", keyword, out res, delays, delays);
                SendAndChk(PortType.UART, "dd if=/tmp/boot1.bin of=/dev/mmcblk0p2 bs=336 count=1", keyword, out res, delays, delays);
                SendAndChk(PortType.UART, "dd if=/dev/zero of=/dev/mmcblk0p3 bs=336 count=1", keyword, out res, delays, delays);
                SendAndChk(PortType.UART, "dd if=/tmp/boot1.bin of=/dev/mmcblk0p3 bs=336 count=1", keyword, out res, delays, delays);
                SendAndChk(PortType.UART, "sync", keyword, out res, delays, delays);
                Thread.Sleep(10 * 1000);
                SendAndChk(PortType.UART, "reboot", keyword, out res, 3000, 5000);
                SendAndChk(PortType.UART, "reboot", keyword, out res, delays, 5000);
                if (CheckGoNoGo())
                {
                    AddData("FWUpgrade", 0);
                }


            }
            catch (Exception w)
            {
                warning = "Exception";
                DisplayMsg(LogType.Log, w.ToString());
            }
            finally
            {
                KillTaskProcess("tftpd32");
            }
        }
        public void Appendix713()
        {
            string keyword = "root@OpenWrt:/#";
            string res = "";
            try
            {
                #region 713 Switch Firmware to Calix Firmware
                SendAndChk(PortType.UART, "echo 0 > /proc/boot_info/bootconfig0/0:HLOS/primaryboot", keyword, out res, 0, 3000);
                SendAndChk(PortType.UART, "echo 0x80000000 > /proc/boot_info/bootconfig0/rootfs/primaryboot", keyword, out res, 0, 3000);
                SendAndChk(PortType.UART, "echo 0 > /proc/boot_info/bootconfig0/0:WIFIFW/primaryboot", keyword, out res, 0, 3000);
                SendAndChk("FWSwitch", PortType.UART, "cat /proc/boot_info/bootconfig0/getbinary_bootconfig > /tmp/boot1.bin", keyword, 0, 3000);
                SendAndChk(PortType.UART, "dd if=/dev/zero of=/dev/mmcblk0p2 bs=336 count=1", keyword, out res, 0, 3000);
                if (!res.Contains("336 bytes"))
                {
                    DisplayMsg(LogType.Log, "Check '336 bytes' fail");
                    AddData("FWSwitch", 1);
                    return;
                }

                SendAndChk(PortType.UART, "dd if=/tmp/boot1.bin of=/dev/mmcblk0p2 bs=336 count=1", keyword, out res, 0, 3000);
                if (!res.Contains("336 bytes"))
                {
                    DisplayMsg(LogType.Log, "Check '336 bytes' fail");
                    AddData("FWSwitch", 1);
                    return;
                }

                SendAndChk(PortType.UART, "dd if=/dev/zero of=/dev/mmcblk0p3 bs=336 count=1", keyword, out res, 0, 3000);
                if (!res.Contains("336 bytes"))
                {
                    DisplayMsg(LogType.Log, "Check '336 bytes' fail");
                    AddData("FWSwitch", 1);
                    return;
                }

                SendAndChk(PortType.UART, "dd if=/tmp/boot1.bin of=/dev/mmcblk0p3 bs=336 count=1", keyword, out res, 0, 3000);
                if (!res.Contains("336 bytes"))
                {
                    DisplayMsg(LogType.Log, "Check '336 bytes' fail");
                    AddData("FWSwitch", 1);
                    return;
                }
                SendAndChk("FWSwitch", PortType.UART, "sync", keyword, 1000, 3000);
                SendCommand(PortType.UART, "reboot", 500);
                #endregion

            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public void downloadImageInTmp()
        {
            bool rs = false;
            string res = string.Empty;
            string keyword = "tmp#";
            int delays = 30 * 1000;
            string auth_code = Func.ReadINI("Setting", "LCS5_Infor", "Auth_Code", "!@#$%^");
            try
            {
                //string tftppath = Path.Combine(Application.StartupPath, "CustomerFW");
                //string cmd = $"-v -b .\\cookies.txt -c .\\cookies.txt -d \"{auth_code}\" http://192.168.1.1/login.cgi";
                //ExecuteCurlCommand2(cmd, 0, out res);
                //Thread.Sleep(2000);
                //cmd = "-v -b .\\cookies.txt -c .\\cookies.txt http://192.168.1.1/en_ssh.cmd";
                //ExecuteCurlCommand2(cmd, 0, out res);
                //Thread.Sleep(1000);
                //if (!diagPort.Contains("Success"))
                //{
                //    DisplayMsg(LogType.Log, $"Check 'Success' fail");
                //    AddData("ChkInfor", 1);
                //    return;
                //}
                //if (!diagPort.Contains("Closing connection 0"))
                //{
                //    DisplayMsg(LogType.Log, $"'Closing connection 0' fail");
                //    AddData("ChkInfor", 1);
                //    return;
                //}
                //AddData("ChkInfor", 0);
                #region Use plink
                SendAndChk(PortType.UART, $"\r\n", "#", out res, 1000, 5000);

                //myCC.Start();

                string sshIp = Func.ReadINI("Setting", "SSH", "IP", "192.168.1.1");
                int sshPort = Convert.ToInt16(Func.ReadINI("Setting", "SSH", "Port", "30007"));
                string sshId = Func.ReadINI("Setting", "SSH", "Login_ID", "support");
                string calixPw = Func.ReadINI("Setting", "SSH", "Login_PW", "support");
                DisplayMsg(LogType.Log, "Calix pw from setting:" + calixPw);
                DisplayMsg(LogType.Log, $"Open SSH with {sshId}@{sshIp}");
                //DisplayMsg(LogType.Log, "Delay 15s");
                //Thread.Sleep(15 * 1000);
                SendAndChk(PortType.UART, $"\r\n", "login", out res, 1000, 5000);
                SendAndChk(PortType.UART, $"{sshId}\r\n", "#", out res, 1000, 5000);
                SendAndChk(PortType.UART, $"{calixPw}\r\n", "#", out res, 1000, 5000);

                string sqfsImage = "openwrt-ipq50xx-ipq50xx_32-ipq5018-mpxx-fit-uImage.itb";
                string squashfsImage = "openwrt-ipq50xx-ipq50xx_32-squashfs-root.img";
                string uImage_calix = "wifi_fw_ipq5018_qcn9000_qcn6122_squashfs.img";
                string root_calixImage = "openwrt-ipq50xx-ipq50xx_32-ipq5018-full-fit-uImage_calix.itb";
                string v2Image = "openwrt-ipq50xx-ipq50xx_32-squashfs-root_calix.img";
                string Image_1 = "wifi_fw_ipq5018_qcn9000_qcn6122_squashfs_calix.img";



                #region Appendix 7.1.2 NO Load calix sub-image >>> Download image in tmp folder and check md5sum value              
                rs = SendAndChk(PortType.UART, "cd tmp/", "tmp #", out res, 10000, 9000);
                SendAndChk(PortType.UART, $"tftp -g -r {Path.GetFileName(sqfsImage)} 192.168.1.100", "tmp #", out res, delays, delays);
                Thread.Sleep(5000);
                SendAndChk(PortType.UART, $"tftp -g -r {Path.GetFileName(squashfsImage)} 192.168.1.100", "tmp #", out res, delays, delays);
                Thread.Sleep(5000);
                SendAndChk(PortType.UART, $"tftp -g -r {Path.GetFileName(uImage_calix)} 192.168.1.100", "tmp #", out res, delays, delays);
                Thread.Sleep(5000);
                SendAndChk(PortType.UART, $"tftp -g -r {Path.GetFileName(root_calixImage)} 192.168.1.100", "tmp #", out res, delays, delays);
                Thread.Sleep(5000);
                SendAndChk(PortType.UART, $"tftp -g -r {Path.GetFileName(v2Image)} 192.168.1.100", "tmp #", out res, delays, delays);
                Thread.Sleep(5000);
                SendAndChk(PortType.UART, $"tftp -g -r {Path.GetFileName(Image_1)} 192.168.1.100", "tmp #", out res, delays, delays);
                Thread.Sleep(30000);
                #endregion
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion
        #endregion

    }
}
