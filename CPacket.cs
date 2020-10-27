using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SocketServer {
    public class CPacket {
        public ISessionHandler Owner { get; private set; }
        public Memory<byte> Buffer { get; private set; }
        public int Position { get; private set; } = 0;// Consts.HEADER_SIZE;
        public Int16 ProtocolId { get; private set; }

        public CPacket() {
            Buffer = CPacketBufferManager.Obtain();
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

        public void RecordSize() {
            var b = Buffer.Span;
            var dataLength = Position - Consts.HEADER_SIZE;

            b[0] = Consts.PACKET_BEGIN;
            b[1] = (byte)(dataLength);
            b[2] = (byte)(dataLength >> 8);
        }

        public void Push(int data) {
            var b = Buffer.Span;
            int p = Position;
            b[p] = (byte)(data);
            b[p + 1] = (byte)(data >> 8);
            b[p + 2] = (byte)(data >> 16);
            b[p + 3] = (byte)(data >> 24);

            Position += sizeof(int);
        }

        public void Push(byte data) {
            var b = Buffer.Span;
            b[Position] = (data);

            Position += sizeof(byte);
        }

        public void Push(byte[] data) {
            Push(data, 0, data.Length);
        }

        public void Push(Memory<byte> data) {
            data.CopyTo(Buffer.Slice(Position));
        }

        public void Push(byte[] data, int offset, int size) {
            var b = Buffer.Span;
            data
                .AsSpan(offset, size)
                .CopyTo(b.Slice(Position));
            Position += size;
        }

        public void Push(string s) {
            var b = Buffer.Span;
            var len = (Int16)s.Length;
            b[Position + 0] = (byte)(len);
            b[Position + 1] = (byte)(len >> 8);
            Position += sizeof(Int16);

            byte[] byteString = Encoding.UTF8.GetBytes(s);
            byteString.CopyTo(b.Slice(Position));
            Position += byteString.Length;
        }


    }
}
