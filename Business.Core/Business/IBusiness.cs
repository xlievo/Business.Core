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

namespace Business
{
    public interface IBusiness
    {
        System.Action<LoggerData> Logger { get; set; }

        CommandGroup Command { get; set; }

        Configer.Configuration Configuration { get; set; }

        System.Action BindAfter { get; set; }

        System.Action<Configer.Configuration> Config { get; set; }
    }

    //public interface IBusiness<Result, Token> : IBusiness
    //    where Result : Business.Result.IResult
    //    where Token : Auth.IToken, new()
    //{ }

    public interface IBusiness<Result> : IBusiness
        where Result : Business.Result.IResult
    { }

    //public interface IBusiness<Token> : IBusiness<Request.RequestObject<string>, Result.ResultObject<string>, Token>
    //    where Token : Auth.IToken, new()
    //{ }

    /*
    public abstract class BusinessBase : IBusiness
    {
        public BusinessBase() => this.Configuration = new Configer.Configuration();

        public System.Action<LoggerData> Logger { get; set; }

        public CommandGroup Command { get; set; }

        public Configer.Configuration Configuration { get; set; }

        public System.Action BindAfter { get; set; }

        public virtual IBusiness Use<T>(T generic)
        {
            switch (generic)
            {
                case Business.Request.IRequest ss: Configuration.RequestType = ss.GetType().GetTypeInfo(); break;
                case Business.Result.IResult ss: this.result = ss; break;
                case Auth.IToken ss: this.token = ss; break;
                default: break;
            }

            return this;
        }
    }
    */

    //public abstract class BusinessBase<Result, Token> : IBusiness<Result, Token>
    //    where Result : Business.Result.IResult
    //    where Token : Auth.IToken, new()
    //{

    //}

    public abstract class BusinessBase<Result> : IBusiness<Result>
        where Result : Business.Result.IResult
    {
        public System.Action<LoggerData> Logger { get; set; }

        public CommandGroup Command { get; set; }

        public Configer.Configuration Configuration { get; set; }

        public System.Action BindAfter { get; set; }

        public System.Action<Configer.Configuration> Config { get; set; }
    }

    //public abstract class BusinessBase<Token> : BusinessBase<Request.RequestObject<string>, Result.ResultObject<string>, Token>, IBusiness<Token>
    //    where Token : Auth.IToken, new()
    //{ }

    public abstract class BusinessBase : BusinessBase<Result.ResultObject<string>> { }
}
