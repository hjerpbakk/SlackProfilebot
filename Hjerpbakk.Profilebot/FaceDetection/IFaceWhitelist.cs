using System.Threading.Tasks;
using Hjerpbakk.Profilebot.FaceDetection.Report;
using SlackConnector.Models;

namespace Hjerpbakk.Profilebot.FaceDetection {
    /// <summary>
    ///     Knows about whitelisted users.
    /// </summary>
    public interface IFaceWhitelist {
        /// <summary>
        ///     Checks whether a given user is whitelisted.
        /// </summary>
        /// <param name="user">The Slack user to check.</param>
        /// <returns>Whether the given user is whitelisted.</returns>
        Task<bool> IsUserWhitelisted(SlackUser user);

        /// <summary>
        ///     Whitelists the given user.
        /// </summary>
        /// <param name="user">The Slack user to whitelist.</param>
        /// <returns>No object or value is returned by this method when it completes.</returns>
        Task WhitelistUser(SlackUser user);

        /// <summary>
        ///     Uploads a report to the Azure Blob Storage.
        /// </summary>
        /// <param name="report">The report to upload.</param>
        /// <returns>No object or value is returned by this method when it completes.</returns>
        Task UploadReport(ValidationReport report);

        /// <summary>
        ///     Gets the whitelisted users.
        /// </summary>
        /// <returns>The whitelisted users</returns>
        Task<SlackUser[]> GetWhitelistedUsers();
    }
}