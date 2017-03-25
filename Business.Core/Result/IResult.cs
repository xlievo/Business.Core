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
            this.commandID = new System.Collections.Generic.List<string>();
            this.commandID.AddRange(commandID);
        }

        public Command(params IResult[] notifys)
        {
            this.overall = false;
            this.notifys = new System.Collections.Generic.List<IResult>();
            this.commandID = new System.Collections.Generic.List<string>();
            this.notifys.AddRange(notifys);
        }

        readonly System.Collections.Generic.List<string> commandID;
        public System.Collections.Generic.List<string> CommandID { get { return commandID; } }

        bool overall;
        public bool Overall { get { return overall; } set { overall = value; } }

        readonly System.Collections.Generic.List<IResult> notifys;
        public System.Collections.Generic.List<IResult> Notifys { get { return notifys; } }
    }

    public static class ResultFactory
    {
        public static int GetState(int state)
        {
            return 0 < state ? 0 - System.Math.Abs(state) : state;
        }

        public static Result Create<Result>(bool overall = false, string callback = null, params IResult[] notifys)
            where Result : IResult, new()
        {
            System.Type[] genericArguments;
            if (!typeof(IResult<>).IsAssignableFrom(typeof(Result), out genericArguments))
            {
                throw new System.Exception("Result type is not generic IResult<>.");
            }
            return new Result() { State = 1, DataType = genericArguments[0], Command = new Command(notifys) { Overall = overall }, Callback = callback };
        }

        public static Result Create<Result>(int state, bool overall, string callback = null, params IResult[] notifys)
            where Result : IResult, new()
        {
            System.Type[] genericArguments;
            if (!typeof(IResult<>).IsAssignableFrom(typeof(Result), out genericArguments))
            {
                throw new System.Exception("Result type is not generic IResult<>.");
            }
            return new Result() { State = state, DataType = genericArguments[0], Command = new Command(notifys) { Overall = overall }, Callback = callback };
        }

        public static Result Create<Result>(int state, string message, bool overall = false, string callback = null, params IResult[] notifys)
           where Result : IResult, new()
        {
            System.Type[] genericArguments;
            if (!typeof(IResult<>).IsAssignableFrom(typeof(Result), out genericArguments))
            {
                throw new System.Exception("Result type is not generic IResult<>.");
            }
            return new Result() { State = GetState(state), Message = message, DataType = genericArguments[0], Command = new Command(notifys) { Overall = overall }, Callback = callback };
        }

        public static IResult<Data> Create<Data>(Data data, IResult<Data> result, int state = 1, bool overall = false, string callback = null, params IResult[] notifys)
        {
            if (1 > state) { state = System.Math.Abs(state); }

            result.State = state;
            result.Data = data;
            result.HasData = !System.Object.Equals(null, data);
            result.DataType = typeof(Data);
            result.Command = new Command(notifys) { Overall = overall };
            result.Callback = callback;
            return result;
        }

        public static IResult<Data> Create<Data>(Data data, IResult<Data> result, int state = 1, params string[] commandID)
        {
            if (1 > state) { state = System.Math.Abs(state); }

            result.State = state;
            result.Data = data;
            result.HasData = !System.Object.Equals(null, data);
            result.DataType = typeof(Data);
            result.Command = new Command(commandID);
            return result;
        }
        
        static IResult ResultCreate(IBusiness business, System.Type type)
        {
            var result = (IResult)System.Activator.CreateInstance(business.ResultType.MakeGenericType(type));
            result.DataType = type;
            return result;
        }

        public static IResult ResultCreate(this IBusiness business, bool overall = false, string callback = null, params IResult[] notifys)
        {
            return ResultCreate<string>(business, null, 1, overall, callback, notifys);
        }

        public static IResult ResultCreate(this IBusiness business, int state, bool overall, string callback = null, params IResult[] notifys)
        {
            return ResultCreate<string>(business, null, state, overall, callback, notifys);
        }

        public static IResult ResultCreate(this IBusiness business, int state, string message, bool overall = false, string callback = null, params IResult[] notifys)
        {
            var result = ResultCreate<string>(business, null, 1, overall, callback, notifys);
            result.State = GetState(state);
            result.Message = message;
            return result;
        }
        
        public static IResult ResultCreate<Data>(this IBusiness business, Data data, int state = 1, bool overall = false, string callback = null, params IResult[] notifys)
        {
            var result = ResultCreate(business, typeof(Data));

            if (1 > state) { state = System.Math.Abs(state); }

            result.State = state;
            result.Data = data;
            result.HasData = !System.Object.Equals(null, data);
            result.Command = new Command(notifys) { Overall = overall };
            result.Callback = callback;
            return result;
        }

        public static IResult ResultCreate<Data>(IBusiness business, Data data, int state = 1, params string[] commandID)
        {
            var result = ResultCreate(business, typeof(Data));

            if (1 > state) { state = System.Math.Abs(state); }

            result.State = state;
            result.Data = data;
            result.HasData = !System.Object.Equals(null, data);
            result.Command = new Command(commandID);
            return result;
        }

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

            IResult result2 = null;

            if (0 < result.State)
            {
                if (result.HasData)
                {
                    result2 = ResultCreate(business, result.ToDataBytes(), result.State);
                }
                else
                {
                    result2 = ResultCreate(business, result.State, result.Command.Overall);
                }
            }
            else
            {
                result2 = ResultCreate(business, result.State, result.Message);
            }

            //====================================//
            result2.Callback = result.Callback;
            result2.Command = result.Command;

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

            IResult result2 = null;

            if (0 < result.State)
            {
                if (result.HasData)
                {
                    result2 = ResultCreate(business, result.ToDataString(), result.State);
                }
                else
                {
                    result2 = ResultCreate(business, result.State, result.Command.Overall);
                }
            }
            else
            {
                result2 = ResultCreate(business, result.State, result.Message);
            }

            //====================================//
            result2.Callback = result.Callback;
            result2.Command = result.Command;

            return result2;
        }

        //==================================================================//

        public static IResult Create()
        {
            return Create<ResultBase<string>>();
        }

        public static IResult Create(int state)
        {
            return Create<ResultBase<string>>(state, false);
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
    }
}