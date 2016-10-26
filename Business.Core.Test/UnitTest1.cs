using System;
using Business.Attributes;
using Business.Result;
using NUnit.Framework;

namespace Business.Core.Test
{
    [ConvertC(-99, Group = "CCC")]
    public struct Register
    {
        [Check(1)]
        [Size(12, "argument \"account\" size verification failed", Min = 4, Max = "8", TrimChar = true)]
        [CheckChar(-13, "\" char account\" verification failed", Mode = CheckCharAttribute.CheckCharMode.Number)]
        public string account;

        public int Password { get; set; }

        public Register2 Password2 { get; set; }
    }

    [CanNotNull(98, "\"Register2\" not is null")]
    //[Convert(99)]
    public class Register2
    {
        [CanNotNull(-11, "\"Register2 account\" not is null")]
        [Size(12, "Register2 argument \"account\" size verification failed", Min = 4, Max = "8", TrimChar = true)]
        [CheckChar(-13, "\" char account\" verification failed", Mode = CheckCharAttribute.CheckCharMode.Number)]
        public string account;

        public Register Password2 { get; set; }
    }

    public enum RegisterType { }

    public class CheckAttribute : ArgumentAttribute
    {
        public CheckAttribute(int code = -800, string message = null)
            : base(code, message) { }

        public override IResult Proces(dynamic value, Type type, string method, string member, dynamic business)
        {
            if (Object.Equals(null, value))
            {
                //Return error message
                return this.ResultCreate(Code, Message ?? string.Format("argument \"{0}\" can not null.", member));
            }

            //Return pass or data
            return this.ResultCreate();
        }
    }

    public class ConvertAttribute : ArgumentAttribute
    {
        public ConvertAttribute(int code = -911, string message = null)
            : base(code, message) { }

        public override IResult Proces(dynamic value, System.Type type, string method, string member, dynamic business)
        {
            try
            {
                //Return pass or data
                return this.ResultCreate(value);
            }
            catch
            {
                //Return error message
                return this.ResultCreate(Code, Message ?? string.Format("Arguments {0} deserialize error", member));
            }
        }
    }

    public class ConvertAAttribute : ArgumentAttribute
    {
        public ConvertAAttribute(int code = -911, string message = null)
            : base(code, message) { }

        public override IResult Proces(dynamic value, System.Type type, string method, string member, dynamic business)
        {
            try
            {
                //Return pass or data
                return this.ResultCreate(value);
            }
            catch
            {
                //Return error message
                return this.ResultCreate(Code, Message ?? string.Format("Arguments {0} deserialize error", member));
            }
        }
    }

    public class ConvertBAttribute : ArgumentAttribute
    {
        public ConvertBAttribute(int code = -911, string message = null)
            : base(code, message) { }

        public override IResult Proces(dynamic value, System.Type type, string method, string member, dynamic business)
        {
            try
            {
                //Return pass or data
                return this.ResultCreate(value);
            }
            catch
            {
                //Return error message
                return this.ResultCreate(Code, Message ?? string.Format("Arguments {0} deserialize error", member));
            }
        }
    }

    public class ConvertCAttribute : ArgumentAttribute
    {
        public ConvertCAttribute(int code = -911, string message = null)
            : base(code, message) { }

        public override IResult Proces(dynamic value, System.Type type, string method, string member, dynamic business)
        {
            try
            {
                //Return pass or data
                return this.ResultCreate(value);
            }
            catch
            {
                //Return error message
                return this.ResultCreate(Code, Message ?? string.Format("Arguments {0} deserialize error", member));
            }
        }
    }

    [Logger(LogType.Record, CanValue = LoggerAttribute.ValueMode.All)]
    [Logger(LogType.Exception, CanValue = LoggerAttribute.ValueMode.All)]
    [Logger(LogType.Error, CanValue = LoggerAttribute.ValueMode.All)]
    public class A : IBusiness
    {
        public A()
        {
            this.WriteLogAsync = log =>
            {
                //...
            };
        }

        public Action<Log> WriteLogAsync { get; set; }
        public System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IReadOnlyDictionary<string, Command>> Command { get; set; }
        public Type ResultType { get; set; }
        public Func<Auth.IToken> Token { get; set; }
        public IConfig Config { get; set; }

        //=========================Check============================//
        [Logger(LogType.Record, CanResult = true)]
        [Logger(LogType.Record, CanValue = LoggerAttribute.ValueMode.Select)]
        //[Logger(LogType.Exception, CanValue = true)]
        [Logger(LogType.Error, CanValue = LoggerAttribute.ValueMode.All)]
        public virtual IResult TestParameterA_01([Logger(LogType.Record)]Register ags)
        {
            //return this.ResultCreate(new { ags.account, ags.Password }, true, this.ResultCreate(commandID: "id2"), this.ResultCreate("", 1, true, "id"));
            return this.ResultCreate(new { ags.account, ags.Password });
        }

        public virtual IResult TestParameterA_02(Register ags, [Size(12, "password size verification failed", Min = 4, Max = "8")]int password)
        {
            return this.ResultCreate(new { ags.account, ags.Password, password });
        }

        //==========================Convert===========================//
        public virtual IResult TestParameterB_01([Convert(99)]Arg<Register> ags)
        {
            return this.ResultCreate(new { ags.Out.account, ags.Out.Password });
        }

        public virtual IResult TestParameterB_02([Convert(99)]Arg<Register> ags, [Size(12, "password size verification failed", Min = 4, Max = "8")]int password)
        {
            return this.ResultCreate(new { ags.Out.account, ags.Out.Password, password });
        }

        //==========================Group===========================//
        [Command(Group = "AAA")]
        [Command(Group = "BBB")]
        public virtual IResult TestParameterC_01([Convert(99)][ConvertA(99, Group = "AAA")][ConvertB(99, Group = "BBB")]Arg<Register> ags)
        {
            return this.ResultCreate(ags.Group);
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
        static A A2 = Bind<A>.Create();
        static System.Collections.Generic.IReadOnlyDictionary<string, Command> Cmd = A2.Command[Bind.CommandGroupDefault];

        static UnitTest1()
        {
            //A2.TestParameterA_01(new Register { account = "1111", Password = 1111, Password2 = new Register2 { account = "111" } });
            //A2.TestParameterC_02(new Arg<Register> { In = new Register { account = "1111", Password2 = new Register2 { account = "11111" } } }, 666);
        }

        static bool IsClass<T>()
        {
            var type = typeof(T);

            var result = !type.IsPrimitive && !type.IsEnum && !type.IsArray && !type.IsSecurityTransparent;

            return result;
        }

        [Test]
        public void TestMethod0()
        {
            var result = IsClass<System.DateTime>();
            result = result ? result : IsClass<System.String>();
            result = result ? result : IsClass<System.Object>();
            result = result ? result : IsClass<System.Int32>();
            result = result ? result : IsClass<System.UInt32>();
            result = result ? result : IsClass<System.Byte>();
            result = result ? result : IsClass<System.Byte[]>();
            result = result ? result : IsClass<System.Char>();
            result = result ? result : IsClass<System.IntPtr>();
            result = result ? result : IsClass<System.DateTime>();
            result = result ? result : IsClass<System.Collections.Generic.List<string>>();
            result = result ? result : IsClass<System.Decimal>();
            result = result ? result : IsClass<System.Guid>();
            result = result ? result : IsClass<System.Nullable<int>>();
            result = result ? result : IsClass<System.Random>();
            result = result ? result : IsClass<System.TimeZone>();
            result = result ? result : IsClass<RegisterType>();
            Assert.IsFalse(result);
            result = result ? result : IsClass<Register>();
            result = !result ? result : IsClass<Register2>();
            Assert.IsTrue(result);
        }

        #region TestParameterA

        [Test]
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

        [Test]
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

        [Test]
        public void TestParameterA_03()
        {
            var result = A2.TestParameterA_01(new Register { account = "1111", Password = 1111, Password2 = new Register2 { account = "111" } });
            var result2 = Cmd["TestParameterA_01"].Call(new Register { account = "1111", Password = 1111, Password2 = new Register2 { account = "111" } });
            Assert.IsTrue(0 < result.State);
            Assert.IsTrue(System.Object.Equals(result.Data, new { account = "1111", Password = 1111 }));
            Assert.IsTrue(System.Object.Equals(result.Message, null));
            Assert.IsTrue(System.Object.Equals(result.State, result2.State));
            Assert.IsTrue(System.Object.Equals(result.Message, result2.Message));
            Assert.IsTrue(System.Object.Equals(result.Data, result2.Data));
        }

        [Test]
        public void TestParameterA_04()
        {
            var result = A2.TestParameterA_02(new Register { account = "1111", Password = 1111, Password2 = new Register2 { account = "11111" } }, 6);
            var result2 = Cmd["TestParameterA_02"].Call(new Register { account = "1111", Password = 1111, Password2 = new Register2 { account = "11111" } }, 6);

            Assert.IsTrue(0 < result.State);
            Assert.IsTrue(System.Object.Equals(result.Data, new { account = "1111", Password = 1111, password = 6 }));
            Assert.IsTrue(System.Object.Equals(result.Message, null));
            Assert.IsTrue(System.Object.Equals(result.State, result2.State));
            Assert.IsTrue(System.Object.Equals(result.Message, result2.Message));
            Assert.IsTrue(System.Object.Equals(result.Data, result2.Data));
        }

        [Test]
        public void TestParameterA_05()
        {
            var result = A2.TestParameterA_02(new Register { account = "1111", Password = 1111, Password2 = new Register2 { account = "11111" } }, 666);
            var result2 = Cmd["TestParameterA_02"].Call(new Register { account = "1111", Password = 1111, Password2 = new Register2 { account = "11111" } }, 666);

            Assert.IsTrue(1 > result.State);
            Assert.IsTrue(System.Object.Equals(result.Data, null));
            Assert.IsTrue(System.Object.Equals(result.Message, "password size verification failed"));
            Assert.IsTrue(System.Object.Equals(result.State, result2.State));
            Assert.IsTrue(System.Object.Equals(result.Message, result2.Message));
            Assert.IsTrue(System.Object.Equals(result.Data, result2.Data));
        }

        #endregion

        #region TestParameterB

        [Test]
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

        [Test]
        public void TestParameterB_02()
        {
            var result = A2.TestParameterB_02(new Arg<Register> { In = new Register { account = "1111", Password2 = new Register2 { account = "11111" } } }, 666);
            var result2 = Cmd["TestParameterB_02"].Call(new Register { account = "1111", Password2 = new Register2 { account = "11111" } }, 666);

            Assert.IsTrue(1 > result.State);
            Assert.IsTrue(System.Object.Equals(result.Data, null));
            Assert.IsTrue(System.Object.Equals(result.Message, "password size verification failed"));
            Assert.IsTrue(System.Object.Equals(result.State, result2.State));
            Assert.IsTrue(System.Object.Equals(result.Message, result2.Message));
            Assert.IsTrue(System.Object.Equals(result.Data, result2.Data));
        }

        #endregion

        #region TestParameterC

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
            Assert.IsTrue(System.Object.Equals(result.Data, null));
            Assert.IsTrue(System.Object.Equals(result.Message, "password size verification failed"));
            Assert.IsTrue(System.Object.Equals(result.State, result2.State));
            Assert.IsTrue(System.Object.Equals(result.Message, result2.Message));
            Assert.IsTrue(System.Object.Equals(result.Data, result2.Data));
        }

        #endregion

        [Test]
        public void TestLogger_01()
        {
            var result = A2.TestParameterA_01(new Register { account = "aaa" });
            var result2 = Cmd["TestParameterA_01"].Call(new Register { account = "aaa" });

            Assert.IsTrue(1 > result.State);
            Assert.IsTrue(System.Object.Equals(result.Message, "argument \"account\" size verification failed"));
            Assert.IsTrue(System.Object.Equals(result.State, result2.State));
            Assert.IsTrue(System.Object.Equals(result.Message, result2.Message));
            Assert.IsTrue(System.Object.Equals(result.Data, result2.Data));
        }
    }


}
