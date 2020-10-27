using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer {
    public class CNetworkService {
        public delegate void SessionHandler(Session token);
        public event SessionHandler OnSessionCreated;

        void OnNewClient(Socket client) {
            Session userToken = new Session {
                Socket = client,
                NetworkService = this
            };

            OnSessionCreated(userToken);
            DoReceive(userToken);
        }

        async void DoReceive(Session token) {
            var clientSocket = token.Socket;
            var buf = CPacketBufferManager.Obtain();
            while (true) {
                try {
                    var len = await clientSocket.ReceiveAsync(buf, SocketFlags.None);
                    if (len > 0) {
                        token.OnReceive(buf.Slice(0, len));
                    }
                } catch (Exception e) {
                    Console.WriteLine(e);
                    CloseClient(token);
                }
            }
        }
        internal async void Send(Session token, CPacket p) {
            await token.Socket.SendAsync(p.Buffer, SocketFlags.None);
            token.OnSendCompleted();
        }

        void CloseClient(Session token) {
            token.OnRemoved();

            try {
                token.Socket.Shutdown(SocketShutdown.Send);
            } catch {

            }
            token.Socket.Close();
        }

        public void Listen(string host, int port) {
            var listener = new CListener();
            listener.OnNewClient += OnNewClient;
            listener.Start(host, port);
        }
    }
}
