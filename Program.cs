using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace SocketServer {
    class MyPeer : IPeer {
        readonly CUserToken token;
        public MyPeer(CUserToken token) {
            this.token = token;
        }

        void IPeer.OnMessage(byte[] buffer) {
            Console.WriteLine("OnMessage");
        }

        void IPeer.OnDisconnected() {
            Console.WriteLine("OnDisconnected");
        }
        void IPeer.OnSendCompleted() {
            Console.WriteLine("OnSendCompleted");
        }
    }

    class Program {
        static void Main(string[] args) {
            CNetworkService svc = new CNetworkService();
            svc.OnSessionCreated += OnSessionCreated;
            svc.Listen("0.0.0.0", 8080);
            Console.WriteLine("sever start 8080");
            Console.ReadLine();
           
        }

        private static void OnSessionCreated(CUserToken token) {
            Console.WriteLine(token);
            token.Peer = new MyPeer(token);
        }

    }
}
