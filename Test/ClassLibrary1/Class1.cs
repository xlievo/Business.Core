using Business;
using Business.Attributes;
using Business.Result;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// ddddddddddddddddddddd!!!
/// </summary>
public class BusinessMember2
{

}

/// <summary>
/// Session~~~~~~~~~~~~~~~
/// </summary>
public class SessionAttribute : ArgumentAttribute
{
    public SessionAttribute(int state = -800, string message = null) : base(state, message)
    {
        this.BindAfter += () =>
        {
            this.Description = this.Replace("{Nick} 这个参数必须填写");
        };
    }

    public async override ValueTask<IResult> Proces(dynamic value)
    {
        var session = new Session { UserName = value.Key };
        return this.ResultCreate(session);
    }
}



/// <summary>
/// Session！！！
/// </summary>
[Session]
public class Session
{
    public int UserId { get; set; }
    public string UserName { get; set; }
    public List<string> Roles { get; set; }
}

public class SessionArg : Arg<Session, Business.Auth.Token> { }