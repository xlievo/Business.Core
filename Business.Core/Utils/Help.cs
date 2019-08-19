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

namespace Business.Utils
{
    using Business.Document;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public struct Accessor
    {
        public Accessor(System.Type type, System.Func<object, object> getter, System.Action<object, object> setter)
        {
            this.Type = type;
            this.Getter = getter;
            this.Setter = setter;
        }

        public System.Type Type { get; private set; }
        public System.Func<object, object> Getter { get; private set; }
        public System.Action<object, object> Setter { get; private set; }

        public object TryGetter(object obj)
        {
            try { return Getter(obj); } catch { return Type.IsValueType ? System.Activator.CreateInstance(Type) : null; }
        }
    }

    public struct Accessors
    {
        public ConcurrentReadOnlyDictionary<string, Accessor> Accessor;

        public object[] ConstructorArgs;
    }

    public static class Help
    {
        public static void LoadAccessors(this System.Type type, ConcurrentReadOnlyDictionary<string, Accessors> accessors, string key = null)
        {
            var key2 = string.IsNullOrWhiteSpace(key) ? type.FullName : key;

            if (type.IsAbstract || accessors.ContainsKey(key2)) { return; }

            var member = accessors.dictionary.GetOrAdd(key2, _ => new Accessors { Accessor = new ConcurrentReadOnlyDictionary<string, Accessor>(), ConstructorArgs = type.GetConstructors()?.FirstOrDefault()?.GetParameters().Select(c => c.HasDefaultValue ? c.DefaultValue : default).ToArray() });

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

        public static Business UseType<Business>(this Business business, params System.Type[] argType) where Business : IBusiness
        {
            if (null == business) { throw new System.ArgumentNullException(nameof(business)); }

            if (null == argType) { return business; }

            foreach (var item in argType)
            {
                if (null == item || business.Configer.UseTypes.Contains(item.FullName)) { continue; }

                business.Configer.UseTypes.collection.Add(item.FullName);
            }

            //foreach (var item2 in item.Configuration.MetaData.Values)
            System.Threading.Tasks.Parallel.ForEach(business.Configer.MetaData.Values, item =>
            {
                foreach (var arg in item.Args)
                {
                    var type2 = arg.HasIArg ? arg.IArgInType : arg.Type;

                    if (!business.Configer.UseTypes.Contains(type2.FullName) || !item.UseTypePosition.dictionary.TryAdd(arg.Position, type2))
                    {
                        continue;
                    }

                    arg.UseType = true;

                    foreach (var item2 in arg.Group)
                    {
                        //remove not parameter attr

                        var first = item2.Value.Attrs.First;

                        while (NodeState.DAT == first.State)
                        {
                            var attr = first.Value;

                            if (attr.Declaring != Attributes.AttributeBase.DeclaringType.Parameter)
                            {
                                item2.Value.Attrs.Remove(attr, out _);
                            }

                            first = first.Next;
                        }
                        //foreach (var attr in item2.Value.Attrs)
                        //{
                        //    if (attr.Source != Attributes.AttributeBase.SourceType.Parameter)
                        //    {
                        //        item2.Value.Attrs.Remove(attr);
                        //    }
                        //}

                        //add default convert
                        if (arg.HasIArg && NodeState.DAT != first.State)
                        {
                            var attr = new Attributes.ArgumentDefaultAttribute(item.ResultType, item.ResultTypeDefinition) { Declaring = Attributes.AttributeBase.DeclaringType.Parameter };
                            item2.Value.Attrs.TryAdd(attr);
                            //arg.ArgAttr.collection.Add(new Attributes.ArgumentDefaultAttribute(business.Configer.ResultType) { Source = Attributes.AttributeBase.SourceType.Parameter });
                        }
                    }
                }

                //item.HasArgSingle = 1 >= item.Args.Count(c => !c.UseType);
            });

            return business;
        }

        public static Business UseType<Business>(this Business business, params string[] argName) where Business : IBusiness
        {
            if (null == business) { throw new System.ArgumentNullException(nameof(business)); }

            if (null == argName) { return business; }

            var argName2 = argName.Where(c => !string.IsNullOrWhiteSpace(c)).ToList();

            if (0 == argName2.Count) { return business; }

            System.Threading.Tasks.Parallel.ForEach(business.Configer.MetaData.Values, item =>
            {
                foreach (var arg in item.Args)
                {
                    var type2 = arg.HasIArg ? arg.IArgInType : arg.Type;

                    if (!argName2.Contains(arg.Name) || !item.UseTypePosition.dictionary.TryAdd(arg.Position, type2))
                    {
                        continue;
                    }

                    arg.UseType = true;

                    arg.Use = new Attributes.UseAttribute(true);

                    foreach (var item2 in arg.Group)
                    {
                        //remove not parameter attr

                        var first = item2.Value.Attrs.First;

                        while (NodeState.DAT == first.State)
                        {
                            var attr = first.Value;

                            if (attr.Declaring != Attributes.AttributeBase.DeclaringType.Parameter)
                            {
                                item2.Value.Attrs.Remove(attr, out _);
                            }

                            first = first.Next;
                        }

                        //add default convert
                        if (arg.HasIArg && NodeState.DAT != first.State)
                        {
                            var attr = new Attributes.ArgumentDefaultAttribute(item.ResultType, item.ResultTypeDefinition) { Declaring = Attributes.AttributeBase.DeclaringType.Parameter };
                            item2.Value.Attrs.TryAdd(attr);
                        }
                    }
                }

                //item.HasArgSingle = 1 >= item.Args.Count(c => !c.UseType);
            });

            return business;
        }

        public static string GetXmlPath(this Assembly assembly) => System.IO.Path.Combine(System.IO.Path.GetDirectoryName(assembly.Location), $"{System.IO.Path.GetFileNameWithoutExtension(assembly.ManifestModule.Name)}.xml");

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
        */

        public static Newtonsoft.Json.JsonSerializerSettings JsonSettings = new Newtonsoft.Json.JsonSerializerSettings
        {
            ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
            ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
            DateFormatString = "yyyy-MM-dd HH:mm:ss",
            DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Local,
            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
        };

        static DocArg GetDocArg(DocArgSource argSource)
        {
            var docArg = new DocArg { Id = argSource.Args.Group[argSource.Group].Path, LastType = argSource.Args.LastType.Name, Array = argSource.Args.HasCollection };
            docArg.Title = $"{argSource.Args.Name} ({docArg.LastType})";
            docArg.Description = argSource.Summary;

            docArg.Token = argSource.Args.HasToken;

            //var hasDescription = string.IsNullOrWhiteSpace(docArg.Description);
            var attrs = System.String.Empty;
            for (int i = 0; i < argSource.Attributes.Count; i++)
            {
                attrs += $"<h5 tag=\"h5\" style=\"margin:0px;margin-bottom:{(argSource.Attributes.Count - 1 > i ? 2 : !docArg.Token && argSource.Args.HasDefinition ? 15 : 2)}px;margin-top:2px;\"><code>{argSource.Attributes[i]}</code></h5>";
            }

            if (!string.IsNullOrWhiteSpace(attrs))
            {
                docArg.Description += System.Environment.NewLine + attrs;
            }

            if (argSource.Args.HasDefaultValue)
            {
                docArg.DefaultValue = System.Convert.ToString(argSource.Args.DefaultValue);
            }

            if (!docArg.Token && argSource.Args.HasDefinition)
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

            if (argSource.Args.LastType.IsEnum)
            {
                docArg.Enum = argSource.Args.LastType.GetEnumNames();
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
            else if (argSource.Args.HasCollection)
            {
                docArg.Type = "array";
                docArg.Title = $"{argSource.Args.Name} ({docArg.LastType} Array)";
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

            return docArg;
        }

        static (string, string) GetDocArgType(System.TypeCode typeCode)
        {
            switch (typeCode)
            {
                case System.TypeCode.Boolean:
                    return ("boolean", string.Empty);
                case System.TypeCode.Byte:
                    return ("string", "binary");
                case System.TypeCode.DBNull:
                case System.TypeCode.Empty:
                case System.TypeCode.Char:
                case System.TypeCode.String:
                    return ("string", string.Empty);
                case System.TypeCode.DateTime:
                    return ("string", "datetime-local");
                case System.TypeCode.Decimal:
                case System.TypeCode.Double:
                    return ("number", string.Empty);
                case System.TypeCode.Int16:
                    return ("integer", string.Empty);
                case System.TypeCode.Int32:
                    return ("integer", "int32");
                case System.TypeCode.Int64:
                    return ("integer", "int64");
                case System.TypeCode.SByte:
                    return ("integer", string.Empty);
                case System.TypeCode.Single:
                    return ("number", "float");
                case System.TypeCode.UInt16:
                    return ("integer", string.Empty);
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
        /// <param name="outFile"></param>
        /// <returns></returns>
        public static Business UseDoc<Business>(this Business business, string outFile = null) where Business : IBusiness => UseDoc(business, c => GetDocArg(c), outFile);

        /// <summary>
        /// Generate document objects for specified business classes.
        /// </summary>
        /// <typeparam name="Business"></typeparam>
        /// <typeparam name="DocArg"></typeparam>
        /// <param name="business"></param>
        /// <param name="argCallback"></param>
        /// <param name="outFile"></param>
        /// <returns></returns>
        public static Business UseDoc<Business, DocArg>(this Business business, System.Func<DocArgSource, DocArg> argCallback, string outFile = null) where Business : IBusiness where DocArg : IDocArg<DocArg>
        {
            if (null == business) { throw new System.ArgumentNullException(nameof(business)); }
            if (null == argCallback) { throw new System.ArgumentNullException(nameof(argCallback)); }

            var path = business.GetType().BaseType.Assembly.GetXmlPath();

            Configer.Xmls.TryGetValue(path, out Xml xml);

            if (null == xml && System.IO.File.Exists(path))
            {
                xml = Configer.Xmls.dictionary.GetOrAdd(path, Xml.DeserializeDoc(FileReadString(path)));
            }

            business.Configer.Doc = UseDoc(business, argCallback, xml?.members?.ToDictionary(c => c.name, c => c));

            if (!string.IsNullOrEmpty(outFile))
            {
                if (outFile.Contains("{BusinessName}"))
                {
                    outFile = outFile.Replace("{BusinessName}", business.Configer.Info.BusinessName);
                }

                if (System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(outFile)))
                {
                    System.IO.File.WriteAllText(outFile, business.Configer.Doc.ToString(), UTF8);
                }
            }

            return business;
        }

        /// <summary>
        /// Gets the document object of the specified business class.
        /// </summary>
        /// <typeparam name="Business"></typeparam>
        /// <typeparam name="DocArg"></typeparam>
        /// <param name="business"></param>
        /// <param name="argCallback"></param>
        /// <param name="xmlMembers"></param>
        /// <returns></returns>
        public static Doc<DocArg> UseDoc<Business, DocArg>(this Business business, System.Func<DocArgSource, DocArg> argCallback, IDictionary<string, Xml.member> xmlMembers) where Business : IBusiness where DocArg : IDocArg<DocArg>
        {
            if (null == argCallback) { throw new System.ArgumentNullException(nameof(argCallback)); }

            var members = business.Command.AsParallel().ToDictionary(c => c.Key, c => c.Value.OrderBy(c2 => c2.Value.Meta.Position).ToDictionary(c2 => c2.Key, c2 =>
            {
                var meta = c2.Value.Meta;

                Xml.member member = null;
                xmlMembers?.TryGetValue($"M:{meta.MethodTypeFullName}", out member);

                var member2 = new Member<DocArg>
                {
                    Name = meta.CommandGroup[c2.Value.Key].OnlyName,
                    HasReturn = meta.HasReturn,
                    Description = member?.summary?.sub,
                    //Returns = meta.ReturnType.GetTypeDefinition(xmlMembers, member?.returns?.sub),
                    Args = new Dictionary<string, DocArg>(),
                    ArgSingle = c2.Value.HasArgSingle,
                } as IMember<DocArg>;

                foreach (var item in meta.Args.Where(c3 => !c3.Group[c2.Value.Key].IgnoreArg))
                {
                    member2.Args.Add(item.Name, GetDocArg(c2.Value.Key, item, argCallback, xmlMembers, member?._params?.Find(c4 => c4.name == item.Name)?.text));
                }

                return member2;
            }));

            return new Doc<DocArg> { Name = business.Configer.Info.BusinessName, Members = members };
        }

        const string AttributeSign = "Attribute";

        static DocArg GetDocArg<DocArg>(string group, Meta.Args args, System.Func<DocArgSource, DocArg> argCallback, IDictionary<string, Xml.member> xmlMembers, string summary = null) where DocArg : IDocArg<DocArg>
        {
            if (null == argCallback) { throw new System.ArgumentNullException(nameof(argCallback)); }

            if (string.IsNullOrWhiteSpace(summary))
            {
                Xml.member member = null;

                switch (args.MemberDefinition)
                {
                    case Meta.MemberDefinitionCode.No:
                        break;
                    case Meta.MemberDefinitionCode.Definition:
                        xmlMembers?.TryGetValue($"T:{args.ArgTypeFullName}", out member);
                        break;
                    case Meta.MemberDefinitionCode.Field:
                        xmlMembers?.TryGetValue($"F:{args.ArgTypeFullName}", out member);
                        break;
                    case Meta.MemberDefinitionCode.Property:
                        xmlMembers?.TryGetValue($"P:{args.ArgTypeFullName}", out member);
                        break;
                }

                summary = member?.summary?.text;

                if (string.IsNullOrWhiteSpace(summary) && args.HasDefinition)
                {
                    xmlMembers?.TryGetValue($"T:{args.LastType.FullName.Replace('+', '.')}", out member);

                    summary = member?.summary?.text;
                }
            }

            var argGroup = args.Group[group];

            var attrs = new List<string>();

            var attr = argGroup.Attrs.First;

            while (NodeState.DAT == attr.State)
            {
                attrs.Add(string.IsNullOrWhiteSpace(attr.Value.Description) ? attr.Value.Type.Name.EndsWith(AttributeSign) ? attr.Value.Type.Name.Substring(0, attr.Value.Type.Name.Length - AttributeSign.Length) : attr.Value.Type.Name : attr.Value.Description);
                attr = attr.Next;
            }

            var arg = argCallback(new DocArgSource { Group = group, Args = args, Attributes = attrs, Summary = summary });

            if (!argGroup.Ignore.Any(c => c.Mode == Attributes.IgnoreMode.ArgChild) && !args.LastType.IsEnum)
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
                            arg.Items.Children.Add(item.Name, GetDocArg(group, item, argCallback, xmlMembers));
                        }
                        else
                        {
                            arg.Children.Add(item.Name, GetDocArg(group, item, argCallback, xmlMembers));
                        }
                    }
                }
            }

            return arg;
        }

        public static TypeDefinition GetTypeDefinition(this System.Type returnType, IDictionary<string, Xml.member> xmlMembers = null, string summary = null)
        {
            var hasDefinition = returnType.IsDefinition();
            var definitions = hasDefinition ? new List<string> { returnType.FullName } : new List<string>();
            var childrens = new ReadOnlyCollection<TypeDefinition>();
            var fullName = returnType.FullName.Replace('+', '.');
            var memberDefinition = hasDefinition ? Meta.MemberDefinitionCode.Definition : Meta.MemberDefinitionCode.No;
            //..//

            Xml.member member = null;
            if (string.IsNullOrWhiteSpace(summary) && memberDefinition == Meta.MemberDefinitionCode.Definition)
            {
                xmlMembers?.TryGetValue($"T:{fullName}", out member);

                summary = member?.summary?.text;
            }

            var definition = new TypeDefinition
            {
                Name = returnType.Name,
                Type = returnType,
                DefaultValue = returnType.IsValueType && typeof(void) != returnType ? System.Activator.CreateInstance(returnType) : null,

                IsNumeric = returnType.IsNumeric(),
                IsDictionary = typeof(System.Collections.IDictionary).IsAssignableFrom(returnType),
                IsCollection = returnType.IsCollection(),
                IsEnum = returnType.IsEnum,
                EnumNames = returnType.IsEnum ? returnType.GetEnumNames() : null,
                EnumValues = returnType.IsEnum ? returnType.GetEnumValues() : null,

                FullName = fullName,
                Children = hasDefinition ? GetTypeDefinition(returnType, definitions, childrens, string.Empty, xmlMembers) : new ReadOnlyCollection<TypeDefinition>(),
                Childrens = childrens,
                MemberDefinition = memberDefinition,
                Summary = summary,
                Path = returnType.Name,
            };

            return definition;
        }

        static ReadOnlyCollection<TypeDefinition> GetTypeDefinition(System.Type type, List<string> definitions, ReadOnlyCollection<TypeDefinition> childrens, string path, IDictionary<string, Xml.member> xmlMembers = null)
        {
            var types = new ReadOnlyCollection<TypeDefinition>();

            var members = type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.GetField | BindingFlags.GetProperty);

            foreach (var item in members)
            {
                System.Type memberType = null;
                var memberDefinition = Meta.MemberDefinitionCode.No;

                switch (item.MemberType)
                {
                    case MemberTypes.Field:
                        {
                            var member = item as FieldInfo;
                            memberType = member.FieldType;
                            memberDefinition = Meta.MemberDefinitionCode.Field;
                        }
                        break;
                    case MemberTypes.Property:
                        {
                            var member = item as PropertyInfo;
                            memberType = member.PropertyType;
                            memberDefinition = Meta.MemberDefinitionCode.Property;
                        }
                        break;
                    default: continue;
                }

                var hasDefinition = memberType.IsDefinition();
                if (definitions.Contains(memberType.FullName)) { continue; }
                else if (hasDefinition) { definitions.Add(memberType.FullName); }
                var childrens2 = new ReadOnlyCollection<TypeDefinition>();
                var fullName = $"{type.FullName.Replace('+', '.')}.{item.Name}";

                Xml.member member2 = null;

                switch (memberDefinition)
                {
                    case Meta.MemberDefinitionCode.No:
                        break;
                    case Meta.MemberDefinitionCode.Definition:
                        xmlMembers?.TryGetValue($"T:{fullName}", out member2);
                        break;
                    case Meta.MemberDefinitionCode.Field:
                        xmlMembers?.TryGetValue($"F:{fullName}", out member2);
                        break;
                    case Meta.MemberDefinitionCode.Property:
                        xmlMembers?.TryGetValue($"P:{fullName}", out member2);
                        break;
                }

                var summary = member2?.summary?.text;

                // .. //
                var path2 = !string.IsNullOrWhiteSpace(path) ? $"{path}.{item.Name}" : item.Name;
                var definition = new TypeDefinition
                {
                    Name = item.Name,
                    Type = memberType,
                    DefaultValue = memberType.IsValueType ? System.Activator.CreateInstance(memberType) : null,

                    IsNumeric = memberType.IsNumeric(),
                    IsDictionary = typeof(System.Collections.IDictionary).IsAssignableFrom(memberType),
                    IsCollection = memberType.IsCollection(),
                    IsEnum = memberType.IsEnum,
                    EnumNames = memberType.IsEnum ? memberType.GetEnumNames() : null,
                    EnumValues = memberType.IsEnum ? memberType.GetEnumValues() : null,

                    FullName = fullName,
                    Children = hasDefinition ? GetTypeDefinition(memberType, definitions, childrens2, path2, xmlMembers) : new ReadOnlyCollection<TypeDefinition>(),
                    Childrens = childrens2,
                    MemberDefinition = memberDefinition,
                    Summary = summary,
                    Path = path2,
                };

                types.collection.Add(definition);
                childrens.collection.Add(definition);

                foreach (var child in childrens2)
                {
                    childrens.collection.Add(child);
                }
            }

            return types;
        }

        public struct TypeDefinition
        {
            public string Name { get; set; }

            public System.Type Type { get; set; }

            public bool IsEnum { get; set; }

            public bool IsCollection { get; set; }

            public bool IsDictionary { get; set; }

            public bool IsNumeric { get; set; }

            public string[] EnumNames { get; set; }

            public System.Array EnumValues { get; set; }

            public object DefaultValue { get; set; }

            public string FullName { get; set; }

            public string Path { get; set; }

            public string Summary { get; set; }

            public Meta.MemberDefinitionCode MemberDefinition { get; set; }

            public ReadOnlyCollection<TypeDefinition> Children { get; set; }

            public ReadOnlyCollection<TypeDefinition> Childrens { get; set; }
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

        public static Business LoggerSet<Business>(this Business business, Attributes.LoggerAttribute logger, params System.Type[] argType) where Business : IBusiness
        {
            if (null == business) { throw new System.ArgumentNullException(nameof(business)); }

            if (null == logger || null == argType || 0 == argType.Length) { return business; }

            System.Threading.Tasks.Parallel.ForEach(business.Configer.MetaData.Values, item =>
            {
                var groups = item.CommandGroup.Values.Where(c => Bind.GroupEquals(logger, c.Group));

                if (!groups.Any()) { return; }

                logger.Declaring = Attributes.AttributeBase.DeclaringType.Parameter;

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

        public static Business LoggerSet<Business>(this Business business, Attributes.LoggerAttribute logger, params string[] argName) where Business : IBusiness
        {
            if (null == business) { throw new System.ArgumentNullException(nameof(business)); }

            if (null == logger || null == argName) { return business; }

            var argName2 = argName.Where(c => !string.IsNullOrWhiteSpace(c)).ToList();

            if (0 == argName2.Count) { return business; }

            System.Threading.Tasks.Parallel.ForEach(business.Configer.MetaData.Values, item =>
            {
                var groups = item.CommandGroup.Values.Where(c => Bind.GroupEquals(logger, c.Group));

                if (!groups.Any()) { return; }

                logger.Declaring = Attributes.AttributeBase.DeclaringType.Parameter;

                System.Threading.Tasks.Parallel.ForEach(argName2, name =>
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

        public static Business IgnoreSet<Business>(this Business business, Attributes.Ignore ignore, params System.Type[] argType) where Business : IBusiness
        {
            if (null == business) { throw new System.ArgumentNullException(nameof(business)); }

            if (null == ignore || null == argType || 0 == argType.Length) { return business; }

            System.Threading.Tasks.Parallel.ForEach(business.Configer.MetaData.Values, item =>
            {
                var groups = item.CommandGroup.Values.Where(c => Bind.GroupEquals(ignore, c.Group));

                if (!groups.Any()) { return; }

                ignore.Declaring = Attributes.AttributeBase.DeclaringType.Parameter;

                System.Threading.Tasks.Parallel.ForEach(argType, type =>
                {
                    foreach (var group in groups)
                    {
                        foreach (var arg in item.Args)
                        {
                            if (Equals(arg.Type, type) || Equals(arg.IArgOutType, type))
                            {
                                var g = arg.Group[group.Key];
                                if (g.Ignore.Any(c => ignore.GroupKey() == c.GroupKey()))
                                {
                                    continue;
                                }
                                g.Ignore.collection.Add(ignore);
                                g.IgnoreArg = g.Ignore.Any(c => c.Mode == Attributes.IgnoreMode.Arg);
                                business.Command[group.Group][group.OnlyName].HasArgSingle = 1 >= item.Args.Count(c => !c.HasToken && !c.Group[group.Key].IgnoreArg);
                            }
                        }
                    }
                });
            });

            return business;
        }

        public static Business IgnoreSet<Business>(this Business business, Attributes.Ignore ignore, params string[] argName) where Business : IBusiness
        {
            if (null == business) { throw new System.ArgumentNullException(nameof(business)); }

            if (null == ignore || null == argName) { return business; }

            var argName2 = argName.Where(c => !string.IsNullOrWhiteSpace(c)).ToList();

            if (0 == argName2.Count) { return business; }

            System.Threading.Tasks.Parallel.ForEach(business.Configer.MetaData.Values, item =>
            {
                var groups = item.CommandGroup.Values.Where(c => Bind.GroupEquals(ignore, c.Group));
                //checked group
                if (!groups.Any()) { return; }

                ignore.Declaring = Attributes.AttributeBase.DeclaringType.Parameter;

                System.Threading.Tasks.Parallel.ForEach(argName2, name =>
                {
                    foreach (var group in groups)
                    {
                        foreach (var arg in item.Args)
                        {
                            if (Equals(arg.Name, name))
                            {
                                var g = arg.Group[group.Key];
                                if (g.Ignore.Any(c => ignore.GroupKey() == c.GroupKey()))
                                {
                                    continue;
                                }
                                g.Ignore.collection.Add(ignore);
                                g.IgnoreArg = g.Ignore.Any(c => c.Mode == Attributes.IgnoreMode.Arg);
                                business.Command[group.Group][group.OnlyName].HasArgSingle = 1 >= item.Args.Count(c => !c.HasToken && !c.Group[group.Key].IgnoreArg);
                            }
                        }
                    }
                });
            });

            return business;
        }

        public static Business MemberSet<Business>(this Business business, string memberName, object memberObj, bool skipNull = false) where Business : IBusiness
        {
            if (null == business) { throw new System.ArgumentNullException(nameof(business)); }

            if (!Configer.Accessors.TryGetValue(business.Configer.Info.BusinessName, out Accessors meta) || !meta.Accessor.TryGetValue(memberName, out Accessor accessor)) { return business; }

            if (skipNull && !object.Equals(null, accessor.Getter(business)))
            {
                return business;
            }

            var value = Help.ChangeType(memberObj, accessor.Type);
            accessor.Setter(business, value);

            business.Configer.MemberSetAfter?.Invoke(memberName, value);

            return business;
        }

        static Meta.MetaLogger GetMetaLogger(Meta.MetaLogger metaLogger, Attributes.LoggerAttribute logger, string group)
        {
            var logger2 = logger.Clone();
            logger2.Group = group;

            switch (logger2.LogType)
            {
                case LoggerType.All:
                    metaLogger.Record = logger2.Clone().SetType(LoggerType.Record);
                    metaLogger.Error = logger2.Clone().SetType(LoggerType.Error);
                    metaLogger.Exception = logger2.Clone().SetType(LoggerType.Exception);
                    break;
                case LoggerType.Record:
                    metaLogger.Record = logger2;
                    break;
                case LoggerType.Error:
                    metaLogger.Error = logger2;
                    break;
                case LoggerType.Exception:
                    metaLogger.Exception = logger2;
                    break;
            }

            return metaLogger;
        }

        public static string BaseDirectory
        {
            get
            {
#if NETFX
                return System.AppDomain.CurrentDomain.BaseDirectory;
#else
                return System.AppContext.BaseDirectory;
#endif
            }
        }

        public static string BaseDirectoryCombine(params string[] path) => System.IO.Path.Combine(new string[] { BaseDirectory }.Concat(path).ToArray());

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

        public static Assembly LoadAssembly(string assemblyFile)
        {
            try
            {
                var ass2 = Assembly.UnsafeLoadFrom(assemblyFile);

                return (null != ass2 && !ass2.IsDynamic) ? ass2 : null;
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

        public static Accessor GetAccessor(this FieldInfo fieldInfo)
        {
            if (null == fieldInfo) { throw new System.ArgumentNullException(nameof(fieldInfo)); }

            var getter = Emit.FieldAccessorGenerator.CreateGetter(fieldInfo);
            var setter = Emit.FieldAccessorGenerator.CreateSetter(fieldInfo);

            return new Accessor(fieldInfo.FieldType, getter, setter);
        }

        public static Accessor GetAccessor(this PropertyInfo propertyInfo)
        {
            if (null == propertyInfo) { throw new System.ArgumentNullException(nameof(propertyInfo)); }

            var getter = Emit.PropertyAccessorGenerator.CreateGetter(propertyInfo);
            var setter = Emit.PropertyAccessorGenerator.CreateSetter(propertyInfo);

            return new Accessor(propertyInfo.PropertyType, getter, setter);
        }

        #region GetAttributes

        public static T Clone<T>(this T attribute) where T : Attributes.AttributeBase => attribute.Clone<T>();

        public static List<Attribute> Distinct<Attribute>(this List<Attribute> attributes, IEnumerable<Attribute> clones = null) where Attribute : Attributes.AttributeBase
        {
            var gropus = new List<Attributes.GropuAttribute>();

            for (int i = attributes.Count - 1; i >= 0; i--)
            {
                var item = attributes[i];

                if (item is Attributes.GropuAttribute)
                {
                    gropus.Add(item as Attributes.GropuAttribute);

                    attributes.RemoveAt(i);
                }
            }

            var attrs = gropus.Distinct(Attributes.GropuAttribute.Comparer).ToDictionary(c => c.GroupKey(), c => c);

            if (null != clones && clones.Any())
            {
                foreach (var item in clones)
                {
                    if (item is Attributes.GropuAttribute)
                    {
                        var item2 = item as Attributes.GropuAttribute;

                        var groupKey = item2.GroupKey();

                        if (!attrs.ContainsKey(groupKey))
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

        public static List<Attribute> GetAttrs<Attribute>(this IList<Attributes.AttributeBase> attributes, System.Func<Attribute, bool> predicate = null, bool clone = false) where Attribute : Attributes.AttributeBase
        {
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

        public static Attribute GetAttr<Attribute>(this IList<Attributes.AttributeBase> attributes, System.Func<Attribute, bool> predicate = null) where Attribute : Attributes.AttributeBase
        {
            if (null == predicate)
            {
                return attributes.FirstOrDefault(c => c is Attribute) as Attribute;
            }
            else
            {
                return attributes.FirstOrDefault(c => c is Attribute && predicate((Attribute)c)) as Attribute;
            }
        }

        public static T[] GetAttributes<T>(this MemberInfo member, bool inherit = true) where T : System.Attribute
        {
            if (null == member) { throw new System.ArgumentNullException(nameof(member)); }
            return member.GetCustomAttributes<T>(inherit).ToArray();
            //return (T[])System.Attribute.GetCustomAttributes(member, typeof(T), inherit);
        }
        public static T GetAttribute<T>(this MemberInfo member, bool inherit = true) where T : System.Attribute
        {
            if (null == member) { throw new System.ArgumentNullException(nameof(member)); }

            if (member.MemberType == MemberTypes.TypeInfo && typeof(object) == (System.Type)member) { return default; }

            return member.GetCustomAttribute<T>(inherit);
        }

        ////public static System.Collections.Generic.List<Attributes.AttributeBase> GetAttributes(this MemberInfo member, bool inherit = true) => member.GetAttributes<Attributes.AttributeBase>(inherit).ToList();

        public static T[] GetAttributes<T>(this ParameterInfo member, bool inherit = true) where T : System.Attribute
        {
            if (null == member) { throw new System.ArgumentNullException(nameof(member)); }
            return member.GetCustomAttributes<T>(inherit).ToArray();
            //return (T[])System.Attribute.GetCustomAttributes(member, typeof(T), inherit);
        }
        public static T GetAttribute<T>(this ParameterInfo member, bool inherit = true) where T : System.Attribute
        {
            if (null == member) { throw new System.ArgumentNullException(nameof(member)); }
            return member.GetCustomAttribute<T>(inherit);
        }

        public static bool Exists<T>(this T[] attrs) where T : System.Attribute
        {
            if (null == attrs) { throw new System.ArgumentNullException(nameof(attrs)); }

            return 0 < attrs.Length;
        }

        public static bool Exists<T>(this MemberInfo member, bool inherit = true) where T : System.Attribute
        {
            if (null == member) { throw new System.ArgumentNullException(nameof(member)); }

            return member.IsDefined(typeof(T));
        }
        public static bool Exists<T>(this ParameterInfo member, bool inherit = true) where T : System.Attribute
        {
            if (null == member) { throw new System.ArgumentNullException(nameof(member)); }

            return member.IsDefined(typeof(T));
        }

        #endregion

        public static bool IsAssignableFrom(this System.Type type, System.Type fromType, out System.Type[] genericArguments)
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
        public static byte[] StreamReadByte(this System.IO.Stream stream)
        {
            if (null == stream) { throw new System.ArgumentNullException(nameof(stream)); }

            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            return bytes;
        }
        public static async System.Threading.Tasks.ValueTask<byte[]> StreamReadByteAsync(this System.IO.Stream stream)
        {
            if (null == stream) { throw new System.ArgumentNullException(nameof(stream)); }

            var bytes = new byte[stream.Length];
            await stream.ReadAsync(bytes, 0, bytes.Length);
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            return bytes;
        }

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

        public static string StreamReadString(this System.IO.Stream stream, System.Text.Encoding encoding = null)
        {
            using (var reader = new System.IO.StreamReader(stream, encoding ?? UTF8))
            {
                return reader.ReadToEnd();
            }
        }
        public static async System.Threading.Tasks.ValueTask<string> StreamReadStringAsync(this System.IO.Stream stream, System.Text.Encoding encoding = null)
        {
            using (var reader = new System.IO.StreamReader(stream, encoding ?? UTF8))
            {
                return await reader.ReadToEndAsync();
            }
        }

        public static string FileReadString(string path, System.Text.Encoding encoding = null)
        {
            if (string.IsNullOrWhiteSpace(path)) { throw new System.ArgumentException(nameof(path)); }

            using (var fileStream = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
            {
                return fileStream.StreamReadString(encoding ?? UTF8);
            }
        }
        public static async System.Threading.Tasks.ValueTask<string> FileReadStringAsync(string path, System.Text.Encoding encoding = null)
        {
            if (string.IsNullOrWhiteSpace(path)) { throw new System.ArgumentException(nameof(path)); }

            using (var fileStream = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
            {
                return await fileStream.StreamReadStringAsync(encoding ?? UTF8);
            }
        }
        public static byte[] FileReadByte(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) { throw new System.ArgumentException(nameof(path)); }

            using (var fileStream = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
            {
                return fileStream.StreamCopyByte();
            }
        }
        public static async System.Threading.Tasks.ValueTask<byte[]> FileReadByteAsync(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) { throw new System.ArgumentException(nameof(path)); }

            using (var fileStream = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
            {
                return await fileStream.StreamCopyByteAsync();
            }
        }

        public static void StreamWrite(this System.IO.Stream stream, string value, System.Text.Encoding encoding = null)
        {
            using (var writer = new System.IO.StreamWriter(stream, encoding ?? UTF8))
            {
                writer.AutoFlush = true;
                writer.Write(value);
            }
        }
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

        public static string MD5(this string value, bool hasUpper = false, System.Text.Encoding encoding = null)
        {
            if (null == value) { throw new System.ArgumentNullException(nameof(value)); }

            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var result = System.BitConverter.ToString(md5.ComputeHash((encoding ?? UTF8).GetBytes(value))).Replace("-", string.Empty);
                return hasUpper ? result.ToUpperInvariant() : result.ToLowerInvariant();
            }
        }

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

        public static void DeleteFolder(string path)
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

        public static bool ContainsAndNotNull(this IDictionary<string, dynamic> dict, string source)
        {
            return null != dict && dict.ContainsKey(source) && !object.Equals(null, dict[source]);
        }

        #region WriteLocal

        /// <summary>
        /// Write exception to file
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="write"></param>
        /// <param name="console"></param>
        /// <param name="path"></param>
        /// <param name="dateFormat"></param>
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

        public static void Console(string text, bool autoTime = true, bool console = true, bool write = false, string path = "business.log.txt", string dateFormat = "yyyy-MM-dd HH:mm:ss:fff", System.Text.Encoding encoding = null) => WriteLocal(text, path, autoTime, write, console, dateFormat, encoding);

        /// <summary>
        /// Ignore erroneous characters: Unable to translate Unicode...
        /// </summary>
        public static readonly System.Text.Encoding UTF8 = System.Text.Encoding.GetEncoding("UTF-8", new System.Text.EncoderReplacementFallback(string.Empty), new System.Text.DecoderExceptionFallback());

        #endregion

        /// <summary>
        /// Ignore erroneous characters: Unable to translate Unicode...
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string UTF8String(this string value) => UTF8.GetString(UTF8.GetBytes(value ?? string.Empty));

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

        static System.Predicate<int> number = delegate (int c) { return !(c >= 48 && c <= 57); };
        static System.Predicate<int> upper = delegate (int c) { return !(c >= 65 && c <= 90); };
        static System.Predicate<int> lower = delegate (int c) { return !(c >= 97 && c <= 122); };
        static System.Predicate<int> chinese = delegate (int c) { return !(c >= 0x4e00 && c <= 0x9fbb); };

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

        /// <summary>
        /// ToString("N")
        /// </summary>
        public static string Guid => System.Guid.NewGuid().ToString("N");

        #endregion

        public static string HumanReadableFilesize(double size)
        {
            var units = new string[] { "B", "KB", "MB", "GB", "TB", "PB" };
            double mod = 1024.0;
            int i = 0;
            while (size >= mod)
            {
                size /= mod;
                i++;
            }
            return System.Math.Round(size, 2) + units[i];
        }

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
        public static string GetMethodFullName(this MethodInfo methodInfo)
        {
            if (null == methodInfo) { throw new System.ArgumentNullException(nameof(methodInfo)); }

            return $"{ methodInfo.DeclaringType.FullName}.{ methodInfo.Name}";
        }

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

        public static bool CompareEquals<T>(this T objectFromCompare, T objectToCompare)
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

        public static Type ChangeType<Type>(this object value)
        {
            return (Type)ChangeType(value, typeof(Type));
        }

        public static object ChangeType(this object value, System.Type type)
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

        public static int Random(int minValue, int maxValue)
        {
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                return new System.Random(System.BitConverter.ToInt32(bytes, 0)).Next(minValue, maxValue);
            }
        }
        public static int Random(this int maxValue)
        {
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                return new System.Random(System.BitConverter.ToInt32(bytes, 0)).Next(maxValue);
            }
        }
        public static double Random()
        {
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                return new System.Random(System.BitConverter.ToInt32(bytes, 0)).NextDouble();
            }
        }

        public static bool CheckEmail(this string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            return System.Text.RegularExpressions.Regex.IsMatch(email.Trim(), @"^([0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9})$");
        }

        public static double Scale(this double value, int size = 2)
        {
            var p = System.Math.Pow(10, size);
            return (int)(value * (int)p) / p;
        }
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

        public static string GetName(this System.Enum value) => null == value ? null : System.Enum.GetName(value.GetType(), value);
        public static int? GetValue(this System.Enum value) => null == value ? new int?() : value.GetHashCode();

        public static System.Collections.IList Adde(this System.Collections.IList list, params object[] item)
        {
            if (null == list) { throw new System.ArgumentNullException(nameof(list)); }

            if (null == item || 0 == item.Length) { return list; }

            var list2 = System.Collections.ArrayList.Adapter(list);

            list2.AddRange(item);

            return list2;
        }

        public static string AddeJson(this string json, params object[] item)
        {
            var list = string.IsNullOrEmpty(json) ? new System.Collections.ArrayList() : json.TryJsonDeserialize<System.Collections.IList>() ?? new System.Collections.ArrayList { json };

            return list.Adde(item).JsonSerialize();
        }

        //public static bool SpinWait(this int millisecondsTimeout) => System.Threading.SpinWait.SpinUntil(() => false, millisecondsTimeout);

        //public static bool SpinWait(this System.TimeSpan timeout) => System.Threading.SpinWait.SpinUntil(() => false, timeout);

        static readonly List<string> SysTypes = Assembly.GetExecutingAssembly().GetType().Module.Assembly.GetExportedTypes().Select(c => c.FullName).ToList();
        public static bool IsDefinition(this System.Type type) => !SysTypes.Contains(type.FullName) && (type.IsClass || (type.IsValueType && !type.IsEnum && !type.IsArray && !type.IsCollection() && !type.IsEnumerable()));

        #region Json

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

        public static ReadOnlyCollection<T> ToReadOnly<T>(this IList<T> list) => new ReadOnlyCollection<T>(list);

        public static ReadOnlyCollection<T> ToReadOnly<T>(this IEnumerable<T> values) => new ReadOnlyCollection<T>(values);

        public static ConcurrentReadOnlyDictionary<TKey, TElement> ToReadOnlyDictionary<TKey, TSource, TElement>(this IEnumerable<TSource> source, System.Func<TSource, TKey> keySelector, System.Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer = null)
        {
            var dictionary = new System.Collections.Concurrent.ConcurrentDictionary<TKey, TElement>(comparer);

            foreach (var item in source)
            {
                dictionary.TryAdd(keySelector(item), elementSelector(item));
            }

            return new ConcurrentReadOnlyDictionary<TKey, TElement>(dictionary);
        }

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

    public class ConcurrentReadOnlyDictionary<TKey, TValue> : System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>
    {
        internal readonly System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue> dictionary;

        public ConcurrentReadOnlyDictionary(System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue> dictionary) : base(dictionary) => this.dictionary = dictionary;

        public ConcurrentReadOnlyDictionary() : this(new System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>()) { }

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

    public class ReadOnlyCollection<TValue> : System.Collections.ObjectModel.ReadOnlyCollection<TValue>
    {
        internal IList<TValue> collection { get => this.Items; }

        public ReadOnlyCollection(IList<TValue> collection) : base(collection) { }

        public ReadOnlyCollection() : this(new List<TValue>()) { }

        public ReadOnlyCollection(int capacity) : this(new List<TValue>(capacity)) { }

        public ReadOnlyCollection(IEnumerable<TValue> values) : this()
        {
            if (null == values) { throw new System.ArgumentNullException(nameof(values)); }

            foreach (var item in values)
            {
                this.Items.Add(item);
            }
        }
    }

    #region Equals

    public static class Equality<T>
    {
        public static IEqualityComparer<T> CreateComparer<V>(System.Func<T, V> keySelector)
        {
            return new CommonEqualityComparer<V>(keySelector);
        }
        public static IEqualityComparer<T> CreateComparer<V>(System.Func<T, V> keySelector, IEqualityComparer<V> comparer)
        {
            return new CommonEqualityComparer<V>(keySelector, comparer);
        }

        class CommonEqualityComparer<V> : IEqualityComparer<T>
        {
            private System.Func<T, V> keySelector;
            private IEqualityComparer<V> comparer;

            public CommonEqualityComparer(System.Func<T, V> keySelector, IEqualityComparer<V> comparer)
            {
                this.keySelector = keySelector;
                this.comparer = comparer;
            }
            public CommonEqualityComparer(System.Func<T, V> keySelector)
                : this(keySelector, System.Collections.Generic.EqualityComparer<V>.Default) { }

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

    public static class ComparisonHelper<T>
    {
        public static IComparer<T> CreateComparer<V>(System.Func<T, V> keySelector)
        {
            return new CommonComparer<V>(keySelector);
        }
        public static IComparer<T> CreateComparer<V>(System.Func<T, V> keySelector, IComparer<V> comparer)
        {
            return new CommonComparer<V>(keySelector, comparer);
        }

        class CommonComparer<V> : IComparer<T>
        {
            private System.Func<T, V> keySelector;
            private IComparer<V> comparer;

            public CommonComparer(System.Func<T, V> keySelector, IComparer<V> comparer)
            {
                this.keySelector = keySelector;
                this.comparer = comparer;
            }
            public CommonComparer(System.Func<T, V> keySelector)
                : this(keySelector, System.Collections.Generic.Comparer<V>.Default)
            { }

            public int Compare(T x, T y)
            {
                return comparer.Compare(keySelector(x), keySelector(y));
            }
        }
    }

    #endregion

    #region ConcurrentLinkedList https://github.com/danielkylewood/concurrent-linked-list

    public class ConcurrentLinkedList<T> : IConcurrentLinkedList<T>
    {
        public Node<T> First => _first;

        private int _counter;
        private Node<T> _first;
        private readonly Node<T> _dummy;
        private readonly System.Collections.Concurrent.ConcurrentDictionary<int, ThreadState<T>> _threads;

        public ConcurrentLinkedList()
        {
            _counter = 0;
            _dummy = new Node<T>();
            _threads = new System.Collections.Concurrent.ConcurrentDictionary<int, ThreadState<T>>();
            _first = new Node<T>(default(T), NodeState.REM, -1);
        }

        /// <summary>
        /// Attempts to add the specified value to the <see cref="ConcurrentLinkedList{T}"/>.
        /// </summary>
        public bool TryAdd(T value)
        {
            var node = new Node<T>(value, (int)NodeState.INS, System.Threading.Thread.CurrentThread.ManagedThreadId);

            Enlist(node);
            var insertionResult = HelpInsert(node, value);

            var originalValue = node.AtomicCompareAndExchangeState(insertionResult ? NodeState.DAT : NodeState.INV, NodeState.INS);
            if (originalValue != NodeState.INS)
            {
                HelpRemove(node, value, out _);
                node.State = NodeState.INV;
            }

            return insertionResult;
        }

        /// <summary>
        /// Attempts to remove the specified value from the <see cref="ConcurrentLinkedList{T}"/>.
        /// </summary>
        public bool Remove(T value, out T result)
        {
            var node = new Node<T>(value, NodeState.REM, System.Threading.Thread.CurrentThread.ManagedThreadId);

            Enlist(node);
            var removeResult = HelpRemove(node, value, out result);
            node.State = NodeState.INV;
            return removeResult;
        }

        /// <summary>
        /// Determines whether the <see cref="ConcurrentLinkedList{T}"/> contains the specified key.
        /// </summary>
        public bool Contains(T value)
        {
            var current = _first;
            while (current != null)
            {
                if (current.Value.Equals(value))
                {
                    var state = current.State;
                    if (state != NodeState.INV)
                    {
                        return state == NodeState.INS || state == NodeState.DAT;
                    }
                }

                current = current.Next;
            }

            return false;
        }

        private static bool HelpInsert(Node<T> node, T value)
        {
            var previous = node;
            var current = previous.Next;
            while (current != null)
            {
                var state = current.State;
                if (state == NodeState.INV)
                {
                    var successor = current.Next;
                    previous.Next = successor;
                    current = successor;
                }
                else if (current.Value != null && !current.Value.Equals(value))
                {
                    previous = current;
                    current = current.Next;
                }
                else if (state == NodeState.REM)
                {
                    return true;
                }
                else if (state == NodeState.INS || state == NodeState.DAT)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool HelpRemove(Node<T> node, T value, out T result)
        {
            result = default(T);
            var previous = node;
            var current = previous.Next;

            while (current != null)
            {
                var state = current.State;
                if (state == NodeState.INV)
                {
                    var successor = current.Next;
                    previous.Next = successor;
                    current = successor;
                }
                else if (!current.Value.Equals(value))
                {
                    previous = current;
                    current = current.Next;
                }
                else if (state == NodeState.REM)
                {
                    return false;
                }
                else if (state == NodeState.INS)
                {
                    var originalValue = current.AtomicCompareAndExchangeState(NodeState.REM, NodeState.INS);
                    if (originalValue == NodeState.INS)
                    {
                        result = current.Value;
                        current.State = NodeState.INV;
                        return true;
                    }
                }
                else if (state == NodeState.DAT)
                {
                    result = current.Value;
                    current.State = NodeState.INV;
                    return true;
                }
            }

            return false;
        }

        private void Enlist(Node<T> node)
        {
            var phase = System.Threading.Interlocked.Increment(ref _counter);
            var threadState = new ThreadState<T>(phase, true, node);
            var currentThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            _threads.AddOrUpdate(currentThreadId, threadState, (key, value) => threadState);

            foreach (var threadId in _threads.Keys)
            {
                HelpEnlist(threadId, phase);
            }

            HelpFinish();
        }

        private void HelpEnlist(int threadId, int phase)
        {
            while (IsPending(threadId, phase))
            {
                var current = _first;
                var previous = current.Previous;
                if (current.Equals(_first))
                {
                    if (previous == null)
                    {
                        if (IsPending(threadId, phase))
                        {
                            var node = _threads[threadId].Node;
                            var original = System.Threading.Interlocked.CompareExchange(ref current.Previous, node, null);
                            if (original is null)
                            {
                                HelpFinish();
                                return;
                            }
                        }
                    }
                    else
                    {
                        HelpFinish();
                    }
                }
            }
        }

        private void HelpFinish()
        {
            var current = _first;
            var previous = current.Previous;
            if (previous != null && !previous.IsDummy())
            {
                var threadId = previous.ThreadId;
                var threadState = _threads[threadId];
                if (current.Equals(_first) && previous.Equals(threadState.Node))
                {
                    var currentState = _threads[threadId];
                    var updatedState = new ThreadState<T>(threadState.Phase, false, threadState.Node);
                    _threads.TryUpdate(threadId, updatedState, currentState);
                    previous.Next = current;
                    System.Threading.Interlocked.CompareExchange(ref _first, previous, current);
                    current.Previous = _dummy;
                }
            }
        }

        private bool IsPending(int threadId, int phase)
        {
            var threadState = _threads[threadId];
            return threadState.Pending && threadState.Phase <= phase;
        }
    }

    public interface IConcurrentLinkedList<T>
    {
        Node<T> First { get; }

        /// <summary>
        /// Attempts to add the specified value to the <see cref="ConcurrentLinkedList{T}"/>.
        /// </summary>
        bool TryAdd(T value);

        /// <summary>
        /// Attempts to remove the specified value from the <see cref="ConcurrentLinkedList{T}"/>.
        /// </summary>
        bool Remove(T value, out T result);

        /// <summary>
        /// Determines whether the <see cref="ConcurrentLinkedList{T}"/> contains the specified key.
        /// </summary>
        bool Contains(T value);
    }

    public class Node<T>
    {
        public T Value;
        public Node<T> Next;

        private int _state;
        private readonly bool _isDummy;

        internal Node<T> Previous;
        internal int ThreadId;
        internal NodeState State
        {
            get => (NodeState)_state;
            set => _state = (int)value;
        }

        internal Node()
        {
            _isDummy = true;
            Value = default(T);
        }

        internal Node(T value, NodeState state, int threadId)
        {
            Value = value;
            ThreadId = threadId;
            _state = (int)state;
            _isDummy = false;
        }

        internal NodeState AtomicCompareAndExchangeState(NodeState value, NodeState compare)
        {
            return (NodeState)System.Threading.Interlocked.CompareExchange(ref _state, (int)value, (int)compare);
        }

        internal bool IsDummy()
        {
            return _isDummy;
        }
    }

    public enum NodeState
    {
        INS = 0,
        REM = 1,
        DAT = 2,
        INV = 3
    }

    internal class ThreadState<T>
    {
        public int Phase;
        public bool Pending;
        public Node<T> Node;

        public ThreadState(int phase, bool pending, Node<T> node)
        {
            Phase = phase;
            Pending = pending;
            Node = node;
        }
    }

    #endregion
}