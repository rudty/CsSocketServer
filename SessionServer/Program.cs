using System;
using System.Threading.Tasks;
using SocketServer;
using SocketServer.Net;
using SocketServer.Net.IO;

namespace SessionServer {
    class Program {
        static Task OnUserMessageListener(Session session, CPacket packetInputStream) {
            Hello h = Hello.Parser.ParseFrom(packetInputStream.Buffer.ToArray());
            

            return Task.CompletedTask;
        }
        static void Main(string[] args) {
            var s = new Server();
            s.AddEventListener("hello", OnUserMessageListener);
            s.ListenAndServe("0.0.0.0", 8080);
        }
    }
}
