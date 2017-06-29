using System;

namespace Hjerpbakk.Profilebot.Commands {
    internal class ValidateAllProfilesCommand : ProfileBotCommand {
        static readonly Lazy<ValidateAllProfilesCommand> instance;

        static ValidateAllProfilesCommand() {
            instance = new Lazy<ValidateAllProfilesCommand>(() => new ValidateAllProfilesCommand());
        }

        ValidateAllProfilesCommand() { }

        public static ValidateAllProfilesCommand Create() {
            return instance.Value;
        }
    }
}