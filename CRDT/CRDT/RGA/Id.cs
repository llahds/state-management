namespace CRDT.RGA
{
    public struct Id : IEquatable<Id>, IComparable<Id>
    {
        public static readonly Id ROOT = new Id { ReplicationId = -1, OperationId = -1 };
        public int ReplicationId { get; set; }
        public int OperationId { get; set; }

        public Task WriteTo(BinaryWriter writer)
        {
            writer.Write(ReplicationId);
            writer.Write(OperationId);

            return Task.CompletedTask;
        }

        public int CompareTo(Id other)
        {
            var r = ReplicationId.CompareTo(other.ReplicationId);

            if (r == 0)
            {
                return OperationId.CompareTo(other.OperationId);
            }

            return r;
        }

        public override string ToString()
        {
            return $"({ReplicationId},{OperationId})";
        }

        public bool Equals(Id other)
        {
            return ReplicationId.Equals(other.ReplicationId)
                && OperationId.Equals(other.OperationId);
        }
    }
}
