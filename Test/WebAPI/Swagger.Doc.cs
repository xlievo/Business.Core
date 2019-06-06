namespace Swagger.Doc
{
    public class Info
    {
        /// <summary>
        /// This is a sample server Petstore server.  You can find out more about     Swagger at [http://swagger.io](http://swagger.io) or on [irc.freenode.net, #swagger](http://swagger.io/irc/).      For this sample, you can use the api key `special-key` to test the authorization     filters.
        /// </summary>
        public string description { get; set; } = System.String.Empty;
        /// <summary>
        /// 1.0.0
        /// </summary>
        public string version { get; set; } = System.String.Empty;
        /// <summary>
        /// Swagger Petstore
        /// </summary>
        public string title { get; set; } = System.String.Empty;

        //public License license { get; set; }

        public class License
        {
            public string name { get; set; } = "Apache 2.0";

            public string url { get; set; } = "http://www.apache.org/licenses/LICENSE-2.0.html";
        }
    }

    public class ExternalDocs
    {
        /// <summary>
        /// Find out more
        /// </summary>
        public string description { get; set; } = System.String.Empty;
        /// <summary>
        /// http://swagger.io
        /// </summary>
        public string url { get; set; } = "http://";
    }

    public class Tags
    {
        /// <summary>
        /// pet
        /// </summary>
        public string name { get; set; } = System.String.Empty;
        /// <summary>
        /// Everything about your Pets
        /// </summary>
        public string description { get; set; } = System.String.Empty;
        /// <summary>
        /// ExternalDocs
        /// </summary>
        public ExternalDocs externalDocs { get; set; }
    }

    /*
    public class Parameters
    {
        /// <summary>
        /// name
        /// </summary>
        public string name { get; set; } = System.String.Empty;
        /// <summary>
        /// path, formData
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "in")]
        public string _in { get; set; } = System.String.Empty;
        /// <summary>
        /// ID of pet to update
        /// </summary>
        public string description { get; set; } = System.String.Empty;
        /// <summary>
        /// Required
        /// </summary>
        public bool required { get; set; }
        /// <summary>
        /// integer
        /// </summary>
        public string type { get; set; } = System.String.Empty;
        /// <summary>
        /// int64
        /// </summary>
        public string format { get; set; } = System.String.Empty;

        /// <summary>
        /// default
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "default")]
        public string defaultValue { get; set; } = System.String.Empty;

        /// <summary>
        /// x-data
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "x-data")]
        public string data { get; set; } = System.String.Empty;
    }
    */

    public class _200
    {
        /// <summary>
        /// successful operation
        /// </summary>
        public string description { get; set; } = System.String.Empty;
    }

    public class _400
    {
        /// <summary>
        /// successful operation
        /// </summary>
        public string description { get; set; }
    }

    public class Responses
    {
        /// <summary>
        /// 200
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "200")]
        public _200 _200 { get; set; }
    }

    public class Post
    {
        /// <summary>
        /// Tags
        /// </summary>
        public string[] tags { get; set; }
        /// <summary>
        /// uploads an image
        /// </summary>
        public string summary { get; set; } = System.String.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string description { get; set; } = System.String.Empty;
        /// <summary>
        /// uploadFile
        /// </summary>
        public string operationId { get; set; } = System.String.Empty;
        /// <summary>
        /// Consumes
        /// </summary>
        public string[] consumes { get; set; }
        /// <summary>
        /// Produces
        /// </summary>
        public string[] produces { get; set; }
        /// <summary>
        /// Parameters
        /// </summary>
        public System.Collections.Generic.List<System.Collections.Generic.IDictionary<string, object>> parameters { get; set; }
        /// <summary>
        /// Responses
        /// </summary>
        public Responses responses { get; set; }
    }

    //public class pet_petId_uploadImage
    //{
    //    /// <summary>
    //    /// Post
    //    /// </summary>
    //    public Post post { get; set; }
    //}

    //public class Paths
    //{
    //    /// <summary>
    //    /// /pet/{petId}/uploadImage
    //    /// </summary>
    //    [Newtonsoft.Json.JsonProperty(PropertyName = "/pet?c={c}&t={t}&d={d}")]
    //    public pet_petId_uploadImage _pet_petId_uploadImage { get; set; }
    //}

    public class Root
    {
        /// <summary>
        /// 2.0
        /// </summary>
        public string swagger { get; set; } = "2.0";

        /// <summary>
        /// Info
        /// </summary>
        public Info info { get; set; }

        /// <summary>
        /// localhost
        /// </summary>
        public string host { get; set; } = System.String.Empty;

        /// <summary>
        /// /v1
        /// </summary>
        public string basePath { get; set; } = "/";

        /// <summary>
        /// Tags
        /// </summary>
        public Tags[] tags { get; set; }

        /// <summary>
        /// Schemes
        /// </summary>
        public string[] schemes { get; set; } = { "https", "http" };

        /// <summary>
        /// Paths
        /// </summary>
        public dynamic paths { get; set; }

        /// <summary>
        /// ExternalDocs
        /// </summary>
        public ExternalDocs externalDocs { get; set; }
    }
}