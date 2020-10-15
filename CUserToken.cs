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

        CMessageResolver messageResolver = new CMessageResolver();

        public IPeer Peer { get; set; }


        public void onReceive(byte[] buffer, int offset, int byteTransferred) {
            messageResolver.onReceive(buffer, offset, byteTransferred, onMessage);
        }

        public void onMessage(byte[] buffer) {
            Peer?.onMessage(buffer);
        }

        public void onRemoved() {
            sendingQueue.Clear();
            Peer?.onRemoved();
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

        public void disconnect() {
            try {
                Socket.Shutdown(SocketShutdown.Send);
            } catch {

            }
            Socket.Close();
        }
    }
}
