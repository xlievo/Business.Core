using Business;
using Business.Attributes;
using Business.Result;
using Business.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using static Args;

[JsonArg(Group = "j")]
[Command(Group = "j")]
[MessagePackArg(Group = "s")]
[Command(Group = "s")]
[Logger]
[Info("API")]
public class BusinessMember2 : BusinessBase<ResultObject<string>>
{
    static BusinessMember2()
    {
        var con = Startup.appSettings.GetSection("Redis").GetSection("ConnectionString").Value;
        System.Console.WriteLine($"Redis={con}");
        var csredis = new CSRedis.CSRedisClient(con);
        RedisHelper.Initialization(csredis);

        RedisHelper.HSetAsync("Role", "value", "111");
        RedisHelper.HSetAsync("Role", "value2", "222");
        RedisHelper.HSetAsync("Role", "value3", "333");

        var values = RedisHelper.HGetAll("Role");
    }

    public BusinessMember2()
    {
        this.Logger = x =>
        {
            try
            {
                System.Threading.SpinWait.SpinUntil(() => false, 3000);

                x.Value = x.Value?.ToValue();

                var log = x.JsonSerialize();

                Help.WriteLocal(log, console: true, write: x.Type == LoggerType.Exception);
            }
            catch (Exception exception)
            {
                Help.ExceptionWrite(exception, true, true);
            }
        };
    }

    public virtual async Task<dynamic> Test001(Business.Auth.Token token, Arg<Test001> arg, [Ignore(IgnoreMode.BusinessArg)][Test2]decimal mm = 0.0234m)
    {
        dynamic args = new System.Dynamic.ExpandoObject();
        args.token = token;
        args.arg = arg.Out;
        if (arg.Out.B == "ex")
        {
            throw new System.Exception("Method exception!");
        }

        var exit = await RedisHelper.HExistsAsync("Role", "value2");

        if (exit)
        {
            arg.Out.B = await RedisHelper.HGetAsync("Role", "value2");
        }

        return this.ResultCreate(args);
    }

    public virtual async Task Test002(Business.Auth.Token token, Arg<Test001> arg, [Ignore(IgnoreMode.BusinessArg)][Test2]decimal mm = 0.0234m)
    {
        dynamic args = new System.Dynamic.ExpandoObject();
        args.token = token;
        args.arg = arg.Out;
        if (arg.Out.B == "ex")
        {
            throw new System.Exception("Method exception!");
        }
    }

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

    public virtual async Task<dynamic> Test004(Business.Auth.Token token, Arg<List<Test001>> arg, WebSocket socket)
    {
        await socket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, null, CancellationToken.None);
        //dynamic args = new System.Dynamic.ExpandoObject();
        //args.token = 11;
        //args.arg = 22;

        //var args = new { token, arg = arg.Out };

        return this.ResultCreate(new { token, arg = arg?.Out });
    }

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
        var exit = await RedisHelper.HExistsAsync("Role", "value2");

        switch (value)
        {
            case "ok":
                return this.ResultCreate(exit ? await RedisHelper.HGetAsync("Role", "value2") : "not!");

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
    public class Test001
    {
        [Test]
        [Nick("password")]
        public string A { get; set; }

        public string B { get; set; }
    }
}