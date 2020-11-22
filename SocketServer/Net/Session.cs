using System;
using System.IO;
using System.Net.Sockets;
using SocketServer.Core;
using SocketServer.TaskRunner;

namespace SocketServer.Net {

    /// <summary>
    /// 접속 1 소켓 당 1개 생성하는 세션 관리 객체
    /// 
    /// </summary>
    public class Session: IDisposable {

        internal Stream ClientStream;

        internal Socket Socket { get; private set; }

        internal NetworkService NetworkService { get; private set; }

        public readonly string SessionID;

        public object UserData { get; set; }

        bool online = true;

        readonly SingleTaskRunner sessionTaskExecutor = new SingleTaskRunner();

        /// <summary>
        /// 패킷 순차적으로 보냄
        /// </summary>
        readonly SingleTaskRunner sendExecutor = new SingleTaskRunner();

        private readonly ISessionEventListener sessionEventListener;

        public Session(Socket socket, NetworkService networkService, ISessionEventListener sessionEventListener) {
            SessionID = Guid.NewGuid().ToString();
            this.Socket = socket;
            this.NetworkService = networkService;
            this.sessionEventListener = sessionEventListener;
            ClientStream = new NetworkStream(socket);
        }

        public void OnPacketReceive(CPacket message) {
            sessionTaskExecutor.Add(() => sessionEventListener.OnPacketReceived(this, message));
        }

        public void OnPacketDecodeFail(Exception ex, byte[] buffer) {
            sessionTaskExecutor.Add(() => sessionEventListener.OnPacketDecodeFail(this, ex, buffer));
        }

        public void OnRemoved() {
            online = false;
            sessionTaskExecutor.Add(() => sessionEventListener.OnDisconnected(this));
        }

        public void Send(CPacket p) {
            sendExecutor.Add(() => {
                if (online) {
                    NetworkService.Send(this, p);
                }
            });
        }

        public void Dispose() {
            try {
                Socket.Shutdown(SocketShutdown.Send);
            } catch {

            }
            ClientStream.Close();
            Socket.Close();
        }
    }
}
