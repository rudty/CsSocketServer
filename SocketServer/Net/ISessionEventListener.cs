using SocketServer.Net.IO;
using System;
using System.Threading.Tasks;
using SocketServer.Core;

namespace SocketServer.Net {
    public interface ISessionEventListener {
        Task OnCreate(Session session);
        Task OnPacketReceived(Session session, CPacket buffer);

        Task OnPacketDecodeFail(Session session, Exception ex, byte[] buffer);

        Task OnDisconnected(Session session);
        Task OnSendCompleted(Session session, CPacket p);
    }
}
