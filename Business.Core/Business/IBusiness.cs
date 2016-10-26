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

namespace Business
{
    public interface IBusiness
    {
        System.Action<Log> WriteLogAsync { get; set; }

        System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IReadOnlyDictionary<string, Command>> Command { get; set; }

        System.Type ResultType { get; set; }

        System.Func<Auth.IToken> Token { get; set; }

        IConfig Config { get; set; }
    }

    public interface IBusiness<Token> : IBusiness
        where Token : Auth.IToken, new() { }

    public abstract class BusinessBase : IBusiness
    {
        public System.Action<Log> WriteLogAsync { get; set; }

        public System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IReadOnlyDictionary<string, Command>> Command { get; set; }

        public System.Type ResultType { get; set; }
        
        public System.Func<Auth.IToken> Token { get; set; }

        public IConfig Config { get; set; }
    }

    public abstract class BusinessBase<Token> : BusinessBase, IBusiness<Token>
        where Token : Auth.IToken, new() { }

    public interface IConfig
    {
        Attributes.ConfigAttribute Info { get; set; }

        bool EnableWatcher { get; set; }

        bool Logger(LogType type, bool canWrite = false, Business.Attributes.LoggerAttribute.ValueMode? canValue = null, bool canResult = false, string method = "*.*");

        bool Attribute(string method, string argument, string attributeFullName, string member, string value);
    }
}
