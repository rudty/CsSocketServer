using System;
using System.Collections.Generic;
using System.Text;

namespace SocketServer.Net {

    /// <summary>
    /// 일일히 함수를 걸기에는 번거로우므로 
    /// 해당 Attribute 를 등록해서도 처리할 수 있게 함
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class MessageHandler: Attribute {
        public readonly string Message;

        public MessageHandler(string message) {
            Message = message;
        }

        public MessageHandler(): this(null) {
        }
    }
}

