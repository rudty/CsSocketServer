using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using SocketServer.Net.IO;
using SocketServer.Core;
namespace SocketServer.Net {

    /// <summary>
    /// Receive/Send 클래스
    /// </summary>
    public class NetworkService: IDisposable {
        private readonly ISessionEventListener sessionEventListener;
        private Listener listener = new Listener();

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
            var packetReader = new CPacketStreamReader(session.ClientStream);
                while (true) {
                    try {
                        var p = await packetReader.ReceiveAsync();
                        if (p == null) {
                            break;
                        }
                        session.OnPacketReceive(p);
                    } catch (PacketDecodeFailException e) {
                        session.OnPacketDecodeFail(e, e.Buffer);
                        break;
                    } catch (Exception e) {
                        Console.WriteLine(e);
                        break;
                    }
              }
              CloseClient(session);
        }

        internal Task Send(Session session, CPacket p) {
            try {
                p.Sealed();
                return session.ClientStream.WriteAsync(p);
             } catch (Exception e) {
                Console.WriteLine(e);
            }
            return Task.CompletedTask;
        }

        public void CloseClient(Session session) {
            session.OnRemoved();
            session.Dispose();
        }

        public void ListenAndServe(string host, int port) {
            listener.OnNewClient += OnNewClient;
            listener.Start(host, port);
        }

        public void Dispose() {
            listener.OnNewClient -= OnNewClient;
        }
    }
}
