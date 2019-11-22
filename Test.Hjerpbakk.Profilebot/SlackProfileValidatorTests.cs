using System;
using System.Threading.Tasks;
using Hjerpbakk.Profilebot;
using Hjerpbakk.Profilebot.Contracts;
using Hjerpbakk.Profilebot.FaceDetection;
using Moq;
using SlackConnector.Models;
using Xunit;

namespace Test.Hjerpbakk.Profilebot {
    public class SlackProfileValidatorTests {
        public SlackProfileValidatorTests() {
            faceDetectionClient = new Mock<IFaceDetectionClient>();
            slackProfileValidator = new SlackProfileValidator(new SlackUser {Id = "U1TBU8336"}, faceDetectionClient.Object);
        }

        const string UserId = "SLACKUSERID";

        readonly ISlackProfileValidator slackProfileValidator;
        readonly Mock<IFaceDetectionClient> faceDetectionClient;

        static SlackUser CreateUser() {
            return new SlackUser {Id = UserId, Name = "roh"};
        }

        // ReSharper disable once UnusedParameter.Local
        static void VerifyValidationResult(ProfileValidationResult validationResult) {
            Assert.Equal(false, validationResult.IsValid);
            Assert.Equal(UserId, validationResult.User.Id);
        }

        // ReSharper disable once UnusedParameter.Local
        static void VerifyEmailExist(ProfileValidationResult validationResult) {
            Assert.Contains("Din DIPS-epost må være registrert på brukeren din.", validationResult.Errors);
        }

        // ReSharper disable once UnusedParameter.Local
        static void VerifyOtherThanDIPSMail(ProfileValidationResult validationResult) {
            Assert.Contains("Kun DIPS-epost skal benyttes.", validationResult.Errors);
        }

        // ReSharper disable once UnusedParameter.Local
        static void VerifyWrongDIPSMail(ProfileValidationResult validationResult) {
            Assert.Contains("Brukernavnet ditt skal kun være dine tre DIPS-bokstaver. Dette kan endres via https://dipsasa.slack.com/account/settings", validationResult.Errors);
        }

        // ReSharper disable once UnusedParameter.Local
        static void VerifyMissingFirstName(ProfileValidationResult validationResult) {
            Assert.Contains("Fornavn må registreres slik at folk vet hvem du er.", validationResult.Errors);
        }

        // ReSharper disable once UnusedParameter.Local
        static void VerifyMissingLastName(ProfileValidationResult validationResult) {
            Assert.Contains("Etternavn må registreres slik at folk vet hvem du er.", validationResult.Errors);
        }

        // ReSharper disable once UnusedParameter.Local
        static void VerifyMissingWhatIDo(ProfileValidationResult validationResult) {
            Assert.Contains("Feltet \"What I do\" må inneholde team og hva du kan i DIPS.", validationResult.Errors);
        }

        // ReSharper disable once UnusedParameter.Local
        static void VerifyMissingImage(ProfileValidationResult validationResult) {
            Assert.Contains("Legg inn et profilbilde slik at folk kjenner deg igjen.", validationResult.Errors);
        }

        // ReSharper disable once UnusedParameter.Local
        static void VerifyBadImage(ProfileValidationResult validationResult) {
            Assert.Contains("Bad image", validationResult.Errors);
        }

        [Fact]
        public void VerifyProfile_AdminIdIsNull_Throws() {
            var exception = Record.Exception(() => new SlackProfileValidator(new SlackUser(), new Mock<IFaceDetectionClient>().Object));

            Assert.IsType<ArgumentException>(exception);
        }

        [Fact]
        public async Task VerifyProfile_CompleteProfile_Valid() {
            var user = CreateUser();
            user.Email = "roh@dips.no";
            user.FirstName = "Runar";
            user.LastName = "Hjerpbakk";
            user.WhatIDo = "Software Engineering Manager";
            user.Image = "http://image.jpg";
            faceDetectionClient.Setup(f => f.ValidateProfileImage(It.IsAny<SlackUser>())).ReturnsAsync(FaceDetectionResult.Valid);

            var validationResult = await slackProfileValidator.ValidateProfile(user);

            Assert.Equal(true, validationResult.IsValid);
            Assert.Same(user, validationResult.User);
            Assert.Equal("", validationResult.Errors);
            Assert.Null(validationResult.ImageURL);
        }

        [Fact]
        public async Task VerifyProfile_EverythingMissing_InvalidAndAllErrorsReturned() {
            var user = CreateUser();

            var validationResult = await slackProfileValidator.ValidateProfile(user);

            VerifyValidationResult(validationResult);
            VerifyEmailExist(validationResult);
            VerifyMissingFirstName(validationResult);
            VerifyMissingLastName(validationResult);
            VerifyMissingWhatIDo(validationResult);
            VerifyMissingImage(validationResult);
        }

        [Fact]
        public void VerifyProfile_FaceClientIsNull_Throws() {
            var exception = Record.Exception(() => new SlackProfileValidator(new SlackUser {Id = "U1TBU8336"}, null));

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public async Task VerifyProfile_HasMissingFirstName_Invalid() {
            var user = CreateUser();
            user.Email = "roh@dips.no";

            var validationResult = await slackProfileValidator.ValidateProfile(user);

            VerifyValidationResult(validationResult);
            VerifyMissingFirstName(validationResult);
        }

        [Fact]
        public async Task VerifyProfile_HasMissingImage_Invalid() {
            var user = CreateUser();
            user.Email = "roh@dips.no";
            user.FirstName = "Runar";
            user.LastName = "Hjerpbakk";
            user.WhatIDo = "Software Engineering Manager";

            var validationResult = await slackProfileValidator.ValidateProfile(user);

            VerifyValidationResult(validationResult);
            VerifyMissingImage(validationResult);
        }

        [Fact]
        public async Task VerifyProfile_HasMissingLastName_Invalid() {
            var user = CreateUser();
            user.Email = "roh@dips.no";
            user.FirstName = "Runar";

            var validationResult = await slackProfileValidator.ValidateProfile(user);

            VerifyValidationResult(validationResult);
            VerifyMissingLastName(validationResult);
        }

        [Fact]
        public async Task VerifyProfile_HasMissingWhatIDo_Invalid() {
            var user = CreateUser();
            user.Email = "roh@dips.no";
            user.FirstName = "Runar";
            user.LastName = "Hjerpbakk";

            var validationResult = await slackProfileValidator.ValidateProfile(user);

            VerifyValidationResult(validationResult);
            VerifyMissingWhatIDo(validationResult);
        }

        [Fact]
        public async Task VerifyProfile_HasNoId_Throws() {
            var user = new SlackUser();

            // ReSharper disable once PossibleNullReferenceException
            var exception = await Record.ExceptionAsync(() => slackProfileValidator.ValidateProfile(user));

            Assert.IsType<ArgumentException>(exception);
        }

        [Fact]
        public async Task VerifyProfile_HasNoMail_Invalid() {
            var user = CreateUser();

            var validationResult = await slackProfileValidator.ValidateProfile(user);

            VerifyValidationResult(validationResult);
            VerifyEmailExist(validationResult);
        }

        [Fact]
        public async Task VerifyProfile_HasNoName_Throws() {
            var user = new SlackUser {Id = UserId};

            // ReSharper disable once PossibleNullReferenceException
            var exception = await Record.ExceptionAsync(() => slackProfileValidator.ValidateProfile(user));

            Assert.IsType<ArgumentException>(exception);
            Assert.Contains("Name cannot be empty.", exception.Message);
        }

        [Fact]
        public async Task VerifyProfile_HasOtherThanDIPSMail_Invalid() {
            var user = CreateUser();
            user.Email = "runar@hjerpbakk.com";

            var validationResult = await slackProfileValidator.ValidateProfile(user);

            VerifyValidationResult(validationResult);
            VerifyOtherThanDIPSMail(validationResult);
        }

        [Fact]
        public async Task VerifyProfile_HasWrongDIPSMail_Invalid() {
            var user = CreateUser();
            user.Email = "runar@dips.no";

            var validationResult = await slackProfileValidator.ValidateProfile(user);

            VerifyValidationResult(validationResult);
            VerifyWrongDIPSMail(validationResult);
        }

        [Fact]
        public async Task VerifyProfile_InvalidImage_InvalidAndAllErrorsReturned() {
            var user = CreateUser();
            user.Image = "http://image.jpg";
            faceDetectionClient.Setup(f => f.ValidateProfileImage(It.IsAny<SlackUser>())).ReturnsAsync(new FaceDetectionResult("Bad image"));

            var validationResult = await slackProfileValidator.ValidateProfile(user);

            VerifyValidationResult(validationResult);
            VerifyEmailExist(validationResult);
            VerifyMissingFirstName(validationResult);
            VerifyMissingLastName(validationResult);
            VerifyMissingWhatIDo(validationResult);
            VerifyBadImage(validationResult);
        }

        [Fact]
        public async Task VerifyProfile_SlackUserIsNull_Throws() {
            // ReSharper disable once PossibleNullReferenceException
            var exception = await Record.ExceptionAsync(() => slackProfileValidator.ValidateProfile(null));

            Assert.IsType<ArgumentNullException>(exception);
        }
    }
}