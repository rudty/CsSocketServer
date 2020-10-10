using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SocketServer {
    class CListener {
        private SocketAsyncEventArgs acceptArgs;
        private Socket listenSocket;
        private AutoResetEvent flowControlEvent = new AutoResetEvent(false);

        public delegate void NewClientHandler(Socket client, object token);
        public NewClientHandler callbackOnNewClient = null;

        public CListener() {

        }


        void onAcceptCompleted(object o, SocketAsyncEventArgs e) {
            var socketError = e.SocketError;
            var clientSocket = e.AcceptSocket;
            var userToken = e.UserToken;
            flowControlEvent.Set();
            if (socketError == SocketError.Success) {
                callbackOnNewClient?.Invoke(clientSocket, userToken);
            } else {
                Console.WriteLine("fail accept" + socketError);
            }
        }

        private void doAccept() {
            while (true) {
                try {
                    acceptArgs.AcceptSocket = null;
                    if (false == listenSocket.AcceptAsync(acceptArgs)) {
                        onAcceptCompleted(acceptArgs.AcceptSocket, acceptArgs);
                    }
                    flowControlEvent.WaitOne();
                } catch (Exception e) {
                    Console.WriteLine(e);
                }
            }
        }

        public void start(string host, int port, int backlog) {
            listenSocket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);

            var address = IPAddress.Any;
            if (host != "0.0.0.0") {
                address = IPAddress.Parse(host);
            }

            var endPoint = new IPEndPoint(address, port);

            try {
                listenSocket.Bind(endPoint);
                listenSocket.Listen(backlog);

                acceptArgs = new SocketAsyncEventArgs();
                acceptArgs.Completed += new EventHandler<SocketAsyncEventArgs>(onAcceptCompleted);
           
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }

            doAccept();
        }
    }
}
