using System;

namespace Hjerpbakk.Profilebot.Commands {
    internal class ShowWhitelistedUsersCommand : ProfileBotCommand {
        static readonly Lazy<ShowWhitelistedUsersCommand> instance;

        static ShowWhitelistedUsersCommand() {
            instance = new Lazy<ShowWhitelistedUsersCommand>(() => new ShowWhitelistedUsersCommand());
        }

        ShowWhitelistedUsersCommand() { }

        public static ShowWhitelistedUsersCommand Create() {
            return instance.Value;
        }
    }
}