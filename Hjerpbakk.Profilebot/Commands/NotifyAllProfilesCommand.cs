using System;

namespace Hjerpbakk.ProfileBot.Commands {
    internal class NotifyAllProfilesCommand : ProfileBotCommand {
        static readonly Lazy<NotifyAllProfilesCommand> instance;

        static NotifyAllProfilesCommand() {
            instance = new Lazy<NotifyAllProfilesCommand>(() => new NotifyAllProfilesCommand());
        }

        NotifyAllProfilesCommand() { }

        public static NotifyAllProfilesCommand Create() =>
            instance.Value;
    }
}