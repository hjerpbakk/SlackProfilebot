using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hjerpbakk.Profilebot;
using Hjerpbakk.Profilebot.Contracts;
using SlackConnector;
using SlackConnector.EventHandlers;
using SlackConnector.Models;

namespace Hjerpbakk.Profilebot {
    /// <summary>
    ///     Wrapps the Slack APIs needed for Profilebot.
    /// </summary>
    public sealed class SlackIntegration : ISlackIntegration {
        readonly ISlackConnector connector;
        readonly string slackKey;

        ISlackConnection connection;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="connector">The Slack connector to use.</param>
        /// <param name="slackKey">The Slack key to use.</param>
        public SlackIntegration(ISlackConnector connector, string slackKey) {
            this.connector = connector ?? throw new ArgumentNullException(nameof(connector));

            if (string.IsNullOrEmpty(slackKey)) {
                throw new ArgumentException(nameof(slackKey));
            }

            this.slackKey = slackKey;
        }

        /// <summary>
        ///     Raised everytime the bot gets a DM.
        /// </summary>
        public event MessageReceivedEventHandler MessageReceived {
            add => connection.OnMessageReceived += value;
            remove => connection.OnMessageReceived -= value;
        }

        /// <summary>
        ///     Connects the bot to Slack.
        /// </summary>
        /// <returns>No object or value is returned by this method when it completes.</returns>
        public async Task Connect() {
            connection = await connector.Connect(slackKey);
        }

        public void Dispose() {
            connection?.Disconnect();
        }

        /// <summary>
        ///     Gets all the users in the Slack team.
        /// </summary>
        /// <returns>All users.</returns>
        public async Task<IEnumerable<SlackUser>> GetAllUsers() {
            return await connection.GetUsers();
        }

        /// <summary>
        ///     Gets the user with the given Id.
        /// </summary>
        /// <param name="userId">The id of the user to be found.</param>
        /// <returns>The wanted user or null if not found.</returns>
        public async Task<SlackUser> GetUser(string userId) {
            if (string.IsNullOrEmpty(userId)) {
                throw new ArgumentException(nameof(userId));
            }

            return connection.UserCache.ContainsKey(userId)
                ? connection.UserCache[userId]
                : (await GetAllUsers()).SingleOrDefault(u => u.Id == userId);
        }

        /// <summary>
        ///     Sends a DM to the given user.
        /// </summary>
        /// <param name="user">The recipient of the DM.</param>
        /// <param name="text">The message itself.</param>
        /// <returns>No object or value is returned by this method when it completes.</returns>
        public async Task SendDirectMessage(SlackUser user, string text) {
            user.Guard();
            if (string.IsNullOrEmpty(text)) {
                throw new ArgumentException(nameof(text));
            }

            var channel = await connection.JoinDirectMessageChannel(user.Id);
            await connection.IndicateTyping(channel);
            var message = new BotMessage {ChatHub = channel, Text = text};
            await connection.Say(message);
        }

        /// <summary>
        ///     Indicates that the bot is typing for a given user.
        /// </summary>
        /// <param name="user">The user who will see the bot typing.</param>
        /// <returns>No object or value is returned by this method when it completes.</returns>
        public async Task IndicateTyping(SlackUser user) {
            user.Guard();
            await connection.IndicateTyping(await connection.JoinDirectMessageChannel(user.Id));
        }
    }
}