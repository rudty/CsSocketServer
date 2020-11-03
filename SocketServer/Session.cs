using System;
using System.Collections.Generic;
using System.Net.Sockets;
using SocketServer.Net.IO;
using SocketServer.Net;

namespace SocketServer {

    public class Session {
        
        internal Socket Socket { get; set; }

        internal NetworkService NetworkService { get; set; }

        public readonly string SessionID;

        bool online = true;

        Queue<CPacket> sendingQueue = new Queue<CPacket>();
        InputMessageResolver messageResolver = new InputMessageResolver();

        public ISessionHandler SessionHandler { private get; set; }

        public Session() {
            SessionID = Guid.NewGuid().ToString();
            messageResolver.OnMessageReceive += OnMessageReceive;
            messageResolver.OnMessageDecodeFail += OnMessageDecodeFail;
        }
    
        public void OnReceive(Memory<byte> buffer) {
            messageResolver.OnRawByteReceive(buffer);
        }

        public void OnMessageReceive(byte[] buffer) {
            SessionHandler?.OnMessage(this, buffer);
        }

        public void OnMessageDecodeFail(Exception ex, Memory<byte> buffer) {
            Console.WriteLine(ex);
            CPacket p = new CPacket();
            p.Push(buffer);
            Send(p);
        }

        public void OnRemoved() {
            SessionHandler?.OnDisconnected(this);

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
