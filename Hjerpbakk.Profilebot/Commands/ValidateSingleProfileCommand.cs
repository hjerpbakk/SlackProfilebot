using SlackConnector.Models;

namespace Hjerpbakk.Profilebot.Commands {
    internal class ValidateSingleProfileCommand : ProfileBotCommand<SlackUser> {
        public ValidateSingleProfileCommand(SlackUser slackUser) : base(slackUser) { }
    }
}