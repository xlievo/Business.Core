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
    using Utils;

    /// <summary>
    /// Log subscription queue
    /// </summary>
    public class Logger
    {
        public struct BatchOptions
        {
            /// <summary>
            /// Return log time interval, default System.TimeSpan.Zero equals not enabled,5 seconds is reasonable
            /// </summary>
            public System.TimeSpan Interval { get; set; }

            /// <summary>
            /// Return log number, less than 1 no restrictions
            /// </summary>
            public int MaxNumber { get; set; }
        }

        public BatchOptions Batch { get; set; } = new BatchOptions();

        /// <summary>
        /// Logger
        /// </summary>
        //public System.Action<System.Collections.Generic.IEnumerable<LoggerData>> Call { get; private set; }
        public System.Func<System.Collections.Generic.IEnumerable<LoggerData>, System.Threading.Tasks.Task> Call { get; private set; }

        /// <summary>
        /// Gets the maximum out queue thread for this logger queue, default 1. Please increase the number of concurrent threads appropriately.
        /// </summary>
        public int WorkThreads { get; private set; } = 1;

        /// <summary>
        /// Gets the max capacity of this logger queue
        /// </summary>
        public int? MaxCapacity { get; private set; }

        /// <summary>
        /// Whether the callback log uses a new thread, default true.
        /// </summary>
        public bool ThreadCall { get; set; } = true;

        internal readonly System.Collections.Concurrent.BlockingCollection<LoggerData> LoggerQueue;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="call"></param>
        /// <param name="workThreads">Gets the maximum out queue thread for this logger queue, default 1. Please increase the number of concurrent threads appropriately.</param>
        /// <param name="maxCapacity">Gets the max capacity of this logger queue</param>
        public Logger(System.Func<System.Collections.Generic.IEnumerable<LoggerData>, System.Threading.Tasks.Task> call, int? workThreads = null, int? maxCapacity = null)
        {
            if (workThreads.HasValue && 0 < workThreads.Value)
            {
                this.WorkThreads = workThreads.Value;
            }

            if (maxCapacity.HasValue && -1 < maxCapacity.Value)
            {
                this.MaxCapacity = maxCapacity;
            }

            LoggerQueue = MaxCapacity.HasValue && 0 < MaxCapacity.Value ? new System.Collections.Concurrent.BlockingCollection<LoggerData>(MaxCapacity.Value) : new System.Collections.Concurrent.BlockingCollection<LoggerData>();

            if (null != call)
            {
                this.Call = call;
            }

            if (null != Call)
            {
                for (int i = 0; i < WorkThreads; i++)
                {
                    System.Threading.Tasks.Task.Factory.StartNew(async () =>
                    {
                        var list = new System.Collections.Generic.LinkedList<LoggerData>();

                        var wait = new System.Threading.SpinWait();
                        var watch = new System.Diagnostics.Stopwatch();

                        while (!LoggerQueue.IsCompleted)
                        {
                            var isRunning = 0 != System.TimeSpan.Zero.CompareTo(Batch.Interval);

                            if (!watch.IsRunning && isRunning)
                            {
                                watch.Start();
                            }
                            else if (!isRunning && watch.IsRunning)
                            {
                                watch.Stop();
                            }

                            if (0 < list.Count && (!isRunning || (isRunning && (0 < watch.Elapsed.CompareTo(Batch.Interval) || (0 < Batch.MaxNumber && Batch.MaxNumber <= list.Count)))))
                            {
                                if (ThreadCall)
                                {
                                    System.Threading.Tasks.Task.Factory.StartNew(async obj => await Call(obj as LoggerData[]), list.ToArray()).ContinueWith(c => c.Exception?.Console());
                                }
                                else
                                {
                                    try { await Call(list.ToArray()); }
                                    catch (System.Exception ex) { ex.Console(); }
                                }

                                list.Clear();

                                if (watch.IsRunning)
                                {
                                    watch.Restart();
                                }
                            }

                            if (LoggerQueue.TryTake(out LoggerData logger))
                            {
                                list.AddLast(logger);
                            }

                            wait.SpinOnce();
                        }

                        if (watch.IsRunning)
                        {
                            watch.Stop();
                        }

                        list.Clear();// count > 0 ?
                    }, System.Threading.Tasks.TaskCreationOptions.DenyChildAttach | System.Threading.Tasks.TaskCreationOptions.LongRunning);
                }
            }
        }
    }

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
            return Help.JsonSerialize(this);
        }
    }
}