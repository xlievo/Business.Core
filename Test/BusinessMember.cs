using Business;
using Business.Attributes;
using Business.Result;
using Business.Utils;
using System;
using System.Threading.Tasks;
using System.Linq;

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

    
    [Logger(LoggerType.Record, false)]
    public struct Token2
    {
        public string Key { get; set; }

        public string Remote { get; set; }

        public string CommandID { get; set; }
    }

    public class Token3
    {
        public string Key { get; set; }

        public string Remote { get; set; }
    }

    [TokenCheck]
    public class Token : Arg<Token3, Token2> { }

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
            //Microsoft.AspNetCore.Mvc.ControllerBase v = value;
            //return this.ResultCreate(value.Request.QueryString.Value);
            return this.ResultCreate();
        }
    }

    public virtual async Task<IResult> TestAgs001(BusinessController control, Arg<Ags2> a, decimal mm = 0.0234m, Arg<BusinessController, BusinessController> ss = default(Arg<BusinessController, BusinessController>), Token token = null)
    {
        return this.ResultCreate(new { a = a.In, Remote = string.Format("{0}:{1}", control.HttpContext.Connection.RemoteIpAddress.ToString(), control.HttpContext.Connection.RemotePort), control.Request.Cookies });
    }
    /*
    public virtual async Task<dynamic> TestAgs002(Microsoft.AspNetCore.Mvc.ControllerBase b)
    {

        return b.File(System.IO.File.ReadAllBytes(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "magazine.jpg")), "image/jpeg");
        //return b.Redirect("https://www.baidu.com");
    }

    [Command("T001")]
    [Command("T002")]
    [Command("T003", Group = "A")]
    [Command("T004", Group = "B")]
    public virtual async Task<IResult> TestReturn001([Size(Min = 3, Nick = "密码")]int a)
    {
        switch (a)
        {
            case 3: throw new System.Exception("ex");
            case 4:
                System.Threading.Thread.Sleep(2000);
                return this.ResultCreate("Sleep");
            //case 5:
            //    return this.ResultCreate(new Business.Http.FileResult { ContentType = "", FilePath = "" });
            //case 6:
            //    return this.ResultCreate(new Business.Http.StreamResult { ContentType = "", Stream = new byte[0] });
            //case 7:
            //    return this.ResultCreate(new Business.Http.HttpResult { Contents = 3.ToString(), ContentType = "text/html" });
            //case 8:
            //return this.ResultCreate(new Business.Http.RedirectResult("http://www.baidu.com", RedirectType.Permanent));
            default: break;
        }

        return this.ResultCreate(data: 5);
    }

    public virtual async Task<dynamic> TestReturn002([Size(Min = 3)]int a)
    {
        return await System.Threading.Tasks.Task.Run(() =>
        {
            switch (a)
            {
                case 3: throw new System.Exception("ex");
                case 4:
                    System.Threading.Thread.Sleep(2000);
                    return this.ResultCreate("Sleep");
                //case 5:
                //    return this.ResultCreate(new Business.Http.FileResult { ContentType = "", FilePath = "" });
                //case 6:
                //    return this.ResultCreate(new Business.Http.StreamResult { ContentType = "", Stream = new byte[0] });
                //case 7:
                //    return this.ResultCreate(new Business.Http.HttpResult { Contents = 3.ToString(), ContentType = "text/html" });
                //case 8:
                //return this.ResultCreate(new Business.Http.RedirectResult("http://www.baidu.com", RedirectType.Permanent));
                default: break;
            }

            return this.ResultCreate(data: 5);
        });
    }

    public virtual async Task<dynamic> TestReturn0021([Size(Min = 3)]int a)
    {
        return await System.Threading.Tasks.Task.Run(() =>
        {
            switch (a)
            {
                case 3: throw new System.Exception("ex");
                case 4:
                    System.Threading.Thread.Sleep(2000);
                    break;
                //case 7:
                //    return new Business.Http.HttpResult { Contents = 3.ToString(), ContentType = "text/html" };
                default: break;
            }

            return default(int);
        });
    }

    public virtual async Task TestReturn003([Size(Min = 3)]int a)
    {
        await System.Threading.Tasks.Task.Run(() =>
        {
            switch (a)
            {
                case 3: throw new System.Exception("ex");
                case 4:
                    System.Threading.Thread.Sleep(2000);
                    return this.ResultCreate("Sleep");
                //case 5:
                //    return this.ResultCreate(new Business.Http.FileResult { ContentType = "", FilePath = "" });
                //case 6:
                //    return this.ResultCreate(new Business.Http.StreamResult { ContentType = "", Stream = new byte[0] });
                //case 7:
                //    return this.ResultCreate(new Business.Http.HttpResult { Contents = 3.ToString(), ContentType = "text/html" });
                //case 8:
                //return this.ResultCreate(new Business.Http.RedirectResult("http://www.baidu.com", RedirectType.Permanent));
                default: break;
            }

            return this.ResultCreate(data: 5);
        });
    }

    public virtual IResult TestReturn004([Size(Min = 3)]int a)
    {
        switch (a)
        {
            case 3: throw new System.Exception("ex");
            case 4:
                System.Threading.Thread.Sleep(2000);
                return this.ResultCreate("Sleep");
            //case 5:
            //    return this.ResultCreate(new Business.Http.FileResult { ContentType = "", FilePath = "" });
            //case 6:
            //    return this.ResultCreate(new Business.Http.StreamResult { ContentType = "", Stream = new byte[0] });
            //case 7:
            //    return this.ResultCreate(new Business.Http.HttpResult { Contents = 3.ToString(), ContentType = "text/html" });
            //case 8:
            //return this.ResultCreate(new Business.Http.RedirectResult("http://www.baidu.com", RedirectType.Permanent));
            default: break;
        }

        return this.ResultCreate(data: 5);
    }

    public virtual IResult Test0011([Size(Min = 3)]int a)
    {
        if (a == 3)
        {
            throw new System.Exception("ex");
        }
        else
        {
            //return new Business.Http.HttpResult { Contents = 3.ToString(), ContentType = "text/html" };
            return this.ResultCreate(new { Contents = 3.ToString(), ContentType = "text/html" });
        }
    }

    public virtual void Test0033([Size(Min = 3)]int a)
    {
        if (a == 3)
        {
            throw new System.Exception("ex");
        }
        else if (a == 4)
        {
            System.Threading.Thread.Sleep(2000);
        }
    }

    public virtual async Task<dynamic> Test00111([Size(Min = 3)]int a)
    {
        return await System.Threading.Tasks.Task.Run(() =>
        {
            //System.Threading.Thread.Sleep(3000);
            //return this.ResultCreate(-911, "aaaaaaa");
            if (a == 3)
            {
                throw new System.Exception("ex");
            }
            else
            {
                //return new Business.Http.HttpResult { Contents = 3.ToString(), ContentType = "text/html" };

                return this.ResultCreate(data: 5);
            }
        });
    }

    public virtual async Task<dynamic> Test00222([Size(Min = 3)]int a)
    {
        return await System.Threading.Tasks.Task.Run(() =>
        {
            //System.Threading.Thread.Sleep(5000);
            //return this.ResultCreate(-911, "aaaaaaa");
            if (a == 3)
            {
                throw new System.Exception("ex");
            }
            else
            {
                return "3";
            }
        });
    }

    public virtual async Task<dynamic> Test001111([Size(Min = 3)]int a)
    {
        return await System.Threading.Tasks.Task.Run(() =>
        {
            //System.Threading.Thread.Sleep(3000);
            //return this.ResultCreate(-911, "aaaaaaa");
            if (a == 3)
            {
                throw new System.Exception("ex");
            }
            else
            {
                //return new Business.Http.HttpResult { Contents = 3.ToString(), ContentType = "text/html" };

                return 5;
            }
        });
    }

    public virtual async Task<dynamic> Test002222([Size(Min = 3)]int a)
    {
        return await System.Threading.Tasks.Task.Run(() =>
        {
            //System.Threading.Thread.Sleep(5000);
            //return this.ResultCreate(-911, "aaaaaaa");
            if (a == 3)
            {
                throw new System.Exception("ex");
            }
            else
            {
                return new { Contents = 3.ToString(), ContentType = "text/html" };
            }
        });
    }

    public virtual async Task<dynamic> Test003333([Size(Min = 3)]int a)
    {
        return await System.Threading.Tasks.Task.Run(() =>
        {
            //System.Threading.Thread.Sleep(3000);
            //return this.ResultCreate(-911, "aaaaaaa");
            if (a == 3)
            {
                throw new System.Exception("ex");
            }
            else
            {
                //return new Business.Http.HttpResult { Contents = 3.ToString(), ContentType = "text/html" };

                return this.ResultCreate(data: 5);
            }
        });
    }
    */
}
