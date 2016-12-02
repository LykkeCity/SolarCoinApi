using NSubstitute;
using NUnit.Framework;
using SolarCoinApi.Core.Log;
using SolarCoinApi.RpcJson.JsonRpc;
using SolarCoinApi.CashOutJobRunner;
using System.Threading.Tasks;

namespace SolarCoinApi.Tests.CashOutJobRunner
{
    [TestFixture]
    public class Tests
    {
        [TestCase(0.1, "jdljlkjd")]
        [TestCase(125.26, "asdfasdf")]
        [TestCase(10, "8ejqpoicv")]
        public async Task cashout_test(double amountToTransferD, string address)
        {
            decimal amountToTransfer = (decimal)amountToTransferD;

            var message = new ToSendMessageFromQueue { Address = address, Amount = amountToTransfer };

            var fakeRpcClient = Substitute.For<IJsonRpcClient>();
            fakeRpcClient.SendToAddress(address, amountToTransfer).Returns("sometxid");

            var fakeLogger = Substitute.For<ILog>();

            var queueTrigger = new CashOutQueueTrigger(fakeRpcClient, fakeLogger);


            //Act
            await queueTrigger.ReceiveMessage(message);


            //Assert
            await fakeRpcClient.Received().SendToAddress(address, amountToTransfer);
        }
    }
}
