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
using Business;
using Business.Core.Document;

#region Socket Support

/// <summary>
/// result
/// </summary>
/// <typeparam name="Type"></typeparam>
public struct ResultObject<Type> : IResult<Type>
{
    public static readonly System.Type ResultTypeDefinition = typeof(ResultObject<>).GetGenericTypeDefinition();

    /// <summary>
    /// Activator.CreateInstance
    /// </summary>
    /// <param name="dataType"></param>
    /// <param name="data"></param>
    /// <param name="state"></param>
    /// <param name="message"></param>
    /// <param name="genericDefinition"></param>
    /// <param name="checkData"></param>
    public ResultObject(System.Type dataType, Type data, int state = 1, string message = null, System.Type genericDefinition = null, bool checkData = true)
    {
        this.DataType = dataType;
        this.Data = data;
        this.State = state;
        this.Message = message;
        this.HasData = checkData ? !Equals(null, data) : false;
        this.Callback = default;

        this.GenericDefinition = genericDefinition;
    }

    /// <summary>
    /// MessagePack.MessagePackSerializer.Serialize(this)
    /// </summary>
    /// <param name="data"></param>
    /// <param name="state"></param>
    /// <param name="message"></param>
    public ResultObject(Type data, int state = 1, string message = null)
    {
        this.Data = data;
        this.State = state;
        this.Message = message;
        this.HasData = !Equals(null, data);

        this.Callback = null;
        this.DataType = null;
        this.GenericDefinition = null;
    }

    /// <summary>
    /// The results of the state is greater than or equal to 1: success, equal to 0: system level exceptions, less than 0: business class error.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("S")]
    public int State { get; set; }

    /// <summary>
    /// Success can be null
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("M")]
    public string Message { get; set; }

    /// <summary>
    /// Specific dynamic data objects
    /// </summary>
    dynamic IResult.Data { get => Data; }

    /// <summary>
    /// Specific Byte/Json data objects
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("D")]
    public Type Data { get; set; }

    /// <summary>
    /// Whether there is value
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("H")]
    public bool HasData { get; set; }

    /// <summary>
    /// Gets the token of this result, used for callback
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [System.Text.Json.Serialization.JsonPropertyName("B")]
    public string Callback { get; set; }

    [MessagePack.IgnoreMember]
    [System.Text.Json.Serialization.JsonIgnore]
    public System.Type DataType { get; set; }

    [MessagePack.IgnoreMember]
    [System.Text.Json.Serialization.JsonIgnore]
    public System.Type GenericDefinition { get; }

    /// <summary>
    /// Json format
    /// </summary>
    /// <returns></returns>
    public override string ToString() => Help.JsonSerialize(this);

    /// <summary>
    /// Json format Data
    /// </summary>
    /// <returns></returns>
    public string ToDataString() => Help.JsonSerialize(this.Data);

    /// <summary>
    /// ProtoBuf format
    /// </summary>
    /// <returns></returns>
    public byte[] ToBytes() => MessagePack.MessagePackSerializer.Serialize(this);

    /// <summary>
    /// ProtoBuf format Data
    /// </summary>
    /// <returns></returns>
    public byte[] ToDataBytes() => MessagePack.MessagePackSerializer.Serialize(this.Data);
}

public struct ReceiveData
{
    /// <summary>
    /// business
    /// </summary>
    public string a;

    /// <summary>
    /// cmd
    /// </summary>
    public string c;

    /// <summary>
    /// token
    /// </summary>
    public string t;

    /// <summary>
    /// data
    /// </summary>
    public byte[] d;

    /// <summary>
    /// callback
    /// </summary>
    public string b;
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
[Use]
public struct Token : Business.Core.Auth.IToken
{
    [System.Text.Json.Serialization.JsonPropertyName("K")]
    public string Key { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("R")]
    public string Remote { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("P")]
    public string Path { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public string Callback { get; set; }
}

/// <summary>
/// my session
/// </summary>
[SessionCheck]
[Use(true, Token = true)]
public struct Session
{
    public string Account { get; set; }
    public int Nick { get; set; }
    public List<string> Roles { get; set; }
}

[Command(Group = "j")]
[@JsonArg(Group = "j")]
[Command(Group = "s")]
[@MessagePackArg(Group = "s")]
[Logger]
public abstract class BusinessBase : BusinessBase<ResultObject<object>>
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

        Console.WriteLine($"Date: {DateTimeOffset.Now}");
        Console.WriteLine(System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
        Console.WriteLine($"LogPath: {LogPath}");

        MessagePack.MessagePackSerializer.DefaultOptions = MessagePack.Resolvers.ContractlessStandardResolver.Options.WithResolver(MessagePack.Resolvers.CompositeResolver.Create(new MessagePack.Formatters.IMessagePackFormatter[] { new MessagePack.Formatters.IgnoreFormatter<Type>(), new MessagePack.Formatters.IgnoreFormatter<System.Reflection.MethodBase>(), new MessagePack.Formatters.IgnoreFormatter<System.Reflection.MethodInfo>(), new MessagePack.Formatters.IgnoreFormatter<System.Reflection.PropertyInfo>(), new MessagePack.Formatters.IgnoreFormatter<System.Reflection.FieldInfo>() }, new MessagePack.IFormatterResolver[] { MessagePack.Resolvers.ContractlessStandardResolver.Instance }));
    }

    static void InitRedis()
    {
        //RedisHelper.Initialization(new CSRedis.CSRedisClient("mymaster,password=123456,prefix=", new[] { "192.168.1.121:26379", "192.168.1.122:26379", "192.168.1.123:26379" }));

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
    /// "context", "socket", "httpFile" 
    /// </summary>
    internal static readonly string[] contextParameterNames = new string[] { "context", "socket", "httpFile" };

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

        Bootstrap.Create()
            .UseType(contextParameterNames)
            .IgnoreSet(new Ignore(IgnoreMode.Arg), contextParameterNames)
            .LoggerSet(new LoggerAttribute(canWrite: false), contextParameterNames)
            .UseDoc(docDir, new Config { Debug = true, Benchmark = true, SetToken = true, Testing = true, Group = "j", GroupSelect = "j", GroupEnable = true })
            .Build();

        //writ url to page
        DocUI.Write(docDir, debug: true);
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
        var keepAliveInterval = webSocketcfg.GetValue("KeepAliveInterval", 120);
        receiveBufferSize = webSocketcfg.GetValue("ReceiveBufferSize", 4096);
        maxDegreeOfParallelism = webSocketcfg.GetValue("MaxDegreeOfParallelism", -1);
        //var allowedOrigins = webSocketcfg.GetSection("AllowedOrigins").GetChildren();

        var webSocketOptions = new WebSocketOptions()
        {
            KeepAliveInterval = TimeSpan.FromSeconds(keepAliveInterval),
            ReceiveBufferSize = receiveBufferSize
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

    public static int receiveBufferSize;

    public static int maxDegreeOfParallelism;

    public static readonly System.Collections.Concurrent.ConcurrentDictionary<string, WebSocket> Sockets = new System.Collections.Concurrent.ConcurrentDictionary<string, WebSocket>();

    //static T GetValue<T>(IConfiguration configuration, string key, T defaultValue = default)
    //{
    //    try
    //    {
    //        return configuration.GetValue(key, defaultValue);
    //    }
    //    catch { return defaultValue; }
    //}

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
            var buffer = new byte[receiveBufferSize];
            var socketResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!socketResult.CloseStatus.HasValue)
            {
                try
                {
                    var receiveData = MessagePack.MessagePackSerializer.Deserialize<ReceiveData>(buffer);

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
                        var b = receiveData.b ?? receiveData.c;

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
                            Callback = b
                        }, "session")
                        );

                        // Socket set callback
                        if (null != result)
                        {
                            if (typeof(IResult).IsAssignableFrom(result.GetType()))
                            {
                                var result2 = result as IResult;
                                result2.Callback = b;

                                var data = ResultFactory.ResultCreateToDataBytes(result2).ToBytes();

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
            Parallel.ForEach(Sockets, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }, async c =>
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
            Parallel.ForEach(id, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }, async c =>
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

        string c, t, d, g = null, value;
        g = "j";//fixed grouping
        //g = route.Group;
        IDictionary<string, string> parameters;

        switch (this.Request.Method)
        {
            case "GET":
                //requestData = new RequestData(this.Request.Query);
                //c = route.Command ?? this.Request.Query["c"];
                //t = this.Request.Query["t"];
                //d = this.Request.Query["d"];
                //g = this.Request.Query["g"];
                //b = this.Request.Query["b"];
                parameters = this.Request.Query.ToDictionary(k => k.Key, v =>
                {
                    var v2 = (string)v.Value;
                    return !string.IsNullOrEmpty(v2) ? v2 : null;
                });
                c = route.Command ?? (parameters.TryGetValue("c", out value) ? value : null);
                t = parameters.TryGetValue("t", out value) ? value : null;
                d = parameters.TryGetValue("d", out value) ? value : null;
                break;
            case "POST":
                {
                    //if (this.Request.HasFormContentType)
                    //requestData = new RequestData(await this.Request.ReadFormAsync());
                    //var form = await this.Request.ReadFormAsync();
                    //c = route.Command ?? form["c"];
                    //t = form["t"];
                    //d = form["d"];
                    //g = form["g"];
                    //b = form["b"];
                    parameters = (await this.Request.ReadFormAsync()).ToDictionary(k => k.Key, v =>
                    {
                        var v2 = (string)v.Value;
                        return !string.IsNullOrEmpty(v2) ? v2 : null;
                    });
                    c = route.Command ?? (parameters.TryGetValue("c", out value) ? value : null);
                    t = parameters.TryGetValue("t", out value) ? value : null;
                    d = parameters.TryGetValue("d", out value) ? value : null;
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
            return await DocUI.Benchmark(arg);
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

        var token = new Token //token
        {
            Key = t,
            Remote = string.Format("{0}:{1}", this.HttpContext.Connection.RemoteIpAddress.ToString(), this.HttpContext.Connection.RemotePort),
            Path = this.Request.Path.Value,
        };

        var result = null != route.Command ?
                // Normal routing mode
                await cmd.AsyncCall(
                    //the data of this request, allow null.
                    parameters,
                    //the incoming use object
                    new UseEntry(this, Common.contextParameterNames), //context
                    new UseEntry(token, "session")) :
                // Framework routing mode
                await cmd.AsyncCall(
                    //the data of this request, allow null.
                    cmd.HasArgSingle ? new object[] { d } : d.TryJsonDeserializeStringArray(),
                    //the incoming use object
                    new UseEntry(this, Common.contextParameterNames), //context
                    new UseEntry(token, "session"));

        return result;
    }
}