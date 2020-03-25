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
    }

    public class BootstrapConfig
    {
        internal protected BootstrapConfig()
        {
            Use = new System.Collections.Concurrent.ConcurrentQueue<System.Func<IBusiness, IBusiness>>();
            UseDoc = null;
        }

        public BootstrapConfig(Auth.IInterceptor interceptor, object[] constructorArguments, System.Type type = null) : this()
        {
            Interceptor = interceptor ?? new Auth.Interceptor();
            ConstructorArguments = constructorArguments;
            Type = type;
        }

        public System.Collections.Concurrent.ConcurrentQueue<System.Func<IBusiness, IBusiness>> Use { get; }

        public System.Action UseDoc { get; internal set; }

        public Auth.IInterceptor Interceptor { get; }

        public object[] ConstructorArguments { get; }

        public System.Type Type { get; }
    }

    public class Bootstrap : IBootstrap
    {
        public Bootstrap(BootstrapConfig config = default) => Config = config ?? new BootstrapConfig();

        public BootstrapConfig Config { get; }

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
        public static BootstrapAll Create(params object[] constructorArguments) => Create(null, constructorArguments);

        /// <summary>
        /// bootstrap all Business class
        /// </summary>
        /// <param name="interceptor"></param>
        /// <param name="constructorArguments"></param>
        /// <returns></returns>
        public static BootstrapAll Create(Auth.IInterceptor interceptor = null, params object[] constructorArguments) => new BootstrapAll(new BootstrapConfig(interceptor, constructorArguments));

        #endregion

        IBusiness business;

        public virtual object Build()
        {
            var bind = new Bind(Config.Type, Config.Interceptor, Config.ConstructorArguments);

            business = bind.hasBusiness ? (IBusiness)bind.instance : null;

            if (null != business)
            {
                foreach (var item in Config.Use)
                {
                    item.Invoke(business);
                }

                //Config.Use.ForEach(c => c.Value.Invoke(business));

                Config.UseDoc?.Invoke();
            }

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
            this.Config.UseDoc = () => business?.UseDoc(outDir, config);
            return this;
        }
    }

    public class Bootstrap<Business> : IBootstrap
        where Business : class
    {
        public Bootstrap(BootstrapConfig config = default) => Config = config ?? new BootstrapConfig();

        public BootstrapConfig Config { get; }

        IBusiness business;

        public virtual Business Build()
        {
            var bind = new Bind(Config.Type, Config.Interceptor, Config.ConstructorArguments);

            business = bind.hasBusiness ? (IBusiness)bind.instance : null;

            if (null != business)
            {
                foreach (var item in Config.Use)
                {
                    item.Invoke(business);
                }

                //Config.Use.ForEach(c => c.Value.Invoke(business));

                Config.UseDoc?.Invoke();
            }

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
            this.Config.UseDoc = () => business?.UseDoc(outDir, config);
            return this;
        }
    }

    public class BootstrapAll : IBootstrap
    {
        public BootstrapAll(BootstrapConfig config = default) => Config = config ?? new BootstrapConfig();

        public BootstrapConfig Config { get; }

        /// <summary>
        /// Load all business classes in the run directory
        /// </summary>
        /// <param name="assemblyFiles"></param>
        /// <param name="businessTypeFullName"></param>
        public virtual void Build(string[] assemblyFiles = null, string[] businessTypeFullName = null)
        {
            Help.LoadAssemblys((null == assemblyFiles || !assemblyFiles.Any()) ? System.IO.Directory.GetFiles(Help.BaseDirectory, "*.dll") : assemblyFiles, true, type =>
            {
                if (typeof(IBusiness).IsAssignableFrom(type) && !type.IsAbstract)
                {
                    if (null != businessTypeFullName && businessTypeFullName.Any())
                    {
                        if (businessTypeFullName.Contains(type.FullName))
                        {
                            new Bind(type, Config.Interceptor, Config.ConstructorArguments);
                            //Create(type, bootstrap.constructorArguments);
                            return true;
                        }
                    }
                    else
                    {
                        //Create(type, bootstrap.constructorArguments);
                        new Bind(type, Config.Interceptor, Config.ConstructorArguments);
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
                //Config.Use.ForEach(c => c.Value.Invoke(business));
            }

            Config.UseDoc?.Invoke();
        }

        /// <summary>
        /// Generating Document Model for All Business Classes. business.doc
        /// </summary>
        /// <param name="outDir"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public virtual BootstrapAll UseDoc(string outDir = null, Config config = default)
        {
            this.Config.UseDoc = () =>
            {
                var exists = !string.IsNullOrEmpty(outDir) && System.IO.Directory.Exists(outDir);
                var doc = new System.Collections.Generic.Dictionary<string, IDoc>();

                foreach (var item in Configer.BusinessList.OrderBy(c => c.Key))
                {
                    item.Value.UseDoc(null, config);

                    if (exists)
                    {
                        doc.Add(item.Value.Configer.Doc.Name, item.Value.Configer.Doc);
                    }
                }

                if (exists)
                {
                    System.IO.File.WriteAllText(System.IO.Path.Combine(outDir, "business.doc"), doc.JsonSerialize(Configer.DocJsonSettings), Help.UTF8);
                    //System.IO.File.WriteAllText(System.IO.Path.Combine(outDir, "business.doc"), doc.JsonSerialize(DocJsonSettings2), Help.UTF8);
                }
            };

            return this;
        }
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
        public static Bootstrap UseType<Bootstrap>(this Bootstrap bootstrap, params System.Type[] parameterType) where Bootstrap : IBootstrap => Use(bootstrap, c => c.UseType(parameterType));

        /// <summary>
        /// Inject a parameter type, depending on the parameter name
        /// </summary>
        /// <typeparam name="Bootstrap"></typeparam>
        /// <param name="bootstrap"></param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public static Bootstrap UseType<Bootstrap>(this Bootstrap bootstrap, params string[] parameterName) where Bootstrap : IBootstrap => Use(bootstrap, c => c.UseType(parameterName));

        /// <summary>
        /// Set the log characteristics of a parameter, depending on the parameter type
        /// </summary>
        /// <param name="bootstrap"></param>
        /// <param name="logger"></param>
        /// <param name="argType"></param>
        /// <returns></returns>
        public static Bootstrap LoggerSet<Bootstrap>(this Bootstrap bootstrap, Annotations.LoggerAttribute logger, params System.Type[] argType) where Bootstrap : IBootstrap => Use(bootstrap, c => c.LoggerSet(logger, argType));

        /// <summary>
        /// Set the log characteristics of a parameter, depending on the parameter name
        /// </summary>
        /// <param name="bootstrap"></param>
        /// <param name="logger"></param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public static Bootstrap LoggerSet<Bootstrap>(this Bootstrap bootstrap, Annotations.LoggerAttribute logger, params string[] parameterName) where Bootstrap : IBootstrap => Use(bootstrap, c => c.LoggerSet(logger, parameterName));

        /// <summary>
        /// Set a parameter's ignore feature, depending on the parameter name
        /// </summary>
        /// <param name="bootstrap"></param>
        /// <param name="ignore"></param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public static Bootstrap IgnoreSet<Bootstrap>(this Bootstrap bootstrap, Annotations.Ignore ignore, params string[] parameterName) where Bootstrap : IBootstrap => Use(bootstrap, c => c.IgnoreSet(ignore, parameterName));

        public static Bootstrap IgnoreSet<Bootstrap>(this Bootstrap bootstrap, Annotations.Ignore ignore, params System.Type[] argType) where Bootstrap : IBootstrap => Use(bootstrap, c => c.IgnoreSet(ignore, argType));

        public static Bootstrap MemberSet<Bootstrap>(this Bootstrap bootstrap, string memberName, object memberObj, bool skipNull = false) where Bootstrap : IBootstrap => Use(bootstrap, c => c.MemberSet(memberName, memberObj, skipNull));
    }
}