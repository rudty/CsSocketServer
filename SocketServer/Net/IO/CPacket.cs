using System;
using System.Reflection;
using System.Text;

namespace SocketServer.Net.IO {
    public class CPacket : IDisposable {
        public const int PACKET_BEGIN = 0x8F;
        public const int HEADER_SIZE = 3;
        public const int MESSAGE_BUFFER_SIZE = 1024;

        public Memory<byte> Buffer { get; internal set; }
        public int Position { get; internal set; } = HEADER_SIZE;

        public static CPacket New {
            get {
                return new CPacket();
            }
        }

        public CPacket() {
            Buffer = CPacketBufferManager.Obtain();
        }

        /// <summary>
        /// 가지고 있는 byte로 CPacket를 초기화합니다
        /// 인자로 받은 b는 반드시 CPacketBufferManager 에서 꺼내온 것이어야 합니다.
        /// </summary>
        /// <param name="b">CPacketBufferManager 로부터 할당 받은 메모리</param>
        public CPacket(Memory<byte> b) {
            this.Buffer = b;
        }
    
        ~CPacket() {
            Recycle();
        }

        public void Recycle() {
            if (!Buffer.IsEmpty) {
                CPacketBufferManager.Recycle(Buffer);
                Buffer = null;
            }
        }

        public Memory<byte> Packing() {
            var b = Buffer.Span;
            var dataLength = Position - HEADER_SIZE;

            b[0] = PACKET_BEGIN;
            b[1] = (byte)(dataLength);
            b[2] = (byte)(dataLength >> 8);

            return Buffer.Slice(0, Position);
        }

        void IDisposable.Dispose() {
            Recycle();
        }
    }
}
