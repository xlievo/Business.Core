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
    using Document;
    using System.Collections.Generic;
    using System.Linq;
    using Utils;

    /// <summary>
    /// IBootstrap
    /// </summary>
    public interface IBootstrap
    {
        /// <summary>
        /// Config
        /// </summary>
        BootstrapConfig Config { get; }

        /// <summary>
        /// UseDoc
        /// </summary>
        IBootstrap UseDoc(string outDir = null, System.Action<Options> options = default);

        /// <summary>
        /// UseResultType
        /// </summary>
        IBootstrap UseResultType(System.Type resultType);

        ///// <summary>
        ///// UseArgType
        ///// </summary>
        //IBootstrap UseArgType(System.Type argType);

        /// <summary>
        /// Log callback for all business classes
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        IBootstrap UseLogger(Logger logger);

        /// <summary>
        /// UseInjection
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        IBootstrap UseInjection(System.Func<string, System.Type, object> member);

        /// <summary>
        /// Build
        /// </summary>
        dynamic Build();
    }

    /// <summary>
    /// BootstrapConfig
    /// </summary>
    public class BootstrapConfig
    {
        /// <summary>
        /// BootstrapConfig
        /// </summary>
        internal protected BootstrapConfig()
        {
            Use = new System.Collections.Concurrent.ConcurrentQueue<System.Func<IBusiness, IBusiness>>();
            //UseDoc = null;
        }

        /// <summary>
        /// BootstrapConfig
        /// </summary>
        /// <param name="interceptor"></param>
        /// <param name="type"></param>
        public BootstrapConfig(System.Type interceptor, System.Type type = null) : this()
        {
            Interceptor = interceptor ?? typeof(Auth.InterceptorCastle);
            Type = type;
        }

        /// <summary>
        /// Use
        /// </summary>
        public System.Collections.Concurrent.ConcurrentQueue<System.Func<IBusiness, IBusiness>> Use { get; }

        /// <summary>
        /// UseDoc
        /// </summary>
        public UseDocConfig UseDoc { get; set; }

        /// <summary>
        /// Interceptor
        /// </summary>
        public System.Type Interceptor { get; }

        ///// <summary>
        ///// ConstructorArgument
        ///// </summary>
        //public object[] ConstructorArgument { get; }

        /// <summary>
        /// InjectionName
        /// </summary>
        public System.Func<string, System.Type, object> Injection { get; set; }

        /// <summary>
        /// Type
        /// </summary>
        public System.Type Type { get; }

        /// <summary>
        /// ResultType
        /// </summary>
        public System.Type ResultType { get; set; }

        /// <summary>
        /// BuildBefore
        /// </summary>
        public System.Action<IBootstrap> BuildBefore { get; set; }

        /// <summary>
        /// BuildAfter
        /// </summary>
        public System.Action<IBootstrap> BuildAfter { get; set; }

        /// <summary>
        /// Log callback for all business classes
        /// </summary>
        public Logger Logger { get; set; }

        /// <summary>
        /// Attributes
        /// </summary>
        public IEnumerable<Annotations.GroupAttribute> Attributes { get; set; }

        internal readonly IList<System.Type> useTypes = new List<System.Type>();

        /// <summary>
        /// UseDocConfig
        /// </summary>
        public class UseDocConfig
        {

            /// <summary>
            /// OutDir
            /// </summary>
            public string OutDir { get; set; }

            /// <summary>
            /// Options
            /// </summary>
            public Options Options { get; set; }

            /// <summary>
            /// Use
            /// </summary>
            public System.Action<string, Options> Use { get; internal set; }
        }
    }

    /// <summary>
    /// Bootstrap
    /// </summary>
    public class Bootstrap : IBootstrap
    {
        /// <summary>
        /// Bootstrap
        /// </summary>
        /// <param name="config"></param>
        public Bootstrap(BootstrapConfig config = default) => Config = config ?? new BootstrapConfig();

        #region IBootstrap

        /// <summary>
        /// Config
        /// </summary>
        public BootstrapConfig Config { get; }

        IBootstrap IBootstrap.UseDoc(string outDir, System.Action<Options> options) => UseDoc(outDir, options);

        IBootstrap IBootstrap.UseResultType(System.Type resultType) => UseResultType(resultType);

        //IBootstrap IBootstrap.UseArgType(System.Type argType) => UseArgType(argType);

        IBootstrap IBootstrap.UseLogger(Logger logger) => UseLogger(logger);

        IBootstrap IBootstrap.UseInjection(System.Func<string, System.Type, object> member) => UseInjection(member);

        #endregion

        #region Create

        ///// <summary>
        ///// Initialize a Generic proxy class
        ///// </summary>
        ///// <typeparam name="Business"></typeparam>
        ///// <param name="constructorArguments"></param>
        ///// <returns></returns>
        //public static Bootstrap<Business> Create<Business>(params object[] constructorArguments) where Business : class => Create<Business>(constructorArguments, null);

        /// <summary>
        /// Initialize a Generic proxy class
        /// </summary>
        /// <typeparam name="Business"></typeparam>
        /// <param name="interceptor"></param>
        /// <returns></returns>
        public static Bootstrap<Business> Create<Business>(System.Type interceptor = null) where Business : class => new Bootstrap<Business>(new BootstrapConfig(interceptor, typeof(Business)));

        ///// <summary>
        ///// Initialize a Type proxy class
        ///// </summary>
        ///// <param name="type"></param>
        ///// <param name="constructorArguments"></param>
        ///// <returns></returns>
        //public static Bootstrap Create(System.Type type, params object[] constructorArguments) => Create(type, null, constructorArguments);

        ///// <summary>
        ///// Initialize a Type proxy class
        ///// </summary>
        ///// <param name="type"></param>
        ///// <param name="constructorArguments"></param>
        ///// <returns></returns>
        //public static Bootstrap Create(System.Type type, params object[] constructorArguments) => Create(type, constructorArguments, null);

        /// <summary>
        /// Initialize a Type proxy class
        /// </summary>
        /// <param name="type"></param>
        /// <param name="interceptor"></param>
        /// <returns></returns>
        public static Bootstrap Create(System.Type type, System.Type interceptor = null) => new Bootstrap(new BootstrapConfig(interceptor, type));

        ///// <summary>
        ///// bootstrap all Business class
        ///// </summary>
        ///// <param name="constructorArguments"></param>
        ///// <returns></returns>
        //public static BootstrapAll CreateAll(params object[] constructorArguments) => CreateAll(constructorArguments, null);

        /// <summary>
        /// bootstrap all Business class
        /// </summary>
        /// <param name="interceptor"></param>
        /// <returns></returns>
        public static BootstrapAll CreateAll(System.Type interceptor = null) => new BootstrapAll(new BootstrapConfig(interceptor));

        ///// <summary>
        ///// bootstrap all Business class
        ///// </summary>
        ///// <typeparam name="Business"></typeparam>
        ///// <param name="constructorArgument"></param>
        ///// <returns></returns>
        //public static BootstrapAll<Business> CreateAll<Business>(params object[] constructorArgument) where Business : class, IBusiness => CreateAll<Business>(constructorArgument, null);

        /// <summary>
        /// bootstrap all Business class
        /// </summary>
        /// <typeparam name="Business"></typeparam>
        /// <param name="interceptor"></param>
        /// <returns></returns>
        public static BootstrapAll<Business> CreateAll<Business>(System.Type interceptor = null) where Business : class, IBusiness => new BootstrapAll<Business>(new BootstrapConfig(interceptor));

        //public static BootstrapAll<Business> CreateAll<Business>(object[] constructorArgument, System.Func<string, System.Type, object> constructorArgumentFunc, System.Type interceptor = null) where Business : class, IBusiness => new BootstrapAll<Business>(new BootstrapConfig(interceptor, constructorArgument, constructorArgumentFunc));

        #endregion

        IBusiness business;

        /// <summary>
        /// Build
        /// </summary>
        /// <returns></returns>
        public virtual object Build()
        {
            Config.BuildBefore?.Invoke(this);

            var bind = new Bind(Config.Type, Config.Interceptor, Config.Injection, Config.ResultType, Config.useTypes, Config.Logger, Config.Attributes);

            business = bind.hasBusiness ? (IBusiness)bind.instance : null;

            if (null != business)
            {
                foreach (var item in Config.Use)
                {
                    item.Invoke(business);
                }

                Config.UseDoc?.Use?.Invoke(Config.UseDoc.OutDir, Config.UseDoc.Options);
            }

            Config.BuildAfter?.Invoke(this);

            return bind.instance;
        }

        /// <summary>
        /// Generating Document Model for All Business Classes. business.doc
        /// </summary>
        /// <param name="outDir"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public virtual Bootstrap UseDoc(string outDir = null, System.Action<Options> options = default)
        {
            var _options = null == options ? null : new Options();
            options?.Invoke(_options);

            this.Config.UseDoc = new BootstrapConfig.UseDocConfig
            {
                OutDir = outDir,
                Options = _options,
                //Use = (dir, cfg) => business?.UseDoc(dir, cfg)
                Use = (dir, opt) => { if (null != business) { Help.UseDoc(business, dir, opt); } }
            };
            return this;
        }

        /// <summary>
        /// Generating Document Model for All Business Classes. business.doc
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public virtual Bootstrap UseDoc(System.Action<Options> options) => UseDoc(null, options);

        #region UseResultType UseArgType

        /// <summary>
        /// use result type
        /// </summary>
        /// <param name="resultType"></param>
        /// <returns></returns>
        public virtual Bootstrap UseResultType(System.Type resultType)
        {
            if (!typeof(Result.IResult).IsAssignableFrom(resultType))
            {
                throw new System.ArgumentException(nameof(resultType), "Business.Core.Result.IResult interface not implemented");
            }

            Config.ResultType = resultType;
            return this;
        }

        ///// <summary>
        ///// use arg type
        ///// </summary>
        ///// <param name="argType"></param>
        ///// <returns></returns>
        //public virtual Bootstrap UseArgType(System.Type argType)
        //{
        //    if (!typeof(IArg).IsAssignableFrom(argType))
        //    {
        //        throw new System.ArgumentException(nameof(argType), "Business.Core.IArg interface not implemented");
        //    }

        //    Config.ArgType = argType;
        //    return this;
        //}

        /// <summary>
        /// Log callback for all business classes
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        public virtual Bootstrap UseLogger(Logger logger)
        {
            Config.Logger = logger;
            return this;
        }

        /// <summary>
        /// UseInjection
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public virtual Bootstrap UseInjection(System.Func<string, System.Type, object> member)
        {
            Config.Injection = member;
            return this;
        }

        #endregion
    }

    /// <summary>
    /// Bootstrap
    /// </summary>
    /// <typeparam name="Business"></typeparam>
    public class Bootstrap<Business> : IBootstrap
        where Business : class
    {
        /// <summary>
        /// Bootstrap
        /// </summary>
        /// <param name="config"></param>
        public Bootstrap(BootstrapConfig config = default) => Config = config ?? new BootstrapConfig();

        #region IBootstrap

        /// <summary>
        /// Config
        /// </summary>
        public BootstrapConfig Config { get; }

        IBootstrap IBootstrap.UseDoc(string outDir, System.Action<Options> options) => UseDoc(outDir, options);

        IBootstrap IBootstrap.UseResultType(System.Type resultType) => UseResultType(resultType);

        //IBootstrap IBootstrap.UseArgType(System.Type argType) => UseArgType(argType);

        IBootstrap IBootstrap.UseLogger(Logger logger) => UseLogger(logger);

        IBootstrap IBootstrap.UseInjection(System.Func<string, System.Type, object> member) => UseInjection(member);

        dynamic IBootstrap.Build() => Build();

        #endregion

        IBusiness business;

        /// <summary>
        /// Build
        /// </summary>
        /// <returns></returns>
        public virtual Business Build()
        {
            Config.BuildBefore?.Invoke(this);

            var bind = new Bind(Config.Type, Config.Interceptor, Config.Injection, Config.ResultType, Config.useTypes, Config.Logger, Config.Attributes);

            business = bind.hasBusiness ? (IBusiness)bind.instance : null;

            if (null != business)
            {
                foreach (var item in Config.Use)
                {
                    item.Invoke(business);
                }

                Config.UseDoc?.Use?.Invoke(Config.UseDoc.OutDir, Config.UseDoc.Options);
            }

            Config.BuildAfter?.Invoke(this);

            return bind.instance as Business;
        }

        /// <summary>
        /// Generating Document Model for All Business Classes. business.doc
        /// </summary>
        /// <param name="outDir"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public virtual Bootstrap<Business> UseDoc(string outDir = null, System.Action<Options> options = default)
        {
            var _options = null == options ? null : new Options();
            options?.Invoke(_options);

            this.Config.UseDoc = new BootstrapConfig.UseDocConfig
            {
                OutDir = outDir,
                Options = _options,
                //Use = (dir, cfg) => business?.UseDoc(dir, cfg)
                Use = (dir, opt) => { if (null != business) { Help.UseDoc(business, dir, opt); } }
            };
            return this;
        }

        /// <summary>
        /// UseDoc
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public virtual Bootstrap<Business> UseDoc(System.Action<Options> options) => UseDoc(null, options);

        #region UseResultType UseArgType

        /// <summary>
        /// use result type
        /// </summary>
        /// <param name="resultType"></param>
        /// <returns></returns>
        public virtual Bootstrap<Business> UseResultType(System.Type resultType)
        {
            if (!typeof(Result.IResult).IsAssignableFrom(resultType))
            {
                throw new System.ArgumentException(nameof(resultType), "Business.Core.Result.IResult interface not implemented");
            }

            Config.ResultType = resultType;
            return this;
        }

        ///// <summary>
        ///// use arg type
        ///// </summary>
        ///// <param name="argType"></param>
        ///// <returns></returns>
        //public virtual Bootstrap<Business> UseArgType(System.Type argType)
        //{
        //    if (!typeof(IArg).IsAssignableFrom(argType))
        //    {
        //        throw new System.ArgumentException(nameof(argType), "Business.Core.IArg interface not implemented");
        //    }

        //    Config.ArgType = argType;
        //    return this;
        //}

        /// <summary>
        /// Log callback for all business classes
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        public virtual Bootstrap<Business> UseLogger(Logger logger)
        {
            Config.Logger = logger;
            return this;
        }

        /// <summary>
        /// UseInjection
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public virtual Bootstrap<Business> UseInjection(System.Func<string, System.Type, object> member)
        {
            Config.Injection = member;
            return this;
        }

        #endregion
    }

    /// <summary>
    /// BootstrapAll
    /// </summary>
    public class BootstrapAll : IBootstrap
    {
        /// <summary>
        /// BootstrapAll
        /// </summary>
        /// <param name="config"></param>
        public BootstrapAll(BootstrapConfig config = default) => Config = config ?? new BootstrapConfig();

        #region IBootstrap

        /// <summary>
        /// Config
        /// </summary>
        public BootstrapConfig Config { get; }

        IBootstrap IBootstrap.UseDoc(string outDir, System.Action<Options> options) => UseDoc(outDir, options);

        IBootstrap IBootstrap.UseResultType(System.Type resultType) => UseResultType(resultType);

        //IBootstrap IBootstrap.UseArgType(System.Type argType) => UseArgType(argType);

        IBootstrap IBootstrap.UseLogger(Logger logger) => UseLogger(logger);

        IBootstrap IBootstrap.UseInjection(System.Func<string, System.Type, object> member) => UseInjection(member);

        dynamic IBootstrap.Build()
        {
            Build();
            return null;
        }

        #endregion

        /// <summary>
        /// Load all business classes in the run directory
        /// </summary>
        /// <param name="assemblyFiles"></param>
        /// <param name="businessTypeFullName"></param>
        public virtual void Build(string[] assemblyFiles = null, string[] businessTypeFullName = null)
        {
            Config.BuildBefore?.Invoke(this);

            //Get self reference assembly under single file publishing
            var aas = (null == assemblyFiles || !assemblyFiles.Any()) ? System.IO.Directory.GetFiles(Help.BaseDirectory, "*.dll").AsParallel().Select(c => Help.LoadAssembly(c)).AsEnumerable().Concat(System.AppDomain.CurrentDomain.GetAssemblies()).Distinct(Equality<System.Reflection.Assembly>.CreateComparer(c => c?.FullName, System.StringComparer.CurrentCultureIgnoreCase)) : assemblyFiles.AsParallel().Select(c => Help.LoadAssembly(c));

            _ = Help.LoadAssemblys(aas, type =>
            {
                if (typeof(IBusiness).IsAssignableFrom(type) && !type.IsAbstract)
                {
                    if (null != businessTypeFullName && businessTypeFullName.Any())
                    {
                        if (businessTypeFullName.Contains(type.FullName))
                        {
                            _ = new Bind(type, Config.Interceptor, Config.Injection, Config.ResultType, Config.useTypes, Config.Logger, Config.Attributes);
                            //Create(type, bootstrap.constructorArguments);
                            return true;
                        }
                    }
                    else
                    {
                        //Create(type, bootstrap.constructorArguments);
                        _ = new Bind(type, Config.Interceptor, Config.Injection, Config.ResultType, Config.useTypes, Config.Logger, Config.Attributes);
                        return true;
                    }
                }

                return false;
            });

            foreach (var business in Configer.BusinessList.Values.AsParallel())
            {
                foreach (var item in Config.Use)
                {
                    item.Invoke(business);
                }
            }

            Config.UseDoc.Use?.Invoke(Config.UseDoc.OutDir, Config.UseDoc.Options);

            Config.BuildAfter?.Invoke(this);
        }

        /// <summary>
        /// Generating Document Model for All Business Classes. business.doc
        /// </summary>
        /// <param name="outDir"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public virtual BootstrapAll UseDoc(string outDir = null, System.Action<Options> options = default)
        {
            var _options = null == options ? null : new Options();
            options?.Invoke(_options);

            this.Config.UseDoc = new BootstrapConfig.UseDocConfig
            {
                OutDir = outDir,
                Options = _options,
                Use = (dir, opt) =>
                {
                    var exists = !string.IsNullOrEmpty(dir) && System.IO.Directory.Exists(dir);
                    var doc = new Dictionary<string, IDoc>();

                    foreach (var item in Configer.BusinessList.OrderBy(c => c.Key))
                    {
                        //item.Value.UseDoc(null, cfg);
                        Help.UseDoc(item.Value, null, opt);

                        if (exists)
                        {
                            doc.Add(item.Value.Configer.Doc.Name, item.Value.Configer.Doc);
                        }
                    }

                    if (exists)
                    {
                        System.IO.File.WriteAllText(System.IO.Path.Combine(dir, Configer.documentFileName), doc.JsonSerialize(Configer.JsonOptionsDoc), Help.UTF8);
                        //System.IO.File.WriteAllText(System.IO.Path.Combine(outDir, "business.doc"), doc.JsonSerialize(DocJsonSettings2), Help.UTF8);
                    }
                }
            };

            return this;
        }

        /// <summary>
        /// UseDoc
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public virtual BootstrapAll UseDoc(System.Action<Options> options) => UseDoc(null, options);

        #region UseResultType UseArgType

        /// <summary>
        /// use result type
        /// </summary>
        /// <param name="resultType"></param>
        /// <returns></returns>
        public virtual BootstrapAll UseResultType(System.Type resultType)
        {
            if (!typeof(Result.IResult).IsAssignableFrom(resultType))
            {
                throw new System.ArgumentException(nameof(resultType), "Business.Core.Result.IResult interface not implemented");
            }

            Config.ResultType = resultType;
            return this;
        }

        ///// <summary>
        ///// use arg type
        ///// </summary>
        ///// <param name="argType"></param>
        ///// <returns></returns>
        //public virtual BootstrapAll UseArgType(System.Type argType)
        //{
        //    if (!typeof(IArg).IsAssignableFrom(argType))
        //    {
        //        throw new System.ArgumentException(nameof(argType), "Business.Core.IArg interface not implemented");
        //    }

        //    Config.ArgType = argType;
        //    return this;
        //}

        /// <summary>
        /// Log callback for all business classes
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        public virtual BootstrapAll UseLogger(Logger logger)
        {
            Config.Logger = logger;
            return this;
        }

        /// <summary>
        /// UseInjection
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public virtual BootstrapAll UseInjection(System.Func<string, System.Type, object> member)
        {
            Config.Injection = member;
            return this;
        }

        #endregion
    }

    /// <summary>
    /// BootstrapAll
    /// </summary>
    /// <typeparam name="Business"></typeparam>
    public class BootstrapAll<Business> : IBootstrap
        where Business : class, IBusiness
    {
        /// <summary>
        /// BusinessList
        /// </summary>
        public ConcurrentReadOnlyDictionary<string, Business> BusinessList = new ConcurrentReadOnlyDictionary<string, Business>(System.StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// BootstrapAll
        /// </summary>
        /// <param name="config"></param>
        public BootstrapAll(BootstrapConfig config = default) => Config = config ?? new BootstrapConfig();

        #region IBootstrap

        /// <summary>
        /// Config
        /// </summary>
        public BootstrapConfig Config { get; }

        IBootstrap IBootstrap.UseDoc(string outDir, System.Action<Options> options) => UseDoc(outDir, options);

        IBootstrap IBootstrap.UseResultType(System.Type resultType) => UseResultType(resultType);

        //IBootstrap IBootstrap.UseArgType(System.Type argType) => UseArgType(argType);

        IBootstrap IBootstrap.UseLogger(Logger logger) => UseLogger(logger);

        IBootstrap IBootstrap.UseInjection(System.Func<string, System.Type, object> member) => UseInjection(member);

        dynamic IBootstrap.Build()
        {
            Build();
            return null;
        }

        #endregion

        /// <summary>
        /// Load all business classes in the run directory
        /// </summary>
        /// <param name="assemblyFiles"></param>
        /// <param name="businessTypeFullName"></param>
        public virtual void Build(string[] assemblyFiles = null, string[] businessTypeFullName = null)
        {
            Config.BuildBefore?.Invoke(this);

            //Get self reference assembly under single file publishing
            var aas = (null == assemblyFiles || !assemblyFiles.Any()) ? System.IO.Directory.GetFiles(Help.BaseDirectory, "*.dll").AsParallel().Select(c => Help.LoadAssembly(c)).AsEnumerable().Concat(System.AppDomain.CurrentDomain.GetAssemblies()).Distinct(Equality<System.Reflection.Assembly>.CreateComparer(c => c?.FullName, System.StringComparer.CurrentCultureIgnoreCase)) : assemblyFiles.AsParallel().Select(c => Help.LoadAssembly(c));

            _ = Help.LoadAssemblys(aas, type =>
            {
                if (typeof(IBusiness).IsAssignableFrom(type) && !type.IsAbstract)
                {
                    if (null != businessTypeFullName && businessTypeFullName.Any())
                    {
                        if (businessTypeFullName.Contains(type.FullName))
                        {
                            if (new Bind(type, Config.Interceptor, Config.Injection, Config.ResultType, Config.useTypes, Config.Logger, Config.Attributes).instance is Business business)
                            {
                                BusinessList.dictionary.TryAdd(business.Configer.Info.BusinessName, business);
                            }
                            //Create(type, bootstrap.constructorArguments);
                            return true;
                        }
                    }
                    else
                    {
                        //Create(type, bootstrap.constructorArguments);
                        if (new Bind(type, Config.Interceptor, Config.Injection, Config.ResultType, Config.useTypes, Config.Logger, Config.Attributes).instance is Business business)
                        {
                            BusinessList.dictionary.TryAdd(business.Configer.Info.BusinessName, business);
                        }
                        return true;
                    }
                }

                return false;
            });

            foreach (var business in BusinessList.Values.AsParallel())
            {
                foreach (var item in Config.Use)
                {
                    item.Invoke(business);
                }
            }

            Config.UseDoc?.Use?.Invoke(Config.UseDoc.OutDir, Config.UseDoc.Options);

            Config.BuildAfter?.Invoke(this);
        }

        /// <summary>
        /// Generating Document Model for All Business Classes. business.doc
        /// </summary>
        /// <param name="outDir"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public virtual BootstrapAll<Business> UseDoc(string outDir = null, System.Action<Options> options = default)
        {
            var _options = null == options ? null : new Options();
            options?.Invoke(_options);

            this.Config.UseDoc = new BootstrapConfig.UseDocConfig
            {
                OutDir = outDir,
                Options = _options,
                Use = (dir, opt) =>
                {
                    var exists = !string.IsNullOrEmpty(dir) && System.IO.Directory.Exists(dir);
                    var doc = new Dictionary<string, IDoc>();

                    foreach (var item in BusinessList.OrderBy(c => c.Key))
                    {
                        //item.Value.UseDoc(null, cfg);
                        Help.UseDoc(item.Value, null, opt);

                        if (exists)
                        {
                            doc.Add(item.Value.Configer.Doc.Name, item.Value.Configer.Doc);
                        }
                    }

                    if (exists)
                    {
                        System.IO.File.WriteAllText(System.IO.Path.Combine(dir, Configer.documentFileName), doc.JsonSerialize(Configer.JsonOptionsDoc), Help.UTF8);
                        //System.IO.File.WriteAllText(System.IO.Path.Combine(outDir, "business.doc"), doc.JsonSerialize(DocJsonSettings2), Help.UTF8);
                    }
                }
            };

            return this;
        }

        /// <summary>
        /// Generating Document Model for All Business Classes. business.doc
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public virtual BootstrapAll<Business> UseDoc(System.Action<Options> options) => UseDoc(null, options);

        #region UseResultType UseArgType

        /// <summary>
        /// use result type
        /// </summary>
        /// <param name="resultType"></param>
        /// <returns></returns>
        public virtual BootstrapAll<Business> UseResultType(System.Type resultType)
        {
            if (!typeof(Result.IResult).IsAssignableFrom(resultType))
            {
                throw new System.ArgumentException(nameof(resultType), "Business.Core.Result.IResult interface not implemented");
            }

            Config.ResultType = resultType;
            return this;
        }

        ///// <summary>
        ///// use arg type
        ///// </summary>
        ///// <param name="argType"></param>
        ///// <returns></returns>
        //public virtual BootstrapAll<Business> UseArgType(System.Type argType)
        //{
        //    if (!typeof(IArg).IsAssignableFrom(argType))
        //    {
        //        throw new System.ArgumentException(nameof(argType), "Business.Core.IArg interface not implemented");
        //    }

        //    Config.ArgType = argType;
        //    return this;
        //}

        /// <summary>
        /// Log callback for all business classes
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        public virtual BootstrapAll<Business> UseLogger(Logger logger)
        {
            Config.Logger = logger;
            return this;
        }

        /// <summary>
        /// UseInjection
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public virtual BootstrapAll<Business> UseInjection(System.Func<string, System.Type, object> member)
        {
            Config.Injection += member;
            return this;
        }

        #endregion
    }
}

namespace Business.Core.Utils
{
    //using Boot;

    /// <summary>
    /// BootstrapExtensions
    /// </summary>
    public static class BootstrapExtensions
    {
        /// <summary>
        /// Use
        /// </summary>
        /// <typeparam name="Bootstrap"></typeparam>
        /// <param name="bootstrap"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        static Bootstrap Use<Bootstrap>(this Bootstrap bootstrap, System.Func<IBusiness, IBusiness> operation)
            where Bootstrap : IBootstrap
        {
            bootstrap.Config.Use.Enqueue(operation);
            return bootstrap;
        }

        /*
        /// <summary>
        /// Inject a parameter type, depending on the parameter type
        /// </summary>
        /// <param name="bootstrap"></param>
        /// <param name="parameterType"></param>
        /// <returns></returns>
        public static Bootstrap UseType<Bootstrap>(this Bootstrap bootstrap, params System.Type[] parameterType) where Bootstrap : IBootstrap => Use(bootstrap, c => Help.UseType(c, parameterType));

        /// <summary>
        /// Inject a parameter type, depending on the parameter name
        /// </summary>
        /// <typeparam name="Bootstrap"></typeparam>
        /// <param name="bootstrap"></param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public static Bootstrap UseType<Bootstrap>(this Bootstrap bootstrap, params string[] parameterName) where Bootstrap : IBootstrap => Use(bootstrap, c => Help.UseType(c, parameterName));
        */

        /// <summary>
        /// Inject a parameter type, depending on the parameter type
        /// </summary>
        /// <param name="bootstrap"></param>
        /// <param name="parameterType"></param>
        /// <returns></returns>
        public static Bootstrap UseType<Bootstrap>(this Bootstrap bootstrap, params System.Type[] parameterType) where Bootstrap : IBootstrap
        {
            if (null != parameterType)
            {
                foreach (var item in parameterType)
                {
                    bootstrap.Config.useTypes.Add(item);
                }
            }
            return bootstrap;
        }

        /// <summary>
        /// Set the log characteristics of a parameter, depending on the parameter type
        /// </summary>
        /// <param name="bootstrap"></param>
        /// <param name="logger"></param>
        /// <param name="argType"></param>
        /// <returns></returns>
        public static Bootstrap LoggerSet<Bootstrap>(this Bootstrap bootstrap, Annotations.LoggerAttribute logger, params System.Type[] argType) where Bootstrap : IBootstrap => Use(bootstrap, c => Help.LoggerSet(c, logger, argType));
        /*
        /// <summary>
        /// Set the log characteristics of a parameter, depending on the parameter name
        /// </summary>
        /// <param name="bootstrap"></param>
        /// <param name="logger"></param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public static Bootstrap LoggerSet<Bootstrap>(this Bootstrap bootstrap, Annotations.LoggerAttribute logger, params string[] parameterName) where Bootstrap : IBootstrap => Use(bootstrap, c => Help.LoggerSet(c, logger, parameterName));

        /// <summary>
        /// Set a parameter's ignore feature, depending on the parameter name
        /// </summary>
        /// <param name="bootstrap"></param>
        /// <param name="ignore"></param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public static Bootstrap IgnoreSet<Bootstrap>(this Bootstrap bootstrap, Annotations.Ignore ignore, params string[] parameterName) where Bootstrap : IBootstrap => Use(bootstrap, c => Help.IgnoreSet(c, ignore, parameterName));
        */
        /// <summary>
        /// IgnoreSet
        /// </summary>
        /// <typeparam name="Bootstrap"></typeparam>
        /// <param name="bootstrap"></param>
        /// <param name="ignore"></param>
        /// <param name="argType"></param>
        /// <returns></returns>
        public static Bootstrap IgnoreSet<Bootstrap>(this Bootstrap bootstrap, Annotations.Ignore ignore, params System.Type[] argType) where Bootstrap : IBootstrap => Use(bootstrap, c => Help.IgnoreSet(c, ignore, argType));

        /// <summary>
        /// MemberSet
        /// </summary>
        /// <typeparam name="Bootstrap"></typeparam>
        /// <param name="bootstrap"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="skipNull"></param>
        /// <returns></returns>
        public static Bootstrap MemberSet<Bootstrap>(this Bootstrap bootstrap, string name, object value, bool skipNull = false) where Bootstrap : IBootstrap => Use(bootstrap, c => Help.MemberSet(c, name, value, skipNull));
    }
}