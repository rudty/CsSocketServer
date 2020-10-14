using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace SocketServer {
    class CUserToken {

        internal SocketAsyncEventArgs ReceiveEventArgs { get; set; }
        internal SocketAsyncEventArgs SendEventArgs { get; set; }
        public Socket Socket { get; set; }

        Queue<CPacket> sendingQueue = new Queue<CPacket>();

        public void onReceive(byte[] buffer, int offset, int byteTransferred) {

        }

        public void onRemoved() {

        }

        public void processSend(SocketAsyncEventArgs e) {
            if (e.BytesTransferred <= 0 || e.SocketError != SocketError.Success) {
                Console.WriteLine("send error");
            }
            lock (sendingQueue) {
                sendingQueue.Dequeue();
                if (sendingQueue.Count > 0) {
                    startSend();
                }
            }
        }

        void startSend() {
            lock (sendingQueue) {
                var msg = sendingQueue.Peek();
                msg.recordSize();
                SendEventArgs.SetBuffer(SendEventArgs.Offset, msg.Position);
                Array.Copy(msg.Buffer, 0, SendEventArgs.Buffer, SendEventArgs.Offset,  msg.Position);
                if (false == Socket.SendAsync(SendEventArgs)) {
                    processSend(SendEventArgs);
                }
            }
        }

        public void send(CPacket msg) {
            var p = msg.Clone();
            lock (sendingQueue) {
                sendingQueue.Enqueue(msg);
                if (sendingQueue.Count <= 0) {
                    startSend();
                }
            }
        }
    }
}
