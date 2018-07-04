using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

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

        services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto }).UseAuthentication().UseCors("any");

        app.UseMvc(routes =>
        {
            foreach (var item in Business.Bind.BusinessList)
            {
                routes.MapRoute(
                 name: "default",
                 template: string.Format("{0}/{{*path}}", item.Key),
                 defaults: new { controller = "Business", action = "Call" });
            }
        });

        Business.Bind.UseType(typeof(BusinessController));
    }
}

[Business.Attributes.Logger(Business.LoggerType.All, false)]
public class BusinessController : Controller
{
    [HttpGet]
    [HttpPost]
    [EnableCors("any")]
    public async System.Threading.Tasks.Task<dynamic> Call(string path)
    {
        if (null != path) { return this.NotFound(); }

        var c = System.String.Empty;
        var t = System.String.Empty;
        var d = System.String.Empty;

        switch (this.Request.Method)
        {
            case "GET":
                c = this.Request.Query["c"].ToString();
                t = this.Request.Query["t"].ToString();
                d = this.Request.Query["d"].ToString();
                break;
            case "POST":
                {
                    if (this.Request.HasFormContentType)
                    {
                        var form = await this.Request.ReadFormAsync();
                        c = form["c"].ToString();
                        t = form["t"].ToString();
                        d = form["d"].ToString();
                    }
                }
                break;
            default: return this.NotFound();
        }

        //this.Request.Headers["X-Real-IP"].FirstOrDefault() 
        return await Business.Bind.BusinessList[this.Request.Path.Value.TrimStart('/').Split('/')[0]].Command.AsyncCallUseType(new Business.Request.RequestObject
        {
            Cmd = c,
            Token = t,
            Data = Business.Utils.Help.TryJsonDeserialize(d, out object[] data) ? data : new object[] { d },
            Remote = string.Format("{0}:{1}", this.HttpContext.Connection.RemoteIpAddress.ToString(), this.HttpContext.Connection.RemotePort),
        }, new object[] { this });//, result => result.Callback = "Callback"
    }
}