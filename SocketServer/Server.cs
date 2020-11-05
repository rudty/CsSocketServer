using System;
using System.Collections.Generic;
using SocketServer.Net;
using SocketServer.Net.IO;

namespace SocketServer {
    public class Server: ISessionHandler {

        public delegate bool OnSessionRegisterListener(Session session);
        public delegate void OnUserMessageListener(Session session, CPacketInputStream packetInputStream);

        public event OnSessionRegisterListener SessionRegisterListener;

        Dictionary<string, OnUserMessageListener> messageListeners = new Dictionary<string, OnUserMessageListener>(); 

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
            session.SessionHandler = this;
        }

        /// <summary>
        /// 세션과 연결이 끊어졌을 때 수행
        /// </summary>
        /// <param name="session"></param>
        void ISessionHandler.OnDisconnected(Session session) {
            lock (allSession) {
                allSession.Remove(session.SessionID);
            }
        }

        void OnSessionRegister(Session session, CPacketInputStream p) {
            if (SessionRegisterListener(session)) {
                lock (allSession) {
                    allSession.Add(session.SessionID, session);
                }
            }
        }

        void ISessionHandler.OnMessage(Session session, byte[] buffer) {
            var packetInputStream = new CPacketInputStream(buffer);
            int header = packetInputStream.NextByte();
            switch (header) {
                case 0:
                    OnSessionRegister(session, packetInputStream);
                    break;
                case 1: {
                        string message = packetInputStream.NextString();
                        if (messageListeners.TryGetValue(message, out var listener)) {
                            listener(session, packetInputStream);
                        }
                        break;
                    }
            }
        }

        void ISessionHandler.OnSendCompleted(Session session) {
        }

        void ISessionHandler.OnDecodeFail(Session session, Exception ex, Memory<byte> buffer) {
            Console.WriteLine(ex);
            CPacket p = new CPacket();
            p.Push(buffer);
            session.Send(p);
        }
    }
}
