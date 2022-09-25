﻿using MiniPwrSupply.WuizhiCmd;
using MiniPwrSupply.DoWuzhiCmd;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Management;


namespace MiniPwrSupply
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public bool ReadySet
        {
            get
            {
                bool isStart = false;
                mutex.WaitOne();
                isStart = mGet_Start;
                mutex.ReleaseMutex();
                return isStart;
            }
            set
            {
                mutex.WaitOne();
                mGet_Start = value;
                mutex.ReleaseMutex();
            }
        }

        public enum WuzhiPower
        {
            PowerOn,
            PowerOff
        }

        public enum WuzhiConnectStatus
        {
            Connect,
            DisConnect
        }

        private SerialPort comport;
        private string wuzhiComport = string.Empty;
        private Int32 totalLength = 0;

        private delegate void Display(Byte[] buffer);

        private Boolean Isreceiving = true;
        //--------------------------

        private IWuzhiCmd mWuzhiCmd = null;
        private static Mutex mutex = new Mutex();
        private volatile bool mGet_Start;
        private StringBuilder receiveCall = new StringBuilder();
        private static string DEVICE_ID = "FTDIBUS" + "COMPORT" + "&" + "VID_0403" + "&" + "PID_6001";
        
        private bool mIsConnectedSerialPort = false;
        public Form1 mInstatnce = null;
        private Int32 innerReceiveDataFullLength;
        private static int iRetryTime = 6;
        public static int Err_checksum_is_wrong = 144;
        public static int Err_wrong_params_setting_or_params_overflow = 160;
        public static int Err_cmd_cannot_executed = 176;
        public static int Err_cmd_is_invaild = 192;
        public static int Err_cmd_is_unknown = 208;

        private void ShowErrMsg(string errMsg)
        {
            MessageBox.Show(errMsg, @"Error Occur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void DisplayText(Byte[] buffer)
        {
            textBox1.Text += String.Format("{0}{1}", BitConverter.ToString(buffer), Environment.NewLine);
            totalLength = totalLength + buffer.Length;
            label_DataReceived.Text = totalLength.ToString();
        }

        

        private void _WaitForUIThread(Action callback)
        {
            try
            {
                IAsyncResult r = this.BeginInvoke(new Action(() =>
                {
                    callback();
                }));
                r.AsyncWaitHandle.WaitOne();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //private void _hexAddition<Tiida>(IList<Tiida> tiidas)      //array<T>(string, T)改用Generic方式, 捨棄 byte[] hexBytes       // checksum = all hex addition substring last 2
        
        private string _ByteArrayToString(byte[] bytes)
        {
            StringBuilder hex = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                string hexStr = String.Format("{0:x2} ", b);
                //hex.AppendFormat("{0:x2} ", b);
                hex.Append(hexStr.ToUpper());
            }
            //Console.WriteLine("hexTostring --->" + hex.ToString());
            richTextBox1.AppendText("hexTostring --->" + hex.ToString());
            return hex.ToString();
        }

        private void serialport1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            WuzhiPower wuzhiPower = new WuzhiPower();

            string receivedata = string.Empty;      //      AA-01-12-80-00-00-00-00-00-00-00-00-00-00 00-00-00-00-00-3D
            int len = serialPort1.BytesToRead;      //      170-01-18-128-00-....-00-00-00-00-00-00 00-00-00-00-00-61
            string showInfo = string.Empty;
            Int32 wuzhi_ErrType = 0;
            //--------------------------------------
            //if ((sender as SerialPort).BytesToRead > 0)
            //{
            //    Byte[] buffer1 = new Byte[1024];
            //    try
            //    {
            //        Int32 length = (sender as SerialPort).Read(buffer1, 0, buffer1.Length);
            //        Array.Resize(ref buffer1, length);
            //        Display d = new Display(DisplayText);
            //        this.Invoke(d, new Object[] { buffer1 });
            //    }
            //    catch (TimeoutException timeoutEx)
            //    {
            //        //以下這邊請自行撰寫你想要的例外處理
            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show(BitConverter.ToString(buffer1));
            //        //以下這邊請自行撰寫你想要的例外處理
            //    }
            //}

            //--------------------------------------
            if (wuzhiPower == WuzhiPower.PowerOff)
            {
            }
            else if (wuzhiPower == WuzhiPower.PowerOn)
            {
            }
            try
            {
                SerialPort sp = (SerialPort)sender;
                SerialPort serialPort1 = sender as SerialPort;
                byte[] buff = new byte[serialPort1.BytesToRead]; //this is to provide the data buffer
                Stream portStream = serialPort1.BaseStream;
                portStream.Read(buff, 0, buff.Length);
                string dataString = Encoding.UTF8.GetString(buff);
                //bool hasData = dataString.Split(' ').Contains("AA"); //this is to check if your data has this, if it doesn't do something
                //You get your data from serial port as byte[]
                //Do something on your data
                byte[] globalBuffer = new byte[8800]; //large buffer, put globally
                buff.CopyTo(globalBuffer, 0);
                //In your data received, use Buffer.BlockCopy to copy data to your globalBuffer
                //if (globalBuffer.Length >= 20)          //Beware the index ---> 20*2+19
                //{ //less than this length, then the data is incomplete
                //  //Do the checking if length is at least 14
                //}

                //byte[] buff = new byte[serialPort1.BytesToRead];
                //receivedata = Encoding.UTF8.GetString(buff);
                //Int32 length = (sender as SerialPort).Read(buff, 0, buff.Length);   //serialPort1.BytesToRead;
                //Array.Resize(ref buff, length);
                //MessageBox.Show(BitConverter.ToString(buff));
                //Displays d = new Displays(DisplayTxt);

                if (globalBuffer.Length >= 20)//legal       //(len != 0)
                {
                    //byte[] buff = new byte[1024]; //len
                    //(sender as SerialPort).Read(buff, 0, 1024); //len

                    if (buff[3] == Form1.Err_checksum_is_wrong)    //0x90
                    {
                        showInfo = "checksum is wrong";
                    }
                    else if (buff[3] == Form1.Err_wrong_params_setting_or_params_overflow) //0xA0
                    {
                        showInfo = "wrong params setting or params overflow";
                    }
                    else if (buff[3] == Form1.Err_cmd_cannot_executed) // 0xB0
                    {
                        showInfo = "cmd cannot executed";
                    }
                    else if (buff[3] == Form1.Err_cmd_is_invaild) // 0xC0
                    {
                        showInfo = "cmd is invaild";
                    }
                    else if (buff[3] == Form1.Err_cmd_is_unknown) // 0xD0
                    {
                        showInfo = "cmd is unknown";
                    }
                    else        // buff[3] == 128 ---> 0x80
                    {
                        showInfo = "WuzhiCmd succeed!!";
                    }
                }
                //richTextBox1.AppendText(showInfo + "\r\n buffToarry -->" + Encoding.UTF8.GetString(buff)); //buff.ToArray().ToString()

                this.Invoke(new Action(() =>
                {
                    receivedata = BitConverter.ToString(buff);      //      AA-01-12-80-00-00-00-00-00-00-00-00-00-00 00-00-00-00-00-3D
                    richTextBox1.AppendText("\n" + DateTime.UtcNow.AddHours(8).ToString(@"MM/dd hh:mm:ss:f") + "receivedata: --->" + receivedata + "\r\n");//receivedata.ToArray().ToString());
                    txtbx_TryCmd.Clear();
                    txtbx_TryCmd.Text += string.Format(@"0x{0:X}", receivedata);
                }));
                //this._WaitForUIThread(() =>
                //{
                //});

                if (receivedata.Length == 0)                //(receivedata.Contains("第三bytes為80=succeed, 90=failure otherwise"))
                {
                    ShowErrMsg("WuZhiCmd Err!!!" + MessageBox.Show("serialport1_DataReceived crash"));
                    throw new Exception("Cmd Err");
                }
            }
            catch (IndexOutOfRangeException indexOut)
            {
                ShowErrMsg(@"DataReceived 回傳Array index在界線之外" + indexOut.Message);
                throw new Exception();
            }
            catch (Exception ex)
            {
                ShowErrMsg(@"serialport1_DataReceived Err" + ex.Message);
                throw ex;
            }
        }

        private void DisplayTxt(byte[] buffer)
        {
        }

        private void parse(List<Byte> tempList)
        {
            MessageBox.Show(tempList[0].ToString());
            //if (tempList[0] == (Byte)S && tempList[tempList.Count - 1] == (Byte)E)
            if (true)
            {
                tempList.RemoveAt(0);
                tempList.RemoveAt(tempList.Count - 1);
                Display d = new Display(DisplayText);
                this.Invoke(d, new Object[] { tempList.ToArray() });
            }
        }

        private Boolean GetFullReceiveData(List<Byte> tempList)
        {
            byte Head = 2;
            if (innerReceiveDataFullLength > 0 && tempList.Count >= tempList.IndexOf(Head) + innerReceiveDataFullLength)
            {
                Byte[] temp = new Byte[innerReceiveDataFullLength];
                Array.Copy(tempList.ToArray(), tempList.IndexOf(Head), temp, 0, temp.Length);
                //innerResponseFullBytes = temp;
                InterpretReceiveData();
                innerReceiveDataFullLength = 0;
                return true;
            }
            else
            { return false; }
        }

        private void InterpretReceiveData()
        {
            //if (CheckData())
            //{
            //    innerResponseCommand = (CommandCode)innerResponseFullBytes[3];
            //    switch (innerResponseCommand)
            //    {
            //        case CommandCode.ReadBlock:
            //            InterpretReadBlock();
            //            break;
            //        case CommandCode.WriteBlock:
            //            ReceiveActionResult(Convert.ToBoolean(innerResponseFullBytes[4]), innerResponseCommand);
            //            break;
            //        case CommandCode.LightControl:
            //            ReceiveActionResult(Convert.ToBoolean(innerResponseFullBytes[4]), innerResponseCommand);
            //            break;
            //        case CommandCode.DataError:
            //            ReceiveErrorData();
            //            break;
            //        default:
            //            throw new ArgumentOutOfRangeException();
            //    }
            //}
        }

        private void GetReceiveDataFullLength(List<Byte> tempList)
        {
            byte Head = 2;
            if (tempList.Count >= 2 && innerReceiveDataFullLength == 0)
            {
                Int32 startIndex = tempList.IndexOf(Head);
                if (startIndex >= 0 && startIndex < tempList.Count - 1)
                {
                    innerReceiveDataFullLength = Convert.ToInt32(tempList[startIndex + 1]) + 2;
                }
            }
        }

        private void DoReceive()
        {
            List<Byte> tempList = new List<Byte>();

            while (Isreceiving)
            {
                Int32 receivedValue = comport.ReadByte();
                switch (receivedValue)
                {
                    case 1:
                        tempList.Clear();
                        tempList.Add((Byte)receivedValue);
                        break;

                    case 2:
                        tempList.Add((Byte)receivedValue);
                        this.parse(tempList);
                        break;

                    case -1:
                        break;

                    default:
                        tempList.Add((Byte)receivedValue);
                        break;
                }
            }
        }

        private void _comportScanning()
        {
            string[] ports = SerialPort.GetPortNames();
            SerialPort[] serialport = new SerialPort[ports.Length];
            foreach (string p in ports)
            {
                int i = Array.IndexOf(ports, p);
                serialport[i] = new SerialPort(); //note this line, otherwise you have no serial port declared, only array reference which can contains real SerialPort object
                serialport[i].PortName = p;
                serialport[i].BaudRate = 9600;
                //serialport[i].Open();
                //Scan inputs for "connectAlready"
                //Enumerable.Range(txtbx_com.Text).Append(serialport[i].ToString());
                //= serialport[i].ToString();
            }
            this.Invoke(new Action(() => { this.richTextBox1.AppendText("comport scann: {x}" + serialport + "\r\n"); }));
            //this._WaitForUIThread(() =>
            //{
            //    richTextBox1.AppendText("comport scann: {x}" + serialport);
            //});
            //1.Scan COM Ports
            //2.Receive inputs from the devices ---> Event handler for DataReceived for your serial port
            //3.When an input has a specific phrase such as "connectAlready",
            //4.Close all ports and create a new one on the port that received the phrase.
            //5.Now that the program knows what COM port the Arduino is on, it can carry on its tasks and send it commands through SerialPorts.
        }

        //-------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------
        //----------------------------------User Input-----------------------------------------------------
        //--------------------------------------UI---------------------------------------------------------
        //-------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------
        //private string getAvailablePorts()
        //{
        //    string[] ss = MulGetHardwareInfo(HardwareEnum.Win32_PnPEntity, "Name");    //Get PC hardware information
        //    System.Collections.ArrayList portArray = new System.Collections.ArrayList();
        //    try
        //    {
        //        for (var i = 0; i < ss.Length; i++)
        //        {
        //            if (ss[i].IndexOf("(") > -1 && ss[i].IndexOf(")") > -1)
        //            {
        //                portArray.Add(ss[i].Substring(ss[i].IndexOf("(") + 1, ss[i].IndexOf(")") - ss[i].IndexOf("(") - 1));
        //            }
        //        }

        //        if (portArray.Count <= 0)
        //            return "";
        //        else
        //            return portArray[0].ToString();
        //    }
        //    catch
        //    {
        //        MessageBox.Show("Get serial ports error!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        return "";
        //    }

        //    //if (portArray.Count > 0)
        //    //{
        //    //    //cmbPortName.Items.AddRange(portArray.ToArray());
        //    //    //cmbPortName.SelectedIndex = 0;
        //    //    //return portArray[0].ToString ();
        //    //}
        //}

        ///// <summary>
        ///// Get PC hardware information
        ///// </summary>
        ///// <param name="hardType"></param>
        ///// <param name="propKey"></param>
        ///// <returns></returns>
        //public static string[] MulGetHardwareInfo(HardwareEnum hardType, string propKey)
        //{
        //    List<string> strs = new List<string>();
        //    try
        //    {
        //        using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from " + hardType))
        //        {
        //            var hardInfos = searcher.Get();
        //            foreach (var hardInfo in hardInfos)
        //            {
        //                if (hardInfo["PNPDeviceID"].ToString().Contains(TWE_LITE_ID))
        //                {
        //                    strs.Add(hardInfo.Properties[propKey].Value.ToString());
        //                }
        //            }
        //            searcher.Dispose();
        //        }
        //        return strs.ToArray();
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //    finally
        //    { strs = null; }
        //}
        private void btn_refresh_Click(object sender, EventArgs e)
        {
            int tryCount = 1;
            do
            {
                try
                {
                    tryCount++;
                    string[] ports = SerialPort.GetPortNames();
                    cmbx_com.Items.Clear();
                    cmbx_com.Items.AddRange(ports);
                    btn_open.Enabled = true;
                    //btn_connection.Enabled = true;
                    btn_sendcmd.Enabled = false;
                    break;
                }
                catch (Exception ex)
                {
                    if (tryCount < iRetryTime)
                    {
                        continue;
                    }
                    MessageBox.Show("CANNOT figure out comport literally.");
                    throw ex;
                }
            } while (cmbx_com.Text.Length == 0);
        }

        private void btn_open_Click(object sender, EventArgs e)
        {
            try
            {
                btn_open.Enabled = false;
                btn_Power.Enabled = true;
                txtbx_Iset.Enabled = true;
                txtbx_Vset.Enabled = true;
                richTextBox1.Clear();
                string receiveData = string.Empty;
                //this._comportScanning();

                if (!mIsConnectedSerialPort)
                {
                    //this.serialPort1 = new System.IO.Ports.SerialPort(@"COM" + this.txtbx_com.Text, Convert.ToInt32(this.txtbx_baudrate.Text.Trim()), Parity.None, 8, StopBits.One);
                    this.serialPort1 = new SerialPort(this.cmbx_com.Text, Convert.ToInt32(this.cmbx_baudrate.Text.Trim()), Parity.None, 8, StopBits.One);
                    
                    serialPort1.DtrEnable = true;           // Gets or sets a value that enables the Data Terminal Ready (DTR) signal during serial communication.
                    serialPort1.RtsEnable = true;           //序列通訊期間是否啟用 Request to Send (RTS)

                    serialPort1.ReadTimeout = 20000;        // stop received after 20000sec
                    //serialPort1.DataReceived += new SerialDataReceivedEventHandler(serialport1_DataReceived);
                    this.mIsConnectedSerialPort = true;
                }
                richTextBox1.AppendText("\n\r SerialPort: --->" + this.serialPort1.PortName + "\n\r BaudRate: ---> " + this.serialPort1.BaudRate);
                if (serialPort1.IsOpen)
                {
                    serialPort1.Close();
                    serialPort1.Dispose();
                    System.Threading.Thread.Sleep(200);
                }
                serialPort1.Open();
                richTextBox1.AppendText("\n\r >>> SerialPort is opened \n\r");
                System.Threading.Thread.Sleep(300);
                // ---------------------------------------------------------------------------
                // ---------------------------------------------------------------------------
                // ---------------------------------------------------------------------------
                //byte[] wuzhiCmd = txtbx_WuzhiCmd.Text.Split(' ').Select(i => Convert.ToByte(i, 16)).ToArray();
                Int32 tryCount = 1;
                do
                {
                    try
                    {
                        tryCount++;
                        //byte[] buff = new byte[length];
                        //serialPort1.Read(cmd, 0, 20);
                        //receiveData = Encoding.Default.GetString(cmd);
                        //richTextBox1.AppendText("\n show receviveData " + receiveData);
                        Thread.Sleep(300);
                        //Save_LOG_data("Sending cmd ---->" + BitConverter.ToString(cmd));
                        //serialPort1.Write(wuzhiCmd, 0, wuzhiCmd.Length);
                        //richTextBox1.AppendText("\n show cmd ---> " + string.Format("0x{0:X}", cmd));
                        break;
                    }
                    catch (Exception ex)
                    {
                        if (iRetryTime < (tryCount - 1))
                        {
                            System.Threading.Thread.Sleep(5);
                            continue;
                        }
                        ShowErrMsg("connect serialport ERR");
                        throw new Exception("serialPort1.Write ERR" + ex.Message);
                    }
                } while (receiveData == null);
            }
            catch (Exception ex)
            {
                ShowErrMsg(@"尚未選擇COMPORT");
                throw ex;
            }
            finally
            {
                //serialPort1.Close();
            }
        }

        private bool Chk_Input_Content()
        {
            if (this.txtbx_addr.Text.Trim().Length >= 4) //或不屬於數字
            {
                this.ShowErrMsg(@"Address format fault.");
                return false;
            }

            if (this.txtbx_Vset.Text.Trim().Length == 0)
            {
                this.ShowErrMsg(@"尚未輸入 Vset");
                return false;
            }

            if (this.txtbx_Iset.Text.Trim().Length == 0)
            {
                this.ShowErrMsg(@"尚未輸入 Iset");
                return false;
            }

            return true;
        }

        private void _IsPowerOn(WuzhiPower status)
        {
            string powerCmd = string.Empty;
            if (status == WuzhiPower.PowerOn)
            {
                powerCmd = "aa 01 22 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ce";
            }
            else if (status == WuzhiPower.PowerOff)
            {
                powerCmd = "aa 01 22 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 cd";
            }
            byte[] pwrCmd = powerCmd.Split(' ').Select(i => Convert.ToByte(i, 16)).ToArray();
            serialPort1.Write(pwrCmd, 0, pwrCmd.Length);
            serialPort1.DataReceived += new SerialDataReceivedEventHandler(serialport1_DataReceived);
        }

        private void _IsConnected(WuzhiConnectStatus status)
        {
            string syncCmd = string.Empty;
            if (status == WuzhiConnectStatus.Connect)
            {
                //aa 01 20 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 cc 是連線????
                //aa 01 29 03 06 00 c8 00 00 00 00 00 00 00 00 00 00 00 00 a5
                //aa 01 2b 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 d6
                syncCmd = "aa 01 20 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 cc";
            }
            else if (status == WuzhiConnectStatus.DisConnect)
            {
                //aa 01 29 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 d4 斷線後持續
                //aa 01 20 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 cb
                syncCmd = "aa 01 20 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 cb";
            }
            byte[] syncmd = syncCmd.Split(' ').Select(i => Convert.ToByte(i, 16)).ToArray();
            serialPort1.Write(syncmd, 0, syncmd.Length);
            serialPort1.DataReceived += new SerialDataReceivedEventHandler(serialport1_DataReceived);
        }

        private void _unknownCmd(string wCmd)
        {
            string unknownCmd = string.Empty;
            //aa 01 20 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 cc
            byte[] xCmd = wCmd.Split(' ').Select(i => Convert.ToByte(i, 16)).ToArray();
            serialPort1.Write(xCmd, 0, xCmd.Length);
            serialPort1.DataReceived += new SerialDataReceivedEventHandler(serialport1_DataReceived);
        }

        private string _decstringToHex(string args)
        {
            //var hexstring = string.Join("", args.Select(i => string.Format("{0:X2}", Convert.ToInt32(i))));
            var decstring = Convert.ToInt32(args);
            string hexstr = string.Format("{0:X}", decstring);
            return hexstr;
        }

        private static void StartToTestRFThreadFunc(object obj)
        {
            Form1 self = obj as Form1;
            //TestItem currentItem = TestFlowSingleton.Instance.GetNextTestItem();
            //self.mIC.SetIsTesting(true);
            //UtilsSingleton.Instance.SetMainForm(self);
            // --- Show Group Status
            self.BeginInvoke(new Action(() =>
            {
                //self.ShowTestGroupInformation(false, currentItem);
            }));
            try
            {
                //self.mIC.StartToTest();
                self.mWuzhiCmd.TakeInitiatives();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string ToHexString(string str)
        {
            var sb = new StringBuilder();

            var bytes = Encoding.Unicode.GetBytes(str);
            foreach (var t in bytes)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString(); // returns: "48656C6C6F20776F726C64" for "Hello world"
        }

        private void cmbx_com_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbx_com.SelectedIndex.ToString().Length != 0)
            {
                cmbx_com.Enabled = true;
            }
        }
        private string _GetComport()
        {
            try
            {
                string comport = "";
                using (System.Management.ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_PnPEntity"))
                {
                    foreach (var hardInfo in searcher.Get())
                    {
                        if (hardInfo.Properties["Name"].Value != null && hardInfo.Properties["Name"].Value.ToString().Contains("(COM") && hardInfo.ToString().Contains("VID_0403") && hardInfo.ToString().Contains("PID_6001"))
                        {
                            comport = hardInfo.Properties["Name"].Value.ToString();
                            //Console.WriteLine(comport);
                        }
                    }

                    if (comport.Length == 0)
                    {
                        //throw new Exception("Not Found wuzhi Comport");
                        return string.Empty;
                    }

                    return comport.Trim().Split('(')[1].Replace(')', ' ').Trim();
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
            string action = ">>> Synchronize serial port: " + this.wuzhiComport;
            try
            {
                
                for (int i = 0; i < 20; i++)
                {
                    this.wuzhiComport = this._GetComport();
                    if (this.wuzhiComport.Trim().Equals(string.Empty))
                    {
                        System.Threading.Thread.Sleep(500);
                    }
                    else
                    {
                        break;
                    }
                }
                if (cmbx_com.Text.Length == 0)
                {
                    string[] ports = SerialPort.GetPortNames();
                    cmbx_com.Items.Clear();
                    cmbx_com.Items.AddRange(ports);
                }
                //comport = new SerialPort("COM5", 9600, Parity.None, 8, StopBits.One);
                //comport.DataReceived += new SerialDataReceivedEventHandler(comport_DataReceived);
                //if (!comport.IsOpen)
                //{
                //    comport.Open();
                //}
            }
            catch (Exception ex)
            {
                throw new Exception("\r\n.....wuzhiComport sync serialport ERR......\r\n");
            }
            finally
            {
                this.richTextBox1.Clear();
            }
            // INIT baudrate
        }

        private void Form1_Closing(object sender, FormClosingEventArgs e)
        {
            DialogResult dr = MessageBox.Show(this, "Be sure to Eixt？", "Window Closing Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Stop);

            if (dr == DialogResult.Yes)
            {
                //try
                //{
                //    if (bWriteLogFlag)
                //    {
                //        this.WriteTotalCountData();
                //    }
                //    ResultCsvSingleton.Instance.DeleteEmptyTestResultCsv();
                //    ResultCsvItSingleton.Instance.DeleteEmptyTestResultCsv();
                //}
                //catch { }

                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
        }

        #region Dashboard

        private void btn_sendcmd_Click(object sender, EventArgs e)
        {
            //https://stackoverflow.com/questions/415291/best-way-to-combine-two-or-more-byte-arrays-in-c-sharp

            int length = serialPort1.BytesToRead;
            //btn_connection.Enabled = true;
            string synchroHead = this._decstringToHex("170"); // string.Format(@"0x{0:X}", this.decstringToHex("170"));   //AA
            string Address = this._decstringToHex(txtbx_addr.Text.Trim());
            double Vset = 0.0d;
            double Iset = 0.0d;
            string receiveData = string.Empty;
            string vsetting = string.Empty;
            string isetting = string.Empty;

            try
            {
                if (txtbx_Iset.Text.Length > 4)
                {   // confirm whether belong to double
                    //double isetting = Convert.ToDouble(txtbx_Iset.Text.Trim().Substring(0, 4));
                    if (Double.TryParse(txtbx_Iset.Text.Trim().Substring(0, txtbx_Iset.TextLength), out Iset))
                    {
                        if (Iset > 5.1)
                        {
                            this.ShowErrMsg("Over Maximum Amp");
                        }
                        else if (Iset <= 0)
                        {
                            this.ShowErrMsg("Under Minimum Amp");
                        }
                    }
                    else
                    {
                        this.ShowErrMsg("Mistaken Current type***");
                    }
                }
                else if (txtbx_Vset.Text.Length > 4)       // coz the protocol wuzhi define merely suit in two bytes hexadecimal
                {
                    //double vsetting = Convert.ToDouble(txtbx_Vset.Text.Trim().Substring(0, txtbx_Vset.TextLength));
                    if (double.TryParse(txtbx_Vset.Text.Trim().Substring(0, txtbx_Vset.TextLength), out Vset))
                    {
                        if (Vset > 55)
                        {
                            this.ShowErrMsg("Over Maximum Voltage");
                        }
                        else if (Vset <= 0)
                        {
                            this.ShowErrMsg("Under Minimum Voltage");
                        }
                    }
                    else
                    {
                        this.ShowErrMsg("Mistaken Voltage type***");
                    }
                }
                else
                {
                    if (!this.Chk_Input_Content())
                    {
                        this.ShowErrMsg("Value Missing~~~!");
                    }
                    else
                    {
                        double.TryParse(txtbx_Vset.Text.Trim(), out Vset);
                        double.TryParse(txtbx_Iset.Text.Trim(), out Iset);
                    }
                    //byte[] bytes = BitConverter.GetBytes(Vset * 100).Concat(BitConverter.GetBytes(Iset * 100)) as byte[];
                    //Buffer.BlockCopy();
                    //string tmp = (string)txtbx_Vset.Text.Trim().Concat(txtbx_Iset.Text.Trim());
                }
            }
            catch (ArgumentOutOfRangeException argex)
            {
                throw new ArgumentOutOfRangeException("Vset or Iset text substring Err!--->" + argex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("_sendCmd ERR!!" + ex.Message);
            }

            //----------------------------------------------
            string[] visetting = WuzhiCmd.Instance.split_VI(Vset, Iset);    //split iset vset into 4 bytes
            byte[] cmd = WuzhiCmd.Instance._VIset_Cmd(synchroHead, Address, visetting);


            Int32 tryCount = 1;
            tryCount++;
            serialPort1.DataReceived += new SerialDataReceivedEventHandler(serialport1_DataReceived);
            do
            {
                try
                {
                    //byte[] buff = new byte[length];
                    //serialPort1.Read(cmd, 0, 20);
                    //receiveData = Encoding.Default.GetString(cmd);
                    //richTextBox1.AppendText("\n show receviveData " + receiveData);
                    Thread.Sleep(300);
                    //Save_LOG_data("Sending cmd ---->" + BitConverter.ToString(cmd));
                    serialPort1.Write(cmd, 0, cmd.Length);
                    //richTextBox1.AppendText("\n show cmd ---> " + string.Format("0x{0:X}", cmd));
                    break;
                }
                catch (Exception ex)
                {
                    if (iRetryTime < (tryCount - 1))
                    {
                        System.Threading.Thread.Sleep(5);
                        continue;
                    }
                    ShowErrMsg("connect serialport ERR");
                    throw new Exception("serialPort1.Write ERR" + ex.Message);
                }
            } while (receiveData == null);
        }

        private void btn_Power_Click(object sender, EventArgs e)
        {
            Button OneShotBtn = (Button)sender;
            switch (OneShotBtn.Text)
            {
                case "OFF":
                    btn_Power.BackColor = Color.DarkRed;
                    btn_Power.Text = "ON";
                    label_PowerBtn.Text = "Power Off";
                    label_PowerBtn.Text.ToUpper();
                    this._IsPowerOn(WuzhiPower.PowerOff);
                    btn_sendcmd.Enabled = false;
                    btn_open.Enabled = false;
                    break;

                case "ON":
                    btn_Power.BackColor = Color.DarkGreen;
                    btn_Power.Text = "OFF";
                    label_PowerBtn.Text = "Power On";
                    label_PowerBtn.Text.ToUpper();
                    this._IsPowerOn(WuzhiPower.PowerOn);
                    if (!btn_open.Enabled)
                    {
                        btn_sendcmd.Enabled = true;
                        break;
                    }
                    else
                    {
                        btn_open.Enabled = true;
                        btn_sendcmd.Enabled = false;
                        break;
                    }

                default:
                    break;
            }
        }

        private void btn_connection_Click(object sender, EventArgs e)
        {
            Button OneShotBtn = (Button)sender;
            switch (OneShotBtn.Text)
            {
                case "DisConnection":
                    btn_connection.BackColor = Color.DarkRed;
                    btn_connection.Text = "DisConnection";
                    this._IsConnected(WuzhiConnectStatus.DisConnect);
                    //btn_sendcmd.Enabled = false;
                    //btn_open.Enabled = false;
                    break;

                case "Connection":
                    btn_connection.BackColor = Color.DarkGreen;
                    btn_connection.Text = "Connection";
                    this._IsConnected(WuzhiConnectStatus.Connect);
                    //btn_connection.Enabled = false;
                    if (!btn_open.Enabled)
                    {
                        btn_sendcmd.Enabled = true;
                        break;
                    }
                    else
                    {
                        btn_open.Enabled = true;
                        btn_sendcmd.Enabled = false;
                        break;
                    }

                default:
                    break;
            }
        }

        #endregion Dashboard

        #region DebugPage Button

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            byte[] wuzhiCmd = txtbx_WuzhiCmd.Text.Split(' ').Select(i => Convert.ToByte(i, 16)).ToArray();
            byte[] aabyte = new byte[20];
            byte[] cmd = new byte[20];      // wuzhiCmd.Length = 20
            List<string> cmdlst = new List<string>();
            //Encoding utf_8 = Encoding.UTF8;
            //byte[] result = cmdlst.SelectMany(x => utf_8.GetBytes(x)).ToArray();
            //richTextBox1.AppendText(cmdlst.ToArray().ToString());
            //richTextBox1.AppendText(result[0].ToString() + result[1].ToString);

            //string totalStr = string.Format("0x{0:X}", total);
            //richTextBox1.AppendText(string.Format("0x{0:X}", total) + "\r\n" + string.Format("{0:X}", total));
            //this._ByteArrayToString();
            //string.Format("0x{0:X}", cmdlst);

            //if ((hex.Length % 2) != 0)
            //{
            //    hex = "0" + hex;
            //}

        }

        private void btn_hexaddition_Click(object sender, EventArgs e)
        {
             this.richTextBox1.Clear();
            this.txtbx_TryCmd.Clear();
            try
            {
                Int32 hexAdd = WuzhiCmd.Instance._hexAddition(txtbx_WuzhiCmd.Text.Split(' '));
                txtbx_TryCmd.Text += "Hex Addition" + string.Format("0x{0:X}", hexAdd);
            }
            catch (Exception ex)
            {

                throw new Exception("Hex Addition ERR!! --->" + ex.Message);
            }
        }

        private void btn_TryCmd_Click(object sender, EventArgs e)
        {
            try
            {
                if (serialPort1.IsOpen)
                {
                    //serialPort1.DataReceived += new SerialDataReceivedEventHandler(serialport1_DataReceived);
                    //byte[] wuzhiCmd = txtbx_WuzhiCmd.Text.Split(' ').Select(i => Convert.ToByte(i, 16)).ToArray();
                    //serialPort1.Write(wuzhiCmd, 0, wuzhiCmd.Length);
                    this._unknownCmd(txtbx_WuzhiCmd.Text.Trim());
                }
            }
            catch (InvalidOperationException invaild)
            {
                throw new Exception("Comport already closed" + invaild.Message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion DebugPage Button
    }
}