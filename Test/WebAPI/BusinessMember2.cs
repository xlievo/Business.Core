using Business;
using Business.Attributes;
using Business.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Args;

[JsonArg]
[Logger]
[Info("API")]
public class BusinessMember : BusinessBase
{
    public BusinessMember()
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

    public virtual async Task<dynamic> Test001(Business.Auth.Token token, Arg<Test001> arg, [Ignore(IgnoreMode.BusinessArg)]decimal arg2 = default)
    {
        dynamic args = new System.Dynamic.ExpandoObject();
        args.arg = arg.Out;
        args.arg2 = arg2;
        return this.ResultCreate(args);
    }
}

public class Args
{
    public struct Test001
    {
        public string A { get; set; }

        public string B { get; set; }
    }
}
