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

namespace Business.Core
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

        /*
        /// <summary>
        /// Business member accessor
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        object this[string member] { get; set; }
        */
    }

    public interface IBusiness<Result, Arg> : IBusiness where Result : Core.Result.IResult where Arg : IArg, new() { }

    public interface IBusiness<Result> : IBusiness<Result, Arg<object>> where Result : Core.Result.IResult { }

    public abstract partial class BusinessBase<Result, Arg> : IBusiness<Result, Arg>
        where Result : Core.Result.IResult
        where Arg : IArg, new()
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
        public dynamic ResultCreate(int state = 1, string message = null, [System.Runtime.CompilerServices.CallerMemberName] string method = null) => this.Configer.MetaData.TryGetValue(method ?? string.Empty, out Meta.MetaData meta) ? Core.Result.ResultFactory.ResultCreate(meta, state, message) : Core.Result.ResultFactory.ResultCreate(this.Configer.ResultTypeDefinition, state, message);

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <typeparam name="Data"></typeparam>
        /// <param name="data"></param>
        /// <param name="message"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public Core.Result.IResult<Data> ResultCreate<Data>(Data data, string message = null, int state = 1) => Core.Result.ResultFactory.ResultCreate(this.Configer.ResultTypeDefinition, data, message, state);

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <param name="data"></param>
        /// <param name="message"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public Core.Result.IResult ResultCreate(object data, string message = null, int state = 1) => Core.Result.ResultFactory.ResultCreate(this.Configer.ResultTypeDefinition, data, message, state);

        #endregion

        /*
        /// <summary>
        /// Business member accessor
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public object this[string member]
        {
            get
            {
                if (!Configer.Accessors.TryGetValue(this.Configer.Info.BusinessName, out Utils.Accessors meta) || !meta.Accessor.TryGetValue(member, out Utils.Accessor accessor)) { return null; }

                return accessor.Getter(this);
            }
            set
            {
                if (!Configer.Accessors.TryGetValue(this.Configer.Info.BusinessName, out Utils.Accessors meta) || !meta.Accessor.TryGetValue(member, out Utils.Accessor accessor)) { return; }

                try
                {
                    var value2 = Utils.Help.ChangeType(value, accessor.Type);
                    accessor.Setter(this, value2);
                }
                catch { }
            }
        }
        */
    }

    public abstract class BusinessBase<Result> : BusinessBase<Result, Arg<object>>, IBusiness<Result> where Result : Core.Result.IResult { }

    public abstract class BusinessBase : BusinessBase<Result.ResultObject<object>> { }
}
