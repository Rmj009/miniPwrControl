using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;

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
            InitializeComponent();
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

    }
}
