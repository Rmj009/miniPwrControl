using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.Data;
using System.Drawing;

using System.Linq;
using System.Text;

using System.Windows.Forms;
using System.IO.Ports;
using MiniPwrSupply.Properties;
using MiniPwrSupply.Config;
using System.Threading;

//namespace SimpleReceiveEventCS
//{
namespace MiniPwrSupply.DoWuzhiCmd
{
    public class WuzhiCmd
    {
        public static double Vset = 0.0d;
        public static double Iset = 0.0d;
        private static WuzhiCmd mInstance = null;
        private string vsetting = string.Empty;
        private string isetting = string.Empty;
        private string vsetting1 = string.Empty;
        private string isetting1 = string.Empty;
        private string vsetting2 = string.Empty;
        private string isetting2 = string.Empty;
        public static int Err_checksum_is_wrong = 144;
        public static int Err_wrong_params_setting_or_params_overflow = 160;
        public static int Err_cmd_cannot_executed = 176;
        public static int Err_cmd_is_invaild = 192;
        public static int Err_cmd_is_unknown = 208;

        private Action<string, UInt32> mLogCallback = null;

        private static string synchroHead = "AA"; //_decstringToHex("170"); // string.Format(@"0x{0:X}", this.decstringToHex("170"));   //AA

        public static WuzhiCmd Instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = new WuzhiCmd();
                }
                return mInstance;
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

        public string IsbufferValid(byte[] buff)
        {
            string showInfo = string.Empty;
            if (buff[3] == Err_checksum_is_wrong)    //0x90
            {
                showInfo = "checksum is wrong";
                //Save_LOG_data(showInfo);
            }
            else if (buff[3] == Err_wrong_params_setting_or_params_overflow) //0xA0
            {
                showInfo = "wrong params setting or params overflow";
            }
            else if (buff[3] == Err_cmd_cannot_executed) // 0xB0
            {
                showInfo = "cmd cannot executed";
            }
            else if (buff[3] == Err_cmd_is_invaild) // 0xC0
            {
                showInfo = "cmd is invaild";
            }
            else if (buff[3] == Err_cmd_is_unknown) // 0xD0
            {
                showInfo = "cmd is unknown";
            }
            else        // buff[3] == 128 ---> 0x80
            {
                showInfo = "WuzhiCmd succeed!!";
            }
            return showInfo;
        }

        private void TakeInitiatives()
        {
        }

        private void ShowErrMsg(string errMsg)
        {
            MessageBox.Show(errMsg, @"Error Occur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public byte[] _IsPowerOn(WuzhiPower status)
        {
            string powerCmd = string.Empty;
            if (status == WuzhiPower.PowerOn)
            {
                powerCmd = wuzhiCmdDict.PowerOn;
            }
            else if (status == WuzhiPower.PowerOff)
            {
                powerCmd = wuzhiCmdDict.PowerOff;
            }
            return powerCmd.Split(' ').Select(i => Convert.ToByte(i, 16)).ToArray();
            //serialPort1.Write(pwrCmd, 0, pwrCmd.Length);
            //serialPort1.DataReceived += new SerialDataReceivedEventHandler(serialport1_DataReceived);
        }

        public byte[] _IsConnected(WuzhiConnectStatus status)
        {
            string syncCmd = string.Empty;
            if (status == WuzhiConnectStatus.Connect)
            {
                syncCmd = wuzhiCmdDict.Connect;
            }
            else if (status == WuzhiConnectStatus.DisConnect)
            {
                syncCmd = wuzhiCmdDict.DisConnect;
                // syncmd2 has no dataReceived
            }
            return syncCmd.Split(' ').Select(i => Convert.ToByte(i, 16)).ToArray();
            //serialPort1.Write(cmd, 0, cmd.Length);
            //serialPort1.DataReceived += new SerialDataReceivedEventHandler(serialport1_DataReceived);
        }

        public byte[] _listenState()
        {
            string listening = wuzhiCmdDict.Listen_Vtate;
            return listening.Split(' ').Select(i => Convert.ToByte(i, 16)).ToArray();
            //serialPort1.Write(cmd, 0, cmd.Length);
            //serialPort1.DataReceived += new SerialDataReceivedEventHandler(serialport1_DataReceived);
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
            //richTextBox1.AppendText("hexTostring --->" + hex.ToString());
            return hex.ToString();
        }

        //private void _hexAddition<Tiida>(IList<Tiida> tiidas)      //array<T>(string, T)改用Generic方式, 捨棄 byte[] hexBytes       // checksum = all hex addition substring last 2
        public Int32 _hexAddition(string[] strArray)
        {
            byte[] hexAdded = Enumerable.Range(0, strArray.Length - 2).Select(x => Convert.ToByte(strArray[x], 16)).ToArray();
            Int32 total = hexAdded.Sum(x => x);
            //richTextBox1.AppendText("\r\n strArray _hexAddition ---> " + string.Format("0x{0:X}", total));
            return total;
            //if (strArray.Length == 19)
            //{
            //}
            //else if (strArray.Length == 20)
            //{
            //    //byte[] hexAdded = Enumerable.Range(0, wuzhiCmd.Length - 2).Where(x => x % 2 == 0).Select(x => Convert.ToByte(wuzhiCmd.Substring(x, 2), 16)).ToArray();  //wuzhiCmd.Substring(x,2)
            //    byte[] hexAdded = Enumerable.Range(0, txtbx_WuzhiCmd.Text.Split(' ').Length - 1).Select(x => Convert.ToByte(txtbx_WuzhiCmd.Text.Split(' ')[x], 16)).ToArray();

            //    total = hexAdded.Sum(x => x);

            //    //richTextBox1.AppendText("wuzhiCmd _hexAddition ---> " + string.Format("0x{0:X}", total));
            //    return total;
            //}
            //else
            //{
            //    return 0;
            //}
            //else if (1 == 1)
            //{
            //    richTextBox1.AppendText("IsReadyOnly return TRUE ---> array,\r\n while FLASE ---> list \r\n" + tiidas.IsReadOnly.ToString() + "\r\n");
            //    //foreach (Tiida item in tiidas)
            //    //{
            //    //    richTextBox1.AppendText(item.ToString() + " ");
            //    //}
            //}
        }

        private void DoReceive()
        {
            bool Isreceiving = true;
            List<Byte> tempList = new List<Byte>();
            byte[] buffer = new byte[2048];
            while (Isreceiving)
            {
                //Int32 receivedValue = serialPort1.ReadByte();   // at the end  of byte[] if receivedVaule = -1
                //do
                //{
                //    Int32 length = serialPort1.Read(buffer, 0, buffer.Length);
                //    Array.Resize(ref buffer, length);
                //    Display d = new Display(DisplayText);
                //    this.Invoke(d, new Object[] { buffer });
                //    Array.Resize(ref buffer, 1024);
                //} while (serialPort1.BytesToRead < 2048);

                //Thread.Sleep(20);
                //switch (receivedValue)
                //{
                //    case 1:
                //        tempList.Clear();
                //        tempList.Add((Byte)receivedValue);
                //        break;

                //    case 2:
                //        tempList.Add((Byte)receivedValue);
                //        this.parse(tempList);
                //        break;

                //    case -1:
                //        break;

                //    default:
                //        tempList.Add((Byte)receivedValue);
                //        break;
                //}
            }
        }

        private void comport_DataReceived(Object sender, SerialDataReceivedEventArgs e)
        {
            Byte[] buffer = new Byte[1024];
            Int32 length = (sender as SerialPort).Read(buffer, 0, buffer.Length);
            Array.Resize(ref buffer, length);
            //Display d = new Display(DisplayText);
            //this.Invoke(d, new Object[] { buffer });
        }

        public string _decstringToHex(string args)
        {
            //var hexstring = string.Join("", args.Select(i => string.Format("{0:X2}", Convert.ToInt32(i))));
            var decstring = Convert.ToInt32(args);
            string hexstr = string.Format("{0:X}", decstring);
            return hexstr;
        }

        public double[] chkVIset(string strVset, string strIset)
        {
            //if (strVset.Length > 4 || strIset.Length > 4)
            //{
            //    this.ShowErrMsg("超過界定值設定");
            //    return;
            //}
            if (strIset.Length > 4)
            {   // confirm whether belong to double
                //double isetting = Convert.ToDouble(txtbx_Iset.Text.Trim().Substring(0, 4));
                if (Double.TryParse(strIset.Trim().Substring(0, strIset.Length), out Iset))
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
            else if (strVset.Length > 4)       // coz the protocol wuzhi define merely suit in two bytes hexadecimal
            {
                if (double.TryParse(strVset.Trim().Substring(0, strVset.Length), out Vset))
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
            else if (strVset.Length > 4 && strIset.Length > 4)
            {
                if (double.TryParse(strVset.Trim().Substring(0, strVset.Length), out Vset))
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

                if (Double.TryParse(strIset.Trim().Substring(0, strIset.Length), out Iset))
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
            else
            {
                double.TryParse(strVset.Trim(), out Vset);
                double.TryParse(strIset.Trim(), out Iset);

            }
            return new double[] { Iset, Vset };
        }

        public string[] split_VI(double Vset, double Iset)
        {
            try
            {   //double -> int -> string -> Hex
                string VsetStr = Convert.ToString((int)(Vset * 100));
                string IsetStr = Convert.ToString((int)(Iset * 1000));
                vsetting = this._decstringToHex(VsetStr); //(Int32)Vset * 100
                isetting = this._decstringToHex(IsetStr); //Convert.ToString((Int32)Iset, 16));
                int vset_len = vsetting.ToCharArray().Length;
                int iset_len = isetting.ToCharArray().Length;
                if (vset_len == 3) // && iset_len ==3)
                {
                    if (iset_len == 3)
                    {
                        vsetting1 = vsetting.Substring(0, vset_len - 2);
                        vsetting2 = vsetting.Substring(vset_len - 2, vset_len - 1);
                        isetting1 = isetting.Substring(0, iset_len - 2);
                        isetting2 = isetting.Substring(iset_len - 2, iset_len - 1);
                    }
                    else if (iset_len == 4)
                    {
                        vsetting1 = vsetting.Substring(0, vset_len - 2);
                        vsetting2 = vsetting.Substring(vset_len - 2, vset_len - 1);
                        isetting1 = isetting.Substring(0, iset_len - 2);
                        isetting2 = isetting.Substring(iset_len - 2, iset_len - 2);
                    }
                    else
                    {
                        vsetting1 = vsetting.Substring(0, vset_len - 2);
                        vsetting2 = vsetting.Substring(vset_len - 2, vset_len - 1);
                        isetting1 = "00";
                        isetting2 = isetting;
                    }
                }
                else if (vset_len == 4)
                {
                    if (iset_len == 3)
                    {
                        vsetting1 = vsetting.Substring(0, vset_len - 2);
                        vsetting2 = vsetting.Substring(vset_len - 2, vset_len - 2);
                        isetting1 = isetting.Substring(0, iset_len - 2);
                        isetting2 = isetting.Substring(iset_len - 2, iset_len - 1);
                    }
                    else if (iset_len == 4)
                    {
                        vsetting1 = vsetting.Substring(0, vset_len - 2);
                        vsetting2 = vsetting.Substring(vset_len - 2, vset_len - 2);
                        isetting1 = isetting.Substring(0, iset_len - 2);
                        isetting2 = isetting.Substring(iset_len - 2, iset_len - 2);
                    }
                    else
                    {
                        vsetting1 = vsetting.Substring(0, vset_len - 2);
                        vsetting2 = vsetting.Substring(vset_len - 2, vset_len - 2);
                        isetting1 = "00";
                        isetting2 = isetting;
                    }
                }
                else if (iset_len == 3)
                {
                    if (vset_len == 3)
                    {
                        vsetting1 = vsetting.Substring(0, vset_len - 2);
                        vsetting2 = vsetting.Substring(vset_len - 2, vset_len - 1);
                        isetting1 = isetting.Substring(0, iset_len - 2);
                        isetting2 = isetting.Substring(iset_len - 2, iset_len - 1);
                    }
                    else if (vset_len == 4)
                    {
                        vsetting1 = vsetting.Substring(0, vset_len - 2);
                        vsetting2 = vsetting.Substring(vset_len - 2, vset_len - 2);
                        isetting1 = isetting.Substring(0, iset_len - 2);
                        isetting2 = isetting.Substring(iset_len - 2, iset_len - 1);
                    }
                    else
                    {
                        vsetting1 = "00";
                        vsetting2 = vsetting;
                        isetting1 = isetting.Substring(0, iset_len - 2);
                        isetting2 = isetting.Substring(iset_len - 2, iset_len - 1);
                    }
                }
                else if (iset_len == 4)
                {
                    if (vset_len == 3)
                    {
                        vsetting1 = vsetting.Substring(0, vset_len - 2);
                        vsetting2 = vsetting.Substring(vset_len - 2, vset_len - 1);
                        isetting1 = isetting.Substring(0, iset_len - 2);
                        isetting2 = isetting.Substring(iset_len - 2, iset_len - 2);
                    }
                    else if (vset_len == 4)
                    {
                        vsetting1 = vsetting.Substring(0, vset_len - 2);
                        vsetting2 = vsetting.Substring(vset_len - 2, vset_len - 2);
                        isetting1 = isetting.Substring(0, iset_len - 2);
                        isetting2 = isetting.Substring(iset_len - 2, iset_len - 2);
                    }
                    else
                    {
                        vsetting1 = "00";
                        vsetting2 = vsetting;
                        isetting1 = isetting.Substring(0, iset_len - 2);
                        isetting2 = isetting.Substring(iset_len - 2, iset_len - 2);
                    }
                }
                else
                {
                    vsetting1 = "00";
                    vsetting2 = vsetting;
                    isetting1 = "00";
                    isetting2 = isetting;
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

            return new string[] { vsetting1, vsetting2, isetting1, isetting2 };
        }

        public byte[] _VIset_Cmd(string synchroHead, string addr, string[] visetting)
        {
            vsetting1 = visetting[0];
            vsetting2 = visetting[1];
            isetting1 = visetting[2];
            isetting2 = visetting[3];
            string checksum = string.Empty;
            //Thread thread = new Thread(new ParameterizedThreadStart(StartToTestRFThreadFunc));
            //thread.Start(this);
            //----------------------------------------------
            string[] PowerCmd = {
                 synchroHead,
                    addr,
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
            cmd[1] = Convert.ToByte(addr, 16);   //0x01
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

            return cmd;
        }
    }
}