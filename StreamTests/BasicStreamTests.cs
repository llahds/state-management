using Newtonsoft.Json.Linq;
using Streams.Events;
using Streams.Events.Filters;
using Streams.Events.Handlers;
using Streams.Events.Storage;

namespace StreamTests
{
    [TestClass]
    public class BasicStreamTests
    {
        [TestMethod]
        public async Task GroupBy_With_Having_Count_Emits_Expected_Values()
        {
            // select
            //   customerId,
            //   count(*) orderCount
            // from [orders]#time.window(3 hours)
            // group by customerId
            // having count(*) > 3

            var orders = new EventStream(
                store: new TimeWindow(new TimeSpan(0, 3, 0, 0)),
                filter: new MatchAllFilter()
            );

            var groupBy = await orders.GroupBy(E => E["customerId"].Value<int>(), new PredicateFilter(J => J["events"].Count() > 3));

            var select = await groupBy.Select(J => JObject.FromObject(new { customerId = J["key"], orderCount = J["events"].Count() }));

            Event evt = null;
            await select.Subscribe(new Log(E => evt = E));

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

            Assert.IsNotNull(evt);
            Assert.IsTrue(evt.Payload["customerId"].Value<int>() == 2 && evt.Payload["orderCount"].Value<int>() == 4);

            await orders.Emit(JObject.FromObject(new { customerId = 3, order = 11 }));

            Assert.IsTrue(evt.Payload["customerId"].Value<int>() == 3 && evt.Payload["orderCount"].Value<int>() == 4);
        }

        [TestMethod]
        public async Task GroupBy_With_Having_Count_With_Delayed_Emit_Emits_Expected_Value()
        {
            // select
            //   customerId,
            //   count(*) orderCount
            // from [orders]#time.window(3 hours)
            // group by customerId
            // having count(*) > 3

            var orders = new EventStream(
                store: new TimeWindow(new TimeSpan(0, 0, 0, 0, 100)),
                filter: new MatchAllFilter()
            );

            var groupBy = await orders.GroupBy(E => E["customerId"].Value<int>(), new PredicateFilter(J => J["events"].Count() > 3));

            var select = await groupBy.Select(J => JObject.FromObject(new { customerId = J["key"], orderCount = J["events"].Count() }));

            Event evt = null;
            await select.Subscribe(new Log(E => evt = E));

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

            Assert.IsNotNull(evt);
            Assert.IsTrue(evt.Payload["customerId"].Value<int>() == 2 && evt.Payload["orderCount"].Value<int>() == 4);

            Task.Delay(101).Wait();

            await orders.Emit(JObject.FromObject(new { customerId = 3, order = 11 }));

            Assert.IsTrue(evt.Payload["customerId"].Value<int>() == 2);
        }
    }
}