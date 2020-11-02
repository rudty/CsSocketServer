using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Sockets;
using System.Text;
using SocketServer;
using System;

namespace SocketServerTest {
    //[TestClass]
    //public class ServerPeer {

    //    class TestPeer : ISessionHandler {
    //        readonly Session token;
    //        public TestPeer(Session token) {
    //            this.token = token;
    //        }

    //        void ISessionHandler.OnMessage(byte[] buffer) {
    //            Console.WriteLine(Encoding.UTF8.GetString(buffer));
    //        }

    //        void ISessionHandler.OnDisconnected() {
    //        }
    //        void ISessionHandler.OnSendCompleted() {
    //        }
    //    }


    //    [TestMethod]
    //    public void SocketHello() {
    //        var token = new Session();
    //        token.SessionHandler = new TestPeer(token);

    //        byte[] header = new byte[]{
    //            0x8f,
    //            5,
    //            0 
    //        };
            
    //        token.OnReceive(new Memory<byte>(header));
    //        token.OnReceive(new Memory<byte>(Encoding.UTF8.GetBytes("HELLO")));
    //    }
    //}
}
