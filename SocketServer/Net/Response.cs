using SocketServer.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer.Net {
    public class Response: IDisposable {
        /// <summary>
        /// 클라이언트에게 보낼 패킷
        /// </summary>
        public CPacket Packet { get; private set; } = CPacket.NewSend;

        /// <summary>
        /// 요청한 세션
        /// </summary>
        public Session Session;

        public Response(Session session) {
            Session = session;
        }

        /// <summary>
        /// 클라이언트에게 해당 패킷을 보냅니다
        /// 패킷에 무언가를 담았어야 동작합니다 
        /// </summary>
        public void Send() {
            if (false == Packet.IsEmpty) {
                Session.Send(Packet);
                Packet = null;
            }
        }

        public void Dispose() {
            Session = null;
            if (Packet != null) {
                Packet.Recycle();
            }
        }
    }
}
