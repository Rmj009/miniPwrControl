using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MiniPwrSupply.CSV
{
    public abstract class CsvBase<TEST_ITEM> : ICsv
    {
        protected System.Threading.Mutex mThreadMutex = new System.Threading.Mutex();
        protected DataTable mOperationTable = new DataTable();
        protected DataTable mCalCountTable = new DataTable();
        protected DataTable mResultTable = new DataTable();
        private List<string> CalCountLists = new List<string>();
        protected List<string> mIgnoreTestItems = new List<string>();
        protected string mCsvTempPath = "";
        protected string mCsvReleasePath = "";
        protected int OP_Begin_TestTime_Index = 11;
        protected int OP_End_TestTime_Index = 12;
        protected List<TEST_ITEM> mTestItems = new List<TEST_ITEM>();
        protected TEST_ITEM mLimitUpper = default(TEST_ITEM);
        protected TEST_ITEM mLimitLower = default(TEST_ITEM);

        #region outside interface

        public void Clear()
        {
            this.mOperationTable.Clear();
            this.mCalCountTable.Clear();
            this.mResultTable.Clear();
            this.mTestItems.Clear();
        }

        public virtual void CalcFinalResult()
        {
            throw new NotImplementedException();
        }

        public void SetCsvTempFilePath(string tempPath)
        {
            this.mCsvTempPath = tempPath;
        }

        public void SetCsvReleaseFilePath(string releasePath)
        {
            this.mCsvReleasePath = releasePath;
        }

        public void SetIgnoreResultTitle(string item)
        {
            this.mIgnoreTestItems.Add(item);
            if (this.mResultTable.Columns[item] != null)
            {
                this.mResultTable.Columns.Remove(item);
            }
        }

        public virtual void AddTestResultItem(object item)
        {
            throw new NotImplementedException();
        }

        public virtual void GetCalcFinishResult(ref double fYieldRate, ref int total, ref int pass, ref int fail)
        {
            throw new NotImplementedException();
        }

        private void _ClearForEndTime()
        {
            this.mOperationTable.Clear();
            this.mCalCountTable.Clear();
            this.mResultTable.Clear();
        }

        public void SetBeginTestTime(string value)
        {
            this.Clear();
            //this._InitOperationTable();
            this._InitResultCountTable();
            this._InitResultItemTable();
            mOperationTable.Rows[OP_Begin_TestTime_Index]["OpValue"] = value;
            this._saveInfoTablesToCsv();
            this._InitResultItemLimitUpperAndLower();
            this._InitResultItemTitleToCsv();
        }

        public void SetEndTestTime(string value)
        {
            mOperationTable.Rows[OP_End_TestTime_Index]["OpValue"] = value;
        }

        public string GetLastResult()
        {
            if (this.mTestItems.Count == 0)
            {
                return "";
            }
            return null;
            //var json = new Newtonsoft.Json.Linq.JObject();
            //TEST_ITEM lastItem = this.mTestItems.Last();
            //Type myType = lastItem.GetType();
            //PropertyInfo[] props = myType.GetProperties();
            //for (int i = 0; i < props.Length; i++)
            //{
            //    string methodName = ((PropertyInfo)props[i]).Name;
            //    string methodValue = "";
            //    if (this.mIgnoreTestItems.Contains(methodName))
            //    {
            //        continue;
            //    }
            //    if (((PropertyInfo)props[i]).GetValue(lastItem) != null)
            //    {
            //        methodValue = ((PropertyInfo)props[i]).GetValue(lastItem).ToString().Trim();
            //    }

            //    json.Add(methodName, methodValue);
            //}
            //return json.ToString().Replace("\r\n", "");
        }

        public string GetResultTitleJSON()
        {
            //var json = new Newtonsoft.Json.Linq.JObject();
            //Type myType = (typeof(TEST_ITEM));
            //PropertyInfo[] props = myType.GetProperties();
            //for (int i = 0; i < props.Length; i++)
            //{
            //    PropertyInfo myMethodInfo = (PropertyInfo)props[i];
            //    json.Add(myMethodInfo.Name, myMethodInfo.Name);
            //}
            ////Console.WriteLine("json title : " + json.ToString());
            //return json.ToString().Replace("\r\n", "");
            return null;
        }

        public string GetResultLimitLowerJson()
        {
            //var json = new Newtonsoft.Json.Linq.JObject();
            Type myType = this.mLimitLower.GetType();
            PropertyInfo[] props = myType.GetProperties();
            for (int i = 0; i < props.Length; i++)
            {
                string methodName = ((PropertyInfo)props[i]).Name;
                string methodValue = "";
                if (this.mIgnoreTestItems.Contains(methodName))
                {
                    continue;
                }
                if (((PropertyInfo)props[i]).GetValue(this.mLimitLower) != null)
                {
                    methodValue = ((PropertyInfo)props[i]).GetValue(this.mLimitLower).ToString().Trim();
                }

                //json.Add(methodName, methodValue);
            }
            return null;
            //json.ToString().Replace("\r\n", "");
        }

        public string GetResultLimitUpperJson()
        {
            //var json = new Newtonsoft.Json.Linq.JObject();
            Type myType = this.mLimitUpper.GetType();
            PropertyInfo[] props = myType.GetProperties();
            for (int i = 0; i < props.Length; i++)
            {
                string methodName = ((PropertyInfo)props[i]).Name;
                string methodValue = "";
                if (this.mIgnoreTestItems.Contains(methodName))
                {
                    continue;
                }
                if (((PropertyInfo)props[i]).GetValue(this.mLimitUpper) != null)
                {
                    methodValue = ((PropertyInfo)props[i]).GetValue(this.mLimitUpper).ToString().Trim();
                }

                //json.Add(methodName, methodValue);
            }
            return null;
            //json.ToString().Replace("\r\n", "");
        }

        public void DebugShowAll()
        {
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("------------ OperationTable ------------");

            foreach (DataRow item in mOperationTable.Rows)
            {
                Console.WriteLine("item : " + item[0] + ", value : " + item[1]);
            }

            Console.WriteLine("----------------------------------------");
            Console.WriteLine("------------ CalCountTable ------------");

            foreach (DataRow item in mCalCountTable.Rows)
            {
                Console.WriteLine("item : " + item[0] + ", value : " + item[1]);
            }

            Console.WriteLine("----------------------------------------");
            Console.WriteLine("------------ ResultTable ------------");
            foreach (DataRow item in mResultTable.Rows)
            {
                Console.WriteLine("item : " + item[0] + ", value : " + item[1]);
            }
        }

        public void DeleteEmptyTestResultCsv()
        {
            if (mTestItems.Count == 0)
            {
                try
                {
                    System.IO.File.Delete(this.mCsvTempPath);
                    System.IO.File.Delete(this.mCsvReleasePath);
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show("Log --> [no Item in .csv]  DeleteEmptyTestResultCsv() " + ex.ToString());
                }
            }
        }

        public abstract void ChangeLastResultErrorCodeAndException(string errorCode, string err);

        public abstract void ChangeLastResultTrayData(string trayDutSN, string trayName, string trayX, string trayY);

        #endregion outside interface

        #region Initial DataTable Column

        protected void _InitOperationTable()
        {
            //this._addTableColumnType(ref mOperationTable, "OpItem", typeof(string));
            //this._addTableColumnType(ref mOperationTable, "OpValue", typeof(string));
            //int dutDispatchType = UtilsSingleton.Instance.GetDUT_Dispatch_Mac_Type();
            //int testMode = SystemIni.Instance.DUT_TestMode();
            //mOperationTable.Rows.Add("LotNo", OperatorSingleton.Instance.LotNo);
            //mOperationTable.Rows.Add("SitePCName", UtilsSingleton.Instance.GetCurrentComputerName());
            //mOperationTable.Rows.Add("Operation Name", OperatorSingleton.Instance.Operator);
            //mOperationTable.Rows.Add("PGM Name", OperatorSingleton.Instance.PGMName);
            //mOperationTable.Rows.Add("Load Board", OperatorSingleton.Instance.LoadBoard);
            //mOperationTable.Rows.Add("Device", OperatorSingleton.Instance.Device);
            //mOperationTable.Rows.Add("Mode", OperatorSingleton.Instance.Mode);
            //mOperationTable.Rows.Add("Mac Dispatch Type", dutDispatchType == UtilsSingleton.TEST_DISPATH_MAC_TYPE_FAKE ? "Fake" : "Real");

            //string testModeStr = "";
            //if (testMode == 1)
            //{
            //    testModeStr = "Production";
            //}
            //else if (testMode == 2)
            //{
            //    testModeStr = "Production + Product";
            //}
            //else if (testMode == 3)
            //{
            //    testModeStr = "Product";
            //}
            //else if (testMode == 4)
            //{
            //    testModeStr = "Production + Erase";
            //}

            //mOperationTable.Rows.Add("Flow", OperatorSingleton.Instance.Flow + " ( " + testModeStr + " )");
            ////if (testMode == UtilsSingleton.TEST_MODE_Production || testMode == UtilsSingleton.TEST_MODE_Production_Product)
            ////{
            ////    mOperationTable.Rows.Add("Production Image", OperatorSingleton.Instance.ProductionImageName);
            ////}

            ////if (testMode == UtilsSingleton.TEST_MODE_Product || testMode == UtilsSingleton.TEST_MODE_Production_Product)
            ////{
            ////    mOperationTable.Rows.Add("Product Image", OperatorSingleton.Instance.ProductImageName);
            ////}
            //mOperationTable.Rows.Add("Production Image", OperatorSingleton.Instance.ProductionImageName);
            //mOperationTable.Rows.Add("Product Image", OperatorSingleton.Instance.ProductImageName);
            ////  mOperationTable.Rows.Add("Product Image");
            //mOperationTable.Rows.Add("BeginTestTime", "");

            //mOperationTable.Rows.Add("EndTestTime", "");
        }

        private void _AddItemToList(List<string> items, string data)
        {
            if (!items.Contains(data))
            {
                items.Add(data);
            }
        }

        protected void _InitResultCountTable()
        {
            this._AddItemToList(this.CalCountLists, ("Pass yield rate"));
            this._AddItemToList(this.CalCountLists, ("Total Count"));
            this._AddItemToList(this.CalCountLists, ("Pass Count"));
            this._AddItemToList(this.CalCountLists, ("Fail Count"));
            this._AddItemToList(this.CalCountLists, ("Cable Loss"));
            this._addTableColumnType(ref mCalCountTable, "CountItem", typeof(string));
            this._addTableColumnType(ref mCalCountTable, "CountValue", typeof(string));
            foreach (string item in CalCountLists)
            {
                mCalCountTable.Rows.Add(item, "");
            }
        }

        protected void _InitResultItemTable()
        {
            Type myType = (typeof(TEST_ITEM));
            PropertyInfo[] props = myType.GetProperties();
            for (int i = 0; i < props.Length; i++)
            {
                string methodName = ((PropertyInfo)props[i]).Name;
                if (this.mIgnoreTestItems.Contains(methodName))
                {
                    continue;
                }

                this._addTableColumnType(ref mResultTable, methodName, typeof(string));
            }
        }

        #endregion Initial DataTable Column

        #region abstract function

        protected abstract void _InitResultItemLimitUpperAndLower();

        protected abstract string _AliasHeaderName(string srcHeaderName);

        #endregion abstract function

        protected void _setFinalCount(string passRate, string total, string pass, string fail, string cableloss = null)
        {
            mCalCountTable.Rows[_getIndexByStrList(ref CalCountLists, "Pass yield rate")]["CountValue"] = passRate;
            mCalCountTable.Rows[_getIndexByStrList(ref CalCountLists, "Total Count")]["CountValue"] = total;
            mCalCountTable.Rows[_getIndexByStrList(ref CalCountLists, "Pass Count")]["CountValue"] = pass;
            mCalCountTable.Rows[_getIndexByStrList(ref CalCountLists, "Fail Count")]["CountValue"] = fail;
            mCalCountTable.Rows[_getIndexByStrList(ref CalCountLists, "Cable Loss")]["CountValue"] = cableloss;
            _reSaveCsvFile();
        }

        private void _InitResultItemTitleToCsv()
        {
            this.mResultTable.Clear();
            if (mLimitUpper == null || mLimitLower == null)
            {
                //throw new UIException("Not Set Upper And Lower !!", "msgCode");
            }
            _addTestItemValueToDataTable(mLimitUpper);
            _addTestItemValueToDataTable(mLimitLower);
            _addTestItemTitleToDataTable();
            this._saveResultToCsv();
        }

        protected void _writeStrToCsv(string content, bool isAppend = true)
        {
            try
            {
                mThreadMutex.WaitOne();
                FileMode mode = FileMode.Open;
                if (System.IO.File.Exists(this.mCsvTempPath))
                {
                    mode = isAppend ? FileMode.Append : FileMode.Truncate;
                }
                else
                {
                    mode = FileMode.CreateNew;
                }
                // FileMode mode = isAppend ? FileMode.Append : FileMode.Truncate;

                using (FileStream fs = new FileStream(this.mCsvTempPath,
                                        mode,
                                        FileAccess.Write,
                                        FileShare.ReadWrite))
                {
                    // System.Text.Encoding.BigEndianUnicode
                    //using (StreamWriter sw = new StreamWriter(fs, Encoding.GetEncoding("UTF-32")))
                    using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                    {
                        sw.Write(content, 0, content.Length);
                        sw.Flush();
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                mThreadMutex.ReleaseMutex();
            }
        }

        protected void _addTableColumnType(ref DataTable dataTable, string columnName, Type type)
        {
            DataColumnCollection columns = dataTable.Columns;
            if (!columns.Contains(columnName))
            {
                dataTable.Columns.Add(columnName, type);
            }
        }

        protected void _addTestItemValueToDataTable(TEST_ITEM obj)
        {
            Type myType = obj.GetType();
            PropertyInfo[] props = myType.GetProperties();
            DataRow row = mResultTable.NewRow();
            for (int i = 0; i < props.Length; i++)
            {
                string methodName = ((PropertyInfo)props[i]).Name;
                string methodValue = "";
                if (this.mIgnoreTestItems.Contains(methodName))
                {
                    continue;
                }
                if (((PropertyInfo)props[i]).GetValue(obj) != null)
                {
                    methodValue = ((PropertyInfo)props[i]).GetValue(obj).ToString();
                }
                row[methodName] = methodValue;
            }

            mResultTable.Rows.Add(row);
        }

        protected void _addTestItemTitleToDataTable()
        {
            Type myType = (typeof(TEST_ITEM));
            PropertyInfo[] props = myType.GetProperties();
            DataRow row = mResultTable.NewRow();

            for (int i = 0; i < props.Length; i++)
            {
                string methodName = ((PropertyInfo)props[i]).Name;

                if (this.mIgnoreTestItems.Contains(methodName))
                {
                    continue;
                }

                string testName = _AliasHeaderName(methodName);
                row[methodName] = testName;
            }

            mResultTable.Rows.Add(row);
        }

        private int _getIndexByStrList(ref List<string> srcList, string key)
        {
            for (int i = 0; i < srcList.Count; i++)
            {
                if (srcList[i].Equals(key))
                {
                    return i;
                }
            }
            return -1;
        }

        private void _saveResultToCsv()
        {
            int totalRowsCount = this.mResultTable.Rows.Count;
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < totalRowsCount; i++)
            {
                // --- OP
                for (int x = 0; x < this.mResultTable.Columns.Count; x++)
                {
                    if (x == (this.mResultTable.Columns.Count - 1))
                    {
                        sb.Append(this.mResultTable.Rows[i][x].ToString() + "\r\n");
                    }
                    else
                    {
                        sb.Append(this.mResultTable.Rows[i][x].ToString() + ",");
                    }
                }
            }
            this._writeStrToCsv(sb.ToString());
            // System.IO.File.AppendAllText(mPassDir + "aa.csv", sb.ToString(), Encoding.UTF8);
        }

        private void _saveInfoTablesToCsv()
        {
            // ---- merge opinformation && totalcount table
            int totalOpRowsCount = this.mOperationTable.Rows.Count;
            int totalCalCount = this.mCalCountTable.Rows.Count;
            int part1RowCount = totalOpRowsCount >= totalCalCount ? totalOpRowsCount : totalCalCount;

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < part1RowCount; i++)
            {
                // --- OP
                if (totalOpRowsCount <= i)
                {
                    for (int x = 0; x < this.mOperationTable.Columns.Count; x++)
                    {
                        sb.Append(",");
                    }
                }
                else
                {
                    for (int x = 0; x < this.mOperationTable.Columns.Count; x++)
                    {
                        sb.Append(this.mOperationTable.Rows[i][x] + ",");
                    }
                }
                sb.Append(",,");

                // --- TotalCount
                if (totalCalCount <= i)
                {
                    sb.Append("\r\n");
                }
                else
                {
                    for (int x = 0; x < this.mCalCountTable.Columns.Count; x++)
                    {
                        if (x == (this.mCalCountTable.Columns.Count - 1))
                        {
                            sb.Append(this.mCalCountTable.Rows[i][x] + "\r\n");
                        }
                        else
                        {
                            sb.Append(this.mCalCountTable.Rows[i][x] + ",");
                        }
                    }
                }
            }

            sb.Append("\r\n");
            //System.IO.File.WriteAllText(mPassDir + "aa.csv", sb.ToString(), Encoding.UTF8);
            this._writeStrToCsv(sb.ToString(), false);
        }

        protected void _reSaveCsvFile()
        {
            this._saveInfoTablesToCsv();
            this._saveResultToCsv();
            // -- temp remove
            System.IO.File.Copy(this.mCsvTempPath, this.mCsvReleasePath);
            System.IO.File.Delete(this.mCsvTempPath);
        }

        protected void _reSaveTempCsvFile()
        {
            this._saveInfoTablesToCsv();
            this._saveResultToCsv();
        }
    }
}