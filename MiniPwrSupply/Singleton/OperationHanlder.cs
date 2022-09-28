using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPwrSupply.Singleton
{
    internal class OperatorSingleton
    {
        private static readonly object mLock = new object();
        private static OperatorSingleton mInstance = null;

        private OperatorSingleton()
        {
        }

        public static OperatorSingleton Instance
        {
            get
            {
                lock (OperatorSingleton.mLock)
                {
                    if (mInstance == null)
                    {
                        mInstance = new OperatorSingleton();
                    }
                }
                return mInstance;
            }
        }

        public void Clear()
        {
            this.CableLosses = "";
            this.ChipFamily = "";
            this.LotNo = "";
            this.PO = "";
            this.Operator = "";
            this.PGMName = "";
            this.ChipName = "";
            this.LoadBoard = "";
            this.Device = "";
            this.Mode = "";
            this.ProductImageName = "";
            this.ProductionImageName = "";
            this.SuperName = "ASECL";
            this.SipSerialName = "";

            this.NetIsAutoGen = false;
            this.NetServerURLS = null;
            this.NetStatus = "";
            this.NetProductFamilyName = "";
            this.NetCustomer = "";
            // this.NetDutComputers = "";
            this.NetFWName = "";
            this.NetFWVersion = "";
            this.NetSWName = "";
            this.NetSWVersion = "";
            this.NetMacDispathType = "";
            this.NetMacCount = -1;
            this.NetMacType = "";
            this.NetMacStart = "";
            this.NetOpId = "";
            this.NetMacName = "";
            this.Flow = "";
            this.NetGroupPC = "";
            this.NetPid = "";

            this.NetTrayMode = false;
            this.NetTrayHeight = 0;
            this.NetTrayWidth = 0;
            //this.NetTrayData = null;
        }

        public string CableLosses { get; set; }

        public string ChipFamily { get; set; }

        public string LotNo { get; set; }

        public string PO { get; set; }

        public string Operator { get; set; }

        public string PGMName { get; set; }

        public string LoadBoard { get; set; }

        public string Device { get; set; }

        public string NetGroupPC { get; set; }

        public string Mode { get; set; }

        public string ChipName { get; set; }

        public string Flow { get; set; }

        public string ProductImageName { get; set; }

        public string ProductionImageName { get; set; }

        // public string
        public string SuperName { get; set; }

        public string SipSerialName { get; set; }

        public bool NetIsAutoGen { get; set; }

        public string[] NetServerURLS { get; set; }
        public string NetStatus { get; set; }
        public string NetProductFamilyName { get; set; }
        public string NetCustomer { get; set; }

        // public string NetDutComputers { get; set; }
        public string NetFWName { get; set; }

        public string NetFWVersion { get; set; }
        public string NetSWName { get; set; }
        public string NetSWVersion { get; set; }
        public string NetMacDispathType { get; set; }
        public int NetMacCount { get; set; }
        public string NetMacType { get; set; }
        public string NetMacStart { get; set; }
        public string NetOpId { get; set; }

        public string NetPid { get; set; }
        public string NetMacName { get; set; }

        public bool NetTrayMode { get; set; }

        public int NetTrayHeight { get; set; }

        public int NetTrayWidth { get; set; }

        //public Newtonsoft.Json.Linq.JObject NetTrayData { get; set; }
    }
}