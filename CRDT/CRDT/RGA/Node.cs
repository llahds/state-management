using Newtonsoft.Json;

namespace CRDT.RGA
{
    public class Node<T> where T : IComparable<T>, IEquatable<T>
    {
        public Id Id { get; set; }
        public T Value { get; set; }
        public bool IsDeleted { get; set; }
        public SortedSet<Id> Children { get; set; }

        public async Task WriteTo(BinaryWriter writer)
        {
            await Id.WriteTo(writer);
            writer.Write(JsonConvert.SerializeObject(Value));
            writer.Write(IsDeleted);
            writer.Write(Children.Count);
            foreach (var child in Children)
            {
                await child.WriteTo(writer);
            }
        }

        public override string ToString()
        {
            return $"{Value?.ToString() ?? ""} {Id}";
        }
    }
}