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

    public struct LoggerData
    {
        public LoggerType Type { get; set; }

        //object value;
        public LoggerValue Value { get; set; }

        //object result;
        public dynamic Result { get; set; }

        //double time;
        public double Time { get; set; }

        //string member;
        public string Member { get; set; }

        public string Group { get; set; }
    }

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
        public System.Collections.Generic.IDictionary<string, bool> HasIArg { get { return hasIArg; } }

        System.Collections.Generic.IDictionary<string, dynamic> dictionary;

        public LoggerValue ToValue(LoggerValueType valueType = LoggerValueType.In)
        {
            dictionary = this.ToDictionary(c => c.Key, c => valueType == LoggerValueType.Out && hasIArg[c.Key] ? c.Value.Out : c.Value.In);
            return this;
        }

        /// <summary>
        /// JSON format
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Extensions.Help.JsonSerialize(null == dictionary ? this : dictionary);
        }
    }
}