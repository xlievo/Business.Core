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
    /// <summary>
    /// IBusiness
    /// </summary>
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

        ///// <summary>
        ///// Get token
        ///// </summary>
        //System.Func<dynamic, Auth.IToken, System.Threading.Tasks.Task<Auth.IToken>> GetToken { get; set; }

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        /// <param name="callback"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        dynamic ResultCreate(int state = 1, string message = null, string callback = null, [System.Runtime.CompilerServices.CallerMemberName] string method = null);

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <typeparam name="Data"></typeparam>
        /// <param name="data"></param>
        /// <param name="message"></param>
        /// <param name="state"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        Result.IResult<Data> ResultCreate<Data>(Data data, string message = null, int state = 1, string callback = null);

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <param name="data"></param>
        /// <param name="message"></param>
        /// <param name="state"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        Result.IResult ResultCreate(object data, string message = null, int state = 1, string callback = null);

        /*
        /// <summary>
        /// Business member accessor
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        object this[string member] { get; set; }
        */
    }

    /// <summary>
    /// IBusiness
    /// </summary>
    /// <typeparam name="Result"></typeparam>
    /// <typeparam name="Arg"></typeparam>
    public interface IBusiness<Result, Arg> : IBusiness where Result : Core.Result.IResult where Arg : IArg, new() { }

    /// <summary>
    /// IBusiness
    /// </summary>
    /// <typeparam name="Result"></typeparam>
    public interface IBusiness<Result> : IBusiness<Result, Arg<object>> where Result : Core.Result.IResult { }

    /// <summary>
    /// BusinessBase
    /// </summary>
    public abstract class BusinessBase : IBusiness
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
        /// <param name="callback"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public dynamic ResultCreate(int state = 1, string message = null, string callback = null, [System.Runtime.CompilerServices.CallerMemberName] string method = null) => this.Configer.MetaData.TryGetValue(method ?? string.Empty, out Meta.MetaData meta) ? Result.ResultFactory.ResultCreate(meta, state, message, callback) : Result.ResultFactory.ResultCreate(this.Configer.ResultTypeDefinition, state, message, callback);

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <typeparam name="Data"></typeparam>
        /// <param name="data"></param>
        /// <param name="message"></param>
        /// <param name="state"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public Result.IResult<Data> ResultCreate<Data>(Data data, string message = null, int state = 1, string callback = null) => Result.ResultFactory.ResultCreate(this.Configer.ResultTypeDefinition, data, message, state, callback);

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <param name="data"></param>
        /// <param name="message"></param>
        /// <param name="state"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public Result.IResult ResultCreate(object data, string message = null, int state = 1, string callback = null) => Result.ResultFactory.ResultCreate(this.Configer.ResultTypeDefinition, data, message, state, callback);

        #endregion
    }

    /// <summary>
    /// BusinessBase
    /// </summary>
    /// <typeparam name="Result"></typeparam>
    /// <typeparam name="Arg"></typeparam>
    public abstract partial class BusinessBase<Result, Arg> : BusinessBase, IBusiness<Result, Arg>
        where Result : Core.Result.IResult
        where Arg : IArg, new()
    {
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

    /// <summary>
    /// BusinessBase
    /// </summary>
    /// <typeparam name="Result"></typeparam>
    public abstract class BusinessBase<Result> : BusinessBase<Result, Arg<object>>, IBusiness<Result> where Result : Core.Result.IResult { }
}