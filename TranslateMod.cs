using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using static PE多功能信息处理插件.Class2;
using static PE多功能信息处理插件.Program;

namespace PE多功能信息处理插件
{
    internal class TranslateMod
    {
        private readonly bool Read = false;
        private readonly bool ReadW = true;
        private readonly bool Write = false;
        public List<FormText> Readinfo = new List<FormText>();
        private readonly List<FormText> Writeinfo = new List<FormText>();
        private readonly Regex regex = new Regex(@"^[A-Za-z0-9.: -]+$", RegexOptions.Compiled);

        public TranslateMod()
        {
            Write = false;
            SwichControl(ARGS.Host.Connector.Form as Form, 0);
            /* using (Stream Filestream = new FileStream(new FileInfo(ARGS.Host.Connector.System.HostApplicationPath).DirectoryName + @"\_data\boot3.xml", FileMode.OpenOrCreate))
             {
                 XmlSerializer Ser = new XmlSerializer(typeof(FormText[]));
                 Ser.Serialize(Filestream, Readinfo.ToArray());
             }*/
        }

        public TranslateMod(List<FormText> Writeinfo, bool Read = true)
        {
            Write = true;
            ReadW = Read;
            this.Writeinfo = new List<FormText>(Writeinfo);
            SwichControl(ARGS.Host.Connector.Form as Form, 0);
        }

        public void SwichControl(dynamic Obj, int count)
        {
            if (Obj is Form)
            {
                if (count < 4)
                {
                    Interlocked.Increment(ref count);
                    GetOrChangeForm(Obj as Form, count);
                }
            }
            else if (Obj is Control)
            {
                GetOrChangeControl(Obj as Control, count);
            }
            else if (Obj is ToolStripItem)
            {
                GetOrChangeToolStripItem(Obj as ToolStripItem, count);
            }
        }

        private void GetOrChangeToolStripItem(ToolStripItem toolStripItem, int count)
        {
            if (toolStripItem is ToolStripComboBox)
            {
                var ComboBox = toolStripItem as ToolStripComboBox;
                if (Read)
                {
                    //var StringBuild = new StringBuilder();
                    foreach (var item in ComboBox.Items)
                    {
                        ReadAdd(item.ToString(), ComboBox.Name);
                    }
                }
                if (Write)
                {
                    /* var tempinfo = Writeinfo.FirstOrDefault(x => x.ID == ComboBox.Name && x.Parent == (ComboBox.Owner != null ? ComboBox.Owner.Name : "null"));
                     if (tempinfo != null)
                     {
                         var StringSplit = tempinfo.text.Split('|');
                         for (int i = 0; i < ComboBox.Items.Count; i++)
                         {
                             ComboBox.Items[i] = StringSplit[i];
                         }
                     }*/
                    for (int i = 0; i < ComboBox.Items.Count; i++)
                    {
                        var temp = WriteAdd(ComboBox.Items[i].ToString(), ComboBox.Name);
                        if (temp != null)
                        {
                            ComboBox.Items[i] = temp;
                        }
                    }
                }
            }
            else if (toolStripItem.Text != "0" && !string.IsNullOrWhiteSpace(toolStripItem.Text))
            {
                if (toolStripItem.Name == "MenuItem_TopMost" && FormTopMost == null)
                {
                    FormTopMost = toolStripItem;
                }
                if (Read)
                {
                    ReadAdd(toolStripItem.Text, toolStripItem.Name);
                    ReadAdd(toolStripItem.ToolTipText, toolStripItem.Name);
                }
                if (Write)
                {
                    toolStripItem.Text = WriteAdd(toolStripItem.Text, toolStripItem.Name);
                    toolStripItem.ToolTipText = WriteAdd(toolStripItem.ToolTipText, toolStripItem.Name);
                    /*  if (Writeinfo != null)
                      {
                          var tempinfo = Writeinfo.FirstOrDefault(x => x.ID == toolStripItem.Name && x.Parent == (toolStripItem.Owner != null ? toolStripItem.Owner.Name : "null"));
                          if (tempinfo != null)
                          {
                              toolStripItem.Text = tempinfo.text;
                              toolStripItem.ToolTipText = tempinfo.ToolTipText;
                          }
                      }*/
                }
            }
            foreach (var obj in toolStripItem.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Select(fi => fi.GetValue(toolStripItem)))
            {
                SwichControl(obj, count);
            }
        }

        private string WriteAdd(string v, string v2)
        {
            if (CheckBox(v))
            {
                if (ReadW)
                {
                    var temp = Writeinfo.FirstOrDefault(x => x.OriText == v && x.ControlName == v2);
                    if (temp != null)
                    {
                        v = temp.TransText;
                        if (!ReadW)
                        {
                            v = temp.OriText;
                        }
                        temp.Count -= 1;
                        if (temp.Count == 0)
                        {
                            Writeinfo.Remove(temp);
                        }
                    }
                }
                else
                {
                    var temp = Writeinfo.FirstOrDefault(x => x.TransText == v && x.ControlName == v2);
                    if (temp != null)
                    {
                        v = temp.OriText;
                        temp.Count -= 1;
                        if (temp.Count == 0)
                        {
                            Writeinfo.Remove(temp);
                        }
                    }
                }
            }
            return v;
        }

        private void ReadAdd(string v, string v2)
        {
            if (!CheckBox(v)) return;
            var Find = Readinfo.FirstOrDefault(x => x.OriText == v && x.ControlName == v2);
            if (Find == null)
            {
                Readinfo.Add(new FormText(v, 1, v2));
            }
            else
            {
                Find.Count += 1;
            }
        }

        private static bool CheckBox(string v)
        {
            return v != null && v.Select(x => x.ToString()).ToArray().Any(item => Convert.ToChar(item) > 128);
            /*foreach (var item in v.Select(x => x.ToString()).ToArray())
            {
                if (Convert.ToChar(item) > 128)
                {
                    return true;
                }
            }
            return false;*/
        }

        private void GetOrChangeControl(Control Control, int count)
        {
            if (Control is ComboBox)
            {
                var ComboBox = Control as ComboBox;
                if (ComboBox.Items.Count > 1)
                {
                    if (Read)
                    {
                        //var StringBuild = new StringBuilder();
                        foreach (var item in ComboBox.Items)
                        {
                            ReadAdd(item.ToString(), ComboBox.Name);
                        }
                    }
                    if (Write)
                    {
                        for (int i = 0; i < ComboBox.Items.Count; i++)
                        {
                            var temp = WriteAdd(ComboBox.Items[i].ToString(), ComboBox.Name);
                            if (temp != null)
                            {
                                ComboBox.Items[i] = temp;
                            }
                        }
                        /* var tempinfo = Writeinfo.FirstOrDefault(x => x.ID == ComboBox.Name && x.Parent == (ComboBox.Parent != null ? ComboBox.Parent.Name : "null"));
                         if (tempinfo != null)
                         {
                             var StringSplit = tempinfo.text.Split(new char[] { '|' }, System.StringSplitOptions.RemoveEmptyEntries);
                             for (int i = 0; i < StringSplit.Length; i++)
                             {
                                 ComboBox.Items[i] = StringSplit[i];
                             }
                         }*/
                    }
                }
            }
            else if (Control is TabPage)
            {
                var TabPage = Control as TabPage;
                if (ReadW)
                {
                    switch (TabPage.Name)
                    {
                        case "tabPage5":
                            if (Bonepage == null)
                            {
                                Bonepage = TabPage;
                                foreach (Control item in TabPage.Controls.Cast<Control>().Where(item => item.Name.EndsWith("Find")))
                                {
                                    BoneSearch = item as TextBox;
                                    break;
                                }
                            }
                            break;

                        case "tabPage9":
                            if (Bodypage == null)
                            {
                                Bodypage = TabPage;
                                foreach (Control item in TabPage.Controls.Cast<Control>().Where(item => item.Name.EndsWith("Find")))
                                {
                                    BodySearch = item as TextBox;
                                    break;
                                }
                            }
                            break;

                        case "tabPage10":
                            if (jointpage == null)
                            {
                                jointpage = TabPage;
                                foreach (Control item in TabPage.Controls.Cast<Control>().Where(item => item.Name.EndsWith("Find")))
                                {
                                    JointSearch = item as TextBox;
                                    break;
                                }
                            }
                            break;
                    }
                    ReadAdd(TabPage.Text, TabPage.Name);
                }
                if (Write)
                {
                    TabPage.Text = WriteAdd(TabPage.Text, TabPage.Name);
                    /* if (Writeinfo != null)
                     {
                         var tempinfo = Writeinfo.FirstOrDefault(x => x.ID == TabPage.Name && x.Parent == (TabPage.Parent != null ? TabPage.Parent.Name : "null"));
                         if (tempinfo != null)
                         {
                             TabPage.Text = tempinfo.text;
                         }
                     }*/
                }
            }
            else if (Control.Text != "0" && !string.IsNullOrWhiteSpace(Control.Text))
            {
                if (!regex.IsMatch(Control.Text))
                {
                    switch (Control.Name)
                    {
                        case "":
                            if (Write)
                            {
                                //ReadAdd(Control.Text, "TextBox");
                                if (Writeinfo.Count != 0) Writeinfo[0].OriText = Control.Text;
                                Control.Text = WriteAdd(Control.Text, "TextBox").Replace("\n", "\r\n");
                            }
                            break;
                        /*case "btnVertex_NormalizeWeight":
                                break;*/
                        case "btnGetObject":
                            if (btnGetObject == null) btnGetObject = Control as Button;
                            break;

                        default:
                            if (Control.Name == "lblInfo_PmxVer" || Control.Name == "label80")
                            {
                                return;
                            }
                            if (Read)
                            {
                                ReadAdd(Control.Text, Control.Name);
                            }
                            if (Write)
                            {
                                /*  if (Writeinfo != null)
                              {
                                  var tempinfo = Writeinfo.FirstOrDefault(x => x.ID == Control.Name && x.Parent == (Control.Parent != null ? Control.Parent.Name : "null"));
                                  if (tempinfo != null)
                                  {
                                      Control.Text = tempinfo.text;
                                  }
                              }*/
                                Control.Text = WriteAdd(Control.Text, Control.Name);
                            }
                            break;
                    }
                }
            }
            else if (Control.Name == "vtCtrl")
            {
                foreach (var fi in Control.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    SwichControl(fi.GetValue(Control), count);
                }
            }
            else if (Control is ContextMenuStrip)
            {
                if (contextMaterial == null || contextBone == null)
                {
                    if (Control.Name == "contextMaterial")
                    {
                        contextMaterial = Control as ContextMenuStrip;
                    }
                    if (Control.Name == "contextBone2")
                    {
                        contextBone = Control as ContextMenuStrip;
                    }
                }
            }
        }

        //private FormText TempForm = new FormText();

        private void GetOrChangeForm(Form form, int count)
        {
            if (form.Name == "PmxViewEdit")
            {
                form.Font = new System.Drawing.Font("MS UI Gothic", 9f);
            }
            if (Read)
            {
                ReadAdd(form.Text, form.Name);
            }
            if (Write)
            {
                form.Text = WriteAdd(form.Text, form.Name);
                /* var temp = Writeinfo.FirstOrDefault(x => x.ID == form.Name && x.Parent == (form.Parent!=null? form.Parent.Name:"null"));
                 if(temp!=null)    form.Name = temp.text;*/
            }
            foreach (var fi in form.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                SwichControl(fi.GetValue(form), count);
            }
        }
    }
}