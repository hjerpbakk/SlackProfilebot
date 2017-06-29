using System;
using System.Threading.Tasks;
using Hjerpbakk.Profilebot.Configuration;
using Hjerpbakk.Profilebot.FaceDetection;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Moq;
using SlackConnector.Models;
using Xunit;

namespace Test.Hjerpbakk.Profilebot.FaceDetection {
    public class FaceDetectionClientTests {
        Mock<IFaceServiceClient> faceDetectionClientFake;
        Mock<IFaceWhitelist> faceWhitelistFake;

        IFaceDetectionClient Create() {
            faceDetectionClientFake = new Mock<IFaceServiceClient>();
            faceWhitelistFake = new Mock<IFaceWhitelist>();
            var faceDetectionClient = new FaceDetectionClient(faceDetectionClientFake.Object, faceWhitelistFake.Object, new FaceDetectionConfiguration());
            return faceDetectionClient;
        }

        [Fact]
        public void FaceClientServiceMissing_Fails() {
            var exception = Record.Exception(() => new FaceDetectionClient(null, new Mock<IFaceWhitelist>().Object, new FaceDetectionConfiguration()));

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void FaceWhiteListMissing_Fails() {
            var exception = Record.Exception(() => new FaceDetectionClient(new Mock<IFaceServiceClient>().Object, null, new FaceDetectionConfiguration()));

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public async Task ValidateProfileImage_0Faces() {
            var faceDetectionClient = Create();
            faceDetectionClientFake.Setup(f => f.DetectAsync(It.IsAny<string>(), false, false, null)).ReturnsAsync(new Face[0]);
            var expected = new FaceDetectionResult("Kunne ikke se et ansikt i bildet ditt. Last opp et profilbilde av deg selv.");

            var faceDetectionResult = await faceDetectionClient.ValidateProfileImage(new SlackUser {Id = "id", Image = "http://image.jpg"});

            Assert.Equal(expected, faceDetectionResult);
        }

        [Fact]
        public async Task ValidateProfileImage_1Face() {
            var faceDetectionClient = Create();
            faceDetectionClientFake.Setup(f => f.DetectAsync(It.IsAny<string>(), false, false, null)).ReturnsAsync(new[] {new Face()});

            var faceDetectionResult = await faceDetectionClient.ValidateProfileImage(new SlackUser {Id = "id", Image = "http://image.jpg"});

            Assert.Equal(FaceDetectionResult.Valid, faceDetectionResult);
        }

        [Fact]
        public async Task ValidateProfileImage_Crashes() {
            var faceDetectionClient = Create();
            faceDetectionClientFake.Setup(f => f.DetectAsync(It.IsAny<string>(), false, false, null)).ThrowsAsync(new Exception());

            var faceDetectionResult = await faceDetectionClient.ValidateProfileImage(new SlackUser {Id = "id", Image = "http://image.jpg"});

            Assert.Equal(FaceDetectionResult.Valid, faceDetectionResult);
        }

        [Fact]
        public async Task ValidateProfileImage_FaceException() {
            var faceDetectionClient = Create();
            faceDetectionClientFake.SetupSequence(f => f.DetectAsync(It.IsAny<string>(), false, false, null)).ThrowsAsync(new FaceAPIException()).ReturnsAsync(new[] {new Face()});

            var faceDetectionResult = await faceDetectionClient.ValidateProfileImage(new SlackUser {Id = "id", Image = "http://image.jpg"});

            Assert.Equal(FaceDetectionResult.Valid, faceDetectionResult);
        }

        [Fact]
        public async Task ValidateProfileImage_MoreThan1Faces() {
            var faceDetectionClient = Create();
            faceDetectionClientFake.Setup(f => f.DetectAsync(It.IsAny<string>(), false, false, null)).ReturnsAsync(new[] {new Face(), new Face()});
            var expected = new FaceDetectionResult("Fant flere ansikter i bildet litt. Last opp et profilbilde av deg selv.");

            var faceDetectionResult = await faceDetectionClient.ValidateProfileImage(new SlackUser {Id = "id", Image = "http://image.jpg"});

            Assert.Equal(expected, faceDetectionResult);
        }

        [Fact]
        public async Task ValidateProfileImage_TimesOut() {
            var faceDetectionClient = Create();
            faceDetectionClientFake.Setup(f => f.DetectAsync(It.IsAny<string>(), false, false, null)).ThrowsAsync(new FaceAPIException());

            var faceDetectionResult = await faceDetectionClient.ValidateProfileImage(new SlackUser {Id = "id", Image = "http://image.jpg"});

            Assert.Equal(FaceDetectionResult.Valid, faceDetectionResult);
        }

        [Fact]
        public async Task ValidateProfileImage_UserIsInvalid_Fails() {
            var faceDetectionClient = Create();

            // ReSharper disable once PossibleNullReferenceException
            var exception = await Record.ExceptionAsync(() => faceDetectionClient.ValidateProfileImage(null));

            Assert.IsType<ArgumentNullException>(exception);

            // ReSharper disable once PossibleNullReferenceException
            exception = await Record.ExceptionAsync(() => faceDetectionClient.ValidateProfileImage(new SlackUser()));

            Assert.IsType<ArgumentException>(exception);

            // ReSharper disable once PossibleNullReferenceException
            exception = await Record.ExceptionAsync(() => faceDetectionClient.ValidateProfileImage(new SlackUser {Id = "Id"}));

            Assert.IsType<ArgumentException>(exception);
        }

        [Fact]
        public async Task ValidateProfileImage_UserIsWhitelisted_Valid() {
            var faceDetectionClient = Create();
            faceWhitelistFake.Setup(f => f.IsUserWhitelisted(It.IsAny<SlackUser>())).ReturnsAsync(true);

            var faceDetectionResult = await faceDetectionClient.ValidateProfileImage(new SlackUser {Id = "id", Image = "http://image.jpg"});

            Assert.Equal(FaceDetectionResult.Valid, faceDetectionResult);
        }
    }
}