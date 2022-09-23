using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using RFTestTool.Config;
//using RFTestTool.Instrument;
//using RFTestTool.Singleton;

namespace MiniPwrSupply.DoWuzhiCmd
{
    public abstract class WuzhiCmdBase
    {
        private bool mIsTesting = false;
        private System.Threading.Mutex mTestingMutex = new System.Threading.Mutex();
        public bool IsTesting()
        {
            bool isTesting = false;
            mTestingMutex.WaitOne();
            isTesting = this.mIsTesting;
            mTestingMutex.ReleaseMutex();
            return isTesting;
        }
        public void SetIsTesting(bool isTesting)
        {
            mTestingMutex.WaitOne();
            this.mIsTesting = isTesting;
            mTestingMutex.ReleaseMutex();
        }

        //protected IDAQInstrument CreateDAQInstr(IChipSystemIni config)
        //{
        //    if (config.IsDAQInstrumentEnable())
        //    {
        //        object obj = UtilsSingleton.Instance.GetInstrumentObject(config.GetDAQInstrumentModel());
        //        if (obj == null)
        //        {
        //            config.SetDAQInstrument_Enable(false);
        //            throw new Exception(@"Not Found DAQ Instrument : " + config.GetDAQInstrumentModel());
        //        }
        //        else
        //        {
        //            return (IDAQInstrument)obj;
        //        }
        //    }
        //    return null;
        //}

        //protected IDAQInstrument CreateGPIODAQInstr(IChipSystemIni config)
        //{
        //    if (config.IsGPIODAQInstrumentEnable())
        //    {
        //        object obj = UtilsSingleton.Instance.GetInstrumentObject(config.GetGPIODAQInstrumentModel());
        //        if (obj == null)
        //        {
        //            config.SetGPIODAQInstrument_Enable(false);
        //            throw new Exception(@"Not Found GPIO DAQ Instrument : " + config.GetGPIODAQInstrumentModel());
        //        }
        //        else
        //        {
        //            return (IDAQInstrument)obj;
        //        }
        //    }
        //    return null;
        //}

        //protected INI_DAQInstrument CreateNIDAQInstr(IChipSystemIni config)
        //{
        //    if (config.IsNIDAQInstrumentEnable())
        //    {
        //        object obj = UtilsSingleton.Instance.GetInstrumentObject(config.GetNIDAQModel());
        //        if (obj == null)
        //        {
        //            config.SetNIDAQInstrument_Enable(false);
        //            throw new Exception(@"Not Found NI DAQ Instrument : " + config.GetNIDAQModel());
        //        }
        //        else
        //        {
        //            return (INI_DAQInstrument)obj;
        //        }
        //    }
        //    return null;
        //}

        //protected IPowerSupplyInstrument CreatePowerSupplyInstr(IChipSystemIni config)
        //{
        //    if (config.IsPowerSupplyEnable())
        //    {
        //        object obj = UtilsSingleton.Instance.GetInstrumentObject(config.GetPowerSupplyModel());
        //        if (obj == null)
        //        {
        //            config.SetPowerSupplyInstrument_Enable(false);
        //            throw new Exception(@"Not Found PowerSupply Instrument : " + config.GetPowerSupplyModel());
        //        }
        //        else
        //        {
        //            return (IPowerSupplyInstrument)obj;
        //        }
        //    }
        //    return null;
        //}

        //protected IRFInstrument CreateRFInstr(IChipSystemIni config)
        //{
        //    if (config.IsRFInstrumentEnable())
        //    {
        //        object obj = UtilsSingleton.Instance.GetInstrumentObject(config.GetRFInstrumentModel());
        //        if (obj == null)
        //        {
        //            config.SetRFInstrument_Enable(false);
        //            throw new Exception(@"Not Found RF Instrument : " + config.GetRFInstrumentModel());
        //        }
        //        else
        //        {
        //            IRFInstrument instr = (IRFInstrument)obj;
        //            if (config.GetRFInstrumentModel().Equals("MT8872A"))
        //            {
        //                ((MT8872A)instr).IsMultiPortEnable = config.IsMT8872A_MutliPortEnable();
        //                ((MT8872A)instr).MultiPortFirstTcpIP = config.GetMT8872A_MutliPortFirstTcpIP();
        //                ((MT8872A)instr).MultiPortFirstErrorDelay = config.GetMT8872A_MutliPortMutliPortFailConnectDelay();
        //                ((MT8872A)instr).IsMultiPortUtilityToolIsAlreadySetting = config.IsMT8872A_MultiPortUtilityToolIsAlreadySetting();
        //            }
        //            return instr;
        //        }
        //    }
        //    return null;
        //}

        //protected ISpectrumInstrument CreateSpectrumInstr(IChipSystemIni config)
        //{
        //    if (config.IsSpectrumInstrumentEnable())
        //    {
        //        object obj = UtilsSingleton.Instance.GetInstrumentObject(config.GetSpectrumModel());
        //        if (obj == null)
        //        {
        //            config.SetSpectrumInstrument_Enable(false);
        //            throw new Exception(@"Not Found Spectrum Instrument : " + config.GetSpectrumModel());
        //        }
        //        else
        //        {
        //            return (ISpectrumInstrument)obj;
        //        }
        //    }
        //    return null;
        //}

        //protected void SafeCloseInstrument(Instrument.IInstrument instrument)
        //{
        //    if (instrument != null)
        //    {
        //        instrument.Close();
        //    }
        //}

        //protected void SafeDisposeInstrument(Instrument.IInstrument instrument)
        //{
        //    if (instrument != null)
        //    {
        //        instrument.IDispose();
        //    }
        //}


    }
}