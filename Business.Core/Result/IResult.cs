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
        /// System.Type object that represents a generic type definition from which the current generic type can be constructed.
        /// </summary>
        System.Type GenericDefinition { get; }
    }

    public interface IResult<DataType> : IResult
    {
        /// <summary>
        /// Specific Byte/Json data objects
        /// </summary>
        new DataType Data { get; }
    }

    public static class ResultFactory
    {
        //public static System.Type GetResultType<Result>() where Result : IResult => typeof(Result).GetGenericTypeDefinition();

        internal static int ConvertErrorState(this int state) => 0 < state ? 0 - System.Math.Abs(state) : state;

        // Annotations use
        internal static IResult CreateMeta(System.Type resultType, System.Type resultTypeDefinition, int state = 1, string message = null)
        {
            var type = resultType.GenericTypeArguments[0];
            var result = (IResult)System.Activator.CreateInstance(resultType, new object[] { type, type.IsValueType ? System.Activator.CreateInstance(type) : null, state, message, resultTypeDefinition });
            return result;
        }

        // Interceptor use
        internal static IResult CreateMeta(Meta.MetaData meta, int state = 1, string message = null) => CreateMeta(meta.ResultType, meta.ResultTypeDefinition, state, message);

        // Annotations use
        internal static IResult<Data> CreateMetaData<Data>(System.Type resultTypeDefinition, Data data = default, string message = null, int state = 1)
        {
            var type = typeof(Data);
            var result = (IResult<Data>)System.Activator.CreateInstance(resultTypeDefinition.MakeGenericType(type), new object[] { type, data, state, message, resultTypeDefinition });
            return result;
        }

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <typeparam name="Data"></typeparam>
        /// <param name="resultTypeDefinition"></param>
        /// <param name="data"></param>
        /// <param name="state"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        static IResult<Data> Create<Data>(System.Type resultTypeDefinition, Data data = default, int state = 1, string message = null)
        {
            var type = typeof(Data);
            var result = (IResult<Data>)System.Activator.CreateInstance(resultTypeDefinition.MakeGenericType(type), new object[] { type, data, state, message, resultTypeDefinition });
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
        static IResult<Data> Create<Data>(IBusiness business, Data data = default, int state = 1, string message = null) => Create(business.Configer.ResultTypeDefinition, data, state, message);

        #region resultType

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <param name="resultTypeDefinition"></param>
        /// <returns></returns>
        //public static IResult ResultCreate(this TypeInfo resultType, string message = null) => Create<string>(resultType, message: message);

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <param name="resultType"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static IResult ResultCreate(System.Type resultTypeDefinition, int state) => Create<string>(resultTypeDefinition, state: state);

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <param name="resultTypeDefinition"></param>
        /// <param name="state"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static IResult ResultCreate(System.Type resultTypeDefinition, int state = 1, string message = null) => Create<string>(resultTypeDefinition, state: state, message: message);

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <typeparam name="Data"></typeparam>
        /// <param name="resultTypeDefinition"></param>
        /// <param name="data"></param>
        /// <param name="message"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static IResult ResultCreate<Data>(System.Type resultTypeDefinition, Data data, string message = null, int state = 1) => Create(resultTypeDefinition, data, 0 > state ? System.Math.Abs(state) : state, message);

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

        public static IResult<Data> ResultCreate<Data>(IBusiness business, int state) => Create<Data>(business, state: state);

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <param name="business"></param>
        /// <param name="state"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static IResult ResultCreate(IBusiness business, int state = 1, string message = null) => Create<string>(business, state: state, message: message);

        public static IResult<Data> ResultCreate<Data>(IBusiness business, int state = 1, string message = null) => Create<Data>(business, state: state, message: message);

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <typeparam name="Data"></typeparam>
        /// <param name="business"></param>
        /// <param name="data"></param>
        /// <param name="message"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static IResult<Data> ResultCreate<Data>(IBusiness business, Data data, string message = null, int state = 1) => Create(business, data, 0 > state ? System.Math.Abs(state) : state, message);

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

            var result2 = Create(result.GenericDefinition, result.HasData ? result.ToDataBytes() : null, result.State, result.Message);
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
            var result2 = Create(result.GenericDefinition, result.HasData ? result.ToDataString() : null, result.State, result.Message);
            //====================================//
            result2.Callback = result.Callback;

            return result2;
        }
    }
}