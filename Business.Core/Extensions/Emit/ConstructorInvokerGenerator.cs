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
    public static class ConstructorInvokerGenerator
    {
        public static System.Func<Type> CreateDelegate<Type>(System.Type type)
        {
            if (type == null)
            {
                throw new System.ArgumentNullException("type");
            }

            if (type.IsInterface)
            {
                throw new System.ArgumentException(string.Format("{0} is an interface.", type), "type");
            }

            if (type.IsAbstract)
            {
                throw new System.ArgumentException(string.Format("{0} is abstract.", type), "type");
            }

            System.Reflection.ConstructorInfo constructorInfo = null;
            if (type.IsClass)
            {
                constructorInfo = type.GetConstructor(System.Type.EmptyTypes);

                if (constructorInfo == null)
                {
                    throw new System.ArgumentException(string.Format("{0} does not have a public parameterless constructor.", type), "type");
                }
            }

            var dynamicMethod = EmitUtils.CreateDynamicMethod(string.Format("{0}_{1}", "$Create", type), typeof(Type), System.Type.EmptyTypes, type);
            var il = dynamicMethod.GetILGenerator();

            if (type.IsClass)
            {
                il.Newobj(constructorInfo);
            }
            else
            {
                il.DeclareLocal(type);
                il.LoadLocalVariableAddress(0);
                il.Initobj(type);

                il.LoadLocalVariable(0);
                il.Box(type);
            }

            il.Ret();
            return (System.Func<Type>)dynamicMethod.CreateDelegate(typeof(System.Func<Type>));
        }

        public static System.Func<object[], object> CreateDelegate(System.Reflection.ConstructorInfo constructorInfo, bool validateArguments = true)
        {
            if (constructorInfo == null)
            {
                throw new System.ArgumentNullException("constructorInfo");
            }

            var delclaringType = constructorInfo.DeclaringType;
            if (delclaringType.IsAbstract)
            {
                throw new System.ArgumentException("The declaring type of the constructor is abstract.", "constructorInfo");
            }
            
            var dynamicMethod = EmitUtils.CreateDynamicMethod(string.Format("{0}_{1}", "$Create", delclaringType.Name),
                typeof(object),
                new[] { typeof(object[]) },
                constructorInfo.DeclaringType);
            var il = dynamicMethod.GetILGenerator();

            var args = constructorInfo.GetParameters();
            var lableValidationCompleted = il.DefineLabel();
            if (!validateArguments || args.Length == 0)
            {
                il.Br_S(lableValidationCompleted);
            }
            else
            {
                var lableCheckArgumentsLength = il.DefineLabel();

                il.Ldarg_0();
                il.Brtrue_S(lableCheckArgumentsLength);

                il.ThrowArgumentsNullExcpetion("arguments");

                il.MarkLabel(lableCheckArgumentsLength);
                il.Ldarg_0();
                il.Ldlen();
                il.Conv_I4();
                il.LoadInt32(args.Length);
                il.Bge_S(lableValidationCompleted);

                il.ThrowArgumentsExcpetion("Not enough arguments in the argument array.", "arguments");
            }

            il.MarkLabel(lableValidationCompleted);
            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    il.Ldarg_0();
                    il.LoadInt32(i);
                    il.Ldelem_Ref();
                    il.CastValue(args[i].ParameterType);
                }
            }
            il.Newobj(constructorInfo);
            il.Ret();

            return (System.Func<object[], object>)dynamicMethod.CreateDelegate(typeof(System.Func<object[], object>));
        }
    }
}
