namespace Hjerpbakk.ProfileBot.Commands {
    internal class ProfileBotCommand {
        protected ProfileBotCommand() { }
    }

    internal class ProfileBotCommand<T> : ProfileBotCommand where T : struct {
        public ProfileBotCommand(T payload) {
            Payload = payload;
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