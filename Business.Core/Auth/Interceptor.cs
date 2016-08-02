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

using System.Linq;

namespace Business.Auth
{
    using Business;
    using Result;

    public interface IInterceptor : Castle.DynamicProxy.IInterceptor, System.IDisposable
    {
        System.Action<Log> WriteLogAsync { get; set; }

        System.Collections.Generic.IReadOnlyDictionary<string, MetaData> MetaData { get; set; }

        dynamic Business { get; set; }
    }

    public interface IInterceptor<Result> : IInterceptor
        where Result : IResult, new() { }

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
            var logType = LogType.Record;
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
                            invocation.ReturnValue = Bind.GetReturnValue<Result>(result.State, result.Message, meta); logType = LogType.Exception; return;
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
                                if (0 >= result.State) { invocation.ReturnValue = Bind.GetReturnValue<Result>(result.State, result.Message, meta); logType = LogType.Exception; return; }
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
                var total = Extensions.Help.Scale(startTime.Elapsed.TotalSeconds);

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

                    if (logType == LogType.Exception)
                    {
                        this.WriteLogAsync.BeginInvoke(new Log { Type = logType, Value = argsObj, Result = invocation.ReturnValue, Time = total, Member = methodName }, null, null);
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

                        this.WriteLogAsync.BeginInvoke(new Log { Type = logType, Value = !logAttr.NotValue ? argsObj : null, Result = !logAttr.NotResult ? invocation.ReturnValue : null, Time = total, Member = methodName }, null, null);
                    }
                }
            }
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
        public System.Collections.Generic.IReadOnlyDictionary<string, MetaData> MetaData { get; set; }

        public System.Action<Log> WriteLogAsync { get; set; }

        public dynamic Business { get; set; }

        public void Dispose()
        {
            //if (null != MetaData) { MetaData.Clear(); }
        }
    }
}