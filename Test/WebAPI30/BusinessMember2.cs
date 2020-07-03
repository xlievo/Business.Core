using Business.Core;
using Business.Core.Annotations;
using Business.Core.Auth;
using Business.Core.Result;
using Business.Core.Utils;
using Microsoft.AspNetCore.Http;
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

[Info("API/v2", CommandGroupDefault = null, Alias = "TEST0")]
[@JsonArg2(Group = "j")]
public class BusinessMember3 : BusinessBase
{
    public BusinessMember3() : base()
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

    [Testing("test2", "[\"2019-12-02T21:02\", 0.110]")]
    [Testing("test3", "[\"2019-12-02T22:02\", 0.110]")]
    [Testing("test4", "[\"2019-12-02T23:02\"")]
    [Testing("test3333", "[\"2019-12-02T23:02\", 0.110]")]
    [Ignore(IgnoreMode.BusinessArg)]
    public virtual async Task<DateTime?> Test000(Session session, DateTime? date, decimal mm = 0.0234m, dynamic context = null)
    {
        var dd = this.Command;
        return date;
        //return this.ResultCreate(date);
    }

    public struct Test001
    {
        public string A { get; set; }

        public string B { get; set; }
    }

    public virtual async Task<dynamic> Test004(Session session, List<Test001> arg, dynamic context, [Ignore(IgnoreMode.BusinessArg)][Test2] decimal mm = 0.0234m)
    {
        Microsoft.AspNetCore.Http.HttpContext httpContext = context.HttpContext;

        //await socket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, null, CancellationToken.None);
        //return this.ResultCreate();
        return this.ResultCreate(new { session, arg, State = httpContext.Connection.RemoteIpAddress.ToString() });
        //return this.ResultCreate(new { token, arg = arg?.Out, State = context.WebSocket.State });
    }
}

[DocGroup("Module 1", Position = 1, Badge = "Beta", Active = true)]
[DocGroup("Module 2", Position = 2)]
[DocGroup("Module 3", Position = 3)]
[Info("API", Alias = "UserCentre", CommandGroupDefault = null)]
[Testing("test2222", "\"2019-12-02T21:02\"", Method = "Test000")]
[Testing("test2222", "\"2019-12-02T22:02\"", Method = "Test000")]
[Testing("test3333", "\"2019-12-02T23:02\"", Method = "Test000")]
[Testing("test2222", "\"2019-12-02T21:02\"", Method = "Test000")]
[Testing("test2222", "\"2019-12-02T22:02\"", Method = "Test000")]
[Testing("test3333", "\"2019-12-02T23:02\"", Method = "Test000")]
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
public partial class BusinessMember2 : BusinessBase
{

    public struct Test001Result
    {
        /// <summary>
        /// aaa
        /// </summary>
        public string a { get; set; }

        /// <summary>
        /// bbb
        /// </summary>
        public List<Test001Result2> b { get; set; }
    }

    public struct ObjectList
    {
        /// <summary>
        /// aaa2
        /// </summary>
        public List<dynamic> Data { get; set; }
    }

    public struct Test001Result2
    {
        /// <summary>
        /// aaa2
        /// </summary>
        public string a { get; set; }

        /// <summary>
        /// bbb2
        /// </summary>
        public string b { get; set; }
    }

    /// <summary>
    /// zzzzZZZZZZZZZZZZZZZZZZZZZzzzzzzzzzzzz
    /// </summary>
    [Parameters]
    public class Z
    {
        /// <summary>
        /// aaa2
        /// </summary>
        public string a { get; set; }

        /// <summary>
        /// bbb2
        /// </summary>
        public DateTime b { get; set; }

        public int c { get; set; }

        [@JsonArg]
        public Arg<Test001Result2> d { get; set; }
    }

    //post json
    public virtual async Task<IResult<Z>> TestZ(Z z)
    {
        return this.ResultCreate(z);
    }

    public virtual async Task<IResult<dynamic>> TestObjectList(Token token2, ObjectList arg)
    {
        return this.ResultCreate();
    }

    public virtual async Task<IResult<DateTime>> Test00X(Token token2, Test001<Test0011> arg)
    {
        return this.ResultCreate();
    }

    [JsonCommand("Test00X122222222222")]
    public virtual async Task<IResult<DateTime>> Test00X1(Token token2, Test003<Test0011> arg)
    {
        return this.ResultCreate();
    }

    //[Testing("test2", "\"2019-12-02T21:02\"", tokenMethod: login)]
    //[Testing("test3", "\"2019-12-02T22:02\"", tokenMethod: login)]
    //[Testing("test4", "\"2019-12-02T23:02\"", tokenMethod: login)]
    //[Testing("test3333", "\"2019-12-02T23:02\"", tokenMethod: login)]
    //[Ignore(IgnoreMode.BusinessArg)]
    //public virtual async Task<IResult<DateTime>> Test000X(Session session, Arg<DateTime?> date)
    //{
    //    return this.ResultCreate(date.Out.GetValueOrDefault());
    //}
    //[Testing("test3333", "\"2019-12-02T23:02\"", tokenMethod: login)]
    //public virtual async Task<IResult<DateTime>> Test000X1(Session session, Arg<DateTime?> date)
    //{
    //    return this.ResultCreate(date.Out.GetValueOrDefault());
    //}
    //[Testing("test3333", "\"2019-12-02T23:02\"", tokenMethod: login)]
    //[Ignore(IgnoreMode.BusinessArg)]
    //public virtual async Task<IResult<DateTime>> Test000X2(Session session, DateTime? date)
    //{
    //    return this.ResultCreate(date.GetValueOrDefault());
    //}
    //[Testing("test3333", "\"2019-12-02T23:02\"", tokenMethod: login)]
    //public virtual async Task<IResult<DateTime>> Test000X3(Session session, DateTime? date)
    //{
    //    return this.ResultCreate(date.GetValueOrDefault());
    //}

    //[Testing("test3333", "[\"2019-12-02T23:02\", 0.110]")]
    //[Ignore(IgnoreMode.BusinessArg)]
    //public virtual async Task<DateTime?> Test000Y(Session session, Arg<DateTime?> date, decimal mm = 0.0234m, dynamic context = null)
    //{
    //    return date.Out;
    //}
    //[Testing("test3333", "[\"2019-12-02T23:02\", 0.110]")]
    //public virtual async Task<DateTime?> Test000Y1(Session session, Arg<DateTime?> date, decimal mm = 0.0234m, dynamic context = null)
    //{
    //    return date.Out;
    //}
    //[Testing("test3333", "[\"2019-12-02T23:02\", 0.110]")]
    //[Ignore(IgnoreMode.BusinessArg)]
    //public virtual async Task<DateTime?> Test000Y2(Session session, DateTime? date, decimal mm = 0.0234m, dynamic context = null)
    //{
    //    return date;
    //}
    //[Testing("test3333", "[\"2019-12-02T23:02\", 0.110]")]
    //public virtual async Task<DateTime?> Test000Y3(Session session, DateTime? date, decimal mm = 0.0234m, dynamic context = null)
    //{
    //    return date;
    //}

    /// <summary>
    /// Simple asp.net HTTP request file
    /// </summary>
    [Use(typeof(BusinessController))]
    [HttpFile]
    public class HttpFile : ReadOnlyCollection<IFormFile>
    {
        public HttpFile() { }

        public HttpFile(IEnumerable<IFormFile> values) : base(values) { }
    }

    /// <summary>
    /// Simple asp.net HTTP request file attribute
    /// </summary>
    public class HttpFileAttribute : Business.Core.Annotations.HttpFileAttribute
    {
        readonly IEqualityComparer<IFormFile> comparer = Equality<IFormFile>.CreateComparer(c => c.Name);

        /// <summary>
        /// Simple asp.net HTTP request file attribute
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        public HttpFileAttribute(int state = 830, string message = null) : base(state, message) { comparer = null; }

        public override async ValueTask<IResult> Proces<Type>(dynamic value)
        {
            BusinessController context = value;

            if (!context.Request.HasFormContentType || Equals(null, context))
            {
                return this.ResultCreate<Type>(default);
            }

            return this.ResultCreate(new HttpFile(context.Request.Form.Files.Distinct(comparer)));
        }
    }

    public const string login = "[\"Login\",\"{User:\\\"ddd\\\",Password:\\\"123456\\\"}\",\"D\"]";

    public struct Test1110
    {
        /// <summary>
        /// Test003 BBBBBBBBbbbbbbbbbbbbbbbbbBBBBBBBBBBBBBBBBBB!!!
        /// </summary>
        public string BBB { get; set; }

        /// <summary>
        /// Test004 BBBBBBBBbbbbbbbbbbbbbbbbbBBBBBBBBBBBBBBBBBB
        /// </summary>
        public string BBBB { get; set; }

        /// <summary>
        /// AAAAAAAAAaaaaaaaaaaaaaaaaaaaaaaaAAAAAAA
        /// </summary>
        public List<string> AAA { get; set; }
    }

    [Testing("test2", "{\"Arg\":{\"BBB\":\"分发给\",\"bbbb\":\"的撒\",\"aaa\":[\"aaa\",\"sss单独\"]},\"dateTime\":\"2020-07-03T02:24\"}")]
    [Testing("test3", "{\"arg\":{\"bbb\":\"分发给222\",\"bbbb\":\"的撒333\",\"aaa\":[\"aaa\",\"sss单独\"]},\"dateTime\":\"2020-07-03T02:24\"}")]
    public virtual async Task<dynamic> Test0001(Test1110 ARG, Arg<DateTime?> DateTime)
    {
        return this.ResultCreate(ARG);
    }

    /// <summary>
    /// test doc Test001
    /// and Test001
    /// </summary>
    /// <param name="session222">A token 
    /// sample222</param>
    /// <param name="Arg"></param>
    /// <param name="dateTime"></param>
    /// <param name="httpFile"></param>
    /// <param name="mm">mmmmmmmm!</param>
    /// <param name="fff"></param>
    /// <param name="bbb"></param>
    /// <returns>
    /// rrrrrr
    /// rrrrrr2
    /// </returns>
    [Doc("Alias", Group = "Module 1", Position = 1)]
    [Command("AAA")]
    //[Command("jjjTest001jjj", Group = "j")]
    //[Command("wwwwwwwwwwww", Group = "j")]
    [JsonCommand("jjjTest001jjj22222222222")]
    [Command(Group = "zzz")]
    //"{\"arg\":{\"bbb\":\"\",\"bbbb\":\"\",\"aaa\":[\"qqq\",\"www\"],\"a\":\"\",\"b\":\"\",\"c\":{\"c1\":\"\",\"c2\":\"\",\"c3\":[{\"c31\":\"eee\",\"c32\":\"rrr\",\"aaa\":[\"111\",\"222\"]}]},\"d\":0,\"e\":false,\"f\":\"\",\"myEnum\":0},\"dateTime\":\"\",\"mm\":0.0234,\"fff\":666,\"bbb\":true}"
    [Testing("test2",
         "{\"arg\":{\"AAA\":[\"qqq\",\"www\"],\"A\":\"http://127.0.0.1:5000/doc/index.html\",\"B\":\"\",\"C\":{\"C1\":\"\",\"C2\":\"\",\"C3\":[{\"C31\":\"eee\",\"C32\":\"rrr\",\"AAA\":[\"111\",\"222\"]}]},\"D\":0,\"E\":false,\"F\":\"2019-12-02T06:24\",\"myEnum\":0},\"dateTime\":\"2019-12-02T04:24\"}",
         //"[{\"AAA\":[],\"A\":\"http://127.0.0.1:5000/doc/index.html\",\"B\":\"\",\"C\":{\"C1\":\"\",\"C2\":\"\",\"C3\":[]},\"D\":0,\"E\":false,\"F\":\"2019-12-02T06:24\",\"myEnum\":\"C\"},\"2019-12-02T04:24\",99.0234,777,false]",
         "{\"AAA\":\"111\",\"BBB\":\"222\"}")]
    [Testing("test3",
        "{\"arg\":{\"AAA\":[],\"A\":\"http://127.0.0.1:5000/doc/index.html\",\"B\":\"\",\"C\":{\"C1\":\"\",\"C2\":\"\",\"C3\":[]},\"D\":0,\"E\":false,\"F\":\"2019-12-02T06:24\",\"myEnum\":2},\"dateTime\":\"2019-12-02T05:24\",\"mm\":99.0234,\"fff\":777,\"bbb\":false}")]
    //"[{\"AAA\":[],\"A\":\"http://127.0.0.1:5000/doc/index.html\",\"B\":\"\",\"C\":{\"C1\":\"\",\"C2\":\"\",\"C3\":[]},\"D\":0,\"E\":false,\"F\":\"2019-12-02T06:24\",\"myEnum\":\"C\"},\"2019-12-02T05:24\",99.0234,777,false]")
    [Testing("test4",
        "{\"arg\":{\"AAA\":[],\"A\":\"http://127.0.0.1:5000/doc/index.html\",\"B\":\"\",\"C\":{\"C1\":\"\",\"C2\":\"\",\"C3\":[]},\"D\":0,\"E\":false,\"F\":\"2019-12-02T06:24\",\"myEnum\":4},\"dateTime\":\"2019-12-02T06:24\",\"mm\":99.0234,\"fff\":777,\"bbb\":false}")]
    //"[{\"AAA\":[],\"A\":\"http://127.0.0.1:5000/doc/index.html\",\"B\":\"\",\"C\":{\"C1\":\"\",\"C2\":\"\",\"C3\":[]},\"D\":0,\"E\":false,\"F\":\"2019-12-02T06:24\",\"myEnum\":\"C\"},\"2019-12-02T06:24\",99.0234,777,false]")]
    [Testing("test5",
        "{\"arg\":{\"AAA\":[],\"A\":\"http://127.0.0.1:5000/doc/index.html\",\"B\":\"\",\"C\":{\"C1\":\"\",\"C2\":\"\",\"C3\":[]},\"D\":0,\"E\":false,\"F\":\"2019-12-02T06:24\",\"myEnum\":2},\"dateTime\":\"2019-12-02T07:24\",\"mm\":99.0234,\"fff\":777,\"bbb\":false}")]
    //"[{\"AAA\":[],\"A\":\"http://127.0.0.1:5000/doc/index.html\",\"B\":\"\",\"C\":{\"C1\":\"\",\"C2\":\"\",\"C3\":[]},\"D\":0,\"E\":false,\"F\":\"2019-12-02T06:24\",\"myEnum\":\"C\"},\"2019-12-02T07:24\",99.0234,777,false]")]
    [Testing("test, important logic, do not delete!!!",
        "{\"arg\":{\"AAA\":[],\"A\":\"http://127.0.0.1:5000/doc/index.html\",\"B\":\"\",\"C\":{\"C1\":\"ok\",\"C2\":\"😀😭\",\"C3\":[]},\"D\":0,\"E\":false,\"F\":\"2019-12-02T06:24\",\"myEnum\":4},\"dateTime\":\"2019-12-02T08:24\",\"mm\":111.0123456,\"fff\":555,\"bbb\":true}")]
    //"[{\"AAA\":[],\"A\":\"http://127.0.0.1:5000/doc/index.html\",\"B\":\"\",\"C\":{\"C1\":\"ok\",\"C2\":\"😀😭\",\"C3\":[]},\"D\":0,\"E\":false,\"F\":\"2019-12-02T06:24\",\"myEnum\":\"C\"},\"2019-12-02T08:24\",99.0234,777,false]")]
    public virtual async Task<dynamic> Test001(Session session222, Arg<Test111> Arg, Arg<DateTime?> dateTime, HttpFile httpFile = default, [Ignore(IgnoreMode.BusinessArg)][Test2] decimal mm = 0.0234m, [Ignore(IgnoreMode.BusinessArg)] int fff = 666, [Ignore(IgnoreMode.BusinessArg)] bool bbb = true)
    {
        DDD = 9;
        //Logger.loggerQueue?.queue.TryAdd(new Logger.LoggerData
        //{
        //    Type = Logger.Type.Record,
        //    Value = new Dictionary<string, dynamic>(6)
        //    {
        //        { "token", session },
        //        { "arg", arg },
        //        { "dateTime", dateTime },
        //        //{ "httpFile", httpFile },
        //        { "mm", mm },
        //        { "fff", fff },
        //        { "bbb", bbb },
        //    },
        //    Member = "BusinessMember2.Test001",
        //    Group = "j"
        //});

        dynamic args = new System.Dynamic.ExpandoObject();
        args.token = session222;
        args.arg = Arg.Out;
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

        var files = httpFile?.Select(c => new { key = c.Name, length = c.Length }).ToList();

        return this.ResultCreate(new { arg = Arg.Out, files });
        //return this.ResultCreate(new List<Test001Result?> { ss });
        //return ss;
    }

    public virtual async Task<MyEnum> TestMyEnum(Session session222, Arg<Test004> arg, Arg<DateTime?> dateTime, HttpFile httpFile = default, [Ignore(IgnoreMode.BusinessArg)][Test2] decimal mm = 0.0234m, [Ignore(IgnoreMode.BusinessArg)] int fff = 666, [Ignore(IgnoreMode.BusinessArg)] bool bbb = true)
    {
        return MyEnum.B;
    }

    public virtual async Task<Test0011> TestTest0011(Session session222, Arg<Test004> arg, Arg<DateTime?> dateTime, HttpFile httpFile = default, [Ignore(IgnoreMode.BusinessArg)][Test2] decimal mm = 0.0234m, [Ignore(IgnoreMode.BusinessArg)] int fff = 666, [Ignore(IgnoreMode.BusinessArg)] bool bbb = true)
    {
        return new Test0011 { AAA = new List<string> { "aaa" } };
    }

    /// <summary>
    /// MyEnumMyEnumMyEnumMyEnumMyEnumMyEnum
    /// </summary>
    public enum MyEnum
    {
        /// <summary>
        /// MyEnum A
        /// </summary>
        A = 0,
        B = 2,
        /// <summary>
        /// MyEnum C
        /// </summary>
        C = 4
    }

    public virtual int DDD { get; set; }

    /// <summary>
    /// 222
    /// </summary>
    /// <param name="token">A token sample</param>
    /// <param name="arg"></param>
    /// <param name="mm"></param>
    /// <returns>test return!!!</returns>
    [HttpFile]
    [Doc("Test002 222", Badge = "      ")]
    public virtual async Task<dynamic> Test002(Token token, Test002 arg, [Ignore(IgnoreMode.BusinessArg)][Test2] decimal mm = 0.0234m, [HttpFile] Dictionary<string, dynamic> context = null)
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
    [Doc("Test003 222", Badge = "New", Group = "Module 3")]
    public virtual async Task Test003(Token token, Test002 arg, [Ignore(IgnoreMode.BusinessArg)][Test2] decimal mm = 0.0234m)
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

    [Doc]
    public virtual async Task<dynamic> Test004(Token token, List<string> arg)
    {
        //Microsoft.AspNetCore.Http.HttpContext httpContext = context.HttpContext;

        //await socket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, null, CancellationToken.None);
        //return this.ResultCreate();
        return this.ResultCreate(new { arg = arg });
        //return this.ResultCreate(new { token, arg = arg?.Out, State = context.WebSocket.State });
    }

    [Doc(Group = "Module 1", Position = 0)]
    public virtual async Task<dynamic> Test005(Token token, List<Test002> arg, dynamic context)
    {
        Microsoft.AspNetCore.Http.HttpContext httpContext = context.HttpContext;

        //await socket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, null, CancellationToken.None);
        //return this.ResultCreate();
        return this.ResultCreate(new { token, arg = arg, State = httpContext.Connection.RemoteIpAddress.ToString() });
        //return this.ResultCreate(new { token, arg = arg?.Out, State = context.WebSocket.State });
    }
    [Doc(Group = "Module 2")]
    public virtual async Task<dynamic> Test006(Token token) => this.ResultCreate();

    public virtual async Task<dynamic> Test0061() => this.ResultCreate();

    public virtual IResult<Test001Result> Test0062([Ignore(IgnoreMode.BusinessArg)] string aaa)
    {
        Test001Result ss = new Test001Result { a = aaa, b = new List<Test001Result2> { new Test001Result2 { a = aaa, b = "sss" } } };

        return this.ResultCreate(ss);
    }
    public virtual IResult<Test001Result?> Test00621([Ignore(IgnoreMode.BusinessArg)] decimal? bbb, [Ignore(IgnoreMode.BusinessArg)] decimal aaa = 111.123m)
    {
        Test001Result? ss = new Test001Result { a = aaa.ToString(), b = new List<Test001Result2> { new Test001Result2 { a = aaa.ToString(), b = bbb?.ToString() } } };

        return this.ResultCreate(ss);
    }

    public virtual IResult<Test001Result?> Test0063(Session session, [Ignore(IgnoreMode.BusinessArg)] string aaa)
    {
        Test001Result? ss = new Test001Result { a = aaa, b = new List<Test001Result2> { new Test001Result2 { a = aaa, b = "sss" } } };

        return this.ResultCreate(ss);
    }

    public virtual async Task<IResult<Test002>> Test0010(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test001<Test0011>>> Test0011(Session session, Test002 arg)
    {
        return this.ResultCreate();
    }
    public virtual async Task<IResult<Test004>> Test0012(Session session, Test002 arg)
    {
        return this.ResultCreate();
    }
    public virtual async Task<IResult<Dictionary<string, List<Test002>>>> Test0013(Session session, Dictionary<string, List<Test002>> arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test002>> Test0014(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test002>> Test0015(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test002>> Test0016(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test002>> Test0017(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test002>> Test0018(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test002>> Test0019(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test002>> Test0020(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test002>> Test0021(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test002>> Test0022(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test002>> Test0023(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test002>> Test0024(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test002>> Test0025(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test002>> Test0026(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test002>> Test0027(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test002>> Test0028(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test002>> Test0029(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test002>> Test0030(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test002>> Test0031(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test002>> Test0032(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test002>> Test00333(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test002>> Test0034(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test002>> Test0035(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test002>> Test0036(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test002>> Test0037(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test002>> Test0038(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test002>> Test0039(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test002>> Test0040(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test002>> Test0041(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test002>> Test0042(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test002>> Test0043(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test002>> Test0044(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test002>> Test0045(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test002>> Test0046(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test002>> Test0047(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test002>> Test0048(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test002>> Test0049(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test002>> Test0050(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test002>> Test0051(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test002>> Test0052(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test002>> Test0053(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test002>> Test0054(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async Task<IResult<Test002>> Test0055(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="arg">argargargarg</param>
    /// <param name="arg2">arg2!!!</param>
    /// <returns></returns>
    public virtual async Task<dynamic> Test007(Test002 arg, List<string> arg2) => this.ResultCreate();

    public virtual async void Test0033(Token token, Test002 arg, [Ignore(IgnoreMode.BusinessArg)][Test2] decimal mm = 0.0234m)
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

        public List<Test002> arg { get; set; }
    }
}

public class TestAttribute : ArgumentAttribute
{
    public TestAttribute(int state = 111, string message = null) : base(state, message) { }

    public override async ValueTask<IResult> Proces<Type>(dynamic token, dynamic value)
    {
        //var exit = await RedisHelper.HExistsAsync("Role", "value2");

        switch (value)
        {
            case "ok":
                //return this.ResultCreate(exit ? await RedisHelper.HGetAsync("Role", "value2") : "not!");
                return this.ResultCreate("OK!!!");
            case "error":
                return this.ResultCreate(this.State, $"{this.Alias} cannot be empty");

            case "data":
                return this.ResultCreate(value + "1122");

            //default: throw new System.Exception("Argument exception!");
            default: break;
        }

        System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(new System.Exception("Argument exception!")).Throw();

        return default;
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
    /// Test001Test001Tes
    /// t001Test001Test001Test001
    /// </summary>
    public struct Test111
    {
        /// <summary>
        /// Test003 BBBBBBBBbbbbbbbbbbbbbbbbbBBBBBBBBBBBBBBBBBB!!!
        /// </summary>
        public string BBB { get; set; }

        /// <summary>
        /// Test004 BBBBBBBBbbbbbbbbbbbbbbbbbBBBBBBBBBBBBBBBBBB
        /// </summary>
        public string BBBB { get; set; }

        /// <summary>
        /// AAAAAAAAAaaaaaaaaaaaaaaaaaaaaaaaAAAAAAA
        /// </summary>
        //[Test]
        public List<string> AAA { get; set; }

        /// <summary>
        /// AAAAAAAAAaaaaaaaaaaaaaaaaaaaaaaaAAAAAAA
        /// </summary>
        //[Test]
        [Alias("password")]
        [@CheckNull]
        //[@CheckEmail]
        [@CheckUrl]
        public string A { get; set; }

        /// <summary>
        /// BBBBBBBBbbbbbbbbbbbbbbbbbBBBBBBBBBBBBBBBBBB
        /// </summary>
        public string B { get; set; }


        //[Test]
        public Test0010<Test0011> C { get; set; }

        /// <summary>
        /// DDD
        /// </summary>
        public decimal? D { get; set; }

        public bool E { get; set; }

        /// <summary>
        /// FF
        /// </summary>
        public DateTime F { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public MyEnum myEnum { get; set; }

        /// <summary>
        /// Test0010 Test0010 Test0010 Test0010
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public struct Test0010<T>
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
            public List<T> C3 { get; set; }

            //public string C22 { get; set; }

            //public Test0011 C33 { get; set; }

            //public Test0011 C34 { get; set; }

            //public string C35 { get; set; }
        }
    }

    /// <summary>
    /// MyEnumMyEnumMyEnumMyEnumMyEnumMyEnum
    /// </summary>
    public enum MyEnum
    {
        /// <summary>
        /// MyEnum A
        /// </summary>
        A = 0,
        /// <summary>
        /// 
        /// </summary>
        B = 2,
        /// <summary>
        /// MyEnum C
        /// </summary>
        C = 4,
    }

    public class Test002 : Test001<Test0011>
    {
        /// <summary>
        /// Test002 BBBBBBBBbbbbbbbbbbbbbbbbbBBBBBBBBBBBBBBBBBB
        /// </summary>
        public string BB { get; set; }
    }

    public class Test003<T> : Test001<T>
    {
        /// <summary>
        /// Test003 BBBBBBBBbbbbbbbbbbbbbbbbbBBBBBBBBBBBBBBBBBB!!!
        /// </summary>
        public string BBB { get; set; }
    }

    /// <summary>
    /// Test004Test004Test004Test004Test004Test004Test004
    /// Test004Test004Test004Test004Test004Test004Test004
    /// </summary>
    public class Test004 : Test003<Test0011>
    {
        /// <summary>
        /// Test004 BBBBBBBBbbbbbbbbbbbbbbbbbBBBBBBBBBBBBBBBBBB
        /// </summary>
        public string BBBB { get; set; }
    }

    /// <summary>
    /// Test001Test001Tes
    /// t001Test001Test001Test001
    /// </summary>
    public class Test001<T>
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
        [Alias("password")]
        [@CheckNull]
        //[@CheckEmail]
        [@CheckUrl]
        public string A { get; set; }

        /// <summary>
        /// BBBBBBBBbbbbbbbbbbbbbbbbbBBBBBBBBBBBBBBBBBB
        /// </summary>
        public string B { get; set; }


        //[Test]
        public Test0010<T> C { get; set; }

        /// <summary>
        /// DDD
        /// </summary>
        public decimal? D { get; set; }

        public bool E { get; set; }

        /// <summary>
        /// FF
        /// </summary>
        public DateTime F { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public MyEnum myEnum { get; set; }

        /// <summary>
        /// Test0010 Test0010 Test0010 Test0010
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public struct Test0010<T>
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
            public List<T> C3 { get; set; }

            //public string C22 { get; set; }

            //public Test0011 C33 { get; set; }

            //public Test0011 C34 { get; set; }

            //public string C35 { get; set; }
        }
    }

    /// <summary>
    /// Test0011Test0011Test0011Test0011
    /// </summary>
    public class Test0011
    {
        /// <summary>
        /// C31C31C31C31C31C31
        /// </summary>
        public string C31 { get; set; }

        /// <summary>
        /// C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32
        /// </summary>
        public string C32 { get; set; }

        /// <summary>
        /// AAA@@@
        /// </summary>
        public List<string> AAA { get; set; }
    }
}