using System;
using System.Collections.Generic;
using System.Text;

namespace SocketServer {
    class CPacketBufferManager {
        private static object csBuffer = new object();
        private static Stack<CPacket> pool;
    }
}
