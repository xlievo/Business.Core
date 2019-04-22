using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using WebSocket4Net;

namespace WebSocketTest
{
    public struct ReceiveData
    {
        /// <summary>
        /// business
        /// </summary>
        public string a { get; set; }

        /// <summary>
        /// cmd
        /// </summary>
        public string c { get; set; }

        /// <summary>
        /// token
        /// </summary>
        public string t { get; set; }

        /// <summary>
        /// data
        /// </summary>
        public byte[] d { get; set; }

        /// <summary>
        /// callback
        /// </summary>
        public string b { get; set; }
    }

    public class ResultObject
    {
        /// <summary>
        /// The results of the state is greater than or equal to 1: success, equal to 0: system level exceptions, less than 0: business class error.
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// Success can be null
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Specific Byte/Json data objects
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// Whether there is value
        /// </summary>
        public bool HasData { get; set; }

        /// <summary>
        /// Gets the token of this result, used for callback
        /// </summary>
        public string Callback { get; set; }
    }

    public struct Test001
    {
        public string A { get; set; }

        public string B { get; set; }
    }

    public struct Result
    {
        public Token token { get; set; }

        public List<Test001> arg { get; set; }
    }

    public struct Token
    {
        public string Key { get; set; }

        public string Remote { get; set; }
    }

    [TestClass]
    public class UnitTest1
    {
        public static bool SpinWait(int millisecondsTimeout) => System.Threading.SpinWait.SpinUntil(() => false, millisecondsTimeout);

        static WebSocket webSocketClient = new WebSocket("ws://localhost:5000/ws");

        static UnitTest1()
        {
            MessagePack.Resolvers.CompositeResolver.RegisterAndSetAsDefault(MessagePack.Resolvers.ContractlessStandardResolver.Instance);

            webSocketClient.Error += (o, e) =>
            {

            };

            //ConfigSecurity(webSocketClient);

            webSocketClient.Closed += (o, e) =>
            {

            };

            webSocketClient.DataReceived += (o, e) =>
            {
                var result = Business.Utils.Help2.TryBinaryDeserialize<ResultObject>(e.Data);

                if (0 < result.State)
                {
                    var resultData = Business.Utils.Help2.TryBinaryDeserialize<Result>(result.Data);

                }
            };
        }

        [TestMethod]
        public void TestWebSocketOrg()
        {
            webSocketClient.Open();

            SpinWait(3000);

            System.Threading.Tasks.Parallel.For(0, 10, c =>
            {
                var d = MessagePack.MessagePackSerializer.Serialize(new List<Test001> { new Test001 { A = "aaa", B = c.ToString() } });
                var data = MessagePack.MessagePackSerializer.Serialize(new ReceiveData { a = "API", c = "Test004", d = d });

                webSocketClient.Send(data, 0, data.Length);
            });

            SpinWait(3000);

            webSocketClient.Close();
        }
    }
}
