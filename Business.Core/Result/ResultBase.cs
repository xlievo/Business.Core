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
    public struct ResultBase<Type> : IResult<Type>
    {
        /// <summary>
        /// The results of the state is greater than or equal to 1: success, equal to 0: not to capture the system level exceptions, less than 0: business class error.
        /// </summary>
        public System.Int32 State { get; set; }

        /// <summary>
        /// Success can be null
        /// </summary>
        public System.String Message { get; set; }

        /// <summary>
        /// Specific dynamic data objects
        /// </summary>
        dynamic IResult.Data { get { return this.Data; } set { this.Data = value; } }

        /// <summary>
        /// Specific Byte/Json data objects
        /// </summary>
        public Type Data { get; set; }

        /// <summary>
        /// Whether there is value
        /// </summary>
        public System.Boolean HasData { get; set; }

        public System.Type DataType { get; set; }

        /// <summary>
        /// Gets the token of this result, used for callback
        /// </summary>
        public string Callback { get; set; }

        public ICommand Command { get; set; }

        /// <summary>
        /// Json Data
        /// </summary>
        /// <returns></returns>
        [System.Obsolete("Not implemented")]
        public string ToDataString()
        {
            //return Newtonsoft.Json.JsonConvert.SerializeObject(this.Data);
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// ProtoBuf
        /// </summary>
        /// <returns></returns>
        [System.Obsolete("Not implemented")]
        public byte[] ToBytes()
        {
            //return Extensions.Help.ProtoBufSerialize(this);
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// ProtoBuf Data
        /// </summary>
        /// <returns></returns>
        [System.Obsolete("Not implemented")]
        public byte[] ToDataBytes()
        {
            //return Extensions.Help.ProtoBufSerialize(this.Data);
            throw new System.NotImplementedException();
        }
    }
}