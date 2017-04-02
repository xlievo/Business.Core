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
        public System.Collections.Generic.IDictionary<string, dynamic> Value { get; set; }

        //object result;
        public dynamic Result { get; set; }

        //double time;
        public double Time { get; set; }

        //string member;
        public string Member { get; set; }

        public string Group { get; set; }
    }
}