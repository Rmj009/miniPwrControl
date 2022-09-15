//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace MiniPwrSupply.CmdDictonary
//{
//    internal class CmdDictonary
//    {
//        public string CmdDict { get; set; }

//        private CmdDictonary()
//        {
//        }

//        public enum MyEnum
//        {
//        }

//        public LinkedList<MyEnum> EnumList = new LinkedList<MyEnum>();

//        public static CmdDictonary Instance
//        {
//            get
//            {
//                lock (CmdDictonary.mLock)
//                {
//                    if (mInstance)
//                    {
//                    }
//                }
//            }
//        }
//byte[] cmd = new byte[20];
//cmd[0] = 0xAA;
//cmd[1] = 0x01;
//cmd[2] = 0x22;
//cmd[3] = 0x00;
//cmd[4] = 0x00;
//cmd[5] = 0x00;
//cmd[6] = 0x00;
//cmd[7] = 0x00;
//cmd[8] = 0x00;      // Voltage
//cmd[9] = 0x00;      //  current hex+
//cmd[10] = 0x00;     // current hex+
//cmd[11] = 0x00;
//cmd[12] = 0x00;
//cmd[13] = 0x00;
//cmd[14] = 0x00;
//cmd[15] = 0x00;
//cmd[16] = 0x00;
//cmd[17] = 0x00;
//cmd[18] = 0x00;
//cmd[19] = 0xCD;
//    }
//}