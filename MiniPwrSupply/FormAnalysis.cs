using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


namespace MiniPwrSupply
{

    public partial class FormAnalysis : Form
    {
        private System.Threading.Thread _test;
        string[] FileName = null;
        bool bTestEnd = false;
        string sLogPath = null;
        string sResultPath = null;
        string sLogNamePath = null;
        string sResultFormat = null;
        string sAutoDraw = null;
        string sEndKey = null;
        string sGRR = null;
        string sAutoGetTestItem = null;
        string sRawDataFormat = null;
        string[,] value = null;
        string[] sATS_test_time = null;
        string[] sTestItem = null;
        string sTemp = "";
        string[] _sTime = new string[1000];
        string _sTimeTemp = "";
        string[] _sKey = new string[1000];
        string[] _sKey2 = new string[1000];
        string[] _sKeyTemp = new string[1000];
        string[] _sSPEC_USL = null;
        string[] _sSPEC_LSL = null;
        int iTest_times = 0;
        int iTestTimes = 0;
        string sGRR_Tolerance = "";
        string sGRR_Tolerance_MA2 = string.Empty;
        string sGRR_Tolerance_MA3 = string.Empty;
        string sGRR_Tolerance_MA4 = string.Empty;
        string sGRR_Tolerance_MA5 = string.Empty;
        //int iEndKeyCount = 0;
        string text = "";
        string Illegal_character = "[\"//:><|\\?*]{1}";//定義非法字元，存檔時移除用
        double tmpStDev = 0F;
        string[,] sDatabase = null;
        string sFunction_type = "";
        int iTestCount = 0;
        string sStation = "";

        //Process bar        
        delegate void UpdateLableHandler(string text);
        Graphics g = null;
        //Excel
        static Excel.Application Excel_APP = null;
        Excel.Workbook Excel_WB1 = null;
        Excel.Worksheet Excel_WS1 = null;
        Excel.Worksheet Excel_WS2 = null;
        static Excel.Application Excel_APP_GRR = null;
        Excel.Workbook Excel_WB1_GRR = null;
        Excel.Worksheet Excel_WS1_GRR = null;
        static Excel.Application Excel_APP_GRR_LAWI = null;
        Excel.Workbook Excel_WB1_GRR_LAWI = null;
        Excel.Worksheet Excel_WS1_GRR_LAWI = null;
        private NewNetPort _netport = new NewNetPort();
        public FormAnalysis()
        {
            InitializeComponent();
            AssemblyTitleAttribute AsbTitle = (AssemblyTitleAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyTitleAttribute));
            Text = AsbTitle.Title + " - " + File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location).ToString("yyyy/MM/dd HH:mm:ss");
        }
        private void frmMain_Load(object sender, EventArgs e)
        {
            try
            {
                //FileName = Directory.GetFiles(@"\\172.16.41.130\nh-project\Tool\Parser tool");
                //foreach (string FN in FileName)
                //{
                //    if (FN.Contains("Version") && !FN.Contains(File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location).ToString("yyyyMMdd-HHmmss")))
                //    {                        
                //        //MessageBox.Show("目前有新版本 : " + FN.Replace(@"\\172.16.41.130\nh-project\Tool\Parser tool\Version_", "").Replace(".txt","") + "，請至FTP上下載最新版本" + @"\\172.16.41.130\nh-project\Tool\Parser tool");
                //    }
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Excel_WB1.Close();
            //Excel_APP.Quit();
            //Excel_APP = null;
            //if (sGRR == "Enable")
            //{
            //    Excel_WB1_GRR.Close(0);
            //    Excel_APP_GRR.Quit();
            //    System.Runtime.InteropServices.Marshal.ReleaseComObject(Excel_APP_GRR);
            //    Excel_WS1_GRR = null;
            //    Excel_WB1_GRR = null;
            //    Excel_APP_GRR = null;
            //}
            Application.Exit();
            Environment.Exit(0);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                progressBar1.Value = 0;
                bTestEnd = false;
                button1.Text = "Parsing...";
                switch (sFunction_type)
                {
                    case "Parsing log":
                        #region  讀取選取資料
                        sLogNamePath = comboBox1.SelectedItem.ToString();
                        sResultFormat = comboBox2.SelectedItem.ToString();
                        sAutoDraw = comboBox3.SelectedItem.ToString();
                        sEndKey = comboBox4.SelectedItem.ToString();
                        sGRR = comboBox5.SelectedItem.ToString();
                        sAutoGetTestItem = comboBox6.SelectedItem.ToString();
                        sRawDataFormat = comboBox7.SelectedItem.ToString();
                        #endregion
                        #region  回填此次選取值
                        WNC.API.Func.WriteINI("setting", "setting", "Result format default index", Convert.ToString(comboBox2.SelectedIndex));
                        WNC.API.Func.WriteINI("setting", "setting", "Auto sketch default index", Convert.ToString(comboBox3.SelectedIndex));
                        WNC.API.Func.WriteINI("setting", "setting", "End_key default index", Convert.ToString(comboBox4.SelectedIndex));
                        WNC.API.Func.WriteINI("setting", "setting", "GRR default index", Convert.ToString(comboBox5.SelectedIndex));
                        WNC.API.Func.WriteINI("setting", "setting", "Auto get test item default index", Convert.ToString(comboBox6.SelectedIndex));
                        WNC.API.Func.WriteINI("setting", "setting", "Raw data format default index", Convert.ToString(comboBox7.SelectedIndex));
                        #endregion
                        _test = new Thread(new ThreadStart(ParserData));
                        _test = new Thread(new ThreadStart(ReadExcelFileToArray));
                        break;
                    case "Cycle time":
                        #region  讀取選取資料
                        sLogNamePath = comboBox1.SelectedItem.ToString();
                        #endregion
                        _test = new Thread(new ThreadStart(CycleTimeAnalysis));
                        break;
                    case "Test time":
                        #region  讀取選取資料
                        sLogNamePath = comboBox1.SelectedItem.ToString();
                        sResultFormat = comboBox2.SelectedItem.ToString();
                        sAutoDraw = comboBox3.SelectedItem.ToString();
                        sEndKey = comboBox4.SelectedItem.ToString();
                        sGRR = comboBox5.SelectedItem.ToString();
                        #endregion
                        #region  回填此次選取值
                        WNC.API.Func.WriteINI("setting", "setting", "Result format default index", Convert.ToString(comboBox2.SelectedIndex));
                        WNC.API.Func.WriteINI("setting", "setting", "Auto sketch default index", Convert.ToString(comboBox3.SelectedIndex));
                        WNC.API.Func.WriteINI("setting", "setting", "End_key default index", Convert.ToString(comboBox4.SelectedIndex));
                        WNC.API.Func.WriteINI("setting", "setting", "GRR default index", Convert.ToString(comboBox5.SelectedIndex));
                        #endregion
                        _test = new Thread(new ThreadStart(TestTimeAnalysis));
                        break;
                    default:
                        break;
                }
                _test.Start();
                button1.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                sFunction_type = "Parsing log";
                comboBox1.Visible = true;
                label1.Visible = true;
                comboBox2.Visible = true;
                label2.Visible = true;
                comboBox3.Visible = true;
                label3.Visible = true;
                comboBox4.Visible = true;
                label4.Visible = true;
                comboBox5.Visible = true;
                label5.Visible = true;
                comboBox6.Visible = true;
                label7.Visible = true;
                comboBox7.Visible = true;
                label8.Visible = true;
                button1.Visible = true;
                comboBox1.Items.Clear();
                DirectoryInfo DirInfo = new DirectoryInfo(WNC.API.Func.ReadINI("setting", "setting", "Parsing log LogPath", ""));
                object[] oObject1 = DirInfo.GetFileSystemInfos().OrderByDescending(f => f.LastWriteTime).ToArray();
                comboBox1.Items.AddRange(oObject1);
                comboBox1.SelectedIndex = 0;

                ArrayList aArr = new ArrayList();
                int iCount = 1;
                while (WNC.API.Func.ReadINI("setting", "setting", "End_key" + iCount, "") != "")
                {
                    aArr.Add(WNC.API.Func.ReadINI("setting", "setting", "End_key" + iCount, ""));
                    iCount++;
                }
                object[] oObject = aArr.ToArray();
                comboBox4.Items.AddRange(oObject);
                #region 設定預設值(前次使用選取值)
                comboBox2.SelectedIndex = Convert.ToInt32(WNC.API.Func.ReadINI("setting", "setting", "Result format default index", ""));
                comboBox3.SelectedIndex = Convert.ToInt32(WNC.API.Func.ReadINI("setting", "setting", "Auto sketch default index", ""));
                comboBox4.SelectedIndex = Convert.ToInt32(WNC.API.Func.ReadINI("setting", "setting", "End_key default index", ""));
                comboBox5.SelectedIndex = Convert.ToInt32(WNC.API.Func.ReadINI("setting", "setting", "GRR default index", ""));
                comboBox6.SelectedIndex = Convert.ToInt32(WNC.API.Func.ReadINI("setting", "setting", "Auto get test item default index", ""));
                comboBox7.SelectedIndex = Convert.ToInt32(WNC.API.Func.ReadINI("setting", "setting", "Raw data format default index", ""));
                #endregion

                sLogPath = WNC.API.Func.ReadINI("setting", "setting", "Parsing log LogPath", "");
                sResultPath = WNC.API.Func.ReadINI("setting", "setting", "Parsing log ResultPath", "");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                sFunction_type = "Cycle time";
                comboBox1.Visible = true;
                label1.Visible = true;
                comboBox2.Visible = false;
                label2.Visible = false;
                comboBox3.Visible = false;
                label3.Visible = false;
                comboBox4.Visible = false;
                label4.Visible = false;
                comboBox5.Visible = false;
                label5.Visible = false;
                comboBox6.Visible = false;
                label7.Visible = false;
                comboBox7.Visible = false;
                label8.Visible = false;
                button1.Visible = true;
                comboBox1.Items.Clear();
                DirectoryInfo DirInfo = new DirectoryInfo(WNC.API.Func.ReadINI("setting", "setting", "Break cycle time LogPath", ""));
                object[] oObject1 = DirInfo.GetFileSystemInfos().OrderByDescending(f => f.LastWriteTime).ToArray();
                comboBox1.Items.AddRange(oObject1);
                comboBox1.SelectedIndex = 0;

                sLogPath = WNC.API.Func.ReadINI("setting", "setting", "Break cycle time LogPath", "");
                sResultPath = WNC.API.Func.ReadINI("setting", "setting", "Break cycle time ResultPath", "");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                sFunction_type = "Test time";
                comboBox1.Visible = true;
                label1.Visible = true;
                comboBox2.Visible = true;
                label2.Visible = true;
                comboBox3.Visible = true;
                label3.Visible = true;
                comboBox4.Visible = true;
                label4.Visible = true;
                comboBox5.Visible = true;
                label5.Visible = true;
                button1.Visible = true;
                comboBox6.Visible = false;
                label7.Visible = false;
                comboBox6.Visible = false;
                label7.Visible = false;
                comboBox1.Items.Clear();
                DirectoryInfo DirInfo = new DirectoryInfo(WNC.API.Func.ReadINI("setting", "setting", "Break test time LogPath", ""));
                object[] oObject1 = DirInfo.GetFileSystemInfos().OrderByDescending(f => f.LastWriteTime).ToArray();
                comboBox1.Items.AddRange(oObject1);
                comboBox1.SelectedIndex = 0;

                ArrayList End_Key_aArr = new ArrayList();
                int iCount = 1;
                while (WNC.API.Func.ReadINI("setting", "setting", "End_key" + iCount, "") != "")
                {
                    End_Key_aArr.Add(WNC.API.Func.ReadINI("setting", "setting", "End_key" + iCount, ""));
                    iCount++;
                }
                object[] oObject = End_Key_aArr.ToArray();
                comboBox4.Items.AddRange(oObject);
                #region 設定預設值(前次使用選取值)
                comboBox2.SelectedIndex = Convert.ToInt32(WNC.API.Func.ReadINI("setting", "setting", "Result format default index", ""));
                comboBox3.SelectedIndex = Convert.ToInt32(WNC.API.Func.ReadINI("setting", "setting", "Auto sketch default index", ""));
                comboBox4.SelectedIndex = Convert.ToInt32(WNC.API.Func.ReadINI("setting", "setting", "End_key default index", ""));
                comboBox5.SelectedIndex = Convert.ToInt32(WNC.API.Func.ReadINI("setting", "setting", "GRR default index", ""));
                #endregion

                sLogPath = WNC.API.Func.ReadINI("setting", "setting", "Break test time LogPath", "");
                sResultPath = WNC.API.Func.ReadINI("setting", "setting", "Break test time ResultPath", "");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void printResult(string text)
        {
            if (!bTestEnd)
            {
                progressBar1.PerformStep();
            }
            else
            {
                button1.Text = "START";
                button1.Enabled = true;
            }
        }
        private void ProgressBarSize(string text)
        {
            progressBar1.Minimum = 0;
            progressBar1.Maximum = FileName.Length;
            progressBar1.Step = 1;
        }
        public static bool IsNumeric(string TextBoxValue)
        {
            try
            {
                double d = Convert.ToDouble(TextBoxValue);
                bool aaa = Double.TryParse(TextBoxValue, out d);
                return true;
            }
            catch
            {
                return false;
            }
        }
        private void WriteToCSV()
        {
            string line = "";
            FileStream mystream = new FileStream(sResultPath + "//" + sLogNamePath + "//" + sLogNamePath + ".csv", FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            StreamWriter mywrite = new StreamWriter(mystream);
            line = "Test item,";
            for (int j = 0; j < sTestItem.Length; j++)
            {
                line += _sKey[j] + ",";
                if (j == sTestItem.Length - 1)
                {
                    line += "File name" + ",";
                }
            }
            mywrite.WriteLine(line);
            line = "LSL,";
            for (int j = 0; j < sTestItem.Length; j++)
            {
                line += _sSPEC_LSL[j] + ",";
            }
            mywrite.WriteLine(line);
            line = "USL,";
            for (int j = 0; j < sTestItem.Length; j++)
            {
                line += _sSPEC_USL[j] + ",";
            }
            mywrite.WriteLine(line);
            for (int r = 0; r < iTest_times; r++)
            {
                line = r + 1 + ",";
                for (int j = 0; j < _sKey.Length + 1; j++)
                {
                    line += value[r, j] + ",";
                }
                mywrite.WriteLine(line);
            }
            mywrite.Close();
            mystream.Close();
        }
        private void WriteToExcel_ParserData_vertical_old()
        {
            Excel.Application Excel_APP = new Excel.Application();
            Excel.Workbook Excel_WB1 = Excel_APP.Workbooks.Add(true);
            Excel.Worksheet Excel_WS1 = new Excel.Worksheet();
            Excel_WS1 = (Excel.Worksheet)Excel_WB1.Worksheets[1];
            Excel_APP.Visible = true;
            Excel.Range Range = Excel_WS1.Range["A1"];
            Excel_WS1.Name = "Raw data";
            Excel_WS1.Cells[1, 1] = "Test item";
            Excel_WS1.Cells[1, 2] = "LSL";
            Excel_WS1.Cells[1, 3] = "USL";
            Excel_WS1.Cells[sTestItem.Length + 2, 1] = "File name";
            //Excel_WS1.Cells[sTestItem.Length + 3, 1] = "Top1";
            //Excel_WS1.Cells[sTestItem.Length + 4, 1] = "Top2";
            //Excel_WS1.Cells[sTestItem.Length + 5, 1] = "Top3";
            string sRepairPosition_Temp1 = "";
            string[] sRepairPosition_Temp2 = new string[1000];
            string[,] sRepairPosition_Temp3 = null;
            string[,] sRepairPosition = null;
            //ArrayList[][] arrRepairPosition = new ArrayList[2][];
            int iArrayTemp = 0;
            for (int Y_axis = 1; Y_axis <= iTest_times + 3; Y_axis++)//iTest_times + 3為總測試次數加Test item、LSL、USL
            {
                sRepairPosition_Temp1 = "";
                for (int X_axis = 1; X_axis <= _sKey.Length + 3; X_axis++)//_sKey.Length + 2為總測項加Test Item 、 File name、Top1
                {

                    if ((X_axis == 1 || X_axis == _sKey.Length + 2) && (Y_axis == 1 || Y_axis == 2 || Y_axis == 3))
                    {
                        continue;
                    }
                    else if (X_axis == 1)//標記測試數
                    {
                        Excel_WS1.Cells[X_axis, Y_axis] = Y_axis - 3;
                    }
                    else if ((Y_axis == 1) & (X_axis < _sKey.Length + 2))//寫入測項
                    {
                        Excel_WS1.Cells[X_axis, Y_axis] = _sKey[X_axis - 2];
                    }
                    else if ((Y_axis == 2) & (X_axis < _sKey.Length + 2))//寫入LSL
                    {
                        Excel_WS1.Cells[X_axis, Y_axis] = _sSPEC_LSL[X_axis - 2];
                    }
                    else if ((Y_axis == 3) & (X_axis < _sKey.Length + 2))//寫入USL
                    {
                        Excel_WS1.Cells[X_axis, Y_axis] = _sSPEC_USL[X_axis - 2];
                    }
                    else if ((Y_axis >= 4) & (X_axis == _sKey.Length + 3) & (sRepairPosition_Temp1 != ""))//寫入Top1~TopX
                    {
                        sRepairPosition_Temp2 = sRepairPosition_Temp1.Substring(0, sRepairPosition_Temp1.Length - 1).Split('、');
                        sRepairPosition_Temp3 = new string[2, sRepairPosition_Temp2.Length];
                        sRepairPosition = new string[2, sRepairPosition_Temp2.Length];
                        for (int i = 0; i < sRepairPosition_Temp2.Length; i++)//記錄全部維修點
                        {
                            if (sRepairPosition_Temp2[i] == null || sRepairPosition_Temp2[i] == "")
                            {
                                break;
                            }
                            for (int iItemCount = 0; iItemCount < sRepairPosition_Temp3.Length; iItemCount++)
                            {
                                if (sRepairPosition_Temp3[0, iItemCount] == sRepairPosition_Temp2[i])
                                {
                                    sRepairPosition_Temp3[1, iItemCount] = Convert.ToString(Convert.ToDouble(sRepairPosition_Temp3[1, iItemCount]) + 1);
                                    break;
                                }
                                else if (((sRepairPosition_Temp3[0, iItemCount] == "") || (sRepairPosition_Temp3[0, iItemCount] == null)))
                                {
                                    sRepairPosition_Temp3[0, iItemCount] = sRepairPosition_Temp2[i];
                                    sRepairPosition_Temp3[1, iItemCount] = "1";
                                    //arrRepairPosition[0][iItemCount].Add(sRepairPosition_Temp2[i]);
                                    //arrRepairPosition[1][iItemCount].Add("1");
                                    break;
                                }
                            }
                        }
                        for (int x = 0; x < sRepairPosition_Temp3.Length / 2; x++)//排序
                        {
                            iArrayTemp = 0;
                            for (int i = 0; i < sRepairPosition_Temp2.Length; i++)
                            {
                                if (Convert.ToDouble(sRepairPosition_Temp3[1, iArrayTemp]) < Convert.ToDouble(sRepairPosition_Temp3[1, i]))
                                {
                                    iArrayTemp = i;
                                }
                            }
                            sRepairPosition[0, x] = sRepairPosition_Temp3[0, iArrayTemp];
                            sRepairPosition[1, x] = sRepairPosition_Temp3[1, iArrayTemp];
                            sRepairPosition_Temp3[0, iArrayTemp] = null;
                            sRepairPosition_Temp3[1, iArrayTemp] = null;
                            if (sRepairPosition[0, x] != null)//印出維修點優先順序
                            {
                                Excel_WS1.Cells[sTestItem.Length + 3 + x, 1] = "Repair position Top" + (1 + x);
                                Excel_WS1.Cells[sTestItem.Length + 3 + x, Y_axis] = sRepairPosition[0, x] + " " + "[" + (Convert.ToDouble(sRepairPosition[1, x]) / sRepairPosition_Temp2.Length).ToString("P") + "(" + sRepairPosition[1, x] + "/" + sRepairPosition_Temp2.Length + ")]";
                            }
                        }
                    }
                    else if (X_axis <= _sKey.Length + 2)
                    {
                        Excel_WS1.Cells[X_axis, Y_axis] = value[Y_axis - 4, X_axis - 2];
                        if (IsNumeric(value[Y_axis - 4, X_axis - 2]) && value[Y_axis - 4, X_axis - 2] != null)
                        {
                            if (Convert.ToDouble(value[Y_axis - 4, X_axis - 2]) > Convert.ToDouble(_sSPEC_USL[X_axis - 2]) || Convert.ToDouble(value[Y_axis - 4, X_axis - 2]) < Convert.ToDouble(_sSPEC_LSL[X_axis - 2]))
                            {
                                Range = (Excel.Range)Excel_WS1.Cells[X_axis, Y_axis];
                                for (int i = 1; i < sDatabase.GetLength(1); i++)
                                {
                                    if (_sKey[X_axis - 2] == sDatabase[0, i])
                                    {
                                        if (sDatabase[2, i] == "" || sDatabase[2, i] == null)
                                        {
                                            MessageBox.Show(_sKey[X_axis - 2] + "未設定維修點請確認");
                                            return;
                                        }
                                        sRepairPosition_Temp1 = sRepairPosition_Temp1 + sDatabase[2, i] + "、";
                                        break;
                                    }
                                    else if (i == sDatabase.GetLength(1) - 1)
                                    {
                                        MessageBox.Show(_sKey[X_axis - 2] + "未比對到NG測項請確認");
                                        return;
                                    }
                                }
                                //Range = Excel_WS1.get_Range((Excel.Range)Excel_WS1.Cells[Y_axis, X_axis], (Excel.Range)Excel_WS1.Cells[Y_axis, X_axis]);
                                Range.Interior.ColorIndex = 3;//格子顯示紅色
                            }
                        }
                        else if (value[Y_axis - 4, X_axis - 2] == "NG")
                        {
                            if (value[Y_axis - 4, X_axis - 2] != _sSPEC_USL[X_axis - 2])
                            {
                                Range = (Excel.Range)Excel_WS1.Cells[X_axis, Y_axis];
                                for (int i = 1; i < sDatabase.GetLength(1); i++)
                                {
                                    if (_sKey[X_axis - 2] == sDatabase[0, i])
                                    {
                                        if (sDatabase[2, i] == "" || sDatabase[2, i] == null)
                                        {
                                            MessageBox.Show(_sKey[X_axis - 2] + "未設定維修點請確認");
                                            return;
                                        }
                                        sRepairPosition_Temp1 = sRepairPosition_Temp1 + sDatabase[2, i] + "、";
                                        break;
                                    }
                                    else if (i == sDatabase.GetLength(1) - 1)
                                    {
                                        MessageBox.Show(_sKey[X_axis - 2] + "未比對到NG測項請確認");
                                        return;
                                    }
                                }
                                //Range = Excel_WS1.get_Range((Excel.Range)Excel_WS1.Cells[Y_axis, X_axis], (Excel.Range)Excel_WS1.Cells[Y_axis, X_axis]);
                                Range.Interior.ColorIndex = 3;//格子顯示紅色
                            }
                        }
                    }
                }
            }
            //設定顯示時間之格子的格式並置中
            //Range = Excel_WS1.Range["A:H"];
            //Range.EntireColumn.AutoFit();//自動調整格子寬度
            //Range.HorizontalAlignment = Excel.XlVAlign.xlVAlignCenter;//置中
            //Range.EntireColumn.NumberFormat = "HH:MM:SS.000";//調整時間顯示格式

            ////調整全部格子自動寬度、標題及總結格子的顏色
            //Range = Excel_WS1.Range["A1:F1"];
            //Range.EntireColumn.AutoFit();//自動調整格子寬度
            //Range.HorizontalAlignment = Excel.XlVAlign.xlVAlignCenter;//置中
            //Range.Interior.ColorIndex = 10;//格子顯示綠色
            //Range = Excel_WS1.Range["H1:J1"];
            //Range.EntireColumn.AutoFit();//自動調整格子寬度
            //Range.HorizontalAlignment = Excel.XlVAlign.xlVAlignCenter;//置中
            //Range.Interior.ColorIndex = 10;//格子顯示綠色
            //Range = Excel_WS1.Range["A" + (r + 1) + ":F" + (r + 1)];
            //Range.Interior.ColorIndex = 6;//格子顯示黃色
            //Range = Excel_WS1.Range["H" + (r + 1) + ":J" + (r + 1)];
            //Range.Interior.ColorIndex = 6;//格子顯示黃色
            //Range = Excel_WS1.Range["H" + (r + 1) + ":J" + (r + 1)];
            //Range.Interior.ColorIndex = 3;//格子顯示紅色

            //設定全部框線
            //Range = Excel_WS1.Range["A1:F" + (r + 1)];
            //Range.Borders.Weight = Excel.XlBorderWeight.xlThin;//增加框線
            //excel_sketch();
            string ExcelFile = sResultPath + "\\" + sLogNamePath + "\\" + sLogNamePath;
            if (File.Exists(ExcelFile + ".xlsx"))
                File.Delete(ExcelFile + ".xlsx");
            Excel_WB1.SaveAs(ExcelFile, Excel.XlFileFormat.xlOpenXMLWorkbook, System.Reflection.Missing.Value, System.Reflection.Missing.Value, false, false,
    Excel.XlSaveAsAccessMode.xlExclusive, false, false, System.Reflection.Missing.Value, System.Reflection.Missing.Value, System.Reflection.Missing.Value);
            Excel_WB1.Close();
            Excel_APP.Quit();
            Excel_APP = null;
        }
        private void WriteToExcel_ParserData_vertical()
        {
            try
            {
                Excel.Application Excel_APP = new Excel.Application();
                Excel.Workbook Excel_WB1 = Excel_APP.Workbooks.Add(true);
                Excel.Worksheet Excel_WS1 = new Excel.Worksheet();
                Excel_WS1 = ((Excel.Worksheet)Excel_WB1.Worksheets[1]);
                Excel_APP.Visible = true;
                Excel.Range Range = Excel_WS1.Range["A1"];
                int iTitleNameCount = 1;//Test time、LSL等...的數量
                                        //bool sGRR=="Enable" = true;
                Excel_WS1.Name = "Raw data";
                Excel_WS1.Cells[1, iTitleNameCount] = "Test item";
                iTitleNameCount++;
                Excel_WS1.Cells[1, iTitleNameCount] = "LSL";
                iTitleNameCount++;
                Excel_WS1.Cells[1, iTitleNameCount] = "USL";
                iTitleNameCount++;
                Excel_WS1.Cells[1, iTitleNameCount] = "AVG";
                iTitleNameCount++;
                Excel_WS1.Cells[1, iTitleNameCount] = "STDEV";
                iTitleNameCount++;
                Excel_WS1.Cells[1, iTitleNameCount] = "Cpk";
                if (sGRR == "Enable")
                {
                    iTitleNameCount++;
                    Excel_WS1.Cells[1, iTitleNameCount] = "GRR%";
                    Excel_APP_GRR = new Excel.Application();
                    Excel_WB1_GRR = Excel_APP_GRR.Workbooks.Open(Directory.GetCurrentDirectory() + "\\Two way anova & GRR_test.xlsx");
                }
                Excel_WS1.Cells[sTestItem.Length + 2, 1] = "File name";

                for (int X_axis = 1; X_axis <= _sKey.Length + 2; X_axis++)//_sKey.Length + 2為總測項加Test Item 、 File name
                {
                    for (int Y_axis = 1; Y_axis <= iTest_times + iTitleNameCount; Y_axis++)//iTest_times + 6為總測試次數加Test item、LSL、USL、AVG、STDEV、Cpk
                    {
                        if ((X_axis == 1 || X_axis == _sKey.Length + 2) && Y_axis <= iTitleNameCount) //頭尾欄前六列不需填資料
                        {
                            continue;
                        }
                        else if (X_axis == 1)//標記測試數
                        {
                            Excel_WS1.Cells[X_axis, Y_axis] = Y_axis - iTitleNameCount;
                        }
                        else if ((Y_axis == 1) & (X_axis < _sKey.Length + 2))//寫入測項
                        {
                            Excel_WS1.Cells[X_axis, Y_axis] = _sKey[X_axis - 2];
                        }
                        else if ((Y_axis == 2) & (X_axis < _sKey.Length + 2))//寫入LSL
                        {
                            Excel_WS1.Cells[X_axis, Y_axis] = _sSPEC_LSL[X_axis - 2];
                        }
                        else if ((Y_axis == 3) & (X_axis < _sKey.Length + 2))//寫入USL
                        {
                            Excel_WS1.Cells[X_axis, Y_axis] = _sSPEC_USL[X_axis - 2];
                        }
                        else if ((Y_axis == 4) & (X_axis < _sKey.Length + 2))//寫入AVG
                        {
                            if (!_sKey[X_axis - 2].ToUpper().Contains("ICCID") && !_sKey[X_axis - 2].ToUpper().Contains("IMSI") && !_sKey[X_axis - 2].ToUpper().Contains("IMEI"))
                            {
                                if (sGRR == "Enable")
                                {
                                    Excel_WS1.Cells[X_axis, Y_axis] = "=IFERROR(AVERAGE(RC[4]:RC[" + (iTest_times + 3) + "]), \"N/A\")";//IFERROR(AVERAGE(R[3]C: R[13]C), "N/A")
                                    Range = (Excel.Range)Excel_WS1.Cells[X_axis, Y_axis];
                                    Range.NumberFormatLocal = "0.00";
                                }
                                else
                                {
                                    Excel_WS1.Cells[X_axis, Y_axis] = "=IFERROR(AVERAGE(RC[3]:RC[" + (iTest_times + 2) + "]), \"N/A\")";//IFERROR(AVERAGE(R[3]C: R[13]C), "N/A")
                                    Range = (Excel.Range)Excel_WS1.Cells[X_axis, Y_axis];
                                    Range.NumberFormatLocal = "0.00";
                                }
                            }
                            else
                            {
                                Excel_WS1.Cells[X_axis, Y_axis] = "N/A";
                            }
                        }
                        else if ((Y_axis == 5) & (X_axis < _sKey.Length + 2))//寫入STDEV
                        {

                            if (!_sKey[X_axis - 2].ToUpper().Contains("ICCID") && !_sKey[X_axis - 2].ToUpper().Contains("IMSI") && !_sKey[X_axis - 2].ToUpper().Contains("IMEI"))
                            {
                                if (sGRR == "Enable")
                                {
                                    //Excel_WS1.Cells[X_axis,Y_axis] = "=IFERROR(ROUND(STDEV(RC[3]:RC[" + (iTest_times + 2) + "]),2), \"N/A\")";//IFERROR(STDEV(R[2]C: R[12]C), "N/A")
                                    Excel_WS1.Cells[X_axis, Y_axis] = "=IFERROR(STDEV(RC[3]:RC[" + (iTest_times + 2) + "]), \"N/A\")";//IFERROR(STDEV(R[2]C: R[12]C), "N/A")
                                    Range = (Excel.Range)Excel_WS1.Cells[X_axis, Y_axis];
                                    Range.NumberFormatLocal = "0.00";
                                }
                                else
                                {
                                    //Excel_WS1.Cells[X_axis,Y_axis] = "=IFERROR(ROUND(STDEV(RC[2]:RC[" + (iTest_times + 1) + "]),2), \"N/A\")";//IFERROR(STDEV(R[2]C: R[12]C), "N/A")
                                    Excel_WS1.Cells[X_axis, Y_axis] = "=IFERROR(STDEV(RC[3]:RC[" + (iTest_times + 2) + "]), \"N/A\")";//IFERROR(STDEV(R[2]C: R[12]C), "N/A")
                                    Range = (Excel.Range)Excel_WS1.Cells[X_axis, Y_axis];
                                    Range.NumberFormatLocal = "0.00";
                                }
                            }
                            else
                            {
                                Excel_WS1.Cells[X_axis, Y_axis] = "N/A";
                            }
                        }
                        else if ((Y_axis == 6) & (X_axis < _sKey.Length + 2))//寫入Cpk
                        {
                            if (!_sKey[X_axis - 2].ToUpper().Contains("ICCID") && !_sKey[X_axis - 2].ToUpper().Contains("IMSI") && !_sKey[X_axis - 2].ToUpper().Contains("IMEI") && !_sKey[X_axis - 2].ToUpper().Contains("ATS_TIME"))
                            {
                                //Cpk,MIN((USL-AVG)/(3*標準差stdev),(AVG-LSL)/(3*標準差stdev))
                                Excel_WS1.Cells[X_axis, Y_axis] = "=IFERROR(MIN((RC[-3] - RC[-2]) / (3 * RC[-1]),(RC[-2] - RC[-4])/ (3 * RC[-1])),\"N/A\")";//IFERROR(MIN((R[-3]C - R[-2]C) / (3 * R[-1]C),(R[-2]C - R[-4]C)/ (3 * R[-1]C)),"N/A")
                                Range = (Excel.Range)Excel_WS1.Cells[X_axis, Y_axis];
                                Excel.FormatCondition condition1 = (Excel.FormatCondition)Range.FormatConditions.Add(Excel.XlFormatConditionType.xlCellValue, Excel.XlFormatConditionOperator.xlLess, 1.33);
                                condition1.Font.Bold = true;
                                condition1.Interior.ColorIndex = 3;//紅色
                                Range.NumberFormatLocal = "0.00";

                                string[] FileName_CPK = Directory.GetFiles(sResultPath + "\\" + sLogNamePath);
                                foreach (string FN in FileName_CPK)
                                {
                                    if (FN.Contains(_sKey[X_axis - 2].Replace(".", "") + "_CPK.PNG"))
                                    {
                                        Excel_WS1.Hyperlinks.Add(Excel_WS1.Cells[X_axis, Y_axis], FN.Replace(".", "").Replace("PNG", ".PNG"), "", "");
                                    }
                                }
                            }
                            else
                            {
                                Excel_WS1.Cells[X_axis, Y_axis] = "N/A";
                            }
                        }
                        else if ((Y_axis == 7) & (X_axis < _sKey.Length + 2) & sGRR == "Enable")//寫入GRR Torence
                        {
                            if (!_sKey[X_axis - 2].ToUpper().Contains("ICCID") && !_sKey[X_axis - 2].ToUpper().Contains("IMSI") && !_sKey[X_axis - 2].ToUpper().Contains("IMEI") && !_sKey[X_axis - 2].ToUpper().Contains("ATS_TIME") && IsNumeric(value[0, X_axis - 2]) && !_sKey[X_axis - 2].ToUpper().Contains("CHKCOUNT") && !_sKey[X_axis - 2].ToUpper().Contains("RETRYTIMES"))
                            {
                                //Cpk,MIN((USL-AVG)/(3*標準差stdev),(AVG-LSL)/(3*標準差stdev))
                                //Excel_WS1.Cells[X_axis,Y_axis] = "=IFERROR(MIN((R[-3]C - R[-2]C) / (3 * R[-1]C),(R[-2]C - R[-4]C)/ (3 * R[-1]C)),\"N/A\")";//IFERROR(MIN((R[-3]C - R[-2]C) / (3 * R[-1]C),(R[-2]C - R[-4]C)/ (3 * R[-1]C)),"N/A")
                                Minitab_GRR(X_axis, X_axis);
                                Excel_WS1.Cells[X_axis, Y_axis] = sGRR_Tolerance;
                                Range = (Excel.Range)Excel_WS1.Cells[X_axis, Y_axis];
                                Excel.FormatCondition condition1 = (Excel.FormatCondition)Range.FormatConditions.Add(Excel.XlFormatConditionType.xlCellValue, Excel.XlFormatConditionOperator.xlBetween, 10, 30);
                                condition1.Font.Bold = true;
                                condition1.Interior.ColorIndex = 6;//黃色
                                Excel.FormatCondition condition2 = (Excel.FormatCondition)Range.FormatConditions.Add(Excel.XlFormatConditionType.xlCellValue, Excel.XlFormatConditionOperator.xlGreater, 30);
                                condition2.Font.Bold = true;
                                condition2.Interior.ColorIndex = 3;//紅色
                            }
                            else
                            {
                                Excel_WS1.Cells[X_axis, Y_axis] = "N/A";
                            }
                        }
                        else
                        {
                            if (value[Y_axis - (iTitleNameCount + 1), X_axis - 2] == "" || value[Y_axis - (iTitleNameCount + 1), X_axis - 2] == null)//不等於空值就移除"="
                            {
                                Excel_WS1.Cells[X_axis, Y_axis] = value[Y_axis - (iTitleNameCount + 1), X_axis - 2];
                            }
                            else
                            {
                                Excel_WS1.Cells[X_axis, Y_axis] = value[Y_axis - (iTitleNameCount + 1), X_axis - 2].Replace("=", "");
                            }
                            if (IsNumeric(value[Y_axis - (iTitleNameCount + 1), X_axis - 2]) && value[Y_axis - (iTitleNameCount + 1), X_axis - 2] != null)
                            {
                                if (!_sKey[X_axis - 2].ToUpper().Contains("ICCID") && !_sKey[X_axis - 2].ToUpper().Contains("IMSI") && !_sKey[X_axis - 2].ToUpper().Contains("IMEI") && !_sKey[X_axis - 2].ToUpper().Contains("ATS_TIME"))
                                {
                                    if (Convert.ToDouble(value[Y_axis - (iTitleNameCount + 1), X_axis - 2]) > Convert.ToDouble(_sSPEC_USL[X_axis - 2]) || Convert.ToDouble(value[Y_axis - (iTitleNameCount + 1), X_axis - 2]) < Convert.ToDouble(_sSPEC_LSL[X_axis - 2]))
                                    {
                                        Range = (Excel.Range)Excel_WS1.Cells[X_axis, Y_axis];
                                        //Range = Excel_WS1.get_Range((Excel.Range)Excel_WS1.Cells[X_axis,Y_axis], (Excel.Range)Excel_WS1.Cells[X_axis,Y_axis]);
                                        Range.Interior.ColorIndex = 3;//格子顯示紅色
                                    }
                                }
                            }
                            else if (value[Y_axis - (iTitleNameCount + 1), X_axis - 2] == "NG")
                            {
                                if (value[Y_axis - (iTitleNameCount + 1), X_axis - 2] != _sSPEC_USL[X_axis - 2])
                                {
                                    Range = (Excel.Range)Excel_WS1.Cells[X_axis, Y_axis];
                                    //Range = Excel_WS1.get_Range((Excel.Range)Excel_WS1.Cells[X_axis,Y_axis], (Excel.Range)Excel_WS1.Cells[X_axis,Y_axis]);
                                    Range.Interior.ColorIndex = 3;//格子顯示紅色
                                }
                            }
                        }
                    }
                }

                //設定顯示時間之格子的格式並置中
                //Range = Excel_WS1.Range["A:H"];
                //Range.EntireColumn.AutoFit();//自動調整格子寬度
                //Range.HorizontalAlignment = Excel.XlVAlign.xlVAlignCenter;//置中
                //Range.EntireColumn.NumberFormat = "HH:MM:SS.000";//調整時間顯示格式

                ////調整全部格子自動寬度、標題及總結格子的顏色
                //Range = Excel_WS1.Range["A1:F1"];
                //Range.EntireColumn.AutoFit();//自動調整格子寬度
                //Range.HorizontalAlignment = Excel.XlVAlign.xlVAlignCenter;//置中
                //Range.Interior.ColorIndex = 10;//格子顯示綠色
                //Range = Excel_WS1.Range["H1:J1"];
                //Range.EntireColumn.AutoFit();//自動調整格子寬度
                //Range.HorizontalAlignment = Excel.XlVAlign.xlVAlignCenter;//置中
                //Range.Interior.ColorIndex = 10;//格子顯示綠色
                //Range = Excel_WS1.Range["A" + (r + 1) + ":F" + (r + 1)];
                //Range.Interior.ColorIndex = 6;//格子顯示黃色
                //Range = Excel_WS1.Range["H" + (r + 1) + ":J" + (r + 1)];
                //Range.Interior.ColorIndex = 6;//格子顯示黃色
                //Range = Excel_WS1.Range["H" + (r + 1) + ":J" + (r + 1)];
                //Range.Interior.ColorIndex = 3;//格子顯示紅色

                //設定全部框線
                //Range = Excel_WS1.Range["A1:F" + (r + 1)];
                //Range.Borders.Weight = Excel.XlBorderWeight.xlThin;//增加框線
                //excel_sketch();
                Excel_WS1.Cells.Font.Name = "Calibri";//設定Excel資料字體字型
                Excel_WS1.Cells.Font.Size = 8;//設定Excel資料字體大小
                Range = Excel_WS1.Range["D:F"];
                Range.HorizontalAlignment = Excel.XlVAlign.xlVAlignCenter;//置中
                Range.Font.ColorIndex = 8;//字體顯示藍色
                Excel_WS1.Cells.ColumnWidth = 5;//設定欄寬全部為5
                Range = Excel_WS1.Range["A:A"];//調整Item欄位自動大小
                Range.EntireColumn.AutoFit();
                Range.Font.ColorIndex = 8;//字體顯示藍色
                string ExcelFile = sResultPath + "\\" + sLogNamePath + "\\" + sLogNamePath;
                if (File.Exists(ExcelFile + ".xlsx"))
                    File.Delete(ExcelFile + ".xlsx");
                Excel_WB1.SaveAs(ExcelFile, Excel.XlFileFormat.xlOpenXMLWorkbook, System.Reflection.Missing.Value, System.Reflection.Missing.Value, false, false,
        Excel.XlSaveAsAccessMode.xlExclusive, false, false, System.Reflection.Missing.Value, System.Reflection.Missing.Value, System.Reflection.Missing.Value);
                Excel_WB1.Close();
                Excel_APP.Quit();
                Excel_APP = null;
                if (sGRR == "Enable")
                {
                    Excel_WB1_GRR.Close(0);
                    Excel_APP_GRR.Quit();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(Excel_APP_GRR);
                    Excel_WS1_GRR = null;
                    Excel_WB1_GRR = null;
                    Excel_APP_GRR = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void WriteToExcel_ParserData_horizontal()
        {
            try
            {
                Excel.Application Excel_APP = new Excel.Application();
                Excel.Workbook Excel_WB1 = Excel_APP.Workbooks.Add(true);
                Excel.Worksheet Excel_WS1 = new Excel.Worksheet();
                Excel_WS1 = ((Excel.Worksheet)Excel_WB1.Worksheets[1]);
                Excel_APP.Visible = true;
                Excel.Range Range = Excel_WS1.Range["A1"];
                int iTitleNameCount = 1;//Test time、LSL等...的數量
                                        //bool sGRR=="Enable" = true;
                Excel_WS1.Name = "Raw data";
                Excel_WS1.Cells[iTitleNameCount, 1] = "Test item";
                iTitleNameCount++;
                Excel_WS1.Cells[iTitleNameCount, 1] = "LSL";
                iTitleNameCount++;
                Excel_WS1.Cells[iTitleNameCount, 1] = "USL";
                iTitleNameCount++;
                Excel_WS1.Cells[iTitleNameCount, 1] = "AVG";
                iTitleNameCount++;
                Excel_WS1.Cells[iTitleNameCount, 1] = "STDEV";
                iTitleNameCount++;
                Excel_WS1.Cells[iTitleNameCount, 1] = "Cpk";
                if (sGRR == "Enable")
                {
                    iTitleNameCount++;
                    Excel_WS1.Cells[iTitleNameCount, 1] = "GRR Tolerance";
                    Excel_APP_GRR = new Excel.Application();
                    Excel_WB1_GRR = Excel_APP_GRR.Workbooks.Open(Directory.GetCurrentDirectory() + "\\Two way anova & GRR_test.xlsx");
                }
                Excel_WS1.Cells[1, sTestItem.Length + 2] = "File name";

                for (int X_axis = 1; X_axis <= _sKey.Length + 2; X_axis++)//_sKey.Length + 2為總測項加Test Item 、 File name
                {
                    for (int Y_axis = 1; Y_axis <= iTest_times + iTitleNameCount; Y_axis++)//iTest_times + 6為總測試次數加Test item、LSL、USL、AVG、STDEV、Cpk
                    {
                        if ((X_axis == 1 || X_axis == _sKey.Length + 2) && Y_axis <= iTitleNameCount) //頭尾欄前六列不需填資料
                        {
                            continue;
                        }
                        else if (X_axis == 1)//標記測試數
                        {
                            Excel_WS1.Cells[Y_axis, X_axis] = Y_axis - iTitleNameCount;
                        }
                        else if ((Y_axis == 1) & (X_axis < _sKey.Length + 2))//寫入測項
                        {
                            Excel_WS1.Cells[Y_axis, X_axis] = _sKey[X_axis - 2];
                        }
                        else if ((Y_axis == 2) & (X_axis < _sKey.Length + 2))//寫入LSL
                        {
                            Excel_WS1.Cells[Y_axis, X_axis] = _sSPEC_LSL[X_axis - 2];
                        }
                        else if ((Y_axis == 3) & (X_axis < _sKey.Length + 2))//寫入USL
                        {
                            Excel_WS1.Cells[Y_axis, X_axis] = _sSPEC_USL[X_axis - 2];
                        }
                        else if ((Y_axis == 4) & (X_axis < _sKey.Length + 2))//寫入AVG
                        {
                            if (!_sKey[X_axis - 2].ToUpper().Contains("ICCID") && !_sKey[X_axis - 2].ToUpper().Contains("IMSI") && !_sKey[X_axis - 2].ToUpper().Contains("IMEI"))
                            {
                                if (sGRR == "Enable")
                                {
                                    Excel_WS1.Cells[Y_axis, X_axis] = "=IFERROR(AVERAGE(R[4]C:R[" + (iTest_times + 3) + "]C), \"N/A\")";//IFERROR(AVERAGE(R[3]C: R[13]C), "N/A")
                                }
                                else
                                {
                                    Excel_WS1.Cells[Y_axis, X_axis] = "=IFERROR(AVERAGE(R[3]C:R[" + (iTest_times + 2) + "]C), \"N/A\")";//IFERROR(AVERAGE(R[3]C: R[13]C), "N/A")
                                }
                            }
                            else
                            {
                                Excel_WS1.Cells[Y_axis, X_axis] = "N/A";
                            }
                        }
                        else if ((Y_axis == 5) & (X_axis < _sKey.Length + 2))//寫入STDEV
                        {

                            if (!_sKey[X_axis - 2].ToUpper().Contains("ICCID") && !_sKey[X_axis - 2].ToUpper().Contains("IMSI") && !_sKey[X_axis - 2].ToUpper().Contains("IMEI"))
                            {
                                if (sGRR == "Enable")
                                {
                                    Excel_WS1.Cells[Y_axis, X_axis] = "=IFERROR(STDEV(R[3]C:R[" + (iTest_times + 2) + "]C), \"N/A\")";//IFERROR(STDEV(R[2]C: R[12]C), "N/A")
                                }
                                else
                                {
                                    Excel_WS1.Cells[Y_axis, X_axis] = "=IFERROR(STDEV(R[2]C:R[" + (iTest_times + 1) + "]C), \"N/A\")";//IFERROR(STDEV(R[2]C: R[12]C), "N/A")
                                }
                            }
                            else
                            {
                                Excel_WS1.Cells[Y_axis, X_axis] = "N/A";
                            }
                        }
                        else if ((Y_axis == 6) & (X_axis < _sKey.Length + 2))//寫入Cpk
                        {
                            if (!_sKey[X_axis - 2].ToUpper().Contains("ICCID") && !_sKey[X_axis - 2].ToUpper().Contains("IMSI") && !_sKey[X_axis - 2].ToUpper().Contains("IMEI") && !_sKey[X_axis - 2].ToUpper().Contains("ATS_TIME"))
                            {
                                //Cpk,MIN((USL-AVG)/(3*標準差stdev),(AVG-LSL)/(3*標準差stdev))
                                Excel_WS1.Cells[Y_axis, X_axis] = "=IFERROR(MIN((R[-3]C - R[-2]C) / (3 * R[-1]C),(R[-2]C - R[-4]C)/ (3 * R[-1]C)),\"N/A\")";//IFERROR(MIN((R[-3]C - R[-2]C) / (3 * R[-1]C),(R[-2]C - R[-4]C)/ (3 * R[-1]C)),"N/A")
                                Range = (Excel.Range)Excel_WS1.Cells[Y_axis, X_axis];
                                Excel.FormatCondition condition1 = (Excel.FormatCondition)Range.FormatConditions.Add(Excel.XlFormatConditionType.xlCellValue, Excel.XlFormatConditionOperator.xlLess, 1.33);
                                condition1.Font.Bold = true;
                                condition1.Interior.ColorIndex = 3;//紅色
                            }
                            else
                            {
                                Excel_WS1.Cells[Y_axis, X_axis] = "N/A";
                            }
                        }
                        else if ((Y_axis == 7) & (X_axis < _sKey.Length + 2) & sGRR == "Enable")//寫入GRR Torence
                        {
                            if (!_sKey[X_axis - 2].ToUpper().Contains("ICCID") && !_sKey[X_axis - 2].ToUpper().Contains("IMSI") && !_sKey[X_axis - 2].ToUpper().Contains("IMEI") && !_sKey[X_axis - 2].ToUpper().Contains("ATS_TIME") && IsNumeric(value[0, X_axis - 2]) && !_sKey[X_axis - 2].ToUpper().Contains("CHKCOUNT") && !_sKey[X_axis - 2].ToUpper().Contains("RETRYTIMES"))
                            {
                                //Cpk,MIN((USL-AVG)/(3*標準差stdev),(AVG-LSL)/(3*標準差stdev))
                                //Excel_WS1.Cells[Y_axis, X_axis] = "=IFERROR(MIN((R[-3]C - R[-2]C) / (3 * R[-1]C),(R[-2]C - R[-4]C)/ (3 * R[-1]C)),\"N/A\")";//IFERROR(MIN((R[-3]C - R[-2]C) / (3 * R[-1]C),(R[-2]C - R[-4]C)/ (3 * R[-1]C)),"N/A")
                                Minitab_GRR(X_axis, X_axis);
                                Excel_WS1.Cells[Y_axis, X_axis] = sGRR_Tolerance;
                                Range = (Excel.Range)Excel_WS1.Cells[Y_axis, X_axis];
                                Excel.FormatCondition condition1 = (Excel.FormatCondition)Range.FormatConditions.Add(Excel.XlFormatConditionType.xlCellValue, Excel.XlFormatConditionOperator.xlBetween, 10, 30);
                                condition1.Font.Bold = true;
                                condition1.Interior.ColorIndex = 6;//黃色
                                Excel.FormatCondition condition2 = (Excel.FormatCondition)Range.FormatConditions.Add(Excel.XlFormatConditionType.xlCellValue, Excel.XlFormatConditionOperator.xlGreater, 30);
                                condition2.Font.Bold = true;
                                condition2.Interior.ColorIndex = 3;//紅色
                            }
                            else
                            {
                                Excel_WS1.Cells[Y_axis, X_axis] = "N/A";
                            }
                        }
                        else
                        {
                            if (value[Y_axis - (iTitleNameCount + 1), X_axis - 2] == "" || value[Y_axis - (iTitleNameCount + 1), X_axis - 2] == null)//不等於空值就移除"="
                            {
                                Excel_WS1.Cells[Y_axis, X_axis] = value[Y_axis - (iTitleNameCount + 1), X_axis - 2];
                            }
                            else
                            {
                                Excel_WS1.Cells[Y_axis, X_axis] = value[Y_axis - (iTitleNameCount + 1), X_axis - 2].Replace("=", "");
                            }
                            if (IsNumeric(value[Y_axis - (iTitleNameCount + 1), X_axis - 2]) && value[Y_axis - (iTitleNameCount + 1), X_axis - 2] != null)
                            {
                                if (!_sKey[X_axis - 2].ToUpper().Contains("ICCID") && !_sKey[X_axis - 2].ToUpper().Contains("IMSI") && !_sKey[X_axis - 2].ToUpper().Contains("IMEI") && !_sKey[X_axis - 2].ToUpper().Contains("ATS_TIME"))
                                {
                                    if (Convert.ToDouble(value[Y_axis - (iTitleNameCount + 1), X_axis - 2]) > Convert.ToDouble(_sSPEC_USL[X_axis - 2]) || Convert.ToDouble(value[Y_axis - (iTitleNameCount + 1), X_axis - 2]) < Convert.ToDouble(_sSPEC_LSL[X_axis - 2]))
                                    {
                                        Range = (Excel.Range)Excel_WS1.Cells[Y_axis, X_axis];
                                        //Range = Excel_WS1.get_Range((Excel.Range)Excel_WS1.Cells[Y_axis, X_axis], (Excel.Range)Excel_WS1.Cells[Y_axis, X_axis]);
                                        Range.Interior.ColorIndex = 3;//格子顯示紅色
                                    }
                                }
                            }
                            else if (value[Y_axis - (iTitleNameCount + 1), X_axis - 2] == "NG")
                            {
                                if (value[Y_axis - (iTitleNameCount + 1), X_axis - 2] != _sSPEC_USL[X_axis - 2])
                                {
                                    Range = (Excel.Range)Excel_WS1.Cells[Y_axis, X_axis];
                                    //Range = Excel_WS1.get_Range((Excel.Range)Excel_WS1.Cells[Y_axis, X_axis], (Excel.Range)Excel_WS1.Cells[Y_axis, X_axis]);
                                    Range.Interior.ColorIndex = 3;//格子顯示紅色
                                }
                            }
                        }
                    }
                }

                //設定顯示時間之格子的格式並置中
                //Range = Excel_WS1.Range["A:H"];
                //Range.EntireColumn.AutoFit();//自動調整格子寬度
                //Range.HorizontalAlignment = Excel.XlVAlign.xlVAlignCenter;//置中
                //Range.EntireColumn.NumberFormat = "HH:MM:SS.000";//調整時間顯示格式

                ////調整全部格子自動寬度、標題及總結格子的顏色
                //Range = Excel_WS1.Range["A1:F1"];
                //Range.EntireColumn.AutoFit();//自動調整格子寬度
                //Range.HorizontalAlignment = Excel.XlVAlign.xlVAlignCenter;//置中
                //Range.Interior.ColorIndex = 10;//格子顯示綠色
                //Range = Excel_WS1.Range["H1:J1"];
                //Range.EntireColumn.AutoFit();//自動調整格子寬度
                //Range.HorizontalAlignment = Excel.XlVAlign.xlVAlignCenter;//置中
                //Range.Interior.ColorIndex = 10;//格子顯示綠色
                //Range = Excel_WS1.Range["A" + (r + 1) + ":F" + (r + 1)];
                //Range.Interior.ColorIndex = 6;//格子顯示黃色
                //Range = Excel_WS1.Range["H" + (r + 1) + ":J" + (r + 1)];
                //Range.Interior.ColorIndex = 6;//格子顯示黃色
                //Range = Excel_WS1.Range["H" + (r + 1) + ":J" + (r + 1)];
                //Range.Interior.ColorIndex = 3;//格子顯示紅色

                //設定全部框線
                //Range = Excel_WS1.Range["A1:F" + (r + 1)];
                //Range.Borders.Weight = Excel.XlBorderWeight.xlThin;//增加框線
                //excel_sketch();
                string ExcelFile = sResultPath + "\\" + sLogNamePath + "\\" + sLogNamePath;
                if (File.Exists(ExcelFile + ".xlsx"))
                    File.Delete(ExcelFile + ".xlsx");
                Excel_WB1.SaveAs(ExcelFile, Excel.XlFileFormat.xlOpenXMLWorkbook, System.Reflection.Missing.Value, System.Reflection.Missing.Value, false, false,
        Excel.XlSaveAsAccessMode.xlExclusive, false, false, System.Reflection.Missing.Value, System.Reflection.Missing.Value, System.Reflection.Missing.Value);
                Excel_WB1.Close();
                Excel_APP.Quit();
                Excel_APP = null;
                if (sGRR == "Enable")
                {
                    Excel_WB1_GRR.Close(0);
                    Excel_APP_GRR.Quit();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(Excel_APP_GRR);
                    Excel_WS1_GRR = null;
                    Excel_WB1_GRR = null;
                    Excel_APP_GRR = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void WriteToExcel_CycleTimeAnalysis()
        {
            //增加新分頁
            Excel_WS1 = (Excel.Worksheet)Excel_WB1.Worksheets.Add();

            Excel_APP.Visible = false;
            Excel.Range Range = Excel_WS1.Range["A1"];
            Excel_WS1.Name = sStation;
            Excel_WS1.Cells[1, 1] = "Test times(Start)";
            Excel_WS1.Cells[1, 2] = "Test times(End)";
            Excel_WS1.Cells[1, 3] = "Test Item";
            Excel_WS1.Cells[1, 4] = "Note";
            Excel_WS1.Cells[1, 5] = "Origin test time(s)";
            Excel_WS1.Cells[1, 6] = "Remind";
            Excel_WS1.Cells[1, 8] = "Action";
            Excel_WS1.Cells[1, 9] = "Reduce time";
            Excel_WS1.Cells[1, 10] = "Optimized test time(s)";

            for (int r = 0; r < 999; r++)
            {
                if (_sTime[r] != null)
                {
                    if (r == 0)
                    {
                        Excel_WS1.Cells[r + 2, 1] = _sTime[r];
                        Excel_WS1.Cells[r + 2, 2] = _sTime[r + 1];
                    }
                    else
                    {
                        Excel_WS1.Cells[r + 2, 2] = _sTime[r + 1];
                        Excel_WS1.Cells[r + 2, 1] = "=B" + (r + 1);
                    }
                    Excel_WS1.Cells[r + 2, 3] = _sKey[r];
                    Excel_WS1.Cells[r + 2, 5] = "=VALUE(TEXT(B" + (r + 2) + "-A" + (r + 2) + "," + "\"[SS].000\"))";
                    if (sStation == "Final")// 提醒與PCBA相同測項
                    {
                        Excel_WS1.Cells[r + 2, 6] = "= IF(AND(IF((ISNA(VLOOKUP(C" + (r + 2) + ", PCBA!C:C, 1, FALSE))), \"\", VLOOKUP(C" + (r + 2) + ", PCBA!C:C, 1, FALSE)) = C" + (r + 2) + ", C" + (r + 2) + " <> \"RetryTime\", C" + (r + 2) + " <> \"Test Time\", C" + (r + 2) + " <> \"Linux\"), \"此測項與PCBA站有相同測項，請確認是否可以刪減\", \"\")";
                    }
                    Excel_WS1.Cells[r + 2, 10] = "=E" + (r + 2) + "-I" + (r + 2);
                }
                else if (_sTime[r - 1] != null)
                {
                    //設定總測試時間
                    Excel_WS1.Cells[r + 1, 1] = "";
                    Excel_WS1.Cells[r + 1, 4] = _sKeyTemp[1];
                    Excel_WS1.Cells[r + 1, 5] = "=SUM(E2:E" + r + ")";
                    Excel_WS1.Cells[r + 1, 9] = "=SUM(I2:I" + r + ")";
                    Excel_WS1.Cells[r + 1, 10] = "=SUM(J2:J" + r + ")";
                    Range = Excel_WS1.Range["D" + (r + 1)];
                    Range.AddComment().Text("Test time in log");//增加註釋  

                    //偵測log中顯示之test time與計算出來的test time是否差異過大
                    Excel_WS1.Cells[r + 1, 6] = "=IF(AND(E" + (r + 1) + "<D" + (r + 1) + "+5,E" + (r + 1) + ">D" + (r + 1) + "-5),\"\",\"總測試時間與log中test time差異過大，請確認\")";

                    //設定顯示時間之格子的格式並置中
                    Range = Excel_WS1.Range["A:B"];
                    Range.EntireColumn.AutoFit();//自動調整格子寬度
                    Range.HorizontalAlignment = Excel.XlVAlign.xlVAlignCenter;//置中
                    Range.EntireColumn.NumberFormat = "HH:MM:SS.000";//調整時間顯示格式
                    Range = Excel_WS1.Range["E:E"];
                    Range.EntireColumn.AutoFit();//自動調整格子寬度
                    Range.HorizontalAlignment = Excel.XlVAlign.xlVAlignCenter;//置中
                    Range = Excel_WS1.Range["I:J"];
                    Range.EntireColumn.AutoFit();//自動調整格子寬度
                    Range.HorizontalAlignment = Excel.XlVAlign.xlVAlignCenter;//置中

                    //調整全部格子自動寬度、標題及總結格子的顏色
                    Range = Excel_WS1.Range["A1:F1"];
                    Range.EntireColumn.AutoFit();//自動調整格子寬度
                    Range.HorizontalAlignment = Excel.XlVAlign.xlVAlignCenter;//置中
                    Range.Interior.ColorIndex = 33;//格子顯示藍色
                    Range.Font.ColorIndex = 2;//字體顯示白色
                    Range = Excel_WS1.Range["H1:J1"];
                    Range.EntireColumn.AutoFit();//自動調整格子寬度
                    Range.HorizontalAlignment = Excel.XlVAlign.xlVAlignCenter;//置中
                    Range.Interior.ColorIndex = 33;//格子顯示藍色
                    Range.Font.ColorIndex = 2;//字體顯示白色
                    Range = Excel_WS1.Range["A" + (r + 1) + ":F" + (r + 1)];
                    Range.Interior.ColorIndex = 6;//格子顯示黃色
                    Range = Excel_WS1.Range["H" + (r + 1) + ":J" + (r + 1)];
                    Range.Interior.ColorIndex = 6;//格子顯示黃色

                    //設定全部框線
                    Range = Excel_WS1.Range["A1:F" + (r + 1)];
                    Range.Borders.Weight = Excel.XlBorderWeight.xlThin;//增加框線
                    Range = Excel_WS1.Range["H1:J" + (r + 1)];
                    Range.Borders.Weight = Excel.XlBorderWeight.xlThin;//增加框線

                    //Worksheet 移動
                    Excel_WS1.Move(Excel_WB1.Worksheets[Excel_WB1.Worksheets.Count]);

                    //Summary
                    Excel_WS1 = (Excel.Worksheet)Excel_WB1.Worksheets[Excel_WB1.Worksheets.Count];
                    Excel_WS1.Cells[iTestCount + 1, 1] = sStation;
                    Excel_WS1.Cells[iTestCount + 1, 2] = "=" + sStation + "!" + "E" + (r + 1);
                    Excel_WS1.Cells[iTestCount + 1, 3] = "=" + sStation + "!" + "J" + (r + 1);

                    if (iTestCount == FileName.Length)
                    {
                        Excel_WS1.Cells[1, 1] = "Station";
                        Excel_WS1.Cells[1, 2] = "Origin test time(s)";
                        Excel_WS1.Cells[1, 3] = "Optimized test time(s)";
                        Range = Excel_WS1.Range["A:C"];
                        Range.EntireColumn.AutoFit();//自動調整格子寬度
                        Range.HorizontalAlignment = Excel.XlVAlign.xlVAlignCenter;//置中
                        Range = Excel_WS1.Range["A1:C1"];
                        Range.Interior.ColorIndex = 33;//格子顯示藍色
                        Range.Font.ColorIndex = 2;//字體顯示白色
                        //設定全部框線
                        Range = Excel_WS1.Range["A1:C" + (iTestCount + 1)];
                        Range.Borders.Weight = Excel.XlBorderWeight.xlThin;//增加框線
                        Excel_WS1.Name = "Summary";
                        Excel_WS1.Move(Excel_WB1.Worksheets[1]);
                    }
                    this.Invoke(new UpdateLableHandler(printResult), new object[] { text });
                }
            }
        }
        private void WriteToExcel_TestTimeAnalysis()
        {
            try
            {
                Excel.Application Excel_APP = new Excel.Application();
                Excel.Workbook Excel_WB1 = Excel_APP.Workbooks.Add(true);
                Excel.Worksheet Excel_WS1 = new Excel.Worksheet();
                Excel_WS1 = ((Excel.Worksheet)Excel_WB1.Worksheets[1]);
                Excel_APP.Visible = true;
                Excel.Range Range = Excel_WS1.Range["A1"];
                int iTitleNameCount = 1;
                Excel_WS1.Name = "Raw data";
                Excel_WS1.Cells[iTitleNameCount, 1] = "Test item";
                Excel_WS1.Cells[1, 2] = "Test time";
                Excel_WS1.Cells[1, 3] = "Start";
                Excel_WS1.Cells[1, sTestItem.Length + 4] = "File name";

                for (int X_axis = 1; X_axis <= _sKey.Length + 4; X_axis++)//_sKey.Length + 4為總測項加Test Item 、Test time、 Start 、 File name
                {
                    for (int Y_axis = 1; Y_axis <= iTest_times + iTitleNameCount; Y_axis++)//iTest_times + 1為總測試次數加Test item
                    {
                        if ((X_axis == 1 || X_axis == 2 || X_axis == 3 || X_axis == _sKey.Length + 4) && Y_axis <= iTitleNameCount) // 1/2/3&最後一欄不需填資料(項目名稱在上面已輸入)
                        {
                            continue;
                        }
                        else if (X_axis == 1)//標記測試數
                        {
                            Excel_WS1.Cells[Y_axis, X_axis] = Y_axis - iTitleNameCount;
                        }
                        else if (X_axis == 2)//ATS test time
                        {
                            Excel_WS1.Cells[Y_axis, X_axis] = sATS_test_time[Y_axis - 2];
                        }
                        else if ((Y_axis == 1) & (X_axis < _sKey.Length + 4))//寫入測項名稱
                        {
                            Excel_WS1.Cells[Y_axis, X_axis] = _sKey2[X_axis - 4];
                        }
                        else//寫入測試值
                        {
                            Excel_WS1.Cells[Y_axis, X_axis] = value[Y_axis - (iTitleNameCount + 1), X_axis - 3];
                        }
                    }
                }
                //格子的格式並置中
                Excel_WS1.Columns.AutoFit();//自動調整格子寬度
                Excel_WS1.Columns.HorizontalAlignment = Excel.XlVAlign.xlVAlignCenter;//置中
                Range = Excel_WS1.Range[Excel_WS1.Cells[1, 1], Excel_WS1.Cells[1, sTestItem.Length + 4]];//選取項目列
                Range.Interior.ColorIndex = 33;//格子顯示藍色
                Range.Font.ColorIndex = 2;//字體顯示白色
                Range = Excel_WS1.Range[Excel_WS1.Cells[1, 1], Excel_WS1.Cells[iTest_times + iTitleNameCount, sTestItem.Length + 4]];//選取全部含資料格
                Range.Borders.Weight = Excel.XlBorderWeight.xlThin;//增加框線
                //設定顯示時間之格子的格式並置中
                Range = Excel_WS1.Range[Excel_WS1.Cells[2, 3], Excel_WS1.Cells[sTestItem.Length, iTest_times]];
                Range.EntireColumn.NumberFormat = "HH:MM:SS.000";//調整時間顯示格式


                Excel_WS1 = (Excel.Worksheet)Excel_WB1.Worksheets.Add();
                Excel_WS1.Name = "Summary";
                for (int X_axis = 1; X_axis <= _sKey.Length + 1; X_axis++)
                {
                    for (int Y_axis = 1; Y_axis <= iTest_times + iTitleNameCount; Y_axis++)//iTest_times + 1為總測試次數加Test item
                    {
                        if (Y_axis == 1) //項目
                        {
                            if (X_axis == _sKey.Length + 1)
                            {
                                Excel_WS1.Cells[1, _sKey.Length + 1] = "Test time";
                            }
                            else
                            {
                                Excel_WS1.Cells[Y_axis, X_axis] = _sKey2[X_axis - 1];
                            }
                        }
                        else if (X_axis == _sKey.Length + 1)
                        {
                            Excel_WS1.Cells[Y_axis, X_axis] = sATS_test_time[Y_axis - 2];
                        }
                        else
                        {
                            Excel_WS1.Cells[Y_axis, X_axis] = "=VALUE(TEXT('Raw data'!RC[3]-'Raw data'!RC[2],\"[SS].000\"))";
                        }
                    }
                }
                //格子的格式並置中
                Excel_WS1.Columns.AutoFit();//自動調整格子寬度
                Excel_WS1.Columns.HorizontalAlignment = Excel.XlVAlign.xlVAlignCenter;//置中
                Range = Excel_WS1.Range[Excel_WS1.Cells[1, 1], Excel_WS1.Cells[1, sTestItem.Length + 1]];//選取項目列
                Range.Interior.ColorIndex = 33;//格子顯示藍色
                Range.Font.ColorIndex = 2;//字體顯示白色
                Range = Excel_WS1.Range[Excel_WS1.Cells[1, 1], Excel_WS1.Cells[iTest_times + iTitleNameCount, sTestItem.Length + 1]];//選取全部含資料格
                Range.Borders.Weight = Excel.XlBorderWeight.xlThin;//增加框線


                string ExcelFile = sResultPath + "\\" + sLogNamePath + "\\" + sLogNamePath;
                if (File.Exists(ExcelFile + ".xlsx"))
                    File.Delete(ExcelFile + ".xlsx");
                Excel_WB1.SaveAs(ExcelFile, Excel.XlFileFormat.xlOpenXMLWorkbook, System.Reflection.Missing.Value, System.Reflection.Missing.Value, false, false,
        Excel.XlSaveAsAccessMode.xlExclusive, false, false, System.Reflection.Missing.Value, System.Reflection.Missing.Value, System.Reflection.Missing.Value);
                Excel_WB1.Close();
                Excel_APP.Quit();
                Excel_APP = null;
                if (sGRR == "Enable")
                {
                    Excel_WB1_GRR.Close(0);
                    Excel_APP_GRR.Quit();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(Excel_APP_GRR);
                    Excel_WS1_GRR = null;
                    Excel_WB1_GRR = null;
                    Excel_APP_GRR = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public DataTable ReadExcel(string fileName, string fileExt)
        {
            string conn = string.Empty;
            DataTable dtexcel = new DataTable();
            if (fileExt.CompareTo(".xls") == 0)
                conn = @"provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileName + ";Extended Properties='Excel 8.0;HRD=Yes;IMEX=1';"; //for below excel 2007  
            else
                conn = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileName + ";Extended Properties='Excel 12.0;HDR=Yes';"; //for above excel 2007  
            using (OleDbConnection con = new OleDbConnection(conn))
            {
                try
                {
                    OleDbDataAdapter oleAdpt = new OleDbDataAdapter("select * from [Sheet1$]", con); //here we read data from sheet1  
                    oleAdpt.Fill(dtexcel); //fill excel data into dataTable  
                    //string[,] excelll = dtexcel.Rows.OfType<DataRow>().Select(k => k[0].ToString()).ToArray();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
            }
            return dtexcel;
        }
        private void ReadExcelFileToDatabase()
        {
            string filePath = string.Empty;
            string fileExt = string.Empty;
            filePath = Directory.GetCurrentDirectory() + "\\" + "LVP2_NG_database.xlsx"; //get the path of the file  
            fileExt = Path.GetExtension(filePath); //get the file extension  
            if (fileExt.CompareTo(".xls") == 0 || fileExt.CompareTo(".xlsx") == 0)
            {
                try
                {
                    int iDatacount_Columns = 0;
                    int iDatacount_Rows = 0;
                    DataTable dtExcel = new DataTable();
                    dtExcel = ReadExcel(Directory.GetCurrentDirectory() + "\\" + "LVP2_NG_database.xlsx", fileExt); //read excel file  
                    sDatabase = new string[dtExcel.Columns.Count, dtExcel.Rows.Count];
                    string[] _sKeyTemp = new string[10000];
                    //List<string[,]> excelll = new List<string[dtExcel.Columns.Count, dtExcel.Rows.Count]>;
                    //List<string[,]> sDatabase = new List<string[,]>();
                    foreach (DataColumn dc in dtExcel.Columns)
                    {
                        foreach (DataRow dr in dtExcel.Rows)
                        {
                            //sDatabase[iDatacount_Columns, iDatacount_Rows] = dr.ToString();
                            if (iDatacount_Columns == 0 && iDatacount_Rows != 0)
                            {
                                sTemp = dtExcel.Rows[iDatacount_Rows][iDatacount_Columns].ToString().Replace(':', '_');
                                sTemp = sTemp.Replace("SPEC_", "SPEC:").Replace("Log _", "Log :").Replace("Value_", "Value:").Replace("[ Log ]  ", "");
                                _sKeyTemp = sTemp.Split(' ');
                                for (int iItemCount = 1; iItemCount < _sKeyTemp.Length; iItemCount++)//將測項名稱包含空白的重新串起來
                                {
                                    _sKeyTemp[0] = _sKeyTemp[0] + " " + _sKeyTemp[iItemCount];
                                }
                                sDatabase[iDatacount_Columns, iDatacount_Rows] = _sKeyTemp[0];
                            }
                            else
                            {
                                sDatabase[iDatacount_Columns, iDatacount_Rows] = dtExcel.Rows[iDatacount_Rows][iDatacount_Columns].ToString();
                            }
                            iDatacount_Rows++;
                        }
                        iDatacount_Rows = 0;
                        iDatacount_Columns++;
                    }
                    //for (iDatacount_Columns = 0; iDatacount_Columns < dtExcel.Columns.Count; iDatacount_Columns++)
                    //{
                    //    for (iDatacount_Rows = 0; iDatacount_Rows < dtExcel.Columns.Count; iDatacount_Rows++)
                    //    {
                    //        sDatabase[iDatacount_Columns, iDatacount_Rows] = dtExcel[iDatacount_Columns][iDatacount_Rows];
                    //        columnCount++;
                    //    }
                    //    sDatabase.Add(myTableRow);
                    //}
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
        //public static DataTable ImportSheet(string fileName)
        //{
        //    var datatable = new DataTable();
        //    var workbook = new XLWorkbook(fileName);
        //    var xlWorksheet = workbook.Worksheet(1);
        //    var range = xlWorksheet.Range(xlWorksheet.FirstCellUsed(), xlWorksheet.LastCellUsed());

        //    var col = range.ColumnCount();
        //    var row = range.RowCount();

        //    //if a datatable already exists, clear the existing table 
        //    datatable.Clear();
        //    for (var i = 1; i <= col; i++)
        //    {
        //        var column = xlWorksheet.Cell(1, i);
        //        datatable.Columns.Add(column.Value.ToString());
        //    }

        //    var firstHeadRow = 0;
        //    foreach (var item in range.Rows())
        //    {
        //        if (firstHeadRow != 0)
        //        {
        //            var array = new object[col];
        //            for (var y = 1; y <= col; y++)
        //            {
        //                array[y - 1] = item.Cell(y).Value;
        //            }

        //            datatable.Rows.Add(array);
        //        }
        //        firstHeadRow++;
        //    }
        //    return datatable;
        //}
        private void ReadExcelFileToArray()
        {
            string filePath = string.Empty;
            string filePath_new = string.Empty;
            string fileExt = string.Empty;
            List<string> GRR_lst = new List<string> { "GRR_MA2", "GRR_MA3", "GRR_MA4", "GRR_MA5" };
            filePath = Directory.GetCurrentDirectory() + "\\" + WNC.API.Func.ReadINI("setting", "setting", "File_Name", "") + ".xlsx"; //get the path of the file  
            filePath_new = Directory.GetCurrentDirectory() + "\\" + WNC.API.Func.ReadINI("setting", "setting", "File_Name", "") + "_new"; //get the path of the file 
            fileExt = Path.GetExtension(filePath); //get the file extension  
            int iDatacount_Columns = 0;
            int iDatacount_Rows = 0;
            if (fileExt.CompareTo(".xls") == 0 || fileExt.CompareTo(".xlsx") == 0)
            {
                try
                {
                    Excel_APP_GRR = new Excel.Application();
                    Excel_WB1_GRR = Excel_APP_GRR.Workbooks.Open(Directory.GetCurrentDirectory() + "\\Two way anova & GRR_test.xlsx");
                    Excel_APP_GRR_LAWI = new Excel.Application();
                    Excel_APP_GRR_LAWI.Visible = true;
                    Excel_WB1_GRR_LAWI = Excel_APP_GRR_LAWI.Workbooks.Open(Directory.GetCurrentDirectory() + "\\" + WNC.API.Func.ReadINI("setting", "setting", "File_Name", "") + ".xlsx");
                    Excel_WS1_GRR_LAWI = Excel_WB1_GRR_LAWI.Worksheets[1];
                    for (int i = 5; i < 9; i++)
                    {
                        Excel.Range newRow = Excel_APP_GRR_LAWI.Rows[i];
                        newRow.Insert();
                        Excel.Range firstCell = Excel_APP_GRR_LAWI.Cells[i, 1];
                        firstCell.Value = GRR_lst[i - 5];
                    }
                    int iStart = Convert.ToInt32(WNC.API.Func.ReadINI("setting", "setting", "Start", ""));
                    int iEnd = Convert.ToInt32(WNC.API.Func.ReadINI("setting", "setting", "End", ""));
                    for (int Y = iStart; Y <= iEnd; Y++)
                    {
                        Minitab_GRR_LAWI(1, Y);
                        Excel_WS1_GRR_LAWI.Cells[4, Y] = sGRR_Tolerance;
                        Excel_WS1_GRR_LAWI.Cells[5, Y] = sGRR_Tolerance_MA2;
                        Excel_WS1_GRR_LAWI.Cells[6, Y] = sGRR_Tolerance_MA3;
                        Excel_WS1_GRR_LAWI.Cells[7, Y] = sGRR_Tolerance_MA4;
                        Excel_WS1_GRR_LAWI.Cells[8, Y] = sGRR_Tolerance_MA5;

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
        public void excel_sketch()
        {
            //Excel._Application myExcel = null;
            //Excel._Workbook myBook = null;
            //Excel._Worksheet mySheet = null;
            try
            {
                //myExcel = new Excel.Application();    //開啟一個新的應用程式
                Excel_APP.DisplayAlerts = false;        //停用警告訊息
                //myBook = myExcel.Workbooks.Add(true); //新增活頁簿
                Excel_WS2 = (Excel.Worksheet)Excel_WB1.Worksheets.Add();
                Excel_WS2.Name = "圖表";            //變更活頁簿名稱
                Excel_WS1 = (Excel.Worksheet)Excel_WB1.Worksheets[2];//引用第一張工作表
                Excel_APP.Visible = true;               //顯示Excel程式

                //Excel_WS1.Cells.Font.Name = "標楷體";   //設定Excel資料字體字型
                //Excel_WS1.Cells.Font.Size = 20;         //設定Excel資料字體大小

                //在工作簿 新增一張 統計圖表，單獨放在一個分頁裡面
                Excel_WB1.Charts.Add(Type.Missing, Type.Missing, 1, Type.Missing);
                //選擇 統計圖表 的 圖表種類
                Excel_WB1.ActiveChart.ChartType = Excel.XlChartType.xlLineMarkers;//插入折線圖
                //設定數據範圍
                //string strRange = "A1:B4";
                //設定 統計圖表 的 數據範圍內容
                Excel_WB1.ActiveChart.SetSourceData(Excel_WS1.get_Range("D4", "D" + (iTest_times + 3)), Excel.XlRowCol.xlColumns);
                //將新增的統計圖表 插入到 指定位置(可以從單獨的分頁放到一個分頁裡面)
                Excel_WB1.ActiveChart.Location(Excel.XlChartLocation.xlLocationAsObject, Excel_WS2.Name);

                Excel_WS2.Shapes.Item("Chart 1").Width = 450;   //調整圖表寬度
                Excel_WS2.Shapes.Item("Chart 1").Height = 254;  //調整圖表高度
                Excel_WS2.Shapes.Item("Chart 1").Top = 0;      //調整圖表在分頁中的高度(上邊距) 位置
                Excel_WS2.Shapes.Item("Chart 1").Left = 0;    //調整圖表在分頁中的左右(左邊距) 位置

                //設定 繪圖區 的 背景顏色
                Excel_WB1.ActiveChart.PlotArea.Interior.Color = ColorTranslator.ToOle(Color.LightGray);
                //設定 繪圖區 的 邊框線條樣式
                Excel_WB1.ActiveChart.PlotArea.Border.LineStyle = Excel.XlLineStyle.xlDash;
                //設定 繪圖區 的 寬度
                Excel_WB1.ActiveChart.PlotArea.Width = 420;
                //設定 繪圖區 的 高度
                Excel_WB1.ActiveChart.PlotArea.Height = 230;
                //設定 繪圖區 在 圖表中的 高低位置(上邊距)
                Excel_WB1.ActiveChart.PlotArea.Top = 41;
                //設定 繪圖區 在 圖表中的 左右位置(左邊距)
                Excel_WB1.ActiveChart.PlotArea.Left = 10;
                //設定 繪圖區 的 x軸名稱下方 顯示y軸的 數據資料
                Excel_WB1.ActiveChart.HasDataTable = false;

                //設定 圖表的 背景顏色__方法1 使用colorIndex(放上色彩索引)
                Excel_WB1.ActiveChart.ChartArea.Interior.ColorIndex = 10;
                //設定 圖表的 背景顏色__方法2 使用color(放入色彩名稱)
                Excel_WB1.ActiveChart.ChartArea.Interior.Color = ColorTranslator.ToOle(Color.LightGray);
                //設定 圖表的 邊框顏色__方法1 使用colorIndex(放上色彩索引)
                Excel_WB1.ActiveChart.ChartArea.Border.ColorIndex = 10;
                //設定 圖表的 邊框顏色__方法2 使用color(放入色彩名稱)
                Excel_WB1.ActiveChart.ChartArea.Border.Color = ColorTranslator.ToOle(Color.LightGreen);
                //設定 圖表的 邊框樣式 
                Excel_WB1.ActiveChart.ChartArea.Border.LineStyle = Excel.XlLineStyle.xlDash;

                //設置Legend圖例
                Excel_WB1.ActiveChart.Legend.Top = 5;           //設定 圖例 的 上邊距
                Excel_WB1.ActiveChart.Legend.Left = 185;        //設定 圖例 的 左邊距
                //設定 圖例 的 背景色彩
                Excel_WB1.ActiveChart.Legend.Interior.Color = ColorTranslator.ToOle(Color.LightGreen);
                Excel_WB1.ActiveChart.Legend.Width = 55;        //設定 圖例 的 寬度
                Excel_WB1.ActiveChart.Legend.Height = 20;       //設定 圖例 的 高度
                Excel_WB1.ActiveChart.Legend.Font.Size = 11;    //設定 圖例 的 字體大小 
                Excel_WB1.ActiveChart.Legend.Font.Bold = true;  //設定 圖例 的 字體樣式=粗體
                Excel_WB1.ActiveChart.Legend.Font.Name = "細明體";//設定 圖例 的 字體字型=細明體
                Excel_WB1.ActiveChart.Legend.Position = Excel.XlLegendPosition.xlLegendPositionTop;//設訂 圖例 的 位置靠上 
                Excel_WB1.ActiveChart.Legend.Border.LineStyle = Excel.XlLineStyle.xlDash;//設定 圖例 的 邊框線條

                //設定 圖表 x 軸 內容
                //宣告
                Excel.Axis xAxis = (Excel.Axis)Excel_WB1.ActiveChart.Axes(Excel.XlAxisType.xlValue, Excel.XlAxisGroup.xlPrimary);
                //設定 圖表 x軸 橫向線條 線條樣式
                xAxis.MajorGridlines.Border.LineStyle = Excel.XlLineStyle.xlDash;
                //設定 圖表 x軸 橫向線條顏色__方法1
                xAxis.MajorGridlines.Border.ColorIndex = 8;
                //設定 圖表 x軸 橫向線條顏色__方法2
                xAxis.MajorGridlines.Border.Color = ColorTranslator.ToOle(Color.LightGreen);
                xAxis.HasTitle = true;  //設定 x軸 座標軸標題 = false(不顯示)，不打就是不顯示
                xAxis.AxisTitle.Text = "測試數值";
                xAxis.MinimumScale = Convert.ToDouble(_sSPEC_LSL[3]);  //設定 x軸 數值 最小值      
                xAxis.MaximumScale = Convert.ToDouble(_sSPEC_USL[3]);  //設定 x軸 數值 最大值
                xAxis.TickLabels.Font.Name = "標楷體"; //設定 x軸 字體字型=標楷體
                xAxis.TickLabels.Font.Size = 7;       //設定 x軸 字體大小

                //設定 圖表 y軸 內容
                Excel.Axis yAxis = (Excel.Axis)Excel_WB1.ActiveChart.Axes(Excel.XlAxisType.xlCategory, Excel.XlAxisGroup.xlPrimary);
                yAxis.TickLabels.Font.Name = "標楷體"; //設定 y軸 字體字型=標楷體 
                yAxis.TickLabels.Font.Size = 7;       //設定 y軸 字體大小
                yAxis.HasTitle = true;  //設定 y軸 座標軸標題 = false(不顯示)，不打就是不顯示
                yAxis.AxisTitle.Text = "測試次數";
                //yAxis.MinimumScale = 0;  //設定 y軸 數值 最小值      
                //yAxis.MaximumScale = iTest_times;  //設定 y軸 數值 最大值

                //設定 圖表 標題 顯示 = false(關閉)
                Excel_WB1.ActiveChart.HasTitle = true;
                //設定 圖表 標題 = 匯率
                Excel_WB1.ActiveChart.ChartTitle.Text = _sKey[2];
                //設定 圖表 標題 陰影 = false(關閉)
                Excel_WB1.ActiveChart.ChartTitle.Shadow = false;
                //設定 圖表 標題 邊框樣式
                Excel_WB1.ActiveChart.ChartTitle.Border.LineStyle = Excel.XlLineStyle.xlDash;



                ////選擇統計圖表的 圖表種類=3D類型的統計圖表 Floor才可以使用
                //myBook.ActiveChart.ChartType = Excel.XlChartType.xl3DColumn;//插入3D統計圖表
                ////設定 圖表的 Floor顏色__方法1 使用colorIndex(放上色彩索引)
                //myBook.ActiveChart.Floor.Interior.ColorIndex = 1;
                ////設定 圖表的 Floor顏色__方法2 使用color(放入色彩名稱)
                //myBook.ActiveChart.Floor.Interior.Color = ColorTranslator.ToOle(Color.LightGreen);           
            }
            catch (Exception)
            {
                Excel_APP.Visible = true;
            }
            //finally
            //{
            //    //把執行的Excel資源釋放
            //    System.Runtime.InteropServices.Marshal.ReleaseComObject(myExcel);
            //    myExcel = null;
            //    myBook = null;
            //    mySheet = null;
            //}
        }
        public void excel_sketch_2()
        {
            try
            {
                Excel.Application excel = new Excel.Application();
                Excel.Workbook workbook = excel.Workbooks.Add();
                Excel.Worksheet worksheet = workbook.ActiveSheet;
                excel.Visible = true;

                // 设置数据范围
                Excel.Range dataRange = worksheet.Range["A1:A100"];

                // 填充数据
                for (int i = 1; i <= 100; i++)
                {
                    dataRange.Cells[i, 1].Value = GetNormalDistributionValue(0, 1);
                }

                // 创建Chart对象
                Excel.ChartObjects chartObjects = (Excel.ChartObjects)worksheet.ChartObjects(Type.Missing);
                Excel.ChartObject chartObject = chartObjects.Add(100, 100, 500, 250);
                Excel.Chart chart = chartObject.Chart;

                // 设置图表类型
                chart.ChartType = Excel.XlChartType.xlXYScatterLines;

                // 添加数据系列
                Excel.SeriesCollection seriesCollection = chart.SeriesCollection();
                Excel.Series series = seriesCollection.NewSeries();
                series.Values = dataRange;

                // 添加常数数据系列
                Excel.Range constRange = worksheet.Range["B1:B2"];
                constRange.Cells[1, 1].Value = 0.1;
                constRange.Cells[2, 1].Value = 0.1;

                Excel.Series constSeries = seriesCollection.NewSeries();
                constSeries.Values = constRange;
                constSeries.ChartType = Excel.XlChartType.xlLine;

                // 设置图表标题
                chart.HasTitle = true;
                chart.ChartTitle.Text = "Normal Distribution Chart";

                // 设置X轴和Y轴标题
                chart.Axes(Excel.XlAxisType.xlCategory).HasTitle = true;
                chart.Axes(Excel.XlAxisType.xlCategory).AxisTitle.Text = "Value";
                chart.Axes(Excel.XlAxisType.xlValue).HasTitle = true;
                chart.Axes(Excel.XlAxisType.xlValue).AxisTitle.Text = "Probability Density";

                // 保存Excel文件
                workbook.SaveAs("Normal Distribution Chart.xlsx");

                // 关闭Excel应用程序
                excel.Quit();
            }
            catch (Exception)
            {
                Excel_APP.Visible = true;
            }
        }
        // 获取正态分布值
        static double GetNormalDistributionValue(double mean, double stdDev)

        {
            Random rand = new Random();
            double u1 = rand.NextDouble();
            double u2 = rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                     Math.Sin(2.0 * Math.PI * u2);
            double randNormal =
                     mean + stdDev * randStdNormal;
            return randNormal;
        }
        public void Progress_step()
        {
            try
            {
                this.Invoke(new UpdateLableHandler(printResult), new object[] { text });
                string str = Math.Round((Decimal)(100 * progressBar1.Value) / progressBar1.Maximum, 4).ToString("#0.0000 ") + "%";
                Font font = new Font("Times New Roman", (float)10, FontStyle.Regular);
                PointF pt = new PointF(this.progressBar1.Width / 2 - 17, this.progressBar1.Height / 2 - 7);
                g.DrawString(str, font, Brushes.Blue, pt);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw new Exception(ex.Message);
            }
        }
        public void Minitab_sketch_All()
        {
            ProcessKill_AskBeforeKill("Mtb.exe");
            Mtb.Application MtbApp = new Mtb.Application();
            MtbApp.UserInterface.Visible = true;
            Console.WriteLine("Status = " + MtbApp.Status);
            Console.WriteLine("LastError = " + MtbApp.LastError);
            Console.WriteLine("Application Path = " + MtbApp.AppPath);
            Console.WriteLine("Window Handle = " + MtbApp.Handle);
            string[,] isNumber_string = new string[_sKey.Length, 6];
            int isNumber_string_count = 0;
            Mtb.Project MtbProj = MtbApp.ActiveProject;
            Mtb.Columns MtbColumns = MtbProj.ActiveWorksheet.Columns;
            for (int X_axis = 0; X_axis < _sKey.Length; X_axis++)
            {
                double Sum_double = 0;
                double Average_double = 0;
                string[] data_string = new string[iTest_times];
                ArrayList ArrayList = new ArrayList();
                double[] doubleValueList = new double[1];
                if (IsNumeric(value[0, X_axis]) && !_sKey[X_axis].Contains("IMEI") && !_sKey[X_axis].Contains("IMSI") && !_sKey[X_axis].Contains("ICCID") && !_sKey[X_axis].Contains("EID"))
                {
                    Sum_double = 0;
                    Average_double = 0;
                    Mtb.Column MtbColumn1 = MtbColumns.Add(null, null, 1);
                    MtbColumn1.Name = _sKey[X_axis];
                    for (int r = 0; r < iTest_times; r++)
                    {
                        if (value[r, X_axis] != null)
                        {
                            data_string[r] = value[r, X_axis];
                            ArrayList.Add(Convert.ToDouble(value[r, X_axis]));//紀錄最大最小值用
                            Sum_double += Convert.ToDouble(value[r, X_axis]);//計算平均值用
                        }
                    }
                    MtbColumn1.SetData(data_string);
                    ArrayList.Sort();
                    if (ArrayList.Count > 0)//有值的才計算標準差
                    {
                        doubleValueList = new double[ArrayList.Count];
                        for (int r = 0; r < ArrayList.Count; r++)
                        {
                            doubleValueList[r] = Convert.ToDouble(ArrayList[r]);//計算標準差使用
                        }
                    }
                    StDev(doubleValueList);//計算標準差回傳tmpStDev
                    Average_double = Sum_double / ArrayList.Count;//計算平均值             
                    //紀錄要畫的欄位&SPEC
                    isNumber_string[isNumber_string_count, 0] = (X_axis + 1).ToString();//Minitab 中的欄位
                    isNumber_string[isNumber_string_count, 1] = Regex.Replace(_sKey[X_axis], Illegal_character, "");//測項名稱
                    isNumber_string[isNumber_string_count, 2] = _sSPEC_LSL[X_axis];//LSL SPEC
                    isNumber_string[isNumber_string_count, 3] = _sSPEC_USL[X_axis];//USL SPEC
                    if (ArrayList.Count > 0)//沒有值的不計算
                    {
                        isNumber_string[isNumber_string_count, 4] = Convert.ToString(Convert.ToDouble(ArrayList[ArrayList.Count - 1]) - Convert.ToDouble(ArrayList[0]));//紀錄MAX-MIN
                    }
                    isNumber_string[isNumber_string_count, 5] = Convert.ToString(Math.Min((Convert.ToDouble(_sSPEC_USL[X_axis]) - Average_double) / (3 * tmpStDev), (Average_double - Convert.ToDouble(_sSPEC_LSL[X_axis])) / (3 * tmpStDev)));//Cpk,MIN((USL-AVG)/(3*標準差stdev),(AVG-LSL)/(3*標準差stdev))
                    isNumber_string_count++;
                }
                else
                {
                    Mtb.Column MtbColumn1 = MtbColumns.Add(null, null, 1);
                    MtbColumn1.Name = _sKey[X_axis];
                    for (int r = 0; r < iTest_times; r++)
                    {
                        data_string[r] = value[r, X_axis];
                    }
                    MtbColumn1.SetData(data_string);
                }
            }
            int Graph_count = 1;
            for (int i = 0; i < isNumber_string_count; i++)
            {
                if (isNumber_string[i, 4] != null)//沒有值的不畫圖
                {
                    //Text to Number
                    MtbProj.ExecuteCommand("Numeric C" + isNumber_string[i, 0] + " C" + isNumber_string[i, 0] + "." + "FNumeric C" + isNumber_string[i, 0] + ";Auto.");
                    Graph_count = Graph_count + 2;//Text to  Number執行了兩個指令，圖形存檔需標記第幾個指令
                    //時序圖                
                    MtbProj.ExecuteCommand("TSPlot C" + isNumber_string[i, 0] + ";TITLE \"" + isNumber_string[i, 1] + "\";AXLABEL 1 \"Test times\";AXLABEL 2 \"Test data\";ANGLE 0;REFERENCE 2 " + isNumber_string[i, 2] + " " + isNumber_string[i, 3] + ";Overlay;Symbol;Connect.");
                    Mtb.Graph MtbGraph = MtbProj.Commands.Item(Graph_count).Outputs.Item(1).Graph;
                    MtbGraph.SaveAs(sResultPath + "\\" + sLogNamePath + "\\" + isNumber_string[i, 1].Replace(".", ""), true, Mtb.MtbGraphFileTypes.GFPNGHighColor);
                    Graph_count++;

                    //CPK圖                    
                    if (isNumber_string[i, 4] != "0" && (isNumber_string[i, 3] != isNumber_string[i, 2]))//數值不是全部相同&上下限不相同
                    {
                        MtbProj.ExecuteCommand("Capa C" + isNumber_string[i, 0] + " 1;Lspec " + isNumber_string[i, 2] + ";Uspec " + isNumber_string[i, 3] + ";Pooled;AMR;UnBiased;OBiased;Toler 6;Within;Title \"" + isNumber_string[i, 1] + "\";Overall;NoCI;PPM;CStat.");
                        MtbGraph = MtbProj.Commands.Item(Graph_count).Outputs.Item(1).Graph;
                        if (Convert.ToDouble(isNumber_string[i, 5]) < 1.33)//cpk<1.33名稱冠上NG
                        {
                            MtbGraph.SaveAs(sResultPath + "\\" + sLogNamePath + "\\" + "!!!CPK lower than 1_33!!!_(" + isNumber_string[i, 5].Replace(".", "_") + ")" + isNumber_string[i, 1].Replace(".", "") + "_CPK", true, Mtb.MtbGraphFileTypes.GFPNGHighColor);
                        }
                        else
                        {
                            MtbGraph.SaveAs(sResultPath + "\\" + sLogNamePath + "\\" + isNumber_string[i, 1].Replace(".", "") + "_CPK", true, Mtb.MtbGraphFileTypes.GFPNGHighColor);
                        }
                        Graph_count++;
                    }
                }
            }
            MtbProj.SaveAs(sResultPath + "\\" + sLogNamePath + "\\" + sLogNamePath + ".MPJ", true);
            MtbApp.Quit();
            ProcessKill("Mtb.exe");
        }
        public void Minitab_sketch_OnlyFail()
        {
            ProcessKill_AskBeforeKill("Mtb.exe");
            Mtb.Application MtbApp = new Mtb.Application();
            MtbApp.UserInterface.Visible = true;
            Console.WriteLine("Status = " + MtbApp.Status);
            Console.WriteLine("LastError = " + MtbApp.LastError);
            Console.WriteLine("Application Path = " + MtbApp.AppPath);
            Console.WriteLine("Window Handle = " + MtbApp.Handle);
            string[,] isNumber_string = new string[_sKey.Length, 6];
            int isNumber_string_count = 0;
            Mtb.Project MtbProj = MtbApp.ActiveProject;
            Mtb.Columns MtbColumns = MtbProj.ActiveWorksheet.Columns;
            for (int X_axis = 0; X_axis < _sKey.Length; X_axis++)
            {
                double Sum_double = 0;
                double Average_double = 0;
                string[] data_string = new string[iTest_times];
                ArrayList ArrayList = new ArrayList();
                double[] doubleValueList = new double[1];
                if (IsNumeric(value[0, X_axis]) && !_sKey[X_axis].Contains("IMEI") && !_sKey[X_axis].Contains("IMSI") && !_sKey[X_axis].Contains("ICCID") && !_sKey[X_axis].Contains("EID"))
                {
                    Sum_double = 0;
                    Average_double = 0;
                    Mtb.Column MtbColumn1 = MtbColumns.Add(null, null, 1);
                    MtbColumn1.Name = _sKey[X_axis];
                    for (int r = 0; r < iTest_times; r++)
                    {
                        if (value[r, X_axis] != null)
                        {
                            data_string[r] = value[r, X_axis];
                            ArrayList.Add(Convert.ToDouble(value[r, X_axis]));//紀錄最大最小值用
                            Sum_double += Convert.ToDouble(value[r, X_axis]);//計算平均值用
                        }
                    }
                    MtbColumn1.SetData(data_string);
                    ArrayList.Sort();
                    if (ArrayList.Count > 0)//有值的才計算標準差
                    {
                        doubleValueList = new double[ArrayList.Count];
                        for (int r = 0; r < ArrayList.Count; r++)
                        {
                            doubleValueList[r] = Convert.ToDouble(ArrayList[r]);//計算標準差使用
                        }
                    }
                    StDev(doubleValueList);//計算標準差回傳tmpStDev
                    Average_double = Sum_double / ArrayList.Count;//計算平均值             
                    //紀錄要畫的欄位&SPEC
                    isNumber_string[isNumber_string_count, 0] = (X_axis + 1).ToString();//Minitab 中的欄位
                    isNumber_string[isNumber_string_count, 1] = Regex.Replace(_sKey[X_axis], Illegal_character, "");//測項名稱
                    isNumber_string[isNumber_string_count, 2] = _sSPEC_LSL[X_axis];//LSL SPEC
                    isNumber_string[isNumber_string_count, 3] = _sSPEC_USL[X_axis];//USL SPEC
                    if (ArrayList.Count > 0)//沒有值的不計算
                    {
                        isNumber_string[isNumber_string_count, 4] = Convert.ToString(Convert.ToDouble(ArrayList[ArrayList.Count - 1]) - Convert.ToDouble(ArrayList[0]));//紀錄MAX-MIN
                    }
                    isNumber_string[isNumber_string_count, 5] = Convert.ToString(Math.Min((Convert.ToDouble(_sSPEC_USL[X_axis]) - Average_double) / (3 * tmpStDev), (Average_double - Convert.ToDouble(_sSPEC_LSL[X_axis])) / (3 * tmpStDev)));//Cpk,MIN((USL-AVG)/(3*標準差stdev),(AVG-LSL)/(3*標準差stdev))
                    isNumber_string_count++;
                }
                else
                {
                    Mtb.Column MtbColumn1 = MtbColumns.Add(null, null, 1);
                    MtbColumn1.Name = _sKey[X_axis];
                    for (int r = 0; r < iTest_times; r++)
                    {
                        data_string[r] = value[r, X_axis];
                    }
                    MtbColumn1.SetData(data_string);
                }
            }
            int Graph_count = 1;
            for (int i = 0; i < isNumber_string_count; i++)
            {
                if ((isNumber_string[i, 4] != null) && (Convert.ToDouble(isNumber_string[i, 5]) < 1.33))//有值且cpk<1.33的畫圖
                {
                    //Text to Number
                    MtbProj.ExecuteCommand("Numeric C" + isNumber_string[i, 0] + " C" + isNumber_string[i, 0] + "." + "FNumeric C" + isNumber_string[i, 0] + ";Auto.");
                    Graph_count = Graph_count + 2;//Text to  Number執行了兩個指令，圖形存檔需標記第幾個指令
                    //時序圖                
                    MtbProj.ExecuteCommand("TSPlot C" + isNumber_string[i, 0] + ";TITLE \"" + isNumber_string[i, 1] + "\";AXLABEL 1 \"Test times\";AXLABEL 2 \"Test data\";ANGLE 0;REFERENCE 2 " + isNumber_string[i, 2] + " " + isNumber_string[i, 3] + ";Overlay;Symbol;Connect.");
                    Mtb.Graph MtbGraph = MtbProj.Commands.Item(Graph_count).Outputs.Item(1).Graph;
                    MtbGraph.SaveAs(sResultPath + "\\" + sLogNamePath + "\\" + isNumber_string[i, 1].Replace(".", ""), true, Mtb.MtbGraphFileTypes.GFPNGHighColor);
                    Graph_count++;

                    //CPK圖                    
                    if (isNumber_string[i, 4] != "0" && (isNumber_string[i, 3] != isNumber_string[i, 2]))//數值不是全部相同&上下限不相同
                    {
                        MtbProj.ExecuteCommand("Capa C" + isNumber_string[i, 0] + " 1;Lspec " + isNumber_string[i, 2] + ";Uspec " + isNumber_string[i, 3] + ";Pooled;AMR;UnBiased;OBiased;Toler 6;Within;Title \"" + isNumber_string[i, 1] + "\";Overall;NoCI;PPM;CStat.");
                        MtbGraph = MtbProj.Commands.Item(Graph_count).Outputs.Item(1).Graph;
                        if (Convert.ToDouble(isNumber_string[i, 5]) < 1.33)//cpk<1.33名稱冠上NG
                        {
                            MtbGraph.SaveAs(sResultPath + "\\" + sLogNamePath + "\\" + "!!!CPK lower than 1_33!!!_(" + isNumber_string[i, 5].Replace(".", "_") + ")" + isNumber_string[i, 1].Replace(".", "") + "_CPK", true, Mtb.MtbGraphFileTypes.GFPNGHighColor);
                        }
                        else
                        {
                            MtbGraph.SaveAs(sResultPath + "\\" + sLogNamePath + "\\" + isNumber_string[i, 1].Replace(".", "") + "_CPK", true, Mtb.MtbGraphFileTypes.GFPNGHighColor);
                        }
                        Graph_count++;
                    }
                }
            }
            MtbProj.SaveAs(sResultPath + "\\" + sLogNamePath + "\\" + sLogNamePath + ".MPJ", true);
            MtbApp.Quit();
            ProcessKill("Mtb.exe");
        }
        private void Minitab_GRR(int X_axis, int Y_axis)
        {
            try
            {
                object misValue = System.Reflection.Missing.Value;
                //Excel_WS1_GRR = ((Excel.Worksheet)Excel_WB1_GRR.Worksheets[1]);
                Excel_WS1_GRR = Excel_WB1_GRR.Worksheets[1];
                Excel_APP_GRR.Visible = false;
                bool bGRR_data_valid = true;
                //Excel.Range Range_GRR = Excel_WS1_GRR.Range["A1"];
                Excel_WS1_GRR.Cells[2, 5] = _sSPEC_USL[X_axis - 2];//USL
                Excel_WS1_GRR.Cells[2, 6] = _sSPEC_LSL[X_axis - 2];//LSL
                int iValue_count = 0;
                for (int i = 1; i <= 90; i++)
                {
                    //if (value[i - 1, X_axis - 2] == null)
                    //{
                    //    bGRR_data_valid = false;
                    //    break;
                    //}
                    if (value[iValue_count, X_axis - 2] != "" && value[iValue_count, X_axis - 2] != null)
                    {
                        Excel_WS1_GRR.Cells[i + 1, 3] = value[iValue_count, X_axis - 2];
                    }
                    else
                    {
                        i--;
                    }
                    iValue_count++;
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
        private void Minitab_GRR_LAWI(int X_axis, int Y_axis)
        {
            try
            {
                object misValue = System.Reflection.Missing.Value;
                //Excel_WS1_GRR = ((Excel.Worksheet)Excel_WB1_GRR.Worksheets[1]);
                Excel_WS1_GRR = Excel_WB1_GRR.Worksheets[1];
                //Excel_WS1_GRR_LAWI = Excel_WB1_GRR_LAWI.Worksheets[1];
                Excel_APP_GRR.Visible = true;
                bool bGRR_data_valid = true;
                //Excel.Range Range_GRR = Excel_WS1_GRR.Range["A1"];
                Excel_WS1_GRR.Cells[2, 5] = Excel_WS1_GRR_LAWI.Cells[2, Y_axis];//USL
                Excel_WS1_GRR.Cells[2, 6] = Excel_WS1_GRR_LAWI.Cells[3, Y_axis];//LSL
                int iValue_count = 0;

                for (int i = 1; i <= 90; i++)
                {
                    Excel_WS1_GRR.Cells[i + 1, 3] = Excel_WS1_GRR_LAWI.Cells[i + 8, Y_axis];
                    //if (Excel_WS1_GRR_LAWI.Cells[i + 4, Y_axis] != "" && Excel_WS1_GRR_LAWI.Cells[i + 4, Y_axis] != null)
                    //{
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
                Excel_WS1_GRR = ((Excel.Worksheet)Excel_WB1_GRR.Worksheets[1]);
                for (int j = 1; j <= 90; j++)
                {
                    Excel_WS1_GRR.Cells[j + 1, 4] = Excel_WS1_GRR.Cells[j + 1, 8];
                    Excel_WS1_GRR.Cells[j + 1, 3] = Excel_WS1_GRR.Cells[j + 1, 4];
                }
                Excel_WS1_GRR = ((Excel.Worksheet)Excel_WB1_GRR.Worksheets[6]);
                if (bGRR_data_valid)
                {
                    sGRR_Tolerance_MA2 = Excel_WS1_GRR.Cells[2, 5].Value.ToString();
                }
                else
                {
                    sGRR_Tolerance_MA2 = "N/A(資料不足90筆)";
                }
                //------------------
                Excel_WS1_GRR = ((Excel.Worksheet)Excel_WB1_GRR.Worksheets[1]);
                for (int k = 1; k <= 90; k++)
                {
                    Excel_WS1_GRR.Cells[k + 1, 5] = Excel_WS1_GRR.Cells[k + 1, 9];
                    Excel_WS1_GRR.Cells[k + 1, 3] = Excel_WS1_GRR.Cells[k + 1, 5];
                }
                Excel_WS1_GRR = ((Excel.Worksheet)Excel_WB1_GRR.Worksheets[6]);
                if (bGRR_data_valid)
                {
                    sGRR_Tolerance_MA3 = Excel_WS1_GRR.Cells[2, 5].Value.ToString();
                }
                else
                {
                    sGRR_Tolerance_MA3 = "N/A(資料不足90筆)";
                }
                //------------------
                Excel_WS1_GRR = ((Excel.Worksheet)Excel_WB1_GRR.Worksheets[1]);
                for (int j = 1; j <= 90; j++)
                {
                    Excel_WS1_GRR.Cells[j + 1, 6] = Excel_WS1_GRR.Cells[j + 1, 10];
                    Excel_WS1_GRR.Cells[j + 1, 3] = Excel_WS1_GRR.Cells[j + 1, 6];
                }
                Excel_WS1_GRR = ((Excel.Worksheet)Excel_WB1_GRR.Worksheets[6]);
                if (bGRR_data_valid)
                {
                    sGRR_Tolerance_MA4 = Excel_WS1_GRR.Cells[2, 5].Value.ToString();
                }
                else
                {
                    sGRR_Tolerance_MA4 = "N/A(資料不足90筆)";
                }
                //------------------
                Excel_WS1_GRR = ((Excel.Worksheet)Excel_WB1_GRR.Worksheets[1]);
                for (int j = 1; j <= 90; j++)
                {
                    Excel_WS1_GRR.Cells[j + 1, 7] = Excel_WS1_GRR.Cells[j + 1, 11];
                    Excel_WS1_GRR.Cells[j + 1, 3] = Excel_WS1_GRR.Cells[j + 1, 7];
                }
                Excel_WS1_GRR = ((Excel.Worksheet)Excel_WB1_GRR.Worksheets[6]);
                if (bGRR_data_valid)
                {
                    sGRR_Tolerance_MA5 = Excel_WS1_GRR.Cells[2, 5].Value.ToString();
                }
                else
                {
                    sGRR_Tolerance_MA5 = "N/A(資料不足90筆)";
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
        private void ProcessKill_AskBeforeKill(string ProcessName)
        {
            try
            {
                Process[] processes = Process.GetProcessesByName(ProcessName.Replace(".exe", ""));
                if (processes.Length > 0)
                {
                    MessageBox.Show("即將刪除所有\"" + ProcessName + "\"程序，請確認檔案都已儲存並在確認後點擊確定");
                    using (Process P = new Process())
                    {
                        P.StartInfo = new ProcessStartInfo()
                        {
                            FileName = "taskkill",
                            CreateNoWindow = true,
                            WindowStyle = ProcessWindowStyle.Hidden,
                            //Arguments = "/F /IM \"" + ProcessName + "\""
                            Arguments = "/F /IM " + ProcessName
                        };
                        P.Start();
                        P.WaitForExit(60000);
                    }
                }
            }
            catch
            {
                using (Process P = new Process())
                {
                    P.StartInfo = new ProcessStartInfo()
                    {
                        FileName = "tskill",
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        Arguments = "\"" + ProcessName + "\" /A /V"
                    };
                    P.Start();
                    P.WaitForExit(60000);
                }
            }
        }
        private void ProcessKill(string ProcessName)
        {
            try
            {
                Process[] processes = Process.GetProcessesByName(ProcessName.Replace(".exe", ""));
                if (processes.Length > 0)
                {
                    using (Process P = new Process())
                    {
                        P.StartInfo = new ProcessStartInfo()
                        {
                            FileName = "taskkill",
                            CreateNoWindow = true,
                            WindowStyle = ProcessWindowStyle.Hidden,
                            //Arguments = "/F /IM \"" + ProcessName + "\""
                            Arguments = "/F /IM " + ProcessName
                        };
                        P.Start();
                        P.WaitForExit(60000);
                    }
                }
            }
            catch
            {
                using (Process P = new Process())
                {
                    P.StartInfo = new ProcessStartInfo()
                    {
                        FileName = "tskill",
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        Arguments = "\"" + ProcessName + "\" /A /V"
                    };
                    P.Start();
                    P.WaitForExit(60000);
                }
            }
        }
        public double StDev(double[] arrData) //計算標準偏差
        {
            double xSum = 0F;
            double xAvg = 0F;
            double sSum = 0F;
            tmpStDev = 0F;
            int arrNum = arrData.Length;
            for (int i = 0; i < arrNum; i++)
            {
                xSum += arrData[i];
            }
            xAvg = xSum / arrNum;
            for (int j = 0; j < arrNum; j++)
            {
                sSum += ((arrData[j] - xAvg) * (arrData[j] - xAvg));
            }
            tmpStDev = Convert.ToSingle(Math.Sqrt((sSum / (arrNum - 1))).ToString());
            return tmpStDev;
        }
        private void SpilitTime(string sReadLine, ref string sTime)
        {
            string[] _sTimeTemp = new string[1000];
            DateTime dtDate;
            _sTimeTemp = sTemp.Split(' ');
            _sTimeTemp = _sTimeTemp[0].Split('-');
            _sTimeTemp[1] = _sTimeTemp[1].Insert(4, ":");
            _sTimeTemp[1] = _sTimeTemp[1].Insert(2, ":");
            if (DateTime.TryParse(_sTimeTemp[1], out dtDate))
            {
                sTime = _sTimeTemp[1];
            }
            else
            {
                sTime = "";
            }
        }
        private void ParserData()
        {
            try
            {
                iTestTimes = Convert.ToInt32(WNC.API.Func.ReadINI("setting", "setting", "TestTimes", "1000"));
                //iEndKeyCount = Convert.ToInt32(WNC.API.Func.ReadINI("setting", "setting", "EndKeyCount", "1"));
                if (!Directory.Exists(sLogPath + "\\" + sLogNamePath))
                {
                    Directory.CreateDirectory(sLogPath + "\\" + sLogNamePath);
                }
                if (!Directory.Exists(sResultPath + "\\" + sLogNamePath))
                {
                    Directory.CreateDirectory(sResultPath + "\\" + sLogNamePath);
                }
                FileName = Directory.GetFiles(sLogPath + "\\" + sLogNamePath);//讀取文件檔案
                text = "";
                sTemp = "";
                FileStream fsFile = null;
                StreamReader srReader = null;
                this.Invoke(new UpdateLableHandler(ProgressBarSize), new object[] { text });//
                int iKeyCount = 0;
                int rows = 0;
                int iItemMAX = 10000;
                //string sAutoGetKey = WNC.API.Func.ReadINI("setting", "setting", "KeyWord2", "1000");
                string[] _sKeyTemp = new string[iItemMAX];
                string[] _sKeyTemp2 = new string[iItemMAX];
                string[] _sSPEC_Temp = new string[iItemMAX];
                string[] _sSPEC_USL_Temp = new string[iItemMAX];
                string[] _sSPEC_LSL_Temp = new string[iItemMAX];

                //string[] sEnd_key = new string[iEndKeyCount];

                //for (int iCount = 1; iCount <= iEndKeyCount; iCount++)
                //{
                //    sEnd_key[iCount - 1] = WNC.API.Func.ReadINI("setting", "setting", "End_key" + iCount, "End");
                //}    

                #region AnalyzeTestItem_SPEC
                bool bReadEnd = false;
                ArrayList Key_manual_aArr = new ArrayList();
                if (sAutoGetTestItem == "Manual")
                {
                    int iCount = 1;
                    string Key_manual = "";
                    while (WNC.API.Func.ReadINI("setting", "setting", "Key" + iCount, "") != "")
                    {
                        Key_manual = WNC.API.Func.ReadINI("setting", "setting", "Key" + iCount, "");
                        Key_manual = Key_manual.Replace(':', '_');
                        Key_manual = Key_manual.Replace("SPEC_", "SPEC:").Replace("Log _", "Log :").Replace("Value_", "Value:").Replace("[ Log ]  ", "").Replace("[SUI]   ", "");
                        Key_manual_aArr.Add(Key_manual);
                        iCount++;
                    }
                }
                foreach (string FN in FileName)
                {
                    fsFile = File.Open(FN, FileMode.Open, FileAccess.Read, FileShare.None);
                    srReader = new StreamReader(fsFile);
                    while (!srReader.EndOfStream)
                    {
                        sTemp = srReader.ReadLine();
                        string sTempUpper = sTemp.ToUpper();
                        if (sTemp.Contains(" SPEC:") && !sTempUpper.Contains("LABEL") && !sTempUpper.Contains("COUNT:"))
                        {
                            if (sTempUpper.Contains("COUNT:"))
                            {

                            }
                            sTemp = sTemp.Replace(':', '_');
                            sTemp = sTemp.Replace("SPEC_", "SPEC:").Replace("Log _", "Log :").Replace("Value_", "Value:").Replace("[ Log ]  ", "").Replace("[SUI]   ", "");
                            _sKeyTemp = sTemp.Split(' ');
                            for (int iSPEC_Count = 0; iSPEC_Count <= _sKeyTemp.Length - 1; iSPEC_Count++)
                            {
                                if (_sKeyTemp[iSPEC_Count].Contains("SPEC"))
                                {
                                    _sSPEC_Temp = _sKeyTemp[iSPEC_Count].Split(':');
                                    _sSPEC_Temp = _sSPEC_Temp[1].Split('~');
                                    break;
                                }
                            }
                            if (sTemp.Contains("Value:"))
                            {
                                //針對一行的格式進行處理，EX : 20211111-021323.142 Volt1 TP 5GS 2GFEM SPEC:4.75~5.25 Value:4.9928348136315
                                for (int iItemCount = 2; iItemCount < _sKeyTemp.Length - 2; iItemCount++)//將測項名稱包含空白的重新串起來
                                {
                                    _sKeyTemp[1] = _sKeyTemp[1] + " " + _sKeyTemp[iItemCount];
                                }
                            }
                            else
                            {
                                //針對兩行格式進行處理
                                //EX :
                                //20210512-094430.550 BAT AI TP 3V8 SPEC:3.46~3.9
                                //20210512-094430.559 BAT AI TP 3V8 Value:3.7802
                                for (int iItemCount = 2; iItemCount < _sKeyTemp.Length - 1; iItemCount++)//將測項名稱包含空白的重新串起來
                                {
                                    _sKeyTemp[1] = _sKeyTemp[1] + " " + _sKeyTemp[iItemCount];
                                }
                            }
                            if (sAutoGetTestItem == "Manual")
                            {
                                if (Key_manual_aArr.Contains(_sKeyTemp[1]))
                                {
                                    for (int iItemCount = 0; iItemCount < iItemMAX; iItemCount++)
                                    {
                                        if (_sKeyTemp2[iItemCount] == _sKeyTemp[1])
                                        {
                                            break;
                                        }
                                        else if (((_sKeyTemp2[iItemCount] == "") || (_sKeyTemp2[iItemCount] == null)))
                                        {
                                            _sKeyTemp2[iKeyCount] = _sKeyTemp[1];
                                            _sSPEC_LSL_Temp[iKeyCount] = _sSPEC_Temp[0];
                                            _sSPEC_USL_Temp[iKeyCount] = _sSPEC_Temp[_sSPEC_Temp.Length - 1];
                                            iKeyCount++;
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                for (int iItemCount = 0; iItemCount < iItemMAX; iItemCount++)
                                {
                                    if (_sKeyTemp2[iItemCount] == _sKeyTemp[1])
                                    {
                                        break;
                                    }
                                    else if (((_sKeyTemp2[iItemCount] == "") || (_sKeyTemp2[iItemCount] == null)))
                                    {
                                        _sKeyTemp2[iKeyCount] = _sKeyTemp[1];
                                        _sSPEC_LSL_Temp[iKeyCount] = _sSPEC_Temp[0];
                                        _sSPEC_USL_Temp[iKeyCount] = _sSPEC_Temp[_sSPEC_Temp.Length - 1];
                                        iKeyCount++;
                                        break;
                                    }
                                }
                            }
                        }
                        //for (int iCount = 1; iCount <= iEndKeyCount; iCount++)
                        //{
                        //    if (sTemp.Contains(sEnd_key[iCount - 1]))
                        //    {
                        //        bReadEnd = true;
                        //        break;
                        //    }
                        //}
                        if (sTemp.Contains(sEndKey))
                        {
                            bReadEnd = true;
                            break;
                        }
                    }
                    if (bReadEnd)
                    {
                        srReader.Close();
                        fsFile.Close();
                        for (int iItemCount = 0; iItemCount < iItemMAX; iItemCount++)
                        {
                            if (_sKeyTemp2[iItemCount] == null)
                            {
                                _sKey = new string[iItemCount];
                                _sSPEC_LSL = new string[iItemCount];
                                _sSPEC_USL = new string[iItemCount];
                                for (int iItemCount1 = 0; iItemCount1 < iItemCount; iItemCount1++)
                                {
                                    _sKey[iItemCount1] = _sKeyTemp2[iItemCount1];
                                    _sSPEC_USL[iItemCount1] = _sSPEC_USL_Temp[iItemCount1];
                                    _sSPEC_LSL[iItemCount1] = _sSPEC_LSL_Temp[iItemCount1];
                                }
                                break;
                            }
                        }
                        break;
                    }
                    if (!bReadEnd)
                    {
                        MessageBox.Show("未讀到End key，請確認...");
                        srReader.Close();
                        fsFile.Close();
                        bTestEnd = true;
                        this.Invoke(new UpdateLableHandler(printResult), new object[] { text });
                        return;
                    }
                }
                #endregion

                #region AnalyzeTestData
                sTemp = "";
                value = new string[iTestTimes, _sKey.Length + 1];
                iTest_times = 0;
                sTestItem = new string[_sKey.Length];
                int iType = 1;
                bool bEndKey = false;
                foreach (string FN in FileName)
                {
                    fsFile = File.Open(FN, FileMode.Open, FileAccess.Read, FileShare.None);
                    srReader = new StreamReader(fsFile);
                    bEndKey = false;
                    value[rows, _sKey.Length] = FN.Replace(sLogPath + "\\" + sLogNamePath + "\\", "");
                    while (!srReader.EndOfStream)
                    {
                        iType = 1;//Status UI中SPEC & Value在不同行，用":"進行切割，抓陣列[1]，"20180626-000826.648 ZwaveFreq Value:908.39179"
                        sTemp = srReader.ReadLine();
                        if (sTemp.Contains(" Value") || sTemp.Contains(" Log :"))
                        {
                            sTemp = sTemp.Replace(':', '_');
                            sTemp = sTemp.Replace("SPEC_", "SPEC:").Replace("Log _", "Log:").Replace("Value_", "Value:").Replace("[ Log ]  ", "").Replace("[SUI]   ", "");
                            _sKeyTemp = sTemp.Split(' ');
                            if (sTemp.Contains("SPEC"))
                            {
                                for (int iItemCount = 2; iItemCount < _sKeyTemp.Length - 2; iItemCount++)//將測項名稱包含空白的重新串起來
                                {
                                    _sKeyTemp[1] = _sKeyTemp[1] + " " + _sKeyTemp[iItemCount];
                                }
                            }
                            else
                            {
                                for (int iItemCount = 2; iItemCount < _sKeyTemp.Length - 1; iItemCount++)//將測項名稱包含空白的重新串起來
                                {
                                    _sKeyTemp[1] = _sKeyTemp[1] + " " + _sKeyTemp[iItemCount];

                                }
                            }
                            if (sTemp.Contains("SPEC:"))//針對Status UI中SPEC & Value在同一行進行分類，用":"進行切割，抓陣列[2]，"20180626-000826.648 ZwaveFreq SPEC:908.3881908~908.4118092 Value:908.39179"
                            {
                                iType = 2;
                            }
                            for (int j = 0; j < _sKey.Length; j++)
                            {
                                for (int x = 0; x < _sKeyTemp.Length; x++)
                                {
                                    if (_sKeyTemp[x].Equals(_sKey[j]))
                                    {
                                        string[] sSpilt = sTemp.Split(':');
                                        value[rows, j] = sSpilt[iType].Trim();
                                    }
                                }
                            }
                        }
                        //for (int iCount = 1; iCount <= iEndKeyCount; iCount++)
                        //{
                        //    if (sTemp.Contains(sEnd_key[iCount - 1]))
                        //    {
                        //        rows++;
                        //        this.Invoke(new UpdateLableHandler(printResult), new object[] { text });
                        //        bEndKey = true;
                        //    }
                        //}
                        if (sTemp.Contains(sEndKey))
                        {
                            rows++;
                            this.Invoke(new UpdateLableHandler(printResult), new object[] { text });
                            bEndKey = true;
                        }
                        if (srReader.EndOfStream && !bEndKey)
                        {
                            MessageBox.Show("檔案不包含End key請確認:" + FN);
                            srReader.Close();
                            fsFile.Close();
                            bTestEnd = true;
                            this.Invoke(new UpdateLableHandler(printResult), new object[] { text });
                            return;
                        }
                    }
                    iTest_times = rows;
                    srReader.Close();
                    fsFile.Close();
                }
                if ((rows != FileName.Length) && (rows < FileName.Length))
                {
                    MessageBox.Show("檔案數目錯誤，請確認EndKey是否正確");
                    srReader.Close();
                    fsFile.Close();
                    bTestEnd = true;
                    this.Invoke(new UpdateLableHandler(printResult), new object[] { text });
                    return;
                }
                #endregion

                #region Minitab auto sketch                
                switch (sAutoDraw)
                {
                    case "Enable(All)":
                        Minitab_sketch_All();
                        break;
                    case "Enable(Only fail)":
                        Minitab_sketch_OnlyFail();
                        break;
                    default:
                        break;
                }
                #endregion

                #region Create result
                switch (sResultFormat)
                {
                    case "csv":
                        WriteToCSV();
                        break;
                    case "xlsx":
                        switch (sRawDataFormat)
                        {
                            case "Horizontal":
                                WriteToExcel_ParserData_horizontal();
                                break;
                            case "Vertical":
                                //ReadExcelFileToDatabase();
                                WriteToExcel_ParserData_vertical();
                                break;
                            default:
                                MessageBox.Show("Setting.ini的Data form項目設定錯誤，請確認填入\"horizontal\" or \"vertical\"");
                                break;
                        }
                        break;
                    default:
                        MessageBox.Show("Setting.ini的Result format項目設定錯誤，請確認填入\"csv\" or \"xlsx\"");
                        break;
                }
                #endregion

                bTestEnd = true;
                this.Invoke(new UpdateLableHandler(printResult), new object[] { text });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw new Exception(ex.Message);
            }
        }
        private void CycleTimeAnalysis()
        {
            try
            {
                if (!Directory.Exists(sLogPath + "\\" + sLogNamePath))
                {
                    Directory.CreateDirectory(sLogPath + "\\" + sLogNamePath);
                }
                if (!Directory.Exists(sResultPath))
                {
                    Directory.CreateDirectory(sResultPath);
                }
                #region 讀取文件檔案
                try
                {
                    FileName = Directory.GetFiles(sLogPath + "\\" + sLogNamePath);
                    //FileName = Directory.GetFiles(sLogPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                #endregion
                this.Invoke(new UpdateLableHandler(ProgressBarSize), new object[] { text });//
                                                                                            //int iEndKeyCount = Convert.ToInt32(WNC.API.Func.ReadINI("setting", "setting", "EndKeyCount", "1"));
                int iEndKeyCount = 1;
                string[] sEnd_key = new string[iEndKeyCount];
                int iKeyCount = 0;
                iTestCount = 0;
                sTemp = "";
                _sTime = new string[1000];
                _sTimeTemp = "";
                _sKey = new string[1000];
                _sKeyTemp = new string[1000];
                Excel_APP = new Excel.Application();
                Excel_WB1 = Excel_APP.Workbooks.Add(true);
                Excel_WS1 = new Excel.Worksheet();
                sEnd_key[0] = "Test Time";
                //for (int iCount = 1; iCount <= iEndKeyCount; iCount++)
                //{
                //    sEnd_key[iCount - 1] = WNC.API.Func.ReadINI("setting", "setting", "End_key" + iCount, "End");
                //}
                #region AnalyzeTestItem_SPEC
                foreach (string FN in FileName)
                {
                    iTestCount++;
                    FileStream fsFile = File.Open(FN, FileMode.Open, FileAccess.Read, FileShare.None);
                    StreamReader srReader = new StreamReader(fsFile);
                    _sTime = new string[1000];
                    _sKey = new string[1000];
                    _sTimeTemp = "";
                    _sKeyTemp = new string[1000];
                    iKeyCount = 0;
                    while (!srReader.EndOfStream)
                    {
                        sTemp = srReader.ReadLine();
                        if (sTemp.Contains("-") && (_sTime[0] == null || _sTime[0] == "") && !sTemp.Contains("/"))
                        {
                            SpilitTime(sTemp, ref _sTimeTemp);
                            _sTime[0] = _sTimeTemp;

                        }
                        if (sTemp.Contains(" SPEC:"))
                        {
                            SpilitTime(sTemp, ref _sTimeTemp);
                            _sKeyTemp = sTemp.Replace("[SUI]   ", "").Split(' ');
                            for (int iItemCount = 2; iItemCount < _sKeyTemp.Length - 1; iItemCount++)//將測項名稱包含空白的重新串起來
                            {
                                if (_sKeyTemp[iItemCount].Contains("SPEC:"))
                                {
                                    break;
                                }
                                _sKeyTemp[1] = _sKeyTemp[1] + " " + _sKeyTemp[iItemCount];
                            }
                            _sKey[iKeyCount] = _sKeyTemp[1];
                            iKeyCount++;
                            _sTime[iKeyCount] = _sTimeTemp;
                        }
                        else if (sTemp.Contains("Test Time:") && sTemp.Contains("Sec"))
                        {
                            SpilitTime(sTemp, ref _sTimeTemp);
                            _sKeyTemp = sTemp.Replace("[SUI]   ", "").Split(' ');
                            _sKeyTemp = _sKeyTemp[2].Split(':');
                            _sKey[iKeyCount] = "Test Time";
                            iKeyCount++;
                            _sTime[iKeyCount] = _sTimeTemp;
                        }
                        for (int iCount = 1; iCount <= iEndKeyCount; iCount++)
                        {
                            if (sTemp.Contains(sEnd_key[iCount - 1]))
                            {
                                _sKey[iKeyCount] = "總測試時間";
                                break;
                            }
                        }
                    }
                    srReader.Close();
                    fsFile.Close();
                    sStation = _netport.getMiddleString(FN, "__", "__");
                    WriteToExcel_CycleTimeAnalysis();
                }
                #endregion
                string ExcelFile = sResultPath + "\\" + sLogNamePath;
                if (File.Exists(ExcelFile + ".xlsx"))
                    File.Delete(ExcelFile + ".xlsx");
                Excel_WB1.SaveAs(ExcelFile, Excel.XlFileFormat.xlOpenXMLWorkbook, System.Reflection.Missing.Value, System.Reflection.Missing.Value, false, false,
        Excel.XlSaveAsAccessMode.xlExclusive, false, false, System.Reflection.Missing.Value, System.Reflection.Missing.Value, System.Reflection.Missing.Value);

                //Excel_WB1.Save();
                //System.Runtime.InteropServices.Marshal.ReleaseComObject(Excel_APP);
                //Excel_WS1 = null;
                Excel_WB1.Close();
                //Excel_WB1 = null;
                Excel_APP.Quit();
                Excel_APP = null;
                bTestEnd = true;
                this.Invoke(new UpdateLableHandler(printResult), new object[] { text });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw new Exception(ex.Message);
            }
        }
        private void TestTimeAnalysis()
        {
            try
            {
                iTestTimes = Convert.ToInt32(WNC.API.Func.ReadINI("setting", "setting", "TestTimes", "1000"));
                //iEndKeyCount = Convert.ToInt32(WNC.API.Func.ReadINI("setting", "setting", "EndKeyCount", "1"));
                if (!Directory.Exists(sLogPath + "\\" + sLogNamePath))
                {
                    Directory.CreateDirectory(sLogPath + "\\" + sLogNamePath);
                }
                if (!Directory.Exists(sResultPath + "\\" + sLogNamePath))
                {
                    Directory.CreateDirectory(sResultPath + "\\" + sLogNamePath);
                }
                FileName = Directory.GetFiles(sLogPath + "\\" + sLogNamePath);//讀取文件檔案
                text = "";
                int iEndKeyCount = 1;
                int rows = 0;
                FileStream fsFile = null;
                StreamReader srReader = null;
                string[] sEnd_key = new string[iEndKeyCount];
                this.Invoke(new UpdateLableHandler(ProgressBarSize), new object[] { text });//
                _sKey = WNC.API.Func.ReadINI("setting", "setting", "KeyWord", "1000").Split(',');
                _sKey2 = WNC.API.Func.ReadINI("setting", "setting", "KeyWord2", "1000").Split(',');

                #region AnalyzeTestData
                sTemp = "";
                value = new string[iTestTimes, _sKey.Length + 2];
                sATS_test_time = new string[iTestTimes];
                iTest_times = 0;
                sTestItem = new string[_sKey.Length];
                bool bEndKey = false;
                foreach (string FN in FileName)
                {
                    fsFile = File.Open(FN, FileMode.Open, FileAccess.Read, FileShare.None);
                    srReader = new StreamReader(fsFile);
                    bEndKey = false;
                    value[rows, _sKey.Length + 1] = FN.Replace(sLogPath + "\\" + sLogNamePath + "\\", "");
                    while (!srReader.EndOfStream)
                    {
                        sTemp = srReader.ReadLine();
                        if (sTemp.Contains("-") && (value[rows, 0] == null || value[rows, 0] == "") && !sTemp.Contains("/"))
                        {
                            SpilitTime(sTemp, ref _sTimeTemp);
                            value[rows, 0] = _sTimeTemp;
                        }
                        for (int j = 0; j < _sKey.Length; j++)
                        {
                            if (sTemp.Contains(_sKey[j]))
                            {
                                SpilitTime(sTemp, ref _sTimeTemp);
                                value[rows, j + 1] = _sTimeTemp;
                            }
                        }
                        if (sTemp.Contains("Test Time:") && sTemp.Contains("Sec"))
                        {
                            _sKeyTemp = sTemp.Replace("[SUI]   ", "").Split(' ');
                            _sKeyTemp = _sKeyTemp[2].Split(':');
                            sATS_test_time[rows] = _sKeyTemp[1];
                        }
                        if (sTemp.Contains(sEndKey))
                        {
                            rows++;
                            this.Invoke(new UpdateLableHandler(printResult), new object[] { text });
                            bEndKey = true;
                        }
                        if (srReader.EndOfStream && !bEndKey)
                        {
                            MessageBox.Show("檔案不包含End key請確認:" + FN);
                            srReader.Close();
                            fsFile.Close();
                            bTestEnd = true;
                            this.Invoke(new UpdateLableHandler(printResult), new object[] { text });
                            return;
                        }
                    }
                    iTest_times = rows;
                    srReader.Close();
                    fsFile.Close();
                }
                if ((rows != FileName.Length) && (rows < FileName.Length))
                {
                    MessageBox.Show("檔案數目錯誤，請確認EndKey是否正確");
                    srReader.Close();
                    fsFile.Close();
                    bTestEnd = true;
                    this.Invoke(new UpdateLableHandler(printResult), new object[] { text });
                    return;
                }
                #endregion

                #region Minitab auto sketch                
                if (sAutoDraw == "Enable")
                {
                    //Minitab_sketch();
                }
                #endregion

                #region Create result
                switch (sResultFormat)
                {
                    case "csv":
                        WriteToCSV();
                        break;
                    case "xlsx":
                        switch (sRawDataFormat)
                        {
                            case "Horizontal":
                                WriteToExcel_TestTimeAnalysis();
                                break;
                            //case "Vertical":
                            //    ReadExcelFileToDatabase();
                            //    WriteToExcel_ParserData_vertical();
                            //    break;
                            default:
                                MessageBox.Show("Setting.ini的Data form項目設定錯誤，請確認填入\"horizontal\" or \"vertical\"");
                                break;
                        }
                        break;
                    default:
                        MessageBox.Show("Setting.ini的Result format項目設定錯誤，請確認填入\"csv\" or \"xlsx\"");
                        break;
                }
                #endregion

                bTestEnd = true;
                this.Invoke(new UpdateLableHandler(printResult), new object[] { text });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw new Exception(ex.Message);
            }
        }
        private void ParserData_WithoutSPEC()
        {
            try
            {
                iTestTimes = Convert.ToInt32(WNC.API.Func.ReadINI("setting", "setting", "TestTimes", "1000"));
                //iEndKeyCount = Convert.ToInt32(WNC.API.Func.ReadINI("setting", "setting", "EndKeyCount", "1"));
                if (!Directory.Exists(sLogPath + "\\" + sLogNamePath))
                {
                    Directory.CreateDirectory(sLogPath + "\\" + sLogNamePath);
                }
                if (!Directory.Exists(sResultPath + "\\" + sLogNamePath))
                {
                    Directory.CreateDirectory(sResultPath + "\\" + sLogNamePath);
                }
                FileName = Directory.GetFiles(sLogPath + "\\" + sLogNamePath);//讀取文件檔案
                text = "";
                sTemp = "";
                FileStream fsFile = null;
                StreamReader srReader = null;
                this.Invoke(new UpdateLableHandler(ProgressBarSize), new object[] { text });//
                int iKeyCount = 0;
                int rows = 0;
                int iItemMAX = 10000;
                //string sAutoGetKey = WNC.API.Func.ReadINI("setting", "setting", "KeyWord2", "1000");
                string[] _sKeyTemp = new string[iItemMAX];
                string[] _sKeyTemp2 = new string[iItemMAX];
                string[] _sSPEC_Temp = new string[iItemMAX];
                string[] _sSPEC_USL_Temp = new string[iItemMAX];
                string[] _sSPEC_LSL_Temp = new string[iItemMAX];

                //string[] sEnd_key = new string[iEndKeyCount];

                //for (int iCount = 1; iCount <= iEndKeyCount; iCount++)
                //{
                //    sEnd_key[iCount - 1] = WNC.API.Func.ReadINI("setting", "setting", "End_key" + iCount, "End");
                //}    

                #region AnalyzeTestItem_SPEC
                bool bReadEnd = false;
                ArrayList Key_manual_aArr = new ArrayList();
                if (sAutoGetTestItem == "Manual")
                {
                    int iCount = 1;
                    string Key_manual = "";
                    while (WNC.API.Func.ReadINI("setting", "setting", "Key" + iCount, "") != "")
                    {
                        Key_manual = WNC.API.Func.ReadINI("setting", "setting", "Key" + iCount, "");
                        Key_manual = Key_manual.Replace(':', '_');
                        Key_manual = Key_manual.Replace("SPEC_", "SPEC:").Replace("Log _", "Log :").Replace("Value_", "Value:").Replace("[ Log ]  ", "").Replace("[SUI]   ", "");
                        Key_manual_aArr.Add(Key_manual);
                        iCount++;
                    }
                }
                foreach (string FN in FileName)
                {
                    fsFile = File.Open(FN, FileMode.Open, FileAccess.Read, FileShare.None);
                    srReader = new StreamReader(fsFile);
                    while (!srReader.EndOfStream)
                    {
                        sTemp = srReader.ReadLine();
                        string sTempUpper = sTemp.ToUpper();
                        if (sTemp.Contains(" SPEC:") && !sTempUpper.Contains("LABEL"))
                        {
                            sTemp = sTemp.Replace(':', '_');
                            sTemp = sTemp.Replace("SPEC_", "SPEC:").Replace("Log _", "Log :").Replace("Value_", "Value:").Replace("[ Log ]  ", "").Replace("[SUI]   ", "");
                            _sKeyTemp = sTemp.Split(' ');
                            for (int iSPEC_Count = 0; iSPEC_Count <= _sKeyTemp.Length - 1; iSPEC_Count++)
                            {
                                if (_sKeyTemp[iSPEC_Count].Contains("SPEC"))
                                {
                                    _sSPEC_Temp = _sKeyTemp[iSPEC_Count].Split(':');
                                    _sSPEC_Temp = _sSPEC_Temp[1].Split('~');
                                    break;
                                }
                            }
                            if (sTemp.Contains("Value:"))
                            {
                                //針對一行的格式進行處理，EX : 20211111-021323.142 Volt1 TP 5GS 2GFEM SPEC:4.75~5.25 Value:4.9928348136315
                                for (int iItemCount = 2; iItemCount < _sKeyTemp.Length - 2; iItemCount++)//將測項名稱包含空白的重新串起來
                                {
                                    _sKeyTemp[1] = _sKeyTemp[1] + " " + _sKeyTemp[iItemCount];
                                }
                            }
                            else
                            {
                                //針對兩行格式進行處理
                                //EX :
                                //20210512-094430.550 BAT AI TP 3V8 SPEC:3.46~3.9
                                //20210512-094430.559 BAT AI TP 3V8 Value:3.7802
                                for (int iItemCount = 2; iItemCount < _sKeyTemp.Length - 1; iItemCount++)//將測項名稱包含空白的重新串起來
                                {
                                    _sKeyTemp[1] = _sKeyTemp[1] + " " + _sKeyTemp[iItemCount];
                                }
                            }
                            if (sAutoGetTestItem == "Manual")
                            {
                                if (Key_manual_aArr.Contains(_sKeyTemp[1]))
                                {
                                    for (int iItemCount = 0; iItemCount < iItemMAX; iItemCount++)
                                    {
                                        if (_sKeyTemp2[iItemCount] == _sKeyTemp[1])
                                        {
                                            break;
                                        }
                                        else if (((_sKeyTemp2[iItemCount] == "") || (_sKeyTemp2[iItemCount] == null)))
                                        {
                                            _sKeyTemp2[iKeyCount] = _sKeyTemp[1];
                                            _sSPEC_LSL_Temp[iKeyCount] = _sSPEC_Temp[0];
                                            _sSPEC_USL_Temp[iKeyCount] = _sSPEC_Temp[_sSPEC_Temp.Length - 1];
                                            iKeyCount++;
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                for (int iItemCount = 0; iItemCount < iItemMAX; iItemCount++)
                                {
                                    if (_sKeyTemp2[iItemCount] == _sKeyTemp[1])
                                    {
                                        break;
                                    }
                                    else if (((_sKeyTemp2[iItemCount] == "") || (_sKeyTemp2[iItemCount] == null)))
                                    {
                                        _sKeyTemp2[iKeyCount] = _sKeyTemp[1];
                                        _sSPEC_LSL_Temp[iKeyCount] = _sSPEC_Temp[0];
                                        _sSPEC_USL_Temp[iKeyCount] = _sSPEC_Temp[_sSPEC_Temp.Length - 1];
                                        iKeyCount++;
                                        break;
                                    }
                                }
                            }
                        }
                        //for (int iCount = 1; iCount <= iEndKeyCount; iCount++)
                        //{
                        //    if (sTemp.Contains(sEnd_key[iCount - 1]))
                        //    {
                        //        bReadEnd = true;
                        //        break;
                        //    }
                        //}
                        if (sTemp.Contains(sEndKey))
                        {
                            bReadEnd = true;
                            break;
                        }
                    }
                    if (bReadEnd)
                    {
                        srReader.Close();
                        fsFile.Close();
                        for (int iItemCount = 0; iItemCount < iItemMAX; iItemCount++)
                        {
                            if (_sKeyTemp2[iItemCount] == null)
                            {
                                _sKey = new string[iItemCount];
                                _sSPEC_LSL = new string[iItemCount];
                                _sSPEC_USL = new string[iItemCount];
                                for (int iItemCount1 = 0; iItemCount1 < iItemCount; iItemCount1++)
                                {
                                    _sKey[iItemCount1] = _sKeyTemp2[iItemCount1];
                                    _sSPEC_USL[iItemCount1] = _sSPEC_USL_Temp[iItemCount1];
                                    _sSPEC_LSL[iItemCount1] = _sSPEC_LSL_Temp[iItemCount1];
                                }
                                break;
                            }
                        }
                        break;
                    }
                    if (!bReadEnd)
                    {
                        MessageBox.Show("未讀到End key，請確認...");
                        srReader.Close();
                        fsFile.Close();
                        bTestEnd = true;
                        this.Invoke(new UpdateLableHandler(printResult), new object[] { text });
                        return;
                    }
                }
                #endregion

                #region AnalyzeTestData
                sTemp = "";
                value = new string[iTestTimes, _sKey.Length + 1];
                iTest_times = 0;
                sTestItem = new string[_sKey.Length];
                int iType = 1;
                bool bEndKey = false;
                foreach (string FN in FileName)
                {
                    fsFile = File.Open(FN, FileMode.Open, FileAccess.Read, FileShare.None);
                    srReader = new StreamReader(fsFile);
                    bEndKey = false;
                    value[rows, _sKey.Length] = FN.Replace(sLogPath + "\\" + sLogNamePath + "\\", "");
                    while (!srReader.EndOfStream)
                    {
                        iType = 1;//Status UI中SPEC & Value在不同行，用":"進行切割，抓陣列[1]，"20180626-000826.648 ZwaveFreq Value:908.39179"
                        sTemp = srReader.ReadLine();
                        if (sTemp.Contains(" Value") || sTemp.Contains(" Log :"))
                        {
                            sTemp = sTemp.Replace(':', '_');
                            sTemp = sTemp.Replace("SPEC_", "SPEC:").Replace("Log _", "Log:").Replace("Value_", "Value:").Replace("[ Log ]  ", "").Replace("[SUI]   ", "");
                            _sKeyTemp = sTemp.Split(' ');
                            if (sTemp.Contains("SPEC"))
                            {
                                for (int iItemCount = 2; iItemCount < _sKeyTemp.Length - 2; iItemCount++)//將測項名稱包含空白的重新串起來
                                {
                                    _sKeyTemp[1] = _sKeyTemp[1] + " " + _sKeyTemp[iItemCount];
                                }
                            }
                            else
                            {
                                for (int iItemCount = 2; iItemCount < _sKeyTemp.Length - 1; iItemCount++)//將測項名稱包含空白的重新串起來
                                {
                                    _sKeyTemp[1] = _sKeyTemp[1] + " " + _sKeyTemp[iItemCount];

                                }
                            }
                            if (sTemp.Contains("SPEC:"))//針對Status UI中SPEC & Value在同一行進行分類，用":"進行切割，抓陣列[2]，"20180626-000826.648 ZwaveFreq SPEC:908.3881908~908.4118092 Value:908.39179"
                            {
                                iType = 2;
                            }
                            for (int j = 0; j < _sKey.Length; j++)
                            {
                                for (int x = 0; x < _sKeyTemp.Length; x++)
                                {
                                    if (_sKeyTemp[x].Equals(_sKey[j]))
                                    {
                                        string[] sSpilt = sTemp.Split(':');
                                        value[rows, j] = sSpilt[iType].Trim();
                                    }
                                }
                            }
                        }
                        //for (int iCount = 1; iCount <= iEndKeyCount; iCount++)
                        //{
                        //    if (sTemp.Contains(sEnd_key[iCount - 1]))
                        //    {
                        //        rows++;
                        //        this.Invoke(new UpdateLableHandler(printResult), new object[] { text });
                        //        bEndKey = true;
                        //    }
                        //}
                        if (sTemp.Contains(sEndKey))
                        {
                            rows++;
                            this.Invoke(new UpdateLableHandler(printResult), new object[] { text });
                            bEndKey = true;
                        }
                        if (srReader.EndOfStream && !bEndKey)
                        {
                            MessageBox.Show("檔案不包含End key請確認:" + FN);
                            srReader.Close();
                            fsFile.Close();
                            bTestEnd = true;
                            this.Invoke(new UpdateLableHandler(printResult), new object[] { text });
                            return;
                        }
                    }
                    iTest_times = rows;
                    srReader.Close();
                    fsFile.Close();
                }
                if ((rows != FileName.Length) && (rows < FileName.Length))
                {
                    MessageBox.Show("檔案數目錯誤，請確認EndKey是否正確");
                    srReader.Close();
                    fsFile.Close();
                    bTestEnd = true;
                    this.Invoke(new UpdateLableHandler(printResult), new object[] { text });
                    return;
                }
                #endregion

                #region Minitab auto sketch                
                if (sAutoDraw == "Enable(All)")
                {
                    Minitab_sketch_All();
                }
                #endregion

                #region Create result
                switch (sResultFormat)
                {
                    case "csv":
                        WriteToCSV();
                        break;
                    case "xlsx":
                        switch (sRawDataFormat)
                        {
                            case "Horizontal":
                                WriteToExcel_ParserData_horizontal();
                                break;
                            //case "Vertical":
                            //    ReadExcelFileToDatabase();
                            //    WriteToExcel_ParserData_vertical();
                            //    break;
                            default:
                                MessageBox.Show("Setting.ini的Data form項目設定錯誤，請確認填入\"horizontal\" or \"vertical\"");
                                break;
                        }
                        break;
                    default:
                        MessageBox.Show("Setting.ini的Result format項目設定錯誤，請確認填入\"csv\" or \"xlsx\"");
                        break;
                }
                #endregion

                bTestEnd = true;
                this.Invoke(new UpdateLableHandler(printResult), new object[] { text });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}