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

        //object valueObj;
        public dynamic In { get; set; }

        //readonly string group;
        public string Group { get; set; }
    }
}
