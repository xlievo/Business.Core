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
    public enum LogType
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

    //public interface IBusinessLog
    //{
    //    BusinessLogType Type { get; set; }

    //    object[] Value { get; set; }

    //    dynamic Result { get; set; }

    //    double Time { get; set; }

    //    string Member { get; set; }

    //    //string Description { get; set; }
    //}

    public struct Log //System.IEquatable<BusinessLog>
    {
        //public bool Equals(BusinessLogData other) { return other.Guid.Equals(this.Guid); }
        //[System.Runtime.CompilerServices.CallerMemberName] 
        //public Log(BusinessLogType type, object value = null, object result = null, double time = 0, string member = null, string description = null)
        //{
        //    this.type = type;
        //    this.value = value;
        //    this.result = result;
        //    this.time = time;
        //    this.member = member;
        //    this.description = description;
        //}

        //LogType type;
        public LogType Type { get; set; }

        //object value;
        public object[] Value { get; set; }

        //object result;
        public dynamic Result { get; set; }

        //double time;
        public double Time { get; set; }

        //string member;
        public string Member { get; set; }

        public string Group { get; set; }

        public Auth.Token Token { get; set; }
        //string description;
        //public string Description { get; set; }
    }
}