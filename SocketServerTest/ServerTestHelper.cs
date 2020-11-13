using SocketServer;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using SocketServer.Net.IO;
using System.Collections.Generic;
using SocketServer.Core;

namespace SocketServerTest {
    public class LocalTestClient: IDisposable{
        readonly TcpClient client = new TcpClient();
        public LocalTestClient(int port) {
            client.NoDelay = true;
            client.ReceiveTimeout = 2000;
            client.SendTimeout = 2000;
            client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port));
        }

        public void Send(CPacket p) {
            var pack = p.Packing();
            var m = pack.AsMemory();
            client.Client.Send(m.Span);
        }

        public CPacket ReceivePacket() {
            var b = CPacketBufferManager.Obtain();
            int len = client.Client.Receive(b.Span);
            return new CPacket(new Slice<byte>(b.ToArray()));
        }

        void IDisposable.Dispose() {
        }
    }

    class EventListener {
        public string key;
        public Server.OnUserMessageListener listener;
        public EventListener(string key, Server.OnUserMessageListener listener) {
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
        public ServerWrapper AddEventListener(string k, Server.OnUserMessageListener l) {
            server.AddEventListener(k, l);
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
