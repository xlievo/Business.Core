using Business;
using Business.Attributes;
using Business.Result;
using Business.Utils;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

[assembly: Logger(LoggerType.All)]
namespace BenchmarkTest
{
    class Program
    {
        static BusinessMember Member;
        static CommandGroup Cmd;
        static Configer Cfg;

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
            var count = 10000;

#else
            var count = 100000;
#endif
            System.Console.WriteLine($"RUN Count = {count} X 3");

            var results = new ConcurrentBag<int>();
            var tasks = new ConcurrentBag<Task>();
            var watch = new System.Diagnostics.Stopwatch();

            //~preheat
            var t = Cmd.AsyncCall("Test001", new object[] { new Arg00 { A = 123 } }, new UseEntry[] { new UseEntry("sss", "use01"), new UseEntry(new Business.Auth.Token { Key = "a", Remote = "b" }) });
            t.Wait();
            //~end

            //===========================================================//

            Configer.LoggerUseThreadPoolAll();

            System.Console.WriteLine($"LoggerUseThreadPool = {Cfg.LoggerUseThreadPool}");

            if (Cfg.LoggerUseThreadPool)
            {
                //System.Threading.ThreadPool.SetMinThreads(800, 300);
            }

            System.Threading.ThreadPool.GetMinThreads(out int workerThreads, out int completionPortThreads);
            System.Threading.ThreadPool.GetMaxThreads(out int workerThreads2, out int completionPortThreads2);
            System.Console.WriteLine($"Min {workerThreads}, {completionPortThreads} Max {workerThreads2}, {completionPortThreads2}");

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
            var total = Help.Scale(watch.Elapsed.TotalMilliseconds, 3);

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

            Console.WriteLine($"ResultCount={results.Count} TaskCount={tasks.Count}  Loggers={BusinessMember.Loggers.Count} Time={total}");

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
    }

    /// <summary>
    /// Attr01
    /// </summary>
    public class Proces01 : ArgumentAttribute
    {
        public Proces01(int state = -110, string message = null) : base(state, message) { }

        public async override Task<IResult> Proces(dynamic value) => this.ResultCreate(data: value + 1);
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

        public BusinessMember() => this.Logger = logger =>
        {
            System.Threading.Thread.SpinWait(100);

            Loggers.Add(logger);
        };

        public virtual dynamic Test000([Use(true)]dynamic use01, Arg<Arg00> arg00, Business.Auth.Token token) => this.ResultCreate(data: arg00.Out.A + 1);

        public virtual async Task<dynamic> Test001([Use(true)]dynamic use01, Arg<Arg00> arg00, Business.Auth.Token token) => this.ResultCreate(data: arg00.Out.A + 1);

        public virtual IResult Test002([Use(true)]dynamic use01, Arg<Arg00> arg00, Business.Auth.Token token) => this.ResultCreate(data: arg00.Out.A + 1);
    }
}
