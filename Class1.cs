using MetroFramework;
using Microsoft.CSharp;
using PEPlugin;
using PEPlugin.Pmd;
using PEPlugin.Pmx;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using static PE多功能信息处理插件.Class2;

namespace PE多功能信息处理插件
{
    public class Program : IPEPlugin, IPEPluginOption, IPEImportPlugin, IPEExportPlugin
    {
        public static IPERunArgs ARGS;

        public string Description => "";
        public string Name => "";

        public IPEPluginOption Option => this;

        public string Version => "0";

        public string Ext => ".pmx";

        public string Caption => "加密的Pmx文件";
        public bool Bootup => true;

        public bool RegisterMenu => false;

        public string RegisterMenuText => "多功能信息处理";

        public void Dispose()
        {
            try
            {
                var sender = ARGS.Host.Connector.Form;
                if ((sender as Form).Text.IndexOf("*", StringComparison.Ordinal) != -1)
                {
                    if (Directory.Exists(new FileInfo(ARGS.Host.Connector.System.HostApplicationPath).DirectoryName +
                                         @"\Backup") ==
                        false) //如果不存在就创建file文件夹
                    {
                        Directory.CreateDirectory(new FileInfo(ARGS.Host.Connector.System.HostApplicationPath)
                                                      .DirectoryName + @"\Backup");
                    }
                    ThreadPool.QueueUserWorkItem(o =>
                    {
                        var SaveFileList =
                            new DirectoryInfo(
                                new FileInfo(ARGS.Host.Connector.System.HostApplicationPath).DirectoryName +
                                @"\Backup").GetFiles().ToList();
                        if (SaveFileList.Count > 50)
                        {
                            SaveFileList.Sort((F, f) => F.CreationTime.CompareTo(f.CreationTime));
                            for (int i = 0; i < SaveFileList.Count-40; i++)
                            {
                                SaveFileList[i].Delete();
                            }
                        }
                    });
                    IPXPmx pmx = ARGS.Host.Connector.Pmx.GetCurrentState();
                    if (pmx.FilePath != "" && pmx.Vertex.Count != 0)
                    {
                        pmx.ToFile(new FileInfo(ARGS.Host.Connector.System.HostApplicationPath).DirectoryName +
                                   @"\Backup\" + new FileInfo(pmx.FilePath).Name);
                    }
                }

                bootstate.OldOpen = ARGS.Host.Connector.Pmx.CurrentPath;
                bootstate.FormTop = Formtemp.TopMost ? 1 : 0;
                ThreadPool.QueueUserWorkItem(Save);
                newopen.Close();
            }
            catch (Exception)
            {
            }
        }

        private readonly ToolStripMenuItem ChineseTooltemp = new ToolStripMenuItem();
        private readonly ToolStripMenuItem KeyTooltemp = new ToolStripMenuItem();
        private readonly ToolStripMenuItem Toolshow = new ToolStripMenuItem();

        private Form Formtemp = null;
        private Form ViewForm = null;
        private string oldformtext = "";
        private IPXPmx PmxTemp;
        private TextBox CustomBoneSearch = null;
        private TextBox CustomBodySearch = null;
        private TextBox CustomJointSearch = null;
        public static TabPage jointpage = null;
        public static TabPage Bonepage = null;
        public static TabPage Bodypage = null;
        public static TextBox BoneSearch = null;
        public static TextBox BodySearch = null;
        public static TextBox JointSearch = null;
        public static List<keySet> KeyData = new List<keySet>();
        public static BootState bootstate;
        public static List<ToolItemInfo> ShortCutInfo = new List<ToolItemInfo>();
        public static ContextMenuStrip contextMaterial = null;
        public static ContextMenuStrip contextBone = null;
        public void Run(IPERunArgs args)
        {
            ARGS = args;
            Formtemp = args.Host.Connector.Form as Form;
            ViewForm = args.Host.Connector.View.PmxView as Form;

            #region 读取配置

            try
            {
                using (Stream stream = new FileStream(new FileInfo(args.Host.Connector.System.HostApplicationPath).DirectoryName + @"\_data\boot.xml", FileMode.OpenOrCreate))
                {
                    IFormatter Formatter = new BinaryFormatter();
                    Formatter.Binder = new UBinder();
                    bootstate = (BootState)Formatter.Deserialize(stream);
                }
            }
            catch (Exception)
            {
                bootstate = new BootState(0, 0, 0) { FormTopmost = 1 };
            }
            finally
            {
                if (bootstate.BezierFirstColor == 0 && bootstate.BezierSecondColor == 0 && bootstate.BezierLinkSize == 0)
                {
                    bootstate.BezierFirstColor = 7;
                    bootstate.BezierSecondColor = 27;
                    bootstate.BezierLinkSize = 3f;
                }
                if (bootstate.WeightAddKey == 0 || bootstate.WeightAppleKey == 0 || bootstate.WeightGetKey == 0 || bootstate.WeightMinusKey == 0)
                {
                    bootstate.WeightAddKey = '+';
                    bootstate.WeightMinusKey = '-';
                    bootstate.WeightAppleKey = '*';
                    bootstate.WeightGetKey = '/';
                }
            }

            #endregion 读取配置

            #region 汉化初始化模块

            /*var ori= new List<FormText>((new XmlSerializer(typeof(FormText[])).Deserialize(new FileStream(new FileInfo(ARGS.Host.Connector.System.HostApplicationPath).DirectoryName + @"\_data\Ori.xml", FileMode.Open)) as FormText[]));
            var Trans = new List<FormText>((new XmlSerializer(typeof(FormText[])).Deserialize(new FileStream(new FileInfo(ARGS.Host.Connector.System.HostApplicationPath).DirectoryName + @"\_data\Trans.xml", FileMode.Open)) as FormText[]));
            List<KeyValuePair<string, string>> Sa = new List<KeyValuePair<string, string>>();
            foreach (var item in ori)
            {
                var temp = Trans.FirstOrDefault(x => x.ControlName == item.ControlName);
                if(temp==null)
                { continue; }
                Sa.Add(new KeyValuePair<string, string> (item.OriText, temp.OriText));
                item.TransText = temp.OriText;
                Trans.Remove(temp);
            }
            using (Stream Filestream = new FileStream(new FileInfo(ARGS.Host.Connector.System.HostApplicationPath).DirectoryName + @"\_data\boot3.xml", FileMode.OpenOrCreate))
            {
                XmlSerializer Ser = new XmlSerializer(typeof(FormText[]));
                Ser.Serialize(Filestream, ori.ToArray());
            }*/

            if (bootstate.openstate == 1)
            {
                new TranslateMod(new List<FormText>((new XmlSerializer(typeof(FormText[])).Deserialize(new MemoryStream(Resource1.zn_CN))) as FormText[]));
            }
            else
            {
                new TranslateMod();
            }

            #endregion 汉化初始化模块

            var StartMission = new Task(() =>
           {
               #region 菜单栏获取

               ShortCutInfo = new List<ToolItemInfo>();
               foreach (ToolStripMenuItem temp1 in Formtemp.MainMenuStrip.Items)
               {
                   foreach (var temp2 in temp1.DropDownItems.Cast<object>().Where(temp2 => temp2.GetType().ToString() == "System.Windows.Forms.ToolStripMenuItem"))
                   {
                       foreach (var temp3 in ((ToolStripMenuItem)temp2).DropDownItems.Cast<object>().Where(temp3 => temp3.GetType().ToString() == "System.Windows.Forms.ToolStripMenuItem"))
                       {
                           foreach (var temp4 in ((ToolStripMenuItem)temp3).DropDownItems.Cast<object>().Where(temp4 => temp4.GetType().ToString() == "System.Windows.Forms.ToolStripMenuItem"))
                           {
                               ShortCutInfo.Add(new ToolItemInfo((ToolStripMenuItem)temp4));
                               getmuch(temp4);
                           }
                           ShortCutInfo.Add(new ToolItemInfo((ToolStripMenuItem)temp3));
                       }
                       ShortCutInfo.Add(new ToolItemInfo((ToolStripMenuItem)temp2));
                   }
                   ShortCutInfo.Add(new ToolItemInfo(temp1));
               }
               foreach (var T1 in ViewForm.MainMenuStrip.Items.OfType<ToolStripMenuItem>().Select(temp1 => temp1))
               {
                   foreach (var temp2 in T1.DropDownItems.Cast<object>().Where(temp2 => temp2.GetType().ToString() == "System.Windows.Forms.ToolStripMenuItem"))
                   {
                       foreach (var temp3 in ((ToolStripMenuItem)temp2).DropDownItems.Cast<object>().Where(temp3 => temp3.GetType().ToString() == "System.Windows.Forms.ToolStripMenuItem"))
                       {
                           foreach (var temp4 in ((ToolStripMenuItem)temp3).DropDownItems.Cast<object>().Where(temp4 => temp4.GetType().ToString() == "System.Windows.Forms.ToolStripMenuItem"))
                           {
                               ShortCutInfo.Add(new ToolItemInfo((ToolStripMenuItem)temp4));
                           }
                           ShortCutInfo.Add(new ToolItemInfo((ToolStripMenuItem)temp3));
                       }
                       ShortCutInfo.Add(new ToolItemInfo((ToolStripMenuItem)temp2));
                   }
                   ShortCutInfo.Add(new ToolItemInfo(T1));
               }

               #endregion 菜单栏获取
           });
            StartMission.ContinueWith(_obj =>
            {
                #region 搜索模块

                var point = new System.Drawing.Point(65, 8);
                var size = new System.Drawing.Size(65, 10);
                if (int.Parse(System.Diagnostics.FileVersionInfo
                        .GetVersionInfo(args.Host.Connector.System.HostApplicationPath).ProductVersion
                        .Replace(".", "")) > 0250)
                {
                    point = new System.Drawing.Point(55, 8);
                    size = new System.Drawing.Size(85, 10);
                }
                KeyPressEventHandler ControlSearch = (s, e) =>
                {
                    bool FinSearch = true;
                    if (e.KeyChar == 13 && FinSearch)
                    {
                        ThreadPool.QueueUserWorkItem(obj =>
                        {
                            try
                            {
                                FinSearch = false;
                                TextBox Page = s as TextBox;
                                var Pmx = ARGS.Host.Connector.Pmx.GetCurrentState();
                                string[] searchchar = new Func<string[]>(() =>
                                {
                                    List<string> temp = new List<string>();
                                    if (Page.Text != null)
                                    {
                                        string tempchar = "";
                                        foreach (string _Temp in Page.Text.Select(x => x.ToString()).ToArray())
                                        {
                                            if (_Temp == @"," || _Temp == @"，" || _Temp == @"." || _Temp == @"。" ||
                                                _Temp == @" " || _Temp == @"　" || _Temp == @";" || _Temp == @"；")
                                            {
                                                if (tempchar != "")
                                                {
                                                    temp.Add(tempchar);
                                                }
                                                tempchar = "";
                                            }
                                            else
                                            {
                                                tempchar = tempchar + _Temp;
                                            }
                                        }
                                        if (tempchar != "")
                                        {
                                            temp.Add(tempchar);
                                        }
                                    }
                                    return temp.ToArray();
                                })();
                                List<int> search = new List<int>();
                                List<int> Tsearch = new List<int>();
                                switch (Page.Name)
                                {
                                    case "BoneSearch":
                                        foreach (var item in searchchar)
                                        {
                                            if (Tsearch.Count == 0)
                                            {
                                                Tsearch.AddRange(from item2 in Pmx.Bone
                                                                 where item2.Name.ToLower()
                                                                           .IndexOf(item.ToLower(),
                                                                               StringComparison.CurrentCulture) != -1
                                                                 select Pmx.Bone.IndexOf(item2));
                                                /* foreach (var item2 in Pmx.Bone)
                                                 {
                                                     if (item2.Name.ToLower().IndexOf(item.ToLower(), StringComparison.CurrentCulture) != -1)
                                                     {
                                                         Tsearch.Add(Pmx.Bone.IndexOf(item2));
                                                     }
                                                 }*/
                                            }
                                            else
                                            {
                                                search = new List<int>(Tsearch
                                                    .Where(item3 => Pmx.Bone[item3].Name.ToLower()
                                                                        .IndexOf(item.ToLower(),
                                                                            StringComparison.CurrentCulture) != -1));
                                                Tsearch = new List<int>(search);
                                            }
                                            if (searchchar.Length == 1)
                                            {
                                                search = new List<int>(Tsearch);
                                            }
                                        }
                                        ViewForm.BeginInvoke(new Action(() => ARGS.Host.Connector.View.PmxView
                                            .SetSelectedBoneIndices(search.ToArray())));
                                        break;

                                    case "BodySearch":
                                        foreach (var item in searchchar)
                                        {
                                            if (Tsearch.Count == 0)
                                            {
                                                Tsearch.AddRange(from item2 in Pmx.Body
                                                                 where item2.Name.ToLower()
                                                                           .IndexOf(item.ToLower(),
                                                                               StringComparison.CurrentCulture) != -1
                                                                 select Pmx.Body.IndexOf(item2));
                                                if (searchchar.Length == 1)
                                                {
                                                    search = new List<int>(Tsearch);
                                                }
                                            }
                                            else
                                            {
                                                search = Tsearch
                                                    .Where(item3 => Pmx.Body[item3].Name.ToLower()
                                                                        .IndexOf(item.ToLower(),
                                                                            StringComparison.CurrentCulture) != -1)
                                                    .ToList();

                                                Tsearch = new List<int>(search);
                                            }
                                        }
                                        ViewForm.BeginInvoke(new Action(() => ARGS.Host.Connector.View.PmxView
                                            .SetSelectedBodyIndices(search.ToArray())));
                                        break;

                                    case "JointSearch":
                                        foreach (var item in searchchar)
                                        {
                                            if (Tsearch.Count == 0)
                                            {
                                                Tsearch.AddRange(from item2 in Pmx.Joint
                                                                 where item2.Name.ToLower()
                                                                           .IndexOf(item.ToLower(),
                                                                               StringComparison.CurrentCulture) != -1
                                                                 select Pmx.Joint.IndexOf(item2));
                                                if (searchchar.Length == 1)
                                                {
                                                    search = new List<int>(Tsearch);
                                                }
                                            }
                                            else
                                            {
                                                search = Tsearch
                                                    .Where(item3 => Pmx.Joint[item3].Name.ToLower()
                                                                        .IndexOf(item.ToLower(),
                                                                            StringComparison.CurrentCulture) != -1)
                                                    .ToList();
                                                Tsearch = new List<int>(search);
                                            }
                                        }
                                        ViewForm.BeginInvoke(new Action(() => ARGS.Host.Connector.View.PmxView
                                            .SetSelectedJointIndices(search.ToArray())));
                                        break;
                                }
                                Formtemp.BeginInvoke(new Action(() =>
                                {
                                    try
                                    {
                                        btnGetObject.PerformClick();
                                        ARGS.Host.Connector.View.PmxView.UpdateView();
                                    }
                                    catch (Exception)
                                    {
                                    }
                                }));
                            }
                            catch (Exception)
                            {
                            }
                            finally
                            {
                                FinSearch = true;
                                GC.Collect();
                            }
                        });
                    }
                };
                EventHandler PageGotFoucus = (s, e) =>
                {
                    TextBox Page = s as TextBox;
                    {
                        if (Page.Text == "搜索")
                        {
                            Page.Text = "";
                        }
                    }
                };
                EventHandler PageLostFoucus = (s, e) =>
                {
                    TextBox Page = s as TextBox;
                    {
                        if (Page.Text == "")
                        {
                            Page.Text = "搜索";
                        }
                    }
                };
                Formtemp.BeginInvoke(new MethodInvoker(() =>
                {
                    var Font = new System.Drawing.Font("微软雅黑", 7.5f);
                    {
                        CustomBoneSearch = new TextBox {Name = "BoneSearch"};
                        CustomBoneSearch.KeyPress += ControlSearch;
                        CustomBoneSearch.GotFocus += PageGotFoucus;
                        CustomBoneSearch.LostFocus += PageLostFoucus;
                        CustomBoneSearch.Text = "搜索";
                        CustomBoneSearch.Location = point;
                        CustomBoneSearch.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
                        CustomBoneSearch.Size = size;
                        CustomBoneSearch.Visible = false;
                        CustomBoneSearch.Font = Font;
                        Bonepage.Controls.Add(CustomBoneSearch);
                        CustomBoneSearch.BringToFront();
                    }
                    {
                        CustomBodySearch = new TextBox {Name = "BodySearch"};
                        CustomBodySearch.KeyPress += ControlSearch;
                        CustomBodySearch.GotFocus += PageGotFoucus;
                        CustomBodySearch.LostFocus += PageLostFoucus;
                        CustomBodySearch.Text = "搜索";
                        CustomBodySearch.Location = point;
                        CustomBodySearch.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
                        CustomBodySearch.Size = size;
                        CustomBodySearch.Visible = false;
                        CustomBodySearch.Font = Font;
                        Bodypage.Controls.Add(CustomBodySearch);
                        CustomBodySearch.BringToFront();
                    }
                    {
                        CustomJointSearch = new TextBox {Name = "JointSearch"};
                        CustomJointSearch.KeyPress += ControlSearch;
                        CustomJointSearch.GotFocus += PageGotFoucus;
                        CustomJointSearch.LostFocus += PageLostFoucus;
                        CustomJointSearch.Text = "搜索";
                        CustomJointSearch.Location = point;
                        CustomJointSearch.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
                        CustomJointSearch.Size = size;
                        CustomJointSearch.Visible = false;
                        CustomJointSearch.Font = Font;
                        jointpage.Controls.Add(CustomJointSearch);
                        CustomJointSearch.BringToFront();
                    }
                }));
                //OriFormInfo = new List<Info>(OriFormInfo.GroupBy(a => a.FormName).Select(g => g.FirstOrDefault()));
                //Linq表达式意义，根据FormName进行分组，并选取每组的第一项;

                #endregion 搜索模块

                #region 一键汉化

                Action<bool> ControlCheck = Check =>
                {
                    if (Check)
                    {
                        if (BoneSearch != null)
                        {
                            BoneSearch.Enabled = true;
                            BoneSearch.Visible = true;
                            BodySearch.Enabled = true;
                            BodySearch.Visible = true;
                            JointSearch.Enabled = true;
                            JointSearch.Visible = true;
                        }
                        CustomBoneSearch.Enabled = false;
                        CustomBoneSearch.Visible = false;

                        CustomBodySearch.Enabled = false;
                        CustomBodySearch.Visible = false;

                        CustomJointSearch.Enabled = false;
                        CustomJointSearch.Visible = false;
                    }
                    else
                    {
                        if (BoneSearch != null)
                        {
                            BoneSearch.Enabled = false;
                            BoneSearch.Visible = false;
                            BodySearch.Enabled = false;
                            BodySearch.Visible = false;
                            JointSearch.Enabled = false;
                            JointSearch.Visible = false;
                        }
                        CustomBoneSearch.Enabled = true;
                        CustomBoneSearch.Visible = true;

                        CustomBodySearch.Enabled = true;
                        CustomBodySearch.Visible = true;

                        CustomJointSearch.Enabled = true;
                        CustomJointSearch.Visible = true;
                    }
                };
                Formtemp.BeginInvoke(new MethodInvoker(() =>
                {
                    if (bootstate.openstate == 1)
                    {
                        ChineseTooltemp.Name = "汉化";
                        ChineseTooltemp.Text = "已汉化";
                        ChineseTooltemp.Font = Formtemp.MainMenuStrip.Font;
                        Formtemp.MainMenuStrip.Items.Add(ChineseTooltemp);
                        ControlCheck(false);
                    }
                    else
                    {
                        ChineseTooltemp.Font = Formtemp.MainMenuStrip.Font;
                        Formtemp.MainMenuStrip.Items.Add(ChineseTooltemp);
                        ChineseTooltemp.Text = "点击汉化";
                        ControlCheck(true);
                    }
                }));
                ChineseTooltemp.Click += (sender, e) =>
                {
                    if (ChineseTooltemp.Text == "已汉化")
                    {
                        new TranslateMod(
                            new List<FormText>(
                                (new XmlSerializer(typeof(FormText[])).Deserialize(new MemoryStream(Resource1.zn_CN)))
                                as FormText[]), false);
                        bootstate.openstate = 0;
                        ChineseTooltemp.Text = "点击汉化";
                        ControlCheck(true);
                        ThreadPool.QueueUserWorkItem(Save);
                    }
                    else
                    {
                        new TranslateMod(new List<FormText>(
                            (new XmlSerializer(typeof(FormText[])).Deserialize(new MemoryStream(Resource1.zn_CN))) as
                            FormText[]));
                        bootstate.openstate = 1;
                        ChineseTooltemp.Text = "已汉化";
                        ControlCheck(false);
                        ThreadPool.QueueUserWorkItem(Save);
                    }
                };

                #endregion 一键汉化

                #region 获取每次打开模型的路径

                Formtemp.TextChanged += (sender, e) =>
                {
                    if ((sender as Form).Text != "" && (sender as Form).Text != oldformtext &&
                        (sender as Form).Text != oldformtext + " *")
                    {
                        oldformtext = (sender as Form).Text;
                        PmxTemp = ARGS.Host.Connector.Pmx.GetCurrentState();
                        ThreadPool.QueueUserWorkItem(state =>
                        {
                            if (PmxTemp.FilePath != "")
                            {
                                List<OpenHis> HisOpen = new List<OpenHis>();
                                if (bootstate.HisOpen != null)
                                {
                                    HisOpen.AddRange(bootstate.HisOpen);
                                }
                                foreach (var temp in HisOpen.Where(temp => temp.modelpath == PmxTemp.FilePath))
                                {
                                    HisOpen.Remove(temp);
                                    break;
                                }
                                HisOpen.Add(new OpenHis(PmxTemp.ModelInfo.ModelName, PmxTemp.FilePath,
                                    DateTime.Now.ToLocalTime().ToString()));
                                bootstate.HisOpen = new OpenHis[HisOpen.Count];
                                HisOpen.CopyTo(bootstate.HisOpen);
                                ThreadPool.QueueUserWorkItem(Save);
                            }
                        });
                    }
                };

                #endregion 获取每次打开模型的路径

                #region UV复制模块

                {
                    var Add = new ToolStripMenuItem {Text = "合并选中材质的UV到"};
                    EventHandler ToolClick = (o, s) =>
                    {
                        var Material = args.Host.Connector.Form.GetSelectedMaterialIndices();
                        if (Material.Length == 2)
                        {
                            var model = args.Host.Connector.Pmx.GetCurrentState();
                            var Face1 = model.Material[Material[0]].Faces;
                            var Face2 = model.Material[Material[1]].Faces;
                            if (Face1.Count == Face2.Count)
                            {
                                switch (o.ToString())
                                {
                                    case "UV1":
                                    {
                                        for (int i = 0; i < Face1.Count; i++)
                                        {
                                            model.Material[Material[0]].Faces[i].Vertex1.UVA1 =
                                                new PEPlugin.SDX.V4(model.Material[Material[1]].Faces[i].Vertex1.UV.U,
                                                    model.Material[Material[1]].Faces[i].Vertex1.UV.V, 0, 0);
                                            model.Material[Material[0]].Faces[i].Vertex2.UVA1 =
                                                new PEPlugin.SDX.V4(model.Material[Material[1]].Faces[i].Vertex2.UV.U,
                                                    model.Material[Material[1]].Faces[i].Vertex2.UV.V, 0, 0);
                                            model.Material[Material[0]].Faces[i].Vertex3.UVA1 =
                                                new PEPlugin.SDX.V4(model.Material[Material[1]].Faces[i].Vertex3.UV.U,
                                                    model.Material[Material[1]].Faces[i].Vertex3.UV.V, 0, 0);
                                        }
                                    }
                                        break;

                                    case "UV2":
                                    {
                                        for (int i = 0; i < Face1.Count; i++)
                                        {
                                            model.Material[Material[0]].Faces[i].Vertex1.UVA2 =
                                                new PEPlugin.SDX.V4(model.Material[Material[1]].Faces[i].Vertex1.UV.U,
                                                    model.Material[Material[1]].Faces[i].Vertex1.UV.V, 0, 0);
                                            model.Material[Material[0]].Faces[i].Vertex2.UVA2 =
                                                new PEPlugin.SDX.V4(model.Material[Material[1]].Faces[i].Vertex2.UV.U,
                                                    model.Material[Material[1]].Faces[i].Vertex2.UV.V, 0, 0);
                                            model.Material[Material[0]].Faces[i].Vertex3.UVA2 =
                                                new PEPlugin.SDX.V4(model.Material[Material[1]].Faces[i].Vertex3.UV.U,
                                                    model.Material[Material[1]].Faces[i].Vertex3.UV.V, 0, 0);
                                        }
                                    }
                                        break;

                                    case "UV3":
                                    {
                                        for (int i = 0; i < Face1.Count; i++)
                                        {
                                            model.Material[Material[0]].Faces[i].Vertex1.UVA3 =
                                                new PEPlugin.SDX.V4(model.Material[Material[1]].Faces[i].Vertex1.UV.U,
                                                    model.Material[Material[1]].Faces[i].Vertex1.UV.V, 0, 0);
                                            model.Material[Material[0]].Faces[i].Vertex2.UVA3 =
                                                new PEPlugin.SDX.V4(model.Material[Material[1]].Faces[i].Vertex2.UV.U,
                                                    model.Material[Material[1]].Faces[i].Vertex2.UV.V, 0, 0);
                                            model.Material[Material[0]].Faces[i].Vertex3.UVA3 =
                                                new PEPlugin.SDX.V4(model.Material[Material[1]].Faces[i].Vertex3.UV.U,
                                                    model.Material[Material[1]].Faces[i].Vertex3.UV.V, 0, 0);
                                        }
                                    }
                                        break;

                                    case "UV4":
                                    {
                                        for (int i = 0; i < Face1.Count; i++)
                                        {
                                            model.Material[Material[0]].Faces[i].Vertex1.UVA4 =
                                                new PEPlugin.SDX.V4(model.Material[Material[1]].Faces[i].Vertex1.UV.U,
                                                    model.Material[Material[1]].Faces[i].Vertex1.UV.V, 0, 0);
                                            model.Material[Material[0]].Faces[i].Vertex2.UVA4 =
                                                new PEPlugin.SDX.V4(model.Material[Material[1]].Faces[i].Vertex2.UV.U,
                                                    model.Material[Material[1]].Faces[i].Vertex2.UV.V, 0, 0);
                                            model.Material[Material[0]].Faces[i].Vertex3.UVA4 =
                                                new PEPlugin.SDX.V4(model.Material[Material[1]].Faces[i].Vertex3.UV.U,
                                                    model.Material[Material[1]].Faces[i].Vertex3.UV.V, 0, 0);
                                        }
                                    }
                                        break;
                                }
                                Formtemp.BeginInvoke(new Action(() =>
                                {
                                    ARGS.Host.Connector.Pmx.Update(model);
                                    ARGS.Host.Connector.Form.UpdateList(UpdateObject.Vertex);
                                    ARGS.Host.Connector.View.PmxView.UpdateModel();
                                    ARGS.Host.Connector.View.PmxView.UpdateView();
                                }));
                                MetroMessageBox.Show(Formtemp, "复制完成", "", MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                            }
                            else
                            {
                                MetroMessageBox.Show(Formtemp, "选中的两个面并不相同", "", MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                            }
                        }
                        else
                        {
                            MetroMessageBox.Show(Formtemp, "请选择2个材质", "", MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        }
                    };
                    Add.DropDownItems.Add("UV1", null, ToolClick);
                    Add.DropDownItems.Add("UV2", null, ToolClick);
                    Add.DropDownItems.Add("UV3", null, ToolClick);
                    Add.DropDownItems.Add("UV4", null, ToolClick);
                    Formtemp.BeginInvoke(new Action(() =>
                    {
                        List<ToolStripItem> SaveToolStrip = contextMaterial.Items.Cast<ToolStripItem>().ToList();
                        contextMaterial.Items.Clear();
                        for (int i = 0; i < SaveToolStrip.Count; i++)
                        {
                            contextMaterial.Items.Add(SaveToolStrip[i]);
                            if (i == 0)
                            {
                                contextMaterial.Items.Add(Add);
                            }
                        }
                    }));
                }

                #endregion UV复制模块

                #region 骨骼名称置换

                {
                    var Add = new ToolStripMenuItem {Text = "改变日英名称"};
                    Func<Dictionary<string, string>> GetDic = delegate
                    {
                        var Dic = new Dictionary<string, string>();
                        var path = new FileInfo(ARGS.Host.Connector.System.HostApplicationPath).DirectoryName +
                                   @"\_data\和英変換.txt";

                        if (!File.Exists(path))
                        {
                            if (File.Exists(new FileInfo(ARGS.Host.Connector.System.HostApplicationPath)
                                                .DirectoryName +
                                            @"\_data\榓塸曄姺.txt"))
                            {
                                File.Copy(new FileInfo(ARGS.Host.Connector.System.HostApplicationPath)
                                              .DirectoryName +
                                          @"\_data\榓塸曄姺.txt", path);
                            }
                        }
                        using (var FileDic = new StreamReader(new FileStream(path, FileMode.OpenOrCreate),
                            Encoding.GetEncoding(932), false))
                        {
                            while (FileDic.Peek() != -1)
                            {
                                var TempString = Regex.Split(FileDic.ReadLine(), ", ", RegexOptions.Compiled);
                                if (!Dic.ContainsKey(TempString[1]))
                                {
                                    Dic.Add(TempString[1], TempString[0]);
                                }
                            }
                        }
                        return Dic;
                    };
                    EventHandler ToolClick = (o, s) =>
                    {
                        var BoneList = args.Host.Connector.Pmx.GetCurrentState();
                        var BoneCount = args.Host.Connector.View.PmxView.GetSelectedBoneIndices();
                        var TempDic = GetDic();
                        switch (o.ToString())
                        {

                            case "改变日语名":

                                foreach (var VARIABLE in BoneCount)
                                {
                                    if (TempDic.TryGetValue(BoneList.Bone[VARIABLE].Name, out string TempString))
                                    {
                                        BoneList.Bone[VARIABLE].Name = TempString;
                                    }
                                }
                                break;
                            case "改变英语名":
                                var DicEn = new Dictionary<string, string>();
                                foreach (var VARIABLE in TempDic.Where(VARIABLE => !DicEn.ContainsKey(VARIABLE.Value)))
                                {
                                    DicEn.Add(VARIABLE.Value, VARIABLE.Key);
                                }
                                foreach (var VARIABLE in BoneCount.Where(
                                    VARIABLE => DicEn.ContainsKey(BoneList.Bone[VARIABLE].Name)))
                                {
                                    BoneList.Bone[VARIABLE].NameE = DicEn[BoneList.Bone[VARIABLE].Name];
                                }
                                break;
                            case "复制日语名给英语名":
                                foreach (var Tbone in BoneCount)
                                {
                                    BoneList.Bone[Tbone].NameE = BoneList.Bone[Tbone].Name;
                                }
                                break;
                            case "保存到词典":
                                using (var FileDic =
                                    new StreamWriter(
                                        new FileStream(new FileInfo(ARGS.Host.Connector.System.HostApplicationPath)
                                                           .DirectoryName +
                                                       @"\_data\和英変換.txt", FileMode.Create),
                                        Encoding.GetEncoding(932)))
                                {
                                    foreach (var VARIABLE in BoneCount)
                                    {
                                        if (TempDic.ContainsKey(BoneList.Bone[VARIABLE].NameE))
                                        {
                                            TempDic[BoneList.Bone[VARIABLE].NameE] = BoneList.Bone[VARIABLE].Name;
                                        }
                                        else
                                        {
                                            TempDic.Add(BoneList.Bone[VARIABLE].NameE, BoneList.Bone[VARIABLE].Name);
                                        }
                                    }
                                    foreach (var Temp in TempDic)
                                    {
                                        FileDic.WriteLine(Temp.Value + ", " + Temp.Key);
                                    }
                                }
                                break;
                        }
                        Formtemp.BeginInvoke(new Action(() =>
                        {
                            ARGS.Host.Connector.Pmx.Update(BoneList);
                            ARGS.Host.Connector.Form.UpdateList(UpdateObject.Bone);
                        }));
                    };
                    Add.DropDownItems.Add("改变日语名", null, ToolClick);
                    Add.DropDownItems.Add("改变英语名", null, ToolClick);
                    Add.DropDownItems.Add("复制日语名给英语名", null, ToolClick);
                    Add.DropDownItems.Add("保存到词典", null, ToolClick);
                    Formtemp.BeginInvoke(new Action(() =>
                    {
                        List<ToolStripItem> SaveToolStrip = contextBone.Items.Cast<ToolStripItem>().ToList();
                        contextBone.Items.Clear();
                        for (int i = 0; i < SaveToolStrip.Count; i++)
                        {
                            contextBone.Items.Add(SaveToolStrip[i]);
                            if (i == 0)
                            {
                                contextBone.Items.Add(Add);
                            }
                        }
                    }));
                }

                #endregion
                new Thread(() =>
                {
                    #region 快捷键

                    if (((System.ComponentModel.EventHandlerList)(typeof(Form)).GetProperty("Events", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Formtemp, null))[(typeof(Control)).GetField("EventKeyDown", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null)] == null || ((System.ComponentModel.EventHandlerList)(typeof(Form)).GetProperty("Events", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(ViewForm, null))[(typeof(Control)).GetField("EventKeyDown", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null)] == null)
                    {
                        ViewForm.KeyDown += (obj, e) =>
                        {
                            if (bootstate.Keystate == 1)
                            {
                                string IntPutKey;
                                string[] stringtemp = e.KeyData.ToString().Split(',');
                                try
                                {
                                    stringtemp[1] = stringtemp[1].Replace(" ", string.Empty);
                                }
                                catch (Exception) { }
                                if (stringtemp.Length > 1)
                                {
                                    if (e.KeyData.ToString().IndexOf("Control", StringComparison.Ordinal) > -1) { stringtemp[1] = "CTRL"; }
                                    switch (stringtemp[0])
                                    {
                                        case "ShiftKey":
                                            IntPutKey = stringtemp[1] + "+";
                                            break;

                                        case "ControlKey":
                                            IntPutKey = "CTRL" + "+";
                                            break;

                                        case "Menu":
                                            IntPutKey = stringtemp[1] + "+";
                                            break;

                                        default:
                                            IntPutKey = stringtemp[1] + "+" + stringtemp[0];
                                            break;
                                    }
                                }
                                else
                                {
                                    IntPutKey = e.KeyData.ToString();
                                }
                                var key = (from tempset in KeyData
                                           where tempset.itemKey == IntPutKey
                                           select tempset).FirstOrDefault();
                                if (key == null) return;
                                if (key.item == null)
                                {
                                    key.item = (from ToolTemp in ShortCutInfo
                                                where ToolTemp.Item.Name == key.itemFun
                                                select ToolTemp).FirstOrDefault();
                                }
                                key.item.Item.PerformClick();
                            }
                        };
                    }
                    KeyTooltemp.Click += delegate
                    {
                        if (KeyTooltemp.Text == "快捷键启用中")
                        {
                            bootstate.Keystate = 0;
                            KeyTooltemp.Text = "快捷键禁用中";
                            ThreadPool.QueueUserWorkItem(Save);
                        }
                        else
                        {
                            if ((new FileInfo(new FileInfo(ARGS.Host.Connector.System.HostApplicationPath).DirectoryName + @"\_data\KeyData.XML").Exists))
                            {
                                try
                                {
                                    IFormatter formatter = new BinaryFormatter();
                                    formatter.Binder = new UBinder();
                                    Stream stream = new FileStream(new FileInfo(ARGS.Host.Connector.System.HostApplicationPath).DirectoryName + @"\_data\KeyData.XML", FileMode.Open, FileAccess.Read, FileShare.Read);
                                    keySet[] KeyTemp = (keySet[])formatter.Deserialize(stream);
                                    KeyData.Clear();
                                    KeyData.AddRange(KeyTemp);
                                    stream.Close();
                                }
                                catch (Exception)
                                {
                                    bootstate.Keystate = 0;
                                    KeyTooltemp.Text = "快捷键禁用中";
                                    ThreadPool.QueueUserWorkItem(Save);
                                }
                            }
                            else
                            {
                                bootstate.Keystate = 0;
                                KeyTooltemp.Text = "快捷键禁用中";
                                ThreadPool.QueueUserWorkItem(Save);
                            }

                            {
                                bootstate.Keystate = 1;
                                KeyTooltemp.Text = "快捷键启用中";
                                ThreadPool.QueueUserWorkItem(Save);
                            }
                        }
                    };
                    if ((new FileInfo(new FileInfo(ARGS.Host.Connector.System.HostApplicationPath).DirectoryName + @"\_data\KeyData.XML").Exists))
                    {
                        IFormatter formatter = new BinaryFormatter();
                        formatter.Binder = new UBinder();
                        Stream stream = new FileStream(new FileInfo(ARGS.Host.Connector.System.HostApplicationPath).DirectoryName + @"\_data\KeyData.XML", FileMode.Open, FileAccess.Read, FileShare.Read);
                        keySet[] KeyTemp = (keySet[])formatter.Deserialize(stream);
                        stream.Close();
                        KeyData.AddRange(KeyTemp);
                        if (bootstate.Keystate == 1)//是否自动启动快捷键
                        {
                            KeyTooltemp.Name = "快捷键";
                            KeyTooltemp.Text = "快捷键启用中";
                            KeyTooltemp.Font = Formtemp.MainMenuStrip.Font;
                            Formtemp.MainMenuStrip.Items.Add(KeyTooltemp);
                        }
                        else
                        {
                            KeyTooltemp.Text = "快捷键禁用中";
                            KeyTooltemp.Font = Formtemp.MainMenuStrip.Font;
                            Formtemp.MainMenuStrip.Items.Add(KeyTooltemp);
                        }
                    }
                    else
                    {
                        bootstate.Keystate = 0;
                        KeyTooltemp.Text = "快捷键禁用中";
                        KeyTooltemp.Font = Formtemp.MainMenuStrip.Font;
                        Formtemp.MainMenuStrip.Items.Add(KeyTooltemp);
                    }

                    #endregion 快捷键

                    #region 主窗口显示

                    Toolshow.Click += delegate
                    {
                        if (newopen == null)
                        {
                            /*Application.ThreadException += delegate
                             {
                                 Console.WriteLine();
                             };
                            AppDomain.CurrentDomain.UnhandledException += delegate
                            {
                                Console.WriteLine();
                            };
                            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                            Application.EnableVisualStyles();*/
                            newopen = new Metroform();
                            newopen.Show();
                        }
                        else
                        {
                            newopen.WindowState = FormWindowState.Normal;
                            newopen.BringToFront();
                        }
                    };
                    Toolshow.Name = "多功能插件";
                    Toolshow.Text = "显示多功能插件";
                    Toolshow.Font = Formtemp.MainMenuStrip.Font;
                    Formtemp.MainMenuStrip.Items.Add(Toolshow);

                    #endregion 主窗口显示

                    #region 自动启动模型

                    try
                    {
                        if (bootstate.AutoOpen == 1 && bootstate.OldOpen != "")
                        {
                            Formtemp.BeginInvoke(new Action(() => ARGS.Host.Connector.Form.OpenPMXFile(bootstate.OldOpen)));
                        }
                    }
                    catch (Exception)
                    {
                        bootstate.OldOpen = "";
                    }

                    #endregion 自动启动模型

                    #region PE主窗口是否前置

                    if (bootstate.FormTop == 1)
                    {
                        Formtemp.BeginInvoke(new Action(() => FormTopMost.PerformClick()));
                    }

                    #endregion PE主窗口是否前置
                }).Start();
            });
            StartMission.Start();

            #region 未来功能

            /* int codePage = Encoding.Default.CodePage;
            if (codePage == 932)
            {
                  Class2 .Language = new ResourceManager("MmdModelManage.ja-JP", typeof(ja_JP).Assembly);
            }
            else if (codePage == 936)
            {
                Language = new ResourceManager("PE多功能信息处理插件.zh-CN", typeof(zh_CN).Assembly);
            }
            else
            {
                  Class2.Language = new ResourceManager("MmdModelManage.en-US", typeof(en_US).Assembly);
            }
            seletepath.Description = Language.GetString("seletepathDescription");*/

            #endregion 未来功能
        }

        private void getmuch(object temp4)
        {
            if (temp4.GetType().ToString() == "System.Windows.Forms.ToolStripMenuItem")
            {
                foreach (var temp in ((ToolStripMenuItem)temp4).DropDownItems.Cast<object>().Where(temp => temp.GetType().ToString() == "System.Windows.Forms.ToolStripMenuItem"))
                {
                    ShortCutInfo.Add(new ToolItemInfo((ToolStripMenuItem)temp));
                    getmuch(temp);
                }
            }
        }

        public static void Save(object obj)
        {
            try
            {
                using (Stream Filestream = new FileStream(new FileInfo(ARGS.Host.Connector.System.HostApplicationPath).DirectoryName + @"\_data\boot.xml", FileMode.OpenOrCreate))
                {
                    IFormatter Fileformatter = new BinaryFormatter();
                    Fileformatter.Serialize(Filestream, bootstate);
                    Filestream.Close();
                }
            }
            catch (Exception)
            {
            }
        }

        public IPXPmx Import(string path, IPERunArgs args)
        {
            var pmx = PEStaticBuilder.Pmx.Pmx();
            if (new FileInfo(path).Exists)
            {
                using (var filestream = new FileStream(path, FileMode.Open))
                {
                    BinaryReader readstream = new BinaryReader(filestream);
                    readstream.ReadBytes(3);
                    switch (readstream.ReadByte())
                    {
                        case 32:
                            filestream.Position = 0;
                            pmx.FromStream(filestream);
                            break;

                        case 64:
                            using (var decompressedFileStream = new MemoryStream())
                            {
                                using (
                                    var decompressionStream = new DeflateStream(new MemoryStream(Resource1.program),
                                        CompressionMode.Decompress))
                                {
                                    decompressionStream.CopyTo(decompressedFileStream);
                                    CompilerResults Coderes =
                                new CSharpCodeProvider().CreateCompiler()
                                    .CompileAssemblyFromSource(
                                        new CompilerParameters(new[]
                                        {"System.dll", "System.Core.dll", "System.Windows.Forms.dll", "System.Data.dll"}),
                                       Encoding.UTF8.GetString(decompressedFileStream.ToArray()));
                                    if (!Coderes.Errors.HasErrors)
                                    {
                                        List<object> ImpotrPara = new List<object>();
                                        var open = new Form2();
                                        open.ShowDialog();
                                        ImpotrPara.Add(filestream);
                                        ImpotrPara.Add(open.Password);
                                        filestream.Position = 0;
                                        try
                                        {
                                            pmx.FromStream((MemoryStream)
                                                                       Coderes.CompiledAssembly.CreateInstance("PmxRead.Program")
                                                                           .GetType()
                                                                           .GetMethod("Import")
                                                                           .Invoke(Coderes.CompiledAssembly.CreateInstance("PmxRead.Program"),
                                                                               new object[] { ImpotrPara }));
                                        }
                                        catch (Exception)
                                        {
                                            MetroMessageBox.Show((args.Host.Connector.Form as Form), "密码错误，无法加载", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        }
                                    }
                                }
                            }
                            break;

                        default:
                            filestream.Position = 0;
                            BinaryWriter writerstream = new BinaryWriter(filestream);
                            writerstream.Seek(3, 0);
                            writerstream.Write(32);
                            filestream.Position = 0;
                            pmx.FromStream(filestream);
                            break;
                    }
                    filestream.Close();
                    filestream.Dispose();
                }
            }
            return pmx;
        }

        public void Export(IPXPmx pmx, string path, IPERunArgs args)
        {
            using (var decompressedFileStream = new MemoryStream())
            {
                using (
                    var decompressionStream = new DeflateStream(new MemoryStream(Resource1.program),
                        CompressionMode.Decompress))
                {
                    decompressionStream.CopyTo(decompressedFileStream);
                    var open = new Form2();
                    open.ShowDialog();
                    if (open.Password == "")
                    {
                        return;
                    }
                    var savefile = new FileStream(path, FileMode.OpenOrCreate);
                    MemoryStream savestream = new MemoryStream();
                    pmx.ToStream(savestream);
                    CompilerResults Coderes =
                        new CSharpCodeProvider().CreateCompiler()
                            .CompileAssemblyFromSource(
                                new CompilerParameters(new[]
                                {"System.dll", "System.Core.dll", "System.Windows.Forms.dll", "System.Data.dll"}),
                               Encoding.UTF8.GetString(decompressedFileStream.ToArray()));
                    if (!Coderes.Errors.HasErrors)
                    {
                        List<object> ImpotrPara = new List<object> { savestream, open.Password };
                        open.Close();
                        open.Dispose();
                        MemoryStream savedata = (MemoryStream)
                            Coderes.CompiledAssembly.CreateInstance("PmxRead.Program")
                                .GetType()
                                .GetMethod("Export")
                                .Invoke(Coderes.CompiledAssembly.CreateInstance("PmxRead.Program"), new object[] { ImpotrPara });
                        savefile.Write(savedata.ToArray(), 0, (int)savedata.Length);
                        savefile.Close();
                        savefile.Dispose();
                    }
                }
            }
        }
    }
}