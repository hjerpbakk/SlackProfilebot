namespace Hjerpbakk.ProfileBot.Commands {
    internal class NotifySingleProfileCommand : ProfileBotCommand<SlackStringUser> {
        public NotifySingleProfileCommand(SlackStringUser slackStringUser) : base(slackStringUser) { }
    }
}