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
    public interface IResult
    {
        /// <summary>
        /// The results of the state is greater than or equal to 1: success, equal to 0: not to capture the system level exceptions, less than 0: business class error.
        /// </summary>
        System.Int32 State { get; set; }

        /// <summary>
        /// Success can be null
        /// </summary>
        System.String Message { get; set; }

        /// <summary>
        /// Specific Byte/Json data objects
        /// </summary>
        dynamic Data { get; set; }

        /// <summary>
        /// Whether there is value
        /// </summary>
        System.Boolean HasData { get; set; }

        System.Type DataType { get; set; }

        /// <summary>
        /// Gets the token of this result, used for callback
        /// </summary>
        System.String Callback { get; set; }

        ///// <summary>
        ///// Whether to notify all
        ///// </summary>
        //System.Boolean Overall { get; set; }

        ///// <summary>
        ///// Socket identity
        ///// </summary>
        //System.String CommandID { get; set; }

        ///// <summary>
        ///// Notify list
        ///// </summary>
        //System.Collections.Generic.IEnumerable<IResult> Notifys { get; set; }

        ICommand Command { get; set; }

        /// <summary>
        /// Json Data
        /// </summary>
        /// <returns></returns>
        System.String ToDataString();

        /// <summary>
        /// ProtoBuf Data
        /// </summary>
        /// <returns></returns>
        System.Byte[] ToDataBytes();

        /// <summary>
        /// ProtoBuf
        /// </summary>
        /// <returns></returns>
        System.Byte[] ToBytes();

        /// <summary>
        /// Json
        /// </summary>
        /// <returns></returns>
        System.String ToString();
    }

    public interface IResult<DataType> : IResult
    {
        /// <summary>
        /// Specific Byte/Json data objects
        /// </summary>
        new DataType Data { get; set; }
    }

    public interface ICommand
    {
        /// <summary>
        /// Whether to notify all
        /// </summary>
        System.Boolean Overall { get; set; }

        /// <summary>
        /// Socket identity
        /// </summary>
        System.String CommandID { get; set; }

        /// <summary>
        /// Notify list
        /// </summary>
        System.Collections.Generic.IEnumerable<IResult> Notifys { get; set; }
    }
}