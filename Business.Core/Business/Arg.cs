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
        dynamic Out { get; set; }

        dynamic In { get; set; }

        string Group { get; set; }
    }

    public interface IArg<OutType> : IArg
    {
        new OutType Out { get; set; }
    }

    /// <summary>
    /// This is a parameter package, used to transform parameters
    /// </summary>
    /// <typeparam name="OutType"></typeparam>
    public class Arg<OutType> : IArg<OutType>
    {
        public static implicit operator Arg<OutType>(string value) => new Arg<OutType>() { In = value };
        public static implicit operator Arg<OutType>(byte[] value) => new Arg<OutType>() { In = value };

        OutType value;
        dynamic IArg.Out { get => this.value; set => this.value = value; }

        /// <summary>
        /// The final output object
        /// </summary>
        public OutType Out { get => this.value; set => this.value = value; }

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
        public byte[] ToBytes() => Utils.Help.ProtoBufSerialize(value);

        /// <summary>
        /// JSON format Out
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Utils.Help.JsonSerialize(value);
    }
}
