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

namespace Business.Core.Meta
{
    using Annotations;
    using Utils;

    #region Meta

    /// <summary>
    /// ArgGroup
    /// </summary>
    public class ArgGroup
    {
        /// <summary>
        /// ArgGroup
        /// </summary>
        /// <param name="path"></param>
        public ArgGroup(string path) => Path = path;

        /// <summary>
        /// ArgGroup
        /// </summary>
        /// <param name="ignore"></param>
        /// <param name="ignoreArg"></param>
        /// <param name="attrs"></param>
        /// <param name="alias"></param>
        /// <param name="path"></param>
        /// <param name="owner"></param>
        /// <param name="root"></param>
        /// <param name="httpFile"></param>
        /// <param name="deserialize"></param>
        public ArgGroup(ReadOnlyCollection<Ignore> ignore, bool ignoreArg, ReadOnlyCollection<ArgumentAttribute> attrs, string alias, string path, string owner, string root, bool httpFile, ArgumentAttribute deserialize)
        {
            Ignore = ignore;
            IgnoreArg = ignoreArg;
            Attrs = attrs;
            Path = path;
            Alias = alias;
            Owner = owner;
            Root = root;
            Logger = default;
            HttpFile = httpFile;
            Deserialize = deserialize;
        }

        /// <summary>
        /// Ignore
        /// </summary>
        public ReadOnlyCollection<Ignore> Ignore { get; }

        /// <summary>
        /// IgnoreArg
        /// </summary>
        public bool IgnoreArg { get; internal set; }

        //public ConcurrentLinkedList<ArgumentAttribute> Attrs { get; internal set; }
        /// <summary>
        /// Attrs
        /// </summary>
        public ReadOnlyCollection<ArgumentAttribute> Attrs { get; internal set; }

        /// <summary>
        /// Alias
        /// </summary>
        public string Alias { get; }

        /// <summary>
        /// Path
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Owner
        /// </summary>
        public string Owner { get; }

        /// <summary>
        /// Root
        /// </summary>
        public string Root { get; }

        /// <summary>
        /// Logger
        /// </summary>
        public MetaLogger Logger { get; internal set; }

        ///// <summary>
        ///// IArgInLogger
        ///// </summary>
        //public MetaLogger IArgInLogger { get; internal set; }

        /// <summary>
        /// HttpFile
        /// </summary>
        public bool HttpFile { get; }

        /// <summary>
        /// Deserialize
        /// </summary>
        public ArgumentAttribute Deserialize { get; }
    }

    /// <summary>
    /// MemberDefinitionCode
    /// </summary>
    public enum MemberDefinitionCode
    {
        /// <summary>
        /// No
        /// </summary>
        No,

        /// <summary>
        /// Definition
        /// </summary>
        Definition,

        /// <summary>
        /// Field
        /// </summary>
        Field,

        /// <summary>
        /// Property
        /// </summary>
        Property,
    }

    /// <summary>
    /// ITypeDefinition
    /// </summary>
    /// <typeparam name="TypeDefinition"></typeparam>
    public interface ITypeDefinition<TypeDefinition> where TypeDefinition : ITypeDefinition<TypeDefinition>
    {
        /// <summary>
        /// Name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Type
        /// </summary>
        System.Type Type { get; }

        /// <summary>
        /// LastType
        /// </summary>
        System.Type LastType { get; }

        /// <summary>
        /// Remove IArg Null type
        /// </summary>
        System.Type CurrentType { get; }

        /// <summary>
        /// HasDefinition
        /// </summary>
        bool HasDefinition { get; }

        /// <summary>
        /// HasCollection
        /// </summary>
        bool HasCollection { get; }

        /// <summary>
        /// HasDictionary
        /// </summary>
        bool HasDictionary { get; }

        //bool IsEnum { get; set; }

        //bool HasNumeric { get; set; }

        //string[] EnumNames { get; set; }

        //System.Array EnumValues { get; set; }

        //string Path { get; set; }

        //string Summary { get; set; }

        /// <summary>
        /// DefaultValue
        /// </summary>
        object DefaultValue { get; }

        /// <summary>
        /// Nullable
        /// </summary>
        bool Nullable { get; }

        /// <summary>
        /// FullName
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// MemberDefinition
        /// </summary>
        MemberDefinitionCode MemberDefinition { get; }

        /// <summary>
        /// HasToken
        /// </summary>
        bool HasToken { get; }

        /// <summary>
        /// HasDefaultValue
        /// </summary>
        bool HasDefaultValue { get; }

        /// <summary>
        /// Group
        /// </summary>
        ConcurrentReadOnlyDictionary<string, ArgGroup> Group { get; }

        /// <summary>
        /// Children
        /// </summary>
        ReadOnlyCollection<TypeDefinition> Children { get; }

        /// <summary>
        /// Childrens
        /// </summary>
        ReadOnlyCollection<TypeDefinition> Childrens { get; }

        /// <summary>
        /// Dynamic object, DocUI string type display
        /// </summary>
        bool HasDynamicObject { get; }
    }

    /// <summary>
    /// Argument
    /// </summary>
    public class Args : ITypeDefinition<Args>
    {
        //public override string ToString() => string.Format("{0} {1}", Group2, Name);

        //argChild
        /// <summary>
        /// Argument
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="lastType"></param>
        /// <param name="currentType"></param>
        /// <param name="position"></param>
        /// <param name="defaultValue"></param>
        /// <param name="hasDefaultValue"></param>
        /// <param name="defaultTypeValue"></param>
        /// <param name="hasDictionary"></param>
        /// <param name="hasCollection"></param>
        /// <param name="nullable"></param>
        /// <param name="accessor"></param>
        /// <param name="group"></param>
        /// <param name="children"></param>
        /// <param name="childrens"></param>
        /// <param name="hasLower"></param>
        /// <param name="hasDefinition"></param>
        /// <param name="use"></param>
        /// <param name="useType"></param>
        /// <param name="hasToken"></param>
        /// <param name="methodTypeFullName"></param>
        /// <param name="fullName"></param>
        /// <param name="memberDefinition"></param>
        /// <param name="hasDynamicObject"></param>
        public Args(string name, System.Type type, System.Type lastType, System.Type currentType, int position, object defaultValue, bool hasDefaultValue, object defaultTypeValue, bool hasDictionary, bool hasCollection, bool nullable, Accessor accessor, ConcurrentReadOnlyDictionary<string, ArgGroup> group, ReadOnlyCollection<Args> children, ReadOnlyCollection<Args> childrens, bool hasLower, bool hasDefinition, UseAttribute use, bool useType, bool hasToken, string methodTypeFullName, string fullName, MemberDefinitionCode memberDefinition, bool hasDynamicObject)
        {
            Name = name;
            Type = type;
            //OrigType = origType;
            LastType = lastType;
            //CurrentOrigType = currentOrigType;
            CurrentType = currentType;
            Position = position;
            HasDictionary = hasDictionary;
            HasCollection = hasCollection;
            //HasCollectionAttr = hasCollectionAttr;
            Nullable = nullable;
            //HasString = hasString;
            Accessor = accessor;
            Group = group;
            Children = children;
            Childrens = childrens;

            HasLower = hasLower;
            HasDefinition = hasDefinition;
            //HasIArg = hasIArg;
            //IArgOutType = iArgOutType;
            //IArgInType = iArgInType;
            //this.trim = trim;
            //Path = path;
            //Source = source;

            DefaultValue = defaultValue;// type.GetTypeInfo().IsValueType ? System.Activator.CreateInstance(type) : null;
            HasDefaultValue = hasDefaultValue;

            DefaultTypeValue = defaultTypeValue;
            //Logger = default;
            //IArgInLogger = default;
            //Group2 = group2;
            //Owner = owner;
            //Ignore = ignore;
            Use = use;
            UseType = useType;
            HasToken = hasToken;
            MethodTypeFullName = methodTypeFullName;
            FullName = fullName;
            MemberDefinition = memberDefinition;
            //HasCast = hasCast;
            HasDynamicObject = hasDynamicObject;
            HasObject = typeof(object).Equals(LastType);
        }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Type
        /// </summary>
        public System.Type Type { get; internal set; }

        ///// <summary>
        ///// OrigType
        ///// </summary>
        //public System.Type OrigType { get; }

        /// <summary>
        /// LastType
        /// </summary>
        public System.Type LastType { get; }

        ///// <summary>
        ///// Remove IArg type
        ///// </summary>
        //public System.Type CurrentOrigType { get; }

        /// <summary>
        /// Remove IArg Null type
        /// </summary>
        public System.Type CurrentType { get; }

        /// <summary>
        /// Position
        /// </summary>
        public int Position { get; }

        /// <summary>
        /// DefaultValue
        /// </summary>
        public object DefaultValue { get; }

        /// <summary>
        /// HasDefaultValue
        /// </summary>
        public bool HasDefaultValue { get; }

        /// <summary>
        /// DefaultTypeValue
        /// </summary>
        public object DefaultTypeValue { get; }

        /// <summary>
        /// HasDictionary
        /// </summary>
        public bool HasDictionary { get; }

        /// <summary>
        /// HasCollection
        /// </summary>
        public bool HasCollection { get; }
        //===============hasCollectionAttr==================//
        //public bool HasCollectionAttr { get; internal set; }

        /// <summary>
        /// Nullable
        /// </summary>
        public bool Nullable { get; }

        /// <summary>
        /// Accessor
        /// </summary>
        public Accessor Accessor { get; }

        /// <summary>
        /// Group
        /// </summary>
        public ConcurrentReadOnlyDictionary<string, ArgGroup> Group { get; internal set; }
        ////===============argAttr==================//
        //public SafeList<Attributes.ArgumentAttribute> ArgAttr { get; }

        /// <summary>
        /// Children
        /// </summary>
        public ReadOnlyCollection<Args> Children { get; }

        /// <summary>
        /// Childrens
        /// </summary>
        public ReadOnlyCollection<Args> Childrens { get; }

        /// <summary>
        /// Whether there are children
        /// </summary>
        public bool HasLower { get; internal set; }

        /// <summary>
        /// HasDefinition
        /// </summary>
        public bool HasDefinition { get; }

        ///// <summary>
        ///// IArgOutType
        ///// </summary>
        //public System.Type IArgOutType { get; }

        ///// <summary>
        ///// IArgInType
        ///// </summary>
        //public System.Type IArgInType { get; }
        ////==============path===================//
        //public string Path { get; }
        ////==============source===================//
        //public string Source { get; }

        ///// <summary>
        ///// HasIArg
        ///// </summary>
        //public bool HasIArg { get; internal set; }
        //public MetaLogger Logger { get; }
        //public MetaLogger IArgInLogger { get; }
        //==============group===================//
        //public string Group2 { get; }
        //==============owner===================//
        //public string Owner { get; }
        ////==============ignore===================//
        //public ReadOnlyCollection<Attributes.Ignore> Ignore { get; }

        /// <summary>
        /// Use
        /// </summary>
        public UseAttribute Use { get; internal set; }

        /// <summary>
        /// UseType
        /// </summary>
        public bool UseType { get; internal set; }

        /// <summary>
        /// HasToken
        /// </summary>
        public bool HasToken { get; }

        /// <summary>
        /// MethodTypeFullName
        /// </summary>
        public string MethodTypeFullName { get; }

        /// <summary>
        /// FullName
        /// </summary>
        public string FullName { get; }

        /// <summary>
        /// xml using
        /// </summary>
        public MemberDefinitionCode MemberDefinition { get; }

        ///// <summary>
        ///// HasCast
        ///// </summary>
        //public bool HasCast { get; internal set; }

        /// <summary>
        /// Parameters
        /// </summary>
        public bool Parameters { get; internal set; }

        /// <summary>
        /// Dynamic object, DocUI string type display
        /// </summary>
        public bool HasDynamicObject { get; }

        /// <summary>
        /// HasObject
        /// </summary>
        public bool HasObject { get; }
    }

    /// <summary>
    /// MetaLogger
    /// </summary>
    public struct MetaLogger
    {
        /// <summary>
        /// Record
        /// </summary>
        public LoggerAttribute Record { get; set; }

        /// <summary>
        /// Error
        /// </summary>
        public LoggerAttribute Error { get; set; }

        /// <summary>
        /// Exception
        /// </summary>
        public LoggerAttribute Exception { get; set; }
    }

    /// <summary>
    /// CommandGroup
    /// </summary>
    public readonly struct CommandGroup
    {
        /// <summary>
        /// CommandGroup
        /// </summary>
        /// <param name="group"></param>
        /// <param name="full"></param>
        public CommandGroup(ReadOnlyDictionary<string, CommandAttribute> group, ReadOnlyDictionary<string, ReadOnlyDictionary<string, CommandAttribute>> full) { Group = group; Full = full; }

        /// <summary>
        /// Group
        /// </summary>
        public ReadOnlyDictionary<string, CommandAttribute> Group { get; }

        /// <summary>
        /// Full
        /// </summary>
        public ReadOnlyDictionary<string, ReadOnlyDictionary<string, CommandAttribute>> Full { get; }
    }

    /// <summary>
    /// MetaData
    /// </summary>
    public readonly struct MetaData
    {
        /// <summary>
        /// Name
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Name;

        /// <summary>
        /// MetaData
        /// </summary>
        /// <param name="accessor"></param>
        /// <param name="commandGroup"></param>
        /// <param name="args"></param>
        /// <param name="argAll"></param>
        /// <param name="tokens"></param>
        /// <param name="metaLogger"></param>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <param name="fullName"></param>
        /// <param name="business"></param>
        /// <param name="hasAsync"></param>
        /// <param name="hasValueTask"></param>
        /// <param name="hasReturn"></param>
        /// <param name="hasIResult"></param>
        /// <param name="hasIResultGeneric"></param>
        /// <param name="returnType"></param>
        /// <param name="resultTypeDefinition"></param>
        /// <param name="resultType"></param>
        /// <param name="defaultValue"></param>
        /// <param name="attributes"></param>
        /// <param name="position"></param>
        /// <param name="groupDefault"></param>
        /// <param name="useTypePosition"></param>
        /// <param name="methodTypeFullName"></param>
        /// <param name="doc"></param>
        /// <param name="hasParameters"></param>
        public MetaData(System.Func<object, object[], object> accessor, CommandGroup commandGroup, ReadOnlyCollection<Args> args, ReadOnlyCollection<Args> argAll, ReadOnlyCollection<Args> tokens, ReadOnlyDictionary<string, MetaLogger> metaLogger, string path, string name, string fullName, string business, bool hasAsync, bool hasValueTask, bool hasReturn, bool hasIResult, bool hasIResultGeneric, System.Type returnType, System.Type resultTypeDefinition, System.Type resultType, object[] defaultValue, System.Collections.Generic.IReadOnlyDictionary<string, GroupAttribute> attributes, int position, string groupDefault, ConcurrentReadOnlyDictionary<int, System.Type> useTypePosition, string methodTypeFullName, DocAttribute doc, bool hasParameters)
        {
            Accessor = accessor;
            CommandGroup = commandGroup;
            Args = args;
            ArgAll = argAll;
            Tokens = tokens;
            MetaLogger = metaLogger;
            Path = path;
            Name = name;
            FullName = fullName;
            Business = business;
            //this.returnType = returnType;
            //this.hasAsync = Utils.Help.IsAssignableFrom(typeof(System.Threading.Tasks.Task<>).GetTypeInfo(), returnType, out System.Type[] arguments) || typeof(System.Threading.Tasks.Task).IsAssignableFrom(returnType);
            //typeof(void) != method.ReturnType
            HasAsync = hasAsync;// Help.IsAssignableFrom(typeof(System.Threading.Tasks.Task<>), returnType, out System.Type[] arguments) || returnType == typeof(System.Threading.Tasks.Task);
            HasValueTask = hasValueTask;
            HasReturn = hasReturn;// !(typeof(void) == returnType || (HasAsync && null == asyncArguments));
            //typeof(IResult).IsAssignableFrom(method.ReturnType),
            //typeof(System.Object).Equals(method.ReturnType)
            //var hasAsyncGeneric = HasAsync && null != asyncArguments;
            HasIResult = hasIResult;// typeof(Result.IResult<>).IsAssignableFrom(hasAsyncGeneric ? asyncArguments[0] : returnType, out System.Type[] resultGeneric) || typeof(Result.IResult).IsAssignableFrom(hasAsyncGeneric ? asyncArguments[0] : returnType);
            HasIResultGeneric = hasIResultGeneric;
            //HasObject = hasObject;// typeof(object).Equals(hasAsyncGeneric ? asyncArguments[0] : returnType);
            ReturnType = returnType;// hasAsyncGeneric ? asyncArguments[0] : returnType;
            ReturnValueTaskType = hasReturn ? typeof(System.Threading.Tasks.ValueTask<>).MakeGenericType(returnType) : null;
            HasObject = typeof(object).Equals(returnType);
            ResultTypeDefinition = resultTypeDefinition;
            //ReturnResult = HasIResult ? resultType.MakeGenericType(null != resultGeneric ? resultGeneric[0] : typeof(string)) : null;
            ResultType = resultType;// resultType.MakeGenericType(HasIResult && null != resultGeneric ? resultGeneric[0] : typeof(string));
            ResultGeneric = resultType.GenericTypeArguments[0];

            DefaultValue = defaultValue;
            //this.logAttrs = logAttrs;
            Attributes = attributes;
            Position = position;
            GroupDefault = groupDefault;
            //ArgsFirst = argsFirst;
            UseTypePosition = useTypePosition;
            MethodTypeFullName = methodTypeFullName;
            //Ignore = ignore;
            //HasArgSingle = hasArgSingle;
            Doc = doc;
            HasParameters = hasParameters;
        }

        /// <summary>
        /// Accessor
        /// </summary>
        public System.Func<object, object[], object> Accessor { get; }

        /// <summary>
        /// CommandGroup
        /// </summary>
        public CommandGroup CommandGroup { get; }

        /// <summary>
        /// Args
        /// </summary>
        public ReadOnlyCollection<Args> Args { get; }

        /// <summary>
        /// ArgAll
        /// </summary>
        public ReadOnlyCollection<Args> ArgAll { get; }

        /// <summary>
        /// Tokens
        /// </summary>
        public ReadOnlyCollection<Args> Tokens { get; }

        /// <summary>
        /// MetaLogger
        /// </summary>
        public ReadOnlyDictionary<string, MetaLogger> MetaLogger { get; }

        /// <summary>
        /// Path
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// FullName
        /// </summary>
        public string FullName { get; }

        /// <summary>
        /// Business
        /// </summary>
        public string Business { get; }

        /// <summary>
        /// HasReturn
        /// </summary>
        public bool HasReturn { get; }

        /// <summary>
        /// HasIResult
        /// </summary>
        public bool HasIResult { get; }

        /// <summary>
        /// HasIResultGeneric
        /// </summary>
        public bool HasIResultGeneric { get; }

        /// <summary>
        /// HasObject
        /// </summary>
        public bool HasObject { get; }

        /// <summary>
        /// ReturnType
        /// </summary>
        public System.Type ReturnType { get; }

        /// <summary>
        /// ReturnValueTaskType
        /// </summary>
        public System.Type ReturnValueTaskType { get; }

        /// <summary>
        /// ResultTypeDefinition
        /// </summary>
        public System.Type ResultTypeDefinition { get; }

        /// <summary>
        /// ResultType
        /// </summary>
        public System.Type ResultType { get; }

        /// <summary>
        /// ResultGeneric
        /// </summary>
        public System.Type ResultGeneric { get; }

        /// <summary>
        /// HasAsync
        /// </summary>
        public bool HasAsync { get; }

        /// <summary>
        /// HasValueTask
        /// </summary>
        public bool HasValueTask { get; }

        /// <summary>
        /// DefaultValue
        /// </summary>
        public object[] DefaultValue { get; }

        /// <summary>
        /// Attributes
        /// </summary>
        public System.Collections.Generic.IReadOnlyDictionary<string, GroupAttribute> Attributes { get; }

        /// <summary>
        /// Position
        /// </summary>
        public int Position { get; }

        /// <summary>
        /// GroupDefault
        /// </summary>
        public string GroupDefault { get; }
        ////==============argsFirst===================//
        //public ReadOnlyCollection<Args> ArgsFirst { get; }

        /// <summary>
        /// UseTypePosition
        /// </summary>
        public ConcurrentReadOnlyDictionary<int, System.Type> UseTypePosition { get; }

        /// <summary>
        /// MethodTypeFullName
        /// </summary>
        public string MethodTypeFullName { get; }
        ////==============ignore===================//
        //public Attributes.Ignore Ignore { get; }
        //==============hasArgSingle===================//
        //public bool HasArgSingle { get; internal set; }

        /// <summary>
        /// Doc
        /// </summary>
        public DocAttribute Doc { get; }

        /// <summary>
        /// Parameters
        /// </summary>
        public bool HasParameters { get; }

    }

    #endregion
}
