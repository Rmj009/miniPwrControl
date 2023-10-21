using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPwrSupply.CSV
{
    public interface ICsvItem
    {
        string Result { get; set; }
        string TotalTestTime { get; set; }
        string ERR_Message { get; set; }
        string Machine_Error_Code { get; set; }
        string New_MacAddress { get; set; }
        string PSKEY_Dev_Name { get; set; }
        string Source_MacAddress { get; set; }
    }
}
