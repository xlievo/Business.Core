using Business.Core.Annotations;
using Business.Core.Result;
using Business.Core.Utils;
using Business.Core.Auth;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Generic;
using static BusinessMember;
using Business.Core;

[assembly: JsonArg(Group = "G01")]
[assembly: Logger(Logger.Type.All)]
[assembly: Command(Group = "G01")]
[assembly: Command(Group = CommandGroupDefault.Group)]

public struct CommandGroupDefault
{
    public const string Group = "333";
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

    public async override ValueTask<IResult> Proces(dynamic value)
    {
        return this.ResultCreate(new Use02 { B = value.A });
    }
}

/// <summary>
/// Attr01
/// </summary>
public class Proces01 : ArgumentAttribute
{
    public Proces01(int state = -110, string message = null) : base(state, message) { }

    public async override ValueTask<IResult> Proces<Type>(dynamic value) => this.ResultCreate($"{value}.1234567890");
}

public class AES2 : AESAttribute
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
public class Arg00
{
    /// <summary>
    /// Child Property, Field Agr<> type is only applicable to JSON
    /// </summary>
    [CheckNull]
    [AES2("18dc5b9d92a843a8a178069b600fca47", Alias = "pas", Group = CommandGroupDefault.Group, Salt = "ZxeHNedT6bKpu9MEAlzq0w==")]
    //[AES2("18dc5b9d92a843a8a178069b600fca47", Nick = "pas", Salt = "ZxeHNedT6bKpu9MEAlzq0w==")]
    [Proces01(113, "{Alias} cannot be empty, please enter the correct {Alias}", Alias = "pas2", Group = CommandGroupDefault.Group)]
    public dynamic A { get; set; }

    public string B { get; set; }
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
    [AES2("18dc5b9d92a843a8a178069b600fca47", Alias = "pas", Group = CommandGroupDefault.Group, Salt = "ZxeHNedT6bKpu9MEAlzq0w==")]
    //[AES2("18dc5b9d92a843a8a178069b600fca47", Nick = "pas", Salt = "ZxeHNedT6bKpu9MEAlzq0w==")]
    [Proces01(113, "{Alias} cannot be empty, please enter the correct {Alias}", Alias = "pas2", Group = CommandGroupDefault.Group)]
    public dynamic A { get; set; }

    public string B { get; set; }
}

[Info(CommandGroupDefault = CommandGroupDefault.Group)]
public class BusinessLoggerAndArg : BusinessBase<ResultObject<object>>
{
    public BusinessLoggerAndArg()
    {
        this.Logger = new Logger(async loggers =>
        {
            //var data = logger.Value?.ToValue();

            loggers.Select(c =>
            {
                //c.Value = c.Value?.ToValue();
                return c;
            });

            Console.WriteLine(loggers.JsonSerialize());
        });
    }

    public virtual async ValueTask<dynamic> TestLoggerAndArg(
        Use01 use01,

        Arg01 arg01,

        [Use(ParameterName = true)] Use01 use02,

        [Use(typeof(Use01))] dynamic use03,

        Use01 use04,

        [ProcesUse02][Use(typeof(Use01))] dynamic use05,

        [ProcesUse02][Use(typeof(Use01))] Use02 use06,

        [Logger(Logger.Type.Record, Group = CommandGroupDefault.Group, CanWrite = false)] Token token = default)
        =>
        this.ResultCreate(arg01.A);
}

public enum TestMode
{
    OK,
    ERROR,
    Exception,
}

[Info("Business2")]
[Logger]
public class BusinessMember2
{
    public virtual async ValueTask<dynamic> Test000(
        [Use(ParameterName = true)] dynamic use01,

        Arg00 arg01,

        [Business.Core.Annotations.Ignore(IgnoreMode.BusinessArg)]
            [Size(Min = 2, Max = 32, MinMsg = "{Alias} minimum range {Min}", MaxMsg = "{Alias} maximum range {Max}", State = 113)]
            [Alias("arg.b")]
            decimal b = 0.0234m,

        [Business.Core.Annotations.Ignore(IgnoreMode.BusinessArg)]
            [Size(Min = 2, Max = 32, MinMsg = "{Alias} minimum range {Min}", MaxMsg = "{Alias} maximum range {Max}", State = 113, Alias = "arg.c", Group = CommandGroupDefault.Group)]
            [Size(Min = 2, Max = 32, MinMsg = "{Alias} minimum range {Min}", MaxMsg = "{Alias} maximum range {Max}", State = 114, Alias = "G01arg.c", Group = "G01")]
            decimal c = 0.0234m,

        [Logger(Logger.Type.Record, Group = CommandGroupDefault.Group, CanWrite = false)] Token token = default)
        =>
        arg01.A;

    [Command(Group = "G01", OnlyName = "G01Test001")]
    [Command(Group = "G01", OnlyName = "G01Test002")]
    [Command(OnlyName = "DEFTest001")]
    [Command(OnlyName = "Test001")]
    public virtual async ValueTask<dynamic> Test001(
        [Use(ParameterName = true)] dynamic use01,

        Arg01 arg01,

        [Business.Core.Annotations.Ignore(IgnoreMode.BusinessArg)]
            [Size(Min = 2, Max = 32, MinMsg = "{Alias} minimum range {Min}", MaxMsg = "{Alias} maximum range {Max}", State = 113)]
            [Alias("arg.b")]
            decimal b = 0.0234m,

        [Business.Core.Annotations.Ignore(IgnoreMode.BusinessArg)]
            [Size(Min = 2, Max = 32, MinMsg = "{Alias} minimum range {Min}", MaxMsg = "{Alias} maximum range {Max}", State = 113, Alias = "arg.c", Group = CommandGroupDefault.Group)]
            [Size(Min = 2, Max = 32, MinMsg = "{Alias} minimum range {Min}", MaxMsg = "{Alias} maximum range {Max}", State = 114, Alias = "G01arg.c", Group = "G01")]
            decimal c = 0.0234m,

        [Logger(Logger.Type.Record, Group = CommandGroupDefault.Group, CanWrite = false)] Token token = default)
        =>
        arg01.A;
}

[Info("Business", CommandGroupDefault = CommandGroupDefault.Group)]
public class BusinessMember : IBusiness<ResultObject<object>>
{
    public BusinessMember()
    {
        //this.Logger = logger =>
        //{
        //    logger.Value = logger.Value?.ToValue();

        //    Console.WriteLine(logger.JsonSerialize());
        //    System.Diagnostics.Debug.WriteLine(logger.JsonSerialize());

        //    if (logger.Member == "BusinessMember.Test0011" || logger.Member == "BusinessMember.Test0012")
        //    {
        //        switch (logger.Type)
        //        {
        //            case LoggerType.Record:
        //                {
        //                    Assert.IsNull(logger.Result);
        //                }
        //                break;
        //            case LoggerType.Error:
        //                {
        //                    Assert.AreEqual(typeof(IResult).IsAssignableFrom(logger.Result.GetType()), true);
        //                    var result = logger.Result as IResult;
        //                    Assert.AreEqual(1 > result.State, true);
        //                    Assert.IsNotNull(result.Message);
        //                }
        //                break;
        //            case LoggerType.Exception:
        //                {

        //                }
        //                break;
        //            default: break;
        //        }
        //    }
        //};

        this.Logger = new Logger(async (Logger.LoggerData x) =>
        {
            //foreach (var item in x)
            //{
            //    Help.Console(item.ToString());
            //}
            //Common.LogClient.Call("Write", null, new Logs { Index = "log", Data = x.Select(c => c.ToString()) }.JsonSerialize());
            //Help.Console(.ToString());

            Help.Console(x.ToString());

        });
    }

    #region realization

    public Logger Logger { get; set; }

    public CommandGroup Command { get; set; }

    public Configer Configer { get; set; }

    public Action BindAfter { get; set; }

    public Action<Configer> BindBefore { get; set; }

    public dynamic ResultCreate(int state = 1, string message = null, string callback = null, [System.Runtime.CompilerServices.CallerMemberName] string method = null) => this.Configer.MetaData.TryGetValue(method ?? string.Empty, out Business.Core.Meta.MetaData meta) ? ResultFactory.ResultCreate(meta, state, message) : ResultFactory.ResultCreate(this.Configer.ResultTypeDefinition, state, message);

    public IResult<Data> ResultCreate<Data>(Data data, string message = null, int state = 1, string callback = null) => ResultFactory.ResultCreate(this.Configer.ResultTypeDefinition, data, message, state);

    public IResult ResultCreate(object data, string message = null, int state = 1, string callback = null) => ResultFactory.ResultCreate(this.Configer.ResultTypeDefinition, data, message, state);

    #endregion

    public virtual async ValueTask<dynamic> Test000(
        [Use(ParameterName = true)] dynamic use01,

        Arg00 arg01,

        [Business.Core.Annotations.Ignore(IgnoreMode.BusinessArg)]
            [Size(Min = 2, Max = 32, MinMsg = "{Alias} minimum range {Min}", MaxMsg = "{Alias} maximum range {Max}", State = 113)]
            [Alias("arg.b")]
            decimal b = 0.0234m,

        [Business.Core.Annotations.Ignore(IgnoreMode.BusinessArg)]
            [Size(Min = 2, Max = 32, MinMsg = "{Alias} minimum range {Min}", MaxMsg = "{Alias} maximum range {Max}", State = 113, Alias = "arg.c", Group = CommandGroupDefault.Group)]
            [Size(Min = 2, Max = 32, MinMsg = "{Alias} minimum range {Min}", MaxMsg = "{Alias} maximum range {Max}", State = 114, Alias = "G01arg.c", Group = "G01")]
            decimal c = 0.0234m,

        [Logger(Logger.Type.Record, Group = CommandGroupDefault.Group, CanWrite = false)] Token token = default) => this.ResultCreate(arg01.A);

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
    public virtual async ValueTask<dynamic> Test001(
        [Use(ParameterName = true)] dynamic use01,

        Arg01 arg01,

        [Business.Core.Annotations.Ignore(IgnoreMode.BusinessArg)]
            [Size(Min = 2, Max = 32, MinMsg = "{Alias} minimum range {Min}", MaxMsg = "{Alias} maximum range {Max}", State = 113)]
            [Alias("arg.b")]
            decimal b = 0.0234m,

        [Business.Core.Annotations.Ignore(IgnoreMode.BusinessArg)]
            [Size(Min = 2, Max = 32, MinMsg = "{Alias} minimum range {Min}", MaxMsg = "{Alias} maximum range {Max}", State = 113, Alias = "arg.c", Group = CommandGroupDefault.Group)]
            [Size(Min = 2, Max = 32, MinMsg = "{Alias} minimum range {Min}", MaxMsg = "{Alias} maximum range {Max}", State = 114, Alias = "G01arg.c", Group = "G01")]
            decimal c = 0.0234m,

        [Logger(Logger.Type.Record, Group = CommandGroupDefault.Group, CanWrite = false)] Token token = default)
        =>
        this.ResultCreate(arg01.A);

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
        [Use(ParameterName = true)] dynamic use01,

        Arg01 arg01,

        [Business.Core.Annotations.Ignore(IgnoreMode.BusinessArg)]
            [Size(Min = 2, Max = 32, MinMsg = "{Alias} minimum range {Min}", MaxMsg = "{Alias} maximum range {Max}", State = 113)]
            [Alias("arg.b")]
            decimal b = 0.0234m,

        [Business.Core.Annotations.Ignore(IgnoreMode.BusinessArg)]
            [Size(Min = 2, Max = 32, MinMsg = "{Alias} minimum range {Min}", MaxMsg = "{Alias} maximum range {Max}", State = 113, Alias = "arg.c", Group = CommandGroupDefault.Group)]
            [Size(Min = 2, Max = 32, MinMsg = "{Alias} minimum range {Min}", MaxMsg = "{Alias} maximum range {Max}", State = 114, Alias = "G01arg.c", Group = "G01")]
            decimal c = 0.0234m,

        [Logger(Logger.Type.Record, Group = CommandGroupDefault.Group, CanWrite = false)] Token token = default)
        =>
        this.ResultCreate(arg01.A);

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
        [Use(ParameterName = true)] dynamic use01,

        Arg01 arg01,

        [Business.Core.Annotations.Ignore(IgnoreMode.BusinessArg)]
            [Size(Min = 2, Max = 32, MinMsg = "{Alias} minimum range {Min}", MaxMsg = "{Alias} maximum range {Max}", State = 113)]
            [Alias("arg.b")]
            decimal b = 0.0234m,

        [Business.Core.Annotations.Ignore(IgnoreMode.BusinessArg)]
            [Size(Min = 2, Max = 32, MinMsg = "{Alias} minimum range {Min}", MaxMsg = "{Alias} maximum range {Max}", State = 113, Alias = "arg.c", Group = CommandGroupDefault.Group)]
            [Size(Min = 2, Max = 32, MinMsg = "{Alias} minimum range {Min}", MaxMsg = "{Alias} maximum range {Max}", State = 114, Alias = "G01arg.c", Group = "G01")]
            decimal c = 0.0234m,

        [Logger(Logger.Type.Record, Group = CommandGroupDefault.Group, CanWrite = false)] Token token = default)
        =>
        this.ResultCreate(arg01.A);

    public virtual async ValueTask Test0011(
        [Use(ParameterName = true)] dynamic use01,

        Arg01 arg01,

        [Business.Core.Annotations.Ignore(IgnoreMode.BusinessArg)]
            [Size(Min = 2, Max = 32, MinMsg = "{Alias} minimum range {Min}", MaxMsg = "{Alias} maximum range {Max}", State = 113)]
            [Alias("arg.b")]
            decimal b = 0.0234m,

        [Business.Core.Annotations.Ignore(IgnoreMode.BusinessArg)]
            [Size(Min = 2, Max = 32, MinMsg = "{Alias} minimum range {Min}", MaxMsg = "{Alias} maximum range {Max}", State = 113, Alias = "arg.c", Group = CommandGroupDefault.Group)]
            [Size(Min = 2, Max = 32, MinMsg = "{Alias} minimum range {Min}", MaxMsg = "{Alias} maximum range {Max}", State = 114, Alias = "G01arg.c", Group = "G01")]
            decimal c = 0.0234m,

        [Logger(Logger.Type.Record, Group = CommandGroupDefault.Group, CanWrite = false)] Token token = default)
        =>
        this.ResultCreate(arg01.A);

    public virtual async void Test0012(
        [Use(ParameterName = true)] dynamic use01,

        Arg01 arg01,

        [Business.Core.Annotations.Ignore(IgnoreMode.BusinessArg)]
            [Size(Min = 2, Max = 32, MinMsg = "{Alias} minimum range {Min}", MaxMsg = "{Alias} maximum range {Max}", State = 113)]
            [Alias("arg.b")]
            decimal b = 0.0234m,

        [Business.Core.Annotations.Ignore(IgnoreMode.BusinessArg)]
            [Size(Min = 2, Max = 32, MinMsg = "{Alias} minimum range {Min}", MaxMsg = "{Alias} maximum range {Max}", State = 113, Alias = "arg.c", Group = CommandGroupDefault.Group)]
            [Size(Min = 2, Max = 32, MinMsg = "{Alias} minimum range {Min}", MaxMsg = "{Alias} maximum range {Max}", State = 114, Alias = "G01arg.c", Group = "G01")]
            decimal c = 0.0234m,

        [Logger(Logger.Type.Record, Group = CommandGroupDefault.Group, CanWrite = false)] Token token = default)
    { }

    public virtual async ValueTask Test00122(
        [Use(ParameterName = true)] dynamic use01,

        Arg01 arg01,

        [Business.Core.Annotations.Ignore(IgnoreMode.BusinessArg)]
            [Size(Min = 2, Max = 32, MinMsg = "{Alias} minimum range {Min}", MaxMsg = "{Alias} maximum range {Max}", State = 113)]
            [Alias("arg.b")]
            decimal b = 0.0234m,

        [Business.Core.Annotations.Ignore(IgnoreMode.BusinessArg)]
            [Size(Min = 2, Max = 32, MinMsg = "{Alias} minimum range {Min}", MaxMsg = "{Alias} maximum range {Max}", State = 113, Alias = "arg.c", Group = CommandGroupDefault.Group)]
            [Size(Min = 2, Max = 32, MinMsg = "{Alias} minimum range {Min}", MaxMsg = "{Alias} maximum range {Max}", State = 114, Alias = "G01arg.c", Group = "G01")]
            decimal c = 0.0234m,

        [Logger(Logger.Type.Record, Group = CommandGroupDefault.Group, CanWrite = false)] Token token = default)
    { }

    [Command(Group = "G02", OnlyName = "G02Test002")]
    public virtual async ValueTask<dynamic> Test002() => this.ResultCreate(200);

    public virtual async ValueTask<dynamic> Test003() => this.ResultCreate(-200);

    public virtual async ValueTask<dynamic> Test004() => this.ResultCreate(new { a = "aaa" });

    // public virtual async ValueTask Test0044() => this.ResultCreate(-200);

    public virtual async ValueTask<int> Test001222(
        [Use(ParameterName = true)] dynamic use01,

        Arg01 arg01,

        [Business.Core.Annotations.Ignore(IgnoreMode.BusinessArg)]
            [Size(Min = 2, Max = 32, MinMsg = "{Alias} minimum range {Min}", MaxMsg = "{Alias} maximum range {Max}", State = 113)]
            [Alias("arg.b")]
            decimal b = 0.0234m,

        [Business.Core.Annotations.Ignore(IgnoreMode.BusinessArg)]
            [Size(Min = 2, Max = 32, MinMsg = "{Alias} minimum range {Min}", MaxMsg = "{Alias} maximum range {Max}", State = 113, Alias = "arg.c", Group = CommandGroupDefault.Group)]
            [Size(Min = 2, Max = 32, MinMsg = "{Alias} minimum range {Min}", MaxMsg = "{Alias} maximum range {Max}", State = 114, Alias = "G01arg.c", Group = "G01")]
            decimal c = 0.0234m,

        [Logger(Logger.Type.Record, Group = CommandGroupDefault.Group, CanWrite = false)] Token token = default) => 333;

    public virtual async ValueTask<string> Test001223(
       [Use(ParameterName = true)] dynamic use01,

       Arg01 arg01,

       [Business.Core.Annotations.Ignore(IgnoreMode.BusinessArg)]
            [Size(Min = 2, Max = 32, MinMsg = "{Alias} minimum range {Min}", MaxMsg = "{Alias} maximum range {Max}", State = 113)]
            [Alias("arg.b")]
            decimal b = 0.0234m,

       [Business.Core.Annotations.Ignore(IgnoreMode.BusinessArg)]
            [Size(Min = 2, Max = 32, MinMsg = "{Alias} minimum range {Min}", MaxMsg = "{Alias} maximum range {Max}", State = 113, Alias = "arg.c", Group = CommandGroupDefault.Group)]
            [Size(Min = 2, Max = 32, MinMsg = "{Alias} minimum range {Min}", MaxMsg = "{Alias} maximum range {Max}", State = 114, Alias = "G01arg.c", Group = "G01")]
            decimal c = 0.0234m,

       [Logger(Logger.Type.Record, Group = CommandGroupDefault.Group, CanWrite = false)] Token token = default) => "333";

    public virtual async void Test00122x(
        [Use(ParameterName = true)] dynamic use01,

        Arg01 arg01,

        [Business.Core.Annotations.Ignore(IgnoreMode.BusinessArg)]
            [Size(Min = 2, Max = 32, MinMsg = "{Alias} minimum range {Min}", MaxMsg = "{Alias} maximum range {Max}", State = 113)]
            [Alias("arg.b")]
            decimal b = 0.0234m,

        [Business.Core.Annotations.Ignore(IgnoreMode.BusinessArg)]
            [Size(Min = 2, Max = 32, MinMsg = "{Alias} minimum range {Min}", MaxMsg = "{Alias} maximum range {Max}", State = 113, Alias = "arg.c", Group = CommandGroupDefault.Group)]
            [Size(Min = 2, Max = 32, MinMsg = "{Alias} minimum range {Min}", MaxMsg = "{Alias} maximum range {Max}", State = 114, Alias = "G01arg.c", Group = "G01")]
            decimal c = 0.0234m,

        [Logger(Logger.Type.Record, Group = CommandGroupDefault.Group, CanWrite = false)] Token token = default)
    { }

    public virtual int Test001222x(
        [Use(ParameterName = true)] dynamic use01,

        Arg01 arg01,

        [Business.Core.Annotations.Ignore(IgnoreMode.BusinessArg)]
            [Size(Min = 2, Max = 32, MinMsg = "{Alias} minimum range {Min}", MaxMsg = "{Alias} maximum range {Max}", State = 113)]
            [Alias("arg.b")]
            decimal b = 0.0234m,

        [Business.Core.Annotations.Ignore(IgnoreMode.BusinessArg)]
            [Size(Min = 2, Max = 32, MinMsg = "{Alias} minimum range {Min}", MaxMsg = "{Alias} maximum range {Max}", State = 113, Alias = "arg.c", Group = CommandGroupDefault.Group)]
            [Size(Min = 2, Max = 32, MinMsg = "{Alias} minimum range {Min}", MaxMsg = "{Alias} maximum range {Max}", State = 114, Alias = "G01arg.c", Group = "G01")]
            decimal c = 0.0234m,

        [Logger(Logger.Type.Record, Group = CommandGroupDefault.Group, CanWrite = false)] Token token = default) => -333;

    public virtual string Test001223x(
       [Use(ParameterName = true)] dynamic use01,

       Arg01 arg01,

       [Business.Core.Annotations.Ignore(IgnoreMode.BusinessArg)]
            [Size(Min = 2, Max = 32, MinMsg = "{Alias} minimum range {Min}", MaxMsg = "{Alias} maximum range {Max}", State = 113)]
            [Alias("arg.b")]
            decimal b = 0.0234m,

       [Business.Core.Annotations.Ignore(IgnoreMode.BusinessArg)]
            [Size(Min = 2, Max = 32, MinMsg = "{Alias} minimum range {Min}", MaxMsg = "{Alias} maximum range {Max}", State = 113, Alias = "arg.c", Group = CommandGroupDefault.Group)]
            [Size(Min = 2, Max = 32, MinMsg = "{Alias} minimum range {Min}", MaxMsg = "{Alias} maximum range {Max}", State = 114, Alias = "G01arg.c", Group = "G01")]
            decimal c = 0.0234m,

       [Logger(Logger.Type.Record, Group = CommandGroupDefault.Group, CanWrite = false)] Token token = default) => "333";

    //Test005
    public virtual async ValueTask<dynamic> Test005(TestMode mode)
    {
        switch (mode)
        {
            case TestMode.OK:
                dynamic a = new { a = "aaa" };
                return this.ResultCreate(a);
            case TestMode.ERROR:
                return this.ResultCreate(-999, "test error");
            case TestMode.Exception:
                throw new Exception("exception");
            default: return this.ResultCreate();
        }
    }
    public virtual async ValueTask<IResult> Test005a(TestMode mode)
    {
        switch (mode)
        {
            case TestMode.OK:
                dynamic a = new { a = "aaa" };
                return this.ResultCreate(a);
            case TestMode.ERROR:
                return this.ResultCreate(-999, "test error");
            case TestMode.Exception:
                throw new Exception("exception");
            default: return this.ResultCreate();
        }
    }
    public virtual IResult Test005b(TestMode mode)
    {
        switch (mode)
        {
            case TestMode.OK:
                dynamic a = new { a = "aaa" };
                return this.ResultCreate(a);
            case TestMode.ERROR:
                return this.ResultCreate(-999, "test error");
            case TestMode.Exception:
                throw new Exception("exception");
            default: return this.ResultCreate();
        }
    }
    //Test006
    public virtual async ValueTask<dynamic> Test006(TestMode mode)
    {
        switch (mode)
        {
            case TestMode.OK:
                string a = "aaa";
                return this.ResultCreate(a);
            case TestMode.ERROR:
                return this.ResultCreate(-999, "test error");
            case TestMode.Exception:
                throw new Exception("exception");
            default: return this.ResultCreate();
        }
    }
    public virtual async ValueTask<IResult<string>> Test006a(TestMode mode)
    {
        switch (mode)
        {
            case TestMode.OK:
                string a = "aaa";
                return this.ResultCreate(a);
            case TestMode.ERROR:
                return this.ResultCreate(-999, "test error");
            case TestMode.Exception:
                throw new Exception("exception");
            default: return this.ResultCreate();
        }
    }
    public virtual IResult<string> Test006b(TestMode mode)
    {
        switch (mode)
        {
            case TestMode.OK:
                string a = "aaa";
                return this.ResultCreate(a);
            case TestMode.ERROR:
                return this.ResultCreate(-999, "test error");
            case TestMode.Exception:
                throw new Exception("exception");
            default: return this.ResultCreate();
        }
    }
    //Test007
    public virtual async ValueTask<dynamic> Test007(TestMode mode)
    {
        switch (mode)
        {
            case TestMode.OK:
                return this.ResultCreate(-200m);
            case TestMode.ERROR:
                return this.ResultCreate(-999, "test error");
            case TestMode.Exception:
                throw new Exception("exception");
            default: return this.ResultCreate();
        }
    }
    public virtual async ValueTask<IResult<decimal>> Test007a(TestMode mode)
    {
        switch (mode)
        {
            case TestMode.OK:
                return this.ResultCreate(-200m);
            case TestMode.ERROR:
                return this.ResultCreate(-999, "test error");
            case TestMode.Exception:
                throw new Exception("exception");
            default: return this.ResultCreate();
        }
    }
    public virtual IResult<decimal> Test007b(TestMode mode)
    {
        switch (mode)
        {
            case TestMode.OK:
                return this.ResultCreate(-200m);
            case TestMode.ERROR:
                return this.ResultCreate(-999, "test error");
            case TestMode.Exception:
                throw new Exception("exception");
            default: return this.ResultCreate();
        }
    }
    //Test008
    public virtual async ValueTask<dynamic> Test008(TestMode mode)
    {
        switch (mode)
        {
            case TestMode.OK:
                return this.ResultCreate(data: -200);
            case TestMode.ERROR:
                return this.ResultCreate(-999, "test error");
            case TestMode.Exception:
                throw new Exception("exception");
            default: return this.ResultCreate();
        }
    }
    public virtual async ValueTask<IResult<int>> Test008a(TestMode mode)
    {
        switch (mode)
        {
            case TestMode.OK:
                return this.ResultCreate(data: -200);
            case TestMode.ERROR:
                return this.ResultCreate(-999, "test error");
            case TestMode.Exception:
                throw new Exception("exception");
            default: return this.ResultCreate();
        }
    }
    public virtual IResult<int> Test008b(TestMode mode)
    {
        switch (mode)
        {
            case TestMode.OK:
                return this.ResultCreate(data: -200);
            case TestMode.ERROR:
                return this.ResultCreate(-999, "test error");
            case TestMode.Exception:
                throw new Exception("exception");
            default: return this.ResultCreate();
        }
    }
    //Test009
    public virtual async ValueTask<dynamic> Test009(TestMode mode)
    {
        switch (mode)
        {
            case TestMode.OK:
                dynamic a = null;
                return this.ResultCreate(a, "111", 111);
            case TestMode.ERROR:
                return this.ResultCreate(-999, "test error");
            case TestMode.Exception:
                throw new Exception("exception");
            default: return this.ResultCreate();
        }
    }
    public virtual async ValueTask<IResult<dynamic>> Test009a(TestMode mode)
    {
        switch (mode)
        {
            case TestMode.OK:
                dynamic a = null;
                return this.ResultCreate(a, "111", 111);
            case TestMode.ERROR:
                return this.ResultCreate(-999, "test error");
            case TestMode.Exception:
                throw new Exception("exception");
            default: return this.ResultCreate();
        }
    }
    public virtual IResult<dynamic> Test009b(TestMode mode)
    {
        switch (mode)
        {
            case TestMode.OK:
                dynamic a = null;
                return this.ResultCreate(a, "111", 111);
            case TestMode.ERROR:
                return this.ResultCreate(-999, "test error");
            case TestMode.Exception:
                throw new Exception("exception");
            default: return this.ResultCreate();
        }
    }
    //Test010
    public virtual async ValueTask<dynamic> Test010(TestMode mode)
    {
        switch (mode)
        {
            case TestMode.OK:
                return this.ResultCreate(null);
            case TestMode.ERROR:
                return this.ResultCreate(-999, "test error");
            case TestMode.Exception:
                throw new Exception("exception");
            default: return this.ResultCreate();
        }
    }
    public virtual async ValueTask<IResult> Test010a(TestMode mode)
    {
        switch (mode)
        {
            case TestMode.OK:
                return this.ResultCreate(null);
            case TestMode.ERROR:
                return this.ResultCreate(-999, "test error");
            case TestMode.Exception:
                throw new Exception("exception");
            default: return this.ResultCreate();
        }
    }
    public virtual IResult Test010b(TestMode mode)
    {
        switch (mode)
        {
            case TestMode.OK:
                return this.ResultCreate(null);
            case TestMode.ERROR:
                return this.ResultCreate(-999, "test error");
            case TestMode.Exception:
                throw new Exception("exception");
            default: return this.ResultCreate();
        }
    }
    //Test011
    public virtual async ValueTask<dynamic> Test011(TestMode mode)
    {
        switch (mode)
        {
            case TestMode.OK:
                int? a = null;
                return this.ResultCreate(a);
            case TestMode.ERROR:
                return this.ResultCreate(-999, "test error");
            case TestMode.Exception:
                throw new Exception("exception");
            default: return this.ResultCreate();
        }
    }
    public virtual async ValueTask<IResult<int?>> Test011a(TestMode mode)
    {
        switch (mode)
        {
            case TestMode.OK:
                int? a = null;
                return this.ResultCreate(a);
            case TestMode.ERROR:
                return this.ResultCreate(-999, "test error");
            case TestMode.Exception:
                throw new Exception("exception");
            default: return this.ResultCreate();
        }
    }
    public virtual IResult<int?> Test011b(TestMode mode)
    {
        switch (mode)
        {
            case TestMode.OK:
                int? a = null;
                return this.ResultCreate(a);
            case TestMode.ERROR:
                return this.ResultCreate(-999, "test error");
            case TestMode.Exception:
                throw new Exception("exception");
            default: return this.ResultCreate();
        }
    }
    //Test012
    public virtual async ValueTask<dynamic> Test012(TestMode mode)
    {
        switch (mode)
        {
            case TestMode.OK:
                return this.ResultCreate(DateTime.Now);
            case TestMode.ERROR:
                return this.ResultCreate(-999, "test error");
            case TestMode.Exception:
                throw new Exception("exception");
            default: return this.ResultCreate();
        }
    }
    public virtual async ValueTask<IResult<DateTime>> Test012a(TestMode mode)
    {
        switch (mode)
        {
            case TestMode.OK:
                return this.ResultCreate(DateTime.Now);
            case TestMode.ERROR:
                return this.ResultCreate(-999, "test error");
            case TestMode.Exception:
                throw new Exception("exception");
            default: return this.ResultCreate();
        }
    }
    public virtual IResult<DateTime> Test012b(TestMode mode)
    {
        switch (mode)
        {
            case TestMode.OK:
                return this.ResultCreate(DateTime.Now);
            case TestMode.ERROR:
                return this.ResultCreate(-999, "test error");
            case TestMode.Exception:
                throw new Exception("exception");
            default: return this.ResultCreate();
        }
    }
    //Test013
    public virtual async ValueTask<dynamic> Test013(TestMode mode)
    {
        switch (mode)
        {
            case TestMode.OK:
                DateTime? a = null;
                return this.ResultCreate(a);
            case TestMode.ERROR:
                return this.ResultCreate(-999, "test error");
            case TestMode.Exception:
                throw new Exception("exception");
            default: return this.ResultCreate();
        }
    }
    public virtual async ValueTask<IResult<DateTime?>> Test013a(TestMode mode)
    {
        switch (mode)
        {
            case TestMode.OK:
                DateTime? a = null;
                return this.ResultCreate(a);
            case TestMode.ERROR:
                return this.ResultCreate(-999, "test error");
            case TestMode.Exception:
                throw new Exception("exception");
            default: return this.ResultCreate();
        }
    }
    public virtual IResult<DateTime?> Test013b(TestMode mode)
    {
        switch (mode)
        {
            case TestMode.OK:
                DateTime? a = null;
                return this.ResultCreate(a);
            case TestMode.ERROR:
                return this.ResultCreate(-999, "test error");
            case TestMode.Exception:
                throw new Exception("exception");
            default: return this.ResultCreate();
        }
    }
    public virtual IResult Test013c(TestMode mode)
    {
        switch (mode)
        {
            case TestMode.OK:
                return this.ResultCreate(DateTime.Now);
            case TestMode.ERROR:
                return this.ResultCreate(-999, "test error");
            case TestMode.Exception:
                throw new Exception("exception");
            default: return this.ResultCreate();
        }
    }
    public virtual DateTime Test013d(TestMode mode)
    {
        switch (mode)
        {
            case TestMode.OK:
                return DateTime.Now;
            case TestMode.ERROR:
                return default;
            case TestMode.Exception:
                throw new Exception("exception");
            default: return default;
        }
    }
    public virtual DateTime? Test013e(TestMode mode)
    {
        switch (mode)
        {
            case TestMode.OK:
                return DateTime.Now;
            case TestMode.ERROR:
                return default;
            case TestMode.Exception:
                throw new Exception("exception");
            default: return default;
        }
    }
    public virtual string Test013f(TestMode mode)
    {
        switch (mode)
        {
            case TestMode.OK:
                return DateTime.Now.ToString();
            case TestMode.ERROR:
                return default;
            case TestMode.Exception:
                throw new Exception("exception");
            default: return default;
        }
    }
    //Test014
    public virtual async ValueTask Test014(TestMode mode)
    {
        switch (mode)
        {
            case TestMode.OK:
                break;
            case TestMode.ERROR:
                break;
            case TestMode.Exception:
                throw new Exception("exception");
            default: break;
        }
    }
    //public virtual async void Test014a(TestMode mode)
    //{
    //    switch (mode)
    //    {
    //        case TestMode.OK:
    //            break;
    //        case TestMode.ERROR:
    //            break;
    //        case TestMode.Exception:
    //            throw new System.Exception("exception");
    //        default: break;
    //    }
    //}
    public virtual void Test014b(TestMode mode)
    {
        switch (mode)
        {
            case TestMode.OK:
                break;
            case TestMode.ERROR:
                break;
            case TestMode.Exception:
                throw new Exception("exception");
            default: break;
        }
    }

    //TestUse01
    public virtual async ValueTask<dynamic> TestUse01(TestMode mode, [Use(ParameterName = true)] dynamic use01)
    {
        switch (mode)
        {
            case TestMode.OK:
                return this.ResultCreate(use01);
            case TestMode.ERROR:
                return this.ResultCreate(-999, "test error");
            case TestMode.Exception:
                throw new Exception("exception");
            default: return this.ResultCreate();
        }
    }
    public virtual async ValueTask<IResult> TestUse01a(TestMode mode, [Use(ParameterName = true)] dynamic use01)
    {
        switch (mode)
        {
            case TestMode.OK:
                return this.ResultCreate(use01);
            case TestMode.ERROR:
                return this.ResultCreate(-999, "test error");
            case TestMode.Exception:
                throw new Exception("exception");
            default: return this.ResultCreate();
        }
    }
    public virtual IResult TestUse01b(TestMode mode, [Use(ParameterName = true)] dynamic use01)
    {
        switch (mode)
        {
            case TestMode.OK:
                return this.ResultCreate(use01);
            case TestMode.ERROR:
                return this.ResultCreate(-999, "test error");
            case TestMode.Exception:
                throw new Exception("exception");
            default: return this.ResultCreate();
        }
    }
    //TestUse02
    public virtual async ValueTask<dynamic> TestUse02(TestMode mode, IToken token = default)
    {
        switch (mode)
        {
            case TestMode.OK:
                return this.ResultCreate(token);
            case TestMode.ERROR:
                return this.ResultCreate(-999, "test error");
            case TestMode.Exception:
                throw new Exception("exception");
            default: return this.ResultCreate();
        }
    }

    public virtual async ValueTask<IResult<IToken>> TestUse02a(TestMode mode, IToken token = default)
    {
        switch (mode)
        {
            case TestMode.OK:
                return this.ResultCreate(token);
            case TestMode.ERROR:
                return this.ResultCreate(-999, "test error");
            case TestMode.Exception:
                throw new Exception("exception");
            default: return this.ResultCreate();
        }
    }
    public virtual IResult<IToken> TestUse02b(TestMode mode, IToken token = default)
    {
        switch (mode)
        {
            case TestMode.OK:
                return this.ResultCreate(token);
            case TestMode.ERROR:
                return this.ResultCreate(-999, "test error");
            case TestMode.Exception:
                throw new Exception("exception");
            default: return this.ResultCreate();
        }
    }
    //TestUse03
    public virtual async ValueTask<dynamic> TestUse03(TestMode mode, dynamic a, [Use(ParameterName = true)] dynamic use01)
    {
        switch (mode)
        {
            case TestMode.OK:
                return this.ResultCreate($"{a}{use01}");
            case TestMode.ERROR:
                return this.ResultCreate(-999, "test error");
            case TestMode.Exception:
                throw new Exception("exception");
            default: return this.ResultCreate();
        }
    }
    public virtual async ValueTask<IResult<string>> TestUse03a(TestMode mode, dynamic a, [Use(ParameterName = true)] dynamic use01)
    {
        switch (mode)
        {
            case TestMode.OK:
                return this.ResultCreate($"{a}{use01}");
            case TestMode.ERROR:
                return this.ResultCreate(-999, "test error");
            case TestMode.Exception:
                throw new Exception("exception");
            default: return this.ResultCreate();
        }
    }
    public virtual IResult<string> TestUse03b(TestMode mode, dynamic a, [Use(ParameterName = true)] dynamic use01)
    {
        switch (mode)
        {
            case TestMode.OK:
                return this.ResultCreate($"{a}{use01}");
            case TestMode.ERROR:
                return this.ResultCreate(-999, "test error");
            case TestMode.Exception:
                throw new Exception("exception");
            default: return this.ResultCreate();
        }
    }
    //TestAnonymous
    public virtual async ValueTask<dynamic> TestAnonymous(TestMode mode, dynamic a, [Use(ParameterName = true)] dynamic use01)
    {
        switch (mode)
        {
            case TestMode.OK:
                return this.ResultCreate(new { a, b = use01 });
            case TestMode.ERROR:
                return this.ResultCreate(-999, "test error");
            case TestMode.Exception:
                throw new Exception("exception");
            default: return this.ResultCreate();
        }
    }
    public virtual async ValueTask<IResult> TestAnonymousa(TestMode mode, dynamic a, [Use(ParameterName = true)] dynamic use01)
    {
        switch (mode)
        {
            case TestMode.OK:
                return this.ResultCreate(new { a, b = use01 });
            case TestMode.ERROR:
                return this.ResultCreate(-999, "test error");
            case TestMode.Exception:
                throw new Exception("exception");
            default: return this.ResultCreate();
        }
    }
    public virtual IResult TestAnonymousb(TestMode mode, dynamic a, [Use(ParameterName = true)] dynamic use01)
    {
        switch (mode)
        {
            case TestMode.OK:
                return this.ResultCreate(new { a, b = use01 });
            case TestMode.ERROR:
                return this.ResultCreate(-999, "test error");
            case TestMode.Exception:
                throw new Exception("exception");
            default: return this.ResultCreate();
        }
    }

    public virtual async ValueTask<dynamic> TestAnonymous2(dynamic a, [Use(ParameterName = true)] dynamic use01) => new { a, b = use01 };
    public virtual async ValueTask<dynamic> TestAnonymous2e(dynamic a, [Use(ParameterName = true)] dynamic use01) => this.ResultCreate(-999, "test error");

    public virtual async ValueTask<dynamic> TestDynamic(dynamic a, [Use(ParameterName = true)] dynamic use01) => use01;

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

        //public async override ValueTask<IResult> Proces(dynamic value, int collectionIndex, dynamic key)
        //{
        //    //if (200000 < value.A)
        //    //{
        //    //    value.A = 369;
        //    //}
        //    value.A = value.A + 1;
        //    return this.ResultCreate(value);
        //}
    }

    public class CheckedMemberTypeAttribute : ArgumentAttribute
    {
        public CheckedMemberTypeAttribute(int state = -2000, string message = null) : base(state, message) { }

        //public async override ValueTask<IResult> Proces(dynamic value, int collectionIndex, dynamic key)
        //{
        //    Assert.IsTrue(this.ArgMeta.MemberType.IsCollection());
        //    return this.ResultCreate();
        //}
    }

    //[TestCollection4(CollectionItem = true)]
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
        [CheckedMemberType]
        public List<TestCollectionArg2> B { get; set; }

        public TestCollectionArg3 C { get; set; }

        public struct TestCollectionArg3
        {
            public string E { get; set; }

            public string F { get; set; }
        }
    }

    //[TestCollection4(CollectionItem = true)]
    public class TestDictArg
    {
        [CheckNull(-1103)]
        public class TestDictArg2
        {
            [TestCollection(-1106)]
            public string C { get; set; }

            //[TestCollection2(-1107)]
            public int D { get; set; }
        }

        [CheckNull(-1104)]
        //[TestCollection3(-1108)]
        public int A { get; set; }

        [CheckNull(-1105)]
        public Dictionary<string, TestDictArg2> B { get; set; }

        public TestCollectionArg3 C { get; set; }

        public struct TestCollectionArg3
        {
            public string E { get; set; }

            public string F { get; set; }
        }
    }

    public virtual async ValueTask<dynamic> TestCollection(
        [CheckNull(-1100)]
        //[ArgumentDefault(-1102)]
        //[CheckNull(-1101, CollectionItem = true)]
        List<TestCollectionArg> a)
    {
        return this.ResultCreate();
    }

    public virtual async ValueTask<dynamic> TestDict(
        [CheckNull(-1100)]
        //[ArgumentDefault(-1102)]
        //[CheckNull(-1101, CollectionItem = true)]
        Dictionary<string, TestDictArg> a)
    {
        return this.ResultCreate();
    }

    public class TestHasLowerArg
    {
        public class TestHasLower2
        {
            public List<TestHasLower3> C { get; set; }

            public int D { get; set; }
        }

        public List<TestHasLower2> B { get; set; }

        public class TestHasLower3
        {
            public string E { get; set; }

            [CheckNull(-1100)]
            public string F { get; set; }
        }
    }

    public struct TestHasLowerArg_b
    {
        [CheckNull(-1200)]
        public string A { get; set; }
    }

    public virtual async ValueTask<dynamic> TestHasLower(List<TestHasLowerArg> a, TestHasLowerArg_b b = default, string c = default)
    {
        return this.ResultCreate();
    }

    public class TestHasLowerArg2
    {
        public class TestHasLower2
        {
            public TestHasLower3 C { get; set; }

            public int D { get; set; }
        }

        public TestHasLower2 B { get; set; }

        public class TestHasLower3
        {
            public string E { get; set; }

            [CheckNull(-1100)]
            public string F { get; set; }
        }
    }

    public virtual async ValueTask<dynamic> TestHasLower2(TestHasLowerArg2 a, TestHasLowerArg_b b = default, string c = default)
    {
        return this.ResultCreate();
    }

    public class TestCollectionExceptionAttribute : ArgumentAttribute
    {
        public TestCollectionExceptionAttribute(int state = -1111, string message = null) : base(state, message) { }

        //public async override ValueTask<IResult> Proces(dynamic value, int collectionIndex, dynamic key)
        //{
        //    if (1 == collectionIndex)
        //    {
        //        throw new System.Exception($"Attribute Proces exception! {collectionIndex}");
        //    }

        //    return this.ResultCreate();
        //}
    }

    public class TestExceptionAttribute : ArgumentAttribute
    {
        public TestExceptionAttribute(int state = -1111, string message = null) : base(state, message) { }

        public async override ValueTask<IResult> Proces(dynamic value)
        {
            throw new Exception($"Attribute Proces exception!");
        }
    }

    public class TestCollectionExceptionArg
    {
        public class TestHasLower2
        {
            public List<TestHasLower3> C { get; set; }

            public int D { get; set; }
        }

        public List<TestHasLower2> B { get; set; }

        public class TestHasLower3
        {
            public string E { get; set; }

            [TestException]
            public string F { get; set; }
        }
    }

    public virtual async ValueTask<dynamic> TestCollectionException(List<TestCollectionExceptionArg> a) => this.ResultCreate();

    public class TestExceptionArg
    {
        public class TestHasLower2
        {
            public TestHasLower3 C { get; set; }

            public int D { get; set; }
        }

        public TestHasLower2 B { get; set; }

        public class TestHasLower3
        {
            public string E { get; set; }

            [TestException]
            public string F { get; set; }
        }
    }

    public virtual async ValueTask<dynamic> TestException(TestExceptionArg a) => this.ResultCreate();
}

[TestClass]
public class TestBusinessMember
{
    static BusinessMember2 Member2 = Bootstrap.Create<BusinessMember2>().Build();
    static BusinessMember Member = Bootstrap.Create<BusinessMember>().UseType(typeof(IToken)).UseDoc().Build();
    static CommandGroup Cmd = Member.Command;
    static Configer Cfg = Member.Configer;

    public static dynamic AsyncCall(CommandGroup businessCommand, string cmd, string group = null, object[] args = null, params UseEntry[] useObj)
    {
        var t = businessCommand.AsyncCall(cmd, args, null, useObj, group);
        t.AsTask().Wait();
        return t.Result;
    }

    static dynamic AsyncCall(string cmd, string group = null, object[] args = null, params UseEntry[] useObj) => AsyncCall(Cmd, cmd, group, args, useObj);

    [TestMethod]
    public void TestCfgInfo()
    {
        Assert.AreEqual(Cfg.Info.CommandGroupDefault, CommandGroupDefault.Group);
        Assert.AreEqual(Cfg.Info.Meta.Declaring, AttributeBase.MetaData.DeclaringType.Class);
        Assert.AreEqual(Cfg.Info.BusinessName, "Business");
    }

    [TestMethod]
    public void TestCfgResultType()
    {
        Assert.AreEqual(Cfg.ResultTypeDefinition, typeof(ResultObject<>).GetGenericTypeDefinition());
        Assert.AreEqual(typeof(IResult).IsAssignableFrom(Cfg.ResultTypeDefinition), true);
    }

    [TestMethod]
    public void TestCfgUseTypes()
    {
        Assert.AreEqual(Cfg.UseTypes.Count, 1);
        Assert.AreEqual(Cfg.UseTypes.ContainsKey(typeof(IToken).FullName), true);

        var meta = Cmd.GetCommand("Test001").Meta;
        Assert.AreEqual(meta.Args.First(c => c.Type == typeof(Token)).UseType, true);
    }

    [TestMethod]
    public void TestCfgDoc()
    {
        Assert.AreNotEqual(Cfg.Doc, null);
        Assert.AreEqual(Cfg.Doc.Group.Count, 3);
        Assert.AreEqual(Cfg.Doc.Group.ContainsKey(CommandGroupDefault.Group), true);
        Assert.AreEqual(Cfg.Doc.Group.ContainsKey("G01"), true);
        Assert.AreEqual(Cfg.Doc.Group.ContainsKey("G02"), true);

        Assert.AreEqual(Cfg.Doc.Group[CommandGroupDefault.Group].ContainsKey("Test001"), true);
        Assert.AreEqual(Cfg.Doc.Group["G01"].ContainsKey("G01Test001"), true);
        Assert.AreEqual(Cfg.Doc.Group["G01"].ContainsKey("G01Test002"), true);

        Assert.AreEqual(Cfg.Doc.Group[CommandGroupDefault.Group]["Test001"].Description, "This is Test001.");
        //Assert.AreEqual(Cfg.Doc.Group[CommandGroupDefault.Group]["Test001"].Args["use01"].Description, "This is use01.");
        Assert.IsTrue(Cfg.Doc.Group[CommandGroupDefault.Group]["Test001"].Args.Children["arg01"].Description.StartsWith("This is Arg01."));
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

        var json = Cfg.Attributes.FirstOrDefault(c => c.Meta.Type == typeof(JsonArgAttribute));
        Assert.AreNotEqual(json, null);
        Assert.AreEqual(json.Meta.Declaring, AttributeBase.MetaData.DeclaringType.Assembly);

        var logger = Cfg.Attributes.FirstOrDefault(c => c.Meta.Type == typeof(LoggerAttribute));
        Assert.AreNotEqual(logger, null);
        Assert.AreEqual(logger.Meta.Declaring, AttributeBase.MetaData.DeclaringType.Assembly);

        var command = Cfg.Attributes.FirstOrDefault(c => c.Meta.Type == typeof(CommandAttribute));
        Assert.AreNotEqual(command, null);
        Assert.AreEqual(command.Meta.Declaring, AttributeBase.MetaData.DeclaringType.Assembly);

        var info = Cfg.Attributes.FirstOrDefault(c => c.Meta.Type == typeof(Info));
        Assert.AreNotEqual(info, null);
        Assert.AreEqual(info.Meta.Declaring, AttributeBase.MetaData.DeclaringType.Class);
    }

    [TestMethod]
    public void TestCmd()
    {
        Assert.AreEqual(Cmd.Count, 3);
        Assert.AreEqual(Cmd.ContainsKey(CommandGroupDefault.Group), true);
        Assert.AreEqual(Cmd.ContainsKey("G01"), true);
        Assert.AreEqual(Cmd.ContainsKey("G02"), true);

        Assert.AreEqual(Cmd[CommandGroupDefault.Group].Values.Any(c => c.key == $"{CommandGroupDefault.Group}.Test001"), true);
        Assert.AreEqual(Cmd[CommandGroupDefault.Group].Values.Any(c => c.key == $"{CommandGroupDefault.Group}.DEFTest001"), true);
        Assert.AreEqual(Cmd[CommandGroupDefault.Group].Values.Any(c => c.key == $"{CommandGroupDefault.Group}.Test002"), true);

        Assert.AreEqual(Cmd["G01"].Values.Any(c => c.key == "G01.G01Test001"), true);
        Assert.AreEqual(Cmd["G01"].Values.Any(c => c.key == "G01.G01Test002"), true);
        Assert.AreEqual(Cmd["G01"].Values.Any(c => c.key == "G01.Test002"), true);

        Assert.AreEqual(Cmd["G02"].Values.Any(c => c.key == "G02.G02Test002"), true);
    }

    [TestMethod]
    public void TestAttrSort()
    {
        var meta = Cmd.GetCommand("Test001").Meta;

        var i = 0;
        var attr = meta.Args[1].Children[0].Group[meta.GroupDefault].Attrs[i];

        Assert.AreEqual(attr.State, -113);
        i = ++i;
        attr = meta.Args[1].Children[0].Group[meta.GroupDefault].Attrs[i];
        Assert.AreEqual(attr.State, -800);
        i = ++i;
        attr = meta.Args[1].Children[0].Group[meta.GroupDefault].Attrs[i];
        Assert.AreEqual(attr.State, -821);
    }

    [TestMethod]
    public void TestArgNick()
    {
        var meta = Cmd.GetCommand("Test001").Meta;

        var attr = meta.Args[2].Group[meta.GroupDefault].Attrs.First();
        Assert.AreEqual(attr.Alias, "arg.b");

        attr = meta.Args[3].Group[meta.GroupDefault].Attrs.First();
        Assert.AreEqual(attr.Alias, "arg.c");

        attr = meta.Args[3].Group["G01.G01Test001"].Attrs.First();
        Assert.AreEqual(attr.Alias, "G01arg.c");

        attr = meta.Args[3].Group["G01.G01Test002"].Attrs.First();
        Assert.AreEqual(attr.Alias, "G01arg.c");
    }

    [TestMethod]
    public void TestLoggerAndArg()
    {
        var member = Bootstrap.Create<BusinessLoggerAndArg>().UseDoc().Build();

        Help.LoggerSet(member, new LoggerAttribute(Logger.Type.Record, canWrite: false) { Group = "333" }, typeof(Use02));

        var t2 = AsyncCall(member.Command, "TestLoggerAndArg", null, new object[] {
            new Arg01 { A = "abc" } },
            new UseEntry(new Use01 { A = "bbb" }, "use02"),
            new UseEntry(new Use01 { A = "aaa" }));
        Assert.AreEqual(t2.Message, null);
        Assert.AreEqual(t2.State, 1);
        Assert.AreEqual(t2.HasData, true);

        Task.WaitAll();
    }

    static dynamic Checked(string cmd, int state, string message, object[] args, params UseEntry[] useObj)
    {
        var result = AsyncCall(Member.Command, cmd, null, (0 < args?.Length ? new object[] { TestMode.ERROR }.Concat(args) : new object[] { TestMode.ERROR }).ToArray(), useObj);
        Assert.AreEqual(result.State, -999);
        Assert.AreEqual(result.Message, "test error");

        result = AsyncCall(Member.Command, cmd, null, (0 < args?.Length ? new object[] { TestMode.Exception }.Concat(args) : new object[] { TestMode.Exception }).ToArray(), useObj);
        Assert.AreEqual(result.State, 0);
        Assert.IsTrue(result.Message.Contains("System.Exception: exception"));

        result = AsyncCall(Member.Command, cmd, null, (0 < args?.Length ? new object[] { TestMode.OK }.Concat(args) : new object[] { TestMode.OK }).ToArray(), useObj);
        Assert.AreEqual(result.State, state);
        Assert.AreEqual(result.Message, message);

        return result.Data;
    }
    static dynamic CheckedNot(string cmd, int state, string message, object[] args, params UseEntry[] useObj)
    {
        var result = AsyncCall(Member.Command, cmd, null, (0 < args?.Length ? new object[] { TestMode.ERROR }.Concat(args) : new object[] { TestMode.ERROR }).ToArray(), useObj);

        result = AsyncCall(Member.Command, cmd, null, (0 < args?.Length ? new object[] { TestMode.Exception }.Concat(args) : new object[] { TestMode.Exception }).ToArray(), useObj);

        result = AsyncCall(Member.Command, cmd, null, (0 < args?.Length ? new object[] { TestMode.OK }.Concat(args) : new object[] { TestMode.OK }).ToArray(), useObj);

        return result;
    }

    void TestResult(IBusiness business)
    {
        //var t0 = AsyncCall(business.Command, "Test000", null, new object[] { new Arg00 { A = "abc" }, 2, 2 });
        //Assert.AreEqual(t0.State, 1);
        //Assert.AreEqual(t0.Message, null);
        //Assert.AreEqual(t0.HasData, true);

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
            new UseEntry(new Token { Key = "a", Remote = new Remote("b") }));

        //var t444 = business.Command.AsyncCallFull("G01Test001", new { arg01 = new Arg01 { A = "abc" }, b = 2, c = 2 }.JsonSerialize(), new UseEntry[] { new UseEntry(new Token { Key = "a", Remote = new Remote("b") }) }, "G01");
        //t444.AsTask().Wait();
        //var t4 = t444.Result;

        Assert.AreEqual(t4.Message, null);
        Assert.AreEqual(t4.State, t2.State);
        //Assert.AreEqual(t4.HasData, false);

        var t5 = AsyncCall(business.Command, "Test002");
        Assert.AreEqual(t5.Message, null);
        Assert.AreEqual(t5.State, 200);

        var t6 = AsyncCall(business.Command, "Test003");
        Assert.AreEqual(t6.Message, null);
        Assert.AreEqual(t6.State, -200);

        var t7 = AsyncCall(business.Command, "Test004");
        Assert.AreEqual(t7.Message, null);
        Assert.AreEqual(t7.Data.a, "aaa");

        //var t77 = AsyncCall(business.Command, "Test0044");
        //Assert.AreEqual(t77, null);


        var t8 = Checked("Test005", 1, null, null);
        Assert.AreEqual(t8.a, "aaa");
        t8 = Checked("Test005a", 1, null, null);
        Assert.AreEqual(t8.a, "aaa");
        t8 = Checked("Test005b", 1, null, null);
        Assert.AreEqual(t8.a, "aaa");

        t8 = Checked("Test006", 1, null, null);
        Assert.AreEqual(t8, "aaa");
        t8 = Checked("Test006a", 1, null, null);
        Assert.AreEqual(t8, "aaa");
        t8 = Checked("Test006b", 1, null, null);
        Assert.AreEqual(t8, "aaa");

        t8 = Checked("Test007", 1, null, null);
        Assert.AreEqual(t8, -200m);
        t8 = Checked("Test007a", 1, null, null);
        Assert.AreEqual(t8, -200m);
        t8 = Checked("Test007b", 1, null, null);
        Assert.AreEqual(t8, -200m);

        t8 = Checked("Test008", 1, null, null);
        Assert.AreEqual(t8, -200);
        t8 = Checked("Test008a", 1, null, null);
        Assert.AreEqual(t8, -200);
        t8 = Checked("Test008b", 1, null, null);
        Assert.AreEqual(t8, -200);

        t8 = Checked("Test009", 111, "111", null);
        Assert.AreEqual(t8, null);
        t8 = Checked("Test009a", 111, "111", null);
        Assert.AreEqual(t8, null);
        t8 = Checked("Test009b", 111, "111", null);
        Assert.AreEqual(t8, null);

        t8 = Checked("Test010", 1, null, null);
        Assert.AreEqual(t8, null);
        t8 = Checked("Test010a", 1, null, null);
        Assert.AreEqual(t8, null);
        t8 = Checked("Test010b", 1, null, null);
        Assert.AreEqual(t8, null);

        t8 = Checked("Test011", 1, null, null);
        Assert.AreEqual(t8, null);
        t8 = Checked("Test011a", 1, null, null);
        Assert.AreEqual(t8, null);
        t8 = Checked("Test011b", 1, null, null);
        Assert.AreEqual(t8, null);

        t8 = Checked("Test012", 1, null, null);
        Assert.AreEqual(t8.Date, DateTime.Now.Date);
        t8 = Checked("Test012a", 1, null, null);
        Assert.AreEqual(t8.Date, DateTime.Now.Date);
        t8 = Checked("Test012b", 1, null, null);
        Assert.AreEqual(t8.Date, DateTime.Now.Date);

        t8 = Checked("Test013", 1, null, null);
        Assert.AreEqual(t8, null);
        t8 = Checked("Test013a", 1, null, null);
        Assert.AreEqual(t8, null);
        t8 = Checked("Test013b", 1, null, null);
        Assert.AreEqual(t8, null);

        t8 = CheckedNot("Test013c", 1, null, null);
        Assert.AreEqual(t8.Data.Date, DateTime.Now.Date);
        t8 = CheckedNot("Test013d", 1, null, null);
        Assert.AreEqual(t8.Date, DateTime.Now.Date);
        t8 = CheckedNot("Test013e", 1, null, null);
        Assert.AreEqual(t8.Date, DateTime.Now.Date);
        t8 = CheckedNot("Test013f", 1, null, null);
        Assert.AreEqual(System.DateTime.Parse(t8).Date, DateTime.Now.Date);

        t8 = CheckedNot("Test014", 1, null, null);
        Assert.AreEqual(t8, null);
        //t8 = CheckedNot("Test014a", 1, null, null);
        //Assert.AreEqual(t8, null);
        t8 = CheckedNot("Test014b", 1, null, null);
        Assert.AreEqual(t8, null);

        t8 = Checked("TestUse01", 1, null, null, new UseEntry("sss", "use01"));
        Assert.AreEqual(t8, "sss");
        t8 = Checked("TestUse01a", 1, null, null, new UseEntry("sss", "use01"));
        Assert.AreEqual(t8, "sss");
        t8 = Checked("TestUse01b", 1, null, null, new UseEntry("sss", "use01"));
        Assert.AreEqual(t8, "sss");

        var token = new Token { Key = "a", Remote = new Remote("b") };
        t8 = Checked("TestUse02", 1, null, null, new UseEntry(token));
        Assert.AreEqual(t8, token);
        t8 = Checked("TestUse02a", 1, null, null, new UseEntry(token));
        Assert.AreEqual(t8, token);
        t8 = Checked("TestUse02b", 1, null, null, new UseEntry(token));
        Assert.AreEqual(t8, token);

        t8 = Checked("TestUse03", 1, null, new object[] { "abc" }, new UseEntry("sss", "use01"));
        Assert.AreEqual(t8, "abcsss");
        t8 = Checked("TestUse03a", 1, null, new object[] { "abc" }, new UseEntry("sss", "use01"));
        Assert.AreEqual(t8, "abcsss");
        t8 = Checked("TestUse03b", 1, null, new object[] { "abc" }, new UseEntry("sss", "use01"));
        Assert.AreEqual(t8, "abcsss");

        t8 = Checked("TestAnonymous", 1, null, new object[] { "abc" }, new UseEntry("sss", "use01"));
        Assert.AreEqual(t8.a, "abc");
        Assert.AreEqual(t8.b, "sss");
        t8 = Checked("TestAnonymousa", 1, null, new object[] { "abc" }, new UseEntry("sss", "use01"));
        Assert.AreEqual(t8.a, "abc");
        Assert.AreEqual(t8.b, "sss");
        t8 = Checked("TestAnonymousb", 1, null, new object[] { "abc" }, new UseEntry("sss", "use01"));
        Assert.AreEqual(t8.a, "abc");
        Assert.AreEqual(t8.b, "sss");


        var t20 = AsyncCall(business.Command, "TestAnonymous2", null, new object[] { "abc" }, new UseEntry("sss", "use01"));
        Assert.AreEqual(t20.a, "abc");
        Assert.AreEqual(t20.b, "sss");

        var t21result = new Token { Key = "a", Remote = new Remote("b") };
        var t21 = AsyncCall(business.Command, "TestDynamic", null, new object[] { "abc" }, new UseEntry(t21result, "use01"));
        Assert.AreEqual(t21, t21result);

        var b2 = Member2.Test001(null, new Arg01 { A = "abc" });
        b2.AsTask().Wait();
        Assert.AreEqual(typeof(IResult).IsAssignableFrom(b2.Result.GetType()), true);
        Assert.AreEqual(b2.Result.State, -113);
        Assert.AreEqual(b2.Result.Message, "arg.b minimum range 2");

        var t00 = Member2.Test000(null, new Arg00 { A = "abc" });
        t00.AsTask().Wait();
        Assert.AreEqual(typeof(IResult).IsAssignableFrom(t00.Result.GetType()), true);
        Assert.AreEqual(t00.Result.State, -113);
        Assert.AreEqual(t00.Result.Message, "arg.b minimum range 2");
    }

    [TestMethod]
    public void TestResult01()
    {
        //var t00 = Member.Test000(null, new Arg00 { A = "abc" });
        //t00.AsTask().Wait();
        //Assert.AreEqual(typeof(IResult).IsAssignableFrom(t00.Result.GetType()), true);
        //Assert.AreEqual(t00.Result.State, -113);
        //Assert.AreEqual(t00.Result.Message, "arg.b minimum range 2");

        var t000 = AsyncCall(Member.Command, "Test000", null, new object[] { new Arg00 { A = "abc" }, 2, 2 });
        Assert.AreEqual(t000.State, 1);
        Assert.AreEqual(t000.Message, null);
        Assert.AreEqual(t000.HasData, true);

        var t0 = Member.Test001(null, new Arg01 { A = "abc" });
        t0.AsTask().Wait();
        Assert.AreEqual(typeof(IResult).IsAssignableFrom(t0.Result.GetType()), true);
        Assert.AreEqual(t0.Result.State, -113);
        Assert.AreEqual(t0.Result.Message, "arg.b minimum range 2");

        var t1 = Member.Test001(null, new Arg01 { A = "abc" }, 2, 2);
        t1.AsTask().Wait();
        Assert.AreEqual(t1.Result.State, 1);
        Assert.AreEqual(t1.Result.HasData, true);

        //Cmd
        var t2 = Cmd.AsyncCall("Test001", new object[] { new Arg01 { A = "abc" } });
        t2.AsTask().Wait();
        Assert.AreEqual(typeof(IResult).IsAssignableFrom(t2.Result.GetType()), true);
        Assert.AreEqual(t2.Result.State, -113);
        Assert.AreEqual(t2.Result.Message, "arg.b minimum range 2");

        var t3 = Cmd.AsyncCall("Test001", new object[] { new Arg01 { A = "abc" }, 2, 2 });
        t3.AsTask().Wait();
        Assert.AreEqual(t3.Result.State, 1);
        Assert.AreEqual(t3.Result.HasData, true);

        TestResult(Member);
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
        t2.AsTask().Wait();
        Assert.AreEqual(typeof(IResult).IsAssignableFrom(t2.Result.GetType()), true);
        Assert.AreEqual(t2.Result.State, -113);
        Assert.AreEqual(t2.Result.Message, "arg.b minimum range 2");

        var t3 = Cmd.AsyncCall("Test0001", new object[] { new Arg01 { A = "abc" }, 2, 2 });
        t3.AsTask().Wait();
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
        t2.AsTask().Wait();
        Assert.AreEqual(typeof(IResult).IsAssignableFrom(t2.Result.GetType()), true);
        Assert.AreEqual(t2.Result.State, -113);
        Assert.AreEqual(t2.Result.Message, "arg.b minimum range 2");

        var t3 = Cmd.AsyncCall("Test00001", new object[] { new Arg01 { A = "abc" }, 2, 2 });
        t3.AsTask().Wait();
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
        t0.AsTask().Wait();

        var t1 = Member.Test0011(null, new Arg01 { A = "abc" }, 2, 2);
        t1.AsTask().Wait();

        //Cmd
        var t2 = Cmd.AsyncCall("Test0011", new object[] { new Arg01 { A = "abc" } });
        t2.AsTask().Wait();
        Assert.IsNull(t2.Result);

        var t3 = Cmd.AsyncCall("Test0011", new object[] { new Arg01 { A = "abc" }, 2, 2 });
        t3.AsTask().Wait();
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
        t2.AsTask().Wait();
        Assert.IsNull(t2.Result);

        var t3 = Cmd.AsyncCall("Test0012", new object[] { new Arg01 { A = "abc" }, 2, 2 });
        t3.AsTask().Wait();
        Assert.IsNull(t3.Result);
    }

    [TestMethod]
    public void TestResult0122()
    {
        Member.Test00122(null, new Arg01 { A = "abc" });

        Member.Test00122(null, new Arg01 { A = "abc" }, 2, 2);

        Member.Test00122(null, new Arg01 { A = "abc", B = "ex" }, 2, 2);

        //Cmd
        var t2 = Cmd.AsyncCall("Test00122", new object[] { new Arg01 { A = "abc" } });
        t2.AsTask().Wait();
        Assert.IsNull(t2.Result);

        var t3 = Cmd.AsyncCall("Test00122", new object[] { new Arg01 { A = "abc" }, 2, 2 });
        t3.AsTask().Wait();
        Assert.IsNull(t3.Result);
    }

    [TestMethod]
    public void TestResult01222()
    {
        var d = Member.Test001222(null, new Arg01 { A = "abc" });

        Assert.AreEqual(d.Result, 0);

        d = Member.Test001222(null, new Arg01 { A = "abc" }, 2, 2);

        d = Member.Test001222(null, new Arg01 { A = "abc", B = "ex" }, 2, 2);

        //Cmd
        var t2 = Cmd.AsyncCall("Test001222", new object[] { new Arg01 { A = "abc" } });
        t2.AsTask().Wait();
        Assert.AreEqual(t2.Result, 0);

        var t3 = Cmd.AsyncCall("Test001222", new object[] { new Arg01 { A = "abc" }, 2, 2 });
        t3.AsTask().Wait();
        Assert.AreEqual(t3.Result, 333);
    }

    [TestMethod]
    public void TestResult01223()
    {
        var d = Member.Test001223(null, new Arg01 { A = "abc" });

        Assert.AreEqual(d.Result, null);

        d = Member.Test001223(null, new Arg01 { A = "abc" }, 2, 2);

        d = Member.Test001223(null, new Arg01 { A = "abc", B = "ex" }, 2, 2);

        //Cmd
        var t2 = Cmd.AsyncCall("Test001223", new object[] { new Arg01 { A = "abc" } });
        t2.AsTask().Wait();
        Assert.AreEqual(t2.Result, null);

        var t3 = Cmd.AsyncCall("Test001223", new object[] { new Arg01 { A = "abc" }, 2, 2 });
        t3.AsTask().Wait();
        Assert.AreEqual(t3.Result, "333");
    }

    [TestMethod]
    public void TestResult0122x()
    {
        Member.Test00122x(null, new Arg01 { A = "abc" });

        Member.Test00122x(null, new Arg01 { A = "abc" }, 2, 2);

        Member.Test00122x(null, new Arg01 { A = "abc", B = "ex" }, 2, 2);

        //Cmd
        var t2 = Cmd.AsyncCall("Test00122x", new object[] { new Arg01 { A = "abc" } });
        t2.AsTask().Wait();
        Assert.IsNull(t2.Result);

        var t3 = Cmd.AsyncCall("Test00122x", new object[] { new Arg01 { A = "abc" }, 2, 2 });
        t3.AsTask().Wait();
        Assert.IsNull(t3.Result);
    }

    [TestMethod]
    public void TestResult01222x()
    {
        var d = Member.Test001222x(null, new Arg01 { A = "abc" });

        Assert.AreEqual(d, 0);

        d = Member.Test001222x(null, new Arg01 { A = "abc" }, 2, 2);

        d = Member.Test001222x(null, new Arg01 { A = "abc", B = "ex" }, 2, 2);

        //Cmd
        var t2 = Cmd.AsyncCall("Test001222x", new object[] { new Arg01 { A = "abc" } });
        t2.AsTask().Wait();
        Assert.AreEqual(t2.Result, 0);

        var t3 = Cmd.AsyncCall("Test001222x", new object[] { new Arg01 { A = "abc" }, 2, 2 });
        t3.AsTask().Wait();
        Assert.AreEqual(t3.Result, -333);
    }

    [TestMethod]
    public void TestResult01223x()
    {
        var d = Member.Test001223x(null, new Arg01 { A = "abc" });

        Assert.AreEqual(d, null);

        d = Member.Test001223x(null, new Arg01 { A = "abc" }, 2, 2);

        d = Member.Test001223x(null, new Arg01 { A = "abc", B = "ex" }, 2, 2);

        //Cmd
        var t2 = Cmd.AsyncCall("Test001223x", new object[] { new Arg01 { A = "abc" } });
        t2.AsTask().Wait();
        Assert.AreEqual(t2.Result, null);

        var t3 = Cmd.AsyncCall("Test001223x", new object[] { new Arg01 { A = "abc" }, 2, 2 });
        t3.AsTask().Wait();
        Assert.AreEqual(t3.Result, "333");
    }

    /*
    [TestMethod]
    public void TestCollection()
    {
        var list3 = new List<TestCollectionArg> { new TestCollectionArg { A = 200000 } };

        var t22 = AsyncCall(Member.Command, "TestCollection", null, new object[] { list3 });

        //TestCollectionArg2 checknull 1003
        Assert.AreEqual(t22.State, -1103);

        list3[0].B = new List<TestCollectionArg.TestCollectionArg2> { new TestCollectionArg.TestCollectionArg2 { C = "sss", D = 888 }, new TestCollectionArg.TestCollectionArg2 { C = "sss2", D = 999 } };

        var t23 = AsyncCall(Member.Command, "TestCollection", null, new object[] { list3 });

        Assert.AreEqual(t23.State, -1106);

        //Assert.AreEqual(list3[0].A, 370);

        list3[0].B[1] = default;

        var t24 = AsyncCall(Member.Command, "TestCollection", null, new object[] { list3 });

        Assert.IsTrue(t24.State == -1103);

        list3[0] = null;

        var t25 = AsyncCall(Member.Command, "TestCollection", null, new object[] { list3 });

        Assert.AreEqual(t25.State, -1101);
    }

    [TestMethod]
    public void TestDict()
    {
        var list3 = new Dictionary<string, TestDictArg> { { "a", new TestDictArg { A = 200000 } } };

        var t22 = AsyncCall(Member.Command, "TestDict", null, new object[] { list3 });

        Assert.AreEqual(t22.State, -1103);

        list3["a"].B = new Dictionary<string, TestDictArg.TestDictArg2> { { "b", new TestDictArg.TestDictArg2 { C = "sss", D = 888 } }, { "c", new TestDictArg.TestDictArg2 { C = "sss2", D = 999 } } };

        var t23 = AsyncCall(Member.Command, "TestDict", null, new object[] { list3 });

        Assert.AreEqual(t23.State, -1106);

        //Assert.AreEqual(list3[0].A, 370);

        list3["a"].B["c"] = default;

        var t24 = AsyncCall(Member.Command, "TestDict", null, new object[] { list3 });

        Assert.IsTrue(t24.State == -1103);

        list3["a"] = null;

        var t25 = AsyncCall(Member.Command, "TestDict", null, new object[] { list3 });

        Assert.AreEqual(t25.State, -1101);
    }

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
    public void TestDict2()
    {
        var list2 = new Dictionary<string, TestDictArg.TestDictArg2>();

        for (int i = 0; i < 50; i++)
        {
            list2.Add($"{i}", new TestDictArg.TestDictArg2 { C = $"{i}", D = i });
        }

        var t22 = AsyncCall(Member.Command, "TestDict", null, new object[] {
             new Dictionary<string,TestDictArg> { { "a", new TestDictArg { A = 1, B = list2 } } }
            });

        Assert.AreEqual(t22.State, 1);//?? State == 0 ?? Collection was modified; enumeration operation may not execute.

        //Assert.AreEqual(list2["0"].D, 1);
        //Assert.AreEqual(list2["1"].D, 2);
        //Assert.AreEqual(list2["2"].D, 3);
        //Assert.AreEqual(list2["3"].D, 4);
        //Assert.AreEqual(list2["4"].D, 5);
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

        IResult t22 = AsyncCall(Member.Command, "TestCollection", null, new object[] { list2 });

        Assert.AreEqual(t22.State, 1);

        for (int i = 0; i < list2.Count; i++)
        {
            Assert.AreEqual(list2[i].A, list3[i].A + 2);
        }
    }

    [TestMethod]
    public void TestDict3()
    {
        var list2 = new Dictionary<string, TestDictArg>();

        for (int i = 0; i < 5; i++)
        {
            var b = new Dictionary<string, TestDictArg.TestDictArg2> { { $"{i}", new TestDictArg.TestDictArg2 { C = $"{i}", D = i } } };

            list2.Add($"{i}", new TestDictArg { A = i, B = b, C = new TestDictArg.TestCollectionArg3 { E = "e", F = "f" } });
        }

        var list3 = Force.DeepCloner.DeepClonerExtensions.DeepClone(list2);

        IResult t22 = AsyncCall(Member.Command, "TestDict", null, new object[] { list2 });

        Assert.AreEqual(t22.State, 1);

        for (int i = 0; i < list2.Count; i++)
        {
            Assert.AreEqual(list2[$"{i}"].A, list3[$"{i}"].A + 2);
        }
    }

        [TestMethod]
    public void TestCollectionException()
    {
        var list2 = new List<TestExceptionArg> { new TestExceptionArg { B = new List<TestExceptionArg.TestHasLower2> { new TestExceptionArg.TestHasLower2 { C = new List<TestExceptionArg.TestHasLower3> { new TestExceptionArg.TestHasLower3 { E = "EEE" }, new TestExceptionArg.TestHasLower3 { E = "EEE", F = "F1" }, new TestExceptionArg.TestHasLower3 { E = "EEE", F = "F2" } }, D = 99 } } } };

        IResult r = AsyncCall(Member.Command, "TestCollectionException", null, new object[] { list2 });

        Assert.AreEqual(r.State, 0);
        Assert.AreEqual(r.Message.Contains("Attribute Proces exception! 1"), true);
    }
    */
    [TestMethod]
    public void TestHasLower()
    {
        var list2 = new List<TestHasLowerArg> { new TestHasLowerArg { B = new List<TestHasLowerArg.TestHasLower2> { new TestHasLowerArg.TestHasLower2 { C = new List<TestHasLowerArg.TestHasLower3> { new TestHasLowerArg.TestHasLower3 { E = "EEE" } }, D = 99 } } } };

        var r = AsyncCall(Member.Command, "TestHasLower", null, new object[] { list2 });

        Assert.AreEqual(r.State, -1200);//checked Collection -1100

        list2[0].B[0].C[0].F = "FFF";
        r = AsyncCall(Member.Command, "TestHasLower", null, new object[] { list2 });

        Assert.AreEqual(r.State, -1200);

        Assert.AreEqual(Member.Configer.MetaData["TestHasLower"].Args[0].HasLower, true);
        Assert.AreEqual(Member.Configer.MetaData["TestHasLower"].Args[0].Children[0].Children[0].Children[0].HasLower, false);
        Assert.AreEqual(Member.Configer.MetaData["TestHasLower"].Args[0].Children[0].Children[0].Children[1].HasLower, true);

        Assert.AreEqual(Member.Configer.MetaData["TestHasLower"].Args[1].HasLower, true);
        Assert.AreEqual(Member.Configer.MetaData["TestHasLower"].Args[2].HasLower, false);
    }

    [TestMethod]
    public void TestHasLower2()
    {
        var list2 = new TestHasLowerArg2 { B = new TestHasLowerArg2.TestHasLower2 { C = new TestHasLowerArg2.TestHasLower3 { E = "EEE" }, D = 99 } };

        var r = AsyncCall(Member.Command, "TestHasLower2", null, new object[] { list2 });

        Assert.AreEqual(r.State, -1100);

        list2.B.C.F = "FFF";
        r = AsyncCall(Member.Command, "TestHasLower2", null, new object[] { list2 });

        Assert.AreEqual(r.State, -1200);

        Assert.AreEqual(Member.Configer.MetaData["TestHasLower2"].Args[0].HasLower, true);
        Assert.AreEqual(Member.Configer.MetaData["TestHasLower2"].Args[0].Children[0].Children[0].Children[0].HasLower, false);
        Assert.AreEqual(Member.Configer.MetaData["TestHasLower2"].Args[0].Children[0].Children[0].Children[1].HasLower, true);

        Assert.AreEqual(Member.Configer.MetaData["TestHasLower2"].Args[1].HasLower, true);
        Assert.AreEqual(Member.Configer.MetaData["TestHasLower2"].Args[2].HasLower, false);
    }

    [TestMethod]
    public void TestException()
    {
        var arg = new TestExceptionArg { B = new TestExceptionArg.TestHasLower2 { C = new TestExceptionArg.TestHasLower3 { E = "EEE", F = "666" }, D = 99 } };

        IResult r = AsyncCall(Member.Command, "TestException", null, new object[] { arg });

        Assert.AreEqual(r.State, 0);
        Assert.AreEqual(r.Message.Contains("Attribute Proces exception!"), true);
    }

    [TestMethod]
    public void TestAttrs()
    {
        var attrs = Member.Configer.MetaData["Test0011"].Attributes;

        foreach (var item in attrs)
        {
            var attr = item.Value.Clone();
        }
        //Member.Configer.Info.CommandGroupDefault

        //Assert.AreEqual(r.State, 0);
        //Assert.AreEqual(r.Message.Contains("Attribute Proces exception! 1"), true);
    }
}
