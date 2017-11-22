using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using PEPlugin.Pmx;

namespace PE多功能信息处理插件
{
   public class MathHelper
    {

        public static void 点在面上的投影(Vector3 P1, Plane plane, Vector3 V0)
        {
            //http://blog.csdn.net/abcjennifer/article/details/6688080资料来源
            var t = P1 - Vector3.Dot(plane.Normal, (P1 - V0)) / Vector3.Dot(plane.Normal, plane.Normal) * plane.Normal;
            var Dis = Vector3.Distance(P1, t);
            /* var N1 = pmx.Vertex[SelectVertex[1]].Position.ToVector3() -
                      pmx.Vertex[SelectVertex[0]].Position.ToVector3();
 
             var N2 = pmx.Vertex[SelectVertex[2]].Position.ToVector3() -
                      pmx.Vertex[SelectVertex[0]].Position.ToVector3();
             var N3 = N1.Y * N2.Z - N2.Y * N1.Z;
             var n4 = Vector3.Cross(N1, N2);
             float sb, sn, sd;
             sn = -Vector3.Dot(n4, (new Vector3() - pmx.Vertex[SelectVertex[0]].Position.ToVector3()));
             sd = Vector3.Dot(n4, n4);
             sb = sn / sd;
             var vb = pmx.Vertex[SelectVertex[0]].Position.ToVector3() + sb * n4;
             var Dis = Vector3
                 .Distance(pmx.Vertex[SelectVertex[0]].Position.ToVector3(), vb);*/
            /*  float NX = (float) ((double) point2.X - (double) point1.X);
              float NY = (float) ((double) point2.Y - (double) point1.Y);
              float NZ = (float) ((double) point2.Z - (double) point1.Z);
              float MX = (float) ((double) point3.X - (double) point1.X);
              float MY = (float) ((double) point3.Y - (double) point1.Y);
              float MZ = (float) ((double) point3.Z - (double) point1.Z);
              float X1 = (float) ((double) MZ * (double) NY - (double) MY * (double) NZ);
              float Y1 = (float) ((double) MX * (double) NZ - (double) MZ * (double) NX);
              float Z1 = (float) ((double) MY * (double) NX - (double) MX * (double) NY); //Cross
              double y1_1 = (double) Y1;
              double x1_1 = (double) X1;
              double z1_1 = (double) Z1;
              float num13 = (float) 1.0;
              double x1_2 = x1_1;
              double num15 = x1_2 * x1_2;
              double y1_2 = y1_1;
              double num17 = num15 + y1_2 * y1_2;
              double z1_2 = z1_1;
              float num19 = (float) ((double) 1 / (double) ((float) Math.Sqrt(x1_2 * x1_2 + y1_2 * y1_2 + z1_2 * z1_2)));
              float num20 = (float) ((double) num19 * (double) X1);
              this.Normal.X = num20;
              float num21 = (float) ((double) num19 * (double) Y1);
              this.Normal.Y = num21;
              float num22 = (float) ((double) num19 * (double) Z1);
              this.Normal.Z = num22;
              this.D = (float) (-(float) ((double) point1.Y * (double) num21 + (double) point1.X * (double) num20 +
                                          (double) point1.Z * (double) num22));*/
        }

        /// <summary>
        /// 求一条直线与平面的交点
        /// </summary>
        /// <param name="SelectFace">选中的面</param>
        /// <param name="SelectVertex">选中的顶点</param>
        /// <returns>返回交点坐标，长度为3</returns>
        public static Vector3 直线与平面的交点(IPXVertex SelectVertex, IPXFace SelectFace)
        {
            //http://blog.csdn.net/abcjennifer/article/details/6688080资料来源
            var TempPlane = new Plane(SelectFace.Vertex1.Position.ToVector3(), SelectFace.Vertex2.Position.ToVector3(),
                SelectFace.Vertex3.Position.ToVector3());

            var t = ((SelectFace.Vertex1.Position.X - SelectVertex.Position.X) * TempPlane.Normal.X +
                     (SelectFace.Vertex1.Position.Y - SelectVertex.Position.Y) * TempPlane.Normal.Y +
                     (SelectFace.Vertex1.Position.Z - SelectVertex.Position.Z) * TempPlane.Normal.Z) /
                    Vector3.Dot(SelectVertex.Normal, TempPlane.Normal);
            return new Vector3(SelectVertex.Position.X + SelectVertex.Normal.X * t,
                SelectVertex.Position.Y + SelectVertex.Normal.Y * t,
                SelectVertex.Position.Z + SelectVertex.Normal.Z * t);
        }

        public static bool 点是否在三角平面内判定(Vector3 SelectVertex, IPXFace SelectFace)
        {
            //https://www.cnblogs.com/graphics/archive/2010/08/05/1793393.html资料来源
            Vector3 v0 = SelectFace.Vertex3.Position.ToVector3() - SelectFace.Vertex1.Position.ToVector3();
            Vector3 v1 = SelectFace.Vertex2.Position.ToVector3() - SelectFace.Vertex1.Position.ToVector3();
            Vector3 v2 = SelectVertex - SelectFace.Vertex1.Position.ToVector3();
            float dot00 = Vector3.Dot(v0, v0);
            float dot01 = Vector3.Dot(v0, v1);
            float dot02 = Vector3.Dot(v0, v2);
            float dot11 = Vector3.Dot(v1, v1);
            float dot12 = Vector3.Dot(v1, v2);
            float inverDeno = 1 / (dot00 * dot11 - dot01 * dot01);
            float u = (dot11 * dot02 - dot01 * dot12) * inverDeno;
            if (u < 0 || u > 1)
            {
                return false;
            }
            float v = (dot00 * dot12 - dot01 * dot02) * inverDeno;
            if (v < 0 || v > 1)
            {
                return false;
            }
            return u + v <= 1;
        }
    }
}
