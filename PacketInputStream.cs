using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SocketServer {
    public class PacketInputStream {
        int offset = 0;
        Memory<byte> buffer;
        
        public PacketInputStream(byte[] b) {
            buffer = b.AsMemory();
        }

        public PacketInputStream(Memory<byte> b) {
            buffer = b;
        }

        public byte NextByte() {
            var s = buffer.Span;
            byte v = s[offset];
            offset += 1;

            return v;
        }

        public int NextInt() {
            var s = buffer.Span;
            int v = s[offset];
            v += (s[offset + 1] << 8);
            v += (s[offset + 2] << 16);
            v += (s[offset + 3] << 24);

            offset += 4;

            return v;
        }

        public string NextString() { 
            var s = buffer.Span;
            int len = s[offset];
            len += (s[offset + 1] << 8);
            offset += 2;

            string r = Encoding.UTF8.GetString(s.Slice(offset, len));
            offset += len;

            return r;
        }

        private object NextInternal(Type structType) {
            var o = Activator.CreateInstance(structType);
            foreach (var f in structType.GetRuntimeFields()) {
                var fieldType = f.FieldType;
                var elem = f.GetValue(o);

                if (fieldType == typeof(int)) {
                    f.SetValue(o, NextInt());
                } else if (fieldType == typeof(byte)) {
                    f.SetValue(o, NextByte());
                } else if (fieldType == typeof(string)) {
                    f.SetValue(o, NextString());
                } else {
                    if (fieldType.BaseType != typeof(object)) {
                        throw new ArgumentException($"{fieldType} not support type");
                    }
                    if (fieldType.IsPrimitive) {
                        throw new ArgumentException($"{fieldType} not support type");
                    }
                    f.SetValue(o, NextInternal(fieldType));
                }
            }
            return o;
        }

        public T Next<T>() where T: class {
            return (T)NextInternal(typeof(T));
        }
    }
}
