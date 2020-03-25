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
    using Utils;
    using Annotations;
    using Meta;
    using System.Threading.Tasks;

    /// <summary>
    /// Log subscription queue
    /// </summary>
    public class Logger
    {
        /// <summary>
        /// Logger value type
        /// </summary>
        public enum LoggerValueType
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
        public struct LoggerData
        {
            public System.DateTimeOffset Dtt { get; set; }

            /// <summary>
            /// token
            /// </summary>
            public dynamic Token { get; set; }

            /// <summary>
            /// Logger type
            /// </summary>
            public Type Type { get; set; }

            /// <summary>
            /// The parameters of the method
            /// </summary>
            public System.Collections.Generic.IDictionary<string, dynamic> Value { get; set; }

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
            public override string ToString() => new LoggerDataJson
            {
                Dtt = Dtt,
                Token = Token,
                Type = Type,
                Member = Member,
                Value = Value?.JsonSerialize(),
                Result = Result?.ToString(),
                Time = Time,
                Group = Group,
            }.JsonSerialize();

            struct LoggerDataJson
            {
                public System.DateTimeOffset Dtt { get; set; }

                /// <summary>
                /// token
                /// </summary>
                public dynamic Token { get; set; }

                /// <summary>
                /// Logger type
                /// </summary>
                public Type Type { get; set; }

                /// <summary>
                /// The parameters of the method
                /// </summary>
                public string Value { get; set; }

                /// <summary>
                /// The method's Return Value
                /// </summary>
                public string Result { get; set; }

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
        }

        public struct BatchOptions
        {
            /// <summary>
            /// Return log time interval, default System.TimeSpan.Zero equals not enabled, x seconds is reasonable
            /// </summary>
            public System.TimeSpan Interval { get; set; }

            /// <summary>
            /// Return log number, less than 1 no restrictions
            /// </summary>
            public int MaxNumber { get; set; }
        }

        public BatchOptions Batch { get; }

        public LoggerValueType ValueType { get; set; } = LoggerValueType.In;

        internal readonly System.Func<LoggerData, Task> call;

        public readonly Queue<LoggerData> loggerQueue;

        public Logger(System.Func<LoggerData, Task> call, LoggerValueType loggerValueType = LoggerValueType.In)
        {
            this.ValueType = loggerValueType;

            this.call = call;
        }

        /// <summary>
        /// Logger
        /// </summary>
        /// <param name="call"></param>
        /// <param name="batch"></param>
        /// <param name="maxWorkThreads">Gets the maximum out queue thread for this queue, default 1</param>
        /// <param name="syn">Whether each outgoing thread has synchronous callback, asynchronous by default</param>
        /// <param name="maxCapacity">Gets the max capacity of this queue</param>
        /// <param name="loggerValueType"></param>
        public Logger(System.Func<System.Collections.Generic.IEnumerable<LoggerData>, Task> call, BatchOptions batch = default, int maxWorkThreads = 1, bool syn = false, int? maxCapacity = null, LoggerValueType loggerValueType = LoggerValueType.In)
        {
            this.ValueType = loggerValueType;

            loggerQueue = new Queue<LoggerData>(call, new Queue<LoggerData>.BatchOptions { Interval = batch.Interval, MaxNumber = batch.MaxNumber }, maxWorkThreads, syn, maxCapacity);
        }

        /*
        public static LoggerData GetLoggerData(IBusiness business, System.Collections.Generic.IDictionary<string, dynamic> value, string group = null, [System.Runtime.CompilerServices.CallerMemberName] string method = null)
        {
            if (!business.Configer.MetaData.TryGetValue(method ?? string.Empty, out MetaData meta))
            {
                return default;
            }

            if (string.IsNullOrWhiteSpace(group))
            {
                group = business.Configer.Info.CommandGroupDefault;
            }

            if (!meta.CommandGroup.Full.TryGetValue(group, out ReadOnlyDictionary<string, CommandAttribute> commands) || 0 == commands?.Count)
            {
                return default;
            }

            var command = commands.First().Value;

            //var argsObjLog = new System.Collections.Generic.List<ArgsLog>(meta.Args.Count);

            //foreach (var c in meta.Args)
            //{
            //    if (!c.Group.TryGetValue(command.Key, out ArgGroup argGroup))
            //    {
            //        continue;
            //    }

            //    argsObjLog.Add(new ArgsLog { name = c.Name, value = value[c.Position], logger = argGroup.Logger, iArgInLogger = c.Group[command.Key].IArgInLogger, hasIArg = c.HasIArg });
            //}

            //if (!meta.MetaLogger.TryGetValue(command.Key, out MetaLogger metaLogger))
            //{
            //    return default;
            //}

            //var logType = Type.Record;

            //var logObjs = LoggerSet(logType, metaLogger, argsObjLog, out _, out _, business.Configer.Logger?.ValueType);

            return new LoggerData { Type = Type.Record, Value = value, Member = meta.FullName, Group = command.Group };
        }
        */

        internal struct ArgsLog
        {
            public string name;
            public dynamic value;
            public MetaLogger logger;
            public MetaLogger iArgInLogger;
            public bool hasIArg;
        }

        internal static System.Collections.Generic.IDictionary<string, dynamic> LoggerSet(Type logType, MetaLogger logger, System.Collections.Generic.IList<ArgsLog> argsObjLog, out bool canWrite, out bool canResult, LoggerValueType? loggerValueType)
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

                        if (0 < argsObjLog.Count)
                        {
                            var logObjs = new System.Collections.Generic.Dictionary<string, dynamic>(argsObjLog.Count);
                            foreach (var log in argsObjLog)
                            {
                                LoggerSet(logger.Record.CanValue, log.logger.Record, log.iArgInLogger.Record, logObjs, log, loggerValueType);
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

                        if (0 < argsObjLog.Count)
                        {
                            var logObjs = new System.Collections.Generic.Dictionary<string, dynamic>(argsObjLog.Count);
                            foreach (var log in argsObjLog)
                            {
                                LoggerSet(logger.Error.CanValue, log.logger.Error, log.iArgInLogger.Error, logObjs, log, loggerValueType);
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

                        if (0 < argsObjLog.Count)
                        {
                            var logObjs = new System.Collections.Generic.Dictionary<string, dynamic>(argsObjLog.Count);
                            foreach (var log in argsObjLog)
                            {
                                LoggerSet(logger.Exception.CanValue, log.logger.Exception, log.iArgInLogger.Exception, logObjs, log, loggerValueType);
                            }
                            return 0 == logObjs.Count ? null : logObjs;
                        }
                    }
                    return null;
                default: return null;
            }

            //return 0 == logObjs.Count ? null : logObjs;
        }

        static void LoggerSet(LoggerValueMode canValue, LoggerAttribute argLogAttr, LoggerAttribute iArgInLogAttr, System.Collections.Generic.IDictionary<string, dynamic> logObjs, ArgsLog log, LoggerValueType? loggerValueType)
        {
            //meta
            switch (canValue)
            {
                case LoggerValueMode.All:
                    if ((null != argLogAttr && !argLogAttr.CanWrite))
                    {
                        break;
                    }

                    if (null != iArgInLogAttr && !iArgInLogAttr.CanWrite && log.hasIArg)
                    {
                        var iArg = log.value as IArg;
                        logObjs.Add(log.name, iArg.Out);
                        //logObjs.HasIArg[log.name] = false;
                    }
                    else if (log.hasIArg)
                    {
                        var iArg = log.value as IArg;
                        switch (loggerValueType ?? LoggerValueType.In)
                        {
                            case LoggerValueType.All:
                                logObjs.Add(log.name, iArg ?? log.value);
                                break;
                            case LoggerValueType.In:
                                logObjs.Add(log.name, null == iArg ? log.value : iArg.In);
                                //logObjs.HasIArg[log.name] = false;
                                break;
                            case LoggerValueType.Out:
                                logObjs.Add(log.name, null == iArg ? log.value : iArg.Out);
                                //logObjs.HasIArg[log.name] = false;
                                break;
                            default: break;
                        }
                    }
                    else
                    {
                        logObjs.Add(log.name, log.value);
                    }
                    break;
                case LoggerValueMode.Select:
                    if (null != argLogAttr && argLogAttr.CanWrite)
                    {
                        if (null != iArgInLogAttr && !iArgInLogAttr.CanWrite && log.hasIArg)
                        {
                            logObjs.Add(log.name, (log.value as IArg).Out);
                            //logObjs.HasIArg[log.name] = false;
                        }
                        else if (log.hasIArg)
                        {
                            var iArg = log.value as IArg;
                            switch (loggerValueType ?? LoggerValueType.In)
                            {
                                case LoggerValueType.All:
                                    logObjs.Add(log.name, iArg ?? log.value);
                                    break;
                                case LoggerValueType.In:
                                    logObjs.Add(log.name, null == iArg ? log.value : iArg.In);
                                    //logObjs.HasIArg[log.name] = false;
                                    break;
                                case LoggerValueType.Out:
                                    logObjs.Add(log.name, null == iArg ? log.value : iArg.Out);
                                    //logObjs.HasIArg[log.name] = false;
                                    break;
                                default: break;
                            }
                        }
                        else
                        {
                            logObjs.Add(log.name, log.value);
                        }
                    }
                    break;
                default: break;
            }
        }
    }

    /*
    /// <summary>
    /// The parameters of the method
    /// </summary>
    public class LoggerValue : System.Collections.Generic.Dictionary<string, dynamic>
    {
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
        public LoggerValue ToValue(Logger.LoggerValueType valueType = Logger.LoggerValueType.In)
        {
            var dictionary = new LoggerValue(0 < this.Count ? HasIArg.ToDictionary(c => c.Key, c => false) : HasIArg, this.Count);

            foreach (var item in this)
            {
                dictionary.Add(item.Key, HasIArg[item.Key] ? (valueType == Logger.LoggerValueType.Out ? (item.Value as IArg).Out : (item.Value as IArg).In) : item.Value);
            }

            return dictionary;
        }

        /// <summary>
        /// JSON format, if the total number to 0, then returned null
        /// </summary>
        /// <returns></returns>
        public override string ToString() => this.JsonSerialize();
    }
    */
}