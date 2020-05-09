namespace Business
{
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// DocUI
    /// </summary>
    public class DocUI
    {
        static readonly bool Unix = false;
        const string Csproj = "Business.DocUI.csproj";
        /// <summary>
        /// doc\\index.html
        /// </summary>
        static readonly string Index = "doc\\index.html";
        static readonly System.Collections.Generic.Dictionary<string, System.IO.Stream> Doc = new System.Collections.Generic.Dictionary<string, System.IO.Stream>();
        static readonly string ab = "ab";
        static readonly string bombardier = "bombardier";

        static DocUI()
        {
            var ass = System.Reflection.Assembly.GetExecutingAssembly();
            var assName = ass.GetName().Name;
            var csproj = $"{assName}.{Csproj}";

            switch (System.Environment.OSVersion.Platform)
            {
                case System.PlatformID.MacOSX:
                case System.PlatformID.Unix:
                    Unix = true;
                    Index = "doc/index.html";
                    break;
            }

            if (!Unix)
            {
                ab = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "ab.exe");
                bombardier = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "bombardier-windows-386.exe");
            }

            var resourceList = System.Xml.Linq.XDocument.Load(ass.GetManifestResourceStream(csproj)).Descendants("EmbeddedResource")
                .Select(c2 => Unix ? c2.Attribute("Include")?.Value?.Replace("\\", "/") : c2.Attribute("Include")?.Value)
                .Where(c2 => null != c2 && Csproj != c2).ToDictionary(c =>
            {
                var file = System.IO.Path.GetFileName(c);
                //System.Console.WriteLine($"file {file}");
                var c2 = c.Substring(0, c.Length - file.Length);
                var c3 = c2.Replace(".", "._").Replace("-", "_");

                c3 = Unix ? c3.Replace("/", ".") : c3.Replace("\\", ".");

                //System.Console.WriteLine($"{c3}{file}");
                return $"{c3}{file}";
            }, c => c);

            var start = $"{assName}.";
            //resourceList.Keys.ToList().ForEach(c => System.Console.WriteLine($"key {c}"));
            //System.Console.WriteLine($"start {System.Xml.Linq.XDocument.Load(ass.GetManifestResourceStream(csproj)).ToString()}");

            foreach (var c in ass.GetManifestResourceNames())
            {
                if (csproj == c)
                {
                    continue;
                }

                var k = c.TrimStart(start.ToCharArray());

                if (resourceList.TryGetValue(k, out string v))
                {
                    if (!Doc.ContainsKey(v))
                    {
                        Doc.Add(v, ass.GetManifestResourceStream(c));
                    }
                }
                else
                {
                    System.Console.WriteLine(k);
                }
            }
        }

        /// <summary>
        /// outDir Default value is "System.AppDomain.CurrentDomain.BaseDirectory\wwwroot"
        /// </summary>
        /// <param name="outDir"></param>
        /// <param name="docRequestPath"></param>
        /// <param name="debug"></param>
        public static void Write(string outDir = null, string docRequestPath = null, bool debug = false)
        {
            if (string.IsNullOrWhiteSpace(outDir))
            {
                outDir = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "wwwroot");
            }

            if (!debug)
            {
                var pageOutDir2 = System.IO.Path.Combine(outDir, "doc");

                if (!System.IO.Directory.Exists(pageOutDir2))
                {
                    try
                    {
                        System.IO.Directory.CreateDirectory(pageOutDir2);
                    }
                    catch (System.Exception ex)
                    {
                        System.Console.WriteLine(ex);
                        return;
                    }
                }
                else
                {
                    try
                    {
                        System.IO.Directory.Delete(pageOutDir2, true);
                    }
                    catch (System.Exception ex)
                    {
                        System.Console.WriteLine(ex);
                    }

                    try
                    {
                        System.IO.Directory.CreateDirectory(pageOutDir2);
                    }
                    catch (System.Exception ex)
                    {
                        System.Console.WriteLine(ex);
                    }

                    if (!System.IO.Directory.Exists(pageOutDir2))
                    {
                        return;
                    }
                }

                Doc.AsParallel().ForAll(c =>
                {
                    var path = System.IO.Path.Combine(outDir, c.Key);
                    var dir = System.IO.Path.GetDirectoryName(path);

                    if (!System.IO.Directory.Exists(dir))
                    {
                        System.IO.Directory.CreateDirectory(dir);
                    }

                    var ext = System.IO.Path.GetExtension(c.Key);

                    if (".html" == ext || ".js" == ext || ".css" == ext || ".map" == ext)
                    {
                        var text = StreamReadString(c.Value);

                        if (Index == c.Key)
                        {
                            text = text.Replace("{URL}", docRequestPath);
                        }

                        System.IO.File.WriteAllText(path, text, System.Text.Encoding.UTF8);
                    }
                    else
                    {
                        if ("ab.exe" == c.Key)
                        {
                            if (!Unix)
                            {
                                System.IO.File.WriteAllBytes(ab, StreamReadByte(c.Value));
                            }
                            return;
                        }

                        var bytes = StreamReadByte(c.Value);
                        System.IO.File.WriteAllBytes(path, bytes);
                    }
                });
            }
            else
            {
                var path = System.IO.Path.Combine(outDir, Index);

                if (!System.IO.File.Exists(path))
                {
                    return;
                }

                var text = System.IO.File.ReadAllText(path, System.Text.Encoding.UTF8);

                text = text.Replace("{URL}", docRequestPath);

                System.IO.File.WriteAllText(path, text, System.Text.Encoding.UTF8);
            }
        }

        static string StreamReadString(System.IO.Stream stream)
        {
            using (var reader = new System.IO.StreamReader(stream, System.Text.Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        static byte[] StreamReadByte(System.IO.Stream stream)
        {
            if (null == stream) { throw new System.ArgumentNullException(nameof(stream)); }

            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            return bytes;
        }

        /// <summary>
        /// BenchmarkArg
        /// </summary>
        public struct BenchmarkArg
        {

            /// <summary>
            /// n
            /// </summary>
            public int n { get; set; }

            /// <summary>
            /// c
            /// </summary>
            public int c { get; set; }

            /// <summary>
            /// data
            /// </summary>
            public string data { get; set; }

            /// <summary>
            /// host
            /// </summary>
            public string host { get; set; }
        }
        /*
        macOS: ~
        Alpine: apk add apache2-utils
        CentOS/RHEL: yum -y install httpd-tools
        Ubuntu: apt-get install apache2-utils
        */
        /// <summary>
        /// ab
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static async Task<string> Benchmark(BenchmarkArg arg)
        {
            if (default(BenchmarkArg).Equals(arg)) { return new System.ArgumentNullException(nameof(arg)).Message; }

            if (!Unix && !System.IO.File.Exists(ab))
            {
                return $"{ab} not exist!";
            }

            //System.Console.WriteLine($"n:{arg.n} c:{arg.c}");

            var dataPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, $"ab_{System.Guid.NewGuid():N}");

            System.IO.File.WriteAllText(dataPath, arg.data, System.Text.Encoding.UTF8);

            try
            {
                var args = $"-n {arg.n} -c {arg.c} -p \"{dataPath}\" -T \"application/x-www-form-urlencoded\" \"{arg.host}\"";

                using (var cmd = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(ab, args) { RedirectStandardOutput = true }))
                {
                    using (var output = cmd.StandardOutput)
                    {
                        return await output.ReadToEndAsync();
                    }
                }
            }
            catch (System.Exception ex)
            {
                return System.Convert.ToString(ex);
            }
            finally
            {
                try
                {
                    System.IO.File.Delete(dataPath);
                }
                catch (System.Exception ex)
                {
                    System.Console.WriteLine(ex);
                }
            }
        }

        /*
        /// <summary>
        /// bombardier https://github.com/codesenberg/bombardier
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static async Task<string> Benchmark2(BenchmarkArg arg)
        {
            if (default(BenchmarkArg).Equals(arg)) { return new System.ArgumentNullException(nameof(arg)).Message; }

            if (!Unix && !System.IO.File.Exists(ab))
            {
                return $"{ab} not exist!";
            }

            //System.Console.WriteLine($"n:{arg.n} c:{arg.c}");

            try
            {
                //https://github.com/codesenberg/bombardier
                //bombardier -m POST -H "Content-Type: application/x-www-form-urlencoded; charset=UTF-8" -b "action=get_data&number=1234" http://le-host.fr/other/parts/of/the/url
                //var data = System.Text.Json.JsonSerializer.Serialize(arg.data,);
                var args = $"-n {arg.n} -c {arg.c}  -m POST -b \"{arg.data}\" {arg.host}";
                //System.Console.WriteLine(args);

                using (var cmd = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(bombardier, args) { RedirectStandardOutput = true }))
                {
                    using (var output = cmd.StandardOutput)
                    {
                        return await output.ReadToEndAsync();
                    }
                }
            }
            catch (System.Exception ex)
            {
                return System.Convert.ToString(ex);
            }
        }
        */
    }
}
