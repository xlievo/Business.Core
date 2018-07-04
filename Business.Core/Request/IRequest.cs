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
    /*
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
            /// Command Error -1
            /// </summary>
            Business_CmdError = -1,

            /// <summary>
            /// Remote Illegal -2
            /// </summary>
            Exp_RemoteIllegal = -2,
            /// <summary>
            /// Data Error -3
            /// </summary>
            Business_DataError = -3,
            /// <summary>
            /// Group Error -4
            /// </summary>
            Business_GroupError = -4,
        }

        internal const string ProcesException = "Processing exception";
        internal const string DataNull = "Data cannot be null";
        internal const string RemoteNull = "Remote cannot be null";
        internal const string CmdNull = "Cmd cannot be null";
        internal const string GroupError = "Without this business group {0}";
        internal const string CmdError = "Without this Cmd {0}";
        internal const string DataError = "Data error";

        #endregion
    }
    */

    public interface IRequest
    {
        /// <summary>
        /// Gets the cmd of this request.
        /// </summary>
        string Cmd { get; set; }

        /// <summary>
        /// Gets the data of this request.
        /// </summary>
        object[] Data { get; set; }

        /// <summary>
        /// Gets the login key of this request.
        /// </summary>
        string Token { get; set; }

        string Group { get; set; }

        string Remote { get; set; }

        /// <summary>
        /// Gets the token of this result, used for callback
        /// </summary>
        string Callback { get; set; }
    }

    [ProtoBuf.ProtoContract(SkipConstructor = true)]
    public class RequestObject : IRequest
    {
        string cmd;
        /// <summary>
        /// Gets the cmd of this request.
        /// </summary>
        [ProtoBuf.ProtoMember(1, Name = "C")]
        [Newtonsoft.Json.JsonProperty(PropertyName = "C")]
        public virtual string Cmd { get => cmd; set => cmd = value; }

        object[] data;
        /// <summary>
        /// Gets the data of this request.
        /// </summary>
        [ProtoBuf.ProtoMember(2, Name = "D")]
        [Newtonsoft.Json.JsonProperty(PropertyName = "D")]
        public virtual object[] Data { get => data; set => data = value; }

        string token;
        /// <summary>
        /// Gets the login key of this request.
        /// </summary>
        [ProtoBuf.ProtoMember(3, Name = "T")]
        [Newtonsoft.Json.JsonProperty(PropertyName = "T")]
        public virtual string Token { get => token; set => token = value; }

        string group;
        [ProtoBuf.ProtoMember(4, Name = "G")]
        [Newtonsoft.Json.JsonProperty(PropertyName = "G")]
        public virtual string Group { get => group; set => group = value; }

        string remote;
        [Newtonsoft.Json.JsonProperty(PropertyName = "R")]
        public virtual string Remote { get => remote; set => remote = value; }

        string callback;
        /// <summary>
        /// Gets the Command token of this request, used for callback
        /// </summary>
        [ProtoBuf.ProtoMember(5, Name = "B")]
        [Newtonsoft.Json.JsonIgnore]
        [Newtonsoft.Json.JsonProperty(PropertyName = "B")]
        public virtual string Callback { get => callback; set => callback = value; }
    }

    //public interface IRequest
    //{
    //    string Cmd { get; set; }

    //    dynamic Data { get; }

    //    string Token { get; set; }

    //    string Group { get; set; }

    //    string Remote { get; set; }

    //    string Callback { get; set; }

    //    /*
    //    //IBusiness Business { get; set; }
    //    Configer.Configuration Configuration { get; set; }

    //    System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IReadOnlyDictionary<string, Command>> Command { get; set; }
    //    */
    //}

    /*
    public interface IRequest<DataType> : IRequest
    {
        new DataType Data { get; set; }

        //IRequest<dynamic> Create(dynamic requestData, System.Reflection.TypeInfo requestType);

        dynamic Call(IBusiness business, DataType data, string remote, string group = null, Http.IHttpRequest httpRequest = null, Http.IHttpRequest scoketRequest = null, params object[] useTypeObj);
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
        //[Newtonsoft.Json.JsonIgnore]
        [Newtonsoft.Json.JsonProperty(PropertyName = "B")]
        public virtual string Callback { get => callback; set => callback = value; }

        public Action<IRequest> Callbacks { get; set; }

        public virtual IRequest<dynamic> Create(dynamic requestData, System.Type requestType)
        {
            var defaultValue = this as IRequest<dynamic>;

            if (null == requestData)
            {
                return defaultValue;
            }

            System.Type type = requestData.GetType();

            switch (type.FullName)
            {
                case "System.Byte[]":
                    {
                        if (!Utils.Help.TryProtoBufDeserialize(requestData, requestType.MakeGenericType(typeof(byte[])), out IRequest<object> request))
                        {
                            return defaultValue;
                        }

                        if (null == request)
                        {
                            return defaultValue;
                        }

                        //request.Data = new object[] { request.Data };
                        return request;
                    }
                case "System.String":
                    {
                        if (!Utils.Help.TryJsonDeserialize(requestData, requestType.MakeGenericType(typeof(object)), out IRequest<object> request) || System.Object.Equals(defaultValue, request))
                        {
                            return defaultValue;
                        }

                        object[] data = null;

                        if (System.String.IsNullOrEmpty(System.Convert.ToString(request.Data)))
                        {
                            data = new object[0];
                        }
                        else
                        {
                            var dataArray = Utils.Help.TryJsonDeserialize<Newtonsoft.Json.Linq.JArray>(System.Convert.ToString(request.Data));

                            data = null == dataArray ? new object[1] { request.Data } : dataArray.Select(c => c.Type == Newtonsoft.Json.Linq.JTokenType.Object ? c.ToString() : c.ToObject<object>()).ToArray();
                        }

                        request.Data = data;
                        return request;
                    }
                default: return requestData;
            }
        }

        public dynamic Call(IBusiness business, Type data, string remote, string group = null, Http.IHttpRequest httpRequest = null, Http.IHttpRequest scoketRequest = null, params object[] useTypeObj)
        {
            var request = Create(data, business.Configuration.RequestType);

            var resultType = business.Configuration.ResultType;

            if (System.Object.Equals(business.Configuration.RequestDefault, request)) { return Result.ResultFactory.ResultCreate(resultType, (int)Request.Mark.MarkItem.Business_DataError, Request.Mark.DataNull); }

            try
            {
                //checked Remote
                if (!System.String.IsNullOrWhiteSpace(remote)) { request.Remote = remote; }
                if (System.String.IsNullOrWhiteSpace(request.Remote)) { return Result.ResultFactory.ResultCreate(resultType, (int)Request.Mark.MarkItem.Exp_RemoteIllegal, Request.Mark.RemoteNull); }

                ////checker Cmd
                //if (System.String.IsNullOrWhiteSpace(request.Cmd)) { return Result.ResultFactory.ResultCreate(resultType, (int)Request.Mark.MarkItem.Business_CmdError, Request.Mark.CmdNull); }

                //checked Group
                if (!System.String.IsNullOrWhiteSpace(group)) { request.Group = group; }
                //if (System.String.IsNullOrWhiteSpace(request.Group)) { request.Group = Bind.CommandGroupDefault; }

                //get Group
                var command = business.Command.GetCommand(request.Cmd, request.Group);
                if (null == command)
                {
                    return Result.ResultFactory.ResultCreate(resultType, (int)Request.Mark.MarkItem.Business_CmdError, string.Format(Request.Mark.CmdError, request.Cmd));
                }
                //if (!business.Command.TryGetValue(request.Group, out ConcurrentReadOnlyDictionary<string, Command> cmdGroup))
                //{
                //    return Result.ResultFactory.ResultCreate(resultType, (int)Request.Mark.MarkItem.Business_GroupError, string.Format(Request.Mark.GroupError, request.Group));
                //}

                ////get Cmd
                //if (!cmdGroup.TryGetValue(request.Cmd, out Command command))
                //{
                //    return Result.ResultFactory.ResultCreate(resultType, (int)Request.Mark.MarkItem.Business_CmdError, string.Format(Request.Mark.CmdError, request.Cmd));
                //}



                var meta = business.Configuration.MetaData[command.Meta.Name];

                var args = new object[meta.ArgAttrs[meta.GroupDefault].Args.Count];

                #region Token

                if (0 < meta.TokenPosition.Length)
                {
                    var token = business.Configuration.Token();
                    token.Key = request.Token;
                    token.Remote = request.Remote;
                    //token.CommandID = commandID;

                    foreach (var item in meta.TokenPosition)
                    {
                        args[item] = token;
                    }
                }

                #endregion

                #region HttpRequest

                if (0 < meta.HttpRequestPosition.Length && null != httpRequest)
                {
                    if (null != httpRequest.Files && !httpRequest.Files.Any())
                    {
                        httpRequest.Files = null;
                    }

                    if (null != httpRequest.Cookies && 0 == httpRequest.Cookies.Count)
                    {
                        httpRequest.Cookies = null;
                    }

                    if (null != httpRequest.Headers && 0 == httpRequest.Headers.Count)
                    {
                        httpRequest.Headers = null;
                    }

                    foreach (var item in meta.HttpRequestPosition)
                    {
                        args[item] = httpRequest;
                    }
                }

                #endregion

                args = command.GetAgs(request.Data, useTypeObj);

                //if (0 < meta.UseTypePosition.Count && null != useTypeObj && 0 < useTypeObj.Length)
                //{
                //    foreach (var item in useTypeObj)
                //    {
                //        if (System.Object.Equals(null, item)) { continue; }

                //        //var name = item.GetType().FullName;

                //        //if (meta.UseTypePosition.TryGetValue(name, out int i))
                //        //{
                //        //    args[i] = item;
                //        //}

                //        var useArg = meta.UseTypePosition.FirstOrDefault(c => c.Key.IsAssignableFrom(item.GetType()));

                //        if (null != useArg.Key)
                //        {
                //            args[useArg.Value] = item;
                //        }
                //    }
                //}

                //if (0 < request.Data.Length && 0 < args.Length)
                //{
                //    int l = 0;
                //    for (int i = 0; i < args.Length; i++)
                //    {
                //        //if (meta.TokenPosition.Contains(i) || meta.HttpRequestPosition.Contains(i))
                //        //{
                //        //    continue;
                //        //}

                //        if (meta.UseTypePosition.Values.Contains(i))
                //        {
                //            continue;
                //        }

                //        if (request.Data.Length < l++)
                //        {
                //            break;
                //        }

                //        if (l - 1 < request.Data.Length)
                //        {
                //            args[i] = request.Data[l - 1];
                //        }
                //    }
                //}

                //var result = command.Call(args);

                if (command.Meta.HasAsync)
                {
                    var task = command.AsyncCall(args);

                    //var task = result as System.Threading.Tasks.Task;

                    return task.ContinueWith(c =>
                    {
                        if (command.Meta.HasReturn)
                        {
                            var returnValue = c.Result;

                            if (command.Meta.HasIResult)
                            {
                                Result.IResult r = c.Result;

                                if (null != r)
                                {
                                    r.Callback = request.Callback;

                                    returnValue = r;
                                }
                            }

                            //var returnValue = c.Result;

                            //if (!System.Object.Equals(null, returnValue) && typeof(Result.IResult).IsAssignableFrom(returnValue.GetType()))
                            //{
                            //    returnValue.Callback = request.Callback;
                            //}

                            return returnValue;
                        }
                        else
                        {
                            return null;
                        }
                    });
                }
                else
                {
                    var result = command.Call(args);

                    if (command.Meta.HasIResult)
                    {
                        if (System.Object.Equals(null, result))
                        {
                            result = Result.ResultFactory.ResultCreate(resultType);
                        }

                        result.Callback = request.Callback;
                    }

                    return result;
                }

                //return result;

                //====================================//
                //var type = typeof(T);
                //var result2 = business.ResultCreateToDataBytes(result);
                //result2.Callback = businessData.Callback;

                //return result2;
            }
            catch (System.Exception ex)
            {
                ex = Utils.Help.ExceptionWrite(ex, console: true);
                //...
                return Result.ResultFactory.ResultCreate(resultType, (int)Request.Mark.MarkItem.Exp_UndefinedException, ex.Message);
            }
        }
    }
    */
}
