using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Business;
using Business.Utils;
using Business.Attributes;
using Business.Result;
using System.Collections.Generic;

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

/// <summary>
/// my toekn
/// </summary>
public class Token : Business.Auth.Token { }

/// <summary>
/// my session
/// </summary>
public class SessionObj
{
    public string Account { get; set; }
    public int Nick { get; set; }
    public List<string> Roles { get; set; }
}

public class Session : Arg<SessionObj, Token> { }

[Command(Group = "j")]
[Command(Group = "s")]

[JsonArg(Group = "j")]
[MessagePackArg(Group = "s")]

[Logger]
public abstract class BusinessBase : BusinessBase<ResultObject<string>>
{
    public readonly Host host;

    public BusinessBase(Host host)
    {
        this.host = host;

        this.Logger = new Logger(async x =>
        {
            var logs = x.Select(c =>
            {
                c.Value = c.Value.ToValue();
                return c;
            });

            System.Console.WriteLine(logs.JsonSerialize());
        }, maxCapacity: 1000)
        {
            //Batch = new Logger.BatchOptions
            //{
            //    Interval = System.TimeSpan.FromSeconds(6),
            //    MaxNumber = 100
            //}
        };
    }
}

public class Common
{
    public static string LogPath = System.IO.Path.Combine(System.IO.Path.DirectorySeparatorChar.ToString(), "data", $"{AppDomain.CurrentDomain.FriendlyName}.log.txt");

    public static Host Host = new Host();

    static Common()
    {
        //ThreadPool.SetMinThreads(50, 50);
        //ThreadPool.GetMinThreads(out int workerThreads, out int completionPortThreads);
        //ThreadPool.GetMaxThreads(out int workerThreads2, out int completionPortThreads2);

        //Console.WriteLine($"Min {workerThreads}, {completionPortThreads}");
        //Console.WriteLine($"Max {workerThreads2}, {completionPortThreads2}");

        AppDomain.CurrentDomain.UnhandledException += (sender, e) => (e.ExceptionObject as Exception)?.ExceptionWrite(true, true, LogPath);

        Console.WriteLine($"LogPath: {LogPath}");

        MessagePack.Resolvers.CompositeResolver.RegisterAndSetAsDefault(MessagePack.Resolvers.ContractlessStandardResolver.Instance);

        Newtonsoft.Json.JsonConvert.DefaultSettings = (() =>
        {
            var settings = Help.JsonSettings;
            settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            return settings;
        });
    }

    public static void InitBusiness(string docDir = null)
    {
        //1
        Configer.LoadBusiness(new object[] { Host });
        //2
        Configer.UseType("context", "socket");
        Configer.IgnoreSet(new Ignore(IgnoreMode.Arg), "context", "socket");
        Configer.LoggerSet(new LoggerAttribute(canWrite: false), "context", "socket");
        //==================The third step==================//
        //3
        Configer.UseDoc(docDir);
        //writ url to page
        Business.DocUI.UI.Write($"{Host.Addresses}{"/"}business.doc");
    }
}
