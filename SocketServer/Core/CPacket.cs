using System;
using System.Reflection;
using System.Text;

namespace SocketServer.Core {
    public class CPacket : IDisposable {
        public const int PACKET_BEGIN = 0x8F;
        public const int HEADER_SIZE = 3;
        public const int MESSAGE_BUFFER_SIZE = 1024;

        public Slice<byte> Buffer { get; internal set; }
        public int Position { get; internal set; } = HEADER_SIZE;

        /// <summary>
        /// CPacket 에 아무것도 들어있지 않은지 
        /// </summary>
        public bool IsEmpty {
            get {
                return Position == HEADER_SIZE;
            }
        } 

        public static CPacket NewSend {
            get {
                return new CPacket();
            }
        }
        public static CPacket NewReceive {
            get {
                return new CPacket() {
                    Position = 0,
                };
            }
        }


        public CPacket() {
            Buffer = SliceMemoryPool.Obtain();
        }

        /// <summary>
        /// 가지고 있는 byte로 CPacket를 초기화합니다
        /// 인자로 받은 b는 반드시 CPacketBufferManager 에서 꺼내온 것이어야 합니다.
        /// </summary>
        /// <param name="b">CPacketBufferManager 로부터 할당 받은 메모리</param>
        public CPacket(Slice<byte> b) {
            this.Buffer = b;
        }
    
        ~CPacket() {
            Recycle();
        }

        public void Recycle() {
            if (!Buffer.IsEmpty) {
                SliceMemoryPool.Recycle(Buffer);
                Buffer = null;
            }
        }

        public Memory<byte> Packing() {
            Slice<byte> b = Buffer;
            var dataLength = Position - HEADER_SIZE;

            b[0] = PACKET_BEGIN;
            b[1] = (byte)(dataLength);
            b[2] = (byte)(dataLength >> 8);

            return Buffer.AsMemory(0, Position);
        }

        public void MoveToFirst() {
            this.Position = HEADER_SIZE;
        }

        void IDisposable.Dispose() {
            Recycle();
        }
    }
}
