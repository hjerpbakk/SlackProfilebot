using System;
using SlackConnector.Models;

namespace Hjerpbakk.Profilebot.Contracts {
    /// <summary>
    ///     The result of a Slack profile validation.
    /// </summary>
    public struct ProfileValidationResult {
        /// <summary>
        ///     Constructs a result of a profile that failed validation.
        /// </summary>
        /// <param name="user">The Slack user to which this result belongs.</param>
        /// <param name="errors">The errors found.</param>
        /// <param name="imageURL">Has value if the user's profile image is invalid.</param>
        public ProfileValidationResult(SlackUser user, string errors, Uri imageURL = null) :
            this(false, user, errors, imageURL) {
            if (string.IsNullOrEmpty(errors)) {
                throw new ArgumentException(nameof(errors));
            }
        }

        ProfileValidationResult(bool isValid, SlackUser user, string errors, Uri imageURL) {
            user.Guard();
            IsValid = isValid;
            User = user;
            Errors = errors;
            ImageURL = imageURL;
        }

        /// <summary>
        ///     Whether the validation was successful.
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        ///     The Slack user to which this result belongs.
        /// </summary>
        public SlackUser User { get; }

        /// <summary>
        ///     The errors found, if any.
        /// </summary>
        public string Errors { get; }

        /// <summary>
        ///     Has value if the user's profile image is invalid.
        /// </summary>
        public Uri ImageURL { get; }

        /// <summary>
        ///     Creates a successful result.
        /// </summary>
        public static ProfileValidationResult Valid(SlackUser user) {
            return new ProfileValidationResult(true, user, "", null);
        }
    }
}