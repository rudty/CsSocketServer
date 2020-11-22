using SocketServer.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocketServer.Net {
    class PacketDecodeFailException: Exception {
        public byte[] Buffer;

        public PacketDecodeFailException(byte[] buffer, int offset, int length, string what): base(what) {
            Buffer = new byte[buffer.Length];
            Array.Copy(buffer, offset, this.Buffer, offset, length);
        }
    }
}
