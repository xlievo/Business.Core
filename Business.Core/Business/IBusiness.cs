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

        #endregion
    }

    public abstract class BusinessBase : BusinessBase<Result.ResultObject<string>> { }
}
