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
    using Annotations;
    using Meta;
    using System.Threading.Tasks;
    using Utils;

    /// <summary>
    /// Log subscription queue
    /// </summary>
    public class Logger
    {
        /// <summary>
        /// Logger value type
        /// </summary>
        public enum ValueType
        {
            /// <summary>
            /// In
            /// </summary>
            All = 0,
            /// <summary>
            /// In
            /// </summary>
            In = 1,
            /// <summary>
            /// Out
            /// </summary>
            Out = 2
        }

        /// <summary>
        /// Needs of the logging categories
        /// </summary>
        public enum Type
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
        public readonly struct LoggerData
        {
            /// <summary>
            /// Logger data object
            /// </summary>
            /// <param name="dtt"></param>
            /// <param name="token"></param>
            /// <param name="type"></param>
            /// <param name="value"></param>
            /// <param name="result"></param>
            /// <param name="time"></param>
            /// <param name="member"></param>
            /// <param name="group"></param>
            public LoggerData(System.DateTimeOffset dtt, Auth.IToken token, Type type, dynamic value, dynamic result, double time, string member, string group)
            {
                Dtt = dtt;
                Token = token;
                Type = type;
                Value = value;
                Result = result;
                Time = time;
                Member = member;
                Group = group;
            }

            /// <summary>
            /// Dtt
            /// </summary>
            public System.DateTimeOffset Dtt { get; }

            /// <summary>
            /// token
            /// </summary>
            public Auth.IToken Token { get; }

            /// <summary>
            /// Logger type
            /// </summary>
            public Type Type { get; }

            ///// <summary>
            ///// The parameters of the method
            ///// </summary>
            //public System.Collections.Generic.IDictionary<string, dynamic> Value { get; }

            /// <summary>
            /// The parameters of the method
            /// </summary>
            public dynamic Value { get; }

            /// <summary>
            /// The method's Return Value
            /// </summary>
            public dynamic Result { get; }

            /// <summary>
            /// Method execution time
            /// </summary>
            public double Time { get; }

            /// <summary>
            /// Method full name
            /// </summary>
            public string Member { get; }

            /// <summary>
            /// Used for the command group
            /// </summary>
            public string Group { get; }

            /// <summary>
            /// Json format
            /// </summary>
            /// <returns></returns>
            public override string ToString() => new LoggerDataJson(Dtt, Token, Type, null != Value ? Help.JsonSerialize(Value) : null, Result?.ToString(), Time, Member, Group).JsonSerialize();

            readonly struct LoggerDataJson
            {
                public LoggerDataJson(System.DateTimeOffset dtt, Auth.IToken token, Type type, string value, string result, double time, string member, string group)
                {
                    Dtt = dtt;
                    Token = token;
                    Type = type;
                    Value = value;
                    Result = result;
                    Time = time;
                    Member = member;
                    Group = group;
                }

                public System.DateTimeOffset Dtt { get; }

                /// <summary>
                /// token
                /// </summary>
                public Auth.IToken Token { get; }

                /// <summary>
                /// Logger type
                /// </summary>
                public Type Type { get; }

                /// <summary>
                /// The parameters of the method
                /// </summary>
                public string Value { get; }

                /// <summary>
                /// The method's Return Value
                /// </summary>
                public string Result { get; }

                /// <summary>
                /// Method execution time
                /// </summary>
                public double Time { get; }

                /// <summary>
                /// Method full name
                /// </summary>
                public string Member { get; }

                /// <summary>
                /// Used for the command group
                /// </summary>
                public string Group { get; }
            }
        }

        /// <summary>
        /// BatchOptions
        /// </summary>
        public struct BatchOptions
        {
            /// <summary>
            /// BatchOptions
            /// </summary>
            /// <param name="interval">Return log time interval, default System.TimeSpan.Zero equals not enabled, x seconds is reasonable</param>
            /// <param name="maxNumber">Return log number, less than 1 no restrictions</param>
            public BatchOptions(System.TimeSpan interval, int maxNumber)
            {
                Interval = interval;
                MaxNumber = maxNumber;
            }

            /// <summary>
            /// Return log time interval, default System.TimeSpan.Zero equals not enabled, x seconds is reasonable
            /// </summary>
            public System.TimeSpan Interval { get; set; }

            /// <summary>
            /// Return log number, less than 1 no restrictions
            /// </summary>
            public int MaxNumber { get; set; }
        }

        internal readonly System.Func<LoggerData, ValueTask> call;

        /// <summary>
        /// loggerQueue
        /// </summary>
        public readonly Queue<LoggerData> loggerQueue;

        /// <summary>
        /// Logger
        /// </summary>
        /// <param name="call"></param>
        public Logger(System.Func<LoggerData, ValueTask> call) => this.call = call;

        /// <summary>
        /// Logger
        /// </summary>
        /// <param name="call"></param>
        /// <param name="batch"></param>
        /// <param name="syn"></param>
        /// <param name="maxCapacity">Gets the max capacity of this queue</param>
        public Logger(System.Func<System.Collections.Generic.IEnumerable<LoggerData>, ValueTask> call, BatchOptions batch = default, bool syn = false, int? maxCapacity = null) => loggerQueue = new Queue<LoggerData>(call, new Queue<LoggerData>.BatchOptions(batch.Interval, batch.MaxNumber), syn, maxCapacity: maxCapacity);

        internal readonly struct ArgsLog
        {
            public readonly string name;
            public readonly dynamic value;
            public readonly MetaLogger logger;

            public ArgsLog(string name, dynamic value, MetaLogger logger)
            {
                this.name = name;
                this.value = value;
                this.logger = logger;
            }
        }

        internal static System.Collections.Generic.IDictionary<string, dynamic> LoggerSet(Type logType, MetaLogger logger, out bool canWrite, out bool canResult, System.Collections.Generic.IList<ArgsLog> argsObjLog = null)
        {
            canWrite = canResult = false;

            switch (logType)
            {
                case Type.Record:
                    if (logger.Record.CanWrite)
                    {
                        canWrite = true;

                        if (logger.Record.CanResult)
                        {
                            canResult = true;
                        }

                        if (0 < argsObjLog?.Count)
                        {
                            var logObjs = new System.Collections.Generic.Dictionary<string, dynamic>(argsObjLog.Count);
                            foreach (var log in argsObjLog)
                            {
                                LoggerSet(logger.Record.CanValue, log.logger.Record, logObjs, log);
                            }
                            return 0 == logObjs.Count ? null : logObjs;
                        }
                    }
                    return null;
                case Type.Error:
                    if (logger.Error.CanWrite)
                    {
                        canWrite = true;

                        if (logger.Error.CanResult)
                        {
                            canResult = true;
                        }

                        if (0 < argsObjLog?.Count)
                        {
                            var logObjs = new System.Collections.Generic.Dictionary<string, dynamic>(argsObjLog.Count);
                            foreach (var log in argsObjLog)
                            {
                                LoggerSet(logger.Error.CanValue, log.logger.Error, logObjs, log);
                            }
                            return 0 == logObjs.Count ? null : logObjs;
                        }
                    }
                    return null;
                case Type.Exception:
                    if (logger.Exception.CanWrite)
                    {
                        canWrite = true;

                        if (logger.Exception.CanResult)
                        {
                            canResult = true;
                        }

                        if (0 < argsObjLog?.Count)
                        {
                            var logObjs = new System.Collections.Generic.Dictionary<string, dynamic>(argsObjLog.Count);
                            foreach (var log in argsObjLog)
                            {
                                LoggerSet(logger.Exception.CanValue, log.logger.Exception, logObjs, log);
                            }
                            return 0 == logObjs.Count ? null : logObjs;
                        }
                    }
                    return null;
                default: return null;
            }

            //return 0 == logObjs.Count ? null : logObjs;

            void LoggerSet(LoggerValueMode canValue, LoggerAttribute argLogAttr, System.Collections.Generic.IDictionary<string, dynamic> logObjs, ArgsLog log)
            {
                switch (canValue)
                {
                    case LoggerValueMode.All:
                        if ((null != argLogAttr && !argLogAttr.CanWrite))
                        {
                            break;
                        }
                        logObjs.Add(log.name, log.value);
                        break;
                    case LoggerValueMode.Select:
                        if (null != argLogAttr && argLogAttr.CanWrite)
                        {
                            logObjs.Add(log.name, log.value);
                        }
                        break;
                    default: break;
                }
            }
        }

        /*
        static void LoggerSet(LoggerValueMode canValue, LoggerAttribute argLogAttr, System.Collections.Generic.IDictionary<string, dynamic> logObjs, ArgsLog log)
        {
            //meta
            switch (canValue)
            {
                case LoggerValueMode.All:
                    if ((null != argLogAttr && !argLogAttr.CanWrite))
                    {
                        break;
                    }

                    logObjs.Add(log.name, log.value);

                    //if (null != iArgInLogAttr && !iArgInLogAttr.CanWrite)
                    //{
                    //    var iArg = log.value as IArg;
                    //    logObjs.Add(log.name, iArg.Out);
                    //    //logObjs.HasIArg[log.name] = false;
                    //}
                    //else if (log.hasIArg)
                    //{
                    //    var iArg = log.value as IArg;
                    //    switch (loggerValueType ?? ValueType.In)
                    //    {
                    //        case ValueType.All:
                    //            logObjs.Add(log.name, iArg ?? log.value);
                    //            break;
                    //        case ValueType.In:
                    //            logObjs.Add(log.name, null == iArg ? log.value : iArg.In);
                    //            //logObjs.HasIArg[log.name] = false;
                    //            break;
                    //        case ValueType.Out:
                    //            logObjs.Add(log.name, null == iArg ? log.value : iArg.Out);
                    //            //logObjs.HasIArg[log.name] = false;
                    //            break;
                    //        default: break;
                    //    }
                    //}
                    //else
                    //{
                    //    logObjs.Add(log.name, log.value);
                    //}
                    break;
                case LoggerValueMode.Select:
                    if (null != argLogAttr && argLogAttr.CanWrite)
                    {
                        logObjs.Add(log.name, log.value);

                        //if (null != iArgInLogAttr && !iArgInLogAttr.CanWrite && log.hasIArg)
                        //{
                        //    logObjs.Add(log.name, (log.value as IArg).Out);
                        //    //logObjs.HasIArg[log.name] = false;
                        //}
                        //else if (log.hasIArg)
                        //{
                        //    var iArg = log.value as IArg;
                        //    switch (loggerValueType ?? ValueType.In)
                        //    {
                        //        case ValueType.All:
                        //            logObjs.Add(log.name, iArg ?? log.value);
                        //            break;
                        //        case ValueType.In:
                        //            logObjs.Add(log.name, null == iArg ? log.value : iArg.In);
                        //            //logObjs.HasIArg[log.name] = false;
                        //            break;
                        //        case ValueType.Out:
                        //            logObjs.Add(log.name, null == iArg ? log.value : iArg.Out);
                        //            //logObjs.HasIArg[log.name] = false;
                        //            break;
                        //        default: break;
                        //    }
                        //}
                        //else
                        //{
                        //    logObjs.Add(log.name, log.value);
                        //}
                    }
                    break;
                default: break;
            }
        }
        */
    }
}