using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Text;
using Common.Log;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SolarCoinApi.Core.Options;
using SolarCoinApi.RpcJson.JsonRpc;

namespace SolarCoinApi.RpcJson.JsonRpc
{
    public interface IJsonRpcClientRaw
    {
        Task<string> Invoke(string method, params object[] args);
    }

    public interface IJsonRpcRequestBuilder
    {
        string BuildJson(string method, params object[] args);
    }

    public class JsonRpcRequestBuilder : IJsonRpcRequestBuilder
    {
        public string BuildJson(string method, params object[] args)
        {
            if (args.Length == 2 && args[0] is object[] && args[1] is Dictionary<string, decimal>)
            {
                var serrializedArr = JsonConvert.SerializeObject(args[0]);

                var dic = (Dictionary<string, decimal>)args[1];
                var serrializedDic = new StringBuilder("{");
                int i = 0;
                foreach (KeyValuePair<string, decimal> entry in dic)
                {
                    if (i != 0)
                        serrializedDic.Append(",");
                    serrializedDic.Append($"\"{entry.Key}\":{entry.Value}");
                    i++;
                }
                serrializedDic.Append("}");

                var j = new
                {
                    id = "id",
                    method = method,
                    @params = new object[] { "$#%" }
                };

                var serrialized = JsonConvert.SerializeObject(j);

                serrialized = serrialized.Replace("\"$#%\"", $"{serrializedArr}, {serrializedDic}");

                return serrialized;
            }

            var json = new
            {
                id = "id",
                method = method,
                @params = args
            };

            return JsonConvert.SerializeObject(json);
        }
    }

    public class JsonRpcClientRaw : IJsonRpcClientRaw
    {
        private IJsonRpcRequestBuilder _requestBuilder;
        private IOptions<RpcWalletGeneratorOptions> _options;
        private ILog _logger;
        private HttpClient _httpClient;

        public JsonRpcClientRaw(IJsonRpcRequestBuilder requestBuilder, ILog logger, IOptions<RpcWalletGeneratorOptions> options)
        {
            _requestBuilder = requestBuilder;
            _options = options;
            _logger = logger;

            var credentials = new NetworkCredential(_options.Value.Username, _options.Value.Password);
            var handler = new HttpClientHandler { Credentials = credentials };

            _httpClient = new HttpClient(handler);
        }
        public async Task<string> Invoke(string method, params object[] args)
        {

            var req = _requestBuilder.BuildJson(method, args);

            //await _logger.WriteInfoAsync("JsonRpcClientRaw", "Posting to RPC", "", $"{req}");
            await _logger.WriteInfoAsync("JsonRpcClientRaw", "", method, "Posting to RPC");


            using (
                HttpResponseMessage response =
                    await
                        _httpClient.PostAsync(_options.Value.Endpoint,
                            new StringContent(req, Encoding.UTF8,
                                "application/json"))
            )
            using (HttpContent content = response.Content)
            {
                if (response.StatusCode != HttpStatusCode.OK &&
                    response.StatusCode != HttpStatusCode.InternalServerError)
                {
                    Console.Write(response.StatusCode);
                    throw new Exception(
                        $"Request to RPC on '{_options.Value.Endpoint}' with username '{_options.Value.Username}' and password '{_options.Value.Password}' ended up with Status '{response.StatusCode}'");

                }

                string result = await content.ReadAsStringAsync();

                //Console.WriteLine("RESULT::: " + result);
                if (!string.IsNullOrEmpty(result))
                {
                    return result;
                }
            }

            return null;

        }
    }

    public interface IJsonRpcClient
    {
        Task<MakeKeyPairResponseModel> MakeKeyPair(string compress = "true");
        Task<string> CreateRawTransaction(object[] inputs, Dictionary<string, decimal> to);
        Task<SignRawTransactionResponseModel> SignRawTransaction(string hex, string privateKey);
        Task<string> SendRawTransaction(string hex);
        Task<string> SendToAddress(string address, decimal amount);
        Task<ImportPrivateKeyResponseModel> ImportPrivateKey(string privkey);
        Task<int> GetBlockCount();
        Task<List<ListTransactionsItemResponseModel>> ListTransactions(int count);
        Task<string> GetRawTransaction(string hex);

    }

    public class JsonRpcClient : IJsonRpcClient
    {

        private readonly IJsonRpcClientRaw _rawClient;
        private readonly IJsonRpcRawResponseFormatter _responseFormatter;
        private readonly ILog _logger;
        public JsonRpcClient(IJsonRpcClientRaw rawClient, IJsonRpcRawResponseFormatter responseFormatter, ILog logger)
        {
            _rawClient = rawClient;
            _responseFormatter = responseFormatter;
            _logger = logger;
        }

        public Task<MakeKeyPairResponseModel> MakeKeyPair(string compress = "true")
        {
            return this.Invoke<MakeKeyPairResponseModel>("makekeypair", compress);

        }

        public Task<string> CreateRawTransaction(object[] inputs, Dictionary<string, decimal> to)
        {
            return this.Invoke<string>("createrawtransaction", inputs, to);
        }

        public Task<int> GetBlockCount()
        {
            return this.Invoke<int>("getblockcount");
        }

        public Task<SignRawTransactionResponseModel> SignRawTransaction(string hex, string privateKey)
        {
            return this.Invoke<SignRawTransactionResponseModel>("signrawtransaction", hex, null, new object[] { privateKey });
        }

        public Task<string> SendRawTransaction(string hex)
        {
            return this.Invoke<string>("sendrawtransaction", hex);
        }

        public Task<string> SendToAddress(string address, decimal amount)
        {
            return this.Invoke<string>("sendtoaddress", address, amount);
        }

        public Task<ImportPrivateKeyResponseModel> ImportPrivateKey(string privkey)
        {
            return this.Invoke<ImportPrivateKeyResponseModel>("importprivkey", privkey);
        }
        public Task<List<ListTransactionsItemResponseModel>> ListTransactions(int count)
        {
            return this.Invoke<List<ListTransactionsItemResponseModel>>("listtransactions", "", count, 0);
        }
        public Task<string> GetRawTransaction(string hex)
        {
            return this.Invoke<string>("getrawtransaction", hex);
        }

        private async Task<T> Invoke<T>(string method, params object[] args)
        {
            try
            {
                var response = await _rawClient.Invoke(method, args);
                var result = _responseFormatter.Format<T>(response);

                if (result.Error != null)
                    throw new Exception(result.Error.Message);

                return result.Result;
            }
            catch (Exception e)
            {
                var a = 234;
                throw;
            }
        }
    }

    public interface IJsonRpcRawResponseFormatter
    {
        JsonRpcResponseModel<T> Format<T>(string resp);
    }

    public class JsonRpcRawResponseFormatter : IJsonRpcRawResponseFormatter
    {
        public JsonRpcResponseModel<T> Format<T>(string resp)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<JsonRpcResponseModel<T>>(resp);
        }
    }

    public class JsonRpcResponseModel
    {
        public string Id { set; get; }
        public JsonResponseModelError Error { set; get; }
    }

    public class JsonRpcResponseModel<T> : JsonRpcResponseModel
    {
        public T Result { set; get; }
    }

    public abstract class JsonResponseModelResult { }

    public class JsonResponseModelError
    {
        public long Code { set; get; }
        public string Message { set; get; }
    }

    public class MakeKeyPairResponseModel
    {
        public string PublicKey { set; get; }
        public string PrivateKey { set; get; }
        public string Address { set; get; }
        public string PrivKey { set; get; }
    }

    public class CreateRawTransactionResponseModel
    {
        public string Hex { set; get; }
    }

    public class SignRawTransactionResponseModel
    {
        public string Hex { set; get; }
        public bool Complete { set; get; }
    }

    public class SendToAddressResponseModel
    {
        public string TxId { set; get; }
    }

    public class ImportPrivateKeyResponseModel
    {

    }

    public class ListTransactionsItemResponseModel
    {
        public string Account { set; get; }
        public string Address { set; get; }
        public string Category { set; get; }
        public string Fee { set; get; }
        public string Amount { set; get; }
        [JsonProperty("tx-comment")]
        public string TxComment { set; get; }
        public string Confirmations { set; get; }
        private string Generated { set; get; }
        public string BlockHash { set; get; }
        public string BlockIndex { set; get; }
        public string BlockTime { set; get; }
        public string TxId { set; get; }
        public string Time { set; get; }
        public string TimeReceived { set; get; }
    }
}
