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

                    if (System.String.IsNullOrWhiteSpace(iArg.Group)) { iArg.Group = defaultCommandKey; }

                    //iArg.Log = item.IArgLog;

                    result.Add(item.Position, iArg);
                }
            }

            return result;
        }

        internal static object GetReturnValue<Result>(int state, string message, MetaData meta) where Result : IResult, new() { return meta.HasIResult ? Business.Result.ResultFactory.Create<Result>(state, message) : meta.HasReturn && meta.ReturnType.IsValueType ? System.Activator.CreateInstance(meta.ReturnType) : null; }

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
    public class Bind<Business> : Bind<Business, ResultObject<string>>
        where Business : class
    {
        public Bind(Auth.IInterceptor<ResultObject<string>> interceptor, params object[] constructorArguments) : base(interceptor, constructorArguments) { }

        public new static Business Create(params object[] constructorArguments)
        {
            return new Bind<Business>(new Auth.Interceptor<ResultObject<string>>(), constructorArguments).Instance;
        }
    }

    /// <summary>
    /// Result.ResultBase
    /// </summary>
    /// <typeparam name="Business"></typeparam>
    public class Bind2<Business> : Bind<Business, ResultBase<string>>
        where Business : class
    {
        public Bind2(Auth.IInterceptor<ResultBase<string>> interceptor, params object[] constructorArguments) : base(interceptor, constructorArguments) { }

        public new static Business Create(params object[] constructorArguments)
        {
            return new Bind2<Business>(new Auth.Interceptor<ResultBase<string>>(), constructorArguments).Instance;
        }
    }

    public class Bind<Business, Result> : BindBase<Result>
        where Business : class
        where Result : IResult, new()
    {
        public static Business Create(params object[] constructorArguments)
        {
            return new Bind<Business, Result>(new Auth.Interceptor<Result>(), constructorArguments).Instance;
        }

        public new Business Instance { get; private set; }

        public Bind(Auth.IInterceptor<Result> interceptor, params object[] constructorArguments)
            : base(typeof(Business), interceptor, constructorArguments)
        {
            Instance = (Business)base.Instance;
        }
    }

    public class BindBase : BindBase<ResultObject<string>>
    {
        public BindBase(System.Type type, Auth.IInterceptor<ResultObject<string>> interceptor, params object[] constructorArguments) : base(type, interceptor, constructorArguments) { }

        public static object Create(System.Type type, params object[] constructorArguments)
        {
            return new BindBase(type, new Auth.Interceptor<ResultObject<string>>(), constructorArguments).Instance;
        }
    }
    public class BindBase2 : BindBase<ResultBase<string>>
    {
        public BindBase2(System.Type type, Auth.IInterceptor<ResultBase<string>> interceptor, params object[] constructorArguments) : base(type, interceptor, constructorArguments) { }

        public static object Create(System.Type type, params object[] constructorArguments)
        {
            return new BindBase2(type, new Auth.Interceptor<ResultBase<string>>(), constructorArguments).Instance;
        }
    }

    public class BindBase<Result> : System.IDisposable
        where Result : IResult, new()
    {
        public object Instance { get; private set; }

        public BindBase(System.Type type, Auth.IInterceptor<Result> interceptor, params object[] constructorArguments)
        {
            var methods = GetMethods2(type);

            var options = new Castle.DynamicProxy.ProxyGenerationOptions(new BusinessAllMethodsHook(methods.Item1));
            var proxy = new Castle.DynamicProxy.ProxyGenerator();

            //Instance = proxy.CreateClassProxy<Business>(options, interceptor);

            Instance = proxy.CreateClassProxy(type, options, constructorArguments, interceptor);

            var resultType = (typeof(IResult<>).IsAssignableFrom(typeof(Result), out System.Type[] resultArguments) ? typeof(Result) : typeof(ResultBase<>)).GetGenericTypeDefinition();

            interceptor.MetaData = GetInterceptorMetaData(methods.Item2, type, resultType);
            interceptor.Business = Instance;

            if (typeof(IBusiness).IsAssignableFrom(type))
            {
                var business = (IBusiness)Instance;

                business.Command = GetBusinessCommand(methods.Item2, Instance, interceptor.MetaData, business);

                interceptor.Logger = business.Logger;

                business.ResultType = resultType;

                business.Configuration = new Configer.Config(interceptor.MetaData, type.FullName);

                #region Config.Info

                var configAttrs = type.GetAttributes<Attributes.ConfigAttribute>();
                if (0 < configAttrs.Length)
                {
                    business.Configuration.Info = configAttrs[0];
                    if (System.String.IsNullOrWhiteSpace(business.Configuration.Info.Name))
                    {
                        business.Configuration.Info.Name = type.Name;
                    }
                }
                else
                {
                    business.Configuration.Info = new Attributes.ConfigAttribute(type.Name);
                }

                Bind.BusinessList.TryAdd(business.Configuration.Info.Name, business);

                #endregion

                #region Token

                business.Token = typeof(IBusiness<>).IsAssignableFrom(type, out System.Type[] businessArguments) ? ConstructorInvokerGenerator.CreateDelegate<Auth.IToken>(businessArguments[0]) : () => { return new Auth.Token(); };

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
        }

        public void Dispose()
        {
            if (typeof(System.IDisposable).IsAssignableFrom(Instance.GetType()))
            {
                ((System.IDisposable)Instance).Dispose();
            }
        }

        #region

        static System.Tuple<System.Reflection.MethodInfo[], System.Reflection.MethodInfo[]> GetMethods2(System.Type type)
        {
            var ignoreList = new System.Collections.Generic.List<System.Reflection.MethodInfo>();
            var list = new System.Collections.Generic.List<System.Reflection.MethodInfo>();

            var methods = System.Array.FindAll(type.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic), c => c.IsVirtual && !c.IsFinal && !IsFinalize(c) && !IsGetType(c) && !IsMemberwiseClone(c) && !IsEquals(c) && !IsGetHashCode(c) && !IsToString(c));

            foreach (var item in methods)
            {
                var ignoreAttrs = item.GetAttributes<Attributes.IgnoreAttribute>();
                if (0 < ignoreAttrs.Length)
                {
                    ignoreList.Add(item);
                }
                else
                {
                    list.Add(item);
                }
            }

            //Property
            var propertys = type.GetProperties();

            foreach (var item in propertys)
            {
                var ignoreAttrs = item.GetAttributes<Attributes.IgnoreAttribute>();
                if (0 < ignoreAttrs.Length)
                {
                    var set = item.GetSetMethod(true);
                    if (null != set)
                    {
                        ignoreList.Add(set);
                        if (list.Contains(set))
                        {
                            list.Remove(set);
                        }
                    }

                    var get = item.GetGetMethod(true);
                    if (null != get)
                    {
                        ignoreList.Add(get);
                        if (list.Contains(get))
                        {
                            list.Remove(get);
                        }
                    }
                }
            }

            return System.Tuple.Create(ignoreList.ToArray(), list.ToArray());
        }

        static System.Collections.Generic.List<Attributes.LoggerAttribute> LoggerAttr(System.Reflection.ParameterInfo member, System.Type type)
        {
            var attrs = new System.Collections.Generic.List<Attributes.LoggerAttribute>(member.GetAttributes<Attributes.LoggerAttribute>());
            attrs.AddRange(type.GetAttributes<Attributes.LoggerAttribute>());
            return attrs.Distinct(Equality<Attributes.LoggerAttribute>.CreateComparer(c => c.LogType)).ToList();
        }
        static System.Collections.Generic.List<Attributes.LoggerAttribute> LoggerAttr(System.Reflection.MemberInfo member)
        {
            return member.GetAttributes<Attributes.LoggerAttribute>().Distinct(Equality<Attributes.LoggerAttribute>.CreateComparer(c => c.LogType)).ToList();
        }

        static MetaLogger GetMetaLogger(System.Collections.Generic.List<Attributes.LoggerAttribute> methodLoggerAttr, System.Collections.Generic.List<Attributes.LoggerAttribute> businessLoggerAttr)
        {
            var metaLogger = new MetaLogger();

            foreach (var item in businessLoggerAttr)
            {
                if (!methodLoggerAttr.Exists(c => c.LogType == item.LogType))
                {
                    methodLoggerAttr.Add(item.Clone());
                }
            }

            foreach (var item in methodLoggerAttr)
            {
                switch (item.LogType)
                {
                    case LoggerType.Record: metaLogger.RecordAttr = item; break;
                    case LoggerType.Error: metaLogger.ErrorAttr = item; break;
                    case LoggerType.Exception: metaLogger.ExceptionAttr = item; break;
                }
            }

            if (null == metaLogger.RecordAttr)
            {
                metaLogger.RecordAttr = new Attributes.LoggerAttribute(LoggerType.Record, false);
            }
            if (null == metaLogger.ErrorAttr)
            {
                metaLogger.ErrorAttr = new Attributes.LoggerAttribute(LoggerType.Error, false);
            }
            if (null == metaLogger.ExceptionAttr)
            {
                metaLogger.ExceptionAttr = new Attributes.LoggerAttribute(LoggerType.Exception, false);
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
                    case LoggerType.Record: metaLogger.RecordAttr = item; break;
                    case LoggerType.Error: metaLogger.ErrorAttr = item; break;
                    case LoggerType.Exception: metaLogger.ExceptionAttr = item; break;
                }
            }

            if (null == metaLogger.RecordAttr)
            {
                metaLogger.RecordAttr = new Attributes.LoggerAttribute(LoggerType.Record, false);
            }
            if (null == metaLogger.ErrorAttr)
            {
                metaLogger.ErrorAttr = new Attributes.LoggerAttribute(LoggerType.Error, false);
            }
            if (null == metaLogger.ExceptionAttr)
            {
                metaLogger.ExceptionAttr = new Attributes.LoggerAttribute(LoggerType.Exception, false);
            }

            return metaLogger;
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
            return !type.FullName.StartsWith("System.") && !type.IsPrimitive && !type.IsEnum && !type.IsArray;
            //return !type.IsPrimitive && !type.IsEnum && !type.IsArray && !type.IsSecurityTransparent;
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

        static object[] GetArgsObj(object[] defaultObj, object[] argsObj, System.Collections.Generic.IReadOnlyList<Args> iArgs, string group, System.Collections.Generic.IList<Args> args)
        {
            var defaultObj2 = new object[defaultObj.Length];
            System.Array.Copy(defaultObj, defaultObj2, defaultObj2.Length);

            if (null != argsObj)
            {
                for (int i = 0; i < argsObj.Length; i++)
                {
                    if (!System.Object.Equals(null, argsObj[i]) && i < defaultObj2.Length)
                    {
                        if (!System.Object.Equals(defaultObj2[i], argsObj[i]))
                        {
                            //int/long
                            defaultObj2[i] = args[i].HasIArg ? argsObj[i] : System.Convert.ChangeType(argsObj[i], args[i].Type);
                        }
                    }
                }
            }

            foreach (var item in iArgs)
            {
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
                if (System.String.IsNullOrWhiteSpace(item.Group)) { item.Group = Bind.CommandGroupDefault; }
                if (System.String.IsNullOrWhiteSpace(item.OnlyName)) { item.OnlyName = method.Name; }
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
                        return call(proxy, GetArgsObj(meta.DefaultValue, arguments, meta.IArgs, key, meta.ArgAttrs[Bind.GetCommandGroupDefault(method.Name)].Args));
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
            var businessLoggerAttr = LoggerAttr(member);

            var metaData = new System.Collections.Concurrent.ConcurrentDictionary<string, MetaData>();

#if DEBUG
            foreach (var method in methods)
#else
            methods.AsParallel().ForAll(method =>
#endif
            //methods.AsParallel().ForAll(method =>
            {
                //======LogAttribute======//
                var methodLoggerAttr = LoggerAttr(method);
                var metaLogger = GetMetaLogger(methodLoggerAttr, businessLoggerAttr);

                var commandGroup = CmdAttrGroup(method);

                var parameters = method.GetParameters();

                var argAttrGroup = new System.Collections.Concurrent.ConcurrentDictionary<string, ArgAttrs>();//commandGroup.Count

                foreach (var item in commandGroup)
                {
                    var key = Bind.GetCommandGroup(item.Group, item.OnlyName);

                    if (argAttrGroup.ContainsKey(key)) { throw new System.Exception(string.Format("Command {0} member {1} name exists!", item.Group, item.OnlyName)); }

                    argAttrGroup.TryAdd(key, new ArgAttrs(item, new System.Collections.Generic.List<Args>(), new System.Collections.Generic.List<CommandArgs>()));
                }

                foreach (var argInfo in parameters)
                {
                    var type = argInfo.ParameterType;
                    //==================================//
                    var current = GetCurrentType(argInfo, type);
                    var currentType = current.type;
                    //==================================//
                    var argAttr = GetArgAttr(argInfo, currentType);
                    var logAttrArg = LoggerAttr(argInfo, currentType);
                    var metaLoggerArg = GetMetaLogger(logAttrArg);
                    //==================================//

                    var hasString = currentType.Equals(typeof(System.String));
                    var hasDeserialize = IsClass(type);
                    var argAttrTrim = argAttr.Exists(c => c.TrimChar);

                    //set all
                    foreach (var item in argAttrGroup)
                    {
                        var argAttrs = argAttr.FindAll(c => item.Value.CommandAttr.Group == c.Group || Bind.CommandGroupDefault == c.Group);

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
#if DEBUG
            };
#else
            });
#endif

            return metaData;
        }

        struct CurrentType
        {
            public System.Boolean hasIArg;
            public System.Type type;
        }

        static CurrentType GetCurrentType(System.Reflection.ICustomAttributeProvider member, System.Type type)
        {
            var hasIArg = typeof(IArg<>).IsAssignableFrom(type, out System.Type[] iArgOutType);

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
            argAttr.Sort(ComparisonHelper<Attributes.ArgumentAttribute>.CreateComparer(c => ResultFactory.GetState(c.Code)));
            argAttr.Reverse();
            foreach (var item in argAttr)
            {
                if (System.String.IsNullOrWhiteSpace(item.Group)) { item.Group = Bind.CommandGroupDefault; }
            }

            return argAttr;
        }

        #endregion

        static System.Collections.Generic.List<Args> GetArgAttr(System.Type type, bool argAttrTrim, string parameterName, Attributes.CommandAttribute commandAttr, ref System.Collections.Generic.List<System.Type> deserializes)
        {
            var argAttrs = new System.Collections.Generic.List<Args>();

            var fields = type.GetFields();
            foreach (var field in fields)
            {
                var current = GetCurrentType(field, field.FieldType);
                var argAttrChild = GetArgAttr(field, current.type);

                var hasString = typeof(System.String).Equals(current.type);
                var hasDeserialize = IsClass(current.type);

                if (deserializes.Contains(current.type)) { continue; }
                else if (hasDeserialize) { deserializes.Add(current.type); }

                var trim = hasString && (commandAttr.TrimChar || argAttrTrim || argAttrChild.Exists(c => c.TrimChar));

                if (trim || hasDeserialize || 0 < argAttrChild.Count)
                {
                    argAttrs.Add(new Args(field.Name, field.FieldType, hasString, FieldAccessorGenerator.CreateGetter(field), FieldAccessorGenerator.CreateSetter(field), argAttrChild, hasDeserialize ? GetArgAttr(current.type, trim, field.Name, commandAttr, ref deserializes) : new System.Collections.Generic.List<Args>(), hasDeserialize, current.hasIArg, current.hasIArg ? current.type : null, hasString && trim, string.Format("{0}.{1}", parameterName, field.Name)));
                }
            }

            var propertys = type.GetProperties();
            foreach (var property in propertys)
            {
                var current = GetCurrentType(property, property.PropertyType);
                var argAttrChild = GetArgAttr(property, current.type);

                var hasString = typeof(System.String).Equals(current.type);
                var hasDeserialize = IsClass(current.type);

                if (deserializes.Contains(current.type)) { continue; }
                else if (hasDeserialize) { deserializes.Add(current.type); }

                var trim = hasString && (commandAttr.TrimChar || argAttrTrim || argAttrChild.Exists(c => c.TrimChar));

                if (trim || hasDeserialize || 0 < argAttrChild.Count)
                {
                    var getter = PropertyAccessorGenerator.CreateGetter(property);
                    var setter = PropertyAccessorGenerator.CreateSetter(property);
                    if (null == getter || null == setter) { continue; }
                    argAttrs.Add(new Args(property.Name, property.PropertyType, hasString, getter, setter, argAttrChild, hasDeserialize ? GetArgAttr(current.type, trim, property.Name, commandAttr, ref deserializes) : new System.Collections.Generic.List<Args>(), hasDeserialize, current.hasIArg, current.hasIArg ? current.type : null, hasString && trim, string.Format("{0}.{1}", parameterName, property.Name)));
                }
            }

            return argAttrs;
        }

        static System.Collections.Concurrent.ConcurrentBag<CommandArgs> GetCommandArgs(System.Type type, ref System.Collections.Generic.List<System.Type> deserializes)
        {
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
}
