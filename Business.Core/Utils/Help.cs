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
    using System.Linq;
    using System.Reflection;

    public struct Accessor
    {
        public Accessor(System.Type type, System.Func<object, object> getter, System.Action<object, object> setter)
        {
            this.type = type;
            this.getter = getter;
            this.setter = setter;
        }

        readonly System.Type type;
        public System.Type Type => type;

        readonly System.Func<object, object> getter;
        public System.Func<object, object> Getter => getter;

        System.Action<object, object> setter;
        public System.Action<object, object> Setter => setter;

        public object TryGetter(object obj)
        {
            try { return Getter(obj); } catch { return type.IsValueType ? System.Activator.CreateInstance(type) : null; }
        }
    }

    public static class Help
    {
        //public static Business UseType<Business>(this Business business, params System.Type[] type)
        //   where Business : class, IBusiness
        //{
        //    if (null == business) { throw new System.ArgumentNullException(nameof(business)); }
        //    //this.Configuration.UseTypes.dictionary.TryAdd(type.FullName, type);

        //    //this.Configuration.MetaData.Values.
        //    //business.Configuration.MetaData[].
        //    foreach (var item in business.Configuration.MetaData)
        //    {
        //        foreach (var item2 in item.Value.ArgAttrs[Bind.CommandGroupDefault].Args)
        //        {
        //            if (item2.Type.FullName)
        //            {

        //            }
        //        }
        //    }

        //    return business;
        //}

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

        public static Assembly LoadAssembly(string path)
        {
#if NETFX
            return System.Reflection.Assembly.UnsafeLoadFrom(path);
#else
            return System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromAssemblyName(System.Runtime.Loader.AssemblyLoadContext.GetAssemblyName(path));
#endif
        }

        public static System.Collections.Generic.List<Assembly> LoadAssemblys(string path, string searchPattern = "*.dll")
        {
            var ass = new System.Collections.Generic.List<Assembly>();
            System.IO.Directory.GetFiles(path, searchPattern).ToList().ForEach(c => { try { var ass2 = Help.LoadAssembly(c); if (null != ass2 && !ass2.IsDynamic) { ass.Add(ass2); } } catch (System.Exception ex) { System.Console.WriteLine(c); ex.ExceptionWrite(console: true); } });
            return ass;
        }

        public static void LoadAssemblys(string path, System.Action<Assembly, TypeInfo> callback, string searchPattern = "*.dll")
        {
            if (null == callback) { throw new System.ArgumentNullException("callback"); }

            Help.LoadAssemblys(path, searchPattern).ForEach(c =>
            {
                try
                {
                    var types = c.GetExportedTypes();

                    foreach (var type in types)
                    {
                        var typeInfo = type.GetTypeInfo();
                        callback(c, typeInfo);
                    }
                }
                catch (System.Exception ex)
                {
                    System.Console.WriteLine(c.Location);
                    ex.ExceptionWrite(console: true);
                }
            });
        }

        public static Accessor GetAccessor(this System.Reflection.FieldInfo fieldInfo)
        {
            if (null == fieldInfo) { throw new System.ArgumentNullException(nameof(fieldInfo)); }

            var getter = Emit.FieldAccessorGenerator.CreateGetter(fieldInfo);
            var setter = Emit.FieldAccessorGenerator.CreateSetter(fieldInfo);

            return new Accessor(fieldInfo.FieldType, getter, setter);
        }

        public static Accessor GetAccessor(this System.Reflection.PropertyInfo propertyInfo)
        {
            if (null == propertyInfo) { throw new System.ArgumentNullException(nameof(propertyInfo)); }

            var getter = Emit.PropertyAccessorGenerator.CreateGetter(propertyInfo);
            var setter = Emit.PropertyAccessorGenerator.CreateSetter(propertyInfo);

            return new Accessor(propertyInfo.PropertyType, getter, setter);
        }

        #region GetAttributes

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

        public static System.Collections.Generic.List<Attribute> GetAttr<Attribute>(this System.Collections.Generic.List<Attributes.AttributeBase> attributes, System.Collections.Generic.IEqualityComparer<Attribute> comparer) where Attribute : Attributes.AttributeBase
        {
            var all = attributes.FindAll(c => c is Attribute).Cast<Attribute>();

            var attrs = all.Distinct(comparer).ToList();

            foreach (var item in all)
            {
                attributes.Remove(item);
            }

            attributes.AddRange(attrs);

            //foreach (var item in dd)
            //{
            //    attributes.Remove(item);
            //}
            //all.Except(attrs).ToList().ForEach(c => attributes.Remove(c));

            return attrs;
        }

        public static T[] GetAttributes<T>(this System.Reflection.MemberInfo member, bool inherit = true) where T : System.Attribute
        {
            if (null == member) { throw new System.ArgumentNullException(nameof(member)); }
            return member.GetCustomAttributes<T>(inherit).ToArray();
            //return (T[])System.Attribute.GetCustomAttributes(member, typeof(T), inherit);
        }
        public static T GetAttribute<T>(this System.Reflection.MemberInfo member, bool inherit = true) where T : System.Attribute
        {
            if (null == member) { throw new System.ArgumentNullException(nameof(member)); }
            return member.GetCustomAttribute<T>(inherit);
        }
        public static System.Collections.Generic.List<Attributes.AttributeBase> GetAttributes(this System.Reflection.MemberInfo member, bool inherit = true) => member.GetAttributes<Attributes.AttributeBase>(inherit).Where(c => !c.GetType().Equals(typeof(Attributes.ArgumentAttribute))).ToList();

        public static T[] GetAttributes<T>(this System.Reflection.ParameterInfo member, bool inherit = true) where T : System.Attribute
        {
            if (null == member) { throw new System.ArgumentNullException(nameof(member)); }
            return member.GetCustomAttributes<T>(inherit).ToArray();
            //return (T[])System.Attribute.GetCustomAttributes(member, typeof(T), inherit);
        }
        public static T GetAttribute<T>(this System.Reflection.ParameterInfo member, bool inherit = true) where T : System.Attribute
        {
            if (null == member) { throw new System.ArgumentNullException(nameof(member)); }
            return member.GetCustomAttribute<T>(inherit);
        }

        public static bool Exists<T>(this T[] attrs) where T : System.Attribute
        {
            if (null == attrs) { throw new System.ArgumentNullException(nameof(attrs)); }

            return 0 < attrs.Length;
        }

        public static bool Exists<T>(this System.Reflection.MemberInfo member, bool inherit = true) where T : System.Attribute
        {
            if (null == member) { throw new System.ArgumentNullException(nameof(member)); }

            return member.IsDefined(typeof(T));
        }
        public static bool Exists<T>(this System.Reflection.ParameterInfo member, bool inherit = true) where T : System.Attribute
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
        public static async System.Threading.Tasks.Task<byte[]> StreamReadByteAsync(this System.IO.Stream stream)
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
        public static async System.Threading.Tasks.Task<byte[]> StreamCopyByteAsync(this System.IO.Stream stream)
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
            using (var reader = new System.IO.StreamReader(stream, encoding ?? System.Text.Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
        public static async System.Threading.Tasks.Task<string> StreamReadStringAsync(this System.IO.Stream stream, System.Text.Encoding encoding = null)
        {
            using (var reader = new System.IO.StreamReader(stream, encoding ?? System.Text.Encoding.UTF8))
            {
                return await reader.ReadToEndAsync();
            }
        }

        public static string FileReadString(string path, System.Text.Encoding encoding = null)
        {
            if (System.String.IsNullOrWhiteSpace(path)) { throw new System.ArgumentException(nameof(path)); }

            using (var fileStream = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
            {
                return fileStream.StreamReadString(encoding ?? System.Text.Encoding.UTF8);
            }
        }
        public static async System.Threading.Tasks.Task<string> FileReadStringAsync(string path, System.Text.Encoding encoding = null)
        {
            if (System.String.IsNullOrWhiteSpace(path)) { throw new System.ArgumentException(nameof(path)); }

            using (var fileStream = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
            {
                return await fileStream.StreamReadStringAsync(encoding ?? System.Text.Encoding.UTF8);
            }
        }
        public static byte[] FileReadByte(string path)
        {
            if (System.String.IsNullOrWhiteSpace(path)) { throw new System.ArgumentException(nameof(path)); }

            using (var fileStream = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
            {
                return fileStream.StreamCopyByte();
            }
        }
        public static async System.Threading.Tasks.Task<byte[]> FileReadByteAsync(string path)
        {
            if (System.String.IsNullOrWhiteSpace(path)) { throw new System.ArgumentException(nameof(path)); }

            using (var fileStream = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
            {
                return await fileStream.StreamCopyByteAsync();
            }
        }

        public static void StreamWrite(this System.IO.Stream stream, string value, System.Text.Encoding encoding = null)
        {
            using (var writer = new System.IO.StreamWriter(stream, encoding ?? System.Text.Encoding.UTF8))
            {
                writer.AutoFlush = true;
                writer.Write(value);
            }
        }
        public static async System.Threading.Tasks.Task StreamWriteAsync(this System.IO.Stream stream, string value, System.Text.Encoding encoding = null)
        {
            using (var writer = new System.IO.StreamWriter(stream, encoding ?? System.Text.Encoding.UTF8))
            {
                writer.AutoFlush = true;
                await writer.WriteAsync(value);
            }
        }
        /*
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
        */
        /// <summary>
        /// gzip to byte[]
        /// </summary>
        /// <param name="value">byte[]</param>
        /// <returns>byte[]</returns>
        public static System.Byte[] GZipDecompressByte(this System.Byte[] value)
        {
            if (null == value) { throw new System.ArgumentNullException(nameof(value)); }

            using (var m = GZipDecompressStream(value)) { return m.ToArray(); }
        }
        /// <summary>
        /// gzip to byte[]
        /// </summary>
        /// <param name="value">byte[]</param>
        /// <returns>MemoryStream</returns>
        public static System.IO.MemoryStream GZipDecompressStream(this System.Byte[] value)
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

        public static string MD5(this string value, string encodingNmae = "UTF-8", bool hasUpper = false)
        {
            if (null == value) { throw new System.ArgumentNullException(nameof(value)); }

            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var result = System.BitConverter.ToString(md5.ComputeHash(System.Text.Encoding.GetEncoding(encodingNmae).GetBytes(value))).Replace("-", System.String.Empty);
                return hasUpper ? result.ToUpperInvariant() : result.ToLowerInvariant();
            }
        }

        public static class AES
        {
            public struct Result
            {
                public string iv;
                public string value;
            }

            public static Result Encrypt(string input, string key, string iv = null)
            {
                if (null == input) { throw new System.ArgumentNullException(nameof(input)); }

                if (null == key) { throw new System.ArgumentNullException(nameof(key)); }

                var encryptKey = System.Text.Encoding.UTF8.GetBytes(key);

                using (var aesAlg = System.Security.Cryptography.Aes.Create())
                {
                    var encryptIV = null == iv ? aesAlg.IV : System.Convert.FromBase64String(iv);

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

                                return new Result { iv = System.Convert.ToBase64String(encryptIV), value = System.Convert.ToBase64String(value) };
                            }
                        }
                    }
                }
            }

            public static string Decrypt(string input, string key, string iv)
            {
                if (null == input) { throw new System.ArgumentNullException(nameof(input)); }

                if (null == key) { throw new System.ArgumentNullException(nameof(key)); }

                var data = System.Convert.FromBase64String(input);

                var decryptIV = System.Convert.FromBase64String(iv);

                var cipher = new byte[data.Length];

                System.Buffer.BlockCopy(data, 0, cipher, 0, data.Length);
                var decryptKey = System.Text.Encoding.UTF8.GetBytes(key);

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

        public static bool ContainsAndNotNull(this System.Collections.Generic.IDictionary<string, dynamic> dict, string source)
        {
            return null != dict && dict.ContainsKey(source) && !System.Object.Equals(null, dict[source]);
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
        public static System.Exception ExceptionWrite(this System.Exception ex, bool write = false, bool console = false, string path = "business.log.txt", string dateFormat = "yyyy-MM-dd HH:mm:ss:fff")
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

            WriteLocal(message, path, false, write, console, dateFormat);

            return inner;
        }

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
        public static void WriteLocal(string text, string path = "business.log.txt", bool autoTime = true, bool write = true, bool console = false, string dateFormat = "yyyy-MM-dd HH:mm:ss:fff")
        {
            if (System.String.IsNullOrWhiteSpace(path)) { throw new System.ArgumentException(nameof(path)); }

            if (!write && !console) { return; }

            if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(path)))
            {
                path = System.IO.Path.Combine(BaseDirectory, System.IO.Path.GetFileName(path));
            }

            if (autoTime)
            {
                text = string.Format("[{0}] {1}", System.DateTime.Now.ToString(dateFormat), text);
            }

            if (console) { System.Console.WriteLine(text); }

            if (write)
            {
                locker.EnterWriteLock();

                try
                {
                    var prefix = System.String.Empty;

                    if (System.IO.File.Exists(path))
                    {
                        using (var fileStream = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
                        {
                            using (var stream = new System.IO.StreamReader(fileStream, System.Text.Encoding.UTF8))
                            {
                                if (-1 != stream.Peek()) { prefix = string.Format("{0}{0}", System.Environment.NewLine); }
                            }
                        }
                    }

                    System.IO.File.AppendAllText(path, string.Format("{0}{1}", prefix, text), System.Text.Encoding.UTF8);
                }
                finally { locker.ExitWriteLock(); }
            }
        }

        public static void Console(string text, bool autoTime = true, bool console = true, bool write = false, string path = "business.log.txt", string dateFormat = "yyyy-MM-dd HH:mm:ss:fff") => WriteLocal(text, path, autoTime, write, console, dateFormat);

        #endregion

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
            Number = 1,
            /// <summary>
            /// Allow upper
            /// </summary>
            Upper = 2,
            /// <summary>
            /// Allow lower
            /// </summary>
            Lower = 4,
            /// <summary>
            /// Allow chinese
            /// </summary>
            Chinese = 8
        }

        static System.Predicate<int> number = delegate (int c) { return !(c >= 48 && c <= 57); };
        static System.Predicate<int> upper = delegate (int c) { return !(c >= 65 && c <= 90); };
        static System.Predicate<int> lower = delegate (int c) { return !(c >= 97 && c <= 122); };
        static System.Predicate<int> chinese = delegate (int c) { return !(c >= 0x4e00 && c <= 0x9fbb); };

        public static bool CheckChar(string value, CheckCharMode mode = CheckCharMode.All)
        {
            if (System.String.IsNullOrWhiteSpace(value)) { return false; }

            var _value = value.Trim();
            var list = new System.Collections.Generic.List<int>();
            for (int i = 0; i < _value.Length; i++) { list.Add(_value[i]); }

            switch (mode)
            {
                case CheckCharMode.All:
                    return !list.Exists(c => number(c) && upper(c) && lower(c) && chinese(c));
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
                i *= ((int)b + 1);
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
        public static string GetMethodFullName(this System.Reflection.MethodInfo methodInfo)
        {
            if (null == methodInfo) { throw new System.ArgumentNullException(nameof(methodInfo)); }

            return string.Format("{0}.{1}", methodInfo.DeclaringType.FullName, methodInfo.Name);
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

            var props = typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
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
            if (System.String.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            return System.Text.RegularExpressions.Regex.IsMatch(email.Trim(), @"^([0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9})$");
        }

        public static double Scale(this double value, int size = 2)
        {
            var p = System.Math.Pow(10, size);
            return (double)((int)(value * (int)p) / p);
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
            return null == value ? null : System.Enum.GetName(value.GetType(), value);
        }
        public static int? GetValue(this System.Enum value)
        {
            return null == value ? new System.Nullable<int>() : value.GetHashCode();
        }
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
            var list = System.String.IsNullOrEmpty(json) ? new System.Collections.ArrayList() : json.TryJsonDeserialize<System.Collections.IList>() ?? new System.Collections.ArrayList { json };

            return list.Adde(item).JsonSerialize();
        }

        #region Json

        public static Type TryJsonDeserialize<Type>(this string value, Newtonsoft.Json.JsonSerializerSettings settings)
        {
            try { return Newtonsoft.Json.JsonConvert.DeserializeObject<Type>(value, settings); }
            catch (System.Exception)
            {
                return default(Type);
            }
        }
        public static Type TryJsonDeserialize<Type>(this string value, params Newtonsoft.Json.JsonConverter[] converters)
        {
            try { return Newtonsoft.Json.JsonConvert.DeserializeObject<Type>(value, converters); }
            catch (System.Exception)
            {
                return default(Type);
            }
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
        public static bool TryJsonDeserialize<Type>(this string value, System.Type type, out Type result)
        {
            try
            {
                result = (Type)Newtonsoft.Json.JsonConvert.DeserializeObject(value, type);
                return true;
            }
            catch
            {
                result = default(Type);
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
                result = default(Type);
                return false;
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
        public static bool TryProtoBufDeserialize<Type>(this System.Byte[] source, System.Type type, out Type result)
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
        public static bool TryProtoBufDeserialize<Type>(this System.Byte[] source, out Type result)
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
        public static object TryProtoBufDeserialize(this System.Byte[] source, System.Type type)
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
            var collectionType = typeof(System.Collections.Generic.ICollection<>);

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
            var enumerableType = typeof(System.Collections.Generic.IEnumerable<>);

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
        public static System.Collections.Generic.IEnumerable<System.Type> NotOfType<TType>(this System.Collections.Generic.IEnumerable<System.Type> types)
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
        readonly internal System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue> dictionary;

        public ConcurrentReadOnlyDictionary(System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue> dictionary) : base(dictionary) => this.dictionary = dictionary;

        public ConcurrentReadOnlyDictionary() : this(new System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>()) { }
    }

    public class ConcurrentReadOnlyCollection<TValue> : System.Collections.Generic.IReadOnlyCollection<TValue>
    {
        readonly internal System.Collections.Concurrent.ConcurrentBag<TValue> collection;

        public int Count => collection.Count;

        public ConcurrentReadOnlyCollection(System.Collections.Concurrent.ConcurrentBag<TValue> dictionary) => this.collection = dictionary;

        public ConcurrentReadOnlyCollection() : this(new System.Collections.Concurrent.ConcurrentBag<TValue>()) { }

        public System.Collections.Generic.IEnumerator<TValue> GetEnumerator() => collection.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

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