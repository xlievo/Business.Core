using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Microsoft.Net.Http.Headers;
using Business;
using Business.Attributes;
using Business.Auth;
using Business.Utils;
using Business.Result;
//using Microsoft.AspNetCore.Server.Kestrel.Core;

#region Socket Support

public class MessagePackArgAttribute : ArgumentAttribute
{
    public MessagePackArgAttribute(int state = -13, string message = null) : base(state, message) => this.CanNull = false;

    public override async ValueTask<IResult> Proces(dynamic value, IArg arg)
    {
        var result = CheckNull(this, value);
        if (!result.HasData) { return result; }

        try
        {
            return this.ResultCreate(arg.ToOut(value));
        }
        catch { return this.ResultCreate(State, Message ?? $"Arguments {this.Nick} MessagePack deserialize error"); }
    }
}

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

public class ResultObject<Type> : Business.Result.ResultObject<Type>
{
    public static readonly System.Type ResultTypeDefinition = typeof(ResultObject<>).GetGenericTypeDefinition();

    public ResultObject(System.Type dataType, Type data, int state = 1, string message = null, System.Type genericDefinition = null, bool checkData = true)
        : base(dataType, data, state, message, genericDefinition, checkData) { }

    public ResultObject(Type data, int state = 1, string message = null) : base(data, state, message) { }

    [MessagePack.IgnoreMember]
    public override System.Type DataType { get => base.DataType; set => base.DataType = value; }

    [MessagePack.IgnoreMember]
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

public class Program
{
    public static string LogPath = System.IO.Path.Combine(System.IO.Path.DirectorySeparatorChar.ToString(), "data", $"{System.AppDomain.CurrentDomain.FriendlyName}.log.txt");

    public static string addresses = string.Empty;

    public static void Main(string[] args)
    {
        System.AppDomain.CurrentDomain.UnhandledException += (sender, e) => (e.ExceptionObject as System.Exception)?.ExceptionWrite(true, true, LogPath);

        System.Console.WriteLine($"LogPath:{LogPath}");

        var host = CreateWebHostBuilder(args).Build();
        addresses = host.ServerFeatures.Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>().Addresses.FirstOrDefault();
        host.Run();
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .UseKestrel()
            .UseStartup<Startup>();
    //.ConfigureKestrel((context, options) =>
    //{
    //    // Set properties and call methods on options
    //});
}

public class Startup
{
    public IConfigurationSection appSettings = null;
    public IHostingEnvironment env;
    public System.Net.Http.IHttpClientFactory httpClientFactory;

    public static int ReceiveBufferSize;

    public static int MaxDegreeOfParallelism;

    public static readonly System.Collections.Concurrent.ConcurrentDictionary<string, WebSocket> Sockets = new System.Collections.Concurrent.ConcurrentDictionary<string, WebSocket>();

    public static T GetValue<T>(IConfiguration configuration, string key, T defaultValue = default)
    {
        try
        {
            return configuration.GetValue<T>(key);
        }
        catch { return defaultValue; }
    }

    public Startup(IConfiguration configuration)
    {
        System.Threading.ThreadPool.SetMinThreads(50, 50);
        System.Threading.ThreadPool.GetMinThreads(out int workerThreads, out int completionPortThreads);
        System.Threading.ThreadPool.GetMaxThreads(out int workerThreads2, out int completionPortThreads2);

        System.Console.WriteLine($"Min {workerThreads}, {completionPortThreads}");
        System.Console.WriteLine($"Max {workerThreads2}, {completionPortThreads2}");

        Configuration = configuration;
        appSettings = Configuration.GetSection("AppSettings");

        //var doc = GetSwagger().JsonSerialize();
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("any", builder =>
            {
                builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
            });
        });

        services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
            .AddJsonOptions(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                options.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Local;
            });

        httpClientFactory = services.AddHttpClient().BuildServiceProvider().GetService<IHttpClientFactory>();
        //AppContext.SetSwitch("System.Net.Http.UseSocketsHttpHandler", false);
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        this.env = env;

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        var wwwroot = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "wwwroot");

        if (!System.IO.Directory.Exists(wwwroot))
        {
            System.IO.Directory.CreateDirectory(wwwroot);
        }
        // Set up custom content types -associating file extension to MIME type
        var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
        //provider.Mappings[".yaml"] = "text/yaml";
        provider.Mappings[".doc"] = "application/json";
        app.UseDefaultFiles().UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(wwwroot),
            ContentTypeProvider = provider,
            OnPrepareResponse = c =>
            {
                if (c.File.Exists && string.Equals(".doc", System.IO.Path.GetExtension(c.File.Name)))
                {
                    //c.Context.Response.Headers[HeaderNames.CacheControl] = "public,max-age=600"; //600
                    c.Context.Response.Headers[HeaderNames.CacheControl] = "public, no-cache, no-store";
                    c.Context.Response.Headers[HeaderNames.Pragma] = "no-cache";
                    c.Context.Response.Headers[HeaderNames.Expires] = "-1";
                    //c.Context.Response.Headers[HeaderNames.CacheControl] = Configuration["StaticFiles:Headers:Cache-Control"];
                    //c.Context.Response.Headers[HeaderNames.Pragma] = Configuration["StaticFiles:Headers:Pragma"];
                    //c.Context.Response.Headers[HeaderNames.Expires] = Configuration["StaticFiles:Headers:Expires"];
                }

                c.Context.Response.Headers[HeaderNames.AccessControlAllowOrigin] = "*";
            }
        }).UseDirectoryBrowser();

        app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto }).UseAuthentication().UseCors("any");

        //==================First step==================//
        //1
        Configer.LoadBusiness(new object[] { new Host { Addresses = Program.addresses, AppSettings = appSettings, ENV = env, HttpClientFactory = httpClientFactory } });
        //Configer.UseType(typeof(HttpContext), typeof(WebSocket));
        //2
        Configer.UseType("context");
        Configer.IgnoreSet(new Ignore(IgnoreMode.Arg), "context");
        Configer.LoggerSet(new LoggerAttribute(canWrite: false), "context");
        //==================The third step==================//
        //3
        Configer.UseDoc(System.IO.Path.Combine(wwwroot));

        //==================The second step==================//
        //add route
        app.UseMvc(routes =>
        {
            foreach (var item in Configer.BusinessList)
            {
                routes.MapRoute(
                 name: item.Key,
                 template: string.Format("{0}/{{*path}}", item.Key),
                 defaults: new { controller = "Business", action = "Call" });

                //foreach (var group in item.Value.Configer.Doc.Members.Values)
                //{
                //    foreach (var member in group.Values)
                //    {
                //        var json = new Business.Document.DocArg
                //        {
                //            Id = member.Name,
                //            Title = member.Name,
                //            Type = "object",
                //            Description = member.Description,
                //            Children = new Dictionary<string, Business.Document.DocArg>
                //            {
                //                { "Input", new Business.Document.DocArg { Children = new Dictionary<string, Business.Document.DocArg>() } },
                //                { "Output", new Business.Document.DocArg { Children = new Dictionary<string, Business.Document.DocArg>() } }
                //            },
                //        };

                //        var args = member.Args as Dictionary<string, Business.Document.DocArg>;
                //        var tokens = args.Where(c => c.Value.Token).ToList();
                //        foreach (var token in tokens)
                //        {
                //            args.Remove(token.Key);
                //        }

                //        if (0 < tokens.Count)
                //        {
                //            json.Children["Input"].Children.Add("t", new Business.Document.DocArg
                //            {
                //                Id = $"{member.Name}.t",
                //                Title = "t (String)",
                //                Type = "string",
                //                Description = "API token",
                //            });
                //        }

                //        json.Children["Input"].Children.Add("d", new Business.Document.DocArg
                //        {
                //            Id = $"{member.Name}.d",
                //            Title = "d (JsonArray)",
                //            Type = "object",
                //            Children = args,
                //            Description = "API data",
                //        });

                //        //in
                //        // var data = json.JsonSerialize(Help.JsonSettings);

                //    }
                //}
            }
        });

        #region AcceptWebSocket

        MessagePack.Resolvers.CompositeResolver.RegisterAndSetAsDefault(MessagePack.Resolvers.ContractlessStandardResolver.Instance);

        var webSocketcfg = appSettings.GetSection("WebSocket");
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
                        System.Console.WriteLine($"Add:{context.Connection.Id} Sockets:{Sockets.Count}");
#endif
                        await Keep(context, webSocket);

                        Sockets.TryRemove(context.Connection.Id, out _);
#if DEBUG
                        System.Console.WriteLine($"Remove:{context.Connection.Id} Sockets:{Sockets.Count}");
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
    async Task Keep(HttpContext context, WebSocket webSocket)
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
                    receiveData.a = "API";
                    receiveData.c = "Test001";
                    receiveData.t = "token";
                    receiveData.d = new Args.Test001 { A = "error", B = "bbb" }.BinarySerialize();
                    receiveData.b = "bbb";
                    //*

                    if (string.IsNullOrWhiteSpace(receiveData.a) || !Configer.BusinessList.TryGetValue(receiveData.a, out IBusiness business))
                    {
                        result = Bind.BusinessError(ResultObject<string>.ResultTypeDefinition, receiveData.a);
                        await SendAsync(result.ToBytes(), id);
                    }
                    else
                    {
                        var dict = new System.Dynamic.ExpandoObject() as IDictionary<string, object>; // context objects
                        //dict.Add("Addresses", Program.Addresses);
                        //dict.Add("AppSettings", Startup.AppSettings);
                        //dict.Add("ENV", Startup.ENV);
                        dict.Add("HttpContext", context);
                        dict.Add("WebSocket", webSocket);

                        result = await business.Command.AsyncCall(
                        //the cmd of this request.
                        receiveData.c,
                        //the data of this request, allow null.
                        new object[] { receiveData.d },
                        //the group of this request.
                        "s", //fixed grouping
                        //the incoming use object
                        new UseEntry(dict, "context"), //controller
                        new UseEntry(new Token //token
                        {
                            Key = receiveData.t,
                            Remote = string.Format("{0}:{1}", context.Connection.RemoteIpAddress.ToString(), context.Connection.RemotePort),
                            Callback = receiveData.b
                        }, "session") //[Use(true)]
                        );

                        if (null != result)
                        {
                            if (typeof(IResult).IsAssignableFrom(result.GetType()))
                            {
                                result.Callback = receiveData.b;

                                var data = Business.Result.ResultFactory.ResultCreateToDataBytes(result).ToBytes();
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
                catch (System.Exception ex)
                {
                    Help.ExceptionWrite(ex, true, true, Program.LogPath);
                    var result = ResultFactory.ResultCreate(ResultObject<string>.ResultTypeDefinition, 0, System.Convert.ToString(ex));
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
        catch (System.Exception ex)
        {
            Help.ExceptionWrite(ex, true, true, Program.LogPath);
            var result = ResultFactory.ResultCreate(ResultObject<string>.ResultTypeDefinition, 0, System.Convert.ToString(ex));
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
[RequestFormLimits(ValueCountLimit = int.MaxValue, ValueLengthLimit = int.MaxValue, MultipartHeadersLengthLimit = int.MaxValue, MultipartBodyLengthLimit = long.MaxValue)]
public class BusinessController : Controller
{
    public BusinessController(System.Net.Http.IHttpClientFactory httpClientFactory) => Configer.MemberSet("httpClientFactory", httpClientFactory, true);

    /// <summary>
    /// Call
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    [HttpGet]
    [HttpPost]
    [EnableCors("any")]
    public async Task<dynamic> Call(string path)
    {
        if (null != path || !Configer.BusinessList.TryGetValue(this.Request.Path.Value.TrimStart('/').Split('/')[0], out IBusiness business)) { return this.NotFound(); }

        string c, t, d, g, b = null;

        switch (this.Request.Method)
        {
            case "GET":
                //requestData = new RequestData(this.Request.Query);
                c = this.Request.Query["c"];
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
                    c = form["c"];
                    t = form["t"];
                    d = form["d"];
                    //g = form["g"];
                    //b = form["b"];
                }
                break;
            default: return this.NotFound();
        }

        var dict = new System.Dynamic.ExpandoObject() as IDictionary<string, object>;
        //dict.Add("Addresses", Program.Addresses);
        //dict.Add("AppSettings", Startup.AppSettings);
        //dict.Add("ENV", Startup.ENV);
        dict.Add("Controller", this);

        g = "j";//fixed grouping

        //this.Request.Headers["X-Real-IP"].FirstOrDefault() 

        var cmd = business.Command.GetCommand(
            //the cmd of this request.
            c,
            //the group of this request.
            g);

        //var result = await Configer.BusinessList[this.Request.Path.Value.TrimStart('/').Split('/')[0]].Command.AsyncCall(
        var result = await cmd.AsyncCall(
            //the data of this request, allow null.
            cmd.HasArgSingle ? new object[] { d } : d.TryJsonDeserialize<object[]>(),
            //the incoming use object
            new UseEntry(dict, "context"), //context
            new UseEntry(new Token //token
            {
                Key = t,
                Remote = string.Format("{0}:{1}", this.HttpContext.Connection.RemoteIpAddress.ToString(), this.HttpContext.Connection.RemotePort),
                //Callback = b
            }, "session") //[Use(true)]
            );

        if (null != result)
        {
            if (typeof(IResult).IsAssignableFrom(result.GetType()))
            {
                result.Callback = b;
            }
        }

        return result;
    }
}