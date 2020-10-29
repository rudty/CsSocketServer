using System;
using System.Collections.Generic;
using System.Text;

namespace SocketServer {
    public class Server: ISessionHandler {
        const int MAX_PACKET_SIZE = 100;

        public delegate void OnUserMessageListener(Session session, string message, Memory<byte> buffer);
        public event OnUserMessageListener UserMessageListener;

        readonly CNetworkService networkService = new CNetworkService();

        /// <summary>
        /// 전체 유저 session
        /// </summary>
        readonly Dictionary<string, Session> session = new Dictionary<string, Session>();

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
            
            //int header = buffer[0];
            //Memory<byte> m = buffer.AsMemory().Slice(1);
            //packetHandler[header]?.OnMessage(session, m);

            //CPacket p = new CPacket();
            //p.Push("hello");
            //p.Push(buffer);
            //session.Send(p);
        }

        void ISessionHandler.OnSendCompleted(Session session) {
            //throw new NotImplementedException();
        }
    }
}
