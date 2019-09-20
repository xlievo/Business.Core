namespace Business.DocUI
{
    using System.Linq;

    public class UI
    {
        const string Csproj = "Business.DocUI.csproj";
        const string Index = "doc\\index.html";
        static readonly System.Collections.Generic.Dictionary<string, System.IO.Stream> Doc = new System.Collections.Generic.Dictionary<string, System.IO.Stream>();

        static UI()
        {
            var ass = System.Reflection.Assembly.GetExecutingAssembly();
            var assName = ass.GetName().Name;
            var csproj = $"{assName}.{Csproj}";

            var resourceList = System.Xml.Linq.XDocument.Load(ass.GetManifestResourceStream(csproj)).Descendants("EmbeddedResource").Select(c2 => c2.Attribute("Include")?.Value).Where(c2 => null != c2 && Csproj != c2).ToDictionary(c =>
            {
                var file = System.IO.Path.GetFileName(c);
                var c2 = c.Substring(0, c.Length - file.Length);
                var c3 = c2.Replace(".", "._").Replace("-", "_").Replace("\\", ".");
                return $"{c3}{file}";
            }, c => c);

            var start = $"{assName}.";

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

        public static void WritePage(string docRequestPath, string pageOutDir = null)
        {
            if (string.IsNullOrWhiteSpace(docRequestPath))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(pageOutDir))
            {
                pageOutDir = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "wwwroot");
            }

            if (!System.IO.Directory.Exists(pageOutDir))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(pageOutDir);
                }
                catch (System.Exception ex)
                {
                    System.Console.WriteLine(ex);
                    return;
                }
            }

            Doc.AsParallel().ForAll(c =>
            {
                var path = System.IO.Path.Combine(pageOutDir, c.Key);
                var dir = System.IO.Path.GetDirectoryName(path);

                if (!System.IO.Directory.Exists(dir))
                {
                    System.IO.Directory.CreateDirectory(dir);
                }

                var ex = System.IO.Path.GetExtension(c.Key);

                if (".html" == ex || ".js" == ex || ".css" == ex || ".map" == ex)
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

        public static string StreamReadString(System.IO.Stream stream)
        {
            using (var reader = new System.IO.StreamReader(stream, System.Text.Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        public static byte[] StreamReadByte(System.IO.Stream stream)
        {
            if (null == stream) { throw new System.ArgumentNullException(nameof(stream)); }

            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            return bytes;
        }
    }
}
