using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using SocketServer.Core;

namespace SocketServerTest {

    [TestClass]
    public class SliceTest {
        [TestMethod]
        public void TestNew() {
            byte[] b = new byte[1024];
            new Slice<byte>(b, 0, 1024);
            new Slice<byte>(b, 3, 1021);
            new Slice<byte>(b, 1023, 1);
        }

        [TestMethod]
        public void Indexer() {
            byte[] b = new byte[10];
            Slice<byte> s = new Slice<byte>(b, 0, 1);
            Console.WriteLine(s[0]);
            s[0] = 3;

            try {
                s[1] = 2;
                Assert.Fail();
            } catch (IndexOutOfRangeException) {

            }

        }

        [TestMethod]
        public void Range() {
            byte[] b = new byte[1024];
            var s = new Slice<byte>(b, 0, 1024);
            var s2 = s[0..2];
            var s3 = s[0..1024];
            var s4 = s[..^256];
        }
    } 
}
