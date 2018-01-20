using System;
using System.Threading;
using System.Collections;
using System.Threading.Tasks;

namespace Saasi.Shared.Queue
{
    public class ThrottleQueue : IThrottleQueue
    {
        private System.Collections.Generic.Queue<TaskCompletionSource<Guid>> _queue = new System.Collections.Generic.Queue<TaskCompletionSource<Guid>>();
        private System.Collections.Generic.HashSet<Guid> _execPool = new System.Collections.Generic.HashSet<Guid>();
        private object _locker = new object();

        private int Limit;

        public int ItemsWaiting { get {return _queue.Count;}}
        public int ItemsRunning { get {return _execPool.Count;}}
        
        public ThrottleQueue(int limit = 100)
        {
            this.Limit = limit;
        }

        private Guid PutInExecutionPool() {
            var guid = Guid.NewGuid();
            lock (_locker) {
                _execPool.Add(guid);
            }
            return guid;
        }
        private void RemoveFromExecutionPool(Guid guid) {
            lock (_locker){
                _execPool.Remove(guid);
            }
        }
        
        public void Finish(Guid guid)
        {
            RemoveFromExecutionPool(guid);
            lock(_locker) {
                while (_execPool.Count < Limit && _queue.Count > 0) {
                    var taskSource = _queue.Dequeue();
                    taskSource.SetResult(PutInExecutionPool());
                }
            }
        }

        Task<Guid> IThrottleQueue.QueueUp()
        {
            var taskSource = new TaskCompletionSource<Guid>();
            lock (_locker){
                if (_execPool.Count < Limit) {
                    taskSource.SetResult(PutInExecutionPool());
                }
                else {
                    _queue.Enqueue(taskSource);
                }
            }
            return taskSource.Task;
        }
    }
    public interface IThrottleQueue {
        Task<Guid> QueueUp();
        void Finish(Guid guid);

        int ItemsRunning {get;}
        int ItemsWaiting {get;}
    }
}
