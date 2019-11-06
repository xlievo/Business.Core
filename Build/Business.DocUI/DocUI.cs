namespace Business
{
    using System.Linq;
    using System.Threading.Tasks;

    public class DocUI
    {
        static readonly bool Unix = false;
        const string Csproj = "Business.DocUI.csproj";
        /// <summary>
        /// doc\\index.html
        /// </summary>
        static readonly string Index = "doc\\index.html";
        static readonly System.Collections.Generic.Dictionary<string, System.IO.Stream> Doc = new System.Collections.Generic.Dictionary<string, System.IO.Stream>();
        static string AB = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "ab.exe");

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

            //if (!System.IO.Directory.Exists(AB))
            //{
            //    System.IO.Directory.CreateDirectory(AB);
            //}
        }

        /// <summary>
        /// outDir Default value is "System.AppDomain.CurrentDomain.BaseDirectory\wwwroot"
        /// </summary>
        /// <param name="outDir"></param>
        /// <param name="docRequestPath"></param>
        /// <param name="update"></param>
        public static void Write(string outDir = null, string docRequestPath = null, bool update = false)
        {
            if (string.IsNullOrWhiteSpace(outDir))
            {
                outDir = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "wwwroot");
            }

            if (!update)
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
                                System.IO.File.WriteAllBytes(AB, StreamReadByte(c.Value));
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

        /*
        macOS: ~
        Alpine: apk add apache2-utils
        CentOS/RHEL: yum -y install httpd-tools
        Ubuntu: apt-get install apache2-utils
        */
        public static async Task<string> ab(int n, int c, string data, string host)
        {
            if (!Unix && !System.IO.File.Exists(AB))
            {
                return $"{AB} not exist!";
            }

            System.Console.WriteLine($"n:{n} c:{c}");

            //var dir = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "ab");

            //if (!System.IO.Directory.Exists(dir))
            //{
            //    System.IO.Directory.CreateDirectory(dir);
            //}

            var dataPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, $"ab_{System.Guid.NewGuid().ToString("N")}");

            try
            {
                //System.IO.File.WriteAllText(dataPath, System.Net.WebUtility.UrlEncode(data), System.Text.Encoding.UTF8);
                System.IO.File.WriteAllText(dataPath, data, System.Text.Encoding.UTF8);

                using (var cmd = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(!Unix ? AB : "ab", $"-n {n} -c {c} -p \"{dataPath}\" -T \"application/x-www-form-urlencoded\" \"{host}\"") { RedirectStandardOutput = true }))
                //using (var cmd = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(!Unix ? AB : "ab", "-n 10 -c 10 http://localhost:5000/API/") { RedirectStandardOutput = true }))
                {
                    using (var output = cmd.StandardOutput)
                    {
                        return await output.ReadToEndAsync();
                    }
                }
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
    }
}
