using SocketServer.Core;
using System;

namespace SocketServer.Net {
    public class Request: IDisposable {

        /// <summary>
        /// 클라이언트에서 요청한 패킷의 타입 메세지
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// 클라이언트에서 보낸 패킷
        /// </summary>
        public CPacket Packet { get; private set; }

        /// <summary>
        /// 클라이언트 정보가 포함된 세션
        /// </summary>
        public Session Session { get; private set; }

        public Request(string message, CPacket requestPacket, Session session) {
            Message = message;
            Packet = requestPacket;
            Session = session;
        }

        void IDisposable.Dispose() {
            Packet.Recycle();
        }
    }
}
