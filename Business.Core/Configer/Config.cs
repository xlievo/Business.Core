namespace Business.Configer
{
    using Business.Attributes;
    using System.Linq;

    public class Config : IConfiguration
    {
        /// <summary>
        /// *.*
        /// </summary>
        const string ANY = "*.*";

        public ConfigAttribute Info { get; set; }

        static readonly string CfgPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Business.config");
        static System.DateTime cfgLastTime = System.DateTime.MinValue;
        static readonly System.IO.FileSystemWatcher CfgWatcher = new System.IO.FileSystemWatcher(System.IO.Path.GetDirectoryName(CfgPath)) { InternalBufferSize = 4096, Filter = System.IO.Path.GetFileName(CfgPath), NotifyFilter = System.IO.NotifyFilters.LastWrite | System.IO.NotifyFilters.FileName };
        static System.Action<System.Collections.Generic.IEnumerable<LoggerItem>, System.Collections.Generic.IEnumerable<AttributeItem>> CfgChanged;

        public static ConfigSection Instance = GetSection();

        static Config()
        {
            CfgWatcher.Renamed += Changed;
            CfgWatcher.Changed += Changed;
        }

        static ConfigSection GetSection()
        {
            if (System.IO.File.Exists(CfgPath))
            {
                try
                {
                    return System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(new System.Configuration.ExeConfigurationFileMap() { ExeConfigFilename = CfgPath }, System.Configuration.ConfigurationUserLevel.None).GetSection("Business") as ConfigSection;
                }
                catch// (System.Exception ex)
                {
                    //System.Console.WriteLine(ex);
                    return null;
                }
            }

            return null;
        }

        public static System.Tuple<System.Collections.Generic.IEnumerable<IGrouping<string, LoggerItem>>, System.Collections.Generic.IEnumerable<IGrouping<string, AttributeItem>>> GetGroup(ConfigSection cfgSection)
        {
            var loggerSections = cfgSection.Logger.Cast<LoggerItem>().Where(c => !System.String.IsNullOrWhiteSpace(c.Business) && !System.String.IsNullOrWhiteSpace(c.Method) && c.Type.HasValue).Distinct(Extensions.Equality<LoggerItem>.CreateComparer(c => new { c.Business, c.Method, c.Type })).GroupBy(g => g.Business);

            var attributeSections = cfgSection.Attribute.Cast<AttributeItem>().Where(c => !System.String.IsNullOrWhiteSpace(c.Business) && !System.String.IsNullOrWhiteSpace(c.Method) && !System.String.IsNullOrWhiteSpace(c.Argument) && !System.String.IsNullOrWhiteSpace(c.Attribute) && !System.String.IsNullOrWhiteSpace(c.Member)).Distinct().GroupBy(g => g.Business);

            return System.Tuple.Create(loggerSections, attributeSections);
        }

        static void Changed(object sender, System.IO.FileSystemEventArgs e)
        {
            if (!e.FullPath.Equals(CfgPath, System.StringComparison.InvariantCultureIgnoreCase)) { return; }

            var lastTime = System.IO.File.GetLastWriteTime(e.FullPath);

            if (0 <= cfgLastTime.CompareTo(lastTime)) { return; }

            cfgLastTime = lastTime;

            if (null != CfgChanged)
            {
                var cfgSection = GetSection();
                if (null != cfgSection)
                {
                    var sections = GetGroup(cfgSection);
                    var loggerSections = sections.Item1;
                    var attributeSections = sections.Item2;

                    var list = CfgChanged.GetInvocationList();

                    foreach (var del in list)
                    {
                        var loggerGroup = loggerSections.FirstOrDefault(c => ((Config)del.Target).fullName.Equals(c.Key));
                        var attributeGroup = attributeSections.FirstOrDefault(c => ((Config)del.Target).fullName.Equals(c.Key));

                        if (null != loggerGroup || null != attributeGroup)
                        {
                            ((System.Action<System.Collections.Generic.IEnumerable<LoggerItem>, System.Collections.Generic.IEnumerable<AttributeItem>>)del)(loggerGroup, attributeGroup);
                        }
                    }

                    //CfgChanged(sections);
                }
            }
        }

        readonly System.Collections.Concurrent.ConcurrentDictionary<string, MetaData> metaData;

        readonly System.Guid guid = System.Guid.NewGuid();

        internal readonly string fullName;

        public Config(System.Collections.Concurrent.ConcurrentDictionary<string, MetaData> metaData, string fullName)
        {
            this.metaData = metaData;
            this.fullName = fullName;
        }

        void Changed(System.Collections.Generic.IEnumerable<LoggerItem> loggerGroup, System.Collections.Generic.IEnumerable<AttributeItem> attributeGroup)
        {
            if (null != loggerGroup)
            {
                Logger(metaData, loggerGroup);
            }

            if (null != attributeGroup)
            {
                Attribute(metaData, attributeGroup);
            }
        }

        public static void Logger(System.Collections.Concurrent.ConcurrentDictionary<string, MetaData> metaData, System.Collections.Generic.IEnumerable<LoggerItem> group)
        {
            var all = group.Where(c => ANY == c.Method);
            var loggers = group.Where(g => metaData.ContainsKey(g.Method));

            foreach (var item in all)
            {
                foreach (var meta in metaData)
                {
                    if (loggers.Any(c => c.Method.Equals(meta.Key) && c.Type.Value.Equals(item.Type.Value))) { continue; }

                    SetMetaLogger(meta.Value.MetaLogger, item.Type.Value, item.CanWrite, item.CanValue.HasValue ? item.CanValue.Value : LoggerAttribute.ValueMode.No, item.CanResult);
                }

                System.Console.WriteLine(System.String.Format("Logger {0} {1} {2}", item.Business, item.Method, item.Type));
            }

            foreach (var item in loggers)
            {
                SetMetaLogger(metaData[item.Method].MetaLogger, item.Type.Value, item.CanWrite, item.CanValue.HasValue ? item.CanValue.Value : LoggerAttribute.ValueMode.No, item.CanResult);

                System.Console.WriteLine(System.String.Format("Logger {0} {1} {2}", item.Business, item.Method, item.Type));
            }
        }

        public bool EnableWatcher
        {
            get
            {
                if (null != CfgChanged)
                {
                    return CfgChanged.GetInvocationList().Any(c => ((Config)c.Target).guid.Equals(this.guid));
                }

                return false;
            }
            set
            {
                lock (CfgWatcher)
                {
                    if (value)
                    {
                        if (null == CfgChanged || !CfgChanged.GetInvocationList().Any(c => ((Config)c.Target).guid.Equals(this.guid)))
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
                        if (null != CfgChanged)
                        {
                            CfgChanged -= Changed;
                        }

                        if (null == CfgChanged || 0 == CfgChanged.GetInvocationList().Length)
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

        public bool Logger(LoggerType type, bool canWrite = false, LoggerAttribute.ValueMode? canValue = null, bool canResult = false, string method = ANY)
        {
            if (System.String.IsNullOrWhiteSpace(method))
            {
                return false;
            }

            if (ANY == method)
            {
                foreach (var meta in this.metaData)
                {
                    SetMetaLogger(meta.Value.MetaLogger, type, canWrite, canValue.HasValue ? canValue.Value : LoggerAttribute.ValueMode.No, canResult);
                }

                return true;
            }
            else
            {
                MetaData meta;
                if (this.metaData.TryGetValue(method, out meta))
                {
                    SetMetaLogger(meta.MetaLogger, type, canWrite, canValue.HasValue ? canValue.Value : LoggerAttribute.ValueMode.No, canResult);

                    return true;
                }
            }

            return false;
        }

        static void SetMetaLogger(MetaLogger metaLogger, LoggerType type, bool canWrite, LoggerAttribute.ValueMode canValue, bool canResult)
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

            MetaData meta;
            if (metaData.TryGetValue(method, out meta))
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

                var attr = arg.ArgAttr.FirstOrDefault(c => c.fullName == attribute);
                if (null == attr) { return false; }

                return attr.MemberSet(member, value);
            }

            return false;
        }
    }
}
