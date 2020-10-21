using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer {
    class CNetworkService {

        const int MAX_CONNECTION_SIZE = 100;
        const int DEFAULT_BUFFER_SIZE = 1024;
        private SocketAsyncEventArgsPool receiveEventArgsPool = new SocketAsyncEventArgsPool(MAX_CONNECTION_SIZE);
        private SocketAsyncEventArgsPool sendEventArgsPool = new SocketAsyncEventArgsPool(MAX_CONNECTION_SIZE);
        private BufferManager receiveBufferManager = new BufferManager(MAX_CONNECTION_SIZE * DEFAULT_BUFFER_SIZE, DEFAULT_BUFFER_SIZE);

        public delegate void SessionHandler(CUserToken token);
        public event SessionHandler SessonCreateCallback;

        public CNetworkService() {
            receiveBufferManager.InitBuffer();
            for (int i  = 0; i < MAX_CONNECTION_SIZE; ++i) {
                CUserToken token = new CUserToken();

                var rArgs = new SocketAsyncEventArgs();
                rArgs.Completed += OnReceive;
                rArgs.UserToken = token;
                receiveEventArgsPool.Push(rArgs);
                receiveBufferManager.SetBuffer(rArgs);

                var sArgs = new SocketAsyncEventArgs();
                sArgs.Completed += OnSendCompleted;
                sArgs.UserToken = token;
                sendEventArgsPool.Push(sArgs);
                // send는 buffer 할당하지 않았음 
            }
        }

        void OnSendCompleted(object sender, SocketAsyncEventArgs e) {
            var token = e.UserToken as CUserToken;
            if (e.BytesTransferred <= 0 || e.SocketError != SocketError.Success) {
                Console.WriteLine("send error");
                CloseClient(token);
            } else {
                token.OnSendCompleted(e);
            }
        }

        void OnNewClient(Socket client) {
            var receiveArgs = receiveEventArgsPool.Pop();
            var sendArgs = sendEventArgsPool.Pop();

            var userToken = receiveArgs.UserToken as CUserToken;
            userToken.Socket = client;
            userToken.ReceiveEventArgs = receiveArgs;
            userToken.SendEventArgs = sendArgs;
            userToken.NetworkService = this;

            SessonCreateCallback?.Invoke(userToken);
            DoReceive(userToken);
        }

        void DoReceive(CUserToken token) {
            var receiveArgs = token.ReceiveEventArgs;
            if (false == token.Socket.ReceiveAsync(receiveArgs)) {
                OnReceive(token.Socket, receiveArgs);
            }
        }

        /// <summary>
        /// 클라이언트로부터 무언가를 전달받았을 때 호출됩니다.
        /// </summary>
        /// <param name="nil">사용하지 말 것</param>
        /// <param name="receiveArgs">소켓 이벤트</param>
        void OnReceive(object nil, SocketAsyncEventArgs receiveArgs) {
            CUserToken token = receiveArgs.UserToken as CUserToken;
            try {
                if (receiveArgs.LastOperation != SocketAsyncOperation.Receive) {
                    CloseClient(token);
                    throw new ArgumentException("last operation completed on the socket was not a receive");
                }

                if (receiveArgs.SocketError != SocketError.Success) {
                    CloseClient(token);
                    throw new InvalidOperationException($"error {receiveArgs.SocketError}, transferred {receiveArgs.BytesTransferred}");
                }

                if (receiveArgs.BytesTransferred == 0) {
                    CloseClient(token);
                } 
                    
                token.OnReceive(receiveArgs.Buffer, receiveArgs.Offset, receiveArgs.BytesTransferred);
                
                Task.Run(() => DoReceive(token));
            } catch (Exception e) {
                Console.WriteLine(e);
            }
        }

        internal void Send(CUserToken token, CPacket p) {
            var e = token.SendEventArgs;
            e.SetBuffer(p.Buffer.Slice(0, p.Position));
            if (false == token.Socket.SendAsync(e)) {
                OnSendCompleted(token.Socket, e);
            }
        }

        void CloseClient(CUserToken token) {
            token.OnRemoved();

            try {
                token.Socket.Shutdown(SocketShutdown.Send);
            } catch {

            }
            token.Socket.Close();

            receiveEventArgsPool?.Push(token.ReceiveEventArgs);
            sendEventArgsPool?.Push(token.SendEventArgs);
        }

        public void Listen(string host, int port) {
            var listener = new CListener();
            listener.OnNewClient += OnNewClient;
            listener.Start(host, port);
        }
    }
}
