using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SlackConnector.EventHandlers;
using SlackConnector.Models;

namespace Hjerpbakk.Profilebot {
    /// <summary>
    ///     Interface for the Slack APIs needed for Profilebot.
    /// </summary>
    public interface ISlackIntegration : IDisposable {
        /// <summary>
        ///     Raised everytime the bot gets a DM.
        /// </summary>
        event MessageReceivedEventHandler MessageReceived;

        /// <summary>
        ///     Connects the bot to Slack.
        /// </summary>
        /// <returns>No object or value is returned by this method when it completes.</returns>
        Task Connect();

        /// <summary>
        ///     Gets all the users in the Slack team.
        /// </summary>
        /// <returns>All users.</returns>
        Task<IEnumerable<SlackUser>> GetAllUsers();

        /// <summary>
        ///     Gets the user with the given Id.
        /// </summary>
        /// <param name="userId">The id of the user to be found.</param>
        /// <returns>The wanted user or null if not found.</returns>
        Task<SlackUser> GetUser(string userId);

        /// <summary>
        ///     Sends a DM to the given user.
        /// </summary>
        /// <param name="user">The recipient of the DM.</param>
        /// <param name="text">The message itself.</param>
        /// <returns>No object or value is returned by this method when it completes.</returns>
        Task SendDirectMessage(SlackUser user, string text);

        /// <summary>
        ///     Indicates that the bot is typing for a given user.
        /// </summary>
        /// <param name="user">The user who will see the bot typing.</param>
        /// <returns>No object or value is returned by this method when it completes.</returns>
        Task IndicateTyping(SlackUser user);
    }
}