using System;
using System.Collections.Generic;
using System.Text;

namespace SocketServer {
    public interface IPeer {
        void onMessage(byte[] buffer);
        void onRemoved();
        void send();
        void disconnect();
        void processUserOperation();
    }
}
