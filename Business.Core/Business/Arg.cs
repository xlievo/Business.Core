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

        Attributes.LogMode Log { get; set; }
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

        //public Arg(object valueObj, string group = null)
        //{
        //    this.valueObj = valueObj;
        //    this.value = default(Type);
        //    this.group = group;
        //}

        OutType value;
        dynamic IArg.Out { get { return value; } set { this.value = value; } }

        public OutType Out { get { return value; } set { this.value = value; } }

        //object valueObj;
        public dynamic In { get; set; }

        //readonly string group;
        public string Group { get; set; }

        public Attributes.LogMode Log { get; set; }
    }

    //public struct Arg<OutType, InType> : IArg<OutType, InType>
    //{
    //    public static implicit operator Arg<OutType, InType>(InType value)
    //    {
    //        return new Arg<OutType, InType>() { In = value };
    //    }

    //    OutType value;
    //    dynamic IArg.Out { get { return value; } set { this.value = value; } }

    //    public OutType Out { get { return value; } }

    //    public InType In { get; set; }

    //    object IArg.In { get { return this.In; } set { this.In = (InType)value; } }

    //    public string Group { get; set; }
    //}
}
