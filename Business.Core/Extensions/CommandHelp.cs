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
        [ProtoBuf.ProtoContract(SkipConstructor = true)]
        public class BusinessData<Type>
        {
            public static implicit operator BusinessData<Type>(string value)
            {
                return Business.Extensions.Help.JsonDeserialize<BusinessData<Type>>(value);
            }
            public static implicit operator BusinessData<Type>(byte[] value)
            {
                return Business.Extensions.Help.ProtoBufDeserialize<BusinessData<Type>>(value);
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

        public static IResult BusinessCall(this byte[] value, IBusiness business, string remote, string businessGroup = null, string commandID = null)
        {
            try
            {
                BusinessData<byte[]> businessData = value;

                if (!System.String.IsNullOrEmpty(remote)) { businessData.Remote = remote; }
                if (System.String.IsNullOrEmpty(businessData.Remote)) { return business.ResultCreate((int)Mark.MarkItem.Exp_RemoteIllegal, "Remote is null"); }

                if (null == businessData.Cmd) { return business.ResultCreate((int)Mark.MarkItem.Business_CmdError, "Cmd is null"); }

                if (!System.String.IsNullOrEmpty(businessGroup)) { businessData.Group = businessGroup; }
                if (System.String.IsNullOrEmpty(businessData.Group)) { businessData.Group = Bind.CommandGroupDefault; }

                System.Collections.Generic.IReadOnlyDictionary<string, Business.Command> group;
                if (!business.Command.TryGetValue(businessData.Group, out group))
                {
                    return business.ResultCreate((int)Mark.MarkItem.Business_GroupError, string.Format("Not businessGroup {0}", businessData.Group));
                }

                Business.Command command;
                if (!group.TryGetValue(businessData.Cmd, out command))
                {
                    return business.ResultCreate((int)Mark.MarkItem.Business_CmdError, string.Format("Not Cmd {0}", businessData.Cmd));
                }

                Result.IResult result = command.Call(new Auth.Token { Key = businessData.Token, Remote = businessData.Remote, CommandID = commandID }, businessData.Data);

                if (!command.Meta.HasReturn)
                {
                    return null;
                }

                Result.IResult result2;

                if (System.Object.Equals(null, result))
                {
                    result2 = business.ResultCreate();
                }
                else
                {
                    result2 = business.ResultCreateToDataBytes(result);
                }
                //====================================//
                result2.Callback = businessData.Callback;

                return result2;
            }
            catch (System.Exception ex)
            {
                try
                {
                    return business.ResultCreate((int)Mark.MarkItem.Business_DataError, System.Convert.ToString(ex));
                }
                catch { return null; }
            }
        }

        public static string BusinessCall(this string value, IBusiness business, string remote = null, string businessGroup = null)
        {
            return BusinessCall((BusinessData<string>)value, business, remote, businessGroup);
        }
        public static string BusinessCall(this BusinessData<string> businessData, IBusiness business, string remote = null, string businessGroup = null)
        {
            try
            {
                if (!System.String.IsNullOrEmpty(remote)) { businessData.Remote = remote; }
                if (System.String.IsNullOrEmpty(businessData.Remote)) { return business.ResultCreate((int)Mark.MarkItem.Exp_RemoteIllegal, Mark.Get<string>(Mark.MarkItem.Exp_RemoteIllegal)).ToString(); }

                if (null == businessData.Cmd) { return business.ResultCreate((int)Mark.MarkItem.Business_CmdError, Mark.Get<string>(Mark.MarkItem.Business_CmdError)).ToString(); }

                if (!System.String.IsNullOrEmpty(businessGroup)) { businessData.Group = businessGroup; }
                if (System.String.IsNullOrEmpty(businessData.Group)) { businessData.Group = Bind.CommandGroupDefault; }

                System.Collections.Generic.IReadOnlyDictionary<string, Business.Command> group;
                if (!business.Command.TryGetValue(businessData.Group, out group))
                {
                    return business.ResultCreate((int)Mark.MarkItem.Business_GroupError, string.Format("Not businessGroup {0}", businessData.Group)).ToString();
                }

                Business.Command command;
                if (!group.TryGetValue(businessData.Cmd, out command))
                {
                    return business.ResultCreate((int)Mark.MarkItem.Business_CmdError, string.Format("Not Cmd {0}", businessData.Cmd)).ToString();
                }

                var result = command.Call(new Auth.Token { Key = businessData.Token, Remote = businessData.Remote }, businessData.Data);
                //var result = command.Call(business.TokenCreate(businessData.Token, businessData.Remote), businessData.Data);

                if (!command.Meta.HasReturn) { return null; }
                //Result.IResult result2;

                if (System.Object.Equals(null, result))
                {
                    return business.ResultCreate().ToString();
                }
                //else
                //{
                //    result2 = command.ResultCreateToDataString(result);
                //}
                //====================================//
                //result.Callback = commandData.Callback;


                return result.ToString();
            }
            catch (System.Exception ex)
            {
                try
                {
                    return business.ResultCreate((int)Mark.MarkItem.Business_DataError, System.Convert.ToString(ex)).ToString();
                }
                catch { return null; }
            }
        }
    }
}