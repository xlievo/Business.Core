﻿/*==================================
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
    /// Serialize result
    /// </summary>
    /// <typeparam name="Type"></typeparam>
    public class ResultObject<Type> : IResult<Type>
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
        /// <param name="genericDefinition"></param>
        /// <param name="checkData"></param>
        public ResultObject(System.Type dataType, Type data, int state = 1, string message = null, System.Type genericDefinition = null, bool checkData = true)
        {
            this.dataType = dataType;
            this.data = data;
            this.state = state;
            this.message = message;
            this.hasData = checkData ? !object.Equals(null, data) : false;
            this.callback = default;

            this.genericDefinition = genericDefinition;
        }

        /// <summary>
        /// MessagePack.MessagePackSerializer.Serialize(this)
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        /// <param name="genericType"></param>
        public ResultObject(Type data, int state = 1, string message = null)
        {
            this.data = data;
            this.state = state;
            this.message = message;
            this.hasData = !object.Equals(null, data);
        }

        //public ResultObject(Type data, int state = 1, string message = null) : this(data, null, state, message) { }

        System.Int32 state;
        /// <summary>
        /// The results of the state is greater than or equal to 1: success, equal to 0: system level exceptions, less than 0: business class error.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("S")]
        public virtual System.Int32 State { get => state; set => state = value; }

        System.String message;
        /// <summary>
        /// Success can be null
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("M")]
        public virtual System.String Message { get => message; set => message = value; }

        /// <summary>
        /// Specific dynamic data objects
        /// </summary>
        dynamic IResult.Data { get => data; }

        Type data;
        /// <summary>
        /// Specific Byte/Json data objects
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("D")]
        public virtual Type Data { get => data; set => data = value; }

        System.Boolean hasData;
        /// <summary>
        /// Whether there is value
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("H")]
        public virtual System.Boolean HasData { get => hasData; set => hasData = value; }

        System.String callback;
        /// <summary>
        /// Gets the token of this result, used for callback
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        [System.Text.Json.Serialization.JsonPropertyName("B")]
        public virtual System.String Callback { get => callback; set => callback = value; }

        System.Type dataType;
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual System.Type DataType { get => dataType; set => dataType = value; }

        System.Type genericDefinition;
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual System.Type GenericDefinition => genericDefinition;

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
        public virtual string ToDataString() => Utils.Help.JsonSerialize(this.Data);

        /// <summary>
        /// ProtoBuf format
        /// </summary>
        /// <returns></returns>
        public virtual byte[] ToBytes() => throw new System.NotImplementedException(); //Utils.Help.ProtoBufSerialize(this);

        /// <summary>
        /// ProtoBuf format Data
        /// </summary>
        /// <returns></returns>
        public virtual byte[] ToDataBytes() => throw new System.NotImplementedException(); //Utils.Help.ProtoBufSerialize(this.Data);

        /// <summary>
        /// Get generic data
        /// </summary>
        /// <typeparam name="DataType">Generic type</typeparam>
        /// <returns></returns>
        public virtual DataType Get<DataType>() => ((IResult)this).Data;
    }

    /* MessagePack
    public class ResultObject<Type> : Business.Result.ResultObject<Type>
    {
        public ResultObject(Type data, System.Type dataType, int state = 1, string message = null, System.Type genericType = null)
            : base(data, dataType, state, message, genericType) { }

        public ResultObject(Type data, int state = 1, string message = null) : this(data, null, state, message) { }

        [MessagePack.IgnoreMember]
        public override System.Type DataType { get => base.DataType; set => base.DataType = value; }

        [MessagePack.IgnoreMember]
        public override System.Type GenericType => base.GenericType;
    }
    */
}