using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace PE多功能信息处理插件
{
    public class Class2
    {
        public static Dictionary<string, BezierPoint> HisForm = new Dictionary<string, BezierPoint>();
        public static ResourceManager Language;
        public static Button btnGetObject = null;
        public static ToolStripItem FormTopMost = null;

        public static readonly Color[] color = {
            Color.Green, Color.AntiqueWhite, Color.Aqua, Color.Aquamarine, Color.Azure, Color.Beige, Color.Bisque,
            Color.Black, Color.BlanchedAlmond, Color.Blue, Color.BlueViolet, Color.Brown, Color.BurlyWood,
            Color.CadetBlue, Color.Chartreuse, Color.Chocolate, Color.Coral, Color.CornflowerBlue, Color.Cornsilk,
            Color.Crimson, Color.Cyan, Color.DarkBlue, Color.DarkCyan, Color.DarkGoldenrod, Color.DarkGray,
            Color.DarkGreen, Color.DarkKhaki, Color.DarkMagenta, Color.DarkOliveGreen, Color.DarkOrange,
            Color.DarkOrchid, Color.DarkRed, Color.DarkSalmon, Color.DarkSeaGreen, Color.DarkSlateBlue,
            Color.DarkSlateGray, Color.DarkTurquoise, Color.DarkViolet, Color.DeepSkyBlue, Color.DeepPink,
            Color.DimGray, Color.DodgerBlue, Color.Firebrick, Color.FloralWhite, Color.ForestGreen, Color.Fuchsia,
            Color.Gainsboro, Color.GhostWhite, Color.Gold, Color.Goldenrod, Color.Gray, Color.AliceBlue,
            Color.GreenYellow, Color.Honeydew, Color.HotPink, Color.IndianRed, Color.Indigo, Color.Ivory, Color.Khaki,
            Color.Lavender, Color.LavenderBlush, Color.LawnGreen, Color.LemonChiffon, Color.LightBlue, Color.LightCoral,
            Color.LightCyan, Color.LightGoldenrodYellow, Color.LightGray, Color.LightGreen, Color.LightPink,
            Color.LightSalmon, Color.LightSeaGreen, Color.LightSkyBlue, Color.LightSlateGray, Color.LightSteelBlue,
            Color.LightYellow, Color.Lime, Color.LimeGreen, Color.Linen, Color.Magenta, Color.Maroon,
            Color.MediumAquamarine, Color.MediumBlue, Color.MediumOrchid, Color.MediumPurple, Color.MediumSeaGreen,
            Color.MediumSlateBlue, Color.MediumSpringGreen, Color.MediumTurquoise, Color.MediumVioletRed,
            Color.MidnightBlue, Color.MintCream, Color.MistyRose, Color.Moccasin, Color.NavajoWhite, Color.Navy,
            Color.OldLace, Color.Olive, Color.OliveDrab, Color.Orange, Color.OrangeRed, Color.Orchid,
            Color.PaleGoldenrod, Color.PaleGreen, Color.PaleTurquoise, Color.PaleVioletRed, Color.PapayaWhip,
            Color.PeachPuff, Color.Peru, Color.Pink, Color.Plum, Color.PowderBlue, Color.Purple, Color.Red,
            Color.RosyBrown, Color.RoyalBlue, Color.SaddleBrown, Color.Salmon, Color.SandyBrown, Color.SeaGreen,
            Color.SeaShell, Color.Sienna, Color.Silver, Color.SkyBlue, Color.SlateBlue, Color.SlateGray, Color.Snow,
            Color.SpringGreen, Color.SteelBlue, Color.Tan, Color.Teal, Color.Thistle, Color.Tomato, Color.Transparent,
            Color.Turquoise, Color.Violet, Color.Wheat, Color.White, Color.WhiteSmoke, Color.Yellow, Color.YellowGreen
        };

        [Serializable]
        public class BootState
        {
            public int openstate { get; set; }
            public int Keystate { get; set; }
            public int StyleState { get; set; }
            public int FormX { get; set; }
            public int FormY { get; set; }
            public int FormForSizeX { get; set; }
            public int FormForSizeY { get; set; }
            public int AutoOpen { get; set; }
            public int WeightGetKey { get; set; }
            public int WeightAddKey { get; set; }
            public int WeightMinusKey { get; set; }
            public int WeightAppleKey { get; set; }
            public string OldOpen;
            public OpenHis[] HisOpen;
            public int FormTopmost;
            public int BezierFirstColor;
            public int BezierSecondColor;
            public float BezierLinkSize;
            public int ShowUpdata;
            public int FormTop;
            public int UpdataDateCheck;

            public BootState(int open, int Key, int Style)
            {
                openstate = open;
                Keystate = Key;
                StyleState = Style;
            }
        }

        [Serializable]
        public class OpenHis
        {
            public string modelname;
            public string modelpath;
            public string modeldata;

            public OpenHis(string modelName, string filePath, string modeldata)
            {
                if (modelName == "")
                {
                    if (filePath != "")
                    {
                        modelName = new FileInfo(filePath).Name;
                    }
                }
                modelname = modelName;
                modelpath = filePath;
                this.modeldata = modeldata;
            }
        }

        public class FormText
        {
            public string OriText;
            public string TransText;
            public string ControlName;
            public int Count;

            public FormText()
            {
            }

            public FormText(string v1, int v2, string v3)
            {
                OriText = v1;
                Count = v2;
                ControlName = v3;
            }

            public FormText(string OriText, int Count, string ControlName, string TransText)
            {
                this.OriText = OriText;
                this.Count = Count;
                this.ControlName = ControlName;
                this.TransText = TransText;
            }
        }

        public class OperaList
        {
            public List<int> Count;

            public OperaList(List<int> Count)
            {
                this.Count = Count;
            }
        }

        public class UBinder : SerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                if (typeName.EndsWith("keySet"))
                {
                    return typeof(keySet);
                }
                /* if (typeName.StartsWith("System.Collections.Generic.List"))
                 {
                     return Type.GetType(String.Format("{0}, {1}", typeName, assemblyName));
                 }*/
                return (Assembly.GetExecutingAssembly()).GetType(typeName);
            }
        }

        public class BezierPoint
        {
            public Point p1;
            public Point p2;
            public Point p3;
            public Point p4;

            public BezierPoint(Point p1, Point p2, Point p3, Point p4)
            {
                this.p1 = p1;
                this.p2 = p2;
                this.p3 = p3;
                this.p4 = p4;
            }
        }

        public class TabInfo
        {
            public int TabID;
            public string TabName;
            public List<KeyInfo> KeyLIst = new List<KeyInfo>();

            public TabInfo(int tabID, string tabName, string itemname, string itemKey, int itemLocalX, int itemLocaly,
                string itemFun)
            {
                TabID = tabID;
                TabName = tabName;
                KeyLIst.Add(new KeyInfo(itemname, itemKey, itemLocalX, itemLocaly, itemFun));
            }

            public class KeyInfo
            {
                public int itemLocalX;
                public int itemLocaly;
                public string itemKey;
                public string itemFun;
                public string itemname;

                public KeyInfo(string itemname, string itemKey, int itemLocalX, int itemLocaly, string itemFun)
                {
                    this.itemname = itemname;
                    this.itemKey = itemKey;
                    this.itemLocalX = itemLocalX;
                    this.itemLocaly = itemLocaly;
                    this.itemFun = itemFun;
                }
            }
        }

        public class ItemDate
        {
            public int Count;
            public double FirstNummer;
            public double SecendNummer;

            public ItemDate(int v1, double v2)
            {
                Count = v1;
                FirstNummer = v2;
            }

            public ItemDate(int v1, double v2, double v) : this(v1, v2)
            {
                SecendNummer = v;
            }
        }

        public class TheDataForBezier
        {
            public string mode;
            public string FirstNummerLow;
            public string LastNummerLow;
            public string FirstNummerHigh;
            public string LastNummerHigh;
            public int[] ItemCount;
            public OperaList[] ListItemCount;
            public List<ItemDate> SaveItem;
            public int UseMode = 0;

            public TheDataForBezier(string v1, string text1, string text2, int[] v2)
            {
                mode = v1;
                FirstNummerLow = text1;
                LastNummerLow = text2;
                ItemCount = v2;
            }

            public TheDataForBezier(string v1, string text1, string text2, OperaList[] v2)
            {
                mode = v1;
                FirstNummerLow = text1;
                LastNummerLow = text2;
                ListItemCount = v2;
            }

            public TheDataForBezier(string v1, string text1, string text2, string limit_MoveHigh_FirstXNummer,
                string limit_MoveHigh_LastXNummer, int[] v2)
            {
                mode = v1;
                FirstNummerLow = text1;
                LastNummerLow = text2;
                FirstNummerHigh = limit_MoveHigh_FirstXNummer;
                LastNummerHigh = limit_MoveHigh_LastXNummer;
                ItemCount = v2;
            }

            public TheDataForBezier(string v1, string text1, string text2, string limit_MoveHigh_FirstXNummer,
                string limit_MoveHigh_LastXNummer, OperaList[] v2)
            {
                mode = v1;
                FirstNummerLow = text1;
                LastNummerLow = text2;
                FirstNummerHigh = limit_MoveHigh_FirstXNummer;
                LastNummerHigh = limit_MoveHigh_LastXNummer;
                ListItemCount = v2;
            }
        }

        [Serializable]
        public class keySet
        {
            public int TabID;
            public string TabName;
            public string itemname;
            public int itemLocalX;
            public int itemLocaly;
            public string itemKey;
            public string itemFun;

            [NonSerialized] public ToolItemInfo item;

            public keySet(int TabID, string TabName, string itemname, int itemLocalX, int itemLocaly, string itemKey,
                string itemFun)
            {
                this.TabID = TabID;
                this.TabName = TabName;
                this.itemname = itemname;
                this.itemLocalX = itemLocalX;
                this.itemLocaly = itemLocaly;
                this.itemKey = itemKey;
                this.itemFun = itemFun;
            }
        }

        public class ToolItemInfo
        {
            public ToolItemInfo(ToolStripMenuItem temp4)
            {
                Item = temp4;
            }

            public ToolItemInfo(ToolStripMenuItem temp4, string v)
            {
                Item = temp4;
                path = v;
            }

            public ToolStripMenuItem Item { get; set; }
            public string path { get; set; }
        }

        [Serializable]
        public class MorphOpera
        {
            public Morph[] MorphList;

            [Serializable]
            public class Morph
            {
                public String MorphName;
                public int Panel;
                public Vertex[] VertexList;

                [Serializable]
                public class Vertex
                {
                    public int index;
                    public float tox;
                    public float toy;
                    public float toz;
                }
            }

            public Index[] IndexList;

            [Serializable]
            public class Index
            {
                private int _index;

                public int index
                {
                    get => _index;
                    set => _index = value;
                }
                public int toindex;
                public float x;
                public float y;
                public float z;
                public float UVX;
                public float UVY;
                public float NormalX;
                public float NormalY;
                public float NormalZ;
            }
        }
    }
}