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
        internal static IResult ErrorBusiness(System.Type resultTypeDefinition, string business) => ResultFactory.ResultCreate(resultTypeDefinition, -1, string.Format("Without this Business{0}", string.IsNullOrEmpty(business) ? null : $" {business}"));

        internal static IResult ErrorCmd(System.Type resultTypeDefinition, string cmd) => ResultFactory.ResultCreate(resultTypeDefinition, -2, string.Format("Without this Cmd{0}", string.IsNullOrEmpty(cmd) ? null : $" {cmd}"));

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
        /*
        internal static dynamic GetReturnValue(IResult result, MetaData meta)
        {
            //var result2 = (meta.HasIResult || meta.HasObject) ? result : meta.HasReturn && meta.ReturnType.IsValueType ? System.Activator.CreateInstance(meta.ReturnType) : null;
            object result2 = result;

            //if (!meta.HasObject)
            //{
            //    if (meta.HasReturn && meta.ReturnType.IsValueType)
            //    {
            //        result2 = System.Activator.CreateInstance(meta.ReturnType);
            //    }
            //    else
            //    {
            //        result2 = null;
            //    }
            //}

            if (meta.HasReturn && meta.ReturnType.IsValueType)
            {
                result2 = System.Activator.CreateInstance(meta.ReturnType);
            }
            //else
            //{
            //    result2 = null;
            //}


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
                //if (meta.HasIResult)
                //{
                //    return System.Threading.Tasks.Task.FromResult(result2 as IResult<dynamic>);
                //}

                return System.Threading.Tasks.Task.FromResult(meta.HasIResult ? result2 as IResult : meta.HasReturn ? result2 : null);
            }

            return result2;
        }
        */
        internal static dynamic GetReturnValue(IResult result, MetaData meta)
        {
            object result2 = result;
            if (meta.HasReturn)
            {
                if (!meta.HasObject)
                {
                    dynamic result3;

                    if (meta.ReturnType.IsValueType)
                    {
                        result3 = System.Activator.CreateInstance(meta.ReturnType);
                    }
                    else
                    {
                        result3 = Create(meta.ReturnType);
                    }

                    if (meta.HasAsync)
                    {
                        return System.Threading.Tasks.Task.FromResult(result3);
                    }

                    return result3;
                }
                else if (meta.ReturnType.IsValueType)
                {
                    result2 = System.Activator.CreateInstance(meta.ReturnType);
                }
            }
            else
            {

                result2 = null;
            }

            if (meta.HasAsync)
            {
                return System.Threading.Tasks.Task.FromResult(result2);
            }

            return result2;
        }

        /// <summary>
        /// general object creation
        /// </summary>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static dynamic Create(System.Type targetType)
        {
            //string test first - it has no parameterless constructor
            if (System.Type.GetTypeCode(targetType) == System.TypeCode.String)
            {
                return string.Empty;
            }

            // get the default constructor and instantiate
            var types = new System.Type[0];
            var info = targetType.GetConstructor(types);
            object targetObject = null;

            if (info == null) //must not have found the constructor
            {
                if (targetType.BaseType.UnderlyingSystemType.FullName.Contains("Enum"))
                {
                    targetObject = System.Activator.CreateInstance(targetType);
                }
                else
                {
                    throw new System.ArgumentException("Unable to instantiate type: " + targetType.AssemblyQualifiedName + " - Constructor not found");
                }
            }
            else
            {
                targetObject = info.Invoke(null);
            }

            if (targetObject == null)
            {
                throw new System.ArgumentException("Unable to instantiate type: " + targetType.AssemblyQualifiedName + " - Unknown Error");
            }

            return targetObject;
        }

        internal static dynamic GetReturnValueIResult<Data>(IResult<Data> result, MetaData meta)
        {
            if (meta.HasAsync)
            {
                if (meta.HasIResultGeneric)
                {
                    return System.Threading.Tasks.Task.FromResult(result);
                }

                return System.Threading.Tasks.Task.FromResult(result as IResult);
            }

            return result;
        }
        //internal static dynamic GetReturnValueIResultGeneric<Data>(IResult<Data> result, MetaData meta)
        //{
        //    if (meta.HasIResult || meta.HasObject)
        //    {
        //        if (meta.HasAsync)
        //        {
        //            return System.Threading.Tasks.Task.FromResult(result);
        //        }
        //        else
        //        {
        //            return result;
        //        }
        //    }
        //    else if (meta.HasReturn && meta.ReturnType.IsValueType)
        //    {
        //        var resultObj = System.Activator.CreateInstance(meta.ReturnType);

        //        if (meta.HasAsync)
        //        {
        //            return System.Threading.Tasks.Task.FromResult(resultObj);
        //        }
        //        else
        //        {
        //            return resultObj;
        //        }
        //    }

        //    return null;
        //}

        #endregion

        #region Create

        /// <summary>
        /// Initialize a Generic proxy class
        /// </summary>
        /// <typeparam name="Business"></typeparam>
        /// <param name="constructorArguments"></param>
        /// <returns></returns>
        public static Business Create<Business>(params object[] constructorArguments)
            where Business : class => (Business)Create(typeof(Business), null, constructorArguments);

        /// <summary>
        /// Initialize a Generic proxy class
        /// </summary>
        /// <typeparam name="Business"></typeparam>
        /// <param name="interceptor"></param>
        /// <param name="constructorArguments"></param>
        /// <returns></returns>
        public static Business Create<Business>(Auth.IInterceptor interceptor = null, params object[] constructorArguments)
            where Business : class => (Business)Create(typeof(Business), interceptor, constructorArguments);

        /// <summary>
        /// Initialize a Type proxy class
        /// </summary>
        /// <param name="type"></param>
        /// <param name="constructorArguments"></param>
        /// <returns></returns>
        public static IBusiness Create(System.Type type, params object[] constructorArguments) => Create(type, null, constructorArguments) as IBusiness;

        /// <summary>
        /// Initialize a Type proxy class
        /// </summary>
        /// <param name="type"></param>
        /// <param name="interceptor"></param>
        /// <param name="constructorArguments"></param>
        /// <returns></returns>
        public static object Create(System.Type type, Auth.IInterceptor interceptor = null, params object[] constructorArguments) => new Bind(type, interceptor ?? new Auth.Interceptor(), constructorArguments).Instance;

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
        public object Instance;

        public Bind(System.Type type, Auth.IInterceptor interceptor, params object[] constructorArguments)
        {
            var typeInfo = type.GetTypeInfo();

            var methods = GetMethods(typeInfo);

            var proxy = new Castle.DynamicProxy.ProxyGenerator();

            try
            {
                var types = constructorArguments?.Select(c => c.GetType())?.ToArray();

                var constructor = null == types ? null : type.GetConstructor(types);

                Instance = proxy.CreateClassProxy(type, null == constructor ? type.GetConstructors()?.FirstOrDefault()?.GetParameters().Select(c => c.HasDefaultValue ? c.DefaultValue : default).ToArray() : constructorArguments, interceptor);
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
            info.TypeFullName = type.FullName.Replace('+', '.');

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

            try
            {
                interceptor.MetaData = GetInterceptorMetaData(cfg, methods);
            }
            catch (System.Exception ex)
            {

                throw ex;
            }

            //interceptor.ResultType = cfg.ResultType;

            interceptor.Configer = cfg;

            if (null != business)
            {
                cfg.MetaData = interceptor.MetaData;

                business.Configer = cfg;

                business.Command = GetBusinessCommand(business);

                cfg.Logger = business.Logger;

                Configer.BusinessList.dictionary.TryAdd(business.Configer.Info.BusinessName, business);

                foreach (var item in business.Command)
                {
                    if (string.IsNullOrWhiteSpace(item.Key)) { continue; }

                    var route2 = new Configer.Route(cfg.Info.BusinessName, item.Key);
                    if (!Configer.Routes.dictionary.TryAdd(route2.ToString(), route2))
                    {
                        throw new System.Exception($"Routes exists \"{route2}\"");
                    }
                }
                var route = new Configer.Route(cfg.Info.BusinessName);
                if (!Configer.Routes.dictionary.TryAdd(route.ToString(), route))
                {
                    throw new System.Exception($"Routes exists \"{route}\"");
                }

                type.LoadAccessors(Configer.Accessors, business.Configer.Info.BusinessName);

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

        //static readonly System.Collections.Generic.List<string> SysTypes = Assembly.GetExecutingAssembly().GetType().Module.Assembly.GetExportedTypes().Select(c => c.FullName).ToList();
        //static bool HasDefinition(System.Type type)
        //{
        //    return !SysTypes.Contains(type.FullName) && (type.IsClass || (type.IsValueType && !type.IsEnum && !type.IsArray && !type.IsCollection() && !type.IsEnumerable()));
        //    //return type.IsClass || (type.IsValueType && !type.IsPrimitive && !type.IsEnum && !type.IsArray);
        //    //return !type.FullName.StartsWith("System.") && (type.IsClass || (type.IsValueType && !type.IsPrimitive && !type.IsEnum && !type.IsArray));
        //    //return !type.IsPrimitive && !type.IsEnum && !type.IsArray && !type.IsSecurityTransparent;
        //}

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
                            if (args[i].Nullable && args[i].LastType.GetTypeCode() == System.TypeCode.DateTime && string.IsNullOrEmpty(System.Convert.ToString(argsObj[i])))
                            {
                                argsObj[i] = null;
                            }
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
            var group = new CommandGroup(business.Configer.ResultTypeDefinition, business.Configer.Info.CommandGroupDefault);

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

                var call = dynamicMethodBuilder.GetDelegate(method);// as System.Func<object, object[], dynamic>;
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
                name = $"{name}{{{string.Join(",", type.GenericTypeArguments.Select(c => GetMethodTypeFullName(c)))}}}";
            }

            return name;
        }

        static ConcurrentReadOnlyDictionary<string, MetaData> GetInterceptorMetaData(Configer cfg, System.Collections.Generic.Dictionary<int, MethodInfo> methods)
        {
            var metaData = new ConcurrentReadOnlyDictionary<string, MetaData>();

#if DEBUG
            foreach (var methodMeta in methods)
#else
            System.Threading.Tasks.Parallel.ForEach(methods, methodMeta =>
#endif
            {
                var method = methodMeta.Value;

                #region method info

                var hasAsync = Help.IsAssignableFrom(typeof(System.Threading.Tasks.Task<>), method.ReturnType, out System.Type[] asyncGeneric) || method.ReturnType == typeof(System.Threading.Tasks.Task);
                var hasReturn = !(typeof(void) == method.ReturnType || (hasAsync && null == asyncGeneric));
                var hasAsyncGeneric = hasAsync && null != asyncGeneric;
                var hasIResult = typeof(IResult<>).IsAssignableFrom(hasAsyncGeneric ? asyncGeneric[0] : method.ReturnType, out System.Type[] resultGeneric) || typeof(IResult).IsAssignableFrom(hasAsyncGeneric ? asyncGeneric[0] : method.ReturnType);
                var hasIResultGeneric = hasIResult && null != resultGeneric;
                //var hasObject = typeof(object).Equals(hasAsyncGeneric ? asyncGeneric[0] : method.ReturnType);
                //var returnType = hasAsyncGeneric ? asyncGeneric[0] : method.ReturnType;
                var returnType = hasIResultGeneric ? resultGeneric[0] : hasIResult ? typeof(object) : hasAsyncGeneric ? asyncGeneric[0] : method.ReturnType;
                var resultType = cfg.ResultTypeDefinition.MakeGenericType(hasIResultGeneric ? resultGeneric[0] : typeof(string));

                #endregion

                var space = method.DeclaringType.FullName;

                var attributes2 = AttributeBase.GetAttributes(method).Distinct(cfg.Attributes, c => c is TestingAttribute test && null != test.Method && !test.Method.Equals(method.Name, System.StringComparison.InvariantCultureIgnoreCase), c =>
                {
                    if (c is TestingAttribute test)
                    {
                        test.Method = method.Name;
                    }
                });

                var argAttrs = attributes2.GetAttrs<ArgumentAttribute>(c => !typeof(HttpFileAttribute).IsAssignableFrom(c.Type));

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

                    var route = new Configer.Route(cfg.Info.BusinessName, item.Value.Group, item.Value.OnlyName);
                    if (!Configer.Routes.dictionary.TryAdd(route.ToString(), route))
                    {
                        throw new System.Exception($"Routes exists \"{route}\"");
                    }
                }

                var childAll = new ReadOnlyCollection<Args>();
                var args = new ReadOnlyCollection<Args>(parameters.Length);

                foreach (var argInfo in parameters)
                {
                    var path = argInfo.Name;
                    var parameterType = argInfo.ParameterType.GetTypeInfo();
                    //==================================//
                    var current = GetCurrentType(parameterType);
                    //var currentType = current.outType;
                    var argAttrAll = AttributeBase.GetAttributes(argInfo, current.outType);

                    if (current.hasDictionary)
                    {
                        current.outType = current.outType.GenericTypeArguments[1];
                    }
                    else if (current.hasCollection)
                    {
                        current.outType = current.outType.GenericTypeArguments[0];
                    }

                    //var use = current.hasIArg ? current.inType.GetAttribute<UseAttribute>() : argAttrAll.GetAttr<UseAttribute>();

                    var use = current.hasIArg ? current.inType.GetAttribute<UseAttribute>() ?? argAttrAll.GetAttr<UseAttribute>(c => c.ParameterName) : argAttrAll.GetAttr<UseAttribute>();

                    var hasUse = null != use || (current.hasIArg ? cfg.UseTypes.Contains(current.inType.FullName) : false);
                    //var nick = argAttrAll.GetAttr<NickAttribute>();

                    var hasCollectionAttr = false;
                    if (current.hasCollection)
                    {
                        var collectionAttr = AttributeBase.GetCollectionAttributes(current.outType);
                        if (0 < collectionAttr.Count)
                        {
                            hasCollectionAttr = true; //checked hasCollectionAttr 1
                            argAttrAll.AddRange(collectionAttr);
                        }
                    }

                    argAttrAll = argAttrAll.Distinct(!hasUse ? argAttrs : null);

                    //==================================//
                    var logAttrArg = argAttrAll.GetAttrs<LoggerAttribute>();
                    var inLogAttrArg = current.hasIArg ? AttributeBase.GetAttributes<LoggerAttribute>(current.inType, AttributeBase.DeclaringType.Parameter, GropuAttribute.Comparer) : null;

                    var argGroup = GetArgGroup(argAttrAll, current, path, default, commandGroup, resultType, cfg.ResultTypeDefinition, hasUse, out _, out bool hasCollectionAttr2, argInfo.Name, logAttrArg, inLogAttrArg);

                    var hasDefinition = current.outType.IsDefinition();

                    var definitions = hasDefinition ? new System.Collections.Generic.List<string> { current.outType.FullName } : new System.Collections.Generic.List<string>();

                    var hasLower = false;
                    var childrens2 = hasUse && !current.hasIArg ? new ReadOnlyCollection<Args>(0) : hasDefinition ? new ReadOnlyCollection<Args>() : new ReadOnlyCollection<Args>(0);
                    var children = hasUse && !current.hasIArg ? new ReadOnlyCollection<Args>(0) : hasDefinition ? GetArgChild(current.outType, path, commandGroup, ref definitions, resultType, cfg.ResultTypeDefinition, cfg.UseTypes, out hasLower, argInfo.Name, childrens2) : new ReadOnlyCollection<Args>(0);

                    var arg = new Args(argInfo.Name,
                    argInfo.ParameterType,
                    current.outType,
                    argInfo.Position,
                    argInfo.HasDefaultValue ? argInfo.DefaultValue : default,
                    argInfo.HasDefaultValue,
                    current.hasDictionary,
                    current.hasCollection,
                    hasCollectionAttr || hasCollectionAttr2,
                    current.hasCollection ? typeof(IArg<,>).GetTypeInfo().IsAssignableFrom(current.outType, out _) : false,
                    current.nullable,
                    default,
                    argGroup,
                    children,
                    childrens2,
                    hasLower,
                    hasDefinition,
                    current.hasIArg,
                    current.hasIArg ? current.outType : default,
                    current.hasIArg ? current.inType : default,
                    //path,
                    use,
                    hasUse,
                    typeof(Auth.IToken).IsAssignableFrom(current.hasIArg ? current.inType : current.outType),
                    //item.Value.CommandAttr.OnlyName,
                    GetMethodTypeFullName(parameterType),
                    current.outType.FullName.Replace('+', '.'),
                    hasDefinition ? MemberDefinitionCode.Definition : MemberDefinitionCode.No);

                    args.collection.Add(arg);
                    childAll.collection.Add(arg);

                    foreach (var child in childrens2)
                    {
                        childAll.collection.Add(child);
                    }

                    if (hasUse)
                    {
                        useTypePosition.dictionary.TryAdd(argInfo.Position, current.inType);
                    }
                }

                //var groupDefault = cfg.GetCommandGroup(cfg.Info.CommandGroupDefault, method.Name);
                //var args = argAttrGroup.FirstOrDefault().Value.Args;//[groupDefault].Args;
                var fullName = method.GetMethodFullName();

                var meta = new MetaData(commandGroup, args, childAll, args?.Where(c => c.HasIArg).ToReadOnly(), loggerGroup, $"{space}.{method.Name}", method.Name, fullName, hasAsync, hasReturn, hasIResult, hasIResultGeneric, returnType, cfg.ResultTypeDefinition, resultType, GetDefaultValue(args), attributes2, methodMeta.Key, cfg.GetCommandGroup(cfg.Info.CommandGroupDefault, method.Name), useTypePosition, GetMethodTypeFullName(fullName, args));

                if (!metaData.dictionary.TryAdd(method.Name, meta))
                {
                    throw new System.Exception($"MetaData name exists \"{method.Name}\"");
                }

                //var route = $"{cfg.Info.BusinessName}/{method.Name}";
                //if (!Configer.Routes.dictionary.TryAdd(route, (cfg.Info.BusinessName, method.Name)))
                //{
                //    throw new System.Exception($"Routes name exists \"{route}{System.Environment.NewLine}{Configer.Routes[route].Item1}:{ Configer.Routes[route].Item2}\"");
                //}
#if DEBUG
            };
#else
            });
#endif

            return metaData;
        }

        internal struct CurrentType { public bool hasIArg; public System.Type outType; public System.Type inType; public bool hasCollection; public bool hasDictionary; public System.Type orgType; public bool nullable; }

        internal static CurrentType GetCurrentType(System.Type type)
        {
            var hasIArg = typeof(IArg<,>).GetTypeInfo().IsAssignableFrom(type, out System.Type[] iArgOutType);

            var current = new CurrentType { hasIArg = hasIArg, outType = hasIArg ? iArgOutType[0] : type, inType = hasIArg ? iArgOutType[1] : type };
            var nullType = System.Nullable.GetUnderlyingType(current.outType);
            if (null != nullType)
            {
                current.outType = nullType;
                current.nullable = true;
            }
            current.orgType = current.outType;

            current.hasCollection = current.outType.IsCollection();
            current.hasDictionary = typeof(System.Collections.IDictionary).IsAssignableFrom(current.outType);

            return current;
        }

        #region GetArgAttr        

        internal static System.Collections.Generic.List<ArgumentAttribute> GetArgAttr(System.Collections.Generic.List<AttributeBase> attributes, System.Type memberType, System.Type resultType, System.Type resultTypeDefinition, string path)
        {
            var argAttr = attributes.Where(c => c is ArgumentAttribute).Cast<ArgumentAttribute>().ToList();
            GetArgAttrSort(argAttr);
            return Bind.GetArgAttr(argAttr, resultType, resultTypeDefinition, path, memberType);
        }

        internal static System.Collections.Generic.List<ArgumentAttribute> GetArgAttr(System.Collections.Generic.List<ArgumentAttribute> argAttr, System.Type resultType, System.Type resultTypeDefinition, string path, System.Type memberType)
        {
            //argAttr = argAttr.FindAll(c => c.Enable);
            //argAttr.Sort(ComparisonHelper<Attributes.ArgumentAttribute>.CreateComparer(c =>c.State.ConvertErrorState()));
            //argAttr.Reverse();

            //var procesTypes = new System.Type[] { typeof(object) }; //default
            var procesIArgTypes = new System.Type[] { typeof(object), typeof(IArg) };
            var procesIArgCollectionTypes = new System.Type[] { typeof(object), typeof(IArg), typeof(int), typeof(object) };
            //var argumentAttributeFullName = typeof(ArgumentAttribute).FullName;

            foreach (var item in argAttr)
            {
                //if (string.IsNullOrWhiteSpace(item.Group)) { item.Group = groupDefault; }

                item.Meta.resultType = resultType;
                item.Meta.resultTypeDefinition = resultTypeDefinition;
                //item.Meta.Business = business;
                item.Meta.Member = path;
                item.Meta.MemberType = memberType;

                //!procesIArgMethod.DeclaringType.FullName.Equals(argumentAttributeFullName)
                if (item.Type.GetMethod("Proces", BindingFlags.Public | BindingFlags.Instance, null, procesIArgTypes, null).DeclaringType.FullName.Equals(item.Type.FullName))
                {
                    item.Meta.HasProcesIArg = ArgumentAttribute.MetaData.ProcesMode.ProcesIArg;
                }
                else if (item.Type.GetMethod("Proces", BindingFlags.Public | BindingFlags.Instance, null, procesIArgCollectionTypes, null).DeclaringType.FullName.Equals(item.Type.FullName))
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

        static ReadOnlyCollection<Args> GetArgChild(System.Type type, string path, ConcurrentReadOnlyDictionary<string, CommandAttribute> commands, ref System.Collections.Generic.List<string> definitions, System.Type resultType, System.Type resultTypeDefinition, System.Collections.Generic.IList<string> useTypes, out bool hasLower, string root, ReadOnlyCollection<Args> childrens)
        {
            hasLower = false;

            var args = new ReadOnlyCollection<Args>();

            var position = 0;

            var members = type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.GetField | BindingFlags.GetProperty);

            var definitions2 = new System.Collections.Generic.List<string>(definitions);

            foreach (var item in members)
            {
                System.Type memberType = null;
                Utils.Accessor accessor = default;
                var memberDefinition = MemberDefinitionCode.No;
                switch (item.MemberType)
                {
                    case MemberTypes.Field:
                        {
                            var member = item as FieldInfo;
                            accessor = member.GetAccessor();
                            if (null == accessor.Getter || null == accessor.Setter) { continue; }
                            memberType = member.FieldType;
                            memberDefinition = MemberDefinitionCode.Field;
                        }
                        break;
                    case MemberTypes.Property:
                        {
                            var member = item as PropertyInfo;
                            accessor = member.GetAccessor();
                            if (null == accessor.Getter || null == accessor.Setter) { continue; }
                            memberType = member.PropertyType;
                            memberDefinition = MemberDefinitionCode.Property;
                        }
                        break;
                    default: continue;
                }

                var current = GetCurrentType(memberType);

                var argAttrAll = AttributeBase.GetAttributes(item, current.outType);

                if (current.hasDictionary)
                {
                    current.outType = current.outType.GenericTypeArguments[1];
                }
                else if (current.hasCollection)
                {
                    current.outType = current.outType.GenericTypeArguments[0];
                }

                var hasDefinition = current.outType.IsDefinition();

                if (definitions.Contains(current.outType.FullName)) { continue; }
                else if (hasDefinition) { definitions2.Add(current.outType.FullName); }

                var path2 = $"{path}.{item.Name}";

                //var use = argAttrAll.GetAttr<UseAttribute>();
                var use = current.hasIArg ? current.inType.GetAttribute<UseAttribute>() ?? argAttrAll.GetAttr<UseAttribute>(c => c.ParameterName) : argAttrAll.GetAttr<UseAttribute>();
                var hasUse = null != use || (current.hasIArg ? useTypes.Contains(current.inType.FullName) : false);

                var hasCollectionAttr = false;
                if (current.hasCollection)
                {
                    var collectionAttr = AttributeBase.GetCollectionAttributes(current.outType);
                    if (0 < collectionAttr.Count)
                    {
                        hasCollectionAttr = true;//checked hasCollectionAttr 1
                        argAttrAll.AddRange(collectionAttr);
                    }
                }

                argAttrAll = argAttrAll.Distinct();

                var argGroup = GetArgGroup(argAttrAll, current, path2, path, commands, resultType, resultTypeDefinition, hasUse, out bool hasLower2, out bool hasCollectionAttr2, root);

                var hasLower3 = false;
                var childrens2 = hasDefinition ? new ReadOnlyCollection<Args>() : new ReadOnlyCollection<Args>(0);
                var children = hasDefinition ? GetArgChild(current.outType, path2, commands, ref definitions2, resultType, resultTypeDefinition, useTypes, out hasLower3, root, childrens2) : new ReadOnlyCollection<Args>(0);

                if (hasLower2 || hasLower3)
                {
                    hasLower = true;
                }

                var arg = new Args(item.Name,
                    memberType,
                    current.outType,
                    position++,
                    default,
                    default,
                    current.hasDictionary,
                    current.hasCollection,
                    hasCollectionAttr || hasCollectionAttr2,
                    current.hasCollection ? typeof(IArg<,>).GetTypeInfo().IsAssignableFrom(current.outType, out _) : false,
                    current.nullable,
                    accessor,
                    argGroup,
                    children,
                    childrens2,
                    hasLower2 || hasLower3,
                    hasDefinition,
                    current.hasIArg,
                    current.hasIArg ? current.outType : default,
                    current.hasIArg ? current.inType : default,
                    //path2,
                    use,
                    hasUse,
                    typeof(Auth.IToken).IsAssignableFrom(current.hasIArg ? current.inType : current.outType),
                    GetMethodTypeFullName(memberType),
                    $"{type.FullName.Replace('+', '.')}.{item.Name}",
                    memberDefinition);

                args.collection.Add(arg);
                childrens.collection.Add(arg);

                foreach (var child in childrens2)
                {
                    childrens.collection.Add(child);
                }
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

        static ConcurrentReadOnlyDictionary<string, ArgGroup> GetArgGroup(System.Collections.Generic.List<AttributeBase> argAttrAll, CurrentType current, string path, string owner, ConcurrentReadOnlyDictionary<string, CommandAttribute> commands, System.Type resultType, System.Type resultTypeDefinition, bool hasUse, out bool hasLower, out bool hasCollectionAttr, string root, System.Collections.Generic.List<LoggerAttribute> log = null, System.Collections.Generic.List<LoggerAttribute> inLog = null)
        {
            hasLower = false;
            hasCollectionAttr = false;

            var argAttrs = GetArgAttr(argAttrAll, current.orgType, resultType, resultTypeDefinition, path);

            var nick = argAttrAll.GetAttr<NickAttribute>();

            var argGroup = new ConcurrentReadOnlyDictionary<string, ArgGroup>();

            foreach (var item in commands)
            {
                var ignores = argAttrAll.GetAttrs<Ignore>(c => GroupEquals(c, item.Value.Group), clone: true);

                var ignoreBusinessArg = ignores.Any(c => c.Mode == IgnoreMode.BusinessArg);
                // || (item.Group == c.Group || string.IsNullOrWhiteSpace(c.Group))

                //var argAttrChild = (hasUse || item.Value.IgnoreBusinessArg || ignoreBusinessArg) ?
                var argAttrChild = (hasUse || ignoreBusinessArg) ?
                    argAttrs.FindAll(c => GroupEquals(c, item.Value.Group) && c.Declaring == AttributeBase.DeclaringType.Parameter).Select(c => c.Clone()).ToList() :
                    argAttrs.FindAll(c => GroupEquals(c, item.Value.Group)).Select(c => c.Clone()).ToList();

                var nickValue = string.IsNullOrWhiteSpace(nick?.Nick) ? argAttrChild.Where(c => !string.IsNullOrWhiteSpace(c.Nick) && GroupEquals(c, item.Value.Group)).GroupBy(c => c.Nick, System.StringComparer.InvariantCultureIgnoreCase).FirstOrDefault()?.Key : nick.Nick;

                var argAttr = new ConcurrentLinkedList<ArgumentAttribute>();//argAttrChild.Count

                var path2 = $"{item.Value.OnlyName}.{path}";

                var httpFile = false;

                foreach (var c in argAttrChild)
                {
                    var attr = string.IsNullOrWhiteSpace(c.Group) ? c.Clone() : c;

                    attr.Meta.Method = item.Value.OnlyName;
                    attr.Meta.Member = path2;

                    //attr.Meta.Business = c.Meta.Business;
                    attr.Meta.resultType = c.Meta.resultType;
                    attr.Meta.resultTypeDefinition = c.Meta.resultTypeDefinition;
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

                    if (typeof(HttpFileAttribute).IsAssignableFrom(c.Type) && !httpFile)
                    {
                        httpFile = true;
                    }
                }

                //add default convert
                //if (current.hasIArg && 0 == argAttr.Count)
                if (default == owner && current.hasIArg && null == argAttr.First.Value)
                {
                    //if (default == owner) ?? Is the first level added or every level added?
                    argAttr.TryAdd(new ArgumentDefaultAttribute(resultType, resultTypeDefinition) { Declaring = AttributeBase.DeclaringType.Parameter });

                    if (!hasLower) { hasLower = true; }
                }

                //owner ?? Do need only the superior, not the superior path? For doc
                var group = new ArgGroup(ignores.ToReadOnly(), ignores.Any(c => c.Mode == IgnoreMode.Arg), argAttr, nickValue, path2, default == owner ? item.Value.OnlyName : $"{item.Value.OnlyName}.{owner}", $"{item.Value.OnlyName}.{root}", httpFile);

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

    /// <summary>
    /// Command grouping
    /// </summary>
    public class CommandGroup : ConcurrentReadOnlyDictionary<string, ConcurrentReadOnlyDictionary<string, Command>>
    {
        readonly System.Type resultTypeDefinition;

        readonly string groupDefault;

        public CommandGroup(System.Type resultTypeDefinition, string groupDefault)
        {
            this.resultTypeDefinition = resultTypeDefinition;
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

            return null == command ? Bind.ErrorCmd(resultTypeDefinition, cmd) : command.Call(args, useObj);
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

            return null == command ? await System.Threading.Tasks.Task.FromResult(Bind.ErrorCmd(resultTypeDefinition, cmd)) : await command.AsyncCall(args, useObj);
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
            this.HasArgSingle = 1 >= this.Meta.Args.Count(c => !c.HasToken && !c.Group[Key].IgnoreArg);
            this.HasHttpFile = this.Meta.Args.Any(c => c.Group[Key].HttpFile) || this.Meta.Attributes.Any(c => typeof(HttpFileAttribute).IsAssignableFrom(c.Type));
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
                    var arg = Meta.Args[i];

                    if (Meta.UseTypePosition.ContainsKey(i))
                    {
                        if (0 < useObj?.Length)
                        {
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
                    else if (arg.Group[Key].IgnoreArg)
                    {
                        continue;
                    }

                    if (null != args && 0 < args.Length)
                    {
                        //if (args.Length < l)
                        //{
                        //    break;
                        //}

                        if (l < args.Length)
                        {
                            args2[i] = args[l++];
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
                return ResultFactory.ResultCreate(Meta, 0, System.Convert.ToString(Help.ExceptionWrite(ex)));
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
                    using (var task = System.Threading.Tasks.Task.Factory.StartNew(obj =>
                    {
                        var obj2 = (dynamic)obj;
                        return obj2.call(obj2.args);

                    }, new { call, args = GetAgs(args, useObj) }))
                    {
                        return await task;
                    }
                }
            }
            catch (System.Exception ex)
            {
                return await System.Threading.Tasks.Task.FromResult(ResultFactory.ResultCreate(Meta, 0, System.Convert.ToString(ex.ExceptionWrite())));
            }
        }

        public virtual async System.Threading.Tasks.Task<Result> AsyncCall<Result>(object[] args, params UseEntry[] useObj) => await AsyncCall(args, useObj);

        public virtual async System.Threading.Tasks.Task<IResult> AsyncIResult(object[] args, params UseEntry[] useObj) => await AsyncCall(args, useObj);

        #endregion

        public readonly string Key;

        public MetaData Meta { get; private set; }

        public bool HasArgSingle { get; internal set; }

        public bool HasHttpFile { get; internal set; }
    }
}

namespace Business.Meta
{
    using Business.Utils;

    #region Meta

    public class ArgGroup
    {
        public ArgGroup(string path) => Path = path;

        public ArgGroup(ReadOnlyCollection<Attributes.Ignore> ignore, bool ignoreArg, ConcurrentLinkedList<Attributes.ArgumentAttribute> attrs, string nick, string path, string owner, string root, bool httpFile)
        {
            Ignore = ignore;
            IgnoreArg = ignoreArg;
            Attrs = attrs;
            Path = path;
            Nick = nick;
            Owner = owner;
            Root = root;
            Logger = default;
            IArgInLogger = default;
            HttpFile = httpFile;
        }

        public ReadOnlyCollection<Attributes.Ignore> Ignore { get; private set; }

        public bool IgnoreArg { get; internal set; }

        public ConcurrentLinkedList<Attributes.ArgumentAttribute> Attrs { get; private set; }

        public string Nick { get; private set; }

        public string Path { get; private set; }

        public string Owner { get; private set; }

        public string Root { get; private set; }

        public MetaLogger Logger { get; internal set; }

        public MetaLogger IArgInLogger { get; internal set; }

        public bool HttpFile { get; private set; }
    }

    public enum MemberDefinitionCode
    {
        No,
        Definition,
        Field,
        Property,
    }

    public interface ITypeDefinition<TypeDefinition> where TypeDefinition : ITypeDefinition<TypeDefinition>
    {
        string Name { get; }

        System.Type Type { get; }

        System.Type LastType { get; }

        bool HasDefinition { get; }

        bool HasCollection { get; }

        bool HasDictionary { get; }

        //bool IsEnum { get; set; }

        //bool HasNumeric { get; set; }

        //string[] EnumNames { get; set; }

        //System.Array EnumValues { get; set; }

        //string Path { get; set; }

        //string Summary { get; set; }

        object DefaultValue { get; }

        bool Nullable { get; }

        string FullName { get; }

        MemberDefinitionCode MemberDefinition { get; }

        bool HasToken { get; }

        bool HasDefaultValue { get; }

        ConcurrentReadOnlyDictionary<string, ArgGroup> Group { get; }

        ReadOnlyCollection<TypeDefinition> Children { get; }

        ReadOnlyCollection<TypeDefinition> Childrens { get; }
    }

    /// <summary>
    /// Argument
    /// </summary>
    public class Args : ITypeDefinition<Args>
    {
        //public override string ToString() => string.Format("{0} {1}", Group2, Name);

        //argChild
        public Args(string name, System.Type type, System.Type lastType, int position, object defaultValue, bool hasDefaultValue, bool hasDictionary, bool hasCollection, bool hasCollectionAttr, bool hasCollectionIArg, bool nullable, Accessor accessor, ConcurrentReadOnlyDictionary<string, ArgGroup> group, ReadOnlyCollection<Args> children, ReadOnlyCollection<Args> childrens, bool hasLower, bool hasDefinition, bool hasIArg, System.Type iArgOutType, System.Type iArgInType, Attributes.UseAttribute use, bool useType, bool hasToken, string methodTypeFullName, string fullName, MemberDefinitionCode memberDefinition)
        {
            Name = name;
            Type = type;
            LastType = lastType;
            Position = position;
            HasDictionary = hasDictionary;
            HasCollection = hasCollection;
            HasCollectionAttr = hasCollectionAttr;
            HasCollectionIArg = hasCollectionIArg;
            Nullable = nullable;
            //HasString = hasString;
            Accessor = accessor;
            Group = group;
            Children = children;
            Childrens = childrens;

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
            HasToken = hasToken;
            MethodTypeFullName = methodTypeFullName;
            FullName = fullName;
            MemberDefinition = memberDefinition;
        }

        //===============name==================//
        public string Name { get; private set; }
        //===============type==================//
        public System.Type Type { get; private set; }
        //===============lastType==================//
        public System.Type LastType { get; private set; }
        //===============position==================//
        public int Position { get; private set; }
        //===============defaultValue==================//
        public object DefaultValue { get; private set; }
        //===============hasDefaultValue==================//
        public bool HasDefaultValue { get; private set; }
        public bool HasDictionary { get; private set; }
        //===============hasCollection==================//
        public bool HasCollection { get; private set; }
        //===============hasCollectionAttr==================//
        public bool HasCollectionAttr { get; internal set; }
        //===============hasCollectionIArg==================//
        public bool HasCollectionIArg { get; internal set; }
        //===============nullable==================//
        public bool Nullable { get; private set; }
        //===============accessor==================//
        public Accessor Accessor { get; private set; }
        //===============group==================//
        public ConcurrentReadOnlyDictionary<string, ArgGroup> Group { get; private set; }
        ////===============argAttr==================//
        //public SafeList<Attributes.ArgumentAttribute> ArgAttr { get; private set; }
        //===============children==================//
        public ReadOnlyCollection<Args> Children { get; private set; }
        //===============childrens==================//
        public ReadOnlyCollection<Args> Childrens { get; private set; }
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
        //===============hasToken==================//
        public bool HasToken { get; private set; }
        //==============methodTypeFullName===================//
        public string MethodTypeFullName { get; private set; }
        //==============argTypeFullName===================//
        public string FullName { get; private set; }
        //==============memberDefinition===================//
        /// <summary>
        /// xml using
        /// </summary>
        public MemberDefinitionCode MemberDefinition { get; private set; }
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

        //MetaData
        public MetaData(ConcurrentReadOnlyDictionary<string, Attributes.CommandAttribute> commandGroup, ReadOnlyCollection<Args> args, ReadOnlyCollection<Args> argAll, ReadOnlyCollection<Args> iArgs, ConcurrentReadOnlyDictionary<string, MetaLogger> metaLogger, string path, string name, string fullName, bool hasAsync, bool hasReturn, bool hasIResult, bool hasIResultGeneric, System.Type returnType, System.Type resultTypeDefinition, System.Type resultType, object[] defaultValue, System.Collections.Generic.List<Attributes.AttributeBase> attributes, int position, string groupDefault, ConcurrentReadOnlyDictionary<int, System.Type> useTypePosition, string methodTypeFullName)
        {
            CommandGroup = commandGroup;
            Args = args;
            ArgAll = argAll;
            IArgs = iArgs;
            MetaLogger = metaLogger;
            Path = path;
            Name = name;
            FullName = fullName;

            //this.returnType = returnType;
            //this.hasAsync = Utils.Help.IsAssignableFrom(typeof(System.Threading.Tasks.Task<>).GetTypeInfo(), returnType, out System.Type[] arguments) || typeof(System.Threading.Tasks.Task).IsAssignableFrom(returnType);
            //typeof(void) != method.ReturnType
            HasAsync = hasAsync;// Help.IsAssignableFrom(typeof(System.Threading.Tasks.Task<>), returnType, out System.Type[] arguments) || returnType == typeof(System.Threading.Tasks.Task);
            HasReturn = hasReturn;// !(typeof(void) == returnType || (HasAsync && null == asyncArguments));
            //typeof(IResult).IsAssignableFrom(method.ReturnType),
            //typeof(System.Object).Equals(method.ReturnType)
            //var hasAsyncGeneric = HasAsync && null != asyncArguments;
            HasIResult = hasIResult;// typeof(Result.IResult<>).IsAssignableFrom(hasAsyncGeneric ? asyncArguments[0] : returnType, out System.Type[] resultGeneric) || typeof(Result.IResult).IsAssignableFrom(hasAsyncGeneric ? asyncArguments[0] : returnType);
            HasIResultGeneric = hasIResultGeneric;
            //HasObject = hasObject;// typeof(object).Equals(hasAsyncGeneric ? asyncArguments[0] : returnType);
            ReturnType = returnType;// hasAsyncGeneric ? asyncArguments[0] : returnType;
            HasObject = typeof(object).Equals(returnType);
            ResultTypeDefinition = resultTypeDefinition;
            //ReturnResult = HasIResult ? resultType.MakeGenericType(null != resultGeneric ? resultGeneric[0] : typeof(string)) : null;
            ResultType = resultType;// resultType.MakeGenericType(HasIResult && null != resultGeneric ? resultGeneric[0] : typeof(string));
            ResultGeneric = resultType.GenericTypeArguments[0];

            DefaultValue = defaultValue;
            //this.logAttrs = logAttrs;
            Attributes = attributes;
            Position = position;
            GroupDefault = groupDefault;
            //ArgsFirst = argsFirst;
            UseTypePosition = useTypePosition;
            MethodTypeFullName = methodTypeFullName;
            //Ignore = ignore;
            //HasArgSingle = hasArgSingle;
        }

        //==============commandAttr===================//
        public ConcurrentReadOnlyDictionary<string, Attributes.CommandAttribute> CommandGroup { get; private set; }
        //==============argAttrs===================//
        public ReadOnlyCollection<Args> Args { get; private set; }
        //==============argAll===================//
        public ReadOnlyCollection<Args> ArgAll { get; private set; }
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
        //==============hasIResultGeneric===================//
        public bool HasIResultGeneric { get; private set; }
        //==============hasObject===================//
        public bool HasObject { get; private set; }
        //==============returnType===================//
        public System.Type ReturnType { get; private set; }
        //==============resultTypeDefinition===================//
        public System.Type ResultTypeDefinition { get; private set; }
        //==============resultType===================//
        public System.Type ResultType { get; private set; }
        //==============resultGeneric===================//
        public System.Type ResultGeneric { get; private set; }
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
        //==============hasArgSingle===================//
        //public bool HasArgSingle { get; internal set; }
    }

    #endregion
}
