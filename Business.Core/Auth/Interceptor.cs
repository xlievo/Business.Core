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
        ConcurrentReadOnlyDictionary<string, MetaData> MetaData { get; set; }

        Configer Configer { get; set; }
    }

    struct ArgsLog
    {
        public string name;
        public dynamic value;
        public MetaLogger logger;
        public MetaLogger iArgInLogger;
        public bool hasIArg;
    }

    struct ArgResult
    {
        public bool isUpdate;

        public dynamic value;
    }

    /// <summary>
    /// Interceptor
    /// </summary>
    public class Interceptor : IInterceptor
    {
        public void Intercept(Castle.DynamicProxy.IInvocation invocation) => invocation.ReturnValue = InterceptAsync(invocation).Result;

        /// <summary>
        /// Intercept
        /// </summary>
        /// <param name="invocation"></param>
        public virtual async System.Threading.Tasks.Task<dynamic> InterceptAsync(Castle.DynamicProxy.IInvocation invocation)
        {
            var proceed = invocation.CaptureProceedInfo();
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
                invocation.ReturnValue = Bind.GetReturnValue(ResultFactory.ResultCreate(meta, -2, $"Without this Cmd {invocation.Method.Name}"), meta);
                return invocation.ReturnValue;
            }

            dynamic returnValue = null;

            try
            {
                //if (null != Configer.CallBefore)
                //{
                //    await Configer.CallBefore(meta);
                //}

                foreach (var item in meta.Args)
                {
                    IResult result = null;
                    var value = argsObj[item.Position];
                    var iArgIn = item.HasIArg ? iArgs[item.Position].In : null;
                    var attrs = item.Group[iArgGroup].Attrs;

                    var first = attrs.First;

                    //while (NodeState.DAT == first.State) // .net fx ConcurrentLinkedList TryAdd error! State = INV 
                    while (NodeState.DAT == first.State)
                    {
                        var argAttr = first.Value;

                        if (argAttr.CollectionItem) { first = first.Next; continue; }

                        result = await GetProcesResult(argAttr, item.HasIArg ? iArgIn : value, item.HasIArg ? iArgs[item.Position] : null);

                        if (1 > result.State)
                        {
                            logType = LoggerType.Error;
                            returnValue = result;
                            invocation.ReturnValue = meta.HasIResult ? Bind.GetReturnValueIResult((dynamic)result, meta) : Bind.GetReturnValue(result, meta);
                            return invocation.ReturnValue;
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

                    if (item.HasLower || item.HasCollectionAttr)
                    {
                        if (item.HasCollection)
                        {
                            #region Collection

                            dynamic currentValue2 = currentValue;
                            int collectioCount = currentValue2.Count;

                            var collectioTasks = new System.Collections.Generic.List<System.Threading.Tasks.Task>(collectioCount);

                            System.Threading.Tasks.Parallel.For(0, collectioCount, (c, o) =>
                            {
                                if (item.HasCollectionAttr)
                                {
                                    object v2 = null;
                                    dynamic key = null;
                                    if (item.HasDictionary)
                                    {
                                        var entry = System.Linq.Enumerable.ElementAt(currentValue2, c);
                                        key = entry.Key;
                                        v2 = entry.Value;
                                    }
                                    else
                                    {
                                        v2 = currentValue2[c];
                                    }

                                    var collectioTask = ArgsResultCollection(meta, item, attrs, v2, iArgGroup, command.OnlyName, c, (object)key).ContinueWith(c2 =>
                                    {
                                        if (null != c2.Exception)
                                        {
                                            var ex = c2.Exception.ExceptionWrite();
                                            logType = LoggerType.Exception;
                                            returnValue = ResultFactory.ResultCreate(meta, 0, System.Convert.ToString(ex));
                                            o.Stop();
                                            return;
                                        }

                                        var result2 = c2.Result;

                                        if (1 > result2.State)
                                        {
                                            logType = LoggerType.Error;
                                            returnValue = result2;
                                            //invocation.ReturnValue = Bind.GetReturnValue(result2, meta);
                                            o.Stop();
                                            return;
                                        }
                                        else
                                        {
                                            result = result2;
                                            if (item.HasDictionary)
                                            {
                                                currentValue2[key] = result2.Data.value;
                                            }
                                            else
                                            {
                                                currentValue2[c] = result2.Data.value;
                                            }
                                        }
                                    });

                                    collectioTasks.Add(collectioTask);
                                }
                                //==========HasLower==========//
                                else if (item.HasLower)
                                {
                                    object v2 = null;
                                    dynamic key = null;
                                    if (item.HasDictionary)
                                    {
                                        var entry = System.Linq.Enumerable.ElementAt(currentValue2, c);
                                        key = entry.Key;
                                        v2 = entry.Value;
                                    }
                                    else
                                    {
                                        v2 = currentValue2[c];
                                    }

                                    var collectioTask2 = ArgsResult(meta, iArgGroup, item.Children, command.OnlyName, v2, c).ContinueWith(c3 =>
                                     {
                                         if (null != c3.Exception)
                                         {
                                             var ex = c3.Exception.ExceptionWrite();
                                             logType = LoggerType.Exception;
                                             returnValue = ResultFactory.ResultCreate(meta, 0, System.Convert.ToString(ex));
                                             o.Stop();
                                             return;
                                         }

                                         var result3 = c3.Result;

                                         if (1 > result3.State)
                                         {
                                             logType = LoggerType.Error;
                                             returnValue = result3;
                                             //invocation.ReturnValue = Bind.GetReturnValue(result3, meta);
                                             o.Stop();
                                             return;
                                         }
                                         else
                                         {
                                             result = result3;
                                             if (item.HasDictionary)
                                             {
                                                 currentValue2[key] = result3.Data.value;
                                             }
                                             else
                                             {
                                                 currentValue2[c] = result3.Data.value;
                                             }
                                         }
                                     });

                                    collectioTasks.Add(collectioTask2);
                                }
                            });

                            System.Threading.Tasks.Task.WaitAll(collectioTasks.ToArray());

                            if (null != returnValue)
                            {
                                invocation.ReturnValue = meta.HasIResult ? Bind.GetReturnValueIResult(returnValue, meta) : Bind.GetReturnValue(returnValue, meta);
                                return invocation.ReturnValue;
                            }

                            #endregion
                        }
                        else
                        {
                            result = await ArgsResult(meta, iArgGroup, item.Children, command.OnlyName, currentValue);
                            //if (null != result)
                            if (1 > result.State)
                            {
                                logType = LoggerType.Error;
                                returnValue = result;
                                invocation.ReturnValue = meta.HasIResult ? Bind.GetReturnValueIResult((dynamic)result, meta) : Bind.GetReturnValue(result, meta);
                                return invocation.ReturnValue;
                            }
                        }

                        if (item.HasIArg && !System.Object.Equals(null, result))
                        {
                            if (result.Data is ArgResult && result.Data.isUpdate)
                            {
                                iArgs[item.Position].Out = currentValue;
                            }
                        }
                    }
                }

                //===============================//
                //watch.Restart();
                //..CallBeforeMethod..//
                if (null != Configer.CallBeforeMethod)
                {
                    await Configer.CallBeforeMethod(meta, meta.Args.ToDictionary(c => c.Name, c => new MethodArgs { Name = c.Name, Value = c.HasIArg ? iArgs[c.Position] : argsObj[c.Position], HasIArg = c.HasIArg, Type = c.Type, OutType = c.IArgOutType, InType = c.IArgInType }));
                }

                proceed.Invoke();

                if (meta.HasAsync)
                {
                    var task = invocation.ReturnValue as System.Threading.Tasks.Task;
                    if (null != task?.Exception)
                    {
                        throw task?.Exception;
                    }
                }

                //result
                if (!meta.HasReturn)
                {
                    returnValue = null;
                    invocation.ReturnValue = Bind.GetReturnValue(ResultFactory.ResultCreate(meta), meta);
                }
                else
                {
                    //log
                    returnValue = invocation.ReturnValue;
                }

                //..CallAfterMethod..//
                if (null != Configer.CallAfterMethod)
                {
                    await Configer.CallAfterMethod(meta, meta.Args.ToDictionary(c => c.Name, c => new MethodArgs { Name = c.Name, Value = c.HasIArg ? iArgs[c.Position] : argsObj[c.Position], HasIArg = c.HasIArg, Type = c.Type, OutType = c.IArgOutType, InType = c.IArgInType }), invocation.ReturnValue);
                }

                return invocation.ReturnValue;
            }
            catch (System.Exception ex)
            {
                ex = ex.ExceptionWrite();
                logType = LoggerType.Exception;

                //log
                returnValue = ResultFactory.ResultCreate(meta, 0, System.Convert.ToString(ex));

                //result
                invocation.ReturnValue = meta.HasIResult ? Bind.GetReturnValueIResult(returnValue, meta) : Bind.GetReturnValue(returnValue, meta);

                //if (!meta.HasAsync)
                //{
                //    invocation.ReturnValue = result;
                //}
                //else
                //{
                //    invocation.ReturnValue = System.Threading.Tasks.Task.FromResult(result);
                //}

                //invocation.ReturnValue = !meta.HasAsync ? Bind.GetReturnValue(0, System.Convert.ToString(ex), meta, ResultType) : System.Threading.Tasks.Task.FromException(ex);
                return invocation.ReturnValue;// meta.HasReturn ? invocation.ReturnValue : default;
            }
            finally
            {
                Finally(command, meta, returnValue, logType, iArgs, argsObj, methodName, watch);
            }
        }

        /*
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
        */

        async void Finally(CommandAttribute command, MetaData meta, dynamic returnValue, LoggerType logType, System.Collections.Generic.Dictionary<int, IArg> iArgs, object[] argsObj, string methodName, System.Diagnostics.Stopwatch watch)
        {
            if (meta.HasAsync)
            {
                var task = returnValue as System.Threading.Tasks.Task<dynamic>;
                if (null != task)
                {
                    returnValue = await task;
                }
            }

            if (!object.Equals(null, returnValue) && typeof(IResult).IsAssignableFrom(returnValue.GetType()))
            {
                var result = returnValue as IResult;
                if (0 > result.State)
                {
                    logType = LoggerType.Error;
                }
            }

            //var argsObjLog = meta.Args.Select(c => new ArgsLog { name = c.Name, value = c.HasIArg ? iArgs[c.Position] : argsObj[c.Position], logger = c.Group[command.Key].Logger, iArgInLogger = c.Group[command.Key].IArgInLogger, hasIArg = c.HasIArg }).ToList();

            //var logObjs = LoggerSet(logType, meta.MetaLogger[command.Key], argsObjLog, argsObjLog.ToDictionary(c => c.name, c => c.hasIArg), out bool canWrite, out bool canResult);

            var argsObjLog = new System.Collections.Generic.List<ArgsLog>(meta.Args.Count);
            var argsObjLogHasIArg = new System.Collections.Generic.Dictionary<string, bool>(meta.Args.Count);

            foreach (var c in meta.Args)
            {
                var value = c.HasIArg ? iArgs[c.Position] : argsObj[c.Position];

                argsObjLog.Add(new ArgsLog { name = c.Name, value = value, logger = c.Group[command.Key].Logger, iArgInLogger = c.Group[command.Key].IArgInLogger, hasIArg = c.HasIArg });
                argsObjLogHasIArg.Add(c.Name, c.HasIArg);
            }

            var logObjs = LoggerSet(logType, meta.MetaLogger[command.Key], argsObjLog, argsObjLogHasIArg, out bool canWrite, out bool canResult);

            watch.Stop();
            var total = Help.Scale(watch.Elapsed.TotalSeconds, 3);

            if (null == Configer.Logger?.Call) { return; }

            if (canWrite)
            {
                if (Configer.Logger?.MaxCapacity <= Configer.Logger?.LoggerQueue.Count)
                {
                    return;
                }

                Configer.Logger?.LoggerQueue.TryAdd(new LoggerData { Type = logType, Value = logObjs, Result = canResult ? returnValue : null, Time = total, Member = methodName, Group = command.Group });
                //if (loggerUseThreadPool)
                //{
                //    System.Threading.Tasks.Task.Run(() => logger(new LoggerData { Type = logType, Value = logObjs, Result = canResult ? returnValue : null, Time = total, Member = methodName, Group = command.Group }));
                //}
                //else
                //{
                //    System.Threading.Tasks.Task.Factory.StartNew(() => logger(new LoggerData { Type = logType, Value = logObjs, Result = canResult ? returnValue : null, Time = total, Member = methodName, Group = command.Group }), System.Threading.Tasks.TaskCreationOptions.DenyChildAttach | System.Threading.Tasks.TaskCreationOptions.LongRunning);
                //}
            }
        }

        async static System.Threading.Tasks.ValueTask<IResult> GetProcesResult(ArgumentAttribute argAttr, dynamic value, IArg arg, int collectionIndex = -1, dynamic dictKey = null)
        {
            switch (argAttr.Meta.HasProcesIArg)
            {
                case ArgumentAttribute.MetaData.ProcesMode.Proces:
                    return await argAttr.Proces(value);
                case ArgumentAttribute.MetaData.ProcesMode.ProcesIArg:
                    return await argAttr.Proces(value, arg);
                case ArgumentAttribute.MetaData.ProcesMode.ProcesIArgCollection:
                    return await argAttr.Proces(value, arg, collectionIndex, dictKey);
                default: return null;
            }
        }

        async System.Threading.Tasks.Task<IResult> ArgsResult(MetaData meta, string group, System.Collections.Generic.IList<Args> args, string methodName, object currentValue, int collectionIndex = -1, dynamic dictKey = null)
        {
            bool isUpdate = false;

            foreach (var item in args)
            {
                IResult result = null;

                var memberValue = null != currentValue ? item.Accessor.TryGetter(currentValue) : null;
                //========================================//

                var iArgIn = item.HasIArg ? null != memberValue ? ((IArg)memberValue).In : null : null;

                var attrs = item.Group[group].Attrs;

                var first = attrs.First;

                while (NodeState.DAT == first.State)
                {
                    var argAttr = first.Value;

                    if (argAttr.CollectionItem) { first = first.Next; continue; }

                    result = await GetProcesResult(argAttr, item.HasIArg ? iArgIn : memberValue, (item.HasIArg && null != memberValue) ? (IArg)memberValue : null, collectionIndex, dictKey);

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
                            if (null != currentValue)
                            {
                                item.Accessor.Setter(currentValue, memberValue);
                            }
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

                //if (0 < item.ArgAttrChild.Count && null != currentValue2)
                if ((item.HasLower || item.HasCollectionAttr) && null != currentValue2)
                {
                    dynamic result2 = null;

                    if (item.HasCollection)
                    {
                        #region Collection

                        dynamic result3 = null; //error
                        dynamic currentValue3 = currentValue2;
                        int collectioCount = currentValue3.Count;

                        var collectioTasks = new System.Collections.Generic.List<System.Threading.Tasks.Task>(collectioCount);

                        System.Threading.Tasks.Parallel.For(0, collectioCount, (c, o) =>
                        {
                            if (item.HasCollectionAttr)
                            {
                                object v2 = null;
                                dynamic key = null;
                                if (item.HasDictionary)
                                {
                                    var entry = System.Linq.Enumerable.ElementAt(currentValue3, c);
                                    key = entry.Key;
                                    v2 = entry.Value;
                                }
                                else
                                {
                                    v2 = currentValue3[c];
                                }

                                var collectioTask = ArgsResultCollection(meta, item, attrs, v2, group, methodName, c, (object)key).ContinueWith(c2 =>
                                {
                                    if (null != c2.Exception)
                                    {
                                        var ex = c2.Exception.ExceptionWrite();
                                        result3 = ResultFactory.ResultCreate(meta, 0, System.Convert.ToString(ex));
                                        o.Stop();
                                        return;
                                    }

                                    var result4 = c2.Result;

                                    if (1 > result4.State)
                                    {
                                        result3 = result4;
                                        o.Stop();
                                        return;
                                    }
                                    else
                                    {
                                        result2 = result4;
                                        if (item.HasDictionary)
                                        {
                                            currentValue3[key] = result4.Data.value;
                                        }
                                        else
                                        {
                                            currentValue3[c] = result4.Data.value;
                                        }
                                    }
                                });

                                collectioTasks.Add(collectioTask);
                            }
                            else if (item.HasLower)
                            {
                                object v2 = null;
                                dynamic key = null;
                                if (item.HasDictionary)
                                {
                                    var entry = System.Linq.Enumerable.ElementAt(currentValue3, c);
                                    key = entry.Key;
                                    v2 = entry.Value;
                                }
                                else
                                {
                                    v2 = currentValue3[c];
                                }

                                var collectioTask2 = ArgsResult(meta, group, item.Children, methodName, v2, c).ContinueWith(c3 =>
                                {
                                    if (null != c3.Exception)
                                    {
                                        var ex = c3.Exception.ExceptionWrite();
                                        result3 = ResultFactory.ResultCreate(meta, 0, System.Convert.ToString(ex));
                                        o.Stop();
                                        return;
                                    }

                                    var result5 = c3.Result;

                                    if (1 > result5.State)
                                    {
                                        result3 = result5;
                                        o.Stop();
                                        return;
                                    }
                                    else
                                    {
                                        result2 = result5;
                                        if (item.HasDictionary)
                                        {
                                            currentValue3[key] = result5.Data.value;
                                        }
                                        else
                                        {
                                            currentValue3[c] = result5.Data.value;
                                        }
                                    }
                                });

                                collectioTasks.Add(collectioTask2);
                            }
                        });

                        System.Threading.Tasks.Task.WaitAll(collectioTasks.ToArray());

                        if (null != result3)
                        {
                            return result3;
                        }

                        #endregion
                    }
                    else
                    {
                        result2 = await ArgsResult(meta, group, item.Children, methodName, currentValue2);

                        if (1 > result2.State)
                        {
                            return result2;
                        }
                    }

                    if (!System.Object.Equals(null, result2) && result2.Data is ArgResult && result2.Data.isUpdate)
                    {
                        if (item.Type.IsValueType || item.HasIArg)
                        {
                            if (null != currentValue)
                            {
                                item.Accessor.Setter(currentValue, currentValue2);
                            }
                        }

                        if (!isUpdate) { isUpdate = !isUpdate; }
                    }
                }
            }

            return ResultFactory.ResultCreate(Configer.ResultTypeDefinition, new ArgResult { isUpdate = isUpdate, value = currentValue });
        }

        async System.Threading.Tasks.Task<IResult> ArgsResultCollection(MetaData meta, Args item, ConcurrentLinkedList<ArgumentAttribute> attrs, object currentValue, string group, string methodName, int collectionIndex, dynamic dictKey = null)
        {
            bool isUpdate = false;

            var iArg = item.HasCollectionIArg && null != currentValue ? (IArg)currentValue : null;
            var iArgIn = item.HasCollectionIArg ? iArg?.In : null;

            var first = attrs.First;

            while (NodeState.DAT == first.State)
            {
                var argAttr = first.Value;

                if (!argAttr.CollectionItem) { first = first.Next; continue; }

                var result = await GetProcesResult(argAttr, item.HasCollectionIArg ? iArgIn : currentValue, iArg, collectionIndex, dictKey);

                if (1 > result.State)
                {
                    return result;
                }

                first = first.Next;

                if (result.HasData)
                {
                    if (!item.HasCollectionIArg)
                    {
                        currentValue = result.Data;
                        if (!isUpdate) { isUpdate = !isUpdate; }
                    }
                    else
                    {
                        if (NodeState.DAT == first.State)
                        {
                            iArgIn = result.Data;
                        }
                        else if (null != currentValue)
                        {
                            ((IArg)currentValue).Out = result.Data;
                        }
                    }
                }
            }
            //==========HasLower==========//
            if (item.HasLower)
            {
                var result2 = await ArgsResult(meta, group, item.Children, methodName, currentValue);

                if (1 > result2.State)
                {
                    return result2;
                }

                return ResultFactory.ResultCreate(Configer.ResultTypeDefinition, data: result2.Data);
            }

            return ResultFactory.ResultCreate(Configer.ResultTypeDefinition, new ArgResult { isUpdate = isUpdate, value = currentValue });
        }

        static void LoggerSet(LoggerValueMode canValue, LoggerAttribute argLogAttr, LoggerAttribute iArgInLogAttr, LoggerValue logObjs, ArgsLog log)
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

        ///// <summary>
        ///// ResultType
        ///// </summary>
        //public System.Type ResultType { get; set; }

        /// <summary>
        /// Configer
        /// </summary>
        public Configer Configer { get; set; }
        //public void Dispose()
        //{
        //    foreach (var item in this.MetaData)
        //    {
        //        item.Value.Args.collection.Clear();
        //        item.Value.Attributes.Clear();
        //    }
        //    this.MetaData.dictionary.Clear();
        //    this.Logger = null;
        //    this.MetaData = null;
        //    this.Business = null;
        //}
    }
}