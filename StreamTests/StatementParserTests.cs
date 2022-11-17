using Streams.Parsing;

namespace StreamTests
{
    [TestClass]
    public class StatementParserTests
    {
        [TestMethod]
        public void _()
        {
            var statement = @"from foo keep last 2 minutes select foo.id group by foo.id, foo.field2 into foo2 (id); ";
            var parser = new StatementParser();
            parser.Parse(statement);
        }
    }
}
