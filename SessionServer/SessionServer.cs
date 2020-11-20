using SocketServer;
using SocketServer.Net;
using SocketServer.Net.IO;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;

namespace SessionServer {
    public class SessionServer {
        Server server = new Server();
        ConcurrentDictionary<string, Session> allSessions = new ConcurrentDictionary<string, Session>();

        public SessionServer() {
            server.AddEventListener("hello", HelloRequest);
            server.OnClientDisconnect += OnClientDisconnect;
        }

        private Task OnClientDisconnect(Session session) {
            return Task.CompletedTask;
        }

        Task HelloRequest(Request req, Response res) {
            Hello h = req.Packet.Next(Hello.Parser);
            h.Value += 1;
            return Task.CompletedTask;
        }
    }
}
