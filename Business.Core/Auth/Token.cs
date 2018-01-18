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

namespace Business.Auth
{
    public interface IToken
    {
        string Key { get; set; }

        string Remote { get; set; }

        string CommandID { get; set; }
    }

    /// <summary>
    /// A token sample
    /// </summary>
    [ProtoBuf.ProtoContract(SkipConstructor = true)]
    public class Token : IToken
    {
        public static implicit operator Token(string value) => new Token { Key = value };
        public static implicit operator Token(byte[] value) => Utils.Help.TryProtoBufDeserialize<Token>(value);

        /// <summary>
        /// JSON format
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Utils.Help.JsonSerialize(this);

        /// <summary>
        /// ProtoBuf format
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes() => Utils.Help.ProtoBufSerialize(this);

        /// <summary>
        /// The user token
        /// </summary>
        [ProtoBuf.ProtoMember(1, Name = "K")]
        [Newtonsoft.Json.JsonProperty(PropertyName = "K")]
        public virtual string Key { get; set; }

        /// <summary>
        /// Remote IP address
        /// </summary>
        [ProtoBuf.ProtoMember(2, Name = "R")]
        [Newtonsoft.Json.JsonProperty(PropertyName = "R")]
        public virtual string Remote { get; set; }

        /// <summary>
        /// Socket identity
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public virtual string CommandID { get; set; }
    }
}
