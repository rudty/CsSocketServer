using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace SocketServer {
    
    class CMessageResolver {
        public delegate void CompletedMessageCallback(byte[] buffer);
        public event CompletedMessageCallback OnMessageReceive;
        public event CompletedMessageCallback OnDecodeFail;

        int remainBytes = 0;
        int currentPosition = 0;
        int positionToRead = 0;
        int messageSize = 0;

        byte[] messageBuffer = new byte[Consts.MESSAGE_BUFFER_SIZE];

        bool readUntil(byte[] buffer, ref int srcPosition, int offset, int transffered) {
            if (currentPosition >= offset + transffered) {
                return false;
            }

            int copySize = positionToRead - currentPosition;

            Array.Copy(
                buffer,
                srcPosition,
                messageBuffer,
                currentPosition,
                copySize);


            srcPosition += copySize;
            currentPosition += copySize;
            remainBytes -= copySize;

            if (currentPosition < positionToRead) {
                return false;
            }
            return true;
        }

        void DecodeHeader(byte[] buffer, int offset, int transffered) {
            const int minPacketSize = Consts.HEADER_SIZE + 1;
            if (transffered < minPacketSize) {
                throw new Exception($"packet size must > {transffered}");
            }

            if (buffer[offset] != Consts.PACKET_BEGIN) {
                throw new Exception($"packet header[0] error {buffer[0]}");
            }

            int len = 0;
            len += buffer[offset + 1];
            len += buffer[offset + 2] << 8;

            if (len < 0) {
                throw new Exception($"packet length error {len}");
            }

            this.messageSize = len;
            this.currentPosition = Consts.HEADER_SIZE;
        }

        public void OnReceive(byte[] buffer, int offset, int transffered) {
            //remainBytes = transffered;

            //int srcPosition = offset;

            if (currentPosition < Consts.HEADER_SIZE) {
                DecodeHeader(buffer, offset, transffered);
            }

            //while (remainBytes > 0) {
            //    if (currentPosition < Consts.HEADER_SIZE) {
            //        positionToRead = Consts.HEADER_SIZE;
            //        if (false == readUntil(buffer, ref srcPosition, offset, transffered)) {
            //            return;
            //        }


            //        messageSize = BitConverter.ToInt16(messageBuffer, 0);
            //        positionToRead = messageSize + Consts.HEADER_SIZE;
            //    }

            //    if (readUntil(buffer, ref srcPosition, offset, transffered)) {
            //        callback(messageBuffer);
            //        clearBuffer();
            //    }
            //}
            if (transffered > 0) {
                byte[] b = new byte[transffered];
                Array.Copy(buffer, offset, b, 0, transffered);
                OnDecodeFail(b);
            }
        }

        private void clearBuffer() {
            for (int i = 0; i < Consts.HEADER_SIZE; ++i) {
                messageBuffer[i] = 0;
            }
            currentPosition = 0;
            this.messageSize = 0;
        }
    }
}
