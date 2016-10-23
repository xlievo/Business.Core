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

namespace Business.Attributes
{
    using System.Linq;
    using Result;

    #region abstract

    #region

    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class IgnoreAttribute : System.Attribute { }

    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class ConfigAttribute : System.Attribute
    {
        public ConfigAttribute(string name, int port = 5000)
        {
            this.name = name;
            this.port = port;
        }

        public ConfigAttribute(int port, string name = null)
        {
            this.port = port;
            this.name = name;
        }

        string name;
        public string Name { get { return this.name; } internal set { this.name = value; } }

        int port;
        public int Port { get { return this.port; } internal set { this.port = value; } }
    }

    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Method | System.AttributeTargets.Struct | System.AttributeTargets.Parameter, AllowMultiple = true, Inherited = true)]
    public class LoggerAttribute : System.Attribute, Extensions.ICloneable<LoggerAttribute>
    {
        public LoggerAttribute Clone()
        {
            return new LoggerAttribute(logType, canWrite) { CanValue = CanValue, CanResult = CanResult };
        }

        object System.ICloneable.Clone()
        {
            return this.Clone();
        }

        public enum ValueMode
        {
            No = 0,
            Select = 1,
            All = 2,
        }

        public LoggerAttribute(LogType logType, bool canWrite = true)
        {
            this.logType = logType;
            this.canWrite = canWrite;
        }

        readonly LogType logType;
        public LogType LogType
        {
            get { return logType; }
        }

        bool canWrite;
        public bool CanWrite { get { return canWrite; } set { canWrite = value; } }

        public ValueMode CanValue { get; set; }

        public bool CanResult { get; set; }
    }

    #endregion

    //public interface IAuthAttribute
    //{
    //    int Code { get; }

    //    string Message { get; }

    //    bool TrimChar { get; set; }

    //    //string Group { get; set; }

    //    //string RelationArg { get; set; }

    //    IResult Proces(dynamic value, System.Type type, string method, string member, dynamic business);
    //}

    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct | System.AttributeTargets.Property | System.AttributeTargets.Field | System.AttributeTargets.Parameter, AllowMultiple = true, Inherited = true)]
    public abstract class ArgumentAttribute : System.Attribute
    {
        #region MetaData

        public static readonly System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, System.Tuple<System.Type, System.Action<object, object>>>> MetaData;

        //public struct Member
        //{
        //    public Member(System.Collections.Generic.Dictionary<string, System.Tuple<System.Type, System.Action<object, object>>> accessor)
        //    {
        //        this.accessor = accessor;
        //    }

        //    readonly System.Collections.Generic.Dictionary<string, System.Tuple<System.Type, System.Action<object, object>>> accessor;
        //    public System.Collections.Generic.Dictionary<string, System.Tuple<System.Type, System.Action<object, object>>> Accessor { get { return accessor; } }
        //}

        static System.Collections.Generic.Dictionary<string, System.Tuple<System.Type, System.Action<object, object>>> GetMetaData(System.Type type)
        {
            var member = new System.Collections.Generic.Dictionary<string, System.Tuple<System.Type, System.Action<object, object>>>();

            var fields = type.GetFields();
            foreach (var field in fields)
            {
                member.Add(field.Name, System.Tuple.Create(field.FieldType, Extensions.Emit.FieldAccessorGenerator.CreateSetter(field)));
            }

            var propertys = type.GetProperties();
            foreach (var property in propertys)
            {
                var setter = Extensions.Emit.PropertyAccessorGenerator.CreateSetter(property);
                if (null == setter) { continue; }
                member.Add(property.Name, System.Tuple.Create(property.PropertyType, setter));
            }

            return member;
        }

        static ArgumentAttribute()
        {
            MetaData = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, System.Tuple<System.Type, System.Action<object, object>>>>();

            var ass = System.AppDomain.CurrentDomain.GetAssemblies().Where(c => !c.IsDynamic);

            foreach (var item in ass)
            {
                try
                {
                    var types = item.GetExportedTypes();

                    foreach (var type in types)
                    {
                        if (type.IsSubclassOf(typeof(ArgumentAttribute)) && !type.IsAbstract)
                        {
                            MetaData.Add(type.FullName, GetMetaData(type));
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    System.IO.File.AppendAllText(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Business.Lib.log.txt"), string.Format("{1}{0}{2}{0}", System.Environment.NewLine, item.FullName, ex), System.Text.Encoding.UTF8);
                }
            }
        }

        #endregion

        public virtual bool MemberSet(System.Type type, string value, out object outValue)
        {
            outValue = null;

            if (type.IsEnum)
            {
                if (System.String.IsNullOrWhiteSpace(value)) { return false; }

                var enums = System.Enum.GetValues(type).Cast<System.Enum>();
                var enumValue = enums.FirstOrDefault(c => value.Equals(c.ToString(), System.StringComparison.InvariantCultureIgnoreCase));
                if (null != enumValue)
                {
                    outValue = enumValue;
                    return true;
                }
                return false;
            }
            else
            {
                switch (type.FullName)
                {
                    case "System.String":
                        outValue = value;
                        return true;
                    case "System.Int16":
                        short value2;
                        if (!System.Int16.TryParse(value, out value2))
                        {
                            outValue = value2;
                            return true;
                        }
                        return false;
                    case "System.Int32":
                        int value3;
                        if (System.Int32.TryParse(value, out value3))
                        {
                            outValue = value3;
                            return true;
                        }
                        return false;
                    case "System.Int64":
                        long value4;
                        if (System.Int64.TryParse(value, out value4))
                        {
                            outValue = value4;
                            return true;
                        }
                        return false;
                    case "System.Decimal":
                        decimal value5;
                        if (System.Decimal.TryParse(value, out value5))
                        {
                            outValue = value5;
                            return true;
                        }
                        return false;
                    case "System.Double":
                        double value6;
                        if (System.Double.TryParse(value, out value6))
                        {
                            outValue = value6;
                            return true;
                        }
                        return false;
                    default: return false;
                }
            }
        }

        internal bool MemberSet(string member, string value)
        {
            System.Collections.Generic.Dictionary<string, System.Tuple<System.Type, System.Action<object, object>>> meta;
            if (MetaData.TryGetValue(this.GetType().FullName, out meta))
            {
                System.Tuple<System.Type, System.Action<object, object>> accessor;
                if (meta.TryGetValue(member, out accessor))
                {
                    if (null == accessor.Item2) { return false; }

                    try
                    {
                        object outValue;
                        if (MemberSet(accessor.Item1, value, out outValue))
                        {
                            accessor.Item2(this, outValue);
                            return true;
                        }
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        public ArgumentAttribute(int code, string message = null)
        {
            this.fullName = this.GetType().FullName;
            this.code = code;
            this.message = message;
        }

        internal readonly string fullName;

        int code;
        public int Code { get { return code; } set { this.code = value; } }

        string message;
        public string Message { get { return message; } set { this.message = value; } }

        public bool TrimChar { get; set; }

        public string Group { get; set; }

        //public string Group { get; set; }

        //public string RelationArg { get; set; }

        //protected internal bool HasDeserialize { get; internal set; }
        //public bool HasCommand { get; internal set; }

        //protected internal System.Type ResultType { get; internal set; }

        public abstract IResult Proces(dynamic value, System.Type type, string method, string member, dynamic business);

        #region Result

        public IResult ResultCreate()
        {
            return ResultFactory.Create();
        }

        public IResult ResultCreate(int state)
        {
            return ResultFactory.Create(state);
        }

        public IResult ResultCreate(int state, string message)
        {
            return ResultFactory.Create(state, message);
        }

        public IResult<Data> ResultCreate<Data>(Data data, int state = 1)
        {
            return ResultFactory.Create(data, state);
        }

        #endregion
    }

    //[System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct | System.AttributeTargets.Parameter, AllowMultiple = true, Inherited = true)]
    //public abstract class ArgAttribute : ArgumentAttribute
    //{
    //    public ArgAttribute(int code, string message = null)
    //        : base(code, message) { }

    //    public string Group { get; set; }

    //    public override abstract IResult Proces(dynamic value, System.Type type, string method, string member, dynamic business);
    //}
    
    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class CommandAttribute : System.Attribute
    {
        public CommandAttribute(string onlyName = null) { this.OnlyName = onlyName; }

        public bool TrimChar { get; set; }

        public string Group { get; set; }

        public string OnlyName { get; set; }

        //public bool HasUnified { get; set; }

        //public override IResult Proces(dynamic value, System.Type type, string method, string member, dynamic business)
        //{
        //    return this.ResultCreate();
        //}
    }

    //[System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct | System.AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    //public class DeserializeAttribute : ArgumentAttribute
    //{
    //    public DeserializeAttribute(int code = -901, string message = null)
    //        : base(code, message) { }

    //    public override IResult Proces(dynamic value, System.Type type, string method, string member, dynamic business)
    //    {
    //        try
    //        {
    //            return this.ResultCreate(value);
    //        }
    //        catch { return this.ResultCreate(Code, Message ?? string.Format("Arguments {0} deserialize error", member)); }
    //    }
    //}

    //public enum LogMode
    //{
    //    No = 0,
    //    In = 1,
    //    Out = 2,
    //    All = 3
    //}

    //[System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct | System.AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    //public class ArgLog : System.Attribute
    //{
    //    public ArgLog(LogMode log)
    //    {
    //        this.log = log;
    //    }

    //    readonly LogMode log;
    //    public LogMode Log { get { return log; } }
    //}

    #endregion

    #region

    /*
    public sealed class JsonArgAttribute : DeserializeArgAttribute
    {
        public JsonArgAttribute(int code = -902, string message = null)
            : base(code, message) { }

        public override IResult Proces(dynamic value, System.Type type, string method, string member, dynamic business)
        {
            try
            {
                return this.ResultCreate(Newtonsoft.Json.JsonConvert.DeserializeObject(System.Convert.ToString(value), type));
            }
            catch { return this.ResultCreate(Code, Message ?? string.Format("Arguments {0} Json deserialize error", member)); }
        }
    }

    public sealed class ProtoBufArgAttribute : DeserializeArgAttribute
    {
        public ProtoBufArgAttribute(int code = -903, string message = null)
            : base(code, message) { }

        public override IResult Proces(dynamic value, System.Type type, string method, string member, dynamic business)
        {
            try
            {
                using (var stream = new System.IO.MemoryStream((byte[])value))
                {
                    return this.ResultCreate(ProtoBuf.Meta.RuntimeTypeModel.Default.Deserialize(stream, null, type));
                }
            }
            catch { return this.ResultCreate(Code, Message ?? string.Format("Arguments {0} ProtoBuf deserialize error", member)); }
        }
    }
    */
    /*
    public class CmdAttribute : CommandAttribute
    {
        public CmdAttribute(string onlyName = null, int code = -901, string message = null) : base(code, message) { this.OnlyName = onlyName; }

        public enum DataType
        {
            Byte,
            ProtoBuf,
            Json,
        }

        public new DataType ResultType { get { return (DataType)base.ResultType; } set { base.ResultType = (int)value; } }

        public override IResult Proces(dynamic value, System.Type type, string method, string member, dynamic business)
        {
            try
            {
                return this.ResultCreate(value);
            }
            catch { return this.ResultCreate(Code, Message ?? string.Format("Arguments {0} deserialize error", member)); }
        }
    }
    */
    //public sealed class JsonCmdAttribute : CommandAttribute
    //{
    //    public JsonCmdAttribute(string onlyName = null, int code = -902, string message = null) : base(onlyName, code, message) { }

    //    public override IResult Proces(dynamic value, System.Type type, string method, string member, dynamic business)
    //    {
    //        try
    //        {
    //            return this.ResultCreate(Newtonsoft.Json.JsonConvert.DeserializeObject(System.Convert.ToString(value), type));
    //        }
    //        catch { return this.ResultCreate(Code, Message ?? string.Format("Arguments {0} Json deserialize error", member)); }
    //    }
    //}

    //public sealed class ProtoBufCmdAttribute : CommandAttribute
    //{
    //    public ProtoBufCmdAttribute(string onlyName = null, int code = -903, string message = null) : base(onlyName, code, message) { }

    //    public override IResult Proces(dynamic value, System.Type type, string method, string member, dynamic business)
    //    {
    //        try
    //        {
    //            using (var stream = new System.IO.MemoryStream((byte[])value))
    //            {
    //                return this.ResultCreate(ProtoBuf.Meta.RuntimeTypeModel.Default.Deserialize(stream, null, type));
    //            }
    //        }
    //        catch { return this.ResultCreate(Code, Message ?? string.Format("Arguments {0} ProtoBuf deserialize error", member)); }
    //    }
    //}

    public sealed class CanNotNullAttribute : ArgumentAttribute
    {
        public CanNotNullAttribute(int code = -800, string message = null)
            : base(code, message) { }

        public override IResult Proces(dynamic value, System.Type type, string method, string member, dynamic business)
        {
            if (typeof(System.String).Equals(type))
            {
                if (System.String.IsNullOrEmpty(value))
                {
                    return this.ResultCreate(Code, Message ?? string.Format("argument \"{0}\" can not null.", member));
                }
            }
            else if (System.Object.Equals(null, value))
            {
                return this.ResultCreate(Code, Message ?? string.Format("argument \"{0}\" can not null.", member));
            }

            return this.ResultCreate();
        }
    }

    public sealed class SizeAttribute : ArgumentAttribute
    {
        public SizeAttribute(int code = -801, string message = null)
            : base(code, message) { }

        public object Min { get; set; }
        public object Max { get; set; }

        public override IResult Proces(dynamic value, System.Type type, string method, string member, dynamic business)
        {
            if (System.Object.Equals(null, value)) { return this.ResultCreate(); }

            var msg = System.String.Empty;

            switch (type.FullName)
            {
                case "System.String":
                    var ags1 = value.Trim();
                    if (null != Min && Extensions.Help.ChangeType<System.Int32>(Min) > ags1.Length)
                    {
                        msg = string.Format("argument \"{0}\" minimum length value {1}.", member, Min);
                    }
                    if (null != Max && Extensions.Help.ChangeType<System.Int32>(Max) < ags1.Length)
                    {
                        msg = string.Format("argument \"{0}\" maximum length value {1}.", member, Max);
                    }
                    break;
                case "System.DateTime":
                    var ags2 = System.Convert.ToDateTime(value);
                    if (null != Min && Extensions.Help.ChangeType<System.DateTime>(Min) > ags2)
                    {
                        msg = string.Format("argument \"{0}\" minimum value {1}.", member, Min);
                    }
                    if (null != Max && Extensions.Help.ChangeType<System.DateTime>(Max) < ags2)
                    {
                        msg = string.Format("argument \"{0}\" maximum value {1}.", member, Max);
                    }
                    break;
                case "System.Int32":
                    var ags3 = System.Convert.ToInt32(value);
                    if (null != Min && Extensions.Help.ChangeType<System.Int32>(Min) > ags3)
                    {
                        msg = string.Format("argument \"{0}\" minimum value {1}.", member, Min);
                    }
                    if (null != Max && Extensions.Help.ChangeType<System.Int32>(Max) < ags3)
                    {
                        msg = string.Format("argument \"{0}\" maximum value {1}.", member, Max);
                    }
                    break;
                case "System.Int64":
                    var ags4 = System.Convert.ToInt64(value);
                    if (null != Min && Extensions.Help.ChangeType<System.Int64>(Min) > ags4)
                    {
                        msg = string.Format("argument \"{0}\" minimum value {1}.", member, Min);
                    }
                    if (null != Max && Extensions.Help.ChangeType<System.Int64>(Max) < ags4)
                    {
                        msg = string.Format("argument \"{0}\" maximum value {1}.", member, Max);
                    }
                    break;
                case "System.Decimal":
                    var ags5 = System.Convert.ToDecimal(value);
                    if (null != Min && Extensions.Help.ChangeType<System.Decimal>(Min) > ags5)
                    {
                        msg = string.Format("argument \"{0}\" minimum value {1}.", member, Min);
                    }
                    if (null != Max && Extensions.Help.ChangeType<System.Decimal>(Max) < ags5)
                    {
                        msg = string.Format("argument \"{0}\" maximum value {1}.", member, Max);
                    }
                    break;
                case "System.Double":
                    var ags6 = System.Convert.ToDouble(value);
                    if (null != Min && Extensions.Help.ChangeType<System.Double>(Min) > ags6)
                    {
                        msg = string.Format("argument \"{0}\" minimum value {1}.", member, Min);
                    }
                    if (null != Max && Extensions.Help.ChangeType<System.Double>(Max) < ags6)
                    {
                        msg = string.Format("argument \"{0}\" maximum value {1}.", member, Max);
                    }
                    break;
                default:
                    var iList = type.GetInterface("System.Collections.IList");
                    if (null != iList)
                    {
                        var list = value as System.Collections.IList;
                        if (null != Min && Extensions.Help.ChangeType<System.Int32>(Min) > list.Count)
                        {
                            msg = string.Format("argument \"{0}\" minimum count value {1}.", member, Min);
                        }
                        if (null != Max && Extensions.Help.ChangeType<System.Int32>(Max) < list.Count)
                        {
                            msg = string.Format("argument \"{0}\" maximum count value {1}.", member, Max);
                        }
                    }
                    break;
            }

            if (!System.String.IsNullOrEmpty(msg))
            {
                return this.ResultCreate(Code, Message ?? msg);
            }

            return this.ResultCreate();
        }
    }

    public sealed class ScaleAttribute : ArgumentAttribute
    {
        public ScaleAttribute(int code = -802, string message = null)
            : base(code, message) { }

        int size = 2;
        public int Size { get { return size; } set { size = value; } }

        public override IResult Proces(dynamic value, System.Type type, string method, string member, dynamic business)
        {
            dynamic sp;

            switch (type.FullName)
            {
                case "System.Decimal":
                    sp = System.Convert.ToDecimal(System.Math.Pow(10, size));
                    break;
                case "System.Double":
                    sp = System.Convert.ToDouble(System.Math.Pow(10, size));
                    break;
                default: return this.ResultCreate();
            }

            var t = System.Math.Truncate(value);

            var result = t + (0 > value ? System.Math.Ceiling((value - t) * sp) : System.Math.Floor((value - t) * sp)) / sp;

            return this.ResultCreate(result);
        }
    }

    public sealed class CheckEmailAttribute : ArgumentAttribute
    {
        public CheckEmailAttribute(int code = -803, string message = null)
            : base(code, message) { }

        public override IResult Proces(dynamic value, System.Type type, string method, string member, dynamic business)
        {
            if (System.Object.Equals(null, value)) { return this.ResultCreate(); }

            var _value = System.Convert.ToString(value).Trim();
            if (!System.String.IsNullOrEmpty(_value) && !Extensions.Help.CheckEmail(_value))
            {
                return this.ResultCreate(Code, Message ?? string.Format("argument \"{0}\" email error.", member));
            }
            return this.ResultCreate();
        }
    }

    public sealed class CheckCharAttribute : ArgumentAttribute
    {
        public CheckCharAttribute(int code = -804, string message = null)
            : base(code, message) { }

        CheckCharMode mode = CheckCharMode.All;
        public CheckCharMode Mode { get { return mode; } set { mode = value; } }

        public override IResult Proces(dynamic value, System.Type type, string method, string member, dynamic business)
        {
            if (System.Object.Equals(null, value)) { return this.ResultCreate(); }

            var _value = value.Trim();
            if (!System.String.IsNullOrEmpty(_value) && !CheckChar(_value, Mode))
            {
                return this.ResultCreate(Code, Message ?? string.Format("argument \"{0}\" char verification failed.", member));
            }
            return this.ResultCreate();
        }

        [System.Flags]
        public enum CheckCharMode
        {
            All = 0,
            Number = 1,
            Upper = 2,
            Lower = 4,
            Chinese = 8
        }

        static bool CheckChar(string value, CheckCharMode mode = CheckCharMode.All)
        {
            if (null == value || System.String.IsNullOrEmpty(value)) { return false; }

            var list = new System.Collections.Generic.List<int>();
            for (int i = 0; i < value.Length; i++) { list.Add(value[i]); }

            System.Predicate<int> number = delegate(int c) { return !(c >= 48 && c <= 57); };
            System.Predicate<int> upper = delegate(int c) { return !(c >= 65 && c <= 90); };
            System.Predicate<int> lower = delegate(int c) { return !(c >= 97 && c <= 122); };
            System.Predicate<int> chinese = delegate(int c) { return !(c >= 0x4e00 && c <= 0x9fbb); };

            switch (mode)
            {
                case CheckCharMode.All:
                    return !list.Exists(c =>
                number(c) &&
                upper(c) &&
                lower(c) &&
                chinese(c));
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
    }

    #endregion

    //public virtual IResult CheckAuth(object value, string mname)
    //{
    //    if (System.Object.Equals(null, value)) { return null; }

    //    Auth.Token _token = System.Convert.ToString(value);

    //    if (null == _token) { return null; }

    //    //var session = GetSession<Session>(_token);

    //    //if (null == session)
    //    //{
    //    //    //return this.ResultCreate<Result>(Mark.MarkItem.Exp_SessionOut);
    //    //    return null;
    //    //}

    //    var session = Extensions.Help.ProtoBufDeserialize(null, sessionType);

    //    return this.ResultCreate(session);
    //    //return session;
    //    //throw new System.NotImplementedException();
    //}

    #region Deserialize

    //public sealed class JsonCmdAttribute : CommandAttribute
    //{
    //    public JsonCmdAttribute(string onlyName = null, int code = -12, string message = null) : base(onlyName, code, message) { }

    //    public override IResult Proces(dynamic value, System.Type type, string method, string member, dynamic business)
    //    {
    //        try
    //        {
    //            return this.ResultCreate(Newtonsoft.Json.JsonConvert.DeserializeObject(System.Convert.ToString(value), type));
    //        }
    //        catch { return this.ResultCreate(Code, Message ?? string.Format("Arguments {0} Json deserialize error", member)); }
    //    }
    //}

    //public sealed class ProtoBufCmdAttribute : CommandAttribute
    //{
    //    public ProtoBufCmdAttribute(string onlyName = null, int code = -13, string message = null) : base(onlyName, code, message) { }

    //    public override IResult Proces(dynamic value, System.Type type, string method, string member, dynamic business)
    //    {
    //        try
    //        {
    //            using (var stream = new System.IO.MemoryStream((byte[])value))
    //            {
    //                return this.ResultCreate(ProtoBuf.Meta.RuntimeTypeModel.Default.Deserialize(stream, null, type));
    //            }
    //        }
    //        catch { return this.ResultCreate(Code, Message ?? string.Format("Arguments {0} ProtoBuf deserialize error", member)); }
    //    }
    //}

    public sealed class JsonArgAttribute : ArgumentAttribute
    {
        public JsonArgAttribute(int code = -12, string message = null)
            : base(code, message) { }

        public override IResult Proces(dynamic value, System.Type type, string method, string member, dynamic business)
        {
            try
            {
                return this.ResultCreate(Newtonsoft.Json.JsonConvert.DeserializeObject(value, type));
            }
            catch { return this.ResultCreate(Code, Message ?? string.Format("Arguments {0} Json deserialize error", member)); }
        }
    }

    public sealed class ProtoBufArgAttribute : ArgumentAttribute
    {
        public ProtoBufArgAttribute(int code = -13, string message = null)
            : base(code, message) { }

        public override IResult Proces(dynamic value, System.Type type, string method, string member, dynamic business)
        {
            try
            {
                using (var stream = new System.IO.MemoryStream(value))
                {
                    return this.ResultCreate(ProtoBuf.Meta.RuntimeTypeModel.Default.Deserialize(stream, null, type));
                }
            }
            catch { return this.ResultCreate(Code, Message ?? string.Format("Arguments {0} ProtoBuf deserialize error", member)); }
        }
    }

    #endregion
}
