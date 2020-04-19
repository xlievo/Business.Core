using Business.Core.Result;
using System.Threading.Tasks;

namespace Business.Core.Annotations
{
    public class MessagePackArg : ArgumentAttribute
    {
        public MessagePackArg(int state = -13, string message = null) : base(state, message)
        {
            this.CanNull = false;
            this.Description = "MessagePackArg Binary parsing";
            this.ArgMeta.Filter |= FilterModel.NotDefinition;
        }

        public override async ValueTask<IResult> Proces<Type>(dynamic value)
        {
            var result = CheckNull(this, value);
            if (!result.HasData) { return result; }

            try
            {
                return this.ResultCreate(MessagePack.MessagePackSerializer.Deserialize<Type>(value));
            }
            catch (System.Exception ex) { return this.ResultCreate(State, Message ?? $"Arguments {this.Alias} MessagePack deserialize error. {ex.Message}"); }
        }
    }

    public class JsonArg : JsonArgAttribute
    {
        public JsonArg(int state = -12, string message = null) : base(state, message)
        {
            this.CanNull = false;
            this.Description = "Json parsing";
        }
    }
    public class JsonArg2 : JsonArg
    {
        public JsonArg2(int state = -112, string message = null) : base(state, message)
        {
            this.CanNull = false;
            this.Description = "Json2 parsing";
        }
    }
    /*
    public class HttpFile : HttpFileAttribute
    {
        public HttpFile(int state = 830, string message = null) : base(state, message)
        {
            this.Description = $"Http file upload";
        }
    }
    */
    public class CheckNull : CheckNullAttribute
    {
        public CheckNull(int state = -800, string message = null) : base(state, message)
        {
            this.CanNull = false;
            this.Description = "{Alias} must be filled in";
            this.Message = "{Alias} must be filled in";
        }
    }

    public class Size : SizeAttribute
    {
        public Size(int state = -801, string message = null) : base(state, message)
        {
            this.Description = "{Alias} Range [{Min} {Max}]";
            this.Message = "{Alias} Range [{Min} {Max}]";
        }
    }

    public class Scale : ScaleAttribute
    {
        public Scale(int state = -802, string message = null) : base(state, message)
        {
            this.Description = "{Alias} Intercept the last {Size} bits";
            this.Message = "{Alias} Intercept the last {Size} bits";
        }
    }

    public class CheckEmail : CheckEmailAttribute
    {
        public CheckEmail(int state = -803, string message = null) : base(state, message)
        {
            this.Description = "{Alias} Check the validity of email address";
            this.Message = "{Alias} Illegal mail address";
        }
    }

    public class CheckChar : CheckCharAttribute
    {
        public CheckChar(int state = -804, string message = null) : base(state, message)
        {
            this.Description = "{Alias} Check character legitimacy";
            this.Message = "{Alias} Illegal character";
        }
    }

    public class CheckPhone : CheckPhoneAttribute
    {
        public CheckPhone(int state = -806, string message = null) : base(state, message)
        {
            this.Description = "{Alias} Check the validity of phone number";
            this.Message = "{Alias} Illegal phone number";
        }
    }

    public class CheckUrl : CheckUrlAttribute
    {
        public CheckUrl(int state = -807, string message = null) : base(state, message)
        {
            this.Description = "{Alias} Check the validity of url";
            this.Message = "{Alias} Illegal url";
        }
    }

    public class MD5 : MD5Attribute
    {
        public MD5(int state = -820, string message = null) : base(state, message)
        {
            this.Description = "MD5";
        }
    }

    public class AES : AESAttribute
    {
        public AES(string key = null, int state = -821, string message = null) : base(key, state, message)
        {
            this.Description = "AES";
        }
    }

    public class SessionCheck : ArgumentAttribute
    {
        public SessionCheck(int state = -81, string message = null) : base(state, message)
        {
            this.CanNull = false;
            this.Description = "Session check";
        }

        public override async ValueTask<IResult> Proces<Type>(dynamic token, dynamic value)
        {
            var result = CheckNull(this, value);
            if (!result.HasData) { return result; }

            var key = value.Key as string;

            //..1: check 2: convert

            var session = new Session { Account = key };

            return this.ResultCreate(session);//return out session
        }
    }

    public class TokenCheck : ArgumentAttribute
    {
        public TokenCheck(int state = -80, string message = null) : base(state, message)
        {
            this.CanNull = false;
            this.Description = "Token check";
            this.Message = "Token is null";
        }

        public override async ValueTask<IResult> Proces(dynamic value)
        {
            var result = CheckNull(this, value);
            if (!result.HasData) { return result; }

            var key = value.Key as string;

            //..1: check token key

            if (string.IsNullOrWhiteSpace(key))
            {
                //return this.ResultCreate(this.State, this.Message);
            }

            return this.ResultCreate(); //ok
        }
    }
}
