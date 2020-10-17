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
        private const int DEFAULT_BACKLOG_SIZE = 511;
        public delegate void NewClientHandler(Socket client);
        public event NewClientHandler OnNewClient;

        private Socket acceptSocket;
        private AutoResetEvent flowControlEvent; 
        
        /// <summary>
        /// 연결된 클라이언트를 받습니다 
        /// flowControlEvent.Set() 을 호출한 이후로는
        /// 다시 DoAccept() 함수가 진행되므로 
        /// 변수 e를 참조 하지 마십시오
        /// </summary>
        /// <param name="nil">사용하지 말 것.</param>
        /// <param name="e">소켓이벤트</param>
        void OnAcceptCompleted(object nil, SocketAsyncEventArgs e) {
            var socketError = e.SocketError;
            var clientSocket = e.AcceptSocket;
            flowControlEvent.Set();

            if (socketError == SocketError.Success) {
                Task.Run(() => {
                    OnNewClient?.Invoke(clientSocket);
                });
            } else {
                Console.WriteLine("fail accept" + socketError);
            }
        }

        void DoAccept() {
            var acceptArgs = new SocketAsyncEventArgs();
            acceptArgs.Completed += OnAcceptCompleted;
            while (true) {
                try {
                    acceptArgs.AcceptSocket = null;
                    if (false == acceptSocket.AcceptAsync(acceptArgs)) {
                        OnAcceptCompleted(acceptArgs.AcceptSocket, acceptArgs);
                    }
                    flowControlEvent.WaitOne();
                } catch (Exception e) {
                    Console.WriteLine(e);
                }
            }
        }

        void BindAndListen(string host, int port) {
            var address = IPAddress.Any;
            if (host != "0.0.0.0") {
                address = IPAddress.Parse(host);
            }

            var endPoint = new IPEndPoint(address, port);

            try {
                acceptSocket.Bind(endPoint);
                acceptSocket.Listen(DEFAULT_BACKLOG_SIZE);
            } catch (Exception e) {
                Console.WriteLine(e);
                throw e;
            }
        }

        public void Start(string host, int port) {
            acceptSocket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);

            flowControlEvent = new AutoResetEvent(false);

            BindAndListen(host, port);
            DoAccept();
        }
    }
}
