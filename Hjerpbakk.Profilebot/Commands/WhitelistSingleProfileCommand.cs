using Hjerpbakk.Profilebot.Commands;
using SlackConnector.Models;

namespace Hjerpbakk.Profilebot.Commands {
    internal class WhitelistSingleProfileCommand : ProfileBotCommand<SlackUser> {
        public WhitelistSingleProfileCommand(SlackUser slackUser) : base(slackUser) { }
    }
}