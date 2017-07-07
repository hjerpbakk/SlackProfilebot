using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hjerpbakk.Profilebot.FaceDetection;
using Hjerpbakk.Profilebot;
using Hjerpbakk.Profilebot.Contracts;
using Moq;
using SlackConnector.Models;
using Xunit;

namespace Test.Hjerpbakk.Profilebot {
    public class ProfilebotImplmentationTests {
        static ProfilebotImplmentationTests() {
            adminUser = new SlackUser {Id = "U1TBU8336"};
        }

        static readonly SlackUser adminUser;

        public static IEnumerable<object[]> InvalidMessages = new[] {
            new object[] {null},
            new object[] {new SlackMessage {ChatHub = new SlackChatHub {Type = SlackChatHubType.DM}}},
            new object[] {new SlackMessage {User = new SlackUser(), ChatHub = new SlackChatHub {Type = SlackChatHubType.DM}}},
            new object[] {new SlackMessage {User = new SlackUser {Id = "Id"}, ChatHub = new SlackChatHub {Type = SlackChatHubType.DM}}},
            new object[] {new SlackMessage {User = new SlackUser {Id = "Id"}, Text = "SlackChatHubMissing"}}
        };

        [Theory]
        [MemberData(nameof(InvalidMessages))]
        public async Task InvalidMessages_Fails(SlackMessage message) {
            var creationResult = await CreateProfileBot(true);
            var slackIntegration = creationResult.SlackIntegration;

            slackIntegration.Raise(s => s.MessageReceived += null, message);

            slackIntegration.Verify(s => s.SendDirectMessage(It.Is<SlackUser>(u => u.Id == adminUser.Id), It.IsRegex("I crashed")));
        }

        async Task<(ProfilebotImplmentation ProfileBot, Mock<ISlackIntegration> SlackIntegration, Mock<ISlackProfileValidator> SlackProfileChecker, Mock<IFaceWhitelist> FaceWhitelistFake)> CreateProfileBot(bool init = false) {
            var slackIntegrationFake = new Mock<ISlackIntegration>();
            var slackProfileCheckerFake = new Mock<ISlackProfileValidator>();
            var faceWhitelistFake = new Mock<IFaceWhitelist>();
            var profileBot = new ProfilebotImplmentation(slackIntegrationFake.Object, slackProfileCheckerFake.Object, adminUser, faceWhitelistFake.Object);
            if (init) {
                await profileBot.Connect();
            }

            return (profileBot, slackIntegrationFake, slackProfileCheckerFake, faceWhitelistFake);
        }

        [Fact]
        public async Task AnswerRegularUser_IncompleteProfile() {
            var user = new SlackUser {Id = "U1TBU8337"};
            var creationResult = await CreateProfileBot(true);
            var slackIntegration = creationResult.SlackIntegration;
            slackIntegration.Setup(s => s.GetUser("U1TBU8337")).ReturnsAsync(user);
            creationResult.SlackProfileChecker.Setup(s => s.ValidateProfile(It.IsAny<SlackUser>())).Returns<SlackUser>(s => Task.FromResult(FailedResult(s.Id)));

            slackIntegration.Raise(s => s.MessageReceived += null,
                MessageParserTests.CreateMessage(user, "Any message"));

            slackIntegration.Verify(s => s.SendDirectMessage(user, "Checking your profile"));
            slackIntegration.Verify(s => s.SendDirectMessage(user, "Error U1TBU8337"));
        }

        [Fact]
        public async Task AnswerRegularUser_ValidProfile() {
            var user = new SlackUser {Id = "U1TBU8337"};
            var creationResult = await CreateProfileBot(true);
            var slackIntegration = creationResult.SlackIntegration;
            slackIntegration.Setup(s => s.GetUser("U1TBU8337")).ReturnsAsync(user);
            creationResult.SlackProfileChecker.Setup(s => s.ValidateProfile(It.IsAny<SlackUser>())).ReturnsAsync(ValidResult());

            slackIntegration.Raise(s => s.MessageReceived += null,
                MessageParserTests.CreateMessage(user, "Any message"));

            slackIntegration.Verify(s => s.SendDirectMessage(user, "Checking your profile"));
            slackIntegration.Verify(s => s.SendDirectMessage(user, "Well done <@U1TBU8337>, your profile is complete"));
        }

        [Fact]
        public async Task CommandFails() {
            var creationResult = await CreateProfileBot(true);
            var slackIntegration = creationResult.SlackIntegration;
            slackIntegration.Setup(s => s.GetUser(adminUser.Id)).Throws<Exception>();

            slackIntegration.Raise(s => s.MessageReceived += null,
                MessageParserTests.CreateMessage(adminUser, "Notify <@U1TBU8336>"));

            slackIntegration.Verify(s => s.SendDirectMessage(It.Is<SlackUser>(u => u.Id == adminUser.Id), "Notifying <@U1TBU8336>"));
            slackIntegration.Verify(s => s.SendDirectMessage(It.Is<SlackUser>(u => u.Id == adminUser.Id), It.IsRegex("I crashed")));
        }

        [Fact]
        public void Constructor_NoAdminId_Fails() {
            var slackIntegrationFake = new Mock<ISlackIntegration>();
            var slackProfileCheckerFake = new Mock<ISlackProfileValidator>();
            var faceWhitelistFake = new Mock<IFaceWhitelist>();
            var exception = Record.Exception(() => new ProfilebotImplmentation(slackIntegrationFake.Object, slackProfileCheckerFake.Object, new SlackUser(), faceWhitelistFake.Object));

            Assert.IsType<ArgumentException>(exception);
        }

        [Fact]
        public void Constructor_NoFaceWhitelist_Fails() {
            var exception =
                Record.Exception(
                    () => new ProfilebotImplmentation(new Mock<ISlackIntegration>().Object, new Mock<ISlackProfileValidator>().Object, adminUser, null));

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void Constructor_NoSlackIntegration_Fails() {
            var exception =
                Record.Exception(() => new ProfilebotImplmentation(null, new Mock<ISlackProfileValidator>().Object, adminUser, new Mock<IFaceWhitelist>().Object));

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void Constructor_NoSlackProfileChecker_Fails() {
            var exception =
                Record.Exception(
                    () => new ProfilebotImplmentation(new Mock<ISlackIntegration>().Object, null, adminUser, new Mock<IFaceWhitelist>().Object));

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public async Task Dispose_Called_Disposes() {
            var creationResult = await CreateProfileBot();

            creationResult.ProfileBot.Dispose();

            creationResult.SlackIntegration.Verify(s => s.Dispose());
        }

        [Fact]
        public async Task Init_Called_Inits() {
            var creationResult = await CreateProfileBot();

            await creationResult.ProfileBot.Connect();

            creationResult.SlackIntegration.Verify(s => s.Connect());
        }

        [Fact]
        public async Task InvalidMessage_AdminReportAlsoFails_DoesNotCrash() {
            var creationResult = await CreateProfileBot(true);
            var slackIntegration = creationResult.SlackIntegration;
            slackIntegration.Setup(s => s.SendDirectMessage(It.Is<SlackUser>(u => u.Id == adminUser.Id), It.IsRegex("I crashed"))).Throws(new Exception());
            slackIntegration.Setup(s => s.SendDirectMessage(It.Is<SlackUser>(u => u.Id == adminUser.Id), It.IsRegex("Available commands"))).Throws(new Exception());

            slackIntegration.Raise(s => s.MessageReceived += null, MessageParserTests.CreateMessage(adminUser, "Message"));

            slackIntegration.Verify(s => s.SendDirectMessage(It.Is<SlackUser>(u => u.Id == adminUser.Id), It.IsRegex("I crashed")));
        }

        [Fact]
        public async Task NotifyAllProfiles() {
            var users = new List<SlackUser> {new SlackUser {Id = "User1"}, new SlackUser {Id = "User2"}};
            var creationResult = await CreateProfileBot(true);
            var slackIntegration = creationResult.SlackIntegration;
            slackIntegration.Setup(s => s.GetAllUsers()).ReturnsAsync(users);
            creationResult.SlackProfileChecker.Setup(s => s.ValidateProfile(It.IsAny<SlackUser>()))
                .Returns<SlackUser>(s => Task.FromResult(FailedResult(s.Id)));

            slackIntegration.Raise(s => s.MessageReceived += null,
                MessageParserTests.CreateMessage(adminUser, "Notify all users"));

            slackIntegration.Verify(s => s.SendDirectMessage(It.Is<SlackUser>(u => u.Id == users[0].Id), "Error User1"));
            slackIntegration.Verify(s => s.SendDirectMessage(It.Is<SlackUser>(u => u.Id == adminUser.Id), "Error User1"));
            slackIntegration.Verify(s => s.SendDirectMessage(It.Is<SlackUser>(u => u.Id == users[1].Id), "Error User2"));
            slackIntegration.Verify(s => s.SendDirectMessage(It.Is<SlackUser>(u => u.Id == adminUser.Id), "Error User2"));
            slackIntegration.Verify(s => s.SendDirectMessage(It.Is<SlackUser>(u => u.Id == adminUser.Id), "2 users have bad profiles:\r\n<@User1>, <@User2>"));
        }

        [Fact]
        public async Task NotifySingleProfile_IncompleteProfile() {
            var user = new SlackUser {Id = adminUser.Id};
            var creationResult = await CreateProfileBot(true);
            var slackIntegration = creationResult.SlackIntegration;
            slackIntegration.Setup(s => s.GetUser(adminUser.Id)).ReturnsAsync(user);
            creationResult.SlackProfileChecker.Setup(s => s.ValidateProfile(It.IsAny<SlackUser>())).Returns<SlackUser>(s => Task.FromResult(FailedResult(s.Id)));

            slackIntegration.Raise(s => s.MessageReceived += null,
                MessageParserTests.CreateMessage(adminUser, "Notify <@U1TBU8336>"));

            slackIntegration.Verify(s => s.SendDirectMessage(adminUser, "Notifying <@U1TBU8336>"));
            slackIntegration.Verify(s => s.SendDirectMessage(adminUser, "Error U1TBU8336"));
            slackIntegration.Verify(s => s.SendDirectMessage(adminUser, "Error U1TBU8336"));
        }

        [Fact]
        public async Task NotifySingleProfile_ValidProfile() {
            var user = new SlackUser {Id = adminUser.Id};
            var creationResult = await CreateProfileBot(true);
            var slackIntegration = creationResult.SlackIntegration;
            slackIntegration.Setup(s => s.GetUser(adminUser.Id)).ReturnsAsync(user);
            creationResult.SlackProfileChecker.Setup(s => s.ValidateProfile(It.IsAny<SlackUser>())).ReturnsAsync(ValidResult());

            slackIntegration.Raise(s => s.MessageReceived += null,
                MessageParserTests.CreateMessage(adminUser, "Notify <@U1TBU8336>"));

            slackIntegration.Verify(s => s.SendDirectMessage(adminUser, "Notifying <@U1TBU8336>"));
            slackIntegration.Verify(s => s.SendDirectMessage(adminUser, "<@U1TBU8336> has a complete profile"));
        }

        [Fact]
        public async Task UnknownCommand_AdminSendsMessage_ListsAvailableCommands() {
            var creationResult = await CreateProfileBot(true);
            var slackIntegration = creationResult.SlackIntegration;

            slackIntegration.Raise(s => s.MessageReceived += null,
                MessageParserTests.CreateMessage(adminUser, "Unknown command"));

            slackIntegration.Verify(s => s.SendDirectMessage(adminUser, $"Available commands are:{Environment.NewLine}- validate all users{Environment.NewLine}- notify all users{Environment.NewLine}- validate @user{Environment.NewLine}- notify @user{Environment.NewLine}- whitelist{Environment.NewLine}- whitelist @user{Environment.NewLine}- version"));
        }

        [Fact]
        public async Task MessageInChannel_DoesNothing() {
            var creationResult = await CreateProfileBot(true);
            var slackIntegration = creationResult.SlackIntegration;
            var message = MessageParserTests.CreateMessage(adminUser, "Unknown command");
            message.ChatHub = new SlackChatHub {Type = SlackChatHubType.Channel};

            slackIntegration.Raise(s => s.MessageReceived += null, message);

            slackIntegration.Verify(s => s.SendDirectMessage(It.IsAny<SlackUser>(), It.IsAny<string>()), Times.Never());
        }

        [Fact]
        public async Task MessageInGroupChat_DoesNothing() {
            var creationResult = await CreateProfileBot(true);
            var slackIntegration = creationResult.SlackIntegration;
            var message = MessageParserTests.CreateMessage(adminUser, "Unknown command");
            message.ChatHub = new SlackChatHub {Type = SlackChatHubType.Group};

            slackIntegration.Raise(s => s.MessageReceived += null, message);

            slackIntegration.Verify(s => s.SendDirectMessage(It.IsAny<SlackUser>(), It.IsAny<string>()), Times.Never());
        }

        [Fact]
        public async Task VerifyAllProfiles() {
            var users = new List<SlackUser> {new SlackUser {Id = "User1"}, new SlackUser {Id = "User2"}};
            var creationResult = await CreateProfileBot(true);
            creationResult.SlackIntegration.Setup(s => s.GetAllUsers()).ReturnsAsync(users);
            creationResult.SlackProfileChecker.Setup(s => s.ValidateProfile(It.IsAny<SlackUser>()))
                .Returns<SlackUser>(s => Task.FromResult(FailedResult(s.Id)));

            creationResult.SlackIntegration.Raise(s => s.MessageReceived += null,
                MessageParserTests.CreateMessage(adminUser, "validate all users"));

            creationResult.SlackIntegration.Verify(s => s.SendDirectMessage(It.Is<SlackUser>(u => u.Id == adminUser.Id), "2 users have bad profiles:\r\n<@User1>, <@User2>"));
        }

        [Fact]
        public async Task VerifyAllProfiles_NoProfileErrors() {
            var users = new List<SlackUser> {new SlackUser(), new SlackUser()};
            var creationResult = await CreateProfileBot(true);
            creationResult.SlackIntegration.Setup(s => s.GetAllUsers()).ReturnsAsync(users);
            creationResult.SlackProfileChecker.Setup(s => s.ValidateProfile(It.IsAny<SlackUser>()))
                .ReturnsAsync(ValidResult());

            creationResult.SlackIntegration.Raise(s => s.MessageReceived += null,
                MessageParserTests.CreateMessage(adminUser, "validate all users"));

            creationResult.SlackIntegration.Verify(s => s.SendDirectMessage(It.Is<SlackUser>(u => u.Id == adminUser.Id), "No profiles contain errors :)"));
        }

        [Fact]
        public async Task VerifySingleProfile_IncompleteProfile() {
            var user = new SlackUser {Id = adminUser.Id};
            var creationResult = await CreateProfileBot(true);
            var slackIntegration = creationResult.SlackIntegration;
            slackIntegration.Setup(s => s.GetUser(adminUser.Id)).ReturnsAsync(user);
            creationResult.SlackProfileChecker.Setup(s => s.ValidateProfile(It.IsAny<SlackUser>())).Returns<SlackUser>(s => Task.FromResult(FailedResult(s.Id)));

            slackIntegration.Raise(s => s.MessageReceived += null,
                MessageParserTests.CreateMessage(adminUser, "Validate <@U1TBU8336>"));

            slackIntegration.Verify(s => s.SendDirectMessage(adminUser, "Validating <@U1TBU8336>"));
            slackIntegration.Verify(s => s.SendDirectMessage(adminUser, "Error U1TBU8336"));
        }

        [Fact]
        public async Task VerifySingleProfile_ValidProfile() {
            var user = new SlackUser {Id = adminUser.Id};
            var creationResult = await CreateProfileBot(true);
            var slackIntegration = creationResult.SlackIntegration;
            slackIntegration.Setup(s => s.GetUser(adminUser.Id)).ReturnsAsync(user);
            creationResult.SlackProfileChecker.Setup(s => s.ValidateProfile(It.IsAny<SlackUser>())).ReturnsAsync(ValidResult());

            slackIntegration.Raise(s => s.MessageReceived += null,
                MessageParserTests.CreateMessage(adminUser, "Validate <@U1TBU8336>"));

            slackIntegration.Verify(s => s.SendDirectMessage(adminUser, "Validating <@U1TBU8336>"));
            slackIntegration.Verify(s => s.SendDirectMessage(adminUser, "<@U1TBU8336> has a complete profile"));
        }

        [Fact]
        public async Task WhitelistProfile() {
            var creationResult = await CreateProfileBot(true);
            var slackIntegration = creationResult.SlackIntegration;

            slackIntegration.Raise(s => s.MessageReceived += null,
                MessageParserTests.CreateMessage(adminUser, "Whitelist <@U1TBU8336>"));

            slackIntegration.Verify(s => s.IndicateTyping(It.Is<SlackUser>(u => u.Id == adminUser.Id)));
            creationResult.FaceWhitelistFake.Verify(f => f.WhitelistUser(It.Is<SlackUser>(s => s.Id == adminUser.Id)));
            slackIntegration.Verify(s => s.SendDirectMessage(It.Is<SlackUser>(u => u.Id == adminUser.Id), "Whitelisted <@U1TBU8336>"));
        }

        [Fact]
        public async Task Version_ShowsVersion() {
            var creationResult = await CreateProfileBot(true);

            creationResult.SlackIntegration.Raise(s => s.MessageReceived += null, MessageParserTests.CreateMessage(adminUser, "version"));

            creationResult.SlackIntegration.Verify(s => s.IndicateTyping(It.Is<SlackUser>(u => u.Id == adminUser.Id)));
            creationResult.SlackIntegration.Verify(s => s.SendDirectMessage(It.Is<SlackUser>(u => u.Id == adminUser.Id), It.IsRegex("\\d*[.]\\d*[.]\\d*[.]\\d*")));
        }

        [Fact]
        public async Task Whitelist_ShowWhitelistedUsers() {
            var creationResult = await CreateProfileBot(true);
            var slackIntegration = creationResult.SlackIntegration;
            creationResult.FaceWhitelistFake.Setup(w => w.GetWhitelistedUsers()).ReturnsAsync(new[] {new SlackUser {Id = "U1TBU8336"}, new SlackUser {Id = "U1TBU8346"}});

            slackIntegration.Raise(s => s.MessageReceived += null, MessageParserTests.CreateMessage(adminUser, "Whitelist"));

            slackIntegration.Verify(s => s.IndicateTyping(It.Is<SlackUser>(u => u.Id == adminUser.Id)));
            creationResult.FaceWhitelistFake.Verify(f => f.GetWhitelistedUsers());
            slackIntegration.Verify(s => s.SendDirectMessage(It.Is<SlackUser>(u => u.Id == adminUser.Id), "Whitelist: <@U1TBU8336>, <@U1TBU8346>"));
        }

        static ProfileValidationResult ValidResult() =>
            ProfileValidationResult.Valid(new SlackUser {Id = "userId"});

        static ProfileValidationResult FailedResult(string userId) =>
            new ProfileValidationResult(new SlackUser {Id = userId}, $"Error {userId}");
    }
}