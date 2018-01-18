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

namespace Business.Utils.Emit
{
    public static class MethodInvokerGenerator
    {
        public static System.Func<object, object[], object> CreateDelegate(System.Reflection.MethodInfo methodInfo, bool validateArguments = true, string methodName = null)
        {
            if (methodInfo == null)
            {
                throw new System.ArgumentNullException(nameof(methodInfo));
            }

            var args = methodInfo.GetParameters();
            var dynamicMethod = EmitUtils.CreateDynamicMethod(string.Format("{0}_{1}|{2}", "$Call", methodInfo.Name, methodName),
                typeof(object),
                new[] { typeof(object), typeof(object[]) },
                methodInfo.DeclaringType);
            var il = dynamicMethod.GetILGenerator();

            var lableValidationCompleted = il.DefineLabel();
            if (!validateArguments || (methodInfo.IsStatic && args.Length == 0))
            {
                il.Br_S(lableValidationCompleted);
            }
            else
            {
                var lableCheckArgumentsRef = il.DefineLabel();
                var lableCheckArgumentsLength = il.DefineLabel();

                if (!methodInfo.IsStatic)
                {
                    il.Ldarg_0();
                    il.Brtrue_S(args.Length > 0 ? lableCheckArgumentsRef : lableValidationCompleted);

                    il.ThrowArgumentsNullExcpetion("instance");
                }

                if (args.Length > 0)
                {
                    il.MarkLabel(lableCheckArgumentsRef);
                    il.Ldarg_1();
                    il.Brtrue_S(lableCheckArgumentsLength);

                    il.ThrowArgumentsNullExcpetion("arguments");

                    il.MarkLabel(lableCheckArgumentsLength);
                    il.Ldarg_1();
                    il.Ldlen();
                    il.Conv_I4();
                    il.LoadInt32(args.Length);
                    il.Bge_S(lableValidationCompleted);

                    il.ThrowArgumentsExcpetion("Not enough arguments in the argument array.", "arguments");
                }
            }

            il.MarkLabel(lableValidationCompleted);
            if (!methodInfo.IsStatic)
            {
                il.Ldarg_0();
                il.CastReference(methodInfo.DeclaringType);
            }

            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    il.Ldarg_1();
                    il.LoadInt32(i);
                    il.Ldelem_Ref();
                    il.CastValue(args[i].ParameterType);
                }
            }

            il.CallMethod(methodInfo);
            if (methodInfo.ReturnType == typeof(void))
            {
                il.Ldc_I4_0();
            }
            else
            {
                il.BoxIfNeeded(methodInfo.ReturnType);
            }
            il.Ret();

            var methodDelegate = dynamicMethod.CreateDelegate(typeof(System.Func<object, object[], object>));
            return (System.Func<object, object[], object>)methodDelegate;
        }

        //public delegate Result Call<Result>(object instance, params object[] args);

        public static System.Func<object, object[], Result> CreateDelegate<Result>(System.Reflection.MethodInfo methodInfo, bool validateArguments, string methodName = null)
        {
            if (methodInfo == null)
            {
                throw new System.ArgumentNullException("methodInfo");
            }

            var args = methodInfo.GetParameters();
            var dynamicMethod = EmitUtils.CreateDynamicMethod(string.Format("{0}_{1}|{2}", "$Call", methodInfo.Name, methodName),
                //var dynamicMethod = EmitUtils.CreateDynamicMethod("$Call" + methodInfo.Name,
                typeof(Result),
                new[] { typeof(object), typeof(object[]) },
                methodInfo.DeclaringType);
            var il = dynamicMethod.GetILGenerator();

            var lableValidationCompleted = il.DefineLabel();
            if (!validateArguments || (methodInfo.IsStatic && args.Length == 0))
            {
                il.Br_S(lableValidationCompleted);
            }
            else
            {
                var lableCheckArgumentsRef = il.DefineLabel();
                var lableCheckArgumentsLength = il.DefineLabel();

                if (!methodInfo.IsStatic)
                {
                    il.Ldarg_0();
                    il.Brtrue_S(args.Length > 0 ? lableCheckArgumentsRef : lableValidationCompleted);

                    il.ThrowArgumentsNullExcpetion("instance");
                }

                if (args.Length > 0)
                {
                    il.MarkLabel(lableCheckArgumentsRef);
                    il.Ldarg_1();
                    il.Brtrue_S(lableCheckArgumentsLength);

                    il.ThrowArgumentsNullExcpetion("arguments");

                    il.MarkLabel(lableCheckArgumentsLength);
                    il.Ldarg_1();
                    il.Ldlen();
                    il.Conv_I4();
                    il.LoadInt32(args.Length);
                    il.Bge_S(lableValidationCompleted);

                    il.ThrowArgumentsExcpetion("Not enough arguments in the argument array.", "arguments");
                }
            }

            il.MarkLabel(lableValidationCompleted);
            if (!methodInfo.IsStatic)
            {
                il.Ldarg_0();
                il.CastReference(methodInfo.DeclaringType);
            }

            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    il.Ldarg_1();
                    il.LoadInt32((short)i);
                    il.Ldelem_Ref();
                    il.CastValue(args[i].ParameterType);
                }
            }

            il.CallMethod(methodInfo);
            if (methodInfo.ReturnType == typeof(void))
            {
                il.Ldc_I4_0();
            }
            else
            {
                il.BoxIfNeeded(methodInfo.ReturnType);
            }
            il.Ret();

            var methodDelegate = dynamicMethod.CreateDelegate(typeof(System.Func<object, object[], Result>));
            return (System.Func<object, object[], Result>)methodDelegate;
        }

        public static System.Action<object, object[]> CreateDelegate2
(System.Reflection.MethodInfo methodInfo, bool validateArguments = true, string methodName = null)
        {
            if (methodInfo == null)
            {
                throw new System.ArgumentNullException(nameof(methodInfo));
            }

            var args = methodInfo.GetParameters();
            var dynamicMethod = EmitUtils.CreateDynamicMethod(string.Format("{0}_{1}|{2}", "$Call", methodInfo.Name, methodName),
                null,
                new[] { typeof(object), typeof(object[]) },
                methodInfo.DeclaringType);
            var il = dynamicMethod.GetILGenerator();

            var lableValidationCompleted = il.DefineLabel();
            if (!validateArguments || (methodInfo.IsStatic && args.Length == 0))
            {
                il.Br_S(lableValidationCompleted);
            }
            else
            {
                var lableCheckArgumentsRef = il.DefineLabel();
                var lableCheckArgumentsLength = il.DefineLabel();

                if (!methodInfo.IsStatic)
                {
                    il.Ldarg_0();
                    il.Brtrue_S(args.Length > 0 ? lableCheckArgumentsRef : lableValidationCompleted);

                    il.ThrowArgumentsNullExcpetion("instance");
                }

                if (args.Length > 0)
                {
                    il.MarkLabel(lableCheckArgumentsRef);
                    il.Ldarg_1();
                    il.Brtrue_S(lableCheckArgumentsLength);

                    il.ThrowArgumentsNullExcpetion("arguments");

                    il.MarkLabel(lableCheckArgumentsLength);
                    il.Ldarg_1();
                    il.Ldlen();
                    il.Conv_I4();
                    il.LoadInt32(args.Length);
                    il.Bge_S(lableValidationCompleted);

                    il.ThrowArgumentsExcpetion("Not enough arguments in the argument array.", "arguments");
                }
            }

            il.MarkLabel(lableValidationCompleted);
            if (!methodInfo.IsStatic)
            {
                il.Ldarg_0();
                il.CastReference(methodInfo.DeclaringType);
            }

            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    il.Ldarg_1();
                    il.LoadInt32(i);
                    il.Ldelem_Ref();
                    il.CastValue(args[i].ParameterType);
                }
            }

            il.CallMethod(methodInfo);

            il.Ret();

            var methodDelegate = dynamicMethod.CreateDelegate(typeof(System.Action<object, object[]>));
            return (System.Action<object, object[]>)methodDelegate;
        }
    }
}
