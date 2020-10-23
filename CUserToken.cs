using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace SocketServer {
    class CUserToken {

        internal SocketAsyncEventArgs ReceiveEventArgs { get; set; }
        internal SocketAsyncEventArgs SendEventArgs { get; set; }
        internal Socket Socket { get; set; }

        internal CNetworkService NetworkService { get; set; }

        bool online = true;

        Queue<CPacket> sendingQueue = new Queue<CPacket>();

        CMessageResolver messageResolver = new CMessageResolver();

        public IPeer Peer { private get; set; }

        public CUserToken() {
            messageResolver.OnMessageReceive += OnMessageReceive;
            messageResolver.OnMessageDecodeFail += OnMessageDecodeFail;
        }
    
        public void OnReceive(byte[] buffer, int offset, int byteTransferred) {
            messageResolver.OnRawByteReceive(buffer, offset, byteTransferred);
 
        }

        public void OnMessageReceive(byte[] buffer) {
            //Peer?.onMessage(buffer);
            //send(new CPacket(buffer));
            //SendEventArgs.SetBuffer(SendEventArgs.Offset, buffer.Length);
            //Array.Copy(buffer, 0, SendEventArgs.Buffer, SendEventArgs.Offset, buffer.Length);
            //if (false == Socket.SendAsync(SendEventArgs)) {
            //    processSend(SendEventArgs);
            //}
        }

        public void OnMessageDecodeFail(Exception ex, byte[] buffer) {
            Console.WriteLine(ex);
            CPacket p = new CPacket();
            p.Push(buffer, 0, buffer.Length);
            Send(p);
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
                DoSendQueue();
            }
        }

        void DoSendQueue() {
            if (online) {
                if (sendingQueue.Count > 0) {
                    var msg = sendingQueue.Peek();
                    //msg.recordSize();
                    NetworkService.Send(this, msg);
                }
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
