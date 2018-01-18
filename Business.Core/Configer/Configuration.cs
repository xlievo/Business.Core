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

namespace Business.Configer
{
    using Business.Utils;
    using Business.Meta;
    using System.Linq;
    using System.Reflection;

    #region ConfigJson

    public struct ConfigJson
    {
        public static implicit operator ConfigJson(string value) => Utils.Help.TryJsonDeserialize<ConfigJson>(value);

        public override string ToString() => Utils.Help.JsonSerialize(this);

        public ConfigJson(System.Collections.Generic.List<ConfigAttribute> attributes)
        {
            this.attributes = attributes;
            this.group = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<ConfigAttribute>>>();

            if (0 < this.attributes.Count)
            {
                var business = this.attributes.GroupBy(c => c.Target.Business, System.StringComparer.CurrentCultureIgnoreCase);

                foreach (var item in business)
                {
                    var method = item.GroupBy(c => c.Target.Method, System.StringComparer.CurrentCultureIgnoreCase);
                    this.group.Add(item.Key, method.ToDictionary(c => c.Key, c => c.ToList()));
                }
            }
        }

        readonly System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<ConfigAttribute>>> group;
        [Newtonsoft.Json.JsonIgnore]
        public System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<ConfigAttribute>>> Group { get => group; }

        readonly System.Collections.Generic.List<ConfigAttribute> attributes;
        public System.Collections.Generic.List<ConfigAttribute> Attributes { get => attributes; }

        public struct ConfigAttribute
        {
            public override string ToString() => string.Format("{0}->{1}", Target.ToString(), Attribute);

            public ConfigAttribute(ConfigTarget target, string group, string attribute, System.Collections.Generic.Dictionary<string, object> member)
            {
                this.target = target;
                this.group = !System.String.IsNullOrWhiteSpace(group) ? group.Trim() : Bind.CommandGroupDefault;
                this.attribute = !System.String.IsNullOrWhiteSpace(attribute) ? attribute.Trim() : System.String.Empty;
                this.member = member;
            }

            readonly ConfigTarget target;
            public ConfigTarget Target { get => target; }

            readonly string group;
            public System.String Group { get => group; }

            readonly string attribute;
            public System.String Attribute { get => attribute; }

            readonly System.Collections.Generic.Dictionary<string, object> member;
            public System.Collections.Generic.Dictionary<string, object> Member { get => member; }
        }

        public struct ConfigTarget
        {
            public static implicit operator ConfigTarget(string value) => new ConfigTarget(value);

            public override string ToString() => this.target;

            const char separator = '.';
            public ConfigTarget(string target)
            {
                this.target = business = method = parameter = property = System.String.Empty;
                if (System.String.IsNullOrWhiteSpace(target)) { return; }

                var sp = target.Split(separator);
                if (!System.String.IsNullOrWhiteSpace(sp[0])) { business = sp[0].Trim(); this.target += business; }

                if (1 < sp.Length)
                {
                    if (!System.String.IsNullOrWhiteSpace(sp[1])) { method = sp[1].Trim(); this.target += string.Format("{0}{1}", separator, method); }

                    if (2 < sp.Length)
                    {
                        if (!System.String.IsNullOrWhiteSpace(sp[2])) { parameter = sp[2].Trim(); this.target += string.Format("{0}{1}", separator, parameter); }

                        if (3 < sp.Length)
                        {
                            if (!System.String.IsNullOrWhiteSpace(sp[3])) { property = sp[3].Trim(); this.target += string.Format("{0}{1}", separator, property); }
                        }
                    }
                }
            }

            readonly string target;

            readonly string business;
            public string Business { get => business; }

            readonly string method;
            public string Method { get => method; }

            readonly string parameter;
            public string Parameter { get => parameter; }

            readonly string property;
            public string Property { get => property; }
        }
    }

    #endregion

#if !Mobile
    public partial class Configuration
    {
        #region Static

        /// <summary>
        /// *
        /// </summary>
        const string Any = "*";

        static readonly string CfgPath = System.IO.Path.Combine(Help.BaseDirectory, "business.json");
        static System.DateTime cfgLastTime = System.DateTime.MinValue;

        static readonly System.Threading.ReaderWriterLockSlim locker = new System.Threading.ReaderWriterLockSlim();
        internal static readonly System.IO.FileSystemWatcher CfgWatcher = new System.IO.FileSystemWatcher(System.IO.Path.GetDirectoryName(CfgPath)) { InternalBufferSize = 4096, Filter = System.IO.Path.GetFileName(CfgPath), NotifyFilter = System.IO.NotifyFilters.LastWrite | System.IO.NotifyFilters.FileName };

        static Configuration()
        {
            CfgWatcher.Renamed += Changed;
            CfgWatcher.Changed += Changed;
        }

        static ConfigJson GetConfig()
        {
            if (!System.IO.File.Exists(CfgPath)) { return default(ConfigJson); }

            if (!locker.TryEnterReadLock(300)) { return default(ConfigJson); }

            try
            {
                return Help.FileReadString(CfgPath);
            }
            catch
            {
                return default(ConfigJson);
            }
            finally { locker.ExitReadLock(); }
        }

        static void Changed(object sender, System.IO.FileSystemEventArgs e)
        {
            if (!e.FullPath.Equals(CfgPath, System.StringComparison.CurrentCultureIgnoreCase)) { return; }

            var lastTime = System.IO.File.GetLastWriteTime(e.FullPath);

            if (0 <= cfgLastTime.CompareTo(lastTime)) { return; }

            cfgLastTime = lastTime;

            //------------------//

            var dels = Bind.businessList.Where(c => c.Value.Configuration.EnableWatcher).Select(c => c.Value.Configuration).ToList();
            if (0 == dels.Count) { return; }

            var config = GetConfig();
            if (default(ConfigJson).Equals(config) || 0 == config.Attributes.Count) { return; }

            foreach (var item in config.Group)
            {
                var dels2 = dels.Where(c => c.info.BusinessName.Equals(item.Key));

                foreach (var del in dels2)
                {
                    SetBusinessAttribute(del.attributes, del.metaData, item.Value);
                }
            }
        }

        #endregion

        /*
        public bool Logger(LoggerType type, bool canWrite = false, LoggerValueMode? canValue = null, bool canResult = false, string method = Any)
        {
            if (System.String.IsNullOrWhiteSpace(method))
            {
                return false;
            }

            if (Any == method)
            {
                foreach (var meta in this.metaData)
                {
                    SetMetaLogger(meta.Value.MetaLogger, type, canWrite, canValue ?? LoggerValueMode.No, canResult);
                }

                return true;
            }
            else
            {
                if (this.metaData.TryGetValue(method, out MetaData meta))
                {
                    SetMetaLogger(meta.MetaLogger, type, canWrite, canValue ?? LoggerValueMode.No, canResult);

                    return true;
                }
            }

            return false;
        }

        static void SetMetaLogger(MetaLogger metaLogger, LoggerType type, bool canWrite, LoggerValueMode canValue, bool canResult)
        {
            switch (type)
            {
                case LoggerType.Record:
                    metaLogger.RecordAttr.CanWrite = canWrite;
                    metaLogger.RecordAttr.CanResult = canResult;
                    metaLogger.RecordAttr.CanValue = canValue;
                    break;
                case LoggerType.Error:
                    metaLogger.ErrorAttr.CanWrite = canWrite;
                    metaLogger.ErrorAttr.CanResult = canResult;
                    metaLogger.ErrorAttr.CanValue = canValue;
                    break;
                case LoggerType.Exception:
                    metaLogger.ExceptionAttr.CanWrite = canWrite;
                    metaLogger.ExceptionAttr.CanResult = canResult;
                    metaLogger.ExceptionAttr.CanValue = canValue;
                    break;
            }
        }
        
         public static void Logger(System.Collections.Concurrent.ConcurrentDictionary<string, MetaData> metaData, System.Collections.Generic.IEnumerable<LoggerItem> group)
        {
            var all = group.Where(c => Any == c.Method);
            var loggers = group.Where(g => metaData.ContainsKey(g.Method));

            foreach (var item in all)
            {
                foreach (var meta in metaData)
                {
                    if (loggers.Any(c => c.Method.Equals(meta.Key) && c.Type.Value.Equals(item.Type.Value))) { continue; }

                    SetMetaLogger(meta.Value.MetaLogger, item.Type.Value, item.CanWrite, item.CanValue.HasValue ? item.CanValue.Value : LoggerValueMode.No, item.CanResult);
                }

                System.Console.WriteLine(System.String.Format("Logger {0} {1} {2}", item.Business, item.Method, item.Type));
            }

            foreach (var item in loggers)
            {
                SetMetaLogger(metaData[item.Method].MetaLogger, item.Type.Value, item.CanWrite, item.CanValue.HasValue ? item.CanValue.Value : LoggerValueMode.No, item.CanResult);

                System.Console.WriteLine(System.String.Format("Logger {0} {1} {2}", item.Business, item.Method, item.Type));
            }
        }
        public static void Attribute(System.Collections.Concurrent.ConcurrentDictionary<string, MetaData> metaData, System.Collections.Generic.IEnumerable<AttributeItem> group)
        {
            foreach (var item in group)
            {
                if (Attribute(metaData, item.Method, item.Argument, item.Attribute, item.Member, item.Value))
                {
                    System.Console.WriteLine(System.String.Format("Attribute {0} {1} {2} {3} {4}", item.Business, item.Method, item.Argument, item.Attribute, item.Member));
                }
            }
        }
        
        public bool Attribute(string method, string argument, string attributeFullName, string member, string value)
        {
            return Attribute(metaData, method, argument, attributeFullName, member, value);
        }

        public static bool Attribute(System.Collections.Concurrent.ConcurrentDictionary<string, MetaData> metaData, string method, string argument, string attribute, string member, string value)
        {
            if (System.String.IsNullOrWhiteSpace(method) || System.String.IsNullOrWhiteSpace(argument) || System.String.IsNullOrWhiteSpace(attribute) || System.String.IsNullOrWhiteSpace(member))
            {
                return false;
            }

            if (metaData.TryGetValue(method, out MetaData meta))
            {
                Args arg = default(Args);
                var args = meta.ArgAttrs[Bind.GetCommandGroupDefault(method)].Args;
                foreach (var item in argument.Split('.'))
                {
                    arg = args.FirstOrDefault(c => c.Name == item);
                    if (default(Args).Equals(arg)) { break; }
                    args = arg.ArgAttrChild;
                }

                if (default(Args).Equals(arg)) { return false; }

                //var attr = arg.ArgAttr.Where(c => c.FullName == attributeFullName);

                //foreach (var item in attr)
                //{
                //    item.MemberSet(member, value);
                //}

                //return true;

                var attr = arg.ArgAttr.FirstOrDefault(c => c.FullName == attribute);
                if (null == attr) { return false; }

                return attr.MemberSet(member, value);
            }

            return false;
        }
        */
    }
#endif
    public partial class Configuration
    {
        internal readonly System.String ID = System.Guid.NewGuid().ToString("N");

        public Configuration(Attributes.InfoAttribute info, TypeInfo resultType, System.Func<Auth.IToken> token, TypeInfo requestType, System.Collections.Concurrent.ConcurrentDictionary<string, MetaData> metaData, System.Collections.Generic.List<Attributes.AttributeBase> attributes, bool enableWatcher, System.Collections.Concurrent.ConcurrentDictionary<string, Attributes.RouteAttribute> routes)
        {
            this.info = info;
            this.resultType = resultType;
            this.token = token;
            this.requestType = requestType;
            this.requestDefault = System.Activator.CreateInstance(requestType.MakeGenericType(typeof(object)));
            this.metaData = metaData;
            this.attributes = attributes;
            this.routes = routes;

#if !Mobile
            if (enableWatcher && !CfgWatcher.EnableRaisingEvents)
            {
                CfgWatcher.EnableRaisingEvents = true;
            }
#endif
            this.enableWatcher = enableWatcher;
        }

#if !Mobile
        #region set business attribute

        static void SetBusinessAttribute(System.Collections.Generic.IReadOnlyList<Attributes.AttributeBase> attributes, System.Collections.Concurrent.ConcurrentDictionary<string, MetaData> metaData, System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<ConfigJson.ConfigAttribute>> configAttributes)
        {
            foreach (var item in configAttributes)
            {
                if (0 == item.Value.Count) { continue; }

                switch (item.Key)
                {
                    //business
                    case "":
                        foreach (var item2 in item.Value)
                        {
                            var attribute = attributes.FirstOrDefault(c => c.TypeFullName.Equals(item2.Attribute, System.StringComparison.CurrentCultureIgnoreCase));

                            if (null == attribute) { continue; }

                            foreach (var item3 in item2.Member)
                            {
                                attribute[item3.Key] = item3.Value;
                            }
                        }
                        break;
                    //all method
                    case Any:
                        foreach (var item2 in metaData)
                        {
                            SetMethodAttribute(item2.Value, item.Value);
                        }
                        break;
                    //method
                    default:
                        if (!metaData.TryGetValue(item.Key, out MetaData method)) { break; }
                        SetMethodAttribute(method, item.Value);
                        break;
                }
            }
        }

        static void SetMethodAttribute(MetaData method, System.Collections.Generic.List<ConfigJson.ConfigAttribute> item)
        {
            var methodAttrsGroup = item.GroupBy(c => c.Target.Parameter);

            foreach (var item2 in methodAttrsGroup)
            {
                switch (item2.Key)
                {
                    //method
                    case "":
                        foreach (var configAttribute in item2)
                        {
                            var attribute = method.Attributes.FirstOrDefault(c => c.TypeFullName.Equals(configAttribute.Attribute, System.StringComparison.CurrentCultureIgnoreCase));

                            if (null == attribute)
                            {
                                continue;
                            }

                            foreach (var member in configAttribute.Member)
                            {
                                attribute[member.Key] = member.Value;
                            }
                        }
                        break;
                    case Any: //any parameter
                        foreach (var configAttribute in item2)
                        {
                            if (!method.ArgAttrs.TryGetValue(Bind.GetCommandGroup(configAttribute.Group, method.Name), out ArgAttrs argAttrs))
                            {
                                continue;
                            }

                            foreach (var item3 in argAttrs.Args)
                            {
                                SetParameterAttribute(item3, configAttribute);
                            }
                        }
                        break;
                    //parameter
                    default:
                        foreach (var configAttribute in item2)
                        {
                            if (!method.ArgAttrs.TryGetValue(Bind.GetCommandGroup(configAttribute.Group, method.Name), out ArgAttrs argAttrs))
                            {
                                continue;
                            }

                            var parameter = argAttrs.Args.FirstOrDefault(c => c.Name.Equals(item2.Key, System.StringComparison.CurrentCultureIgnoreCase));

                            if (default(Args).Equals(parameter))
                            {
                                continue;
                            }

                            SetParameterAttribute(parameter, configAttribute);
                        }
                        break;
                }
            }
        }

        static void SetParameterAttribute(Args args, ConfigJson.ConfigAttribute configAttribute)
        {
            switch (configAttribute.Target.Property)
            {
                case "":
                    {
                        //parameter
                        var attribute = args.ArgAttr.FirstOrDefault(c => c.TypeFullName.Equals(configAttribute.Attribute, System.StringComparison.CurrentCultureIgnoreCase));

                        if (null == attribute)
                        {
                            break;
                        }

                        foreach (var member in configAttribute.Member)
                        {
                            attribute[member.Key] = member.Value;
                        }
                    }
                    break;
                // any property
                case Any:
                    {
                        foreach (var item in args.ArgAttrChild)
                        {
                            var attribute = item.ArgAttr.FirstOrDefault(c => c.TypeFullName.Equals(configAttribute.Attribute, System.StringComparison.CurrentCultureIgnoreCase));

                            if (null == attribute)
                            {
                                continue;
                            }

                            foreach (var member in configAttribute.Member)
                            {
                                attribute[member.Key] = member.Value;
                            }
                        }
                    }
                    break;
                //property
                default:
                    {
                        var property = args.ArgAttrChild.FirstOrDefault(c => c.Name.Equals(configAttribute.Target.Property, System.StringComparison.CurrentCultureIgnoreCase));

                        if (default(Args).Equals(property))
                        {
                            break;
                        }

                        var attribute = property.ArgAttr.FirstOrDefault(c => c.TypeFullName.Equals(configAttribute.Attribute, System.StringComparison.CurrentCultureIgnoreCase));

                        if (null == attribute)
                        {
                            break;
                        }

                        foreach (var member in configAttribute.Member)
                        {
                            attribute[member.Key] = member.Value;
                        }
                    }
                    break;
            }
        }

        #endregion
#endif
        readonly System.Collections.Concurrent.ConcurrentDictionary<string, MetaData> metaData;
        public System.Collections.Concurrent.ConcurrentDictionary<string, MetaData> MetaData { get => metaData; }

        readonly System.Collections.Generic.List<Attributes.AttributeBase> attributes;
        public System.Collections.Generic.IReadOnlyList<Attributes.AttributeBase> Attributes { get => attributes; }

        readonly Attributes.InfoAttribute info;
        public Attributes.InfoAttribute Info { get => info; }

        readonly bool enableWatcher;
        public bool EnableWatcher { get => enableWatcher; }

        readonly TypeInfo resultType;
        public TypeInfo ResultType { get => resultType; }

        readonly System.Func<Auth.IToken> token;
        public System.Func<Auth.IToken> Token { get => token; }

        readonly TypeInfo requestType;
        public TypeInfo RequestType { get => requestType; }

        readonly dynamic requestDefault;
        public dynamic RequestDefault { get => requestDefault; }

        readonly System.Collections.Concurrent.ConcurrentDictionary<string, Attributes.RouteAttribute> routes;
        public System.Collections.Concurrent.ConcurrentDictionary<string, Attributes.RouteAttribute> Routes { get => routes; }

        //readonly System.Collections.Generic.List<Attributes.RouteAttribute> route;
        //public System.Collections.Generic.IReadOnlyList<Attributes.RouteAttribute> Route { get => route; }

        //readonly SocketAttribute socket;
        //public SocketAttribute Socket { get => socket; }

        /*
        public bool EnableWatcher
        {
            get
            {
                return null == CfgChanged ? false : GetCfgChanged().Any(c => ((Config)c).Identity.Equals(this.Identity));
            }
            internal set
            {
                lock (CfgWatcher)
                {
                    if (value)
                    {
                        if (null == CfgChanged || !GetCfgChanged().Any(c => ((Config)c).Identity.Equals(this.Identity)))
                        {
                            CfgChanged += Changed;
                        }

                        if (!CfgWatcher.EnableRaisingEvents)
                        {
                            CfgWatcher.EnableRaisingEvents = true;
                        }
                    }
                    else
                    {
                        if (null == CfgChanged) { return; }

                        CfgChanged -= Changed;

                        if (0 == CfgChanged.GetInvocationList().Length)
                        {
                            if (CfgWatcher.EnableRaisingEvents)
                            {
                                CfgWatcher.EnableRaisingEvents = false;
                            }
                        }
                    }
                }
            }
        }
        */

        //public void Dispose()
        //{
        //    this.attributes.Clear();

        //    if (this.enableWatcher)
        //    {
        //        CfgChanged -= Changed;

        //        var locked = System.Threading.Interlocked.Decrement(ref interlocked);

        //        if (0 == locked)
        //        {
        //            CfgWatcher.EnableRaisingEvents = false;
        //        }
        //    }
        //}
    }
}
