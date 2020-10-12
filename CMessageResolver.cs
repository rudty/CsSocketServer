using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace SocketServer {
    class CMessageResolver {
        const int HEADERSIZE = 2;

        public delegate void CompletedMessageCallback(byte[] buffer);

        int remainBytes = 0;
        int currentPosition = 0;
        int positionToRead = 0;
        int messageSize = 0;

        byte[] messageBuffer = new byte[1024];


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

        public void onReceive(byte[] buffer, int offset, int transffered, CompletedMessageCallback callback) {
            this.remainBytes = transffered;
        
            int srcPosition = offset;

            while (remainBytes > 0) {
                if (currentPosition < HEADERSIZE) {
                    positionToRead = HEADERSIZE;
                    if (false == readUntil(buffer, ref srcPosition, offset, transffered)) { 
                        return;
                    }


                    messageSize = BitConverter.ToInt16(messageBuffer, 0);
                    positionToRead = messageSize + HEADERSIZE;
                }

                if (readUntil(buffer, ref srcPosition, offset, transffered)) {
                    callback(messageBuffer);
                    clearBuffer();
                }
            }
        }

        private void clearBuffer() {
            for (int i = 0; i < HEADERSIZE; ++i) {
                messageBuffer[i] = 0;
            }
            currentPosition = 0;
            this.messageSize = 0;
        }
    }
}
