﻿using System;
using System.Net.Sockets;
using SocketServer.Net.IO;

namespace SocketServer.Net {

    /// <summary>
    /// Receive/Send 클래스
    /// </summary>
    public class NetworkService {
        public delegate void SessionHandler(Session session);
        public event SessionHandler OnSessionCreated;

        void OnNewClient(Socket client) {
            Session session = new Session {
                Socket = client,
                NetworkService = this
            };

            OnSessionCreated(session);
            DoReceive(session);
        }

        /// <summary>
        /// 접속이 시작되면 클라이언트로부터 입력을 받음
        /// </summary>
        /// <param name="session">계속 입력을 받을 세션</param>
        async void DoReceive(Session session) {
            var clientSocket = session.Socket;
            var buf = CPacketBufferManager.Obtain();
            while (true) {
                try {
                    var len = await clientSocket.ReceiveAsync(buf, SocketFlags.None);
                    if (len > 0) {
                        session.OnReceive(buf.Slice(0, len));
                    }
                    if (len == 0) {
                        Console.WriteLine("0");
                    }
                } catch (Exception e) {
                    Console.WriteLine(e);
                    CloseClient(session);
                }
            }
        }
        internal async void Send(Session session, CPacket p) {
            await session.Socket.SendAsync(p.Packing(), SocketFlags.None);
            session.OnSendCompleted();
        }

        void CloseClient(Session session) {
            session.OnRemoved();

            try {
                session.Socket.Shutdown(SocketShutdown.Send);
            } catch {

            }
            session.Socket.Close();
        }

        public void ListenAndServe(string host, int port) {
            var listener = new Listener();
            listener.OnNewClient += OnNewClient;
            listener.Start(host, port);
        }
    }
}