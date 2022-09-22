using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MiniPwrSupply.Config
{
    public abstract class Ini
    {
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filepath);


        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filepath);

        [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileString", SetLastError = true)]
        private static extern int GetPrivateProfileString(string section, string key, string def, Byte[] retVal, int size, string filepath);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern uint GetPrivateProfileSection(string lpAppName, IntPtr lpReturnedString, uint nSize, string lpFileName);

        [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileSectionNames", SetLastError = true)]
        private static extern uint GetPrivateProfileSectionNames(IntPtr retVal, uint size, string filePath);

        private string mFile = "";
        private string mSysFile = "";

        protected void SetIniPos(string file)
        {
            if (!System.IO.File.Exists(file))
            {
                //throw new UIException(@"Not Found Ini File : " + file, UIErrCode.Ini_File_notFound);
            }
            mFile = file;
        }

        protected void SetSystemIniPos(string file)
        {
            if (!System.IO.File.Exists(file))
            {
                //throw new UIException(@"Not Found System Ini File : " + file, UIErrCode.Ini_File_notFound);
            }
            mSysFile = file;
        }

        // Write ini file
        protected void IniWriteValue(string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, mFile);
        }

        protected void _IniDelKey(string Section, string Key)
        {
            WritePrivateProfileString(Section, Key, null, mFile);
        }

        protected List<string> _IniReadKey(string Section)
        {

            ASCIIEncoding ascii = new ASCIIEncoding();
            int buffer;
            buffer = 256; // Define length
            StringBuilder temp = new StringBuilder(buffer);
            Byte[] tmpstr = new byte[1024];
            int size = GetPrivateProfileString(Section, null, null, tmpstr, 1024, mFile);
            string sections = ascii.GetString(tmpstr);
            List<string> list = new List<string>();
            string[] sectionList = sections.Split(new char[1] { '\0' });
            for (int i = 0; i < sectionList.Length; i++)
            {
                if (sectionList[i].Equals("Run") ||
                    sectionList[i].Equals(string.Empty) ||
                    (GetPrivateProfileString(Section, sectionList[i], "", temp, buffer, mFile) == 0))
                {
                    continue;
                }
                list.Add(sectionList[i]);
            }
            return list;
        }

        // Read ini file
        protected string IniReadValue(string Section, string Key)
        {
            int buffer;
            buffer = 81920; // Define length
            StringBuilder temp = new StringBuilder(buffer);
            int size = GetPrivateProfileString(Section, Key, "", temp, buffer, mFile);
            //if (size == 0) {
            //    throw new Exception("Ini Read Section : " + Section + ", Key : " + Key + " value is null !!");
            //}
            return temp.ToString().Trim();
        }

        protected void SysIniWriteValue(string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, mSysFile);
        }

        // Read ini file
        protected string SysIniReadValue(string Section, string Key)
        {
            int buffer;
            buffer = 81920; // Define length
            StringBuilder temp = new StringBuilder(buffer);
            GetPrivateProfileString(Section, Key, "", temp, buffer, mSysFile);
            return temp.ToString().Trim();
        }

        public class IniItem
        {
            private string mKey = "";
            private string mValue = "";

            public IniItem(String key, String v)
            {
                this.mValue = v;
                this.mKey = key;
            }

            public String Value
            {
                get
                {
                    return this.mValue;
                }

                set
                {
                    this.mValue = value;
                }
            }

            public String Key
            {
                get
                {
                    return this.mKey;
                }

                set
                {
                    this.mKey = value;
                }
            }
        }

        public List<IniItem> IniReadSection(string section)
        {
            UInt32 MAX_BUFFER = 32767;

            string[] items = new string[0];

            IntPtr pReturnedString = Marshal.AllocCoTaskMem((int)MAX_BUFFER * sizeof(char));

            UInt32 bytesReturned = GetPrivateProfileSection(section, pReturnedString, MAX_BUFFER, mFile);

            if (!(bytesReturned == MAX_BUFFER - 2) || (bytesReturned == 0))
            {
                string returnedString = Marshal.PtrToStringAuto(pReturnedString, (int)bytesReturned);

                items = returnedString.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
            }

            Marshal.FreeCoTaskMem(pReturnedString);

            List<IniItem> returnItems = new List<IniItem>();

            foreach (String item in items)
            {
                String[] split = item.Trim().Split('=');
                if (split.Length == 2)
                {
                    returnItems.Add(new IniItem(split[0], split[1]));
                }
            }

            return returnItems;
        }

        public List<IniItem> SysIniReadSection(string section)
        {
            UInt32 MAX_BUFFER = 32767;

            string[] items = new string[0];

            IntPtr pReturnedString = Marshal.AllocCoTaskMem((int)MAX_BUFFER * sizeof(char));

            UInt32 bytesReturned = GetPrivateProfileSection(section, pReturnedString, MAX_BUFFER, mSysFile);

            if (!(bytesReturned == MAX_BUFFER - 2) || (bytesReturned == 0))
            {
                string returnedString = Marshal.PtrToStringAuto(pReturnedString, (int)bytesReturned);

                items = returnedString.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
            }

            Marshal.FreeCoTaskMem(pReturnedString);

            List<IniItem> returnItems = new List<IniItem>();

            foreach (String item in items)
            {
                String[] split = item.Trim().Split('=');
                if (split.Length == 2)
                {
                    returnItems.Add(new IniItem(split[0], split[1]));
                }
            }

            return returnItems;
        }
    }
}