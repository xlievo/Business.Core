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

namespace Business.Extensions
{
    public static class Help
    {
        ///// <summary>
        /////   Gets the attributes.
        ///// </summary>
        ///// <param name = "member">The member.</param>
        ///// <returns>The member attributes.</returns>
        //public static T[] GetAttributes<T>(this System.Reflection.ICustomAttributeProvider member, bool inherit = true) where T : class
        //{
        //    if (null == member) { throw new System.ArgumentNullException("member"); }

        //    if (typeof(T) != typeof(object))
        //    {
        //        return (T[])member.GetCustomAttributes(typeof(T), inherit);
        //    }
        //    return (T[])member.GetCustomAttributes(inherit);
        //}

        public static T[] GetAttributes<T>(this System.Reflection.MemberInfo member, bool inherit = true) where T : System.Attribute
        {
            if (null == member) { throw new System.ArgumentNullException("member"); }

            //foreach (var item in System.Attribute.GetCustomAttributes(member, typeof(T), inherit))
            //{
            //    System.Console.WriteLine(item.GetType().FullName);
            //}

            return (T[])System.Attribute.GetCustomAttributes(member, typeof(T), inherit);
        }

        public static T[] GetAttributes<T>(this System.Reflection.ParameterInfo member, bool inherit = true) where T : System.Attribute
        {
            if (null == member) { throw new System.ArgumentNullException("member"); }

            return (T[])System.Attribute.GetCustomAttributes(member, typeof(T), inherit);
        }

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
                    var interfaces = fromType.GetInterfaces();
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
                    genericArguments = fromType.GetGenericArguments();
                    if (genericArguments.Length == type.GetGenericArguments().Length)
                    {
                        try
                        {
                            var closedType = type.MakeGenericType(genericArguments);
                            if (closedType.IsAssignableFrom(fromType))
                            {
                                return true;
                            }
                        }
                        catch (System.ArgumentException)
                        {

                        }
                    }
                }
                fromType = fromType.BaseType;
            }
            genericArguments = null;
            return false;
        }

        public static System.IO.MemoryStream StreamCopy(this System.IO.Stream stream)
        {
            if (null == stream) { throw new System.ArgumentNullException("stream"); }

            var outStream = new System.IO.MemoryStream();
            const int bufferLen = 4096;//4k size
            var buffer = new byte[bufferLen];
            int count = 0;
            while ((count = stream.Read(buffer, 0, bufferLen)) > 0) { outStream.Write(buffer, 0, count); }
            return outStream;
        }
        /// <summary>
        /// gzip to byte[]
        /// </summary>
        /// <param name="value">byte[]</param>
        /// <returns>byte[]</returns>
        public static System.Byte[] GZipCompressByte(this System.Byte[] value)
        {
            if (null == value) { throw new System.ArgumentNullException("value"); }

            using (var m = new System.IO.MemoryStream())
            {
                //序列化压缩时要在返回字节数组时必须先关闭压缩对象，不然小于4k大小的字节数组会解压不出来就会产生空置的问题
                using (var g = new System.IO.Compression.GZipStream(m, System.IO.Compression.CompressionMode.Compress)) { g.Write(value, 0, value.Length); }
                return m.GetBuffer();
            }
        }
        /// <summary>
        /// gzip to byte[]
        /// </summary>
        /// <param name="value">byte[]</param>
        /// <returns>byte[]</returns>
        public static System.Byte[] GZipDecompressByte(this System.Byte[] value)
        {
            if (null == value) { throw new System.ArgumentNullException("value"); }

            using (var m = GZipDecompressStream(value)) { return m.ToArray(); }
        }
        /// <summary>
        /// gzip to byte[]
        /// </summary>
        /// <param name="value">byte[]</param>
        /// <returns>MemoryStream</returns>
        public static System.IO.MemoryStream GZipDecompressStream(this System.Byte[] value)
        {
            if (null == value) { throw new System.ArgumentNullException("value"); }

            using (var m = new System.IO.MemoryStream(value))
            {
                m.Seek(0, System.IO.SeekOrigin.Begin);
                using (var g = new System.IO.Compression.GZipStream(m, System.IO.Compression.CompressionMode.Decompress, true))
                {
                    return StreamCopy(g);
                }
            }
        }

        public static string MD5Encoding(string value, string encodingNmae = "UTF-8", bool hasUpper = false)
        {
            if (null == value) { throw new System.ArgumentNullException("value"); }

            using (var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider())
            {
                var result = System.BitConverter.ToString(md5.ComputeHash(System.Text.Encoding.GetEncoding(encodingNmae).GetBytes(value))).Replace("-", System.String.Empty);
                return hasUpper ? result.ToUpperInvariant() : result.ToLowerInvariant();
            }
        }

        [System.Flags]
        public enum CheckCharMode
        {
            /// <summary>
            /// Allow all
            /// </summary>
            All = 0,
            /// <summary>
            /// Allow number
            /// </summary>
            Number = 2,
            /// <summary>
            /// Allow upper
            /// </summary>
            Upper = 4,
            /// <summary>
            /// Allow lower
            /// </summary>
            Lower = 8,
            /// <summary>
            /// Allow chinese
            /// </summary>
            Chinese = 16
        }

        public static bool CheckChar(string value, CheckCharMode mode = CheckCharMode.All)
        {
            if (System.String.IsNullOrWhiteSpace(value)) { return false; }

            var _value = value.Trim();
            var list = new System.Collections.Generic.List<int>();
            for (int i = 0; i < _value.Length; i++) { list.Add(_value[i]); }

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

        #region Guid

        public static string NewGuidNumber()
        {
            return System.BitConverter.ToUInt32(System.Guid.NewGuid().ToByteArray(), 0).ToString();
        }
        public static long GuidNumber()
        {
            return (long)System.BitConverter.ToUInt64(System.Guid.NewGuid().ToByteArray(), 0);
        }
        public static string GuidString()
        {
            long i = 1;

            foreach (byte b in System.Guid.NewGuid().ToByteArray())
            {
                i *= ((int)b + 1);
            }

            return string.Format("{0:x}", i - System.DateTime.Now.Ticks);
        }

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
            };
        }

        public static string GetMethodFullName(this System.Reflection.MethodInfo methodInfo)
        {
            if (null == methodInfo) { throw new System.ArgumentNullException("methodInfo"); }

            return string.Format("{0}.{1}", methodInfo.DeclaringType.FullName, methodInfo.Name);
        }

        public static Type ChangeType<Type>(this object value)
        {
            return (Type)ChangeType(value, typeof(Type));
        }

        public static object ChangeType(this object value, System.Type type)
        {
            if (null == value) { return System.Activator.CreateInstance(type); }

            try
            {
                return System.Convert.ChangeType(value, type);
            }
            catch { return System.Activator.CreateInstance(type); }
        }

        public static int Random(int minValue, int maxValue)
        {
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                return new System.Random(System.BitConverter.ToInt32(bytes, 0)).Next(minValue, maxValue);
            }
        }
        public static int Random(this int maxValue)
        {
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                return new System.Random(System.BitConverter.ToInt32(bytes, 0)).Next(maxValue);
            }
        }
        public static double Random()
        {
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                return new System.Random(System.BitConverter.ToInt32(bytes, 0)).NextDouble();
            }
        }

        public static bool CheckEmail(this string email)
        {
            if (System.String.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            return System.Text.RegularExpressions.Regex.IsMatch(email.Trim(), @"^([0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9})$");
        }

        public static double Scale(this double value, int size = 2)
        {
            //var sp = System.Convert.ToDouble(System.Math.Pow(10, size));
            //var t = System.Math.Truncate(value);

            //var result = t + (0 > value ? System.Math.Ceiling((value - t) * sp) : System.Math.Floor((value - t) * sp)) / sp;

            //return result;

            var p = System.Math.Pow(10, size);
            return (double)((int)(value * (int)p) / p);

            //return System.Convert.ToDouble(value.ToString("N", new System.Globalization.NumberFormatInfo { NumberDecimalDigits = size }));
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

        public static string GetName(this System.Enum value)
        {
            return System.Enum.GetName(value.GetType(), value);
        }

        #region Json

        public static Type TryJsonDeserialize<Type>(this string value)
        {
            try { return Newtonsoft.Json.JsonConvert.DeserializeObject<Type>(value); }
            catch { return default(Type); }
        }

        public static Type TryJsonDeserialize<Type>(this string value, out string error)
        {
            error = null;

            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<Type>(value);
            }
            catch (System.Exception ex)
            {
                error = System.Convert.ToString(ex);
                return default(Type);
            }
        }
        public static object TryJsonDeserialize(this string value, System.Type type, out string error)
        {
            error = null;

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
        public static string JsonSerialize<Type>(this Type value)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(value);
        }

        #endregion

        #region ProtoBuf Serialize
        public static Type TryProtoBufDeserialize<Type>(this System.Byte[] source)
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
        public static System.Byte[] ProtoBufSerialize<Type>(this Type instance)
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

        #endregion
    }

    #region ICloneable

    /// <summary>  
    /// Interface definition for cloneable objects.  
    /// </summary>  
    /// <typeparam name="T">Type of the cloneable objects.</typeparam>  
    public interface ICloneable<T> : System.ICloneable
        where T : ICloneable<T>
    {
        /// <summary>  
        /// Clones this instance.  
        /// </summary>  
        /// <returns>The cloned instance.</returns>  
        new T Clone();
    }

    #endregion

    #region Equals

    public static class Equality<T>
    {
        public static System.Collections.Generic.IEqualityComparer<T> CreateComparer<V>(System.Func<T, V> keySelector)
        {
            return new CommonEqualityComparer<V>(keySelector);
        }
        public static System.Collections.Generic.IEqualityComparer<T> CreateComparer<V>(System.Func<T, V> keySelector, System.Collections.Generic.IEqualityComparer<V> comparer)
        {
            return new CommonEqualityComparer<V>(keySelector, comparer);
        }

        class CommonEqualityComparer<V> : System.Collections.Generic.IEqualityComparer<T>
        {
            private System.Func<T, V> keySelector;
            private System.Collections.Generic.IEqualityComparer<V> comparer;

            public CommonEqualityComparer(System.Func<T, V> keySelector, System.Collections.Generic.IEqualityComparer<V> comparer)
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
        public static System.Collections.Generic.IComparer<T> CreateComparer<V>(System.Func<T, V> keySelector)
        {
            return new CommonComparer<V>(keySelector);
        }
        public static System.Collections.Generic.IComparer<T> CreateComparer<V>(System.Func<T, V> keySelector, System.Collections.Generic.IComparer<V> comparer)
        {
            return new CommonComparer<V>(keySelector, comparer);
        }

        class CommonComparer<V> : System.Collections.Generic.IComparer<T>
        {
            private System.Func<T, V> keySelector;
            private System.Collections.Generic.IComparer<V> comparer;

            public CommonComparer(System.Func<T, V> keySelector, System.Collections.Generic.IComparer<V> comparer)
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

}