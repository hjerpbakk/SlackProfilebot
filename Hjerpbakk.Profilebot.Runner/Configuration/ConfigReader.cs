using System;
using System.IO;
using System.Reflection;
using Hjerpbakk.Profilebot.Configuration;
using Newtonsoft.Json.Linq;
using SlackConnector.Models;

namespace Hjerpbakk.Profilebot.Runner.Configuration {
    public class ConfigReader {
        readonly Lazy<JObject> currentJObject;

        public ConfigReader() {
            currentJObject = new Lazy<JObject>(GetJObject);
        }

        public string SlackApiKey => currentJObject.Value.Value<string>("apiToken");

        public SlackUser AdminUser => new SlackUser {Id = currentJObject.Value.Value<string>("adminUserId")};

        public string ApplicationInsightsInstrumentationKey => currentJObject.Value.Value<string>("applicationInsightsInstrumentationKey");

        public FaceDetectionConfiguration FaceDetectionConfiguration => new FaceDetectionConfiguration(currentJObject.Value.Value<string>("faceAPIAccessKey"), currentJObject.Value.Value<string>("faceAPIURL"), TimeSpan.FromMilliseconds(600D));

        public BlobStorageConfiguration BlobStorageConfiguration => new BlobStorageConfiguration(currentJObject.Value.Value<string>("blobStorageAccountName"), currentJObject.Value.Value<string>("blobStorageAccessKey"), currentJObject.Value.Value<string>("endpointSuffix"));

        public bool ShouldStartHeartBeat => currentJObject.Value.Value<bool>("heartBeat");

        static JObject GetJObject() {
            var assemblyLocation = AssemblyLocation();
            var fileName = Path.Combine(assemblyLocation, @"Configuration\config.json");
            if (!File.Exists(fileName)) {
                throw new FileNotFoundException("Copy config.default.json, paste and rename to config.json and fill in the required values.");
            }

            var json = File.ReadAllText(fileName);
            return JObject.Parse(json);
        }

        static string AssemblyLocation() {
            var assembly = Assembly.GetExecutingAssembly();
            var codebase = new Uri(assembly.CodeBase);
            var path = Path.GetDirectoryName(codebase.LocalPath);
            return path;
        }
    }
}