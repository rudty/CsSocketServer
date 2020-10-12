using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace SocketServer {
    class CUserToken {

        internal SocketAsyncEventArgs ReceiveEventArgs { get; set; }
        internal SocketAsyncEventArgs SendEventArgs { get; set; }
        public Socket Socket { get; set; }

        public void onReceive(byte[] buffer, int offset, int byteTransferred) {
            
        }

        public void onRemoved() {

        }

        public void processSend(SocketAsyncEventArgs e) {

        }
    }
}
