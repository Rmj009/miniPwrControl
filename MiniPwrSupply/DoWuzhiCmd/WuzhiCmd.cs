using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Collections.Generic;

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

        private string _decstringToHex(string args)
        {
            //var hexstring = string.Join("", args.Select(i => string.Format("{0:X2}", Convert.ToInt32(i))));
            var decstring = Convert.ToInt32(args);
            string hexstr = string.Format("{0:X}", decstring);
            return hexstr;
        }

        private void comport_DataReceived(Object sender, SerialDataReceivedEventArgs e)
        {
            Byte[] buffer = new Byte[1024];
            Int32 length = (sender as SerialPort).Read(buffer, 0, buffer.Length);
            Array.Resize(ref buffer, length);
            //Display d = new Display(DisplayText);
            //this.Invoke(d, new Object[] { buffer });
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
    }

    public partial class Form1 : Form
    {
        //private SerialPort comport;
        //private Int32 totalLength = 0;
        //delegate void Display(Byte[] buffer);
    }
}