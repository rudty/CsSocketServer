using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace SocketServer {
    class CUserToken {

        public SocketAsyncEventArgs ReceiveEventArgs { private get; set; }
        public SocketAsyncEventArgs SendEventArgs { private get; set; }
        public Socket Socket { get; set; }

        public void onReceive(byte[] buffer, int offset, int byteTransferred) {
            
        }
    }
}
