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

namespace MiniPwrSupply
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private static Mutex mutex = new Mutex();
        private volatile bool mGet_Start;
        private StringBuilder receiveCall = new StringBuilder();
        private bool mIsConnectedSerialPort = false;

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

        private void ShowErrMsg(string errMsg)
        {
            MessageBox.Show(errMsg, @"Error Occur", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                richTextBox1.Clear();
                if (!mIsConnectedSerialPort)
                {
                    this.serialPort1 = new System.IO.Ports.SerialPort(@"COM" + this.txtbx_com.Text, Convert.ToInt32(this.txtbx_baudrate.Text.Trim()), Parity.None, 8, StopBits.One);
                    serialPort1.ReadTimeout = 100;
                    serialPort1.DataReceived += serialport1_DataReceived;
                    this.mIsConnectedSerialPort = true;
                }
                richTextBox1.AppendText("SerialPort --->" + this.serialPort1.PortName + "\nBaudRate:" + this.serialPort1.BaudRate);
                if (serialPort1.IsOpen)
                {
                    serialPort1.Close();
                    serialPort1.Dispose();
                    System.Threading.Thread.Sleep(200);
                }
                serialPort1.Open();
                richTextBox1.AppendText("\nSerialPort is opened");
                //System.Threading.Thread.Sleep(500);
                //serialPort1.Write(@"#START%");
                System.Threading.Thread.Sleep(300);
            }
            catch (Exception ex)
            {
                ShowErrMsg("connect serialport ERR");
                throw ex;
            }
        }

        private void btn_sendcmd_Click(object sender, EventArgs e)
        {
            string sResult = string.Empty;
            string cmdName = "Voltage_current";
            string setVoltage = txtbx_Vset.Text;
            string setIcurrent = txtbx_Iset.Text;
            int length = serialPort1.BytesToRead;
            string receiveData = string.Empty;

            try
            {
                byte[] cmd = new byte[20];
                cmd[0] = 0xAA;
                cmd[1] = 0x01;
                cmd[2] = 0x22;
                cmd[3] = 0x01;
                cmd[4] = 0x00;
                cmd[5] = 0x00;
                cmd[6] = 0x00;
                cmd[7] = 0x00;
                cmd[8] = 0x00;      // Voltage
                cmd[9] = 0x00;      //  current hex+
                cmd[10] = 0x00;     // current hex+
                cmd[11] = 0x00;
                cmd[12] = 0x00;
                cmd[13] = 0x00;
                cmd[14] = 0x00;
                cmd[15] = 0x00;
                cmd[16] = 0x00;
                cmd[17] = 0x00;
                cmd[18] = 0x00;
                cmd[19] = 0xce;
                byte[] result = null;
                //this._SendCmd(cmd, ref result);
                try
                {
                    do
                    {
                        //byte[] buff = new byte[length];
                        //serialPort1.Read(cmd, 0, 20);
                        //receiveData = Encoding.Default.GetString(cmd);
                        //richTextBox1.AppendText("\n show receviveData " + receiveData);
                        Thread.Sleep(300);
                        serialPort1.Write(cmd, 0, cmd.Length);
                        richTextBox1.AppendText("\n show cmd " + cmd);
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
                serialPort1.Close();
            }
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
            int length = serialPort1.BytesToRead;
            string receivedata = string.Empty;
            string datatime = string.Empty;
            try
            {
                SerialPort sp = (SerialPort)sender;
                string indata = sp.ReadExisting();

                //if (IsFirstTest == true)
                //{
                //    receiveCall.Clear();
                //    receiveCall.Append(receivedata);

                //    Thread.Sleep(1000);
                //    serialPort1.Write(@"#START%");
                //    richTextBox1.AppendText(@"Receive #START%");
                //    Thread.Sleep(200);

                //    receiveCall.Clear();
                //    IsFirstTest = false;
                //}
                if (length != 0)
                {
                    byte[] buff = new byte[length];
                    serialPort1.Read(buff, 0, length);
                    receivedata = Encoding.Default.GetString(buff);
                }
                switch (receivedata)
                {
                    //case String a when a.Contains()

                    default:
                        receiveCall.Clear();
                        receiveCall.Append(receivedata);
                        richTextBox1.AppendText("Data acquire from : " + indata);
                        break;
                }
                this.Invoke(new Action(() => { this.richTextBox1.AppendText(receivedata + "\r\n"); }));
            }
            catch (Exception ex)
            {
                ShowErrMsg(@"serialport1_DataReceived Err" + ex.Message);
                throw ex;
            }
        }
    }
}