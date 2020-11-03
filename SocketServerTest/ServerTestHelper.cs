using SocketServer;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using SocketServer.Net.IO;

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
            var m = p.Packing();
            client.Client.Send(m.Span);
        }

        public Memory<byte> ReceiveAny() {
            byte[] b = new byte[1024];
            int len = client.Client.Receive(b);
            return b.AsMemory(3, len);
        }

        void IDisposable.Dispose() {
        }
    }

    /// <summary>
    /// 테스트가 끝날 때 서버 이벤트를 자동으로 제거하기 위해
    /// wrapper 를 생성
    /// </summary>
    public class ServerWrapper: IDisposable {
        readonly Server server;

        Server.OnUserMessageListener registerListener = null;
        public Server.OnUserMessageListener UserMessageListener {
            set {
                registerListener = value;
                server.UserMessageListener += value;
            }
            get {
                return null;
            }
        }

        public ServerWrapper(Server s) {
            server = s;
        }

        void IDisposable.Dispose() {
            if (registerListener != null) {
                server.UserMessageListener -= registerListener;
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
