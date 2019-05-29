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
    using System.Reflection;
    using System.Linq;
    using Business.Utils;
    using Business.Utils.Emit;
    using Business.Result;
    using Business.Meta;
    using Business.Attributes;

    public struct MethodArgs
    {
        public string Name;

        public dynamic Value;

        public bool HasIArg;

        public System.Type Type;

        public System.Type OutType;

        public System.Type InType;
    }

    public partial class Bind
    {
        public static IResult BusinessError(System.Type resultType, string business) => ResultFactory.ResultCreate(resultType, -1, $"Without this Business {business}");

        public static IResult CmdError(System.Type resultType, string cmd) => ResultFactory.ResultCreate(resultType, -2, $"Without this Cmd {cmd}");

        #region Internal

        internal static System.Collections.Generic.Dictionary<int, IArg> GetIArgs(System.Collections.Generic.IReadOnlyList<Args> iArgs, object[] argsObj, string defaultCommandKey)
        {
            var result = new System.Collections.Generic.Dictionary<int, IArg>();

            if (0 < iArgs?.Count)
            {
                foreach (var item in iArgs)
                {
                    var iArg = (IArg)(argsObj[item.Position] ?? System.Activator.CreateInstance(item.Type));

                    if (string.IsNullOrWhiteSpace(iArg.Group)) { iArg.Group = defaultCommandKey; }

                    //iArg.Log = item.IArgLog;

                    result.Add(item.Position, iArg);
                }
            }

            return result;
        }

        //internal static object GetReturnValue(int state, string message, MetaData meta, System.Type resultType) => GetReturnValue(ResultFactory.ResultCreate(resultType, state, message), meta);

        internal static object GetReturnValue(IResult result, MetaData meta)
        {
            var result2 = (meta.HasIResult || meta.HasObject) ? result : meta.HasReturn && meta.ReturnType.IsValueType ? System.Activator.CreateInstance(meta.ReturnType) : null;

            if (meta.HasAsync)
            {
                //return meta.HasIResult ? System.Threading.Tasks.Task.FromResult((IResult)result2) : meta.HasReturn ? System.Threading.Tasks.Task.FromResult(result2) : System.Threading.Tasks.Task.Run(() => { });
                //if (meta.HasIResult)
                //{
                //    return System.Threading.Tasks.Task.FromResult((IResult)result2);
                //}
                //else
                //{
                //    return System.Threading.Tasks.Task.FromResult(meta.HasReturn ? result2 : null);
                //}

                return System.Threading.Tasks.Task.FromResult(meta.HasIResult ? result2 as IResult : meta.HasReturn ? result2 : null);
            }

            return result2;
        }

        #endregion

        #region Create

        /// <summary>
        /// Initialize a Generic proxy class
        /// </summary>
        /// <typeparam name="Business"></typeparam>
        /// <param name="constructorArguments"></param>
        /// <returns></returns>
        public static Business Create<Business>(params object[] constructorArguments)
            where Business : class
        {
            return (Business)Create(typeof(Business), null, constructorArguments);
        }

        /// <summary>
        /// Initialize a Generic proxy class
        /// </summary>
        /// <typeparam name="Business"></typeparam>
        /// <param name="interceptor"></param>
        /// <param name="constructorArguments"></param>
        /// <returns></returns>
        public static Business Create<Business>(Auth.IInterceptor interceptor = null, params object[] constructorArguments)
            where Business : class
        {
            return (Business)Create(typeof(Business), interceptor, constructorArguments);
        }

        /// <summary>
        /// Initialize a Type proxy class
        /// </summary>
        /// <param name="type"></param>
        /// <param name="constructorArguments"></param>
        /// <returns></returns>
        public static object Create(System.Type type, params object[] constructorArguments)
        {
            return Create(type, null, constructorArguments);
        }

        /// <summary>
        /// Initialize a Type proxy class
        /// </summary>
        /// <param name="type"></param>
        /// <param name="interceptor"></param>
        /// <param name="constructorArguments"></param>
        /// <returns></returns>
        public static object Create(System.Type type, Auth.IInterceptor interceptor = null, params object[] constructorArguments)
        {
            return new Bind(type, interceptor ?? new Auth.Interceptor(), constructorArguments).Instance;
        }

        #endregion
    }

    class BusinessAllMethodsHook : Castle.DynamicProxy.AllMethodsHook
    {
        readonly MethodInfo[] ignoreMethods;

        public BusinessAllMethodsHook(params MethodInfo[] method)
            : base() { ignoreMethods = method; }

        public override bool ShouldInterceptMethod(System.Type type, MethodInfo methodInfo)
        {
            if (System.Array.Exists(ignoreMethods, c => string.Equals(c.GetMethodFullName(), methodInfo.GetMethodFullName()))) { return false; }

            return base.ShouldInterceptMethod(type, methodInfo);
        }
    }

    public partial class Bind : System.IDisposable
    {
        public object Instance { get; private set; }

        public Bind(System.Type type, Auth.IInterceptor interceptor, params object[] constructorArguments)
        {
            var typeInfo = type.GetTypeInfo();

            var methods = GetMethods(typeInfo);

            var proxy = new Castle.DynamicProxy.ProxyGenerator();

            try
            {
                Instance = proxy.CreateClassProxy(type, constructorArguments, interceptor);
            }
            catch (System.Exception ex)
            {
                throw ex.ExceptionWrite(true, true);
            }

            var generics = typeof(IBusiness<>).IsAssignableFrom(type.GetTypeInfo(), out System.Type[] businessArguments);

            var resultType = generics ? businessArguments[0].GetGenericTypeDefinition() : typeof(ResultObject<string>).GetGenericTypeDefinition();

            var attributes = AttributeBase.GetTopAttributes(typeInfo);//GetArgAttr(typeInfo);

            #region LoggerAttribute

            //var loggerBase = attributes.GetAttr<LoggerAttribute>();//AssemblyAttr<LoggerAttribute>(typeInfo.Assembly, GropuAttribute.Comparer);

            //foreach (var item in loggerBase)
            //{
            //    if (!attributes.Any(c => c is LoggerAttribute && ((LoggerAttribute)c).LogType == item.LogType))
            //    {
            //        attributes.Add(item.Clone());
            //    }
            //}

            #endregion
            /*
#region RouteAttribute

            var routeBase = AssemblyAttr(typeInfo.Assembly, routeComparer);

            foreach (var item in routeBase)
            {
                if (!attributes.Any(c => c is RouteAttribute && System.String.Equals(((RouteAttribute)c).GetKey(true), item.GetKey(true), System.StringComparison.CurrentCultureIgnoreCase)))
                {
                    attributes.Add(item.Clone<RouteAttribute>());
                }
            }

#endregion
            */
            var info = attributes.GetAttr<Info>() ?? new Info(type.Name);

            if (string.IsNullOrWhiteSpace(info.BusinessName))
            {
                info.BusinessName = type.Name;
            }

            var business = typeof(IBusiness).IsAssignableFrom(type) ? (IBusiness)Instance : null;
#if !Mobile
            //var cfg = new Configer.Configuration(info, resultType, attributes, typeInfo.GetAttributes<Attributes.EnableWatcherAttribute>().Exists());
#else
            
#endif
            var cfg = new Configer(info, resultType, attributes);

            business?.BindBefore?.Invoke(cfg);

            interceptor.MetaData = GetInterceptorMetaData(cfg, methods, Instance);

            //interceptor.ResultType = cfg.ResultType;

            interceptor.Configer = cfg;

            if (null != business)
            {
                cfg.MetaData = interceptor.MetaData;

                business.Configer = cfg;

                business.Command = GetBusinessCommand(business);

                interceptor.Logger = business.Logger;

                Configer.BusinessList.dictionary.TryAdd(business.Configer.Info.BusinessName, business);

                business.BindAfter?.Invoke();
            }

            /*
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
            */
        }

        public void Dispose()
        {
            var type = Instance.GetType();

            if (typeof(IBusiness).IsAssignableFrom(type))
            {
                var business = (IBusiness)Instance;

                Configer.BusinessList.dictionary.TryRemove(business.Configer.Info.BusinessName, out _);
                /*
#if !Mobile
                if (!Bind.BusinessList.Values.Any(c => c.Configuration.EnableWatcher) && Configer.Configuration.CfgWatcher.EnableRaisingEvents)
                {
                    Configer.Configuration.CfgWatcher.EnableRaisingEvents = false;
                }
#endif
                */
            }

            if (typeof(System.IDisposable).IsAssignableFrom(type))
            {
                ((System.IDisposable)Instance).Dispose();
            }
        }

        #region

        /*
        static (MethodInfo[], System.Collections.Generic.Dictionary<int, MethodMeta>) GetMethods2(TypeInfo type)
        {
            var ignoreList = new System.Collections.Generic.List<MethodInfo>();
            var list = new System.Collections.Generic.Dictionary<string, MethodMeta>();

            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public).Where(c => c.IsVirtual && !c.IsFinal);

            //var methods = type.DeclaredMethods.Where(c => c.IsVirtual && !c.IsFinal && c.IsPublic);

            foreach (var item in methods)
            {
                var ignore = item.GetAttribute<Attributes.Ignore>();
                if (null != ignore && ignore.Contains(Attributes.IgnoreMode.Method) && string.IsNullOrWhiteSpace(ignore.Group))
                {
                    ignoreList.Add(item);
                }
                else if (item.DeclaringType.Equals(type))
                {
                    list.Add(item.Name, new MethodMeta { Ignore = ignore, Method = item });
                }
            }

            //Property
            foreach (var item in type.DeclaredProperties)
            {
                var ignore = item.GetAttribute<Attributes.Ignore>();
                if (null != ignore && ignore.Contains(Attributes.IgnoreMode.Method) && string.IsNullOrWhiteSpace(ignore.Group))
                {
                    var set = item.GetSetMethod(true);
                    if (null != set)
                    {
                        ignoreList.Add(set);
                        if (list.ContainsKey(set.Name))
                        {
                            list.Remove(set.Name);
                        }
                    }

                    var get = item.GetGetMethod(true);
                    if (null != get)
                    {
                        ignoreList.Add(get);
                        if (list.ContainsKey(get.Name))
                        {
                            list.Remove(get.Name);
                        }
                    }
                }
            }

            var i = 0;
            //return (ignoreList.ToArray(), list.ToDictionary(c => i++, c => c));
            return (ignoreList.ToArray(), list.ToDictionary(c => i++, c => c.Value));
        }
        */

        static System.Collections.Generic.Dictionary<int, MethodInfo> GetMethods(TypeInfo type)
        {
            var list = new System.Collections.Generic.List<MethodInfo>();

            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public).Where(c => c.IsVirtual && !c.IsFinal);

            //var methods = type.DeclaredMethods.Where(c => c.IsVirtual && !c.IsFinal && c.IsPublic);

            foreach (var item in methods)
            {
                if (item.DeclaringType.Equals(type))
                {
                    //list.Add(new MethodMeta { Ignore = item.GetAttributes<Attributes.Ignore>(), Method = item });
                    list.Add(item);
                }
            }

            var i = 0;
            //return (ignoreList.ToArray(), list.ToDictionary(c => i++, c => c));
            return list.ToDictionary(c => i++, c => c);
        }

        static System.Collections.Generic.List<T> AssemblyAttr<T>(Assembly member, System.Collections.Generic.IEqualityComparer<T> comparer) where T : System.Attribute => member.GetCustomAttributes<T>().Distinct(comparer).ToList();

        static MetaLogger GetMetaLogger(System.Collections.Generic.List<LoggerAttribute> loggers, string group)
        {
            if (null == loggers) { return default; }

            var metaLogger = new MetaLogger();

            var loggers2 = loggers.Where(c => GroupEquals(c, group));

            foreach (var item in loggers2)
            {
                switch (item.LogType)
                {
                    case LoggerType.Record: metaLogger.Record = item; break;
                    case LoggerType.Error: metaLogger.Error = item; break;
                    case LoggerType.Exception: metaLogger.Exception = item; break;
                }
            }

            var all = loggers2.FirstOrDefault(c => c.LogType == LoggerType.All);

            if (null == metaLogger.Record)
            {
                metaLogger.Record = null == all ? new LoggerAttribute(LoggerType.Record, false) : all.Clone().SetType(LoggerType.Record);
            }
            if (null == metaLogger.Error)
            {
                metaLogger.Error = null == all ? new LoggerAttribute(LoggerType.Error, false) : all.Clone().SetType(LoggerType.Error);
            }
            if (null == metaLogger.Exception)
            {
                metaLogger.Exception = null == all ? new LoggerAttribute(LoggerType.Exception, false) : all.Clone().SetType(LoggerType.Exception);
            }

            return metaLogger;
        }

        /*
        static System.Collections.Generic.List<Attributes.AttributeBase> GetRoute(string methodName, System.Collections.Generic.List<RouteAttribute> businessRouteAttr, System.Collections.Generic.List<Attributes.AttributeBase> attributes, System.Collections.Generic.List<Attributes.CommandAttribute> commands)
        {
            var all = attributes.FindAll(c => c is RouteAttribute).Cast<RouteAttribute>().ToList();

            var notGroup = all.Where(c => !commands.Exists(c2 => System.String.Equals(c2.Group, c.Group, System.StringComparison.CurrentCultureIgnoreCase))).ToList();

            foreach (var item in notGroup)
            {
                all.Remove(item);
                attributes.Remove(item);
            }

            foreach (var item in businessRouteAttr)
            {
                if (!commands.Exists(c => System.String.Equals(c.Group, item.Group, System.StringComparison.CurrentCultureIgnoreCase)))
                {
                    continue;
                }

                if (!all.Any(c => System.String.Equals(c.GetKey(true), item.GetKey(true), System.StringComparison.CurrentCultureIgnoreCase)))
                {
                    var route = item.Clone<RouteAttribute>();
                    all.Add(route);
                    attributes.Add(route);
                }
            }

            foreach (var item in all)
            {
                if (System.String.IsNullOrWhiteSpace(item.Path))
                {
                    item.Path = methodName;
                }

                if (System.String.IsNullOrWhiteSpace(item.Group))
                {
                    item.Group = Bind.CommandGroupDefault;
                }

                item.MethodName = methodName;
            }

            return attributes;
        }
        */
        static bool IsClass(System.Type type)
        {
            //return type.IsClass || (type.IsValueType && !type.IsPrimitive && !type.IsEnum && !type.IsArray);
            return !type.FullName.StartsWith("System.") && (type.IsClass || (type.IsValueType && !type.IsPrimitive && !type.IsEnum && !type.IsArray));
            //return !type.IsPrimitive && !type.IsEnum && !type.IsArray && !type.IsSecurityTransparent;
        }

        static object[] GetDefaultValue(System.Collections.Generic.IList<Args> args)
        {
            if (null == args) { return new object[0]; }

            var argsObj = new object[args.Count];

            for (int i = 0; i < args.Count; i++)
            {
                var arg = args[i];

                if (arg.HasIArg) { continue; }

                if (arg.Type.GetTypeInfo().IsValueType && null == arg.DefaultValue)
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
                    if (!Equals(null, argsObj[i]) && i < defaultObj2.Length)
                    {
                        if (!Equals(defaultObj2[i], argsObj[i]))
                        {
                            //int/long
                            //defaultObj2[i] = args[i].HasIArg ? argsObj[i] : Help.ChangeType(argsObj[i], args[i].Type);
                            defaultObj2[i] = args[i].UseType || args[i].HasIArg ? argsObj[i] : Help.ChangeType(argsObj[i], args[i].Type);
                        }
                    }
                }
            }

            foreach (var item in iArgs)
            {
                if (null == defaultObj2[item.Position] && !item.Type.IsValueType)
                {
                    continue;
                }

                var iArg = (IArg)System.Activator.CreateInstance(item.Type);

                //Not entry for value type
                if (!(null == defaultObj2[item.Position] && item.IArgInType.IsValueType))
                {
                    iArg.In = defaultObj2[item.Position];
                }

                //iArg.In = defaultObj2[item.Position];
                iArg.Group = group;

                defaultObj2[item.Position] = iArg;
            }

            //foreach (var item in iArgs)
            //{
            //    var iArg = (IArg)System.Activator.CreateInstance(item.Type);

            //    if (!(null == defaultObj2[item.Position] && item.IArgInType.IsValueType))
            //    {
            //        iArg.In = defaultObj2[item.Position];
            //    }

            //    //iArg.In = defaultObj2[item.Position];
            //    iArg.Group = group;

            //    defaultObj2[item.Position] = iArg;
            //}

            return defaultObj2;
        }

        static ConcurrentReadOnlyDictionary<string, CommandAttribute> CmdAttrGroup(Configer cfg, string methodName, System.Collections.Generic.List<AttributeBase> attributes, string groupDefault, System.Collections.Generic.List<Ignore> ignore)
        {
            var group = new ConcurrentReadOnlyDictionary<string, CommandAttribute>();

            //ignore
            var ignores = ignore.Where(c => c.Mode == IgnoreMode.Method).ToList();

            var ignoreAll = ignores.Exists(c => string.IsNullOrWhiteSpace(c.Group));

            //ignore all
            if (ignoreAll)
            {
                attributes.FindAll(c => c is CommandAttribute).ForEach(c => attributes.Remove(c));

                return group;
            }

            var notMethods = new System.Collections.Generic.List<CommandAttribute>();

            var isDef = false;

            for (int i = attributes.Count - 1; i >= 0; i--)
            {
                var item = attributes[i];

                if (item is CommandAttribute)
                {
                    var item2 = item as CommandAttribute;
                    if (string.IsNullOrWhiteSpace(item2.Group)) { item2.Group = groupDefault; }
                    if (string.IsNullOrWhiteSpace(item2.OnlyName)) { item2.OnlyName = methodName; }

                    //ignore
                    if (0 < ignores.Count && ignores.Exists(c => c.Group == item2.Group))
                    {
                        attributes.Remove(item);
                    }
                    else
                    {
                        if (!isDef && item2.Group == groupDefault) { isDef = true; }

                        if (item2.Declaring != AttributeBase.DeclaringType.Method)
                        {
                            notMethods.Add(item2);

                            //if (!group.Any(c => c.Value.Source == AttributeBase.SourceType.Method && c.Value.Group == clone.Group))
                            //{
                            //    group.dictionary.TryAdd(cfg.GetCommandGroup(clone.Group, clone.OnlyName), clone);
                            //}
                            //group.dictionary.TryAdd(cfg.GetCommandGroup(clone.Group, clone.OnlyName), clone);
                        }
                        else
                        {
                            //group.dictionary.AddOrUpdate(cfg.GetCommandGroup(item2.Group, item2.OnlyName), item2, (key, oldValue) => oldValue.Source != AttributeBase.SourceType.Method ? item2 : oldValue);
                            var key = cfg.GetCommandGroup(item2.Group, item2.OnlyName);
                            item2.Key = key;
                            group.dictionary.TryAdd(key, item2);

                            //var command = group.FirstOrDefault(c => c.Value.Source != AttributeBase.SourceType.Method && c.Value.Group == item2.Group);
                            //if (!default(System.Collections.Generic.KeyValuePair<string, CommandAttribute>).Equals(command))
                            //{
                            //    attributes.Remove(command.Value);
                            //    group.dictionary.TryRemove(command.Key, out _);
                            //}
                        }
                    }
                }
            }

            foreach (var item in notMethods)
            {
                if (!group.Any(c => c.Value.Group == item.Group))
                {
                    var clone = item.Clone();
                    //clone.Source = AttributeBase.SourceType.Method;
                    clone.OnlyName = methodName;

                    var key = cfg.GetCommandGroup(clone.Group, clone.OnlyName);
                    clone.Key = key;
                    group.dictionary.TryAdd(key, clone);
                }
            }

            //foreach (var item in group)
            //{
            //    if (item.Value.Source != AttributeBase.SourceType.Method)
            //    {
            //        if (group.Any(c => c.Value.Source == AttributeBase.SourceType.Method && c.Value.Group == item.Value.Group))
            //        {
            //            attributes.Remove(item.Value);
            //            group.dictionary.TryRemove(item.Key, out _);
            //        }
            //    }
            //}

            //add default group
            /*if (!group.ContainsKey(groupDefault))*/// && methodName == c.OnlyName
            if (!isDef) //(!group.Values.Any(c => groupDefault == c.Group))
            {
                var key = cfg.GetCommandGroup(groupDefault, methodName);
                group.dictionary.TryAdd(key, new CommandAttribute(methodName) { Group = groupDefault, Key = key });
            }

            return group;
        }

        public static CommandGroup GetBusinessGroup(IBusiness business, ConcurrentReadOnlyDictionary<string, MetaData> metaData, System.Func<string, MethodInfo, MetaData, Command> action)
        {
            var group = new CommandGroup(business.Configer.ResultType, business.Configer.Info.CommandGroupDefault);

            //========================================//

            var proxyType = business.GetType();

#if DEBUG
            foreach (var item in metaData)
#else
            System.Threading.Tasks.Parallel.ForEach(metaData, item =>
#endif
            {
                var meta = item.Value;

                var method2 = proxyType.GetMethod(meta.Name);

                //set all
                foreach (var item2 in meta.CommandGroup)
                {
                    var groups = group.dictionary.GetOrAdd(item2.Value.Group, key => new ConcurrentReadOnlyDictionary<string, Command>());

                    if (!groups.dictionary.TryAdd(item2.Value.OnlyName, action(item2.Key, method2, meta)))
                    {
                        throw new System.Exception($"Command \"{item2.Key}\" member \"{item2.Value.OnlyName}\" name exists");
                    }
                }
#if DEBUG
            };
#else
            });
#endif

            //========================================//

            return group;
        }

        static readonly DynamicMethodBuilder dynamicMethodBuilder = new DynamicMethodBuilder();

        static CommandGroup GetBusinessCommand(IBusiness business)
        {
            //var routeValues = business.Configuration.Routes.Values;

            return GetBusinessGroup(business, business.Configer.MetaData, (key, method, meta) =>
            {
                //var key = business.Configer.GetCommandGroup(item.Group, item.OnlyName);//item.GetKey();//

                //var call = !meta.HasReturn && !meta.HasAsync ? (p, p1) =>
                //{
                //    MethodInvokerGenerator.CreateDelegate2(method, false, key)(p, p1); return null;
                //}
                //:
                //MethodInvokerGenerator.CreateDelegate<dynamic>(method, false, key);

                var call = dynamicMethodBuilder.GetDelegate(method) as System.Func<object, object[], dynamic>;
                /*
#region Routes

                if (null != routeValues)
                {
                    var values = routeValues.Where(c => c.MethodName == meta.Name && System.String.Equals(c.Group, item.Group, System.StringComparison.CurrentCultureIgnoreCase));
                    foreach (var item2 in values)
                    {
                        item2.Cmd = item.OnlyName;
                    }
                }

#endregion
                */

                //return new Command(arguments => call(business, GetArgsObj(meta.DefaultValue, arguments, meta.IArgs, key, meta.ArgAttrs[meta.GroupDefault].Args)), meta.Name, meta.HasReturn, meta.HasIResult, meta.ReturnType, meta.HasAsync, meta);
                return new Command(arguments => call(business, GetArgsObj(meta.DefaultValue, arguments, meta.IArgs, key, meta.Args)), meta, key);
                //, meta.ArgAttrs[Bind.GetDefaultCommandGroup(method.Name)].CommandArgs
            });
        }

        /*
        static System.Collections.Generic.IEqualityComparer<RouteAttribute> routeComparer = Equality<RouteAttribute>.CreateComparer(c => c.GetKey(true), System.StringComparer.CurrentCultureIgnoreCase);
        */

        static string GetMethodTypeFullName(string fullName, System.Collections.Generic.IList<Args> args) => string.Format("{0}{1}", fullName, (null == args || 0 == args.Count) ? null : string.Format("({0})", null == args ? null : string.Join(",", args.Select(c => c.MethodTypeFullName))));

        static string GetMethodTypeFullName(System.Type type)
        {
            if (null == type) { throw new System.ArgumentNullException(nameof(type)); }

            var name = type.FullName.Split('`')[0].Replace("+", ".");

            if (type.IsConstructedGenericType)
            {
                name = $"{name}{{{string.Join(", ", type.GenericTypeArguments.Select(c => GetMethodTypeFullName(c)))}}}";
            }

            return name;
        }

        static ConcurrentReadOnlyDictionary<string, MetaData> GetInterceptorMetaData(Configer cfg, System.Collections.Generic.Dictionary<int, MethodInfo> methods, object instance)
        {
            var metaData = new ConcurrentReadOnlyDictionary<string, MetaData>();

#if DEBUG
            foreach (var methodMeta in methods)
#else
            System.Threading.Tasks.Parallel.ForEach(methods, methodMeta =>
#endif
            {
                var method = methodMeta.Value;
                var space = method.DeclaringType.FullName;
                var attributes2 = AttributeBase.GetAttributes(method).Distinct(cfg.Attributes);

                var argAttrs = attributes2.GetAttrs<ArgumentAttribute>();

                //======LogAttribute======//
                var loggers = attributes2.GetAttrs<LoggerAttribute>();

                var ignores = attributes2.GetAttrs<Ignore>();

                //======CmdAttrGroup======//
                var commandGroup = CmdAttrGroup(cfg, method.Name, attributes2, cfg.Info.CommandGroupDefault, ignores);

                var parameters = method.GetParameters();

                var loggerGroup = new ConcurrentReadOnlyDictionary<string, MetaLogger>();

                var tokenPosition = new System.Collections.Generic.List<int>(parameters.Length);
                //var httpRequestPosition = new System.Collections.Generic.List<int>(parameters.Length);
                var useTypePosition = new ConcurrentReadOnlyDictionary<int, System.Type>();

                foreach (var item in commandGroup)
                {
                    loggerGroup.dictionary.TryAdd(item.Key, GetMetaLogger(loggers, item.Value.Group));
                }

                var args = new ReadOnlyCollection<Args>(parameters.Length);

                foreach (var argInfo in parameters)
                {
                    var path = argInfo.Name;
                    var parameterType = argInfo.ParameterType.GetTypeInfo();
                    //==================================//
                    var current = GetCurrentType(parameterType);
                    var currentType = current.outType;
                    var argAttrAll = AttributeBase.GetAttributes(argInfo, currentType);

                    var hasCollection = currentType.IsCollection();
                    if (hasCollection)
                    {
                        currentType = currentType.GenericTypeArguments[0];
                    }
                    //var use = current.hasIArg ? current.inType.GetAttribute<UseAttribute>() : argAttrAll.GetAttr<UseAttribute>();

                    var use = current.hasIArg ? current.inType.GetAttribute<UseAttribute>() ?? argAttrAll.GetAttr<UseAttribute>(c => c.ParameterName) : argAttrAll.GetAttr<UseAttribute>();

                    var hasUse = null != use || (current.hasIArg ? cfg.UseTypes.Contains(current.inType.FullName) : false);
                    //var nick = argAttrAll.GetAttr<NickAttribute>();

                    argAttrAll = argAttrAll.Distinct(!hasUse ? argAttrs : null);

                    //==================================//
                    var logAttrArg = argAttrAll.GetAttrs<LoggerAttribute>();
                    var inLogAttrArg = current.hasIArg ? AttributeBase.GetAttributes<LoggerAttribute>(current.inType, AttributeBase.DeclaringType.Parameter, GropuAttribute.Comparer) : null;

                    var hasDefinition = IsClass(currentType);

                    var definitions = hasDefinition ? new System.Collections.Generic.List<System.Type> { currentType } : new System.Collections.Generic.List<System.Type>();

                    var hasCollectionAttr = false;
                    if (hasCollection)
                    {
                        var collectionAttr = AttributeBase.GetCollectionAttributes(currentType);
                        if (0 < collectionAttr.Count)
                        {
                            hasCollectionAttr = true; //checked hasCollectionAttr 1
                            argAttrAll.AddRange(collectionAttr);
                        }
                    }

                    var argGroup = GetArgGroup(argAttrAll, current, path, default, commandGroup, cfg.ResultType, instance, hasUse, out _, out bool hasCollectionAttr2, logAttrArg, inLogAttrArg);

                    var hasLower = false;
                    var argAttrChild = hasUse && !current.hasIArg ? new ReadOnlyCollection<Args>(0) : hasDefinition ? GetArgAttr(currentType, path, commandGroup, ref definitions, cfg.ResultType, instance, cfg.UseTypes, out hasLower) : new ReadOnlyCollection<Args>(0);

                    args.collection.Add(new Args(argInfo.Name,
                    argInfo.ParameterType,
                    argInfo.Position,
                    argInfo.HasDefaultValue ? argInfo.DefaultValue : default,
                    argInfo.HasDefaultValue,
                    hasCollection,
                    hasCollectionAttr || hasCollectionAttr2,
                    hasCollection ? typeof(IArg<,>).GetTypeInfo().IsAssignableFrom(currentType, out _) : false,
                    default,
                    argGroup,
                    argAttrChild,
                    hasLower,
                    hasDefinition,
                    current.hasIArg,
                    current.hasIArg ? currentType : default,
                    current.hasIArg ? current.inType : default,
                    //path,
                    use,
                    hasUse,
                    //item.Value.CommandAttr.OnlyName,
                    GetMethodTypeFullName(parameterType),
                    currentType.FullName.Replace('+', '.'),
                    hasDefinition ? Args.ArgTypeCode.Definition : Args.ArgTypeCode.No));

                    if (hasUse)
                    {
                        useTypePosition.dictionary.TryAdd(argInfo.Position, current.inType);
                    }
                }

                //var groupDefault = cfg.GetCommandGroup(cfg.Info.CommandGroupDefault, method.Name);
                //var args = argAttrGroup.FirstOrDefault().Value.Args;//[groupDefault].Args;
                var fullName = method.GetMethodFullName();

                var meta = new MetaData(commandGroup, args, args?.Where(c => c.HasIArg).ToReadOnly(), loggerGroup, $"{space}.{method.Name}", method.Name, fullName, method.ReturnType.GetTypeInfo(), cfg.ResultType, GetDefaultValue(args), attributes2, methodMeta.Key, cfg.GetCommandGroup(cfg.Info.CommandGroupDefault, method.Name), useTypePosition, GetMethodTypeFullName(fullName, args));

                if (!metaData.dictionary.TryAdd(method.Name, meta))
                {
                    throw new System.Exception($"MetaData name exists \"{method.Name}\"");
                }
#if DEBUG
            };
#else
            });
#endif

            return metaData;
        }

        struct CurrentType { public bool hasIArg; public System.Type outType; public System.Type inType; }

        static CurrentType GetCurrentType(System.Type type)
        {
            var hasIArg = typeof(IArg<,>).GetTypeInfo().IsAssignableFrom(type, out System.Type[] iArgOutType);

            return new CurrentType { hasIArg = hasIArg, outType = hasIArg ? iArgOutType[0] : type, inType = hasIArg ? iArgOutType[1] : type };
        }

        #region GetArgAttr        

        internal static System.Collections.Generic.List<ArgumentAttribute> GetArgAttr(System.Collections.Generic.List<AttributeBase> attributes, System.Type memberType, System.Type resultType, dynamic business, string path)
        {
            var argAttr = attributes.Where(c => c is ArgumentAttribute).Cast<ArgumentAttribute>().ToList();
            GetArgAttrSort(argAttr);
            return Bind.GetArgAttr(argAttr, resultType, business, path, memberType);
        }

        internal static System.Collections.Generic.List<ArgumentAttribute> GetArgAttr(System.Collections.Generic.List<ArgumentAttribute> argAttr, System.Type resultType, dynamic business, string path, System.Type memberType)
        {
            //argAttr = argAttr.FindAll(c => c.Enable);
            //argAttr.Sort(ComparisonHelper<Attributes.ArgumentAttribute>.CreateComparer(c =>c.State.ConvertErrorState()));
            //argAttr.Reverse();

            var procesTypes = new System.Type[] { typeof(object) };
            var procesIArgTypes = new System.Type[] { typeof(object), typeof(IArg) };
            var procesIArgCollectionTypes = new System.Type[] { typeof(object), typeof(IArg), typeof(int) };
            var argumentAttributeFullName = typeof(ArgumentAttribute).FullName;

            foreach (var item in argAttr)
            {
                //if (string.IsNullOrWhiteSpace(item.Group)) { item.Group = groupDefault; }

                item.Meta.resultType = resultType;
                item.Meta.Business = business;
                item.Meta.Member = path;
                item.Meta.MemberType = memberType;
                //item.Meta.HasProcesIArg = !item.Type.GetMethod("Proces", BindingFlags.Public | BindingFlags.Instance, null, procesTypes, null).DeclaringType.FullName.Equals(argumentAttributeFullName);
                //item.Meta.HasProcesIArg = ArgumentAttribute.MetaData.ProcesMode.Proces; //item.Type.GetMethod("Proces", BindingFlags.Public | BindingFlags.Instance, null, procesTypes, null).DeclaringType.FullName.Equals(argumentAttributeFullName) ? ArgumentAttribute.MetaData.ProcesMode.Proces : item.Type.GetMethod("Proces", BindingFlags.Public | BindingFlags.Instance, null, procesIArgTypes, null).DeclaringType.FullName.Equals(argumentAttributeFullName)?ArgumentAttribute.MetaData.ProcesMode.ProcesIArg: ArgumentAttribute.MetaData.ProcesMode.ProcesIArgCollection;
                if (!item.Type.GetMethod("Proces", BindingFlags.Public | BindingFlags.Instance, null, procesIArgCollectionTypes, null).DeclaringType.FullName.Equals(argumentAttributeFullName))
                {
                    item.Meta.HasProcesIArg = ArgumentAttribute.MetaData.ProcesMode.ProcesIArgCollection;
                }
                else if (!item.Type.GetMethod("Proces", BindingFlags.Public | BindingFlags.Instance, null, procesIArgCollectionTypes, null).DeclaringType.FullName.Equals(argumentAttributeFullName))
                {
                    item.Meta.HasProcesIArg = ArgumentAttribute.MetaData.ProcesMode.ProcesIArgCollection;
                }
                //if (string.IsNullOrWhiteSpace(item.Nick) && !string.IsNullOrWhiteSpace(nick))
                //{
                //    item.Nick = nick;
                //}

                // replace
                //item.Message = Attributes.ArgumentAttribute.MemberReplace(item, item.Message);
                //item.Description = Attributes.ArgumentAttribute.MemberReplace(item, item.Description);

                //item.BindAfter?.Invoke();
            }

            return argAttr;
        }

        internal static void GetArgAttrSort(System.Collections.Generic.List<ArgumentAttribute> argAttr)
        {
            argAttr.Sort(ComparisonHelper<ArgumentAttribute>.CreateComparer(c => c.State.ConvertErrorState()));
            //argAttr.Reverse();
        }

        #endregion

        static ReadOnlyCollection<Args> GetArgAttr(System.Type type, string path, ConcurrentReadOnlyDictionary<string, CommandAttribute> commands, ref System.Collections.Generic.List<System.Type> definitions, System.Type resultType, object business, System.Collections.Generic.IList<string> useTypes, out bool hasLower)
        {
            hasLower = false;

            var args = new ReadOnlyCollection<Args>();

            var position = 0;

            var members = type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.GetField | BindingFlags.GetProperty);

            foreach (var item in members)
            {
                System.Type memberType = null;
                Accessor accessor = default;
                Args.ArgTypeCode argType = Args.ArgTypeCode.No;

                switch (item.MemberType)
                {
                    case MemberTypes.Field:
                        {
                            var member = item as FieldInfo;
                            accessor = member.GetAccessor();
                            if (null == accessor.Getter || null == accessor.Setter) { continue; }
                            memberType = member.FieldType;
                            argType = Args.ArgTypeCode.Field;
                        }
                        break;
                    case MemberTypes.Property:
                        {
                            var member = item as PropertyInfo;
                            accessor = member.GetAccessor();
                            if (null == accessor.Getter || null == accessor.Setter) { continue; }
                            memberType = member.PropertyType;
                            argType = Args.ArgTypeCode.Property;
                        }
                        break;
                    default: continue;
                }

                var current = GetCurrentType(memberType);

                var argAttrAll = AttributeBase.GetAttributes(item, current.outType);

                var hasCollection = current.outType.IsCollection();
                if (hasCollection)
                {
                    current.outType = current.outType.GenericTypeArguments[0];
                }

                var hasDefinition = IsClass(current.outType);

                if (definitions.Contains(current.outType)) { continue; }
                else if (hasDefinition) { definitions.Add(current.outType); }

                var path2 = $"{path}.{item.Name}";

                //var use = argAttrAll.GetAttr<UseAttribute>();
                var use = current.hasIArg ? current.inType.GetAttribute<UseAttribute>() ?? argAttrAll.GetAttr<UseAttribute>(c => c.ParameterName) : argAttrAll.GetAttr<UseAttribute>();
                var hasUse = null != use || (current.hasIArg ? useTypes.Contains(current.inType.FullName) : false);

                var hasCollectionAttr = false;
                if (hasCollection)
                {
                    var collectionAttr = AttributeBase.GetCollectionAttributes(current.outType);
                    if (0 < collectionAttr.Count)
                    {
                        hasCollectionAttr = true;//checked hasCollectionAttr 1
                        argAttrAll.AddRange(collectionAttr);
                    }
                }

                var argGroup = GetArgGroup(argAttrAll, current, path2, path, commands, resultType, business, hasUse, out bool hasLower2, out bool hasCollectionAttr2);

                var hasLower3 = false;
                var argAttrChild = hasDefinition ? GetArgAttr(current.outType, path2, commands, ref definitions, resultType, business, useTypes, out hasLower3) : new ReadOnlyCollection<Args>(0);

                if (hasLower2 || hasLower3)
                {
                    hasLower = true;
                }

                args.collection.Add(new Args(item.Name,
                    memberType,
                    position++,
                    default,
                    default,
                    hasCollection,
                    hasCollectionAttr || hasCollectionAttr2,
                    hasCollection ? typeof(IArg<,>).GetTypeInfo().IsAssignableFrom(current.outType, out _) : false,
                    accessor,
                    argGroup,
                    argAttrChild,
                    hasLower2 || hasLower3,
                    hasDefinition,
                    current.hasIArg,
                    current.hasIArg ? current.outType : default,
                    current.hasIArg ? current.inType : default,
                    //path2,
                    use,
                    hasUse,
                    GetMethodTypeFullName(memberType),
                    $"{type.FullName.Replace('+', '.')}.{item.Name}",
                    argType));
            }

            return args;
        }

        /// <summary>
        /// string.IsNullOrWhiteSpace(x.Group) || x.Group == group
        /// </summary>
        /// <param name="x"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        internal static bool GroupEquals(GropuAttribute x, string group) => string.IsNullOrWhiteSpace(x.Group) || x.Group == group;

        static ConcurrentReadOnlyDictionary<string, ArgGroup> GetArgGroup(System.Collections.Generic.List<AttributeBase> argAttrAll, CurrentType current, string path, string owner, ConcurrentReadOnlyDictionary<string, CommandAttribute> commands, System.Type resultType, object business, bool hasUse, out bool hasLower, out bool hasCollectionAttr, System.Collections.Generic.List<LoggerAttribute> log = null, System.Collections.Generic.List<LoggerAttribute> inLog = null)
        {
            hasLower = false;
            hasCollectionAttr = false;

            var argAttrs = GetArgAttr(argAttrAll, current.outType, resultType, business, path);

            var nick = argAttrAll.GetAttr<NickAttribute>();

            var argGroup = new ConcurrentReadOnlyDictionary<string, ArgGroup>();

            foreach (var item in commands)
            {
                var ignores = argAttrAll.GetAttrs<Ignore>(c => GroupEquals(c, item.Value.Group), clone: true);

                var ignoreBusinessArg = ignores.Any(c => c.Mode == IgnoreMode.BusinessArg);
                // || (item.Group == c.Group || string.IsNullOrWhiteSpace(c.Group))

                var argAttrChild = (hasUse || item.Value.IgnoreBusinessArg || ignoreBusinessArg) ?
                    argAttrs.FindAll(c => GroupEquals(c, item.Value.Group) && c.Declaring == AttributeBase.DeclaringType.Parameter).Select(c => c.Clone()).ToList() :
                    argAttrs.FindAll(c => GroupEquals(c, item.Value.Group)).Select(c => c.Clone()).ToList();

                var nickValue = string.IsNullOrWhiteSpace(nick?.Nick) ? argAttrChild.Where(c => !string.IsNullOrWhiteSpace(c.Nick) && GroupEquals(c, item.Value.Group)).GroupBy(c => c.Nick, System.StringComparer.InvariantCultureIgnoreCase).FirstOrDefault()?.Key : nick.Nick;

                var argAttr = new ConcurrentLinkedList<ArgumentAttribute>();//argAttrChild.Count

                var path2 = $"{item.Value.OnlyName}.{path}";

                foreach (var c in argAttrChild)
                {
                    var attr = string.IsNullOrWhiteSpace(c.Group) ? c.Clone() : c;

                    attr.Meta.Method = item.Value.OnlyName;
                    attr.Meta.Member = path2;

                    attr.Meta.Business = c.Meta.Business;
                    attr.Meta.resultType = c.Meta.resultType;
                    attr.Meta.MemberType = c.Meta.MemberType;
                    attr.Meta.HasProcesIArg = c.Meta.HasProcesIArg;

                    if (string.IsNullOrWhiteSpace(attr.Nick))
                    {
                        attr.Nick = !string.IsNullOrWhiteSpace(nickValue) ? nickValue : attr.Meta.Member;
                    }

                    attr.BindAfter?.Invoke();

                    if (!argAttr.TryAdd(attr))
                    {
                        System.Console.WriteLine("ConcurrentLinkedList TryAdd error! State = INV");
                    }

                    if (!hasLower) { hasLower = true; }

                    if (c.CollectionItem && !hasCollectionAttr) { hasCollectionAttr = true; }//checked hasCollectionAttr 2
                }

                if (default == owner)
                {
                    //add default convert
                    //if (current.hasIArg && 0 == argAttr.Count)
                    if (current.hasIArg && null == argAttr.First.Value)
                    {
                        argAttr.TryAdd(new ArgumentDefaultAttribute(resultType) { Declaring = AttributeBase.DeclaringType.Parameter });

                        if (!hasLower) { hasLower = true; }
                    }
                }

                var group = new ArgGroup(ignores.ToReadOnly(), argAttr, nickValue, path2, default == owner ? item.Value.OnlyName : $"{item.Value.OnlyName}.{owner}");

                if (0 < log?.Count)
                {
                    group.Logger = GetMetaLogger(log, item.Value.Group);
                }

                if (0 < inLog?.Count)
                {
                    group.IArgInLogger = GetMetaLogger(inLog, item.Value.Group);
                }

                argGroup.dictionary.TryAdd(item.Key, group);
            }

            return argGroup;
        }

        #endregion
    }

    public class CommandGroup : ConcurrentReadOnlyDictionary<string, ConcurrentReadOnlyDictionary<string, Command>>
    {
        readonly System.Type resultType;

        readonly string groupDefault;

        public CommandGroup(System.Type resultType, string groupDefault)
        {
            this.resultType = resultType;
            this.groupDefault = groupDefault;
        }

        public virtual Command GetCommand(string cmd, string group = null)
        {
            if (string.IsNullOrEmpty(cmd))
            {
                return null;
            }

            return !TryGetValue(string.IsNullOrWhiteSpace(group) ? groupDefault : group, out ConcurrentReadOnlyDictionary<string, Command> cmdGroup) || !cmdGroup.TryGetValue(cmd, out Command command) ? null : command;
        }

        #region Call

        public virtual Result Call<Result>(string cmd, object[] args = null, UseEntry[] useObj = null, string group = null) => Call(cmd, args, useObj, group);

        public virtual IResult CallIResult(string cmd, object[] args = null, UseEntry[] useObj = null, string group = null) => Call(cmd, args, useObj, group);

        public virtual dynamic Call(string cmd, object[] args = null, UseEntry[] useObj = null, string group = null)
        {
            var command = GetCommand(cmd, group);

            return null == command ? Bind.CmdError(resultType, cmd) : command.Call(args, useObj);
        }

        public virtual Result Call<Result>(string cmd, object[] args = null, string group = null, params UseEntry[] useObj) => Call(cmd, args, useObj, group);

        public virtual IResult CallIResult(string cmd, object[] args = null, string group = null, params UseEntry[] useObj) => Call(cmd, args, useObj, group);

        public virtual dynamic Call(string cmd, object[] args = null, string group = null, params UseEntry[] useObj) => Call(cmd, args, useObj, group);

        #endregion

        #region AsyncCallUse

        public virtual async System.Threading.Tasks.Task<Result> AsyncCall<Result>(string cmd, object[] args = null, UseEntry[] useObj = null, string group = null) => await AsyncCall(cmd, args, useObj, group);

        public virtual async System.Threading.Tasks.Task<IResult> AsyncIResult(string cmd, object[] args = null, UseEntry[] useObj = null, string group = null) => await AsyncCall(cmd, args, useObj, group);

        public virtual async System.Threading.Tasks.Task<dynamic> AsyncCall(string cmd, object[] args = null, UseEntry[] useObj = null, string group = null)
        {
            var command = GetCommand(cmd, group);

            return null == command ? await System.Threading.Tasks.Task.FromResult(Bind.CmdError(resultType, cmd)) : await command.AsyncCall(args, useObj);
        }

        public virtual async System.Threading.Tasks.Task<IResult> AsyncIResult(string cmd, object[] args = null, string group = null, params UseEntry[] useObj) => await AsyncCall(cmd, args, useObj, group);

        public virtual async System.Threading.Tasks.Task<Result> AsyncCall<Result>(string cmd, object[] args = null, string group = null, params UseEntry[] useObj) => await AsyncCall(cmd, args, useObj, group);

        public virtual async System.Threading.Tasks.Task<dynamic> AsyncCall(string cmd, object[] args = null, string group = null, params UseEntry[] useObj) => await AsyncCall(cmd, args, useObj, group);

        #endregion
    }

    public class Command
    {
        public Command(System.Func<object[], dynamic> call, MetaData meta, string key)
        {
            this.call = call;
            this.Meta = meta;
            this.Key = key;
        }

        //===============member==================//
        readonly System.Func<object[], dynamic> call;

        public virtual object[] GetAgs(object[] args, params UseEntry[] useObj)
        {
            var args2 = new object[Meta.Args.Count];

            if (0 < args2.Length)
            {
                int l = 0;
                for (int i = 0; i < args2.Length; i++)
                {
                    if (Meta.UseTypePosition.ContainsKey(i))
                    {
                        if (0 < useObj?.Length)
                        {
                            var arg = Meta.Args[i];

                            if (arg.Use?.ParameterName ?? false)
                            {
                                foreach (var use in useObj)
                                {
                                    if (use.ParameterName?.Contains(arg.Name, System.StringComparer.InvariantCultureIgnoreCase) ?? false)
                                    {
                                        args2[i] = use.Value;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                foreach (var use in useObj)
                                {
                                    if (Meta.UseTypePosition[i].IsAssignableFrom(use.Type))
                                    {
                                        args2[i] = use.Value;
                                        break;
                                    }
                                }
                            }
                        }

                        continue;
                    }

                    if (null != args && 0 < args.Length)
                    {
                        if (args.Length < l++)
                        {
                            break;
                        }

                        if (l - 1 < args.Length)
                        {
                            args2[i] = args[l - 1];
                        }
                    }
                }
            }

            return args2;
        }

        #region Call

        public virtual dynamic Call(object[] args, params UseEntry[] useObj)
        {
            try
            {
                return call(GetAgs(args, useObj));
            }
            catch (System.Exception ex)
            {
                return ResultFactory.ResultCreate(Meta.ResultType, 0, System.Convert.ToString(Help.ExceptionWrite(ex)));
            }
        }
        public virtual Result Call<Result>(object[] args, params UseEntry[] useObj) => Call(args, useObj);
        public virtual IResult CallIResult(object[] args, params UseEntry[] useObj) => Call(args, useObj);

        #endregion

        #region AsyncCall

        public virtual async System.Threading.Tasks.Task<dynamic> AsyncCall(object[] args, params UseEntry[] useObj)
        {
            try
            {
                if (Meta.HasAsync)
                {
                    return await call(GetAgs(args, useObj));
                }
                else
                {
                    using (var task = System.Threading.Tasks.Task.Factory.StartNew(obj => { var obj2 = (dynamic)obj; return obj2.call(obj2.args); }, new { call, args = GetAgs(args, useObj) })) { return await task; }
                }
            }
            catch (System.Exception ex)
            {
                return await System.Threading.Tasks.Task.FromResult(ResultFactory.ResultCreate(Meta.ResultType, 0, System.Convert.ToString(Help.ExceptionWrite(ex))));
            }
        }

        public virtual async System.Threading.Tasks.Task<Result> AsyncCall<Result>(object[] args, params UseEntry[] useObj) => await AsyncCall(args, useObj);

        public virtual async System.Threading.Tasks.Task<IResult> AsyncIResult(object[] args, params UseEntry[] useObj) => await AsyncCall(args, useObj);

        #endregion

        public readonly string Key;

        public MetaData Meta { get; private set; }
    }
}

namespace Business.Meta
{
    using Business.Utils;
    using System.Reflection;

    #region Meta

    public class ArgGroup
    {
        public ArgGroup(ReadOnlyCollection<Attributes.Ignore> ignore, ConcurrentLinkedList<Attributes.ArgumentAttribute> attrs, string nick, string path, string owner)
        {
            Ignore = ignore;
            Attrs = attrs;
            Path = path;
            Nick = nick;
            Owner = owner;
            Logger = default;
            IArgInLogger = default;
        }

        public ReadOnlyCollection<Attributes.Ignore> Ignore { get; private set; }

        public ConcurrentLinkedList<Attributes.ArgumentAttribute> Attrs { get; private set; }

        public string Nick { get; private set; }

        public string Path { get; private set; }

        public string Owner { get; private set; }

        public MetaLogger Logger { get; internal set; }

        public MetaLogger IArgInLogger { get; internal set; }
    }

    /// <summary>
    /// Argument
    /// </summary>
    public class Args
    {
        //public override string ToString() => string.Format("{0} {1}", Group2, Name);

        //argChild
        public Args(string name, System.Type type, int position, object defaultValue, bool hasDefaultValue, bool hasCollection, bool hasCollectionAttr, bool hasCollectionIArg, Accessor accessor, ConcurrentReadOnlyDictionary<string, ArgGroup> group, ReadOnlyCollection<Args> argAttrChild, bool hasLower, bool hasDefinition, bool hasIArg, System.Type iArgOutType, System.Type iArgInType, Attributes.UseAttribute use, bool useType, string methodTypeFullName, string argTypeFullName, ArgTypeCode argType)
        {
            Name = name;
            Type = type;
            Position = position;
            HasCollection = hasCollection;
            HasCollectionAttr = hasCollectionAttr;
            HasCollectionIArg = hasCollectionIArg;
            //HasString = hasString;
            Accessor = accessor;
            Group = group;
            ArgAttrChild = argAttrChild;
            HasLower = hasLower;
            HasDefinition = hasDefinition;
            HasIArg = hasIArg;
            IArgOutType = iArgOutType;
            IArgInType = iArgInType;
            //this.trim = trim;
            //Path = path;
            //Source = source;

            DefaultValue = defaultValue;// type.GetTypeInfo().IsValueType ? System.Activator.CreateInstance(type) : null;
            HasDefaultValue = hasDefaultValue;
            //Logger = default;
            //IArgInLogger = default;
            //Group2 = group2;
            //Owner = owner;
            //Ignore = ignore;
            Use = use;
            UseType = useType;
            MethodTypeFullName = methodTypeFullName;
            ArgTypeFullName = argTypeFullName;
            ArgType = argType;
        }

        //===============name==================//
        public string Name { get; private set; }
        //===============type==================//
        public System.Type Type { get; private set; }
        //===============position==================//
        public int Position { get; private set; }
        //===============defaultValue==================//
        public object DefaultValue { get; private set; }
        //===============hasDefaultValue==================//
        public bool HasDefaultValue { get; private set; }
        //===============hasCollection==================//
        public bool HasCollection { get; private set; }
        //===============hasCollectionAttr==================//
        public bool HasCollectionAttr { get; internal set; }
        //===============hasCollectionIArg==================//
        public bool HasCollectionIArg { get; internal set; }
        //===============accessor==================//
        public Accessor Accessor { get; private set; }
        //===============group==================//
        public ConcurrentReadOnlyDictionary<string, ArgGroup> Group { get; private set; }
        ////===============argAttr==================//
        //public SafeList<Attributes.ArgumentAttribute> ArgAttr { get; private set; }
        //===============argAttrChild==================//
        public ReadOnlyCollection<Args> ArgAttrChild { get; private set; }
        //===============hasLower==================//
        public bool HasLower { get; internal set; }
        //===============hasDefinition==================//
        public bool HasDefinition { get; private set; }
        //===============iArgOutType==================//
        public System.Type IArgOutType { get; private set; }
        //===============iArgInType==================//
        public System.Type IArgInType { get; private set; }
        ////==============path===================//
        //public string Path { get; private set; }
        ////==============source===================//
        //public string Source { get; private set; }
        //===============hasIArg==================//
        public bool HasIArg { get; private set; }
        //public MetaLogger Logger { get; private set; }
        //public MetaLogger IArgInLogger { get; private set; }
        //==============group===================//
        //public string Group2 { get; private set; }
        //==============owner===================//
        //public string Owner { get; private set; }
        ////==============ignore===================//
        //public ReadOnlyCollection<Attributes.Ignore> Ignore { get; private set; }
        //==============use===================//
        public Attributes.UseAttribute Use { get; internal set; }
        //==============useType===================//
        public bool UseType { get; internal set; }
        //==============methodTypeFullName===================//
        public string MethodTypeFullName { get; private set; }
        //==============argTypeFullName===================//
        public string ArgTypeFullName { get; private set; }
        //==============argType===================//
        /// <summary>
        /// xml using
        /// </summary>
        public ArgTypeCode ArgType { get; private set; }

        public enum ArgTypeCode
        {
            No,
            Definition,
            Field,
            Property,
        }
    }

    public struct MetaLogger
    {
        public Attributes.LoggerAttribute Record { get; set; }
        public Attributes.LoggerAttribute Error { get; set; }
        public Attributes.LoggerAttribute Exception { get; set; }
    }

    public struct MetaData
    {
        public override string ToString() => Name;

        /// <summary>
        /// MetaData
        /// </summary>
        /// <param name="commandGroup"></param>
        /// <param name="args"></param>
        /// <param name="iArgs"></param>
        /// <param name="metaLogger"></param>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <param name="fullName"></param>
        /// <param name="returnType"></param>
        /// <param name="resultType"></param>
        /// <param name="defaultValue"></param>
        /// <param name="attributes"></param>
        /// <param name="position"></param>
        /// <param name="groupDefault"></param>
        /// <param name="useTypePosition"></param>
        /// <param name="methodTypeFullName"></param>
        public MetaData(ConcurrentReadOnlyDictionary<string, Attributes.CommandAttribute> commandGroup, ReadOnlyCollection<Args> args, ReadOnlyCollection<Args> iArgs, ConcurrentReadOnlyDictionary<string, MetaLogger> metaLogger, string path, string name, string fullName, TypeInfo returnType, System.Type resultType, object[] defaultValue, System.Collections.Generic.List<Attributes.AttributeBase> attributes, int position, string groupDefault, ConcurrentReadOnlyDictionary<int, System.Type> useTypePosition, string methodTypeFullName)
        {
            CommandGroup = commandGroup;
            Args = args;
            IArgs = iArgs;
            MetaLogger = metaLogger;
            Path = path;
            Name = name;
            FullName = fullName;

            //this.returnType = returnType;
            //this.hasAsync = Utils.Help.IsAssignableFrom(typeof(System.Threading.Tasks.Task<>).GetTypeInfo(), returnType, out System.Type[] arguments) || typeof(System.Threading.Tasks.Task).IsAssignableFrom(returnType);
            //typeof(void) != method.ReturnType
            HasAsync = Help.IsAssignableFrom(typeof(System.Threading.Tasks.Task<>).GetTypeInfo(), returnType, out System.Type[] arguments) || returnType == typeof(System.Threading.Tasks.Task);
            HasReturn = !(typeof(void) == returnType || (HasAsync && null == arguments));
            //typeof(IResult).IsAssignableFrom(method.ReturnType),
            //typeof(System.Object).Equals(method.ReturnType)
            var hasGeneric = HasAsync && null != arguments;
            HasIResult = typeof(Result.IResult).IsAssignableFrom(hasGeneric ? arguments[0] : returnType);
            HasObject = typeof(object).Equals(hasGeneric ? arguments[0] : returnType);
            ReturnType = hasGeneric ? arguments[0] : returnType;
            ResultType = resultType;
            DefaultValue = defaultValue;
            //this.logAttrs = logAttrs;
            Attributes = attributes;
            Position = position;
            GroupDefault = groupDefault;
            //ArgsFirst = argsFirst;
            UseTypePosition = useTypePosition;
            MethodTypeFullName = methodTypeFullName;
            //Ignore = ignore;
        }

        //==============commandAttr===================//
        public ConcurrentReadOnlyDictionary<string, Attributes.CommandAttribute> CommandGroup { get; private set; }
        //==============argAttrs===================//
        public ReadOnlyCollection<Args> Args { get; private set; }
        //==============iArgs===================//
        public ReadOnlyCollection<Args> IArgs { get; private set; }
        //==============MetaLogger===================//
        public ConcurrentReadOnlyDictionary<string, MetaLogger> MetaLogger { get; private set; }
        //==============path===================//
        public string Path { get; private set; }
        //==============name===================//
        public string Name { get; private set; }
        //==============fullName===================//
        public string FullName { get; private set; }
        //==============hasReturn===================//
        public bool HasReturn { get; private set; }
        //==============hasIResult===================//
        public bool HasIResult { get; private set; }
        //==============hasObject===================//
        public bool HasObject { get; private set; }
        //==============returnType===================//
        public System.Type ReturnType { get; private set; }
        //==============resultType===================//
        public System.Type ResultType { get; private set; }
        //==============hasAsync===================//
        public bool HasAsync { get; private set; }
        //==============defaultValue===================//
        public object[] DefaultValue { get; private set; }
        //==============attributes===================//
        public System.Collections.Generic.List<Attributes.AttributeBase> Attributes { get; private set; }
        //===============position==================//
        public int Position { get; private set; }
        //==============groupDefault===================//
        public string GroupDefault { get; private set; }
        ////==============argsFirst===================//
        //public ReadOnlyCollection<Args> ArgsFirst { get; private set; }
        //==============useTypesPosition===================//
        public ConcurrentReadOnlyDictionary<int, System.Type> UseTypePosition { get; private set; }
        //==============methodTypeFullName===================//
        public string MethodTypeFullName { get; private set; }
        ////==============ignore===================//
        //public Attributes.Ignore Ignore { get; private set; }
    }

    #endregion
}
