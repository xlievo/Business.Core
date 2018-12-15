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
    using Business.Utils;
    using Result;

    /// <summary>
    /// IInterceptor
    /// </summary>
    public interface IInterceptor : Castle.DynamicProxy.IInterceptor
    {
        System.Action<LoggerData> Logger { get; set; }

        ConcurrentReadOnlyDictionary<string, MetaData> MetaData { get; set; }

        System.Type ResultType { get; set; }
    }

    struct ArgsLog
    {
        public string name;
        public dynamic value;
        public MetaLogger logger;
        public MetaLogger iArgInLogger;
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
        public virtual async void Intercept(Castle.DynamicProxy.IInvocation invocation)
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            var meta = this.MetaData[invocation.Method.Name];
            var methodName = meta.FullName;
            var argsObj = invocation.Arguments;
            var logType = LoggerType.Record;
            //==================================//
            var iArgGroup = meta.GroupDefault;
            var iArgs = Bind.GetIArgs(meta.IArgs, argsObj, iArgGroup);
            if (0 < iArgs.Count)
            {
                var group = iArgs[meta.IArgs[0].Position].Group;
                if (!string.IsNullOrEmpty(group)) { iArgGroup = group; }
            }

            if (!meta.CommandGroup.TryGetValue(iArgGroup, out CommandAttribute command))
            {
                invocation.ReturnValue = Bind.GetReturnValue(Bind.CmdError(ResultType, invocation.Method.Name), meta);
                return;
            }

            dynamic returnValue = null;

            try
            {
                foreach (var item in meta.Args)
                {
                    IResult result = null;
                    var value = argsObj[item.Position];
                    var iArgIn = item.HasIArg ? iArgs[item.Position].In : null;
                    var attrs = item.Group[iArgGroup].Attrs;

                    var first = attrs.First;

                    while (NodeState.DAT == first.State)
                    {
                        var argAttr = first.Value;

                        result = argAttr.Meta.HasProcesIArg ? await argAttr.Proces(item.HasIArg ? iArgIn : value, item.HasIArg ? iArgs[item.Position] : null) : await argAttr.Proces(item.HasIArg ? iArgIn : value);

                        if (1 > result.State)
                        {
                            logType = LoggerType.Error;
                            returnValue = result;
                            invocation.ReturnValue = Bind.GetReturnValue(result, meta);
                            return;
                        }

                        //========================================//

                        first = first.Next;

                        if (result.HasData)
                        {
                            if (!item.HasIArg)
                            {
                                argsObj[item.Position] = result.Data;
                            }
                            else
                            {
                                if (NodeState.DAT == first.State)
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

                    //var isUpdate = false;

                    result = await ArgsResult2(iArgGroup, item.ArgAttrChild, command.OnlyName, currentValue);
                    //if (null != result)
                    if (1 > result.State)
                    {
                        logType = LoggerType.Error;
                        returnValue = result;
                        invocation.ReturnValue = Bind.GetReturnValue(result, meta);
                        return;
                    }

                    if (item.HasIArg && result.Data)
                    {
                        iArgs[item.Position].Out = currentValue;
                    }
                }

                //===============================//
                //watch.Restart();
                invocation.Proceed();
                returnValue = invocation.ReturnValue;
            }
            catch (System.Exception ex)
            {
                ex = ex.ExceptionWrite();
                logType = LoggerType.Exception;
                if (!meta.HasAsync)
                {
                    returnValue = ResultFactory.ResultCreate(ResultType, 0, System.Convert.ToString(ex));
                }
                invocation.ReturnValue = !meta.HasAsync ? Bind.GetReturnValue(0, System.Convert.ToString(ex), meta, ResultType) : System.Threading.Tasks.Task.FromException(ex);
            }
            finally
            {
                if (meta.HasAsync)
                {
                    var task = invocation.ReturnValue as System.Threading.Tasks.Task;

                    if (meta.HasIResult)
                    {
                        invocation.ReturnValue = task.ContinueWith<IResult>(c => Async<IResult>(command, c, ResultType, meta, returnValue, logType, iArgs, argsObj, methodName, this.Logger, watch));
                    }
                    else
                    {
                        invocation.ReturnValue = task.ContinueWith(c => Async<dynamic>(command, c, ResultType, meta, returnValue, logType, iArgs, argsObj, methodName, this.Logger, watch));
                    }
                }
                else
                {
                    Finally(command, meta, returnValue, logType, iArgs, argsObj, methodName, this.Logger, watch);
                }
            }
        }

        static dynamic Async<Type>(CommandAttribute command, System.Threading.Tasks.Task task, System.Type resultType, MetaData meta, dynamic returnValue, LoggerType logType, System.Collections.Generic.Dictionary<int, IArg> iArgs, object[] argsObj, string methodName, System.Action<LoggerData> logger, System.Diagnostics.Stopwatch watch)
        {
            try
            {
                if (null != task.Exception)
                {
                    logType = LoggerType.Exception;
                    returnValue = ResultFactory.ResultCreate(resultType, 0, System.Convert.ToString(task.Exception.ExceptionWrite()));

                    return meta.HasReturn ? returnValue : default;
                }
                else if (meta.HasReturn)
                {
                    if (logType == LoggerType.Record)
                    {
                        returnValue = ((System.Threading.Tasks.Task<Type>)task).Result;
                    }

                    return returnValue;
                }

                if (logType == LoggerType.Record)
                {
                    returnValue = default;
                }

                return default;
            }
            catch (System.Exception ex)
            {
                logType = LoggerType.Exception;
                returnValue = ResultFactory.ResultCreate(resultType, 0, System.Convert.ToString(ex.ExceptionWrite()));
                return meta.HasReturn ? returnValue : null;
            }
            finally
            {
                Finally(command, meta, returnValue, logType, iArgs, argsObj, methodName, logger, watch);
            }
        }

        static void Finally(CommandAttribute command, MetaData meta, dynamic returnValue, LoggerType logType, System.Collections.Generic.Dictionary<int, IArg> iArgs, object[] argsObj, string methodName, System.Action<LoggerData> logger, System.Diagnostics.Stopwatch watch)
        {
            if (null == logger) { return; }

            if (!object.Equals(null, returnValue) && typeof(IResult).IsAssignableFrom(returnValue.GetType()))
            {
                var result = returnValue as IResult;
                if (0 > result.State)
                {
                    logType = LoggerType.Error;
                }
            }

            var argsObjLog = meta.Args.Select(c => new ArgsLog { name = c.Name, value = c.HasIArg ? iArgs[c.Position] : argsObj[c.Position], logger = c.Group[command.Key].Logger, iArgInLogger = c.Group[command.Key].IArgInLogger, hasIArg = c.HasIArg }).ToList();

            var logObjs = LoggerSet(logType, meta.MetaLogger[command.Key], argsObjLog, argsObjLog.ToDictionary(c => c.name, c => c.hasIArg), out bool canWrite, out bool canResult);

            watch.Stop();
            var total = Help.Scale(watch.Elapsed.TotalSeconds, 3);

            if (canWrite)
            {
                System.Threading.Tasks.Task.Run(() => logger(new LoggerData { Type = logType, Value = logObjs, Result = canResult ? returnValue : null, Time = total, Member = methodName, Group = command.Group }));
            }
        }

        async System.Threading.Tasks.Task<IResult> ArgsResult2(string group, System.Collections.Generic.IList<Args> args, string methodName, object currentValue)
        {
            bool isUpdate = false;

            foreach (var item in args)
            {
                IResult result = null;

                var memberValue = item.Accessor.TryGetter(currentValue);
                //========================================//

                var iArgIn = item.HasIArg ? null != memberValue ? ((IArg)memberValue).In : null : null;

                var attrs = item.Group[group].Attrs;

                var first = attrs.First;

                while (NodeState.DAT == first.State)
                {
                    var argAttr = first.Value;

                    result = argAttr.Meta.HasProcesIArg ? await argAttr.Proces(item.HasIArg ? iArgIn : memberValue, (item.HasIArg && null != memberValue) ? (IArg)memberValue : null) : await argAttr.Proces(item.HasIArg ? iArgIn : memberValue);

                    if (1 > result.State)
                    {
                        return result;
                    }

                    first = first.Next;

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
                            if (NodeState.DAT == first.State)
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

                object currentValue2 = item.HasIArg ?
                        ((null != result && result.HasData) ? result.Data : (null != memberValue ? ((IArg)memberValue).Out : null)) :
                        ((null != result && result.HasData) ? result.Data : memberValue);

                if (null == currentValue2)
                {
                    continue;
                }

                if (0 < item.ArgAttrChild.Count && null != currentValue2)
                {
                    var result2 = await ArgsResult2(group, item.ArgAttrChild, methodName, currentValue2);
                    if (1 > result2.State)
                    {
                        return result2;
                    }

                    if (result2.Data)
                    {
                        if (item.Type.IsValueType || item.HasIArg)
                        {
                            item.Accessor.Setter(currentValue, currentValue2);
                        }
                        if (!isUpdate) { isUpdate = !isUpdate; }
                    }
                }
            }

            return ResultFactory.ResultCreate(ResultType, isUpdate);
        }

        static void LoggerSet(LoggerValueMode canValue, LoggerAttribute argLogAttr, LoggerAttribute iArgInLogAttr, LoggerValue logObjs, ArgsLog log)
        {
            //meta
            switch (canValue)
            {
                case LoggerValueMode.All:
                    if (null != argLogAttr && !argLogAttr.CanWrite)
                    {
                        break;
                    }

                    if (null != iArgInLogAttr && !iArgInLogAttr.CanWrite && log.hasIArg)
                    {
                        logObjs.Add(log.name, (log.value as IArg).Out);
                        logObjs.HasIArg[log.name] = false;
                    }
                    else if (log.hasIArg)
                    {
                        logObjs.Add(log.name, log.value as IArg);
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
                            logObjs.HasIArg[log.name] = false;
                        }
                        else if (log.hasIArg)
                        {
                            logObjs.Add(log.name, log.value as IArg);
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

        static LoggerValue LoggerSet(LoggerType logType, MetaLogger logger, System.Collections.Generic.IList<ArgsLog> argsObjLog, System.Collections.Generic.IDictionary<string, bool> argsObjLogHasIArg, out bool canWrite, out bool canResult)
        {
            canWrite = canResult = false;
            var logObjs = new LoggerValue(argsObjLogHasIArg, argsObjLog.Count);

            switch (logType)
            {
                case LoggerType.Record:
                    if (logger.Record.CanWrite)
                    {
                        foreach (var log in argsObjLog)
                        {
                            LoggerSet(logger.Record.CanValue, log.logger.Record, log.iArgInLogger.Record, logObjs, log);
                        }
                        canWrite = true;

                        if (logger.Record.CanResult)
                        {
                            canResult = true;
                        }
                    }
                    break;
                case LoggerType.Error:
                    if (logger.Error.CanWrite)
                    {
                        foreach (var log in argsObjLog)
                        {
                            LoggerSet(logger.Error.CanValue, log.logger.Error, log.iArgInLogger.Error, logObjs, log);
                        }
                        canWrite = true;

                        if (logger.Error.CanResult)
                        {
                            canResult = true;
                        }
                    }
                    break;
                case LoggerType.Exception:
                    if (logger.Exception.CanWrite)
                    {
                        foreach (var log in argsObjLog)
                        {
                            LoggerSet(logger.Exception.CanValue, log.logger.Exception, log.iArgInLogger.Exception, logObjs, log);
                        }
                        canWrite = true;

                        if (logger.Exception.CanResult)
                        {
                            canResult = true;
                        }
                    }
                    break;
                default: break;
            }

            return 0 == logObjs.Count ? null : logObjs;
        }

        /// <summary>
        /// MetaData
        /// </summary>
        public ConcurrentReadOnlyDictionary<string, MetaData> MetaData { get; set; }

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
        public System.Type ResultType { get; set; }

        public void Dispose()
        {
            foreach (var item in this.MetaData)
            {
                item.Value.Args.collection.Clear();
                item.Value.Attributes.Clear();
            }
            this.MetaData.dictionary.Clear();
            this.Logger = null;
            this.MetaData = null;
            this.Business = null;
        }
    }
}