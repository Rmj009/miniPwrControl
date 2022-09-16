using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFTestTool.Util
{
    public class MSG
    {
        public static UInt32 ERROR = 0;
        public static UInt32 NORMAL = 1;
        public static UInt32 TITLE = 2;
        public static UInt32 CUSTOM = 3;
    }

    public class TestItem
    {
        private bool mIsRunEnd = false;
        private string mItemIndex = "";
        private string mBTDevName = "";
        private string mBTMac = "";
        private string mBTLicense = "";
        private string mTestResult = "";
        private bool mCanRunNext = true;
        private bool mIsChangeMacLogName = false;
        private string mNewLogBTMacName = "";

        public string BTDevName
        {
            get
            {
                return this.mBTDevName;
            }

            set
            {
                this.mBTDevName = value;
            }
        }

        public string BTLicense
        {
            get
            {
                return this.mBTLicense;
            }

            set
            {
                this.mBTLicense = value;
            }
        }

        public string BTMac
        {
            get
            {
                return this.mBTMac;
            }

            set
            {
                this.mBTMac = value;
            }
        }

        public bool IsRunEnd
        {
            get
            {
                return this.mIsRunEnd;
            }

            set
            {
                this.mIsRunEnd = value;
            }
        }

        public string ItemIndex
        {
            get
            {
                return this.mItemIndex;
            }

            set
            {
                this.mItemIndex = value;
            }
        }

        public string TestResult
        {
            get
            {
                return this.mTestResult;
            }

            set
            {
                this.mTestResult = value;
            }
        }

        public bool CanRunNext
        {
            get
            {
                return this.mCanRunNext;
            }

            set
            {
                this.mCanRunNext = value;
            }
        }

        public bool IsChangeMacLogName
        {
            get
            {
                return this.mIsChangeMacLogName;
            }

            set
            {
                this.mIsChangeMacLogName = value;
            }
        }

        public string NewLogBTMacName
        {
            get
            {
                return this.mNewLogBTMacName;
            }

            set
            {
                this.mNewLogBTMacName = value;
            }
        }

        public string License { get; set; }
    }

    public class NetworkTestItem
    {
        public NetworkTestItem()
        {
            IsRunEnd = false;
            IsNoMac = true;
            IsFinish = false;
            IsPassByHaveMac = false;
            Name = "";
            Mac = "";
            License = "";
            TestMode = "";
            FlashUUID = "";
            IsChangeMacLogName = false;
            NewLogBTMacName = "";
            TwoD = "";
            TwoD_Model = "";
            LEADTEK_License_Server = "";
            mCanRunNext = true;
            mItemIndex = "";
            TestResult = "";
        }

        public string TestResult { get; set; }

        public bool IsRunEnd { get; set; }

        public string License { get; set; }

        public bool IsNoMac { get; set; }

        public string TwoD { get; set; }

        public string TwoD_Model { get; set; }

        public string Name { get; set; }

        public string Mac { get; set; }

        public string TestMode { get; set; }

        public bool IsChangeMacLogName { get; set; }

        public string NewLogBTMacName { get; set; }

        public string FlashUUID { get; set; }

        public bool IsFinish { get; set; }

        public bool IsPassByHaveMac { get; set; }

        public string LEADTEK_License_Server { get; set; }

        public string License_Dongle_Vendor { get; set; }

        public bool mCanRunNext { get; set; }

        public string mItemIndex { get; set; }

    }
}