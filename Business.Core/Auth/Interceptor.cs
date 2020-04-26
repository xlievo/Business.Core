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

namespace Business.Core.Auth
{
    using Core;
    using Utils;
    using System.Linq;

    /// <summary>
    /// IInterceptor
    /// </summary>
    public interface IInterceptor
    {
        /// <summary>
        /// Configer
        /// </summary>
        Configer Configer { get; set; }

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="type"></param>
        /// <param name="constructorArguments"></param>
        /// <param name="ignoreMethods"></param>
        /// <returns></returns>
        object Create(System.Type type, object[] constructorArguments = null, System.Collections.Generic.IEnumerable<System.Reflection.MethodInfo> ignoreMethods = null);

        /// <summary>
        /// Intercept
        /// </summary>
        /// <param name="configer"></param>
        /// <param name="method"></param>
        /// <param name="arguments"></param>
        /// <param name="call"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        System.Threading.Tasks.ValueTask<dynamic> Intercept(Configer configer, string method, object[] arguments, System.Func<dynamic> call, string group = null);
    }

    readonly struct ArgResult
    {
        public ArgResult(bool isUpdate, dynamic value)
        {
            this.isUpdate = isUpdate;
            this.value = value;
        }

        public readonly bool isUpdate;
        public readonly dynamic value;
    }

    /// <summary>
    /// Interceptor
    /// </summary>
    public class Interceptor : IInterceptor, Castle.DynamicProxy.IInterceptor
    {
        class BusinessAllMethodsHook : Castle.DynamicProxy.AllMethodsHook
        {
            readonly System.Collections.Generic.IEnumerable<string> ignoreMethods;

            public BusinessAllMethodsHook(System.Collections.Generic.IEnumerable<System.Reflection.MethodInfo> methods = null)
                : base() => ignoreMethods = methods?.Select(c => c.GetMethodFullName());

            public override bool ShouldInterceptMethod(System.Type type, System.Reflection.MethodInfo methodInfo)
            {
                if (null != ignoreMethods)
                {
                    if (ignoreMethods.Any(c => string.Equals(c, methodInfo.GetMethodFullName()))) { return false; }
                }

                return base.ShouldInterceptMethod(type, methodInfo);
            }
        }

        /// <summary>
        /// Configer
        /// </summary>
        public Configer Configer { get; set; }

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="businessType"></param>
        /// <param name="constructorArguments"></param>
        /// <param name="ignoreMethods"></param>
        /// <returns></returns>
        public object Create(System.Type businessType, object[] constructorArguments = null, System.Collections.Generic.IEnumerable<System.Reflection.MethodInfo> ignoreMethods = null)
        {
            var proxy = new Castle.DynamicProxy.ProxyGenerator();

            try
            {
                var types = constructorArguments?.Select(c => c.GetType())?.ToArray();

                var constructor = null == types ? null : businessType.GetConstructor(types);

                var instance = proxy.CreateClassProxy(businessType, new Castle.DynamicProxy.ProxyGenerationOptions(new BusinessAllMethodsHook(ignoreMethods)), null == constructor ? businessType.GetConstructors()?.FirstOrDefault()?.GetParameters().Select(c => c.HasDefaultValue ? c.DefaultValue : default).ToArray() : constructorArguments, this);

                return instance;
            }
            catch (System.Exception ex)
            {
                throw ex.ExceptionWrite(true, true);
            }
        }

        /// <summary>
        /// Intercept
        /// </summary>
        /// <param name="invocation"></param>
        public void Intercept(Castle.DynamicProxy.IInvocation invocation)
        {
            if (null == this.Configer.MetaData)
            {
                invocation.ReturnValue = null; return;
            }

            var proceed = invocation.CaptureProceedInfo();

            invocation.ReturnValue = Intercept(this.Configer, invocation.Method.Name, invocation.Arguments, () =>
            {
                proceed.Invoke();
                return invocation.ReturnValue;
            }).Result;
        }

        /// <summary>
        /// Intercept
        /// </summary>
        /// <param name="configer"></param>
        /// <param name="method"></param>
        /// <param name="arguments"></param>
        /// <param name="call"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.ValueTask<dynamic> Intercept(Configer configer, string method, object[] arguments, System.Func<dynamic> call, string group = null) => await InterceptorExtensions.Intercept(configer, method, arguments, call, group);
    }
}

namespace Business.Core.Utils
{
    using Result;
    using Annotations;
    using Meta;
    using Auth;
    using System.Linq;

    /// <summary>
    /// InterceptorExtensions
    /// </summary>
    public static class InterceptorExtensions
    {
        /// <summary>
        /// Intercept
        /// </summary>
        /// <param name="configer"></param>
        /// <param name="method"></param>
        /// <param name="arguments"></param>
        /// <param name="call"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public static async System.Threading.Tasks.ValueTask<dynamic> Intercept(Configer configer, string method, object[] arguments, System.Func<dynamic> call, string group = null)
        {
            var dtt = System.DateTimeOffset.Now;
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            var meta = configer.MetaData[method];
            var methodName = meta.FullName;
            var argsObj = arguments;
            var logType = Logger.Type.Record;
            //==================================//
            var iArgs = Help.GetIArgs(meta.IArgs, argsObj);

            dynamic token = null;
            dynamic token2 = null;
            dynamic returnValue = null;

            var group2 = group ?? meta.GroupDefault;
            if (!meta.CommandGroup.Group.TryGetValue(group2, out CommandAttribute command))
            {
                returnValue = ResultFactory.ResultCreate(meta, -3, $"Without this Group {group2}");
                return meta.HasIResult ? Help.GetReturnValueIResult(returnValue, meta) : Help.GetReturnValue(returnValue, meta);
            }

            try
            {
                //..CallBeforeMethod..//
                if (null != configer.CallBeforeMethod)
                {
                    var before = new Configer.MethodBefore { Meta = meta, Args = meta.Args.ToDictionary(c => c.Name, c => c.HasIArg ? iArgs[c.Position].In : argsObj[c.Position]), Cancel = false };

                    await configer.CallBeforeMethod(before);

                    if (before.Cancel)
                    {
                        returnValue = ResultFactory.ResultCreate(meta, -4, $"{methodName} Cancel");
                        return meta.HasIResult ? Help.GetReturnValueIResult(returnValue, meta) : Help.GetReturnValue(returnValue, meta);
                    }
                }

                var tokenArg = meta.Args.FirstOrDefault(c => c.HasToken);
                if (null != tokenArg)
                {
                    token = argsObj[tokenArg.Position];
                    token2 = token;
                }

                foreach (var item in meta.Args)
                {
                    IResult result = null;
                    var value = argsObj[item.Position];
                    var iArgIn = item.HasIArg ? iArgs[item.Position].In : null;
                    var attrs = item.Group[command.Key].Attrs;
                    //if (item.HasToken)
                    //{
                    //    token = value;
                    //}

                    dynamic error = null;
                    for (int i = 0; i < attrs.Count; i++)
                    //await attrs.ForEach(async c =>
                    {
                        //var argAttr = c.Value;
                        var argAttr = attrs[i];

                        //if (argAttr.CollectionItem) { continue; }

                        result = await GetProcesResult(argAttr, item.HasIArg ? iArgIn : value, token2);

                        if (1 > result.State)
                        {
                            logType = Logger.Type.Error;
                            returnValue = result;
                            error = meta.HasIResult ? Help.GetReturnValueIResult(returnValue, meta) : Help.GetReturnValue(result, meta);
                            //return true; //break
                            break;
                        }

                        if (result.HasDataResult)
                        {
                            if (!item.HasIArg)
                            {
                                argsObj[item.Position] = result.Data;
                            }
                            else
                            {
                                if (i < attrs.Count - 1)
                                //if (NodeState.DAT == c.Next?.State)
                                {
                                    iArgIn = result.Data;
                                }
                                else
                                {
                                    iArgs[item.Position].Out = result.Data;
                                    if (item.HasCast)
                                    {
                                        argsObj[item.Position] = result.Data;
                                    }
                                }
                            }
                        }

                        //return false;
                    }

                    if (!Equals(null, error))
                    {
                        return error;
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

                    if (item.HasLower)// || item.HasCollectionAttr
                    {
                        #region Collection
                        /*
                        if (item.HasCollection)
                        {
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

                                    var collectioTask = ArgsResultCollection(meta, item, attrs, v2, command.Key, command.OnlyName, c, (object)key).ContinueWith(c2 =>
                                    {
                                        if (null != c2.Exception)
                                        {
                                            var ex = c2.Exception.ExceptionWrite();
                                            logType = Logger.Type.Exception;
                                            returnValue = ResultFactory.ResultCreate(meta, 0, System.Convert.ToString(ex));
                                            o.Stop();
                                            return;
                                        }

                                        var result2 = c2.Result;

                                        if (1 > result2.State)
                                        {
                                            logType = Logger.Type.Error;
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

                                    var collectioTask2 = ArgsResult(meta, command.Key, item.Children, command.OnlyName, v2, c).ContinueWith(c3 =>
                                    {
                                        if (null != c3.Exception)
                                        {
                                            var ex = c3.Exception.ExceptionWrite();
                                            logType = Logger.Type.Exception;
                                            returnValue = ResultFactory.ResultCreate(meta, 0, System.Convert.ToString(ex));
                                            o.Stop();
                                            return;
                                        }

                                        var result3 = c3.Result;

                                        if (1 > result3.State)
                                        {
                                            logType = Logger.Type.Error;
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

                            if (!Equals(null, returnValue))
                            {
                                return meta.HasIResult ? Help.GetReturnValueIResult(returnValue, meta) : Help.GetReturnValue(returnValue, meta);
                            }
                        }
                        else
                        {
                            
                        }
                        */
                        #endregion

                        result = await ArgsResult(meta, command.Key, item.Children, currentValue, token2);
                        //if (null != result)
                        if (1 > result.State)
                        {
                            logType = Logger.Type.Error;
                            returnValue = result;
                            return meta.HasIResult ? Help.GetReturnValueIResult(returnValue, meta) : Help.GetReturnValue(result, meta);
                        }

                        if (item.HasIArg && !Equals(null, result))
                        {
                            if (result.Data is ArgResult && result.Data.isUpdate)
                            {
                                iArgs[item.Position].Out = currentValue;
                                if (item.HasCast)
                                {
                                    argsObj[item.Position] = currentValue;
                                }
                            }
                        }
                    }

                    if (item.HasToken)
                    {
                        token2 = currentValue;
                    }
                }

                //===============================//
                //watch.Restart();
                ////..CallBeforeMethod..//
                //if (null != this.Configer.CallBeforeMethod)
                //{
                //    await this.Configer.CallBeforeMethod(new Bind.MethodBefore { Meta = meta, Args = meta.Args.ToDictionary(c => c.Name, c => new Bind.MethodArgs { Name = c.Name, Value = c.HasIArg ? iArgs[c.Position] : argsObj[c.Position], HasIArg = c.HasIArg, Type = c.Type, OutType = c.IArgOutType, InType = c.IArgInType }), Cancel = false });
                //}

                var returnValue2 = call();

                if (meta.HasAsync)
                {
                    var task = returnValue2 as System.Threading.Tasks.Task;
                    if (null != task?.Exception)
                    {
                        throw task?.Exception;
                    }
                }

                //result
                if (!meta.HasReturn && !meta.HasAsync)
                {
                    //log
                    returnValue2 = Help.GetReturnValue(ResultFactory.ResultCreate(meta), meta);
                }

                //..CallAfterMethod..//
                if (null != configer.CallAfterMethod)
                {
                    await configer.CallAfterMethod(new Configer.MethodAfter { Meta = meta, Args = meta.Args.ToDictionary(c => c.Name, c => new Configer.MethodArgs { Name = c.Name, Value = c.HasCast ? iArgs[c.Position].In : c.HasIArg ? iArgs[c.Position] : argsObj[c.Position], HasIArg = c.HasIArg && !c.HasCast, Type = c.HasCast ? c.LastType : c.Type, OutType = c.IArgOutType, InType = c.IArgInType }), Result = returnValue2 });
                }

                if (!meta.HasReturn && meta.HasAsync)
                {
                    await (returnValue2 as System.Threading.Tasks.Task);

                    returnValue2 = Help.GetReturnValue(ResultFactory.ResultCreate(meta), meta);
                }

                returnValue = returnValue2;

                return returnValue2;
            }
            catch (System.Exception ex)
            {
                ex = ex.ExceptionWrite();
                logType = Logger.Type.Exception;

                //log
                returnValue = ResultFactory.ResultCreate(meta, 0, System.Convert.ToString(ex));

                //result
                //returnValue2 = meta.HasIResult ? Bind.GetReturnValueIResult(returnValue, meta) : Bind.GetReturnValue(returnValue, meta);

                //if (!meta.HasAsync)
                //{
                //    invocation.ReturnValue = result;
                //}
                //else
                //{
                //    invocation.ReturnValue = System.Threading.Tasks.Task.FromResult(result);
                //}

                //invocation.ReturnValue = !meta.HasAsync ? Bind.GetReturnValue(0, System.Convert.ToString(ex), meta, ResultType) : System.Threading.Tasks.Task.FromException(ex);
                return meta.HasIResult ? Help.GetReturnValueIResult(returnValue, meta) : Help.GetReturnValue(returnValue, meta);// meta.HasReturn ? invocation.ReturnValue : default;
            }
            finally
            {
                Finally(configer.Logger, command, meta, returnValue, logType, iArgs, argsObj, methodName, watch, token, dtt);
            }
        }

        /// <summary>
        /// Finally
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="command"></param>
        /// <param name="meta"></param>
        /// <param name="returnValue"></param>
        /// <param name="logType"></param>
        /// <param name="iArgs"></param>
        /// <param name="argsObj"></param>
        /// <param name="methodName"></param>
        /// <param name="watch"></param>
        /// <param name="token"></param>
        /// <param name="dtt"></param>
        public async static void Finally(Logger logger, CommandAttribute command, MetaData meta, dynamic returnValue, Logger.Type logType, System.Collections.Generic.Dictionary<int, IArg> iArgs, object[] argsObj, string methodName, System.Diagnostics.Stopwatch watch, dynamic token, System.DateTimeOffset dtt)
        {
            if (meta.HasAsync)
            {
                if (returnValue is System.Threading.Tasks.Task task)
                {
                    try
                    {
                        await task;
                        returnValue = returnValue.Result;
                    }
                    catch (System.Exception ex)
                    {
                        logType = Logger.Type.Exception;
                        returnValue = ResultFactory.ResultCreate(meta, 0, System.Convert.ToString(ex.ExceptionWrite()));
                    }
                }
            }

            if (!object.Equals(null, returnValue) && typeof(IResult).IsAssignableFrom(returnValue.GetType()))
            {
                var result = returnValue as IResult;
                if (0 > result.State)
                {
                    logType = Logger.Type.Error;
                }
            }

            //var argsObjLog = meta.Args.Select(c => new ArgsLog { name = c.Name, value = c.HasIArg ? iArgs[c.Position] : argsObj[c.Position], logger = c.Group[command.Key].Logger, iArgInLogger = c.Group[command.Key].IArgInLogger, hasIArg = c.HasIArg }).ToList();

            //var logObjs = LoggerSet(logType, meta.MetaLogger[command.Key], argsObjLog, argsObjLog.ToDictionary(c => c.name, c => c.hasIArg), out bool canWrite, out bool canResult);

            var argsObjLog = new System.Collections.Generic.List<Logger.ArgsLog>(meta.Args.Count);
            //var argsObjLogHasIArg = new System.Collections.Generic.Dictionary<string, bool>(meta.Args.Count);
            foreach (var c in meta.Args)
            {
                if (!c.Group.TryGetValue(command.Key, out ArgGroup argGroup))
                {
                    continue;
                }

                argsObjLog.Add(new Logger.ArgsLog(c.Name, c.HasIArg ? iArgs[c.Position] : argsObj[c.Position], argGroup.Logger, c.Group[command.Key].IArgInLogger, c.HasIArg));
                //argsObjLogHasIArg.Add(c.Name, c.HasIArg);
            }

            if (!meta.MetaLogger.TryGetValue(command.Key, out MetaLogger metaLogger))
            {
                watch.Stop();
                return;
            }

            var logObjs = Logger.LoggerSet(logType, metaLogger, argsObjLog, out bool canWrite, out bool canResult);//, logger?.ValueType

            watch.Stop();

            //if (null == logger) { return; }

            if (canWrite)
            {
                var data = new Logger.LoggerData(dtt, token, logType, logObjs, canResult ? returnValue : null, Help.Scale(watch.Elapsed.TotalSeconds, 3), methodName, command.Group);

                if (null == logger)
                {
                    Help.Console(data.TryJsonSerialize());
                    return;
                }

                if (null != logger.call)
                {
                    System.Threading.Tasks.Task.Run(() => logger.call(data).AsTask().ContinueWith(c => c.Exception?.Console()));
                }
                else if (null != logger.loggerQueue)
                {
                    //if (logger.MaxCapacity <= logger.loggerQueue.queue.Count)
                    //{
                    //    return;
                    //}

                    if (!logger.loggerQueue.queue.TryAdd(data))
                    {
                        Help.Console(data.TryJsonSerialize());
                    }
                }
                else
                {
                    Help.Console(data.TryJsonSerialize());
                    return;
                }
            }
        }

        /// <summary>
        /// GetProcesResult
        /// </summary>
        /// <param name="argAttr"></param>
        /// <param name="value"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async static System.Threading.Tasks.ValueTask<IResult> GetProcesResult(ArgumentAttribute argAttr, dynamic value, dynamic token)//, int collectionIndex = -1, dynamic dictKey = null
        {
            switch (argAttr.ArgMeta.Proces?.Mode)
            {
                case Proces.ProcesMode.Proces:
                    return await argAttr.Proces(value);
                case Proces.ProcesMode.ProcesGeneric:
                    {
                        dynamic result = argAttr.ArgMeta.Proces.Call(argAttr, new object[] { value });
                        return await result;
                    }
                case Proces.ProcesMode.ProcesGenericToken:
                    {
                        dynamic result = argAttr.ArgMeta.Proces.Call(argAttr, new object[] { token, value });
                        return await result;
                    }
                //case Proces.ProcesMode.ProcesCollection:
                //    return await argAttr.Proces(value, collectionIndex, dictKey);
                //case Proces.ProcesMode.ProcesCollectionGeneric:
                //    {
                //        dynamic result = argAttr.ArgMeta.Proces.Call(argAttr, new object[] { value, collectionIndex, dictKey });
                //        return await result;
                //    }
                default: return await argAttr.Proces(value);
            }
        }

        /// <summary>
        /// ArgsResult
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="group"></param>
        /// <param name="args"></param>
        /// <param name="currentValue"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async static System.Threading.Tasks.ValueTask<IResult> ArgsResult(MetaData meta, string group, System.Collections.Generic.IList<Args> args, object currentValue, dynamic token)//, int collectionIndex = -1, dynamic dictKey = null
        {
            bool isUpdate = false;

            foreach (var item in args)
            {
                IResult result = null;

                var memberValue = null != currentValue ? item.Accessor.TryGetter(currentValue) : null;
                //========================================//

                var iArgIn = item.HasIArg ? null != memberValue ? ((IArg)memberValue).In : null : null;

                var attrs = item.Group[group].Attrs;

                dynamic error = null;
                for (int i = 0; i < attrs.Count; i++)
                //await attrs.ForEach(async c =>
                {
                    //var argAttr = c.Value;
                    var argAttr = attrs[i];

                    //if (argAttr.CollectionItem) { continue; }

                    result = await GetProcesResult(argAttr, item.HasIArg ? iArgIn : memberValue, token);//, collectionIndex, dictKey

                    if (1 > result.State)
                    {
                        error = result;
                        //return true;
                        break;
                    }

                    if (result.HasDataResult)
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
                            //if (NodeState.DAT == c.Next?.State)
                            if (i < attrs.Count - 1)
                            {
                                iArgIn = result.Data;
                            }
                            else if (null != memberValue)
                            {
                                ((IArg)memberValue).Out = result.Data;
                                item.Accessor.Setter(currentValue, memberValue);
                            }
                        }
                    }

                    //return false;
                }

                if (!Equals(null, error))
                {
                    return error;
                }

                object currentValue2 = item.HasIArg ?
                        ((null != result && result.HasData) ? result.Data : (null != memberValue ? ((IArg)memberValue).Out : null)) :
                        ((null != result && result.HasData) ? result.Data : memberValue);

                if (null == currentValue2)
                {
                    continue;
                }

                //if (0 < item.ArgAttrChild.Count && null != currentValue2)
                //if ((item.HasLower || item.HasCollectionAttr) && null != currentValue2)
                if (item.HasLower && null != currentValue2)
                {
                    #region Collection
                    /*
                    dynamic result2 = null;

                    if (item.HasCollection)
                    {
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

                        if (!Equals(null, result3))
                        {
                            return result3;
                        }
                    }
                    else
                    {
                        
                    }
                    */
                    #endregion

                    var result2 = await ArgsResult(meta, group, item.Children, currentValue2, token);

                    if (1 > result2.State)
                    {
                        return result2;
                    }

                    if (!object.Equals(null, result2) && result2.Data is ArgResult && result2.Data.isUpdate)
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

            return ResultFactory.ResultCreate(meta.ResultTypeDefinition, new ArgResult(isUpdate, currentValue));
        }

        /*
        public async static System.Threading.Tasks.Task<IResult> ArgsResultCollection(MetaData meta, Args item, ReadOnlyCollection<ArgumentAttribute> attrs, object currentValue, string group, string methodName)//, int collectionIndex, dynamic dictKey = null
        {
            bool isUpdate = false;

            var iArg = item.HasCollectionIArg && null != currentValue ? (IArg)currentValue : null;
            var iArgIn = item.HasCollectionIArg ? iArg?.In : null;

            dynamic error = null;
            //await attrs.ForEach(async c =>
            for (int i = 0; i < attrs.Count; i++)
            {
                //var argAttr = c.Value;
                var argAttr = attrs[i];

                //if (!argAttr.CollectionItem) { continue; }

                var result = await GetProcesResult(argAttr, item.HasCollectionIArg ? iArgIn : currentValue);//, collectionIndex, dictKey

                if (1 > result.State)
                {
                    error = result;
                    break;
                }

                if (result.HasData)
                {
                    if (!item.HasCollectionIArg)
                    {
                        currentValue = result.Data;
                        if (!isUpdate) { isUpdate = !isUpdate; }
                    }
                    else
                    {
                        //if (NodeState.DAT == c.Next?.State)
                        if (i < attrs.Count - 1)
                        {
                            iArgIn = result.Data;
                        }
                        else if (null != currentValue)
                        {
                            ((IArg)currentValue).Out = result.Data;
                        }
                    }
                }

                //return false;
            }

            if (!Equals(null, error))
            {
                return error;
            }

            //==========HasLower==========//
            if (item.HasLower)
            {
                var result2 = await ArgsResult(meta, group, item.Children, methodName, currentValue);

                if (1 > result2.State)
                {
                    return result2;
                }

                return ResultFactory.ResultCreate(meta.ResultTypeDefinition, data: result2.Data);
            }

            return ResultFactory.ResultCreate(meta.ResultTypeDefinition, new ArgResult(isUpdate, currentValue));
        }
        */
    }
}