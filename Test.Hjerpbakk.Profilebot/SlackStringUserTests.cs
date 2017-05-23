using System;
using System.Collections.Generic;
using Hjerpbakk.ProfileBot;
using Xunit;

namespace Test.Hjerpbakk.Profilebot {
    public class SlackStringUserTests {
        public static IEnumerable<object[]> InvalidSlackIds = new[] {
            new object[] {"U1TBU8336"},
            new object[] {"<U1TBU8336"},
            new object[] {"<@U1TBU8336"}
        };

        [Fact]
        public void Contstructor() {
            var slackStringUser = new SlackStringUser("<@U1TBU8336>");

            Assert.Equal("<@U1TBU8336>", slackStringUser.SlackUserIdAsString);
            Assert.Equal("U1TBU8336", slackStringUser.UserId);
        }

        [Fact]
        public void Constructor_EmptyId_Fails() {
            var exception = Record.Exception(() => new SlackStringUser(null));

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Theory]
        [MemberData(nameof(InvalidSlackIds))]
        public void Constructor_WrongIdFormat_Fails(string badUserId) {
            var exception = Record.Exception(() => new SlackStringUser(badUserId));

            Assert.IsType<ArgumentException>(exception);
            Assert.Contains("<@SLACK_ID>", exception.Message);
        }

        [Fact]
        public void Equals() {
            var user1 = new SlackStringUser("<@U1TBU8336>");
            var user2 = new SlackStringUser("<@U1TBU8336>");
            var user3 = new SlackStringUser("<@U1TBU8337>");

            Assert.True(user1.Equals(user2));
            Assert.True(user2.Equals(user1));
            Assert.False(user1.Equals(user3));
            Assert.False(user2.Equals(user3));
            Assert.Equal(user1, user2);
        }
    }
}