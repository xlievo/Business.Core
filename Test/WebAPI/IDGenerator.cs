namespace Easy.Common
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Dynamic;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Xml;
    using System.Xml.Linq;

    /// <summary>
    /// Inspired by <see href="https://github.com/aspnet/KestrelHttpServer/blob/6fde01a825cffc09998d3f8a49464f7fbe40f9c4/src/Kestrel.Core/Internal/Infrastructure/CorrelationIdGenerator.cs"/>,
    /// this class generates an efficient 20-bytes ID which is the concatenation of a <c>base36</c> encoded
    /// machine name and <c>base32</c> encoded <see cref="long"/> using the alphabet <c>0-9</c> and <c>A-V</c>.
    /// </summary>
    public sealed class IDGenerator
    {
        private const string Encode_32_Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUV";
        private static readonly char[] _prefix = new char[6];
        private static long _lastId = DateTime.UtcNow.Ticks;

        private static readonly ThreadLocal<char[]> _charBufferThreadLocal =
            new ThreadLocal<char[]>(() =>
            {
                var buffer = new char[20];
                buffer[0] = _prefix[0];
                buffer[1] = _prefix[1];
                buffer[2] = _prefix[2];
                buffer[3] = _prefix[3];
                buffer[4] = _prefix[4];
                buffer[5] = _prefix[5];
                buffer[6] = '-';
                return buffer;
            });

        static IDGenerator() => PopulatePrefix();
        private IDGenerator() { }

        /// <summary>
        /// Returns a single instance of the <see cref="IDGenerator"/>.
        /// </summary>
        public static IDGenerator Instance { get; } = new IDGenerator();

        /// <summary>
        /// Returns an ID. e.g: <c>XOGLN1-0HLHI1F5INOFA</c>
        /// </summary>
        public string Next => GenerateImpl(Interlocked.Increment(ref _lastId));

        private static string GenerateImpl(long id)
        {
            var buffer = _charBufferThreadLocal.Value;

            buffer[7] = Encode_32_Chars[(int)(id >> 60) & 31];
            buffer[8] = Encode_32_Chars[(int)(id >> 55) & 31];
            buffer[9] = Encode_32_Chars[(int)(id >> 50) & 31];
            buffer[10] = Encode_32_Chars[(int)(id >> 45) & 31];
            buffer[11] = Encode_32_Chars[(int)(id >> 40) & 31];
            buffer[12] = Encode_32_Chars[(int)(id >> 35) & 31];
            buffer[13] = Encode_32_Chars[(int)(id >> 30) & 31];
            buffer[14] = Encode_32_Chars[(int)(id >> 25) & 31];
            buffer[15] = Encode_32_Chars[(int)(id >> 20) & 31];
            buffer[16] = Encode_32_Chars[(int)(id >> 15) & 31];
            buffer[17] = Encode_32_Chars[(int)(id >> 10) & 31];
            buffer[18] = Encode_32_Chars[(int)(id >> 5) & 31];
            buffer[19] = Encode_32_Chars[(int)id & 31];

            return new string(buffer, 0, buffer.Length);
        }

        private static void PopulatePrefix()
        {
            var machineHash = Math.Abs(Environment.MachineName.GetHashCode());
            var machineEncoded = Base36.Encode(machineHash);

            var i = _prefix.Length - 1;
            var j = 0;
            while (i >= 0)
            {
                if (j < machineEncoded.Length)
                {
                    _prefix[i] = machineEncoded[j];
                    j++;
                }
                else
                {
                    _prefix[i] = '0';
                }
                i--;
            }
        }
    }

    /// <summary>
    /// A Base36 Encoder and Decoder
    /// </summary>
    public static class Base36
    {
        private const string Base36Characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        /// <summary>
        /// Encode the given number into a <see cref="Base36"/>string.
        /// </summary>
        /// <param name="input">The number to encode.</param>
        /// <returns>Encoded <paramref name="input"/> as string.</returns>
        public static string Encode(long input)
        {
            Ensure.That<ArgumentException>(input >= 0, "Input cannot be negative.");

            var arr = Base36Characters.ToCharArray();
            var result = new Stack<char>();
            while (input != 0)
            {
                result.Push(arr[input % 36]);
                input /= 36;
            }
            return new string(result.ToArray());
        }

        /// <summary>
        /// Decode the <see cref="Base36"/> encoded string into a long.
        /// </summary>
        /// <param name="input">The number to decode.</param>
        /// <returns>Decoded <paramref name="input"/> as long.</returns> 
        public static long Decode(string input)
        {
            Ensure.NotNull(input, nameof(input));

            var reversed = input.ToLower().Reverse();
            long result = 0;
            var pos = 0;
            foreach (var c in reversed)
            {
                result += Base36Characters.IndexOf(c) * (long)Math.Pow(36, pos);
                pos++;
            }
            return result;
        }
    }

    /// <summary>
    /// Helper class that will <see langword="throw"/> exceptions when conditions are not satisfied.
    /// </summary>
    public static class Ensure
    {
        /// <summary>
        /// Ensures that the given expression is <see langword="true"/>.
        /// </summary>
        /// <typeparam name="TException">Type of exception to throw</typeparam>
        /// <param name="condition">Condition to test/ensure</param>
        /// <param name="message">Message for the exception</param>
        /// <exception>
        ///     Thrown when <cref>TException</cref> <paramref name="condition"/> is <see langword="false"/>.
        /// </exception>
        [DebuggerStepThrough]
        public static void That<TException>(bool condition, string message = "The given condition is false.") where TException : Exception
        {
            if (!condition) { throw (TException)Activator.CreateInstance(typeof(TException), message); }
        }

        /// <summary>
        /// Ensures given <paramref name="condition"/> is <see langword="true"/>.
        /// </summary>
        /// <param name="condition">Condition to test</param>
        /// <param name="message">Message for the exception</param>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="condition"/> is <see langword="false"/>.
        /// </exception>
        [DebuggerStepThrough]
        public static void That(bool condition, string message = "The given condition is false.")
            => That<ArgumentException>(condition, message);

        /// <summary>
        /// Ensures given <paramref name="condition"/> is <see langword="false"/>.
        /// </summary>
        /// <typeparam name="TException">Type of exception to throw</typeparam>
        /// <param name="condition">Condition to test</param>
        /// <param name="message">Message for the exception</param>
        /// <exception> 
        ///     Thrown when <paramref name="condition"/> is <see langword="false"/>.
        /// </exception>
        [DebuggerStepThrough]
        public static void Not<TException>(bool condition, string message = "The given condition is true.") where TException : Exception
            => That<TException>(!condition, message);

        /// <summary>
        /// Ensures given <paramref name="condition"/> is <see langword="false"/>.
        /// </summary>
        /// <param name="condition">Condition to test</param>
        /// <param name="message">Message for the exception</param>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="condition"/> is <see langword="false"/>.
        /// </exception>
        [DebuggerStepThrough]
        public static void Not(bool condition, string message = "The given condition is true.")
            => Not<ArgumentException>(condition, message);

        /// <summary>
        /// Ensures given <see langword="object"/> is not null.
        /// </summary>
        /// <typeparam name="T">Type of the given <see langword="object"/> .</typeparam>
        /// <param name="value"> Value of the <see langword="object"/> to check for <see langword="null"/> reference.</param>
        /// <param name="argName"> Name of the argument.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="value"/> is null
        /// </exception>
        /// <returns> The <typeparamref name="T"/>.</returns>
        [DebuggerStepThrough]
        public static T NotNull<T>(T value, string argName) where T : class
        {
            if (argName.IsNullOrEmptyOrWhiteSpace()) { argName = "Invalid"; }

            That<ArgumentNullException>(value != null, argName);
            return value;
        }

        /// <summary>
        /// Ensures given objects are equal.
        /// </summary>
        /// <typeparam name="T">Type of objects to compare for equality</typeparam>
        /// <param name="left">The left item</param>
        /// <param name="right">The right item</param>
        /// <param name="message">Message for the exception</param>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="left"/> not equal to <paramref name="right"/>
        /// </exception>
        /// <remarks>Null values will cause an exception to be thrown</remarks>
        [DebuggerStepThrough]
        public static void Equal<T>(T left, T right, string message = "Values must be equal.")
            => That<ArgumentException>(Comparer<T>.Default.Compare(left, right) == 0, message);

        /// <summary>
        /// Ensures given objects are not equal.
        /// </summary>
        /// <typeparam name="T">Type of objects to compare for equality</typeparam>
        /// <param name="left">The left item</param>
        /// <param name="right">The right item</param>
        /// <param name="message">Message for the exception</param>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="left"/> equal to <paramref name="right"/>
        /// </exception>
        /// <remarks>Null values will cause an exception to be thrown</remarks>
        [DebuggerStepThrough]
        public static void NotEqual<T>(T left, T right, string message = "Values must not be equal.")
            => That<ArgumentException>(Comparer<T>.Default.Compare(left, right) != 0, message);

        /// <summary>
        /// Ensures a given <paramref name="collection"/> is not null or empty.
        /// </summary>
        /// <typeparam name="T">Collection type.</typeparam>
        /// <param name="collection">Collection to check.</param>
        /// <param name="message">Message for the exception</param>
        /// <returns>The evaluated collection.</returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="collection"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="collection"/> is empty.
        /// </exception>
        [DebuggerStepThrough]
        public static ICollection<T> NotNullOrEmpty<T>(ICollection<T> collection, string message = "Collection must not be null or empty.")
        {
            NotNull(collection, nameof(collection));
            Not<ArgumentException>(!collection.Any(), message);
            return collection;
        }

        /// <summary>
        /// Ensures the given string is not <see langword="null"/> or empty or whitespace.
        /// </summary>
        /// <param name="value"><c>String</c> <paramref name="value"/> to check.</param>
        /// <param name="message">Message for the exception</param>
        /// <returns>Value to return if it is not null, empty or whitespace.</returns>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="value"/> is null or empty or whitespace.
        /// </exception>
        [DebuggerStepThrough]
        public static string NotNullOrEmptyOrWhiteSpace(string value, string message = "String must not be null, empty or whitespace.")
        {
            That<ArgumentException>(value.IsNotNullOrEmptyOrWhiteSpace(), message);
            return value;
        }

        /// <summary>
        /// Ensures given <see cref="DirectoryInfo"/> exists.
        /// </summary>
        /// <param name="directoryInfo">DirectoryInfo object representing the directory to check for existence.</param>
        /// <returns>DirectoryInfo to return if the directory exists.</returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="directoryInfo"/> is null.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        ///     Thrown when <paramref name="directoryInfo"/> is not found.
        /// </exception>
        /// <exception cref="IOException">
        ///     A device such as a disk drive is not ready.
        /// </exception>
        [DebuggerStepThrough]
        public static DirectoryInfo Exists(DirectoryInfo directoryInfo)
        {
            NotNull(directoryInfo, nameof(directoryInfo));

            directoryInfo.Refresh();
            That<DirectoryNotFoundException>(directoryInfo.Exists, $"Cannot find: '{directoryInfo.FullName}'.");
            return directoryInfo;
        }

        /// <summary>
        /// Ensures given <paramref name="fileInfo"/> exists.
        /// </summary>
        /// <param name="fileInfo">FileInfo object representing the file to check for existence.</param>
        /// <returns>FileInfo to return if the file exists.</returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="fileInfo"/> is null.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        ///     Thrown when <paramref name="fileInfo"/> does not exist.
        /// </exception>
        [DebuggerStepThrough]
        public static FileInfo Exists(FileInfo fileInfo)
        {
            NotNull(fileInfo, nameof(fileInfo));

            fileInfo.Refresh();
            That<FileNotFoundException>(fileInfo.Exists, $"Cannot find: '{fileInfo.FullName}'.");
            return fileInfo;
        }
    }

    /// <summary>
    /// Extensions for <see cref="string"/>
    /// </summary>
    public static class StringExtensions
    {
        private static readonly char[] InvalidFileNameCharacters = Path.GetInvalidFileNameChars();
        private static readonly char[] InvalidPathCharacters = Path.GetInvalidPathChars();

        /// <summary>
        /// A nicer way of calling <see cref="string.IsNullOrEmpty(string)"/>
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns>
        /// <see langword="true"/> if the format parameter is null or an empty string (""); otherwise, <see langword="false"/>.
        /// </returns>
        [DebuggerStepThrough]
        public static bool IsNullOrEmpty(this string value) => string.IsNullOrEmpty(value);

        /// <summary>
        /// A nice way of calling the inverse of <see cref="string.IsNullOrEmpty(string)"/>
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns>
        /// <see langword="true"/> if the format parameter is not null or an empty string (""); otherwise, <see langword="false"/>.
        /// </returns>
        [DebuggerStepThrough]
        public static bool IsNotNullOrEmpty(this string value) => !value.IsNullOrEmpty();

        /// <summary>
        /// A nice way of checking if a string is null, empty or whitespace 
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns>
        /// <see langword="true"/> if the format parameter is null or an empty string (""); otherwise, <see langword="false"/>.
        /// </returns>
        [DebuggerStepThrough]
        public static bool IsNullOrEmptyOrWhiteSpace(this string value) => string.IsNullOrWhiteSpace(value);

        /// <summary>
        /// A nice way of checking the inverse of (if a string is null, empty or whitespace) 
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns>
        /// <see langword="true"/> if the format parameter is not null or an empty string (""); otherwise, <see langword="false"/>.
        /// </returns>
        [DebuggerStepThrough]
        public static bool IsNotNullOrEmptyOrWhiteSpace(this string value)
            => !value.IsNullOrEmptyOrWhiteSpace();

        /// <summary>
        /// Parses a string as Boolean, valid inputs are: <c>true|false|yes|no|1|0</c>.
        /// <remarks>Input is parsed as Case-Insensitive.</remarks>
        /// </summary>
        public static bool TryParseAsBool(this string value, out bool result)
        {
            Ensure.NotNull(value, nameof(value));

            const StringComparison CompPolicy = StringComparison.OrdinalIgnoreCase;

            if (value.Equals("true", CompPolicy)
                || value.Equals("yes", CompPolicy)
                || value.Equals("1", CompPolicy))
            {
                result = true;
                return true;
            }

            if (value.Equals("false", CompPolicy)
                || value.Equals("no", CompPolicy)
                || value.Equals("0", CompPolicy))
            {
                result = false;
                return true;
            }

            result = false;
            return false;
        }

        /// <summary>
        /// Allows for using strings in <see langword="null"/> coalescing operations.
        /// </summary>
        /// <param name="value">The string value to check.</param>
        /// <returns>
        /// Null if <paramref name="value"/> is empty or the original <paramref name="value"/>.
        /// </returns>
        [DebuggerStepThrough]
        public static string NullIfEmpty(this string value) => value == string.Empty ? null : value;

        /// <summary>
        /// Tries to extract the value between the tag <paramref name="tagName"/> 
        /// from the <paramref name="input"/>.
        /// <remarks>This method is case insensitive.</remarks>
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="tagName">The tag whose value will be returned e.g <c>span, img</c>.</param>
        /// <param name="value">The extracted value.</param>
        /// <returns><c>True</c> if successful otherwise <c>False</c>.</returns>
        public static bool TryExtractValueFromTag(this string input, string tagName, out string value)
        {
            Ensure.NotNull(input, nameof(input));
            Ensure.NotNull(tagName, nameof(tagName));

            var pattern = $"<{tagName}[^>]*>(.*)</{tagName}>";
            var match = Regex.Match(input, pattern, RegexOptions.IgnoreCase);

            if (match.Success)
            {
                value = match.Groups[1].ToString();
                return true;
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Returns a string array containing the trimmed substrings in this <paramref name="value"/>
        /// that are delimited by the provided <paramref name="separators"/>.
        /// </summary>
        public static string[] SplitAndTrim(this string value, params char[] separators)
        {
            Ensure.NotNull(value, nameof(value));
            return value.Trim()
                .Split(separators, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .ToArray();
        }

        /// <summary>
        /// Checks if the <paramref name="input"/> contains the <paramref name="stringToCheckFor"/> 
        /// based on the provided <paramref name="comparison"/> rules.
        /// </summary>
        public static bool Contains(this string input, string stringToCheckFor, StringComparison comparison)
            => input.IndexOf(stringToCheckFor, comparison) >= 0;

        /// <summary>
        /// Checks that given <paramref name="input"/> matches any of the potential matches.
        /// Inspired by: http://stackoverflow.com/a/20644611/23199
        /// </summary>
        public static bool EqualsAny(
            this string input, StringComparer comparer, string match1, string match2)
                => comparer.Equals(input, match1) || comparer.Equals(input, match2);

        /// <summary>
        /// Checks that given <paramref name="input"/> matches any of the potential matches.
        /// Inspired by: http://stackoverflow.com/a/20644611/23199
        /// </summary>
        public static bool EqualsAny(
            this string input, StringComparer comparer, string match1, string match2, string match3)
                => comparer.Equals(input, match1) || comparer.Equals(input, match2) || comparer.Equals(input, match3);

        /// <summary>
        /// Checks that given <paramref name="input"/> is in a list of 
        /// potential <paramref name="matches"/>.
        /// <remarks>Inspired by: <see href="http://stackoverflow.com/a/20644611/23199"/> </remarks>
        /// </summary>
        public static bool EqualsAny(this string input, StringComparer comparer, params string[] matches)
            => matches.Any(x => comparer.Equals(x, input));

        /// <summary>
        /// Checks to see if the given input is a valid palindrome or not.
        /// </summary>
        public static bool IsPalindrome(this string input)
        {
            Ensure.NotNull(input, nameof(input));
            var min = 0;
            var max = input.Length - 1;
            while (true)
            {
                if (min > max) { return true; }

                var a = input[min];
                var b = input[max];
                if (char.ToLower(a) != char.ToLower(b)) { return false; }

                min++;
                max--;
            }
        }

        /// <summary>
        /// Truncates the <paramref name="input"/> to the maximum length of <paramref name="maxLength"/> 
        /// and replaces the truncated part with <paramref name="suffix"/>
        /// </summary>
        /// <param name="input">The input string</param>
        /// <param name="maxLength">Total length of characters to maintain before truncation.</param>
        /// <param name="suffix">The suffix to add to the end of the truncated <paramref name="input"/></param>
        public static string Truncate(this string input, int maxLength, string suffix = "")
        {
            Ensure.NotNull(input, nameof(input));
            Ensure.NotNull(suffix, nameof(suffix));

            if (maxLength < 0) { return input; }
            if (maxLength == 0) { return string.Empty; }

            var chars = input.Take(maxLength).ToArray();

            if (chars.Length != input.Length)
            {
                return new string(chars) + suffix;
            }

            return new string(chars);
        }

        /// <summary>
        /// Removes different types of new lines from a given string.
        /// </summary>
        /// <param name="input">input string.</param>
        /// <returns>The given input minus any new line characters.</returns>
        public static string RemoveNewLines(this string input)
        {
            Ensure.NotNull(input, nameof(input));
            return input.Replace("\n", string.Empty).Replace("\r", string.Empty);
        }

        /// <summary>
        /// Separates a PascalCase string.
        /// </summary>
        /// <example> "ThisIsPascalCase".SeparatePascalCase(); // returns "This Is Pascal Case" </example>
        /// <param name="value">The format to split</param>
        /// <returns>The original string separated on each uppercase character.</returns>
        public static string SeparatePascalCase(this string value)
        {
            Ensure.NotNullOrEmptyOrWhiteSpace(value);
            return Regex.Replace(value, "([A-Z])", " $1").Trim();
        }

        /// <summary>
        /// Converts string to Pascal Case
        /// <example>This Is A Pascal Case String.</example>
        /// </summary>
        /// <param name="input">The given input.</param>
        /// <returns>The given <paramref name="input"/> converted to Pascal Case.</returns>
        public static string ToPascalCase(this string input)
        {
            Ensure.NotNull(input, nameof(input));

            var cultureInfo = Thread.CurrentThread.CurrentCulture;
            var textInfo = cultureInfo.TextInfo;
            return textInfo.ToTitleCase(input);
        }

        /// <summary>
        /// Compares <paramref name="input"/> against <paramref name="target"/>, 
        /// the comparison is case-sensitive.
        /// </summary>
        /// <param name="input">The input string</param>
        /// <param name="target">The target string</param>
        public static bool IsEqualTo(this string input, string target)
        {
            if (input == null && target == null) { return true; }
            if (input == null || target == null) { return false; }
            if (input.Length != target.Length) { return false; }

            return string.CompareOrdinal(input, target) == 0;
        }

        /// <summary>
        /// Handy method to print arguments to <c>System.Console</c>.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="args">The arguments.</param>
        [DebuggerStepThrough]
        public static void Print(this string input, params object[] args) => Console.WriteLine(input, args);

        /// <summary>
        /// Generates a slug.
        /// <remarks>
        /// Credit goes to <see href="http://stackoverflow.com/questions/2920744/url-slugify-alrogithm-in-cs"/>.
        /// </remarks>
        /// </summary>
        [DebuggerStepThrough]
        public static string GenerateSlug(this string value, uint? maxLength = null)
        {
            // prepare string, remove diacritics, lower case and convert hyphens to whitespace
            var result = RemoveDiacritics(value).Replace("-", " ").ToLowerInvariant();

            result = Regex.Replace(result, @"[^a-z0-9\s-]", string.Empty); // remove invalid characters
            result = Regex.Replace(result, @"\s+", " ").Trim(); // convert multiple spaces into one space

            if (maxLength.HasValue)
            {
                result = result.Substring(0, result.Length <= maxLength
                    ? result.Length : (int)maxLength.Value).Trim();
            }
            return Regex.Replace(result, @"\s", "-");
        }

        /// <summary>
        /// Removes the diacritics from the given <paramref name="input"/> 
        /// </summary>
        /// <remarks>
        /// Credit goes to <see href="http://stackoverflow.com/a/249126"/>.
        /// </remarks>
        [DebuggerStepThrough]
        public static string RemoveDiacritics(this string input)
        {
            var normalizedString = input.Normalize(NormalizationForm.FormD);
            var stringBuilder = StringBuilderCache.Acquire();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return StringBuilderCache.GetStringAndRelease(stringBuilder).Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        /// A method to convert English digits to Persian numbers.
        /// </summary>
        [DebuggerStepThrough]
        public static string ToPersianNumber(this string input)
        {
            Ensure.NotNull(input, nameof(input));
            return input
                .Replace("0", "۰")
                .Replace("1", "۱")
                .Replace("2", "۲")
                .Replace("3", "۳")
                .Replace("4", "۴")
                .Replace("5", "۵")
                .Replace("6", "۶")
                .Replace("7", "۷")
                .Replace("8", "۸")
                .Replace("9", "۹");
        }

        /// <summary>
        /// Gets a sequence containing every element with the name equal to <paramref name="name"/>.
        /// </summary>
        /// <param name="xmlInput">The input containing XML</param>
        /// <param name="name">The name of the elements to return</param>
        /// <param name="ignoreCase">The flag indicating whether the name should be looked up in a case sensitive manner</param>
        /// <returns>
        /// The sequence containing all the elements <see cref="XElement"/> matching the <paramref name="name"/>.
        /// </returns>
        [DebuggerStepThrough]
        public static IEnumerable<XElement> GetElements(
            this string xmlInput, XName name, bool ignoreCase = true)
        {
            Ensure.NotNull(xmlInput, nameof(xmlInput));
            return xmlInput.GetElements(name, new XmlReaderSettings(), ignoreCase);
        }

        /// <summary>
        /// Gets a sequence containing every element with the name equal to <paramref name="name"/>.
        /// </summary>
        /// <param name="xmlInput">The input containing XML</param>
        /// <param name="name">The name of the elements to return</param>
        /// <param name="settings">The settings used by the <see cref="XmlReader"/></param>
        /// <param name="ignoreCase">The flag indicating whether the name should be looked up in a case sensitive manner</param>
        /// <returns>
        /// The sequence containing all the elements <see cref="XElement"/> matching the <paramref name="name"/>.
        /// </returns>
        [DebuggerStepThrough]
        public static IEnumerable<XElement> GetElements(
            this string xmlInput, XName name, XmlReaderSettings settings, bool ignoreCase = true)
        {
            Ensure.NotNull(xmlInput, nameof(xmlInput));
            Ensure.NotNull(name, nameof(name));
            Ensure.NotNull(settings, nameof(settings));

            using (var stringReader = new StringReader(xmlInput))
            using (var xmlReader = XmlReader.Create(stringReader, settings))
            {
                foreach (var xElement in xmlReader.GetEelements(name, ignoreCase))
                {
                    yield return xElement;
                }
            }
        }

        /// <summary>
        /// Compresses the given <paramref name="input"/> to <c>Base64</c> string.
        /// </summary>
        /// <param name="input">The string to be compressed</param>
        /// <returns>The compressed string in <c>Base64</c></returns>
        [DebuggerStepThrough]
        public static string Compress(this string input)
        {
            Ensure.NotNull(input, nameof(input));

            var buffer = Encoding.UTF8.GetBytes(input);
            using (var memStream = new MemoryStream())
            using (var zipStream = new GZipStream(memStream, CompressionMode.Compress, true))
            {
                zipStream.Write(buffer, 0, buffer.Length);
                zipStream.Close();

                memStream.Position = 0;

                var compressedData = new byte[memStream.Length];
                memStream.Read(compressedData, 0, compressedData.Length);

                var gZipBuffer = new byte[compressedData.Length + 4];
                Buffer.BlockCopy(compressedData, 0, gZipBuffer, 4, compressedData.Length);
                Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gZipBuffer, 0, 4);
                return Convert.ToBase64String(gZipBuffer);
            }
        }

        /// <summary>
        /// Decompresses a <c>Base64</c> compressed string.
        /// </summary>
        /// <param name="compressedInput">The string compressed in <c>Base64</c></param>
        /// <returns>The uncompressed string</returns>
        [DebuggerStepThrough]
        public static string Decompress(this string compressedInput)
        {
            Ensure.NotNull(compressedInput, nameof(compressedInput));

            var gZipBuffer = Convert.FromBase64String(compressedInput);
            using (var memStream = new MemoryStream())
            {
                var dataLength = BitConverter.ToInt32(gZipBuffer, 0);
                memStream.Write(gZipBuffer, 4, gZipBuffer.Length - 4);
                memStream.Position = 0;

                var buffer = new byte[dataLength];
                using (var zipStream = new GZipStream(memStream, CompressionMode.Decompress))
                {
                    zipStream.Read(buffer, 0, buffer.Length);
                }

                return Encoding.UTF8.GetString(buffer);
            }
        }

        /// <summary>
        /// Ensures the given <paramref name="input"/> can be used as a file name.
        /// </summary>
        [DebuggerStepThrough]
        public static bool IsValidFileName(this string input)
            => input.IsNotNullOrEmptyOrWhiteSpace() && input.IndexOfAny(InvalidFileNameCharacters) == -1;

        /// <summary>
        /// Ensures the given <paramref name="input"/> can be used as a path.
        /// </summary>
        [DebuggerStepThrough]
        public static bool IsValidPathName(this string input)
            => input.IsNotNullOrEmptyOrWhiteSpace() && input.IndexOfAny(InvalidPathCharacters) == -1;

        /// <summary>
        /// Returns a <see cref="Guid"/> from a <c>Base64</c> encoded <paramref name="input"/>.
        /// <example>
        /// DRfscsSQbUu8bXRqAvcWQA== or DRfscsSQbUu8bXRqAvcWQA depending on <paramref name="trimmed"/>.
        /// </example>
        /// <remarks>
        /// See: <see href="https://blog.codinghorror.com/equipping-our-ascii-armor/"/>
        /// </remarks>
        /// </summary>
        [DebuggerStepThrough]
        public static Guid ToGuid(this string input, bool trimmed = true)
            => trimmed ? new Guid(Convert.FromBase64String(input + "=="))
                : new Guid(Convert.FromBase64String(input));

        /// <summary>
        /// Returns all the start and end indexes of the occurrences of the 
        /// given <paramref name="startTag"/> and <paramref name="endTag"/> 
        /// in the given <paramref name="input"/>.
        /// </summary>
        /// <param name="input">The input to search.</param>
        /// <param name="startTag">The starting tag e.g. <c>&lt;div></c>.</param>
        /// <param name="endTag">The ending tag e.g. <c>&lt;/div></c>.</param>
        /// <returns>
        /// A sequence <see cref="KeyValuePair{TKey,TValue}"/> where the key is 
        /// the starting position and value is the end position.
        /// </returns>
        [DebuggerStepThrough]
        public static IEnumerable<KeyValuePair<int, int>> GetStartAndEndIndexes(
            this string input, string startTag, string endTag)
        {
            var startIdx = 0;
            int endIdx;

            while ((startIdx = input.IndexOf(startTag, startIdx, StringComparison.Ordinal)) != -1
                && (endIdx = input.IndexOf(endTag, startIdx, StringComparison.Ordinal)) != -1)
            {
                var result = new KeyValuePair<int, int>(startIdx, endIdx);
                startIdx = endIdx;
                yield return result;
            }
        }

        /// <summary>
        /// Returns the size of the given <paramref name="input"/> encoded 
        /// as <c>UTF-16</c> characters in bytes.
        /// </summary>
        [DebuggerStepThrough]
        public static int GetSize(this string input) => input.Length * sizeof(char);
    }

    /// <summary>
    /// Provides a cached reusable instance of <see cref="StringBuilder"/> per thread 
    /// it is an optimization that reduces the number of instances constructed and collected.
    /// <remarks>
    /// <para>A StringBuilder instance is cached in <c>Thread Local Storage</c> and so there is one per thread.</para>
    /// </remarks>
    /// </summary>
    public static class StringBuilderCache
    {
        [ThreadStatic]
        private static StringBuilder _cache;

        /// <summary>
        /// Acquires a cached instance of <see cref="StringBuilder"/> if one exists otherwise a new instance.
        /// </summary>
        /// <returns>An instance of <see cref="StringBuilder"/></returns>
        [DebuggerStepThrough]
        public static StringBuilder Acquire()
        {
            var result = _cache;
            if (result == null) { return new StringBuilder(); }

            result.Clear();
            _cache = null; // of that if caller forgets to release and return it is not kept alive by this class
            return result;
        }

        /// <summary>
        /// Gets the string representation of the <paramref name="builder"/> and releases it to the cache.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/></param>
        /// <returns>The string representation of the <paramref name="builder"/></returns>
        [DebuggerStepThrough]
        public static string GetStringAndRelease(StringBuilder builder)
        {
            var result = builder.ToString();
            _cache = builder;
            return result;
        }
    }

    /// <summary>
    /// Extension methods for classes in the <see cref="System.Xml"/> namespace.
    /// </summary>
    public static class XmlExtensions
    {
        /// <summary>
        /// Sets the default XML namespace of every element in the given XML element
        /// </summary>
        public static void SetDefaultXmlNamespace(this XElement element, XNamespace xmlns)
        {
            Ensure.NotNull(element, nameof(element));
            Ensure.NotNull(xmlns, nameof(xmlns));

            if (element.Name.NamespaceName == string.Empty)
            {
                element.Name = xmlns + element.Name.LocalName;
            }

            foreach (var e in element.Elements())
            {
                e.SetDefaultXmlNamespace(xmlns);
            }
        }

        /// <summary>
        /// Gets a sequence containing every element with the name equal to <paramref name="name"/>.
        /// </summary>
        /// <param name="reader">The <see cref="XmlReader"/> used to read the XML</param>
        /// <param name="name">The name of the elements to return</param>
        /// <param name="ignoreCase">The flag indicating whether the name should be looked up in a case sensitive manner</param>
        /// <returns>The sequence containing all the elements <see cref="XElement"/> matching the <paramref name="name"/></returns>
        public static IEnumerable<XElement> GetEelements(this XmlReader reader, XName name, bool ignoreCase = true)
        {
            Ensure.NotNull(reader, nameof(reader));
            Ensure.NotNull(name, nameof(name));

            var compPolicy = ignoreCase
                ? StringComparison.InvariantCultureIgnoreCase
                : StringComparison.InvariantCulture;

            reader.MoveToElement();
            while (reader.Read())
            {
                while (reader.NodeType == XmlNodeType.Element
                       && reader.Name.Equals(name.LocalName, compPolicy))
                {
                    yield return (XElement)XNode.ReadFrom(reader);
                }
            }
        }

        /// <summary>
        /// Converts the content of the given <paramref name="reader"/> to <see cref="DynamicDictionary"/>.
        /// </summary>
        public static DynamicDictionary ToDynamic(this XmlReader reader, bool ignoreCase = true)
        {
            Ensure.NotNull(reader, nameof(reader));

            var result = new DynamicDictionary(ignoreCase);
            var elements = new List<XElement>();
            result["Elements"] = elements;

            reader.MoveToElement();
            while (reader.Read())
            {
                while (reader.NodeType == XmlNodeType.Element)
                {
                    var element = (XElement)XNode.ReadFrom(reader);
                    elements.Add(element);
                }
            }

            return result;
        }
    }

    /// <summary>
    /// Provides an abstraction for an object to be used dynamically as a key value pair
    /// where the property name is the key and value is an <see cref="object"/>.
    /// </summary>
    public sealed class DynamicDictionary : DynamicObject, IDictionary<string, object>
    {
        private readonly IDictionary<string, object> _dictionary;

        /// <summary>
        /// Creates a new instance of <see cref="DynamicDictionary"/>.
        /// </summary>
        /// <param name="ignoreCase">
        /// The flag indicating whether property names should be treated case sensitively.
        /// </param>
        [DebuggerStepThrough]
        public DynamicDictionary(bool ignoreCase = true)
            => _dictionary = new Dictionary<string, object>(
                    ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);

        /// <summary>
        /// Add the given <paramref name="item"/> to this instance.
        /// </summary>
        /// <param name="item"></param>
        public void Add(KeyValuePair<string, object> item) => _dictionary.Add(item);

        /// <summary>
        /// Removes all the items from this instance.
        /// </summary>
        public void Clear() => _dictionary.Clear();

        /// <summary>
        /// Determines whether this instance contains the given <paramref name="item"/>.
        /// </summary>
        public bool Contains(KeyValuePair<string, object> item) => _dictionary.Contains(item);

        /// <summary>
        /// Copies the elements of this instance to the given <paramref name="array"/>, starting at a particular <paramref name="array"/>.
        /// </summary>
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
            => _dictionary.CopyTo(array, arrayIndex);

        /// <summary>
        /// Removes the given <paramref name="item"/> from this instance.
        /// </summary>
        public bool Remove(KeyValuePair<string, object> item) => _dictionary.Remove(item);

        /// <summary>
        /// Gets the number of elements contained in this instance.
        /// </summary>
        public int Count => _dictionary.Keys.Count;

        /// <summary>
        /// Determines whether this instance is <c>Read-Only</c>.
        /// </summary>
        public bool IsReadOnly => _dictionary.IsReadOnly;

        /// <summary>
        /// Returns an enumerator that iterates through the keys and values of this instance.
        /// </summary>
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _dictionary.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through the keys and values of this instance.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Determines whether this instance contains an element with the given <paramref name="key"/>.
        /// </summary>
        public bool ContainsKey(string key) => _dictionary.ContainsKey(key);

        /// <summary>
        /// Adds an element for the given <paramref name="key"/> and associated <paramref name="value"/> to this instance.
        /// </summary>
        public void Add(string key, object value) => _dictionary.Add(key, value);

        /// <summary>
        /// Removes the element with the given <paramref name="key"/> from this instance.
        /// </summary>
        public bool Remove(string key) => _dictionary.Remove(key);

        /// <summary>
        /// Attempts to get the value associated to the given <paramref name="key"/>.
        /// </summary>
        public bool TryGetValue(string key, out object value) =>
            _dictionary.TryGetValue(key, out value);

        /// <summary>
        /// Gets or sets the value stored against the given <paramref name="key"/>.
        /// <remarks>If the given <paramref name="key"/> does not exist, <c>NULL</c> is returned.</remarks>
        /// </summary>
        public object this[string key]
        {
            get
            {
                _dictionary.TryGetValue(key, out object result);
                return result;
            }

            set => _dictionary[key] = value;
        }

        /// <summary>
        /// Gets an <see cref="ICollection{String}"/> containing the keys of this instance.
        /// </summary>
        public ICollection<string> Keys => _dictionary.Keys;

        /// <summary>
        /// Gets an <see cref="ICollection{Object}"/> containing the values of this instance.
        /// </summary>
        public ICollection<object> Values => _dictionary.Values;

        ICollection<string> IDictionary<string, object>.Keys => throw new NotImplementedException();

        ICollection<object> IDictionary<string, object>.Values => throw new NotImplementedException();

        int ICollection<KeyValuePair<string, object>>.Count => throw new NotImplementedException();

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly => throw new NotImplementedException();

        object IDictionary<string, object>.this[string key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <summary>
        /// Attempts to get the member.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (_dictionary.ContainsKey(binder.Name))
            {
                result = _dictionary[binder.Name];
                return true;
            }

            if (base.TryGetMember(binder, out result))
            {
                return true;
            }

            // always return null if not found.
            result = null;
            return true;
        }

        /// <summary>
        /// Attempts to set the member.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool TrySetMember(SetMemberBinder binder, object result)
        {
            if (!_dictionary.ContainsKey(binder.Name))
            {
                _dictionary.Add(binder.Name, result);
            }
            else
            {
                _dictionary[binder.Name] = result;
            }
            return true;
        }

        /// <summary>
        /// Attempts to invoke the member.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (_dictionary.ContainsKey(binder.Name) && _dictionary[binder.Name] is Delegate)
            {
                var del = (Delegate)_dictionary[binder.Name];
                result = del.DynamicInvoke(args);
                return true;
            }

            return base.TryInvokeMember(binder, args, out result);
        }

        /// <summary>
        /// Attempts to delete the member.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool TryDeleteMember(DeleteMemberBinder binder)
        {
            if (_dictionary.ContainsKey(binder.Name))
            {
                _dictionary.Remove(binder.Name);
                return true;
            }

            return base.TryDeleteMember(binder);
        }

        /// <summary>
        /// Returns the enumeration of all dynamic member names.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override IEnumerable<string> GetDynamicMemberNames() => _dictionary.Keys;

        void IDictionary<string, object>.Add(string key, object value)
        {
            throw new NotImplementedException();
        }

        bool IDictionary<string, object>.ContainsKey(string key)
        {
            throw new NotImplementedException();
        }

        bool IDictionary<string, object>.Remove(string key)
        {
            throw new NotImplementedException();
        }

        bool IDictionary<string, object>.TryGetValue(string key, out object value)
        {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<string, object>>.Clear()
        {
            throw new NotImplementedException();
        }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}