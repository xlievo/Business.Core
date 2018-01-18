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

namespace Business.Request
{
    using Business.Result;
    using System.Linq;

    public struct Mark
    {
        #region MarkEnum

        public enum MarkItem
        {
            /// <summary>
            /// Undefined Exception 0
            /// </summary>
            Exp_UndefinedException = 0,

            /// <summary>
            /// Remote Illegal -2
            /// </summary>
            Exp_RemoteIllegal = -2,
            /// <summary>
            /// Data Error -3
            /// </summary>
            Business_DataError = -3,
            /// <summary>
            /// "Command Error -4
            /// </summary>
            Business_CmdError = -4,
            /// <summary>
            /// Group Error -5
            /// </summary>
            Business_GroupError = -5,
        }

        public const string ProcesException = "Processing exception";
        public const string DataNull = "Data cannot be null";
        public const string RemoteNull = "Remote cannot be null";
        public const string CmdNull = "Cmd cannot be null";
        public const string GroupError = "Without this business group {0}";
        public const string CmdError = "Without this Cmd {0}";
        public const string DataError = "Data error";

        #endregion
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

    public interface IRequest
    {
        string Cmd { get; set; }

        dynamic Data { get; }

        string Token { get; set; }

        string Group { get; set; }

        string Remote { get; set; }

        string Callback { get; set; }

        IBusiness Business { get; set; }

        IResult Call(dynamic data, string remote = null, string group = null, string commandID = null, System.Collections.Generic.IEnumerable<HttpFile> httpFiles = null);
    }

    public interface IRequest<DataType> : IRequest
    {
        new DataType Data { get; set; }
    }

    [ProtoBuf.ProtoContract(SkipConstructor = true)]
    public class RequestObject<Type> : IRequest<Type>
    {
        string cmd;
        /// <summary>
        /// Gets the cmd of this request.
        /// </summary>
        [ProtoBuf.ProtoMember(1, Name = "C")]
        [Newtonsoft.Json.JsonProperty(PropertyName = "C")]
        public virtual string Cmd { get => cmd; set => cmd = value; }

        Type data;
        dynamic IRequest.Data { get => data; }

        /// <summary>
        /// Gets the data of this request.
        /// </summary>
        [ProtoBuf.ProtoMember(2, Name = "D")]
        [Newtonsoft.Json.JsonProperty(PropertyName = "D")]
        public virtual Type Data { get => data; set => data = value; }

        string token;
        /// <summary>
        /// Gets the login key of this request.
        /// </summary>
        [ProtoBuf.ProtoMember(4, Name = "T")]
        [Newtonsoft.Json.JsonProperty(PropertyName = "T")]
        public virtual string Token { get => token; set => token = value; }

        string group;
        [ProtoBuf.ProtoMember(5, Name = "G")]
        [Newtonsoft.Json.JsonProperty(PropertyName = "G")]
        public virtual string Group { get => group; set => group = value; }

        string remote;
        [Newtonsoft.Json.JsonProperty(PropertyName = "R")]
        public virtual string Remote { get => remote; set => remote = value; }

        string callback;
        /// <summary>
        /// Gets the Command token of this request, used for callback
        /// </summary>
        [ProtoBuf.ProtoMember(3, Name = "B")]
        [Newtonsoft.Json.JsonIgnore]
        public virtual string Callback { get => callback; set => callback = value; }

        [Newtonsoft.Json.JsonIgnore]
        public IBusiness Business { get; set; }

        static IRequest<dynamic> ToObject(IBusiness business, dynamic value)
        {
            if (null == value) { return business.Configuration.RequestDefault; }

            var requestType = business.Configuration.RequestType;

            System.Type type = value.GetType();

            switch (type.FullName)
            {
                case "System.Byte[]":
                    {
                        var value2 = Utils.Help.TryProtoBufDeserialize<IRequest<byte[]>>(value);

                        if (null == value2)
                        {
                            return business.Configuration.RequestDefault;
                        }

                        var result = (IRequest<dynamic>)System.Activator.CreateInstance(requestType.MakeGenericType(typeof(object)));
                        result.Cmd = value2.Cmd;
                        result.Data = new object[] { value2.Data };
                        result.Token = value2.Token;
                        result.Group = value2.Group;
                        result.Remote = value2.Remote;
                        result.Callback = value2.Callback;
                        return result;
                    }
                case "System.String":
                    {
                        if (!Utils.Help.TryJsonDeserialize(value, requestType.MakeGenericType(typeof(object)), out IRequest<object> value2) || System.Object.Equals(business.Configuration.RequestDefault, value2))
                        {
                            return business.Configuration.RequestDefault;
                        }

                        object[] data = null;

                        if (System.String.IsNullOrEmpty(System.Convert.ToString(value2.Data)))
                        {
                            data = new object[0];
                        }
                        else
                        {
                            var data2 = Utils.Help.TryJsonDeserialize<Newtonsoft.Json.Linq.JArray>(System.Convert.ToString(value2.Data));

                            data = null == data2 ? new object[1] { value2.Data } : data2.Select(c => c.Type == Newtonsoft.Json.Linq.JTokenType.Object ? c.ToString() : c.ToObject<object>()).ToArray();
                        }

                        var result = (IRequest<dynamic>)System.Activator.CreateInstance(requestType.MakeGenericType(typeof(object)));
                        result.Cmd = value2.Cmd;
                        result.Data = data;
                        result.Token = value2.Token;
                        result.Group = value2.Group;
                        result.Remote = value2.Remote;
                        result.Callback = value2.Callback;
                        return result;
                    }
                default: return business.Configuration.RequestDefault;
            }
        }

        public virtual IResult Call(dynamic data, string remote = null, string group = null, string commandID = null, System.Collections.Generic.IEnumerable<HttpFile> httpFiles = null)
        {
            var request = ToObject(Business, data);

            var resultType = Business.Configuration.ResultType;

            if (System.Object.Equals(Business.Configuration.RequestDefault, request)) { return ResultFactory.ResultCreate(resultType, (int)Mark.MarkItem.Business_DataError, Mark.DataNull); }

            try
            {
                //checked Remote
                if (!System.String.IsNullOrWhiteSpace(remote)) { request.Remote = remote; }
                if (System.String.IsNullOrWhiteSpace(request.Remote)) { return ResultFactory.ResultCreate(resultType, (int)Mark.MarkItem.Exp_RemoteIllegal, Mark.RemoteNull); }

                //checker Cmd
                if (System.String.IsNullOrWhiteSpace(request.Cmd)) { return ResultFactory.ResultCreate(resultType, (int)Mark.MarkItem.Business_CmdError, Mark.CmdNull); }

                //checked Group
                if (!System.String.IsNullOrWhiteSpace(group)) { request.Group = group; }
                if (System.String.IsNullOrWhiteSpace(request.Group)) { request.Group = Bind.CommandGroupDefault; }

                //get Group
                if (!Business.Command.TryGetValue(request.Group, out System.Collections.Generic.IReadOnlyDictionary<string, Command> cmdGroup))
                {
                    return ResultFactory.ResultCreate(resultType, (int)Mark.MarkItem.Business_GroupError, string.Format(Mark.GroupError, request.Group));
                }

                //get Cmd
                if (!cmdGroup.TryGetValue(request.Cmd, out Command command))
                {
                    return ResultFactory.ResultCreate(resultType, (int)Mark.MarkItem.Business_CmdError, string.Format(Mark.CmdError, request.Cmd));
                }

                var meta = Business.Configuration.MetaData[command.Name];

                IResult result;
                //var args = new List<object>(request.Data);
                var args = new object[meta.ArgAttrs[meta.GroupDefault].Args.Count];

                if (0 < meta.TokenPosition.Length)
                {
                    var token = Business.Configuration.Token();
                    token.Key = request.Token;
                    token.Remote = request.Remote;
                    token.CommandID = commandID;

                    foreach (var item in meta.TokenPosition)
                    {
                        args[item] = token;
                    }
                }
                //if (-1 != meta.TokenPosition)
                //{
                //    var token = Business.Configuration.Token();
                //    token.Key = request.Token;
                //    token.Remote = request.Remote;
                //    token.CommandID = commandID;
                //    args[meta.TokenPosition] = token;
                //}
                if (0 < meta.HttpFilePosition.Length && httpFiles.Any())
                {
                    foreach (var item in meta.HttpFilePosition)
                    {
                        args[item] = httpFiles;
                    }
                }
                //if (-1 != meta.HttpFilePosition && httpFiles.Any())
                //{
                //    args[meta.HttpFilePosition] = httpFiles;
                //}

                if (0 < request.Data.Length && 0 < args.Length)
                {
                    int l = 0;
                    for (int i = 0; i < args.Length; i++)
                    {
                        //if (i == meta.TokenPosition || i == meta.HttpFilePosition) { continue; }
                        if (meta.TokenPosition.Contains(i) || meta.HttpFilePosition.Contains(i)) { continue; }

                        if (request.Data.Length < l++)
                        {
                            break;
                        }

                        if (l - 1 < request.Data.Length)
                        {
                            args[i] = request.Data[l - 1];
                        }
                    }
                }

                result = command.Call(args);

                if (!command.HasReturn) { return null; }

                if (System.Object.Equals(null, result))
                {
                    result = ResultFactory.ResultCreate(resultType);
                }

                result.Callback = request.Callback;

                return result;

                //====================================//
                //var type = typeof(T);
                //var result2 = business.ResultCreateToDataBytes(result);
                //result2.Callback = businessData.Callback;

                //return result2;
            }
            catch (System.Exception ex)
            {
                ex = Utils.Help.ExceptionWrite(ex, console: true);

                return ResultFactory.ResultCreate(resultType, (int)Mark.MarkItem.Exp_UndefinedException, ex.Message);
            }
        }
    }
}
