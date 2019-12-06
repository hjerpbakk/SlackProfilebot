using Hjerpbakk.Profilebot.Configuration;
using Microsoft.Extensions.Configuration;
using SlackConnector.Models;
using System;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace Hjerpbakk.Profilebot.Runner.Configuration
{
    public class Configuration
    {
        readonly IConfiguration currentConfig;

        public Configuration()
        {
            currentConfig = GetConfiguration();
        }

        public string SlackApiKey => currentConfig.GetValue<string>("apiToken");

        public SlackUser AdminUser => new SlackUser { Id = currentConfig.GetValue<string>("adminUserId") };

        public string ApplicationInsightsInstrumentationKey => currentConfig.GetValue<string>("applicationInsightsInstrumentationKey");

        public FaceDetectionConfiguration FaceDetectionConfiguration => new FaceDetectionConfiguration(currentConfig.GetValue<string>("faceAPIAccessKey"), currentConfig.GetValue<string>("faceAPIURL"), TimeSpan.FromMilliseconds(600D));

        public BlobStorageConfiguration BlobStorageConfiguration => new BlobStorageConfiguration(currentConfig.GetValue<string>("blobStorageAccountName"), currentConfig.GetValue<string>("blobStorageAccessKey"), currentConfig.GetValue<string>("endpointSuffix"));

        public bool ShouldStartHeartBeat => currentConfig.GetValue<bool>("heartBeat");

        static string AssemblyLocation()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var codebase = new Uri(assembly.CodeBase);
            var path = Path.GetDirectoryName(codebase.LocalPath);
            return path;
        }

        static bool EnvironmentVariablesExist()
        {
            string[] keys = {"apiToken", "adminUserId", "applicationInsightsInstrumentationKey", "faceAPIAccessKey", "faceAPIURL",
                             "blobStorageAccountName", "blobStorageAccessKey", "endpointSuffix", "heartBeat"};

            foreach (string k in keys)
            {
                if (String.IsNullOrEmpty(Environment.GetEnvironmentVariable(k)))
                    return false;
            }
            return true;
        }

        private IConfiguration GetConfiguration()
        {
            var assemblyLocation = AssemblyLocation();
            var fileName = Path.Combine(assemblyLocation, @"Configuration\config.json");

            if (!File.Exists(fileName))
            {
                Console.WriteLine("No config.json found, searching environment variables.");
                if (!EnvironmentVariablesExist())
                    throw new ConfigurationErrorsException("No configuration found. Either copy config.default.json, rename it to config.json and specify settings, or use environment variables");
                else
                    Console.WriteLine("Valid environment variables discovered!");
            }

            return new ConfigurationBuilder()
                .AddJsonFile(fileName, optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }
    }

}
