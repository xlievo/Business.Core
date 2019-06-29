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

namespace Business.Document
{
    using System.Collections.Generic;
    using System.Linq;
    using Business.Utils;

    [System.Xml.Serialization.XmlRoot("doc")]
    public class Xml
    {
        public static Xml DeserializeDoc(string xml)
        {
            if (null == xml) { throw new System.ArgumentNullException(nameof(xml)); }

            using (var reader = new System.IO.StringReader(xml))
            {
                var xmldes = new System.Xml.Serialization.XmlSerializer(typeof(Xml));

                try
                {
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

                            var subList = new List<string>(member.summary.para.Select(c => c.text));
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

                            var subList = new List<string>(member.returns.para.Select(c => c.text));
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

                            var subList = new List<string>(item.para.Select(c => c.text));
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
                catch// (System.Exception ex)
                {
                    return default;
                }
            }
        }

        public static string WhitespaceTrim(string value)
        {
            if (null == value)
            {
                return null;
            }

            var lin = value.Trim().Split(new char[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries).Select(c => c.Trim());

            return string.Join(System.Environment.NewLine, lin).Trim();
        }

        [System.Xml.Serialization.XmlElement("assembly")]
        public assembly _assembly;

        public List<member> members;

        public class assembly
        {
            public string name;

            public override string ToString() => name;
        }

        public class member
        {
            [System.Xml.Serialization.XmlAttribute("name")]
            public string name;

            [System.Xml.Serialization.XmlElement("summary")]
            public values summary;

            [System.Xml.Serialization.XmlElement("param")]
            public List<param> _params;

            [System.Xml.Serialization.XmlElement("returns")]
            public values returns;

            public override string ToString() => name;

            public class param
            {
                [System.Xml.Serialization.XmlAttribute("name")]
                public string name;

                [System.Xml.Serialization.XmlText]
                public string text;

                [System.Xml.Serialization.XmlElement("para")]
                public List<para> para;

                public string sub;

                public override string ToString() => $"{name} {text}";
            }

            public class values
            {
                [System.Xml.Serialization.XmlText]
                public string text;

                [System.Xml.Serialization.XmlElement("para")]
                public List<para> para;

                public string sub;

                public override string ToString() => text;
            }

            public class para
            {
                [System.Xml.Serialization.XmlText]
                public string text;

                public override string ToString() => text;
            }
        }
    }

    public interface IDoc
    {
        string Name { get; set; }

        dynamic Members { get; }
    }

    public interface IDoc<DocArg> : IDoc where DocArg : IDocArg<DocArg>
    {
        new Dictionary<string, Dictionary<string, IMember<DocArg>>> Members { get; set; }
    }

    public interface IMember<DocArg> where DocArg : IDocArg<DocArg>
    {
        string Name { get; set; }

        bool HasReturn { get; set; }

        Help.TypeDefinition Returns { get; set; }

        string Summary { get; set; }

        Dictionary<string, DocArg> Args { get; set; }
    }

    public interface IDocArg<Arg> where Arg : IDocArg<Arg>
    {
        Dictionary<string, Arg> Children { get; set; }
    }

    public class Doc<DocArg> : IDoc<DocArg> where DocArg : IDocArg<DocArg>
    {
        public string Name { get; set; }

        public Dictionary<string, Dictionary<string, IMember<DocArg>>> Members { get; set; }

        dynamic IDoc.Members { get => Members; }
    }

    public class Member<DocArg> : IMember<DocArg> where DocArg : IDocArg<DocArg>
    {
        public string Name { get; set; }

        public bool HasReturn { get; set; }

        public Help.TypeDefinition Returns { get; set; }

        public string Summary { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "properties")]
        public Dictionary<string, DocArg> Args { get; set; }
    }

    public class DocArg : IDocArg<DocArg>
    {
        [Newtonsoft.Json.JsonProperty(PropertyName = "properties")]
        public Dictionary<string, DocArg> Children { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "id")]
        public string ID { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "format")]
        public string Format { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "enum")]
        public string[] Enum { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "default")]
        public string DefaultValue { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "uniqueItems")]
        public bool UniqueItems { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "options")]
        public Options Options { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "items")]
        public Items<DocArg> Items { get; set; }

        #region
        /*
        [Newtonsoft.Json.JsonProperty(PropertyName = "minLength")]
        public string MinLength { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "maxLength")]
        public string MaxLength { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "minimum")]
        public string Minimum { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "maximum")]
        public string Maximum { get; set; }
        */
        #endregion
    }

    public class Options
    {
        [Newtonsoft.Json.JsonProperty(PropertyName = "infoText")]
        public string InfoText { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "collapsed")]
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

        [Newtonsoft.Json.JsonProperty(PropertyName = "hidden")]
        public bool Hidden { get; set; }

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

    public class Items<DocArg> where DocArg : IDocArg<DocArg>
    {
        [Newtonsoft.Json.JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "properties")]
        public Dictionary<string, DocArg> Children { get; set; }
    }

    public struct DocArgSource
    {
        public string Group { get; set; }

        public Meta.Args Args { get; set; }

        public IList<Attributes.ArgumentAttribute> Attributes { get; set; }

        public string Summary { get; set; }
    }
    //public class Doc
    //{
    //    public string Name { get; set; }

    //    public Dictionary<string, Dictionary<string, Member>> Members { get; set; }

    //    public class Member
    //    {
    //        //==============name===================//
    //        public string Name { get; set; }

    //        //==============hasReturn===================//
    //        public bool HasReturn { get; set; }

    //        //==============ReturnType===================//
    //        public Help.TypeDefinition Returns { get; set; }

    //        //==============Returns===================//
    //        //public string Returns { get; set; }

    //        ////===============position==================//
    //        //public int Position { get; set; }

    //        ////==============groupDefault===================//
    //        //public virtual string GroupDefault { get; set; }

    //        //==============Summary===================//
    //        public string Summary { get; set; }

    //        //==============args===================//
    //        public ReadOnlyCollection<Arg> Args { get; set; }

    //        public ReadOnlyCollection<Arg> ArgList { get; set; }

    //        public class Arg
    //        {
    //            /*
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
    //            public struct json
    //            {
    //                public string title { get; set; }

    //                public string type { get; set; }

    //                public string format { get; set; }

    //                [Newtonsoft.Json.JsonProperty(PropertyName = "default")]
    //                public string _default { get; set; }

    //                public IDictionary<string, object> options { get; set; }

    //                [Newtonsoft.Json.JsonProperty(PropertyName = "enum")]
    //                public string[] _enum { get; set; }

    //                public string enumSource { get; set; }

    //                public IDictionary<string, object> properties { get; set; }

    //                public string basicCategoryTitle { get; set; }

    //                public IDictionary<string, object> watch { get; set; }
    //            }
    //            //===============name==================//
    //            public string Name { get; set; }
    //            //===============type==================//
    //            public System.Type Type { get; set; }
    //            //===============type==================//
    //            public bool IsEnum { get; set; }
    //            public bool IsCollection { get; set; }
    //            public bool IsDictionary { get; set; }
    //            public bool IsNumeric { get; set; }
    //            public string[] EnumNames { get; set; }
    //            public System.Array EnumValues { get; set; }
    //            //===============useType==================//
    //            public bool UseType { get; set; }
    //            ////===============position==================//
    //            //public int Position { get; set; }
    //            ////===============hasDefaultValue==================//
    //            public bool HasDefaultValue { get; set; }
    //            //===============defaultValue==================//
    //            public object DefaultValue { get; set; }
    //            //===============argAttr==================//
    //            public IEnumerable<Attribute> Attrs { get; set; }
    //            //===============hasDefinition==================//
    //            public bool HasDefinition { get; set; }
    //            ////==============group===================//
    //            //public string Group { get; set; }
    //            ////==============hasChild===================//
    //            //public virtual bool HasChild { get; set; }
    //            //===============children==================//
    //            public ReadOnlyCollection<Arg> Children { get; set; }
    //            //===============childrens==================//
    //            public ReadOnlyCollection<Arg> Childrens { get; set; }
    //            //==============Summary===================//
    //            public string Summary { get; set; }
    //            //===============nick==================//
    //            public string Nick { get; set; }

    //            public string Path { get; set; }

    //            public string Parent { get; set; }

    //            public string Root { get; set; }

    //            public class Attribute
    //            {
    //                //public string Nick { get; set; }
    //                public string Key { get; set; }

    //                public string Type { get; set; }

    //                public string Description { get; set; }

    //                public string Message { get; set; }

    //                public int State { get; set; }

    //                public bool CollectionItem { get; set; }
    //            }
    //        }
    //    }
    //}
}
