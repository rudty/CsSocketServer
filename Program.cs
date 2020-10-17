using System;

namespace SocketServer {
    class MyPeer : IPeer {
        readonly CUserToken token;
        public MyPeer(CUserToken token) {
            this.token = token;
        }

        void IPeer.disconnect() {
            Console.WriteLine("disconnect");
        }

        void IPeer.onMessage(byte[] buffer) {
            Console.WriteLine("onMessage");
        }

        void IPeer.onRemoved() {
            Console.WriteLine("onRemoved");
        }

        void IPeer.processUserOperation() {
            Console.WriteLine("processUserOperation");
        }

        void IPeer.send() {
            Console.WriteLine("send");
        }
    }

    class Program {
        static void Main(string[] args) {
            CNetworkService svc = new CNetworkService();
            svc.SessonCreateCallback += onSessionCreated;
            svc.listen("0.0.0.0", 8080, 100);
            Console.WriteLine("sever start 8080");
            Console.ReadLine();


        }

        private static void onSessionCreated(CUserToken token) {
            Console.WriteLine(token);
            token.Peer = new MyPeer(token);
        }

    }
}
