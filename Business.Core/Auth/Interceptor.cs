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
        /// Business
        /// </summary>
        object Business { get; set; }

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
    }

    /// <summary>
    /// Interceptor
    /// </summary>
    public class Interceptor : IInterceptor
    {
        /// <summary>
        /// Business
        /// </summary>
        public object Business { get; set; }

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
        public virtual object Create(System.Type businessType, object[] constructorArguments = null, System.Func<System.Type, object> constructorArgumentsFunc = null, System.Collections.Generic.IEnumerable<System.Reflection.MethodInfo> ignoreMethods = null)
        {
            try
            {
                var args = Help.GetConstructorParameters(businessType, constructorArguments, constructorArgumentsFunc);

                var instance = System.Activator.CreateInstance(businessType, args);

                return instance;
            }
            catch (System.Exception ex)
            {
                var inner = ex.GetBase();

                inner.GlobalLog();

                throw inner;
            }
        }
    }

    /// <summary>
    /// InterceptorCastle
    /// </summary>
    public class InterceptorCastle : Interceptor, Castle.DynamicProxy.IInterceptor
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
        /// Create
        /// </summary>
        /// <param name="businessType"></param>
        /// <param name="constructorArguments"></param>
        /// <param name="constructorArgumentsFunc"></param>
        /// <param name="ignoreMethods"></param>
        /// <returns></returns>
        public override object Create(System.Type businessType, object[] constructorArguments = null, System.Func<System.Type, object> constructorArgumentsFunc = null, System.Collections.Generic.IEnumerable<System.Reflection.MethodInfo> ignoreMethods = null)
        {
            var proxy = new Castle.DynamicProxy.ProxyGenerator();

            try
            {
                var args = Help.GetConstructorParameters(businessType, constructorArguments, constructorArgumentsFunc);

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
            if (null == this.Configer?.MetaData)
            {
                invocation.ReturnValue = null; return;
            }

            var proceed = invocation.CaptureProceedInfo();

            invocation.ReturnValue = this.Intercept(invocation.Method.Name, invocation.Arguments, arguments =>
            {
                proceed.Invoke();
                return invocation.ReturnValue;
            }).Result;
        }
    }
}

namespace Business.Core.Utils
{
    using Annotations;
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
        /// <param name="interceptor"></param>
        /// <param name="method"></param>
        /// <param name="arguments"></param>
        /// <param name="call"></param>
        /// <param name="group"></param>
        /// <param name="token"></param>
        /// <param name="multipleParameterDeserialize"></param>
        /// <param name="origParameters"></param>
        /// <returns></returns>
        public static async System.Threading.Tasks.ValueTask<dynamic> Intercept(this Auth.IInterceptor interceptor, string method, object[] arguments, System.Func<object[], dynamic> call, string group = null, Auth.IToken token = null, bool multipleParameterDeserialize = false, dynamic origParameters = null)
        {
            var configer = interceptor.Configer;
            var dtt = System.DateTimeOffset.Now;
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            var meta = configer.MetaData[method];
            var methodName = meta.FullName;
            var argsObj = arguments;
            var logType = Logger.Type.Record;
            //==================================//
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
                    var before = new Configer.MethodBefore { Meta = meta, Args = meta.Args.ToDictionary(c => c.Name, c => new Configer.MethodArgs { Name = c.Name, Value = argsObj[c.Position], Type = c.Type }), Cancel = false };

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
                    token = argsObj[tokenArg.Position] as Auth.IToken;
                }
                token2 = token;

                foreach (var item in meta.Args)
                {
                    IResult result = null;
                    var value = argsObj[item.Position];
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
                                argsObj[item.Position] = value;
                            }
                        }
                    }

                    if (logType == Logger.Type.Error)
                    {
                        return error;
                    }

                    //========================================//
                    //item.HasIArg && 
                    if (null == value)
                    {
                        continue;
                    }

                    //!item.HasCollection !!!Checking arrays is not supported
                    if (item.HasLower && !item.HasCollection)// || item.HasCollectionAttr
                    {
                        result = await ArgsResult(meta, command.Key, item.Children, value, token2);

                        if (1 > result.State)
                        {
                            logType = Logger.Type.Error;
                            returnValue = result;
                            return meta.HasIResult ? Help.GetReturnValueIResult(returnValue, meta) : Help.GetReturnValue(result, meta);
                        }

                        if (result.HasData)
                        {
                            value = result.Data;
                        }
                    }

                    if (item.HasToken)
                    {
                        token2 = value;
                    }

                    argsObj[item.Position] = value;
                }

                //===============================//

                var returnValue2 = null == call ? meta.Accessor(interceptor.Business, argsObj) : call(argsObj);

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
                Finally(configer.Logger, command, meta, returnValue, logType, argsObj, methodName, watch, typeof(Auth.IToken).IsAssignableFrom(token2?.GetType()) ? token2 : token, dtt, configer.CallAfterMethod, origParameters);
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
        /// <param name="argsObj"></param>
        /// <param name="methodName"></param>
        /// <param name="watch"></param>
        /// <param name="token"></param>
        /// <param name="dtt"></param>
        /// <param name="callAfterMethod"></param>
        /// <param name="origParameters"></param>
        public async static void Finally(Logger logger, CommandAttribute command, MetaData meta, dynamic returnValue, Logger.Type logType, object[] argsObj, string methodName, System.Diagnostics.Stopwatch watch, Auth.IToken token, System.DateTimeOffset dtt, System.Func<Configer.MethodAfter, System.Threading.Tasks.Task> callAfterMethod, dynamic origParameters = null)
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
                await callAfterMethod(new Configer.MethodAfter { Meta = meta, Args = meta.Args.ToDictionary(c => c.Name, c => new Configer.MethodArgs { Name = c.Name, Value = argsObj[c.Position], Type = c.Type }), Result = returnValue });
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

            if (!object.Equals(null, returnValue) && typeof(IResult).IsAssignableFrom(returnValue.GetType()))
            {
                var result = returnValue as IResult;
                if (0 > result.State)
                {
                    logType = Logger.Type.Error;
                }
            }

            dynamic logObjs = Logger.LoggerSet(meta, command.Key, argsObj, logType, metaLogger, out bool canWrite, out bool canResult, origParameters);

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

        /// <summary>
        /// ArgsResult
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="group"></param>
        /// <param name="args"></param>
        /// <param name="currentValue"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async static System.Threading.Tasks.ValueTask<IResult> ArgsResult(MetaData meta, string group, System.Collections.Generic.IEnumerable<Args> args, object currentValue, dynamic token)
        {
            foreach (var item in args)
            {
                IResult result = null;

                var memberValue = null != currentValue ? item.Accessor.TryGetter(currentValue) : null;
                //========================================//

                var attrs = item.Group[group].Attrs;

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
                }

                if (!Equals(null, error))
                {
                    return error;
                }

                if (null == memberValue)
                {
                    continue;
                }

                if (item.HasLower && !item.HasCollection)// && null != currentValue2
                {
                    var result2 = await ArgsResult(meta, group, item.Children, memberValue, token);

                    if (1 > result2.State)
                    {
                        return result2;
                    }

                    if (result2.HasData)
                    {
                        memberValue = result2.Data;
                    }
                }

                if (null != currentValue)
                {
                    item.Accessor.Setter(currentValue, memberValue);
                }
            }

            return ResultFactory.ResultCreate(meta.ResultTypeDefinition, currentValue);
        }
    }
}