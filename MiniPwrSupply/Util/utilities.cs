using ATS.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MiniPwrSupply.Util
{
    class ATSUtil
    {
        private bool SwitchDmpMode()
        {
            //if (!CheckGoNoGo())
            //{
            //    return false;
            //}
            bool IsFwGreater = false;
            string res = string.Empty;
            string FWversion = string.Empty;
            string item = $"SwitchDmpMode";
            string keyword = @"root@OpenWrt";
            DisplayMsg(LogType.Log, $"=============== {item} ===============");
            Version targetVerison = new Version("1.0.0.0");
            //Version targetVerison = Version.Parse("1.0.0.0");
            try
            {
                SendAndChk(PortType.SSH, "mt info", keyword, out res, 0, 3000);
                Match m = Regex.Match(res, @"FW Version: (?<FWver>.+)");
                if (m.Success)
                {
                    FWversion = m.Groups["FWver"].Value.Trim().Split('v')[1];
                    DisplayMsg(LogType.Log, "DUT FWversion: " + FWversion);
                    //string a = FWversion.Split('v')[1];
                }
                Version FwVer = Version.Parse(FWversion);
                IsFwGreater = FwVer.CompareTo(targetVerison) > 0 ? true : false;
                //if (FwVer.CompareTo(targetVerison) > 0)
                //{
                //    IsFwGreater = true;
                //}
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, $"{item}" + ex.Message);
                AddData(item, 1);
            }
            return IsFwGreater;
        }
        private void DisplayMsg(LogType type, string msg)
        {
            var TryExamples = new Foo();
            TryExamples.SuspendEvents();
            try
            {
                if (string.Compare(type.ToString(), LogType.Empty.ToString(), true) == 0)
                {
                    status_ATS.AddLog(msg);
                }
                {
                    status_ATS.AddLog("[ " + type.ToString() + " ]" + msg);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                TryExamples.ResumeEvents();
            }
        }

    }
}
