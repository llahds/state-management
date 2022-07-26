using Newtonsoft.Json.Linq;

public class Event
{
    public long Id { get; set; }
    public JObject Payload { get; set; }
    public DateTime ExpiresOn { get; set; }
}
