using System;
using System.Collections.Generic;
using System.Text;

namespace SocketServer {
    public interface IPeer {
        void OnMessage(byte[] buffer);
        void OnDisconnected();
        void OnSendCompleted();
    }
}
