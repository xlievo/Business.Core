﻿using Business.Core.Annotations;
using Business.Core.Result;
using Business.Core.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http;
using Business.Core;

public class GitHubClient
{
    public HttpClient Client { get; private set; }

    public GitHubClient(HttpClient httpClient)
    {
        httpClient.BaseAddress = new Uri("https://api.github.com/");
        httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
        httpClient.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactory-Sample");
        Client = httpClient;
    }

    public async Task<string> GetData()
    {
        return await Client.GetStringAsync("/");
    }
}

[Info(Alias = "TEST2")]
[Logger]
public class BusinessMember : BusinessBase
{
    GitHubClient gitHubClient;

    public BusinessMember() : base()
    {
        gitHubClient = new GitHubClient(Common.Host.HttpClientFactory?.CreateClient("github"));
    }

    public class A2 : ArgumentAttribute
    {
        public A2(int state = -110, string message = null) : base(state, message)
        {
        }

        public async override ValueTask<IResult> Proces(dynamic value)
        {
            return this.ResultCreate(string.Format("{0}+{1}", value, "1234567890"));
        }
    }

    public struct Ags2
    {
        [A2]
        public string A2 { get; set; }

        public Ags3 B2 { get; set; }
    }

    public class Ags3
    {
        //[A2]
        public string A3 { get; set; }

        public Ags4 B3 { get; set; }
    }

    public struct Ags4
    {
        [A2]
        public string A4 { get; set; }

        public string B4 { get; set; }
    }

    public struct Files
    {
        public string Key { get; set; }

        public string Name { get; set; }

        public byte[] File { get; set; }
    }

    public class FileCheck : ArgumentAttribute
    {
        public FileCheck(int state = -110, string message = null) : base(state, message) { }

        public override async ValueTask<IResult> Proces(dynamic value)
        {
            if (null == value) { return this.ResultCreate(); }
            BusinessController col = value;

            if (!col.Request.HasFormContentType) { return this.ResultCreate(); }

            var list = new List<Files>();
            foreach (var item in col.Request.Form.Files)
            {
                using (var m = new System.IO.MemoryStream())
                {
                    await item.CopyToAsync(m);
                    list.Add(new Files { Name = item.FileName, Key = item.Name, File = m.GetBuffer() });
                }
            }

            return this.ResultCreate(list);
        }
    }

    public virtual async Task<dynamic> TestAgs001([DynamicObject] dynamic context, Ags2 a, [Ignore(IgnoreMode.BusinessArg)] decimal mm = 0.0234m, [FileCheck][Use(typeof(BusinessController))] List<Files> sss = default, Token token = default)
    {
        return await gitHubClient.GetData();

        //var client = httpClientFactory.CreateClient("github");
        //string result = await client.GetStringAsync("https://www.github.com");
        //return this.ResultCreate(result);

        //return this.ResultCreate(a.Out);

        //return this.ResultCreate(new { a = a.In, Remote = string.Format("{0}:{1}", control.HttpContext.Connection.RemoteIpAddress.ToString(), control.HttpContext.Connection.RemotePort), control.Request.Cookies });

        //return control.Redirect("https://www.github.com");
    }
}
