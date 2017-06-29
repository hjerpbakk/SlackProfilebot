using System;

namespace Hjerpbakk.Profilebot.Configuration {
    /// <summary>
    ///     Configuration needed to use the Azure Cognitive Services.
    /// </summary>
    public struct FaceDetectionConfiguration {
        public FaceDetectionConfiguration(string key, string url, TimeSpan delay) {
            if (string.IsNullOrEmpty(key)) {
                throw new ArgumentException(nameof(key));
            }

            if (string.IsNullOrEmpty(url)) {
                throw new ArgumentException(nameof(url));
            }

            Key = key;
            URL = url;
            Delay = delay;
        }

        /// <summary>
        ///     The access key for the Azure Cognitive Services.
        /// </summary>
        public string Key { get; }

        /// <summary>
        ///     The URL to the Azure Cognitive Services API.
        /// </summary>
        public string URL { get; }

        /// <summary>
        ///     The time to wait if calls to the Azure Cognitive Services
        ///     are throttled.
        /// </summary>
        public TimeSpan Delay { get; }
    }
}