using System;
using System.IO;
using System.Threading.Tasks;
using SocketServer;
using SocketServer.Net;
using SocketServer.Net.IO;

namespace SessionServer {
    class AA : Stream {
        public override bool CanRead => throw new NotImplementedException();

        public override bool CanSeek => throw new NotImplementedException();

        public override bool CanWrite => throw new NotImplementedException();

        public override long Length => throw new NotImplementedException();

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Flush() {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count) {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin) {
            throw new NotImplementedException();
        }

        public override void SetLength(long value) {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count) {
            throw new NotImplementedException();
        }
    }
    class Program {
        static Task OnUserMessageListener(Session session, CPacket packetInputStream) {
            
            Hello h = Hello.Parser.ParseFrom(packetInputStream.Buffer.ToArray());
            

            return Task.CompletedTask;
        }
        static void Main(string[] args) {
            var s = new Server();
            s.AddEventListener("hello", OnUserMessageListener);
            s.ListenAndServe("0.0.0.0", 8080);
        }
    }
}
