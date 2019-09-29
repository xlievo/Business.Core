using Business.Attributes;
using Business.Result;
using System.Threading.Tasks;

namespace Business.Annotations
{
    public class MessagePackArg : ArgumentAttribute
    {
        public MessagePackArg(int state = -13, string message = null) : base(state, message)
        {
            this.CanNull = false;
            this.Description = "MessagePackArg Binary parsing";
        }

        public override async ValueTask<IResult> Proces(dynamic value, IArg arg)
        {
            var result = CheckNull(this, value);
            if (!result.HasData) { return result; }

            try
            {
                return this.ResultCreate(arg.ToOut(value));
            }
            catch { return this.ResultCreate(State, Message ?? $"Arguments {this.Nick} MessagePack deserialize error"); }
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
            this.Description = "{Nick} must be filled in";
            this.Message = "{Nick} must be filled in";
        }
    }

    public class Size : SizeAttribute
    {
        public Size(int state = -801, string message = null) : base(state, message)
        {
            this.Description = "{Nick} Range [{Min} {Max}]";
            this.Message = "{Nick} Range [{Min} {Max}]";
        }
    }

    public class Scale : ScaleAttribute
    {
        public Scale(int state = -802, string message = null) : base(state, message)
        {
            this.Description = "{Nick} Intercept the last {Size} bits";
            this.Message = "{Nick} Intercept the last {Size} bits";
        }
    }

    public class CheckEmail : CheckEmailAttribute
    {
        public CheckEmail(int state = -803, string message = null) : base(state, message)
        {
            this.Description = "{Nick} Check the validity of email address";
            this.Message = "{Nick} Illegal mail address";
        }
    }

    public class CheckChar : CheckCharAttribute
    {
        public CheckChar(int state = -804, string message = null) : base(state, message)
        {
            this.Description = "{Nick} Check character legitimacy";
            this.Message = "{Nick} Illegal character";
        }
    }

    public class MD5 : MD5Attribute
    {
        public MD5(int state = -820, string message = null) : base(state, message)
        {
            this.Description = "MD5";
        }
    }

    public class AES : Attributes.AESAttribute
    {
        public AES(string key = null, int state = -821, string message = null) : base(key, state, message)
        {
            this.Description = "AES";
        }
    }
}
