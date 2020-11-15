using System;
using System.Collections.Generic;
using SocketServer.Net;
using SocketServer.Net.IO;
using System.Threading.Tasks;
using SocketServer.Core;
using System.Runtime.InteropServices;

namespace SocketServer {
    public class Server: ISessionEventListener {

        public delegate bool OnSessionRegisterListener(Session session);
        public delegate Task ClientMessageListener(Session session, string message, CPacket packetInputStream);

        readonly Dictionary<string, ClientMessageListener> messageListeners = new Dictionary<string, ClientMessageListener>(); 

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
        public void AddEventListener(string k, ClientMessageListener l) {
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

        public void RemoveEventListener(string k, ClientMessageListener l) {
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

        async Task ProcessUserMessage(Session session, CPacket p) {
            string message = p.NextString();
            bool exists;
            ClientMessageListener listener;
            lock (messageListeners) {
                exists = messageListeners.TryGetValue(message, out listener);
            }

            if (exists) {
                await listener(session, message, p);
            }
        }

        Task ISessionEventListener.OnPacketReceived(Session session, CPacket p) {
            var t = ProcessUserMessage(session, p);
            return t;
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
