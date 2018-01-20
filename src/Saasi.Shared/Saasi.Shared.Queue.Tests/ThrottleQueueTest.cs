using System;
using Xunit;
using Saasi.Shared.Queue;
using System.Threading;
using System.Threading.Tasks;

namespace Saasi.Shared.Queue.Tests
{
    public class ThrottleQueueTest
    {
        [Fact]
        public async void Test1()
        {
            IThrottleQueue tq = new ThrottleQueue(1);
            var t1 = await tq.QueueUp();
            Assert.Equal(0, tq.ItemsWaiting);
            Assert.Equal(1, tq.ItemsRunning);
            Console.WriteLine(t1);
            Task.Factory.StartNew(() => {
                Thread.Sleep(1000);
                tq.Finish(t1);
                Assert.Equal(tq.ItemsWaiting, 0);
                Thread.Sleep(2000);
            });
            var t2 = await tq.QueueUp();
            Console.WriteLine(t2);
            Assert.Equal(1, tq.ItemsRunning);
            tq.Finish(t2);
            Assert.Equal(0, tq.ItemsRunning);
            Assert.NotEqual(t1.ToString(), t2.ToString());
        }
    }
}
