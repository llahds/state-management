using Newtonsoft.Json.Linq;

// select
//   customerId,
//   count(*) orderCount
// from [orders]#time.window(3 hours)
// group by customerId
// having count(*) > 3

var orders = new EventStream(
    store: new TimeWindow(new TimeSpan(3, 0, 0)),
    filter: new MatchAllFilter()
);

var groupBy = await orders.GroupBy(E => E["customerId"].Value<int>(), new MatchAllFilter());

await groupBy.Subscribe(new Log());

await orders.Emit(JObject.FromObject(new { customerId = 1, order = 1 }));
await orders.Emit(JObject.FromObject(new { customerId = 2, order = 2 }));
await orders.Emit(JObject.FromObject(new { customerId = 3, order = 3 }));

await orders.Emit(JObject.FromObject(new { customerId = 1, order = 4 }));
await orders.Emit(JObject.FromObject(new { customerId = 2, order = 5 }));
await orders.Emit(JObject.FromObject(new { customerId = 3, order = 6 }));

await orders.Emit(JObject.FromObject(new { customerId = 1, order = 7 }));
await orders.Emit(JObject.FromObject(new { customerId = 2, order = 8 }));
await orders.Emit(JObject.FromObject(new { customerId = 3, order = 9 }));

await orders.Emit(JObject.FromObject(new { customerId = 2, order = 10 }));
await orders.Emit(JObject.FromObject(new { customerId = 3, order = 11 }));

Console.WriteLine("FIN");
