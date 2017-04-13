using NUnit.Framework;
using SolarCoinApi.CashInHandlerJobRunner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Newtonsoft.Json;
using System.Reflection;
using AzureStorage.Queue;
using AzureStorage.Tables;
using Common.Log;
using SolarCoinApi.Core;
using SolarCoinApi.RpcJson.JsonRpc;

namespace SolarCoinApi.Tests.CashInHandlerJobRunner
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public async Task cashin_handler_test()
        {
            long s = 100000000; // accedpted vout amounts are in satoshis, should conver to solar coins, 1 slr = 10^8 satoshis

            //Arrange

            var hotWalletAddress = Guid.NewGuid().ToString();
            var txFee = 0.01m;
            var minTxAmount = 0.1m;


            //Arranging generated wallets storage
            var generatedWallets = new NoSqlTableInMemory<WalletStorageEntity>();
            generatedWallets.Insert(new WalletStorageEntity { PartitionKey = "part", RowKey = Guid.NewGuid().ToString(), Address = "8first_generated_address", PrivateKey = "first_priv_key" });
            generatedWallets.Insert(new WalletStorageEntity { PartitionKey = "part", RowKey = Guid.NewGuid().ToString(), Address = "8second_generated_address", PrivateKey = "second_priv_key" });
            generatedWallets.Insert(new WalletStorageEntity { PartitionKey = "part", RowKey = Guid.NewGuid().ToString(), Address = "8third_generated_address", PrivateKey = "third_priv_key" });
            generatedWallets.Insert(new WalletStorageEntity { PartitionKey = "part", RowKey = Guid.NewGuid().ToString(), Address = "8fourth_generated_address", PrivateKey = "fourth_priv_key" });

            var fakeTxesQueue = Substitute.For<IQueueExt>();

            var fakeRpcClient = Substitute.For<IJsonRpcClient>();
            fakeRpcClient.CreateRawTransaction(Arg.Any<object[]>(), Arg.Any<Dictionary<string, decimal>>()).Returns("string");
            fakeRpcClient.SignRawTransaction(Arg.Any<string>(), Arg.Any<string>()).Returns(new SignRawTransactionResponseModel());
            fakeRpcClient.SendRawTransaction(Arg.Any<string>()).Returns("string");

            var fakeSlackNotifier = Substitute.For<ISlackNotifier>();

            var fakeLogger = Substitute.For<ILog>();


            var queueTrigger = new CashInHandlerQueueTrigger("SolarCoinApi.Test.CashInHandlerJobRunner", generatedWallets, fakeLogger, fakeTxesQueue, fakeRpcClient, fakeSlackNotifier, hotWalletAddress, txFee, minTxAmount);

            

            string txId = Guid.NewGuid().ToString();
            var message = new TransitQueueMessage
            {
                TxId = txId,
                Vouts = new List<Vout>
                {
                    new Vout { Address = "8third_generated_address", Amount = (long)(2.3652m*s)},
                    new Vout { Address = "some_unrelated_address_1", Amount = (long)(5m*s)}, // should be ignored
                    new Vout { Address = "some_unrelated_address_2", Amount = (long)(6.365m*s)}, // should be ignored
                    new Vout { Address = "8third_generated_address", Amount = (long)(2.146m*s)},
                    new Vout { Address = "some_unrelated_address_3", Amount = (long)(9.213m*s)}, // should be ignored
                    new Vout { Address = "8first_generated_address", Amount = (long)(0.09m*s)}, // this shouldn't be put to queue because it's less than min tx amount
                    new Vout { Address = "8fourth_generated_address", Amount = (long)(100.2568m*s)}
                }
            };



            // Act
            await queueTrigger.ReceiveMessage(message);

            // Assert

            // Check if all relevant outputs where put in the queue correctly
            await fakeTxesQueue.Received(1).PutRawMessageAsync(Arg.Is<string>(x => JsonConvert.DeserializeObject<QueueModel>(x).Equals(new QueueModel { Address = "8third_generated_address", TxId = txId, Amount = 2.3652m + 2.146m })));
            await fakeTxesQueue.Received(1).PutRawMessageAsync(Arg.Is<string>(x => JsonConvert.DeserializeObject<QueueModel>(x).Equals(new QueueModel { Address = "8fourth_generated_address", TxId = txId, Amount = 100.2568m })));
            await fakeTxesQueue.Received(2).PutRawMessageAsync(Arg.Any<string>());

            // Check if all relevant outputs where transferred to the hot wallet
            await fakeRpcClient.Received(1).CreateRawTransaction(
                Arg.Is<object[]>(
                    x => x.Count() == 1 &&
                    x.All(y => (string)(y.GetType().GetProperty("txid").GetValue(y)) == txId) &&
                    x.Any(y => (int)(y.GetType().GetProperty("vout").GetValue(y)) == 6)),
                Arg.Is<Dictionary<string, decimal>>(x => x.Count == 1 && x[hotWalletAddress] == 100.2568m - txFee));

            await fakeRpcClient.Received(1).CreateRawTransaction(
                Arg.Is<object[]>(x => x.Count() == 2 &&
                    x.All(y => (string)(y.GetType().GetProperty("txid").GetValue(y)) == txId) &&
                    x.Any(y => (int)(y.GetType().GetProperty("vout").GetValue(y)) == 0) &&
                    x.Any(y => (int)(y.GetType().GetProperty("vout").GetValue(y)) == 3)),
                Arg.Is<Dictionary<string, decimal>>(x => x.Count == 1 && x[hotWalletAddress] == 2.3652m + 2.146m - txFee));

            await fakeRpcClient.Received(2).CreateRawTransaction(Arg.Any<object[]>(), Arg.Any<Dictionary<string, decimal>>());

            await fakeRpcClient.Received(1).SignRawTransaction(Arg.Any<string>(), "third_priv_key");
            await fakeRpcClient.Received(1).SignRawTransaction(Arg.Any<string>(), "fourth_priv_key");
            await fakeRpcClient.Received(2).SignRawTransaction(Arg.Any<string>(), Arg.Any<string>());

            await fakeRpcClient.Received(2).SendRawTransaction(Arg.Any<string>());

        }
    }
}
