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
        Bootstrap.BootstrapConfig Config { get; }
    }

    public class Bootstrap : IBootstrap
    {
        public Bootstrap(BootstrapConfig config = default) => Config = config ?? new BootstrapConfig();

        public class BootstrapConfig
        {
            internal protected BootstrapConfig()
            {
                Use = new ConcurrentLinkedList<System.Func<IBusiness, IBusiness>>();
                UseDoc = null;
            }

            public BootstrapConfig(Auth.IInterceptor interceptor, object[] constructorArguments, System.Type type = null) : this()
            {
                Interceptor = interceptor;
                ConstructorArguments = constructorArguments;
                Type = type;
            }

            public ConcurrentLinkedList<System.Func<IBusiness, IBusiness>> Use { get; }

            public System.Func<IBootstrap> UseDoc { get; internal set; }

            public Auth.IInterceptor Interceptor { get; }

            public object[] ConstructorArguments { get; }

            public System.Type Type { get; }
        }

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

        public static BootstrapAll Create(params object[] constructorArguments) => Create(null, constructorArguments);

        /// <summary>
        /// bootstrap all Business class
        /// </summary>
        /// <param name="interceptor"></param>
        /// <param name="constructorArguments"></param>
        /// <returns></returns>
        public static BootstrapAll Create(Auth.IInterceptor interceptor = null, params object[] constructorArguments) => new BootstrapAll(new BootstrapConfig(interceptor, constructorArguments));

        #endregion

        internal IBusiness business;

        public virtual object Build()
        {
            var bind = new Bind(Config.Type, Config.Interceptor ?? new Auth.Interceptor(), Config.ConstructorArguments);

            business = bind.hasBusiness ? (IBusiness)bind.instance : null;

            if (null != business)
            {
                var first = Config.Use.First;

                while (NodeState.DAT == first.State)
                {
                    var conf = first.Value;

                    conf.Invoke(business);

                    first = first.Next;
                }

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
            this.Config.UseDoc = () =>
            {
                business?.UseDoc(outDir, config);
                return this;
            };

            return this;
        }
    }

    public class Bootstrap<Business> : Bootstrap
        where Business : class
    {
        public Bootstrap(BootstrapConfig config = default) : base(config) { }

        public new virtual Business Build() => base.Build() as Business;

        /// <summary>
        /// Generating Document Model for All Business Classes. business.doc
        /// </summary>
        /// <param name="outDir"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public new virtual Bootstrap<Business> UseDoc(string outDir = null, Config config = default) => base.UseDoc(outDir, config) as Bootstrap<Business>;
    }

    public class BootstrapAll : Bootstrap
    {
        public BootstrapAll(BootstrapConfig config = default) : base(config) { }

        /// <summary>
        /// Load all business classes in the run directory
        /// </summary>
        /// <param name="assemblyFiles"></param>
        /// <param name="businessTypeName"></param>
        public virtual void Build(string[] assemblyFiles = null, string[] businessTypeName = null)
        {
            Help.LoadAssemblys((null == assemblyFiles || !assemblyFiles.Any()) ? System.IO.Directory.GetFiles(Help.baseDirectory, "*.dll") : assemblyFiles, true, type =>
            {
                if (typeof(IBusiness).IsAssignableFrom(type) && !type.IsAbstract)
                {
                    if (null != businessTypeName && businessTypeName.Any())
                    {
                        if (businessTypeName.Contains(type.Name))
                        {
                            new Bind(type, Config.Interceptor ?? new Auth.Interceptor(), Config.ConstructorArguments);
                            //Create(type, bootstrap.constructorArguments);
                            return true;
                        }
                    }
                    else
                    {
                        //Create(type, bootstrap.constructorArguments);
                        new Bind(type, Config.Interceptor ?? new Auth.Interceptor(), Config.ConstructorArguments);
                        return true;
                    }
                }

                return false;
            });

            foreach (var business in Configer.BusinessList.Values.AsParallel())
            {
                var first = Config.Use.First;

                while (NodeState.DAT == first.State)
                {
                    var conf = first.Value;

                    conf.Invoke(business);

                    first = first.Next;
                }
            }

            Config.UseDoc?.Invoke();
        }

        /// <summary>
        /// Generating Document Model for All Business Classes. business.doc
        /// </summary>
        /// <param name="outDir"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public new virtual BootstrapAll UseDoc(string outDir = null, Config config = default)
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
                    System.IO.File.WriteAllText(System.IO.Path.Combine(outDir, "business.doc"), doc.JsonSerialize(Configer.DocJsonSettings), Help.utf8);
                    //System.IO.File.WriteAllText(System.IO.Path.Combine(outDir, "business.doc"), doc.JsonSerialize(DocJsonSettings2), Help.UTF8);
                }

                return this;
            };

            return this;
        }
    }
}

namespace Business.Core.Utils
{
    public static class BootstrapExtensions
    {
        public static Bootstrap Use<Bootstrap>(this Bootstrap bootstrap, System.Func<IBusiness, IBusiness> operation)
            where Bootstrap : IBootstrap
        {
            bootstrap.Config.Use.TryAdd(operation);
            return bootstrap;
        }

        /// <summary>
        /// Inject a parameter type, depending on the parameter type
        /// </summary>
        /// <param name="bootstrap"></param>
        /// <param name="argType"></param>
        /// <returns></returns>
        public static Bootstrap UseType<Bootstrap>(this Bootstrap bootstrap, params System.Type[] argType) where Bootstrap : IBootstrap => Use(bootstrap, c => c.UseType(argType));

        /// <summary>
        /// Inject a parameter type, depending on the parameter name
        /// </summary>
        /// <typeparam name="Bootstrap"></typeparam>
        /// <param name="bootstrap"></param>
        /// <param name="argName"></param>
        /// <returns></returns>
        public static Bootstrap UseType<Bootstrap>(this Bootstrap bootstrap, params string[] argName) where Bootstrap : IBootstrap => Use(bootstrap, c => c.UseType(argName));

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
        /// <param name="argName"></param>
        /// <returns></returns>
        public static Bootstrap LoggerSet<Bootstrap>(this Bootstrap bootstrap, Annotations.LoggerAttribute logger, params string[] argName) where Bootstrap : IBootstrap => Use(bootstrap, c => c.LoggerSet(logger, argName));

        /// <summary>
        /// Set a parameter's ignore feature, depending on the parameter name
        /// </summary>
        /// <param name="bootstrap"></param>
        /// <param name="ignore"></param>
        /// <param name="argName"></param>
        /// <returns></returns>
        public static Bootstrap IgnoreSet<Bootstrap>(this Bootstrap bootstrap, Annotations.Ignore ignore, params string[] argName) where Bootstrap : IBootstrap => Use(bootstrap, c => c.IgnoreSet(ignore, argName));

        public static Bootstrap IgnoreSet<Bootstrap>(this Bootstrap bootstrap, Annotations.Ignore ignore, params System.Type[] argType) where Bootstrap : IBootstrap => Use(bootstrap, c => c.IgnoreSet(ignore, argType));

        public static Bootstrap MemberSet<Bootstrap>(this Bootstrap bootstrap, string memberName, object memberObj, bool skipNull = false) where Bootstrap : IBootstrap => Use(bootstrap, c => c.MemberSet(memberName, memberObj, skipNull));
    }
}