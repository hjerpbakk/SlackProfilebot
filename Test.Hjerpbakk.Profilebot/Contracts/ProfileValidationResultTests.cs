using Hjerpbakk.ProfileBot.Contracts;
using Xunit;

namespace Test.Hjerpbakk.Profilebot.Contracts {
    public class ProfileValidationResultTests {
        [Fact]
        public void Constructor() {
            var profileValidationResult = new ProfileValidationResult(true, "userid", "errors");

            Assert.True(profileValidationResult.IsValid);
            Assert.Equal("userid", profileValidationResult.UserId);
            Assert.Equal("errors", profileValidationResult.Errors);
        }

        [Fact]
        public void Constructor_Default() {
            var profileValidationResult = new ProfileValidationResult();

            Assert.False(profileValidationResult.IsValid);
            Assert.Null(profileValidationResult.UserId);
            Assert.Null(profileValidationResult.Errors);
        }

        [Fact]
        public void Valid() {
            var profileValidationResult = ProfileValidationResult.CreateValid();

            Assert.True(profileValidationResult.IsValid);
            Assert.Equal("", profileValidationResult.UserId);
            Assert.Equal("", profileValidationResult.Errors);
        }
    }
}