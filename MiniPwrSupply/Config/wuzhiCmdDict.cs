using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MiniPwrSupply.Config
{
    internal class wuzhiConfig
    {
        public enum wzCmd
        {
            [EnumMember(Value = "aa 01 20 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 cc")]
            MakeConnection,

            //aa 01 20 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 cc 是連線????
            //aa 01 29 03 06 00 c8 00 00 00 00 00 00 00 00 00 00 00 00 a5
            //aa 01 2b 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 d6

            [EnumMember(Value = "aa 01 20 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 cb")]
            DisConnection,

            //aa 01 29 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 d4 斷線後持續
            //aa 01 20 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 cb

            [EnumMember(Value = "aa 01 22 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ce")]
            PowerOn,

            [EnumMember(Value = "aa 01 22 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 cd")]
            PowerOff,

            [EnumMember(Value = "AA-01-12-80-00-00-00-00-00-00-00-00-00-00 00-00-00-00-00-3D")]
            Recevied_Success, //170 -01-18-128-00-...-00-00-00-00-00-00-00 00-00-00-00-00-61
        }

    }

    public static class wuzhiCmdDict
    {
        public const string
            Listen_Vtate = "aa 01 29 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 d4", // listen dataReceived, only response Voltage state
                                                                                          //Listen_Istate = "01 29 03 06 00 88 00 00 00 00 00 00 00 00 00 00 00 00 65", //  only response Current state
            Connect = "aa 01 20 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 cc",
            DisConnect = "aa 01 20 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 cb",
            PowerOn = "aa 01 22 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ce",
            PowerOff = "aa 01 22 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 cd";
    }
}