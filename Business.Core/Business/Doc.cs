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

namespace Business.Core.Document
{
    using Utils;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// doc .xml
    /// </summary>
    [System.Xml.Serialization.XmlRoot("doc")]
    public class Xml
    {
        /// <summary>
        /// doc .xml
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static Xml DeserializeDoc(string xml)
        {
            if (null == xml) { throw new System.ArgumentNullException(nameof(xml)); }

            try
            {
                using (var reader = new System.IO.StringReader(xml))
                {
                    var xmldes = new System.Xml.Serialization.XmlSerializer(typeof(Xml));

                    var doc = (Xml)xmldes.Deserialize(reader);

                    foreach (var member in doc.members)
                    {
                        if (null != member.summary)
                        {
                            member.summary.text = WhitespaceTrim(member.summary.text);
                            foreach (var item in member.summary.para)
                            {
                                item.text = WhitespaceTrim(item.text);
                            }

                            var subList = new List<string>(member.summary.para.Select(c => WhitespaceTrim(c.text)));
                            if (!string.IsNullOrEmpty(member.summary.text))
                            {
                                subList.Insert(0, member.summary.text);
                            }
                            if (0 < subList.Count)
                            {
                                member.summary.sub = string.Join(System.Environment.NewLine, subList);
                            }
                        }

                        if (null != member.returns)
                        {
                            member.returns.text = WhitespaceTrim(member.returns.text);
                            foreach (var item in member.returns.para)
                            {
                                item.text = WhitespaceTrim(item.text);
                            }

                            var subList = new List<string>(member.returns.para.Select(c => WhitespaceTrim(c.text)));
                            if (!string.IsNullOrEmpty(member.returns.text))
                            {
                                subList.Insert(0, member.returns.text);
                            }
                            if (0 < subList.Count)
                            {
                                member.returns.sub = string.Join(System.Environment.NewLine, subList);
                            }
                        }

                        foreach (var item in member._params)
                        {
                            item.text = WhitespaceTrim(item.text);

                            var subList = new List<string>(item.para.Select(c => WhitespaceTrim(c.text)));
                            if (!string.IsNullOrEmpty(item.text))
                            {
                                subList.Insert(0, item.text);
                            }
                            if (0 < subList.Count)
                            {
                                item.sub = string.Join(System.Environment.NewLine, subList);
                            }
                        }
                    }

                    return doc;
                }
            }
            catch// (System.Exception ex)
            {
                return default;
            }
        }

        /// <summary>
        /// WhitespaceTrim
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string WhitespaceTrim(string value)
        {
            if (null == value)
            {
                return null;
            }

            var lin = value.Trim().Split(new char[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries).Select(c => c.Trim());

            return string.Join(System.Environment.NewLine, lin).Trim();
        }

        /// <summary>
        /// _assembly
        /// </summary>
        [System.Xml.Serialization.XmlElement("assembly")]
        public assembly _assembly;

        /// <summary>
        /// members
        /// </summary>
        public List<member> members;

        /// <summary>
        /// assembly
        /// </summary>
        public class assembly
        {
            /// <summary>
            /// name
            /// </summary>
            public string name;

            /// <summary>
            /// name
            /// </summary>
            /// <returns></returns>
            public override string ToString() => name;
        }

        /// <summary>
        /// member
        /// </summary>
        public class member
        {
            /// <summary>
            /// name
            /// </summary>
            [System.Xml.Serialization.XmlAttribute("name")]
            public string name;

            /// <summary>
            /// summary
            /// </summary>
            [System.Xml.Serialization.XmlElement("summary")]
            public values summary;

            /// <summary>
            /// _params
            /// </summary>
            [System.Xml.Serialization.XmlElement("param")]
            public List<param> _params;

            /// <summary>
            /// returns
            /// </summary>
            [System.Xml.Serialization.XmlElement("returns")]
            public values returns;

            /// <summary>
            /// name
            /// </summary>
            /// <returns></returns>
            public override string ToString() => name;

            /// <summary>
            /// param
            /// </summary>
            public class param
            {
                /// <summary>
                /// name
                /// </summary>
                [System.Xml.Serialization.XmlAttribute("name")]
                public string name;

                /// <summary>
                /// text
                /// </summary>
                [System.Xml.Serialization.XmlText]
                public string text;

                /// <summary>
                /// para
                /// </summary>
                [System.Xml.Serialization.XmlElement("para")]
                public List<para> para;

                /// <summary>
                /// sub
                /// </summary>
                public string sub;

                /// <summary>
                /// $"{name} {text}"
                /// </summary>
                /// <returns></returns>
                public override string ToString() => $"{name} {text}";
            }

            /// <summary>
            /// values
            /// </summary>
            public class values
            {
                /// <summary>
                /// text
                /// </summary>
                [System.Xml.Serialization.XmlText]
                public string text;

                /// <summary>
                /// para
                /// </summary>
                [System.Xml.Serialization.XmlElement("para")]
                public List<para> para;

                /// <summary>
                /// sub
                /// </summary>
                public string sub;

                /// <summary>
                /// text
                /// </summary>
                /// <returns></returns>
                public override string ToString() => text;
            }

            /// <summary>
            /// para
            /// </summary>
            public class para
            {
                /// <summary>
                /// text
                /// </summary>
                [System.Xml.Serialization.XmlText]
                public string text;

                /// <summary>
                /// text
                /// </summary>
                /// <returns></returns>
                public override string ToString() => text;
            }
        }
    }

    /// <summary>
    /// DocGroup
    /// </summary>
    public readonly struct DocGroup
    {
        /// <summary>
        /// DocGroup
        /// </summary>
        /// <param name="docGroup"></param>
        public DocGroup(Annotations.DocGroupAttribute docGroup)
        {
            Group = docGroup.Group;
            this.position = docGroup.Position;
            Badge = docGroup.Badge;
            Active = docGroup.Active;
        }

        /// <summary>
        /// DocGroup
        /// </summary>
        /// <param name="group"></param>
        /// <param name="badge"></param>
        /// <param name="active"></param>
        /// <param name="position"></param>
        public DocGroup(string group, string badge = default, bool active = default, int position = default)
        {
            this.position = position;
            Group = group;
            Badge = badge;
            Active = active;
        }

        internal readonly int position;

        /// <summary>
        /// Group
        /// </summary>
        public string Group { get; }

        /// <summary>
        /// Badge
        /// </summary>
        public string Badge { get; }

        /// <summary>
        /// Active
        /// </summary>
        public bool Active { get; }

        /// <summary>
        /// EqualityComparer
        /// </summary>
        public class EqualityComparer : IEqualityComparer<DocGroup>
        {
            /// <summary>
            /// Equals
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            public bool Equals(DocGroup x, DocGroup y) => string.Equals(x.Group, y.Group, System.StringComparison.InvariantCultureIgnoreCase);

            /// <summary>
            /// GetHashCode
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public int GetHashCode(DocGroup obj) => obj.Group.GetHashCode();
        }

        /// <summary>
        /// comparer
        /// </summary>
        public static readonly EqualityComparer comparer = new EqualityComparer();
    }

    /// <summary>
    /// DocInfo
    /// </summary>
    public readonly struct DocInfo
    {
        /// <summary>
        /// DocInfo
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="position"></param>
        /// <param name="key"></param>
        public DocInfo(Annotations.DocAttribute alias, int position, string key)
        {
            Key = key;
            Group = alias.Group;
            Name = alias.Alias;
            this.position = 0 <= alias.Position ? alias.Position : position;
            Badge = alias.Badge;
        }

        internal readonly int position;

        /// <summary>
        /// Key
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Group
        /// </summary>
        public string Group { get; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Badge
        /// </summary>
        public string Badge { get; }
    }

    /// <summary>
    /// Options
    /// </summary>
    public class Options
    {
        /// <summary>
        /// Host
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Whether to render the Debug element in the UI
        /// </summary>
        public bool Debug { get; set; }

        /// <summary>
        /// Whether to render the Benchmark element in the UI
        /// </summary>
        public bool Benchmark { get; set; }

        /// <summary>
        /// Whether to render the Testing element in the UI
        /// </summary>
        public bool Testing { get; set; }

        /// <summary>
        /// Generate only documents for the specified group
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// Currently selected group
        /// </summary>
        public string GroupSelect { get; set; }

        /// <summary>
        /// Whether to render the Group element in the UI
        /// </summary>
        public bool GroupEnable { get; set; }

        /// <summary>
        /// Whether to render the SetToken element in the UI
        /// </summary>
        public bool SetToken { get; set; }

        /// <summary>
        /// Whether to open the side navigation bar
        /// </summary>
        public bool Navigtion { get; set; }

        /// <summary>
        /// Benchmark tests whether the passed parameters are JSON serialized. By default false, does not need to be serialized
        /// </summary>
        public bool BenchmarkJSON { get; set; }
    }

    /// <summary>
    /// IDoc
    /// </summary>
    public interface IDoc
    {
        /// <summary>
        /// Name
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Alias
        /// </summary>
        string Alias { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Group
        /// </summary>
        dynamic Group { get; }

        /// <summary>
        /// GroupDefault
        /// </summary>
        string GroupDefault { get; set; }

        /// <summary>
        /// Options
        /// </summary>
        Options Options { get; set; }

        /// <summary>
        /// DocGroup
        /// </summary>
        IEnumerable<KeyValuePair<DocGroup, IEnumerable<DocInfo>>> DocGroup { get; set; }
    }

    /// <summary>
    /// IDoc
    /// </summary>
    /// <typeparam name="DocArg"></typeparam>
    public interface IDoc<DocArg> : IDoc where DocArg : IDocArg<DocArg>
    {
        /// <summary>
        /// Group
        /// </summary>
        new Dictionary<string, Dictionary<string, IMember<DocArg>>> Group { get; set; }
    }

    /// <summary>
    /// IMember
    /// </summary>
    /// <typeparam name="DocArg"></typeparam>
    public interface IMember<DocArg> where DocArg : IDocArg<DocArg>
    {
        /// <summary>
        /// Key
        /// </summary>
        string Key { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Alias
        /// </summary>
        string Alias { get; set; }

        /// <summary>
        /// AliasGroup
        /// </summary>
        string AliasGroup { get; set; }

        /// <summary>
        /// HasReturn
        /// </summary>
        bool HasReturn { get; set; }

        /// <summary>
        /// HasIResult
        /// </summary>
        bool HasIResult { get; set; }

        /// <summary>
        /// Returns
        /// </summary>
        Document.DocArg Returns { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// DescriptionParam
        /// </summary>
        Dictionary<string, string> DescriptionParam { get; set; }

        /// <summary>
        /// DescriptionResult
        /// </summary>
        string DescriptionResult { get; set; }

        /// <summary>
        /// HasToken
        /// </summary>
        bool HasToken { get; set; }

        /// <summary>
        /// Args
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("properties")]
        Dictionary<string, DocArg> Args { get; set; }

        /// <summary>
        /// ArgSingle
        /// </summary>
        bool ArgSingle { get; set; }

        /// <summary>
        /// HttpFile
        /// </summary>
        bool HttpFile { get; set; }

        /// <summary>
        /// Testing
        /// </summary>
        Dictionary<string, Testing> Testing { get; set; }
    }

    /// <summary>
    /// IDocArg
    /// </summary>
    /// <typeparam name="DocArg"></typeparam>
    public interface IDocArg<DocArg> where DocArg : IDocArg<DocArg>
    {
        /// <summary>
        /// Children
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("properties")]
        Dictionary<string, DocArg> Children { get; set; }

        /// <summary>
        /// Items
        /// </summary>
        Items<DocArg> Items { get; set; }

        /// <summary>
        /// Type
        /// </summary>
        string Type { get; set; }

        /// <summary>
        /// ValueType
        /// </summary>
        bool ValueType { get; set; }

        //string Name { get; set; }

        //bool Token { get; set; }

        //Dictionary<string, object> Options { get; set; }

        //string Format { get; set; }

        //string[] Enum { get; set; }
    }

    /// <summary>
    /// Doc
    /// </summary>
    /// <typeparam name="DocArg"></typeparam>
    public class Doc<DocArg> : IDoc<DocArg> where DocArg : IDocArg<DocArg>
    {
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Friendly name
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Group
        /// </summary>
        public Dictionary<string, Dictionary<string, IMember<DocArg>>> Group { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Group
        /// </summary>
        dynamic IDoc.Group { get => Group; }

        /// <summary>
        /// GroupDefault
        /// </summary>
        public string GroupDefault { get; set; }

        /// <summary>
        /// Options
        /// </summary>
        public Options Options { get; set; }

        /// <summary>
        /// DocGroup
        /// </summary>
        public IEnumerable<KeyValuePair<DocGroup, IEnumerable<DocInfo>>> DocGroup { get; set; }

        /// <summary>
        /// Json format
        /// </summary>
        /// <returns></returns>
        public override string ToString() => new Dictionary<string, Doc<DocArg>> { { this.Name, this } }.JsonSerialize(Configer.JsonOptionsDoc);
    }

    /// <summary>
    /// Member
    /// </summary>
    /// <typeparam name="DocArg"></typeparam>
    public class Member<DocArg> : IMember<DocArg> where DocArg : IDocArg<DocArg>
    {
        /// <summary>
        /// Key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Friendly name
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Friendly name group
        /// </summary>
        public string AliasGroup { get; set; }

        /// <summary>
        /// HasReturn
        /// </summary>
        public bool HasReturn { get; set; }

        /// <summary>
        /// HasIResult
        /// </summary>
        public bool HasIResult { get; set; }

        /// <summary>
        /// Returns
        /// </summary>
        public Document.DocArg Returns { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// DescriptionParam
        /// </summary>
        public Dictionary<string, string> DescriptionParam { get; set; }

        /// <summary>
        /// DescriptionResult
        /// </summary>
        public string DescriptionResult { get; set; }

        /// <summary>
        /// HasToken
        /// </summary>
        public bool HasToken { get; set; }

        /// <summary>
        /// Args
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("properties")]
        public Dictionary<string, DocArg> Args { get; set; }

        /// <summary>
        /// ArgSingle
        /// </summary>
        public bool ArgSingle { get; set; }

        /// <summary>
        /// HttpFile
        /// </summary>
        public bool HttpFile { get; set; }

        /// <summary>
        /// Testing
        /// </summary>
        public Dictionary<string, Testing> Testing { get; set; }
    }

    /// <summary>
    /// Testing
    /// </summary>
    public readonly struct Testing
    {
        /// <summary>
        /// Testing
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <param name="token"></param>
        public Testing(string name, object value, string result, string token)
        {
            Name = name;
            Value = value;
            Result = result;
            Token = token;
            //TokenMethod = tokenMethod;
        }

        /// <summary>
        /// test key
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// test args
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// test result check
        /// </summary>
        public string Result { get; }

        /// <summary>
        /// test fixed roken
        /// </summary>
        public string Token { get; }

        ///// <summary>
        ///// Support method Result.D, input json array [\"Login\",\"{User:\\\"aaa\\\",Password:\\\"123456\\\"}\"]
        ///// </summary>
        //public string TokenMethod { get; }
    }

    /// <summary>
    /// DocArg
    /// </summary>
    public class DocArg : IDocArg<DocArg>
    {
        /// <summary>
        /// DefaultValue
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("default")]
        public object DefaultValue { get; set; }

        /// <summary>
        /// Children
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("properties")]
        public Dictionary<string, DocArg> Children { get; set; }

        /// <summary>
        /// Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// LastType
        /// </summary>
        public string LastType { get; set; }

        /// <summary>
        /// Token
        /// </summary>
        public bool Token { get; set; }

        /// <summary>
        /// Format
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Array
        /// </summary>
        public bool Array { get; set; }

        /// <summary>
        /// Enums
        /// </summary>
        public IEnumerable<EnumItems> Enums { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// DescriptionTip
        /// </summary>
        public IEnumerable<string> DescriptionTip { get; set; }

        /// <summary>
        /// UniqueItems
        /// </summary>
        public bool UniqueItems { get; set; }

        /// <summary>
        /// Options
        /// </summary>
        public Dictionary<string, object> Options { get; set; }

        /// <summary>
        /// Items
        /// </summary>
        public Items<DocArg> Items { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ValueType
        /// </summary>
        public bool ValueType { get; set; }

        #region
        /*
        public string MinLength { get; set; }

        public string MaxLength { get; set; }

        public string Minimum { get; set; }

        public string Maximum { get; set; }
        */
        #endregion
    }

    public readonly struct EnumItems
    {
        public EnumItems(string name, int value, string description)
        {
            Name = name;
            Value = value;
            Description = description;
        }

        public string Name { get; }

        public int Value { get; }

        public string Description { get; }
    }

    /// <summary>
    /// Items
    /// </summary>
    /// <typeparam name="DocArg"></typeparam>
    public class Items<DocArg> where DocArg : IDocArg<DocArg>
    {
        /// <summary>
        /// Type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Format
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Children
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("properties")]
        public Dictionary<string, DocArg> Children { get; set; }

        //public Dictionary<string, object> Options { get; set; }
    }

    /*
    public class Options
    {
        public InputAttributes InputAttributes { get; set; }

        public string InfoText { get; set; }

        public bool Compact { get; set; } //title hidden

        public bool Hidden { get; set; }

        public bool Collapsed { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "disable_collapse")]
        public bool Disable_Collapse { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "disable_edit_json")]
        public bool Disable_Edit_Json { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "disable_properties")]
        public bool Disable_Properties { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "array_controls_top")]
        public bool Array_Controls_Top { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "enum_titles")]
        public int[] Enum_Titles { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "expand_height")]
        public bool Expand_Height { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "grid_columns")]
        public int Grid_Columns { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "grid_break")]
        public bool Grid_Break { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "input_height")]
        public int Input_Height { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "input_width")]
        public int Input_Width { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "remove_empty_properties")]
        public bool Remove_Empty_Properties { get; set; }

        #region array

        [Newtonsoft.Json.JsonProperty(PropertyName = "disable_array_add")]
        public bool Disable_Array_Add { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "disable_array_delete")]
        public bool Disable_Array_Delete { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "disable_array_reorder")]
        public bool Disable_Array_Reorder { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "disable_array_delete_all_rows")]
        public bool Disable_Array_Delete_All_Rows { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "disable_array_delete_last_row")]
        public bool Disable_Array_Delete_Last_Row { get; set; }

        #endregion
    }

    public struct InputAttributes
    {
        public string Placeholder { get; set; }
    }
    */

    /// <summary>
    /// TypeDefinition
    /// </summary>
    /// <typeparam name="TypeDefinition"></typeparam>
    public readonly struct DocArgSource<TypeDefinition>
        where TypeDefinition : Meta.ITypeDefinition<TypeDefinition>
    {
        /// <summary>
        /// DocArgSource
        /// </summary>
        /// <param name="group"></param>
        /// <param name="args"></param>
        /// <param name="attributes"></param>
        /// <param name="summary"></param>
        /// <param name="enums"></param>
        public DocArgSource(string group, Meta.ITypeDefinition<TypeDefinition> args, IList<string> attributes, string summary, IEnumerable<EnumItems> enums)
        {
            Group = group;
            Args = args;
            Attributes = attributes;
            Summary = summary;
            Enums = enums;
        }

        /// <summary>
        /// Group
        /// </summary>
        public string Group { get; }

        /// <summary>
        /// Args
        /// </summary>
        public Meta.ITypeDefinition<TypeDefinition> Args { get; }

        /// <summary>
        /// Attributes
        /// </summary>
        public IList<string> Attributes { get; }

        /// <summary>
        /// Summary
        /// </summary>
        public string Summary { get; }

        /// <summary>
        /// Enums
        /// </summary>
        public IEnumerable<EnumItems> Enums { get; }
    }
    /*
    //            color
    //            date
    //            datetime
    //            datetime-local
    //            email
    //            month
    //            password
    //            number
    //            range
    //            tel
    //            text
    //            textarea
    //            time
    //            url
    //            week
    //            */
    //            /*
    //            string
    //            array
    //            object
    //            integer
    //            number
    //            boolean
    //            null
    //            */
}
