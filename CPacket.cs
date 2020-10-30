using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace SocketServer {
    public class CPacket : IDisposable {
        public ISessionHandler Owner { get; private set; }
        public Memory<byte> Buffer { get; internal set; }
        public int Position { get; internal set; } = Consts.HEADER_SIZE;
        public Int16 ProtocolId { get; private set; }

        public static CPacket New {
            get {
                return new CPacket();
            }
        }

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


        public static CPacket operator +(CPacket p, int v) => p.Push(v);

        public CPacket Push(int data) {
            var b = Buffer.Span;
            int p = Position;
            b[p] = (byte)(data);
            b[p + 1] = (byte)(data >> 8);
            b[p + 2] = (byte)(data >> 16);
            b[p + 3] = (byte)(data >> 24);

            Position += sizeof(int);
            return this;
        }

        public static CPacket operator +(CPacket p, byte v) => p.Push(v);
        public CPacket Push(byte data) {
            var b = Buffer.Span;
            b[Position] = (data);

            Position += sizeof(byte);
            return this;
        }

        public static CPacket operator +(CPacket p, byte[] v) => p.Push(v);
        public CPacket Push(byte[] data) {
            Push(data, 0, data.Length);
            return this;
        }

        public static CPacket operator +(CPacket p, Memory<byte> v) => p.Push(v);
        public CPacket Push(Memory<byte> data) {
            data.CopyTo(Buffer.Slice(Position));
            return this;
        }

        public CPacket Push(byte[] data, int offset, int size) {
            var b = Buffer.Span;
            data
                .AsSpan(offset, size)
                .CopyTo(b.Slice(Position));
            Position += size;
            return this;
        }

        public static CPacket operator +(CPacket p, string v) => p.Push(v);
        public CPacket Push(string s) {
            var b = Buffer.Span;
            var len = (Int16)s.Length;
            b[Position + 0] = (byte)(len);
            b[Position + 1] = (byte)(len >> 8);
            Position += sizeof(Int16);

            byte[] byteString = Encoding.UTF8.GetBytes(s);
            byteString.CopyTo(b.Slice(Position));
            Position += byteString.Length;
            return this;
        }


        public CPacket Push<T>(T o) where T: struct {
            var structType = o.GetType();
            foreach (var f in structType.GetRuntimeFields()) {
                var fieldType = f.FieldType;

                if (fieldType == typeof(int)) {
                    Push((int)f.GetValue(o));
                    continue;
                }

                if (fieldType == typeof(string)) {
                    Push(f.GetValue(o) as string);
                    continue;
                }

                if (false == fieldType.IsPrimitive) {
                    Push(o);
                }
            }
            return this;
        }

        void IDisposable.Dispose() {
            Recycle();
        }
    }
}
