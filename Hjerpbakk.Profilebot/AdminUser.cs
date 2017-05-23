using System;

namespace Hjerpbakk.ProfileBot {
    /// <summary>
    /// Represents an admin user in Slack.
    /// </summary>
    public struct AdminUser {
        /// <summary>
        /// Creates a Slack admin user with a given id.
        /// </summary>
        /// <param name="adminUserId">The Id of the Slack admin user.</param>
        public AdminUser(string adminUserId) {
            if (string.IsNullOrEmpty(adminUserId)) {
                throw new ArgumentException(adminUserId);
            }

            Id = adminUserId;
        }

        /// <summary>
        /// The Id of the Slack admin user.
        /// </summary>
        public string Id { get; }
    }
}