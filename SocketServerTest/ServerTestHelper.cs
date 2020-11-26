using SocketServer;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using SocketServer.Core;
using Google.Protobuf;
using SocketServer.Net.IO;

namespace SocketServerTest {
    public class LocalTestClient: IDisposable{
        readonly TcpClient client = new TcpClient();
        public LocalTestClient(int port) {
            client.NoDelay = true;
            client.ReceiveTimeout = 200000;
            client.SendTimeout = 200000;
            client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port));
        }

        public void Send(CPacket p) {
            p.Sealed();
            client.Client.Send(p.Buffer, 0, p.Position, SocketFlags.None);
        }

        public CPacket ReceivePacket() {
            var p = CPacket.NewSend;
            int len = client.Client.Receive(p.Buffer, 0, p.Buffer.Length, SocketFlags.None);
            return p;
        }

        public void Dispose() {
            client.Close();
        }
    }

    class EventListener {
        public string key;
        public Server.ClientMessageListener listener;
        public EventListener(string key, Server.ClientMessageListener listener) {
            this.key = key;
            this.listener = listener;
        }

    }

    /// <summary>
    /// 테스트가 끝날 때 서버 이벤트를 자동으로 제거하기 위해
    /// wrapper 를 생성
    /// </summary>
    public class ServerWrapper: IDisposable {
        readonly Server server;

        List<EventListener> eventListeners = new List<EventListener>();
        public ServerWrapper AddEventListener(string k, Server.ClientMessageListener l) {
            server.AddMessageHandler(k, l);
            eventListeners.Add(new EventListener(k, l));
            return this;
        }

        public ServerWrapper(Server s) {
            server = s;
        }

        void IDisposable.Dispose() {
            foreach (var l in eventListeners) {
                server.RemoveEventListener(l.key, l.listener);
            }    
        }
    }

    public class TestConnection {
        public ServerWrapper TestServer { get; set; }
        public LocalTestClient TestClient { get; set; }
    }

    class ServerTestHelper {
        public static ThreadLocal<TestConnection> Connection = new ThreadLocal<TestConnection>(() => {
            RunBackgroundServer(out Server s, out LocalTestClient c);
            var conn = new TestConnection {
                TestServer = new ServerWrapper(s),
                TestClient = c
            };
            return conn;
        });

        public static ServerWrapper TestServer {
            get { return Connection.Value.TestServer;  }
        }

        public static LocalTestClient TestClient {
            get { return Connection.Value.TestClient; }
        }

        static int GetRandomPort() {
            Random r = new Random();
            return r.Next(10000, 50000);
        }

        public static void RunBackgroundServer(out Server server, out LocalTestClient client) {
            Server s = new Server();
            int port = GetRandomPort();

            Task.Run(() => {
                while (true) {
                    try {
                        s.ListenAndServe("0.0.0.0", port);
                        break;
                    } catch {
                        port = GetRandomPort();
                    }
                }
            });
            Thread.Sleep(1000);
            var c = new LocalTestClient(port);

            server = s;
            client = c;
        }
    }
}
