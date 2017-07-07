using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Hjerpbakk.Profilebot.Commands;
using Hjerpbakk.Profilebot.Contracts;
using Hjerpbakk.Profilebot.FaceDetection;
using Hjerpbakk.Profilebot.FaceDetection.Report;
using NLog;
using SlackConnector.Models;

namespace Hjerpbakk.Profilebot {
    /// <summary>
    ///     A Slack-bot which verifies user profiles. The bot get its commands
    ///     from direct messages. If the bot doesn't understand, it will list
    ///     its available commands.
    /// </summary>
    public sealed class ProfilebotImplmentation : IDisposable {
        static readonly Logger logger;

        readonly SlackUser adminUser;
        readonly IFaceWhitelist faceWhitelist;
        readonly ISlackIntegration slackIntegration;
        readonly ISlackProfileValidator slackProfileValidator;

        /// <summary>
        ///     Gets the logger for this class.
        /// </summary>
        static ProfilebotImplmentation() {
            logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        ///     Creates the Slack-bot.
        /// </summary>
        /// <param name="slackIntegration">Used for talking to the Slack APIs.</param>
        /// <param name="slackProfileValidator">Used for checking user profiles for completeness.</param>
        /// <param name="adminUser">Used for sending the results to the Slack admin.</param>
        /// <param name="faceWhitelist">Knows about whitelisted users.</param>
        public ProfilebotImplmentation(ISlackIntegration slackIntegration, ISlackProfileValidator slackProfileValidator, SlackUser adminUser, IFaceWhitelist faceWhitelist) {
            this.slackIntegration = slackIntegration ?? throw new ArgumentNullException(nameof(slackIntegration));
            this.slackProfileValidator = slackProfileValidator ?? throw new ArgumentNullException(nameof(slackProfileValidator));

            if (string.IsNullOrEmpty(adminUser.Id)) {
                throw new ArgumentException(nameof(adminUser.Id));
            }

            this.adminUser = new SlackUser {Id = adminUser.Id};
            this.faceWhitelist = faceWhitelist ?? throw new ArgumentNullException(nameof(faceWhitelist));
        }

        /// <summary>
        ///     Disconnects the bot from Slack.
        /// </summary>
        public void Dispose() {
            slackIntegration.MessageReceived -= MessageReceived;
            slackIntegration.Dispose();
        }

        /// <summary>
        ///     Connects the bot to Slack.
        /// </summary>
        /// <returns>No object or value is returned by this method when it completes.</returns>
        public async Task Connect() {
            await slackIntegration.Connect();
            slackIntegration.MessageReceived += MessageReceived;
        }

        /// <summary>
        ///     Parses the messages sent to the bot and answers to the best of its abilities.
        ///     Extend this method to include more commands.
        /// </summary>
        /// <param name="message">The message sent to the bot.</param>
        /// <returns>No object or value is returned by this method when it completes.</returns>
        async Task MessageReceived(SlackMessage message) {
            try {
                VerifyMessageIsComplete(message);
                if (message.ChatHub.Type != SlackChatHubType.DM) {
                    return;
                }

                var command = MessageParser.ParseCommand(message, adminUser);
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
                        await ValidateSingleProfile(message.User, c.Payload);
                        break;
                    case NotifySingleProfileCommand c:
                        await ValidateSingleProfile(message.User, c.Payload, true);
                        break;
                    case WhitelistSingleProfileCommand c:
                        await WhitelistProfile(c.Payload);
                        break;
                    case ShowVersionNumberCommand _:
                        await SendVersionNumber();
                        break;
                    case ShowWhitelistedUsersCommand _:
                        await SendWhitelistedUsers();
                        break;
                    default:
                        await slackIntegration.SendDirectMessage(message.User,
                            $"Available commands are:{Environment.NewLine}- validate all users{Environment.NewLine}- notify all users{Environment.NewLine}- validate @user{Environment.NewLine}- notify @user{Environment.NewLine}- whitelist{Environment.NewLine}- whitelist @user{Environment.NewLine}- version");
                        break;
                }
            }
            catch (Exception e) {
                logger.Error(e);
                try {
                    await slackIntegration.SendDirectMessage(adminUser, $"I crashed:{Environment.NewLine}{e}");
                }
                catch (Exception exception) {
                    logger.Fatal(exception);
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

            if (message.ChatHub == null) {
                throw new ArgumentNullException(nameof(message.ChatHub));
            }
        }

        async Task AnswerRegularUser(SlackUser user) {
            await slackIntegration.SendDirectMessage(user, "Checking your profile");
            var verificationResult = await slackProfileValidator.ValidateProfile(user);
            if (verificationResult.IsValid) {
                await slackIntegration.SendDirectMessage(user,
                    $"Well done <@{user.Id}>, your profile is complete");
            }
            else {
                await slackIntegration.SendDirectMessage(user, verificationResult.Errors);
            }
        }

        async Task ValidateAllProfiles(bool informUsers = false) {
            if (informUsers) {
                await slackIntegration.SendDirectMessage(adminUser, "Notifying all users");
            }
            else {
                await slackIntegration.SendDirectMessage(adminUser, "Validating all users");
            }

            async Task<string> ValidateAllProfiles() {
                var usersWithIncompleteProfiles = new List<ProfileValidationResult>();
                foreach (var user in await slackIntegration.GetAllUsers()) {
                    await slackIntegration.IndicateTyping(adminUser);
                    var verificationResult = await slackProfileValidator.ValidateProfile(user);
                    if (verificationResult.IsValid) {
                        continue;
                    }

                    usersWithIncompleteProfiles.Add(verificationResult);
                    if (!informUsers) {
                        continue;
                    }

                    await slackIntegration.SendDirectMessage(verificationResult.User, verificationResult.Errors);
                    await slackIntegration.SendDirectMessage(adminUser, verificationResult.Errors);
                }

                var validationReport = new ValidationReport(usersWithIncompleteProfiles.ToArray());
                await faceWhitelist.UploadReport(validationReport);
                return validationReport.ToString();
            }

            await slackIntegration.SendDirectMessage(adminUser, await ValidateAllProfiles());
        }

        async Task ValidateSingleProfile(SlackUser sender, SlackUser user, bool notify = false) {
            var verb = notify ? "Notifying" : "Validating";
            await slackIntegration.SendDirectMessage(sender, $"{verb} {user.FormattedUserId}");
            var userToCheck = await slackIntegration.GetUser(user.Id);
            var validationResult = await slackProfileValidator.ValidateProfile(userToCheck);
            if (validationResult.IsValid) {
                await slackIntegration.SendDirectMessage(sender, $"{user.FormattedUserId} has a complete profile");
                return;
            }

            await slackIntegration.SendDirectMessage(sender, validationResult.Errors);
            if (notify) {
                await slackIntegration.SendDirectMessage(validationResult.User, validationResult.Errors);
            }
        }

        async Task WhitelistProfile(SlackUser user) {
            await slackIntegration.IndicateTyping(adminUser);
            await faceWhitelist.WhitelistUser(user);
            await slackIntegration.SendDirectMessage(adminUser, $"Whitelisted {user.FormattedUserId}");
        }

        async Task SendVersionNumber() {
            await slackIntegration.IndicateTyping(adminUser);
            var version = Assembly.GetAssembly(typeof(ProfilebotImplmentation)).GetName().Version.ToString();
            await slackIntegration.SendDirectMessage(adminUser, version);
        }

        async Task SendWhitelistedUsers() {
            await slackIntegration.IndicateTyping(adminUser);
            var whitelistedUsers = await faceWhitelist.GetWhitelistedUsers();
            var message = "Whitelist: " + string.Join(", ", whitelistedUsers.Select(u => u.FormattedUserId));
            await slackIntegration.SendDirectMessage(adminUser, message);
        }
    }
}