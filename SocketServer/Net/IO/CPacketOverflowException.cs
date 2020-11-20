using System;

namespace SocketServer.Net.IO {
    public class CPacketOverflowException : OverflowException {
        public CPacketOverflowException(string message) : base(message) {
        }
    }
}
