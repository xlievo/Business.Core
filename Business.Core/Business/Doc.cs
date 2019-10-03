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
            }
            catch// (System.Exception ex)
            {
                return default;
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
        string Host { get; set; }

        string Name { get; set; }

        string Description { get; set; }

        dynamic Group { get; }

        string GroupDefault { get; set; }
    }

    public interface IDoc<DocArg> : IDoc where DocArg : IDocArg<DocArg>
    {
        new Dictionary<string, Dictionary<string, IMember<DocArg>>> Group { get; set; }
    }

    public interface IMember<DocArg> where DocArg : IDocArg<DocArg>
    {
        string Name { get; set; }

        bool HasReturn { get; set; }

        Document.DocArg Returns { get; set; }

        string Description { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "properties")]
        Dictionary<string, DocArg> Args { get; set; }

        bool ArgSingle { get; set; }
    }

    public interface IDocArg<DocArg> where DocArg : IDocArg<DocArg>
    {
        [Newtonsoft.Json.JsonProperty(PropertyName = "properties")]
        Dictionary<string, DocArg> Children { get; set; }

        Items<DocArg> Items { get; set; }

        string Type { get; set; }

        //bool Token { get; set; }

        //Dictionary<string, object> Options { get; set; }

        //string Format { get; set; }

        //string[] Enum { get; set; }
    }

    public class Doc<DocArg> : IDoc<DocArg> where DocArg : IDocArg<DocArg>
    {
        public string Host { get; set; }

        public string Name { get; set; }

        public Dictionary<string, Dictionary<string, IMember<DocArg>>> Group { get; set; }

        public string Description { get; set; }

        dynamic IDoc.Group { get => Group; }

        public string GroupDefault { get; set; }

        /// <summary>
        /// Json format
        /// </summary>
        /// <returns></returns>
        public override string ToString() => new Dictionary<string, Doc<DocArg>> { { this.Name, this } }.JsonSerialize(Configer.DocJsonSettings);
    }

    public class Member<DocArg> : IMember<DocArg> where DocArg : IDocArg<DocArg>
    {
        public string Name { get; set; }

        public bool HasReturn { get; set; }

        public Document.DocArg Returns { get; set; }

        public string Description { get; set; }

        public Dictionary<string, DocArg> Args { get; set; }

        public bool ArgSingle { get; set; }

        public bool HttpFile { get; set; }
    }

    public class DocArg : IDocArg<DocArg>
    {
        [Newtonsoft.Json.JsonProperty(PropertyName = "default")]
        public string DefaultValue { get; set; }

        public Dictionary<string, DocArg> Children { get; set; }

        public string Id { get; set; }

        public string Type { get; set; }

        public string LastType { get; set; }

        public bool Token { get; set; }

        public string Format { get; set; }

        public string Title { get; set; }

        public string[] Enum { get; set; }

        public bool Array { get; set; }

        public string Description { get; set; }

        public bool UniqueItems { get; set; }

        public Dictionary<string, object> Options { get; set; }

        public Items<DocArg> Items { get; set; }

        #region
        /*
        public string MinLength { get; set; }

        public string MaxLength { get; set; }

        public string Minimum { get; set; }

        public string Maximum { get; set; }
        */
        #endregion
    }

    public class Items<DocArg> where DocArg : IDocArg<DocArg>
    {
        public string Type { get; set; }

        public string Format { get; set; }

        public string Title { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "properties")]
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

    public struct DocArgSource<TypeDefinition>
        where TypeDefinition : Meta.ITypeDefinition<TypeDefinition>
    {
        public string Group { get; set; }

        public Meta.ITypeDefinition<TypeDefinition> Args { get; set; }

        public IList<string> Attributes { get; set; }

        public string Summary { get; set; }
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
