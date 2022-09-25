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

//namespace SimpleReceiveEventCS
//{
namespace MiniPwrSupply.DoWuzhiCmd
{
    public class WuzhiCmd
    {
        private static WuzhiCmd mInstance = null;
        private string vsetting = string.Empty;
        private string isetting = string.Empty;
        private string vsetting1 = string.Empty;
        private string isetting1 = string.Empty;
        private string vsetting2 = string.Empty;
        private string isetting2 = string.Empty;
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
        private void TakeInitiatives()
        {
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

        public string[] split_VI(double Vset, double Iset)
        {
            try
            {
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

        public byte[] _VIset_Cmd(string synchroHead,  string addr, string[] visetting) 
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

    public partial class Form1 : Form
    {
        //private SerialPort comport;
        //private Int32 totalLength = 0;
        //delegate void Display(Byte[] buffer);
    }
}