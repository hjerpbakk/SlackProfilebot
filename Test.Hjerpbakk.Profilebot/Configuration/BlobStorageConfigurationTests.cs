using System;
using Hjerpbakk.Profilebot.Configuration;
using Xunit;

namespace Test.Hjerpbakk.Profilebot.Configuration {
    public class BlobStorageConfigurationTests {
        [Fact]
        public void EmptyParameters() {
            var exception = Record.Exception(() => new BlobStorageConfiguration(null, "key", "suffix"));

            Assert.IsType<ArgumentException>(exception);

            var exception1 = Record.Exception(() => new BlobStorageConfiguration("account", null, "suffix"));

            Assert.IsType<ArgumentException>(exception1);

            var exception2 = Record.Exception(() => new BlobStorageConfiguration("account", "key", null));

            Assert.IsType<ArgumentException>(exception2);
        }

        [Fact]
        public void LocalConnectionString() {
            Assert.Equal("UseDevelopmentStorage=true;", BlobStorageConfiguration.Local.ConnectionString);
        }

        [Fact]
        public void RemoteConnectionString() {
            const string BlobStorageAccount = "account";
            const string BlobStorageKey = "key";
            const string EndpointSuffix = "suffix";

            var connectionString = new BlobStorageConfiguration(BlobStorageAccount, BlobStorageKey, EndpointSuffix).ConnectionString;

            Assert.Equal("DefaultEndpointsProtocol=https;AccountName=account;AccountKey=key;EndpointSuffix=suffix", connectionString);
        }
    }
}