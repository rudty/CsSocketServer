using System;
using System.Collections.Generic;
using System.Text;

namespace SocketServer {
    public interface ISessionHandler {
        void OnMessage(Session session, byte[] buffer);

        void OnDecodeFail(Session session, Exception ex, Memory<byte> buffer);
        
        void OnDisconnected(Session session);
        void OnSendCompleted(Session session);
    }
}
