using System.Threading.Tasks;
using SocketServer;
using SocketServer.Net;
using SocketServer.Net.IO;
namespace SessionServer {
    class Program {
        static Task HelloRequest(Request req) {
            Hello h = req.Packet.Next(Hello.Parser);
            h.Value += 1;
            return Task.CompletedTask;
        }

        static void Main(string[] args) {
            var s = new Server();
            s.AddEventListener("hello", HelloRequest);
            s.ListenAndServe("0.0.0.0", 8080);
        }
    }
}
