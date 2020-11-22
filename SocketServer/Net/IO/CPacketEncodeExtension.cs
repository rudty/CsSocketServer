using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using SocketServer.Core;
using Google.Protobuf;

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

        public static CPacket Add(this CPacket p, short data) {
            var b = p.Buffer;
            int o = p.Position;
            b[o] = (byte)(data);
            b[o + 1] = (byte)(data >> 8);

            p.Position += sizeof(short);
            return p;
        }

        public static CPacket Add(this CPacket p, byte data) {
            var b = p.Buffer;
            b[p.Position] = (data);

            p.Position += sizeof(byte);
            return p;
        }

        public static CPacket Add(this CPacket p, byte[] data) {
            return Add(p, data, 0, data.Length);
        }

        private static void ThrowIfOverFlow(CPacket p, int addSize) {
            var b = p.Buffer;
            if (addSize > b.Length - p.Position) {
                throw new OverflowException($"packet overflow require:{addSize}, remain {b.Length - p.Position}");
            }
        }

        public static CPacket Add(this CPacket p, byte[] data, int offset, int size) {
            var b = p.Buffer;
            Array.Copy(data, offset, b, p.Position, size);
            p.Position += size;
            return p;
        }

        public static CPacket Add(this CPacket p, string s) {
            var len = (Int16)s.Length;
            p.Add(len);

            var b = p.Buffer;

            byte[] byteString = Encoding.UTF8.GetBytes(s);
            ThrowIfOverFlow(p, byteString.Length);

            Array.Copy(byteString, 0, b, p.Position, byteString.Length);
            p.Position += byteString.Length;
            return p;
        }

        public static CPacket Add(this CPacket p, IMessage m) {
            var len = (Int16)m.CalculateSize();
            p.Add(len);
            var s = p.Buffer;
            var span = s.AsSpan(p.Position, len);
            m.WriteTo(span);
            p.Position += len;
            return p;
        }

    }
}
