using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.IO;

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

        private static Mutex mutex = new Mutex();
        private volatile bool mGet_Start;
        private StringBuilder receiveCall = new StringBuilder();
        private Action<string, UInt32> mLogCallback = null;
        private bool mIsConnectedSerialPort = false;
        private static int iRetryTime = 6;

        private void ShowErrMsg(string errMsg)
        {
            MessageBox.Show(errMsg, @"Error Occur", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private string _hexAddition(byte[] hexBytes)
        {
            int len;
            byte[] hexByte = new byte[hexBytes.Length];
            string wuzhiCmd = this._ByteArrayToString(hexBytes);
            byte[] hexAdded = Enumerable.Range(0, wuzhiCmd.Length - 2).Where(x => x % 2 == 0).Select(x => Convert.ToByte(wuzhiCmd.Substring(x, 2), 16)).ToArray();  //wuzhiCmd.Substring(x,2)
            int total = hexAdded.Sum(x => x);
            //string totalStr = string.Format("0x{0:X}", total);
            return string.Format("0x{0:X}", total);
        }

        private void _SendCmd(byte[] cmd, ref byte[] result) //, Binary_Serail_DLL.BinaryFormatCheck callback = null
        {
            Console.WriteLine("[ SEND ] " + this._ByteArrayToString(cmd).Trim());
            //this.Save_LOG_data("[ SEND ] " + this._ByteArrayToString(cmd).Trim());

            result = null;

            //Binary_Serail_DLL.BINARY_SERIAL_RESULT serialResult = new Binary_Serail_DLL.BINARY_SERIAL_RESULT();
            IntPtr callbackPtr = IntPtr.Zero;
            //if (callback == null)
            //{
            //    callbackPtr = Marshal.GetFunctionPointerForDelegate(this.mBinaryCheckCallback);
            //    callbackPtr = Marshal.GetFunctionPointerForDelegate(this.mBinaryCheckCallback);
            //}
            //else
            //{
            //    callbackPtr = Marshal.GetFunctionPointerForDelegate(callback);
            //}

            //if (!Binary_Serail_DLL.Binary_TransferReceive(cmd, (uint)cmd.Length, callbackPtr, ref serialResult, SERIAL_TIME_OUT))
            //{
            //    throw new Exception("Serial Error !! Command : " + this._ByteArrayToString(cmd).Trim());
            //}

            //result = new byte[serialResult.readLen];
            //Buffer.BlockCopy(serialResult.readData, 0, result, 0, serialResult.readLen);

            Console.WriteLine("[--RESP--] " + this._ByteArrayToString(result).Trim());
            //this.Save_LOG_data("[  RESP  ] " + this._ByteArrayToString(result).Trim());
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
            int len = serialPort1.BytesToRead;
            try
            {
                SerialPort sp = (SerialPort)sender;
                SerialPort serialPort1 = sender as SerialPort;
                byte[] data = new byte[serialPort1.BytesToRead]; //this is to provide the data buffer
                Stream portStream = serialPort1.BaseStream;
                portStream.Read(data, 0, data.Length);
                string dataString = Encoding.UTF8.GetString(data);
                bool hasData = dataString.Split(' ').Contains("AA"); //this is to check if your data has this, if it doesn't do something
                //You get your data from serial port as byte[]
                //Do something on your data
                byte[] globalBuffer = new byte[4000]; //large buffer, put globally

                //In your data received, use Buffer.BlockCopy to copy data to your globalBuffer
                if (globalBuffer.Length >= 20)          //Beware the index ---> 20*2+19
                { //less than this length, then the data is incomplete
                  //Do the checking if length is at least 14
                }

                //byte[] buff = new byte[serialPort1.BytesToRead];
                //richTextBox1.AppendText("-------------Data Received!-------------");
                //receivedata = Encoding.UTF8.GetString(buff);
                //Int32 length = (sender as SerialPort).Read(buff, 0, buff.Length);   //serialPort1.BytesToRead;
                //Array.Resize(ref buff, length);
                //MessageBox.Show(BitConverter.ToString(buff));
                //Displays d = new Displays(DisplayTxt);

                if (globalBuffer.Length >= 20)//legal       //(len != 0)
                {
                    byte[] buff = new byte[1024]; //len
                    (sender as SerialPort).Read(buff, 0, 1024); //len
                    if (buff[3] == 0x90)       
                    {
                        richTextBox1.AppendText("checksum is wrong");
                    }
                    else if (buff[3] == 0xA0)
                    {
                        richTextBox1.AppendText("wrong params setting or params overflow");
                    }
                    else if (buff[3] == 0xB0)
                    {
                        richTextBox1.AppendText("cmd cannot executed");
                    }
                    else if (buff[3] == 0xC0)
                    {
                        richTextBox1.AppendText("cmd is invaild");
                    }
                    else if (buff[3] == 0xD0)
                    {
                        richTextBox1.AppendText("cmd is unknown");
                    }
                    else        // buff[3] == 0x80
                    {
                        richTextBox1.AppendText("WuzhiCmd succeed!!");
                    }

                    receivedata = BitConverter.ToString(buff);

                }
                this._WaitForUIThread(() =>
                {
                    //richTextBox1.AppendText("\r\n buffToarry -->" + Encoding.UTF8.GetString(buff)); //buff.ToArray().ToString()
                    richTextBox1.AppendText(" \r\n receivedata --->" + receivedata);//receivedata.ToArray().ToString());
                });

                if (receivedata.Length == 0)//(receivedata.Contains("第三bytes為80=succeed, 90=failure otherwise"))
                {
                    ShowErrMsg("WuZhiCmd Err!!!" + MessageBox.Show("serialport1_DataReceived crash"));
                    throw new Exception("Cmd Err");
                }
                //this.Invoke(new Action(() => { this.richTextBox1.AppendText("Invoke receivedata --->" + receivedata + "\r\n"); }));
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
                serialport[i].Open();
                //Scan inputs for "connectAlready"
                //Enumerable.Range(txtbx_com.Text).Append(serialport[i].ToString());
                    //= serialport[i].ToString();

            }
            this._WaitForUIThread(() => 
            {
                richTextBox1.AppendText("comport scann: {x}" + serialport);
            });
            //1.Scan COM Ports
            //2.Receive inputs from the devices ---> Event handler for DataReceived for your serial port
            //3.When an input has a specific phrase such as "connectAlready",
            //4.Close all ports and create a new one on the port that received the phrase.
            //5.Now that the program knows what COM port the Arduino is on, it can carry on its tasks and send it commands through SerialPorts.

        }

        private void btn_refresh_Click(object sender, EventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void btn_open_Click(object sender, EventArgs e)
        {
            try
            {
                btn_open.Enabled = false;
                txtbx_Iset.Enabled = true;
                txtbx_Vset.Enabled = true;
                richTextBox1.Clear();
                string receiveData = string.Empty;
                serialPort1.DtrEnable = true;
                serialPort1.RtsEnable = true;

                this._comportScanning();

                if (!mIsConnectedSerialPort)
                {
                    this.serialPort1 = new System.IO.Ports.SerialPort(@"COM" + this.txtbx_com.Text, Convert.ToInt32(this.txtbx_baudrate.Text.Trim()), Parity.None, 8, StopBits.One);
                    serialPort1.ReadTimeout = 100;
                    serialPort1.DataReceived += new SerialDataReceivedEventHandler(serialport1_DataReceived);
                    this.mIsConnectedSerialPort = true;
                }
                richTextBox1.AppendText("SerialPort: --->" + this.serialPort1.PortName + "\nBaudRate: ---> " + this.serialPort1.BaudRate);
                if (serialPort1.IsOpen)
                {
                    serialPort1.Close();
                    serialPort1.Dispose();
                    System.Threading.Thread.Sleep(200);
                }
                serialPort1.Open();
                richTextBox1.AppendText("\n SerialPort is opened \n");
                System.Threading.Thread.Sleep(300);
                // ---------------------------------------------------------------------------
                // ---------------------------------------------------------------------------
                // ---------------------------------------------------------------------------
                byte[] cmd = new byte[20];                                                                                                              // wuzhiCmd.Length = 20
                cmd[0] = 0xaa;
                cmd[1] = 0x01;
                cmd[2] = 0x2C;
                cmd[3] = 0x14;
                cmd[4] = 0x50;
                cmd[5] = 0x14;
                cmd[6] = 0x50;
                cmd[7] = 0x00; //Vset[0];       //0x00
                cmd[8] = 0xc8; //Vset[1];      //0x00;      // Voltage
                cmd[9] = 0x0c; //Iset[0];      //0x00;      //  current hex+
                cmd[10] = 0x80; //Iset[1];       //0x00;     // current hex+
                cmd[11] = 0x00;
                cmd[12] = 0x00;
                cmd[13] = 0x00;
                cmd[14] = 0x42;
                cmd[15] = 0x00;
                cmd[16] = 0x00;
                cmd[17] = 0x00;
                cmd[18] = 0x00;
                cmd[19] = 0x35;
                //this._SendCmd(cmd, ref result);
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
            catch (Exception ex)
            {
                ShowErrMsg("connect serialport ERR");
                throw ex;
            }
            finally
            {
                btn_sendcmd.Enabled = true;
            }
        }

        private void btn_sendcmd_Click(object sender, EventArgs e)
        {
            //string sResult = string.Empty;
            //string cmdName = "Voltage_current";
            //int length = serialPort1.BytesToRead;
            string receiveData = string.Empty;
            byte[] wuzhiCmd = txtbx_WuzhiCmd.Text.Split(' ').Select(i => Convert.ToByte(i, 16)).ToArray();
            byte[] Vset = Encoding.UTF8.GetBytes(txtbx_Vset.Text);  // convert string to HEX
            byte[] Iset = Encoding.UTF8.GetBytes(txtbx_Iset.Text);
            try
            {

                byte[] cmd = new byte[20];      // wuzhiCmd.Length = 20
                //cmd = Enumerable.Range(0, wuzhiCmd.Length - 2).Where(x => x % 2 == 0).Select(x => Convert.ToByte(wuzhiCmd.Substring(x, 2), 16)).ToArray();
                //foreach (byte item in cmd)
                //{
                //    item.Substring(2, wuzhiCmd.Length);
                //    string.Format("0x{0:X}", item).Append();
                //}
                
                cmd[0] = 0xAA;
                cmd[1] = 0x01;
                cmd[2] = 0x22;
                cmd[3] = 0x00;
                cmd[4] = 0x00;
                cmd[5] = 0x00;
                cmd[6] = 0x00;
                cmd[7] = 0x00; //Vset[0];       //0x00
                cmd[8] = 0x00; //Vset[1];      //0x00;      // Voltage
                cmd[9] = 0x00; //Iset[0];      //0x00;      //  current hex+
                cmd[10] = 0x00; //Iset[1];       //0x00;     // current hex+
                cmd[11] = 0x00;
                cmd[12] = 0x00;
                cmd[13] = 0x00;
                cmd[14] = 0x00;
                cmd[15] = 0x00;
                cmd[16] = 0x00;
                cmd[17] = 0x00;
                cmd[18] = 0x00;
                cmd[19] = 0xcd;
                //this._SendCmd(cmd, ref result);
                try
                {
                    do
                    {
                        cmd.GetType();
                        if (cmd == wuzhiCmd)
                        {
                            ShowErrMsg("OKOKOK");
                        }
                        //byte[] buff = new byte[length];
                        //serialPort1.Read(cmd, 0, 20);
                        //receiveData = Encoding.Default.GetString(cmd);
                        //richTextBox1.AppendText("\n show receviveData " + receiveData);
                        Thread.Sleep(300);
                        //Save_LOG_data("Sending cmd ---->" + BitConverter.ToString(cmd));
                        serialPort1.Write(cmd, 0, cmd.Length);
                        //richTextBox1.AppendText("\n show cmd ---> " + string.Format("0x{0:X}", cmd));
                        break;
                    } while (receiveData == null);
                }
                catch (Exception)
                {
                    throw;
                }
                //sResult = this._ByteArrayToString(result);
            }
            catch (Exception ex)
            {
                throw new Exception("cmd Err!!" + ex.Message);
            }
            finally
            {
                //serialPort1.Close();
                btn_open.Enabled = true;
                btn_sendcmd.Enabled = false;
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
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            byte[] wuzhiCmd = txtbx_WuzhiCmd.Text.Split(' ').Select(i => Convert.ToByte(i, 16)).ToArray();
            richTextBox1.AppendText(wuzhiCmd.Length.ToString());

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
            var aa = "aa";
            for (int i = 1; i < 21; i++)
            {
                //cmd[i] = 
                    string.Format("0x{0:X}", aa);  //string.Format();
            }

            Console.WriteLine("cmdddd: >>> ", cmd);



            Encoding utf_8 = Encoding.UTF8;
            byte[] result = cmdlst.SelectMany(x => utf_8.GetBytes(x)).ToArray();
            richTextBox1.AppendText(cmdlst.ToArray().ToString());
            //richTextBox1.AppendText(result[0].ToString() + result[1].ToString);

            //string totalStr = string.Format("0x{0:X}", total);
            //richTextBox1.AppendText(string.Format("0x{0:X}", total) + "\r\n" + string.Format("{0:X}", total));
            //this._ByteArrayToString();
            string.Format("0x{0:X}", cmdlst);


            //if ((hex.Length % 2) != 0) 
            //{
            //    hex = "0" + hex;
            //}

            //return Enumerable.Range(0, hex.Length)
            //                 .Where(x => x % 2 == 0)
            //                 .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
            //                 .ToArray();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //this._comportScanning();
        }
    }
}