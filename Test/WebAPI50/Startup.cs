using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI50
{
    public class Startup
    {
        internal static string httpAddress = null;

        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
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
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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


            var addresses = app.ServerFeatures.Get<IServerAddressesFeature>().Addresses.Select(c =>
            {
                var address = c;
                var address2 = address.ToLower();
                if (address2.StartsWith("http://+:") || address2.StartsWith("http://*:") || address2.StartsWith("https://+:") || address2.StartsWith("https://*:"))
                {
                    address = address.Replace("*", "localhost").Replace("+", "localhost");
                }
                if (null == httpAddress && address2.StartsWith("http://"))
                {
                    httpAddress = address;
                }
                return address;
            }).ToArray();
            Common.Host.Addresses = addresses;

            //System.ComponentModel.DataAnnotations.EmailAddressAttribute
            //==================First step==================//
            //Common.Host.ENV = env;
            Common.Host.AppSettings = Configuration.GetSection("AppSettings");
            app.InitBusiness(wwwroot);
            //==================The second step==================//
        }
    }
}
