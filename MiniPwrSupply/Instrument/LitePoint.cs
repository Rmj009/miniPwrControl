using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Text.RegularExpressions;
using WNC.API;
using EventHandle;

namespace MiniPwrSupply.Instrument
{
    public class LitePoint
    {
        private string ToolPath = string.Empty;
        private string SummaryPath = string.Empty;

        private string logPath = string.Empty;
        public string LogPath
        {
            get
            {
                return this.logPath;
            }
        }

        private string sn = string.Empty;
        public string Sn
        {
            get
            {
                return this.sn;
            }
        }

        public static event EventLogHandler Message;

        protected virtual void OnMessageDisplay(EventLogArgs e)
        {
            if (Message != null)
                Message(e);
        }

        private void DisplayMsg(LogType type, string message)
        {
            EventLogArgs eLog = new EventLogArgs("[ " + type.ToString() + " ]  " + message);
            OnMessageDisplay(eLog);
        }

        public bool SetParameter(string ToolPath)
        {
            try
            {
                Func.WriteINI(Path.GetDirectoryName(ToolPath), "123", "A", "B", "1");
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
            }
            return true;
        }

        public bool Start()
        {
            try
            {
                DisplayMsg(LogType.Log, "Test ATSuite");
                if (File.Exists(SummaryPath))
                {
                    File.Delete(SummaryPath);
                    DisplayMsg(LogType.Log, "Delete " + SummaryPath);
                }
                CheckAndEnableTools(ToolPath);
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, @"Sart() NG >>> " + ex.Message);
            }
            return SetParameter(ToolPath);
        }

        public void CloseTool()
        {
            //Func.WriteINI(Path.GetDirectoryName(SummaryPath), Path.GetFileName(SummaryPath), "System", "Action", "99");
            KillTaskProcess("ATSuite");
        }

        /// <returns>回應為true代表有出現Summary log而不是指PASS/FAIL</returns>
        public bool WaitResult(int timeOutMs, string logAll_csv)
        {
            DateTime dt = DateTime.Now;
            string SummaryContent = string.Empty;
            //int retryConfirm = 0;
            while (true)
            {
                TimeSpan ts = new TimeSpan(DateTime.Now.Ticks - dt.Ticks);
                if (ts.TotalMilliseconds > timeOutMs)
                {
                    DisplayMsg(LogType.Error, "Wait ATSuite.exe test result timeout!!");
                    return false;
                }
                if (File.Exists(SummaryPath)) // once the ATSuite done the job
                {
                    SummaryContent = File.ReadAllText(SummaryPath);
                    DisplayMsg(LogType.Log, "SummaryContent : " + SummaryContent);
                    //if (SummaryContent.Contains("PASS"))
                    if (SummaryContent.Contains("PASS") || SummaryContent.Contains("FAIL")) // FAIL
                    {
                        Thread.Sleep(5000);
                        if (File.Exists(logAll_csv))
                        {
                            return true;
                        }
                        continue;
                    }
                    else
                    {
                        MessageBox.Show(" debug WHY false??");
                        return false;
                    }
                }
                else
                {
                    Thread.Sleep(1000);
                }
                //if (File.Exists(SummaryPath))
                //{
                //    SummaryContent = File.ReadAllText(SummaryPath);
                //    DisplayMsg(LogType.Log, "SummaryContent : " + SummaryContent);
                //    //if (SummaryContent.Contains("PASS"))
                //    if (SummaryContent.Contains("**** P A S S ****") || SummaryContent.Contains("FAIL"))
                //    {
                //        return true;
                //    }
                //    else
                //    {
                //        return false;
                //    }
                //}
                //else
                //{
                //    System.Threading.Thread.Sleep(1000);
                //}
            }
        }

        public void ExistTool()
        {
            KillTaskProcess(Path.GetFileNameWithoutExtension(ToolPath));
        }

        private void CheckAndEnableTools(string toolsPath)
        {
            try
            {
                if (toolsPath.Trim().Length == 0)
                {
                    return;
                }

                string[] toolPathList = toolsPath.Split(new char[] { ',' });

                foreach (string toolPath in toolPathList)
                {
                    if (!File.Exists(toolPath))
                    {
                        continue;
                    }

                    if (!CheckToolExist(toolPath))
                    {
                        OpenTestTool(toolPath);
                        System.Threading.Thread.Sleep(5 * 1000);
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, "CheckAndEnableTools exception >>>" + ex.Message);
            }
        }

        private bool CheckToolExist(string toolPath)
        {
            Process[] prcAppNameAll = Process.GetProcesses();
            foreach (Process appName in prcAppNameAll)
            {
                string execName = Path.GetFileNameWithoutExtension(toolPath);

                if (execName == appName.ProcessName)
                {
                    return true;
                }
            }
            return false;
        }

        private void OpenTestTool(string toolPath)
        {
            Process testTool = new Process();
            //testTool.StartInfo.FileName = toolPath;
            testTool.StartInfo.FileName = Path.GetFileName(toolPath);
            testTool.StartInfo.WorkingDirectory = Path.GetDirectoryName(toolPath);
            testTool.Start();
        }

        private void KillTaskProcess(string taskName)
        {
            try
            {
                Process[] localAll = Process.GetProcesses();

                foreach (Process i in localAll)
                {
                    Regex r;
                    Match m;
                    string strEscape;

                    strEscape = Regex.Escape(taskName);
                    r = new Regex(@strEscape, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                    m = r.Match(i.ProcessName.ToString());

                    if (m.Success)
                    {
                        DisplayMsg(LogType.Log, "Kill task : " + taskName);
                        Process[] killProcess = Process.GetProcessesByName(i.ProcessName.ToString());
                        killProcess[0].Kill();
                        m.NextMatch();
                        DisplayMsg(LogType.Log, "Kill task complete");
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                MessageBox.Show(ex.Message);
            }
        }

        public LitePoint(string toolPath, string SummaryPath)
        {
            this.ToolPath = toolPath;
            this.SummaryPath = SummaryPath;
            //this.sn = sreialNumber;
        }

    }
}
