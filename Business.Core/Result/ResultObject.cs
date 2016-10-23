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
    public struct ResultObject<Type> : IResult<Type>
    {
        public static implicit operator ResultObject<Type>(string value)
        {
            return Business.Extensions.Help.JsonDeserialize<ResultObject<Type>>(value);
        }
        public static implicit operator ResultObject<Type>(byte[] value)
        {
            return Business.Extensions.Help.ProtoBufDeserialize<ResultObject<Type>>(value);
        }

        /// <summary>
        /// The results of the state is greater than or equal to 1: success, equal to 0: not to capture the system level exceptions, less than 0: business class error.
        /// </summary>
        [ProtoBuf.ProtoMember(1, Name = "S")]
        [Newtonsoft.Json.JsonProperty(PropertyName = "S")]
        public System.Int32 State { get; set; }

        /// <summary>
        /// Success can be null
        /// </summary>
        [ProtoBuf.ProtoMember(2, Name = "M")]
        [Newtonsoft.Json.JsonProperty(PropertyName = "M")]
        public System.String Message { get; set; }

        /// <summary>
        /// Specific Byte/Json data objects
        /// </summary>
        [ProtoBuf.ProtoMember(3, Name = "D")]
        [Newtonsoft.Json.JsonProperty(PropertyName = "D")]
        public Type Data { get; set; }

        /// <summary>
        /// Whether there is value
        /// </summary>
        [ProtoBuf.ProtoMember(4, Name = "H")]
        [Newtonsoft.Json.JsonProperty(PropertyName = "H")]
        public System.Boolean HasData { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public System.Type DataType { get; set; }

        /// <summary>
        /// Gets the token of this result, used for callback
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [ProtoBuf.ProtoMember(5, Name = "B")]
        public string Callback { get; set; }

        public ICommand Command { get; set; }

        /// <summary>
        /// Specific dynamic data objects
        /// </summary>
        dynamic IResult.Data { get { return this.Data; } set { this.Data = value; } }

        /// <summary>
        /// Json
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// Json Data
        /// </summary>
        /// <returns></returns>
        public string ToDataString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this.Data);
        }

        /// <summary>
        /// ProtoBuf
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            return Business.Extensions.Help.ProtoBufSerialize(this);
        }

        /// <summary>
        /// ProtoBuf Data
        /// </summary>
        /// <returns></returns>
        public byte[] ToDataBytes()
        {
            return Business.Extensions.Help.ProtoBufSerialize(this.Data);
        }
    }
}