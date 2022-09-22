using MiniPwrSupply.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPwrSupply.Singleton
{
    internal class LogSingleton
    {
        //private static volatile LogSingleton m_singletonLog = null;
        ////  private static object m_threadLock = new object();
        //private static System.Threading.Mutex ms_threadMutex = new System.Threading.Mutex();
        public static int NORMAL = 0;

        public static int ERROR = 1;
        public static int SCPI = 2;

        private static readonly object mLock = new object();
        private static LogSingleton mInstance = null;
        private static System.Threading.Mutex ms_threadMutex = new System.Threading.Mutex();

        private const int mc_logMaxCount = 30;
        private string m_logDirPath = System.Windows.Forms.Application.StartupPath + @"\Log\" + DateTime.Now.ToString("yyyyMMdd");
        private static string m_logFileName = "";
        private static string temp_logFileName = "";
        private static string TMP_FILE = "tmp.log";
        private string mRootFlowPath = "";
        private string mRootPath = "";

        public static LogSingleton Instance
        {
            get
            {
                lock (LogSingleton.mLock)
                {
                    if (mInstance == null)
                    {
                        mInstance = new LogSingleton();

                        //string logPath = SystemIni.Instance.LogPath();
                        //string systemLogPath = SystemIni.Instance.LogPath();
                        //string logPath = systemLogPath + @"\Logs\" + OperatorSingleton.Instance.LotNo + @"\" + OperatorSingleton.Instance.Flow + @"\Log\";
                        //if (systemLogPath.Length > 0)
                        //{
                        //    mInstance.mRootFlowPath = systemLogPath + @"\Logs\" + OperatorSingleton.Instance.LotNo + @"\" + OperatorSingleton.Instance.Flow;
                        //    mInstance.mRootPath = systemLogPath + @"\Logs\" + OperatorSingleton.Instance.LotNo;
                        //    mInstance.m_logDirPath = logPath;
                        //    temp_logFileName = mInstance.m_logDirPath + @"\" + OperatorSingleton.Instance.LotNo + "_" + TMP_FILE;
                        //}
                        //else
                        //{
                        //    mInstance.mRootFlowPath = System.Windows.Forms.Application.StartupPath + @"\Logs\" + OperatorSingleton.Instance.LotNo + @"\" + OperatorSingleton.Instance.Flow;
                        //    mInstance.mRootPath = System.Windows.Forms.Application.StartupPath + @"\Logs\" + OperatorSingleton.Instance.LotNo;
                        //    mInstance.m_logDirPath = System.Windows.Forms.Application.StartupPath + @"\Logs\" + OperatorSingleton.Instance.LotNo + @"\" + OperatorSingleton.Instance.Flow + @"\Log\";
                        //}
                    }
                }
                return mInstance;
            }
        }

        public string GetRootFlowPath()
        {
            return mRootFlowPath;
        }

        public string GetRootPath()
        {
            return mRootPath;
        }

        public string GetLogName()
        {
            string[] splits = m_logFileName.Split('\\');
            return splits[splits.Length - 1];
        }

        public string GetLogPath()
        {
            return m_logFileName;
        }

        public string GetTempLogName()
        {
            string[] splits = temp_logFileName.Split('\\');
            return splits[splits.Length - 1];
        }

        public string GetTempPath()
        {
            return temp_logFileName;
        }

        public bool ReCreateLogDir(string fileName)
        {
            try
            {
                //m_logDirPath = System.Windows.Forms.Application.StartupPath + @"\Log\" + DateTime.Now.ToString("yyyyMMdd");
                mInstance.CreateLogFileEnv();
                //string tmp = m_logDirPath + @"\" + OperatorSingleton.Instance.LotNo + "_" + TMP_FILE;
                //temp_logFileName = m_logDirPath + @"\" + OperatorSingleton.Instance.LotNo + "_" + fileName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + @".log_temp";
                //m_logFileName = m_logDirPath + @"\" + OperatorSingleton.Instance.LotNo + "_" + fileName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + @".log";
                //if (System.IO.File.Exists(tmp))
                //{
                //    System.IO.File.Move(tmp, temp_logFileName);
                //}
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(@"Log() --> " + ex.ToString());
                return false;
            }

            return true;
        }

        //public bool CreateWebLogTmpDir()
        //{
        //    try
        //    {
        //        //m_logDirPath = System.Windows.Forms.Application.StartupPath + @"\Log\" + DateTime.Now.ToString("yyyyMMdd");
        //        mInstance.CreateLogFileEnv();
        //        if (OperatorSingleton.Instance.Flow.ToLower().Equals("corr"))
        //        {
        //            OperatorSingleton.Instance.LotNo = "TestCorr";
        //        }
        //        temp_logFileName = m_logDirPath + @"\" + OperatorSingleton.Instance.LotNo + "_" + TMP_FILE;
        //        //temp_logFileName = m_logDirPath + @"\" + OperatorSingleton.Instance.LotNo + "_" + TMP_FILE + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + @".log_temp";
        //        //m_logFileName = m_logDirPath + @"\" + OperatorSingleton.Instance.LotNo + "_" + TMP_FILE + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + @".log";
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Windows.Forms.MessageBox.Show(@"ReCreateWebLogDir() --> " + ex.ToString());
        //        return false;
        //    }
        //    return true;
        //}

        //public void RenameMacFilePath(string mac)
        //{
        //    try
        //    {
        //        ms_threadMutex.WaitOne();
        //        if (System.IO.File.Exists(temp_logFileName))
        //        {
        //            string newFileName = m_logDirPath + @"\" + OperatorSingleton.Instance.LotNo + "_" + mac + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + @".log_temp";
        //            m_logFileName = m_logDirPath + @"\" + OperatorSingleton.Instance.LotNo + "_" + mac + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + @".log";
        //            System.IO.File.Move(temp_logFileName, newFileName);
        //            temp_logFileName = newFileName;
        //        }
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //    finally
        //    {
        //        ms_threadMutex.ReleaseMutex();
        //    }
        //}

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

        //public void MoveLogName(string newName)
        //{
        //    try
        //    {
        //        ms_threadMutex.WaitOne();
        //        if (System.IO.File.Exists(temp_logFileName))
        //        {
        //            string newFileName = m_logDirPath + @"\" + OperatorSingleton.Instance.LotNo + "_" + newName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + @".log_temp";
        //            m_logFileName = m_logDirPath + @"\" + OperatorSingleton.Instance.LotNo + "_" + newName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + @".log";
        //            System.IO.File.Move(temp_logFileName, newFileName);
        //            temp_logFileName = newFileName;
        //        }
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //    finally
        //    {
        //        ms_threadMutex.ReleaseMutex();
        //    }
        //}

        public void SetFinalLog(bool isPass, string testMode = null)
        {
            try
            {
                ms_threadMutex.WaitOne();
                if (System.IO.File.Exists(temp_logFileName))
                {
                    if (m_logFileName.Contains("PASS_") || m_logFileName.Contains("FAIL_"))
                    {
                        string[] logNames = m_logFileName.Split('\\');
                        string logName = logNames[logNames.Length - 1];
                        string[] logs = logName.Split('_');
                        string finalLogName = "";
                        for (int i = 0; i < logs.Length; i++)
                        {
                            if (i == 0 || i == 1 || logs[i].Trim().Length == 0)
                            {
                                continue;
                            }
                            finalLogName += logs[i] + @"_";
                        }

                        int finalLogNameLen = finalLogName.Length;
                        if (finalLogName[finalLogNameLen - 1] == '_')
                        {
                            finalLogName = finalLogName.Substring(0, finalLogNameLen - 1);
                        }

                        m_logFileName = this.m_logDirPath + "\\" + (isPass ? "PASS_" : "FAIL_") + (testMode != null ? testMode + "_" : "") + finalLogName;
                    }
                    else
                    {
                        string[] logNames = m_logFileName.Split('\\');
                        m_logFileName = this.m_logDirPath + "\\" + (isPass ? "PASS_" : "FAIL_") + (testMode != null ? testMode + "_" : "") + logNames[logNames.Length - 1];
                    }

                    System.IO.File.Copy(temp_logFileName, m_logFileName, true);
                    System.IO.File.Delete(temp_logFileName);
                    //temp_logFileName = m_logDirPath + @"\" + OperatorSingleton.Instance.LotNo + "_" + TMP_FILE;
                    //m_logFileName = m_logDirPath + @"\" + OperatorSingleton.Instance.LotNo + "_tmp_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + @".log";
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                ms_threadMutex.ReleaseMutex();
            }
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

            string nowDateTime = "";
            if (type == LogSingleton.NORMAL)
            {
                nowDateTime = @"[ " + DateTime.Now.ToString(@"yyyy/MM/dd HH:mm:ss") + @" ] : ";
            }
            else if (type == LogSingleton.SCPI)
            {
                nowDateTime = @"[ " + DateTime.Now.ToString(@"yyyy/MM/dd HH:mm:ss") + @"_ SCPI ] : ";
            }
            else
            {
                nowDateTime = @"[ " + DateTime.Now.ToString(@"yyyy/MM/dd HH:mm:ss") + @"_ 錯誤 ] : ";
            }
            //string nowDateTime = @"[ " + DateTime.Now.ToString(@"yyyy/MM/dd HH:mm:ss") + @" _ 系統訊息 ] : ";
            // string nowDateTime = @"[ " + DateTime.Now.ToString(@"yyyy/MM/dd HH:mm:ss") + @" ] : ";
            System.IO.FileStream fStream = new System.IO.FileStream(temp_logFileName, System.IO.FileMode.Append, System.IO.FileAccess.Write);
            System.IO.StreamWriter sWriter = new System.IO.StreamWriter(fStream, Encoding.Unicode);
            sWriter.WriteLine(nowDateTime + content);
            sWriter.Close();
            fStream.Close();
            ms_threadMutex.ReleaseMutex();
        }
    }
}