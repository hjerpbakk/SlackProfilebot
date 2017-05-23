using System;
using Hjerpbakk.ProfileBot;
using Hjerpbakk.ProfileBot.Contracts;
using SlackConnector.Models;
using Xunit;

namespace Test.Hjerpbakk.Profilebot {
    public class SlackProfileValidatorTests {
        const string UserId = "SLACKUSERID";

        readonly ISlackProfileValidator slackProfileValidator;

        public SlackProfileValidatorTests() {
            slackProfileValidator = new SlackProfileValidator(new AdminUser("U1TBU8336"));
        }

        [Fact]
        public void VerifyProfile_SlackUserIsNull_Throws() {
            var exception = Record.Exception(() => slackProfileValidator.ValidateProfile(null));

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void VerifyProfile_AdminIdIsNull_Throws() {
            var exception = Record.Exception(() => new SlackProfileValidator(new AdminUser()));

            Assert.IsType<ArgumentException>(exception);
        }

        [Fact]
        public void VerifyProfile_HasNoId_Throws() {
            var user = new SlackUser();

            var exception = Record.Exception(() => slackProfileValidator.ValidateProfile(user));

            Assert.IsType<ArgumentException>(exception);
            Assert.Contains("Id cannot be empty.", exception.Message);
        }

        [Fact]
        public void VerifyProfile_HasNoName_Throws() {
            var user = new SlackUser {Id = UserId};

            var exception = Record.Exception(() => slackProfileValidator.ValidateProfile(user));

            Assert.IsType<ArgumentException>(exception);
            Assert.Contains("Name cannot be empty.", exception.Message);
        }

        [Fact]
        public void VerifyProfile_HasNoMail_Invalid() {
            var user = CreateUser();

            var validationResult = slackProfileValidator.ValidateProfile(user);

            VerifyValidationResult(validationResult);
            VerifyEmailExist(validationResult);
        }

        [Fact]
        public void VerifyProfile_HasOtherThanDIPSMail_Invalid() {
            var user = CreateUser();
            user.Email = "runar@hjerpbakk.com";

            var validationResult = slackProfileValidator.ValidateProfile(user);

            VerifyValidationResult(validationResult);
            VerifyOtherThanDIPSMail(validationResult);
        }

        [Fact]
        public void VerifyProfile_HasWrongDIPSMail_Invalid() {
            var user = CreateUser();
            user.Email = "runar@dips.no";

            var validationResult = slackProfileValidator.ValidateProfile(user);

            VerifyValidationResult(validationResult);
            VerifyWrongDIPSMail(validationResult);
        }

        [Fact]
        public void VerifyProfile_HasMissingFirstName_Invalid() {
            var user = CreateUser();
            user.Email = "roh@dips.no";

            var validationResult = slackProfileValidator.ValidateProfile(user);

            VerifyValidationResult(validationResult);
            VerifyMissingFirstName(validationResult);
        }

        [Fact]
        public void VerifyProfile_HasMissingLastName_Invalid() {
            var user = CreateUser();
            user.Email = "roh@dips.no";
            user.FirstName = "Runar";

            var validationResult = slackProfileValidator.ValidateProfile(user);

            VerifyValidationResult(validationResult);
            VerifyMissingLastName(validationResult);
        }

        [Fact]
        public void VerifyProfile_HasMissingWhatIDo_Invalid() {
            var user = CreateUser();
            user.Email = "roh@dips.no";
            user.FirstName = "Runar";
            user.LastName = "Hjerpbakk";

            var validationResult = slackProfileValidator.ValidateProfile(user);

            VerifyValidationResult(validationResult);
            VerifyMissingWhatIDo(validationResult);
        }

        [Fact]
        public void VerifyProfile_HasMissingImage_Invalid() {
            var user = CreateUser();
            user.Email = "roh@dips.no";
            user.FirstName = "Runar";
            user.LastName = "Hjerpbakk";
            user.WhatIDo = "Software Engineering Manager";

            var validationResult = slackProfileValidator.ValidateProfile(user);

            VerifyValidationResult(validationResult);
            VerifyMissingImage(validationResult);
        }

        [Fact]
        public void VerifyProfile_CompleteProfile_Valid() {
            var user = CreateUser();
            user.Email = "roh@dips.no";
            user.FirstName = "Runar";
            user.LastName = "Hjerpbakk";
            user.WhatIDo = "Software Engineering Manager";
            user.Image = "http://image.com";

            var validationResult = slackProfileValidator.ValidateProfile(user);

            Assert.Equal(true, validationResult.IsValid);
            Assert.Null(validationResult.UserId);
            Assert.Null(validationResult.Errors);
        }

        [Fact]
        public void VerifyProfile_EverythingMissing_InvalidAndAllErrorsReturned() {
            var user = CreateUser();

            var validationResult = slackProfileValidator.ValidateProfile(user);

            VerifyValidationResult(validationResult);
            VerifyEmailExist(validationResult);
            VerifyMissingFirstName(validationResult);
            VerifyMissingLastName(validationResult);
            VerifyMissingWhatIDo(validationResult);
            VerifyMissingImage(validationResult);
        }

        static SlackUser CreateUser() =>
            new SlackUser {Id = UserId, Name = "roh"};

        static void VerifyValidationResult(ProfileValidationResult validationResult) {
            Assert.Equal(false, validationResult.IsValid);
            Assert.Equal(UserId, validationResult.UserId);
        }

        static void VerifyEmailExist(ProfileValidationResult validationResult) =>
            Assert.Contains("Din DIPS-epost må være registrert på brukeren din.", validationResult.Errors);

        static void VerifyOtherThanDIPSMail(ProfileValidationResult validationResult) =>
            Assert.Contains("Kun DIPS-epost skal benyttes.", validationResult.Errors);

        static void VerifyWrongDIPSMail(ProfileValidationResult validationResult) =>
            Assert.Contains("Brukernavnet ditt skal kun være dine tre DIPS-bokstaver. Dette kan endres via https://dipsasa.slack.com/account/settings", validationResult.Errors);

        static void VerifyMissingFirstName(ProfileValidationResult validationResult) =>
            Assert.Contains("Fornavn må registreres slik at folk vet hvem du er.", validationResult.Errors);

        static void VerifyMissingLastName(ProfileValidationResult validationResult) =>
            Assert.Contains("Etternavn må registreres slik at folk vet hvem du er.", validationResult.Errors);

        static void VerifyMissingWhatIDo(ProfileValidationResult validationResult) =>
            Assert.Contains("Feltet \"What I do\" må inneholde team og hva du kan i DIPS.", validationResult.Errors);

        static void VerifyMissingImage(ProfileValidationResult validationResult) =>
            Assert.Contains("Legg inn et profilbilde slik at folk kjenner deg igjen.", validationResult.Errors);
    }
}