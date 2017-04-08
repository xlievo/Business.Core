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

using Business.Attributes;

namespace Business.Auth
{
    [ProtoBuf.ProtoContract(SkipConstructor = true)]
    public class Session
    {
        public static implicit operator Session(string value)
        {
            return Business.Extensions.Help.TryJsonDeserialize<Session>(value);
        }
        public static implicit operator Session(byte[] value)
        {
            return Business.Extensions.Help.TryProtoBufDeserialize<Session>(value);
        }
        public byte[] ToBytes()
        {
            return Business.Extensions.Help.ProtoBufSerialize(this);
        }
        public override string ToString()
        {
            return Business.Extensions.Help.JsonSerialize(this);
        }

        [ProtoBuf.ProtoMember(1)]
        public virtual string Account { get; set; }
        [ProtoBuf.ProtoMember(2)]
        public virtual string Password { get; set; }
        [ProtoBuf.ProtoMember(3)]
        public virtual string Remote { get; set; }
        [ProtoBuf.ProtoMember(4)]
        public virtual string Key { get; set; }
        [ProtoBuf.ProtoMember(5)]
        public virtual System.DateTime Time { get; set; }
        [ProtoBuf.ProtoMember(6)]
        public virtual string Source { get; set; }
        [ProtoBuf.ProtoMember(7)]
        public virtual string Nick { get; set; }
        [ProtoBuf.ProtoMember(8)]
        public virtual bool Activate { get; set; }
        [ProtoBuf.ProtoMember(9)]
        public virtual string Email { get; set; }
        [ProtoBuf.ProtoMember(10)]
        public virtual string Json { get; set; }
        [ProtoBuf.ProtoMember(11)]
        public virtual string Phone { get; set; }
        [ProtoBuf.ProtoMember(12)]
        public virtual System.Collections.Generic.List<string> Role { get; set; }
    }
}