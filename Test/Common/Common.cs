using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using System.Net.WebSockets;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Business.Core;
using Business.Core.Utils;
using Business.Core.Annotations;
using Business.Core.Result;
using Business.Utils;
using Business;
using Business.Core.Document;

#region Socket Support

/// <summary>
/// This is a parameter package, used to transform parameters
/// </summary>
/// <typeparam name="OutType"></typeparam>
public class Arg<OutType> : Arg<OutType, dynamic>
{
    public static implicit operator Arg<OutType>(string value) => new Arg<OutType>() { In = value };
    public static implicit operator Arg<OutType>(byte[] value) => new Arg<OutType>() { In = value };
    public static implicit operator Arg<OutType>(OutType value) => new Arg<OutType>() { In = value };
    public override dynamic ToOut(dynamic value) => MessagePack.MessagePackSerializer.Deserialize<OutType>(value);
}

public class ResultObject<Type> : Business.Core.Result.ResultObject<Type>
{
    public static readonly System.Type ResultTypeDefinition = typeof(ResultObject<>).GetGenericTypeDefinition();

    public ResultObject(System.Type dataType, Type data, int state = 1, string message = null, System.Type genericDefinition = null, bool checkData = true)
        : base(dataType, data, state, message, genericDefinition, checkData) { }

    public ResultObject(Type data, int state = 1, string message = null) : base(data, state, message) { }

    [MessagePack.IgnoreMember]
    [System.Text.Json.Serialization.JsonIgnore]
    public override System.Type DataType { get => base.DataType; set => base.DataType = value; }

    [MessagePack.IgnoreMember]
    [System.Text.Json.Serialization.JsonIgnore]
    public override System.Type GenericDefinition => base.GenericDefinition;

    public override byte[] ToBytes() => MessagePack.MessagePackSerializer.Serialize(this);

    public override byte[] ToDataBytes() => MessagePack.MessagePackSerializer.Serialize(this.Data);
}

public struct ReceiveData
{
    /// <summary>
    /// business
    /// </summary>
    public string a { get; set; }

    /// <summary>
    /// cmd
    /// </summary>
    public string c { get; set; }

    /// <summary>
    /// token
    /// </summary>
    public string t { get; set; }

    /// <summary>
    /// data
    /// </summary>
    public byte[] d { get; set; }

    /// <summary>
    /// callback
    /// </summary>
    public string b { get; set; }
}

#endregion

/// <summary>
/// Business constructor would match given arguments
/// </summary>
public struct Host
{
    public string Addresses { get; set; }

    public IConfigurationSection AppSettings { get; set; }

    public IHostingEnvironment ENV { get; set; }

    public IHttpClientFactory HttpClientFactory { get; set; }
}

/// <summary>
/// my token
/// </summary>
[TokenCheck]
public class Token : Business.Core.Auth.Token
{
    [System.Text.Json.Serialization.JsonPropertyName("P")]
    public string Path { get; set; }
}

/// <summary>
/// my session
/// </summary>
public class Session
{
    public string Account { get; set; }
    public int Nick { get; set; }
    public List<string> Roles { get; set; }
}

/// <summary>
/// Session arg object
/// </summary>
[SessionCheck]
public class SessionArg : Arg<Session, Token> { }

[Command(Group = "j")]
[@JsonArg(Group = "j")]
[Command(Group = "s")]
[@MessagePackArg(Group = "s")]
[Logger]
public abstract class BusinessBase : BusinessBase<ResultObject<string>>
{
    public BusinessBase()
    {
        this.Logger = new Logger(async (Logger.LoggerData x) =>
        {
            try
            {
                //var logs = x.Select(c =>
                //{
                //    //if (c.Type == Logger.LoggerType.Exception)
                //    //{
                //    //    Console.WriteLine(c.JsonSerialize());
                //    //}
                //    Console.WriteLine(c.JsonSerialize());
                //    return c;
                //}).ToList();

                Console.WriteLine(x.JsonSerialize());

                //System.Console.WriteLine(logs.JsonSerialize());
            }
            catch (Exception ex)
            {
                Help.ExceptionWrite(ex, true, true);
            }
        })
        {
            //Batch = new Logger.BatchOptions
            //{
            //    Interval = System.TimeSpan.FromSeconds(6),
            //    MaxNumber = 100
            //}
        };
    }
}

public static class Common
{
    public static readonly string LogPath = System.IO.Path.Combine(System.IO.Path.DirectorySeparatorChar.ToString(), "data", $"{AppDomain.CurrentDomain.FriendlyName}.log.txt");

    public static Host Host = new Host();

    static Common()
    {
        Host.HttpClientFactory = new ServiceCollection()
            .AddHttpClient("any").ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
            {
                AllowAutoRedirect = false,
                UseDefaultCredentials = true,
            }).Services
            .BuildServiceProvider().GetService<IHttpClientFactory>();
        AppContext.SetSwitch("System.Net.Http.UseSocketsHttpHandler", false);

        //ThreadPool.SetMinThreads(50, 50);
        //ThreadPool.GetMinThreads(out int workerThreads, out int completionPortThreads);
        //ThreadPool.GetMaxThreads(out int workerThreads2, out int completionPortThreads2);

        //Console.WriteLine($"Min {workerThreads}, {completionPortThreads}");
        //Console.WriteLine($"Max {workerThreads2}, {completionPortThreads2}");

        AppDomain.CurrentDomain.UnhandledException += (sender, e) => (e.ExceptionObject as Exception)?.ExceptionWrite(true, true, LogPath);

        Console.WriteLine(System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
        Console.WriteLine($"LogPath: {LogPath}");

        MessagePack.Resolvers.CompositeResolver.RegisterAndSetAsDefault(MessagePack.Resolvers.ContractlessStandardResolver.Instance);
    }

    static void InitRedis()
    {
        RedisHelper.Initialization(new CSRedis.CSRedisClient("mymaster,password=123456,prefix=", new[] { "192.168.1.121:26379", "192.168.1.122:26379", "192.168.1.123:26379" }));

        /* https://redis.io/topics/sentinel
         * If everything appears to be normal for 30 second, the TILT mode is exited.

mkdir -p /root/workspace/redis && chown -R 1001 /root/workspace/redis && mkdir -p /root/workspace/redis-sentinel && chown -R 1001 /root/workspace/redis-sentinel
         
docker run -d --name redis -e REDIS_PASSWORD="123456" -e REDIS_REPLICATION_MODE=master -v /root/workspace/redis:/bitnami/redis/data -p 6379:6379 bitnami/redis:latest

docker run -d --name redis -e REDIS_PASSWORD="123456" -e REDIS_REPLICATION_MODE=slave -e REDIS_MASTER_HOST=192.168.1.121 -e REDIS_MASTER_PORT_NUMBER=6379 -e REDIS_MASTER_PASSWORD=123456 -v /root/workspace/redis:/bitnami/redis/data -p 6379:6379 bitnami/redis:latest

docker run -itd --name redis-sentinel -e REDIS_MASTER_HOST=192.168.1.121 -e REDIS_MASTER_PORT_NUMBER=6379 -e REDIS_MASTER_PASSWORD="123456" -v /root/workspace/redis-sentinel:/bitnami/redis-sentinel/conf -p 26379:26379 bitnami/redis-sentinel:latest

        docker run -it --rm bitnami/redis redis-cli -h 172.17.0.1 -p 6379 -a 123456 info Replication
        docker run -it --rm bitnami/redis redis-cli -h 172.17.0.1 -p 6379 -a 123456 set aaa 111
        */

        /* test sentinel
        Exception ex = null;

        do
        {
            try
            {
                RedisHelper.Set("aaa", "a1");
                ex = null;
            }
            catch (Exception exception)
            {
                ex = exception;
                ex.Console();
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(3));
            }
        } while (null != ex);
        */
    }

    /// <summary>
    /// Call this method after environment initialization is complete
    /// </summary>
    /// <param name="app"></param>
    /// <param name="docDir"></param>
    public static void InitBusiness(this IApplicationBuilder app, string docDir = null)
    {
        /*
#if DEBUG
        var con = Startup.appSettings.GetSection("Redis").GetSection("ConnectionString").Value;
#else
        var con = Startup.appSettings.GetSection("Redis").GetSection("ConnectionString2").Value;
#endif
        System.Console.WriteLine($"Redis={con}");
        var csredis = new CSRedis.CSRedisClient(con);
        RedisHelper.Initialization(csredis);
        */

        //InitRedis();

        Bind.CreateAll()
            .UseType("context", "socket", "httpFile")
            .IgnoreSet(new Ignore(IgnoreMode.Arg), "context", "socket", "httpFile")
            .LoggerSet(new LoggerAttribute(canWrite: false), "context", "socket", "httpFile")
            .UseDoc(docDir, new Config { Debug = true, Benchmark = true, SetToken = true, Group = "j", Testing = true, GroupEnable = true })
            .LoadBusiness(new object[] { Host });

        //writ url to page
        DocUI.Write(docDir, update: true);
        //add route
        app.UseMvc(routes =>
        {
            foreach (var item in Configer.BusinessList)
            {
                routes.MapRoute(
                name: item.Key,
                template: $"{item.Key}/{{*path}}",
                defaults: new { controller = "Business", action = "Call" });
            }
        });

        #region AcceptWebSocket

        var webSocketcfg = Host.AppSettings.GetSection("WebSocket");
        var keepAliveInterval = GetValue(webSocketcfg, "KeepAliveInterval", 120);
        ReceiveBufferSize = GetValue(webSocketcfg, "ReceiveBufferSize", 4096);
        MaxDegreeOfParallelism = GetValue(webSocketcfg, "MaxDegreeOfParallelism", -1);
        //var allowedOrigins = webSocketcfg.GetSection("AllowedOrigins").GetChildren();

        var webSocketOptions = new WebSocketOptions()
        {
            KeepAliveInterval = TimeSpan.FromSeconds(keepAliveInterval),
            ReceiveBufferSize = ReceiveBufferSize
        };

        //foreach (var item in allowedOrigins)
        //{
        //    webSocketOptions.AllowedOrigins.Add(item.Value);
        //}

        app.UseWebSockets(webSocketOptions);

        app.Use(async (context, next) =>
        {
            if (context.Request.Path == "/ws")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    using (var webSocket = await context.WebSockets.AcceptWebSocketAsync())
                    {
                        Sockets.TryAdd(context.Connection.Id, webSocket);
#if DEBUG
                        Console.WriteLine($"Add:{context.Connection.Id} Sockets:{Sockets.Count}");
#endif
                        await Keep(context, webSocket);

                        Sockets.TryRemove(context.Connection.Id, out _);
#if DEBUG
                        Console.WriteLine($"Remove:{context.Connection.Id} Sockets:{Sockets.Count}");
#endif
                    }
                }
                else
                {
                    context.Response.StatusCode = 400;
                }
            }
            else
            {
                await next();
            }
        });

        #endregion
    }

    #region WebSocket

    public static int ReceiveBufferSize;

    public static int MaxDegreeOfParallelism;

    public static readonly System.Collections.Concurrent.ConcurrentDictionary<string, WebSocket> Sockets = new System.Collections.Concurrent.ConcurrentDictionary<string, WebSocket>();

    static T GetValue<T>(IConfiguration configuration, string key, T defaultValue = default)
    {
        try
        {
            return configuration.GetValue<T>(key);
        }
        catch { return defaultValue; }
    }

    public static async Task Keep(HttpContext context, WebSocket webSocket)
    {
        //var auth = context.Request.Headers["u"].ToString();

        //if (string.IsNullOrWhiteSpace(auth))
        //{
        //    return;
        //}

        var id = context.Connection.Id;

        try
        {
            var buffer = new byte[ReceiveBufferSize];
            var socketResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!socketResult.CloseStatus.HasValue)
            {
                try
                {
                    var receiveData = buffer.TryBinaryDeserialize<ReceiveData>();

                    dynamic result;

                    //* test data
                    //receiveData.a = "API";
                    //receiveData.c = "Test001";
                    //receiveData.t = "token";
                    //receiveData.d = new Args.Test001 { A = "error", B = "bbb" }.BinarySerialize();
                    //receiveData.b = "bbb";
                    //*

                    if (string.IsNullOrWhiteSpace(receiveData.a) || !Configer.BusinessList.TryGetValue(receiveData.a, out IBusiness business))
                    {
                        result = ResultObject<string>.ResultTypeDefinition.ErrorBusiness(receiveData.a);// Bind.BusinessError(ResultObject<string>.ResultTypeDefinition, receiveData.a);
                        await SendAsync(result.ToBytes(), id);
                    }
                    else
                    {
                        result = await business.Command.AsyncCall(
                        //the cmd of this request.
                        receiveData.c,
                        //the data of this request, allow null.
                        new object[] { receiveData.d },
                        //the group of this request.
                        "s", //fixed grouping
                        //the incoming use object
                        new UseEntry(context, "context"), //context
                        new UseEntry(webSocket, "socket"), //webSocket
                        new UseEntry(new Token //token
                        {
                            Key = receiveData.t,
                            Remote = string.Format("{0}:{1}", context.Connection.RemoteIpAddress.ToString(), context.Connection.RemotePort),
                            Callback = receiveData.b
                        })
                        );

                        if (null != result)
                        {
                            if (typeof(IResult).IsAssignableFrom(result.GetType()))
                            {
                                var result2 = result as IResult;
                                result2.Callback = receiveData.b;

                                var data = ResultFactory.ResultCreateToDataBytes(result2).ToBytes();
                                /* test
                                var result3 = MessagePack.MessagePackSerializer.Deserialize<ResultObject<byte[]>>(data);
                                var result4 = MessagePack.MessagePackSerializer.Deserialize<BusinessMember2.Result>(result3.Data);
                                */

                                //var result4 = MessagePack.MessagePackSerializer.Deserialize<ResultObject<byte[]>>(data);
                                //var result5 = MessagePack.MessagePackSerializer.Deserialize<BusinessMember2.Test001Result>(result4.Data);

                                await SendAsync(data, id);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Help.ExceptionWrite(ex, true, true, LogPath);
                    var result = ResultFactory.ResultCreate(ResultObject<string>.ResultTypeDefinition, 0, Convert.ToString(ex));
                    await SendAsync(result.ToBytes(), id);
                }

                if (webSocket.State != WebSocketState.Open)
                {
                    break;
                }

                socketResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            if (webSocket.State == WebSocketState.Open)
            {
                await webSocket.CloseAsync(socketResult.CloseStatus.Value, socketResult.CloseStatusDescription, CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            Help.ExceptionWrite(ex, true, true, LogPath);
            var result = ResultFactory.ResultCreate(ResultObject<string>.ResultTypeDefinition, 0, Convert.ToString(ex));
            await SendAsync(result.ToBytes(), id);
        }
    }

    #region SendAsync

    public static async Task SendAsync(byte[] bytes, params string[] id) => await SendAsync(bytes, WebSocketMessageType.Binary, true, id);

    public static async Task SendAsync(byte[] bytes, WebSocketMessageType messageType = WebSocketMessageType.Binary, params string[] id) => await SendAsync(bytes, messageType, true, id);

    public static async Task SendAsync(byte[] bytes, WebSocketMessageType messageType = WebSocketMessageType.Binary, bool endOfMessage = true, params string[] id)
    {
        if (null == id || 0 == id.Length)
        {
            Parallel.ForEach(Sockets, new ParallelOptions { MaxDegreeOfParallelism = MaxDegreeOfParallelism }, async c =>
            {
                if (c.Value.State != WebSocketState.Open) { return; }

                await c.Value.SendAsync(new ArraySegment<byte>(bytes), messageType, endOfMessage, CancellationToken.None);
            });
        }
        else if (1 == id.Length)
        {
            var c = id[0];

            if (string.IsNullOrWhiteSpace(c)) { return; }

            if (!Sockets.TryGetValue(c, out WebSocket webSocket)) { return; }

            if (webSocket.State != WebSocketState.Open) { return; }

            await webSocket.SendAsync(new ArraySegment<byte>(bytes), messageType, endOfMessage, CancellationToken.None);
        }
        else
        {
            Parallel.ForEach(id, new ParallelOptions { MaxDegreeOfParallelism = MaxDegreeOfParallelism }, async c =>
            {
                if (string.IsNullOrWhiteSpace(c)) { return; }

                if (!Sockets.TryGetValue(c, out WebSocket webSocket)) { return; }

                if (webSocket.State != WebSocketState.Open) { return; }

                await webSocket.SendAsync(new ArraySegment<byte>(bytes), messageType, endOfMessage, CancellationToken.None);
            });
        }
    }

    #endregion

    #endregion
}

/// <summary>
/// A class for an MVC controller with view support.
/// </summary>
[Use]
//Internal object do not write logs
[Logger(canWrite: false)]
[RequestSizeLimit(long.MaxValue)]
//int.MaxValue bug https://github.com/aspnet/AspNetCore/issues/13719
[RequestFormLimits(KeyLengthLimit = 1_009_100_000, ValueCountLimit = 1_009_100_000, ValueLengthLimit = 1_009_100_000, MultipartHeadersLengthLimit = int.MaxValue, MultipartBodyLengthLimit = long.MaxValue, MultipartBoundaryLengthLimit = int.MaxValue)]
public class BusinessController : Controller
{
    /// <summary>
    /// Call
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [HttpPost]
    [EnableCors("any")]
    public async Task<dynamic> Call()
    {
        #region route

        if (!Configer.Routes.TryGetValue(this.Request.Path.Value.TrimStart('/'), out Configer.Route route) || !Configer.BusinessList.TryGetValue(route.Business, out IBusiness business)) { return this.NotFound(); }

        #endregion

        string c, t, d, g, b = null;
        g = "j";//fixed grouping
        //g = route.Group;

        switch (this.Request.Method)
        {
            case "GET":
                //requestData = new RequestData(this.Request.Query);
                c = route.Command ?? this.Request.Query["c"];
                t = this.Request.Query["t"];
                d = this.Request.Query["d"];
                //g = this.Request.Query["g"];
                //b = this.Request.Query["b"];
                break;
            case "POST":
                {
                    //if (this.Request.HasFormContentType)
                    //requestData = new RequestData(await this.Request.ReadFormAsync());
                    var form = await this.Request.ReadFormAsync();
                    c = route.Command ?? form["c"];
                    t = form["t"];
                    d = form["d"];
                    //g = form["g"];
                    //b = form["b"];
                }
                break;
            default: return this.NotFound();
        }

        #region benchmark

        if ("benchmark" == c)
        {
            var arg = d.TryJsonDeserialize<DocUI.benchmarkArg>();
            if (default(DocUI.benchmarkArg).Equals(arg)) { return new ArgumentNullException(nameof(arg)).Message; }
            arg.host = $"{this.Request.Scheme}://localhost:{this.HttpContext.Connection.LocalPort}/{business.Configer.Info.BusinessName}";
            return await DocUI.benchmark(arg);
        }

        #endregion

        //this.Request.Headers["X-Real-IP"].FirstOrDefault() 

        var cmd = business.Command.GetCommand(
            //the cmd of this request.
            c,
            //the group of this request.
            g);

        if (null == cmd)
        {
            return Help.ErrorCmd(business, c);
        }

        var result = await cmd.AsyncCall(
            //the data of this request, allow null.
            cmd.HasArgSingle ? new object[] { d } : d.TryJsonDeserializeObjectArray(),
            //the incoming use object
            new UseEntry(this, "context", "httpFile"), //context
            new UseEntry(new Token //token
            {
                Key = t,
                Remote = string.Format("{0}:{1}", this.HttpContext.Connection.RemoteIpAddress.ToString(), this.HttpContext.Connection.RemotePort),
                Path = this.Request.Path.Value,
                //Callback = b
            })
            );

        if (!object.Equals(null, result))
        {
            if (typeof(IResult).IsAssignableFrom(result.GetType()))
            {
                var result2 = result as IResult;
                result2.Callback = b;
                return result2;
            }
        }

        return result;
    }
}