#region

using MetroFramework;
using MetroFramework.Controls;
using MetroFramework.Forms;
using PEPlugin;
using PEPlugin.Pmd;
using PEPlugin.Pmx;
using PEPlugin.SDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static PE多功能信息处理插件.Class2;
using static PE多功能信息处理插件.Program;

#endregion

namespace PE多功能信息处理插件
{
    public partial class Metroform : MetroForm
    {
        public bool Run = true;
        private bool savecheck = false;
        private List<OpenHis> HisTemp = new List<OpenHis>();
        public List<int> BoneCount = new List<int>();
        public List<int> BodyCount = new List<int>();
        public List<int> JointCount = new List<int>();
        public IPXPmx GetPmx;
        #region 界面操作

        public Metroform()
        {
            InitializeComponent();
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw
                | ControlStyles.Selectable
                | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.SupportsTransparentBackColor, true);
            Style = metroStyleManager.Style = (MetroColorStyle)bootstate.StyleState;
            Meminfo.Click += delegate { GC.Collect(); };
            CheckForIllegalCrossThreadCalls = false;
            Task.Factory.StartNew(() =>
            {
                while (Run)
                {
                    try
                    {
                        Thread.Sleep(1000);
                        Meminfo.Text = "Memory:" + ((Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024) + "MB").ToString();
                    }
                    catch (Exception)
                    {
                        if (Run)
                        { continue; }
                        else { break; }
                    }
                }
            }, TaskCreationOptions.LongRunning);
            new Thread(() =>
           {
               try
               {
                   if (bootstate.ShowUpdata == 0 && bootstate.UpdataDateCheck != new CultureInfo("zh-CN").Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFullWeek, DayOfWeek.Friday))
                   {
                       bootstate.UpdataDateCheck = new CultureInfo("zh-CN").Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFullWeek, DayOfWeek.Friday);
                       var Update = new Regex(@"((?<!\d)((\d{2,4}(\.|年|\/|\-))((((0?[13578]|1[02])(\.|月|\/|\-))((3[01])|([12][0-9])|(0?[1-9])))|(0?2(\.|月|\/|\-)((2[0-8])|(1[0-9])|(0?[1-9])))|(((0?[469]|11)(\.|月|\/|\-))((30)|([12][0-9])|(0?[1-9]))))|((([0-9]{2})((0[48]|[2468][048]|[13579][26])|((0[48]|[2468][048]|[3579][26])00))(\.|年|\/|\-))0?2(\.|月|\/|\-)29))日?(?!\d))").Match(System.Text.Encoding.UTF8.GetString(new System.Net.WebClient().DownloadData("https://bowlroll.net/file/95442")));
                       if (Update.Value != Resource1.UpdateData)
                       {
                           BeginInvoke(new MethodInvoker(() =>
                           {
                               MetroTaskWindow.ShowTaskWindow(this.Parent, "检测到更新", new TaskWindowControl2(), 10);
                           }));
                       }
                   }
               }
               catch (Exception)
               {
               }
           }).Start();
            if (bootstate.FormX != 0)
            {
                if (bootstate.FormX > 0 && bootstate.FormX < Screen.PrimaryScreen.Bounds.Width && bootstate.FormY < Screen.PrimaryScreen.Bounds.Height)
                {
                    this.Location = new Point(bootstate.FormX, bootstate.FormY);
                }
                Size = new Size(bootstate.FormForSizeY, bootstate.FormForSizeX);
            }
            #region 列表初始化
            {
                DataTable table = new DataTable();
                table.Columns.Add("操作");
                table.Columns.Add("骨骼和权重");
                VertexList.DataSource = table;
            }
            {
                var table = new DataTable();
                table.Columns.Add("骨骼顺序");
                table.Columns.Add("骨骼名称");
                BoneList.DataSource = table;
            }
            {
                var table = new DataTable();
                table.Columns.Add("刚体顺序");
                table.Columns.Add("刚体名称");
                BodyList.DataSource = table;
            }
            {
                var table = new DataTable();
                table.Columns.Add("J点顺序");
                table.Columns.Add("J点名称");
                JointList.DataSource = table;
            }
            {
                var table = new DataTable();
                table.Columns.Add("ID");
                table.Columns.Add("模型名称");
                table.Columns.Add("打开日期");
                table.Columns.Add("模型路径");
                if (bootstate.HisOpen != null)
                {
                    HisTemp = new List<OpenHis>();
                    HisTemp.AddRange(bootstate.HisOpen.ToArray());
                    HisTemp.Reverse();
                    for (int i = 0; i < HisTemp.Count; i++)
                    {
                        table.Rows.Add(i + 1, HisTemp[i].modelname, HisTemp[i].modeldata, HisTemp[i].modelpath);
                    }
                }
                HisOpenList.DataSource = table;
            }
            #endregion
            #region 自动打开模型和窗口前置初始化
            try
            {
                if (bootstate.AutoOpen == 1)
                {
                    AutoOpenModel.Checked = true;
                }
                else
                {
                    AutoOpenModel.Checked = false;
                }
            }
            catch (Exception)
            {
                bootstate.AutoOpen = 0;
                AutoOpenModel.Checked = false;
            }
            try
            {
                if (bootstate.FormTopmost == 1)
                {
                    FormTopmost.Checked = true;
                    TopMost = true;
                }
                else
                {
                    FormTopmost.Checked = false;
                    TopMost = false;
                }
            }
            catch (Exception)
            {
            }
            #endregion
            #region T窗口权重调整初始化
            try
            {
                if (bootstate.WeightAddKey == 0 || bootstate.WeightAppleKey == 0 || bootstate.WeightGetKey == 0 || bootstate.WeightMinusKey == 0)
                {
                    bootstate.WeightAddKey = '+';
                    bootstate.WeightMinusKey = '-';
                    bootstate.WeightAppleKey = '*';
                    bootstate.WeightGetKey = '/';
                }
                WeightAddKey.Text = ((char)bootstate.WeightAddKey).ToString();
                WeightMinusKey.Text = ((char)bootstate.WeightMinusKey).ToString();
                WeightAppleKey.Text = ((char)bootstate.WeightAppleKey).ToString();
                WeightGetKey.Text = ((char)bootstate.WeightGetKey).ToString();
            }
            catch (Exception)
            {
            }
            #endregion
            #region 刚体/J点下拉框初始化
            MassFuntionSelect.SelectedIndex = 0;
            PositionDampingSelect.SelectedIndex = 0;
            RotationDampingFuntionSelect.SelectedIndex = 0;
            RestitutionFuntionSelect.SelectedIndex = 0;
            FrictionFuntionSelect.SelectedIndex = 0;
            RadialFuntionSelect.SelectedIndex = 0;
            HeightFuntionSelect.SelectedIndex = 0;
            DepthFuntionSelect.SelectedIndex = 0;
            SpringConst_Move_FuntionSelect.SelectedIndex = 0;
            SpringConst_Rotate_Nummer_FuntionSelect.SelectedIndex = 0;
            Limit_Move_X_FuntionSelect.SelectedIndex = 0;
            Limit_Move_Y_FuntionSelect.SelectedIndex = 0;
            Limit_Move_Z_FuntionSelect.SelectedIndex = 0;
            Limit_Angle_FuntionSelect_X.SelectedIndex = 0;
            Limit_Angle_FuntionSelect_Y.SelectedIndex = 0;
            Limit_Angle_FuntionSelect_Z.SelectedIndex = 0;
            BoneConnectMode.SelectedIndex = 0;
            BodySelectGroup.SelectedIndex = 1;
            SelectConnectObject.DataSource = new List<string>();
            LocalSelect.SelectedIndex = 0;
            #endregion
            #region 按键调整初始化
            if ((new FileInfo(new FileInfo(ARGS.Host.Connector.System.HostApplicationPath).DirectoryName + @"\_data\KeyData.XML").Exists))
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Binder = new UBinder();
                Stream stream = new FileStream(new FileInfo(ARGS.Host.Connector.System.HostApplicationPath).DirectoryName + @"\_data\KeyData.XML", FileMode.Open, FileAccess.Read, FileShare.Read);
                List<keySet> KeyTemp = new List<keySet>((keySet[])formatter.Deserialize(stream));
                stream.Close();
                KeyTemp.Sort((x, y) => x.TabID.CompareTo(y.TabID));
                List<TabInfo> TabTemp = new List<TabInfo>();
                foreach (var temp in KeyTemp)
                {
                    var temp2 = (from T in TabTemp
                                 where temp.TabID == T.TabID
                                 select T).FirstOrDefault();
                    if (temp2 != null)
                    {
                        temp2.KeyLIst.Add(new TabInfo.KeyInfo(temp.itemname, temp.itemKey, temp.itemLocalX, temp.itemLocaly, temp.itemFun));
                    }
                    else
                    {
                        TabTemp.Add(new TabInfo(temp.TabID, temp.TabName, temp.itemname, temp.itemKey, temp.itemLocalX, temp.itemLocaly, temp.itemFun));
                    }
                }
                ShortcutKeyTab.BeginInvoke(new MethodInvoker(() =>
                {
                    foreach (var temp in TabTemp)
                    {
                        MetroTabPage page = new MetroTabPage();
                        page.Text = temp.TabName;
                        ShortcutKeyTab.TabPages.Add(page);
                        foreach (var temp2 in temp.KeyLIst)
                        {
                            MetroTile TempTile = new MetroTile();
                            TempTile.Location = new Point(temp2.itemLocalX, temp2.itemLocaly);
                            TempTile.Text = temp2.itemname;
                            string[] eachchar = temp2.itemname.Select(x => x.ToString()).ToArray();
                            TempTile.Size = new Size(eachchar.Length * 13, 38);
                            TempTile.TextAlign = ContentAlignment.TopLeft;
                            TempTile.MouseDown += Controls_MouseDown;
                            TempTile.MouseUp += Controls_MouseUp;
                            TempTile.MouseMove += Controls_MouseMove;
                            TempTile.MouseClick += Controls_MouseClick;
                            page.Controls.Add(TempTile);
                        }
                    }
                }));
            }
            #endregion
            GetPmx = ARGS.Host.Connector.Pmx.GetCurrentState();
            FormClosed += delegate
             {
                 try
                 {
                     Run = false;
                     bootstate.FormX = Location.X;
                     bootstate.FormY = Location.Y;
                     bootstate.FormForSizeX = Size.Height;
                     bootstate.FormForSizeY = Size.Width;
                     ThreadPool.QueueUserWorkItem(new WaitCallback(Save));
                     foreach (var Temp in ListForm)
                     {
                         try
                         {
                             Temp.Dispose();
                         }
                         catch (Exception)
                         {
                         }
                     }
                     newopen.Dispose();
                     newopen = null;
                 }
                 catch (Exception)
                 {
                 }
             };
            Task.Factory.StartNew(() =>
            {
                List<int> Hisbone = new List<int>();
                List<int> Hisbody = new List<int>();
                List<int> Jointbody = new List<int>();
                List<IPXBone> BoneHis = new List<IPXBone>();
                List<IPXBody> BodyHis = new List<IPXBody>();
                List<IPXJoint> JointHis = new List<IPXJoint>();
                do
                {
                    try
                    {
                        if (ALLTAB.SelectedTab.Name == "历史打开")
                        {
                            if (bootstate.HisOpen != null)
                            {
                                if (bootstate.HisOpen.Length != HisTemp.Count)
                                {
                                    BeginInvoke(new MethodInvoker(() =>
                                    {
                                        try
                                        {
                                            var table = HisOpenList.DataSource as DataTable;
                                            HisTemp = new List<OpenHis>();
                                            HisTemp.AddRange(bootstate.HisOpen.ToArray());
                                            HisTemp.Reverse();
                                            table.Rows.Clear();
                                            table.Columns.Clear();
                                            table.Columns.Add("ID");
                                            table.Columns.Add("模型名称");
                                            table.Columns.Add("打开日期");
                                            table.Columns.Add("模型路径");
                                            table.Rows.Add();
                                            table.Rows.Clear();
                                            for (int i = 0; i < HisTemp.Count; i++)
                                            {
                                                table.Rows.Add(i + 1, HisTemp[i].modelname, HisTemp[i].modeldata, HisTemp[i].modelpath);
                                            }
                                        }
                                        catch (Exception)
                                        {
                                        }
                                    }));
                                }
                            }
                        }
                        if (ManualRadioButton.Checked)
                        {
                            switch (ALLTAB.SelectedTab.Name)
                            {
                                case "Bone":
                                    {
                                        List<int> selectbone = new List<int>(ARGS.Host.Connector.View.PMDView.GetSelectedBoneIndices().Distinct());
                                        if (!Hisbone.SequenceEqual(selectbone) && !LockSelect.Checked)
                                        {
                                            GetPmx = ARGS.Host.Connector.Pmx.GetCurrentState();
                                            IPXPmx ThePmxOfNow = GetPmx;
                                            Hisbone = new List<int>(selectbone.ToArray());
                                            ClearList("bone");
                                            if (selectbone.Count != 0)
                                            {
                                                BoneCount.Clear();
                                                BoneCount.AddRange(selectbone);
                                                BeginInvoke(new MethodInvoker(() =>
                                                {
                                                    var table = BoneList.DataSource as DataTable;
                                                    foreach (int temp in selectbone)
                                                    {
                                                        if (MirrorMode.Checked)
                                                        {
                                                            table.Rows.Add(temp, ThePmxOfNow.Bone[temp].Name);
                                                        }
                                                        else
                                                        {
                                                            var MirrorBoneName = ThePmxOfNow.Bone[temp].Name.Replace(MirrorOriChar.Text, MirrorFinChar.Text);

                                                            if (MirrorBoneName == ThePmxOfNow.Bone[temp].Name)
                                                            {
                                                                var TempBone = (from item in ThePmxOfNow.Bone
                                                                                orderby Getdistance(ThePmxOfNow.Bone[temp].Position, item.Position) ascending
                                                                                select item).FirstOrDefault();
                                                                if (TempBone != ThePmxOfNow.Bone[temp])
                                                                {
                                                                    table.Rows.Add(temp + ":" + ThePmxOfNow.Bone[temp].Name, "->", ThePmxOfNow.Bone.IndexOf(TempBone) + ":" + TempBone.Name);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                var getbone = ThePmxOfNow.Bone.FirstOrDefault((x) => x.Name == MirrorBoneName);
                                                                if (getbone != null)
                                                                {
                                                                    table.Rows.Add(temp + ":" + ThePmxOfNow.Bone[temp].Name, "->", ThePmxOfNow.Bone.IndexOf(getbone) + ":" + MirrorBoneName);
                                                                }
                                                                else
                                                                {
                                                                    var TempBone = (from item in ThePmxOfNow.Bone
                                                                                    orderby Getdistance(ThePmxOfNow.Bone[temp].Position, item.Position) ascending
                                                                                    select item).FirstOrDefault();
                                                                    if (TempBone != ThePmxOfNow.Bone[temp])
                                                                    {
                                                                        table.Rows.Add(temp + ":" + ThePmxOfNow.Bone[temp].Name, "->", ThePmxOfNow.Bone.IndexOf(TempBone) + ":" + TempBone.Name);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    if (table.Rows.Count != 0)
                                                    {
                                                        InputBoneName.Text = DeleteBoneNummer.Checked ? Regex.Replace(BoneList.Rows[0].Cells[1].Value.ToString(), @"\d", "") : BoneList.Rows[0].Cells[1].Value.ToString();
                                                    }
                                                }));
                                            }
                                            else
                                            {
                                                InputBoneName.Text = "";
                                            }
                                        }
                                    }
                                    break;

                                case "Body":
                                    {
                                        var table = BodyList.DataSource as DataTable;
                                        List<int> selectbody = new List<int>(ARGS.Host.Connector.View.PMDView.GetSelectedBodyIndices());
                                        if (!Hisbody.SequenceEqual(selectbody) && !LockSelect.Checked)
                                        {
                                            GetPmx = ARGS.Host.Connector.Pmx.GetCurrentState();
                                            IPXPmx ThePmxOfNow = GetPmx;
                                            Hisbody = new List<int>(selectbody.ToArray());
                                            ClearList("body");
                                            if (selectbody.Count != 0)
                                            {
                                                if (!selectbody.All(b => BodyCount.Any(a => a.Equals(b))) || selectbody.Count != BodyList.RowCount)
                                                {
                                                    BodyCount.Clear();
                                                    BodyCount.AddRange(selectbody);
                                                    BeginInvoke(new MethodInvoker(() =>
                                                    {
                                                        try
                                                        {
                                                            foreach (int temp in selectbody)
                                                            {
                                                                table.Rows.Add(temp, ThePmxOfNow.Body[temp].Name);
                                                            }
                                                            if (table.Rows.Count == 0) return;
                                                            InputBodyName.Text = DeleteBodyNummer.Checked ? Regex.Replace(BodyList.Rows[0].Cells[1].Value.ToString(), @"\d", "") : BodyList.Rows[0].Cells[1].Value.ToString();
                                                            if (CheckSyncSelect.Checked)
                                                            {
                                                                MassFuntionFirstNummer.Text = ThePmxOfNow.Body[BodyCount[0]].Mass.ToString();
                                                                MassFuntionLastNummer.Text = ThePmxOfNow.Body[BodyCount[BodyCount.Count - 1]].Mass.ToString();

                                                                PositionDampingFirstNummer.Text = ThePmxOfNow.Body[BodyCount[0]].PositionDamping.ToString();
                                                                PositionDampingLastNummer.Text = ThePmxOfNow.Body[BodyCount[BodyCount.Count - 1]].PositionDamping.ToString();

                                                                RotationDampingFirstNummer.Text = ThePmxOfNow.Body[BodyCount[0]].RotationDamping.ToString();
                                                                RotationDampingLastNummer.Text = ThePmxOfNow.Body[BodyCount[BodyCount.Count - 1]].RotationDamping.ToString();

                                                                RestitutionFuntionFirstNummer.Text = ThePmxOfNow.Body[BodyCount[0]].Restitution.ToString();
                                                                RestitutionFuntionLastNummer.Text = ThePmxOfNow.Body[BodyCount[BodyCount.Count - 1]].Restitution.ToString();

                                                                FrictionFuntionFirstNummer.Text = ThePmxOfNow.Body[BodyCount[0]].Friction.ToString();
                                                                FrictionFuntionLastNummer.Text = ThePmxOfNow.Body[BodyCount[BodyCount.Count - 1]].Friction.ToString();
                                                            }
                                                        }
                                                        catch (Exception)
                                                        {
                                                        }
                                                    }));
                                                }
                                                else
                                                {
                                                    InputBodyName.Text = "";
                                                }
                                            }
                                        }
                                    }
                                    break;

                                case "joint":
                                    {
                                        List<int> selectjoint = new List<int>(ARGS.Host.Connector.View.PMDView.GetSelectedJointIndices());
                                        if (!Jointbody.SequenceEqual(selectjoint) && !LockSelect.Checked)
                                        {
                                            GetPmx = ARGS.Host.Connector.Pmx.GetCurrentState();
                                            IPXPmx ThePmxOfNow = GetPmx;
                                            Jointbody = new List<int>(selectjoint.ToArray());
                                            ClearList("joint");
                                            if (selectjoint.Count != 0)
                                            {
                                                if (!selectjoint.All(b => JointCount.Any(a => a.Equals(b))) || selectjoint.Count != JointList.RowCount)
                                                {
                                                    JointCount.Clear();
                                                    JointCount.AddRange(selectjoint);
                                                    BeginInvoke(new MethodInvoker(() =>
                                                    {
                                                        var table = JointList.DataSource as DataTable;
                                                        foreach (int temp in selectjoint)
                                                        {
                                                            table.Rows.Add(temp, ThePmxOfNow.Joint[temp].Name);
                                                        }
                                                        if (table.Rows.Count == 0) return;
                                                        InputJointName.Text = DeleteJointNummer.Checked ? Regex.Replace(JointList.Rows[0].Cells[1].Value.ToString(), @"\d", "") : JointList.Rows[0].Cells[1].Value.ToString();
                                                        if (CheckSyncSelect.Checked)
                                                        {
                                                            SpringConst_Move_FirstXNummer.Text = ThePmxOfNow.Joint[JointCount[0]].SpringConst_Move.X.ToString();
                                                            SpringConst_Move_LastXNummer.Text = ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].SpringConst_Move.X.ToString();

                                                            SpringConst_Move_FirstYNummer.Text = ThePmxOfNow.Joint[JointCount[0]].SpringConst_Move.Y.ToString();
                                                            SpringConst_Move_LastYNummer.Text = ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].SpringConst_Move.Y.ToString();

                                                            SpringConst_Move_FirstZNummer.Text = ThePmxOfNow.Joint[JointCount[0]].SpringConst_Move.Z.ToString();
                                                            SpringConst_Move_LastZNummer.Text = ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].SpringConst_Move.Z.ToString();

                                                            SpringConst_Rotate_FirstXNummer.Text = ThePmxOfNow.Joint[JointCount[0]].SpringConst_Rotate.X.ToString();
                                                            SpringConst_Rotate_LastXNummer.Text = ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].SpringConst_Rotate.X.ToString();

                                                            SpringConst_Rotate_FirstYNummer.Text = ThePmxOfNow.Joint[JointCount[0]].SpringConst_Rotate.Y.ToString();
                                                            SpringConst_Rotate_LastYNummer.Text = ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].SpringConst_Rotate.Y.ToString();

                                                            SpringConst_Rotate_FirstZNummer.Text = ThePmxOfNow.Joint[JointCount[0]].SpringConst_Rotate.Z.ToString();
                                                            SpringConst_Rotate_LastZNummer.Text = ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].SpringConst_Rotate.Z.ToString();

                                                            Limit_MoveLow_FirstXNummer.Text = ThePmxOfNow.Joint[JointCount[0]].Limit_MoveLow.X.ToString();
                                                            Limit_MoveLow_LastXNummer.Text = ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].Limit_MoveLow.X.ToString();
                                                            Limit_MoveHigh_FirstXNummer.Text = ThePmxOfNow.Joint[JointCount[0]].Limit_MoveHigh.X.ToString();
                                                            Limit_MoveHigh_LastXNummer.Text = ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].Limit_MoveHigh.X.ToString();

                                                            Limit_MoveLow_FirstYNummer.Text = ThePmxOfNow.Joint[JointCount[0]].Limit_MoveLow.Y.ToString();
                                                            Limit_MoveLow_LastYNummer.Text = ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].Limit_MoveLow.Y.ToString();
                                                            Limit_MoveHigh_FirstYNummer.Text = ThePmxOfNow.Joint[JointCount[0]].Limit_MoveHigh.Y.ToString();
                                                            Limit_MoveHigh_LastYNummer.Text = ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].Limit_MoveHigh.Y.ToString();

                                                            Limit_MoveLow_FirstZNummer.Text = ThePmxOfNow.Joint[JointCount[0]].Limit_MoveLow.Z.ToString();
                                                            Limit_MoveLow_LastZNummer.Text = ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].Limit_MoveLow.Z.ToString();
                                                            Limit_MoveHigh_FirstZNummer.Text = ThePmxOfNow.Joint[JointCount[0]].Limit_MoveHigh.Z.ToString();
                                                            Limit_MoveHigh_LastZNummer.Text = ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].Limit_MoveHigh.Z.ToString();

                                                            Limit_AngleLow_FirstXNummer.Text = (180 * ((ThePmxOfNow.Joint[JointCount[0]].Limit_AngleLow.X)) / Math.PI).ToString();
                                                            Limit_AngleLow_LastXNummer.Text = (180 * ((ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].Limit_AngleLow.X)) / Math.PI).ToString();
                                                            Limit_AngleHigh_FirstXNummer.Text = (180 * ((ThePmxOfNow.Joint[JointCount[0]].Limit_AngleHigh.X)) / Math.PI).ToString();
                                                            Limit_AngleHigh_LastXNummer.Text = (180 * ((ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].Limit_AngleHigh.X)) / Math.PI).ToString();

                                                            Limit_AngleLow_FirstYNummer.Text = (180 * ((ThePmxOfNow.Joint[JointCount[0]].Limit_AngleLow.Y)) / Math.PI).ToString();
                                                            Limit_AngleLow_LastYNummer.Text = (180 * ((ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].Limit_AngleLow.Y)) / Math.PI).ToString();
                                                            Limit_AngleHigh_FirstYNummer.Text = (180 * ((ThePmxOfNow.Joint[JointCount[0]].Limit_AngleHigh.Y)) / Math.PI).ToString();
                                                            Limit_AngleHigh_LastYNummer.Text = (180 * ((ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].Limit_AngleHigh.Y)) / Math.PI).ToString();

                                                            Limit_AngleLow_FirstZNummer.Text = (180 * ((ThePmxOfNow.Joint[JointCount[0]].Limit_AngleLow.Z)) / Math.PI).ToString();
                                                            Limit_AngleLow_LastZNummer.Text = (180 * ((ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].Limit_AngleLow.Z)) / Math.PI).ToString();
                                                            Limit_AngleHigh_FirstZNummer.Text = (180 * ((ThePmxOfNow.Joint[JointCount[0]].Limit_AngleHigh.Z)) / Math.PI).ToString();
                                                            Limit_AngleHigh_LastZNummer.Text = (180 * ((ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].Limit_AngleHigh.Z)) / Math.PI).ToString();
                                                        }
                                                    }));
                                                }
                                                else
                                                {
                                                    InputJointName.Text = "";
                                                }
                                            }
                                        }
                                        break;
                                    }
                            }
                        }
                        else if (AutomaticRadioButton.Checked)
                        {
                            switch (ALLTAB.SelectedTab.Name)
                            {
                                case "Bone":
                                    if (BoneHis.Count != GetPmx.Bone.Count)
                                    {
                                        GetPmx = ARGS.Host.Connector.Pmx.GetCurrentState();
                                        IPXPmx ThePmxOfNow = GetPmx;
                                        BoneHis = new List<IPXBone>(ThePmxOfNow.Bone);
                                        ClearList("bone");
                                        BeginInvoke(new MethodInvoker(() =>
                                        {
                                            try
                                            {
                                                var table = BoneList.DataSource as DataTable;
                                                BoneCount.Clear();
                                                for (int i = 0; i < GetPmx.Bone.Count; i++)
                                                {
                                                    table.Rows.Add(i, GetPmx.Bone[i].Name);
                                                }
                                                if (table.Rows.Count == 0) return;
                                                InputBoneName.Text = DeleteBoneNummer.Checked ? Regex.Replace(BoneList.Rows[0].Cells[1].Value.ToString(), @"\d", "") : BoneList.Rows[0].Cells[1].Value.ToString();
                                            }
                                            catch (Exception)
                                            {
                                            }
                                        }));
                                    }
                                    break;

                                case "Body":
                                    if (BodyHis.Count != GetPmx.Body.Count)
                                    {
                                        GetPmx = ARGS.Host.Connector.Pmx.GetCurrentState();
                                        IPXPmx ThePmxOfNow = GetPmx;
                                        BodyHis = new List<IPXBody>(ThePmxOfNow.Body);
                                        ClearList("body");
                                        BeginInvoke(new MethodInvoker(() =>
                                        {
                                            try
                                            {
                                                var table = BodyList.DataSource as DataTable;
                                                BodyCount.Clear();
                                                for (int i = 0; i < ThePmxOfNow.Body.Count; i++)
                                                {
                                                    table.Rows.Add(i, ThePmxOfNow.Body[i].Name);
                                                }
                                                if (table.Rows.Count == 0) return;
                                                InputBodyName.Text = DeleteBodyNummer.Checked ? Regex.Replace(BodyList.Rows[0].Cells[1].Value.ToString(), @"\d", "") : BodyList.Rows[0].Cells[1].Value.ToString();
                                                if (CheckSyncSelect.Checked)
                                                {
                                                    MassFuntionFirstNummer.Text = ThePmxOfNow.Body[BodyCount[0]].Mass.ToString();
                                                    MassFuntionLastNummer.Text = ThePmxOfNow.Body[BodyCount[BodyCount.Count - 1]].Mass.ToString();

                                                    PositionDampingFirstNummer.Text = ThePmxOfNow.Body[BodyCount[0]].PositionDamping.ToString();
                                                    PositionDampingLastNummer.Text = ThePmxOfNow.Body[BodyCount[BodyCount.Count - 1]].PositionDamping.ToString();

                                                    RotationDampingFirstNummer.Text = ThePmxOfNow.Body[BodyCount[0]].RotationDamping.ToString();
                                                    RotationDampingLastNummer.Text = ThePmxOfNow.Body[BodyCount[BodyCount.Count - 1]].RotationDamping.ToString();

                                                    RestitutionFuntionFirstNummer.Text = ThePmxOfNow.Body[BodyCount[0]].Restitution.ToString();
                                                    RestitutionFuntionLastNummer.Text = ThePmxOfNow.Body[BodyCount[BodyCount.Count - 1]].Restitution.ToString();

                                                    FrictionFuntionFirstNummer.Text = ThePmxOfNow.Body[BodyCount[0]].Friction.ToString();
                                                    FrictionFuntionLastNummer.Text = ThePmxOfNow.Body[BodyCount[BodyCount.Count - 1]].Friction.ToString();
                                                }
                                            }
                                            catch (Exception)
                                            {
                                            }
                                        }));
                                    }
                                    break;

                                case "joint":
                                    if (JointHis.Count != GetPmx.Joint.Count)
                                    {
                                        GetPmx = ARGS.Host.Connector.Pmx.GetCurrentState();
                                        IPXPmx ThePmxOfNow = GetPmx;
                                        JointHis = new List<IPXJoint>(ThePmxOfNow.Joint);
                                        ClearList("joint");
                                        BeginInvoke(new MethodInvoker(() =>
                                        {
                                            try
                                            {
                                                var table = JointList.DataSource as DataTable;
                                                JointCount.Clear();
                                                for (int temp = 0; temp < ThePmxOfNow.Joint.Count; temp++)
                                                {
                                                    table.Rows.Add(temp, ThePmxOfNow.Joint[temp].Name);
                                                }
                                                if (table.Rows.Count == 0) return;
                                                InputJointName.Text = DeleteJointNummer.Checked ? Regex.Replace(JointList.Rows[0].Cells[1].Value.ToString(), @"\d", "") : JointList.Rows[0].Cells[1].Value.ToString();
                                                if (CheckSyncSelect.Checked)
                                                {
                                                    SpringConst_Move_FirstXNummer.Text = ThePmxOfNow.Joint[JointCount[0]].SpringConst_Move.X.ToString();
                                                    SpringConst_Move_LastXNummer.Text = ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].SpringConst_Move.X.ToString();

                                                    SpringConst_Move_FirstYNummer.Text = ThePmxOfNow.Joint[JointCount[0]].SpringConst_Move.Y.ToString();
                                                    SpringConst_Move_LastYNummer.Text = ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].SpringConst_Move.Y.ToString();

                                                    SpringConst_Move_FirstZNummer.Text = ThePmxOfNow.Joint[JointCount[0]].SpringConst_Move.Z.ToString();
                                                    SpringConst_Move_LastZNummer.Text = ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].SpringConst_Move.Z.ToString();

                                                    SpringConst_Rotate_FirstXNummer.Text = ThePmxOfNow.Joint[JointCount[0]].SpringConst_Rotate.X.ToString();
                                                    SpringConst_Rotate_LastXNummer.Text = ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].SpringConst_Rotate.X.ToString();

                                                    SpringConst_Rotate_FirstYNummer.Text = ThePmxOfNow.Joint[JointCount[0]].SpringConst_Rotate.Y.ToString();
                                                    SpringConst_Rotate_LastYNummer.Text = ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].SpringConst_Rotate.Y.ToString();

                                                    SpringConst_Rotate_FirstZNummer.Text = ThePmxOfNow.Joint[JointCount[0]].SpringConst_Rotate.Z.ToString();
                                                    SpringConst_Rotate_LastZNummer.Text = ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].SpringConst_Rotate.Z.ToString();

                                                    Limit_MoveLow_FirstXNummer.Text = ThePmxOfNow.Joint[JointCount[0]].Limit_MoveLow.X.ToString();
                                                    Limit_MoveLow_LastXNummer.Text = ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].Limit_MoveLow.X.ToString();
                                                    Limit_MoveHigh_FirstXNummer.Text = ThePmxOfNow.Joint[JointCount[0]].Limit_MoveHigh.X.ToString();
                                                    Limit_MoveHigh_LastXNummer.Text = ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].Limit_MoveHigh.X.ToString();

                                                    Limit_MoveLow_FirstYNummer.Text = ThePmxOfNow.Joint[JointCount[0]].Limit_MoveLow.Y.ToString();
                                                    Limit_MoveLow_LastYNummer.Text = ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].Limit_MoveLow.Y.ToString();
                                                    Limit_MoveHigh_FirstYNummer.Text = ThePmxOfNow.Joint[JointCount[0]].Limit_MoveHigh.Y.ToString();
                                                    Limit_MoveHigh_LastYNummer.Text = ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].Limit_MoveHigh.Y.ToString();

                                                    Limit_MoveLow_FirstZNummer.Text = ThePmxOfNow.Joint[JointCount[0]].Limit_MoveLow.Z.ToString();
                                                    Limit_MoveLow_LastZNummer.Text = ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].Limit_MoveLow.Z.ToString();
                                                    Limit_MoveHigh_FirstZNummer.Text = ThePmxOfNow.Joint[JointCount[0]].Limit_MoveHigh.Z.ToString();
                                                    Limit_MoveHigh_LastZNummer.Text = ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].Limit_MoveHigh.Z.ToString();

                                                    Limit_AngleLow_FirstXNummer.Text = (180 * ((ThePmxOfNow.Joint[JointCount[0]].Limit_AngleLow.X)) / Math.PI).ToString();
                                                    Limit_AngleLow_LastXNummer.Text = (180 * ((ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].Limit_AngleLow.X)) / Math.PI).ToString();
                                                    Limit_AngleHigh_FirstXNummer.Text = (180 * ((ThePmxOfNow.Joint[JointCount[0]].Limit_AngleHigh.X)) / Math.PI).ToString();
                                                    Limit_AngleHigh_LastXNummer.Text = (180 * ((ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].Limit_AngleHigh.X)) / Math.PI).ToString();

                                                    Limit_AngleLow_FirstYNummer.Text = (180 * ((ThePmxOfNow.Joint[JointCount[0]].Limit_AngleLow.Y)) / Math.PI).ToString();
                                                    Limit_AngleLow_LastYNummer.Text = (180 * ((ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].Limit_AngleLow.Y)) / Math.PI).ToString();
                                                    Limit_AngleHigh_FirstYNummer.Text = (180 * ((ThePmxOfNow.Joint[JointCount[0]].Limit_AngleHigh.Y)) / Math.PI).ToString();
                                                    Limit_AngleHigh_LastYNummer.Text = (180 * ((ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].Limit_AngleHigh.Y)) / Math.PI).ToString();

                                                    Limit_AngleLow_FirstZNummer.Text = (180 * ((ThePmxOfNow.Joint[JointCount[0]].Limit_AngleLow.Z)) / Math.PI).ToString();
                                                    Limit_AngleLow_LastZNummer.Text = (180 * ((ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].Limit_AngleLow.Z)) / Math.PI).ToString();
                                                    Limit_AngleHigh_FirstZNummer.Text = (180 * ((ThePmxOfNow.Joint[JointCount[0]].Limit_AngleHigh.Z)) / Math.PI).ToString();
                                                    Limit_AngleHigh_LastZNummer.Text = (180 * ((ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].Limit_AngleHigh.Z)) / Math.PI).ToString();
                                                }
                                            }
                                            catch (Exception)
                                            {
                                            }
                                        }));
                                    }
                                    break;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                } while (Run);
            }, TaskCreationOptions.LongRunning);
        }

        public void ThemeSet_Click(object sender, EventArgs e)
        {
            Theme = metroStyleManager.Theme = metroStyleManager.Theme == MetroThemeStyle.Light ? MetroThemeStyle.Dark : MetroThemeStyle.Light;
            lightordark.Text = lightordark.Text == @"光亮模式" ? "夜晚模式" : "光亮模式";
            Refresh();
        }

        public void SwichTheme_Click(object sender, EventArgs e)
        {
            bootstate.StyleState = new Random().Next(0, 13);
            if (bootstate.StyleState != 2)
            {
                Style = metroStyleManager.Style = (MetroColorStyle)bootstate.StyleState;
            }
            else
            {
                bootstate.StyleState = new Random().Next(3, 13);
                Style = metroStyleManager.Style = (MetroColorStyle)bootstate.StyleState;
            }
            foreach (var Temp in ListForm)
            {
                Temp.Style = (MetroColorStyle)bootstate.StyleState;
                Temp.Refresh();
            }
            ThreadPool.QueueUserWorkItem(new WaitCallback(Save));
            Refresh();
        }

        public void FormTopmost_CheckedChanged(object sender, EventArgs e)
        {
            if (FormTopmost.Checked)
            {
                this.TopMost = true;
                bootstate.FormTopmost = 1;
            }
            else
            {
                this.TopMost = false;
                bootstate.FormTopmost = 0;
            }
        }

        public void AutomaticRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            IPXPmx ThePmxOfNow = GetPmx;
            if (AutomaticRadioButton.Checked)
            {
                CheckSyncSelect.Visible = true;
                LockSelect.Visible = false;
                BoneList.MouseUp += GetBoneSelect;
                BodyList.MouseUp += GetBodySelect;
                JointList.MouseUp += GetJointSelect;
            }
            else
            {
                LockSelect.Checked = false;
                CheckSyncSelect.Visible = false;
                LockSelect.Visible = true;
                AutomaticRadioButton.Checked = false;
                {
                    ClearList("bone");
                    BoneList.MouseUp -= GetBoneSelect;
                }
                {
                    ClearList("body");
                    BodyList.MouseUp -= GetBodySelect;
                }
                {
                    ClearList("joint");
                    JointList.MouseUp -= GetJointSelect;
                }
            }
        }

        private void ClearList(string mode)
        {
            this.BeginInvoke(new MethodInvoker(() =>
            {
                switch (mode)
                {
                    case "bone":
                        {
                            var table = BoneList.DataSource as DataTable;
                            if (MirrorMode.Checked)
                            {
                                table.Rows.Clear();
                                table.Columns.Clear();
                                table.Columns.Add("骨骼顺序");
                                table.Columns.Add("骨骼名称");
                                table.Rows.Add();
                                table.Rows.Clear();
                            }
                            else
                            {
                                foreach (Control item in BoneList.Controls)
                                {
                                    if (item.Name == "MirrorControl")
                                    {
                                        item.Visible = false;
                                        break;
                                    }
                                }
                                table.Rows.Clear();
                                table.Columns.Clear();
                                table.Columns.Add("参照骨骼");
                                table.Columns.Add("->");
                                table.Columns.Add("镜像骨骼");
                                table.Rows.Add();
                                table.Rows.Clear();
                            }
                        }
                        break;

                    case "body":
                        {
                            var table = BodyList.DataSource as DataTable;
                            table.Rows.Clear();
                            table.Columns.Clear();
                            table.Columns.Add("刚体顺序");
                            table.Columns.Add("刚体名称");
                            table.Rows.Add();
                            table.Rows.Clear();
                        }
                        break;

                    case "joint":
                        {
                            var table = JointList.DataSource as DataTable;
                            table.Rows.Clear();
                            table.Columns.Clear();
                            table.Columns.Add("J点顺序");
                            table.Columns.Add("J点名称");
                            table.Rows.Add();
                            table.Rows.Clear();
                        }
                        break;
                }
            }));
        }

        public void GetJointSelect(object sender, MouseEventArgs e)
        {
            IPXPmx ThePmxOfNow = GetPmx;
            if (AutomaticRadioButton.Checked)
            {
                try
                {
                    var temp = new DataGridViewCell[JointList.SelectedCells.Count];
                    JointList.SelectedCells.CopyTo(temp, 0);
                    JointCount.Clear();
                    for (int i = 0; i < JointList.SelectedCells.Count; i++)
                    {
                        if (temp[i].ColumnIndex == 0)
                        {
                            JointCount.Add(Convert.ToInt32(temp[i].Value));
                        }
                        if (i == JointList.SelectedCells.Count - 1)
                        {
                            InputJointName.Text = DeleteJointNummer.Checked ? Regex.Replace(temp[i].Value.ToString(), @"\d", "") : temp[i].Value.ToString();
                        }
                        JointCount.Sort();
                        if (JointCount.Count != 0 && CheckSyncSelect.Checked)
                        {
                            SpringConst_Move_FirstXNummer.Text = ThePmxOfNow.Joint[JointCount[0]].SpringConst_Move.X.ToString();
                            SpringConst_Move_LastXNummer.Text = ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].SpringConst_Move.X.ToString();

                            SpringConst_Move_FirstYNummer.Text = ThePmxOfNow.Joint[JointCount[0]].SpringConst_Move.Y.ToString();
                            SpringConst_Move_LastYNummer.Text = ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].SpringConst_Move.Y.ToString();

                            SpringConst_Move_FirstZNummer.Text = ThePmxOfNow.Joint[JointCount[0]].SpringConst_Move.Z.ToString();
                            SpringConst_Move_LastZNummer.Text = ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].SpringConst_Move.Z.ToString();

                            SpringConst_Rotate_FirstXNummer.Text = ThePmxOfNow.Joint[JointCount[0]].SpringConst_Rotate.X.ToString();
                            SpringConst_Rotate_LastXNummer.Text = ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].SpringConst_Rotate.X.ToString();

                            SpringConst_Rotate_FirstYNummer.Text = ThePmxOfNow.Joint[JointCount[0]].SpringConst_Rotate.Y.ToString();
                            SpringConst_Rotate_LastYNummer.Text = ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].SpringConst_Rotate.Y.ToString();

                            SpringConst_Rotate_FirstZNummer.Text = ThePmxOfNow.Joint[JointCount[0]].SpringConst_Rotate.Z.ToString();
                            SpringConst_Rotate_LastZNummer.Text = ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].SpringConst_Rotate.Z.ToString();

                            Limit_MoveLow_FirstXNummer.Text = ThePmxOfNow.Joint[JointCount[0]].Limit_MoveLow.X.ToString();
                            Limit_MoveLow_LastXNummer.Text = ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].Limit_MoveLow.X.ToString();
                            Limit_MoveHigh_FirstXNummer.Text = ThePmxOfNow.Joint[JointCount[0]].Limit_MoveHigh.X.ToString();
                            Limit_MoveHigh_LastXNummer.Text = ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].Limit_MoveHigh.X.ToString();

                            Limit_MoveLow_FirstYNummer.Text = ThePmxOfNow.Joint[JointCount[0]].Limit_MoveLow.Y.ToString();
                            Limit_MoveLow_LastYNummer.Text = ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].Limit_MoveLow.Y.ToString();
                            Limit_MoveHigh_FirstYNummer.Text = ThePmxOfNow.Joint[JointCount[0]].Limit_MoveHigh.Y.ToString();
                            Limit_MoveHigh_LastYNummer.Text = ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].Limit_MoveHigh.Y.ToString();

                            Limit_MoveLow_FirstZNummer.Text = ThePmxOfNow.Joint[JointCount[0]].Limit_MoveLow.Z.ToString();
                            Limit_MoveLow_LastZNummer.Text = ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].Limit_MoveLow.Z.ToString();
                            Limit_MoveHigh_FirstZNummer.Text = ThePmxOfNow.Joint[JointCount[0]].Limit_MoveHigh.Z.ToString();
                            Limit_MoveHigh_LastZNummer.Text = ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].Limit_MoveHigh.Z.ToString();

                            Limit_AngleLow_FirstXNummer.Text = (180 * ((ThePmxOfNow.Joint[JointCount[0]].Limit_AngleLow.X)) / Math.PI).ToString();
                            Limit_AngleLow_LastXNummer.Text = (180 * ((ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].Limit_AngleLow.X)) / Math.PI).ToString();
                            Limit_AngleHigh_FirstXNummer.Text = (180 * ((ThePmxOfNow.Joint[JointCount[0]].Limit_AngleHigh.X)) / Math.PI).ToString();
                            Limit_AngleHigh_LastXNummer.Text = (180 * ((ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].Limit_AngleHigh.X)) / Math.PI).ToString();

                            Limit_AngleLow_FirstYNummer.Text = (180 * ((ThePmxOfNow.Joint[JointCount[0]].Limit_AngleLow.Y)) / Math.PI).ToString();
                            Limit_AngleLow_LastYNummer.Text = (180 * ((ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].Limit_AngleLow.Y)) / Math.PI).ToString();
                            Limit_AngleHigh_FirstYNummer.Text = (180 * ((ThePmxOfNow.Joint[JointCount[0]].Limit_AngleHigh.Y)) / Math.PI).ToString();
                            Limit_AngleHigh_LastYNummer.Text = (180 * ((ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].Limit_AngleHigh.Y)) / Math.PI).ToString();

                            Limit_AngleLow_FirstZNummer.Text = (180 * ((ThePmxOfNow.Joint[JointCount[0]].Limit_AngleLow.Z)) / Math.PI).ToString();
                            Limit_AngleLow_LastZNummer.Text = (180 * ((ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].Limit_AngleLow.Z)) / Math.PI).ToString();
                            Limit_AngleHigh_FirstZNummer.Text = (180 * ((ThePmxOfNow.Joint[JointCount[0]].Limit_AngleHigh.Z)) / Math.PI).ToString();
                            Limit_AngleHigh_LastZNummer.Text = (180 * ((ThePmxOfNow.Joint[JointCount[JointCount.Count - 1]].Limit_AngleHigh.Z)) / Math.PI).ToString();
                            ARGS.Host.Connector.View.PmxView.SetSelectedJointIndices(JointCount.ToArray());
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        public void GetBodySelect(object sender, MouseEventArgs e)
        {
            IPXPmx ThePmxOfNow = GetPmx;
            if (!AutomaticRadioButton.Checked)
            {
                /*var temp = new DataGridViewCell[BodyList.SelectedCells.Count];
                BodyList.SelectedCells.CopyTo(temp, 0);
                var bodycount = new List<int>();
                for (int i = 0; i < BodyList.SelectedCells.Count; i++)
                {
                    if (temp[i].ColumnIndex == 0)
                    {
                        bodycount.Add(Convert.ToInt32(temp[i].Value));
                    }
                }
                ARGS.Host.Connector.View.PmxView.SetSelectedBodyIndices(bodycount.ToArray());*/
            }
            else
            {
                try
                {
                    var temp = new DataGridViewCell[BodyList.SelectedCells.Count];
                    BodyList.SelectedCells.CopyTo(temp, 0);
                    BodyCount.Clear();
                    for (int i = 0; i < BodyList.SelectedCells.Count; i++)
                    {
                        if (temp[i].ColumnIndex == 0)
                        {
                            BodyCount.Add(Convert.ToInt32(temp[i].Value));
                        }
                        if (i == BodyList.SelectedCells.Count - 1)
                        {
                            InputBodyName.Text = DeleteBodyNummer.Checked ? Regex.Replace(temp[i].Value.ToString(), @"\d", "") : temp[i].Value.ToString();
                        }
                        BodyCount.Sort();
                        if (BodyCount.Count != 0 && CheckSyncSelect.Checked)
                        {
                            MassFuntionFirstNummer.Text = ThePmxOfNow.Body[BodyCount[0]].Mass.ToString();
                            MassFuntionLastNummer.Text = ThePmxOfNow.Body[BodyCount[BodyCount.Count - 1]].Mass.ToString();

                            PositionDampingFirstNummer.Text = ThePmxOfNow.Body[BodyCount[0]].PositionDamping.ToString();
                            PositionDampingLastNummer.Text = ThePmxOfNow.Body[BodyCount[BodyCount.Count - 1]].PositionDamping.ToString();

                            RotationDampingFirstNummer.Text = ThePmxOfNow.Body[BodyCount[0]].RotationDamping.ToString();
                            RotationDampingLastNummer.Text = ThePmxOfNow.Body[BodyCount[BodyCount.Count - 1]].RotationDamping.ToString();

                            RestitutionFuntionFirstNummer.Text = ThePmxOfNow.Body[BodyCount[0]].Restitution.ToString();
                            RestitutionFuntionLastNummer.Text = ThePmxOfNow.Body[BodyCount[BodyCount.Count - 1]].Restitution.ToString();

                            FrictionFuntionFirstNummer.Text = ThePmxOfNow.Body[BodyCount[0]].Friction.ToString();
                            FrictionFuntionLastNummer.Text = ThePmxOfNow.Body[BodyCount[BodyCount.Count - 1]].Friction.ToString();

                            ARGS.Host.Connector.View.PmxView.SetSelectedBodyIndices(BodyCount.ToArray());
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        public void GetBoneSelect(object sender, MouseEventArgs e)
        {
            try
            {
                var temp = new DataGridViewCell[BoneList.SelectedCells.Count];
                BoneList.SelectedCells.CopyTo(temp, 0);
                BoneCount.Clear();
                for (int i = 0; i < BoneList.SelectedCells.Count; i++)
                {
                    if (temp[i].ColumnIndex == 0)
                    {
                        BoneCount.Add(Convert.ToInt32(temp[i].Value));
                    }
                    if (i == BoneList.SelectedCells.Count - 1)
                    {
                        InputBoneName.Text = DeleteBoneNummer.Checked ? Regex.Replace(temp[i].Value.ToString(), @"\d", "") : temp[i].Value.ToString();
                    }
                    BoneCount.Sort();
                    if (BoneCount.Count != 0 && CheckSyncSelect.Checked)
                    {
                        ARGS.Host.Connector.View.PmxView.SetSelectedBoneIndices(BoneCount.ToArray());
                        ARGS.Host.Connector.View.PmxView.UpdateView();
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        #endregion

        public void ChangeBoneName_Click(object sender, EventArgs e)
        {
            if (BoneCount.Count != 0)
            {
                ThreadPool.QueueUserWorkItem((object state) =>
                {
                    try
                    {
                        var temppmx = ARGS.Host.Connector.Pmx.GetCurrentState();
                        List<ListChangeInfo> BoneChangeList = new List<ListChangeInfo>();
                        var tempString = InputBoneName.Text;
                        for (var i = 0; i < BoneCount.Count; i++)
                        {
                            temppmx.Bone[BoneCount[i]].Name = tempString + (i + 1);
                            if (!AutomaticRadioButton.Checked)
                            {
                                BoneChangeList.Add(new ListChangeInfo(i, tempString + (i + 1)));
                            }
                            else
                            {
                                BoneChangeList.Add(new ListChangeInfo(BoneCount[i], tempString + (i + 1)));
                            }
                        }
                        ThFunOfSaveToPmx(temppmx, "Bone");
                        TheFunOfChangeListShow(BoneChangeList, "Bone");
                    }
                    catch (Exception) { }
                });
            }
            else
            {
                MetroMessageBox.Show(this, "请先选择骨骼后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public class ListChangeInfo
        {
            public int i;
            public string Name;

            public ListChangeInfo(int i, string v)
            {
                this.i = i;
                Name = v;
            }
        }

        public delegate void SetTheFunOfChangeListShow(List<ListChangeInfo> boneChangeList, string mode);

        public void TheFunOfChangeListShow(List<ListChangeInfo> ChangeList, string mode)
        {
            if (InvokeRequired)
            {
                SetTheFunOfChangeListShow d = new SetTheFunOfChangeListShow(TheFunOfChangeListShow);
                Invoke(d, ChangeList, mode);
            }
            else
            {
                switch (mode)
                {
                    case "Bone":
                        foreach (ListChangeInfo Temp in ChangeList)
                        {
                            BoneList.Rows[Temp.i].Cells[1].Value = Temp.Name;
                        }
                        Progress_Spinner.Speed = 1;
                        break;

                    case "Bone2":
                        AnalyseBoneAndDeleteBoneLabel.Text = "需要删除骨骼数:" + delBoneList.Count.ToString() + "个";
                        ARGS.Host.Connector.View.PMDView.SetSelectedBoneIndices(delBoneList.ToArray());
                        Progress_Spinner.Speed = 1;
                        //      ARGS.Host.Connector.View.PMDView.UpdateModel();
                        break;

                    case "Finally":
                        Progress_Spinner.Speed = 1;
                        break;

                    case "Body":
                        foreach (ListChangeInfo Temp in ChangeList)
                        {
                            BodyList.Rows[Temp.i].Cells[1].Value = Temp.Name;
                        }
                        Progress_Spinner.Speed = 1;
                        break;

                    case "Joint":
                        foreach (ListChangeInfo Temp in ChangeList)
                        {
                            JointList.Rows[Temp.i].Cells[1].Value = Temp.Name;
                        }
                        Progress_Spinner.Speed = 1;
                        break;
                }
            }
        }

        public delegate void SetThFunOfSaveToPmx(IPXPmx temppmx, string mode);

        public void ThFunOfSaveToPmx(IPXPmx temppmx, string mode)
        {
            if (InvokeRequired)
            {
                SetThFunOfSaveToPmx d = new SetThFunOfSaveToPmx(ThFunOfSaveToPmx);
                Invoke(d, temppmx, mode);
            }
            else
            {
                switch (mode)
                {
                    case "Bone":

                        ARGS.Host.Connector.Pmx.Update(temppmx);
                        ARGS.Host.Connector.Form.UpdateList(UpdateObject.Bone);
                        ARGS.Host.Connector.View.PmxView.UpdateView();
                        ARGS.Host.Connector.View.PmxView.UpdateModel();
                        Progress_Spinner.Speed = 1;
                        break;

                    case "Body":

                        ARGS.Host.Connector.Pmx.Update(temppmx);
                        ARGS.Host.Connector.Form.UpdateList(UpdateObject.Body);
                        ARGS.Host.Connector.View.PmxView.UpdateModel();
                        ARGS.Host.Connector.View.PmxView.UpdateView();
                        Progress_Spinner.Speed = 1;
                        break;

                    case "Joint":
                        ARGS.Host.Connector.Pmx.Update(temppmx);
                        ARGS.Host.Connector.Form.UpdateList(UpdateObject.Joint);
                        ARGS.Host.Connector.View.PmxView.UpdateModel();
                        ARGS.Host.Connector.View.PmxView.UpdateView();
                        Progress_Spinner.Speed = 1;
                        break;

                    case "Vertex":
                        ARGS.Host.Connector.Pmx.Update(temppmx);
                        //args.Host.Connector.Form.UpdateList(UpdateObject.Vertex);
                        ARGS.Host.Connector.View.TransformView.UpdateView();
                        //ARGS.Host.Connector.View.TransformView.Focus();
                        break;
                }
            }
        }

        public void BoneNameCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (BoneNameCheck.CheckState == CheckState.Checked)
            {
                InputBoneName.Enabled = true;
                InputBoneName.UseCustomBackColor = false;
            }
            else
            {
                InputBoneName.Enabled = false;
                InputBoneName.UseCustomBackColor = true;
            }
            Refresh();
        }

        public void ChangeBoneFamily_Click(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem((object state) =>
            {
                if (BoneCount.Count != 0)
                {
                    if (!AutomaticRadioButton.Checked)
                    {
                        BoneCount.Clear();
                        BoneCount.AddRange(ARGS.Host.Connector.View.PmxView.GetSelectedBoneIndices());
                    }
                    else
                    {
                        BeginInvoke(new MethodInvoker(() =>
                        {
                            var temp = new DataGridViewCell[BoneList.SelectedCells.Count];
                            BoneList.SelectedCells.CopyTo(temp, 0);
                            BoneCount.Clear();
                            for (int i = 0; i < BoneList.SelectedCells.Count; i++)
                            {
                                if (temp[i].ColumnIndex == 0)
                                {
                                    BoneCount.Add(Convert.ToInt32(temp[i].Value));
                                }
                            }
                            BoneCount.Sort();
                        }));
                    }
                    IPXPmx TEMPPMX = ARGS.Host.Connector.Pmx.GetCurrentState();
                    for (int i = 0; i < BoneCount.Count; i++)
                    {
                        if (i == 0)
                        {
                            TEMPPMX.Bone[BoneCount[i]].ToBone = TEMPPMX.Bone[BoneCount[i + 1]];
                        }
                        else if (i == BoneCount.Count - 1)
                        {
                            TEMPPMX.Bone[BoneCount[i]].Parent = TEMPPMX.Bone[BoneCount[i - 1]];
                            TEMPPMX.Bone[BoneCount[i]].ToBone = null;
                        }
                        else
                        {
                            TEMPPMX.Bone[BoneCount[i]].Parent = TEMPPMX.Bone[BoneCount[i - 1]];
                            TEMPPMX.Bone[BoneCount[i]].ToBone = TEMPPMX.Bone[BoneCount[i + 1]];
                        }
                    }
                    ThFunOfSaveToPmx(TEMPPMX, "Bone");
                }
                else
                {
                    MetroMessageBox.Show(this, "请先选择骨骼后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            });
        }

        public class AnyBoneList
        {
            public IPXBone[] Bone;

            public AnyBoneList(IPXBone[] iPXBone)
            {
                Bone = iPXBone;
            }
        }

        public void AnalyseBone_Click(object sender, EventArgs e)
        {
            if (BoneCount.Count != 0)
            {
                AnalyseBoneProgressBar.Maximum = BoneCount.Count;
                AnalyseBoneProgressBar.Value = 0;
                delBoneList = new List<int>();
                Tempbone2 = new List<AnyBoneList>();
                ThreadPool.QueueUserWorkItem((object state) => { TheFunOfAnalyseBone(); });
            }
            else
            {
                MetroMessageBox.Show(this, "请先选择骨骼后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private List<int> delBoneList = new List<int>();
        private List<AnyBoneList> Tempbone2 = new List<AnyBoneList>();

        public void TheFunOfAnalyseBone()
        {
            List<IPXBone> DelBoneList = new List<IPXBone>();
            try
            {
                IPXPmx TEMPPMX = ARGS.Host.Connector.Pmx.GetCurrentState();
                int i = 0;
                List<IPXBone> Tempbone = new List<IPXBone>();
                List<IPXVertex> TempVertex;
                if (ARGS.Host.Connector.View.PMDView.GetSelectedVertexIndices().Length != 0)
                {
                    TempVertex = new List<IPXVertex>();
                    foreach (var temp in ARGS.Host.Connector.View.PMDView.GetSelectedVertexIndices())
                    {
                        TempVertex.Add(TEMPPMX.Vertex[temp]);
                    }
                }
                else
                {
                    TempVertex = new List<IPXVertex>(TEMPPMX.Vertex);
                }

                foreach (var temp in BoneCount)
                {
                    Tempbone.Add(TEMPPMX.Bone[temp]);
                }
                GetBone(Tempbone);

                foreach (var Temp in Tempbone2)
                {
                    bool Find = false;
                    for (int x = Temp.Bone.Length - 1; x >= 0; x--)
                    {
                        foreach (var VertexTemp in TEMPPMX.Vertex)
                        {
                            if (VertexTemp.Bone1 == Temp.Bone[x] || VertexTemp.Bone2 == Temp.Bone[x] || VertexTemp.Bone3 == Temp.Bone[x] || VertexTemp.Bone4 == Temp.Bone[x])
                            {
                                Find = true;
                                break;
                            }
                        }
                        if (!Find)
                        {
                            delBoneList.Add(TEMPPMX.Bone.IndexOf(Temp.Bone[x]));
                        }
                    }
                    i++;
                    BeginInvoke(new MethodInvoker(() =>
                    {
                        AnalyseBoneProgressBar.Value = i;
                    }));
                }

                /*      foreach (IPXVertex VertexTemp in TEMPPMX.Vertex)
                      {
                          foreach (var Temp in Tempbone2)
                          {
                              for (int x = Temp.Bone.Length - 1; x > 0; x--)
                              {
                                  if (VertexTemp.Bone1 == Temp.Bone[x] || VertexTemp.Bone2 == Temp.Bone[x] || VertexTemp.Bone3 == Temp.Bone[x] || VertexTemp.Bone4 == Temp.Bone[x])
                                  {
                                      Tempbone.Remove(Temp.Bone[x]);
                                      TheFunOfChangeProgressBarShow(i, "Bone");
                                      break;
                                  }
                              }
                          }
                          i++;
                      }*/

                /*    foreach (IPXVertex VertexTemp in TEMPPMX.Vertex)
                    {
                        foreach (int BoneTemp in DelBoneList)
                        {
                            if (VertexTemp.Bone1 == TEMPPMX.Bone[BoneTemp] || VertexTemp.Bone2 == TEMPPMX.Bone[BoneTemp] || VertexTemp.Bone3 == TEMPPMX.Bone[BoneTemp] || VertexTemp.Bone4 == TEMPPMX.Bone[BoneTemp])
                            {
                                DelBoneList.Remove(BoneTemp);
                                TheFunOfChangeProgressBarShow(i, "Bone");
                                break;
                            }
                        }
                        i++;
                    }*/
                TheFunOfChangeListShow(null, "Bone2");
            }
            catch (Exception)
            {
            }
        }

        private void GetBone(List<IPXBone> tempbone)
        {
            List<IPXBone> Bone = new List<IPXBone>();
            Bone.Add(tempbone[0]);
            IPXBone Tempbone = tempbone[0].ToBone;
            foreach (var temp in tempbone)
            {
                if (temp == Tempbone)
                {
                    Bone.Add(temp);
                    Tempbone = temp.ToBone;
                }
            }
            foreach (var deltemp in Bone)
            {
                tempbone.Remove(deltemp);
            }
            Tempbone2.Add(new AnyBoneList(Bone.ToArray()));
            if (tempbone.Count != 0)
            {
                GetBone(tempbone);
            }
        }

        public void StartAnalyseBoneDeleteCheck_Click(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem((object state) =>
            {
                IPXPmx TEMPPMX = ARGS.Host.Connector.Pmx.GetCurrentState();
                List<int> delList = new List<int>();
                if (!GetFormAnyDate.Checked)
                {
                    if (ARGS.Host.Connector.View.PMDView.GetSelectedBoneIndices().Length == 0)
                    {
                        return;
                    }
                    delList = new List<int>(ARGS.Host.Connector.View.PMDView.GetSelectedBoneIndices());
                }
                else
                {
                    if (delBoneList.Count == 0)
                    {
                        return;
                    }
                    delList = new List<int>(delBoneList);
                }

                List<IPXBone> BoneDelTemp = new List<IPXBone>();
                foreach (int DelTemp in delList)
                {
                    BoneDelTemp.Add(TEMPPMX.Bone[DelTemp]);
                }
                delBoneList.Clear();
                foreach (IPXBone DelTemp in BoneDelTemp)
                {
                    TEMPPMX.Bone.Remove(DelTemp);
                }
                ThFunOfSaveToPmx(TEMPPMX, "Bone");

                if (AnalyseBoneDeleteBodyAndJoointCheck.Checked)
                {
                    TEMPPMX = ARGS.Host.Connector.Pmx.GetCurrentState();
                    List<IPXBody> BodyDelList = new List<IPXBody>();
                    for (int i = 0; i < TEMPPMX.Body.Count; i++)
                    {
                        if (TEMPPMX.Body[i].Bone == null)
                        {
                            BodyDelList.Add(TEMPPMX.Body[i]);
                        }
                    }
                    foreach (IPXBody DelTemp in BodyDelList)
                    {
                        TEMPPMX.Body.Remove(DelTemp);
                    }
                    BodyDelList.Clear();
                    ARGS.Host.Connector.Pmx.Update(TEMPPMX);
                    ARGS.Host.Connector.Form.UpdateList(UpdateObject.Body);
                    TEMPPMX = ARGS.Host.Connector.Pmx.GetCurrentState();
                    List<IPXJoint> JointDelList = new List<IPXJoint>();
                    for (int i = 0; i < TEMPPMX.Joint.Count; i++)
                    {
                        if (TEMPPMX.Joint[i].BodyA == null)
                        {
                            JointDelList.Add(TEMPPMX.Joint[i]);
                        }
                        if (TEMPPMX.Joint[i].BodyB == null)
                        {
                            JointDelList.Add(TEMPPMX.Joint[i]);
                        }
                    }
                    foreach (IPXJoint DelTemp in JointDelList)
                    {
                        TEMPPMX.Joint.Remove(DelTemp);
                    }
                    JointDelList.Clear();
                    ThFunOfSaveToPmx(TEMPPMX, "Joint");
                }
            });
        }

        public void MoreThanZeroNumCheck(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\b' && e.KeyChar != '.' && e.KeyChar != '\u0016' && e.KeyChar != '\u0003')//这是允许输入退格键
            {
                if ((e.KeyChar < '0') || (e.KeyChar > '9'))//这是允许输入0-9数字
                {
                    e.Handled = true;
                }
            }
        }

        public void NumCheck(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\b' && e.KeyChar != '.' && e.KeyChar != '\u0016' && e.KeyChar != '\u0003')//这是允许输入退格键
            {
                if ((e.KeyChar < '0') || (e.KeyChar > '9'))//这是允许输入0-9数字
                {
                    if (e.KeyChar != '-')
                    {
                        e.Handled = true;
                    }
                }
            }
        }

        public void BodyNameCheck_Click(object sender, EventArgs e)
        {
            if (BodyNameCheck.CheckState == CheckState.Checked)
            {
                InputBodyName.Enabled = true;
                InputBodyName.UseCustomBackColor = false;
            }
            else
            {
                InputBodyName.Enabled = false;
                InputBodyName.UseCustomBackColor = true;
            }
            Refresh();
        }

        public void ChangeBodyName_Click(object sender, EventArgs e)
        {
            if (BodyCount.Count != 0)
            {
                ThreadPool.QueueUserWorkItem((object state) =>
                {
                    try
                    {
                        var temppmx = ARGS.Host.Connector.Pmx.GetCurrentState();
                        List<ListChangeInfo> BodyChangeList = new List<ListChangeInfo>();
                        var tempString = InputBodyName.Text;
                        for (var i = 0; i < BodyCount.Count; i++)
                        {
                            temppmx.Body[BodyCount[i]].Name = tempString + (i + 1);
                            if (!AutomaticRadioButton.Checked)
                            {
                                BodyChangeList.Add(new ListChangeInfo(i, tempString + (i + 1)));
                            }
                            else
                            {
                                BodyChangeList.Add(new ListChangeInfo(BodyCount[i], tempString + (i + 1)));
                            }
                        }
                        ThFunOfSaveToPmx(temppmx, "Body");
                        TheFunOfChangeListShow(BodyChangeList, "Body");
                    }
                    catch (Exception) { }
                });
            }
            else
            {
                MetroMessageBox.Show(this, "请先选择刚体后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public void ChangeBodyNameWithBone_Click(object sender, EventArgs e)
        {
            if (BodyCount.Count != 0)
            {
                ThreadPool.QueueUserWorkItem((object state) =>
                {
                    try
                    {
                        var temppmx = ARGS.Host.Connector.Pmx.GetCurrentState();
                        List<ListChangeInfo> BodyChangeList = new List<ListChangeInfo>();
                        var tempString = InputBodyName.Text;
                        for (var i = 0; i < BodyCount.Count; i++)
                        {
                            temppmx.Body[BodyCount[i]].Name = temppmx.Body[BodyCount[i]].Bone.Name;
                            if (!AutomaticRadioButton.Checked)
                            {
                                BodyChangeList.Add(new ListChangeInfo(i, temppmx.Body[BodyCount[i]].Bone.Name));
                            }
                            else
                            {
                                BodyChangeList.Add(new ListChangeInfo(BodyCount[i], temppmx.Body[BodyCount[i]].Bone.Name));
                            }
                        }
                        ThFunOfSaveToPmx(temppmx, "Body");
                        TheFunOfChangeListShow(BodyChangeList, "Body");
                    }
                    catch (Exception) { }
                });
            }
            else
            {
                MetroMessageBox.Show(this, "请先选择刚体后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public void DeleteUnlinkedBodies_Click(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem((object state) =>
            {
                var TEMPPMX = ARGS.Host.Connector.Pmx.GetCurrentState();
                List<IPXBody> BodyDelList = new List<IPXBody>();
                for (int i = 0; i < TEMPPMX.Body.Count; i++)
                {
                    if (TEMPPMX.Body[i].Bone == null)
                    {
                        BodyDelList.Add(TEMPPMX.Body[i]);
                    }
                }
                foreach (IPXBody DelTemp in BodyDelList)
                {
                    TEMPPMX.Body.Remove(DelTemp);
                }
                BodyDelList.Clear();
                ThFunOfSaveToPmx(TEMPPMX, "Body");
            });
            TheFunOfDeleteUnlinkedJoint();
        }

        public void TheFunOfDeleteUnlinkedJoint()
        {
            var TEMPPMX = ARGS.Host.Connector.Pmx.GetCurrentState();
            var JointDelList = new List<IPXJoint>();
            for (int i = 0; i < TEMPPMX.Joint.Count; i++)
            {
                if (TEMPPMX.Joint[i].BodyA == null)
                {
                    JointDelList.Add(TEMPPMX.Joint[i]);
                }
                if (TEMPPMX.Joint[i].BodyB == null)
                {
                    JointDelList.Add(TEMPPMX.Joint[i]);
                }
            }
            foreach (var DelTemp in JointDelList)
            {
                TEMPPMX.Joint.Remove(DelTemp);
            }
            JointDelList.Clear();
            ThFunOfSaveToPmx(TEMPPMX, "Joint");
        }

        public float[] TheFunOfMath(string Funtion, int count, float First, float Last, int Mode)
        {
            List<float> MathReturn = new List<float>();
            float A = 0;
            switch (Funtion)
            {
                case "=AX":
                    A = (Last - First) / (count - 1);
                    for (int i = 0; i < count; i++)
                    {
                        MathReturn.Add((float)Math.Round(i * A + First, 2));
                    }
                    break;

                case "=Cos(Xπ)":
                    for (int i = 0; i < count; i++)
                    {
                        MathReturn.Add((float)Math.Round((Last - First) * Math.Cos(((Math.PI / 2) / (count - 1)) * i) + First, 2));
                    }
                    break;

                case "=A/X":
                    A = (count * (Last - First)) / 4;
                    for (int i = 1; i < count + 1; i++)
                    {
                        MathReturn.Add((float)Math.Round(A / i, 2));
                    }
                    break;

                case "=-A/X+B":
                    if (First == 0)
                    { First = 0.1f; }
                    A = (count * (Last - First)) / 4;
                    for (int i = 1; i < count + 1; i++)
                    {
                        MathReturn.Add((float)Math.Round(A / i, 2));
                    }
                    MathReturn.Reverse();
                    break;

                case "=Sin(Xπ)":
                    for (int i = 0; i < count; i++)
                    {
                        MathReturn.Add((float)Math.Round((Last - First) * Math.Sin((Math.PI / (count - 1)) * i) + First, 2));
                    }
                    break;
            }
            return MathReturn.ToArray();
        }

        public void ApplyAndSetBody(object sender, EventArgs e)
        {
            IPXPmx ThePmxOfNow = GetPmx;
            if (BodyOperaMode1.Checked)
            {
                if (BodyCount.Count != 0)
                {
                    MetroTextBox NewTextBox = sender as MetroTextBox;
                    switch (NewTextBox.Name)
                    {
                        case "MassFuntionLastNummer":
                            try
                            {
                                if (MassFuntionSelect.SelectedItem.ToString() == "Bezier")
                                {
                                    bool find = false;
                                    foreach (var temp in ListForm)
                                    {
                                        if (temp.Text == "刚体质量")
                                        {
                                            find = true;
                                            temp.TopLevel = true;
                                            break;
                                        }
                                    }
                                    if (!find)
                                    {
                                        BodyBezierMode NewOpen = new BodyBezierMode();
                                        NewOpen.Text = "刚体质量";
                                        NewOpen.SetDate = new TheDataForBezier("Mass", MassFuntionFirstNummer.Text, MassFuntionLastNummer.Text, BodyCount.ToArray());
                                        NewOpen.Show(this.Owner);
                                        ListForm.Add(NewOpen);
                                    }
                                }
                                else
                                {
                                    List<float> Temp = new List<float>(TheFunOfMath(MassFuntionSelect.SelectedItem.ToString(), BodyCount.Count, float.Parse(MassFuntionFirstNummer.Text), float.Parse(MassFuntionLastNummer.Text), 0));
                                    for (int i = 0; i < BodyCount.Count; i++)
                                    {
                                        if (Temp[i] < 0)
                                        {
                                            ThePmxOfNow.Body[BodyCount[i]].Mass = -Temp[i];
                                        }
                                        else
                                        {
                                            ThePmxOfNow.Body[BodyCount[i]].Mass = Temp[i];
                                        }
                                    }
                                    ThFunOfSaveToPmx(ThePmxOfNow, "Body");
                                }
                            }
                            catch (Exception) { }
                            break;

                        case "PositionDampingLastNummer":
                            try
                            {
                                if (PositionDampingSelect.SelectedItem.ToString() == "Bezier")
                                {
                                    bool find = false;

                                    foreach (var temp in ListForm)
                                    {
                                        if (temp.Text == "刚体移动衰减")
                                        {
                                            find = true;
                                            temp.TopLevel = true;
                                            break;
                                        }
                                    }
                                    if (!find)
                                    {
                                        BodyBezierMode NewOpen = new BodyBezierMode();
                                        NewOpen.Text = "刚体移动衰减";
                                        NewOpen.SetDate = new TheDataForBezier("PositionDamping", PositionDampingFirstNummer.Text, PositionDampingLastNummer.Text, BodyCount.ToArray());
                                        NewOpen.Show(this.Owner);
                                        ListForm.Add(NewOpen);
                                    }
                                }
                                else
                                {
                                    List<float> Temp = new List<float>(TheFunOfMath(PositionDampingSelect.SelectedItem.ToString(), BodyCount.Count, float.Parse(PositionDampingFirstNummer.Text), float.Parse(PositionDampingLastNummer.Text), 0));
                                    for (int i = 0; i < BodyCount.Count; i++)
                                    {
                                        if (Temp[i] < 0)
                                        {
                                            ThePmxOfNow.Body[BodyCount[i]].PositionDamping = -Temp[i];
                                        }
                                        else
                                        {
                                            ThePmxOfNow.Body[BodyCount[i]].PositionDamping = Temp[i];
                                        }
                                    }
                                    ThFunOfSaveToPmx(ThePmxOfNow, "Body");
                                }
                            }
                            catch (Exception) { }
                            break;

                        case "RotationDampingLastNummer":
                            try
                            {
                                if (RotationDampingFuntionSelect.SelectedItem.ToString() == "Bezier")
                                {
                                    bool find = false;

                                    foreach (var temp in ListForm)
                                    {
                                        if (temp.Text == "刚体旋转衰减")
                                        {
                                            find = true;
                                            temp.TopLevel = true;
                                            break;
                                        }
                                    }
                                    if (!find)
                                    {
                                        BodyBezierMode NewOpen = new BodyBezierMode();
                                        NewOpen.Text = "刚体旋转衰减";
                                        NewOpen.SetDate = new TheDataForBezier("RotationDamping", RotationDampingFirstNummer.Text, RotationDampingLastNummer.Text, BodyCount.ToArray());
                                        NewOpen.Show(this.Owner);
                                        ListForm.Add(NewOpen);
                                    }
                                }
                                else
                                {
                                    List<float> Temp = new List<float>(TheFunOfMath(RotationDampingFuntionSelect.SelectedItem.ToString(), BodyCount.Count, float.Parse(RotationDampingFirstNummer.Text), float.Parse(RotationDampingLastNummer.Text), 0));
                                    for (int i = 0; i < BodyCount.Count; i++)
                                    {
                                        if (Temp[i] < 0)
                                        {
                                            ThePmxOfNow.Body[BodyCount[i]].RotationDamping = -Temp[i];
                                        }
                                        else
                                        {
                                            ThePmxOfNow.Body[BodyCount[i]].RotationDamping = Temp[i];
                                        }
                                    }
                                    ThFunOfSaveToPmx(ThePmxOfNow, "Body");
                                }
                            }
                            catch (Exception) { }
                            break;

                        case "RestitutionFuntionLastNummer":
                            try
                            {
                                if (RestitutionFuntionSelect.SelectedItem.ToString() == "Bezier")
                                {
                                    bool find = false;

                                    foreach (var temp in ListForm)
                                    {
                                        if (temp.Text == "刚体反应力")
                                        {
                                            find = true;
                                            temp.TopLevel = true;
                                            break;
                                        }
                                    }
                                    if (!find)
                                    {
                                        BodyBezierMode NewOpen = new BodyBezierMode();
                                        NewOpen.Text = "刚体反应力";
                                        NewOpen.SetDate = new TheDataForBezier("Restitution", RestitutionFuntionFirstNummer.Text, RestitutionFuntionLastNummer.Text, BodyCount.ToArray());

                                        NewOpen.Show(this.Owner);
                                        ListForm.Add(NewOpen);
                                    }
                                }
                                else
                                {
                                    List<float> Temp = new List<float>(TheFunOfMath(RestitutionFuntionSelect.SelectedItem.ToString(), BodyCount.Count, float.Parse(RestitutionFuntionFirstNummer.Text), float.Parse(RestitutionFuntionLastNummer.Text), 0));
                                    for (int i = 0; i < BodyCount.Count; i++)
                                    {
                                        if (Temp[i] < 0)
                                        {
                                            ThePmxOfNow.Body[BodyCount[i]].Restitution = -Temp[i];
                                        }
                                        else
                                        {
                                            ThePmxOfNow.Body[BodyCount[i]].Restitution = Temp[i];
                                        }
                                    }
                                    ThFunOfSaveToPmx(ThePmxOfNow, "Body");
                                }
                            }
                            catch (Exception) { }
                            break;

                        case "FrictionFuntionLastNummer":
                            try
                            {
                                if (FrictionFuntionSelect.SelectedItem.ToString() == "Bezier")
                                {
                                    bool find = false;

                                    foreach (var temp in ListForm)
                                    {
                                        if (temp.Text == "刚体摩擦力")
                                        {
                                            find = true;
                                            temp.TopLevel = true;
                                            break;
                                        }
                                    }
                                    if (!find)
                                    {
                                        BodyBezierMode NewOpen = new BodyBezierMode();
                                        NewOpen.Text = "刚体摩擦力";
                                        NewOpen.SetDate = new TheDataForBezier("Friction", FrictionFuntionFirstNummer.Text, FrictionFuntionLastNummer.Text, BodyCount.ToArray());

                                        NewOpen.Show(this.Owner);
                                        ListForm.Add(NewOpen);
                                    }
                                }
                                else
                                {
                                    List<float> Temp = new List<float>(TheFunOfMath(FrictionFuntionSelect.SelectedItem.ToString(), BodyCount.Count, float.Parse(FrictionFuntionFirstNummer.Text), float.Parse(FrictionFuntionLastNummer.Text), 0));
                                    for (int i = 0; i < BodyCount.Count; i++)
                                    {
                                        if (Temp[i] < 0)
                                        {
                                            ThePmxOfNow.Body[BodyCount[i]].Friction = -Temp[i];
                                        }
                                        else
                                        {
                                            ThePmxOfNow.Body[BodyCount[i]].Friction = Temp[i];
                                        }
                                    }
                                    ThFunOfSaveToPmx(ThePmxOfNow, "Body");
                                }
                            }
                            catch (Exception) { }
                            break;

                        case "RadialLastNummer":
                            try
                            {
                                if (RadialFuntionSelect.SelectedItem.ToString() == "Bezier")
                                {
                                    bool find = false;

                                    foreach (var temp in ListForm)
                                    {
                                        if (temp.Text == "刚体半径/宽")
                                        {
                                            find = true;
                                            temp.TopLevel = true;
                                            break;
                                        }
                                    }
                                    if (!find)
                                    {
                                        BodyBezierMode NewOpen = new BodyBezierMode();
                                        NewOpen.Text = "刚体半径/宽";
                                        NewOpen.SetDate = new TheDataForBezier("Radial", RadialFirstNummer.Text, RadialLastNummer.Text, BodyCount.ToArray());
                                        NewOpen.Show(this.Owner);
                                        ListForm.Add(NewOpen);
                                    }
                                }
                                else
                                {
                                    List<float> Temp = new List<float>(TheFunOfMath(RadialFuntionSelect.SelectedItem.ToString(), BodyCount.Count, float.Parse(RadialFirstNummer.Text), float.Parse(RadialLastNummer.Text), 0));
                                    for (int i = 0; i < BodyCount.Count; i++)
                                    {
                                        if (Temp[i] < 0)
                                        {
                                            ThePmxOfNow.Body[BodyCount[i]].BoxSize.X = -Temp[i];
                                        }
                                        else
                                        {
                                            ThePmxOfNow.Body[BodyCount[i]].BoxSize.X = Temp[i];
                                        }
                                    }

                                    ThFunOfSaveToPmx(ThePmxOfNow, "Body");
                                }
                            }
                            catch (Exception) { }
                            break;

                        case "HeightLastNummer":
                            try
                            {
                                if (HeightFuntionSelect.SelectedItem.ToString() == "Bezier")
                                {
                                    bool find = false;

                                    foreach (var temp in ListForm)
                                    {
                                        if (temp.Text == "刚体高度")
                                        {
                                            find = true;
                                            temp.TopLevel = true;
                                            break;
                                        }
                                    }
                                    if (!find)
                                    {
                                        BodyBezierMode NewOpen = new BodyBezierMode();
                                        NewOpen.Text = "刚体高度";
                                        NewOpen.SetDate = new TheDataForBezier("Height", HeightFirstNummer.Text, HeightLastNummer.Text, BodyCount.ToArray());
                                        NewOpen.Show(this.Owner);
                                        ListForm.Add(NewOpen);
                                    }
                                }
                                else
                                {
                                    List<float> Temp = new List<float>(TheFunOfMath(HeightFuntionSelect.SelectedItem.ToString(), BodyCount.Count, float.Parse(HeightFirstNummer.Text), float.Parse(HeightLastNummer.Text), 0));
                                    for (int i = 0; i < BodyCount.Count; i++)
                                    {
                                        if (Temp[i] < 0)
                                        {
                                            ThePmxOfNow.Body[BodyCount[i]].BoxSize.Y = -Temp[i];
                                        }
                                        else
                                        {
                                            ThePmxOfNow.Body[BodyCount[i]].BoxSize.Y = Temp[i];
                                        }
                                    }

                                    ThFunOfSaveToPmx(ThePmxOfNow, "Body");
                                }
                            }
                            catch (Exception) { }
                            break;

                        case "DepthLastNummer":
                            try
                            {
                                if (DepthFuntionSelect.SelectedItem.ToString() == "Bezier")
                                {
                                    bool find = false;

                                    foreach (var temp in ListForm)
                                    {
                                        if (temp.Text == "刚体深度")
                                        {
                                            find = true;
                                            temp.TopLevel = true;
                                            break;
                                        }
                                    }
                                    if (!find)
                                    {
                                        BodyBezierMode NewOpen = new BodyBezierMode();
                                        NewOpen.Text = "刚体深度";
                                        NewOpen.SetDate = new TheDataForBezier("Depth", DepthFirstNummer.Text, DepthLastNummer.Text, BodyCount.ToArray());
                                        NewOpen.Show(this.Owner);
                                        ListForm.Add(NewOpen);
                                    }
                                }
                                else
                                {
                                    List<float> Temp = new List<float>(TheFunOfMath(DepthFuntionSelect.SelectedItem.ToString(), BodyCount.Count, float.Parse(DepthFirstNummer.Text), float.Parse(DepthLastNummer.Text), 0));
                                    for (int i = 0; i < BodyCount.Count; i++)
                                    {
                                        if (Temp[i] < 0)
                                        {
                                            ThePmxOfNow.Body[BodyCount[i]].BoxSize.Z = -Temp[i];
                                        }
                                        else
                                        {
                                            ThePmxOfNow.Body[BodyCount[i]].BoxSize.Z = Temp[i];
                                        }
                                    }

                                    ThFunOfSaveToPmx(ThePmxOfNow, "Body");
                                }
                            }
                            catch (Exception) { }
                            break;
                    }
                    ARGS.Host.Connector.View.PmxView.SetSelectedBodyIndices(new int[] { BodyCount[0] });
                }
                else
                {
                    MetroMessageBox.Show(this, "请先选择刚体后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
            else if (BodyOperaMode2.Checked)
            {
                if (BodyListForOpera.Count != 0)
                {
                    MetroTextBox NewTextBox = sender as MetroTextBox;
                    if (BodyListForOpera.Count != 0)
                    {
                        switch (NewTextBox.Name)
                        {
                            case "MassFuntionLastNummer":
                                try
                                {
                                    if (MassFuntionSelect.SelectedItem.ToString() == "Bezier")
                                    {
                                        bool find = false;
                                        foreach (var temp in ListForm)
                                        {
                                            if (temp.Text == "刚体质量,批处理模式")
                                            {
                                                find = true;
                                                temp.TopLevel = true;
                                                break;
                                            }
                                        }
                                        if (!find)
                                        {
                                            BodyBezierMode NewOpen = new BodyBezierMode();
                                            NewOpen.Text = "刚体质量,批处理模式";
                                            NewOpen.SetDate = new TheDataForBezier("Mass", MassFuntionFirstNummer.Text, MassFuntionLastNummer.Text, BodyListForOpera.ToArray());
                                            NewOpen.SetDate.UseMode = 1;
                                            NewOpen.Show(this.Owner);
                                            ListForm.Add(NewOpen);
                                        }
                                    }
                                    else
                                    {
                                        foreach (OperaList BodyTemp in BodyListForOpera)
                                        {
                                            List<float> Temp = new List<float>(TheFunOfMath(MassFuntionSelect.SelectedItem.ToString(), BodyTemp.Count.Count, float.Parse(MassFuntionFirstNummer.Text), float.Parse(MassFuntionLastNummer.Text), 0));
                                            for (int i = 0; i < BodyTemp.Count.Count; i++)
                                            {
                                                if (Temp[i] < 0)
                                                {
                                                    ThePmxOfNow.Body[BodyTemp.Count[i]].Mass = -Temp[i];
                                                }
                                                else
                                                {
                                                    ThePmxOfNow.Body[BodyTemp.Count[i]].Mass = Temp[i];
                                                }
                                            }
                                            ThFunOfSaveToPmx(ThePmxOfNow, "Body");
                                        }
                                    }
                                }
                                catch (Exception) { }
                                break;

                            case "PositionDampingLastNummer":
                                try
                                {
                                    if (PositionDampingSelect.SelectedItem.ToString() == "Bezier")
                                    {
                                        bool find = false;
                                        foreach (var temp in ListForm)
                                        {
                                            if (temp.Text == "刚体移动衰减,批处理模式")
                                            {
                                                find = true;
                                                temp.TopLevel = true;
                                                break;
                                            }
                                        }
                                        if (!find)
                                        {
                                            BodyBezierMode NewOpen = new BodyBezierMode();
                                            NewOpen.Text = "刚体移动衰减,批处理模式";
                                            NewOpen.SetDate = new TheDataForBezier("PositionDamping", PositionDampingFirstNummer.Text, PositionDampingLastNummer.Text, BodyListForOpera.ToArray());
                                            NewOpen.SetDate.UseMode = 1;
                                            NewOpen.Show(this.Owner);
                                            ListForm.Add(NewOpen);
                                        }
                                    }
                                    else
                                    {
                                        foreach (OperaList BodyTemp in BodyListForOpera)
                                        {
                                            List<float> Temp = new List<float>(TheFunOfMath(PositionDampingSelect.SelectedItem.ToString(), BodyTemp.Count.Count, float.Parse(PositionDampingFirstNummer.Text), float.Parse(PositionDampingLastNummer.Text), 0));
                                            for (int i = 0; i < BodyTemp.Count.Count; i++)
                                            {
                                                if (Temp[i] < 0)
                                                {
                                                    ThePmxOfNow.Body[BodyTemp.Count[i]].PositionDamping = -Temp[i];
                                                }
                                                else
                                                {
                                                    ThePmxOfNow.Body[BodyTemp.Count[i]].PositionDamping = Temp[i];
                                                }
                                            }
                                            ThFunOfSaveToPmx(ThePmxOfNow, "Body");
                                        }
                                    }
                                }
                                catch (Exception) { }
                                break;

                            case "RotationDampingLastNummer":
                                try
                                {
                                    if (RotationDampingFuntionSelect.SelectedItem.ToString() == "Bezier")
                                    {
                                        bool find = false;
                                        foreach (var temp in ListForm)
                                        {
                                            if (temp.Text == "刚体旋转衰减,批处理模式")
                                            {
                                                find = true;
                                                temp.TopLevel = true;
                                                break;
                                            }
                                        }
                                        if (!find)
                                        {
                                            BodyBezierMode NewOpen = new BodyBezierMode();
                                            NewOpen.Text = "刚体旋转衰减,批处理模式";
                                            NewOpen.SetDate = new TheDataForBezier("RotationDamping", RotationDampingFirstNummer.Text, RotationDampingLastNummer.Text, BodyListForOpera.ToArray());
                                            NewOpen.SetDate.UseMode = 1;

                                            NewOpen.Show(this.Owner);
                                            ListForm.Add(NewOpen);
                                        }
                                    }
                                    else
                                    {
                                        foreach (OperaList BodyTemp in BodyListForOpera)
                                        {
                                            List<float> Temp = new List<float>(TheFunOfMath(RotationDampingFuntionSelect.SelectedItem.ToString(), BodyTemp.Count.Count, float.Parse(RotationDampingFirstNummer.Text), float.Parse(RotationDampingLastNummer.Text), 0));
                                            for (int i = 0; i < BodyTemp.Count.Count; i++)
                                            {
                                                if (Temp[i] < 0)
                                                {
                                                    ThePmxOfNow.Body[BodyTemp.Count[i]].RotationDamping = -Temp[i];
                                                }
                                                else
                                                {
                                                    ThePmxOfNow.Body[BodyTemp.Count[i]].RotationDamping = Temp[i];
                                                }
                                            }
                                            ThFunOfSaveToPmx(ThePmxOfNow, "Body");
                                        }
                                    }
                                }
                                catch (Exception) { }
                                break;

                            case "RestitutionFuntionLastNummer":
                                try
                                {
                                    if (RestitutionFuntionSelect.SelectedItem.ToString() == "Bezier")
                                    {
                                        bool find = false;
                                        foreach (var temp in ListForm)
                                        {
                                            if (temp.Text == "刚体反应力,批处理模式")
                                            {
                                                find = true;
                                                temp.TopLevel = true;
                                                break;
                                            }
                                        }
                                        if (!find)
                                        {
                                            BodyBezierMode NewOpen = new BodyBezierMode();
                                            NewOpen.Text = "刚体反应力,批处理模式";
                                            NewOpen.SetDate = new TheDataForBezier("Restitution", RestitutionFuntionFirstNummer.Text, RestitutionFuntionLastNummer.Text, BodyListForOpera.ToArray());
                                            NewOpen.SetDate.UseMode = 1;
                                            NewOpen.Show(this.Owner);
                                            ListForm.Add(NewOpen);
                                        }
                                    }
                                    else
                                    {
                                        foreach (OperaList BodyTemp in BodyListForOpera)
                                        {
                                            List<float> Temp = new List<float>(TheFunOfMath(RestitutionFuntionSelect.SelectedItem.ToString(), BodyTemp.Count.Count, float.Parse(RestitutionFuntionFirstNummer.Text), float.Parse(RestitutionFuntionLastNummer.Text), 0));
                                            for (int i = 0; i < BodyTemp.Count.Count; i++)
                                            {
                                                try
                                                {
                                                    if (Temp[i] < 0)
                                                    {
                                                        ThePmxOfNow.Body[BodyTemp.Count[i]].Restitution = -Temp[i];
                                                    }
                                                    else
                                                    {
                                                        ThePmxOfNow.Body[BodyTemp.Count[i]].Restitution = Temp[i];
                                                    }
                                                }
                                                catch (Exception)
                                                {
                                                    Console.WriteLine();
                                                }
                                            }
                                            ThFunOfSaveToPmx(ThePmxOfNow, "Body");
                                        }
                                    }
                                }
                                catch (Exception) { }
                                break;

                            case "FrictionFuntionLastNummer":
                                try
                                {
                                    if (FrictionFuntionSelect.SelectedItem.ToString() == "Bezier")
                                    {
                                        bool find = false;
                                        foreach (var temp in ListForm)
                                        {
                                            if (temp.Text == "刚体摩擦力,批处理模式")
                                            {
                                                find = true;
                                                temp.TopLevel = true;
                                                break;
                                            }
                                        }
                                        if (!find)
                                        {
                                            BodyBezierMode NewOpen = new BodyBezierMode();
                                            NewOpen.Text = "刚体摩擦力,批处理模式";
                                            NewOpen.SetDate = new TheDataForBezier("Friction", FrictionFuntionFirstNummer.Text, FrictionFuntionLastNummer.Text, BodyListForOpera.ToArray());
                                            NewOpen.SetDate.UseMode = 1;
                                            NewOpen.Show(this.Owner);
                                            ListForm.Add(NewOpen);
                                        }
                                    }
                                    else
                                    {
                                        foreach (OperaList BodyTemp in BodyListForOpera)
                                        {
                                            List<float> Temp = new List<float>(TheFunOfMath(FrictionFuntionSelect.SelectedItem.ToString(), BodyTemp.Count.Count, float.Parse(FrictionFuntionFirstNummer.Text), float.Parse(FrictionFuntionLastNummer.Text), 0));
                                            for (int i = 0; i < BodyTemp.Count.Count; i++)
                                            {
                                                if (Temp[i] < 0)
                                                {
                                                    ThePmxOfNow.Body[BodyTemp.Count[i]].Friction = -Temp[i];
                                                }
                                                else
                                                {
                                                    ThePmxOfNow.Body[BodyTemp.Count[i]].Friction = Temp[i];
                                                }
                                            }
                                            ThFunOfSaveToPmx(ThePmxOfNow, "Body");
                                        }
                                    }
                                }
                                catch (Exception) { }
                                break;

                            case "RadialLastNummer":
                                try
                                {
                                    if (RadialFuntionSelect.SelectedItem.ToString() == "Bezier")
                                    {
                                        bool find = false;
                                        foreach (var temp in ListForm)
                                        {
                                            if (temp.Text == "刚体半径/宽,批处理模式")
                                            {
                                                find = true;
                                                temp.TopLevel = true;
                                                break;
                                            }
                                        }
                                        if (!find)
                                        {
                                            BodyBezierMode NewOpen = new BodyBezierMode();
                                            NewOpen.Text = "刚体半径/宽,批处理模式";
                                            NewOpen.SetDate = new TheDataForBezier("Radial", RadialFirstNummer.Text, RadialLastNummer.Text, BodyListForOpera.ToArray());
                                            NewOpen.SetDate.UseMode = 1;
                                            NewOpen.Show(this.Owner);
                                            ListForm.Add(NewOpen);
                                        }
                                    }
                                    else
                                    {
                                        foreach (OperaList BodyTemp in BodyListForOpera)
                                        {
                                            List<float> Temp = new List<float>(TheFunOfMath(RadialFuntionSelect.SelectedItem.ToString(), BodyTemp.Count.Count, float.Parse(RadialFirstNummer.Text), float.Parse(RadialLastNummer.Text), 0));
                                            for (int i = 0; i < BodyTemp.Count.Count; i++)
                                            {
                                                if (Temp[i] < 0)
                                                {
                                                    ThePmxOfNow.Body[BodyTemp.Count[i]].BoxSize.X = -Temp[i];
                                                }
                                                else
                                                {
                                                    ThePmxOfNow.Body[BodyTemp.Count[i]].BoxSize.X = Temp[i];
                                                }
                                            }
                                            ThFunOfSaveToPmx(ThePmxOfNow, "Body");
                                        }
                                    }
                                }
                                catch (Exception) { }
                                break;

                            case "HeightLastNummer":
                                try
                                {
                                    if (HeightFuntionSelect.SelectedItem.ToString() == "Bezier")
                                    {
                                        bool find = false;
                                        foreach (var temp in ListForm)
                                        {
                                            if (temp.Text == "刚体高度,批处理模式")
                                            {
                                                find = true;
                                                temp.TopLevel = true;
                                                break;
                                            }
                                        }
                                        if (!find)
                                        {
                                            BodyBezierMode NewOpen = new BodyBezierMode();
                                            NewOpen.Text = "刚体高度,批处理模式";
                                            NewOpen.SetDate = new TheDataForBezier("Height", HeightFirstNummer.Text, HeightLastNummer.Text, BodyListForOpera.ToArray());
                                            NewOpen.SetDate.UseMode = 1;
                                            NewOpen.Show(this.Owner);
                                            ListForm.Add(NewOpen);
                                        }
                                    }
                                    else
                                    {
                                        foreach (OperaList BodyTemp in BodyListForOpera)
                                        {
                                            List<float> Temp = new List<float>(TheFunOfMath(HeightFuntionSelect.SelectedItem.ToString(), BodyTemp.Count.Count, float.Parse(HeightFirstNummer.Text), float.Parse(HeightLastNummer.Text), 0));
                                            for (int i = 0; i < BodyTemp.Count.Count; i++)
                                            {
                                                if (Temp[i] < 0)
                                                {
                                                    ThePmxOfNow.Body[BodyTemp.Count[i]].BoxSize.Y = -Temp[i];
                                                }
                                                else
                                                {
                                                    ThePmxOfNow.Body[BodyTemp.Count[i]].BoxSize.Y = Temp[i];
                                                }
                                            }
                                            ThFunOfSaveToPmx(ThePmxOfNow, "Body");
                                        }
                                    }
                                }
                                catch (Exception) { }
                                break;

                            case "DepthLastNummer":
                                try
                                {
                                    if (DepthFuntionSelect.SelectedItem.ToString() == "Bezier")
                                    {
                                        bool find = false;
                                        foreach (var temp in ListForm)
                                        {
                                            if (temp.Text == "刚体深度,批处理模式")
                                            {
                                                find = true;
                                                temp.TopLevel = true;
                                                break;
                                            }
                                        }
                                        if (!find)
                                        {
                                            BodyBezierMode NewOpen = new BodyBezierMode();
                                            NewOpen.Text = "刚体深度,批处理模式";
                                            NewOpen.SetDate = new TheDataForBezier("Depth", DepthFirstNummer.Text, DepthLastNummer.Text, BodyListForOpera.ToArray());
                                            NewOpen.SetDate.UseMode = 1;
                                            NewOpen.Show(this.Owner);
                                            ListForm.Add(NewOpen);
                                        }
                                    }
                                    else
                                    {
                                        foreach (OperaList BodyTemp in BodyListForOpera)
                                        {
                                            List<float> Temp = new List<float>(TheFunOfMath(DepthFuntionSelect.SelectedItem.ToString(), BodyTemp.Count.Count, float.Parse(DepthFirstNummer.Text), float.Parse(DepthLastNummer.Text), 0));
                                            for (int i = 0; i < BodyTemp.Count.Count; i++)
                                            {
                                                if (Temp[i] < 0)
                                                {
                                                    ThePmxOfNow.Body[BodyTemp.Count[i]].BoxSize.Z = -Temp[i];
                                                }
                                                else
                                                {
                                                    ThePmxOfNow.Body[BodyTemp.Count[i]].BoxSize.Z = Temp[i];
                                                }
                                            }
                                            ThFunOfSaveToPmx(ThePmxOfNow, "Body");
                                        }
                                    }
                                }
                                catch (Exception) { }
                                break;
                        }
                    }
                    else
                    {
                        MetroMessageBox.Show(this, "请先添加列表后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MetroMessageBox.Show(this, "请先选择刚体后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
            else if (BodyOperaMode3.Checked)
            {
                if (BodyListForOpera.Count != 0)
                {
                    MetroTextBox NewTextBox = sender as MetroTextBox;
                    if (BodyListForOpera.Count != 0)
                    {
                        switch (NewTextBox.Name)
                        {
                            case "MassFuntionLastNummer":
                                try
                                {
                                    if (MassFuntionSelect.SelectedItem.ToString() == "Bezier")
                                    {
                                        bool find = false;
                                        foreach (var temp in ListForm)
                                        {
                                            if (temp.Text == "刚体质量,块处理模式")
                                            {
                                                find = true;
                                                temp.TopLevel = true;
                                                break;
                                            }
                                        }
                                        if (!find)
                                        {
                                            BodyBezierMode NewOpen = new BodyBezierMode();
                                            NewOpen.Text = "刚体质量,块处理模式";
                                            NewOpen.SetDate = new TheDataForBezier("Mass", MassFuntionFirstNummer.Text, MassFuntionLastNummer.Text, BodyListForOpera.ToArray());
                                            NewOpen.SetDate.ItemCount = new int[BodyListForOpera.Count];
                                            for (int x = 0; x < BodyListForOpera.Count; x++)
                                            {
                                                try
                                                {
                                                    NewOpen.SetDate.ItemCount[x] = BodyListForOpera[x].Count[0];
                                                }
                                                catch (Exception)
                                                {
                                                    NewOpen.SetDate.ItemCount[x] = 0;
                                                }
                                            }
                                            NewOpen.SetDate.UseMode = 2;
                                            NewOpen.Show(this.Owner);
                                            ListForm.Add(NewOpen);
                                        }
                                    }
                                    else
                                    {
                                        List<float> Temp = new List<float>(TheFunOfMath(MassFuntionSelect.SelectedItem.ToString(), BodyListForOpera.Count, float.Parse(MassFuntionFirstNummer.Text), float.Parse(MassFuntionLastNummer.Text), 0));
                                        for (int i = 0; i < BodyListForOpera.Count; i++)
                                        {
                                            foreach (int BodyTemp in BodyListForOpera[i].Count)
                                            {
                                                if (Temp[i] < 0)
                                                {
                                                    ThePmxOfNow.Body[BodyTemp].Mass = -Temp[i];
                                                }
                                                else
                                                {
                                                    ThePmxOfNow.Body[BodyTemp].Mass = Temp[i];
                                                }
                                            }
                                        }
                                        ThFunOfSaveToPmx(ThePmxOfNow, "Body");
                                    }
                                }
                                catch (Exception) { }
                                break;

                            case "PositionDampingLastNummer":
                                try
                                {
                                    if (PositionDampingSelect.SelectedItem.ToString() == "Bezier")
                                    {
                                        bool find = false;
                                        foreach (var temp in ListForm)
                                        {
                                            if (temp.Text == "刚体移动衰减,块处理模式")
                                            {
                                                find = true;
                                                temp.TopLevel = true;
                                                break;
                                            }
                                        }
                                        if (!find)
                                        {
                                            BodyBezierMode NewOpen = new BodyBezierMode();
                                            NewOpen.Text = "刚体移动衰减,块处理模式";
                                            NewOpen.SetDate = new TheDataForBezier("PositionDamping", PositionDampingFirstNummer.Text, PositionDampingLastNummer.Text, BodyListForOpera.ToArray());
                                            NewOpen.SetDate.ItemCount = new int[BodyListForOpera.Count];
                                            for (int x = 0; x < BodyListForOpera.Count; x++)
                                            {
                                                try
                                                {
                                                    NewOpen.SetDate.ItemCount[x] = BodyListForOpera[x].Count[0];
                                                }
                                                catch (Exception)
                                                {
                                                    NewOpen.SetDate.ItemCount[x] = 0;
                                                }
                                            }
                                            NewOpen.SetDate.UseMode = 2;
                                            NewOpen.Show(this.Owner);
                                            ListForm.Add(NewOpen);
                                        }
                                    }
                                    else
                                    {
                                        List<float> Temp = new List<float>(TheFunOfMath(PositionDampingSelect.SelectedItem.ToString(), BodyListForOpera.Count, float.Parse(PositionDampingFirstNummer.Text), float.Parse(PositionDampingLastNummer.Text), 0));
                                        for (int i = 0; i < BodyListForOpera.Count; i++)
                                        {
                                            foreach (int BodyTemp in BodyListForOpera[i].Count)
                                            {
                                                if (Temp[i] < 0)
                                                {
                                                    ThePmxOfNow.Body[BodyTemp].PositionDamping = -Temp[i];
                                                }
                                                else
                                                {
                                                    ThePmxOfNow.Body[BodyTemp].PositionDamping = Temp[i];
                                                }
                                            }
                                        }
                                        ThFunOfSaveToPmx(ThePmxOfNow, "Body");
                                    }
                                }
                                catch (Exception) { }
                                break;

                            case "RotationDampingLastNummer":
                                try
                                {
                                    if (RotationDampingFuntionSelect.SelectedItem.ToString() == "Bezier")
                                    {
                                        bool find = false;
                                        foreach (var temp in ListForm)
                                        {
                                            if (temp.Text == "刚体旋转衰减,块处理模式")
                                            {
                                                find = true;
                                                temp.TopLevel = true;
                                                break;
                                            }
                                        }
                                        if (!find)
                                        {
                                            BodyBezierMode NewOpen = new BodyBezierMode();
                                            NewOpen.Text = "刚体旋转衰减,块处理模式";
                                            NewOpen.SetDate = new TheDataForBezier("RotationDamping", RotationDampingFirstNummer.Text, RotationDampingLastNummer.Text, BodyListForOpera.ToArray());
                                            NewOpen.SetDate.ItemCount = new int[BodyListForOpera.Count];
                                            for (int x = 0; x < BodyListForOpera.Count; x++)
                                            {
                                                try
                                                {
                                                    NewOpen.SetDate.ItemCount[x] = BodyListForOpera[x].Count[0];
                                                }
                                                catch (Exception)
                                                {
                                                    NewOpen.SetDate.ItemCount[x] = 0;
                                                }
                                            }
                                            NewOpen.SetDate.UseMode = 2;
                                            NewOpen.Show(this.Owner);
                                            ListForm.Add(NewOpen);
                                        }
                                    }
                                    else
                                    {
                                        List<float> Temp = new List<float>(TheFunOfMath(RotationDampingFuntionSelect.SelectedItem.ToString(), BodyListForOpera.Count, float.Parse(RotationDampingFirstNummer.Text), float.Parse(RotationDampingLastNummer.Text), 0));
                                        for (int i = 0; i < BodyListForOpera.Count; i++)
                                        {
                                            foreach (int BodyTemp in BodyListForOpera[i].Count)
                                            {
                                                if (Temp[i] < 0)
                                                {
                                                    ThePmxOfNow.Body[BodyTemp].RotationDamping = -Temp[i];
                                                }
                                                else
                                                {
                                                    ThePmxOfNow.Body[BodyTemp].RotationDamping = Temp[i];
                                                }
                                            }
                                        }
                                        ThFunOfSaveToPmx(ThePmxOfNow, "Body");
                                    }
                                }
                                catch (Exception) { }
                                break;

                            case "RestitutionFuntionLastNummer":
                                try
                                {
                                    if (RestitutionFuntionSelect.SelectedItem.ToString() == "Bezier")
                                    {
                                        bool find = false;
                                        foreach (var temp in ListForm)
                                        {
                                            if (temp.Text == "刚体反应力,块处理模式")
                                            {
                                                find = true;
                                                temp.TopLevel = true;
                                                break;
                                            }
                                        }
                                        if (!find)
                                        {
                                            BodyBezierMode NewOpen = new BodyBezierMode();
                                            NewOpen.Text = "刚体反应力,块处理模式";
                                            NewOpen.SetDate = new TheDataForBezier("Restitution", RestitutionFuntionFirstNummer.Text, RestitutionFuntionLastNummer.Text, BodyListForOpera.ToArray());
                                            NewOpen.SetDate.ItemCount = new int[BodyListForOpera.Count];
                                            for (int x = 0; x < BodyListForOpera.Count; x++)
                                            {
                                                try
                                                {
                                                    NewOpen.SetDate.ItemCount[x] = BodyListForOpera[x].Count[0];
                                                }
                                                catch (Exception)
                                                {
                                                    NewOpen.SetDate.ItemCount[x] = 0;
                                                }
                                            }
                                            NewOpen.SetDate.UseMode = 2;
                                            NewOpen.Show(this.Owner);
                                            ListForm.Add(NewOpen);
                                        }
                                    }
                                    else
                                    {
                                        List<float> Temp = new List<float>(TheFunOfMath(RestitutionFuntionSelect.SelectedItem.ToString(), BodyListForOpera.Count, float.Parse(RestitutionFuntionFirstNummer.Text), float.Parse(RestitutionFuntionLastNummer.Text), 0));
                                        for (int i = 0; i < BodyListForOpera.Count; i++)
                                        {
                                            foreach (int BodyTemp in BodyListForOpera[i].Count)
                                            {
                                                if (Temp[i] < 0)
                                                {
                                                    ThePmxOfNow.Body[BodyTemp].Restitution = -Temp[i];
                                                }
                                                else
                                                {
                                                    ThePmxOfNow.Body[BodyTemp].Restitution = Temp[i];
                                                }
                                            }
                                        }
                                        ThFunOfSaveToPmx(ThePmxOfNow, "Body");
                                    }
                                }
                                catch (Exception) { }
                                break;

                            case "FrictionFuntionLastNummer":
                                try
                                {
                                    if (FrictionFuntionSelect.SelectedItem.ToString() == "Bezier")
                                    {
                                        bool find = false;
                                        foreach (var temp in ListForm)
                                        {
                                            if (temp.Text == "刚体摩擦力,块处理模式")
                                            {
                                                find = true;
                                                temp.TopLevel = true;
                                                break;
                                            }
                                        }
                                        if (!find)
                                        {
                                            BodyBezierMode NewOpen = new BodyBezierMode();
                                            NewOpen.Text = "刚体摩擦力,块处理模式";
                                            NewOpen.SetDate = new TheDataForBezier("Friction", FrictionFuntionFirstNummer.Text, FrictionFuntionLastNummer.Text, BodyListForOpera.ToArray());
                                            NewOpen.SetDate.ItemCount = new int[BodyListForOpera.Count];
                                            for (int x = 0; x < BodyListForOpera.Count; x++)
                                            {
                                                try
                                                {
                                                    NewOpen.SetDate.ItemCount[x] = BodyListForOpera[x].Count[0];
                                                }
                                                catch (Exception)
                                                {
                                                    NewOpen.SetDate.ItemCount[x] = 0;
                                                }
                                            }
                                            NewOpen.SetDate.UseMode = 2;
                                            NewOpen.Show(this.Owner);
                                            ListForm.Add(NewOpen);
                                        }
                                    }
                                    else
                                    {
                                        List<float> Temp = new List<float>(TheFunOfMath(FrictionFuntionSelect.SelectedItem.ToString(), BodyListForOpera.Count, float.Parse(FrictionFuntionFirstNummer.Text), float.Parse(FrictionFuntionLastNummer.Text), 0));
                                        for (int i = 0; i < BodyListForOpera.Count; i++)
                                        {
                                            foreach (int BodyTemp in BodyListForOpera[i].Count)
                                            {
                                                if (Temp[i] < 0)
                                                {
                                                    ThePmxOfNow.Body[BodyTemp].Friction = -Temp[i];
                                                }
                                                else
                                                {
                                                    ThePmxOfNow.Body[BodyTemp].Friction = Temp[i];
                                                }
                                            }
                                        }
                                        ThFunOfSaveToPmx(ThePmxOfNow, "Body");
                                    }
                                }
                                catch (Exception) { }
                                break;

                            case "RadialLastNummer":
                                try
                                {
                                    if (RadialFuntionSelect.SelectedItem.ToString() == "Bezier")
                                    {
                                        bool find = false;
                                        foreach (var temp in ListForm)
                                        {
                                            if (temp.Text == "刚体半径/宽,块处理模式")
                                            {
                                                find = true;
                                                temp.TopLevel = true;
                                                break;
                                            }
                                        }
                                        if (!find)
                                        {
                                            BodyBezierMode NewOpen = new BodyBezierMode();
                                            NewOpen.Text = "刚体半径/宽,块处理模式";
                                            NewOpen.SetDate = new TheDataForBezier("Radial", RadialFirstNummer.Text, RadialLastNummer.Text, BodyListForOpera.ToArray());
                                            NewOpen.SetDate.ItemCount = new int[BodyListForOpera.Count];
                                            for (int x = 0; x < BodyListForOpera.Count; x++)
                                            {
                                                try
                                                {
                                                    NewOpen.SetDate.ItemCount[x] = BodyListForOpera[x].Count[0];
                                                }
                                                catch (Exception)
                                                {
                                                    NewOpen.SetDate.ItemCount[x] = 0;
                                                }
                                            }
                                            NewOpen.SetDate.UseMode = 2;
                                            NewOpen.Show(this.Owner);
                                            ListForm.Add(NewOpen);
                                        }
                                    }
                                    else
                                    {
                                        List<float> Temp = new List<float>(TheFunOfMath(RadialFuntionSelect.SelectedItem.ToString(), BodyListForOpera.Count, float.Parse(RadialFirstNummer.Text), float.Parse(RadialLastNummer.Text), 0));
                                        for (int i = 0; i < BodyListForOpera.Count; i++)
                                        {
                                            foreach (int BodyTemp in BodyListForOpera[i].Count)
                                            {
                                                if (Temp[i] < 0)
                                                {
                                                    ThePmxOfNow.Body[BodyTemp].BoxSize.X = -Temp[i];
                                                }
                                                else
                                                {
                                                    ThePmxOfNow.Body[BodyTemp].BoxSize.X = Temp[i];
                                                }
                                            }
                                        }
                                        ThFunOfSaveToPmx(ThePmxOfNow, "Body");
                                    }
                                }
                                catch (Exception) { }
                                break;

                            case "HeightLastNummer":
                                try
                                {
                                    if (HeightFuntionSelect.SelectedItem.ToString() == "Bezier")
                                    {
                                        bool find = false;
                                        foreach (var temp in ListForm)
                                        {
                                            if (temp.Text == "刚体高度,块处理模式")
                                            {
                                                find = true;
                                                temp.TopLevel = true;
                                                break;
                                            }
                                        }
                                        if (!find)
                                        {
                                            BodyBezierMode NewOpen = new BodyBezierMode();
                                            NewOpen.Text = "刚体高度,块处理模式";
                                            NewOpen.SetDate = new TheDataForBezier("Height", HeightFirstNummer.Text, HeightLastNummer.Text, BodyListForOpera.ToArray());
                                            NewOpen.SetDate.ItemCount = new int[BodyListForOpera.Count];
                                            for (int x = 0; x < BodyListForOpera.Count; x++)
                                            {
                                                try
                                                {
                                                    NewOpen.SetDate.ItemCount[x] = BodyListForOpera[x].Count[0];
                                                }
                                                catch (Exception)
                                                {
                                                    NewOpen.SetDate.ItemCount[x] = 0;
                                                }
                                            }
                                            NewOpen.SetDate.UseMode = 2;
                                            NewOpen.Show(this.Owner);
                                            ListForm.Add(NewOpen);
                                        }
                                    }
                                    else
                                    {
                                        List<float> Temp = new List<float>(TheFunOfMath(HeightFuntionSelect.SelectedItem.ToString(), BodyListForOpera.Count, float.Parse(HeightFirstNummer.Text), float.Parse(HeightLastNummer.Text), 0));
                                        for (int i = 0; i < BodyListForOpera.Count; i++)
                                        {
                                            foreach (int BodyTemp in BodyListForOpera[i].Count)
                                            {
                                                if (Temp[i] < 0)
                                                {
                                                    ThePmxOfNow.Body[BodyTemp].BoxSize.Y = -Temp[i];
                                                }
                                                else
                                                {
                                                    ThePmxOfNow.Body[BodyTemp].BoxSize.Y = Temp[i];
                                                }
                                            }
                                        }
                                        ThFunOfSaveToPmx(ThePmxOfNow, "Body");
                                    }
                                }
                                catch (Exception) { }
                                break;

                            case "DepthLastNummer":
                                try
                                {
                                    if (DepthFuntionSelect.SelectedItem.ToString() == "Bezier")
                                    {
                                        bool find = false;
                                        foreach (var temp in ListForm)
                                        {
                                            if (temp.Text == "刚体深度,块处理模式")
                                            {
                                                find = true;
                                                temp.TopLevel = true;
                                                break;
                                            }
                                        }
                                        if (!find)
                                        {
                                            BodyBezierMode NewOpen = new BodyBezierMode();
                                            NewOpen.Text = "刚体深度,块处理模式";
                                            NewOpen.SetDate = new TheDataForBezier("Depth", DepthFirstNummer.Text, DepthLastNummer.Text, BodyListForOpera.ToArray());
                                            NewOpen.SetDate.ItemCount = new int[BodyListForOpera.Count];
                                            for (int x = 0; x < BodyListForOpera.Count; x++)
                                            {
                                                try
                                                {
                                                    NewOpen.SetDate.ItemCount[x] = BodyListForOpera[x].Count[0];
                                                }
                                                catch (Exception)
                                                {
                                                    NewOpen.SetDate.ItemCount[x] = 0;
                                                }
                                            }
                                            NewOpen.SetDate.UseMode = 2;
                                            NewOpen.Show(this.Owner);
                                            ListForm.Add(NewOpen);
                                        }
                                    }
                                    else
                                    {
                                        List<float> Temp = new List<float>(TheFunOfMath(DepthFuntionSelect.SelectedItem.ToString(), BodyListForOpera.Count, float.Parse(DepthFirstNummer.Text), float.Parse(DepthLastNummer.Text), 0));
                                        for (int i = 0; i < BodyListForOpera.Count; i++)
                                        {
                                            foreach (int BodyTemp in BodyListForOpera[i].Count)
                                            {
                                                if (Temp[i] < 0)
                                                {
                                                    ThePmxOfNow.Body[BodyTemp].BoxSize.Z = -Temp[i];
                                                }
                                                else
                                                {
                                                    ThePmxOfNow.Body[BodyTemp].BoxSize.Z = Temp[i];
                                                }
                                            }
                                        }
                                        ThFunOfSaveToPmx(ThePmxOfNow, "Body");
                                    }
                                }
                                catch (Exception) { }
                                break;
                        }
                    }
                    else
                    {
                        MetroMessageBox.Show(this, "请先添加列表后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MetroMessageBox.Show(this, "请先选择刚体后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        public void JointNameCheck_Click(object sender, EventArgs e)
        {
            if (JointNameCheck.CheckState == CheckState.Checked)
            {
                InputJointName.Enabled = true;
                InputJointName.UseCustomBackColor = false;
            }
            else
            {
                InputJointName.Enabled = false;
                InputJointName.UseCustomBackColor = true;
            }
            Refresh();
        }

        public void SpringConst_Move_LastXNummer_ClearClicked()
        {
            SpringConst_Move_FirstXNummer.Text = "";
        }

        public void SpringConst_Move_LastYNummer_ClearClicked()
        {
            SpringConst_Move_FirstYNummer.Text = "";
        }

        public void SpringConst_Move_LastZNummer_ClearClicked()
        {
            SpringConst_Move_FirstZNummer.Text = "";
        }

        public void SpringConst_Rotate_LastXNummer_ClearClicked()
        {
            SpringConst_Rotate_FirstXNummer.Text = "";
        }

        public void SpringConst_Rotate_LastYNummer_ClearClicked()
        {
            SpringConst_Rotate_FirstYNummer.Text = "";
        }

        public void SpringConst_Rotate_LastZNummer_ClearClicked()
        {
            SpringConst_Rotate_FirstZNummer.Text = "";
        }

        public void Limit_MoveHigh_LastXNummer_ClearClicked()
        {
            Limit_MoveLow_FirstXNummer.Text = "";
            Limit_MoveHigh_FirstXNummer.Text = "";
            Limit_MoveLow_LastXNummer.Text = "";
        }

        public void Limit_MoveHigh_LastYNummer_ClearClicked()
        {
            Limit_MoveLow_FirstYNummer.Text = "";
            Limit_MoveHigh_FirstYNummer.Text = "";
            Limit_MoveLow_LastYNummer.Text = "";
        }

        public void Limit_MoveHigh_LastZNummer_ClearClicked()
        {
            Limit_MoveLow_FirstZNummer.Text = "";
            Limit_MoveHigh_FirstZNummer.Text = "";
            Limit_MoveLow_LastZNummer.Text = "";
        }

        public void Limit_AngleHigh_LastXNummer_ClearClicked()
        {
            Limit_AngleLow_FirstXNummer.Text = "";
            Limit_AngleHigh_FirstXNummer.Text = "";
            Limit_AngleLow_LastXNummer.Text = "";
        }

        public void Limit_AngleHigh_LastYNummer_ClearClicked()
        {
            Limit_AngleLow_FirstYNummer.Text = "";
            Limit_AngleHigh_FirstYNummer.Text = "";
            Limit_AngleLow_LastYNummer.Text = "";
        }

        public void Limit_AngleHigh_LastZNummer_ClearClicked()
        {
            Limit_AngleLow_FirstZNummer.Text = "";
            Limit_AngleHigh_FirstZNummer.Text = "";
            Limit_AngleLow_LastZNummer.Text = "";
        }

        public void SetConst(object sender, EventArgs e)
        {
            IPXPmx ThePmxOfNow = GetPmx;
            if (JointOperaMode1.Checked)
            {
                if (JointCount.Count != 0)
                {
                    MetroTextBox NewTextBox = sender as MetroTextBox;
                    switch (NewTextBox.Name)
                    {
                        case "SpringConst_Move_LastXNummer":
                            try
                            {
                                List<float> Temp = new List<float>(TheFunOfMath(SpringConst_Move_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(SpringConst_Move_FirstXNummer.Text), float.Parse(SpringConst_Move_LastXNummer.Text), 0));

                                for (int i = 0; i < JointCount.Count; i++)
                                {
                                    ThePmxOfNow.Joint[JointCount[i]].SpringConst_Move.X = Temp[i];
                                }
                            }
                            catch (Exception) { }
                            break;

                        case "SpringConst_Move_LastYNummer":
                            try
                            {
                                List<float> Temp = new List<float>(TheFunOfMath(SpringConst_Move_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(SpringConst_Move_FirstYNummer.Text), float.Parse(SpringConst_Move_LastYNummer.Text), 0));

                                for (int i = 0; i < JointCount.Count; i++)
                                {
                                    ThePmxOfNow.Joint[JointCount[i]].SpringConst_Move.Y = Temp[i];
                                }
                            }
                            catch (Exception) { }
                            break;

                        case "SpringConst_Move_LastZNummer":
                            try
                            {
                                List<float> Temp = new List<float>(TheFunOfMath(SpringConst_Move_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(SpringConst_Move_FirstZNummer.Text), float.Parse(SpringConst_Move_LastZNummer.Text), 0));

                                for (int i = 0; i < JointCount.Count; i++)
                                {
                                    ThePmxOfNow.Joint[JointCount[i]].SpringConst_Move.Z = Temp[i];
                                }
                            }
                            catch (Exception) { }
                            break;

                        case "SpringConst_Rotate_LastXNummer":
                            try
                            {
                                List<float> Temp = new List<float>(TheFunOfMath(SpringConst_Rotate_Nummer_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(SpringConst_Rotate_FirstXNummer.Text), float.Parse(SpringConst_Rotate_LastXNummer.Text), 0));

                                for (int i = 0; i < JointCount.Count; i++)
                                {
                                    ThePmxOfNow.Joint[JointCount[i]].SpringConst_Rotate.X = Temp[i];
                                }
                            }
                            catch (Exception) { }
                            break;

                        case "SpringConst_Rotate_LastYNummer":
                            try
                            {
                                List<float> Temp = new List<float>(TheFunOfMath(SpringConst_Rotate_Nummer_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(SpringConst_Rotate_FirstYNummer.Text), float.Parse(SpringConst_Rotate_LastYNummer.Text), 0));

                                for (int i = 0; i < JointCount.Count; i++)
                                {
                                    ThePmxOfNow.Joint[JointCount[i]].SpringConst_Rotate.Y = Temp[i];
                                }
                            }
                            catch (Exception) { }
                            break;

                        case "SpringConst_Rotate_LastZNummer":
                            try
                            {
                                List<float> Temp = new List<float>(TheFunOfMath(SpringConst_Rotate_Nummer_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(SpringConst_Rotate_FirstZNummer.Text), float.Parse(SpringConst_Rotate_LastZNummer.Text), 0));

                                for (int i = 0; i < JointCount.Count; i++)
                                {
                                    ThePmxOfNow.Joint[JointCount[i]].SpringConst_Rotate.Z = Temp[i];
                                }
                            }
                            catch (Exception) { }
                            break;
                    }
                    ARGS.Host.Connector.Pmx.Update(ThePmxOfNow);
                    ARGS.Host.Connector.Form.UpdateList(UpdateObject.Joint);
                    ARGS.Host.Connector.View.PmxView.UpdateView();

                    ARGS.Host.Connector.View.PmxView.SetSelectedBodyIndices(new int[] { BodyCount[0] });
                }
                else
                {
                    MetroMessageBox.Show(this, "请先选择J点后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
            else if (JointOperaMode2.Checked)
            {
                if (JointListForOpera.Count != 0)
                {
                    MetroTextBox NewTextBox = sender as MetroTextBox;
                    foreach (OperaList ListTemp in JointListForOpera)
                    {
                        JointCount.Clear();
                        JointCount.AddRange(ListTemp.Count);
                        switch (NewTextBox.Name)
                        {
                            case "SpringConst_Move_LastXNummer":
                                try
                                {
                                    List<float> Temp = new List<float>(TheFunOfMath(SpringConst_Move_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(SpringConst_Move_FirstXNummer.Text), float.Parse(SpringConst_Move_LastXNummer.Text), 0));

                                    for (int i = 0; i < JointCount.Count; i++)
                                    {
                                        ThePmxOfNow.Joint[JointCount[i]].SpringConst_Move.X = Temp[i];
                                    }
                                }
                                catch (Exception) { }
                                break;

                            case "SpringConst_Move_LastYNummer":
                                try
                                {
                                    List<float> Temp = new List<float>(TheFunOfMath(SpringConst_Move_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(SpringConst_Move_FirstYNummer.Text), float.Parse(SpringConst_Move_LastYNummer.Text), 0));

                                    for (int i = 0; i < JointCount.Count; i++)
                                    {
                                        ThePmxOfNow.Joint[JointCount[i]].SpringConst_Move.Y = Temp[i];
                                    }
                                }
                                catch (Exception) { }
                                break;

                            case "SpringConst_Move_LastZNummer":
                                try
                                {
                                    List<float> Temp = new List<float>(TheFunOfMath(SpringConst_Move_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(SpringConst_Move_FirstZNummer.Text), float.Parse(SpringConst_Move_LastZNummer.Text), 0));

                                    for (int i = 0; i < JointCount.Count; i++)
                                    {
                                        ThePmxOfNow.Joint[JointCount[i]].SpringConst_Move.Z = Temp[i];
                                    }
                                }
                                catch (Exception) { }
                                break;

                            case "SpringConst_Rotate_LastXNummer":
                                try
                                {
                                    List<float> Temp = new List<float>(TheFunOfMath(SpringConst_Rotate_Nummer_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(SpringConst_Rotate_FirstXNummer.Text), float.Parse(SpringConst_Rotate_LastXNummer.Text), 0));

                                    for (int i = 0; i < JointCount.Count; i++)
                                    {
                                        ThePmxOfNow.Joint[JointCount[i]].SpringConst_Rotate.X = Temp[i];
                                    }
                                }
                                catch (Exception) { }
                                break;

                            case "SpringConst_Rotate_LastYNummer":
                                try
                                {
                                    List<float> Temp = new List<float>(TheFunOfMath(SpringConst_Rotate_Nummer_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(SpringConst_Rotate_FirstYNummer.Text), float.Parse(SpringConst_Rotate_LastYNummer.Text), 0));

                                    for (int i = 0; i < JointCount.Count; i++)
                                    {
                                        ThePmxOfNow.Joint[JointCount[i]].SpringConst_Rotate.Y = Temp[i];
                                    }
                                }
                                catch (Exception) { }
                                break;

                            case "SpringConst_Rotate_LastZNummer":
                                try
                                {
                                    List<float> Temp = new List<float>(TheFunOfMath(SpringConst_Rotate_Nummer_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(SpringConst_Rotate_FirstZNummer.Text), float.Parse(SpringConst_Rotate_LastZNummer.Text), 0));

                                    for (int i = 0; i < JointCount.Count; i++)
                                    {
                                        ThePmxOfNow.Joint[JointCount[i]].SpringConst_Rotate.Z = Temp[i];
                                    }
                                }
                                catch (Exception) { }
                                break;
                        }
                        ARGS.Host.Connector.Pmx.Update(ThePmxOfNow);
                        ARGS.Host.Connector.Form.UpdateList(UpdateObject.Joint);
                        ARGS.Host.Connector.View.PmxView.UpdateView();
                    }
                }
                else
                {
                    MetroMessageBox.Show(this, "请先添加列表后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
            else if (JointOperaMode3.Checked)
            {
                if (JointListForOpera.Count != 0)
                {
                    MetroTextBox NewTextBox = sender as MetroTextBox;

                    switch (NewTextBox.Name)
                    {
                        case "SpringConst_Move_LastXNummer":
                            try
                            {
                                List<float> Temp = new List<float>(TheFunOfMath(SpringConst_Move_FuntionSelect.SelectedItem.ToString(), JointListForOpera.Count, float.Parse(SpringConst_Move_FirstXNummer.Text), float.Parse(SpringConst_Move_LastXNummer.Text), 0));

                                for (int i = 0; i < JointListForOpera.Count; i++)
                                {
                                    foreach (int ListTemp in JointListForOpera[i].Count)
                                    {
                                        ThePmxOfNow.Joint[ListTemp].SpringConst_Move.X = Temp[i];
                                    }
                                }
                            }
                            catch (Exception) { }
                            break;

                        case "SpringConst_Move_LastYNummer":
                            try
                            {
                                List<float> Temp = new List<float>(TheFunOfMath(SpringConst_Move_FuntionSelect.SelectedItem.ToString(), JointListForOpera.Count, float.Parse(SpringConst_Move_FirstYNummer.Text), float.Parse(SpringConst_Move_LastYNummer.Text), 0));

                                for (int i = 0; i < JointListForOpera.Count; i++)
                                {
                                    foreach (int ListTemp in JointListForOpera[i].Count)
                                    {
                                        ThePmxOfNow.Joint[ListTemp].SpringConst_Move.Y = Temp[i];
                                    }
                                }
                            }
                            catch (Exception) { }
                            break;

                        case "SpringConst_Move_LastZNummer":
                            try
                            {
                                List<float> Temp = new List<float>(TheFunOfMath(SpringConst_Move_FuntionSelect.SelectedItem.ToString(), JointListForOpera.Count, float.Parse(SpringConst_Move_FirstZNummer.Text), float.Parse(SpringConst_Move_LastZNummer.Text), 0));

                                for (int i = 0; i < JointListForOpera.Count; i++)
                                {
                                    foreach (int ListTemp in JointListForOpera[i].Count)
                                    {
                                        ThePmxOfNow.Joint[ListTemp].SpringConst_Move.Z = Temp[i];
                                    }
                                }
                            }
                            catch (Exception) { }
                            break;

                        case "SpringConst_Rotate_LastXNummer":
                            try
                            {
                                List<float> Temp = new List<float>(TheFunOfMath(SpringConst_Rotate_Nummer_FuntionSelect.SelectedItem.ToString(), JointListForOpera.Count, float.Parse(SpringConst_Rotate_FirstXNummer.Text), float.Parse(SpringConst_Rotate_LastXNummer.Text), 0));

                                for (int i = 0; i < JointListForOpera.Count; i++)
                                {
                                    foreach (int ListTemp in JointListForOpera[i].Count)
                                    {
                                        ThePmxOfNow.Joint[ListTemp].SpringConst_Rotate.X = Temp[i];
                                    }
                                }
                            }
                            catch (Exception) { }
                            break;

                        case "SpringConst_Rotate_LastYNummer":
                            try
                            {
                                List<float> Temp = new List<float>(TheFunOfMath(SpringConst_Rotate_Nummer_FuntionSelect.SelectedItem.ToString(), JointListForOpera.Count, float.Parse(SpringConst_Rotate_FirstYNummer.Text), float.Parse(SpringConst_Rotate_LastYNummer.Text), 0));

                                for (int i = 0; i < JointListForOpera.Count; i++)
                                {
                                    foreach (int ListTemp in JointListForOpera[i].Count)
                                    {
                                        ThePmxOfNow.Joint[ListTemp].SpringConst_Rotate.Y = Temp[i];
                                    }
                                }
                            }
                            catch (Exception) { }
                            break;

                        case "SpringConst_Rotate_LastZNummer":
                            try
                            {
                                List<float> Temp = new List<float>(TheFunOfMath(SpringConst_Rotate_Nummer_FuntionSelect.SelectedItem.ToString(), JointListForOpera.Count, float.Parse(SpringConst_Rotate_FirstZNummer.Text), float.Parse(SpringConst_Rotate_LastZNummer.Text), 0));

                                for (int i = 0; i < JointListForOpera.Count; i++)
                                {
                                    foreach (int ListTemp in JointListForOpera[i].Count)
                                    {
                                        ThePmxOfNow.Joint[ListTemp].SpringConst_Rotate.Z = Temp[i];
                                    }
                                }
                            }
                            catch (Exception) { }
                            break;
                    }
                    ARGS.Host.Connector.Pmx.Update(ThePmxOfNow);
                    ARGS.Host.Connector.Form.UpdateList(UpdateObject.Joint);
                    ARGS.Host.Connector.View.PmxView.UpdateView();
                }
                else
                {
                    MetroMessageBox.Show(this, "请先添加列表后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
        }

        public void SetAllConst_Click(object sender, EventArgs e)
        {
            IPXPmx ThePmxOfNow = GetPmx;
            if (JointOperaMode1.Checked)
            {
                if (JointCount.Count != 0)
                {
                    try
                    {
                        {
                            List<float> Temp = new List<float>(TheFunOfMath(SpringConst_Move_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(SpringConst_Move_FirstXNummer.Text), float.Parse(SpringConst_Move_LastXNummer.Text), 0));
                            for (int i = 0; i < JointCount.Count; i++)
                            {
                                ThePmxOfNow.Joint[JointCount[i]].SpringConst_Move.X = Temp[i];
                            }
                        }
                        {
                            List<float> Temp = new List<float>(TheFunOfMath(SpringConst_Move_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(SpringConst_Move_FirstYNummer.Text), float.Parse(SpringConst_Move_LastYNummer.Text), 0));
                            for (int i = 0; i < JointCount.Count; i++)
                            {
                                ThePmxOfNow.Joint[JointCount[i]].SpringConst_Move.Y = Temp[i];
                            }
                        }
                        {
                            List<float> Temp = new List<float>(TheFunOfMath(SpringConst_Move_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(SpringConst_Move_FirstZNummer.Text), float.Parse(SpringConst_Move_LastZNummer.Text), 0));
                            for (int i = 0; i < JointCount.Count; i++)
                            {
                                ThePmxOfNow.Joint[JointCount[i]].SpringConst_Move.Z = Temp[i];
                            }
                        }
                        {
                            List<float> Temp = new List<float>(TheFunOfMath(SpringConst_Rotate_Nummer_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(SpringConst_Rotate_FirstXNummer.Text), float.Parse(SpringConst_Rotate_LastXNummer.Text), 0));
                            for (int i = 0; i < JointCount.Count; i++)
                            {
                                ThePmxOfNow.Joint[JointCount[i]].SpringConst_Rotate.X = Temp[i];
                            }
                        }
                        {
                            List<float> Temp = new List<float>(TheFunOfMath(SpringConst_Rotate_Nummer_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(SpringConst_Rotate_FirstYNummer.Text), float.Parse(SpringConst_Rotate_LastYNummer.Text), 0));
                            for (int i = 0; i < JointCount.Count; i++)
                            {
                                ThePmxOfNow.Joint[JointCount[i]].SpringConst_Rotate.Y = Temp[i];
                            }
                        }
                        {
                            List<float> Temp = new List<float>(TheFunOfMath(SpringConst_Rotate_Nummer_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(SpringConst_Rotate_FirstZNummer.Text), float.Parse(SpringConst_Rotate_LastZNummer.Text), 0));
                            for (int i = 0; i < JointCount.Count; i++)
                            {
                                ThePmxOfNow.Joint[JointCount[i]].SpringConst_Rotate.Z = Temp[i];
                            }
                        }
                        ARGS.Host.Connector.Pmx.Update(ThePmxOfNow);
                        ARGS.Host.Connector.Form.UpdateList(UpdateObject.Joint);
                        ARGS.Host.Connector.View.PmxView.UpdateView();
                    }
                    catch (Exception) { }
                }
                else
                {
                    MetroMessageBox.Show(this, "请先选择J点后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
            else if (JointOperaMode2.Checked)
            {
                if (JointListForOpera.Count != 0)
                {
                    MetroTextBox NewTextBox = sender as MetroTextBox;
                    foreach (OperaList ListTemp in JointListForOpera)
                    {
                        JointCount.Clear();
                        JointCount.AddRange(ListTemp.Count);
                        try
                        {
                            {
                                List<float> Temp = new List<float>(TheFunOfMath(SpringConst_Move_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(SpringConst_Move_FirstXNummer.Text), float.Parse(SpringConst_Move_LastXNummer.Text), 0));

                                for (int i = 0; i < JointCount.Count; i++)
                                {
                                    ThePmxOfNow.Joint[JointCount[i]].SpringConst_Move.X = Temp[i];
                                }
                            }
                            {
                                List<float> Temp = new List<float>(TheFunOfMath(SpringConst_Move_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(SpringConst_Move_FirstYNummer.Text), float.Parse(SpringConst_Move_LastYNummer.Text), 0));

                                for (int i = 0; i < JointCount.Count; i++)
                                {
                                    ThePmxOfNow.Joint[JointCount[i]].SpringConst_Move.Y = Temp[i];
                                }
                            }

                            {
                                List<float> Temp = new List<float>(TheFunOfMath(SpringConst_Move_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(SpringConst_Move_FirstZNummer.Text), float.Parse(SpringConst_Move_LastZNummer.Text), 0));

                                for (int i = 0; i < JointCount.Count; i++)
                                {
                                    ThePmxOfNow.Joint[JointCount[i]].SpringConst_Move.Z = Temp[i];
                                }
                            }
                            {
                                List<float> Temp = new List<float>(TheFunOfMath(SpringConst_Rotate_Nummer_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(SpringConst_Rotate_FirstXNummer.Text), float.Parse(SpringConst_Rotate_LastXNummer.Text), 0));

                                for (int i = 0; i < JointCount.Count; i++)
                                {
                                    ThePmxOfNow.Joint[JointCount[i]].SpringConst_Rotate.X = Temp[i];
                                }
                            }
                            {
                                List<float> Temp = new List<float>(TheFunOfMath(SpringConst_Rotate_Nummer_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(SpringConst_Rotate_FirstYNummer.Text), float.Parse(SpringConst_Rotate_LastYNummer.Text), 0));

                                for (int i = 0; i < JointCount.Count; i++)
                                {
                                    ThePmxOfNow.Joint[JointCount[i]].SpringConst_Rotate.Y = Temp[i];
                                }
                            }

                            {
                                List<float> Temp = new List<float>(TheFunOfMath(SpringConst_Rotate_Nummer_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(SpringConst_Rotate_FirstZNummer.Text), float.Parse(SpringConst_Rotate_LastZNummer.Text), 0));

                                for (int i = 0; i < JointCount.Count; i++)
                                {
                                    ThePmxOfNow.Joint[JointCount[i]].SpringConst_Rotate.Z = Temp[i];
                                }
                            }
                        }
                        catch (Exception) { }
                    }
                    ARGS.Host.Connector.Pmx.Update(ThePmxOfNow);
                    ARGS.Host.Connector.Form.UpdateList(UpdateObject.Joint);
                    ARGS.Host.Connector.View.PmxView.UpdateView();
                }
                else
                {
                    MetroMessageBox.Show(this, "请先添加列表后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
            else if (JointOperaMode3.Checked)
            {
                if (JointListForOpera.Count != 0)
                {
                    MetroTextBox NewTextBox = sender as MetroTextBox;

                    {
                        List<float> Temp = new List<float>(TheFunOfMath(SpringConst_Move_FuntionSelect.SelectedItem.ToString(), JointListForOpera.Count, float.Parse(SpringConst_Move_FirstXNummer.Text), float.Parse(SpringConst_Move_LastXNummer.Text), 0));

                        for (int i = 0; i < JointListForOpera.Count; i++)
                        {
                            foreach (int ListTemp in JointListForOpera[i].Count)
                            {
                                ThePmxOfNow.Joint[ListTemp].SpringConst_Move.X = Temp[i];
                            }
                        }
                    }

                    {
                        List<float> Temp = new List<float>(TheFunOfMath(SpringConst_Move_FuntionSelect.SelectedItem.ToString(), JointListForOpera.Count, float.Parse(SpringConst_Move_FirstYNummer.Text), float.Parse(SpringConst_Move_LastYNummer.Text), 0));

                        for (int i = 0; i < JointListForOpera.Count; i++)
                        {
                            foreach (int ListTemp in JointListForOpera[i].Count)
                            {
                                ThePmxOfNow.Joint[ListTemp].SpringConst_Move.Y = Temp[i];
                            }
                        }
                    }

                    {
                        List<float> Temp = new List<float>(TheFunOfMath(SpringConst_Move_FuntionSelect.SelectedItem.ToString(), JointListForOpera.Count, float.Parse(SpringConst_Move_FirstZNummer.Text), float.Parse(SpringConst_Move_LastZNummer.Text), 0));

                        for (int i = 0; i < JointListForOpera.Count; i++)
                        {
                            foreach (int ListTemp in JointListForOpera[i].Count)
                            {
                                ThePmxOfNow.Joint[ListTemp].SpringConst_Move.Z = Temp[i];
                            }
                        }
                    }

                    {
                        List<float> Temp = new List<float>(TheFunOfMath(SpringConst_Rotate_Nummer_FuntionSelect.SelectedItem.ToString(), JointListForOpera.Count, float.Parse(SpringConst_Rotate_FirstXNummer.Text), float.Parse(SpringConst_Rotate_LastXNummer.Text), 0));

                        for (int i = 0; i < JointListForOpera.Count; i++)
                        {
                            foreach (int ListTemp in JointListForOpera[i].Count)
                            {
                                ThePmxOfNow.Joint[ListTemp].SpringConst_Rotate.X = Temp[i];
                            }
                        }
                    }

                    {
                        List<float> Temp = new List<float>(TheFunOfMath(SpringConst_Rotate_Nummer_FuntionSelect.SelectedItem.ToString(), JointListForOpera.Count, float.Parse(SpringConst_Rotate_FirstYNummer.Text), float.Parse(SpringConst_Rotate_LastYNummer.Text), 0));

                        for (int i = 0; i < JointListForOpera.Count; i++)
                        {
                            foreach (int ListTemp in JointListForOpera[i].Count)
                            {
                                ThePmxOfNow.Joint[ListTemp].SpringConst_Rotate.Y = Temp[i];
                            }
                        }
                    }

                    {
                        List<float> Temp = new List<float>(TheFunOfMath(SpringConst_Rotate_Nummer_FuntionSelect.SelectedItem.ToString(), JointListForOpera.Count, float.Parse(SpringConst_Rotate_FirstZNummer.Text), float.Parse(SpringConst_Rotate_LastZNummer.Text), 0));

                        for (int i = 0; i < JointListForOpera.Count; i++)
                        {
                            foreach (int ListTemp in JointListForOpera[i].Count)
                            {
                                ThePmxOfNow.Joint[ListTemp].SpringConst_Rotate.Z = Temp[i];
                            }
                        }
                    }

                    ARGS.Host.Connector.Pmx.Update(ThePmxOfNow);
                    ARGS.Host.Connector.Form.UpdateList(UpdateObject.Joint);
                    ARGS.Host.Connector.View.PmxView.UpdateView();
                }
                else
                {
                    MetroMessageBox.Show(this, "请先添加列表后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
        }

        public void SetLimit(object sender, EventArgs e)
        {
            IPXPmx ThePmxOfNow = GetPmx;
            if (JointOperaMode1.Checked)
            {
                if (JointCount.Count != 0)
                {
                    MetroTextBox NewTextBox = sender as MetroTextBox;
                    switch (NewTextBox.Name)
                    {
                        case "Limit_MoveHigh_LastXNummer":
                            try
                            {
                                if (Limit_Move_X_FuntionSelect.SelectedItem.ToString() == "Bezier")
                                {
                                    bool find = false;
                                    foreach (var temp in ListForm)
                                    {
                                        if (temp.Text == "Joint,X轴移动限制")
                                        {
                                            find = true;
                                            temp.TopLevel = true;
                                            break;
                                        }
                                    }
                                    if (!find)
                                    {
                                        JointBezierMode NewOpen = new JointBezierMode();
                                        NewOpen.Text = "Joint,X轴移动限制";
                                        NewOpen.SetDate = new TheDataForBezier("Limit_Move_X", Limit_MoveLow_FirstXNummer.Text, Limit_MoveLow_LastXNummer.Text, Limit_MoveHigh_FirstXNummer.Text, Limit_MoveHigh_LastXNummer.Text, JointCount.ToArray());
                                        NewOpen.Show(this.Owner);
                                        ListForm.Add(NewOpen);
                                    }
                                }
                                else
                                {
                                    List<float> Temp = new List<float>(TheFunOfMath(Limit_Move_X_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_MoveLow_FirstXNummer.Text), float.Parse(Limit_MoveLow_LastXNummer.Text), 0));
                                    List<float> Temp2 = new List<float>(TheFunOfMath(Limit_Move_X_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_MoveHigh_FirstXNummer.Text), float.Parse(Limit_MoveHigh_LastXNummer.Text), 0));
                                    for (int i = 0; i < JointCount.Count; i++)
                                    {
                                        ThePmxOfNow.Joint[JointCount[i]].Limit_MoveLow.X = Temp[i];
                                        ThePmxOfNow.Joint[JointCount[i]].Limit_MoveHigh.X = Temp2[i];
                                    }
                                }
                            }
                            catch (Exception) { }

                            break;

                        case "Limit_MoveHigh_LastYNummer":
                            try
                            {
                                if (Limit_Move_Y_FuntionSelect.SelectedItem.ToString() == "Bezier")
                                {
                                    bool find = false;
                                    foreach (var temp in ListForm)
                                    {
                                        if (temp.Text == "Joint,Y轴移动限制")
                                        {
                                            find = true;
                                            temp.TopLevel = true;
                                            break;
                                        }
                                    }
                                    if (!find)
                                    {
                                        JointBezierMode NewOpen = new JointBezierMode();
                                        NewOpen.Text = "Joint,Y轴移动限制";
                                        NewOpen.SetDate = new TheDataForBezier("Limit_Move_Y", Limit_MoveLow_FirstYNummer.Text, Limit_MoveLow_LastYNummer.Text, Limit_MoveHigh_FirstYNummer.Text, Limit_MoveHigh_LastYNummer.Text, JointCount.ToArray());
                                        NewOpen.Show(this.Owner);
                                        ListForm.Add(NewOpen);
                                    }
                                }
                                else
                                {
                                    List<float> Temp = new List<float>(TheFunOfMath(Limit_Move_Y_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_MoveLow_FirstYNummer.Text), float.Parse(Limit_MoveLow_LastYNummer.Text), 0));
                                    List<float> Temp2 = new List<float>(TheFunOfMath(Limit_Move_Y_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_MoveHigh_FirstYNummer.Text), float.Parse(Limit_MoveHigh_LastYNummer.Text), 0));
                                    for (int i = 0; i < JointCount.Count; i++)
                                    {
                                        ThePmxOfNow.Joint[JointCount[i]].Limit_MoveLow.Y = Temp[i];
                                        ThePmxOfNow.Joint[JointCount[i]].Limit_MoveHigh.Y = Temp2[i];
                                    }
                                }
                            }
                            catch (Exception) { }
                            break;

                        case "Limit_MoveHigh_LastZNummer":
                            try
                            {
                                if (Limit_Move_Z_FuntionSelect.SelectedItem.ToString() == "Bezier")
                                {
                                    bool find = false;
                                    foreach (var temp in ListForm)
                                    {
                                        if (temp.Text == "Joint,Z轴移动限制")
                                        {
                                            find = true;
                                            temp.TopLevel = true;
                                            break;
                                        }
                                    }
                                    if (!find)
                                    {
                                        JointBezierMode NewOpen = new JointBezierMode();
                                        NewOpen.Text = "Joint,Z轴移动限制";
                                        NewOpen.SetDate = new TheDataForBezier("Limit_Move_Z", Limit_MoveLow_FirstZNummer.Text, Limit_MoveLow_LastZNummer.Text, Limit_MoveHigh_FirstZNummer.Text, Limit_MoveHigh_LastZNummer.Text, JointCount.ToArray());
                                        NewOpen.Show(this.Owner);
                                        ListForm.Add(NewOpen);
                                    }
                                }
                                else
                                {
                                    List<float> Temp = new List<float>(TheFunOfMath(Limit_Move_Z_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_MoveLow_FirstZNummer.Text), float.Parse(Limit_MoveLow_LastZNummer.Text), 0));
                                    List<float> Temp2 = new List<float>(TheFunOfMath(Limit_Move_Z_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_MoveHigh_FirstZNummer.Text), float.Parse(Limit_MoveHigh_LastZNummer.Text), 0));
                                    for (int i = 0; i < JointCount.Count; i++)
                                    {
                                        ThePmxOfNow.Joint[JointCount[i]].Limit_MoveLow.Z = Temp[i];
                                        ThePmxOfNow.Joint[JointCount[i]].Limit_MoveHigh.Z = Temp2[i];
                                    }
                                }
                            }
                            catch (Exception) { }
                            break;

                        case "Limit_AngleHigh_LastXNummer":
                            try
                            {
                                if (Limit_Angle_FuntionSelect_X.SelectedItem.ToString() == "Bezier")
                                {
                                    bool find = false;
                                    foreach (var temp in ListForm)
                                    {
                                        if (temp.Text == "Joint,X轴旋转限制")
                                        {
                                            find = true;
                                            temp.TopLevel = true;
                                            break;
                                        }
                                    }
                                    if (!find)
                                    {
                                        JointBezierMode NewOpen = new JointBezierMode();
                                        NewOpen.Text = "Joint,X轴旋转限制";
                                        NewOpen.SetDate = new TheDataForBezier("Limit_Angle_X", Limit_AngleLow_FirstXNummer.Text, Limit_AngleLow_LastXNummer.Text, Limit_AngleHigh_FirstXNummer.Text, Limit_AngleHigh_LastXNummer.Text, JointCount.ToArray());
                                        NewOpen.Show(this.Owner);
                                        ListForm.Add(NewOpen);
                                    }
                                }
                                else
                                {
                                    List<float> Temp = new List<float>(TheFunOfMath(Limit_Angle_FuntionSelect_X.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_AngleLow_FirstXNummer.Text), float.Parse(Limit_AngleLow_LastXNummer.Text), 0));
                                    List<float> Temp2 = new List<float>(TheFunOfMath(Limit_Angle_FuntionSelect_X.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_AngleHigh_FirstXNummer.Text), float.Parse(Limit_AngleHigh_LastXNummer.Text), 0));
                                    for (int i = 0; i < JointCount.Count; i++)
                                    {
                                        if (Limit_Angle_FuntionSelect_X.SelectedItem.ToString() == "=A/X" || Limit_Angle_FuntionSelect_X.SelectedItem.ToString() == "=-A/X+B")
                                        {
                                            ThePmxOfNow.Joint[JointCount[i]].Limit_AngleLow.X = -(float)((Temp[i] * Math.PI) / 180);
                                        }
                                        else
                                        {
                                            ThePmxOfNow.Joint[JointCount[i]].Limit_AngleLow.X = (float)((Temp[i] * Math.PI) / 180);
                                        }
                                        ThePmxOfNow.Joint[JointCount[i]].Limit_AngleHigh.X = (float)((Temp2[i] * Math.PI) / 180);
                                    }
                                }
                            }
                            catch (Exception) { }
                            break;

                        case "Limit_AngleHigh_LastYNummer":
                            try
                            {
                                if (Limit_Angle_FuntionSelect_Y.SelectedItem.ToString() == "Bezier")
                                {
                                    bool find = false;
                                    foreach (var temp in ListForm)
                                    {
                                        if (temp.Text == "Joint,Y轴旋转限制")
                                        {
                                            find = true;
                                            temp.TopLevel = true;
                                            break;
                                        }
                                    }
                                    if (!find)
                                    {
                                        JointBezierMode NewOpen = new JointBezierMode();
                                        NewOpen.Text = "Joint,Y轴旋转限制";
                                        NewOpen.SetDate = new TheDataForBezier("Limit_Angle_Y", Limit_AngleLow_FirstYNummer.Text, Limit_AngleLow_LastYNummer.Text, Limit_AngleHigh_FirstYNummer.Text, Limit_AngleHigh_LastYNummer.Text, JointCount.ToArray());
                                        NewOpen.Show(this.Owner);
                                        ListForm.Add(NewOpen);
                                    }
                                }
                                else
                                {
                                    List<float> Temp = new List<float>(TheFunOfMath(Limit_Angle_FuntionSelect_Y.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_AngleLow_FirstYNummer.Text), float.Parse(Limit_AngleLow_LastYNummer.Text), 0));
                                    List<float> Temp2 = new List<float>(TheFunOfMath(Limit_Angle_FuntionSelect_Y.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_AngleHigh_FirstYNummer.Text), float.Parse(Limit_AngleHigh_LastYNummer.Text), 0));
                                    for (int i = 0; i < JointCount.Count; i++)
                                    {
                                        if (Limit_Angle_FuntionSelect_Y.SelectedItem.ToString() == "=A/X" || Limit_Angle_FuntionSelect_Y.SelectedItem.ToString() == "=-A/X+B")
                                        {
                                            ThePmxOfNow.Joint[JointCount[i]].Limit_AngleLow.Y = -(float)((Temp[i] * Math.PI) / 180);
                                        }
                                        else
                                        {
                                            ThePmxOfNow.Joint[JointCount[i]].Limit_AngleLow.Y = (float)((Temp[i] * Math.PI) / 180);
                                        }
                                        ThePmxOfNow.Joint[JointCount[i]].Limit_AngleHigh.Y = (float)((Temp2[i] * Math.PI) / 180);
                                    }
                                }
                            }
                            catch (Exception) { }
                            break;

                        case "Limit_AngleHigh_LastZNummer":
                            try
                            {
                                if (Limit_Angle_FuntionSelect_Z.SelectedItem.ToString() == "Bezier")
                                {
                                    bool find = false;
                                    foreach (var temp in ListForm)
                                    {
                                        if (temp.Text == "Joint,Z轴旋转限制")
                                        {
                                            find = true;
                                            temp.TopLevel = true;
                                            break;
                                        }
                                    }
                                    if (!find)
                                    {
                                        JointBezierMode NewOpen = new JointBezierMode();
                                        NewOpen.Text = "Joint,Z轴旋转限制";
                                        NewOpen.SetDate = new TheDataForBezier("Limit_Angle_Z", Limit_AngleLow_FirstZNummer.Text, Limit_AngleLow_LastZNummer.Text, Limit_AngleHigh_FirstZNummer.Text, Limit_AngleHigh_LastZNummer.Text, JointCount.ToArray());
                                        NewOpen.Show(this.Owner);
                                        ListForm.Add(NewOpen);
                                    }
                                }
                                else
                                {
                                    List<float> Temp = new List<float>(TheFunOfMath(Limit_Angle_FuntionSelect_Z.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_AngleLow_FirstZNummer.Text), float.Parse(Limit_AngleLow_LastZNummer.Text), 0));
                                    List<float> Temp2 = new List<float>(TheFunOfMath(Limit_Angle_FuntionSelect_Z.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_AngleHigh_FirstZNummer.Text), float.Parse(Limit_AngleHigh_LastZNummer.Text), 0));
                                    for (int i = 0; i < JointCount.Count; i++)
                                    {
                                        if (Limit_Angle_FuntionSelect_Z.SelectedItem.ToString() == "=A/X" || Limit_Angle_FuntionSelect_Z.SelectedItem.ToString() == "=-A/X+B")
                                        {
                                            ThePmxOfNow.Joint[JointCount[i]].Limit_AngleLow.Z = -(float)((Temp[i] * Math.PI) / 180);
                                        }
                                        else
                                        {
                                            ThePmxOfNow.Joint[JointCount[i]].Limit_AngleLow.Z = (float)((Temp[i] * Math.PI) / 180);
                                        }
                                        ThePmxOfNow.Joint[JointCount[i]].Limit_AngleHigh.Z = (float)((Temp2[i] * Math.PI) / 180);
                                    }
                                }
                            }
                            catch (Exception) { }
                            break;
                    }
                    ARGS.Host.Connector.Pmx.Update(ThePmxOfNow);
                    ARGS.Host.Connector.Form.UpdateList(UpdateObject.Joint);
                    ARGS.Host.Connector.View.PmxView.UpdateView();
                    ARGS.Host.Connector.View.PmxView.SetSelectedJointIndices(new int[] { JointCount[0] });
                }
                else
                {
                    MetroMessageBox.Show(this, "请先选择J点后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
            else if (JointOperaMode2.Checked)
            {
                if (JointListForOpera.Count != 0)
                {
                    MetroTextBox NewTextBox = sender as MetroTextBox;

                    switch (NewTextBox.Name)
                    {
                        case "Limit_MoveHigh_LastXNummer":
                            try
                            {
                                if (Limit_Move_X_FuntionSelect.SelectedItem.ToString() == "Bezier")
                                {
                                    bool find = false;
                                    foreach (var temp in ListForm)
                                    {
                                        if (temp.Text == "Joint,X轴移动限制,批处理模式")
                                        {
                                            find = true;
                                            temp.TopLevel = true;
                                            break;
                                        }
                                    }
                                    if (!find)
                                    {
                                        JointBezierMode NewOpen = new JointBezierMode();
                                        NewOpen.Text = "Joint,X轴移动限制,批处理模式";
                                        NewOpen.SetDate = new TheDataForBezier("Limit_Move_X", Limit_MoveLow_FirstXNummer.Text, Limit_MoveLow_LastXNummer.Text, Limit_MoveHigh_FirstXNummer.Text, Limit_MoveHigh_LastXNummer.Text, JointListForOpera.ToArray());
                                        NewOpen.SetDate.UseMode = 1;
                                        NewOpen.Show(this.Owner);
                                        ListForm.Add(NewOpen);
                                    }
                                }
                                else
                                {
                                    foreach (OperaList ListTemp in JointListForOpera)
                                    {
                                        JointCount.Clear();
                                        JointCount.AddRange(ListTemp.Count);
                                        List<float> Temp = new List<float>(TheFunOfMath(Limit_Move_X_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_MoveLow_FirstXNummer.Text), float.Parse(Limit_MoveLow_LastXNummer.Text), 0));
                                        List<float> Temp2 = new List<float>(TheFunOfMath(Limit_Move_X_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_MoveHigh_FirstXNummer.Text), float.Parse(Limit_MoveHigh_LastXNummer.Text), 0));
                                        for (int i = 0; i < JointCount.Count; i++)
                                        {
                                            ThePmxOfNow.Joint[JointCount[i]].Limit_MoveLow.X = Temp[i];
                                            ThePmxOfNow.Joint[JointCount[i]].Limit_MoveHigh.X = Temp2[i];
                                        }
                                    }
                                }
                            }
                            catch (Exception) { }
                            break;

                        case "Limit_MoveHigh_LastYNummer":
                            try
                            {
                                if (Limit_Move_Y_FuntionSelect.SelectedItem.ToString() == "Bezier")
                                {
                                    bool find = false;
                                    foreach (var temp in ListForm)
                                    {
                                        if (temp.Text == "Joint,Y轴移动限制,批处理模式")
                                        {
                                            find = true;
                                            temp.TopLevel = true;
                                            break;
                                        }
                                    }
                                    if (!find)
                                    {
                                        JointBezierMode NewOpen = new JointBezierMode();
                                        NewOpen.Text = "Joint,Y轴移动限制,批处理模式";
                                        NewOpen.SetDate = new TheDataForBezier("Limit_Move_Y", Limit_MoveLow_FirstYNummer.Text, Limit_MoveLow_LastYNummer.Text, Limit_MoveHigh_FirstYNummer.Text, Limit_MoveHigh_LastYNummer.Text, JointListForOpera.ToArray());
                                        NewOpen.SetDate.UseMode = 1;
                                        NewOpen.Show(this.Owner);
                                        ListForm.Add(NewOpen);
                                    }
                                }
                                else
                                {
                                    foreach (OperaList ListTemp in JointListForOpera)
                                    {
                                        JointCount.Clear();
                                        JointCount.AddRange(ListTemp.Count);
                                        List<float> Temp = new List<float>(TheFunOfMath(Limit_Move_Y_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_MoveLow_FirstYNummer.Text), float.Parse(Limit_MoveLow_LastYNummer.Text), 0));
                                        List<float> Temp2 = new List<float>(TheFunOfMath(Limit_Move_Y_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_MoveHigh_FirstYNummer.Text), float.Parse(Limit_MoveHigh_LastYNummer.Text), 0));
                                        for (int i = 0; i < JointCount.Count; i++)
                                        {
                                            ThePmxOfNow.Joint[JointCount[i]].Limit_MoveLow.Y = Temp[i];
                                            ThePmxOfNow.Joint[JointCount[i]].Limit_MoveHigh.Y = Temp2[i];
                                        }
                                    }
                                }
                            }
                            catch (Exception) { }
                            break;

                        case "Limit_MoveHigh_LastZNummer":
                            try
                            {
                                if (Limit_Move_Z_FuntionSelect.SelectedItem.ToString() == "Bezier")
                                {
                                    bool find = false;
                                    foreach (var temp in ListForm)
                                    {
                                        if (temp.Text == "Joint,Z轴移动限制,批处理模式")
                                        {
                                            find = true;
                                            temp.TopLevel = true;
                                            break;
                                        }
                                    }
                                    if (!find)
                                    {
                                        JointBezierMode NewOpen = new JointBezierMode();
                                        NewOpen.Text = "Joint,Z轴移动限制,批处理模式";
                                        NewOpen.SetDate = new TheDataForBezier("Limit_Move_Z", Limit_MoveLow_FirstZNummer.Text, Limit_MoveLow_LastZNummer.Text, Limit_MoveHigh_FirstZNummer.Text, Limit_MoveHigh_LastZNummer.Text, JointListForOpera.ToArray());
                                        NewOpen.SetDate.UseMode = 1;
                                        NewOpen.Show(this.Owner);
                                        ListForm.Add(NewOpen);
                                    }
                                }
                                else
                                {
                                    foreach (OperaList ListTemp in JointListForOpera)
                                    {
                                        JointCount.Clear();
                                        JointCount.AddRange(ListTemp.Count);
                                        List<float> Temp = new List<float>(TheFunOfMath(Limit_Move_Z_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_MoveLow_FirstZNummer.Text), float.Parse(Limit_MoveLow_LastZNummer.Text), 0));
                                        List<float> Temp2 = new List<float>(TheFunOfMath(Limit_Move_Z_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_MoveHigh_FirstZNummer.Text), float.Parse(Limit_MoveHigh_LastZNummer.Text), 0));
                                        for (int i = 0; i < JointCount.Count; i++)
                                        {
                                            ThePmxOfNow.Joint[JointCount[i]].Limit_MoveLow.Z = Temp[i];
                                            ThePmxOfNow.Joint[JointCount[i]].Limit_MoveHigh.Z = Temp2[i];
                                        }
                                    }
                                }
                            }
                            catch (Exception) { }
                            break;

                        case "Limit_AngleHigh_LastXNummer":
                            try
                            {
                                if (Limit_Angle_FuntionSelect_X.SelectedItem.ToString() == "Bezier")
                                {
                                    bool find = false;
                                    foreach (var temp in ListForm)
                                    {
                                        if (temp.Text == "Joint,X轴旋转限制,批处理模式")
                                        {
                                            find = true;
                                            temp.TopLevel = true;
                                            break;
                                        }
                                    }
                                    if (!find)
                                    {
                                        JointBezierMode NewOpen = new JointBezierMode();
                                        NewOpen.Text = "Joint,X轴旋转限制,批处理模式";
                                        NewOpen.SetDate = new TheDataForBezier("Limit_Angle_X", Limit_AngleLow_FirstXNummer.Text, Limit_AngleLow_LastXNummer.Text, Limit_AngleHigh_FirstXNummer.Text, Limit_AngleHigh_LastXNummer.Text, JointListForOpera.ToArray());
                                        NewOpen.SetDate.UseMode = 1;
                                        NewOpen.Show(this.Owner);
                                        ListForm.Add(NewOpen);
                                    }
                                }
                                else
                                {
                                    foreach (OperaList ListTemp in JointListForOpera)
                                    {
                                        JointCount.Clear();
                                        JointCount.AddRange(ListTemp.Count);
                                        List<float> Temp = new List<float>(TheFunOfMath(Limit_Angle_FuntionSelect_X.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_AngleLow_FirstXNummer.Text), float.Parse(Limit_AngleLow_LastXNummer.Text), 0));
                                        List<float> Temp2 = new List<float>(TheFunOfMath(Limit_Angle_FuntionSelect_X.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_AngleHigh_FirstXNummer.Text), float.Parse(Limit_AngleHigh_LastXNummer.Text), 0));
                                        for (int i = 0; i < JointCount.Count; i++)
                                        {
                                            if (Limit_Angle_FuntionSelect_X.SelectedItem.ToString() == "=A/X" || Limit_Angle_FuntionSelect_X.SelectedItem.ToString() == "=-A/X+B")
                                            {
                                                ThePmxOfNow.Joint[JointCount[i]].Limit_AngleLow.X = -(float)((Temp[i] * Math.PI) / 180);
                                            }
                                            else
                                            {
                                                ThePmxOfNow.Joint[JointCount[i]].Limit_AngleLow.X = (float)((Temp[i] * Math.PI) / 180);
                                            }
                                            ThePmxOfNow.Joint[JointCount[i]].Limit_AngleHigh.X = (float)((Temp2[i] * Math.PI) / 180);
                                        }
                                    }
                                }
                            }
                            catch (Exception) { }
                            break;

                        case "Limit_AngleHigh_LastYNummer":
                            try
                            {
                                if (Limit_Angle_FuntionSelect_Y.SelectedItem.ToString() == "Bezier")
                                {
                                    bool find = false;
                                    foreach (var temp in ListForm)
                                    {
                                        if (temp.Text == "Joint,Y轴旋转限制,批处理模式")
                                        {
                                            find = true;
                                            temp.TopLevel = true;
                                            break;
                                        }
                                    }
                                    if (!find)
                                    {
                                        JointBezierMode NewOpen = new JointBezierMode();
                                        NewOpen.Text = "Joint,Y轴旋转限制,批处理模式";
                                        NewOpen.SetDate = new TheDataForBezier("Limit_Angle_Y", Limit_AngleLow_FirstYNummer.Text, Limit_AngleLow_LastYNummer.Text, Limit_AngleHigh_FirstYNummer.Text, Limit_AngleHigh_LastYNummer.Text, JointListForOpera.ToArray());
                                        NewOpen.SetDate.UseMode = 1;
                                        NewOpen.Show(this.Owner);
                                        ListForm.Add(NewOpen);
                                    }
                                }
                                else
                                {
                                    foreach (OperaList ListTemp in JointListForOpera)
                                    {
                                        JointCount.Clear();
                                        JointCount.AddRange(ListTemp.Count);
                                        List<float> Temp = new List<float>(TheFunOfMath(Limit_Angle_FuntionSelect_Y.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_AngleLow_FirstYNummer.Text), float.Parse(Limit_AngleLow_LastYNummer.Text), 0));
                                        List<float> Temp2 = new List<float>(TheFunOfMath(Limit_Angle_FuntionSelect_Y.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_AngleHigh_FirstYNummer.Text), float.Parse(Limit_AngleHigh_LastYNummer.Text), 0));
                                        for (int i = 0; i < JointCount.Count; i++)
                                        {
                                            if (Limit_Angle_FuntionSelect_Y.SelectedItem.ToString() == "=A/X" || Limit_Angle_FuntionSelect_Y.SelectedItem.ToString() == "=-A/X+B")
                                            {
                                                ThePmxOfNow.Joint[JointCount[i]].Limit_AngleLow.Y = -(float)((Temp[i] * Math.PI) / 180);
                                            }
                                            else
                                            {
                                                ThePmxOfNow.Joint[JointCount[i]].Limit_AngleLow.Y = (float)((Temp[i] * Math.PI) / 180);
                                            }
                                            ThePmxOfNow.Joint[JointCount[i]].Limit_AngleHigh.Y = (float)((Temp2[i] * Math.PI) / 180);
                                        }
                                    }
                                }
                            }
                            catch (Exception) { }
                            break;

                        case "Limit_AngleHigh_LastZNummer":
                            try
                            {
                                if (Limit_Angle_FuntionSelect_Z.SelectedItem.ToString() == "Bezier")
                                {
                                    bool find = false;
                                    foreach (var temp in ListForm)
                                    {
                                        if (temp.Text == "Joint,Z轴旋转限制,批处理模式")
                                        {
                                            find = true;
                                            temp.TopLevel = true;
                                            break;
                                        }
                                    }
                                    if (!find)
                                    {
                                        JointBezierMode NewOpen = new JointBezierMode();
                                        NewOpen.Text = "Joint,Z轴旋转限制,批处理模式";
                                        NewOpen.SetDate = new TheDataForBezier("Limit_Angle_Z", Limit_AngleLow_FirstZNummer.Text, Limit_AngleLow_LastZNummer.Text, Limit_AngleHigh_FirstZNummer.Text, Limit_AngleHigh_LastZNummer.Text, JointListForOpera.ToArray());
                                        NewOpen.SetDate.UseMode = 1;
                                        NewOpen.Show(this.Owner);
                                        ListForm.Add(NewOpen);
                                    }
                                }
                                else
                                {
                                    foreach (OperaList ListTemp in JointListForOpera)
                                    {
                                        JointCount.Clear();
                                        JointCount.AddRange(ListTemp.Count);
                                        List<float> Temp = new List<float>(TheFunOfMath(Limit_Angle_FuntionSelect_Z.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_AngleLow_FirstZNummer.Text), float.Parse(Limit_AngleLow_LastZNummer.Text), 0));
                                        List<float> Temp2 = new List<float>(TheFunOfMath(Limit_Angle_FuntionSelect_Z.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_AngleHigh_FirstZNummer.Text), float.Parse(Limit_AngleHigh_LastZNummer.Text), 0));
                                        for (int i = 0; i < JointCount.Count; i++)
                                        {
                                            if (Limit_Angle_FuntionSelect_Z.SelectedItem.ToString() == "=A/X" || Limit_Angle_FuntionSelect_Z.SelectedItem.ToString() == "=-A/X+B")
                                            {
                                                ThePmxOfNow.Joint[JointCount[i]].Limit_AngleLow.Z = -(float)((Temp[i] * Math.PI) / 180);
                                            }
                                            else
                                            {
                                                ThePmxOfNow.Joint[JointCount[i]].Limit_AngleLow.Z = (float)((Temp[i] * Math.PI) / 180);
                                            }
                                            ThePmxOfNow.Joint[JointCount[i]].Limit_AngleHigh.Z = (float)((Temp2[i] * Math.PI) / 180);
                                        }
                                    }
                                }
                            }
                            catch (Exception) { }
                            break;
                    }
                    ARGS.Host.Connector.Pmx.Update(ThePmxOfNow);
                    ARGS.Host.Connector.Form.UpdateList(UpdateObject.Joint);
                    ARGS.Host.Connector.View.PmxView.UpdateView();
                }
                else
                {
                    MetroMessageBox.Show(this, "请先添加列表后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
            else if (JointOperaMode3.Checked)
            {
                if (JointListForOpera.Count != 0)
                {
                    MetroTextBox NewTextBox = sender as MetroTextBox;
                    switch (NewTextBox.Name)
                    {
                        case "Limit_MoveHigh_LastXNummer":
                            try
                            {
                                if (Limit_Move_X_FuntionSelect.SelectedItem.ToString() == "Bezier")
                                {
                                    bool find = false;
                                    foreach (var temp in ListForm)
                                    {
                                        if (temp.Text == "Joint,X轴移动限制,块处理模式")
                                        {
                                            find = true;
                                            temp.TopLevel = true;
                                            break;
                                        }
                                    }
                                    if (!find)
                                    {
                                        JointBezierMode NewOpen = new JointBezierMode();
                                        NewOpen.Text = "Joint,X轴移动限制,块处理模式";
                                        NewOpen.SetDate = new TheDataForBezier("Limit_Move_X", Limit_MoveLow_FirstXNummer.Text, Limit_MoveLow_LastXNummer.Text, Limit_MoveHigh_FirstXNummer.Text, Limit_MoveHigh_LastXNummer.Text, JointListForOpera.ToArray());
                                        NewOpen.SetDate.ItemCount = new int[JointListForOpera.Count];
                                        for (int x = 0; x < JointListForOpera.Count; x++)
                                        {
                                            try
                                            {
                                                NewOpen.SetDate.ItemCount[x] = JointListForOpera[x].Count[0];
                                            }
                                            catch (Exception)
                                            {
                                                NewOpen.SetDate.ItemCount[x] = 0;
                                            }
                                        }
                                        NewOpen.SetDate.UseMode = 2;
                                        NewOpen.Show(this.Owner);
                                        ListForm.Add(NewOpen);
                                    }
                                }
                                else
                                {
                                    List<float> Temp = new List<float>(TheFunOfMath(Limit_Move_X_FuntionSelect.SelectedItem.ToString(), JointListForOpera.Count, float.Parse(Limit_MoveLow_FirstXNummer.Text), float.Parse(Limit_MoveLow_LastXNummer.Text), 0));
                                    List<float> Temp2 = new List<float>(TheFunOfMath(Limit_Move_X_FuntionSelect.SelectedItem.ToString(), JointListForOpera.Count, float.Parse(Limit_MoveHigh_FirstXNummer.Text), float.Parse(Limit_MoveHigh_LastXNummer.Text), 0));
                                    for (int i = 0; i < JointListForOpera.Count; i++)
                                    {
                                        foreach (int JointTemp in JointListForOpera[i].Count)
                                        {
                                            ThePmxOfNow.Joint[JointTemp].Limit_MoveLow.X = Temp[i];
                                            ThePmxOfNow.Joint[JointTemp].Limit_MoveHigh.X = Temp2[i];
                                        }
                                    }
                                }
                            }
                            catch (Exception) { }
                            break;

                        case "Limit_MoveHigh_LastYNummer":
                            try
                            {
                                if (Limit_Move_Y_FuntionSelect.SelectedItem.ToString() == "Bezier")
                                {
                                    bool find = false;
                                    foreach (var temp in ListForm)
                                    {
                                        if (temp.Text == "Joint,Y轴移动限制,块处理模式")
                                        {
                                            find = true;
                                            temp.TopLevel = true;
                                            break;
                                        }
                                    }
                                    if (!find)
                                    {
                                        JointBezierMode NewOpen = new JointBezierMode();
                                        NewOpen.Text = "Joint,Y轴移动限制,块处理模式";
                                        NewOpen.SetDate = new TheDataForBezier("Limit_Move_Y", Limit_MoveLow_FirstYNummer.Text, Limit_MoveLow_LastYNummer.Text, Limit_MoveHigh_FirstYNummer.Text, Limit_MoveHigh_LastYNummer.Text, JointListForOpera.ToArray());
                                        NewOpen.SetDate.ItemCount = new int[JointListForOpera.Count];
                                        for (int x = 0; x < JointListForOpera.Count; x++)
                                        {
                                            try
                                            {
                                                NewOpen.SetDate.ItemCount[x] = JointListForOpera[x].Count[0];
                                            }
                                            catch (Exception)
                                            {
                                                NewOpen.SetDate.ItemCount[x] = 0;
                                            }
                                        }
                                        NewOpen.SetDate.UseMode = 2;
                                        NewOpen.Show(this.Owner);
                                        ListForm.Add(NewOpen);
                                    }
                                }
                                else
                                {
                                    List<float> Temp = new List<float>(TheFunOfMath(Limit_Move_Y_FuntionSelect.SelectedItem.ToString(), JointListForOpera.Count, float.Parse(Limit_MoveLow_FirstYNummer.Text), float.Parse(Limit_MoveLow_LastYNummer.Text), 0));
                                    List<float> Temp2 = new List<float>(TheFunOfMath(Limit_Move_Y_FuntionSelect.SelectedItem.ToString(), JointListForOpera.Count, float.Parse(Limit_MoveHigh_FirstYNummer.Text), float.Parse(Limit_MoveHigh_LastYNummer.Text), 0));
                                    for (int i = 0; i < JointListForOpera.Count; i++)
                                    {
                                        foreach (int JointTemp in JointListForOpera[i].Count)
                                        {
                                            ThePmxOfNow.Joint[JointTemp].Limit_MoveLow.Y = Temp[i];
                                            ThePmxOfNow.Joint[JointTemp].Limit_MoveHigh.Y = Temp2[i];
                                        }
                                    }
                                }
                            }
                            catch (Exception) { }
                            break;

                        case "Limit_MoveHigh_LastZNummer":
                            try
                            {
                                if (Limit_Move_Z_FuntionSelect.SelectedItem.ToString() == "Bezier")
                                {
                                    bool find = false;
                                    foreach (var temp in ListForm)
                                    {
                                        if (temp.Text == "Joint,Z轴移动限制,块处理模式")
                                        {
                                            find = true;
                                            temp.TopLevel = true;
                                            break;
                                        }
                                    }
                                    if (!find)
                                    {
                                        JointBezierMode NewOpen = new JointBezierMode();
                                        NewOpen.Text = "Joint,Z轴移动限制,块处理模式";
                                        NewOpen.SetDate = new TheDataForBezier("Limit_Move_Z", Limit_MoveLow_FirstZNummer.Text, Limit_MoveLow_LastZNummer.Text, Limit_MoveHigh_FirstZNummer.Text, Limit_MoveHigh_LastZNummer.Text, JointListForOpera.ToArray());
                                        NewOpen.SetDate.ItemCount = new int[JointListForOpera.Count];
                                        for (int x = 0; x < JointListForOpera.Count; x++)
                                        {
                                            try
                                            {
                                                NewOpen.SetDate.ItemCount[x] = JointListForOpera[x].Count[0];
                                            }
                                            catch (Exception)
                                            {
                                                NewOpen.SetDate.ItemCount[x] = 0;
                                            }
                                        }
                                        NewOpen.SetDate.UseMode = 2;
                                        NewOpen.Show(this.Owner);
                                        ListForm.Add(NewOpen);
                                    }
                                }
                                else
                                {
                                    List<float> Temp = new List<float>(TheFunOfMath(Limit_Move_Z_FuntionSelect.SelectedItem.ToString(), JointListForOpera.Count, float.Parse(Limit_MoveLow_FirstZNummer.Text), float.Parse(Limit_MoveLow_LastZNummer.Text), 0));
                                    List<float> Temp2 = new List<float>(TheFunOfMath(Limit_Move_Z_FuntionSelect.SelectedItem.ToString(), JointListForOpera.Count, float.Parse(Limit_MoveHigh_FirstZNummer.Text), float.Parse(Limit_MoveHigh_LastZNummer.Text), 0));
                                    for (int i = 0; i < JointListForOpera.Count; i++)
                                    {
                                        foreach (int JointTemp in JointListForOpera[i].Count)
                                        {
                                            ThePmxOfNow.Joint[JointTemp].Limit_MoveLow.Z = Temp[i];
                                            ThePmxOfNow.Joint[JointTemp].Limit_MoveHigh.Z = Temp2[i];
                                        }
                                    }
                                }
                            }
                            catch (Exception) { }
                            break;

                        case "Limit_Angle_FuntionSelect_X":
                            try
                            {
                                if (Limit_Angle_FuntionSelect_X.SelectedItem.ToString() == "Bezier")
                                {
                                    bool find = false;
                                    foreach (var temp in ListForm)
                                    {
                                        if (temp.Text == "Joint,X轴旋转限制,块处理模式")
                                        {
                                            find = true;
                                            temp.TopLevel = true;
                                            break;
                                        }
                                    }
                                    if (!find)
                                    {
                                        JointBezierMode NewOpen = new JointBezierMode();
                                        NewOpen.Text = "Joint,X轴旋转限制,块处理模式";
                                        NewOpen.SetDate = new TheDataForBezier("Limit_Angle_X", Limit_AngleLow_FirstXNummer.Text, Limit_AngleLow_LastXNummer.Text, Limit_AngleHigh_FirstXNummer.Text, Limit_AngleHigh_LastXNummer.Text, JointListForOpera.ToArray());
                                        NewOpen.SetDate.ItemCount = new int[JointListForOpera.Count];
                                        for (int x = 0; x < JointListForOpera.Count; x++)
                                        {
                                            try
                                            {
                                                NewOpen.SetDate.ItemCount[x] = JointListForOpera[x].Count[0];
                                            }
                                            catch (Exception)
                                            {
                                                NewOpen.SetDate.ItemCount[x] = 0;
                                            }
                                        }
                                        NewOpen.SetDate.UseMode = 2;
                                        NewOpen.Show(this.Owner);
                                        ListForm.Add(NewOpen);
                                    }
                                }
                                else
                                {
                                    List<float> Temp = new List<float>(TheFunOfMath(Limit_Angle_FuntionSelect_X.SelectedItem.ToString(), JointListForOpera.Count, float.Parse(Limit_AngleLow_FirstXNummer.Text), float.Parse(Limit_AngleLow_LastXNummer.Text), 0));
                                    List<float> Temp2 = new List<float>(TheFunOfMath(Limit_Angle_FuntionSelect_X.SelectedItem.ToString(), JointListForOpera.Count, float.Parse(Limit_AngleHigh_FirstXNummer.Text), float.Parse(Limit_AngleHigh_LastXNummer.Text), 0));
                                    for (int i = 0; i < JointListForOpera.Count; i++)
                                    {
                                        foreach (int JointTemp in JointListForOpera[i].Count)
                                        {
                                            if (Limit_Angle_FuntionSelect_X.SelectedItem.ToString() == "=A/X" || Limit_Angle_FuntionSelect_X.SelectedItem.ToString() == "=-A/X+B")
                                            {
                                                ThePmxOfNow.Joint[JointTemp].Limit_AngleLow.X = -(float)((Temp[i] * Math.PI) / 180);
                                            }
                                            else
                                            {
                                                ThePmxOfNow.Joint[JointTemp].Limit_AngleLow.X = (float)((Temp[i] * Math.PI) / 180);
                                            }
                                            ThePmxOfNow.Joint[JointTemp].Limit_AngleHigh.X = (float)((Temp2[i] * Math.PI) / 180);
                                        }
                                    }
                                }
                            }
                            catch (Exception) { }
                            break;

                        case "Limit_AngleHigh_LastYNummer":
                            try
                            {
                                if (Limit_Angle_FuntionSelect_Y.SelectedItem.ToString() == "Bezier")
                                {
                                    bool find = false;
                                    foreach (var temp in ListForm)
                                    {
                                        if (temp.Text == "Joint,Y轴旋转限制,块处理模式")
                                        {
                                            find = true;
                                            temp.TopLevel = true;
                                            break;
                                        }
                                    }
                                    if (!find)
                                    {
                                        JointBezierMode NewOpen = new JointBezierMode();
                                        NewOpen.Text = "Joint,Y轴旋转限制,块处理模式";
                                        NewOpen.SetDate = new TheDataForBezier("Limit_Angle_Y", Limit_AngleLow_FirstYNummer.Text, Limit_AngleLow_LastYNummer.Text, Limit_AngleHigh_FirstYNummer.Text, Limit_AngleHigh_LastYNummer.Text, JointListForOpera.ToArray());
                                        NewOpen.SetDate.ItemCount = new int[JointListForOpera.Count];
                                        for (int x = 0; x < JointListForOpera.Count; x++)
                                        {
                                            try
                                            {
                                                NewOpen.SetDate.ItemCount[x] = JointListForOpera[x].Count[0];
                                            }
                                            catch (Exception)
                                            {
                                                NewOpen.SetDate.ItemCount[x] = 0;
                                            }
                                        }
                                        NewOpen.SetDate.UseMode = 2;
                                        NewOpen.Show(this.Owner);
                                        ListForm.Add(NewOpen);
                                    }
                                }
                                else
                                {
                                    List<float> Temp = new List<float>(TheFunOfMath(Limit_Angle_FuntionSelect_Y.SelectedItem.ToString(), JointListForOpera.Count, float.Parse(Limit_AngleLow_FirstYNummer.Text), float.Parse(Limit_AngleLow_LastYNummer.Text), 0));
                                    List<float> Temp2 = new List<float>(TheFunOfMath(Limit_Angle_FuntionSelect_Y.SelectedItem.ToString(), JointListForOpera.Count, float.Parse(Limit_AngleHigh_FirstYNummer.Text), float.Parse(Limit_AngleHigh_LastYNummer.Text), 0));
                                    for (int i = 0; i < JointListForOpera.Count; i++)
                                    {
                                        foreach (int JointTemp in JointListForOpera[i].Count)
                                        {
                                            if (Limit_Angle_FuntionSelect_Y.SelectedItem.ToString() == "=A/X" || Limit_Angle_FuntionSelect_Y.SelectedItem.ToString() == "=-A/X+B")
                                            {
                                                ThePmxOfNow.Joint[JointTemp].Limit_AngleLow.Y = -(float)((Temp[i] * Math.PI) / 180);
                                            }
                                            else
                                            {
                                                ThePmxOfNow.Joint[JointTemp].Limit_AngleLow.Y = (float)((Temp[i] * Math.PI) / 180);
                                            }
                                            ThePmxOfNow.Joint[JointTemp].Limit_AngleHigh.Y = (float)((Temp2[i] * Math.PI) / 180);
                                        }
                                    }
                                }
                            }
                            catch (Exception) { }
                            break;

                        case "Limit_AngleHigh_LastZNummer":
                            try
                            {
                                if (Limit_Angle_FuntionSelect_Z.SelectedItem.ToString() == "Bezier")
                                {
                                    bool find = false;
                                    foreach (var temp in ListForm)
                                    {
                                        if (temp.Text == "Joint,Z轴旋转限制,块处理模式")
                                        {
                                            find = true;
                                            temp.TopLevel = true;
                                            break;
                                        }
                                    }
                                    if (!find)
                                    {
                                        JointBezierMode NewOpen = new JointBezierMode();
                                        NewOpen.Text = "Joint,Z轴旋转限制,块处理模式";
                                        NewOpen.SetDate = new TheDataForBezier("Limit_Angle_Z", Limit_AngleLow_FirstZNummer.Text, Limit_AngleLow_LastZNummer.Text, Limit_AngleHigh_FirstZNummer.Text, Limit_AngleHigh_LastZNummer.Text, JointListForOpera.ToArray());
                                        NewOpen.SetDate.ItemCount = new int[JointListForOpera.Count];
                                        for (int x = 0; x < JointListForOpera.Count; x++)
                                        {
                                            try
                                            {
                                                NewOpen.SetDate.ItemCount[x] = JointListForOpera[x].Count[0];
                                            }
                                            catch (Exception)
                                            {
                                                NewOpen.SetDate.ItemCount[x] = 0;
                                            }
                                        }
                                        NewOpen.SetDate.UseMode = 2;
                                        NewOpen.Show(this.Owner);
                                        ListForm.Add(NewOpen);
                                    }
                                }
                                else
                                {
                                    List<float> Temp = new List<float>(TheFunOfMath(Limit_Angle_FuntionSelect_Z.SelectedItem.ToString(), JointListForOpera.Count, float.Parse(Limit_AngleLow_FirstZNummer.Text), float.Parse(Limit_AngleLow_LastZNummer.Text), 0));
                                    List<float> Temp2 = new List<float>(TheFunOfMath(Limit_Angle_FuntionSelect_Z.SelectedItem.ToString(), JointListForOpera.Count, float.Parse(Limit_AngleHigh_FirstZNummer.Text), float.Parse(Limit_AngleHigh_LastZNummer.Text), 0));
                                    for (int i = 0; i < JointListForOpera.Count; i++)
                                    {
                                        foreach (int JointTemp in JointListForOpera[i].Count)
                                        {
                                            if (Limit_Angle_FuntionSelect_Z.SelectedItem.ToString() == "=A/X" || Limit_Angle_FuntionSelect_Z.SelectedItem.ToString() == "=-A/X+B")
                                            {
                                                ThePmxOfNow.Joint[JointTemp].Limit_AngleLow.Z = -(float)((Temp[i] * Math.PI) / 180);
                                            }
                                            else
                                            {
                                                ThePmxOfNow.Joint[JointTemp].Limit_AngleLow.Z = (float)((Temp[i] * Math.PI) / 180);
                                            }
                                            ThePmxOfNow.Joint[JointTemp].Limit_AngleHigh.Z = (float)((Temp2[i] * Math.PI) / 180);
                                        }
                                    }
                                }
                            }
                            catch (Exception) { }
                            break;
                    }
                    ARGS.Host.Connector.Pmx.Update(ThePmxOfNow);
                    ARGS.Host.Connector.Form.UpdateList(UpdateObject.Joint);
                }
                else
                {
                    MetroMessageBox.Show(this, "请先添加列表后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
        }

        public void Limit_Move_Check_Click(object sender, EventArgs e)
        {
            IPXPmx ThePmxOfNow = GetPmx;
            if (JointOperaMode1.Checked)
            {
                if (JointCount.Count != 0)
                {
                    if (Limit_Move_X_FuntionSelect.SelectedItem.ToString() == "Bezier" && Limit_Move_Y_FuntionSelect.SelectedItem.ToString() == "Bezier" && Limit_Move_Z_FuntionSelect.SelectedItem.ToString() == "Bezier")
                    {
                        bool find = false;
                        foreach (var temp in ListForm)
                        {
                            if (temp.Text == "Joint,移动限制")
                            {
                                find = true;
                                temp.TopLevel = true;
                                break;
                            }
                        }
                        if (!find)
                        {
                            JointBezierModeSetAll NewOpen = new JointBezierModeSetAll();
                            NewOpen.Text = "Joint,移动限制";
                            NewOpen.SetDate1 = new TheDataForBezier("Limit_Move_X", Limit_MoveLow_FirstXNummer.Text, Limit_MoveLow_LastXNummer.Text, Limit_MoveHigh_FirstXNummer.Text, Limit_MoveHigh_LastXNummer.Text, JointCount.ToArray());
                            NewOpen.SetDate2 = new TheDataForBezier("Limit_Move_Y", Limit_MoveLow_FirstYNummer.Text, Limit_MoveLow_LastYNummer.Text, Limit_MoveHigh_FirstYNummer.Text, Limit_MoveHigh_LastYNummer.Text, JointCount.ToArray());
                            NewOpen.SetDate3 = new TheDataForBezier("Limit_Move_Z", Limit_MoveLow_FirstZNummer.Text, Limit_MoveLow_LastZNummer.Text, Limit_MoveHigh_FirstZNummer.Text, Limit_MoveHigh_LastZNummer.Text, JointCount.ToArray());
                            NewOpen.UseMode = 0;
                            NewOpen.Show(this.Owner);
                            ListForm.Add(NewOpen);
                        }
                    }
                    else
                    {
                        try
                        {
                            {
                                if (Limit_Move_X_FuntionSelect.SelectedItem.ToString() == "Bezier")
                                {
                                    bool find = false;
                                    foreach (var temp in ListForm)
                                    {
                                        if (temp.Text == "Joint,X轴移动限制")
                                        {
                                            find = true;
                                            temp.TopLevel = true;
                                            break;
                                        }
                                    }
                                    if (!find)
                                    {
                                        JointBezierMode NewOpen = new JointBezierMode();
                                        NewOpen.Text = "Joint,X轴移动限制";
                                        NewOpen.SetDate = new TheDataForBezier("Limit_Move_X", Limit_MoveLow_FirstXNummer.Text, Limit_MoveLow_LastXNummer.Text, Limit_MoveHigh_FirstXNummer.Text, Limit_MoveHigh_LastXNummer.Text, JointCount.ToArray());
                                        NewOpen.Show(this.Owner);
                                        ListForm.Add(NewOpen);
                                    }
                                }
                                else
                                {
                                    List<float> Temp = new List<float>(TheFunOfMath(Limit_Move_X_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_MoveLow_FirstXNummer.Text), float.Parse(Limit_MoveLow_LastXNummer.Text), 0));
                                    List<float> Temp2 = new List<float>(TheFunOfMath(Limit_Move_X_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_MoveHigh_FirstXNummer.Text), float.Parse(Limit_MoveHigh_LastXNummer.Text), 0));
                                    for (int i = 0; i < JointCount.Count; i++)
                                    {
                                        ThePmxOfNow.Joint[JointCount[i]].Limit_MoveLow.X = Temp[i];
                                        ThePmxOfNow.Joint[JointCount[i]].Limit_MoveHigh.X = Temp2[i];
                                    }
                                    ARGS.Host.Connector.Pmx.Update(ThePmxOfNow);
                                    ARGS.Host.Connector.Form.UpdateList(UpdateObject.Joint);
                                }
                            }
                            {
                                if (Limit_Move_Y_FuntionSelect.SelectedItem.ToString() == "Bezier")
                                {
                                    bool find = false;
                                    foreach (var temp in ListForm)
                                    {
                                        if (temp.Text == "Joint,Y轴移动限制")
                                        {
                                            find = true;
                                            temp.TopLevel = true;
                                            break;
                                        }
                                    }
                                    if (!find)
                                    {
                                        JointBezierMode NewOpen = new JointBezierMode();
                                        NewOpen.Text = "Joint,Y轴移动限制";
                                        NewOpen.SetDate = new TheDataForBezier("Limit_Move_Y", Limit_MoveLow_FirstYNummer.Text, Limit_MoveLow_LastYNummer.Text, Limit_MoveHigh_FirstYNummer.Text, Limit_MoveHigh_LastYNummer.Text, JointCount.ToArray());
                                        NewOpen.Show(this.Owner);
                                        ListForm.Add(NewOpen);
                                    }
                                }
                                else
                                {
                                    List<float> Temp = new List<float>(TheFunOfMath(Limit_Move_Y_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_MoveLow_FirstYNummer.Text), float.Parse(Limit_MoveLow_LastYNummer.Text), 0));
                                    List<float> Temp2 = new List<float>(TheFunOfMath(Limit_Move_Y_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_MoveHigh_FirstYNummer.Text), float.Parse(Limit_MoveHigh_LastYNummer.Text), 0));
                                    for (int i = 0; i < JointCount.Count; i++)
                                    {
                                        ThePmxOfNow.Joint[JointCount[i]].Limit_MoveLow.Y = Temp[i];
                                        ThePmxOfNow.Joint[JointCount[i]].Limit_MoveHigh.Y = Temp2[i];
                                    }
                                    ARGS.Host.Connector.Pmx.Update(ThePmxOfNow);
                                    ARGS.Host.Connector.Form.UpdateList(UpdateObject.Joint);
                                }
                            }
                            {
                                if (Limit_Move_Z_FuntionSelect.SelectedItem.ToString() == "Bezier")
                                {
                                    bool find = false;
                                    foreach (var temp in ListForm)
                                    {
                                        if (temp.Text == "Joint,Z轴移动限制")
                                        {
                                            find = true;
                                            temp.TopLevel = true;
                                            break;
                                        }
                                    }
                                    if (!find)
                                    {
                                        JointBezierMode NewOpen = new JointBezierMode();
                                        NewOpen.Text = "Joint,Z轴移动限制";
                                        NewOpen.SetDate = new TheDataForBezier("Limit_Move_Z", Limit_MoveLow_FirstZNummer.Text, Limit_MoveLow_LastZNummer.Text, Limit_MoveHigh_FirstZNummer.Text, Limit_MoveHigh_LastZNummer.Text, JointCount.ToArray());
                                        NewOpen.Show(this.Owner);
                                        ListForm.Add(NewOpen);
                                    }
                                }
                                else
                                {
                                    List<float> Temp = new List<float>(TheFunOfMath(Limit_Move_Z_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_MoveLow_FirstZNummer.Text), float.Parse(Limit_MoveLow_LastZNummer.Text), 0));
                                    List<float> Temp2 = new List<float>(TheFunOfMath(Limit_Move_Z_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_MoveHigh_FirstZNummer.Text), float.Parse(Limit_MoveHigh_LastZNummer.Text), 0));
                                    for (int i = 0; i < JointCount.Count; i++)
                                    {
                                        ThePmxOfNow.Joint[JointCount[i]].Limit_MoveLow.Z = Temp[i];
                                        ThePmxOfNow.Joint[JointCount[i]].Limit_MoveHigh.Z = Temp2[i];
                                    }
                                    ARGS.Host.Connector.Pmx.Update(ThePmxOfNow);
                                    ARGS.Host.Connector.Form.UpdateList(UpdateObject.Joint);
                                }
                            }
                        }
                        catch (Exception) { }
                        ARGS.Host.Connector.View.PmxView.UpdateView();
                    }
                }
                else
                {
                    MetroMessageBox.Show(this, "请先选择J点后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
            else if (JointOperaMode2.Checked)
            {
                if (JointListForOpera.Count != 0)
                {
                    if (Limit_Move_X_FuntionSelect.SelectedItem.ToString() == "Bezier" && Limit_Move_Y_FuntionSelect.SelectedItem.ToString() == "Bezier" && Limit_Move_Z_FuntionSelect.SelectedItem.ToString() == "Bezier")
                    {
                        bool find = false;
                        foreach (var temp in ListForm)
                        {
                            if (temp.Text == "Joint,移动限制,批量模式")
                            {
                                find = true;
                                temp.TopLevel = true;
                                break;
                            }
                        }
                        if (!find)
                        {
                            JointBezierModeSetAll NewOpen = new JointBezierModeSetAll();
                            NewOpen.Text = "Joint,移动限制,批量模式";
                            NewOpen.SetDate1 = new TheDataForBezier("Limit_Move_X", Limit_MoveLow_FirstXNummer.Text, Limit_MoveLow_LastXNummer.Text, Limit_MoveHigh_FirstXNummer.Text, Limit_MoveHigh_LastXNummer.Text, JointListForOpera.ToArray());
                            NewOpen.SetDate2 = new TheDataForBezier("Limit_Move_Y", Limit_MoveLow_FirstYNummer.Text, Limit_MoveLow_LastYNummer.Text, Limit_MoveHigh_FirstYNummer.Text, Limit_MoveHigh_LastYNummer.Text, JointListForOpera.ToArray());
                            NewOpen.SetDate3 = new TheDataForBezier("Limit_Move_Z", Limit_MoveLow_FirstZNummer.Text, Limit_MoveLow_LastZNummer.Text, Limit_MoveHigh_FirstZNummer.Text, Limit_MoveHigh_LastZNummer.Text, JointListForOpera.ToArray());
                            NewOpen.UseMode = 1;
                            NewOpen.Show(this.Owner);
                            ListForm.Add(NewOpen);
                        }
                    }
                    else
                    {
                        if (Limit_Move_X_FuntionSelect.SelectedItem.ToString() == "Bezier")
                        {
                            bool find = false;
                            foreach (var temp in ListForm)
                            {
                                if (temp.Text == "Joint,X轴移动限制,批处理模式")
                                {
                                    find = true;
                                    temp.TopLevel = true;
                                    break;
                                }
                            }
                            if (!find)
                            {
                                JointBezierMode NewOpen = new JointBezierMode();
                                NewOpen.Text = "Joint,X轴移动限制,批处理模式";
                                NewOpen.SetDate = new TheDataForBezier("Limit_Move_X", Limit_MoveLow_FirstXNummer.Text, Limit_MoveLow_LastXNummer.Text, Limit_MoveHigh_FirstXNummer.Text, Limit_MoveHigh_LastXNummer.Text, JointListForOpera.ToArray());
                                NewOpen.SetDate.UseMode = 1;
                                NewOpen.Show(this.Owner);
                                ListForm.Add(NewOpen);
                            }
                        }
                        else
                        {
                            foreach (OperaList ListTemp in JointListForOpera)
                            {
                                JointCount.Clear();
                                JointCount.AddRange(ListTemp.Count);
                                List<float> Temp = new List<float>(TheFunOfMath(Limit_Move_X_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_MoveLow_FirstXNummer.Text), float.Parse(Limit_MoveLow_LastXNummer.Text), 0));
                                List<float> Temp2 = new List<float>(TheFunOfMath(Limit_Move_X_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_MoveHigh_FirstXNummer.Text), float.Parse(Limit_MoveHigh_LastXNummer.Text), 0));
                                for (int i = 0; i < JointCount.Count; i++)
                                {
                                    ThePmxOfNow.Joint[JointCount[i]].Limit_MoveLow.X = Temp[i];
                                    ThePmxOfNow.Joint[JointCount[i]].Limit_MoveHigh.X = Temp2[i];
                                }
                            }
                            ARGS.Host.Connector.Pmx.Update(ThePmxOfNow);
                            ARGS.Host.Connector.Form.UpdateList(UpdateObject.Joint);
                        }
                        if (Limit_Move_Y_FuntionSelect.SelectedItem.ToString() == "Bezier")
                        {
                            bool find = false;
                            foreach (var temp in ListForm)
                            {
                                if (temp.Text == "Joint,Y轴移动限制,批处理模式")
                                {
                                    find = true;
                                    temp.TopLevel = true;
                                    break;
                                }
                            }
                            if (!find)
                            {
                                JointBezierMode NewOpen = new JointBezierMode();
                                NewOpen.Text = "Joint,Y轴移动限制,批处理模式";
                                NewOpen.SetDate = new TheDataForBezier("Limit_Move_Y", Limit_MoveLow_FirstYNummer.Text, Limit_MoveLow_LastYNummer.Text, Limit_MoveHigh_FirstYNummer.Text, Limit_MoveHigh_LastYNummer.Text, JointListForOpera.ToArray());
                                NewOpen.SetDate.UseMode = 1;
                                NewOpen.Show(this.Owner);
                                ListForm.Add(NewOpen);
                            }
                        }
                        else
                        {
                            foreach (OperaList ListTemp in JointListForOpera)
                            {
                                JointCount.Clear();
                                JointCount.AddRange(ListTemp.Count);
                                List<float> Temp = new List<float>(TheFunOfMath(Limit_Move_Y_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_MoveLow_FirstYNummer.Text), float.Parse(Limit_MoveLow_LastYNummer.Text), 0));
                                List<float> Temp2 = new List<float>(TheFunOfMath(Limit_Move_Y_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_MoveHigh_FirstYNummer.Text), float.Parse(Limit_MoveHigh_LastYNummer.Text), 0));
                                for (int i = 0; i < JointCount.Count; i++)
                                {
                                    ThePmxOfNow.Joint[JointCount[i]].Limit_MoveLow.Y = Temp[i];
                                    ThePmxOfNow.Joint[JointCount[i]].Limit_MoveHigh.Y = Temp2[i];
                                }
                            }
                            ARGS.Host.Connector.Pmx.Update(ThePmxOfNow);
                            ARGS.Host.Connector.Form.UpdateList(UpdateObject.Joint);
                        }
                        if (Limit_Move_Z_FuntionSelect.SelectedItem.ToString() == "Bezier")
                        {
                            bool find = false;
                            foreach (var temp in ListForm)
                            {
                                if (temp.Text == "Joint,Z轴移动限制,批处理模式")
                                {
                                    find = true;
                                    temp.TopLevel = true;
                                    break;
                                }
                            }
                            if (!find)
                            {
                                JointBezierMode NewOpen = new JointBezierMode();
                                NewOpen.Text = "Joint,Z轴移动限制,批处理模式";
                                NewOpen.SetDate = new TheDataForBezier("Limit_Move_Z", Limit_MoveLow_FirstZNummer.Text, Limit_MoveLow_LastZNummer.Text, Limit_MoveHigh_FirstZNummer.Text, Limit_MoveHigh_LastZNummer.Text, JointListForOpera.ToArray());
                                NewOpen.SetDate.UseMode = 1;
                                NewOpen.Show(this.Owner);
                                ListForm.Add(NewOpen);
                            }
                        }
                        else
                        {
                            foreach (OperaList ListTemp in JointListForOpera)
                            {
                                JointCount.Clear();
                                JointCount.AddRange(ListTemp.Count);
                                List<float> Temp = new List<float>(TheFunOfMath(Limit_Move_Z_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_MoveLow_FirstZNummer.Text), float.Parse(Limit_MoveLow_LastZNummer.Text), 0));
                                List<float> Temp2 = new List<float>(TheFunOfMath(Limit_Move_Z_FuntionSelect.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_MoveHigh_FirstZNummer.Text), float.Parse(Limit_MoveHigh_LastZNummer.Text), 0));
                                for (int i = 0; i < JointCount.Count; i++)
                                {
                                    ThePmxOfNow.Joint[JointCount[i]].Limit_MoveLow.Z = Temp[i];
                                    ThePmxOfNow.Joint[JointCount[i]].Limit_MoveHigh.Z = Temp2[i];
                                }
                            }
                            ARGS.Host.Connector.Pmx.Update(ThePmxOfNow);
                            ARGS.Host.Connector.Form.UpdateList(UpdateObject.Joint);
                        }
                    }

                    ARGS.Host.Connector.View.PmxView.UpdateView();
                }
                else
                {
                    MetroMessageBox.Show(this, "请先添加列表后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
            else if (JointOperaMode3.Checked)
            {
                if (JointListForOpera.Count != 0)
                {
                    if (Limit_Move_X_FuntionSelect.SelectedItem.ToString() == "Bezier" && Limit_Move_Y_FuntionSelect.SelectedItem.ToString() == "Bezier" && Limit_Move_Z_FuntionSelect.SelectedItem.ToString() == "Bezier")
                    {
                        bool find = false;
                        foreach (var temp in ListForm)
                        {
                            if (temp.Text == "Joint,移动限制,块处理模式")
                            {
                                find = true;
                                temp.TopLevel = true;
                                break;
                            }
                        }
                        if (!find)
                        {
                            JointBezierModeSetAll NewOpen = new JointBezierModeSetAll();
                            NewOpen.Text = "Joint,移动限制,块处理模式";
                            NewOpen.SetDate1 = new TheDataForBezier("Limit_Move_X", Limit_MoveLow_FirstXNummer.Text, Limit_MoveLow_LastXNummer.Text, Limit_MoveHigh_FirstXNummer.Text, Limit_MoveHigh_LastXNummer.Text, JointListForOpera.ToArray());
                            NewOpen.SetDate2 = new TheDataForBezier("Limit_Move_Y", Limit_MoveLow_FirstYNummer.Text, Limit_MoveLow_LastYNummer.Text, Limit_MoveHigh_FirstYNummer.Text, Limit_MoveHigh_LastYNummer.Text, JointListForOpera.ToArray());
                            NewOpen.SetDate3 = new TheDataForBezier("Limit_Move_Z", Limit_MoveLow_FirstZNummer.Text, Limit_MoveLow_LastZNummer.Text, Limit_MoveHigh_FirstZNummer.Text, Limit_MoveHigh_LastZNummer.Text, JointListForOpera.ToArray());
                            NewOpen.UseMode = 2;
                            NewOpen.SetDate1.ItemCount = new int[JointListForOpera.Count];
                            NewOpen.SetDate2.ItemCount = new int[JointListForOpera.Count];
                            NewOpen.SetDate3.ItemCount = new int[JointListForOpera.Count];
                            for (int x = 0; x < JointListForOpera.Count; x++)
                            {
                                try
                                {
                                    NewOpen.SetDate1.ItemCount[x] = JointListForOpera[x].Count[0];
                                    NewOpen.SetDate2.ItemCount[x] = JointListForOpera[x].Count[0];
                                    NewOpen.SetDate3.ItemCount[x] = JointListForOpera[x].Count[0];
                                }
                                catch (Exception)
                                {
                                    NewOpen.SetDate1.ItemCount[x] = 0;
                                    NewOpen.SetDate2.ItemCount[x] = 0;
                                    NewOpen.SetDate3.ItemCount[x] = 0;
                                }
                            }
                            NewOpen.Show(this.Owner);
                            ListForm.Add(NewOpen);
                        }
                    }
                    else
                    {
                        if (Limit_Move_X_FuntionSelect.SelectedItem.ToString() == "Bezier")
                        {
                            bool find = false;
                            foreach (var temp in ListForm)
                            {
                                if (temp.Text == "Joint,X轴移动限制,块处理模式")
                                {
                                    find = true;
                                    temp.TopLevel = true;
                                    break;
                                }
                            }
                            if (!find)
                            {
                                JointBezierMode NewOpen = new JointBezierMode();
                                NewOpen.Text = "Joint,X轴移动限制,块处理模式";
                                NewOpen.SetDate = new TheDataForBezier("Limit_Move_X", Limit_MoveLow_FirstXNummer.Text, Limit_MoveLow_LastXNummer.Text, Limit_MoveHigh_FirstXNummer.Text, Limit_MoveHigh_LastXNummer.Text, JointListForOpera.ToArray());
                                NewOpen.SetDate.ItemCount = new int[JointListForOpera.Count];
                                for (int x = 0; x < JointListForOpera.Count; x++)
                                {
                                    try
                                    {
                                        NewOpen.SetDate.ItemCount[x] = JointListForOpera[x].Count[0];
                                    }
                                    catch (Exception)
                                    {
                                        NewOpen.SetDate.ItemCount[x] = 0;
                                    }
                                }
                                NewOpen.SetDate.UseMode = 2;
                                NewOpen.Show(this.Owner);
                                ListForm.Add(NewOpen);
                            }
                        }
                        else
                        {
                            List<float> Temp = new List<float>(TheFunOfMath(Limit_Move_X_FuntionSelect.SelectedItem.ToString(), JointListForOpera.Count, float.Parse(Limit_MoveLow_FirstXNummer.Text), float.Parse(Limit_MoveLow_LastXNummer.Text), 0));
                            List<float> Temp2 = new List<float>(TheFunOfMath(Limit_Move_X_FuntionSelect.SelectedItem.ToString(), JointListForOpera.Count, float.Parse(Limit_MoveHigh_FirstXNummer.Text), float.Parse(Limit_MoveHigh_LastXNummer.Text), 0));
                            for (int i = 0; i < JointListForOpera.Count; i++)
                            {
                                foreach (int JointTemp in JointListForOpera[i].Count)
                                {
                                    ThePmxOfNow.Joint[JointTemp].Limit_MoveLow.X = Temp[i];
                                    ThePmxOfNow.Joint[JointTemp].Limit_MoveHigh.X = Temp2[i];
                                }
                            }
                            ARGS.Host.Connector.Pmx.Update(ThePmxOfNow);
                            ARGS.Host.Connector.Form.UpdateList(UpdateObject.Joint);
                        }

                        if (Limit_Move_Y_FuntionSelect.SelectedItem.ToString() == "Bezier")
                        {
                            bool find = false;
                            foreach (var temp in ListForm)
                            {
                                if (temp.Text == "Joint,Y轴移动限制,块处理模式")
                                {
                                    find = true;
                                    temp.TopLevel = true;
                                    break;
                                }
                            }
                            if (!find)
                            {
                                JointBezierMode NewOpen = new JointBezierMode();
                                NewOpen.Text = "Joint,Y轴移动限制,块处理模式";
                                NewOpen.SetDate = new TheDataForBezier("Limit_Move_Y", Limit_MoveLow_FirstYNummer.Text, Limit_MoveLow_LastYNummer.Text, Limit_MoveHigh_FirstYNummer.Text, Limit_MoveHigh_LastYNummer.Text, JointListForOpera.ToArray());
                                NewOpen.SetDate.ItemCount = new int[JointListForOpera.Count];
                                for (int x = 0; x < JointListForOpera.Count; x++)
                                {
                                    try
                                    {
                                        NewOpen.SetDate.ItemCount[x] = JointListForOpera[x].Count[0];
                                    }
                                    catch (Exception)
                                    {
                                        NewOpen.SetDate.ItemCount[x] = 0;
                                    }
                                }
                                NewOpen.SetDate.UseMode = 2;
                                NewOpen.Show(this.Owner);
                                ListForm.Add(NewOpen);
                            }
                        }
                        else
                        {
                            List<float> Temp = new List<float>(TheFunOfMath(Limit_Move_Y_FuntionSelect.SelectedItem.ToString(), JointListForOpera.Count, float.Parse(Limit_MoveLow_FirstYNummer.Text), float.Parse(Limit_MoveLow_LastYNummer.Text), 0));
                            List<float> Temp2 = new List<float>(TheFunOfMath(Limit_Move_Y_FuntionSelect.SelectedItem.ToString(), JointListForOpera.Count, float.Parse(Limit_MoveHigh_FirstYNummer.Text), float.Parse(Limit_MoveHigh_LastYNummer.Text), 0));
                            for (int i = 0; i < JointListForOpera.Count; i++)
                            {
                                foreach (int JointTemp in JointListForOpera[i].Count)
                                {
                                    ThePmxOfNow.Joint[JointTemp].Limit_MoveLow.Y = Temp[i];
                                    ThePmxOfNow.Joint[JointTemp].Limit_MoveHigh.Y = Temp2[i];
                                }
                            }
                            ARGS.Host.Connector.Pmx.Update(ThePmxOfNow);
                            ARGS.Host.Connector.Form.UpdateList(UpdateObject.Joint);
                        }
                        if (Limit_Move_Z_FuntionSelect.SelectedItem.ToString() == "Bezier")
                        {
                            bool find = false;
                            foreach (var temp in ListForm)
                            {
                                if (temp.Text == "Joint,Z轴移动限制,块处理模式")
                                {
                                    find = true;
                                    temp.TopLevel = true;
                                    break;
                                }
                            }
                            if (!find)
                            {
                                JointBezierMode NewOpen = new JointBezierMode();
                                NewOpen.Text = "Joint,Z轴移动限制,块处理模式";
                                NewOpen.SetDate = new TheDataForBezier("Limit_Move_Z", Limit_MoveLow_FirstZNummer.Text, Limit_MoveLow_LastZNummer.Text, Limit_MoveHigh_FirstZNummer.Text, Limit_MoveHigh_LastZNummer.Text, JointListForOpera.ToArray());
                                NewOpen.SetDate.ItemCount = new int[JointListForOpera.Count];
                                for (int x = 0; x < JointListForOpera.Count; x++)
                                {
                                    try
                                    {
                                        NewOpen.SetDate.ItemCount[x] = JointListForOpera[x].Count[0];
                                    }
                                    catch (Exception)
                                    {
                                        NewOpen.SetDate.ItemCount[x] = 0;
                                    }
                                }
                                NewOpen.SetDate.UseMode = 2;
                                NewOpen.Show(this.Owner);
                                ListForm.Add(NewOpen);
                            }
                        }
                        else
                        {
                            List<float> Temp = new List<float>(TheFunOfMath(Limit_Move_Z_FuntionSelect.SelectedItem.ToString(), JointListForOpera.Count, float.Parse(Limit_MoveLow_FirstZNummer.Text), float.Parse(Limit_MoveLow_LastZNummer.Text), 0));
                            List<float> Temp2 = new List<float>(TheFunOfMath(Limit_Move_Z_FuntionSelect.SelectedItem.ToString(), JointListForOpera.Count, float.Parse(Limit_MoveHigh_FirstZNummer.Text), float.Parse(Limit_MoveHigh_LastZNummer.Text), 0));
                            for (int i = 0; i < JointListForOpera.Count; i++)
                            {
                                foreach (int JointTemp in JointListForOpera[i].Count)
                                {
                                    ThePmxOfNow.Joint[JointTemp].Limit_MoveLow.Z = Temp[i];
                                    ThePmxOfNow.Joint[JointTemp].Limit_MoveHigh.Z = Temp2[i];
                                }
                            }
                            ARGS.Host.Connector.Pmx.Update(ThePmxOfNow);
                            ARGS.Host.Connector.Form.UpdateList(UpdateObject.Joint);
                        }
                        ARGS.Host.Connector.View.PmxView.UpdateView();
                    }
                }
                else
                {
                    MetroMessageBox.Show(this, "请先添加列表后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
        }

        public void Limit_Angle_Check_Click(object sender, EventArgs e)
        {
            IPXPmx ThePmxOfNow = GetPmx;
            if (JointOperaMode1.Checked)
            {
                if (JointCount.Count != 0)
                {
                    try
                    {
                        if (Limit_Angle_FuntionSelect_X.SelectedItem.ToString() == "Bezier" && Limit_Angle_FuntionSelect_Y.SelectedItem.ToString() == "Bezier" && Limit_Angle_FuntionSelect_Z.SelectedItem.ToString() == "Bezier")
                        {
                            bool find = false;
                            foreach (var temp in ListForm)
                            {
                                if (temp.Text == "Joint,旋转限制")
                                {
                                    find = true;
                                    temp.TopLevel = true;
                                    break;
                                }
                            }
                            if (!find)
                            {
                                JointBezierModeSetAll NewOpen = new JointBezierModeSetAll();
                                NewOpen.Text = "Joint,旋转限制";
                                NewOpen.SetDate1 = new TheDataForBezier("Limit_Angle_X", Limit_AngleLow_FirstXNummer.Text, Limit_AngleLow_LastXNummer.Text, Limit_AngleHigh_FirstXNummer.Text, Limit_AngleHigh_LastXNummer.Text, JointCount.ToArray());
                                NewOpen.SetDate2 = new TheDataForBezier("Limit_Angle_Y", Limit_AngleLow_FirstYNummer.Text, Limit_AngleLow_LastYNummer.Text, Limit_AngleHigh_FirstYNummer.Text, Limit_AngleHigh_LastYNummer.Text, JointCount.ToArray());
                                NewOpen.SetDate3 = new TheDataForBezier("Limit_Angle_Z", Limit_AngleLow_FirstZNummer.Text, Limit_AngleLow_LastZNummer.Text, Limit_AngleHigh_FirstZNummer.Text, Limit_AngleHigh_LastZNummer.Text, JointCount.ToArray());
                                NewOpen.UseMode = 0;
                                NewOpen.Show(this.Owner);
                                ListForm.Add(NewOpen);
                            }
                        }
                        else
                        {
                            if (Limit_Angle_FuntionSelect_X.SelectedItem.ToString() == "Bezier")
                            {
                                bool find = false;
                                foreach (var temp in ListForm)
                                {
                                    if (temp.Text == "Joint,X轴旋转限制")
                                    {
                                        find = true;
                                        temp.TopLevel = true;
                                        break;
                                    }
                                }
                                if (!find)
                                {
                                    JointBezierMode NewOpen = new JointBezierMode();
                                    NewOpen.Text = "Joint,X轴旋转限制";
                                    NewOpen.SetDate = new TheDataForBezier("Limit_Angle_X", Limit_AngleLow_FirstXNummer.Text, Limit_AngleLow_LastXNummer.Text, Limit_AngleHigh_FirstXNummer.Text, Limit_AngleHigh_LastXNummer.Text, JointCount.ToArray());
                                    NewOpen.Show(this.Owner);
                                    ListForm.Add(NewOpen);
                                }
                            }
                            else
                            {
                                List<float> Temp = new List<float>(TheFunOfMath(Limit_Angle_FuntionSelect_X.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_AngleLow_FirstXNummer.Text), float.Parse(Limit_AngleLow_LastXNummer.Text), 0));
                                List<float> Temp2 = new List<float>(TheFunOfMath(Limit_Angle_FuntionSelect_X.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_AngleHigh_FirstXNummer.Text), float.Parse(Limit_AngleHigh_LastXNummer.Text), 0));
                                for (int i = 0; i < JointCount.Count; i++)
                                {
                                    if (Limit_Angle_FuntionSelect_X.SelectedItem.ToString() == "=A/X" || Limit_Angle_FuntionSelect_X.SelectedItem.ToString() == "=-A/X+B")
                                    {
                                        ThePmxOfNow.Joint[JointCount[i]].Limit_AngleLow.X = -(float)((Temp[i] * Math.PI) / 180);
                                    }
                                    else
                                    {
                                        ThePmxOfNow.Joint[JointCount[i]].Limit_AngleLow.X = (float)((Temp[i] * Math.PI) / 180);
                                    }
                                    ThePmxOfNow.Joint[JointCount[i]].Limit_AngleHigh.X = (float)((Temp2[i] * Math.PI) / 180);
                                }
                                ARGS.Host.Connector.Pmx.Update(ThePmxOfNow);
                                ARGS.Host.Connector.Form.UpdateList(UpdateObject.Joint);
                            }

                            if (Limit_Angle_FuntionSelect_Y.SelectedItem.ToString() == "Bezier")
                            {
                                bool find = false;
                                foreach (var temp in ListForm)
                                {
                                    if (temp.Text == "Joint,Y轴旋转限制")
                                    {
                                        find = true;
                                        temp.TopLevel = true;
                                        break;
                                    }
                                }
                                if (!find)
                                {
                                    JointBezierMode NewOpen = new JointBezierMode();
                                    NewOpen.Text = "Joint,Y轴旋转限制";
                                    NewOpen.SetDate = new TheDataForBezier("Limit_Angle_Y", Limit_AngleLow_FirstYNummer.Text, Limit_AngleLow_LastYNummer.Text, Limit_AngleHigh_FirstYNummer.Text, Limit_AngleHigh_LastYNummer.Text, JointCount.ToArray());
                                    NewOpen.Show(this.Owner);
                                    ListForm.Add(NewOpen);
                                }
                            }
                            else
                            {
                                List<float> Temp = new List<float>(TheFunOfMath(Limit_Angle_FuntionSelect_Y.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_AngleLow_FirstYNummer.Text), float.Parse(Limit_AngleLow_LastYNummer.Text), 0));
                                List<float> Temp2 = new List<float>(TheFunOfMath(Limit_Angle_FuntionSelect_Y.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_AngleHigh_FirstYNummer.Text), float.Parse(Limit_AngleHigh_LastYNummer.Text), 0));
                                for (int i = 0; i < JointCount.Count; i++)
                                {
                                    if (Limit_Angle_FuntionSelect_Y.SelectedItem.ToString() == "=A/X" || Limit_Angle_FuntionSelect_Y.SelectedItem.ToString() == "=-A/X+B")
                                    {
                                        ThePmxOfNow.Joint[JointCount[i]].Limit_AngleLow.Y = -(float)((Temp[i] * Math.PI) / 180);
                                    }
                                    else
                                    {
                                        ThePmxOfNow.Joint[JointCount[i]].Limit_AngleLow.Y = (float)((Temp[i] * Math.PI) / 180);
                                    }
                                    ThePmxOfNow.Joint[JointCount[i]].Limit_AngleHigh.Y = (float)((Temp2[i] * Math.PI) / 180);
                                }
                                ARGS.Host.Connector.Pmx.Update(ThePmxOfNow);
                                ARGS.Host.Connector.Form.UpdateList(UpdateObject.Joint);
                            }

                            if (Limit_Angle_FuntionSelect_Z.SelectedItem.ToString() == "Bezier")
                            {
                                bool find = false;
                                foreach (var temp in ListForm)
                                {
                                    if (temp.Text == "Joint,Z轴旋转限制")
                                    {
                                        find = true;
                                        temp.TopLevel = true;
                                        break;
                                    }
                                }
                                if (!find)
                                {
                                    JointBezierMode NewOpen = new JointBezierMode();
                                    NewOpen.Text = "Joint,Z轴旋转限制";
                                    NewOpen.SetDate = new TheDataForBezier("Limit_Angle_Z", Limit_AngleLow_FirstZNummer.Text, Limit_AngleLow_LastZNummer.Text, Limit_AngleHigh_FirstZNummer.Text, Limit_AngleHigh_LastZNummer.Text, JointCount.ToArray());
                                    NewOpen.Show(this.Owner);
                                    ListForm.Add(NewOpen);
                                }
                            }
                            else
                            {
                                List<float> Temp = new List<float>(TheFunOfMath(Limit_Angle_FuntionSelect_Z.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_AngleLow_FirstZNummer.Text), float.Parse(Limit_AngleLow_LastZNummer.Text), 0));
                                List<float> Temp2 = new List<float>(TheFunOfMath(Limit_Angle_FuntionSelect_Z.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_AngleHigh_FirstZNummer.Text), float.Parse(Limit_AngleHigh_LastZNummer.Text), 0));
                                for (int i = 0; i < JointCount.Count; i++)
                                {
                                    if (Limit_Angle_FuntionSelect_Z.SelectedItem.ToString() == "=A/X" || Limit_Angle_FuntionSelect_Z.SelectedItem.ToString() == "=-A/X+B")
                                    {
                                        ThePmxOfNow.Joint[JointCount[i]].Limit_AngleLow.Z = -(float)((Temp[i] * Math.PI) / 180);
                                    }
                                    else
                                    {
                                        ThePmxOfNow.Joint[JointCount[i]].Limit_AngleLow.Z = (float)((Temp[i] * Math.PI) / 180);
                                    }
                                    ThePmxOfNow.Joint[JointCount[i]].Limit_AngleHigh.Z = (float)((Temp2[i] * Math.PI) / 180);
                                }
                                ARGS.Host.Connector.Pmx.Update(ThePmxOfNow);
                                ARGS.Host.Connector.Form.UpdateList(UpdateObject.Joint);
                            }

                            ARGS.Host.Connector.View.PmxView.UpdateView();
                        }
                    }
                    catch (Exception) { }
                }
                else
                {
                    MetroMessageBox.Show(this, "请先选择J点后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
            else if (JointOperaMode2.Checked)
            {
                if (JointListForOpera.Count != 0)
                {
                    if (Limit_Angle_FuntionSelect_X.SelectedItem.ToString() == "Bezier" && Limit_Angle_FuntionSelect_Y.SelectedItem.ToString() == "Bezier" && Limit_Angle_FuntionSelect_Z.SelectedItem.ToString() == "Bezier")
                    {
                        bool find = false;
                        foreach (var temp in ListForm)
                        {
                            if (temp.Text == "Joint,旋转限制,批量模式")
                            {
                                find = true;
                                temp.TopLevel = true;
                                break;
                            }
                        }
                        if (!find)
                        {
                            JointBezierModeSetAll NewOpen = new JointBezierModeSetAll();
                            NewOpen.Text = "Joint,旋转限制,批量模式";
                            NewOpen.SetDate1 = new TheDataForBezier("Limit_Angle_X", Limit_AngleLow_FirstXNummer.Text, Limit_AngleLow_LastXNummer.Text, Limit_AngleHigh_FirstXNummer.Text, Limit_AngleHigh_LastXNummer.Text, JointListForOpera.ToArray());
                            NewOpen.SetDate2 = new TheDataForBezier("Limit_Angle_Y", Limit_AngleLow_FirstYNummer.Text, Limit_AngleLow_LastYNummer.Text, Limit_AngleHigh_FirstYNummer.Text, Limit_AngleHigh_LastYNummer.Text, JointListForOpera.ToArray());
                            NewOpen.SetDate3 = new TheDataForBezier("Limit_Angle_Z", Limit_AngleLow_FirstZNummer.Text, Limit_AngleLow_LastZNummer.Text, Limit_AngleHigh_FirstZNummer.Text, Limit_AngleHigh_LastZNummer.Text, JointListForOpera.ToArray());
                            NewOpen.UseMode = 1;
                            NewOpen.Show(this.Owner);
                            ListForm.Add(NewOpen);
                        }
                    }
                    else
                    {
                        {
                            if (Limit_Angle_FuntionSelect_X.SelectedItem.ToString() == "Bezier")
                            {
                                bool find = false;
                                foreach (var temp in ListForm)
                                {
                                    if (temp.Text == "Joint,X轴旋转限制,批处理模式")
                                    {
                                        find = true;
                                        temp.TopLevel = true;
                                        break;
                                    }
                                }
                                if (!find)
                                {
                                    JointBezierMode NewOpen = new JointBezierMode();
                                    NewOpen.Text = "Joint,X轴旋转限制,批处理模式";
                                    NewOpen.SetDate = new TheDataForBezier("Limit_Angle_X", Limit_AngleLow_FirstXNummer.Text, Limit_AngleLow_LastXNummer.Text, Limit_AngleHigh_FirstXNummer.Text, Limit_AngleHigh_LastXNummer.Text, JointListForOpera.ToArray());
                                    NewOpen.SetDate.UseMode = 1;
                                    NewOpen.Show(this.Owner);
                                    ListForm.Add(NewOpen);
                                }
                            }
                            else
                            {
                                foreach (OperaList ListTemp in JointListForOpera)
                                {
                                    JointCount.Clear();
                                    JointCount.AddRange(ListTemp.Count);
                                    List<float> Temp = new List<float>(TheFunOfMath(Limit_Angle_FuntionSelect_X.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_AngleLow_FirstXNummer.Text), float.Parse(Limit_AngleLow_LastXNummer.Text), 0));
                                    List<float> Temp2 = new List<float>(TheFunOfMath(Limit_Angle_FuntionSelect_X.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_AngleHigh_FirstXNummer.Text), float.Parse(Limit_AngleHigh_LastXNummer.Text), 0));
                                    for (int i = 0; i < JointCount.Count; i++)
                                    {
                                        if (Limit_Angle_FuntionSelect_X.SelectedItem.ToString() == "=A/X" || Limit_Angle_FuntionSelect_X.SelectedItem.ToString() == "=-A/X+B")
                                        {
                                            ThePmxOfNow.Joint[JointCount[i]].Limit_AngleLow.X = -(float)((Temp[i] * Math.PI) / 180);
                                        }
                                        else
                                        {
                                            ThePmxOfNow.Joint[JointCount[i]].Limit_AngleLow.X = (float)((Temp[i] * Math.PI) / 180);
                                        }
                                        ThePmxOfNow.Joint[JointCount[i]].Limit_AngleHigh.X = (float)((Temp2[i] * Math.PI) / 180);
                                    }
                                    ARGS.Host.Connector.Pmx.Update(ThePmxOfNow);
                                    ARGS.Host.Connector.Form.UpdateList(UpdateObject.Joint);
                                }
                            }
                        }

                        {
                            if (Limit_Angle_FuntionSelect_Y.SelectedItem.ToString() == "Bezier")
                            {
                                bool find = false;
                                foreach (var temp in ListForm)
                                {
                                    if (temp.Text == "Joint,Y轴旋转限制,批处理模式")
                                    {
                                        find = true;
                                        temp.TopLevel = true;
                                        break;
                                    }
                                }
                                if (!find)
                                {
                                    JointBezierMode NewOpen = new JointBezierMode();
                                    NewOpen.Text = "Joint,Y轴旋转限制,批处理模式";
                                    NewOpen.SetDate = new TheDataForBezier("Limit_Angle_Y", Limit_AngleLow_FirstYNummer.Text, Limit_AngleLow_LastYNummer.Text, Limit_AngleHigh_FirstYNummer.Text, Limit_AngleHigh_LastYNummer.Text, JointListForOpera.ToArray());
                                    NewOpen.SetDate.UseMode = 1;
                                    NewOpen.Show(this.Owner);
                                    ListForm.Add(NewOpen);
                                }
                            }
                            else
                            {
                                foreach (OperaList ListTemp in JointListForOpera)
                                {
                                    JointCount.Clear();
                                    JointCount.AddRange(ListTemp.Count);
                                    List<float> Temp = new List<float>(TheFunOfMath(Limit_Angle_FuntionSelect_Y.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_AngleLow_FirstYNummer.Text), float.Parse(Limit_AngleLow_LastYNummer.Text), 0));
                                    List<float> Temp2 = new List<float>(TheFunOfMath(Limit_Angle_FuntionSelect_Y.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_AngleHigh_FirstYNummer.Text), float.Parse(Limit_AngleHigh_LastYNummer.Text), 0));
                                    for (int i = 0; i < JointCount.Count; i++)
                                    {
                                        if (Limit_Angle_FuntionSelect_Y.SelectedItem.ToString() == "=A/X" || Limit_Angle_FuntionSelect_Y.SelectedItem.ToString() == "=-A/X+B")
                                        {
                                            ThePmxOfNow.Joint[JointCount[i]].Limit_AngleLow.Y = -(float)((Temp[i] * Math.PI) / 180);
                                        }
                                        else
                                        {
                                            ThePmxOfNow.Joint[JointCount[i]].Limit_AngleLow.Y = (float)((Temp[i] * Math.PI) / 180);
                                        }
                                        ThePmxOfNow.Joint[JointCount[i]].Limit_AngleHigh.Y = (float)((Temp2[i] * Math.PI) / 180);
                                    }
                                    ARGS.Host.Connector.Pmx.Update(ThePmxOfNow);
                                    ARGS.Host.Connector.Form.UpdateList(UpdateObject.Joint);
                                }
                            }
                        }

                        {
                            if (Limit_Angle_FuntionSelect_Z.SelectedItem.ToString() == "Bezier")
                            {
                                bool find = false;
                                foreach (var temp in ListForm)
                                {
                                    if (temp.Text == "Joint,Z轴旋转限制,批处理模式")
                                    {
                                        find = true;
                                        temp.TopLevel = true;
                                        break;
                                    }
                                }
                                if (!find)
                                {
                                    JointBezierMode NewOpen = new JointBezierMode();
                                    NewOpen.Text = "Joint,Z轴旋转限制,批处理模式";
                                    NewOpen.SetDate = new TheDataForBezier("Limit_Angle_Z", Limit_AngleLow_FirstZNummer.Text, Limit_AngleLow_LastZNummer.Text, Limit_AngleHigh_FirstZNummer.Text, Limit_AngleHigh_LastZNummer.Text, JointListForOpera.ToArray());
                                    NewOpen.SetDate.UseMode = 1;
                                    NewOpen.Show(this.Owner);
                                    ListForm.Add(NewOpen);
                                }
                            }
                            else
                            {
                                foreach (OperaList ListTemp in JointListForOpera)
                                {
                                    JointCount.Clear();
                                    JointCount.AddRange(ListTemp.Count);
                                    List<float> Temp = new List<float>(TheFunOfMath(Limit_Angle_FuntionSelect_Z.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_AngleLow_FirstZNummer.Text), float.Parse(Limit_AngleLow_LastZNummer.Text), 0));
                                    List<float> Temp2 = new List<float>(TheFunOfMath(Limit_Angle_FuntionSelect_Z.SelectedItem.ToString(), JointCount.Count, float.Parse(Limit_AngleHigh_FirstZNummer.Text), float.Parse(Limit_AngleHigh_LastZNummer.Text), 0));
                                    for (int i = 0; i < JointCount.Count; i++)
                                    {
                                        if (Limit_Angle_FuntionSelect_Z.SelectedItem.ToString() == "=A/X" || Limit_Angle_FuntionSelect_Z.SelectedItem.ToString() == "=-A/X+B")
                                        {
                                            ThePmxOfNow.Joint[JointCount[i]].Limit_AngleLow.Z = -(float)((Temp[i] * Math.PI) / 180);
                                        }
                                        else
                                        {
                                            ThePmxOfNow.Joint[JointCount[i]].Limit_AngleLow.Z = (float)((Temp[i] * Math.PI) / 180);
                                        }
                                        ThePmxOfNow.Joint[JointCount[i]].Limit_AngleHigh.Z = (float)((Temp2[i] * Math.PI) / 180);
                                    }
                                    ARGS.Host.Connector.Pmx.Update(ThePmxOfNow);
                                    ARGS.Host.Connector.Form.UpdateList(UpdateObject.Joint);
                                }
                            }
                        }
                        ARGS.Host.Connector.View.PmxView.UpdateView();
                    }
                }
                else
                {
                    MetroMessageBox.Show(this, "请先添加列表后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
            else if (JointOperaMode3.Checked)
            {
                if (JointListForOpera.Count != 0)
                {
                    if (Limit_Angle_FuntionSelect_X.SelectedItem.ToString() == "Bezier" && Limit_Angle_FuntionSelect_Y.SelectedItem.ToString() == "Bezier" && Limit_Angle_FuntionSelect_Z.SelectedItem.ToString() == "Bezier")
                    {
                        bool find = false;
                        foreach (var temp in ListForm)
                        {
                            if (temp.Text == "Joint,旋转限制,块处理模式")
                            {
                                find = true;
                                temp.TopLevel = true;
                                break;
                            }
                        }
                        if (!find)
                        {
                            JointBezierModeSetAll NewOpen = new JointBezierModeSetAll();
                            NewOpen.Text = "Joint,旋转限制,块处理模式";
                            NewOpen.SetDate1 = new TheDataForBezier("Limit_Angle_X", Limit_AngleLow_FirstXNummer.Text, Limit_AngleLow_LastXNummer.Text, Limit_AngleHigh_FirstXNummer.Text, Limit_AngleHigh_LastXNummer.Text, JointListForOpera.ToArray());
                            NewOpen.SetDate2 = new TheDataForBezier("Limit_Angle_Y", Limit_AngleLow_FirstYNummer.Text, Limit_AngleLow_LastYNummer.Text, Limit_AngleHigh_FirstYNummer.Text, Limit_AngleHigh_LastYNummer.Text, JointListForOpera.ToArray());
                            NewOpen.SetDate3 = new TheDataForBezier("Limit_Angle_Z", Limit_AngleLow_FirstZNummer.Text, Limit_AngleLow_LastZNummer.Text, Limit_AngleHigh_FirstZNummer.Text, Limit_AngleHigh_LastZNummer.Text, JointListForOpera.ToArray());
                            NewOpen.UseMode = 2;
                            NewOpen.SetDate1.ItemCount = new int[JointListForOpera.Count];
                            NewOpen.SetDate2.ItemCount = new int[JointListForOpera.Count];
                            NewOpen.SetDate3.ItemCount = new int[JointListForOpera.Count];
                            for (int x = 0; x < JointListForOpera.Count; x++)
                            {
                                try
                                {
                                    NewOpen.SetDate1.ItemCount[x] = JointListForOpera[x].Count[0];
                                    NewOpen.SetDate2.ItemCount[x] = JointListForOpera[x].Count[0];
                                    NewOpen.SetDate3.ItemCount[x] = JointListForOpera[x].Count[0];
                                }
                                catch (Exception)
                                {
                                    NewOpen.SetDate1.ItemCount[x] = 0;
                                    NewOpen.SetDate2.ItemCount[x] = 0;
                                    NewOpen.SetDate3.ItemCount[x] = 0;
                                }
                            }
                            NewOpen.Show(this.Owner);
                            ListForm.Add(NewOpen);
                        }
                    }
                    else
                    {
                        {
                            if (Limit_Angle_FuntionSelect_X.SelectedItem.ToString() == "Bezier")
                            {
                                bool find = false;
                                foreach (var temp in ListForm)
                                {
                                    if (temp.Text == "Joint,X轴旋转限制,块处理模式")
                                    {
                                        find = true;
                                        temp.TopLevel = true;
                                        break;
                                    }
                                }
                                if (!find)
                                {
                                    JointBezierMode NewOpen = new JointBezierMode();
                                    NewOpen.Text = "Joint,X轴旋转限制,块处理模式";
                                    NewOpen.SetDate = new TheDataForBezier("Limit_Angle_X", Limit_AngleLow_FirstXNummer.Text, Limit_AngleLow_LastXNummer.Text, Limit_AngleHigh_FirstXNummer.Text, Limit_AngleHigh_LastXNummer.Text, JointListForOpera.ToArray());
                                    NewOpen.SetDate.ItemCount = new int[JointListForOpera.Count];
                                    for (int x = 0; x < JointListForOpera.Count; x++)
                                    {
                                        try
                                        {
                                            NewOpen.SetDate.ItemCount[x] = JointListForOpera[x].Count[0];
                                        }
                                        catch (Exception)
                                        {
                                            NewOpen.SetDate.ItemCount[x] = 0;
                                        }
                                    }
                                    NewOpen.SetDate.UseMode = 2;
                                    NewOpen.Show(this.Owner);
                                    ListForm.Add(NewOpen);
                                }
                            }
                            else
                            {
                                List<float> Temp = new List<float>(TheFunOfMath(Limit_Angle_FuntionSelect_X.SelectedItem.ToString(), JointListForOpera.Count, float.Parse(Limit_AngleLow_FirstXNummer.Text), float.Parse(Limit_AngleLow_LastXNummer.Text), 0));
                                List<float> Temp2 = new List<float>(TheFunOfMath(Limit_Angle_FuntionSelect_X.SelectedItem.ToString(), JointListForOpera.Count, float.Parse(Limit_AngleHigh_FirstXNummer.Text), float.Parse(Limit_AngleHigh_LastXNummer.Text), 0));
                                for (int i = 0; i < JointListForOpera.Count; i++)
                                {
                                    foreach (int JointTemp in JointListForOpera[i].Count)
                                    {
                                        if (Limit_Angle_FuntionSelect_X.SelectedItem.ToString() == "=A/X" || Limit_Angle_FuntionSelect_X.SelectedItem.ToString() == "=-A/X+B")
                                        {
                                            ThePmxOfNow.Joint[JointTemp].Limit_AngleLow.X = -(float)((Temp[i] * Math.PI) / 180);
                                        }
                                        else
                                        {
                                            ThePmxOfNow.Joint[JointTemp].Limit_AngleLow.X = (float)((Temp[i] * Math.PI) / 180);
                                        }
                                        ThePmxOfNow.Joint[JointTemp].Limit_AngleHigh.X = (float)((Temp2[i] * Math.PI) / 180);
                                    }
                                }
                                ARGS.Host.Connector.Pmx.Update(ThePmxOfNow);
                                ARGS.Host.Connector.Form.UpdateList(UpdateObject.Joint);
                            }
                        }

                        {
                            if (Limit_Angle_FuntionSelect_Y.SelectedItem.ToString() == "Bezier")
                            {
                                bool find = false;
                                foreach (var temp in ListForm)
                                {
                                    if (temp.Text == "Joint,Y轴旋转限制,块处理模式")
                                    {
                                        find = true;
                                        temp.TopLevel = true;
                                        break;
                                    }
                                }
                                if (!find)
                                {
                                    JointBezierMode NewOpen = new JointBezierMode();
                                    NewOpen.Text = "Joint,Y轴旋转限制,块处理模式";
                                    NewOpen.SetDate = new TheDataForBezier("Limit_Angle_Y", Limit_AngleLow_FirstYNummer.Text, Limit_AngleLow_LastYNummer.Text, Limit_AngleHigh_FirstYNummer.Text, Limit_AngleHigh_LastYNummer.Text, JointListForOpera.ToArray());
                                    NewOpen.SetDate.ItemCount = new int[JointListForOpera.Count];
                                    for (int x = 0; x < JointListForOpera.Count; x++)
                                    {
                                        try
                                        {
                                            NewOpen.SetDate.ItemCount[x] = JointListForOpera[x].Count[0];
                                        }
                                        catch (Exception)
                                        {
                                            NewOpen.SetDate.ItemCount[x] = 0;
                                        }
                                    }
                                    NewOpen.SetDate.UseMode = 2;
                                    NewOpen.Show(this.Owner);
                                    ListForm.Add(NewOpen);
                                }
                            }
                            else
                            {
                                List<float> Temp = new List<float>(TheFunOfMath(Limit_Angle_FuntionSelect_Y.SelectedItem.ToString(), JointListForOpera.Count, float.Parse(Limit_AngleLow_FirstYNummer.Text), float.Parse(Limit_AngleLow_LastYNummer.Text), 0));
                                List<float> Temp2 = new List<float>(TheFunOfMath(Limit_Angle_FuntionSelect_Y.SelectedItem.ToString(), JointListForOpera.Count, float.Parse(Limit_AngleHigh_FirstYNummer.Text), float.Parse(Limit_AngleHigh_LastYNummer.Text), 0));
                                for (int i = 0; i < JointListForOpera.Count; i++)
                                {
                                    foreach (int JointTemp in JointListForOpera[i].Count)
                                    {
                                        if (Limit_Angle_FuntionSelect_Y.SelectedItem.ToString() == "=A/X" || Limit_Angle_FuntionSelect_Y.SelectedItem.ToString() == "=-A/X+B")
                                        {
                                            ThePmxOfNow.Joint[JointTemp].Limit_AngleLow.Y = -(float)((Temp[i] * Math.PI) / 180);
                                        }
                                        else
                                        {
                                            ThePmxOfNow.Joint[JointTemp].Limit_AngleLow.Y = (float)((Temp[i] * Math.PI) / 180);
                                        }
                                        ThePmxOfNow.Joint[JointTemp].Limit_AngleHigh.Y = (float)((Temp2[i] * Math.PI) / 180);
                                    }
                                }
                                ARGS.Host.Connector.Pmx.Update(ThePmxOfNow);
                                ARGS.Host.Connector.Form.UpdateList(UpdateObject.Joint);
                            }
                        }

                        {
                            if (Limit_Angle_FuntionSelect_Z.SelectedItem.ToString() == "Bezier")
                            {
                                bool find = false;
                                foreach (var temp in ListForm)
                                {
                                    if (temp.Text == "Joint,Z轴旋转限制,块处理模式")
                                    {
                                        find = true;
                                        temp.TopLevel = true;
                                        break;
                                    }
                                }
                                if (!find)
                                {
                                    JointBezierMode NewOpen = new JointBezierMode();
                                    NewOpen.Text = "Joint,Z轴旋转限制,块处理模式";
                                    NewOpen.SetDate = new TheDataForBezier("Limit_Angle_Z", Limit_AngleLow_FirstZNummer.Text, Limit_AngleLow_LastZNummer.Text, Limit_AngleHigh_FirstZNummer.Text, Limit_AngleHigh_LastZNummer.Text, JointListForOpera.ToArray());
                                    NewOpen.SetDate.ItemCount = new int[JointListForOpera.Count];
                                    for (int x = 0; x < JointListForOpera.Count; x++)
                                    {
                                        try
                                        {
                                            NewOpen.SetDate.ItemCount[x] = JointListForOpera[x].Count[0];
                                        }
                                        catch (Exception)
                                        {
                                            NewOpen.SetDate.ItemCount[x] = 0;
                                        }
                                    }
                                    NewOpen.SetDate.UseMode = 2;
                                    NewOpen.Show(this.Owner);
                                    ListForm.Add(NewOpen);
                                }
                            }
                            else
                            {
                                List<float> Temp = new List<float>(TheFunOfMath(Limit_Angle_FuntionSelect_Z.SelectedItem.ToString(), JointListForOpera.Count, float.Parse(Limit_AngleLow_FirstZNummer.Text), float.Parse(Limit_AngleLow_LastZNummer.Text), 0));
                                List<float> Temp2 = new List<float>(TheFunOfMath(Limit_Angle_FuntionSelect_Z.SelectedItem.ToString(), JointListForOpera.Count, float.Parse(Limit_AngleHigh_FirstZNummer.Text), float.Parse(Limit_AngleHigh_LastZNummer.Text), 0));
                                for (int i = 0; i < JointListForOpera.Count; i++)
                                {
                                    foreach (int JointTemp in JointListForOpera[i].Count)
                                    {
                                        if (Limit_Angle_FuntionSelect_Z.SelectedItem.ToString() == "=A/X" || Limit_Angle_FuntionSelect_Z.SelectedItem.ToString() == "=-A/X+B")
                                        {
                                            ThePmxOfNow.Joint[JointTemp].Limit_AngleLow.Z = -(float)((Temp[i] * Math.PI) / 180);
                                        }
                                        else
                                        {
                                            ThePmxOfNow.Joint[JointTemp].Limit_AngleLow.Z = (float)((Temp[i] * Math.PI) / 180);
                                        }
                                        ThePmxOfNow.Joint[JointTemp].Limit_AngleHigh.Z = (float)((Temp2[i] * Math.PI) / 180);
                                    }
                                }
                                ARGS.Host.Connector.Pmx.Update(ThePmxOfNow);
                                ARGS.Host.Connector.Form.UpdateList(UpdateObject.Joint);
                            }
                        }
                        ARGS.Host.Connector.View.PmxView.UpdateView();
                    }
                }
                else
                {
                    MetroMessageBox.Show(this, "请先添加列表后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
        }

        public void SelectConnectBody_DropDown(object sender, EventArgs e)
        {
            IPXPmx ThePmxOfNow = GetPmx;
            List<string> add = new List<string>();
            foreach (IPXBody temp in ThePmxOfNow.Body)
            {
                add.Add(temp.Name);
            }
            BodySelectGroup.DataSource = add;
        }

        public void HorizontalStart_Click(object sender, EventArgs e)
        {
            if (BoneCount.Count != 0)
            {
                TheFunOfHorizontalConnect();
            }
            else
            {
                MetroMessageBox.Show(this, "请先选择骨骼后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public void BodyOperaMode_CheckedChanged(object sender, EventArgs e)
        {
            if (BodyOperaMode2.Checked || BodyOperaMode3.Checked)
            {
                AddToBodyList.Enabled = true;
                DeleteBodyListItem.Enabled = true;
                ClearBodyList.Enabled = true;
                BodyListCountLabel.Enabled = true;
                BodyCountText.Enabled = true;
            }
            else
            {
                AddToBodyList.Enabled = false;
                DeleteBodyListItem.Enabled = false;
                ClearBodyList.Enabled = false;
                BodyListCountLabel.Enabled = false;
                BodyCountText.Enabled = false;
            }
        }

        public List<OperaList> BodyListForOpera = new List<OperaList>();

        public void AddToBodyList_Click(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem((object state) =>
            {
                if (!AutomaticRadioButton.Checked)
                {
                    if (BodyCount.Count != 0)
                    {
                        if (BodyCountText.Text != "" && int.Parse(BodyCountText.Text) == 0)
                        {
                            BodyListForOpera.Add(new OperaList(new List<int>(ARGS.Host.Connector.View.PmxView.GetSelectedBodyIndices())));
                            BodyList.DataSource = new DataTable();
                            BodyCount.Clear();
                            BodyListCountLabel.Text = "已经添加:" + BodyListForOpera.Count + "项";
                        }
                        else
                        {
                            List<int> tempcount = new List<int>();
                            foreach (var temp in new List<int>(ARGS.Host.Connector.View.PmxView.GetSelectedBodyIndices()))
                            {
                                tempcount.Add(temp);
                                if (tempcount.Count == int.Parse(BodyCountText.Text))
                                {
                                    BodyListForOpera.Add(new OperaList(tempcount));
                                    tempcount = new List<int>();
                                }
                            }
                            BodyList.DataSource = new DataTable();
                            BodyCount.Clear();
                            BodyListCountLabel.Text = "已经添加:" + BodyListForOpera.Count + "项";
                        }
                    }
                    else
                    {
                        MetroMessageBox.Show(this, "请先选择刚体后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
                else
                {
                    if (BodyCountText.Text != "" && int.Parse(BodyCountText.Text) == 0)
                    {
                        var temp = new DataGridViewCell[BodyList.SelectedCells.Count];
                        BodyList.SelectedCells.CopyTo(temp, 0);
                        List<int> bodycount = new List<int>();
                        for (int i = 0; i < BodyList.SelectedCells.Count; i++)
                        {
                            if (temp[i].ColumnIndex == 0)
                            {
                                bodycount.Add(Convert.ToInt32(temp[i].Value));
                            }
                        }
                        bodycount.Sort();
                        BodyListForOpera.Add(new OperaList(bodycount));
                        BodyListCountLabel.Text = "已经添加:" + BodyListForOpera.Count + "项";
                    }
                    else
                    {
                        var temp = new DataGridViewCell[BodyList.SelectedCells.Count];
                        BodyList.SelectedCells.CopyTo(temp, 0);
                        List<int> bodycount = new List<int>();
                        for (int i = 0; i < BodyList.SelectedCells.Count; i++)
                        {
                            if (temp[i].ColumnIndex == 0)
                            {
                                bodycount.Add(Convert.ToInt32(temp[i].Value));
                            }
                        }
                        bodycount.Sort();
                        List<int> tempcount = new List<int>();
                        foreach (var Temp in bodycount)
                        {
                            tempcount.Add(Temp);
                            if (tempcount.Count == int.Parse(BodyCountText.Text))
                            {
                                BodyListForOpera.Add(new OperaList(tempcount));
                                tempcount = new List<int>();
                            }
                        }
                        BodyListCountLabel.Text = "已经添加:" + BodyListForOpera.Count + "项";
                    }
                }
            });
        }

        public void DeleteBodyListItem_Click(object sender, EventArgs e)
        {
            if (BodyListForOpera.Count != 0)
            {
                BodyListForOpera.RemoveAt(BodyListForOpera.Count - 1);
                BodyListCountLabel.Text = "已经添加:" + BodyListForOpera.Count + "项";
            }
            else
            {
                MetroMessageBox.Show(this, "列表已经无项目", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public void ClearBodyList_Click(object sender, EventArgs e)
        {
            if (BodyListForOpera.Count != 0)
            {
                BodyListForOpera.Clear();
                BodyListCountLabel.Text = "已经添加:" + BodyListForOpera.Count + "项";
            }
            else
            {
                MetroMessageBox.Show(this, "列表已经无项目", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public List<OperaList> JointListForOpera = new List<OperaList>();

        public void JointOperaMode1_CheckedChanged(object sender, EventArgs e)
        {
            if (JointOperaMode1.Checked)
            {
                AddToJointList.Enabled = false;
                ClearJointItem.Enabled = false;
                DeleteJointListItem.Enabled = false;
                JointListCountLabel.Enabled = false;
                SaveToJointHis.Enabled = false;
                LoadFromJointHis.Enabled = false;
                SelectJointHis.Enabled = false;
                ClearJointHis.Enabled = false;
                JointCountText.Enabled = false;
            }
            else
            {
                AddToJointList.Enabled = true;
                ClearJointItem.Enabled = true;
                DeleteJointListItem.Enabled = true;
                JointListCountLabel.Enabled = true;
                SaveToJointHis.Enabled = true;
                LoadFromJointHis.Enabled = true;
                SelectJointHis.Enabled = true;
                ClearJointHis.Enabled = true;
                JointCountText.Enabled = true;
            }
        }

        public void ClearJointItem_Click(object sender, EventArgs e)
        {
            if (JointListForOpera.Count != 0)
            {
                JointListForOpera.Clear();
                JointListCountLabel.Text = "已经添加:" + JointListForOpera.Count + "项";
            }
            else
            {
                MetroMessageBox.Show(this, "列表已经无项目", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public void DeleteJointListItem_Click(object sender, EventArgs e)
        {
            if (JointListForOpera.Count != 0)
            {
                JointListForOpera.RemoveAt(JointListForOpera.Count - 1);
                JointListCountLabel.Text = "已经添加:" + JointListForOpera.Count + "项";
            }
            else
            {
                MetroMessageBox.Show(this, "列表已经无项目", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public void AddToJointList_Click(object sender, EventArgs e)
        {
            if (!AutomaticRadioButton.Checked)
            {
                if (JointCount.Count != 0)
                {
                    if (JointCountText.Text != "" && int.Parse(JointCountText.Text) == 0)
                    {
                        JointListForOpera.Add(new OperaList(new List<int>(ARGS.Host.Connector.View.PmxView.GetSelectedJointIndices())));
                        ClearList("joint");
                        JointCount.Clear();
                        JointListCountLabel.Text = "已经添加:" + JointListForOpera.Count + "项";
                    }
                    else
                    {
                        List<int> tempcount = new List<int>();
                        foreach (var temp in new List<int>(ARGS.Host.Connector.View.PmxView.GetSelectedJointIndices()))
                        {
                            tempcount.Add(temp);
                            if (tempcount.Count == int.Parse(JointCountText.Text))
                            {
                                JointListForOpera.Add(new OperaList(tempcount));
                                tempcount = new List<int>();
                            }
                        }
                        ClearList("joint");
                        JointCount.Clear();
                        JointListCountLabel.Text = "已经添加:" + JointListForOpera.Count + "项";
                    }
                }
                else
                {
                    MetroMessageBox.Show(this, "请先选择J点后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
            else
            {
                if (JointCountText.Text != "" && int.Parse(JointCountText.Text) == 0)
                {
                    var temp = new DataGridViewCell[JointList.SelectedCells.Count];
                    JointList.SelectedCells.CopyTo(temp, 0);
                    List<int> jointcount = new List<int>();
                    for (int i = 0; i < JointList.SelectedCells.Count; i++)
                    {
                        if (temp[i].ColumnIndex == 0)
                        {
                            jointcount.Add(Convert.ToInt32(temp[i].Value));
                        }
                    }
                    jointcount.Sort();
                    JointListForOpera.Add(new OperaList(jointcount));
                    JointListCountLabel.Text = "已经添加:" + JointListForOpera.Count + "项";
                }
                else
                {
                    var temp = new DataGridViewCell[JointList.SelectedCells.Count];
                    JointList.SelectedCells.CopyTo(temp, 0);
                    List<int> jointcount = new List<int>();
                    for (int i = 0; i < JointList.SelectedCells.Count; i++)
                    {
                        if (temp[i].ColumnIndex == 0)
                        {
                            jointcount.Add(Convert.ToInt32(temp[i].Value));
                        }
                    }
                    jointcount.Sort();
                    List<int> tempcount = new List<int>();
                    foreach (var Temp in new List<int>(jointcount))
                    {
                        tempcount.Add(Temp);
                        if (tempcount.Count == int.Parse(JointCountText.Text))
                        {
                            JointListForOpera.Add(new OperaList(tempcount));
                            tempcount = new List<int>();
                        }
                    }
                    JointListCountLabel.Text = "已经添加:" + JointListForOpera.Count + "项";
                }
            }
        }

        public class JointHis
        {
            public List<OperaList> HisJoint = new List<OperaList>();

            public string SpringConst_Move_FirstXNummer;
            public string SpringConst_Move_LastXNummer;

            public string SpringConst_Move_FirstYNummer;
            public string SpringConst_Move_LastYNummer;

            public string SpringConst_Move_FirstZNummer;
            public string SpringConst_Move_LastZNummer;

            public string SpringConst_Rotate_FirstXNummer;
            public string SpringConst_Rotate_LastXNummer;

            public string SpringConst_Rotate_FirstYNummer;
            public string SpringConst_Rotate_LastYNummer;

            public string SpringConst_Rotate_FirstZNummer;
            public string SpringConst_Rotate_LastZNummer;

            public string Limit_MoveLow_FirstXNummer;
            public string Limit_MoveLow_LastXNummer;
            public string Limit_MoveHigh_FirstXNummer;
            public string Limit_MoveHigh_LastXNummer;

            public string Limit_MoveLow_FirstYNummer;
            public string Limit_MoveLow_LastYNummer;
            public string Limit_MoveHigh_FirstYNummer;
            public string Limit_MoveHigh_LastYNummer;

            public string Limit_MoveLow_FirstZNummer;
            public string Limit_MoveLow_LastZNummer;
            public string Limit_MoveHigh_FirstZNummer;
            public string Limit_MoveHigh_LastZNummer;

            public string Limit_AngleLow_FirstXNummer;
            public string Limit_AngleLow_LastXNummer;
            public string Limit_AngleHigh_FirstXNummer;
            public string Limit_AngleHigh_LastXNummer;

            public string Limit_AngleLow_FirstYNummer;
            public string Limit_AngleLow_LastYNummer;
            public string Limit_AngleHigh_FirstYNummer;
            public string Limit_AngleHigh_LastYNummer;

            public string Limit_AngleLow_FirstZNummer;
            public string Limit_AngleLow_LastZNummer;
            public string Limit_AngleHigh_FirstZNummer;
            public string Limit_AngleHigh_LastZNummer;

            public int SetMode;

            public JointHis(OperaList[] operaList)
            {
                HisJoint.AddRange(operaList);
            }
        }

        public List<JointHis> jointhis = new List<JointHis>();

        public void SaveToJointHis_Click(object sender, EventArgs e)
        {
            if (JointListForOpera.Count != 0)
            {
                bool find = false;
                foreach (var Temp in jointhis)
                {
                    if (Temp.HisJoint.All(JointListForOpera.Contains) && Temp.HisJoint.Count == JointListForOpera.Count)
                    {
                        Temp.SpringConst_Move_FirstXNummer = SpringConst_Move_FirstXNummer.Text;
                        Temp.SpringConst_Move_LastXNummer = SpringConst_Move_LastXNummer.Text;

                        Temp.SpringConst_Move_FirstYNummer = SpringConst_Move_FirstYNummer.Text;
                        Temp.SpringConst_Move_LastYNummer = SpringConst_Move_LastYNummer.Text;

                        Temp.SpringConst_Move_FirstZNummer = SpringConst_Move_FirstZNummer.Text;
                        Temp.SpringConst_Move_LastZNummer = SpringConst_Move_LastZNummer.Text;

                        Temp.SpringConst_Rotate_FirstXNummer = SpringConst_Rotate_FirstXNummer.Text;
                        Temp.SpringConst_Rotate_LastXNummer = SpringConst_Rotate_LastXNummer.Text;

                        Temp.SpringConst_Rotate_FirstYNummer = SpringConst_Rotate_FirstYNummer.Text;
                        Temp.SpringConst_Rotate_LastYNummer = SpringConst_Rotate_LastYNummer.Text;

                        Temp.SpringConst_Rotate_FirstZNummer = SpringConst_Rotate_FirstZNummer.Text;
                        Temp.SpringConst_Rotate_LastZNummer = SpringConst_Rotate_LastZNummer.Text;

                        Temp.Limit_MoveLow_FirstXNummer = Limit_MoveLow_FirstXNummer.Text;
                        Temp.Limit_MoveLow_LastXNummer = Limit_MoveLow_LastXNummer.Text;
                        Temp.Limit_MoveHigh_FirstXNummer = Limit_MoveHigh_FirstXNummer.Text;
                        Temp.Limit_MoveHigh_LastXNummer = Limit_MoveHigh_LastXNummer.Text;

                        Temp.Limit_MoveLow_FirstYNummer = Limit_MoveLow_FirstYNummer.Text;
                        Temp.Limit_MoveLow_LastYNummer = Limit_MoveLow_LastYNummer.Text;
                        Temp.Limit_MoveHigh_FirstYNummer = Limit_MoveHigh_FirstYNummer.Text;
                        Temp.Limit_MoveHigh_LastYNummer = Limit_MoveHigh_LastYNummer.Text;

                        Temp.Limit_MoveLow_FirstZNummer = Limit_MoveLow_FirstZNummer.Text;
                        Temp.Limit_MoveLow_LastZNummer = Limit_MoveLow_LastZNummer.Text;
                        Temp.Limit_MoveHigh_FirstZNummer = Limit_MoveHigh_FirstZNummer.Text;
                        Temp.Limit_MoveHigh_LastZNummer = Limit_MoveHigh_LastZNummer.Text;

                        Temp.Limit_AngleLow_FirstXNummer = Limit_AngleLow_FirstXNummer.Text;
                        Temp.Limit_AngleLow_LastXNummer = Limit_AngleLow_LastXNummer.Text;
                        Temp.Limit_AngleHigh_FirstXNummer = Limit_AngleHigh_FirstXNummer.Text;
                        Temp.Limit_AngleHigh_LastXNummer = Limit_AngleHigh_LastXNummer.Text;

                        Temp.Limit_AngleLow_FirstYNummer = Limit_AngleLow_FirstYNummer.Text;
                        Temp.Limit_AngleLow_LastYNummer = Limit_AngleLow_LastYNummer.Text;
                        Temp.Limit_AngleHigh_FirstYNummer = Limit_AngleHigh_FirstYNummer.Text;
                        Temp.Limit_AngleHigh_LastYNummer = Limit_AngleHigh_LastYNummer.Text;

                        Temp.Limit_AngleLow_FirstZNummer = Limit_AngleLow_FirstZNummer.Text;
                        Temp.Limit_AngleLow_LastZNummer = Limit_AngleLow_LastZNummer.Text;
                        Temp.Limit_AngleHigh_FirstZNummer = Limit_AngleHigh_FirstZNummer.Text;
                        Temp.Limit_AngleHigh_LastZNummer = Limit_AngleHigh_LastZNummer.Text;
                        find = true;
                        break;
                    }
                }
                if (!find)
                {
                    JointHis Addtemp = new JointHis(JointListForOpera.ToArray());

                    Addtemp.SpringConst_Move_FirstXNummer = SpringConst_Move_FirstXNummer.Text;
                    Addtemp.SpringConst_Move_LastXNummer = SpringConst_Move_LastXNummer.Text;

                    Addtemp.SpringConst_Move_FirstYNummer = SpringConst_Move_FirstYNummer.Text;
                    Addtemp.SpringConst_Move_LastYNummer = SpringConst_Move_LastYNummer.Text;

                    Addtemp.SpringConst_Move_FirstZNummer = SpringConst_Move_FirstZNummer.Text;
                    Addtemp.SpringConst_Move_LastZNummer = SpringConst_Move_LastZNummer.Text;

                    Addtemp.SpringConst_Rotate_FirstXNummer = SpringConst_Rotate_FirstXNummer.Text;
                    Addtemp.SpringConst_Rotate_LastXNummer = SpringConst_Rotate_LastXNummer.Text;

                    Addtemp.SpringConst_Rotate_FirstYNummer = SpringConst_Rotate_FirstYNummer.Text;
                    Addtemp.SpringConst_Rotate_LastYNummer = SpringConst_Rotate_LastYNummer.Text;

                    Addtemp.SpringConst_Rotate_FirstZNummer = SpringConst_Rotate_FirstZNummer.Text;
                    Addtemp.SpringConst_Rotate_LastZNummer = SpringConst_Rotate_LastZNummer.Text;

                    Addtemp.Limit_MoveLow_FirstXNummer = Limit_MoveLow_FirstXNummer.Text;
                    Addtemp.Limit_MoveLow_LastXNummer = Limit_MoveLow_LastXNummer.Text;
                    Addtemp.Limit_MoveHigh_FirstXNummer = Limit_MoveHigh_FirstXNummer.Text;
                    Addtemp.Limit_MoveHigh_LastXNummer = Limit_MoveHigh_LastXNummer.Text;

                    Addtemp.Limit_MoveLow_FirstYNummer = Limit_MoveLow_FirstYNummer.Text;
                    Addtemp.Limit_MoveLow_LastYNummer = Limit_MoveLow_LastYNummer.Text;
                    Addtemp.Limit_MoveHigh_FirstYNummer = Limit_MoveHigh_FirstYNummer.Text;
                    Addtemp.Limit_MoveHigh_LastYNummer = Limit_MoveHigh_LastYNummer.Text;

                    Addtemp.Limit_MoveLow_FirstZNummer = Limit_MoveLow_FirstZNummer.Text;
                    Addtemp.Limit_MoveLow_LastZNummer = Limit_MoveLow_LastZNummer.Text;
                    Addtemp.Limit_MoveHigh_FirstZNummer = Limit_MoveHigh_FirstZNummer.Text;
                    Addtemp.Limit_MoveHigh_LastZNummer = Limit_MoveHigh_LastZNummer.Text;

                    Addtemp.Limit_AngleLow_FirstXNummer = Limit_AngleLow_FirstXNummer.Text;
                    Addtemp.Limit_AngleLow_LastXNummer = Limit_AngleLow_LastXNummer.Text;
                    Addtemp.Limit_AngleHigh_FirstXNummer = Limit_AngleHigh_FirstXNummer.Text;
                    Addtemp.Limit_AngleHigh_LastXNummer = Limit_AngleHigh_LastXNummer.Text;

                    Addtemp.Limit_AngleLow_FirstYNummer = Limit_AngleLow_FirstYNummer.Text;
                    Addtemp.Limit_AngleLow_LastYNummer = Limit_AngleLow_LastYNummer.Text;
                    Addtemp.Limit_AngleHigh_FirstYNummer = Limit_AngleHigh_FirstYNummer.Text;
                    Addtemp.Limit_AngleHigh_LastYNummer = Limit_AngleHigh_LastYNummer.Text;

                    Addtemp.Limit_AngleLow_FirstZNummer = Limit_AngleLow_FirstZNummer.Text;
                    Addtemp.Limit_AngleLow_LastZNummer = Limit_AngleLow_LastZNummer.Text;
                    Addtemp.Limit_AngleHigh_FirstZNummer = Limit_AngleHigh_FirstZNummer.Text;
                    Addtemp.Limit_AngleHigh_LastZNummer = Limit_AngleHigh_LastZNummer.Text;

                    if (JointOperaMode2.Checked)
                    {
                        Addtemp.SetMode = 1;
                    }
                    else
                    {
                        Addtemp.SetMode = 0;
                    }

                    jointhis.Add(Addtemp);

                    List<string> add = new List<string>();
                    for (int i = 0; i < jointhis.Count; i++)
                    {
                        add.Add(i.ToString());
                    }
                    SelectJointHis.DataSource = add;
                }
            }
            else
            {
                MetroMessageBox.Show(this, "列表为空,无法保存", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
        }

        public void LoadFromJointHis_Click(object sender, EventArgs e)
        {
            if (jointhis.Count != 0)
            {
                JointHis temp = jointhis[Convert.ToInt16(SelectJointHis.SelectedItem)];
                JointListForOpera = temp.HisJoint;

                JointListCountLabel.Text = "已经添加:" + JointListForOpera.Count + "项";

                SpringConst_Move_FirstXNummer.Text = temp.SpringConst_Move_FirstXNummer;
                SpringConst_Move_LastXNummer.Text = temp.SpringConst_Move_LastXNummer;

                SpringConst_Move_FirstYNummer.Text = temp.SpringConst_Move_FirstYNummer;
                SpringConst_Move_LastYNummer.Text = temp.SpringConst_Move_LastYNummer;

                SpringConst_Move_FirstZNummer.Text = temp.SpringConst_Move_FirstZNummer;
                SpringConst_Move_LastZNummer.Text = temp.SpringConst_Move_LastZNummer;

                SpringConst_Rotate_FirstXNummer.Text = temp.SpringConst_Rotate_FirstXNummer;
                SpringConst_Rotate_LastXNummer.Text = temp.SpringConst_Rotate_LastXNummer;

                SpringConst_Rotate_FirstYNummer.Text = temp.SpringConst_Rotate_FirstYNummer;
                SpringConst_Rotate_LastYNummer.Text = temp.SpringConst_Rotate_LastYNummer;

                SpringConst_Rotate_FirstZNummer.Text = temp.SpringConst_Rotate_FirstZNummer;
                SpringConst_Rotate_LastZNummer.Text = temp.SpringConst_Rotate_LastZNummer;

                Limit_MoveLow_FirstXNummer.Text = temp.Limit_MoveLow_FirstXNummer;
                Limit_MoveLow_LastXNummer.Text = temp.Limit_MoveLow_LastXNummer;
                Limit_MoveHigh_FirstXNummer.Text = temp.Limit_MoveHigh_FirstXNummer;
                Limit_MoveHigh_LastXNummer.Text = temp.Limit_MoveHigh_LastXNummer;

                Limit_MoveLow_FirstYNummer.Text = temp.Limit_MoveLow_FirstYNummer;
                Limit_MoveLow_LastYNummer.Text = temp.Limit_MoveLow_LastYNummer;
                Limit_MoveHigh_FirstYNummer.Text = temp.Limit_MoveHigh_FirstYNummer;
                Limit_MoveHigh_LastYNummer.Text = temp.Limit_MoveHigh_LastYNummer;

                Limit_MoveLow_FirstZNummer.Text = temp.Limit_MoveLow_FirstZNummer;
                Limit_MoveLow_LastZNummer.Text = temp.Limit_MoveLow_LastZNummer;
                Limit_MoveHigh_FirstZNummer.Text = temp.Limit_MoveHigh_FirstZNummer;
                Limit_MoveHigh_LastZNummer.Text = temp.Limit_MoveHigh_LastZNummer;

                Limit_AngleLow_FirstXNummer.Text = temp.Limit_AngleLow_FirstXNummer;
                Limit_AngleLow_LastXNummer.Text = temp.Limit_AngleLow_LastXNummer;
                Limit_AngleHigh_FirstXNummer.Text = temp.Limit_AngleHigh_FirstXNummer;
                Limit_AngleHigh_LastXNummer.Text = temp.Limit_AngleHigh_LastXNummer;

                Limit_AngleLow_FirstYNummer.Text = temp.Limit_AngleLow_FirstYNummer;
                Limit_AngleLow_LastYNummer.Text = temp.Limit_AngleLow_LastYNummer;
                Limit_AngleHigh_FirstYNummer.Text = temp.Limit_AngleHigh_FirstYNummer;
                Limit_AngleHigh_LastYNummer.Text = temp.Limit_AngleHigh_LastYNummer;

                Limit_AngleLow_FirstZNummer.Text = temp.Limit_AngleLow_FirstZNummer;
                Limit_AngleLow_LastZNummer.Text = temp.Limit_AngleLow_LastZNummer;
                Limit_AngleHigh_FirstZNummer.Text = temp.Limit_AngleHigh_FirstZNummer;
                Limit_AngleHigh_LastZNummer.Text = temp.Limit_AngleHigh_LastZNummer;

                if (temp.SetMode == 1)
                {
                    JointOperaMode2.Checked = true;
                }
                else
                {
                    JointOperaMode3.Checked = true;
                }

                if (CheckSyncSelect.Checked)
                {
                    List<int> ShowTemp = new List<int>();
                    foreach (var jointtemp in JointListForOpera)
                    {
                        foreach (var temp2 in jointtemp.Count)
                        {
                            ShowTemp.Add(temp2);
                        }
                    }
                    ARGS.Host.Connector.View.PMDView.SetSelectedJointIndices(ShowTemp.ToArray());
                }
            }
            else
            {
                MetroMessageBox.Show(this, "没有历史记录", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
        }

        public void ClearJointHis_Click(object sender, EventArgs e)
        {
            if (jointhis.Count != 0)
            {
                jointhis.Clear();
                List<string> add = new List<string>();
                for (int i = 0; i < jointhis.Count; i++)
                {
                    add.Add(i.ToString());
                }
                SelectJointHis.DataSource = add;
            }
            else
            {
                MetroMessageBox.Show(this, "没有历史记录", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public delegate void SetTheFunOfChangechangeform();

        public void changeform()
        {
            if (InvokeRequired)
            {
                SetTheFunOfChangechangeform d = new SetTheFunOfChangechangeform(changeform);
                Invoke(d);
            }
            else
            {
                MetroTile temp2 = new MetroTile();
                temp2.Location = new Point(0, new Random().Next(0, 200));
                MetroTabPage page1 = new MetroTabPage();
                page1.Size = new Size(592, 592);
                page1.Text = "new Page";
                page1.Controls.Add(temp2);
                ShortcutKeyTab.TabPages.Add(page1);
            }
        }

        #region 快捷键模块

        public void SetKey_Click(object sender, EventArgs e)
        {
            if (!startset)
            {
                FormItemList.Nodes.Clear();
                ShortCutInfo.Clear();
                startset = true;
                Form PmxForm = ARGS.Host.Connector.Form as Form;
                Form ViewForm = ARGS.Host.Connector.View.PmxView as Form;
                TreeNode node00 = new TreeNode();
                node00.Text = PmxForm.Text;
                List<object> tt = new List<object>();

                foreach (var temp1 in PmxForm.MainMenuStrip.Items)
                {
                    tt.Add(temp1);
                }
                foreach (ToolStripMenuItem temp1 in PmxForm.MainMenuStrip.Items)
                {
                    TreeNode node11 = new TreeNode();
                    node11.Text = temp1.Text;

                    foreach (var temp2 in temp1.DropDownItems)
                    {
                        TreeNode node22 = new TreeNode();
                        if (temp2.GetType().ToString() == "System.Windows.Forms.ToolStripMenuItem")
                        {
                            node22.Text = ((ToolStripMenuItem)temp2).Text;
                            foreach (var temp3 in ((ToolStripMenuItem)temp2).DropDownItems)
                            {
                                TreeNode node33 = new TreeNode();
                                if (temp3.GetType().ToString() == "System.Windows.Forms.ToolStripMenuItem")
                                {
                                    node33.Text = (((ToolStripMenuItem)temp3).Text);
                                    foreach (var temp4 in ((ToolStripMenuItem)temp3).DropDownItems)
                                    {
                                        TreeNode node44 = new TreeNode();
                                        if (temp4.GetType().ToString() == "System.Windows.Forms.ToolStripMenuItem")
                                        {
                                            /*node44.Text = (((ToolStripMenuItem)temp4).Text);
                                             node33.Nodes.Add(node44);*/
                                            ShortCutInfo.Add(new ToolItemInfo((ToolStripMenuItem)temp4, node44.Text));
                                            node33.Nodes.Add(getmuch(temp4));
                                        }
                                    }
                                    node22.Nodes.Add(node33);
                                    ShortCutInfo.Add(new ToolItemInfo((ToolStripMenuItem)temp3, node33.Text));
                                }
                            }
                            node11.Nodes.Add(node22);
                            ShortCutInfo.Add(new ToolItemInfo((ToolStripMenuItem)temp2, node22.Text));
                        }
                    }
                    node00.Nodes.Add(node11);
                    ShortCutInfo.Add(new ToolItemInfo(temp1, node11.Text));
                }

                FormItemList.Nodes.Add(node00);
                TreeNode node01 = new TreeNode();
                node01.Text = ViewForm.Text;
                foreach (var temp1 in ViewForm.MainMenuStrip.Items)
                {
                    if (temp1 is ToolStripMenuItem)
                    {
                        var T1 = temp1 as ToolStripMenuItem;
                        TreeNode node11 = new TreeNode();
                        node11.Text = T1.Text;
                        foreach (var temp2 in T1.DropDownItems)
                        {
                            TreeNode node22 = new TreeNode();
                            if (temp2.GetType().ToString() == "System.Windows.Forms.ToolStripMenuItem")
                            {
                                node22.Text = ((ToolStripMenuItem)temp2).Text;
                                foreach (var temp3 in ((ToolStripMenuItem)temp2).DropDownItems)
                                {
                                    TreeNode node33 = new TreeNode();
                                    if (temp3.GetType().ToString() == "System.Windows.Forms.ToolStripMenuItem")
                                    {
                                        node33.Text = (((ToolStripMenuItem)temp3).Text);
                                        foreach (var temp4 in ((ToolStripMenuItem)temp3).DropDownItems)
                                        {
                                            TreeNode node44 = new TreeNode();
                                            if (temp4.GetType().ToString() == "System.Windows.Forms.ToolStripMenuItem")
                                            {
                                                node44.Text = (((ToolStripMenuItem)temp4).Text);
                                                node33.Nodes.Add(node44);
                                                ShortCutInfo.Add(new ToolItemInfo((ToolStripMenuItem)temp4, node44.Text));
                                            }
                                        }
                                        node22.Nodes.Add(node33);
                                        ShortCutInfo.Add(new ToolItemInfo((ToolStripMenuItem)temp3, node33.Text));
                                    }
                                }
                                node11.Nodes.Add(node22);
                                ShortCutInfo.Add(new ToolItemInfo((ToolStripMenuItem)temp2, node22.Text));
                            }
                        }
                        node01.Nodes.Add(node11);
                        ShortCutInfo.Add(new ToolItemInfo(T1, node11.Text));
                    }
                }
                FormItemList.Nodes.Add(node01);

                AddTabList.Visible = true;
                KeySetLabel1.Visible = true;

                DeleteTabList.Visible = true;
                ChangeTabListName.Visible = true;
                KeySetLabel2.Visible = true;
                KeyItemName.Visible = true;
                AllowDrop.Visible = true;
                KeySetLabel3.Visible = true;
                Keyset.Visible = true;
                DeleteKeyItem.Visible = true;
                AddKeyItem.Visible = true;
                FormItemList.Visible = true;
                ShowShortcutKey.Visible = true;
            }
            else
            {
                AddTabList.Visible = false;
                KeySetLabel1.Visible = false;
                ShowShortcutKey.Visible = false;
                DeleteTabList.Visible = false;
                ChangeTabListName.Visible = false;
                KeySetLabel2.Visible = false;
                KeyItemName.Visible = false;
                AllowDrop.Visible = false;
                KeySetLabel3.Visible = false;
                Keyset.Visible = false;
                DeleteKeyItem.Visible = false;
                AddKeyItem.Visible = false;
                FormItemList.Visible = false;

                startset = false;
            }
        }

        public TreeNode getmuch(object temp)
        {
            TreeNode node = new TreeNode();
            if (temp.GetType().ToString() == "System.Windows.Forms.ToolStripMenuItem")
            {
                node.Text = (((ToolStripMenuItem)temp).Text);
                foreach (var temp4 in ((ToolStripMenuItem)temp).DropDownItems)
                {
                    TreeNode node2 = new TreeNode();
                    if (temp4.GetType().ToString() == "System.Windows.Forms.ToolStripMenuItem")
                    {
                        node2.Text = (((ToolStripMenuItem)temp4).Text);
                        node.Nodes.Add(node2);
                        ShortCutInfo.Add(new ToolItemInfo((ToolStripMenuItem)temp4, node2.Text));
                        this.getmuch(temp4);
                    }
                }
                ShortCutInfo.Add(new ToolItemInfo((ToolStripMenuItem)temp, node.Text));
                return node;
            }
            return node;
        }

        public void AddTabList_Click(object sender, EventArgs e)
        {
            MetroTabPage page1 = new MetroTabPage();
            page1.Text = ChangeTabListName.Text;
            ShortcutKeyTab.TabPages.Add(page1);
        }

        public void ShortcutKeyTab_SelectedIndexChanged(object sender, EventArgs e)
        {
            MetroTabPage page1 = ShortcutKeyTab.SelectedTab as MetroTabPage;
            ChangeTabListName.Text = page1.Text;
        }

        public void DeleteTabList_Click(object sender, EventArgs e)
        {
            try
            {
                MetroTabPage page1 = ShortcutKeyTab.SelectedTab as MetroTabPage;

                List<keySet> Deltemp = new List<keySet>();
                foreach (var temp in KeyData)
                {
                    if (temp.TabID == ShortcutKeyTab.SelectedIndex)
                    {
                        Deltemp.Add(temp);
                    }
                }
                foreach (var Del in Deltemp)
                {
                    KeyData.Remove(Del);
                }

                ShortcutKeyTab.TabPages.Remove(page1);

                List<TabInfo> TabTemp = new List<TabInfo>();
                KeyData.Sort((x, y) => x.TabID.CompareTo(y.TabID));
                foreach (var temp in KeyData)
                {
                    bool add = true;
                    foreach (var temp2 in TabTemp)
                    {
                        if (temp.TabID == temp2.TabID)
                        {
                            temp2.KeyLIst.Add(new TabInfo.KeyInfo(temp.itemname, temp.itemKey, temp.itemLocalX, temp.itemLocaly, temp.itemFun));
                            add = false;
                        }
                    }
                    if (add)
                    {
                        TabTemp.Add(new TabInfo(temp.TabID, temp.TabName, temp.itemname, temp.itemKey, temp.itemLocalX, temp.itemLocaly, temp.itemFun));
                    }
                }
                for (int i = 0; i < TabTemp.Count; i++)
                {
                    TabTemp[i].TabID = i;
                }
                KeyData.Clear();
                foreach (var temp in TabTemp)
                {
                    foreach (var temp2 in temp.KeyLIst)
                    {
                        KeyData.Add(new keySet(temp.TabID, temp.TabName, temp2.itemname, temp2.itemLocalX, temp2.itemLocaly, temp2.itemKey, temp2.itemFun));
                    }
                }
                SaveKeyDataToXML(KeyData);
            }
            catch (Exception) { }
        }

        public void ChangeTabListName_ButtonClick(object sender, EventArgs e)
        {
            try
            {
                MetroTabPage page1 = ShortcutKeyTab.SelectedTab as MetroTabPage;
                page1.Text = ChangeTabListName.Text;
                foreach (var temp in KeyData)
                {
                    if (temp.TabID == ShortcutKeyTab.SelectedIndex)
                    {
                        temp.TabName = ChangeTabListName.Text;
                        SaveKeyDataToXML(KeyData);
                    }
                }
            }
            catch (Exception) { }
            SaveKeyDataToXML(KeyData);
        }

        public void FormItemList_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode Temp = FormItemList.SelectedNode;
            KeyItemName.Text = Temp.Text;
            Keyset.Text = "";
            operaitem = null;
        }

        public int CurX = 0;
        public int CurY = 0;
        public bool Mousedown = false;
        public keySet operaitem;
        public Control tempcontrol;

        public void Controls_MouseDown(object sender, MouseEventArgs e)
        {
            if (startset)
            {
                try
                {
                    CurX = e.X;
                    CurY = e.Y;
                    Mousedown = true;
                    tempcontrol = sender as Control;
                    //MetroTabPage page = ShortcutKeyTab.SelectedTab as MetroTabPage;
                    foreach (var temp in KeyData)
                    {
                        if (new Point(temp.itemLocalX, temp.itemLocaly) == tempcontrol.Location && ShortcutKeyTab.SelectedIndex == temp.TabID)
                        {
                            operaitem = temp;
                            Keyset.Text = temp.itemKey;
                            KeyItemName.Text = temp.itemname;
                            break;
                        }
                    }
                }
                catch (Exception) { }
            }
        }

        public bool startset = false;

        public void Controls_MouseMove(object sender, MouseEventArgs e)
        {
            if (Mousedown && AllowDrop.Checked && startset)
            {
                Point pTemp = new Point(Cursor.Position.X, Cursor.Position.Y);
                pTemp = ShortcutKeyTab.PointToClient(pTemp);
                Control control = sender as Control;
                control.Location = new Point(pTemp.X - CurX, pTemp.Y - 40 - CurY);
            }
        }

        public void Controls_MouseUp(object sender, MouseEventArgs e)
        {
            Mousedown = false;
            if (AllowDrop.Checked && startset)
            {
                Control control = sender as Control;
                foreach (var temp in KeyData)
                {
                    if (new Point(operaitem.itemLocalX, operaitem.itemLocaly) == new Point(temp.itemLocalX, temp.itemLocaly) && ShortcutKeyTab.SelectedIndex == temp.TabID)
                    {
                        temp.itemLocalX = control.Location.X;
                        temp.itemLocaly = control.Location.Y;
                        break;
                    }
                }
                SaveKeyDataToXML(KeyData);
            }
        }

        public void AddKeyItem_Click(object sender, EventArgs e)
        {
            MetroTabPage page = ShortcutKeyTab.SelectedTab as MetroTabPage;
            if (page != null)
            {
                TreeNode Temp = FormItemList.SelectedNode;
                // string itemname = "";
                if (Temp != null)
                {
                    MetroTile TempTile = new MetroTile();
                    KeyData.Sort((x, y) => -x.itemLocaly.CompareTo(y.itemLocaly));
                    var Select = (from T in KeyData where T.TabID == ShortcutKeyTab.SelectedIndex select T).FirstOrDefault();
                    TempTile.Location = new Point(5, Select != null ? Select.itemLocaly + 40 : 5);
                    TempTile.Text = ShowShortcutKey.Checked ? TempTile.Text = KeyItemName.Text + "|" + Keyset.Text : TempTile.Text = KeyItemName.Text;
                    string[] eachchar = KeyItemName.Text.Select(x => x.ToString()).ToArray();
                    TempTile.Size = new Size(eachchar.Length * 13, 38);
                    TempTile.TextAlign = ContentAlignment.TopLeft;
                    TempTile.MouseDown += Controls_MouseDown;
                    TempTile.MouseUp += Controls_MouseUp;
                    TempTile.MouseMove += Controls_MouseMove;
                    TempTile.MouseClick += Controls_MouseClick;
                    KeyData.Add(new keySet(ShortcutKeyTab.SelectedIndex, page.Text, KeyItemName.Text, TempTile.Location.X, TempTile.Location.Y, Keyset.Text, (from INFOTEMP in ShortCutInfo where INFOTEMP.path == Temp.Text select INFOTEMP).FirstOrDefault().Item.Name));
                    page.Controls.Add(TempTile);
                    SaveKeyDataToXML(KeyData);
                }
                else
                {
                    MetroMessageBox.Show(this, "请先添加项后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MetroMessageBox.Show(this, "请先添加/选择列表后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public void Controls_MouseClick(object sender, MouseEventArgs e)
        {
            if (!startset)
            {
                (from temp in KeyData
                 where new Point(temp.itemLocalX, temp.itemLocaly) == (sender as Control).Location && ShortcutKeyTab.SelectedIndex == temp.TabID
                 from INFOTEMP in ShortCutInfo
                 where INFOTEMP.Item.Name == temp.itemFun
                 select INFOTEMP).FirstOrDefault().Item.PerformClick();
            }
        }

        public void DeleteKeyItem_Click(object sender, EventArgs e)
        {
            if (operaitem != null)
            {
                MetroTabPage page = ShortcutKeyTab.SelectedTab as MetroTabPage;
                page.Controls.Remove(tempcontrol);
                foreach (keySet temp in KeyData)
                {
                    if (new Point(operaitem.itemLocalX, operaitem.itemLocaly) == new Point(temp.itemLocalX, temp.itemLocaly))
                    {
                        KeyData.Remove(temp);
                        break;
                    }
                }
                SaveKeyDataToXML(KeyData);
            }
        }

        public void KeyItemName_ButtonClick(object sender, EventArgs e)
        {
            if (operaitem != null)
            {
                foreach (var temp in KeyData)
                {
                    if (new Point(operaitem.itemLocalX, operaitem.itemLocaly) == new Point(temp.itemLocalX, temp.itemLocaly))
                    {
                        temp.itemname = KeyItemName.Text;
                        break;
                    }
                }
                SaveKeyDataToXML(KeyData);
            }
        }

        public void Keyset_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                string[] stringtemp = e.KeyData.ToString().Split(',');
                string temptext = "";
                if (stringtemp.Length > 1)
                {
                    if (e.KeyData.ToString().IndexOf("Control") > -1) { stringtemp[1] = "CTRL"; }
                    if (stringtemp[0] == "ShiftKey")
                    {
                        temptext = stringtemp[1] + "+";
                    }
                    else if (stringtemp[0] == "ControlKey") { temptext = "CTRL" + "+"; }
                    else if (stringtemp[0] == "Menu") { temptext = stringtemp[1] + "+"; }
                    else { temptext = stringtemp[1] + "+" + stringtemp[0]; }
                }
                else
                {
                    temptext = e.KeyData.ToString();
                }
                if (operaitem != null)
                {
                    bool add = true;
                    foreach (var temp in KeyData)
                    {
                        if (new Point(operaitem.itemLocalX, operaitem.itemLocaly) == new Point(temp.itemLocalX, temp.itemLocaly))
                        {
                            foreach (var temp2 in KeyData)
                            {
                                if (temp2.itemKey != "" && temp2.itemKey == temptext)
                                {
                                    add = false;
                                    break;
                                }
                            }
                            if (add)
                            {
                                Keyset.Text = temptext;
                                temp.itemKey = Keyset.Text;
                                break;
                            }
                        }
                    }
                    SaveKeyDataToXML(KeyData);
                }
                else
                {
                    bool add = true;
                    foreach (var temp2 in KeyData)
                    {
                        if (temp2.itemKey != "" && temp2.itemKey == temptext)
                        {
                            add = false;
                            break;
                        }
                    }
                    if (add)
                    {
                        Keyset.Text = temptext;
                    }
                }
            }
            catch (Exception) { }
        }

        public void SaveKeyDataToXML(List<keySet> SaveTemp)
        {
            Stream Filestream = new FileStream(new FileInfo(ARGS.Host.Connector.System.HostApplicationPath).DirectoryName + @"\_data\KeyData.XML", FileMode.Create, FileAccess.Write, FileShare.None);
            IFormatter formatter2 = new BinaryFormatter();
            formatter2.Serialize(Filestream, SaveTemp.ToArray());
            Filestream.Close();
        }

        public void ShowShortcutKey_CheckedChanged(object sender, EventArgs e)
        {
            if (ShowShortcutKey.Checked)
            {
                foreach (var temp in ShortcutKeyTab.TabPages)
                {
                    foreach (var Controlstemp in (temp as MetroTabPage).Controls)
                    {
                        try
                        {
                            MetroTile TempTile = Controlstemp as MetroTile;
                            foreach (var keytemp in KeyData)
                            {
                                if (new Point(keytemp.itemLocalX, keytemp.itemLocaly) == TempTile.Location && keytemp.itemname == TempTile.Text)
                                {
                                    if (keytemp.itemKey != "")
                                    {
                                        TempTile.Text = keytemp.itemname + "|" + keytemp.itemKey;
                                        break;
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
            else
            {
                foreach (var temp in ShortcutKeyTab.TabPages)
                {
                    foreach (var Controlstemp in (temp as MetroTabPage).Controls)
                    {
                        try
                        {
                            MetroTile TempTile = Controlstemp as MetroTile;
                            foreach (var keytemp in KeyData)
                            {
                                if (new Point(keytemp.itemLocalX, keytemp.itemLocaly) == TempTile.Location && keytemp.itemname + "|" + keytemp.itemKey == TempTile.Text)
                                {
                                    if (keytemp.itemKey != "")
                                    {
                                        TempTile.Text = keytemp.itemname;
                                        break;
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
        }

        #endregion

        public void ConnectointNameWithBody_Click(object sender, EventArgs e)
        {
            IPXPmx ThePmxOfNow = GetPmx;
            if (ThePmxOfNow == null)
            {
                ThePmxOfNow = ARGS.Host.Connector.Pmx.GetCurrentState();
            }
            if (!AutomaticRadioButton.Checked)
            {
                if (JointCount.Count != 0)
                {
                    for (int i = 0; i < JointCount.Count; i++)
                    {
                        IPXBody tempbody = null;
                        foreach (var Temp in ThePmxOfNow.Body)
                        {
                            if (Temp.Name == ThePmxOfNow.Joint[JointCount[i]].Name)
                            {
                                tempbody = Temp;
                                break;
                            }
                        }
                        if (tempbody != null)
                        {
                            ThePmxOfNow.Joint[JointCount[i]].BodyB = tempbody;
                        }
                    }
                }
                else
                {
                    MetroMessageBox.Show(this, "请先选择J点后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
            else
            {
                var temp = new DataGridViewCell[JointList.SelectedCells.Count];
                JointList.SelectedCells.CopyTo(temp, 0);
                List<int> jointcount = new List<int>();
                for (int i = 0; i < JointList.SelectedCells.Count; i++)
                {
                    if (temp[i].ColumnIndex == 0)
                    {
                        IPXBody tempbody = null;
                        foreach (var Temp in ThePmxOfNow.Body)
                        {
                            if (Temp.Name == ThePmxOfNow.Joint[Convert.ToInt32(temp[i].Value)].Name)
                            {
                                tempbody = Temp;
                                break;
                            }
                        }
                        if (tempbody != null)
                        {
                            ThePmxOfNow.Joint[Convert.ToInt32(temp[i].Value)].BodyB = tempbody;
                        }
                    }
                }
            }
            ARGS.Host.Connector.Pmx.Update(ThePmxOfNow);
            ARGS.Host.Connector.Form.UpdateList(UpdateObject.Joint);
            ARGS.Host.Connector.View.PMDView.UpdateView();
        }

        public void LoadModel_Click(object sender, EventArgs e)
        {
            var temp = new DataGridViewCell[HisOpenList.SelectedCells.Count];
            HisOpenList.SelectedCells.CopyTo(temp, 0);
            for (int i = 0; i < HisOpenList.SelectedCells.Count; i++)
            {
                if (temp[i].ColumnIndex == 3)
                {
                    openpath = temp[i].Value.ToString();
                    Thread d = new Thread(openmodel);
                    d.Start();
                    break;
                }
            }
        }

        public string openpath = "";

        public void HisOpenList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var Temp = sender as MetroGrid;
            var temp = new DataGridViewCell[Temp.SelectedCells.Count];
            Temp.SelectedCells.CopyTo(temp, 0);
            for (int i = 0; i < Temp.SelectedCells.Count; i++)
            {
                if (temp[i].ColumnIndex == 3)
                {
                    openpath = temp[i].Value.ToString();
                    Thread d = new Thread(openmodel);
                    d.Start();
                    break;
                }
            }
        }

        public delegate void InvokeFormTostart();

        public void openmodel()
        {
            Form Formtemp = ARGS.Host.Connector.Form as Form;
            if (Formtemp.InvokeRequired)
            {
                InvokeFormTostart d = new InvokeFormTostart(openmodel);
                Formtemp.Invoke(d);
            }
            else
            {
                if (new FileInfo(openpath).Exists)
                {
                    ARGS.Host.Connector.Form.OpenPMXFile(openpath);
                }
                else
                {
                    if (MetroMessageBox.Show(this, "未找到模型，是否删除该项？", "", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        var temp = new DataGridViewCell[HisOpenList.SelectedCells.Count];
                        HisOpenList.SelectedCells.CopyTo(temp, 0);
                        for (int i = 0; i < HisOpenList.SelectedCells.Count; i++)
                        {
                            if (temp[i].ColumnIndex == 3)
                            {
                                HisTemp = new List<OpenHis>();
                                HisTemp.AddRange(bootstate.HisOpen.ToArray());
                                foreach (var Deltemp in HisTemp)
                                {
                                    if (Deltemp.modelpath == temp[i].Value.ToString())
                                    {
                                        HisTemp.Remove(Deltemp);
                                        break;
                                    }
                                }
                            }
                        }
                        ThreadPool.QueueUserWorkItem(new WaitCallback(SaveHisInfo));
                    }
                }
                foreach (var temp in bootstate.HisOpen)
                {
                    if (temp.modelpath == openpath)
                    {
                        temp.modeldata = DateTime.Now.ToLocalTime().ToString();
                        break;
                    }
                }
            }
        }

        public void OpenDirectory_Click(object sender, EventArgs e)
        {
            var temp = new DataGridViewCell[HisOpenList.SelectedCells.Count];
            HisOpenList.SelectedCells.CopyTo(temp, 0);
            for (int i = 0; i < HisOpenList.SelectedCells.Count; i++)
            {
                if (temp[i].ColumnIndex == 3)
                {
                    Process.Start("explorer.exe", new FileInfo(temp[i].Value.ToString()).DirectoryName);
                }
            }
        }

        public void DelModelHis_Click(object sender, EventArgs e)
        {
            var temp = new DataGridViewCell[HisOpenList.SelectedCells.Count];
            HisOpenList.SelectedCells.CopyTo(temp, 0);
            for (int i = 0; i < HisOpenList.SelectedCells.Count; i++)
            {
                if (temp[i].ColumnIndex == 3)
                {
                    HisTemp = new List<OpenHis>();
                    HisTemp.AddRange(bootstate.HisOpen.ToArray());
                    foreach (var Deltemp in HisTemp)
                    {
                        if (Deltemp.modelpath == temp[i].Value.ToString())
                        {
                            HisTemp.Remove(Deltemp);
                            break;
                        }
                    }
                }
            }
            ThreadPool.QueueUserWorkItem(new WaitCallback(SaveHisInfo));
        }

        public void SaveHisInfo(object save)
        {
            bootstate.HisOpen = new OpenHis[HisTemp.Count];
            HisTemp.CopyTo(bootstate.HisOpen);
            ThreadPool.QueueUserWorkItem(new WaitCallback(Save));
            BeginInvoke(new MethodInvoker(() =>
            {
                var table = HisOpenList.DataSource as DataTable;
                table.Rows.Clear();
                table.Columns.Clear();
                table.Columns.Add("ID");
                table.Columns.Add("模型名称");
                table.Columns.Add("打开日期");
                table.Columns.Add("模型路径");
                table.Rows.Add();
                table.Rows.Clear();
                if (bootstate.HisOpen != null)
                {
                    HisTemp = new List<OpenHis>();
                    HisTemp.AddRange(bootstate.HisOpen.ToArray());
                    HisTemp.Reverse();
                    for (int i = 0; i < HisTemp.Count; i++)
                    {
                        table.Rows.Add(i + 1, HisTemp[i].modelname, HisTemp[i].modeldata, HisTemp[i].modelpath);
                    }
                }
            }));
        }

        public void ClearHisList_Click(object sender, EventArgs e)
        {
            HisTemp = new List<OpenHis>();
            ThreadPool.QueueUserWorkItem(new WaitCallback(SaveHisInfo));
        }

        public void AutoOpenModel_CheckedChanged(object sender, EventArgs e)
        {
            if (AutoOpenModel.Checked)
            {
                bootstate.AutoOpen = 1;
                ThreadPool.QueueUserWorkItem(new WaitCallback(Save));
            }
            else
            {
                bootstate.AutoOpen = 0;
                ThreadPool.QueueUserWorkItem(new WaitCallback(Save));
            }
        }

        public void Metroform_MouseMove(object sender, MouseEventArgs e)
        {
            bootstate.FormX = this.Location.X;
            bootstate.FormY = this.Location.Y;
            bootstate.FormForSizeX = this.Size.Height;
            bootstate.FormForSizeY = this.Size.Width;
        }

        public void Progress_Spinner_Click(object sender, EventArgs e)
        {
            MetroTaskWindow.ShowTaskWindow(this.Parent, "作者信息", new TaskWindowControl(), 10);
        }

        public void Keyset_ButtonClick(object sender, EventArgs e)
        {
            if (operaitem != null)
            {
                Keyset.Text = "";
                foreach (keySet temp in KeyData)
                {
                    if (new Point(operaitem.itemLocalX, operaitem.itemLocaly) == new Point(temp.itemLocalX, temp.itemLocaly))
                    {
                        temp.itemKey = "";
                        break;
                    }
                }
                SaveKeyDataToXML(KeyData);
            }
        }

        public void ChangeJointNameWithBody_Click(object sender, EventArgs e)
        {
            if (JointCount.Count != 0)
            {
                var temppmx = ARGS.Host.Connector.Pmx.GetCurrentState();
                List<ListChangeInfo> JointChangeList = new List<ListChangeInfo>();
                for (var i = 0; i < JointCount.Count; i++)
                {
                    if (temppmx.Joint[JointCount[i]].BodyB == null)
                    {
                        continue;
                    }
                    temppmx.Joint[JointCount[i]].Name = temppmx.Joint[JointCount[i]].BodyB.Name;
                    if (!AutomaticRadioButton.Checked)
                    {
                        JointChangeList.Add(new ListChangeInfo(i, temppmx.Joint[JointCount[i]].BodyB.Name));
                    }
                    else
                    {
                        JointChangeList.Add(new ListChangeInfo(JointCount[i], temppmx.Joint[JointCount[i]].BodyB.Name));
                    }
                }
                ThFunOfSaveToPmx(temppmx, "Joint");
                TheFunOfChangeListShow(JointChangeList, "Joint");
            }
            else
            {
                MetroMessageBox.Show(this, "请先选择Joint后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void SelectFatherBone_DropDown(object sender, EventArgs e)
        {
            IPXPmx ThePmxOfNow = GetPmx;
            List<string> add = new List<string>();
            for (int i = 0; i < ThePmxOfNow.Bone.Count; i++)
            {
                add.Add(i + ":" + ThePmxOfNow.Bone[i].Name);
            }
            if ((sender as ComboBox).Name == "OriBoneCombox")
            {
                OriBoneCombox.DataSource = add;
            }
            else if ((sender as ComboBox).Name == "FinBoneCombo")
            {
                FinBoneCombo.DataSource = add;
            }
            else
            {
                SelectFatherBone.DataSource = add;
            }
        }

        private List<int> SelectVertexIndex = new List<int>();

        private void GetSelectVertex_Click(object sender, EventArgs e)
        {
            if (ARGS.Host.Connector.View.PmxView.GetSelectedVertexIndices().Length != 0)
            {
                SelectVertexIndex = new List<int>(ARGS.Host.Connector.View.PmxView.GetSelectedVertexIndices());

                ShowHowMuchSelectVertex.Text = "已选中" + SelectVertexIndex.Count + "个顶点";
            }
            else
            {
                MetroMessageBox.Show(this, "请先选择顶点后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public class CreateBoneInfo
        {
            public List<int> Vertex;
            public V3[] NearVertex = new V3[4];
            public V3 BonePoint;
            public string Bonename;
        }

        private void StartSoftBody_Click_1(object sender, EventArgs e)
        {
            if (SelectVertexIndex == null)
            {
                MetroMessageBox.Show(this, "请先选择顶点后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else if (SelectVertexIndex.Count == 0)
            {
                MetroMessageBox.Show(this, "请先选择顶点后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else if (SelectFatherBone.SelectedIndex == -1)
            {
                MetroMessageBox.Show(this, "请先选择亲骨后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            /*  NearVertexPointText = NearVertexPoint.Text;
              BoneTempName= BaseBoneName.Text;*/

            ThreadPool.QueueUserWorkItem((object state) => { StartSetSofyBody(); showvertextext(); });
        }

        public class SoftBoneInfoZwei
        {
            public Double Distance;
            public V3 Bone;
        }

        private void StartSetSofyBody()
        {
            IPXPmx ThePmxOfNow = GetPmx;
            List<CreateBoneInfo> TempBoneInfoList = new List<CreateBoneInfo>();
            SelectVertexIndex.Sort((x, y) => ThePmxOfNow.Vertex[x].Position.Y.CompareTo(ThePmxOfNow.Vertex[y].Position.Y));
            do
            {
                CreateBoneInfo Temp = new CreateBoneInfo();
                Temp.Vertex = new List<int>();
                Temp.BonePoint = ThePmxOfNow.Vertex[SelectVertexIndex[0]].Position;
                Temp.Vertex.Add(SelectVertexIndex[0]);
                SelectVertexIndex.Remove(SelectVertexIndex[0]);
                try
                {
                    double TempDouble = 0;
                    if (double.TryParse(newopen.NearVertexPoint.Text, out TempDouble))
                    {
                        Parallel.ForEach(SelectVertexIndex, temp =>
                        {
                            V3 temppoint = ThePmxOfNow.Vertex[temp].Position;
                            if (Math.Sqrt((Math.Pow(temppoint.X - Temp.BonePoint.X, 2) + Math.Pow(temppoint.Y - Temp.BonePoint.Y, 2) + Math.Pow(temppoint.Z - Temp.BonePoint.Z, 2))) <= TempDouble)
                            {
                                Temp.Vertex.Add(temp);
                            }
                        });
                    }
                }
                catch (Exception) { return; }
                float x = 0;
                float y = 0;
                float z = 0;
                foreach (var temp in Temp.Vertex)
                {
                    V3 temppoint = ThePmxOfNow.Vertex[temp].Position;
                    SelectVertexIndex.Remove(temp);
                    x = x + temppoint.X;
                    y = y + temppoint.Y;
                    z = z + temppoint.Z;
                }
                Temp.BonePoint.X = x / Temp.Vertex.Count;
                Temp.BonePoint.Y = y / Temp.Vertex.Count;
                Temp.BonePoint.Z = z / Temp.Vertex.Count;
                TempBoneInfoList.Add(Temp);
            } while (SelectVertexIndex.Count != 0 && Run);
            if (!Run)
            {
                return;
            }
            IPXPmxBuilder bdx = ARGS.Host.Builder.Pmx;
            int Cunt = 0;
            Thread.Sleep(100);

            /* foreach (var temp in TempBoneInfoList)
             {
                 var TempBone = bdx.Bone();
                 TempBone.Position = temp.BonePoint;
                 TempBone.Name = newopen.BaseBoneName.Text + "_" + Cunt;
                 TempBone.Parent = parentbone;
                 ThePmxOfNow.Bone.Add(TempBone);
                /* ARGS.Host.Connector.Pmx.Update(ThePmxOfNow);
                 ARGS.Host.Connector.Form.UpdateList(UpdateObject.Bone);
                 foreach(var temp2 in temp.Vertex)
                 {
                     ThePmxOfNow.Vertex[temp2].Bone1 = TempBone;
                     ThePmxOfNow.Vertex[temp2].Weight1 = 100;
                 }
             }*/

            List<CreateBoneInfo> TempBoneData = new List<CreateBoneInfo>();
            List<CreateBoneInfo> TempBoneData2 = new List<CreateBoneInfo>(TempBoneInfoList);
            do
            {
                TempBoneInfoList.Sort((x, y) => x.BonePoint.X.CompareTo(y.BonePoint.X));
                TempBoneInfoList.Sort((x, y) => -x.BonePoint.Y.CompareTo(y.BonePoint.Y));
                var TempBone = bdx.Bone();
                TempBone.Position = TempBoneInfoList[0].BonePoint;
                TempBoneInfoList[0].Bonename = TempBone.Name = newopen.BaseBoneName.Text + "_" + Cunt;

                TempBone.Parent = ThePmxOfNow.Bone[newopen.SelectFatherBone.SelectedIndex];
                ThePmxOfNow.Bone.Add(TempBone);
                /* ARGS.Host.Connector.Pmx.Update(ThePmxOfNow);
                 ARGS.Host.Connector.Form.UpdateList(UpdateObject.Bone);*/

                foreach (var temp2 in TempBoneInfoList[0].Vertex)
                {
                    ThePmxOfNow.Vertex[temp2].Bone1 = TempBone;
                    ThePmxOfNow.Vertex[temp2].Weight1 = 100;
                }

                var temppoint = TempBoneInfoList[0].BonePoint;
                List<SoftBoneInfoZwei> Tp = new List<SoftBoneInfoZwei>();
                Parallel.ForEach(TempBoneData2, Temp =>
                {
                    SoftBoneInfoZwei TPTemp = new SoftBoneInfoZwei();
                    TPTemp.Distance = Math.Sqrt((Math.Pow(Temp.BonePoint.X - temppoint.X, 2) + Math.Pow(Temp.BonePoint.Y - temppoint.Y, 2) + Math.Pow(Temp.BonePoint.Z - temppoint.Z, 2)));
                    TPTemp.Bone = Temp.BonePoint;
                    lock (Tp)
                    {
                        Tp.Add(TPTemp);
                    }
                });
                Tp.Sort((x, y) => x.Distance.CompareTo(y.Distance));
                TempBoneInfoList[0].NearVertex[0] = Tp[1].Bone;
                TempBoneInfoList[0].NearVertex[1] = Tp[2].Bone;
                TempBoneInfoList[0].NearVertex[2] = Tp[3].Bone;
                TempBoneInfoList[0].NearVertex[3] = Tp[4].Bone;
                var TempBody = bdx.Body();
                TempBody.BoxKind = BodyBoxKind.Sphere;
                TempBody.Bone = TempBone;
                TempBody.BoxSize = new V3((float)(Math.Sqrt((Math.Pow(TempBoneInfoList[0].NearVertex[0].X - temppoint.X, 2) + Math.Pow(TempBoneInfoList[0].NearVertex[0].Y - temppoint.Y, 2) + Math.Pow(TempBoneInfoList[0].NearVertex[0].Z - temppoint.Z, 2)))), 0, 0);
                TempBody.BoxKind = BodyBoxKind.Sphere;
                TempBody.Name = TempBoneInfoList[0].Bonename;
                TempBody.Position = TempBoneInfoList[0].BonePoint;
                TempBody.Friction = 0.5f;
                TempBody.Mass = 1;
                TempBody.Group = 9;
                TempBody.PassGroup[9] = true;
                TempBody.Mode = BodyMode.Dynamic;
                TempBody.Restitution = 0f;
                TempBody.PositionDamping = 0.5f;
                TempBody.RotationDamping = 0.5f;
                ThePmxOfNow.Body.Add(TempBody);
                TempBoneData.Add(TempBoneInfoList[0]);
                TempBoneInfoList.RemoveAt(0);
                Cunt++;
            } while (TempBoneInfoList.Count > 0);

            List<IPXJoint> TempJoint = new List<IPXJoint>();
            for (int Ic = 0; Ic < 2; Ic++)
            {
                Cunt = 0;
                do
                {
                    var BuildJoint = bdx.Joint();
                    IPXBody tempbodyA = null;
                    IPXBody tempbodyB = null;

                    Parallel.ForEach(ThePmxOfNow.Body, (Temp, State) =>
                    {
                        if (Temp.Name == TempBoneData[Cunt].Bonename)
                        {
                            tempbodyA = Temp;
                            State.Stop();
                            return;
                        }
                    });
                    foreach (var t in TempBoneData[Cunt].NearVertex)
                    {
                        Parallel.ForEach(ThePmxOfNow.Body, (Temp, State) =>
                        {
                            if (Temp.Position == t)
                            {
                                tempbodyB = Temp;
                                State.Stop();
                                return;
                            }
                        });
                        bool find = false;
                        foreach (var T2 in TempJoint)
                        {
                            if (tempbodyA == T2.BodyA && tempbodyB == T2.BodyB)
                            {
                                find = true;
                            }
                            if (tempbodyB == T2.BodyA && tempbodyA == T2.BodyB)
                            {
                                find = true;
                            }
                        }
                        if (!find)
                        {
                            break;
                        }
                    }

                    BuildJoint.Name = TempBoneData[Cunt].Bonename;
                    BuildJoint.BodyA = tempbodyA;
                    BuildJoint.BodyB = tempbodyB;
                    BuildJoint.Position = new V3((tempbodyA.Position.X + tempbodyB.Position.X) / 2, (tempbodyA.Position.Y + tempbodyB.Position.Y) / 2, (tempbodyA.Position.Z + tempbodyB.Position.Z) / 2);
                    TempJoint.Add(BuildJoint);
                    Cunt++;
                } while (TempBoneData.Count > Cunt);
            }
            TempJoint.ForEach(Joint => ThePmxOfNow.Joint.Add(Joint));
            ThFunOfSaveToPmx(ThePmxOfNow, "Bone");
            ThFunOfSaveToPmx(ThePmxOfNow, "Body");
            ThFunOfSaveToPmx(ThePmxOfNow, "Joint");
        }

        private void showvertextext()
        {
            try
            {
                if (SelectVertexIndex.Count != 0)
                {
                    do
                    {
                        Thread.Sleep(10);
                        if (ShowHowMuchSelectVertex.InvokeRequired)
                        {
                            ShowHowMuchSelectVertex.Invoke(new Action(() => { ShowHowMuchSelectVertex.Text = "已选中" + SelectVertexIndex.Count + "个顶点"; }));
                        }
                        else
                        {
                            ShowHowMuchSelectVertex.Text = "已选中" + SelectVertexIndex.Count + "个顶点";
                        }
                    } while (SelectVertexIndex.Count != 0);
                    ShowHowMuchSelectVertex.Text = "已选中" + SelectVertexIndex.Count + "个顶点";
                }
            }
            catch (Exception) { }
        }

        private void SortBone_Click(object sender, EventArgs e)
        {
            IPXPmx ThePmxOfNow = GetPmx;
            if (BoneCount.Count != 0)
            {
                if (!AutomaticRadioButton.Checked)
                {
                    BoneCount.Clear();
                    BoneCount.AddRange(ARGS.Host.Connector.View.PmxView.GetSelectedBoneIndices());
                    List<IPXBone> BoneTemp = new List<IPXBone>();
                    foreach (var t in Sortbone(new List<int>(BoneCount)))
                    {
                        BoneTemp.Add(ThePmxOfNow.Bone[t]);
                    }
                    List<ListChangeInfo> BoneChangeList = new List<ListChangeInfo>();
                    var tempString = InputBoneName.Text;
                    for (int i = 0; i < BoneCount.Count; i++)
                    {
                        ThePmxOfNow.Bone[BoneCount[i]] = BoneTemp[i];
                        if (!AutomaticRadioButton.Checked)
                        {
                            BoneChangeList.Add(new ListChangeInfo(i, ThePmxOfNow.Bone[BoneCount[i]].Name));
                        }
                        else
                        {
                            BoneChangeList.Add(new ListChangeInfo(BoneCount[i], ThePmxOfNow.Bone[BoneCount[i]].Name));
                        }
                    }
                    ThFunOfSaveToPmx(ThePmxOfNow, "Bone");
                    TheFunOfChangeListShow(BoneChangeList, "Bone");
                }
                else
                {
                    var temp = new DataGridViewCell[BoneList.SelectedCells.Count];
                    BoneList.SelectedCells.CopyTo(temp, 0);
                    BoneCount.Clear();
                    for (int i = 0; i < BoneList.SelectedCells.Count; i++)
                    {
                        if (temp[i].ColumnIndex == 0)
                        {
                            BoneCount.Add(Convert.ToInt32(temp[i].Value));
                        }
                    }
                    BoneCount.Sort();
                    List<IPXBone> BoneTemp = new List<IPXBone>();
                    foreach (var t in Sortbone(new List<int>(BoneCount)))
                    {
                        BoneTemp.Add(ThePmxOfNow.Bone[t]);
                    }
                    List<ListChangeInfo> BoneChangeList = new List<ListChangeInfo>();
                    var tempString = InputBoneName.Text;
                    for (int i = 0; i < BoneCount.Count; i++)
                    {
                        ThePmxOfNow.Bone[BoneCount[i]] = BoneTemp[i];
                        if (!AutomaticRadioButton.Checked)
                        {
                            BoneChangeList.Add(new ListChangeInfo(i, ThePmxOfNow.Bone[BoneCount[i]].Name));
                        }
                        else
                        {
                            BoneChangeList.Add(new ListChangeInfo(BoneCount[i], ThePmxOfNow.Bone[BoneCount[i]].Name));
                        }
                    }
                    ThFunOfSaveToPmx(ThePmxOfNow, "Bone");
                    TheFunOfChangeListShow(BoneChangeList, "Bone");
                }
                if (ChangeBoneFamilyWithSort.Checked)
                {
                    IPXPmx TEMPPMX = ARGS.Host.Connector.Pmx.GetCurrentState();
                    for (int i = 0; i < BoneCount.Count; i++)
                    {
                        if (i == 0)
                        {
                            TEMPPMX.Bone[BoneCount[i]].ToBone = TEMPPMX.Bone[BoneCount[i + 1]];
                        }
                        else if (i == BoneCount.Count - 1)
                        {
                            TEMPPMX.Bone[BoneCount[i]].Parent = TEMPPMX.Bone[BoneCount[i - 1]];
                            TEMPPMX.Bone[BoneCount[i]].ToBone = null;
                        }
                        else
                        {
                            TEMPPMX.Bone[BoneCount[i]].Parent = TEMPPMX.Bone[BoneCount[i - 1]];
                            TEMPPMX.Bone[BoneCount[i]].ToBone = TEMPPMX.Bone[BoneCount[i + 1]];
                        }
                    }
                    ThFunOfSaveToPmx(TEMPPMX, "Bone");
                }
            }
            else
            {
                MetroMessageBox.Show(this, "请先选择骨骼后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private List<int> Sortbone(List<int> bonecount)
        {
            IPXPmx ThePmxOfNow = GetPmx;
            switch (LocalSelect.SelectedItem.ToString())
            {
                case "X轴":
                    bonecount.Sort((x, y) => ThePmxOfNow.Bone[x].Position.X.CompareTo(ThePmxOfNow.Bone[y].Position.X));
                    break;

                case "Y轴":
                    bonecount.Sort((x, y) => -ThePmxOfNow.Bone[x].Position.Y.CompareTo(ThePmxOfNow.Bone[y].Position.Y));
                    break;

                case "Z轴":
                    bonecount.Sort((x, y) => -ThePmxOfNow.Bone[x].Position.Z.CompareTo(ThePmxOfNow.Bone[y].Position.Z));
                    break;
            }
            return bonecount;
        }

        private void ChangeJointName_Click(object sender, EventArgs e)
        {
            if (JointCount.Count != 0)
            {
                ThreadPool.QueueUserWorkItem((object state) =>
                {
                    try
                    {
                        var temppmx = ARGS.Host.Connector.Pmx.GetCurrentState();
                        List<ListChangeInfo> JointChangeList = new List<ListChangeInfo>();
                        var tempString = InputJointName.Text;
                        for (var i = 0; i < JointCount.Count; i++)
                        {
                            temppmx.Joint[JointCount[i]].Name = tempString + (i + 1);
                            if (!AutomaticRadioButton.Checked)
                            {
                                JointChangeList.Add(new ListChangeInfo(i, tempString + (i + 1)));
                            }
                            else
                            {
                                JointChangeList.Add(new ListChangeInfo(JointCount[i], tempString + (i + 1)));
                            }
                        }
                        ThFunOfSaveToPmx(temppmx, "Joint");
                        TheFunOfChangeListShow(JointChangeList, "Joint");
                    }
                    catch (Exception) { }
                });
            }
            else
            {
                MetroMessageBox.Show(this, "请先选择Joint后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        #region T窗口权重修改模块
        private String ControlMode = "获取";
        private string BoneSelect = "";
        private string BackText = "";
        private List<string> Boneshow = new List<string>();
        private BindingList<WeightChangeList> WeightUndo = new BindingList<WeightChangeList>();
        private Dictionary<string, string> Vertextable = new Dictionary<string, string>();
        private ToolStripMenuItem MenuItem_UpdateSelect = null;
        private PictureBox SingleBufferedPictureBox = null;
        private Graphics g;

        public class WeightChangeList
        {
            public string Mode;
            public int[] vertex;
            public IPXBone Bone1;
            public int Bone1Index;
            public float Weight1;
            public IPXBone Bone2;
            public float Weight2;
            public int Bone2Index;
            public IPXBone Bone3;
            public int Bone3Index;
            public float Weight3;
            public IPXBone Bone4;
            public float Weight4;
            public int Bone4Index;
            public List<HisWeight> Hisweight = new List<HisWeight>();

            public class HisWeight
            {
                public int vertex;
                public IPXBone Bone1;
                public int Bone1Index;
                public float Weight1;
                public IPXBone Bone2;
                public float Weight2;
                public int Bone2Index;
                public IPXBone Bone3;
                public int Bone3Index;
                public float Weight3;
                public IPXBone Bone4;
                public float Weight4;
                public int Bone4Index;
            }
        }

        private void Startweightchange(object sender, EventArgs e)
        {
            GetPmx = ARGS.Host.Connector.Pmx.GetCurrentState();
            var TransformView = ARGS.Host.Connector.View.TransformView as Form;
            if (StartWeightChangeButton.Text == "开始")
            {
                StartWeightChangeButton.Text = "关闭";
                //TransformView.KeyDown += WeightControl;
                TransformView.KeyPress += WeightOperaByKey;
                if (MenuItem_UpdateSelect == null || SingleBufferedPictureBox == null)
                {
                    foreach (FieldInfo info in TransformView.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
                    {
                        var obj2 = info.GetValue(TransformView);
                        if (obj2 != null)
                        {
                            if (obj2 is ToolStripItem)
                            {
                                switch ((obj2 as ToolStripItem).Name)
                                {
                                    case "MenuItem_UpdateSelect":
                                        MenuItem_UpdateSelect = (ToolStripMenuItem)obj2;
                                        break;
                                }
                            }
                            if (obj2.GetType().Name == "SingleBufferedPictureBox")
                            {
                                SingleBufferedPictureBox = (PictureBox)obj2;
                            }
                        }
                    }
                }
                TransformView.KeyDown += SingleBufferedPictureBoxKey;
                /*  this.WeightNum1.GotFocus += WeightInputGetFocus;
                  this.WeightNum2.GotFocus += WeightInputGetFocus;
                  this.WeightNum3.GotFocus += WeightInputGetFocus;
                  this.WeightNum4.GotFocus += WeightInputGetFocus;*/
                /*  WeightNum1.LostFocus += WeightInputLostFocus;
                  WeightNum2.LostFocus += WeightInputLostFocus;
                  WeightNum3.LostFocus += WeightInputLostFocus;
                  WeightNum4.LostFocus += WeightInputLostFocus;*/
                Vertextable = new Dictionary<string, string>();
                MouseControlWeight.Enabled = true;
                metroLabel35.Enabled = true;
                new Thread((object state) =>
                {
                    while (StartWeightChangeButton.Text == "关闭" && Run)
                    {
                        Thread.Sleep(100);
                        if (savecheck)
                        {
                            TransformView.BeginInvoke(new MethodInvoker(() =>
                            {
                                ThFunOfSaveToPmx(GetPmx, "Vertex");
                            }));
                            savecheck = false;
                        }
                    }
                }).Start();
            }
            else
            {
                StartWeightChangeButton.Text = "开始";
                MouseControlWeight.Checked = false;
                TransformView.KeyPress -= WeightOperaByKey;
                WeightUndo = new BindingList<WeightChangeList>();
                Vertextable = new Dictionary<string, string>();
                MouseControlWeight.Enabled = false;
                metroLabel35.Enabled = false;
                TransformView.KeyDown -= SingleBufferedPictureBoxKey;
                {
                    DataTable table = VertexList.DataSource as DataTable;
                    table.Rows.Clear();
                    table.Columns.Clear();
                    table.Columns.Add("操作");
                    table.Columns.Add("骨骼和权重");
                    table.Rows.Add();
                    table.Rows.Clear();
                }
            }
        }

        private void CantInput(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void SingleBufferedPictureBoxKey(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.C)
            {
                if (WeightUndo.Count == 0) return;
                new Thread((object state) =>
                {
                    try
                    {
                        var ThePmxOfNow = GetPmx;
                        var HisOpera = WeightUndo[WeightUndo.Count - 1];
                        foreach (var item in HisOpera.Hisweight)
                        {
                            ThePmxOfNow.Vertex[item.vertex].Bone1 = item.Bone1;
                            ThePmxOfNow.Vertex[item.vertex].Bone2 = item.Bone2;
                            ThePmxOfNow.Vertex[item.vertex].Bone3 = item.Bone3;
                            ThePmxOfNow.Vertex[item.vertex].Bone4 = item.Bone4;
                            ThePmxOfNow.Vertex[item.vertex].Weight1 = item.Weight1;
                            ThePmxOfNow.Vertex[item.vertex].Weight2 = item.Weight2;
                            ThePmxOfNow.Vertex[item.vertex].Weight3 = item.Weight3;
                            ThePmxOfNow.Vertex[item.vertex].Weight4 = item.Weight4;
                        }
                        savecheck = true;
                        WeightUndo.RemoveAt(WeightUndo.Count - 1);
                    }
                    catch (Exception)
                    {
                    }
                }).Start();

                var table = VertexList.DataSource as DataTable;
                table.Rows.RemoveAt(table.Rows.Count - 1);
            }
        }

        private void WeightUndoChange(object mode)
        {
            try
            {
                IPXPmx ThePmxOfNow = GetPmx;
                DataTable table = VertexList.DataSource as DataTable;
                if (WeightUndo.Count != 0)
                {
                    string temp1 = "";
                    string temp2 = "";
                    string temp3 = "";
                    string temp4 = "";
                    if (!string.IsNullOrWhiteSpace(Bone1List.Text))
                    {
                        var Bone1 = ThePmxOfNow.Bone[int.Parse(Bone1List.Text.Split(':')[0])];
                        temp1 = ThePmxOfNow.Bone.IndexOf(Bone1) + ":" + Bone1.Name + " " + WeightNum1.Text;
                    }
                    if (Bone2_s.Checked && !string.IsNullOrWhiteSpace(Bone2List.Text))
                    {
                        var Bone2 = ThePmxOfNow.Bone[int.Parse(Bone2List.Text.Split(':')[0])];
                        temp2 = " | " + ThePmxOfNow.Bone.IndexOf(Bone2) + ":" + Bone2.Name + " " + WeightNum2.Text;
                    }
                    if (Bone3_s.Checked && !string.IsNullOrWhiteSpace(Bone3List.Text))
                    {
                        var Bone3 = ThePmxOfNow.Bone[int.Parse(Bone3List.Text.Split(':')[0])];
                        temp3 = " | " + ThePmxOfNow.Bone.IndexOf(Bone3) + ":" + Bone3.Name + " " + WeightNum3.Text;
                    }
                    if (Bone4_s.Checked && !string.IsNullOrWhiteSpace(Bone4List.Text))
                    {
                        var Bone4 = ThePmxOfNow.Bone[int.Parse(Bone4List.Text.Split(':')[0])];
                        temp4 = " | " + ThePmxOfNow.Bone.IndexOf(Bone4) + ":" + Bone4.Name + " " + WeightNum4.Text;
                    }
                    if (temp1 + temp2 + temp3 + temp4 != "")
                    {
                        Vertextable.Add((Vertextable.Count) + mode.ToString(), temp1 + temp2 + temp3 + temp4);
                        table.Rows.Add((Vertextable.Count) + mode.ToString(), temp1 + temp2 + temp3 + temp4);
                        VertexList.Update();
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void WeightOperaByKey(object sender, KeyPressEventArgs e)//按键控制权重
        {
            if (e.KeyChar == bootstate.WeightAddKey)
            {
                ControlMode = "加算";
                if (!MouseControlWeight.Checked)
                {
                    MenuItem_UpdateSelect.PerformClick();//T窗口选中顶点转移到主模型窗口
                    var Verttexlist = ARGS.Host.Connector.View.PmxView.GetSelectedVertexIndices();//从主模型窗口获得选中顶点
                    if (Verttexlist.Length != 0) //判断是否获取了顶点
                    {
                        WeightAdd(Verttexlist);
                    }
                }
            }
            else if (e.KeyChar == bootstate.WeightMinusKey)
            {
                ControlMode = "减算";
                if (!MouseControlWeight.Checked)
                {
                    MenuItem_UpdateSelect.PerformClick();//T窗口选中顶点转移到主模型窗口
                    var Verttexlist = ARGS.Host.Connector.View.PmxView.GetSelectedVertexIndices();//从主模型窗口获得选中顶点
                    if (Verttexlist.Length != 0) //判断是否获取了顶点
                    {
                        WeightMinus(Verttexlist);
                    }
                }
            }
            else if (e.KeyChar == bootstate.WeightAppleKey)
            {
                ControlMode = "应用";
                if (!MouseControlWeight.Checked)
                {
                    MenuItem_UpdateSelect.PerformClick();//T窗口选中顶点转移到主模型窗口
                    var Verttexlist = ARGS.Host.Connector.View.PmxView.GetSelectedVertexIndices();//从主模型窗口获得选中顶点
                    if (Verttexlist.Length != 0) //判断是否获取了顶点
                    {
                        WeightApple(Verttexlist, null);
                    }
                }
            }
            else if (e.KeyChar == bootstate.WeightGetKey)
            {
                ControlMode = "获取";
                if (!MouseControlWeight.Checked)
                {
                    MenuItem_UpdateSelect.PerformClick();//T窗口选中顶点转移到主模型窗口
                    var Verttexlist = ARGS.Host.Connector.View.PmxView.GetSelectedVertexIndices();//从主模型窗口获得选中顶点
                    if (Verttexlist.Length != 0) //判断是否获取了顶点
                    {
                        /* Thread thread = new Thread(new ParameterizedThreadStart(GetWeight));
                         thread.Priority = ThreadPriority.Highest;
                         thread.Start(Verttexlist);*/
                        //ThreadPool.QueueUserWorkItem(new WaitCallback(GetWeight), Verttexlist);
                        ChangeVerForm(Verttexlist[0]);
                    }
                }
            }

            if (MouseControlWeight.Checked && StartWeightChangeButton.Text == "关闭")
            {
                SingleBufferedPictureBox.BeginInvoke(new MethodInvoker(() =>
               {
                   SingleBufferedPictureBox.CreateGraphics().Clear(SingleBufferedPictureBox.BackColor); SingleBufferedPictureBox.Invalidate();
               }));
            }
        }

        private void WeightAdd(object verttexlist)
        {
            try
            {
                IPXPmx ThePmxOfNow = GetPmx;
                WeightChangeList HisOpera = new WeightChangeList();
                HisOpera.Mode = "加算";
                if (Bone1_s.Checked && Bone1List.Text != "")
                {
                    HisOpera.Bone1Index = int.Parse(Bone1List.Text.Split(':')[0]);
                    HisOpera.Bone1 = ThePmxOfNow.Bone[HisOpera.Bone1Index];
                    HisOpera.Weight1 = float.Parse(WeightNum1.Text);
                }
                if (Bone2_s.Checked && Bone2List.Text != "")
                {
                    HisOpera.Bone2Index = int.Parse(Bone2List.Text.Split(':')[0]);
                    HisOpera.Bone2 = ThePmxOfNow.Bone[HisOpera.Bone2Index];
                    HisOpera.Weight2 = float.Parse(WeightNum2.Text);
                }
                if (Bone3_s.Checked && Bone3List.Text != "")
                {
                    HisOpera.Bone3Index = int.Parse(Bone3List.Text.Split(':')[0]);
                    HisOpera.Bone3 = ThePmxOfNow.Bone[HisOpera.Bone3Index];
                    HisOpera.Weight3 = float.Parse(WeightNum3.Text);
                }
                if (Bone4_s.Checked && Bone4List.Text != "")
                {
                    HisOpera.Bone4Index = int.Parse(Bone4List.Text.Split(':')[0]);
                    HisOpera.Bone4 = ThePmxOfNow.Bone[HisOpera.Bone4Index];
                    HisOpera.Weight4 = float.Parse(WeightNum4.Text);
                }
                CheckBoneSelect(out MetroTextBox C1, out MetroTextBox C2, out MetroTextBox C3, out MetroTextBox C4);
                float ADDTEMP = float.Parse(C1.Text) + float.Parse(WeightChangeNum.Text);
                float MinusTEMP = float.Parse(C2.Text) - float.Parse(WeightChangeNum.Text);
                if (ADDTEMP + MinusTEMP + (C3 != null ? float.Parse(C3.Text) : 0) + (C4 != null ? float.Parse(C4.Text) : 0) > 100 || MinusTEMP < 0)
                {
                    return;
                }
                C1.Text = ADDTEMP.ToString("0.00").Replace(".00", "");
                C2.Text = MinusTEMP.ToString("0.00").Replace(".00", "");
                WeightApple(verttexlist, HisOpera);
                /*  ThreadPool.QueueUserWorkItem((object state) =>
                  {
                      if (MouseControlWeight.Checked && StartWeightChangeButton.Text == "关闭")
                      {
                          SingleBufferedPictureBox.BeginInvoke(new MethodInvoker(() =>
                          {
                              SingleBufferedPictureBox.CreateGraphics().Clear(SingleBufferedPictureBox.BackColor);
                              SingleBufferedPictureBox.Invalidate();
                          }));
                      }
                  });*/
                #region
                /*if (Bone4_s.Checked)//如果有四根骨骼权重
                {
                    float ADDTEMP = float.Parse(C1.Text) + float.Parse(WeightChangeNum.Text);

                    float MinusTEMP = float.Parse(C2.Text) - float.Parse(WeightChangeNum.Text);

                    if (MinusTEMP < 0)
                    {
                    }
                }

                if (BDEF1Radio.Checked)
                {
                    float Tempnum = HisOpera.Weight1 + float.Parse(WeightChangeNum.Text);
                    if (!Bone3_s.Checked)
                    {
                        if (Bone2_s.Checked)
                        {
                            if (Tempnum < 100)
                            {
                                WeightNum1.Text = Tempnum.ToString("0.00").Replace(".00", "");
                                WeightNum2.Text = (100 - Tempnum).ToString("0.00").Replace(".00", "");
                                WeightApple(verttexlist, HisOpera);
                                return;
                            }
                            else
                            {
                                WeightNum1.Text = "100";
                                WeightNum2.Text = "0";
                                WeightApple(verttexlist, HisOpera);
                                return;
                            }
                        }
                        else
                        {
                            if (Tempnum < 100)
                            {
                                WeightNum1.Text = Tempnum.ToString("0.00").Replace(".00", "");
                                WeightApple(verttexlist, HisOpera);
                                return;
                            }
                            else
                            {
                                WeightNum1.Text = "100";
                                WeightApple(verttexlist, HisOpera);
                                return;
                            }
                        }
                    }
                    else
                    {
                        if (Tempnum < 100)
                        {
                            WeightNum1.Text = Tempnum.ToString("0.00").Replace(".00", "");
                        }
                        else
                        {
                            return;
                        }
                    }
                }
                else if (BDEF2Radio.Checked)
                {
                    float Tempnum = HisOpera.Weight2 + float.Parse(WeightChangeNum.Text);
                    if (!Bone3_s.Checked)
                    {
                        if (Tempnum < 100)
                        {
                            WeightNum2.Text = Tempnum.ToString("0.00").Replace(".00", "");
                            WeightNum1.Text = (100 - Tempnum).ToString("0.00").Replace(".00", "");
                            WeightApple(verttexlist, HisOpera);
                            return;
                        }
                        else
                        {
                            WeightNum1.Text = "100";
                            WeightNum2.Text = "0";
                            WeightApple(verttexlist, HisOpera);
                            return;
                        }
                    }
                    else
                    {
                        WeightNum2.Text = Tempnum.ToString("0.00").Replace(".00", "");
                    }
                }
                else if (BDEF3Radio.Checked)
                {
                    WeightNum3.Text = (HisOpera.Weight3 + float.Parse(WeightChangeNum.Text)) < 100 ? (HisOpera.Weight3 + float.Parse(WeightChangeNum.Text)).ToString("0.00")
                        .Replace(".00", "") : WeightNum3.Text;
                }
                else if (BDEF4Radio.Checked)
                {
                    WeightNum4.Text = (HisOpera.Weight4 + float.Parse(WeightChangeNum.Text)) < 100 ? (HisOpera.Weight4 + float.Parse(WeightChangeNum.Text)).ToString("0.00")
                        .Replace(".00", "") : WeightNum4.Text;
                }
                if (Bone3_s.Checked)
                {
                    if (BDEF1Radio_2.Checked)
                    {
                        if ((HisOpera.Weight1 - float.Parse(WeightChangeNum.Text)) > 0)
                        {
                            WeightNum1.Text = (HisOpera.Weight1 - float.Parse(WeightChangeNum.Text)).ToString("0.00").Replace(".00", "");
                            WeightApple(verttexlist, HisOpera);
                        }
                        else
                        {
                            DataBack(HisOpera);
                        }
                    }
                    else if (BDEF2Radio_2.Checked)
                    {
                        if ((HisOpera.Weight2 - float.Parse(WeightChangeNum.Text)) > 0)
                        {
                            WeightNum2.Text = (HisOpera.Weight2 - float.Parse(WeightChangeNum.Text)).ToString("0.00").Replace(".00", "");
                            WeightApple(verttexlist, HisOpera);
                        }
                        else
                        {
                            DataBack(HisOpera);
                        }
                    }
                    else if (BDEF3Radio_2.Checked)
                    {
                        if ((HisOpera.Weight3 - float.Parse(WeightChangeNum.Text)) > 0)
                        {
                            WeightNum3.Text = (HisOpera.Weight3 - float.Parse(WeightChangeNum.Text)).ToString("0.00").Replace(".00", "");
                            WeightApple(verttexlist, HisOpera);
                        }
                        else
                        {
                            DataBack(HisOpera);
                        }
                    }
                    else if (BDEF4Radio_2.Checked)
                    {
                        if ((HisOpera.Weight4 - float.Parse(WeightChangeNum.Text)) > 0)
                        {
                            WeightNum4.Text = (HisOpera.Weight4 - float.Parse(WeightChangeNum.Text)).ToString("0.00").Replace(".00", "");
                            WeightApple(verttexlist, HisOpera);
                        }
                        else
                        {
                            DataBack(HisOpera);
                        }
                    }
                }*/
                #endregion
            }
            catch (Exception)
            {
            }
        }

        private void CheckBoneSelect(out MetroTextBox c1, out MetroTextBox c2, out MetroTextBox c3, out MetroTextBox c4)
        {
            c1 = null;
            c2 = null;
            c3 = null;
            c4 = null;
            if (!Bone3_s.Checked)
            {
                if (BDEF1Radio.Checked)
                {
                    c1 = WeightNum1;
                    c2 = WeightNum2;
                }
                else
                {
                    c1 = WeightNum2;
                    c2 = WeightNum1;
                }
            }
            else
            {
                if (BDEF1Radio.Checked)
                {
                    c1 = WeightNum1;
                    if (BDEF2Radio_2.Checked)
                    {
                        c2 = WeightNum2;
                        if (float.Parse(WeightNum3.Text) >= float.Parse(WeightNum4.Text))
                        {
                            c3 = WeightNum3;
                        }
                        else
                        {
                            c4 = WeightNum4;
                        }
                    }
                    else if (BDEF3Radio_2.Checked)
                    {
                        c2 = WeightNum3;
                        if (float.Parse(WeightNum2.Text) >= float.Parse(WeightNum4.Text))
                        {
                            c3 = WeightNum2;
                        }
                        else
                        {
                            c4 = WeightNum4;
                        }
                    }
                    else
                    {
                        c2 = WeightNum4;
                        if (float.Parse(WeightNum2.Text) >= float.Parse(WeightNum3.Text))
                        {
                            c3 = WeightNum2;
                        }
                        else
                        {
                            c4 = WeightNum3;
                        }
                    }
                }
                else if (BDEF2Radio.Checked)
                {
                    c1 = WeightNum2;
                    if (BDEF1Radio_2.Checked)
                    {
                        c2 = WeightNum1;
                        if (float.Parse(WeightNum3.Text) >= float.Parse(WeightNum4.Text))
                        {
                            c3 = WeightNum3;
                        }
                        else
                        {
                            c4 = WeightNum4;
                        }
                    }
                    else if (BDEF3Radio_2.Checked)
                    {
                        c2 = WeightNum3;
                        if (float.Parse(WeightNum1.Text) >= float.Parse(WeightNum4.Text))
                        {
                            c3 = WeightNum1;
                        }
                        else
                        {
                            c4 = WeightNum4;
                        }
                    }
                    else
                    {
                        c2 = WeightNum4;
                        if (float.Parse(WeightNum1.Text) >= float.Parse(WeightNum3.Text))
                        {
                            c3 = WeightNum1;
                        }
                        else
                        {
                            c4 = WeightNum3;
                        }
                    }
                }
                else if (BDEF3Radio.Checked)
                {
                    c1 = WeightNum3;
                    if (BDEF2Radio_2.Checked)
                    {
                        c2 = WeightNum2;
                        if (float.Parse(WeightNum1.Text) >= float.Parse(WeightNum4.Text))
                        {
                            c3 = WeightNum1;
                        }
                        else
                        {
                            c4 = WeightNum4;
                        }
                    }
                    else if (BDEF1Radio_2.Checked)
                    {
                        c2 = WeightNum1;
                        if (float.Parse(WeightNum2.Text) >= float.Parse(WeightNum4.Text))
                        {
                            c3 = WeightNum2;
                        }
                        else
                        {
                            c4 = WeightNum4;
                        }
                    }
                    else
                    {
                        c2 = WeightNum4;
                        if (float.Parse(WeightNum1.Text) >= float.Parse(WeightNum2.Text))
                        {
                            c3 = WeightNum1;
                        }
                        else
                        {
                            c4 = WeightNum2;
                        }
                    }
                }
                else if (BDEF4Radio.Checked)
                {
                    c1 = WeightNum4;
                    if (BDEF2Radio_2.Checked)
                    {
                        c2 = WeightNum2;
                        if (float.Parse(WeightNum1.Text) >= float.Parse(WeightNum3.Text))
                        {
                            c3 = WeightNum1;
                        }
                        else
                        {
                            c4 = WeightNum3;
                        }
                    }
                    else if (BDEF3Radio_2.Checked)
                    {
                        c2 = WeightNum3;
                        if (float.Parse(WeightNum1.Text) >= float.Parse(WeightNum2.Text))
                        {
                            c3 = WeightNum1;
                        }
                        else
                        {
                            c4 = WeightNum2;
                        }
                    }
                    else
                    {
                        c2 = WeightNum1;
                        if (float.Parse(WeightNum2.Text) >= float.Parse(WeightNum3.Text))
                        {
                            c3 = WeightNum2;
                        }
                        else
                        {
                            c4 = WeightNum3;
                        }
                    }
                }
            }
        }

        private void WeightMinus(object verttexlist)
        {
            try
            {
                IPXPmx ThePmxOfNow = GetPmx;
                WeightChangeList HisOpera = new WeightChangeList();

                HisOpera.Mode = "减算";
                if (Bone1_s.Checked && Bone1List.Text != "")
                {
                    HisOpera.Bone1Index = int.Parse(Bone1List.Text.Split(':')[0]);
                    HisOpera.Bone1 = ThePmxOfNow.Bone[HisOpera.Bone1Index];
                    HisOpera.Weight1 = float.Parse(WeightNum1.Text);
                }
                if (Bone2_s.Checked && Bone2List.Text != "")
                {
                    HisOpera.Bone2Index = int.Parse(Bone2List.Text.Split(':')[0]);
                    HisOpera.Bone2 = ThePmxOfNow.Bone[HisOpera.Bone2Index];
                    HisOpera.Weight2 = float.Parse(WeightNum2.Text);
                }
                if (Bone3_s.Checked && Bone3List.Text != "")
                {
                    HisOpera.Bone3Index = int.Parse(Bone3List.Text.Split(':')[0]);
                    HisOpera.Bone3 = ThePmxOfNow.Bone[HisOpera.Bone3Index];
                    HisOpera.Weight3 = float.Parse(WeightNum3.Text);
                }
                if (Bone4_s.Checked && Bone4List.Text != "")
                {
                    HisOpera.Bone4Index = int.Parse(Bone4List.Text.Split(':')[0]);
                    HisOpera.Bone4 = ThePmxOfNow.Bone[HisOpera.Bone4Index];
                    HisOpera.Weight4 = float.Parse(WeightNum4.Text);
                }
                CheckBoneSelect(out MetroTextBox C1, out MetroTextBox C2, out MetroTextBox C3, out MetroTextBox C4);
                float MinusTEMP = float.Parse(C1.Text) - float.Parse(WeightChangeNum.Text);
                float ADDTEMP = float.Parse(C2.Text) + float.Parse(WeightChangeNum.Text);
                if (ADDTEMP + MinusTEMP + (C3 != null ? float.Parse(C3.Text) : 0) + (C4 != null ? float.Parse(C4.Text) : 0) > 100 || MinusTEMP < 0)
                {
                    return;
                }
                C1.Text = MinusTEMP.ToString("0.00").Replace(".00", "");
                C2.Text = ADDTEMP.ToString("0.00").Replace(".00", "");
                /*ThreadPool.QueueUserWorkItem((object state) =>
                {
                    if (MouseControlWeight.Checked && StartWeightChangeButton.Text == "关闭")
                    {
                        SingleBufferedPictureBox.BeginInvoke(new MethodInvoker(() =>
                        {
                            SingleBufferedPictureBox.CreateGraphics().Clear(SingleBufferedPictureBox.BackColor);
                            SingleBufferedPictureBox.Invalidate();
                        }));
                    }
                });*/
                WeightApple(verttexlist, HisOpera);
                #region
                /*  if (BDEF1Radio.Checked)
                  {
                      float Tempnum = HisOpera.Weight1 - float.Parse(WeightChangeNum.Text);
                      if (!Bone3_s.Checked)
                      {
                          if (Bone2_s.Checked)
                          {
                              if (Tempnum > 0)
                              {
                                  WeightNum1.Text = Tempnum.ToString("0.00").Replace(".00", "");
                                  WeightNum2.Text = (100 - Tempnum).ToString("0.00").Replace(".00", "");
                                  WeightUndo.Add(HisOpera);
                                  WeightApple(verttexlist,HisOpera);
                                  return;
                              }
                              else
                              {
                                  WeightNum1.Text = "0";
                                  WeightNum2.Text = "100";
                                  WeightUndo.Add(HisOpera);
                                  WeightApple(verttexlist,HisOpera);
                                  return;
                              }
                          }
                          else
                          {
                              if (Tempnum > 0)
                              {
                                  WeightNum1.Text = Tempnum.ToString("0.00").Replace(".00", "");
                                  WeightUndo.Add(HisOpera);
                                  WeightApple(verttexlist,HisOpera);
                                  return;
                              }
                              else
                              {
                                  WeightNum1.Text = "0";
                                  WeightUndo.Add(HisOpera);
                                  WeightApple(verttexlist,HisOpera);
                                  return;
                              }
                          }
                      }
                      else
                      {
                          if (Tempnum > 0)
                          {
                              WeightNum1.Text = Tempnum.ToString("0.00").Replace(".00", "");
                          }
                          else
                          {
                              return;
                          }
                      }
                  }
                  else if (BDEF2Radio.Checked)
                  {
                      float Tempnum = HisOpera.Weight2 - float.Parse(WeightChangeNum.Text);
                      if (!Bone3_s.Checked)
                      {
                          if (Tempnum > 0)
                          {
                              WeightNum2.Text = Tempnum.ToString("0.00").Replace(".00", "");
                              WeightNum1.Text = (100 - Tempnum).ToString("0.00").Replace(".00", "");
                              WeightUndo.Add(HisOpera);
                              WeightApple(verttexlist,HisOpera);
                              return;
                          }
                          else
                          {
                              WeightNum1.Text = "0";
                              WeightNum2.Text = "100";
                              WeightUndo.Add(HisOpera);
                              WeightApple(verttexlist,HisOpera);
                              return;
                          }
                      }
                      else
                      {
                          WeightNum2.Text = Tempnum.ToString("0.00").Replace(".00", "");
                      }
                  }
                  else if (BDEF3Radio.Checked)
                  {
                      WeightNum3.Text = (HisOpera.Weight3 - float.Parse(WeightChangeNum.Text)) > 0 ? (HisOpera.Weight3 - float.Parse(WeightChangeNum.Text)).ToString("0.00")
                          .Replace(".00", "") : WeightNum3.Text;
                  }
                  else if (BDEF4Radio.Checked)
                  {
                      WeightNum4.Text = (HisOpera.Weight4 + float.Parse(WeightChangeNum.Text)) > 0 ? (HisOpera.Weight4 - float.Parse(WeightChangeNum.Text)).ToString("0.00")
                          .Replace(".00", "") : WeightNum4.Text;
                  }
                  if (Bone3_s.Checked)
                  {
                      if (BDEF1Radio_2.Checked)
                      {
                          if ((HisOpera.Weight1 + float.Parse(WeightChangeNum.Text)) < 100)
                          {
                              WeightNum1.Text = (HisOpera.Weight1 + float.Parse(WeightChangeNum.Text)).ToString("0.00").Replace(".00", "");
                              WeightUndo.Add(HisOpera);
                              WeightApple(verttexlist,HisOpera);
                          }
                          else
                          {
                              DataBack(HisOpera);
                          }
                      }
                      else if (BDEF2Radio_2.Checked)
                      {
                          if ((HisOpera.Weight2 + float.Parse(WeightChangeNum.Text)) <100)
                          {
                              WeightNum2.Text = (HisOpera.Weight2 + float.Parse(WeightChangeNum.Text)).ToString("0.00").Replace(".00", "");
                              WeightUndo.Add(HisOpera);
                              WeightApple(verttexlist,HisOpera);
                          }
                          else
                          {
                              DataBack(HisOpera);
                          }
                      }
                      else if (BDEF3Radio_2.Checked)
                      {
                          if ((HisOpera.Weight3 + float.Parse(WeightChangeNum.Text)) <100)
                          {
                              WeightNum3.Text = (HisOpera.Weight3 + float.Parse(WeightChangeNum.Text)).ToString("0.00").Replace(".00", "");
                              WeightUndo.Add(HisOpera);
                              WeightApple(verttexlist,HisOpera);
                          }
                          else
                          {
                              DataBack(HisOpera);
                          }
                      }
                      else if (BDEF4Radio_2.Checked)
                      {
                          if ((HisOpera.Weight4 + float.Parse(WeightChangeNum.Text)) <100)
                          {
                              WeightNum4.Text = (HisOpera.Weight4 + float.Parse(WeightChangeNum.Text)).ToString("0.00").Replace(".00", "");
                              WeightUndo.Add(HisOpera);
                              WeightApple(verttexlist,HisOpera);
                          }
                          else
                          {
                              DataBack(HisOpera);
                          }
                      }
                  }*/
                #endregion
            }
            catch (Exception)
            {
            }
        }

        private void DataBack(WeightChangeList hisOpera)
        {
            if (BDEF1Radio.Checked)
            {
                WeightNum1.Text = hisOpera.Weight1.ToString("0.00").Replace(".00", "");
            }
            else if (BDEF2Radio.Checked)
            {
                WeightNum2.Text = hisOpera.Weight2.ToString("0.00").Replace(".00", "");
            }
            else if (BDEF3Radio.Checked)
            {
                WeightNum3.Text = hisOpera.Weight3.ToString("0.00").Replace(".00", "");
            }
            else if (BDEF4Radio.Checked)
            {
                WeightNum4.Text = hisOpera.Weight4.ToString("0.00").Replace(".00", "");
            }
        }

        private void WeightApple(object verttexlist, WeightChangeList HisOpera)
        {
            try
            {
                IPXPmx ThePmxOfNow = GetPmx;
                if (HisOpera == null)
                {
                    HisOpera = new WeightChangeList();
                    HisOpera.Mode = "应用";
                    if (Bone1_s.Checked && Bone1List.Text != "")
                    {
                        HisOpera.Bone1Index = int.Parse(Bone1List.Text.Split(':')[0]);
                        HisOpera.Bone1 = ThePmxOfNow.Bone[HisOpera.Bone1Index];
                        HisOpera.Weight1 = float.Parse(WeightNum1.Text);
                    }
                    if (Bone2_s.Checked && Bone2List.Text != "")
                    {
                        HisOpera.Bone2Index = int.Parse(Bone2List.Text.Split(':')[0]);
                        HisOpera.Bone2 = ThePmxOfNow.Bone[HisOpera.Bone2Index];
                        HisOpera.Weight2 = float.Parse(WeightNum2.Text);
                    }
                    if (Bone3_s.Checked && Bone3List.Text != "")
                    {
                        HisOpera.Bone3Index = int.Parse(Bone3List.Text.Split(':')[0]);
                        HisOpera.Bone3 = ThePmxOfNow.Bone[HisOpera.Bone3Index];
                        HisOpera.Weight3 = float.Parse(WeightNum3.Text);
                    }
                    if (Bone4_s.Checked && Bone4List.Text != "")
                    {
                        HisOpera.Bone4Index = int.Parse(Bone4List.Text.Split(':')[0]);
                        HisOpera.Bone4 = ThePmxOfNow.Bone[HisOpera.Bone4Index];
                        HisOpera.Weight4 = float.Parse(WeightNum4.Text);
                    }
                }
                HisOpera.Hisweight = new List<WeightChangeList.HisWeight>();
                foreach (var item in verttexlist as int[])
                {
                    var temp = new WeightChangeList.HisWeight();
                    temp.vertex = item;
                    temp.Bone1 = ThePmxOfNow.Vertex[item].Bone1;
                    temp.Bone2 = ThePmxOfNow.Vertex[item].Bone2;
                    temp.Bone3 = ThePmxOfNow.Vertex[item].Bone3;
                    temp.Bone4 = ThePmxOfNow.Vertex[item].Bone4;
                    temp.Weight1 = ThePmxOfNow.Vertex[item].Weight1;
                    temp.Weight2 = ThePmxOfNow.Vertex[item].Weight2;
                    temp.Weight3 = ThePmxOfNow.Vertex[item].Weight3;
                    temp.Weight4 = ThePmxOfNow.Vertex[item].Weight4;
                    ThePmxOfNow.Vertex[item].Bone1 = null;
                    ThePmxOfNow.Vertex[item].Bone2 = null;
                    ThePmxOfNow.Vertex[item].Bone3 = null;
                    ThePmxOfNow.Vertex[item].Bone4 = null;
                    ThePmxOfNow.Vertex[item].Weight1 = 0;
                    ThePmxOfNow.Vertex[item].Weight2 = 0;
                    ThePmxOfNow.Vertex[item].Weight3 = 0;
                    ThePmxOfNow.Vertex[item].Weight4 = 0;
                    ThePmxOfNow.Vertex[item].Bone1 = HisOpera.Bone1 == null ? null : HisOpera.Bone1;
                    ThePmxOfNow.Vertex[item].Weight1 = HisOpera.Bone1 == null ? 0 : float.Parse(WeightNum1.Text) / 100;
                    ThePmxOfNow.Vertex[item].Bone2 = HisOpera.Bone2 == null ? null : HisOpera.Bone2;
                    ThePmxOfNow.Vertex[item].Weight2 = HisOpera.Bone2 == null ? 0 : float.Parse(WeightNum2.Text) / 100;
                    ThePmxOfNow.Vertex[item].Bone3 = HisOpera.Bone3 == null ? null : HisOpera.Bone3;
                    ThePmxOfNow.Vertex[item].Weight3 = HisOpera.Bone3 == null ? 0 : float.Parse(WeightNum3.Text) / 100;
                    ThePmxOfNow.Vertex[item].Bone4 = HisOpera.Bone4 == null ? null : HisOpera.Bone4;
                    ThePmxOfNow.Vertex[item].Weight4 = HisOpera.Bone4 == null ? 0 : float.Parse(WeightNum4.Text) / 100;
                    HisOpera.Hisweight.Add(temp);
                }
                HisOpera.vertex = verttexlist as int[];
                WeightUndo.Add(HisOpera);
                WeightUndoChange(HisOpera.Mode);//历史修改显示到前端
                                                //WeightUndoChange(HisOpera.Mode);
                                                //ThFunOfSaveToPmx(ThePmxOfNow, "Vertex");
                savecheck = true;
            }
            catch (Exception) { };
        }

        private void ChangeVerForm(int vernum)//根据顶点设置插件窗口的权重显示
        {
            try
            {
                IPXPmx ThePmxOfNow = GetPmx;
                if (ThePmxOfNow.Bone.Count != Boneshow.Count)
                {
                    Boneshow = new List<string>();
                    var count = 0;
                    foreach (var item in ThePmxOfNow.Bone)
                    {
                        Boneshow.Add(count + ":" + item.Name);
                        count++;
                    }
                }
                VertexCount.Text = vernum.ToString();
                {
                    var Bone4 = ThePmxOfNow.Vertex[vernum].Bone4;
                    var Bone4Index = ThePmxOfNow.Bone.IndexOf(Bone4);
                    WeightNum4.Text = 0.ToString("0.00").Replace(".00", "");
                    if (Bone4 != null)
                    {
                        if (Bone4Index == 0 && ThePmxOfNow.Vertex[vernum].Weight4 == 0)//用来判断 要显示的顶点是否是0号骨骼并且权重值为0
                        {
                            Bone4_s.Checked = false;
                            Bone4List.DataSource = new List<string>();
                        }
                        else
                        {
                            Bone4_s.Checked = true;
                            Bone4List.DataSource = new List<string>(Boneshow);
                            Bone4List.SelectedIndex = Boneshow.IndexOf(Bone4Index.ToString() + ":" + Bone4.Name) != -1 ? Boneshow.IndexOf(Bone4Index.ToString() + ":" + Bone4.Name) : 0;
                            WeightNum4.Text = (ThePmxOfNow.Vertex[vernum].Weight4 * 100).ToString("0.00").Replace(".00", "");
                        }
                    }
                    else
                    {
                        Bone4_s.Checked = false;
                        Bone4List.DataSource = new List<string>();
                    }
                }

                {
                    var Bone3 = ThePmxOfNow.Vertex[vernum].Bone3;
                    var Bone3Index = ThePmxOfNow.Bone.IndexOf(Bone3);
                    WeightNum3.Text = 0.ToString("0.00").Replace(".00", "");
                    if (Bone3 != null)
                    {
                        if (Bone3Index == 0 && ThePmxOfNow.Vertex[vernum].Weight3 == 0)//用来判断 要显示的顶点是否是0号骨骼并且权重值为0
                        {
                            Bone3_s.Checked = false;
                            Bone3List.DataSource = new List<string>();
                        }
                        else
                        {
                            Bone3_s.Checked = true;
                            Bone3List.DataSource = new List<string>(Boneshow);
                            Bone3List.SelectedIndex = Boneshow.IndexOf(Bone3Index.ToString() + ":" + Bone3.Name) != -1 ? Boneshow.IndexOf(Bone3Index.ToString() + ":" + Bone3.Name) : 0;
                            WeightNum3.Text = (ThePmxOfNow.Vertex[vernum].Weight3 * 100).ToString("0.00").Replace(".00", "");
                        }
                    }
                    else
                    {
                        Bone3_s.Checked = false;
                        Bone3List.DataSource = new List<string>();
                    }
                }
                {
                    var Bone2 = ThePmxOfNow.Vertex[vernum].Bone2;
                    var Bone2Index = ThePmxOfNow.Bone.IndexOf(Bone2);
                    WeightNum2.Text = 0.ToString("0.00").Replace(".00", "");
                    if (Bone2 != null)
                    {
                        if (Bone2Index == 0 && ThePmxOfNow.Vertex[vernum].Weight2 == 0)//用来判断 要显示的顶点是否是0号骨骼并且权重值为0
                        {
                            Bone2_s.Checked = false;
                            Bone2List.DataSource = new List<string>();
                        }
                        else
                        {
                            Bone2_s.Checked = true;
                            Bone2List.DataSource = new List<string>(Boneshow);
                            Bone2List.SelectedIndex = Boneshow.IndexOf(Bone2Index.ToString() + ":" + Bone2.Name) != -1 ? Boneshow.IndexOf(Bone2Index.ToString() + ":" + Bone2.Name) : 0;
                            WeightNum2.Text = (ThePmxOfNow.Vertex[vernum].Weight2 * 100).ToString("0.00").Replace(".00", "");
                        }
                    }
                    else
                    {
                        Bone2_s.Checked = false;
                        Bone2List.DataSource = new List<string>();
                    }
                }
                {
                    var Bone1 = ThePmxOfNow.Vertex[vernum].Bone1;
                    var Bone1Index = ThePmxOfNow.Bone.IndexOf(Bone1);
                    WeightNum1.Text = 0.ToString("0.00").Replace(".00", "");
                    if (Bone1 != null)
                    {
                        Bone1_s.Checked = true;
                        Bone1List.DataSource = new List<string>(Boneshow);
                        Bone1List.SelectedIndex = Boneshow.IndexOf(ThePmxOfNow.Bone.IndexOf(Bone1).ToString() + ":" + Bone1.Name) != -1 ? Boneshow.IndexOf(ThePmxOfNow.Bone.IndexOf(Bone1).ToString() + ":" + Bone1.Name) : 0;
                        WeightNum1.Text = (ThePmxOfNow.Vertex[vernum].Weight1 * 100).ToString("0.00").Replace(".00", "");
                    }
                    else
                    {
                        Bone1_s.Checked = false;
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void BoneSelectChange(object sender, EventArgs e)
        {
            var tempcontrol = sender as MetroCheckBox;
            switch (tempcontrol.Name)
            {
                case "Bone1_s":
                    if (tempcontrol.Checked)
                    {
                        Bone1List.Enabled = true;
                        Weight1Label.Enabled = true;
                        WeightNum1.Enabled = true;
                        BDEF1Radio.Enabled = true;
                        Bone2_s.Checked = false;
                        Bone3_s.Checked = false;
                        Bone4_s.Checked = false;
                    }
                    else
                    {
                        Bone1List.Text = "";
                        WeightNum1.Text = "0";
                        /* Bone1List.Enabled = false;
                         Weight1Label.Enabled = false;
                         WeightNum1.Enabled = false;
                         BDEF1Radio.Enabled = false;
                         BDEF1Radio.Checked = false;*/
                        tempcontrol.Checked = true;
                    }
                    break;

                case "Bone2_s":
                    if (tempcontrol.Checked)
                    {
                        Bone2List.Enabled = true;
                        Weight2Label.Enabled = true;
                        WeightNum2.Enabled = true;
                        BDEF2Radio.Visible = true;
                    }
                    else
                    {
                        if (Bone4_s.Checked == true || Bone3_s.Checked == true)
                        {
                            Bone3_s.Checked = false;
                            Bone4_s.Checked = false;
                        }
                        Bone2List.Text = "";
                        WeightNum2.Text = "0";
                        Bone2List.Enabled = false;
                        Weight2Label.Enabled = false;
                        WeightNum2.Enabled = false;
                        BDEF2Radio.Visible = false;
                        BDEF1Radio.Checked = true;
                        Bone2List.DataSource = null;
                        WeightNum2.Text = "0";
                    }
                    break;

                case "Bone3_s":
                    if (tempcontrol.Checked)
                    {
                        if (Bone2_s.Checked == false)
                        {
                            Bone2_s.Checked = true;
                            BDEF1Radio.Checked = true;
                        }
                        Bone3List.Enabled = true;
                        Weight3Label.Enabled = true;
                        WeightNum3.Enabled = true;
                        BDEF3Radio.Visible = true;

                        BDEF1Radio_2.Visible = true;
                        BDEF2Radio_2.Visible = true;
                        BDEF3Radio_2.Visible = true;
                    }
                    else
                    {
                        Bone4_s.Checked = false;
                        Bone3List.Text = "";
                        WeightNum3.Text = "0";
                        Bone3List.Enabled = false;
                        Weight3Label.Enabled = false;
                        WeightNum3.Enabled = false;
                        BDEF3Radio.Visible = false;
                        if (BDEF3Radio.Checked) BDEF2Radio.Checked = true;

                        BDEF1Radio_2.Visible = false;
                        BDEF2Radio_2.Visible = false;
                        BDEF3Radio_2.Visible = false;
                        BDEF4Radio_2.Visible = false;
                        Bone3List.DataSource = null;
                        WeightNum3.Text = "0";
                    }
                    break;

                case "Bone4_s":
                    if (tempcontrol.Checked)
                    {
                        if (Bone2_s.Checked == false || Bone3_s.Checked == false)
                        {
                            Bone2_s.Checked = true;
                            Bone3_s.Checked = true;
                            BDEF1Radio.Checked = true;
                        }
                        Bone4List.Enabled = true;
                        Weight4Label.Enabled = true;
                        WeightNum4.Enabled = true;
                        BDEF4Radio.Visible = true;

                        BDEF1Radio_2.Visible = true;
                        BDEF2Radio_2.Visible = true;
                        BDEF3Radio_2.Visible = true;
                        BDEF4Radio_2.Visible = true;
                    }
                    else
                    {
                        Bone4List.Text = "";
                        WeightNum4.Text = "0";
                        Bone4List.Enabled = false;
                        Weight4Label.Enabled = false;
                        WeightNum4.Enabled = false;
                        BDEF4Radio.Visible = false;
                        if (BDEF4Radio.Checked) BDEF3Radio.Checked = true;
                        BDEF4Radio_2.Visible = false;
                        Bone4List.DataSource = null;
                        WeightNum4.Text = "0";
                    }
                    break;
            }
            if (BDEF1Radio.Checked && BDEF1Radio_2.Checked) BDEF2Radio_2.Checked = true;
            else if (BDEF2Radio.Checked && BDEF2Radio_2.Checked) BDEF1Radio_2.Checked = true;
            else if (BDEF3Radio.Checked && BDEF3Radio_2.Checked) BDEF2Radio_2.Checked = true;
            else if (BDEF4Radio.Checked && BDEF4Radio_2.Checked) BDEF3Radio_2.Checked = true;
        }

        private void SelectChange(object sender, EventArgs e)
        {
            var SelectTemp = sender as RadioButton;
            switch (SelectTemp.Name)
            {
                case "BDEF1Radio":
                    {
                        if (Bone1List.Text != "" && SelectTemp.Checked)
                        {
                            BoneSelect = Bone1List.Text;
                        }
                    }
                    break;

                case "BDEF2Radio":
                    {
                        if (Bone2List.Text != "" && SelectTemp.Checked)
                        {
                            BoneSelect = Bone1List.Text;
                        }
                    }
                    break;

                case "BDEF3Radio":
                    {
                        if (Bone3List.Text != "" && SelectTemp.Checked)
                        {
                            BoneSelect = Bone1List.Text;
                        }
                    }
                    break;

                case "BDEF4Radio":
                    {
                        if (Bone4List.Text != "" && SelectTemp.Checked)
                        {
                            BoneSelect = Bone1List.Text;
                        }
                    }
                    break;
            }
            RadioAvoidSelect();
        }

        private void RadioAvoidSelect()
        {
            BDEF1Radio_2.Enabled = true;
            BDEF2Radio_2.Enabled = true;
            BDEF3Radio_2.Enabled = true;
            BDEF4Radio_2.Enabled = true;
            if (BDEF1Radio.Checked)
            {
                if (BDEF1Radio_2.Checked) BDEF2Radio_2.Checked = true;
                BDEF1Radio_2.Enabled = false;
            }
            else if (BDEF2Radio.Checked)
            {
                if (BDEF2Radio_2.Checked) BDEF3Radio_2.Checked = true;
                BDEF2Radio_2.Enabled = false;
            }
            else if (BDEF3Radio.Checked)
            {
                if (BDEF3Radio_2.Checked) BDEF1Radio_2.Checked = true;
                BDEF3Radio_2.Enabled = false;
            }
            else if (BDEF4Radio.Checked)
            {
                if (BDEF4Radio_2.Checked) BDEF1Radio_2.Checked = true;
                BDEF4Radio_2.Enabled = false;
            }
        }

        private void MoreThanZeroNumCheck4bonechang(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                NullButton.Focus(); //按Enter键后，让输入框失去焦点，激活失去焦点事件
            }
            else if (e.KeyChar == '\u001b') //按Esc键后，让输入框返回备份数值
            {
                var TempControl = sender as TextBox;
                TempControl.Text = BackText;
            }
            else if (e.KeyChar != '\b' && e.KeyChar != '.' && e.KeyChar != '\u0016' && e.KeyChar != '\u0003') //这是允许输入退格键
            {
                if ((e.KeyChar < '0') || (e.KeyChar > '9')) //这是允许输入0-9数字
                {
                    e.Handled = true;
                }
            }
        }

        private void MoreThanZeroNumCheck4WeightNum(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                NullButton.Focus(); //按Enter键后，让输入框失去焦点，激活失去焦点事件
            }
            else if (e.KeyChar == '\u001b') //按Esc键后，让输入框返回备份数值
            {
                var TempControl = sender as MetroTextBox;
                TempControl.Text = BackText;
            }
            else if (e.KeyChar != '\b' && e.KeyChar != '.' && e.KeyChar != '\u0016' && e.KeyChar != '\u0003') //这是允许输入退格键
            {
                if ((e.KeyChar < '0') || (e.KeyChar > '9')) //这是允许输入0-9数字
                {
                    e.Handled = true;
                }
            }
        }

        private void WeightTextChange(object sender, EventArgs e)
        {
            var TempControl = sender as MetroTextBox;
            try
            {
                if (TempControl.Text == "")
                {
                    return;
                }
                if (float.Parse(TempControl.Text) >= 100)
                {
                    TempControl.Text = 100.ToString();
                    TempControl.Select(0, 3);
                }
                else if (float.Parse(TempControl.Text) <= 0)
                {
                    TempControl.Text = 0.ToString();
                }
            }
            catch (Exception)
            {
                TempControl.Text = 0.ToString();
            }
        }

        private void ShowBone(object sender, EventArgs e)
        {
            IPXPmx ThePmxOfNow = GetPmx;
            if (ThePmxOfNow == null)
            {
                ThePmxOfNow = ARGS.Host.Connector.Pmx.GetCurrentState();
            }
            var count = 0;
            var Control = sender as MetroComboBox;
            var show = new List<string>();
            foreach (var item in ThePmxOfNow.Bone)
            {
                show.Add(count + ":" + item.Name);
                count++;
            }
            var TT = Control.Text;
            Control.DataSource = show;
            Control.SelectedIndex = show.IndexOf(TT) != -1 ? show.IndexOf(TT) : 0;
        }

        private void VertexList_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            IPXPmx ThePmxOfNow = GetPmx;
            var temp = new DataGridViewCell[VertexList.SelectedCells.Count];
            if (VertexTab.SelectedTab.Text == "T窗口权重调整")
            {
                if (StartWeightChangeButton.Text == "关闭")
                {
                    VertexList.SelectedCells.CopyTo(temp, 0);
                    /*if (ThePmxOfNow.Bone.Count != Boneshow.Count)
                       {
                           Boneshow = new List<string>();
                           var count = 0;
                           foreach (var item in ThePmxOfNow.Bone)
                           {
                               Boneshow.Add(count + ":" + item.Name);
                               count++;
                           }
                       }*/
                    if (VertexList.SelectedCells.Count > 0 || WeightUndo.Count > 0)
                    {
                        var tempinfo = WeightUndo[temp[0].RowIndex];
                        List<IPXBone> BoneList = new List<IPXBone>(ThePmxOfNow.Bone);
                        if (tempinfo.Bone1 != null)
                        {
                            var ter = ThePmxOfNow.Bone.IndexOf(tempinfo.Bone1);
                            WeightNum1.Text = tempinfo.Weight1.ToString();
                            Bone1List.DataSource = new List<string>(Boneshow);
                            Bone1List.SelectedIndex = tempinfo.Bone1Index;
                        }
                        if (tempinfo.Bone2 != null)
                        {
                            Bone2_s.Checked = true;
                            WeightNum2.Text = tempinfo.Weight2.ToString();
                            Bone2List.DataSource = new List<string>(Boneshow);
                            Bone2List.SelectedIndex = tempinfo.Bone2Index;
                        }
                        else
                        {
                            Bone2_s.Checked = false;
                            Bone2List.DataSource = null;
                        }
                        if (tempinfo.Bone3 != null)
                        {
                            Bone3_s.Checked = true;
                            WeightNum3.Text = tempinfo.Weight3.ToString();
                            Bone3List.DataSource = new List<string>(Boneshow);
                            Bone3List.SelectedIndex = tempinfo.Bone3Index;
                        }
                        else
                        {
                            Bone3_s.Checked = false;
                            Bone3List.DataSource = null;
                        }
                        if (tempinfo.Bone4 != null)
                        {
                            Bone4_s.Checked = true;
                            WeightNum4.Text = tempinfo.Weight4.ToString();
                            Bone4List.DataSource = new List<string>(Boneshow);
                            Bone4List.SelectedIndex = tempinfo.Bone4Index;
                        }
                        else
                        {
                            Bone4_s.Checked = false;
                            Bone4List.DataSource = null;
                        }
                    }
                }
            }
            else if (VertexTab.SelectedTab.Text == "表情操作")
            {
                VertexList.SelectedCells.CopyTo(temp, 0);
                if (MorphMission == null)
                {
                    if (MorphBacData.Count != 0)
                    {
                        MorphBackCountLabel.Text = "备份表情顶点数：" + MorphBacData[temp[0].RowIndex].VertexList.Count().ToString();
                        var i = 0;
                        foreach (var item2 in MorphBacData[temp[0].RowIndex].VertexList)
                        {
                            var Index = MorphSearchList.Find((b) => b.index == item2.index).toindex;
                            if (Index != -1)
                            {
                                Interlocked.Increment(ref i);
                            }
                        }
                        MorphCountLabel.Text = "匹配表情顶点数：" + i;
                    }
                }
            }
        }

        private void WeightKeySet(object sender, KeyPressEventArgs e)//自定义按键设置
        {
            var TempKey = sender as MetroTextBox;
            if (e.KeyChar < 32 || e.KeyChar == bootstate.WeightGetKey
                || e.KeyChar == bootstate.WeightAddKey
                || e.KeyChar == bootstate.WeightAppleKey
                || e.KeyChar == bootstate.WeightMinusKey)
            {
                return;
            }
            switch (TempKey.Name)
            {
                case "WeightGetKey":
                    WeightGetKey.Text = e.KeyChar.ToString();
                    bootstate.WeightGetKey = e.KeyChar;
                    break;

                case "WeightAddKey":
                    WeightAddKey.Text = e.KeyChar.ToString();
                    bootstate.WeightAddKey = e.KeyChar;
                    break;

                case "WeightMinusKey":
                    WeightMinusKey.Text = e.KeyChar.ToString();
                    bootstate.WeightMinusKey = e.KeyChar;
                    break;

                case "WeightAppleKey":
                    WeightAppleKey.Text = e.KeyChar.ToString();
                    bootstate.WeightAppleKey = e.KeyChar;
                    break;
            }
            ThreadPool.QueueUserWorkItem(new WaitCallback(Save));
        }

        private void MouseControlWeight_CheckedChanged(object sender, EventArgs e)
        {
            if (MouseControlWeight.Checked)
            {
                g = SingleBufferedPictureBox.CreateGraphics();
                SingleBufferedPictureBox.MouseClick += MouseControl;
                ThreadPool.QueueUserWorkItem(new WaitCallback(ShowTextInModelView));
                ThreadPool.QueueUserWorkItem(new WaitCallback(StringMaxCount));
            }
            else
            {
                g.Dispose();
                SingleBufferedPictureBox.MouseClick -= MouseControl;
            }
        }

        private void StringMaxCount(object state)
        {
            try
            {
                while (true)
                {
                    if (MouseControlWeight.Checked)
                    {
                        Thread.Sleep(100);
                        Max = Math.Max(Bone1List.Text.Length, Math.Max(Bone2List.Text.Length, Math.Max(Bone3List.Text.Length, Bone4List.Text.Length))) * 15 + 15;
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private int Max = 0;
        private readonly Font font = new Font("微软雅黑", 15);
        private readonly SolidBrush SR = new SolidBrush(Color.Red);
        private readonly SolidBrush SG = new SolidBrush(Color.Green);
        private readonly SolidBrush SB = new SolidBrush(Color.Blue);

        private void ShowTextInModelView(object gc)
        {
            try
            {
                while (true)
                {
                    if (MouseControlWeight.Checked)
                    {
                        g.DrawString(ControlMode, new Font(this.Font.FontFamily, 25), SB, new PointF(5, 5));
                        g.DrawString(Bone1List.Text, font, SB, new PointF(5, 40));
                        g.DrawString(Bone2List.Text, font, SB, new PointF(5, 65));
                        g.DrawString(Bone3List.Text, font, SB, new PointF(5, 90));
                        g.DrawString(Bone4List.Text, font, SB, new PointF(5, 115));
                        g.DrawString(Bone1List.Text != "" ? WeightNum1.Text : "", font, BDEF1Radio.Checked ? SR : BDEF1Radio_2.Checked ? SG : SB, new PointF(Max, 40));
                        g.DrawString(Bone2List.Text != "" ? WeightNum2.Text : "", font, BDEF2Radio.Checked ? SR : BDEF2Radio_2.Checked ? SG : SB, new PointF(Max, 65));
                        g.DrawString(Bone3List.Text != "" ? WeightNum3.Text : "", font, BDEF3Radio.Checked ? SR : BDEF3Radio_2.Checked ? SG : SB, new PointF(Max, 90));
                        g.DrawString(Bone4List.Text != "" ? WeightNum4.Text : "", font, BDEF4Radio.Checked ? SR : BDEF4Radio_2.Checked ? SG : SB, new PointF(Max, 115));
                    }
                    else { break; }
                }
            }
            catch (Exception)
            {
            }
        }

        private void MouseControl(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (ControlMode == "加算")
                {
                    MenuItem_UpdateSelect.PerformClick();//T窗口选中顶点转移到主模型窗口
                    var Verttexlist = ARGS.Host.Connector.View.PmxView.GetSelectedVertexIndices();//从主模型窗口获得选中顶点
                    if (Verttexlist.Length != 0) //判断是否获取了顶点
                    {
                        ThreadPool.QueueUserWorkItem(new WaitCallback(WeightAdd), Verttexlist);
                    }
                }
                else if (ControlMode == "减算")
                {
                    MenuItem_UpdateSelect.PerformClick();//T窗口选中顶点转移到主模型窗口
                    var Verttexlist = ARGS.Host.Connector.View.PmxView.GetSelectedVertexIndices();//从主模型窗口获得选中顶点
                    if (Verttexlist.Length != 0) //判断是否获取了顶点
                    {
                        ThreadPool.QueueUserWorkItem(new WaitCallback(WeightMinus), Verttexlist);
                    }
                }
                else if (ControlMode == "应用")
                {
                    MenuItem_UpdateSelect.PerformClick();//T窗口选中顶点转移到主模型窗口
                    var Verttexlist = ARGS.Host.Connector.View.PmxView.GetSelectedVertexIndices();//从主模型窗口获得选中顶点
                    if (Verttexlist.Length != 0) //判断是否获取了顶点
                    {
                        WeightApple(Verttexlist, null);
                    }
                }
                else if (ControlMode == "获取")
                {
                    MenuItem_UpdateSelect.PerformClick();//T窗口选中顶点转移到主模型窗口
                    var Verttexlist = ARGS.Host.Connector.View.PmxView.GetSelectedVertexIndices();//从主模型窗口获得选中顶点
                    if (Verttexlist.Length != 0) //判断是否获取了顶点
                    {
                        ChangeVerForm(Verttexlist[0]);
                    }
                }
            }
        }

        #endregion
        #region 骨骼列连接

        private void BoneConnectMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ADDBONELIST.Visible == true)
            {
                ADDBONELIST.Visible = false;
                ClearBoneLIST.Visible = false;
                DeleteBoneListItem.Visible = false;
                BoneListConutLabel.Visible = false;
            }
            switch (BoneConnectMode.SelectedIndex)
            {
                case 0:
                    {
                        BoneConnectLabel2.Enabled = false;
                        SelectConnectObject.Enabled = false;
                        StartOperaBone.Text = "生成水平\r\n刚体和J点";
                        CheckBoneConnectFunc.Visible = true;
                        CheckBoneConnectFunc.Checked = false;
                        CheckBoneConnectFunc.Text = "骨骼是否封闭";
                    }
                    break;

                case 1:
                    {
                        BoneConnectLabel2.Enabled = true;
                        BoneConnectLabel2.Text = "选择连接刚体:";
                        SelectConnectObject.Enabled = true;
                        StartOperaBone.Text = "生成垂直\r\n刚体和J点";
                        CheckBoneConnectFunc.Visible = true;
                        CheckBoneConnectFunc.Checked = false;
                        CheckBoneConnectFunc.Text = "修正J点错误\r\n的刚体连接";
                    }
                    break;

                case 2:
                    {
                        BoneConnectLabel2.Enabled = true;
                        SelectConnectObject.Enabled = true;
                        CheckBoneConnectFunc.Visible = false;
                        CheckBoneConnectFunc.Checked = false;
                        BoneConnectLabel2.Text = "选择连接骨骼:";
                        StartOperaBone.Text = "生成多对一\r\n刚体和J点";
                    }
                    break;

                case 3:
                    {
                        ADDBONELIST.Visible = true;
                        ClearBoneLIST.Visible = true;
                        DeleteBoneListItem.Visible = true;
                        BoneListConutLabel.Visible = true;
                        BoneConnectLabel2.Enabled = false;
                        SelectConnectObject.Enabled = false;
                        CheckBoneConnectFunc.Visible = false;
                        CheckBoneConnectFunc.Checked = false;
                        StartOperaBone.Text = "生成多对多\r\n刚体和J点";
                    }
                    break;
            }
        }

        private void StartOperaBone_Click(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem((object state) =>
            {
                IPXPmx ThePmxOfNow = GetPmx;
                IPXPmxBuilder bdx = ARGS.Host.Builder.Pmx;
                switch (BoneConnectMode.SelectedIndex)
                {
                    case 0:
                        {
                            if (BoneCount.Count != 0)
                            {
                                TheFunOfHorizontalConnect();
                            }
                            else
                            {
                                MetroMessageBox.Show(this, "请先选择骨骼后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        break;

                    case 1:
                        {
                            if (BoneCount.Count != 0)
                            {
                                if (SelectConnectObject.SelectedIndex == -1)
                                {
                                    MetroMessageBox.Show(this, "请选择第一排刚体的连接刚体后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    return;
                                }
                                else
                                {
                                    TheFunOfVerticalConnect();
                                }
                            }
                            else
                            {
                                MetroMessageBox.Show(this, "请先选择骨骼后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        break;

                    case 2:
                        {
                            if (BoneCount.Count != 0)
                            {
                                if (SelectConnectObject.SelectedIndex == -1)
                                {
                                    MetroMessageBox.Show(this, "请选择连接骨骼后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    return;
                                }
                                else
                                {
                                    var TempBone = ThePmxOfNow.Bone[SelectConnectObject.SelectedIndex];
                                    bool find = true;
                                    foreach (var item in ThePmxOfNow.Body)
                                    {
                                        if (item.Bone == TempBone)//可能存在多个匹配的刚体
                                        {
                                            find = true;
                                            TheFunOfMultiConnect(item);
                                            break;
                                        }
                                    }
                                    if (!find)
                                    {
                                        IPXBody TempBody1 = null;
                                        TempBody1 = bdx.Body();
                                        TempBody1.BoxKind = BodyBoxKind.Sphere;
                                        TempBody1.Bone = TempBone;
                                        TempBody1.Name = TempBone.Name;
                                        TempBody1.Position = TempBone.Position;
                                        TempBody1.Friction = 0.5f;
                                        TempBody1.Mass = 1;
                                        TempBody1.Group = Convert.ToInt16(BodySelectGroup.SelectedItem) - 1;
                                        TempBody1.PassGroup[Convert.ToInt16(BodySelectGroup.SelectedItem) - 1] = true;
                                        TempBody1.Mode = BodyMode.Dynamic;
                                        TempBody1.Restitution = 0f;
                                        TempBody1.PositionDamping = 0.5f;
                                        TempBody1.RotationDamping = 0.5f;
                                        TempBody1.BoxSize = new V3(0.5f, 0, 0);
                                        ARGS.Host.Connector.Pmx.Update(ThePmxOfNow);
                                        ARGS.Host.Connector.Form.UpdateList(UpdateObject.Body);
                                        TheFunOfMultiConnect(TempBody1);
                                    }
                                }
                            }
                            else
                            {
                                MetroMessageBox.Show(this, "请先选择骨骼后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        break;

                    case 3:
                        {
                            if (Bonelist.Count > 0)
                            {
                                //首先分析列表的刚体是否存在，不存在就放弃
                                //再次分析，列表骨骼对应是否重复，重复的就放弃
                                //根据骨骼建立刚体顺序，
                                //连接方式，列表1连接列表2，列表2连接列表3，如果列表有一列不存在了就停止
                                //最近距离骨骼搜索功能 再说
                                for (int i = 1; i < Bonelist.Count; i++)
                                {
                                    for (int j = 0; j < (Bonelist[i].Length < Bonelist[i - 1].Length ? Bonelist[i].Length : Bonelist[i - 1].Length); j++)
                                    {
                                        var y = Bonelist[i][j];
                                        var Body2 = ThePmxOfNow.Body.Where(x => x.Bone.Equals(ThePmxOfNow.Bone[Bonelist[i][j]]));
                                        var Body1 = ThePmxOfNow.Body.Where(x => x.Bone.Equals(ThePmxOfNow.Bone[Bonelist[i - 1][j]]));
                                        if (Body1.Count() == 0 || Body2.Count() == 0)
                                        {
                                            continue;
                                        }
                                        foreach (var item in Body1)//可能存在一根骨骼 多个刚体的存在，处理方式就是全部链接，链接方式，顺序交叉，估计用不到
                                        {
                                            foreach (var item2 in Body2)
                                            {
                                                if (item.Bone == item2.Bone)//根据body骨骼是否相同，判断是否是同一类刚体，是就跳过
                                                {
                                                    break;
                                                }
                                                if (ThePmxOfNow.Joint.Where(x => x.BodyA == item && x.BodyB == item2).Count() == 0 && ThePmxOfNow.Joint.Where(x => x.BodyA == item2 && x.BodyB == item).Count() == 0)//根据刚体判定时候已经存在刚体链接一样的J点
                                                {
                                                    IPXJoint JointTemp = bdx.Joint();
                                                    JointTemp.Name = item2.Name;
                                                    JointTemp.Position.X = (item.Position.X + item2.Position.X) / 2;
                                                    JointTemp.Position.Y = (item.Position.Y + item2.Position.Y) / 2;
                                                    JointTemp.Position.Z = (item.Position.Z + item2.Position.Z) / 2;
                                                    JointTemp.BodyA = item;
                                                    JointTemp.BodyB = item2;
                                                    JointTemp.Kind = JointKind.Sp6DOF;
                                                    ThePmxOfNow.Joint.Add(JointTemp);
                                                }
                                            }
                                        }
                                    }
                                }
                                BeginInvoke(new MethodInvoker(() =>
                                {
                                    ARGS.Host.Connector.Pmx.Update(ThePmxOfNow);
                                    ARGS.Host.Connector.Form.UpdateList(UpdateObject.Joint);
                                    ARGS.Host.Connector.View.PMDView.UpdateModel();
                                    ARGS.Host.Connector.View.PMDView.UpdateView();
                                }));
                            }
                            else
                            {
                                MetroMessageBox.Show(this, "请添加2个以上的骨骼列表后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        break;
                }
            });
        }

        private void TheFunOfMultiConnect(IPXBody TempBody1)
        {
            IPXPmx ThePmxOfNow = GetPmx;
            List<IPXBone> BoneTemp = new List<IPXBone>();
            foreach (int temp in BoneCount)
            {
                BoneTemp.Add(ThePmxOfNow.Bone[temp]);
            }
            BoneTemp.Sort((x, y) => x.Position.X.CompareTo(y.Position.X));
            BoneCount.Clear();
            foreach (IPXBone temp in AnyToGetDisTance(BoneTemp))
            {
                BoneCount.Add(ThePmxOfNow.Bone.IndexOf(temp));
            }
            try
            {
                if (CheckBoneConnectFunc.Checked)
                {
                    BoneCount.Add(BoneCount[0]);
                }

                IList<IPXBody> body = ThePmxOfNow.Body;
                IList<IPXJoint> joint = ThePmxOfNow.Joint;

                for (int i = 0; i < BoneCount.Count - 1; i++)
                {
                    IPXPmxBuilder bdx = ARGS.Host.Builder.Pmx;
                    bool addbody2 = true;
                    IPXBone TempBone1 = ThePmxOfNow.Bone[BoneCount[i]];
                    IPXBone TempBone2 = ThePmxOfNow.Bone[BoneCount[i + 1]];
                    IPXBody TempBody2 = null;

                    for (int x = 0; x < body.Count; x++)
                    {
                        if (ThePmxOfNow.Body[x].Bone == TempBone2)
                        {
                            addbody2 = false;
                            TempBody2 = body[x];
                        }
                    }

                    if (addbody2)
                    {
                        TempBody2 = bdx.Body();
                        TempBody2.BoxKind = BodyBoxKind.Sphere;
                        TempBody2.Bone = TempBone2;
                        TempBody2.Name = TempBone2.Name;
                        TempBody2.Position = TempBone2.Position;
                        TempBody2.Friction = 0.5f;
                        TempBody2.Mass = 1;
                        TempBody2.Group = Convert.ToInt16(BodySelectGroup.SelectedItem) - 1;
                        TempBody2.PassGroup[Convert.ToInt16(BodySelectGroup.SelectedItem) - 1] = true;
                        TempBody2.Mode = BodyMode.Dynamic;
                        TempBody2.Restitution = 0f;
                        TempBody2.PositionDamping = 0.5f;
                        TempBody2.RotationDamping = 0.5f;
                        TempBody2.BoxSize = new V3(((float)Math.Sqrt(Math.Pow(TempBone2.Position.X - TempBone1.Position.X, 2) + Math.Pow(TempBone2.Position.Y - TempBone1.Position.Y, 2) + Math.Pow(TempBone2.Position.Z - TempBone1.Position.Z, 2))) / 2, 0, 0);
                        body.Add(TempBody2);
                    }
                    bool NotFindJoint = true;
                    foreach (IPXJoint temp in joint)
                    {
                        if (temp.BodyA == TempBody1)
                        {
                            if (temp.BodyB == TempBody2)
                            {
                                NotFindJoint = false;
                                break;
                            }
                        }
                    }
                    if (NotFindJoint)
                    {
                        IPXJoint JointTemp = bdx.Joint();
                        JointTemp.Name = TempBody1.Name + "并列";
                        JointTemp.Position.X = (TempBody1.Position.X + TempBody2.Position.X) / 2;
                        JointTemp.Position.Y = (TempBody1.Position.Y + TempBody2.Position.Y) / 2;
                        JointTemp.Position.Z = (TempBody1.Position.Z + TempBody2.Position.Z) / 2;
                        JointTemp.BodyA = TempBody1;
                        JointTemp.BodyB = TempBody2;
                        JointTemp.Kind = JointKind.Sp6DOF;
                        joint.Add(JointTemp);
                    }
                }
            }
            catch (Exception)
            {
            }
            BeginInvoke(new MethodInvoker(() =>
            {
                ARGS.Host.Connector.Pmx.Update(ThePmxOfNow);
                ARGS.Host.Connector.Form.UpdateList(UpdateObject.All);
                ARGS.Host.Connector.View.PMDView.UpdateModel();
                ARGS.Host.Connector.View.PMDView.UpdateView();
                BoneCount.Clear();
            }));
            if (!AutomaticRadioButton.Checked)
            {
                ClearList("bone");
            }
        }

        private static IEnumerable<IPXBone> AnyToGetDisTance(List<IPXBone> OperaTemp)
        {
            IPXBone getbone = OperaTemp[0];
            do
            {
                double distance = 0;
                IPXBone SaveBone = null;
                foreach (IPXBone caltemp in OperaTemp)
                {
                    double caldistance = Math.Sqrt((Math.Pow(caltemp.Position.X - getbone.Position.X, 2) + Math.Pow(caltemp.Position.Y - getbone.Position.Y, 2) + Math.Pow(caltemp.Position.Z - getbone.Position.Z, 2)));
                    if (caldistance <= distance || distance == 0)
                    {
                        distance = caldistance;
                        SaveBone = caltemp;
                    }
                }
                getbone = SaveBone;
                OperaTemp.Remove(SaveBone);
                yield return getbone;
            } while (OperaTemp.Count > 0);
            yield break;
        }

        public void TheFunOfHorizontalConnect()
        {
            IPXPmx ThePmxOfNow = GetPmx;
            List<IPXBone> BoneTemp = new List<IPXBone>();
            foreach (int temp in BoneCount)
            {
                BoneTemp.Add(ThePmxOfNow.Bone[temp]);
            }
            BoneTemp.Sort((x, y) => x.Position.X.CompareTo(y.Position.X));
            BoneCount.Clear();
            foreach (IPXBone temp in AnyToGetDisTance(BoneTemp))
            {
                BoneCount.Add(ThePmxOfNow.Bone.IndexOf(temp));
            }
            try
            {
                if (CheckBoneConnectFunc.Checked)
                {
                    BoneCount.Add(BoneCount[0]);
                }

                IList<IPXBody> body = ThePmxOfNow.Body;
                IList<IPXJoint> joint = ThePmxOfNow.Joint;

                for (int i = 0; i < BoneCount.Count - 1; i++)
                {
                    IPXPmxBuilder bdx = ARGS.Host.Builder.Pmx;
                    bool addbody1 = true;
                    bool addbody2 = true;
                    IPXBone TempBone1 = ThePmxOfNow.Bone[BoneCount[i]];
                    IPXBone TempBone2 = ThePmxOfNow.Bone[BoneCount[i + 1]];
                    IPXBody TempBody1 = null;
                    IPXBody TempBody2 = null;
                    for (int x = 0; x < body.Count; x++)
                    {
                        if (ThePmxOfNow.Body[x].Bone == TempBone1)
                        {
                            addbody1 = false;
                            TempBody1 = body[x];
                        }
                    }
                    for (int x = 0; x < body.Count; x++)
                    {
                        if (ThePmxOfNow.Body[x].Bone == TempBone2)
                        {
                            addbody2 = false;
                            TempBody2 = body[x];
                        }
                    }
                    if (addbody1)
                    {
                        TempBody1 = bdx.Body();
                        TempBody1.BoxKind = BodyBoxKind.Sphere;
                        TempBody1.Bone = TempBone1;
                        TempBody1.Name = TempBone1.Name;
                        TempBody1.Position = TempBone1.Position;
                        TempBody1.Friction = 0.5f;
                        TempBody1.Mass = 1;
                        TempBody1.Group = Convert.ToInt16(BodySelectGroup.SelectedItem) - 1;
                        TempBody1.PassGroup[Convert.ToInt16(BodySelectGroup.SelectedItem) - 1] = true;
                        TempBody1.Mode = BodyMode.Dynamic;
                        TempBody1.Restitution = 0f;
                        TempBody1.PositionDamping = 0.5f;
                        TempBody1.RotationDamping = 0.5f;
                        TempBody1.BoxSize = new V3(((float)Math.Sqrt(Math.Pow(TempBone2.Position.X - TempBone1.Position.X, 2) + Math.Pow(TempBone2.Position.Y - TempBone1.Position.Y, 2) + Math.Pow(TempBone2.Position.Z - TempBone1.Position.Z, 2))) / 2, 0, 0);
                        body.Add(TempBody1);
                    }
                    if (addbody2)
                    {
                        TempBody2 = bdx.Body();
                        TempBody2.BoxKind = BodyBoxKind.Sphere;
                        TempBody2.Bone = TempBone2;
                        TempBody2.Name = TempBone2.Name;
                        TempBody2.Position = TempBone2.Position;
                        TempBody2.Friction = 0.5f;
                        TempBody2.Mass = 1;
                        TempBody2.Group = Convert.ToInt16(BodySelectGroup.SelectedItem) - 1;
                        TempBody2.PassGroup[Convert.ToInt16(BodySelectGroup.SelectedItem) - 1] = true;
                        TempBody2.Mode = BodyMode.Dynamic;
                        TempBody2.Restitution = 0f;
                        TempBody2.PositionDamping = 0.5f;
                        TempBody2.RotationDamping = 0.5f;
                        TempBody2.BoxSize = new V3(((float)Math.Sqrt(Math.Pow(TempBone2.Position.X - TempBone1.Position.X, 2) + Math.Pow(TempBone2.Position.Y - TempBone1.Position.Y, 2) + Math.Pow(TempBone2.Position.Z - TempBone1.Position.Z, 2))) / 2, 0, 0);
                        body.Add(TempBody2);
                    }
                    bool NotFindJoint = true;
                    foreach (IPXJoint temp in joint)
                    {
                        if (temp.BodyA == TempBody1)
                        {
                            if (temp.BodyB == TempBody2)
                            {
                                NotFindJoint = false;
                                break;
                            }
                        }
                    }
                    if (NotFindJoint)
                    {
                        IPXJoint JointTemp = bdx.Joint();
                        JointTemp.Name = TempBody1.Name;
                        JointTemp.Position.X = (TempBody1.Position.X + TempBody2.Position.X) / 2;
                        JointTemp.Position.Y = (TempBody1.Position.Y + TempBody2.Position.Y) / 2;
                        JointTemp.Position.Z = (TempBody1.Position.Z + TempBody2.Position.Z) / 2;
                        JointTemp.BodyA = TempBody1;
                        JointTemp.BodyB = TempBody2;
                        JointTemp.Kind = JointKind.Sp6DOF;
                        joint.Add(JointTemp);
                    }
                }
            }
            catch (Exception)
            {
            }
            BeginInvoke(new MethodInvoker(() =>
            {
                ARGS.Host.Connector.Pmx.Update(ThePmxOfNow);
                ARGS.Host.Connector.Form.UpdateList(UpdateObject.Body);
                ARGS.Host.Connector.Form.UpdateList(UpdateObject.Joint);
                ARGS.Host.Connector.View.PMDView.UpdateModel();
                ARGS.Host.Connector.View.PMDView.UpdateView();
                BoneCount.Clear();
            }));
            if (!AutomaticRadioButton.Checked)
            {
                ClearList("bone");
            }
        }

        public void TheFunOfVerticalConnect()
        {
            IPXPmx ThePmxOfNow = GetPmx;
            var BoneTemp = new List<IPXBone>();
            foreach (int temp in BoneCount)
            {
                BoneTemp.Add(ThePmxOfNow.Bone[temp]);
            }
            BoneTemp.Sort((x, y) => -x.Position.Y.CompareTo(y.Position.Y));
            BoneCount.Clear();
            foreach (IPXBone temp in BoneTemp)
            {
                BoneCount.Add(ThePmxOfNow.Bone.IndexOf(temp));
            }

            IList<IPXBody> body = ThePmxOfNow.Body;
            IList<IPXJoint> joint = ThePmxOfNow.Joint;
            IPXBody PreBody = null;

            for (int i = 0, j = 0; i < BoneCount.Count; i++)
            {
                IPXPmxBuilder bdx = ARGS.Host.Builder.Pmx;

                bool NotFindBody = true;
                int FindBody = 0;
                for (int x = ThePmxOfNow.Body.Count - 1; x >= 0; x--)
                {
                    if (ThePmxOfNow.Body[x].Bone == ThePmxOfNow.Bone[BoneCount[i]])//遍历所有刚体，寻找重复骨骼
                    {
                        NotFindBody = false;
                        FindBody = x;
                        break;
                    }
                }
                if (j == 0 && NotFindBody)//建立刚体 并且是第一个骨骼
                {
                    IPXBody BodyTemp = bdx.Body();//建立刚体
                    BodyTemp.BoxKind = BodyBoxKind.Sphere;
                    IPXBone Bone = ThePmxOfNow.Bone[BoneCount[i]];
                    BodyTemp.Bone = Bone;
                    BodyTemp.Name = Bone.Name;
                    BodyTemp.Position = Bone.Position;
                    BodyTemp.Friction = 0.5f;
                    BodyTemp.Mass = 1;
                    BodyTemp.Group = Convert.ToInt16(BodySelectGroup.SelectedItem) - 1;
                    BodyTemp.PassGroup[Convert.ToInt16(BodySelectGroup.SelectedItem) - 1] = true;
                    BodyTemp.Mode = BodyMode.Dynamic;
                    BodyTemp.Restitution = 0f;
                    BodyTemp.PositionDamping = 0.5f;
                    BodyTemp.RotationDamping = 0.5f;
                    IPXBody tempbody = null;
                    try
                    {
                        tempbody = ThePmxOfNow.Body[SelectConnectObject.SelectedIndex];//从上一刚体获得刚体大小
                    }
                    catch (Exception)
                    {
                        SelectConnectObject.DataSource = new List<string>();
                        return;
                    }
                    BodyTemp.BoxSize = new V3(((float)Math.Sqrt(Math.Pow(tempbody.Position.X - Bone.Position.X, 2) + Math.Pow(tempbody.Position.Y - Bone.Position.Y, 2) + Math.Pow(tempbody.Position.Z - Bone.Position.Z, 2))) - (Math.Max(tempbody.BoxSize.X, tempbody.BoxSize.Y)), 0, 0);
                    body.Add(BodyTemp);
                    PreBody = BodyTemp;

                    IPXJoint JointTemp = bdx.Joint();//建立连接J点
                    JointTemp.Name = Bone.Name;
                    JointTemp.Position.X = (tempbody.Position.X + BodyTemp.Position.X) / 2;
                    JointTemp.Position.Y = (tempbody.Position.Y + BodyTemp.Position.Y) / 2;
                    JointTemp.Position.Z = (tempbody.Position.Z + BodyTemp.Position.Z) / 2;
                    JointTemp.BodyA = tempbody;
                    JointTemp.BodyB = BodyTemp;
                    JointTemp.Kind = JointKind.Sp6DOF;
                    bool jointadd = true;
                    foreach (IPXJoint temp in ThePmxOfNow.Joint)
                    {
                        List<IPXJoint> TempJoint = new List<IPXJoint>();
                        if (temp.BodyA == JointTemp.BodyA)
                        {
                            TempJoint.Add(temp);
                        }
                        if (TempJoint.Count != 0)
                        {
                            bool find = false;
                            foreach (IPXJoint Temp in TempJoint)
                            {
                                if (Temp.BodyB == JointTemp.BodyB)
                                {
                                    jointadd = false;
                                    find = true;
                                    break;
                                }
                            }
                            if (find)
                            { break; }
                        }
                    }
                    if (jointadd)
                    {
                        joint.Add(JointTemp);
                    }
                    j++;
                }
                else if (NotFindBody)
                {
                    IPXBody BodyTemp = bdx.Body();
                    BodyTemp.BoxKind = BodyBoxKind.Sphere;
                    IPXBone Bone = ThePmxOfNow.Bone[BoneCount[i]];
                    BodyTemp.Bone = Bone;
                    BodyTemp.Name = Bone.Name;
                    BodyTemp.Position = Bone.Position;
                    BodyTemp.Friction = 0.5f;
                    BodyTemp.Mass = 1;
                    BodyTemp.Group = Convert.ToInt16(BodySelectGroup.SelectedItem) - 1;
                    BodyTemp.PassGroup[Convert.ToInt16(BodySelectGroup.SelectedItem) - 1] = true;
                    BodyTemp.Mode = BodyMode.Dynamic;
                    BodyTemp.Restitution = 0f;
                    BodyTemp.PositionDamping = 0.5f;
                    BodyTemp.RotationDamping = 0.5f;
                    BodyTemp.BoxSize = new V3(((float)Math.Sqrt(Math.Pow(PreBody.Position.X - Bone.Position.X, 2) + Math.Pow(PreBody.Position.Y - Bone.Position.Y, 2) + Math.Pow(PreBody.Position.Z - Bone.Position.Z, 2))) / 2, 0, 0);
                    body.Add(BodyTemp);
                    IPXJoint JointTemp = bdx.Joint();
                    JointTemp.Name = BodyTemp.Name;
                    JointTemp.Position.X = (PreBody.Position.X + BodyTemp.Position.X) / 2;
                    JointTemp.Position.Y = (PreBody.Position.Y + BodyTemp.Position.Y) / 2;
                    JointTemp.Position.Z = (PreBody.Position.Z + BodyTemp.Position.Z) / 2;
                    JointTemp.BodyA = PreBody;
                    JointTemp.BodyB = BodyTemp;
                    JointTemp.Kind = JointKind.Sp6DOF;
                    bool jointadd = true;
                    foreach (IPXJoint temp in ThePmxOfNow.Joint)
                    {
                        List<IPXJoint> TempJoint = new List<IPXJoint>();
                        if (temp.BodyA == JointTemp.BodyA)
                        {
                            TempJoint.Add(temp);
                        }
                        if (TempJoint.Count != 0)
                        {
                            bool find = false;
                            foreach (IPXJoint Temp in TempJoint)
                            {
                                if (Temp.BodyB == JointTemp.BodyB)
                                {
                                    jointadd = false;
                                    find = true;
                                    break;
                                }
                            }
                            if (find)
                            { break; }
                        }
                    }
                    if (jointadd)
                    {
                        joint.Add(JointTemp);
                    }
                    PreBody = BodyTemp;
                }
                else if (j == 0 && NotFindBody == false)
                {
                    IPXBody tempbody = ThePmxOfNow.Body[SelectConnectObject.SelectedIndex];
                    IPXBody BodyTemp = ThePmxOfNow.Body[FindBody];
                    PreBody = BodyTemp;
                    IPXJoint JointTemp = bdx.Joint();
                    JointTemp.Name = BodyTemp.Name;
                    JointTemp.Position.X = (tempbody.Position.X + BodyTemp.Position.X) / 2;
                    JointTemp.Position.Y = (tempbody.Position.Y + BodyTemp.Position.Y) / 2;
                    JointTemp.Position.Z = (tempbody.Position.Z + BodyTemp.Position.Z) / 2;
                    JointTemp.BodyA = tempbody;
                    JointTemp.BodyB = BodyTemp;
                    JointTemp.Kind = JointKind.Sp6DOF;
                    bool jointadd = true;
                    foreach (IPXJoint temp in ThePmxOfNow.Joint)
                    {
                        List<IPXJoint> TempJoint = new List<IPXJoint>();
                        if (temp.BodyA == JointTemp.BodyA)
                        {
                            TempJoint.Add(temp);
                        }
                        if (TempJoint.Count != 0)
                        {
                            bool find = false;
                            foreach (IPXJoint Temp in TempJoint)
                            {
                                if (Temp.BodyB == JointTemp.BodyB)
                                {
                                    jointadd = false;
                                    find = true;
                                    break;
                                }
                            }
                            if (find)
                            { break; }
                        }
                    }
                    if (CheckBoneConnectFunc.Checked)
                    {
                        for (int z = joint.Count - 1; z >= 0; z--)
                        {
                            if (joint[z].BodyA == JointTemp.BodyB)
                            {
                                if (joint[z].BodyB == JointTemp.BodyA)
                                {
                                    joint[z].BodyA = JointTemp.BodyA;
                                    joint[z].BodyB = JointTemp.BodyB;
                                    jointadd = false;
                                    break;
                                }
                            }
                        }
                    }
                    if (jointadd)
                    {
                        joint.Add(JointTemp);
                    }
                    j++;
                }
            }
            BeginInvoke(new MethodInvoker(() =>
            {
                ARGS.Host.Connector.Pmx.Update(ThePmxOfNow);
                ARGS.Host.Connector.Form.UpdateList(UpdateObject.Body);
                ARGS.Host.Connector.Form.UpdateList(UpdateObject.Joint);
                ARGS.Host.Connector.View.PMDView.UpdateModel();
                ARGS.Host.Connector.View.PMDView.UpdateView();
                BoneCount.Clear();
            }));
            if (!AutomaticRadioButton.Checked)
            {
                ClearList("bone");
            }
        }

        private void SelectConnectObject_DropDown(object sender, EventArgs e)
        {
            IPXPmx ThePmxOfNow = GetPmx;
            if (ThePmxOfNow == null)
            {
                ThePmxOfNow = ARGS.Host.Connector.Pmx.GetCurrentState();
            }
            List<string> add = new List<string>();
            add.Clear();
            switch (BoneConnectMode.SelectedIndex)
            {
                case 1:
                    {
                        foreach (var temp in ThePmxOfNow.Body)
                        {
                            add.Add(temp.Name);
                        }
                        SelectConnectObject.DataSource = add;
                    }
                    break;

                case 2:
                    {
                        foreach (var temp in ThePmxOfNow.Bone)
                        {
                            add.Add(temp.Name);
                        }
                        SelectConnectObject.DataSource = add;
                    }
                    break;
            }
        }

        private List<int[]> Bonelist = new List<int[]>();

        private void ADDBONELIST_Click(object sender, EventArgs e)
        {
            if (BoneCount.Count != 0)
            {
                Bonelist.Add(BoneCount.ToArray());
                BoneCount.Clear();
                if (!AutomaticRadioButton.Checked)
                {
                    ClearList("bone");
                }
                BoneListConutLabel.Text = "已经添加:" + Bonelist.Count + "项";
            }
            else
            {
                MetroMessageBox.Show(this, "请先选择骨骼后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ClearBoneLIST_Click(object sender, EventArgs e)
        {
            Bonelist = new List<int[]>();
            BoneListConutLabel.Text = "已经添加:0项";
        }

        private void DeleteBoneListItem_Click(object sender, EventArgs e)
        {
            if (Bonelist.Count > 0)
            {
                Bonelist.RemoveAt(Bonelist.Count - 1);
                BoneListConutLabel.Text = "已经添加:" + Bonelist.Count + "项";
            }
        }

        #endregion
        #region 镜像操作

        private void MirrorOperaStatus(object sender, EventArgs e)
        {
            SelectVertexIndex = new List<int>();
            MirrorVertexCountLabel.Text = "已添加:0";
            if (MirrorWeightCheck.Checked)
            {
                MirrorAddVertexButton.Enabled = true;
            }
            else
            {
                MirrorAddVertexButton.Enabled = false;
            }
        }

        private void MirrorAddVertexButton_Click(object sender, EventArgs e)
        {
            if (MirrorAddVertexButton.Text == "添加顶点")
            {
                if (ARGS.Host.Connector.View.PmxView.GetSelectedVertexIndices().Length != 0)
                {
                    SelectVertexIndex = new List<int>(ARGS.Host.Connector.View.PmxView.GetSelectedVertexIndices());
                    MirrorVertexCountLabel.Text = "已添加:" + SelectVertexIndex.Count;
                    MirrorAddVertexButton.Text = "清空";
                }
                else
                {
                    MetroMessageBox.Show(this, "请先选择顶点后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MirrorVertexCountLabel.Text = "已添加:0";
                MirrorAddVertexButton.Text = "添加顶点";
            }
        }

        private Func<V3, V3, float> Getdistance = (index, pos) =>
                   {
                       float Num = -index.X - pos.X;
                       float Num2 = index.Y - pos.Y;
                       float Num3 = index.Z - pos.Z;
                       return Num * Num + Num2 * Num2 + Num3 * Num3;//负负得正，一定为正
                   };

        private void MirrorSelectBoneButton_Click(object sender, EventArgs e)
        {
            if (BoneCount.Count != 0)
            {
                new Task(() =>
                {
                    Dictionary<IPXBone, IPXBone> MirrorBone = new Dictionary<IPXBone, IPXBone>();
                    List<int> SDEF = new List<int>();
                    var temppmx = ARGS.Host.Connector.Pmx.GetCurrentState();
                    Action<int, int> ApplyWeight = (OriIndex, MirrorIndex) =>
                    {
                        temppmx.Vertex[OriIndex].Bone1 = temppmx.Vertex[MirrorIndex].Bone1;
                        temppmx.Vertex[OriIndex].Weight1 = temppmx.Vertex[MirrorIndex].Weight1;
                        temppmx.Vertex[OriIndex].Bone2 = temppmx.Vertex[MirrorIndex].Bone2;
                        temppmx.Vertex[OriIndex].Weight2 = temppmx.Vertex[MirrorIndex].Weight2;
                        temppmx.Vertex[OriIndex].Bone3 = temppmx.Vertex[MirrorIndex].Bone3;
                        temppmx.Vertex[OriIndex].Weight3 = temppmx.Vertex[MirrorIndex].Weight3;
                        temppmx.Vertex[OriIndex].Bone4 = temppmx.Vertex[MirrorIndex].Bone4;
                        temppmx.Vertex[OriIndex].Weight4 = temppmx.Vertex[MirrorIndex].Weight4;
                        var Bone1 = (from t in MirrorBone
                                     where t.Key.Equals(temppmx.Vertex[MirrorIndex].Bone1)
                                     select t.Value).FirstOrDefault();
                        if (Bone1 != null)
                        {
                            temppmx.Vertex[OriIndex].Bone1 = Bone1;
                        }
                        if (temppmx.Vertex[MirrorIndex].Bone2 != null)
                        {
                            var Bone2 = (from t in MirrorBone
                                         where t.Key.Equals(temppmx.Vertex[MirrorIndex].Bone2)
                                         select t.Value).FirstOrDefault();
                            if (Bone2 != null)
                            {
                                temppmx.Vertex[OriIndex].Bone2 = Bone2;
                            }
                        }
                        if (temppmx.Vertex[MirrorIndex].Bone3 != null)
                        {
                            var Bone3 = (from t in MirrorBone
                                         where t.Key.Equals(temppmx.Vertex[MirrorIndex].Bone3)
                                         select t.Value).FirstOrDefault();
                            if (Bone3 != null)
                            {
                                temppmx.Vertex[MirrorIndex].Bone2 = Bone3;
                            }
                            var Bone4 = (from t in MirrorBone
                                         where t.Key.Equals(temppmx.Vertex[MirrorIndex].Bone4)
                                         select t.Value).FirstOrDefault();
                            if (Bone4 != null)
                            {
                                temppmx.Vertex[OriIndex].Bone4 = Bone4;
                            }
                        }
                        if (temppmx.Vertex[OriIndex].SDEF)
                        {
                            SDEF.Add(OriIndex);
                        }
                    };
                    int ProgressBar = 0;
                    var ProgressBarThread = new Thread(() =>
                          {
                              AnalyseBoneProgressBar.Value = 0;
                              AnalyseBoneProgressBar.Maximum = SelectVertexIndex.Count();
                              do
                              {
                                  Thread.Sleep(10);
                                  AnalyseBoneProgressBar.Value = ProgressBar;
                                  AnalyseBoneAndDeleteBoneLabel.Text = ProgressBar + "/" + SelectVertexIndex.Count();
                              }
                              while (ProgressBar != AnalyseBoneProgressBar.Maximum);
                              AnalyseBoneProgressBar.Value = 0;
                              AnalyseBoneAndDeleteBoneLabel.Text = "需要删除骨骼数: 0个";
                              Refresh();
                          });
                    if (MirrorMode.Checked)
                    {
                        List<IPXBone> TempBone = new List<IPXBone>();//镜像的骨骼
                        #region 骨骼镜像
                        BoneCount.ForEach((Index) =>
                        {
                            var tempbone = (IPXBone)temppmx.Bone[Index].Clone();
                            tempbone.Position.X = -tempbone.Position.X;
                            TempBone.Add(tempbone);
                        });
                        for (int i = 0; i < BoneCount.Count; i++)
                        {
                            if (temppmx.Bone[BoneCount[i]].Parent != null)
                            {
                                var parent = (from t in BoneCount
                                              where temppmx.Bone[BoneCount[i]].Parent == temppmx.Bone[t]
                                              select t).FirstOrDefault();
                                if (parent != 0)
                                {
                                    TempBone[i].Parent = TempBone[BoneCount.IndexOf(parent)];
                                }
                            }
                            if (temppmx.Bone[BoneCount[i]].ToBone != null)
                            {
                                var ToBone = (from t in BoneCount
                                              where temppmx.Bone[BoneCount[i]].ToBone == temppmx.Bone[t]
                                              select t).FirstOrDefault();
                                if (ToBone != 0)
                                {
                                    TempBone[i].ToBone = TempBone[BoneCount.IndexOf(ToBone)];
                                }
                            }
                            else
                            {
                                TempBone[i].ToOffset.X = -TempBone[i].ToOffset.X;
                            }
                            TempBone[i].Name = TempBone[i].Name.Replace(MirrorOriChar.Text, MirrorFinChar.Text);
                            if (TempBone[i].Name == temppmx.Bone[BoneCount[i]].Name)
                            {
                                TempBone[i].Name = MirrorFinChar.Text + "-" + TempBone[i].Name;
                            }
                            temppmx.Bone.Add(TempBone[i]);
                            try
                            {
                                MirrorBone.Add(temppmx.Bone[BoneCount[i]], TempBone[i]);//找到相同骨骼会出错注意,考虑同名问题
                            }
                            catch (Exception)
                            {
                            }
                        }
                        #endregion
                        #region 权重镜像
                        if (MirrorWeightCheck.Checked && SelectVertexIndex.Count != 0)
                        {
                            ProgressBarThread.Start();
                            var tempVertex = new List<int>();
                            //计算选中顶点中心点的坐标，以中心点为原点，计算选中顶点和中心点最远的距离
                            //计算对称中心点以最远距离为范围圆内的所有顶点
                            //上述顶点选中顶点
                            var V3Count = new V3();
                            foreach (var item in SelectVertexIndex)
                            {
                                V3Count += temppmx.Vertex[item].Position;
                            }
                            V3Count = V3Count / SelectVertexIndex.Count; //得到中心点坐标
                            var NUM = (from item in SelectVertexIndex
                                       let dis = Getdistance(V3Count, temppmx.Vertex[item].Position)
                                       orderby dis descending
                                       select dis).First() + 1;//得到选中顶点和中心坐标最远的距离+1
                            tempVertex = (from T in Enumerable.Range(0, temppmx.Vertex.Count).AsParallel()
                                          where NUM >= Getdistance(V3Count, temppmx.Vertex[T].Position)
                                          select T).ToList();//遍历顶点，得到距离范围内顶点序列
                            Parallel.ForEach(SelectVertexIndex, item =>
                            {
                                var num = 10000000000f;
                                var Select = -1;
                                foreach (var item2 in tempVertex.AsParallel())
                                {
                                    var dis = Getdistance(temppmx.Vertex[item].Position, temppmx.Vertex[item2].Position);
                                    if (num > dis)
                                    {
                                        num = dis;
                                        Select = item2;
                                    }
                                }
                                ApplyWeight(item, Select);
                                Interlocked.Increment(ref ProgressBar);
                            });
                        }
                        #endregion
                        #region 刚体镜像
                        var MirrorBody = new Dictionary<IPXBody, IPXBody>();
                        if (MirrorBodyCheck.Checked)
                        {
                            foreach (var item in MirrorBone)
                            {
                                var Body = (from T in temppmx.Body
                                            where T.Bone != null
                                            where T.Bone.Equals(item.Key)
                                            select T).FirstOrDefault();
                                if (Body != null)
                                {
                                    var CreateBody = (IPXBody)Body.Clone();
                                    CreateBody.Name = item.Value.Name;
                                    CreateBody.Bone = item.Value;
                                    CreateBody.Position.X = -CreateBody.Position.X;
                                    CreateBody.Rotation.Y = -CreateBody.Rotation.Y;
                                    CreateBody.Rotation.Z = -CreateBody.Rotation.Z;
                                    MirrorBody.Add(Body, CreateBody);
                                    temppmx.Body.Add(CreateBody);
                                }
                            }
                        }
                        #endregion
                        #region J点镜像
                        if (MirrorJointCheck.Checked)
                        {
                            if (MirrorBody.Count != 0)
                            {
                                var AllAddB = new List<IPXJoint>();
                                var AllAddA = new List<IPXJoint>();
                                foreach (var item in MirrorBody)
                                {
                                    var joint = from T in temppmx.Joint
                                                where T.BodyB == item.Key
                                                select T;
                                    var joint2 = from T in temppmx.Joint
                                                 where T.BodyA == item.Key
                                                 select T;
                                    if (joint.Count() != 0)
                                    {
                                        joint.ToList().ForEach((T) =>
                                        {
                                            var CreateJoint = (IPXJoint)T.Clone();
                                            CreateJoint.Name = item.Value.Name;
                                            CreateJoint.Position.X = -CreateJoint.Position.X;
                                            CreateJoint.Rotation.Y = -CreateJoint.Rotation.Y;
                                            CreateJoint.Rotation.Z = -CreateJoint.Rotation.Z;
                                            CreateJoint.BodyB = item.Value;
                                            AllAddB.Add(CreateJoint);
                                        });
                                    }
                                    if (joint2.Count() != 0)
                                    {
                                        joint2.ToList().ForEach((T) =>
                                        {
                                            var CreateJoint = (IPXJoint)T.Clone();
                                            CreateJoint.Name = item.Value.Name;
                                            CreateJoint.Position.X = -CreateJoint.Position.X;
                                            CreateJoint.Rotation.Y = -CreateJoint.Rotation.Y;
                                            CreateJoint.Rotation.Z = -CreateJoint.Rotation.Z;
                                            CreateJoint.BodyA = item.Value;
                                            AllAddA.Add(CreateJoint);
                                        });
                                    }
                                }
                                if (AllAddB.Count != 0)
                                {
                                    foreach (var item in MirrorBody)
                                    {
                                        AllAddB.ForEach((T) =>
                                        {
                                            if (T.BodyA == item.Key)
                                            {
                                                T.BodyA = item.Value;
                                            }
                                        });
                                    }
                                }
                                if (AllAddA.Count != 0)
                                {
                                    foreach (var item in MirrorBody)
                                    {
                                        AllAddA.ForEach((T) =>
                                        {
                                            if (T.BodyB == item.Key)
                                            {
                                                T.BodyB = item.Value;
                                            }
                                            var find = from T1 in AllAddB
                                                       where T1.BodyA == T.BodyA && T1.BodyB == T.BodyB
                                                       select T1;
                                            if (find.Count() != 0)
                                            {
                                                find.ToList().ForEach((T2) =>
                                                {
                                                    AllAddB.Remove(T2);
                                                });
                                            }
                                        });
                                    }
                                }

                                if (AllAddB.Count != 0)
                                {
                                    AllAddB.ForEach((T) =>
                                    {
                                        temppmx.Joint.Add(T);
                                    });
                                }
                                if (AllAddA.Count != 0)
                                {
                                    AllAddA.ForEach((T) =>
                                    {
                                        temppmx.Joint.Add(T);
                                    });
                                }
                            }
                            else
                            {
                                MetroMessageBox.Show(this, "镜像创建模式下勾选刚体镜像后才能进行J点镜像", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }
                        }
                        BeginInvoke(new MethodInvoker(() =>
                        {
                            ARGS.Host.Connector.Pmx.Update(temppmx);
                            ARGS.Host.Connector.Form.UpdateList(UpdateObject.All);
                            ARGS.Host.Connector.View.PmxView.UpdateView();
                            ARGS.Host.Connector.View.PmxView.UpdateModel();
                            ARGS.Host.Connector.View.PmxView.SetSelectedVertexIndices(SDEF.ToArray());
                            ((Button)btnGetObject).PerformClick();
                        }));
                        #endregion
                    }
                    else
                    {
                        #region 骨骼位置镜像
                        //根据骨骼名建立选中骨骼和对称骨骼的对应词典
                        var TempBone = new Dictionary<int, int>();
                        foreach (DataGridViewRow item in BoneList.Rows)
                        {
                            var bone1 = int.Parse(item.Cells[0].Value.ToString().Split(':')[0]);
                            var bone2 = int.Parse(item.Cells[2].Value.ToString().Split(':')[0]);
                            TempBone.Add(bone1, bone2);
                            MirrorBone.Add(temppmx.Bone[bone1], temppmx.Bone[bone2]);
                        }
                        #region 骨骼镜像
                        if (MirrorBoneCheck.Checked)
                        {
                            TempBone.AsParallel().ForAll((Temp) =>
                            {
                                //目前先这样，看情况追加骨骼亲子啊，IK同步啊之类的
                                if (MirrorPosionPlusPara.Checked)
                                {
                                    temppmx.Bone[Temp.Value].Position = temppmx.Bone[Temp.Key].Position;
                                }
                            });
                        }
                        #endregion
                        #region 权重镜像
                        if (MirrorWeightCheck.Checked && SelectVertexIndex.Count != 0)
                        {
                            int i = 0;
                            ProgressBarThread.Start();
                            //计算选中顶点中心点的坐标，以中心点为原点，计算选中顶点和中心点最远的距离
                            //计算对称中心点以最远距离为范围圆内的所有顶点
                            //上述顶点选中顶点
                            var V3Count = new V3();
                            foreach (var item in SelectVertexIndex)
                            {
                                V3Count += temppmx.Vertex[item].Position;
                            }
                            V3Count = V3Count / SelectVertexIndex.Count;
                            //得到中心点坐标
                            var NUM = (from item in SelectVertexIndex
                                       let dis = Getdistance(V3Count, temppmx.Vertex[item].Position)
                                       orderby dis descending
                                       select dis).First() + 1;//得到选中顶点和中心坐标最远的距离
                            var tempVertex = from T in Enumerable.Range(0, temppmx.Vertex.Count).AsParallel()
                                             where NUM >= Getdistance(V3Count, temppmx.Vertex[T].Position)
                                             select T;//遍历顶点，得到距离范围内顶点序列
                            Parallel.ForEach(SelectVertexIndex, item =>
                            {
                                var num = 10000000000f;
                                var Select = -1;
                                foreach (var item2 in tempVertex.AsParallel())
                                {
                                    var dis = Getdistance(temppmx.Vertex[item].Position, temppmx.Vertex[item2].Position);
                                    if (num > dis)
                                    {
                                        num = dis;
                                        Select = item2;
                                    }
                                }
                                ApplyWeight(item, Select);
                                Interlocked.Increment(ref i);
                            });
                        }
                        #endregion
                        #region 刚体镜像
                        if (MirrorJointCheck.Checked)
                        {
                            var MirrorBody = new Dictionary<int, int>();
                            TempBone.AsParallel().ForAll((Temp) =>
                            {
                                var Body1 = temppmx.Body.FirstOrDefault((x) => x.Bone == temppmx.Bone[Temp.Key]);
                                var Body2 = temppmx.Body.FirstOrDefault((x) => x.Bone == temppmx.Bone[Temp.Value]);
                            });
                        }
                        #endregion
                        #endregion
                    }
                }).Start();
            }
            else
            {
                MetroMessageBox.Show(this, "请先选择骨骼后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void MirrorMode_CheckedChanged(object sender, EventArgs e)
        {
            List<string> Bonelist = new List<string>();
            var com = new ComboBox();
            com.Visible = false;
            com.Name = "MirrorControl";
            com.SelectedValueChanged += delegate
            {
                try
                {
                    BoneList.CurrentCell.Value = com.SelectedItem;
                }
                catch (Exception)
                {
                }
            };

            ScrollEventHandler Scroll = (o, s) =>
            {
                com.Visible = false;
            };

            EventHandler CurrentCellChanged = (object s, EventArgs o) =>
              {
                  if (BoneList.CurrentCell != null)
                  {
                      var selectrow = BoneList.CurrentCell;
                      if (selectrow.ColumnIndex == 2)
                      {
                          com.Left = BoneList.GetCellDisplayRectangle(selectrow.ColumnIndex, selectrow.RowIndex, true).Left;
                          com.Top = BoneList.GetCellDisplayRectangle(selectrow.ColumnIndex, selectrow.RowIndex, true).Top;
                          com.Width = BoneList.GetCellDisplayRectangle(selectrow.ColumnIndex, selectrow.RowIndex, true).Width;
                          com.SelectedItem = BoneList.CurrentCell.Value;
                          com.Visible = true;
                      }
                  }
              };

            ClearList("bone");
            if (MirrorMode.Checked)
            {
                MirrorModeLabel.Text = "镜像创建";
                MirrorBoneCheck.Visible = false;
                MirrorPosionPlusPara.Visible = false;
                MirrorOnlyPara.Visible = false;
                MirrorSelectBoneButton.Text = "镜像创建选中";
                BoneList.Scroll -= Scroll;
                BoneList.CurrentCellChanged -= CurrentCellChanged;
                BoneList.Controls.Clear();
                com.Dispose();
                BoneList.Refresh();
            }
            else
            {
                MirrorModeLabel.Text = "对称匹配";
                MirrorBoneCheck.Visible = true;
                MirrorPosionPlusPara.Visible = true;
                MirrorOnlyPara.Visible = true;
                MirrorSelectBoneButton.Text = "镜像匹配选中";
                BoneList.Scroll += Scroll;
                BoneList.CurrentCellChanged += CurrentCellChanged;
                BoneList.Controls.Add(com);
                new Task(() =>
                {
                    do
                    {
                        var temppmx = ARGS.Host.Connector.Pmx.GetCurrentState();
                        Bonelist.Clear();
                        for (int i = 0; i < temppmx.Bone.Count; i++)
                        {
                            Bonelist.Add(i + ":" + temppmx.Bone[i].Name);
                        }
                        com.DataSource = Bonelist;
                        Thread.Sleep(1000);
                    } while (!MirrorMode.Checked);
                }).Start();
            }
        }

        private void MirrorCharLabel_Click(object sender, EventArgs e)
        {
            var temp = MirrorOriChar.Text;
            MirrorOriChar.Text = MirrorFinChar.Text;
            MirrorFinChar.Text = temp;
        }

        #endregion
        #region 表情操作
        private List<MorphOpera.Morph> MorphBacData = new List<MorphOpera.Morph>();
        private Task MorphMission;
        private List<MorphOpera.Index> MorphSearchList = new List<MorphOpera.Index>();

        private void VertexTab_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (VertexTab.SelectedTab.Text == "T窗口权重调整")
            {
                DataTable table = VertexList.DataSource as DataTable;
                table.Rows.Clear();
                table.Columns.Clear();
                table.Columns.Add("操作");
                table.Columns.Add("骨骼和权重");
                table.Rows.Add();
                table.Rows.Clear();
            }
            else if (VertexTab.SelectedTab.Text == "表情操作")
            {
                DataTable table = VertexList.DataSource as DataTable;
                table.Rows.Clear();
                table.Columns.Clear();
                table.Columns.Add("ID");
                table.Columns.Add("表情名");
                table.Rows.Add();
                table.Rows.Clear();
            }
            else if (VertexTab.SelectedTab.Text == "UV操作")
            {
                DataTable table = VertexList.DataSource as DataTable;
                table.Rows.Clear();
                table.Columns.Clear();
                table.Columns.Add("ID");
                table.Columns.Add("材质");
                table.Rows.Add();
                table.Rows.Clear();
            }
        }

        private void MorphBack_Click(object sender, EventArgs e)
        {
            GetPmx = ARGS.Host.Connector.Pmx.GetCurrentState();
            var morph = new MorphOpera();
            if (GetPmx?.Morph.Count != 0)
            {
                var task = new Task(() =>
                  {
                      MorphBacData = new List<MorphOpera.Morph>();
                      List<int> VerIndex = new List<int>();
                      List<MorphOpera.Index> Verindex = new List<MorphOpera.Index>();
                      var i = 0;
                      new Thread(() =>
                      {
                          MorphMissionTaskBar.Value = 0;
                          MorphMissionTaskBar.Maximum = GetPmx.Morph.Count();
                          do
                          {
                              Thread.Sleep(5);
                              MorphMissionTaskBar.Value = i;
                              MorphBarLabel.Text = i + "->" + MorphMissionTaskBar.Maximum;
                          }
                          while (MorphMissionTaskBar.Value != MorphMissionTaskBar.Maximum);
                          MorphMissionTaskBar.Value = 0;
                          MorphBarLabel.Text = "0->100";
                      }).Start();
                      foreach (var item in GetPmx.Morph)
                      {
                          if (item.IsVertex)
                          {
                              var TempData = new MorphOpera.Morph();
                              TempData.MorphName = item.Name;
                              TempData.Panel = item.Panel;
                              var VertexList = new List<MorphOpera.Morph.Vertex>();
                              foreach (IPXVertexMorphOffset item2 in item.Offsets)
                              {
                                  var TempVertex = new MorphOpera.Morph.Vertex();
                                  TempVertex.index = GetPmx.Vertex.IndexOf(item2.Vertex);
                                  if (Verindex.FindIndex((b) => b.index == TempVertex.index) == -1)
                                  {
                                      var temp = new MorphOpera.Index();
                                      temp.index = TempVertex.index;
                                      temp.x = item2.Vertex.Position.X;
                                      temp.y = item2.Vertex.Position.Y;
                                      temp.z = item2.Vertex.Position.Z;
                                      temp.UVX = item2.Vertex.UV.X;
                                      temp.UVY = item2.Vertex.UV.Y;
                                      temp.NormalX = item2.Vertex.Normal.X;
                                      temp.NormalY = item2.Vertex.Normal.Y;
                                      temp.NormalZ = item2.Vertex.Normal.Z;
                                      Verindex.Add(temp);
                                  }
                                  if (VerIndex.IndexOf(TempVertex.index) == -1)
                                  {
                                      VerIndex.Add(TempVertex.index);
                                  }
                                  TempVertex.tox = item2.Offset.X;
                                  TempVertex.toy = item2.Offset.Y;
                                  TempVertex.toz = item2.Offset.Z;
                                  VertexList.Add(TempVertex);
                              }
                              TempData.VertexList = VertexList.ToArray();
                              MorphBacData.Add(TempData);
                          }
                          Interlocked.Increment(ref i);
                      }
                      morph.MorphList = MorphBacData.ToArray();
                      morph.IndexList = Verindex.ToArray();
                  });
                task.Start();
                using (SaveFileDialog Save = new SaveFileDialog())
                {
                    Save.Title = "备份表情";
                    Save.Filter = "Xml文件(*.Xml)|*.Xml";
                    Save.AddExtension = true;
                    Save.FileName = GetPmx.ModelInfo.ModelName;
                    Save.ShowDialog();
                    if (Save.FileName == "") return;
                    task.ContinueWith((t) =>
                    {
                        try
                        {
                            IFormatter Fileformatter = new BinaryFormatter();
                            Stream Filestream = new FileStream(Save.FileName, FileMode.Create, FileAccess.Write, FileShare.None);
                            Fileformatter.Serialize(Filestream, morph);
                            Filestream.Close();
                            BeginInvoke(new MethodInvoker(() =>
                            {
                                MetroMessageBox.Show(this, "备份完成", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }));
                        }
                        catch (Exception)
                        {
                        }
                    });
                }
            }
        }

        private void MorphSelectAll_Click(object sender, EventArgs e)
        {
            if (VertexList.Rows.Count != 0)
            {
                VertexList.CurrentCell = VertexList.Rows[0].Cells[0];
                VertexList.SelectAll();
            }
        }

        private void LoadMorphBac_Click(object sender, EventArgs e)
        {
            if (MorphMission != null)
            {
                return;
            }
            using (OpenFileDialog open = new OpenFileDialog())
            {
                MorphMissionTaskBar.Value = 0;
                open.Title = "载入表情";
                open.Filter = "Xml文件(*.Xml)|*.Xml";
                open.AddExtension = true;
                open.ShowDialog();
                if (open.FileName == "") return;
                if (new FileInfo(open.FileName).Exists)
                {
                    try
                    {
                        IFormatter formatter = new BinaryFormatter();
                        formatter.Binder = new UBinder();
                        Stream stream = new FileStream(open.FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                        MorphOpera Data = (MorphOpera)formatter.Deserialize(stream);
                        stream.Close();
                        if (Data.MorphList.Length != 0)
                        {
                            MorphBacData = new List<MorphOpera.Morph>(Data.MorphList);
                            DataTable table = VertexList.DataSource as DataTable;
                            table.Rows.Clear();
                            table.Columns.Clear();
                            table.Columns.Add("ID");
                            table.Columns.Add("表情名");
                            table.Rows.Add();
                            table.Rows.Clear();
                            GetPmx = ARGS.Host.Connector.Pmx.GetCurrentState();
                            foreach (var item in MorphBacData)
                            {
                                table.Rows.Add(MorphBacData.IndexOf(item), item.MorphName);
                            }
                            MorphMission = new Task(() =>
                               {
                                   GetPmx = ARGS.Host.Connector.Pmx.GetCurrentState();
                                   IPXPmxBuilder bdx = ARGS.Host.Builder.Pmx;
                                   //  var OriTemp = new ConcurrentBag<MorphOpera.Index>(Data.IndexList);
                                   var OriTemp = new List<MorphOpera.Index>(Data.IndexList);
                                   int i = 0;
                                   new Thread(() =>
                                    {
                                        MorphMissionTaskBar.Value = 0;
                                        MorphMissionTaskBar.Maximum = Data.IndexList.Count();
                                        do
                                        {
                                            Thread.Sleep(5);
                                            MorphMissionTaskBar.Value = i;
                                            MorphBarLabel.Text = i + "->" + Data.IndexList.Count();
                                        }
                                        while (MorphMissionTaskBar.Value != MorphMissionTaskBar.Maximum);
                                        MorphMissionTaskBar.Value = 0;
                                        MorphBarLabel.Text = "0->100";
                                    }).Start();
                                   MorphOpera.Index Last = null;
                                   foreach (var temp in OriTemp)
                                   {
                                       if (MorphPositionWithUV.Checked)
                                       {
                                           var Get = (from item in GetPmx.Vertex
                                                      where item.Position.X == temp.x
                                                      where item.Position.Y == temp.y
                                                      where item.Position.Z == temp.z
                                                      where item.UV.X == temp.UVX
                                                      where item.UV.Y == temp.UVY
                                                      select new
                                                      {
                                                          VertexIndex = GetPmx.Vertex.IndexOf(item),
                                                      });
                                           if (Get == null)
                                           {
                                               if (MorpfSearchClose.Checked)
                                               {
                                                   temp.toindex = (from item in GetPmx.Vertex
                                                                   orderby Math.Sqrt((Math.Pow(item.Position.X - temp.x, 2) + Math.Pow(item.Position.Y - temp.y, 2) + Math.Pow(item.Position.Z - temp.z, 2))) ascending
                                                                   select new
                                                                   {
                                                                       VertexIndex = GetPmx.Vertex.IndexOf(item),
                                                                   }).FirstOrDefault().VertexIndex;
                                               }
                                               else
                                               {
                                                   temp.toindex = -1;
                                               }
                                           }
                                           else
                                           {
                                               if (Get.Count() != 1)
                                               {
                                                   int lastindex = 1000000000;
                                                   if (Last == null)
                                                   {
                                                       var inde_x = OriTemp.IndexOf(temp);
                                                       if (inde_x != 0)
                                                       {
                                                           Last = OriTemp[inde_x - 1];
                                                       }
                                                   }
                                                   foreach (var item in Get)
                                                   {
                                                       if (item.VertexIndex > Last.toindex && item.VertexIndex < lastindex)
                                                       {
                                                           temp.toindex = item.VertexIndex;
                                                           lastindex = item.VertexIndex;
                                                       }
                                                   }
                                               }
                                               else
                                               {
                                                   temp.toindex = Get.FirstOrDefault().VertexIndex;
                                               }
                                           }
                                       }
                                       else
                                       {
                                           var Get = from item in GetPmx.Vertex
                                                     where item.Position.X == temp.x
                                                     where item.Position.Y == temp.y
                                                     where item.Position.Z == temp.z
                                                     where item.Normal.X == temp.NormalX
                                                     where item.Normal.Y == temp.NormalY
                                                     where item.Normal.Z == temp.NormalZ
                                                     select new
                                                     {
                                                         VertexIndex = GetPmx.Vertex.IndexOf(item),
                                                     };
                                           if (Get == null)
                                           {
                                               if (MorpfSearchClose.Checked)
                                               {
                                                   temp.toindex = (from item in GetPmx.Vertex
                                                                   orderby Math.Sqrt((Math.Pow(item.Position.X - temp.x, 2) + Math.Pow(item.Position.Y - temp.y, 2) + Math.Pow(item.Position.Z - temp.z, 2))) ascending
                                                                   select new
                                                                   {
                                                                       VertexIndex = GetPmx.Vertex.IndexOf(item),
                                                                   }).FirstOrDefault().VertexIndex;
                                               }
                                               else
                                               {
                                                   temp.toindex = -1;
                                               }
                                           }
                                           else
                                           {
                                               if (Get.Count() != 1)
                                               {
                                                   int lastindex = 1000000000;
                                                   if (Last == null)
                                                   {
                                                       var inde_x = OriTemp.IndexOf(temp);
                                                       if (inde_x != 0)
                                                       {
                                                           Last = OriTemp[inde_x - 1];
                                                       }
                                                   }
                                                   foreach (var item in Get)
                                                   {
                                                       if (item.VertexIndex > Last.toindex && item.VertexIndex < lastindex)
                                                       {
                                                           temp.toindex = item.VertexIndex;
                                                           lastindex = item.VertexIndex;
                                                       }
                                                   }
                                               }
                                               else
                                               {
                                                   temp.toindex = Get.FirstOrDefault().VertexIndex;
                                               }
                                           }
                                       }
                                       Last = temp;
                                       Interlocked.Increment(ref i);
                                   }
                                   MorphSearchList = OriTemp.ToList();
                                   MorphMission = null;
                               });
                            MorphMission.Start();
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        private void MorphImportSelect_Click(object sender, EventArgs e)
        {
            if (MorphMission == null)
            {
                if (MorphBacData.Count != 0)
                {
                    ThreadPool.QueueUserWorkItem((object state) =>
                    {
                        GetPmx = ARGS.Host.Connector.Pmx.GetCurrentState();
                        for (int i = VertexList.SelectedRows.Count - 1; i >= 0; i--)
                        {
                            var item = MorphBacData[VertexList.Rows.IndexOf(VertexList.SelectedRows[i])];
                            var CreateMorph = ARGS.Host.Builder.Pmx.Morph();
                            CreateMorph.Kind = MorphKind.Vertex;
                            CreateMorph.Name = item.MorphName;
                            CreateMorph.Panel = item.Panel;
                            foreach (var item2 in item.VertexList)
                            {
                                var Index = MorphSearchList.Find((b) => b.index == item2.index).toindex;
                                if (Index != -1) CreateMorph.Offsets.Add(ARGS.Host.Builder.Pmx.VertexMorphOffset(GetPmx.Vertex[Index], new V3(item2.tox, item2.toy, item2.toz)));
                            }
                            GetPmx.Morph.Add(CreateMorph);
                        }

                        BeginInvoke(new MethodInvoker(() =>
                        {
                            ARGS.Host.Connector.Pmx.Update(GetPmx);
                            ARGS.Host.Connector.Form.UpdateList(UpdateObject.Morph);
                            ARGS.Host.Connector.View.PmxView.UpdateModel();
                            ARGS.Host.Connector.View.PmxView.UpdateView();
                        }));
                    });
                }
            }
            else
            {
                MetroMessageBox.Show(this, "请等待后台数据处理完毕", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        #endregion
        #region 骨骼权重置换


        private void ReplaceSelectVertexBone_Click(object sender, EventArgs e)
        {
            var SelectVertex = ARGS.Host.Connector.View.PmxView.GetSelectedVertexIndices();
            if (SelectVertex.Count() != 0)
            {
                IPXPmx ThePmxOfNow = GetPmx;
                if (ThePmxOfNow == null)
                {
                    ThePmxOfNow = ARGS.Host.Connector.Pmx.GetCurrentState();
                }
                if (OriBoneCombox.Text != "" && FinBoneCombo.Text != "")
                {
                    new Task(() =>
                      {
                          var bone1 = ThePmxOfNow.Bone[OriBoneCombox.SelectedIndex];
                          var bone2 = ThePmxOfNow.Bone[FinBoneCombo.SelectedIndex];
                          Parallel.ForEach(SelectVertex, vertex =>
                          {
                              if (ThePmxOfNow.Vertex[vertex].Bone1 == bone1)
                              {
                                  ThePmxOfNow.Vertex[vertex].Bone1 = bone2;
                              }
                              else if (ThePmxOfNow.Vertex[vertex].Bone2 == bone1)
                              {
                                  ThePmxOfNow.Vertex[vertex].Bone2 = bone2;
                              }
                              else if (ThePmxOfNow.Vertex[vertex].Bone3 == bone1)
                              {
                                  ThePmxOfNow.Vertex[vertex].Bone3 = bone2;
                              }
                              else if (ThePmxOfNow.Vertex[vertex].Bone4 == bone1)
                              {
                                  ThePmxOfNow.Vertex[vertex].Bone4 = bone2;
                              }
                          });
                          BeginInvoke(new Action(()=> 
                          {
                              ARGS.Host.Connector.Pmx.Update(ThePmxOfNow);
                              ARGS.Host.Connector.Form.UpdateList(UpdateObject.Vertex);
                              ARGS.Host.Connector.View.PmxView.UpdateModel();
                              ARGS.Host.Connector.View.PmxView.UpdateView();
                              MetroMessageBox.Show(this, "置换完成", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                          }));
                      }).Start();
                }
                else
                {
                    MetroMessageBox.Show(this, "请选择置换前后骨骼后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MetroMessageBox.Show(this, "请先选择顶点后再继续", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        #endregion
    }
}