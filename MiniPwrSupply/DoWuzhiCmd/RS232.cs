using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using RFTestTool.Instrument;

namespace MiniPwrSupply.WuizhiCmd
{
    interface IWuzhiCmd
    {
        bool IsTesting();
        void SetIsTesting(bool isTesting);
        void TakeInitiatives();
        //void StartToTest(object testItem);
        void Dispose();
        void IDispose();
        void DoReceive();

        void CreateInstrumentObject();

        void SetICSystemIni(object iniObject);

        //void SetRunResultCallback(Action<String, bool> resultCallback);

        void SetUICallback(Action<string> uiCallback);

        void SetLogCallback(Action<string, UInt32> logAction);

        //void SetDAQInstrument(IDAQInstrument dAQInstrument);

        //void SetRFInstrument(IRFInstrument rFInstrument);

        //void SetPowerSupplyInstrument(IPowerSupplyInstrument powerSupplyInstrument);

        //void SetBatteryPowerSupplyInstrument(IPowerSupplyInstrument powerSupplyInstrument);

        //void SetSwitchPowerSupplyInstrument(IPowerSupplyInstrument switchInstrument);

        //void SetThermalDetectPowerSupplyInstrument(IPowerSupplyInstrument thermalInstrument);

        void StartToTestFromMacServer(object testItem);

        void FailEraseFlash();

        string GetErrorCode();

        void unlock(string key);
    }
}
