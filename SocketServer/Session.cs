using System;
using System.Collections.Generic;
using System.Net.Sockets;
using SocketServer.Net.IO;
using SocketServer.Net;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace SocketServer {

    /// <summary>
    /// 접속 1 소켓 당 1개 생성하는 세션 관리 객체
    /// 
    /// </summary>
    public class Session {
        
        internal Socket Socket { get; private set; }

        internal NetworkService NetworkService { get; private set; }

        public readonly string SessionID;

        public object UserData { get; set; }

        bool online = true;

        MessageTaskRunner sessionRunner = new MessageTaskRunner();

        public ISessionEventListener OnSessionEventListener { private get; set; }

        public Session(Socket socket, NetworkService networkService) {
            SessionID = Guid.NewGuid().ToString();
            this.Socket = socket;
            this.NetworkService = networkService;
        }
 
        public void OnPacketReceive(CPacket message) {
            var c = OnSessionEventListener;
            if (c != null) {
                sessionRunner.Add(() => c.OnPacketReceived(this, message));
            }
        }

        public void OnMessageDecodeFail(Exception ex, Memory<byte> buffer) {
            var c = OnSessionEventListener;
            if (c != null) {
                sessionRunner.Add(() => c.OnPacketDecodeFail(this, ex, buffer));
            }
        }

        public void OnRemoved() {
            online = false;

            var c = OnSessionEventListener;
            if (c != null) {
                sessionRunner.Add(() => c.OnDisconnected(this));
            }
        }

        public void Send(CPacket p) {
            sessionRunner.Add(() => {
                if (online) {
                    NetworkService.Send(this, p);
                }
            });
        }
    }
}
