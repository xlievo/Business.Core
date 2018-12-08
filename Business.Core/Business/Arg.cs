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

namespace Business
{
    public interface IArg
    {
        dynamic In { get; set; }

        dynamic Out { get; set; }

        string Group { get; set; }

        string ToString();

        byte[] ToBytes();

        dynamic ToOut(dynamic value);
    }

    public interface IArg<OutType, InType> : IArg
    {
        new InType In { get; set; }

        new OutType Out { get; set; }
    }

    /*
    /// <summary>
    /// This is a parameter package, used to transform parameters
    /// </summary>
    /// <typeparam name="OutType"></typeparam>
    public class Arg<OutType> : IArg<OutType>
    {
        public static implicit operator Arg<OutType>(string value) => new Arg<OutType>() { In = value };
        public static implicit operator Arg<OutType>(byte[] value) => new Arg<OutType>() { In = value };

        OutType outValue;
        dynamic IArg.Out { get => this.outValue; set => this.outValue = value; }

        /// <summary>
        /// The final output object
        /// </summary>
        public OutType Out { get => this.outValue; set => this.outValue = value; }

        /// <summary>
        /// The first input object
        /// </summary>
        public dynamic In { get; set; }

        /// <summary>
        /// Used for the command group
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public string Group { get; set; }

        /// <summary>
        /// ProtoBuf format Out
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes() => Utils.Help.ProtoBufSerialize(outValue);

        /// <summary>
        /// JSON format Out
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Utils.Help.JsonSerialize(outValue);
    }
    */
    /// <summary>
    /// This is a parameter package, used to transform parameters
    /// </summary>
    /// <typeparam name="OutType"></typeparam>
    /// <typeparam name="InType"></typeparam>
    public class Arg<OutType, InType> : IArg<OutType, InType>
    {
        public static implicit operator Arg<OutType, InType>(InType value) => new Arg<OutType, InType> { In = value };
        //default OutType
        //public static implicit operator Arg<OutType, InType>(OutType value) => new Arg<OutType, InType>() { In = (InType)Utils.Help.ChangeType(value, typeof(InType)) };
        //public static implicit operator Arg<OutType, InType>(OutType value) => new Arg<OutType, InType>() { In = (dynamic)value };

        dynamic IArg.Out { get => this.Out; set => this.Out = value; }

        /// <summary>
        /// The final output object
        /// </summary>
        public virtual OutType Out { get; set; }

        dynamic IArg.In { get => this.In; set => this.In = value; }

        /// <summary>
        /// The first input object
        /// </summary>
        public virtual InType In { get; set; }

        /// <summary>
        /// Used for the command group
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public virtual string Group { get; set; }

        /// <summary>
        /// ProtoBuf format Out
        /// </summary>
        /// <returns></returns>
        public virtual byte[] ToBytes() => throw new System.NotImplementedException();// Utils.Help.ProtoBufSerialize(outValue);

        /// <summary>
        /// JSON format Out
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Utils.Help.JsonSerialize(Out);

        public virtual dynamic ToOut(dynamic value) => throw new System.NotImplementedException();
    }

    /// <summary>
    /// This is a parameter package, used to transform parameters
    /// </summary>
    /// <typeparam name="OutType"></typeparam>
    public class Arg<OutType> : Arg<OutType, dynamic>
    {
        public static implicit operator Arg<OutType>(string value) => new Arg<OutType>() { In = value };
        public static implicit operator Arg<OutType>(byte[] value) => new Arg<OutType>() { In = value };
        //default OutType
        public static implicit operator Arg<OutType>(OutType value) => new Arg<OutType>() { In = value };
    }
}
