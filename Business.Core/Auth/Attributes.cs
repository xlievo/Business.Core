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
    using Result;

    #region abstract

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
        public ArgumentAttribute(int code, string message = null)
        {
            this.code = code;
            this.message = message;
        }

        readonly int code;
        public int Code { get { return code; } }

        readonly string message;
        public string Message { get { return message; } }

        public bool TrimChar { get; set; }

        //public string Group { get; set; }

        //public string RelationArg { get; set; }

        //protected internal bool HasDeserialize { get; internal set; }
        //public bool HasCommand { get; internal set; }

        //protected internal System.Type ResultType { get; internal set; }

        public abstract IResult Proces(dynamic value, System.Type type, string method, string member, dynamic business);
    }

    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct | System.AttributeTargets.Parameter, AllowMultiple = true, Inherited = true)]
    public abstract class ArgAttribute : ArgumentAttribute
    {
        public ArgAttribute(int code, string message = null)
            : base(code, message) { }

        public string Group { get; set; }

        public override abstract IResult Proces(dynamic value, System.Type type, string method, string member, dynamic business);
    }

    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class CommandAttribute : ArgAttribute
    {
        public CommandAttribute(string onlyName = null, int code = -901, string message = null) : base(code, message) { this.OnlyName = onlyName; }

        public string OnlyName { get; set; }

        public bool HasUnified { get; set; }

        public override IResult Proces(dynamic value, System.Type type, string method, string member, dynamic business)
        {
            return ResultFactory.Create();
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct | System.AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class DeserializeAttribute : ArgAttribute
    {
        public DeserializeAttribute(int code = -901, string message = null)
            : base(code, message) { }

        public override IResult Proces(dynamic value, System.Type type, string method, string member, dynamic business)
        {
            try
            {
                return ResultFactory.Create(value);
            }
            catch { return ResultFactory.Create(Code, Message ?? string.Format("Arguments {0} deserialize error", member)); }
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class BusinessLogAttribute : System.Attribute
    {
        public BusinessLogAttribute(bool notRecord = false)
        {
            this.notRecord = notRecord;
        }

        readonly bool notRecord;
        public bool NotRecord { get { return notRecord; } }

        public bool NotValue { get; set; }

        public bool NotResult { get; set; }
    }

    [System.AttributeUsage(System.AttributeTargets.Method | System.AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class NotInterceptAttribute : System.Attribute { }

    public enum LogMode
    {
        No = 0,
        In = 1,
        Out = 2,
        All = 3
    }

    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct | System.AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class ArgLog : System.Attribute
    {
        public ArgLog(LogMode log)
        {
            this.log = log;
        }

        readonly LogMode log;
        public LogMode Log { get { return log; } }
    }

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
                return ResultFactory.Create(Newtonsoft.Json.JsonConvert.DeserializeObject(System.Convert.ToString(value), type));
            }
            catch { return ResultFactory.Create(Code, Message ?? string.Format("Arguments {0} Json deserialize error", member)); }
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
                    return ResultFactory.Create(ProtoBuf.Meta.RuntimeTypeModel.Default.Deserialize(stream, null, type));
                }
            }
            catch { return ResultFactory.Create(Code, Message ?? string.Format("Arguments {0} ProtoBuf deserialize error", member)); }
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
                return ResultFactory.Create(value);
            }
            catch { return ResultFactory.Create(Code, Message ?? string.Format("Arguments {0} deserialize error", member)); }
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
    //            return ResultFactory.Create(Newtonsoft.Json.JsonConvert.DeserializeObject(System.Convert.ToString(value), type));
    //        }
    //        catch { return ResultFactory.Create(Code, Message ?? string.Format("Arguments {0} Json deserialize error", member)); }
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
    //                return ResultFactory.Create(ProtoBuf.Meta.RuntimeTypeModel.Default.Deserialize(stream, null, type));
    //            }
    //        }
    //        catch { return ResultFactory.Create(Code, Message ?? string.Format("Arguments {0} ProtoBuf deserialize error", member)); }
    //    }
    //}

    public sealed class CanNotNullAttribute : ArgumentAttribute
    {
        public CanNotNullAttribute(int code = -800, string message = null)
            : base(code, message) { }

        public override IResult Proces(dynamic value, System.Type type, string method, string member, dynamic business)
        {
            if (System.Object.Equals(null, value))
            {
                return ResultFactory.Create(Code, Message ?? string.Format("argument \"{0}\" can not null.", member));
            }
            return ResultFactory.Create();
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
            if (System.Object.Equals(null, value)) { return ResultFactory.Create(); }

            var msg = System.String.Empty;

            switch (type.FullName)
            {
                case "System.String":
                    var _ags1 = System.Convert.ToString(value).Trim();
                    if (null != Min && Extensions.Help.ChangeType<System.Int32>(Min) > _ags1.Length)
                    {
                        msg = string.Format("argument \"{0}\" minimum length value {1}.", member, Min);
                    }
                    if (null != Max && Extensions.Help.ChangeType<System.Int32>(Max) < _ags1.Length)
                    {
                        msg = string.Format("argument \"{0}\" maximum length value {1}.", member, Max);
                    }
                    break;
                case "System.DateTime":
                    var _ags2 = System.Convert.ToDateTime(value);
                    if (null != Min && Extensions.Help.ChangeType<System.DateTime>(Min) > _ags2)
                    {
                        msg = string.Format("argument \"{0}\" minimum value {1}.", member, Min);
                    }
                    if (null != Max && Extensions.Help.ChangeType<System.DateTime>(Max) < _ags2)
                    {
                        msg = string.Format("argument \"{0}\" maximum value {1}.", member, Max);
                    }
                    break;
                case "System.Int32":
                    var _ags3 = System.Convert.ToInt32(value);
                    if (null != Min && Extensions.Help.ChangeType<System.Int32>(Min) > _ags3)
                    {
                        msg = string.Format("argument \"{0}\" minimum value {1}.", member, Min);
                    }
                    if (null != Max && Extensions.Help.ChangeType<System.Int32>(Max) < _ags3)
                    {
                        msg = string.Format("argument \"{0}\" maximum value {1}.", member, Max);
                    }
                    break;
                case "System.Int64":
                    var _ags4 = System.Convert.ToInt64(value);
                    if (null != Min && Extensions.Help.ChangeType<System.Int64>(Min) > _ags4)
                    {
                        msg = string.Format("argument \"{0}\" minimum value {1}.", member, Min);
                    }
                    if (null != Max && Extensions.Help.ChangeType<System.Int64>(Max) < _ags4)
                    {
                        msg = string.Format("argument \"{0}\" maximum value {1}.", member, Max);
                    }
                    break;
                case "System.Decimal":
                    var _ags5 = System.Convert.ToDecimal(value);
                    if (null != Min && Extensions.Help.ChangeType<System.Decimal>(Min) > _ags5)
                    {
                        msg = string.Format("argument \"{0}\" minimum value {1}.", member, Min);
                    }
                    if (null != Max && Extensions.Help.ChangeType<System.Decimal>(Max) < _ags5)
                    {
                        msg = string.Format("argument \"{0}\" maximum value {1}.", member, Max);
                    }
                    break;
                case "System.Double":
                    var _ags6 = System.Convert.ToDouble(value);
                    if (null != Min && Extensions.Help.ChangeType<System.Double>(Min) > _ags6)
                    {
                        msg = string.Format("argument \"{0}\" minimum value {1}.", member, Min);
                    }
                    if (null != Max && Extensions.Help.ChangeType<System.Double>(Max) < _ags6)
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
                return ResultFactory.Create(Code, Message ?? msg);
            }

            return ResultFactory.Create();
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
                default: return ResultFactory.Create();
            }

            var t = System.Math.Truncate(value);

            var result = t + (0 > value ? System.Math.Ceiling((value - t) * sp) : System.Math.Floor((value - t) * sp)) / sp;

            return ResultFactory.Create(result);
        }
    }

    public sealed class CheckEmailAttribute : ArgumentAttribute
    {
        public CheckEmailAttribute(int code = -803, string message = null)
            : base(code, message) { }

        public override IResult Proces(dynamic value, System.Type type, string method, string member, dynamic business)
        {
            if (System.Object.Equals(null, value)) { return ResultFactory.Create(); }

            var _value = System.Convert.ToString(value).Trim();
            if (!System.String.IsNullOrEmpty(_value) && !Extensions.Help.CheckEmail(_value))
            {
                return ResultFactory.Create(Code, Message ?? string.Format("argument \"{0}\" email error.", member));
            }
            return ResultFactory.Create();
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
            if (System.Object.Equals(null, value)) { return ResultFactory.Create(); }

            var _value = System.Convert.ToString(value).Trim();
            if (!System.String.IsNullOrEmpty(_value) && !CheckChar(_value, Mode))
            {
                return ResultFactory.Create(Code, Message ?? string.Format("argument \"{0}\" char verification failed.", member));
            }
            return ResultFactory.Create();
        }

        [System.Flags]
        public enum CheckCharMode
        {
            All = 0,
            Number = 2,
            Upper = 4,
            Lower = 8,
            Chinese = 16
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
    //    //    //return ResultFactory.Create<Result>(Mark.MarkItem.Exp_SessionOut);
    //    //    return null;
    //    //}

    //    var session = Extensions.Help.ProtoBufDeserialize(null, sessionType);

    //    return ResultFactory.Create(session);
    //    //return session;
    //    //throw new System.NotImplementedException();
    //}

    #region

    public sealed class JsonCmdAttribute : CommandAttribute
    {
        public JsonCmdAttribute(string onlyName = null, int code = -902, string message = null) : base(onlyName, code, message) { }

        public override IResult Proces(dynamic value, System.Type type, string method, string member, dynamic business)
        {
            try
            {
                return ResultFactory.Create(Newtonsoft.Json.JsonConvert.DeserializeObject(System.Convert.ToString(value), type));
            }
            catch { return ResultFactory.Create(Code, Message ?? string.Format("Arguments {0} Json deserialize error", member)); }
        }
    }

    public sealed class ProtoBufCmdAttribute : CommandAttribute
    {
        public ProtoBufCmdAttribute(string onlyName = null, int code = -903, string message = null) : base(onlyName, code, message) { }

        public override IResult Proces(dynamic value, System.Type type, string method, string member, dynamic business)
        {
            try
            {
                using (var stream = new System.IO.MemoryStream((byte[])value))
                {
                    return ResultFactory.Create(ProtoBuf.Meta.RuntimeTypeModel.Default.Deserialize(stream, null, type));
                }
            }
            catch { return ResultFactory.Create(Code, Message ?? string.Format("Arguments {0} ProtoBuf deserialize error", member)); }
        }
    }

    public sealed class JsonArgAttribute : DeserializeAttribute
    {
        public JsonArgAttribute(int code = -902, string message = null)
            : base(code, message) { }

        public override IResult Proces(dynamic value, System.Type type, string method, string member, dynamic business)
        {
            try
            {
                return ResultFactory.Create(Newtonsoft.Json.JsonConvert.DeserializeObject(value, type));
            }
            catch { return ResultFactory.Create(Code, Message ?? string.Format("Arguments {0} Json deserialize error", member)); }
        }
    }

    public sealed class ProtoBufArgAttribute : DeserializeAttribute
    {
        public ProtoBufArgAttribute(int code = -903, string message = null)
            : base(code, message) { }

        public override IResult Proces(dynamic value, System.Type type, string method, string member, dynamic business)
        {
            try
            {
                using (var stream = new System.IO.MemoryStream(value))
                {
                    return ResultFactory.Create(ProtoBuf.Meta.RuntimeTypeModel.Default.Deserialize(stream, null, type));
                }
            }
            catch { return ResultFactory.Create(Code, Message ?? string.Format("Arguments {0} ProtoBuf deserialize error", member)); }
        }
    }

    #endregion
}
