namespace CRDT.RGA
{
    public class OperationLog<T>
    {
        private int operationId = 0;
        private readonly List<Item<T>> operations;
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
        private readonly int replicationId;

        public OperationLog(int replicationId)
        {
            operations = new List<Item<T>>();
            this.replicationId = replicationId;
        }

        public Task<Id> Add(T value)
        {
            return Add(Id.ROOT, value);
        }

        public Task Add(T[] values)
        {
            return Add(Id.ROOT, values);
        }

        public async Task Add(Id parentId, T[] values)
        {
            var currentId = await Add(parentId, values.First());

            foreach (var value in values.Skip(1))
            {
                currentId = await Add(currentId, value);
            }
        }

        public async Task<Id> Add(Id parentId, T value)
        {
            var o = Interlocked.Increment(ref operationId);

            await semaphore.WaitAsync();

            try
            {
                var id = new Id { ReplicationId = replicationId, OperationId = o };

                operations.Add(new Item<T>
                {
                    Parent = parentId,
                    Id = id,
                    Operation = OperationType.Insert,
                    Value = value,
                });

                return id;
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task Clear()
        {
            await semaphore.WaitAsync();

            try
            {
                operations.Clear();
            }
            finally
            {
                semaphore.Release();
            }
        }

        public IEnumerable<Item<T>> Snapshot()
        {
            return operations.ToArray();
        }

        public async Task Remove(Id id)
        {
            var o = Interlocked.Increment(ref operationId);

            await semaphore.WaitAsync();

            try
            {
                operations.Add(new Item<T>
                {
                    Parent = id,
                    Id = new Id { ReplicationId = replicationId, OperationId = o },
                    Operation = OperationType.Remove,
                });
            }
            finally
            {
                semaphore.Release();
            }
        }
    }

}