﻿using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;
using static PE多功能信息处理插件.Class2.FormInfo;
using static PE多功能信息处理插件.Program;

namespace PE多功能信息处理插件
{
    internal class TranslateMod
    {
        private bool Read = true;
        private bool Write = false;
        private List<FormText> TranslateFile = new List<FormText>();
        private List<FormText> Readinfo = new List<FormText>();
        private List<FormText> Writeinfo = new List<FormText>();
        private readonly Regex regex = new Regex(@"^[A-Za-z0-9]+$", RegexOptions.Compiled);

        public TranslateMod(Form formtemp, bool write)
        {
            Write = write;
            if (write)
            {
                XmlSerializer Ser = new XmlSerializer(typeof(FormText[]));
                TranslateFile = new List<FormText>(Ser.Deserialize(new MemoryStream(Resource1.zn_CN)) as FormText[]);
            }
            SwichControl(formtemp, 0);
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
            if (!regex.IsMatch(toolStripItem.Text))
            {
            }
            foreach (var fi in toolStripItem.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                var obj = fi.GetValue(toolStripItem);
                SwichControl(obj, count);
            }
        }

        private void GetOrChangeControl(Control Control, int count)
        {
            if (Control is ComboBox)
            {
                var ComboBox = Control as ComboBox;
                if (ComboBox.Items.Count != 0)
                {
                    if (Read)
                    {
                        var StringBuild = new StringBuilder();
                        foreach (var item in ComboBox.Items)
                        {
                            StringBuild.Append(item.ToString());
                            StringBuild.Append("|");
                        }
                        Readinfo.Add(new FormText(ComboBox.Name, StringBuild.ToString()));
                    }
                    if (Write)
                    {
                        var tempinfo = Writeinfo.Find(x => x.ID == ComboBox.Name);
                        if (tempinfo != null)
                        {
                            var StringSplit = tempinfo.text.Split('|');
                            for (int i = 0; i < ComboBox.Items.Count; i++)
                            {
                                ComboBox.Items[i] = StringSplit[i];
                            }
                        }
                    }
                }
            }
            else if (Control is TabPage)
            {
                var TabPage = Control as TabPage;
                if (Read)
                {
                    if (TabPage.Name == "tabPage5")
                    {
                        if (Bonepage == null)
                        {
                            Bonepage = TabPage;
                            foreach (Control item in TabPage.Controls)
                            {
                                if (item.Name.EndsWith("Find"))
                                {
                                    BoneSearch = item as TextBox;
                                    break;
                                }
                            }
                        }
                    }
                    else if (TabPage.Name == "tabPage9")
                    {
                        if (Bodypage == null)
                        {
                            Bodypage = TabPage;
                            foreach (Control item in TabPage.Controls)
                            {
                                if (item.Name.EndsWith("Find"))
                                {
                                    BodySearch = item as TextBox;
                                    break;
                                }
                            }
                        }
                    }
                    else if (TabPage.Name == "tabPage10")
                    {
                        if (jointpage == null)
                        {
                            jointpage = TabPage;
                            foreach (Control item in TabPage.Controls)
                            {
                                if (item.Name.EndsWith("Find"))
                                {
                                    JointSearch = item as TextBox;
                                    break;
                                }
                            }
                        }
                    }
                    Readinfo.Add(new FormText(TabPage.Name, TabPage.Text));
                }
                if (Write)
                {
                    if (Writeinfo != null)
                    {
                        var tempinfo = Writeinfo.Find(x => x.ID == TabPage.Name);
                        if (tempinfo != null)
                        {
                            TabPage.Text = tempinfo.text;
                        }
                    }
                }
            }
            else if (Control.Text != "0" && Control.Text != "")
            {
                if (!regex.IsMatch(Control.Text))
                {
                    if (Control.Name.ToString() == "")
                    {
                        Readinfo.Add(new FormText("TextBox", Control.Text));
                    }
                    else
                    {
                        if (Read)
                        {
                            Readinfo.Add(new FormText(Control.Name, Control.Text));
                        }
                        if (Write)
                        {
                            if (Writeinfo != null)
                            {
                                var tempinfo = Writeinfo.Find(x => x.ID == Control.Name);
                                if (tempinfo != null)
                                {
                                    Control.Text = tempinfo.text;
                                }
                            }
                        }
                    }
                }
            }
            else if (Control.Name == "vtCtrl")
            {
                foreach (var fi in Control.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    var obj = fi.GetValue(Control);
                    SwichControl(obj, count);
                }
            }
        }

        private void GetOrChangeForm(Form form, int count)
        {
            if (form.Name == "PmxViewEdit")
            {
                form.Font = new System.Drawing.Font("MS UI Gothic", 9f);
            }
            foreach (var fi in form.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                var obj = fi.GetValue(form);
                SwichControl(obj, count);
            }
        }
    }
}