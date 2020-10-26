using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace SocketServer {
    public class CUserToken {
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
    
        public void OnReceive(Memory<byte> buffer) {
            messageResolver.OnRawByteReceive(buffer);
        }

        public void OnMessageReceive(byte[] buffer) {
            Peer?.OnMessage(buffer);
        }

        public void OnMessageDecodeFail(Exception ex, Memory<byte> buffer) {
            Console.WriteLine(ex);
            CPacket p = new CPacket();
            p.Push(buffer);
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

        public void OnSendCompleted() {
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
