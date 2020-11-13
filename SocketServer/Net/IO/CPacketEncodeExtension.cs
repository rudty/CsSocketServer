using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using SocketServer.Core;

namespace SocketServer.Net.IO {

    /// <summary>
    /// 이 클래스는 CPacket 에서 Encode 에 관련한 함수를 관리합니다.
    /// </summary>
    public static class CPacketEncodeExtension {

        public static CPacket Add(this CPacket p, int data) {
            var b = p.Buffer;
            int o = p.Position;
            b[o] = (byte)(data);
            b[o + 1] = (byte)(data >> 8);
            b[o + 2] = (byte)(data >> 16);
            b[o + 3] = (byte)(data >> 24);

            p.Position += sizeof(int);
            return p;
        }

        public static CPacket Add(this CPacket p, byte data) {
            var b = p.Buffer;
            b[p.Position] = (data);

            p.Position += sizeof(byte);
            return p;
        }

        public static CPacket Add(this CPacket p, byte[] data) {
            p.Add(data, 0, data.Length);
            return p;
        }

        public static CPacket Add(this CPacket p, Memory<byte> data) {
            data.CopyTo(p.Buffer[p.Position..]);
            return p;
        }

        private static void ThrowIfOverFlow(CPacket p, int addSize) {
            var b = p.Buffer;
            if (addSize > b.Length - p.Position) {
                throw new OverflowException($"packet overflow require:{addSize}, remain {b.Length - p.Position}");
            }
        }

        public static CPacket Add(this CPacket p, byte[] data, int offset, int size) {
            ThrowIfOverFlow(p, size);

            var b = p.Buffer;
            Array.Copy(data, offset, b.Buffer, p.Position, size);
            p.Position += size;
            return p;
        }

        public static CPacket Add(this CPacket p, string s) {
            var b = p.Buffer;
            var len = (Int16)s.Length;
            var o = p.Position;
            b[o + 0] = (byte)(len);
            b[o + 1] = (byte)(len >> 8);
            p.Position += sizeof(Int16);

            byte[] byteString = Encoding.UTF8.GetBytes(s);
            ThrowIfOverFlow(p, byteString.Length);

            Array.Copy(byteString, 0, b.Buffer, p.Position, byteString.Length);
            p.Position += byteString.Length;
            return p;
        }

        /// <summary>
        ///  사용자 형식의 클래스를 넣었을때 
        ///  해당 클래스를 직렬화 시킬 수 있음
        /// </summary>
        /// <typeparam name="T">모든 클래스</typeparam>
        /// <param name="o">모든 오브젝트</param>
        /// <returns></returns>
        private static void AddClass<T>(CPacket p, T o) {
            var structType = o.GetType();
            foreach (var f in structType.GetRuntimeFields()) {
                var fieldType = f.FieldType;
                var elem = f.GetValue(o);
                switch (elem) {
                    case int v:
                        p.Add(v);
                        break;
                    case byte v:
                        p.Add(v);
                        break;
                    case string v:
                        p.Add(v);
                        break;
                    default: {
                            if (elem == null) {
                                throw new ArgumentException($"{fieldType} cannot serialize null type");
                            }
                            if (fieldType.BaseType != typeof(object)) {
                                throw new ArgumentException($"{fieldType} not support type");
                            }
                            if (fieldType.IsPrimitive) {
                                throw new ArgumentException($"{fieldType} not support type");
                            }
                            AddClass(p, elem);
                            break;
                        }
                }
            }
        }

        public static CPacket Add<T>(this CPacket p, T o) where T : class {
            AddClass(p, o);
            return p;
        }
    }
}
