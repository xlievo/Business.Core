using Business;
using Business.Attributes;
using Business.Result;
using Business.Utils;
using System;
using System.Threading.Tasks;

public class Test
{
    public static BusinessMember Member = Bind.Create<BusinessMember>();
    public static ConcurrentReadOnlyDictionary<string, Command> Cmd = Member.Command[Bind.CommandGroupDefault];
}

[Logger]
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

        this.BindAfter = () =>
        {

        };

        //this.Config += cfg => cfg.UseType(typeof(BusinessController));
    }

    [JsonArg]
    public struct Ags2
    {
        public string A { get; set; }

        public string B { get; set; }
    }

    public class TokenCheck : ArgumentAttribute
    {
        public TokenCheck(int state = -110, string message = null, bool canNull = true) : base(state, message, canNull)
        {
        }

        public override IResult Proces(dynamic value)
        {
            return this.ResultCreate();
        }
    }

    public class ControllerCheck : ArgumentAttribute
    {
        public ControllerCheck(int state = -110, string message = null, bool canNull = true) : base(state, message, canNull)
        {
        }

        public override IResult Proces(dynamic value)
        {
            return this.ResultCreate();
        }
    }

    public virtual async Task<dynamic> TestAgs001(BusinessController control, Arg<Ags2> a, decimal mm = 0.0234m, Arg<BusinessController, BusinessController> ss = default(Arg<BusinessController, BusinessController>), Token token = default(Token))
    {
        return this.ResultCreate(new { a = a.In, Remote = string.Format("{0}:{1}", control.HttpContext.Connection.RemoteIpAddress.ToString(), control.HttpContext.Connection.RemotePort), control.Request.Cookies });

        return control.Redirect("https://www.github.com");
    }
}
