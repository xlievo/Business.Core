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
    using Business.Request;
    using Business.Meta;

    public partial class Bind
    {
        internal static IResult CmdError(System.Type resultType, string cmd) => resultType.ResultCreate(-1, string.Format("Without this Cmd {0}", cmd));

        /// <summary>
        /// Default
        /// </summary>
        public const string CommandGroupDefault = "Default";

        #region Internal

        internal static System.Func<string, string> GetCommandGroupDefault = name => GetCommandGroup(CommandGroupDefault, name);

        internal static System.Func<string, string, string> GetCommandGroup = (group, name) => string.Format("{0}.{1}", group, name);

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

        internal static object GetReturnValue(int state, string message, MetaData meta, System.Type resultType) => (meta.HasIResult || meta.HasObject) ? resultType.ResultCreate(state, message) : meta.HasReturn && meta.ReturnType.IsValueType ? System.Activator.CreateInstance(meta.ReturnType) : null;

        // : meta.ReturnType.Equals(typeof(string)) ? message
        internal static object GetReturnValue(IResult result, MetaData meta)
        {
            var result2 = (meta.HasIResult || meta.HasObject) ? result : meta.HasReturn && meta.ReturnType.IsValueType ? System.Activator.CreateInstance(meta.ReturnType) : null;

            //if (meta.HasAsync)
            //{
            //    return meta.HasIResult ? System.Threading.Tasks.Task.Run(() => (IResult)result2) : meta.HasReturn ? System.Threading.Tasks.Task.Run(() => result2) : System.Threading.Tasks.Task.Run(() => { });
            //}
            if (meta.HasAsync)
            {
                return meta.HasIResult ? System.Threading.Tasks.Task.FromResult((IResult)result2) : meta.HasReturn ? System.Threading.Tasks.Task.FromResult(result2) : System.Threading.Tasks.Task.Run(() => { });
            }

            return result2;
        }

        internal static System.Collections.Generic.List<Attributes.ArgumentAttribute> GetArgAttr(System.Collections.Generic.List<Attributes.ArgumentAttribute> argAttr, System.Type resultType, dynamic business, string path, System.Type memberType)
        {
            //argAttr = argAttr.FindAll(c => c.Enable);
            argAttr.Sort(ComparisonHelper<Attributes.ArgumentAttribute>.CreateComparer(c => ResultFactory.GetState(c.State)));
            argAttr.Reverse();
            foreach (var item in argAttr)
            {
                if (System.String.IsNullOrWhiteSpace(item.Group)) { item.Group = Bind.CommandGroupDefault; }

                item.resultType = resultType;
                item.Business = business;
                item.Member = path;
                item.MemberType = memberType;
                // replace
                //item.Message = Attributes.ArgumentAttribute.MemberReplace(item, item.Message);
                //item.Description = Attributes.ArgumentAttribute.MemberReplace(item, item.Description);

                item.BindAfter?.Invoke();
            }

            return argAttr;
        }

        public static ConcurrentReadOnlyDictionary<string, IBusiness> BusinessList = new ConcurrentReadOnlyDictionary<string, IBusiness>(new System.Collections.Concurrent.ConcurrentDictionary<string, IBusiness>(System.StringComparer.InvariantCultureIgnoreCase));

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

        public static void UseType(params System.Type[] type)
        {
            foreach (var item in Bind.BusinessList.Values)
            {
                item.Configuration.UseType(type);

                //foreach (var item2 in item.Configuration.MetaData.Values)
                System.Threading.Tasks.Parallel.ForEach(item.Configuration.MetaData.Values, item2 =>
                {
                    foreach (var item3 in item2.ArgAttrs[item2.GroupDefault].Args)
                    {
                        var type2 = item3.HasIArg ? item3.IArgInType : item3.Type;
                        if (item.Configuration.UseTypes.Contains(type2.FullName))
                        {
                            item2.UseTypePosition.dictionary.TryAdd(item3.Position, type2);
                        }
                    }
                });
            }
        }

        #endregion
    }

    class BusinessAllMethodsHook : Castle.DynamicProxy.AllMethodsHook
    {
        readonly MethodInfo[] ignoreMethods;

        public BusinessAllMethodsHook(params MethodInfo[] method)
            : base() { this.ignoreMethods = method; }

        public override bool ShouldInterceptMethod(System.Type type, MethodInfo methodInfo)
        {
            if (System.Array.Exists(ignoreMethods, c => System.String.Equals(c.GetMethodFullName(), methodInfo.GetMethodFullName()))) { return false; }

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

            var options = new Castle.DynamicProxy.ProxyGenerationOptions(new BusinessAllMethodsHook(methods.Item1));
            var proxy = new Castle.DynamicProxy.ProxyGenerator();

            try
            {
                //Castle.DynamicProxy.ProxyUtil.IsAccessible();
                Instance = proxy.CreateClassProxy(type, options, constructorArguments, interceptor);
            }
            catch (System.Exception ex)
            {
                throw ex.ExceptionWrite(true, true);
            }

            var generics = typeof(IBusiness<>).IsAssignableFrom(type.GetTypeInfo(), out System.Type[] businessArguments);

            //var requestType = generics ? businessArguments[0].GetGenericTypeDefinition() : typeof(RequestObject<string>).GetGenericTypeDefinition();
            var resultType = generics ? businessArguments[0].GetGenericTypeDefinition() : typeof(ResultObject<string>).GetGenericTypeDefinition();
            //var token = generics ? ConstructorInvokerGenerator.CreateDelegate<Auth.IToken>(businessArguments[1]) : () => new Auth.Token();

            //var resultType = ((typeof(IResult<>).GetTypeInfo().IsAssignableFrom(typeof(Result).GetTypeInfo(), out _) ? typeof(Result) : typeof(ResultObject<>)).GetGenericTypeDefinition()).GetTypeInfo();

            //var requestType = ((typeof(IRequest<>).GetTypeInfo().IsAssignableFrom(typeof(Request).GetTypeInfo(), out _) ? typeof(Request) : typeof(RequestObject<>)).GetGenericTypeDefinition()).GetTypeInfo();

            var attributes = typeInfo.GetAttributes();
            //Help.GetAttr(attributes, Equality<Attributes.LoggerAttribute>.CreateComparer(c => c.LogType));
            //Help.GetAttr(attributes, Equality<Attributes.RouteAttribute>.CreateComparer(c => new { c.Path, c.Verbs, c.Group }));

            #region LoggerAttribute

            var loggerBase = AssemblyAttr(typeInfo.Assembly, logComparer);

            foreach (var item in loggerBase)
            {
                if (!attributes.Any(c => c is Attributes.LoggerAttribute && ((Attributes.LoggerAttribute)c).LogType == item.LogType))
                {
                    attributes.Add(item.Clone<Attributes.LoggerAttribute>());
                }
            }

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
            var info = typeInfo.GetAttribute<Attributes.InfoAttribute>() ?? new Attributes.InfoAttribute(type.Name);

            if (System.String.IsNullOrWhiteSpace(info.BusinessName))
            {
                info.BusinessName = type.Name;
            }

            //var info = typeInfo.GetAttribute<Attributes.InfoAttribute>();
            //if (null != info)
            //{
            //    if (System.String.IsNullOrWhiteSpace(info.BusinessName))
            //    {
            //        info.BusinessName = type.Name;
            //    }
            //}
            //else
            //{
            //    info = new Attributes.InfoAttribute(type.Name);
            //}

            //if (routes.Values.Any(c => System.String.Equals(c.Path, info.BusinessName, System.StringComparison.InvariantCultureIgnoreCase) && c.Root))
            //{
            //    throw new System.Exception(string.Format("Route path exists \"{0}\"", info.BusinessName));
            //}


            var business = typeof(IBusiness).IsAssignableFrom(type) ? (IBusiness)Instance : null;
#if !Mobile
            //var cfg = new Configer.Configuration(info, resultType, attributes, typeInfo.GetAttributes<Attributes.EnableWatcherAttribute>().Exists());
#else
            
#endif
            var cfg = new Configer.Configuration(info, resultType, attributes);

            if (null != business && null != business.Config)
            {
                business.Config(cfg);
            }

            interceptor.MetaData = GetInterceptorMetaData(cfg, methods.Item2, Instance);

            interceptor.ResultType = cfg.ResultType;

            if (null != business)
            {
                //var info = typeInfo.GetAttribute<Attributes.InfoAttribute>();
                //if (null != info)
                //{
                //    if (System.String.IsNullOrWhiteSpace(info.BusinessName))
                //    {
                //        info.BusinessName = type.Name;
                //    }
                //}
                //else
                //{
                //    info = new Attributes.InfoAttribute(type.Name);
                //}

                //if (routes.Values.Any(c => System.String.Equals(c.Path, info.BusinessName, System.StringComparison.InvariantCultureIgnoreCase) && c.Root))
                //{
                //    throw new System.Exception(string.Format("Route path exists \"{0}\"", info.BusinessName));
                //}

                //var requestDefault = (IRequest)System.Activator.CreateInstance(requestType.MakeGenericType(typeof(object)));
                //requestDefault.Business = business;
                cfg.MetaData = interceptor.MetaData;

                business.Configuration = cfg;

                business.Command = GetBusinessCommand(business);

                interceptor.Logger = business.Logger;

                Bind.BusinessList.dictionary.TryAdd(business.Configuration.Info.BusinessName, business);
                //Bind.BusinessList.dictionary.TryAdd(business.Configuration.Info.BusinessName, new ConcurrentReadOnlyCollection<IBusiness> { business });
                /*
#region HttpAttribute

                var httpBase = typeInfo.Assembly.GetCustomAttribute<HttpAttribute>();
                if (null != httpBase)
                {
                    var http = attributes.FirstOrDefault(c => c is HttpAttribute) as HttpAttribute;
                    if (null == http)
                    {
                        attributes.Add(httpBase.Clone<HttpAttribute>());
                    }
                    else if (!http.defaultConstructor)
                    {
                        http.Set(httpBase.Host, httpBase.AllowedOrigins, httpBase.AllowedMethods, httpBase.AllowedHeaders, httpBase.AllowCredentials, httpBase.ResponseContentType, httpBase.Description);
                    }
                }

#endregion
                */
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

                Bind.BusinessList.dictionary.TryRemove(business.Configuration.ID, out _);
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

        static System.Tuple<MethodInfo[], System.Collections.Generic.Dictionary<int, MethodInfo>> GetMethods(TypeInfo type)
        {
            var ignoreList = new System.Collections.Generic.List<MethodInfo>();
            var list = new System.Collections.Generic.List<MethodInfo>();

            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public).Where(c => c.IsVirtual && !c.IsFinal);

            //var methods = type.DeclaredMethods.Where(c => c.IsVirtual && !c.IsFinal && c.IsPublic);

            foreach (var item in methods)
            {
                if (item.GetAttributes<Attributes.IgnoreAttribute>().Exists())
                {
                    ignoreList.Add(item);
                }
                else if (item.DeclaringType.Equals(type))
                {
                    list.Add(item);
                }
            }

            //Property
            foreach (var item in type.DeclaredProperties)
            {
                if (item.GetAttributes<Attributes.IgnoreAttribute>().Exists())
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

            var i = 0;
            //return (ignoreList.ToArray(), list.ToDictionary(c => i++, c => c));
            return System.Tuple.Create(ignoreList.ToArray(), list.ToDictionary(c => i++, c => c));
        }

        static System.Collections.Generic.List<T> AssemblyAttr<T>(Assembly member, System.Collections.Generic.IEqualityComparer<T> comparer) where T : System.Attribute => member.GetCustomAttributes<T>().Distinct(comparer).ToList();

        //static System.Collections.Generic.List<T> LoggerAttr<T>(ParameterInfo member, System.Type type, System.Collections.Generic.IEqualityComparer<T> comparer) where T : System.Attribute
        //{
        //    var attrs = new System.Collections.Generic.List<T>(member.GetAttributes<T>());
        //    attrs.AddRange(type.GetAttributes<T>());
        //    return attrs.Distinct(comparer).ToList();
        //}

        static System.Collections.Generic.List<T> LoggerAttr<T>(System.Type type, System.Collections.Generic.IEqualityComparer<T> comparer, ParameterInfo member = null) where T : System.Attribute
        {
            var attrs = new System.Collections.Generic.List<T>(type.GetAttributes<T>());
            if (null != member)
            {
                attrs.AddRange(member.GetAttributes<T>());
            }
            return attrs.Distinct(comparer).ToList();
        }

        /*
        static System.Collections.Generic.List<Attributes.LoggerAttribute> LoggerAttr(MemberInfo member)
        {
            return member.GetAttributes<Attributes.LoggerAttribute>().Distinct(Equality<Attributes.LoggerAttribute>.CreateComparer(c => c.LogType)).ToList();
        }
        */
        /*
        static System.Collections.Generic.List<Attributes.LoggerAttribute> LoggerAttr(System.Collections.Generic.List<Attributes.AttributeBase> attributes)
        {
            var all = attributes.FindAll(c => c is Attributes.LoggerAttribute).Cast<Attributes.LoggerAttribute>();

            var attrs = all.Distinct(Equality<Attributes.LoggerAttribute>.CreateComparer(c => c.LogType)).ToList();

            all.Except(attrs).ToList().ForEach(c => attributes.Remove(c));

            return attrs;
        }
        */

        static MetaLogger GetMetaLogger(System.Collections.Generic.List<Attributes.LoggerAttribute> methodLoggerAttr, System.Collections.Generic.List<Attributes.LoggerAttribute> businessLoggerAttr, System.Collections.Generic.List<Attributes.AttributeBase> attributes)
        {
            var metaLogger = new MetaLogger();

            foreach (var item in businessLoggerAttr)
            {
                if (!methodLoggerAttr.Exists(c => c.LogType == item.LogType))
                {
                    var log = item.Clone<Attributes.LoggerAttribute>();
                    methodLoggerAttr.Add(log);
                    attributes.Add(log);
                }
            }

            foreach (var item in methodLoggerAttr)
            {
                switch (item.LogType)
                {
                    case LoggerType.Record: metaLogger.Record = item; break;
                    case LoggerType.Error: metaLogger.Error = item; break;
                    case LoggerType.Exception: metaLogger.Exception = item; break;

                    default:
                        /*
                        if ((item.LogType & LoggerType.Record) != 0 && null == metaLogger.Record)
                        {
                            metaLogger.Record = item.Clone<Attributes.LoggerAttribute>().SetLogType(LoggerType.Record);
                        }
                        if ((item.LogType & LoggerType.Error) != 0 && null == metaLogger.Error)
                        {
                            metaLogger.Error = item.Clone<Attributes.LoggerAttribute>().SetLogType(LoggerType.Error);
                        }
                        if ((item.LogType & LoggerType.Exception) != 0 && null == metaLogger.Exception)
                        {
                            metaLogger.Exception = item.Clone<Attributes.LoggerAttribute>().SetLogType(LoggerType.Exception);
                        }
                        */
                        break;
                }
            }

            var all = methodLoggerAttr.FirstOrDefault(c => c.LogType == LoggerType.All);

            if (null == metaLogger.Record)
            {
                metaLogger.Record = null == all ? new Attributes.LoggerAttribute(LoggerType.Record, false) : all.Clone<Attributes.LoggerAttribute>().SetLogType(LoggerType.Record);
                attributes.Add(metaLogger.Record);
            }
            if (null == metaLogger.Error)
            {
                metaLogger.Error = null == all ? new Attributes.LoggerAttribute(LoggerType.Error, false) : all.Clone<Attributes.LoggerAttribute>().SetLogType(LoggerType.Error);
                attributes.Add(metaLogger.Error);
            }
            if (null == metaLogger.Exception)
            {
                metaLogger.Exception = null == all ? new Attributes.LoggerAttribute(LoggerType.Exception, false) : all.Clone<Attributes.LoggerAttribute>().SetLogType(LoggerType.Exception);
                attributes.Add(metaLogger.Exception);
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
                    case LoggerType.Record: metaLogger.Record = item; break;
                    case LoggerType.Error: metaLogger.Error = item; break;
                    case LoggerType.Exception: metaLogger.Exception = item; break;
                    default:
                        /*
                        if ((item.LogType & LoggerType.Record) != 0 && null == metaLogger.Record)
                        {
                            metaLogger.Record = item.Clone<Attributes.LoggerAttribute>().SetLogType(LoggerType.Record);
                        }
                        if ((item.LogType & LoggerType.Error) != 0 && null == metaLogger.Error)
                        {
                            metaLogger.Error = item.Clone<Attributes.LoggerAttribute>().SetLogType(LoggerType.Error);
                        }
                        if ((item.LogType & LoggerType.Exception) != 0 && null == metaLogger.Exception)
                        {
                            metaLogger.Exception = item.Clone<Attributes.LoggerAttribute>().SetLogType(LoggerType.Exception);
                        }
                        */
                        break;
                }
            }

            var all = attrs.FirstOrDefault(c => c.LogType == LoggerType.All);

            if (null != all)
            {
                if (null == metaLogger.Record)
                {
                    metaLogger.Record = all.Clone<Attributes.LoggerAttribute>().SetLogType(LoggerType.Record);
                }
                if (null == metaLogger.Error)
                {
                    metaLogger.Error = all.Clone<Attributes.LoggerAttribute>().SetLogType(LoggerType.Error);
                }
                if (null == metaLogger.Exception)
                {
                    metaLogger.Exception = all.Clone<Attributes.LoggerAttribute>().SetLogType(LoggerType.Exception);
                }
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

        static System.Object[] GetDefaultValue(MethodInfo method, System.Collections.Generic.IList<Args> args)
        {
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
                    if (!System.Object.Equals(null, argsObj[i]) && i < defaultObj2.Length)
                    {
                        if (!System.Object.Equals(defaultObj2[i], argsObj[i]))
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
                var iArg = (IArg)System.Activator.CreateInstance(item.Type);

                if (!(null == defaultObj2[item.Position] && item.IArgInType.IsValueType))
                {
                    iArg.In = defaultObj2[item.Position];
                }

                //iArg.In = defaultObj2[item.Position];
                iArg.Group = group;

                defaultObj2[item.Position] = iArg;
            }

            return defaultObj2;
        }

        static System.Collections.Generic.List<Attributes.AttributeBase> CmdAttrGroup(string methodName, System.Collections.Generic.List<Attributes.AttributeBase> attributes)
        {
            var all = attributes.FindAll(c => c is Attributes.CommandAttribute).Cast<Attributes.CommandAttribute>();

            foreach (var item in all)
            {
                if (System.String.IsNullOrWhiteSpace(item.Group)) { item.Group = Bind.CommandGroupDefault; }
                if (System.String.IsNullOrWhiteSpace(item.OnlyName)) { item.OnlyName = methodName; }
            }

            if (!all.Any(c => Bind.CommandGroupDefault == c.Group && methodName == c.OnlyName))
            {
                var cmd = new Attributes.CommandAttribute(methodName) { Group = Bind.CommandGroupDefault };
                attributes.Add(cmd);
            }

            return attributes;
        }

        public static CommandGroup GetBusinessGroup(IBusiness instance, ConcurrentReadOnlyDictionary<string, MetaData> metaData, System.Func<Attributes.CommandAttribute, MethodInfo, MetaData, Command> action)
        {
            var group = new CommandGroup(instance.Configuration.ResultType);

            //========================================//

            var proxyType = instance.GetType();

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
                    var groups = group.dictionary.GetOrAdd(item2.Group, new ConcurrentReadOnlyDictionary<string, Command>());

                    if (!groups.dictionary.TryAdd(item2.OnlyName, action(item2, method2, meta)))
                    {
                        throw new System.Exception(string.Format("Command \"{0}\" member \"{1}\" name exists", item2.Group, item2.OnlyName));
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
        /*
        public static Commands<Local<T>> GetBusinessLocalList<T>(IBusiness business, System.Func<Args, T> action)
        {
            return GetBusinessGroup(business, business.Configuration.MetaData, (command, method, meta) =>
            {
                var key = Bind.GetCommandGroup(command.Group, command.OnlyName);

                var logger = new LocalLogger(new LocalLogger.LoggerAttribute(meta.MetaLogger.Record.LogType, meta.MetaLogger.Record.CanWrite, meta.MetaLogger.Record.CanValue, meta.MetaLogger.Record.CanResult), new LocalLogger.LoggerAttribute(meta.MetaLogger.Exception.LogType, meta.MetaLogger.Exception.CanWrite, meta.MetaLogger.Exception.CanValue, meta.MetaLogger.Exception.CanResult), new LocalLogger.LoggerAttribute(meta.MetaLogger.Error.LogType, meta.MetaLogger.Error.CanWrite, meta.MetaLogger.Error.CanValue, meta.MetaLogger.Error.CanResult));

                var list = new System.Collections.Generic.List<T>();
                GetLocalArgsList(meta.ArgAttrs[key].Args, action, ref list);

                return new Local<T>(meta.Name, meta.ReturnType.FullName, logger, meta.Attributes.Select(c => new LocalAttribute(c.TypeFullName)).ToList(), list, meta.Position, meta.Path, command.Group, command.OnlyName);
            });
        }

        static void GetLocalArgsList<T>(System.Collections.Generic.IList<Args> args, System.Func<Args, T> action, ref System.Collections.Generic.List<T> list)
        {
            if (0 == args.Count) { return; }

            foreach (var item in args)
            {
                if (null != item.Ignore)
                {
                    if (item.Ignore.HasChild && item.HasDeserialize)
                    {
                        var result = action(item);
                        if (!System.Object.Equals(null, result))
                        {
                            list.Add(result);
                        }
                        continue;
                    }
                    continue;
                }
                else
                {
                    GetLocalArgsList(item.ArgAttrChild, action, ref list);

                    var result = action(item);
                    if (!System.Object.Equals(null, result))
                    {
                        list.Add(result);
                    }
                }
            }
        }

        public static Commands<Local<T>> GetBusinessLocal<T>(IBusiness business, System.Func<Args, T> action) where T : ILocalArgs<T>
        {
            return GetBusinessGroup(business, business.Configuration.MetaData, (command, method, meta) =>
            {
                var key = Bind.GetCommandGroup(command.Group, command.OnlyName);

                var record = meta.MetaLogger.Record;
                var exception = meta.MetaLogger.Exception;
                var error = meta.MetaLogger.Error;

                var logger = new LocalLogger(new LocalLogger.LoggerAttribute(record.LogType, record.CanWrite, record.CanValue, record.CanResult), new LocalLogger.LoggerAttribute(exception.LogType, exception.CanWrite, exception.CanValue, exception.CanResult), new LocalLogger.LoggerAttribute(error.LogType, error.CanWrite, error.CanValue, error.CanResult));

                return new Local<T>(meta.Name, meta.ReturnType.FullName, logger, meta.Attributes.Select(c => new LocalAttribute(c.TypeFullName)).ToList(), GetLocalArgs(meta.ArgAttrs[key].Args, action), meta.Position, meta.Path, command.Group, command.OnlyName);
            });
        }

        static System.Collections.Generic.List<T> GetLocalArgs<T>(System.Collections.Generic.IList<Args> args, System.Func<Args, T> action)
            where T : ILocalArgs<T>
        {
            var list = new System.Collections.Generic.List<T>();

            if (0 == args.Count) { return list; }

            foreach (var item in args)
            {
                if (null != item.Ignore)
                {
                    if (item.Ignore.HasChild && item.HasDeserialize)
                    {
                        var result = action(item);
                        if (!System.Object.Equals(null, result))
                        {
                            list.Add(result);
                        }
                        continue;
                    }
                    continue;
                }
                else
                {
                    var result = action(item);
                    if (!System.Object.Equals(null, result))
                    {
                        result.ArgAttrChild = GetLocalArgs(item.ArgAttrChild, action);
                        list.Add(result);
                    }
                }
                //var result = action(item);
                //value.ArgAttrChild = GetLocalArgs(item.ArgAttrChild, action);
                //list.Add(result);
            }

            return list;
        }
        */

        static CommandGroup GetBusinessCommand(IBusiness business)
        {
            //var routeValues = business.Configuration.Routes.Values;

            return GetBusinessGroup(business, business.Configuration.MetaData, (item, method, meta) =>
            {
                var key = Bind.GetCommandGroup(item.Group, item.OnlyName);//item.GetKey();//

                var call = !meta.HasReturn && !meta.HasAsync ? (p, p1) => { MethodInvokerGenerator.CreateDelegate2(method, false, key)(p, p1); return null; } : MethodInvokerGenerator.CreateDelegate<dynamic>(method, false, key);

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
                return new Command(arguments => call(business, GetArgsObj(meta.DefaultValue, arguments, meta.IArgs, key, meta.ArgAttrs[meta.GroupDefault].Args)), meta);
                //, meta.ArgAttrs[Bind.GetDefaultCommandGroup(method.Name)].CommandArgs
            });
        }

        static System.Collections.Generic.IEqualityComparer<Attributes.LoggerAttribute> logComparer = Equality<Attributes.LoggerAttribute>.CreateComparer(c => c.LogType);

        static System.Collections.Generic.IEqualityComparer<Attributes.CommandAttribute> commandComparer = Equality<Attributes.CommandAttribute>.CreateComparer(c => c.GetKey(), System.StringComparer.CurrentCultureIgnoreCase);
        /*
        static System.Collections.Generic.IEqualityComparer<RouteAttribute> routeComparer = Equality<RouteAttribute>.CreateComparer(c => c.GetKey(true), System.StringComparer.CurrentCultureIgnoreCase);
        */
        static ConcurrentReadOnlyDictionary<string, MetaData> GetInterceptorMetaData(Configer.Configuration cfg, System.Collections.Generic.Dictionary<int, MethodInfo> methods, object instance)
        {
            //var routes2 = new ConcurrentReadOnlyDictionary<string, RouteAttribute>();

            //var logComparer = Equality<Attributes.LoggerAttribute>.CreateComparer(c => c.LogType);
            var businessLoggerAttr = cfg.Attributes.GetAttr(logComparer);

            //var commandComparer = Equality<Attributes.CommandAttribute>.CreateComparer(c => c.GetKey(), System.StringComparer.CurrentCultureIgnoreCase);

            //var routeComparer = Equality<Attributes.RouteAttribute>.CreateComparer(c => c.GetKey(true), System.StringComparer.CurrentCultureIgnoreCase);
            //var businessRouteAttr = cfg.Attributes.GetAttr(routeComparer);

            var metaData = new ConcurrentReadOnlyDictionary<string, MetaData>();

#if DEBUG
            foreach (var methodMeta in methods)
#else
            System.Threading.Tasks.Parallel.ForEach(methods, methodMeta =>
#endif
            {
                var method = methodMeta.Value;
                var space = method.DeclaringType.FullName;
                var attributes2 = method.GetAttributes();
                //======LogAttribute======//
                var methodLoggerAttr = attributes2.GetAttr(logComparer);
                var logger = GetMetaLogger(methodLoggerAttr, businessLoggerAttr, attributes2);

                //======CmdAttrGroup======//
                var commandGroup = CmdAttrGroup(method.Name, attributes2).GetAttr(commandComparer);
                /*
                //======RouteAttribute======//
                var route = GetRoute(method.Name, businessRouteAttr, attributes2, commandGroup).GetAttr(routeComparer);
                foreach (var item in route)
                {
                    var key = item.GetKey();
                    if (!routes2.dictionary.TryAdd(key, item))
                    {
                        throw new System.Exception(string.Format("Route path exists \"{0}\"", key));
                    }
                }
                */
                var parameters = method.GetParameters();

                var argAttrGroup = new ConcurrentReadOnlyDictionary<string, ArgAttrs>();//commandGroup.Count

                var tokenPosition = new System.Collections.Generic.List<int>(parameters.Length);
                //var httpRequestPosition = new System.Collections.Generic.List<int>(parameters.Length);
                var useTypePosition = new ConcurrentReadOnlyDictionary<int, System.Type>();

                foreach (var item in commandGroup)
                {
                    var key = Bind.GetCommandGroup(item.Group, item.OnlyName);//item.GetKey();// 

                    argAttrGroup.dictionary.TryAdd(key, new ArgAttrs(item, new System.Collections.Generic.List<Args>(parameters.Length)));
                }

                foreach (var argInfo in parameters)
                {
                    var path = argInfo.Name;
                    var parameterType = argInfo.ParameterType.GetTypeInfo();
                    //==================================//
                    var current = GetCurrentType(argInfo, parameterType);
                    var currentType = current.outType;
                    //==================================//
                    var argAttr = GetArgAttr(argInfo, currentType, cfg.ResultType, current.hasIArg, instance, path);
                    var logAttrArg = LoggerAttr(currentType, logComparer, argInfo);
                    var loggerArg = GetMetaLogger(logAttrArg);
                    var iArgInLogger = current.hasIArg ? GetMetaLogger(LoggerAttr(current.inType, logComparer)) : default(MetaLogger);

                    //==================================//
                    var ignore = GetAttrIgnore(argInfo, currentType);
                    var token = ExistAttr<Attributes.TokenAttribute>(argInfo, currentType);
                    //var httpFile = ExistAttr<Attributes.HttpFileAttribute>(argInfo, currentType);

                    //var hasString = currentType.Equals(typeof(System.String));
                    var hasDeserialize = IsClass(parameterType);
                    //var argAttrTrim = argAttr.Exists(c => c.TrimChar);

                    //var hasUseType = cfg.UseTypes.Contains(currentType.FullName) || (current.hasIArg && cfg.UseTypes.Contains(current.inType.FullName));

                    var hasUseType = cfg.UseTypes.Contains(current.hasIArg ? current.inType.FullName : parameterType.FullName);

                    //set all
                    foreach (var item in argAttrGroup)
                    {
                        var argAttrs = argAttr.FindAll(c => item.Value.CommandAttr.Group == c.Group || Bind.CommandGroupDefault == c.Group);
                        argAttrs.ForEach(c => c.Method = item.Value.CommandAttr.OnlyName);

                        var deserializes = hasDeserialize ? new System.Collections.Generic.List<System.Type> { currentType } : new System.Collections.Generic.List<System.Type>();

                        var source = hasDeserialize ? current.outType.FullName.Replace('+', '.') : string.Format("{0}.{1}.{2}", space, method.Name, argInfo.Name);

                        var argAttrChilds = hasDeserialize ? GetArgAttr(currentType, path, source, string.Format("{0}.{1}", item.Value.CommandAttr.OnlyName, argInfo.Name), item.Value.CommandAttr, ref deserializes, cfg.ResultType, instance) : new System.Collections.Generic.List<Args>(0);

                        item.Value.Args.Add(new Args(argInfo.Name, argInfo.ParameterType, argInfo.Position, argInfo.DefaultValue, argInfo.HasDefaultValue, currentType.Equals(typeof(System.String)), argAttrs, argAttrChilds, hasDeserialize, current.hasIArg, current.hasIArg ? currentType : null, current.hasIArg ? current.inType : null, loggerArg, iArgInLogger, path, source, item.Value.CommandAttr.Group, item.Value.CommandAttr.OnlyName, ignore, hasUseType));

                        //add default convert
                        if (current.hasIArg && 0 == argAttrs.Count)
                        {
                            argAttrs.Add(new Attributes.ArgumentDefaultAttribute(cfg.ResultType));
                        }
                    }

                    //if (typeof(Auth.IToken).IsAssignableFrom(currentType) || token)
                    //{
                    //    tokenPosition.Add(argInfo.Position);
                    //}

                    if (hasUseType)
                    {
                        useTypePosition.dictionary.TryAdd(argInfo.Position, current.hasIArg ? current.inType : currentType);
                    }
                    //var deserializes2 = hasDeserialize ? new System.Collections.Generic.List<System.Type> { currentType } : new System.Collections.Generic.List<System.Type>();

                    //var commandArgs = new CommandArgs(argInfo.Name, parameterType, argInfo.HasDefaultValue, hasDeserialize, hasDeserialize ? GetCommandArgs(currentType, ref deserializes2) : new System.Collections.Concurrent.ConcurrentBag<CommandArgs>());
                    //argAttrGroup[Bind.GetCommandGroupDefault(method.Name)].CommandArgs.Add(commandArgs);
                }

                var groupDefault = Bind.GetCommandGroupDefault(method.Name);
                var args = argAttrGroup[groupDefault].Args;

                var meta = new MetaData(commandGroup, argAttrGroup, args.Where(c => c.HasIArg).ToList(), logger, string.Format("{0}.{1}", space, method.Name), method.Name, method.GetMethodFullName(), method.ReturnType.GetTypeInfo(), cfg.ResultType, GetDefaultValue(method, args), attributes2, methodMeta.Key, groupDefault, useTypePosition);

                if (!metaData.dictionary.TryAdd(method.Name, meta))
                {
                    throw new System.Exception(string.Format("MetaData name exists \"{0}\"", method.Name));
                }
#if DEBUG
            };
#else
            });
#endif

            return metaData;
        }

        struct CurrentType { public System.Boolean hasIArg; public System.Type outType; public System.Type inType; }

        static CurrentType GetCurrentType(ICustomAttributeProvider member, System.Type type)
        {
            var hasIArg = typeof(IArg<,>).GetTypeInfo().IsAssignableFrom(type, out System.Type[] iArgOutType);

            return new CurrentType { hasIArg = hasIArg, outType = hasIArg ? iArgOutType[0] : type, inType = hasIArg ? iArgOutType[1] : null };
        }

        #region GetArgAttr

        internal static System.Collections.Generic.List<Attributes.ArgumentAttribute> GetArgAttr(ParameterInfo member, System.Type type, System.Type resultType, bool hasIArg, dynamic business, string path)
        {
            var argAttr = new System.Collections.Generic.List<Attributes.ArgumentAttribute>();
            argAttr.AddRange(member.GetAttributes<Attributes.ArgumentAttribute>());
            if (hasIArg)
            {
                argAttr.AddRange(member.ParameterType.GetAttributes<Attributes.ArgumentAttribute>());
            }
            argAttr.AddRange(type.GetAttributes<Attributes.ArgumentAttribute>());
            return Bind.GetArgAttr(argAttr, resultType, business, path, type);
        }
        internal static System.Collections.Generic.List<Attributes.ArgumentAttribute> GetArgAttr(MemberInfo member, System.Type type, System.Type resultType, dynamic business, string path)
        {
            var argAttr = new System.Collections.Generic.List<Attributes.ArgumentAttribute>();
            argAttr.AddRange(member.GetAttributes<Attributes.ArgumentAttribute>());
            argAttr.AddRange(type.GetAttributes<Attributes.ArgumentAttribute>());
            return Bind.GetArgAttr(argAttr, resultType, business, path, type);
        }
        static Attributes.IgnoreAttribute GetAttrIgnore(ParameterInfo member, System.Type type)
        {
            return member.GetAttribute<Attributes.IgnoreAttribute>() ?? type.GetAttribute<Attributes.IgnoreAttribute>();
        }
        static Attributes.IgnoreAttribute GetAttrIgnore(MemberInfo member, System.Type type)
        {
            return member.GetAttribute<Attributes.IgnoreAttribute>() ?? type.GetAttribute<Attributes.IgnoreAttribute>();
        }

        static bool ExistAttr<Attribute>(ParameterInfo member, System.Type type) where Attribute : System.Attribute
        {
            return member.Exists<Attribute>() || type.Exists<Attribute>();
        }

        #endregion

        static System.Collections.Generic.List<Args> GetArgAttr(System.Type type, string path, string source, string owner, Attributes.CommandAttribute commandAttr, ref System.Collections.Generic.List<System.Type> deserializes, System.Type resultType, object business)
        {
            var argAttrs = new System.Collections.Generic.List<Args>();

            var position = 0;

            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
            {
                var current = GetCurrentType(field, field.FieldType);

                var ignore = GetAttrIgnore(field, current.outType);

                var hasDeserialize = IsClass(current.outType);

                if (deserializes.Contains(current.outType)) { continue; }
                else if (hasDeserialize) { deserializes.Add(current.outType); }

                var path2 = string.Format("{0}.{1}", path, field.Name);

                var argAttrChild = GetArgAttr(field, current.outType, resultType, business, path2).FindAll(c => commandAttr.Group == c.Group || Bind.CommandGroupDefault == c.Group);
                argAttrChild.ForEach(c => c.Method = commandAttr.OnlyName);

                //var trim = hasString && (commandAttr.TrimChar || argAttrTrim || argAttrChild.Exists(c => c.TrimChar));

                //if (trim || hasDeserialize || 0 < argAttrChild.Count)
                //{
                //}
                var accessor = field.GetAccessor();
                if (null == accessor.Getter || null == accessor.Setter) { continue; }

                var source2 = hasDeserialize ? current.outType.FullName.Replace('+', '.') : string.Format("{0}.{1}", source, field.Name);
                argAttrs.Add(new Args(field.Name, field.FieldType, position++, typeof(System.String).Equals(current.outType), accessor, argAttrChild, hasDeserialize ? GetArgAttr(current.outType, path2, source2, field.Name, commandAttr, ref deserializes, resultType, business) : new System.Collections.Generic.List<Args>(0), hasDeserialize, current.hasIArg, current.hasIArg ? current.outType : null, current.hasIArg ? current.inType : null, path2, source2, commandAttr.Group, owner, ignore));
            }

            foreach (var property in type.GetTypeInfo().DeclaredProperties)
            {
                var current = GetCurrentType(property, property.PropertyType);

                var ignore = GetAttrIgnore(property, current.outType);

                var hasDeserialize = IsClass(current.outType);

                if (deserializes.Contains(current.outType)) { continue; }
                else if (hasDeserialize) { deserializes.Add(current.outType); }

                var path2 = string.Format("{0}.{1}", path, property.Name);

                var argAttrChild = GetArgAttr(property, current.outType, resultType, business, path2).FindAll(c => commandAttr.Group == c.Group || Bind.CommandGroupDefault == c.Group);
                argAttrChild.ForEach(c => c.Method = commandAttr.OnlyName);

                //var trim = hasString && (commandAttr.TrimChar || argAttrTrim || argAttrChild.Exists(c => c.TrimChar));

                //if (trim || hasDeserialize || 0 < argAttrChild.Count)
                //{
                //}
                var accessor = property.GetAccessor();
                if (null == accessor.Getter || null == accessor.Setter) { continue; }

                var source2 = hasDeserialize ? current.outType.FullName.Replace('+', '.') : string.Format("{0}.{1}", source, property.Name);
                argAttrs.Add(new Args(property.Name, property.PropertyType, position++, typeof(System.String).Equals(current.outType), accessor, argAttrChild, hasDeserialize ? GetArgAttr(current.outType, path2, source2, property.Name, commandAttr, ref deserializes, resultType, business) : new System.Collections.Generic.List<Args>(0), hasDeserialize, current.hasIArg, current.hasIArg ? current.outType : null, current.hasIArg ? current.inType : null, path2, source2, commandAttr.Group, owner, ignore));
            }

            return argAttrs;
        }

        #endregion
    }

    public class CommandGroup : ConcurrentReadOnlyDictionary<string, ConcurrentReadOnlyDictionary<string, Command>>
    {
        readonly System.Type resultType;

        public CommandGroup(System.Type resultType) => this.resultType = resultType;

        /*
        public virtual IResult Get2(string cmd, string group = null)
        {
            if (System.String.IsNullOrWhiteSpace(cmd))
            {
                return resultType.ResultCreate(0, System.Convert.ToString(Utils.Help.ExceptionWrite(new System.ArgumentException(nameof(cmd)))));
            }

            if (!this.TryGetValue(System.String.IsNullOrWhiteSpace(group) ? Bind.CommandGroupDefault : group, out ConcurrentReadOnlyDictionary<string, Command> cmdGroup))
            {
                return resultType.ResultCreate((int)Request.Mark.MarkItem.Business_GroupError, string.Format(Request.Mark.GroupError, group));
            }

            if (!cmdGroup.TryGetValue(cmd, out Command command))
            {
                return resultType.ResultCreate((int)Request.Mark.MarkItem.Business_CmdError, string.Format(Request.Mark.CmdError, cmd));
            }

            return resultType.ResultCreate(command);
        }
        */

        public virtual Command GetCommand(string cmd, string group = null)
        {
            if (System.String.IsNullOrEmpty(cmd))
            {
                return null;
            }

            if (System.String.IsNullOrWhiteSpace(group))
            {
                return !this[Bind.CommandGroupDefault].TryGetValue(cmd, out Command command) ? null : command;
            }
            else
            {
                return !this.TryGetValue(group, out ConcurrentReadOnlyDictionary<string, Command> cmdGroup) || !cmdGroup.TryGetValue(cmd, out Command command) ? null : command;
            }
        }

        #region IRequest

        public virtual async System.Threading.Tasks.Task<dynamic> AsyncCallUseType(IRequest request, object[] useObj, System.Action<dynamic> callback = null)
        {
            return await AsyncCallUseType(request.Cmd, request.Group, request.Data, useObj).ContinueWith(c =>
            {
                callback?.Invoke(c.Result);

                return c.Result;
            });
        }

        public virtual async System.Threading.Tasks.Task<Result> AsyncCallUseType<Result>(IRequest request, object[] useObj, System.Action<Result> callback = null) => await AsyncCallUseType(request, useObj, c => callback?.Invoke(c));

        public virtual async System.Threading.Tasks.Task<IResult> AsyncIResultUseType(IRequest request, object[] useObj, System.Action<IResult> callback = null) => await AsyncCallUseType(request, useObj, c => callback?.Invoke(c));

        #endregion

        public virtual async System.Threading.Tasks.Task<dynamic> AsyncCallUseType(string cmd, string group = null, object[] args = null, params object[] useObj)
        {
            var command = GetCommand(cmd, group);

            if (null == command)
            {
                return await System.Threading.Tasks.Task.FromResult(Bind.CmdError(resultType, cmd));
            }

            return await command.AsyncCallUseType(args, useObj);
        }

        #region AsyncCallGroup

        public virtual async System.Threading.Tasks.Task<dynamic> AsyncCallGroup(string cmd, string group, params object[] args)
        {
            var command = GetCommand(cmd, group);

            if (null == command)
            {
                return await System.Threading.Tasks.Task.FromResult(Bind.CmdError(resultType, cmd));
            }

            return await command.AsyncCall(args);
        }

        public virtual async System.Threading.Tasks.Task<Result> AsyncCallGroup<Result>(string cmd, string group, params object[] args) => await AsyncCallGroup(cmd, group, args);

        public virtual async System.Threading.Tasks.Task<IResult> AsyncIResultGroup(string cmd, string group, params object[] args) => await AsyncCallGroup(cmd, group, args);

        #endregion

        #region AsyncCall

        public virtual async System.Threading.Tasks.Task<dynamic> AsyncCall(string cmd, params object[] args) => await AsyncCallGroup(cmd, null, args);

        public virtual async System.Threading.Tasks.Task<Result> AsyncCall<Result>(string cmd, params object[] args) => await AsyncCall(cmd, args);

        public virtual async System.Threading.Tasks.Task<IResult> AsyncIResult(string cmd, params object[] args) => await AsyncCall(cmd, args);

        #endregion

        #region IRequest

        public virtual dynamic CallUseType(IRequest request, params object[] useObj) => CallUseType(request.Cmd, request.Group, request.Data, useObj);

        public virtual Result CallUseType<Result>(IRequest request, params object[] useObj) => CallUseType(request, useObj);

        public virtual IResult CallIResultUseType(IRequest request, params object[] useObj) => CallUseType(request, useObj);

        #endregion

        public virtual dynamic CallUseType(string cmd, string group = null, object[] args = null, params object[] useObj)
        {
            var command = GetCommand(cmd, group);

            return null == command ? Bind.CmdError(resultType, cmd) : command.CallUseType(args, useObj);
        }

        #region CallGroup

        public virtual dynamic CallGroup(string cmd, string group, params object[] args)
        {
            var command = GetCommand(cmd, group);

            return null == command ? Bind.CmdError(resultType, cmd) : command.Call(args);
        }

        public virtual Result CallGroup<Result>(string cmd, string group, params object[] args) => CallGroup(cmd, group, args);

        public virtual IResult CallIResultGroup(string cmd, string group, params object[] args) => CallGroup(cmd, group, args);

        #endregion

        #region Call

        public virtual dynamic Call(string cmd, params object[] args) => CallGroup(cmd, null, args);

        public virtual Result Call<Result>(string cmd, params object[] args) => Call(cmd, args);

        public virtual IResult CallIResult(string cmd, params object[] args) => Call(cmd, args);

        #endregion
    }

    public class Command
    {
        //public Command(System.Func<object[], dynamic> call, string name, bool hasReturn, bool hasIResult, System.Type returnType, bool hasAsync, ConcurrentReadOnlyDictionary<System.Type, int> useTypePosition, MetaData meta)
        //{
        //    this.call = call;
        //    this.name = name;
        //    //this.hasReturn = hasReturn;
        //    //this.hasIResult = hasIResult;
        //    //this.returnType = returnType;
        //    //this.hasAsync = hasAsync;
        //    //this.useTypePosition = useTypePosition;
        //    this.meta = meta;
        //}
        public Command(System.Func<object[], dynamic> call, MetaData meta)
        {
            this.call = call;
            this.meta = meta;
        }

        //===============member==================//
        readonly System.Func<object[], dynamic> call;

        #region CallUseType

        public virtual dynamic CallUseType(object[] args, params object[] useObj) => Call(GetAgs(args, useObj));

        public virtual Result CallUseType<Result>(object[] args, params object[] useObj) => CallUseType(args, useObj);

        public virtual IResult CallIResultUseType(object[] args, params object[] useObj) => CallUseType(args, useObj);

        #endregion

        public virtual dynamic Call(params object[] args)
        {
            try
            {
                return call(args);
            }
            catch (System.Exception ex)
            {
                return this.meta.ResultType.ResultCreate(0, System.Convert.ToString(Utils.Help.ExceptionWrite(ex)));
            }
        }
        public virtual Result Call<Result>(params object[] args) => Call(args);
        public virtual IResult CallIResult(params object[] args) => Call(args);

        //#if Standard
        //        public virtual async System.Threading.Tasks.Task<dynamic> AsyncCall(params object[] args) => await System.Threading.Tasks.Task.Factory.StartNew(obj => { var obj2 = (dynamic)obj; return obj2.call(obj2.args); }, new { call, args });
        //#else

        public virtual object[] GetAgs(object[] args, params object[] useObj)
        {
            var args2 = new object[meta.ArgAttrs[meta.GroupDefault].Args.Count];

            //if (0 < meta.UseTypePosition.Count && null != useObj && 0 < useObj.Length)
            //{
            //    foreach (var item in useObj)
            //    {
            //        if (System.Object.Equals(null, item)) { continue; }

            //        var useArg = meta.UseTypePosition.FirstOrDefault(c => c.Value.IsAssignableFrom(item.GetType()));

            //        if (null != useArg.Value)
            //        {
            //            args2[useArg.Key] = item;
            //        }
            //    }

            //    for (int i = 0; i < args.Length; i++)
            //    {

            //    }
            //}

            if (null != args && 0 < args.Length && 0 < args2.Length)
            {
                int l = 0;
                for (int i = 0; i < args2.Length; i++)
                {
                    //if (meta.TokenPosition.Contains(i) || meta.HttpRequestPosition.Contains(i))
                    //{
                    //    continue;
                    //}
                    if (meta.UseTypePosition.ContainsKey(i))
                    {
                        if (null != useObj && 0 < useObj.Length)
                        {
                            var item = useObj.FirstOrDefault(c => meta.UseTypePosition[i].IsAssignableFrom(c.GetType()));

                            if (!System.Object.Equals(null, item))
                            {
                                args2[i] = item;
                            }
                        }

                        continue;
                    }

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

            return args2;
        }

        #region AsyncCallUseType

        public virtual async System.Threading.Tasks.Task<dynamic> AsyncCallUseType(object[] args, params object[] useObj) => await AsyncCall(GetAgs(args, useObj));

        public virtual async System.Threading.Tasks.Task<Result> AsyncCallUseType<Result>(object[] args, params object[] useObj) => await AsyncCallUseType(args, useObj);

        public virtual async System.Threading.Tasks.Task<IResult> AsyncIResultUseType(object[] args, params object[] useObj) => await AsyncCallUseType(args, useObj);

        #endregion

        public virtual async System.Threading.Tasks.Task<dynamic> AsyncCall(params object[] args)
        {
            try
            {
                if (meta.HasAsync)
                {
                    //return this.HasIResult ? await (call(args) as System.Threading.Tasks.Task<IResult>) : await (call(args) as System.Threading.Tasks.Task<dynamic>);
                    return await call(args);
                }
                else
                {
                    using (var task = System.Threading.Tasks.Task.Factory.StartNew(obj => { var obj2 = (dynamic)obj; return obj2.call(obj2.args); }, new { call, args })) { return await task; }
                }
            }
            catch (System.Exception ex)
            {
                return await System.Threading.Tasks.Task.FromResult(this.meta.ResultType.ResultCreate(0, System.Convert.ToString(Utils.Help.ExceptionWrite(ex))));
            }
        }

        public virtual async System.Threading.Tasks.Task<Result> AsyncCall<Result>(params object[] args) => await AsyncCall(args);

        public virtual async System.Threading.Tasks.Task<IResult> AsyncIResult(params object[] args) => await AsyncCall(args);
        //#endif

        //readonly string name;
        //public string Name { get => name; }

        //readonly bool hasReturn;
        //public bool HasReturn { get => hasReturn; }

        //readonly bool hasIResult;
        //public bool HasIResult { get => hasIResult; }

        //readonly System.Type returnType;
        //public System.Type ReturnType { get => returnType; }

        //readonly bool hasAsync;
        //public bool HasAsync { get => hasAsync; }

        //readonly ConcurrentReadOnlyDictionary<System.Type, int> useTypePosition;
        //public ConcurrentReadOnlyDictionary<System.Type, int> UseTypePosition { get => useTypePosition; }

        readonly MetaData meta;
        public MetaData Meta { get => meta; }
    }
}

namespace Business.Meta
{
    using Business.Utils;
    using System.Reflection;

    #region Meta

    public struct ArgAttrs
    {
        public ArgAttrs(Attributes.CommandAttribute commandAttr, System.Collections.Generic.IList<Args> args)
        {
            this.commandAttr = commandAttr;
            this.args = args;
            //this.commandArgs = commandArgs;
        }

        readonly Attributes.CommandAttribute commandAttr;

        /// <summary>
        /// Interceptor using.
        /// </summary>
        public Attributes.CommandAttribute CommandAttr { get => commandAttr; }

        System.Collections.Generic.IList<Args> args;
        public System.Collections.Generic.IList<Args> Args { get => args; internal set => args = value; }
        ////===============commandArgs==================//
        //readonly System.Collections.Generic.IList<CommandArgs> commandArgs;
        //public System.Collections.Generic.IList<CommandArgs> CommandArgs { get => commandArgs; }
    }

    /// <summary>
    /// Argument
    /// </summary>
    public struct Args
    {
        //Arg
        public Args(System.String name, System.Type type, System.Int32 position, System.Object defaultValue, System.Boolean hasDefaultValue, System.Boolean hasString, System.Collections.Generic.IReadOnlyList<Attributes.ArgumentAttribute> argAttr, System.Collections.Generic.IList<Args> argAttrChild, System.Boolean hasDeserialize, System.Boolean hasIArg, System.Type iArgOutType, System.Type iArgInType, MetaLogger logger, MetaLogger iArgInLogger, string path, string source, string group, string owner, Attributes.IgnoreAttribute ignore, bool useType)
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
            this.iArgInType = iArgInType;
            this.logger = logger;
            this.iArgInLogger = iArgInLogger;
            this.accessor = default(Accessor);
            //this.trim = false;
            this.path = path;
            this.source = source;
            this.group = group;
            this.owner = owner;
            this.ignore = ignore;
            this.useType = useType;
        }

        //argChild
        public Args(System.String name, System.Type type, System.Int32 position, System.Boolean hasString, Utils.Accessor accessor, System.Collections.Generic.IReadOnlyList<Attributes.ArgumentAttribute> argAttr, System.Collections.Generic.IList<Args> argAttrChild, System.Boolean hasDeserialize, System.Boolean hasIArg, System.Type iArgOutType, System.Type iArgInType, string path, string source, string group, string owner, Attributes.IgnoreAttribute ignore)
        {
            this.name = name;
            this.type = type;
            this.position = position;
            this.hasString = hasString;
            this.accessor = accessor;
            this.argAttr = argAttr;
            this.argAttrChild = argAttrChild;
            this.hasDeserialize = hasDeserialize;
            this.hasIArg = hasIArg;
            this.iArgOutType = iArgOutType;
            this.iArgInType = iArgInType;
            //this.trim = trim;
            this.path = path;
            this.source = source;

            this.defaultValue = null;// type.GetTypeInfo().IsValueType ? System.Activator.CreateInstance(type) : null;
            this.hasDefaultValue = false;
            this.logger = default(MetaLogger);
            this.iArgInLogger = default(MetaLogger);
            this.group = group;
            this.owner = owner;
            this.ignore = ignore;
            this.useType = false;
        }

        //===============name==================//
        readonly System.String name;
        public System.String Name { get => name; }
        //===============type==================//
        readonly System.Type type;
        public System.Type Type { get => type; }
        //===============position==================//
        readonly System.Int32 position;
        public System.Int32 Position { get => position; }
        //===============defaultValue==================//
        readonly System.Object defaultValue;
        public System.Object DefaultValue { get => defaultValue; }
        //===============hasDefaultValue==================//
        readonly System.Boolean hasDefaultValue;
        public System.Boolean HasDefaultValue { get => hasDefaultValue; }
        //===============hasString==================//
        readonly System.Boolean hasString;
        public System.Boolean HasString { get => hasString; }
        //===============accessor==================//
        readonly Utils.Accessor accessor;
        public Utils.Accessor Accessor { get => accessor; }
        //===============argAttr==================//
        readonly System.Collections.Generic.IReadOnlyList<Attributes.ArgumentAttribute> argAttr;
        public System.Collections.Generic.IReadOnlyList<Attributes.ArgumentAttribute> ArgAttr { get => argAttr; }
        //===============argAttrChild==================//
        readonly System.Collections.Generic.IList<Args> argAttrChild;
        public System.Collections.Generic.IList<Args> ArgAttrChild => argAttrChild;
        //===============hasDeserialize==================//
        readonly System.Boolean hasDeserialize;
        public System.Boolean HasDeserialize { get => hasDeserialize; }
        //===============IArgOutType==================//
        readonly System.Type iArgOutType;
        public System.Type IArgOutType { get => iArgOutType; }
        //===============IArgInType==================//
        readonly System.Type iArgInType;
        public System.Type IArgInType { get => iArgInType; }
        ////==============trim===================//
        //readonly System.Boolean trim;
        //public System.Boolean Trim { get => trim; 
        //==============path===================//
        readonly System.String path;
        public System.String Path { get => path; }
        //==============source===================//
        readonly System.String source;
        public System.String Source { get => source; }
        //==============logAttr===================//
        //===============hasIArg==================//
        readonly System.Boolean hasIArg;
        public System.Boolean HasIArg { get => hasIArg; }
        readonly MetaLogger logger;
        public MetaLogger Logger { get => logger; }
        readonly MetaLogger iArgInLogger;
        public MetaLogger IArgInLogger { get => iArgInLogger; }
        //==============group===================//
        readonly string group;
        public string Group { get => group; }
        //==============owner===================//
        readonly string owner;
        public string Owner { get => owner; }
        //==============ignore===================//
        readonly Attributes.IgnoreAttribute ignore;
        public Attributes.IgnoreAttribute Ignore { get => ignore; }
        //==============useType===================//
        readonly bool useType;
        public bool UseType { get => useType; }
    }

    public struct MetaLogger
    {
        public Attributes.LoggerAttribute Record { get; set; }
        public Attributes.LoggerAttribute Exception { get; set; }
        public Attributes.LoggerAttribute Error { get; set; }
    }

    public struct MetaData
    {
        public MetaData(System.Collections.Generic.IReadOnlyList<Attributes.CommandAttribute> commandGroup, ConcurrentReadOnlyDictionary<string, ArgAttrs> argAttrs, System.Collections.Generic.IReadOnlyList<Args> iArgs, MetaLogger metaLogger, string path, string name, string fullName, TypeInfo returnType, System.Type resultType, object[] defaultValue, System.Collections.Generic.List<Attributes.AttributeBase> attributes, int position, string groupDefault, ConcurrentReadOnlyDictionary<int, System.Type> useTypePosition)
        {
            this.commandGroup = commandGroup;
            this.argAttrs = argAttrs;
            this.iArgs = iArgs;
            this.metaLogger = metaLogger;
            this.path = path;
            this.name = name;
            this.fullName = fullName;

            //this.returnType = returnType;
            this.hasAsync = Utils.Help.IsAssignableFrom(typeof(System.Threading.Tasks.Task<>).GetTypeInfo(), returnType, out System.Type[] arguments) || typeof(System.Threading.Tasks.Task).IsAssignableFrom(returnType);
            //typeof(void) != method.ReturnType
            this.hasReturn = !(typeof(void) == returnType || (this.hasAsync && null == arguments));
            //typeof(IResult).IsAssignableFrom(method.ReturnType),
            //typeof(System.Object).Equals(method.ReturnType)
            var hasGeneric = this.hasAsync && null != arguments;
            this.hasIResult = typeof(Result.IResult).IsAssignableFrom(hasGeneric ? arguments[0] : returnType);
            this.hasObject = typeof(System.Object).Equals(hasGeneric ? arguments[0] : returnType);
            this.returnType = hasGeneric ? arguments[0] : returnType;
            this.resultType = resultType;
            this.defaultValue = defaultValue;
            //this.logAttrs = logAttrs;
            this.attributes = attributes;
            this.position = position;
            this.groupDefault = groupDefault;
            this.useTypePosition = useTypePosition;
        }

        //==============commandAttr===================//
        readonly System.Collections.Generic.IReadOnlyList<Attributes.CommandAttribute> commandGroup;
        public System.Collections.Generic.IReadOnlyList<Attributes.CommandAttribute> CommandGroup { get => commandGroup; }
        //==============argAttrs===================//
        readonly ConcurrentReadOnlyDictionary<string, ArgAttrs> argAttrs;
        public ConcurrentReadOnlyDictionary<string, ArgAttrs> ArgAttrs { get => argAttrs; }
        //==============iArgs===================//
        readonly System.Collections.Generic.IReadOnlyList<Args> iArgs;
        public System.Collections.Generic.IReadOnlyList<Args> IArgs { get => iArgs; }
        //==============MetaLogger===================//
        readonly MetaLogger metaLogger;
        public MetaLogger MetaLogger { get => metaLogger; }
        //==============path===================//
        readonly System.String path;
        public System.String Path { get => path; }
        //==============name===================//
        readonly string name;
        public string Name { get => name; }
        //==============fullName===================//
        readonly string fullName;
        public string FullName { get => fullName; }
        //==============hasReturn===================//
        readonly bool hasReturn;
        public bool HasReturn { get => hasReturn; }
        //==============hasIResult===================//
        readonly bool hasIResult;
        public bool HasIResult { get => hasIResult; }
        //==============hasObject===================//
        readonly bool hasObject;
        public bool HasObject { get => hasObject; }
        //==============returnType===================//
        readonly System.Type returnType;
        public System.Type ReturnType { get => returnType; }
        //==============resultType===================//
        readonly System.Type resultType;
        public System.Type ResultType { get => resultType; }
        //==============hasAsync===================//
        readonly bool hasAsync;
        public bool HasAsync { get => hasAsync; }
        //==============defaultValue===================//
        readonly object[] defaultValue;
        public object[] DefaultValue { get => defaultValue; }
        //==============attributes===================//
        readonly System.Collections.Generic.List<Attributes.AttributeBase> attributes;
        public System.Collections.Generic.List<Attributes.AttributeBase> Attributes { get => attributes; }
        //===============position==================//
        readonly System.Int32 position;
        public System.Int32 Position { get => position; }
        //==============groupDefault===================//
        readonly string groupDefault;
        public string GroupDefault { get => groupDefault; }
        //==============useTypesPosition===================//
        readonly ConcurrentReadOnlyDictionary<int, System.Type> useTypePosition;
        public ConcurrentReadOnlyDictionary<int, System.Type> UseTypePosition { get => useTypePosition; }
    }

    /*
    public interface ILocalArgs<T> { System.Collections.Generic.IList<T> ArgAttrChild { get; set; } }

    public struct LocalArgs : ILocalArgs<LocalArgs>
    {
        public LocalArgs(System.String name, System.String type, System.Int32 position, System.Object defaultValue, System.Boolean hasDefaultValue, System.Collections.Generic.IReadOnlyList<LocalAttribute> argAttr, System.Collections.Generic.IList<LocalArgs> argAttrChild, System.Boolean hasDeserialize, System.Boolean hasIArg, LocalLogger metaLogger, System.String fullName, string group, string owner)
        {
            this.name = name;
            this.type = type;
            this.position = position;
            this.defaultValue = defaultValue;
            this.hasDefaultValue = hasDefaultValue;
            this.argAttr = argAttr;
            this.argAttrChild = argAttrChild;
            this.hasDeserialize = hasDeserialize;
            this.hasIArg = hasIArg;
            this.metaLogger = metaLogger;
            this.fullName = fullName;
            this.group = group;
            this.owner = owner;
        }

        //===============name==================//
        readonly System.String name;
        public System.String Name { get => name; }
        //===============type==================//
        readonly System.String type;
        public System.String Type { get => type; }
        //===============position==================//
        readonly System.Int32 position;
        public System.Int32 Position { get => position; }
        //===============defaultValue==================//
        readonly System.Object defaultValue;
        public System.Object DefaultValue { get => defaultValue; }
        //===============hasDefaultValue==================//
        readonly System.Boolean hasDefaultValue;
        public System.Boolean HasDefaultValue { get => hasDefaultValue; }
        //===============argAttr==================//
        readonly System.Collections.Generic.IReadOnlyList<LocalAttribute> argAttr;
        public System.Collections.Generic.IReadOnlyList<LocalAttribute> ArgAttr { get => argAttr; }
        //===============argAttrChild==================//
        System.Collections.Generic.IList<LocalArgs> argAttrChild;
        public System.Collections.Generic.IList<LocalArgs> ArgAttrChild { get => argAttrChild; set => argAttrChild = value; }
        //===============hasDeserialize==================//
        readonly System.Boolean hasDeserialize;
        public System.Boolean HasDeserialize { get => hasDeserialize; }
        //===============hasIArg==================//
        readonly System.Boolean hasIArg;
        public System.Boolean HasIArg { get => hasIArg; }
        //==============fullName===================//
        readonly System.String fullName;
        public System.String FullName { get => fullName; }
        //==============logAttr===================//
        readonly LocalLogger metaLogger;
        public LocalLogger MetaLogger { get => metaLogger; }
        //==============group===================//
        readonly string group;
        public string Group { get => group; }
        //==============owner===================//
        readonly string owner;
        public string Owner { get => owner; }
    }

    public struct LocalLogger
    {
        public LocalLogger(LoggerAttribute record, LoggerAttribute exception, LoggerAttribute error)
        {
            this.record = record;
            this.exception = exception;
            this.error = error;
        }

        readonly LoggerAttribute record;
        public LoggerAttribute Record { get => record; }

        readonly LoggerAttribute exception;
        public LoggerAttribute Exception { get => exception; }

        readonly LoggerAttribute error;
        public LoggerAttribute Error { get => error; }

        public class LoggerAttribute
        {
            public LoggerAttribute(LoggerType logType, bool canWrite, Attributes.LoggerValueMode canValue, bool canResult)
            {
                this.logType = logType;
                this.canWrite = canWrite;
                this.canValue = canValue;
                this.canResult = canResult;
            }

            readonly LoggerType logType;
            /// <summary>
            /// Record type
            /// </summary>
            public LoggerType LogType
            {
                get { return logType; }
            }

            readonly bool canWrite;
            /// <summary>
            /// Allow record
            /// </summary>
            public bool CanWrite { get => canWrite; }

            readonly Attributes.LoggerValueMode canValue;
            /// <summary>
            /// Allowed to return to parameters
            /// </summary>
            public Attributes.LoggerValueMode CanValue { get => canValue; }

            readonly bool canResult;
            /// <summary>
            /// Allowed to return to results
            /// </summary>
            public bool CanResult { get => canResult; }
        }
    }

    public struct LocalAttribute
    {
        public LocalAttribute(string type, bool allowMultiple = false, bool inherited = false)
        {
            this.type = type;
            this.allowMultiple = allowMultiple;
            this.inherited = inherited;
        }

        readonly string type;
        public System.String Type { get => type; }

        readonly bool allowMultiple;
        /// <summary>
        /// Is it possible to specify attributes for multiple instances for a program element
        /// </summary>
        public bool AllowMultiple { get => allowMultiple; }

        readonly bool inherited;
        /// <summary>
        /// Determines whether the attributes indicated by the derived class and the overridden member are inherited
        /// </summary>
        public bool Inherited { get => inherited; }
    }

    public struct Local<T>
    {
        /// <summary>
        /// Json
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Utils.Help.JsonSerialize(this);

        public Local(string name, string returnType, LocalLogger metaLogger, System.Collections.Generic.IList<LocalAttribute> attributes, System.Collections.Generic.List<T> args, int position, string path, string group, string onlyName)
        {
            this.name = name;
            this.returnType = returnType;
            this.metaLogger = metaLogger;
            this.attributes = attributes as System.Collections.Generic.IReadOnlyList<LocalAttribute>;
            this.args = args;
            this.position = position;
            this.path = path;
            this.group = group;
            this.onlyName = onlyName;
        }

        readonly string name;
        public string Name { get => name; }

        readonly string returnType;
        public string ReturnType { get => returnType; }

        readonly LocalLogger metaLogger;
        public LocalLogger MetaLogger { get => metaLogger; }

        readonly System.Collections.Generic.IReadOnlyList<LocalAttribute> attributes;
        public System.Collections.Generic.IReadOnlyList<LocalAttribute> Attributes { get => attributes; }

        readonly System.Collections.Generic.List<T> args;
        public System.Collections.Generic.List<T> Args { get => args; }

        //===============position==================//
        readonly System.Int32 position;
        public System.Int32 Position { get => position; }

        //==============path===================//
        readonly string path;
        public string Path { get => path; }

        //==============group===================//
        readonly string group;
        public string Group { get => group; }

        //==============onlyName===================//
        readonly string onlyName;
        public string OnlyName { get => onlyName; }
    }
    */

    #endregion
}
