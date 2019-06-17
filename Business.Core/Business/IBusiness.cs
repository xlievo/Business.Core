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

        dynamic ResultCreate(int state = 1, string message = null, [System.Runtime.CompilerServices.CallerMemberName] string method = null);

        Result.IResult<Data> ResultCreate<Data>(Data data, string message = null, int state = 1);

        Result.IResult ResultCreate(object data, string message = null, int state = 1);
    }

    public interface IBusiness<Result> : IBusiness where Result : Business.Result.IResult { }

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
        /// <param name="state"></param>
        /// <param name="message"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public dynamic ResultCreate(int state = 1, string message = null, [System.Runtime.CompilerServices.CallerMemberName] string method = null) => Business.Result.ResultFactory.ResultCreate(this.Configer.MetaData[method], state, message);

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <typeparam name="Data"></typeparam>
        /// <param name="data"></param>
        /// <param name="message"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public Business.Result.IResult<Data> ResultCreate<Data>(Data data, string message = null, int state = 1) => Business.Result.ResultFactory.ResultCreate(this.Configer.ResultTypeDefinition, data, message, state);

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <param name="data"></param>
        /// <param name="message"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public Business.Result.IResult ResultCreate(object data, string message = null, int state = 1) => Business.Result.ResultFactory.ResultCreate(this.Configer.ResultTypeDefinition, data, message, state);

        #endregion
    }

    public abstract class BusinessBase : BusinessBase<Result.ResultObject<string>> { }
}
