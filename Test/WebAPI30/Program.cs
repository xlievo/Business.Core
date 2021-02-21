using System;
using System.Linq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Business.Core.Utils;
//using Microsoft.AspNetCore.Server.Kestrel.Core;

public class Program
{
    public static async Task Main(string[] args)
    {
        //var fileName = "2019-02-12-23-59-03";
        //var fileName2 = System.IO.Path.GetFileNameWithoutExtension(fileName);
        //var ext = System.IO.Path.GetExtension(fileName);
        //var fileName3 = $"{fileName2}_2{ext}";

        //var file2 = await Help.FileReadByteAsync(System.IO.Path.Combine(@"D:\", fileName));

        //var file = await Help.FileReadByteAsync(System.IO.Path.Combine(@"D:\", fileName), async c => await c.GZipByteAsync(System.IO.Compression.CompressionMode.Compress));

        //using (var m = new System.IO.MemoryStream(file))
        //{
        //    //var file3 = await m.GZipByteAsync(System.IO.Compression.CompressionMode.Decompress);

        //    await System.IO.File.WriteAllBytesAsync(System.IO.Path.Combine(@"D:\", fileName3), file);
        //}



        var host = WebHost.CreateDefaultBuilder(args)
            .UseKestrel(options =>
            {
                options.Limits.MinRequestBodyDataRate = null;
                options.AllowSynchronousIO = true;
                options.Limits.KeepAliveTimeout =
                        TimeSpan.FromMinutes(2);
                options.Limits.RequestHeadersTimeout =
                    TimeSpan.FromMinutes(1);
            })
            .UseStartup<Startup>()

#if !DEBUG
            //.UseUrls("http://192.168.1.107:5000")
#endif
            //.UseUrls("http://192.168.1.107:5000")
            //.ConfigureKestrel((context, options) =>
            //{
            //    // Set properties and call methods on options
            //});
            //.UseUrls("http://*:5000")
            .Build();
        Common.Host.Addresses = host.ServerFeatures.Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>().Addresses.FirstOrDefault() ?? "http://localhost:5000";
        Console.WriteLine($"Addresses: {Common.Host.Addresses}");
        host.Run();
    }

    //public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
    //    WebHost.CreateDefaultBuilder(args)
    //        .UseKestrel()
    //        .UseStartup<Startup>();
    //.ConfigureKestrel((context, options) =>
    //{
    //    // Set properties and call methods on options
    //});
}

public class Startup
{
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
            });
        });

        services.AddMvc(option => option.EnableEndpointRouting = false).SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
            .AddJsonOptions(o =>
            {
                //o.JsonSerializerOptions.IgnoreNullValues = true;
                //o.JsonSerializerOptions.PropertyNamingPolicy = Help.JsonNamingPolicyCamelCase.Instance;
            });

        services.AddHttpClient();
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

        //Business.Core.Configer.documentFileName = "business.doc";

        // Set up custom content types -associating file extension to MIME type
        var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
        //provider.Mappings[".yaml"] = "text/yaml";
        provider.Mappings[".doc"] = "application/json";
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(wwwroot),
            ContentTypeProvider = provider,
            OnPrepareResponse = c =>
            {
                //if (c.File.Exists && string.Equals(".doc", System.IO.Path.GetExtension(c.File.Name)))
                if (c.File.Exists && Business.Core.Configer.documentFileName.Equals(c.File.Name))
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
        });//.UseDirectoryBrowser();

        app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto }).UseAuthentication().UseCors("any");

        //System.ComponentModel.DataAnnotations.EmailAddressAttribute
        //==================First step==================//
        Common.Host.ENV = env;
        Common.Host.AppSettings = Configuration.GetSection("AppSettings");
        app.InitBusiness(wwwroot);
        //==================The second step==================//
    }
}