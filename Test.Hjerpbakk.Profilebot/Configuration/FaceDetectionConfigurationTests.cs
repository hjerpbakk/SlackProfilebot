using System;
using Hjerpbakk.Profilebot.Configuration;
using Xunit;

namespace Test.Hjerpbakk.Profilebot.Configuration {
    public class FaceDetectionConfigurationTests {
        [Fact]
        public void CTOR() {
            var configuration = new FaceDetectionConfiguration("key", "url", TimeSpan.FromMilliseconds(1D));

            Assert.Equal("key", configuration.Key);
            Assert.Equal("url", configuration.URL);
            Assert.Equal(TimeSpan.FromMilliseconds(1D), configuration.Delay);
        }

        [Fact]
        public void EmptyKey_Fails() {
            var exception = Record.Exception(() => new FaceDetectionConfiguration(null, "url", TimeSpan.Zero));

            Assert.IsType<ArgumentException>(exception);
        }

        [Fact]
        public void EmptyUrl_Fails() {
            var exception = Record.Exception(() => new FaceDetectionConfiguration("key", null, TimeSpan.Zero));

            Assert.IsType<ArgumentException>(exception);
        }
    }
}