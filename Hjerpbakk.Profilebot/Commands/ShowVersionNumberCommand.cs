using System;

namespace Hjerpbakk.Profilebot.Commands {
    internal class ShowVersionNumberCommand : ProfileBotCommand {
        static readonly Lazy<ShowVersionNumberCommand> instance;

        static ShowVersionNumberCommand() {
            instance = new Lazy<ShowVersionNumberCommand>(() => new ShowVersionNumberCommand());
        }

        ShowVersionNumberCommand() { }

        public static ShowVersionNumberCommand Create() {
            return instance.Value;
        }
    }
}