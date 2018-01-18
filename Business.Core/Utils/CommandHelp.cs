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

namespace Business.Utils
{
    using Business.Meta;
    using Result;
    using System.Linq;

    public static class CommandHelp
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

        const string ProcesException = "Processing exception";
        const string DataNull = "Data cannot be null";
        const string RemoteNull = "Remote cannot be null";
        const string CmdNull = "Cmd cannot be null";
        const string GroupError = "Without this business group {0}";
        const string CmdError = "Without this Cmd {0}";
        const string DataError = "Data error";

        #endregion

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
            public System.IO.Stream Value { get => value; }
        }

        [ProtoBuf.ProtoContract(SkipConstructor = true)]
        public class BusinessData<Type>
        {
            public static implicit operator BusinessData<Type>(string value)
            {
                var value2 = Help.TryJsonDeserialize<BusinessData<string>>(value);

                if (null == value2)
                {
                    return default(BusinessData<Type>);
                }

                object[] data = null;

                if (System.String.IsNullOrEmpty(value2.Data))
                {
                    data = new object[0];
                }
                else
                {
                    var data2 = Help.TryJsonDeserialize<Newtonsoft.Json.Linq.JArray>(value2.Data);

                    data = null == data2 ? new object[1] { value2.Data } : data2.Select(c => c.Type == Newtonsoft.Json.Linq.JTokenType.Object ? c.ToString() : c.ToObject<object>()).ToArray();
                }

                return new BusinessData<Type> { Cmd = value2.Cmd, Token = value2.Token, Data = (Type)System.Convert.ChangeType(data, typeof(dynamic[])), Group = value2.Group, Callback = value2.Callback, Remote = value2.Remote };

                //return Help.TryJsonDeserialize<BusinessData<Type>>(value);
            }
            public static implicit operator BusinessData<Type>(byte[] value)
            {
                return Help.TryProtoBufDeserialize<BusinessData<Type>>(value);
            }

            /// <summary>
            /// Gets the cmd of this request.
            /// </summary>
            [ProtoBuf.ProtoMember(1, Name = "C")]
            [Newtonsoft.Json.JsonProperty(PropertyName = "C")]
            public virtual string Cmd { get; set; }

            /// <summary>
            /// Gets the data of this request.
            /// </summary>
            [ProtoBuf.ProtoMember(2, Name = "D")]
            [Newtonsoft.Json.JsonProperty(PropertyName = "D")]
            public virtual Type Data { get; set; }

            /// <summary>
            /// Gets the Command token of this request, used for callback
            /// </summary>
            [ProtoBuf.ProtoMember(3, Name = "B")]
            [Newtonsoft.Json.JsonIgnore]
            public virtual string Callback { get; set; }

            /// <summary>
            /// Gets the login key of this request.
            /// </summary>
            [ProtoBuf.ProtoMember(4, Name = "T")]
            [Newtonsoft.Json.JsonProperty(PropertyName = "T")]
            public virtual string Token { get; set; }

            [ProtoBuf.ProtoMember(5, Name = "G")]
            [Newtonsoft.Json.JsonProperty(PropertyName = "G")]
            public virtual string Group { get; set; }

            [Newtonsoft.Json.JsonProperty(PropertyName = "R")]
            public virtual string Remote { get; set; }
        }

        public static IResult BusinessCall<T>(this IBusiness business, BusinessData<T> businessData, string remote, string businessGroup = null, string commandID = null, System.Collections.Generic.IEnumerable<HttpFile> httpFiles = null)
        {
            var resultType = business.Configuration.ResultType;

            if (null == businessData) { return ResultFactory.ResultCreate(resultType, (int)MarkItem.Business_DataError, DataNull); }

            try
            {
                //checked Remote
                if (!System.String.IsNullOrWhiteSpace(remote)) { businessData.Remote = remote; }
                if (System.String.IsNullOrWhiteSpace(businessData.Remote)) { return ResultFactory.ResultCreate(resultType, (int)MarkItem.Exp_RemoteIllegal, RemoteNull); }

                //checker Cmd
                if (null == businessData.Cmd) { return ResultFactory.ResultCreate(resultType, (int)MarkItem.Business_CmdError, CmdNull); }

                //checked Group
                if (!System.String.IsNullOrWhiteSpace(businessGroup)) { businessData.Group = businessGroup; }
                if (System.String.IsNullOrWhiteSpace(businessData.Group)) { businessData.Group = Bind.CommandGroupDefault; }

                //get Group
                if (!business.Command.TryGetValue(businessData.Group, out System.Collections.Generic.IReadOnlyDictionary<string, Command> group))
                {
                    return ResultFactory.ResultCreate(resultType, (int)MarkItem.Business_GroupError, string.Format(GroupError, businessData.Group));
                }

                //get Cmd
                if (!group.TryGetValue(businessData.Cmd, out Command command))
                {
                    return ResultFactory.ResultCreate(resultType, (int)MarkItem.Business_CmdError, string.Format(CmdError, businessData.Cmd));
                }

                IResult result;
                var args = (typeof(T).IsArray && !System.Object.Equals(null, businessData.Data)) ? businessData.Data as object[] : new object[] { businessData.Data };
                if (!System.String.IsNullOrEmpty(businessData.Token))
                {
                    var token = business.Configuration.Token();
                    token.Key = businessData.Token;
                    token.Remote = businessData.Remote;
                    token.CommandID = commandID;

                    var filesAny = httpFiles.Any();
                    var list = new object[1 + args.Length + (filesAny ? 1 : 0)];
                    list[0] = token;
                    System.Array.Copy(args, 0, list, 1, args.Length);
                    if (filesAny) { list[list.Length - 1] = httpFiles; }
                    args = list;
                }

                result = command.Call(args);

                if (!command.HasReturn) { return null; }

                if (System.Object.Equals(null, result))
                {
                    result = ResultFactory.ResultCreate(resultType);
                }

                result.Callback = businessData.Callback;

                return result;

                //====================================//
                //var type = typeof(T);
                //var result2 = business.ResultCreateToDataBytes(result);
                //result2.Callback = businessData.Callback;

                //return result2;
            }
            catch (System.Exception ex)
            {
                Help.WriteLocal(ex, console: true);

                return ResultFactory.ResultCreate(resultType, (int)MarkItem.Exp_UndefinedException, ex.Message);
            }
        }

        /*
        public static IResult BusinessCall(this byte[] value, IBusiness business, string remote, string commandID, string businessGroup = null)
        {
            var result = BusinessCall((BusinessData<byte[]>)value, business, remote, businessGroup, commandID);

            return business.Configuration.ResultType.ResultCreateToDataBytes(result);
        }
        public static IResult BusinessCall(this string value, IBusiness business, string remote = null, string businessGroup = null)
        {
            return BusinessCall((BusinessData<object[]>)value, business, remote, businessGroup);
        }
        public static IResult BusinessCall(this BusinessData<dynamic[]> businessData, IBusiness business, string remote = null, string businessGroup = null)
        {
            return BusinessCall<dynamic[]>(businessData, business, remote, businessGroup);
        }
        public static IResult BusinessCall(this BusinessData<string[]> businessData, IBusiness business, string remote = null, string businessGroup = null)
        {
            return BusinessCall<string[]>(businessData, business, remote, businessGroup);
        }
        public static IResult BusinessCall(this BusinessData<dynamic> businessData, IBusiness business, string remote = null, string businessGroup = null)
        {
            return BusinessCall<dynamic>(businessData, business, remote, businessGroup);
        }
        */
        struct CommandResult
        {
            public bool Overall { get; set; }
            public string CommandID { get; set; }
            public byte[] Data { get; set; }
        }

        /*
        static System.Collections.Generic.List<CommandResult> BusinessCall2(this byte[] value, IBusiness business, string remote, string commandID, string businessGroup = null)
        {
            var result = BusinessCall(value, business, remote, businessGroup, commandID);

            var list = new System.Collections.Generic.List<CommandResult>();

            var data = result.ToBytes();

            if (result.Command.Overall)
            {
                list.Add(new CommandResult { Overall = true, Data = data });
            }
            else
            {
                foreach (var item in result.Command.CommandID)
                {
                    if (System.String.IsNullOrEmpty(item)) { continue; }
                    list.Add(new CommandResult { CommandID = item, Data = data });
                }
            }

            if (0 < result.Command.Notifys.Count)
            {
                foreach (var item in result.Command.Notifys)
                {
                    var data2 = item.ToBytes();

                    if (item.Command.Overall)
                    {
                        list.Add(new CommandResult { Overall = true, Data = data2 });
                    }
                    //else if (1 == item.Command.CommandID.Count && !System.String.IsNullOrEmpty(item.Command.CommandID[0]))
                    //{
                    //    list.Add(new CommandResult { CommandID = item.Command.CommandID[0], Data = item.ToBytes() });
                    else
                    {
                        foreach (var item2 in item.Command.CommandID)
                        {
                            if (System.String.IsNullOrEmpty(item2)) { continue; }
                            list.Add(new CommandResult { CommandID = item2, Data = data2 });
                        }
                    }
                }
            }

            return list;
        }
        */
    }
}