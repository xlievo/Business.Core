﻿/*==================================
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
    using Utils;
    using Result;
    using Meta;
    using Annotations;
    using System.Reflection;
    using System.Linq;
    using Business.Core.Document;

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

        internal protected Bind(System.Type type, Auth.IInterceptor interceptor, params object[] constructorArguments)
        {
            var typeInfo = type.GetTypeInfo();

            var methods = GetMethods(typeInfo);

            instance = interceptor.Create(type, constructorArguments);

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

            var generics = typeof(IBusiness<,>).IsAssignableFrom(type.GetTypeInfo(), out System.Type[] businessArguments);

            var resultType = generics ? businessArguments[0].GetGenericTypeDefinition() : typeof(ResultObject<object>).GetGenericTypeDefinition();
            var argType = generics ? businessArguments[1].GetGenericTypeDefinition() : typeof(Arg<object>).GetGenericTypeDefinition();

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

            if (string.IsNullOrWhiteSpace(info.Alias))
            {
                info.Alias = type.Name;
            }

            hasBusiness = typeof(IBusiness).IsAssignableFrom(type);
            var business = hasBusiness ? (IBusiness)instance : null;
#if !Mobile
            //var cfg = new Configer.Configuration(info, resultType, attributes, typeInfo.GetAttributes<Attributes.EnableWatcherAttribute>().Exists());
#else
            
#endif
            var cfg = new Configer(info, resultType, argType, attributes, interceptor)
            {
                DocInfo = attributes.GetAttr<DocAttribute>()
            };

            business?.BindBefore?.Invoke(cfg);

            try
            {
                cfg.MetaData = GetMetaData(cfg, methods);
                interceptor.Configer = cfg;
            }
            catch
            {
                throw;
            }

            //interceptor.ResultType = cfg.ResultType;

            if (null != business)
            {
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

        //static System.Collections.Generic.List<T> AssemblyAttr<T>(Assembly member, System.Collections.Generic.IEqualityComparer<T> comparer) where T : System.Attribute => member.GetCustomAttributes<T>().Distinct(comparer).ToList();

        static MetaLogger GetMetaLogger(System.Collections.Generic.List<LoggerAttribute> loggers, string group)
        {
            if (null == loggers) { return default; }

            var metaLogger = new MetaLogger();

            var loggers2 = loggers.Where(c => Help.GroupEquals(c, group));

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

        static object[] GetDefaultValue(System.Collections.Generic.IList<Args> args)
        {
            if (null == args) { return System.Array.Empty<object>(); }

            var argsObj = new object[args.Count];

            for (int i = 0; i < args.Count; i++)
            {
                var arg = args[i];

                if (arg.HasIArg && !arg.HasCast)
                {
                    continue;
                }

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

        static object[] GetArgsObj(object[] defaultObj, object[] argsObj, System.Collections.Generic.IReadOnlyList<Args> iArgs, System.Collections.Generic.IList<Args> args)
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
                            //if (args[i].Nullable && args[i].LastType.GetTypeCode() == System.TypeCode.DateTime && string.IsNullOrEmpty(System.Convert.ToString(argsObj[i])))
                            //{
                            //    argsObj[i] = null;
                            //}
                            //if (args[i].LastType.Equals(typeof(System.DateTime)) || args[i].LastType.Equals(typeof(System.DateTimeOffset)))
                            //{
                            //    defaultObj2[i] = Help.ChangeType(argsObj[i], args[i].LastType);
                            //    continue;
                            //}
                            if (args[i].UseType || args[i].HasDefinition)
                            {
                                defaultObj2[i] = argsObj[i];
                            }
                            else
                            {
                                if (args[i].Nullable && string.IsNullOrEmpty(argsObj[i]?.ToString()))
                                {
                                    continue;
                                }

                                defaultObj2[i] = Help.ChangeType(argsObj[i], args[i].LastType);
                            }
                            //defaultObj2[i] = args[i].UseType || args[i].HasDefinition ? argsObj[i] : Help.ChangeType(argsObj[i], args[i].LastType);

                            //defaultObj2[i] = args[i].UseType || (args[i].HasIArg && !args[i].HasCast) ? argsObj[i] : Help.ChangeType(argsObj[i], args[i].LastType);

                            //defaultObj2[i] = args[i].UseType || args[i].HasIArg ? argsObj[i] : Help.ChangeType(argsObj[i], args[i].Type);
                        }
                    }
                }
            }

            foreach (var item in iArgs)
            {
                if (item.HasCast || (null == defaultObj2[item.Position] && !item.Type.IsValueType))
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
                //iArg.Group = group;

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

        static Meta.CommandGroup CmdAttrGroup(Configer cfg, string methodName, System.Collections.Generic.List<AttributeBase> attributes, System.Collections.Generic.List<Ignore> ignore)
        {
            var groupDefault = cfg.Info.CommandGroupDefault;
            var group = new Meta.CommandGroup(new ReadOnlyDictionary<string, CommandAttribute>(), new ReadOnlyDictionary<string, ReadOnlyDictionary<string, CommandAttribute>>());

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

                        if (item2.Meta.Declaring != AttributeBase.MetaData.DeclaringType.Method)
                        {
                            notMethods.Add(item2);
                        }
                        else
                        {
                            var key = cfg.Info.GetCommandGroup(item2.Group, item2.OnlyName);
                            item2.Key = key;
                            group.Group.dictionary.Add(key, item2);

                            if (group.Full.TryGetValue(item2.Group, out ReadOnlyDictionary<string, CommandAttribute> commands))
                            {
                                commands.dictionary.Add(key, item2);
                            }
                            else
                            {
                                group.Full.dictionary.Add(item2.Group, new ReadOnlyDictionary<string, CommandAttribute>(new System.Collections.Generic.Dictionary<string, CommandAttribute> { { key, item2 } }));
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
                        group.Full.dictionary.Add(clone.Group, new ReadOnlyDictionary<string, CommandAttribute>(new System.Collections.Generic.Dictionary<string, CommandAttribute> { { key, clone } }));
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
                    group.Full.dictionary.Add(groupDefault, new ReadOnlyDictionary<string, CommandAttribute>(new System.Collections.Generic.Dictionary<string, CommandAttribute> { { key, command } }));
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

                    if (!groups.dictionary.TryAdd(item2.Value.OnlyName, new Command(arguments =>
                    {
                        var args = GetArgsObj(meta.DefaultValue, arguments, meta.IArgs, meta.Args);

                        return business.Configer.Interceptor.Intercept(business.Configer, meta.Name, args, () => meta.Accessor(business, args), item2.Key).Result;
                    }, meta, item2.Key)))
                    {
                        throw new System.Exception($"Command \"{item2.Key}\" member \"{item2.Value.OnlyName}\" name exists");
                    }
                }

                if (!string.IsNullOrWhiteSpace(meta.Doc?.Alias) && !string.IsNullOrWhiteSpace(meta.Doc.Group))
                {
                    docGroup.dictionary.GetOrAdd(new DocGroup { Group = meta.Doc.Group }, new System.Collections.Concurrent.ConcurrentQueue<DocInfo>()).Enqueue(new DocInfo(meta.Doc, meta.Position, meta.Name));
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

        static readonly Utils.Emit.DynamicMethodBuilder dynamicMethodBuilder = new Utils.Emit.DynamicMethodBuilder();

        static ConcurrentReadOnlyDictionary<string, MetaData> GetMetaData(Configer cfg, System.Collections.Generic.Dictionary<int, MethodInfo> methods)
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

                var hasAsync = Help.IsAssignableFrom(typeof(System.Threading.Tasks.Task<>), method.ReturnType, out System.Type[] asyncGeneric) || method.ReturnType == typeof(System.Threading.Tasks.Task);
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

                var doc = attributes2.GetAttr<DocAttribute>();

                if (null != doc && string.IsNullOrWhiteSpace(doc.Alias))
                {
                    doc.Alias = name;
                }

                //======CmdAttrGroup======//
                var commandGroup = CmdAttrGroup(cfg, name, attributes2, ignores);

                var parameters = method.GetParameters();

                var loggerGroup = new ReadOnlyDictionary<string, MetaLogger>();

                var tokenPosition = new System.Collections.Generic.List<int>(parameters.Length);
                //var httpRequestPosition = new System.Collections.Generic.List<int>(parameters.Length);
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

                foreach (var argInfo in parameters)
                {
                    var path = argInfo.Name;
                    var parameterType = argInfo.ParameterType;
                    //==================================//
                    var current = Help.GetCurrentType(parameterType.GetTypeInfo());

                    var argAttrAll = AttributeBase.GetAttributes(argInfo, current.outType, current.hasCollection ? current.orgType : null);

                    //var use = current.hasIArg ? current.inType.GetAttribute<UseAttribute>() : argAttrAll.GetAttr<UseAttribute>();

                    var use = current.hasIArg ? current.inType.GetAttribute<UseAttribute>() ?? argAttrAll.GetAttr<UseAttribute>(c => c.ParameterName) : argAttrAll.GetAttr<UseAttribute>();

                    var hasUse = null != use || (current.hasIArg ? cfg.UseTypes.ContainsKey(current.inType.FullName) : false);
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

                    //argAttrAll = argAttrAll.Distinct(!hasUse ? argAttrs : null);
                    argAttrAll = argAttrAll.Distinct(argAttrs);

                    //==================================//
                    var logAttrArg = argAttrAll.GetAttrs<LoggerAttribute>();
                    var inLogAttrArg = current.hasIArg ? AttributeBase.GetAttributes<LoggerAttribute>(current.inType, AttributeBase.MetaData.DeclaringType.Parameter, GroupAttribute.Comparer) : null;

                    var iArgGenericType = new System.Type[cfg.ArgTypeDefinition.GetTypeInfo().GenericTypeParameters.Length];
                    var parameterType2 = parameterType;
                    var cast = !hasUse && !current.hasIArg;
                    //var cast = !hasUse && current.hasDefinition && !current.hasIArg && current.outType.IsClass;
                    if (cast)
                    {
                        current.hasIArg = true;
                        current.inType = typeof(object);

                        for (int i = 0; i < iArgGenericType.Length; i++)
                        {
                            iArgGenericType[i] = typeof(object);
                        }
                        if (0 < iArgGenericType.Length)
                        {
                            iArgGenericType[0] = parameterType;
                            parameterType2 = cfg.ArgTypeDefinition.MakeGenericType(iArgGenericType);
                        }
                    }

                    var argGroup = GetArgGroup(argAttrAll, current, cfg.Info.TypeFullName, cfg.Info.BusinessName, name, path, default, argInfo.Name, commandGroup.Group, resultType, cfg.ResultTypeDefinition, hasUse, out _, out bool hasCollectionAttr2, argInfo.Name, logAttrArg, inLogAttrArg);

                    var definitions = current.hasDefinition ? new System.Collections.Generic.List<string> { current.outType.FullName } : new System.Collections.Generic.List<string>();
                    var hasLower = false;
                    var childrens2 = hasUse && !current.hasIArg ? ReadOnlyCollection<Args>.Empty : current.hasDefinition ? new ReadOnlyCollection<Args>() : ReadOnlyCollection<Args>.Empty;

                    var children = hasUse && !current.hasIArg ? ReadOnlyCollection<Args>.Empty : current.hasDefinition ? GetArgChild(current.outType, cfg.Info.TypeFullName, cfg.Info.BusinessName, name, path, commandGroup.Group, ref definitions, resultType, cfg.ResultTypeDefinition, cfg.UseTypes, out hasLower, argInfo.Name, childrens2) : ReadOnlyCollection<Args>.Empty;

                    var arg = new Args(argInfo.Name,
                    //cast ? typeof(Arg<>).GetGenericTypeDefinition().MakeGenericType(parameterType) : parameterType,
                    //cast ? cfg.ArgTypeDefinition.MakeGenericType(parameterType2) : parameterType,
                    parameterType2,
                    parameterType,
                    current.outType,
                    argInfo.Position,
                    argInfo.HasDefaultValue ? argInfo.DefaultValue : default,
                    argInfo.HasDefaultValue,
                    current.hasDictionary,
                    current.hasCollection,
                    hasCollectionAttr || hasCollectionAttr2,
                    current.hasCollection ? typeof(IArg).GetTypeInfo().IsAssignableFrom(current.outType, out _) : false,
                    current.nullable,
                    default,
                    argGroup,
                    children,
                    childrens2,
                    hasLower,
                    current.hasDefinition,
                    current.hasIArg,
                    current.hasIArg ? current.outType : default,
                    current.hasIArg ? current.inType : default,
                    //path,
                    use,
                    hasUse,
                    typeof(Auth.IToken).IsAssignableFrom(current.hasIArg ? current.inType : current.outType) || true == use?.Token,
                    //item.Value.CommandAttr.OnlyName,
                    GetMethodTypeFullName(parameterType),
                    current.outType.GetTypeName(),
                    current.hasDefinition ? MemberDefinitionCode.Definition : MemberDefinitionCode.No,
                    cast);

                    args.Collection.Add(arg);
                    childAll.Collection.Add(arg);

                    foreach (var child in childrens2)
                    {
                        childAll.Collection.Add(child);
                    }

                    if (hasUse)
                    {
                        useTypePosition.dictionary.TryAdd(argInfo.Position, current.inType);
                    }
                }

                //var groupDefault = cfg.GetCommandGroup(cfg.Info.CommandGroupDefault, name);
                //var args = argAttrGroup.FirstOrDefault().Value.Args;//[groupDefault].Args;
                var fullName = method.GetMethodFullName();

                var meta = new MetaData(dynamicMethodBuilder.GetDelegate(method), commandGroup, args, childAll, args?.Where(c => c.HasIArg).ToReadOnly(), loggerGroup, method.GetMethodFullName(), name, fullName, hasAsync, hasReturn, hasIResult, hasIResultGeneric, returnType, cfg.ResultTypeDefinition, resultType, GetDefaultValue(args), attributes2, methodMeta.Key, cfg.Info.GetCommandGroup(cfg.Info.CommandGroupDefault, name), useTypePosition, GetMethodTypeFullName(fullName, args), doc);

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

        static ReadOnlyCollection<Args> GetArgChild(System.Type type, string declaring, string businessName, string method, string path, System.Collections.Generic.IDictionary<string, CommandAttribute> commands, ref System.Collections.Generic.List<string> definitions, System.Type resultType, System.Type resultTypeDefinition, ConcurrentReadOnlyDictionary<string, System.Type> useTypes, out bool hasLower, string root, ReadOnlyCollection<Args> childrens)
        {
            hasLower = false;

            var args = new ReadOnlyCollection<Args>();

            var position = 0;

            var members = type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.GetField | BindingFlags.GetProperty);

            var definitions2 = new System.Collections.Generic.List<string>(definitions);

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

                var argAttrAll = AttributeBase.GetAttributes(item, current.outType);

                //var hasDefinition = current.outType.IsDefinition();

                if (definitions.Contains(current.outType.FullName)) { continue; }
                else if (current.hasDefinition) { definitions2.Add(current.outType.FullName); }

                var path2 = $"{path}.{item.Name}";

                //var use = argAttrAll.GetAttr<UseAttribute>();
                var use = current.hasIArg ? current.inType.GetAttribute<UseAttribute>() ?? argAttrAll.GetAttr<UseAttribute>(c => c.ParameterName) : argAttrAll.GetAttr<UseAttribute>();
                var hasUse = null != use || (current.hasIArg ? useTypes.ContainsKey(current.inType.FullName) : false);

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

                var argGroup = GetArgGroup(argAttrAll, current, declaring, businessName, method, path2, path, item.Name, commands, resultType, resultTypeDefinition, hasUse, out bool hasLower2, out bool hasCollectionAttr2, root);

                var hasLower3 = false;
                var childrens2 = current.hasDefinition ? new ReadOnlyCollection<Args>() : ReadOnlyCollection<Args>.Empty;
                var children = current.hasDefinition ? GetArgChild(current.outType, declaring, businessName, method, path2, commands, ref definitions2, resultType, resultTypeDefinition, useTypes, out hasLower3, root, childrens2) : ReadOnlyCollection<Args>.Empty;

                if (hasLower2 || hasLower3)
                {
                    hasLower = true;
                }

                var arg = new Args(item.Name,
                    memberType,
                    memberType,
                    current.outType,
                    position++,
                    default,
                    default,
                    current.hasDictionary,
                    current.hasCollection,
                    hasCollectionAttr || hasCollectionAttr2,
                    current.hasCollection ? typeof(IArg).GetTypeInfo().IsAssignableFrom(current.outType, out _) : false,
                    current.nullable,
                    accessor,
                    argGroup,
                    children,
                    childrens2,
                    hasLower2 || hasLower3,
                    current.hasDefinition,
                    current.hasIArg,
                    current.hasIArg ? current.outType : default,
                    current.hasIArg ? current.inType : default,
                    //path2,
                    use,
                    hasUse,
                    typeof(Auth.IToken).IsAssignableFrom(current.hasIArg ? current.inType : current.outType) || true == use?.Token,
                    GetMethodTypeFullName(memberType),
                    $"{type.GetTypeName(item.DeclaringType)}.{item.Name}",
                    memberDefinition,
                    false);

                args.Collection.Add(arg);
                childrens.Collection.Add(arg);

                foreach (var child in childrens2)
                {
                    childrens.Collection.Add(child);
                }
            }

            return args;
        }

        //static readonly System.Type[] procesTypes = new System.Type[] { typeof(object) };
        //static readonly System.Type[] procesIArgCollectionTypes = new System.Type[] { typeof(object), typeof(IArg), typeof(int), typeof(object) };

        static ConcurrentReadOnlyDictionary<string, ArgGroup> GetArgGroup(System.Collections.Generic.List<AttributeBase> argAttrAll, Help.CurrentType current, string declaring, string businessName, string method, string path, string owner, string member, System.Collections.Generic.IDictionary<string, CommandAttribute> commands, System.Type resultType, System.Type resultTypeDefinition, bool hasUse, out bool hasLower, out bool hasCollectionAttr, string root, System.Collections.Generic.List<LoggerAttribute> log = null, System.Collections.Generic.List<LoggerAttribute> inLog = null)
        {
            hasLower = false;
            hasCollectionAttr = false;

            //var argAttrs = GetArgAttr(argAttrAll, current.orgType, resultType, resultTypeDefinition);

            var argAttrs = argAttrAll.Where(c => c is ArgumentAttribute).Cast<ArgumentAttribute>().ToList();
            argAttrs.Sort(ComparisonHelper<ArgumentAttribute>.CreateComparer(c => System.Math.Abs(c.State.ConvertErrorState())));

            var aliass = argAttrAll.GetAttrs<AliasAttribute>().Where(c => !string.IsNullOrWhiteSpace(c.Name));

            var argGroup = new ConcurrentReadOnlyDictionary<string, ArgGroup>();

            foreach (var item in commands)
            {
                var ignores = argAttrAll.GetAttrs<Ignore>(c => Help.GroupEquals(c, item.Value.Group), clone: true);

                var ignoreBusinessArg = ignores.Any(c => c.Mode == IgnoreMode.BusinessArg);
                // || (item.Group == c.Group || string.IsNullOrWhiteSpace(c.Group))

                //var argAttrChild = (hasUse || item.Value.IgnoreBusinessArg || ignoreBusinessArg) ?
                //var argAttrChild = (hasUse || ignoreBusinessArg) ?
                var argAttrChild = ignoreBusinessArg ?
                    argAttrs.FindAll(c => Help.GroupEquals(c, item.Value.Group) && c.Meta.Declaring == AttributeBase.MetaData.DeclaringType.Parameter).Select(c => c.Clone()).ToList() :
                    argAttrs.FindAll(c => Help.GroupEquals(c, item.Value.Group)).Select(c => c.Clone()).ToList();

                //var nickValue = string.IsNullOrWhiteSpace(nick?.Name) ? argAttrChild.Where(c => !string.IsNullOrWhiteSpace(c.Nick) && Help.GroupEquals(c, item.Value.Group)).GroupBy(c => c.Nick, System.StringComparer.InvariantCultureIgnoreCase).FirstOrDefault()?.Key : nick.Name;
                var alias = aliass.FirstOrDefault(c => Help.GroupEquals(c, item.Value.Group));
                var aliasValue = string.IsNullOrWhiteSpace(alias?.Name) ? argAttrChild.Where(c => !string.IsNullOrWhiteSpace(c.Alias) && Help.GroupEquals(c, item.Value.Group)).FirstOrDefault()?.Alias : alias.Name;

                //var argAttr = new ConcurrentLinkedList<ArgumentAttribute>();//argAttrChild.Count
                var argAttr = new ReadOnlyCollection<ArgumentAttribute>();

                var httpFile = false;

                foreach (var c in argAttrChild)
                {
                    if (ArgumentAttribute.GetFilter(c.ArgMeta.Filter, hasUse, current.hasDefinition, c.Meta.Declaring))
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
                        attr.ArgMeta.Proces.Call = dynamicMethodBuilder.GetDelegate(attr.ArgMeta.Proces.MethodInfo.MakeGenericMethod(current.orgType));
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
                    attr.ArgMeta.MethodOnlyName = item.Value.OnlyName;
                    attr.ArgMeta.MemberPath = path;
                    attr.ArgMeta.Member = member;
                    attr.ArgMeta.MemberType = current.orgType;
                    //attr.ArgumentMeta.Method = item.Value.OnlyName;
                    //attr.ArgumentMeta.Member = path2;
                    ////attr.Meta.Business = c.Meta.Business;
                    //attr.ArgumentMeta.resultType = c.ArgumentMeta.resultType;
                    //attr.ArgumentMeta.resultTypeDefinition = c.ArgumentMeta.resultTypeDefinition;
                    //attr.ArgumentMeta.MemberType = c.ArgumentMeta.MemberType;
                    //attr.ArgumentMeta.HasProcesIArg = c.ArgumentMeta.HasProcesIArg;

                    if (string.IsNullOrWhiteSpace(attr.Alias))
                    {
                        attr.Alias = !string.IsNullOrWhiteSpace(aliasValue) ? aliasValue : attr.ArgMeta.Member;
                    }

                    attr.BindAfter?.Invoke();

                    //if (!argAttr.TryAdd(attr))
                    //{
                    //    System.Console.WriteLine("ConcurrentLinkedList TryAdd error! State = INV");
                    //}
                    argAttr.Collection.Add(attr);

                    if (!hasLower) { hasLower = true; }

                    if (c.CollectionItem && !hasCollectionAttr) { hasCollectionAttr = true; }//checked hasCollectionAttr 2

                    if (typeof(HttpFileAttribute).IsAssignableFrom(c.Meta.Type) && !httpFile)
                    {
                        httpFile = true;
                    }
                }

                //add default convert
                //if (current.hasIArg && 0 == argAttr.Count)
                //if (default == owner && current.hasIArg && null == argAttr.First.Value)
                if (default == owner && current.hasIArg && 0 == argAttr.Count)
                {
                    //if (default == owner) ?? Is the first level added or every level added?
                    var def = new ArgumentDefaultAttribute(resultType, resultTypeDefinition);
                    def.Meta.Declaring = AttributeBase.MetaData.DeclaringType.Parameter;
                    //argAttr.TryAdd(def);
                    argAttr.Collection.Add(def);

                    if (!hasLower) { hasLower = true; }
                }

                //owner ?? Do need only the superior, not the superior path? For doc
                var group = new ArgGroup(ignores.ToReadOnly(), ignores.Any(c => c.Mode == IgnoreMode.Arg), argAttr, aliasValue, $"{item.Value.OnlyName}.{path}", default == owner ? item.Value.OnlyName : $"{item.Value.OnlyName}.{owner}", $"{item.Value.OnlyName}.{root}", httpFile);

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

        public virtual Result Call<Result>(string cmd, params UseEntry[] useObj) => Call(cmd, null, useObj);

        public virtual IResult CallIResult(string cmd, params UseEntry[] useObj) => Call(cmd, null, useObj);

        public virtual dynamic Call(string cmd, params UseEntry[] useObj) => Call(cmd, null, useObj);

        public virtual Result Call<Result>(string cmd, object[] parameters = null, UseEntry[] useObj = null, string group = null) => Call(cmd, parameters, useObj, group);

        public virtual IResult CallIResult(string cmd, object[] parameters = null, UseEntry[] useObj = null, string group = null) => Call(cmd, parameters, useObj, group);

        public virtual dynamic Call(string cmd, object[] parameters = null, UseEntry[] useObj = null, string group = null)
        {
            var command = GetCommand(cmd, group);

            return null == command ? Help.ErrorCmd(resultTypeDefinition, cmd) : command.Call(parameters, useObj);
        }

        public virtual Result Call<Result>(string cmd, object[] parameters = null, string group = null, params UseEntry[] useObj) => Call(cmd, parameters, useObj, group);

        public virtual IResult CallIResult(string cmd, object[] parameters = null, string group = null, params UseEntry[] useObj) => Call(cmd, parameters, useObj, group);

        public virtual dynamic Call(string cmd, object[] parameters = null, string group = null, params UseEntry[] useObj) => Call(cmd, parameters, useObj, group);

        #endregion

        #region Call

        public virtual Result Call<Result>(string cmd, System.Collections.Generic.IDictionary<string, string> parameters = null, UseEntry[] useObj = null, string group = null) => Call(cmd, parameters, useObj, group);

        public virtual IResult CallIResult(string cmd, System.Collections.Generic.IDictionary<string, string> parameters = null, UseEntry[] useObj = null, string group = null) => Call(cmd, parameters, useObj, group);

        public virtual dynamic Call(string cmd, System.Collections.Generic.IDictionary<string, string> args, UseEntry[] useObj, string group)
        {
            var command = GetCommand(cmd, group);

            return null == command ? Help.ErrorCmd(resultTypeDefinition, cmd) : command.Call(args, useObj);
        }

        public virtual Result Call<Result>(string cmd, System.Collections.Generic.IDictionary<string, string> parameters = null, string group = null, params UseEntry[] useObj) => Call(cmd, parameters, useObj, group);

        public virtual IResult CallIResult(string cmd, System.Collections.Generic.IDictionary<string, string> parameters = null, string group = null, params UseEntry[] useObj) => Call(cmd, parameters, useObj, group);

        public virtual dynamic Call(string cmd, System.Collections.Generic.IDictionary<string, string> parameters = null, string group = null, params UseEntry[] useObj) => Call(cmd, parameters, useObj, group);

        #endregion

        #region AsyncCallUse

        public virtual async System.Threading.Tasks.Task<Result> AsyncCall<Result>(string cmd, params UseEntry[] useObj) => await AsyncCall(cmd, null, useObj);

        public virtual async System.Threading.Tasks.Task<IResult> AsyncIResult(string cmd, params UseEntry[] useObj) => await AsyncCall(cmd, null, useObj);

        public virtual async System.Threading.Tasks.Task<dynamic> AsyncCall(string cmd, params UseEntry[] useObj) => await AsyncCall(cmd, null, useObj);

        public virtual async System.Threading.Tasks.Task<Result> AsyncCall<Result>(string cmd, object[] parameters = null, UseEntry[] useObj = null, string group = null) => await AsyncCall(cmd, parameters, useObj, group);

        public virtual async System.Threading.Tasks.Task<IResult> AsyncIResult(string cmd, object[] parameters = null, UseEntry[] useObj = null, string group = null) => await AsyncCall(cmd, parameters, useObj, group);

        public virtual async System.Threading.Tasks.Task<dynamic> AsyncCall(string cmd, object[] parameters = null, UseEntry[] useObj = null, string group = null)
        {
            var command = GetCommand(cmd, group);

            return null == command ? await System.Threading.Tasks.Task.FromResult(Help.ErrorCmd(resultTypeDefinition, cmd)) : await command.AsyncCall(parameters, useObj);
        }

        public virtual async System.Threading.Tasks.Task<IResult> AsyncIResult(string cmd, object[] parameters = null, string group = null, params UseEntry[] useObj) => await AsyncCall(cmd, parameters, useObj, group);

        public virtual async System.Threading.Tasks.Task<Result> AsyncCall<Result>(string cmd, object[] parameters = null, string group = null, params UseEntry[] useObj) => await AsyncCall(cmd, parameters, useObj, group);

        public virtual async System.Threading.Tasks.Task<dynamic> AsyncCall(string cmd, object[] parameters = null, string group = null, params UseEntry[] useObj) => await AsyncCall(cmd, parameters, useObj, group);

        #endregion

        #region AsyncCallUse

        public virtual async System.Threading.Tasks.Task<Result> AsyncCall<Result>(string cmd, System.Collections.Generic.IDictionary<string, string> parameters = null, UseEntry[] useObj = null, string group = null) => await AsyncCall(cmd, parameters, useObj, group);

        public virtual async System.Threading.Tasks.Task<IResult> AsyncIResult(string cmd, System.Collections.Generic.IDictionary<string, string> parameters = null, UseEntry[] useObj = null, string group = null) => await AsyncCall(cmd, parameters, useObj, group);

        public virtual async System.Threading.Tasks.Task<dynamic> AsyncCall(string cmd, System.Collections.Generic.IDictionary<string, string> parameters, UseEntry[] useObj, string group)
        {
            var command = GetCommand(cmd, group);

            return null == command ? await System.Threading.Tasks.Task.FromResult(Help.ErrorCmd(resultTypeDefinition, cmd)) : await command.AsyncCall(parameters, useObj);
        }

        public virtual async System.Threading.Tasks.Task<IResult> AsyncIResult(string cmd, System.Collections.Generic.IDictionary<string, string> parameters = null, string group = null, params UseEntry[] useObj) => await AsyncCall(cmd, parameters, useObj, group);

        public virtual async System.Threading.Tasks.Task<Result> AsyncCall<Result>(string cmd, System.Collections.Generic.IDictionary<string, string> parameters = null, string group = null, params UseEntry[] useObj) => await AsyncCall(cmd, parameters, useObj, group);

        public virtual async System.Threading.Tasks.Task<dynamic> AsyncCall(string cmd, System.Collections.Generic.IDictionary<string, string> parameters = null, string group = null, params UseEntry[] useObj) => await AsyncCall(cmd, parameters, useObj, group);

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
            this.HasHttpFile = this.Meta.Args.Any(c => c.Group[Key].HttpFile) || this.Meta.Attributes.Any(c => typeof(HttpFileAttribute).IsAssignableFrom(c.Meta.Type));
        }

        //===============member==================//
        readonly System.Func<object[], dynamic> call;

        public virtual object[] GetArgsUse(UseEntry[] useObj, System.Action<object[], int, Args, ArgGroup> action)
        {
            var parameters = new object[Meta.Args.Count];

            if (0 < parameters.Length)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    var arg = Meta.Args[i];
                    arg.Group.TryGetValue(Key, out ArgGroup group);

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
                                foreach (var use in useObj)
                                {
                                    if (Meta.UseTypePosition[i].IsAssignableFrom(use.Type))
                                    {
                                        parameters[i] = use.Value;
                                        break;
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

        public virtual object[] GetAgs(object[] parameters, params UseEntry[] useObj)
        {
            int l = 0;
            return GetArgsUse(useObj, (args2, i, arg, group) =>
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

        public virtual object[] GetAgs(System.Collections.Generic.IDictionary<string, string> parameters, params UseEntry[] useObj)
        {
            return GetArgsUse(useObj, (args2, i, arg, group) =>
            {
                if (null != parameters && 0 < parameters.Count)
                {
                    if (parameters.TryGetValue(arg.Name, out string value))
                    {
                        args2[i] = value;
                    }
                }
            });
        }

        #region Call

        public dynamic Call(System.Collections.Generic.IDictionary<string, string> parameters, params UseEntry[] useObj) => Syn(GetAgs(parameters, useObj));

        public Result Call<Result>(System.Collections.Generic.IDictionary<string, string> parameters, params UseEntry[] useObj) => Call(parameters, useObj);

        public IResult CallIResult(System.Collections.Generic.IDictionary<string, string> parameters, params UseEntry[] useObj) => Call(parameters, useObj);

        dynamic Syn(object[] parameters)
        {
            try
            {
                return call(parameters);
            }
            catch (System.Exception ex)
            {
                return ResultFactory.ResultCreate(Meta, 0, System.Convert.ToString(Help.ExceptionWrite(ex)));
            }
        }

        public dynamic Call(object[] parameters, params UseEntry[] useObj) => Syn(GetAgs(parameters, useObj));

        public Result Call<Result>(object[] parameters, params UseEntry[] useObj) => Call(parameters, useObj);

        public IResult CallIResult(object[] parameters, params UseEntry[] useObj) => Call(parameters, useObj);

        #endregion

        #region AsyncCall

        public async System.Threading.Tasks.Task<dynamic> AsyncCall(System.Collections.Generic.IDictionary<string, string> parameters, params UseEntry[] useObj) => await Async(GetAgs(parameters, useObj));

        public async System.Threading.Tasks.Task<Result> AsyncCall<Result>(System.Collections.Generic.IDictionary<string, string> parameters, params UseEntry[] useObj) => await AsyncCall(parameters, useObj);

        public async System.Threading.Tasks.Task<IResult> AsyncIResult(System.Collections.Generic.IDictionary<string, string> parameters, params UseEntry[] useObj) => await AsyncCall(parameters, useObj);

        async System.Threading.Tasks.Task<dynamic> Async(object[] parameters)
        {
            try
            {
                if (Meta.HasAsync)
                {
                    return await call(parameters);
                }
                else
                {
                    using (var task = System.Threading.Tasks.Task.Factory.StartNew(obj =>
                    {
                        var obj2 = (dynamic)obj;
                        return obj2.call(obj2.parameters);

                    }, new { call, parameters }))
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

        public async System.Threading.Tasks.Task<dynamic> AsyncCall(object[] parameters, params UseEntry[] useObj) => await Async(GetAgs(parameters, useObj));

        public async System.Threading.Tasks.Task<Result> AsyncCall<Result>(object[] parameters, params UseEntry[] useObj) => await AsyncCall(parameters, useObj);

        public async System.Threading.Tasks.Task<IResult> AsyncIResult(object[] parameters, params UseEntry[] useObj) => await AsyncCall(parameters, useObj);

        #endregion

        public readonly string Key;

        public MetaData Meta { get; private set; }

        public bool HasArgSingle { get; internal set; }

        public bool HasHttpFile { get; internal set; }
    }
}

namespace Business.Core.Meta
{
    using Business.Core.Annotations;
    using Business.Core.Utils;

    #region Meta

    public class ArgGroup
    {
        public ArgGroup(string path) => Path = path;

        public ArgGroup(ReadOnlyCollection<Ignore> ignore, bool ignoreArg, ReadOnlyCollection<ArgumentAttribute> attrs, string alias, string path, string owner, string root, bool httpFile)
        {
            Ignore = ignore;
            IgnoreArg = ignoreArg;
            Attrs = attrs;
            Path = path;
            Alias = alias;
            Owner = owner;
            Root = root;
            Logger = default;
            IArgInLogger = default;
            HttpFile = httpFile;
        }

        public ReadOnlyCollection<Ignore> Ignore { get; private set; }

        public bool IgnoreArg { get; internal set; }

        //public ConcurrentLinkedList<ArgumentAttribute> Attrs { get; internal set; }
        public ReadOnlyCollection<ArgumentAttribute> Attrs { get; internal set; }

        public string Alias { get; private set; }

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
        public Args(string name, System.Type type, System.Type origType, System.Type lastType, int position, object defaultValue, bool hasDefaultValue, bool hasDictionary, bool hasCollection, bool hasCollectionAttr, bool hasCollectionIArg, bool nullable, Accessor accessor, ConcurrentReadOnlyDictionary<string, ArgGroup> group, ReadOnlyCollection<Args> children, ReadOnlyCollection<Args> childrens, bool hasLower, bool hasDefinition, bool hasIArg, System.Type iArgOutType, System.Type iArgInType, UseAttribute use, bool useType, bool hasToken, string methodTypeFullName, string fullName, MemberDefinitionCode memberDefinition, bool hasCast)
        {
            Name = name;
            Type = type;
            OrigType = origType;
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
            HasCast = hasCast;
        }

        //===============name==================//
        public string Name { get; private set; }
        //===============type==================//
        public System.Type Type { get; internal set; }
        //===============origType==================//
        public System.Type OrigType { get; private set; }
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
        public ConcurrentReadOnlyDictionary<string, ArgGroup> Group { get; internal set; }
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
        public bool HasIArg { get; internal set; }
        //public MetaLogger Logger { get; private set; }
        //public MetaLogger IArgInLogger { get; private set; }
        //==============group===================//
        //public string Group2 { get; private set; }
        //==============owner===================//
        //public string Owner { get; private set; }
        ////==============ignore===================//
        //public ReadOnlyCollection<Attributes.Ignore> Ignore { get; private set; }
        //==============use===================//
        public UseAttribute Use { get; internal set; }
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
        //==============hasCast===================//
        public bool HasCast { get; internal set; }
    }

    public struct MetaLogger
    {
        public LoggerAttribute Record { get; set; }
        public LoggerAttribute Error { get; set; }
        public LoggerAttribute Exception { get; set; }
    }

    public struct CommandGroup
    {
        public CommandGroup(ReadOnlyDictionary<string, CommandAttribute> group, ReadOnlyDictionary<string, ReadOnlyDictionary<string, CommandAttribute>> full) { Group = group; Full = full; }

        public ReadOnlyDictionary<string, CommandAttribute> Group { get; }

        public ReadOnlyDictionary<string, ReadOnlyDictionary<string, CommandAttribute>> Full { get; }
    }

    public struct MetaData
    {
        public override string ToString() => Name;

        //MetaData
        public MetaData(System.Func<object, object[], object> accessor, CommandGroup commandGroup, ReadOnlyCollection<Args> args, ReadOnlyCollection<Args> argAll, ReadOnlyCollection<Args> iArgs, ReadOnlyDictionary<string, MetaLogger> metaLogger, string path, string name, string fullName, bool hasAsync, bool hasReturn, bool hasIResult, bool hasIResultGeneric, System.Type returnType, System.Type resultTypeDefinition, System.Type resultType, object[] defaultValue, System.Collections.Generic.List<AttributeBase> attributes, int position, string groupDefault, ConcurrentReadOnlyDictionary<int, System.Type> useTypePosition, string methodTypeFullName, DocAttribute doc)
        {
            Accessor = accessor;
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
            this.Doc = doc;
        }

        public System.Func<object, object[], object> Accessor { get; private set; }

        //==============commandAttr===================//
        public CommandGroup CommandGroup { get; private set; }
        //==============argAttrs===================//
        public ReadOnlyCollection<Args> Args { get; private set; }
        //==============argAll===================//
        public ReadOnlyCollection<Args> ArgAll { get; private set; }
        //==============iArgs===================//
        public ReadOnlyCollection<Args> IArgs { get; private set; }
        //==============MetaLogger===================//
        public ReadOnlyDictionary<string, MetaLogger> MetaLogger { get; private set; }
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
        public System.Collections.Generic.List<AttributeBase> Attributes { get; private set; }
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
        //==============doc===================//
        public DocAttribute Doc { get; private set; }

    }

    #endregion
}