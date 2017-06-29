using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Hjerpbakk.Profilebot.Configuration;
using Hjerpbakk.Profilebot.Contracts;
using Hjerpbakk.Profilebot.FaceDetection;
using Hjerpbakk.Profilebot.FaceDetection.Report;
using Microsoft.WindowsAzure.Storage;
using SlackConnector.Models;
using Xunit;

namespace Test.Hjerpbakk.Profilebot.FaceDetection {
    /// <summary>
    ///     If tests fail, verify that the Azure Storage Emulator is running
    ///     on the local machine.
    /// </summary>
    public class FaceWhitelistTests {
        static FaceWhitelist Create() {
            return new FaceWhitelist(BlobStorageConfiguration.Local);
        }

        static async Task VerifyUserIsWhiteListed(SlackUser user) {
            var storageAccount = CloudStorageAccount.Parse(BlobStorageConfiguration.Local.ConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("whitelist");
            using (var memoryStream = new MemoryStream()) {
                await container.GetBlockBlobReference(user.Id).DownloadToStreamAsync(memoryStream);
                var actualUserId = Encoding.UTF8.GetString(memoryStream.ToArray());
                Assert.Equal(user.Id, actualUserId);
            }
        }

        [Fact]
        public void Ctor() {
            Create();
        }

        [Fact]
        public async Task IsUserWhitelisted_Empty_Fails() {
            var faceWhitelist = Create();

            // ReSharper disable once PossibleNullReferenceException
            var exception = await Record.ExceptionAsync(() => faceWhitelist.IsUserWhitelisted(new SlackUser()));

            Assert.IsType<ArgumentException>(exception);
        }

        /// <summary>
        ///     If test fail, verify that the Azure Storage Emulator is running
        ///     on the local machine.
        /// </summary>
        [Fact]
        public async Task IsUserWhitelisted_NonWhitelistedUser_False() {
            var faceWhitelist = Create();
            var nonWhitelistedUser = new SlackUser {Id = Path.GetRandomFileName()};

            Assert.False(await faceWhitelist.IsUserWhitelisted(nonWhitelistedUser));
        }

        [Fact]
        public async Task IsUserWhitelisted_Null_Fails() {
            var faceWhitelist = Create();

            // ReSharper disable once PossibleNullReferenceException
            var exception = await Record.ExceptionAsync(() => faceWhitelist.IsUserWhitelisted(null));

            Assert.IsType<ArgumentNullException>(exception);
        }

        /// <summary>
        ///     If test fail, verify that the Azure Storage Emulator is running
        ///     on the local machine.
        /// </summary>
        [Fact]
        public async Task IsUserWhitelisted_WhitelistedUser_True() {
            var faceWhitelist = Create();
            var whitelistedUser = new SlackUser {Id = Path.GetRandomFileName()};
            await faceWhitelist.WhitelistUser(whitelistedUser);

            Assert.True(await faceWhitelist.IsUserWhitelisted(whitelistedUser));
        }

        /// <summary>
        ///     If test fail, verify that the Azure Storage Emulator is running
        ///     on the local machine.
        /// </summary>
        [Fact]
        public async Task UploadReport() {
            var faceWhitelist = Create();
            var suspectImage = new Uri("http://" + Path.GetRandomFileName());
            var name = Path.GetRandomFileName();
            var userId = Path.GetRandomFileName();

            await faceWhitelist.UploadReport(new ValidationReport(new ProfileValidationResult(new SlackUser {Id = userId, Name = name}, "errors", suspectImage)));

            var storageAccount = CloudStorageAccount.Parse(BlobStorageConfiguration.Local.ConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("report");
            using (var memoryStream = new MemoryStream()) {
                await container.GetBlockBlobReference("report").DownloadToStreamAsync(memoryStream);
                var report = Encoding.UTF8.GetString(memoryStream.ToArray());
                Assert.Contains(suspectImage.AbsolutePath, report);
                Assert.Contains(name, report);
                Assert.Contains(userId, report);
            }
        }

        [Fact]
        public async Task UploadReport_Empty_Fails() {
            var faceWhitelist = Create();

            // ReSharper disable once PossibleNullReferenceException
            var exception = await Record.ExceptionAsync(() => faceWhitelist.UploadReport(null));

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public async Task WhitelistUser_Empty_Fails() {
            var faceWhitelist = Create();

            // ReSharper disable once PossibleNullReferenceException
            var exception = await Record.ExceptionAsync(() => faceWhitelist.WhitelistUser(new SlackUser()));

            Assert.IsType<ArgumentException>(exception);
        }

        /// <summary>
        ///     If test fail, verify that the Azure Storage Emulator is running
        ///     on the local machine.
        /// </summary>
        [Fact]
        public async Task WhitelistUser_ExistingUser_RemainsWhitelisted() {
            var faceWhitelist = Create();
            var userToWhiteList = new SlackUser {Id = Path.GetRandomFileName()};

            await faceWhitelist.WhitelistUser(userToWhiteList);

            await VerifyUserIsWhiteListed(userToWhiteList);

            await faceWhitelist.WhitelistUser(userToWhiteList);

            await VerifyUserIsWhiteListed(userToWhiteList);
        }

        /// <summary>
        ///     If test fail, verify that the Azure Storage Emulator is running
        ///     on the local machine.
        /// </summary>
        [Fact]
        public async Task WhitelistUser_NewUser_IsWhitelisted() {
            var faceWhitelist = Create();
            var userToWhiteList = new SlackUser {Id = Path.GetRandomFileName()};

            await faceWhitelist.WhitelistUser(userToWhiteList);

            await VerifyUserIsWhiteListed(userToWhiteList);
        }

        [Fact]
        public async Task WhitelistUser_Null_Fails() {
            var faceWhitelist = Create();

            // ReSharper disable once PossibleNullReferenceException
            var exception = await Record.ExceptionAsync(() => faceWhitelist.WhitelistUser(null));

            Assert.IsType<ArgumentNullException>(exception);
        }
    }
}