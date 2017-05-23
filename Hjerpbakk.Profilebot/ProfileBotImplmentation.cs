using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hjerpbakk.ProfileBot.Commands;
using Hjerpbakk.ProfileBot.Contracts;
using NLog;
using SlackConnector.Models;

namespace Hjerpbakk.ProfileBot {
    /// <summary>
    /// A Slack-bot which verifies user profiles. The bot get its commands
    /// from direct messages. If the bot doesn't understand, it will list
    /// its available commands.
    /// </summary>
    public sealed class ProfilebotImplmentation : IDisposable {
        static readonly Logger logger;

        readonly ISlackIntegration slackIntegration;

        readonly ISlackProfileValidator slackProfileValidator;
        readonly string adminUserId;

        /// <summary>
        /// Gets the logger for this class.
        /// </summary>
        static ProfilebotImplmentation() {
            logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Creates the Slack-bot.
        /// </summary>
        /// <param name="slackIntegration">Used for talking to the Slack APIs.</param>
        /// <param name="slackProfileValidator">Used for checking user profiles for completeness.</param>
        /// <param name="adminUser">Used for sending the results to the Slack admin.</param>
        public ProfilebotImplmentation(ISlackIntegration slackIntegration, ISlackProfileValidator slackProfileValidator, AdminUser adminUser) {
            this.slackIntegration = slackIntegration ?? throw new ArgumentNullException(nameof(slackIntegration));
            this.slackProfileValidator = slackProfileValidator ?? throw new ArgumentNullException(nameof(slackProfileValidator));

            if (string.IsNullOrEmpty(adminUser.Id)) {
                throw new ArgumentException(nameof(adminUser.Id));
            }

            adminUserId = adminUser.Id;
        }

        /// <summary>
        /// Connects the bot to Slack.
        /// </summary>
        /// <returns>No object or value is returned by this method when it completes.</returns>
        public async Task Connect() {
            await slackIntegration.Connect();
            slackIntegration.MessageReceived += MessageReceived;
        }

        /// <summary>
        /// Disconnects the bot from Slack.
        /// </summary>
        public void Dispose() {
            slackIntegration.MessageReceived -= MessageReceived;
            slackIntegration.Dispose();
        }

        /// <summary>
        /// Parses the messages sent to the bot and answers to the best of its abilities.
        /// 
        /// Extend this method to include more commands.
        /// </summary>
        /// <param name="message">The message sent to the bot.</param>
        /// <returns>No object or value is returned by this method when it completes.</returns>
        async Task MessageReceived(SlackMessage message) {
            try {
                VerifyMessageIsComplete(message);
                var command = MessageParser.ParseCommand(message, adminUserId);
                switch (command) {
                    case AnswerRegularUserCommand _:
                        await AnswerRegularUser(message.User);
                        break;
                    case ValidateAllProfilesCommand _:
                        await ValidateAllProfiles();
                        break;
                    case NotifyAllProfilesCommand _:
                        await ValidateAllProfiles(true);
                        break;
                    case ValidateSingleProfileCommand c:
                        await ValidateSingleProfile(message.User.Id, c.Payload);
                        break;
                    case NotifySingleProfileCommand c:
                        await ValidateSingleProfile(message.User.Id, c.Payload, true);
                        break;
                    default:
                        await slackIntegration.SendDirectMessage(message.User.Id,
                            $"Available commands are:{Environment.NewLine}- validate all users{Environment.NewLine}- notify all users{Environment.NewLine}- validate @user{Environment.NewLine}- notify @user");
                        break;
                }
            }
            catch (Exception e) {
                try {
                    logger.Error(e);
                    await slackIntegration.SendDirectMessage(adminUserId, $"I crashed:{Environment.NewLine}{e}");
                }
                catch (Exception exception) {
                    logger.Error(exception);
                    throw;
                }
            }
        }

        static void VerifyMessageIsComplete(SlackMessage message) {
            if (message == null) {
                throw new ArgumentNullException(nameof(message));
            }

            if (message.User == null) {
                throw new ArgumentNullException(nameof(message.User));
            }

            if (string.IsNullOrEmpty(message.User.Id)) {
                throw new ArgumentException(nameof(message.User.Id));
            }

            if (string.IsNullOrEmpty(message.Text)) {
                throw new ArgumentException(nameof(message.Text));
            }
        }

        async Task AnswerRegularUser(SlackUser user) {
            await slackIntegration.SendDirectMessage(user.Id, "Checking your profile");
            var verificationResult = slackProfileValidator.ValidateProfile(user);
            if (verificationResult.IsValid) {
                await slackIntegration.SendDirectMessage(user.Id,
                    $"Well done <@{user.Id}>, your profile is complete");
            }
            else {
                await slackIntegration.SendDirectMessage(user.Id,
                    verificationResult.Errors);
            }
        }

        async Task ValidateAllProfiles(bool informUsers = false) {
            if (informUsers) {
                await slackIntegration.SendDirectMessage(adminUserId, "Notifying all users");
            }
            else {
                await slackIntegration.SendDirectMessage(adminUserId, "Validating all users");
            }
            var usersWithIncompleteProfiles = new List<ProfileValidationResult>();
            foreach (var user in await slackIntegration.GetAllUsers()) {
                var verificationResult = slackProfileValidator.ValidateProfile(user);
                if (verificationResult.IsValid) {
                    continue;
                }

                usersWithIncompleteProfiles.Add(verificationResult);
                if (!informUsers) {
                    continue;
                }

                await slackIntegration.SendDirectMessage(verificationResult.UserId, verificationResult.Errors);
                await slackIntegration.SendDirectMessage(adminUserId, verificationResult.Errors);
            }

            var messageToOwner = usersWithIncompleteProfiles.Count == 0
                ? "No profiles contain errors :)"
                : $"{usersWithIncompleteProfiles.Count} users have bad profiles:{Environment.NewLine}{String.Join(", ", usersWithIncompleteProfiles.Select(error => $"<@{error.UserId}>"))}";
            await slackIntegration.SendDirectMessage(adminUserId, messageToOwner);
        }

        async Task ValidateSingleProfile(string sender, SlackStringUser user, bool notify = false) {
            var verb = notify ? "Notifying" : "Validating";
            await slackIntegration.SendDirectMessage(sender, $"{verb} {user.SlackUserIdAsString}");
            var userToCheck = await slackIntegration.GetUser(user.UserId);
            var verificationResult = slackProfileValidator.ValidateProfile(userToCheck);
            if (verificationResult.IsValid) {
                await slackIntegration.SendDirectMessage(sender,
                    $"{user.SlackUserIdAsString} has a complete profile");
                return;
            }

            await slackIntegration.SendDirectMessage(sender, verificationResult.Errors);
            if (notify) {
                await slackIntegration.SendDirectMessage(verificationResult.UserId, verificationResult.Errors);
            }
        }
    }
}