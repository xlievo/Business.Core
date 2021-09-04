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

namespace Business.Core
{
    using Annotations;
    using Auth;
    using Document;
    using Meta;
    using Result;
    using Utils;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    //internal partial class Bind
    //{
    //    internal static IResult ErrorBusiness(System.Type resultTypeDefinition, string business) => ResultFactory.ResultCreate(resultTypeDefinition, -1, string.Format("Without this Business{0}", string.IsNullOrEmpty(business) ? null : $" {business}"));

    //    internal static IResult ErrorCmd(System.Type resultTypeDefinition, string cmd) => ResultFactory.ResultCreate(resultTypeDefinition, -2, string.Format("Without this Cmd{0}", string.IsNullOrEmpty(cmd) ? null : $" {cmd}"));

    //    #region Internal

    //    //internal static object GetReturnValue(int state, string message, MetaData meta, System.Type resultType) => GetReturnValue(ResultFactory.ResultCreate(resultType, state, message), meta);
    //    /*
    //    internal static dynamic GetReturnValue(IResult result, MetaData meta)
    //    {
    //        //var result2 = (meta.HasIResult || meta.HasObject) ? result : meta.HasReturn && meta.ReturnType.IsValueType ? System.Activator.CreateInstance(meta.ReturnType) : null;
    //        object result2 = result;

    //        //if (!meta.HasObject)
    //        //{
    //        //    if (meta.HasReturn && meta.ReturnType.IsValueType)
    //        //    {
    //        //        result2 = System.Activator.CreateInstance(meta.ReturnType);
    //        //    }
    //        //    else
    //        //    {
    //        //        result2 = null;
    //        //    }
    //        //}

    //        if (meta.HasReturn && meta.ReturnType.IsValueType)
    //        {
    //            result2 = System.Activator.CreateInstance(meta.ReturnType);
    //        }
    //        //else
    //        //{
    //        //    result2 = null;
    //        //}


    //        if (meta.HasAsync)
    //        {
    //            //return meta.HasIResult ? System.Threading.Tasks.Task.FromResult((IResult)result2) : meta.HasReturn ? System.Threading.Tasks.Task.FromResult(result2) : System.Threading.Tasks.Task.Run(() => { });
    //            //if (meta.HasIResult)
    //            //{
    //            //    return System.Threading.Tasks.Task.FromResult((IResult)result2);
    //            //}
    //            //else
    //            //{
    //            //    return System.Threading.Tasks.Task.FromResult(meta.HasReturn ? result2 : null);
    //            //}
    //            //if (meta.HasIResult)
    //            //{
    //            //    return System.Threading.Tasks.Task.FromResult(result2 as IResult<dynamic>);
    //            //}

    //            return System.Threading.Tasks.Task.FromResult(meta.HasIResult ? result2 as IResult : meta.HasReturn ? result2 : null);
    //        }

    //        return result2;
    //    }
    //    */

    //    internal static dynamic GetReturnValue(IResult result, MetaData meta)
    //    {
    //        object result2 = result;

    //        if (meta.HasReturn)
    //        {
    //            if (!meta.HasObject)
    //            {
    //                dynamic result3 = null;

    //                if (meta.ReturnType.IsValueType)
    //                {
    //                    result3 = System.Activator.CreateInstance(meta.ReturnType);
    //                }
    //                //else
    //                //{
    //                //    result3 = Help.CreateInstance(meta.ReturnType);
    //                //}

    //                if (meta.HasAsync)
    //                {
    //                    return System.Threading.Tasks.Task.FromResult<dynamic>(result3);
    //                }

    //                return result3;
    //            }
    //            else if (meta.ReturnType.IsValueType)
    //            {
    //                result2 = System.Activator.CreateInstance(meta.ReturnType);
    //            }
    //        }
    //        else
    //        {

    //            result2 = null;
    //        }

    //        if (meta.HasAsync)
    //        {
    //            return System.Threading.Tasks.Task.FromResult<dynamic>(result2);
    //        }

    //        return result2;
    //    }

    //    internal static dynamic GetReturnValueIResult<Data>(IResult<Data> result, MetaData meta)
    //    {
    //        if (meta.HasAsync)
    //        {
    //            if (meta.HasIResultGeneric)
    //            {
    //                return System.Threading.Tasks.Task.FromResult(result);
    //            }

    //            return System.Threading.Tasks.Task.FromResult(result as IResult);
    //        }

    //        return result;
    //    }
    //    //internal static dynamic GetReturnValueIResultGeneric<Data>(IResult<Data> result, MetaData meta)
    //    //{
    //    //    if (meta.HasIResult || meta.HasObject)
    //    //    {
    //    //        if (meta.HasAsync)
    //    //        {
    //    //            return System.Threading.Tasks.Task.FromResult(result);
    //    //        }
    //    //        else
    //    //        {
    //    //            return result;
    //    //        }
    //    //    }
    //    //    else if (meta.HasReturn && meta.ReturnType.IsValueType)
    //    //    {
    //    //        var resultObj = System.Activator.CreateInstance(meta.ReturnType);

    //    //        if (meta.HasAsync)
    //    //        {
    //    //            return System.Threading.Tasks.Task.FromResult(resultObj);
    //    //        }
    //    //        else
    //    //        {
    //    //            return resultObj;
    //    //        }
    //    //    }

    //    //    return null;
    //    //}

    //    #endregion

    //    #region Create

    //    /*
    //    /// <summary>
    //    /// Initialize a Generic proxy class
    //    /// </summary>
    //    /// <typeparam name="Business"></typeparam>
    //    /// <param name="constructorArguments"></param>
    //    /// <returns></returns>
    //    //public static Business Create<Business>(params object[] constructorArguments)
    //    //    where Business : class => (Business)Create(typeof(Business), null, constructorArguments);

    //    public static Bind Create<Business>(params object[] constructorArguments) => Create(typeof(Business), null, constructorArguments);

    //    /// <summary>
    //    /// Initialize a Generic proxy class
    //    /// </summary>
    //    /// <typeparam name="Business"></typeparam>
    //    /// <param name="interceptor"></param>
    //    /// <param name="constructorArguments"></param>
    //    /// <returns></returns>
    //    //public static Business Create<Business>(Auth.IInterceptor interceptor = null, params object[] constructorArguments)
    //    //    where Business : class => (Business)Create(typeof(Business), interceptor, constructorArguments);

    //    public static Bind Create<Business>(Auth.IInterceptor interceptor = null, params object[] constructorArguments) => Create(typeof(Business), interceptor, constructorArguments);

    //    /// <summary>
    //    /// Initialize a Type proxy class
    //    /// </summary>
    //    /// <param name="type"></param>
    //    /// <param name="constructorArguments"></param>
    //    /// <returns></returns>
    //    //public static IBusiness Create(System.Type type, params object[] constructorArguments) => Create(type, null, constructorArguments) as IBusiness;

    //    public static Bind Create(System.Type type, params object[] constructorArguments) => Create(type, null, constructorArguments);

    //    /// <summary>
    //    /// Initialize a Type proxy class
    //    /// </summary>
    //    /// <param name="type"></param>
    //    /// <param name="interceptor"></param>
    //    /// <param name="constructorArguments"></param>
    //    /// <returns></returns>
    //    //public static object Create(System.Type type, Auth.IInterceptor interceptor = null, params object[] constructorArguments) => new Bind(type, interceptor ?? new Auth.Interceptor(), constructorArguments).Instance;

    //    public static Bind Create(System.Type type, Auth.IInterceptor interceptor = null, params object[] constructorArguments)
    //    {
    //        var bind = new Bind();

    //        bind.bootstrapConfig.type = type;
    //        bind.bootstrapConfig.interceptor = interceptor;
    //        bind.bootstrapConfig.constructorArguments = constructorArguments;

    //        return bind;
    //    }
    //    */
    //    #endregion
    //}

    //class Bind<Business> : Bind
    //{
    //    internal readonly new Business instance;

    //    internal protected Bind(Auth.IInterceptor interceptor, params object[] constructorArguments) : base(interceptor.Create(typeof(Business), constructorArguments), interceptor) => this.instance = (Business)base.instance;
    //}

    partial class Bind : System.IDisposable
    {
        internal readonly object instance;

        internal readonly bool hasBusiness;

        internal protected Bind(System.Type type, System.Type interceptorType, object[] constructorArguments = null, System.Func<System.Type, object> constructorArgumentsFunc = null, System.Type resultType = null, IEnumerable<System.Type> useTypes = null, Logger logger = null, IEnumerable<GroupAttribute> attributes = null)
        {
            var typeInfo = type.GetTypeInfo();

            var interceptor = System.Activator.CreateInstance(interceptorType) as IInterceptor;
            /*
             * Get self reference assembly under single file publishing
             * Exclude duplicate routes
            
            var attributes = AttributeBase.GetTopAttributes(typeInfo);//GetArgAttr(typeInfo);

            var info = attributes.GetAttr<Info>() ?? new Info(type.Name);
            info.TypeFullName = type.FullName.Replace('+', '.');

            if (string.IsNullOrWhiteSpace(info.BusinessName))
            {
                info.BusinessName = type.Name;
            }

            if (string.IsNullOrWhiteSpace(info.Alias))
            {
                info.Alias = info.BusinessName;
            }

            if (Configer.BusinessList.ContainsKey(info.BusinessName))
            {
                return;
            }
            */
            var (methods, ignores) = GetMethods(typeInfo);

            instance = interceptor.Create(type, constructorArguments, constructorArgumentsFunc, ignores);

            //var proxy = new Castle.DynamicProxy.ProxyGenerator();

            //try
            //{
            //    var types = constructorArguments?.Select(c => c.GetType())?.ToArray();

            //    var constructor = null == types ? null : type.GetConstructor(types);

            //    instance = proxy.CreateClassProxy(type, null == constructor ? type.GetConstructors()?.FirstOrDefault()?.GetParameters().Select(c => c.HasDefaultValue ? c.DefaultValue : default).ToArray() : constructorArguments, interceptor);
            //}
            //catch (System.Exception ex)
            //{
            //    throw ex.ExceptionWrite(true, true);
            //}

            //var generics = typeof(IBusiness<,>).IsAssignableFrom(type.GetTypeInfo(), out System.Type[] businessArguments);
            //var resultType = (generics ? businessArguments[0] : typeof(ResultObject<>)).GetGenericTypeDefinition();
            //var argType = (generics ? businessArguments[1] : typeof(Arg<object>)).GetGenericTypeDefinition();
            if (typeof(IBusiness<>).IsAssignableFrom(type.GetTypeInfo(), out System.Type[] businessArguments))
            {
                resultType = businessArguments[0].GetGenericTypeDefinition();
                //argType = businessArguments[1].GetGenericTypeDefinition();
            }
            else //if (typeof(IBusiness).IsAssignableFrom(type))
            {
                resultType = (resultType ?? typeof(ResultObject<>)).GetGenericTypeDefinition();
                //argType = (argType ?? typeof(Arg<>)).GetGenericTypeDefinition();
            }

            var topAttrs = AttributeBase.GetTopAttributes(typeInfo).Distinct(attributes);//GetArgAttr(typeInfo);

            //attributes
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
            var info = topAttrs.GetAttr<Info>() ?? new Info(type.Name);
            info.TypeFullName = type.FullName.Replace('+', '.');

            if (string.IsNullOrWhiteSpace(info.BusinessName))
            {
                info.BusinessName = type.Name;
            }

            if (string.IsNullOrWhiteSpace(info.Alias))
            {
                info.Alias = info.BusinessName;
            }

            hasBusiness = typeof(IBusiness).IsAssignableFrom(type);
            var business = hasBusiness ? (IBusiness)instance : null;
#if !Mobile
            //var cfg = new Configer.Configuration(info, resultType, attributes, typeInfo.GetAttributes<Attributes.EnableWatcherAttribute>().Exists());
#else
            
#endif
            var cfg = new Configer(info, resultType, topAttrs, interceptor, useTypes)
            {
                DocInfo = topAttrs.GetAttr<DocAttribute>()
            };

            business?.BindBefore?.Invoke(cfg);

            try
            {
                cfg.MetaData = GetMetaData(cfg, methods);
                interceptor.Configer = cfg;
                interceptor.Business = instance;
            }
            catch
            {
                throw;
            }

            //interceptor.ResultType = cfg.ResultType;

            if (null != business)
            {
                if (null == business.Logger)
                {
                    business.Logger = logger;
                }

                cfg.Logger = business.Logger;

                business.Configer = cfg;

                GetGroup(business);

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
            var type = instance?.GetType();

            if (null != type)
            {
                if (typeof(IBusiness).IsAssignableFrom(type))
                {
                    var business = (IBusiness)instance;

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
                    ((System.IDisposable)instance).Dispose();
                }
            }
        }

        #region

        static (Dictionary<int, MethodInfo> methods, List<MethodInfo> ignores) GetMethods(TypeInfo type)
        {
            var list = new List<MethodInfo>();

            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public).Where(c => c.IsVirtual && !c.IsFinal);

            var ignores = type.GetProperties(BindingFlags.Instance | BindingFlags.Public).SelectMany(c => c.GetAccessors()).ToList();

            //var methods = type.DeclaredMethods.Where(c => c.IsVirtual && !c.IsFinal && c.IsPublic);

            var methods2 = methods.Except(ignores);

            foreach (var item in methods2)
            {
                //if (item.DeclaringType.Equals(type))
                //{

                //}
                //list.Add(new MethodMeta { Ignore = item.GetAttributes<Attributes.Ignore>(), Method = item });

                if (item.GetAttributes<Ignore>().Any(c => IgnoreMode.Method == c.Mode) || Help.BaseObjectMethods.CompareTo(item))
                {
                    ignores.Add(item);
                }
                else
                {
                    list.Add(item);
                }
            }

            var i = 0;
            //return (ignoreList.ToArray(), list.ToDictionary(c => i++, c => c));
            return (list.ToDictionary(c => i++, c => c), ignores);
        }

        //static System.Collections.Generic.List<T> AssemblyAttr<T>(Assembly member, System.Collections.Generic.IEqualityComparer<T> comparer) where T : System.Attribute => member.GetCustomAttributes<T>().Distinct(comparer).ToList();

        static MetaLogger GetMetaLogger(List<LoggerAttribute> loggers, string group)
        {
            if (null == loggers) { return default; }

            var metaLogger = new MetaLogger();

            var loggers2 = loggers.Where(c => c.GroupEquals(group));

            foreach (var item in loggers2)
            {
                switch (item.LogType)
                {
                    case Logger.Type.Record: metaLogger.Record = item; break;
                    case Logger.Type.Error: metaLogger.Error = item; break;
                    case Logger.Type.Exception: metaLogger.Exception = item; break;
                }
            }

            var all = loggers2.FirstOrDefault(c => c.LogType == Logger.Type.All);

            if (null == metaLogger.Record)
            {
                metaLogger.Record = null == all ? new LoggerAttribute(Logger.Type.Record, false) : all.Clone().SetType(Logger.Type.Record);
            }
            if (null == metaLogger.Error)
            {
                metaLogger.Error = null == all ? new LoggerAttribute(Logger.Type.Error, false) : all.Clone().SetType(Logger.Type.Error);
            }
            if (null == metaLogger.Exception)
            {
                metaLogger.Exception = null == all ? new LoggerAttribute(Logger.Type.Exception, false) : all.Clone().SetType(Logger.Type.Exception);
            }

            return metaLogger;
        }

        //static readonly System.Collections.Generic.List<string> SysTypes = Assembly.GetExecutingAssembly().GetType().Module.Assembly.GetExportedTypes().Select(c => c.FullName).ToList();
        //static bool HasDefinition(System.Type type)
        //{
        //    return !SysTypes.Contains(type.FullName) && (type.IsClass || (type.IsValueType && !type.IsEnum && !type.IsArray && !type.IsCollection() && !type.IsEnumerable()));
        //    //return type.IsClass || (type.IsValueType && !type.IsPrimitive && !type.IsEnum && !type.IsArray);
        //    //return !type.FullName.StartsWith("System.") && (type.IsClass || (type.IsValueType && !type.IsPrimitive && !type.IsEnum && !type.IsArray));
        //    //return !type.IsPrimitive && !type.IsEnum && !type.IsArray && !type.IsSecurityTransparent;
        //}

        static object[] GetDefaultValue(IList<Args> args)
        {
            if (null == args) { return System.Array.Empty<object>(); }

            var argsObj = new object[args.Count];

            for (int i = 0; i < args.Count; i++)
            {
                var arg = args[i];

                //if (arg.HasIArg && !arg.HasCast)
                //{
                //    continue;
                //}

                if (!arg.Nullable && arg.LastType.IsValueType && null == arg.DefaultValue)
                {
                    argsObj[i] = System.Activator.CreateInstance(arg.LastType);
                }
                else if (arg.HasDefaultValue)
                {
                    argsObj[i] = arg.DefaultValue;
                }
            }

            return argsObj;
        }

        /*
        static object[] GetArgsObj(object[] defaultObj, object[] argsObj, IList<Args> args)
        {
            var defaultObj2 = new object[defaultObj.Length];
            System.Array.Copy(defaultObj, defaultObj2, defaultObj2.Length);

            if (null != argsObj)
            {
                for (int i = 0; i < argsObj.Length; i++)
                {
                    var obj = argsObj[i];

                    if (!Equals(null, obj) && i < defaultObj2.Length)
                    {
                        if (!Equals(defaultObj2[i], obj))
                        {
                            var arg = args[i];
                            //int/long
                            //defaultObj2[i] = args[i].HasIArg ? argsObj[i] : Help.ChangeType(argsObj[i], args[i].Type);
                            //if (args[i].Nullable && args[i].LastType.GetTypeCode() == System.TypeCode.DateTime && string.IsNullOrEmpty(System.Convert.ToString(argsObj[i])))
                            //{
                            //    argsObj[i] = null;
                            //}
                            //if (args[i].LastType.Equals(typeof(System.DateTime)) || args[i].LastType.Equals(typeof(System.DateTimeOffset)))
                            //{
                            //    defaultObj2[i] = Help.ChangeType(argsObj[i], args[i].LastType);
                            //    continue;
                            //}
                            if (arg.UseType || arg.HasDefinition)
                            {
                                defaultObj2[i] = obj;
                            }
                            else
                            {
                                //if (arg.Nullable && string.IsNullOrEmpty(obj.ToString()))//???
                                //{
                                //    continue;
                                //}

                                defaultObj2[i] = arg.HasCollection || arg.HasDynamicObject || arg.HasObject ? obj : Help.ChangeType(obj, arg.LastType);
                            }
                            //defaultObj2[i] = args[i].UseType || args[i].HasDefinition ? argsObj[i] : Help.ChangeType(argsObj[i], args[i].LastType);

                            //defaultObj2[i] = args[i].UseType || (args[i].HasIArg && !args[i].HasCast) ? argsObj[i] : Help.ChangeType(argsObj[i], args[i].LastType);

                            //defaultObj2[i] = args[i].UseType || args[i].HasIArg ? argsObj[i] : Help.ChangeType(argsObj[i], args[i].Type);
                        }
                    }
                }
            }

            //foreach (var item in iArgs)
            //{
            //    if (item.HasCast || (null == defaultObj2[item.Position] && !item.Type.IsValueType))
            //    {
            //        continue;
            //    }

            //    var iArg = (IArg)System.Activator.CreateInstance(item.Type);

            //    //Not entry for value type
            //    if (!(null == defaultObj2[item.Position] && item.IArgInType.IsValueType))
            //    {
            //        iArg.In = defaultObj2[item.Position];
            //    }

            //    //iArg.In = defaultObj2[item.Position];
            //    //iArg.Group = group;

            //    defaultObj2[item.Position] = iArg;
            //}

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
        */
        static Meta.CommandGroup CmdAttrGroup(Configer cfg, string methodName, List<AttributeBase> attributes, List<Ignore> ignore)
        {
            var groupDefault = cfg.Info.CommandGroupDefault;
            var group = new Meta.CommandGroup(new ReadOnlyDictionary<string, CommandAttribute>(), new ReadOnlyDictionary<string, ReadOnlyDictionary<string, CommandAttribute>>());

            //ignore
            var ignores = ignore.Where(c => c.Mode == IgnoreMode.Group).ToList();

            var ignoreAll = ignores.Exists(c => string.IsNullOrWhiteSpace(c.Group));

            //ignore all
            if (ignoreAll)
            {
                attributes.FindAll(c => c is CommandAttribute).ForEach(c => attributes.Remove(c));

                return group;
            }

            var notMethods = new List<CommandAttribute>();

            var isDef = false;

            for (int i = attributes.Count - 1; i >= 0; i--)
            {
                if (attributes[i] is CommandAttribute item)
                {
                    if (string.IsNullOrWhiteSpace(item.Group)) { item.Group = groupDefault; }
                    if (string.IsNullOrWhiteSpace(item.OnlyName)) { item.OnlyName = methodName; }

                    //ignore
                    if (0 < ignores.Count && ignores.Exists(c => c.Group == item.Group))
                    {
                        attributes.Remove(item);
                    }
                    else
                    {
                        if (!isDef && item.Group == groupDefault) { isDef = true; }

                        if (item.Meta.Declaring != AttributeBase.MetaData.DeclaringType.Method)
                        {
                            notMethods.Add(item);
                        }
                        else
                        {
                            var key = cfg.Info.GetCommandGroup(item.Group, item.OnlyName);
                            item.Key = key;
                            group.Group.dictionary.Add(key, item);

                            if (group.Full.TryGetValue(item.Group, out ReadOnlyDictionary<string, CommandAttribute> commands))
                            {
                                commands.dictionary.Add(key, item);
                            }
                            else
                            {
                                group.Full.dictionary.Add(item.Group, new ReadOnlyDictionary<string, CommandAttribute>(new Dictionary<string, CommandAttribute> { { key, item } }));
                            }
                        }
                    }
                }
            }

            foreach (var item in notMethods)
            {
                if (!group.Group.Any(c => c.Value.Group == item.Group))
                {
                    var clone = item.Clone();
                    //clone.Source = AttributeBase.SourceType.Method;
                    clone.OnlyName = methodName;

                    var key = cfg.Info.GetCommandGroup(clone.Group, clone.OnlyName);
                    clone.Key = key;
                    group.Group.dictionary.Add(key, clone);

                    if (group.Full.TryGetValue(clone.Group, out ReadOnlyDictionary<string, CommandAttribute> commands))
                    {
                        commands.dictionary.Add(key, clone);
                    }
                    else
                    {
                        group.Full.dictionary.Add(clone.Group, new ReadOnlyDictionary<string, CommandAttribute>(new Dictionary<string, CommandAttribute> { { key, clone } }));
                    }
                }
            }

            //add default group
            /*if (!group.ContainsKey(groupDefault))*/// && methodName == c.OnlyName
            if (!isDef) //(!group.Values.Any(c => groupDefault == c.Group))
            {
                var key = cfg.Info.GetCommandGroup(groupDefault, methodName);
                var command = new CommandAttribute(methodName) { Group = groupDefault, Key = key };
                group.Group.dictionary.Add(key, command);

                if (group.Full.TryGetValue(groupDefault, out ReadOnlyDictionary<string, CommandAttribute> commands))
                {
                    commands.dictionary.Add(key, command);
                }
                else
                {
                    group.Full.dictionary.Add(groupDefault, new ReadOnlyDictionary<string, CommandAttribute>(new Dictionary<string, CommandAttribute> { { key, command } }));
                }
            }

            return group;
        }

        static void GetGroup(IBusiness business)
        {
            var commandGroup = new CommandGroup(business.Configer.ResultTypeDefinition, business.Configer.Info.CommandGroupDefault);

            var docGroup = new ConcurrentReadOnlyDictionary<DocGroup, System.Collections.Concurrent.ConcurrentQueue<DocInfo>>(DocGroup.comparer);

            //docGroup add
            business.Configer.Attributes.GetAttrs<DocGroupAttribute>(c => AttributeBase.MetaData.DeclaringType.Class == c.Meta.Declaring && !string.IsNullOrWhiteSpace(c.Group)).ForEach(c => docGroup.dictionary.TryAdd(new DocGroup(c), new System.Collections.Concurrent.ConcurrentQueue<DocInfo>()));

            //========================================//

#if DEBUG
            foreach (var item in business.Configer.MetaData)
#else
            System.Threading.Tasks.Parallel.ForEach(business.Configer.MetaData, item =>
#endif
            {
                var meta = item.Value;

                //set all
                foreach (var item2 in meta.CommandGroup.Group)
                {
                    var groups = commandGroup.dictionary.GetOrAdd(item2.Value.Group, key => new ConcurrentReadOnlyDictionary<string, Command>());

                    //if (!groups.dictionary.TryAdd(item2.Value.OnlyName, new Command((arguments, token) =>
                    //{
                    //    var args = GetArgsObj(meta.DefaultValue, arguments, meta.Args);

                    //    return business.Configer.Interceptor.Intercept(meta.Name, args, () => meta.Accessor(business, args), item2.Key, item2.Value, token).Result;
                    //}, meta, item2.Key, item2.Value.Group)))
                    //{
                    //    throw new System.Exception($"Command \"{item2.Key}\" member \"{item2.Value.OnlyName}\" name exists");
                    //}

                    if (!groups.dictionary.TryAdd(item2.Value.OnlyName, new Command(meta, item2.Key, item2.Value.Group, business)))
                    {
                        throw new System.Exception($"Command \"{item2.Key}\" member \"{item2.Value.OnlyName}\" name exists");
                    }
                }

                //if (!string.IsNullOrWhiteSpace(meta.Doc?.Alias) && !string.IsNullOrWhiteSpace(meta.Doc.Group))
                if (!string.IsNullOrWhiteSpace(meta.Doc?.Group))
                {
                    docGroup.dictionary.GetOrAdd(new DocGroup(meta.Doc.Group), new System.Collections.Concurrent.ConcurrentQueue<DocInfo>()).Enqueue(new DocInfo(meta.Doc, meta.Position, meta.Name));
                }
#if DEBUG
            };
#else
            });
#endif

            //========================================//

            business.Command = commandGroup;
            business.Configer.DocGroup = docGroup;
        }

        //        static void GetGroup(ConcurrentReadOnlyDictionary<string, CommandAttribute> commandGroup)
        //        {
        //            var commandGroup2 = new ConcurrentReadOnlyDictionary<string, System.Collections.Concurrent.ConcurrentQueue<CommandAttribute>>(System.StringComparer.InvariantCultureIgnoreCase);
        //            //========================================//

        //#if DEBUG
        //            foreach (var item in commandGroup)
        //#else
        //            System.Threading.Tasks.Parallel.ForEach(commandGroup, item =>
        //#endif
        //            {
        //                var meta = item.Value;

        //                var groups2 = commandGroup.dictionary.GetOrAdd(meta.Name, key => new ConcurrentReadOnlyDictionary<string, System.Collections.Concurrent.ConcurrentQueue<CommandAttribute>>(System.StringComparer.InvariantCultureIgnoreCase)).dictionary.GetOrAdd(item2.Value.Group, key => new System.Collections.Concurrent.ConcurrentQueue<CommandAttribute>());
        //                groups2.Enqueue(item2.Value);
        //#if DEBUG
        //            };
        //#else
        //            });
        //#endif

        //            //========================================//

        //            //business.Command = commandGroup;
        //            //business.Configer.CommandGroup = commandGroup2;
        //            //business.Configer.AliasGroup = aliasGroup;
        //        }

        static string GetMethodTypeFullName(string fullName, IList<Args> args) => string.Format("{0}{1}", fullName, (null == args || 0 == args.Count) ? null : string.Format("({0})", null == args ? null : string.Join(",", args.Select(c => c.MethodTypeFullName))));

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

        static ConcurrentReadOnlyDictionary<string, MetaData> GetMetaData(Configer cfg, Dictionary<int, MethodInfo> methods)
        {
            var metaData = new ConcurrentReadOnlyDictionary<string, MetaData>();

#if DEBUG
            foreach (var methodMeta in methods)
#else
            System.Threading.Tasks.Parallel.ForEach(methods, methodMeta =>
#endif
            {
                var method = methodMeta.Value;
                var name = method.Name;

                #region method info

                var hasValueTask = Help.IsAssignableFrom(typeof(System.Threading.Tasks.ValueTask<>), method.ReturnType, out System.Type[] asyncGeneric) || method.ReturnType == typeof(System.Threading.Tasks.ValueTask);
                var hasAsync = Help.IsAssignableFrom(typeof(System.Threading.Tasks.Task<>), method.ReturnType, out System.Type[] asyncGeneric2) || method.ReturnType == typeof(System.Threading.Tasks.Task) || hasValueTask;
                if (null == asyncGeneric)
                {
                    asyncGeneric = asyncGeneric2;
                }

                var hasReturn = !(typeof(void) == method.ReturnType || (hasAsync && null == asyncGeneric));
                var hasAsyncGeneric = hasAsync && null != asyncGeneric;
                var hasIResult = typeof(IResult<>).IsAssignableFrom(hasAsyncGeneric ? asyncGeneric[0] : method.ReturnType, out System.Type[] resultGeneric) || typeof(IResult).IsAssignableFrom(hasAsyncGeneric ? asyncGeneric[0] : method.ReturnType);
                var hasIResultGeneric = hasIResult && null != resultGeneric;
                //var hasObject = typeof(object).Equals(hasAsyncGeneric ? asyncGeneric[0] : method.ReturnType);
                //var returnType = hasAsyncGeneric ? asyncGeneric[0] : method.ReturnType;
                var returnType = hasIResultGeneric ? resultGeneric[0] : hasIResult ? typeof(object) : hasAsyncGeneric ? asyncGeneric[0] : method.ReturnType;
                var resultType = cfg.ResultTypeDefinition.MakeGenericType(hasIResultGeneric ? resultGeneric[0] : typeof(object));

                #endregion

                var attributes2 = AttributeBase.GetAttributes(method).Distinct(cfg.Attributes, c => (c is TestingAttribute test && null != test.Method && !test.Method.Equals(name, System.StringComparison.InvariantCultureIgnoreCase)) || c is DocAttribute || c is DocGroupAttribute, c =>
                {
                    if (c is TestingAttribute test)
                    {
                        test.Method = name;
                    }
                });

                //======Log======//
                var loggers = attributes2.GetAttrs<LoggerAttribute>();

                var ignores = attributes2.GetAttrs<Ignore>();

                //======CmdAttrGroup======//
                var commandGroup = CmdAttrGroup(cfg, name, attributes2, ignores);

                var parameters = method.GetParameters();

                var loggerGroup = new ReadOnlyDictionary<string, MetaLogger>();

                var useTypePosition = new ConcurrentReadOnlyDictionary<int, System.Type>();

                foreach (var item in commandGroup.Group)
                {
                    loggerGroup.dictionary.Add(item.Key, GetMetaLogger(loggers, item.Value.Group));

                    var route = new Configer.Route(cfg.Info.BusinessName, item.Value.Group, item.Value.OnlyName);
                    if (!Configer.Routes.dictionary.TryAdd(route.ToString(), route))
                    {
                        throw new System.Exception($"Routes exists \"{route}\"");
                    }
                }

                var argAttrs = attributes2.Where(c => !typeof(HttpFileAttribute).IsAssignableFrom(c.Meta.Type) && c is ArgumentAttribute).Concat(ignores).ToList();

                //var argAttrs = attributes2.GetAttrs<ArgumentAttribute>(c => !typeof(HttpFileAttribute).IsAssignableFrom(c.Meta.Type));

                var childAll = new ReadOnlyCollection<Args>();
                var args = new ReadOnlyCollection<Args>(parameters.Length);
                var tokens = new ReadOnlyCollection<Args>();
                var hasParameters = false;
                //group.Attrs?.Any(c => c is Annotations.ParametersAttribute) ?? false

                foreach (var argInfo in parameters)
                {
                    var path = argInfo.Name;
                    var parameterType = argInfo.ParameterType;
                    //==================================//
                    var current = Help.GetCurrentType(parameterType.GetTypeInfo());

                    var argAttrAll = AttributeBase.GetAttributes(argInfo, current.type, current.hasCollection ? current.currentType : null);

                    var use = argAttrAll.GetAttr<UseAttribute>();

                    var hasToken = typeof(IToken).IsAssignableFrom(current.type) || typeof(IToken).IsAssignableFrom(use?.ParameterType);//true == use?.Token;

                    var hasUse = hasToken || null != use || cfg.UseTypes.ContainsKey(current.currentType.FullName);
                    
                    argAttrAll = argAttrAll.Distinct(argAttrs);

                    //==================================//
                    var logAttrArg = argAttrAll.GetAttrs<LoggerAttribute>();
                    
                    var definitions = current.hasDefinition ? new System.Collections.Specialized.StringCollection { current.type.FullName } : new System.Collections.Specialized.StringCollection();
                    var hasLower = false;
                    var childrens2 = hasUse ? ReadOnlyCollection<Args>.Empty : current.hasDefinition ? new ReadOnlyCollection<Args>() : ReadOnlyCollection<Args>.Empty;

                    var children = hasUse ? ReadOnlyCollection<Args>.Empty : current.hasDefinition ? GetArgChild(attributes2, current.type, cfg.Info.TypeFullName, cfg.Info.BusinessName, name, path, commandGroup.Group, definitions, resultType, cfg.ResultTypeDefinition, cfg.UseTypes, out hasLower, argInfo.Name, childrens2) : ReadOnlyCollection<Args>.Empty;

                    var arg = new Args(argInfo.Name,
                    //cast ? typeof(Arg<>).GetGenericTypeDefinition().MakeGenericType(parameterType) : parameterType,
                    //cast ? cfg.ArgTypeDefinition.MakeGenericType(parameterType2) : parameterType,
                    parameterType,
                    current.type,
                    current.currentType,
                    argInfo.Position,
                    argInfo.HasDefaultValue ? argInfo.DefaultValue : default,
                    argInfo.HasDefaultValue,
                    current.currentType.IsValueType ? System.Activator.CreateInstance(current.currentType) : null,
                    current.hasDictionary,
                    current.hasCollection,
                    //false,//hasCollectionAttr || hasCollectionAttr2,
                    //current.hasCollection ? typeof(IArg).GetTypeInfo().IsAssignableFrom(current.type, out _) : false,
                    current.nullable,
                    default,
                    null,//argGroup,
                    children,
                    childrens2,
                    hasLower,
                    current.hasDefinition,
                    //path,
                    use,
                    hasUse,
                    hasToken,
                    //item.Value.CommandAttr.OnlyName,
                    GetMethodTypeFullName(parameterType),
                    current.type.GetTypeName(),
                    current.hasDefinition ? MemberDefinitionCode.Definition : MemberDefinitionCode.No,
                    null != argAttrAll.GetAttr<DynamicObjectAttribute>());

                    args.Collection.Add(arg);
                    childAll.Collection.Add(arg);

                    foreach (var child in childrens2)
                    {
                        childAll.Collection.Add(child);
                    }

                    if (hasUse)
                    {
                        useTypePosition.dictionary.TryAdd(argInfo.Position, current.origType);
                    }

                    var argGroup = GetArgGroup(attributes2, argAttrAll, arg, cfg.Info.TypeFullName, cfg.Info.BusinessName, name, path, default, argInfo.Name, commandGroup.Group, resultType, cfg.ResultTypeDefinition, hasUse, out _, argInfo.Name, logAttrArg);
                    //arg.HasCollectionAttr = hasCollectionAttr || hasCollectionAttr2;
                    arg.Group = argGroup;

                    if (hasToken)
                    {
                        tokens.Collection.Add(arg);
                    }

                    if (!hasParameters)
                    {
                        hasParameters = argAttrAll.Any(c => c is ParametersAttribute);
                    }
                }

                var fullName = method.GetMethodFullName();

                var meta = new MetaData(Help.dynamicMethodBuilder.GetDelegate(method), commandGroup, args, childAll, tokens, loggerGroup, method.GetMethodFullName(), name, fullName, cfg.Info.TypeFullName, hasAsync, hasValueTask, hasReturn, hasIResult, hasIResultGeneric, returnType, cfg.ResultTypeDefinition, resultType, GetDefaultValue(args), attributes2, methodMeta.Key, cfg.Info.GetCommandGroup(cfg.Info.CommandGroupDefault, name), useTypePosition, GetMethodTypeFullName(fullName, args), attributes2.GetAttr<DocAttribute>(), hasParameters);

                if (!metaData.dictionary.TryAdd(name, meta))
                {
                    throw new System.Exception($"MetaData name exists \"{name}\"");
                }

                //var route = $"{cfg.Info.BusinessName}/{name}";
                //if (!Configer.Routes.dictionary.TryAdd(route, (cfg.Info.BusinessName, name)))
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

        #region GetArgAttr        
        /*
        internal static System.Collections.Generic.List<ArgumentAttribute> GetArgAttr(System.Collections.Generic.List<AttributeBase> attributes, System.Type memberType, System.Type resultType, System.Type resultTypeDefinition)
        {
            var argAttr = attributes.Where(c => c is ArgumentAttribute).Cast<ArgumentAttribute>().ToList();
            GetArgAttrSort(argAttr);
            return GetArgAttr(argAttr, resultType, resultTypeDefinition, memberType);
        }

        internal static System.Collections.Generic.List<ArgumentAttribute> GetArgAttr(System.Collections.Generic.List<ArgumentAttribute> argAttr, System.Type resultType, System.Type resultTypeDefinition, System.Type memberType)
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

                item.ArgMeta.resultType = resultType;
                item.ArgMeta.resultTypeDefinition = resultTypeDefinition;
                //item.ArgumentMeta.Business = business;
                //item.ArgMeta.MemberPath = path;
                item.ArgMeta.MemberType = memberType;

                //!procesIArgMethod.DeclaringType.FullName.Equals(argumentAttributeFullName)
                if (item.Meta.Type.GetMethod("Proces", BindingFlags.Public | BindingFlags.Instance, null, procesIArgTypes, null).DeclaringType.FullName.Equals(item.Meta.Type.FullName))
                {
                    item.ArgMeta.HasProcesIArg = ArgumentAttribute.MetaData.ProcesMode.ProcesIArg;
                }
                else if (item.Meta.Type.GetMethod("Proces", BindingFlags.Public | BindingFlags.Instance, null, procesIArgCollectionTypes, null).DeclaringType.FullName.Equals(item.Meta.Type.FullName))
                {
                    item.ArgMeta.HasProcesIArg = ArgumentAttribute.MetaData.ProcesMode.ProcesIArgCollection;
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
        */
        #endregion

        static ReadOnlyCollection<Args> GetArgChild(List<AttributeBase> metaAttributes, System.Type type, string declaring, string businessName, string method, string path, IDictionary<string, CommandAttribute> commands, System.Collections.Specialized.StringCollection definitions, System.Type resultType, System.Type resultTypeDefinition, ConcurrentReadOnlyDictionary<string, System.Type> useTypes, out bool hasLower, string root, ReadOnlyCollection<Args> childrens)//, System.Type argTypeDefinition
        {
            hasLower = false;

            var args = new ReadOnlyCollection<Args>();

            var position = 0;

            var members = type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.GetField | BindingFlags.GetProperty);

            foreach (var item in members)
            {
                System.Type memberType = null;
                Accessor accessor = default;
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

                var current = Help.GetCurrentType(memberType);

                var argAttrAll = AttributeBase.GetAttributes(item, current.type, current.hasCollection ? current.currentType : null);

                //var hasDefinition = current.outType.IsDefinition();

                var definitions2 = new System.Collections.Specialized.StringCollection();
                foreach (var def in definitions)
                {
                    definitions2.Add(def);
                }
                if (definitions.Contains(current.type.FullName)) { continue; }
                else if (current.hasDefinition) { definitions2.Add(current.type.FullName); }

                var path2 = $"{path}.{item.Name}";

                //var use = argAttrAll.GetAttr<UseAttribute>();
                var use = argAttrAll.GetAttr<UseAttribute>();

                var hasToken = typeof(IToken).IsAssignableFrom(current.type) || typeof(IToken).IsAssignableFrom(use?.ParameterType);// true == use?.Token;

                var hasUse = hasToken || null != use || useTypes.ContainsKey(current.currentType.FullName);

                argAttrAll = argAttrAll.Distinct();

                var hasLower3 = false;
                var childrens2 = current.hasDefinition ? new ReadOnlyCollection<Args>() : ReadOnlyCollection<Args>.Empty;
                var children = current.hasDefinition ? GetArgChild(metaAttributes, current.type, declaring, businessName, method, path2, commands, definitions2, resultType, resultTypeDefinition, useTypes, out hasLower3, root, childrens2) : ReadOnlyCollection<Args>.Empty;

                var arg = new Args(item.Name,
                    memberType,
                    current.type,
                    current.currentType,
                    position++,
                    default,
                    default,
                    current.currentType.IsValueType ? System.Activator.CreateInstance(current.currentType) : null,
                    current.hasDictionary,
                    current.hasCollection,
                    //false,//hasCollectionAttr || hasCollectionAttr2,
                    //current.hasCollection ? typeof(IArg).GetTypeInfo().IsAssignableFrom(current.type, out _) : false,
                    current.nullable,
                    accessor,
                    null,//argGroup,
                    children,
                    childrens2,
                    false,//hasLower2 || hasLower3,
                    current.hasDefinition,
                    //path2,
                    use,
                    hasUse,
                    hasToken,
                    GetMethodTypeFullName(memberType),
                    $"{type.GetTypeName(item.DeclaringType)}.{item.Name}",
                    memberDefinition,
                    null != argAttrAll.GetAttr<DynamicObjectAttribute>());

                args.Collection.Add(arg);
                childrens.Collection.Add(arg);

                foreach (var child in childrens2)
                {
                    childrens.Collection.Add(child);
                }

                var argGroup = GetArgGroup(metaAttributes, argAttrAll, arg, declaring, businessName, method, path2, path, item.Name, commands, resultType, resultTypeDefinition, hasUse, out bool hasLower2, root);
                arg.Group = argGroup;
                //arg.HasCollectionAttr = hasCollectionAttr || hasCollectionAttr2;
                arg.HasLower = hasLower2 || hasLower3;

                if (hasLower2 || hasLower3)
                {
                    hasLower = true;
                }
            }

            return args;
        }

        //static readonly System.Type[] procesTypes = new System.Type[] { typeof(object) };
        //static readonly System.Type[] procesIArgCollectionTypes = new System.Type[] { typeof(object), typeof(IArg), typeof(int), typeof(object) };

        static ConcurrentReadOnlyDictionary<string, ArgGroup> GetArgGroup(List<AttributeBase> metaAttributes, List<AttributeBase> argAttrAll, Args arg, string declaring, string businessName, string method, string path, string owner, string member, IDictionary<string, CommandAttribute> commands, System.Type resultType, System.Type resultTypeDefinition, bool hasUse, out bool hasLower, string root, List<LoggerAttribute> log = null)
        {
            hasLower = false;
            //hasCollectionAttr = false;

            //var argAttrs = GetArgAttr(argAttrAll, current.orgType, resultType, resultTypeDefinition);

            var argAttrs = argAttrAll.Where(c => c is ArgumentAttribute).Cast<ArgumentAttribute>().ToList();
            argAttrs.Sort(ComparisonHelper<ArgumentAttribute>.CreateComparer(c => System.Math.Abs(c.State.ConvertErrorState())));

            var aliass = argAttrAll.GetAttrs<AliasAttribute>().Where(c => !string.IsNullOrWhiteSpace(c.Name));

            var argGroup = new ConcurrentReadOnlyDictionary<string, ArgGroup>();

            foreach (var item in commands)
            {
                var onlyName = item.Value.OnlyName;
                var group = item.Value.Group;

                var ignores = argAttrAll.GetAttrs<Ignore>(c => c.GroupEquals(group), clone: true);

                var ignoreArg = ignores.Any(c => c.Mode == IgnoreMode.Arg);

                var ignoreBusinessArg = ignores.Any(c => c.Mode == IgnoreMode.BusinessArg);
                // || (item.Group == c.Group || string.IsNullOrWhiteSpace(c.Group))

                //var argAttrChild = (hasUse || item.Value.IgnoreBusinessArg || ignoreBusinessArg) ?
                //var argAttrChild = (hasUse || ignoreBusinessArg) ?
                var argAttrChild = ignoreBusinessArg ?
                    argAttrs.FindAll(c => c.GroupEquals(group) && c.Meta.Declaring == AttributeBase.MetaData.DeclaringType.Parameter).Select(c => c.Clone()).ToList() :
                    argAttrs.FindAll(c => c.GroupEquals(group)).Select(c => c.Clone()).ToList();

                //var nickValue = string.IsNullOrWhiteSpace(nick?.Name) ? argAttrChild.Where(c => !string.IsNullOrWhiteSpace(c.Nick) && Help.GroupEquals(c, item.Value.Group)).GroupBy(c => c.Nick, System.StringComparer.InvariantCultureIgnoreCase).FirstOrDefault()?.Key : nick.Name;
                var alias = aliass.FirstOrDefault(c => c.GroupEquals(group));
                var aliasValue = string.IsNullOrWhiteSpace(alias?.Name) ? argAttrChild.Where(c => !string.IsNullOrWhiteSpace(c.Alias) && c.GroupEquals(group)).FirstOrDefault()?.Alias : alias.Name;
                var aliasValue2 = !string.IsNullOrWhiteSpace(aliasValue) ? aliasValue : Configer.JsonOptionsDoc.PropertyNamingPolicy.ConvertName(member);

                //var argAttr = new ConcurrentLinkedList<ArgumentAttribute>();//argAttrChild.Count
                var argAttr = new ReadOnlyCollection<ArgumentAttribute>();

                var httpFile = false;

                arg.Parameters = argAttrChild.Any(c => c is ParametersAttribute);

                foreach (var c in argAttrChild)
                {
                    c.ArgMeta.Arg = arg;

                    if (ArgumentAttribute.GetFilter(c.ArgMeta.Filter, hasUse, arg.HasDefinition, c.Meta.Declaring))
                    {
                        continue;
                    }

                    if (null != c.ArgMeta.Skip && c.ArgMeta.Skip(hasUse, arg.HasDefinition, c.Meta.Declaring, argAttrChild, ignoreArg, arg.HasDynamicObject))
                    {
                        continue;
                    }

                    //if (!string.IsNullOrWhiteSpace(c.ApplyName) && !c.ApplyName.Equals(member))
                    //{
                    //    continue;
                    //}

                    var attr = string.IsNullOrWhiteSpace(c.Group) ? c.Clone() : c;

                    attr.ArgMeta.resultType = resultType;
                    attr.ArgMeta.resultTypeDefinition = resultTypeDefinition;

                    if (null != attr.ArgMeta.Proces && attr.ArgMeta.Proces.MethodInfo.IsGenericMethod)
                    {
                        attr.ArgMeta.Proces.Call = Help.dynamicMethodBuilder.GetDelegate(attr.ArgMeta.Proces.MethodInfo.MakeGenericMethod(arg.Type));
                    }
                    ////!procesIArgMethod.DeclaringType.FullName.Equals(argumentAttributeFullName)
                    //if (attr.Meta.Type.GetMethod("Proces", BindingFlags.Public | BindingFlags.Instance, null, procesIArgTypes, null).DeclaringType.FullName.Equals(attr.Meta.Type.FullName))
                    //{
                    //    attr.ArgMeta.HasProcesIArg = ArgumentAttribute.MetaData.ProcesMode.ProcesIArg;
                    //}
                    //else if (attr.Meta.Type.GetMethod("Proces", BindingFlags.Public | BindingFlags.Instance, null, procesIArgCollectionTypes, null).DeclaringType.FullName.Equals(attr.Meta.Type.FullName))
                    //{
                    //    attr.ArgMeta.HasProcesIArg = ArgumentAttribute.MetaData.ProcesMode.ProcesIArgCollection;
                    //}

                    attr.ArgMeta.Business = declaring;
                    attr.ArgMeta.BusinessName = businessName;
                    attr.ArgMeta.Method = method;
                    attr.ArgMeta.MethodOnlyName = onlyName;
                    attr.ArgMeta.MemberPath = path;
                    attr.ArgMeta.Member = member;
                    attr.ArgMeta.MemberType = arg.CurrentType;
                    //attr.ArgMeta.MemberOrigType = arg.CurrentOrigType;
                    attr.ArgMeta.Key = item.Key;

                    //attr.ArgumentMeta.Method = item.Value.OnlyName;
                    //attr.ArgumentMeta.Member = path2;
                    ////attr.Meta.Business = c.Meta.Business;
                    //attr.ArgumentMeta.resultType = c.ArgumentMeta.resultType;
                    //attr.ArgumentMeta.resultTypeDefinition = c.ArgumentMeta.resultTypeDefinition;
                    //attr.ArgumentMeta.MemberType = c.ArgumentMeta.MemberType;
                    //attr.ArgumentMeta.HasProcesIArg = c.ArgumentMeta.HasProcesIArg;

                    if (string.IsNullOrWhiteSpace(attr.Alias))
                    {
                        attr.Alias = aliasValue2;
                    }

                    attr.BindAfter?.Invoke();

                    //if (!argAttr.TryAdd(attr))
                    //{
                    //    System.Console.WriteLine("ConcurrentLinkedList TryAdd error! State = INV");
                    //}
                    argAttr.Collection.Add(attr);

                    if (!hasLower) { hasLower = true; }

                    //if (c.CollectionItem && !hasCollectionAttr) { hasCollectionAttr = true; }//checked hasCollectionAttr 2

                    if (typeof(HttpFileAttribute).IsAssignableFrom(c.Meta.Type) && !httpFile)
                    {
                        httpFile = true;
                    }
                }

                //add default convert
                //if (current.hasIArg && 0 == argAttr.Count)
                //if (default == owner && current.hasIArg && null == argAttr.First.Value)
                //if (default == owner && 0 == argAttr.Count)
                //{
                //    //if (default == owner) ?? Is the first level added or every level added?
                //    var def = new ArgumentDefaultAttribute(resultType, resultTypeDefinition);
                //    def.Meta.Declaring = AttributeBase.MetaData.DeclaringType.Parameter;
                //    //argAttr.TryAdd(def);
                //    argAttr.Collection.Add(def);

                //    if (!hasLower) { hasLower = true; }
                //}

                //owner ?? Do need only the superior, not the superior path? For doc
                var group2 = new ArgGroup(ignores.ToReadOnly(), ignoreArg, argAttr, aliasValue, $"{onlyName}.{path}", default == owner ? onlyName : $"{onlyName}.{owner}", $"{onlyName}.{root}", httpFile, arg.HasDefinition || arg.HasCollection ? Command.GetDeserialize(metaAttributes, resultType, resultTypeDefinition, arg.CurrentType, group, aliasValue2) : null);

                if (0 < log?.Count)
                {
                    group2.Logger = GetMetaLogger(log, group);
                }

                //if (0 < inLog?.Count)
                //{
                //    group2.IArgInLogger = GetMetaLogger(inLog, group);
                //}

                argGroup.dictionary.TryAdd(item.Key, group2);
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

        /// <summary>
        /// CommandGroup
        /// </summary>
        /// <param name="resultTypeDefinition"></param>
        /// <param name="groupDefault"></param>
        public CommandGroup(System.Type resultTypeDefinition, string groupDefault)
        {
            this.resultTypeDefinition = resultTypeDefinition;
            this.groupDefault = groupDefault;
        }

        /// <summary>
        /// GetCommand
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public virtual Command GetCommand(string cmd, string group = null)
        {
            if (string.IsNullOrEmpty(cmd))
            {
                return null;
            }

            return !TryGetValue(string.IsNullOrWhiteSpace(group) ? groupDefault : group, out ConcurrentReadOnlyDictionary<string, Command> cmdGroup) || !cmdGroup.TryGetValue(cmd, out Command command) ? null : command;
        }

        #region Call

        /// <summary>
        /// Call
        /// </summary>
        /// <typeparam name="Result"></typeparam>
        /// <param name="cmd"></param>
        /// <param name="token"></param>
        /// <param name="useObj"></param>
        /// <returns></returns>
        public virtual Result Call<Result>(string cmd, IToken token = null, params UseEntry[] useObj) => Call(cmd, null, token, useObj);

        /// <summary>
        /// CallIResult
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="token"></param>
        /// <param name="useObj"></param>
        /// <returns></returns>
        public virtual IResult CallIResult(string cmd, IToken token = null, params UseEntry[] useObj) => Call(cmd, null, token, useObj);

        /// <summary>
        /// Call
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="token"></param>
        /// <param name="useObj"></param>
        /// <returns></returns>
        public virtual dynamic Call(string cmd, IToken token = null, params UseEntry[] useObj) => Call(cmd, null, token, useObj);

        /// <summary>
        /// Call
        /// </summary>
        /// <typeparam name="Result"></typeparam>
        /// <param name="cmd"></param>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <param name="useObj"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public virtual Result Call<Result>(string cmd, object[] parameters = null, IToken token = null, UseEntry[] useObj = null, string group = null) => Call(cmd, parameters, token, useObj, group);

        /// <summary>
        /// CallIResult
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <param name="useObj"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public virtual IResult CallIResult(string cmd, object[] parameters = null, IToken token = null, UseEntry[] useObj = null, string group = null) => Call(cmd, parameters, token, useObj, group);

        /// <summary>
        /// Call
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <param name="useObj"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public virtual dynamic Call(string cmd, object[] parameters = null, IToken token = null, UseEntry[] useObj = null, string group = null)
        {
            var command = GetCommand(cmd, group);

            return null == command ? Help.ErrorCmd(resultTypeDefinition, cmd) : command.Call(parameters, token, useObj);
        }

        /// <summary>
        /// Call
        /// </summary>
        /// <typeparam name="Result"></typeparam>
        /// <param name="cmd"></param>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <param name="group"></param>
        /// <param name="useObj"></param>
        /// <returns></returns>
        public virtual Result Call<Result>(string cmd, object[] parameters = null, IToken token = null, string group = null, params UseEntry[] useObj) => Call(cmd, parameters, token, useObj, group);

        /// <summary>
        /// Call
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <param name="group"></param>
        /// <param name="useObj"></param>
        /// <returns></returns>
        public virtual IResult CallIResult(string cmd, object[] parameters = null, IToken token = null, string group = null, params UseEntry[] useObj) => Call(cmd, parameters, token, useObj, group);

        /// <summary>
        /// Call
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <param name="group"></param>
        /// <param name="useObj"></param>
        /// <returns></returns>
        public virtual dynamic Call(string cmd, object[] parameters = null, IToken token = null, string group = null, params UseEntry[] useObj) => Call(cmd, parameters, token, useObj, group);

        #endregion

        #region Call

        /// <summary>
        /// Call
        /// </summary>
        /// <typeparam name="Result"></typeparam>
        /// <param name="cmd"></param>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <param name="useObj"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public virtual Result Call<Result>(string cmd, IDictionary<string, string> parameters = null, IToken token = null, UseEntry[] useObj = null, string group = null) => Call(cmd, parameters, token, useObj, group);

        /// <summary>
        /// CallIResult
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <param name="useObj"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public virtual IResult CallIResult(string cmd, IDictionary<string, string> parameters = null, IToken token = null, UseEntry[] useObj = null, string group = null) => Call(cmd, parameters, token, useObj, group);

        /// <summary>
        /// Call
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="args"></param>
        /// <param name="token"></param>
        /// <param name="useObj"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public virtual dynamic Call(string cmd, IDictionary<string, string> args, IToken token, UseEntry[] useObj, string group)
        {
            var command = GetCommand(cmd, group);

            return null == command ? Help.ErrorCmd(resultTypeDefinition, cmd) : command.Call(args, token, useObj);
        }

        /// <summary>
        /// Call
        /// </summary>
        /// <typeparam name="Result"></typeparam>
        /// <param name="cmd"></param>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <param name="group"></param>
        /// <param name="useObj"></param>
        /// <returns></returns>
        public virtual Result Call<Result>(string cmd, IDictionary<string, string> parameters = null, IToken token = null, string group = null, params UseEntry[] useObj) => Call(cmd, parameters, token, useObj, group);

        /// <summary>
        /// CallIResult
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <param name="group"></param>
        /// <param name="useObj"></param>
        /// <returns></returns>
        public virtual IResult CallIResult(string cmd, IDictionary<string, string> parameters = null, IToken token = null, string group = null, params UseEntry[] useObj) => Call(cmd, parameters, token, useObj, group);

        /// <summary>
        /// Call
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <param name="group"></param>
        /// <param name="useObj"></param>
        /// <returns></returns>
        public virtual dynamic Call(string cmd, IDictionary<string, string> parameters = null, IToken token = null, string group = null, params UseEntry[] useObj) => Call(cmd, parameters, token, useObj, group);

        #endregion

        #region AsyncCallUse

        /// <summary>
        /// AsyncCall
        /// </summary>
        /// <typeparam name="Result"></typeparam>
        /// <param name="cmd"></param>
        /// <param name="token"></param>
        /// <param name="useObj"></param>
        /// <returns></returns>
        public virtual async System.Threading.Tasks.ValueTask<Result> AsyncCall<Result>(string cmd, IToken token = null, params UseEntry[] useObj) => await AsyncCall(cmd, null, token, useObj);

        /// <summary>
        /// AsyncIResult
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="token"></param>
        /// <param name="useObj"></param>
        /// <returns></returns>
        public virtual async System.Threading.Tasks.ValueTask<IResult> AsyncIResult(string cmd, IToken token = null, params UseEntry[] useObj) => await AsyncCall(cmd, null, token, useObj);

        /// <summary>
        /// AsyncCall
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="token"></param>
        /// <param name="useObj"></param>
        /// <returns></returns>
        public virtual async System.Threading.Tasks.ValueTask<dynamic> AsyncCall(string cmd, IToken token = null, params UseEntry[] useObj) => await AsyncCall(cmd, null, token, useObj);

        /// <summary>
        /// AsyncCall
        /// </summary>
        /// <typeparam name="Result"></typeparam>
        /// <param name="cmd"></param>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <param name="useObj"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public virtual async System.Threading.Tasks.ValueTask<Result> AsyncCall<Result>(string cmd, object[] parameters = null, IToken token = null, UseEntry[] useObj = null, string group = null) => await AsyncCall(cmd, parameters, token, useObj, group);

        /// <summary>
        /// AsyncIResult
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <param name="useObj"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public virtual async System.Threading.Tasks.ValueTask<IResult> AsyncIResult(string cmd, object[] parameters = null, IToken token = null, UseEntry[] useObj = null, string group = null) => await AsyncCall(cmd, parameters, token, useObj, group);

        /// <summary>
        /// AsyncCall
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <param name="useObj"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public virtual async System.Threading.Tasks.ValueTask<dynamic> AsyncCall(string cmd, object[] parameters = null, IToken token = null, UseEntry[] useObj = null, string group = null)
        {
            var command = GetCommand(cmd, group);

            return null == command ? await System.Threading.Tasks.Task.FromResult(Help.ErrorCmd(resultTypeDefinition, cmd)) : await command.AsyncCall(parameters, token, useObj);
        }

        /// <summary>
        /// AsyncIResult
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <param name="group"></param>
        /// <param name="useObj"></param>
        /// <returns></returns>
        public virtual async System.Threading.Tasks.ValueTask<IResult> AsyncIResult(string cmd, object[] parameters = null, IToken token = null, string group = null, params UseEntry[] useObj) => await AsyncCall(cmd, parameters, token, useObj, group);

        /// <summary>
        /// AsyncCall
        /// </summary>
        /// <typeparam name="Result"></typeparam>
        /// <param name="cmd"></param>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <param name="group"></param>
        /// <param name="useObj"></param>
        /// <returns></returns>
        public virtual async System.Threading.Tasks.ValueTask<Result> AsyncCall<Result>(string cmd, object[] parameters = null, IToken token = null, string group = null, params UseEntry[] useObj) => await AsyncCall(cmd, parameters, token, useObj, group);

        /// <summary>
        /// AsyncCall
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <param name="group"></param>
        /// <param name="useObj"></param>
        /// <returns></returns>
        public virtual async System.Threading.Tasks.ValueTask<dynamic> AsyncCall(string cmd, object[] parameters = null, IToken token = null, string group = null, params UseEntry[] useObj) => await AsyncCall(cmd, parameters, token, useObj, group);

        #endregion

        #region AsyncCallUse

        /// <summary>
        /// AsyncCall
        /// </summary>
        /// <typeparam name="Result"></typeparam>
        /// <param name="cmd"></param>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <param name="useObj"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public virtual async System.Threading.Tasks.ValueTask<Result> AsyncCall<Result>(string cmd, IDictionary<string, string> parameters = null, IToken token = null, UseEntry[] useObj = null, string group = null) => await AsyncCall(cmd, parameters, token, useObj, group);

        /// <summary>
        /// AsyncIResult
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <param name="useObj"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public virtual async System.Threading.Tasks.ValueTask<IResult> AsyncIResult(string cmd, IDictionary<string, string> parameters = null, IToken token = null, UseEntry[] useObj = null, string group = null) => await AsyncCall(cmd, parameters, token, useObj, group);

        /// <summary>
        /// AsyncCall
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <param name="useObj"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public virtual async System.Threading.Tasks.ValueTask<dynamic> AsyncCall(string cmd, IDictionary<string, string> parameters, IToken token, UseEntry[] useObj, string group)
        {
            var command = GetCommand(cmd, group);

            return null == command ? await System.Threading.Tasks.Task.FromResult(Help.ErrorCmd(resultTypeDefinition, cmd)) : await command.AsyncCall(parameters, token, useObj);
        }

        /// <summary>
        /// AsyncIResult
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <param name="group"></param>
        /// <param name="useObj"></param>
        /// <returns></returns>
        public virtual async System.Threading.Tasks.ValueTask<IResult> AsyncIResult(string cmd, IDictionary<string, string> parameters = null, IToken token = null, string group = null, params UseEntry[] useObj) => await AsyncCall(cmd, parameters, token, useObj, group);

        /// <summary>
        /// AsyncCall
        /// </summary>
        /// <typeparam name="Result"></typeparam>
        /// <param name="cmd"></param>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <param name="group"></param>
        /// <param name="useObj"></param>
        /// <returns></returns>
        public virtual async System.Threading.Tasks.ValueTask<Result> AsyncCall<Result>(string cmd, IDictionary<string, string> parameters = null, IToken token = null, string group = null, params UseEntry[] useObj) => await AsyncCall(cmd, parameters, token, useObj, group);

        /// <summary>
        /// AsyncCall
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <param name="group"></param>
        /// <param name="useObj"></param>
        /// <returns></returns>
        public virtual async System.Threading.Tasks.ValueTask<dynamic> AsyncCall(string cmd, IDictionary<string, string> parameters = null, IToken token = null, string group = null, params UseEntry[] useObj) => await AsyncCall(cmd, parameters, token, useObj, group);

        #endregion

        //#region AsyncCallFull

        ///// <summary>
        ///// AsyncCallFull
        ///// </summary>
        ///// <param name="cmd"></param>
        ///// <param name="parameters"></param>
        ///// <param name="useObj"></param>
        ///// <param name="group"></param>
        ///// <param name="token"></param>
        ///// <returns></returns>
        //public async System.Threading.Tasks.ValueTask<dynamic> AsyncCallFull(string cmd, string parameters, UseEntry[] useObj = null, string group = null, IToken token = default)
        //{
        //    var command = GetCommand(cmd, group);

        //    return null == command ? await System.Threading.Tasks.Task.FromResult(Help.ErrorCmd(resultTypeDefinition, cmd)) : await command.AsyncCall(parameters, token, useObj);
        //}

        //#endregion
    }

    ///// <summary>
    ///// CallArg
    ///// </summary>
    //public readonly struct CallArg
    //{
    //    /// <summary>
    //    /// CallArg
    //    /// </summary>
    //    /// <param name="group"></param>
    //    /// <param name="token"></param>
    //    /// <param name="multipleParameterDeserialize"></param>
    //    public CallArg(string group, IToken token, bool multipleParameterDeserialize)
    //    {
    //        Group = group;
    //        Token = token;
    //        MultipleParameterDeserialize = multipleParameterDeserialize;
    //    }

    //    /// <summary>
    //    /// Group
    //    /// </summary>
    //    public string Group { get; }

    //    /// <summary>
    //    /// Token
    //    /// </summary>
    //    public IToken Token { get; }

    //    /// <summary>
    //    /// MultipleParameterDeserialize
    //    /// </summary>
    //    public bool MultipleParameterDeserialize { get; }
    //}

    /// <summary>
    /// Command
    /// </summary>
    public class Command
    {
        readonly static System.Reflection.Emit.ModuleBuilder dynamicArgsModule = System.Reflection.Emit.AssemblyBuilder.DefineDynamicAssembly(new AssemblyName() { Name = "DynamicArgsAssembly" }, System.Reflection.Emit.AssemblyBuilderAccess.Run).DefineDynamicModule("MainModule");

        readonly string parametersTypeKey;

        IList<KeyValuePair<string, System.Type>> parametersTypes;

        ArgumentAttribute deserialize;

        readonly System.Func<object[], IToken, bool, dynamic, dynamic> call;

        /// <summary>
        /// Command
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="key"></param>
        /// <param name="group"></param>
        /// <param name="business"></param>
        public Command(MetaData meta, string key, string group, IBusiness business)
        {
            this.call = (arguments, token, multipleParameterDeserialize, origParameters) =>
            {
                var args = GetArgsObj(meta.DefaultValue, arguments, meta.Args);

                return InterceptorExtensions.Intercept(business.Configer.Interceptor, meta.Name, args, null, key, token, multipleParameterDeserialize, origParameters).Result;
            };

            Meta = meta;
            this.key = key;
            this.group = group;
            HasHttpFile = Meta.Args.Any(c => c.Group[key].HttpFile) || Meta.Attributes.Any(c => typeof(HttpFileAttribute).IsAssignableFrom(c.Meta.Type));
            HasReturn = Meta.HasReturn;
            parametersTypeKey = $"{Meta.Business}.{key}";
            StatisArgs();
        }

        static object[] GetArgsObj(object[] defaultObj, object[] argsObj, IList<Args> args)
        {
            var defaultObj2 = new object[defaultObj.Length];
            System.Array.Copy(defaultObj, defaultObj2, defaultObj2.Length);

            if (null != argsObj)
            {
                for (int i = 0; i < argsObj.Length; i++)
                {
                    var obj = argsObj[i];

                    if (!Equals(null, obj) && i < defaultObj2.Length)
                    {
                        if (!Equals(defaultObj2[i], obj))
                        {
                            var arg = args[i];

                            if (arg.UseType || arg.HasDefinition)
                            {
                                defaultObj2[i] = obj;
                            }
                            else
                            {
                                defaultObj2[i] = arg.HasCollection || arg.HasDynamicObject || arg.HasObject ? obj : Help.ChangeType(obj, arg.LastType);
                            }
                        }
                    }
                }
            }

            return defaultObj2;
        }

        /// <summary>
        /// Statistics of real submission parameters and archiving
        /// </summary>
        internal void StatisArgs()
        {
            Parameters = Meta.Args.Where(c => !c.UseType && !c.HasToken && !c.Group[key].IgnoreArg).ToList();

            HasArgSingle = 1 >= Parameters.Count;

            if (1 < Parameters.Count)
            {
                parametersTypes = Parameters.Select(c => new KeyValuePair<string, System.Type>(c.Name, c.CurrentType)).ToList();

                ParametersType = Utils.Emit.Emit.BuildPropertys(dynamicArgsModule, parametersTypeKey, parametersTypes);

                if (null != ParametersType)
                {
                    ParametersType.LoadAccessors(Configer.AccessorsArgs, parametersTypeKey, fields: false, update: true);
                }
                else
                {
                    Configer.AccessorsArgs.dictionary.TryRemove(parametersTypeKey, out _);
                }
            }
            else if (0 < Parameters.Count)
            {
                ParametersType = Parameters[0].CurrentType;
            }

            if (null != ParametersType)
            {
                deserialize = GetDeserialize(Meta.Attributes, Meta.ResultType, Meta.ResultTypeDefinition, ParametersType, group, $"{Meta.CommandGroup.Group[key].OnlyName} Arguments");
            }
        }

        internal static ArgumentAttribute GetDeserialize(List<AttributeBase> attributes, System.Type resultType, System.Type resultTypeDefinition, System.Type parametersType, string group, string alias)
        {
            var deserialize = attributes.FirstOrDefault(c => c is ArgumentAttribute attr && attr.ArgMeta.Deserialize && attr.GroupEquals(group))?.Clone() as ArgumentAttribute;

            if (null != deserialize)
            {
                deserialize.Alias = alias;
                deserialize.ArgMeta.resultType = resultType;
                deserialize.ArgMeta.resultTypeDefinition = resultTypeDefinition;

                deserialize.ArgMeta.MemberType = parametersType;
                if (null != deserialize.ArgMeta.Proces && deserialize.ArgMeta.Proces.MethodInfo.IsGenericMethod)
                {
                    deserialize.ArgMeta.Proces.Call = Help.dynamicMethodBuilder.GetDelegate(deserialize.ArgMeta.Proces.MethodInfo.MakeGenericMethod(deserialize.ArgMeta.MemberType));
                }
            }

            return deserialize;
        }

        ///// <summary>
        ///// Convert parameter object to array
        ///// </summary>
        ///// <param name="data"></param>
        ///// <returns></returns>
        //async System.Threading.Tasks.ValueTask<IResult> GetParametersObjects(string data)
        //{
        //    if (null == deserialize)
        //    {
        //        return Help.ErrorNull(Meta.ResultTypeDefinition, nameof(deserialize));
        //    }

        //    var result = await deserialize.GetProcesResult(data);

        //    if (0 >= result.State || !result.HasData)
        //    {
        //        return result;
        //    }

        //    var parameters = result.Data;
        //    var values = new object[parametersTypes?.Count ?? 0];

        //    if (null != parametersTypes && Configer.AccessorsArgs.TryGetValue(parametersTypeKey, out Accessors accessors))
        //    {
        //        var i = 0;

        //        foreach (var item in parametersTypes)
        //        {
        //            if (accessors.Accessor.TryGetValue(item.Key, out Accessor accessor))
        //            {
        //                values[i] = accessor.Getter(parameters);
        //            }

        //            i++;
        //        }
        //    }

        //    return result.GenericDefinition.ResultCreate(values);
        //}
        /*
        /// <summary>
        /// Convert parameter object to array
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public virtual object[] GetParametersObjects(object parameters)
        {
            if (parameters is null)
            {
                return null;
            }

            if (null != parametersTypes && Configer.AccessorsArgs.TryGetValue(parametersTypeKey, out Accessors accessors))
            {
                var values = new object[parametersTypes.Count()];
                var i = 0;

                foreach (var item in parametersTypes)
                {
                    if (accessors.Accessor.TryGetValue(item.Key, out Accessor accessor))
                    {
                        values[i] = accessor.Getter(parameters);
                    }

                    i++;
                }

                return values;
            }

            return null;
        }
        */

        /// <summary>
        /// GetArgsUse
        /// </summary>
        /// <param name="useObj"></param>
        /// <param name="token"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public virtual object[] GetArgsUse(UseEntry[] useObj, IToken token, System.Action<object[], int, Args, ArgGroup> action)
        {
            var parameters = new object[Meta.Args.Count];

            if (0 < parameters.Length)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    var arg = Meta.Args[i];
                    if (null != token && arg.HasToken)
                    {
                        parameters[i] = token;
                        continue;
                    }

                    arg.Group.TryGetValue(key, out ArgGroup group);

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
                                        parameters[i] = use.Value;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                var hasParameterType = null != arg.Use?.ParameterType;

                                foreach (var use in useObj)
                                {
                                    if (hasParameterType)
                                    {
                                        if (arg.Use.ParameterType.IsAssignableFrom(use.Type))
                                        {
                                            parameters[i] = use.Value;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (Meta.UseTypePosition[i].IsAssignableFrom(use.Type))
                                        {
                                            parameters[i] = use.Value;
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        continue;
                    }
                    else if (group.IgnoreArg)
                    {
                        continue;
                    }

                    action(parameters, i, arg, group);
                }
            }

            return parameters;
        }

        /// <summary>
        /// GetAgs
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <param name="useObj"></param>
        /// <returns></returns>
        public virtual object[] GetAgs(object[] parameters, IToken token, params UseEntry[] useObj)
        {
            int l = 0;
            return GetArgsUse(useObj, token, (args2, i, arg, group) =>
             {
                 if (null != parameters && 0 < parameters.Length)
                 {
                     //if (args.Length < l)
                     //{
                     //    break;
                     //}

                     if (l < parameters.Length)
                     {
                         args2[i] = parameters[l++];
                     }
                 }
             });
        }

        /// <summary>
        /// GetAgs
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <param name="useObj"></param>
        /// <returns></returns>
        public virtual object[] GetAgs(IDictionary<string, string> parameters, IToken token, params UseEntry[] useObj)
        {
            return GetArgsUse(useObj, token, (args2, i, arg, group) =>
             {
                 if (0 < parameters?.Count)
                 {
                     if (arg.Parameters)
                     {
                         args2[i] = parameters;
                     }
                     else if (parameters.TryGetValue(arg.Name, out string value))
                     {
                         args2[i] = value;
                     }
                 }
             });
        }

        #region Call

        /// <summary>
        /// Call
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <param name="useObj"></param>
        /// <returns></returns>
        public dynamic Call(IDictionary<string, string> parameters, IToken token, params UseEntry[] useObj) => Syn(GetAgs(parameters, token, useObj), token);

        /// <summary>
        /// Call
        /// </summary>
        /// <typeparam name="Result"></typeparam>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <param name="useObj"></param>
        /// <returns></returns>
        public Result Call<Result>(IDictionary<string, string> parameters, IToken token, params UseEntry[] useObj) => Call(parameters, token, useObj);

        /// <summary>
        /// CallIResult
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <param name="useObj"></param>
        /// <returns></returns>
        public IResult CallIResult(IDictionary<string, string> parameters, IToken token, params UseEntry[] useObj) => Call(parameters, token, useObj);

        dynamic Syn(object[] parameters, IToken token = default)
        {
            try
            {
                return call(parameters, token, default, default);
            }
            catch (System.Exception ex)
            {
                return ResultFactory.ResultCreate(Meta, 0, System.Convert.ToString(ex.GetBase()));
            }
        }

        /// <summary>
        /// Call
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <param name="useObj"></param>
        /// <returns></returns>
        public dynamic Call(object[] parameters, IToken token, params UseEntry[] useObj) => Syn(GetAgs(parameters, token, useObj), token);

        /// <summary>
        /// Call
        /// </summary>
        /// <typeparam name="Result"></typeparam>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <param name="useObj"></param>
        /// <returns></returns>
        public Result Call<Result>(object[] parameters, IToken token, params UseEntry[] useObj) => Call(parameters, token, useObj);

        /// <summary>
        /// CallIResult
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <param name="useObj"></param>
        /// <returns></returns>
        public IResult CallIResult(object[] parameters, IToken token, params UseEntry[] useObj) => Call(parameters, token, useObj);

        #endregion

        #region AsyncCall

        /// <summary>
        /// AsyncCall
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <param name="useObj"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.ValueTask<dynamic> AsyncCall(IDictionary<string, string> parameters, IToken token, params UseEntry[] useObj) => await Async(GetAgs(parameters, token, useObj), token, default, parameters);

        /// <summary>
        /// AsyncCall
        /// </summary>
        /// <typeparam name="Result"></typeparam>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <param name="useObj"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.ValueTask<Result> AsyncCall<Result>(IDictionary<string, string> parameters, IToken token, params UseEntry[] useObj) => await AsyncCall(parameters, token, useObj);

        /// <summary>
        /// AsyncIResult
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <param name="useObj"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.ValueTask<IResult> AsyncIResult(IDictionary<string, string> parameters, IToken token, params UseEntry[] useObj) => await AsyncCall(parameters, token, useObj);

        async System.Threading.Tasks.ValueTask<dynamic> Async(object[] parameters, IToken token = default, bool multipleParameterDeserialize = false, dynamic origParameters = null)
        {
            try
            {
                if (Meta.HasAsync)
                {
                    if (!Meta.HasReturn)
                    {
                        await call(parameters, token, multipleParameterDeserialize, origParameters);

                        return null;
                    }

                    return await call(parameters, token, multipleParameterDeserialize, origParameters);
                }
                else
                {
                    using (var task = System.Threading.Tasks.Task.Factory.StartNew(obj =>
                    {
                        var obj2 = (dynamic)obj;
                        return call(obj2.parameters, obj2.token, obj2.multipleParameterDeserialize, obj2.origParameters);

                    }, new { parameters, token, multipleParameterDeserialize, origParameters }))
                    {
                        return await task;
                    }
                }
            }
            catch (System.Exception ex)
            {
                return await System.Threading.Tasks.Task.FromResult(ResultFactory.ResultCreate(Meta, 0, System.Convert.ToString(ex.GetBase())));
            }
        }

        /// <summary>
        /// AsyncCall
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <param name="useObj"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.ValueTask<dynamic> AsyncCall(object[] parameters, IToken token, params UseEntry[] useObj) => await Async(GetAgs(parameters, token, useObj), token);

        /// <summary>
        /// AsyncCall
        /// </summary>
        /// <typeparam name="Result"></typeparam>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <param name="useObj"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.ValueTask<Result> AsyncCall<Result>(object[] parameters, IToken token, params UseEntry[] useObj) => await AsyncCall(parameters, token, useObj);

        /// <summary>
        /// AsyncIResult
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <param name="useObj"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.ValueTask<IResult> AsyncIResult(object[] parameters, IToken token, params UseEntry[] useObj) => await AsyncCall(parameters, token, useObj);

        #endregion

        #region AsyncCallFull

        /// <summary>
        /// AsyncCallFull
        /// </summary>
        /// <typeparam name="DataType"></typeparam>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <param name="useObj"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.ValueTask<dynamic> AsyncCallFull<DataType>(DataType parameters, IToken token, params UseEntry[] useObj)
        {
            if (HasArgSingle)
            {
                return await Async(GetAgs(new object[] { parameters }, token, useObj), token, !HasArgSingle, parameters);
            }
            else
            {
                if (null != deserialize && null != parametersTypes && Configer.AccessorsArgs.TryGetValue(parametersTypeKey, out Accessors accessors))
                {
                    var result = await deserialize.GetProcesResult(parameters, token);

                    if (0 >= result.State || !result.HasData)
                    {
                        return result;
                    }

                    var values = new object[parametersTypes?.Count ?? 0];
                    var i = 0;

                    foreach (var item in parametersTypes)
                    {
                        if (accessors.Accessor.TryGetValue(item.Key, out Accessor accessor))
                        {
                            values[i] = accessor.Getter(result.Data);
                        }

                        i++;
                    }

                    return await Async(GetAgs(values, token, useObj), token, !HasArgSingle, parameters);
                }

                return Help.ErrorNull(Meta.ResultTypeDefinition, nameof(deserialize));
            }
        }

        /// <summary>
        /// AsyncCallFull
        /// </summary>
        /// <typeparam name="Result"></typeparam>
        /// <typeparam name="DataType"></typeparam>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <param name="useObj"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.ValueTask<Result> AsyncCallFull<Result, DataType>(DataType parameters, IToken token, params UseEntry[] useObj) => await AsyncCallFull(parameters, token, useObj);

        /// <summary>
        /// AsyncCallFullIResult
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <param name="useObj"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.ValueTask<IResult> AsyncIResultFull<DataType>(DataType parameters, IToken token, params UseEntry[] useObj) => await AsyncCallFull(parameters, token, useObj);

        #endregion

        /// <summary>
        /// Key
        /// </summary>
        public readonly string key;

        /// <summary>
        /// Generate only documents for the specified group
        /// </summary>
        public readonly string group;

        /// <summary>
        /// Meta
        /// </summary>
        public MetaData Meta { get; private set; }

        /// <summary>
        /// HasArgSingle
        /// </summary>
        public bool HasArgSingle { get; internal set; }

        /// <summary>
        /// HasHttpFile
        /// </summary>
        public bool HasHttpFile { get; internal set; }

        /// <summary>
        /// HasReturn
        /// </summary>
        public bool HasReturn { get; internal set; }

        ///// <summary>
        ///// Parameter type called
        ///// </summary>
        //public System.Type ParametersType { get; internal set; }

        /// <summary>
        /// Parameter list without token
        /// </summary>
        public IList<Args> Parameters { get; internal set; }

        /// <summary>
        /// Parameter type called
        /// </summary>
        public System.Type ParametersType { get; internal set; }
    }
}

namespace Business.Core.Meta
{
    using Annotations;
    using Utils;

    #region Meta

    /// <summary>
    /// ArgGroup
    /// </summary>
    public class ArgGroup
    {
        /// <summary>
        /// ArgGroup
        /// </summary>
        /// <param name="path"></param>
        public ArgGroup(string path) => Path = path;

        /// <summary>
        /// ArgGroup
        /// </summary>
        /// <param name="ignore"></param>
        /// <param name="ignoreArg"></param>
        /// <param name="attrs"></param>
        /// <param name="alias"></param>
        /// <param name="path"></param>
        /// <param name="owner"></param>
        /// <param name="root"></param>
        /// <param name="httpFile"></param>
        /// <param name="deserialize"></param>
        public ArgGroup(ReadOnlyCollection<Ignore> ignore, bool ignoreArg, ReadOnlyCollection<ArgumentAttribute> attrs, string alias, string path, string owner, string root, bool httpFile, ArgumentAttribute deserialize)
        {
            Ignore = ignore;
            IgnoreArg = ignoreArg;
            Attrs = attrs;
            Path = path;
            Alias = alias;
            Owner = owner;
            Root = root;
            Logger = default;
            HttpFile = httpFile;
            Deserialize = deserialize;
        }

        /// <summary>
        /// Ignore
        /// </summary>
        public ReadOnlyCollection<Ignore> Ignore { get; private set; }

        /// <summary>
        /// IgnoreArg
        /// </summary>
        public bool IgnoreArg { get; internal set; }

        //public ConcurrentLinkedList<ArgumentAttribute> Attrs { get; internal set; }
        /// <summary>
        /// Attrs
        /// </summary>
        public ReadOnlyCollection<ArgumentAttribute> Attrs { get; internal set; }

        /// <summary>
        /// Alias
        /// </summary>
        public string Alias { get; }

        /// <summary>
        /// Path
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Owner
        /// </summary>
        public string Owner { get; }

        /// <summary>
        /// Root
        /// </summary>
        public string Root { get; }

        /// <summary>
        /// Logger
        /// </summary>
        public MetaLogger Logger { get; internal set; }

        ///// <summary>
        ///// IArgInLogger
        ///// </summary>
        //public MetaLogger IArgInLogger { get; internal set; }

        /// <summary>
        /// HttpFile
        /// </summary>
        public bool HttpFile { get; }

        /// <summary>
        /// Deserialize
        /// </summary>
        public ArgumentAttribute Deserialize { get; }
    }

    /// <summary>
    /// MemberDefinitionCode
    /// </summary>
    public enum MemberDefinitionCode
    {
        /// <summary>
        /// No
        /// </summary>
        No,

        /// <summary>
        /// Definition
        /// </summary>
        Definition,

        /// <summary>
        /// Field
        /// </summary>
        Field,

        /// <summary>
        /// Property
        /// </summary>
        Property,
    }

    /// <summary>
    /// ITypeDefinition
    /// </summary>
    /// <typeparam name="TypeDefinition"></typeparam>
    public interface ITypeDefinition<TypeDefinition> where TypeDefinition : ITypeDefinition<TypeDefinition>
    {
        /// <summary>
        /// Name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Type
        /// </summary>
        System.Type Type { get; }

        /// <summary>
        /// LastType
        /// </summary>
        System.Type LastType { get; }

        /// <summary>
        /// Remove IArg Null type
        /// </summary>
        System.Type CurrentType { get; }

        /// <summary>
        /// HasDefinition
        /// </summary>
        bool HasDefinition { get; }

        /// <summary>
        /// HasCollection
        /// </summary>
        bool HasCollection { get; }

        /// <summary>
        /// HasDictionary
        /// </summary>
        bool HasDictionary { get; }

        //bool IsEnum { get; set; }

        //bool HasNumeric { get; set; }

        //string[] EnumNames { get; set; }

        //System.Array EnumValues { get; set; }

        //string Path { get; set; }

        //string Summary { get; set; }

        /// <summary>
        /// DefaultValue
        /// </summary>
        object DefaultValue { get; }

        /// <summary>
        /// Nullable
        /// </summary>
        bool Nullable { get; }

        /// <summary>
        /// FullName
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// MemberDefinition
        /// </summary>
        MemberDefinitionCode MemberDefinition { get; }

        /// <summary>
        /// HasToken
        /// </summary>
        bool HasToken { get; }

        /// <summary>
        /// HasDefaultValue
        /// </summary>
        bool HasDefaultValue { get; }

        /// <summary>
        /// Group
        /// </summary>
        ConcurrentReadOnlyDictionary<string, ArgGroup> Group { get; }

        /// <summary>
        /// Children
        /// </summary>
        ReadOnlyCollection<TypeDefinition> Children { get; }

        /// <summary>
        /// Childrens
        /// </summary>
        ReadOnlyCollection<TypeDefinition> Childrens { get; }

        /// <summary>
        /// Dynamic object, DocUI string type display
        /// </summary>
        bool HasDynamicObject { get; }
    }

    /// <summary>
    /// Argument
    /// </summary>
    public class Args : ITypeDefinition<Args>
    {
        //public override string ToString() => string.Format("{0} {1}", Group2, Name);

        //argChild
        /// <summary>
        /// Argument
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="lastType"></param>
        /// <param name="currentType"></param>
        /// <param name="position"></param>
        /// <param name="defaultValue"></param>
        /// <param name="hasDefaultValue"></param>
        /// <param name="defaultTypeValue"></param>
        /// <param name="hasDictionary"></param>
        /// <param name="hasCollection"></param>
        /// <param name="nullable"></param>
        /// <param name="accessor"></param>
        /// <param name="group"></param>
        /// <param name="children"></param>
        /// <param name="childrens"></param>
        /// <param name="hasLower"></param>
        /// <param name="hasDefinition"></param>
        /// <param name="use"></param>
        /// <param name="useType"></param>
        /// <param name="hasToken"></param>
        /// <param name="methodTypeFullName"></param>
        /// <param name="fullName"></param>
        /// <param name="memberDefinition"></param>
        /// <param name="hasDynamicObject"></param>
        public Args(string name, System.Type type, System.Type lastType, System.Type currentType, int position, object defaultValue, bool hasDefaultValue, object defaultTypeValue, bool hasDictionary, bool hasCollection, bool nullable, Accessor accessor, ConcurrentReadOnlyDictionary<string, ArgGroup> group, ReadOnlyCollection<Args> children, ReadOnlyCollection<Args> childrens, bool hasLower, bool hasDefinition, UseAttribute use, bool useType, bool hasToken, string methodTypeFullName, string fullName, MemberDefinitionCode memberDefinition, bool hasDynamicObject)
        {
            Name = name;
            Type = type;
            //OrigType = origType;
            LastType = lastType;
            //CurrentOrigType = currentOrigType;
            CurrentType = currentType;
            Position = position;
            HasDictionary = hasDictionary;
            HasCollection = hasCollection;
            //HasCollectionAttr = hasCollectionAttr;
            Nullable = nullable;
            //HasString = hasString;
            Accessor = accessor;
            Group = group;
            Children = children;
            Childrens = childrens;

            HasLower = hasLower;
            HasDefinition = hasDefinition;
            //HasIArg = hasIArg;
            //IArgOutType = iArgOutType;
            //IArgInType = iArgInType;
            //this.trim = trim;
            //Path = path;
            //Source = source;

            DefaultValue = defaultValue;// type.GetTypeInfo().IsValueType ? System.Activator.CreateInstance(type) : null;
            HasDefaultValue = hasDefaultValue;

            DefaultTypeValue = defaultTypeValue;
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
            //HasCast = hasCast;
            HasDynamicObject = hasDynamicObject;
            HasObject = typeof(object).Equals(LastType);
        }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Type
        /// </summary>
        public System.Type Type { get; internal set; }

        ///// <summary>
        ///// OrigType
        ///// </summary>
        //public System.Type OrigType { get; private set; }

        /// <summary>
        /// LastType
        /// </summary>
        public System.Type LastType { get; private set; }

        ///// <summary>
        ///// Remove IArg type
        ///// </summary>
        //public System.Type CurrentOrigType { get; private set; }

        /// <summary>
        /// Remove IArg Null type
        /// </summary>
        public System.Type CurrentType { get; private set; }

        /// <summary>
        /// Position
        /// </summary>
        public int Position { get; private set; }

        /// <summary>
        /// DefaultValue
        /// </summary>
        public object DefaultValue { get; private set; }

        /// <summary>
        /// HasDefaultValue
        /// </summary>
        public bool HasDefaultValue { get; private set; }

        /// <summary>
        /// DefaultTypeValue
        /// </summary>
        public object DefaultTypeValue { get; private set; }

        /// <summary>
        /// HasDictionary
        /// </summary>
        public bool HasDictionary { get; private set; }

        /// <summary>
        /// HasCollection
        /// </summary>
        public bool HasCollection { get; private set; }
        //===============hasCollectionAttr==================//
        //public bool HasCollectionAttr { get; internal set; }

        /// <summary>
        /// Nullable
        /// </summary>
        public bool Nullable { get; private set; }

        /// <summary>
        /// Accessor
        /// </summary>
        public Accessor Accessor { get; private set; }

        /// <summary>
        /// Group
        /// </summary>
        public ConcurrentReadOnlyDictionary<string, ArgGroup> Group { get; internal set; }
        ////===============argAttr==================//
        //public SafeList<Attributes.ArgumentAttribute> ArgAttr { get; private set; }

        /// <summary>
        /// Children
        /// </summary>
        public ReadOnlyCollection<Args> Children { get; private set; }

        /// <summary>
        /// Childrens
        /// </summary>
        public ReadOnlyCollection<Args> Childrens { get; private set; }

        /// <summary>
        /// Whether there are children
        /// </summary>
        public bool HasLower { get; internal set; }

        /// <summary>
        /// HasDefinition
        /// </summary>
        public bool HasDefinition { get; private set; }

        ///// <summary>
        ///// IArgOutType
        ///// </summary>
        //public System.Type IArgOutType { get; private set; }

        ///// <summary>
        ///// IArgInType
        ///// </summary>
        //public System.Type IArgInType { get; private set; }
        ////==============path===================//
        //public string Path { get; private set; }
        ////==============source===================//
        //public string Source { get; private set; }

        ///// <summary>
        ///// HasIArg
        ///// </summary>
        //public bool HasIArg { get; internal set; }
        //public MetaLogger Logger { get; private set; }
        //public MetaLogger IArgInLogger { get; private set; }
        //==============group===================//
        //public string Group2 { get; private set; }
        //==============owner===================//
        //public string Owner { get; private set; }
        ////==============ignore===================//
        //public ReadOnlyCollection<Attributes.Ignore> Ignore { get; private set; }

        /// <summary>
        /// Use
        /// </summary>
        public UseAttribute Use { get; internal set; }

        /// <summary>
        /// UseType
        /// </summary>
        public bool UseType { get; internal set; }

        /// <summary>
        /// HasToken
        /// </summary>
        public bool HasToken { get; private set; }

        /// <summary>
        /// MethodTypeFullName
        /// </summary>
        public string MethodTypeFullName { get; private set; }

        /// <summary>
        /// FullName
        /// </summary>
        public string FullName { get; private set; }

        /// <summary>
        /// xml using
        /// </summary>
        public MemberDefinitionCode MemberDefinition { get; private set; }

        ///// <summary>
        ///// HasCast
        ///// </summary>
        //public bool HasCast { get; internal set; }

        /// <summary>
        /// Parameters
        /// </summary>
        public bool Parameters { get; internal set; }

        /// <summary>
        /// Dynamic object, DocUI string type display
        /// </summary>
        public bool HasDynamicObject { get; }

        /// <summary>
        /// HasObject
        /// </summary>
        public bool HasObject { get; }
    }

    /// <summary>
    /// MetaLogger
    /// </summary>
    public struct MetaLogger
    {
        /// <summary>
        /// Record
        /// </summary>
        public LoggerAttribute Record { get; set; }

        /// <summary>
        /// Error
        /// </summary>
        public LoggerAttribute Error { get; set; }

        /// <summary>
        /// Exception
        /// </summary>
        public LoggerAttribute Exception { get; set; }
    }

    /// <summary>
    /// CommandGroup
    /// </summary>
    public readonly struct CommandGroup
    {
        /// <summary>
        /// CommandGroup
        /// </summary>
        /// <param name="group"></param>
        /// <param name="full"></param>
        public CommandGroup(ReadOnlyDictionary<string, CommandAttribute> group, ReadOnlyDictionary<string, ReadOnlyDictionary<string, CommandAttribute>> full) { Group = group; Full = full; }

        /// <summary>
        /// Group
        /// </summary>
        public ReadOnlyDictionary<string, CommandAttribute> Group { get; }

        /// <summary>
        /// Full
        /// </summary>
        public ReadOnlyDictionary<string, ReadOnlyDictionary<string, CommandAttribute>> Full { get; }
    }

    /// <summary>
    /// MetaData
    /// </summary>
    public readonly struct MetaData
    {
        /// <summary>
        /// Name
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Name;

        /// <summary>
        /// MetaData
        /// </summary>
        /// <param name="accessor"></param>
        /// <param name="commandGroup"></param>
        /// <param name="args"></param>
        /// <param name="argAll"></param>
        /// <param name="tokens"></param>
        /// <param name="metaLogger"></param>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <param name="fullName"></param>
        /// <param name="business"></param>
        /// <param name="hasAsync"></param>
        /// <param name="hasValueTask"></param>
        /// <param name="hasReturn"></param>
        /// <param name="hasIResult"></param>
        /// <param name="hasIResultGeneric"></param>
        /// <param name="returnType"></param>
        /// <param name="resultTypeDefinition"></param>
        /// <param name="resultType"></param>
        /// <param name="defaultValue"></param>
        /// <param name="attributes"></param>
        /// <param name="position"></param>
        /// <param name="groupDefault"></param>
        /// <param name="useTypePosition"></param>
        /// <param name="methodTypeFullName"></param>
        /// <param name="doc"></param>
        /// <param name="hasParameters"></param>
        public MetaData(System.Func<object, object[], object> accessor, CommandGroup commandGroup, ReadOnlyCollection<Args> args, ReadOnlyCollection<Args> argAll, ReadOnlyCollection<Args> tokens, ReadOnlyDictionary<string, MetaLogger> metaLogger, string path, string name, string fullName, string business, bool hasAsync, bool hasValueTask, bool hasReturn, bool hasIResult, bool hasIResultGeneric, System.Type returnType, System.Type resultTypeDefinition, System.Type resultType, object[] defaultValue, System.Collections.Generic.List<AttributeBase> attributes, int position, string groupDefault, ConcurrentReadOnlyDictionary<int, System.Type> useTypePosition, string methodTypeFullName, DocAttribute doc, bool hasParameters)
        {
            Accessor = accessor;
            CommandGroup = commandGroup;
            Args = args;
            ArgAll = argAll;
            Tokens = tokens;
            MetaLogger = metaLogger;
            Path = path;
            Name = name;
            FullName = fullName;
            Business = business;
            //this.returnType = returnType;
            //this.hasAsync = Utils.Help.IsAssignableFrom(typeof(System.Threading.Tasks.Task<>).GetTypeInfo(), returnType, out System.Type[] arguments) || typeof(System.Threading.Tasks.Task).IsAssignableFrom(returnType);
            //typeof(void) != method.ReturnType
            HasAsync = hasAsync;// Help.IsAssignableFrom(typeof(System.Threading.Tasks.Task<>), returnType, out System.Type[] arguments) || returnType == typeof(System.Threading.Tasks.Task);
            HasValueTask = hasValueTask;
            HasReturn = hasReturn;// !(typeof(void) == returnType || (HasAsync && null == asyncArguments));
            //typeof(IResult).IsAssignableFrom(method.ReturnType),
            //typeof(System.Object).Equals(method.ReturnType)
            //var hasAsyncGeneric = HasAsync && null != asyncArguments;
            HasIResult = hasIResult;// typeof(Result.IResult<>).IsAssignableFrom(hasAsyncGeneric ? asyncArguments[0] : returnType, out System.Type[] resultGeneric) || typeof(Result.IResult).IsAssignableFrom(hasAsyncGeneric ? asyncArguments[0] : returnType);
            HasIResultGeneric = hasIResultGeneric;
            //HasObject = hasObject;// typeof(object).Equals(hasAsyncGeneric ? asyncArguments[0] : returnType);
            ReturnType = returnType;// hasAsyncGeneric ? asyncArguments[0] : returnType;
            ReturnValueTaskType = hasReturn ? typeof(System.Threading.Tasks.ValueTask<>).MakeGenericType(returnType) : null;
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
            Doc = doc;
            HasParameters = hasParameters;
        }

        /// <summary>
        /// Accessor
        /// </summary>
        public System.Func<object, object[], object> Accessor { get; }

        /// <summary>
        /// CommandGroup
        /// </summary>
        public CommandGroup CommandGroup { get; }

        /// <summary>
        /// Args
        /// </summary>
        public ReadOnlyCollection<Args> Args { get; }

        /// <summary>
        /// ArgAll
        /// </summary>
        public ReadOnlyCollection<Args> ArgAll { get; }

        /// <summary>
        /// Tokens
        /// </summary>
        public ReadOnlyCollection<Args> Tokens { get; }

        /// <summary>
        /// MetaLogger
        /// </summary>
        public ReadOnlyDictionary<string, MetaLogger> MetaLogger { get; }

        /// <summary>
        /// Path
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// FullName
        /// </summary>
        public string FullName { get; }

        /// <summary>
        /// Business
        /// </summary>
        public string Business { get; }

        /// <summary>
        /// HasReturn
        /// </summary>
        public bool HasReturn { get; }

        /// <summary>
        /// HasIResult
        /// </summary>
        public bool HasIResult { get; }

        /// <summary>
        /// HasIResultGeneric
        /// </summary>
        public bool HasIResultGeneric { get; }

        /// <summary>
        /// HasObject
        /// </summary>
        public bool HasObject { get; }

        /// <summary>
        /// ReturnType
        /// </summary>
        public System.Type ReturnType { get; }

        /// <summary>
        /// ReturnValueTaskType
        /// </summary>
        public System.Type ReturnValueTaskType { get; }

        /// <summary>
        /// ResultTypeDefinition
        /// </summary>
        public System.Type ResultTypeDefinition { get; }

        /// <summary>
        /// ResultType
        /// </summary>
        public System.Type ResultType { get; }

        /// <summary>
        /// ResultGeneric
        /// </summary>
        public System.Type ResultGeneric { get; }

        /// <summary>
        /// HasAsync
        /// </summary>
        public bool HasAsync { get; }

        /// <summary>
        /// HasValueTask
        /// </summary>
        public bool HasValueTask { get; }

        /// <summary>
        /// DefaultValue
        /// </summary>
        public object[] DefaultValue { get; }

        /// <summary>
        /// Attributes
        /// </summary>
        public System.Collections.Generic.List<AttributeBase> Attributes { get; }

        /// <summary>
        /// Position
        /// </summary>
        public int Position { get; }

        /// <summary>
        /// GroupDefault
        /// </summary>
        public string GroupDefault { get; }
        ////==============argsFirst===================//
        //public ReadOnlyCollection<Args> ArgsFirst { get; private set; }

        /// <summary>
        /// UseTypePosition
        /// </summary>
        public ConcurrentReadOnlyDictionary<int, System.Type> UseTypePosition { get; }

        /// <summary>
        /// MethodTypeFullName
        /// </summary>
        public string MethodTypeFullName { get; }
        ////==============ignore===================//
        //public Attributes.Ignore Ignore { get; private set; }
        //==============hasArgSingle===================//
        //public bool HasArgSingle { get; internal set; }

        /// <summary>
        /// Doc
        /// </summary>
        public DocAttribute Doc { get; }

        /// <summary>
        /// Parameters
        /// </summary>
        public bool HasParameters { get; }

    }

    #endregion
}