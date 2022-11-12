using Irony.Parsing;
using Streams.Parsing;

namespace StreamTests
{
    [TestClass]
    public class GrammarTests
    {
        [TestMethod]
        public void Empty_String_Returns_No_Errors()
        {
            var statement = "";
            var parser = new Parser(new QLGrammar());
            var parse = parser.Parse(statement);
            Assert.IsFalse(parse.HasErrors());
        }

        [TestMethod]
        public void Correctly_Formed_Create_Endpoint_Statement_Returns_No_Errors()
        {
            var statement = "create endpoint foo (test number, test2 date, test3 string)";
            var parser = new Parser(new QLGrammar());
            var parse = parser.Parse(statement);
            Assert.IsFalse(parse.HasErrors());
        }

        [TestMethod]
        public void Malformed_Create_Endpoint_Statement_Returns_Parse_Error()
        {
            var statement = "create endpoint foo (test number, test2 date, test3 string";
            var parser = new Parser(new QLGrammar());
            var parse = parser.Parse(statement);
            Assert.IsTrue(parse.HasErrors());
        }

        [TestMethod]
        public void Multiple_Statements_Returns_No_Errors()
        {
            var statement = "create endpoint foo (test number, test2 date, test3 string); create endpoint foo1 (bob number, bob1 date, bob2 string);";
            var parser = new Parser(new QLGrammar());
            var parse = parser.Parse(statement);
            Assert.IsFalse(parse.HasErrors());
        }

        [TestMethod]
        public void Query_With_One_Join_Parses_Correctly()
        {
            var statement = "from foo [foo = 1 and foo2 = '3'] keep last 1 days select foo.id into foo2 (id)";
            var parser = new Parser(new QLGrammar());
            var parse = parser.Parse(statement);
            Assert.IsFalse(parse.HasErrors());
        }

        [TestMethod]
        public void Query_With_Multiple_Joins_Parses_Correctly()
        {
            var statement = "from foo keep last 1 hours join foo2 keep last 1 days on foo.id = foo2.id join foo3 keep last 25.2 milliseconds on foo2.id = foo3.id and foo2.id2 = foo3.id2 select foo.id, foo.id2 into foo (field1, field2)";
            var parser = new Parser(new QLGrammar());
            var parse = parser.Parse(statement);
            Assert.IsFalse(parse.HasErrors());
        }

        [TestMethod]
        public void Query_With_Multiple_Statements_Parses_Correctly()
        {
            var statement = @"from foo keep last 2 minutes select foo.id into foo2 (id); 
from foo keep last 2 minutes join foo2 keep last 2 hours on foo.id = foo2.id join foo3 keep last 2 seconds on foo2.id = foo3.id and foo2.id2 = foo3.id2 select foo.id, foo.id2 into foo (field1, field2);";
            var parser = new Parser(new QLGrammar());
            var parse = parser.Parse(statement);
            Assert.IsFalse(parse.HasErrors());
        }
    }
}
