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
    /// <summary>
    /// IArg
    /// </summary>
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

    /// <summary>
    /// IArg
    /// </summary>
    /// <typeparam name="OutType"></typeparam>
    /// <typeparam name="InType"></typeparam>
    public interface IArg<OutType, InType> : IArg
    {
        /// <summary>
        /// In
        /// </summary>
        new InType In { get; set; }

        /// <summary>
        /// Out
        /// </summary>
        new OutType Out { get; set; }
    }

    /// <summary>
    /// IArg
    /// </summary>
    /// <typeparam name="OutType"></typeparam>
    public interface IArg<OutType> : IArg
    {
        /// <summary>
        /// Out
        /// </summary>
        new OutType Out { get; set; }
    }

    /// <summary>
    /// This is a parameter package, used to transform parameters
    /// </summary>
    /// <typeparam name="OutType"></typeparam>
    /// <typeparam name="InType"></typeparam>
    public struct Arg<OutType, InType> : IArg<OutType, InType>
    {
        /// <summary>
        /// Arg
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Arg<OutType, InType>(InType value) => new Arg<OutType, InType> { In = value };
        //default OutType
        //public static implicit operator Arg<OutType, InType>(OutType value) => new Arg<OutType, InType>() { In = (InType)Utils.Help.ChangeType(value, typeof(InType)) };

        /// <summary>
        /// Arg
        /// </summary>
        /// <param name="value"></param>
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
        /// <summary>
        /// Arg
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Arg<OutType>(string value) => new Arg<OutType>() { In = value };

        /// <summary>
        /// Arg
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Arg<OutType>(byte[] value) => new Arg<OutType>() { In = value };

        //default OutType
        /// <summary>
        /// Arg
        /// </summary>
        /// <param name="value"></param>
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
