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
    using Utils;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Linq;

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
            /// Gets the fully qualified type name, including the namespace but not the assembly
            /// </summary>
            public System.Type Type { get; internal set; }

            /// <summary>
            /// Declare the source of this feature
            /// </summary>
            public DeclaringType Declaring { get; internal set; }

            /// <summary>
            /// Clone
            /// </summary>
            /// <param name="metaData"></param>
            public void Clone(MetaData metaData) => this.Declaring = metaData.Declaring;
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

            var attributes = new System.Collections.Generic.List<AttributeBase>(classAttr).Distinct(assemblyAttr);

            return attributes;
        }

        internal static System.Collections.Generic.List<AttributeBase> GetAttributes(MemberInfo member, bool inherit = true)
        {
            var attributes = member.GetAttributes<AttributeBase>(inherit).ToList();
            attributes.ForEach(c => c.Meta.Declaring = MetaData.DeclaringType.Method);
            return attributes;
        }

        internal static System.Collections.Generic.List<AttributeBase> GetAttributes(ParameterInfo member, System.Type outType, System.Type origType = null)
        {
            var attributes = new System.Collections.Generic.List<AttributeBase>(member.GetAttributes<AttributeBase>());
            attributes.AddRange(member.ParameterType.GetAttributes<AttributeBase>());
            if (!member.ParameterType.Equals(outType))
            {
                attributes.AddRange(outType.GetAttributes<AttributeBase>());
            }
            if (null != origType && !outType.Equals(origType))
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
        internal static System.Collections.Generic.List<AttributeBase> GetAttributes(MemberInfo member, System.Type type)
        {
            var attributes = new System.Collections.Generic.List<AttributeBase>();
            attributes.AddRange(member.GetAttributes<AttributeBase>());
            attributes.AddRange(type.GetAttributes<AttributeBase>());
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
                if (!Accessors.TryGetValue(this.Meta.Type.FullName, out Accessors meta) || !meta.Accessor.TryGetValue(member, out Accessor accessor)) { return null; }

                return accessor.Getter(this);
            }
            set
            {
                if (!Accessors.TryGetValue(this.Meta.Type.FullName, out Accessors meta) || !meta.Accessor.TryGetValue(member, out Accessor accessor)) { return; }

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
            this.Meta = new MetaData { Type = this.TypeId as System.Type };

            this.Meta.Type.LoadAccessors(Accessors, methods: true);
        }

        #region

        /// <summary>
        /// Meta
        /// </summary>
        public MetaData Meta { get; }

        #endregion

        /// <summary>
        /// Depth Clone
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual T Clone<T>() where T : AttributeBase
        {
            if (this.Meta.Type.IsAbstract) { return default; }

            if (!Accessors.TryGetValue(this.Meta.Type.FullName, out Accessors meta))
            {
                return default;
            }

            if (!(System.Activator.CreateInstance(this.Meta.Type, meta.ConstructorArgs) is AttributeBase attr)) { return default; }

            foreach (var item in meta.Accessor)
            {
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
            if (string.IsNullOrWhiteSpace(value) || !Accessors.TryGetValue(this.Meta.Type.FullName, out Accessors meta))
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
            this.Name = name;
            this.Value = value;
            this.Result = result;
            this.Token = token;
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
        public override string GroupKey(string space = "->") => $"{base.GroupKey(space)}{space}{this.Method}{space}{this.Name}";
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
            this.Mode = mode;
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
        public override string GroupKey(string space = "->") => $"{base.GroupKey(space)}{space}{this.Mode.GetName()}";

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
            this.BusinessName = businessName;
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

        /// <summary>
        /// Document file name
        /// </summary>
        public string DocFileName { get; internal set; }

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
            this.Value = value;
            this.Type = this.Value?.GetType();
            this.ParameterName = parameterName;
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
    [System.AttributeUsage(System.AttributeTargets.Interface | System.AttributeTargets.Class | System.AttributeTargets.Struct | System.AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public sealed class UseAttribute : AttributeBase
    {
        /// <summary>
        /// Injecting Objects Corresponding to Parameter type
        /// </summary>
        /// <param name="parameterType"></param>
        public UseAttribute(System.Type parameterType = null) => this.ParameterType = parameterType;

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
    /// Friendly name
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct | System.AttributeTargets.Parameter | System.AttributeTargets.Property | System.AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public sealed class AliasAttribute : GroupAttribute
    {
        /// <summary>
        /// AliasAttribute
        /// </summary>
        /// <param name="name"></param>
        public AliasAttribute(string name = null) => this.Name = name;

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
        public DocGroupAttribute(string group = null) => this.Group = group;

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
        public DocAttribute(string alias = null) => this.Alias = alias;

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
        public LoggerAttribute(Logger.Type logType = Logger.Type.All, bool canWrite = true, Logger.ValueType valueType = Logger.ValueType.In)
        {
            this.LogType = logType;
            this.CanWrite = canWrite;
            this.ValueType = valueType;
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
        /// Allowed to return to parameters
        /// </summary>
        public LoggerValueMode CanValue { get; set; }

        /// <summary>
        /// Allowed to return to results
        /// </summary>
        public bool CanResult { get; set; } = true;

        /// <summary>
        /// Logger value type
        /// </summary>
        public Logger.ValueType ValueType { get; set; } = Logger.ValueType.In;

        /// <summary>
        /// SetType
        /// </summary>
        /// <param name="logType"></param>
        /// <returns></returns>
        public LoggerAttribute SetType(Logger.Type logType)
        {
            this.LogType = logType;
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
        public override string GroupKey(string space = "->") => $"{ base.GroupKey(space)}{space}{this.LogType.GetName()}";

        /// <summary>
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{this.Meta.Type.Name} {this.LogType.ToString()}";
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
        public GroupAttribute() => type = GetType(this.Meta.Type);

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
        public virtual string GroupKey(string space = "->") => $"{type.FullName}{space}{this.Group.Trim()}";
        //public virtual string GroupKey(string space = "->") => $"{this.Meta.Type.FullName}{space}{this.Group.Trim()}";

        /// <summary>
        /// Comparer
        /// </summary>
        public static System.Collections.Generic.IEqualityComparer<GroupAttribute> Comparer { get => Equality<GroupAttribute>.CreateComparer(c => c.GroupKey(), System.StringComparer.CurrentCultureIgnoreCase); }
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
            internal System.Type argTypeDefinition;

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
            /// MemberType
            /// </summary>
            public System.Type MemberType { get; internal set; }

            /// <summary>
            /// Arg
            /// </summary>
            public Meta.Args Arg { get; internal set; }

            internal Proces Proces { get; set; }

            /// <summary>
            /// This value indicates that the annotation is a filter model used to apply parameters. The default value is UseNotParameterLevel, which means that the injection parameters are filtered out and the annotation is non parameter level
            /// <para>default: FilterModel.UseNotParameterLevel</para>
            /// </summary>
            public FilterModel Filter { get; set; } = FilterModel.UseNotParameterLevel;

            /// <summary>
            /// Custom filtering, Returns true to indicate that it does not work on this object, default false
            /// </summary>
            public System.Func<bool, bool, AttributeBase.MetaData.DeclaringType, System.Collections.Generic.IEnumerable<ArgumentAttribute>, bool> Skip { get; set; }

            /// <summary>
            /// Clone
            /// </summary>
            /// <param name="metaData"></param>
            public void Clone(MetaData metaData)
            {
                this.resultType = metaData.resultType;
                this.resultTypeDefinition = metaData.resultTypeDefinition;
                this.Business = metaData.Business;
                this.BusinessName = metaData.BusinessName;
                this.Method = metaData.Method;
                this.MethodOnlyName = metaData.MethodOnlyName;
                this.MemberPath = metaData.MemberPath;
                this.Member = metaData.Member;
                this.MemberType = metaData.MemberType;
                this.Arg = metaData.Arg;

                if (null != metaData.Proces)
                {
                    this.Proces = new Proces(metaData.Proces.MethodInfo, metaData.Proces.ParameterType)
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
        public ArgumentAttribute(int state, string message = null)
        {
            this.State = state;
            this.Message = message;
            //this.CanNull = canNull;
            this.ArgMeta = new MetaData();

            if (Accessors.TryGetValue(this.Meta.Type.FullName, out Accessors meta) && meta.Methods.TryGetValue(nameof(ArgumentAttribute.Proces), out Proces method))
            {
                this.ArgMeta.Proces = new Proces(method.MethodInfo, method.ParameterType);

                if (Enumerable.SequenceEqual(procesMethod.proces, method.ParameterType))
                {
                    this.ArgMeta.Proces.Mode = method.MethodInfo.IsGenericMethod ? Utils.Proces.ProcesMode.ProcesGeneric : Utils.Proces.ProcesMode.Proces;
                }
                else if (Enumerable.SequenceEqual(procesMethod.procesToken, method.ParameterType))
                {
                    this.ArgMeta.Proces.Mode = Utils.Proces.ProcesMode.ProcesGenericToken;
                }
                //else if (Enumerable.SequenceEqual(procesMethod.procesCollection, method.ParameterType))
                //{
                //    this.ArgMeta.Proces.Mode = method.MethodInfo.IsGenericMethod ? Utils.Proces.ProcesMode.ProcesCollectionGeneric : Utils.Proces.ProcesMode.ProcesCollection;
                //}
            }

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

        //public override string GroupKey(string space = "->") => $"{base.GroupKey(space)}{space}{this.CollectionItem}";

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
        public virtual async ValueTask<IResult> Proces(dynamic value) => this.ResultCreate<dynamic>(value);

        /// <summary>
        /// Start processing the Parameter object, By this.ResultCreate() method returns
        /// </summary>
        /// <typeparam name="Type"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual async ValueTask<IResult> Proces<Type>(dynamic value) => this.ResultCreate<Type>(value);

        /// <summary>
        /// Start processing the Parameter object, By this.ResultCreate() method returns
        /// </summary>
        /// <typeparam name="Type"></typeparam>
        /// <param name="token"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual async ValueTask<IResult> Proces<Type>(dynamic token, dynamic value) => this.ResultCreate<Type>(value);

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
        public IResult ResultCreate(int state) => ResultFactory.ResultCreate(ArgMeta.resultType, ArgMeta.resultTypeDefinition, state);

        /// <summary>
        /// Used to create the Proces() method returns object
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public IResult ResultCreate(int state = 1, string message = null) => ResultFactory.ResultCreate(ArgMeta.resultType, ArgMeta.resultTypeDefinition, state, message);

        /// <summary>
        /// Used to create the Proces() method returns object
        /// </summary>
        /// <typeparam name="Data"></typeparam>
        /// <param name="data"></param>
        /// <param name="message"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public IResult ResultCreate<Data>(Data data, string message = null, int state = 1) => ResultFactory.ResultCreate(ArgMeta.resultTypeDefinition, data, message, state);

        /// <summary>
        /// Used to create the Proces() method returns object
        /// </summary>
        /// <param name="data"></param>
        /// <param name="message"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public IResult ResultCreate(object data, string message = null, int state = 1) => ResultFactory.ResultCreate(ArgMeta.resultTypeDefinition, data, message, state);

        #endregion

        /// <summary>
        /// CheckNull
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="value"></param>
        /// <returns></returns>
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
                    return attribute.ResultCreate(attribute.State, attribute.Message ?? $"argument \"{attribute.Alias}\" can not null.");
                }
            }

            return attribute.ResultCreate(data: value);
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
        public CommandAttribute(string onlyName = null) { this.OnlyName = onlyName; }

        /// <summary>
        /// Key
        /// </summary>
        public string Key { get; internal set; }

        /// <summary>
        /// OnlyName
        /// </summary>
        public string OnlyName { get; set; }

        /// <summary>
        /// OnlyNameByte
        /// </summary>
        public byte OnlyNameByte { get; set; }

        //public bool IgnoreBusinessArg { get; set; }

        /// <summary>
        /// GroupKey
        /// </summary>
        /// <param name="space"></param>
        /// <returns></returns>
        public override string GroupKey(string space = "->") => $"{base.GroupKey(space)}{space}{this.OnlyName}";

        /// <summary>
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{this.Meta.Type.Name} {Group} {OnlyName}";
    }

    #region

    /// <summary>
    /// CheckNullAttribute
    /// </summary>
    public class CheckNullAttribute : ArgumentAttribute
    {
        /// <summary>
        /// CheckNullAttribute
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        public CheckNullAttribute(int state = -800, string message = null) : base(state, message) => this.CanNull = false;

        /// <summary>
        /// Proces
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override async ValueTask<IResult> Proces(dynamic value)
        {
            if (typeof(string).Equals(this.ArgMeta.MemberType))
            {
                if (string.IsNullOrEmpty(value))
                {
                    return this.ResultCreate(State, Message ?? $"argument \"{this.Alias}\" can not null.");
                }
            }
            else if (object.Equals(null, value))
            {
                return this.ResultCreate(State, Message ?? $"argument \"{ this.Alias}\" can not null.");
            }

            return this.ResultCreate();
        }
    }

    /// <summary>
    /// SizeAttribute
    /// </summary>
    public class SizeAttribute : ArgumentAttribute
    {
        /// <summary>
        /// SizeAttribute
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
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

        /// <summary>
        /// Min
        /// </summary>
        public object Min { get; set; }
        /// <summary>
        /// Max
        /// </summary>
        public object Max { get; set; }

        /// <summary>
        /// MinMsg
        /// </summary>
        public string MinMsg { get; set; } = "argument \"{Alias}\" minimum range {Min}.";
        /// <summary>
        /// MaxMsg
        /// </summary>
        public string MaxMsg { get; set; } = "argument \"{Alias}\" maximum range {Max}.";

        /// <summary>
        /// Proces
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override async ValueTask<IResult> Proces(dynamic value)
        {
            var result = CheckNull(this, value);
            if (!result.HasData) { return result; }

            var type = System.Nullable.GetUnderlyingType(this.ArgMeta.MemberType) ?? this.ArgMeta.MemberType;

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
                        return this.ResultCreate(State, $"argument \"{this.Alias}\" type error");
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

    /// <summary>
    /// ScaleAttribute
    /// </summary>
    public class ScaleAttribute : ArgumentAttribute
    {
        /// <summary>
        /// ScaleAttribute
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        public ScaleAttribute(int state = -802, string message = null) : base(state, message) { }

        /// <summary>
        /// Size
        /// </summary>
        public int Size { get; set; } = 2;

        /// <summary>
        /// Proces
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override async ValueTask<IResult> Proces(dynamic value)
        {
            var result = CheckNull(this, value);
            if (!result.HasData) { return result; }

            switch (this.ArgMeta.MemberType.GetTypeCode())
            {
                case System.TypeCode.Decimal:
                    return this.ResultCreate(Help.Scale((decimal)value, Size));
                case System.TypeCode.Double:
                    return this.ResultCreate(Help.Scale((double)value, Size));
                default: return this.ResultCreate(State, Message ?? $"argument \"{this.Alias}\" type error");
            }
        }
    }

    /// <summary>
    /// https://github.com/Microsoft/referencesource/blob/master/System.ComponentModel.DataAnnotations/DataAnnotations/EmailAddressAttribute.cs
    /// </summary>
    public class CheckEmailAttribute : ArgumentAttribute
    {
        /// <summary>
        /// CheckEmailAttribute
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        public CheckEmailAttribute(int state = -803, string message = null) : base(state, message) { }

        /// <summary>
        /// Proces
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override async ValueTask<IResult> Proces(dynamic value)
        {
            var result = CheckNull(this, value);
            if (!result.HasData) { return result; }

            //if (!Utils.Help.CheckEmail(value))
            if (!IsValid(value))
            {
                return this.ResultCreate(State, Message ?? $"argument \"{this.Alias}\" email error");
            }
            return this.ResultCreate();
        }

        bool IsValid(dynamic value)
        {
            if (value == null)
            {
                return true;
            }

            string valueAsString = value as string;

            // Use RegEx implementation if it has been created, otherwise use a non RegEx version.
            return valueAsString != null && _regex.Match(valueAsString).Length > 0;
        }

        // This attribute provides server-side email validation equivalent to jquery validate,
        // and therefore shares the same regular expression.  See unit tests for examples.
        private static System.Text.RegularExpressions.Regex _regex = CreateRegEx();

        private static System.Text.RegularExpressions.Regex CreateRegEx()
        {
            const string pattern = @"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$";
            const System.Text.RegularExpressions.RegexOptions options = System.Text.RegularExpressions.RegexOptions.Compiled | System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.ExplicitCapture;

            // Set explicit regex match timeout, sufficient enough for email parsing
            // Unless the global REGEX_DEFAULT_MATCH_TIMEOUT is already set
            var matchTimeout = System.TimeSpan.FromSeconds(2);

            try
            {
                if (System.AppDomain.CurrentDomain.GetData("REGEX_DEFAULT_MATCH_TIMEOUT") == null)
                {
                    return new System.Text.RegularExpressions.Regex(pattern, options, matchTimeout);
                }
            }
            catch
            {
                // Fallback on error
            }

            // Legacy fallback (without explicit match timeout)
            return new System.Text.RegularExpressions.Regex(pattern, options);
        }
    }

    /// <summary>
    /// CheckCharAttribute
    /// </summary>
    public class CheckCharAttribute : ArgumentAttribute
    {
        /// <summary>
        /// CheckCharAttribute
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        public CheckCharAttribute(int state = -804, string message = null) : base(state, message) { }

        /// <summary>
        /// Mode
        /// </summary>
        public Help.CheckCharMode Mode { get; set; } = Help.CheckCharMode.All;

        /// <summary>
        /// Proces
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override async ValueTask<IResult> Proces(dynamic value)
        {
            var result = CheckNull(this, value);
            if (!result.HasData) { return result; }

            if (!Utils.Help.CheckChar(value, Mode))
            {
                return this.ResultCreate(State, Message ?? $"argument \"{this.Alias}\" char verification failed");
            }
            return this.ResultCreate();
        }
    }

    /// <summary>
    /// Indicates whether the specified regular expression finds a match in the specified input string, using the specified matching options.
    /// https://github.com/microsoft/referencesource/blob/master/System.ComponentModel.DataAnnotations/DataAnnotations/RegularExpressionAttribute.cs
    /// </summary>
    public class RegexAttribute : ArgumentAttribute
    {
        /// <summary>
        /// Indicates whether the specified regular expression finds a match in the specified input string, using the specified matching options.
        /// </summary>
        /// <param name="pattern">The regular expression pattern to match.</param>
        /// <param name="state"></param>
        /// <param name="message"></param>
        public RegexAttribute(string pattern, int state = -805, string message = null) : base(state, message)
        {
            this.Pattern = pattern;
            //this.Options = options;
        }

        /// <summary>
        /// A bitwise combination of the enumeration values that provide options for matching.
        /// </summary>
        //public System.Text.RegularExpressions.RegexOptions Options { get; set; } = System.Text.RegularExpressions.RegexOptions.None;

        public override async ValueTask<IResult> Proces(dynamic value)
        {
            var result = CheckNull(this, value);
            if (!result.HasData) { return result; }

            //if (!System.Text.RegularExpressions.Regex.IsMatch(value.Trim(), Pattern, Options))
            if (!IsValid(value))
            {
                return this.ResultCreate(State, Message ?? $"argument \"{this.Alias}\" regular expression verification failed");
            }

            return this.ResultCreate();
        }

        bool IsValid(object value)
        {
            this.SetupRegex();

            // Convert the value to a string
            string stringValue = System.Convert.ToString(value, System.Globalization.CultureInfo.CurrentCulture);

            // Automatically pass if value is null or empty. RequiredAttribute should be used to assert a value is not empty.
            if (string.IsNullOrEmpty(stringValue))
            {
                return true;
            }

            var m = this.Regex.Match(stringValue);

            // We are looking for an exact match, not just a search hit. This matches what
            // the RegularExpressionValidator control does
            return (m.Success && m.Index == 0 && m.Length == stringValue.Length);
        }

        /// <summary>
        /// Gets the regular expression pattern to use
        /// </summary>
        public string Pattern { get; private set; }

        /// <summary>
        ///     Gets or sets the timeout to use when matching the regular expression pattern (in milliseconds)
        ///     (-1 means never timeout).
        /// </summary>
        public int MatchTimeoutInMilliseconds
        {
            get
            {
                return _matchTimeoutInMilliseconds;
            }
            set
            {
                _matchTimeoutInMilliseconds = value;
                _matchTimeoutSet = true;
            }
        }

        private int _matchTimeoutInMilliseconds;
        private bool _matchTimeoutSet;

        private System.Text.RegularExpressions.Regex Regex { get; set; }

        /// <summary>
        /// Sets up the <see cref="Regex"/> property from the <see cref="Pattern"/> property.
        /// </summary>
        /// <exception cref="ArgumentException"> is thrown if the current <see cref="Pattern"/> cannot be parsed</exception>
        /// <exception cref="InvalidOperationException"> is thrown if the current attribute is ill-formed.</exception>
        /// <exception cref="ArgumentOutOfRangeException"> thrown if <see cref="MatchTimeoutInMilliseconds" /> is negative (except -1),
        /// zero or greater than approximately 24 days </exception>
        private void SetupRegex()
        {
            if (this.Regex == null)
            {
                if (string.IsNullOrEmpty(this.Pattern))
                {
                    throw new System.ArgumentException(this.Pattern);
                }

                if (!_matchTimeoutSet)
                {
                    MatchTimeoutInMilliseconds = GetDefaultTimeout();
                }

                Regex = MatchTimeoutInMilliseconds == -1
                    ? new System.Text.RegularExpressions.Regex(Pattern)
                    : Regex = new System.Text.RegularExpressions.Regex(Pattern, default(System.Text.RegularExpressions.RegexOptions), System.TimeSpan.FromMilliseconds(MatchTimeoutInMilliseconds));
            }
        }

        /// <summary>
        /// Returns the default MatchTimeout based on UseLegacyRegExTimeout switch.
        /// </summary>
        private static int GetDefaultTimeout()
        {
            return 2000;
        }
    }

    /// <summary>
    /// https://github.com/microsoft/referencesource/blob/master/System.ComponentModel.DataAnnotations/DataAnnotations/PhoneAttribute.cs
    /// </summary>
    public class CheckPhoneAttribute : ArgumentAttribute
    {
        /// <summary>
        /// CheckPhoneAttribute
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        public CheckPhoneAttribute(int state = -806, string message = null) : base(state, message) { }

        /// <summary>
        /// Proces
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override async ValueTask<IResult> Proces(dynamic value)
        {
            var result = CheckNull(this, value);
            if (!result.HasData) { return result; }

            if (!IsValid(value))
            {
                return this.ResultCreate(State, Message ?? $"argument \"{this.Alias}\" phone error");
            }
            return this.ResultCreate();
        }

        bool IsValid(dynamic value)
        {
            if (value == null)
            {
                return true;
            }

            string valueAsString = value as string;

            // Use RegEx implementation if it has been created, otherwise use a non RegEx version.
            return valueAsString != null && _regex.Match(valueAsString).Length > 0;
        }

        private static System.Text.RegularExpressions.Regex _regex = CreateRegEx();

        private static System.Text.RegularExpressions.Regex CreateRegEx()
        {
            const string pattern = @"^(\+\s?)?((?<!\+.*)\(\+?\d+([\s\-\.]?\d+)?\)|\d+)([\s\-\.]?(\(\d+([\s\-\.]?\d+)?\)|\d+))*(\s?(x|ext\.?)\s?\d+)?$";
            const System.Text.RegularExpressions.RegexOptions options = System.Text.RegularExpressions.RegexOptions.Compiled | System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.ExplicitCapture;

            // Set explicit regex match timeout, sufficient enough for phone parsing
            // Unless the global REGEX_DEFAULT_MATCH_TIMEOUT is already set
            var matchTimeout = System.TimeSpan.FromSeconds(2);

            try
            {
                if (System.AppDomain.CurrentDomain.GetData("REGEX_DEFAULT_MATCH_TIMEOUT") == null)
                {
                    return new System.Text.RegularExpressions.Regex(pattern, options, matchTimeout);
                }
            }
            catch
            {
                // Fallback on error
            }

            // Legacy fallback (without explicit match timeout)
            return new System.Text.RegularExpressions.Regex(pattern, options);
        }
    }

    /// <summary>
    /// https://github.com/microsoft/referencesource/blob/master/System.ComponentModel.DataAnnotations/DataAnnotations/UrlAttribute.cs
    /// https://stackoverflow.com/questions/45707293/url-validation-attribute-marks-localhost-as-invalid-url
    /// </summary>
    public class CheckUrlAttribute : ArgumentAttribute
    {
        /// <summary>
        /// CheckUrlAttribute
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        public CheckUrlAttribute(int state = -807, string message = null) : base(state, message) { }

        /// <summary>
        /// Proces
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override async ValueTask<IResult> Proces(dynamic value)
        {
            var result = CheckNull(this, value);
            if (!result.HasData) { return result; }

            if (!IsValid(value))
            {
                return this.ResultCreate(State, Message ?? $"argument \"{this.Alias}\" url error");
            }
            return this.ResultCreate();
        }

        bool IsValid(object value)
        {
            if (value == null)
            {
                return true;
            }

            string valueAsString = value as string;

            // Use RegEx implementation if it has been created, otherwise use a non RegEx version.
            return valueAsString != null && _regex.Match(valueAsString).Length > 0;
        }

        // This attribute provides server-side url validation equivalent to jquery validate,
        // and therefore shares the same regular expression.  See unit tests for examples.
        private static System.Text.RegularExpressions.Regex _regex = CreateRegEx();

        private static System.Text.RegularExpressions.Regex CreateRegEx()
        {
            const string pattern = @"^(https?|ftp):\/\/(((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:)*@)?(((\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5]))|((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?)(:\d*)?)(\/((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)+(\/(([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)*)*)?)?(\?((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)|[\uE000-\uF8FF]|\/|\?)*)?(\#((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)|\/|\?)*)?$";

            const System.Text.RegularExpressions.RegexOptions options = System.Text.RegularExpressions.RegexOptions.Compiled | System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.ExplicitCapture;

            // Set explicit regex match timeout, sufficient enough for url parsing
            // Unless the global REGEX_DEFAULT_MATCH_TIMEOUT is already set
            var matchTimeout = System.TimeSpan.FromSeconds(2);

            try
            {
                if (System.AppDomain.CurrentDomain.GetData("REGEX_DEFAULT_MATCH_TIMEOUT") == null)
                {
                    return new System.Text.RegularExpressions.Regex(pattern, options, matchTimeout);
                }
            }
            catch
            {
                // Fallback on error
            }

            // Legacy fallback (without explicit match timeout)
            return new System.Text.RegularExpressions.Regex(pattern, options);
        }
    }

    /// <summary>
    /// MD5Attribute
    /// </summary>
    public class MD5Attribute : ArgumentAttribute
    {
        /// <summary>
        /// MD5Attribute
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        public MD5Attribute(int state = -820, string message = null) : base(state, message) { }

        /// <summary>
        /// Encoding
        /// </summary>
        public System.Text.Encoding Encoding { get; set; }

        /// <summary>
        /// HasUpper
        /// </summary>
        public bool HasUpper { get; set; }

        /// <summary>
        /// Proces
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
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
    public class AESAttribute : ArgumentAttribute
    {
        /// <summary>
        /// key
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// salt
        /// </summary>
        public string Salt { get; set; }

        /// <summary>
        /// Encoding
        /// </summary>
        public System.Text.Encoding Encoding { get; set; }

        /// <summary>
        /// AES return to item1=Data and item2=Salt
        /// </summary>
        /// <param name="key"></param>
        /// <param name="state"></param>
        /// <param name="message"></param>
        public AESAttribute(string key = null, int state = -821, string message = null) : base(state, message) => this.Key = key;

        /// <summary>
        /// Proces
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override async ValueTask<IResult> Proces(dynamic value)
        {
            var result = CheckNull(this, value);
            if (!result.HasData) { return result; }

            return this.ResultCreate(Help.AES.Encrypt(value, Key, Salt, Encoding));
        }

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="password"></param>
        /// <param name="encryptData"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        public bool Equals(string password, string encryptData, string salt)
        {
            try
            {
                return Equals(password, Help.AES.Decrypt(encryptData, Key, salt, Encoding));
            }
            catch
            {
                return false;
            }
        }
    }

    #endregion

    #region Deserialize

    /// <summary>
    /// ArgumentDefaultAttribute
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.All, AllowMultiple = false)]
    public sealed class ArgumentDefaultAttribute : ArgumentAttribute
    {
        /// <summary>
        /// ArgumentDefaultAttribute
        /// </summary>
        /// <param name="resultType"></param>
        /// <param name="resultTypeDefinition"></param>
        /// <param name="argTypeDefinition"></param>
        /// <param name="state"></param>
        /// <param name="message"></param>
        internal ArgumentDefaultAttribute(System.Type resultType, System.Type resultTypeDefinition, System.Type argTypeDefinition, int state = -11, string message = null) : base(state, message)
        {
            this.ArgMeta.resultType = resultType;
            this.ArgMeta.resultTypeDefinition = resultTypeDefinition;
            this.ArgMeta.argTypeDefinition = argTypeDefinition;
        }

        /// <summary>
        /// ArgumentDefaultAttribute
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        public ArgumentDefaultAttribute(int state = -11, string message = null) : base(state, message) { }
    }

    /// <summary>
    /// ParametersAttribute
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class ParametersAttribute : ArgumentAttribute
    {
        /// <summary>
        /// ParametersAttribute
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        public ParametersAttribute(int state = -11, string message = null) : base(state, message)
        {
            this.CanNull = false;
            this.Description = "Parameters parsing";
            this.ArgMeta.Skip = (bool hasUse, bool hasDefinition, AttributeBase.MetaData.DeclaringType declaring, System.Collections.Generic.IEnumerable<ArgumentAttribute> arguments) => !hasDefinition;
        }

        /// <summary>
        /// Proces
        /// </summary>
        /// <typeparam name="Type"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public override async ValueTask<IResult> Proces<Type>(dynamic value)
        {
            var result = CheckNull(this, value);
            if (!result.HasData) { return result; }

            try
            {
                System.Collections.Generic.IDictionary<string, string> dict = value;

                if (0 < dict.Count && 0 < this.ArgMeta.Arg.Children?.Count)
                {
                    var arg = System.Activator.CreateInstance<Type>();
                    //var dict2 = new System.Collections.Generic.Dictionary<string, object>(dict.Count);

                    foreach (var item in this.ArgMeta.Arg.Children)
                    {
                        if (dict.TryGetValue(item.Name, out string v))
                        {
                            if (!item.HasDefinition)
                            {
                                var v2 = Help.ChangeType(v, item.OrigType);
                                item.Accessor.Setter(arg, v2);
                            }
                            else if (typeof(IArg).IsAssignableFrom(item.OrigType))
                            {
                                var iarg = System.Activator.CreateInstance(this.ArgMeta.argTypeDefinition.MakeGenericType(item.CurrentOrigType)) as IArg;
                                iarg.In = v;
                                item.Accessor.Setter(arg, iarg);
                            }
                            //dict2.Add(item.Name, v2);
                        }
                    }

                    //return this.ResultCreate(dict2.JsonSerialize().TryJsonDeserialize<Type>());
                    return this.ResultCreate(arg);
                }

                return this.ResultCreate<Type>(default);
            }
            catch (System.Exception ex) { return this.ResultCreate(State, Message ?? $"Parameters {this.Alias} Proces error. {ex.Message}"); }
        }
    }

    /// <summary>
    /// System.Text.Json.JsonSerializer.Deserialize
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Assembly | System.AttributeTargets.Method | System.AttributeTargets.Class | System.AttributeTargets.Struct | System.AttributeTargets.Property | System.AttributeTargets.Field | System.AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class JsonArgAttribute : ArgumentAttribute
    {
        /// <summary>
        /// JsonArgAttribute
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        public JsonArgAttribute(int state = -12, string message = null) : base(state, message)
        {
            this.CanNull = false;
            this.Description = "Json parsing";
            //this.ArgMeta.Filter |= FilterModel.NotDefinition;
            this.ArgMeta.Skip = (bool hasUse, bool hasDefinition, AttributeBase.MetaData.DeclaringType declaring, System.Collections.Generic.IEnumerable<ArgumentAttribute> arguments) => !hasDefinition || this.ArgMeta.Arg.Parameters;

            options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                IgnoreNullValues = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            options.Converters.Add(new Help.DateTimeConverter());
            options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        }

        /// <summary>
        /// options
        /// </summary>
        public System.Text.Json.JsonSerializerOptions options;

        /// <summary>
        /// Proces
        /// </summary>
        /// <typeparam name="Type"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public override async ValueTask<IResult> Proces<Type>(dynamic value)
        {
            var result = CheckNull(this, value);
            if (!result.HasData) { return result; }

            if (this.ArgMeta.MemberType.Equals(value.GetType()))
            {
                return this.ResultCreate(value);
            }

            try
            {
                return this.ResultCreate(System.Text.Json.JsonSerializer.Deserialize<Type>(value, options));
            }
            catch (System.Exception ex) { return this.ResultCreate(State, Message ?? $"Arguments {this.Alias} Json deserialize error. {ex.Message}"); }
        }
    }

    /// <summary>
    /// Simple asp.net HTTP request file attribute
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method | System.AttributeTargets.Class | System.AttributeTargets.Struct | System.AttributeTargets.Property | System.AttributeTargets.Field | System.AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public abstract class HttpFileAttribute : ArgumentAttribute
    {
        /// <summary>
        /// Simple asp.net HTTP request file attribute
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        public HttpFileAttribute(int state = 830, string message = null) : base(state, message) { }
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
