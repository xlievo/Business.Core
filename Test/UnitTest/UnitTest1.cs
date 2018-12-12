using Business;
using Business.Attributes;
using Business.Result;
using Business.Utils;
using Business.Auth;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System;

[assembly: JsonArg(Group = "G01")]
[assembly: Logger(LoggerType.All)]
[assembly: Command(Group = "G01")]
[assembly: Command(Group = "DEF")]
namespace UnitTest
{
    [Logger(LoggerType.Record, CanWrite = false)]
    [Use]
    public struct Use01
    {
        public string A;
    }

    public struct Use02
    {
        public string B;
    }

    /// <summary>
    /// Attr01
    /// </summary>
    public class ProcesUse02 : ArgumentAttribute
    {
        public ProcesUse02(int state = -100, string message = null, bool canNull = true) : base(state, message, canNull) { }

        public async override Task<IResult> Proces(dynamic value) => this.ResultCreate(new Use02 { B = value.A });
    }

    /// <summary>
    /// Attr01
    /// </summary>
    public class Proces01 : ArgumentAttribute
    {
        public Proces01(int state = -110, string message = null, bool canNull = true) : base(state, message, canNull) { }

        public async override Task<IResult> Proces(dynamic value) => this.ResultCreate($"{value}.1234567890");
    }

    public class AES2 : AES
    {
        public AES2(string key, int state = -821, string message = null, bool canNull = true) : base(key, state, message, canNull)
        {
        }

        public async override Task<IResult> Proces(dynamic value)
        {
            Assert.AreEqual(value, "abc.1234567890");
            return await base.Proces((object)value);
        }
    }

    /// <summary>
    /// This is Arg01.
    /// </summary>
    public struct Arg01
    {
        /// <summary>
        /// Child Property, Field Agr<> type is only applicable to JSON
        /// </summary>
        [CheckNull]
        [AES2("18dc5b9d92a843a8a178069b600fca47", Nick = "pas", Group = "DEF", Salt = "ZxeHNedT6bKpu9MEAlzq0w==")]
        [Proces01(113, "{Nick} cannot be empty, please enter the correct {Nick}", Nick = "pas2", Group = "DEF")]
        public Arg<object, dynamic> A;
    }

    [Info("Business", CommandGroupDefault = "DEF")]
    public class BusinessMember : BusinessBase<ResultObject<object>>
    {
        public BusinessMember()
        {
            this.Logger = logger =>
            {
                var data = logger.Value?.ToValue();

                System.Console.WriteLine(data.JsonSerialize());
            };
        }

        /// <summary>
        /// This is Test001.
        /// </summary>
        /// <param name="use01">This is use01.</param>
        /// <param name="arg01"></param>
        /// <param name="b">This is b.</param>
        /// <param name="c">This is c.</param>
        /// <param name="token">This is token.</param>
        /// <returns></returns>
        [Command(Group = "G01", OnlyName = "G01Test001")]
        [Command(Group = "G01", OnlyName = "G01Test002")]
        [Command(OnlyName = "DEFTest001")]
        [Command(OnlyName = "Test001")]
        public virtual async Task<dynamic> Test001(
            [Use(true)]dynamic use01,

            Arg<Arg01> arg01,

            [Business.Attributes.Ignore(IgnoreMode.BusinessArg)]
            [Size(Min = 2, Max = 32, MinMsg = "{Nick} minimum range {Min}", MaxMsg = "{Nick} maximum range {Max}", State = 113)]
            [Nick("arg.b")]
            decimal b = 0.0234m,

            [Business.Attributes.Ignore(IgnoreMode.BusinessArg)]
            [Size(Min = 2, Max = 32, MinMsg = "{Nick} minimum range {Min}", MaxMsg = "{Nick} maximum range {Max}", State = 113, Nick = "arg.c", Group = "DEF")]
            [Size(Min = 2, Max = 32, MinMsg = "{Nick} minimum range {Min}", MaxMsg = "{Nick} maximum range {Max}", State = 114, Nick = "G01arg.c", Group = "G01")]
            decimal c = 0.0234m,

            [Logger(LoggerType.Record, Group = "DEF", CanWrite = false)]Token token = default)
            =>
            this.ResultCreate(arg01.Out.A.Out);

        [Command(Group = "G02", OnlyName = "G02Test002")]
        public virtual async Task<dynamic> Test002() => this.ResultCreate(200);

        public virtual async Task<dynamic> Test003() => this.ResultCreate(-200);

        public virtual async Task<dynamic> Test004() => this.ResultCreate(new { a = "aaa" });

        public virtual async Task<dynamic> Test005()
        {
            dynamic a = new { a = "aaa" };
            return this.ResultCreate(a);
        }

        public virtual async Task<dynamic> Test006()
        {
            string a = "aaa";
            return this.ResultCreate(a);
        }

        public virtual async Task<dynamic> Test007() => this.ResultCreate(-200m);

        public virtual async Task<dynamic> Test008() => this.ResultCreate(data: -200);

        public virtual async Task<dynamic> Test009()
        {
            dynamic a = null;
            return this.ResultCreate(a, "111", 111);
        }

        public virtual async Task<dynamic> Test010() => this.ResultCreate(null);

        public virtual async Task<dynamic> Test011()
        {
            int? a = null;
            return this.ResultCreate(a);
        }

        public virtual async Task<dynamic> Test012()
        {
            int? a = 111;
            return this.ResultCreate(a);
        }

        public virtual async Task<dynamic> TestUse01([Use(true)]dynamic use01) => this.ResultCreate(use01);

        public virtual async Task<dynamic> TestUse02(IToken token = default) => this.ResultCreate(token);

        public virtual async Task<dynamic> TestUse03(dynamic a, [Use(true)]dynamic use01) => this.ResultCreate($"{a}{use01}");

        public virtual async Task<dynamic> TestAnonymous(dynamic a, [Use(true)]dynamic use01) => this.ResultCreate(new { a, b = use01 });

        public virtual async Task<dynamic> TestAnonymous2(dynamic a, [Use(true)]dynamic use01) => new { a, b = use01 };

        public virtual async Task<dynamic> TestDynamic(dynamic a, [Use(true)]dynamic use01) => use01;
    }

    [Info(CommandGroupDefault = "DEF")]
    public class BusinessMember2 : BusinessBase<ResultObject<object>>
    {
        public BusinessMember2()
        {
            this.Logger = logger =>
            {
                var data = logger.Value?.ToValue();

                System.Console.WriteLine(data.JsonSerialize());
            };
        }

        public virtual async Task<dynamic> TestLoggerAndArg(
            Use01 use01,

            Arg<Arg01> arg01,

            [Use(true)]Arg<Use01> use02,

            Arg<dynamic, Use01> use03,

            Arg<Use01, Use01> use04,

            [ProcesUse02]Arg<dynamic, Use01> use05,

            [ProcesUse02]Arg<Use02, Use01> use06,

            [Logger(LoggerType.Record, Group = "DEF", CanWrite = false)]Token token = default)
            =>
            this.ResultCreate(arg01.Out.A);
    }

    [Info("Business", CommandGroupDefault = "DEF")]
    public class BusinessMember3 : IBusiness<ResultObject<object>>
    {
        public BusinessMember3()
        {
            this.Logger = logger =>
            {
                var data = logger.Value?.ToValue();

                System.Console.WriteLine(data.JsonSerialize());
            };
        }

        public Action<LoggerData> Logger { get; set; }
        public CommandGroup Command { get; set; }
        public Configer Configer { get; set; }
        public Action BindAfter { get; set; }
        public Action<Configer> BindBefore { get; set; }
        public IResult ResultCreate(int state) => ResultFactory.ResultCreate(this, state);
        public IResult ResultCreate(int state = 1, string message = null) => ResultFactory.ResultCreate(this, state, message);
        public IResult ResultCreate<Data>(Data data, string message = null, int state = 1) => ResultFactory.ResultCreate(this, data, message, state);
        public IResult ResultCreate(object data, string message = null, int state = 1) => ResultFactory.ResultCreate(this, data, message, state);

        /// <summary>
        /// This is Test001.
        /// </summary>
        /// <param name="use01">This is use01.</param>
        /// <param name="arg01"></param>
        /// <param name="b">This is b.</param>
        /// <param name="c">This is c.</param>
        /// <param name="token">This is token.</param>
        /// <returns></returns>
        [Command(Group = "G01", OnlyName = "G01Test001")]
        [Command(Group = "G01", OnlyName = "G01Test002")]
        [Command(OnlyName = "DEFTest001")]
        [Command(OnlyName = "Test001")]
        public virtual async Task<dynamic> Test001(
            [Use(true)]dynamic use01,

            Arg<Arg01> arg01,

            [Business.Attributes.Ignore(IgnoreMode.BusinessArg)]
            [Size(Min = 2, Max = 32, MinMsg = "{Nick} minimum range {Min}", MaxMsg = "{Nick} maximum range {Max}", State = 113)]
            [Nick("arg.b")]
            decimal b = 0.0234m,

            [Business.Attributes.Ignore(IgnoreMode.BusinessArg)]
            [Size(Min = 2, Max = 32, MinMsg = "{Nick} minimum range {Min}", MaxMsg = "{Nick} maximum range {Max}", State = 113, Nick = "arg.c", Group = "DEF")]
            [Size(Min = 2, Max = 32, MinMsg = "{Nick} minimum range {Min}", MaxMsg = "{Nick} maximum range {Max}", State = 114, Nick = "G01arg.c", Group = "G01")]
            decimal c = 0.0234m,

            [Logger(LoggerType.Record, Group = "DEF", CanWrite = false)]Token token = default)
            =>
            this.ResultCreate(arg01.Out.A.Out);

        [Command(Group = "G02", OnlyName = "G02Test002")]
        public virtual async Task<dynamic> Test002() => this.ResultCreate(200);

        public virtual async Task<dynamic> Test003() => this.ResultCreate(-200);

        public virtual async Task<dynamic> Test004() => this.ResultCreate(new { a = "aaa" });

        public virtual async Task<dynamic> Test005()
        {
            dynamic a = new { a = "aaa" };
            return this.ResultCreate(a);
        }

        public virtual async Task<dynamic> Test006()
        {
            string a = "aaa";
            return this.ResultCreate(a);
        }

        public virtual async Task<dynamic> Test007() => this.ResultCreate(-200m);

        public virtual async Task<dynamic> Test008() => this.ResultCreate(data: -200);

        public virtual async Task<dynamic> Test009()
        {
            dynamic a = null;
            return this.ResultCreate(a, "111", 111);
        }

        public virtual async Task<dynamic> Test010() => this.ResultCreate(null);

        public virtual async Task<dynamic> Test011()
        {
            int? a = null;
            return this.ResultCreate(a);
        }

        public virtual async Task<dynamic> Test012()
        {
            int? a = 111;
            return this.ResultCreate(a);
        }

        public virtual async Task<dynamic> TestUse01([Use(true)]dynamic use01) => this.ResultCreate(use01);

        public virtual async Task<dynamic> TestUse02(IToken token = default) => this.ResultCreate(token);

        public virtual async Task<dynamic> TestUse03(dynamic a, [Use(true)]dynamic use01) => this.ResultCreate($"{a}{use01}");

        public virtual async Task<dynamic> TestAnonymous(dynamic a, [Use(true)]dynamic use01) => this.ResultCreate(new { a, b = use01 });

        public virtual async Task<dynamic> TestAnonymous2(dynamic a, [Use(true)]dynamic use01) => new { a, b = use01 };

        public virtual async Task<dynamic> TestDynamic(dynamic a, [Use(true)]dynamic use01) => use01;
    }

    [TestClass]
    public class TestBusinessMember
    {
        static BusinessMember Member = Bind.Create<BusinessMember>().UseType(typeof(IToken)).UseDoc();
        static CommandGroup Cmd = Member.Command;
        static Configer Cfg = Member.Configer;

        static BusinessMember3 Member3 = Bind.Create<BusinessMember3>().UseType(typeof(IToken)).UseDoc();
        static CommandGroup Cmd3 = Member3.Command;
        static Configer Cfg3 = Member3.Configer;

        static dynamic AsyncCallUse(CommandGroup businessCommand, string cmd, string group = null, object[] args = null, params object[] useObj)
        {
            var t = businessCommand.AsyncCallUse(cmd, group, args, useObj);
            t.Wait();
            return t.Result;
        }

        static dynamic AsyncCallUse(string cmd, string group = null, object[] args = null, params object[] useObj) => AsyncCallUse(Cmd, cmd, group, args, useObj);


        [TestMethod]
        public void TestCfgInfo()
        {
            Assert.AreEqual(Cfg.Info.CommandGroupDefault, "DEF");
            Assert.AreEqual(Cfg.Info.Source, AttributeBase.SourceType.Class);
            Assert.AreEqual(Cfg.Info.BusinessName, "Business");
        }

        [TestMethod]
        public void TestCfgResultType()
        {
            Assert.AreEqual(Cfg.ResultType, typeof(ResultObject<>).GetGenericTypeDefinition());
            Assert.AreEqual(typeof(IResult).IsAssignableFrom(Cfg.ResultType), true);
        }

        [TestMethod]
        public void TestCfgUseTypes()
        {
            Assert.AreEqual(Cfg.UseTypes.Count, 1);
            Assert.AreEqual(Cfg.UseTypes.Contains(typeof(IToken).FullName), true);

            var meta = Cmd.GetCommand("Test001").Meta;
            Assert.AreEqual(meta.Args.First(c => c.Type == typeof(Token)).UseType, true);
        }

        [TestMethod]
        public void TestCfgDoc()
        {
            Assert.AreNotEqual(Cfg.Doc, null);
            Assert.AreEqual(Cfg.Doc.Members.Count, 3);
            Assert.AreEqual(Cfg.Doc.Members.ContainsKey("DEF"), true);
            Assert.AreEqual(Cfg.Doc.Members.ContainsKey("G01"), true);
            Assert.AreEqual(Cfg.Doc.Members.ContainsKey("G02"), true);

            Assert.AreEqual(Cfg.Doc.Members["DEF"].ContainsKey("Test001"), true);
            Assert.AreEqual(Cfg.Doc.Members["G01"].ContainsKey("G01Test001"), true);
            Assert.AreEqual(Cfg.Doc.Members["G01"].ContainsKey("G01Test002"), true);

            Assert.AreEqual(Cfg.Doc.Members["DEF"]["Test001"].Summary, "This is Test001.");
            Assert.AreEqual(Cfg.Doc.Members["DEF"]["Test001"].Args.ElementAt(0).Summary, "This is Arg01.");
            Assert.AreEqual(Cfg.Doc.Members["DEF"]["Test001"].Args.ElementAt(1).Summary, "This is b.");
        }

        [TestMethod]
        public void TestCfgAttributes()
        {
            Assert.AreEqual(Cfg.Attributes.Count, 5);

            var json = Cfg.Attributes.FirstOrDefault(c => c.Type == typeof(JsonArgAttribute));
            Assert.AreNotEqual(json, null);
            Assert.AreEqual(json.Source, AttributeBase.SourceType.Assembly);

            var logger = Cfg.Attributes.FirstOrDefault(c => c.Type == typeof(LoggerAttribute));
            Assert.AreNotEqual(logger, null);
            Assert.AreEqual(logger.Source, AttributeBase.SourceType.Assembly);

            var command = Cfg.Attributes.FirstOrDefault(c => c.Type == typeof(CommandAttribute));
            Assert.AreNotEqual(command, null);
            Assert.AreEqual(command.Source, AttributeBase.SourceType.Assembly);

            var info = Cfg.Attributes.FirstOrDefault(c => c.Type == typeof(Info));
            Assert.AreNotEqual(info, null);
            Assert.AreEqual(info.Source, AttributeBase.SourceType.Class);
        }

        [TestMethod]
        public void TestCmd()
        {
            Assert.AreEqual(Cmd.Count, 3);
            Assert.AreEqual(Cmd.ContainsKey("DEF"), true);
            Assert.AreEqual(Cmd.ContainsKey("G01"), true);
            Assert.AreEqual(Cmd.ContainsKey("G02"), true);

            Assert.AreEqual(Cmd["DEF"].Values.Any(c => c.Key == "DEF.Test001"), true);
            Assert.AreEqual(Cmd["DEF"].Values.Any(c => c.Key == "DEF.DEFTest001"), true);
            Assert.AreEqual(Cmd["DEF"].Values.Any(c => c.Key == "DEF.Test002"), true);

            Assert.AreEqual(Cmd["G01"].Values.Any(c => c.Key == "G01.G01Test001"), true);
            Assert.AreEqual(Cmd["G01"].Values.Any(c => c.Key == "G01.G01Test002"), true);
            Assert.AreEqual(Cmd["G01"].Values.Any(c => c.Key == "G01.Test002"), true);

            Assert.AreEqual(Cmd["G02"].Values.Any(c => c.Key == "G02.G02Test002"), true);
        }

        [TestMethod]
        public void TestAttrSort()
        {
            var meta = Cmd.GetCommand("Test001").Meta;

            var attr = meta.Args[1].ArgAttrChild[0].Group[meta.GroupDefault].Attrs.First;

            Assert.AreEqual(attr.Value.State, -113);
            attr = attr.Next;
            Assert.AreEqual(attr.Value.State, -800);
            attr = attr.Next;
            Assert.AreEqual(attr.Value.State, -821);
        }

        [TestMethod]
        public void TestArgNick()
        {
            var meta = Cmd.GetCommand("Test001").Meta;

            var attr = meta.Args[2].Group[meta.GroupDefault].Attrs.First;
            Assert.AreEqual(attr.Value.Nick, "arg.b");

            attr = meta.Args[3].Group[meta.GroupDefault].Attrs.First;
            Assert.AreEqual(attr.Value.Nick, "arg.c");

            attr = meta.Args[3].Group["G01.G01Test001"].Attrs.First;
            Assert.AreEqual(attr.Value.Nick, "G01arg.c");

            attr = meta.Args[3].Group["G01.G01Test002"].Attrs.First;
            Assert.AreEqual(attr.Value.Nick, "G01arg.c");
        }

        [TestMethod]
        public void TestResult01()
        {
            var t0 = Member.Test001(null, new Arg01 { A = "abc" });
            t0.Wait();
            Assert.AreEqual(typeof(IResult).IsAssignableFrom(t0.Result.GetType()), true);
            Assert.AreEqual(t0.Result.State, -113);
            Assert.AreEqual(t0.Result.Message, "arg.b minimum range 2");

            var t1 = Member.Test001(null, new Arg01 { A = "abc" }, 2, 2);
            t1.Wait();
            Assert.AreEqual(t1.Result.State, 1);
            Assert.AreEqual(t1.Result.HasData, true);

            var t2 = AsyncCallUse("Test001", null, new object[] { new Arg01 { A = "abc" }, 2, 2 });
            Assert.AreEqual(t2.State, t1.Result.State);
            Assert.AreEqual(t2.HasData, t1.Result.HasData);

            var t3 = AsyncCallUse("DEFTest001", null, new object[] { new Arg01 { A = "abc" }, 2, 2 });
            Assert.AreEqual(t3.State, t1.Result.State);
            Assert.AreEqual(t3.HasData, t1.Result.HasData);
            Assert.AreEqual(t3.Data.Item1, t1.Result.Data.Item1);
            Assert.AreEqual(t3.Data.Item2, t1.Result.Data.Item2);

            var t4 = AsyncCallUse("G01Test001", "G01",
                //args
                new object[] { new Arg01 { A = "abc" }.JsonSerialize(), 2, 2 },
                //useObj
                new UseEntry("use01", "sss"), new Token { Key = "a", Remote = "b" });
            Assert.AreEqual(t4.Message, null);
            Assert.AreEqual(t4.State, t1.Result.State);
            Assert.AreEqual(t4.HasData, false);

            var t5 = AsyncCallUse("Test002");
            Assert.AreEqual(t5.Message, null);
            Assert.AreEqual(t5.State, 200);

            var t6 = AsyncCallUse("Test003");
            Assert.AreEqual(t6.Message, null);
            Assert.AreEqual(t6.State, -200);

            var t7 = AsyncCallUse("Test004");
            Assert.AreEqual(t7.Message, null);
            Assert.AreEqual(t7.Data.a, "aaa");

            var t8 = AsyncCallUse("Test005");
            Assert.AreEqual(t8.Message, null);
            Assert.AreEqual(t8.Data.a, "aaa");

            var t9 = AsyncCallUse("Test006");
            Assert.AreEqual(t9.Message, null);
            Assert.AreEqual(t9.Data, "aaa");

            var t10 = AsyncCallUse("Test007");
            Assert.AreEqual(t10.Message, null);
            Assert.AreEqual(t10.Data, -200m);

            var t11 = AsyncCallUse("Test008");
            Assert.AreEqual(t11.Message, null);
            Assert.AreEqual(t11.Data, -200);

            var t12 = AsyncCallUse("Test009");
            Assert.AreEqual(t12.Message, "111");
            Assert.AreEqual(t12.State, 111);
            Assert.AreEqual(t12.Data, null);

            var t13 = AsyncCallUse("Test010");
            Assert.AreEqual(t13.Data, null);

            var t14 = AsyncCallUse("Test011");
            Assert.AreEqual(t14.Message, null);
            Assert.AreEqual(t14.Data, null);

            var t15 = AsyncCallUse("Test012");
            Assert.AreEqual(t15.Message, null);
            Assert.AreEqual(t15.Data, 111);

            var t16 = AsyncCallUse("TestUse01", null, null, new UseEntry("use01", "sss"));
            Assert.AreEqual(t16.Message, null);
            Assert.AreEqual(t16.Data, "sss");

            var token = new Token { Key = "a", Remote = "b" };
            var t17 = AsyncCallUse("TestUse02", null, null, token);
            Assert.AreEqual(t17.Message, null);
            Assert.AreEqual(t17.Data, token);

            var t18 = AsyncCallUse("TestUse03", null, new object[] { "abc" }, new UseEntry("use01", "sss"));
            Assert.AreEqual(t18.Message, null);
            Assert.AreEqual(t18.Data, "abcsss");

            var t19 = AsyncCallUse("TestAnonymous", null, new object[] { "abc" }, new UseEntry("use01", "sss"));
            Assert.AreEqual(t19.Message, null);
            Assert.AreEqual(t19.Data.a, "abc");
            Assert.AreEqual(t19.Data.b, "sss");

            var t20 = AsyncCallUse("TestAnonymous2", null, new object[] { "abc" }, new UseEntry("use01", "sss"));
            Assert.AreEqual(t20.a, "abc");
            Assert.AreEqual(t20.b, "sss");

            var t21result = new Token { Key = "a", Remote = "b" };
            var t21 = AsyncCallUse("TestDynamic", null, new object[] { "abc" }, new UseEntry("use01", t21result));
            Assert.AreEqual(t21, t21result);
        }

        [TestMethod]
        public void TestLoggerAndArg()
        {
            var member = Bind.Create<BusinessMember2>().UseDoc();
            var t2 = AsyncCallUse(member.Command, "TestLoggerAndArg", null, new object[] { new Arg01 { A = "abc" } }, new UseEntry("use02", new Use01 { A = "bbb" }), new Use01 { A = "aaa" });
            Assert.AreEqual(t2.Message, null);
            Assert.AreEqual(t2.State, 1);
            Assert.AreEqual(t2.HasData, true);
        }

        [TestMethod]
        public void TestResult03()
        {
            var t0 = Member3.Test001(null, new Arg01 { A = "abc" });
            t0.Wait();
            Assert.AreEqual(typeof(IResult).IsAssignableFrom(t0.Result.GetType()), true);
            Assert.AreEqual(t0.Result.State, -113);
            Assert.AreEqual(t0.Result.Message, "arg.b minimum range 2");

            var t1 = Member3.Test001(null, new Arg01 { A = "abc" }, 2, 2);
            t1.Wait();
            Assert.AreEqual(t1.Result.State, 1);
            Assert.AreEqual(t1.Result.HasData, true);

            var t2 = AsyncCallUse(Cmd3, "Test001", null, new object[] { new Arg01 { A = "abc" }, 2, 2 });
            Assert.AreEqual(t2.State, t1.Result.State);
            Assert.AreEqual(t2.HasData, t1.Result.HasData);

            var t3 = AsyncCallUse(Cmd3, "DEFTest001", null, new object[] { new Arg01 { A = "abc" }, 2, 2 });
            Assert.AreEqual(t3.State, t1.Result.State);
            Assert.AreEqual(t3.HasData, t1.Result.HasData);
            Assert.AreEqual(t3.Data.Item1, t1.Result.Data.Item1);
            Assert.AreEqual(t3.Data.Item2, t1.Result.Data.Item2);

            var t4 = AsyncCallUse(Cmd3, "G01Test001", "G01",
                //args
                new object[] { new Arg01 { A = "abc" }.JsonSerialize(), 2, 2 },
                //useObj
                new UseEntry("use01", "sss"), new Token { Key = "a", Remote = "b" });
            Assert.AreEqual(t4.Message, null);
            Assert.AreEqual(t4.State, t1.Result.State);
            Assert.AreEqual(t4.HasData, false);

            var t5 = AsyncCallUse(Cmd3, "Test002");
            Assert.AreEqual(t5.Message, null);
            Assert.AreEqual(t5.State, 200);

            var t6 = AsyncCallUse(Cmd3, "Test003");
            Assert.AreEqual(t6.Message, null);
            Assert.AreEqual(t6.State, -200);

            var t7 = AsyncCallUse(Cmd3, "Test004");
            Assert.AreEqual(t7.Message, null);
            Assert.AreEqual(t7.Data.a, "aaa");

            var t8 = AsyncCallUse(Cmd3, "Test005");
            Assert.AreEqual(t8.Message, null);
            Assert.AreEqual(t8.Data.a, "aaa");

            var t9 = AsyncCallUse(Cmd3, "Test006");
            Assert.AreEqual(t9.Message, null);
            Assert.AreEqual(t9.Data, "aaa");

            var t10 = AsyncCallUse(Cmd3, "Test007");
            Assert.AreEqual(t10.Message, null);
            Assert.AreEqual(t10.Data, -200m);

            var t11 = AsyncCallUse(Cmd3, "Test008");
            Assert.AreEqual(t11.Message, null);
            Assert.AreEqual(t11.Data, -200);

            var t12 = AsyncCallUse(Cmd3, "Test009");
            Assert.AreEqual(t12.Message, "111");
            Assert.AreEqual(t12.State, 111);
            Assert.AreEqual(t12.Data, null);

            var t13 = AsyncCallUse(Cmd3, "Test010");
            Assert.AreEqual(t13.Data, null);

            var t14 = AsyncCallUse(Cmd3, "Test011");
            Assert.AreEqual(t14.Message, null);
            Assert.AreEqual(t14.Data, null);

            var t15 = AsyncCallUse(Cmd3, "Test012");
            Assert.AreEqual(t15.Message, null);
            Assert.AreEqual(t15.Data, 111);

            var t16 = AsyncCallUse(Cmd3, "TestUse01", null, null, new UseEntry("use01", "sss"));
            Assert.AreEqual(t16.Message, null);
            Assert.AreEqual(t16.Data, "sss");

            var token = new Token { Key = "a", Remote = "b" };
            var t17 = AsyncCallUse(Cmd3, "TestUse02", null, null, token);
            Assert.AreEqual(t17.Message, null);
            Assert.AreEqual(t17.Data, token);

            var t18 = AsyncCallUse(Cmd3, "TestUse03", null, new object[] { "abc" }, new UseEntry("use01", "sss"));
            Assert.AreEqual(t18.Message, null);
            Assert.AreEqual(t18.Data, "abcsss");

            var t19 = AsyncCallUse(Cmd3, "TestAnonymous", null, new object[] { "abc" }, new UseEntry("use01", "sss"));
            Assert.AreEqual(t19.Message, null);
            Assert.AreEqual(t19.Data.a, "abc");
            Assert.AreEqual(t19.Data.b, "sss");

            var t20 = AsyncCallUse(Cmd3, "TestAnonymous2", null, new object[] { "abc" }, new UseEntry("use01", "sss"));
            Assert.AreEqual(t20.a, "abc");
            Assert.AreEqual(t20.b, "sss");

            var t21result = new Token { Key = "a", Remote = "b" };
            var t21 = AsyncCallUse(Cmd3, "TestDynamic", null, new object[] { "abc" }, new UseEntry("use01", t21result));
            Assert.AreEqual(t21, t21result);
        }
    }
}