using Business.Core.Annotations;
using Business.Core.Result;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Args;

[Info("API/v2", CommandGroupDefault = null)]
public class BusinessMember3 : BusinessBase
{
    public BusinessMember3()
    {
        this.BindBefore = c =>
        {
            c.CallBeforeMethod = async method =>
            {
                //method.Cancel = true;
            };

            c.CallAfterMethod = async method =>
            {
                if (typeof(IAsyncResult).IsAssignableFrom(method.Result?.GetType()))
                {
                    //await result;

                    //await System.Threading.Tasks.Task.Run(() =>
                    //{
                    //    System.Threading.Thread.Sleep(3000);
                    //});

                    //result2.State = 111;
                }
            };
        };
    }

    [Testing("test2", "\"2019-12-02T21:02\"")]
    [Testing("test3", "\"2019-12-02T22:02\"")]
    [Testing("test4", "\"2019-12-02T23:02\"")]
    [Testing("test3333", "\"2019-12-02T23:02\"")]
    public virtual async Task<DateTime?> Test000(Session session, [Ignore(IgnoreMode.BusinessArg)]DateTime? date, dynamic context = null)
    {
        return date;
        //return this.ResultCreate(date);
    }

    public struct Test001
    {
        public string A { get; set; }

        public string B { get; set; }
    }

    public virtual async Task<dynamic> Test004(Session session, List<Test001> arg, dynamic context, [Ignore(IgnoreMode.BusinessArg)][Test2]decimal mm = 0.0234m)
    {
        Microsoft.AspNetCore.Http.HttpContext httpContext = context.HttpContext;

        //await socket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, null, CancellationToken.None);
        //return this.ResultCreate();
        return this.ResultCreate(new { session, arg = arg, State = httpContext.Connection.RemoteIpAddress.ToString() });
        //return this.ResultCreate(new { token, arg = arg?.Out, State = context.WebSocket.State });
    }
}

[Testing("test2222", "\"2019-12-02T21:02\"", tokenMethod: login, Method = "Test000")]
[Testing("test2222", "\"2019-12-02T22:02\"", tokenMethod: login, Method = "Test000")]
[Testing("test3333", "\"2019-12-02T23:02\"", tokenMethod: login, Method = "Test000")]
[Testing("test2222", "\"2019-12-02T21:02\"", tokenMethod: login, Method = "Test000")]
[Testing("test2222", "\"2019-12-02T22:02\"", tokenMethod: login, Method = "Test000")]
[Testing("test3333", "\"2019-12-02T23:02\"", tokenMethod: login, Method = "Test000")]
public partial class BusinessMember2
{
    public BusinessMember2()
    {
        this.BindBefore = c =>
        {
            c.CallBeforeMethod = async method =>
            {
                method.Cancel = false;
            };

            c.CallAfterMethod = async method =>
            {
                if (typeof(IAsyncResult).IsAssignableFrom(method.Result?.GetType()))
                {
                    //await result;

                    //await System.Threading.Tasks.Task.Run(() =>
                    //{
                    //    System.Threading.Thread.Sleep(3000);
                    //});

                    //result2.State = 111;
                }
            };
        };
    }
}

/// <summary>
/// BusinessMember2
/// businessDescription
/// businessDescription3
/// </summary>
[Info("API", CommandGroupDefault = null)]
public partial class BusinessMember2 : BusinessBase
{

    public struct Test001Result
    {
        /// <summary>
        /// aaa
        /// </summary>
        public string A { get; set; }

        /// <summary>
        /// bbb
        /// </summary>
        public string B { get; set; }
    }

    public virtual async Task<IResult<DateTime>> Test00X(Token token2, Test001 arg)
    {
        return this.ResultCreate();
    }

    [Testing("test2", "\"2019-12-02T21:02\"", tokenMethod: login)]
    [Testing("test3", "\"2019-12-02T22:02\"", tokenMethod: login)]
    [Testing("test4", "\"2019-12-02T23:02\"", tokenMethod: login)]
    [Testing("test3333", "\"2019-12-02T23:02\"", tokenMethod: login)]
    [Ignore(IgnoreMode.BusinessArg)]
    public virtual async Task<IResult<DateTime?>> Test000(Session session, [Ignore(IgnoreMode.BusinessArg)]DateTime? date)
    {
        return this.ResultCreate(date);
    }

    public const string login = "[\"Login\",\"{User:\\\"ddd\\\",Password:\\\"123456\\\"}\",\"D\"]";

    /// <summary>
    /// test doc Test001
    /// and Test001
    /// </summary>
    /// <param name="token">A token sample222</param>
    /// <param name="arg"></param>
    /// <param name="dateTime"></param>
    /// <param name="httpFile"></param>
    /// <param name="mm">mmmmmmmm!</param>
    /// <param name="fff"></param>
    /// <param name="bbb"></param>
    /// <returns>rrrrrr</returns>
    [Command("AAA")]
    [Command("jjj", Group = "j")]
    [Testing("test2",
        "[{\"AAA\":[],\"A\":\"http://127.0.0.1:5000/doc/index.html\",\"B\":\"\",\"C\":{\"C1\":\"\",\"C2\":\"\",\"C3\":[]},\"D\":0,\"E\":false,\"F\":\"2019-12-02T06:24\",\"myEnum\":\"C\"},\"2019-12-02T04:24\",99.0234,777,false]",
        "{\"AAA\":\"111\",\"BBB\":\"222\"}",
        tokenMethod: login)]
    [Testing("test3",
        "[{\"AAA\":[],\"A\":\"http://127.0.0.1:5000/doc/index.html\",\"B\":\"\",\"C\":{\"C1\":\"\",\"C2\":\"\",\"C3\":[]},\"D\":0,\"E\":false,\"F\":\"2019-12-02T06:24\",\"myEnum\":\"C\"},\"2019-12-02T05:24\",99.0234,777,false]",
        tokenMethod: login)]
    [Testing("test4",
        "[{\"AAA\":[],\"A\":\"http://127.0.0.1:5000/doc/index.html\",\"B\":\"\",\"C\":{\"C1\":\"\",\"C2\":\"\",\"C3\":[]},\"D\":0,\"E\":false,\"F\":\"2019-12-02T06:24\",\"myEnum\":\"C\"},\"2019-12-02T06:24\",99.0234,777,false]",
        tokenMethod: login)]
    [Testing("test5",
        "[{\"AAA\":[],\"A\":\"http://127.0.0.1:5000/doc/index.html\",\"B\":\"\",\"C\":{\"C1\":\"\",\"C2\":\"\",\"C3\":[]},\"D\":0,\"E\":false,\"F\":\"2019-12-02T06:24\",\"myEnum\":\"C\"},\"2019-12-02T07:24\",99.0234,777,false]",
        tokenMethod: login)]
    [Testing("test, important logic, do not delete!!!",
        "[{\"AAA\":[],\"A\":\"http://127.0.0.1:5000/doc/index.html\",\"B\":\"\",\"C\":{\"C1\":\"ok\",\"C2\":\"😀😭\",\"C3\":[]},\"D\":0,\"E\":false,\"F\":\"2019-12-02T06:24\",\"myEnum\":\"C\"},\"2019-12-02T08:24\",99.0234,777,false]",
        tokenMethod: login)]
    public virtual async Task<IResult<Test001Result?>> Test001(Token token, Arg<Test001> arg, Arg<DateTime?> dateTime, HttpFile httpFile = default, [Ignore(IgnoreMode.BusinessArg)][Test2]decimal mm = 0.0234m, [Ignore(IgnoreMode.BusinessArg)]int fff = 666, [Ignore(IgnoreMode.BusinessArg)]bool bbb = true)
    {
        dynamic args = new System.Dynamic.ExpandoObject();
        args.token = token;
        args.arg = arg.Out;
        if (args.arg.B == "ex")
        {
            throw new System.Exception("Method exception!");
        }

        if (args.arg.B == "ex2")
        {
            return this.ResultCreate(-911, "dsddsa");
        }

        //var exit = await RedisHelper.HExistsAsync("Role", "value2");

        //if (exit)
        //{
        //    //arg.Out.B = await RedisHelper.HGetAsync("Role", "value2");
        //    //return this.ResultCreate<Token>(null);

        //    return this.ResultCreate<Test001Result>(-110, "sss");
        //}

        //foreach (var item in context.Out)
        //{
        //    using (var m = item.Value.OpenReadStream())
        //    {
        //        var bytes = await Help.StreamReadByteAsync(m);

        //        System.IO.File.WriteAllBytes("testfile", bytes);
        //    }
        //}

        var files = httpFile.Select(c => new { key = c.Key, length = c.Value.Length }).ToList();

        //return this.ResultCreate(new { arg = arg.Out, files });
        Test001Result? ss = new Test001Result { A = args.arg.A, B = "SSS" };

        return this.ResultCreate(ss);
        //return ss;
    }

    public enum MyEnum
    {
        A,
        B,
        C
    }

    /// <summary>
    /// 222
    /// </summary>
    /// <param name="token">A token sample</param>
    /// <param name="arg"></param>
    /// <param name="mm"></param>
    /// <returns>test return!!!</returns>
    [HttpFile]
    public virtual async Task<dynamic> Test002(Token token, Test001 arg, [Ignore(IgnoreMode.BusinessArg)][Test2]decimal mm = 0.0234m, [HttpFile]Dictionary<string, dynamic> context = null)
    {
        dynamic args = new System.Dynamic.ExpandoObject();
        args.token = token;
        args.arg = arg;
        if (arg.B == "ex")
        {
            throw new System.Exception("Method exception!");
        }

        //return new Test001Result { A = "AAA", B = "BBB" };
        //return MyEnum.C;
        return "sss";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="token"></param>
    /// <param name="arg"></param>
    /// <param name="mm"></param>
    /// <returns></returns>
    public virtual async Task Test003(Token token, Test001 arg, [Ignore(IgnoreMode.BusinessArg)][Test2]decimal mm = 0.0234m)
    {
        //await System.Threading.Tasks.Task.Delay(10000);
        dynamic args = new System.Dynamic.ExpandoObject();
        args.token = token;
        args.arg = arg;
        if (arg.B == "ex")
        {
            throw new System.Exception("Method exception!");
        }
    }

    public virtual async Task<dynamic> Test004(Token token, List<Test001> arg, dynamic context, [Ignore(IgnoreMode.BusinessArg)][Test2]decimal mm = 0.0234m)
    {
        Microsoft.AspNetCore.Http.HttpContext httpContext = context.HttpContext;

        //await socket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, null, CancellationToken.None);
        //return this.ResultCreate();
        return this.ResultCreate(new { token, arg = arg, State = httpContext.Connection.RemoteIpAddress.ToString() });
        //return this.ResultCreate(new { token, arg = arg?.Out, State = context.WebSocket.State });
    }

    public virtual async Task<dynamic> Test005(Token token, List<Test001> arg, dynamic context)
    {
        Microsoft.AspNetCore.Http.HttpContext httpContext = context.HttpContext;

        //await socket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, null, CancellationToken.None);
        //return this.ResultCreate();
        return this.ResultCreate(new { token, arg = arg, State = httpContext.Connection.RemoteIpAddress.ToString() });
        //return this.ResultCreate(new { token, arg = arg?.Out, State = context.WebSocket.State });
    }

    public virtual async Task<dynamic> Test006(Token token) => this.ResultCreate();

    public virtual async Task<dynamic> Test0061() => this.ResultCreate();

    public virtual IResult<Test001Result?> Test0062([Ignore(IgnoreMode.BusinessArg)]string aaa)
    {
        Test001Result? ss = new Test001Result { A = aaa, B = "SSS" };

        return this.ResultCreate(ss);
    }
    public virtual IResult<Test001Result?> Test00621([Ignore(IgnoreMode.BusinessArg)]decimal? bbb, [Ignore(IgnoreMode.BusinessArg)]decimal aaa = 111.123m)
    {
        Test001Result? ss = new Test001Result { A = aaa.ToString(), B = bbb?.ToString() };

        return this.ResultCreate(ss);
    }

    public virtual IResult<Test001Result?> Test0063(Session session, [Ignore(IgnoreMode.BusinessArg)]string aaa)
    {
        Test001Result? ss = new Test001Result { A = aaa, B = "SSS" };

        return this.ResultCreate(ss);
    }

    public virtual async Task<IResult<Test001>> Test0010(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0011(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0012(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0013(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0014(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0015(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0016(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0017(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0018(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0019(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0020(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0021(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0022(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0023(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0024(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0025(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0026(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0027(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0028(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0029(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0030(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0031(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0032(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test00333(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0034(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0035(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0036(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0037(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0038(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0039(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0040(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0041(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0042(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0043(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0044(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0045(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0046(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0047(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0048(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0049(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0050(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0051(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0052(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0053(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0054(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001>> Test0055(Session session, Test001 arg)
    {
        return this.ResultCreate(arg);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="arg"></param>
    /// <param name="arg2">arg2!!!</param>
    /// <returns></returns>
    public virtual async Task<dynamic> Test007(Test001 arg, List<string> arg2) => this.ResultCreate();

    public virtual async void Test0033(Token token, Test001 arg, [Ignore(IgnoreMode.BusinessArg)][Test2]decimal mm = 0.0234m)
    {
        dynamic args = new System.Dynamic.ExpandoObject();
        args.token = token;
        args.arg = arg;
        if (arg.B == "ex")
        {
            throw new System.Exception("Method exception!");
        }
    }

    public struct Result
    {
        public Token token { get; set; }

        public List<Test001> arg { get; set; }
    }
}

public class TestAttribute : ArgumentAttribute
{
    public TestAttribute(int state = 111, string message = null) : base(state, message) { }

    public override async ValueTask<IResult> Proces(dynamic value)
    {
        //var exit = await RedisHelper.HExistsAsync("Role", "value2");

        switch (value)
        {
            case "ok":
                //return this.ResultCreate(exit ? await RedisHelper.HGetAsync("Role", "value2") : "not!");
                return this.ResultCreate("OK!!!");
            case "error":
                return this.ResultCreate(this.State, $"{this.Nick} cannot be empty");

            case "data":
                return this.ResultCreate(value + "1122");

            default: throw new System.Exception("Argument exception!");
        }
    }
}

public class Test2Attribute : ArgumentAttribute
{
    public Test2Attribute(int state = 112, string message = null) : base(state, message) { }

    public override async ValueTask<IResult> Proces(dynamic value)
    {
        return this.ResultCreate(value + 0.1m);
    }
}


public class Args
{
    /// <summary>
    /// MyEnumMyEnumMyEnumMyEnumMyEnumMyEnum
    /// </summary>
    public enum MyEnum
    {
        A = 0,
        B = 2,
        C = 4,
    }

    /// <summary>
    /// Test001Test001Tes
    /// t001Test001Test001Test001
    /// </summary>
    public class Test001
    {
        /// <summary>
        /// AAAAAAAAAaaaaaaaaaaaaaaaaaaaaaaaAAAAAAA
        /// </summary>
        //[Test]
        public List<string> AAA { get; set; }

        /// <summary>
        /// AAAAAAAAAaaaaaaaaaaaaaaaaaaaaaaaAAAAAAA
        /// </summary>
        //[Test]
        [Nick("password")]
        [@CheckNull]
        //[@CheckEmail]
        [@CheckUrl]
        public string A { get; set; }

        /// <summary>
        /// BBBBBBBBbbbbbbbbbbbbbbbbbBBBBBBBBBBBBBBBBBB
        /// </summary>
        public string B { get; set; }

        /// <summary>
        /// CccccccccccccccccccccccccccCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC12
        /// </summary>
        //[Test]
        public Test0010 C { get; set; }

        /// <summary>
        /// DDD
        /// </summary>
        public decimal? D { get; set; }

        public bool E { get; set; }

        /// <summary>
        /// FF
        /// </summary>
        public DateTime F { get; set; }

        public MyEnum myEnum { get; set; }

        public struct Test0010
        {
            /// <summary>
            /// C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1
            /// </summary>
            [Test]
            public string C1 { get; set; }

            /// <summary>
            /// C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2
            /// </summary>
            public string C2 { get; set; }

            /// <summary>
            /// C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3
            /// </summary>
            public List<Test0011> C3 { get; set; }

            //public string C22 { get; set; }

            //public Test0011 C33 { get; set; }

            //public Test0011 C34 { get; set; }

            //public string C35 { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public class Test0011
            {
                /// <summary>
                /// 
                /// </summary>
                public string C31 { get; set; }

                /// <summary>
                /// 
                /// </summary>
                public string C32 { get; set; }

                /// <summary>
                /// AAA@@@
                /// </summary>
                public List<string> AAA { get; set; }
            }
        }
    }
}