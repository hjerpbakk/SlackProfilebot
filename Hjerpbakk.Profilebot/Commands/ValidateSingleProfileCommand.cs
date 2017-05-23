namespace Hjerpbakk.ProfileBot.Commands {
    internal class ValidateSingleProfileCommand : ProfileBotCommand<SlackStringUser> {
        public ValidateSingleProfileCommand(SlackStringUser slackStringUser) : base(slackStringUser) { }
    }
}