using Hjerpbakk.ProfileBot.Commands;
using Xunit;

namespace Test.Hjerpbakk.Profilebot {
    public class ProfileBotCommandTests {
        [Fact]
        public void Constructor_CommandWithPayload() {
            const int Payload = 1;

            var command = new ProfileBotCommand<int>(Payload);

            Assert.Equal(Payload, command.Payload);
        }

        [Fact]
        public void Equals_TwoUnknown_Equal() {
            Assert.Equal(UnknownCommand.Create(), UnknownCommand.Create());
        }

        [Fact]
        public void GetHashCode_TwoUnknown_Equal() {
            Assert.Equal(UnknownCommand.Create().GetHashCode(), UnknownCommand.Create().GetHashCode());
        }

        [Fact]
        public void Equals_TwoDifferent_NotEqual() {
            // ReSharper disable once SuspiciousTypeConversion.Global
            Assert.False(UnknownCommand.Create().Equals(ValidateAllProfilesCommand.Create()));
        }

        [Fact]
        public void GetHashCode_TwoDifferent_NotEqual() {
            Assert.NotEqual(UnknownCommand.Create().GetHashCode(), ValidateAllProfilesCommand.Create().GetHashCode());
        }

        [Fact]
        public void Equals_OneUnknownOneNUll_NotEqual() {
            Assert.False(UnknownCommand.Create().Equals(null));
        }

        [Fact]
        public void Equals_SameCommandTypeDifferentPayload_NotEqual() {
            Assert.NotEqual(new ProfileBotCommand<int>(1), new ProfileBotCommand<int>(2));
        }

        [Fact]
        public void Equals_SameCommandTypeSamePayload_Equal() {
            Assert.Equal(new ProfileBotCommand<int>(1), new ProfileBotCommand<int>(1));
        }

        [Fact]
        public void GetHashCode_SameCommandTypeSamePayload_Equal() {
            Assert.Equal(new ProfileBotCommand<int>(1).GetHashCode(),
                new ProfileBotCommand<int>(1).GetHashCode());
        }
    }
}