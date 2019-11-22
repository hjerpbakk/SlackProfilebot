using Hjerpbakk.Profilebot.Commands;
using SlackConnector.Models;

namespace Hjerpbakk.Profilebot {
    internal static class MessageParser {
        public static ProfileBotCommand ParseCommand(SlackMessage message, SlackUser adminUser) {
            var normalizedMessage = message.Text.Trim().ToLower();
            return message.User.Id == adminUser.Id ? ParseAdminCommand(normalizedMessage) : AnswerRegularUserCommand.Create();
        }

        static ProfileBotCommand ParseAdminCommand(string normalizedMessage) {
            switch (normalizedMessage) {
                case "validate all users":
                    return ValidateAllProfilesCommand.Create();
                case "notify all users":
                    return NotifyAllProfilesCommand.Create();
                case "version":
                    return ShowVersionNumberCommand.Create();
                default:
                    var commandParts = normalizedMessage.Split(' ');
                    if (commandParts.Length == 2 && commandParts[1].StartsWith("<@") && commandParts[1][commandParts[1].Length - 1] == '>') {
                        return ParseVerbSubjectCommands(commandParts);
                    }

                    if (normalizedMessage == "whitelist") {
                        return ShowWhitelistedUsersCommand.Create();
                    }

                    return UnknownCommand.Create();
            }
        }

        static ProfileBotCommand ParseVerbSubjectCommands(string[] commandParts) {
            var verb = commandParts[0];
            var slackUserId = commandParts[1].Substring(2, commandParts[1].Length - 3).ToUpper();
            var subject = new SlackUser {Id = slackUserId};
            switch (verb) {
                case "validate":
                    return new ValidateSingleProfileCommand(subject);
                case "notify":
                    return new NotifySingleProfileCommand(subject);
                case "whitelist":
                    return new WhitelistSingleProfileCommand(subject);
                default:
                    return UnknownCommand.Create();
            }
        }
    }
}