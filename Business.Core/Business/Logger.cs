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

    public class Logger
    {
        /// <summary>
        /// Return log number, default 1
        /// </summary>
        public int Number { get; set; } = 1;

        System.TimeSpan timeOut = System.TimeSpan.Zero;

        /// <summary>
        /// Return log timeout, default System.TimeSpan.Zero equals not enabled
        /// </summary>
        public System.TimeSpan TimeOut
        {
            get => timeOut;
            set
            {
                if (0 == System.TimeSpan.Zero.CompareTo(value))
                {
                    watch.Reset();
                }
                else
                {
                    watch.Start();
                }

                timeOut = value;
            }
        }

        /// <summary>
        /// Whether the callback log uses a new thread
        /// </summary>
        public bool UseThread { get; set; }

        /// <summary>
        /// Logger
        /// </summary>
        public System.Action<System.Collections.Generic.IEnumerable<LoggerData>> Call { get; private set; }

        /// <summary>
        /// Gets the max capacity of this logger queue
        /// </summary>
        public int? MaxCapacity { get; private set; }

        internal readonly System.Collections.Concurrent.BlockingCollection<LoggerData> LoggerQueue;

        readonly System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

        public Logger(System.Action<System.Collections.Generic.IEnumerable<LoggerData>> call, int? maxCapacity = null, int? number = null, System.TimeSpan? timeOut = null, bool useThread = false)
        {
            this.UseThread = useThread;

            if (maxCapacity.HasValue && -1 < maxCapacity.Value)
            {
                this.MaxCapacity = maxCapacity;
            }

            LoggerQueue = MaxCapacity.HasValue ? new System.Collections.Concurrent.BlockingCollection<LoggerData>(MaxCapacity.Value) : new System.Collections.Concurrent.BlockingCollection<LoggerData>();

            if (number.HasValue)
            {
                this.Number = number.Value;
            }

            if (timeOut.HasValue)
            {
                this.TimeOut = timeOut.Value;
            }

            if (null != call)
            {
                this.Call = call;
            }

            if (null != Call)
            {
                System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    var list = new System.Collections.Generic.List<LoggerData>(Number);

                    var wait = new System.Threading.SpinWait();

                    while (!LoggerQueue.IsCompleted)
                    {
                        if (0 < list.Count && ((0 < Number && Number <= list.Count) || (watch.IsRunning && 0 < watch.Elapsed.CompareTo(TimeOut))))
                        {
                            if (UseThread)
                            {
                                System.Threading.Tasks.Task.Factory.StartNew((c) => Call(c as LoggerData[]), list.ToArray()).ContinueWith(c =>
                                {
                                    if (null != c.Exception)
                                    {
                                        c.Exception.Console();
                                    }
                                });
                            }
                            else
                            {
                                try { Call(list.ToArray()); }
                                catch (System.Exception ex) { ex.Console(); }
                            }

                            list.Clear();

                            if (watch.IsRunning)
                            {
                                watch.Restart();
                            }
                        }

                        if ((0 < Number || watch.IsRunning) && LoggerQueue.TryTake(out LoggerData logger))
                        {
                            list.Add(logger);
                        }

                        wait.SpinOnce();
                    }

                    watch.Stop();
                    list.Clear();// count > 0 ?
                }, System.Threading.Tasks.TaskCreationOptions.DenyChildAttach | System.Threading.Tasks.TaskCreationOptions.LongRunning);
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
            return Utils.Help.JsonSerialize(this);
        }
    }
}