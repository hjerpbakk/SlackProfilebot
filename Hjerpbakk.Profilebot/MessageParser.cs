using Hjerpbakk.ProfileBot.Commands;
using SlackConnector.Models;

namespace Hjerpbakk.ProfileBot {
    internal static class MessageParser {
        public static ProfileBotCommand ParseCommand(SlackMessage message, string adminUserId) {
            var normalizedMessage = message.Text.ToLower();
            return message.User.Id == adminUserId ? ParseAdminCommand(normalizedMessage) : AnswerRegularUserCommand.Create();
        }

        static ProfileBotCommand ParseAdminCommand(string normalizedMessage) {
            switch (normalizedMessage) {
                case "validate all users":
                    return ValidateAllProfilesCommand.Create();
                case "notify all users":
                    return NotifyAllProfilesCommand.Create();
            }

            var commandParts = normalizedMessage.Split(' ');
            if (commandParts.Length == 2 && commandParts[1].StartsWith("<@")) {
                var verb = commandParts[0];
                var subject = new SlackStringUser(commandParts[1].ToUpper());
                switch (verb) {
                    case "validate":
                        return new ValidateSingleProfileCommand(subject);
                    case "notify":
                        return new NotifySingleProfileCommand(subject);
                }
            }

            return UnknownCommand.Create();
        }
    }
}