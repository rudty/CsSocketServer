using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace SocketServer {
    
    class CMessageResolver {
        public delegate void CompletedMessageCallback(byte[] buffer);
        public delegate void MessageDecodeFailCallback(Exception ex, byte[] buffer);

        public event CompletedMessageCallback OnMessageReceive;
        public event MessageDecodeFailCallback OnMessageDecodeFail;

        int currentPosition = 0;
        int messageSize = 0;
        readonly byte[] messageBuffer = new byte[Consts.MESSAGE_BUFFER_SIZE];

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

            if (len > messageBuffer.Length) {
                throw new Exception($"message size({len}) > messageBuffer size({messageBuffer.Length})");
            }

            this.messageSize = len;
        }

        public void OnRawByteReceive(byte[] buffer, int offset, int transffered) {
            try {
                if (currentPosition < Consts.HEADER_SIZE) {
                    DecodeHeader(buffer, offset, transffered);
                    currentPosition = Consts.HEADER_SIZE;
                    offset += Consts.HEADER_SIZE;
                    transffered -= Consts.HEADER_SIZE;

                }
                int copySize = transffered;
                int maxRemainByte = messageBuffer.Length - currentPosition;

                if (currentPosition + transffered > maxRemainByte) {
                    copySize = maxRemainByte;
                }

                Array.Copy(
                    buffer,
                    offset,
                    messageBuffer,
                    currentPosition,
                    copySize);

                currentPosition += copySize;

                if (currentPosition >= messageSize) {
                    OnMessageReceive(messageBuffer);
                    ClearBuffer();
                }

            } catch (Exception e) {

                if (transffered > 0) {
                    byte[] b = new byte[transffered];
                    Array.Copy(buffer, offset, b, 0, transffered);
                    OnMessageDecodeFail(e, b);
                }
            }
        }

        private void ClearBuffer() {
            for (int i = 0; i < Consts.HEADER_SIZE; ++i) {
                messageBuffer[i] = 0;
            }
            currentPosition = 0;
            messageSize = 0;
        }
    }
}
