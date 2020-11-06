using System;
using System.Reflection;
using System.Text;

namespace SocketServer.Net.IO {
    /// <summary>
    /// 이 클래스는 CPacket 에서 Decode 에 관련한 함수를 관리합니다.
    /// </summary>
    public static class CPacketDecodeExtension {

        public static byte NextByte(this CPacket p) {
            var s = p.Buffer.Span;
            byte v = s[p.Position];
            p.Position += 1;

            return v;
        }

        public static int NextInt(this CPacket p) {
            var s = p.Buffer.Span;
            int offset = p.Position;

            int v = s[offset];
            v += (s[offset + 1] << 8);
            v += (s[offset + 2] << 16);
            v += (s[offset + 3] << 24);

            p.Position += 4;

            return v;
        }

        public static string NextString(this CPacket p) {
            var s = p.Buffer.Span;
            int offset = p.Position;

            int len = s[offset];
            len += (s[offset + 1] << 8);
            offset += 2;

            string r = Encoding.UTF8.GetString(s.Slice(offset, len));
            offset += len;

            p.Position = offset;
            return r;
        }

        private static object NextInternal(CPacket p, Type structType) {
            var o = Activator.CreateInstance(structType);
            foreach (var f in structType.GetRuntimeFields()) {
                var fieldType = f.FieldType;
                var elem = f.GetValue(o);

                if (fieldType == typeof(int)) {
                    f.SetValue(o, p.NextInt());
                } else if (fieldType == typeof(byte)) {
                    f.SetValue(o, p.NextByte());
                } else if (fieldType == typeof(string)) {
                    f.SetValue(o, p.NextString());
                } else {
                    if (fieldType.BaseType != typeof(object)) {
                        throw new ArgumentException($"{fieldType} not support type");
                    }
                    if (fieldType.IsPrimitive) {
                        throw new ArgumentException($"{fieldType} not support type");
                    }
                    f.SetValue(o, NextInternal(p, fieldType));
                }
            }
            return o;
        }

        public static T Next<T>(this CPacket p) where T : class {
            return (T)NextInternal(p, typeof(T));
        }
    }
}
