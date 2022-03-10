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

namespace Business.Core.Annotations
{
    using Core;
    using Result;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Utils;

    #region abstract

    /// <summary>
    /// AttributeBase
    /// </summary>
    public abstract class AttributeBase : System.Attribute
    {
        #region MetaData

        /// <summary>
        /// MetaData
        /// </summary>
        public class MetaData
        {
            const string AttributeSign = "Attribute";

            /// <summary>
            /// MetaData
            /// </summary>
            /// <param name="type"></param>
            public MetaData(System.Type type)
            {
                Type = type;
                Name = type.Name.EndsWith(AttributeSign) ? type.Name.Substring(0, type.Name.Length - AttributeSign.Length) : type.Name;
            }

            /// <summary>
            /// Gets the fully qualified type name, including the namespace but not the assembly
            /// </summary>
            public System.Type Type { get; }

            /// <summary>
            /// type name
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// Declare the source of this feature
            /// </summary>
            public DeclaringType Declaring { get; internal set; }

            /// <summary>
            /// Source types that declare this feature
            /// </summary>
            public enum DeclaringType
            {
                /// <summary>
                /// Assembly
                /// </summary>
                Assembly,

                /// <summary>
                /// Class
                /// </summary>
                Class,

                /// <summary>
                /// Method
                /// </summary>
                Method,

                /// <summary>
                /// Parameter
                /// </summary>
                Parameter,

                /// <summary>
                /// Children
                /// </summary>
                Children,
            }

            /// <summary>
            /// Clone
            /// </summary>
            /// <param name="metaData"></param>
            public void Clone(MetaData metaData) => Declaring = metaData.Declaring;
        }

        internal static readonly ConcurrentReadOnlyDictionary<string, Accessors> Accessors = new ConcurrentReadOnlyDictionary<string, Accessors>();

        #region GetAttributes

        //internal static System.Collections.Generic.List<AttributeBase> GetArgAttr(System.Type type, bool inherit = true)
        //{
        //    var argAttr = new System.Collections.Generic.List<AttributeBase>(type.GetAttributes<AttributeBase>(inherit));
        //    argAttr.Distinct(type.Assembly.GetCustomAttributes<AttributeBase>());//.AddRange(type.Assembly.GetCustomAttributes<AttributeBase>());
        //    return argAttr;
        //}
        internal static System.Collections.Generic.List<AttributeBase> GetTopAttributes(System.Type classType)
        {
            var classAttr = new System.Collections.Generic.List<AttributeBase>();

            //If the subclass annotation is overridden, only the subclass annotation is obtained
            var classType2 = classType;

            while (null != classType2 && !classType2.Equals(typeof(object)))
            {
                var classAttr3 = classType2.GetCustomAttributes<AttributeBase>(false).ToList();

                classAttr.AddRange(classAttr3.Where(c => !(c is GroupAttribute)));

                classAttr.Distinct(classAttr3);

                classType2 = classType2.BaseType;
            }

            //var classAttr = classType.GetAttributes<AttributeBase>();

            foreach (var item in classAttr)
            {
                item.Meta.Declaring = MetaData.DeclaringType.Class;
            }

            //Still get business class assembly annotations
            var assemblyAttr = classType.Assembly.GetCustomAttributes<AttributeBase>();

            foreach (var item in assemblyAttr)
            {
                item.Meta.Declaring = MetaData.DeclaringType.Assembly;
            }

            var attributes = classAttr.Distinct(assemblyAttr);

            return attributes;
        }

        internal static System.Collections.Generic.List<AttributeBase> GetAttributes(MemberInfo member, bool inherit = true)
        {
            var attributes = member.GetAttributes<AttributeBase>(inherit).ToList();
            attributes.ForEach(c => c.Meta.Declaring = MetaData.DeclaringType.Method);
            return attributes;
        }

        internal static System.Collections.Generic.List<AttributeBase> GetAttributes(ParameterInfo member, System.Type type, System.Type origType = null)
        {
            var attributes = new System.Collections.Generic.List<AttributeBase>(member.GetAttributes<AttributeBase>());
            attributes.AddRange(member.ParameterType.GetAttributes<AttributeBase>());
            if (!member.ParameterType.Equals(type))
            {
                attributes.AddRange(type.GetAttributes<AttributeBase>());
            }
            if (null != origType && !type.Equals(origType))
            {
                attributes.AddRange(origType.GetAttributes<AttributeBase>());
            }
            attributes.ForEach(c => c.Meta.Declaring = MetaData.DeclaringType.Parameter);
            return attributes;
        }

        //internal static System.Collections.Generic.List<ArgumentAttribute> GetCollectionAttributes(System.Type type)
        //{
        //    var attributes = new System.Collections.Generic.List<ArgumentAttribute>(type.GetAttributes<ArgumentAttribute>());
        //    attributes.ForEach(c =>
        //    {
        //        c.Meta.Declaring = MetaData.DeclaringType.Parameter;
        //        c.CollectionItem = true;
        //    });
        //    return attributes;
        //}
        internal static Attribute GetAttribute<Attribute>(ParameterInfo member, System.Type type) where Attribute : AttributeBase
        {
            var attribute = member.GetAttribute<Attribute>() ?? type.GetAttribute<Attribute>();
            if (null != attribute)
            {
                attribute.Meta.Declaring = MetaData.DeclaringType.Parameter;
            }
            return attribute;
        }
        internal static System.Collections.Generic.List<AttributeBase> GetAttributes(MemberInfo member, System.Type type, System.Type origType)
        {
            var attributes = new System.Collections.Generic.List<AttributeBase>();
            attributes.AddRange(member.GetAttributes<AttributeBase>());
            attributes.AddRange(type.GetAttributes<AttributeBase>());
            if (null != origType && !type.Equals(origType))
            {
                attributes.AddRange(origType.GetAttributes<AttributeBase>());
            }
            attributes.ForEach(c => c.Meta.Declaring = MetaData.DeclaringType.Children);
            return attributes;
        }

        internal static System.Collections.Generic.List<AttributeBase> GetAttributes(System.Type type, System.Type origType)
        {
            var attributes = new System.Collections.Generic.List<AttributeBase>();
            attributes.AddRange(type.GetAttributes<AttributeBase>());
            if (null != origType && !type.Equals(origType))
            {
                attributes.AddRange(origType.GetAttributes<AttributeBase>());
            }
            attributes.ForEach(c => c.Meta.Declaring = MetaData.DeclaringType.Children);
            return attributes;
        }

        internal static System.Collections.Generic.List<Attribute> GetAttributes<Attribute>(System.Type type, MetaData.DeclaringType source, System.Collections.Generic.IEqualityComparer<Attribute> comparer = null) where Attribute : AttributeBase
        {
            var attributes = type.GetAttributes<Attribute>().Distinct(comparer).ToList();
            attributes.ForEach(c => c.Meta.Declaring = source);
            return attributes;
        }

        internal static bool ExistAttr<Attribute>(ParameterInfo member, System.Type type) where Attribute : System.Attribute
        {
            return member.Exists<Attribute>() || type.Exists<Attribute>();
        }

        #endregion

        #endregion

        /// <summary>
        /// When implemented in a derived class, gets a unique identifier for this System.Attribute.
        /// </summary>
        public sealed override object TypeId => base.TypeId;

        /// <summary>
        /// Accessor
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public object this[string member]
        {
            get
            {
                if (!Accessors.TryGetValue(Meta.Type.FullName, out Accessors meta) || !meta.Accessor.TryGetValue(member, out Accessor accessor)) { return null; }

                return accessor.Getter(this);
            }
            set
            {
                if (!Accessors.TryGetValue(Meta.Type.FullName, out Accessors meta) || !meta.Accessor.TryGetValue(member, out Accessor accessor)) { return; }

                try
                {
                    var value2 = Help.ChangeType(value, accessor.Type);
                    if (Equals(value2, accessor.Getter(this))) { return; }

                    accessor.Setter(this, value2);
                }
                catch { }
            }
        }

        /// <summary>
        /// AttributeBase
        /// </summary>
        public AttributeBase()
        {
            Meta = new MetaData(TypeId as System.Type);

            Meta.Type.LoadAccessors(Accessors, methods: true);
        }

        /// <summary>
        /// Meta
        /// </summary>
        public MetaData Meta { get; }

        /// <summary>
        /// Depth Clone
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual T Clone<T>() where T : AttributeBase
        {
            if (Meta.Type.IsAbstract) { return default; }

            if (!Accessors.TryGetValue(Meta.Type.FullName, out Accessors meta))
            {
                return default;
            }

            if (!(System.Activator.CreateInstance(Meta.Type, meta.ConstructorArgs) is AttributeBase attr))
            {
                return default;
            }

            foreach (var item in meta.Accessor)
            {
                if (item.Value.Injection)
                {
                    continue;
                }

                item.Value.Setter(attr, item.Value.Getter(this));
            }

            attr.Meta.Clone(Meta);

            return attr as T;
        }

        /// <summary>
        /// Replace
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual string Replace(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || !Accessors.TryGetValue(Meta.Type.FullName, out Accessors meta))
            {
                return value;
            }

            var sb = new System.Text.StringBuilder(value);

            foreach (var item in meta.Accessor)
            {
                if (item.Value.Injection) { continue; }

                var member = $"{{{item.Key}}}";

                if (-1 < value.IndexOf(member))
                {
                    sb = sb.Replace(member, System.Convert.ToString(item.Value.Getter(this)));
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// return type name
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Meta.Name;
    }

    #endregion

    #region

    /// <summary>
    /// TestingAttribute
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class TestingAttribute : GroupAttribute
    {
        /// <summary>
        /// regression testing
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <param name="token"></param>
        ///// <param name="tokenMethod">Support method Result.D, input json array [\"Login\",\"{User:\\\"aaa\\\",Password:\\\"123456\\\"}\"]</param>
        //public TestingAttribute(string name, object value, string result = null, string token = null, string tokenMethod = null)
        public TestingAttribute(string name, object value, string result = null, string token = null)
        {
            Name = name;
            Value = value;
            Result = result;
            Token = token;
            //this.TokenMethod = tokenMethod;
            //this.Method = method;
        }

        /// <summary>
        /// test key
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// test args
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// result
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// test fixed roken
        /// </summary>
        public string Token { get; set; }

        ///// <summary>
        ///// Support method Result.D, input json array [\"Login\",\"{User:\\\"aaa\\\",Password:\\\"123456\\\"}\"]
        ///// </summary>
        //public string TokenMethod { get; set; }

        /// <summary>
        /// Target method
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// GroupKey
        /// </summary>
        /// <param name="space"></param>
        /// <returns></returns>
        public override string GroupKey(string space = "->") => $"{base.GroupKey(space)}{space}{Method}{space}{Name}";
    }

    /// <summary>
    /// IgnoreMode
    /// </summary>
    public enum IgnoreMode
    {
        /// <summary>
        /// Ignore business method
        /// </summary>
        Method,
        /// <summary>
        /// Ignore business group method
        /// </summary>
        Group,
        /// <summary>
        /// Ignore document presentation
        /// </summary>
        Arg,
        /// <summary>
        /// Ignore document child presentation
        /// </summary>
        ArgChild,
        /// <summary>
        /// Ignoring global parameter annotations injection
        /// </summary>
        BusinessArg,
    }

    /* 1:Ignore business method 2:Ignore business group method 3,4:Ignore document 5:Ignore Business ArgAttr */
    /// <summary>
    /// Ignore
    /// <para>1: Ignore business method</para>
    /// <para>2: Ignore business group method</para>
    /// <para>3: Ignore document presentation</para>
    /// <para>4: Ignore document child presentation</para>
    /// <para>5: Ignoring global parameter annotations injection</para>
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Assembly | System.AttributeTargets.Class | System.AttributeTargets.Method | System.AttributeTargets.Property | System.AttributeTargets.Parameter | System.AttributeTargets.Field | System.AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
    public class Ignore : GroupAttribute
    {
        /// <summary>
        /// Ignore
        /// </summary>
        /// <param name="mode"></param>
        public Ignore(IgnoreMode mode = IgnoreMode.Method)
        {
            Mode = mode;
        }

        /// <summary>
        /// Mode
        /// </summary>
        public IgnoreMode Mode { get; private set; }

        //public string Group { get; set; }

        //public bool Contains(IgnoreMode mode) => 0 != (this.Mode & mode);

        /// <summary>
        /// GroupKey
        /// </summary>
        /// <param name="space"></param>
        /// <returns></returns>
        public override string GroupKey(string space = "->") => $"{base.GroupKey(space)}{space}{Mode.GetName()}";

        //public bool Contains(IgnoreMode mode) => 0 != (this.Mode & mode);
    }

    ///// <summary>
    ///// Token
    ///// </summary>
    //[System.AttributeUsage(System.AttributeTargets.Parameter | System.AttributeTargets.Class | System.AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    //public sealed class TokenAttribute : System.Attribute { }

    //[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    //public sealed class EnableWatcherAttribute : System.Attribute { }

    /// <summary>
    /// Business info
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class Info : AttributeBase
    {
        /// <summary>
        /// Business Formal Name
        /// </summary>
        /// <param name="businessName"></param>
        public Info(string businessName = null)
        {
            BusinessName = businessName;
            //this.ConfigFileName = System.IO.Path.Combine(Help.BaseDirectory, configFileName);
        }

        /// <summary>
        /// Business formal name
        /// </summary>
        public string BusinessName { get; internal set; }

        /// <summary>
        /// Business friendly alias
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Type fullName
        /// </summary>
        public string TypeFullName { get; internal set; }

        /// <summary>
        /// Document physical path
        /// </summary>
        public string DocPhysicalPath { get; internal set; }

        ///// <summary>
        ///// Document file name
        ///// </summary>
        //public string DocFileName { get; internal set; }

        ///// <summary>
        ///// Document web request path
        ///// </summary>
        //public string DocRequestPath { get; internal set; }

        string commandGroupDefault = string.Empty;// "Default";
        /// <summary>
        /// Group default value
        /// </summary>
        public string CommandGroupDefault { get => commandGroupDefault; set => commandGroupDefault = value?.Trim() ?? string.Empty; }

        //public string ConfigFileName { get; internal set; }

        /// <summary>
        /// GetCommandGroup
        /// </summary>
        /// <param name="group"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual string GetCommandGroup(string group, string name) => string.IsNullOrWhiteSpace(group) ? name : $"{group}.{name}";
    }

    /// <summary>
    /// UseEntry
    /// </summary>
    public readonly struct UseEntry
    {
        /// <summary>
        /// UseEntry
        /// </summary>
        /// <param name="value"></param>
        /// <param name="parameterName"></param>
        public UseEntry(object value, params string[] parameterName)
        {
            Value = value;
            Type = Value?.GetType();
            ParameterName = parameterName;
        }

        /// <summary>
        /// Type
        /// </summary>
        public System.Type Type { get; }

        /// <summary>
        /// Value
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// ParameterName
        /// </summary>
        public string[] ParameterName { get; }
    }

    /// <summary>
    /// Injecting Objects Corresponding to Parameters
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Interface | System.AttributeTargets.Class | System.AttributeTargets.Struct | System.AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class UseAttribute : AttributeBase
    {
        /// <summary>
        /// Injecting Objects Corresponding to Parameter type
        /// </summary>
        /// <param name="parameterType"></param>
        public UseAttribute(System.Type parameterType = null) => ParameterType = parameterType;

        /// <summary>
        /// Use parameter names to correspond to injection objects
        /// </summary>
        public bool ParameterName { get; set; }

        /// <summary>
        /// Injecting Objects Corresponding to Parameter type
        /// </summary>
        public System.Type ParameterType { get; set; }
    }

    /// <summary>
    /// Dynamic object, DocUI string type display
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Interface | System.AttributeTargets.Class | System.AttributeTargets.Struct | System.AttributeTargets.Parameter | System.AttributeTargets.Property | System.AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class DynamicObjectAttribute : AttributeBase { }

    /// <summary>
    /// Injection Property and Field
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class InjectionAttribute : AttributeBase { }

    /// <summary>
    /// Friendly name
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct | System.AttributeTargets.Parameter | System.AttributeTargets.Property | System.AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public sealed class AliasAttribute : GroupAttribute
    {
        /// <summary>
        /// AliasAttribute
        /// </summary>
        /// <param name="name"></param>
        public AliasAttribute(string name = null) => Name = name;

        /// <summary>
        /// Friendly name
        /// </summary>
        public string Name { get; internal set; }
    }

    /// <summary>
    /// Document grouping configuration
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class DocGroupAttribute : GroupAttribute
    {
        /// <summary>
        /// Grouping name
        /// </summary>
        /// <param name="group"></param>
        public DocGroupAttribute(string group = null) => Group = group;

        /// <summary>
        /// Grouping name
        /// </summary>
        public override string Group { get => base.Group; set => base.Group = value; }

        /// <summary>
        /// position
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// badge
        /// </summary>
        public string Badge { get; set; }

        /// <summary>
        /// Allowed to expand
        /// </summary>
        public bool Active { get; set; }
    }

    /// <summary>
    /// Document configuration
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class DocAttribute : GroupAttribute
    {
        /// <summary>
        /// Friendly name
        /// </summary>
        /// <param name="alias"></param>
        public DocAttribute(string alias = null) => Alias = alias;

        /// <summary>
        /// Method alias grouping
        /// </summary>
        public override string Group { get => base.Group; set => base.Group = value; }

        /// <summary>
        /// Friendly name
        /// </summary>
        public string Alias { get; internal set; }

        /// <summary>
        /// position
        /// </summary>
        public int Position { get; set; } = -1;

        /// <summary>
        /// badge
        /// </summary>
        public string Badge { get; set; }

        //public string Description { get; set; }
    }

    /// <summary>
    /// LoggerAttribute
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Interface | System.AttributeTargets.Assembly | System.AttributeTargets.Class | System.AttributeTargets.Method | System.AttributeTargets.Struct | System.AttributeTargets.Parameter, AllowMultiple = true, Inherited = true)]
    public class LoggerAttribute : GroupAttribute
    {
        /// <summary>
        /// LoggerAttribute
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="canWrite"></param>
        /// <param name="valueType"></param>
        public LoggerAttribute(Logger.Type logType = Logger.Type.All, bool canWrite = true, Logger.ValueType valueType = Logger.ValueType.Out)
        {
            LogType = logType;
            CanWrite = canWrite;
            ValueType = valueType;
        }

        /// <summary>
        /// Record type
        /// </summary>
        public Logger.Type LogType { get; private set; }

        /// <summary>
        /// Allow record
        /// </summary>
        public bool CanWrite { get; set; }

        /// <summary>
        /// Logger value type
        /// </summary>
        public Logger.ValueType ValueType { get; set; } = Logger.ValueType.Out;

        /// <summary>
        /// Allowed to return to parameters
        /// </summary>
        public LoggerValueMode CanValue { get; set; }

        /// <summary>
        /// Allowed to return to results
        /// </summary>
        public bool CanResult { get; set; } = true;

        /// <summary>
        /// SetType
        /// </summary>
        /// <param name="logType"></param>
        /// <returns></returns>
        public LoggerAttribute SetType(Logger.Type logType)
        {
            LogType = logType;
            return this;
        }

        //public static System.Collections.Generic.IEqualityComparer<LoggerAttribute> Comparer = Equality<LoggerAttribute>.CreateComparer(c => c.LogType);

        //public override T Clone<T>() => new LoggerAttribute(this.LogType) { Group = this.Group, CanResult = this.CanResult, CanValue = this.CanValue, CanWrite = this.CanWrite } as T;

        //public override string Key(string space = "->") => string.Format("{1}{0}{2}", space, this.Mode.GetName(), this.Group.Trim());

        /// <summary>
        /// GroupKey
        /// </summary>
        /// <param name="space"></param>
        /// <returns></returns>
        public override string GroupKey(string space = "->") => $"{ base.GroupKey(space)}{space}{LogType.GetName()}";

        /// <summary>
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{Meta.Name} {LogType}";
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

    /// <summary>
    /// GroupAttribute
    /// </summary>
    public abstract class GroupAttribute : AttributeBase
    {
        static System.Type GetType(System.Type type)
        {
            while (null != type.BaseType && !type.BaseType.IsAbstract)
            {
                type = type.BaseType;
            }

            return type;
        }

        /// <summary>
        /// Get unified type
        /// </summary>
        readonly System.Type type;

        /// <summary>
        /// GroupAttribute
        /// </summary>
        /// <param name="type"></param>
        public GroupAttribute(System.Type type = null) => this.type = GetType(type ?? Meta.Type);

        string group = string.Empty;

        /// <summary>
        /// Used for the command group
        /// </summary>
        public virtual string Group { get => group; set => group = value?.Trim() ?? string.Empty; }

        /// <summary>
        /// GroupKey
        /// </summary>
        /// <param name="space"></param>
        /// <returns></returns>
        public virtual string GroupKey(string space = "->") => $"{type.FullName}{space}{Group.Trim()}";
        //public virtual string GroupKey(string space = "->") => $"{this.Meta.Type.FullName}{space}{this.Group.Trim()}";

        /// <summary>
        /// Comparer
        /// </summary>
        public static System.Collections.Generic.IEqualityComparer<GroupAttribute> Comparer { get => Equality<GroupAttribute>.CreateComparer(c => c.GroupKey(), System.StringComparer.CurrentCultureIgnoreCase); }
    }

    /// <summary>
    /// ArgumentDeserialize
    /// </summary>
    public abstract class ArgumentDeserialize : ArgumentAttribute
    {
        /// <summary>
        /// ArgumentDeserialize
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        /// <param name="type"></param>
        protected ArgumentDeserialize(int state, string message = null, System.Type type = null) : base(state, message, type)
        {
            ArgMeta.Skip = (bool hasUse, bool hasDefinition, AttributeBase.MetaData.DeclaringType declaring, System.Collections.Generic.IEnumerable<ArgumentAttribute> arguments, bool ignoreArg, bool dynamicObject) => (!hasDefinition && !ArgMeta.Arg.HasCollection && !dynamicObject) || ArgMeta.Arg.Parameters || ignoreArg;
            ArgMeta.Deserialize = true;
        }
    }

    /// <summary>
    /// Base class for all attributes that apply to parameters
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Assembly | System.AttributeTargets.Method | System.AttributeTargets.Class | System.AttributeTargets.Struct | System.AttributeTargets.Property | System.AttributeTargets.Field | System.AttributeTargets.Parameter, AllowMultiple = true, Inherited = true)]
    public abstract class ArgumentAttribute : GroupAttribute
    {
        /// <summary>
        /// MetaData
        /// </summary>
        public new class MetaData
        {
            internal System.Type resultType;
            internal System.Type resultTypeDefinition;
            //internal System.Type argTypeDefinition;

            internal static System.Collections.Generic.Dictionary<string, Accessor> Accessor = new System.Collections.Generic.Dictionary<string, Accessor>();

            static MetaData()
            {
                foreach (var property in typeof(MetaData).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Static))
                {
                    if (typeof(System.MulticastDelegate).IsAssignableFrom(property.PropertyType))
                    {
                        continue;
                    }

                    var accessor = property.GetAccessor();
                    if (null == accessor.Getter || null == accessor.Setter) { continue; }
                    Accessor.Add($"ArgMeta.{property.Name}", accessor);
                }
            }

            //public dynamic Business { get; internal set; }

            /// <summary>
            /// Declaring
            /// </summary>
            public string Business { get; internal set; }

            /// <summary>
            /// Business Friendly Name
            /// </summary>
            public string BusinessName { get; internal set; }

            /// <summary>
            /// Method
            /// </summary>
            public string Method { get; internal set; }

            /// <summary>
            /// MethodOnlyName
            /// </summary>
            public string MethodOnlyName { get; internal set; }

            /// <summary>
            /// MemberPath
            /// </summary>
            public string MemberPath { get; internal set; }

            /// <summary>
            /// Member
            /// </summary>
            public string Member { get; internal set; }

            /// <summary>
            /// Remove IArg Null type
            /// </summary>
            public System.Type MemberType { get; internal set; }

            ///// <summary>
            ///// Remove IArg type
            ///// </summary>
            //public System.Type MemberOrigType { get; internal set; }

            /// <summary>
            /// Arg
            /// </summary>
            public Meta.Args Arg { get; internal set; }

            /// <summary>
            /// is Deserialize
            /// </summary>
            public bool Deserialize { get; internal set; }

            internal Proces Proces { get; set; }

            /// <summary>
            /// command key
            /// </summary>
            public string Key { get; internal set; }

            /// <summary>
            /// This value indicates that the annotation is a filter model used to apply parameters. The default value is UseNotParameterLevel, which means that the injection parameters are filtered out and the annotation is non parameter level
            /// <para>default: FilterModel.UseNotParameterLevel</para>
            /// </summary>
            public FilterModel Filter { get; set; } = FilterModel.UseNotParameterLevel;

            /// <summary>
            /// Custom filtering, Returns true to indicate that it does not work on this object, default false
            /// </summary>
            public System.Func<bool, bool, AttributeBase.MetaData.DeclaringType, System.Collections.Generic.IEnumerable<ArgumentAttribute>, bool, bool, bool> Skip { get; set; }

            /// <summary>
            /// Clone
            /// </summary>
            /// <param name="metaData"></param>
            public void Clone(MetaData metaData)
            {
                resultType = metaData.resultType;
                resultTypeDefinition = metaData.resultTypeDefinition;
                Business = metaData.Business;
                BusinessName = metaData.BusinessName;
                Method = metaData.Method;
                MethodOnlyName = metaData.MethodOnlyName;
                MemberPath = metaData.MemberPath;
                Member = metaData.Member;
                MemberType = metaData.MemberType;
                //this.MemberOrigType = metaData.MemberOrigType;
                Arg = metaData.Arg;
                Deserialize = metaData.Deserialize;
                Key = metaData.Key;

                if (null != metaData.Proces)
                {
                    Proces = new Proces(metaData.Proces.MethodInfo, metaData.Proces.ParameterType)
                    {
                        Mode = metaData.Proces.Mode,
                        Call = metaData.Proces.Call
                    };
                }
            }
        }

        /// <summary>
        /// Clone
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override T Clone<T>()
        {
            var clone = base.Clone<ArgumentAttribute>();
            clone.ArgMeta.Clone(ArgMeta);
            return clone as T;
        }

        readonly struct ProcesMethod
        {
            public readonly System.Type[] proces;
            public readonly System.Type[] procesToken;

            public ProcesMethod(System.Type[] proces, System.Type[] procesToken)
            {
                this.proces = proces;
                this.procesToken = procesToken;
            }
        }

        //static readonly ProcesMethod procesMethod = new ProcesMethod(Help.GetMethod<ArgumentAttribute>(c => c.Proces(null)).GetParameters().Select(c => c.ParameterType).ToArray(), Help.GetMethod<ArgumentAttribute>(c => c.Proces(null, -1, null)).GetParameters().Select(c => c.ParameterType).ToArray());
        static readonly ProcesMethod procesMethod = new ProcesMethod(Help.GetMethod<ArgumentAttribute>(c => c.Proces(null)).GetParameters().Select(c => c.ParameterType).ToArray(), Help.GetMethod<ArgumentAttribute>(c => c.Proces<object>(null, null)).GetParameters().Select(c => c.ParameterType).ToArray());
        //static readonly ProcesMethod procesMethod = new ProcesMethod();

        /// <summary>
        /// Argument base attribute
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        /// <param name="type"></param>
        public ArgumentAttribute(int state, string message = null, System.Type type = null) : base(type)
        {
            State = state;
            Message = message;
            //this.CanNull = canNull;
            ArgMeta = new MetaData();

            if (Accessors.TryGetValue(Meta.Type.FullName, out Accessors meta) && meta.Methods.TryGetValue(nameof(ArgumentAttribute.Proces), out Proces method))
            {
                ArgMeta.Proces = new Proces(method.MethodInfo, method.ParameterType);

                if (Enumerable.SequenceEqual(procesMethod.proces, method.ParameterType))
                {
                    ArgMeta.Proces.Mode = method.MethodInfo.IsGenericMethod ? Utils.Proces.ProcesMode.ProcesGeneric : Utils.Proces.ProcesMode.Proces;
                }
                else if (Enumerable.SequenceEqual(procesMethod.procesToken, method.ParameterType))
                {
                    ArgMeta.Proces.Mode = Utils.Proces.ProcesMode.ProcesGenericToken;
                }
                //else if (Enumerable.SequenceEqual(procesMethod.procesCollection, method.ParameterType))
                //{
                //    this.ArgMeta.Proces.Mode = method.MethodInfo.IsGenericMethod ? Utils.Proces.ProcesMode.ProcesCollectionGeneric : Utils.Proces.ProcesMode.ProcesCollection;
                //}
            }

            BindAfter += () =>
            {
                if (!string.IsNullOrWhiteSpace(Message))
                {
                    Message = Replace(Message);
                }

                if (!string.IsNullOrWhiteSpace(Description))
                {
                    Description = Replace(Description);
                }
            };
        }

        /// <summary>
        /// GetProcesResult
        /// </summary>
        /// <param name="value"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async ValueTask<IResult> GetProcesResult(dynamic value, dynamic token = null)
        {
            var result = await CheckNull(value);

            if (0 >= result.State)
            {
                return result;
            }

            switch (ArgMeta.Proces?.Mode)
            {
                case Utils.Proces.ProcesMode.ProcesGeneric:
                    {
                        result = ArgMeta.Proces.Call(this, new object[] { value });
                        return await result;
                    }
                case Utils.Proces.ProcesMode.ProcesGenericToken:
                    {
                        result = ArgMeta.Proces.Call(this, new object[] { token, value });
                        return await result;
                    }
                default: return await this.Proces(value);
            }
        }

        /// <summary>
        /// Replace
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override string Replace(string value)
        {
            var sb = new System.Text.StringBuilder(base.Replace(value));

            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            foreach (var item in MetaData.Accessor)
            {
                var member = $"{{{item.Key}}}";

                if (-1 < value.IndexOf(member))
                {
                    sb = sb.Replace(member, System.Convert.ToString(item.Value.Getter(ArgMeta)));
                }
            }

            return sb.ToString();
        }

        //public override string GroupKey(string space = "->") => $"{base.GroupKey(space)}{space}{this.CollectionItem}";

        /// <summary>
        /// Business
        /// </summary>
        public dynamic Business { get; internal set; }

        /// <summary>
        /// BindAfter
        /// </summary>
        public System.Action BindAfter { get; set; }

        /// <summary>
        /// ArgMeta
        /// </summary>
        public MetaData ArgMeta { get; }

        ///// <summary>
        ///// Whether to apply to each item of a set parameter
        ///// </summary>
        //public bool CollectionItem { get; set; }

        /// <summary>
        /// By checking the Allow null value, Default to true
        /// </summary>
        public bool CanNull { get; set; }

        int state;
        /// <summary>
        /// Used to return state
        /// </summary>
        public virtual int State { get => state; set => state = value.ConvertErrorState(); }

        /// <summary>
        /// Used to return error messages
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Amicable name
        /// </summary>
        public string Alias { get; set; }

        ///// <summary>
        ///// This value indicates whether it should only be used for a specific name parameter, and the default null applies to all parameters
        ///// </summary>
        //public string ApplyName { get; set; }

        internal static bool GetFilter(FilterModel filter, bool hasUse, bool hasDefinition, AttributeBase.MetaData.DeclaringType declaring)
        {
            switch (filter)
            {
                case FilterModel.No: break;
                case FilterModel.UseParameterLevel:
                    if (hasUse && AttributeBase.MetaData.DeclaringType.Parameter == declaring) return true;
                    break;
                case FilterModel.UseNotParameterLevel:
                    if (hasUse && AttributeBase.MetaData.DeclaringType.Parameter != declaring) return true;
                    break;
                case FilterModel.NotUseParameterLevel:
                    if (!hasUse && AttributeBase.MetaData.DeclaringType.Parameter == declaring) return true;
                    break;
                case FilterModel.NotUseNotParameterLevel:
                    if (!hasUse && AttributeBase.MetaData.DeclaringType.Parameter != declaring) return true;
                    break;
                case FilterModel.Definition:
                    if (hasDefinition) return true;
                    break;
                case FilterModel.NotDefinition:
                    if (!hasDefinition) return true;
                    break;

                case FilterModel.UseParameterLevel | FilterModel.Definition:
                    if ((hasUse && AttributeBase.MetaData.DeclaringType.Parameter == declaring) || hasDefinition) return true;
                    break;
                case FilterModel.UseNotParameterLevel | FilterModel.Definition:
                    if ((hasUse && AttributeBase.MetaData.DeclaringType.Parameter != declaring) || hasDefinition) return true;
                    break;

                case FilterModel.UseParameterLevel | FilterModel.NotDefinition:
                    if ((hasUse && AttributeBase.MetaData.DeclaringType.Parameter == declaring) || !hasDefinition) return true;
                    break;
                case FilterModel.UseNotParameterLevel | FilterModel.NotDefinition:
                    if ((hasUse && AttributeBase.MetaData.DeclaringType.Parameter != declaring) || !hasDefinition) return true;
                    break;

                //=======================================//

                case FilterModel.NotUseParameterLevel | FilterModel.Definition:
                    if ((!hasUse && AttributeBase.MetaData.DeclaringType.Parameter == declaring) || hasDefinition) return true;
                    break;
                case FilterModel.NotUseNotParameterLevel | FilterModel.Definition:
                    if ((!hasUse && AttributeBase.MetaData.DeclaringType.Parameter != declaring) || hasDefinition) return true;
                    break;

                case FilterModel.NotUseParameterLevel | FilterModel.NotDefinition:
                    if ((!hasUse && AttributeBase.MetaData.DeclaringType.Parameter == declaring) || !hasDefinition) return true;
                    break;
                case FilterModel.NotUseNotParameterLevel | FilterModel.NotDefinition:
                    if ((!hasUse && AttributeBase.MetaData.DeclaringType.Parameter != declaring) || !hasDefinition) return true;
                    break;
                default: break;
            }

            return false;
        }

        #region Proces

        /// <summary>
        /// Start processing the Parameter object, By this.ResultCreate() method returns
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual ValueTask<IResult> Proces(dynamic value) => new ValueTask<IResult>(ResultCreate<dynamic>(value));

        /// <summary>
        /// Start processing the Parameter object, By this.ResultCreate() method returns
        /// </summary>
        /// <typeparam name="Type"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual ValueTask<IResult> Proces<Type>(dynamic value) => new ValueTask<IResult>(ResultCreate<Type>(value));

        /// <summary>
        /// Start processing the Parameter object, By this.ResultCreate() method returns
        /// </summary>
        /// <typeparam name="Type"></typeparam>
        /// <param name="token"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual ValueTask<IResult> Proces<Type>(dynamic token, dynamic value) => new ValueTask<IResult>(ResultCreate<Type>(value));

        ///// <summary>
        ///// Start processing the Parameter object, By this.ResultCreate() method returns
        ///// </summary>
        ///// <param name="value"></param>
        ///// <param name="collectionIndex"></param>
        ///// <param name="dictKey"></param>
        ///// <returns></returns>
        //public virtual async ValueTask<IResult> Proces(dynamic value, int collectionIndex, dynamic dictKey) => this.ResultCreate<dynamic>(value);

        ///// <summary>
        ///// Start processing the Parameter object, By this.ResultCreate() method returns
        ///// </summary>
        ///// <typeparam name="Type"></typeparam>
        ///// <param name="value"></param>
        ///// <param name="collectionIndex"></param>
        ///// <param name="dictKey"></param>
        ///// <returns></returns>
        //public virtual async ValueTask<IResult> Proces<Type>(dynamic value, int collectionIndex, dynamic dictKey) => this.ResultCreate<dynamic>(value);

        #endregion

        #region Result

        /// <summary>
        /// Used to create the Proces() method returns object
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public IResult ResultCreate(int state = 1) => ResultFactory.ResultCreate(ArgMeta.resultType, ArgMeta.resultTypeDefinition, state);

        /// <summary>
        /// Used to create the Proces() method returns object
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public IResult ResultCreate(int state, string message) => ResultFactory.ResultCreate(ArgMeta.resultType, ArgMeta.resultTypeDefinition, state, message);

        /// <summary>
        /// Used to create the Proces() method returns object
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IResult ResultCreate(int state, string message, string callback = null) => ResultFactory.ResultCreate(ArgMeta.resultType, ArgMeta.resultTypeDefinition, state, message, callback);

        /// <summary>
        /// Used to create the Proces() method returns object
        /// </summary>
        /// <typeparam name="Data"></typeparam>
        /// <param name="data"></param>
        /// <param name="message"></param>
        /// <param name="state"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IResult ResultCreate<Data>(Data data, string message = null, int state = 1, string callback = null) => ResultFactory.ResultCreate(ArgMeta.resultTypeDefinition, data, message, state, callback);

        /// <summary>
        /// Used to create the Proces() method returns object
        /// </summary>
        /// <param name="data"></param>
        /// <param name="message"></param>
        /// <param name="state"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IResult ResultCreate(object data, string message = null, int state = 1, string callback = null) => ResultFactory.ResultCreate(ArgMeta.resultTypeDefinition, data, message, state, callback);

        /// <summary>
        /// Using the current State, Message returns an error result
        /// </summary>
        /// <returns></returns>
        public IResult ResultError() => ResultCreate(State, Message);

        #endregion

        ///// <summary>
        ///// CheckNull
        ///// </summary>
        ///// <param name="attribute"></param>
        ///// <param name="value"></param>
        ///// <returns></returns>
        //public static IResult CheckNull(ArgumentAttribute attribute, dynamic value)
        //{
        //    if (object.Equals(null, value))
        //    {
        //        if (attribute.CanNull)
        //        {
        //            return attribute.ResultCreate();
        //        }
        //        else
        //        {
        //            return attribute.ResultCreate(attribute.State, attribute.Message ?? $"argument \"{attribute.Alias}\" can not null.");
        //        }
        //    }

        //    return attribute.ResultCreate(data: value);
        //}

        /// <summary>
        /// CheckNull
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual ValueTask<IResult> CheckNull(dynamic value)
        {
            if ((ArgMeta.Arg?.Nullable ?? false) || CanNull || !Equals(null, value))
            {
                return new ValueTask<IResult>(ResultCreate());
            }

            return new ValueTask<IResult>(ResultCreate(State, Message ?? $"argument \"{Alias}\" can not null."));
        }
    }

    /// <summary>
    /// FilterModel
    /// </summary>
    [System.Flags]
    public enum FilterModel
    {
        /// <summary>
        /// Apply all parameters, including injection, without filtering
        /// </summary>
        No = 2,

        ///// <summary>
        ///// Filter out non parameter level annotation
        ///// </summary>
        //ParameterLevel = 4,

        ///// <summary>
        ///// Filter out non parameter level annotation
        ///// </summary>
        //NotParameterLevel = 8,

        ///// <summary>
        ///// Filter out injection parameters
        ///// </summary>
        //Use = 16,

        ///// <summary>
        ///// Filter out non injection parameters
        ///// </summary>
        //NotUse = 32,

        /// <summary>
        /// Filter out injection parameters also parameter level annotation
        /// </summary>
        UseParameterLevel = 64,

        /// <summary>
        /// Filter out injection parameters also non parameter level annotation
        /// </summary>
        UseNotParameterLevel = 128,

        /// <summary>
        /// Filter out non injection parameters also parameter level annotation
        /// </summary>
        NotUseParameterLevel = 256,

        /// <summary>
        /// Filter out non injection parameters also non parameter level annotation
        /// </summary>
        NotUseNotParameterLevel = 512,

        /// <summary>
        /// This value indicates whether it filter out custom parameters
        /// </summary>
        Definition = 1024,

        /// <summary>
        /// This value indicates whether it applies only to non custom parameters
        /// </summary>
        NotDefinition = 2048,

        ///// <summary>
        ///// This value indicates whether it applies only to Parameters parameters
        ///// </summary>
        //Parameters = 4096
    }

    /// <summary>
    /// Command attribute on a method, for multiple sources to invoke the method
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Assembly | System.AttributeTargets.Class | System.AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class CommandAttribute : GroupAttribute
    {
        /// <summary>
        /// Command attribute on a method, for multiple sources to invoke the method
        /// </summary>
        /// <param name="onlyName"></param>
        public CommandAttribute(string onlyName = null) { OnlyName = onlyName; }

        /// <summary>
        /// Key
        /// </summary>
        public string Key { get; internal set; }

        /// <summary>
        /// OnlyName
        /// </summary>
        public string OnlyName { get; set; }

        ///// <summary>
        ///// OnlyNameByte
        ///// </summary>
        //public byte OnlyNameByte { get; set; }

        //public bool IgnoreBusinessArg { get; set; }

        /// <summary>
        /// GroupKey
        /// </summary>
        /// <param name="space"></param>
        /// <returns></returns>
        public override string GroupKey(string space = "->") => $"{base.GroupKey(space)}{space}{OnlyName}";

        /// <summary>
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{Meta.Name} {Group} {OnlyName}";
    }
}
