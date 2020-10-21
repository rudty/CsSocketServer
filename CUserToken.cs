using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace SocketServer {
    class CUserToken {

        bool online = true;

        internal SocketAsyncEventArgs ReceiveEventArgs { get; set; }
        internal SocketAsyncEventArgs SendEventArgs { get; set; }
        internal Socket Socket { get; set; }

        internal CNetworkService NetworkService { get; set; }

        Queue<CPacket> sendingQueue = new Queue<CPacket>();

        //CMessageResolver messageResolver = new CMessageResolver();

        public IPeer Peer { private get; set; }
    
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

        public void OnRemoved() {
            Peer?.OnDisconnected();

            lock (sendingQueue) {
                online = false;
                while (sendingQueue.Count > 0) {
                    var p = sendingQueue.Dequeue();
                    p.Recycle();
                }
            }
        }

        public void OnSendCompleted(SocketAsyncEventArgs e) {
            lock (sendingQueue) {
                var lastSend = sendingQueue.Dequeue();
                lastSend.Recycle();
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
                if (online) {
                    sendingQueue.Enqueue(msg);
                    if (sendingQueue.Count == 1) {
                        DoSendQueue();
                    }
                }
            }
        }
    }
}
