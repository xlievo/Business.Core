## Platform

Support .Net4.5 Mono 4.x

## Depend

Castle.Core 4.0.0

Newtonsoft.Json 10.0.1

protobuf-net 2.1.0

## Install

NuGet:https://www.nuget.org/packages/Business.Core/

PM> Install-Package Business.Core

### Please add to AssemblyInfo.cs

[assembly: InternalsVisibleTo("Business.Core")]

## Please refer unit test

#   a. Check 

	public struct Register
    {
		[Check(1)]
        [Size(12, "argument \"account\" size verification failed", Min = 4, Max = "8", TrimChar = true)]
        [CheckNull(-13, "\" char account\" verification failed", Mode = Help.CheckCharMode.Number)]
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
    
	[Logger(LoggerType.Record, CanValue = ValueMode.All)]
    public class A : IBusiness
    {
        public A()
        {
            //[Logger(LoggerType.Record, CanValue = ValueMode.All)] control
            this.Logger = log =>
            {
                //...
            };
        }

        public Action<LoggerData> Logger { get; set; }
        public System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IReadOnlyDictionary<string, Command>> Command { get; set; }
        public Type ResultType { get; set; }
        public Func<Auth.IToken> Token { get; set; }
        public IConfiguration Configuration { get; set; }

        public virtual IResult TestParameterA_01(Register ags)
        {
            return this.ResultCreate(new { ags.account, ags.Password });
        }

        public virtual IResult TestParameterA_02(Register ags, [Size(12, "password size verification failed", Min = 4, Max = "8")]int password)
        {
            return this.ResultCreate(new { ags.account, ags.Password, password });
        }
    }

	[TestFixture]
    public class UnitTest1
    {

		//Initialization you code

        static A A2 = Bind<A>.Create();
        static System.Collections.Generic.IReadOnlyDictionary<string, Command> Cmd = A2.Command[Bind.CommandGroupDefault];

        [Test]
        public void TestParameterA_01()
        {
            var result = A2.TestParameterA_01(new Register { account = "aaa" });
            var result2 = Cmd["TestParameterA_01"].Call(new Register { account = "aaa" });

            Assert.IsTrue(1 > result.State);
            Assert.AreEqual(result.Message, "argument \"account\" size verification failed");
            Assert.AreEqual(result.State, result2.State);
            Assert.AreEqual(result.Message, result2.Message);
            Assert.AreEqual(result.Data, result2.Data);
        }

        [Test]
        public void TestParameterA_02()
        {
            var result = A2.TestParameterA_01(new Register { account = "aaaa" });
            var result2 = Cmd["TestParameterA_01"].Call(new Register { account = "aaaa" });

            Assert.IsTrue(1 > result.State);
            Assert.AreEqual(result.Message, "\" char account\" verification failed");
            Assert.AreEqual(result.State, result2.State);
            Assert.AreEqual(result.Message, result2.Message);
            Assert.AreEqual(result.Data, result2.Data);
        }

        [Test]
        public void TestParameterA_03()
        {
           var result = A2.TestParameterA_01(new Register { account = "1111", Password = 1111, Password2 = new Register2 { account = "111" } });
            var result2 = Cmd["TestParameterA_01"].Call(new Register { account = "1111", Password = 1111, Password2 = new Register2 { account = "111" } });
            Assert.IsTrue(0 < result.State);
            Assert.AreEqual(result.Data, new { account = "1111", Password = 1111 });
            Assert.AreEqual(result.Message, null);
            Assert.AreEqual(result.State, result2.State);
            Assert.AreEqual(result.Message, result2.Message);
            Assert.AreEqual(result.Data, result2.Data);
        }

        [Test]
        public void TestParameterA_04()
        {
            var result = A2.TestParameterA_02(new Register { account = "1111", Password = 1111, Password2 = new Register2 { account = "11111" } }, 6);
            var result2 = Cmd["TestParameterA_02"].Call(new Register { account = "1111", Password = 1111, Password2 = new Register2 { account = "11111" } }, 6);

            Assert.IsTrue(0 < result.State);
            Assert.AreEqual(result.Data, new { account = "1111", Password = 1111, password = 6 });
            Assert.AreEqual(result.Message, null);
            Assert.AreEqual(result.State, result2.State);
            Assert.AreEqual(result.Message, result2.Message);
            Assert.AreEqual(result.Data, result2.Data);
        }

        [Test]
        public void TestParameterA_05()
        {
            var result = A2.TestParameterA_02(new Register { account = "1111", Password = 1111, Password2 = new Register2 { account = "11111" } }, 666);
            var result2 = Cmd["TestParameterA_02"].Call(new Register { account = "1111", Password = 1111, Password2 = new Register2 { account = "11111" } }, 666);

            Assert.IsTrue(1 > result.State);
            Assert.AreEqual(result.Data, null);
            Assert.AreEqual(result.Message, "password size verification failed");
            Assert.AreEqual(result.State, result2.State);
            Assert.AreEqual(result.Message, result2.Message);
            Assert.AreEqual(result.Data, result2.Data);
        }
	}

#   b. Convert 

	public struct Register
    {
        [Size(12, "argument \"account\" size verification failed", Min = 4, Max = "8", TrimChar = true)]
        [CheckNull(-13, "\" char account\" verification failed", Mode = Help.CheckCharMode.Number)]
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

	[Logger(LoggerType.Record, CanValue = ValueMode.All)]
	public class A : IBusiness
    {
        public Action<LoggerData> Logger { get; set; }
        public System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IReadOnlyDictionary<string, Command>> Command { get; set; }
        public Type ResultType { get; set; }
        public Func<Auth.IToken> Token { get; set; }
        public IConfiguration Configuration { get; set; }

        public virtual IResult TestParameterB_01([Convert(99)]Arg<Register> ags)
        {
            return this.ResultCreate(new { ags.Out.account, ags.Out.Password });
        }

        public virtual IResult TestParameterB_02([Convert(99)]Arg<Register> ags, [Size(12, "password size verification failed", Min = 4, Max = "8")]int password)
        {
            return this.ResultCreate(new { ags.Out.account, ags.Out.Password, password });
        }
    }

	[TestFixture]
    public class UnitTest1
    {

		//Initialization you code

        static A A2 = Bind<A>.Create();
        static System.Collections.Generic.IReadOnlyDictionary<string, Command> Cmd = A2.Command[Bind.CommandGroupDefault];

        [Test]
        public void TestParameterB_01()
        {
            var result = A2.TestParameterB_01(new Arg<Register> { In = new Register { account = "aaa" } });
            var result2 = Cmd["TestParameterB_01"].Call(new Register { account = "aaa" });

            Assert.IsTrue(1 > result.State);
            Assert.AreEqual(result.Message, "argument \"account\" size verification failed");
            Assert.AreEqual(result.State, result2.State);
            Assert.AreEqual(result.Message, result2.Message);
            Assert.AreEqual(result.Data, result2.Data);
        }

        [Test]
        public void TestParameterB_02()
        {
            var result = A2.TestParameterB_02(new Arg<Register> { In = new Register { account = "1111", Password2 = new Register2 { account = "11111" } } }, 666);
            var result2 = Cmd["TestParameterB_02"].Call(new Register { account = "1111", Password2 = new Register2 { account = "11111" } }, 666);

            Assert.IsTrue(1 > result.State);
            Assert.AreEqual(result.Data, null);
            Assert.AreEqual(result.Message, "password size verification failed");
            Assert.AreEqual(result.State, result2.State);
            Assert.AreEqual(result.Message, result2.Message);
            Assert.AreEqual(result.Data, result2.Data);
        }
    }

#   c. Group

	[ConvertC(99, Group = "CCC")]
    public struct Register
    {
        [Size(12, "argument \"account\" size verification failed", Min = 4, Max = "8", TrimChar = true)]
        [CheckNull(-13, "\" char account\" verification failed", Mode = Help.CheckCharMode.Number)]
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
    
    [Logger(LoggerType.Record, CanValue = ValueMode.All)]
    public class A : IBusiness
    {
        public Action<LoggerData> Logger { get; set; }
        public System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IReadOnlyDictionary<string, Command>> Command { get; set; }
        public Type ResultType { get; set; }
        public Func<Auth.IToken> Token { get; set; }
        public IConfiguration Configuration { get; set; }

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
    
    [TestFixture]
    public class UnitTest1
    {

		//Initialization you code

        static A A2 = Bind<A>.Create();

        [Test]
        public void TestParameterC_01()
        {
            var cmd_A = A2.Command["AAA"];
            var cmd_B = A2.Command["BBB"];

            var result = A2.TestParameterC_01(new Arg<Register> { In = new Register { account = "1111", Password2 = new Register2 { } } });
            var result2 = cmd_A["TestParameterC_01"].Call(new Register { account = "1111", Password2 = new Register2 { } });
            var result3 = cmd_B["TestParameterC_01"].Call(new Register { account = "1111", Password2 = new Register2 { } });

            Assert.IsTrue(0 < result.State);
            Assert.AreEqual(result.Data, "DefaultTestParameterC_01");
            Assert.AreEqual(result2.Data, "AAATestParameterC_01");
            Assert.AreEqual(result3.Data, "BBBTestParameterC_01");
        }

        [Test]
        public void TestParameterC_02()
        {
            var cmd_A = A2.Command["AAA"];
            var cmd_C = A2.Command["CCC"];

            var result = A2.TestParameterC_02(new Arg<Register> { In = new Register { account = "1111", Password2 = new Register2 { account = "11111" } } }, 666);
            var result2 = cmd_A["TestParameterC_02"].Call(new Register { account = "1111", Password2 = new Register2 { account = "11111" } }, 666);
            var result3 = cmd_C["TestParameterC_02"].Call(new Register { account = "1111", Password2 = new Register2 { account = "11111" } }, 666);

            Assert.IsTrue(1 > result.State);
            Assert.AreEqual(result.Data, null);
            Assert.AreEqual(result.Message, "password size verification failed");
            Assert.AreEqual(result.State, result2.State);
            Assert.AreEqual(result.Message, result2.Message);
            Assert.AreEqual(result.Data, result2.Data);
        }
    }
