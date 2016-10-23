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
    public static class PropertyAccessorGenerator
    {
        public static System.Func<object, object> CreateGetter(System.Reflection.PropertyInfo propertyInfo, bool nonPublic = true)
        {
            return CreateGetter<object, object>(propertyInfo, nonPublic);
        }

        public static System.Func<TSource, TRet> CreateGetter<TSource, TRet>(System.Reflection.PropertyInfo propertyInfo, bool nonPublic)
        {
            if (propertyInfo == null)
            {
                throw new System.ArgumentNullException("propertyInfo");
            }

            if (propertyInfo.GetIndexParameters().Length > 0)
            {
                throw new System.ArgumentException("Cannot create a dynamic getter for anindexed property.", "propertyInfo");
            }

            if (typeof(TSource) != typeof(object)
                && !propertyInfo.DeclaringType.IsAssignableFrom(typeof(TSource)))
            {
                throw new System.ArgumentException("The declaring type of the property is not assignable from the type of the instance.", "propertyInfo");
            }

            if (!typeof(TRet).IsAssignableFrom(propertyInfo.PropertyType))
            {
                throw new System.ArgumentException("The type of the return value is not assignable from the type of the property.", "propertyInfo");
            }

            //the method call of the get accessor method fails in runtime 
            //if the declaring type of the property is an interface and TSource is a value type, 
            //in this case, we should find the property from TSource whose DeclaringType is TSource itself
            if (typeof(TSource).IsValueType && propertyInfo.DeclaringType.IsInterface)
            {
                propertyInfo = typeof(TSource).GetProperty(propertyInfo.Name);
            }

            var getMethod = propertyInfo.GetGetMethod(nonPublic);
            if (getMethod == null)
            {
                //if (nonPublic)
                //{
                //    throw new System.ArgumentException("The property does not have a get method.", "propertyInfo");
                //}

                //throw new System.ArgumentException("The property does not have a public get method.", "propertyInfo");

                return null;
            }

            return EmitPropertyGetter<TSource, TRet>(propertyInfo, getMethod);
        }

        public static System.Action<object, object> CreateSetter(System.Reflection.PropertyInfo propertyInfo, bool nonPublic = true)
        {
            return CreateSetter<object, object>(propertyInfo, nonPublic);
        }

        public static System.Action<TTarget, TValue> CreateSetter<TTarget, TValue>(System.Reflection.PropertyInfo propertyInfo, bool nonPublic)
        {
            if (propertyInfo == null)
            {
                throw new System.ArgumentNullException("propertyInfo");
            }

            if (typeof(TTarget).IsValueType)
            {
                throw new System.ArgumentException("The type of the isntance should not be a value type. " + "For a value type, use System.Object instead.", "propertyInfo");
            }

            if (propertyInfo.GetIndexParameters().Length > 0)
            {
                throw new System.ArgumentException("Cannot create a dynamic setter for an indexed property.", "propertyInfo");
            }

            if (typeof(TTarget) != typeof(object)
                && !propertyInfo.DeclaringType.IsAssignableFrom(typeof(TTarget)))
            {
                throw new System.ArgumentException("The declaring type of the property is not assignable from the type of the isntance.", "propertyInfo");
            }

            if (typeof(TValue) != typeof(object)
                && !propertyInfo.PropertyType.IsAssignableFrom(typeof(TValue)))
            {
                throw new System.ArgumentException("The type of the property is not assignable from the type of the value.", "propertyInfo");
            }

            var setMethod = propertyInfo.GetSetMethod(nonPublic);
            if (setMethod == null)
            {
                //if (nonPublic)
                //{
                //    throw new System.ArgumentException("The property does not have a set method.", "propertyInfo");
                //}

                //throw new System.ArgumentException("The property does not have a public set method.", "propertyInfo");
                return null;
            }

            return EmitPropertySetter<TTarget, TValue>(propertyInfo, setMethod);
        }

        private static System.Func<TSource, TReturn> EmitPropertyGetter<TSource, TReturn>(System.Reflection.PropertyInfo propertyInfo, System.Reflection.MethodInfo getMethod)
        {
            var dynamicMethod = EmitUtils.CreateDynamicMethod(string.Format("{0}_{1}", "$Get", propertyInfo.Name),
                typeof(TReturn),
                new[] { typeof(TSource) },
                propertyInfo.DeclaringType);
            var il = dynamicMethod.GetILGenerator();

            if (!getMethod.IsStatic)
            {
                if (typeof(TSource).IsValueType)
                {
                    il.Ldarga_S(0);
                }
                else
                {
                    il.Ldarg_0();
                    il.CastReference(propertyInfo.DeclaringType);
                }
            }

            il.CallMethod(getMethod);

            if (!typeof(TReturn).IsValueType && propertyInfo.PropertyType.IsValueType)
            {
                il.Box(propertyInfo.PropertyType);
            }

            il.Ret();

            return (System.Func<TSource, TReturn>)dynamicMethod.CreateDelegate(typeof(System.Func<TSource, TReturn>));
        }

        private static System.Action<TTarget, TValue> EmitPropertySetter<TTarget, TValue>(System.Reflection.PropertyInfo propertyInfo, System.Reflection.MethodInfo setMethod)
        {
            var propType = propertyInfo.PropertyType;
            var declaringType = propertyInfo.DeclaringType;
            var dynamicMethod = EmitUtils.CreateDynamicMethod(string.Format("{0}_{1}", "$Set", propertyInfo.Name),
                null,
                new[] { typeof(TTarget), typeof(TValue) },
                declaringType);
            var il = dynamicMethod.GetILGenerator();

            il.DeclareLocal(propType);
            il.Ldarg_1();
            if (!typeof(TValue).IsValueType)
            {
                il.CastValue(propType);
            }
            il.Stloc_0();

            if (!setMethod.IsStatic)
            {
                il.Ldarg_0();
                il.CastReference(declaringType);
            }

            il.Ldloc_0();
            il.CallMethod(setMethod);
            il.Ret();

            return (System.Action<TTarget, TValue>)dynamicMethod.CreateDelegate(typeof(System.Action<TTarget, TValue>));
        }
    }
}
