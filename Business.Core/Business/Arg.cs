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
        //public static implicit operator Arg<OutType, InType>(InType value) => new Arg<OutType, InType>() { In = value };

        //public static implicit operator Arg<OutType, InType>(OutType value) => new Arg<OutType, InType>() { Out = value };

        OutType outValue;
        dynamic IArg.Out { get => this.outValue; set => this.outValue = value; }

        /// <summary>
        /// The final output object
        /// </summary>
        public OutType Out { get => this.outValue; set => this.outValue = value; }

        InType inValue;
        dynamic IArg.In { get => this.inValue; set => this.inValue = value; }

        /// <summary>
        /// The first input object
        /// </summary>
        public InType In { get => this.inValue; set => this.inValue = value; }

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

    /// <summary>
    /// This is a parameter package, used to transform parameters
    /// </summary>
    /// <typeparam name="OutType"></typeparam>
    public class Arg<OutType> : Arg<OutType, dynamic> { }
}
