/*==================================
             ########
            ##########

             ########
            ##########
          ##############
         #######  #######
        ######      ######
        #####        #####
        ####          ####
        ####   ####   ####
        #####  ####  #####
         ################
          ##############
==================================*/

using System;

namespace Business
{
    public interface IBusiness
    {
        System.Action<LoggerData> Logger { get; set; }

        System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IReadOnlyDictionary<string, Command>> Command { get; set; }

        Configer.Configuration Configuration { get; set; }

        System.Action BindAfter { get; set; }
    }

    public interface IBusiness<Request, Result, Token> : IBusiness
        where Request : Business.Request.IRequest
        where Result : Business.Result.IResult
        where Token : Auth.IToken, new()
    { }

    public interface IBusiness<Request, Result> : IBusiness<Request, Result, Auth.Token>
        where Request : Business.Request.IRequest
        where Result : Business.Result.IResult
    { }

    public interface IBusiness<Token> : IBusiness<Request.RequestObject<string>, Result.ResultObject<string>, Token>
        where Token : Auth.IToken, new()
    { }

    public abstract class BusinessBase<Request, Result, Token> : IBusiness<Request, Result, Token>
        where Request : Business.Request.IRequest
        where Result : Business.Result.IResult
        where Token : Auth.IToken, new()
    {
        public System.Action<LoggerData> Logger { get; set; }

        public System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IReadOnlyDictionary<string, Command>> Command { get; set; }

        public Configer.Configuration Configuration { get; set; }

        public Action BindAfter { get; set; }
    }

    public abstract class BusinessBase<Request, Result> : BusinessBase<Request, Result, Auth.Token>, IBusiness<Request, Result>
        where Request : Business.Request.IRequest
        where Result : Business.Result.IResult
    { }

    public abstract class BusinessBase<Token> : BusinessBase<Request.RequestObject<string>, Result.ResultObject<string>, Token>, IBusiness<Token>
        where Token : Auth.IToken, new()
    { }

    public abstract class BusinessBase : BusinessBase<Request.RequestObject<string>, Result.ResultObject<string>, Auth.Token> { }
}
