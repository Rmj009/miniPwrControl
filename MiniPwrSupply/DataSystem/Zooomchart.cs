using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace MiniPwrSupply.ControlSystem
{
    public partial class ZoomChart : Form
    {
        DataTable dtAll;
        DataTable dtPass;
        DataTable dtNG;
        DataTable dtWarning;
        public ZoomChart(string Title, DataTable dtAll, DataTable dtPass, DataTable dtNG, DataTable dtWarning)
        {
            InitializeComponent();
            this.label4.Text = Title;
            this.dtAll = new DataTable();
            this.dtPass = new DataTable();
            this.dtNG = new DataTable();
            this.dtWarning = new DataTable();
            this.dtAll = dtAll.Copy();
            this.dtPass = dtPass.Copy();
            this.dtNG = dtNG.Copy();
            this.dtWarning = dtWarning.Copy();
            List<string> xValuesChart = new List<string>();
            List<int> yValuesChart = new List<int>();
            this.StartPosition = FormStartPosition.CenterScreen;
            this.chart1.Series[0]["PieLabelStyle"] = "Outside";
            this.chart1.Series[0]["PieLineColor"] = "Black";
            if (Title == "測試結果比例：")
            {
                //xValuesChart.Clear();
                //yValuesChart.Clear();
                //xValuesChart.Add("Pass");
                //xValuesChart.Add("NG");
                //xValuesChart.Add("Warning");
                //yValuesChart.Add(dtPass.Rows.Count);
                //yValuesChart.Add(dtNG.Rows.Count);
                //yValuesChart.Add(dtWarning.Rows.Count);
                //this.chart1.Series[0].Points.DataBindXY(xValuesChart, yValuesChart);
                //this.chart1.Series[0].Points[0].Color = Color.FromArgb(0, 200, 50);
                //this.chart1.Series[0].Points[1].Color = Color.FromArgb(220, 0, 0);
                //this.chart1.Series[0].Points[2].Color = Color.FromArgb(250, 230, 50);
                xValuesChart.Clear();
                yValuesChart.Clear();
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
                    xValuesChart.Add(de.Key.ToString());
                    yValuesChart.Add(Convert.ToInt32(de.Value));
                }
                this.chart1.Series[0].Points.DataBindXY(xValuesChart, yValuesChart);
                #endregion
            }
            else if (Title == "生產力比例(Pass進系統非Golden)：")
            {
                xValuesChart.Clear();
                yValuesChart.Clear();
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
                        xValuesChart.Add(de.Key.ToString());
                        yValuesChart.Add(Convert.ToInt32(de.Value));
                    }
                    this.chart1.Series[0].Points.DataBindXY(xValuesChart, yValuesChart);
                    #endregion
                }
                else
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
                        xValuesChart.Add(de.Key.ToString());
                        yValuesChart.Add(Convert.ToInt32(de.Value));
                    }
                    this.chart1.Series[0].Points.DataBindXY(xValuesChart, yValuesChart);
                    #endregion
                }
            }
            else if (Title == "不良分佈在機台/治具比例：")
            {
                xValuesChart.Clear();
                yValuesChart.Clear();
                if (dtAll.Columns.Count > 14)//選了站別
                {
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
                        xValuesChart.Add(de.Key.ToString());
                        yValuesChart.Add(Convert.ToInt32(de.Value));
                    }
                    this.chart1.Series[0].Points.DataBindXY(xValuesChart, yValuesChart);
                    #endregion
                }
                else
                {
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
                        xValuesChart.Add(de.Key.ToString());
                        yValuesChart.Add(Convert.ToInt32(de.Value));
                    }
                    this.chart1.Series[0].Points.DataBindXY(xValuesChart, yValuesChart);
                    #endregion
                }
            }
            else if (Title == "不良Item比例：")
            {
                xValuesChart.Clear();
                yValuesChart.Clear();
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
                    xValuesChart.Add(de.Key.ToString());
                    yValuesChart.Add(Convert.ToInt32(de.Value));
                }
                this.chart1.Series[0].Points.DataBindXY(xValuesChart, yValuesChart);
                #endregion
            }
        }
    }
}
