using System;
using System.Collections.Generic;
using System.Text;

namespace SocketServer {
    public interface ISessionEventListener {
        void OnMessageReceived(Session session, byte[] buffer);

        void OnDataDecodeFail(Session session, Exception ex, Memory<byte> buffer);
        
        void OnDisconnected(Session session);
        void OnSendCompleted(Session session);
    }
}
