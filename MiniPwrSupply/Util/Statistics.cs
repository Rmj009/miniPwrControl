using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using static System.Net.Mime.MediaTypeNames;
using System.IO;

namespace ControlSystem
{
    public partial class Statistics : Form
    {
        DataTable dtAll;
        DataTable dtPass;
        DataTable dtNG;
        DataTable dtWarning;
        public Statistics(DataTable dtAll, DataTable dtPass, DataTable dtNG, DataTable dtWarning)
        {
            
            this.dtAll = new DataTable();
            this.dtPass = new DataTable();
            this.dtNG = new DataTable();
            this.dtWarning = new DataTable();
            this.dtAll = dtAll.Copy();
            this.dtPass = dtPass.Copy();
            this.dtNG = dtNG.Copy();
            this.dtWarning = dtWarning.Copy();
            List<string> xValuesChart1 = new List<string>();
            List<int> yValuesChart1 = new List<int>();
            List<string> xValuesChart2 = new List<string>();
            List<int> yValuesChart2 = new List<int>();
            List<string> xValuesChart3 = new List<string>();
            List<int> yValuesChart3 = new List<int>();
            List<string> xValuesChart4 = new List<string>();
            List<int> yValuesChart4 = new List<int>();

            this.chart1.Series[0]["PieLabelStyle"] = "Outside";
            this.chart1.Series[0]["PieLineColor"] = "Black";
            this.chart2.Series[0]["PieLabelStyle"] = "Outside";
            this.chart2.Series[0]["PieLineColor"] = "Black";
            this.chart3.Series[0]["PieLabelStyle"] = "Outside";
            this.chart3.Series[0]["PieLineColor"] = "Black";
            this.chart4.Series[0]["PieLabelStyle"] = "Outside";
            this.chart4.Series[0]["PieLineColor"] = "Black";

            // Rena: Chart1: 測試結果比例
            #region Chart1
            Hashtable ht1_1 = new Hashtable();
            for (int i = 0; i < dtAll.Rows.Count; i++)
            {
                string result = dtAll.Rows[i][5].ToString();
                if (!ht1_1.ContainsKey(result))
                {
                    ht1_1.Add(result, 1);
                }
                else
                {
                    int num = Convert.ToInt32(ht1_1[result]);
                    num++;
                    ht1_1[result] = num;
                }
            }
            foreach (DictionaryEntry de in ht1_1)
            {
                xValuesChart1.Add(de.Key.ToString());
                yValuesChart1.Add(Convert.ToInt32(de.Value));
            }
            this.chart1.Series[0].Points.DataBindXY(xValuesChart1, yValuesChart1);
            #endregion

            // Rena: Chart2: 生產力比例
            // Rena: Chart3: 不良分佈在機台/治具比例
            if (dtAll.Columns.Count > 14)//選了站別
            {
                #region Chart2
                Hashtable ht2_1 = new Hashtable();
                for (int i = 0; i < dtPass.Rows.Count; i++)
                {
                    string insfcs = dtPass.Rows[i][4].ToString();
                    string fixture = dtPass.Rows[i][3].ToString();
                    if (insfcs == "1")
                    {
                        if (!ht2_1.ContainsKey(fixture))
                        {
                            ht2_1.Add(fixture, 1);
                        }
                        else
                        {
                            int num = Convert.ToInt32(ht2_1[fixture]);
                            num++;
                            ht2_1[fixture] = num;
                        }
                    }
                }
                foreach (DictionaryEntry de in ht2_1)
                {
                    xValuesChart2.Add(de.Key.ToString());
                    yValuesChart2.Add(Convert.ToInt32(de.Value));
                }
                this.chart2.Series[0].Points.DataBindXY(xValuesChart2, yValuesChart2);
                #endregion

                #region Chart3
                Hashtable ht3_1 = new Hashtable();
                for (int i = 0; i < dtNG.Rows.Count; i++)
                {
                    string fixture = dtNG.Rows[i][3].ToString();
                    if (!ht3_1.ContainsKey(fixture))
                    {
                        ht3_1.Add(fixture, 1);
                    }
                    else
                    {
                        int num = Convert.ToInt32(ht3_1[fixture]);
                        num++;
                        ht3_1[fixture] = num;
                    }
                }
                foreach (DictionaryEntry de in ht3_1)
                {
                    xValuesChart3.Add(de.Key.ToString());
                    yValuesChart3.Add(Convert.ToInt32(de.Value));
                }
                this.chart3.Series[0].Points.DataBindXY(xValuesChart3, yValuesChart3);
                #endregion

            }
            else //未選站別
            {
                #region Chart2
                Hashtable ht2_2 = new Hashtable();
                for (int i = 0; i < dtPass.Rows.Count; i++)
                {
                    string insfcs = dtPass.Rows[i][4].ToString();
                    string station = dtPass.Rows[i][2].ToString();
                    if (insfcs == "1")
                    {
                        if (!ht2_2.ContainsKey(station))
                        {
                            ht2_2.Add(station, 1);
                        }
                        else
                        {
                            int num = Convert.ToInt32(ht2_2[station]);
                            num++;
                            ht2_2[station] = num;
                        }
                    }
                }
                foreach (DictionaryEntry de in ht2_2)
                {
                    xValuesChart2.Add(de.Key.ToString());
                    yValuesChart2.Add(Convert.ToInt32(de.Value));
                }
                this.chart2.Series[0].Points.DataBindXY(xValuesChart2, yValuesChart2);
                #endregion

                #region Chart3
                Hashtable ht3_2 = new Hashtable();
                for (int i = 0; i < dtNG.Rows.Count; i++)
                {
                    string station = dtNG.Rows[i][2].ToString();
                    if (!ht3_2.ContainsKey(station))
                    {
                        ht3_2.Add(station, 1);
                    }
                    else
                    {
                        int num = Convert.ToInt32(ht3_2[station]);
                        num++;
                        ht3_2[station] = num;
                    }
                }
                foreach (DictionaryEntry de in ht3_2)
                {
                    xValuesChart3.Add(de.Key.ToString());
                    yValuesChart3.Add(Convert.ToInt32(de.Value));
                }
                this.chart3.Series[0].Points.DataBindXY(xValuesChart3, yValuesChart3);
                #endregion
            }

            // Rena: Chart4: 不良Item比例
            #region Chart4
            Hashtable ht4 = new Hashtable();
            for (int i = 0; i < dtNG.Rows.Count; i++)
            {
                string ngitem = dtNG.Rows[i][6].ToString();
                if (!ht4.ContainsKey(ngitem))
                {
                    ht4.Add(ngitem, 1);
                }
                else
                {
                    int num = Convert.ToInt32(ht4[ngitem]);
                    num++;
                    ht4[ngitem] = num;
                }
            }
            foreach (DictionaryEntry de in ht4)
            {
                xValuesChart4.Add(de.Key.ToString());
                yValuesChart4.Add(Convert.ToInt32(de.Value));
            }
            this.chart4.Series[0].Points.DataBindXY(xValuesChart4, yValuesChart4);
            #endregion

            #region 15行資料
            string line15s = "";
            for (int i = 0; i < dtAll.Columns.Count; i++)
            {
                string columnTitle = dtAll.Columns[i].ColumnName;
                if (columnTitle.Length > 4 && columnTitle.Substring(columnTitle.Length - 4, 4) == "_Raw")
                {
                    line15s += columnTitle.Substring(0, columnTitle.Length - 4) + "，";
                }
            }
            label_15lines.Text = line15s.TrimEnd('，');
            #endregion

            #region ATS開啟次數
            int ATSOpenTimes = 0;
            for (int i = 0; i < dtAll.Rows.Count; i++)
            {
                string a = dtAll.Rows[i][12].ToString();
                ATSOpenTimes += Convert.ToInt16(a);
            }
            label_ATSOpenTimes.Text = ATSOpenTimes.ToString() + "次";
            #endregion

            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void chart1_Click(object sender, EventArgs e)
        {
            ZoomChart zoomchart = new ZoomChart("測試結果比例：", dtAll, dtPass, dtNG, dtWarning);
            zoomchart.ShowDialog();
        }

        private void chart2_Click(object sender, EventArgs e)
        {
            ZoomChart zoomchart = new ZoomChart("生產力比例(Pass進系統非Golden)：", dtAll, dtPass, dtNG, dtWarning);
            zoomchart.ShowDialog();
        }

        private void chart3_Click(object sender, EventArgs e)
        {
            ZoomChart zoomchart = new ZoomChart("不良分佈在機台/治具比例：", dtAll, dtPass, dtNG, dtWarning);
            zoomchart.ShowDialog();
        }

        private void chart4_Click(object sender, EventArgs e)
        {
            ZoomChart zoomchart = new ZoomChart("不良Item比例：", dtAll, dtPass, dtNG, dtWarning);
            zoomchart.ShowDialog();
        }
        private void ReadExcelFileToArray()
        {
            string filePath = string.Empty;
            string filePath_new = string.Empty;
            string fileExt = string.Empty;

            if (fileExt.CompareTo(".xls") == 0 || fileExt.CompareTo(".xlsx") == 0)
            {
                try
                {
                    int iDatacount_Columns = 0;
                    int iDatacount_Rows = 0;
                    Excel_APP_GRR = new Excel.Application();
                    Excel_WB1_GRR = Excel_APP_GRR.Workbooks.Open(Directory.GetCurrentDirectory() + "\\Two way anova & GRR_test.xlsx");
                    Excel_APP_GRR_LAWI = new Excel.Application();
                    Excel_APP_GRR_LAWI.Visible = true;
                    Excel_WB1_GRR_LAWI = Excel_APP_GRR_LAWI.Workbooks.Open(Directory.GetCurrentDirectory() + "\\" + WNC.API.Func.ReadINI("setting", "setting", "File_Name", "") + ".xlsx");
                    Excel_WS1_GRR_LAWI = Excel_WB1_GRR_LAWI.Worksheets[1];
                    int iStart = Convert.ToInt32(WNC.API.Func.ReadINI("setting", "setting", "Start", ""));
                    int iEnd = Convert.ToInt32(WNC.API.Func.ReadINI("setting", "setting", "End", ""));
                    for (int Y = iStart; Y <= iEnd; Y++)
                    {
                        Minitab_GRR_LAWI(1, Y);
                        Excel_WS1_GRR_LAWI.Cells[4, Y] = sGRR_Tolerance;
                    }
                    if (File.Exists(filePath_new + ".xlsx"))
                        File.Delete(filePath_new + ".xlsx");
                    Excel_WB1_GRR_LAWI.SaveAs(filePath_new, Excel.XlFileFormat.xlOpenXMLWorkbook, System.Reflection.Missing.Value, System.Reflection.Missing.Value, false, false,
        Excel.XlSaveAsAccessMode.xlExclusive, false, false, System.Reflection.Missing.Value, System.Reflection.Missing.Value, System.Reflection.Missing.Value);
                    Excel_WB1_GRR.Close(0);
                    Excel_APP_GRR.Quit();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(Excel_APP_GRR);
                    Excel_WS1_GRR = null;
                    Excel_WB1_GRR = null;
                    Excel_APP_GRR = null;
                    Excel_WB1_GRR_LAWI.Close(0);
                    Excel_APP_GRR_LAWI.Quit();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(Excel_APP_GRR_LAWI);
                    Excel_WS1_GRR_LAWI = null;
                    Excel_WB1_GRR_LAWI = null;
                    Excel_APP_GRR_LAWI = null;
                    bTestEnd = true;
                    this.Invoke(new UpdateLableHandler(printResult), new object[] { text });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
            }
            else
            {
                MessageBox.Show("Please choose .xls or .xlsx file only.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error); //custom messageBox to show error  
            }
        }


        private void Minitab_GRR_LAWI(int X_axis, int Y_axis)
        {
            try
            {
                object misValue = System.Reflection.Missing.Value;
                //Excel_WS1_GRR = ((Excel.Worksheet)Excel_WB1_GRR.Worksheets[1]);
                Excel_WS1_GRR = Excel_WB1_GRR.Worksheets[1];
                //Excel_WS1_GRR_LAWI = Excel_WB1_GRR_LAWI.Worksheets[1];
                Excel_APP_GRR.Visible = false;
                bool bGRR_data_valid = true;
                //Excel.Range Range_GRR = Excel_WS1_GRR.Range["A1"];
                Excel_WS1_GRR.Cells[2, 5] = Excel_WS1_GRR_LAWI.Cells[2, Y_axis];//USL
                Excel_WS1_GRR.Cells[2, 6] = Excel_WS1_GRR_LAWI.Cells[3, Y_axis];//LSL
                int iValue_count = 0;
                for (int i = 1; i <= 90; i++)
                {
                    //if (Excel_WS1_GRR_LAWI.Cells[i + 4, Y_axis] != "" && Excel_WS1_GRR_LAWI.Cells[i + 4, Y_axis] != null)
                    //{
                    Excel_WS1_GRR.Cells[i + 1, 3] = Excel_WS1_GRR_LAWI.Cells[i + 4, Y_axis];
                    //}
                    //else
                    //{
                    //    i--;
                    //}
                    //iValue_count++;
                }
                Excel_WS1_GRR = ((Excel.Worksheet)Excel_WB1_GRR.Worksheets[6]);
                //string sdfsdf = Excel_WS1_GRR.Range["E2"].Value.ToString();
                if (bGRR_data_valid)
                {
                    sGRR_Tolerance = Excel_WS1_GRR.Cells[2, 5].Value.ToString();
                }
                else
                {
                    sGRR_Tolerance = "N/A(資料不足90筆)";
                }
                //Excel_WB1_GRR.Close(false, misValue, misValue);
            }
            catch
            {
                //MessageBox.Show(ex.Message);
                //throw new Exception(ex.Message);                
                sGRR_Tolerance = "N/A(資料不足90筆)";
            }
        }

    }
}
