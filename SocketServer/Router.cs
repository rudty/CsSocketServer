using SocketServer.Net;
using System;
using System.Threading.Tasks;

namespace SocketServer {
    public class RouterBase: IDisposable {
        private static AutoServerBootHelper helper = new AutoServerBootHelper();

        protected RouterBase() {
            Server server = helper.server;
            server.Boot(this);
            server.OnClientDisconnect += OnClientDisconnect;
        }

        protected Task OnClientDisconnect(Session session) {
            return Task.CompletedTask;
        }

        public void Dispose() {
            Server server = helper.server;
            server.OnClientDisconnect -= OnClientDisconnect;
        }
    }
}
