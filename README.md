## Platform

Support .Net Framework 4.6, Mono 4.x, .Net Standard 2.0

## Depend

Castle.Core

Newtonsoft.Json

## Install

[![NuGet Version](https://img.shields.io/nuget/v/Business.Core.svg?style=flat)](https://www.nuget.org/packages/Business.Core/)
[![NuGet](https://img.shields.io/nuget/dt/Business.Core.svg)](https://www.nuget.org/packages/Business.Core)
[![Badge](https://img.shields.io/badge/link-996.icu-red.svg)](https://996.icu/#/en_US)

PM> Install-Package Business.Core

## Please refer unit test

## Preface: This library is not RPC, WebAPI, MVC, REST, Http, Rabbitmq, Socket, Kestrel.
## What can he do?

This library has the following functions

1: Call Method with a fixed routing, including grouping mode

2: A Process for Processing Parameters

3: A standard return object wrapper

This library has the following characteristics

1: Callbacks for method calls

2: Support object-to-parameter injection

3: A Model for Documentation

4: async Keyword Support

You can use any communication layer you like, such as ASP.Net Core MVC, WebAPI, Sockets, or even UI logic. It can dramatically improve the readability and specifications of your business logic.

Welcome Pull out to branch, but if you have a good idea, I hope it will be discussed thoroughly before you take action.

This library does not support AOT compilation mode, but can run on Xamarin.Android.

### Before you begin, you should be familiar with 2 interface definitions

```C#
public interface IResult
{
    /// <summary>
    /// The results of the state is greater than or equal 
    /// to 1: success, equal to 0: system level exceptions, less than 0: business class error.
    /// </summary>
    System.Int32 State { get; }

    /// <summary>
    /// Success can be null
    /// </summary>
    System.String Message { get; }

    /// <summary>
    /// Specific Byte/Json data objects
    /// </summary>
    dynamic Data { get; }

    /// <summary>
    /// Whether there is value
    /// </summary>
    System.Boolean HasData { get; }

    /// <summary>
    /// ProtoBuf,MessagePack or Other
    /// </summary>
    /// <returns></returns>
    System.Byte[] ToBytes();

    /// <summary>
    /// Json
    /// </summary>
    /// <returns></returns>
    System.String ToString();
}

public interface IArg
{
    /// <summary>
    /// The first input object
    /// </summary>
    dynamic In { get; set; }

    /// <summary>
    /// The final output object
    /// </summary>
    dynamic Out { get; set; }
}
```

### Begin use

1: Define some processing features, if there are multiple, they are sorted by state

```C#
public class JsonArg : ArgumentAttribute
{
    public JsonArg(int state = -12, string message = null, bool canNull = false) : base(state, message, canNull) { }

    public Newtonsoft.Json.JsonSerializerSettings Settings { get; set; }

    public override async Task<IResult> Proces(dynamic value)
    {
        try
        {
            return this.ResultCreate(Newtonsoft.Json.JsonConvert.DeserializeObject(value, this.Meta.MemberType, Settings));
        }
        catch { return this.ResultCreate(State, Message ?? $"Arguments {this.Meta.Member} Json deserialize error"); }
    }
}

public class CheckNull : ArgumentAttribute
{
    public CheckNull(int state = -800, string message = null) : base(state, message, false) { }

    public override async Task<IResult> Proces(dynamic value)
    {
        if (object.Equals(null, value))
        {
            return this.ResultCreate(State, Message ?? $"argument \"{Nick}\" can not null.");
        }

        return this.ResultCreate();
    }
}
```

2: Define your business class

```C#
public class BusinessMember : BusinessBase
{
    [JsonArg]
    public struct Dto
    {
        [CheckNull]
        public string A { get; set; }
    }

    public virtual async Task<dynamic> MyLogic(Arg<Dto> arg)
    {
        return this.ResultCreate(arg.Out.A);
    }
}
```

3: Register your business class

```C#
static BusinessMember Member = Bind.Create<BusinessMember>();
```

4: Calling business methods

```C#
Member.Command.AsyncCall("MyLogic", new object[] { "{\"A\":\"abc\"}" });
```

## See Unit Testing for more features
