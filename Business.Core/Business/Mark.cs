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
            /// Arguments Illegal -1
            /// </summary>
            Exp_ArgumentsIllegal = -1,
            /// <summary>
            /// Session Illegal -2
            /// </summary>
            Exp_SessionIllegal = -2,
            /// <summary>
            /// Session Out -3
            /// </summary>
            Exp_SessionOut = -3,
            /// <summary>
            /// Competence Illegal -4
            /// </summary>
            Exp_CompetenceIllegal = -4,
            /// <summary>
            /// User Error -5
            /// </summary>
            Exp_UserError = -5,
            /// <summary>
            /// Password Error -6
            /// </summary>
            Exp_PasswordError = -6,
            /// <summary>
            /// User Frozen -7
            /// </summary>
            Exp_UserFrozen = -7,

            /// <summary>
            /// Login_Error -8
            /// </summary>
            Login_Error = -8,
            /// <summary>
            /// Command_DataError -9
            /// </summary>
            Command_DataError = -9,
            /// <summary>
            /// Command_KeyError -10
            /// </summary>
            Command_KeyError = -10,

            /// <summary>
            /// Login_ErrorCount 3
            /// </summary>
            Login_ErrorCount = 95,
            /// <summary>
            /// Login_ErrorRange 2
            /// </summary>
            Login_ErrorRange = 94,
            /// <summary>
            /// Login_ErrorFreeze 3
            /// </summary>
            Login_ErrorFreeze = 93, 
        }

        #endregion

        readonly static System.Collections.Specialized.HybridDictionary marks = new System.Collections.Specialized.HybridDictionary(20) { { MarkItem.Exp_UndefinedException, "Undefined Exception" }, { MarkItem.Exp_ArgumentsIllegal, "Arguments Illegal" }, { MarkItem.Exp_SessionIllegal, "Session Illegal" }, { MarkItem.Exp_SessionOut, "Session Out" }, { MarkItem.Exp_CompetenceIllegal, "Competence Illegal" }, { MarkItem.Exp_UserError, "User Error" }, { MarkItem.Exp_PasswordError, "Password Error" }, { MarkItem.Exp_UserFrozen, "User Frozen" }, { MarkItem.Login_Error, "Login Error" }, { MarkItem.Command_DataError, "Command Data Error" }, { MarkItem.Command_KeyError, "Command Error" }, { MarkItem.Login_ErrorCount, 3 }, { MarkItem.Login_ErrorRange, 2 }, { MarkItem.Login_ErrorFreeze, 3 } };

        public static Type Get<Type>(this MarkItem mark) { return Extensions.Help.ChangeType<Type>(marks[mark]); }
    }
}