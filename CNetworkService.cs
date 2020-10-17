using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer {
    class CNetworkService {

        const int maxConnections = 10000;
        private SocketAsyncEventArgsPool receiveEventArgsPool = new SocketAsyncEventArgsPool(maxConnections);
        private SocketAsyncEventArgsPool sendEventArgsPool = new SocketAsyncEventArgsPool(maxConnections);
        private BufferManager bufferManager = new BufferManager(maxConnections * 2 * 1024, 1024);

        public delegate void SessionHandler(CUserToken token);
        public SessionHandler SessonCreateCallback { get; set; }

        public CNetworkService() {
            bufferManager.InitBuffer();
            for (int i  = 0; i < maxConnections; ++i) {
                CUserToken token = new CUserToken();

                var rArgs = new SocketAsyncEventArgs();
                rArgs.Completed += receiveComplete;
                rArgs.UserToken = token;
                receiveEventArgsPool.Push(rArgs);

                bufferManager.SetBuffer(rArgs);

                var sArgs = new SocketAsyncEventArgs();
                sArgs.Completed += sendCompleted;
                sArgs.UserToken = token;
                sendEventArgsPool.Push(sArgs);

                bufferManager.SetBuffer(sArgs);
            }
        }

        private void sendCompleted(object sender, SocketAsyncEventArgs e) {
            var token = e.UserToken as CUserToken;
            token.processSend(e);
        }

        private void receiveComplete(object sender, SocketAsyncEventArgs e) {
            if (e.LastOperation == SocketAsyncOperation.Receive) {
                processReceive(e);
            } else {
                throw new ArgumentException("last operation completed on the socket was not a receive");
            }
        }

        void onNewClient(Socket client, object token) {
            SocketAsyncEventArgs receiveArgs = receiveEventArgsPool.Pop();
            SocketAsyncEventArgs sendArgs = sendEventArgsPool.Pop();

            if (SessonCreateCallback != null) {
                var t = receiveArgs.UserToken as CUserToken;
                SessonCreateCallback.Invoke(t);
            }

            beginReceive(client, receiveArgs, sendArgs);

        }

        void processReceive(SocketAsyncEventArgs receiveArgs) {
            CUserToken token = receiveArgs.UserToken as CUserToken;
            if (receiveArgs.SocketError == SocketError.Success) {
                bool receiveRequire = true;
                if (receiveArgs.BytesTransferred > 0) {
                    token.onReceive(receiveArgs.Buffer, receiveArgs.Offset, receiveArgs.BytesTransferred);

                    if (token.Socket.ReceiveAsync(receiveArgs)) {
                        receiveRequire = false;
                    }
                } 

                if (receiveRequire) {
                    Task.Run(() => beginReceive(token.Socket, receiveArgs, token.SendEventArgs));
                }
            } else {
                Console.WriteLine("error {0}, transferred {1}",
                    receiveArgs.SocketError,
                    receiveArgs.BytesTransferred);
                closeClientSocket(token);
            }
        }

        void closeClientSocket(CUserToken token) {
            token.onRemoved();
            receiveEventArgsPool?.Push(token.ReceiveEventArgs);
            sendEventArgsPool?.Push(token.SendEventArgs);
        }

        void beginReceive(Socket client, SocketAsyncEventArgs receiveArgs, SocketAsyncEventArgs sendArgs) {
            CUserToken token = receiveArgs.UserToken as CUserToken;
            token.Socket = client;
            token.SendEventArgs = sendArgs;
            token.ReceiveEventArgs = receiveArgs;
            
            if (false == client.ReceiveAsync(receiveArgs)) {
                processReceive(receiveArgs);
            }
        }

        public void listen(string host, int port, int backlog) {
            var listener = new CListener();
            listener.callbackOnNewClient += onNewClient;
            listener.start(host, port, backlog);
        }
    }
}
