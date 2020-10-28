using System;
using System.Collections.Generic;
using System.Text;

namespace SocketServer {
    public interface IPacketReceive {
        public void OnMessage(Session session, Memory<byte> buffer);
    }
}
