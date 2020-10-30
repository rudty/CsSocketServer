using System;
using System.Collections.Generic;
using System.Text;

namespace SocketServer {
    public class Server: ISessionHandler {
        const int MAX_PACKET_SIZE = 100;

        public delegate bool OnSessionRegisterListener(Session session);
        public delegate void OnUserMessageListener(Session session, string message, Memory<byte> buffer);

        public event OnUserMessageListener UserMessageListener;
        public event OnSessionRegisterListener SessionRegisterListener;

        readonly CNetworkService networkService = new CNetworkService();

        /// <summary>
        /// 전체 유저 session
        /// </summary>
        readonly Dictionary<string, Session> allSession = new Dictionary<string, Session>();

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
            lock (allSession) {
                allSession.Remove(session.SessionID);
            }
            //throw new NotImplementedException();
        }

        void OnSessionRegister(Session session, Memory<byte> m) {
            if (SessionRegisterListener(session)) {
                lock (allSession) {
                    allSession.Add(session.SessionID, session);
                }
            }
        }

        void ISessionHandler.OnMessage(Session session, byte[] buffer) {
            int header = buffer[0];
            Memory<byte> m = buffer.AsMemory().Slice(1);
            switch (header) {
                case 0:
                    OnSessionRegister(session, m);
                    break;
            }
        }

        void ISessionHandler.OnSendCompleted(Session session) {
        }
    }
}
