namespace CRDT.RGA
{
    public class Item<T>
    {
        public Id Id { get; set; }
        public Id Parent { get; set; }
        public OperationType Operation { get; set; }
        public T Value { get; set; }
    }
}