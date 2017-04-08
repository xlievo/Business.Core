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

    public class Arg<OutType> : IArg<OutType>
    {
        public static implicit operator Arg<OutType>(string value)
        {
            return new Arg<OutType>() { In = value };
        }
        public static implicit operator Arg<OutType>(byte[] value)
        {
            return new Arg<OutType>() { In = value };
        }

        OutType value;
        dynamic IArg.Out { get { return value; } set { this.value = value; } }

        public OutType Out { get { return value; } set { this.value = value; } }

        public dynamic In { get; set; }

        public string Group { get; set; }

        /// <summary>
        /// ProtoBuf format Out
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            return Business.Extensions.Help.ProtoBufSerialize(value);
        }

        /// <summary>
        /// JSON format Out
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Business.Extensions.Help.JsonSerialize(value);
        }
    }
}
