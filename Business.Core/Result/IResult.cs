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

    public static class ResultFactory
    {
        //public static System.Type GetResultType<Result>() where Result : IResult => typeof(Result).GetGenericTypeDefinition();

        internal static int ConvertErrorState(this int state) => 0 < state ? 0 - System.Math.Abs(state) : state;

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
    }
}