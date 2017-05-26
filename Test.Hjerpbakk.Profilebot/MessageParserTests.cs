using Hjerpbakk.ProfileBot;
using Hjerpbakk.ProfileBot.Commands;
using SlackConnector.Models;
using Xunit;

namespace Test.Hjerpbakk.Profilebot {
    public class MessageParserTests {
        const string AdminUserId = "AdminUserId";

        [Fact]
        public void ParseCommand_UnknownCommandAsAdmin() {
            var command = MessageParser.ParseCommand(CreateMessage(AdminUserId, "I don't know this API..."), AdminUserId);

            Assert.IsType<UnknownCommand>(command);
        }

        [Fact]
        public void ParseCommand_AnswerNormalUserCommand() {
            var command = MessageParser.ParseCommand(CreateMessage("NotAdmin", "I don't know this API..."), AdminUserId);

            Assert.IsType<AnswerRegularUserCommand>(command);
        }

        [Fact]
        public void ParseCommand_VerifyAllUsers() {
            var command = MessageParser.ParseCommand(CreateMessage(AdminUserId, "Validate all users"), AdminUserId);

            Assert.IsType<ValidateAllProfilesCommand>(command);
        }

        [Fact]
        public void ParseCommand_NotifuAllUsers() {
            var command = MessageParser.ParseCommand(CreateMessage(AdminUserId, "Notify all users"), AdminUserId);

            Assert.IsType<NotifyAllProfilesCommand>(command);
        }

        [Fact]
        public void ParseCommand_VerifySingleUser() {
            var slackUserToBeVerified = new SlackStringUser("<@U4GHU76NA>");

            var command = (ProfileBotCommand<SlackStringUser>) MessageParser.ParseCommand(
                CreateMessage(AdminUserId, "Validate <@U4GHU76NA>"), AdminUserId);

            Assert.IsType<ValidateSingleProfileCommand>(command);
            Assert.Equal(slackUserToBeVerified, command.Payload);
        }

        [Fact]
        public void ParseCommand_NotifySingleUser() {
            var slackUserToBeVerified = new SlackStringUser("<@U4GHU76NA>");

            var command = (ProfileBotCommand<SlackStringUser>) MessageParser.ParseCommand(
                CreateMessage(AdminUserId, "Notify <@U4GHU76NA>"), AdminUserId);

            Assert.IsType<NotifySingleProfileCommand>(command);
            Assert.Equal(slackUserToBeVerified, command.Payload);
        }

        [Fact]
        public void ParseCommand_UnknownSingleUserCommand() {
            var command = MessageParser.ParseCommand(
                CreateMessage(AdminUserId, "Doit <@U4GHU76NA>"), AdminUserId);

            Assert.IsType<UnknownCommand>(command);
        }

        public static SlackMessage CreateMessage(string senderId, string messageText) =>
            new SlackMessage {User = new SlackUser {Id = senderId}, Text = messageText};
    }
}