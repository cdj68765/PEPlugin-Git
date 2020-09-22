using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Serialization.Formatters;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using Newtonsoft.Json;
using PEPlugin;
using PEPlugin.Pmx;
using PEPlugin.SDX;
using UniGLTF;
using UniJSON;
using UnityEngine;

namespace PE多功能信息处理插件
{
    public class Class3 : IPEImportPlugin
    {
        public string Ext => ".vrm";

        public string Caption => "VRM格式";

        public class DateToSave
        {
            public List<V3> posion = new List<V3>();
            public List<V3> normals = new List<V3>();
            public List<V3> uv = new List<V3>();
            public List<int> tri = new List<int>();

            public class V3
            {
                public float x;
                public float y;
                public float z;
            }
        }

        public IPXPmx Import(string path, IPERunArgs args)
        {
            var pmx2 = PEStaticBuilder.Pmx.Pmx();
            {
                SlimDX.Vector3 ChangeTOV32(float x, float y, float z)
                {
                    return new SlimDX.Vector3(x, y, z);
                }
                SlimDX.Vector2 ChangeTOV22(float x, float y)
                {
                    return new SlimDX.Vector2(x, y);
                }

                XmlSerializer binFormat = new XmlSerializer(typeof(List<DateToSave>)); //把对象类型作为参数
                List<DateToSave> Date = new List<DateToSave>();

                using (Stream fStream = File.OpenRead(@"C:\Users\cdj68\Desktop\VRM相关\じぇねりっく長波さま\長波さま.vrm"))
                {
                    Date = binFormat.Deserialize(fStream) as List<DateToSave>;
                }
                foreach (var VARIABLE in Date)
                {
                    var VerList = new List<IPXVertex>();
                    for (int i = 0; i < VARIABLE.posion.Count; i++)
                    {
                        var _Ver = args.Host.Builder.Pmx.Vertex();
                        _Ver.Position = new V3(VARIABLE.posion[i].x, VARIABLE.posion[i].y,
                            VARIABLE.posion[i].z);
                        _Ver.Normal = ChangeTOV32(VARIABLE.normals[i].x, VARIABLE.normals[i].y,
                            VARIABLE.normals[i].z);
                        _Ver.UV = ChangeTOV22(VARIABLE.uv[i].x, VARIABLE.uv[i].y);
                        VerList.Add(_Ver);
                        pmx2.Vertex.Add(_Ver);
                    }
                    var m = args.Host.Builder.Pmx.Material();
                    for (int j = 0; j < VARIABLE.tri.Count();)
                    {
                        var Fac = args.Host.Builder.Pmx.Face();
                        Fac.Vertex1 = VerList[VARIABLE.tri[j]];
                        Interlocked.Increment(ref j);
                        Fac.Vertex2 = VerList[VARIABLE.tri[j]];
                        Interlocked.Increment(ref j);
                        Fac.Vertex3 = VerList[VARIABLE.tri[j]];
                        Interlocked.Increment(ref j);
                        m.Faces.Add(Fac);
                    }
                    m.Diffuse = new V4(2, 2, 2, 1);
                    m.Ambient = new V3(1, 1, 1);
                    m.Specular = new V3(3, 3, 3);
                    pmx2.Material.Add(m);
                }
            }

            SlimDX.Vector3 ChangeTOV3(Vector3 v3)
            {
                return new SlimDX.Vector3(-v3.x, v3.y, -v3.z);
            }

            SlimDX.Vector2 ChangeTOV2(Vector2 v2)
            {
                return new SlimDX.Vector2(v2.x, -v2.y);
            }
            var pmx = PEStaticBuilder.Pmx.Pmx();
            var _chunks = ParseGlbChanks(
                File.ReadAllBytes(path));
            var jsonBytes = _chunks[0].Bytes;
            var glTF = ParseJson(Encoding.UTF8.GetString(jsonBytes.Array, jsonBytes.Offset, jsonBytes.Count),
                new SimpleStorage(_chunks[1].Bytes));
            var TexDic = new Dictionary<int, string>();
            {
                for (int i = 0; i < glTF.textures.Count; ++i)
                {
                    var imageIndex = glTF.GetImageIndexFromTextureIndex(i);
                    var m_imageBytes = ToArray(glTF.GetImageBytes(new SimpleStorage(_chunks[1].Bytes), imageIndex,
                        out string m_textureName));
                    TexDic.Add(i, m_textureName);
                    File.WriteAllBytes($"{Path.GetDirectoryName(path)}\\{m_textureName}", m_imageBytes);
                }

                Byte[] ToArray(ArraySegment<byte> bytes)
                {
                    if (bytes.Array == null)
                    {
                        return new byte[] { };
                    }
                    else if (bytes.Offset == 0 && bytes.Count == bytes.Array.Length)
                    {
                        return bytes.Array;
                    }
                    else
                    {
                        return bytes.Array.Skip(bytes.Offset).Take(bytes.Count).ToArray();
                    }
                }
            }

            #region 材质

            {
                for (int i = 0; i < glTF.meshes.Count; ++i)
                {
                    var gltfMesh = glTF.meshes[i];
                    glTFAttributes lastAttributes = null;
                    var sharedAttributes = true;
                    foreach (var prim in gltfMesh.primitives)
                    {
                        if (lastAttributes != null && !prim.attributes.Equals(lastAttributes))
                        {
                            sharedAttributes = false;
                            break;
                        }

                        lastAttributes = prim.attributes;
                    }

                    var meshContext = sharedAttributes
                            ? _ImportMeshSharingVertexBuffer(gltfMesh)
                            : _ImportMeshIndependentVertexBuffer(gltfMesh)
                        ;
                    meshContext.name = gltfMesh.name;
                    IPXPmxBuilder bdx = args.Host.Builder.Pmx;
                    var VerList = new List<IPXVertex>();
                    for (int j = 0; j < meshContext.positions.Count(); j++)
                    {
                        var Ver = bdx.Vertex();
                        Ver.Position = ChangeTOV3(meshContext.positions[j]);
                        Ver.Normal = ChangeTOV3(meshContext.normals[j]);
                        Ver.UV = ChangeTOV2(meshContext.uv[j]);
                        VerList.Add(Ver);
                        pmx.Vertex.Add(Ver);
                    }

                    var m = bdx.Material();
                    for (int j = 0; j < meshContext.subMeshes[0].Count();)
                    {
                        var Fac = bdx.Face();
                        Fac.Vertex1 = VerList[meshContext.subMeshes[0][j]];
                        Interlocked.Increment(ref j);
                        Fac.Vertex2 = VerList[meshContext.subMeshes[0][j]];
                        Interlocked.Increment(ref j);
                        Fac.Vertex3 = VerList[meshContext.subMeshes[0][j]];
                        Interlocked.Increment(ref j);
                        m.Faces.Add(Fac);
                    }

                    m.Tex = TexDic[glTF.extensions.VRM.materialProperties[i].textureProperties.First().Value];
                    m.Name = glTF.materials[i].name;
                    m.Diffuse = new V4(2, 2, 2, 1);
                    m.Ambient = new V3(1, 1, 1);
                    m.Specular = new V3(3, 3, 3);
                    pmx.Material.Add(m);
                }
                bool IsGeneratedUniGLTFAndOlderThan(string generatorVersion, int major, int minor)
                {
                    if (String.IsNullOrEmpty(generatorVersion)) return false;
                    if (generatorVersion == "UniGLTF") return true;
                    if (!generatorVersion.StartsWith("UniGLTF-")) return false;

                    try
                    {
                        var index = generatorVersion.IndexOf('.');
                        var generatorMajor = Int32.Parse(generatorVersion.Substring(8, index - 8));
                        var generatorMinor = Int32.Parse(generatorVersion.Substring(index + 1));

                        if (generatorMajor < major)
                        {
                            return true;
                        }
                        else
                        {
                            if (generatorMinor >= minor)
                            {
                                return false;
                            }
                            else
                            {
                                return true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarningFormat("{0}: {1}", generatorVersion, ex);
                        return false;
                    }
                }
                bool IsGeneratedUniGLTFAndOlder(int major, int minor)
                {
                    if (glTF == null) return false;
                    if (glTF.asset == null) return false;
                    return IsGeneratedUniGLTFAndOlderThan(glTF.asset.generator, major, minor);
                }
                glTF.MeshContext _ImportMeshSharingVertexBuffer(glTFMesh gltfMesh)
                {
                    var context = new glTF.MeshContext();

                    T[] SelectInplace<T>(T[] src, Func<T, T> pred)
                    {
                        for (int i = 0; i < src.Length; ++i)
                        {
                            src[i] = pred(src[i]);
                        }

                        return src;
                    }

                    {
                        var prim = gltfMesh.primitives.First();
                        context.positions = glTF.GetArrayFromAccessor<Vector3>(prim.attributes.POSITION);
                        SelectInplace(context.positions, x => x.ReverseZ());
                        // normal
                        if (prim.attributes.NORMAL != -1)
                        {
                            context.normals = glTF.GetArrayFromAccessor<Vector3>(prim.attributes.NORMAL);
                            SelectInplace(context.normals, x => x.ReverseZ());
                        }

                        // tangent
                        if (prim.attributes.TANGENT != -1)
                        {
                            context.tangents = glTF.GetArrayFromAccessor<Vector4>(prim.attributes.TANGENT);
                            SelectInplace(context.tangents, x => x.ReverseZ());
                        }

                        // uv
                        if (prim.attributes.TEXCOORD_0 != -1)
                        {
                            if (IsGeneratedUniGLTFAndOlder(1, 16))
                            {
#pragma warning disable 0612
                                // backward compatibility
                                context.uv = glTF.GetArrayFromAccessor<Vector2>(prim.attributes.TEXCOORD_0);
                                SelectInplace(context.uv, x => x.ReverseY());
#pragma warning restore 0612
                            }
                            else
                            {
                                context.uv = glTF.GetArrayFromAccessor<Vector2>(prim.attributes.TEXCOORD_0);
                                SelectInplace(context.uv, x => x.ReverseY());
                            }
                        }
                        else
                        {
                            // for inconsistent attributes in primitives
                            context.uv = new Vector2[context.positions.Length];
                        }

                        // color
                        if (prim.attributes.COLOR_0 != -1)
                        {
                            context.colors = glTF.GetArrayFromAccessor<Color>(prim.attributes.COLOR_0);
                        }

                        // skin
                        if (prim.attributes.JOINTS_0 != -1 && prim.attributes.WEIGHTS_0 != -1)
                        {
                            var joints0 = glTF.GetArrayFromAccessor<glTF.UShort4>(prim.attributes.JOINTS_0); // uint4
                            var weights0 = glTF.GetArrayFromAccessor<glTF.Float4>(prim.attributes.WEIGHTS_0);
                            for (int i = 0; i < weights0.Length; ++i)
                            {
                                weights0[i] = weights0[i].One();
                            }

                            for (int j = 0; j < joints0.Length; ++j)
                            {
                                var bw = new BoneWeight();

                                bw.boneIndex0 = joints0[j].x;
                                bw.weight0 = weights0[j].x;

                                bw.boneIndex1 = joints0[j].y;
                                bw.weight1 = weights0[j].y;

                                bw.boneIndex2 = joints0[j].z;
                                bw.weight2 = weights0[j].z;

                                bw.boneIndex3 = joints0[j].w;
                                bw.weight3 = weights0[j].w;

                                context.boneWeights.Add(bw);
                            }
                        }

                        // blendshape
                        if (prim.targets != null && prim.targets.Count > 0)
                        {
                            context.blendShapes.AddRange(prim.targets.Select((x, i) => new glTF.MeshContext.BlendShape(
                                i < prim.extras.targetNames.Count && !String.IsNullOrEmpty(prim.extras.targetNames[i])
                                    ? prim.extras.targetNames[i]
                                    : i.ToString())));

                            for (int i = 0; i < prim.targets.Count; ++i)
                            {
                                //var name = string.Format("target{0}", i++);
                                var primTarget = prim.targets[i];
                                var blendShape = context.blendShapes[i];

                                void Assign<T>(List<T> dst, T[] src, Func<T, T> pred)
                                {
                                    dst.Capacity = src.Length;
                                    dst.AddRange(src.Select(pred));
                                }

                                if (primTarget.POSITION != -1)
                                {
                                    Assign(blendShape.Positions,
                                        glTF.GetArrayFromAccessor<Vector3>(primTarget.POSITION), x => x.ReverseZ());
                                }

                                if (primTarget.NORMAL != -1)
                                {
                                    Assign(blendShape.Normals,

                                        glTF.GetArrayFromAccessor<Vector3>(primTarget.NORMAL), x => x.ReverseZ());
                                }

                                if (primTarget.TANGENT != -1)
                                {
                                    Assign(blendShape.Tangents,

                                        glTF.GetArrayFromAccessor<Vector3>(primTarget.TANGENT), x => x.ReverseZ());
                                }
                            }
                        }
                    }

                    foreach (var prim in gltfMesh.primitives)
                    {
                        if (prim.indices == -1)
                        {
                            context.subMeshes.Add(glTF.TriangleUtil
                                .FlipTriangle(Enumerable.Range(0, context.positions.Length)).ToArray());
                        }
                        else
                        {
                            var indices = glTF.GetIndices(prim.indices);
                            context.subMeshes.Add(indices);
                        }

                        // material
                        context.materialIndices.Add(prim.material);
                    }

                    return context;
                }
                glTF.MeshContext _ImportMeshIndependentVertexBuffer(glTFMesh gltfMesh)
                {
                    //Debug.LogWarning("_ImportMeshIndependentVertexBuffer");

                    var targets = gltfMesh.primitives[0].targets;
                    for (int i = 1; i < gltfMesh.primitives.Count; ++i)
                    {
                        if (!gltfMesh.primitives[i].targets.SequenceEqual(targets))
                        {
                            throw new NotImplementedException(String.Format("diffirent targets: {0} with {1}",
                                gltfMesh.primitives[i],
                                targets));
                        }
                    }

                    var positions = new List<Vector3>();
                    var normals = new List<Vector3>();
                    var tangents = new List<Vector4>();
                    var uv = new List<Vector2>();
                    var colors = new List<Color>();
                    var meshContext = new glTF.MeshContext();
                    foreach (var prim in gltfMesh.primitives)
                    {
                        var indexOffset = positions.Count;
                        var indexBuffer = prim.indices;

                        var positionCount = positions.Count;
                        positions.AddRange(glTF.GetArrayFromAccessor<Vector3>(prim.attributes.POSITION)
                            .Select(x => x.ReverseZ()));
                        positionCount = positions.Count - positionCount;

                        // normal
                        if (prim.attributes.NORMAL != -1)
                        {
                            normals.AddRange(glTF.GetArrayFromAccessor<Vector3>(prim.attributes.NORMAL)
                                .Select(x => x.ReverseZ()));
                        }

                        if (prim.attributes.TANGENT != -1)
                        {
                            tangents.AddRange(glTF.GetArrayFromAccessor<Vector4>(prim.attributes.TANGENT)
                                .Select(x => x.ReverseZ()));
                        }

                        // uv
                        if (prim.attributes.TEXCOORD_0 != -1)
                        {
                            if (IsGeneratedUniGLTFAndOlder(1, 16))
                            {
#pragma warning disable 0612
                                // backward compatibility
                                uv.AddRange(glTF.GetArrayFromAccessor<Vector2>(prim.attributes.TEXCOORD_0)
                                    .Select(x => x.ReverseY()));
#pragma warning restore 0612
                            }
                            else
                            {
                                uv.AddRange(glTF.GetArrayFromAccessor<Vector2>(prim.attributes.TEXCOORD_0)
                                    .Select(x => x.ReverseUV()));
                            }
                        }
                        else
                        {
                            // for inconsistent attributes in primitives
                            uv.AddRange(new Vector2[positionCount]);
                        }

                        // color
                        if (prim.attributes.COLOR_0 != -1)
                        {
                            colors.AddRange(glTF.GetArrayFromAccessor<Color>(prim.attributes.COLOR_0));
                        }

                        // skin
                        if (prim.attributes.JOINTS_0 != -1 && prim.attributes.WEIGHTS_0 != -1)
                        {
                            var joints0 = glTF.GetArrayFromAccessor<glTF.UShort4>(prim.attributes.JOINTS_0); // uint4
                            var weights0 = glTF.GetArrayFromAccessor<glTF.Float4>(prim.attributes.WEIGHTS_0)
                                .Select(x => x.One()).ToArray();

                            for (int j = 0; j < joints0.Length; ++j)
                            {
                                var bw = new BoneWeight();

                                bw.boneIndex0 = joints0[j].x;
                                bw.weight0 = weights0[j].x;

                                bw.boneIndex1 = joints0[j].y;
                                bw.weight1 = weights0[j].y;

                                bw.boneIndex2 = joints0[j].z;
                                bw.weight2 = weights0[j].z;

                                bw.boneIndex3 = joints0[j].w;
                                bw.weight3 = weights0[j].w;

                                meshContext.boneWeights.Add(bw);
                            }
                        }

                        // blendshape
                        if (prim.targets != null && prim.targets.Count > 0)
                        {
                            for (int i = 0; i < prim.targets.Count; ++i)
                            {
                                //var name = string.Format("target{0}", i++);
                                var primTarget = prim.targets[i];
                                var blendShape = new glTF.MeshContext.BlendShape(
                                        !String.IsNullOrEmpty(prim.extras.targetNames[i])
                                            ? prim.extras.targetNames[i]
                                            : i.ToString())
                                    ;
                                if (primTarget.POSITION != -1)
                                {
                                    blendShape.Positions.AddRange(
                                        glTF.GetArrayFromAccessor<Vector3>(primTarget.POSITION)
                                            .Select(x => x.ReverseZ()).ToArray());
                                }

                                if (primTarget.NORMAL != -1)
                                {
                                    blendShape.Normals.AddRange(
                                        glTF.GetArrayFromAccessor<Vector3>(primTarget.NORMAL).Select(x => x.ReverseZ())
                                            .ToArray());
                                }

                                if (primTarget.TANGENT != -1)
                                {
                                    blendShape.Tangents.AddRange(
                                        glTF.GetArrayFromAccessor<Vector3>(primTarget.TANGENT).Select(x => x.ReverseZ())
                                            .ToArray());
                                }

                                meshContext.blendShapes.Add(blendShape);
                            }
                        }

                        var indices =
                                (indexBuffer >= 0)
                                    ? glTF.GetIndices(indexBuffer)
                                    : glTF.TriangleUtil.FlipTriangle(Enumerable.Range(0, meshContext.positions.Length))
                                        .ToArray() // without index array
                            ;
                        for (int i = 0; i < indices.Length; ++i)
                        {
                            indices[i] += indexOffset;
                        }

                        meshContext.subMeshes.Add(indices);

                        // material
                        meshContext.materialIndices.Add(prim.material);
                    }

                    meshContext.positions = positions.ToArray();
                    meshContext.normals = normals.ToArray();
                    meshContext.tangents = tangents.ToArray();
                    meshContext.uv = uv.ToArray();

                    return meshContext;
                }
            }

            #endregion 材质

            #region 骨骼

            {
                /*   foreach (var x in glTF.nodes)
                   {
                    var Ret=   ImportNode(x).transform;
                   }
                     GameObject ImportNode(glTFNode node)
                   {
                       var nodeName = node.name;
                       if (!String.IsNullOrEmpty(nodeName) && nodeName.Contains("/"))
                       {
                           Debug.LogWarningFormat("node {0} contains /. replace _", node.name);
                           nodeName = nodeName.Replace("/", "_");
                       }
                       var go = new GameObject(nodeName);
                       if (node.translation != null && node.translation.Length > 0)
                       {
                           go.transform.localPosition = new Vector3(
                               node.translation[0],
                               node.translation[1],
                               node.translation[2]);
                       }
                       if (node.rotation != null && node.rotation.Length > 0)
                       {
                           go.transform.localRotation = new Quaternion(
                               node.rotation[0],
                               node.rotation[1],
                               node.rotation[2],
                               node.rotation[3]);
                       }
                       if (node.scale != null && node.scale.Length > 0)
                       {
                           go.transform.localScale = new Vector3(
                               node.scale[0],
                               node.scale[1],
                               node.scale[2]);
                       }
                       if (node.matrix != null && node.matrix.Length > 0)
                       {
                           var m = UnityExtensions.MatrixFromArray(node.matrix);
                           go.transform.localRotation = m.ExtractRotation();
                           go.transform.localPosition = m.ExtractPosition();
                           go.transform.localScale = m.ExtractScale();
                       }
                       return go;
                   }
                   IPXPmxBuilder bdx = args.Host.Builder.Pmx;
                   for (int i = 0; i < glTF.extensions.VRM.humanoid.humanBones.Count; i++)
                   {
                       var Bone = bdx.Bone();
                       var TempBone = glTF.nodes[glTF.extensions.VRM.humanoid.humanBones[i].node];
                       Bone.Name = TempBone.name;
                       Bone.Position = new V3(TempBone.translation[0], TempBone.translation[1],
                           TempBone.translation[2]);
                       pmx.Bone.Add(Bone);
                   }*/

                /*    for (int i = 0; i < glTF.nodes.Count; i++)
                    {
                        var Bone = bdx.Bone();
                        var TempBone = glTF.nodes[i];
                        Bone.Name = TempBone.name;
                        Bone.Position = new PEPlugin.SDX.V3(TempBone.translation[0], TempBone.translation[1],
                            TempBone.translation[2]);
                        pmx.Bone.Add(Bone);
                    }*/

                /*  for (int i = 0; i < glTF.nodes.Count; i++)
                  {
                      var TempBone = glTF.nodes[i];
                      if (TempBone.children != null)
                      {
                          pmx.Bone[i].ToBone = pmx.Bone[TempBone.children.First()];
                          foreach (var VARIABLE in TempBone.children)
                          {
                              pmx.Bone[VARIABLE].Parent = pmx.Bone[i];
                          }
                      }
                  }*/
            }

            #endregion 骨骼

            return pmx2;
        }

        public static IEnumerable<Transform> Traverse(Transform t)
        {
            yield return t;
            foreach (Transform x in t)
            {
                foreach (Transform y in x.Traverse())
                {
                    yield return y;
                }
            }
        }

        private string GLB_MAGIC = "glTF";
        private float GLB_VERSION = 2.0f;

        private List<glTF.GlbChunk> ParseGlbChanks(byte[] bytes)
        {
            if (bytes.Length == 0) throw new Exception("empty bytes");

            var pos = 0;
            if (Encoding.ASCII.GetString(bytes, 0, 4) != GLB_MAGIC) throw new Exception("invalid magic");

            pos += 4;

            var version = BitConverter.ToUInt32(bytes, pos);
            if (version != GLB_VERSION)
            {
                Console.WriteLine("unknown version: {0}", version);
                return null;
            }

            pos += 4;

            //var totalLength = BitConverter.ToUInt32(bytes, pos);
            pos += 4;

            var chunks = new List<glTF.GlbChunk>();
            while (pos < bytes.Length)
            {
                var chunkDataSize = BitConverter.ToInt32(bytes, pos);
                pos += 4;

                //var type = (GlbChunkType)BitConverter.ToUInt32(bytes, pos);
                var chunkTypeBytes = bytes.Skip(pos).Take(4).Where(x => x != 0).ToArray();
                var chunkTypeStr = Encoding.ASCII.GetString(chunkTypeBytes);
                var type = ToChunkType(chunkTypeStr);

                glTF.GlbChunkType ToChunkType(string src)
                {
                    switch (src)
                    {
                        case "BIN":
                            return glTF.GlbChunkType.BIN;

                        case "JSON":
                            return glTF.GlbChunkType.JSON;

                        default:
                            throw new FormatException("unknown chunk type: " + src);
                    }
                }

                pos += 4;

                chunks.Add(new glTF.GlbChunk
                {
                    ChunkType = type,
                    Bytes = new ArraySegment<byte>(bytes, pos, chunkDataSize)
                });

                pos += chunkDataSize;
            }

            return chunks;
        }

        private glTF ParseJson(string json, IStorage storage)
        {
            var GLTF = JsonConvert.DeserializeObject<glTF>(json);
            // var GLTF2 = JsonUtility.FromJson<glTF>(json);
            if (GLTF.asset.version != "2.0")
            {
            }

            // Version Compatibility
            RestoreOlderVersionValues();

            // parepare byte buffer
            //GLTF.baseDir = System.IO.Path.GetDirectoryName(Path);
            foreach (var buffer in GLTF.buffers) buffer.OpenStorage(storage);

            void RestoreOlderVersionValues()
            {
                var parsed = JSON.Parse(json);
                for (var i = 0; i < GLTF.images.Count; ++i)
                    if (String.IsNullOrEmpty(GLTF.images[i].name))
                        try
                        {
                            var extraName = parsed["images"][i]["extra"]["name"].Value;
                            if (!String.IsNullOrEmpty(extraName)) GLTF.images[i].name = extraName;
                        }
                        catch (Exception)
                        {
                            // do nothing
                        }

                for (var i = 0; i < GLTF.meshes.Count; ++i)
                {
                    var mesh = GLTF.meshes[i];
                    try
                    {
                        for (var j = 0; j < mesh.primitives.Count; ++j)
                        {
                            var primitive = mesh.primitives[j];
                            for (var k = 0; k < primitive.targets.Count; ++k)
                            {
                                var extraName = parsed["meshes"][i]["primitives"][j]["targets"][k]["extra"]["name"]
                                    .Value;
                                //Debug.LogFormat("restore morphName: {0}", extraName);
                                primitive.extras.targetNames.Add(extraName);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // do nothing
                    }
                }
            }

            return GLTF;
        }
    }
}