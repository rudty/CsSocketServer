﻿using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
namespace SocketServer {
    class Program {
        static void Main(string[] args) {
            Server s = new Server();
            //s.SetOnPacketReceivedCallback(1, new PacketReceive());
            s.ListenAndServe("0.0.0.0", 8080);
        } 
     }
}