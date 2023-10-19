using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPwrSupply.CSV
{
    internal class ICsvItem
    {
        class LCS5CsvRFTestItem : ICsvItem
        {
            public string No { get; set; }
            public string Date_Time { get; set; }
            public string END_Time { get; set; }

            public string ERR_Message
            {
                get
                {
                    throw new NotImplementedException();
                }

                set
                {
                    throw new NotImplementedException();
                }
            }

            public string Machine_Error_Code
            {
                get
                {
                    throw new NotImplementedException();
                }

                set
                {
                    throw new NotImplementedException();
                }
            }

            public string New_MacAddress
            {
                get
                {
                    throw new NotImplementedException();
                }

                set
                {
                    throw new NotImplementedException();
                }
            }

            public string PSKEY_Dev_Name
            {
                get
                {
                    throw new NotImplementedException();
                }

                set
                {
                    throw new NotImplementedException();
                }
            }

            public string Result
            {
                get
                {
                    throw new NotImplementedException();
                }

                set
                {
                    throw new NotImplementedException();
                }
            }

            public string Source_MacAddress
            {
                get
                {
                    throw new NotImplementedException();
                }

                set
                {
                    throw new NotImplementedException();
                }
            }

            public string TotalTestTime
            {
                get
                {
                    throw new NotImplementedException();
                }

                set
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
    internal class LCS5Csv : CsvBase<LCS5CsvRFTestItem>
    {
        public override void ChangeLastResultErrorCodeAndException(string errorCode, string err)
        {
            throw new NotImplementedException();
        }

        public override void ChangeLastResultTrayData(string trayDutSN, string trayName, string trayX, string trayY)
        {
            throw new NotImplementedException();
        }

        protected override string _AliasHeaderName(string srcHeaderName)
        {
            throw new NotImplementedException();
        }

        protected override void _InitResultItemLimitUpperAndLower()
        {
            LCS5CsvRFTestItem upperLimit = new LCS5CsvRFTestItem();
            LCS5CsvRFTestItem lowerLimit = new LCS5CsvRFTestItem();

            upperLimit.No = "Upper Limit";
            lowerLimit.No = "Lower Limit";

            this.mLimitUpper = upperLimit;
            this.mLimitLower = lowerLimit;
            //throw new NotImplementedException();
        }
    }
}
