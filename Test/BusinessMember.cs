using Business;
using Business.Attributes;
using Business.Result;
using Business.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

public class ResultObject<Type> : Business.Result.ResultObject<Type>
{
    static ResultObject() => MessagePack.Resolvers.CompositeResolver.RegisterAndSetAsDefault(MessagePack.Resolvers.ContractlessStandardResolver.Instance);

    public ResultObject(Type data, System.Type dataType, int state = 1, string message = null, System.Type genericType = null)
        : base(data, dataType, state, message, genericType) { }

    public ResultObject(Type data, int state = 1, string message = null) : this(data, null, state, message) { }

    [MessagePack.IgnoreMember]
    public override System.Type DataType { get => base.DataType; set => base.DataType = value; }

    [MessagePack.IgnoreMember]
    public override System.Type GenericType => base.GenericType;

    public override byte[] ToBytes() => MessagePack.MessagePackSerializer.Serialize(this);
}

[Logger]
public class BusinessMember : BusinessBase<ResultObject<object>>
{
    public BusinessMember()
    {
        this.Logger = x =>
        {
            try
            {
                x.Value = x.Value?.ToValue();

                var log = x.JsonSerialize();

                Help.WriteLocal(log, console: true, write: x.Type == LoggerType.Exception);
            }
            catch (Exception exception)
            {
                Help.ExceptionWrite(exception, true, true);
            }
        };

        this.BindAfter = () =>
        {

        };

        //this.Config += cfg => cfg.UseType(typeof(BusinessController));
    }

    public class A2 : ArgumentAttribute
    {
        public A2(int state = -110, string message = null, bool canNull = true) : base(state, message, canNull)
        {
        }

        public async override Task<IResult> Proces(dynamic value)
        {
            return this.ResultCreate(string.Format("{0}+{1}", value, "1234567890"));
        }
    }

    [JsonArg]
    public struct Ags2
    {
        [A2]
        public string A2 { get; set; }

        public Ags3 B2 { get; set; }
    }

    public class Ags3
    {
        //[A2]
        public string A3 { get; set; }

        public Ags4 B3 { get; set; }
    }

    public struct Ags4
    {
        [A2]
        public string A4 { get; set; }

        public string B4 { get; set; }
    }

    public struct Files
    {
        public string Key { get; set; }

        public string Name { get; set; }

        public byte[] File { get; set; }
    }

    public class FileCheck : ArgumentAttribute
    {
        public FileCheck(int state = -110, string message = null, bool canNull = true) : base(state, message, canNull) { }

        public override async Task<IResult> Proces(dynamic value)
        {
            BusinessController col = value;

            if (!col.Request.HasFormContentType) { return this.ResultCreate(); }

            var list = new List<Files>();
            foreach (var item in col.Request.Form.Files)
            {
                using (var m = new System.IO.MemoryStream())
                {
                    await item.CopyToAsync(m);
                    list.Add(new Files { Name = item.FileName, Key = item.Name, File = m.GetBuffer() });
                }
            }

            return this.ResultCreate(list);
        }
    }

    public virtual async Task<dynamic> TestAgs001(BusinessController control, Arg<Ags2> a, decimal mm = 0.0234m, [FileCheck]Arg<List<Files>, BusinessController> ss = default, Business.Auth.Token token = default)
    {
        return this.ResultCreate(a.Out);

        return this.ResultCreate(new { a = a.In, Remote = string.Format("{0}:{1}", control.HttpContext.Connection.RemoteIpAddress.ToString(), control.HttpContext.Connection.RemotePort), control.Request.Cookies });

        return control.Redirect("https://www.github.com");
    }
}
