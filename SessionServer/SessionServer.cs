using SocketServer;
using SocketServer.Net;
using SocketServer.Net.IO;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;

namespace SessionServer {
    public class SessionRouter {
        Server server = new Server();
        //ConcurrentDictionary<string, Session> allSessions = new ConcurrentDictionary<string, Session>();

        public SessionRouter() {
            server.Boot(this);
            server.OnClientDisconnect += OnClientDisconnect;
        }

        private Task OnClientDisconnect(Session session) {
            return Task.CompletedTask;
        }

        [MessageHandler("hello")]
        public Task HelloRequest(Request req, Response res) {
            Hello h = req.Packet.Next(Hello.Parser);
            h.Value *= 2;
            res.Packet.Add(h);
            return Task.CompletedTask;
        }

        public void ListenAndServe(string host, int port) {
            server.ListenAndServe(host, port);
        }
    }
}
