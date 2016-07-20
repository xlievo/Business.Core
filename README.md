## Platform

Support .Net4.5 Mono 4.x

## Depend

Castle.Core 3.3.3

Microsoft.CSharp

## Install

NuGet:https://www.nuget.org/packages/Business.Core/

PM> Install-Package Business.Core

### Please add to

AssemblyInfo.cs

[assembly: InternalsVisibleTo("Business.Core")]

## Please refer unit test

#   a. Check 

	public struct Register
    {
		[Check(1)]
        [Size(12, "argument \"account\" size verification failed", Min = 4, Max = "8", TrimChar = true)]
        [CheckChar(-13, "\" char account\" verification failed", Mode = CheckCharAttribute.CheckCharMode.Number)]
        public string account;

        public int Password { get; set; }
    }

	public class CheckAttribute : ArgumentAttribute
    {
        public CheckAttribute(int code = -800, string message = null)
            : base(code, message) { }

        public override IResult Proces(dynamic value, System.Type type, string method, string member, dynamic business)
        {
            if (System.Object.Equals(null, value))
            {
                //Return error message
                return ResultFactory.Create(Code, Message ?? string.Format("argument \"{0}\" can not null.", member));
            }

            //Return pass or data
            return ResultFactory.Create();
        }
    }
    
	[BusinessLog]
	public class A : IBusiness
    {
		public A()
        {
            //[BusinessLog] control
            this.WriteLogAsync = (log) => 
            {
                //...
            };
        }

        public Action<BusinessLog> WriteLogAsync { get; set; }
        public System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IReadOnlyDictionary<string, BusinessCommand>> Command { get; set; }
        public Type ResultType { get; set; }

        public virtual IResult TestParameterA_01(Register ags)
        {
            return this.ResultCreate(new { ags.account, ags.Password });
        }

        public virtual IResult TestParameterA_02(Register ags, [Size(12, "password size verification failed", Min = 4, Max = "8")]int password)
        {
            return this.ResultCreate(new { ags.account, ags.Password, password });
        }
    }

	[TestClass]
    public class UnitTest1
    {

		//Initialization you code

        static A A2 = BusinessBind<A>.Create();
        static System.Collections.Generic.IReadOnlyDictionary<string, BusinessCommand> Cmd = A2.Command[BusinessBind.DefaultCommandGroup];

        [TestMethod]
        public void TestParameterA_01()
        {
            var result = A2.TestParameterA_01(new Register { account = "aaa" });
            var result2 = Cmd["TestParameterA_01"].Call(new Register { account = "aaa" });

            Assert.IsTrue(1 > result.State);
            Assert.IsTrue(System.Object.Equals(result.Message, "argument \"account\" size verification failed"));
            Assert.IsTrue(System.Object.Equals(result.State, result2.State));
            Assert.IsTrue(System.Object.Equals(result.Message, result2.Message));
            Assert.IsTrue(System.Object.Equals(result.Data, result2.Data));
        }

        [TestMethod]
        public void TestParameterA_02()
        {
            var result = A2.TestParameterA_01(new Register { account = "aaaa" });
            var result2 = Cmd["TestParameterA_01"].Call(new Register { account = "aaaa" });

            Assert.IsTrue(1 > result.State);
            Assert.IsTrue(System.Object.Equals(result.Message, "\" char account\" verification failed"));
            Assert.IsTrue(System.Object.Equals(result.State, result2.State));
            Assert.IsTrue(System.Object.Equals(result.Message, result2.Message));
            Assert.IsTrue(System.Object.Equals(result.Data, result2.Data));
        }

        [TestMethod]
        public void TestParameterA_03()
        {
            var result = A2.TestParameterA_01(new Register { account = "1111", Password = 1111 });
            var result2 = Cmd["TestParameterA_01"].Call(new Register { account = "1111", Password = 1111 });

            Assert.IsTrue(0 < result.State);
            Assert.IsTrue(System.Object.Equals(result.Data, new { account = "1111", Password = 1111 }));
            Assert.IsTrue(System.Object.Equals(result.Message, null));
            Assert.IsTrue(System.Object.Equals(result.State, result2.State));
            Assert.IsTrue(System.Object.Equals(result.Message, result2.Message));
            Assert.IsTrue(System.Object.Equals(result.Data, result2.Data));
        }

        [TestMethod]
        public void TestParameterA_04()
        {
            var result = A2.TestParameterA_02(new Register { account = "1111", Password = 1111 }, 6);
            var result2 = Cmd["TestParameterA_02"].Call(new Register { account = "1111", Password = 1111 }, 6);

            Assert.IsTrue(0 < result.State);
            Assert.IsTrue(System.Object.Equals(result.Data, new { account = "1111", Password = 1111, password = 6 }));
            Assert.IsTrue(System.Object.Equals(result.Message, null));
            Assert.IsTrue(System.Object.Equals(result.State, result2.State));
            Assert.IsTrue(System.Object.Equals(result.Message, result2.Message));
            Assert.IsTrue(System.Object.Equals(result.Data, result2.Data));
        }

        [TestMethod]
        public void TestParameterA_05()
        {
            var result = A2.TestParameterA_02(new Register { account = "1111", Password = 1111 }, 666);
            var result2 = Cmd["TestParameterA_02"].Call(new Register { account = "1111", Password = 1111 }, 666);

            Assert.IsTrue(1 > result.State);
            Assert.IsTrue(System.Object.Equals(result.Data, null));
            Assert.IsTrue(System.Object.Equals(result.Message, "password size verification failed"));
            Assert.IsTrue(System.Object.Equals(result.State, result2.State));
            Assert.IsTrue(System.Object.Equals(result.Message, result2.Message));
            Assert.IsTrue(System.Object.Equals(result.Data, result2.Data));
        }
	}

#   b. Convert 

	public struct Register
    {
        [Size(12, "argument \"account\" size verification failed", Min = 4, Max = "8", TrimChar = true)]
        [CheckChar(-13, "\" char account\" verification failed", Mode = CheckCharAttribute.CheckCharMode.Number)]
        public string account;

        public int Password { get; set; }
    }

	public class ConvertAttribute : DeserializeAttribute
    {
        public ConvertAttribute(int code = -911, string message = null)
            : base(code, message) { }

        public override IResult Proces(dynamic value, System.Type type, string method, string member, dynamic business)
        {
            try
            {
                //Return pass or data
                return ResultFactory.Create(value);
            }
            catch
            {
                //Return error message
                return ResultFactory.Create(Code, Message ?? string.Format("Arguments {0} deserialize error", member));
            }
        }
    }

	[BusinessLog]
	public class A : IBusiness
    {
        public Action<BusinessLog> WriteLogAsync { get; set; }
        public System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IReadOnlyDictionary<string, BusinessCommand>> Command { get; set; }
        public Type ResultType { get; set; }

        public virtual IResult TestParameterB_01([Convert(99)]Arg<Register> ags)
        {
            return this.ResultCreate(new { ags.Out.account, ags.Out.Password });
        }

        public virtual IResult TestParameterB_02([Convert(99)]Arg<Register> ags, [Size(12, "password size verification failed", Min = 4, Max = "8")]int password)
        {
            return this.ResultCreate(new { ags.Out.account, ags.Out.Password, password });
        }
    }

	[TestClass]
    public class UnitTest1
    {

		//Initialization you code

        static A A2 = BusinessBind<A>.Create();
        static System.Collections.Generic.IReadOnlyDictionary<string, BusinessCommand> Cmd = A2.Command[BusinessBind.DefaultCommandGroup];

        [TestMethod]
        public void TestParameterB_01()
        {
            var result = A2.TestParameterB_01(new Arg<Register> { In = new Register { account = "aaa" } });
            var result2 = Cmd["TestParameterB_01"].Call(new Register { account = "aaa" });

            Assert.IsTrue(1 > result.State);
            Assert.IsTrue(System.Object.Equals(result.Message, "argument \"account\" size verification failed"));
            Assert.IsTrue(System.Object.Equals(result.State, result2.State));
            Assert.IsTrue(System.Object.Equals(result.Message, result2.Message));
            Assert.IsTrue(System.Object.Equals(result.Data, result2.Data));
        }

        [TestMethod]
        public void TestParameterB_02()
        {
            var result = A2.TestParameterB_02(new Arg<Register> { In = new Register { account = "1111" } }, 666);
            var result2 = Cmd["TestParameterB_02"].Call(new Register { account = "1111" }, 666);

            Assert.IsTrue(1 > result.State);
            Assert.IsTrue(System.Object.Equals(result.Data, null));
            Assert.IsTrue(System.Object.Equals(result.Message, "password size verification failed"));
            Assert.IsTrue(System.Object.Equals(result.State, result2.State));
            Assert.IsTrue(System.Object.Equals(result.Message, result2.Message));
            Assert.IsTrue(System.Object.Equals(result.Data, result2.Data));
        }
    }

#   c. Group

	[ConvertC(99, Group = "CCC")]
    public struct Register
    {
        [Size(12, "argument \"account\" size verification failed", Min = 4, Max = "8", TrimChar = true)]
        [CheckChar(-13, "\" char account\" verification failed", Mode = CheckCharAttribute.CheckCharMode.Number)]
        public string account;

        public int Password { get; set; }
    }
    
    public class ConvertAttribute : DeserializeAttribute
    {
        public ConvertAttribute(int code = -911, string message = null)
            : base(code, message) { }

        public override IResult Proces(dynamic value, System.Type type, string method, string member, dynamic business)
        {
            try
            {
                //Return pass or data
                return ResultFactory.Create(value);
            }
            catch
            {
                //Return error message
                return ResultFactory.Create(Code, Message ?? string.Format("Arguments {0} deserialize error", member));
            }
        }
    }

    public class ConvertAAttribute : DeserializeAttribute
    {
        public ConvertAAttribute(int code = -911, string message = null)
            : base(code, message) { }

        public override IResult Proces(dynamic value, System.Type type, string method, string member, dynamic business)
        {
            try
            {
                //Return pass or data
                return ResultFactory.Create(value);
            }
            catch
            {
                //Return error message
                return ResultFactory.Create(Code, Message ?? string.Format("Arguments {0} deserialize error", member));
            }
        }
    }

    public class ConvertBAttribute : DeserializeAttribute
    {
        public ConvertBAttribute(int code = -911, string message = null)
            : base(code, message) { }

        public override IResult Proces(dynamic value, System.Type type, string method, string member, dynamic business)
        {
            try
            {
                //Return pass or data
                return ResultFactory.Create(value);
            }
            catch
            {
                //Return error message
                return ResultFactory.Create(Code, Message ?? string.Format("Arguments {0} deserialize error", member));
            }
        }
    }

    public class ConvertCAttribute : DeserializeAttribute
    {
        public ConvertCAttribute(int code = -911, string message = null)
            : base(code, message) { }

        public override IResult Proces(dynamic value, System.Type type, string method, string member, dynamic business)
        {
            try
            {
                //Return pass or data
                return ResultFactory.Create(value);
            }
            catch
            {
                //Return error message
                return ResultFactory.Create(Code, Message ?? string.Format("Arguments {0} deserialize error", member));
            }
        }
    }
    
    [BusinessLog]
    public class A : IBusiness
    {
        public Action<BusinessLog> WriteLogAsync { get; set; }
        public System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IReadOnlyDictionary<string, BusinessCommand>> Command { get; set; }
        public Type ResultType { get; set; }

        [Command(Group = "AAA")]
        [Command(Group = "BBB")]
        public virtual IResult TestParameterC_01([Convert(99)][ConvertA(99, Group = "AAA")][ConvertB(99, Group = "BBB")]Arg<Register> ags)
        {
            return this.ResultCreate(new { ags.Out.account, ags.Out.Password });
        }

        [Command(Group = "AAA")]
        [Command(Group = "CCC")]
        public virtual IResult TestParameterC_02([Convert(99)][ConvertA(99, Group = "AAA")]Arg<Register> ags, [Size(12, "password size verification failed", Min = 4, Max = "8")]int password)
        {
            return this.ResultCreate(new { ags.Out.account, ags.Out.Password, password });
        }
    }
    
    [TestClass]
    public class UnitTest1
    {

		//Initialization you code

        static A A2 = BusinessBind<A>.Create();

        [TestMethod]
        public void TestParameterC_01()
        {
            var cmd_A = A2.Command["AAA"];
            var cmd_B = A2.Command["BBB"];

            var result = A2.TestParameterC_01(new Arg<Register> { In = new Register { account = "aaa" } });
            var result2 = cmd_A["TestParameterC_01"].Call(new Register { account = "aaa" });
            var result3 = cmd_B["TestParameterC_01"].Call(new Register { account = "aaa" });

            Assert.IsTrue(1 > result.State);
            Assert.IsTrue(System.Object.Equals(result.Message, "argument \"account\" size verification failed"));
            Assert.IsTrue(System.Object.Equals(result.State, result2.State));
            Assert.IsTrue(System.Object.Equals(result.Message, result2.Message));
            Assert.IsTrue(System.Object.Equals(result.Data, result2.Data));
        }

        [TestMethod]
        public void TestParameterC_02()
        {
            var cmd_A = A2.Command["AAA"];
            var cmd_C = A2.Command["CCC"];

            var result = A2.TestParameterC_02(new Arg<Register> { In = new Register { account = "1111" } }, 666);
            var result2 = cmd_A["TestParameterC_02"].Call(new Register { account = "1111" }, 666);
            var result3 = cmd_C["TestParameterC_02"].Call(new Register { account = "1111" }, 666);

            Assert.IsTrue(1 > result.State);
            Assert.IsTrue(System.Object.Equals(result.Data, null));
            Assert.IsTrue(System.Object.Equals(result.Message, "password size verification failed"));
            Assert.IsTrue(System.Object.Equals(result.State, result2.State));
            Assert.IsTrue(System.Object.Equals(result.Message, result2.Message));
            Assert.IsTrue(System.Object.Equals(result.Data, result2.Data));
        }
    }
