using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using WNC.API;
using System.Text.RegularExpressions;
using System.Collections;

namespace MiniPwrSupply.Singleton
{
    public partial class WiFi
    {
        private const string iperfApp = "iperf3";
        private const string antenna_2G = "ath1";
        private const string antenna_5G = "ath0";
        private const string antenna_6G = "ath2";

        private List<WiFiInformation> wifiInfo;
        private WiFiCollection wifiData = new WiFiCollection(WiFiType.WiFi, RfTest.NONE);
        private List<WiFiCollection> wifiList;

        private void OTA_WiFi()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            wifiList = new List<WiFiCollection>();

            WiFiClear();

            WiFiTest();
        }

        private void WiFiParameter()
        {
            DisplayMsg(LogType.Log, "====== WiFi Parameter ======");

            WiFiInformation parameter;

            wifiInfo = new List<WiFiInformation>();
            parameter = new WiFiInformation(WiFiType.WiFi_2G.ToString());
            wifiInfo.Add(parameter);

            parameter = new WiFiInformation(WiFiType.WiFi_5G.ToString());
            wifiInfo.Add(parameter);

            parameter = new WiFiInformation(WiFiType.WiFi_6G.ToString());
            wifiInfo.Add(parameter);

            foreach (WiFiInformation p in wifiInfo)
            {
                DisplayMsg(LogType.Log, "[ " + p.Name.Replace("_", " ") + " ] Channel : " + p.Channel);
                DisplayMsg(LogType.Log, "[ " + p.Name.Replace("_", " ") + " ] Mode : " + p.Mode);
                DisplayMsg(LogType.Log, "[ " + p.Name.Replace("_", " ") + " ] SSID : " + p.Ssid);
                DisplayMsg(LogType.Log, "[ " + p.Name.Replace("_", " ") + " ] 2G Golden IP : " + p.Golden2G_Ip);
                DisplayMsg(LogType.Log, "[ " + p.Name.Replace("_", " ") + " ] 5G Golden IP : " + p.Golden5G_Ip);
                DisplayMsg(LogType.Log, "[ " + p.Name.Replace("_", " ") + " ] 6G Golden IP : " + p.Golden6G_Ip);
            }
        }
        private WiFiInformation RfType(WiFiType type)
        {
            if (wifiInfo == null)
            {
                return null;
            }

            foreach (WiFiInformation info in wifiInfo)
            {
                if (String.Compare(type.ToString(), info.Name, true) == 0)
                {
                    return info;
                }
            }

            return null;
        }

        private void WiFiClear()
        {
            wifiData.ClearIperf();
            wifiData.ClearRssi();
        }


        private void WiFiDown()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            int delayMs = 0;
            int timeOutMs = 10 * 1000;


            SendAndChk(WiFiType.WiFi.ToString(), PortType.TELNET, "uci set wireless.@wifi-iface[1].ssid=OPEN", "Wrt:/#", delayMs, timeOutMs);
            SendAndChk(WiFiType.WiFi.ToString(), PortType.TELNET, "uci set wireless.@wifi-iface[0].ssid=OPEN", "Wrt:/#", delayMs, timeOutMs);
            SendAndChk(WiFiType.WiFi.ToString(), PortType.TELNET, "uci set wireless.@wifi-iface[2].ssid=OPEN", "Wrt:/#", delayMs, timeOutMs);
            SendAndChk(WiFiType.WiFi.ToString(), PortType.TELNET, "uci set wireless.wifi1.disabled=1", "Wrt:/#", delayMs, timeOutMs);
            SendAndChk(WiFiType.WiFi.ToString(), PortType.TELNET, "uci set wireless.wifi0.disabled=1", "Wrt:/#", delayMs, timeOutMs);
            SendAndChk(WiFiType.WiFi.ToString(), PortType.TELNET, "uci set wireless.wifi2.disabled=1", "Wrt:/#", delayMs, timeOutMs);

            SendAndChk(WiFiType.WiFi.ToString(), PortType.TELNET, "uci commit", "Wrt:/#", delayMs, timeOutMs);
            SendAndChk(WiFiType.WiFi.ToString(), PortType.TELNET, "wifi down", "Wrt:/#", delayMs, timeOutMs);
            SendAndChk(WiFiType.WiFi.ToString(), PortType.TELNET, "wifi", "Wrt:/#", delayMs, timeOutMs);
        }

        private bool WiFiSetting(WiFiType type)
        {
            if (!CheckGoNoGo())
            {
                return false;
            }

            try
            {
                int delayMs = 0;
                int timeOutMs = 10 * 1000;
                WiFiInformation collection = RfType(type);

                switch (type)
                {
                    case WiFiType.WiFi_2G:
                        #region WiFiType.WiFi_2G
                        SendAndChk(collection.Name, PortType.TELNET, "uci set wireless.wifi1.disabled=0", "Wrt:/#", delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.TELNET, "uci set wireless.wifi1.channel=" + collection.Channel, "Wrt:/#", delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.TELNET, "uci set wireless.wifi1.htmode=" + collection.Mode, "Wrt:/#", delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.TELNET, "uci set wireless.@wifi-iface[1].ssid=" + collection.Ssid, "Wrt:/#", delayMs, timeOutMs);
                        break;
                    #endregion
                    case WiFiType.WiFi_5G:
                        #region WiFiType.WiFi_5G
                        SendAndChk(collection.Name, PortType.TELNET, "uci set wireless.wifi0.disabled=0", "Wrt:/#", delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.TELNET, "uci set wireless.wifi0.channel=" + collection.Channel, "Wrt:/#", delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.TELNET, "uci set wireless.wifi0.htmode=" + collection.Mode, "Wrt:/#", delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.TELNET, "uci set wireless.@wifi-iface[0].ssid=" + collection.Ssid, "Wrt:/#", delayMs, timeOutMs);
                        break;
                    #endregion
                    case WiFiType.WiFi_6G:
                        #region WiFiType.WiFi_6G
                        SendAndChk(collection.Name, PortType.TELNET, "uci set wireless.wifi2.disabled=0", "Wrt:/#", delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.TELNET, "uci set wireless.wifi2.channel=" + collection.Channel, "Wrt:/#", delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.TELNET, "uci set wireless.wifi2.htmode=" + collection.Mode, "Wrt:/#", delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.TELNET, "uci set wireless.@wifi-iface[2].ssid=" + collection.Ssid, "Wrt:/#", delayMs, timeOutMs);
                        break;
                    #endregion
                    default:
                        break;
                }
                SendAndChk(collection.Name, PortType.TELNET, "uci commit", "Wrt:/#", delayMs, timeOutMs);
                SendAndChk(collection.Name, PortType.TELNET, "wifi", "Wrt:/#", delayMs, timeOutMs);
                SendAndChk(collection.Name, PortType.TELNET, "sleep 10", "Wrt:/#", delayMs, timeOutMs);

                return false;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                status_ATS.AddDataLog(type.ToString(), NG);
                return false;
            }
        }

        private bool WifiPing(WiFiType type)
        {
            if (!CheckGoNoGo())
            {
                return false;
            }

            string errorItem = "PING_" + type.ToString();
            string ip = string.Empty;
            int delayMs = 0;
            int timeOutMs = 30 * 1000;
            string keyword = "root@OpenWrt:~# \r\n";

            try
            {
                DisplayMsg(LogType.Log, $"========= WiFi Ping {type.ToString()} =========");
                WiFiInformation collection = RfType(type);

                switch (type)
                {
                    case WiFiType.WiFi_2G:
                        #region WiFiType.WiFi_2G
                        ip = collection.Golden2G_Ip;
                        break;
                    #endregion
                    case WiFiType.WiFi_5G:
                        #region WiFiType.WiFi_5G
                        ip = collection.Golden5G_Ip;
                        break;
                    #endregion
                    case WiFiType.WiFi_6G:
                        #region WiFiType.WiFi_6G
                        ip = collection.Golden6G_Ip;
                        break;
                    #endregion
                    default:
                        break;
                }

                for (int Retry_count = 1; Retry_count <= 3; Retry_count++)
                {
                    if (telnet.Ping(ip, timeOutMs))
                    {
                        break;
                    }
                    else if (Retry_count == 3)
                    {
                        AddData(errorItem, 1);
                        return false;
                    }
                    else
                    {
                        DisplayMsg(LogType.Log, type.ToString() + " reset");
                        SendAndChk(collection.Name, PortType.SSH, "wifi down", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "wifi", keyword, delayMs, timeOutMs);
                        Thread.Sleep(5000);
                    }
                }
                AddData(errorItem, 0);
                return true;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData(errorItem, 1);
                return false;
            }
        }

        private bool WiFiPreSetting(WiFiType type)
        {
            if (!CheckGoNoGo())
            {
                return false;
            }
            bool retry = false;
            try
            {
                DisplayMsg(LogType.Log, $"========= WiFi PreSetting {type.ToString()} =========");

            retry:
                int delayMs = 0;
                int timeOutMs = 30 * 1000;
                WiFiInformation collection = RfType(type);
                string keyword = "root@OpenWrt:~# \r\n";

                switch (type)
                {
                    case WiFiType.WiFi_2G: //wifi0
                        #region WiFiType.WiFi_2G
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.wifi0.disabled='0'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.wifi1.disabled='1'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.wifi2.disabled='1'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, $"uci set wireless.wifi0.channel='{collection.Channel}'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.wifi0.hwmode='11beg'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.wifi0.band='1'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.wifi0.htmode='EHT40'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.wifi0.tm_l0='-100 119 0'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.wifi0.tm_l1='117 121 50'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.wifi0.tm_l2='119 123 90'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.wifi0.tm_l3='121 125 100'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.wifi0.country='GB'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.@wifi-iface[0].mode='ap'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, $"uci set wireless.@wifi-iface[0].ssid='{collection.Ssid}'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.@wifi-iface[0].encryption='psk2+ccmp'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.@wifi-iface[0].key='12345678'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.@wifi-iface[0].wds=", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.@wifi-iface[0].disablecoext='1'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.@wifi-iface[0].greenap='0'", keyword, delayMs, timeOutMs);

                        SendAndChk(collection.Name, PortType.SSH, "uci set network.lan.ipaddr=192.168.1.1", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci commit", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "wifi", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "sleep 10", keyword, delayMs, timeOutMs); //TODO:?
                        break;
                    #endregion
                    case WiFiType.WiFi_5G: //wifi2
                        #region WiFiType.WiFi_5G
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.wifi0.disabled='1'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.wifi1.disabled='1'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.wifi2.disabled='0'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, $"uci set wireless.wifi2.channel='{collection.Channel}'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.wifi2.hwmode='11bea'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.wifi2.htmode='EHT160'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.wifi2.tm_l0='-100 119 0'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.wifi2.tm_l1='117 121 50'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.wifi2.tm_l2='119 123 90'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.wifi2.tm_l3='121 125 100'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.wifi2.country='GB'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.@wifi-iface[2].mode='ap'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, $"uci set wireless.@wifi-iface[2].ssid='{collection.Ssid}'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.@wifi-iface[2].encryption='psk2+ccmp'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.@wifi-iface[2].key='12345678'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.@wifi-iface[2].wds=", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.@wifi-iface[2].greenap='0'", keyword, delayMs, timeOutMs);

                        SendAndChk(collection.Name, PortType.SSH, "uci set network.lan.ipaddr=192.168.1.1", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci commit", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "wifi", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "sleep 10", keyword, delayMs, timeOutMs); //TODO:?
                        break;
                    #endregion
                    case WiFiType.WiFi_6G: //wifi1
                        #region WiFiType.WiFi_6G
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.wifi0.disabled='1'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.wifi1.disabled='0'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.wifi2.disabled='1'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, $"uci set wireless.wifi1.channel='{collection.Channel}'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.wifi1.hwmode='11bea'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.wifi1.band='3'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.wifi1.htmode='EHT320'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.wifi1.tm_l0='-100 119 0'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.wifi1.tm_l1='117 121 50'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.wifi1.tm_l2='119 123 90'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.wifi1.tm_l3='121 125 100'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.wifi1.country='GB'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.@wifi-iface[1].mode='ap'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, $"uci set wireless.@wifi-iface[1].ssid='{collection.Ssid}'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.@wifi-iface[1].encryption='ccmp'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.@wifi-iface[1].en_6g_sec_comp='0'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.@wifi-iface[1].extap=", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.@wifi-iface[1].wds='1'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.@wifi-iface[1].sae='1'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.@wifi-iface[1].sae_pwe='2'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.@wifi-iface[1].sae_password='12345678'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.@wifi-iface[1].key='12345678'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.@wifi-iface[1].beacon4Chain='1'", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci set wireless.@wifi-iface[1].greenap='0'", keyword, delayMs, timeOutMs);

                        SendAndChk(collection.Name, PortType.SSH, "uci set network.lan.ipaddr=192.168.1.1", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "uci commit", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "wifi", keyword, delayMs, timeOutMs);
                        SendAndChk(collection.Name, PortType.SSH, "sleep 10", keyword, delayMs, timeOutMs); //TODO:?
                        break;
                    #endregion
                    default:
                        break;
                }

                #region Retry
                if (!CheckGoNoGo() && !retry)
                {
                    retry = true;
                    DisplayMsg(LogType.Log, "Retry Wifi preseting...");
                    RemoveFailedItem();
                    warning = string.Empty;
                    goto retry;
                }
                #endregion

                return true;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                //status_ATS.AddDataLog(type.ToString(), NG);
                AddData("PreSetting_" + type.ToString(), 1);
                return false;
            }
        }

        private void GetPHYRate(WiFiType type, RfTest rf)
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            if (rf == RfTest.Tx)
            {
                DisplayMsg(LogType.Log, "========= " + type.ToString() + " Tx PHY Rate =========");
            }
            else
            {
                DisplayMsg(LogType.Log, "========= " + type.ToString() + " Rx PHY Rate =========");
            }
            string ath = "";

            switch (type)
            {
                case WiFiType.WiFi_2G:
                    #region WiFiType.WiFi_2G
                    ath = "ath1";
                    break;
                #endregion
                case WiFiType.WiFi_5G:
                    #region WiFiType.WiFi_5G
                    ath = "ath0";
                    break;
                #endregion
                case WiFiType.WiFi_6G:
                    #region WiFiType.WiFi_6G
                    ath = "ath2";
                    break;
                #endregion
                default:
                    break;
            }

            SendCommand(PortType.TELNET, "wlanconfig " + ath + " list sta", 200);
            string getMsg = "";
            ChkResponse(PortType.TELNET, ITEM.NONE, "Wrt:/#", out getMsg, 10000);
            string item = type.ToString() + "_" + rf + "_" + "PhyRate";
            try
            {
                string txPHYRate = "";
                string rxPHYRate = "";
                string[] res = getMsg.Split('\n');

                for (int i = 0; i < res.Length; i++)
                {
                    if (res[i].Contains("TXRATE RXRATE"))
                    {
                        string[] dt = res[i + 1].Split('M');//Regex.Split(res[i + 1], @"M");
                        string[] tt = Regex.Split(dt[0], @"\s+");
                        txPHYRate = tt[tt.Length - 1];
                        rxPHYRate = dt[1];
                        break;
                    }
                }
                bool getValueok = false;
                double txPHY = -1;
                double rxPHY = -1;
                if (rf == RfTest.Tx)
                {
                    DisplayMsg(LogType.Log, "Tx Phy Rate: " + txPHYRate);

                    if (Double.TryParse(txPHYRate, out txPHY))
                    {
                        getValueok = true;
                    }

                    if (getValueok)
                    {
                        //status_ATS.AddData(item, txPHY);
                        status_ATS.AddLog(item + " SPEC:-999999~999999 Value:" + txPHY);  //顯示格式還是像AddData  WiFi_2G_Tx_PhyRate SPEC:-999999~999999 Value:573
                    }
                    else
                    {
                        warning = "Get Tx Phy Rate error.";
                    }
                }
                else
                {
                    DisplayMsg(LogType.Log, "Rx Phy Rate: " + rxPHYRate);

                    if (Double.TryParse(rxPHYRate, out rxPHY))
                    {
                        getValueok = true;
                    }

                    if (getValueok)
                    {
                        //status_ATS.AddData(item, rxPHY);
                        status_ATS.AddLog(item + " SPEC:-999999~999999 Value:" + rxPHY); //顯示格式還是像AddData  WiFi_2G_Tx_PhyRate SPEC:-999999~999999 Value:573
                    }
                    else
                    {
                        warning = "Get Rx Phy Rate error.";
                    }
                }
            }
            catch (Exception ex)
            {
                warning = "GetPhyRate error:" + ex.ToString();
            }
        }

        private void WiFiTest()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            int pin1 = Int32.Parse(Func.ReadINI("Setting", "IO_Board_Control", "IO1", "4"));
            int pin2 = Int32.Parse(Func.ReadINI("Setting", "IO_Board_Control", "IO2", "5"));
            int pin3 = Int32.Parse(Func.ReadINI("Setting", "IO_Board_Control", "IO3", "6"));
            int pin4 = Int32.Parse(Func.ReadINI("Setting", "IO_Board_Control", "IO4", "7"));

            #region WiFi_2G
            KillIperfInServer(WiFiType.WiFi_2G);
            KillTaskProcess(iperfApp);
            WiFiPreSetting(WiFiType.WiFi_2G);
            WiFiGolden(WiFiType.WiFi_2G);
            WiFiTest(WiFiType.WiFi_2G, RfTest.Tx, true, 1);
            WiFiTest(WiFiType.WiFi_2G, RfTest.Rx, false, 1);
            #endregion

            #region WiFi_5G
            if (!CheckGoNoGo())
            {
                return;
            }

            KillIperfInServer(WiFiType.WiFi_5G);
            KillTaskProcess(iperfApp);
            WiFiPreSetting(WiFiType.WiFi_5G);
            WiFiGolden(WiFiType.WiFi_5G);

            Thread test5g_tx_1 = new Thread(() =>
            {
                WiFiTest(WiFiType.WiFi_5G, RfTest.Tx, true, 1);
            });
            test5g_tx_1.Start();

            Thread test5g_tx_2 = new Thread(() =>
            {
                WiFiTest(WiFiType.WiFi_5G, RfTest.Tx, true, 2);
            });
            test5g_tx_2.Start();

            if (test5g_tx_1 != null)
                test5g_tx_1.Join();
            if (test5g_tx_2 != null)
                test5g_tx_2.Join();

            KillIperfInServer(WiFiType.WiFi_5G);
            KillTaskProcess(iperfApp);

            Thread test5g_rx_1 = new Thread(() =>
            {
                WiFiTest(WiFiType.WiFi_5G, RfTest.Rx, true, 1);
            });
            test5g_rx_1.Start();

            Thread test5g_rx_2 = new Thread(() =>
            {
                WiFiTest(WiFiType.WiFi_5G, RfTest.Rx, true, 2);
            });
            test5g_rx_2.Start();

            if (test5g_rx_1 != null)
                test5g_rx_1.Join();
            if (test5g_rx_2 != null)
                test5g_rx_2.Join();
            #endregion

            #region WiFi_6G
            if (!CheckGoNoGo())
            {
                return;
            }

            KillIperfInServer(WiFiType.WiFi_6G);
            KillTaskProcess(iperfApp);
            WiFiPreSetting(WiFiType.WiFi_6G);
            WiFiGolden(WiFiType.WiFi_6G);

            Thread test6g_tx_1 = new Thread(() =>
            {
                WiFiTest(WiFiType.WiFi_6G, RfTest.Tx, true, 1);
            });
            test6g_tx_1.Start();

            Thread test6g_tx_2 = new Thread(() =>
            {
                WiFiTest(WiFiType.WiFi_6G, RfTest.Tx, true, 2);
            });
            test6g_tx_2.Start();

            Thread test6g_tx_3 = new Thread(() =>
            {
                WiFiTest(WiFiType.WiFi_6G, RfTest.Tx, true, 3);
            });
            test6g_tx_3.Start();

            Thread test6g_tx_4 = new Thread(() =>
            {
                WiFiTest(WiFiType.WiFi_6G, RfTest.Tx, true, 4);
            });
            test6g_tx_4.Start();

            if (test6g_tx_1 != null)
                test6g_tx_1.Join();
            if (test6g_tx_2 != null)
                test6g_tx_2.Join();
            if (test6g_tx_3 != null)
                test6g_tx_3.Join();
            if (test6g_tx_4 != null)
                test6g_tx_4.Join();

            KillIperfInServer(WiFiType.WiFi_6G);
            KillTaskProcess(iperfApp);

            Thread test6g_rx_1 = new Thread(() =>
            {
                WiFiTest(WiFiType.WiFi_6G, RfTest.Rx, true, 1);
            });
            test6g_rx_1.Start();

            Thread test6g_rx_2 = new Thread(() =>
            {
                WiFiTest(WiFiType.WiFi_6G, RfTest.Rx, true, 2);
            });
            test6g_rx_2.Start();

            Thread test6g_rx_3 = new Thread(() =>
            {
                WiFiTest(WiFiType.WiFi_6G, RfTest.Rx, true, 3);
            });
            test6g_rx_3.Start();

            Thread test6g_rx_4 = new Thread(() =>
            {
                WiFiTest(WiFiType.WiFi_6G, RfTest.Rx, true, 4);
            });
            test6g_rx_4.Start();

            if (test6g_rx_1 != null)
                test6g_rx_1.Join();
            if (test6g_rx_2 != null)
                test6g_rx_2.Join();
            if (test6g_rx_3 != null)
                test6g_rx_3.Join();
            if (test6g_rx_4 != null)
                test6g_rx_4.Join();
            #endregion

            //Rena_20230621,同時打多個iperf需加總結果
            double WiFi_5G_TX_TP_sum = 0;
            double WiFi_5G_RX_TP_sum = 0;
            double WiFi_6G_TX_TP_sum = 0;
            double WiFi_6G_RX_TP_sum = 0;
            int WiFi_5G_TX_TP_cnt = 0;
            int WiFi_5G_RX_TP_cnt = 0;
            int WiFi_6G_TX_TP_cnt = 0;
            int WiFi_6G_RX_TP_cnt = 0;
            ArrayList AllItems = status_ATS.CheckListDataAll();
            foreach (StatusUI2.Data data in AllItems)
            {
                if (data.TestItem.StartsWith("WiFi_5G_TX_TP"))
                {
                    WiFi_5G_TX_TP_cnt++;
                    WiFi_5G_TX_TP_sum += Convert.ToDouble(data.Val);
                }
                if (data.TestItem.StartsWith("WiFi_5G_RX_TP"))
                {
                    WiFi_5G_RX_TP_cnt++;
                    WiFi_5G_RX_TP_sum += Convert.ToDouble(data.Val);
                }
                if (data.TestItem.StartsWith("WiFi_6G_TX_TP"))
                {
                    WiFi_6G_TX_TP_cnt++;
                    WiFi_6G_TX_TP_sum += Convert.ToDouble(data.Val);
                }
                if (data.TestItem.StartsWith("WiFi_6G_RX_TP"))
                {
                    WiFi_6G_RX_TP_cnt++;
                    WiFi_6G_RX_TP_sum += Convert.ToDouble(data.Val);
                }
            }
            DisplayMsg(LogType.Log, $"WiFi_5G_TX_TP_cnt : {WiFi_5G_TX_TP_cnt}");
            DisplayMsg(LogType.Log, $"WiFi_5G_TX_TP_sum : {WiFi_5G_TX_TP_sum}");
            DisplayMsg(LogType.Log, $"WiFi_5G_RX_TP_cnt : {WiFi_5G_RX_TP_cnt}");
            DisplayMsg(LogType.Log, $"WiFi_5G_RX_TP_sum : {WiFi_5G_RX_TP_sum}");
            DisplayMsg(LogType.Log, $"WiFi_6G_TX_TP_cnt : {WiFi_6G_TX_TP_cnt}");
            DisplayMsg(LogType.Log, $"WiFi_6G_TX_TP_sum : {WiFi_6G_TX_TP_sum}");
            DisplayMsg(LogType.Log, $"WiFi_6G_RX_TP_cnt : {WiFi_6G_RX_TP_cnt}");
            DisplayMsg(LogType.Log, $"WiFi_6G_RX_TP_sum : {WiFi_6G_RX_TP_sum}");

            //Rena_20230621, TODO: 設定spec for cnt & sum?
            status_ATS.AddData("WiFi_5G_TX_TP_Sum", "Gbits", WiFi_5G_TX_TP_sum);
            status_ATS.AddData("WiFi_5G_RX_TP_Sum", "Gbits", WiFi_5G_RX_TP_sum);
            status_ATS.AddData("WiFi_6G_TX_TP_Sum", "Gbits", WiFi_6G_TX_TP_sum);
            status_ATS.AddData("WiFi_6G_RX_TP_Sum", "Gbits", WiFi_6G_RX_TP_sum);
            status_ATS.AddData("WiFi_5G_TX_TP_Cnt", "Gbits", WiFi_5G_TX_TP_cnt);
            status_ATS.AddData("WiFi_5G_RX_TP_Cnt", "Gbits", WiFi_5G_RX_TP_cnt);
            status_ATS.AddData("WiFi_6G_TX_TP_Cnt", "Gbits", WiFi_6G_TX_TP_cnt);
            status_ATS.AddData("WiFi_6G_RX_TP_Cnt", "Gbits", WiFi_6G_RX_TP_cnt);
        }

        private string SplitString(string data, string keyword, char char1, char char2)
        {
            string mac = "";
            string[] dt = data.Split(char1);

            for (int i = 0; i < dt.Length; i++)
            {
                //DisplayMsg(LogType.Log, "=================== " + dt[i]);
                if (dt[i].Contains(keyword))
                {
                    mac = dt[i].Split(char2)[1].Trim();
                    break;
                }
            }
            return mac;
        }


        //WiFi 5G需要同時打2個iperf,WiFi 6G需要同時打4個iperf,利用run_no來分辨次數
        private void WiFiTest(WiFiType type, RfTest rfTest, bool init, int run_no)
        {
            try
            {
                if (!CheckGoNoGo())
                {
                    return;
                }

                DisplayMsg(LogType.Log, "========= " + type.ToString().Replace("_", " ") + " (" + rfTest.ToString().ToUpper() + run_no + ") =========");
                System.Threading.Thread rxThread = null;
                Process process;
                string cmd = string.Empty;
                string content = string.Empty;
                string resultFile = string.Empty;
                string fileName = string.Empty;
                string server_msg = string.Empty;
                int timeOutMs = Convert.ToInt32(Func.ReadINI("Setting", "Iperf", "TxTimeOutMs", "0"));

                wifiData = new WiFiCollection(type, rfTest);

                switch (type)
                {
                    case WiFiType.WiFi_2G:
                        #region WiFiType.WiFi_2G
                        resultFile = rfTest.ToString() + "_2G" + ".ini";
                        cmd = Func.ReadINI("Setting", "Iperf", rfTest.ToString() + "_2G", string.Empty);
                        server_msg = rfTest.ToString() + "_2G";
                        break;
                    #endregion
                    case WiFiType.WiFi_5G:
                        #region WiFiType.WiFi_5G
                        resultFile = $"{rfTest.ToString()}_5G_{run_no}.ini";
                        cmd = Func.ReadINI("Setting", "Iperf", $"{rfTest.ToString()}_5G_{run_no}", string.Empty);
                        server_msg = $"{rfTest.ToString()}_5G_{run_no}";
                        break;
                    #endregion
                    case WiFiType.WiFi_6G:
                        #region WiFiType.WiFi_6G
                        resultFile = $"{rfTest.ToString()}_6G_{run_no}.ini";
                        cmd = Func.ReadINI("Setting", "Iperf", $"{rfTest.ToString()}_6G_{run_no}", string.Empty);
                        server_msg = $"{rfTest.ToString()}_6G_{run_no}";
                        break;
                    #endregion
                    default:
                        break;
                }

                fileName = $"{type.ToString()}_{rfTest.ToString()}_{run_no}.bat";
                CreateBatchFile(fileName, cmd, resultFile);
                DisplayMsg(LogType.Log, $"{rfTest.ToString()}_{run_no} command : {cmd}");

                if (!CheckGoNoGo())
                {
                    return;
                }

                if (!SendToServer(type, server_msg, CHK.OK.ToString(), 10 * 1000))//ask server to restart iperf
                {
                    AddData(type.ToString() + "_" + rfTest.ToString().ToUpper(), 1);
                    return;
                }
                if (String.Compare(rfTest.ToString(), RfTest.Rx.ToString(), true) == 0)//Add By Leo 20210715
                {
                    DisplayMsg(LogType.Log, "Delay 2s...");
                    System.Threading.Thread.Sleep(2000);
                }

                Directory.SetCurrentDirectory(Application.StartupPath);
                DisplayMsg(LogType.Log, "Excute File Name : " + Application.StartupPath + "\\" + fileName);
                process = new Process();
                process.StartInfo.FileName = Application.StartupPath + "\\" + fileName;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.Start();

                if (String.Compare(rfTest.ToString(), RfTest.Rx.ToString(), true) == 0)
                {
                    timeOutMs = Convert.ToInt32(Func.ReadINI("Setting", "Iperf", "RxTimeOutMs", "0"));
                    //Thread.Sleep(3000);
                    //Rena_20230627, 同時跑多個iperf時只有第一個run要get rssi
                    if (run_no == 1)
                    {
                        rxThread = new Thread(new ThreadStart(WiFiRx));
                        rxThread.Start();
                    }
                }

                #region Get current
                //Thread getcurr = new Thread(() =>
                //{
                //    if (Func.ReadINI("Setting", "PowerSupply", "PowerSupply", "0") == "1")
                //    {
                //        double curr = -1;
                //        CheckCurrent(Convert.ToInt16(Func.ReadINI("Setting", "PowerSupply", "CH1_Channel", "1")), ref curr);
                //        AddData($"{type}_{rfTest}_Curr", curr);
                //        if (usePS_3323)
                //        {
                //            float vol = _ps3323.Get_Vol(1);
                //            DisplayMsg(LogType.Log, "Vol:" + vol);
                //            AddData($"{type}_{rfTest}_Vol", vol);
                //        }
                //    }

                //});
                //getcurr.Start();
                #endregion

                ChkProcess(ref process, resultFile, timeOutMs);

                wifiData.SetIperfFlag(false);

                if (!ChkIperfResult(resultFile, ref content))
                {
                    AddData(type.ToString() + "_" + rfTest.ToString().ToUpper(), 1);
                    goto Exit;
                }

                FetchIperfResult(ref wifiData, content, type);

                if (type == WiFiType.WiFi_2G)
                {
                    status_ATS.AddData(type.ToString() + "_" + rfTest.ToString().ToUpper() + "_TP", "Mbits", wifiData.FetchIperfResult());   // Mbits
                }
                else
                {
                    status_ATS.AddData($"{type.ToString()}_{rfTest.ToString().ToUpper()}_TP_{run_no}", "Gbits", wifiData.FetchIperfResult());   // Gbits
                }

            //GetPHYRate(type, rfTest);

            Exit:

                wifiList.Add(wifiData);

                if (rxThread != null && rxThread.IsAlive)
                {
                    DisplayMsg(LogType.Warning, "Abort rx thread.");
                    rxThread.Join();
                    rxThread.Abort();
                }
                //if (getcurr != null && getcurr.IsAlive)
                //{
                //    DisplayMsg(LogType.Warning, "Abort get current thread.");
                //    getcurr.Join();
                //    getcurr.Abort();
                //}
                Directory.SetCurrentDirectory(Application.StartupPath);
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData(type.ToString() + "_" + rfTest.ToString().ToUpper(), 1);
            }
        }
        private void WiFiTest_Org_20230621(WiFiType type, RfTest rfTest, bool init)
        {
            try
            {
                if (!CheckGoNoGo())
                {
                    return;
                }

                DisplayMsg(LogType.Log, "========= " + type.ToString().Replace("_", " ") + " (" + rfTest.ToString().ToUpper() + ") =========");
                System.Threading.Thread rxThread = null;
                Process process;
                string cmd = string.Empty;
                string content = string.Empty;
                string resultFile = string.Empty;
                string fileName = string.Empty;
                string server_msg = string.Empty;
                int timeOutMs = Convert.ToInt32(Func.ReadINI("Setting", "Iperf", "TxTimeOutMs", "0"));

                wifiData = new WiFiCollection(type, rfTest);

                KillTaskProcess(iperfApp);

                switch (type)
                {
                    case WiFiType.WiFi_2G:
                        #region WiFiType.WiFi_2G
                        resultFile = rfTest.ToString() + "_2G" + ".ini";
                        cmd = Func.ReadINI("Setting", "Iperf", rfTest.ToString() + "_2G", string.Empty);
                        server_msg = rfTest.ToString() + "_2G";
                        break;
                    #endregion
                    case WiFiType.WiFi_5G:
                        #region WiFiType.WiFi_5G
                        resultFile = rfTest.ToString() + "_5G" + ".ini";
                        cmd = Func.ReadINI("Setting", "Iperf", rfTest.ToString() + "_5G", string.Empty);
                        server_msg = rfTest.ToString() + "_5G";
                        break;
                    #endregion
                    case WiFiType.WiFi_6G:
                        #region WiFiType.WiFi_6G
                        resultFile = rfTest.ToString() + "_6G" + ".ini";
                        cmd = Func.ReadINI("Setting", "Iperf", rfTest.ToString() + "_6G", string.Empty);
                        server_msg = rfTest.ToString() + "_6G";
                        break;
                    #endregion
                    default:
                        break;
                }

                fileName = type.ToString() + "_" + rfTest.ToString() + ".bat";

                CreateBatchFile(fileName, cmd, resultFile);

                DisplayMsg(LogType.Log, rfTest.ToString() + " command : " + cmd);

                //if (init)
                //{
                //    WiFiPreSetting(type);
                //    //WiFiAntenna(type);          
                //}

                if (!CheckGoNoGo())
                {
                    return;
                }

                //Rena_20230620, TODO: debug test 2G only
                //if (!SendToServer(type, rfTest.ToString(), CHK.OK.ToString(), 10 * 1000))//ask server to restart iperf
                if (!SendToServer(type, server_msg, CHK.OK.ToString(), 10 * 1000))//ask server to restart iperf
                {
                    AddData(type.ToString(), 1);
                    return;
                }
                if (String.Compare(rfTest.ToString(), RfTest.Rx.ToString(), true) == 0)//Add By Leo 20210715
                {
                    DisplayMsg(LogType.Log, "Delay 2s...");
                    System.Threading.Thread.Sleep(2000);
                }

                Directory.SetCurrentDirectory(Application.StartupPath);
                DisplayMsg(LogType.Log, "Excute File Name : " + Application.StartupPath + "\\" + fileName);
                process = new Process();
                process.StartInfo.FileName = Application.StartupPath + "\\" + fileName;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.Start();

                if (String.Compare(rfTest.ToString(), RfTest.Rx.ToString(), true) == 0)
                {
                    timeOutMs = Convert.ToInt32(Func.ReadINI("Setting", "Iperf", "RxTimeOutMs", "0"));
                    //Thread.Sleep(3000);
                    rxThread = new Thread(new ThreadStart(WiFiRx));
                    rxThread.Start();
                }


                #region Get current
                Thread getcurr = new Thread(() =>
                {
                    if (Func.ReadINI("Setting", "PowerSupply", "PowerSupply", "0") == "1")
                    {
                        double curr = -1;
                        CheckCurrent(Convert.ToInt16(Func.ReadINI("Setting", "PowerSupply", "CH1_Channel", "1")), ref curr);
                        AddData($"{type}_{rfTest}_Curr", curr);
                        if (usePS_3323)
                        {
                            float vol = _ps3323.Get_Vol(1);
                            DisplayMsg(LogType.Log, "Vol:" + vol);
                            AddData($"{type}_{rfTest}_Vol", vol);
                        }
                    }

                });
                getcurr.Start();
                #endregion

                ChkProcess(ref process, resultFile, timeOutMs);

                wifiData.SetIperfFlag(false);

                KillTaskProcess(iperfApp);

                if (!ChkIperfResult(resultFile, ref content))
                {
                    AddData(type.ToString(), 1);
                    goto Exit;
                }

                FetchIperfResult(ref wifiData, content, type);

                if (type == WiFiType.WiFi_2G)
                {
                    status_ATS.AddData(type.ToString() + "_" + rfTest.ToString().ToUpper() + "_TP", "Mbits", wifiData.FetchIperfResult());   // Mbits
                }
                else
                {
                    status_ATS.AddData(type.ToString() + "_" + rfTest.ToString().ToUpper() + "_TP", "Gbits", wifiData.FetchIperfResult());   // Gbits
                }

            //GetPHYRate(type, rfTest);

            Exit:

                wifiList.Add(wifiData);

                //if (rxThread == null)
                //{
                //    return;
                //}

                if (rxThread != null && rxThread.IsAlive)
                {
                    DisplayMsg(LogType.Warning, "Abort rx thread.");
                    rxThread.Join();
                    rxThread.Abort();
                }
                if (getcurr != null && getcurr.IsAlive)
                {
                    DisplayMsg(LogType.Warning, "Abort get current thread.");
                    getcurr.Join();
                    getcurr.Abort();
                }
                Directory.SetCurrentDirectory(Application.StartupPath);
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData(type.ToString(), 1);
            }
        }

        public void CheckCurrent(int sChannel, ref double curr)
        {
            if (!CheckGoNoGo())
            { return; }

            int CurrDelay = Convert.ToInt32(Func.ReadINI("Setting", "DelayTime", "CurrDelay", "1"));
            string data = "";
            curr = -1;
            try
            {
                DisplayMsg(LogType.Log, "Test Current start ... ");
                //for (int c = 0; c < 1; c++)
                //{
                DisplayMsg(LogType.Log, "Delay Time : " + CurrDelay + " sec .");
                Thread.Sleep(CurrDelay * 1000);
                //if (WNC.API.Func.ReadINI(Application.StartupPath, "Setting", "PowerSupply", "PS3323", "0") == "1")
                //{
                //    string CH = WNC.API.Func.ReadINI(Application.StartupPath, "Setting", "PowerSupply", "Channel", "1");
                //    data = "CHAN " + CH + ";:MEAS:CURR?";
                //    this.ps.DV.Write(data);
                //    Thread.Sleep(200);
                //    //curr = Convert.ToDouble(this.ps.DV.ReadString()) * 1000 * 2;
                //    curr = Convert.ToDouble(this.ps.DV.ReadString()) * 1000;
                //    base.status_ATS.AddLog("Get Channel : " + CH + " Curr : " + curr.ToString());
                //}
                if (usePS_3323)
                {
                    //curr = Convert.ToDouble(_ps3615.GetCurrent(sChannel_3323));
                    curr = _ps3323.Get_curr(sChannel);
                    DisplayMsg(LogType.Log, "Get Channel : " + sChannel + " Curr : " + curr.ToString());
                }
                else if (usePS_3615)
                {
                    curr = Convert.ToDouble(_ps3615.GetCurrent(sChannel));
                    DisplayMsg(LogType.Log, "Get Channel : " + sChannel + " Curr : " + curr.ToString());
                }
            }
            catch (SystemException exception)
            {
                DisplayMsg(LogType.Log, exception.ToString());
                warning = "Exception";
            }
        }

        private bool PingHost(string nameOrAddress, int TimeOut, bool showlog = true)
        {
            bool pingable = false;
            WNC.API.Ping pinger = null;
            try
            {

                DisplayMsg(LogType.Log, String.Format("Ping to {0}, time out is {1}s", nameOrAddress, TimeOut / 1000));
                pinger = new WNC.API.Ping();
                for (int i = 0; i < TimeOut / 1000; i++)
                {
                    var pingResult = pinger.PingHost(nameOrAddress);
                    if (pingResult == WNC.API.Ping.PingResult.Success)
                    {
                        pingable = true;
                        if (showlog)
                            DisplayMsg(LogType.Log, "Ping to " + nameOrAddress + " ok");
                        break;
                    }
                    Thread.Sleep(1000);
                }

            }
            catch (System.Net.NetworkInformation.PingException)
            {
                DisplayMsg(LogType.Warning, "Ping Host Error");
                return pingable;
            }
            return pingable;
        }

        private bool WiFiAntenna(WiFiType type)
        {
            if (!CheckGoNoGo())
            {
                return false;
            }

            string errorItem = "PING_" + type.ToString();
            string ip = string.Empty;
            int delayMs = 0;
            int timeOutMs = 10 * 1000;
            WiFiInformation collection = RfType(type);
            switch (type)
            {
                case WiFiType.WiFi_2G:
                    #region WiFiType.WiFi_2G
                    ip = collection.Golden2G_Ip;
                    SendAndChk(type.ToString(), PortType.TELNET, "ifconfig " + antenna_5G + " down", "Wrt:/#", delayMs, timeOutMs);
                    SendAndChk(type.ToString(), PortType.TELNET, "ifconfig " + antenna_2G + " up", "Wrt:/#", delayMs, timeOutMs);
                    SendAndChk(type.ToString(), PortType.TELNET, "ifconfig " + antenna_6G + " down", "Wrt:/#", delayMs, timeOutMs);
                    break;
                #endregion
                case WiFiType.WiFi_5G:
                    #region WiFiType.WiFi_5G
                    ip = collection.Golden5G_Ip;
                    //SendAndChk(type.ToString(), PortType.TELNET, "ifconfig " + antenna_5G + " up", "Wrt:/#", delayMs, timeOutMs);   // 5G throughput will under 700
                    SendAndChk(type.ToString(), PortType.TELNET, "ifconfig " + antenna_2G + " down", "Wrt:/#", delayMs, timeOutMs);
                    SendAndChk(type.ToString(), PortType.TELNET, "ifconfig " + antenna_6G + " down", "Wrt:/#", delayMs, timeOutMs);

                    SendAndChk(type.ToString(), PortType.GOLDEN_UART_2G_5G, "wifi down", "Wrt:/#", delayMs, timeOutMs);
                    SendAndChk(type.ToString(), PortType.GOLDEN_UART_2G_5G, "wifi", "Wrt:/#", delayMs, timeOutMs);
                    break;
                #endregion
                case WiFiType.WiFi_6G:
                    #region WiFiType.WiFi_6G
                    ip = collection.Golden6G_Ip;
                    SendAndChk(type.ToString(), PortType.TELNET, "ifconfig " + antenna_5G + " down", "Wrt:/#", delayMs, timeOutMs);
                    SendAndChk(type.ToString(), PortType.TELNET, "ifconfig " + antenna_2G + " down", "Wrt:/#", delayMs, timeOutMs);
                    SendAndChk(type.ToString(), PortType.TELNET, "ifconfig " + antenna_6G + " up", "Wrt:/#", delayMs, timeOutMs);
                    break;
                #endregion
                default:
                    break;
            }

            if (!CheckGoNoGo())
            {
                return false;
            }

            timeOutMs = 100 * 1000;

            if (!telnet.Ping(ip, timeOutMs))
            {
                status_ATS.AddDataLog(errorItem, NG);
                return false;
            }

            status_ATS.AddData(errorItem + "_TIME", "sec", -9999, 9999, telnet.pingTimeS, "A00000");
            status_ATS.AddDataLog(errorItem, PASS);
            return true;
        }

        private void WiFiRx()
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            try
            {
                int avgCount = 5;
                int delayMs = 3000;
                DateTime dt;
                TimeSpan ts;
                PortType portType = PortType.SSH;

                wifiData.ClearRssi();

                DisplayMsg(LogType.Log, "Delay " + delayMs + " (ms)..");
                dt = DateTime.Now;
                while (true)
                {
                    ts = new TimeSpan(DateTime.Now.Ticks - dt.Ticks);
                    if (ts.TotalMilliseconds > delayMs)
                    {
                        break;
                    }

                    System.Threading.Thread.Sleep(100);
                }

                switch (wifiData.WifiType)
                {
                    case WiFiType.WiFi_2G:
                        #region WiFiType.WiFi_2G
                        WiFi_2G_Rssi(portType, avgCount);
                        break;
                    #endregion
                    case WiFiType.WiFi_5G:
                        #region WiFiType.WiFi_5G
                        WiFi_5G_Rssi(portType, avgCount);
                        break;
                    #endregion
                    case WiFiType.WiFi_6G:
                        #region WiFiType.WiFi_6G
                        WiFi_6G_Rssi(portType, avgCount);
                        break;
                    #endregion
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                AddData(wifiData.WifiType.ToString(), 1);
            }
        }

        private void WiFi_2G_Rssi(PortType portType, int avgCount)
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            string item = wifiData.WifiType.ToString();

            try
            {
                string res = string.Empty;
                string keyword = "root@OpenWrt:~# \r\n";
                bool result = false;
                int delayMs = 0;
                int timeOutMs = 20 * 1000;

                SendAndChk(portType, "wifistats wifi0 0 10", keyword, delayMs, timeOutMs);

                for (int i = 0; i < avgCount; i++)
                {
                    SendCommand(portType, "wifistats wifi0 10 | grep rssi_chain", delayMs);
                    DisplayMsg(LogType.Log, "Delay 300ms for waiting rssi data");
                    Thread.Sleep(300);//delay for waiting rssi data
                }

                result = ChkResponse(portType, ITEM.WiFi_2G_RSSI, keyword, out res, timeOutMs);

                if (!result)
                {
                    status_ATS.AddData(item + "_CH_0_RSSI", "dBm", -999);
                    return;
                }

                FetchWiFiRssiResult(wifiData, avgCount);
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                status_ATS.AddData(item + "_CH_0_RSSI", "dBm", -999);
            }
        }

        private void WiFi_5G_Rssi(PortType portType, int avgCount)
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            string item = wifiData.WifiType.ToString();

            try
            {
                string res = string.Empty;
                string keyword = "root@OpenWrt:~# \r\n";
                bool result = false;
                int delayMs = 0;
                int timeOutMs = 20 * 1000;

                SendAndChk(portType, "wifistats wifi2 0 10", keyword, delayMs, timeOutMs);

                for (int i = 0; i < avgCount; i++)
                {
                    SendCommand(portType, "wifistats wifi2 10 | grep rssi_chain", delayMs);
                    DisplayMsg(LogType.Log, "Delay 0.5s for waiting rssi data");
                    System.Threading.Thread.Sleep(500);//delay for waiting rssi data
                }

                result = ChkResponse(portType, ITEM.WiFi_5G_RSSI, keyword, out res, timeOutMs);

                if (!result)
                {
                    status_ATS.AddData(item + "_CH_0_RSSI", "dBm", -999);
                    return;
                }

                FetchWiFiRssiResult(wifiData, avgCount);
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                status_ATS.AddData(item + "_CH_0_RSSI", "dBm", -999);
            }
        }

        private void WiFi_6G_Rssi(PortType portType, int avgCount)
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            string item = wifiData.WifiType.ToString();

            try
            {
                string res = string.Empty;
                string keyword = "root@OpenWrt:~# \r\n";
                bool result = false;
                int delayMs = 0;
                int timeOutMs = 20 * 1000;

                SendAndChk(portType, "wifistats wifi1 0 10", keyword, delayMs, timeOutMs);

                for (int i = 0; i < avgCount; i++)
                {
                    SendCommand(portType, "wifistats wifi1 10 | grep rssi_chain", delayMs);
                    DisplayMsg(LogType.Log, "Delay 1s for waiting rssi data");
                    Thread.Sleep(1000);//delay for waiting rssi data
                }

                result = ChkResponse(portType, ITEM.WiFi_6G_RSSI, keyword, out res, timeOutMs);

                if (!result)
                {
                    status_ATS.AddData(item + "_CH_0_RSSI", "dBm", -999);
                    return;
                }

                FetchWiFiRssiResult(wifiData, avgCount);
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                status_ATS.AddData(item + "_CH_0_RSSI", "dBm", -999);
            }
        }

        private void FetchWiFiRssiResult(WiFiCollection data, int avgCount)
        {
            if (!wifiData.ChkRssiCount())   // avgCount Jerry Modified on 2020.02.03
            {
                status_ATS.AddData(data.WifiType.ToString() + "_CH_0_RSSI", "dBm", -999);
                return;
            }

            double value = -95;
            DisplayMsg(LogType.Log, "Add value " + value.ToString());

            int atn1offset;
            int atn2offset;
            int atn3offset;
            int atn4offset;

            try
            {
                atn1offset = Convert.ToInt32(Func.ReadINI("Setting", "RSSI_offset", data.WifiType.ToString() + "_CH_0", "0"));
                atn2offset = Convert.ToInt32(Func.ReadINI("Setting", "RSSI_offset", data.WifiType.ToString() + "_CH_1", "0"));
                atn3offset = Convert.ToInt32(Func.ReadINI("Setting", "RSSI_offset", data.WifiType.ToString() + "_CH_2", "0"));
                atn4offset = Convert.ToInt32(Func.ReadINI("Setting", "RSSI_offset", data.WifiType.ToString() + "_CH_3", "0"));

                if (atn1offset > 3 || atn1offset < -3)
                    atn1offset = 0;
                if (atn2offset > 3 || atn2offset < -3)
                    atn2offset = 0;
                if (atn3offset > 3 || atn3offset < -3)
                    atn3offset = 0;
                if (atn4offset > 3 || atn4offset < -3)
                    atn4offset = 0;

                DisplayMsg(LogType.Log, data.WifiType.ToString() + "_CH_0_Offset:" + atn1offset);
                DisplayMsg(LogType.Log, data.WifiType.ToString() + "_CH_1_Offset:" + atn2offset);
                DisplayMsg(LogType.Log, data.WifiType.ToString() + "_CH_2_Offset:" + atn3offset);
                DisplayMsg(LogType.Log, data.WifiType.ToString() + "_CH_3_Offset:" + atn4offset);
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                status_ATS.Write_Warning("Convert value error", StatusUI2.StatusUI.StatusProc.Warning);
                return;
            }

            status_ATS.AddData(data.WifiType.ToString() + "_CH_0_RSSI", "dBm", wifiData.Avg_ch_0 + value + atn1offset);
            status_ATS.AddData(data.WifiType.ToString() + "_CH_1_RSSI", "dBm", wifiData.Avg_ch_1 + value + atn2offset);

            //Rena_20221013, WiFi 2G只看ch0 & ch1
            if (data.WifiType != WiFiType.WiFi_2G)
            {
                status_ATS.AddData(data.WifiType.ToString() + "_CH_2_RSSI", "dBm", wifiData.Avg_ch_2 + value + atn3offset);
                status_ATS.AddData(data.WifiType.ToString() + "_CH_3_RSSI", "dBm", wifiData.Avg_ch_3 + value + atn4offset);
            }
        }

        private void CheckWiFiCalibrationData()
        {
            if (!CheckGoNoGo())
                return;
            int delayMs = 0;
            int timeOutMs = 5 * 1000;
            DisplayMsg(LogType.Log, "=============== CheckWiFiCalibrationData ===============");

            string res = string.Empty;
            bool result = false;

            SendCommand(PortType.TELNET, "hexdump -s 0x1000 -n 16 /dev/mmcblk0p17", delayMs);
            result = ChkResponse(PortType.TELNET, ITEM.NONE, "root@OpenWrt", out res, timeOutMs);
            //DisplayMsg(LogType.Log, "CheckWiFiCalibrationData 2/5G====="+ res);
            if (res.Contains("ffff ffff ffff ffff") || res.Contains("0000 0000 0000 0000") || !res.Contains("0001000") || !result)
            {
                DisplayMsg(LogType.Log, "CheckWiFiCalibrationData 2G/5G NG");
                status_ATS.AddDataLog("CheckIPQCalibration", NG);
                return;
            }


            SendCommand(PortType.TELNET, "hexdump -s 0x26800 -n 16 /dev/mmcblk0p17", delayMs);
            result = ChkResponse(PortType.TELNET, ITEM.NONE, "root@OpenWrt", out res, timeOutMs);
            //DisplayMsg(LogType.Log, "CheckWiFiCalibrationData 6G=====" + res);
            if (res.Contains("ffff ffff ffff ffff") || res.Contains("0000 0000 0000 0000") || !res.Contains("0026800") || !result)
            {
                DisplayMsg(LogType.Log, "CheckWiFiCalibrationData 6G NG");
                status_ATS.AddDataLog("CheckIPQCalibration", NG);
                return;
            }

            DisplayMsg(LogType.Log, "CheckWiFiCalibrationData 2.5G, 5G & 6G PASS");
            status_ATS.AddDataLog("CheckIPQCalibration", PASS);
        }

        /* Iperf process */
        private void CreateBatchFile(string fileName, string content, string resultFile)
        {
            Directory.SetCurrentDirectory(Application.StartupPath);

            DisplayMsg(LogType.Log, "File : " + fileName);

            if (File.Exists(Application.StartupPath + @"\" + fileName))
            {
                DisplayMsg(LogType.Log, "Delete file : " + fileName);
                File.Delete(Application.StartupPath + @"\" + fileName);
            }

            if (File.Exists(Application.StartupPath + @"\" + resultFile))
            {
                DisplayMsg(LogType.Log, "Delete file : " + resultFile);
                File.Delete(Application.StartupPath + @"\" + resultFile);
            }

            FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            StreamWriter Stw = new StreamWriter(fs);
            Stw.WriteLine(content);
            Stw.Close();
            Stw.Dispose();
            fs.Close();
        }

        private bool ChkIperfResult(string file, ref string content, string keyword = "[SUM]")
        {
            content = string.Empty;

            try
            {
                Directory.SetCurrentDirectory(Application.StartupPath);

                if (!File.Exists(file))
                {
                    return false;
                }

                FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read);
                StreamReader Str = new StreamReader(fs);
                content = Str.ReadToEnd();
                Str.Close();
                Str.Dispose();
                fs.Close();
                DisplayMsg(LogType.Log, "iperf raw data======>");
                DisplayMsg(LogType.Log, content);
                if (content.Contains(keyword))
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                return false;
            }
        }

        private bool ChkIperfStart(string file)
        {
            try
            {
                string content = string.Empty;
                string keyword = "Mbits/sec";

                if (!File.Exists(Application.StartupPath + @"\" + file))
                {
                    return false;
                }

                System.Threading.Thread.Sleep(500);
                wifiData.SetIperfFlag(true);
                return true;

                FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read);
                StreamReader Str = new StreamReader(fs);
                content = Str.ReadToEnd();
                Str.Close();
                Str.Dispose();
                fs.Close();

                if (content.Contains(keyword))
                {
                    DisplayMsg(LogType.Log, "Ongoing iprf process");
                    wifiData.SetIperfFlag(true);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                return false;
            }
        }

        private bool ChkIperfComplete(string file)
        {
            try
            {
                string content = string.Empty;
                string keyword = "iperf Done";

                if (!File.Exists(Application.StartupPath + @"\" + file))
                {
                    return false;
                }

                FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read);
                StreamReader Str = new StreamReader(fs);
                content = Str.ReadToEnd();
                Str.Close();
                Str.Dispose();
                fs.Close();

                if (content.Contains(keyword))
                {
                    DisplayMsg(LogType.Log, "Iprf process complete");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                return false;
            }
        }

        private void FetchIperfResult(ref WiFiCollection data, string content, WiFiType wifiType)
        {
            try
            {
                Directory.SetCurrentDirectory(Application.StartupPath);

                WiFiCollection.IperfCollection collection;

                string item = string.Empty;
                string res = string.Empty;
                string keyword = "SUM";
                string[] msg;

                wifiData.ClearIperf();

                res = content.Replace("\n", "$");
                msg = res.Split('$');

                for (int i = 0; i < msg.Length; i++)
                {
                    DisplayMsg(LogType.Log, msg[i]);
                    if (msg[i].Contains(keyword))
                    {
                        collection = new WiFiCollection.IperfCollection(msg[i], wifiType);

                        if (collection.Result)
                        {
                            //string timeSrat = collection.IntervalSec.Split('-')[0];
                            //string timeEnd = collection.IntervalSec.Split('-')[1];

                            //timeSrat = timeSrat.Split('.')[0];
                            //timeEnd = timeEnd.Split('.')[0];

                            //item = data.WifiType.ToString() + "_" + data.TestType.ToString() + "[" + timeSrat + "-" + timeEnd + "]";

                            //status_ATS.AddData(item,
                            //                   "Mbits",
                            //                   -9999,
                            //                   9999,
                            //                   collection.BandwidthSpeedMbitsSec,
                            //                   "A000000");
                            data.AddIperf(collection);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                //status_ATS.AddDataLog(wifiData.WifiType.ToString(), NG);
                AddData(wifiData.WifiType.ToString(), 1);
            }
        }

        private bool ChkProcess(ref Process process, string file, int timeOutMs)
        {
            try
            {
                DateTime dt;
                TimeSpan ts;
                dt = DateTime.Now;


                while (true)
                {
                    ts = new TimeSpan(DateTime.Now.Ticks - dt.Ticks);
                    if (ts.TotalMilliseconds > timeOutMs)
                    {
                        DisplayMsg(LogType.Error, "Check Iperf process timeOut !!");
                        MessageBox.Show("Vui long phan tich loi. Analysis error");
                        KillIperfPeocess(ref process);
                        return false;
                    }

                    Thread.Sleep(100);

                    if (!wifiData.ChkIperfFlag())
                    {
                        ChkIperfStart(file);
                    }

                    //if (ChkIperfComplete(file))
                    //{
                    //    DisplayMsg(LogType.Log, "Check Iperf process complete (ms) : " + ts.TotalMilliseconds.ToString());
                    //    return true;
                    //}

                    if (!process.HasExited)
                    {
                        continue;
                    }
                    else
                    {
                        DisplayMsg(LogType.Log, "Check Iperf process complete (ms) : " + ts.TotalMilliseconds.ToString());
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                KillIperfPeocess(ref process);
                return false;
            }
        }

        private void KillIperfPeocess(ref Process process)
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill();
                    process.Dispose();
                }

                KillTaskProcess(iperfApp);
                System.Threading.Thread.Sleep(200);
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                KillTaskProcess(iperfApp);
                System.Threading.Thread.Sleep(200);
            }
        }

        /* Connect Server */
        private bool SendToServer(WiFiType type, string msg, string keyword, int timeOutMs)
        {
            DateTime dt;
            TimeSpan ts;

            int retry = 3;
            IPAddress ipAddress;
            IPEndPoint ipPoint;
            Socket socket;
            NetworkStream nkStream;
            StreamWriter stw;
            StreamReader str;
            string ip = string.Empty;
            string res = string.Empty;
            WiFiInformation collection = RfType(type);

        Server_retry:
            try
            {
                switch (wifiData.WifiType)
                {
                    case WiFiType.WiFi_2G:
                        #region WiFiType.WiFi_2G
                        ip = collection.PC_Golden2G_Ip;
                        break;
                    #endregion
                    case WiFiType.WiFi_5G:
                        #region WiFiType.WiFi_5G
                        ip = collection.PC_Golden5G_Ip;
                        break;
                    #endregion
                    case WiFiType.WiFi_6G:
                        #region WiFiType.WiFi_6G
                        ip = collection.PC_Golden6G_Ip;
                        break;
                    #endregion
                    default:
                        break;
                }

                DisplayMsg(LogType.Log, "Connect " + wifiData.WifiType.ToString().Replace("_", " ") + " Server ( " + ip + " ) ..");
                ipAddress = IPAddress.Parse(ip);
                ipPoint = new IPEndPoint(ipAddress, 8081);
                socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(ipPoint);
                DisplayMsg(LogType.Log, "Connect success.");


                dt = DateTime.Now;
                while (true)
                {
                    ts = new TimeSpan(DateTime.Now.Ticks - dt.Ticks);
                    if (ts.TotalMilliseconds > timeOutMs)
                    {
                        DisplayMsg(LogType.Error, "Check Server response timeOut !!");
                        if (socket != null)
                        {
                            socket.Close();
                        }
                        return false;
                    }

                    nkStream = new NetworkStream(socket);
                    stw = new StreamWriter(nkStream);
                    stw.WriteLine(msg);
                    stw.Flush();
                    DisplayMsg(LogType.Log, "Transmit '" + msg + "' to Server");

                    System.Threading.Thread.Sleep(200);

                    str = new StreamReader(nkStream);
                    res = str.ReadLine();
                    DisplayMsg(LogType.Log, "Reveive from Server : " + res);

                    if (res.Contains(keyword))
                    {
                        break;
                    }
                }

                if (socket != null)
                {
                    socket.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, "Connect Server failed : " + ex.Message);
                if (retry-- > 0)
                {
                    DisplayMsg(LogType.Exception, "SendToServer retry...");
                    goto Server_retry;
                }
                return false;
            }
        }
        private bool KillIperfInServer(WiFiType type)
        {
            if (!CheckGoNoGo())
            {
                return false;
            }

            DateTime dt;
            TimeSpan ts;

            int retry = 3;
            IPAddress ipAddress;
            IPEndPoint ipPoint;
            Socket socket;
            NetworkStream nkStream;
            StreamWriter stw;
            StreamReader str;
            string ip = string.Empty;
            string res = string.Empty;
            int timeOutMs = 10 * 1000;

        Server_retry:
            try
            {
                WiFiInformation collection = RfType(type);
                switch (type)
                {
                    case WiFiType.WiFi_2G:
                        #region WiFiType.WiFi_2G
                        ip = collection.PC_Golden2G_Ip;
                        break;
                    #endregion
                    case WiFiType.WiFi_5G:
                        #region WiFiType.WiFi_5G
                        ip = collection.PC_Golden5G_Ip;
                        break;
                    #endregion
                    case WiFiType.WiFi_6G:
                        #region WiFiType.WiFi_6G
                        ip = collection.PC_Golden6G_Ip;
                        break;
                    #endregion
                    default:
                        break;
                }

                DisplayMsg(LogType.Log, "Connect " + type.ToString().Replace("_", " ") + " Server ( " + ip + " ) ..");

                ipAddress = IPAddress.Parse(ip);
                ipPoint = new IPEndPoint(ipAddress, 8081);
                socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(ipPoint);
                DisplayMsg(LogType.Log, "Connect success.");


                dt = DateTime.Now;
                while (true)
                {
                    ts = new TimeSpan(DateTime.Now.Ticks - dt.Ticks);
                    if (ts.TotalMilliseconds > timeOutMs)
                    {
                        DisplayMsg(LogType.Error, "Check Server response timeOut !!");
                        if (socket != null)
                        {
                            socket.Close();
                        }
                        return false;
                    }

                    nkStream = new NetworkStream(socket);
                    stw = new StreamWriter(nkStream);
                    stw.WriteLine("Close");
                    stw.Flush();
                    DisplayMsg(LogType.Log, "Transmit 'Close' to Server");

                    System.Threading.Thread.Sleep(200);

                    str = new StreamReader(nkStream);
                    res = str.ReadLine();
                    DisplayMsg(LogType.Log, "Reveive from Server : " + res);

                    if (res.Contains("OK"))
                    {
                        break;
                    }
                }

                if (socket != null)
                {
                    socket.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, "Connect Server failed : " + ex.Message);
                if (retry-- > 0)
                {
                    DisplayMsg(LogType.Exception, "KillIperfInServer retry...");
                    goto Server_retry;
                }
                return false;
            }
        }
        private void WiFiGolden(WiFiType type)
        {
            if (!CheckGoNoGo())
            {
                return;
            }

            WifiPing(type);
        }
    }
}