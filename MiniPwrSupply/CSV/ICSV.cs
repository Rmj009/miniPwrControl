using System;

namespace MiniPwrSupply.CSV
{
    internal interface ICsv
    {
        void Clear();

        void SetCsvTempFilePath(string tempPath);

        void SetCsvReleaseFilePath(string releasePath);

        void SetIgnoreResultTitle(string item);

        void AddTestResultItem(Object item);

        void ChangeLastResultErrorCodeAndException(string errorCode, string err);

        void ChangeLastResultTrayData(string trayDutSN, string trayName, string trayX, string trayY);

        void SetBeginTestTime(string value);

        void SetEndTestTime(string value);

        void CalcFinalResult();

        void GetCalcFinishResult(ref double fYieldRate, ref int total, ref int pass, ref int fail);

        string GetLastResult();

        string GetResultTitleJSON();

        string GetResultLimitUpperJson();

        string GetResultLimitLowerJson();

        void DebugShowAll();

        void DeleteEmptyTestResultCsv();
    }
}