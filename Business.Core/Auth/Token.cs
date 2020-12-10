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

namespace Business.Core.Auth
{
    /// <summary>
    /// IToken
    /// </summary>
    public interface IToken
    {
        /// <summary>
        /// Key
        /// </summary>
        string Key { get; set; }

        /// <summary>
        /// Remote
        /// </summary>
        Remote Remote { get; set; }

        //System.Net.IPEndPoint Remote2 { get; set; }

        /// <summary>
        /// Callback
        /// </summary>
        string Callback { get; set; }
    }

    //public enum Origin
    //{
    //    Default,

    //    Http,

    //    Socket,

    //    WebSocket,

    //    SOAP,
    //}

    /// <summary>
    /// A token sample
    /// </summary>
    [Annotations.Use]
    [Annotations.Logger(canWrite: false)]
    public struct Token : IToken
    {
        //public static implicit operator Token(string value) => new Token { Key = value };
        //public static implicit operator Token(byte[] value) => Utils.Help.TryProtoBufDeserialize<Token>(value);

        /// <summary>
        /// JSON format
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Utils.Help.JsonSerialize(this);

        ///// <summary>
        ///// ProtoBuf format
        ///// </summary>
        ///// <returns></returns>
        //public byte[] ToBytes() => throw new System.NotImplementedException(); //Utils.Help.ProtoBufSerialize(this);

        /// <summary>
        /// The user token
        /// </summary>
        //[Newtonsoft.Json.JsonProperty(PropertyName = "K")]
        [System.Text.Json.Serialization.JsonPropertyName("K")]
        public string Key { get; set; }

        /// <summary>
        /// Remote IP address
        /// </summary>
        //[Newtonsoft.Json.JsonProperty(PropertyName = "R")]
        [System.Text.Json.Serialization.JsonPropertyName("R")]
        public Remote Remote { get; set; }

        ///// <summary>
        ///// Socket identity
        ///// </summary>
        //[Newtonsoft.Json.JsonIgnore]
        //public virtual string CommandID { get; set; }

        /// <summary>
        /// Gets the token of this result, used for callback
        /// </summary>
        //[Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string Callback { get; set; }
    }

    /// <summary>
    /// Remote
    /// </summary>
    public struct Remote
    {
        /// <summary>
        /// Remote
        /// </summary>
        /// <param name="address">An IP address.</param>
        /// <param name="port">The port number associated with the address, or 0 to specify any available port. port is in host order.</param>
        public Remote(string address, int port = default)
        {
            this.Address = address;
            this.Port = port;
        }

        /// <summary>
        /// Address
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Port
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns>true if obj and this instance are the same type and represent the same value; otherwise, false.</returns>
        public override bool Equals(object obj) => GetHashCode().Equals(obj.GetHashCode());

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode() => this.ToString().GetHashCode();

        /// <summary>
        /// Returns the IP address and port number of the specified endpoint.
        /// </summary>
        /// <returns>A string containing the IP address and the port number of the specified endpoint (for example, 192.168.1.2:80).</returns>
        public override string ToString() => $"{this.Address}:{this.Port}";
    }
}
