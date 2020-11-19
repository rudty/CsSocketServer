using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using SocketServer.Net.IO;
using SocketServer.Core;
namespace SocketServer.Net {

    /// <summary>
    /// Receive/Send 클래스
    /// </summary>
    public class NetworkService {
        private ISessionEventListener sessionEventListener;

        public NetworkService(ISessionEventListener sessionEventListener) {
            if (sessionEventListener == null) {
                throw new NullReferenceException("sessionEventListener must impl");
            }
            this.sessionEventListener = sessionEventListener;
        }

        void OnNewClient(Socket client) {
            Session session = new Session(client, this, sessionEventListener); 
            Task.Run(async () => {
                await sessionEventListener.OnCreate(session);
                await DoReceive(session);
            });
        }

        /// <summary>
        /// 접속이 시작되면 클라이언트로부터 입력을 받음
        /// </summary>
        /// <param name="session">계속 입력을 받을 세션</param>
        async Task DoReceive(Session session) {
            var clientSocket = session.Socket;
            using var networkStream = new NetworkStream(clientSocket);
            var packetReader = new PacketReader(networkStream);
            try {
                while (true) {
                    try {
                        var p = await packetReader.ReceiveAsync();
                        if (p == null) {
                            break;
                        }
                        session.OnPacketReceive(p);
                    } catch (PacketDecodeFailException e) {
                        await sessionEventListener.OnPacketDecodeFail(session, e, e.Buffer);
                    }
                }
            } catch (Exception e) {
                Console.WriteLine(e);
            }
            CloseClient(session);
        }

        internal async void Send(Session session, CPacket p) {
            var pack = p.Packing();
            while (true) {
                int len = await session.Socket.SendAsync(pack, SocketFlags.None);
                if (len == pack.Length) {
                    break;
                }
                pack = pack.Slice(0, len);
            }
        }

        public void CloseClient(Session session) {
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
