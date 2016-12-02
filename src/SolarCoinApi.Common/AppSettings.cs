using Newtonsoft.Json;
using SolarCoinApi.AzureStorage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace SolarCoinApi.Common
{
    public class AppSettings<T> where T : IValidatable
    {
        public static string GetString(string filePath)
        {
            return File.ReadAllText(filePath); ;
        }

        public static async Task<string> GetString(string fileName, string containerName, string connectionString)
        {
            return await new AzureBlobStorage(connectionString).GetAsTextAsync(containerName, fileName);
        }
        public T LoadFile(string filePath)
        {
            var txt = GetString(filePath);
            var fileNameWithExtension = Path.GetFileName(filePath);

            var settings = JsonConvert.DeserializeObject<T>(txt);

            settings.Validate();

            return settings;
        }

        public async Task<T> LoadBlob(string fileName, string containerName, string connectionString)
        {
            var txt = await GetString(fileName, containerName, connectionString);

            var cleanedTxt = Regex.Replace(txt, @"[^\u0009^\u000A^\u000D^\u0020-\u007E]", "");

            var settings = JsonConvert.DeserializeObject<T>(cleanedTxt);

            settings.Validate();

            return settings;
        }
    }

    public interface IValidatable
    {
        void Validate();
    }
}
