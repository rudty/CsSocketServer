using System;
using System.Collections.Generic;
using System.Text;

namespace SocketServer {
    public class Server: ISessionHandler {
        readonly CNetworkService networkService = new CNetworkService();

        public Server() {
            networkService.OnSessionCreated += OnNewClient; 
        }

        public void ListenAndServe(string host, int port) {
            networkService.Listen(host, port);
        }

        void OnNewClient(Session session) {
            session.SessionHandler = this;
        }

        void ISessionHandler.OnDisconnected(Session session) {
            //throw new NotImplementedException();
        }

        void ISessionHandler.OnMessage(Session session, byte[] buffer) {
            //throw new NotImplementedException();
            CPacket p = new CPacket();
            p.Push("hello");
            p.Push(buffer);
            session.Send(p);
        }

        void ISessionHandler.OnSendCompleted(Session session) {
            //throw new NotImplementedException();
        }
    }
}
