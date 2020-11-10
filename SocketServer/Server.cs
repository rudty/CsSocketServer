using System;
using System.Collections.Generic;
using SocketServer.Net;
using SocketServer.Net.IO;
using System.Threading.Tasks;

namespace SocketServer {
    public class Server: ISessionEventListener {

        public delegate bool OnSessionRegisterListener(Session session);
        public delegate Task OnUserMessageListener(Session session, CPacket packetInputStream);

        public event OnSessionRegisterListener SessionRegisterListener;

        readonly Dictionary<string, OnUserMessageListener> messageListeners = new Dictionary<string, OnUserMessageListener>(); 

        readonly NetworkService networkService = new NetworkService();

        /// <summary>
        /// 전체 유저 session
        /// </summary>
        readonly Dictionary<string, Session> allSession = new Dictionary<string, Session>();

        public Server() {
            networkService.OnSessionCreated += OnNewClient; 
        }

        /// <summary>
        /// 소켓 서버를 구동시킵니다
        /// </summary>
        /// <param name="host">허용할 아이피</param>
        /// <param name="port">오픈 포트</param>
        public void ListenAndServe(string host, int port) {
            networkService.ListenAndServe(host, port);
        }

        void OnNewClient(Session session) {
            session.OnSessionEventListener = this;
        }

        ///
        public void AddEventListener(string k, OnUserMessageListener l) {
            if (l != null) {
                lock (messageListeners) {
                    //TODO 클래스에 담는다던가 해서 두번 검색하는거 최적화 필요
                    if (messageListeners.ContainsKey(k)) {
                        messageListeners[k] += l;
                    } else {
                        messageListeners.Add(k, l);
                    }
                }
            }
        }

        public void RemoveEventListener(string k, OnUserMessageListener l) {
            lock (messageListeners) {
                if (messageListeners.ContainsKey(k)) {
                    messageListeners[k] -= l;
                }
            }
        }

        /// <summary>
        /// 세션과 연결이 끊어졌을 때 수행
        /// </summary>
        /// <param name="session"></param>
        Task ISessionEventListener.OnDisconnected(Session session) {
            lock (allSession) {
                allSession.Remove(session.SessionID);
            }
            return Task.CompletedTask;
        }

        void OnSessionRegister(Session session, CPacket p) {
            if (SessionRegisterListener(session)) {
                lock (allSession) {
                    allSession.Add(session.SessionID, session);
                }
            }
        }

        void ProcessUserMessage(Session session, CPacket p) {
            string message = p.NextString();
            bool exists;
            OnUserMessageListener listener;
            lock (messageListeners) {
                exists = messageListeners.TryGetValue(message, out listener);
            }

            if (exists) {
                listener(session, p);
            }
        }

        Task ISessionEventListener.OnPacketReceived(Session session, CPacket p) {
            int header = p.NextByte();
            switch (header) {
                case 0:
                    OnSessionRegister(session, p);
                    break;
                case 1: {
                        if (allSession.ContainsKey(session.SessionID)) {
                            ProcessUserMessage(session, p);
                        } else {
                            networkService.CloseClient(session);

                        }
                        break;
                    }
            }
            return Task.CompletedTask;
        }

        Task ISessionEventListener.OnPacketDecodeFail(Session session, Exception ex, Memory<byte> buffer) {
            Console.WriteLine(ex);
            CPacket p = new CPacket();
            p.Add(buffer);
            session.Send(p);
            return Task.CompletedTask;
        }

        Task ISessionEventListener.OnSendCompleted(Session session) {
            return Task.CompletedTask;
        }
    }
}
