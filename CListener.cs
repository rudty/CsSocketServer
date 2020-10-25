using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SocketServer {

    /// <summary>
    /// 클라이언트가 연결을 시도헀을때 Accept를 하는 함수
    /// </summary>
    class CListener {
        public delegate void NewClientHandler(Socket client);
        public event NewClientHandler OnNewClient;

        readonly Socket serverSocket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);

        async void DoAccept() {
            while (true) {
                Socket client = null;

                try {
                    client = await serverSocket.AcceptAsync();
                } catch (Exception e) {
                    Console.WriteLine(e);
                }

                _ = Task.Run(() => {
                    OnNewClient(client);
                });
            }
        }

        void BindAndListen(string host, int port) {
            var address = IPAddress.Any;
            if (host != "0.0.0.0") {
                address = IPAddress.Parse(host);
            }

            var endPoint = new IPEndPoint(address, port);

            try {
                serverSocket.Bind(endPoint);
                serverSocket.Listen(Consts.DEFAULT_ACCEPT_BACKLOG_SIZE);
            } catch (Exception e) {
                Console.WriteLine(e);
                throw e;
            }
        }

        public void Start(string host, int port) {
            BindAndListen(host, port);
            DoAccept();
        }
    }
}
