using System;
using System.Linq;
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
using Business.Utils;
using Business.Result;
//using Microsoft.AspNetCore.Server.Kestrel.Core;

public class Program
{
    public static void Main(string[] args)
    {
        var host = CreateWebHostBuilder(args)
            //.UseUrls("http://*:5000")
            .Build();
        Common.Host.Addresses = host.ServerFeatures.Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>().Addresses.FirstOrDefault() ?? "http://localhost:5000";
        Console.WriteLine($"Addresses: {Common.Host.Addresses}");
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

    public Startup(IConfiguration configuration) => Configuration = configuration;

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
                .AllowAnyHeader();
                //.AllowCredentials();
            });
        });

        services.AddMvc(option => option.EnableEndpointRouting = false).SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                options.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Local;
            });

        Common.Host.HttpClientFactory = services.AddHttpClient("")
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
            var handler = new HttpClientHandler()
            {
                AllowAutoRedirect = false,
                UseDefaultCredentials = true,
            };

            if (Common.Host.ENV.IsDevelopment())
            {
                handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            }

            return handler;
        })
        .Services
        .BuildServiceProvider()
        .GetService<IHttpClientFactory>();
        //AppContext.SetSwitch("System.Net.Http.UseSocketsHttpHandler", false);
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        var wwwroot = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot");

        if (!System.IO.Directory.Exists(wwwroot))
        {
            System.IO.Directory.CreateDirectory(wwwroot);
        }

        Console.WriteLine($"wwwroot: {wwwroot}");

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

        //System.ComponentModel.DataAnnotations.EmailAddressAttribute
        //==================First step==================//
        Common.Host.ENV = env;
        Common.Host.AppSettings = Configuration.GetSection("AppSettings");
        Common.InitBusiness(wwwroot);
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



        #region AcceptWebSocket

        var webSocketcfg = Common.Host.AppSettings.GetSection("WebSocket");
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

                                var data = Business.Result.ResultFactory.ResultCreateToDataBytes(result2).ToBytes();
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
                    Help.ExceptionWrite(ex, true, true, Common.LogPath);
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
            Help.ExceptionWrite(ex, true, true, Common.LogPath);
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

        g = "j";//fixed grouping

        #region benchmark

        if ("benchmark" == c)
        {
            var arg = d.TryJsonDeserialize<DocUI.benchmarkArg>();
            if (default(DocUI.benchmarkArg).Equals(arg)) { return new System.ArgumentNullException(nameof(arg)).Message; }
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
            return business.ErrorCmd(c);
        }

        var result = await cmd.AsyncCall(
            //the data of this request, allow null.
            cmd.HasArgSingle ? new object[] { d } : d.TryJsonDeserializeObjectArray(),
            //the incoming use object
            new UseEntry(this, "context"), //context
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