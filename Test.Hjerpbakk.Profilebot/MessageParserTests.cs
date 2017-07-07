using System.Collections.Generic;
using Hjerpbakk.Profilebot;
using Hjerpbakk.Profilebot.Commands;
using SlackConnector.Models;
using Xunit;

namespace Test.Hjerpbakk.Profilebot {
    public class MessageParserTests {
        readonly SlackUser adminUser;

        public static IEnumerable<object[]> InvalidSlackIds = new[] {
            new object[] {"U1TBU8336"},
            new object[] {"<U1TBU8336"},
            new object[] {"<@U1TBU8336"}
        };

        public MessageParserTests() {
            adminUser = new SlackUser {Id = "AdminUserId"};
        }

        [Theory]
        [MemberData(nameof(InvalidSlackIds))]
        public void GetSlackUserFromFormattedUserId(string badUserId) {
            var command = MessageParser.ParseCommand(CreateMessage(adminUser, "Whitelist " + badUserId), adminUser);

            Assert.IsType<UnknownCommand>(command);
        }

        [Fact]
        public void ParseCommand_AnswerNormalUserCommand() {
            var command = MessageParser.ParseCommand(CreateMessage(new SlackUser {Id = "NotAdmin"}, "I don't know this API..."), adminUser);

            Assert.IsType<AnswerRegularUserCommand>(command);
        }

        [Fact]
        public void ParseCommand_NotifuAllUsers() {
            var command = MessageParser.ParseCommand(CreateMessage(adminUser, "Notify all users"), adminUser);

            Assert.IsType<NotifyAllProfilesCommand>(command);
        }

        [Fact]
        public void ParseCommand_NotifySingleUser() {
            var slackUserToBeVerified = new SlackUser {Id = "U4GHU76NA"};

            var command = (ProfileBotCommand<SlackUser>) MessageParser.ParseCommand(
                CreateMessage(adminUser, "Notify <@U4GHU76NA>"), adminUser);

            Assert.IsType<NotifySingleProfileCommand>(command);
            Assert.Equal(slackUserToBeVerified.Id, command.Payload.Id);
        }

        [Fact]
        public void ParseCommand_UnknownCommandAsAdmin() {
            var command = MessageParser.ParseCommand(CreateMessage(adminUser, "I don't know this API..."), adminUser);

            Assert.IsType<UnknownCommand>(command);
        }

        [Fact]
        public void ParseCommand_UnknownSingleUserCommand() {
            var command = MessageParser.ParseCommand(
                CreateMessage(adminUser, "Doit <@U4GHU76NA>"), adminUser);

            Assert.IsType<UnknownCommand>(command);
        }

        [Fact]
        public void ParseCommand_VerifyAllUsers() {
            var command = MessageParser.ParseCommand(CreateMessage(adminUser, "Validate all users"), adminUser);

            Assert.IsType<ValidateAllProfilesCommand>(command);
        }

        [Fact]
        public void ParseCommand_VerifySingleUser() {
            var slackUserToBeVerified = new SlackUser {Id = "U4GHU76NA"};

            var command = (ProfileBotCommand<SlackUser>) MessageParser.ParseCommand(
                CreateMessage(adminUser, "Validate <@U4GHU76NA>"), adminUser);

            Assert.IsType<ValidateSingleProfileCommand>(command);
            Assert.Equal(slackUserToBeVerified.Id, command.Payload.Id);
        }

        [Fact]
        public void ParseCommand_WhitelistSingleUser() {
            var slackUserToBeVerified = new SlackUser {Id = "U4GHU76NA"};

            var command = (ProfileBotCommand<SlackUser>) MessageParser.ParseCommand(
                CreateMessage(adminUser, "Whitelist <@U4GHU76NA>"), adminUser);

            Assert.IsType<WhitelistSingleProfileCommand>(command);
            Assert.Equal(slackUserToBeVerified.Id, command.Payload.Id);
        }

        [Fact]
        public void ParseCommand_SpaceAfterUser_IsParsedCorrectly() {
            var command = (ProfileBotCommand<SlackUser>) MessageParser.ParseCommand(
                CreateMessage(adminUser, "Whitelist <@U4GHU76NA> "), adminUser);

            Assert.IsType<WhitelistSingleProfileCommand>(command);
        }

        [Fact]
        public void ParseCommand_ShowVersion() {
            var command = MessageParser.ParseCommand(CreateMessage(adminUser, "version"), adminUser);

            Assert.IsType<ShowVersionNumberCommand>(command);
        }

        [Fact]
        public void ParseCommand_ShowWhitelistedUsers() {
            var command = MessageParser.ParseCommand(CreateMessage(adminUser, "whitelist"), adminUser);

            Assert.IsType<ShowWhitelistedUsersCommand>(command);
        }

        public static SlackMessage CreateMessage(SlackUser sender, string messageText) =>
            new SlackMessage {User = sender, Text = messageText, ChatHub = new SlackChatHub {Type = SlackChatHubType.DM}};
    }
}