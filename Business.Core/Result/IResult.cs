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

namespace Business.Core.Result
{
    /// <summary>
    /// IResult
    /// </summary>
    public interface IResult
    {
        /// <summary>
        /// The results of the state is greater than or equal 
        /// to 1: success, equal to 0: system level exceptions, less than 0: business class error.
        /// </summary>
        int State { get; }

        /// <summary>
        /// Success can be null
        /// </summary>
        string Message { get; }

        /// <summary>
        /// Specific Byte/Json data objects
        /// </summary>
        dynamic Data { get; }

        /// <summary>
        /// Whether there is value
        /// </summary>
        bool HasData { get; }

        /// <summary>
        /// Data type
        /// </summary>
        System.Type DataType { get; }

        /// <summary>
        /// Gets the token of this result, used for callback
        /// </summary>
        string Callback { get; set; }

        ///// <summary>
        ///// Json data
        ///// </summary>
        ///// <returns></returns>
        //string ToDataString();

        ///// <summary>
        ///// Byte data
        ///// </summary>
        ///// <returns></returns>
        //byte[] ToDataBytes();

        /// <summary>
        /// ProtoBuf,MessagePack or Other
        /// </summary>
        /// <param name="dataBytes"></param>
        /// <returns></returns>
        byte[] ToBytes(bool dataBytes = true);

        /// <summary>
        /// Json
        /// </summary>
        /// <returns></returns>
        string ToString();

        /// <summary>
        /// System.Type object that represents a generic type definition from which the current generic type can be constructed.
        /// </summary>
        System.Type GenericDefinition { get; }

        /// <summary>
        /// Return data or not
        /// </summary>
        bool HasDataResult { get; }
    }

    /// <summary>
    /// IResult
    /// </summary>
    /// <typeparam name="DataType"></typeparam>
    public interface IResult<DataType> : IResult
    {
        /// <summary>
        /// Specific Byte/Json data objects
        /// </summary>
        new DataType Data { get; }
    }

    /// <summary>
    /// Result factory
    /// </summary>
    public static class ResultFactory
    {
        internal static int ConvertErrorState(this int state) => 0 < state ? 0 - System.Math.Abs(state) : state;

        // Annotations use
        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <param name="resultType"></param>
        /// <param name="resultTypeDefinition"></param>
        /// <param name="state"></param>
        /// <param name="message"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static IResult ResultCreate(System.Type resultType, System.Type resultTypeDefinition, int state = 1, string message = null, string callback = null) => (IResult)System.Activator.CreateInstance(resultType, new object[] { resultType.GenericTypeArguments[0], default, state, message, callback, resultTypeDefinition, false, false, false });

        // Interceptor use
        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="state"></param>
        /// <param name="message"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static IResult ResultCreate(Meta.MetaData meta, int state = 1, string message = null, string callback = null) => ResultCreate(meta.ResultType, meta.ResultTypeDefinition, state, message, callback);

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <typeparam name="Data"></typeparam>
        /// <param name="resultTypeDefinition"></param>
        /// <param name="data"></param>
        /// <param name="message"></param>
        /// <param name="state"></param>
        /// <param name="callback"></param>
        /// <param name="checkData"></param>
        /// <param name="hasData"></param>
        /// <param name="hasDataResult"></param>
        /// <returns></returns>
        public static IResult<Data> ResultCreate<Data>(this System.Type resultTypeDefinition, Data data = default, string message = null, int state = 1, string callback = null, bool checkData = true, bool hasData = false, bool hasDataResult = true) => (IResult<Data>)ResultCreate(resultTypeDefinition, typeof(Data), data, message, state, callback, checkData, hasData, hasDataResult);

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <param name="resultTypeDefinition"></param>
        /// <param name="dataType"></param>
        /// <param name="data"></param>
        /// <param name="message"></param>
        /// <param name="state"></param>
        /// <param name="callback"></param>
        /// <param name="checkData"></param>
        /// <param name="hasData"></param>
        /// <param name="hasDataResult"></param>
        /// <returns></returns>
        public static IResult ResultCreate(this System.Type resultTypeDefinition, System.Type dataType, object data = null, string message = null, int state = 1, string callback = null, bool checkData = true, bool hasData = false, bool hasDataResult = true)
        {
            var result = (IResult)System.Activator.CreateInstance(resultTypeDefinition.MakeGenericType(dataType), new object[] { dataType, data, state, message, callback, resultTypeDefinition, checkData, hasData, hasDataResult });
            //0 > state ? System.Math.Abs(state) : state
            return result;
        }

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <typeparam name="Data"></typeparam>
        /// <param name="resultTypeDefinition"></param>
        /// <param name="state"></param>
        /// <param name="message"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static IResult<Data> ResultCreate<Data>(this System.Type resultTypeDefinition, int state, string message, string callback = null) => ResultCreate<Data>(resultTypeDefinition, state: state, message: message, callback: callback, checkData: false, hasDataResult: false);

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <param name="resultTypeDefinition"></param>
        /// <param name="state"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static IResult ResultCreate(this System.Type resultTypeDefinition, int state = 1, string message = null) => ResultCreate<string>(resultTypeDefinition, state, message);

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <param name="resultTypeDefinition"></param>
        /// <param name="state"></param>
        /// <param name="message"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static IResult ResultCreate(this System.Type resultTypeDefinition, int state = 1, string message = null, string callback = null) => ResultCreate<string>(resultTypeDefinition, state, message, callback);

        ///// <summary>
        ///// Used to create IResult.Data secondary encapsulation
        ///// </summary>
        ///// <param name="result"></param>
        ///// <returns></returns>
        //public static IResult ResultCreateToDataBytes(this IResult result)
        //{
        //    if (Equals(null, result))
        //    {
        //        return null;
        //    }

        //    IResult result2;

        //    if (result.HasDataResult)
        //    {
        //        result2 = ResultCreate(result.GenericDefinition, result.HasData ? result.ToDataBytes() : null, result.Message, result.State);
        //    }
        //    else
        //    {
        //        result2 = ResultCreate(result.GenericDefinition, result.State, result.Message);
        //    }
        //    //====================================//
        //    result2.Callback = result.Callback;

        //    return result2;
        //}

        ///// <summary>
        ///// Used to create IResult.Data secondary encapsulation
        ///// </summary>
        ///// <param name="result"></param>
        ///// <returns></returns>
        //public static IResult ResultCreateToDataString(this IResult result)
        //{
        //    if (Equals(null, result))
        //    {
        //        return null;
        //    }

        //    IResult result2;

        //    if (result.HasDataResult)
        //    {
        //        result2 = ResultCreate(result.GenericDefinition, result.HasData ? result.ToDataString() : null, result.Message, result.State);
        //    }
        //    else
        //    {
        //        result2 = ResultCreate(result.GenericDefinition, result.State, result.Message);
        //    }
        //    //====================================//
        //    result2.Callback = result.Callback;

        //    return result2;
        //}
    }
}