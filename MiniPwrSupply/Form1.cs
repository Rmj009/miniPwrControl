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
using System.Management;
using System.Diagnostics;
using MiniPwrSupply.Config;
using static MiniPwrSupply.Config.wuzhiConfig;
using System.Collections;
using MiniPwrSupply.Singleton;

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

        public bool IsCheckSum_Legal { get; set; }

        public enum WuzhiConnectStatus
        {
            Connect,
            DisConnect
        }

        private string title = string.Empty;
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
        private const Int32 FrameLen = 20;
        private static int iRetryTime = 6;

        //private const Int32 A = 170;
        private static Int32 checksum = 0;

        public double ListenVoltage = -1;                    //impossible to be negative
        private double vGap = 0.0d;
        public byte[] globalBuffer = new byte[20];  //large buffer, put globally
        public byte[] wzTx = null;
        private Queue<byte> queue = new Queue<byte>(); //AA - 01 - 12 - 80 - 00 - 00 - 00 - 00 - 00 - 00 - 00 - 00 - 00 - 00 00 - 00 - 00 - 00 - 00 - 3D

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

        private void ShowErrMsg(string errMsg)
        {
            MessageBox.Show(errMsg, @"Error Occur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public enum wzCodeName : byte                   // Reference from wz5005 official document
        {
            syncHead = 0xAA,
            read_Input_Volt = 0x29,
            read_Now_Volt = 0x2A,
            set_Read_Volt = 0x2C
        }

        private void DisplayText(Byte[] buffer)
        {
            textBox1.Clear();
            string receivedata = string.Empty;
            textBox1.Text += String.Format("{0}{1}", BitConverter.ToString(buffer), Environment.NewLine);
            totalLength = buffer.Length;
            label_DataReceived.Text = totalLength.ToString();
            try
            {
                //this.Invoke(new Action(() =>
                //{
                //}));
                //this._WaitForUIThread(() =>
                //{
                //});
                receivedata = BitConverter.ToString(buffer);      //      AA-01-12-80-00-00-00-00-00-00-00-00-00-00 00-00-00-00-00-3D
                txtbx_TryCmd.Text += string.Format(@"0x{0:X}", receivedata);
                string CksumResult = this._IsCheckValid(buffer) ? @" YES " : @" NO ";
                string buffValidity = WuzhiCmd.Instance.IsbufferValid(buffer);

                LogSingleton.Instance.WriteLog(@"Display ReceiveData ---> " + BitConverter.ToString(buffer), LogSingleton.wzMEASURE_VALUE);
                LogSingleton.Instance.WriteLog(@"Checksum ---> " + CksumResult, LogSingleton.wzMEASURE_VALUE);
                LogSingleton.Instance.WriteLog(@"Is buffer Valid ---> " + buffValidity, LogSingleton.wzMEASURE_VALUE);

                richTextBox1.AppendText("\n-----------------------------------" + title + "-----------------------------------\n");
                richTextBox1.AppendText("\n" + DateTime.UtcNow.AddHours(8).ToString(@"MM/dd hh:mm:ss") + " " + buffValidity + "\r\n");
                richTextBox1.AppendText("\r\n" + " Receive Bytes ---> " + string.Format("{0}{1}", receivedata, Environment.NewLine));
                richTextBox1.AppendText("\r\n" + " Is CheckSum Legal: ---> " + CksumResult + "\r\n");
                richTextBox1.AppendText("\r\n" + " Voltage Gap Between wzcmd & listen: ---> " + vGap.ToString() + "\r\n");
            }
            catch (IndexOutOfRangeException indexOut)
            {
                ShowErrMsg(@"DataReceived 回傳Array index在界線之外" + indexOut.Message);
                throw new Exception("DataReceived Crush" + indexOut.Message);
            }
            catch (Exception ex)
            {
                ShowErrMsg(@"DisplayText Err" + ex.Message);
                throw new Exception("DisplayText Err!! ---> " + ex.Message);
            }
            finally
            {
                ///
                serialPort1.DiscardInBuffer();
                //serialPort1.DiscardOutBuffer();
                ///
                title = string.Empty;
                txtbx_TryCmd.Clear();
            }
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

        private bool _IsCheckValid(byte[] buffer)
        {
            int itsChksum = -1;
            checksum = buffer[buffer.Length - 1];
            int cksums = Enumerable.Range(0, buffer.Length - 2).ToArray().Sum(i => buffer[i]);
            string chksum = string.Format(@"0x{0:X}", cksums);
            itsChksum = Convert.ToInt32(chksum.Substring(chksum.Length - 2, 2), 16);
            //Stream portStream = serialPort1.BaseStream;
            //receivedata = Encoding.UTF8.GetString(buffer);
            //portStream.Read(buffer, 0, buffer1.Length);
            return itsChksum == checksum ? !IsCheckSum_Legal : IsCheckSum_Legal;
        }

        private void serialport1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {   //確認Checksum合法, 確認每次buffer接收到完整data, 最後Clone接收Bytes到globalbuffer
            int offset = 0;
            int dataBuffLen = 0;
            int tryCount = 0;
            string CksumResult = null;
            bool Isbuffer_copied = false;
            string strVset = txtbx_Vset.Text;
            string strIset = txtbx_Iset.Text;
            string receivedata = string.Empty;
            string receiveListen = string.Empty;
            string tempReceive = string.Empty;
            //Buffer.BlockCopy();

            List<byte> bufflst = new List<byte>();
            //--------------------------------------
            do
            {
                ;//serialPort1.ReadTimeout = 20000;        // stop received after 20000sec
                int buff_len = serialPort1.BytesToRead;     // get bytes in buffer with receive manner
                byte[] buffer = new byte[buff_len];
                if (buff_len > 0)
                {
                    if (!this.serialPort1.IsOpen)
                    {
                        serialPort1.DiscardInBuffer();
                        serialPort1.DiscardOutBuffer();
                        return;  //if the port has already been closed which prevents IO Err from being half way through receiving data when the port is closed.
                    }
                    Thread.Sleep(100);
                    try
                    { //stackoverflow.com/questions/41865560/c-sharp-serialport-receive-doesnt-get-all-data-at-once
                        offset += serialPort1.Read(buffer, offset, buff_len);                   // - offset);
                        receivedata = BitConverter.ToString(buffer);
                        LogSingleton.Instance.WriteLog(@"Buffer Read ---> " + receivedata, LogSingleton.wzRECEIVE_COMMAND);

                        switch (receivedata)
                        {
                            case string a when a.Contains("AA"):
                                receiveCall.Clear();

                                if (buff_len < 20)
                                {
                                    if (receivedata[0] == 0)            // 開頭為零
                                    {
                                        tempReceive = receivedata;
                                    }
                                    else if (tempReceive != string.Empty)
                                    {
                                        receiveCall.Append(receivedata);
                                        receiveCall.Append(tempReceive);
                                        //--------------------------
                                        globalBuffer = globalBuffer.Concat(buffer).ToArray();
                                        Array.Resize(ref globalBuffer, buff_len);
                                        receiveCall.Append(BitConverter.ToString(globalBuffer));
                                        //--------------------------
                                        Array.Resize(ref globalBuffer, 20);
                                        Display d = new Display(DisplayText);
                                        this.Invoke(d, new Object[] { globalBuffer });
                                        break;
                                    }
                                    continue;
                                }
                                else if (buff_len == 20)
                                {
                                    if (buffer[2] == 0x29)
                                    {
                                        ListenVoltage = buffer[5] + buffer[6];   // LISTEN實際輸出Voltage
                                        double cmdVoltage = Convert.ToDouble(txtbx_Vset.Text) * 100;
                                        vGap = cmdVoltage - ListenVoltage;
                                        LogSingleton.Instance.WriteLog("\r\n" + " Voltage Gap Between wzcmd & listen: ---> " + vGap.ToString() + "\r\n", LogSingleton.wzMEASURE_VALUE);
                                    }

                                    if (this._IsCheckValid(buffer))
                                    {
                                        CksumResult = "ReceivedSucceed";

                                        Array.Resize(ref buffer, 20);
                                        Display d = new Display(DisplayText);
                                        this.Invoke(d, new Object[] { buffer });
                                        break;
                                    }
                                    break;
                                }
                                else
                                {
                                    continue;
                                }

                            //if (offset == FrameLen || dataBuffLen == 20) // 最完整收到
                            //{
                            //    globalBuffer = globalBuffer.Concat(buffer).ToArray();

                            //    CksumResult = "YES";
                            //    break;
                            //}

                            //if (receivedata.IndexOf("29") == 3)
                            //{
                            //    ListenVoltage = buffer[5] + buffer[6];   // LISTEN實際輸出Voltage
                            //    double cmdVoltage = Convert.ToDouble(txtbx_Vset.Text) * 100;
                            //    vGap = cmdVoltage - ListenVoltage;
                            //    LogSingleton.Instance.WriteLog("\r\n" + " Voltage Gap Between wzcmd & listen: ---> " + vGap.ToString() + "\r\n", LogSingleton.wzMEASURE_VALUE);
                            //    continue;
                            //}
                            //else
                            //{
                            //    break;
                            //}

                            case string a when a.StartsWith("00"):
                                tempReceive = receivedata;

                                if (receivedata.StartsWith("00"))       // 開頭為零
                                {
                                    tempReceive = receivedata;
                                }
                                break;

                            default:
                                if (receivedata.StartsWith("00"))       // 開頭為零
                                {
                                    tempReceive = receivedata;
                                }
                                buffer.CopyTo(globalBuffer, 0); // Cp to globalbuffer from index 0
                                Array.Resize(ref globalBuffer, buff_len);
                                Isbuffer_copied = true;
                                continue;
                        }

                        dataBuffLen += buff_len;
                        //DoReceive_with_postfix();
                        //if (buffer[2] == 0x29)      //ListenState_V回傳狀態下, 確認輸出Voltage是否指令輸入有誤差
                        //{
                        //    ListenVoltage = buffer[5] + buffer[6];   // LISTEN實際輸出Voltage
                        //    double cmdVoltage = Convert.ToDouble(txtbx_Vset.Text) * 100;
                        //    vGap = cmdVoltage - ListenVoltage;
                        //    LogSingleton.Instance.WriteLog("\r\n" + " Voltage Gap Between wzcmd & listen: ---> " + vGap.ToString() + "\r\n", LogSingleton.wzMEASURE_VALUE);
                        //}
                        //if (offset == FrameLen || dataBuffLen == 20) // 最完整收到
                        //{
                        //    globalBuffer = globalBuffer.Concat(buffer).ToArray();
                        //    Array.Resize(ref globalBuffer, 20);
                        //    Display d = new Display(DisplayText);
                        //    this.Invoke(d, new Object[] { globalBuffer });
                        //    CksumResult = "YES";
                        //    break;
                        //}
                        //else if (receiveCall.ToString().Contains("AA") && receiveCall.ToString().Contains("29"))
                        //{
                        //    ListenVoltage = buffer[5] + buffer[6];   // LISTEN實際輸出Voltage
                        //    double cmdVoltage = Convert.ToDouble(txtbx_Vset.Text) * 100;
                        //    vGap = cmdVoltage - ListenVoltage;
                        //    LogSingleton.Instance.WriteLog("\r\n" + " Voltage Gap Between wzcmd & listen: ---> " + vGap.ToString() + "\r\n", LogSingleton.wzMEASURE_VALUE);
                        //}
                        //else if (globalBuffer.All(i => globalBuffer[i].Equals(170)))  // 170 = AA  收到的位元組不完整開頭是170 或 不是, 開始collect所有位元組
                        //{
                        //    try
                        //    {
                        //        globalBuffer = globalBuffer.Concat(buffer).ToArray();
                        //        int byteGreaterZero = Enumerable.Range(0, globalBuffer.Length).Select(x => x > 0).Count();
                        //        if (byteGreaterZero > 4)
                        //        {
                        //            Array.Resize(ref globalBuffer, 20);
                        //            IsCheckSum_Legal = true;
                        //            Display d = new Display(DisplayText);
                        //            this.Invoke(d, new Object[] { globalBuffer });
                        //            break;
                        //        }
                        //    }
                        //    catch { }
                        //}
                        //else if (offset < 20)
                        //{
                        //    if (Isbuffer_copied == true)
                        //    {
                        //        receiveCall.Append(receivedata);
                        //        globalBuffer = globalBuffer.Concat(buffer).ToArray();
                        //        Array.Resize(ref globalBuffer, buff_len);
                        //        receiveCall.Append(BitConverter.ToString(globalBuffer));
                        //        continue;
                        //    }
                        //    buffer.CopyTo(globalBuffer, 0); // Cp to globalbuffer from index 0
                        //    Array.Resize(ref globalBuffer, buff_len);
                        //    Isbuffer_copied = true;
                        //}
                    }
                    catch (TimeoutException timeoutEx)
                    {
                        serialPort1.Close();
                        serialPort1.Dispose();
                        MessageBox.Show("Received had timeout!! --->" + timeoutEx.Message);
                    }
                    catch (InvalidOperationException invaild)
                    {
                        serialPort1.Close();
                        serialPort1.Dispose();
                        throw new Exception("invalid Operation " + invaild.Message);
                    }
                    catch (IOException IOex)
                    {
                        serialPort1.Close();
                        serialPort1.Dispose();
                        throw new Exception("IO Exception Err!! --> " + IOex.Message);
                    }
                    catch (FormatException Formatex)
                    {
                        MessageBox.Show("Format Err!" + Formatex.Message); //throw Formatex;
                    }
                    catch (Exception ex)
                    {
                        tryCount++;
                        if (tryCount < iRetryTime)
                        {
                            serialPort1.DiscardInBuffer();
                            serialPort1.DiscardOutBuffer();
                            //MessageBox.Show(ex.Message + "\r\n Buffer內位元組: \r\n" + BitConverter.ToString(buffer) + "\r\n 請重新連線");
                            LogSingleton.Instance.WriteLog(ex.Message + "\r\n Buffer內位元組: \r\n" + BitConverter.ToString(buffer) + "\r\n 請重新連線", LogSingleton.wzERROR);
                            continue;
                        }
                    }
                    finally
                    {
                        try
                        {
                            //serialPort1.DiscardInBuffer();
                            //serialPort1.DiscardOutBuffer();
                        }
                        catch (IOException IOex)
                        {
                            throw new Exception(IOex.Message);
                        }
                        catch (InvalidOperationException Invalidex)
                        {
                            throw new Exception(Invalidex.Message);
                        }
                    }
                }
                else
                {
                    CksumResult = "NoBytesReceived";
                    LogSingleton.Instance.WriteLog("Serialport DataReceived ERR: " + CksumResult);
                    //MessageBox.Show("BytesToRead收不到, 接收bytes組小於20");
                }
            } while (CksumResult == null);         // (Isreceiving == true); || tempList.Count <= 20
            //--------------------------------------
            //buffer.CopyTo(globalBuffer, 0);
            //tempBuffer_Length = GetFullReceiveData(tempList);
            //length = serialPort1.Read(buffer, 0, buffer.Length); //(sender as SerialPort)
            //Array.Resize(ref buffer, 20);
            //Display d = new Display(DisplayText);
            //this.Invoke(d, new Object[] { buffer });

            //In your data received, use Buffer.BlockCopy to copy data to your globalBuffer
            //if (globalBuffer.Length >= 20)
            //{ //less than this length, then the data is incomplete
            //  //Do the checking if length is at least 14
            //}
        }

        private void _loopListening(bool IsHeard)
        {
            byte[] wzListenCmd = WuzhiCmd.Instance._listenState();
            do
            {
                serialPort1.Write(wzListenCmd, 0, wzListenCmd.Length);
                serialPort1.DataReceived += new SerialDataReceivedEventHandler(serialport1_DataReceived);
                LogSingleton.Instance.WriteLog(@"Listening ---> " + BitConverter.ToString(wzListenCmd), LogSingleton.wzSEND_COMMAND);
            } while (IsHeard);
        }

        private void parse(List<Byte> tempList)     //這個方法主要是在收到結尾字元後 (因為表示已經收到一段完整資料了) 呼叫，由於我們只要顯示資料內容的部份，所以在這方法中把開頭結尾字元給去除。
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
            byte Head = 0xAA; //170
            if (FrameLen > 0 && tempList.Count >= tempList.IndexOf(Head) + FrameLen)
            {
                Byte[] temp = new Byte[FrameLen];
                Array.Copy(tempList.ToArray(), tempList.IndexOf(Head), temp, 0, temp.Length);
                //innerResponseFullBytes = temp;
                //InterpretReceiveData();
                //FrameLen = 0;
                return true;
            }
            else
            { return false; }
        }

        private void GetReceiveDataFullLength(List<Byte> tempList)
        {
            byte Head = 0xAA;    //AA
            if (tempList.Count >= 2 && FrameLen == 20)
            {
                Int32 startIndex = tempList.IndexOf(Head);
                if (startIndex >= 0 && startIndex < tempList.Count - 1)
                {
                    //FrameLen = Convert.ToInt32(tempList[startIndex + 1]) + 2;
                }
            }
        }

        private void DoReceive2()           //限定次數
        {
            Boolean readingFromBuffer;
            Int32 count = 0;
            Byte[] buffer = new Byte[1024];
            while (Isreceiving)
            {
                readingFromBuffer = true;
                while (serialPort1.BytesToRead < buffer.Length && count < 501)
                {
                    Thread.Sleep(16);
                    count++;
                    if (count > 500)
                    {
                        readingFromBuffer = false;
                    }
                }
                count = 0;
                if (readingFromBuffer)
                {
                    Int32 length = serialPort1.Read(buffer, 0, buffer.Length);
                    Display d = new Display(DisplayText);
                    this.Invoke(d, new Object[] { buffer });
                }
                else
                {
                    serialPort1.DiscardInBuffer();
                }

                Thread.Sleep(16);
            }
        }

        private void DoReceive3()           //限定時間內接收完
        {
            Boolean readingFromBuffer;
            Stopwatch watch = new Stopwatch();
            Byte[] buffer = new Byte[1024];
            while (Isreceiving)
            {
                if (serialPort1.BytesToRead > 0)
                {
                    Thread.Sleep(312);
                    watch.Start();
                    readingFromBuffer = true;
                    while (serialPort1.BytesToRead < buffer.Length && watch.ElapsedMilliseconds < 3001)
                    {
                        Thread.Sleep(16);
                        if (watch.ElapsedMilliseconds > 3000)
                        {
                            readingFromBuffer = false;
                        }
                    }
                    watch.Stop();
                    watch.Reset();
                    if (readingFromBuffer)
                    {
                        Int32 length = serialPort1.Read(buffer, 0, buffer.Length);
                        Display d = new Display(DisplayText);
                        this.Invoke(d, new Object[] { buffer });
                    }
                    else
                    {
                        serialPort1.DiscardInBuffer();
                    }
                }
                Thread.Sleep(16);
            }
        }

        private void DoReceive_with_postfix() //bool Isreceiving
        {
            List<Byte> tempList = new List<Byte>();

            while (Isreceiving)
            {
                Int32 receivedValue = serialPort1.ReadByte();

                switch (receivedValue)
                {
                    case (int)wzCodeName.syncHead: //(int)wzCodeName.read_Input_Volt:           // if read start byte
                        tempList.Clear();
                        tempList.Add((Byte)receivedValue);
                        continue;

                    case (int)wzCodeName.read_Now_Volt:             // if read end byte, i.e. checksum
                        tempList.Add((Byte)receivedValue);
                        parse(tempList);
                        break;

                    case -1:
                        break;

                    default:
                        tempList.Add((Byte)receivedValue);
                        break;
                }
            }
            //Int32 receivedValue = serialPort1.ReadByte();         //第二種讀法, read byte by byte一個一個讀
            //if (tempList.Count == 19)
            //{
            //    //hexChksum = buffer.Sum(i => i);

            //    //tempList.Add((Byte)receivedValue);
            //    int cksums = Enumerable.Range(0, tempList.Count).Sum(i => tempList[i]);   //Select(i => tempList[i])
            //    string chksum = string.Format(@"0x{0:X}", cksums);
            //    itsChksum = Convert.ToInt32(chksum.Substring(chksum.Length - 2, 2), 16);
            //    if (itsChksum == receivedValue)
            //    {
            //        IsCheckSum_Legal = true;
            //        CksumResult = IsCheckSum_Legal ? @"Checksum is legal" : @"Checksum isNot legal";//"Checksum is legal";
            //    }
            //    break;
            //}
            //else if (tempList.Count == 20 || receivedValue == 61) //AA-01-12-80-00-00-00-00-00-00-00-00-00-00 00-00-00-00-00-3D
            //{
            //    break;
            //}
            //else
            //{
            //    tempList.Add((Byte)receivedValue);
            //}
        }

        private void DoReceive4()
        {
            List<Byte> tempList = new List<Byte>();
            Byte[] buffer = new Byte[1024];
            Int32 messageDataLength = 0;
            while (Isreceiving)
            {
                Thread.Sleep(100);
                if (serialPort1.BytesToRead > 0)
                {
                    Int32 receivedLength = serialPort1.Read(buffer, 0, buffer.Length);
                    Array.Resize(ref buffer, receivedLength);
                    tempList.AddRange(buffer);
                    Array.Resize(ref buffer, 1024);
                }
                if (tempList.Count > 0)
                {
                    if (messageDataLength == 0)
                    {
                        messageDataLength = GetMessageDataLength(tempList);
                    }
                    else
                    {
                        //messageDataLength = Parse(tempList, messageDataLength);
                    }
                }
            }
        }

        private const Byte head = 249;

        private Int32 GetMessageDataLength(List<Byte> tempList)
        {
            if (tempList.Count >= 2)
            {
                Int32 startIndex = tempList.IndexOf(head);
                if (startIndex >= 0 && startIndex < tempList.Count)
                {
                    return Convert.ToInt32(tempList[startIndex + 1]);
                }
                else
                { return 0; }
            }
            else
            { return 0; }
        }

        private Byte[] GetSendBuffer(String content)
        {
            Byte[] dataBytes = Encoding.UTF8.GetBytes(content);
            if (dataBytes.Length < 256)
            {
                Byte[] result = new Byte[dataBytes.Length + 2];
                result[0] = head;
                result[1] = Convert.ToByte(dataBytes.Length);
                Array.Copy(dataBytes, 0, result, 2, dataBytes.Length);
                return result;
            }
            else
            {
                throw new OverflowException();
            }
        }

        private void _unknownCmd(string wCmd)
        {
            string unknownCmd = string.Empty;
            //aa 01 20 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 cc
            byte[] xCmd = wCmd.Split(' ').Select(i => Convert.ToByte(i, 16)).ToArray();
            serialPort1.Write(xCmd, 0, xCmd.Length);
            //serialPort1.DataReceived += new SerialDataReceivedEventHandler(serialport1_DataReceived);
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
                        }
                    }

                    if (comport.Length == 0)
                    {
                        return string.Empty;
                    }

                    return comport.Trim().Split('(')[1].Replace(')', ' ').Trim();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("_GetComport err" + ex.Message);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //1.Scan COM Ports
            //2.Receive inputs from the devices ---> Event handler for DataReceived for your serial port
            //3.When an input has a specific phrase such as "connectAlready",
            //4.Close all ports and create a new one on the port that received the phrase.
            //5.Now that the program knows what COM port the Arduino is on, it can carry on its tasks and send it commands through SerialPorts.
            string title = ">>> Synchronize serial port: " + this.wuzhiComport + "\r\n";
            int tryCount = 1;
            do
            {
                try
                {
                    tryCount++;
                    richTextBox1.Clear();

                    for (int i = 0; i < 20; i++)
                    {
                        this.wuzhiComport = this._GetComport();
                        if (this.wuzhiComport.Trim().Equals(string.Empty))
                        {
                            System.Threading.Thread.Sleep(200);
                        }
                        else
                        {
                            break;
                        }
                    }

                    //Thread.Sleep(200);
                    //break;
                }
                catch (Exception ex)
                {
                    if (iRetryTime < (tryCount - 1))
                    {
                        System.Threading.Thread.Sleep(5);
                        continue;
                    }
                    ShowErrMsg(@"COMPORT Search Fault!!!");
                    throw new Exception("\r\n.....wuzhiComport sync serialport ERR......\r\n" + ex.Message);
                }
                finally
                {
                    richTextBox1.AppendText(title);
                    richTextBox1.Clear();
                }
            } while (this.wuzhiComport == null);
        }

        public void Form1_Closing(object sender, FormClosingEventArgs e)
        {
            DialogResult dr = MessageBox.Show(this, "Be sure to Eixt？", "Window Closing Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Stop);

            if (dr == DialogResult.Yes)
            {
                try
                {
                    string[] ports = SerialPort.GetPortNames();
                    SerialPort[] serialport = new SerialPort[ports.Length];
                    foreach (string p in ports)
                    {
                        int i = Array.IndexOf(ports, p);
                        serialport[i] = new SerialPort(); //note this line, otherwise you have no serial port declared, only array reference which can contains real SerialPort object
                        serialport[i].PortName = p;
                        serialport[i].BaudRate = 9600;
                    }
                    LogSingleton.Instance.WriteLog("------------------ Close Form ------------------" + @"with serialport{0}" + serialport[0], LogSingleton.wzEND_TESTING);
                    //if (bWriteLogFlag)
                    //{
                    //    this.WriteTotalCountData();
                    //}
                    //ResultCsvSingleton.Instance.DeleteEmptyTestResultCsv();
                    //ResultCsvItSingleton.Instance.DeleteEmptyTestResultCsv();
                }
                catch { }

                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
        }

        #region Dashboard

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
                    for (int i = 0; i < 20; i++)
                    {
                        this.wuzhiComport = this._GetComport();
                        if (this.wuzhiComport.Trim().Equals(string.Empty))
                        {
                            System.Threading.Thread.Sleep(200);
                        }
                        else
                        {
                            btn_connection.Enabled = true;
                            break;
                        }
                    }
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
                finally
                {
                    try
                    {
                        serialPort1.DiscardInBuffer();
                        serialPort1.DiscardOutBuffer();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            } while (cmbx_com.Text.Length == 0);
        }

        private void btn_sendcmd_Click(object sender, EventArgs e)
        {
            //https://stackoverflow.com/questions/415291/best-way-to-combine-two-or-more-byte-arrays-in-c-sharp

            int length = serialPort1.BytesToRead;
            //btn_connection.Enabled = true;
            //double Vset = 0.0d;
            //double Iset = 0.0d;
            Int32 tryCount = 1;
            bool IsSending = false;
            string strVset = txtbx_Vset.Text;
            string strIset = txtbx_Iset.Text;
            string[] visetting = null;
            //byte[] cmd = null;
            string synchroHead = "AA"; //this._decstringToHex("170"); // string.Format(@"0x{0:X}", this.decstringToHex("170"));   //AA
            string Address = string.Format("0x{0:X}", Convert.ToInt32(txtbx_addr.Text.Trim()));     //this._decstringToHex();
            byte[] wzListenCmd = WuzhiCmd.Instance._listenState();

            title = "----- Setup current voltage -----";

            try
            {
                if (!LogSingleton.Instance.ReCreateLogDir("wuzhi"))
                {
                    LogSingleton.Instance.WriteLog("Create Log Folder in vain", LogSingleton.wzERROR);
                    //return;
                }
                if (!this.Chk_Input_Content())
                {
                    this.ShowErrMsg("Vset or Iset Missing~~~!");
                    return;
                }
                if (btn_Power.Text == "PowerOFF")            // because Btn_Power tradeoff WuzhiCmd.WuzhiPower.PowerOn
                {
                    double[] VIset = WuzhiCmd.Instance.chkVIset(strVset, strIset);
                    double Iset = VIset[0];
                    double Vset = VIset[1];
                    visetting = WuzhiCmd.Instance.split_VI(Vset, Iset);    //split iset vset into 4 bytes
                }
                else
                {
                    this.ShowErrMsg("Power DO NOT turn on!!");
                    return;
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

            do
            {
                try
                {
                    tryCount++;
                    wzTx = WuzhiCmd.Instance._VIset_Cmd(synchroHead, Address, visetting);
                    serialPort1.Write(wzTx, 0, wzTx.Length);
                    Thread.Sleep(200);
                    IsSending = true;
                    serialPort1.DataReceived += new SerialDataReceivedEventHandler(serialport1_DataReceived);
                    if (IsSending == true)
                    {
                        LogSingleton.Instance.WriteLog(@"Sendcmd ---> " + BitConverter.ToString(wzTx), LogSingleton.wzSEND_COMMAND);
                        serialPort1.Write(wzListenCmd, 0, wzListenCmd.Length);
                        serialPort1.DataReceived += new SerialDataReceivedEventHandler(serialport1_DataReceived);
                        LogSingleton.Instance.WriteLog(@"Listening ---> " + BitConverter.ToString(wzListenCmd), LogSingleton.wzSEND_COMMAND);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    if (iRetryTime < (tryCount - 1))
                    {
                        System.Threading.Thread.Sleep(5);
                        LogSingleton.Instance.WriteLog(@"Sendcmd ---> " + BitConverter.ToString(wzTx), LogSingleton.wzSEND_COMMAND);
                        continue;
                    }
                    ShowErrMsg("connect serialport ERR");
                    throw new Exception("serialPort1.Write ERR" + ex.Message);
                }
            } while (!IsSending);
        }

        private void btn_Power_Click(object sender, EventArgs e)
        {
            Button OneShotBtn = (Button)sender;
            byte[] wzCmdBytes = null;
            title = "----- Set Power -----";
            switch (OneShotBtn.Text)
            {
                case "PowerOFF":
                    btn_Power.BackColor = Color.DarkRed;
                    btn_Power.Text = "PowerON";

                    wzCmdBytes = WuzhiCmd.Instance._IsPowerOn(WuzhiCmd.WuzhiPower.PowerOff);
                    btn_sendcmd.Enabled = false;
                    //btn_connection.Enabled = false;
                    LogSingleton.Instance.WriteLog(@"PowerOn ---> " + BitConverter.ToString(wzCmdBytes), LogSingleton.wzSEND_COMMAND);
                    serialPort1.Write(wzCmdBytes, 0, wzCmdBytes.Length);
                    break;

                case "PowerON":
                    btn_Power.BackColor = Color.DarkGreen;
                    btn_Power.Text = "PowerOFF";
                    wzCmdBytes = WuzhiCmd.Instance._IsPowerOn(WuzhiCmd.WuzhiPower.PowerOn);
                    if (btn_connection.Enabled)
                    {
                        btn_sendcmd.Enabled = true;
                    }
                    LogSingleton.Instance.WriteLog(@"PowerOff ---> " + BitConverter.ToString(wzCmdBytes), LogSingleton.wzSEND_COMMAND);
                    serialPort1.Write(wzCmdBytes, 0, wzCmdBytes.Length);
                    break;
            }
        }

        private void StartToTestRFThreadFunc(object obj)
        {
            Form1 self = obj as Form1;
            bool IsHeard = true;
            self.BeginInvoke(new Action(() =>
            {
                //self.ShowTestGroupInformation(false, currentItem);
                this._loopListening(IsHeard);
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

        private void btn_connection_Click(object sender, EventArgs e)   // make a connection while
        {
            Button OneShotBtn = (Button)sender;
            string receiveData = string.Empty;
            string wzBaudrate = this.cmbx_baudrate.Text.Trim();
            byte[] wuzhicmd = null;
            title = "----- Make Connection -----";
            switch (OneShotBtn.Text)
            {
                case "DisConnection":
                    btn_connection.BackColor = Color.DarkRed;
                    btn_connection.Text = "Connection";
                    try
                    {
                        //this._IsConnected(WuzhiConnectStatus.DisConnect);
                        serialPort1.ReceivedBytesThreshold = 1;
                        wuzhicmd = WuzhiCmd.Instance._IsConnected(WuzhiCmd.WuzhiConnectStatus.DisConnect);
                        serialPort1.ReceivedBytesThreshold = 2;
                        // 沒必要額外回傳, 實際官方sdk也沒回傳
                        //serialPort1.DataReceived += new SerialDataReceivedEventHandler(serialport1_DataReceived);
                        serialPort1.Write(wuzhicmd, 0, wuzhicmd.Length);
                    }
                    catch (Exception ex) { ShowErrMsg(@"COMPORT already occupied" + ex.Message); }
                    finally
                    {
                        try
                        {
                            //serialPort1.DiscardInBuffer();
                            //serialPort1.DiscardOutBuffer();
                            //serialPort1.Dispose();
                        }
                        catch { }
                    }
                    btn_sendcmd.Enabled = false;
                    //btn_sendcmd.Enabled = false;
                    //btn_open.Enabled = false;
                    break;

                case "Connection":
                SyncConn:
                    btn_connection.BackColor = Color.DarkGreen;
                    btn_connection.Text = "DisConnection";
                    try
                    {
                        btn_Power.Enabled = true;
                        btn_sendcmd.Enabled = true;
                        txtbx_Iset.Enabled = true;
                        txtbx_Vset.Enabled = true;
                        if (!mIsConnectedSerialPort)
                        {
                            serialPort1.RtsEnable = true;           //序列通訊期間是否啟用 Request to Send (RTS)
                            serialPort1.DtrEnable = true;           // Gets or sets a value that enables the Data Terminal Ready (DTR) signal during serial communication

                            this.mIsConnectedSerialPort = true;
                        }
                        if (serialPort1.IsOpen)
                        {
                            serialPort1.Close();
                            serialPort1.Dispose();
                            System.Threading.Thread.Sleep(200);
                        }

                        richTextBox1.AppendText("\n\r SerialPort: ---> " + this.wuzhiComport.Trim() + "\n\r BaudRate: ---> " + this.serialPort1.BaudRate + "\r\n");
                        System.Threading.Thread.Sleep(300);
                        Int32 tryCount = 1;
                        do
                        {
                            try
                            {
                                tryCount++;
                                serialPort1.ReceivedBytesThreshold = 1;
                                this.serialPort1 = new SerialPort(this.wuzhiComport.Trim(), Convert.ToInt32(wzBaudrate), Parity.None, 8, StopBits.One);
                                serialPort1.Open();
                                //this.Invoke(new Action(() =>
                                //{
                                //}));
                                wuzhicmd = WuzhiCmd.Instance._IsConnected(WuzhiCmd.WuzhiConnectStatus.Connect);
                                serialPort1.Write(wuzhicmd, 0, wuzhicmd.Length);
                                serialPort1.DataReceived += new SerialDataReceivedEventHandler(serialport1_DataReceived);

                                //-------------------------

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
                        Thread thread = new Thread(new ParameterizedThreadStart(StartToTestRFThreadFunc));
                        thread.Start(this);
                    }
                    catch (Exception ex)
                    {
                        ShowErrMsg(@"COMPORT already occupied");
                        throw ex;
                    }
                    break;

                default:
                    goto SyncConn;
                    //break;
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