using Business;
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

[assembly: Command(Group = "j")]
[assembly: Command(Group = "s")]

[assembly: JsonArg(Group = "j")]
[assembly: MessagePackArg(Group = "s")]

/// <summary>
/// BusinessMember2。。。
/// </summary>
[Logger]
[Info("API")]
public class BusinessMember2 : BusinessBase<ResultObject<string>>
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

    public struct CallInfo
    {
        public DateTime Time;

        public string MethodName;
    }

    readonly Host host;

    System.Net.Http.IHttpClientFactory httpClientFactory;

    public BusinessMember2(Host host)
    {
        this.host = host;

        var s = "[{ \"a\": \"333\",\"b\": \"\", \"c\": { \"c1\": \"\",\"c2\": \"\",\"c3\": { \"c31\": \"\",\"c32\": \"uuu\"} },\"d\": 0, \"e\": true, \"myEnum\": 1}, 0.2345]";

        //var ddd = System.Text.Json.JsonSerializer.Serialize(new Test001());

        //var data = System.Text.Json.JsonSerializer.Deserialize<object[]>(s);

        //var d = data[0].ToString();

        //var options = new System.Text.Json.JsonSerializerOptions
        //{
        //    PropertyNameCaseInsensitive = true,
        //    AllowTrailingCommas = true
        //};
        //var test001 = System.Text.Json.JsonSerializer.Deserialize<Test001>(d, options);

        //var d2 = data[1].ToString();

        //if (null == data)
        //{

        //}

        this.Logger = new Logger(async x =>
        {
            try
            {
                System.Threading.SpinWait.SpinUntil(() => false, 3000);

                //x.Value = x.Value?.ToValue();

                //var log = x.JsonSerialize();

                foreach (var item in x)
                {
                    var log = item.JsonSerialize();

                    Help.WriteLocal(log, console: true, write: item.Type == LoggerType.Exception);
                }
            }
            catch (Exception exception)
            {
                Help.ExceptionWrite(exception, true, true);
            }
        });

        this.BindBefore = c =>
        {
            c.CallBeforeMethod = async (meta, args) =>
            {

            };

            c.CallAfterMethod = async (meta, args, result) =>
            {
                if (typeof(System.Threading.Tasks.Task).IsAssignableFrom(result.GetType()))
                {
                    var result2 = await result;

                    //await System.Threading.Tasks.Task.Run(() =>
                    //{
                    //    System.Threading.Thread.Sleep(3000);
                    //});

                    result2.State = 111;
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

    /// <summary>
    /// test doc Test001
    /// and Test001
    /// </summary>
    /// <param name="token">A token sample</param>
    /// <param name="arg"></param>
    /// <param name="mm">mmmmmmmm!</param>
    /// <returns></returns>
    [Command("AAA")]
    public virtual async Task<IResult<Test001>> Test001([Test2]Business.Auth.Token token, Arg<Test001> arg, [Ignore(IgnoreMode.BusinessArg)][Ignore(IgnoreMode.Arg)]Arg<DateTime> dateTime, [Ignore(IgnoreMode.BusinessArg)][Ignore(IgnoreMode.Arg)][Test2]decimal mm = 0.0234m, dynamic context = null)
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
        //    //return this.ResultCreate<Business.Auth.Token>(null);

        //    return this.ResultCreate<Test001Result>(-110, "sss");
        //}

        return this.ResultCreate(arg.Out);
        //return this.ResultCreate(new Test001Result { A = "AAA", B = "SSS" });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="token">A token sample</param>
    /// <param name="arg"></param>
    /// <param name="mm"></param>
    /// <returns>test return!!!</returns>
    public virtual async Task<string> Test002(Business.Auth.Token token, Arg<Test001> arg, [Ignore(IgnoreMode.BusinessArg)][Test2]decimal mm = 0.0234m)
    {
        dynamic args = new System.Dynamic.ExpandoObject();
        args.token = token;
        args.arg = arg.Out;
        if (arg.Out.B == "ex")
        {
            throw new System.Exception("Method exception!");
        }

        return "sss";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="token"></param>
    /// <param name="arg"></param>
    /// <param name="mm"></param>
    /// <returns></returns>
    public virtual async Task Test003(Business.Auth.Token token, Arg<Test001> arg, [Ignore(IgnoreMode.BusinessArg)][Test2]decimal mm = 0.0234m)
    {
        dynamic args = new System.Dynamic.ExpandoObject();
        args.token = token;
        args.arg = arg.Out;
        if (arg.Out.B == "ex")
        {
            throw new System.Exception("Method exception!");
        }
    }

    public virtual async Task<dynamic> Test004(Business.Auth.Token token, Arg<List<Test001>> arg, dynamic context, [Ignore(IgnoreMode.BusinessArg)][Test2]decimal mm = 0.0234m)
    {
        Microsoft.AspNetCore.Http.HttpContext httpContext = context.Controller.HttpContext;

        //await socket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, null, CancellationToken.None);
        //return this.ResultCreate();
        return this.ResultCreate(new { token, arg = arg?.Out, State = httpContext.Connection.RemoteIpAddress.ToString() });
        //return this.ResultCreate(new { token, arg = arg?.Out, State = context.WebSocket.State });
    }

    public virtual async Task<dynamic> Test005(Business.Auth.Token token, Arg<List<Test001>> arg, dynamic context)
    {
        Microsoft.AspNetCore.Http.HttpContext httpContext = context.Controller.HttpContext;

        //await socket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, null, CancellationToken.None);
        //return this.ResultCreate();
        return this.ResultCreate(new { token, arg = arg?.Out, State = httpContext.Connection.RemoteIpAddress.ToString() });
        //return this.ResultCreate(new { token, arg = arg?.Out, State = context.WebSocket.State });
    }

    public virtual async Task<dynamic> Test006(Business.Auth.Token token) => this.ResultCreate();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="arg"></param>
    /// <param name="arg2">arg2!!!</param>
    /// <returns></returns>
    public virtual async Task<dynamic> Test007(Arg<Test001> arg, Arg<List<string>> arg2) => this.ResultCreate();

    public struct Result
    {
        public Business.Auth.Token token { get; set; }

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

public class CheckNull2Attribute : CheckNullAttribute
{
    public CheckNull2Attribute(int state = -800, string message = null) : base(state, message)
    {
        this.BindAfter += () =>
        {
            this.Description = this.Replace("{Nick} 这个参数必须填写");
        };
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
    /// Test001Test001Test001Test001Test001Test001
    /// </summary>
    public class Test001
    {
        /// <summary>
        /// AAAAAAAAAaaaaaaaaaaaaaaaaaaaaaaaAAAAAAA
        /// </summary>
        [Test]
        public List<string> AAA { get; set; }

        /// <summary>
        /// AAAAAAAAAaaaaaaaaaaaaaaaaaaaaaaaAAAAAAA
        /// </summary>
        [Test]
        [Nick("password")]
        [CheckNull2]
        [DefaultValue("")]
        public string A { get; set; }

        /// <summary>
        /// BBBBBBBBbbbbbbbbbbbbbbbbbBBBBBBBBBBBBBBBBBB
        /// </summary>
        public string B { get; set; }

        /// <summary>
        /// CccccccccccccccccccccccccccCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC
        /// </summary>
        [Test]
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