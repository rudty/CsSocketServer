using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace SocketServer {

    public class MessageTaskRunner {
        int work = 0;
        readonly BlockingCollection<Func<Task>> actions = new BlockingCollection<Func<Task>>();
        Task runTask = null;

        async void DoTask() {
            Func<Task> fn;
            while (actions.TryTake(out fn)) {
                try {
                    await fn();
                } catch(Exception e) {
                    Console.WriteLine(e);
                }
            }
            Interlocked.Exchange(ref work, 0);
        }

        public void Add(Action fn) {
            Add(() => {
                fn();
                return Task.CompletedTask;
            });
        }

        public void Add(Func<Task> fn) {
            actions.Add(fn);
            if (Interlocked.Increment(ref work) == 1) {
                runTask = Task.Run(DoTask);
            }
        }

        /// <summary>
        /// 현재 큐에 들어간 모든 함수가 실행이 끝날때까지 기다립니다
        /// </summary>
        public Task Wait() {
            if (runTask == null) {
                return Task.CompletedTask;
            }
            return runTask;
        }
    }

}
