using System;

namespace Hjerpbakk.Profilebot.Configuration {
    /// <summary>
    ///     Configuration needed to use Azure Blob Storage.
    /// </summary>
    public struct BlobStorageConfiguration {
        public BlobStorageConfiguration(string blobStorageAccountName, string blobStorageKey, string endpointSuffix) {
            if (string.IsNullOrEmpty(blobStorageAccountName)) {
                throw new ArgumentException(nameof(blobStorageAccountName));
            }

            if (string.IsNullOrEmpty(blobStorageKey)) {
                throw new ArgumentException(nameof(blobStorageKey));
            }

            if (string.IsNullOrEmpty(endpointSuffix)) {
                throw new ArgumentException(nameof(endpointSuffix));
            }

            ConnectionString = $"DefaultEndpointsProtocol=https;AccountName={blobStorageAccountName};AccountKey={blobStorageKey};EndpointSuffix={endpointSuffix}";
        }

        BlobStorageConfiguration(string connectionString) {
            ConnectionString = connectionString;
        }

        /// <summary>
        ///     Gets the connection string needed for the local Blob Storage simulator.
        /// </summary>
        public static BlobStorageConfiguration Local => new BlobStorageConfiguration("UseDevelopmentStorage=true;");

        /// <summary>
        ///     Gets the connection string.
        /// </summary>
        public string ConnectionString { get; }
    }
}