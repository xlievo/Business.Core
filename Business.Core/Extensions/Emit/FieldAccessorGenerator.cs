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
    public static class FieldAccessorGenerator
    {
        public static System.Func<object, object> CreateGetter(System.Reflection.FieldInfo fieldInfo)
        {
            return CreateGetter<object, object>(fieldInfo);
        }

        public static System.Func<TSource, TRet> CreateGetter<TSource, TRet>(System.Reflection.FieldInfo fieldInfo)
        {
            if (fieldInfo == null)
            {
                throw new System.ArgumentNullException("fieldInfo");
            }

            if (typeof(TSource) != typeof(object)
                && !fieldInfo.DeclaringType.IsAssignableFrom(typeof(TSource)))
            {
                throw new System.ArgumentException(
                   "The field's declaring type is not assignable from the type of the instance.",
                   "fieldInfo");
            }

            if (!typeof(TRet).IsAssignableFrom(fieldInfo.FieldType))
            {
                throw new System.ArgumentException(
                    "The type of the return value is not assignable from the type of the field.",
                    "fieldInfo");
            }

            return EmitFieldGetter<TSource, TRet>(fieldInfo);
        }

        public static System.Action<object, object> CreateSetter(System.Reflection.FieldInfo fieldInfo)
        {
            return CreateSetter<object, object>(fieldInfo);
        }

        public static System.Action<TTarget, TValue> CreateSetter<TTarget, TValue>(System.Reflection.FieldInfo fieldInfo)
        {
            if (fieldInfo == null)
            {
                throw new System.ArgumentNullException("fieldInfo");
            }

            if (typeof(TTarget).IsValueType)
            {
                throw new System.ArgumentException(
                   "The type of the isntance should not be a value type. " +
                   "For a value type, use System.Object instead.",
                   "fieldInfo");
            }

            if (typeof(TTarget) != typeof(object)
                && !fieldInfo.DeclaringType.IsAssignableFrom(typeof(TTarget)))
            {
                throw new System.ArgumentException(
                   "The declaring type of the field is not assignable from the type of the isntance.",
                   "fieldInfo");
            }

            if (typeof(TValue) != typeof(object)
                && !fieldInfo.FieldType.IsAssignableFrom(typeof(TValue)))
            {
                throw new System.ArgumentException(
                    "The type of the field is not assignable from the type of the value.",
                    "fieldInfo");
            }

            return EmitFieldSetter<TTarget, TValue>(fieldInfo);
        }

        private static System.Func<TSource, TReturn> EmitFieldGetter<TSource, TReturn>(System.Reflection.FieldInfo fieldInfo)
        {
            var dynamicMethod = EmitUtils.CreateDynamicMethod(string.Format("{0}_{1}", "$Get", fieldInfo.Name),
                typeof(TReturn),
                new[] { typeof(TSource) },
                fieldInfo.DeclaringType);
            var il = dynamicMethod.GetILGenerator();

            if (fieldInfo.IsStatic)
            {
                il.Ldsfld(fieldInfo);
            }
            else
            {
                if (typeof(TSource).IsValueType)
                {
                    il.Ldarga_S(0);
                }
                else
                {
                    il.Ldarg_0();
                    il.CastReference(fieldInfo.DeclaringType);
                }

                il.Ldfld(fieldInfo);
            }

            if (!typeof(TReturn).IsValueType && fieldInfo.FieldType.IsValueType)
            {
                il.Box(fieldInfo.FieldType);
            }
            il.Ret();

            return (System.Func<TSource, TReturn>)dynamicMethod.CreateDelegate(typeof(System.Func<TSource, TReturn>));
        }

        private static System.Action<TTarget, TValue> EmitFieldSetter<TTarget, TValue>(System.Reflection.FieldInfo fieldInfo)
        {
            var dynamicMethod = EmitUtils.CreateDynamicMethod(string.Format("{0}_{1}", "$Set", fieldInfo.Name),
                null,
                new[] { typeof(TTarget), typeof(TValue) },
                fieldInfo.DeclaringType);
            var il = dynamicMethod.GetILGenerator();

            il.DeclareLocal(fieldInfo.FieldType);
            il.Ldarg_1();
            if (!typeof(TValue).IsValueType)
            {
                il.CastValue(fieldInfo.FieldType);
            }
            il.Stloc_0();

            if (fieldInfo.IsStatic)
            {
                il.Ldloc_0();
                il.Stsfld(fieldInfo);
            }
            else
            {
                il.Ldarg_0();
                il.CastReference(fieldInfo.DeclaringType);
                il.Ldloc_0();
                il.Stfld(fieldInfo);
            }

            il.Ret();
            return (System.Action<TTarget, TValue>)dynamicMethod.CreateDelegate(typeof(System.Action<TTarget, TValue>));
        }
    }
}
