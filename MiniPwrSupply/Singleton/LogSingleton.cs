using MiniPwrSupply.Config;
using MiniPwrSupply.Singleton;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System;

namespace MiniPwrSupply.Singleton
{
    internal class LogSingleton
    {
        public LogSingleton mInstatnce = null;
        private static System.Threading.Mutex ms_threadMutex = new System.Threading.Mutex();
        private static string temp_logFileName = "";

        public static int wzERROR = 0;
        public static int wzDate_Time_Start = 1;
        public static int wzEND_TESTING = 2;
        public static int wzSEND_COMMAND = 3;
        public static int wzRECEIVE_COMMAND = 4;
        public static int wzMEASURE_VALUE = 5;

        //-----------------------------------------------------------------------------
        private string m_logDirPath = System.Windows.Forms.Application.StartupPath + @"\Log\" + DateTime.Now.ToString("yyyyMMdd");

        private static readonly object mLock = new object();
        private static string TMP_FILE = "tmp.log";
        private static LogSingleton mInstance = null;
        private string mRootFlowPath = "";
        private string mRootPath = "";

        private static string m_logFileName = "";

        public static LogSingleton Instance             // TOSHIBA DO NOT NEED MAC ADDRESS therefore "OperatorSingleton.Instance.Flow" no longer required.
        {
            get
            {
                lock (LogSingleton.mLock)
                {
                    if (mInstance == null)
                    {
                        mInstance = new LogSingleton();
                        string systemLogPath = SystemIni.Instance.LogPath();
                        string logPath = systemLogPath + @"\Logs\" + OperatorSingleton.Instance.LotNo + @"\Log\";       // @"\" + OperatorSingleton.Instance.Flow + @"\Log\";
                        if (systemLogPath.Length > 0)
                        {
                            mInstance.mRootFlowPath = systemLogPath + @"\Logs\" + OperatorSingleton.Instance.LotNo + @"\";      // + OperatorSingleton.Instance.Flow;
                            mInstance.mRootPath = systemLogPath + @"\Logs\" + OperatorSingleton.Instance.LotNo;
                            mInstance.m_logDirPath = logPath;
                            temp_logFileName = mInstance.m_logDirPath + @"\" + "_" + TMP_FILE;          //+ OperatorSingleton.Instance.LotNo
                        }
                        else
                        {
                            mInstance.mRootFlowPath = System.Windows.Forms.Application.StartupPath + @"\wzLog\";    // + OperatorSingleton.Instance.LotNo + @"\wzLog"; // + OperatorSingleton.Instance.Flow;
                            mInstance.mRootPath = System.Windows.Forms.Application.StartupPath + @"\wzLogs\";       // + OperatorSingleton.Instance.LotNo;
                            mInstance.m_logDirPath = System.Windows.Forms.Application.StartupPath + @"\wzLogs\";    // + OperatorSingleton.Instance.LotNo + @"\wzLog";    // + OperatorSingleton.Instance.Flow + @"\Log\";
                        }
                    }
                }
                return mInstance;
            }
        }

        private Boolean CreateDir(string path)
        {
            Boolean result = false;

            int time = 0;
            int tryTimes = 5;

            while (time < tryTimes)
            {
                try
                {
                    if (System.IO.Directory.Exists(path))
                    {
                        result = true;
                        time = tryTimes + 1;
                    }
                    else
                    {
                        System.IO.Directory.CreateDirectory(path);
                    }
                }
                catch (System.IO.IOException ex)
                {
                    result = false;

                    System.Threading.Thread.Sleep(500 * (time + 1));
                    if (++time == tryTimes)
                    {
                        System.Windows.Forms.MessageBox.Show("CreateDir( " + path + " ) fail:" + Environment.NewLine + ex.ToString());
                    }
                }
            }

            return result;
        }

        private void CreateLogFileEnv()
        {
            ms_threadMutex.WaitOne();

            try
            {
                // 先判斷 log 資料夾是否存在
                if (!System.IO.Directory.Exists(m_logDirPath))
                {
                    this.CreateDir(m_logDirPath);
                    // System.IO.Directory.CreateDirectory(m_logDirPath);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Log --> createLogFileEnv() " + ex.ToString());
            }

            ms_threadMutex.ReleaseMutex();
        }

        public bool ReCreateLogDir(string fileName)
        {
            try
            {
                //m_logDirPath = System.Windows.Forms.Application.StartupPath + @"\Log\" + DateTime.Now.ToString("yyyyMMdd");
                mInstance.CreateLogFileEnv();
                string tmp = m_logDirPath + @"\" + TMP_FILE; //+ OperatorSingleton.Instance.LotNo + "_"
                temp_logFileName = m_logDirPath + @"\" + fileName + "_" + DateTime.Now.ToString("yyMMdd_HHmmss") + @".log"; //+ OperatorSingleton.Instance.LotNo +
                //m_logFileName = m_logDirPath + @"\" + OperatorSingleton.Instance.LotNo + "_" + fileName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + @".log";
                if (System.IO.File.Exists(tmp))
                {
                    System.IO.File.Move(tmp, temp_logFileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Log() --> " + ex.ToString());
                return false;
            }

            return true;
        }

        public long UnixTimeNow()
        {
            var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)timeSpan.TotalSeconds;
        }

        public void WriteLog(string content, int type = 0)
        {
            ms_threadMutex.WaitOne();
            if (temp_logFileName.Length == 0)
            {
                ms_threadMutex.ReleaseMutex();
                return;
            }

            string seampore = "";
            //nowDateTime = @"[ " + DateTime.UtcNow.AddHours(8).ToString(@"yyyy/MM/ dd HH:mm:ss") + @"_Send Command ] : ";
            //nowDateTime = @"[ " + DateTimeOffset.Now.ToUnixTimeSeconds().ToString() + @"_Send Receive Command  Measured Value  Unexpected Result] : ";
            if (type == LogSingleton.wzSEND_COMMAND)
            {
                seampore = @" Send Command   ";
            }
            else if (type == LogSingleton.wzRECEIVE_COMMAND)
            {
                seampore = @" Receive Command";
            }
            else if (type == LogSingleton.wzMEASURE_VALUE)
            {
                seampore = @" Measured Value ";
            }
            else if (type == LogSingleton.wzDate_Time_Start)
            {
                seampore = @" Running        ";
            }
            else if (type == LogSingleton.wzEND_TESTING)
            {
                seampore = @"  Terminate Testing";
            }
            else if (type == LogSingleton.wzERROR)
            {
                seampore = @"Err";
            }
            else
            {
                seampore = @"_ Unexpected Result";
            }
            FileStream fStream = new System.IO.FileStream(temp_logFileName, System.IO.FileMode.Append, System.IO.FileAccess.Write);
            StreamWriter sWriter = new System.IO.StreamWriter(fStream, Encoding.Unicode);
            sWriter.WriteLine(String.Format(@"[ {0, -10} | {1, -13} ] {2, -5}", DateTime.UtcNow.AddHours(8).ToString(@"yyyy/MM/dd HH:mm:ss"), seampore, content));
            sWriter.Close();
            fStream.Close();
            ms_threadMutex.ReleaseMutex();
        }
    }
}