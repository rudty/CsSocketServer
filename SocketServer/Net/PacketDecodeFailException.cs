using SocketServer.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocketServer.Net {
    class PacketDecodeFailException: Exception {
        public Slice<byte> Buffer;

        public PacketDecodeFailException(Slice<byte> buffer, string what): base(what) {
            Buffer = buffer;
        }
    }
}
