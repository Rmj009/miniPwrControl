using MiniPwrSupply.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPwrSupply.Config
{
    internal class SystemIni : Ini
    {
        private static readonly object mLock = new object();
        private static SystemIni mInstance = null;
        private string INI_POS = System.Windows.Forms.Application.StartupPath + @"\System.ini";

        private SystemIni()
        {
            this.SetSystemIniPos(INI_POS);
        }

        public static SystemIni Instance
        {
            get
            {
                lock (SystemIni.mLock)
                {
                    if (mInstance == null)
                    {
                        mInstance = new SystemIni();
                    }
                }
                return mInstance;
            }
        }

        internal string LogPath()
        {
            throw new NotImplementedException();
        }
    }
}