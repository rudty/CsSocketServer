using SocketServer.Net.IO;
using System;
using System.Threading.Tasks;

namespace SocketServer {
    public interface ISessionEventListener {
        Task OnPacketReceived(Session session, CPacket buffer);

        Task OnPacketDecodeFail(Session session, Exception ex, Memory<byte> buffer);

        Task OnDisconnected(Session session);
        Task OnSendCompleted(Session session);
    }
}
