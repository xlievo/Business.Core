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
    using Business.Extensions;

    public interface IResult
    {
        /// <summary>
        /// The results of the state is greater than or equal to 1: success, equal to 0: not to capture the system level exceptions, less than 0: business class error.
        /// </summary>
        System.Int32 State { get; set; }

        /// <summary>
        /// Success can be null
        /// </summary>
        System.String Message { get; set; }

        /// <summary>
        /// Specific Byte/Json data objects
        /// </summary>
        dynamic Data { get; set; }

        /// <summary>
        /// Whether there is value
        /// </summary>
        System.Boolean HasData { get; set; }

        System.Type DataType { get; set; }

        /// <summary>
        /// Gets the token of this result, used for callback
        /// </summary>
        System.String Callback { get; set; }

        ///// <summary>
        ///// Whether to notify all
        ///// </summary>
        //System.Boolean Overall { get; set; }

        ///// <summary>
        ///// Socket identity
        ///// </summary>
        //System.String CommandID { get; set; }

        ///// <summary>
        ///// Notify list
        ///// </summary>
        //System.Collections.Generic.IEnumerable<IResult> Notifys { get; set; }

        ICommand Command { get; set; }

        /// <summary>
        /// Json Data
        /// </summary>
        /// <returns></returns>
        System.String ToDataString();

        /// <summary>
        /// ProtoBuf Data
        /// </summary>
        /// <returns></returns>
        System.Byte[] ToDataBytes();

        /// <summary>
        /// ProtoBuf
        /// </summary>
        /// <returns></returns>
        System.Byte[] ToBytes();

        /// <summary>
        /// Json
        /// </summary>
        /// <returns></returns>
        System.String ToString();
    }

    public interface IResult<DataType> : IResult
    {
        /// <summary>
        /// Specific Byte/Json data objects
        /// </summary>
        new DataType Data { get; set; }
    }

    public interface ICommand
    {
        /// <summary>
        /// Whether to notify all
        /// </summary>
        System.Boolean Overall { get; set; }

        /// <summary>
        /// Socket identity
        /// </summary>
        System.String CommandID { get; set; }

        /// <summary>
        /// Notify list
        /// </summary>
        System.Collections.Generic.IEnumerable<IResult> Notifys { get; set; }
    }

    public static class ResultFactory
    {
        public static int GetState(int state)
        {
            return 0 < state ? 0 - System.Math.Abs(state) : state;
        }

        public static Result Create<Result>()
            where Result : IResult, new()
        {
            System.Type[] genericArguments;
            if (!typeof(IResult<>).IsAssignableFrom(typeof(Result), out genericArguments))
            {
                throw new System.Exception("Result type is not generic IResult<>.");
            }
            return new Result() { State = 1, DataType = genericArguments[0] };
        }

        public static Result Create<Result>(int state)
            where Result : IResult, new()
        {
            System.Type[] genericArguments;
            if (!typeof(IResult<>).IsAssignableFrom(typeof(Result), out genericArguments))
            {
                throw new System.Exception("Result type is not generic IResult<>.");
            }
            return new Result() { State = state, DataType = genericArguments[0] };
        }

        public static Result Create<Result>(int state, string message)
           where Result : IResult, new()
        {
            System.Type[] genericArguments;
            if (!typeof(IResult<>).IsAssignableFrom(typeof(Result), out genericArguments))
            {
                throw new System.Exception("Result type is not generic IResult<>.");
            }
            return new Result() { State = GetState(state), Message = message, DataType = genericArguments[0] };
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
        static IResult ResultCreate(IBusiness business, System.Type type)
        {
            var result = (IResult)System.Activator.CreateInstance(business.ResultType.MakeGenericType(type));
            result.DataType = type;
            return result;
        }

        public static IResult ResultCreate(this IBusiness business)//, bool overall = false, string commandID = null, params IResult[] notifys
        {
            var result = ResultCreate(business, typeof(string));

            result.State = 1;
            //result.Overall = overall;
            //result.CommandID = commandID;
            //result.Notifys = notifys;
            //result.Socket = new ISocket { Overall = overall, List = list };

            return result;
        }

        public static IResult ResultCreate(this IBusiness business, int state)
        {
            var result = ResultCreate(business, typeof(string));

            result.State = state;
            //result.Overall = overall;
            //result.CommandID = commandID;
            //result.Notifys = notifys;
            //result.Socket = new ISocket { Overall = overall, List = list };

            return result;
        }

        public static IResult ResultCreate(this IBusiness business, int state, string message)
        {
            var result = ResultCreate(business, typeof(string));

            result.State = GetState(state);
            result.Message = message;
            //result.Overall = overall;
            //result.CommandID = commandID;
            //result.Notifys = notifys;
            //result.Socket = new ISocket { Overall = overall, List = list };

            return result;
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
        public static IResult ResultCreate<Data>(this IBusiness business, Data data, int state = 1)
        {
            var result = ResultCreate(business, typeof(Data));

            if (1 > state) { state = System.Math.Abs(state); }

            result.State = state;
            result.Data = data;
            result.HasData = !System.Object.Equals(null, data);
            //result.Overall = overall;
            //result.CommandID = commandID;
            //result.Notifys = notifys;
            //result.Socket = new ISocket { List = list };

            return result;// as IResult<Data>;
        }

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


        public static IResult ResultCreateToDataBytes(this IBusiness business, IResult result)
        {
            if (System.Object.Equals(null, business))
            {
                return null;
            }

            if (System.Object.Equals(null, result))
            {
                return null;
            }

            IResult result2;

            if (0 < result.State)
            {
                if (result.HasData)
                {
                    result2 = ResultCreate(business, result.ToDataBytes(), result.State);
                }
                else
                {
                    result2 = ResultCreate(business, result.State);
                }
            }
            else
            {
                result2 = ResultCreate(business, result.State, result.Message);
            }

            //====================================//
            result2.Callback = result.Callback;
            //result2.Overall = result.Overall;
            //result2.CommandID = result.CommandID;
            //result2.Notifys = result.Notifys;

            return result2;
        }

        public static IResult ResultCreateToDataString(this IBusiness business, IResult result)
        {
            if (System.Object.Equals(null, business))
            {
                return null;
            }

            if (System.Object.Equals(null, result))
            {
                return null;
            }

            IResult result2;

            if (0 < result.State)
            {
                if (result.HasData)
                {
                    result2 = ResultCreate(business, result.ToDataString(), result.State);
                }
                else
                {
                    result2 = ResultCreate(business, result.State);
                }
            }
            else
            {
                result2 = ResultCreate(business, result.State, result.Message);
            }

            //====================================//
            result2.Callback = result.Callback;

            return result2;
        }

        //==================================================================//

        public static IResult Create()
        {
            return Create<ResultBase<string>>();
        }

        public static IResult Create(int state)
        {
            return Create<ResultBase<string>>(state);
        }

        public static IResult Create(int state, string message)
        {
            return Create<ResultBase<string>>(state, message);
        }

        public static IResult<Data> Create<Data>(Data data, int state = 1)
        {
            return Create<Data>(data, new ResultBase<Data>(), state);
        }

        //========================================================//

        //    public static IResult<DataType> DeserializeResultProtoBuf<DataType, Result>(this byte[] source)
        //where Result : class, IResult<DataType>, new()
        //    {
        //        return Extensions.Help.ProtoBufDeserialize<Result>(source);
        //    }
    }
}