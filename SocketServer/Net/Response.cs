using SocketServer.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocketServer.Net {
    public class Response: IDisposable {
        /// <summary>
        /// 클라이언트에게 보낼 패킷
        /// </summary>
        public CPacket Packet { get; private set; } = CPacket.NewSend;

        public Response() {
        }

        void IDisposable.Dispose() {
            Packet.Recycle();
        }
    }
}
