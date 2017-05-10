using MetroFramework;
using MetroFramework.Forms;
using PEPlugin.Pmd;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;
using static PE多功能信息处理插件.Class2;

namespace PE多功能信息处理插件
{
    public partial class JointBezierModeSetAll : MetroForm
    {
        internal TheDataForBezier SetDate1;
        internal TheDataForBezier SetDate2;
        internal TheDataForBezier SetDate3;

        public JointBezierModeSetAll()
        {
            InitializeComponent();
            CanDrag = false;
            index = -1;
            SetStyle(
         ControlStyles.OptimizedDoubleBuffer
         | ControlStyles.ResizeRedraw
         | ControlStyles.Selectable
         | ControlStyles.AllPaintingInWmPaint
         | ControlStyles.UserPaint
         | ControlStyles.SupportsTransparentBackColor,
         true);
            this.Style = (MetroColorStyle)(new Random().Next(3, 13));
            this.metroStyleManager1.Style = this.Style;
            try
            {
                FirstColor.Value = Program.bootstate.BezierFirstColor;
                SecondColor.Value = Program.bootstate.BezierSecondColor;
                LinkSize.Value = (int)Program.bootstate.BezierLinkSize * 20;
            }
            catch (Exception)
            {
                FirstColor.Value = 1;
                SecondColor.Value = 1;
                LinkSize.Value = 20;
            }
        }

        private Point PX1, PX2, PX3, PX4;
        private Point PY1, PY2, PY3, PY4;
        private Point PZ1, PZ2, PZ3, PZ4;
        private int index;

        private void PointOnCubicBezier()
        {
            ThreadPool.QueueUserWorkItem((object state) =>
            {
                PointCubicX();
                PointCubicY();
                PointCubicZ();
            });
            /*      Thread t1 = new Thread(PointCubicX);
                  t1.Start();
                  Thread t2 = new Thread(PointCubicY);
                  t2.Start();
                  Thread t3 = new Thread(PointCubicZ);
                  t3.Start();*/
        }

        private void PointCubicZ()
        {
            var ThePmxOfNow = newopen.GetPmx;
            double ax, bx, cx;
            double ay, by, cy;
            double tSquared, tCubed;
            cx = 3.0 * ((PZ2.X) - (PZ1.X));
            bx = 3.0 * ((PZ3.X) - (PZ2.X)) - cx;
            ax = (PZ4.X) - (PZ1.X) - cx - bx;

            cy = 3.0 * ((-PZ2.Y + 250) - (-PZ1.Y + 250));
            by = 3.0 * ((-PZ3.Y + 250) - (-PZ2.Y + 250)) - cy;
            ay = (-PZ4.Y + 250) - (-PZ1.Y + 250) - cy - by;

            if (UseMode == 0)
            {
                SetDate3.SaveItem = new List<ItemDate>();
                // List<Point> TempReturn = new List<Point>();
                //Dictionary<double, double> pList = new Dictionary<double, double>();
                if (SetDate3.ItemCount.Length > 1)
                {
                    double Long = Math.Pow(Convert.ToDouble(SetDate3.ItemCount.Length - 1), -1d);
                    for (int x = 0; x < SetDate3.ItemCount.Length; x++)
                    {
                        double t = Long * x;
                        tSquared = t * t;
                        tCubed = tSquared * t;
                        int tempnummer = Convert.ToInt16((ay * tCubed) + (by * tSquared) + (cy * t) + (-PZ1.Y + 250));
                        SetDate3.SaveItem.Add(new ItemDate(SetDate3.ItemCount[x],
                            Convert.ToDouble(((((Convert.ToDouble(SetDate3.LastNummerLow) - Convert.ToDouble(SetDate3.FirstNummerLow)) * tempnummer) / 250) + Convert.ToDouble(SetDate3.FirstNummerLow)).ToString("f3")),
                            Convert.ToDouble(((((Convert.ToDouble(SetDate3.LastNummerHigh) - Convert.ToDouble(SetDate3.FirstNummerHigh)) * tempnummer) / 250) + Convert.ToDouble(SetDate3.FirstNummerHigh)).ToString("f3"))));
                    }
                }
                else
                {
                    SetDate3.SaveItem.Add(new ItemDate(SetDate3.ItemCount[0], Convert.ToDouble(SetDate3.FirstNummerLow), Convert.ToDouble(SetDate3.LastNummerHigh)));
                }
            }
            else if (UseMode == 1)
            {
                //List<Point> TempReturn = new List<Point>();
                //Dictionary<double, double> pList = new Dictionary<double, double>();
                if (SetDate3.ListItemCount.Length != 0)
                {
                    SetDate3.SaveItem = new List<ItemDate>();
                    foreach (var Temp in SetDate3.ListItemCount)
                    {
                        SetDate3.ItemCount = Temp.Count.ToArray();
                        if (SetDate3.ItemCount.Length > 1)
                        {
                            double Long = Math.Pow(Convert.ToDouble(SetDate3.ItemCount.Length - 1), -1d);
                            for (int x = 0; x < SetDate3.ItemCount.Length; x++)
                            {
                                double t = Long * x;
                                tSquared = t * t;
                                tCubed = tSquared * t;
                                int tempnummer = Convert.ToInt16((ay * tCubed) + (by * tSquared) + (cy * t) + (-PZ1.Y + 250));
                                SetDate3.SaveItem.Add(new ItemDate(SetDate3.ItemCount[x], Convert.ToDouble(((((Convert.ToDouble(SetDate3.LastNummerLow) - Convert.ToDouble(SetDate3.FirstNummerLow)) * tempnummer) / 250) + Convert.ToDouble(SetDate3.FirstNummerLow)).ToString("f3")), Convert.ToDouble(((((Convert.ToDouble(SetDate3.LastNummerHigh) - Convert.ToDouble(SetDate3.FirstNummerHigh)) * tempnummer) / 250) + Convert.ToDouble(SetDate3.FirstNummerHigh)).ToString("f3"))));
                            }
                        }
                        else
                        {
                            SetDate3.SaveItem.Add(new ItemDate(SetDate3.ItemCount[0], Convert.ToDouble(SetDate3.FirstNummerLow), Convert.ToDouble(SetDate3.LastNummerHigh)));
                        }
                    }
                }
            }
            else if (UseMode == 2)
            {
                //List<Point> TempReturn = new List<Point>();
                //Dictionary<double, double> pList = new Dictionary<double, double>();
                if (SetDate3.ListItemCount.Length > 0)
                {
                    SetDate3.SaveItem = new List<ItemDate>();
                    double Long = Math.Pow(Convert.ToDouble(SetDate3.ListItemCount.Length - 1), -1d);
                    for (int x = 0; x < SetDate3.ListItemCount.Length; x++)
                    {
                        double t = Long * x;
                        tSquared = t * t;
                        tCubed = tSquared * t;
                        int tempnummer = Convert.ToInt16((ay * tCubed) + (by * tSquared) + (cy * t) + (-PZ1.Y + 250));
                        foreach (var temp in SetDate3.ListItemCount[x].Count)
                        {
                            SetDate3.SaveItem.Add(new ItemDate(temp, Convert.ToDouble(((((Convert.ToDouble(SetDate3.LastNummerLow) - Convert.ToDouble(SetDate3.FirstNummerLow)) * tempnummer) / 250) + Convert.ToDouble(SetDate3.FirstNummerLow)).ToString("f3")), Convert.ToDouble(((((Convert.ToDouble(SetDate3.LastNummerHigh) - Convert.ToDouble(SetDate3.FirstNummerHigh)) * tempnummer) / 250) + Convert.ToDouble(SetDate3.FirstNummerHigh)).ToString("f3"))));
                        }
                    }
                }
                else
                {
                    foreach (var temp in SetDate3.ListItemCount[0].Count)
                    {
                        SetDate3.SaveItem.Add(new ItemDate(temp, Convert.ToDouble(SetDate3.FirstNummerLow), Convert.ToDouble(SetDate3.LastNummerHigh)));
                    }
                }
            }
            var table = new DataTable();
            if (LowZ.Checked && HighZ.Checked)
            {
                table.Columns.Add("Joint");
                table.Columns.Add("Z负向");
                table.Columns.Add("Z正向");
                foreach (var dic in SetDate3.SaveItem)
                {
                    table.Rows.Add(dic.Count + ":" + ThePmxOfNow.Joint[dic.Count].Name, dic.FirstNummer, dic.SecendNummer);
                }
            }
            else if (LowZ.Checked && !HighZ.Checked)
            {
                table.Columns.Add("Joint");
                table.Columns.Add("Z负向");
                foreach (var dic in SetDate3.SaveItem)
                {
                    table.Rows.Add(dic.Count + ":" + ThePmxOfNow.Joint[dic.Count].Name, dic.FirstNummer);
                }
            }
            else if (!LowZ.Checked && HighZ.Checked)
            {
                table.Columns.Add("Joint");
                table.Columns.Add("Z正向");
                foreach (var dic in SetDate3.SaveItem)
                {
                    table.Rows.Add(dic.Count + ":" + ThePmxOfNow.Joint[dic.Count].Name, dic.SecendNummer);
                }
            }
            // ShowToWin("GridZ", table);
            Invoke(ShowInfoZ, table);
        }

        private void PointCubicY()
        {
            var ThePmxOfNow = newopen.GetPmx;
            double ax, bx, cx;
            double ay, by, cy;
            double tSquared, tCubed;
            cx = 3.0 * ((PY2.X) - (PY1.X));
            bx = 3.0 * ((PY3.X) - (PY2.X)) - cx;
            ax = (PY4.X) - (PY1.X) - cx - bx;

            cy = 3.0 * ((-PY2.Y + 250) - (-PY1.Y + 250));
            by = 3.0 * ((-PY3.Y + 250) - (-PY2.Y + 250)) - cy;
            ay = (-PY4.Y + 250) - (-PY1.Y + 250) - cy - by;

            if (UseMode == 0)
            {
                SetDate2.SaveItem = new List<ItemDate>();

                //List<Point> TempReturn = new List<Point>();
                //Dictionary<double, double> pList = new Dictionary<double, double>();
                if (SetDate2.ItemCount.Length > 1)
                {
                    double Long = Math.Pow(Convert.ToDouble(SetDate2.ItemCount.Length - 1), -1d);
                    for (int x = 0; x < SetDate2.ItemCount.Length; x++)
                    {
                        double t = Long * x;
                        tSquared = t * t;
                        tCubed = tSquared * t;
                        int tempnummer = Convert.ToInt16((ay * tCubed) + (by * tSquared) + (cy * t) + (-PY1.Y + 250));
                        SetDate2.SaveItem.Add(new ItemDate(SetDate2.ItemCount[x],
                            Convert.ToDouble(((((Convert.ToDouble(SetDate2.LastNummerLow) - Convert.ToDouble(SetDate2.FirstNummerLow)) * tempnummer) / 250) + Convert.ToDouble(SetDate2.FirstNummerLow)).ToString("f3")),
                            Convert.ToDouble(((((Convert.ToDouble(SetDate2.LastNummerHigh) - Convert.ToDouble(SetDate2.FirstNummerHigh)) * tempnummer) / 250) + Convert.ToDouble(SetDate2.FirstNummerHigh)).ToString("f3"))));
                    }
                }
                else
                {
                    SetDate2.SaveItem.Add(new ItemDate(SetDate2.ItemCount[0], Convert.ToDouble(SetDate2.FirstNummerLow), Convert.ToDouble(SetDate2.LastNummerHigh)));
                }
            }
            else if (UseMode == 1)
            {
                //List<Point> TempReturn = new List<Point>();
                // Dictionary<double, double> pList = new Dictionary<double, double>();
                if (SetDate2.ListItemCount.Length != 0)
                {
                    SetDate2.SaveItem = new List<ItemDate>();
                    foreach (var Temp in SetDate2.ListItemCount)
                    {
                        SetDate2.ItemCount = Temp.Count.ToArray();
                        if (SetDate2.ItemCount.Length > 1)
                        {
                            double Long = Math.Pow(Convert.ToDouble(SetDate2.ItemCount.Length - 1), -1d);
                            for (int x = 0; x < SetDate2.ItemCount.Length; x++)
                            {
                                double t = Long * x;
                                tSquared = t * t;
                                tCubed = tSquared * t;
                                int tempnummer = Convert.ToInt16((ay * tCubed) + (by * tSquared) + (cy * t) + (-PY1.Y + 250));
                                SetDate2.SaveItem.Add(new ItemDate(SetDate2.ItemCount[x], Convert.ToDouble(((((Convert.ToDouble(SetDate2.LastNummerLow) - Convert.ToDouble(SetDate2.FirstNummerLow)) * tempnummer) / 250) + Convert.ToDouble(SetDate2.FirstNummerLow)).ToString("f3")), Convert.ToDouble(((((Convert.ToDouble(SetDate2.LastNummerHigh) - Convert.ToDouble(SetDate2.FirstNummerHigh)) * tempnummer) / 250) + Convert.ToDouble(SetDate2.FirstNummerHigh)).ToString("f3"))));
                            }
                        }
                        else
                        {
                            SetDate2.SaveItem.Add(new ItemDate(SetDate2.ItemCount[0], Convert.ToDouble(SetDate2.FirstNummerLow), Convert.ToDouble(SetDate2.LastNummerHigh)));
                        }
                    }
                }
            }
            else if (UseMode == 2)
            {
                // List<Point> TempReturn = new List<Point>();
                //Dictionary<double, double> pList = new Dictionary<double, double>();
                if (SetDate2.ListItemCount.Length > 0)
                {
                    SetDate2.SaveItem = new List<ItemDate>();
                    double Long = Math.Pow(Convert.ToDouble(SetDate2.ListItemCount.Length - 1), -1d);
                    for (int x = 0; x < SetDate2.ListItemCount.Length; x++)
                    {
                        double t = Long * x;
                        tSquared = t * t;
                        tCubed = tSquared * t;
                        int tempnummer = Convert.ToInt16((ay * tCubed) + (by * tSquared) + (cy * t) + (-PY1.Y + 250));
                        foreach (var temp in SetDate2.ListItemCount[x].Count)
                        {
                            SetDate2.SaveItem.Add(new ItemDate(temp, Convert.ToDouble(((((Convert.ToDouble(SetDate2.LastNummerLow) - Convert.ToDouble(SetDate2.FirstNummerLow)) * tempnummer) / 250) + Convert.ToDouble(SetDate2.FirstNummerLow)).ToString("f3")), Convert.ToDouble(((((Convert.ToDouble(SetDate2.LastNummerHigh) - Convert.ToDouble(SetDate2.FirstNummerHigh)) * tempnummer) / 250) + Convert.ToDouble(SetDate2.FirstNummerHigh)).ToString("f3"))));
                        }
                    }
                }
                else
                {
                    foreach (var temp in SetDate2.ListItemCount[0].Count)
                    {
                        SetDate2.SaveItem.Add(new ItemDate(temp, Convert.ToDouble(SetDate2.FirstNummerLow), Convert.ToDouble(SetDate2.LastNummerHigh)));
                    }
                }
            }
            var table = new DataTable();

            if (LowY.Checked && HighY.Checked)
            {
                table.Columns.Add("Joint");
                table.Columns.Add("Y负向");
                table.Columns.Add("Y正向");
                foreach (var dic in SetDate2.SaveItem)
                {
                    table.Rows.Add(dic.Count + ":" + ThePmxOfNow.Joint[dic.Count].Name, dic.FirstNummer, dic.SecendNummer);
                }
            }
            else if (LowY.Checked && !HighY.Checked)
            {
                table.Columns.Add("Joint");
                table.Columns.Add("Y负向");
                foreach (var dic in SetDate2.SaveItem)
                {
                    table.Rows.Add(dic.Count + ":" + ThePmxOfNow.Joint[dic.Count].Name, dic.FirstNummer);
                }
            }
            else if (!LowY.Checked && HighY.Checked)
            {
                table.Columns.Add("Joint");
                table.Columns.Add("Y正向");
                foreach (var dic in SetDate2.SaveItem)
                {
                    table.Rows.Add(dic.Count + ":" + ThePmxOfNow.Joint[dic.Count].Name, dic.SecendNummer);
                }
            }
            // ShowToWin("GridY", table);
            Invoke(ShowInfoY, table);
        }

        private void PointCubicX()
        {
            {
                var ThePmxOfNow = newopen.GetPmx;
                double ax, bx, cx;
                double ay, by, cy;
                double tSquared, tCubed;
                cx = 3.0 * ((PX2.X) - (PX1.X));
                bx = 3.0 * ((PX3.X) - (PX2.X)) - cx;
                ax = (PX4.X) - (PX1.X) - cx - bx;

                cy = 3.0 * ((-PX2.Y + 250) - (-PX1.Y + 250));
                by = 3.0 * ((-PX3.Y + 250) - (-PX2.Y + 250)) - cy;
                ay = (-PX4.Y + 250) - (-PX1.Y + 250) - cy - by;

                if (UseMode == 0)
                {
                    SetDate1.SaveItem = new List<ItemDate>();
                    //List<Point> TempReturn = new List<Point>();
                    //Dictionary<double, double> pList = new Dictionary<double, double>();
                    if (SetDate1.ItemCount.Length > 1)
                    {
                        double Long = Math.Pow(Convert.ToDouble(SetDate1.ItemCount.Length - 1), -1d);
                        for (int x = 0; x < SetDate1.ItemCount.Length; x++)
                        {
                            double t = Long * x;
                            tSquared = t * t;
                            tCubed = tSquared * t;
                            int tempnummer = Convert.ToInt16((ay * tCubed) + (by * tSquared) + (cy * t) + (-PX1.Y + 250));
                            SetDate1.SaveItem.Add(new ItemDate(SetDate1.ItemCount[x],
                                Convert.ToDouble(((((Convert.ToDouble(SetDate1.LastNummerLow) - Convert.ToDouble(SetDate1.FirstNummerLow)) * tempnummer) / 250) + Convert.ToDouble(SetDate1.FirstNummerLow)).ToString("f3")),
                                Convert.ToDouble(((((Convert.ToDouble(SetDate1.LastNummerHigh) - Convert.ToDouble(SetDate1.FirstNummerHigh)) * tempnummer) / 250) + Convert.ToDouble(SetDate1.FirstNummerHigh)).ToString("f3"))));
                        }
                    }
                    else
                    {
                        SetDate1.SaveItem.Add(new ItemDate(SetDate1.ItemCount[0], Convert.ToDouble(SetDate1.FirstNummerLow), Convert.ToDouble(SetDate1.LastNummerHigh)));
                    }
                }
                else if (UseMode == 1)
                {
                    //List<Point> TempReturn = new List<Point>();
                    //Dictionary<double, double> pList = new Dictionary<double, double>();
                    if (SetDate1.ListItemCount.Length != 0)
                    {
                        SetDate1.SaveItem = new List<ItemDate>();
                        foreach (var Temp in SetDate1.ListItemCount)
                        {
                            SetDate1.ItemCount = Temp.Count.ToArray();
                            if (SetDate1.ItemCount.Length > 1)
                            {
                                double Long = Math.Pow(Convert.ToDouble(SetDate1.ItemCount.Length - 1), -1d);
                                for (int x = 0; x < SetDate1.ItemCount.Length; x++)
                                {
                                    double t = Long * x;
                                    tSquared = t * t;
                                    tCubed = tSquared * t;
                                    int tempnummer = Convert.ToInt16((ay * tCubed) + (by * tSquared) + (cy * t) + (-PX1.Y + 250));
                                    SetDate1.SaveItem.Add(new ItemDate(SetDate1.ItemCount[x], Convert.ToDouble(((((Convert.ToDouble(SetDate1.LastNummerLow) - Convert.ToDouble(SetDate1.FirstNummerLow)) * tempnummer) / 250) + Convert.ToDouble(SetDate1.FirstNummerLow)).ToString("f3")), Convert.ToDouble(((((Convert.ToDouble(SetDate1.LastNummerHigh) - Convert.ToDouble(SetDate1.FirstNummerHigh)) * tempnummer) / 250) + Convert.ToDouble(SetDate1.FirstNummerHigh)).ToString("f3"))));
                                }
                            }
                            else
                            {
                                SetDate1.SaveItem.Add(new ItemDate(SetDate1.ItemCount[0], Convert.ToDouble(SetDate1.FirstNummerLow), Convert.ToDouble(SetDate1.LastNummerHigh)));
                            }
                        }
                    }
                }
                else if (UseMode == 2)
                {
                    // List<Point> TempReturn = new List<Point>();
                    //Dictionary<double, double> pList = new Dictionary<double, double>();
                    if (SetDate1.ListItemCount.Length > 0)
                    {
                        SetDate1.SaveItem = new List<ItemDate>();
                        double Long = Math.Pow(Convert.ToDouble(SetDate1.ListItemCount.Length - 1), -1d);
                        for (int x = 0; x < SetDate1.ListItemCount.Length; x++)
                        {
                            double t = Long * x;
                            tSquared = t * t;
                            tCubed = tSquared * t;
                            int tempnummer = Convert.ToInt16((ay * tCubed) + (by * tSquared) + (cy * t) + (-PX1.Y + 250));
                            foreach (var temp in SetDate1.ListItemCount[x].Count)
                            {
                                SetDate1.SaveItem.Add(new ItemDate(temp, Convert.ToDouble(((((Convert.ToDouble(SetDate1.LastNummerLow) - Convert.ToDouble(SetDate1.FirstNummerLow)) * tempnummer) / 250) + Convert.ToDouble(SetDate1.FirstNummerLow)).ToString("f3")), Convert.ToDouble(((((Convert.ToDouble(SetDate1.LastNummerHigh) - Convert.ToDouble(SetDate1.FirstNummerHigh)) * tempnummer) / 250) + Convert.ToDouble(SetDate1.FirstNummerHigh)).ToString("f3"))));
                            }
                        }
                    }
                    else
                    {
                        foreach (var temp in SetDate1.ListItemCount[0].Count)
                        {
                            SetDate1.SaveItem.Add(new ItemDate(temp, Convert.ToDouble(SetDate1.FirstNummerLow), Convert.ToDouble(SetDate1.LastNummerHigh)));
                        }
                    }
                }
                var table = new DataTable();
                if (LowX.Checked && HighX.Checked)
                {
                    table.Columns.Add("Joint");
                    table.Columns.Add("X负向");
                    table.Columns.Add("X正向");
                    foreach (var dic in SetDate1.SaveItem)
                    {
                        table.Rows.Add(dic.Count + ":" + ThePmxOfNow.Joint[dic.Count].Name, dic.FirstNummer, dic.SecendNummer);
                    }
                }
                else if (LowX.Checked && !HighX.Checked)
                {
                    table.Columns.Add("Joint");
                    table.Columns.Add("X负向");
                    foreach (var dic in SetDate1.SaveItem)
                    {
                        table.Rows.Add(dic.Count + ":" + ThePmxOfNow.Joint[dic.Count].Name, dic.FirstNummer);
                    }
                }
                else if (!LowX.Checked && HighX.Checked)
                {
                    table.Columns.Add("Joint");
                    table.Columns.Add("X正向");
                    foreach (var dic in SetDate1.SaveItem)
                    {
                        table.Rows.Add(dic.Count + ":" + ThePmxOfNow.Joint[dic.Count].Name, dic.SecendNummer);
                    }
                }
                // ShowToWin("GridX", table);
                Invoke(ShowInfoX, table);
            }
        }

        public delegate void ShowinfoX(DataTable table);

        public ShowinfoX ShowInfoX;

        public void ShowInfoFunX(DataTable table)
        {
            var Ttable = new DataTable();
            GridX.DataSource = Ttable;
            GridX.DataSource = table;
        }

        public delegate void ShowinfoY(DataTable table);

        public ShowinfoX ShowInfoY;

        public void ShowInfoFunY(DataTable table)
        {
            var Ttable = new DataTable();
            GridY.DataSource = Ttable;
            GridY.DataSource = table;
        }

        public delegate void ShowinfoZ(DataTable table);

        public ShowinfoX ShowInfoZ;

        public void ShowInfoFunZ(DataTable table)
        {
            var Ttable = new DataTable();
            GridZ.DataSource = Ttable;
            GridZ.DataSource = table;
        }

        public delegate void ShowToWinFun(string v, DataTable table);

        private void ShowToWin(string v, DataTable table)
        {
            var Ttable = new DataTable();
            switch (v)
            {
                case "GridX":
                    if (NewForm == "ShowBezier1" || NewForm == "")
                    {
                        BeginInvoke(new MethodInvoker(() =>
                        {
                            GridX.DataSource = table;
                        }));
                        /*  if (GridX.InvokeRequired)
                          {
                              ShowToWinFun d = new ShowToWinFun(ShowToWin);
                              GridX.Invoke(d, v, table);
                          }
                          else
                          {
                              GridX.DataSource = Ttable;
                              GridX.DataSource = table;
                          }*/
                    }
                    break;

                case "GridY":
                    if (NewForm == "ShowBezier2" || NewForm == "")
                    {
                        BeginInvoke(new MethodInvoker(() =>
                        {
                            GridY.DataSource = table;
                        }));
                        /*  if (GridY.InvokeRequired)
                          {
                              ShowToWinFun d = new ShowToWinFun(ShowToWin);
                              GridY.Invoke(d, v, table);
                          }
                          else
                          {
                              GridY.DataSource = Ttable;
                              GridY.DataSource = table;
                          }*/
                    }
                    break;

                case "GridZ":
                    if (NewForm == "ShowBezier3" || NewForm == "")
                    {
                        BeginInvoke(new MethodInvoker(() =>
                        {
                            GridZ.DataSource = table;
                        }));
                        /* if (GridZ.InvokeRequired)
                         {
                             ShowToWinFun d = new ShowToWinFun(ShowToWin);
                             GridZ.Invoke(d, v, table);
                         }
                         else
                         {
                             GridZ.DataSource = Ttable;
                             GridZ.DataSource = table;
                         }*/
                    }
                    break;
            }
        }

        private void ShowBezier_MouseDown(object sender, MouseEventArgs e)
        {
            Point p_mouse = e.Location;
            var NewForm = sender as PictureBox;
            switch (NewForm.Name)
            {
                case "ShowBezier1":
                    CanDrag = false;
                    index = -1;
                    if (Math.Pow(p_mouse.X - PX1.X, 2) + Math.Pow(p_mouse.Y - PX1.Y, 2) <= tol * tol)
                    {
                        CanDrag = true;
                        index = 1;
                    }
                    if (Math.Pow(p_mouse.X - PX2.X, 2) + Math.Pow(p_mouse.Y - PX2.Y, 2) <= tol * tol)
                    {
                        CanDrag = true;
                        index = 2;
                    }
                    if (Math.Pow(p_mouse.X - PX3.X, 2) + Math.Pow(p_mouse.Y - PX3.Y, 2) <= tol * tol)
                    {
                        CanDrag = true;
                        index = 3;
                    }
                    if (Math.Pow(p_mouse.X - PX4.X, 2) + Math.Pow(p_mouse.Y - PX4.Y, 2) <= tol * tol)
                    {
                        CanDrag = true;
                        index = 4;
                    }
                    break;

                case "ShowBezier2":
                    CanDrag = false;
                    index = -1;
                    if (Math.Pow(p_mouse.X - PY1.X, 2) + Math.Pow(p_mouse.Y - PY1.Y, 2) <= tol * tol)
                    {
                        CanDrag = true;
                        index = 1;
                    }
                    if (Math.Pow(p_mouse.X - PY2.X, 2) + Math.Pow(p_mouse.Y - PY2.Y, 2) <= tol * tol)
                    {
                        CanDrag = true;
                        index = 2;
                    }
                    if (Math.Pow(p_mouse.X - PY3.X, 2) + Math.Pow(p_mouse.Y - PY3.Y, 2) <= tol * tol)
                    {
                        CanDrag = true;
                        index = 3;
                    }
                    if (Math.Pow(p_mouse.X - PY4.X, 2) + Math.Pow(p_mouse.Y - PY4.Y, 2) <= tol * tol)
                    {
                        CanDrag = true;
                        index = 4;
                    }
                    break;

                case "ShowBezier3":
                    CanDrag = false;
                    index = -1;
                    if (Math.Pow(p_mouse.X - PZ1.X, 2) + Math.Pow(p_mouse.Y - PZ1.Y, 2) <= tol * tol)
                    {
                        CanDrag = true;
                        index = 1;
                    }
                    if (Math.Pow(p_mouse.X - PZ2.X, 2) + Math.Pow(p_mouse.Y - PZ2.Y, 2) <= tol * tol)
                    {
                        CanDrag = true;
                        index = 2;
                    }
                    if (Math.Pow(p_mouse.X - PZ3.X, 2) + Math.Pow(p_mouse.Y - PZ3.Y, 2) <= tol * tol)
                    {
                        CanDrag = true;
                        index = 3;
                    }
                    if (Math.Pow(p_mouse.X - PZ4.X, 2) + Math.Pow(p_mouse.Y - PZ4.Y, 2) <= tol * tol)
                    {
                        CanDrag = true;
                        index = 4;
                    }
                    break;
            }
        }

        private string NewForm = "";

        private void ShowBezier_MouseMove(object sender, MouseEventArgs e)
        {
            int limt1 = 0;
            int limt2 = 250;
            NewForm = (sender as PictureBox).Name;
            if (CanDrag && e.Button == MouseButtons.Left)
            {
                switch (NewForm)
                {
                    case "ShowBezier1":
                        if (index == 1)
                        {
                            if (e.Location.Y <= limt2 && e.Location.Y >= limt1)
                            {
                                PX1.Y = e.Location.Y;
                            }
                            else if (e.Location.Y >= limt2)
                            {
                                PX1.Y = limt2;
                            }
                            else if (e.Location.Y <= limt1)
                            {
                                PX1.Y = limt1;
                            }
                        }
                        if (index == 2)
                        {
                            if (e.Location.Y <= limt2 && e.Location.Y >= limt1)
                            {
                                PX2.Y = e.Location.Y;
                            }
                            else if (e.Location.Y >= limt2)
                            {
                                PX2.Y = limt2;
                            }
                            else if (e.Location.Y <= limt1)
                            {
                                PX2.Y = limt1;
                            }
                            if (e.Location.X <= limt2 && e.Location.X >= limt1)
                            {
                                PX2.X = e.Location.X;
                            }
                            else if (e.Location.X >= limt2)
                            {
                                PX2.X = limt2;
                            }
                            else if (e.Location.X <= limt1)
                            {
                                PX2.X = limt1;
                            }
                        }
                        if (index == 3)
                        {
                            if (e.Location.Y <= limt2 && e.Location.Y >= limt1)
                            {
                                PX3.Y = e.Location.Y;
                            }
                            else if (e.Location.Y >= limt2)
                            {
                                PX3.Y = limt2;
                            }
                            else if (e.Location.Y <= limt1)
                            {
                                PX3.Y = limt1;
                            }
                            if (e.Location.X <= limt2 && e.Location.X >= limt1)
                            {
                                PX3.X = e.Location.X;
                            }
                            else if (e.Location.X >= limt2)
                            {
                                PX3.X = limt2;
                            }
                            else if (e.Location.X <= limt1)
                            {
                                PX3.X = limt1;
                            }
                        }
                        if (index == 4)
                        {
                            if (e.Location.Y <= limt2 && e.Location.Y >= limt1)
                            {
                                PX4.Y = e.Location.Y;
                            }
                            else if (e.Location.Y >= limt2)
                            {
                                PX4.Y = limt2;
                            }
                            else if (e.Location.Y <= limt1)
                            {
                                PX4.Y = limt1;
                            }
                        }

                        this.ShowBezier1.Invalidate();
                        break;

                    case "ShowBezier2":
                        if (index == 1)
                        {
                            if (e.Location.Y <= limt2 && e.Location.Y >= limt1)
                            {
                                PY1.Y = e.Location.Y;
                            }
                            else if (e.Location.Y >= limt2)
                            {
                                PY1.Y = limt2;
                            }
                            else if (e.Location.Y <= limt1)
                            {
                                PY1.Y = limt1;
                            }
                        }
                        if (index == 2)
                        {
                            if (e.Location.Y <= limt2 && e.Location.Y >= limt1)
                            {
                                PY2.Y = e.Location.Y;
                            }
                            else if (e.Location.Y >= limt2)
                            {
                                PY2.Y = limt2;
                            }
                            else if (e.Location.Y <= limt1)
                            {
                                PY2.Y = limt1;
                            }
                            if (e.Location.X <= limt2 && e.Location.X >= limt1)
                            {
                                PY2.X = e.Location.X;
                            }
                            else if (e.Location.X >= limt2)
                            {
                                PY2.X = limt2;
                            }
                            else if (e.Location.X <= limt1)
                            {
                                PY2.X = limt1;
                            }
                        }
                        if (index == 3)
                        {
                            if (e.Location.Y <= limt2 && e.Location.Y >= limt1)
                            {
                                PY3.Y = e.Location.Y;
                            }
                            else if (e.Location.Y >= limt2)
                            {
                                PY3.Y = limt2;
                            }
                            else if (e.Location.Y <= limt1)
                            {
                                PY3.Y = limt1;
                            }
                            if (e.Location.X <= limt2 && e.Location.X >= limt1)
                            {
                                PY3.X = e.Location.X;
                            }
                            else if (e.Location.X >= limt2)
                            {
                                PY3.X = limt2;
                            }
                            else if (e.Location.X <= limt1)
                            {
                                PY3.X = limt1;
                            }
                        }
                        if (index == 4)
                        {
                            if (e.Location.Y <= limt2 && e.Location.Y >= limt1)
                            {
                                PY4.Y = e.Location.Y;
                            }
                            else if (e.Location.Y >= limt2)
                            {
                                PY4.Y = limt2;
                            }
                            else if (e.Location.Y <= limt1)
                            {
                                PY4.Y = limt1;
                            }
                        }
                        this.ShowBezier2.Invalidate();
                        break;

                    case "ShowBezier3":
                        if (index == 1)
                        {
                            if (e.Location.Y <= limt2 && e.Location.Y >= limt1)
                            {
                                PZ1.Y = e.Location.Y;
                            }
                            else if (e.Location.Y >= limt2)
                            {
                                PZ1.Y = limt2;
                            }
                            else if (e.Location.Y <= limt1)
                            {
                                PZ1.Y = limt1;
                            }
                        }
                        if (index == 2)
                        {
                            if (e.Location.Y <= limt2 && e.Location.Y >= limt1)
                            {
                                PZ2.Y = e.Location.Y;
                            }
                            else if (e.Location.Y >= limt2)
                            {
                                PZ2.Y = limt2;
                            }
                            else if (e.Location.Y <= limt1)
                            {
                                PZ2.Y = limt1;
                            }
                            if (e.Location.X <= limt2 && e.Location.X >= limt1)
                            {
                                PZ2.X = e.Location.X;
                            }
                            else if (e.Location.X >= limt2)
                            {
                                PZ2.X = limt2;
                            }
                            else if (e.Location.X <= limt1)
                            {
                                PZ2.X = limt1;
                            }
                        }
                        if (index == 3)
                        {
                            if (e.Location.Y <= limt2 && e.Location.Y >= limt1)
                            {
                                PZ3.Y = e.Location.Y;
                            }
                            else if (e.Location.Y >= limt2)
                            {
                                PZ3.Y = limt2;
                            }
                            else if (e.Location.Y <= limt1)
                            {
                                PZ3.Y = limt1;
                            }
                            if (e.Location.X <= limt2 && e.Location.X >= limt1)
                            {
                                PZ3.X = e.Location.X;
                            }
                            else if (e.Location.X >= limt2)
                            {
                                PZ3.X = limt2;
                            }
                            else if (e.Location.X <= limt1)
                            {
                                PZ3.X = limt1;
                            }
                        }
                        if (index == 4)
                        {
                            if (e.Location.Y <= limt2 && e.Location.Y >= limt1)
                            {
                                PZ4.Y = e.Location.Y;
                            }
                            else if (e.Location.Y >= limt2)
                            {
                                PZ4.Y = limt2;
                            }
                            else if (e.Location.Y <= limt1)
                            {
                                PZ4.Y = limt1;
                            }
                        }
                        this.ShowBezier3.Invalidate();
                        break;
                }
            }
        }

        private void metroGrid1_Paint(object sender, PaintEventArgs e)
        {
            Rectangle imCurveRect = new Rectangle();
            Graphics g = e.Graphics;
            {
                int x0 = 0;
                int Left = ShowBezier1.Left - 20;
                float Long = (float)255 / (float)(SetDate1.ItemCount.Length == 1 ? 1 : SetDate1.ItemCount.Length - 1);
                for (int i = 0; i < SetDate1.ItemCount.Length; i++)
                {
                    g.DrawLine(new Pen(Color.Black, 2f), new PointF(x0 + i * Long + Left, ShowBezier1.Bottom - 63), new PointF(x0 + i * Long + Left, ShowBezier1.Bottom - 63 + 12));
                }
                if (LowX.Checked)
                {
                    int y0 = 0;
                    int y2 = 250;
                    float unitY = (float)ShowBezier1.Height / 250;
                    double tempnummer = (Convert.ToDouble(SetDate1.LastNummerLow) - Convert.ToDouble(SetDate1.FirstNummerLow)) / 5;
                    for (int i = y0; i <= y2; i++)
                    {
                        if (i % 10 == 0) g.DrawLine(new Pen(Color.Black), new PointF(Left - 5, ShowBezier1.Bottom - 63 - (i - y0) * unitY),
                                             new PointF(Left, ShowBezier1.Bottom - 63 - (i - y0) * unitY)); // ruler line
                        if (i % 50 == 0)
                        {
                            g.DrawLine(new Pen(Color.Black, 2f), new PointF(Left - 10, ShowBezier1.Bottom - 63 - (i - y0) * unitY),
                                     new PointF(Left, ShowBezier1.Bottom - 63 - (i - y0) * unitY)); // ruler line x++;
                        }
                    }
                }
                Left += 265;
                if (HighX.Checked)
                {
                    int y0 = 0;
                    int y2 = 250;
                    float unitY = (float)ShowBezier1.Height / 250;
                    double tempnummer = (Convert.ToDouble(SetDate1.LastNummerHigh) - Convert.ToDouble(SetDate1.FirstNummerHigh)) / 5;
                    for (int i = y0; i <= y2; i++)
                    {
                        if (i % 10 == 0) g.DrawLine(new Pen(Color.Black), new PointF(Left - 10, ShowBezier1.Bottom - 63 - (i - y0) * unitY),
                                             new PointF(Left - 5, ShowBezier1.Bottom - 63 - (i - y0) * unitY)); // ruler line
                        if (i % 50 == 0)
                        {
                            g.DrawLine(new Pen(Color.Black, 2f), new PointF(Left - 10, ShowBezier1.Bottom - 63 - (i - y0) * unitY),
                                     new PointF(Left, ShowBezier1.Bottom - 63 - (i - y0) * unitY)); // ruler line
                        }
                    }
                }
                g.DrawRectangle(new Pen(Color.Black, 2), imCurveRect);
            }

            {
                int x0 = 0;
                int Left = ShowBezier2.Left - 20;
                float Long = 255 / (SetDate2.ItemCount.Length == 1 ? 1 : SetDate2.ItemCount.Length - 1);
                for (int i = 0; i < SetDate2.ItemCount.Length; i++)
                {
                    g.DrawLine(new Pen(Color.Black, 2f), new PointF(x0 + i * Long + Left, ShowBezier2.Bottom - 63), new PointF(x0 + i * Long + Left, ShowBezier2.Bottom - 63 + 12));
                }
                if (LowY.Checked)
                {
                    int y0 = 0;
                    int y2 = 250;
                    float unitY = (float)ShowBezier2.Height / 250;
                    double tempnummer = (Convert.ToDouble(SetDate2.LastNummerLow) - Convert.ToDouble(SetDate2.FirstNummerLow)) / 5;
                    for (int i = y0; i <= y2; i++)
                    {
                        if (i % 10 == 0) g.DrawLine(new Pen(Color.Black), new PointF(Left - 5, ShowBezier2.Bottom - 63 - (i - y0) * unitY),
                                             new PointF(Left, ShowBezier2.Bottom - 63 - (i - y0) * unitY)); // ruler line
                        if (i % 50 == 0)
                        {
                            g.DrawLine(new Pen(Color.Black, 2f), new PointF(Left - 10, ShowBezier2.Bottom - 63 - (i - y0) * unitY),
                                     new PointF(Left, ShowBezier2.Bottom - 63 - (i - y0) * unitY)); // ruler line
                        }
                    }
                }
                Left += 265;
                if (HighY.Checked)
                {
                    int y0 = 0;
                    int y2 = 250;
                    float unitY = (float)ShowBezier2.Height / 250;
                    double tempnummer = (Convert.ToDouble(SetDate2.LastNummerHigh) - Convert.ToDouble(SetDate2.FirstNummerHigh)) / 5;
                    for (int i = y0; i <= y2; i++)
                    {
                        if (i % 10 == 0) g.DrawLine(new Pen(Color.Black), new PointF(Left - 10, ShowBezier2.Bottom - 63 - (i - y0) * unitY),
                                             new PointF(Left - 5, ShowBezier2.Bottom - 63 - (i - y0) * unitY)); // ruler line
                        if (i % 50 == 0)
                        {
                            g.DrawLine(new Pen(Color.Black, 2f), new PointF(Left - 10, ShowBezier2.Bottom - 63 - (i - y0) * unitY),
                                     new PointF(Left, ShowBezier2.Bottom - 63 - (i - y0) * unitY)); // ruler line
                        }
                    }
                }
                g.DrawRectangle(new Pen(Color.Black, 2), imCurveRect);
            }
            {
                int x0 = 0;
                int Left = ShowBezier3.Left - 20;
                float Long = 255 / (SetDate3.ItemCount.Length == 1 ? 1 : SetDate3.ItemCount.Length - 1);
                for (int i = 0; i < SetDate3.ItemCount.Length; i++)
                {
                    g.DrawLine(new Pen(Color.Black, 2f), new PointF(x0 + i * Long + Left, ShowBezier3.Bottom - 63), new PointF(x0 + i * Long + Left, ShowBezier3.Bottom - 63 + 12));
                }
                if (LowZ.Checked)
                {
                    int y0 = 0;
                    int y2 = 250;
                    float unitY = (float)ShowBezier3.Height / 250;
                    double tempnummer = (Convert.ToDouble(SetDate3.LastNummerLow) - Convert.ToDouble(SetDate3.FirstNummerLow)) / 5;
                    for (int i = y0; i <= y2; i++)
                    {
                        if (i % 10 == 0) g.DrawLine(new Pen(Color.Black), new PointF(Left - 5, ShowBezier3.Bottom - 63 - (i - y0) * unitY),
                                             new PointF(Left, ShowBezier3.Bottom - 63 - (i - y0) * unitY)); // ruler line
                        if (i % 50 == 0)
                        {
                            g.DrawLine(new Pen(Color.Black, 2f), new PointF(Left - 10, ShowBezier3.Bottom - 63 - (i - y0) * unitY),
                                     new PointF(Left, ShowBezier3.Bottom - 63 - (i - y0) * unitY)); // ruler line
                        }
                    }
                }
                Left += 265;
                if (HighZ.Checked)
                {
                    int y0 = 0;
                    int y2 = 250;
                    float unitY = (float)ShowBezier3.Height / 250;
                    double tempnummer = (Convert.ToDouble(SetDate3.LastNummerHigh) - Convert.ToDouble(SetDate3.FirstNummerHigh)) / 5;
                    for (int i = y0; i <= y2; i++)
                    {
                        if (i % 10 == 0) g.DrawLine(new Pen(Color.Black), new PointF(Left - 10, ShowBezier3.Bottom - 63 - (i - y0) * unitY),
                                             new PointF(Left - 5, ShowBezier3.Bottom - 63 - (i - y0) * unitY)); // ruler line
                        if (i % 50 == 0)
                        {
                            g.DrawLine(new Pen(Color.Black, 2f), new PointF(Left - 10, ShowBezier3.Bottom - 63 - (i - y0) * unitY),
                                     new PointF(Left, ShowBezier3.Bottom - 63 - (i - y0) * unitY)); // ruler line
                        }
                    }
                }
                g.DrawRectangle(new Pen(Color.Black, 2), imCurveRect);
            }
        }

        private void Form2_Shown(object sender, EventArgs e)
        {
            if (SetDate1 != null)
            {
                XFirstNummer.Text = SetDate1.FirstNummerLow;
                XLastNummer.Text = SetDate1.LastNummerLow;
                XFirstNummerH.Text = SetDate1.FirstNummerHigh;
                XLastNummerH.Text = SetDate1.LastNummerHigh;

                YFirstNummer.Text = SetDate2.FirstNummerLow;
                YLastNummer.Text = SetDate2.LastNummerLow;
                YFirstNummerH.Text = SetDate2.FirstNummerHigh;
                YLastNummerH.Text = SetDate2.LastNummerHigh;

                ZFirstNummer.Text = SetDate3.FirstNummerLow;
                ZLastNummer.Text = SetDate3.LastNummerLow;
                ZFirstNummerH.Text = SetDate3.FirstNummerHigh;
                ZLastNummerH.Text = SetDate3.LastNummerHigh;

                bool findX = false;
                bool findY = false;
                bool findZ = false;
                foreach (var temp in HisForm)
                {
                    if (temp.Key == SetDate1.mode + SetDate1.UseMode)
                    {
                        PX1 = temp.Value.p1;
                        PX2 = temp.Value.p2;
                        PX3 = temp.Value.p3;
                        PX4 = temp.Value.p4;
                        findX = true;
                    }
                    else if (temp.Key == SetDate2.mode + SetDate2.UseMode)
                    {
                        PY1 = temp.Value.p1;
                        PY2 = temp.Value.p2;
                        PY3 = temp.Value.p3;
                        PY4 = temp.Value.p4;
                        findY = true;
                    }
                    else if (temp.Key == SetDate3.mode + SetDate3.UseMode)
                    {
                        PZ1 = temp.Value.p1;
                        PZ2 = temp.Value.p2;
                        PZ3 = temp.Value.p3;
                        PZ4 = temp.Value.p4;
                        findZ = true;
                    }
                }
                if (!findX)
                {
                    PX1 = new Point(0, 250);
                    PX2 = new Point(50, 200);
                    PX3 = new Point(200, 50);
                    PX4 = new Point(250, 0);
                }
                if (!findY)
                {
                    PY1 = new Point(0, 250);
                    PY2 = new Point(50, 200);
                    PY3 = new Point(200, 50);
                    PY4 = new Point(250, 0);
                }
                if (!findZ)
                {
                    PZ1 = new Point(0, 250);
                    PZ2 = new Point(50, 200);
                    PZ3 = new Point(200, 50);
                    PZ4 = new Point(250, 0);
                }
            }
            PointOnCubicBezier();
            ShowInfoX = ShowInfoFunX;
            ShowInfoY = ShowInfoFunY;
            ShowInfoZ = ShowInfoFunZ;
        }

        private void ShowBezier_MouseUp(object sender, MouseEventArgs e)
        {
            PointOnCubicBezier();
            if (metroToggle2.Checked)
            {
                TheFunOfChange(2);
            }
        }

        private void TheFunOfChange(int mode)
        {
            var ThePmxOfNow = newopen.GetPmx;
            switch (SetDate1.mode)
            {
                case "Limit_Move_X":
                    foreach (var temp in SetDate1.SaveItem)
                    {
                        if (LowX.Checked)
                        {
                            ThePmxOfNow.Joint[temp.Count].Limit_MoveLow.X = (float)temp.FirstNummer;
                        }
                        if (HighX.Checked)
                        {
                            ThePmxOfNow.Joint[temp.Count].Limit_MoveHigh.X = (float)temp.SecendNummer;
                        }
                    }
                    break;

                case "Limit_Angle_X":
                    foreach (var temp in SetDate1.SaveItem)
                    {
                        if (LowX.Checked)
                        {
                            ThePmxOfNow.Joint[temp.Count].Limit_AngleLow.X = (float)(((float)temp.FirstNummer * Math.PI) / 180);
                        }
                        if (HighX.Checked)
                        {
                            ThePmxOfNow.Joint[temp.Count].Limit_AngleHigh.X = (float)(((float)temp.SecendNummer * Math.PI) / 180);
                        }
                    }
                    break;
            }
            switch (SetDate2.mode)
            {
                case "Limit_Move_Y":
                    foreach (var temp in SetDate2.SaveItem)
                    {
                        if (LowY.Checked)
                        {
                            ThePmxOfNow.Joint[temp.Count].Limit_MoveLow.Y = (float)temp.FirstNummer;
                        }
                        if (HighY.Checked)
                        {
                            ThePmxOfNow.Joint[temp.Count].Limit_MoveHigh.Y = (float)temp.SecendNummer;
                        }
                    }
                    break;

                case "Limit_Angle_Y":
                    foreach (var temp in SetDate2.SaveItem)
                    {
                        if (LowY.Checked)
                        {
                            ThePmxOfNow.Joint[temp.Count].Limit_AngleLow.Y = (float)(((float)temp.FirstNummer * Math.PI) / 180);
                        }
                        if (HighY.Checked)
                        {
                            ThePmxOfNow.Joint[temp.Count].Limit_AngleHigh.Y = (float)(((float)temp.SecendNummer * Math.PI) / 180);
                        }
                    }
                    break;
            }
            switch (SetDate3.mode)
            {
                case "Limit_Move_Z":
                    foreach (var temp in SetDate3.SaveItem)
                    {
                        if (LowZ.Checked)
                        {
                            ThePmxOfNow.Joint[temp.Count].Limit_MoveLow.Z = (float)temp.FirstNummer;
                        }
                        if (HighZ.Checked)
                        {
                            ThePmxOfNow.Joint[temp.Count].Limit_MoveHigh.Z = (float)temp.SecendNummer;
                        }
                    }
                    break;

                case "Limit_Angle_Z":
                    foreach (var temp in SetDate3.SaveItem)
                    {
                        if (LowZ.Checked)
                        {
                            ThePmxOfNow.Joint[temp.Count].Limit_AngleLow.Z = (float)(((float)temp.FirstNummer * Math.PI) / 180);
                        }
                        if (HighZ.Checked)
                        {
                            ThePmxOfNow.Joint[temp.Count].Limit_AngleHigh.Z = (float)(((float)temp.SecendNummer * Math.PI) / 180);
                        }
                    }
                    break;
            }
            Program.ARGS.Host.Connector.Pmx.Update(ThePmxOfNow);
            Program.ARGS.Host.Connector.Form.UpdateList(UpdateObject.Joint);
            Program.ARGS.Host.Connector.View.PMDView.UpdateModel();
            if (mode == 1)
            {
                Program.ARGS.Host.Connector.View.PmxView.UpdateView();
            }
            else if (mode == 2)
            {
                Program.ARGS.Host.Connector.View.PmxView.UpdateView();
                Program.ARGS.Host.Connector.View.PMDView.UpdateModel();
            }
        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            ListForm.Remove(this);
            foreach (var Temp in HisForm)
            {
                if (Temp.Key == SetDate1.mode + SetDate1.UseMode)
                {
                    HisForm.Remove(Temp.Key);
                    break;
                }
            }
            foreach (var Temp in HisForm)
            {
                if (Temp.Key == SetDate2.mode + SetDate2.UseMode)
                {
                    HisForm.Remove(Temp.Key);
                    break;
                }
            }
            foreach (var Temp in HisForm)
            {
                if (Temp.Key == SetDate3.mode + SetDate3.UseMode)
                {
                    HisForm.Remove(Temp.Key);
                    break;
                }
            }
            HisForm.Add(SetDate1.mode + SetDate1.UseMode, new BezierPoint(PX1, PX2, PX3, PX4));
            HisForm.Add(SetDate2.mode + SetDate2.UseMode, new BezierPoint(PY1, PY2, PY3, PY4));
            HisForm.Add(SetDate3.mode + SetDate3.UseMode, new BezierPoint(PZ1, PZ2, PZ3, PZ4));
        }

        private void metroToggle1_CheckedChanged(object sender, EventArgs e)
        {
            this.TopMost = metroToggle1.Checked ? true : false;
        }

        private float tol = 10;

        private void ShowBezier2_Paint(object sender, PaintEventArgs e)
        {
            Color PenFirst = Color.Blue;
            Color PenSecond = Color.Red;
            float LinkSize = 3;
            if (Program.bootstate.BezierLinkSize != 0)
            {
                LinkSize = Program.bootstate.BezierLinkSize;
            }

            if (Program.bootstate.BezierFirstColor != 0)
            {
                PenFirst = color[Program.bootstate.BezierFirstColor];
                PenSecond = color[Program.bootstate.BezierSecondColor];
            }
            Pen pb = new Pen(PenFirst, LinkSize);
            Pen pc = new Pen(PenSecond, LinkSize - 1);
            SolidBrush sb_on = new SolidBrush(PenSecond);
            SolidBrush sb_text = new SolidBrush(PenFirst);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.DrawBezier(pb, PY1, PY2, PY3, PY4);
            g.DrawLine(pc, PY1, PY2);
            g.DrawLine(pc, PY3, PY4);

            Color First = PenSecond;
            Color Second = PenFirst;
            int Push = (int)LinkSize + 5;
            g.DrawLine(new Pen(First, LinkSize - 1), new Point(PY2.X + Push, PY2.Y), new Point(PY2.X - Push, PY2.Y));
            g.DrawLine(new Pen(First, LinkSize - 1), new Point(PY2.X, PY2.Y + Push), new Point(PY2.X, PY2.Y - Push));
            g.DrawLine(new Pen(First, LinkSize - 1), new Point(PY3.X + Push, PY3.Y), new Point(PY3.X - Push, PY3.Y));
            g.DrawLine(new Pen(First, LinkSize - 1), new Point(PY3.X, PY3.Y + Push), new Point(PY3.X, PY3.Y - Push));
            g.DrawLine(new Pen(Second, LinkSize - 1), new Point(PY1.X + Push, PY1.Y), new Point(PY1.X - 5, PY1.Y));
            g.DrawLine(new Pen(Second, LinkSize - 1), new Point(PY1.X, PY1.Y + Push), new Point(PY1.X, PY1.Y - 5));
            g.DrawLine(new Pen(Second, LinkSize - 1), new Point(PY4.X + Push, PY4.Y), new Point(PY4.X - 5, PY4.Y));
            g.DrawLine(new Pen(Second, LinkSize - 1), new Point(PY4.X, PY4.Y + Push), new Point(PY4.X, PY4.Y - 5));

            if (index == 1)
                g.FillEllipse(sb_on, new RectangleF(PY1.X - tol / 2, PY1.Y - tol / 2, tol, tol));
            if (index == 2)
                g.FillEllipse(sb_on, new RectangleF(PY2.X - tol / 2, PY2.Y - tol / 2, tol, tol));
            if (index == 3)
                g.FillEllipse(sb_on, new RectangleF(PY3.X - tol / 2, PY3.Y - tol / 2, tol, tol));
            if (index == 4)
                g.FillEllipse(sb_on, new RectangleF(PY4.X - tol / 2, PY4.Y - tol / 2, tol, tol));
        }

        private void ShowBezier3_Paint(object sender, PaintEventArgs e)
        {
            Color PenFirst = Color.Blue;
            Color PenSecond = Color.Red;
            float LinkSize = 3;
            if (Program.bootstate.BezierLinkSize != 0)
            {
                LinkSize = Program.bootstate.BezierLinkSize;
            }

            if (Program.bootstate.BezierFirstColor != 0)
            {
                PenFirst = color[Program.bootstate.BezierFirstColor];
                PenSecond = color[Program.bootstate.BezierSecondColor];
            }
            Pen pb = new Pen(PenFirst, LinkSize);
            Pen pc = new Pen(PenSecond, LinkSize - 1);
            SolidBrush sb_on = new SolidBrush(PenSecond);
            SolidBrush sb_text = new SolidBrush(PenFirst);
            Graphics g = e.Graphics;
            if (Program.bootstate.BezierLinkSize != 0)
            {
                LinkSize = Program.bootstate.BezierLinkSize;
            }
            if (Program.bootstate.BezierFirstColor != 0)
            {
                PenFirst = color[Program.bootstate.BezierFirstColor];
                PenSecond = color[Program.bootstate.BezierSecondColor];
            }
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.DrawBezier(pb, PZ1, PZ2, PZ3, PZ4);
            g.DrawLine(pc, PZ1, PZ2);
            g.DrawLine(pc, PZ3, PZ4);
            Color First = PenSecond;
            Color Second = PenFirst;
            int Push = (int)LinkSize + 5;
            g.DrawLine(new Pen(First, LinkSize - 1), new Point(PZ2.X + Push, PZ2.Y), new Point(PZ2.X - Push, PZ2.Y));
            g.DrawLine(new Pen(First, LinkSize - 1), new Point(PZ2.X, PZ2.Y + Push), new Point(PZ2.X, PZ2.Y - Push));
            g.DrawLine(new Pen(First, LinkSize - 1), new Point(PZ3.X + Push, PZ3.Y), new Point(PZ3.X - Push, PZ3.Y));
            g.DrawLine(new Pen(First, LinkSize - 1), new Point(PZ3.X, PZ3.Y + Push), new Point(PZ3.X, PZ3.Y - Push));
            g.DrawLine(new Pen(Second, LinkSize - 1), new Point(PZ1.X + Push, PZ1.Y), new Point(PZ1.X - 5, PZ1.Y));
            g.DrawLine(new Pen(Second, LinkSize - 1), new Point(PZ1.X, PZ1.Y + Push), new Point(PZ1.X, PZ1.Y - 5));
            g.DrawLine(new Pen(Second, LinkSize - 1), new Point(PZ4.X + Push, PZ4.Y), new Point(PZ4.X - 5, PZ4.Y));
            g.DrawLine(new Pen(Second, LinkSize - 1), new Point(PZ4.X, PZ4.Y + Push), new Point(PZ4.X, PZ4.Y - 5));

            if (index == 1)
                g.FillEllipse(sb_on, new RectangleF(PZ1.X - tol / 2, PZ1.Y - tol / 2, tol, tol));
            if (index == 2)
                g.FillEllipse(sb_on, new RectangleF(PZ2.X - tol / 2, PZ2.Y - tol / 2, tol, tol));
            if (index == 3)
                g.FillEllipse(sb_on, new RectangleF(PZ3.X - tol / 2, PZ3.Y - tol / 2, tol, tol));
            if (index == 4)
                g.FillEllipse(sb_on, new RectangleF(PZ4.X - tol / 2, PZ4.Y - tol / 2, tol, tol));
        }

        private void ChangeNummer(object sender, EventArgs e)
        {
            if (XFirstNummer.Text != string.Empty && XLastNummer.Text != string.Empty &&
                XFirstNummerH.Text != string.Empty && XLastNummerH.Text != string.Empty &&
                YFirstNummer.Text != string.Empty && YLastNummer.Text != string.Empty &&
                YFirstNummerH.Text != string.Empty && YLastNummerH.Text != string.Empty &&
                ZFirstNummer.Text != string.Empty && ZLastNummer.Text != string.Empty &&
                ZFirstNummerH.Text != string.Empty && ZLastNummerH.Text != string.Empty)
            {
                if (this.Text == "Joint,移动限制" || this.Text == "Joint,移动限制,块处理模式" || this.Text == "Joint,移动限制,批量模式")
                {
                    Class2.newopen.Limit_MoveLow_FirstXNummer.Text = XFirstNummer.Text;
                    SetDate1.FirstNummerLow = XFirstNummer.Text;
                    Class2.newopen.Limit_MoveLow_LastXNummer.Text = XLastNummer.Text;
                    SetDate1.LastNummerLow = XLastNummer.Text;
                    Class2.newopen.Limit_MoveHigh_FirstXNummer.Text = XFirstNummerH.Text;
                    SetDate1.FirstNummerHigh = XFirstNummerH.Text;
                    Class2.newopen.Limit_MoveHigh_LastXNummer.Text = XLastNummerH.Text;
                    SetDate1.LastNummerHigh = XLastNummerH.Text;
                    Class2.newopen.Limit_MoveLow_FirstYNummer.Text = YFirstNummer.Text;
                    SetDate2.FirstNummerLow = YFirstNummer.Text;
                    Class2.newopen.Limit_MoveLow_LastYNummer.Text = YLastNummer.Text;
                    SetDate2.LastNummerLow = YLastNummer.Text;
                    Class2.newopen.Limit_MoveHigh_FirstYNummer.Text = YFirstNummerH.Text;
                    SetDate2.FirstNummerHigh = YFirstNummerH.Text;
                    Class2.newopen.Limit_MoveHigh_LastYNummer.Text = YLastNummerH.Text;
                    SetDate2.LastNummerHigh = YLastNummerH.Text;
                    Class2.newopen.Limit_MoveLow_FirstZNummer.Text = ZFirstNummer.Text;
                    SetDate3.FirstNummerLow = ZFirstNummer.Text;
                    Class2.newopen.Limit_MoveLow_LastZNummer.Text = ZLastNummer.Text;
                    SetDate3.LastNummerLow = ZLastNummer.Text;
                    Class2.newopen.Limit_MoveHigh_FirstZNummer.Text = ZFirstNummerH.Text;
                    SetDate3.FirstNummerHigh = ZFirstNummerH.Text;
                    Class2.newopen.Limit_MoveHigh_LastZNummer.Text = ZLastNummerH.Text;
                    SetDate3.LastNummerHigh = ZLastNummerH.Text;

                    /*  switch (NewTextBox.Name)
                      {
                          case "XFirstNummer":
                              Class2.newopen.Limit_MoveLow_FirstXNummer.Text = NewTextBox.Text;
                              SetDate1.FirstNummerLow = NewTextBox.Text;
                              break;

                          case "XLastNummer":
                              Class2.newopen.Limit_MoveLow_LastXNummer.Text = NewTextBox.Text;
                              SetDate1.LastNummerLow = NewTextBox.Text;
                              break;

                          case "XFirstNummerH":
                              Class2.newopen.Limit_MoveHigh_FirstXNummer.Text = NewTextBox.Text;
                              SetDate1.FirstNummerHigh = NewTextBox.Text;
                              break;

                          case "XLastNummerH":
                              Class2.newopen.Limit_MoveHigh_LastXNummer.Text = NewTextBox.Text;
                              SetDate1.LastNummerHigh = NewTextBox.Text;
                              break;

                          case "YFirstNummer":
                              Class2.newopen.Limit_MoveLow_FirstYNummer.Text = NewTextBox.Text;
                              SetDate2.FirstNummerLow = NewTextBox.Text;
                              break;

                          case "YLastNummer":
                              Class2.newopen.Limit_MoveLow_LastYNummer.Text = NewTextBox.Text;
                              SetDate2.LastNummerLow = NewTextBox.Text;
                              break;

                          case "YFirstNummerH":
                              Class2.newopen.Limit_MoveHigh_FirstYNummer.Text = NewTextBox.Text;
                              SetDate2.FirstNummerHigh = NewTextBox.Text;
                              break;

                          case "YLastNummerH":
                              Class2.newopen.Limit_MoveHigh_LastYNummer.Text = NewTextBox.Text;
                              SetDate2.LastNummerHigh = NewTextBox.Text;
                              break;

                          case "ZFirstNummer":
                              Class2.newopen.Limit_MoveLow_FirstZNummer.Text = NewTextBox.Text;
                              SetDate3.FirstNummerLow = NewTextBox.Text;
                              break;

                          case "ZLastNummer":
                              Class2.newopen.Limit_MoveLow_LastZNummer.Text = NewTextBox.Text;
                              SetDate3.LastNummerLow = NewTextBox.Text;
                              break;

                          case "ZFirstNummerH":
                              Class2.newopen.Limit_MoveHigh_FirstZNummer.Text = NewTextBox.Text;
                              SetDate3.FirstNummerHigh = NewTextBox.Text;
                              break;

                          case "ZLastNummerH":
                              Class2.newopen.Limit_MoveHigh_LastZNummer.Text = NewTextBox.Text;
                              SetDate3.LastNummerHigh = NewTextBox.Text;
                              break;
                      }*/
                }
                else if (this.Text == "Joint,旋转限制" || this.Text == "Joint,旋转限制,批量模式" || this.Text == "Joint,旋转限制,块处理模式")
                {
                    Class2.newopen.Limit_AngleLow_FirstXNummer.Text = XFirstNummer.Text;
                    SetDate1.FirstNummerLow = XFirstNummer.Text;
                    Class2.newopen.Limit_AngleLow_LastXNummer.Text = XLastNummer.Text;
                    SetDate1.LastNummerLow = XLastNummer.Text;
                    Class2.newopen.Limit_AngleHigh_FirstXNummer.Text = XFirstNummerH.Text;
                    SetDate1.FirstNummerHigh = XFirstNummerH.Text;
                    Class2.newopen.Limit_AngleHigh_LastXNummer.Text = XLastNummerH.Text;
                    SetDate1.LastNummerHigh = XLastNummerH.Text;
                    Class2.newopen.Limit_AngleLow_FirstYNummer.Text = YFirstNummer.Text;
                    SetDate2.FirstNummerLow = YFirstNummer.Text;
                    Class2.newopen.Limit_AngleLow_LastYNummer.Text = YLastNummer.Text;
                    SetDate2.LastNummerLow = YLastNummer.Text;
                    Class2.newopen.Limit_AngleHigh_FirstYNummer.Text = YFirstNummerH.Text;
                    SetDate2.FirstNummerHigh = YFirstNummerH.Text;
                    Class2.newopen.Limit_AngleHigh_LastYNummer.Text = YLastNummerH.Text;
                    SetDate2.LastNummerHigh = YLastNummerH.Text;
                    Class2.newopen.Limit_AngleLow_FirstZNummer.Text = ZFirstNummer.Text;
                    SetDate3.FirstNummerLow = ZFirstNummer.Text;
                    Class2.newopen.Limit_AngleLow_LastZNummer.Text = ZLastNummer.Text;
                    SetDate3.LastNummerLow = ZLastNummer.Text;
                    Class2.newopen.Limit_AngleHigh_FirstZNummer.Text = ZFirstNummerH.Text;
                    SetDate3.FirstNummerHigh = ZFirstNummerH.Text;
                    Class2.newopen.Limit_AngleHigh_LastZNummer.Text = ZLastNummerH.Text;
                    SetDate3.LastNummerHigh = ZLastNummerH.Text;
                    /* case "XFirstNummer":
                         Class2.newopen.Limit_AngleLow_FirstXNummer.Text = NewTextBox.Text;
                         SetDate1.FirstNummerLow = NewTextBox.Text;
                         break;

                     case "XLastNummer":
                         Class2.newopen.Limit_AngleLow_LastXNummer.Text = NewTextBox.Text;
                         SetDate1.LastNummerLow = NewTextBox.Text;
                         break;

                     case "XFirstNummerH":
                         Class2.newopen.Limit_AngleHigh_FirstXNummer.Text = NewTextBox.Text;
                         SetDate1.FirstNummerHigh = NewTextBox.Text;
                         break;

                     case "XLastNummerH":
                         Class2.newopen.Limit_AngleHigh_LastXNummer.Text = NewTextBox.Text;
                         SetDate1.LastNummerHigh = NewTextBox.Text;
                         break;

                     case "YFirstNummer":
                         Class2.newopen.Limit_AngleLow_FirstYNummer.Text = NewTextBox.Text;
                         SetDate2.FirstNummerLow = NewTextBox.Text;
                         break;

                     case "YLastNummer":
                         Class2.newopen.Limit_AngleLow_LastYNummer.Text = NewTextBox.Text;
                         SetDate2.LastNummerLow = NewTextBox.Text;
                         break;

                     case "YFirstNummerH":
                         Class2.newopen.Limit_AngleHigh_FirstYNummer.Text = NewTextBox.Text;
                         SetDate2.FirstNummerHigh = NewTextBox.Text;
                         break;

                     case "YLastNummerH":
                         Class2.newopen.Limit_AngleHigh_LastYNummer.Text = NewTextBox.Text;
                         SetDate2.LastNummerHigh = NewTextBox.Text;
                         break;

                     case "ZFirstNummer":
                         Class2.newopen.Limit_AngleLow_FirstZNummer.Text = NewTextBox.Text;
                         SetDate3.FirstNummerLow = NewTextBox.Text;
                         break;

                     case "ZLastNummer":
                         Class2.newopen.Limit_AngleLow_LastZNummer.Text = NewTextBox.Text;
                         SetDate3.LastNummerLow = NewTextBox.Text;
                         break;

                     case "ZFirstNummerH":
                         Class2.newopen.Limit_AngleHigh_FirstZNummer.Text = NewTextBox.Text;
                         SetDate3.FirstNummerHigh = NewTextBox.Text;
                         break;

                     case "ZLastNummerH":
                         Class2.newopen.Limit_AngleHigh_LastZNummer.Text = NewTextBox.Text;
                         SetDate3.LastNummerHigh = NewTextBox.Text;
                         break;
                 }*/
                }
            }
            NewForm = "";
            PointOnCubicBezier();
            if (metroToggle2.Checked)
            {
                TheFunOfChange(2);
            }
            this.metroGrid1.Invalidate();
        }

        private void Reset(object sender, EventArgs e)
        {
            var NewOpen = sender as MetroFramework.Controls.MetroButton;
            switch (NewOpen.Name)
            {
                case "ResetX":
                    PX1 = new Point(0, 250);
                    PX2 = new Point(50, 200);
                    PX3 = new Point(200, 50);
                    PX4 = new Point(250, 0);
                    break;

                case "ResetY":
                    PY1 = new Point(0, 250);
                    PY2 = new Point(50, 200);
                    PY3 = new Point(200, 50);
                    PY4 = new Point(250, 0);
                    break;

                case "ResetZ":
                    PZ1 = new Point(0, 250);
                    PZ2 = new Point(50, 200);
                    PZ3 = new Point(200, 50);
                    PZ4 = new Point(250, 0);
                    break;
            }
            PointOnCubicBezier();
            this.Refresh();
        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            TheFunOfChange(0);
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            TheFunOfChange(1);
            this.Close();
        }

        private void ValueChange(object sender, EventArgs e)
        {
            MetroFramework.Controls.MetroTrackBar NewTrackBar = sender as MetroFramework.Controls.MetroTrackBar;
            switch (NewTrackBar.Name)
            {
                case "FirstColor":
                    Program.bootstate.BezierFirstColor = NewTrackBar.Value;
                    break;

                case "SecondColor":
                    Program.bootstate.BezierSecondColor = NewTrackBar.Value;
                    break;

                case "LinkSize":
                    Program.bootstate.BezierLinkSize = NewTrackBar.Value / 20;
                    break;
            }
            this.ShowBezier1.Invalidate();
            this.ShowBezier2.Invalidate();
            this.ShowBezier3.Invalidate();
        }

        private void CheckedChanged(object sender, EventArgs e)
        {
            var NewToggle = sender as MetroFramework.Controls.MetroToggle;
            switch (NewToggle.Name)
            {
                case "LowX":
                    if (NewToggle.Checked)
                    {
                        XLabel1.Enabled = true;
                        XLastNummer.Enabled = true;
                        XLabel3.Enabled = true;
                        XFirstNummer.Enabled = true;
                    }
                    else
                    {
                        XLabel1.Enabled = false;
                        XLastNummer.Enabled = false;
                        XLabel3.Enabled = false;
                        XFirstNummer.Enabled = false;
                    }
                    break;

                case "HighX":
                    if (NewToggle.Checked)
                    {
                        XLabel2.Enabled = true;
                        XLastNummerH.Enabled = true;
                        XLabel4.Enabled = true;
                        XFirstNummerH.Enabled = true;
                    }
                    else
                    {
                        XLabel2.Enabled = false;
                        XLastNummerH.Enabled = false;
                        XLabel4.Enabled = false;
                        XFirstNummerH.Enabled = false;
                    }
                    break;

                case "LowY":
                    if (NewToggle.Checked)
                    {
                        YLabel1.Enabled = true;
                        YLastNummer.Enabled = true;
                        YLabel3.Enabled = true;
                        YFirstNummer.Enabled = true;
                    }
                    else
                    {
                        YLabel1.Enabled = false;
                        YLastNummer.Enabled = false;
                        YLabel3.Enabled = false;
                        YFirstNummer.Enabled = false;
                    }
                    break;

                case "HighY":
                    if (NewToggle.Checked)
                    {
                        YLabel2.Enabled = true;
                        YLastNummerH.Enabled = true;
                        YLabel4.Enabled = true;
                        YFirstNummerH.Enabled = true;
                    }
                    else
                    {
                        YLabel2.Enabled = false;
                        YLastNummerH.Enabled = false;
                        YLabel4.Enabled = false;
                        YFirstNummerH.Enabled = false;
                    }
                    break;

                case "LowZ":
                    if (NewToggle.Checked)
                    {
                        ZLabel1.Enabled = true;
                        ZLastNummer.Enabled = true;
                        ZLabel3.Enabled = true;
                        ZFirstNummer.Enabled = true;
                    }
                    else
                    {
                        ZLabel1.Enabled = false;
                        ZLastNummer.Enabled = false;
                        ZLabel3.Enabled = false;
                        ZFirstNummer.Enabled = false;
                    }
                    break;

                case "HighZ":
                    if (NewToggle.Checked)
                    {
                        ZLabel2.Enabled = true;
                        ZLastNummerH.Enabled = true;
                        ZLabel4.Enabled = true;
                        ZFirstNummerH.Enabled = true;
                    }
                    else
                    {
                        ZLabel2.Enabled = false;
                        ZLastNummerH.Enabled = false;
                        ZLabel4.Enabled = false;
                        ZFirstNummerH.Enabled = false;
                    }
                    break;
            }
            this.metroGrid1.Invalidate();
            PointOnCubicBezier();
        }

        private bool CanDrag;
        internal int UseMode;

        private void ShowBezier_Paint(object sender, PaintEventArgs e)
        {
            Color PenFirst = Color.Blue;
            Color PenSecond = Color.Red;
            float LinkSize = 3;
            if (Program.bootstate.BezierLinkSize != 0)
            {
                LinkSize = Program.bootstate.BezierLinkSize;
            }

            if (Program.bootstate.BezierFirstColor != 0)
            {
                PenFirst = color[Program.bootstate.BezierFirstColor];
                PenSecond = color[Program.bootstate.BezierSecondColor];
            }
            Pen pb = new Pen(PenFirst, LinkSize);
            Pen pc = new Pen(PenSecond, LinkSize - 1);
            SolidBrush sb_on = new SolidBrush(PenSecond);
            SolidBrush sb_text = new SolidBrush(PenFirst);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.DrawBezier(pb, PX1, PX2, PX3, PX4);
            g.DrawLine(pc, PX1, PX2);
            g.DrawLine(pc, PX3, PX4);

            Color First = PenSecond;
            Color Second = PenFirst;
            int Push = (int)LinkSize + 5;
            g.DrawLine(new Pen(First, LinkSize - 1), new Point(PX2.X + Push, PX2.Y), new Point(PX2.X - Push, PX2.Y));
            g.DrawLine(new Pen(First, LinkSize - 1), new Point(PX2.X, PX2.Y + Push), new Point(PX2.X, PX2.Y - Push));
            g.DrawLine(new Pen(First, LinkSize - 1), new Point(PX3.X + Push, PX3.Y), new Point(PX3.X - Push, PX3.Y));
            g.DrawLine(new Pen(First, LinkSize - 1), new Point(PX3.X, PX3.Y + Push), new Point(PX3.X, PX3.Y - Push));
            g.DrawLine(new Pen(Second, LinkSize - 1), new Point(PX1.X + Push, PX1.Y), new Point(PX1.X - 5, PX1.Y));
            g.DrawLine(new Pen(Second, LinkSize - 1), new Point(PX1.X, PX1.Y + Push), new Point(PX1.X, PX1.Y - 5));
            g.DrawLine(new Pen(Second, LinkSize - 1), new Point(PX4.X + Push, PX4.Y), new Point(PX4.X - 5, PX4.Y));
            g.DrawLine(new Pen(Second, LinkSize - 1), new Point(PX4.X, PX4.Y + Push), new Point(PX4.X, PX4.Y - 5));

            if (index == 1)
                g.FillEllipse(sb_on, new RectangleF(PX1.X - tol / 2, PX1.Y - tol / 2, tol, tol));
            if (index == 2)
                g.FillEllipse(sb_on, new RectangleF(PX2.X - tol / 2, PX2.Y - tol / 2, tol, tol));
            if (index == 3)
                g.FillEllipse(sb_on, new RectangleF(PX3.X - tol / 2, PX3.Y - tol / 2, tol, tol));
            if (index == 4)
                g.FillEllipse(sb_on, new RectangleF(PX4.X - tol / 2, PX4.Y - tol / 2, tol, tol));
        }

        public void NumCheck(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\r' && e.KeyChar != '\b' && e.KeyChar != '.' && e.KeyChar != '\u0016' && e.KeyChar != '\u0003')//这是允许输入退格键
            {
                if ((e.KeyChar < '0') || (e.KeyChar > '9'))//这是允许输入0-9数字
                {
                    if (e.KeyChar != '-')
                    {
                        e.Handled = true;
                    }
                }
            }
            else if (e.KeyChar == '\r')
            {
                MetroFramework.Controls.MetroTextBox NewTextBox = sender as MetroFramework.Controls.MetroTextBox;
                ChangeNummer(sender, e);
            }
        }
    }
}