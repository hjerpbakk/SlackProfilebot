using System;

namespace Hjerpbakk.ProfileBot.Commands {
    internal class UnknownCommand : ProfileBotCommand {
        static readonly Lazy<UnknownCommand> instance;

        static UnknownCommand() {
            instance = new Lazy<UnknownCommand>(() => new UnknownCommand());
        }

        UnknownCommand() { }

        public static UnknownCommand Create() =>
            instance.Value;
    }
}