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
    using System.Reflection;
    using System.Reflection.Emit;

    internal static class EmitUtils
    {
        public static DynamicMethod CreateDynamicMethod(
            string methodName, System.Type returnType, System.Type[] parameterTypes, System.Type owner)
        {
               var ownerTypeInfo = owner.GetTypeInfo();
            var dynamicMethod = ownerTypeInfo.IsInterface ? new DynamicMethod(methodName, returnType, parameterTypes, ownerTypeInfo.Module, true) : new DynamicMethod(methodName, returnType, parameterTypes, owner, true);

            return dynamicMethod;
        }

        public static ILGenerator BoxIfNeeded(this ILGenerator il, System.Type type)
        {
            if (type.GetTypeInfo().IsValueType)
            {
                il.Box(type);
            }

            return il;
        }

        public static ILGenerator UnBoxIfNeeded(this ILGenerator il, System.Type type)
        {
            if (type.GetTypeInfo().IsValueType)
            {
                il.Unbox_Any(type);
            }

            return il;
        }

        public static ILGenerator CallMethod(this ILGenerator il, System.Reflection.MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new System.ArgumentNullException(nameof(methodInfo));
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

        public static ILGenerator CastValue(this ILGenerator il, System.Type targetType)
        {
            il.Unbox_Any(targetType);
            return il;
        }

        public static ILGenerator CastReference(this ILGenerator il, System.Type targetType)
        {
            if (targetType.GetTypeInfo().IsValueType)
            {
                il.Unbox(targetType);
            }
            else
            {
                il.Castclass(targetType);
            }

            return il;
        }

        public static ILGenerator LoadArgument(this ILGenerator il, short index)
        {
            if (index < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(index), "The index should not be less than zero.");
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

        public static ILGenerator LoadArgumentAddress(this ILGenerator il, short index)
        {
            if (index < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(index), "The index should not be less than zero.");
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

        public static ILGenerator LoadInt32(this ILGenerator il, int i)
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

        public static ILGenerator LoadLocalVariable(this ILGenerator il, short index)
        {
            if (index < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(index), "The index should not be less than zero.");
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

        public static ILGenerator LoadLocalVariableAddress(this ILGenerator il, short index)
        {
            if (index < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(index), "The index should not be less than zero.");
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

        public static ILGenerator ThrowArgumentsNullExcpetion(this ILGenerator il, string paramName)
        {
            il.Ldstr(paramName);
            il.Newobj(typeof(System.ArgumentNullException).GetConstructor(new[] { typeof(string) }));
            il.Throw();

            return il;
        }

        public static ILGenerator ThrowArgumentsExcpetion(this ILGenerator il, string message, string paramName)
        {
            il.Ldstr(message);
            il.Ldstr(paramName);
            il.Newobj(typeof(System.ArgumentException).GetConstructor(new[] { typeof(string), typeof(string) }));
            il.Throw();

            return il;
        }
    }
}
