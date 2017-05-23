using SlackConnector.Models;
using Hjerpbakk.ProfileBot.Contracts;

namespace Hjerpbakk.ProfileBot {
    /// <summary>
    /// Validates the profile of a Slack user according to your team's rules.
    /// </summary>
    public class SlackProfileValidator : ISlackProfileValidator {
        /// <summary>
        /// Validates that a user profile is complete.
        /// </summary>
        /// <param name="user">The user to be validated.</param>
        /// <returns>The result of the validation.</returns>
        public ProfileValidationResult ValidateProfile(SlackUser user) {
            // NOTE: Replace this with your own validation logic
            return !string.IsNullOrEmpty(user.FirstName) ? ProfileValidationResult.CreateValid() : new ProfileValidationResult(false, user.Id, "First name is missing");
        }
    }
}