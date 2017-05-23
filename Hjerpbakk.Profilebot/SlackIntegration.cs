using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hjerpbakk.ProfileBot.Contracts;
using SlackConnector;
using SlackConnector.EventHandlers;
using SlackConnector.Models;

namespace Hjerpbakk.ProfileBot {
    /// <summary>
    /// Wrapps the Slack APIs needed for Profilebot.
    /// </summary>
    public sealed class SlackIntegration : ISlackIntegration {
        readonly ISlackConnector connector;
        readonly string slackKey;

        ISlackConnection connection;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connector"></param>
        /// <param name="slackKey"></param>
        public SlackIntegration(ISlackConnector connector, string slackKey) {
            this.connector = connector ?? throw new ArgumentNullException(nameof(connector));

            if (string.IsNullOrEmpty(slackKey)) {
                throw new ArgumentException(nameof(slackKey));
            }

            this.slackKey = slackKey;
        }

        /// <summary>
        /// Raised everytime the bot gets a DM.
        /// </summary>
        public event MessageReceivedEventHandler MessageReceived {
            add => connection.OnMessageReceived += value;
            remove => connection.OnMessageReceived -= value;
        }

        /// <summary>
        /// Connects the bot to Slack.
        /// </summary>
        /// <returns>No object or value is returned by this method when it completes.</returns>
        public async Task Connect() =>
            connection = await connector.Connect(slackKey);

        /// <summary>
        /// 
        /// </summary>
        public void Dispose() =>
            connection?.Disconnect();

        /// <summary>
        /// Gets all the users in the Slack team.
        /// </summary>
        /// <returns>All users.</returns>
        public async Task<IEnumerable<SlackUser>> GetAllUsers() =>
            await connection.GetUsers();

        /// <summary>
        /// Gets the user with the given Id. 
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
        /// Sends a DM to the given user.
        /// </summary>
        /// <param name="userId">The recipient of the DM.</param>
        /// <param name="text">The message itself.</param>
        /// <returns>No object or value is returned by this method when it completes.</returns>
        public async Task SendDirectMessage(string userId, string text) {
            if (string.IsNullOrEmpty(userId)) {
                throw new ArgumentException(nameof(userId));
            }

            if (string.IsNullOrEmpty(text)) {
                throw new ArgumentException(nameof(text));
            }

            var channel = await connection.JoinDirectMessageChannel(userId);
            var message = new BotMessage {ChatHub = channel, Text = text};
            await connection.IndicateTyping(channel);
            await connection.Say(message);
        }
    }
}