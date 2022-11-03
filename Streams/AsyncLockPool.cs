
namespace Streams
{
    public class AsyncLockPool : IDisposable
    {
        private readonly SemaphoreSlim[] locks;

        public AsyncLockPool(int poolSize)
        {
            this.locks = Enumerable.Range(0, poolSize)
                .Select(L => new SemaphoreSlim(1))
                .ToArray();
        }

        public Task Wait(object key)
        {
            var offset = key.GetHashCode() % this.locks.Length;

            return this.locks[offset].WaitAsync();
        }

        public Task Release(object key)
        {
            var offset = key.GetHashCode() % this.locks.Length;

            this.locks[offset].Release();

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            foreach (var lck in this.locks)
            {
                lck.Dispose();
            }
        }
    }
}