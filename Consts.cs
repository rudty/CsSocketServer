namespace SocketServer {
    class Consts {
        public const int DEFAULT_ACCEPT_BACKLOG_SIZE = 511;
        public const int HEADER_SIZE = 3;
        public const int MESSAGE_BUFFER_SIZE = 1024;
        public const int ALLOCATE_BUFFER_COUNT = 5;

        public const byte PACKET_BEGIN = 0x8F;
    }
}
