using System;

namespace Hjerpbakk.Profilebot.Commands {
    internal class ProfileBotCommand {
        protected ProfileBotCommand() { }
    }

    internal class ProfileBotCommand<T> : ProfileBotCommand where T : class {
        public ProfileBotCommand(T payload) {
            Payload = payload ?? throw new ArgumentNullException();
        }

        public T Payload { get; }

        public override bool Equals(object obj) {
            var commandWithPayload = obj as ProfileBotCommand<T>;
            return commandWithPayload?.Payload.Equals(Payload) ?? ReferenceEquals(this, obj);
        }

        public override int GetHashCode() {
            return Payload.GetHashCode() ^ 3;
        }
    }
}