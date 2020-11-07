using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace SocketServer {

    class MessageTaskRunner {
        bool work = false;
        readonly BlockingCollection<Func<Task>> actions = new BlockingCollection<Func<Task>>();

        async void DoTask() {
            Func<Task> fn;
            while (actions.TryTake(out fn)) {
                try {
                    await fn();
                } catch(Exception e) {
                    Console.WriteLine(e);
                }
            }        
            lock(this) {
                work = false;
            }
        }

        public void Add(Action fn) {
            Add(() => {
                fn();
                return Task.CompletedTask;
            });
        }

        public void Add(Func<Task> fn) {
            actions.Add(fn);
            lock (this) {
                if (work == false) {
                    work = true;
                    Task.Run(DoTask);
                }
            }
        }
    }

}
