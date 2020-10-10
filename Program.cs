using System;

namespace SocketServer {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("Hello World!");
            new CListener().start("0.0.0.0", 8080, 10);
            Console.ReadLine();
        }
    }
}
