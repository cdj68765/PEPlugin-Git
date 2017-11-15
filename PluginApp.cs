using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using PEPlugin.Pmx;
using PEPlugin.SDX;
using PXCPlugin;
using PXCPlugin.Event;
using PXCPlugin.UIModel;
using SlimDX;
using static PXCPlugin.Event.PXEventArgs;

namespace PE多功能信息处理插件
{
    // Token: 0x02000003 RID: 3
    public sealed  class MeasureClass : PXCPluginClass
    {
        // Token: 0x17000002 RID: 2
        // (get) Token: 0x06000003 RID: 3 RVA: 0x00002075 File Offset: 0x00000275
  

        // Token: 0x06000005 RID: 5 RVA: 0x00002084 File Offset: 0x00000284
        public MeasureClass()
        {
            var   args = Program.ARGS.Host.Connector.System.GetCPluginRunArgsClone();
          base.Run(args);
            try
            {
                if (this.m_uim != null)
                {
                    this.m_uim.Visible = true;
                    this.m_text_uim.Visible = true;
                }
                else
                {
                    this.m_view = PXCBridge.ViewCtrl(args.Connector);//注册视图
                    this.m_ev = PXCBridge.CreateEventConnector(args.Connector);//这个月车事件
                 var VL=   m_ev.CreateViewEventListener();
                    VL.ObjectSelected +=(s,o)=>
                    {
                    };
                    this.m_unit = 0;
                    this.m_curTarget = 0;
                    IPXPmx ipxpmx = base.m_bld.Pmx();
                    using (var memoryStream = new FileStream("measure_uim.pmx", FileMode.Open))
                    {
                        ipxpmx.FromStream(memoryStream);
                    }
                    ipxpmx.Material[1].Diffuse.A = 0.9f;
                    ipxpmx.Material[2].Diffuse.A = 0.9f;
                    this.m_uim = PXCBridge.RegisterUIModel(args.Connector, ipxpmx, "LMeasure", null, true, true);//注册对象
                    this.m_listener1 = this.m_uim.CreateEventListener(this.m_ev, new int[]
                    {
                       2
                    });//监听材质2
                    this.m_listener2 = this.m_uim.CreateEventListener(this.m_ev, new int[]
                    {
                        1
                    });//监听材质1,主要影响鼠标指向材质的响应，控制还是依靠骨骼
                    this.SetMouseDragMove(this.m_uim, this.m_listener1, this.m_view, 0);//设置材质2和骨骼1的绑定以及事件
                    this.SetMouseDragMove(this.m_uim, this.m_listener2, this.m_view, 1);//设置材质1和骨骼2的绑定以及事件
                    this.m_initPos0 = new V3(-10f, 20f, 0f);
                    this.m_initPos1 = new V3(-10f, 0f, 0f);
                    this.m_uim.SetBoneTranslate(0, this.m_initPos0);
                    this.m_uim.SetBoneTranslate(1, this.m_initPos1);
                    this.m_uim.UpdateTransform();
                     V3 v = new V3(0.9f, 0.1f, 0.1f);
                      PXUIModelHelper.MaterialColorEvPara materialColorEvPara =
                          new PXUIModelHelper.MaterialColorEvPara(2, ipxpmx)
                          {
                              EnableDiffuse = true,
                              EnableAmbient = true
                          };
                      materialColorEvPara.DiffuseB = new V4(v, materialColorEvPara.DiffuseA.A);
                      materialColorEvPara.AmbientB = v;
                      PXUIModelHelper.MaterialColorEvPara materialColorEvPara2 =
                          new PXUIModelHelper.MaterialColorEvPara(1, ipxpmx)
                          {
                              EnableDiffuse = true,
                              EnableAmbient = true
                          };
                      materialColorEvPara2.DiffuseB = new V4(v, materialColorEvPara2.DiffuseA.A);
                      materialColorEvPara2.AmbientB = v;
                      PXUIModelHelper.SetMouseOverColor(this.m_uim, this.m_listener1, materialColorEvPara);
                      PXUIModelHelper.SetMouseOverColor(this.m_uim, this.m_listener2, materialColorEvPara2);
                    this.SetEvent_AxisFit();//设置双击操作事件
                    ipxpmx.Clear();
                    using (var memoryStream2 = new FileStream("measure_text_uim.pmx", FileMode.Open))
                    {
                        ipxpmx.FromStream(memoryStream2);
                    }
                    ipxpmx.Material[0].Diffuse.A = 0.9f;
                    this.m_text_uim = PXCBridge.RegisterUIModel(args.Connector, ipxpmx, "MeasureText", null, true);//新对象必须注册
                    this.m_text_uim.SetBillboard(1);//0，不朝向，1时刻朝向你，2朝向平行方向的你

                    this.ClearCol0 = Color.FromArgb(220, 220, 220);
                    this.m_textCtrl = PXUIModelHelper.CreateTextControl(this.m_text_uim, 0, 56, 18, this.ClearCol0);
                    this.SetDistanceText();
                }
            }
            catch (Exception)
            {
            }
        }

        // Token: 0x06000006 RID: 6 RVA: 0x00002474 File Offset: 0x00000674
        public override void Dispose()
        {
            base.Dispose();
            this.Release();
        }

        // Token: 0x06000007 RID: 7 RVA: 0x00002484 File Offset: 0x00000684
        private void Release()
        {
            if (this.m_textCtrl != null)
            {
                this.m_textCtrl.Dispose();
                this.m_textCtrl = null;
            }
            if (this.m_listener1 != null)
            {
                this.m_uim.ReleaseEventListener(this.m_listener1);
                this.m_listener1 = null;
            }
            if (this.m_listener2 != null)
            {
                this.m_uim.ReleaseEventListener(this.m_listener2);
                this.m_listener2 = null;
            }
            if (this.m_text_uim != null)
            {
                this.m_text_uim.Release();
                this.m_text_uim = null;
            }
            if (this.m_uim != null)
            {
                this.m_uim.Release();
                this.m_uim = null;
            }
            if (this.m_ev != null)
            {
                PXCBridge.ReleaseEventConnector(this.m_ev);
                this.m_ev = null;
            }
        }

        // Token: 0x06000008 RID: 8 RVA: 0x00002674 File Offset: 0x00000874
        private void SetMouseDragMove(IPXUIModel uim, IPXUIModelEventListener listener, IPXViewControl viewCtrl, int bx)
        {
            V3 st = Vector3.Zero;
            V3 vp = Vector3.Zero;
            listener.MouseDown += delegate (object sender, PXEventArgs.UIModelMouse e)
            {
                try
                {
                    if (e.Button == MouseButtons.Left)
                    {
                       st = uim.GetTransformedBonePosition(bx);
                        vp = viewCtrl.VCursorPosition;
                        viewCtrl.VCursorPosition = st;
                    }
                }
                catch (Exception)
                {
                }
            };
            listener.MouseDrag += delegate (object sender, PXEventArgs.UIModelMouseDrag e)
            {
                try
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        V3 v = e.VDrag + st;//鼠标按下位置和当前指针所在位置的和
                        this.m_uim.SetBoneTranslate(bx, v);
                        uim.UpdateTransform();//立即更新对象状态
                        this.SetDistanceText();
                    }
                }
                catch (Exception)
                {
                }
            };
            if (viewCtrl != null)
            {
                listener.MouseDragEnd += delegate (object sender, PXEventArgs.UIModelMouseDrag e)
                {
                    try
                    {
                        if (e.Button == MouseButtons.Left)
                        {
                            viewCtrl.VCursorPosition = vp;
                        }
                    }
                    catch (Exception)
                    {
                    }
                };
            }
        }

        // Token: 0x06000009 RID: 9 RVA: 0x0000270C File Offset: 0x0000090C
        private void SetDistanceText()
        {
            if (this.m_textCtrl == null)
            {
                return;
            }
            V3 transformedBonePosition = this.m_uim.GetTransformedBonePosition(0);
            V3 transformedBonePosition2 = this.m_uim.GetTransformedBonePosition(1);
            float num = Vector3.Distance(transformedBonePosition, transformedBonePosition2);
            string format = "##.000";
            if (this.m_unit == 1)
            {
                num *= 8f;
                format = "##.00";
                this.m_textCtrl.BackColor = this.ClearCol1;
            }
            else
            {
                this.m_textCtrl.BackColor = this.ClearCol0;
            }
            this.m_textCtrl.Clear();
            this.m_textCtrl.DrawText(num.ToString(format), null, null);
            this.m_textCtrl.UpdateTextImage();
            M world = this.m_text_uim.GetWorld();
            world.M41 = transformedBonePosition.X;
            world.M42 = transformedBonePosition.Y;
            world.M43 = transformedBonePosition.Z;
            this.m_text_uim.SetWorld(world);
        }

        // Token: 0x0600000A RID: 10 RVA: 0x0000286C File Offset: 0x00000A6C
        private void SetEvent_AxisFit()
        {
            this.m_listener1.MouseDoubleClick += delegate (object sender, PXEventArgs.UIModelMouse e)
            {
                try
                {
                    if (e.Button == MouseButtons.Left)
                    {
                       this.AxisFit(0);
                    }
                }
                catch (Exception)
                {
                }
            };
            this.m_listener2.MouseDoubleClick += delegate (object sender, PXEventArgs.UIModelMouse e)
            {
                try
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        this.AxisFit(1);
                    }
                }
                catch (Exception)
                {
                }
            };
        }

        // Token: 0x0600000B RID: 11 RVA: 0x0000289C File Offset: 0x00000A9C
        private void AxisFit(int target = 0)//重置坐标轴
        {
            Vector3 vector = this.m_uim.GetTransformedBonePosition(0);
            Vector3 vector2 = this.m_uim.GetTransformedBonePosition(1);
            float num = vector.X - vector2.X;
            num *= num;
            float num2 = vector.Y - vector2.Y;
            num2 *= num2;
            float num3 = vector.Z - vector2.Z;
            num3 *= num3;
            if (num2 < num && num3 < num)
            {
                if (target == 0)
                {
                    vector.Y = vector2.Y;
                    vector.Z = vector2.Z;
                    this.m_uim.SetBoneTranslate(0, vector);
                }
                else
                {
                    vector2.Y = vector.Y;
                    vector2.Z = vector.Z;
                    this.m_uim.SetBoneTranslate(1, vector2);
                }
            }
            else if (num3 < num2)
            {
                if (target == 0)
                {
                    vector.X = vector2.X;
                    vector.Z = vector2.Z;
                    this.m_uim.SetBoneTranslate(0, vector);
                }
                else
                {
                    vector2.X = vector.X;
                    vector2.Z = vector.Z;
                    this.m_uim.SetBoneTranslate(1, vector2);
                }
            }
            else if (target == 0)
            {
                vector.Y = vector2.Y;
                vector.X = vector2.X;
                this.m_uim.SetBoneTranslate(0, vector);
            }
            else
            {
                vector2.Y = vector.Y;
                vector2.X = vector.X;
                this.m_uim.SetBoneTranslate(1, vector2);
            }
            this.m_uim.UpdateTransform();
            this.SetDistanceText();
        }


        // Token: 0x04000001 RID: 1
        private IPXViewControl m_view;

        // Token: 0x04000002 RID: 2
        private IPXEventConnector m_ev;

        // Token: 0x04000003 RID: 3
        private IPXUIModel m_uim;

        // Token: 0x04000004 RID: 4
        private IPXUIModel m_text_uim;

        // Token: 0x04000005 RID: 5
        private IPXUIModelEventListener m_listener1;

        // Token: 0x04000006 RID: 6
        private IPXUIModelEventListener m_listener2;

        // Token: 0x04000007 RID: 7
        private PXUIModelHelper.TextControl m_textCtrl;


        // Token: 0x04000009 RID: 9
        private int m_curTarget;

        // Token: 0x0400000A RID: 10
        private int m_unit;

        // Token: 0x0400000B RID: 11
        private Color ClearCol0;

        // Token: 0x0400000C RID: 12
        private Color ClearCol1;

        // Token: 0x0400000D RID: 13
        private V3 m_initPos0;

        // Token: 0x0400000E RID: 14
        private V3 m_initPos1;
    }
}
