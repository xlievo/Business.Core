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

namespace Business.Core.Utils
{
    using Document;
    using Core;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Business.Core.Result;
    using Business.Core.Meta;

    /// <summary>
    /// Accessor
    /// </summary>
    public struct Accessor
    {
        /// <summary>
        /// Accessor
        /// </summary>
        /// <param name="type"></param>
        /// <param name="getter"></param>
        /// <param name="setter"></param>
        public Accessor(System.Type type, System.Func<object, object> getter, System.Action<object, object> setter)
        {
            Type = type;
            Getter = getter;
            Setter = setter;
        }

        /// <summary>
        /// Type
        /// </summary>
        public System.Type Type { get; private set; }

        /// <summary>
        /// Getter
        /// </summary>
        public System.Func<object, object> Getter { get; private set; }

        /// <summary>
        /// Setter
        /// </summary>
        public System.Action<object, object> Setter { get; private set; }

        /// <summary>
        /// TryGetter
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public object TryGetter(object obj)
        {
            try { return Getter(obj); } catch { return Type.IsValueType ? System.Activator.CreateInstance(Type) : null; }
        }
    }

    internal class Proces
    {
        public Proces(MethodInfo method, System.Type[] parameterType)
        {
            MethodInfo = method;
            ParameterType = parameterType;
        }

        public MethodInfo MethodInfo { get; }

        public System.Type[] ParameterType { get; }

        public ProcesMode Mode { get; set; }

        public System.Func<object, object[], object> Call { get; set; }

        [System.Flags]
        public enum ProcesMode
        {
            Proces = 2,

            ProcesGeneric = 4,

            ProcesGenericToken = 8,

            //ProcesCollection = 8,

            //ProcesCollectionGeneric = 16
        }
    }

    internal struct Accessors
    {
        public ConcurrentReadOnlyDictionary<string, Accessor> Accessor;

        public object[] ConstructorArgs;

        public ConcurrentReadOnlyDictionary<string, Proces> Methods;
    }

    /// <summary>
    /// Help
    /// </summary>
    public static class Help
    {
        #region Bind

        /// <summary>
        /// ErrorBusiness
        /// </summary>
        /// <param name="resultTypeDefinition"></param>
        /// <param name="business"></param>
        /// <returns></returns>
        public static IResult ErrorBusiness(this System.Type resultTypeDefinition, string business) => ResultFactory.ResultCreate(resultTypeDefinition, -1, string.Format("Without this Business{0}", string.IsNullOrEmpty(business) ? null : $" {business}"));

        /// <summary>
        /// ErrorCmd
        /// </summary>
        /// <param name="resultTypeDefinition"></param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public static IResult ErrorCmd(System.Type resultTypeDefinition, string cmd) => ResultFactory.ResultCreate(resultTypeDefinition, -2, string.Format("Without this Cmd{0}", string.IsNullOrEmpty(cmd) ? null : $" {cmd}"));

        internal static dynamic GetReturnValue(IResult result, MetaData meta)
        {
            object result2 = result;

            if (meta.HasReturn)
            {
                if (!meta.HasObject)
                {
                    dynamic result3 = null;

                    if (meta.ReturnType.IsValueType)
                    {
                        result3 = System.Activator.CreateInstance(meta.ReturnType);
                    }
                    //else
                    //{
                    //    result3 = Help.CreateInstance(meta.ReturnType);
                    //}

                    if (meta.HasAsync)
                    {
                        return System.Threading.Tasks.Task.FromResult<dynamic>(result3);
                    }

                    return result3;
                }
                else if (meta.ReturnType.IsValueType)
                {
                    result2 = System.Activator.CreateInstance(meta.ReturnType);
                }
            }
            else
            {

                result2 = null;
            }

            if (meta.HasAsync)
            {
                return System.Threading.Tasks.Task.FromResult<dynamic>(result2);
            }

            return result2;
        }

        internal static dynamic GetReturnValueIResult<Data>(IResult<Data> result, MetaData meta)
        {
            if (meta.HasAsync)
            {
                if (meta.HasIResultGeneric)
                {
                    return System.Threading.Tasks.Task.FromResult(result);
                }

                return System.Threading.Tasks.Task.FromResult(result as IResult);
            }

            return result;
        }

        internal static Dictionary<int, IArg> GetIArgs(IReadOnlyList<Args> iArgs, object[] argsObj)
        {
            var result = new Dictionary<int, IArg>();

            if (0 < iArgs?.Count)
            {
                foreach (var item in iArgs)
                {
                    IArg iArg;

                    var arg = argsObj[item.Position];

                    if (item.HasCast && !item.Type.IsAssignableFrom(arg?.GetType()))
                    {
                        iArg = (IArg)System.Activator.CreateInstance(item.Type);
                        //Not entry for value type
                        if (!(null == arg && item.IArgInType.IsValueType))
                        {
                            iArg.In = arg;
                        }
                    }
                    else
                    {
                        iArg = (IArg)(arg ?? System.Activator.CreateInstance(item.Type));
                    }

                    //if (string.IsNullOrWhiteSpace(iArg.Group)) { iArg.Group = defaultCommandKey; }

                    //iArg.Log = item.IArgLog;

                    result.Add(item.Position, iArg);
                }
            }

            return result;
        }

        internal struct CurrentType { public bool hasIArg; public System.Type outType; public System.Type inType; public bool hasCollection; public bool hasDictionary; public System.Type origType; public bool nullable; public bool hasDefinition; }

        internal static CurrentType GetCurrentType(System.Type type)
        {
            var hasIArg = typeof(IArg<>).GetTypeInfo().IsAssignableFrom(type, out System.Type[] iArgOutType) || typeof(IArg<,>).GetTypeInfo().IsAssignableFrom(type, out iArgOutType);

            var current = new CurrentType { hasIArg = hasIArg, outType = hasIArg ? iArgOutType[0] : type, inType = (hasIArg && 2 == iArgOutType.Length) ? iArgOutType[1] : type };
            var nullType = System.Nullable.GetUnderlyingType(current.outType);
            if (null != nullType)
            {
                current.outType = nullType;
                current.nullable = true;
            }
            current.origType = current.outType;

            current.hasCollection = typeof(ICollection<>).IsAssignableFrom(current.outType, out System.Type[] coll);// current.outType.IsCollection();
            current.hasDictionary = typeof(IDictionary<,>).IsAssignableFrom(current.outType, out System.Type[] dict) || typeof(System.Collections.IDictionary).IsAssignableFrom(current.outType, out dict);

            //================================//
            if (current.hasDictionary)
            {
                current.outType = dict[1];
                //current.outType = current.outType.GenericTypeArguments[1];
            }
            else if (current.hasCollection)
            {
                current.outType = coll[0];
            }

            current.hasDefinition = current.outType.IsDefinition();

            return current;
        }

        /// <summary>
        /// string.IsNullOrWhiteSpace(x.Group) || x.Group == group
        /// </summary>
        /// <param name="x"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        internal static bool GroupEquals(Annotations.GroupAttribute x, string group) => string.IsNullOrWhiteSpace(x.Group) || x.Group == group;

        #endregion

        /// <summary>
        /// BaseDirectory
        /// </summary>
        public static string BaseDirectory = System.AppDomain.CurrentDomain.BaseDirectory ?? System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        internal static Accessor GetAccessor(this FieldInfo fieldInfo)
        {
            if (null == fieldInfo) { throw new System.ArgumentNullException(nameof(fieldInfo)); }

            var getter = Emit.FieldAccessorGenerator.CreateGetter(fieldInfo);
            var setter = Emit.FieldAccessorGenerator.CreateSetter(fieldInfo);

            return new Accessor(fieldInfo.FieldType, getter, setter);
        }

        internal static Accessor GetAccessor(this PropertyInfo propertyInfo)
        {
            if (null == propertyInfo) { throw new System.ArgumentNullException(nameof(propertyInfo)); }

            var getter = Emit.PropertyAccessorGenerator.CreateGetter(propertyInfo);
            var setter = Emit.PropertyAccessorGenerator.CreateSetter(propertyInfo);

            return new Accessor(propertyInfo.PropertyType, getter, setter);
        }

        /// <summary>
        /// GetMethod
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methodSelector"></param>
        /// <returns></returns>
        public static MethodInfo GetMethod<T>(System.Linq.Expressions.Expression<System.Action<T>> methodSelector) => ((System.Linq.Expressions.MethodCallExpression)methodSelector.Body).Method;

        internal readonly struct ObjectMethods
        {
            public ObjectMethods(ObjectMethod getHashCode, ObjectMethod equals, ObjectMethod toString)
            {
                GetHashCode = getHashCode;
                Equals = equals;
                ToString = toString;
            }

            public ObjectMethod GetHashCode { get; }
            public ObjectMethod Equals { get; }
            public ObjectMethod ToString { get; }

            public bool CompareTo(MethodInfo method)
            {
                var parameters = method.GetParameters().Select(c => c.ParameterType);
                return (method.Name == BaseObjectMethods.GetHashCode.Method.Name && Enumerable.SequenceEqual(parameters, BaseObjectMethods.GetHashCode.Parameters)) || (method.Name == BaseObjectMethods.Equals.Method.Name && Enumerable.SequenceEqual(parameters, BaseObjectMethods.Equals.Parameters)) || (method.Name == BaseObjectMethods.ToString.Method.Name && Enumerable.SequenceEqual(parameters, BaseObjectMethods.ToString.Parameters));
            }

            public readonly struct ObjectMethod
            {
                public ObjectMethod(MethodInfo method)
                {
                    this.Method = method;
                    this.Parameters = method.GetParameters().Select(c => c.ParameterType);
                }

                public MethodInfo Method { get; }
                public IEnumerable<System.Type> Parameters { get; }
            }
        }

        internal static readonly ObjectMethods BaseObjectMethods = new ObjectMethods(new ObjectMethods.ObjectMethod(GetMethod<object>(c => c.GetHashCode())), new ObjectMethods.ObjectMethod(GetMethod<object>(c => c.Equals(null))), new ObjectMethods.ObjectMethod(GetMethod<object>(c => c.ToString())));

        internal static void LoadAccessors(this System.Type type, ConcurrentReadOnlyDictionary<string, Accessors> accessors, string key = null, bool propertys = true, bool fields = true, bool methods = false, bool update = false)
        {
            var key2 = string.IsNullOrWhiteSpace(key) ? type.FullName : key;

            if (type.IsAbstract) { return; }

            if (update)
            {
                accessors.dictionary.TryRemove(key2, out _);
            }
            else if (accessors.ContainsKey(key2))
            {
                return;
            }

            var member = accessors.dictionary.GetOrAdd(key2, _ => new Accessors { Accessor = new ConcurrentReadOnlyDictionary<string, Accessor>(), ConstructorArgs = type.GetConstructors()?.FirstOrDefault()?.GetParameters().Select(c => c.HasDefaultValue ? c.DefaultValue : c.ParameterType.IsValueType ? System.Activator.CreateInstance(c.ParameterType) : default).ToArray(), Methods = new ConcurrentReadOnlyDictionary<string, Proces>() });

            if (fields)
            {
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
            }

            if (propertys)
            {
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

            if (methods)
            {
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                {
                    var accessor = new Proces(method.IsGenericMethod ? method.GetGenericMethodDefinition() : method, method.GetParameters().Select(c => c.ParameterType).ToArray());
                    member.Methods.dictionary.TryAdd(method.Name, accessor);
                }
            }
        }

        /// <summary>
        /// UseType
        /// </summary>
        /// <typeparam name="Business"></typeparam>
        /// <param name="business"></param>
        /// <param name="parameterType"></param>
        /// <returns></returns>
        public static Business UseType<Business>(Business business, params System.Type[] parameterType) where Business : IBusiness
        {
            if (null == business) { throw new System.ArgumentNullException(nameof(business)); }

            if (null == parameterType) { return business; }

            foreach (var item in parameterType)
            {
                if (null == item || business.Configer.UseTypes.ContainsKey(item.FullName)) { continue; }

                business.Configer.UseTypes.dictionary.TryAdd(item.FullName, item);
            }

#if DEBUG
            foreach (var item in business.Configer.MetaData.Values)
#else
            System.Threading.Tasks.Parallel.ForEach(business.Configer.MetaData.Values, item =>
#endif
            {
                var hasUpdate = false;

                foreach (var arg in item.Args)
                {
                    var type2 = (arg.HasIArg && !arg.HasCast) ? arg.IArgInType : arg.LastType;

                    if (!business.Configer.UseTypes.ContainsKey(type2.FullName) || !item.UseTypePosition.dictionary.TryAdd(arg.Position, type2))
                    {
                        continue;
                    }

                    arg.UseType = true;

                    if (arg.HasCast)
                    {
                        arg.HasCast = arg.HasIArg = false;
                        arg.Type = arg.OrigType;
                        item.IArgs.Collection.Remove(arg);
                    }

                    foreach (var item2 in arg.Group)
                    {
                        //remove not parameter attr

                        //var attrs = new ReadOnlyCollection<Annotations.ArgumentAttribute>();

                        //var first = item2.Value.Attrs.ForEach(c =>
                        //{
                        //    var attr = c.Value;

                        //    if (attr.Meta.Declaring != Annotations.AttributeBase.MetaData.DeclaringType.Parameter)
                        //    {
                        //        //item2.Value.Attrs.Remove(attr, out _);
                        //        return;
                        //    }

                        //    attrs.TryAdd(attr);
                        //});

                        item2.Value.Attrs = item2.Value.Attrs.Where(c => Annotations.AttributeBase.MetaData.DeclaringType.Parameter == c.Meta.Declaring).ToReadOnly();

                        //add default convert
                        if (arg.HasIArg && 0 == item2.Value.Attrs.Count)
                        {
                            var attr = new Annotations.ArgumentDefaultAttribute(item.ResultType, item.ResultTypeDefinition, item.ArgTypeDefinition);
                            attr.Meta.Declaring = Annotations.AttributeBase.MetaData.DeclaringType.Parameter;
                            item2.Value.Attrs.Collection.Add(attr);
                            //arg.ArgAttr.collection.Add(new Attributes.ArgumentDefaultAttribute(business.Configer.ResultType) { Source = Attributes.AttributeBase.SourceType.Parameter });
                        }
                    }

                    hasUpdate = true;
                }

                if (hasUpdate)
                {
                    foreach (var group in item.CommandGroup.Group.Values)
                    {
                        business.Command[group.Group][group.OnlyName].StatisArgs();
                    }
                }
                //item.HasArgSingle = 1 >= item.Args.Count(c => !c.UseType);
#if DEBUG
            };
#else
            });
#endif

            return business;
        }

        /// <summary>
        /// UseType
        /// </summary>
        /// <typeparam name="Business"></typeparam>
        /// <param name="business"></param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public static Business UseType<Business>(Business business, params string[] parameterName) where Business : IBusiness
        {
            if (null == business) { throw new System.ArgumentNullException(nameof(business)); }

            if (null == parameterName) { return business; }

            var argName = parameterName.Where(c => !string.IsNullOrWhiteSpace(c)).ToList();

            if (0 == argName.Count) { return business; }

#if DEBUG
            foreach (var item in business.Configer.MetaData.Values)
#else
            System.Threading.Tasks.Parallel.ForEach(business.Configer.MetaData.Values, item =>
#endif
            {
                var hasUpdate = false;

                foreach (var arg in item.Args)
                {
                    var type2 = (arg.HasIArg && !arg.HasCast) ? arg.IArgInType : arg.LastType;

                    if (!argName.Contains(arg.Name) || !item.UseTypePosition.dictionary.TryAdd(arg.Position, type2))
                    {
                        continue;
                    }

                    arg.UseType = true;

                    arg.Use = new Annotations.UseAttribute { ParameterName = true };

                    if (arg.HasCast)
                    {
                        arg.HasCast = arg.HasIArg = false;
                        arg.Type = arg.OrigType;
                        item.IArgs.Collection.Remove(arg);
                    }

                    foreach (var item2 in arg.Group)
                    {
                        //remove not parameter attr

                        //var attrs = new ConcurrentLinkedList<Annotations.ArgumentAttribute>();

                        //var first = item2.Value.Attrs.ForEach(c =>
                        //{
                        //    var attr = c.Value;

                        //    if (attr.Meta.Declaring != Annotations.AttributeBase.MetaData.DeclaringType.Parameter)
                        //    {
                        //        //item2.Value.Attrs.Remove(attr, out _);
                        //        return;
                        //    }

                        //    attrs.TryAdd(attr);
                        //});

                        item2.Value.Attrs = item2.Value.Attrs.Where(c => Annotations.AttributeBase.MetaData.DeclaringType.Parameter == c.Meta.Declaring).ToReadOnly();

                        //add default convert
                        if (arg.HasIArg && 0 == item2.Value.Attrs.Count)
                        //if (arg.HasIArg && NodeState.DAT != first.State)
                        {
                            var attr = new Annotations.ArgumentDefaultAttribute(item.ResultType, item.ResultTypeDefinition, item.ArgTypeDefinition);
                            attr.Meta.Declaring = Annotations.AttributeBase.MetaData.DeclaringType.Parameter;
                            item2.Value.Attrs.Collection.Add(attr);
                        }

                        //business.Command[group.Group][group.OnlyName].HasArgSingle = 1 >= item.Args.Count(c => !c.HasToken && !c.UseType && !c.Group[group.Key].IgnoreArg);
                        //business.Command[group.Group][group.OnlyName].SetParametersType();
                    }

                    hasUpdate = true;
                }

                if (hasUpdate)
                {
                    foreach (var group in item.CommandGroup.Group.Values)
                    {
                        business.Command[group.Group][group.OnlyName].StatisArgs();
                    }
                }
                //item.HasArgSingle = 1 >= item.Args.Count(c => !c.UseType);
#if DEBUG
            };
#else
            });
#endif

            return business;
        }

        //public static void SetParametersType(this System.Type parametersType)
        //{

        //}

        //public static string GetXmlPath(this Assembly assembly) => System.IO.Path.Combine(System.IO.Path.GetDirectoryName(assembly.Location), $"{System.IO.Path.GetFileNameWithoutExtension(assembly.ManifestModule.Name)}.xml");

        /*
        public class SerializaContractResolver : Newtonsoft.Json.Serialization.DefaultContractResolver
        {
            readonly System.Func<string, string> resolvePropertyName;

            public SerializaContractResolver(System.Func<string, string> resolvePropertyName)
            {
                this.resolvePropertyName = resolvePropertyName;
            }

            protected override string ResolvePropertyName(string propertyName) => resolvePropertyName(propertyName);
        }

        public static Newtonsoft.Json.JsonSerializerSettings JsonSettings = new Newtonsoft.Json.JsonSerializerSettings
        {
            ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
            ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
            DateFormatString = "yyyy-MM-dd HH:mm:ss",
            DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Local,
            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
        };
        */

        static DocArg GetDocArg<TypeDefinition>(DocArgSource<TypeDefinition> argSource, bool hideArray = false, bool hideTitleType = false)
            where TypeDefinition : ITypeDefinition<TypeDefinition>
        {
            var group = argSource.Args.Group[argSource.Group];
            var alias = group.Alias;
            if (!string.IsNullOrWhiteSpace(alias))
            {
                alias = $" {alias}";
            }

            var docArg = new DocArg { Id = group.Path, LastType = argSource.Args.LastType.Name, Array = argSource.Args.HasCollection, Name = argSource.Args.Name, ValueType = argSource.Args.LastType.IsValueType };

            var titleArgsName = argSource.TitleArgsName ?? argSource.Args.Name;

            docArg.Title = hideTitleType && argSource.Args.HasDefinition ? $"{titleArgsName} {alias}" : $"{titleArgsName} ({docArg.LastType}){alias}";
            //{argSource.Args.Group[argSource.Group].Nick}
            docArg.Description = argSource.Summary;

            docArg.DescriptionTip = 0 < argSource.Attributes?.Count ? argSource.Attributes : null;

            docArg.Enums = argSource.Enums;

            //docArg.Token = argSource.Args.HasToken;

            if (argSource.Args.HasDefaultValue)
            {
                docArg.DefaultValue = argSource.Args.DefaultValue;
            }

            if (!argSource.Args.HasToken && argSource.Args.HasDefinition)
            {
                docArg.Type = "object";
            }
            else
            {
                docArg.Type = "string";
                if (!string.IsNullOrWhiteSpace(argSource.Summary))
                {
                    docArg.Options = new Dictionary<string, object>
                    {
                        { "inputAttributes", new Dictionary<string, object> { { "placeholder", argSource.Summary } } },
                    };
                }
            }

            if (null != docArg.Enums)
            {
                docArg.Type = "number";
                docArg.Options = new Dictionary<string, object>
                {
                    { "enum_titles",  docArg.Enums.Select(c => $"{c.Name} : {c.Value}") },
                };
            }

            if (argSource.Args.HasDictionary)
            {
                docArg.Type = "object";
                //docArg.Type = "array";
                //docArg.Format = "table";
                //docArg.UniqueItems = true;
                //docArg.Items = new Items
                //{
                //    Type = "object",
                //    Children = new Dictionary<string, IDocArg>
                //    {
                //        { "Key", new DocArg { Type = "string" } },
                //        { "Value", new DocArg { Type = "string" } },
                //    }
                //};
            }
            else if (argSource.Args.HasCollection && !hideArray)
            {
                docArg.Title = hideTitleType && argSource.Args.HasDefinition ? $"{titleArgsName} (Array){alias}" : $"{titleArgsName} ({docArg.LastType} Array){alias}";

                docArg.Type = "array";
                docArg.Items = new Items<DocArg>();
                docArg.Options = new Dictionary<string, object> { { "disable_array_delete_last_row", true }, { "array_controls_top", true } };

                if (argSource.Args.HasDefinition)
                {
                    docArg.Items.Type = "object";
                }
                else
                {
                    var type = GetDocArgType(argSource.Args.LastType.GetTypeCode());
                    if (default != type)
                    {
                        docArg.Items.Type = type.Item1;
                        docArg.Items.Format = type.Item2;
                    }
                    else
                    {
                        docArg.Items = null;
                    }
                }
            }
            else
            {
                var type = GetDocArgType(argSource.Args.LastType.GetTypeCode());
                if (default != type)
                {
                    docArg.Type = type.Item1;
                    docArg.Format = type.Item2;
                }
            }

            if (argSource.Args.HasCollection)
            {
                docArg.Title = hideTitleType && argSource.Args.HasDefinition ? $"{titleArgsName} (Array){alias}" : $"{titleArgsName} ({docArg.LastType} Array){alias}";
            }

            return docArg;
        }

        static (string, string) GetDocArgType(System.TypeCode typeCode)
        {
            switch (typeCode)
            {
                case System.TypeCode.Boolean:
                    return ("boolean", null);
                case System.TypeCode.Byte:
                    return ("string", "binary");
                case System.TypeCode.DBNull:
                case System.TypeCode.Empty:
                case System.TypeCode.Char:
                case System.TypeCode.String:
                    return ("string", null);
                case System.TypeCode.DateTime:
                    return ("string", "datetime-local");
                case System.TypeCode.Decimal:
                case System.TypeCode.Double:
                    return ("number", null);
                case System.TypeCode.Int16:
                    return ("integer", null);
                case System.TypeCode.Int32:
                    return ("integer", "int32");
                case System.TypeCode.Int64:
                    return ("integer", "int64");
                case System.TypeCode.SByte:
                    return ("integer", null);
                case System.TypeCode.Single:
                    return ("number", "float");
                case System.TypeCode.UInt16:
                    return ("integer", null);
                case System.TypeCode.UInt32:
                    return ("integer", "int32");
                case System.TypeCode.UInt64:
                    return ("integer", "int64");
                default: return default;
            }
        }

        /// <summary>
        /// Generate "Business.Document.DocArg" Document Objects for the specified business class.
        /// </summary>
        /// <typeparam name="Business"></typeparam>
        /// <param name="business"></param>
        /// <param name="outDir"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static Business UseDoc<Business>(Business business, string outDir = null, Options options = default) where Business : IBusiness => UseDoc(business, c => GetDocArg(c), outDir, options);

        /// <summary>
        /// Generate document objects for specified business classes.
        /// </summary>
        /// <typeparam name="Business"></typeparam>
        /// <typeparam name="DocArg"></typeparam>
        /// <param name="business"></param>
        /// <param name="argCallback"></param>
        /// <param name="outDir"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static Business UseDoc<Business, DocArg>(Business business, System.Func<DocArgSource<Args>, DocArg> argCallback, string outDir = null, Options options = default) where Business : IBusiness where DocArg : IDocArg<DocArg>, new()
        {
            if (null == business) { throw new System.ArgumentNullException(nameof(business)); }
            if (null == argCallback) { throw new System.ArgumentNullException(nameof(argCallback)); }

            //var path = business.GetType().BaseType.Assembly.GetXmlPath();

            //Configer.Xmls.TryGetValue(path, out Xml xml);

            //if (null == xml && System.IO.File.Exists(path))
            //{
            //    xml = Configer.Xmls.dictionary.GetOrAdd(path, Xml.DeserializeDoc(FileReadString(path)));
            //}

            //business.Configer.Doc = UseDoc(business, argCallback, xml?.members?.ToDictionary(c => c.name, c => c), host);

            /* Note
            The XML documentation comments are not metadata; they are not included in the compiled assembly and therefore they are not accessible through reflection.
            */
            if (null == Configer.Xmls)
            {
                Configer.Xmls = new ConcurrentReadOnlyDictionary<string, Xml.member>();

                System.IO.Directory.GetFiles(BaseDirectory, "*.xml").AsParallel().ForAll(path =>
                {
                    var doc = Xml.DeserializeDoc(FileReadString(path));

                    if (null != doc?.members)
                    {
                        foreach (var item in doc.members)
                        {
                            Configer.Xmls.dictionary.TryAdd(item.name, item);
                        }
                    }
                });
            }

            business.Configer.Doc = UseDoc(business, argCallback, Configer.Xmls, options);

            //business.Configer.Info.DocFileName = $"{business.Configer.Info.TypeFullName}.doc";
            //business.Configer.Info.DocFileName = "business.doc";

            if (!string.IsNullOrEmpty(outDir))
            {
                if (System.IO.Directory.Exists(outDir))
                {
                    var file = System.IO.Path.Combine(outDir, Configer.documentFileName);

                    business.Configer.Info.DocPhysicalPath = file;

                    //business.Configer.Info.DocRequestPath = Uri.TryCreate($"{host ?? string.Empty}/{System.IO.Path.GetFileName(file)}", UriKind.Absolute, out Uri uri) ? uri.AbsoluteUri : $"http://localhost:5000{"/"}{System.IO.Path.GetFileName(file)}";

                    System.IO.File.WriteAllText(file, business.Configer.Doc.ToString(), UTF8);
                }
            }

            return business;
        }

        /// <summary>
        /// FirstCharToLower
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string FirstCharToLower(this string input) => string.IsNullOrEmpty(input) ? input : $"{input[0].ToString().ToLower()}{input.Substring(1)}";

        /// <summary>
        /// Gets the document object of the specified business class.
        /// </summary>
        /// <typeparam name="Business"></typeparam>
        /// <typeparam name="DocArg"></typeparam>
        /// <param name="business"></param>
        /// <param name="argCallback"></param>
        /// <param name="xmlMembers"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static Doc<DocArg> UseDoc<Business, DocArg>(Business business, System.Func<DocArgSource<Args>, DocArg> argCallback, IDictionary<string, Xml.member> xmlMembers, Options options = default) where Business : IBusiness where DocArg : IDocArg<DocArg>, new()
        {
            if (null == argCallback) { throw new System.ArgumentNullException(nameof(argCallback)); }

            var groupDefault = business.Configer.Info.CommandGroupDefault;

            options = options ?? new Options();
            var command = null != options.Group ? business.Command.Where(c => c.Key.Equals(options.Group, System.StringComparison.InvariantCultureIgnoreCase)) : business.Command;

            var group = command.OrderBy(c => c.Key).AsParallel().ToDictionary(c => c.Key, c => c.Value.OrderBy(c2 => c2.Value.Meta.Position).ToDictionary(c2 => c2.Key, c2 =>
            {
                var key = c2.Value.Key;
                var meta = c2.Value.Meta;
                var onlyName = meta.CommandGroup.Group[key].OnlyName;

                Xml.member member = null;
                xmlMembers?.TryGetValue($"M:{meta.MethodTypeFullName}", out member);

                var returnType = meta.ReturnType.GetTypeDefinition(xmlMembers, member?.returns?.sub, groupDefault, $"{onlyName}.Returns");
                var testing = meta.Attributes.GetAttrs<Annotations.TestingAttribute>();

                var token = meta.Tokens.FirstOrDefault();

                var member2 = new Member<DocArg>
                {
                    Key = meta.Name,
                    Name = onlyName,
                    Alias = meta.Doc?.Alias,
                    AliasGroup = meta.Doc?.Group,
                    HasReturn = meta.HasReturn,
                    HasIResult = meta.HasIResult,
                    HasToken = null != token,
                    Description = member?.summary?.sub,
                    DescriptionParam = 0 < member?._params?.Count ? member?._params?.ToDictionary(c3 => c3.name, c3 => c3.sub) : null,
                    DescriptionResult = member?.returns?.sub,
                    Returns = meta.HasReturn ? GetDocArg(groupDefault, returnType, c3 => GetDocArg(c3, true, true), xmlMembers, returnType.Summary) : default,
                    ArgSingle = c2.Value.HasArgSingle,
                    HttpFile = c2.Value.HasHttpFile,
                    Testing = 0 < testing.Count ? testing.ToDictionary(c3 => c3.Name, c3 => new Testing(c3.Name, c3.Value, c3.Result, c3.Token)) : null,
                    Token = GetDocArg(key, token, argCallback, xmlMembers, member?._params?.Find(c4 => c4.name == token?.Name)?.text)
                } as IMember<DocArg>;

                var titleArgsName = "d";
                var args = c2.Value.Parameters.ToDictionary(c3 => c3.Name, c3 => GetDocArg(key, c3, argCallback, xmlMembers, member?._params?.Find(c4 => c4.name == c3.Name)?.text, member2.ArgSingle ? titleArgsName : c3.Name));

                if (!member2.ArgSingle)
                {
                    member2.Args = new DocArg { Id = $"{member2.Name}.{titleArgsName}", Title = $"{titleArgsName} (Object)", Type = "object", Children = args, Description = "API data" };
                }
                else if (0 < args.Count)
                {
                    member2.Args = args.First().Value;
                }

                //foreach (var item in meta.Args.Where(c3 => !c3.Group[key].IgnoreArg))
                //{
                //    member2.Args.Add(item.Name, GetDocArg(key, item, argCallback, xmlMembers, member?._params?.Find(c4 => c4.name == item.Name)?.text));
                //}

                return member2;
            }));

            Xml.member member3 = null;
            xmlMembers?.TryGetValue($"T:{business.Configer.Info.TypeFullName}", out member3);

            options.Host = System.Uri.TryCreate(options.Host, System.UriKind.Absolute, out System.Uri uri) ? $"{uri.Scheme}://{uri.Authority}" : string.Empty;

            return new Doc<DocArg> { Name = business.Configer.Info.BusinessName, Alias = business.Configer.Info.Alias, Group = group, GroupDefault = groupDefault, Description = member3?.summary?.text, Options = options, DocGroup = business.Configer.DocGroup.OrderBy(c => c.Key.position).ToDictionary(c => c.Key, c => c.Value.OrderBy(c2 => c2.position) as IEnumerable<DocInfo>) };
        }

        const string AttributeSign = "Attribute";

        static IEnumerable<EnumItems> GetEnums(this System.Type type, IDictionary<string, Xml.member> xmlMembers)
        {
            return type.GetEnumValues().Cast<int>().Select(c =>
            {
                var enumName = System.Enum.GetName(type, c);
                Xml.member enumMember = null;
                xmlMembers?.TryGetValue($"F:{type.GetTypeName()}.{enumName}", out enumMember);
                var memberSummary = enumMember?.summary?.text;
                return new EnumItems(enumName, c, memberSummary);
            });
        }

        static DocArg GetDocArg<DocArg, TypeDefinition>(string group, TypeDefinition args, System.Func<DocArgSource<TypeDefinition>, DocArg> argCallback, IDictionary<string, Xml.member> xmlMembers, string summary = null, string titleArgsName = null)
            where DocArg : IDocArg<DocArg>
            where TypeDefinition : ITypeDefinition<TypeDefinition>
        {
            if (null == args) { return default; }
            if (null == argCallback) { throw new System.ArgumentNullException(nameof(argCallback)); }

            if (string.IsNullOrWhiteSpace(summary))
            {
                Xml.member member = null;

                switch (args.MemberDefinition)
                {
                    case MemberDefinitionCode.No:
                        break;
                    case MemberDefinitionCode.Definition:
                        xmlMembers?.TryGetValue($"T:{args.FullName}", out member);
                        break;
                    case MemberDefinitionCode.Field:
                        xmlMembers?.TryGetValue($"F:{args.FullName}", out member);
                        break;
                    case MemberDefinitionCode.Property:
                        xmlMembers?.TryGetValue($"P:{args.FullName}", out member);
                        break;
                }

                summary = member?.summary?.text;

                if (string.IsNullOrWhiteSpace(summary) && (args.HasDefinition || args.LastType.IsEnum))
                {
                    xmlMembers?.TryGetValue($"T:{args.LastType.GetTypeName()}", out member);

                    summary = member?.summary?.text;
                }
            }

            //if (args.LastType.IsEnum)
            //{
            //    summary = args.LastType.GetEnumSummary(summary, xmlMembers);
            //}

            var argGroup = args.Group[group];

            var attrs = new List<string>();

            //// while (null != attr && NodeState.DAT == attr.State)
            //argGroup.Attrs.ForEach(c =>
            //{
            //    var attr = c.Value;

            //    if (typeof(Annotations.ArgumentDefaultAttribute).IsAssignableFrom(attr.Meta.Type))
            //    {
            //        return;
            //    }

            //    attrs.Add(string.IsNullOrWhiteSpace(attr.Description) ? attr.Meta.Type.Name.EndsWith(AttributeSign) ? attr.Meta.Type.Name.Substring(0, attr.Meta.Type.Name.Length - AttributeSign.Length) : attr.Meta.Type.Name : attr.Description);
            //});

            if (null != argGroup.Attrs)
            {
                foreach (var attr in argGroup.Attrs)
                {
                    if (typeof(Annotations.ArgumentDefaultAttribute).IsAssignableFrom(attr.Meta.Type))
                    {
                        continue;
                    }

                    attrs.Add(string.IsNullOrWhiteSpace(attr.Description) ? attr.Meta.Type.Name.EndsWith(AttributeSign) ? attr.Meta.Type.Name.Substring(0, attr.Meta.Type.Name.Length - AttributeSign.Length) : attr.Meta.Type.Name : attr.Description);
                }
            }

            var arg = argCallback(new DocArgSource<TypeDefinition>(group, args, attrs, summary, args.LastType.IsEnum ? args.LastType.GetEnums(xmlMembers) : null, titleArgsName));

            if ((null == argGroup.Ignore || !argGroup.Ignore.Any(c => c.Mode == Annotations.IgnoreMode.ArgChild)) && !args.LastType.IsEnum)
            {
                // && !arg.IsDictionary && !arg.IsCollection
                var childrens = args.Children.Where(c => !c.Group[group].IgnoreArg);

                if (childrens.Any())
                {
                    if ("array" == arg.Type)
                    {
                        arg.Items.Children = new Dictionary<string, DocArg>();
                    }
                    else
                    {
                        arg.Children = new Dictionary<string, DocArg>();
                    }

                    foreach (var item in childrens)
                    {
                        if ("array" == arg.Type)
                        {
                            if (arg.Items.Children.ContainsKey(item.Name))
                            {
                                throw new System.ArgumentException($"{item.Group[group].Path} already exist!");
                            }

                            arg.Items.Children.Add(item.Name, GetDocArg(group, item, argCallback, xmlMembers));
                        }
                        else
                        {
                            if (arg.Children.ContainsKey(item.Name))
                            {
                                throw new System.ArgumentException($"{item.Group[group].Path} already exist!");
                            }

                            arg.Children.Add(item.Name, GetDocArg(group, item, argCallback, xmlMembers));
                        }
                    }
                }
            }

            return arg;
        }

        /// <summary>
        /// GetTypeDefinition
        /// </summary>
        /// <param name="type"></param>
        /// <param name="xmlMembers"></param>
        /// <param name="summary"></param>
        /// <param name="groupKey"></param>
        /// <param name="pathRoot"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static TypeDefinition GetTypeDefinition(this System.Type type, IDictionary<string, Xml.member> xmlMembers = null, string summary = null, string groupKey = "", string pathRoot = null, string name = null)
        {
            var current = GetCurrentType(type);
            var definitions = current.hasDefinition ? new List<string> { type.FullName } : new List<string>();
            var childrens = new ReadOnlyCollection<TypeDefinition>();
            var fullName = type.GetTypeName();
            var memberDefinition = current.hasDefinition ? MemberDefinitionCode.Definition : MemberDefinitionCode.No;
            //..//

            Xml.member member = null;
            if (string.IsNullOrWhiteSpace(summary) && (memberDefinition == MemberDefinitionCode.Definition || current.outType.IsEnum))
            {
                xmlMembers?.TryGetValue($"T:{current.outType.GetTypeName()}", out member);

                summary = member?.summary?.text;
            }

            //if (current.outType.IsEnum)
            //{
            //    summary = current.outType.GetEnumSummary(summary, xmlMembers);
            //}

            var group = new ConcurrentReadOnlyDictionary<string, ArgGroup>();
            group.dictionary.TryAdd(groupKey, new ArgGroup(pathRoot ?? type.Name));

            var definition = new TypeDefinition
            {
                Name = name,//current.outType.Name,
                Type = type,
                LastType = current.outType,
                HasDefinition = current.hasDefinition,
                DefaultValue = type.IsValueType && typeof(void) != type ? System.Activator.CreateInstance(type) : null,

                HasNumeric = type.IsNumeric(),
                HasDictionary = current.hasDictionary,
                HasCollection = current.hasCollection,
                HasEnum = type.IsEnum,
                EnumNames = type.IsEnum ? type.GetEnumNames() : null,
                EnumValues = type.IsEnum ? type.GetEnumValues() : null,
                Nullable = current.nullable,

                FullName = fullName,
                Children = current.hasDefinition ? GetTypeDefinition(current.outType, definitions, childrens, pathRoot, xmlMembers, groupKey) : new ReadOnlyCollection<TypeDefinition>(),
                Childrens = childrens,
                MemberDefinition = memberDefinition,
                Summary = summary,
                Path = type.Name,
                Group = group
            };

            return definition;
        }

        static ReadOnlyCollection<TypeDefinition> GetTypeDefinition(System.Type type, List<string> definitions, ReadOnlyCollection<TypeDefinition> childrens, string path, IDictionary<string, Xml.member> xmlMembers = null, string groupKey = "")
        {
            var types = new ReadOnlyCollection<TypeDefinition>();

            var members = type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.GetField | BindingFlags.GetProperty);

            foreach (var item in members)
            {
                System.Type memberType = null;
                var memberDefinition = MemberDefinitionCode.No;

                switch (item.MemberType)
                {
                    case MemberTypes.Field:
                        {
                            var member = item as FieldInfo;
                            memberType = member.FieldType;
                            memberDefinition = MemberDefinitionCode.Field;
                        }
                        break;
                    case MemberTypes.Property:
                        {
                            var member = item as PropertyInfo;
                            memberType = member.PropertyType;
                            memberDefinition = MemberDefinitionCode.Property;
                        }
                        break;
                    default: continue;
                }

                var current = GetCurrentType(memberType);
                //var hasDefinition = memberType.IsDefinition();
                if (definitions.Contains(memberType.FullName)) { continue; }
                else if (current.hasDefinition) { definitions.Add(memberType.FullName); }
                var childrens2 = new ReadOnlyCollection<TypeDefinition>();
                var fullName = $"{type.GetTypeName(item.DeclaringType)}.{item.Name}";

                Xml.member member2 = null;

                switch (memberDefinition)
                {
                    case MemberDefinitionCode.No:
                        break;
                    case MemberDefinitionCode.Definition:
                        xmlMembers?.TryGetValue($"T:{fullName}", out member2);
                        break;
                    case MemberDefinitionCode.Field:
                        xmlMembers?.TryGetValue($"F:{fullName}", out member2);
                        break;
                    case MemberDefinitionCode.Property:
                        xmlMembers?.TryGetValue($"P:{fullName}", out member2);
                        break;
                }

                var summary = member2?.summary?.text;

                if (string.IsNullOrWhiteSpace(summary) && (memberDefinition == MemberDefinitionCode.Definition || current.outType.IsEnum))
                {
                    xmlMembers?.TryGetValue($"T:{current.outType.GetTypeName()}", out member2);

                    summary = member2?.summary?.text;
                }

                //if (current.outType.IsEnum)
                //{
                //    summary = current.outType.GetEnumSummary(summary, xmlMembers);
                //}

                // .. //
                var path2 = !string.IsNullOrWhiteSpace(path) ? $"{path}.{item.Name}" : item.Name;

                var group = new ConcurrentReadOnlyDictionary<string, ArgGroup>();
                group.dictionary.TryAdd(groupKey, new ArgGroup(path2));

                //var current = GetCurrentType(memberType);

                var definition = new TypeDefinition
                {
                    Name = item.Name,
                    Type = memberType,
                    LastType = current.outType,
                    HasDefinition = current.hasDefinition,
                    DefaultValue = memberType.IsValueType ? System.Activator.CreateInstance(memberType) : null,

                    HasNumeric = memberType.IsNumeric(),
                    HasDictionary = current.hasDictionary,
                    HasCollection = current.hasCollection,
                    HasEnum = memberType.IsEnum,
                    EnumNames = memberType.IsEnum ? memberType.GetEnumNames() : null,
                    EnumValues = memberType.IsEnum ? memberType.GetEnumValues() : null,
                    Nullable = current.nullable,

                    FullName = fullName,
                    Children = current.hasDefinition ? GetTypeDefinition(current.outType, definitions, childrens2, path2, xmlMembers, groupKey) : new ReadOnlyCollection<TypeDefinition>(),
                    Childrens = childrens2,
                    MemberDefinition = memberDefinition,
                    Summary = summary,
                    Path = path2,
                    Group = group
                };

                types.Collection.Add(definition);
                childrens.Collection.Add(definition);

                foreach (var child in childrens2)
                {
                    childrens.Collection.Add(child);
                }
            }

            return types;
        }

        /// <summary>
        /// TypeDefinition
        /// </summary>
        public struct TypeDefinition : ITypeDefinition<TypeDefinition>
        {
            /// <summary>
            /// Name
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Type
            /// </summary>
            public System.Type Type { get; set; }

            /// <summary>
            /// LastType
            /// </summary>
            public System.Type LastType { get; set; }

            /// <summary>
            /// HasDefinition
            /// </summary>
            public bool HasDefinition { get; set; }

            /// <summary>
            /// HasCollection
            /// </summary>
            public bool HasCollection { get; set; }

            /// <summary>
            /// HasDictionary
            /// </summary>
            public bool HasDictionary { get; set; }

            /// <summary>
            /// DefaultValue
            /// </summary>
            public object DefaultValue { get; set; }

            /// <summary>
            /// Nullable
            /// </summary>
            public bool Nullable { get; set; }

            /// <summary>
            /// FullName
            /// </summary>
            public string FullName { get; set; }

            /// <summary>
            /// MemberDefinition
            /// </summary>
            public MemberDefinitionCode MemberDefinition { get; set; }

            /// <summary>
            /// HasToken
            /// </summary>
            public bool HasToken { get; set; }

            /// <summary>
            /// HasDefaultValue
            /// </summary>
            public bool HasDefaultValue { get; set; }

            /// <summary>
            /// Group
            /// </summary>
            public ConcurrentReadOnlyDictionary<string, ArgGroup> Group { get; set; }

            /// <summary>
            /// Children
            /// </summary>
            public ReadOnlyCollection<TypeDefinition> Children { get; set; }

            /// <summary>
            /// Childrens
            /// </summary>
            public ReadOnlyCollection<TypeDefinition> Childrens { get; set; }

            //==========================================================//

            /// <summary>
            /// HasEnum
            /// </summary>
            public bool HasEnum { get; set; }

            /// <summary>
            /// HasNumeric
            /// </summary>
            public bool HasNumeric { get; set; }

            /// <summary>
            /// EnumNames
            /// </summary>
            public string[] EnumNames { get; set; }

            /// <summary>
            /// EnumValues
            /// </summary>
            public System.Array EnumValues { get; set; }

            /// <summary>
            /// Path
            /// </summary>
            public string Path { get; set; }

            /// <summary>
            /// Summary
            /// </summary>
            public string Summary { get; set; }
        }

        ///// <summary>
        ///// Logger use threadPool, Default true
        ///// </summary>
        ///// <typeparam name="Business"></typeparam>
        ///// <param name="business"></param>
        ///// <param name="use"></param>
        ///// <returns></returns>
        //public static Business LoggerUseThreadPool<Business>(this Business business, bool use = true) where Business : IBusiness
        //{
        //    business.Configer.LoggerUseThreadPool = use;
        //    return business;
        //}

        /// <summary>
        /// LoggerSet
        /// </summary>
        /// <typeparam name="Business"></typeparam>
        /// <param name="business"></param>
        /// <param name="logger"></param>
        /// <param name="argType"></param>
        /// <returns></returns>
        public static Business LoggerSet<Business>(Business business, Annotations.LoggerAttribute logger, params System.Type[] argType) where Business : IBusiness
        {
            if (null == business) { throw new System.ArgumentNullException(nameof(business)); }

            if (null == logger || null == argType || 0 == argType.Length) { return business; }

            System.Threading.Tasks.Parallel.ForEach(business.Configer.MetaData.Values, item =>
            {
                var groups = item.CommandGroup.Group.Values.Where(c => GroupEquals(logger, c.Group));

                if (!groups.Any()) { return; }

                logger.Meta.Declaring = Annotations.AttributeBase.MetaData.DeclaringType.Parameter;

                System.Threading.Tasks.Parallel.ForEach(argType, type =>
                {
                    foreach (var group in groups)
                    {
                        foreach (var arg in item.Args)
                        {
                            if (Equals(arg.Type, type) || Equals(arg.IArgOutType, type))
                            {
                                arg.Group[group.Key].Logger = GetMetaLogger(arg.Group[group.Key].Logger, logger, group.Group);
                            }

                            if (Equals(arg.IArgInType, type))
                            {
                                arg.Group[group.Key].IArgInLogger = GetMetaLogger(arg.Group[group.Key].IArgInLogger, logger, group.Group);
                            }
                        }
                    }
                });
            });

            return business;
        }

        /// <summary>
        /// LoggerSet
        /// </summary>
        /// <typeparam name="Business"></typeparam>
        /// <param name="business"></param>
        /// <param name="logger"></param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public static Business LoggerSet<Business>(Business business, Annotations.LoggerAttribute logger, params string[] parameterName) where Business : IBusiness
        {
            if (null == business) { throw new System.ArgumentNullException(nameof(business)); }

            if (null == logger || null == parameterName) { return business; }

            var argName = parameterName.Where(c => !string.IsNullOrWhiteSpace(c)).ToList();

            if (0 == argName.Count) { return business; }

            System.Threading.Tasks.Parallel.ForEach(business.Configer.MetaData.Values, item =>
            {
                var groups = item.CommandGroup.Group.Values.Where(c => GroupEquals(logger, c.Group));

                if (!groups.Any()) { return; }

                logger.Meta.Declaring = Annotations.AttributeBase.MetaData.DeclaringType.Parameter;

                System.Threading.Tasks.Parallel.ForEach(argName, name =>
                {
                    foreach (var group in groups)
                    {
                        foreach (var arg in item.Args)
                        {
                            if (Equals(arg.Name, name))
                            {
                                arg.Group[group.Key].Logger = GetMetaLogger(arg.Group[group.Key].Logger, logger, group.Group);

                                if (arg.HasIArg)
                                {
                                    arg.Group[group.Key].IArgInLogger = GetMetaLogger(arg.Group[group.Key].IArgInLogger, logger, group.Group);
                                }
                            }
                        }
                    }
                });
            });

            return business;
        }

        /// <summary>
        /// IgnoreSet
        /// </summary>
        /// <typeparam name="Business"></typeparam>
        /// <param name="business"></param>
        /// <param name="ignore"></param>
        /// <param name="argType"></param>
        /// <returns></returns>
        public static Business IgnoreSet<Business>(Business business, Annotations.Ignore ignore, params System.Type[] argType) where Business : IBusiness
        {
            if (null == business) { throw new System.ArgumentNullException(nameof(business)); }

            if (null == ignore || null == argType || 0 == argType.Length) { return business; }

            System.Threading.Tasks.Parallel.ForEach(business.Configer.MetaData.Values, item =>
            {
                var groups = item.CommandGroup.Group.Values.Where(c => GroupEquals(ignore, c.Group));

                if (!groups.Any()) { return; }

                ignore.Meta.Declaring = Annotations.AttributeBase.MetaData.DeclaringType.Parameter;

                System.Threading.Tasks.Parallel.ForEach(argType, type =>
                {
                    foreach (var group in groups)
                    {
                        var hasUpdate = false;

                        foreach (var arg in item.Args)
                        {
                            if (Equals(arg.Type, type) || Equals(arg.IArgOutType, type))
                            {
                                var g = arg.Group[group.Key];
                                if (g.Ignore.Any(c => ignore.GroupKey() == c.GroupKey()))
                                {
                                    continue;
                                }
                                g.Ignore.Collection.Add(ignore);
                                g.IgnoreArg = g.Ignore.Any(c => c.Mode == Annotations.IgnoreMode.Arg);

                                hasUpdate = true;
                            }
                        }

                        if (hasUpdate)
                        {
                            business.Command[group.Group][group.OnlyName].StatisArgs();
                        }
                    }
                });
            });

            return business;
        }

        /// <summary>
        /// IgnoreSet
        /// </summary>
        /// <typeparam name="Business"></typeparam>
        /// <param name="business"></param>
        /// <param name="ignore"></param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public static Business IgnoreSet<Business>(Business business, Annotations.Ignore ignore, params string[] parameterName) where Business : IBusiness
        {
            if (null == business) { throw new System.ArgumentNullException(nameof(business)); }

            if (null == ignore || null == parameterName) { return business; }

            var argName = parameterName.Where(c => !string.IsNullOrWhiteSpace(c)).ToList();

            if (0 == argName.Count) { return business; }

            System.Threading.Tasks.Parallel.ForEach(business.Configer.MetaData.Values, item =>
            {
                var groups = item.CommandGroup.Group.Values.Where(c => GroupEquals(ignore, c.Group));
                //checked group
                if (!groups.Any()) { return; }

                ignore.Meta.Declaring = Annotations.AttributeBase.MetaData.DeclaringType.Parameter;

                System.Threading.Tasks.Parallel.ForEach(argName, name =>
                {
                    foreach (var group in groups)
                    {
                        var hasUpdate = false;

                        foreach (var arg in item.Args)
                        {
                            if (Equals(arg.Name, name))
                            {
                                var g = arg.Group[group.Key];
                                if (g.Ignore.Any(c => ignore.GroupKey() == c.GroupKey()))
                                {
                                    continue;
                                }
                                g.Ignore.Collection.Add(ignore);
                                g.IgnoreArg = g.Ignore.Any(c => c.Mode == Annotations.IgnoreMode.Arg);

                                hasUpdate = true;
                            }
                        }

                        if (hasUpdate)
                        {
                            business.Command[group.Group][group.OnlyName].StatisArgs();
                        }
                    }
                });
            });

            return business;
        }

        /// <summary>
        /// MemberSet
        /// </summary>
        /// <typeparam name="Business"></typeparam>
        /// <param name="business"></param>
        /// <param name="memberName"></param>
        /// <param name="memberObj"></param>
        /// <param name="skipNull"></param>
        /// <returns></returns>
        public static Business MemberSet<Business>(Business business, string memberName, object memberObj, bool skipNull = false) where Business : IBusiness
        {
            if (null == business) { throw new System.ArgumentNullException(nameof(business)); }

            if (!Configer.Accessors.TryGetValue(business.Configer.Info.BusinessName, out Accessors meta) || !meta.Accessor.TryGetValue(memberName, out Accessor accessor)) { return business; }

            if (skipNull && !Equals(null, accessor.Getter(business)))
            {
                return business;
            }

            var value = ChangeType(memberObj, accessor.Type);
            accessor.Setter(business, value);

            business.Configer.MemberSetAfter?.Invoke(memberName, value);

            return business;
        }

        //public static Result.IResult ErrorBusiness(this System.Type resultTypeDefinition, string businessName) => Bind.ErrorBusiness(resultTypeDefinition, businessName);

        /// <summary>
        /// ErrorCmd
        /// </summary>
        /// <param name="business"></param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public static IResult ErrorCmd(IBusiness business, string cmd) => ErrorCmd(business.Configer.ResultTypeDefinition, cmd);

        static MetaLogger GetMetaLogger(MetaLogger metaLogger, Annotations.LoggerAttribute logger, string group)
        {
            var logger2 = logger.Clone();
            logger2.Group = group;

            switch (logger2.LogType)
            {
                case Logger.Type.All:
                    metaLogger.Record = logger2.Clone().SetType(Logger.Type.Record);
                    metaLogger.Error = logger2.Clone().SetType(Logger.Type.Error);
                    metaLogger.Exception = logger2.Clone().SetType(Logger.Type.Exception);
                    break;
                case Logger.Type.Record:
                    metaLogger.Record = logger2;
                    break;
                case Logger.Type.Error:
                    metaLogger.Error = logger2;
                    break;
                case Logger.Type.Exception:
                    metaLogger.Exception = logger2;
                    break;
            }

            return metaLogger;
        }

        //public static string BaseDirectoryCombine(params string[] path) => System.IO.Path.Combine(new string[] { System.AppDomain.CurrentDomain.BaseDirectory }.Concat(path).ToArray());

        /// <summary>
        /// LoadAssemblys
        /// </summary>
        /// <param name="assemblyFiles"></param>
        /// <param name="parallel"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static List<System.Type> LoadAssemblys(IEnumerable<string> assemblyFiles, bool parallel = false, System.Func<System.Type, bool> callback = null)
        {
            var ass = new List<System.Type>();

            if (parallel)
            {
                assemblyFiles.AsParallel().ForAll(item => LoadAssembly(item, ass, callback));
            }
            else
            {
                foreach (var item in assemblyFiles)
                {
                    LoadAssembly(item, ass, callback);
                }
            }

            return ass;
        }

        static void LoadAssembly(string assemblyFile, List<System.Type> ass, System.Func<System.Type, bool> callback = null)
        {
            var assembly = LoadAssembly(assemblyFile);

            if (null != assembly)
            {
                if (null == callback)
                {
                    return;
                }

                try
                {
                    var types = assembly.GetExportedTypes();

                    foreach (var type in types)
                    {
                        if (callback.Invoke(type))
                        {
                            ass.Add(type);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    System.Console.WriteLine(assembly?.Location);
                    ex.Console();
                }
            }
        }

        /// <summary>
        /// LoadAssembly
        /// </summary>
        /// <param name="assemblyFile"></param>
        /// <returns></returns>
        public static Assembly LoadAssembly(string assemblyFile)
        {
            try
            {
                var ass = Assembly.UnsafeLoadFrom(assemblyFile);

                return (null != ass && !ass.IsDynamic) ? ass : null;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                System.Console.WriteLine(assemblyFile);
                ex.Console();
#endif
                return null;
            }
        }

        #region GetAttributes

        /// <summary>
        /// Clone
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public static T Clone<T>(this T attribute) where T : Annotations.AttributeBase => attribute.Clone<T>();

        internal static List<Attribute> Distinct<Attribute>(this List<Attribute> attributes, IEnumerable<Attribute> clones = null, System.Func<Attribute, bool> clonesExclude = null, System.Action<Attribute> attributesAction = null) where Attribute : Annotations.AttributeBase
        {
            var gropus = new List<Annotations.GroupAttribute>();

            for (int i = attributes.Count - 1; i >= 0; i--)
            {
                var item = attributes[i];

                attributesAction?.Invoke(item);

                if (item is Annotations.GroupAttribute)
                {
                    gropus.Add(item as Annotations.GroupAttribute);

                    attributes.RemoveAt(i);
                }
            }

            var attrs = gropus.Distinct(Annotations.GroupAttribute.Comparer).ToDictionary(c => c.GroupKey(), c => c);

            if (null != clones && clones.Any())
            {
                foreach (var item in clones)
                {
                    if (item is Annotations.GroupAttribute)
                    {
                        var item2 = item as Annotations.GroupAttribute;

                        var groupKey = item2.GroupKey();

                        if (!attrs.ContainsKey(groupKey) && !(null != clonesExclude && clonesExclude(item)))
                        {
                            attrs.Add(groupKey, item2.Clone());
                        }
                        //gropus.Add(item.Clone<Attributes.GropuAttribute>());
                    }
                }
            }

            //var attrs = gropus.Distinct(Attributes.GropuAttribute.Comparer);

            foreach (var item in attrs)
            {
                attributes.Add(item.Value as Attribute);
            }

            return attributes;
        }

        internal static List<Attribute> GetAttrs<Attribute>(this IList<Annotations.AttributeBase> attributes, System.Func<Attribute, bool> predicate = null, bool clone = false) where Attribute : Annotations.AttributeBase
        {
            if (null == attributes) { throw new System.ArgumentNullException(nameof(attributes)); }

            var list = new List<Attribute>(attributes.Count);

            foreach (var item in attributes)
            {
                if (item is Attribute)
                {
                    var attr = item as Attribute;

                    if (null != predicate)
                    {
                        if (predicate(attr))
                        {
                            list.Add(clone ? attr.Clone() : attr);
                        }
                    }
                    else
                    {
                        list.Add(clone ? attr.Clone() : attr);
                    }
                }
            }

            //if (null == predicate)
            //{
            //    //return attributes.Where(c => c is Attribute).Cast<Attribute>().ToList();
            //}
            //else
            //{
            //    //return attributes.Where(c => c is Attribute && predicate((Attribute)c)).Cast<Attribute>().ToList();
            //}

            return list;
        }

        internal static Attribute GetAttr<Attribute>(this IList<Annotations.AttributeBase> attributes, System.Func<Attribute, bool> predicate = null) where Attribute : Annotations.AttributeBase
        {
            if (null == attributes) { throw new System.ArgumentNullException(nameof(attributes)); }

            if (null == predicate)
            {
                return attributes.FirstOrDefault(c => c is Attribute) as Attribute;
            }
            else
            {
                return attributes.FirstOrDefault(c => c is Attribute && predicate((Attribute)c)) as Attribute;
            }
        }

        internal static T[] GetAttributes<T>(this MemberInfo member, bool inherit = true) where T : System.Attribute
        {
            if (null == member) { throw new System.ArgumentNullException(nameof(member)); }
            return member.GetCustomAttributes<T>(inherit).ToArray();
            //return (T[])System.Attribute.GetCustomAttributes(member, typeof(T), inherit);
        }
        internal static T GetAttribute<T>(this MemberInfo member, bool inherit = true) where T : System.Attribute
        {
            if (null == member) { throw new System.ArgumentNullException(nameof(member)); }

            if (member.MemberType == MemberTypes.TypeInfo && typeof(object) == (System.Type)member) { return default; }

            return member.GetCustomAttribute<T>(inherit);
        }

        ////public static System.Collections.Generic.List<Attributes.AttributeBase> GetAttributes(this MemberInfo member, bool inherit = true) => member.GetAttributes<Attributes.AttributeBase>(inherit).ToList();

        internal static T[] GetAttributes<T>(this ParameterInfo member, bool inherit = true) where T : System.Attribute
        {
            if (null == member) { throw new System.ArgumentNullException(nameof(member)); }
            return member.GetCustomAttributes<T>(inherit).ToArray();
            //return (T[])System.Attribute.GetCustomAttributes(member, typeof(T), inherit);
        }
        internal static T GetAttribute<T>(this ParameterInfo member, bool inherit = true) where T : System.Attribute
        {
            if (null == member) { throw new System.ArgumentNullException(nameof(member)); }
            return member.GetCustomAttribute<T>(inherit);
        }

        internal static bool Exists<T>(this T[] attrs) where T : System.Attribute
        {
            if (null == attrs) { throw new System.ArgumentNullException(nameof(attrs)); }

            return 0 < attrs.Length;
        }

        internal static bool Exists<T>(this MemberInfo member, bool inherit = true) where T : System.Attribute
        {
            if (null == member) { throw new System.ArgumentNullException(nameof(member)); }

            return member.IsDefined(typeof(T), inherit);
        }
        internal static bool Exists<T>(this ParameterInfo member, bool inherit = true) where T : System.Attribute
        {
            if (null == member) { throw new System.ArgumentNullException(nameof(member)); }

            return member.IsDefined(typeof(T), inherit);
        }

        //public static IEnumerable<T> SetAttributesMeta<T>(this IEnumerable<T> attrs, Attributes.AttributeBase.DeclaringType declaring, dynamic member, System.Action<T> action = null) where T : Attributes.AttributeBase
        //{
        //    var g = attrs.GroupBy(c => c.Meta.Type.FullName);

        //    foreach (var item in g)
        //    {
        //        var i = 0;
        //        foreach (var item2 in item)
        //        {
        //            item2.Meta.Declaring = declaring;
        //            item2.Meta.Member = member;
        //            item2.Meta.Position = i++;
        //            action?.Invoke(item2);
        //        }
        //    }

        //    return attrs;
        //}

        //public static List<T> SetAttributesMeta<T>(this List<T> attrs, Attributes.AttributeBase.DeclaringType declaring, dynamic member) where T : Attributes.AttributeBase => SetAttributesMeta(attrs, declaring, member);

        #endregion

        internal static bool IsAssignableFrom(this System.Type type, System.Type fromType, out System.Type[] genericArguments)
        {
            if (null != type && null != fromType && type.IsGenericType)
            {
                if (type.IsInterface == fromType.IsInterface)
                {
                    if (InInheritanceChain(type, fromType, out genericArguments))
                    {
                        return true;
                    }
                }
                if (type.IsInterface)
                {
                    var interfaces = fromType.GetInterfaces().Select(c => c.GetTypeInfo()).ToArray();
                    for (int i = 0; i < interfaces.Length; i++)
                    {
                        if (InInheritanceChain(type, interfaces[i], out genericArguments))
                        {
                            return true;
                        }
                    }
                }
            }

            genericArguments = null;
            return false;
        }

        static bool InInheritanceChain(System.Type type, System.Type fromType, out System.Type[] genericArguments)
        {
            while (null != fromType)
            {
                if (fromType.IsGenericType)
                {
                    var genericArguments2 = fromType.GetGenericArguments();
                    if (genericArguments2.Length == type.GetGenericArguments().Length)
                    {
                        try
                        {
                            var closedType = (!type.IsGenericTypeDefinition ? type.GetGenericTypeDefinition() : type).MakeGenericType(genericArguments2);
                            if (closedType.GetTypeInfo().IsAssignableFrom(fromType))
                            {
                                genericArguments = genericArguments2.Select(c => c.GetTypeInfo()).ToArray();
                                return true;
                            }
                        }
                        catch (System.ArgumentException ex)
                        {
                            throw ex;
                        }
                    }
                }
                fromType = fromType.BaseType?.GetTypeInfo();
            }
            genericArguments = null;
            return false;
        }
        /*
        public static System.IO.MemoryStream StreamCopy(this System.IO.Stream stream, int bytesLen = 4096)//4k size
        {
            if (null == stream) { throw new System.ArgumentNullException(nameof(stream)); }

            var outStream = new System.IO.MemoryStream();
            var bytes = new byte[bytesLen];
            int count = 0;
            while ((count = stream.Read(bytes, 0, bytesLen)) > 0) { outStream.Write(bytes, 0, count); }
            return outStream;
        }
        */

        /// <summary>
        /// StreamReadByte
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static byte[] StreamReadByte(this System.IO.Stream stream)
        {
            if (null == stream) { throw new System.ArgumentNullException(nameof(stream)); }

            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            return bytes;
        }
        /// <summary>
        /// StreamReadByteAsync
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static async System.Threading.Tasks.ValueTask<byte[]> StreamReadByteAsync(this System.IO.Stream stream)
        {
            if (null == stream) { throw new System.ArgumentNullException(nameof(stream)); }

            var bytes = new byte[stream.Length];
            await stream.ReadAsync(bytes, 0, bytes.Length);
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            return bytes;
        }

        /// <summary>
        /// StreamCopyByte
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static byte[] StreamCopyByte(this System.IO.Stream stream)
        {
            if (null == stream) { throw new System.ArgumentNullException(nameof(stream)); }

            using (var m = new System.IO.MemoryStream())
            {
                stream.CopyTo(m);
                stream.Seek(0, System.IO.SeekOrigin.Begin);
                return m.ToArray();
            }
        }
        /// <summary>
        /// StreamCopyByteAsync
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static async System.Threading.Tasks.ValueTask<byte[]> StreamCopyByteAsync(this System.IO.Stream stream)
        {
            if (null == stream) { throw new System.ArgumentNullException(nameof(stream)); }

            using (var m = new System.IO.MemoryStream())
            {
                await stream.CopyToAsync(m);
                stream.Seek(0, System.IO.SeekOrigin.Begin);
                return m.ToArray();
            }
        }

        /// <summary>
        /// StreamReadString
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string StreamReadString(this System.IO.Stream stream, System.Text.Encoding encoding = null)
        {
            using (var reader = new System.IO.StreamReader(stream, encoding ?? UTF8))
            {
                return reader.ReadToEnd();
            }
        }
        /// <summary>
        /// StreamReadStringAsync
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static async System.Threading.Tasks.ValueTask<string> StreamReadStringAsync(this System.IO.Stream stream, System.Text.Encoding encoding = null)
        {
            using (var reader = new System.IO.StreamReader(stream, encoding ?? UTF8))
            {
                return await reader.ReadToEndAsync();
            }
        }

        /// <summary>
        /// FileReadString
        /// </summary>
        /// <param name="path"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string FileReadString(string path, System.Text.Encoding encoding = null)
        {
            if (string.IsNullOrWhiteSpace(path)) { throw new System.ArgumentException(nameof(path)); }

            using (var fileStream = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
            {
                return fileStream.StreamReadString(encoding ?? UTF8);
            }
        }
        /// <summary>
        /// FileReadStringAsync
        /// </summary>
        /// <param name="path"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static async System.Threading.Tasks.ValueTask<string> FileReadStringAsync(string path, System.Text.Encoding encoding = null)
        {
            if (string.IsNullOrWhiteSpace(path)) { throw new System.ArgumentException(nameof(path)); }

            using (var fileStream = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
            {
                return await fileStream.StreamReadStringAsync(encoding ?? UTF8);
            }
        }
        /// <summary>
        /// FileReadByte
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static byte[] FileReadByte(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) { throw new System.ArgumentException(nameof(path)); }

            using (var fileStream = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
            {
                return fileStream.StreamCopyByte();
            }
        }
        /// <summary>
        /// FileReadByteAsync
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async System.Threading.Tasks.ValueTask<byte[]> FileReadByteAsync(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) { throw new System.ArgumentException(nameof(path)); }

            using (var fileStream = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
            {
                return await fileStream.StreamCopyByteAsync();
            }
        }

        /// <summary>
        /// StreamWrite
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value"></param>
        /// <param name="encoding"></param>
        public static void StreamWrite(this System.IO.Stream stream, string value, System.Text.Encoding encoding = null)
        {
            using (var writer = new System.IO.StreamWriter(stream, encoding ?? UTF8))
            {
                writer.AutoFlush = true;
                writer.Write(value);
            }
        }
        /// <summary>
        /// StreamWriteAsync
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static async System.Threading.Tasks.ValueTask StreamWriteAsync(this System.IO.Stream stream, string value, System.Text.Encoding encoding = null)
        {
            using (var writer = new System.IO.StreamWriter(stream, encoding ?? UTF8))
            {
                writer.AutoFlush = true;
                await writer.WriteAsync(value);
            }
        }

        /// <summary>
        /// gzip to byte[]
        /// </summary>
        /// <param name="value">byte[]</param>
        /// <returns>byte[]</returns>
        public static byte[] GZipDecompressByte(this byte[] value)
        {
            if (null == value) { throw new System.ArgumentNullException(nameof(value)); }

            using (var m = GZipDecompressStream(value)) { return m.ToArray(); }
        }
        /// <summary>
        /// gzip to byte[]
        /// </summary>
        /// <param name="value">byte[]</param>
        /// <returns>MemoryStream</returns>
        public static System.IO.MemoryStream GZipDecompressStream(this byte[] value)
        {
            if (null == value) { throw new System.ArgumentNullException(nameof(value)); }

            using (var m = new System.IO.MemoryStream(value))
            {
                m.Seek(0, System.IO.SeekOrigin.Begin);
                using (var g = new System.IO.Compression.GZipStream(m, System.IO.Compression.CompressionMode.Decompress, true))
                {
                    var m2 = new System.IO.MemoryStream();
                    g.CopyTo(m2);
                    return m2;
                }
            }
        }

        #region Crypto

        /// <summary>
        /// MD5
        /// </summary>
        /// <param name="value"></param>
        /// <param name="hasUpper"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string MD5(this string value, bool hasUpper = false, System.Text.Encoding encoding = null)
        {
            if (null == value) { throw new System.ArgumentNullException(nameof(value)); }

            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var result = System.BitConverter.ToString(md5.ComputeHash((encoding ?? UTF8).GetBytes(value))).Replace("-", string.Empty);
                return hasUpper ? result.ToUpperInvariant() : result.ToLowerInvariant();
            }
        }

        /// <summary>
        /// AES
        /// </summary>
        public static class AES
        {
            /// <summary>
            /// AES return to item1=Data and item2=Salt
            /// </summary>
            /// <param name="input"></param>
            /// <param name="key"></param>
            /// <param name="iv"></param>
            /// <param name="encoding"></param>
            /// <returns></returns>
            public static (string, string) Encrypt(string input, string key, string iv = null, System.Text.Encoding encoding = null)
            {
                if (null == input) { throw new System.ArgumentNullException(nameof(input)); }

                if (string.IsNullOrWhiteSpace(key)) { throw new System.ArgumentNullException(nameof(key)); }

                var encryptKey = (encoding ?? UTF8).GetBytes(key);

                using (var aesAlg = System.Security.Cryptography.Aes.Create())
                {
                    var encryptIV = string.IsNullOrWhiteSpace(iv) ? aesAlg.IV : System.Convert.FromBase64String(iv);

                    using (var encryptor = aesAlg.CreateEncryptor(encryptKey, encryptIV))
                    {
                        using (var m = new System.IO.MemoryStream())
                        {
                            using (var cs = new System.Security.Cryptography.CryptoStream(m, encryptor, System.Security.Cryptography.CryptoStreamMode.Write))
                            {
                                using (var sw = new System.IO.StreamWriter(cs))
                                {
                                    sw.Write(input);
                                }

                                var decryptedContent = m.ToArray();

                                var value = new byte[decryptedContent.Length];

                                System.Buffer.BlockCopy(decryptedContent, 0, value, 0, decryptedContent.Length);

                                return (System.Convert.ToBase64String(value), System.Convert.ToBase64String(encryptIV));
                                //return new { iv = System.Convert.ToBase64String(encryptIV), value = System.Convert.ToBase64String(value) };
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Decrypt
            /// </summary>
            /// <param name="input"></param>
            /// <param name="key"></param>
            /// <param name="iv"></param>
            /// <param name="encoding"></param>
            /// <returns></returns>
            public static string Decrypt(string input, string key, string iv, System.Text.Encoding encoding = null)
            {
                if (null == input) { throw new System.ArgumentNullException(nameof(input)); }

                if (string.IsNullOrWhiteSpace(key)) { throw new System.ArgumentNullException(nameof(key)); }

                if (string.IsNullOrWhiteSpace(iv)) { throw new System.ArgumentNullException(nameof(iv)); }

                var data = System.Convert.FromBase64String(input);

                var decryptIV = System.Convert.FromBase64String(iv);

                var cipher = new byte[data.Length];

                System.Buffer.BlockCopy(data, 0, cipher, 0, data.Length);
                var decryptKey = (encoding ?? UTF8).GetBytes(key);

                using (var aesAlg = System.Security.Cryptography.Aes.Create())
                {
                    using (var decryptor = aesAlg.CreateDecryptor(decryptKey, decryptIV))
                    {
                        using (var m = new System.IO.MemoryStream(cipher))
                        {
                            using (var cs = new System.Security.Cryptography.CryptoStream(m, decryptor, System.Security.Cryptography.CryptoStreamMode.Read))
                            {
                                using (var sr = new System.IO.StreamReader(cs))
                                {
                                    return sr.ReadToEnd();
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        internal static void DeleteFolder(string path)
        {
            if (!System.IO.Directory.Exists(path)) { return; }

            foreach (var entrie in System.IO.Directory.GetFileSystemEntries(path))
            {
                if (System.IO.File.Exists(entrie))
                {
                    var file = new System.IO.FileInfo(entrie);
                    if (-1 != file.Attributes.ToString().IndexOf("ReadOnly"))
                    {
                        file.Attributes = System.IO.FileAttributes.Normal;
                    }
                    try { System.IO.File.Delete(entrie); }
                    catch { }
                }
                else
                {
                    var dir = new System.IO.DirectoryInfo(entrie);
                    if (0 < dir.GetFiles().Length)
                    {
                        DeleteFolder(dir.FullName);
                    }
                    try { System.IO.Directory.Delete(entrie); }
                    catch { }
                }
            }
        }

        //public static bool ContainsAndNotNull(this IDictionary<string, dynamic> dict, string source)
        //{
        //    return null != dict && dict.ContainsKey(source) && !object.Equals(null, dict[source]);
        //}

        #region WriteLocal

        /// <summary>
        /// Write exception to file
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="write"></param>
        /// <param name="console"></param>
        /// <param name="path"></param>
        /// <param name="dateFormat"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static System.Exception ExceptionWrite(this System.Exception ex, bool write = false, bool console = false, string path = "business.log.txt", string dateFormat = "yyyy-MM-dd HH:mm:ss:fff", System.Text.Encoding encoding = null)
        {
            var inner = ex;
            while (null != inner && null != inner.InnerException) { inner = inner.InnerException; }

            if (null == inner || (!write && !console)) { return inner; }

            var message = string.Format("{0}{1}{0}{2}{3}{2}{4}{2}{5}{2}{6}",
                    "========================",//{0}
                    System.DateTime.Now.ToString(dateFormat),//{1}
                    System.Environment.NewLine,//{2}
                    inner.Message,         //{3}
                    inner.Source,          //{4}
                    inner.StackTrace,      //{5}
                    inner?.StackTrace);//{6}

            WriteLocal(message, path, false, write, console, dateFormat, encoding);

            return inner;
        }

        /// <summary>
        /// Console
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="dateFormat"></param>
        public static void Console(this System.Exception ex, string dateFormat = "yyyy-MM-dd HH:mm:ss:fff") => ex.ExceptionWrite(console: true, dateFormat: dateFormat);

        static readonly System.Threading.ReaderWriterLockSlim locker = new System.Threading.ReaderWriterLockSlim();

        /// <summary>
        /// Write text to file
        /// </summary>
        /// <param name="text"></param>
        /// <param name="path"></param>
        /// <param name="autoTime"></param>
        /// <param name="write"></param>
        /// <param name="console"></param>
        /// <param name="dateFormat"></param>
        /// <param name="encoding"></param>
        public static void WriteLocal(string text, string path = "business.log.txt", bool autoTime = true, bool write = true, bool console = false, string dateFormat = "yyyy-MM-dd HH:mm:ss:fff", System.Text.Encoding encoding = null)
        {
            if (string.IsNullOrWhiteSpace(path)) { throw new System.ArgumentException(nameof(path)); }

            if (!write && !console) { return; }

            if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(path)))
            {
                path = System.IO.Path.Combine(BaseDirectory, System.IO.Path.GetFileName(path));
            }

            if (autoTime)
            {
                text = $"[{System.DateTime.Now.ToString(dateFormat)}] {text}";
            }

            if (console) { System.Console.WriteLine(text); }

            if (write)
            {
                locker.EnterWriteLock();

                try
                {
                    var prefix = string.Empty;

                    if (System.IO.File.Exists(path))
                    {
                        using (var fileStream = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
                        {
                            using (var stream = new System.IO.StreamReader(fileStream, encoding ?? UTF8))
                            {
                                if (-1 != stream.Peek()) { prefix = string.Format("{0}{0}", System.Environment.NewLine); }
                            }
                        }
                    }

                    System.IO.File.AppendAllText(path, $"{prefix}{text}", encoding ?? UTF8);
                }
                finally { locker.ExitWriteLock(); }
            }
        }

        /// <summary>
        /// Console
        /// </summary>
        /// <param name="text"></param>
        /// <param name="autoTime"></param>
        /// <param name="console"></param>
        /// <param name="write"></param>
        /// <param name="path"></param>
        /// <param name="dateFormat"></param>
        /// <param name="encoding"></param>
        public static void Console(string text, bool autoTime = true, bool console = true, bool write = false, string path = "business.log.txt", string dateFormat = "yyyy-MM-dd HH:mm:ss:fff", System.Text.Encoding encoding = null) => WriteLocal(text, path, autoTime, write, console, dateFormat, encoding);

        /// <summary>
        /// Ignore erroneous characters: Unable to translate Unicode...
        /// </summary>
        public static System.Text.Encoding UTF8 = System.Text.Encoding.GetEncoding("UTF-8", new System.Text.EncoderReplacementFallback(string.Empty), new System.Text.DecoderExceptionFallback());

        #endregion

        /// <summary>
        /// Ignore erroneous characters: Unable to translate Unicode...
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string UTF8String(this string value) => UTF8.GetString(UTF8.GetBytes(value ?? string.Empty));

        /// <summary>
        /// CheckCharMode
        /// </summary>
        [System.Flags]
        public enum CheckCharMode
        {
            /// <summary>
            /// Allow all
            /// </summary>
            All = 2,
            /// <summary>
            /// Allow number
            /// </summary>
            Number = 4,
            /// <summary>
            /// Allow upper
            /// </summary>
            Upper = 8,
            /// <summary>
            /// Allow lower
            /// </summary>
            Lower = 16,
            /// <summary>
            /// Allow chinese
            /// </summary>
            Chinese = 32
        }

        static readonly System.Predicate<int> number = delegate (int c) { return !(c >= 48 && c <= 57); };
        static readonly System.Predicate<int> upper = delegate (int c) { return !(c >= 65 && c <= 90); };
        static readonly System.Predicate<int> lower = delegate (int c) { return !(c >= 97 && c <= 122); };
        static readonly System.Predicate<int> chinese = delegate (int c) { return !(c >= 0x4e00 && c <= 0x9fbb); };

        /// <summary>
        /// CheckChar
        /// </summary>
        /// <param name="value"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static bool CheckChar(string value, CheckCharMode mode = CheckCharMode.All)
        {
            if (string.IsNullOrWhiteSpace(value)) { return false; }

            var _value = value.Trim();
            var list = new List<int>();
            for (int i = 0; i < _value.Length; i++) { list.Add(_value[i]); }

            if (0 != (mode & CheckCharMode.All))
            {
                return !list.Exists(c => number(c) && upper(c) && lower(c) && chinese(c));
            }

            switch (mode)
            {
                //case CheckCharMode.All:
                //    return !list.Exists(c => number(c) && upper(c) && lower(c) && chinese(c));
                case CheckCharMode.Number:
                    return !list.Exists(c => number(c));
                case CheckCharMode.Upper:
                    return !list.Exists(c => upper(c));
                case CheckCharMode.Lower:
                    return !list.Exists(c => lower(c));
                case CheckCharMode.Chinese:
                    return !list.Exists(c => chinese(c));
                //==============Number==============//
                case CheckCharMode.Number | CheckCharMode.Upper | CheckCharMode.Lower:
                    return !list.Exists(c => number(c) && upper(c) && lower(c));
                case CheckCharMode.Number | CheckCharMode.Upper | CheckCharMode.Chinese:
                    return !list.Exists(c => number(c) && upper(c) && chinese(c));
                case CheckCharMode.Number | CheckCharMode.Lower | CheckCharMode.Chinese:
                    return !list.Exists(c => number(c) && lower(c) && chinese(c));
                case CheckCharMode.Number | CheckCharMode.Upper:
                    return !list.Exists(c => number(c) && upper(c));
                case CheckCharMode.Number | CheckCharMode.Lower:
                    return !list.Exists(c => number(c) && lower(c));
                case CheckCharMode.Number | CheckCharMode.Chinese:
                    return !list.Exists(c => number(c) && chinese(c));
                //==============Upper==============//
                case CheckCharMode.Upper | CheckCharMode.Lower | CheckCharMode.Chinese:
                    return !list.Exists(c => upper(c) && lower(c) && chinese(c));
                case CheckCharMode.Upper | CheckCharMode.Lower:
                    return !list.Exists(c => upper(c) && lower(c));
                case CheckCharMode.Upper | CheckCharMode.Chinese:
                    return !list.Exists(c => upper(c) && chinese(c));
                //==============Lower==============//
                case CheckCharMode.Lower | CheckCharMode.Chinese:
                    return !list.Exists(c => lower(c) && chinese(c));
                case CheckCharMode.Number | CheckCharMode.Upper | CheckCharMode.Lower | CheckCharMode.Chinese:
                    return !list.Exists(c => number(c) && upper(c) && lower(c) && chinese(c));
                default: return false;
            }
        }

        #region Guid
        /*
        /// <summary>
        /// 9 - 10 digit number
        /// </summary>
        /// <returns></returns>
        public static string NewGuidNumber() => System.BitConverter.ToUInt32(System.Guid.NewGuid().ToByteArray(), 0).ToString();

        public static long GuidNumber()
        {
            return (long)System.BitConverter.ToUInt64(System.Guid.NewGuid().ToByteArray(), 0);
        }
        public static string GuidString()
        {
            long i = 1;

            foreach (byte b in System.Guid.NewGuid().ToByteArray())
            {
                i *= b + 1;
            }

            return string.Format("{0:x}", i - System.DateTime.Now.Ticks);
        }
        */

        /// <summary>
        /// ToString("N")
        /// </summary>
        public static string Guid => System.Guid.NewGuid().ToString("N");

        #endregion

        //public static string HumanReadableFilesize(double size)
        //{
        //    var units = new string[] { "B", "KB", "MB", "GB", "TB", "PB" };
        //    double mod = 1024.0;
        //    int i = 0;
        //    while (size >= mod)
        //    {
        //        size /= mod;
        //        i++;
        //    }
        //    return System.Math.Round(size, 2) + units[i];
        //}

#if NETFX
        public static void MailSend(this string subject, string content, string from, string displayName, string host, string credentialsUserName, string credentialsPassword, int port = 25, bool enableSsl = false, System.Text.Encoding contentEncoding = null, string mediaType = "text/html", params string[] to)
        {
            using (var mailMsg = new System.Net.Mail.MailMessage())
            {
                mailMsg.From = new System.Net.Mail.MailAddress(from, displayName);
                foreach (var item in to) { mailMsg.CC.Add(item); }
                mailMsg.Subject = subject;
                using (var view = System.Net.Mail.AlternateView.CreateAlternateViewFromString(content, contentEncoding, mediaType))
                {
                    mailMsg.AlternateViews.Add(view);
                    using (var smtpClient = new System.Net.Mail.SmtpClient(host, port))
                    {
                        smtpClient.EnableSsl = enableSsl;
                        smtpClient.Credentials = new System.Net.NetworkCredential(credentialsUserName, credentialsPassword);
                        smtpClient.Send(mailMsg);
                    }
                }
            }
        }
#endif
        /// <summary>
        /// general object creation
        /// </summary>
        /// <param name="targetType"></param>
        /// <returns></returns>
        internal static dynamic CreateInstance(System.Type targetType)
        {
            //string test first - it has no parameterless constructor
            if (System.Type.GetTypeCode(targetType) == System.TypeCode.String)
            {
                return string.Empty;
            }

            // get the default constructor and instantiate
            var types = System.Array.Empty<System.Type>();
            var info = targetType.GetConstructor(types);
            object targetObject;

            if (info == null) //must not have found the constructor
            {
                if (targetType.BaseType.UnderlyingSystemType.FullName.Contains("Enum"))
                {
                    targetObject = System.Activator.CreateInstance(targetType);
                }
                else
                {
                    throw new System.ArgumentException("Unable to instantiate type: " + targetType.AssemblyQualifiedName + " - Constructor not found");
                }
            }
            else
            {
                targetObject = info.Invoke(null);
            }

            if (targetObject == null)
            {
                throw new System.ArgumentException("Unable to instantiate type: " + targetType.AssemblyQualifiedName + " - Unknown Error");
            }

            return targetObject;
        }

        internal static string GetMethodFullName(this MethodInfo methodInfo)
        {
            if (null == methodInfo) { throw new System.ArgumentNullException(nameof(methodInfo)); }

            return $"{ methodInfo.DeclaringType.FullName}.{ methodInfo.Name}";
        }

        /// <summary>
        /// Split
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static IEnumerable<T[]> Split<T>(this IEnumerable<T> source, int length)
        {
            if (null == source) { throw new System.ArgumentNullException(nameof(source)); }

            var source2 = source.ToArray();
            var span = new System.Span<T>(source2);

            var sp = new LinkedList<T[]>();

            for (int i = 0; i < source2.Length; i += length)
            {
                sp.AddLast(span.Slice(i, i + length > source2.Length ? source2.Length - i : length).ToArray());
            }

            return sp;
        }

        /// <summary>
        /// CompareEquals
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectFromCompare"></param>
        /// <param name="objectToCompare"></param>
        /// <returns></returns>
        public static bool CompareEquals<T>(T objectFromCompare, T objectToCompare)
        {
            if (objectFromCompare == null && objectToCompare == null)
            {
                return true;
            }
            else if (objectFromCompare == null && objectToCompare != null)
            {
                return false;
            }
            else if (objectFromCompare != null && objectToCompare == null)
            {
                return false;
            }

            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in props)
            {
                var dataFromCompare =
                objectFromCompare.GetType().GetProperty(prop.Name).GetValue(objectFromCompare, null);

                var dataToCompare =
                objectToCompare.GetType().GetProperty(prop.Name).GetValue(objectToCompare, null);

                var type =
                objectFromCompare.GetType().GetProperty(prop.Name).GetValue(objectToCompare, null).GetType();

                if (prop.PropertyType.GetTypeInfo().IsClass && !prop.PropertyType.Equals(typeof(string)))
                {
                    dynamic convertedFromValue = System.Convert.ChangeType(dataFromCompare, type);
                    dynamic convertedToValue = System.Convert.ChangeType(dataToCompare, type);

                    var result = CompareEquals(convertedFromValue, convertedToValue);

                    bool compareResult = (bool)result;
                    if (!compareResult)
                    {
                        return false;
                    }
                }
                else if (!dataFromCompare.Equals(dataToCompare))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// ChangeType
        /// </summary>
        /// <typeparam name="Type"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Type ChangeType<Type>(object value) => (Type)ChangeType(value, typeof(Type));

        /// <summary>
        /// ChangeType
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object ChangeType(object value, System.Type type)
        {
            if (null == type) { throw new System.ArgumentNullException(nameof(type)); }

            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsGenericType &&
        type.GetGenericTypeDefinition().Equals(typeof(System.Nullable<>)))
            {
                if (value == null) { return null; }

                //var nullableConverter = new System.ComponentModel.NullableConverter(type);

                type = System.Nullable.GetUnderlyingType(type);
            }

            if (null == value)
            {
                return typeInfo.IsValueType ? System.Activator.CreateInstance(type) : null;
            }

            if (typeInfo.IsEnum)
            {
                return value is string ? System.Enum.Parse(type, value as string, true) : System.Enum.ToObject(type, value);
            }

            try
            {
                return System.Convert.ChangeType(value, type);
            }
            catch
            {
                //return typeInfo.IsValueType ? System.Activator.CreateInstance(type) : null;
                return value;
            }
        }

        /// <summary>
        /// Random
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public static int Random(int minValue, int maxValue)
        {
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                return new System.Random(System.BitConverter.ToInt32(bytes, 0)).Next(minValue, maxValue);
            }
        }
        /// <summary>
        /// Random
        /// </summary>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public static int Random(this int maxValue)
        {
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                return new System.Random(System.BitConverter.ToInt32(bytes, 0)).Next(maxValue);
            }
        }
        /// <summary>
        /// Random
        /// </summary>
        /// <returns></returns>
        public static double Random()
        {
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                return new System.Random(System.BitConverter.ToInt32(bytes, 0)).NextDouble();
            }
        }

        //public static bool CheckEmail(this string email)
        //{
        //    if (string.IsNullOrWhiteSpace(email))
        //    {
        //        return false;
        //    }

        //    return System.Text.RegularExpressions.Regex.IsMatch(email.Trim(), @"^([0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9})$");
        //}

        /// <summary>
        /// Scale
        /// </summary>
        /// <param name="value"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static double Scale(this double value, int size = 2)
        {
            var p = System.Math.Pow(10, size);
            return (int)(value * (int)p) / p;
        }
        /// <summary>
        /// Scale
        /// </summary>
        /// <param name="value"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static decimal Scale(this decimal value, int size = 2)
        {
            //var sp = System.Convert.ToDecimal(System.Math.Pow(10, size));
            //var t = System.Math.Truncate(value);

            //var result = t + (0 > value ? System.Math.Ceiling((value - t) * sp) : System.Math.Floor((value - t) * sp)) / sp;

            //return result;

            var p = System.Math.Pow(10, size);
            return (decimal)((int)(value * (int)p) / p);

            //return System.Convert.ToDecimal(value.ToString("N", new System.Globalization.NumberFormatInfo { NumberDecimalDigits = size }));
        }

        //public static string ConvertTime2(this System.DateTime time)
        //{
        //    return time.ToString("yyyyMMddHHmmssfffffff");
        //}
        //public static System.DateTime ConvertTime2(this string time)
        //{
        //    return System.DateTime.ParseExact(time, "yyyyMMddHHmmssfffffff", null);
        //}

        /// <summary>
        /// GetName
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetName(this System.Enum value) => null == value ? null : System.Enum.GetName(value.GetType(), value);
        //public static int? GetValue(this System.Enum value) => value?.GetHashCode();

        //public static System.Collections.IList Adde(this System.Collections.IList list, params object[] item)
        //{
        //    if (null == list) { throw new System.ArgumentNullException(nameof(list)); }

        //    if (null == item || 0 == item.Length) { return list; }

        //    var list2 = System.Collections.ArrayList.Adapter(list);

        //    list2.AddRange(item);

        //    return list2;
        //}

        //public static string AddeJson(this string json, params object[] item)
        //{
        //    var list = string.IsNullOrEmpty(json) ? new System.Collections.ArrayList() : json.TryJsonDeserialize<System.Collections.IList>() ?? new System.Collections.ArrayList { json };

        //    return list.Adde(item).JsonSerialize();
        //}

        //public static bool SpinWait(this int millisecondsTimeout) => System.Threading.SpinWait.SpinUntil(() => false, millisecondsTimeout);

        //public static bool SpinWait(this System.TimeSpan timeout) => System.Threading.SpinWait.SpinUntil(() => false, timeout);

        static readonly List<string> SysTypes = Assembly.GetExecutingAssembly().GetType().Module.Assembly.GetExportedTypes().Select(c => c.FullName).ToList();

        /// <summary>
        /// IsDefinition
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsDefinition(this System.Type type) => !SysTypes.Contains(type.FullName) && (type.IsClass || (type.IsValueType && !type.IsEnum && !type.IsArray && !type.IsCollection() && !type.IsEnumerable()));

        #region Json
        /*
        public static Type TryJsonDeserialize<Type>(this string value, Newtonsoft.Json.JsonSerializerSettings settings)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return default;
            }

            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<Type>(value, settings);
            }
            catch (System.Exception)
            {
                return default;
            }
        }
        public static Type TryJsonDeserialize<Type>(this string value, params Newtonsoft.Json.JsonConverter[] converters)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return default;
            }

            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<Type>(value, converters);
            }
            catch
            {
                return default;
            }
        }
        public static Type TryJsonDeserialize<Type>(this string value, out string error)
        {
            error = null;

            if (string.IsNullOrWhiteSpace(value))
            {
                return default;
            }

            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<Type>(value);
            }
            catch (System.Exception ex)
            {
                error = System.Convert.ToString(ex);
                return default;
            }
        }
        public static bool TryJsonDeserialize<Type>(this string value, System.Type type, out Type result)
        {
            try
            {
                result = (Type)Newtonsoft.Json.JsonConvert.DeserializeObject(value, type);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }
        public static bool TryJsonDeserialize<Type>(this string value, out Type result)
        {
            try
            {
                result = Newtonsoft.Json.JsonConvert.DeserializeObject<Type>(value);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }
        public static object TryJsonDeserialize(this string value, System.Type type, out string error)
        {
            error = null;

            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject(value, type);
            }
            catch (System.Exception ex)
            {
                error = System.Convert.ToString(ex);
                return null;
            }
        }
        public static string JsonSerialize<Type>(this Type value, params Newtonsoft.Json.JsonConverter[] converters)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(value, converters);
        }
        public static string JsonSerialize<Type>(this Type value, Newtonsoft.Json.JsonSerializerSettings settings)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(value, settings);
        }
        */
        /*
        /// <summary>
        /// TryJsonDeserializeStringArray
        /// </summary>
        /// <param name="value"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string[] TryJsonDeserializeStringArray(this string value, System.Text.Json.JsonSerializerOptions options = null)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return default;
            }

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement[]>(value, options ?? JsonOptions).Select(c => c.ToString()).ToArray();
            }
            catch
            {
                return default;
            }
        }
        */
        /// <summary>
        /// TryJsonDeserialize
        /// </summary>
        /// <typeparam name="Type"></typeparam>
        /// <param name="value"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static Type TryJsonDeserialize<Type>(this string value, System.Text.Json.JsonSerializerOptions options = null)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return default;
            }

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<Type>(value, options ?? JsonOptions);
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// TryJsonDeserialize
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static object TryJsonDeserialize(this string value, System.Type type, System.Text.Json.JsonSerializerOptions options = null)
        {
            if (string.IsNullOrWhiteSpace(value) || type is null)
            {
                return default;
            }

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize(value, type, options ?? JsonOptions);
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// JsonSerialize
        /// </summary>
        /// <typeparam name="Type"></typeparam>
        /// <param name="value"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string JsonSerialize<Type>(this Type value, System.Text.Json.JsonSerializerOptions options = null) => System.Text.Json.JsonSerializer.Serialize(value, options ?? JsonOptions);

        /// <summary>
        /// JsonOptions
        /// </summary>
        public static System.Text.Json.JsonSerializerOptions JsonOptions = new System.Text.Json.JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        /// <summary>
        /// TryJsonSerialize
        /// </summary>
        /// <typeparam name="Type"></typeparam>
        /// <param name="value"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string TryJsonSerialize<Type>(this Type value, System.Text.Json.JsonSerializerOptions options = null)
        {
            try
            {
                return System.Text.Json.JsonSerializer.Serialize(value, options ?? JsonOptions);
            }
            catch (System.Exception ex)
            {
                return System.Convert.ToString(ex.ExceptionWrite());
            }
        }

        //public static string TryJsonSerialize<Type>(this Type value, Newtonsoft.Json.JsonSerializerSettings settings = null)
        //{
        //    try
        //    {
        //        return Newtonsoft.Json.JsonConvert.SerializeObject(value, settings);
        //    }
        //    catch (System.Exception ex)
        //    {
        //        return Newtonsoft.Json.JsonConvert.SerializeObject(ex);
        //    }
        //}

        #endregion

        #region ProtoBuf Serialize
        /*
        public static Type TryProtoBufDeserialize<Type>(this byte[] source)
        {
            try
            {
                using (var stream = new System.IO.MemoryStream(source))
                {
                    return ProtoBuf.Serializer.Deserialize<Type>(stream);
                }
            }
            catch { return default(Type); }
        }
        public static bool TryProtoBufDeserialize<Type>(this byte[] source, System.Type type, out Type result)
        {
            try
            {
                using (var stream = new System.IO.MemoryStream(source))
                {
                    result = (Type)ProtoBuf.Serializer.Deserialize(type, stream);
                    return true;
                }
            }
            catch
            {
                result = default(Type);
                return false;
            }
        }
        public static bool TryProtoBufDeserialize<Type>(this byte[] source, out Type result)
        {
            try
            {
                using (var stream = new System.IO.MemoryStream(source))
                {
                    result = ProtoBuf.Serializer.Deserialize<Type>(stream);
                    return true;
                }
            }
            catch
            {
                result = default(Type);
                return false;
            }
        }
        public static byte[] ProtoBufSerialize<Type>(this Type instance)
        {
            using (var stream = new System.IO.MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(stream, instance);
                return stream.ToArray();
            }
        }
        public static object ProtoBufDeserialize(this byte[] source, System.Type type)
        {
            using (var stream = new System.IO.MemoryStream(source))
            {
                return ProtoBuf.Meta.RuntimeTypeModel.Default.Deserialize(stream, null, type);
            }
        }
        public static object TryProtoBufDeserialize(this byte[] source, System.Type type)
        {
            try
            {
                using (var stream = new System.IO.MemoryStream(source))
                {
                    return ProtoBuf.Meta.RuntimeTypeModel.Default.Deserialize(stream, null, type);
                }
            }
            catch { return null; }
        }
        */
        #endregion

        #region Nancy Copy

        /// <summary>
        /// GetTypeCode
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static System.TypeCode GetTypeCode(this System.Type type)
        {
            switch (type.FullName)
            {
                case "System.Boolean": return System.TypeCode.Boolean;
                case "System.Char": return System.TypeCode.Char;
                case "System.SByte": return System.TypeCode.SByte;
                case "System.Byte": return System.TypeCode.Byte;
                case "System.Int16": return System.TypeCode.Int16;
                case "System.UInt16": return System.TypeCode.UInt16;
                case "System.Int32": return System.TypeCode.Int32;
                case "System.UInt32": return System.TypeCode.UInt32;
                case "System.Int64": return System.TypeCode.Int64;
                case "System.UInt64": return System.TypeCode.UInt64;
                case "System.Single": return System.TypeCode.Single;
                case "System.Double": return System.TypeCode.Double;
                case "System.Decimal": return System.TypeCode.Decimal;
                case "System.DateTime": return System.TypeCode.DateTime;
                case "System.String": return System.TypeCode.String;
                case "System.Enum": return GetTypeCode(System.Enum.GetUnderlyingType(type));
                default: return System.TypeCode.Object;
            }
        }

        /// <summary>
        /// Checks if a type is an array or not
        /// </summary>
        /// <param name="source">The type to check.</param>
        /// <returns><see langword="true" /> if the type is an array, otherwise <see langword="false" />.</returns>
        public static bool IsArray(this System.Type source)
        {
            return source.GetTypeInfo().BaseType == typeof(System.Array);
        }
        /// <summary>
        /// Checks if a type is an collection or not
        /// </summary>
        /// <param name="source">The type to check.</param>
        /// <returns><see langword="true" /> if the type is an collection, otherwise <see langword="false" />.</returns>
        public static bool IsCollection(this System.Type source)
        {
            var collectionType = typeof(ICollection<>);

            return source.GetTypeInfo().IsGenericType && source
                .GetInterfaces()
                .Any(i => i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == collectionType);
        }
        /// <summary>
        /// Checks if a type is enumerable or not
        /// </summary>
        /// <param name="source">The type to check.</param>
        /// <returns><see langword="true" /> if the type is an enumerable, otherwise <see langword="false" />.</returns>
        public static bool IsEnumerable(this System.Type source)
        {
            var enumerableType = typeof(IEnumerable<>);

            return source.GetTypeInfo().IsGenericType && source.GetGenericTypeDefinition() == enumerableType;
        }

        /// <summary>
        /// Determines if a type is numeric.  Nullable numeric types are considered numeric.
        /// </summary>
        /// <remarks>
        /// Boolean is not considered numeric.
        /// </remarks>
        public static bool IsNumeric(this System.Type source)
        {
            if (source == null)
            {
                return false;
            }

            var underlyingType = System.Nullable.GetUnderlyingType(source) ?? source;

            if (underlyingType.GetTypeInfo().IsEnum)
            {
                return false;
            }

            switch (underlyingType.GetTypeCode())
            {
                case System.TypeCode.Byte:
                case System.TypeCode.Decimal:
                case System.TypeCode.Double:
                case System.TypeCode.Int16:
                case System.TypeCode.Int32:
                case System.TypeCode.Int64:
                case System.TypeCode.SByte:
                case System.TypeCode.Single:
                case System.TypeCode.UInt16:
                case System.TypeCode.UInt32:
                case System.TypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }
        /// <summary>
        /// Filters our all types not assignable to <typeparamref name="TType"/>.
        /// </summary>
        /// <typeparam name="TType">The type that all resulting <see cref="Type"/> should be assignable to.</typeparam>
        /// <param name="types">An <see cref="IEnumerable{T}"/> of <see cref="Type"/> instances that should be filtered.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Type"/> instances.</returns>
        public static IEnumerable<System.Type> NotOfType<TType>(this IEnumerable<System.Type> types)
        {
            return types.Where(t => !typeof(TType).IsAssignableFrom(t));
        }

        /// <summary>
        /// Determines whether the <paramref name="genericType"/> is assignable from
        /// <paramref name="givenType"/> taking into account generic definitions
        /// </summary>
        /// <remarks>
        /// Borrowed from: http://tmont.com/blargh/2011/3/determining-if-an-open-generic-type-isassignablefrom-a-type
        /// </remarks>
        public static bool IsAssignableToGenericType(this System.Type givenType, System.Type genericType)
        {
            if (givenType == null || genericType == null)
            {
                return false;
            }
            return givenType == genericType
                || givenType.MapsToGenericTypeDefinition(genericType)
                || givenType.HasInterfaceThatMapsToGenericTypeDefinition(genericType)
                || givenType.GetTypeInfo().BaseType.IsAssignableToGenericType(genericType);
        }

        private static bool HasInterfaceThatMapsToGenericTypeDefinition(this System.Type givenType, System.Type genericType)
        {
            return givenType
                .GetInterfaces()
                .Where(it => it.GetTypeInfo().IsGenericType)
                .Any(it => it.GetGenericTypeDefinition() == genericType);
        }

        private static bool MapsToGenericTypeDefinition(this System.Type givenType, System.Type genericType)
        {
            return genericType.GetTypeInfo().IsGenericTypeDefinition
                && givenType.GetTypeInfo().IsGenericType
                && givenType.GetGenericTypeDefinition() == genericType;
        }

        #endregion

        /// <summary>
        /// ToReadOnly
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <returns></returns>
        public static ReadOnlyCollection<T> ToReadOnly<T>(this IEnumerable<T> values) => new ReadOnlyCollection<T>(values);

        /// <summary>
        /// ToReadOnlyDictionary
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TElement"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <param name="elementSelector"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        public static ConcurrentReadOnlyDictionary<TKey, TElement> ToReadOnlyDictionary<TKey, TSource, TElement>(this IEnumerable<TSource> source, System.Func<TSource, TKey> keySelector, System.Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer = null)
        {
            var dictionary = new System.Collections.Concurrent.ConcurrentDictionary<TKey, TElement>(comparer);

            foreach (var item in source)
            {
                dictionary.TryAdd(keySelector(item), elementSelector(item));
            }

            return new ConcurrentReadOnlyDictionary<TKey, TElement>(dictionary);
        }

        /// <summary>
        /// DateTimeConverter
        /// </summary>
        public class DateTimeConverter : System.Text.Json.Serialization.JsonConverter<System.DateTime>
        {
            /// <summary>
            /// Reads and converts the JSON to type DateTime.
            /// </summary>
            /// <param name="reader"></param>
            /// <param name="typeToConvert"></param>
            /// <param name="options"></param>
            /// <returns></returns>
            public override System.DateTime Read(ref System.Text.Json.Utf8JsonReader reader, System.Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
            {
                if (!reader.TryGetDateTime(out System.DateTime value))
                {
                    if (reader.ValueSpan.IsEmpty)
                    {
                        return value;
                    }

                    return System.DateTime.Parse(reader.GetString());
                }

                return value;
                //!reader.TryGetDateTime(out System.DateTime value) ? System.DateTime.Parse(reader.GetString()) : value;
            }

            /// <summary>
            /// Writes a specified value as JSON.
            /// </summary>
            /// <param name="writer"></param>
            /// <param name="value"></param>
            /// <param name="options"></param>
            public override void Write(System.Text.Json.Utf8JsonWriter writer, System.DateTime value, System.Text.Json.JsonSerializerOptions options) => writer.WriteStringValue(value.ToLocalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"));
        }

        /// <summary>
        /// FirstCharToLowerNamingPolicy
        /// </summary>
        public class FirstCharToLowerNamingPolicy : System.Text.Json.JsonNamingPolicy
        {
            /// <summary>
            /// ConvertName
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            public override string ConvertName(string name) => FirstCharToLower(name);
        }

        internal static string GetTypeName(this System.Type type, System.Type declaringType = null)
        {
            if (null == type) { throw new System.ArgumentNullException(nameof(type)); }

            if (null != declaringType)
            {
                while (typeof(object) != type.BaseType && type.IsClass && type.IsDefinition())
                {
                    if (type.Equals(declaringType))
                    {
                        break;
                    }
                    type = type.BaseType;
                }
            }

            return (type.IsGenericType ? null == type.DeclaringType ? $"{type.Namespace}.{type.Name}" : $"{type.DeclaringType.GetTypeName()}.{type.Name}" : type.FullName).Replace('+', '.');
        }

        internal static bool Contains<T>(this T @enum, T value) where T : System.Enum => 0 != (@enum & (dynamic)value);

        /*
        public static dynamic Call(this IBusiness business, dynamic data, string remote, string group = null, Http.IHttpRequest httpRequest = null, string commandID = null)
        {
            var request = business.Configuration.RequestDefault.Create(data, business.Configuration.RequestType);

            var resultType = business.Configuration.ResultType;

            if (System.Object.Equals(business.Configuration.RequestDefault, request)) { return Result.ResultFactory.ResultCreate(resultType, (int)Request.Mark.MarkItem.Business_DataError, Request.Mark.DataNull); }

            try
            {
                //checked Remote
                if (!System.String.IsNullOrWhiteSpace(remote)) { request.Remote = remote; }
                if (System.String.IsNullOrWhiteSpace(request.Remote)) { return Result.ResultFactory.ResultCreate(resultType, (int)Request.Mark.MarkItem.Exp_RemoteIllegal, Request.Mark.RemoteNull); }

                //checker Cmd
                if (System.String.IsNullOrWhiteSpace(request.Cmd)) { return Result.ResultFactory.ResultCreate(resultType, (int)Request.Mark.MarkItem.Business_CmdError, Request.Mark.CmdNull); }

                //checked Group
                if (!System.String.IsNullOrWhiteSpace(group)) { request.Group = group; }
                if (System.String.IsNullOrWhiteSpace(request.Group)) { request.Group = Bind.CommandGroupDefault; }

                //get Group
                if (!business.Command.TryGetValue(request.Group, out System.Collections.Generic.IReadOnlyDictionary<string, Command> cmdGroup))
                {
                    return Result.ResultFactory.ResultCreate(resultType, (int)Request.Mark.MarkItem.Business_GroupError, string.Format(Request.Mark.GroupError, request.Group));
                }

                //get Cmd
                if (!cmdGroup.TryGetValue(request.Cmd, out Command command))
                {
                    return Result.ResultFactory.ResultCreate(resultType, (int)Request.Mark.MarkItem.Business_CmdError, string.Format(Request.Mark.CmdError, request.Cmd));
                }

                var meta = business.Configuration.MetaData[command.Name];

                var args = new object[meta.ArgAttrs[meta.GroupDefault].Args.Count];

#region Token

                if (0 < meta.TokenPosition.Length)
                {
                    var token = business.Configuration.Token();
                    token.Key = request.Token;
                    token.Remote = request.Remote;
                    token.CommandID = commandID;

                    foreach (var item in meta.TokenPosition)
                    {
                        args[item] = token;
                    }
                }

#endregion

#region HttpRequest

                if (0 < meta.HttpRequestPosition.Length && null != httpRequest)
                {
                    if (null != httpRequest.Files && 0 == httpRequest.Files.Count)
                    {
                        httpRequest.Files = null;
                    }

                    if (null != httpRequest.Cookies && 0 == httpRequest.Cookies.Count)
                    {
                        httpRequest.Cookies = null;
                    }

                    if (null != httpRequest.Headers && 0 == httpRequest.Headers.Count)
                    {
                        httpRequest.Headers = null;
                    }

                    foreach (var item in meta.HttpRequestPosition)
                    {
                        args[item] = httpRequest;
                    }
                }

#endregion

                if (0 < request.Data.Length && 0 < args.Length)
                {
                    int l = 0;
                    for (int i = 0; i < args.Length; i++)
                    {
                        if (meta.TokenPosition.Contains(i) || meta.HttpRequestPosition.Contains(i)) { continue; }

                        if (request.Data.Length < l++)
                        {
                            break;
                        }

                        if (l - 1 < request.Data.Length)
                        {
                            args[i] = request.Data[l - 1];
                        }
                    }
                }

                var result = command.Call(args);

                if (!command.HasReturn) { return null; }

                if (command.HasIResult)
                {
                    if (System.Object.Equals(null, result))
                    {
                        result = Result.ResultFactory.ResultCreate(resultType);
                    }

                    result.Callback = request.Callback;
                }

                return result;

                //====================================//
                //var type = typeof(T);
                //var result2 = business.ResultCreateToDataBytes(result);
                //result2.Callback = businessData.Callback;

                //return result2;
            }
            catch (System.Exception ex)
            {
                ex = Utils.Help.ExceptionWrite(ex, console: true);
                //...
                return Result.ResultFactory.ResultCreate(resultType, (int)Request.Mark.MarkItem.Exp_UndefinedException, ex.Message);
            }
        }
        */
    }
    /*
#region ICloneable

    /// <summary>  
    /// Interface definition for cloneable objects.  
    /// </summary>  
    /// <typeparam name="T">Type of the cloneable objects.</typeparam>  
    public interface ICloneable<T>
        where T : ICloneable<T>
    {
        /// <summary>  
        /// Clones this instance.  
        /// </summary>  
        /// <returns>The cloned instance.</returns>  
        T Clone();
    }

#endregion
    */

    /// <summary>
    /// ReadOnlyDictionary
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class ReadOnlyDictionary<TKey, TValue> : System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>
    {
        internal readonly IDictionary<TKey, TValue> dictionary;

        /// <summary>
        /// ReadOnlyDictionary
        /// </summary>
        /// <param name="dictionary"></param>
        public ReadOnlyDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) => this.dictionary = dictionary;

        /// <summary>
        /// ReadOnlyDictionary
        /// </summary>
        public ReadOnlyDictionary() : this(new Dictionary<TKey, TValue>()) { }

        /// <summary>
        /// ReadOnlyDictionary
        /// </summary>
        /// <param name="comparer"></param>
        public ReadOnlyDictionary(IEqualityComparer<TKey> comparer) : this(new Dictionary<TKey, TValue>(comparer)) { }
    }

    /// <summary>
    /// ConcurrentReadOnlyDictionary
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class ConcurrentReadOnlyDictionary<TKey, TValue> : System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>
    {
        internal readonly System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue> dictionary;

        /// <summary>
        /// ConcurrentReadOnlyDictionary
        /// </summary>
        /// <param name="dictionary"></param>
        public ConcurrentReadOnlyDictionary(System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue> dictionary) : base(dictionary) => this.dictionary = dictionary;

        /// <summary>
        /// ConcurrentReadOnlyDictionary
        /// </summary>
        public ConcurrentReadOnlyDictionary() : this(new System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>()) { }

        /// <summary>
        /// ConcurrentReadOnlyDictionary
        /// </summary>
        /// <param name="comparer"></param>
        public ConcurrentReadOnlyDictionary(IEqualityComparer<TKey> comparer) : this(new System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>(comparer)) { }

        //public virtual TValue TryGetValue(TKey key)
        //{
        //    if (System.Object.Equals(null, key))
        //    {
        //        return default;
        //    }

        //    this.TryGetValue(key, out TValue value);

        //    return value;
        //}
    }

    /// <summary>
    /// ReadOnlyCollection
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class ReadOnlyCollection<TValue> : System.Collections.ObjectModel.ReadOnlyCollection<TValue>
    {
        /// <summary>
        /// Empty
        /// </summary>
        public static readonly ReadOnlyCollection<TValue> Empty = new ReadOnlyCollection<TValue>(System.Array.Empty<TValue>());

        internal IList<TValue> Collection { get => Items; }

        /// <summary>
        /// ReadOnlyCollection
        /// </summary>
        /// <param name="collection"></param>
        public ReadOnlyCollection(IList<TValue> collection) : base(collection) { }

        /// <summary>
        /// ReadOnlyCollection
        /// </summary>
        public ReadOnlyCollection() : this(new List<TValue>()) { }

        /// <summary>
        /// ReadOnlyCollection
        /// </summary>
        /// <param name="capacity"></param>
        public ReadOnlyCollection(int capacity) : this(new List<TValue>(capacity)) { }

        /// <summary>
        /// ReadOnlyCollection
        /// </summary>
        /// <param name="values"></param>
        public ReadOnlyCollection(IEnumerable<TValue> values) : this(new List<TValue>(values)) { }
    }

    #region Equals

    /// <summary>
    /// Equality
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class Equality<T>
    {
        /// <summary>
        /// CreateComparer
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        public static IEqualityComparer<T> CreateComparer<V>(System.Func<T, V> keySelector)
        {
            return new CommonEqualityComparer<V>(keySelector);
        }
        /// <summary>
        /// CreateComparer
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <param name="keySelector"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        public static IEqualityComparer<T> CreateComparer<V>(System.Func<T, V> keySelector, IEqualityComparer<V> comparer)
        {
            return new CommonEqualityComparer<V>(keySelector, comparer);
        }

        class CommonEqualityComparer<V> : IEqualityComparer<T>
        {
            private readonly System.Func<T, V> keySelector;
            private readonly IEqualityComparer<V> comparer;

            public CommonEqualityComparer(System.Func<T, V> keySelector, IEqualityComparer<V> comparer)
            {
                this.keySelector = keySelector;
                this.comparer = comparer;
            }
            public CommonEqualityComparer(System.Func<T, V> keySelector)
                : this(keySelector, EqualityComparer<V>.Default) { }

            public bool Equals(T x, T y)
            {
                return comparer.Equals(keySelector(x), keySelector(y));
            }
            public int GetHashCode(T obj)
            {
                return comparer.GetHashCode(keySelector(obj));
            }
        }
    }

    /// <summary>
    /// ComparisonHelper
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class ComparisonHelper<T>
    {
        /// <summary>
        /// CreateComparer
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        public static IComparer<T> CreateComparer<V>(System.Func<T, V> keySelector)
        {
            return new CommonComparer<V>(keySelector);
        }
        /// <summary>
        /// CreateComparer
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <param name="keySelector"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        public static IComparer<T> CreateComparer<V>(System.Func<T, V> keySelector, IComparer<V> comparer)
        {
            return new CommonComparer<V>(keySelector, comparer);
        }

        class CommonComparer<V> : IComparer<T>
        {
            private readonly System.Func<T, V> keySelector;
            private readonly IComparer<V> comparer;

            public CommonComparer(System.Func<T, V> keySelector, IComparer<V> comparer)
            {
                this.keySelector = keySelector;
                this.comparer = comparer;
            }
            public CommonComparer(System.Func<T, V> keySelector)
                : this(keySelector, Comparer<V>.Default)
            { }

            public int Compare(T x, T y)
            {
                return comparer.Compare(keySelector(x), keySelector(y));
            }
        }
    }

    #endregion

    #region Queue<T>

    /// <summary>
    /// Queue
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Queue<T>
    {
        /// <summary>
        /// BatchOptions
        /// </summary>
        public struct BatchOptions
        {
            /// <summary>
            /// BatchOptions
            /// </summary>
            /// <param name="interval">Return log time interval, default System.TimeSpan.Zero equals not enabled,5 seconds is reasonable</param>
            /// <param name="maxNumber">Return log number, less than 1 no restrictions</param>
            public BatchOptions(System.TimeSpan interval, int maxNumber)
            {
                Interval = interval;
                MaxNumber = maxNumber;
            }

            /// <summary>
            /// Return log time interval, default System.TimeSpan.Zero equals not enabled,5 seconds is reasonable
            /// </summary>
            public System.TimeSpan Interval { get; set; }

            /// <summary>
            /// Return log number, less than 1 no restrictions
            /// </summary>
            public int MaxNumber { get; set; }
        }

        /// <summary>
        /// queue
        /// </summary>
        public readonly System.Collections.Concurrent.BlockingCollection<T> queue;

        readonly System.Collections.Concurrent.BlockingCollection<T>[] dequeues;

        /// <summary>
        /// Queue
        /// </summary>
        /// <param name="call"></param>
        /// <param name="batch"></param>
        /// <param name="maxWorkThreads">Gets the maximum out queue thread for this queue, default 1</param>
        /// <param name="syn">Whether each outgoing thread has synchronous callback, asynchronous by default</param>
        /// <param name="maxCapacity">Gets the max capacity of this queue</param>
        public Queue(System.Func<IEnumerable<T>, System.Threading.Tasks.ValueTask> call, BatchOptions batch = default, int maxWorkThreads = 1, bool syn = false, int? maxCapacity = null)
        {
            if (null == call) { throw new System.ArgumentNullException(nameof(call)); }

            var isBatch = !default(BatchOptions).Equals(batch);
            var isInterval = 0 != System.TimeSpan.Zero.CompareTo(batch.Interval);

            if (0 >= maxWorkThreads || !isBatch)
            {
                maxWorkThreads = 1;
            }

            queue = maxCapacity.HasValue && 0 < maxCapacity.Value ? new System.Collections.Concurrent.BlockingCollection<T>(maxCapacity.Value) : new System.Collections.Concurrent.BlockingCollection<T>();

            dequeues = new System.Collections.Concurrent.BlockingCollection<T>[maxWorkThreads];
            for (int i = 0; i < maxWorkThreads; i++)
            {
                dequeues[i] = 0 < batch.MaxNumber ? new System.Collections.Concurrent.BlockingCollection<T>(batch.MaxNumber) : isBatch ? new System.Collections.Concurrent.BlockingCollection<T>() : new System.Collections.Concurrent.BlockingCollection<T>(1);
            }

            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                foreach (var item in queue.GetConsumingEnumerable())
                {
                    if (1 == dequeues.Length)
                    {
                        dequeues[0].Add(item);
                    }
                    else
                    {
                        System.Collections.Concurrent.BlockingCollection<T>.AddToAny(dequeues, item);
                    }
                }
            }, System.Threading.Tasks.TaskCreationOptions.LongRunning);

            System.Threading.Tasks.Parallel.For(0, maxWorkThreads, new System.Threading.Tasks.ParallelOptions { MaxDegreeOfParallelism = maxWorkThreads }, async c =>
            {
                var queue = dequeues[c];
                var capacity = queue.BoundedCapacity;

                var watch = new System.Diagnostics.Stopwatch();
                var list = new LinkedList<T>();

                if (isInterval)
                {
                    watch.Start();
                }

                while (!this.queue.IsCompleted)
                {
                    var count = queue.Count;

                    if (0 < count && ((0 < capacity && count == capacity) || !isBatch || !isInterval || (isInterval && 0 < watch.Elapsed.CompareTo(batch.Interval))))
                    {
                        for (int i2 = 0; i2 < count; i2++)
                        {
                            if (queue.TryTake(out T logger))
                            {
                                list.AddLast(logger);
                            }
                        }

                        var data = new T[list.Count];
                        list.CopyTo(data, 0);
                        list.Clear();

                        if (syn)
                        {
                            await call(data).AsTask().ContinueWith(c2 => c2.Exception?.Console());
                        }
                        else
                        {
                            System.Threading.Tasks.Task.Factory.StartNew(obj => call(obj as T[]).AsTask().ContinueWith(c2 => c2.Exception?.Console()), data);
                        }

                        if (watch.IsRunning)
                        {
                            watch.Restart();
                        }
                    }

                    await System.Threading.Tasks.Task.Delay(1);
                }

                if (watch.IsRunning)
                {
                    watch.Stop();
                }

                list.Clear();// count > 0 ?
            });
        }
    }

    #endregion
}