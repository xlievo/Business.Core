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
    public static class Mark
    {
        #region MarkEnum

        public enum MarkItem
        {
            /// <summary>
            /// Undefined Exception 0
            /// </summary>
            Exp_UndefinedException = 0,

            /// <summary>
            /// Remote Illegal -8
            /// </summary>
            Exp_RemoteIllegal = -8,
            /// <summary>
            /// Data Error -10
            /// </summary>
            Business_DataError = -10,
            /// <summary>
            /// "Command Error -11
            /// </summary>
            Business_CmdError = -11,
            /// <summary>
            /// Group Error -12
            /// </summary>
            Business_GroupError = -12,
        }

        #endregion

        readonly static System.Collections.Specialized.HybridDictionary marks = new System.Collections.Specialized.HybridDictionary(10) { { MarkItem.Exp_UndefinedException, "Undefined Exception" }, { MarkItem.Exp_RemoteIllegal, "Remote Illegal" }, { MarkItem.Business_DataError, "Command Data Error" }, { MarkItem.Business_CmdError, "Command Error" }, { MarkItem.Business_GroupError, "Group Error" } };

        public static Type Get<Type>(this MarkItem mark) { return Extensions.Help.ChangeType<Type>(marks[mark]); }
    }
}