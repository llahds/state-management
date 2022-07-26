using CRDT.RGA;

var list0 = new ReplicatedList<char>();

// the current state for client 1
var list1 = new ReplicatedList<char>();

// current state for client 2
var list2 = new ReplicatedList<char>();

// the list of state changes for client 0
var client0 = new OperationLog<char>(0);

// the list of state changes for client 1
var client1 = new OperationLog<char>(1);

// change list for client 2
var client2 = new OperationLog<char>(2);

// client 0 adds the word "hello!"
await client0.Add("hello!".ToCharArray());
await list0.Apply(client0);
var b0_0 = await list0.ToArray(); // client 0 = hello!

// client 1 adds the word "hi!"
await client1.Add("hi!".ToCharArray());
await list1.Apply(client1);
var b1_0 = await list1.ToArray(); // client 1 = hi!

// both lists are sync'd with both sets of changes
await list0.Apply(client1);
await list1.Apply(client0);
var b0_1 = await list0.ToArray(); // --> list0 = hi!hello!
var b1_1 = await list1.ToArray(); // --> list1 = hi!hello!

// client 0 adds " bob" after "hello!"
await client0.Add(b0_1[8].Id, " bob".ToCharArray());
await list0.Apply(client0);
var b0_2 = await list0.ToArray(); // --> list0 = hi!hello bob!

// client 1 adds " sam" after "hello!"
await client1.Add(b0_1[8].Id, " sam".ToCharArray());
await list1.Apply(client1);
var b1_2 = await list1.ToArray(); // --> list1 = hi!hello sam!

// apply both sets of changes to both lists
await list0.Apply(client1);
await list1.Apply(client0);
var b0_3 = await list0.ToArray(); // --> list0: hi!hello sam bob!
var b1_3 = await list1.ToArray(); // --> list1: hi!hello sam bob!

// client 0 removes " sam"
await client0.Remove(b1_3[9].Id);
await client0.Remove(b1_3[10].Id);
await client0.Remove(b1_3[11].Id);
await client0.Remove(b1_3[12].Id);
await list0.Apply(client0);
var b0_4 = await list0.ToArray(); // --> list0: hi!hello bob!

// client 1 hasn't been sync'd with 
// the changes from client 0 yet
var b1_4 = await list1.ToArray(); // --> list1: hi!hello sam bob!

// client 1 adds " dave" after " sam"
await client1.Add(b1_3[12].Id, " dave".ToCharArray());
await list1.Apply(client1);
var b1_5 = await list1.ToArray(); // --> list1: hi!hello sam dave bob!

// sync all changes between both lists
await list0.Apply(client1);
await list0.Apply(client0);
await list1.Apply(client1);
await list1.Apply(client0);

var b0_6 = await list0.ToArray(); // ->> list0 = hi!hello dave bob!
var b1_6 = await list1.ToArray(); // ->> list1 = hi!hello dave bob!

await list2.Apply(client1);
var b2_0 = await list2.ToArray();

await list2.Apply(client0);
var b2_1 = await list2.ToArray();

await client0.Remove(b2_1.Last().Id);
await client0.Add(b2_1.Last().Id, ".".ToCharArray());
await list0.Apply(client0);
var b0_7 = await list0.ToArray();

await client1.Remove(b2_1.Last().Id);
await client1.Add(b2_1.Last().Id, "@".ToCharArray());
await list1.Apply(client1);
var b1_7 = await list1.ToArray();

await client2.Remove(b2_1.Last().Id);
await client2.Add(b2_1.Last().Id, "?".ToCharArray());
await list2.Apply(client2);
var b2_2 = await list2.ToArray();

await list0.Apply(client1);
var b0_8 = await list0.ToArray();

await list1.Apply(client0);
var b1_8 = await list1.ToArray();

await list0.Apply(client2);
await list1.Apply(client2);
var b0_9 = await list0.ToArray();
var b1_9 = await list1.ToArray();

await list2.Apply(client0);
var b2_3 = await list2.ToArray();
await list2.Apply(client1);
var b2_4 = await list2.ToArray();

var list3 = new ReplicatedList<char>();
await list3.Apply(client2);
await list3.Apply(client0);
await list3.Apply(client1);
var b3_0 = await list3.ToArray();

Console.WriteLine(new string(b0_9.Select(T => T.Value).ToArray()));
Console.WriteLine(new string(b1_9.Select(T => T.Value).ToArray()));
Console.WriteLine(new string(b2_4.Select(T => T.Value).ToArray()));
Console.WriteLine(new string(b3_0.Select(T => T.Value).ToArray()));

Console.WriteLine();

// big file
var list = new ReplicatedList<char>();
var log = new OperationLog<char>(0);
await log.Add(File.ReadAllText("./test_01.txt").ToCharArray());
await list.Apply(log);
var s0 = await list.ToArray();

Console.WriteLine();

