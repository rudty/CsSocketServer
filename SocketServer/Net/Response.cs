using SocketServer.Core;
using SocketServer.Net.IO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer.Net {
    public class Response: IDisposable {
        /// <summary>
        /// 클라이언트에게 보낼 패킷
        /// </summary>
        public CPacket Packet { get; private set; } 

        /// <summary>
        /// 요청한 세션
        /// </summary>
        public Session Session { get; private set; }

        /// <summary>
        /// 클라이언트에서 요청한 패킷의 타입 메세지 그대로
        /// </summary>
        public readonly string Message;

        /// <summary>
        /// 시작시 패킷은 헤더 + 메세지 를 가집니다
        /// 이 패킷이 만약 사용되었다면 
        /// startPacketPosition != Packet.Position 이
        /// true 가 나오므로 답장을 보내고
        /// 그렇지 않다면 보내지 않습니다
        /// </summary>
        private readonly int startPacketPosition;

        public Response(string message, Session session) {
            Session = session;
            Message = message;
            Packet = CPacket.NewSend.Add(message);
            startPacketPosition = Packet.Position;
        }

        /// <summary>
        /// 클라이언트에게 해당 패킷을 보냅니다
        /// 패킷에 무언가를 담았어야 동작합니다 
        /// </summary>
        public void Send() {
            if (startPacketPosition != Packet.Position) {
                Session.Send(Packet);
                Packet = null;
            }
        }

        public void Dispose() {
            Session = null;
            if (Packet != null) {
                Packet.Recycle();
                Packet = null;
            }
        }
    }
}
