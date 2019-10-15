using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Business;
using Business.Utils;
using Business.Attributes;
using Business.Annotations;
using Business.Result;
using System.Collections.Generic;

#region Socket Support

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
/// my token
/// </summary>
[TokenCheck]
public class Token : Business.Auth.Token { }

/// <summary>
/// my session
/// </summary>
public class Session
{
    public string Account { get; set; }
    public int Nick { get; set; }
    public List<string> Roles { get; set; }
}

/// <summary>
/// Session arg object
/// </summary>
[SessionCheck]
public class SessionArg : Arg<Session, Token> { }

/// <summary>
/// parameter name: context
/// </summary>
[HttpFile]
public class HttpFile : Arg<Dictionary<string, dynamic>> { }

[Command(Group = "j")]
[@JsonArg(Group = "j")]
[Command(Group = "s")]
[@MessagePackArg(Group = "s")]
[Logger]
public abstract class BusinessBase : BusinessBase<ResultObject<string>>
{
    public readonly Host host;

    public BusinessBase(Host host)
    {
        this.host = host;

        this.Logger = new Logger(async x =>
        {
            try
            {
                var logs = x.Select(c =>
                {
                    c.Value = c.Value.ToValue();
                    Console.WriteLine(c.JsonSerialize());
                    return c;
                }).ToList();

                //System.Console.WriteLine(logs.JsonSerialize());
            }
            catch (Exception ex)
            {
                Help.ExceptionWrite(ex, true, true);
            }
        }, maxCapacity: 10000)
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
    public static readonly string LogPath = System.IO.Path.Combine(System.IO.Path.DirectorySeparatorChar.ToString(), "data", $"{AppDomain.CurrentDomain.FriendlyName}.log.txt");

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
    }

    static void InitRedis()
    {
        RedisHelper.Initialization(new CSRedis.CSRedisClient("mymaster,password=123456,prefix=", new[] { "192.168.1.121:26379", "192.168.1.122:26379", "192.168.1.123:26379" }));

        /* https://redis.io/topics/sentinel
         * If everything appears to be normal for 30 second, the TILT mode is exited.

mkdir -p /root/workspace/redis && chown -R 1001 /root/workspace/redis && mkdir -p /root/workspace/redis-sentinel && chown -R 1001 /root/workspace/redis-sentinel
         
docker run -d --name redis -e REDIS_PASSWORD="123456" -e REDIS_REPLICATION_MODE=master -v /root/workspace/redis:/bitnami/redis/data -p 6379:6379 bitnami/redis:latest

docker run -d --name redis -e REDIS_PASSWORD="123456" -e REDIS_REPLICATION_MODE=slave -e REDIS_MASTER_HOST=192.168.1.121 -e REDIS_MASTER_PORT_NUMBER=6379 -e REDIS_MASTER_PASSWORD=123456 -v /root/workspace/redis:/bitnami/redis/data -p 6379:6379 bitnami/redis:latest

docker run -itd --name redis-sentinel -e REDIS_MASTER_HOST=192.168.1.121 -e REDIS_MASTER_PORT_NUMBER=6379 -e REDIS_MASTER_PASSWORD="123456" -v /root/workspace/redis-sentinel:/bitnami/redis-sentinel/conf -p 26379:26379 bitnami/redis-sentinel:latest

        docker run -it --rm bitnami/redis redis-cli -h 172.17.0.1 -p 6379 -a 123456 info Replication
        docker run -it --rm bitnami/redis redis-cli -h 172.17.0.1 -p 6379 -a 123456 set aaa 111
        */

        /* test sentinel
        Exception ex = null;

        do
        {
            try
            {
                RedisHelper.Set("aaa", "a1");
                ex = null;
            }
            catch (Exception exception)
            {
                ex = exception;
                ex.Console();
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(3));
            }
        } while (null != ex);
        */
    }

    /// <summary>
    /// Call this method after environment initialization is complete
    /// </summary>
    /// <param name="docDir"></param>
    public static void InitBusiness(string docDir = null)
    {
        /*
#if DEBUG
        var con = Startup.appSettings.GetSection("Redis").GetSection("ConnectionString").Value;
#else
        var con = Startup.appSettings.GetSection("Redis").GetSection("ConnectionString2").Value;
#endif
        System.Console.WriteLine($"Redis={con}");
        var csredis = new CSRedis.CSRedisClient(con);
        RedisHelper.Initialization(csredis);
        */

        //InitRedis();

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
        DocUI.Write(update: true);
    }
}