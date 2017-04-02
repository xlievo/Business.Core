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

namespace Business.Extensions
{
    using Result;

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

        const string DataNull = "Data cannot be null";
        const string RemoteNull = "Remote cannot be null";
        const string CmdNull = "Cmd cannot be null";
        const string GroupError = "Without this business group {0}";
        const string CmdError = "Without this Cmd {0}";
        const string DataError = "Data error";

        #endregion

        [ProtoBuf.ProtoContract(SkipConstructor = true)]
        public class BusinessData<Type>
        {
            public static implicit operator BusinessData<Type>(string value)
            {
                return Help.JsonDeserialize<BusinessData<Type>>(value);
            }
            public static implicit operator BusinessData<Type>(byte[] value)
            {
                return Help.ProtoBufDeserialize<BusinessData<Type>>(value);
            }

            /// <summary>
            /// Gets the cmd of this request.
            /// </summary>
            [ProtoBuf.ProtoMember(1, Name = "C")]
            [Newtonsoft.Json.JsonProperty(PropertyName = "C")]
            public string Cmd { get; set; }

            /// <summary>
            /// Gets the data of this request.
            /// </summary>
            [ProtoBuf.ProtoMember(2, Name = "D")]
            [Newtonsoft.Json.JsonProperty(PropertyName = "D")]
            public Type Data { get; set; }

            /// <summary>
            /// Gets the Command token of this request, used for callback
            /// </summary>
            [ProtoBuf.ProtoMember(3, Name = "B")]
            [Newtonsoft.Json.JsonIgnore]
            public string Callback { get; set; }

            /// <summary>
            /// Gets the login key of this request.
            /// </summary>
            [ProtoBuf.ProtoMember(4, Name = "T")]
            [Newtonsoft.Json.JsonProperty(PropertyName = "T")]
            public string Token { get; set; }

            [ProtoBuf.ProtoMember(5, Name = "G")]
            [Newtonsoft.Json.JsonProperty(PropertyName = "G")]
            public string Group { get; set; }

            [Newtonsoft.Json.JsonProperty(PropertyName = "R")]
            public string Remote { get; set; }
        }

        static IResult BusinessCall<T>(BusinessData<T> businessData, IBusiness business, string remote, string businessGroup = null, string commandID = null)
        {
            if (null == businessData) { return ResultFactory.ResultCreate(business, (int)MarkItem.Business_DataError, DataNull); }

            try
            {
                //checked Remote
                if (!System.String.IsNullOrWhiteSpace(remote)) { businessData.Remote = remote; }
                if (System.String.IsNullOrWhiteSpace(businessData.Remote)) { return ResultFactory.ResultCreate(business, (int)MarkItem.Exp_RemoteIllegal, RemoteNull); }

                //checker Cmd
                if (null == businessData.Cmd) { return ResultFactory.ResultCreate(business, (int)MarkItem.Business_CmdError, CmdNull); }

                //checked Group
                if (!System.String.IsNullOrWhiteSpace(businessGroup)) { businessData.Group = businessGroup; }
                if (System.String.IsNullOrWhiteSpace(businessData.Group)) { businessData.Group = Bind.CommandGroupDefault; }

                if (!business.Command.TryGetValue(businessData.Group, out System.Collections.Generic.IReadOnlyDictionary<string, Business.Command> group))
                {
                    return ResultFactory.ResultCreate(business, (int)MarkItem.Business_GroupError, string.Format(GroupError, businessData.Group));
                }

                if (!group.TryGetValue(businessData.Cmd, out Business.Command command))
                {
                    return ResultFactory.ResultCreate(business, (int)MarkItem.Business_CmdError, string.Format(CmdError, businessData.Cmd));
                }

                IResult result;
                var args = typeof(T).IsArray && !System.Object.Equals(null, businessData.Data) ? businessData.Data as object[] : new object[] { businessData.Data };
                if (!System.String.IsNullOrEmpty(businessData.Token))
                {
                    var token = business.Token();
                    token.Key = businessData.Token;
                    token.Remote = businessData.Remote;
                    token.CommandID = commandID;

                    var list = new object[1 + args.Length];
                    list[0] = token;
                    System.Array.Copy(args, 0, list, 1, args.Length);
                    args = list;
                }

                result = command.Call(args);

                if (!command.Meta.HasReturn) { return null; }

                if (System.Object.Equals(null, result))
                {
                    result = ResultFactory.ResultCreate(business);
                }

                result.Callback = businessData.Callback;
                if (0 == result.Command.CommandID.Count)
                {
                    result.Command.CommandID.Add(commandID);
                }
                return result;

                //====================================//
                //var type = typeof(T);
                //var result2 = business.ResultCreateToDataBytes(result);
                //result2.Callback = businessData.Callback;

                //return result2;
            }
            catch //(System.Exception ex)
            {
                try
                {
                    return ResultFactory.ResultCreate(business, (int)MarkItem.Business_DataError, DataError);
                }
                catch { return null; }
            }
        }

        public static IResult BusinessCall(this byte[] value, IBusiness business, string remote, string commandID, string businessGroup = null)
        {
            var result = BusinessCall((BusinessData<byte[]>)value, business, remote, businessGroup, commandID);

            return business.ResultCreateToDataBytes(result);
        }
        public static IResult BusinessCall(this string value, IBusiness business, string remote = null, string businessGroup = null)
        {
            return BusinessCall((BusinessData<string[]>)value, business, remote, businessGroup);
        }
        public static IResult BusinessCall(this BusinessData<string[]> businessData, IBusiness business, string remote = null, string businessGroup = null)
        {
            return BusinessCall<string[]>(businessData, business, remote, businessGroup);
        }

        struct CommandResult
        {
            public bool Overall { get; set; }
            public string CommandID { get; set; }
            public byte[] Data { get; set; }
        }

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
    }
}