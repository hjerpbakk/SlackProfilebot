using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using Hjerpbakk.ProfileBot;
using Hjerpbakk.ProfileBot.Contracts;
using Moq;
using SlackConnector.Models;
using Xunit;

namespace Test.Hjerpbakk.Profilebot {
    public class ProfilebotImplmentationTests {
        static readonly AdminUser adminUser;

        public static IEnumerable<object[]> InvalidMessages = new[] {
            new object[] {null},
            new object[] {new SlackMessage()},
            new object[] {new SlackMessage {User = new SlackUser()}},
            new object[] {new SlackMessage {User = new SlackUser {Id = "Id"}}}
        };

        static ProfilebotImplmentationTests() {
            adminUser = new AdminUser("U1TBU8336");
        }

        [Fact]
        public void Constructor_NoSlackIntegration_Fails() {
            var exception =
                Record.Exception(() => new ProfilebotImplmentation(null, new SlackProfileValidator(adminUser), adminUser));

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void Constructor_NoSlackProfileChecker_Fails() {
            var exception =
                Record.Exception(
                    () => new ProfilebotImplmentation((new Mock<ISlackIntegration>()).Object, null, adminUser));

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public async Task Init_Called_Inits() {
            var creationResult = await CreateProfileBot();

            await creationResult.ProfileBot.Connect();

            creationResult.SlackIntegration.Verify(s => s.Connect());
        }

        [Fact]
        public async Task Dispose_Called_Disposes() {
            var creationResult = await CreateProfileBot();

            creationResult.ProfileBot.Dispose();

            creationResult.SlackIntegration.Verify(s => s.Dispose());
        }

        [Fact]
        public async Task VerifyAllProfiles_NoProfileErrors() {
            var users = new List<SlackUser> {new SlackUser(), new SlackUser()};
            var creationResult = await CreateProfileBot(true);
            creationResult.SlackIntegration.Setup(s => s.GetAllUsers()).ReturnsAsync(users);
            creationResult.SlackProfileChecker.Setup(s => s.ValidateProfile(It.IsAny<SlackUser>()))
                .Returns(ProfileValidationResult.CreateValid());

            creationResult.SlackIntegration.Raise(s => s.MessageReceived += null,
                MessageParserTests.CreateMessage(adminUser.Id, "validate all users"));

            creationResult.SlackIntegration.Verify(s => s.SendDirectMessage(adminUser.Id, "No profiles contain errors :)"));
        }

        [Fact]
        public async Task VerifyAllProfiles() {
            var users = new List<SlackUser> {new SlackUser {Id = "User1"}, new SlackUser {Id = "User2"}};
            var creationResult = await CreateProfileBot(true);
            creationResult.SlackIntegration.Setup(s => s.GetAllUsers()).ReturnsAsync(users);
            creationResult.SlackProfileChecker.Setup(s => s.ValidateProfile(It.IsAny<SlackUser>()))
                .Returns<SlackUser>(s => new ProfileValidationResult(false, s.Id, ""));

            creationResult.SlackIntegration.Raise(s => s.MessageReceived += null,
                MessageParserTests.CreateMessage(adminUser.Id, "validate all users"));

            creationResult.SlackIntegration.Verify(s => s.SendDirectMessage(adminUser.Id,
                "2 users have bad profiles:\r\n<@User1>, <@User2>"));
        }

        [Fact]
        public async Task NotifyAllProfiles() {
            var users = new List<SlackUser> {new SlackUser {Id = "User1"}, new SlackUser {Id = "User2"}};
            var creationResult = await CreateProfileBot(true);
            var slackIntegration = creationResult.SlackIntegration;
            slackIntegration.Setup(s => s.GetAllUsers()).ReturnsAsync(users);
            creationResult.SlackProfileChecker.Setup(s => s.ValidateProfile(It.IsAny<SlackUser>()))
                .Returns<SlackUser>(s => new ProfileValidationResult(false, s.Id, $"Error {s.Id}"));

            slackIntegration.Raise(s => s.MessageReceived += null,
                MessageParserTests.CreateMessage(adminUser.Id, "Notify all users"));

            slackIntegration.Verify(s => s.SendDirectMessage("User1", "Error User1"));
            slackIntegration.Verify(
                s => s.SendDirectMessage(adminUser.Id, "Error User1"));
            slackIntegration.Verify(s => s.SendDirectMessage("User2", "Error User2"));
            slackIntegration.Verify(
                s => s.SendDirectMessage(adminUser.Id, "Error User2"));
            slackIntegration.Verify(s => s.SendDirectMessage(adminUser.Id,
                "2 users have bad profiles:\r\n<@User1>, <@User2>"));
        }

        [Fact]
        public async Task VerifySingleProfile_ValidProfile() {
            var user = new SlackUser {Id = adminUser.Id};
            var creationResult = await CreateProfileBot(true);
            var slackIntegration = creationResult.SlackIntegration;
            slackIntegration.Setup(s => s.GetUser(adminUser.Id)).ReturnsAsync(user);
            creationResult.SlackProfileChecker.Setup(s => s.ValidateProfile(It.IsAny<SlackUser>())).Returns(ProfileValidationResult.CreateValid());

            slackIntegration.Raise(s => s.MessageReceived += null,
                MessageParserTests.CreateMessage(adminUser.Id, "Validate <@U1TBU8336>"));

            slackIntegration.Verify(s => s.SendDirectMessage(adminUser.Id, "Validating <@U1TBU8336>"));
            slackIntegration.Verify(s => s.SendDirectMessage(adminUser.Id, "<@U1TBU8336> has a complete profile"));
        }

        [Fact]
        public async Task VerifySingleProfile_IncompleteProfile() {
            var user = new SlackUser {Id = adminUser.Id};
            var creationResult = await CreateProfileBot(true);
            var slackIntegration = creationResult.SlackIntegration;
            slackIntegration.Setup(s => s.GetUser(adminUser.Id)).ReturnsAsync(user);
            creationResult.SlackProfileChecker.Setup(s => s.ValidateProfile(It.IsAny<SlackUser>())).Returns<SlackUser>(s => new ProfileValidationResult(false, s.Id, $"Error {s.Id}"));

            slackIntegration.Raise(s => s.MessageReceived += null,
                MessageParserTests.CreateMessage(adminUser.Id, "Validate <@U1TBU8336>"));

            slackIntegration.Verify(s => s.SendDirectMessage(adminUser.Id, "Validating <@U1TBU8336>"));
            slackIntegration.Verify(s => s.SendDirectMessage(adminUser.Id, "Error U1TBU8336"));
        }

        [Fact]
        public async Task NotifySingleProfile_ValidProfile() {
            var user = new SlackUser {Id = adminUser.Id};
            var creationResult = await CreateProfileBot(true);
            var slackIntegration = creationResult.SlackIntegration;
            slackIntegration.Setup(s => s.GetUser(adminUser.Id)).ReturnsAsync(user);
            creationResult.SlackProfileChecker.Setup(s => s.ValidateProfile(It.IsAny<SlackUser>())).Returns(ProfileValidationResult.CreateValid());

            slackIntegration.Raise(s => s.MessageReceived += null,
                MessageParserTests.CreateMessage(adminUser.Id, "Notify <@U1TBU8336>"));

            slackIntegration.Verify(s => s.SendDirectMessage(adminUser.Id, "Notifying <@U1TBU8336>"));
            slackIntegration.Verify(s => s.SendDirectMessage(adminUser.Id, "<@U1TBU8336> has a complete profile"));
        }

        [Fact]
        public async Task NotifySingleProfile_IncompleteProfile() {
            var user = new SlackUser {Id = adminUser.Id};
            var creationResult = await CreateProfileBot(true);
            var slackIntegration = creationResult.SlackIntegration;
            slackIntegration.Setup(s => s.GetUser(adminUser.Id)).ReturnsAsync(user);
            creationResult.SlackProfileChecker.Setup(s => s.ValidateProfile(It.IsAny<SlackUser>())).Returns<SlackUser>(s => new ProfileValidationResult(false, s.Id, $"Error {s.Id}"));

            slackIntegration.Raise(s => s.MessageReceived += null,
                MessageParserTests.CreateMessage(adminUser.Id, "Notify <@U1TBU8336>"));

            slackIntegration.Verify(s => s.SendDirectMessage(adminUser.Id, "Notifying <@U1TBU8336>"));
            slackIntegration.Verify(s => s.SendDirectMessage(adminUser.Id, "Error U1TBU8336"));
            slackIntegration.Verify(s => s.SendDirectMessage(adminUser.Id, "Error U1TBU8336"));
        }

        [Fact]
        public async Task AnswerRegularUser_ValidProfile() {
            var user = new SlackUser {Id = "U1TBU8337"};
            var creationResult = await CreateProfileBot(true);
            var slackIntegration = creationResult.SlackIntegration;
            slackIntegration.Setup(s => s.GetUser("U1TBU8337")).ReturnsAsync(user);
            creationResult.SlackProfileChecker.Setup(s => s.ValidateProfile(It.IsAny<SlackUser>())).Returns(ProfileValidationResult.CreateValid());

            slackIntegration.Raise(s => s.MessageReceived += null,
                MessageParserTests.CreateMessage("U1TBU8337", "Any message"));

            slackIntegration.Verify(s => s.SendDirectMessage("U1TBU8337", "Checking your profile"));
            slackIntegration.Verify(s => s.SendDirectMessage("U1TBU8337", "Well done <@U1TBU8337>, your profile is complete"));
        }

        [Fact]
        public async Task AnswerRegularUser_IncompleteProfile() {
            var user = new SlackUser {Id = "U1TBU8337"};
            var creationResult = await CreateProfileBot(true);
            var slackIntegration = creationResult.SlackIntegration;
            slackIntegration.Setup(s => s.GetUser("U1TBU8337")).ReturnsAsync(user);
            creationResult.SlackProfileChecker.Setup(s => s.ValidateProfile(It.IsAny<SlackUser>())).Returns<SlackUser>(s => new ProfileValidationResult(false, s.Id, $"Error {s.Id}"));

            slackIntegration.Raise(s => s.MessageReceived += null,
                MessageParserTests.CreateMessage("U1TBU8337", "Any message"));

            slackIntegration.Verify(s => s.SendDirectMessage("U1TBU8337", "Checking your profile"));
            slackIntegration.Verify(s => s.SendDirectMessage("U1TBU8337", "Error U1TBU8337"));
        }

        [Fact]
        public async Task UnknownCommand_AdminSendsMessage_ListsAvailableCommands() {
            var creationResult = await CreateProfileBot(true);
            var slackIntegration = creationResult.SlackIntegration;

            slackIntegration.Raise(s => s.MessageReceived += null,
                MessageParserTests.CreateMessage(adminUser.Id, "Unknown command"));

            slackIntegration.Verify(s => s.SendDirectMessage(adminUser.Id, $"Available commands are:{Environment.NewLine}- validate all users{Environment.NewLine}- notify all users{Environment.NewLine}- validate @user{Environment.NewLine}- notify @user"));
        }

        [Fact]
        public async Task CommandFails() {
            var creationResult = await CreateProfileBot(true);
            var slackIntegration = creationResult.SlackIntegration;
            slackIntegration.Setup(s => s.GetUser(adminUser.Id)).Throws<Exception>();

            slackIntegration.Raise(s => s.MessageReceived += null,
                MessageParserTests.CreateMessage(adminUser.Id, "Notify <@U1TBU8336>"));

            slackIntegration.Verify(s => s.SendDirectMessage(adminUser.Id, "Notifying <@U1TBU8336>"));
            slackIntegration.Verify(s => s.SendDirectMessage(adminUser.Id, It.IsRegex("I crashed")));
        }

        [Theory]
        [MemberData(nameof(InvalidMessages))]
        public async Task InvalidMessages_Fails(SlackMessage message) {
            var creationResult = await CreateProfileBot(true);
            var slackIntegration = creationResult.SlackIntegration;

            slackIntegration.Raise(s => s.MessageReceived += null, message);

            slackIntegration.Verify(s => s.SendDirectMessage(adminUser.Id, It.IsRegex("I crashed")));
        }

        [Fact]
        public void Constructor_NoAdminId_Fails() {
            var slackIntegrationFake = new Mock<ISlackIntegration>();
            var slackProfileCheckerFake = new Mock<ISlackProfileValidator>();
            var exception = Record.Exception(() => new ProfilebotImplmentation(slackIntegrationFake.Object, slackProfileCheckerFake.Object, new AdminUser()));

            Assert.IsType<ArgumentException>(exception);
        }

        async Task<(ProfilebotImplmentation ProfileBot, Mock<ISlackIntegration> SlackIntegration, Mock<ISlackProfileValidator> SlackProfileChecker)> CreateProfileBot(bool init = false) {
            var slackIntegrationFake = new Mock<ISlackIntegration>();
            var slackProfileCheckerFake = new Mock<ISlackProfileValidator>();
            var profileBot = new ProfilebotImplmentation(slackIntegrationFake.Object, slackProfileCheckerFake.Object, adminUser);
            if (init) {
                await profileBot.Connect();
            }

            return (profileBot, slackIntegrationFake, slackProfileCheckerFake);
        }
    }
}