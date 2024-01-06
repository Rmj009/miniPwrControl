using MiniPwrSupply.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPwrSupply
{
    internal class MiniPwrSystemIni : InterfaceSystemIni
    {
        public bool IsPowerSupplyEnable()
        {
            return this.IniReadValue("wuzhiCmd", @"Enable").Trim().Equals(@"1");
        }
    }
}
