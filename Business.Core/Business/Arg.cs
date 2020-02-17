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

namespace Business.Core
{
    public interface IArg
    {
        /// <summary>
        /// The first input object
        /// </summary>
        dynamic In { get; set; }

        /// <summary>
        /// The final output object
        /// </summary>
        dynamic Out { get; set; }
    }

    public interface IArg<OutType, InType> : IArg
    {
        new InType In { get; set; }

        new OutType Out { get; set; }
    }

    public interface IArg<OutType> : IArg
    {
        new OutType Out { get; set; }
    }

    /// <summary>
    /// This is a parameter package, used to transform parameters
    /// </summary>
    /// <typeparam name="OutType"></typeparam>
    /// <typeparam name="InType"></typeparam>
    public struct Arg<OutType, InType> : IArg<OutType, InType>
    {
        public static implicit operator Arg<OutType, InType>(InType value) => new Arg<OutType, InType> { In = value };
        //default OutType
        //public static implicit operator Arg<OutType, InType>(OutType value) => new Arg<OutType, InType>() { In = (InType)Utils.Help.ChangeType(value, typeof(InType)) };
        public static implicit operator Arg<OutType, InType>(OutType value) => new Arg<OutType, InType>() { In = (dynamic)value };

        dynamic IArg.Out { get => this.Out; set => this.Out = value; }

        /// <summary>
        /// The final output object
        /// </summary>
        public OutType Out { get; set; }

        dynamic IArg.In { get => this.In; set => this.In = value; }

        /// <summary>
        /// The first input object
        /// </summary>
        public InType In { get; set; }
    }

    /// <summary>
    /// This is a parameter package, used to transform parameters
    /// </summary>
    /// <typeparam name="OutType"></typeparam>
    public struct Arg<OutType> : IArg<OutType>
    {
        public static implicit operator Arg<OutType>(string value) => new Arg<OutType>() { In = value };
        public static implicit operator Arg<OutType>(byte[] value) => new Arg<OutType>() { In = value };
        //default OutType
        public static implicit operator Arg<OutType>(OutType value) => new Arg<OutType>() { In = value };

        dynamic IArg.Out { get => this.Out; set => this.Out = value; }

        /// <summary>
        /// The final output object
        /// </summary>
        public OutType Out { get; set; }

        /// <summary>
        /// The first input object
        /// </summary>
        public dynamic In { get; set; }
    }
}
