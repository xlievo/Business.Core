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

namespace Business.Extensions.Emit
{
    internal static class EmitUtils
    {
        public static System.Reflection.Emit.DynamicMethod CreateDynamicMethod(
            string methodName, System.Type returnType, System.Type[] parameterTypes, System.Type owner)
        {
            var dynamicMethod = owner.IsInterface ? new System.Reflection.Emit.DynamicMethod(methodName, returnType, parameterTypes, owner.Module, true) : new System.Reflection.Emit.DynamicMethod(methodName, returnType, parameterTypes, owner, true);

            return dynamicMethod;
        }

        public static System.Reflection.Emit.ILGenerator BoxIfNeeded(this System.Reflection.Emit.ILGenerator il, System.Type type)
        {
            if (type.IsValueType)
            {
                il.Box(type);
            }

            return il;
        }

        public static System.Reflection.Emit.ILGenerator UnBoxIfNeeded(this System.Reflection.Emit.ILGenerator il, System.Type type)
        {
            if (type.IsValueType)
            {
                il.Unbox_Any(type);
            }

            return il;
        }

        public static System.Reflection.Emit.ILGenerator CallMethod(this System.Reflection.Emit.ILGenerator il, System.Reflection.MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new System.ArgumentNullException("methodInfo");
            }

            if (methodInfo.IsVirtual)
            {
                il.Callvirt(methodInfo);
            }
            else
            {
                il.Call(methodInfo);
            }

            return il;
        }

        public static System.Reflection.Emit.ILGenerator CastValue(this System.Reflection.Emit.ILGenerator il, System.Type targetType)
        {
            il.Unbox_Any(targetType);
            return il;
        }

        public static System.Reflection.Emit.ILGenerator CastReference(this System.Reflection.Emit.ILGenerator il, System.Type targetType)
        {
            if (targetType.IsValueType)
            {
                il.Unbox(targetType);
            }
            else
            {
                il.Castclass(targetType);
            }

            return il;
        }

        public static System.Reflection.Emit.ILGenerator LoadArgument(this System.Reflection.Emit.ILGenerator il, short index)
        {
            if (index < 0)
            {
                throw new System.ArgumentOutOfRangeException("index", "The index should not be less than zero.");
            }

            switch (index)
            {
                case 0: il.Ldarg_0(); break;
                case 1: il.Ldarg_1(); break;
                case 2: il.Ldarg_2(); break;
                case 3: il.Ldarg_3(); break;

                default:
                    if (index <= byte.MaxValue)
                    {
                        il.Ldarg_S((byte)index);
                    }
                    else
                    {
                        il.Ldarg(index);
                    }
                    break;
            }

            return il;
        }

        public static System.Reflection.Emit.ILGenerator LoadArgumentAddress(this System.Reflection.Emit.ILGenerator il, short index)
        {
            if (index < 0)
            {
                throw new System.ArgumentOutOfRangeException("index", "The index should not be less than zero.");
            }

            if (index <= byte.MaxValue)
            {
                il.Ldarga_S((byte)index);
            }
            else
            {
                il.Ldarga(index);
            }

            return il;
        }

        public static System.Reflection.Emit.ILGenerator LoadInt32(this System.Reflection.Emit.ILGenerator il, int i)
        {
            switch (i)
            {
                case -1: il.Ldc_I4_M1(); break;
                case 0: il.Ldc_I4_0(); break;
                case 1: il.Ldc_I4_1(); break;
                case 2: il.Ldc_I4_2(); break;
                case 3: il.Ldc_I4_3(); break;
                case 4: il.Ldc_I4_4(); break;
                case 5: il.Ldc_I4_5(); break;
                case 6: il.Ldc_I4_6(); break;
                case 7: il.Ldc_I4_7(); break;
                case 8: il.Ldc_I4_8(); break;

                default:
                    if (i <= byte.MaxValue)
                    {
                        il.Ldc_I4_S((byte)i);
                    }
                    else
                    {
                        il.Ldc_I4(i);
                    }
                    break;
            }

            return il;
        }

        public static System.Reflection.Emit.ILGenerator LoadLocalVariable(this System.Reflection.Emit.ILGenerator il, short index)
        {
            if (index < 0)
            {
                throw new System.ArgumentOutOfRangeException("index", "The index should not be less than zero.");
            }

            switch (index)
            {
                case 0: il.Ldloc_0(); break;
                case 1: il.Ldloc_1(); break;
                case 2: il.Ldloc_2(); break;
                case 3: il.Ldloc_3(); break;

                default:
                    if (index <= byte.MaxValue)
                    {
                        il.Ldloc_S((byte)index);
                    }
                    else
                    {
                        il.Ldloc(index);
                    }
                    break;
            }

            return il;
        }

        public static System.Reflection.Emit.ILGenerator LoadLocalVariableAddress(this System.Reflection.Emit.ILGenerator il, short index)
        {
            if (index < 0)
            {
                throw new System.ArgumentOutOfRangeException("index", "The index should not be less than zero.");
            }

            if (index <= byte.MaxValue)
            {
                il.Ldloca_S((byte)index);
            }
            else
            {
                il.Ldloca(index);
            }

            return il;
        }

        public static System.Reflection.Emit.ILGenerator ThrowArgumentsNullExcpetion(this System.Reflection.Emit.ILGenerator il, string paramName)
        {
            il.Ldstr(paramName);
            il.Newobj(typeof(System.ArgumentNullException).GetConstructor(new[] { typeof(string) }));
            il.Throw();

            return il;
        }

        public static System.Reflection.Emit.ILGenerator ThrowArgumentsExcpetion(this System.Reflection.Emit.ILGenerator il, string message, string paramName)
        {
            il.Ldstr(message);
            il.Ldstr(paramName);
            il.Newobj(typeof(System.ArgumentException).GetConstructor(new[] { typeof(string), typeof(string) }));
            il.Throw();

            return il;
        }
    }
}
