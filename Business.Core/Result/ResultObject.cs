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
    /// <summary>
    /// Serialize result
    /// </summary>
    /// <typeparam name="Type"></typeparam>
    [ProtoBuf.ProtoContract(SkipConstructor = true)]
    public class ResultObject<Type> : IResult<Type>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator ResultObject<Type>(string value) => Utils.Help.TryJsonDeserialize<ResultObject<Type>>(value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator ResultObject<Type>(byte[] value) => Utils.Help.TryProtoBufDeserialize<ResultObject<Type>>(value);

        /// <summary>
        /// Activator.CreateInstance
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dataType"></param>
        /// <param name="state"></param>
        /// <param name="message"></param>
        public ResultObject(Type data, System.Type dataType, int state = 1, string message = null, System.Type genericType = null)
        {
            this.data = data;
            this.dataType = dataType;
            this.state = state;
            this.message = message;
            this.hasData = !System.Object.Equals(null, data);
            this.callback = default(string);

            this.genericType = genericType;
        }

        System.Int32 state;
        /// <summary>
        /// The results of the state is greater than or equal to 1: success, equal to 0: not to capture the system level exceptions, less than 0: business class error.
        /// </summary>
        [ProtoBuf.ProtoMember(1, Name = "S")]
        [Newtonsoft.Json.JsonProperty(PropertyName = "S")]
        public System.Int32 State { get => state; set => state = value; }

        System.String message;
        /// <summary>
        /// Success can be null
        /// </summary>
        [ProtoBuf.ProtoMember(2, Name = "M")]
        [Newtonsoft.Json.JsonProperty(PropertyName = "M")]
        public System.String Message { get => message; set => message = value; }

        /// <summary>
        /// Specific dynamic data objects
        /// </summary>
        dynamic IResult.Data { get => data; }

        Type data;
        /// <summary>
        /// Specific Byte/Json data objects
        /// </summary>
        [ProtoBuf.ProtoMember(3, Name = "D")]
        [Newtonsoft.Json.JsonProperty(PropertyName = "D")]
        public Type Data { get => data; set => data = value; }

        System.Boolean hasData;
        /// <summary>
        /// Whether there is value
        /// </summary>
        [ProtoBuf.ProtoMember(4, Name = "H")]
        [Newtonsoft.Json.JsonProperty(PropertyName = "H")]
        public System.Boolean HasData { get => hasData; set => hasData = value; }

        System.Type dataType;
        [Newtonsoft.Json.JsonIgnore]
        public System.Type DataType { get => dataType; set => dataType = value; }

        System.String callback;
        /// <summary>
        /// Gets the token of this result, used for callback
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [ProtoBuf.ProtoMember(5, Name = "B")]
        [Newtonsoft.Json.JsonProperty(PropertyName = "B")]
        public System.String Callback { get => callback; set => callback = value; }

        System.Type genericType;
        [Newtonsoft.Json.JsonIgnore]
        public System.Type GenericType => genericType;

        //ICommand command;
        //[Newtonsoft.Json.JsonIgnore]
        //public ICommand Command { get => command; set => command = value; }

        /// <summary>
        /// Json format
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Utils.Help.JsonSerialize(this);

        /// <summary>
        /// Json format Data
        /// </summary>
        /// <returns></returns>
        public string ToDataString() => Utils.Help.JsonSerialize(this.Data);

        /// <summary>
        /// ProtoBuf format
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes() => Utils.Help.ProtoBufSerialize(this);

        /// <summary>
        /// ProtoBuf format Data
        /// </summary>
        /// <returns></returns>
        public byte[] ToDataBytes() => Utils.Help.ProtoBufSerialize(this.Data);

        /// <summary>
        /// Get generic data
        /// </summary>
        /// <typeparam name="DataType">Generic type</typeparam>
        /// <returns></returns>
        public DataType Get<DataType>() => ((IResult)this).Data;
    }
}