using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer {

    class Program {
        static void Main(string[] args) {
            Server s = new Server();
            s.ListenAndServe("0.0.0.0", 8080);
        } 
        private static void OnSessionCreated(Session token) {
        }

    }
}
