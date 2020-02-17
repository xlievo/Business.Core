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

    public abstract class AttributeBase : System.Attribute
    {
        #region MetaData

        public class MetaData
        {
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
            public System.Type Type { get; internal set; }

            /// <summary>
            /// Declare the source of this feature
            /// </summary>
            public DeclaringType Declaring { get; internal set; }

            public void Clone(MetaData metaData)
            {
                this.Declaring = metaData.Declaring;
            }
        }

        internal static readonly ConcurrentReadOnlyDictionary<string, Accessors> Accessors = new ConcurrentReadOnlyDictionary<string, Accessors>();

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
                item.Meta.Declaring = MetaData.DeclaringType.Class;
            }

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

        internal static System.Collections.Generic.List<AttributeBase> GetAttributes(ParameterInfo member, System.Type outType, System.Type orgType = null)
        {
            var attributes = new System.Collections.Generic.List<AttributeBase>(member.GetAttributes<AttributeBase>());
            attributes.AddRange(member.ParameterType.GetAttributes<AttributeBase>());
            if (!member.ParameterType.Equals(outType))
            {
                attributes.AddRange(outType.GetAttributes<AttributeBase>());
            }
            if (null != orgType && !outType.Equals(orgType))
            {
                attributes.AddRange(orgType.GetAttributes<AttributeBase>());
            }
            attributes.ForEach(c => c.Meta.Declaring = MetaData.DeclaringType.Parameter);
            return attributes;
        }

        internal static System.Collections.Generic.List<ArgumentAttribute> GetCollectionAttributes(System.Type type)
        {
            var attributes = new System.Collections.Generic.List<ArgumentAttribute>(type.GetAttributes<ArgumentAttribute>());
            attributes.ForEach(c =>
            {
                c.Meta.Declaring = MetaData.DeclaringType.Parameter;
                c.CollectionItem = true;
            });
            return attributes;
        }
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

        public AttributeBase()
        {
            this.Meta = new MetaData { Type = this.TypeId as System.Type };

            this.Meta.Type.LoadAccessors(Accessors, methods: true);
        }

        #region

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

    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class TestingAttribute : GropuAttribute
    {
        /// <summary>
        /// regression testing
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <param name="token"></param>
        /// <param name="tokenMethod">Support method Result.D, input json array [\"Login\",\"{User:\\\"aaa\\\",Password:\\\"123456\\\"}\"]<</param>
        /// <param name="method"></param>
        public TestingAttribute(string name, object value, string result = null, string token = null, string tokenMethod = null)
        {
            this.Name = name;
            this.Value = value;
            this.Result = result;
            this.Token = token;
            this.TokenMethod = tokenMethod;
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

        /// <summary>
        /// Support method Result.D, input json array [\"Login\",\"{User:\\\"aaa\\\",Password:\\\"123456\\\"}\"]
        /// </summary>
        public string TokenMethod { get; set; }

        /// <summary>
        /// Target method
        /// </summary>
        public string Method { get; set; }

        public override string GroupKey(string space = "->") => $"{base.GroupKey(space)}{space}{this.Method}{space}{this.Name}";
    }

    public enum IgnoreMode
    {
        /// <summary>
        /// Ignore business methods
        /// </summary>
        Method,
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

    /* 1:Ignore method 2,3:Ignore document 4:Ignore Business ArgAttr */
    /// <summary>
    /// The Method and Property needs to be ignored and will not be a proxy
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Assembly | System.AttributeTargets.Class | System.AttributeTargets.Method | System.AttributeTargets.Property | System.AttributeTargets.Parameter | System.AttributeTargets.Field | System.AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
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

        /// <summary>
        /// Business Friendly Name
        /// </summary>
        public string BusinessName { get; internal set; }

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

        public virtual string GetCommandGroup(string group, string name) => string.IsNullOrWhiteSpace(group) ? name : $"{group}.{name}";
    }

    public struct UseEntry
    {
        public UseEntry(object value, params string[] parameterName)
        {
            this.Value = value;

            this.Type = this.Value?.GetType();

            this.ParameterName = parameterName;
        }

        public System.Type Type { get; }

        public object Value { get; }

        public string[] ParameterName { get; }
    }

    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct | System.AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public sealed class UseAttribute : AttributeBase
    {
        /// <summary>
        /// Injecting Objects Corresponding to Parameters
        /// </summary>
        /// <param name="parameterName">Use parameter names to correspond to injection objects</param>
        public UseAttribute(bool parameterName = false) => this.ParameterName = parameterName;

        /// <summary>
        /// Use parameter names to correspond to injection objects
        /// </summary>
        public bool ParameterName { get; private set; }

        public bool Token { get; set; }
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
        public LoggerAttribute(Logger.LoggerType logType = Logger.LoggerType.All, bool canWrite = true)
        {
            this.LogType = logType;
            this.CanWrite = canWrite;
        }

        /// <summary>
        /// Record type
        /// </summary>
        public Logger.LoggerType LogType { get; private set; }
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

        public LoggerAttribute SetType(Logger.LoggerType logType)
        {
            this.LogType = logType;
            return this;
        }

        //public static System.Collections.Generic.IEqualityComparer<LoggerAttribute> Comparer = Equality<LoggerAttribute>.CreateComparer(c => c.LogType);

        //public override T Clone<T>() => new LoggerAttribute(this.LogType) { Group = this.Group, CanResult = this.CanResult, CanValue = this.CanValue, CanWrite = this.CanWrite } as T;

        //public override string Key(string space = "->") => string.Format("{1}{0}{2}", space, this.Mode.GetName(), this.Group.Trim());
        public override string GroupKey(string space = "->") => $"{ base.GroupKey(space)}{space}{this.LogType.GetName()}";

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

    public abstract class GropuAttribute : AttributeBase
    {
        string group = string.Empty;

        /// <summary>
        /// Used for the command group
        /// </summary>
        public string Group { get => group; set => group = value?.Trim() ?? string.Empty; }

        public virtual string GroupKey(string space = "->") => $"{this.Meta.Type.FullName}{space}{this.Group.Trim()}";

        public static System.Collections.Generic.IEqualityComparer<GropuAttribute> Comparer { get => Equality<GropuAttribute>.CreateComparer(c => c.GroupKey(), System.StringComparer.CurrentCultureIgnoreCase); }
    }

    /// <summary>
    /// Base class for all attributes that apply to parameters
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Assembly | System.AttributeTargets.Method | System.AttributeTargets.Class | System.AttributeTargets.Struct | System.AttributeTargets.Property | System.AttributeTargets.Field | System.AttributeTargets.Parameter, AllowMultiple = true, Inherited = true)]
    public abstract class ArgumentAttribute : GropuAttribute
    {
        public new class MetaData
        {
            internal System.Type resultType;
            internal System.Type resultTypeDefinition;

            //public dynamic Business { get; internal set; }

            /// <summary>
            /// Declaring
            /// </summary>
            public string Business { get; internal set; }

            /// <summary>
            /// Business Friendly Name
            /// </summary>
            public string BusinessName { get; internal set; }

            public string Method { get; internal set; }

            public string MethodOnlyName { get; internal set; }

            public string MemberPath { get; internal set; }

            public string Member { get; internal set; }

            public System.Type MemberType { get; internal set; }

            public Proces Proces { get; internal set; }

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
                this.Proces = new Proces(metaData.Proces.MethodInfo, metaData.Proces.ParameterType)
                {
                    Mode = metaData.Proces.Mode,
                    Call = metaData.Proces.Call
                };
            }
        }

        public override T Clone<T>()
        {
            var clone = base.Clone<ArgumentAttribute>();
            clone.ArgMeta.Clone(ArgMeta);
            return clone as T;
        }

        struct ProcesMethod
        {
            public System.Type[] proces;
            public System.Type[] procesCollection;
        }

        static readonly ProcesMethod procesMethod = new ProcesMethod
        {
            proces = Help.GetMethod<ArgumentAttribute>(c => c.Proces(null)).GetParameters().Select(c => c.ParameterType).ToArray(),
            procesCollection = Help.GetMethod<ArgumentAttribute>(c => c.Proces(null, -1, null)).GetParameters().Select(c => c.ParameterType).ToArray()
        };

        public ArgumentAttribute(int state, string message = null)
        {
            this.State = state;
            this.Message = message;
            //this.CanNull = canNull;
            this.ArgMeta = new MetaData();

            if (Accessors.TryGetValue(this.Meta.Type.FullName, out Accessors meta) && meta.Methods.TryGetValue("Proces", out Proces method))
            {
                this.ArgMeta.Proces = new Proces(method.MethodInfo, method.ParameterType);

                if (Enumerable.SequenceEqual(procesMethod.proces, method.ParameterType))
                {
                    this.ArgMeta.Proces.Mode = method.MethodInfo.IsGenericMethod ?  Utils.Proces.ProcesMode.ProcesGeneric : Utils.Proces.ProcesMode.Proces;
                }
                else if (Enumerable.SequenceEqual(procesMethod.procesCollection, method.ParameterType))
                {
                    this.ArgMeta.Proces.Mode = method.MethodInfo.IsGenericMethod ? Utils.Proces.ProcesMode.ProcesCollectionGeneric : Utils.Proces.ProcesMode.ProcesCollection;
                }
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

        public override string GroupKey(string space = "->") => $"{base.GroupKey(space)}{space}{this.CollectionItem}";

        public System.Action BindAfter { get; set; }

        public MetaData ArgMeta { get; }

        /// <summary>
        /// Whether to apply to each item of a set parameter
        /// </summary>
        public bool CollectionItem { get; set; }

        /// <summary>
        /// By checking the Allow null value, Default to true
        /// </summary>
        public bool CanNull { get; set; } = true;

        /// <summary>
        /// Apply to custom objects only
        /// </summary>
        public bool HasDefinition { get; set; }

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
        /// <typeparam name="Type"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual async ValueTask<IResult> Proces<Type>(dynamic value) => this.ResultCreate<dynamic>(value);

        /// <summary>
        /// Start processing the Parameter object, By this.ResultCreate() method returns
        /// </summary>
        /// <param name="value"></param>
        /// <param name="collectionIndex"></param>
        /// <param name="dictKey"></param>
        /// <returns></returns>
        public virtual async ValueTask<IResult> Proces(dynamic value, int collectionIndex, dynamic dictKey) => this.ResultCreate<dynamic>(value);

        /// <summary>
        /// Start processing the Parameter object, By this.ResultCreate() method returns
        /// </summary>
        /// <typeparam name="Type"></typeparam>
        /// <param name="value"></param>
        /// <param name="collectionIndex"></param>
        /// <param name="dictKey"></param>
        /// <returns></returns>
        public virtual async ValueTask<IResult> Proces<Type>(dynamic value, int collectionIndex, dynamic dictKey) => this.ResultCreate<dynamic>(value);

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
        /// <param name="state"></param>
        /// <returns></returns>
        public IResult ResultCreate<Data>(Data data, string message = null, int state = 1) => ResultFactory.ResultCreate(ArgMeta.resultTypeDefinition, data, message, state);

        /// <summary>
        /// Used to create the Proces() method returns object
        /// </summary>
        /// <typeparam name="Data"></typeparam>
        /// <param name="data"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public IResult ResultCreate(object data, string message = null, int state = 1) => ResultFactory.ResultCreate(ArgMeta.resultTypeDefinition, data, message, state);

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

        //public bool IgnoreBusinessArg { get; set; }

        public override string GroupKey(string space = "->") => $"{base.GroupKey(space)}{space}{this.OnlyName}";

        public override string ToString() => $"{this.Meta.Type.Name} {Group} {OnlyName}";
    }

    #region

    public class CheckNullAttribute : ArgumentAttribute
    {
        public CheckNullAttribute(int state = -800, string message = null) : base(state, message) => this.CanNull = false;

        public override async ValueTask<IResult> Proces(dynamic value)
        {
            if (typeof(string).Equals(this.ArgMeta.MemberType))
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

            switch (this.ArgMeta.MemberType.GetTypeCode())
            {
                case System.TypeCode.Decimal:
                    return this.ResultCreate(Help.Scale((decimal)value, Size));
                case System.TypeCode.Double:
                    return this.ResultCreate(Help.Scale((double)value, Size));
                default: return this.ResultCreate(State, Message ?? $"argument \"{this.Nick}\" type error");
            }
        }
    }

    /// <summary>
    /// https://github.com/Microsoft/referencesource/blob/master/System.ComponentModel.DataAnnotations/DataAnnotations/EmailAddressAttribute.cs
    /// </summary>
    public class CheckEmailAttribute : ArgumentAttribute
    {
        public CheckEmailAttribute(int state = -803, string message = null) : base(state, message) { }

        public override async ValueTask<IResult> Proces(dynamic value)
        {
            var result = CheckNull(this, value);
            if (!result.HasData) { return result; }

            //if (!Utils.Help.CheckEmail(value))
            if (!IsValid(value))
            {
                return this.ResultCreate(State, Message ?? $"argument \"{this.Nick}\" email error");
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
        /// <param name="options">A bitwise combination of the enumeration values that provide options for matching.</param>
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
                return this.ResultCreate(State, Message ?? $"argument \"{this.Nick}\" regular expression verification failed");
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
        public CheckPhoneAttribute(int state = -806, string message = null) : base(state, message) { }

        public override async ValueTask<IResult> Proces(dynamic value)
        {
            var result = CheckNull(this, value);
            if (!result.HasData) { return result; }

            if (!IsValid(value))
            {
                return this.ResultCreate(State, Message ?? $"argument \"{this.Nick}\" phone error");
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
        public CheckUrlAttribute(int state = -807, string message = null) : base(state, message) { }

        public override async ValueTask<IResult> Proces(dynamic value)
        {
            var result = CheckNull(this, value);
            if (!result.HasData) { return result; }

            if (!IsValid(value))
            {
                return this.ResultCreate(State, Message ?? $"argument \"{this.Nick}\" url error");
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

        public System.Text.Encoding Encoding { get; set; }

        public AESAttribute(string key = null, int state = -821, string message = null) : base(state, message) => this.Key = key;

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

    public sealed class ArgumentDefaultAttribute : ArgumentAttribute
    {
        internal protected ArgumentDefaultAttribute(System.Type resultType, System.Type resultTypeDefinition, int state = -11, string message = null) : base(state, message)
        {
            this.ArgMeta.resultType = resultType;
            this.ArgMeta.resultTypeDefinition = resultTypeDefinition;
        }

        public ArgumentDefaultAttribute(int state = -11, string message = null) : base(state, message) { }
    }

    //public class JsonArgAttribute : ArgumentAttribute
    //{
    //    public static Newtonsoft.Json.JsonSerializerSettings Settings = new Newtonsoft.Json.JsonSerializerSettings
    //    {
    //        ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
    //        ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
    //        DateFormatString = "yyyy-MM-dd HH:mm:ss",
    //        DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Local,
    //        NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
    //        Converters = new System.Collections.Generic.List<Newtonsoft.Json.JsonConverter> { new Newtonsoft.Json.Converters.StringEnumConverter() }
    //    };

    //    public JsonArgAttribute(int state = -12, string message = null) : base(state, message) => this.CanNull = false;

    //    public override async ValueTask<IResult> Proces(dynamic value)
    //    {
    //        var result = CheckNull(this, value);
    //        if (!result.HasData) { return result; }

    //        try
    //        {
    //            return this.ResultCreate(data: Newtonsoft.Json.JsonConvert.DeserializeObject(value?.ToString(), this.Meta.MemberType, Settings));
    //        }
    //        catch { return this.ResultCreate(State, Message ?? $"Arguments {this.Nick} Json deserialize error"); }
    //    }
    //}

    /// <summary>
    /// System.Text.Json.JsonSerializer.Deserialize
    /// </summary>
    public class JsonArgAttribute : ArgumentAttribute
    {
        public JsonArgAttribute(int state = -12, string message = null) : base(state, message)
        {
            this.CanNull = false;
            this.Description = "Json parsing";

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

        public System.Text.Json.JsonSerializerOptions options;

        public override async ValueTask<IResult> Proces<Type>(dynamic value)
        {
            //var value2 = value?.ToString();
            //var value2 = value?.GetRawText();
            var result = CheckNull(this, value);
            if (!result.HasData) { return result; }

            try
            {
                //return this.ResultCreate(System.Text.Json.JsonSerializer.Deserialize(value, this.ArgMeta.MemberType, options));
                return this.ResultCreate(System.Text.Json.JsonSerializer.Deserialize<Type>(value, options));
            }
            catch { return this.ResultCreate(State, Message ?? $"Arguments {this.Nick} Json deserialize error"); }
        }
    }

    /// <summary>
    /// Simple asp.net HTTP request file
    /// </summary>
    [HttpFile]
    public class HttpFile : System.Collections.Generic.Dictionary<string, dynamic> { }

    /// <summary>
    /// Simple asp.net HTTP request file attribute
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method | System.AttributeTargets.Class | System.AttributeTargets.Struct | System.AttributeTargets.Property | System.AttributeTargets.Field | System.AttributeTargets.Parameter, AllowMultiple = true, Inherited = true)]
    public class HttpFileAttribute : ArgumentAttribute
    {
        public HttpFileAttribute(int state = 830, string message = null) : base(state, message) { }

        public override async ValueTask<IResult> Proces(dynamic value)
        {
            var result = CheckNull(this, value);
            if (!result.HasData) { return result; }

            var files = new HttpFile();

            if (value.Request.HasFormContentType)
            {
                foreach (var item in value.HttpContext.Request.Form.Files)
                {
                    if (!files.ContainsKey(item.Name))
                    {
                        files.Add(item.Name, item);
                    }
                }
            }

            return this.ResultCreate(files);
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
