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
using Business;
using Business.Attributes;
using Business.Auth;
using Business.Utils;
using System.Collections.Generic;

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

    public Startup(IConfiguration configuration)
    {
        System.Threading.ThreadPool.SetMinThreads(50, 50);
        System.Threading.ThreadPool.SetMaxThreads(32767, 32767);
        System.Threading.ThreadPool.GetMinThreads(out int workerThreads, out int completionPortThreads);
        System.Threading.ThreadPool.GetMaxThreads(out int workerThreads2, out int completionPortThreads2);

        System.Console.WriteLine($"Min {workerThreads}, {completionPortThreads}");
        System.Console.WriteLine($"Max {workerThreads2}, {completionPortThreads2}");

        Configuration = configuration;
        appSettings = Configuration.GetSection("AppSettings");
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
        Configer.LoadBusiness();

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
        Configer.UseDoc(System.IO.Path.Combine(wwwroot));
    }
}

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
    public async System.Threading.Tasks.Task<dynamic> Call(string path)
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
        var result = await Configer.BusinessList[this.Request.Path.Value.TrimStart('/').Split('/')[0]].Command.AsyncCall(
            //the cmd of this request.
            c,
            //the data of this request, allow null.
            d.TryJsonDeserialize(out object[] data) ? data : new object[] { d },
            //the group of this request.
            g,
            //the incoming use object
            new UseEntry(this, "controller"), //controller
            new UseEntry(new Token //token
            {
                Key = t,
                Remote = string.Format("{0}:{1}", this.HttpContext.Connection.RemoteIpAddress.ToString(), this.HttpContext.Connection.RemotePort),
                //Callback = b
            }, "session") //[Use(true)]
            );

        if (null != result)
        {
            if (typeof(Business.Result.IResult).IsAssignableFrom(result.GetType()))
            {
                result.Callback = b;
            }
        }

        return result;
    }
}