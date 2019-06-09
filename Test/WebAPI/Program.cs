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
using Microsoft.Net.Http.Headers;
using Business;
using Business.Attributes;
using Business.Auth;
using Business.Utils;
using Business.Result;
using Swagger.Doc;
using System.Collections;

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
    public static readonly System.Type ResultType = typeof(ResultObject<>).GetGenericTypeDefinition();

    public ResultObject(Type data, System.Type dataType, int state = 1, string message = null, System.Type genericType = null)
        : base(data, dataType, state, message, genericType) { }

    public ResultObject(Type data, int state = 1, string message = null) : this(data, null, state, message) { }

    [MessagePack.IgnoreMember]
    public override System.Type DataType { get => base.DataType; set => base.DataType = value; }

    [MessagePack.IgnoreMember]
    public override System.Type GenericType => base.GenericType;

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

public class Program
{
    public static void Main(string[] args) => CreateWebHostBuilder(args).Build().Run();

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>();
}

public class Startup
{
    public static IConfigurationSection appSettings = null;

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
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
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
        Configer.LoadBusiness();
        //Configer.UseType(typeof(HttpContext), typeof(WebSocket));
        //2
        Configer.UseType("context");
        Configer.IgnoreSet(new Ignore(IgnoreMode.Arg), "context");
        Configer.LoggerSet(new LoggerAttribute(canWrite: false), "context");

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
            }
        });

        //==================The third step==================//
        //3
        Configer.UseDoc(System.IO.Path.Combine(wwwroot));

        #region SwaggerDoc

        foreach (var item in Configer.BusinessList.Values)
        {
            if (null == item.Configer.Doc) { continue; }

            if (System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(wwwroot)))
            {
                var doc = GetSwaggerDoc(item).JsonSerialize();

                System.IO.File.WriteAllText(System.IO.Path.Combine(wwwroot, $"{item.Configer.Info.BusinessName}.doc"), doc, System.Text.Encoding.UTF8);
            }
        }

        #endregion

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
                    var webSocket = await context.WebSockets.AcceptWebSocketAsync();

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

                    ///* test data
                    receiveData.a = "API";
                    receiveData.c = "Test004";
                    receiveData.t = "token";
                    receiveData.d = new List<Args.Test001> { new Args.Test001 { A = "ok2", B = "bbb" } }.BinarySerialize();
                    receiveData.b = "bbb";
                    //*/

                    if (string.IsNullOrWhiteSpace(receiveData.a) || !Configer.BusinessList.TryGetValue(receiveData.a, out IBusiness business))
                    {
                        result = Bind.BusinessError(ResultObject<string>.ResultType, receiveData.a);
                        await SendAsync(result.ToBytes(), id);
                    }
                    else
                    {
                        var dict = new System.Dynamic.ExpandoObject() as IDictionary<string, object>; // context objects
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

                                await SendAsync(data, id);
                            }
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    var result = ResultFactory.ResultCreate(ResultObject<string>.ResultType, 0, System.Convert.ToString(ex));
                    await SendAsync(result.ToBytes(), id);
                    Help.ExceptionWrite(ex, true, true);
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
            var result = ResultFactory.ResultCreate(ResultObject<string>.ResultType, 0, System.Convert.ToString(ex));
            await SendAsync(result.ToBytes(), id);
            Help.ExceptionWrite(ex, true, true);
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

    /// <summary>
    /// Generating Swagger documents for business classes
    /// </summary>
    /// <param name="business"></param>
    /// <returns></returns>
    public static Root GetSwaggerDoc(IBusiness business)
    {
        var paths = new Dictionary<string, object>();

        foreach (var item in business.Configer.Doc.Members[business.Configer.Info.CommandGroupDefault].Values)
        {
            var methods = new Dictionary<string, object>();
            var parameters = new List<IDictionary<string, object>>();

            parameters.Add(new Dictionary<string, object>
            {
                { "name", "c" },
                { "in", "formData" },
                { "type", "string" },
                { "default", item.Name },
                { "description", "api name" },
                { "required",true },
                { "x-CheckNull", "Values are not allowed to be empty" },
            });

            item.ArgList.ForEach(c2 =>
            {
                if (!c2.UseType && c2.HasDefinition)
                {
                    return;
                }

                string name = string.Empty;
                string description = string.Empty;

                if (typeof(Token).IsAssignableFrom(c2.Type))
                {
                    name = "t";
                    description = "api token";
                }
                else
                {
                    name = $"d.{{{ (c2.Parent == item.Name ? c2.Path.Substring(c2.Parent.Length, c2.Path.Length - c2.Parent.Length).Trim('.') : c2.Path.Substring(c2.Root.Length, c2.Path.Length - c2.Root.Length).Trim('.'))}}}";
                    description = c2.Summary ?? string.Empty;
                }

                var type = GetSwaggerType(c2.Type);

                var c3 = new Dictionary<string, object>
                {
                    { "name", name },
                    { "in", "formData" },
                    { "type", type.Item1 },
                    { "description", description }
                };

                if (!string.IsNullOrWhiteSpace(type.Item1))
                {
                    c3.Add("format", type.Item2);
                }
                if (c2.HasDefaultValue)
                {
                    c3.Add("default", System.Convert.ToString(c2.DefaultValue));
                }

                foreach (var attr in c2.Attrs)
                {
                    c3.Add($"x-{attr.Type}", $"{attr.State} {attr.Description}");
                }

                parameters.Add(c3);
            });

            methods.Add("post", new Dictionary<string, object>
            {
                { "operationId", item.Name },
                { "tags", new string[] { business.Configer.Info.BusinessName } },
                { "summary", item.Summary ?? string.Empty},
                { "consumes", new string[] { "multipart/form-data" } },
                { "produces",new string[] { "application/json" } },
                { "parameters", parameters },
                { "description", string.Empty },
                { "responses", new Responses { _200 = new _200 { description = string.Empty } } }
            });

            paths.Add($"/{item.Name}", methods);
        }

        var root = new Root
        {
            info = new Swagger.Doc.Info { title = "X Project", version = "0.1" },
            host = "localhost:5000",
            externalDocs = new ExternalDocs { description = "ExternalDocs description", url = "https://github.com/xlievo/Business.Core" },
            tags = new Tags[] { new Tags { name = business.Configer.Info.BusinessName, externalDocs = new ExternalDocs { } } },
            paths = paths,
        };

        return root;
    }

    static (string, string) GetSwaggerType(System.Type type)
    {
        var type2 = "string";
        var format = string.Empty;

        switch (type.GetTypeCode())
        {
            case TypeCode.Boolean: type2 = "boolean"; break;
            case TypeCode.Byte: format = "binary"; break;
            //case TypeCode.Char: type2 = "string"; break;
            case TypeCode.DateTime: format = "date-time"; break;
            //case TypeCode.DBNull: return "string";
            case TypeCode.Decimal: type2 = "number"; break;
            case TypeCode.Double: type2 = "number"; break;
            //case TypeCode.Empty: return "string";
            case TypeCode.Int16: type2 = "integer"; break;
            case TypeCode.Int32: type2 = "integer"; format = "int32"; break;
            case TypeCode.Int64: type2 = "integer"; format = "int64"; break;
            //case TypeCode.Object: return "string";
            case TypeCode.SByte: type2 = "integer"; break;
            case TypeCode.Single: type2 = "number"; format = "float"; break;
            //case TypeCode.String: type2 = "string"; break;
            case TypeCode.UInt16: type2 = "integer"; break;
            case TypeCode.UInt32: type2 = "integer"; format = "int32"; break;
            case TypeCode.UInt64: type2 = "integer"; format = "int64"; break;
            default: break;
        }

        return (type2, format);
    }
}

/// <summary>
/// A class for an MVC controller with view support.
/// </summary>
[Use]
//Internal object do not write logs
[Logger(canWrite: false)]
public class BusinessController : Controller
{
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
        if (null != path) { return this.NotFound(); }

        string c = null, t = null, d = null, g = null, b = null;

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
                    //{
                    //}
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
        dict.Add("Controller", this);

        g = "j";//fixed grouping

        //this.Request.Headers["X-Real-IP"].FirstOrDefault() 

        var result = await Configer.BusinessList[this.Request.Path.Value.TrimStart('/').Split('/')[0]].Command.AsyncCall(
            //the cmd of this request.
            c,
            //the data of this request, allow null.
            d.TryJsonDeserialize(out object[] data) ? data : new object[] { d },
            //the group of this request.
            g,
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