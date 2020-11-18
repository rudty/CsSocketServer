using System;
using System.Collections.Generic;
using SocketServer.Net.IO;
using System.Threading.Tasks;
using SocketServer.Core;
using SocketServer.Net;

namespace SocketServer {
    public class Server: ISessionEventListener {

        public delegate Task ClientMessageListener(Request request);

        readonly Dictionary<string, ClientMessageListener> messageListeners = new Dictionary<string, ClientMessageListener>();

        readonly NetworkService networkService;

        /// <summary>
        /// 전체 유저 session
        /// </summary>
        readonly Dictionary<string, Session> allSession = new Dictionary<string, Session>();

        public Server() {
            networkService = new NetworkService(this);
        }

        /// <summary>
        /// 소켓 서버를 구동시킵니다
        /// </summary>
        /// <param name="host">허용할 아이피</param>
        /// <param name="port">오픈 포트</param>
        public void ListenAndServe(string host, int port) {
            networkService.ListenAndServe(host, port);
        }

        ///
        public void AddEventListener(string k, ClientMessageListener l) {
            if (l != null) {
                lock (messageListeners) {
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

        async Task ProcessUserMessage(Session session, CPacket requestPacket) {
            string message = requestPacket.NextString();
            bool exists;
            ClientMessageListener listener;
            lock (messageListeners) {
                exists = messageListeners.TryGetValue(message, out listener);
            }

            if (exists) {
                await listener(new Request(message, requestPacket, session));
            }
        }

        Task ISessionEventListener.OnPacketReceived(Session session, CPacket p) {
            var t = ProcessUserMessage(session, p);
            return t;
        }

        Task ISessionEventListener.OnPacketDecodeFail(Session session, Exception ex, Slice<byte> buffer) {
            Console.WriteLine(ex);
            CPacket p = new CPacket();
            p.Add(buffer);
            session.Send(p);
            return Task.CompletedTask;
        }

        Task ISessionEventListener.OnSendCompleted(Session session) {
            return Task.CompletedTask;
        }

        public Task OnCreate(Session session) {
            return Task.CompletedTask;
        }
    }
}
