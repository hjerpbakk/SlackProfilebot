using System;

namespace Hjerpbakk.Profilebot.Commands {
    internal class UnknownCommand : ProfileBotCommand {
        static readonly Lazy<UnknownCommand> instance;

        static UnknownCommand() {
            instance = new Lazy<UnknownCommand>(() => new UnknownCommand());
        }

        UnknownCommand() { }

        public static UnknownCommand Create() {
            return instance.Value;
        }
    }
}