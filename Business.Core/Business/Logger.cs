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
    using System.Linq;

    /// <summary>
    /// Needs of the logging categories
    /// </summary>
    //[System.Flags]
    public enum LoggerType
    {
        /// <summary>
        /// All
        /// </summary>
        All = 2,

        /// <summary>
        /// Record
        /// </summary>
        Record = 1,

        /// <summary>
        /// Error
        /// </summary>
        Error = -1,

        /// <summary>
        /// Exception
        /// </summary>
        Exception = 0,
    }

    /// <summary>
    /// Logger data object
    /// </summary>
    public struct LoggerData
    {
        /// <summary>
        /// Logger type
        /// </summary>
        public LoggerType Type { get; set; }

        /// <summary>
        /// The parameters of the method
        /// </summary>
        public LoggerValue Value { get; set; }

        /// <summary>
        /// The method's Return Value
        /// </summary>
        public dynamic Result { get; set; }

        /// <summary>
        /// Method execution time
        /// </summary>
        public double Time { get; set; }

        /// <summary>
        /// Method full name
        /// </summary>
        public string Member { get; set; }

        /// <summary>
        /// Used for the command group
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// Json format
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Utils.Help.JsonSerialize(this);
    }

    /// <summary>
    /// The parameters of the method
    /// </summary>
    public class LoggerValue : System.Collections.Generic.Dictionary<string, dynamic>
    {
        /// <summary>
        /// Logger value type
        /// </summary>
        public enum LoggerValueType
        {
            /// <summary>
            /// In
            /// </summary>
            In = 0,
            /// <summary>
            /// Out
            /// </summary>
            Out = 1
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hasIArg"></param>
        /// <param name="capacity"></param>
        protected internal LoggerValue(System.Collections.Generic.IDictionary<string, bool> hasIArg, int capacity) : base(capacity) => this.HasIArg = hasIArg;

        /// <summary>
        /// A combination of parameter names and HasIArg
        /// </summary>
        public System.Collections.Generic.IDictionary<string, bool> HasIArg { get; private set; }

        //LoggerValue dictionary;

        /// <summary>
        /// Filtering input or output objects
        /// </summary>
        /// <param name="valueType"></param>
        /// <returns></returns>
        public LoggerValue ToValue(LoggerValueType valueType = LoggerValueType.In)
        {
            //var dictionary = this.ToDictionary(c => c.Key, c => hasIArg[c.Key] ? (valueType == LoggerValueType.Out ? c.Value.Out : c.Value.In) : c.Value);
            var dictionary = new LoggerValue(0 < this.Count ? HasIArg.ToDictionary(c => c.Key, c => false) : HasIArg, this.Count);

            foreach (var item in this)
            {
                dictionary.Add(item.Key, HasIArg[item.Key] ? (valueType == LoggerValueType.Out ? (item.Value as IArg).Out : (item.Value as IArg).In) : item.Value);
            }

            return dictionary;
        }

        /// <summary>
        /// JSON format, if the total number to 0, then returned null
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Utils.Help.JsonSerialize(this);
            //var current = dictionary ?? this;
            //return 0 == current.Count ? null : Utils.Help.JsonSerialize(current);
        }
    }
}