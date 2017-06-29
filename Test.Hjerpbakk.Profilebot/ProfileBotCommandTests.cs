using System;
using Hjerpbakk.Profilebot.Commands;
using Xunit;

namespace Test.Hjerpbakk.Profilebot {
    public class ProfileBotCommandTests {
        [Fact]
        public void Constructor_CommandWithPayload() {
            var payload = new object();

            var command = new ProfileBotCommand<object>(payload);

            Assert.Same(payload, command.Payload);
        }

        [Fact]
        public void CTOR_PayloudNull_Fails() {
            var exception = Record.Exception(() => new ProfileBotCommand<object>(null));

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void Equals_DifferentTypes_NotEqual() {
            // ReSharper disable once SuspiciousTypeConversion.Global
            Assert.False(new ProfileBotCommand<string>("1").Equals("1"));
        }

        [Fact]
        public void Equals_OneUnknownOneNUll_NotEqual() {
            Assert.False(UnknownCommand.Create().Equals(null));
        }

        [Fact]
        public void Equals_SameCommandTypeDifferentPayload_NotEqual() {
            Assert.NotEqual(new ProfileBotCommand<string>("1"), new ProfileBotCommand<string>("2"));
        }

        [Fact]
        public void Equals_SameCommandTypeSamePayload_Equal() {
            Assert.Equal(new ProfileBotCommand<string>("1"), new ProfileBotCommand<string>("1"));
        }

        [Fact]
        public void Equals_TwoDifferent_NotEqual() {
            // ReSharper disable once SuspiciousTypeConversion.Global
            Assert.False(UnknownCommand.Create().Equals(ValidateAllProfilesCommand.Create()));
        }

        [Fact]
        public void Equals_TwoUnknown_Equal() {
            Assert.Equal(UnknownCommand.Create(), UnknownCommand.Create());
        }

        [Fact]
        public void GetHashCode_SameCommandTypeSamePayload_Equal() {
            Assert.Equal(new ProfileBotCommand<string>("1").GetHashCode(),
                new ProfileBotCommand<string>("1").GetHashCode());
        }

        [Fact]
        public void GetHashCode_TwoDifferent_NotEqual() {
            Assert.NotEqual(UnknownCommand.Create().GetHashCode(), ValidateAllProfilesCommand.Create().GetHashCode());
        }

        [Fact]
        public void GetHashCode_TwoUnknown_Equal() {
            Assert.Equal(UnknownCommand.Create().GetHashCode(), UnknownCommand.Create().GetHashCode());
        }
    }
}