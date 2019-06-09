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

    public class Doc
    {
        public string Name { get; set; }

        public Dictionary<string, Dictionary<string, Member>> Members { get; set; }

        public class Member
        {
            //==============name===================//
            public string Name { get; set; }

            //==============hasReturn===================//
            public bool HasReturn { get; set; }

            //==============Returns===================//
            public string Returns { get; set; }

            ////===============position==================//
            //public int Position { get; set; }

            ////==============groupDefault===================//
            //public virtual string GroupDefault { get; set; }

            //==============Summary===================//
            public string Summary { get; set; }

            //==============args===================//
            public List<Arg> Args { get; set; }

            public List<Arg> ArgList { get; set; }

            public class Arg
            {
                //===============name==================//
                public string Name { get; set; }
                //===============type==================//
                [Newtonsoft.Json.JsonIgnore]
                public virtual System.Type Type { get; set; }
                //===============type==================//
                public bool IsEnum { get; set; }
                public bool IsCollection { get; set; }
                public bool IsDictionary { get; set; }
                public bool IsNumeric { get; set; }
                public string[] EnumNames { get; set; }
                public System.Array EnumValues { get; set; }
                //===============useType==================//
                public bool UseType { get; set; }
                ////===============position==================//
                //public int Position { get; set; }
                ////===============hasDefaultValue==================//
                public bool HasDefaultValue { get; set; }
                //===============defaultValue==================//
                public object DefaultValue { get; set; }
                //===============argAttr==================//
                public IEnumerable<Attribute> Attrs { get; set; }
                //===============hasDefinition==================//
                //public bool HasDefinition { get; set; }
                ////==============group===================//
                //public string Group { get; set; }
                ////==============hasChild===================//
                //public virtual bool HasChild { get; set; }
                //===============child==================//
                public List<Arg> Child { get; set; }
                //===============childAll==================//
                public List<Arg> ChildAll { get; set; }
                //==============Summary===================//
                public string Summary { get; set; }
                //===============nick==================//
                public string Nick { get; set; }

                public string Path { get; set; }

                public string Parent { get; set; }

                public string Root { get; set; }

                public class Attribute
                {
                    //public string Nick { get; set; }
                    public string Key { get; set; }

                    public string Type { get; set; }

                    public string Description { get; set; }

                    public string Message { get; set; }

                    public int State { get; set; }

                    public bool CollectionItem { get; set; }
                }
            }
        }
    }
}
