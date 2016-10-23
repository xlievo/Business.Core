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

namespace Business.Configer
{
    using System.Configuration;
    using System.Linq;

    public class ConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("Logger")]
        public LoggerCollection Logger
        {
            get { return (LoggerCollection)this["Logger"]; }
        }

        [ConfigurationProperty("Attribute")]
        public AttributeCollection Attribute
        {
            get { return (AttributeCollection)this["Attribute"]; }
        }
    }

    [ConfigurationCollection(typeof(LoggerItem), AddItemName = "item")]
    public class LoggerCollection : ConfigurationElementCollection
    {
        public LoggerCollection()
            : base(System.StringComparer.OrdinalIgnoreCase)
        {
        }

        new public LoggerItem this[string name]
        {
            get
            {
                return (LoggerItem)base.BaseGet(name);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new LoggerItem();
        }
        protected override object GetElementKey(ConfigurationElement element)
        {
            return System.Guid.NewGuid().ToString("N");
            //return string.Format("{0}{1}", ((LoggerItem)element).Business, ((LoggerItem)element).Method);
        }

        public System.Collections.Generic.IEnumerable<string> AllKeys { get { return BaseGetAllKeys().Cast<string>(); } }
    }

    public class LoggerItem : ConfigurationElement
    {
        [ConfigurationProperty("business")]
        public string Business { get { return System.Convert.ToString(this["business"]); } }

        [ConfigurationProperty("method")]
        public string Method { get { return System.Convert.ToString(this["method"]); } }

        [ConfigurationProperty("type")]
        string type { get { return System.Convert.ToString(this["type"]); } }

        [ConfigurationProperty("canWrite")]
        string canWrite { get { return System.Convert.ToString(this["canWrite"]); } }

        [ConfigurationProperty("canValue")]
        string canValue { get { return System.Convert.ToString(this["canValue"]); } }

        [ConfigurationProperty("canResult")]
        string canResult { get { return System.Convert.ToString(this["canResult"]); } }

        public LogType? Type
        {
            get
            {
                var type = LogType.Record;

                if (LogType.TryParse(this.type, true, out type))
                {
                    return type;
                }

                return null;
            }
        }

        public bool CanWrite
        {
            get
            {
                bool canWrite = false;
                System.Boolean.TryParse(System.Convert.ToString(this.canWrite), out canWrite);
                return canWrite;
            }
        }

        public Attributes.LoggerAttribute.ValueMode? CanValue
        {
            get
            {
                var canValue = Attributes.LoggerAttribute.ValueMode.No;

                if (Attributes.LoggerAttribute.ValueMode.TryParse(this.canValue, true, out canValue))
                {
                    return canValue;
                }

                return null;
            }
        }

        public bool CanResult
        {
            get
            {
                bool canResult = false;
                System.Boolean.TryParse(this.canResult, out canResult);
                return canResult;
            }
        }
    }

    [ConfigurationCollection(typeof(AttributeItem), AddItemName = "item")]
    public class AttributeCollection : ConfigurationElementCollection
    {

        public AttributeCollection()
            : base(System.StringComparer.OrdinalIgnoreCase)
        {
        }

        new public AttributeItem this[string name]
        {
            get
            {
                return (AttributeItem)base.BaseGet(name);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new AttributeItem();
        }
        protected override object GetElementKey(ConfigurationElement element)
        {
            return System.Guid.NewGuid().ToString("N");
            //return string.Format("{0}{1}", ((LoggerItem)element).Business, ((LoggerItem)element).Method);
        }
    }

    public class AttributeItem : ConfigurationElement
    {
        [ConfigurationProperty("business")]
        public string Business { get { return System.Convert.ToString(this["business"]); } }

        [ConfigurationProperty("method")]
        public string Method { get { return System.Convert.ToString(this["method"]); } }

        [ConfigurationProperty("argument")]
        public string Argument { get { return System.Convert.ToString(this["argument"]); } }

        [ConfigurationProperty("attribute")]
        public string Attribute { get { return System.Convert.ToString(this["attribute"]); } }

        [ConfigurationProperty("member")]
        public string Member { get { return System.Convert.ToString(this["member"]); } }

        [ConfigurationProperty("value")]
        public string Value { get { return System.Convert.ToString(this["value"]); } }
    }
}