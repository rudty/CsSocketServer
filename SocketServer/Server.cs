using System;
using System.Collections.Generic;
using SocketServer.Net.IO;
using System.Threading.Tasks;
using SocketServer.Core;
using SocketServer.Net;
using System.Reflection;

namespace SocketServer {
    public class Server: ISessionEventListener {

        public delegate Task ClientMessageListener(Request request, Response response);
        public delegate Task ClientDisconnectListener(Session session);

        readonly Dictionary<string, ClientMessageListener> messageListeners = new Dictionary<string, ClientMessageListener>();

        readonly NetworkService networkService;

        public event ClientDisconnectListener OnClientDisconnect;

        public Server() {
            networkService = new NetworkService(this);
            OnClientDisconnect += EmptyCloseHandler;
        }

        private static Task EmptyCloseHandler(Session s) {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 소켓 서버를 구동시킵니다
        /// </summary>
        /// <param name="host">허용할 아이피</param>
        /// <param name="port">오픈 포트</param>
        public void ListenAndServe(string host, int port) {
            networkService.ListenAndServe(host, port);
        }

        /// <summary>
        /// 특정 메세지로 입력을 받았을때 구동할 함수를 등록합니다
        /// </summary>
        /// <param name="message">입력받은 메세지</param>
        /// <param name="l">실행할 함수</param>
        public void AddMessageHandler(string message, ClientMessageListener l) {
            if (l != null) {
                lock (messageListeners) {
                    if (messageListeners.ContainsKey(message)) {
                        messageListeners[message] += l;
                    } else {
                        messageListeners.Add(message, l);
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
            return OnClientDisconnect(session); 
        }

        async Task ProcessUserMessage(Session session, CPacket requestPacket) {
            string message = requestPacket.NextString();
            bool exists;
            ClientMessageListener listener;
            lock (messageListeners) {
                exists = messageListeners.TryGetValue(message, out listener);
            }

            if (exists) {
                using var res = new Response();
                await listener(new Request(message, requestPacket, session), res);

                var packet = res.Packet;
                if (false == packet.IsEmpty) {
                    session.Send(packet);
                }
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

        public void Boot(object any) {
            var t = any.GetType();
            var methods = t.GetMethods();
            foreach (var method in methods) {
                if (method.GetCustomAttribute(typeof(MessageHandler)) is MessageHandler attr) {
                    string message = attr.Message;
                    if (message == null) {
                        message = method.Name.ToLower();
                    }

                    if (Delegate.CreateDelegate(
                            typeof(ClientMessageListener), 
                            any, 
                            method) is ClientMessageListener l) {
                        AddMessageHandler(message, l);
                    }
                }
            }
        }
    }
}
