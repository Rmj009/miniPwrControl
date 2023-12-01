using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            //Func.WriteINI(Path.GetDirectoryName(ToolPath), "123", "A", "B", "1");
            return true;
        }

        public bool Start()
        {
            DisplayMsg(LogType.Log, "Test ATSuite");
            if (File.Exists(SummaryPath))
            {
                File.Delete(SummaryPath);
                DisplayMsg(LogType.Log, "Delete " + SummaryPath);
            }
            CheckAndEnableTools(ToolPath);
            return SetParameter(ToolPath);
        }

        public void CloseTool()
        {
            //Func.WriteINI(Path.GetDirectoryName(SummaryPath), Path.GetFileName(SummaryPath), "System", "Action", "99");
            KillTaskProcess("ATSuite");
        }

        /// <returns>回應為true代表有出現Summary log而不是指PASS/FAIL</returns>
        public bool WaitResult(int timeOutMs)
        {
            DateTime dt = DateTime.Now;
            string SummaryContent = string.Empty;
            DisplayMsg(LogType.Log, $"Current timeout 'ATSuite.exe exits in setting is : '{timeOutMs}'");
            while (true)
            {
                TimeSpan ts = new TimeSpan(DateTime.Now.Ticks - dt.Ticks);
                if (ts.TotalMilliseconds > timeOutMs)
                {
                    DisplayMsg(LogType.Error, "Wait ATSuite.exe test result timeout!!"); //tool跑太久 delay 10s 太多次
                    return false;
                }
                if (!CheckToolExist(Path.GetFileNameWithoutExtension(this.ToolPath)) && !File.Exists(SummaryPath)) //copy from LS04 ATS
                {
                    DisplayMsg(LogType.Log, "The tool is closed!! Goto retry");    //TOOL提前結束  大約兩分鐘左右關閉ATSuite
                    return false;
                }
                if (File.Exists(SummaryPath))
                {
                    SummaryContent = File.ReadAllText(SummaryPath);
                    DisplayMsg(LogType.Log, "SummaryContent : " + SummaryContent);
                    //if (SummaryContent.Contains("PASS"))
                    if (SummaryContent.Contains("P A S S"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    DisplayMsg(LogType.Log, "Delay 1s");
                    System.Threading.Thread.Sleep(1 * 1000);
                }
            }
        }

        public void ExistTool()
        {
            KillTaskProcess(Path.GetFileNameWithoutExtension(ToolPath));
        }

        private void CheckAndEnableTools(string toolsPath)
        {
            if (toolsPath.Trim().Length == 0)
            {
                return;
            }

            string[] toolPathList = toolsPath.Split(new char[] { ',' });
            string tool = "";
            foreach (string toolPath in toolPathList)
            {
                if (!File.Exists(toolPath))
                {
                    DisplayMsg(LogType.Log, $"Check '{toolPath}' not exist!");
                    continue;
                }
                tool = toolPath;
                if (!CheckToolExist(toolPath))
                {
                    OpenTestTool(toolPath);
                    //DisplayMsg(LogType.Log, "Delay 5s...");
                    //System.Threading.Thread.Sleep(5 * 1000);
                    break;
                }
                else
                {
                    KillTaskProcess("ATSuite");
                    OpenTestTool(toolPath);
                    break;
                }
            }
            bool check = false;
            for (int i = 0; i < 5; i++)
            {
                if (CheckToolExist(tool))
                {
                    check = true;
                    DisplayMsg(LogType.Log, "Check the tool is existed!");
                    break;
                }
                System.Threading.Thread.Sleep(2000);
            }
            if (!check)
            { MessageBox.Show("Open tool fail"); }
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

