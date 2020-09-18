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
    /// result
    /// </summary>
    /// <typeparam name="Type"></typeparam>
    public struct ResultObject<Type> : IResult<Type>
    {
        //public static implicit operator ResultObject<Type>(string value) => Utils.Help.TryJsonDeserialize<ResultObject<Type>>(value);

        //public static implicit operator ResultObject<Type>(byte[] value) => Utils.Help.TryProtoBufDeserialize<ResultObject<Type>>(value);

        /// <summary>
        /// Activator.CreateInstance
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="data"></param>
        /// <param name="state"></param>
        /// <param name="message"></param>
        /// <param name="callback"></param>
        /// <param name="genericDefinition"></param>
        /// <param name="checkData"></param>
        /// <param name="hasDataResult"></param>
        public ResultObject(System.Type dataType, Type data, int state = 1, string message = null, string callback = null, System.Type genericDefinition = null, bool checkData = true, bool hasDataResult = false)
        {
            this.DataType = dataType;
            this.Data = data;
            this.State = state;
            this.Message = message;
            this.HasData = checkData && !Equals(null, data);

            this.Callback = callback;
            this.GenericDefinition = genericDefinition;
            this.HasDataResult = hasDataResult;
        }

        /// <summary>
        /// MessagePack.MessagePackSerializer.Serialize(this)
        /// </summary>
        /// <param name="data"></param>
        /// <param name="state"></param>
        /// <param name="message"></param>
        /// <param name="hasData"></param>
        public ResultObject(Type data, int state, string message, bool hasData)
        {
            this.Data = data;
            this.State = state;
            this.Message = message;
            this.HasData = hasData;

            this.Callback = null;
            this.DataType = null;
            this.GenericDefinition = null;
            this.HasDataResult = false;
        }

        /// <summary>
        /// The results of the state is greater than or equal to 1: success, equal to 0: system level exceptions, less than 0: business class error.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("S")]
        public int State { get; }

        /// <summary>
        /// Success can be null
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("M")]
        public string Message { get; }

        /// <summary>
        /// Specific dynamic data objects
        /// </summary>
        dynamic IResult.Data { get => Data; }

        /// <summary>
        /// Specific Byte/Json data objects
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("D")]
        public Type Data { get; }

        /// <summary>
        /// Whether there is value
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("H")]
        public bool HasData { get; }

        /// <summary>
        /// Gets the token of this result, used for callback
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public string Callback { get; set; }

        /// <summary>
        /// Data type
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public System.Type DataType { get; }

        /// <summary>
        /// GenericDefinition
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public System.Type GenericDefinition { get; }

        /// <summary>
        /// Return data or not
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public bool HasDataResult { get; }

        /// <summary>
        /// Json format
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Utils.Help.JsonSerialize(this);

        /// <summary>
        /// ProtoBuf,MessagePack or Other
        /// </summary>
        /// <param name="dataBytes"></param>
        /// <returns></returns>
        public byte[] ToBytes(bool dataBytes = true) => throw new System.NotImplementedException(); //Utils.Help.ProtoBufSerialize(this);
    }
}