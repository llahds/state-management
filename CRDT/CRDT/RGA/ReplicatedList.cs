namespace CRDT.RGA
{
    public class ReplicatedList<T> where T : IComparable<T>, IEquatable<T>
    {
        private readonly Dictionary<Id, Node<T>> nodes = new Dictionary<Id, Node<T>>(1_000_000);
        private readonly Dictionary<int, int> clock = new Dictionary<int, int>();
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);

        public ReplicatedList()
        {
            nodes.Add(Id.ROOT, new Node<T> { Id = Id.ROOT, Children = new SortedSet<Id>() });
        }

        public async Task WriteTo(BinaryWriter writer)
        {
            writer.Write(nodes.Count);
            foreach (var node in nodes)
            {
                await node.Value.WriteTo(writer);
            }
            writer.Write(clock.Count);
            foreach (var r in clock)
            {
                writer.Write(r.Key);
                writer.Write(r.Value);
            }
        }

        public async Task<Node<T>[]> ToArray()
        {
            await semaphore.WaitAsync();

            try
            {
                var buffer = new List<Node<T>>();

                var stack = new Stack<Id>();

                stack.Push(Id.ROOT);

                while (stack.Count > 0)
                {
                    var currentId = stack.Pop();

                    var node = nodes[currentId];

                    if (node.IsDeleted == false)
                    {
                        buffer.Add(node);
                    }

                    foreach (var childId in node.Children)
                    {
                        stack.Push(childId);
                    }
                }

                return buffer.ToArray();
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task Apply(OperationLog<T> log)
        {
            await semaphore.WaitAsync();

            try
            {
                foreach (var operation in log.Snapshot())
                {
                    if (clock.ContainsKey(operation.Id.ReplicationId) == false)
                    {
                        clock.Add(operation.Id.ReplicationId, 0);
                    }

                    var lastOperationId = clock[operation.Id.ReplicationId];

                    if (operation.Id.OperationId > lastOperationId)
                    {
                        clock[operation.Id.ReplicationId] = operation.Id.OperationId;

                        if (operation.Operation == OperationType.Insert)
                        {
                            HandleInsert(operation);
                        }
                        else if (operation.Operation == OperationType.Remove)
                        {
                            HandleRemove(operation);
                        }
                    }
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        private void HandleRemove(Item<T> operation)
        {
            if (nodes.ContainsKey(operation.Parent) == false)
            {
                nodes.Add(operation.Parent, new Node<T> { Id = operation.Id, Value = operation.Value, Children = new SortedSet<Id>() });
            }

            nodes[operation.Parent].IsDeleted = true;
        }

        private void HandleInsert(Item<T> operation)
        {
            Node<T> node = default;
            Node<T> parent = default;

            if (nodes.ContainsKey(operation.Id) == false)
            {
                node = new Node<T> { Id = operation.Id, Value = operation.Value, Children = new SortedSet<Id>() };
                nodes.Add(operation.Id, node);
            }
            else
            {
                node = nodes[operation.Id];
            }

            // this handles a special case where changes for multiple replications
            // have been applied in different order. parent nodes needs to 
            // added so that changes can be recorded but the nodes are created
            // as orphans. when the changeset that created the parent node is applied
            // it will create the hierarchy correctly. 
            if (nodes.ContainsKey(operation.Parent) == false)
            {
                parent = new Node<T> { Id = operation.Parent, Value = operation.Value, Children = new SortedSet<Id>() };
                nodes.Add(operation.Parent, parent);
            }
            else
            {
                parent = nodes[operation.Parent];
            }

            // this is a special case to handle setting the value from the above if 
            // block that adds the operation.Parent. the nodes need to be created
            // but will be orphaned (and an incorrect value set) until the log
            // that created them can set the value.
            if (node.Value.Equals(operation.Value) == false)
            {
                node.Value = operation.Value;
            }

            parent.Children.Add(operation.Id);
        }
    }
}