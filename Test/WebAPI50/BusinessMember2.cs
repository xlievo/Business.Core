﻿using Business.Core;
using Business.Core.Annotations;
using Business.Core.Auth;
using Business.Core.Result;
using Business.Core.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Args;
using static Common;

/// <summary>
/// Paging
/// </summary>
/// <typeparam name="T"></typeparam>
public struct Paging<T>
{
    /// <summary>
    /// Data222
    /// </summary>
    public List<T> Data { get; set; }

    /// <summary>
    /// Length2
    /// </summary>
    public int Length { get; set; }

    /// <summary>
    /// CurrentPage2
    /// </summary>
    public int CurrentPage { get; set; }

    /// <summary>
    /// Count2
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// CountPage2
    /// </summary>
    public int CountPage { get; set; }
}

[Info("API/v2", CommandGroupDefault = null, Alias = "TEST0")]
[@JsonArg2(Group = "j")]
public class BusinessMember3 : BusinessBase
{
    public readonly System.Net.Http.IHttpClientFactory httpClientFactory;

    [Injection]
    IApplicationBuilder app;

    public BusinessMember3(System.Net.Http.IHttpClientFactory httpClientFactory, IMemoryCache memoryCache, string dd = "ttt", int cc = 999, ApplicationBuilder app = null) : base()
    {
        memoryCache?.Set("a", 333);
        var ddd = memoryCache?.Get<int>("a");

        this.httpClientFactory = httpClientFactory;

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

        this.BindAfter = () => 
        {
            this.app.ToString();
        };
    }

    [Testing("test2", "[\"2019-12-02T21:02\", 0.110]")]
    [Testing("test3", "[\"2019-12-02T22:02\", 0.110]")]
    [Testing("test4", "[\"2019-12-02T23:02\"")]
    [Testing("test3333", "[\"2019-12-02T23:02\", 0.110]")]
    [Ignore(IgnoreMode.BusinessArg)]
    public virtual async ValueTask<DateTime?> Test000(Session session, DateTime? date, decimal mm = 0.0234m, dynamic context = null)
    {
        var dd = this.Command;
        return date;
        //return this.ResultCreate(date);
    }

    public struct Test001
    {
        public string A { get; set; }

        public string B { get; set; }

        [DynamicObject]
        public System.Text.Json.JsonElement C { get; set; }

    }

    public virtual async ValueTask<dynamic> Test004(Session session, List<Test001> arg, BusinessController context, [Ignore(IgnoreMode.BusinessArg)][Test2] decimal mm = 0.0234m)
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
[@JsonArg2(Group = "j")]
public partial class BusinessMember2
{
    //[Injection]
    readonly HttpClient httpClient2;

    public readonly IHttpClientFactory httpClientFactory;

    public BusinessMember2(IHttpClientFactory httpClientFactory, HttpClient httpClient2)
    {
        this.httpClient2 = httpClient2;
        this.httpClientFactory = httpClientFactory;

        this.BindBefore = c =>
        {
            c.CallBeforeMethod = async method =>
            {
                method.Cancel = false;
            };

            c.CallAfterMethod = async method =>
            {
                //if (typeof(IAsyncResult).IsAssignableFrom(method.Result?.GetType()))
                //{
                //    //await result;

                //    //await System.Threading.Tasks.Task.Run(() =>
                //    //{
                //    //    System.Threading.Thread.Sleep(3000);
                //    //});

                //    //result2.State = 111;
                //}

                if (method.Result is IResult result)
                {
                    if (1 == result.State)
                    {

                        //...//
                        //result.m = "ssssss";
                        method.Result = this.ResultCreate(result.State, "ssssss");
                    }
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

        public Test001Result2 d { get; set; }

        public bool e { get; set; }

        public List<string> f { get; set; }
    }

    //post json
    public virtual async ValueTask<IResult<Z>> TestZ(Token token2, Z z, HttpFile files)
    {
        return this.ResultCreate(z);
    }

    public virtual async ValueTask<IResult<dynamic>> TestObjectList(Token token2, ObjectList arg)
    {
        return this.ResultCreate();
    }

    public virtual async ValueTask<IResult<DateTime>> Test00X(Token token2, Test001<Test0011> arg)
    {
        return this.ResultCreate();
    }

    [JsonCommand("Test00X122222222222")]
    public virtual async ValueTask<IResult<DateTime>> Test00X1(Token token2, Test003<Test0011> arg)
    {
        return this.ResultCreate();
    }

    //[Testing("test2", "\"2019-12-02T21:02\"", tokenMethod: login)]
    //[Testing("test3", "\"2019-12-02T22:02\"", tokenMethod: login)]
    //[Testing("test4", "\"2019-12-02T23:02\"", tokenMethod: login)]
    //[Testing("test3333", "\"2019-12-02T23:02\"", tokenMethod: login)]
    //[Ignore(IgnoreMode.BusinessArg)]
    //public virtual async ValueTask<IResult<DateTime>> Test000X(Session session, Arg<DateTime?> date)
    //{
    //    return this.ResultCreate(date.Out.GetValueOrDefault());
    //}
    //[Testing("test3333", "\"2019-12-02T23:02\"", tokenMethod: login)]
    //public virtual async ValueTask<IResult<DateTime>> Test000X1(Session session, Arg<DateTime?> date)
    //{
    //    return this.ResultCreate(date.Out.GetValueOrDefault());
    //}
    //[Testing("test3333", "\"2019-12-02T23:02\"", tokenMethod: login)]
    //[Ignore(IgnoreMode.BusinessArg)]
    //public virtual async ValueTask<IResult<DateTime>> Test000X2(Session session, DateTime? date)
    //{
    //    return this.ResultCreate(date.GetValueOrDefault());
    //}
    //[Testing("test3333", "\"2019-12-02T23:02\"", tokenMethod: login)]
    //public virtual async ValueTask<IResult<DateTime>> Test000X3(Session session, DateTime? date)
    //{
    //    return this.ResultCreate(date.GetValueOrDefault());
    //}

    //[Testing("test3333", "[\"2019-12-02T23:02\", 0.110]")]
    //[Ignore(IgnoreMode.BusinessArg)]
    //public virtual async ValueTask<DateTime?> Test000Y(Session session, Arg<DateTime?> date, decimal mm = 0.0234m, dynamic context = null)
    //{
    //    return date.Out;
    //}
    //[Testing("test3333", "[\"2019-12-02T23:02\", 0.110]")]
    //public virtual async ValueTask<DateTime?> Test000Y1(Session session, Arg<DateTime?> date, decimal mm = 0.0234m, dynamic context = null)
    //{
    //    return date.Out;
    //}
    //[Testing("test3333", "[\"2019-12-02T23:02\", 0.110]")]
    //[Ignore(IgnoreMode.BusinessArg)]
    //public virtual async ValueTask<DateTime?> Test000Y2(Session session, DateTime? date, decimal mm = 0.0234m, dynamic context = null)
    //{
    //    return date;
    //}
    //[Testing("test3333", "[\"2019-12-02T23:02\", 0.110]")]
    //public virtual async ValueTask<DateTime?> Test000Y3(Session session, DateTime? date, decimal mm = 0.0234m, dynamic context = null)
    //{
    //    return date;
    //}



    public const string login = "[\"Login\",\"{User:\\\"ddd\\\",Password:\\\"123456\\\"}\",\"D\"]";

    /// <summary>
    /// Test1110!!!
    /// </summary>
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

        /// <summary>
        /// Test001Result2!!!
        /// </summary>
        public List<Test001Result2> CCC { get; set; }
    }

    [Testing("test2", "{\"Arg\":{\"BBB\":\"sss\",\"bbbb\":\"dd\",\"aaa\":[\"aaa\",\"ssssaaazzzxxx\"]},\"dateTime\":\"2020-07-03T02:24\"}")]
    [Testing("test3", "{\"arg\":{\"bbb\":\"fff222\",\"bbbb\":\"的撒333\",\"aaa\":[\"aaa\",\"sssaa\"]},\"dateTime\":\"2020-07-03T02:24\"}")]
    public virtual async ValueTask<IResult<Paging<Test1110>>> Test0001(Test1110 ARG, DateTime? DateTime)
    {
        var data = new Paging<Test1110> { };
        return this.ResultCreate(data);
    }

    public virtual async ValueTask<IResult<List<dynamic>>> Test00012X(Test1110 ARG, DateTime? DateTime)
    {
        return this.ResultCreate();
    }

    public virtual async ValueTask<IResult<List<string>>> Test00012X1(Test1110 ARG, DateTime? DateTime)
    {
        return this.ResultCreate();
    }

    public virtual async ValueTask<IResult<List<Test1110>>> Test00012(Test1110 ARG, DateTime? DateTime)
    {
        var data = new List<Test1110> { };
        return this.ResultCreate(data);
    }

    public virtual async ValueTask Test010X(Test0011 test, int b, WebSocket webSocket = null, [Ignore(IgnoreMode.Arg)] params string[] id)
    {

    }

    public class TestListArg
    {
        [@CheckNull]
        public string AAA { get; set; }

        public List<TestList2> BBB { get; set; }

        public class TestList2
        {
            [@CheckNull]
            public string CCC { get; set; }
        }
    }

    public virtual async ValueTask<IResult> TestList(TestListArg arg) => this.ResultCreate(arg);

    public class SourceAddArg
    {
        [@Size(Min = 1, Max = 128)]
        public string Name { get; set; }

        [@Size(Max = 512)]
        public string Remark { get; set; }
    }

    public virtual async ValueTask<dynamic> SourceAdd(Token token, [Parameters] SourceAddArg arg)
    {
        return this.ResultCreate(new { token, arg });
    }

    /// <summary>
    /// Test0011Test0011Test0011Test0011
    /// </summary>
    public class Test001222
    {
        /// <summary>
        /// C31C31C31C31C31C31
        /// </summary>
        public string C31 { get; set; }

        /// <summary>
        /// C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32
        /// </summary>
        public int C32 { get; set; }

        /// <summary>
        /// C322C322C322C322C322
        /// </summary>
        public int C322;

        /// <summary>
        /// C3366C3366C3366C3366C3366C3366C3366C3366
        /// </summary>
        [DynamicObject]
        public System.Text.Json.JsonElement C3366 { get; set; }

        /// <summary>
        /// C333C333C333C333C333C333
        /// </summary>
        public List<TeamMember> C333;

        /// <summary>
        /// C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32
        /// </summary>
        public List<Test0013> C33;

        public class Test0013
        {
            /// <summary>
            /// C31C31C31C31C31C31
            /// </summary>
            public List<TeamMember> C311;

            /// <summary>
            /// C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32
            /// </summary>
            public int C322;
        }

        public class TeamMember
        {
            /// <summary>
            /// UserId
            /// </summary>
            public string UserId { get; set; }
            /// <summary>
            /// UserImg
            /// </summary>
            public string UserImg { get; set; }
        }
    }

    [Doc(Group = "Module 1")]
    public virtual async ValueTask<IResult<Test001222>> TestDoc00555(Test001222 arg)
    {
        return this.ResultCreate(arg);
    }

    /// <summary>
    /// Test0011Test0011Test0011Test0011
    /// </summary>
    [DynamicObject]
    public struct Test00122
    {
        /// <summary>
        /// C31C31C31C31C31C31
        /// </summary>
        public string C31 { get; set; }

        /// <summary>
        /// C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32
        /// </summary>
        public int C32 { get; set; }

        /// <summary>
        /// C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32
        /// </summary>
        [DynamicObject]
        public IEnumerable<Test0013> C33 { get; set; }

        public struct Test0013
        {
            /// <summary>
            /// C31C31C31C31C31C31
            /// </summary>
            public string C311 { get; set; }

            /// <summary>
            /// C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32
            /// </summary>
            public int C322 { get; set; }
        }
    }
    //[{\\\"C311\\\":\\\"222\\\",\\\"C322\\\":555},{\\\"C311\\\":\\\"333\\\",\\\"C322\\\":666}]
    [Testing("test2", "{\"arg\":[\"{\\\"C31\\\":\\\"222\\\",\\\"C32\\\":555,\\\"C33\\\":[{\\\"C311\\\":\\\"222\\\",\\\"C322\\\":555},{\\\"C311\\\":\\\"333\\\",\\\"C322\\\":666}]}\"," +
        "\"{\\\"C31\\\":\\\"333\\\",\\\"C32\\\":666}\"]}")]
    [Testing("test3", "{\"arg\":{\"C31\":\"222\",\"C32\":555,\"C33\":[\"{\\\"C311\\\":\\\"222\\\",\\\"C322\\\":555}\",\"{\\\"C311\\\":\\\"333\\\",\\\"C322\\\":666}\"]}}")]//,\"C33\":[\"{\\\"C311\\\":\\\"333\\\",\\\"C322\\\":666}\"]
    [Doc(Group = "Module 1")]
    public virtual async ValueTask<dynamic> Test00555(IEnumerable<Test00122> arg, [DynamicObject] IEnumerable<dynamic> arg2, [DynamicObject] dynamic arg3)
    {
        //var dd = new Test00122 { C31 = "222", C32 = 555, C33 = new List<string> { new Test00122.Test0013 { C311 = "222", C322 = 555 }.JsonSerialize(), new Test00122.Test0013 { C311 = "333", C322 = 666 }.JsonSerialize() } }.JsonSerialize().JsonSerialize();
        //var dd = new Test00122 { C31 = "222", C32 = 555, C33 = new List<Test00122.Test0013> { new Test00122.Test0013 { C311 = "222", C322 = 555 }, new Test00122.Test0013 { C311 = "333", C322 = 666 } } }.JsonSerialize();

        //var dd3 = arg.JsonSerialize().TryJsonDeserialize<IList<Test00122>>();

        return this.ResultCreate(new { arg, arg2, arg3 });
    }

    public virtual async ValueTask<IResult> TestMyEnum2(MyEnum? arg)
    {
        return this.ResultCreate(arg.HasValue ? null == arg.Value.GetName() ? null : arg.Value : null);
    }

    public virtual async Task Test012()
    {
        return;
    }

    /// <summary>
    /// MyLogicArg!
    /// </summary>
    [@CheckNull]
    [@CheckNull]
    public struct MyLogicArg
    {
        /// <summary>
        /// AAA
        /// </summary>
        [@CheckNull]
        [@CheckNull]
        public string A { get; set; }

        /// <summary>
        /// BBB
        /// </summary>
        [@CheckNull]
        [@CheckNull]
        //[JsonArg2]
        public MyStruct B { get; set; }

        public class MyStruct
        {
            /// <summary>
            /// BBB
            /// </summary>
            [@CheckNull]
            public string BB { get; set; }
        }
    }

    public class MyLogicArg2
    {
        /// <summary>
        /// AAA
        /// </summary>
        [@CheckNull]
        [@CheckNull]
        public string A { get; set; }

        /// <summary>
        /// BBB
        /// </summary>
        [@CheckNull]
        [@CheckNull]
        //[JsonArg2]
        public MyStruct B { get; set; }

        public class MyStruct
        {
            /// <summary>
            /// BBB
            /// </summary>
            [@CheckNull]
            public string BB { get; set; }
        }
    }

    public virtual async ValueTask<dynamic> MyLogic(Token token, BusinessController context, HttpFile files, MyLogicArg arg)
    {
        return this.ResultCreate(new { token, arg });
    }

    public virtual async ValueTask<IResult<MyLogicArg2>> MyParameters(Token token, HttpFile files, [Parameters] MyLogicArg2 arg)
    {
        return this.ResultCreate(arg);
    }

    /// <summary>
    /// test doc Test001!!!
    /// and Test001!!!
    /// </summary>
    /// <param name="session222">A token 
    /// sample222</param>
    /// <param name="Arg"></param>
    /// <param name="dateTime">datetime!!!</param>
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
        "{\"Arg\":{\"AAA\":[],\"A\":\"http://127.0.0.1:5000/doc/index.html\",\"B\":\"\",\"C\":{\"C1\":\"ok\",\"C2\":\"😀😭\",\"C3\":[]},\"D\":900.87,\"E\":false,\"F\":\"2019-12-02T06:24\",\"myEnum\":4},\"dateTime\":\"2019-12-02T08:24\",\"MM\":111.0123456,\"fFf\":555,\"bbB\":true," +
        "\"aaa\":{\"AAA\":[],\"A\":\"http://127.0.0.1:5000/doc/index.html\",\"B\":\"\",\"C\":{\"C1\":\"ok\",\"C2\":\"😀😭\",\"C3\":[]},\"D\":900.87,\"E\":false,\"F\":\"2019-12-02T06:24\",\"myEnum\":4}," +
        "\"bbbb\":\"{\\\"C31\\\":\\\"222\\\",\\\"C32\\\":555}\",\"ccc\":\"{\\\"C31\\\":\\\"222\\\",\\\"C32\\\":555}\"}")]
    //"[{\"AAA\":[],\"A\":\"http://127.0.0.1:5000/doc/index.html\",\"B\":\"\",\"C\":{\"C1\":\"ok\",\"C2\":\"😀😭\",\"C3\":[]},\"D\":0,\"E\":false,\"F\":\"2019-12-02T06:24\",\"myEnum\":\"C\"},\"2019-12-02T08:24\",99.0234,777,false]")]
    public virtual async ValueTask<IResult<Test111>> Test001(Session session222, Test111? Arg, [@CheckNull(CheckValueType = true)] DateTime? dateTime, HttpFile httpFile = default, [Ignore(IgnoreMode.BusinessArg)][Test2] decimal? mm = 0.0234m, [Ignore(IgnoreMode.BusinessArg)] int fff = 666, [Ignore(IgnoreMode.BusinessArg)] bool bbb = true, BusinessController context2 = null, Test111 aaa = default, [DynamicObject] object bbbb = null, object ccc = null)
    {
        //var bing = await httpClient2.GetStringAsync("https://www.bing.com");

        var ip = new Business.Core.Auth.Token { Remote = new Business.Core.Auth.Remote(context2.Request.HttpContext.Connection.RemoteIpAddress.ToString(), context2.Request.HttpContext.Connection.RemotePort), Key = "Key", Callback = "Callback" };

        var ip2 = ip.JsonSerialize();

        var ip22 = ip2.TryJsonDeserialize<Business.Core.Auth.Token>();

        var ip3 = MessagePack.MessagePackSerializer.Serialize(ip);

        var ip33 = MessagePack.MessagePackSerializer.Deserialize<Business.Core.Auth.Token>(ip3);

        var iii = ip33.Equals(ip);

        //ip33.Port = 999;

        //var iii2 = ip33.Equals(ip);

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
        args.arg = Arg.Value;
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

        return this.ResultCreate(Arg.Value);
        //return this.ResultCreate(new List<Test001Result?> { ss });
        //return ss;
    }

    public virtual int? Test001x() => this.Logger.loggerQueue?.queue.Count;

    public virtual async ValueTask<MyEnum> TestMyEnum(Session session222, Test004 arg, DateTime? dateTime, HttpFile httpFile = default, [Ignore(IgnoreMode.BusinessArg)][Test2] decimal mm = 0.0234m, [Ignore(IgnoreMode.BusinessArg)] int fff = 666, [Ignore(IgnoreMode.BusinessArg)] bool bbb = true)
    {
        return MyEnum.B;
    }

    public virtual async ValueTask<Test0011> TestTest0011(Session session222, Test004 arg, DateTime? dateTime, HttpFile httpFile = default, [Ignore(IgnoreMode.BusinessArg)][Test2] decimal mm = 0.0234m, [Ignore(IgnoreMode.BusinessArg)] int fff = 666, [Ignore(IgnoreMode.BusinessArg)] bool bbb = true)
    {
        return new Test0011 { AAA = new List<string> { "aaa" } };
    }

    /// <summary>
    /// MyEnumMyEnumMyEnumMyEnumMyEnumMyEnum
    /// </summary>
    [EnumCheck]
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
    [Common.HttpFile]
    [Doc("Test002 222", Badge = "      ")]
    public virtual async ValueTask<dynamic> Test002(Token token, Test002 arg, [Ignore(IgnoreMode.BusinessArg)][Test2] decimal mm = 0.0234m, [Common.HttpFile] Dictionary<string, dynamic> context = null)
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
    public virtual async ValueTask<dynamic> Test004(Token token, List<string> arg)
    {
        //Microsoft.AspNetCore.Http.HttpContext httpContext = context.HttpContext;

        //await socket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, null, CancellationToken.None);
        //return this.ResultCreate();
        return this.ResultCreate(new { arg = arg });
        //return this.ResultCreate(new { token, arg = arg?.Out, State = context.WebSocket.State });
    }

    [Doc(Group = "Module 1", Position = 0)]
    public virtual async ValueTask<dynamic> Test005(Token token, List<Test002> arg, BusinessController context)
    {
        Microsoft.AspNetCore.Http.HttpContext httpContext = context.HttpContext;

        //await socket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, null, CancellationToken.None);
        //return this.ResultCreate();
        return this.ResultCreate(new { token, arg = arg, State = httpContext.Connection.RemoteIpAddress.ToString() });
        //return this.ResultCreate(new { token, arg = arg?.Out, State = context.WebSocket.State });
    }
    [Doc(Group = "Module 2")]
    public virtual async ValueTask<dynamic> Test006(Token token) => this.ResultCreate();

    public virtual async ValueTask<dynamic> Test0061() => this.ResultCreate();

    [Testing("test2",
        "{\"aAa\":\"AAA 222\"}")]
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

    public virtual async ValueTask<IResult<Test002>> Test0010(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test001<Test0011>>> Test0011(Session session, Test002 arg)
    {
        return this.ResultCreate();
    }
    public virtual async ValueTask<IResult<Test004>> Test0012(Session session, Test002 arg)
    {
        return this.ResultCreate();
    }
    public virtual async ValueTask<IResult<Dictionary<string, List<Test002>>>> Test0013(Session session, Dictionary<string, List<Test002>> arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test002>> Test0014(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test002>> Test0015(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test002>> Test0016(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test002>> Test0017(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test002>> Test0018(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test002>> Test0019(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test002>> Test0020(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test002>> Test0021(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test002>> Test0022(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test002>> Test0023(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test002>> Test0024(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test002>> Test0025(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test002>> Test0026(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test002>> Test0027(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test002>> Test0028(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test002>> Test0029(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test002>> Test0030(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test002>> Test0031(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test002>> Test0032(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test002>> Test00333(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test002>> Test0034(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test002>> Test0035(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test002>> Test0036(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test002>> Test0037(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test002>> Test0038(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test002>> Test0039(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test002>> Test0040(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test002>> Test0041(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test002>> Test0042(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test002>> Test0043(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test002>> Test0044(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test002>> Test0045(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test002>> Test0046(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test002>> Test0047(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test002>> Test0048(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test002>> Test0049(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test002>> Test0050(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test002>> Test0051(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test002>> Test0052(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test002>> Test0053(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test002>> Test0054(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }
    public virtual async ValueTask<IResult<Test002>> Test0055(Session session, Test002 arg)
    {
        return this.ResultCreate(arg);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="arg">argargargarg</param>
    /// <param name="arg2">arg2!!!</param>
    /// <returns></returns>
    public virtual async ValueTask<dynamic> Test007(Test002 arg, List<string> arg2) => this.ResultCreate();

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
    /// Test001Test001Tes!!!
    /// t001Test001Test001Test001!!!
    /// </summary>
    public struct Test111
    {
        /// <summary>
        /// Test004 MENU_ITEMMENU_ITEMMENU_ITEM@@@
        /// </summary>
        public string MENU_ITEM { get; set; }

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
        /// <summary>
        /// CCCCCCCCCCCCCCCCCCCCCCC!!!
        /// </summary>
        public Test00100 C { get; set; }

        /// <summary>
        /// DDD
        /// </summary>
        [@Scale(Size = 2)]
        [@Size(Max = 900.88)]
        public decimal? D { get; set; }

        public bool E { get; set; }

        /// <summary>
        /// FF
        /// </summary>
        [@CheckNull(CheckValueType = true)]
        public DateTime F { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public MyEnum? myEnum { get; set; }

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

        /// <summary>
        /// Test0010 Test0010 Test0010 Test0010
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public struct Test00100
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