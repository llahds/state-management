using Newtonsoft.Json.Linq;

public class MatchAllFilter : IFilter
{
    public ValueTask<bool> Evaluate(JObject evt)
    {
        return ValueTask.FromResult(true);
    }
}
