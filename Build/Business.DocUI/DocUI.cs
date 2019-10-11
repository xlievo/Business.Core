namespace Business
{
    using System.Linq;

    public class DocUI
    {
        static readonly bool Unix = false;
        const string Csproj = "Business.DocUI.csproj";
        static readonly string Index = "doc\\index.html";
        static readonly System.Collections.Generic.Dictionary<string, System.IO.Stream> Doc = new System.Collections.Generic.Dictionary<string, System.IO.Stream>();

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
        }

        public static void Write(string outDir = "wwwroot", string docRequestPath = null, bool update = false)
        {
            outDir = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, string.IsNullOrWhiteSpace(outDir) ? "wwwroot" : outDir);

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
    }
}
