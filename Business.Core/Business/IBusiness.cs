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
        /// <summary>
        /// Log subscription queue
        /// </summary>
        Logger Logger { get; set; }

        /// <summary>
        /// Call methods by command
        /// </summary>
        CommandGroup Command { get; set; }

        /// <summary>
        /// Configurer
        /// </summary>
        Configer Configer { get; set; }

        /// <summary>
        /// After binding
        /// </summary>
        System.Action BindAfter { get; set; }

        /// <summary>
        /// Before binding
        /// </summary>
        System.Action<Configer> BindBefore { get; set; }

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        dynamic ResultCreate(int state = 1, string message = null, [System.Runtime.CompilerServices.CallerMemberName] string method = null);

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <typeparam name="Data"></typeparam>
        /// <param name="data"></param>
        /// <param name="message"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        Result.IResult<Data> ResultCreate<Data>(Data data, string message = null, int state = 1);

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <param name="data"></param>
        /// <param name="message"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        Result.IResult ResultCreate(object data, string message = null, int state = 1);
    }

    public interface IBusiness<Result> : IBusiness where Result : Business.Result.IResult { }

    public abstract class BusinessBase<Result> : IBusiness<Result>
        where Result : Business.Result.IResult
    {
        /// <summary>
        /// Log subscription queue
        /// </summary>
        public Logger Logger { get; set; }

        /// <summary>
        /// Call methods by command
        /// </summary>
        public CommandGroup Command { get; set; }

        /// <summary>
        /// Configurer
        /// </summary>
        public Configer Configer { get; set; }

        /// <summary>
        /// After binding
        /// </summary>
        public System.Action BindAfter { get; set; }

        /// <summary>
        /// Before binding
        /// </summary>
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
