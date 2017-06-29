using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hjerpbakk.Profilebot;
using Moq;
using SlackConnector;
using SlackConnector.Models;
using Xunit;

namespace Test.Hjerpbakk.Profilebot {
    public class SlackIntegrationTests {
        bool called;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        async Task SlackIntegrationOnMessageReceived(SlackMessage message) {
            called = true;
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        async Task<(SlackIntegration SlackIntegration, Mock<ISlackConnector> Connector, Mock<ISlackConnection> Connection)> CreateSlackIntegration(bool init = true) {
            var slackConnectionFake = new Mock<ISlackConnection>();
            var slackConnectorFake = new Mock<ISlackConnector>();
            slackConnectorFake.Setup(s => s.Connect(It.IsAny<string>(), null)).ReturnsAsync(slackConnectionFake.Object);
            var slack = new SlackIntegration(slackConnectorFake.Object, "a");
            if (init) {
                await slack.Connect();
            }

            return (slack, slackConnectorFake, slackConnectionFake);
        }

        [Fact]
        public void Constructor_NoConnector_Fails() {
            var exception = Record.Exception(() => new SlackIntegration(null, "a"));

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void Constructor_NoSlackKey_Fails() {
            var exception = Record.Exception(() => new SlackIntegration(new Mock<ISlackConnector>().Object, null));

            Assert.IsType<ArgumentException>(exception);
        }

        [Fact]
        public async Task Dispose_AfterInit_Disconnects() {
            var slack = await CreateSlackIntegration();

            slack.SlackIntegration.Dispose();

            slack.Connection.Verify(c => c.Disconnect());
        }

        [Fact]
        public async Task Dispose_WithoutInit_DoesNothing() {
            var slack = await CreateSlackIntegration(false);

            slack.SlackIntegration.Dispose();
        }

        [Fact]
        public async Task GetAllUsers_GetAllUsers_ReturnsAllUsers() {
            var slack = await CreateSlackIntegration();

            await slack.SlackIntegration.GetAllUsers();

            slack.Connection.Verify(c => c.GetUsers());
        }

        [Fact]
        public async Task GetUser_InCache() {
            var slack = await CreateSlackIntegration();
            const string UserId = "UserId";
            var slackUser = new SlackUser();
            var userCache = new Dictionary<string, SlackUser> {{UserId, slackUser}};
            slack.Connection.Setup(c => c.UserCache).Returns(userCache);

            var user = await slack.SlackIntegration.GetUser(UserId);

            Assert.Same(slackUser, user);
        }

        [Fact]
        public async Task GetUser_NotInCache() {
            var slack = await CreateSlackIntegration();
            const string UserId = "UserId";
            var userCache = new Dictionary<string, SlackUser>();
            var slackUser = new SlackUser {Id = UserId};
            slack.Connection.Setup(c => c.GetUsers()).ReturnsAsync(new List<SlackUser> {slackUser});
            slack.Connection.Setup(c => c.UserCache).Returns(userCache);

            var user = await slack.SlackIntegration.GetUser(UserId);

            Assert.Same(slackUser, user);
        }

        [Fact]
        public async Task GetUser_NoUserIdGiven_Fails() {
            var slack = await CreateSlackIntegration();

            // ReSharper disable once PossibleNullReferenceException
            var exception = await Record.ExceptionAsync(() => slack.SlackIntegration.GetUser(null));

            Assert.IsType<ArgumentException>(exception);
        }

        [Fact]
        public async Task IndicateTyping() {
            var slack = await CreateSlackIntegration();
            var user = new SlackUser {Id = "UserId"};
            var slackChatHub = new SlackChatHub();
            slack.Connection.Setup(c => c.JoinDirectMessageChannel(user.Id)).ReturnsAsync(slackChatHub);

            await slack.SlackIntegration.IndicateTyping(user);

            slack.Connection.Verify(c => c.IndicateTyping(slackChatHub));
        }

        [Fact]
        public async Task IndicateTyping_NoUserId_Fails() {
            var slack = await CreateSlackIntegration();

            // ReSharper disable once PossibleNullReferenceException
            var exception = await Record.ExceptionAsync(() => slack.SlackIntegration.IndicateTyping(new SlackUser()));

            Assert.IsType<ArgumentException>(exception);
        }

        [Fact]
        public async Task Init_WithValidConnector_Connects() {
            var slack = await CreateSlackIntegration();

            slack.Connector.Verify(c => c.Connect(It.IsAny<string>(), null));
        }

        [Fact]
        public async Task MessageReceived() {
            var slack = await CreateSlackIntegration();
            slack.SlackIntegration.MessageReceived += SlackIntegrationOnMessageReceived;

            slack.Connection.Raise(s => s.OnMessageReceived += null, (SlackMessage) null);

            Assert.True(called);

            called = false;
            slack.SlackIntegration.MessageReceived -= SlackIntegrationOnMessageReceived;

            slack.Connection.Raise(s => s.OnMessageReceived += null, (SlackMessage) null);

            Assert.False(called);
        }

        [Fact]
        public async Task SendDirectMessage_NoText_Fails() {
            var slack = await CreateSlackIntegration();

            // ReSharper disable once PossibleNullReferenceException
            var exception = await Record.ExceptionAsync(() => slack.SlackIntegration.SendDirectMessage(new SlackUser {Id = "UserId"}, null));

            Assert.IsType<ArgumentException>(exception);
        }

        [Fact]
        public async Task SendDirectMessage_NoUserId_Fails() {
            var slack = await CreateSlackIntegration();

            // ReSharper disable once PossibleNullReferenceException
            var exception = await Record.ExceptionAsync(() => slack.SlackIntegration.SendDirectMessage(null, "Message text"));

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public async Task SendDirectMessage_ValidUserAndMessage_SendsMessage() {
            var user = new SlackUser {Id = "UserId"};
            const string Text = "Message text";
            var slack = await CreateSlackIntegration();

            await slack.SlackIntegration.SendDirectMessage(user, Text);

            slack.Connection.Verify(c => c.JoinDirectMessageChannel(user.Id));
            slack.Connection.Verify(c => c.IndicateTyping(It.IsAny<SlackChatHub>()));
            slack.Connection.Verify(c => c.Say(It.Is<BotMessage>(bm => bm.Text == Text)));
        }
    }
}