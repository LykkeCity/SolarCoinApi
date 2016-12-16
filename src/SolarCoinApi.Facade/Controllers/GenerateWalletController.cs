using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SolarCoinApi.AzureStorage;
using SolarCoinApi.AzureStorage.Tables;
using SolarCoinApi.Core;
using SolarCoinApi.Core.Log;
using SolarCoinApi.Core.Options;

namespace SolarCoinApi.Facade.Controllers
{
    [Route("api/GenerateWallet")]
    public class GenerateWalletController : Controller
    {
        private readonly IWalletGenerator _walletGenerator;

        private readonly ILog _logger;
        private readonly ISlackNotifier _slackNotifier;
        private readonly IOptions<WalletGeneratorControllerOptions> _options;

        public GenerateWalletController(IWalletGenerator walletGenerator, ILog logger, ISlackNotifier slackNotifier, IOptions<WalletGeneratorControllerOptions> options)
        {
            _walletGenerator = walletGenerator;
            _slackNotifier = slackNotifier;
            _options = options;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Get()
        {
            try
            {
                var generatedWallet = await _walletGenerator.GenerateWalletAsync();

                if (string.IsNullOrEmpty(generatedWallet.Address) || string.IsNullOrEmpty(generatedWallet.PrivateKey) ||
                    string.IsNullOrEmpty(generatedWallet.PublicKey))
                    throw new Exception("Response lacked one of the fields");

                var storage = new AzureTableStorage<WalletStorageEntity>(_options.Value.ConnectionString, _options.Value.TableName, _logger);

                await storage.InsertAsync(new WalletStorageEntity
                {
                    PartitionKey = "part",
                    RowKey = generatedWallet.Address,
                    Address = generatedWallet.Address,
                    PublicKey = generatedWallet.PublicKey,
                    PrivateKey = generatedWallet.PrivateKey
                });

                return Json(new GenerateWalletControllerResponseModel { Address = generatedWallet.Address });
            }
            catch (Exception e)
            {
                await _slackNotifier.Notify(new SlackMessage { Sender = "SolarCoinApi.Facade", Type = "Errors", Message = "Error occured during SolarCoin address generation." });
                await _logger.WriteError("Wallet generator", "", "", e);
                int i = 0;
                while (e.InnerException != null)
                {
                    e = e.InnerException;
                    await _logger.WriteError("Wallet generator", "", $"InnerException-lvl-{i}", e);
                    i++;
                }
            }

            return new StatusCodeResult(500);
        }
    }
    public class GenerateWalletControllerResponseModel
    {
        public string Address { set; get; }
    }
}
