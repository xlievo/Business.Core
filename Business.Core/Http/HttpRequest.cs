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

namespace Business.Http
{
    using System.Linq;
    using Utils;

    #region Result

    public struct FileResult
    {
        public string FilePath;

        public string ContentType;
    }

    public struct StreamResult
    {
        public System.Byte[] Stream;

        public string ContentType;
    }

    public struct HttpResult
    {
        public string Contents;

        public string ContentType;
    }

    public struct RedirectResult
    {
        public RedirectResult(string location, RedirectType type = RedirectType.SeeOther)
        {
            this.Location = location;
            this.Type = type;
        }

        public string Location { get; }

        /// <summary>
        /// type of redirect
        /// </summary>
        public RedirectType Type { get; }
    }

    /// <summary>
    /// Which type of redirect
    /// </summary>
    public enum RedirectType
    {
        /// <summary>
        /// HTTP 301 - All future requests should be to this URL
        /// </summary>
        Permanent = 0,
        /// <summary>
        /// HTTP 307 - Redirect this request but allow future requests to the original URL
        /// </summary>
        Temporary = 1,
        /// <summary>
        /// HTTP 303 - Redirect this request using an HTTP GET
        /// </summary>
        SeeOther = 2,
    }

    #endregion

    /// <summary>
    /// Http
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Assembly | System.AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class HttpAttribute : Attributes.AttributeBase
    {
        #region Default

        /// <summary>
        /// Web Service
        /// </summary>
        public const string DefaultDescription = "Web Service";
        /// <summary>
        /// http://localhost:8100
        /// </summary>
        public const string DefaultHost = "http://localhost:8100";
        /// <summary>
        /// 8100
        /// </summary>
        public const int DefaultPort = 8100;
        /// <summary>
        /// *
        /// </summary>
        public const string DefaultAllowedOrigins = "*";
        /// <summary>
        /// GET, POST, PUT, DELETE, PATCH, OPTIONS
        /// </summary>
        public const string DefaultAllowedMethods = "GET, POST, PUT, DELETE, PATCH, OPTIONS";
        /// <summary>
        /// Content-Type
        /// </summary>
        public const string DefaultAllowedHeaders = "Content-Type";
        /// <summary>
        /// false
        /// </summary>
        public const bool DefaultAllowCredentials = false;
        /// <summary>
        /// application/json;charset=utf-8
        /// </summary>
        public const string DefaultResponseContentType = "application/json;charset=utf-8";

        #endregion

        /// <summary>
        /// Constructor, assembly attribute.
        /// </summary>
        /// <param name="host">http://localhost:8100</param>
        /// <param name="allowedOrigins">*</param>
        /// <param name="allowedMethods">GET, POST, PUT, DELETE, PATCH, OPTIONS</param>
        /// <param name="allowedHeaders">Content-Type</param>
        /// <param name="allowCredentials">false</param>
        /// <param name="responseContentType">application/json;charset=utf-8</param>
        /// <param name="description">Web Service</param>
        [Newtonsoft.Json.JsonConstructor]
        public HttpAttribute(string host = DefaultHost, string allowedOrigins = DefaultAllowedOrigins, string allowedMethods = DefaultAllowedMethods, string allowedHeaders = DefaultAllowedHeaders, bool allowCredentials = DefaultAllowCredentials, string responseContentType = DefaultResponseContentType, string description = DefaultDescription)
        {
            defaultConstructor = true;

            this.Host = host;
            this.AllowedOrigins = allowedOrigins;
            this.AllowedMethods = allowedMethods;
            this.AllowedHeaders = allowedHeaders;
            this.AllowCredentials = allowCredentials;
            this.ResponseContentType = responseContentType;
            this.Description = description;
        }

        /// <summary>
        /// Constructor, class attribute.
        /// </summary>
        /// <param name="port">8100</param>
        public HttpAttribute(int port) : this()
        {
            this.Port = port;
            defaultConstructor = false;
        }

        internal readonly bool defaultConstructor;

        internal void Set(string host = DefaultHost, string allowedOrigins = DefaultAllowedOrigins, string allowedMethods = DefaultAllowedMethods, string allowedHeaders = DefaultAllowedHeaders, bool allowCredentials = DefaultAllowCredentials, string responseContentType = DefaultResponseContentType, string description = DefaultDescription)
        {
            this.Host = host;
            this.AllowedOrigins = allowedOrigins;
            this.AllowedMethods = allowedMethods;
            this.AllowedHeaders = allowedHeaders;
            this.AllowCredentials = allowCredentials;
            this.ResponseContentType = responseContentType;
            this.Description = description;
        }

        /// <summary>
        /// Url
        /// </summary>
        public System.Uri Url { get; private set; }

        /// <summary>
        /// Add string to end
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public System.String UrlEnd(params string[] value) => null != Url ? string.Format("{0}{1}", Url.AbsoluteUri.TrimEnd('/'), null != value && value.Any(c => !System.String.IsNullOrWhiteSpace(c)) ? string.Format("/{0}", string.Join("/", value.Where(c => !System.String.IsNullOrWhiteSpace(c)))) : System.String.Empty) : System.String.Empty;

        string host = DefaultHost;
        /// <summary>
        /// Host address default localhost
        /// </summary>
        public string Host
        {
            get => host; private set
            {
                if (System.String.IsNullOrWhiteSpace(value)) { throw new System.ArgumentNullException(nameof(Host)); }

                if (!System.Uri.TryCreate(value, System.UriKind.RelativeOrAbsolute, out System.Uri url)) { throw new System.ArgumentException(nameof(Host)); }

                if (!defaultConstructor)
                {
                    Url = new System.Uri(string.Format("{0}://{1}:{2}{3}", url.Scheme, url.Host, port, url.AbsolutePath));
                }
                else
                {
                    Url = url;
                    port = Url.Port;
                }

                host = value;
            }
        }

        int port = DefaultPort;
        /// <summary>
        /// 8100
        /// </summary>
        public int Port
        {
            get => port; private set
            {
                if (!System.Uri.TryCreate(host, System.UriKind.RelativeOrAbsolute, out System.Uri url)) { throw new System.ArgumentException(nameof(Port)); }

                Url = new System.Uri(string.Format("{0}://{1}:{2}{3}", url.Scheme, url.Host, value, url.AbsolutePath));

                port = value;
            }
        }

        /// <summary>
        /// *
        /// </summary>
        public string AllowedOrigins { get; private set; }

        /// <summary>
        /// GET, POST, PUT, DELETE, PATCH, OPTIONS
        /// </summary>
        public string AllowedMethods { get; private set; }

        /// <summary>
        /// Content-Type
        /// </summary>
        public string AllowedHeaders { get; private set; }

        /// <summary>
        /// false
        /// </summary>
        public bool AllowCredentials { get; private set; }

        /// <summary>
        /// application/json;charset=utf-8
        /// </summary>
        public string ResponseContentType { get; private set; }

        /// <summary>
        /// Web Service
        /// </summary>
        public string Description { get; private set; }
    }

    /// <summary>
    /// Http Route
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Assembly | System.AttributeTargets.Class | System.AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RouteAttribute : Attributes.CommandAttribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path"></param>
        public RouteAttribute(string path = null)
            : this(path, null) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path"></param>
        /// <param name="verbs"></param>
        /// <param name="group"></param>
        [Newtonsoft.Json.JsonConstructor]
        public RouteAttribute(string path, string verbs, string group = Bind.CommandGroupDefault)
        {
            Path = path;
            Verbs = verbs;
            Group = group;
            Root = !Path.StartsWith("~");
            if (Root) { Path = Path.TrimStart('~'); }
        }

        string path;
        public string Path { get => path; set => this.OnlyName = path = value?.Trim('/'); }

        public string Verbs { get; set; }

        public string Group { get; set; }

        public string Cmd { get; set; }

        public bool Root { get; set; }

        internal string MethodName { get; set; }

        /// <summary>
        /// space
        /// </summary>
        public const string Space = "->";

        //public override string GetKey(string space = Space) => base.GetKey(space);

        //internal virtual string GetKey(bool all = false, string space = Space) => all ? string.Format("{1}{0}{2}{0}{3}{0}{4}", space, this.Path, this.Verbs, this.Root, this.Group) : string.Format("{1}{0}{2}{0}{3}", space, this.Path, this.Verbs, this.Root);

        internal virtual string GetKey(bool all = false, string space = Space) => GetKey(this.Path, this.Verbs, this.Root, all ? this.Group : null);

        //public virtual string GetKey(string path, string verbs, bool root, string group = null, bool all = false, string space = Space) => all ? string.Format("{1}{0}{2}{0}{3}{0}{4}", space, path, verbs, root, group) : string.Format("{1}{0}{2}{0}{3}", space, path, verbs, root);
        public static string GetKey(string path, string verbs, bool root, string group = null, string space = Space)
        {
            return string.Join(space, System.Array.FindAll(new string[] { path, verbs, root.ToString(), group }, c => null != c));

            //return string.Format("{1}{0}{2}{0}{3}{4}", space, path, verbs, root, !System.String.IsNullOrWhiteSpace(group) ? string.Format("{0}{1}", space, group) : null);
        }

        public override T Clone<T>() => new RouteAttribute { Cmd = this.Cmd, Group = this.Group, MethodName = this.MethodName, Path = this.Path, Root = this.Root, Verbs = this.Verbs } as T;
    }

    /// <summary>
    /// Http POST
    /// </summary>
    public sealed class POSTAttribute : RouteAttribute
    {
        public POSTAttribute(string path = null, string group = Bind.CommandGroupDefault) : base(path, "POST", group) { }

        //public POSTAttribute(bool root, string path = null, string group = Bind.CommandGroupDefault) : this(path, root, group) { }
    }

    /// <summary>
    /// Http GET
    /// </summary>
    public sealed class GETAttribute : RouteAttribute
    {
        public GETAttribute(string path = null, string group = Bind.CommandGroupDefault) : base(path, "GET", group) { }

        //public GETAttribute(bool root, string path = null, string group = Bind.CommandGroupDefault) : this(path, root, group) { }
    }

    /// <summary>
    /// Represents a file that was captured in a HTTP multipart/form-data request
    /// </summary>
    public struct HttpFile
    {
        public HttpFile(string contentType, string name, string key, System.IO.Stream value)
        {
            this.contentType = contentType;
            this.name = name;
            this.key = key;
            this.value = value;
        }

        readonly string contentType;
        /// <summary>
        /// Gets or sets the type of the content.
        /// </summary>
        public string ContentType { get => contentType; }

        readonly string name;
        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        public string Name { get => name; }

        readonly string key;
        /// <summary>
        /// Gets or sets the form element name of this file.
        /// </summary>
        public string Key { get => key; }

        readonly System.IO.Stream value;
        /// <summary>
        /// This is request stream.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public System.IO.Stream Value { get => value; }
    }

    /// <summary>
    /// HttpFile
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Parameter | System.AttributeTargets.Class | System.AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public class HttpFileAttribute : Attributes.ArgumentAttribute
    {
        /// <summary>
        /// KB
        /// </summary>
        public int Size { get; set; }

        public HttpFileAttribute(int size = 4096, int code = -906, string message = null, bool canNull = true) : base(code, message, canNull) => this.Size = size;

        public override Result.IResult Proces(dynamic value)
        {
            var result = CheckNull(this, value);
            if (!result.HasData) { return result; }

            HttpRequest httpRequest = value;

            if (System.Object.Equals(null, httpRequest) || System.Object.Equals(null, httpRequest.Files) || !httpRequest.Files.Any())
            {
                if (this.CanNull)
                {
                    return this.ResultCreate();
                }
                else
                {
                    return this.ResultCreate(State, Message ?? string.Format("argument \"{0}\" can not null.", this.Member));
                }
            }

            System.Collections.Generic.IEnumerable<HttpFile> values = httpRequest.Files;

            foreach (var item in values)
            {
                if (Size < (item.Value.Length / 1024)) { return this.ResultCreate(-413, string.Format("File max size {0} KB.", Size)); }
            }

            var files = values.Distinct(Equality<HttpFile>.CreateComparer(c => c.Key)).ToList();//.ToDictionary(c => c.Key, c => c.Value.StreamReadByte());

            httpRequest.Files = files;

            return this.ResultCreate();
        }
    }

    /// <summary>
    /// Http request
    /// </summary>
    public interface IHttpRequest
    {
        /// <summary>
        /// Gets the HTTP headers sent by the client.
        /// </summary>
        System.Collections.Generic.IDictionary<string, string> Headers { get; set; }

        /// <summary>
        /// Gets a Nancy.IO.RequestStream that can be used to read the incoming HTTP body
        /// </summary>
        byte[] Stream { get; set; }

        /// <summary>
        /// Gets a collection of files sent by the client
        /// </summary>
        System.Collections.Generic.IEnumerable<HttpFile> Files { get; set; }

        /// <summary>
        /// Gets the request cookies.
        /// </summary>
        System.Collections.Generic.IDictionary<string, string> Cookies { get; set; }

        /// <summary>
        /// Gets or sets the HTTP data transfer method used by the client.
        /// </summary>
        string Method { get; set; }

        /// <summary>
        /// Gets the url
        /// </summary>
        string Url { get; set; }

        /// <summary>
        /// Gets the IP address of the client
        /// </summary>
        string Remote { get; set; }
    }

    /// <summary>
    /// Http request
    /// </summary>
    public class HttpRequest : IHttpRequest
    {
        /// <summary>
        /// Gets the HTTP headers sent by the client.
        /// </summary>
        public System.Collections.Generic.IDictionary<string, string> Headers { get; set; }

        /// <summary>
        /// Gets a Nancy.IO.RequestStream that can be used to read the incoming HTTP body
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public byte[] Stream { get; set; }

        /// <summary>
        /// Gets a collection of files sent by the client
        /// </summary>
        public System.Collections.Generic.IEnumerable<HttpFile> Files { get; set; }

        /// <summary>
        /// Gets the request cookies.
        /// </summary>
        public System.Collections.Generic.IDictionary<string, string> Cookies { get; set; }

        /// <summary>
        /// Gets or sets the HTTP data transfer method used by the client.
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Gets the url
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets the IP address of the client
        /// </summary>
        public string Remote { get; set; }
    }
}
