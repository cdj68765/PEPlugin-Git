using MetroFramework;
using MetroFramework.Controls;
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
    public partial class BodyBezierMode : MetroForm
    {
        internal TheDataForBezier SetDate;

        public BodyBezierMode()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            p1 = new Point(0, 250);
            p2 = new Point(50, 200);
            p3 = new Point(200, 50);
            p4 = new Point(250, 0);
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
            metroStyleManager1.Style = (MetroColorStyle)(new Random().Next(3, 13));
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

        private Point p1, p2, p3, p4;
        private int index;

        private void PointOnCubicBezier()
        {
            Thread t = new Thread(PointCubic);
            t.Start();
            // Parallel.Invoke(() => { PointCubic(); });
        }

        private void PointCubic()
        {
            try
            {
                double ax, bx, cx;
                double ay, by, cy;
                double tSquared, tCubed;
                cx = 3.0 * ((p2.X) - (p1.X));
                bx = 3.0 * ((p3.X) - (p2.X)) - cx;
                ax = (p4.X) - (p1.X) - cx - bx;

                cy = 3.0 * ((-p2.Y + 250) - (-p1.Y + 250));
                by = 3.0 * ((-p3.Y + 250) - (-p2.Y + 250)) - cy;
                ay = (-p4.Y + 250) - (-p1.Y + 250) - cy - by;

                if (SetDate.UseMode == 0)
                {
                    SetDate.SaveItem = new List<ItemDate>();
                    //List<Point> TempReturn = new List<Point>();
                    //Dictionary<double, double> pList = new Dictionary<double, double>();
                    if (SetDate.ItemCount.Length > 1)
                    {
                        double Long = Math.Pow(Convert.ToDouble(SetDate.ItemCount.Length - 1), -1d);

                        for (int x = 0; x < SetDate.ItemCount.Length; x++)
                        {
                            double t = Long * x;
                            tSquared = t * t;
                            tCubed = tSquared * t;
                            int tempnummer = Convert.ToInt16((ay * tCubed) + (by * tSquared) + (cy * t) + (-p1.Y + 250));
                            SetDate.SaveItem.Add(new ItemDate(SetDate.ItemCount[x], Convert.ToDouble(((((Convert.ToDouble(SetDate.LastNummerLow) - Convert.ToDouble(SetDate.FirstNummerLow)) * tempnummer) / 250) + Convert.ToDouble(SetDate.FirstNummerLow)).ToString("f3"))));
                        }
                    }
                    else
                    {
                        SetDate.SaveItem.Add(new ItemDate(SetDate.ItemCount[0], Convert.ToDouble(SetDate.FirstNummerLow)));
                    }
                }
                else if (SetDate.UseMode == 1)
                {
                    //List<Point> TempReturn = new List<Point>();
                    //Dictionary<double, double> pList = new Dictionary<double, double>();
                    if (SetDate.ListItemCount.Length != 0)
                    {
                        SetDate.SaveItem = new List<ItemDate>();
                        foreach (var Temp in SetDate.ListItemCount)
                        {
                            SetDate.ItemCount = Temp.Count.ToArray();
                            if (SetDate.ItemCount.Length > 1)
                            {
                                double Long = Math.Pow(Convert.ToDouble(SetDate.ItemCount.Length - 1), -1d);
                                for (int x = 0; x < SetDate.ItemCount.Length; x++)
                                {
                                    double t = Long * x;
                                    tSquared = t * t;
                                    tCubed = tSquared * t;
                                    int tempnummer = Convert.ToInt16((ay * tCubed) + (by * tSquared) + (cy * t) + (-p1.Y + 250));
                                    SetDate.SaveItem.Add(new ItemDate(SetDate.ItemCount[x], Convert.ToDouble(((((Convert.ToDouble(SetDate.LastNummerLow) - Convert.ToDouble(SetDate.FirstNummerLow)) * tempnummer) / 250) + Convert.ToDouble(SetDate.FirstNummerLow)).ToString("f3"))));
                                }
                            }
                            else
                            {
                                SetDate.SaveItem.Add(new ItemDate(SetDate.ItemCount[0], Convert.ToDouble(SetDate.LastNummerLow)));
                            }
                        }
                    }
                }
                else if (SetDate.UseMode == 2)
                {
                    //List<Point> TempReturn = new List<Point>();
                    //Dictionary<double, double> pList = new Dictionary<double, double>();
                    if (SetDate.ListItemCount.Length > 0)
                    {
                        SetDate.SaveItem = new List<ItemDate>();
                        double Long = Math.Pow(Convert.ToDouble(SetDate.ListItemCount.Length - 1), -1d);
                        for (int x = 0; x < SetDate.ListItemCount.Length; x++)
                        {
                            double t = Long * x;
                            tSquared = t * t;
                            tCubed = tSquared * t;
                            int tempnummer = Convert.ToInt16((ay * tCubed) + (by * tSquared) + (cy * t) + (-p1.Y + 250));
                            foreach (var temp in SetDate.ListItemCount[x].Count)
                            {
                                SetDate.SaveItem.Add(new ItemDate(temp, Convert.ToDouble(((((Convert.ToDouble(SetDate.LastNummerLow) - Convert.ToDouble(SetDate.FirstNummerLow)) * tempnummer) / 250) + Convert.ToDouble(SetDate.FirstNummerLow)).ToString("f3")), Convert.ToDouble(((((Convert.ToDouble(SetDate.LastNummerHigh) - Convert.ToDouble(SetDate.FirstNummerHigh)) * tempnummer) / 250) + Convert.ToDouble(SetDate.FirstNummerHigh)).ToString("f3"))));
                            }
                        }
                    }
                    else
                    {
                        foreach (var temp in SetDate.ListItemCount[0].Count)
                        {
                            SetDate.SaveItem.Add(new ItemDate(temp, Convert.ToDouble(SetDate.FirstNummerLow), Convert.ToDouble(SetDate.LastNummerHigh)));
                        }
                    }
                }
                BeginInvoke(new MethodInvoker(() =>
                {
                    var table = new DataTable();
                    table.Columns.Add("刚体");
                    table.Columns.Add("数值");
                    foreach (var dic in SetDate.SaveItem)
                    {
                        table.Rows.Add(dic.Count + ":" + newopen.GetPmx.Body[dic.Count].Name, dic.FirstNummer);
                    }
                    if (metroGrid2.InvokeRequired)
                    {
                        metroGrid2.DataSource = table;
                    }
                    else
                    {
                        metroGrid2.DataSource = table;
                    }
                }));
            }
            catch (Exception) { }
        }

        private void ShowBezier_MouseDown(object sender, MouseEventArgs e)
        {
            CanDrag = false;
            index = -1;
            Point p_mouse = e.Location;

            if (Math.Pow(p_mouse.X - p1.X, 2) + Math.Pow(p_mouse.Y - p1.Y, 2) <= tol * tol)
            {
                CanDrag = true;
                index = 1;
            }
            if (Math.Pow(p_mouse.X - p2.X, 2) + Math.Pow(p_mouse.Y - p2.Y, 2) <= tol * tol)
            {
                CanDrag = true;
                index = 2;
            }
            if (Math.Pow(p_mouse.X - p3.X, 2) + Math.Pow(p_mouse.Y - p3.Y, 2) <= tol * tol)
            {
                CanDrag = true;
                index = 3;
            }
            if (Math.Pow(p_mouse.X - p4.X, 2) + Math.Pow(p_mouse.Y - p4.Y, 2) <= tol * tol)
            {
                CanDrag = true;
                index = 4;
            }
        }

        private void ShowBezier_MouseMove(object sender, MouseEventArgs e)
        {
            int limt1 = 0;
            int limt2 = 250;
            if (CanDrag && e.Button == MouseButtons.Left)
            {
                if (index == 1)
                {
                    if (e.Location.Y <= limt2 && e.Location.Y >= limt1)
                    {
                        p1.Y = e.Location.Y;
                    }
                    else if (e.Location.Y >= limt2)
                    {
                        p1.Y = limt2;
                    }
                    else if (e.Location.Y <= limt1)
                    {
                        p1.Y = limt1;
                    }
                }
                if (index == 2)
                {
                    if (e.Location.Y <= limt2 && e.Location.Y >= limt1)
                    {
                        p2.Y = e.Location.Y;
                    }
                    else if (e.Location.Y >= limt2)
                    {
                        p2.Y = limt2;
                    }
                    else if (e.Location.Y <= limt1)
                    {
                        p2.Y = limt1;
                    }
                    if (e.Location.X <= limt2 && e.Location.X >= limt1)
                    {
                        p2.X = e.Location.X;
                    }
                    else if (e.Location.X >= limt2)
                    {
                        p2.X = limt2;
                    }
                    else if (e.Location.X <= limt1)
                    {
                        p2.X = limt1;
                    }
                }
                if (index == 3)
                {
                    if (e.Location.Y <= limt2 && e.Location.Y >= limt1)
                    {
                        p3.Y = e.Location.Y;
                    }
                    else if (e.Location.Y >= limt2)
                    {
                        p3.Y = limt2;
                    }
                    else if (e.Location.Y <= limt1)
                    {
                        p3.Y = limt1;
                    }
                    if (e.Location.X <= limt2 && e.Location.X >= limt1)
                    {
                        p3.X = e.Location.X;
                    }
                    else if (e.Location.X >= limt2)
                    {
                        p3.X = limt2;
                    }
                    else if (e.Location.X <= limt1)
                    {
                        p3.X = limt1;
                    }
                }
                if (index == 4)
                {
                    if (e.Location.Y <= limt2 && e.Location.Y >= limt1)
                    {
                        p4.Y = e.Location.Y;
                    }
                    else if (e.Location.Y >= limt2)
                    {
                        p4.Y = limt2;
                    }
                    else if (e.Location.Y <= limt1)
                    {
                        p4.Y = limt1;
                    }
                }
            }
            this.ShowBezier.Invalidate();
        }

        /*     private void Form2_Paint(object sender, PaintEventArgs e)
             {
                 Rectangle imCurveRect = new Rectangle();
                 Graphics g = e.Graphics;
                 int x0 = -5;
                 int x2 = 255;
                 float unitX = (float)ShowBezier.Width / 260;
                 for (int i = x0; i <= x2; i++)
                 {
                     if (i % 10 == 0) g.DrawLine(new Pen(Color.Black), new PointF((i - x0) * unitX + ShowBezier.Left,
                                 ShowBezier.Bottom), new PointF((i - x0) * unitX + ShowBezier.Left, ShowBezier.Bottom + 5)); // ruler line
                     if (i % 50 == 0)
                     {
                         g.DrawLine(new Pen(Color.Black, 2f), new PointF((i - x0) * unitX + ShowBezier.Left,
                             ShowBezier.Bottom), new PointF((i - x0) * unitX + ShowBezier.Left, ShowBezier.Bottom + 12));
                         SizeF stringSize = g.MeasureString(i.ToString(), this.Font);
                         PointF stringLoc = new PointF((i - x0) * unitX + ShowBezier.Left - stringSize.Width / 2, ShowBezier.Bottom + 12);
                         g.DrawString(i.ToString(), this.Font, new SolidBrush(Color.Black), stringLoc);
                     }
                 }
                 int y0 = -5;
                 int y2 = 255;
                 float unitY = (float)ShowBezier.Height / 260;
                 for (int i = y0; i <= y2; i++)
                 {
                     if (i % 10 == 0) g.DrawLine(new Pen(Color.Black), new PointF(ShowBezier.Left - 5, ShowBezier.Bottom - (i - y0) * unitY),
                                          new PointF(ShowBezier.Left, ShowBezier.Bottom - (i - y0) * unitY)); // ruler line
                     if (i % 50 == 0)
                     {
                         g.DrawLine(new Pen(Color.Black, 2f), new PointF(ShowBezier.Left - 10, ShowBezier.Bottom - (i - y0) * unitY),
                                  new PointF(ShowBezier.Left, ShowBezier.Bottom - (i - y0) * unitY)); // ruler line
                         SizeF stringSize = g.MeasureString(i.ToString(), this.Font);
                         PointF stringLoc = new PointF(ShowBezier.Left - 10 - stringSize.Width, ShowBezier.Bottom - (i - y0) * unitY - stringSize.Height / 2);
                         g.DrawString(i.ToString(), this.Font, new SolidBrush(Color.Black), stringLoc);
                     }
                 }
                 g.DrawRectangle(new Pen(Color.Black, 2), imCurveRect);
             }*/

        private void metroGrid1_Paint(object sender, PaintEventArgs e)
        {
            Rectangle imCurveRect = new Rectangle();
            Graphics g = e.Graphics;
            int x0 = 0;
            // int x2 = 255;
            int Left = ShowBezier.Left - 20;
            float Long = (float)255 / (float)(SetDate.ItemCount.Length == 1 ? 1 : (float)(SetDate.ItemCount.Length - 1));

            for (int i = 0; i < SetDate.ItemCount.Length; i++)
            {
                g.DrawLine(new Pen(Color.Black, 2f), new PointF((float)(x0 + i * Long + Left), ShowBezier.Bottom - 63), new PointF((float)(x0 + i * Long + Left), ShowBezier.Bottom - 63 + 12));
                SizeF stringSize = g.MeasureString(i.ToString(), this.Font);
                PointF stringLoc = new PointF((float)(x0 + i * Long + Left - stringSize.Width / 2), ShowBezier.Bottom - 63 + 12);
                g.DrawString((i).ToString(), this.Font, new SolidBrush(Color.Black), stringLoc);
            }

            /*float unitX = (float)ShowBezier.Width / 265;
            for (int i = x0; i <= x2; i++)
            {
                    g.DrawLine(new Pen(Color.Black, 2f), new PointF((i - x0) * unitX + Left,
                        ShowBezier.Bottom - 63), new PointF((i - x0) * unitX + Left, ShowBezier.Bottom - 63 + 12));
                    SizeF stringSize = g.MeasureString(i.ToString(), this.Font);
                    PointF stringLoc = new PointF((i - x0) * unitX + Left - stringSize.Width / 2, ShowBezier.Bottom - 63 + 12);
                    g.DrawString(i.ToString(), this.Font, new SolidBrush(Color.Black), stringLoc);
            }*/

            if (SetDate.mode == "1")
            {
                int y0 = 0;
                int y2 = 250;
                float unitY = (float)ShowBezier.Height / 265;
                for (int i = y0; i <= y2; i++)
                {
                    if (i % 10 == 0) g.DrawLine(new Pen(Color.Black), new PointF(Left - 5, ShowBezier.Bottom - 63 - (i - y0) * unitY),
                                         new PointF(Left, ShowBezier.Bottom - 63 - (i - y0) * unitY)); // ruler line
                    if (i % 50 == 0)
                    {
                        g.DrawLine(new Pen(Color.Black, 2f), new PointF(Left - 10, ShowBezier.Bottom - 63 - (i - y0) * unitY),
                                 new PointF(Left, ShowBezier.Bottom - 63 - (i - y0) * unitY)); // ruler line
                        SizeF stringSize = g.MeasureString(i.ToString(), this.Font);
                        PointF stringLoc = new PointF(Left - 15 - stringSize.Width, ShowBezier.Bottom - 63 - (i - y0) * unitY - stringSize.Height / 2);
                        g.DrawString((-i).ToString(), this.Font, new SolidBrush(Color.Black), stringLoc);
                    }
                }
                Left += 265;
                for (int i = y0; i <= y2; i++)
                {
                    if (i % 10 == 0) g.DrawLine(new Pen(Color.Black), new PointF(Left - 10, ShowBezier.Bottom - 63 - (i - y0) * unitY),
                                         new PointF(Left - 5, ShowBezier.Bottom - 63 - (i - y0) * unitY)); // ruler line
                    if (i % 50 == 0)
                    {
                        g.DrawLine(new Pen(Color.Black, 2f), new PointF(Left - 10, ShowBezier.Bottom - 63 - (i - y0) * unitY),
                new PointF(Left, ShowBezier.Bottom - 63 - (i - y0) * unitY)); // ruler line
                        SizeF stringSize = g.MeasureString(i.ToString(), this.Font);
                        if (i == 50)
                        {
                            PointF stringLoc = new PointF(Left + 20 - stringSize.Width, ShowBezier.Bottom - 63 - (i - y0) * unitY - stringSize.Height / 2);
                            g.DrawString(i.ToString(), this.Font, new SolidBrush(Color.Black), stringLoc);
                        }
                        else if (i == 0)
                        {
                            PointF stringLoc = new PointF(Left + 15 - stringSize.Width, ShowBezier.Bottom - 63 - (i - y0) * unitY - stringSize.Height / 2);
                            g.DrawString(i.ToString(), this.Font, new SolidBrush(Color.Black), stringLoc);
                        }
                        else
                        {
                            PointF stringLoc = new PointF(Left + 25 - stringSize.Width, ShowBezier.Bottom - 63 - (i - y0) * unitY - stringSize.Height / 2);
                            g.DrawString(i.ToString(), this.Font, new SolidBrush(Color.Black), stringLoc);
                        }
                    }
                }
            }
            else
            {
                int y0 = 0;
                int y2 = 250;
                float unitY = (float)ShowBezier.Height / 250;
                double tempnummer = (Convert.ToDouble(SetDate.LastNummerLow) - Convert.ToDouble(SetDate.FirstNummerLow)) / 5;
                double[] MathNummer = new double[6];
                for (int x = 0; x < MathNummer.Length - 1; x++)
                {
                    MathNummer[x] = Convert.ToDouble(SetDate.FirstNummerLow) + x * tempnummer;
                }
                MathNummer[5] = Convert.ToDouble(SetDate.LastNummerLow);
                for (int i = y0, x = 0; i <= y2; i++)
                {
                    if (i % 10 == 0) g.DrawLine(new Pen(Color.Black), new PointF(Left - 5, ShowBezier.Bottom - 63 - (i - y0) * unitY),
                                         new PointF(Left, ShowBezier.Bottom - 63 - (i - y0) * unitY)); // ruler line
                    if (i % 50 == 0)
                    {
                        g.DrawLine(new Pen(Color.Black, 2f), new PointF(Left - 10, ShowBezier.Bottom - 63 - (i - y0) * unitY),
                                 new PointF(Left, ShowBezier.Bottom - 63 - (i - y0) * unitY)); // ruler line
                        SizeF stringSize = g.MeasureString((MathNummer[x]).ToString(), this.Font);
                        PointF stringLoc = new PointF(Left - 15 - stringSize.Width, ShowBezier.Bottom - 63 - (i - y0) * unitY - stringSize.Height / 2);
                        g.DrawString((MathNummer[x]).ToString(), this.Font, new SolidBrush(Color.Black), stringLoc);
                        x++;
                    }
                }
            }
            g.DrawRectangle(new Pen(Color.Black, 2), imCurveRect);
        }

        private void Form2_Shown(object sender, EventArgs e)
        {
            if (SetDate != null)
            {
                XFirstNummer.Text = SetDate.FirstNummerLow;
                XLastNummer.Text = SetDate.LastNummerLow;
                bool find = false;
                foreach (var temp in HisForm)
                {
                    if (temp.Key == this.Text)
                    {
                        p1 = temp.Value.p1;
                        p2 = temp.Value.p2;
                        p3 = temp.Value.p3;
                        p4 = temp.Value.p4;
                        find = true;
                        break;
                    }
                }
                if (!find)
                {
                    p1 = new Point(0, 250);
                    p2 = new Point(50, 200);
                    p3 = new Point(200, 50);
                    p4 = new Point(250, 0);
                }
            }
            PointOnCubicBezier();
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
            switch (SetDate.mode)
            {
                case "Mass":
                    foreach (var temp in SetDate.SaveItem)
                    {
                        newopen.GetPmx.Body[temp.Count].Mass = (float)temp.FirstNummer;
                    }

                    break;

                case "PositionDamping":
                    foreach (var temp in SetDate.SaveItem)
                    {
                        newopen.GetPmx.Body[temp.Count].PositionDamping = (float)temp.FirstNummer;
                    }
                    break;

                case "RotationDamping":
                    foreach (var temp in SetDate.SaveItem)
                    {
                        newopen.GetPmx.Body[temp.Count].RotationDamping = (float)temp.FirstNummer;
                    }
                    break;

                case "Restitution":
                    foreach (var temp in SetDate.SaveItem)
                    {
                        newopen.GetPmx.Body[temp.Count].Restitution = (float)temp.FirstNummer;
                    }
                    break;

                case "Friction":
                    foreach (var temp in SetDate.SaveItem)
                    {
                        newopen.GetPmx.Body[temp.Count].Friction = (float)temp.FirstNummer;
                    }
                    break;

                case "Radial":
                    foreach (var temp in SetDate.SaveItem)
                    {
                        newopen.GetPmx.Body[temp.Count].BoxSize.X = (float)temp.FirstNummer;
                    }
                    break;

                case "Height":
                    foreach (var temp in SetDate.SaveItem)
                    {
                        newopen.GetPmx.Body[temp.Count].BoxSize.Y = (float)temp.FirstNummer;
                    }
                    break;

                case "Depth":
                    foreach (var temp in SetDate.SaveItem)
                    {
                        newopen.GetPmx.Body[temp.Count].BoxSize.Z = (float)temp.FirstNummer;
                    }
                    break;
            }
            Program.ARGS.Host.Connector.Pmx.Update(newopen.GetPmx);
            Program.ARGS.Host.Connector.Form.UpdateList(UpdateObject.Body);
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
                if (Temp.Key == this.Text)
                {
                    HisForm.Remove(Temp.Key);
                    break;
                }
            }
            HisForm.Add(this.Text, new BezierPoint(p1, p2, p3, p4));
        }

        private void metroToggle1_CheckedChanged(object sender, EventArgs e)
        {
            this.TopMost = metroToggle1.Checked ? true : false;
        }

        private float tol = 10;

        private void ChangeNummer(object sender, EventArgs e)
        {
            if (XFirstNummer.Text != string.Empty && XLastNummer.Text != string.Empty)
            {
                SetDate.FirstNummerLow = XFirstNummer.Text;
                SetDate.LastNummerLow = XLastNummer.Text;
                switch (SetDate.mode)
                {
                    case "Mass":
                        Class2.newopen.MassFuntionFirstNummer.Text = XFirstNummer.Text;
                        Class2.newopen.MassFuntionLastNummer.Text = XLastNummer.Text;
                        break;

                    case "PositionDamping":
                        Class2.newopen.PositionDampingFirstNummer.Text = XFirstNummer.Text;
                        Class2.newopen.PositionDampingLastNummer.Text = XLastNummer.Text;
                        break;

                    case "RotationDamping":
                        Class2.newopen.RotationDampingFirstNummer.Text = XFirstNummer.Text;
                        Class2.newopen.RotationDampingLastNummer.Text = XLastNummer.Text;
                        break;

                    case "Restitution":
                        Class2.newopen.RestitutionFuntionFirstNummer.Text = XFirstNummer.Text;
                        Class2.newopen.RestitutionFuntionLastNummer.Text = XLastNummer.Text;
                        break;

                    case "Friction":
                        Class2.newopen.FrictionFuntionFirstNummer.Text = XFirstNummer.Text;
                        Class2.newopen.FrictionFuntionLastNummer.Text = XLastNummer.Text;
                        break;

                    case "Radial":
                        Class2.newopen.RadialFirstNummer.Text = XFirstNummer.Text;
                        Class2.newopen.RadialLastNummer.Text = XLastNummer.Text;
                        break;

                    case "Height":
                        Class2.newopen.HeightFirstNummer.Text = XFirstNummer.Text;
                        Class2.newopen.HeightLastNummer.Text = XLastNummer.Text;
                        break;

                    case "Depth":
                        Class2.newopen.DepthFirstNummer.Text = XFirstNummer.Text;
                        Class2.newopen.DepthLastNummer.Text = XLastNummer.Text;
                        break;
                }
            }
            PointOnCubicBezier();
            if (metroToggle2.Checked)
            {
                TheFunOfChange(2);
            }
            this.metroGrid1.Invalidate();
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
            MetroTrackBar NewTrackBar = sender as MetroTrackBar;
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
            this.ShowBezier.Invalidate();
        }

        private void Reset_Click(object sender, EventArgs e)
        {
            p1 = new Point(0, 250);
            p2 = new Point(50, 200);
            p3 = new Point(200, 50);
            p4 = new Point(250, 0);
            PointOnCubicBezier();
            this.Refresh();
        }

        private bool CanDrag;

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
            g.DrawBezier(pb, p1, p2, p3, p4);
            g.DrawLine(pc, p1, p2);
            g.DrawLine(pc, p3, p4);

            Color First = PenSecond;
            Color Second = PenFirst;
            int Push = (int)LinkSize + 5;
            /*  g.DrawLine(new Pen(First, 2), new Point(p2.X + Push, p2.Y + Push), new Point(p2.X - Push, p2.Y - Push));
              g.DrawLine(new Pen(First, 2), new Point(p2.X - Push, p2.Y + Push), new Point(p2.X + Push, p2.Y - Push));
              g.DrawLine(new Pen(First, 2), new Point(p3.X + Push, p3.Y + Push), new Point(p3.X - Push, p3.Y - Push));
              g.DrawLine(new Pen(First, 2), new Point(p3.X - Push, p3.Y + Push), new Point(p3.X + Push, p3.Y - Push));*/
            g.DrawLine(new Pen(First, LinkSize - 1), new Point(p2.X + Push, p2.Y), new Point(p2.X - Push, p2.Y));
            g.DrawLine(new Pen(First, LinkSize - 1), new Point(p2.X, p2.Y + Push), new Point(p2.X, p2.Y - Push));
            g.DrawLine(new Pen(First, LinkSize - 1), new Point(p3.X + Push, p3.Y), new Point(p3.X - Push, p3.Y));
            g.DrawLine(new Pen(First, LinkSize - 1), new Point(p3.X, p3.Y + Push), new Point(p3.X, p3.Y - Push));
            g.DrawLine(new Pen(Second, LinkSize - 1), new Point(p1.X + Push, p1.Y), new Point(p1.X - 5, p1.Y));
            g.DrawLine(new Pen(Second, LinkSize - 1), new Point(p1.X, p1.Y + Push), new Point(p1.X, p1.Y - 5));
            g.DrawLine(new Pen(Second, LinkSize - 1), new Point(p4.X + Push, p4.Y), new Point(p4.X - 5, p4.Y));
            g.DrawLine(new Pen(Second, LinkSize - 1), new Point(p4.X, p4.Y + Push), new Point(p4.X, p4.Y - 5));

            if (index == 1)
                g.FillEllipse(sb_on, new RectangleF(p1.X - tol / 2, p1.Y - tol / 2, tol, tol));
            if (index == 2)
                g.FillEllipse(sb_on, new RectangleF(p2.X - tol / 2, p2.Y - tol / 2, tol, tol));
            if (index == 3)
                g.FillEllipse(sb_on, new RectangleF(p3.X - tol / 2, p3.Y - tol / 2, tol, tol));
            if (index == 4)
                g.FillEllipse(sb_on, new RectangleF(p4.X - tol / 2, p4.Y - tol / 2, tol, tol));
        }

        public void MoreThanZeroNumCheck(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\r' && e.KeyChar != '\b' && e.KeyChar != '.' && e.KeyChar != '\u0016' && e.KeyChar != '\u0003')//这是允许输入退格键
            {
                if ((e.KeyChar < '0') || (e.KeyChar > '9'))//这是允许输入0-9数字
                {
                    e.Handled = true;
                }
            }
            else if (e.KeyChar == '\r')
            {
                MetroTextBox NewTextBox = sender as MetroTextBox;
                ChangeNummer(sender, e);
            }
        }
    }
}