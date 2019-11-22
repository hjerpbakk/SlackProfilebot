using SlackConnector.Models;

namespace Hjerpbakk.Profilebot.Commands {
    internal class NotifySingleProfileCommand : ProfileBotCommand<SlackUser> {
        public NotifySingleProfileCommand(SlackUser slackUser) : base(slackUser) { }
    }
}