using Irony.Parsing;

namespace Streams.Parsing
{
    public class QLGrammar : Grammar
    {
        public QLGrammar()
            : base(false)
        {
            var id = new IdentifierTerminal("ID");
            var comma = ToTerm(",");
            var semicolon = ToTerm(";");
            var statementDelimiter = semicolon | Empty;
            var field = new NonTerminal("FIELD");
            var streamName = new NonTerminal("STREAM_NAME");
            var fieldName = new NonTerminal("FIELD_NAME");
            var boolean = ToTerm("AND") | "OR";
            var stringValue = new StringLiteral("STRING", "'", StringOptions.AllowsDoubledQuote);
            var numberValue = new NumberLiteral("NUMBER");

            streamName.Rule = id;
            fieldName.Rule = id;
            field.Rule = streamName + "." + fieldName;

            var statement = new NonTerminal("STATEMENT");
            var createEndpoint = new NonTerminal("CREATE_ENDPOINT");
            var query = new NonTerminal("QUERY");

            statement.Rule = createEndpoint | query;

            var statements = new NonTerminal("STATEMENTS");

            statements.Rule = this.MakePlusRule(statements, statement) | Empty;

            this.Root = statements;

            // create endpoint - parses "create endpoint foo (test1 number, test2 string);"
            var create = ToTerm("CREATE");
            var stream = ToTerm("ENDPOINT");
            var fieldDef = new NonTerminal("FIELD_DEF");
            var fieldTypeName = new NonTerminal("TYPE_NAME");
            var fieldDefList = new NonTerminal("FIELD_DEF_LIST");

            fieldDef.Rule = id + fieldTypeName;
            fieldTypeName.Rule = ToTerm("NUMBER") | "DATE" | "STRING";
            fieldDefList.Rule = this.MakePlusRule(fieldDefList, comma, fieldDef);
            createEndpoint.Rule = create + stream + id + "(" + fieldDefList + ")" + statementDelimiter;

            // from clause - parses "from foo join foo2 on foo.id = foo2.id join foo3 on foo.id = foo3.id"
            var from = new NonTerminal("FROM");
            var join = new NonTerminal("JOIN");
            var joinList = new NonTerminal("JOIN_LIST");
            var joinOn = new NonTerminal("JOIN_ON");
            var joinOnList = new NonTerminal("JOIN_ON_LIST");
            var filter = new NonTerminal("FILTER");
            var filterExpression = new NonTerminal("FILTER_EXPRESSION");
            var filterExpressionList = new NonTerminal("FILTER_EXPRESSION_LIST");
            var filterOperator = ToTerm("=") | ">" | "<";
            var filterValue = stringValue | numberValue;
            var window = new NonTerminal("WINDOW");
            var timespan = new NonTerminal("TIME_SPAN");
            var timespanUnit = new NonTerminal("TIME_SPAN_UNIT");

            timespanUnit.Rule = ToTerm("days") | "hours" | "minutes" | "seconds" | "milliseconds";
            timespan.Rule = numberValue + timespanUnit;
            window.Rule = ToTerm("keep last") + timespan;

            filterExpression.Rule = id + filterOperator + filterValue;
            filterExpressionList.Rule = this.MakePlusRule(filterExpressionList, boolean, filterExpression);
            filter.Rule = ToTerm("[") + filterExpressionList + "]" | Empty;

            joinOn.Rule = field + "=" + field;
            joinOnList.Rule = this.MakePlusRule(joinOnList, boolean, joinOn);
            from.Rule = ToTerm("from") + id + filter + window;
            join.Rule = ToTerm("join") + id + filter + window + "on" + joinOnList;
            joinList.Rule = MakeStarRule(joinList, join) | Empty;

            // select clause - parses "{from clause} select foo.id, foo.field2"
            var select = new NonTerminal("SELECT");
            var selectFields = new NonTerminal("SELECT_FIELDS");
            selectFields.Rule = this.MakePlusRule(selectFields, comma, field);
            select.Rule = ToTerm("SELECT") + selectFields;



            // into clause - parses "{from clause} {select clause} into foo (field1, field2, field3)"
            var into = new NonTerminal("INTO");
            var intoFieldList = new NonTerminal("INTO_FIELD_LIST");
            intoFieldList.Rule = this.MakePlusRule(intoFieldList, comma, id);
            into.Rule = ToTerm("INTO") + id + "(" + intoFieldList + ")";

            query.Rule = from + joinList + select + into + statementDelimiter;
        }
    }
}
