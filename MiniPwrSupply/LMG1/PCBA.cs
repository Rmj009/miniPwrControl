using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPwrSupply.LMG1
{
    internal class PCBA
    {
        public void ChkEMMC()
        {
            string res = "";
            try 
            {
                SendAndChk(PortType.SSH, "mmc info", keyword, out res, 0, 5000);
                SendAndChk(PortType.SSH, "reset", keyword, out res, 0, 5000);
            }
            catch (Exception e)
            {
                Display()
            }
        }
    }
}
