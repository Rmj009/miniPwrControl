using MiniPwrSupply.WuizhiCmd;
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

        private SerialPort comport;
        private Int32 totalLength = 0;

        private delegate void Display(Byte[] buffer);

        private Boolean Isreceiving = false;
        //--------------------------

        private IWuzhiCmd mWuzhiCmd = null;
        private static Mutex mutex = new Mutex();
        private volatile bool mGet_Start;
        private StringBuilder receiveCall = new StringBuilder();
        private Action<string, UInt32> mLogCallback = null;
        private bool mIsConnectedSerialPort = false;
        public Form1 mInstatnce = null;
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

        public void Save_LOG_data(string sTtestResult, bool isTitle = false, bool isCustom = false, bool isError = false)
        {
            uint type = isTitle ? RFTestTool.Util.MSG.TITLE : RFTestTool.Util.MSG.NORMAL;
            if (type == RFTestTool.Util.MSG.TITLE)
            {
                sTtestResult = "*** " + sTtestResult + " ***";
            }
            type = isCustom ? RFTestTool.Util.MSG.CUSTOM : type;
            type = isError ? RFTestTool.Util.MSG.ERROR : type;

            this.mLogCallback(sTtestResult, type);
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
        private Int32 _hexAddition(string[] strArray)
        {
            if (strArray.Length == 20)
            {
                Int32 total = 0;
                //byte[] hexAdded = Enumerable.Range(0, wuzhiCmd.Length - 2).Where(x => x % 2 == 0).Select(x => Convert.ToByte(wuzhiCmd.Substring(x, 2), 16)).ToArray();  //wuzhiCmd.Substring(x,2)
                byte[] hexAdded = Enumerable.Range(0, txtbx_WuzhiCmd.Text.Split(' ').Length - 1).Select(x => Convert.ToByte(txtbx_WuzhiCmd.Text.Split(' ')[x], 16)).ToArray();

                total = hexAdded.Sum(x => x);

                //richTextBox1.AppendText("wuzhiCmd _hexAddition ---> " + string.Format("0x{0:X}", total));

                return total;
            }
            else if (strArray.Length == 19)
            {
                Int32 total = 0;
                byte[] hexAdded = Enumerable.Range(0, strArray.Length - 2).Select(x => Convert.ToByte(strArray[x], 16)).ToArray();
                total = hexAdded.Sum(x => x);

                //richTextBox1.AppendText("\r\n strArray _hexAddition ---> " + string.Format("0x{0:X}", total));

                return total;
            }
            else
            {
                return 0;
            }
            //else if (1 == 1)
            //{
            //    richTextBox1.AppendText("IsReadyOnly return TRUE ---> array,\r\n while FLASE ---> list \r\n" + tiidas.IsReadOnly.ToString() + "\r\n");
            //    //foreach (Tiida item in tiidas)
            //    //{
            //    //    richTextBox1.AppendText(item.ToString() + " ");
            //    //}
            //}
        }

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

            try
            {
                SerialPort sp = (SerialPort)sender;
                SerialPort serialPort1 = sender as SerialPort;
                byte[] buff = new byte[serialPort1.BytesToRead]; //this is to provide the data buffer
                Stream portStream = serialPort1.BaseStream;
                portStream.Read(buff, 0, buff.Length);
                string dataString = Encoding.UTF8.GetString(buff);
                bool hasData = dataString.Split(' ').Contains("AA"); //this is to check if your data has this, if it doesn't do something
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
                //richTextBox1.AppendText("-------------Data Received!-------------");
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
                    richTextBox1.AppendText(" \r\n receivedata: --->" + receivedata + "\r\n");//receivedata.ToArray().ToString());
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
            if (this.txtbx_addr.Text.Trim().Length >= 4)
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

        private void btn_sendcmd_Click(object sender, EventArgs e)
        {
            //https://stackoverflow.com/questions/415291/best-way-to-combine-two-or-more-byte-arrays-in-c-sharp

            int length = serialPort1.BytesToRead;
            string synchroHead = this._decstringToHex("170"); // string.Format(@"0x{0:X}", this.decstringToHex("170"));   //AA
            string Address = this._decstringToHex(txtbx_addr.Text.Trim());
            double Vset = 0.0d;
            double Iset = 0.0d;
            string receiveData = string.Empty;
            string vsetting = string.Empty;
            string isetting = string.Empty;
            string vsetting1 = string.Empty;
            string vsetting2 = string.Empty;
            string isetting1 = string.Empty;
            string isetting2 = string.Empty;
            string checksum = string.Empty;

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
                throw new Exception("Sendcmd ERR!!" + ex.Message);
            }

            string[] visetting = WuzhiCmd.Instance.split_VI(Vset, Iset);    //split iset vset into 4 bytes
            vsetting1 = visetting[0];
            vsetting2 = visetting[1];
            isetting1 = visetting[2];
            isetting2 = visetting[3];
            //Thread thread = new Thread(new ParameterizedThreadStart(StartToTestRFThreadFunc));
            //thread.Start(this);
            //----------------------------------------------
            string[] PowerCmd = {
                 synchroHead,
                    Address,
                    "2C",
                    "14",
                    "50",
                    "14",
                    "50",
                    vsetting1,
                    vsetting2,
                    isetting1,
                    isetting2,
                    "00",
                    "00",
                    "00",
                    "42",
                    "00",
                    "00",
                    "00",
                    "00"
            };
            //mPowerCmd = Enumerable.Range(0, PowerCmd.Length).Select(i => this._decstringToHex(PowerCmd[i])).ToArray();
            //  00 ---> baudrate
            //var temp = PowerCmd;
            //var temp2 = txtbx_WuzhiCmd.Text.Split(' ');
            string chksum = string.Format("0x{0:X}", this._hexAddition(PowerCmd));
            checksum = chksum.Substring(chksum.Length - 2, 2);
            //----------------------------------------------
            byte[] cmd = new byte[20];
            cmd[0] = Convert.ToByte(synchroHead, 16); //0xAA
            cmd[1] = Convert.ToByte(Address, 16);   //0x01
            cmd[2] = 0x2C;
            cmd[3] = 0x14;
            cmd[4] = 0x50;
            cmd[5] = 0x14;
            cmd[6] = 0x50;
            cmd[7] = Convert.ToByte(vsetting1, 16); //0x
            cmd[8] = Convert.ToByte(vsetting2, 16); //0x
            cmd[9] = Convert.ToByte(isetting1, 16); //0x
            cmd[10] = Convert.ToByte(isetting2, 16); //0x
            cmd[11] = 0x00;
            cmd[12] = 0x00;
            cmd[13] = 0x00;
            cmd[14] = 0x42;
            cmd[15] = 0x00;
            cmd[16] = 0x00;
            cmd[17] = 0x00;
            cmd[18] = 0x00;
            cmd[19] = Convert.ToByte(string.Format("0x{0:X}", checksum), 16);

            //----------------------------------------------

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

        private void Form1_Load(object sender, EventArgs e)
        {
            this.richTextBox1.Clear();
            //this._comportScanning();
            // INIT port
            try
            {
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
                throw ex;
            }
            // INIT baudrate
        }

        private void Form1_Closing(object sender, FormClosingEventArgs e)
        {
            DialogResult dr = MessageBox.Show(this, "Be sure to Eixt？", "Window Closing Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Stop);

            if (dr == DialogResult.Yes)
            {
                //if (this.mIC != null)
                //{
                //    this.mIC.Dispose(); // IC Free Resource
                //    this.mIC.IDispose(); // Instrument Free Resource
                //}

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

        #region DebugPage Button

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            byte[] wuzhiCmd = txtbx_WuzhiCmd.Text.Split(' ').Select(i => Convert.ToByte(i, 16)).ToArray();
            //byte[] hexAdded = Enumerable.Range(0, txtbx_WuzhiCmd.Text.Split(' ').Length - 2).Select(x => Convert.ToByte(txtbx_WuzhiCmd.Text.Split(' ')[x], 16)).ToArray();  //wuzhiCmd.Substring(x,2)

            //richTextBox1.AppendText(string.Format("{0:x}", total));
            //richTextBox1.AppendText(wuzhiCmd.Length.ToString());
            byte[] aabyte = new byte[20];
            byte[] cmd = new byte[20];      // wuzhiCmd.Length = 20
            List<string> cmdlst = new List<string>();
            //byte[] hexAdded = Enumerable.Range(0, wuzhiCmd.Length - 2).Where(x => x % 2 == 0).Select(x => Convert.ToByte(wuzhiCmd.Substring(x, 2), 16)).ToArray();  //wuzhiCmd.Substring(x,2)
            //int total = hexAdded.Sum(x => x);
            //foreach (string item in wuzhiCmd.Split(' '))
            //{
            //    string[] tmp = null;
            //    cmdlst.Add(string.Concat("0x" + item) + string.Empty);
            //    Convert.FromBase64String(string.Concat("0x" + item) + string.Empty);
            //    //(2, wuzhiCmd.Length);
            //    //tmp = string.Format("0x{0:X}", item);
            //    //string.Concat("0x" + item)
            //}
            //var aa = "aa";
            //for (int i = 1; i < 21; i++)
            //{
            //    //cmd[i] =
            //    string.Format("0x{0:X}", aa);  //string.Format();
            //}

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

            //return Enumerable.Range(0, hex.Length)
            //                 .Where(x => x % 2 == 0)
            //                 .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
            //                 .ToArray();
        }

        private void btn_hexaddition_Click(object sender, EventArgs e)
        {
            this.richTextBox1.Clear();
            this._hexAddition(txtbx_WuzhiCmd.Text.Split(' '));
        }

        private void btn_TryCmd_Click(object sender, EventArgs e)
        {
            byte[] wuzhiCmd = txtbx_WuzhiCmd.Text.Split(' ').Select(i => Convert.ToByte(i, 16)).ToArray();
        }

        #endregion DebugPage Button
    }
}