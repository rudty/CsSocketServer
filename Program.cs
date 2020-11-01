using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
namespace SocketServer {
    class Program {

        class TestObject {
            public int a;
            public string b;
        }

        class NestedTestObject {
            public int x;
            public TestObject o;
            //public System.Action c;
        }

        public void Push<T>(T o) where T : class {
            return;  
        }
        static void Main(string[] args) {
            //Server s = new Server();
            //s.SetOnPacketReceivedCallback(1, new PacketReceive());
            //s.ListenAndServe("0.0.0.0", 8080);
            Console.WriteLine(new NestedTestObject().GetType().BaseType);
            var a = new Action(() => { });
            Console.WriteLine(a.GetType().BaseType);
        } 
     }
}
