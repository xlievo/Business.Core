using Business;
using Business.Attributes;
using Business.Result;
using Business.Utils;
using Business.Auth;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Generic;
using static BusinessMember;

[assembly: JsonArg(Group = "G01")]
[assembly: Logger(LoggerType.All)]
[assembly: Command(Group = "G01")]
[assembly: Command(Group = CommandGroupDefault.Group)]

public struct CommandGroupDefault
{
    public const string Group = "333";
}

public class ResultObject<Type> : Business.Result.ResultObject<Type>
{
    //static ResultObject() => MessagePack.Resolvers.CompositeResolver.RegisterAndSetAsDefault(MessagePack.Resolvers.ContractlessStandardResolver.Instance);

    public ResultObject(Type data, System.Type dataType, int state = 1, string message = null, System.Type genericType = null)
        : base(data, dataType, state, message, genericType) { }

    public ResultObject(Type data, int state = 1, string message = null) : this(data, null, state, message) { }

    [MessagePack.IgnoreMember]
    public override System.Type DataType { get => base.DataType; set => base.DataType = value; }

    [MessagePack.IgnoreMember]
    public override System.Type GenericType => base.GenericType;

    public override byte[] ToBytes() => MessagePack.MessagePackSerializer.Serialize(this);
}

//[Logger(LoggerType.Record, CanWrite = false)]
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
    public ProcesUse02(int state = -100, string message = null) : base(state, message) { }

    public async override ValueTask<IResult> Proces(dynamic value) => this.ResultCreate(new Use02 { B = value.A });
}

/// <summary>
/// Attr01
/// </summary>
public class Proces01 : ArgumentAttribute
{
    public Proces01(int state = -110, string message = null) : base(state, message) { }

    public async override ValueTask<IResult> Proces(dynamic value) => this.ResultCreate($"{value}.1234567890");
}

public class AES2 : AES
{
    public AES2(string key, int state = -821, string message = null) : base(key, state, message)
    {
    }

    public async override ValueTask<IResult> Proces(dynamic value)
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
    [AES2("18dc5b9d92a843a8a178069b600fca47", Nick = "pas", Group = CommandGroupDefault.Group, Salt = "ZxeHNedT6bKpu9MEAlzq0w==")]
    //[AES2("18dc5b9d92a843a8a178069b600fca47", Nick = "pas", Salt = "ZxeHNedT6bKpu9MEAlzq0w==")]
    [Proces01(113, "{Nick} cannot be empty, please enter the correct {Nick}", Nick = "pas2", Group = CommandGroupDefault.Group)]
    public Arg<object, dynamic> A;

    public string B;
}

[Info(CommandGroupDefault = CommandGroupDefault.Group)]
public class BusinessLoggerAndArg : BusinessBase<ResultObject<object>>
{
    public BusinessLoggerAndArg()
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

        [Logger(LoggerType.Record, Group = CommandGroupDefault.Group, CanWrite = false)]Token token = default)
        =>
        this.ResultCreate(arg01.Out.A);
}

[Info("Business", CommandGroupDefault = CommandGroupDefault.Group)]
public class BusinessMember : BusinessBase
{
    public BusinessMember()
    {
        this.Logger = logger =>
        {
            logger.Value = logger.Value?.ToValue();

            System.Console.WriteLine(logger.JsonSerialize());
            System.Diagnostics.Debug.WriteLine(logger.JsonSerialize());

            if (logger.Member == "BusinessMember.Test0011" || logger.Member == "BusinessMember.Test0012")
            {
                switch (logger.Type)
                {
                    case LoggerType.Record:
                        {
                            Assert.IsNull(logger.Result);
                        }
                        break;
                    case LoggerType.Error:
                        {
                            Assert.AreEqual(typeof(IResult).IsAssignableFrom(logger.Result.GetType()), true);
                            var result = logger.Result as IResult;
                            Assert.AreEqual(1 > result.State, true);
                            Assert.IsNotNull(result.Message);
                        }
                        break;
                    case LoggerType.Exception:
                        {

                        }
                        break;
                    default: break;
                }
            }
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
            [Size(Min = 2, Max = 32, MinMsg = "{Nick} minimum range {Min}", MaxMsg = "{Nick} maximum range {Max}", State = 113, Nick = "arg.c", Group = CommandGroupDefault.Group)]
            [Size(Min = 2, Max = 32, MinMsg = "{Nick} minimum range {Min}", MaxMsg = "{Nick} maximum range {Max}", State = 114, Nick = "G01arg.c", Group = "G01")]
            decimal c = 0.0234m,

        [Logger(LoggerType.Record, Group = CommandGroupDefault.Group, CanWrite = false)]Token token = default)
        =>
        this.ResultCreate(arg01.Out.A.Out);

    /// <summary>
    /// This is Test001.
    /// </summary>
    /// <param name="use01">This is use01.</param>
    /// <param name="arg01"></param>
    /// <param name="b">This is b.</param>
    /// <param name="c">This is c.</param>
    /// <param name="token">This is token.</param>
    /// <returns></returns>
    [Command(Group = "G01", OnlyName = "G01Test0001")]
    [Command(Group = "G01", OnlyName = "G01Test0002")]
    [Command(OnlyName = "DEFTest0001")]
    [Command(OnlyName = "Test0001")]
    public virtual IResult Test0001(
        [Use(true)]dynamic use01,

        Arg<Arg01> arg01,

        [Business.Attributes.Ignore(IgnoreMode.BusinessArg)]
            [Size(Min = 2, Max = 32, MinMsg = "{Nick} minimum range {Min}", MaxMsg = "{Nick} maximum range {Max}", State = 113)]
            [Nick("arg.b")]
            decimal b = 0.0234m,

        [Business.Attributes.Ignore(IgnoreMode.BusinessArg)]
            [Size(Min = 2, Max = 32, MinMsg = "{Nick} minimum range {Min}", MaxMsg = "{Nick} maximum range {Max}", State = 113, Nick = "arg.c", Group = CommandGroupDefault.Group)]
            [Size(Min = 2, Max = 32, MinMsg = "{Nick} minimum range {Min}", MaxMsg = "{Nick} maximum range {Max}", State = 114, Nick = "G01arg.c", Group = "G01")]
            decimal c = 0.0234m,

        [Logger(LoggerType.Record, Group = CommandGroupDefault.Group, CanWrite = false)]Token token = default)
        =>
        this.ResultCreate(arg01.Out.A.Out);

    /// <summary>
    /// This is Test001.
    /// </summary>
    /// <param name="use01">This is use01.</param>
    /// <param name="arg01"></param>
    /// <param name="b">This is b.</param>
    /// <param name="c">This is c.</param>
    /// <param name="token">This is token.</param>
    /// <returns></returns>
    [Command(Group = "G01", OnlyName = "G01Test00001")]
    [Command(Group = "G01", OnlyName = "G01Test00002")]
    [Command(OnlyName = "DEFTest00001")]
    [Command(OnlyName = "Test00001")]
    public virtual dynamic Test00001(
        [Use(true)]dynamic use01,

        Arg<Arg01> arg01,

        [Business.Attributes.Ignore(IgnoreMode.BusinessArg)]
            [Size(Min = 2, Max = 32, MinMsg = "{Nick} minimum range {Min}", MaxMsg = "{Nick} maximum range {Max}", State = 113)]
            [Nick("arg.b")]
            decimal b = 0.0234m,

        [Business.Attributes.Ignore(IgnoreMode.BusinessArg)]
            [Size(Min = 2, Max = 32, MinMsg = "{Nick} minimum range {Min}", MaxMsg = "{Nick} maximum range {Max}", State = 113, Nick = "arg.c", Group = CommandGroupDefault.Group)]
            [Size(Min = 2, Max = 32, MinMsg = "{Nick} minimum range {Min}", MaxMsg = "{Nick} maximum range {Max}", State = 114, Nick = "G01arg.c", Group = "G01")]
            decimal c = 0.0234m,

        [Logger(LoggerType.Record, Group = CommandGroupDefault.Group, CanWrite = false)]Token token = default)
        =>
        this.ResultCreate(arg01.Out.A.Out);

    public virtual async Task Test0011(
        [Use(true)]dynamic use01,

        Arg<Arg01> arg01,

        [Business.Attributes.Ignore(IgnoreMode.BusinessArg)]
            [Size(Min = 2, Max = 32, MinMsg = "{Nick} minimum range {Min}", MaxMsg = "{Nick} maximum range {Max}", State = 113)]
            [Nick("arg.b")]
            decimal b = 0.0234m,

        [Business.Attributes.Ignore(IgnoreMode.BusinessArg)]
            [Size(Min = 2, Max = 32, MinMsg = "{Nick} minimum range {Min}", MaxMsg = "{Nick} maximum range {Max}", State = 113, Nick = "arg.c", Group = CommandGroupDefault.Group)]
            [Size(Min = 2, Max = 32, MinMsg = "{Nick} minimum range {Min}", MaxMsg = "{Nick} maximum range {Max}", State = 114, Nick = "G01arg.c", Group = "G01")]
            decimal c = 0.0234m,

        [Logger(LoggerType.Record, Group = CommandGroupDefault.Group, CanWrite = false)]Token token = default)
        =>
        this.ResultCreate(arg01.Out.A.Out);

    public virtual async void Test0012(
        [Use(true)]dynamic use01,

        Arg<Arg01> arg01,

        [Business.Attributes.Ignore(IgnoreMode.BusinessArg)]
            [Size(Min = 2, Max = 32, MinMsg = "{Nick} minimum range {Min}", MaxMsg = "{Nick} maximum range {Max}", State = 113)]
            [Nick("arg.b")]
            decimal b = 0.0234m,

        [Business.Attributes.Ignore(IgnoreMode.BusinessArg)]
            [Size(Min = 2, Max = 32, MinMsg = "{Nick} minimum range {Min}", MaxMsg = "{Nick} maximum range {Max}", State = 113, Nick = "arg.c", Group = CommandGroupDefault.Group)]
            [Size(Min = 2, Max = 32, MinMsg = "{Nick} minimum range {Min}", MaxMsg = "{Nick} maximum range {Max}", State = 114, Nick = "G01arg.c", Group = "G01")]
            decimal c = 0.0234m,

        [Logger(LoggerType.Record, Group = CommandGroupDefault.Group, CanWrite = false)]Token token = default)
    { }

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

    /// <summary>
    /// Attr01
    /// </summary>
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

        public async override ValueTask<IResult> Proces(dynamic value)
        {
            if (System.Object.Equals(null, value))
            {
                value = new TestCollectionArg { A = 666 };
            }

            return this.ResultCreate(value);
        }
    }

    [CheckNull(-1101)]
    [ArgumentDefault(-1102)]
    [TestCollection4(-110)]
    public class TestCollectionArg
    {
        //[CheckNull(-1103)]
        public class TestCollectionArg2
        {
            [TestCollection(-1106)]
            public string C { get; set; }

            [TestCollection2(-1107)]
            public int D { get; set; }
        }

        //[CheckNull(-1104)]
        //[TestCollection3(-1108)]
        public int A { get; set; }

        //[CheckNull(-1105)]
        //public List<TestCollectionArg2> B { get; set; }

        //public TestCollectionArg3 C { get; set; }

        public struct TestCollectionArg3
        {
            public string E { get; set; }

            public string F { get; set; }
        }
    }

    public virtual async Task<dynamic> TestCollection([CheckNull(-1100)][ArgumentDefault(-1102)]Arg<List<TestCollectionArg>> a)
    {
        return this.ResultCreate();
    }
}

[Info("Business", CommandGroupDefault = CommandGroupDefault.Group)]
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
            [Size(Min = 2, Max = 32, MinMsg = "{Nick} minimum range {Min}", MaxMsg = "{Nick} maximum range {Max}", State = 113, Nick = "arg.c", Group = CommandGroupDefault.Group)]
            [Size(Min = 2, Max = 32, MinMsg = "{Nick} minimum range {Min}", MaxMsg = "{Nick} maximum range {Max}", State = 114, Nick = "G01arg.c", Group = "G01")]
            decimal c = 0.0234m,

        [Logger(LoggerType.Record, Group = CommandGroupDefault.Group, CanWrite = false)]Token token = default)
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

[Info("Business", CommandGroupDefault = CommandGroupDefault.Group)]
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
            [Size(Min = 2, Max = 32, MinMsg = "{Nick} minimum range {Min}", MaxMsg = "{Nick} maximum range {Max}", State = 113, Nick = "arg.c", Group = CommandGroupDefault.Group)]
            [Size(Min = 2, Max = 32, MinMsg = "{Nick} minimum range {Min}", MaxMsg = "{Nick} maximum range {Max}", State = 114, Nick = "G01arg.c", Group = "G01")]
            decimal c = 0.0234m,

        [Logger(LoggerType.Record, Group = CommandGroupDefault.Group, CanWrite = false)]Token token = default)
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

    static dynamic AsyncCall(CommandGroup businessCommand, string cmd, string group = null, object[] args = null, params UseEntry[] useObj)
    {
        var t = businessCommand.AsyncCall(cmd, args, useObj, group);
        t.Wait();
        return t.Result;
    }

    static dynamic AsyncCall(string cmd, string group = null, object[] args = null, params UseEntry[] useObj) => AsyncCall(Cmd, cmd, group, args, useObj);

    [TestMethod]
    public void TestCfgInfo()
    {
        Assert.AreEqual(Cfg.Info.CommandGroupDefault, CommandGroupDefault.Group);
        Assert.AreEqual(Cfg.Info.Declaring, AttributeBase.DeclaringType.Class);
        Assert.AreEqual(Cfg.Info.BusinessName, "Business");
    }

    [TestMethod]
    public void TestCfgResultType()
    {
        Assert.AreEqual(Cfg.ResultType, typeof(Business.Result.ResultObject<>).GetGenericTypeDefinition());
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
        Assert.AreEqual(Cfg.Doc.Members.ContainsKey(CommandGroupDefault.Group), true);
        Assert.AreEqual(Cfg.Doc.Members.ContainsKey("G01"), true);
        Assert.AreEqual(Cfg.Doc.Members.ContainsKey("G02"), true);

        Assert.AreEqual(Cfg.Doc.Members[CommandGroupDefault.Group].ContainsKey("Test001"), true);
        Assert.AreEqual(Cfg.Doc.Members["G01"].ContainsKey("G01Test001"), true);
        Assert.AreEqual(Cfg.Doc.Members["G01"].ContainsKey("G01Test002"), true);

        Assert.AreEqual(Cfg.Doc.Members[CommandGroupDefault.Group]["Test001"].Summary, "This is Test001.");
        Assert.AreEqual(Cfg.Doc.Members[CommandGroupDefault.Group]["Test001"].Args.ElementAt(0).Summary, "This is Arg01.");
        Assert.AreEqual(Cfg.Doc.Members[CommandGroupDefault.Group]["Test001"].Args.ElementAt(1).Summary, "This is b.");
    }

    [TestMethod]
    public void TestCollectionMeta()
    {
        //Assert.AreNotEqual(Cfg.Doc, null);
        //Assert.AreEqual(Cfg.Doc.Members.Count, 3);
        //Assert.AreEqual(Cfg.Doc.Members.ContainsKey(CommandGroupDefault.Group), true);
        //Assert.AreEqual(Cfg.Doc.Members.ContainsKey("G01"), true);
        //Assert.AreEqual(Cfg.Doc.Members.ContainsKey("G02"), true);

        //Assert.AreEqual(Cfg.Doc.Members[CommandGroupDefault.Group].ContainsKey("Test001"), true);
        //Assert.AreEqual(Cfg.Doc.Members["G01"].ContainsKey("G01Test001"), true);
        //Assert.AreEqual(Cfg.Doc.Members["G01"].ContainsKey("G01Test002"), true);

        //Assert.AreEqual(Cfg.Doc.Members[CommandGroupDefault.Group]["Test001"].Summary, "This is Test001.");
        //Assert.AreEqual(Cfg.Doc.Members[CommandGroupDefault.Group]["Test001"].Args.ElementAt(0).Summary, "This is Arg01.");
        //Assert.AreEqual(Cfg.Doc.Members[CommandGroupDefault.Group]["Test001"].Args.ElementAt(1).Summary, "This is b.");
    }

    [TestMethod]
    public void TestCfgAttributes()
    {
        Assert.AreEqual(Cfg.Attributes.Count, 5);

        var json = Cfg.Attributes.FirstOrDefault(c => c.Type == typeof(JsonArgAttribute));
        Assert.AreNotEqual(json, null);
        Assert.AreEqual(json.Declaring, AttributeBase.DeclaringType.Assembly);

        var logger = Cfg.Attributes.FirstOrDefault(c => c.Type == typeof(LoggerAttribute));
        Assert.AreNotEqual(logger, null);
        Assert.AreEqual(logger.Declaring, AttributeBase.DeclaringType.Assembly);

        var command = Cfg.Attributes.FirstOrDefault(c => c.Type == typeof(CommandAttribute));
        Assert.AreNotEqual(command, null);
        Assert.AreEqual(command.Declaring, AttributeBase.DeclaringType.Assembly);

        var info = Cfg.Attributes.FirstOrDefault(c => c.Type == typeof(Info));
        Assert.AreNotEqual(info, null);
        Assert.AreEqual(info.Declaring, AttributeBase.DeclaringType.Class);
    }

    [TestMethod]
    public void TestCmd()
    {
        Assert.AreEqual(Cmd.Count, 3);
        Assert.AreEqual(Cmd.ContainsKey(CommandGroupDefault.Group), true);
        Assert.AreEqual(Cmd.ContainsKey("G01"), true);
        Assert.AreEqual(Cmd.ContainsKey("G02"), true);

        Assert.AreEqual(Cmd[CommandGroupDefault.Group].Values.Any(c => c.Key == $"{CommandGroupDefault.Group}.Test001"), true);
        Assert.AreEqual(Cmd[CommandGroupDefault.Group].Values.Any(c => c.Key == $"{CommandGroupDefault.Group}.DEFTest001"), true);
        Assert.AreEqual(Cmd[CommandGroupDefault.Group].Values.Any(c => c.Key == $"{CommandGroupDefault.Group}.Test002"), true);

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
    public void TestLoggerAndArg()
    {
        var member = Bind.Create<BusinessLoggerAndArg>().UseDoc();

        Configer.LoggerSet(new LoggerAttribute(LoggerType.Record, canWrite: false) { Group = "333" }, typeof(Use02));

        var t2 = AsyncCall(member.Command, "TestLoggerAndArg", null, new object[] { new Arg01 { A = "abc" } }, new UseEntry(new Use01 { A = "bbb" }, "use02"), new UseEntry(new Use01 { A = "aaa" }));
        Assert.AreEqual(t2.Message, null);
        Assert.AreEqual(t2.State, 1);
        Assert.AreEqual(t2.HasData, true);

        System.Threading.Tasks.Task.WaitAll();
    }

    void TestResult(IBusiness business)
    {
        var t2 = AsyncCall(business.Command, "Test001", null, new object[] { new Arg01 { A = "abc" }, 2, 2 });
        Assert.AreEqual(t2.State, 1);
        Assert.AreEqual(t2.Message, null);
        Assert.AreEqual(t2.HasData, true);

        var t3 = AsyncCall(business.Command, "DEFTest001", null, new object[] { new Arg01 { A = "abc" }, 2, 2 });
        Assert.AreEqual(t3.State, t2.State);
        Assert.AreEqual(t3.HasData, t2.HasData);
        Assert.AreEqual(t3.Data.Item1, t2.Data.Item1);
        Assert.AreEqual(t3.Data.Item2, t2.Data.Item2);

        var t4 = AsyncCall(business.Command, "G01Test001", "G01",
            //args
            new object[] { new Arg01 { A = "abc" }.JsonSerialize(), 2, 2 },
            //useObj
            new UseEntry("use01", "sss"), new UseEntry(new Token { Key = "a", Remote = "b" }));
        Assert.AreEqual(t4.Message, null);
        Assert.AreEqual(t4.State, t2.State);
        Assert.AreEqual(t4.HasData, false);

        var t5 = AsyncCall(business.Command, "Test002");
        Assert.AreEqual(t5.Message, null);
        Assert.AreEqual(t5.State, 200);

        var t6 = AsyncCall(business.Command, "Test003");
        Assert.AreEqual(t6.Message, null);
        Assert.AreEqual(t6.State, -200);

        var t7 = AsyncCall(business.Command, "Test004");
        Assert.AreEqual(t7.Message, null);
        Assert.AreEqual(t7.Data.a, "aaa");

        var t8 = AsyncCall(business.Command, "Test005");
        Assert.AreEqual(t8.Message, null);
        Assert.AreEqual(t8.Data.a, "aaa");

        var t9 = AsyncCall(business.Command, "Test006");
        Assert.AreEqual(t9.Message, null);
        Assert.AreEqual(t9.Data, "aaa");

        var t10 = AsyncCall(business.Command, "Test007");
        Assert.AreEqual(t10.Message, null);
        Assert.AreEqual(t10.Data, -200m);

        var t11 = AsyncCall(business.Command, "Test008");
        Assert.AreEqual(t11.Message, null);
        Assert.AreEqual(t11.Data, -200);

        var t12 = AsyncCall(business.Command, "Test009");
        Assert.AreEqual(t12.Message, "111");
        Assert.AreEqual(t12.State, 111);
        Assert.AreEqual(t12.Data, null);

        var t13 = AsyncCall(business.Command, "Test010");
        Assert.AreEqual(t13.Data, null);

        var t14 = AsyncCall(business.Command, "Test011");
        Assert.AreEqual(t14.Message, null);
        Assert.AreEqual(t14.Data, null);

        var t15 = AsyncCall(business.Command, "Test012");
        Assert.AreEqual(t15.Message, null);
        Assert.AreEqual(t15.Data, 111);

        var t16 = AsyncCall(business.Command, "TestUse01", null, null, new UseEntry("sss", "use01"));
        Assert.AreEqual(t16.Message, null);
        Assert.AreEqual(t16.Data, "sss");

        var token = new Token { Key = "a", Remote = "b" };
        var t17 = AsyncCall(business.Command, "TestUse02", null, null, new UseEntry(token));
        Assert.AreEqual(t17.Message, null);
        Assert.AreEqual(t17.Data, token);

        var t18 = AsyncCall(business.Command, "TestUse03", null, new object[] { "abc" }, new UseEntry("sss", "use01"));
        Assert.AreEqual(t18.Message, null);
        Assert.AreEqual(t18.Data, "abcsss");

        var t19 = AsyncCall(business.Command, "TestAnonymous", null, new object[] { "abc" }, new UseEntry("sss", "use01"));
        Assert.AreEqual(t19.Message, null);
        Assert.AreEqual(t19.Data.a, "abc");
        Assert.AreEqual(t19.Data.b, "sss");

        var t20 = AsyncCall(business.Command, "TestAnonymous2", null, new object[] { "abc" }, new UseEntry("sss", "use01"));
        Assert.AreEqual(t20.a, "abc");
        Assert.AreEqual(t20.b, "sss");

        var t21result = new Token { Key = "a", Remote = "b" };
        var t21 = AsyncCall(business.Command, "TestDynamic", null, new object[] { "abc" }, new UseEntry(t21result, "use01"));
        Assert.AreEqual(t21, t21result);
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

        //Cmd
        var t2 = Cmd.AsyncCall("Test001", new object[] { new Arg01 { A = "abc" } });
        t2.Wait();
        Assert.AreEqual(typeof(IResult).IsAssignableFrom(t2.Result.GetType()), true);
        Assert.AreEqual(t2.Result.State, -113);
        Assert.AreEqual(t2.Result.Message, "arg.b minimum range 2");

        var t3 = Cmd.AsyncCall("Test001", new object[] { new Arg01 { A = "abc" }, 2, 2 });
        t3.Wait();
        Assert.AreEqual(t3.Result.State, 1);
        Assert.AreEqual(t3.Result.HasData, true);

        TestResult(Member);
    }

    [TestMethod]
    public void TestResult02()
    {
        var member = Bind.Create<BusinessMember2>().UseType(typeof(IToken));

        var t0 = member.Test001(null, new Arg01 { A = "abc" });
        t0.Wait();
        Assert.AreEqual(typeof(IResult).IsAssignableFrom(t0.Result.GetType()), true);
        Assert.AreEqual(t0.Result.State, -113);
        Assert.AreEqual(t0.Result.Message, "arg.b minimum range 2");

        var t1 = member.Test001(null, new Arg01 { A = "abc" }, 2, 2);
        t1.Wait();
        Assert.AreEqual(t1.Result.State, 1);
        Assert.AreEqual(t1.Result.HasData, true);

        TestResult(member);
    }

    [TestMethod]
    public void TestResult03()
    {
        var member = Bind.Create<BusinessMember3>().UseType(typeof(IToken));

        var t0 = member.Test001(null, new Arg01 { A = "abc" });
        t0.Wait();
        Assert.AreEqual(typeof(IResult).IsAssignableFrom(t0.Result.GetType()), true);
        Assert.AreEqual(t0.Result.State, -113);
        Assert.AreEqual(t0.Result.Message, "arg.b minimum range 2");

        var t1 = member.Test001(null, new Arg01 { A = "abc" }, 2, 2);
        t1.Wait();
        Assert.AreEqual(t1.Result.State, 1);
        Assert.AreEqual(t1.Result.HasData, true);

        TestResult(member);
    }

    [TestMethod]
    public void TestResult001()
    {
        var t0 = Member.Test0001(null, new Arg01 { A = "abc" });
        Assert.AreEqual(typeof(IResult).IsAssignableFrom(t0.GetType()), true);
        Assert.AreEqual(t0.State, -113);
        Assert.AreEqual(t0.Message, "arg.b minimum range 2");

        var t1 = Member.Test0001(null, new Arg01 { A = "abc" }, 2, 2);
        Assert.AreEqual(t1.State, 1);
        Assert.AreEqual(t1.HasData, true);

        //Cmd
        var t2 = Cmd.AsyncCall("Test0001", new object[] { new Arg01 { A = "abc" } });
        t2.Wait();
        Assert.AreEqual(typeof(IResult).IsAssignableFrom(t2.Result.GetType()), true);
        Assert.AreEqual(t2.Result.State, -113);
        Assert.AreEqual(t2.Result.Message, "arg.b minimum range 2");

        var t3 = Cmd.AsyncCall("Test0001", new object[] { new Arg01 { A = "abc" }, 2, 2 });
        t3.Wait();
        Assert.AreEqual(t3.Result.State, 1);
        Assert.AreEqual(t3.Result.HasData, true);

        //Cmd
        var t4 = Cmd.Call("Test0001", new object[] { new Arg01 { A = "abc" } });
        Assert.AreEqual(typeof(IResult).IsAssignableFrom(t4.GetType()), true);
        Assert.AreEqual(t4.State, -113);
        Assert.AreEqual(t4.Message, "arg.b minimum range 2");

        var t5 = Cmd.Call("Test0001", new object[] { new Arg01 { A = "abc" }, 2, 2 });
        Assert.AreEqual(t5.State, 1);
        Assert.AreEqual(t5.HasData, true);
    }

    [TestMethod]
    public void TestResult0001()
    {
        var t0 = Member.Test00001(null, new Arg01 { A = "abc" });
        Assert.AreEqual(typeof(IResult).IsAssignableFrom(t0.GetType()), true);
        Assert.AreEqual(t0.State, -113);
        Assert.AreEqual(t0.Message, "arg.b minimum range 2");

        var t1 = Member.Test00001(null, new Arg01 { A = "abc" }, 2, 2);
        Assert.AreEqual(t1.State, 1);
        Assert.AreEqual(t1.HasData, true);

        //Cmd
        var t2 = Cmd.AsyncCall("Test00001", new object[] { new Arg01 { A = "abc" } });
        t2.Wait();
        Assert.AreEqual(typeof(IResult).IsAssignableFrom(t2.Result.GetType()), true);
        Assert.AreEqual(t2.Result.State, -113);
        Assert.AreEqual(t2.Result.Message, "arg.b minimum range 2");

        var t3 = Cmd.AsyncCall("Test00001", new object[] { new Arg01 { A = "abc" }, 2, 2 });
        t3.Wait();
        Assert.AreEqual(t3.Result.State, 1);
        Assert.AreEqual(t3.Result.HasData, true);

        //Cmd
        var t4 = Cmd.Call("Test00001", new object[] { new Arg01 { A = "abc" } });
        Assert.AreEqual(typeof(IResult).IsAssignableFrom(t4.GetType()), true);
        Assert.AreEqual(t4.State, -113);
        Assert.AreEqual(t4.Message, "arg.b minimum range 2");

        var t5 = Cmd.Call("Test00001", new object[] { new Arg01 { A = "abc" }, 2, 2 });
        Assert.AreEqual(t5.State, 1);
        Assert.AreEqual(t5.HasData, true);
    }

    [TestMethod]
    public void TestResult011()
    {
        var t0 = Member.Test0011(null, new Arg01 { A = "abc" });
        t0.Wait();

        var t1 = Member.Test0011(null, new Arg01 { A = "abc" }, 2, 2);
        t1.Wait();

        //Cmd
        var t2 = Cmd.AsyncCall("Test0011", new object[] { new Arg01 { A = "abc" } });
        t2.Wait();
        Assert.IsNull(t2.Result);

        var t3 = Cmd.AsyncCall("Test0011", new object[] { new Arg01 { A = "abc" }, 2, 2 });
        t3.Wait();
        Assert.IsNull(t3.Result);
    }

    [TestMethod]
    public void TestResult012()
    {
        Member.Test0012(null, new Arg01 { A = "abc" });

        Member.Test0012(null, new Arg01 { A = "abc" }, 2, 2);

        Member.Test0012(null, new Arg01 { A = "abc", B = "ex" }, 2, 2);

        //Cmd
        var t2 = Cmd.AsyncCall("Test0012", new object[] { new Arg01 { A = "abc" } });
        t2.Wait();
        Assert.IsNull(t2.Result);

        var t3 = Cmd.AsyncCall("Test0012", new object[] { new Arg01 { A = "abc" }, 2, 2 });
        t3.Wait();
        Assert.IsNull(t3.Result);
    }

    [TestMethod]
    public void TestCollection()
    {
        var list2 = new List<TestCollectionArg.TestCollectionArg2>();

        for (int i = 0; i < 2; i++)
        {
            list2.Add(new TestCollectionArg.TestCollectionArg2 { C = $"{i}" });
        }

        list2[1].C = "sss";

        var list3 = new List<TestCollectionArg> { new TestCollectionArg { A = 1 } };

        //var t22 = AsyncCall(Member.Command, "TestCollection", null, new object[] { list3 });

        //Assert.AreEqual(t22.State, -1106);

        //list3[0].B = null;

        //var t23 = AsyncCall(Member.Command, "TestCollection", null, new object[] { list3 });

        //Assert.AreEqual(t23.State, -1105);

        list3[0] = null;

        var t24 = AsyncCall(Member.Command, "TestCollection", null, new object[] { list3 });

        Assert.AreEqual(t24.State, 1);
    }
    /*
    [TestMethod]
    public void TestCollection2()
    {
        var list2 = new List<TestCollectionArg.TestCollectionArg2>();

        for (int i = 0; i < 5; i++)
        {
            list2.Add(new TestCollectionArg.TestCollectionArg2 { C = $"{i}", D = i });
        }

        var t22 = AsyncCall(Member.Command, "TestCollection", null, new object[] {
            new List<TestCollectionArg> { new TestCollectionArg { A = 1, B = list2 } }
        });

        Assert.AreEqual(t22.State, 1);

        Assert.AreEqual(list2[0].D, 1);
        Assert.AreEqual(list2[1].D, 2);
        Assert.AreEqual(list2[2].D, 3);
        Assert.AreEqual(list2[3].D, 4);
        Assert.AreEqual(list2[4].D, 5);
    }

    [TestMethod]
    public void TestCollection3()
    {
        var list2 = new List<TestCollectionArg>();

        for (int i = 0; i < 5; i++)
        {
            var b = new List<TestCollectionArg.TestCollectionArg2> { new TestCollectionArg.TestCollectionArg2 { C = $"{i}", D = i } };

            list2.Add(new TestCollectionArg { A = i, B = b, C = new TestCollectionArg.TestCollectionArg3 { E = "e", F = "f" } });
        }

        var list3 = Force.DeepCloner.DeepClonerExtensions.DeepClone(list2);

        var t22 = AsyncCall(Member.Command, "TestCollection", null, new object[] { list2 });

        Assert.AreEqual(t22.State, 1);

        for (int i = 0; i < list2.Count; i++)
        {
            Assert.AreEqual(list2[i].A, list3[i].A + 1);
        }
    }
    */
}
