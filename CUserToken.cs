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

        public CNetworkService NetworkService { get; set; }

        Queue<CPacket> sendingQueue = new Queue<CPacket>();

        CMessageResolver messageResolver = new CMessageResolver();

        public IPeer Peer { get; set; }


        public void OnReceive(byte[] buffer, int offset, int byteTransferred) {
            //messageResolver.onReceive(buffer, offset, byteTransferred, onMessage);
            CPacket p = new CPacket();
            p.Push(buffer, offset, byteTransferred);
            Send(p);
        }

        public void onMessage(byte[] buffer) {
            //Peer?.onMessage(buffer);
            //send(new CPacket(buffer));
            //SendEventArgs.SetBuffer(SendEventArgs.Offset, buffer.Length);
            //Array.Copy(buffer, 0, SendEventArgs.Buffer, SendEventArgs.Offset, buffer.Length);
            //if (false == Socket.SendAsync(SendEventArgs)) {
            //    processSend(SendEventArgs);
            //}
        }

        public void onRemoved() {
            //sendingQueue.Clear();
            //Peer?.onRemoved();
        }

        public void OnSendCompleted(SocketAsyncEventArgs e) {
            if (e.BytesTransferred <= 0 || e.SocketError != SocketError.Success) {
                Console.WriteLine("send error");
            }
            lock (sendingQueue) {
                sendingQueue.Dequeue();
                if (sendingQueue.Count > 0) {
                    DoSendQueue();
                }
            }
        }

        void DoSendQueue() {
            lock (sendingQueue) {
                var msg = sendingQueue.Peek();
                //msg.recordSize();
               NetworkService.Send(this, msg);  
            }
        }

        public void Send(CPacket msg) {
            lock (sendingQueue) {
                sendingQueue.Enqueue(msg);
                if (sendingQueue.Count == 1) {
                    DoSendQueue();
                }
            }
        }

        public void disconnect() {
            Peer?.disconnect();
            try {
                Socket.Shutdown(SocketShutdown.Send);
            } catch {

            }
            Socket.Close();
        }
    }
}
