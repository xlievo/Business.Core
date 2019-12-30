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

namespace Business.Core.Utils.Emit
{
    using System.Reflection;
    using System.Reflection.Emit;

    internal static class Emit
    {
        public static ILGenerator CallMethod(this ILGenerator il, MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new System.ArgumentNullException(nameof(methodInfo));
            }

            if (methodInfo.IsVirtual)
            {
                il.Emit(OpCodes.Callvirt, methodInfo);
            }
            else
            {
                il.Emit(OpCodes.Call, methodInfo);
            }

            return il;
        }

        public static ILGenerator CastReference(this ILGenerator il, System.Type targetType)
        {
            if (targetType == null)
            {
                throw new System.ArgumentNullException(nameof(targetType));
            }

            if (targetType.GetTypeInfo().IsValueType)
            {
                il.Emit(OpCodes.Unbox, targetType);
            }
            else
            {
                il.Emit(OpCodes.Castclass, targetType);
            }

            return il;
        }

        /// <summary>
        /// Gets the declaring type of the target <paramref name="method"/>.
        /// </summary>
        /// <param name="method">The <see cref="MethodInfo"/> for which to return the declaring type.</param>
        /// <returns>The type that declares the target <paramref name="method"/>.</returns>
        public static System.Type GetDeclaringType(this MethodInfo method)
        {
            var declaringType = method.DeclaringType;
            if (declaringType == null)
            {
                throw new System.ArgumentException($"Method {method} does not have a declaring type", nameof(method));
            }

            return declaringType;
        }
    }

    /// <summary>
    /// Represents the skeleton of a dynamic method.
    /// </summary>    
    public interface IDynamicMethodSkeleton
    {
        /// <summary>
        /// Gets the <see cref="ILGenerator"/> used to emit the method body.
        /// </summary>
        /// <returns>An <see cref="ILGenerator"/> instance.</returns>
        ILGenerator GetILGenerator();

        /// <summary>
        /// Create a delegate used to invoke the dynamic method.
        /// </summary>
        /// <returns>A function delegate.</returns>
        System.Func<object, object[], object> CreateDelegate();
    }

    /// <summary>
    /// Represents a class that is capable of creating a delegate used to invoke 
    /// a method without using late-bound invocation.
    /// </summary>
    public interface IMethodBuilder
    {
        /// <summary>
        /// Gets a delegate that is used to invoke the <paramref name="targetMethod"/>.
        /// </summary>
        /// <param name="targetMethod">The <see cref="MethodInfo"/> that represents the target method to invoke.</param>
        /// <returns>A delegate that represents compiled code used to invoke the <paramref name="targetMethod"/>.</returns>
        System.Func<object, object[], object> GetDelegate(MethodInfo targetMethod);
    }

    /// <summary>
    /// A class that is capable of creating a delegate used to invoke 
    /// a method without using late-bound invocation.
    /// </summary>
    public class DynamicMethodBuilder : IMethodBuilder
    {
        private readonly System.Func<IDynamicMethodSkeleton> methodSkeletonFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicMethodBuilder"/> class.
        /// </summary>
        public DynamicMethodBuilder()
        {
            methodSkeletonFactory = () => new DynamicMethodSkeleton();
        }

        /// <summary>
        /// Gets a delegate that is used to invoke the <paramref name="targetMethod"/>.
        /// </summary>
        /// <param name="targetMethod">The <see cref="MethodInfo"/> that represents the target method to invoke.</param>
        /// <returns>A delegate that represents compiled code used to invoke the <paramref name="targetMethod"/>.</returns>
        public System.Func<object, object[], object> GetDelegate(MethodInfo targetMethod)
        {
            var parameters = targetMethod.GetParameters();
            var methodSkeleton = methodSkeletonFactory();
            var il = methodSkeleton.GetILGenerator();
            PushInstance(targetMethod, il);
            PushArguments(parameters, il);
            CallTargetMethod(targetMethod, il);
            UpdateOutAndRefArguments(parameters, il);
            PushReturnValue(targetMethod, il);
            return methodSkeleton.CreateDelegate();
        }

        private static void PushReturnValue(MethodInfo method, ILGenerator il)
        {
            if (method.ReturnType == typeof(void))
            {
                il.Emit(OpCodes.Ldnull);
            }
            else
            {
                BoxIfNecessary(method.ReturnType, il);
            }

            il.Emit(OpCodes.Ret);
        }

        private static void PushArguments(ParameterInfo[] parameters, ILGenerator il)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                PushObjectValueFromArgumentArray(il, i);
                PushArgument(parameters[i], il);
            }
        }

        private static void PushArgument(ParameterInfo parameter, ILGenerator il)
        {
            var parameterType = parameter.ParameterType;
            if (parameter.IsOut || parameter.ParameterType.IsByRef)
            {
                PushOutOrRefArgument(parameter, il);
            }
            else
            {
                UnboxOrCast(parameterType, il);
            }
        }

        private static void PushOutOrRefArgument(ParameterInfo parameter, ILGenerator il)
        {
            var parameterType = parameter.ParameterType.GetElementType();
            LocalBuilder outValue = il.DeclareLocal(parameterType);
            UnboxOrCast(parameterType, il);
            il.Emit(OpCodes.Stloc, outValue);
            il.Emit(OpCodes.Ldloca, outValue);
        }

        private static void PushObjectValueFromArgumentArray(ILGenerator il, int parameterIndex)
        {
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldc_I4, parameterIndex);
            il.Emit(OpCodes.Ldelem_Ref);
        }

        private static void CallTargetMethod(MethodInfo method, ILGenerator il)
        {
            il.Emit(method.IsAbstract ? OpCodes.Callvirt : OpCodes.Call, method);
        }

        private static void PushInstance(MethodInfo method, ILGenerator il)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, method.GetDeclaringType());
        }

        private static void UnboxOrCast(System.Type parameterType, ILGenerator il)
        {
            //il.Emit(parameterType.GetTypeInfo().IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, parameterType);

            if (parameterType.GetTypeInfo().IsValueType)
            {
                il.Emit(OpCodes.Unbox_Any, parameterType);
            }
            else
            {
                il.Emit(OpCodes.Castclass, typeof(object));
            }
        }

        private static void UpdateOutAndRefArguments(ParameterInfo[] parameters, ILGenerator il)
        {
            int localIndex = 0;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].IsOut || parameters[i].ParameterType.IsByRef)
                {
                    var parameterType = parameters[i].ParameterType.GetElementType();
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldc_I4, i);
                    il.Emit(OpCodes.Ldloc, localIndex);
                    BoxIfNecessary(parameterType, il);
                    il.Emit(OpCodes.Stelem_Ref);
                    localIndex++;
                }
            }
        }

        private static void BoxIfNecessary(System.Type parameterType, ILGenerator il)
        {
            if (parameterType.GetTypeInfo().IsValueType)
            {
                il.Emit(OpCodes.Box, parameterType);
            }
        }

        public class DynamicMethodSkeleton : IDynamicMethodSkeleton
        {
            public readonly DynamicMethod dynamicMethod;

            public DynamicMethodSkeleton() => dynamicMethod = new DynamicMethod("DynamicMethod", typeof(object), new[] { typeof(object), typeof(object[]) }, typeof(DynamicMethodSkeleton).GetTypeInfo().Module, true);

            public DynamicMethodSkeleton(System.Type returnType, System.Type[] parameterTypes, System.Type owner, bool skipVisibility = true)
            {
                var ownerTypeInfo = owner.GetTypeInfo();
                dynamicMethod = ownerTypeInfo.IsInterface ? new DynamicMethod("DynamicMethod", returnType, parameterTypes, ownerTypeInfo.Module, skipVisibility) : new DynamicMethod("DynamicMethod", returnType, parameterTypes, owner, skipVisibility);
            }

            /// <summary>
            /// Gets the <see cref="ILGenerator"/> used to emit the method body.
            /// </summary>
            /// <returns>An <see cref="ILGenerator"/> instance.</returns>
            public ILGenerator GetILGenerator() => dynamicMethod.GetILGenerator();

            /// <summary>
            /// Create a delegate used to invoke the dynamic method.
            /// </summary>
            /// <returns>A function delegate.</returns>
            public System.Func<object, object[], object> CreateDelegate() => (System.Func<object, object[], object>)dynamicMethod.CreateDelegate(typeof(System.Func<object, object[], object>));

            public System.Delegate CreateDelegate(System.Type delegateType) => dynamicMethod.CreateDelegate(delegateType);
        }
    }

    public static class FieldAccessorGenerator
    {
        public static System.Func<object, object> CreateGetter(FieldInfo fieldInfo)
        {
            return CreateGetter<object, object>(fieldInfo);
        }

        public static System.Func<TSource, TRet> CreateGetter<TSource, TRet>(FieldInfo fieldInfo)
        {
            if (fieldInfo == null)
            {
                throw new System.ArgumentNullException(nameof(fieldInfo));
            }

            if (typeof(TSource) != typeof(object)
                && !fieldInfo.DeclaringType.IsAssignableFrom(typeof(TSource)))
            {
                throw new System.ArgumentException(
                   "The field's declaring type is not assignable from the type of the instance.",
                   nameof(fieldInfo));
            }

            if (!typeof(TRet).IsAssignableFrom(fieldInfo.FieldType))
            {
                throw new System.ArgumentException(
                    "The type of the return value is not assignable from the type of the field.",
                   nameof(fieldInfo));
            }

            return EmitFieldGetter<TSource, TRet>(fieldInfo);
        }

        public static System.Action<object, object> CreateSetter(FieldInfo fieldInfo)
        {
            return CreateSetter<object, object>(fieldInfo);
        }

        public static System.Action<TTarget, TValue> CreateSetter<TTarget, TValue>(FieldInfo fieldInfo)
        {
            if (fieldInfo == null)
            {
                throw new System.ArgumentNullException(nameof(fieldInfo));
            }

            if (typeof(TTarget).GetTypeInfo().IsValueType)
            {
                throw new System.ArgumentException(
                   "The type of the isntance should not be a value type. " +
                   "For a value type, use System.Object instead.",
                   nameof(fieldInfo));
            }

            if (typeof(TTarget) != typeof(object)
                && !fieldInfo.DeclaringType.IsAssignableFrom(typeof(TTarget)))
            {
                throw new System.ArgumentException(
                   "The declaring type of the field is not assignable from the type of the isntance.",
                   nameof(fieldInfo));
            }

            if (typeof(TValue) != typeof(object)
                && !fieldInfo.FieldType.IsAssignableFrom(typeof(TValue)))
            {
                throw new System.ArgumentException(
                    "The type of the field is not assignable from the type of the value.",
                    nameof(fieldInfo));
            }

            return EmitFieldSetter<TTarget, TValue>(fieldInfo);
        }

        private static System.Func<TSource, TReturn> EmitFieldGetter<TSource, TReturn>(FieldInfo fieldInfo)
        {
            if (fieldInfo.IsStatic && fieldInfo.IsLiteral)
            {
                return null;
            }

            var dynamicMethod = new DynamicMethodBuilder.DynamicMethodSkeleton(typeof(TReturn), new[] { typeof(TSource) }, fieldInfo.DeclaringType);

            var il = dynamicMethod.GetILGenerator();

            if (fieldInfo.IsStatic)
            {
                il.Emit(OpCodes.Ldsfld, fieldInfo);
            }
            else
            {
                if (typeof(TSource).GetTypeInfo().IsValueType)
                {
                    il.Emit(OpCodes.Ldarga_S, 0);
                }
                else
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.CastReference(fieldInfo.DeclaringType);
                }

                il.Emit(OpCodes.Ldfld, fieldInfo);
            }

            if (!typeof(TReturn).GetTypeInfo().IsValueType && fieldInfo.FieldType.GetTypeInfo().IsValueType)
            {
                il.Emit(OpCodes.Box, fieldInfo.FieldType);
            }
            il.Emit(OpCodes.Ret);

            return (System.Func<TSource, TReturn>)dynamicMethod.CreateDelegate(typeof(System.Func<TSource, TReturn>));
        }

        private static System.Action<TTarget, TValue> EmitFieldSetter<TTarget, TValue>(FieldInfo fieldInfo)
        {
            if (fieldInfo.IsStatic && fieldInfo.IsLiteral)
            {
                return null;
            }

            var dynamicMethod = new DynamicMethodBuilder.DynamicMethodSkeleton(null, new[] { typeof(TTarget), typeof(TValue) }, fieldInfo.DeclaringType);

            var il = dynamicMethod.GetILGenerator();

            il.DeclareLocal(fieldInfo.FieldType);
            il.Emit(OpCodes.Ldarg_1);
            if (!typeof(TValue).GetTypeInfo().IsValueType)
            {
                il.Emit(OpCodes.Unbox_Any, fieldInfo.FieldType);
            }
            il.Emit(OpCodes.Stloc_0);

            if (fieldInfo.IsStatic)
            {
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Stsfld, fieldInfo);
            }
            else
            {
                il.Emit(OpCodes.Ldarg_0);
                il.CastReference(fieldInfo.DeclaringType);
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Stfld, fieldInfo);
            }

            il.Emit(OpCodes.Ret);
            return (System.Action<TTarget, TValue>)dynamicMethod.CreateDelegate(typeof(System.Action<TTarget, TValue>));
        }
    }

    public static class PropertyAccessorGenerator
    {
        public static System.Func<object, object> CreateGetter(PropertyInfo propertyInfo, bool nonPublic = true)
        {
            return CreateGetter<object, object>(propertyInfo, nonPublic);
        }

        public static System.Func<TSource, TRet> CreateGetter<TSource, TRet>(PropertyInfo propertyInfo, bool nonPublic)
        {
            if (propertyInfo == null)
            {
                throw new System.ArgumentNullException(nameof(propertyInfo));
            }

            if (propertyInfo.GetIndexParameters().Length > 0)
            {
                //throw new System.ArgumentException("Cannot create a dynamic getter for anindexed property.", "propertyInfo");
                return null;
            }

            if (typeof(TSource) != typeof(object)
                && !propertyInfo.DeclaringType.IsAssignableFrom(typeof(TSource)))
            {
                //throw new System.ArgumentException("The declaring type of the property is not assignable from the type of the instance.", "propertyInfo");
                return null;
            }

            if (!typeof(TRet).IsAssignableFrom(propertyInfo.PropertyType))
            {
                //throw new System.ArgumentException("The type of the return value is not assignable from the type of the property.", "propertyInfo");
                return null;
            }

            //the method call of the get accessor method fails in runtime 
            //if the declaring type of the property is an interface and TSource is a value type, 
            //in this case, we should find the property from TSource whose DeclaringType is TSource itself

            if (typeof(TSource).GetTypeInfo().IsValueType && propertyInfo.DeclaringType.GetTypeInfo().IsInterface)
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

        public static System.Action<object, object> CreateSetter(PropertyInfo propertyInfo, bool nonPublic = true)
        {
            return CreateSetter<object, object>(propertyInfo, nonPublic);
        }

        public static System.Action<TTarget, TValue> CreateSetter<TTarget, TValue>(PropertyInfo propertyInfo, bool nonPublic)
        {
            if (propertyInfo == null)
            {
                throw new System.ArgumentNullException(nameof(propertyInfo));
            }

            if (typeof(TTarget).GetTypeInfo().IsValueType)
            {
                //throw new System.ArgumentException("The type of the isntance should not be a value type. " + "For a value type, use System.Object instead.", "propertyInfo");
                return null;
            }

            if (propertyInfo.GetIndexParameters().Length > 0)
            {
                //throw new System.ArgumentException("Cannot create a dynamic setter for an indexed property.", "propertyInfo");
                return null;
            }

            if (typeof(TTarget) != typeof(object)
                && !propertyInfo.DeclaringType.IsAssignableFrom(typeof(TTarget)))
            {
                //throw new System.ArgumentException("The declaring type of the property is not assignable from the type of the isntance.", "propertyInfo");
                return null;
            }

            if (typeof(TValue) != typeof(object)
                && !propertyInfo.PropertyType.IsAssignableFrom(typeof(TValue)))
            {
                //throw new System.ArgumentException("The type of the property is not assignable from the type of the value.", "propertyInfo");
                return null;
            }

            var setMethod = propertyInfo.GetSetMethod(nonPublic) ?? propertyInfo.DeclaringType.GetProperty(propertyInfo.Name)?.GetSetMethod(nonPublic);
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

        private static System.Func<TSource, TReturn> EmitPropertyGetter<TSource, TReturn>(PropertyInfo propertyInfo, MethodInfo getMethod)
        {

            var dynamicMethod = new DynamicMethodBuilder.DynamicMethodSkeleton(typeof(TReturn), new[] { typeof(TSource) }, propertyInfo.DeclaringType);

            var il = dynamicMethod.GetILGenerator();

            if (!getMethod.IsStatic)
            {
                if (typeof(TSource).GetTypeInfo().IsValueType)
                {
                    il.Emit(OpCodes.Ldarga_S, 0);
                }
                else
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.CastReference(propertyInfo.DeclaringType);
                }
            }

            il.CallMethod(getMethod);

            if (!typeof(TReturn).GetTypeInfo().IsValueType && propertyInfo.PropertyType.GetTypeInfo().IsValueType)
            {
                il.Emit(OpCodes.Box, propertyInfo.PropertyType);
            }

            il.Emit(OpCodes.Ret);

            return (System.Func<TSource, TReturn>)dynamicMethod.CreateDelegate(typeof(System.Func<TSource, TReturn>));
        }

        private static System.Action<TTarget, TValue> EmitPropertySetter<TTarget, TValue>(PropertyInfo propertyInfo, MethodInfo setMethod)
        {
            var dynamicMethod = new DynamicMethodBuilder.DynamicMethodSkeleton(null, new[] { typeof(TTarget), typeof(TValue) }, propertyInfo.DeclaringType);

            var il = dynamicMethod.GetILGenerator();

            il.DeclareLocal(propertyInfo.PropertyType);
            il.Emit(OpCodes.Ldarg_1);
            if (!typeof(TValue).GetTypeInfo().IsValueType)
            {
                il.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
            }
            il.Emit(OpCodes.Stloc_0);

            if (!setMethod.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.CastReference(propertyInfo.DeclaringType);
            }

            il.Emit(OpCodes.Ldloc_0);
            il.CallMethod(setMethod);
            il.Emit(OpCodes.Ret);

            return (System.Action<TTarget, TValue>)dynamicMethod.CreateDelegate(typeof(System.Action<TTarget, TValue>));
        }
    }
}
