using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace SocketServer {
    class CNetworkService {

        int maxConnections;
        private SocketAsyncEventArgsPool receiveEventArgsPool;
        private SocketAsyncEventArgsPool sendEventArgsPool;
        private BufferManager bufferManager;

        CNetworkService() {
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
            throw new NotImplementedException();
        }

        private void receiveComplete(object sender, SocketAsyncEventArgs e) {
            throw new NotImplementedException();
        }

        void onNewClient(Socket client, object token) {
            SocketAsyncEventArgs receiveArgs = receiveEventArgsPool.Pop();
            SocketAsyncEventArgs sendArgs = receiveEventArgsPool.Pop();

            //if (sessionCreatedCallback != null)


        }

        void beginReceive(Socket client, SocketAsyncEventArgs receiveArgs, SocketAsyncEventArgs sendArgs) {
            CUserToken token = receiveArgs.UserToken as CUserToken;
            token.Socket = client;
            token.SendEventArgs = sendArgs;
            token.ReceiveEventArgs = receiveArgs;
        }

        public void listen(string host, int port, int backlog) {
            var listener = new CListener();
            listener.callbackOnNewClient += onNewClient;
            listener.start(host, port, backlog);
        }
    }
}
