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
    using Business.Extensions;
    using Business.Extensions.Emit;
    using Business.Result;

    public class Bind
    {
        public const string CommandGroupDefault = "Default";

        internal static string GetCommandGroupDefault(string name)
        {
            return GetCommandGroup(CommandGroupDefault, name);
        }

        internal static string GetCommandGroup(string group, string name)
        {
            return string.Format("{0}{1}", group, name);
        }

        internal static System.Collections.Generic.Dictionary<int, IArg> GetIArgs(System.Collections.Generic.IReadOnlyList<Args> iArgs, object[] argsObj, string defaultCommandKey)
        {
            var result = new System.Collections.Generic.Dictionary<int, IArg>();

            if (0 < iArgs.Count)
            {
                foreach (var item in iArgs)
                {
                    var iArg = (IArg)(argsObj[item.Position] ?? System.Activator.CreateInstance(item.Type));

                    if (System.String.IsNullOrEmpty(iArg.Group)) { iArg.Group = defaultCommandKey; }

                    //iArg.Log = item.IArgLog;

                    result.Add(item.Position, iArg);
                }
            }

            return result;
        }

        internal static object GetReturnValue<Result>(int state, string message, MetaData meta) where Result : Business.Result.IResult, new() { return meta.HasIResult ? Business.Result.ResultFactory.Create<Result>(state, message) : meta.HasReturn && meta.ReturnType.IsValueType ? System.Activator.CreateInstance(meta.ReturnType) : null; }

        public static readonly System.Collections.Concurrent.ConcurrentDictionary<string, IBusiness> BusinessList = new System.Collections.Concurrent.ConcurrentDictionary<string, IBusiness>();
    }

    class BusinessAllMethodsHook : Castle.DynamicProxy.AllMethodsHook
    {
        readonly System.Reflection.MethodInfo[] ignoreMethods;

        public BusinessAllMethodsHook(params System.Reflection.MethodInfo[] method)
            : base() { this.ignoreMethods = method; }

        public override bool ShouldInterceptMethod(System.Type type, System.Reflection.MethodInfo methodInfo)
        {
            if (System.Array.Exists(ignoreMethods, c => c.GetMethodFullName().Equals(methodInfo.GetMethodFullName()))) { return false; }

            return base.ShouldInterceptMethod(type, methodInfo);
        }
    }

    /// <summary>
    /// Result.ResultObject
    /// </summary>
    /// <typeparam name="Business"></typeparam>
    public class Bind<Business> : Bind<Business, Result.ResultObject<string>>
        where Business : class
    {
        public Bind(Auth.IInterceptor<Result.ResultObject<string>> interceptor, params object[] constructorArguments) : base(interceptor, constructorArguments) { }

        public new static Business Create(params object[] constructorArguments)
        {
            return new Bind<Business>(new Auth.Interceptor<Result.ResultObject<string>>(), constructorArguments).Instance;
        }
    }

    /// <summary>
    /// Result.ResultBase
    /// </summary>
    /// <typeparam name="Business"></typeparam>
    public class Bind2<Business> : Bind<Business, ResultBase<string>>
        where Business : class//, IBusiness
    {
        public Bind2(Auth.IInterceptor<ResultBase<string>> interceptor, params object[] constructorArguments) : base(interceptor, constructorArguments) { }

        public new static Business Create(params object[] constructorArguments)
        {
            return new Bind2<Business>(new Auth.Interceptor<ResultBase<string>>(), constructorArguments).Instance;
        }
    }

    //public class Bind<Business, Result> : Bind<Business, Result>
    //    where Business : class
    //    where Result : IResult, new()
    //{
    //    public Bind(Auth.IInterceptor<Result> interceptor) : base(interceptor) { }

    //    public static Business Create()
    //    {
    //        return new Bind<Business, Result>(new Auth.Interceptor<Result>()).Instance;
    //    }
    //}
    /*
    public struct LogCfg : ILogCfg
    {
        readonly System.Collections.Concurrent.ConcurrentDictionary<string, MetaData> metaData;

        public LogCfg(System.Collections.Concurrent.ConcurrentDictionary<string, MetaData> metaData)
        {
            this.metaData = metaData;
        }

        public void SetCanWrite(bool canWrite, string method = null)
        {
            if (!System.String.IsNullOrEmpty(method))
            {
                MetaData meta;
                if (this.metaData.ContainsKey(method) && this.metaData.TryGetValue(method, out meta))
                {
                    if (null != meta.LogAttr)
                    {
                        meta.LogAttr.CanWrite = canWrite;
                    }
                }
            }
            else
            {
                foreach (var item in this.metaData.Values)
                {
                    if (null != item.LogAttr)
                    {
                        item.LogAttr.CanWrite = canWrite;
                    }
                }
            }
        }

        public void SetCanWriteException(bool canException, string method = null)
        {
            if (System.String.IsNullOrEmpty(method))
            {
                foreach (var item in this.metaData.Values)
                {
                    if (null != item.LogAttr)
                    {
                        item.LogAttr.CanWriteException = canException;
                    }
                }
            }
            else
            {
                MetaData meta;
                if (this.metaData.ContainsKey(method) && this.metaData.TryGetValue(method, out meta))
                {
                    if (null != meta.LogAttr)
                    {
                        meta.LogAttr.CanWriteException = canException;
                    }
                }
            }
        }

        public void SetCanValue(bool canValue, string method = null)
        {
            if (System.String.IsNullOrEmpty(method))
            {
                foreach (var item in this.metaData.Values)
                {
                    if (null != item.LogAttr)
                    {
                        item.LogAttr.CanValue = canValue;
                    }
                }
            }
            else
            {
                MetaData meta;
                if (this.metaData.ContainsKey(method) && this.metaData.TryGetValue(method, out meta))
                {
                    if (null != meta.LogAttr)
                    {
                        meta.LogAttr.CanValue = canValue;
                    }
                }
            }
        }

        public void SetCanResult(bool canResult, string method = null)
        {
            if (System.String.IsNullOrEmpty(method))
            {
                foreach (var item in this.metaData.Values)
                {
                    if (null != item.LogAttr)
                    {
                        item.LogAttr.CanResult = canResult;
                    }
                }
            }
            else
            {
                MetaData meta;
                if (this.metaData.ContainsKey(method) && this.metaData.TryGetValue(method, out meta))
                {
                    if (null != meta.LogAttr)
                    {
                        meta.LogAttr.CanResult = canResult;
                    }
                }
            }
        }

        //public void SetAttributes(string method, dynamic value, string group = Bind.DefaultCommandGroup)
        //{
        //    MetaData meta;
        //    if (this.metaData.ContainsKey(method) && this.metaData.TryGetValue(method, out meta))
        //    {
        //        if (null != meta.ArgAttrs[group].Args[0].ArgAttr[0])
        //        {
        //            meta.LogAttr.CanResult = canResult;
        //        }
        //    }
        //}
    }
    */

    public class Bind<Business, Result> : System.IDisposable
        where Business : class//, IBusiness
        where Result : IResult, new()
    {
        //public static Business Create()
        //{
        //    return Create<ResultBase<string>>();
        //}

        //public static Business Create<Result>() where Result : IResult, new()
        //{
        //    return Create(new Auth.Interceptor<Result, BusinessLog>());
        //}

        //public static Business Create<Result, BusinessLog>(Auth.IInterceptor<Result, BusinessLog> interceptor = null)
        //    where Result : IResult, new()
        //    where BusinessLog : IBusinessLog, new()
        //{
        //    return new Bind<Business, Result, BusinessLog>(new Auth.Interceptor<Result, BusinessLog>()).Instance;
        //}

        public static Business Create(params object[] constructorArguments)
        //where Result : IResult, new()
        //where BusinessLog : IBusinessLog, new()
        {
            return new Bind<Business, Result>(new Auth.Interceptor<Result>(), constructorArguments).Instance;
        }

        public Business Instance { get; private set; }

        public Bind(Auth.IInterceptor<Result> interceptor, params object[] constructorArguments)
        {
            var type = typeof(Business);
            var methods = GetMethods(type);
            var methods2 = GetMethods2(methods);

            var options = new Castle.DynamicProxy.ProxyGenerationOptions(new BusinessAllMethodsHook(methods2.Item1));
            var proxy = new Castle.DynamicProxy.ProxyGenerator();

            //Instance = proxy.CreateClassProxy<Business>(options, interceptor);

            Instance = (Business)proxy.CreateClassProxy(type, options, constructorArguments, interceptor);

            System.Type[] resultArguments;
            var resultType = (typeof(IResult<>).IsAssignableFrom(typeof(Result), out resultArguments) ? typeof(Result) : typeof(ResultBase<>)).GetGenericTypeDefinition();

            interceptor.MetaData = GetInterceptorMetaData(methods2.Item2, type, resultType);
            interceptor.Business = Instance;

            if (typeof(IBusiness).IsAssignableFrom(type))
            {
                var business = (IBusiness)Instance;

                business.Command = GetBusinessCommand(methods2.Item2, Instance, interceptor.MetaData, business);

                interceptor.WriteLogAsync = business.WriteLogAsync;

                business.ResultType = resultType;

                business.Config = new Configer.Config(interceptor.MetaData, type.FullName);

                #region Config.Info

                var configAttrs = type.GetAttributes<Attributes.ConfigAttribute>();
                if (0 < configAttrs.Length)
                {
                    business.Config.Info = configAttrs[0];
                    if (System.String.IsNullOrWhiteSpace(business.Config.Info.Name))
                    {
                        business.Config.Info.Name = type.Name;
                    }
                }
                else
                {
                    business.Config.Info = new Attributes.ConfigAttribute(type.Name);
                }

                Bind.BusinessList.TryAdd(business.Config.Info.Name, business);

                #endregion

                #region Token

                System.Type[] businessArguments;
                if (typeof(IBusiness<>).IsAssignableFrom(type, out businessArguments))
                {
                    business.Token = Extensions.Emit.ConstructorInvokerGenerator.CreateDelegate<Auth.IToken>(businessArguments[0]);
                }
                else
                {
                    business.Token = () => { return new Auth.Token(); };
                }

                #endregion
            }

            #region Config

            var cfgSection = Configer.Config.Instance;
            if (null != cfgSection)
            {
                var sections = Configer.Config.GetGroup(cfgSection);
                var loggerSections = sections.Item1;
                var attributeSections = sections.Item2;

                var loggerGroup = loggerSections.FirstOrDefault(c => type.FullName.Equals(c.Key));
                var attributeGroup = attributeSections.FirstOrDefault(c => type.FullName.Equals(c.Key));

                if (null != loggerGroup)
                {
                    Configer.Config.Logger(interceptor.MetaData, loggerGroup);
                }

                if (null != attributeGroup)
                {
                    Configer.Config.Attribute(interceptor.MetaData, attributeGroup);
                }
            }

            #endregion

            //Instance.Command = GetBusinessCommand(notIntercept.Item2, Instance, interceptor.MetaData);

            //interceptor.WriteLogAsync = Instance.WriteLogAsync;

            //Instance.ResultType = resultType;
        }

        public void Dispose()
        {
            if (typeof(System.IDisposable).IsAssignableFrom(Instance.GetType()))
            {
                ((System.IDisposable)Instance).Dispose();
            }
        }

        #region

        static System.Tuple<System.Reflection.MethodInfo[], System.Reflection.MethodInfo[]> GetMethods2(System.Reflection.MethodInfo[] methods)
        {
            var ignoreList = new System.Collections.Generic.List<System.Reflection.MethodInfo>();
            var list = new System.Collections.Generic.List<System.Reflection.MethodInfo>();
            foreach (var item in methods)
            {
                var ignoreAttrs = item.GetAttributes<Attributes.IgnoreAttribute>();
                if (0 < ignoreAttrs.Length)// || (!typeof(void).Equals(item.ReturnType) && !typeof(Result.IResult).Equals(item.ReturnType))
                {
                    ignoreList.Add(item);
                }
                else
                {
                    list.Add(item);
                }
            }
            return System.Tuple.Create(ignoreList.ToArray(), list.ToArray());
        }

        //static Attributes.LoggerAttribute LogAttr(System.Reflection.ICustomAttributeProvider member, System.Type type = null)
        //{
        //    var logAttrs = member.GetAttributes<Attributes.LoggerAttribute>();
        //    if (0 == logAttrs.Length && null != type) { logAttrs = type.GetAttributes<Attributes.LoggerAttribute>(); }
        //    return 0 < logAttrs.Length ? logAttrs[0] : null;
        //}

        static System.Collections.Generic.List<Attributes.LoggerAttribute> LogAttr(System.Reflection.ParameterInfo member, System.Type type)
        {
            var attrs = new System.Collections.Generic.List<Attributes.LoggerAttribute>(member.GetAttributes<Attributes.LoggerAttribute>());
            attrs.AddRange(type.GetAttributes<Attributes.LoggerAttribute>());
            return attrs.Distinct(Equality<Attributes.LoggerAttribute>.CreateComparer(c => c.LogType)).ToList();
        }
        static System.Collections.Generic.List<Attributes.LoggerAttribute> LogAttr(System.Reflection.MemberInfo member)
        {
            return member.GetAttributes<Attributes.LoggerAttribute>().Distinct(Equality<Attributes.LoggerAttribute>.CreateComparer(c => c.LogType)).ToList();
        }

        static MetaLogger GetMetaLogger(System.Collections.Generic.List<Attributes.LoggerAttribute> method, System.Collections.Generic.List<Attributes.LoggerAttribute> businessLogAttr)
        {
            var metaLogger = new MetaLogger();

            foreach (var item in businessLogAttr)
            {
                if (!method.Exists(c => c.LogType == item.LogType))
                {
                    method.Add(item.Clone());
                }
            }

            foreach (var item in method)
            {
                switch (item.LogType)
                {
                    case LogType.Record: metaLogger.RecordAttr = item; break;
                    case LogType.Error: metaLogger.ErrorAttr = item; break;
                    case LogType.Exception: metaLogger.ExceptionAttr = item; break;
                }
            }

            if (null == metaLogger.RecordAttr)
            {
                metaLogger.RecordAttr = new Attributes.LoggerAttribute(LogType.Record, false);
            }
            if (null == metaLogger.ErrorAttr)
            {
                metaLogger.ErrorAttr = new Attributes.LoggerAttribute(LogType.Error, false);
            }
            if (null == metaLogger.ExceptionAttr)
            {
                metaLogger.ExceptionAttr = new Attributes.LoggerAttribute(LogType.Exception, false);
            }

            return metaLogger;
        }

        static MetaLogger GetMetaLogger(System.Collections.Generic.List<Attributes.LoggerAttribute> attrs)
        {
            var metaLogger = new MetaLogger();

            foreach (var item in attrs)
            {
                switch (item.LogType)
                {
                    case LogType.Record: metaLogger.RecordAttr = item; break;
                    case LogType.Error: metaLogger.ErrorAttr = item; break;
                    case LogType.Exception: metaLogger.ExceptionAttr = item; break;
                }
            }

            if (null == metaLogger.RecordAttr)
            {
                metaLogger.RecordAttr = new Attributes.LoggerAttribute(LogType.Record, false);
            }
            if (null == metaLogger.ErrorAttr)
            {
                metaLogger.ErrorAttr = new Attributes.LoggerAttribute(LogType.Error, false);
            }
            if (null == metaLogger.ExceptionAttr)
            {
                metaLogger.ExceptionAttr = new Attributes.LoggerAttribute(LogType.Exception, false);
            }

            return metaLogger;
        }

        //static MetaLogger GetMetaLogger(Attributes.LoggerAttribute[] businessLogAttr, System.Reflection.MethodInfo method)
        //{
        //    var methodLogAttr = LogAttr(method);

        //    var logAttr = businessLogAttr.Union(methodLogAttr, Equality<Attributes.LoggerAttribute>.CreateComparer(c => c.LogType));

        //    var metaLogger = new MetaLogger();

        //    foreach (var item in logAttr)
        //    {
        //        switch (item.LogType)
        //        {
        //            case LogType.Record: metaLogger.LogRecordAttr = item; break;
        //            case LogType.Exception: metaLogger.LogExceptionAttr = item; break;
        //            case LogType.Error: metaLogger.LogErrorAttr = item; break;
        //        }
        //    }

        //    if (null == metaLogger.LogRecordAttr)
        //    {
        //        metaLogger.LogRecordAttr = new LoggerAttribute(LogType.Record, false);
        //    }
        //    if (null == metaLogger.LogExceptionAttr)
        //    {
        //        metaLogger.LogExceptionAttr = new LoggerAttribute(LogType.Exception, false);
        //    }
        //    if (null == metaLogger.LogErrorAttr)
        //    {
        //        metaLogger.LogErrorAttr = new LoggerAttribute(LogType.Error, false);
        //    }

        //    return metaLogger;
        //}

        //static Attributes.ArgumentAttribute[] ArgsAttr(System.Reflection.MemberInfo member)
        //{
        //    return member.GetAttributes<Attributes.ArgumentAttribute>();
        //}

        //static Attributes.CommandAttribute[] CmdAttr(System.Reflection.MemberInfo member)
        //{
        //    //Attributes.CommandAttribute commandAttr;
        //    //var commandAttrs = member.GetAttributes<Attributes.CommandAttribute>();
        //    //commandAttr = 0 < commandAttrs.Length ? commandAttrs[0] : null;
        //    //return commandAttr;
        //    return member.GetAttributes<Attributes.CommandAttribute>();
        //}

        static System.Reflection.MethodInfo[] GetMethods(System.Type type)
        {
            return System.Array.FindAll(type.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic), c => c.IsVirtual && !c.IsFinal && !IsFinalize(c) && !IsGetType(c) && !IsMemberwiseClone(c) && !IsEquals(c) && !IsGetHashCode(c) && !IsToString(c));
        }

        #region

        static bool IsFinalize(System.Reflection.MethodInfo methodInfo)
        {
            return methodInfo.DeclaringType == typeof(object) && string.Equals("Finalize", methodInfo.Name, System.StringComparison.OrdinalIgnoreCase);
        }

        static bool IsGetType(System.Reflection.MethodInfo methodInfo)
        {
            return methodInfo.DeclaringType == typeof(object) && string.Equals("GetType", methodInfo.Name, System.StringComparison.OrdinalIgnoreCase);
        }

        static bool IsMemberwiseClone(System.Reflection.MethodInfo methodInfo)
        {
            return methodInfo.DeclaringType == typeof(object) && string.Equals("MemberwiseClone", methodInfo.Name, System.StringComparison.OrdinalIgnoreCase);
        }

        static bool IsEquals(System.Reflection.MethodInfo methodInfo)
        {
            return methodInfo.DeclaringType == typeof(object) && string.Equals("Equals", methodInfo.Name, System.StringComparison.OrdinalIgnoreCase);
        }

        static bool IsGetHashCode(System.Reflection.MethodInfo methodInfo)
        {
            return methodInfo.DeclaringType == typeof(object) && string.Equals("GetHashCode", methodInfo.Name, System.StringComparison.OrdinalIgnoreCase);
        }

        static bool IsToString(System.Reflection.MethodInfo methodInfo)
        {
            return methodInfo.DeclaringType == typeof(object) && string.Equals("ToString", methodInfo.Name, System.StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        static bool IsClass(System.Type type)
        {
            return !type.IsPrimitive && !type.IsEnum && !type.IsArray && !type.IsSecurityTransparent;
        }

        static System.Object[] GetDefaultValue(System.Reflection.MethodInfo method, System.Collections.Generic.IList<Args> args)
        {
            var argsObj = new object[args.Count];

            for (int i = 0; i < args.Count; i++)
            {
                var arg = args[i];

                if (arg.HasIArg) { continue; }

                if (arg.Type.IsValueType && null == arg.DefaultValue)
                {
                    argsObj[i] = System.Activator.CreateInstance(arg.Type);
                }
                else if (arg.HasDefaultValue)
                {
                    argsObj[i] = arg.DefaultValue;
                }
            }

            return argsObj;
        }

        static object[] GetArgsObj(object[] defaultObj, object[] argsObj, System.Collections.Generic.IReadOnlyList<Args> iArgs, string group)
        {
            object[] defaultObj2 = new object[defaultObj.Length];
            System.Array.Copy(defaultObj, defaultObj2, defaultObj2.Length);

            if (null != argsObj)
            {
                for (int i = 0; i < argsObj.Length; i++)
                {
                    if (null != argsObj[i] && i < defaultObj2.Length)
                    {
                        if (!System.Object.Equals(defaultObj2[i], argsObj[i]))
                        {
                            defaultObj2[i] = argsObj[i];
                        }
                    }
                }
            }

            foreach (var item in iArgs)
            {
                //defaultObj[item.Position] = System.Activator.CreateInstance(item.ParameterType, new object[] { defaultObj[item.Position], group });
                var iArg = (IArg)System.Activator.CreateInstance(item.Type);

                iArg.In = defaultObj2[item.Position];
                iArg.Group = group;

                defaultObj2[item.Position] = iArg;
            }

            return defaultObj2;
        }

        static System.Collections.Generic.List<Attributes.CommandAttribute> CmdAttrGroup(System.Reflection.MethodInfo method)
        {
            var commandAttrs = method.GetAttributes<Attributes.CommandAttribute>().GroupBy(c => new { c.Group, c.OnlyName }).Select(c => c.First()).ToList();
            foreach (var item in commandAttrs)
            {
                if (System.String.IsNullOrEmpty(item.Group)) { item.Group = Bind.CommandGroupDefault; }
                if (System.String.IsNullOrEmpty(item.OnlyName)) { item.OnlyName = method.Name; }
            }

            if (!commandAttrs.Exists(c => Bind.CommandGroupDefault == c.Group && method.Name == c.OnlyName)) { commandAttrs.Add(new Attributes.CommandAttribute(method.Name) { Group = Bind.CommandGroupDefault }); }

            return commandAttrs;
        }

        static System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IReadOnlyDictionary<string, Command>> GetBusinessCommand(System.Reflection.MethodInfo[] methods, object proxy, System.Collections.Concurrent.ConcurrentDictionary<string, MetaData> metaData, IBusiness business)
        {
            var commandGroup = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, Command>>();

            //========================================//

            var type = proxy.GetType();

            foreach (var method in methods)
            {
                var meta = metaData[method.Name];

                //set all
                foreach (var item in meta.CommandGroup)
                {
                    var key = Bind.GetCommandGroup(item.Group, item.OnlyName);

                    var method2 = type.GetMethod(method.Name);

                    System.Func<object, object[], dynamic> call;

                    if (typeof(void).Equals(method2.ReturnType))
                    {
                        var call2 = MethodInvokerGenerator.CreateDelegate2(method2, false, key);
                        call = (p, p1) => { call2(p, p1); return null; };
                    }
                    else
                    {
                        call = MethodInvokerGenerator.CreateDelegate<dynamic>(method2, false, key);
                    }

                    var businessCommand = new Command((arguments) =>
                    {
                        return call(proxy, GetArgsObj(meta.DefaultValue, arguments, meta.IArgs, key));
                    }, new CommandMeta(meta.Name, meta.HasReturn, meta.ReturnType));
                    //, meta.ArgAttrs[Bind.GetDefaultCommandGroup(method.Name)].CommandArgs
                    if (commandGroup.ContainsKey(item.Group))
                    {
                        if (commandGroup[item.Group].ContainsKey(item.OnlyName)) { throw new System.Exception(string.Format("Command {0} member {1} name exists!", item.Group, item.OnlyName)); }

                        commandGroup[item.Group].Add(item.OnlyName, businessCommand);
                    }
                    else
                    {
                        commandGroup.Add(item.Group, new System.Collections.Generic.Dictionary<string, Command> { { item.OnlyName, businessCommand } });
                    }
                }
            }

            //========================================//

            var command = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.IReadOnlyDictionary<string, Command>>(commandGroup.Count);

            foreach (var item in commandGroup)
            {
                command.Add(item.Key, item.Value);
            }

            return command;
        }

        static System.Collections.Concurrent.ConcurrentDictionary<string, MetaData> GetInterceptorMetaData(System.Reflection.MethodInfo[] methods, System.Reflection.MemberInfo member, System.Type resultType)
        {
            var businessLogAttr = LogAttr(member);

            var metaData = new System.Collections.Concurrent.ConcurrentDictionary<string, MetaData>();

            //foreach (var method in methods)
            methods.AsParallel().ForAll(method =>
            {
                //======LogAttribute======//
                var methodLogger = LogAttr(method);
                //methodLogger.AddRange(businessLogAttr.Select(c => c.Clone()));
                //var metaLogger = GetMetaLogger(methodLogger);
                var metaLogger = GetMetaLogger(methodLogger, businessLogAttr);

                var commandGroup = CmdAttrGroup(method);

                var parameters = method.GetParameters();

                var argAttrGroup = new System.Collections.Concurrent.ConcurrentDictionary<string, ArgAttrs>();//commandGroup.Count

                foreach (var item in commandGroup)
                {
                    var key = Bind.GetCommandGroup(item.Group, item.OnlyName);

                    if (argAttrGroup.ContainsKey(key)) { throw new System.Exception(string.Format("Command {0} member {1} name exists!", item.Group, item.OnlyName)); }

                    argAttrGroup.TryAdd(key, new ArgAttrs(item, new System.Collections.Generic.List<Args>(), new System.Collections.Generic.List<CommandArgs>()));
                }

                //var logAttrs = new System.Collections.Generic.Dictionary<int, Attributes.LoggerAttribute>();

                foreach (var argInfo in parameters)
                {
                    var type = argInfo.ParameterType;
                    //==================================//
                    //var hasIArg = null != type.GetInterface("Business.IArg`1");
                    //System.Type iArgOutType = null;
                    //if (hasIArg)
                    //{
                    //    iArgOutType = type.GenericTypeArguments[0];
                    //}

                    var current = GetCurrentType(argInfo, type);
                    var currentType = current.type;
                    //==================================//
                    var argAttr = GetArgAttr(argInfo, currentType);
                    var logAttrArg = LogAttr(argInfo, currentType);
                    var metaLoggerArg = GetMetaLogger(logAttrArg);
                    //logAttrs.Add(argInfo.Position, LogAttr(argInfo, currentType));
                    //var argAttr = new System.Collections.Generic.List<Attributes.ArgumentAttribute>();
                    //argAttr.AddRange(argInfo.GetAttributes<Attributes.ArgumentAttribute>());
                    //argAttr.AddRange(currentType.GetAttributes<Attributes.ArgumentAttribute>());

                    //foreach (var item in argAttr)
                    //{
                    //    if (item.GetType().IsSubclassOf(typeof(Attributes.ArgAttribute)))
                    //    {
                    //        var iArgAttr = (Attributes.ArgAttribute)item;
                    //        if (System.String.IsNullOrEmpty(iArgAttr.Group)) { iArgAttr.Group = Bind.DefaultCommandGroup; }
                    //    }
                    //}

                    //argAttr = argAttr.OrderBy(c => c.GetType().IsSubclassOf(typeof(Attributes.ArgAttribute))).ToList();

                    //==================================//

                    var hasString = currentType.Equals(typeof(System.String));
                    var hasDeserialize = IsClass(type);

                    //&& (currentType.IsValueType && currentType.IsLayoutSequential) 
                    var argAttrTrim = argAttr.Exists(c => c.TrimChar);

                    //var argAttrChilds = GetArgAttr(currentType, hasDeserialize, argAttrTrim, argInfo.Name);

                    //var argAttrChilds = new System.Collections.Generic.List<Args>();

                    //if (hasDeserialize)
                    //{
                    //    //if (hasIArg && null != commandAttr) { argAttr.Add(commandAttr); }

                    //    var fields = currentType.GetFields();
                    //    foreach (var field in fields)
                    //    {
                    //        var argAttrChild = new System.Collections.Generic.List<Attributes.ArgumentAttribute>(field.GetAttributes<Attributes.ArgumentAttribute>());

                    //        var trim = argAttrTrim || argAttrChild.Exists(c => c.TrimChar);

                    //        argAttrChilds.Add(new Args(field.Name, field.FieldType, typeof(System.String).Equals(field.FieldType), FieldAccessorGenerator.CreateGetter(field), FieldAccessorGenerator.CreateSetter(field), argAttrChild, null, trim, string.Format("{0}.{1}", argInfo.Name, field.Name)));
                    //    }

                    //    var propertys = currentType.GetProperties();
                    //    foreach (var property in propertys)
                    //    {
                    //        var argAttrChild = new System.Collections.Generic.List<Attributes.ArgumentAttribute>(property.GetAttributes<Attributes.ArgumentAttribute>());

                    //        var trim = argAttrTrim || argAttrChild.Exists(c => c.TrimChar);

                    //        argAttrChilds.Add(new Args(property.Name, property.PropertyType, typeof(System.String).Equals(property.PropertyType), PropertyAccessorGenerator.CreateGetter(property), PropertyAccessorGenerator.CreateSetter(property), argAttrChild, null, trim, string.Format("{0}.{1}", argInfo.Name, property.Name)));
                    //    }
                    //}

                    //set all
                    foreach (var item in argAttrGroup)
                    {
                        var argAttrs = argAttr.FindAll(c => item.Value.CommandAttr.Group == c.Group || Bind.CommandGroupDefault == c.Group);
                        //var argAttrs = argAttr.FindAll(c => (c.GetType().IsSubclassOf(typeof(Attributes.DeserializeAttribute)) && item.Value.CommandAttr.Group == ((Attributes.DeserializeAttribute)c).Group) || !c.GetType().IsSubclassOf(typeof(Attributes.DeserializeAttribute)));
                        ////unified
                        //if (current.hasIArg && item.Value.CommandAttr.HasUnified)
                        //{
                        //    argAttrs = argAttrs.FindAll(c => !c.GetType().IsSubclassOf(typeof(Attributes.DeserializeAttribute)));
                        //    argAttrs.Add(item.Value.CommandAttr);
                        //}

                        var deserializes = hasDeserialize ? new System.Collections.Generic.List<System.Type>() { currentType } : new System.Collections.Generic.List<System.Type>();

                        var argAttrChilds = hasDeserialize ? GetArgAttr(currentType, argAttrTrim, argInfo.Name, item.Value.CommandAttr, ref deserializes) : new System.Collections.Generic.List<Args>();

                        item.Value.Args.Add(new Args(argInfo.Name, type, argInfo.Position, argInfo.DefaultValue, argInfo.HasDefaultValue, hasString, argAttrs, argAttrChilds, hasDeserialize, current.hasIArg, current.hasIArg ? currentType : null, metaLoggerArg));
                    }

                    var deserializes2 = hasDeserialize ? new System.Collections.Generic.List<System.Type>() { currentType } : new System.Collections.Generic.List<System.Type>();

                    var commandArgs = new CommandArgs(argInfo.Name, type, argInfo.HasDefaultValue, hasDeserialize, hasDeserialize ? GetCommandArgs(currentType, ref deserializes2) : new System.Collections.Concurrent.ConcurrentBag<CommandArgs>());
                    argAttrGroup[Bind.GetCommandGroupDefault(method.Name)].CommandArgs.Add(commandArgs);
                }

                var args = argAttrGroup[Bind.GetCommandGroupDefault(method.Name)].Args;

                var meta = new MetaData(commandGroup, argAttrGroup, args.Where(c => c.HasIArg).ToList(), metaLogger, method.Name, method.GetMethodFullName(), typeof(void) != method.ReturnType, typeof(IResult).IsAssignableFrom(method.ReturnType), method.ReturnType, resultType, GetDefaultValue(method, args));

                if (metaData.ContainsKey(method.Name)) { throw new System.Exception(string.Format("MetaData name exists {0}!", method.Name)); }
                metaData.TryAdd(method.Name, meta);
            });

            return metaData;
        }

        struct CurrentType
        {
            public System.Boolean hasIArg;
            public System.Type type;
        }

        static CurrentType GetCurrentType(System.Reflection.ICustomAttributeProvider member, System.Type type)
        {
            System.Type[] iArgOutType;
            var hasIArg = typeof(IArg<>).IsAssignableFrom(type, out iArgOutType);
            //var hasIArg = null != type.GetInterface("Business.IArg`1");

            //System.Type iArgOutType = null;
            ////Attributes.LogMode iArgLog = Attributes.LogMode.In;
            //if (hasIArg)
            //{
            //    iArgOutType = genericArguments[0];

            //    //var log = member.GetAttributes<Attributes.ArgLog>();
            //    //if (0 == log.Length) { log = type.GetAttributes<Attributes.ArgLog>(); }
            //    //if (0 < log.Length) { iArgLog = log[0].Log; }
            //}

            //return new CurrentType { hasIArg = hasIArg, iArgOutType = iArgOutType, type = iArgOutType ?? type };
            return new CurrentType { hasIArg = hasIArg, type = hasIArg ? iArgOutType[0] : type };
        }

        #region GetArgAttr

        static System.Collections.Generic.List<Attributes.ArgumentAttribute> GetArgAttr(System.Reflection.ParameterInfo member, System.Type type)
        {
            var argAttr = new System.Collections.Generic.List<Attributes.ArgumentAttribute>();
            argAttr.AddRange(member.GetAttributes<Attributes.ArgumentAttribute>());
            argAttr.AddRange(type.GetAttributes<Attributes.ArgumentAttribute>());
            return GetArgAttr(argAttr);
        }
        static System.Collections.Generic.List<Attributes.ArgumentAttribute> GetArgAttr(System.Reflection.MemberInfo member, System.Type type)
        {
            var argAttr = new System.Collections.Generic.List<Attributes.ArgumentAttribute>();
            argAttr.AddRange(member.GetAttributes<Attributes.ArgumentAttribute>());
            argAttr.AddRange(type.GetAttributes<Attributes.ArgumentAttribute>());
            return GetArgAttr(argAttr);
        }
        static System.Collections.Generic.List<Attributes.ArgumentAttribute> GetArgAttr(System.Collections.Generic.List<Attributes.ArgumentAttribute> argAttr)
        {
            //==================================//
            //var hasIArg = null != type.GetInterface("Business.IArg`1");
            //System.Type iArgOutType = null;
            //if (hasIArg)
            //{
            //    iArgOutType = type.GenericTypeArguments[0];
            //}
            //var current = GetCurrentType(type);
            //var currentType = current.type;

            //==================================//
            argAttr.Sort(ComparisonHelper<Attributes.ArgumentAttribute>.CreateComparer(c => ResultFactory.GetState(c.Code)));
            argAttr.Reverse();
            foreach (var item in argAttr)
            {
                //if (item.GetType().IsSubclassOf(typeof(Attributes.DeserializeAttribute)))
                //{
                //    var iArgAttr = (Attributes.DeserializeAttribute)item;
                //    if (System.String.IsNullOrEmpty(iArgAttr.Group)) { iArgAttr.Group = Bind.CommandGroupDefault; }
                //}
                if (System.String.IsNullOrWhiteSpace(item.Group)) { item.Group = Bind.CommandGroupDefault; }
            }

            //argAttr = argAttr.OrderBy(c => c.GetType().IsSubclassOf(typeof(Attributes.DeserializeAttribute))).ToList();

            return argAttr;
        }

        #endregion

        static System.Collections.Generic.List<Args> GetArgAttr(System.Type type, bool argAttrTrim, string parameterName, Attributes.CommandAttribute commandAttr, ref System.Collections.Generic.List<System.Type> deserializes)
        {
            var argAttrs = new System.Collections.Generic.List<Args>();

            //var hasDeserialize = !type.IsPrimitive && !type.IsEnum && !type.Equals(typeof(System.Object)) && !type.Equals(typeof(System.String)) && type.IsNested;
            //if (!hasDeserialize) { return argAttrs; }

            //if (hasIArg && null != commandAttr) { argAttr.Add(commandAttr); }

            var fields = type.GetFields();
            foreach (var field in fields)
            {
                //var argAttrChild = new System.Collections.Generic.List<Attributes.ArgumentAttribute>(field.GetAttributes<Attributes.ArgumentAttribute>());

                var current = GetCurrentType(field, field.FieldType);
                var currentType = current.type;
                var argAttrChild = GetArgAttr(field, currentType);
                //var logAttrArg = LogAttr(field, current.type);

                var hasString = typeof(System.String).Equals(currentType);
                var hasDeserialize = IsClass(currentType);

                if (deserializes.Contains(currentType)) { continue; }
                else if (hasDeserialize) { deserializes.Add(currentType); }
                //if (0 == argAttrChild.Count) { continue; }

                //trim = argAttrChild.HasString && (args.CommandAttr.TrimChar || argAttrChild.Trim);
                var trim = hasString && (commandAttr.TrimChar || argAttrTrim || argAttrChild.Exists(c => c.TrimChar));

                if (trim || hasDeserialize || 0 < argAttrChild.Count)
                {
                    argAttrs.Add(new Args(field.Name, field.FieldType, hasString, FieldAccessorGenerator.CreateGetter(field), FieldAccessorGenerator.CreateSetter(field), argAttrChild, hasDeserialize ? GetArgAttr(current.type, trim, field.Name, commandAttr, ref deserializes) : new System.Collections.Generic.List<Args>(), hasDeserialize, current.hasIArg, current.hasIArg ? currentType : null, hasString && trim, string.Format("{0}.{1}", parameterName, field.Name)));
                }
            }

            var propertys = type.GetProperties();
            foreach (var property in propertys)
            {
                //var argAttrChild = new System.Collections.Generic.List<Attributes.ArgumentAttribute>(property.GetAttributes<Attributes.ArgumentAttribute>());

                var current = GetCurrentType(property, property.PropertyType);
                var currentType = current.type;
                var argAttrChild = GetArgAttr(property, currentType);
                //var logAttrArg = LogAttr(property, current.type);

                var hasString = typeof(System.String).Equals(currentType);
                var hasDeserialize = IsClass(currentType);

                if (deserializes.Contains(currentType)) { continue; }
                else if (hasDeserialize) { deserializes.Add(currentType); }
                //if (0 == argAttrChild.Count) { continue; }

                //if (hasString || 0 < argAttrChild.Count)
                //{

                //}

                var trim = hasString && (commandAttr.TrimChar || argAttrTrim || argAttrChild.Exists(c => c.TrimChar));

                if (trim || hasDeserialize || 0 < argAttrChild.Count)
                {
                    var getter = PropertyAccessorGenerator.CreateGetter(property);
                    var setter = PropertyAccessorGenerator.CreateSetter(property);
                    if (null == getter || null == setter) { continue; }
                    argAttrs.Add(new Args(property.Name, property.PropertyType, hasString, getter, setter, argAttrChild, hasDeserialize ? GetArgAttr(current.type, trim, property.Name, commandAttr, ref deserializes) : new System.Collections.Generic.List<Args>(), hasDeserialize, current.hasIArg, current.hasIArg ? currentType : null, hasString && trim, string.Format("{0}.{1}", parameterName, property.Name)));
                }
            }

            return argAttrs;
        }

        static System.Collections.Concurrent.ConcurrentBag<CommandArgs> GetCommandArgs(System.Type type, ref System.Collections.Generic.List<System.Type> deserializes)
        {
            //var argAttrs = new System.Collections.Generic.List<CommandArgs>();
            var argAttrs = new System.Collections.Concurrent.ConcurrentBag<CommandArgs>();

            var fields = type.GetFields();
            foreach (var field in fields)
            {
                var current = GetCurrentType(field, field.FieldType);
                var hasDeserialize = IsClass(current.type);
                if (deserializes.Contains(current.type)) { continue; }
                else if (hasDeserialize) { deserializes.Add(current.type); }

                argAttrs.Add(new CommandArgs(field.Name, current.hasIArg ? current.type : field.FieldType, false, hasDeserialize, hasDeserialize ? GetCommandArgs(current.type, ref deserializes) : new System.Collections.Concurrent.ConcurrentBag<CommandArgs>()));
            }

            var propertys = type.GetProperties();
            foreach (var property in propertys)
            {
                var current = GetCurrentType(property, property.PropertyType);
                var hasDeserialize = IsClass(current.type);
                if (deserializes.Contains(current.type)) { continue; }
                else if (hasDeserialize) { deserializes.Add(current.type); }

                argAttrs.Add(new CommandArgs(property.Name, current.hasIArg ? current.type : property.PropertyType, false, hasDeserialize, hasDeserialize ? GetCommandArgs(current.type, ref deserializes) : new System.Collections.Concurrent.ConcurrentBag<CommandArgs>()));
            }

            return argAttrs;
        }

        #endregion
    }

    #region Meta

    public struct ArgAttrs
    {
        public ArgAttrs(Attributes.CommandAttribute commandAttr, System.Collections.Generic.IList<Args> args, System.Collections.Generic.IList<CommandArgs> commandArgs)
        {
            this.commandAttr = commandAttr;
            this.args = args;
            this.commandArgs = commandArgs;
        }

        readonly Attributes.CommandAttribute commandAttr;
        public Attributes.CommandAttribute CommandAttr { get { return commandAttr; } }

        readonly System.Collections.Generic.IList<Args> args;
        public System.Collections.Generic.IList<Args> Args { get { return args; } }
        //===============commandArgs==================//
        readonly System.Collections.Generic.IList<CommandArgs> commandArgs;
        public System.Collections.Generic.IList<CommandArgs> CommandArgs { get { return commandArgs; } }
    }

    /// <summary>
    /// Argument
    /// </summary>
    public struct Args
    {
        //Arg
        public Args(System.String name, System.Type type, System.Int32 position, System.Object defaultValue, System.Boolean hasDefaultValue, System.Boolean hasString, System.Collections.Generic.IReadOnlyList<Attributes.ArgumentAttribute> argAttr, System.Collections.Generic.IList<Args> argAttrChild, System.Boolean hasDeserialize, System.Boolean hasIArg, System.Type iArgOutType, MetaLogger metaLogger)
        {
            this.name = name;
            this.type = type;
            this.position = position;
            this.defaultValue = defaultValue;
            this.hasDefaultValue = hasDefaultValue;
            this.hasString = hasString;
            this.argAttr = argAttr;
            this.argAttrChild = argAttrChild;
            this.hasDeserialize = hasDeserialize;
            this.hasIArg = hasIArg;
            this.iArgOutType = iArgOutType;
            this.metaLogger = metaLogger;
            //this.iArgLog = iArgLog;
            //this.iArgValueObjGet = iArgValueObjGet;
            //this.iArgValueSet = iArgValueSet;
            //this.iArgValueGet = iArgValueGet;
            //this.iArgCommandGroupGet = commandGroup;
            //this.iArgConstructor = iArgConstructor;
            this.memberAccessorGet = null;
            this.memberAccessorSet = null;
            this.trim = false;
            this.fullName = null;
        }

        //argChild
        public Args(System.String name, System.Type type, System.Boolean hasString, System.Func<object, object> memberAccessorGet, System.Action<object, object> memberAccessorSet, System.Collections.Generic.IReadOnlyList<Attributes.ArgumentAttribute> argAttr, System.Collections.Generic.IList<Args> argAttrChild, System.Boolean hasDeserialize, System.Boolean hasIArg, System.Type iArgOutType, System.Boolean trim, System.String fullName)
        {
            this.name = name;
            this.type = type;
            this.hasString = hasString;
            this.memberAccessorGet = memberAccessorGet;
            this.memberAccessorSet = memberAccessorSet;
            this.argAttr = argAttr;
            this.argAttrChild = argAttrChild;
            this.hasDeserialize = hasDeserialize;
            this.hasIArg = hasIArg;
            this.iArgOutType = iArgOutType;
            this.trim = trim;
            this.fullName = fullName;

            this.position = -1;
            this.defaultValue = null;
            this.hasDefaultValue = false;
            this.metaLogger = default(MetaLogger);
            //this.iArgLog = Attributes.LogMode.In;
        }

        //===============name==================//
        readonly System.String name;
        public System.String Name { get { return name; } }
        //===============type==================//
        readonly System.Type type;
        public System.Type Type { get { return type; } }
        //===============position==================//
        readonly System.Int32 position;
        public System.Int32 Position { get { return position; } }
        //===============defaultValue==================//
        readonly System.Object defaultValue;
        public System.Object DefaultValue { get { return defaultValue; } }
        //===============hasDefaultValue==================//
        readonly System.Boolean hasDefaultValue;
        public System.Boolean HasDefaultValue { get { return hasDefaultValue; } }
        //===============hasString==================//
        readonly System.Boolean hasString;
        public System.Boolean HasString { get { return hasString; } }
        //===============memberAccessorGet==================//
        readonly System.Func<object, object> memberAccessorGet;
        public System.Func<object, object> MemberAccessorGet { get { return memberAccessorGet; } }
        //===============memberAccessorSet==================//
        readonly System.Action<object, object> memberAccessorSet;
        public System.Action<object, object> MemberAccessorSet { get { return memberAccessorSet; } }
        //===============argAttr==================//
        readonly System.Collections.Generic.IReadOnlyList<Attributes.ArgumentAttribute> argAttr;
        public System.Collections.Generic.IReadOnlyList<Attributes.ArgumentAttribute> ArgAttr { get { return argAttr; } }
        //===============argAttrChild==================//
        readonly System.Collections.Generic.IList<Args> argAttrChild;
        public System.Collections.Generic.IList<Args> ArgAttrChild { get { return argAttrChild; } }
        //===============hasDeserialize==================//
        readonly System.Boolean hasDeserialize;
        public System.Boolean HasDeserialize { get { return hasDeserialize; } }
        //===============hasIArg==================//
        readonly System.Boolean hasIArg;
        public System.Boolean HasIArg { get { return hasIArg; } }
        //===============iArgType==================//
        readonly System.Type iArgOutType;
        public System.Type IArgOutType { get { return iArgOutType; } }
        //==============trim===================//
        readonly System.Boolean trim;
        public System.Boolean Trim { get { return trim; } }
        //==============fullName===================//
        readonly System.String fullName;
        public System.String FullName { get { return fullName; } }
        //==============logAttr===================//
        readonly MetaLogger metaLogger;
        public MetaLogger MetaLogger { get { return metaLogger; } }
        ////==============iArgLog===================//
        //readonly Attributes.LogMode iArgLog;
        //public Attributes.LogMode IArgLog { get { return iArgLog; } }

        ////===============iArgValueObj==================//
        //readonly System.Func<object, object> iArgValueObjGet;
        //public System.Func<object, object> IArgValueObjGet { get { return iArgValueObjGet; } }
        ////===============iArgValueSet==================//
        //readonly System.Action<object, object> iArgValueSet;
        //public System.Action<object, object> IArgValueSet { get { return iArgValueSet; } }
        ////===============iArgValueGet==================//
        //readonly System.Func<object, object> iArgValueGet;
        //public System.Func<object, object> IArgValueGet { get { return iArgValueGet; } }
        ////===============hasCommand==================//
        //readonly System.Func<object, string> iArgCommandGroupGet;
        //public System.Func<object, string> IArgCommandGroupGet { get { return iArgCommandGroupGet; } }
        //===============iArgConstructor==================//
        //readonly System.Func<object[], object> iArgConstructor;
        //public System.Func<object[], object> IArgConstructor { get { return iArgConstructor; } }
    }

    public struct MetaLogger
    {
        public Attributes.LoggerAttribute RecordAttr { get; set; }
        public Attributes.LoggerAttribute ExceptionAttr { get; set; }
        public Attributes.LoggerAttribute ErrorAttr { get; set; }
    }

    public struct MetaData
    {
        public MetaData(System.Collections.Generic.IReadOnlyList<Attributes.CommandAttribute> commandGroup, System.Collections.Concurrent.ConcurrentDictionary<string, ArgAttrs> argAttrs, System.Collections.Generic.IReadOnlyList<Args> iArgs, MetaLogger metaLogger, string name, string fullName, bool hasReturn, bool hasIResult, System.Type returnType, System.Type resultType, object[] defaultValue)
        {
            this.commandGroup = commandGroup;
            this.argAttrs = argAttrs;
            this.iArgs = iArgs;
            this.metaLogger = metaLogger;
            this.name = name;
            this.fullName = fullName;
            this.hasReturn = hasReturn;
            this.hasIResult = hasIResult;
            this.returnType = returnType;
            this.resultType = resultType;
            this.defaultValue = defaultValue;
            //this.logAttrs = logAttrs;
        }
        //==============commandAttr===================//
        readonly System.Collections.Generic.IReadOnlyList<Attributes.CommandAttribute> commandGroup;
        public System.Collections.Generic.IReadOnlyList<Attributes.CommandAttribute> CommandGroup { get { return commandGroup; } }
        //==============argMetaData===================//
        readonly System.Collections.Concurrent.ConcurrentDictionary<string, ArgAttrs> argAttrs;
        public System.Collections.Concurrent.ConcurrentDictionary<string, ArgAttrs> ArgAttrs { get { return argAttrs; } }
        //==============iArgs===================//
        readonly System.Collections.Generic.IReadOnlyList<Args> iArgs;
        public System.Collections.Generic.IReadOnlyList<Args> IArgs { get { return iArgs; } }
        //==============MetaLogger===================//
        readonly MetaLogger metaLogger;
        public MetaLogger MetaLogger { get { return metaLogger; } }
        //==============name===================//
        readonly string name;
        public string Name { get { return name; } }
        //==============fullName===================//
        readonly string fullName;
        public string FullName { get { return fullName; } }
        //==============hasReturn===================//
        readonly bool hasReturn;
        public bool HasReturn { get { return hasReturn; } }
        //==============hasIResult===================//
        readonly bool hasIResult;
        public bool HasIResult { get { return hasIResult; } }
        //==============hasIResult===================//
        readonly System.Type returnType;
        public System.Type ReturnType { get { return returnType; } }
        //==============resultType===================//
        readonly System.Type resultType;
        public System.Type ResultType { get { return resultType; } }
        //==============defaultValue===================//
        readonly object[] defaultValue;
        public object[] DefaultValue { get { return defaultValue; } }
        ////==============logAttributes===================//
        //readonly System.Collections.Generic.Dictionary<int, Attributes.LoggerAttribute> logAttrs;
        //public System.Collections.Generic.Dictionary<int, Attributes.LoggerAttribute> LogAttrs { get { return logAttrs; } }
    }

    public struct Command
    {
        public Command(System.Func<object[], dynamic> call, CommandMeta meta)
        {
            this.call = call;
            this.meta = meta;
        }

        //===============member==================//
        readonly System.Func<object[], dynamic> call;

        public dynamic Call(params object[] args)
        {
            return call(args);
        }
        //==============hasReturn===================//
        //readonly bool hasReturn;
        //public bool HasReturn { get { return hasReturn; } }

        readonly CommandMeta meta;
        public CommandMeta Meta { get { return meta; } }
        ////==============hasIResult===================//
        //readonly bool hasIResult;
        //public bool HasIResult { get { return hasIResult; } }
    }

    public struct CommandMeta
    {
        public CommandMeta(string name, bool hasReturn, System.Type returnType)//, System.Collections.Generic.IList<CommandArgs> args)
        {
            this.name = name;
            this.hasReturn = hasReturn;
            this.returnType = returnType;
            //this.args = args;
        }

        readonly string name;
        public string Name { get { return name; } }

        readonly bool hasReturn;
        public bool HasReturn { get { return hasReturn; } }

        readonly System.Type returnType;
        public System.Type ReturnType { get { return returnType; } }

        //readonly System.Collections.Generic.IList<CommandArgs> args;
        //public System.Collections.Generic.IList<CommandArgs> Args { get { return args; } }

        //public override string ToString()
        //{
        //    var sb = new System.Text.StringBuilder(null);

        //    this.args.Select(c => string.Format("{0}{1}{2}", c.Name, c.Type.Name, c.HasDefaultValue));

        //    sb.AppendFormat("{0}{1}", System.Environment.NewLine, this.name, this.returnType.Name);

        //    return sb.ToString();
        //}
    }

    public struct CommandArgs
    {
        public CommandArgs(System.String name, System.Type type, System.Boolean hasDefaultValue, System.Boolean hasDeserialize, System.Collections.Concurrent.ConcurrentBag<CommandArgs> argChild)
        {
            this.name = name;
            this.type = type;
            this.hasDefaultValue = hasDefaultValue;
            this.hasDeserialize = hasDeserialize;
            //this.argAttr = argAttr;
            this.argChild = argChild;
        }

        //===============name==================//
        readonly System.String name;
        public System.String Name { get { return name; } }
        //===============type==================//
        readonly System.Type type;
        public System.Type Type { get { return type; } }
        //===============HasDefaultValue==================//
        readonly System.Boolean hasDefaultValue;
        public System.Boolean HasDefaultValue { get { return hasDefaultValue; } }
        //===============hasDeserialize==================//
        readonly System.Boolean hasDeserialize;
        public System.Boolean HasDeserialize { get { return hasDeserialize; } }
        //===============argChild==================//
        readonly System.Collections.Concurrent.ConcurrentBag<CommandArgs> argChild;
        public System.Collections.Concurrent.ConcurrentBag<CommandArgs> ArgChild { get { return argChild; } }
    }

    #endregion

    //public class Bind<Business> : IBind, System.IDisposable
    //    where Business : class, IBusiness
    //{
    //    //public static Business Create(IInterceptor interceptor = null)
    //    //{
    //    //    if (null == interceptor) { interceptor = new Interceptor(); }
    //    //    var bind = new Bind<Business>(interceptor);
    //    //    return bind.Instance;
    //    //}
    //}
}
