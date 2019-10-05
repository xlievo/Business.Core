﻿using Business;
using Business.Annotations;
using Business.Attributes;
using Business.Result;
using Business.Utils;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using static Args;

/// <summary>
/// BusinessMember2。。。
/// </summary>
[Info("API")]
public class BusinessMember2 : BusinessBase
{
    static BusinessMember2()
    {
        /*
        var con = Startup.appSettings.GetSection("Redis").GetSection("ConnectionString").Value;
        System.Console.WriteLine($"Redis={con}");
        var csredis = new CSRedis.CSRedisClient(con);
        RedisHelper.Initialization(csredis);

        RedisHelper.HSetAsync("Role", "value", "111");
        RedisHelper.HSetAsync("Role", "value2", "222");
        RedisHelper.HSetAsync("Role", "value3", "333");

        var values = RedisHelper.HGetAll("Role");
        */
    }

    public BusinessMember2(Host host) : base(host)
    {
        this.BindBefore = c =>
        {
            c.CallBeforeMethod = async (meta, args) =>
            {

            };

            c.CallAfterMethod = async (meta, args, result) =>
            {
                if (typeof(Task).IsAssignableFrom(result?.GetType()))
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

    public virtual async Task<IResult<Test001>> Test00X(Token token2, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }

    public virtual async Task<IResult<Test001>> Test000(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }

    /// <summary>
    /// test doc Test001
    /// and Test001
    /// </summary>
    /// <param name="token">A token sample222</param>
    /// <param name="arg"></param>
    /// <param name="mm">mmmmmmmm!</param>
    /// <returns></returns>
    [Command("AAA")]
    //Task<IResult<Test001>>
    public virtual async Task<dynamic> Test001(Token token, Arg<Test001> arg, [Ignore(IgnoreMode.BusinessArg)][Ignore(IgnoreMode.Arg)]Arg<DateTime> dateTime, HttpFile context = null, [Test2]decimal mm = 0.0234m, int fff = 666)
    {
        dynamic args = new System.Dynamic.ExpandoObject();
        args.token = token;
        args.arg = arg.Out;
        if (arg.Out.B == "ex")
        {
            throw new System.Exception("Method exception!");
        }

        //var exit = await RedisHelper.HExistsAsync("Role", "value2");

        //if (exit)
        //{
        //    //arg.Out.B = await RedisHelper.HGetAsync("Role", "value2");
        //    //return this.ResultCreate<Token>(null);

        //    return this.ResultCreate<Test001Result>(-110, "sss");
        //}

        var files = context.Out.Select(c => new { key = c.Key, length = c.Value.Length }).ToList();

        return this.ResultCreate(new { arg = arg.Out, files });
        //return this.ResultCreate(new Test001Result { A = "AAA", B = "SSS" });
    }

    static string GetFileName(string path)
    {
        var invalid = System.IO.Path.GetInvalidPathChars();

        foreach (var item in invalid)
        {
            if (path.Contains(item))
            {
                path = path.Replace(item, new char());
            }
        }

        return System.IO.Path.GetFileName(path);
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
    public virtual async Task<dynamic> Test002(Token token, Arg<Test001> arg, [Ignore(IgnoreMode.BusinessArg)][Test2]decimal mm = 0.0234m, Arg<Dictionary<string, dynamic>> context = null)
    {
        dynamic args = new System.Dynamic.ExpandoObject();
        args.token = token;
        args.arg = arg.Out;
        if (arg.Out.B == "ex")
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
    public virtual async Task Test003(Token token, Arg<Test001> arg, [Ignore(IgnoreMode.BusinessArg)][Test2]decimal mm = 0.0234m)
    {
        await System.Threading.Tasks.Task.Delay(10000);
        dynamic args = new System.Dynamic.ExpandoObject();
        args.token = token;
        args.arg = arg.Out;
        if (arg.Out.B == "ex")
        {
            throw new System.Exception("Method exception!");
        }
    }

    public virtual async Task<dynamic> Test004(Token token, Arg<List<Test001>> arg, dynamic context, [Ignore(IgnoreMode.BusinessArg)][Test2]decimal mm = 0.0234m)
    {
        Microsoft.AspNetCore.Http.HttpContext httpContext = context.HttpContext;

        //await socket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, null, CancellationToken.None);
        //return this.ResultCreate();
        return this.ResultCreate(new { token, arg = arg?.Out, State = httpContext.Connection.RemoteIpAddress.ToString() });
        //return this.ResultCreate(new { token, arg = arg?.Out, State = context.WebSocket.State });
    }

    public virtual async Task<dynamic> Test005(Token token, Arg<List<Test001>> arg, dynamic context)
    {
        Microsoft.AspNetCore.Http.HttpContext httpContext = context.HttpContext;

        //await socket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, null, CancellationToken.None);
        //return this.ResultCreate();
        return this.ResultCreate(new { token, arg = arg?.Out, State = httpContext.Connection.RemoteIpAddress.ToString() });
        //return this.ResultCreate(new { token, arg = arg?.Out, State = context.WebSocket.State });
    }

    public virtual async Task<dynamic> Test006(Token token) => this.ResultCreate();

    public virtual async Task<IResult<Test001>> Test0010(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0011(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0012(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0013(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0014(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0015(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0016(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0017(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0018(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0019(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0020(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0021(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0022(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0023(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0024(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0025(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0026(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0027(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0028(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0029(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0030(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0031(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0032(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test00333(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0034(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0035(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0036(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0037(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0038(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0039(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0040(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0041(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0042(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0043(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0044(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0045(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0046(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0047(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0048(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0049(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0050(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0051(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0052(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0053(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0054(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }
    public virtual async Task<IResult<Test001>> Test0055(SessionArg session, Arg<Test001> arg)
    {
        return this.ResultCreate(arg.Out);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="arg"></param>
    /// <param name="arg2">arg2!!!</param>
    /// <returns></returns>
    public virtual async Task<dynamic> Test007(Arg<Test001> arg, Arg<List<string>> arg2) => this.ResultCreate();

    public virtual async void Test0033(Token token, Arg<Test001> arg, [Ignore(IgnoreMode.BusinessArg)][Test2]decimal mm = 0.0234m)
    {
        dynamic args = new System.Dynamic.ExpandoObject();
        args.token = token;
        args.arg = arg.Out;
        if (arg.Out.B == "ex")
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
        [DefaultValue("")]
        public string A { get; set; }

        /// <summary>
        /// BBBBBBBBbbbbbbbbbbbbbbbbbBBBBBBBBBBBBBBBBBB
        /// </summary>
        public string B { get; set; }

        /// <summary>
        /// CccccccccccccccccccccccccccCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC
        /// </summary>
        //[Test]
        public Test0010 C { get; set; }

        public decimal D { get; set; }

        public bool E { get; set; }

        public DateTime F { get; set; }

        [DefaultValue(MyEnum.B)]
        public MyEnum myEnum { get; set; }

        public struct Test0010
        {
            /// <summary>
            /// C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1
            /// </summary>
            //[Test]
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