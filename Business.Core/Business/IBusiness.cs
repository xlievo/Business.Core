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

        Configer Configer { get; set; }

        System.Action BindAfter { get; set; }

        System.Action<Configer> BindBefore { get; set; }
    }

    //public interface IBusiness<Result, Token> : IBusiness
    //    where Result : Business.Result.IResult
    //    where Token : Auth.IToken, new()
    //{ }

    public interface IBusiness<Result> : IBusiness where Result : Business.Result.IResult { }

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

        public Configer Configer { get; set; }

        public System.Action BindAfter { get; set; }

        public System.Action<Configer> BindBefore { get; set; }

        #region business

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <param name="business"></param>
        /// <returns></returns>
        //public static IResult ResultCreate(this IBusiness business) => Create<string>(business);

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <param name="business"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public Business.Result.IResult ResultCreate(int state) => Business.Result.ResultFactory.ResultCreate(this, state);

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <param name="business"></param>
        /// <param name="state"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public Business.Result.IResult ResultCreate(int state = 1, string message = null) => Business.Result.ResultFactory.ResultCreate(this, state, message);

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <typeparam name="Data"></typeparam>
        /// <param name="business"></param>
        /// <param name="data"></param>
        /// <param name="message"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public Business.Result.IResult ResultCreate<Data>(Data data, string message = null, int state = 1) => Business.Result.ResultFactory.ResultCreate(this, data, message, state);

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <typeparam name="Data"></typeparam>
        /// <param name="business"></param>
        /// <param name="data"></param>
        /// <param name="message"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public Business.Result.IResult ResultCreate(object data, string message = null, int state = 1) => Business.Result.ResultFactory.ResultCreate(this, data, message, state);

        /*
        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <param name="business"></param>
        /// <param name="filePath"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static IResult ResultCreate(this IBusiness business, string filePath, string contentType) => Create<HttpFile>(business, new HttpFile { FilePath = filePath, ContentType = contentType });
        */

        #endregion
    }

    //public abstract class BusinessBase<Token> : BusinessBase<Request.RequestObject<string>, Result.ResultObject<string>, Token>, IBusiness<Token>
    //    where Token : Auth.IToken, new()
    //{ }

    public abstract class BusinessBase : BusinessBase<Result.ResultObject<string>> { }
}
