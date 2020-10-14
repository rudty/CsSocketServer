using System;
using System.Collections.Generic;
using System.Text;

namespace SocketServer {
    class CPacket {
        public IPeer Owner { get; private set; }
        public byte[] Buffer { get; private set; }
        public int Position { get; private set; } = Consts.HEADER_SIZE;
        public Int16 ProtocolId { get; private set; }


        public void recordSize() {
            Int16 s = (Int16)(Position - Consts.HEADER_SIZE);
            byte[] header = BitConverter.GetBytes(s);
            header.CopyTo(Buffer, 0);
        }

        public CPacket Clone() {
            return new CPacket {
                Owner = Owner,
                Buffer = (byte[])Buffer.Clone(),
                ProtocolId = ProtocolId
            };
        }
    }
}
