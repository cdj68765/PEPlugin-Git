using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PmxRead
{
    internal class Program
    {
        private static byte additionalUV;
        private static Header.IndexSize boneIndexSize;
        private static Header.StringCode encodeMetho;

        public static MemoryStream Import(object args)
        {
            var Para = args as List<object>;
            long t1 = 0;
            long t2 = 0;
            var texture_file = new List<string>();
            using (var fs = (Stream)Para[0])
            {
                #region 模型头信息

                fs.Position = 0;
                var reader = new BinaryReader(fs);
                reader.ReadBytes(4);
                reader.ReadSingle();
                reader.ReadByte();
                encodeMetho = (Header.StringCode)reader.ReadByte();
                additionalUV = reader.ReadByte();
                var vertexIndexSize = (Header.IndexSize)reader.ReadByte();
                reader.ReadByte();
                reader.ReadByte();
                boneIndexSize = (Header.IndexSize)reader.ReadByte();
                reader.ReadByte();
                reader.ReadByte();

                #endregion 模型头信息

                #region 模型信息

                for (var x = 0; x < 4; x++)
                {
                    reader.ReadBytes(reader.ReadInt32());
                }

                #endregion 模型信息

                #region 模型顶点

                {
                    var vertex = reader.ReadUInt32();
                    for (var i = 0; i < vertex; i++)
                    {
                        for (var j = 0; j < 8 + additionalUV * 4; j++)
                        {
                            reader.ReadSingle();
                        }
                        switch (reader.ReadByte())
                        {
                            case 0:
                                {
                                    CastIntRead(reader, boneIndexSize);
                                }
                                break;

                            case 1:
                                {
                                    CastIntRead(reader, boneIndexSize);
                                    CastIntRead(reader, boneIndexSize);
                                    reader.ReadSingle();
                                }
                                break;

                            case 2:
                                {
                                    CastIntRead(reader, boneIndexSize);
                                    CastIntRead(reader, boneIndexSize);
                                    CastIntRead(reader, boneIndexSize);
                                    CastIntRead(reader, boneIndexSize);
                                    reader.ReadSingle();
                                    reader.ReadSingle();
                                    reader.ReadSingle();
                                    reader.ReadSingle();
                                }
                                break;

                            case 3:
                                {
                                    CastIntRead(reader, boneIndexSize);
                                    CastIntRead(reader, boneIndexSize);
                                    reader.ReadSingle();
                                    ReadSinglesToVector3(reader);
                                    ReadSinglesToVector3(reader);
                                    ReadSinglesToVector3(reader);
                                }
                                break;

                            case 4:
                                {
                                    CastIntRead(reader, boneIndexSize);
                                    CastIntRead(reader, boneIndexSize);
                                    CastIntRead(reader, boneIndexSize);
                                    CastIntRead(reader, boneIndexSize);
                                    reader.ReadSingle();
                                    reader.ReadSingle();
                                    reader.ReadSingle();
                                    reader.ReadSingle();
                                    ReadSinglesToVector3(reader);
                                    ReadSinglesToVector3(reader);
                                    ReadSinglesToVector3(reader);
                                }
                                break;
                        }
                        reader.ReadSingle();
                    }
                }

                #endregion 模型顶点

                #region 模型面

                {
                    var num = reader.ReadUInt32();
                    for (var i = 0; i < num; i++)
                    {
                        CastIntRead(reader,
                            vertexIndexSize);
                    }
                }

                #endregion 模型面

                #region 模型贴图

                {
                    t1 = fs.Position;
                    var num = reader.ReadUInt32();
                    texture_file = new List<string>();
                    for (var i = 0; i < num; i++)
                    {
                        texture_file.Add(ReadString(reader));
                    }
                    t2 = fs.Position;
                }

                #endregion 模型贴图

                fs.Position = 0;
                var T1 = new byte[t1];
                var T2 = new byte[fs.Length - t2];
                long count = 0;
                while (fs.Position < t1)
                {
                    T1[count] = (byte)fs.ReadByte();
                    count++;
                }
                while (fs.Position < t2)
                {
                    fs.ReadByte();
                }
                count = 0;
                while (fs.Position < fs.Length)
                {
                    T2[count] = (byte)fs.ReadByte();
                    count++;
                }
                var save = new MemoryStream();
                save.Position = 0;

                if (
                    texture_file[texture_file.Count - 1].Replace(
                        Encoding.Unicode.GetString(
                            new SHA256Managed().ComputeHash(Encoding.Unicode.GetBytes((string)Para[1]))), "") == "" ||
                    texture_file[texture_file.Count - 1].Replace(
                        Encoding.UTF8.GetString(
                            new SHA256Managed().ComputeHash(Encoding.Unicode.GetBytes((string)Para[1]))), "") == "")
                {
                    T1[3] = 32;
                    save.Write(T1, 0, T1.Length);
                    for (var i = 0; i < 2; i++)
                    {
                        texture_file.RemoveAt(texture_file.Count - 1);
                    }
                }
                else
                {
                    T1[3] = 64;
                    save.Write(T1, 0, T1.Length);
                }
                var buffer = BitConverter.GetBytes(texture_file.Count);
                save.Write(buffer, 0, buffer.Length);
                foreach (var VARIABLE in texture_file)
                {
                    switch (encodeMetho)
                    {
                        case Header.StringCode.Utf16Le:
                            var savebuffer = Encoding.Unicode.GetBytes(VARIABLE);
                            save.Write(BitConverter.GetBytes(savebuffer.Length), 0, 4);
                            save.Write(savebuffer, 0, savebuffer.Length);
                            break;

                        case Header.StringCode.Utf8:
                            var savebuffer2 = Encoding.UTF8.GetBytes(VARIABLE);
                            save.Write(BitConverter.GetBytes(savebuffer2.Length), 0, 4);
                            save.Write(savebuffer2, 0, savebuffer2.Length);
                            break;
                    }
                }
                save.Write(T2, 0, T2.Length);
                save.Position = 0;
                return save;
            }
        }

        public static MemoryStream Export(object args)
        {
            var Para = args as List<object>;
            long t1 = 0;
            long t2 = 0;
            var texture_file = new List<string>();
            using (var fs = (Stream)Para[0])
            {
                #region 模型头信息

                fs.Position = 0;
                var reader = new BinaryReader(fs);
                reader.ReadBytes(4);
                reader.ReadSingle();
                reader.ReadByte();
                encodeMetho = (Header.StringCode)reader.ReadByte();
                additionalUV = reader.ReadByte();
                var vertexIndexSize = (Header.IndexSize)reader.ReadByte();
                reader.ReadByte();
                reader.ReadByte();
                boneIndexSize = (Header.IndexSize)reader.ReadByte();
                reader.ReadByte();
                reader.ReadByte();

                #endregion 模型头信息

                #region 模型信息

                for (var x = 0; x < 4; x++)
                {
                    reader.ReadBytes(reader.ReadInt32());
                }

                #endregion 模型信息

                #region 模型顶点

                {
                    var vertex = reader.ReadUInt32();
                    for (var i = 0; i < vertex; i++)
                    {
                        for (var j = 0; j < 8 + additionalUV * 4; j++)
                        {
                            reader.ReadSingle();
                        }
                        switch (reader.ReadByte())
                        {
                            case 0:
                                {
                                    CastIntRead(reader, boneIndexSize);
                                }
                                break;

                            case 1:
                                {
                                    CastIntRead(reader, boneIndexSize);
                                    CastIntRead(reader, boneIndexSize);
                                    reader.ReadSingle();
                                }
                                break;

                            case 2:
                                {
                                    CastIntRead(reader, boneIndexSize);
                                    CastIntRead(reader, boneIndexSize);
                                    CastIntRead(reader, boneIndexSize);
                                    CastIntRead(reader, boneIndexSize);
                                    reader.ReadSingle();
                                    reader.ReadSingle();
                                    reader.ReadSingle();
                                    reader.ReadSingle();
                                }
                                break;

                            case 3:
                                {
                                    CastIntRead(reader, boneIndexSize);
                                    CastIntRead(reader, boneIndexSize);
                                    reader.ReadSingle();
                                    ReadSinglesToVector3(reader);
                                    ReadSinglesToVector3(reader);
                                    ReadSinglesToVector3(reader);
                                }
                                break;

                            case 4:
                                {
                                    CastIntRead(reader, boneIndexSize);
                                    CastIntRead(reader, boneIndexSize);
                                    CastIntRead(reader, boneIndexSize);
                                    CastIntRead(reader, boneIndexSize);
                                    reader.ReadSingle();
                                    reader.ReadSingle();
                                    reader.ReadSingle();
                                    reader.ReadSingle();
                                    ReadSinglesToVector3(reader);
                                    ReadSinglesToVector3(reader);
                                    ReadSinglesToVector3(reader);
                                }
                                break;
                        }
                        reader.ReadSingle();
                    }
                }

                #endregion 模型顶点

                #region 模型面

                {
                    var num = reader.ReadUInt32();
                    for (var i = 0; i < num; i++)
                    {
                        CastIntRead(reader,
                            vertexIndexSize);
                    }
                }

                #endregion 模型面

                #region 模型贴图

                {
                    t1 = fs.Position;
                    var num = reader.ReadUInt32();
                    texture_file = new List<string>();
                    for (var i = 0; i < num; i++)
                    {
                        texture_file.Add(ReadString(reader));
                    }
                    t2 = fs.Position;
                }

                #endregion 模型贴图

                fs.Position = 0;
                var T1 = new byte[t1];
                var T2 = new byte[fs.Length - t2];
                long count = 0;
                while (fs.Position < t1)
                {
                    T1[count] = (byte)fs.ReadByte();
                    count++;
                }
                while (fs.Position < t2)
                {
                    fs.ReadByte();
                }
                count = 0;
                while (fs.Position < fs.Length)
                {
                    T2[count] = (byte)fs.ReadByte();
                    count++;
                }
                var save = new MemoryStream();
                save.Position = 0;
                T1[3] = 64;
                save.Write(T1, 0, T1.Length);
                var buffer = BitConverter.GetBytes(texture_file.Count + 2);
                texture_file.Add(
                    Encoding.Unicode.GetString(
                        new SHA256Managed().ComputeHash(Encoding.Unicode.GetBytes((string)Para[1]))));
                texture_file.Add(
                    Encoding.Unicode.GetString(
                        new SHA256Managed().ComputeHash(Encoding.Unicode.GetBytes((string)Para[1]))));
                save.Write(buffer, 0, buffer.Length);
                foreach (var VARIABLE in texture_file)
                {
                    switch (encodeMetho)
                    {
                        case Header.StringCode.Utf16Le:
                            var savebuffer = Encoding.Unicode.GetBytes(VARIABLE);
                            save.Write(BitConverter.GetBytes(savebuffer.Length), 0, 4);
                            save.Write(savebuffer, 0, savebuffer.Length);
                            break;

                        case Header.StringCode.Utf8:
                            var savebuffer2 = Encoding.UTF8.GetBytes(VARIABLE);
                            save.Write(BitConverter.GetBytes(savebuffer2.Length), 0, 4);
                            save.Write(savebuffer2, 0, savebuffer2.Length);
                            break;
                    }
                }
                save.Write(T2, 0, T2.Length);
                return save;
            }
        }

        private static string ReadString(BinaryReader reader)
        {
            var num = reader.ReadInt32();
            var array = reader.ReadBytes(num);
            switch (encodeMetho)
            {
                case Header.StringCode.Utf16Le:
                    return Encoding.Unicode.GetString(array);

                case Header.StringCode.Utf8:
                    return Encoding.UTF8.GetString(array);
            }
            return "";
        }

        private static uint CastIntRead(BinaryReader bin, Header.IndexSize index_size)
        {
            switch (index_size)
            {
                case Header.IndexSize.Byte1:
                    {
                        uint num = bin.ReadByte();
                        if (num == 255u)
                        {
                            num = 4294967295u;
                        }
                        return num;
                    }
                case Header.IndexSize.Byte2:
                    {
                        uint num = bin.ReadUInt16();
                        if (num == 65535u)
                        {
                            num = 4294967295u;
                        }
                        return num;
                    }
                case Header.IndexSize.Byte4:
                    {
                        var num = bin.ReadUInt32();
                        return num;
                    }
            }
            return 4294967295u;
        }

        private static void ReadSinglesToVector3(BinaryReader binary_reader_)
        {
            for (var i = 0; i < 3; i++)
            {
                binary_reader_.ReadSingle();
            }
        }

        public class Header
        {
            public enum IndexSize
            {
                Byte1 = 1,
                Byte2,
                Byte4 = 4
            }

            public enum StringCode
            {
                Utf16Le,
                Utf8
            }
        }
    }
}