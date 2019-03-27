﻿using Business;
using Business.Attributes;
using Business.Result;
using Business.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using static Args;

[JsonArg]
[Logger]
[Info("API")]
public class BusinessMember2 : BusinessBase
{
    public BusinessMember2()
    {
        this.Logger = x =>
        {
            try
            {
                x.Value = x.Value?.ToValue();

                var log = x.JsonSerialize();

                Help.WriteLocal(log, console: true, write: x.Type == LoggerType.Exception);
            }
            catch (Exception exception)
            {
                Help.ExceptionWrite(exception, true, true);
            }
        };
    }

    public virtual async Task<dynamic> Test001(Business.Auth.Token token, Arg<Test001> arg, [Business.Attributes.Ignore(IgnoreMode.BusinessArg)]decimal arg2 = default)
    {
        dynamic args = new System.Dynamic.ExpandoObject();
        args.token = token;
        args.arg = arg.Out;
        args.arg2 = arg2;
        return this.ResultCreate(args);
    }
}

public class TestAttribute : ArgumentAttribute
{
    public TestAttribute(int state = 111, string message = null) : base(state, message) { }

    public override async Task<IResult> Proces(dynamic value)
    {
        switch (value)
        {
            case "ok":
                return this.ResultCreate();

            case "error":
                return this.ResultCreate(this.State, $"{this.Nick} cannot be empty");

            case "data":
                return this.ResultCreate(value + "1122");

            default: throw new System.Exception("exception!");
        }
    }
}

public class Args
{
    public struct Test001
    {
        [Test]
        [Nick("password")]
        public string A { get; set; }

        public string B { get; set; }
    }
}