using System;

namespace SocketServer {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("Hello World!");

            CNetworkService svc = new CNetworkService();
            svc.SessonCreateCallback += onSessionCreated;
            svc.listen("0.0.0.0", 8080, 100);
            Console.WriteLine("sever start 8080");
            Console.ReadLine();
        }

        private static void onSessionCreated(CUserToken token) {
            Console.WriteLine(token);
        }
    }
}
