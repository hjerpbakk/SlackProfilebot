using System;
using System.IO;
using System.Reflection;
using Hjerpbakk.Profilebot.Configuration;
using Newtonsoft.Json.Linq;

namespace Hjerpbakk.ProfileBot.Runner.Configuration {
    public class ConfigReader {
        readonly Lazy<JObject> currentJObject;

        public ConfigReader() {
            currentJObject = new Lazy<JObject>(GetJObject);
        }

        public string SlackApiKey => currentJObject.Value.Value<string>("apiToken");

        public AdminUser AdminUser => new AdminUser(currentJObject.Value.Value<string>("adminUserId"));

        public string ApplicationInsightsInstrumentationKey => currentJObject.Value.Value<string>("applicationInsightsInstrumentationKey");

        public FaceDetectionAPI FaceAPI => new FaceDetectionAPI(currentJObject.Value.Value<string>("faceAPIAccessKey"));

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