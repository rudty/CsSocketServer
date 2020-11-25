using System;
using System.Collections.Generic;
using System.Text;

namespace SocketServer {
    public class AutoServerBootHelper {
        public string Host { get; private set; }
        public int Port { get; private set; }

        internal Server server = new Server();

        public AutoServerBootHelper() {
            // TODO 자동으로 환경변수 => 파일 => 실행인자를 읽고
            // host, port 값을 덮어씌울 것.

            var host = GetEnvOrElse("HOST", "0.0.0.0");
            var port = GetEnvOrElse("PORT", "8080");
            int numPort;
            if (false == int.TryParse(port, out numPort)) {
                throw new ArgumentException($"port parse error {port}");
            }
            server.ListenAndServe(host, numPort);
        }

        /// <summary>
        /// 환경변수를 가져옵니다. 만약 환경변수가 없다면
        /// orElse를 실행해서 가져옵니다
        /// </summary>
        /// <param name="varName">가져올 환경변수 이름</param>
        /// <param name="orElse">없다면 수행할 함수</param>
        /// <returns>환경변수</returns>
        private static string GetEnvOrElse(string varName, Func<string> orElse) {
            var e = Environment.GetEnvironmentVariable(varName);
            if (e != null) {
                return e;
            }

            e = orElse();
            if (e != null) {
                return e;
            }
            throw new NullReferenceException("cannot found env");
        }

        /// <summary>
        /// 환경변수를 가져옵니다. 만약 환경변수가 없다면 
        /// orElse를 실행해서 가져옵니다
        /// </summary>
        /// <param name="varName">가져올 환경변수 이름</param>
        /// <param name="orElse">없다면 수행할 함수</param>
        /// <returns>환경변수</returns>
        private static string GetEnvOrElse(string varName, string orElse = "") {
            return GetEnvOrElse(varName, () => orElse);
        }
    }
}
