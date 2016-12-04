using NSubstitute;
using NUnit.Framework;
using SolarCoinApi.Core.Log;
using SolarCoinApi.RpcJson.JsonRpc;
using SolarCoinApi.CashOutJobRunner;
using System.Threading.Tasks;
using SolarCoinApi.AzureStorage.Tables;
using System.Collections.Generic;
using System.Linq;

namespace SolarCoinApi.Tests.CashOutJobRunner
{
    [TestFixture]
    public class Tests
    {
        [TestCase("id1", 0.1, "jdljlkjd")]
        [TestCase("id2", 125.26, "asdfasdf")]
        [TestCase("id3", 10, "8ejqpoicv")]
        public async Task cashout_test_new_txes(string messageId, double amountToTransferD, string address)
        {
            decimal amountToTransfer = (decimal)amountToTransferD;

            var message = new ToSendMessageFromQueue { Id = messageId, Address = address, Amount = amountToTransfer };

            var fakeRpcClient = Substitute.For<IJsonRpcClient>();
            fakeRpcClient.SendToAddress(address, amountToTransfer).Returns("sometxid");

            var fakeTxesStorage = new NoSqlTableInMemory<ExistingCashOutEntity>();
            fakeTxesStorage.Insert(new ExistingCashOutEntity { PartitionKey = "part", RowKey = "id4" });
            fakeTxesStorage.Insert(new ExistingCashOutEntity { PartitionKey = "part", RowKey = "id5" });

            var fakeLogger = Substitute.For<ILog>();

            var queueTrigger = new CashOutQueueTrigger(fakeRpcClient, fakeTxesStorage, fakeLogger);


            //Act
            await queueTrigger.ReceiveMessage(message);


            //Assert
            await fakeRpcClient.Received(1).SendToAddress(address, amountToTransfer);
            Assert.That(fakeTxesStorage.Count(x => x.RowKey == messageId) == 1);
        }

        [TestCase("id1", 0.1, "jdljlkjd")]
        [TestCase("id2", 125.26, "asdfasdf")]
        [TestCase("id3", 10, "8ejqpoicv")]
        public async Task cashout_test_old_txes(string messageId, double amountToTransferD, string address)
        {
            decimal amountToTransfer = (decimal)amountToTransferD;

            var message = new ToSendMessageFromQueue { Id = messageId, Address = address, Amount = amountToTransfer };

            var fakeRpcClient = Substitute.For<IJsonRpcClient>();
            fakeRpcClient.SendToAddress(address, amountToTransfer).Returns("sometxid");

            var fakeTxesStorage = new NoSqlTableInMemory<ExistingCashOutEntity>();
            fakeTxesStorage.Insert(new ExistingCashOutEntity { PartitionKey = "part", RowKey = "id1" });
            fakeTxesStorage.Insert(new ExistingCashOutEntity { PartitionKey = "part", RowKey = "id2" });
            fakeTxesStorage.Insert(new ExistingCashOutEntity { PartitionKey = "part", RowKey = "id3" });
            fakeTxesStorage.Insert(new ExistingCashOutEntity { PartitionKey = "part", RowKey = "id4" });
            fakeTxesStorage.Insert(new ExistingCashOutEntity { PartitionKey = "part", RowKey = "id5" });


            var fakeLogger = Substitute.For<ILog>();

            var queueTrigger = new CashOutQueueTrigger(fakeRpcClient, fakeTxesStorage, fakeLogger);


            //Act
            await queueTrigger.ReceiveMessage(message);


            //Assert
            await fakeRpcClient.Received(0).SendToAddress(Arg.Any<string>(), Arg.Any<decimal>());
        }
    }
}
