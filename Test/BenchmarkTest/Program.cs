﻿using Business;
using Business.Attributes;
using Business.Result;
using Business.Utils;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using static BenchmarkTest.BusinessMember;

[assembly: Logger(LoggerType.All)]
namespace BenchmarkTest
{
    class Program
    {
        static BusinessMember Member;
        static CommandGroup Cmd;
        static Configer Cfg;
        public static int WorkTime = 1200;

        public struct Dto
        {
            [CheckNull]
            public string A { get; set; }
        }

        static void Main(string[] args)
        {
            Member = Bind.Create<BusinessMember>().UseType(typeof(Business.Auth.IToken));
            Cmd = Member.Command;
            Cfg = Member.Configer;

#if DEBUG
            var count = 1000;

#else
            var count = 100000;
#endif
            System.Console.WriteLine($"RUN Count = {count} X 3 WorkTime = {WorkTime}");

            var results = new ConcurrentBag<int>();
            //var tasks = new ConcurrentBag<Task>();
            var watch = new System.Diagnostics.Stopwatch();

            //~preheat
            var t = Cmd.AsyncCall("Test001", new object[] { new Arg00 { A = 123 } }, new UseEntry[] { new UseEntry("sss", "use01"), new UseEntry(new Business.Auth.Token { Key = "a", Remote = "b" }) });
            t.Wait();
            //~end

            //===========================================================//

            //System.Threading.ThreadPool.SetMinThreads(800, 300);

            System.Threading.ThreadPool.GetMinThreads(out int workerThreads, out int completionPortThreads);
            System.Threading.ThreadPool.GetMaxThreads(out int workerThreads2, out int completionPortThreads2);
            System.Console.WriteLine($"Min {workerThreads}, {completionPortThreads} Max {workerThreads2}, {completionPortThreads2}");

            watch.Start();

            Parallel.For(0, count, async c =>
            {
                var result = Cmd.Call("Test000", new object[] { new Arg00 { A = c } }, null, new UseEntry("abc", "use01"), new UseEntry(new Business.Auth.Token { Key = "a", Remote = "b" }));

                results.Add(result.Data);

                //var task = Cmd.AsyncCall("Test001", new object[] { new Arg00 { A = c } }, null, new UseEntry("abc", "use01"), new UseEntry(new Business.Auth.Token { Key = "a", Remote = "b" })).ContinueWith(c2 =>
                //{
                //    results.Add(c2.Result.Data);
                //});

                //tasks.Add(task);

                var task = await Cmd.AsyncCall("Test001", new object[] { new Arg00 { A = c } }, null, new UseEntry("abc", "use01"), new UseEntry(new Business.Auth.Token { Key = "a", Remote = "b" }));

                results.Add(task.Data);

                result = Cmd.CallIResult("Test002", new object[] { new Arg00 { A = c } }, null, new UseEntry("abc", "use01"), new UseEntry(new Business.Auth.Token { Key = "a", Remote = "b" }));

                results.Add(result.Data);
            });

            //Task.WaitAll(tasks.ToArray());

            System.Threading.SpinWait.SpinUntil(() => results.Count + 1 == Loggers.Count);

            watch.Stop();
            var total = Help.Scale(watch.Elapsed.TotalSeconds, 3);

            var results2 = new int[results.Count];

            var i1 = 2;
            for (int i = 0; i < results2.Length; i++)
            {
                results2[i] = i1;

                if (0 == i) { continue; }

                if (0 == (i + 1) % 3)
                {
                    i1++;
                }
            }

            var results3 = results.OrderBy(c => c).ToList();

            if (!Enumerable.SequenceEqual(results2, results3))
            {
                throw new System.Exception("result error!");
            }

            //Console.WriteLine($"ResultCount={results.Count} TaskCount={tasks.Count}  Loggers={BusinessMember.Loggers.Count} Time={total}");
            Console.WriteLine($"ResultCount={results.Count} Loggers={BusinessMember.Loggers.Count} Time={total} Avg={Help.Scale(results.Count / total)}");

            Console.WriteLine($"{string.Join(",", BusinessMember.Logs)}");

            //watch.Restart();

            //System.Threading.Tasks.Parallel.For(0, 1000, c =>
            //{
            //    var member = Bind.Create<BusinessMember>().UseType(typeof(Business.Auth.IToken));
            //    var cmd = member.Command;

            //    TestCollection(cmd);
            //});

            //watch.Stop();
            //total = Help.Scale(watch.Elapsed.TotalSeconds, 3);
            //System.Console.WriteLine($"Bind.Create TestCollection OK Time={total}");

            //watch.Restart();

            //System.Threading.Tasks.Parallel.For(0, 10000, c =>
            //{
            //    TestCollection(Cmd);
            //});

            //watch.Stop();
            //total = Help.Scale(watch.Elapsed.TotalSeconds, 3);
            //System.Console.WriteLine($"TestCollection OK Time={total}");

            //watch.Restart();

            //TestCollection2();

            //watch.Stop();
            //total = Help.Scale(watch.Elapsed.TotalSeconds, 3);
            //System.Console.WriteLine($"TestCollection2 OK Time={total}");

            //===========================================================//
            /*
            Configer.LoggerUseThreadPoolAll(false);

            System.Console.WriteLine($"LoggerUseThreadPool = {Cfg.LoggerUseThreadPool}");

            if (!Cfg.LoggerUseThreadPool)
            {
                System.Threading.ThreadPool.SetMinThreads(4, 4);
            }

            System.Threading.ThreadPool.GetMinThreads(out workerThreads, out completionPortThreads);
            System.Threading.ThreadPool.GetMaxThreads(out workerThreads2, out completionPortThreads2);
            System.Console.WriteLine($"Min {workerThreads}, {completionPortThreads} Max {workerThreads2}, {completionPortThreads2}");

            results.Clear(); tasks.Clear(); BusinessMember.Loggers.Clear();

            watch.Reset();
            watch.Start();

            Parallel.For(0, count, c =>
            {
                var result = Cmd.Call("Test000", new object[] { new Arg00 { A = c } }, null, new UseEntry("abc", "use01"), new UseEntry(new Business.Auth.Token { Key = "a", Remote = "b" }));

                results.Add(result.Data);

                var task = Cmd.AsyncCall("Test001", new object[] { new Arg00 { A = c } }, null, new UseEntry("abc", "use01"), new UseEntry(new Business.Auth.Token { Key = "a", Remote = "b" }))
                .ContinueWith(c2 =>
                {
                    results.Add(c2.Result.Data);
                });

                tasks.Add(task);

                result = Cmd.CallIResult("Test002", new object[] { new Arg00 { A = c } }, null, new UseEntry("abc", "use01"), new UseEntry(new Business.Auth.Token { Key = "a", Remote = "b" }));

                results.Add(result.Data);
            });

            Task.WaitAll(tasks.ToArray());

            watch.Stop();
            total = Help.Scale(watch.Elapsed.TotalMilliseconds, 3);

            Console.WriteLine($"ResultCount={results.Count} TaskCount={tasks.Count}  Loggers={BusinessMember.Loggers.Count} Time={total}");
            */
#if DEBUG
            Console.Read();
#endif

        }


        public async static void TestCollection(CommandGroup cmd)
        {
            var list2 = new List<TestCollectionArg>();

            for (int i = 0; i < 10; i++)
            {
                var b = new List<TestCollectionArg.TestCollectionArg2> { new TestCollectionArg.TestCollectionArg2 { C = $"{i}", D = i } };

                list2.Add(new TestCollectionArg { A = i, B = b, C = new TestCollectionArg.TestCollectionArg3 { E = "e", F = "f" } });
            }

            var list3 = Force.DeepCloner.DeepClonerExtensions.DeepClone(list2);

            var t22 = await cmd.AsyncCall("TestCollection", new object[] { list2 });

            if (t22.State != 1)
            {
                System.Console.WriteLine(t22.JsonSerialize());
            }

            for (int i = 0; i < list2.Count; i++)
            {
                if (list2[i].A != list3[i].A + 2)
                {
                    throw new System.Exception($"result error! {list2[i].A} != {list3[i].A + 2}");
                }
            }
        }

        public async static void TestCollection2()
        {
            var list2 = new List<TestCollectionArg>();

            for (int i = 0; i < 1 * 100000; i++)
            {
                var b = new List<TestCollectionArg.TestCollectionArg2> { new TestCollectionArg.TestCollectionArg2 { C = $"{i}", D = i } };

                list2.Add(new TestCollectionArg { A = i, B = b, C = new TestCollectionArg.TestCollectionArg3 { E = "e", F = "f" } });
            }

            var list3 = Force.DeepCloner.DeepClonerExtensions.DeepClone(list2);

            var t22 = await Cmd.AsyncCall("TestCollection", new object[] { list2 });

            if (t22.State != 1)
            {
                System.Console.WriteLine(t22.JsonSerialize());
            }

            for (int i = 0; i < list2.Count; i++)
            {
                if (list2[i].A != list3[i].A + 2)
                {
                    throw new System.Exception($"result error! {list2[i].A} != {list3[i].A + 2}");
                }
            }
        }
    }

    /// <summary>
    /// Attr01
    /// </summary>
    public class Proces01 : ArgumentAttribute
    {
        public Proces01(int state = -110, string message = null) : base(state, message) { }

        public async override ValueTask<IResult> Proces(dynamic value) => this.ResultCreate(data: value + 1);
    }

    public class Arg00
    {
        /// <summary>
        /// Child Property, Field Agr<> type is only applicable to JSON
        /// </summary>
        [CheckNull]
        [Proces01(-801, "{Nick} cannot be empty, please enter the correct {Nick}", Nick = "pas2")]
        public int A;
    }

    [Info("Business", CommandGroupDefault = "DEF")]
    public class BusinessMember : BusinessBase
    {
        public static ConcurrentBag<LoggerData> Loggers = new ConcurrentBag<LoggerData>();

        public static bool SpinWait(int millisecondsTimeout) => System.Threading.SpinWait.SpinUntil(() => false, millisecondsTimeout);

        public static ConcurrentBag<int> Logs = new ConcurrentBag<int>();

        public BusinessMember() => this.Logger = new Logger(async loggers =>
        {
            if (-1 != Program.WorkTime)
            {
                SpinWait(Program.WorkTime);
            }
            //System.Console.WriteLine(loggers.Count());

            foreach (var item in loggers)
            {
                item.Value = item.Value?.ToValue();
                //item.Type = LoggerType.Error;
                Loggers.Add(item);
            }

            Logs.Add(loggers.ToList().Count);
        }, 100)
        {
            Batch = new Logger.BatchOptions
            {
                Interval = System.TimeSpan.FromSeconds(6),
                //MaxNumber = 100
            }
        };

        public virtual dynamic Test000([Use(true)]dynamic use01, Arg<Arg00> arg00, Business.Auth.Token token) => this.ResultCreate(data: arg00.Out.A + 1);

        public virtual async Task<dynamic> Test001([Use(true)]dynamic use01, Arg<Arg00> arg00, Business.Auth.Token token)
        {
            return this.ResultCreate(data: arg00.Out.A + 1);
        }

        public virtual IResult Test002([Use(true)]dynamic use01, Arg<Arg00> arg00, Business.Auth.Token token) => this.ResultCreate(data: arg00.Out.A + 1);


        public class TestCollectionAttribute : ArgumentAttribute
        {
            public TestCollectionAttribute(int state = -1106, string message = null) : base(state, message) { }

            public async override ValueTask<IResult> Proces(dynamic value)
            {
                if (value == "sss")
                {
                    return this.ResultCreate(this.State);
                }

                return this.ResultCreate();
            }
        }

        public class TestCollection2Attribute : ArgumentAttribute
        {
            public TestCollection2Attribute(int state = -1107, string message = null) : base(state, message) { }

            public async override ValueTask<IResult> Proces(dynamic value)
            {
                return this.ResultCreate(data: value + 1);
            }
        }

        public class TestCollection3Attribute : ArgumentAttribute
        {
            public TestCollection3Attribute(int state = -1108, string message = null) : base(state, message) { }

            public async override ValueTask<IResult> Proces(dynamic value)
            {
                return this.ResultCreate(data: value + 1);
            }
        }

        public class TestCollection4Attribute : ArgumentAttribute
        {
            public TestCollection4Attribute(int state = -1108, string message = null) : base(state, message) { }

            public async override ValueTask<IResult> Proces(dynamic value, IArg arg, int collectionIndex = -1, dynamic key = null)
            {
                value.A = value.A + 1;
                return this.ResultCreate(value);
            }
        }
        [TestCollection4]
        public class TestCollectionArg
        {
            [CheckNull(-1103)]
            public class TestCollectionArg2
            {
                [TestCollection(-1106)]
                public string C { get; set; }

                [TestCollection2(-1107)]
                public int D { get; set; }
            }

            [CheckNull(-1104)]
            [TestCollection3(-1108)]
            public int A { get; set; }

            [CheckNull(-1105)]
            public List<TestCollectionArg2> B { get; set; }

            public TestCollectionArg3 C { get; set; }

            public struct TestCollectionArg3
            {
                public string E { get; set; }

                public string F { get; set; }
            }
        }

        public virtual async Task<dynamic> TestCollection(
        [CheckNull(-1100)]
        [ArgumentDefault(-1102)]
        [CheckNull(-1101, CollectionItem = true)]
        Arg<List<TestCollectionArg>> a) => this.ResultCreate();
    }
}
