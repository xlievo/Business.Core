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

namespace Business.Attributes //Annotations
{
    using Result;
    using Utils;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Linq;

    #region abstract

    public abstract class AttributeBase : System.Attribute
    {
        #region MetaData

        public struct AttributeAccessor
        {
            public ConcurrentReadOnlyDictionary<string, Accessor> Accessor;

            public object[] ConstructorArgs;
        }

        public static readonly ConcurrentReadOnlyDictionary<string, AttributeAccessor> Accessors = new ConcurrentReadOnlyDictionary<string, AttributeAccessor>();

        internal static void LoadAttributes(System.Type type)
        {
            if (type.IsAbstract || Accessors.ContainsKey(type.FullName)) { return; }

            var member = Accessors.dictionary.GetOrAdd(type.FullName, key => new AttributeAccessor { Accessor = new ConcurrentReadOnlyDictionary<string, Accessor>(), ConstructorArgs = type.GetConstructors()?.FirstOrDefault()?.GetParameters().Select(c => c.HasDefaultValue ? c.DefaultValue : default).ToArray() });

            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Static))
            {
                if (typeof(System.MulticastDelegate).IsAssignableFrom(field.FieldType))
                {
                    continue;
                }

                var accessor = field.GetAccessor();
                if (null == accessor.Getter || null == accessor.Setter) { continue; }
                member.Accessor.dictionary.TryAdd(field.Name, accessor);
            }

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Static))
            {
                if (typeof(System.MulticastDelegate).IsAssignableFrom(property.PropertyType))
                {
                    continue;
                }

                var accessor = property.GetAccessor();
                if (null == accessor.Getter || null == accessor.Setter) { continue; }
                member.Accessor.dictionary.TryAdd(property.Name, accessor);
            }
        }

        #region GetAttributes

        //internal static System.Collections.Generic.List<AttributeBase> GetArgAttr(System.Type type, bool inherit = true)
        //{
        //    var argAttr = new System.Collections.Generic.List<AttributeBase>(type.GetAttributes<AttributeBase>(inherit));
        //    argAttr.Distinct(type.Assembly.GetCustomAttributes<AttributeBase>());//.AddRange(type.Assembly.GetCustomAttributes<AttributeBase>());
        //    return argAttr;
        //}
        internal static System.Collections.Generic.List<AttributeBase> GetTopAttributes(System.Type classType, bool inherit = true)
        {
            var classAttr = classType.GetAttributes<AttributeBase>(inherit);

            foreach (var item in classAttr)
            {
                item.Declaring = DeclaringType.Class;
            }

            var assemblyAttr = classType.Assembly.GetCustomAttributes<AttributeBase>();

            foreach (var item in assemblyAttr)
            {
                item.Declaring = DeclaringType.Assembly;
            }

            var attributes = new System.Collections.Generic.List<AttributeBase>(classAttr).Distinct(assemblyAttr);

            return attributes;
        }

        internal static System.Collections.Generic.List<AttributeBase> GetAttributes(MemberInfo member, bool inherit = true)
        {
            var attributes = member.GetAttributes<AttributeBase>(inherit).ToList();
            attributes.ForEach(c => c.Declaring = DeclaringType.Method);
            return attributes;
        }

        internal static System.Collections.Generic.List<AttributeBase> GetAttributes(ParameterInfo member, System.Type type)
        {
            var attributes = new System.Collections.Generic.List<AttributeBase>(member.GetAttributes<AttributeBase>());
            attributes.AddRange(member.ParameterType.GetAttributes<AttributeBase>());
            attributes.AddRange(type.GetAttributes<AttributeBase>());
            attributes.ForEach(c => c.Declaring = DeclaringType.Parameter);
            return attributes;
        }
        internal static System.Collections.Generic.List<ArgumentAttribute> GetCollectionAttributes(System.Type type)
        {
            var attributes = new System.Collections.Generic.List<ArgumentAttribute>(type.GetAttributes<ArgumentAttribute>());
            attributes.ForEach(c =>
            {
                c.Declaring = DeclaringType.Parameter;
                c.Meta.hasCollection = true;
            });
            return attributes;
        }
        internal static Attribute GetAttribute<Attribute>(ParameterInfo member, System.Type type) where Attribute : AttributeBase
        {
            var attribute = member.GetAttribute<Attribute>() ?? type.GetAttribute<Attribute>();
            if (null != attribute)
            {
                attribute.Declaring = DeclaringType.Parameter;
            }
            return attribute;
        }
        internal static System.Collections.Generic.List<AttributeBase> GetAttributes(MemberInfo member, System.Type type)
        {
            var attributes = new System.Collections.Generic.List<AttributeBase>();
            attributes.AddRange(member.GetAttributes<AttributeBase>());
            attributes.AddRange(type.GetAttributes<AttributeBase>());
            attributes.ForEach(c => c.Declaring = DeclaringType.Children);
            return attributes;
        }

        internal static System.Collections.Generic.List<Attribute> GetAttributes<Attribute>(System.Type type, DeclaringType source, System.Collections.Generic.IEqualityComparer<Attribute> comparer = null) where Attribute : AttributeBase
        {
            var attributes = type.GetAttributes<Attribute>().Distinct(comparer).ToList();
            attributes.ForEach(c => c.Declaring = source);
            return attributes;
        }

        internal static bool ExistAttr<Attribute>(ParameterInfo member, System.Type type) where Attribute : System.Attribute
        {
            return member.Exists<Attribute>() || type.Exists<Attribute>();
        }

        #endregion

        /*
        static AttributeBase()
        {
            MetaData = new ConcurrentReadOnlyDictionary<string, ConcurrentReadOnlyDictionary<string, Accessor>>();

#if !Mobile
            var ass = System.AppDomain.CurrentDomain.GetAssemblies().Where(c => !c.IsDynamic);

            foreach (var assembly in ass)
            {
                try
                {
                    var types = assembly.GetExportedTypes();

                    foreach (var type in types)
                    {
                        var typeInfo = type.GetTypeInfo();

                        if (typeInfo.IsSubclassOf(typeof(AttributeBase)) && !typeInfo.IsAbstract)
                        {
                            var member = new ConcurrentReadOnlyDictionary<string, Accessor>();

                            foreach (var field in typeInfo.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
                            {
                                var accessor = field.GetAccessor();
                                if (null == accessor.Getter || null == accessor.Setter) { continue; }
                                member.dictionary.TryAdd(field.Name, accessor);
                            }

                            foreach (var property in typeInfo.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
                            {
                                var accessor = property.GetAccessor();
                                if (null == accessor.Getter || null == accessor.Setter) { continue; }
                                member.dictionary.TryAdd(property.Name, accessor);
                            }

                            MetaData.dictionary.GetOrAdd(typeInfo.FullName, member);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    System.Console.WriteLine(assembly.Location);
                    ex.ExceptionWrite(console: true);
                }
            }
            */

        #endregion

        /// <summary>
        /// Accessor
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public object this[string member]
        {
            get
            {
                if (!Accessors.TryGetValue(this.Type.FullName, out AttributeAccessor meta) || !meta.Accessor.TryGetValue(member, out Accessor accessor)) { return null; }

                return accessor.Getter(this);
            }
            set
            {
                if (!Accessors.TryGetValue(this.Type.FullName, out AttributeAccessor meta) || !meta.Accessor.TryGetValue(member, out Accessor accessor)) { return; }

                try
                {
                    var value2 = Help.ChangeType(value, accessor.Type);
                    if (object.Equals(value2, accessor.Getter(this))) { return; }

                    accessor.Setter(this, value2);
                }
                catch { }
            }
        }

        //public virtual bool MemberSet(System.Type type, string value, out object outValue)
        //{
        //    outValue = null;

        //    if (type.IsEnum)
        //    {
        //        if (System.String.IsNullOrWhiteSpace(value)) { return false; }

        //        var enums = System.Enum.GetValues(type).Cast<System.Enum>();
        //        var enumValue = enums.FirstOrDefault(c => value.Equals(c.ToString(), System.StringComparison.CurrentCultureIgnoreCase));
        //        if (null != enumValue)
        //        {
        //            outValue = enumValue;
        //            return true;
        //        }
        //        return false;
        //    }
        //    else
        //    {
        //        switch (type.FullName)
        //        {
        //            case "System.String":
        //                outValue = value;
        //                return true;
        //            case "System.Int16":
        //                if (System.Int16.TryParse(value, out short value2))
        //                {
        //                    outValue = value2;
        //                    return true;
        //                }
        //                return false;
        //            case "System.Int32":
        //                if (System.Int32.TryParse(value, out int value3))
        //                {
        //                    outValue = value3;
        //                    return true;
        //                }
        //                return false;
        //            case "System.Int64":
        //                if (System.Int64.TryParse(value, out long value4))
        //                {
        //                    outValue = value4;
        //                    return true;
        //                }
        //                return false;
        //            case "System.Decimal":
        //                if (System.Decimal.TryParse(value, out decimal value5))
        //                {
        //                    outValue = value5;
        //                    return true;
        //                }
        //                return false;
        //            case "System.Double":
        //                if (System.Double.TryParse(value, out double value6))
        //                {
        //                    outValue = value6;
        //                    return true;
        //                }
        //                return false;
        //            default:
        //                value.ChangeType(type);
        //                System.Convert.ChangeType(value, type);
        //                return false;
        //        }
        //    }
        //}
        /*
        public bool MemberSet(string member, dynamic value)
        {
            if (!MetaData.TryGetValue(this.type.FullName, out System.Collections.Generic.IReadOnlyDictionary<string, Accessor> meta) || !meta.TryGetValue(member, out Accessor accessor)) { return false; }

            try
            {
                var value2 = Help.ChangeType(value, accessor.Type);
                if (System.Object.Equals(value2, accessor.Getter(this)))
                {
                    return false;
                }
                accessor.Setter(this, value2);
                return true;
            }
            catch
            {
                return false;
            }
        }
        */

        public AttributeBase()//bool config = false
        {
            this.Type = this.GetType();
            //this.AllowMultiple = this.Type.GetTypeInfo().GetAttribute<System.AttributeUsageAttribute>()?.AllowMultiple ?? false;
            //this.basePath = this.Type.FullName.Replace('+', '.');

            //var usage = this.Type.GetTypeInfo().GetAttribute<System.AttributeUsageAttribute>();
            //if (null != usage)
            //{
            //    this.AllowMultiple = usage?.AllowMultiple;
            //    this.Inherited = usage.Inherited;
            //}

            LoadAttributes(this.Type);

            //this.config = config;
        }

        #region

        /// <summary>
        /// Source types that declare this feature
        /// </summary>
        public enum DeclaringType
        {
            Assembly,
            Class,
            Method,
            Parameter,
            Children,
        }

        /// <summary>
        /// Gets the fully qualified type name, including the namespace but not the assembly
        /// </summary>
        public System.Type Type { get; private set; }

        ///// <summary>
        ///// Is it possible to specify attributes for multiple instances for a program element
        ///// </summary>
        //public bool AllowMultiple { get; private set; }

        /// <summary>
        /// Declare the source of this feature
        /// </summary>
        public DeclaringType Declaring { get; internal set; }

        ///// <summary>
        ///// Determines whether the attributes indicated by the derived class and the overridden member are inherited
        ///// </summary>
        //public bool Inherited { get; private set; }

        //bool enable = true;
        //public bool Enable { get => enable; set => enable = value; }

        //readonly bool config;
        //public bool Config { get => config; }

        //readonly string basePath;
        //public virtual string BasePath { get => basePath; }

        //readonly bool restart;
        //public bool Restart { get => restart; }

        ///// <summary>
        ///// Used for the command group
        ///// </summary>
        //public virtual string Group { get; set; }

        //public virtual string GetKey(string space = "->") => string.Format("{1}{0}{2}", space, this.Group, this.BasePath);

        #endregion

        /// <summary>
        /// Depth Clone
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual T Clone<T>() where T : AttributeBase
        {
            if (this.Type.IsAbstract) { return default; }

            return Force.DeepCloner.DeepClonerExtensions.DeepClone(this) as T;

            /*
            if (!Accessors.TryGetValue(this.Type.FullName, out AttributeAccessor meta))
            {
                return default;
            }

            var attr = System.Activator.CreateInstance(this.Type, meta.ConstructorArgs);

            foreach (var item in meta.Accessor.Values)
            {
                item.Setter(attr, item.Getter(this));
            }

            return attr as T;
            */
        }

        public virtual string Replace(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || !Accessors.TryGetValue(this.Type.FullName, out AttributeAccessor meta))
            {
                return value;
            }

            var sb = new System.Text.StringBuilder(value);

            foreach (var item in meta.Accessor)
            {
                var member = $"{{{item.Key}}}";

                if (-1 < value.IndexOf(member))
                {
                    sb = sb.Replace(member, System.Convert.ToString(item.Value.Getter(this)));
                }
            }

            return sb.ToString();
        }
    }

    #endregion

    #region

    public enum IgnoreMode
    {
        Method = 2,
        Arg = 4,
        ArgChild = 8,
        BusinessArg = 16,
    }

    /* 1:Ignore method 2:Ignore document 3:Ignore Business ArgAttr */
    /// <summary>
    /// The Method and Property needs to be ignored and will not be a proxy
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Assembly | System.AttributeTargets.Class | System.AttributeTargets.Method | System.AttributeTargets.Property | System.AttributeTargets.Parameter | System.AttributeTargets.Class | System.AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
    public class Ignore : GropuAttribute
    {
        public Ignore(IgnoreMode mode = IgnoreMode.Method)
        {
            this.Mode = mode;
        }

        public IgnoreMode Mode { get; private set; }

        //public string Group { get; set; }

        //public bool Contains(IgnoreMode mode) => 0 != (this.Mode & mode);

        public override string GroupKey(string space = "->") => $"{base.GroupKey(space)}{space}{this.Mode.GetName()}";
    }

    /// <summary>
    /// Token
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Parameter | System.AttributeTargets.Class | System.AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public sealed class TokenAttribute : System.Attribute { }
    //[System.AttributeUsage(System.AttributeTargets.Parameter | System.AttributeTargets.Class | System.AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    //public sealed class HttpFileAttribute : System.Attribute { }
    //[System.AttributeUsage(System.AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    //public class HttpRequestAttribute : System.Attribute { }


    //[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    //public sealed class EnableWatcherAttribute : System.Attribute { }

    /// <summary>
    /// Info
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class Info : AttributeBase
    {
        public Info(string businessName = null)
        {
            this.BusinessName = businessName;
            //this.ConfigFileName = System.IO.Path.Combine(Help.BaseDirectory, configFileName);
        }

        public string BusinessName { get; internal set; }

        /// <summary>
        /// Default
        /// </summary>
        public string CommandGroupDefault { get; set; } = "Default";

        //public string ConfigFileName { get; internal set; }
    }

    /*
    /// <summary>
    /// Socket
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = false, Inherited = true)]
    public class SocketAttribute : AttributeBase
    {
        /// <summary>
        /// Socket server running mode
        /// </summary>
        public enum SocketMode
        {
            /// <summary>
            /// Tcp mode
            /// </summary>
            Tcp = 0,
            /// <summary>
            /// Udp mode
            /// </summary>
            Udp = 1,
        }

        #region Default

        public const string DefaultDescription = "Socket Service";

        /// <summary>
        /// The default ip
        /// </summary>
        public const string DefaultIp = "127.0.0.1";

        /// <summary>
        /// The default port
        /// </summary>
        public const int DefaultPort = 8200;

        /// <summary>
        /// The default socket mode
        /// </summary>
        public const SocketMode DefaultMode = SocketMode.Tcp;

        /// <summary>
        /// The default socket mode
        /// </summary>
        public const string DefaultSecurity = "None";

        /// <summary>
        /// The default security
        /// </summary>
        public const bool DefaultClearIdleSession = true;

        /// <summary>
        /// Default clear idle session interval
        /// </summary>
        public const int DefaultClearIdleSessionInterval = 120;
        /// <summary>
        /// Default idle session timeout
        /// </summary>
        public const int DefaultIdleSessionTimeOut = 300;
        /// <summary>
        /// The default keep alive interval
        /// </summary>
        public const int DefaultKeepAliveInterval = 60;
        /// <summary>
        /// The default keep alive time
        /// </summary>
        public const int DefaultKeepAliveTime = 600;
        ///// <summary>
        ///// The default listen backlog
        ///// </summary>
        //public const int DefaultListenBacklog = 100;
        /// <summary>
        /// Default MaxConnectionNumber
        /// </summary>
        public const int DefaultMaxConnectionNumber = 100;
        /// <summary>
        /// Default MaxRequestLength
        /// </summary>
        public const int DefaultMaxRequestLength = 1024;
        /// <summary>
        /// Default ReceiveBufferSize
        /// </summary>
        public const int DefaultReceiveBufferSize = 4096;
        /// <summary>
        /// The default send buffer size
        /// </summary>
        public const int DefaultSendBufferSize = 2048;
        /// <summary>
        /// Default sending queue size
        /// </summary>
        public const int DefaultSendingQueueSize = 5;
        /// <summary>
        /// Default send timeout value, in milliseconds
        /// </summary>
        public const int DefaultSendTimeout = 5000;
        /// <summary>
        /// The default session snapshot interval
        /// </summary>
        public const int DefaultSessionSnapshotInterval = 5;

        #endregion

        /// <summary>
        /// Initializes a new instance of the socket configuration class
        /// </summary>
        /// <param name="ip">Gets the ip of listener</param>
        /// <param name="port">Gets the port of listener</param>
        /// <param name="mode">Gets/sets the mode.</param>
        /// <param name="security">Gets/sets the security option, None/Default/Tls/Ssl/...</param>
        /// <param name="clearIdleSession"></param>
        /// <param name="clearIdleSessionInterval">clear idle session interval</param>
        /// <param name="idleSessionTimeOut">idle session timeout</param>
        /// <param name="keepAliveInterval">The keep alive interval</param>
        /// <param name="keepAliveTime">The keep alive time</param>
        /// <param name="maxConnectionNumber">max connection number</param>
        /// <param name="maxRequestLength"></param>
        /// <param name="receiveBufferSize"></param>
        /// <param name="sendBufferSize">The send buffer size</param>
        /// <param name="sendingQueueSize">sending queue size</param>
        /// <param name="sendTimeOut">send timeout value, in milliseconds</param>
        /// <param name="sessionSnapshotInterval">The default session snapshot interval</param>
        /// <param name="description"></param>
        [Newtonsoft.Json.JsonConstructor]
        public SocketAttribute(string ip = DefaultIp, int port = DefaultPort, SocketMode mode = DefaultMode, string security = DefaultSecurity, bool clearIdleSession = true, int clearIdleSessionInterval = DefaultClearIdleSessionInterval, int idleSessionTimeOut = DefaultIdleSessionTimeOut, int keepAliveInterval = DefaultKeepAliveInterval, int keepAliveTime = DefaultKeepAliveTime, int maxConnectionNumber = DefaultMaxConnectionNumber, int maxRequestLength = DefaultMaxRequestLength, int receiveBufferSize = DefaultReceiveBufferSize, int sendBufferSize = DefaultSendBufferSize, int sendingQueueSize = DefaultSendingQueueSize, int sendTimeOut = DefaultSendTimeout, int sessionSnapshotInterval = DefaultSessionSnapshotInterval, string description = DefaultDescription)
        {
            this.Ip = ip;
            this.Port = port;
            this.Mode = mode;
            this.Security = security;
            this.ClearIdleSession = clearIdleSession;
            this.ClearIdleSessionInterval = clearIdleSessionInterval;
            this.IdleSessionTimeOut = idleSessionTimeOut;
            this.KeepAliveInterval = keepAliveInterval;
            this.KeepAliveTime = keepAliveTime;
            this.MaxConnectionNumber = maxConnectionNumber;
            this.MaxRequestLength = maxRequestLength;
            this.ReceiveBufferSize = receiveBufferSize;
            this.SendBufferSize = sendBufferSize;
            this.SendingQueueSize = sendingQueueSize;
            this.SendTimeOut = sendTimeOut;
            this.SessionSnapshotInterval = sessionSnapshotInterval;
            this.Description = description;
        }

        /// <summary>
        /// Gets the ip of listener
        /// </summary>
        public string Ip { get; internal set; }

        /// <summary>
        /// Gets the port of listener
        /// </summary>
        public int Port { get; internal set; }

        /// <summary>
        /// Gets/sets the mode.
        /// </summary>
        public SocketMode Mode { get; internal set; }

        /// <summary>
        /// Gets/sets the security option, None/Default/Tls/Ssl/...
        /// </summary>
        public string Security { get; internal set; }

        /// <summary>
        /// Gets/sets a value indicating whether clear idle session.
        /// </summary>
        public bool ClearIdleSession { get; internal set; }

        /// <summary>
        /// Gets/sets the clear idle session interval, in seconds.
        /// </summary>
        public int ClearIdleSessionInterval { get; internal set; }

        /// <summary>
        /// Gets/sets the idle session timeout time length, in seconds.
        /// </summary>
        public int IdleSessionTimeOut { get; internal set; }

        /// <summary>
        /// Gets the max connection number.
        /// </summary>
        public int MaxConnectionNumber { get; internal set; }

        /// <summary>
        /// Gets/sets the length of the max request.
        /// </summary>
        public int MaxRequestLength { get; internal set; }

        /// <summary>
        /// Gets the size of the receive buffer.
        /// </summary>
        public int ReceiveBufferSize { get; internal set; }

        /// <summary>
        /// Gets/sets the keep alive interval, in seconds.
        /// </summary>
        public int KeepAliveInterval { get; internal set; }

        /// <summary>
        /// Gets/sets the start keep alive time, in seconds
        /// </summary>
        public int KeepAliveTime { get; internal set; }

        /// <summary>
        /// Gets the size of the send buffer.
        /// </summary>
        public int SendBufferSize { get; internal set; }

        /// <summary>
        /// Gets/sets the size of the sending queue.
        /// </summary>
        public int SendingQueueSize { get; internal set; }

        /// <summary>
        /// Gets/sets the send time out.
        /// </summary>
        public int SendTimeOut { get; internal set; }

        /// <summary>
        /// Gets/sets the interval to taking snapshot for all live sessions.
        /// </summary>
        public int SessionSnapshotInterval { get; internal set; }

        public string Description { get; internal set; }
    }
    */

    public struct UseEntry
    {
        public UseEntry(object value, params string[] parameterName)
        {
            this.Value = value;

            this.Type = this.Value?.GetType();

            this.ParameterName = parameterName;
        }

        public System.Type Type { get; private set; }

        public object Value { get; private set; }

        public string[] ParameterName { get; private set; }
    }

    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct | System.AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public sealed class UseAttribute : AttributeBase
    {
        public UseAttribute(bool parameterName = false) => this.ParameterName = parameterName;

        public bool ParameterName { get; private set; }
    }

    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct | System.AttributeTargets.Parameter | System.AttributeTargets.Property | System.AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class NickAttribute : AttributeBase
    {
        public NickAttribute(string nick) => this.Nick = nick;

        /// <summary>
        /// Amicable name
        /// </summary>
        public string Nick { get; private set; }
    }

    [System.AttributeUsage(System.AttributeTargets.Assembly | System.AttributeTargets.Class | System.AttributeTargets.Method | System.AttributeTargets.Struct | System.AttributeTargets.Parameter, AllowMultiple = true, Inherited = true)]
    public class LoggerAttribute : GropuAttribute
    {
        //[Newtonsoft.Json.JsonConstructor]
        public LoggerAttribute(LoggerType logType = LoggerType.All, bool canWrite = true)
        {
            this.LogType = logType;
            this.CanWrite = canWrite;
        }

        /// <summary>
        /// Record type
        /// </summary>
        public LoggerType LogType { get; private set; }
        /// <summary>
        /// Allow record
        /// </summary>
        public bool CanWrite { get; set; }

        /// <summary>
        /// Allowed to return to parameters
        /// </summary>
        public LoggerValueMode CanValue { get; set; }

        /// <summary>
        /// Allowed to return to results
        /// </summary>
        public bool CanResult { get; set; } = true;

        public LoggerAttribute SetType(LoggerType logType)
        {
            this.LogType = logType;
            return this;
        }

        //public static System.Collections.Generic.IEqualityComparer<LoggerAttribute> Comparer = Equality<LoggerAttribute>.CreateComparer(c => c.LogType);

        //public override T Clone<T>() => new LoggerAttribute(this.LogType) { Group = this.Group, CanResult = this.CanResult, CanValue = this.CanValue, CanWrite = this.CanWrite } as T;

        //public override string Key(string space = "->") => string.Format("{1}{0}{2}", space, this.Mode.GetName(), this.Group.Trim());
        public override string GroupKey(string space = "->") => $"{ base.GroupKey(space)}{space}{this.LogType.GetName()}";

        public override string ToString() => $"{this.Type.Name} {this.LogType.ToString()}";
    }

    /// <summary>
    /// Record parameter model
    /// </summary>
    public enum LoggerValueMode
    {
        /// <summary>
        /// Allow selective recording of some parameters
        /// </summary>
        All = 0,
        /// <summary>
        /// All parameter Records
        /// </summary>
        Select = 1,
        /// <summary>
        /// No records
        /// </summary>
        No = 2,
    }

    #endregion

    public abstract class GropuAttribute : AttributeBase
    {
        string group = string.Empty;

        /// <summary>
        /// Used for the command group
        /// </summary>
        public string Group { get => group; set => group = value?.Trim() ?? string.Empty; }

        public virtual string GroupKey(string space = "->") => $"{this.Type.FullName}{space}{this.Group.Trim()}";

        public static System.Collections.Generic.IEqualityComparer<GropuAttribute> Comparer = Equality<GropuAttribute>.CreateComparer(c => c.GroupKey(), System.StringComparer.CurrentCultureIgnoreCase);
    }

    /// <summary>
    /// Base class for all attributes that apply to parameters
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Assembly | System.AttributeTargets.Method | System.AttributeTargets.Class | System.AttributeTargets.Struct | System.AttributeTargets.Property | System.AttributeTargets.Field | System.AttributeTargets.Parameter, AllowMultiple = true, Inherited = true)]
    public abstract class ArgumentAttribute : GropuAttribute
    {
        public class MetaData
        {
            internal System.Type resultType;

            internal bool hasCollection;

            public dynamic Business { get; internal set; }

            public string Method { get; internal set; }

            public string Member { get; internal set; }

            public System.Type MemberType { get; internal set; }

            public bool HasProcesIArg { get; internal set; }
        }

        public ArgumentAttribute(int state, string message = null)
        {
            this.State = state;
            this.Message = message;
            //this.CanNull = canNull;
            this.Meta = new MetaData();

            this.BindAfter += () =>
            {
                if (!string.IsNullOrWhiteSpace(this.Message))
                {
                    this.Message = this.Replace(this.Message);
                }

                if (!string.IsNullOrWhiteSpace(this.Description))
                {
                    this.Description = this.Replace(this.Description);
                }
            };
        }

        public System.Action BindAfter { get; set; }

        public MetaData Meta { get; }

        /// <summary>
        /// By checking the Allow null value, Default to true
        /// </summary>
        public bool CanNull { get; set; } = true;

        int state;
        /// <summary>
        /// Used to return state
        /// </summary>
        public virtual int State { get => state; set => state = value.ConvertErrorState(); }

        /// <summary>
        /// Used to return error messages
        /// </summary>
        public string Message { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// Amicable name
        /// </summary>
        public string Nick { get; set; }

        /// <summary>
        /// Start processing the Parameter object, By this.ResultCreate() method returns
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual async ValueTask<IResult> Proces(dynamic value) => this.ResultCreate<dynamic>(value);

        /// <summary>
        /// Start processing the Parameter object, By this.ResultCreate() method returns
        /// </summary>
        /// <param name="value"></param>
        /// <param name="iArg"></param>
        /// <returns></returns>
        public virtual async ValueTask<IResult> Proces(dynamic value, IArg arg) => this.ResultCreate<dynamic>(value);

        #region Result

        /// <summary>
        /// Used to create the Proces() method returns object
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public IResult ResultCreate(int state) => ResultFactory.ResultCreate(Meta.resultType, state);

        /// <summary>
        /// Used to create the Proces() method returns object
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public IResult ResultCreate(int state = 1, string message = null) => ResultFactory.ResultCreate(Meta.resultType, state, message);

        /// <summary>
        /// Used to create the Proces() method returns object
        /// </summary>
        /// <typeparam name="Data"></typeparam>
        /// <param name="data"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public IResult ResultCreate<Data>(Data data, string message = null, int state = 1) => ResultFactory.ResultCreate(Meta.resultType, data, message, state);

        /// <summary>
        /// Used to create the Proces() method returns object
        /// </summary>
        /// <typeparam name="Data"></typeparam>
        /// <param name="data"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public IResult ResultCreate(object data, string message = null, int state = 1) => ResultFactory.ResultCreate(Meta.resultType, data, message, state);

        #endregion

        public static IResult CheckNull(ArgumentAttribute attribute, dynamic value)
        {
            if (object.Equals(null, value))
            {
                if (attribute.CanNull)
                {
                    return attribute.ResultCreate();
                }
                else
                {
                    return attribute.ResultCreate(attribute.State, attribute.Message ?? $"argument \"{attribute.Nick}\" can not null.");
                }
            }

            return attribute.ResultCreate(data: value);
        }
    }

    /// <summary>
    /// Command attribute on a method, for multiple sources to invoke the method
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Assembly | System.AttributeTargets.Class | System.AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class CommandAttribute : GropuAttribute
    {
        public CommandAttribute(string onlyName = null) { this.OnlyName = onlyName; }

        public string Key { get; internal set; }

        public string OnlyName { get; set; }

        public bool IgnoreBusinessArg { get; set; }

        public override string GroupKey(string space = "->") => $"{base.GroupKey(space)}{space}{this.OnlyName}";

        public override string ToString() => $"{this.Type.Name} {Group} {OnlyName}";
    }

    #region

    public class CheckNullAttribute : ArgumentAttribute
    {
        public CheckNullAttribute(int state = -800, string message = null) : base(state, message) => this.CanNull = false;

        public override async ValueTask<IResult> Proces(dynamic value)
        {
            if (typeof(string).Equals(this.Meta.MemberType))
            {
                if (string.IsNullOrEmpty(value))
                {
                    return this.ResultCreate(State, Message ?? $"argument \"{this.Nick}\" can not null.");
                }
            }
            else if (object.Equals(null, value))
            {
                return this.ResultCreate(State, Message ?? $"argument \"{ this.Nick}\" can not null.");
            }

            return this.ResultCreate();
        }
    }

    public class SizeAttribute : ArgumentAttribute
    {
        public SizeAttribute(int state = -801, string message = null) : base(state, message)
        {
            this.BindAfter += () =>
            {
                if (!string.IsNullOrWhiteSpace(this.MinMsg))
                {
                    this.MinMsg = this.Replace(this.MinMsg);
                }

                if (!string.IsNullOrWhiteSpace(this.MaxMsg))
                {
                    this.MaxMsg = this.Replace(this.MaxMsg);
                }
            };
        }

        public object Min { get; set; }
        public object Max { get; set; }

        public string MinMsg { get; set; } = "argument \"{Nick}\" minimum range {Min}.";
        public string MaxMsg { get; set; } = "argument \"{Nick}\" maximum range {Max}.";

        public override async ValueTask<IResult> Proces(dynamic value)
        {
            var result = CheckNull(this, value);
            if (!result.HasData) { return result; }

            var type = System.Nullable.GetUnderlyingType(this.Meta.MemberType) ?? this.Meta.MemberType;

            string msg = null;

            switch (type.FullName)
            {
                case "System.String":
                    {
                        var ags = System.Convert.ToString(value).Trim();
                        if (null != Min && Help.ChangeType<int>(Min) > ags.Length)
                        {
                            msg = MinMsg; break;
                        }
                        if (null != Max && Help.ChangeType<int>(Max) < ags.Length)
                        {
                            msg = MaxMsg; break;
                        }
                    }
                    break;
                case "System.DateTime":
                    {
                        var ags = System.Convert.ToDateTime(value);
                        if (null != Min && Help.ChangeType<System.DateTime>(Min) > ags)
                        {
                            msg = MinMsg; break;
                        }
                        if (null != Max && Help.ChangeType<System.DateTime>(Max) < ags)
                        {
                            msg = MaxMsg; break;
                        }
                    }
                    break;
                case "System.Int32":
                    {
                        var ags = System.Convert.ToInt32(value);
                        if (null != Min && Help.ChangeType<int>(Min) > ags)
                        {
                            msg = MinMsg; break;
                        }
                        if (null != Max && Help.ChangeType<int>(Max) < ags)
                        {
                            msg = MaxMsg; break;
                        }
                    }
                    break;
                case "System.Int64":
                    {
                        var ags = System.Convert.ToInt64(value);
                        if (null != Min && Help.ChangeType<long>(Min) > ags)
                        {
                            msg = MinMsg; break;
                        }
                        if (null != Max && Help.ChangeType<long>(Max) < ags)
                        {
                            msg = MaxMsg; break;
                        }
                    }
                    break;
                case "System.Decimal":
                    {
                        var ags = System.Convert.ToDecimal(value);
                        if (null != Min && Help.ChangeType<decimal>(Min) > ags)
                        {
                            msg = MinMsg; break;
                        }
                        if (null != Max && Help.ChangeType<decimal>(Max) < ags)
                        {
                            msg = MaxMsg; break;
                        }
                    }
                    break;
                case "System.Double":
                    {
                        var ags = System.Convert.ToDouble(value);
                        if (null != Min && Help.ChangeType<double>(Min) > ags)
                        {
                            msg = MinMsg; break;
                        }
                        if (null != Max && Help.ChangeType<double>(Max) < ags)
                        {
                            msg = MaxMsg; break;
                        }
                    }
                    break;
                default:
                    if (type.IsCollection())
                    {
                        var list = value as System.Collections.ICollection;
                        if (null != Min && Help.ChangeType<int>(Min) > list.Count)
                        {
                            msg = MinMsg; break;
                        }
                        if (null != Max && Help.ChangeType<int>(Max) < list.Count)
                        {
                            msg = MaxMsg; break;
                        }
                    }
                    else
                    {
                        return this.ResultCreate(State, $"argument \"{this.Nick}\" type error");
                    }
                    break;
            }

            //checked error
            if (!string.IsNullOrEmpty(msg))
            {
                return this.ResultCreate(State, Message ?? msg);
            }

            return this.ResultCreate();
        }
    }

    public class ScaleAttribute : ArgumentAttribute
    {
        public ScaleAttribute(int state = -802, string message = null) : base(state, message) { }
        public int Size { get; set; } = 2;

        public override async ValueTask<IResult> Proces(dynamic value)
        {
            var result = CheckNull(this, value);
            if (!result.HasData) { return result; }

            switch (this.Meta.MemberType.FullName)
            {
                case "System.Decimal":
                    return this.ResultCreate(Help.Scale((decimal)value, Size));
                case "System.Double":
                    return this.ResultCreate(Help.Scale((double)value, Size));
                default: return this.ResultCreate(State, Message ?? $"argument \"{this.Nick}\" type error");
            }
        }
    }

    public class CheckEmailAttribute : ArgumentAttribute
    {
        public CheckEmailAttribute(int state = -803, string message = null) : base(state, message) { }

        public override async ValueTask<IResult> Proces(dynamic value)
        {
            var result = CheckNull(this, value);
            if (!result.HasData) { return result; }

            if (!Utils.Help.CheckEmail(value))
            {
                return this.ResultCreate(State, Message ?? $"argument \"{this.Nick}\" email error");
            }
            return this.ResultCreate();
        }
    }

    public class CheckCharAttribute : ArgumentAttribute
    {
        public CheckCharAttribute(int state = -804, string message = null) : base(state, message) { }
        public Help.CheckCharMode Mode { get; set; } = Help.CheckCharMode.All;

        public override async ValueTask<IResult> Proces(dynamic value)
        {
            var result = CheckNull(this, value);
            if (!result.HasData) { return result; }

            if (!Utils.Help.CheckChar(value, Mode))
            {
                return this.ResultCreate(State, Message ?? $"argument \"{this.Nick}\" char verification failed");
            }
            return this.ResultCreate();
        }
    }

    public class MD5Attribute : ArgumentAttribute
    {
        public MD5Attribute(int state = -820, string message = null) : base(state, message) { }

        public System.Text.Encoding Encoding { get; set; }

        public bool HasUpper { get; set; }

        public override async ValueTask<IResult> Proces(dynamic value)
        {
            var result = CheckNull(this, value);
            if (!result.HasData) { return result; }

            return this.ResultCreate(Help.MD5(value, HasUpper, Encoding));
        }
    }

    /// <summary>
    /// AES return to item1=Data and item2=Salt
    /// </summary>
    public class AES : ArgumentAttribute
    {
        /// <summary>
        /// key
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// salt
        /// </summary>
        public string Salt { get; set; }

        public System.Text.Encoding Encoding { get; set; }

        public AES(string key = null, int state = -821, string message = null) : base(state, message) => this.Key = key;

        public override async ValueTask<IResult> Proces(dynamic value)
        {
            var result = CheckNull(this, value);
            if (!result.HasData) { return result; }

            return this.ResultCreate(Help.AES.Encrypt(value, Key, Salt, Encoding));
        }

        public bool Equals(string password, string encryptData, string salt)
        {
            try
            {
                return object.Equals(password, Help.AES.Decrypt(encryptData, Key, salt, Encoding));
            }
            catch
            {
                return false;
            }
        }
    }

    #endregion

    #region Deserialize

    public sealed class ArgumentDefaultAttribute : ArgumentAttribute
    {
        internal ArgumentDefaultAttribute(System.Type resultType, int state = -11, string message = null) : base(state, message) => this.Meta.resultType = resultType;

        public ArgumentDefaultAttribute(int state = -11, string message = null) : base(state, message) { }
    }

    public class JsonArgAttribute : ArgumentAttribute
    {
        public JsonArgAttribute(int state = -12, string message = null) : base(state, message) => this.CanNull = false;

        public Newtonsoft.Json.JsonSerializerSettings Settings { get; set; }

        public override async ValueTask<IResult> Proces(dynamic value)
        {
            var result = CheckNull(this, value);
            if (!result.HasData) { return result; }

            try
            {
                return this.ResultCreate(Newtonsoft.Json.JsonConvert.DeserializeObject(value, this.Meta.MemberType, Settings));
            }
            catch { return this.ResultCreate(State, Message ?? $"Arguments {this.Nick} Json deserialize error"); }
        }
    }
    /*
    public class ProtoBufArgAttribute : ArgumentAttribute
    {
        public ProtoBufArgAttribute(int state = -13, string message = null, bool canNull = false)
            : base(state, message, canNull) { }

        public override async ValueTask<IResult> Proces(dynamic value)
        {
            var result = CheckNull(this, value);
            if (!result.HasData) { return result; }

            try
            {
                using (var stream = new System.IO.MemoryStream(value))
                {
                    return this.ResultCreate(ProtoBuf.Meta.RuntimeTypeModel.Default.Deserialize(stream, null, this.MemberType));
                }
            }
            catch { return this.ResultCreate(State, Message ?? string.Format("Arguments {0} ProtoBuf deserialize error", this.Member)); }
        }
    }
    */
    #endregion
}
