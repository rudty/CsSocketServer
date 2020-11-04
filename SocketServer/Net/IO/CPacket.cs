﻿using System;
using System.Reflection;
using System.Text;

namespace SocketServer.Net.IO {
    public class CPacket : IDisposable {
        public const int PACKET_BEGIN = 0x8F;
        public const int HEADER_SIZE = 3;
        public const int MESSAGE_BUFFER_SIZE = 1024;

        public ISessionHandler Owner { get; private set; }
        public Memory<byte> Buffer { get; internal set; }
        public int Position { get; internal set; } = HEADER_SIZE;
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

        public Memory<byte> Packing() {
            var b = Buffer.Span;
            var dataLength = Position - HEADER_SIZE;

            b[0] = PACKET_BEGIN;
            b[1] = (byte)(dataLength);
            b[2] = (byte)(dataLength >> 8);

            return Buffer.Slice(0, Position);
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

        /// <summary>
        ///  사용자 형식의 클래스를 넣었을때 
        ///  해당 클래스를 직렬화 시킬 수 있음
        /// </summary>
        /// <typeparam name="T">모든 클래스</typeparam>
        /// <param name="o">모든 오브젝트</param>
        /// <returns></returns>
        private void PushInternal<T>(T o) {
            var structType = o.GetType();
            foreach (var f in structType.GetRuntimeFields()) {
                var fieldType = f.FieldType;
                var elem = f.GetValue(o);
                switch (elem) {
                    case int v:
                        Push(v);
                        break;
                    case byte v:
                        Push(v);
                        break;
                    case string v:
                        Push(v);
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
                            PushInternal(elem);
                            break;
                        }
                }
            }
        }

        public CPacket Push<T>(T o) where T: class {
            PushInternal(o);
            return this;
        }

        void IDisposable.Dispose() {
            Recycle();
        }
    }
}