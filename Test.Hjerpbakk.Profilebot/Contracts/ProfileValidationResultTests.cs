using System;
using Hjerpbakk.Profilebot.Contracts;
using SlackConnector.Models;
using Xunit;

namespace Test.Hjerpbakk.Profilebot.Contracts {
    public class ProfileValidationResultTests {
        [Fact]
        public void Constructor() {
            var user = new SlackUser {Id = "id"};
            var url = new Uri("http://image.com");

            var profileValidationResult = new ProfileValidationResult(user, "errors", url);

            Assert.False(profileValidationResult.IsValid);
            Assert.Same(user, profileValidationResult.User);
            Assert.Equal("errors", profileValidationResult.Errors);
            Assert.Same(url, profileValidationResult.ImageURL);
        }

        [Fact]
        public void Constructor_NoErrors_Fails() {
            var exception = Record.Exception(() => new ProfileValidationResult(new SlackUser {Id = "id"}, ""));

            Assert.IsType<ArgumentException>(exception);
        }

        [Fact]
        public void Constructor_Default() {
            var profileValidationResult = new ProfileValidationResult();

            Assert.False(profileValidationResult.IsValid);
            Assert.Null(profileValidationResult.User);
            Assert.Null(profileValidationResult.Errors);
            Assert.Null(profileValidationResult.ImageURL);
        }

        [Fact]
        public void Valid() {
            var user = new SlackUser {Id = "id"};

            var profileValidationResult = ProfileValidationResult.Valid(user);

            Assert.True(profileValidationResult.IsValid);
            Assert.Same(user, profileValidationResult.User);
            Assert.Equal("", profileValidationResult.Errors);
            Assert.Null(profileValidationResult.ImageURL);
        }
    }
}