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
    public enum LoggerType
    {
        /// <summary>
        /// Error = -1
        /// </summary>
        Error = -1,

        /// <summary>
        /// Exception = 0
        /// </summary>
        Exception = 0,

        /// <summary>
        /// Record = 1
        /// </summary>
        Record = 1
    }

    /// <summary>
    /// Logger data object
    /// </summary>
    public struct LoggerData
    {
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
    }

    /// <summary>
    /// The parameters of the method
    /// </summary>
    public class LoggerValue : System.Collections.Generic.Dictionary<string, dynamic>
    {
        public enum LoggerValueType
        {
            In = 0,
            Out = 1
        }

        protected internal LoggerValue(System.Collections.Generic.IDictionary<string, bool> hasIArg, int capacity) : base(capacity)
        {
            this.hasIArg = hasIArg;
        }

        readonly System.Collections.Generic.IDictionary<string, bool> hasIArg;

        /// <summary>
        /// A combination of parameter names and HasIArg
        /// </summary>
        public System.Collections.Generic.IDictionary<string, bool> HasIArg { get { return hasIArg; } }

        System.Collections.Generic.IDictionary<string, dynamic> dictionary;

        /// <summary>
        /// Filtering input or output objects
        /// </summary>
        /// <param name="valueType"></param>
        /// <returns></returns>
        public LoggerValue ToValue(LoggerValueType valueType = LoggerValueType.In)
        {
            dictionary = this.ToDictionary(c => c.Key, c => valueType == LoggerValueType.Out && hasIArg[c.Key] ? c.Value.Out : c.Value.In);
            return this;
        }

        /// <summary>
        /// JSON format, if the total number to 0, then returned null
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var current = dictionary ?? this;
            return 0 == current.Count ? null : Extensions.Help.JsonSerialize(current);
        }
    }
}