using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Business.Utils;
using Business.Attributes;
using Business.Auth;
using Business;
using System.Collections.Generic;

public class Program
{
    public static void Main(string[] args)
    {
        CreateWebHostBuilder(args).Build().Run();
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>();
}

public class Startup
{
    static Startup() => Business.Bind.Create<BusinessMember>();

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
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
            .AddJsonOptions(options => options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver());
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // Set up custom content types -associating file extension to MIME type
        var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
        //provider.Mappings[".yaml"] = "text/yaml";
        provider.Mappings[".doc"] = "application/json";
        app.UseDefaultFiles().UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "wwwroot")),
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

        //====================================//
        Configer.LoadBusiness();

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

        Configer.UseDoc(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "doc"));
        Configer.UseType(typeof(BusinessController));
    }
}

public class ResultObject<Type> : Business.Result.ResultObject<Type>
{
    //static ResultObject() => MessagePack.Resolvers.CompositeResolver.RegisterAndSetAsDefault(MessagePack.Resolvers.ContractlessStandardResolver.Instance);

    public ResultObject(Type data, System.Type dataType, int state = 1, string message = null, System.Type genericType = null)
        : base(data, dataType, state, message, genericType) { }

    public ResultObject(Type data, int state = 1, string message = null) : this(data, null, state, message) { }

    [MessagePack.IgnoreMember]
    public override System.Type DataType { get => base.DataType; set => base.DataType = value; }

    [MessagePack.IgnoreMember]
    public override System.Type GenericType => base.GenericType;

    public override byte[] ToBytes() => MessagePack.MessagePackSerializer.Serialize(this);
}

[Use]
//Internal object do not write logs
[Logger(canWrite: false)]
public class BusinessController : Controller
{
    //static BusinessController() => Configer.UseType(typeof(BusinessController), typeof(Auth.Token));

    public static readonly System.Collections.Concurrent.ConcurrentDictionary<string, System.Net.WebSockets.WebSocket> Sockets = new System.Collections.Concurrent.ConcurrentDictionary<string, System.Net.WebSockets.WebSocket>();

    public static void Register(IApplicationBuilder app)
    {
        var bufferSize = 4 * 1024;
        app.UseWebSockets(new WebSocketOptions
        {
            KeepAliveInterval = System.TimeSpan.FromSeconds(120),  //向客户端发送“ping”帧的频率，以确保代理保持连接处于打开状态
            ReceiveBufferSize = bufferSize   //用于接收数据的缓冲区的大小。 只有高级用户才需要对其进行更改，以便根据数据大小调整性能。
        })
        .Use(async (context, next) =>
        {
            if (context.Request.Path != "/ws")
            {
                await next();
                return;
            }

            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = 400;
                return;
            }

            var webSocket = await context.WebSockets.AcceptWebSocketAsync();

            var ct = context.RequestAborted;
            var buffer = new byte[bufferSize];
            var result = await webSocket.ReceiveAsync(new System.ArraySegment<byte>(buffer), System.Threading.CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }

                await webSocket.SendAsync(new System.ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, System.Threading.CancellationToken.None);

                result = await webSocket.ReceiveAsync(new System.ArraySegment<byte>(buffer), System.Threading.CancellationToken.None);
            }

            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, System.Threading.CancellationToken.None);
            //await Echo(context, webSocket);
        });
    }

    //public static System.Func<BusinessController, RequestData, object[]> UseObj;

    /// <summary>
    /// Call
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    [HttpGet]
    [HttpPost]
    [EnableCors("any")]
    public async System.Threading.Tasks.Task<dynamic> Call(string path)
    {
        if (null != path) { return this.NotFound(); }

        //RequestData requestData = null;
        string c = null, t = null, d = null, g = null, b = null;

        switch (this.Request.Method)
        {
            case "GET":
                //requestData = new RequestData(this.Request.Query);
                c = this.Request.Query["c"];
                t = this.Request.Query["t"];
                d = this.Request.Query["d"];
                g = this.Request.Query["g"];
                //b = this.Request.Query["b"];
                break;
            case "POST":
                {
                    if (this.Request.HasFormContentType)
                    {
                        //requestData = new RequestData(await this.Request.ReadFormAsync());
                        var form = await this.Request.ReadFormAsync();
                        c = form["c"];
                        t = form["t"];
                        d = form["d"];
                        g = form["g"];
                        //b = form["b"];
                    }
                }
                break;
            default: return this.NotFound();
        }

        //this.Request.Headers["X-Real-IP"].FirstOrDefault() 

        var use = new System.Dynamic.ExpandoObject();
        use.TryAdd("Control", this);
        use.TryAdd("Token", new Token //token
        {
            Key = t,
            Remote = string.Format("{0}:{1}", this.HttpContext.Connection.RemoteIpAddress.ToString(), this.HttpContext.Connection.RemotePort),
            //Callback = b
        });

        var result = await Configer.BusinessList[this.Request.Path.Value.TrimStart('/').Split('/')[0]].Command.AsyncCallUse(
            //the cmd of this request.
            c,
            //the group of this request.
            g,
            //the data of this request, allow null.
            Help.TryJsonDeserialize(d, out object[] data) ? data : new object[] { d },
            //the incoming use object
            new object[]
            {
                    this, //controller
                    new Token //token
                    {
                        Key = t,
                        Remote = string.Format("{0}:{1}", this.HttpContext.Connection.RemoteIpAddress.ToString(), this.HttpContext.Connection.RemotePort),
                        //Callback = b
                    },
                    new UseEntry("control", use)
            });

        if (null != result)
        {
            if (typeof(Business.Result.IResult).IsAssignableFrom(result.GetType()))
            {
                result.Callback = b;
            }
        }

        //var bytes = result.ToBytes();

        //var ags4 = MessagePack.MessagePackSerializer.Deserialize<Business.Result.ResultObject<BusinessMember.Ags4>>(bytes);

        //var r2 = ags4?.Data?.A4;

        return result;
    }
}