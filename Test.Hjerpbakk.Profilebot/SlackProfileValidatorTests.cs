using System;
using Hjerpbakk.ProfileBot;
using Hjerpbakk.ProfileBot.Contracts;
using SlackConnector.Models;
using Xunit;

namespace Test.Hjerpbakk.Profilebot {
    public class SlackProfileValidatorTests {
        [Fact]
        public void VerifyProfile_HasNoFirstName_Invalid() {
            var slackProfileValidator = new SlackProfileValidator();

            var validationResult = slackProfileValidator.ValidateProfile(new SlackUser {Id = "UserId"});

            Assert.False(validationResult.IsValid);
            Assert.Equal("UserId", validationResult.UserId);
            Assert.Equal("First name is missing", validationResult.Errors);
        }

        [Fact]
        public void VerifyProfile_HasFirstName_Valid() {
            var slackProfileValidator = new SlackProfileValidator();

            var validationResult = slackProfileValidator.ValidateProfile(new SlackUser {Id = "UserId", FirstName = "FirstName"});

            Assert.True(validationResult.IsValid);
            Assert.Equal("", validationResult.UserId);
            Assert.Equal("", validationResult.Errors);
        }
    }
}