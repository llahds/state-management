using Irony.Parsing;

namespace Streams.Parsing
{
    public class StatementParser
    {
        private readonly Parser parser;

        public StatementParser()
        {
            this.parser = new Parser(new QLGrammar());
        }

        public void Parse(string commandText)
        {
            var parse = this.parser.Parse(commandText);

            // TODO: return parse errors
            if (parse.HasErrors() == false)
            {
                this.ParseStatements(parse.Root);
            }
        }

        private void ParseStatements(ParseTreeNode node)
        {
            foreach (var child in node.ChildNodes)
            {
                this.ParseStatement(child);
            }
        }

        private void ParseStatement(ParseTreeNode node)
        {
            foreach (var child in node.ChildNodes)
            {
                if (child.Term.Name == "QUERY")
                {
                    this.ParseQuery(child);
                }
            }
        }

        private void ParseQuery(ParseTreeNode node)
        {
            foreach (var child in node.ChildNodes)
            {
                if (child.Term.Name == "FROM")
                {
                    this.ParseFrom(child);
                }
                else if (child.Term.Name == "JOIN_LIST")
                {
                    this.ParseJoinList(child);
                }
                else if (child.Term.Name == "SELECT")
                {
                    this.ParseSelect(child);
                }
                else if (child.Term.Name == "GROUP_BY")
                {
                    this.ParseGroupBy(child);
                }
                else if (child.Term.Name == "INTO")
                {
                    this.ParseInto(child);
                }
            }
        }

        private void ParseFrom(ParseTreeNode node)
        {
            var streamName = node.ChildNodes.FirstOrDefault(F => F.Term.Name == "ID")?.Token.Value;
            this.ParseFilter(node.ChildNodes.FirstOrDefault(F => F.Term.Name == "FILTER"));
            this.ParseWindow(node.ChildNodes.FirstOrDefault(F => F.Term.Name == "WINDOW"));
        }

        private void ParseWindow(ParseTreeNode? node)
        {
            var timespan = node?.ChildNodes.FirstOrDefault(F => F.Term.Name == "TIME_SPAN");

            if (timespan != null)
            {
                this.ParseTimespan(timespan);
            }
        }

        private void ParseTimespan(ParseTreeNode? node)
        {
            var unit = Convert.ToUInt32(node?.ChildNodes[0].Token.Value);
            var uom = node?.ChildNodes[1].ChildNodes[0].Token.Value;
        }

        private void ParseFilter(ParseTreeNode? node)
        {

        }

        private void ParseJoinList(ParseTreeNode node)
        {

        }

        private void ParseSelect(ParseTreeNode node)
        {

        }

        private void ParseGroupBy(ParseTreeNode node)
        {

        }

        private void ParseInto(ParseTreeNode node)
        {

        }
    }
}
