using System;
using System.Collections.Generic;
using System.Text;

namespace SocketServer {
    class CPacket {
        public IPeer Owner { get; private set; }
        public byte[] Buffer { get; private set; }
        public int Position { get; private set; }
        public Int16 ProtocolId { get; private set; }




    }
}
