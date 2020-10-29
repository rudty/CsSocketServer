using System;
using System.Collections.Generic;
using System.Text;

namespace SocketServer {
    public class PacketInputStream {
        int offset = 0;
        Memory<byte> buffer;
        
        public PacketInputStream(byte[] b) {
            this.buffer = b.AsMemory();
        }

        public PacketInputStream(Memory<byte> b) {
            this.buffer = b;
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
    }
}
