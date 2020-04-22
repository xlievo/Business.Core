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
    using Utils;
    using Document;
    using System.Linq;

    public interface IBootstrap
    {
        BootstrapConfig Config { get; }

        IBootstrap UseDoc(string outDir = null, Config config = default);

        IBootstrap UseResultType(System.Type resultType);

        IBootstrap UseArgType(System.Type argType);

        dynamic Build();
    }

    public class BootstrapConfig
    {
        internal protected BootstrapConfig()
        {
            Use = new System.Collections.Concurrent.ConcurrentQueue<System.Func<IBusiness, IBusiness>>();
            //UseDoc = null;
        }

        public BootstrapConfig(Auth.IInterceptor interceptor, object[] constructorArguments, System.Type type = null) : this()
        {
            Interceptor = interceptor ?? new Auth.Interceptor();
            ConstructorArguments = constructorArguments;
            Type = type;
        }

        public System.Collections.Concurrent.ConcurrentQueue<System.Func<IBusiness, IBusiness>> Use { get; }

        public UseDocConfig UseDoc { get; set; }

        public Auth.IInterceptor Interceptor { get; }

        public object[] ConstructorArguments { get; }

        public System.Type Type { get; }

        public System.Type ResultType { get; set; }

        public System.Type ArgType { get; set; }

        public System.Action<IBootstrap> BuildBefore { get; set; }

        public System.Action<IBootstrap> BuildAfter { get; set; }

        public class UseDocConfig
        {
            public string OutDir { get; set; }

            public Config Config { get; internal set; }

            public System.Action<string, Config> Use { get; internal set; }
        }
    }

    public class Bootstrap : IBootstrap
    {
        public Bootstrap(BootstrapConfig config = default) => Config = config ?? new BootstrapConfig();

        #region IBootstrap

        public BootstrapConfig Config { get; }

        IBootstrap IBootstrap.UseDoc(string outDir, Config config) => UseDoc(outDir, config);

        IBootstrap IBootstrap.UseResultType(System.Type resultType) => UseResultType(resultType);

        IBootstrap IBootstrap.UseArgType(System.Type argType) => UseArgType(argType);

        #endregion

        #region Create

        /// <summary>
        /// Initialize a Generic proxy class
        /// </summary>
        /// <typeparam name="Business"></typeparam>
        /// <param name="constructorArguments"></param>
        /// <returns></returns>
        public static Bootstrap<Business> Create<Business>(params object[] constructorArguments) where Business : class => Create<Business>(null, constructorArguments);

        /// <summary>
        /// Initialize a Generic proxy class
        /// </summary>
        /// <typeparam name="Business"></typeparam>
        /// <param name="interceptor"></param>
        /// <param name="constructorArguments"></param>
        /// <returns></returns>
        public static Bootstrap<Business> Create<Business>(Auth.IInterceptor interceptor = null, params object[] constructorArguments) where Business : class => new Bootstrap<Business>(new BootstrapConfig(interceptor, constructorArguments, typeof(Business)));

        /// <summary>
        /// Initialize a Type proxy class
        /// </summary>
        /// <param name="type"></param>
        /// <param name="constructorArguments"></param>
        /// <returns></returns>
        //public static Bootstrap Create(System.Type type, params object[] constructorArguments) => Create(type, null, constructorArguments);

        /// <summary>
        /// Initialize a Type proxy class
        /// </summary>
        /// <param name="type"></param>
        /// <param name="interceptor"></param>
        /// <param name="constructorArguments"></param>
        /// <returns></returns>
        public static Bootstrap Create(System.Type type, Auth.IInterceptor interceptor = null, params object[] constructorArguments) => new Bootstrap(new BootstrapConfig(interceptor, constructorArguments, type));

        /// <summary>
        /// bootstrap all Business class
        /// </summary>
        /// <param name="constructorArguments"></param>
        /// <returns></returns>
        public static BootstrapAll CreateAll(params object[] constructorArguments) => CreateAll(null, constructorArguments);

        /// <summary>
        /// bootstrap all Business class
        /// </summary>
        /// <param name="interceptor"></param>
        /// <param name="constructorArguments"></param>
        /// <returns></returns>
        public static BootstrapAll CreateAll(Auth.IInterceptor interceptor = null, params object[] constructorArguments) => new BootstrapAll(new BootstrapConfig(interceptor, constructorArguments));

        /// <summary>
        /// bootstrap all Business class
        /// </summary>
        /// <typeparam name="Business"></typeparam>
        /// <param name="constructorArguments"></param>
        /// <returns></returns>
        public static BootstrapAll<Business> CreateAll<Business>(params object[] constructorArguments) where Business : class, IBusiness => CreateAll<Business>(null, constructorArguments);

        /// <summary>
        /// bootstrap all Business class
        /// </summary>
        /// <typeparam name="Business"></typeparam>
        /// <param name="interceptor"></param>
        /// <param name="constructorArguments"></param>
        /// <returns></returns>
        public static BootstrapAll<Business> CreateAll<Business>(Auth.IInterceptor interceptor = null, params object[] constructorArguments) where Business : class, IBusiness => new BootstrapAll<Business>(new BootstrapConfig(interceptor, constructorArguments));

        #endregion

        IBusiness business;

        /// <summary>
        /// Build
        /// </summary>
        /// <returns></returns>
        public virtual object Build()
        {
            Config.BuildBefore?.Invoke(this);

            var bind = new Bind(Config.Type, Config.Interceptor, Config.ConstructorArguments, Config.ResultType, Config.ArgType);

            business = bind.hasBusiness ? (IBusiness)bind.instance : null;

            if (null != business)
            {
                foreach (var item in Config.Use)
                {
                    item.Invoke(business);
                }

                Config.UseDoc?.Use?.Invoke(Config.UseDoc.OutDir, Config.UseDoc.Config);
            }

            Config.BuildAfter?.Invoke(this);

            return bind.instance;
        }

        /// <summary>
        /// Generating Document Model for All Business Classes. business.doc
        /// </summary>
        /// <param name="outDir"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public virtual Bootstrap UseDoc(string outDir = null, Config config = default)
        {
            this.Config.UseDoc = new BootstrapConfig.UseDocConfig
            {
                OutDir = outDir,
                Config = config,
                //Use = (dir, cfg) => business?.UseDoc(dir, cfg)
                Use = (dir, cfg) => { if (null != business) { Help.UseDoc(business, dir, cfg); } }
            };
            return this;
        }

        /// <summary>
        /// Generating Document Model for All Business Classes. business.doc
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public virtual Bootstrap UseDoc(Config config) => UseDoc(null, config);

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

        /// <summary>
        /// use arg type
        /// </summary>
        /// <param name="argType"></param>
        /// <returns></returns>
        public virtual Bootstrap UseArgType(System.Type argType)
        {
            if (!typeof(IArg).IsAssignableFrom(argType))
            {
                throw new System.ArgumentException(nameof(argType), "Business.Core.IArg interface not implemented");
            }

            Config.ArgType = argType;
            return this;
        }

        #endregion
    }

    public class Bootstrap<Business> : IBootstrap
        where Business : class
    {
        public Bootstrap(BootstrapConfig config = default) => Config = config ?? new BootstrapConfig();

        #region IBootstrap

        public BootstrapConfig Config { get; }

        IBootstrap IBootstrap.UseDoc(string outDir, Config config) => UseDoc(outDir, config);

        IBootstrap IBootstrap.UseResultType(System.Type resultType) => UseResultType(resultType);

        IBootstrap IBootstrap.UseArgType(System.Type argType) => UseArgType(argType);

        dynamic IBootstrap.Build() => Build();

        #endregion

        IBusiness business;

        public virtual Business Build()
        {
            Config.BuildBefore?.Invoke(this);

            var bind = new Bind(Config.Type, Config.Interceptor, Config.ConstructorArguments, Config.ResultType, Config.ArgType);

            business = bind.hasBusiness ? (IBusiness)bind.instance : null;

            if (null != business)
            {
                foreach (var item in Config.Use)
                {
                    item.Invoke(business);
                }

                Config.UseDoc?.Use?.Invoke(Config.UseDoc.OutDir, Config.UseDoc.Config);
            }

            Config.BuildAfter?.Invoke(this);

            return bind.instance as Business;
        }

        /// <summary>
        /// Generating Document Model for All Business Classes. business.doc
        /// </summary>
        /// <param name="outDir"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public virtual Bootstrap<Business> UseDoc(string outDir = null, Config config = default)
        {
            this.Config.UseDoc = new BootstrapConfig.UseDocConfig
            {
                OutDir = outDir,
                Config = config,
                //Use = (dir, cfg) => business?.UseDoc(dir, cfg)
                Use = (dir, cfg) => { if (null != business) { Help.UseDoc(business, dir, cfg); } }
            };
            return this;
        }

        public virtual Bootstrap<Business> UseDoc(Config config) => UseDoc(null, config);

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

        /// <summary>
        /// use arg type
        /// </summary>
        /// <param name="argType"></param>
        /// <returns></returns>
        public virtual Bootstrap<Business> UseArgType(System.Type argType)
        {
            if (!typeof(IArg).IsAssignableFrom(argType))
            {
                throw new System.ArgumentException(nameof(argType), "Business.Core.IArg interface not implemented");
            }

            Config.ArgType = argType;
            return this;
        }

        #endregion
    }

    public class BootstrapAll : IBootstrap
    {
        public BootstrapAll(BootstrapConfig config = default) => Config = config ?? new BootstrapConfig();

        #region IBootstrap

        public BootstrapConfig Config { get; }

        IBootstrap IBootstrap.UseDoc(string outDir, Config config) => UseDoc(outDir, config);

        IBootstrap IBootstrap.UseResultType(System.Type resultType) => UseResultType(resultType);

        IBootstrap IBootstrap.UseArgType(System.Type argType) => UseArgType(argType);

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

            Help.LoadAssemblys((null == assemblyFiles || !assemblyFiles.Any()) ? System.IO.Directory.GetFiles(Help.BaseDirectory, "*.dll") : assemblyFiles, true, type =>
            {
                if (typeof(IBusiness).IsAssignableFrom(type) && !type.IsAbstract)
                {
                    if (null != businessTypeFullName && businessTypeFullName.Any())
                    {
                        if (businessTypeFullName.Contains(type.FullName))
                        {
                            new Bind(type, Config.Interceptor, Config.ConstructorArguments, Config.ResultType, Config.ArgType);
                            //Create(type, bootstrap.constructorArguments);
                            return true;
                        }
                    }
                    else
                    {
                        //Create(type, bootstrap.constructorArguments);
                        new Bind(type, Config.Interceptor, Config.ConstructorArguments, Config.ResultType, Config.ArgType);
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

            Config.UseDoc.Use?.Invoke(Config.UseDoc.OutDir, Config.UseDoc.Config);

            Config.BuildAfter?.Invoke(this);
        }

        /// <summary>
        /// Generating Document Model for All Business Classes. business.doc
        /// </summary>
        /// <param name="outDir"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public virtual BootstrapAll UseDoc(string outDir = null, Config config = default)
        {
            this.Config.UseDoc = new BootstrapConfig.UseDocConfig
            {
                OutDir = outDir,
                Config = config,
                Use = (dir, cfg) =>
                {
                    var exists = !string.IsNullOrEmpty(dir) && System.IO.Directory.Exists(dir);
                    var doc = new System.Collections.Generic.Dictionary<string, IDoc>();

                    foreach (var item in Configer.BusinessList.OrderBy(c => c.Key))
                    {
                        //item.Value.UseDoc(null, cfg);
                        Help.UseDoc(item.Value, null, cfg);

                        if (exists)
                        {
                            doc.Add(item.Value.Configer.Doc.Name, item.Value.Configer.Doc);
                        }
                    }

                    if (exists)
                    {
                        System.IO.File.WriteAllText(System.IO.Path.Combine(dir, "business.doc"), doc.JsonSerialize(Configer.DocJsonSettings), Help.UTF8);
                        //System.IO.File.WriteAllText(System.IO.Path.Combine(outDir, "business.doc"), doc.JsonSerialize(DocJsonSettings2), Help.UTF8);
                    }
                }
            };

            return this;
        }

        public virtual BootstrapAll UseDoc(Config config) => UseDoc(null, config);

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

        /// <summary>
        /// use arg type
        /// </summary>
        /// <param name="argType"></param>
        /// <returns></returns>
        public virtual BootstrapAll UseArgType(System.Type argType)
        {
            if (!typeof(IArg).IsAssignableFrom(argType))
            {
                throw new System.ArgumentException(nameof(argType), "Business.Core.IArg interface not implemented");
            }

            Config.ArgType = argType;
            return this;
        }

        #endregion
    }

    public class BootstrapAll<Business> : IBootstrap
        where Business : class, IBusiness
    {
        public ConcurrentReadOnlyDictionary<string, Business> BusinessList = new ConcurrentReadOnlyDictionary<string, Business>(System.StringComparer.InvariantCultureIgnoreCase);

        public BootstrapAll(BootstrapConfig config = default) => Config = config ?? new BootstrapConfig();

        #region IBootstrap

        public BootstrapConfig Config { get; }

        IBootstrap IBootstrap.UseDoc(string outDir, Config config) => UseDoc(outDir, config);

        IBootstrap IBootstrap.UseResultType(System.Type resultType) => UseResultType(resultType);

        IBootstrap IBootstrap.UseArgType(System.Type argType) => UseArgType(argType);

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

            Help.LoadAssemblys((null == assemblyFiles || !assemblyFiles.Any()) ? System.IO.Directory.GetFiles(Help.BaseDirectory, "*.dll") : assemblyFiles, true, type =>
            {
                if (typeof(IBusiness).IsAssignableFrom(type) && !type.IsAbstract)
                {
                    if (null != businessTypeFullName && businessTypeFullName.Any())
                    {
                        if (businessTypeFullName.Contains(type.FullName))
                        {
                            if (new Bind(type, Config.Interceptor, Config.ConstructorArguments, Config.ResultType, Config.ArgType).instance is Business business)
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
                        if (new Bind(type, Config.Interceptor, Config.ConstructorArguments, Config.ResultType, Config.ArgType).instance is Business business)
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

            Config.UseDoc?.Use?.Invoke(Config.UseDoc.OutDir, Config.UseDoc.Config);

            Config.BuildAfter?.Invoke(this);
        }

        /// <summary>
        /// Generating Document Model for All Business Classes. business.doc
        /// </summary>
        /// <param name="outDir"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public virtual BootstrapAll<Business> UseDoc(string outDir = null, Config config = default)
        {
            this.Config.UseDoc = new BootstrapConfig.UseDocConfig
            {
                OutDir = outDir,
                Config = config,
                Use = (dir, cfg) =>
                {
                    var exists = !string.IsNullOrEmpty(dir) && System.IO.Directory.Exists(dir);
                    var doc = new System.Collections.Generic.Dictionary<string, IDoc>();

                    foreach (var item in BusinessList.OrderBy(c => c.Key))
                    {
                        //item.Value.UseDoc(null, cfg);
                        Help.UseDoc(item.Value, null, cfg);

                        if (exists)
                        {
                            doc.Add(item.Value.Configer.Doc.Name, item.Value.Configer.Doc);
                        }
                    }

                    if (exists)
                    {
                        System.IO.File.WriteAllText(System.IO.Path.Combine(dir, "business.doc"), doc.JsonSerialize(Configer.DocJsonSettings), Help.UTF8);
                        //System.IO.File.WriteAllText(System.IO.Path.Combine(outDir, "business.doc"), doc.JsonSerialize(DocJsonSettings2), Help.UTF8);
                    }
                }
            };

            return this;
        }

        /// <summary>
        /// Generating Document Model for All Business Classes. business.doc
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public virtual BootstrapAll<Business> UseDoc(Config config) => UseDoc(null, config);

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

        /// <summary>
        /// use arg type
        /// </summary>
        /// <param name="argType"></param>
        /// <returns></returns>
        public virtual BootstrapAll<Business> UseArgType(System.Type argType)
        {
            if (!typeof(IArg).IsAssignableFrom(argType))
            {
                throw new System.ArgumentException(nameof(argType), "Business.Core.IArg interface not implemented");
            }

            Config.ArgType = argType;
            return this;
        }

        #endregion
    }
}

namespace Business.Core.Utils
{
    //using Boot;

    public static class BootstrapExtensions
    {
        public static Bootstrap Use<Bootstrap>(this Bootstrap bootstrap, System.Func<IBusiness, IBusiness> operation)
            where Bootstrap : IBootstrap
        {
            bootstrap.Config.Use.Enqueue(operation);
            return bootstrap;
        }

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

        /// <summary>
        /// Set the log characteristics of a parameter, depending on the parameter type
        /// </summary>
        /// <param name="bootstrap"></param>
        /// <param name="logger"></param>
        /// <param name="argType"></param>
        /// <returns></returns>
        public static Bootstrap LoggerSet<Bootstrap>(this Bootstrap bootstrap, Annotations.LoggerAttribute logger, params System.Type[] argType) where Bootstrap : IBootstrap => Use(bootstrap, c => Help.LoggerSet(c, logger, argType));

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

        public static Bootstrap IgnoreSet<Bootstrap>(this Bootstrap bootstrap, Annotations.Ignore ignore, params System.Type[] argType) where Bootstrap : IBootstrap => Use(bootstrap, c => Help.IgnoreSet(c, ignore, argType));

        public static Bootstrap MemberSet<Bootstrap>(this Bootstrap bootstrap, string memberName, object memberObj, bool skipNull = false) where Bootstrap : IBootstrap => Use(bootstrap, c => Help.MemberSet(c, memberName, memberObj, skipNull));
    }
}