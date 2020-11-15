using System;
using System.IO;
using System.Threading.Tasks;
using Google.Protobuf;
using SocketServer;
using SocketServer.Core;
using SocketServer.Net;
using SocketServer.Net.IO;
namespace SessionServer {
    class Program {
        static Task OnUserMessageListener(Session session, string message, CPacket packet) {
            Hello h = packet.Next(Hello.Parser);
            h.Value += 1;
            return Task.CompletedTask;
        }
        static void Main(string[] args) {
            var s = new Server();
            s.AddEventListener("hello", OnUserMessageListener);
            s.ListenAndServe("0.0.0.0", 8080);
        }
    }
}
