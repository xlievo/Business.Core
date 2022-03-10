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

namespace Business.Core.Annotations
{
    using Business.Core.Result;
    using Business.Core.Utils;
    using System.Threading.Tasks;

    /// <summary>
    /// CheckNullAttribute
    /// </summary>
    public class CheckNullAttribute : ArgumentAttribute
    {
        /// <summary>
        /// CheckNullAttribute
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        public CheckNullAttribute(int state = -800, string message = null) : base(state, message) => CanNull = true;

        /// <summary>
        /// Compare type defaults
        /// </summary>
        public bool CheckValueType { get; set; }

        /// <summary>
        /// CheckNull
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override ValueTask<IResult> CheckNull(dynamic value) => new ValueTask<IResult>(ResultCreate());

        /// <summary>
        /// Proces
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override ValueTask<IResult> Proces(dynamic value)
        {
            if (typeof(string).Equals(ArgMeta.MemberType))
            {
                if (string.IsNullOrEmpty(value))
                {
                    return new ValueTask<IResult>(ResultCreate(State, Message ?? $"argument \"{Alias}\" can not null."));
                }
            }
            else if (object.Equals(null, value))
            {
                return new ValueTask<IResult>(ResultCreate(State, Message ?? $"argument \"{Alias}\" can not null."));
            }
            else if (CheckValueType)
            {
                var result = CheckDefinitionValueType(value);
                //if (!Equals(null, result) && !result.HasData)
                //{
                //    return new ValueTask<IResult>(result);
                //}
                if (0 >= result.State)
                {
                    return new ValueTask<IResult>(result);
                }
            }

            return new ValueTask<IResult>(ResultCreate());
        }

        /// <summary>
        /// Check whether the defined value type is the default value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public IResult CheckDefinitionValueType(dynamic value)
        {
            //if (ArgMeta.MemberType.Equals(value.GetType()))
            //{
            //    if (!ArgMeta.Arg.Nullable && ArgMeta.MemberType.IsValueType && object.Equals(ArgMeta.Arg.DefaultTypeValue, value))
            //    {
            //        return ResultCreate(State, Message ?? $"argument \"{Alias}\" can not null.");
            //    }

            //    return ResultCreate(data: value);
            //}

            //return null;

            if (!ArgMeta.Arg.Nullable && ArgMeta.MemberType.IsValueType && object.Equals(ArgMeta.Arg.DefaultTypeValue, value))
            {
                return ResultCreate(State, Message ?? $"argument \"{Alias}\" can not null.");
            }

            return ResultCreate(data: value);
        }
    }

    /// <summary>
    /// SizeAttribute
    /// </summary>
    public class SizeAttribute : ArgumentAttribute
    {
        /// <summary>
        /// SizeAttribute
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        public SizeAttribute(int state = -801, string message = null) : base(state, message)
        {
            BindAfter += () =>
            {
                if (!string.IsNullOrWhiteSpace(MinMsg))
                {
                    MinMsg = Replace(MinMsg);
                }

                if (!string.IsNullOrWhiteSpace(MaxMsg))
                {
                    MaxMsg = Replace(MaxMsg);
                }
            };
        }

        /// <summary>
        /// Min
        /// </summary>
        public object Min { get; set; }
        /// <summary>
        /// Max
        /// </summary>
        public object Max { get; set; }

        /// <summary>
        /// MinMsg
        /// </summary>
        public string MinMsg { get; set; } = "argument \"{Alias}\" minimum range {Min}.";
        /// <summary>
        /// MaxMsg
        /// </summary>
        public string MaxMsg { get; set; } = "argument \"{Alias}\" maximum range {Max}.";

        /// <summary>
        /// Proces
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override ValueTask<IResult> Proces(dynamic value)
        {
            var type = ArgMeta.MemberType;
            //var type = System.Nullable.GetUnderlyingType(this.ArgMeta.MemberType) ?? this.ArgMeta.MemberType;

            string msg = null;

            switch (type.FullName)
            {
                case "System.String":
                    {
                        var ags = System.Convert.ToString(value).Trim();
                        if (null != Min && Help.ChangeType<int>(Min) > ags.Length)
                        {
                            msg = MinMsg; break;
                        }
                        if (null != Max && Help.ChangeType<int>(Max) < ags.Length)
                        {
                            msg = MaxMsg; break;
                        }
                    }
                    break;
                case "System.DateTime":
                    {
                        var ags = System.Convert.ToDateTime(value);
                        if (null != Min && Help.ChangeType<System.DateTime>(Min) > ags)
                        {
                            msg = MinMsg; break;
                        }
                        if (null != Max && Help.ChangeType<System.DateTime>(Max) < ags)
                        {
                            msg = MaxMsg; break;
                        }
                    }
                    break;
                case "System.Int32":
                    {
                        var ags = System.Convert.ToInt32(value);
                        if (null != Min && Help.ChangeType<int>(Min) > ags)
                        {
                            msg = MinMsg; break;
                        }
                        if (null != Max && Help.ChangeType<int>(Max) < ags)
                        {
                            msg = MaxMsg; break;
                        }
                    }
                    break;
                case "System.Int64":
                    {
                        var ags = System.Convert.ToInt64(value);
                        if (null != Min && Help.ChangeType<long>(Min) > ags)
                        {
                            msg = MinMsg; break;
                        }
                        if (null != Max && Help.ChangeType<long>(Max) < ags)
                        {
                            msg = MaxMsg; break;
                        }
                    }
                    break;
                case "System.Decimal":
                    {
                        var ags = System.Convert.ToDecimal(value);
                        if (null != Min && Help.ChangeType<decimal>(Min) > ags)
                        {
                            msg = MinMsg; break;
                        }
                        if (null != Max && Help.ChangeType<decimal>(Max) < ags)
                        {
                            msg = MaxMsg; break;
                        }
                    }
                    break;
                case "System.Double":
                    {
                        var ags = System.Convert.ToDouble(value);
                        if (null != Min && Help.ChangeType<double>(Min) > ags)
                        {
                            msg = MinMsg; break;
                        }
                        if (null != Max && Help.ChangeType<double>(Max) < ags)
                        {
                            msg = MaxMsg; break;
                        }
                    }
                    break;
                default:
                    if (type.IsCollection())
                    {
                        var list = value as System.Collections.ICollection;
                        if (null != Min && Help.ChangeType<int>(Min) > list.Count)
                        {
                            msg = MinMsg; break;
                        }
                        if (null != Max && Help.ChangeType<int>(Max) < list.Count)
                        {
                            msg = MaxMsg; break;
                        }
                    }
                    else
                    {
                        return new ValueTask<IResult>(ResultCreate(State, $"argument \"{Alias}\" type error"));
                    }
                    break;
            }

            //checked error
            if (!string.IsNullOrEmpty(msg))
            {
                return new ValueTask<IResult>(ResultCreate(State, Message ?? msg));
            }

            return new ValueTask<IResult>(ResultCreate());
        }
    }

    /// <summary>
    /// ScaleAttribute
    /// </summary>
    public class ScaleAttribute : ArgumentAttribute
    {
        /// <summary>
        /// ScaleAttribute
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        public ScaleAttribute(int state = -802, string message = null) : base(state, message) { }

        /// <summary>
        /// Size
        /// </summary>
        public int Size { get; set; } = 2;

        /// <summary>
        /// Proces
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override ValueTask<IResult> Proces(dynamic value)
        {
            switch (ArgMeta.MemberType.GetTypeCode())
            {
                case System.TypeCode.Decimal:
                    return new ValueTask<IResult>(ResultCreate(Help.Scale((decimal)value, Size)));
                case System.TypeCode.Double:
                    return new ValueTask<IResult>(ResultCreate(Help.Scale((double)value, Size)));
                default: return new ValueTask<IResult>(ResultCreate(State, Message ?? $"argument \"{Alias}\" type error"));
            }
        }
    }

    /// <summary>
    /// https://github.com/Microsoft/referencesource/blob/master/System.ComponentModel.DataAnnotations/DataAnnotations/EmailAddressAttribute.cs
    /// </summary>
    public class CheckEmailAttribute : ArgumentAttribute
    {
        /// <summary>
        /// CheckEmailAttribute
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        public CheckEmailAttribute(int state = -803, string message = null) : base(state, message) { }

        /// <summary>
        /// Proces
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override ValueTask<IResult> Proces(dynamic value)
        {
            //if (!Utils.Help.CheckEmail(value))
            if (!IsValid(value))
            {
                return new ValueTask<IResult>(ResultCreate(State, Message ?? $"argument \"{Alias}\" email error"));
            }
            return new ValueTask<IResult>(ResultCreate());
        }

        bool IsValid(dynamic value)
        {
            if (value == null)
            {
                return true;
            }

            string valueAsString = value as string;

            // Use RegEx implementation if it has been created, otherwise use a non RegEx version.
            return valueAsString != null && _regex.Match(valueAsString).Length > 0;
        }

        // This attribute provides server-side email validation equivalent to jquery validate,
        // and therefore shares the same regular expression.  See unit tests for examples.
        private static System.Text.RegularExpressions.Regex _regex = CreateRegEx();

        private static System.Text.RegularExpressions.Regex CreateRegEx()
        {
            const string pattern = @"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$";
            const System.Text.RegularExpressions.RegexOptions options = System.Text.RegularExpressions.RegexOptions.Compiled | System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.ExplicitCapture;

            // Set explicit regex match timeout, sufficient enough for email parsing
            // Unless the global REGEX_DEFAULT_MATCH_TIMEOUT is already set
            var matchTimeout = System.TimeSpan.FromSeconds(2);

            try
            {
                if (System.AppDomain.CurrentDomain.GetData("REGEX_DEFAULT_MATCH_TIMEOUT") == null)
                {
                    return new System.Text.RegularExpressions.Regex(pattern, options, matchTimeout);
                }
            }
            catch
            {
                // Fallback on error
            }

            // Legacy fallback (without explicit match timeout)
            return new System.Text.RegularExpressions.Regex(pattern, options);
        }
    }

    /// <summary>
    /// CheckCharAttribute
    /// </summary>
    public class CheckCharAttribute : ArgumentAttribute
    {
        /// <summary>
        /// CheckCharAttribute
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        public CheckCharAttribute(int state = -804, string message = null) : base(state, message) { }

        /// <summary>
        /// Mode
        /// </summary>
        public Help.CheckCharMode Mode { get; set; } = Help.CheckCharMode.All;

        /// <summary>
        /// Proces
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override ValueTask<IResult> Proces(dynamic value)
        {
            if (!Utils.Help.CheckChar(value, Mode))
            {
                return new ValueTask<IResult>(ResultCreate(State, Message ?? $"argument \"{Alias}\" char verification failed"));
            }
            return new ValueTask<IResult>(ResultCreate());
        }
    }

    /// <summary>
    /// Indicates whether the specified regular expression finds a match in the specified input string, using the specified matching options.
    /// https://github.com/microsoft/referencesource/blob/master/System.ComponentModel.DataAnnotations/DataAnnotations/RegularExpressionAttribute.cs
    /// </summary>
    public class RegexAttribute : ArgumentAttribute
    {
        /// <summary>
        /// Indicates whether the specified regular expression finds a match in the specified input string, using the specified matching options.
        /// </summary>
        /// <param name="pattern">The regular expression pattern to match.</param>
        /// <param name="state"></param>
        /// <param name="message"></param>
        public RegexAttribute(string pattern, int state = -805, string message = null) : base(state, message)
        {
            Pattern = pattern;
            //this.Options = options;
        }

        /// <summary>
        /// A bitwise combination of the enumeration values that provide options for matching.
        /// </summary>
        //public System.Text.RegularExpressions.RegexOptions Options { get; set; } = System.Text.RegularExpressions.RegexOptions.None;

        public override ValueTask<IResult> Proces(dynamic value)
        {
            //if (!System.Text.RegularExpressions.Regex.IsMatch(value.Trim(), Pattern, Options))
            if (!IsValid(value))
            {
                return new ValueTask<IResult>(ResultCreate(State, Message ?? $"argument \"{Alias}\" regular expression verification failed"));
            }

            return new ValueTask<IResult>(ResultCreate());
        }

        bool IsValid(object value)
        {
            SetupRegex();

            // Convert the value to a string
            string stringValue = System.Convert.ToString(value, System.Globalization.CultureInfo.CurrentCulture);

            // Automatically pass if value is null or empty. RequiredAttribute should be used to assert a value is not empty.
            if (string.IsNullOrEmpty(stringValue))
            {
                return true;
            }

            var m = Regex.Match(stringValue);

            // We are looking for an exact match, not just a search hit. This matches what
            // the RegularExpressionValidator control does
            return (m.Success && m.Index == 0 && m.Length == stringValue.Length);
        }

        /// <summary>
        /// Gets the regular expression pattern to use
        /// </summary>
        public string Pattern { get; private set; }

        /// <summary>
        ///     Gets or sets the timeout to use when matching the regular expression pattern (in milliseconds)
        ///     (-1 means never timeout).
        /// </summary>
        public int MatchTimeoutInMilliseconds
        {
            get
            {
                return _matchTimeoutInMilliseconds;
            }
            set
            {
                _matchTimeoutInMilliseconds = value;
                _matchTimeoutSet = true;
            }
        }

        private int _matchTimeoutInMilliseconds;
        private bool _matchTimeoutSet;

        private System.Text.RegularExpressions.Regex Regex { get; set; }

        /// <summary>
        /// Sets up the <see cref="Regex"/> property from the <see cref="Pattern"/> property.
        /// </summary>
        private void SetupRegex()
        {
            if (Regex == null)
            {
                if (string.IsNullOrEmpty(Pattern))
                {
                    throw new System.ArgumentException(Pattern);
                }

                if (!_matchTimeoutSet)
                {
                    MatchTimeoutInMilliseconds = GetDefaultTimeout();
                }

                Regex = MatchTimeoutInMilliseconds == -1
                    ? new System.Text.RegularExpressions.Regex(Pattern)
                    : Regex = new System.Text.RegularExpressions.Regex(Pattern, default(System.Text.RegularExpressions.RegexOptions), System.TimeSpan.FromMilliseconds(MatchTimeoutInMilliseconds));
            }
        }

        /// <summary>
        /// Returns the default MatchTimeout based on UseLegacyRegExTimeout switch.
        /// </summary>
        private static int GetDefaultTimeout()
        {
            return 2000;
        }
    }

    /// <summary>
    /// https://github.com/microsoft/referencesource/blob/master/System.ComponentModel.DataAnnotations/DataAnnotations/PhoneAttribute.cs
    /// </summary>
    public class CheckPhoneAttribute : ArgumentAttribute
    {
        /// <summary>
        /// CheckPhoneAttribute
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        public CheckPhoneAttribute(int state = -806, string message = null) : base(state, message) { }

        /// <summary>
        /// Proces
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override ValueTask<IResult> Proces(dynamic value)
        {
            if (!IsValid(value))
            {
                return new ValueTask<IResult>(ResultCreate(State, Message ?? $"argument \"{Alias}\" phone error"));
            }
            return new ValueTask<IResult>(ResultCreate());
        }

        bool IsValid(dynamic value)
        {
            if (value == null)
            {
                return true;
            }

            string valueAsString = value as string;

            // Use RegEx implementation if it has been created, otherwise use a non RegEx version.
            return valueAsString != null && _regex.Match(valueAsString).Length > 0;
        }

        private static System.Text.RegularExpressions.Regex _regex = CreateRegEx();

        private static System.Text.RegularExpressions.Regex CreateRegEx()
        {
            const string pattern = @"^(\+\s?)?((?<!\+.*)\(\+?\d+([\s\-\.]?\d+)?\)|\d+)([\s\-\.]?(\(\d+([\s\-\.]?\d+)?\)|\d+))*(\s?(x|ext\.?)\s?\d+)?$";
            const System.Text.RegularExpressions.RegexOptions options = System.Text.RegularExpressions.RegexOptions.Compiled | System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.ExplicitCapture;

            // Set explicit regex match timeout, sufficient enough for phone parsing
            // Unless the global REGEX_DEFAULT_MATCH_TIMEOUT is already set
            var matchTimeout = System.TimeSpan.FromSeconds(2);

            try
            {
                if (System.AppDomain.CurrentDomain.GetData("REGEX_DEFAULT_MATCH_TIMEOUT") == null)
                {
                    return new System.Text.RegularExpressions.Regex(pattern, options, matchTimeout);
                }
            }
            catch
            {
                // Fallback on error
            }

            // Legacy fallback (without explicit match timeout)
            return new System.Text.RegularExpressions.Regex(pattern, options);
        }
    }

    /// <summary>
    /// https://github.com/microsoft/referencesource/blob/master/System.ComponentModel.DataAnnotations/DataAnnotations/UrlAttribute.cs
    /// https://stackoverflow.com/questions/45707293/url-validation-attribute-marks-localhost-as-invalid-url
    /// </summary>
    public class CheckUrlAttribute : ArgumentAttribute
    {
        /// <summary>
        /// CheckUrlAttribute
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        public CheckUrlAttribute(int state = -807, string message = null) : base(state, message) { }

        /// <summary>
        /// Proces
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override ValueTask<IResult> Proces(dynamic value)
        {
            if (!IsValid(value))
            {
                return new ValueTask<IResult>(ResultCreate(State, Message ?? $"argument \"{Alias}\" url error"));
            }
            return new ValueTask<IResult>(ResultCreate());
        }

        bool IsValid(object value)
        {
            if (value == null)
            {
                return true;
            }

            string valueAsString = value as string;

            // Use RegEx implementation if it has been created, otherwise use a non RegEx version.
            return valueAsString != null && _regex.Match(valueAsString).Length > 0;
        }

        // This attribute provides server-side url validation equivalent to jquery validate,
        // and therefore shares the same regular expression.  See unit tests for examples.
        private static System.Text.RegularExpressions.Regex _regex = CreateRegEx();

        private static System.Text.RegularExpressions.Regex CreateRegEx()
        {
            const string pattern = @"^(https?|ftp):\/\/(((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:)*@)?(((\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5]))|((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?)(:\d*)?)(\/((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)+(\/(([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)*)*)?)?(\?((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)|[\uE000-\uF8FF]|\/|\?)*)?(\#((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)|\/|\?)*)?$";

            const System.Text.RegularExpressions.RegexOptions options = System.Text.RegularExpressions.RegexOptions.Compiled | System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.ExplicitCapture;

            // Set explicit regex match timeout, sufficient enough for url parsing
            // Unless the global REGEX_DEFAULT_MATCH_TIMEOUT is already set
            var matchTimeout = System.TimeSpan.FromSeconds(2);

            try
            {
                if (System.AppDomain.CurrentDomain.GetData("REGEX_DEFAULT_MATCH_TIMEOUT") == null)
                {
                    return new System.Text.RegularExpressions.Regex(pattern, options, matchTimeout);
                }
            }
            catch
            {
                // Fallback on error
            }

            // Legacy fallback (without explicit match timeout)
            return new System.Text.RegularExpressions.Regex(pattern, options);
        }
    }

    /// <summary>
    /// Check whether the enumeration value is correct
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Enum | System.AttributeTargets.Parameter, AllowMultiple = true, Inherited = true)]
    public class EnumCheckAttribute : ArgumentAttribute
    {
        /// <summary>
        /// Check whether the enumeration value is correct
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        public EnumCheckAttribute(int state = 808, string message = null) : base(state, message) { }

        /// <summary>
        /// Check whether the enumeration value is correct
        /// </summary>
        /// <typeparam name="Type"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public override ValueTask<IResult> Proces<Type>(dynamic value)
        {
            System.Enum value2 = value;

            if (ArgMeta.Arg.Nullable && Equals(null, value2))
            {
                return new ValueTask<IResult>(ResultCreate(value2));
            }

            if (null != value2.GetName())
            {
                return new ValueTask<IResult>(ResultCreate(value2));
            }

            return new ValueTask<IResult>(ResultCreate(State, Message ?? $"{Alias} Enum value error."));
        }
    }

    /// <summary>
    /// MD5Attribute
    /// </summary>
    public class MD5Attribute : ArgumentAttribute
    {
        /// <summary>
        /// MD5Attribute
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        public MD5Attribute(int state = -820, string message = null) : base(state, message) { }

        /// <summary>
        /// Encoding
        /// </summary>
        public System.Text.Encoding Encoding { get; set; }

        /// <summary>
        /// HasUpper
        /// </summary>
        public bool HasUpper { get; set; }

        /// <summary>
        /// Proces
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override ValueTask<IResult> Proces(dynamic value)
        {
            return new ValueTask<IResult>(this.ResultCreate(Help.MD5(value, HasUpper, Encoding)));
        }
    }

    /// <summary>
    /// AES return to item1=Data and item2=Salt
    /// </summary>
    public class AESAttribute : ArgumentAttribute
    {
        /// <summary>
        /// key
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// salt
        /// </summary>
        public string Salt { get; set; }

        /// <summary>
        /// Encoding
        /// </summary>
        public System.Text.Encoding Encoding { get; set; }

        /// <summary>
        /// AES return to item1=Data and item2=Salt
        /// </summary>
        /// <param name="key"></param>
        /// <param name="state"></param>
        /// <param name="message"></param>
        public AESAttribute(string key = null, int state = -821, string message = null) : base(state, message) => Key = key;

        /// <summary>
        /// Proces
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override ValueTask<IResult> Proces(dynamic value)
        {
            return new ValueTask<IResult>(this.ResultCreate(Help.AES.Encrypt(value, Key, Salt, Encoding)));
        }

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="password"></param>
        /// <param name="encryptData"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        public bool Equals(string password, string encryptData, string salt)
        {
            try
            {
                return Equals(password, Help.AES.Decrypt(encryptData, Key, salt, Encoding));
            }
            catch
            {
                return false;
            }
        }
    }

    #region Deserialize

    ///// <summary>
    ///// ArgumentDefaultAttribute
    ///// </summary>
    //[System.AttributeUsage(System.AttributeTargets.All, AllowMultiple = false)]
    //public sealed class ArgumentDefaultAttribute : ArgumentAttribute
    //{
    //    /// <summary>
    //    /// ArgumentDefaultAttribute
    //    /// </summary>
    //    /// <param name="resultType"></param>
    //    /// <param name="resultTypeDefinition"></param>
    //    /// <param name="state"></param>
    //    /// <param name="message"></param>
    //    internal ArgumentDefaultAttribute(System.Type resultType, System.Type resultTypeDefinition, int state = -11, string message = null) : base(state, message)
    //    {
    //        ArgMeta.resultType = resultType;
    //        ArgMeta.resultTypeDefinition = resultTypeDefinition;
    //        //ArgMeta.argTypeDefinition = argTypeDefinition;
    //        CanNull = true;
    //    }

    //    /// <summary>
    //    /// ArgumentDefaultAttribute
    //    /// </summary>
    //    /// <param name="state"></param>
    //    /// <param name="message"></param>
    //    public ArgumentDefaultAttribute(int state = -10, string message = null) : base(state, message) { }
    //}

    /// <summary>
    /// ParametersAttribute
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Parameter, AllowMultiple = true, Inherited = true)]
    public class ParametersAttribute : ArgumentDeserialize
    {
        /// <summary>
        /// ParametersAttribute
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        public ParametersAttribute(int state = -11, string message = null) : base(state, message)
        {
            //this.CanNull = false;
            Description = "Parameters parsing";
            ArgMeta.Skip = (bool hasUse, bool hasDefinition, AttributeBase.MetaData.DeclaringType declaring, System.Collections.Generic.IEnumerable<ArgumentAttribute> arguments, bool ignoreArg, bool dynamicObject) => !hasDefinition;
        }

        /// <summary>
        /// Proces
        /// </summary>
        /// <typeparam name="Type"></typeparam>
        /// <param name="value"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public override async ValueTask<IResult> Proces<Type>(dynamic token, dynamic value)
        {
            try
            {
                System.Collections.Generic.IDictionary<string, string> dict = value;

                if (0 < dict.Count && 0 < ArgMeta.Arg.Children?.Count)
                {
                    var arg = System.Activator.CreateInstance<Type>();

                    foreach (var item in ArgMeta.Arg.Children)
                    {
                        if (dict.TryGetValue(item.Name, out string v))
                        {
                            var deserialize = item.Group[this.ArgMeta.Key].Deserialize;

                            if (null == deserialize)
                            {
                                var v2 = Help.ChangeType(v, item.Type);
                                item.Accessor.Setter(arg, v2);
                            }
                            else
                            {
                                var v2 = await deserialize.GetProcesResult(v, token);
                                if (0 >= v2.State)
                                {
                                    return v2;
                                }
                                if (v2.HasData)
                                {
                                    item.Accessor.Setter(arg, v2.Data);
                                }
                            }
                        }
                    }

                    return ResultCreate(arg);
                }

                return ResultCreate<Type>(default);
            }
            catch (System.Exception ex) { return ResultCreate(State, Message ?? $"{Alias} Proces error. {ex.Message}"); }
        }
    }

    /// <summary>
    /// System.Text.Json.JsonSerializer.Deserialize
    /// </summary>
    //[System.AttributeUsage(System.AttributeTargets.Assembly | System.AttributeTargets.Method | System.AttributeTargets.Class | System.AttributeTargets.Struct | System.AttributeTargets.Property | System.AttributeTargets.Field | System.AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class JsonArgAttribute : ArgumentDeserialize
    {
        /// <summary>
        /// JsonArgAttribute
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        /// <param name="type"></param>
        public JsonArgAttribute(int state = -12, string message = null, System.Type type = null) : base(state, message, type)
        {
            //this.CanNull = false;
            Description = "Json parsing";
            //this.ArgMeta.Filter |= FilterModel.NotDefinition;
            //this.ArgMeta.Skip = (bool hasUse, bool hasDefinition, AttributeBase.MetaData.DeclaringType declaring, System.Collections.Generic.IEnumerable<ArgumentAttribute> arguments, bool ignoreArg, bool dynamicObject) => (!hasDefinition && !this.ArgMeta.Arg.HasCollection && !dynamicObject) || this.ArgMeta.Arg.Parameters || ignoreArg;
            //this.ArgMeta.Deserialize = true;

            textJsonOptions = new System.Text.Json.JsonSerializerOptions
            {
                IncludeFields = true,
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                IgnoreNullValues = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString | System.Text.Json.Serialization.JsonNumberHandling.WriteAsString
            };
            textJsonOptions.Converters.Add(new Help.DateTimeConverter());
            textJsonOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        }

        /// <summary>
        /// Options to control the behavior during parsing.
        /// </summary>
        public System.Text.Json.JsonSerializerOptions textJsonOptions;

        ///// <summary>
        ///// Check whether the defined value type is the default value, (top-level object commit), Default true
        ///// </summary>
        //public bool CheckValueType { get; set; } = true;

        /// <summary>
        /// Proces
        /// </summary>
        /// <typeparam name="Type"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public override ValueTask<IResult> Proces<Type>(dynamic value)
        {
            //Check whether the defined value type is the default value, (top-level object commit)
            //result = CheckDefinitionValueType(this, value, CheckValueType);
            //if (!Equals(null, result)) { return new ValueTask<IResult>(result); }

            try
            {
                return new ValueTask<IResult>(this.ResultCreate(System.Text.Json.JsonSerializer.Deserialize<Type>(value, textJsonOptions)));
            }
            catch (System.Exception ex) { return new ValueTask<IResult>(ResultCreate(State, Message ?? $"{Alias} Json deserialize error. {ex.Message}")); }
        }
    }

    /// <summary>
    /// XML.Deserialize
    /// </summary>
    public class XmlArgAttribute : ArgumentDeserialize
    {
        /// <summary>
        /// XmlArgAttribute
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        /// <param name="rootElementName">Controls XML serialization of the attribute target as an XML root element.</param>
        /// <param name="type"></param>
        public XmlArgAttribute(int state = -13, string message = null, string rootElementName = "xml", System.Type type = null) : base(state, message, type)
        {
            //this.CanNull = false;
            Description = "Xml parsing";
            RootElementName = rootElementName;
            //this.ArgMeta.Skip = (bool hasUse, bool hasDefinition, AttributeBase.MetaData.DeclaringType declaring, System.Collections.Generic.IEnumerable<ArgumentAttribute> arguments, bool ignoreArg, bool dynamicObject) => (!hasDefinition && !this.ArgMeta.Arg.HasCollection && !dynamicObject) || this.ArgMeta.Arg.Parameters || ignoreArg;
        }

        /// <summary>
        /// Controls XML serialization of the attribute target as an XML root element.
        /// </summary>
        public string RootElementName { get; set; }

        /// <summary>
        /// Check whether the defined value type is the default value, (top-level object commit), Default true
        /// </summary>
        public bool CheckValueType { get; set; } = true;

        /// <summary>
        /// Proces
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override ValueTask<IResult> Proces(dynamic value)
        {
            //Check whether the defined value type is the default value, (top-level object commit)
            //result = CheckDefinitionValueType(this, value, CheckValueType);
            //if (!Equals(null, result)) { return new ValueTask<IResult>(result); }

            try
            {
                using (var reader = new System.IO.StringReader(value))
                {
                    var xmlSerializer = new System.Xml.Serialization.XmlSerializer(ArgMeta.MemberType, new System.Xml.Serialization.XmlRootAttribute(RootElementName));
                    return new ValueTask<IResult>(ResultCreate(xmlSerializer.Deserialize(reader)));
                }
            }
            catch (System.Exception ex) { return new ValueTask<IResult>(ResultCreate(State, Message ?? $"{Alias} Xml deserialize error. {ex.Message}")); }
        }
    }

    /// <summary>
    /// Simple asp.net HTTP request file attribute
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method | System.AttributeTargets.Class | System.AttributeTargets.Struct | System.AttributeTargets.Property | System.AttributeTargets.Field | System.AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public abstract class HttpFileAttribute : ArgumentAttribute
    {
        /// <summary>
        /// Simple asp.net HTTP request file attribute
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        public HttpFileAttribute(int state = 830, string message = null) : base(state, message) { }
    }
    /*
    public class ProtoBufArgAttribute : ArgumentAttribute
    {
        public ProtoBufArgAttribute(int state = -13, string message = null, bool canNull = false)
            : base(state, message, canNull) { }

        public override async ValueTask<IResult> Proces(dynamic value)
        {
            var result = CheckNull(this, value);
            if (!result.HasData) { return result; }

            try
            {
                using (var stream = new System.IO.MemoryStream(value))
                {
                    return this.ResultCreate(ProtoBuf.Meta.RuntimeTypeModel.Default.Deserialize(stream, null, this.MemberType));
                }
            }
            catch { return this.ResultCreate(State, Message ?? string.Format("Arguments {0} ProtoBuf deserialize error", this.Member)); }
        }
    }
    */
    #endregion
}
