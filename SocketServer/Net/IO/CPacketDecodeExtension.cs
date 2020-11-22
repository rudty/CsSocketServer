using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using SocketServer.Core;

namespace SocketServer.Net.IO {
    /// <summary>
    /// 이 클래스는 CPacket 에서 Decode 에 관련한 함수를 관리합니다.
    /// </summary>
    public static class CPacketDecodeExtension {

        public static byte NextByte(this CPacket p) {
            var s = p.Buffer;
            byte v = s[p.Position];
            p.Position += 1;

            return v;
        }

        public static int NextInt(this CPacket p) {
            var s = p.Buffer;
            int offset = p.Position;

            int v = s[offset];
            v += (s[offset + 1] << 8);
            v += (s[offset + 2] << 16);
            v += (s[offset + 3] << 24);

            p.Position += 4;

            return v;
        }

        public static short NextShort(this CPacket p) {
            var s = p.Buffer;
            int offset = p.Position;
            short value = s[offset];
            value += (short)(s[offset + 1] << 8);
            p.Position += 2;
            return value;
        }

        public static string NextString(this CPacket p) {
            int len = p.NextShort();
            len = Math.Min(len, p.Buffer.Length - p.Position);
            var s = p.Buffer;
  
            int offset = p.Position;
            string r = Encoding.UTF8.GetString(s, offset, len);
            offset += len;

            p.Position = offset;
            return r;
        }

        public static T Next<T>(this CPacket p, Google.Protobuf.MessageParser<T> parser) where T: Google.Protobuf.IMessage<T> {
            int len = p.NextShort();
            var s = p.Buffer;
            var m = parser.ParseFrom(s, p.Position, len);
            p.Position += len;
            return m;
        }
     }
}
