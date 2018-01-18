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

namespace Business.Auth
{
    using System.Linq;
    using Business.Attributes;
    using Business.Meta;
    using Result;

    /// <summary>
    /// IInterceptor
    /// </summary>
    public interface IInterceptor : Castle.DynamicProxy.IInterceptor
    {
        System.Action<LoggerData> Logger { get; set; }

        System.Collections.Concurrent.ConcurrentDictionary<string, MetaData> MetaData { get; set; }

        System.Reflection.TypeInfo ResultType { get; set; }
    }

    struct ArgsLog
    {
        public string name;
        public dynamic value;
        public MetaLogger logger;
        public bool hasIArg;
    }

    /// <summary>
    /// Interceptor
    /// </summary>
    public class Interceptor : IInterceptor
    {
        /// <summary>
        /// Intercept
        /// </summary>
        /// <param name="invocation"></param>
        public virtual void Intercept(Castle.DynamicProxy.IInvocation invocation)
        {
            var startTime = new System.Diagnostics.Stopwatch();
            startTime.Start();
            var meta = this.MetaData[invocation.Method.Name];
            var methodName = meta.FullName;
            var argsObj = invocation.Arguments;
            //var argsObjLog = new System.Collections.Generic.Dictionary<string, System.Tuple<dynamic, MetaLogger>>(argsObj.Length);
            //var argsObjLog = new System.Collections.Generic.List<argsLog>(argsObj.Length);
            //var argsObjLog = argsObj.Select(c => new argsLog { }).ToList();
            //var argsObjLogHasIArg = new System.Collections.Generic.Dictionary<string, bool>(argsObj.Length);
            var logType = LoggerType.Record;
            //==================================//
            var iArgGroup = meta.GroupDefault;
            var iArgs = Bind.GetIArgs(meta.IArgs, argsObj, iArgGroup);
            if (0 < iArgs.Count)
            {
                var group = iArgs[meta.IArgs[0].Position].Group;
                if (!System.String.IsNullOrEmpty(group)) { iArgGroup = group; }
            }

            var args = meta.ArgAttrs[iArgGroup];

            //var argsObjLog = args.Args.Select(c => new argsLog {Key = c.Name, Value = c.HasIArg ? iArgs[c.Position] : argsObj[c.Position], Logger = c.MetaLogger }).ToList();

            try
            {
                foreach (var item in args.Args)
                {
                    IResult result = null;
                    var value = argsObj[item.Position];
                    var iArgIn = item.HasIArg ? iArgs[item.Position].In : null;
                    //argsObjLog.Add(item.Name, System.Tuple.Create(item.HasIArg ? iArgs[item.Position] : value, item.MetaLogger));
                    //argsObjLog.Add(new argsLog { Key = item.Name, Value = item.HasIArg ? iArgs[item.Position] : value, Logger = item.MetaLogger });
                    //argsObjLogHasIArg.Add(item.Name, item.HasIArg);

                    for (int i = 0; i < item.ArgAttr.Count; i++)
                    {
                        var argAttr = item.ArgAttr[i];

                        result = argAttr.Proces(item.HasIArg ? iArgIn : value);

                        if (1 > result.State)
                        {
                            invocation.ReturnValue = Bind.GetReturnValue(result, meta); logType = LoggerType.Error; return;
                        }

                        //========================================//

                        if (result.HasData)
                        {
                            if (!item.HasIArg)
                            {
                                argsObj[item.Position] = result.Data;
                            }
                            else
                            {
                                if (i < item.ArgAttr.Count - 1)
                                {
                                    iArgIn = result.Data;
                                }
                                else
                                {
                                    iArgs[item.Position].Out = result.Data;
                                }
                            }
                        }
                    }

                    //========================================//
                    object currentValue = item.HasIArg ?
                        ((null != result && result.HasData) ? result.Data : iArgs[item.Position].Out) :
                        ((null != result && result.HasData) ? result.Data : value);
                    //========================================//
                    //item.HasIArg && 
                    if (null == currentValue)
                    {
                        continue;
                    }

                    var isUpdate = false;

                    result = ArgsResult(item.ArgAttrChild, args.CommandAttr.OnlyName, ref currentValue, ref isUpdate);
                    if (null != result)
                    {
                        invocation.ReturnValue = Bind.GetReturnValue(result, meta); logType = LoggerType.Error; return;
                    }

                    if (item.HasIArg && isUpdate)
                    {
                        iArgs[item.Position].Out = currentValue;
                    }
                }

                //===============================//
                startTime.Restart();
                invocation.Proceed();
            }
            catch (System.Exception ex)
            {
                invocation.ReturnValue = Bind.GetReturnValue(0, System.Convert.ToString(Utils.Help.ExceptionWrite(ex)), meta, ResultType); logType = LoggerType.Exception;
            }
            finally
            {
                startTime.Stop();
                var total = Utils.Help.Scale(startTime.Elapsed.TotalSeconds, 3);

                if (null != this.Logger)
                {
                    if (meta.HasIResult && 0 > ((IResult)invocation.ReturnValue).State)
                    {
                        logType = LoggerType.Error;
                    }

                    var argsObjLog = args.Args.Select(c => new ArgsLog { name = c.Name, value = c.HasIArg ? iArgs[c.Position] : argsObj[c.Position], logger = c.MetaLogger, hasIArg = c.HasIArg }).ToList();

                    var logObjs = LoggerSet(logType, meta.MetaLogger, argsObjLog, argsObjLog.ToDictionary(c => c.name, c => c.hasIArg), out bool canWrite, out bool canResult);

                    if (canWrite)
                    {
                        System.Threading.Tasks.Task.Run(() => this.Logger(new LoggerData { Type = logType, Value = logObjs, Result = canResult ? invocation.ReturnValue : null, Time = total, Member = methodName, Group = args.CommandAttr.Group }));
                    }
                }
            }
        }

        static IResult ArgsResult(System.Collections.Generic.IList<Args> args, string methodName, ref object currentValue, ref bool isUpdate)
        {
            foreach (var item in args)
            {
                IResult result = null;
                var memberValue = item.Accessor.Getter(currentValue);
                //========================================//

                if (0 < item.ArgAttr.Count)
                {
                    var iArgIn = item.HasIArg ? null != memberValue ? ((IArg)memberValue).In : null : null;

                    for (int i = 0; i < item.ArgAttr.Count; i++)
                    {
                        result = item.ArgAttr[i].Proces(item.HasIArg ? iArgIn : memberValue);

                        if (1 > result.State)
                        {
                            return result;
                        }
                        if (result.HasData)
                        {
                            if (!item.HasIArg)
                            {
                                memberValue = result.Data;
                                item.Accessor.Setter(currentValue, memberValue);
                                if (!isUpdate) { isUpdate = !isUpdate; }
                            }
                            else
                            {
                                if (i < item.ArgAttr.Count - 1)
                                {
                                    iArgIn = result.Data;
                                }
                                else if (null != memberValue)
                                {
                                    ((IArg)memberValue).Out = result.Data;
                                }
                            }
                        }
                    }
                }

                object currentValue2 = item.HasIArg ?
                        ((null != result && result.HasData) ? result.Data : (null != memberValue ? ((IArg)memberValue).Out : null)) :
                        ((null != result && result.HasData) ? result.Data : memberValue);

                if (null == currentValue2)
                {
                    continue;
                }

                if (0 < item.ArgAttrChild.Count && null != currentValue2)
                {
                    var result2 = ArgsResult(item.ArgAttrChild, methodName, ref currentValue2, ref isUpdate);
                    if (null != result2)
                    {
                        return result2;
                    }
                }
            }

            return null;
        }

        static void LoggerSet(LoggerValueMode canValue, LoggerAttribute argLogAttr, System.Collections.Generic.Dictionary<string, dynamic> logObjs, string name, dynamic value)
        {
            switch (canValue)
            {
                case LoggerValueMode.All:
                    if (null != argLogAttr && !argLogAttr.CanWrite)
                    {
                        break;
                    }
                    logObjs.Add(name, value);
                    break;
                case LoggerValueMode.Select:
                    if (null != argLogAttr && argLogAttr.CanWrite)
                    {
                        logObjs.Add(name, value);
                    }
                    break;
                default: break;
            }
        }

        static LoggerValue LoggerSet(LoggerType logType, MetaLogger metaLogger, System.Collections.Generic.IList<ArgsLog> argsObjLog, System.Collections.Generic.IDictionary<string, bool> argsObjLogHasIArg, out bool canWrite, out bool canResult)
        {
            canWrite = canResult = false;
            var logObjs = new LoggerValue(argsObjLogHasIArg, argsObjLog.Count);

            switch (logType)
            {
                case LoggerType.Record:
                    if (metaLogger.Record.CanWrite)
                    {
                        foreach (var log in argsObjLog)
                        {
                            LoggerSet(metaLogger.Record.CanValue, log.logger.Record, logObjs, log.name, log.value);
                        }
                        canWrite = true;

                        if (metaLogger.Record.CanResult)
                        {
                            canResult = true;
                        }
                    }
                    break;
                case LoggerType.Error:
                    if (metaLogger.Error.CanWrite)
                    {
                        foreach (var log in argsObjLog)
                        {
                            LoggerSet(metaLogger.Error.CanValue, log.logger.Error, logObjs, log.name, log.value);
                        }
                        canWrite = true;

                        if (metaLogger.Error.CanResult)
                        {
                            canResult = true;
                        }
                    }
                    break;
                case LoggerType.Exception:
                    if (metaLogger.Exception.CanWrite)
                    {
                        foreach (var log in argsObjLog)
                        {
                            LoggerSet(metaLogger.Exception.CanValue, log.logger.Exception, logObjs, log.name, log.value);
                        }
                        canWrite = true;

                        if (metaLogger.Exception.CanResult)
                        {
                            canResult = true;
                        }
                    }
                    break;
                default: break;
            }

            return logObjs;
        }

        /// <summary>
        /// MetaData
        /// </summary>
        public System.Collections.Concurrent.ConcurrentDictionary<string, MetaData> MetaData { get; set; }

        /// <summary>
        /// Logger
        /// </summary>
        public System.Action<LoggerData> Logger { get; set; }

        /// <summary>
        /// Business
        /// </summary>
        public dynamic Business { get; set; }

        /// <summary>
        /// ResultType
        /// </summary>
        public System.Reflection.TypeInfo ResultType { get; set; }

        public void Dispose()
        {
            foreach (var item in this.MetaData)
            {
                item.Value.ArgAttrs.Clear();
                item.Value.Attributes.Clear();
            }
            this.MetaData.Clear();
            this.Logger = null;
            this.MetaData = null;
            this.Business = null;
        }
    }
}