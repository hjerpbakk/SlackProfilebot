using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hjerpbakk.Profilebot.Configuration;
using Hjerpbakk.Profilebot.Contracts;
using Hjerpbakk.Profilebot.FaceDetection.Report;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using SlackConnector.Models;

namespace Hjerpbakk.Profilebot.FaceDetection {
    /// <summary>
    ///     Knows about whitelisted users.
    /// </summary>
    public class FaceWhitelist : IFaceWhitelist {
        readonly CloudBlobClient blobClient;
        readonly CloudBlobContainer container;
        readonly ConcurrentBag<string> whitelistedUserIds;

        /// <summary>
        ///     Creates the whitelist with the given configuration.
        /// </summary>
        /// <param name="configuration">The Azure Blob Storage configuration to use.</param>
        public FaceWhitelist(BlobStorageConfiguration configuration) {
            whitelistedUserIds = new ConcurrentBag<string>();
            var storageAccount = CloudStorageAccount.Parse(configuration.ConnectionString);

            blobClient = storageAccount.CreateCloudBlobClient();
            const string ContainerName = "whitelist";
            container = blobClient.GetContainerReference(ContainerName);
            container.CreateIfNotExists();
        }

        /// <summary>
        ///     Checks whether a given user is whitelisted.
        /// </summary>
        /// <param name="user">The Slack user to check.</param>
        /// <returns>Whether the given user is whitelisted.</returns>
        public async Task<bool> IsUserWhitelisted(SlackUser user) {
            user.Guard();
            if (whitelistedUserIds.Count == 0) {
                await PopulateWhitelist();
            }

            return whitelistedUserIds.Contains(user.Id);
        }

        /// <summary>
        ///     Whitelists the given user.
        /// </summary>
        /// <param name="user">The Slack user to whitelist.</param>
        /// <returns>No object or value is returned by this method when it completes.</returns>
        public async Task WhitelistUser(SlackUser user) {
            user.Guard();
            if (await IsUserWhitelisted(user)) {
                return;
            }

            var blobRef = container.GetBlockBlobReference(user.Id);
            await blobRef.UploadTextAsync(user.Id);
            whitelistedUserIds.Add(user.Id);
        }

        /// <summary>
        ///     Uploads a report to the Azure Blob Storage.
        /// </summary>
        /// <param name="report">The report to upload.</param>
        /// <returns>No object or value is returned by this method when it completes.</returns>
        public async Task UploadReport(ValidationReport report) {
            if (report == null) {
                throw new ArgumentNullException(nameof(report));
            }

            const string ReportName = "report";
            var reportContainer = blobClient.GetContainerReference(ReportName);
            reportContainer.CreateIfNotExists();

            var blobRef = reportContainer.GetBlockBlobReference(ReportName);
            await blobRef.UploadTextAsync(report.CreateHTMLReport());
        }

        async Task PopulateWhitelist() {
            var blobs = container.ListBlobs();
            foreach (var blob in blobs.Cast<CloudBlockBlob>()) {
                using (var memoryStream = new MemoryStream()) {
                    await blob.DownloadToStreamAsync(memoryStream);
                    var user = Encoding.UTF8.GetString(memoryStream.ToArray());
                    whitelistedUserIds.Add(user);
                }
            }
        }
    }
}