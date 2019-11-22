using System;

namespace Hjerpbakk.Profilebot.Commands {
    internal class NotifyAllProfilesCommand : ProfileBotCommand {
        static readonly Lazy<NotifyAllProfilesCommand> instance;

        static NotifyAllProfilesCommand() {
            instance = new Lazy<NotifyAllProfilesCommand>(() => new NotifyAllProfilesCommand());
        }

        NotifyAllProfilesCommand() { }

        public static NotifyAllProfilesCommand Create() {
            return instance.Value;
        }
    }
}