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
    using Result;

    //public interface IInterceptor : Castle.DynamicProxy.IInterceptor, System.IDisposable
    //{
    //    System.Action<Log> WriteLogAsync { get; set; }

    //    System.Collections.Concurrent.ConcurrentDictionary<string, MetaData> MetaData { get; set; }

    //    dynamic Business { get; set; }
    //}

    public interface IInterceptor<Result> : Castle.DynamicProxy.IInterceptor, System.IDisposable
        where Result : IResult, new()
    {
        System.Action<Log> WriteLogAsync { get; set; }

        System.Collections.Concurrent.ConcurrentDictionary<string, MetaData> MetaData { get; set; }

        dynamic Business { get; set; }
    }

    //public sealed class Interceptor : Interceptor<Business.Result.ResultBase<string>> { }

    //public abstract class Interceptor<Result> : IInterceptor where Result : class, IResult, new() { }

    public class Interceptor<Result> : IInterceptor<Result>
        where Result : IResult, new()
    {
        public virtual void Intercept(Castle.DynamicProxy.IInvocation invocation)
        {
            //var ss1 = new System.Diagnostics.StackFrame(3, false).GetMethod().Name.Split('|');
            //var group = 1 < ss1.Length ? ss1[1] : null;
            //if (null != group) { }

            var startTime = new System.Diagnostics.Stopwatch();
            startTime.Start();
            var meta = this.MetaData[invocation.Method.Name];
            var methodName = meta.FullName;
            var argsObj = invocation.Arguments;
            //var argsObjLog = new System.Collections.ArrayList(argsObj.Length);
            var argsObjLog = new System.Collections.Generic.List<System.Tuple<object, MetaLogger>>(argsObj.Length);
            var logType = LogType.Record;
            var metaLogger = meta.MetaLogger;
            //var commandGroup = meta.CommandGroup;
            //==================================//
            var iArgGroup = Bind.GetCommandGroupDefault(invocation.Method.Name);
            var iArgs = Bind.GetIArgs(meta.IArgs, argsObj, iArgGroup);
            if (0 < iArgs.Count)
            {
                var group = iArgs[meta.IArgs[0].Position].Group;
                //if (!System.String.IsNullOrEmpty(group)) { iArgGroup = iArgs[meta.IArgs[0].Position].Group; }
                if (!System.String.IsNullOrEmpty(group)) { iArgGroup = group; }
            }

            var args = meta.ArgAttrs[iArgGroup];
            
            try
            {
                foreach (var item in args.Args)
                {
                    IResult result = null;
                    var value = argsObj[item.Position];
                    var iArgIn = item.HasIArg ? iArgs[item.Position].In : null;
                    argsObjLog.Add(new System.Tuple<object, MetaLogger>(item.HasIArg ? iArgs[item.Position].In : value, item.MetaLogger));
                    //if (null != logAttr)
                    //{
                    //    if ((logAttr.CanValue && (null == item.LogAttr || (null != item.LogAttr && item.LogAttr.CanWrite))) || (null != item.LogAttr && item.LogAttr.CanWrite))
                    //    {
                    //        argsObjLog.Add(item.HasIArg ? iArgs[item.Position].In : value);
                    //    }
                    //}

                    var iArgHasString = !System.Object.Equals(null, iArgIn) && typeof(System.String).Equals(iArgIn.GetType());
                    var trim = false;

                    foreach (var argAttr in item.ArgAttr)
                    {
                        if (item.HasIArg)
                        {
                            trim = (args.CommandAttr.TrimChar || argAttr.TrimChar) && iArgHasString;

                            if (trim)
                            {
                                iArgIn = System.Convert.ToString(iArgIn).Trim();
                                if (!System.Object.Equals(iArgs[item.Position].In, iArgIn))
                                {
                                    iArgs[item.Position].In = iArgIn;
                                }
                            }

                            result = argAttr.Proces(iArgIn, item.IArgOutType, methodName, item.Name, this.Business);
                        }
                        else
                        {
                            trim = item.HasString && (args.CommandAttr.TrimChar || argAttr.TrimChar) && null != value;

                            if (trim)
                            {
                                value = System.Convert.ToString(value).Trim();
                                argsObj[item.Position] = value;
                            }

                            result = argAttr.Proces(value, item.Type, methodName, item.Name, this.Business);
                        }

                        if (1 > result.State)
                        {
                            invocation.ReturnValue = Bind.GetReturnValue<Result>(result.State, result.Message, meta); logType = LogType.Error; return;
                        }

                        //if (!item.HasIArg && trim) { argsObj[item.Position] = value; }

                        //========================================//

                        if (result.HasData)
                        {
                            if (!item.HasIArg)
                            {
                                argsObj[item.Position] = result.Data;
                            }
                            else
                            {
                                //item.IArgValueSet(value, result.Data);
                                iArgs[item.Position].Out = result.Data;
                            }
                        }
                        //else if (!item.HasIArg && trim)
                        //{
                        //    argsObj[item.Position] = value;
                        //}

                        //========================================//

                        //if (item.HasIArg && result.HasData)
                        //{
                        //    item.IArgValueSet(value, result.Data);
                        //}
                    }

                    //========================================//
                    object currentValue = item.HasIArg ? (null != result && result.HasData) ? result.Data : iArgs[item.Position].Out : (null != result && result.HasData) ? result.Data : value;
                    //========================================//
                    //item.HasIArg && 
                    if (0 == item.ArgAttrChild.Count || null == currentValue)
                    {
                        continue;
                    }
                    //if (0 < item.ArgAttrChild.Count && null == currentValue)
                    //{
                    //    continue;
                    //}

                    var trimIArg = false;

                    foreach (var argAttrChild in item.ArgAttrChild)
                    {
                        //trim = argAttrChild.HasString && (args.CommandAttr.TrimChar || argAttrChild.Trim);

                        if (argAttrChild.Trim || 0 < argAttrChild.ArgAttr.Count)
                        {
                            var memberValue = argAttrChild.MemberAccessorGet(currentValue);
                            if (argAttrChild.Trim && null != memberValue)
                            {
                                memberValue = System.Convert.ToString(memberValue).Trim();
                                argAttrChild.MemberAccessorSet(currentValue, memberValue);
                                if (!trimIArg) { trimIArg = !trimIArg; }
                            }

                            //========================================//

                            foreach (var argAttr in argAttrChild.ArgAttr)
                            {
                                result = argAttr.Proces(memberValue, argAttrChild.Type, methodName, argAttrChild.FullName, this.Business);
                                if (0 >= result.State) { invocation.ReturnValue = Bind.GetReturnValue<Result>(result.State, result.Message, meta); logType = LogType.Error; return; }
                                if (result.HasData)
                                {
                                    if (!argAttrChild.HasIArg)
                                    {
                                        argAttrChild.MemberAccessorSet(currentValue, result.Data);
                                        if (!trimIArg) { trimIArg = !trimIArg; }
                                    }
                                }
                            }
                        }

                        //trim = argAttrChild.HasString && (args.CommandAttr.TrimChar || argAttrChild.Trim) && null != memberValue;

                        //if (trim)
                        //{
                        //    memberValue = System.Convert.ToString(memberValue).Trim();
                        //    argAttrChild.MemberAccessorSet(currentValue, memberValue);
                        //    if (!trimIArg) { trimIArg = !trimIArg; }
                        //}
                    }

                    //========================================//

                    if (item.HasIArg && trimIArg)
                    {
                        iArgs[item.Position].Out = currentValue;
                        //item.IArgValueSet(value, currentValue);
                    }
                }

                //===============================//
                startTime.Restart();
                invocation.Proceed();
            }
            catch (System.Exception ex)
            {
                invocation.ReturnValue = Bind.GetReturnValue<Result>(0, System.Convert.ToString(ex), meta); logType = LogType.Exception;
            }
            finally
            {
                startTime.Stop();
                var total = Extensions.Help.Scale(startTime.Elapsed.TotalSeconds, 3);

                if (null != this.WriteLogAsync)
                {
                    //logType == BusinessLogType.Exception ||
                    //if (null != logAttr && !logAttr.NotRecord && !logAttr.NotValue)
                    //{
                    //    if (0 < iArgs.Count)
                    //    {
                    //        foreach (var item in iArgs) { argsObj[item.Key] = item.Value.Log; }
                    //    }
                    //}

                    //args.ArgAttr[0].
                    // type.GetAttributes<KnownTypeAttribute>().Select(a => a.Type).Distinct().ForEach(t => AddKnownTypeHierarchy(t));  

                    //if (null != metaLogger)
                    //{
                    //    var canWrite = false;

                    //    switch (logType)
                    //    {
                    //        case LogType.Exception:
                    //            if (metaLogger.CanWriteException) { canWrite = true; } break;
                    //        case LogType.Record:
                    //            if (metaLogger.CanWrite) { canWrite = true; } break;
                    //    }

                    //    if (canWrite)
                    //    {
                    //        this.WriteLogAsync.BeginInvoke(new Log { Type = logType, Value = 0 == argsObjLog.Count ? null : argsObjLog.ToArray(), Result = metaLogger.CanResult ? invocation.ReturnValue : null, Time = total, Member = methodName }, null, null);
                    //    }
                    //}

                    if (meta.HasIResult && 0 > ((IResult)invocation.ReturnValue).State)
                    {
                        logType = LogType.Error;
                    }

                    bool canWrite, canResult;
                    var logObjs = Logger(logType, metaLogger, argsObjLog, out canWrite, out canResult);

                    if (canWrite)
                    {
                        this.WriteLogAsync.BeginInvoke(new Log { Type = logType, Value = 0 == logObjs.Count ? null : logObjs.ToArray(), Result = canResult ? invocation.ReturnValue : null, Time = total, Member = methodName, Group = args.CommandAttr.Group }, null, null);
                    }

                    //if (logType == LogType.Exception)
                    //{
                    //    this.WriteLogAsync.BeginInvoke(new Log { Type = logType, Value = argsObj, Result = invocation.ReturnValue, Time = total, Member = methodName }, null, null);
                    //}
                    //else if (null != logAttr && logAttr.CanWrite)
                    //{
                    //    if (logAttr.CanValue)
                    //    {
                    //        //if (0 < iArgs.Count)
                    //        //{
                    //        //    foreach (var item in iArgs)
                    //        //    {
                    //        //        switch (item.Value.Log)
                    //        //        {
                    //        //            case Attributes.LogMode.No:
                    //        //                argsObj[item.Key] = null;
                    //        //                break;
                    //        //            case Attributes.LogMode.In:
                    //        //                argsObj[item.Key] = item.Value.In;
                    //        //                break;
                    //        //            case Attributes.LogMode.Out:
                    //        //                argsObj[item.Key] = item.Value.Out;
                    //        //                break;
                    //        //            case Attributes.LogMode.All: break;
                    //        //        }
                    //        //    }
                    //        //}
                    //    }

                    //    this.WriteLogAsync.BeginInvoke(new Log { Type = logType, Value = !logAttr.CanValue ? argsObj : null, Result = !logAttr.CanResult ? invocation.ReturnValue : null, Time = total, Member = methodName }, null, null);
                    //}
                }
            }
        }

        static void Logger(Attributes.LoggerAttribute logAttr, Attributes.LoggerAttribute argLogAttr, System.Collections.ArrayList logObjs, object value)
        {
            switch (logAttr.CanValue)
            {
                case LoggerAttribute.ValueMode.All:
                    logObjs.Add(value);
                    break;
                case LoggerAttribute.ValueMode.Select:
                    if (null != argLogAttr && argLogAttr.CanWrite)
                    {
                        logObjs.Add(value);
                    }
                    break;
                default: break;
            }
        }

        static System.Collections.ArrayList Logger(LogType logType, MetaLogger metaLogger, System.Collections.Generic.List<System.Tuple<object, MetaLogger>> argsObjLog, out bool canWrite, out bool canResult)
        {
            canWrite = canResult = false;
            var logObjs = new System.Collections.ArrayList(argsObjLog.Count);

            switch (logType)
            {
                case LogType.Record:
                    if (metaLogger.RecordAttr.CanWrite)
                    {
                        foreach (var log in argsObjLog)
                        {
                            Logger(metaLogger.RecordAttr, log.Item2.RecordAttr, logObjs, log.Item1);
                        }
                        canWrite = true;

                        if (metaLogger.RecordAttr.CanResult)
                        {
                            canResult = true;
                        }
                    }
                    break;
                case LogType.Error:
                    if (metaLogger.ErrorAttr.CanWrite)
                    {
                        foreach (var log in argsObjLog)
                        {
                            Logger(metaLogger.ErrorAttr, log.Item2.ErrorAttr, logObjs, log.Item1);
                        }
                        canWrite = true;

                        if (metaLogger.ErrorAttr.CanResult)
                        {
                            canResult = true;
                        }
                    }
                    break;
                case LogType.Exception:
                    if (metaLogger.ExceptionAttr.CanWrite)
                    {
                        foreach (var log in argsObjLog)
                        {
                            Logger(metaLogger.ExceptionAttr, log.Item2.ExceptionAttr, logObjs, log.Item1);
                        }
                        canWrite = true;

                        if (metaLogger.ExceptionAttr.CanResult)
                        {
                            canResult = true;
                        }
                    }
                    break;
            }

            return logObjs;
        }

        /*
        public virtual void Intercept(Castle.DynamicProxy.IInvocation invocation)
        {
            //var ss1 = new System.Diagnostics.StackFrame(3, false).GetMethod().Name.Split('|');
            //var group = 1 < ss1.Length ? ss1[1] : null;
            //if (null != group) { }

            var startTime = new System.Diagnostics.Stopwatch();
            startTime.Start();
            var meta = this.MetaData[invocation.Method.Name];
            var methodName = meta.FullName;
            var argsObj = invocation.Arguments;
            var logType = BusinessLogType.Record;
            var logAttr = meta.BusinessLogAttr;
            //var commandGroup = meta.CommandGroup;
            //==================================//
            var iArgGroup = Bind.GetDefaultCommandGroup(invocation.Method.Name);
            var iArgs = Bind.GetIArgs(meta.IArgs, argsObj, iArgGroup);
            if (0 < iArgs.Count)
            {
                var group = iArgs[meta.IArgs[0].Position].Group;
                //if (!System.String.IsNullOrEmpty(group)) { iArgGroup = iArgs[meta.IArgs[0].Position].Group; }
                if (!System.String.IsNullOrEmpty(group)) { iArgGroup = group; }
            }
            var args = meta.ArgAttrs[iArgGroup];

            try
            {
                foreach (var item in args.Args)
                {
                    IResult result = null;
                    var value = argsObj[item.Position];
                    var iArgIn = item.HasIArg ? iArgs[item.Position].In : null;

                    var iArgHasString = !System.Object.Equals(null, iArgIn) && typeof(System.String).Equals(iArgIn.GetType());
                    var trim = false;

                    foreach (var argAttr in item.ArgAttr)
                    {
                        if (item.HasIArg)
                        {
                            trim = (args.CommandAttr.TrimChar || argAttr.TrimChar) && iArgHasString;

                            if (trim)
                            {
                                iArgIn = System.Convert.ToString(iArgIn).Trim();
                                if (!System.Object.Equals(iArgs[item.Position].In, iArgIn))
                                {
                                    iArgs[item.Position].In = iArgIn;
                                }
                            }

                            result = argAttr.Proces(iArgIn, item.IArgOutType, methodName, item.Name, this.Business);
                        }
                        else
                        {
                            trim = item.HasString && (args.CommandAttr.TrimChar || argAttr.TrimChar) && null != value;

                            if (trim)
                            {
                                value = System.Convert.ToString(value).Trim();
                                argsObj[item.Position] = value;
                            }

                            result = argAttr.Proces(value, item.Type, methodName, item.Name, this.Business);
                        }

                        if (0 >= result.State)
                        {
                            invocation.ReturnValue = Bind.GetReturnValue<Result>(result.State, result.Message, meta); logType = BusinessLogType.Exception; return;
                        }

                        //if (!item.HasIArg && trim) { argsObj[item.Position] = value; }

                        //========================================//

                        if (result.HasData)
                        {
                            if (!item.HasIArg)
                            {
                                argsObj[item.Position] = result.Data;
                            }
                            else
                            {
                                //item.IArgValueSet(value, result.Data);
                                iArgs[item.Position].Out = result.Data;
                            }
                        }
                        //else if (!item.HasIArg && trim)
                        //{
                        //    argsObj[item.Position] = value;
                        //}

                        //========================================//

                        //if (item.HasIArg && result.HasData)
                        //{
                        //    item.IArgValueSet(value, result.Data);
                        //}
                    }

                    //========================================//
                    object currentValue = item.HasIArg ? (null != result && result.HasData) ? result.Data : iArgs[item.Position].Out : (null != result && result.HasData) ? result.Data : value;
                    //========================================//

                    var trimIArg = false;

                    result = CheckChild(item, result, iArgs, value, methodName, this.Business, out trimIArg);
                    if (null != result && 0 >= result.State) { invocation.ReturnValue = Bind.GetReturnValue<Result>(result.State, result.Message, meta); logType = BusinessLogType.Exception; return; }

                    //item.HasIArg && 
                    //if (0 == item.ArgAttrChild.Count || null == currentValue)
                    //{
                    //    continue;
                    //}



                    //foreach (var argAttrChild in item.ArgAttrChild)
                    //{
                    //    if (argAttrChild.Trim || 0 < argAttrChild.ArgAttr.Count)
                    //    {
                    //        var memberValue = argAttrChild.MemberAccessorGet(currentValue);
                    //        if (argAttrChild.Trim && null != memberValue)
                    //        {
                    //            memberValue = System.Convert.ToString(memberValue).Trim();
                    //            argAttrChild.MemberAccessorSet(currentValue, memberValue);
                    //            if (!trimIArg) { trimIArg = !trimIArg; }
                    //        }

                    //        //========================================//

                    //        foreach (var argAttr in argAttrChild.ArgAttr)
                    //        {
                    //            result = argAttr.Proces(memberValue, argAttrChild.Type, methodName, argAttrChild.FullName, this.Business);
                    //            if (0 >= result.State) { invocation.ReturnValue = Bind.GetReturnValue<Result>(result.State, result.Message, meta); logType = BusinessLogType.Exception; return; }
                    //            if (result.HasData)
                    //            {
                    //                if (!argAttrChild.HasIArg)
                    //                {
                    //                    argAttrChild.MemberAccessorSet(currentValue, result.Data);
                    //                    if (!trimIArg) { trimIArg = !trimIArg; }
                    //                }
                    //            }
                    //        }
                    //    }
                    //}

                    //========================================//

                    if (item.HasIArg && trimIArg)
                    {
                        iArgs[item.Position].Out = currentValue;
                        //item.IArgValueSet(value, currentValue);
                    }
                }

                //===============================//
                startTime.Restart();
                invocation.Proceed();
            }
            catch (System.Exception ex)
            {
                invocation.ReturnValue = Bind.GetReturnValue<Result>(0, System.Convert.ToString(ex), meta); logType = BusinessLogType.Exception;
            }
            finally
            {
                #region finally

                startTime.Stop();
                var total = Extensions.Help.Scale(startTime.Elapsed.TotalSeconds);

                if (null != this.WriteLogAsync)
                {
                    if (logType == BusinessLogType.Exception)
                    {
                        this.WriteLogAsync.BeginInvoke(new BusinessLog { Type = logType, Value = argsObj, Result = invocation.ReturnValue, Time = total, Member = methodName }, null, null);
                    }
                    else if (null != logAttr && !logAttr.NotRecord)
                    {
                        if (!logAttr.NotValue)
                        {
                            if (0 < iArgs.Count)
                            {
                                foreach (var item in iArgs)
                                {
                                    switch (item.Value.Log)
                                    {
                                        case Attributes.LogMode.No:
                                            argsObj[item.Key] = null;
                                            break;
                                        case Attributes.LogMode.In:
                                            argsObj[item.Key] = item.Value.In;
                                            break;
                                        case Attributes.LogMode.Out:
                                            argsObj[item.Key] = item.Value.Out;
                                            break;
                                        case Attributes.LogMode.All: break;
                                    }
                                }
                            }
                        }

                        this.WriteLogAsync.BeginInvoke(new BusinessLog { Type = logType, Value = !logAttr.NotValue ? argsObj : null, Result = !logAttr.NotResult ? invocation.ReturnValue : null, Time = total, Member = methodName }, null, null);
                    }
                }

                #endregion
            }
        }
        //, IResult result, dynamic iArgsOut, object value
        static IResult CheckChild(Args item, object currentValue, IResult result, System.Collections.Generic.Dictionary<int, IArg> iArgs, string methodName, IBusiness business, out bool trimIArg)
        {
            trimIArg = false;

            if (0 == item.ArgAttrChild.Count || null == currentValue)
            {
                return null;
            }

            //IResult result = null;

            foreach (var argAttrChild in item.ArgAttrChild)
            {
                if (argAttrChild.Trim || 0 < argAttrChild.ArgAttr.Count)
                {
                    var memberValue = argAttrChild.MemberAccessorGet(currentValue);
                    if (argAttrChild.Trim && null != memberValue)
                    {
                        memberValue = System.Convert.ToString(memberValue).Trim();
                        argAttrChild.MemberAccessorSet(currentValue, memberValue);
                        if (!trimIArg) { trimIArg = !trimIArg; }
                    }

                    //========================================//

                    foreach (var argAttr in argAttrChild.ArgAttr)
                    {
                        result = argAttr.Proces(memberValue, argAttrChild.Type, methodName, argAttrChild.FullName, business);
                        if (0 >= result.State) { return result; }
                        if (result.HasData)
                        {
                            if (!argAttrChild.HasIArg)
                            {
                                argAttrChild.MemberAccessorSet(currentValue, result.Data);
                                if (!trimIArg) { trimIArg = !trimIArg; }
                            }
                        }
                    }

                    object currentValue2 = item.HasIArg ? (null != result && result.HasData) ? result.Data : iArgs[item.Position].Out : (null != result && result.HasData) ? result.Data : value;

                    return CheckChild(argAttrChild, currentValue2, result, iArgs, methodName, business, out trimIArg);
                }
            }

            return CheckChild(item, currentValue, methodName, business, out trimIArg);
        }
        */

        public System.Collections.Concurrent.ConcurrentDictionary<string, MetaData> MetaData { get; set; }

        public System.Action<Log> WriteLogAsync { get; set; }

        public dynamic Business { get; set; }

        public void Dispose()
        {
            //if (null != MetaData) { MetaData.Clear(); }
        }
    }
}