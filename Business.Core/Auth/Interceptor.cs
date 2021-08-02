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
    using System.Linq;
    using Utils;

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
        /// <param name="businessType"></param>
        /// <param name="constructorArguments"></param>
        /// <param name="constructorArgumentsFunc"></param>
        /// <param name="ignoreMethods"></param>
        /// <returns></returns>
        object Create(System.Type businessType, object[] constructorArguments = null, System.Func<System.Type, object> constructorArgumentsFunc = null, System.Collections.Generic.IEnumerable<System.Reflection.MethodInfo> ignoreMethods = null);

        /// <summary>
        /// Intercept
        /// </summary>
        /// <param name="configer"></param>
        /// <param name="method"></param>
        /// <param name="arguments"></param>
        /// <param name="call"></param>
        /// <param name="group"></param>
        /// <param name="multipleParameterDeserialize"></param>
        /// <returns></returns>
        System.Threading.Tasks.ValueTask<dynamic> Intercept(Configer configer, string method, object[] arguments, System.Func<dynamic> call, string group = null, bool multipleParameterDeserialize = false);
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
        /// <param name="constructorArgumentsFunc"></param>
        /// <param name="ignoreMethods"></param>
        /// <returns></returns>
        public object Create(System.Type businessType, object[] constructorArguments = null, System.Func<System.Type, object> constructorArgumentsFunc = null, System.Collections.Generic.IEnumerable<System.Reflection.MethodInfo> ignoreMethods = null)
        {
            var proxy = new Castle.DynamicProxy.ProxyGenerator();

            try
            {
                var parameters = businessType.GetConstructors()?.FirstOrDefault()?.GetParameters();

                var args = new object[parameters?.Length ?? 0];

                if (0 < parameters?.Length)
                {
                    var arguments = constructorArguments?.ToDictionary(c => c.GetType(), c => c);

                    var i = 0;
                    foreach (var parameter in parameters)
                    {
                        var arg = parameter.HasDefaultValue ? parameter.DefaultValue : null;

                        object value = null;

                        if (0 < arguments?.Count && (arguments?.TryGetValue(parameter.ParameterType, out value) ?? false))
                        {
                            arg = value;
                        }
                        else
                        {
                            value = constructorArgumentsFunc?.Invoke(parameter.ParameterType);

                            if (!Equals(null, value))
                            {
                                arg = value;
                            }
                        }

                        args[i++] = arg;
                    }
                }

                var instance = proxy.CreateClassProxy(businessType, new Castle.DynamicProxy.ProxyGenerationOptions(new BusinessAllMethodsHook(ignoreMethods)), args, this);

                return instance;
            }
            catch (System.Exception ex)
            {
                var inner = ex.GetBase();

                inner.GlobalLog();

                throw inner;
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
        /// <param name="multipleParameterDeserialize"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.ValueTask<dynamic> Intercept(Configer configer, string method, object[] arguments, System.Func<dynamic> call, string group = null, bool multipleParameterDeserialize = false) => await InterceptorExtensions.Intercept(configer, method, arguments, call, group, multipleParameterDeserialize);
    }
}

namespace Business.Core.Utils
{
    using Annotations;
    using Auth;
    using Meta;
    using Result;
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
        /// <param name="multipleParameterDeserialize"></param>
        /// <returns></returns>
        public static async System.Threading.Tasks.ValueTask<dynamic> Intercept(Configer configer, string method, object[] arguments, System.Func<dynamic> call, string group, bool multipleParameterDeserialize)
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
                        logType = Logger.Type.Error;
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
                    //var value = argsObj[item.Position];
                    var value = item.HasIArg ? iArgs[item.Position].In : argsObj[item.Position];
                    var attrs = item.Group[command.Key].Attrs;

                    dynamic error = null;

                    for (int i = 0; i < attrs.Count; i++)
                    {
                        var argAttr = attrs[i];

                        if (!(multipleParameterDeserialize && argAttr.ArgMeta.Deserialize))
                        {
                            result = await argAttr.GetProcesResult(value, token2);

                            if (1 > result.State)
                            {
                                logType = Logger.Type.Error;
                                returnValue = result;
                                error = meta.HasIResult ? Help.GetReturnValueIResult(result, meta) : Help.GetReturnValue(result, meta);
                                break;
                            }

                            if (result.HasDataResult)
                            {
                                value = result.Data;
                            }
                        }

                        if (i == attrs.Count - 1)
                        {
                            if (!item.HasIArg)
                            {
                                argsObj[item.Position] = value;
                            }
                            else
                            {
                                iArgs[item.Position].Out = value;
                                if (item.HasCast)
                                {
                                    argsObj[item.Position] = value;
                                }
                            }
                        }
                    }

                    if (logType == Logger.Type.Error)
                    {
                        return error;
                    }

                    //========================================//

                    object currentValue = value;//item.HasIArg ? iArgs[item.Position].Out : value;

                    //object currentValue = item.HasIArg ?
                    //    ((null != result && result.HasData) ? result.Data : iArgs[item.Position].Out) :
                    //    ((null != result && result.HasData) ? result.Data : value);

                    //========================================//
                    //item.HasIArg && 
                    if (null == currentValue)
                    {
                        continue;
                    }

                    //var isUpdate = false;

                    //!item.HasCollection !!!Checking arrays is not supported
                    if (item.HasLower && !item.HasCollection)// || item.HasCollectionAttr
                    {
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

                var returnValue2 = call();

                //checked async exception
                if (meta.HasAsync)
                {
                    var task = meta.HasValueTask ? returnValue2.AsTask() as System.Threading.Tasks.Task : returnValue2 as System.Threading.Tasks.Task;
                    if (null != task.Exception)
                    {
                        throw task.Exception;
                    }
                }

                returnValue = returnValue2;

                return returnValue2;
            }
            catch (System.Exception ex)
            {
                logType = Logger.Type.Exception;

                //log
                returnValue = ResultFactory.ResultCreate(meta, 0, System.Convert.ToString(ex.GetBase()));

                return meta.HasIResult ? Help.GetReturnValueIResult(returnValue, meta) : Help.GetReturnValue(returnValue, meta);
            }
            finally
            {
                Finally(configer.Logger, command, meta, returnValue, logType, iArgs, argsObj, methodName, watch, token, dtt, configer.CallAfterMethod);
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
        /// <param name="callAfterMethod"></param>
        public async static void Finally(Logger logger, CommandAttribute command, MetaData meta, dynamic returnValue, Logger.Type logType, System.Collections.Generic.Dictionary<int, IArg> iArgs, object[] argsObj, string methodName, System.Diagnostics.Stopwatch watch, dynamic token, System.DateTimeOffset dtt, System.Func<Configer.MethodAfter, System.Threading.Tasks.Task> callAfterMethod)
        {
            if (meta.HasAsync && Logger.Type.Record == logType)
            {
                try
                {
                    await returnValue;

                    if (meta.HasReturn)
                    {
                        returnValue = returnValue.Result;
                    }
                    else
                    {
                        returnValue = null;
                    }
                }
                catch (System.Exception ex)
                {
                    logType = Logger.Type.Exception;
                    returnValue = ResultFactory.ResultCreate(meta, 0, System.Convert.ToString(ex.GetBase()));
                }
            }

            //..CallAfterMethod..//
            if (null != callAfterMethod)
            {
                await callAfterMethod(new Configer.MethodAfter { Meta = meta, Args = meta.Args.ToDictionary(c => c.Name, c => new Configer.MethodArgs { Name = c.Name, Value = c.HasCast ? iArgs[c.Position].In : c.HasIArg ? iArgs[c.Position] : argsObj[c.Position], HasIArg = c.HasIArg && !c.HasCast, Type = c.HasCast ? c.LastType : c.Type, OutType = c.IArgOutType, InType = c.IArgInType }), Result = returnValue });
            }

            if (null == logger)
            {
                watch.Stop();
                return;
            }

            if (!meta.MetaLogger.TryGetValue(command.Key, out MetaLogger metaLogger))
            {
                watch.Stop();
                return;
            }

            var argsObjLog = new System.Collections.Generic.List<Logger.ArgsLog>(meta.Args.Count);
            foreach (var c in meta.Args)
            {
                if (!c.Group.TryGetValue(command.Key, out ArgGroup argGroup))
                {
                    continue;
                }

                argsObjLog.Add(new Logger.ArgsLog(c.Name, c.HasIArg ? iArgs[c.Position] : argsObj[c.Position], argGroup.Logger, c.Group[command.Key].IArgInLogger, c.HasIArg));
            }

            if (!object.Equals(null, returnValue) && typeof(IResult).IsAssignableFrom(returnValue.GetType()))
            {
                var result = returnValue as IResult;
                if (0 > result.State)
                {
                    logType = Logger.Type.Error;
                }
            }

            var logObjs = Logger.LoggerSet(logType, metaLogger, argsObjLog, out bool canWrite, out bool canResult);

            watch.Stop();

            if (canWrite)
            {
                var data = new Logger.LoggerData(dtt, token, logType, logObjs, canResult ? returnValue : null, Help.Scale(watch.Elapsed.TotalSeconds, 3), methodName, command.Group);

                if (null != logger.call)
                {
                    System.Threading.Tasks.Task.Run(() => logger.call(data).AsTask().ContinueWith(c => c.Exception?.GlobalLog()));
                }
                else if (null != logger.loggerQueue)
                {
                    if (!logger.loggerQueue.queue.TryAdd(data))
                    {
                        data.TryJsonSerialize().GlobalLog(Logger.Type.Error);
                    }
                }
            }
        }
        /*
        /// <summary>
        /// GetProcesResult
        /// </summary>
        /// <param name="argAttr"></param>
        /// <param name="value"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async static System.Threading.Tasks.ValueTask<IResult> GetProcesResult(ArgumentAttribute argAttr, dynamic value, dynamic token)
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
                default: return await argAttr.Proces(value);
            }
        }
        */
        /// <summary>
        /// ArgsResult
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="group"></param>
        /// <param name="args"></param>
        /// <param name="currentValue"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async static System.Threading.Tasks.ValueTask<IResult> ArgsResult(MetaData meta, string group, System.Collections.Generic.IList<Args> args, object currentValue, dynamic token)
        {
            bool isUpdate = false;

            foreach (var item in args)
            {
                //var iArgs = Help.GetIArgs(item.IArgs, currentValue);

                IResult result = null;

                var memberValue = null != currentValue ? item.Accessor.TryGetter(currentValue) : null;
                //========================================//

                var iArg = item.HasIArg ? Help.GetIArgs(item, memberValue) : null;
                memberValue = item.HasIArg ? (null != memberValue ? iArg.In : null) : memberValue;

                //var iArgIn = item.HasIArg ? (null != memberValue ? ((IArg)memberValue).In : null) : null;

                var attrs = item.Group[group].Attrs;

                if (item.HasIArg && !item.HasCast && 0 == attrs.Count)
                {
                    iArg.Out = iArg.In;
                }

                dynamic error = null;
                for (int i = 0; i < attrs.Count; i++)
                {
                    var argAttr = attrs[i];

                    result = await argAttr.GetProcesResult(memberValue, token);

                    if (1 > result.State)
                    {
                        error = result;
                        break;
                    }

                    if (result.HasDataResult)
                    {
                        memberValue = result.Data;
                    }

                    if (i == attrs.Count - 1)
                    {
                        if (!item.HasIArg)
                        {
                            if (null != currentValue)
                            {
                                item.Accessor.Setter(currentValue, memberValue);
                            }
                            if (!isUpdate) { isUpdate = !isUpdate; }
                        }
                        else
                        {
                            if (result.HasDataResult)
                            {
                                iArg.Out = result.Data;

                                item.Accessor.Setter(currentValue, item.HasCast ? memberValue : iArg);
                            }
                        }
                    }
                }

                if (!Equals(null, error))
                {
                    return error;
                }

                object currentValue2 = memberValue;

                if (null == currentValue2)
                {
                    continue;
                }

                if (item.HasLower && !item.HasCollection)// && null != currentValue2
                {
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
    }
}