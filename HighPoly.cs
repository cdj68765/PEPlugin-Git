using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PEPlugin;
using PEPlugin.Pmd;
using PEPlugin.Pmx;
using PEPlugin.SDX;
using PEPlugin.View;

namespace PE多功能信息处理插件
{
////////////////////////////////////////////////////////////////////
//
// ハイポリ化 プラグイン
//
//
// 面、ウェイト、モーフをハイポリ化します。
// 少しだけ丸く、小さく、滑らかになります。
//
//
// ※以下の場合には正常に動作しなかったり、エラーが発生する可能性があります。
// ・SDEF,QDEFを使っている                                 (対処 : 選択頂点のウェイトをBDEFに戻す)
// ・不正なウェイト,不正面,重複面,不正法線のいずれかがある (対処 : ウェイト正規化,不正面の削除,重複面の削除,不正法線の修正)
// ・1つの辺を3つ以上の面が共有している                    (対処 : 共有頂点の分離)
//
//
// みみお(マダムP)さんの面を自動分割する試作スプリプトを参考にしています。
// 頂点情報の補間はCatmull-Clark法を参考にしています。
//
////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////
//
// =======================================================================
// 制作メモ : <面頂点リストと選択面、および面リストと材質リストの関係について>
//
// 以下の3点をふまえると、複数の選択面を、それぞれが属する材質に分けて処理できることが分かる。
//
// ●面頂点リスト = 面リストを変形した面頂点のリスト = pmd.Face
//
//   PmxEditorの面リストには、
//   面のindexと一緒に、各面の参照頂点(面頂点)のindexが面1つにつき3つ表示されている。
//
//    (面リストにある情報)
//    面_0:(面_0の第0頂点の頂点index, 面_0の第1頂点の頂点index, 面_0の第2頂点の頂点index)
//    面_1:(面_1の第0頂点の頂点index, 面_1の第1頂点の頂点index, 面_1の第2頂点の頂点index)
//    面_2:(面_2の第0頂点の頂点index, 面_2の第1頂点の頂点index, 面_2の第2頂点の頂点index)
//    ...
//
//   面のindexは昇順で固定なので、実質面頂点のindexのリストになっている。
//
//    (面リストを変形して得られる面頂点のリスト)
//    面_0の第0頂点の頂点index
//    面_0の第1頂点の頂点index
//    面_0の第2頂点の頂点index
//    面_1の第0頂点の頂点index
//    面_1の第1頂点の頂点index
//    面_1の第2頂点の頂点index
//    面_2の第0頂点の頂点index
//    面_2の第1頂点の頂点index
//    面_2の第2頂点の頂点index
//    ...
//
//   この面頂点のindexのリストがpmd.Faceである。
//   
// ●選択面として得られる整数配列 = 面頂点リストのindices
//
//   IPEPMDViewConnectorのGetSelectedFaceIndicesで
//   3,4,5,9,10,11
//   というデータが取得されたなら、それは以下を表している。
//
//    面頂点リストのindex= 3 ⇔ 面_( 3/3)の第( 3%3)頂点の頂点index
//    面頂点リストのindex= 4 ⇔ 面_( 4/3)の第( 4%3)頂点の頂点index
//    面頂点リストのindex= 5 ⇔ 面_( 5/3)の第( 5%3)頂点の頂点index
//    面頂点リストのindex= 9 ⇔ 面_( 9/3)の第( 9%3)頂点の頂点index
//    面頂点リストのindex=10 ⇔ 面_(10/3)の第(10%3)頂点の頂点index
//    面頂点リストのindex=11 ⇔ 面_(11/3)の第(11%3)頂点の頂点index
//
//   ちなみに、
//   ・MMD,PMXEでは面は三角面のみなので、選択面として取得できる整数配列の長さは常に3の倍数になる。
//   ・選択面として得られる整数配列は面を選択した順番も保持してる。(面_3,面_2と選択すると、{9,10,11,6,7,8}になる)
// 
// ●面リストの面の並びは材質リストの材質の並びと対応している
//
//   (面リストの内容)
//   材質_0の面_0
//   材質_0の面_1
//   材質_0の面_2
//   ...
//   材質_1の面_0
//   材質_1の面_1
//   材質_1の面_2
//   ...
//   ...
//
//   この対応関係は、"材質の並び替え"や"拡張編集での面の追加"があっても、面リストが自動で並び替えられて保たれる。
//
// =======================================================================
//
//
////////////////////////////////////////////////////////////////////

// using項目はメニューから一括管理

// Scriptプラグインクラス(クラス名変更不可)
    public class CSScriptClass : PEPluginClass
    {
        // コンストラクタ
        public CSScriptClass() : base()
        {
            // 起動オプション
            // boot時実行(true/false), プラグインメニューへの登録(true/false), メニュー登録名("")
            m_option = new PEPluginOption(false, true, "ハイポリ化");
        }

        // エントリポイント
        public override void Run(IPERunArgs args)
        {
            base.Run(args);
            try
            {
                // ここへ処理を追加してください.

                myForm f = new myForm(args);
                f.Show(); // フォームをモードレスフォームとして表示

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
    }

//////////////////////////////////////////////////////////////////

// 頂点の集合 (等値比較付き)
    class CVSet
    {
        public HashSet<IPXVertex> m_vset;

        public CVSet()
        {
            m_vset = new HashSet<IPXVertex>();
        }
        public CVSet(CVSet p)
        {
            m_vset = new HashSet<IPXVertex>(p.m_vset);
        }
        public CVSet(IPXVertex v)
        {
            m_vset = new HashSet<IPXVertex>() { v };
        }
        public CVSet(IPXVertex[] arr)
        {
            m_vset = new HashSet<IPXVertex>(arr);
        }

        // 要素数1のCVSetの集合に分ける
        public HashSet<CVSet> Separate()
        {
            HashSet<CVSet> ret = new HashSet<CVSet>();

            foreach (IPXVertex v in this.m_vset)
            {
                ret.Add(new CVSet(v));
            }

            return ret;
        }

        public override bool Equals(System.Object obj)
        {
            // 自身と入力とで積集合をとり、それら3つの要素数が同じなら等値とする
            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }

            int InputCount = ((CVSet)obj).m_vset.Count; // 入力した集合の要素数
            CVSet p = new CVSet((CVSet)obj); // 入力した集合の複製
            p.m_vset.IntersectWith(this.m_vset); // 複製を積集合に

            return ((p.m_vset.Count == this.m_vset.Count) && (p.m_vset.Count == InputCount));
        }

        public override int GetHashCode()
        {
            int tmp = 0;
            foreach (IPXVertex v in m_vset)
            {
                tmp ^= v.GetHashCode();
            }
            return tmp;
        }
    }

// 頂点情報
    class CVDat
    {
        public IPXVertex m_v;    // 頂点としての情報
        public Dictionary<IPXMorph, V3> m_VMO;  // 頂点モーフのオフセット量 : < モーフ, オフセット量 >
        public Dictionary<IPXMorph, V4> m_UVMO; // UVモーフのオフセット量 : < モーフ, オフセット量 >

        public CVDat()
        {
            m_v = null; // とりあえずnull
            m_VMO = new Dictionary<IPXMorph, V3>();
            m_UVMO = new Dictionary<IPXMorph, V4>();
        }
    }

// 辺や面のキーの集まり
    class CEFKeys
    {
        public HashSet<CVSet> m_EdgeKeys; // 辺のキーの集合
        public HashSet<CVSet> m_FaceKeys; // 面のキーの集合

        public CEFKeys()
        {
            m_EdgeKeys = new HashSet<CVSet>();
            m_FaceKeys = new HashSet<CVSet>();
        }
    }

// 関数を呼ぶためだけのクラス
    class Ifunc
    {
        // 頂点情報の辞書から、指定要素の平均を求める
        static public CVDat CalcAverage(HashSet<CVSet> keys, Dictionary<CVSet, CVDat> dic, IPXPmxBuilder bdx)
        {
            CVDat vdat = new CVDat();
            vdat.m_v = bdx.Vertex();

            // 平均を出すために足した値を格納するので、0に初期化しておく
            vdat.m_v.Normal = new V3(0, 0, 0);
            vdat.m_v.EdgeScale = 0;

            int num = keys.Count;

            // BDEFを想定してるのでSDEF,QDEFは無効化
            vdat.m_v.QDEF = false;
            vdat.m_v.SDEF = false;
            vdat.m_v.SDEF_C = new V3(0, 0, 0);
            vdat.m_v.SDEF_R0 = new V3(0, 0, 0);
            vdat.m_v.SDEF_R1 = new V3(0, 0, 0);

            // ウェイトボーン情報
            Dictionary<IPXBone, float> aveWeightBone = new Dictionary<IPXBone, float>();

            // 頂点モーフ情報
            Dictionary<IPXMorph, V3> aveVMO = new Dictionary<IPXMorph, V3>();

            // 頂点モーフ情報
            Dictionary<IPXMorph, V4> aveUVMO = new Dictionary<IPXMorph, V4>();

            // 各頂点情報を足し合わせる---------------------------------
            foreach (CVSet key in keys)
            {
                // ボーン,ウェイト関係以外の頂点本体の情報は単純に足す
                vdat.m_v.Position += dic[key].m_v.Position;
                vdat.m_v.Normal += dic[key].m_v.Normal;
                vdat.m_v.UV += dic[key].m_v.UV;
                vdat.m_v.UVA1 += dic[key].m_v.UVA1;
                vdat.m_v.UVA2 += dic[key].m_v.UVA2;
                vdat.m_v.UVA3 += dic[key].m_v.UVA3;
                vdat.m_v.UVA4 += dic[key].m_v.UVA4;
                vdat.m_v.EdgeScale += dic[key].m_v.EdgeScale;

                // ボーン,ウェイト関係の足し合わせ
                if (aveWeightBone.ContainsKey(dic[key].m_v.Bone1))
                {
                    aveWeightBone[dic[key].m_v.Bone1] += dic[key].m_v.Weight1;
                }
                else
                {
                    aveWeightBone.Add(dic[key].m_v.Bone1, dic[key].m_v.Weight1);
                }

                if (dic[key].m_v.Bone2 != null)
                {
                    if (aveWeightBone.ContainsKey(dic[key].m_v.Bone2))
                    {
                        aveWeightBone[dic[key].m_v.Bone2] += dic[key].m_v.Weight2;
                    }
                    else
                    {
                        aveWeightBone.Add(dic[key].m_v.Bone2, dic[key].m_v.Weight2);
                    }

                    if (dic[key].m_v.Bone3 != null)
                    {
                        if (aveWeightBone.ContainsKey(dic[key].m_v.Bone3))
                        {
                            aveWeightBone[dic[key].m_v.Bone3] += dic[key].m_v.Weight3;
                        }
                        else
                        {
                            aveWeightBone.Add(dic[key].m_v.Bone3, dic[key].m_v.Weight3);
                        }

                        if (dic[key].m_v.Bone4 != null)
                        {
                            if (aveWeightBone.ContainsKey(dic[key].m_v.Bone4))
                            {
                                aveWeightBone[dic[key].m_v.Bone4] += dic[key].m_v.Weight4;
                            }
                            else
                            {
                                aveWeightBone.Add(dic[key].m_v.Bone4, dic[key].m_v.Weight4);
                            }
                        }
                    }
                }

                // 頂点モーフのオフセット量を足す
                foreach (IPXMorph tmpm in dic[key].m_VMO.Keys)
                {
                    if (aveVMO.ContainsKey(tmpm))
                    {
                        aveVMO[tmpm] += dic[key].m_VMO[tmpm];
                    }
                    else
                    {
                        aveVMO.Add(tmpm, dic[key].m_VMO[tmpm]);
                    }
                }

                // UVモーフのオフセット量を足す
                foreach (IPXMorph tmpm in dic[key].m_UVMO.Keys)
                {
                    if (aveUVMO.ContainsKey(tmpm))
                    {
                        aveUVMO[tmpm] += dic[key].m_UVMO[tmpm];
                    }
                    else
                    {
                        aveUVMO.Add(tmpm, dic[key].m_UVMO[tmpm]);
                    }
                }
            }
            // ---------------------------------各頂点の頂点情報を足し合わせる

            // 平均を求める---------------------------------------------------------
            // ボーン,ウェイト関係以外の頂点本体の情報の平均
            vdat.m_v.Position /= num;
            if (vdat.m_v.Normal.LengthSq() > 0)
            {
                vdat.m_v.Normal.Normalize(); // 法線は正規化
            }
            else
            {
                vdat.m_v.Normal = new V3(0, 0, 1); // 適当に正規化
            }
            vdat.m_v.UV /= num;
            vdat.m_v.UVA1 /= num;
            vdat.m_v.UVA2 /= num;
            vdat.m_v.UVA3 /= num;
            vdat.m_v.UVA4 /= num;
            vdat.m_v.EdgeScale /= num;

            // ボーン,ウェイト関係の平均---------------------------
            // ウェイトでソートして、大きい方の1～4個を使う
            int wbnumAll = aveWeightBone.Count;            // ウェイトボーン総数
            int wbnum = Math.Min(aveWeightBone.Count, 4);       // 有効なウェイトボーン数
            IPXBone[] aveWeightBone_Bone = aveWeightBone.Keys.ToArray(); // ソート後のボーン列
            float[] aveWeightBone_Weight = aveWeightBone.Values.ToArray(); // ソート後のウェイト列

            Array.Sort(aveWeightBone_Weight, aveWeightBone_Bone); // ウェイトで昇順ソート

            // ウェイトを正規化しておく (先頭の4つまで)
            float tmpsum = 0;
            for (int l = 1; l <= wbnum; ++l)
            {
                tmpsum += aveWeightBone_Weight[wbnumAll - l];
            }
            for (int l = 1; l <= wbnum; ++l)
            {
                aveWeightBone_Weight[wbnumAll - l] /= tmpsum;
            }

            // 設定
            vdat.m_v.Bone1 = aveWeightBone_Bone[wbnumAll - 1];
            vdat.m_v.Weight1 = aveWeightBone_Weight[wbnumAll - 1];
            switch (wbnum)
            {
                case 1:
                    vdat.m_v.Bone2 = null;
                    vdat.m_v.Weight2 = 0;
                    vdat.m_v.Bone3 = null;
                    vdat.m_v.Weight3 = 0;
                    vdat.m_v.Bone4 = null;
                    vdat.m_v.Weight4 = 0;
                    break;
                case 2:
                    vdat.m_v.Bone2 = aveWeightBone_Bone[wbnumAll - 2];
                    vdat.m_v.Weight2 = aveWeightBone_Weight[wbnumAll - 2];
                    vdat.m_v.Bone3 = null;
                    vdat.m_v.Weight3 = 0;
                    vdat.m_v.Bone4 = null;
                    vdat.m_v.Weight4 = 0;
                    break;
                case 3:
                    vdat.m_v.Bone2 = aveWeightBone_Bone[wbnumAll - 2];
                    vdat.m_v.Weight2 = aveWeightBone_Weight[wbnumAll - 2];
                    vdat.m_v.Bone3 = aveWeightBone_Bone[wbnumAll - 3];
                    vdat.m_v.Weight3 = aveWeightBone_Weight[wbnumAll - 3];
                    vdat.m_v.Bone4 = null;
                    vdat.m_v.Weight4 = 0;
                    break;
                default:
                    vdat.m_v.Bone2 = aveWeightBone_Bone[wbnumAll - 2];
                    vdat.m_v.Weight2 = aveWeightBone_Weight[wbnumAll - 2];
                    vdat.m_v.Bone3 = aveWeightBone_Bone[wbnumAll - 3];
                    vdat.m_v.Weight3 = aveWeightBone_Weight[wbnumAll - 3];
                    vdat.m_v.Bone4 = aveWeightBone_Bone[wbnumAll - 4];
                    vdat.m_v.Weight4 = aveWeightBone_Weight[wbnumAll - 4];
                    break;
            }
            // ---------------------------ボーン,ウェイト関係の平均

            // 頂点モーフのオフセット量の平均
            foreach (IPXMorph tmpm in aveVMO.Keys)
            {
                vdat.m_VMO.Add(tmpm, aveVMO[tmpm] / num);
            }

            // UVモーフのオフセット量の平均
            foreach (IPXMorph tmpm in aveUVMO.Keys)
            {
                vdat.m_UVMO.Add(tmpm, aveUVMO[tmpm] / num);
            }

            // ---------------------------------------------------------平均を求める

            return vdat;
        }
    }

    public class myForm : Form
    {
        ///////////////////////////////////////////////////////////////////////////////////////
        // 常用接続変数-------------------------------------------------------

        // ホスト配下
        IPEPluginHost host;

        IPXPmxBuilder bdx;
        IPEConnector connect;
        IPEPMDViewConnector view;

        // PMX関連
        IPXPmx pmx;

        IList<IPXVertex> vertex; // vertex   :頂点   | リスト
        IList<IPXMaterial> material; // material :材質   | リスト
        IList<IPXMorph> morph; // morph    :モーフ | リスト

        // PMD関連 (選択面から頂点indexを出すためにIPEPmdのFaceを使う。)
        IPEPmd pmd; // PMD取得

        IList<int> face; // face     :面頂点  | リスト

        // -------------------------------------------------------常用接続変数

        // コントロール関連の変数----------------------------------------
        RadioButton[] RBtn; // 処理モード選択用

        CheckBox ChBx; // 縁の丸めフラグ
        // ----------------------------------------コントロール関連の変数

        ///////////////////////////////////////////////////////////////////////////////////////

        public myForm(IPERunArgs args)
        {
            // 常用接続変数 ホスト配下 の登録
            host = args.Host;
            bdx = host.Builder.Pmx;
            connect = host.Connector;
            view = host.Connector.View.PMDView;

            // フォームの用意--------------------------------------------
            Size = new Size(190, 120);
            Text = "ハイポリ化";
            FormBorderStyle = FormBorderStyle.FixedDialog; // サイズ変更不可のダイアログな境界線
            MaximizeBox = false; // 最大化無効
            MinimizeBox = false; // 最小化無効

            // 実行ボタン----------------------------
            Button Btn = new Button()
            {
                Text = "実行",
                AutoSize = true,
                Location = new Point(100, 60),
            };
            Btn.Click += new EventHandler(Btn_Click);

            this.Controls.Add(Btn);
            // ----------------------------実行ボタン

            // ラジオボタン & グループボックス--------------
            RBtn = new RadioButton[3];
            RBtn[0] = new RadioButton();
            RBtn[0].Text = "丸めあり";
            RBtn[0].AutoSize = true;
            RBtn[0].Location = new Point(10, 17);
            RBtn[1] = new RadioButton();
            RBtn[1].Text = "丸めのみ";
            RBtn[1].AutoSize = true;
            RBtn[1].Location = new Point(10, 37);
            RBtn[2] = new RadioButton();
            RBtn[2].Text = "丸めなし";
            RBtn[2].AutoSize = true;
            RBtn[2].Location = new Point(10, 57);
            RBtn[0].Checked = true;

            GroupBox GBox; // ラジオボタン用
            GBox = new GroupBox();
            GBox.Text = "モード";
            GBox.Location = new Point(5, 5);
            GBox.Size = new Size(85, 80);

            GBox.Controls.Add(RBtn[0]);
            GBox.Controls.Add(RBtn[1]);
            GBox.Controls.Add(RBtn[2]);
            this.Controls.Add(GBox);
            // --------------ラジオボタン & グループボックス

            // チェックボックス-----------------------------
            ChBx = new CheckBox();
            ChBx.Text = "縁も丸める";
            ChBx.Location = new Point(100, 28);

            this.Controls.Add(ChBx);
            // -----------------------------チェックボックス
            // --------------------------------------------フォームの用意
        }

        // 実行ボタンを押した時のイベント
        void Btn_Click(object sender, EventArgs e)
        {
            getcurrent();

            // メインの処理///////////////////////////////////////////////////////
            ////////////////////////////////////////////////

            // 入力の確認-----------------------------------------

            // 選択面の取得 (選択面の構成頂点番号、選択面数*3個の数値 が取得される)
            int[] fx = view.GetSelectedFaceIndices(); // 取得 | 面の利用には材質内Indexへの変換が必要
            int sfnum = fx.Length / 3; // 選択面数

            // 面を1つ以上選択していないなら中断
            if (sfnum <= 0)
            {
                MessageBox.Show("面を選択した状態で実行してね。");

                return;
            }

            // -----------------------------------------入力の確認

            // 主要変数----------------------------------------------------------

            // 選択面indices(昇順) (選択面の面頂点リストを変形したもの)
            List<int> SelectedFace = new List<int>();

            // 新規材質 : < 元材質, 複製材質 >
            Dictionary<IPXMaterial, IPXMaterial> CopyMaterial = new Dictionary<IPXMaterial, IPXMaterial>();

            // 元頂点の頂点情報
            Dictionary<CVSet, CVDat> OldVertex = new Dictionary<CVSet, CVDat>();

            // 面点の頂点情報
            Dictionary<CVSet, CVDat> FacePoint = new Dictionary<CVSet, CVDat>();

            // 辺の隣接情報
            Dictionary<CVSet, HashSet<CVSet>> EdgeAdjacent = new Dictionary<CVSet, HashSet<CVSet>>();

            // 元頂点の隣接情報
            Dictionary<CVSet, CEFKeys> VertexAdjacent = new Dictionary<CVSet, CEFKeys>();

            // 辺の中点の頂点情報 
            Dictionary<CVSet, CVDat> MidPoint = new Dictionary<CVSet, CVDat>();

            // 新規頂点(辺)の頂点情報
            Dictionary<CVSet, CVDat> EdgePoint = new Dictionary<CVSet, CVDat>();

            // 新規頂点(点)の頂点情報
            Dictionary<CVSet, CVDat> VertexPoint = new Dictionary<CVSet, CVDat>();

            // ----------------------------------------------------------主要変数

            ///////////////////////////////////////////////////////////////////////////////
            // ここからの流れ
            //
            // 0. [選択面関連]
            //    選択面indices(昇順)を求める
            //
            // 1. [新規材質関連]
            //    関連材質を元に新規材質を追加
            //      材質／頂点マスキングを変更
            //      新規材質のモーフを設定
            //
            // 2. [新規頂点関連]
            //    元頂点の頂点情報の計算
            //      辺や元頂点の隣接情報、面点、辺の中点の計算
            //        新規頂点(辺)の計算,追加
            //        新規頂点(点)の計算,追加
            //          新規頂点のモーフを設定
            //
            // 3. [新規面関連]
            //    新規面の追加
            //
            ///////////////////////////////////////////////////////////////////////////////

            // 0. [選択面関連]-------------------------------
           var pmxfile=  connect.Pmx.GetCurrentState();
          var facef=  connect.View.PmxView.GetSelectedFaceIndices();//通过插件API得到的是面的顶点数量序列，所以需要转换成面的索引，面的顶点序列是每三个一轮，所以第一个除3就可以获得面序列
            // 選択面indices(昇順)を求める
            for (int i = 0; i < sfnum; ++i)
            {
                SelectedFace.Add(fx[i * 3] / 3);
            }
            SelectedFace.Sort();
            // -------------------------------0. [選択面関連]


            // 1. [新規材質関連]-------------------------------------------------

            int OldMaterialCount = material.Count; // 元の材質数

            // 関連材質を元に新規材質を追加------------------------------

            int tmpmi = 0; // 計算用 : 材質のindex
            int tmpcnt = material[tmpmi].Faces.Count(); // 計算用 : 材質の面数の累積値

            foreach (int sf in SelectedFace)
            {
                while (sf >= tmpcnt)
                {
                    tmpcnt += material[++tmpmi].Faces.Count;
                }

                if (!CopyMaterial.ContainsKey(material[tmpmi]))
                {
                    // 名前,面以外の情報を複製
                    IPXMaterial mtnew = (IPXMaterial) material[tmpmi].Clone();
                    if (RBtn[1].Checked == true)
                    {
                        mtnew.Name += "x1";
                        mtnew.NameE += "x1";
                    }
                    else
                    {
                        mtnew.Name += "x2";
                        mtnew.NameE += "x2";
                    }
                    mtnew.Faces.Clear();

                    material.Add(mtnew); // 材質リストへ追加
                    CopyMaterial.Add(material[tmpmi], material[material.Count - 1]);
                }
            }
            // ------------------------------関連材質を元に新規材質を追加

            // 材質／頂点マスキングを変更-------------------------------------------------------------

            // ラジオボタンの選択状態を"材質"にしておく
            connect.View.PMDViewHelper.PartsSelect.SelectObject = PartsSelectObject.Material;

            // 材質のチェック状態 : 追加した材質のみを選択
            int[] setmx = new int[material.Count - OldMaterialCount];

            for (int i = 0; i < material.Count - OldMaterialCount; ++i)
            {
                setmx[i] = OldMaterialCount + i;
            }

            connect.View.PMDViewHelper.PartsSelect.SetCheckedMaterialIndices(setmx);
            // -------------------------------------------------------------材質／頂点マスキングを変更

            // 新規材質のモーフを設定-------------------------------------------------------------
            foreach (IPXMorph m in morph)
            {
                if (m.IsMaterial)
                {
                    List<IPXMaterialMorphOffset> addMOList = new List<IPXMaterialMorphOffset>();

                    foreach (IPXMaterialMorphOffset mo in m.Offsets)
                    {
                        // 全材質が対象のモーフオフセットなら、何もしない
                        if (mo.Material == null)
                        {
                            continue;
                        }

                        // モーフ対象材質が関連材質なら、そのオフセット量を複製材質のオフセット量として追加
                        if (CopyMaterial.ContainsKey(mo.Material))
                        {
                            var omx = bdx.MaterialMorphOffset();

                            omx.Material = CopyMaterial[mo.Material];

                            omx.Op = mo.Op;
                            omx.Diffuse = mo.Diffuse;
                            omx.Ambient = mo.Ambient;
                            omx.Specular = mo.Specular;
                            omx.Power = mo.Power;
                            omx.EdgeSize = mo.EdgeSize;
                            omx.EdgeColor = mo.EdgeColor;
                            omx.Tex = mo.Tex;
                            omx.Toon = mo.Toon;
                            omx.Sphere = mo.Sphere;

                            addMOList.Add(omx);
                        }
                    }

                    if (addMOList.Count > 0)
                    {
                        foreach (IPXMorphOffset addMO in addMOList)
                        {
                            m.Offsets.Add(addMO);
                        }
                    }
                }
            }
            // -------------------------------------------------------------新規材質のモーフを設定
            // -------------------------------------------------1. [新規材質関連]

            // 2. [新規頂点関連]-----------------------------------------------------------------------
            // 元頂点の頂点情報の計算-----------------------------------------------------------

            // まず、元頂点をキーとした頂点情報のハッシュテーブルを作る
            var dictemp=new Dictionary<int,int>();
            foreach (int sf in SelectedFace)
            {
                for (int fvi = 0; fvi < 3; ++fvi)
                {
                    CVSet key = new CVSet(vertex[face[3 * sf + fvi]]); //从面索引获得面所使用的顶点索引，并且获得顶点信息
                    if (!OldVertex.ContainsKey(key))
                    {
                        CVDat vdat = new CVDat();
                        vdat.m_v = (IPXVertex) vertex[face[3 * sf + fvi]].Clone();
                        OldVertex.Add(key, vdat);
                    }
                }
            }//剔除重复顶点

            // 次に、各頂点モーフ/UVモーフに含まれている元頂点のオフセット量をハッシュテーブルに記録
            foreach (IPXMorph m in morph)
            {
                if (m.IsVertex)
                {
                    foreach (IPXVertexMorphOffset mo in m.Offsets)
                    {
                        CVSet key = new CVSet(mo.Vertex);

                        if (OldVertex.ContainsKey(key))
                        {
                            if (!OldVertex[key].m_VMO.ContainsKey(m)) // モーフ内での頂点重複対策
                            {
                                OldVertex[key].m_VMO.Add(m, mo.Offset);
                            }
                            else
                            {
                                OldVertex[key].m_VMO[m] += mo.Offset;
                            }
                        }
                    }
                }

                if (m.IsUV)
                {
                    foreach (IPXUVMorphOffset mo in m.Offsets)
                    {
                        CVSet key = new CVSet(mo.Vertex);

                        if (OldVertex.ContainsKey(key))
                        {
                            if (!OldVertex[key].m_UVMO.ContainsKey(m)) // モーフ内での頂点重複対策
                            {
                                OldVertex[key].m_UVMO.Add(m, mo.Offset);
                            }
                            else
                            {
                                OldVertex[key].m_UVMO[m] += mo.Offset;
                            }
                        }
                    }
                }
            }

            // -----------------------------------------------------------元頂点の頂点情報の計算

            // 辺や元頂点の隣接情報、面点、辺の中点の計算-------------------------------
            foreach (int sf in SelectedFace)
            {
                for (int fvi = 0; fvi < 3; ++fvi)
                {
                    // 辺のキー
                    CVSet EdgeKey = new CVSet(new IPXVertex[]
                    {
                        vertex[face[3 * sf + fvi]],
                        vertex[face[3 * sf + (fvi + 1) % 3]] 
                    });//594 595 ,595 596,596 594

                    // 辺の中点の頂点情報の計算
                    if (!MidPoint.ContainsKey(EdgeKey))
                    {
                        MidPoint.Add(EdgeKey, Ifunc.CalcAverage(EdgeKey.Separate(), OldVertex, bdx));
                    }
                }

                // 丸めなしでない場合だけ隣接情報、面点を計算
                if (RBtn[2].Checked == false)
                {
                    // 面のキー
                    CVSet FaceKey = new CVSet(new IPXVertex[]
                    {
                        vertex[face[3 * sf]],
                        vertex[face[3 * sf + 1]],
                        vertex[face[3 * sf + 2]]
                    });

                    // 面点の頂点情報の計算
                    if (!FacePoint.ContainsKey(FaceKey)) // 3頂点で2つの面を作ってるモデルへの対策
                    {
                        FacePoint.Add(FaceKey, Ifunc.CalcAverage(FaceKey.Separate(), OldVertex, bdx));
                    }

                    for (int fvi = 0; fvi < 3; ++fvi)
                    {
                        // 辺のキー
                        CVSet EdgeKey = new CVSet(new IPXVertex[]
                        {
                            vertex[face[3 * sf + fvi]],
                            vertex[face[3 * sf + (fvi + 1) % 3]]
                        });

                        // "丸めのみ"and"縁は丸めない"なら辺の隣接情報は計算しない
                        if ((RBtn[1].Checked == false) || (ChBx.Checked == true))
                        {
                            // 辺の隣接情報の計算
                            if (!EdgeAdjacent.ContainsKey(EdgeKey))
                            {
                                HashSet<CVSet> value = new HashSet<CVSet>();
                                EdgeAdjacent.Add(EdgeKey, value);
                            }
                            EdgeAdjacent[EdgeKey].Add(FaceKey);
                        }

                        // 頂点のキー
                        CVSet VertexKey = new CVSet(vertex[face[3 * sf + fvi]]);

                        // 辺のキーその2
                        CVSet EdgeKey2 = new CVSet(new IPXVertex[]
                        {
                            vertex[face[3 * sf + fvi]],
                            vertex[face[3 * sf + (fvi + 2) % 3]]
                        });

                        // 元頂点の隣接情報の計算
                        if (!VertexAdjacent.ContainsKey(VertexKey))
                        {
                            CEFKeys value = new CEFKeys();
                            VertexAdjacent.Add(VertexKey, value);
                        }
                        VertexAdjacent[VertexKey].m_FaceKeys.Add(FaceKey);
                        VertexAdjacent[VertexKey].m_EdgeKeys.Add(EdgeKey);
                        VertexAdjacent[VertexKey].m_EdgeKeys.Add(EdgeKey2);
                    }
                }
            }
            // -------------------------------辺や元頂点の隣接情報、面点、辺の中点の計算
            return;
            // 新規頂点(辺)の計算,追加-------------------------------------

            // 丸めのみなら新規頂点(辺)は追加しない
            if (RBtn[1].Checked == false)
            {
                foreach (CVSet EdgeKey in MidPoint.Keys)
                {
                    // "丸めなし"or("丸めあり"and"縁の辺") : 新規頂点(辺)は辺の中点
                    if ((RBtn[2].Checked == true) || (EdgeAdjacent[EdgeKey].Count < 2))
                    {
                        EdgePoint.Add(EdgeKey, MidPoint[EdgeKey]);
                        vertex.Add(EdgePoint[EdgeKey].m_v);
                    }
                    else
                    {
                        // "丸めあり"and"縁の辺でない" : 新規頂点(辺)は辺頂点2つと隣接面点2つ(以上)の平均

                        // 平均を求めるための一時変数 (辞書とそのキー)
                        HashSet<CVSet> keyset = new HashSet<CVSet>();
                        Dictionary<CVSet, CVDat> dic = new Dictionary<CVSet, CVDat>();

                        // 辺頂点2つを一時変数に追加
                        foreach (CVSet vkey in EdgeKey.Separate())
                        {
                            keyset.Add(vkey);
                            dic.Add(vkey, OldVertex[vkey]);
                        }

                        // 辺隣接面点2つ(以上)を一時変数に追加
                        foreach (CVSet fkey in EdgeAdjacent[EdgeKey])
                        {
                            keyset.Add(fkey);
                            dic.Add(fkey, FacePoint[fkey]);
                        }

                        // 平均を求めて、追加
                        EdgePoint.Add(EdgeKey, Ifunc.CalcAverage(keyset, dic, bdx));
                        vertex.Add(EdgePoint[EdgeKey].m_v);
                    }
                }
            }
            // -------------------------------------新規頂点(辺)の計算,追加

            // 新規頂点(点)の計算,追加----------------------------------------

            foreach (CVSet VertexKey in OldVertex.Keys)
            {
                // "丸めなし"or("縁は丸めない"and"縁の点") を判定
                if ((RBtn[2].Checked == true) || ((ChBx.Checked == false) && (VertexAdjacent[VertexKey].m_EdgeKeys.Count
                                                                              != VertexAdjacent[VertexKey].m_FaceKeys
                                                                                  .Count)))
                {
                    // 新規頂点(点)は元頂点
                    VertexPoint.Add(VertexKey, OldVertex[VertexKey]);
                    vertex.Add(VertexPoint[VertexKey].m_v);
                }
                else
                {
                    // 平均を求めるための一時変数 (辞書とそのキー)
                    HashSet<CVSet> keyset = new HashSet<CVSet>();
                    Dictionary<CVSet, CVDat> dic = new Dictionary<CVSet, CVDat>();

                    // "縁の点"であるか判定
                    if (VertexAdjacent[VertexKey].m_EdgeKeys.Count
                        != VertexAdjacent[VertexKey].m_FaceKeys.Count)
                    {
                        // 新規頂点(点)は隣接辺のうちで縁にある辺の中点と元頂点の平均

                        // 元頂点を一時変数に追加
                        keyset.Add(VertexKey);
                        dic.Add(VertexKey, OldVertex[VertexKey]);

                        // 隣接辺のうちで縁にある辺を求める
                        foreach (CVSet EdgeKey in VertexAdjacent[VertexKey].m_EdgeKeys)
                        {
                            if (EdgeAdjacent[EdgeKey].Count < 2)
                            {
                                // 縁にある隣接辺の中点を一時変数に追加
                                keyset.Add(EdgeKey);
                                dic.Add(EdgeKey, MidPoint[EdgeKey]);
                            }
                        }
                    }
                    else
                    {
                        // 新規頂点(点)はCatmull-Clark法で求める

                        // (FPA+2MPA+(n-3)OV)/n を求める
                        // (FPA:隣接面点の平均, MPA:隣接辺の中点の平均, OV:元頂点, n:隣接(面or辺)数)

                        // 隣接辺の中点の平均
                        CVDat MidPointAverage =
                            Ifunc.CalcAverage(VertexAdjacent[VertexKey].m_EdgeKeys, MidPoint, bdx);
                        // 隣接面点の平均
                        CVDat FacePointAverage =
                            Ifunc.CalcAverage(VertexAdjacent[VertexKey].m_FaceKeys, FacePoint, bdx);

                        // 隣接面点の平均を一時変数に追加 (キーは適当)
                        CVSet FPAkey = new CVSet(bdx.Vertex());
                        keyset.Add(FPAkey);
                        dic.Add(FPAkey, FacePointAverage);

                        // 隣接辺の中点の平均を2つ一時変数に追加 (キーは適当)
                        CVSet MPAkey1 = new CVSet(bdx.Vertex());
                        keyset.Add(MPAkey1);
                        dic.Add(MPAkey1, MidPointAverage);
                        CVSet MPAkey2 = new CVSet(bdx.Vertex());
                        keyset.Add(MPAkey2);
                        dic.Add(MPAkey2, MidPointAverage);

                        // 元頂点を複数個一時変数に追加 (キーは適当)
                        for (int i = 0; i < VertexAdjacent[VertexKey].m_EdgeKeys.Count - 3; ++i)
                        {
                            CVSet tmpVkey = new CVSet(bdx.Vertex());
                            keyset.Add(tmpVkey);
                            dic.Add(tmpVkey, OldVertex[VertexKey]);
                        }
                    }

                    // 平均を求める
                    VertexPoint.Add(VertexKey, Ifunc.CalcAverage(keyset, dic, bdx));
                    vertex.Add(VertexPoint[VertexKey].m_v); // 新規頂点(点)の追加
                }
            }
            // ----------------------------------------新規頂点(点)の計算,追加

            // 新規頂点のモーフを設定-----------------------------------------------

            // 新規頂点(点)の分
            foreach (CVDat vdat in VertexPoint.Values)
            {
                // 頂点モーフ
                foreach (var vmo in vdat.m_VMO)
                {
                    IPXVertexMorphOffset AddMO = bdx.VertexMorphOffset();

                    AddMO.Vertex = vdat.m_v;
                    AddMO.Offset = vmo.Value;

                    vmo.Key.Offsets.Add(AddMO);
                }

                // UVモーフ
                foreach (var uvmo in vdat.m_UVMO)
                {
                    IPXUVMorphOffset AddMO = bdx.UVMorphOffset();

                    AddMO.Vertex = vdat.m_v;
                    AddMO.Offset = uvmo.Value;

                    uvmo.Key.Offsets.Add(AddMO);
                }
            }

            // 丸めのみでないなら追加した新規頂点(辺)の分を追加
            if (RBtn[1].Checked == false)
            {
                foreach (CVDat vdat in EdgePoint.Values)
                {
                    // 頂点モーフ
                    foreach (var vmo in vdat.m_VMO)
                    {
                        IPXVertexMorphOffset tmp = bdx.VertexMorphOffset();

                        tmp.Vertex = vdat.m_v;
                        tmp.Offset = vmo.Value;

                        vmo.Key.Offsets.Add(tmp);
                    }

                    // UVモーフ
                    foreach (var uvmo in vdat.m_UVMO)
                    {
                        IPXUVMorphOffset tmp = bdx.UVMorphOffset();

                        tmp.Vertex = vdat.m_v;
                        tmp.Offset = uvmo.Value;

                        uvmo.Key.Offsets.Add(tmp);
                    }
                }
            }

            // -----------------------------------------------新規頂点のモーフを設定
            // -----------------------------------------------------------------------2. [新規頂点関連]


            // 3. [新規面関連]----------------------------------------------------------------

            //    新規面の追加
            tmpmi = 0; // 計算用 : 材質のindex
            tmpcnt = material[tmpmi].Faces.Count(); // 計算用 : 材質の面数の累積値

            foreach (int sf in SelectedFace)
            {
                while (sf >= tmpcnt)
                {
                    tmpcnt += material[++tmpmi].Faces.Count;
                }

                // 選択面に用いる新規頂点(点)のキー
                CVSet VP0Keys = new CVSet(vertex[face[3 * sf]]);
                CVSet VP1Keys = new CVSet(vertex[face[3 * sf + 1]]);
                CVSet VP2Keys = new CVSet(vertex[face[3 * sf + 2]]);

                if (RBtn[1].Checked == true)
                {
                    // 丸めのみなら新規頂点(点)3つで1つの面を作る
                    IPXFace f1 = bdx.Face();

                    f1.Vertex1 = VertexPoint[VP0Keys].m_v;
                    f1.Vertex2 = VertexPoint[VP1Keys].m_v;
                    f1.Vertex3 = VertexPoint[VP2Keys].m_v;

                    CopyMaterial[material[tmpmi]].Faces.Add(f1);
                }
                else
                {
                    // 丸めのみでないなら新規頂点(辺)3つも用いて4つの面を作る
                    CVSet EP0Keys = new CVSet(new IPXVertex[]
                    {
                        vertex[face[3 * sf]],
                        vertex[face[3 * sf + 1]]
                    });
                    CVSet EP1Keys = new CVSet(new IPXVertex[]
                    {
                        vertex[face[3 * sf + 1]],
                        vertex[face[3 * sf + 2]]
                    });
                    CVSet EP2Keys = new CVSet(new IPXVertex[]
                    {
                        vertex[face[3 * sf + 2]],
                        vertex[face[3 * sf]]
                    });

                    IPXFace f1 = bdx.Face();
                    IPXFace f2 = bdx.Face();
                    IPXFace f3 = bdx.Face();
                    IPXFace f4 = bdx.Face();

                    f1.Vertex1 = VertexPoint[VP0Keys].m_v;
                    f1.Vertex2 = EdgePoint[EP0Keys].m_v;
                    f1.Vertex3 = EdgePoint[EP2Keys].m_v;

                    f2.Vertex1 = VertexPoint[VP1Keys].m_v;
                    f2.Vertex2 = EdgePoint[EP1Keys].m_v;
                    f2.Vertex3 = EdgePoint[EP0Keys].m_v;

                    f3.Vertex1 = VertexPoint[VP2Keys].m_v;
                    f3.Vertex2 = EdgePoint[EP2Keys].m_v;
                    f3.Vertex3 = EdgePoint[EP1Keys].m_v;

                    f4.Vertex1 = EdgePoint[EP0Keys].m_v;
                    f4.Vertex2 = EdgePoint[EP1Keys].m_v;
                    f4.Vertex3 = EdgePoint[EP2Keys].m_v;

                    CopyMaterial[material[tmpmi]].Faces.Add(f1);
                    CopyMaterial[material[tmpmi]].Faces.Add(f2);
                    CopyMaterial[material[tmpmi]].Faces.Add(f3);
                    CopyMaterial[material[tmpmi]].Faces.Add(f4);
                }
            }

            // ----------------------------------------------------------------3. [新規面関連]

            ////////////////////////////////////////////////////////////////////////////
            // ///////////////////////////////////////////////////////メインの処理

            update();

            MessageBox.Show("完了です。\n");
        }

        // PMX,PMD関連の取得
        void getcurrent()
        {
            // PMX関連
            pmx = connect.Pmx.GetCurrentState();
            vertex = pmx.Vertex;
            material = pmx.Material;
            morph = pmx.Morph;

            // PMD関連
            pmd = connect.Pmd.GetCurrentState();
            face = pmd.Face;
        }

        // 更新
        void update()
        {
            // PMX更新
            connect.Pmx.Update(pmx);

            // Form更新
            connect.Form.UpdateList(UpdateObject.All);

            // View更新
            connect.View.PMDView.UpdateModel();
            connect.View.PMDView.UpdateView();
        }
    }
}