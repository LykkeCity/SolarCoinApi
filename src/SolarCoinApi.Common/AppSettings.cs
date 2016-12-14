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
using System.Reflection;
using System.ComponentModel;

namespace SolarCoinApi.Common
{
    public class AppSettings<T> where T : IValidatable, new()
    {
        public static string GetString(string filePath)
        {
            return File.ReadAllText(filePath);
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
                
        public T LoadFromEnvironment()
        {
            var r = new T();

            ReadPropertiesRecursive(r, typeof(T), new List<string>());

            return r;
        }
        

        private static void ReadPropertiesRecursive(object obj, Type type, List<string> prefixes)
        {

            foreach (PropertyInfo property in type.GetProperties())
            {
                if (property.PropertyType.GetTypeInfo().IsClass && property.PropertyType != typeof(string))
                {
                    prefixes.Add(property.Name);
                    var val = property.GetValue(obj);
                    if (val == null)
                        val = Activator.CreateInstance(property.PropertyType);
                    property.SetValue(obj, val);
                    ReadPropertiesRecursive(val, property.PropertyType, prefixes);
                    prefixes.Remove(property.Name);
                }
                else
                {
                    var propertyFullName = prefixes != null && prefixes.Count > 0 ? $"{prefixes.Aggregate((i, j) => i + "." + j)}.{property.Name}" : property.Name;
                    
                    Console.WriteLine(propertyFullName);

                    var val = Environment.GetEnvironmentVariable(propertyFullName);
                    if (val == null)
                        throw new ArgumentException($"'{propertyFullName}' was not found among environment variables");

                    property.SetValue(obj, Convert.ChangeType(Environment.GetEnvironmentVariable(propertyFullName), property.PropertyType));
                }
            }
        }

            
    }


    public class Aa
    {
        public Aa()
        {
            //Bb = new Bb();
        }
        public Bb Bb { set; get; }
        public string Dd { set; get; }
    }


    public class Bb
    {
        public string bb { set; get; }
        public decimal cc { set; get; }
        public Ee EeName { set;get;}
    }

    public class Ee
    {
        public int ee { set; get; }
    }

    public interface IValidatable
    {
        void Validate();
    }

    public static class StringHelpers
    {
        public static T Convert<T>(this string input)
        {
            try
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter != null)
                {
                    // Cast ConvertFromString(string text) : object to (T)
                    return (T)converter.ConvertFromString(input);
                }
                return default(T);
            }
            catch (NotSupportedException)
            {
                return default(T);
            }
        }
    }
}
