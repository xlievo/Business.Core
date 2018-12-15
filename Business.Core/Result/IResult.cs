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

namespace Business.Result
{
    public interface IResult
    {
        /// <summary>
        /// The results of the state is greater than or equal 
        /// to 1: success, equal to 0: system level exceptions, less than 0: business class error.
        /// </summary>
        System.Int32 State { get; }

        /// <summary>
        /// Success can be null
        /// </summary>
        System.String Message { get; }

        /// <summary>
        /// Specific Byte/Json data objects
        /// </summary>
        dynamic Data { get; }

        /// <summary>
        /// Whether there is value
        /// </summary>
        System.Boolean HasData { get; }

        System.Type DataType { get; }

        /// <summary>
        /// Gets the token of this result, used for callback
        /// </summary>
        System.String Callback { get; set; }

        /// <summary>
        /// Json Data
        /// </summary>
        /// <returns></returns>
        System.String ToDataString();

        /// <summary>
        /// Byte Data
        /// </summary>
        /// <returns></returns>
        System.Byte[] ToDataBytes();

        /// <summary>
        /// ProtoBuf,MessagePack or Other
        /// </summary>
        /// <returns></returns>
        System.Byte[] ToBytes();

        /// <summary>
        /// Json
        /// </summary>
        /// <returns></returns>
        System.String ToString();

        /// <summary>
        /// Get generic data
        /// </summary>
        /// <typeparam name="DataType">Generic type</typeparam>
        /// <returns></returns>
        DataType Get<DataType>();

        /// <summary>
        /// 
        /// </summary>
        System.Type GenericType { get; }
    }

    public interface IResult<DataType> : IResult
    {
        /// <summary>
        /// Specific Byte/Json data objects
        /// </summary>
        new DataType Data { get; }
    }
    /*
    #region ICommand

    public interface ICommand
    {
        /// <summary>
        /// Socket identity
        /// </summary>
        System.Collections.Generic.List<System.String> CommandID { get; }

        /// <summary>
        /// Whether to notify all
        /// </summary>
        System.Boolean Overall { get; set; }

        /// <summary>
        /// Notify list
        /// </summary>
        System.Collections.Generic.List<IResult> Notifys { get; }
    }

    public struct Command : ICommand
    {
        public Command(params string[] commandID)
        {
            this.overall = false;
            this.notifys = new System.Collections.Generic.List<IResult>();
            this.commandID = new System.Collections.Generic.List<string>(commandID);
        }

        public Command(params IResult[] notifys)
        {
            this.overall = false;
            this.notifys = new System.Collections.Generic.List<IResult>();
            this.commandID = new System.Collections.Generic.List<string>();
            this.notifys.AddRange(notifys);
        }

        readonly System.Collections.Generic.List<string> commandID;
        public System.Collections.Generic.List<string> CommandID { get => commandID; }

        bool overall;
        public bool Overall { get => overall; set => overall = value; }

        readonly System.Collections.Generic.List<IResult> notifys;
        public System.Collections.Generic.List<IResult> Notifys { get => notifys; }
    }

    #endregion
    */
    public static class ResultFactory
    {
        internal static int ConvertErrorState(this int state) => 0 < state ? 0 - System.Math.Abs(state) : state;

        #region Create

        /*
public static Result Create<Result>()
    where Result : IResult, new()
{
    if (!typeof(IResult<>).GetTypeInfo().IsAssignableFrom(typeof(Result).GetTypeInfo(), out TypeInfo[] genericArguments))
    {
        throw new System.Exception("Result type is not generic IResult<>.");
    }
    return new Result() { State = 1, DataType = genericArguments[0].AsType() };
}

public static Result Create<Result>(int state)
    where Result : IResult, new()
{
    if (!typeof(IResult<>).GetTypeInfo().IsAssignableFrom(typeof(Result).GetTypeInfo(), out TypeInfo[] genericArguments))
    {
        throw new System.Exception("Result type is not generic IResult<>.");
    }
    return new Result() { State = state, DataType = genericArguments[0].AsType() };
}

public static Result Create<Result>(int state, string message)
   where Result : IResult, new()
{
    if (!typeof(IResult<>).GetTypeInfo().IsAssignableFrom(typeof(Result).GetTypeInfo(), out TypeInfo[] genericArguments))
    {
        throw new System.Exception("Result type is not generic IResult<>.");
    }
    return new Result() { State = GetState(state), Message = message, DataType = genericArguments[0].AsType() };
}

public static IResult<Data> Create<Data>(Data data, IResult<Data> result, int state = 1)
{
    if (1 > state) { state = System.Math.Abs(state); }

    result.State = state;
    result.Data = data;
    result.HasData = !System.Object.Equals(null, data);
    result.DataType = typeof(Data);
    return result;
}

public static IResult Create<Data>(TypeInfo resultType, Data data, int state = 1)=> ResultCreate2<Data>(resultType, data, 0 > state ? System.Math.Abs(state) : state);

#region Create.ResultBase

public static IResult Create() => Create<ResultBase<string>>();

public static IResult Create(int state) => Create<ResultBase<string>>(state);

public static IResult Create(int state, string message) => Create<ResultBase<string>>(state, message);

public static IResult<Data> Create<Data>(Data data, int state = 1) => Create<Data>(data, new ResultBase<Data>(), state);

#endregion
*/

        #endregion


        //public static IResult<Data> Create<Data>(Data data, IResult<Data> result, int state = 1)
        //{
        //    if (1 > state) { state = System.Math.Abs(state); }

        //    result.State = state;
        //    result.Data = data;
        //    result.HasData = !System.Object.Equals(null, data);
        //    result.DataType = typeof(Data);
        //    return result;
        //}
        //public static IResult Create<Data>(Data data, System.Type resultType, int state = 1)
        //{
        //    var type = resultType.MakeGenericType(typeof(Data));
        //    var result = (IResult)System.Activator.CreateInstance(type);

        //    if (1 > state) { state = System.Math.Abs(state); }

        //    result.State = state;
        //    result.Data = data;
        //    result.HasData = true;

        //    return result;
        //}
        //==================================================================//
        /*
        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <param name="business"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        static IResult ResultCreate(IBusiness business, System.Type type)
        {
            var result = (IResult)System.Activator.CreateInstance(business.Configuration.ResultType.MakeGenericType(type));
            result.DataType = type;
            return result;
        }
        */

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <typeparam name="Data"></typeparam>
        /// <param name="resultType"></param>
        /// <param name="data"></param>
        /// <param name="state"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        static IResult Create<Data>(System.Type resultType, Data data = default, int state = 1, string message = null)
        {
            var type = typeof(Data);
            var result = (IResult)System.Activator.CreateInstance(resultType.MakeGenericType(type), new object[] { data, type, state, message, resultType });
            return result;
        }
        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <typeparam name="Data"></typeparam>
        /// <param name="business"></param>
        /// <param name="data"></param>
        /// <param name="state"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        static IResult Create<Data>(IBusiness business, Data data = default, int state = 1, string message = null) => Create(business.Configer.ResultType, data, state, message);

        #region resultType

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <param name="resultType"></param>
        /// <returns></returns>
        //public static IResult ResultCreate(this TypeInfo resultType, string message = null) => Create<string>(resultType, message: message);

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <param name="resultType"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static IResult ResultCreate(System.Type resultType, int state) => Create<string>(resultType, state: state);

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <param name="resultType"></param>
        /// <param name="state"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static IResult ResultCreate(System.Type resultType, int state = 1, string message = null) => Create<string>(resultType, state: state, message: message);

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <typeparam name="Data"></typeparam>
        /// <param name="resultType"></param>
        /// <param name="data"></param>
        /// <param name="message"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static IResult ResultCreate<Data>(System.Type resultType, Data data, string message = null, int state = 1) => Create(resultType, data, 0 > state ? System.Math.Abs(state) : state, message);

        #endregion

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
        public static IResult ResultCreate(IBusiness business, int state) => Create<string>(business, state: state);

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <param name="business"></param>
        /// <param name="state"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static IResult ResultCreate(IBusiness business, int state = 1, string message = null) => Create<string>(business, state: state, message: message);

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <typeparam name="Data"></typeparam>
        /// <param name="business"></param>
        /// <param name="data"></param>
        /// <param name="message"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static IResult ResultCreate<Data>(IBusiness business, Data data, string message = null, int state = 1) => Create(business, data, 0 > state ? System.Math.Abs(state) : state, message);

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

        /// <summary>
        /// Used to create IResult.Data secondary encapsulation
        /// </summary>
        /// <param name="resultType"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static IResult ResultCreateToDataBytes(this IResult result)
        {
            //if (null == resultType)
            //{
            //    return null;
            //}

            if (System.Object.Equals(null, result))
            {
                return null;
            }

            //IResult result2 = null;

            //if (0 < result.State)
            //{
            //    if (result.HasData)
            //    {
            //        result2 = ResultCreate(resultType, result.ToDataBytes(), result.State);
            //    }
            //    else
            //    {
            //        result2 = ResultCreate(resultType, result.State);
            //    }
            //}
            //else
            //{
            //    result2 = ResultCreate(resultType, result.State, result.Message);
            //}

            var result2 = Create(result.GenericType, result.HasData ? result.ToDataBytes() : null, result.State, result.Message);
            //====================================//
            result2.Callback = result.Callback;

            return result2;
        }

        /// <summary>
        /// Used to create IResult.Data secondary encapsulation
        /// </summary>
        /// <param name="resultType"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static IResult ResultCreateToDataString(this IResult result)
        {
            if (System.Object.Equals(null, result))
            {
                return null;
            }

            //IResult result2 = null;

            //if (0 < result.State)
            //{
            //    if (result.HasData)
            //    {
            //        result2 = ResultCreate(resultType, result.ToDataString(), result.State);
            //    }
            //    else
            //    {
            //        result2 = ResultCreate(resultType, result.State);
            //    }
            //}
            //else
            //{
            //    result2 = ResultCreate(resultType, result.State, result.Message);
            //}
            //var r = result.GetType().GetGenericTypeDefinition();
            var result2 = Create(result.GenericType, result.HasData ? result.ToDataString() : null, result.State, result.Message);
            //====================================//
            result2.Callback = result.Callback;

            return result2;
        }

        //public static IResult ResultCreate<Data>(this Data data, IBusiness business, System.Type type, int state = 1)
        //{
        //    var result = ResultCreate(business, type);

        //    if (1 > state) { state = System.Math.Abs(state); }

        //    result.State = state;
        //    var obj = System.Convert.ChangeType(data, type);
        //    result.Data = data;
        //    result.HasData = true;

        //    return result;
        //}



        ///// <summary>
        ///// Used to create the IResult returns object
        ///// </summary>
        ///// <typeparam name="Data"></typeparam>
        ///// <param name="business"></param>
        ///// <param name="data"></param>
        ///// <param name="state"></param>
        ///// <param name="commandID"></param>
        ///// <returns></returns>
        //public static IResult ResultCreate<Data>(IBusiness business, Data data, int state = 1)
        //{
        //    var result = ResultCreate<Data>(business, data);

        //    if (1 > state) { state = System.Math.Abs(state); }

        //    result.State = state;
        //    //result.Data = data;
        //    result.HasData = !System.Object.Equals(null, data);
        //    return result;
        //}

        //public static IResult ResultCreate<Data>(this IBusiness business, Data data, bool overall = false, string commandID = null, params IResult[] list)
        //{
        //    return ResultCreate<Data>(business, data, 1, overall, commandID, list);
        //}
        //public static IResult ResultCreate<Data>(this IBusiness business, Data data, bool overall = false, params IResult[] list)
        //{
        //    return ResultCreate<Data>(business, data, 1, overall, null, list);
        //}

        //public static IResult ResultCreate<Data>(this IBusiness business, Data data, int state = 1, bool overall = false, string commandID = null)
        //{
        //    var result = ResultCreate(business, typeof(Data));

        //    if (1 > state) { state = System.Math.Abs(state); }

        //    result.State = state;
        //    result.Data = data;
        //    result.HasData = !System.Object.Equals(null, data);

        //    result.Overall = overall;
        //    result.CommandID = commandID;
        //    //result.Socket = new ISocket { CommandID = commandID };

        //    return result;// as IResult<Data>;
        //}



        //==================================================================//


        //========================================================//

        //    public static IResult<DataType> DeserializeResultProtoBuf<DataType, Result>(this byte[] source)
        //where Result : class, IResult<DataType>, new()
        //    {
        //        return Utils.Help.ProtoBufDeserialize<Result>(source);
        //    }
    }

    /*
    public interface IResultCreate
    {
        IResult OK { get; }

        IResult Error { get; }

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <returns></returns>
        IResult Create();

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        IResult Create(int state);

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        IResult Create(int state, string message);

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <typeparam name="Data"></typeparam>
        /// <param name="data"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        IResult Create<Data>(Data data, int state = 1);
    }

    public struct ResultCreate : IResultCreate
    {
        public ResultCreate(TypeInfo resultType, int state = -999, string message = "The default error")
        {
            this.resultType = resultType;
            this.state = state;
            this.message = message;
        }

        internal readonly TypeInfo resultType;
        internal readonly int state;
        internal readonly string message;

        public IResult OK { get => resultType.ResultCreate(); }
        public IResult Error { get => resultType.ResultCreate(state, message); }

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <param name="business"></param>
        /// <returns></returns>
        public IResult Create() => resultType.ResultCreate();

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <param name="business"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public IResult Create(int state) => resultType.ResultCreate(state);

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <param name="business"></param>
        /// <param name="state"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public IResult Create(int state, string message) => resultType.ResultCreate(state, message);

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <typeparam name="Data"></typeparam>
        /// <param name="business"></param>
        /// <param name="data"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public IResult Create<Data>(Data data, int state = 1) => resultType.ResultCreate(data, state);
    }
    */
}