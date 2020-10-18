using System;
using System.Collections.Generic;
using System.Text;

namespace SocketServer {
    class CPacket {
        public IPeer Owner { get; private set; }
        public byte[] Buffer { get; private set; }
        public int Position { get; private set; } = 0;// Consts.HEADER_SIZE;
        public Int16 ProtocolId { get; private set; }

        public CPacket()
            : this(new byte[1024]) {
        }

        public CPacket(byte[] buf) {
            Buffer = buf;
        }

        public void recordSize() {
            Int16 s = (Int16)(Position - Consts.HEADER_SIZE);
            byte[] header = BitConverter.GetBytes(s);
            header.CopyTo(Buffer, 0);
        }

        public void Push(int data) {
            var b = Buffer;
            b[0] = (byte)(data);
            b[1] = (byte)(data >> 8);
            b[2] = (byte)(data >> 16);
            b[3] = (byte)(data >> 24);

            Position += sizeof(int);
        }

        public void Push(byte data) {
            var b = Buffer;
            b[0] = (data);

            Position += sizeof(byte);
        }

        public void Push(byte[] data, int offset, int size) {
            var b = Buffer;
            Array.Copy(data, offset, b, Position, size);
            Position += size;
        }

        public void Push(string s) {
            var b = Buffer;
            var len = (Int16)s.Length;
            b[0] = (byte)(len);
            b[1] = (byte)(len >> 8);
            Position += sizeof(Int16);

            byte[] byteString = Encoding.UTF8.GetBytes(s);
            byteString.CopyTo(b, Position);
            Position += byteString.Length;
        }


        public CPacket Clone() {
            return new CPacket((byte[])Buffer.Clone()) {
                Owner = Owner,
                ProtocolId = ProtocolId
            };
        }
    }
}
