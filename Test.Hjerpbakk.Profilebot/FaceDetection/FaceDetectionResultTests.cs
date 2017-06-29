using System;
using Hjerpbakk.Profilebot.FaceDetection;
using Xunit;

namespace Test.Hjerpbakk.Profilebot.FaceDetection {
    public class FaceDetectionResultTests {
        [Fact]
        public void CTOR() {
            var exception = Record.Exception(() => new FaceDetectionResult(""));

            Assert.IsType<ArgumentException>(exception);
        }

        [Fact]
        public void Equals() {
            var first = FaceDetectionResult.Valid;
            var other = FaceDetectionResult.Valid;

            Assert.Equal(first, other);
            Assert.True(first.Equals((object) other));
            Assert.False(first.Equals(null));
            Assert.False(first.Equals(new object()));

            other = new FaceDetectionResult("errors");

            Assert.NotEqual(first, other);
        }

        [Fact]
        public void GetHashCodeTest() {
            Assert.NotEqual(new FaceDetectionResult().GetHashCode(), FaceDetectionResult.Valid.GetHashCode());
        }

        [Fact]
        public void Invalid() {
            var result = new FaceDetectionResult("errors");

            Assert.False(result.IsValid);
            Assert.Equal("errors", result.Errors);
        }

        [Fact]
        public void Valid() {
            var result = FaceDetectionResult.Valid;

            Assert.True(result.IsValid);
            Assert.Equal("", result.Errors);
        }
    }
}