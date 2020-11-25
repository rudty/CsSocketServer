using SocketServer;
using SocketServer.Net;
using SocketServer.Net.IO;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;

namespace SessionServer {
    public class SessionRouter: RouterBase {
        //ConcurrentDictionary<string, Session> allSessions = new ConcurrentDictionary<string, Session>();

        [MessageHandler("hello")]
        public Task HelloRequest(Request req, Response res) {
            Hello h = req.Packet.Next(Hello.Parser);
            h.Value *= 2;
            res.Packet.Add(h);
            return Task.CompletedTask;
        }
    }
}
