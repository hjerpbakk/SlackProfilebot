using System;

namespace Hjerpbakk.Profilebot.Commands {
    internal class AnswerRegularUserCommand : ProfileBotCommand {
        static readonly Lazy<AnswerRegularUserCommand> instance;

        static AnswerRegularUserCommand() {
            instance = new Lazy<AnswerRegularUserCommand>(() => new AnswerRegularUserCommand());
        }

        AnswerRegularUserCommand() { }

        public static AnswerRegularUserCommand Create() {
            return instance.Value;
        }
    }
}