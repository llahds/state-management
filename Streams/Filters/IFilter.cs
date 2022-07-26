using Newtonsoft.Json.Linq;

public interface IFilter
{
    ValueTask<bool> Evaluate(JObject evt);
}
