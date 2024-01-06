using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
using WNC.API;
using EventHandle;

namespace MiniPwrSupply.Singleton
{

    public enum WiFiType
    {
        WiFi,
        WiFi_2G,
        WiFi_5G,
        WiFi_6G,
    }

    public enum RfTest
    {
        Tx,
        Rx,
        NONE,
    }

    public class WiFiInformation
    {
        private string name = string.Empty;
        public string Name
        {
            get
            {
                return this.name;
            }
        }

        private string channel = string.Empty;
        public string Channel
        {
            get
            {
                return this.channel;
            }
        }

        private string mode = string.Empty;
        public string Mode
        {
            get
            {
                return this.mode;
            }
        }

        private string ssid = string.Empty;
        public string Ssid
        {
            get
            {
                return this.ssid;
            }
        }

        private string golden2G_Ip = string.Empty;
        public string Golden2G_Ip
        {
            get
            {
                return this.golden2G_Ip;
            }
        }

        private string golden5G_Ip = string.Empty;
        public string Golden5G_Ip
        {
            get
            {
                return this.golden5G_Ip;
            }
        }
        private string golden6G_Ip = string.Empty;
        public string Golden6G_Ip
        {
            get
            {
                return this.golden6G_Ip;
            }
        }

        private string pc_golden6G_Ip = string.Empty;
        public string PC_Golden6G_Ip
        {
            get
            {
                return this.pc_golden6G_Ip;
            }
        }

        private string pc_golden5G_Ip = string.Empty;
        public string PC_Golden5G_Ip
        {
            get
            {
                return this.pc_golden5G_Ip;
            }
        }

        private string pc_golden2G_Ip = string.Empty;
        public string PC_Golden2G_Ip
        {
            get
            {
                return this.pc_golden2G_Ip;
            }
        }


        public WiFiInformation(string section)
        {
            this.name = section;
            this.channel = Func.ReadINI("Setting", section, "Channel", string.Empty);
            this.mode = Func.ReadINI("Setting", section, "Mode", string.Empty);
            this.ssid = Func.ReadINI("Setting", section, "SSID", string.Empty);
            this.golden2G_Ip = Func.ReadINI("Setting", section, "2G_Golden_IP", string.Empty);
            this.golden5G_Ip = Func.ReadINI("Setting", section, "5G_Golden_IP", string.Empty);
            this.golden6G_Ip = Func.ReadINI("Setting", section, "6G_Golden_IP", string.Empty);

            this.pc_golden2G_Ip = Func.ReadINI("Setting", section, "PC_2G_Golden_IP", string.Empty);
            this.pc_golden5G_Ip = Func.ReadINI("Setting", section, "PC_5G_Golden_IP", string.Empty);
            this.pc_golden6G_Ip = Func.ReadINI("Setting", section, "PC_6G_Golden_IP", string.Empty);
        }
    }

    public class WiFiCollection
    {
        public static event EventLogHandler Message;

        protected virtual void OnMessageDisplay(EventLogArgs e)
        {
            if (Message != null)
                Message(e);
        }

        private void DisplayMsg(LogType type, string message)
        {
            EventLogArgs eLog = new EventLogArgs("[ " + type.ToString() + " ]  " + message);
            OnMessageDisplay(eLog);
        }

        private WiFiType wifiType = WiFiType.WiFi;
        public WiFiType WifiType
        {
            get
            {
                return this.wifiType;
            }
        }

        private RfTest testType = RfTest.NONE;
        public RfTest TestType
        {
            get
            {
                return this.testType;
            }
        }

        public class IperfCollection
        {
            private bool result = false;
            public bool Result
            {
                get
                {
                    return this.result;
                }
            }

            private string msg = string.Empty;
            public string Msg
            {
                get
                {
                    return this.msg;
                }
            }

            private string intervalSec = string.Empty;
            public string IntervalSec
            {
                get
                {
                    return this.intervalSec;
                }
            }

            private double transferMBytes = -999;
            public double TransferMBytes
            {
                get
                {
                    return this.transferMBytes;
                }
            }

            private double bandwidthMbitsSec = -999;
            public double BandwidthSpeedMbitsSec
            {
                get
                {
                    return this.bandwidthMbitsSec;
                }
            }

            public IperfCollection(string content, WiFiType wifiType)
            {
                try
                {
                    string[] str;
                    this.msg = content.Trim();
                    str = msg.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (str.Length != 7)
                    {
                        this.result = false;
                        return;
                    }

                    this.intervalSec = str[1];
                    this.transferMBytes = Convert.ToDouble(str[3]);
                    if ((wifiType == WiFiType.WiFi_5G || wifiType == WiFiType.WiFi_6G))
                    {
                        if (content.Contains("Mbits/sec"))
                            this.bandwidthMbitsSec = Convert.ToDouble(str[5]) / 1000;
                        else if (content.Contains("Gbits/sec"))
                            this.bandwidthMbitsSec = Convert.ToDouble(str[5]);
                    }
                    else
                    {
                        this.bandwidthMbitsSec = Convert.ToDouble(str[5]);
                    }
                    this.result = true;
                }
                catch (Exception ex)
                {
                    this.result = false;
                    return;
                }
            }
        }

        private List<IperfCollection> iperfList;
        public List<IperfCollection> IperfList
        {
            get
            {
                return this.iperfList;
            }
        }

        private List<double> rssi_ch_0;
        public List<double> Rssi_ch_0
        {
            get
            {
                return this.rssi_ch_0;
            }
        }

        private List<double> rssi_ch_1;
        public List<double> Rssi_ch_1
        {
            get
            {
                return this.rssi_ch_1;
            }
        }

        private List<double> rssi_ch_2;
        public List<double> Rssi_ch_2
        {
            get
            {
                return this.rssi_ch_2;
            }
        }

        private List<double> rssi_ch_3;
        public List<double> Rssi_ch_3
        {
            get
            {
                return this.rssi_ch_3;
            }
        }

        private bool iperfFlag = false;

        private double avg_ch_0 = -999;
        public double Avg_ch_0
        {
            get
            {
                return this.avg_ch_0;
            }
        }

        private double avg_ch_1 = -999;
        public double Avg_ch_1
        {
            get
            {
                return this.avg_ch_1;
            }
        }

        private double avg_ch_2 = -999;
        public double Avg_ch_2
        {
            get
            {
                return this.avg_ch_2;
            }
        }

        private double avg_ch_3 = -999;
        public double Avg_ch_3
        {
            get
            {
                return this.avg_ch_3;
            }
        }

        public void AddData(int ch, string data)
        {
            try
            {
                //DisplayMsg(LogType.Log, $"AddData ch:{ch} data:{data}"); //Rena_20230621, add for debug

                switch (ch)
                {
                    case 0:
                        this.rssi_ch_0.Add(Convert.ToDouble(data));
                        return;
                    case 1:
                        this.rssi_ch_1.Add(Convert.ToDouble(data));
                        return;
                    case 2:
                        this.rssi_ch_2.Add(Convert.ToDouble(data));
                        return;
                    case 3:
                        this.rssi_ch_3.Add(Convert.ToDouble(data));
                        return;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void AddIperf(IperfCollection data)
        {
            this.iperfList.Add(data);
        }

        public void SetIperfFlag(bool flag)
        {
            this.iperfFlag = flag;
        }

        public bool ChkIperfFlag()
        {
            return this.iperfFlag;
        }

        public bool ChkRssiCount()
        {
            //if (rssi_ch_0.Count != avgCount ||
            //    rssi_ch_1.Count != avgCount ||
            //    rssi_ch_2.Count != avgCount ||
            //    rssi_ch_3.Count != avgCount ||
            //    rssi_ch_0.Count == 0)
            //{
            //    return false;
            //}

            this.avg_ch_0 = 0;
            this.avg_ch_1 = 0;
            this.avg_ch_2 = 0;
            this.avg_ch_3 = 0;

            rssi_ch_0.Sort();
            rssi_ch_1.Sort();
            rssi_ch_2.Sort();
            rssi_ch_3.Sort();
            try
            {
                if (rssi_ch_0.Count >= 5)
                {
                    rssi_ch_0.RemoveAt(rssi_ch_0.Count - 1);
                    rssi_ch_0.RemoveAt(0);
                }
                if (rssi_ch_1.Count >= 5)
                {
                    rssi_ch_1.RemoveAt(rssi_ch_1.Count - 1);
                    rssi_ch_1.RemoveAt(0);
                }
                if (rssi_ch_2.Count >= 5)
                {
                    rssi_ch_2.RemoveAt(rssi_ch_2.Count - 1);
                    rssi_ch_2.RemoveAt(0);
                }
                if (rssi_ch_3.Count >= 5)
                {
                    rssi_ch_3.RemoveAt(rssi_ch_3.Count - 1);
                    rssi_ch_3.RemoveAt(0);
                }
                /*Rena_20230627 TODO: disable for HQ debug test
                #region MyRegion alan modify as Leo requirement 09/05/2021

                for (int i = 0; i < rssi_ch_0.Count; i++)
                {
                    if (rssi_ch_0[i] > 115 || rssi_ch_0[i] < 5)
                    {
                        rssi_ch_0.RemoveAt(i);
                        i--;
                    }
                    else
                        avg_ch_0 += rssi_ch_0[i];
                }

                for (int i = 0; i < rssi_ch_1.Count; i++)
                {
                    if (rssi_ch_1[i] > 115 || rssi_ch_1[i] < 5)
                    {
                        rssi_ch_1.RemoveAt(i);
                        i--;
                    }
                    else
                        avg_ch_1 += rssi_ch_1[i];
                }

                //Rena_20221013, WiFi 2G只看ch0 & ch1
                if (this.wifiType != WiFiType.WiFi_2G)
                {
                    for (int i = 0; i < rssi_ch_2.Count; i++)
                    {
                        if (rssi_ch_2[i] > 115 || rssi_ch_2[i] < 5)
                        {
                            rssi_ch_2.RemoveAt(i);
                            i--;
                        }
                        else
                            avg_ch_2 += rssi_ch_2[i];
                    }

                    for (int i = 0; i < rssi_ch_3.Count; i++)
                    {
                        if (rssi_ch_3[i] > 115 || rssi_ch_3[i] < 5)
                        {
                            rssi_ch_3.RemoveAt(i);
                            i--;
                        }
                        else
                            avg_ch_3 += rssi_ch_3[i];
                    }
                }
                #endregion
                */
                //for (int i = 0; i < rssi_ch_0.Count; i++)
                //{
                //    avg_ch_0 += rssi_ch_0[i];
                //    avg_ch_1 += rssi_ch_1[i];
                //    avg_ch_2 += rssi_ch_2[i];
                //    avg_ch_3 += rssi_ch_3[i];
                //}

                if (rssi_ch_0.Count < 3 || rssi_ch_1.Count < 3
                    || rssi_ch_2.Count < 3 || rssi_ch_3.Count < 3)
                {
                    DisplayMsg(LogType.Log, String.Format("RSSI_CH_0 Count:{0}\nRSSI_CH_1 Count:{1}\nRSSI_CH_2 Count:{2}\nRSSI_CH_3 Count:{3}", rssi_ch_0.Count, rssi_ch_1.Count, rssi_ch_2.Count, rssi_ch_3.Count));
                    return false;
                }

                this.avg_ch_0 = avg_ch_0 / rssi_ch_0.Count;
                this.avg_ch_1 = avg_ch_1 / rssi_ch_1.Count;
                this.avg_ch_2 = avg_ch_2 / rssi_ch_2.Count;
                this.avg_ch_3 = avg_ch_3 / rssi_ch_3.Count;

            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.ToString());
                return false;
            }

            return true;
        }

        public double FetchIperfResult()
        {
            if (iperfList.Count == 0)
            {
                return -999;
            }
            // sort
            double value = 0;
            List<double> listValue = new List<double>();
            for (int i = 0; i < iperfList.Count; i++)
            {
                if (iperfList[i].BandwidthSpeedMbitsSec == -999)
                {
                    continue;
                }
                listValue.Add(iperfList[i].BandwidthSpeedMbitsSec);
                //value += iperfList[i].BandwidthSpeedMbitsSec;
            }
            listValue.Sort();
            listValue.Remove(0);
            listValue.Remove(listValue.Count - 1);
            foreach (var item in listValue)
            {
                value += item;
            }
            //value = value / iperfList.Count;
            value = value / listValue.Count;
            return value;
        }

        public void ClearRssi()
        {
            this.iperfFlag = false;
            this.rssi_ch_0 = new List<double>();
            this.rssi_ch_1 = new List<double>();
            this.rssi_ch_2 = new List<double>();
            this.rssi_ch_3 = new List<double>();
        }

        public void ClearIperf()
        {
            this.iperfList = new List<IperfCollection>();
        }

        public WiFiCollection(WiFiType wifiType, RfTest rfTest)
        {
            this.iperfFlag = false;
            this.wifiType = wifiType;
            this.testType = rfTest;
            this.rssi_ch_0 = new List<double>();
            this.rssi_ch_1 = new List<double>();
            this.rssi_ch_2 = new List<double>();
            this.rssi_ch_3 = new List<double>();
            this.iperfList = new List<IperfCollection>();
        }
    }
}